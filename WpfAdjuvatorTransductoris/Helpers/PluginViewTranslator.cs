using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.ViewDescriber;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;

namespace WpfAdjuvatorTransductoris.Helpers;

/// <summary>
/// This class performs translation from ViewDescriber to WPF making plugin window description independent.
/// </summary>
public static class PluginViewTranslator
{
    private static PluginDataCarrierWindow? _window;
    
    // ReSharper disable once CollectionNeverQueried.Local
    private static readonly List<RoutedCommand> Commands = new();
    private static Thickness SetThickness(ViewAttrBase form) =>
        new(form.MarginLeft, form.MarginTop, form.MarginRight, form.MarginBottom);

    private static StackPanel CreateWindowContent(ViewDefinition viewDefinition)
    {
        StackPanel mainPanel = new()
        {
            Orientation = Orientation.Vertical
        };
        
        foreach (var form in viewDefinition.Forms)
        {
            mainPanel.Children.Add(InitForm(form));
        }

        return mainPanel;
    }
    
    public static PluginDataCarrierWindow GetWindow(ViewDefinition viewDefinition, DataModel? dataModel = null)
    {
        _window = new PluginDataCarrierWindow(viewDefinition, dataModel);
        SetElementParams(_window, viewDefinition);
        _window.Title = viewDefinition.Name;
        _window.Content = CreateWindowContent(viewDefinition);
        _window.CorrectSize();
        return _window;
    }

    private static void CorrectSize(this PluginDataCarrierWindow window)
    {
        window.Height += SystemParameters.WindowCaptionHeight + SystemParameters.BorderWidth * 2;
        window.Width += SystemParameters.BorderWidth * 2;
    }
    
    private static FrameworkElement CreateElement(ViewForm form)
        => (form.ViewType) switch
        {
            ViewTypes.Button => new Button(),
            ViewTypes.Label => new TextBlock(),
            ViewTypes.TextInput => new TextBox(),
            ViewTypes.FolderSelection => InitFolderSelection(form),
            _ => throw new ArgumentOutOfRangeException()
        };

    private static void BindContent(FrameworkElement elem, ViewForm source)
    {
        if (source.Content == null) return;
        switch (elem)
        {
            case StackPanel:
                return;
            case not (TextBlock or TextBox):
                Bind(elem, ContentControl.ContentProperty, source.Content, new PropertyPath(nameof(source.Content.Property)));
                break;
            case TextBlock:
                Bind(elem, TextBlock.TextProperty, source.Content, new PropertyPath(nameof(source.Content.Property)));
                break;
            case TextBox:
                Bind(elem, TextBox.TextProperty, source.Content, new PropertyPath(nameof(source.Content.Property)));
                break;
        }
    }
    private static FrameworkElement InitForm(ViewForm form)
    {
        FrameworkElement elem = CreateElement(form);
        SetElementParams(elem, form);
        BindContent(elem, form);
        Bind(elem, UIElement.IsEnabledProperty, form, new PropertyPath(nameof(form.IsEnabled)));
        Bind(elem, UIElement.VisibilityProperty, form, new PropertyPath(nameof(form.IsVisible)), new PluginViewBoolToVisibleConverter());
        SetEventRelays(elem);
        return elem;
    }

    private static void SetEventRelays(FrameworkElement elem)
    {
        switch (elem)
        {
            case TextBox textBox: 
                if (textBox.DataContext is not ViewForm { IsContentChangedRegistered: true }) return;
                textBox.TextChanged += (sender, _) =>
                {
                    if (((TextBox)sender).DataContext is not ViewForm context) return;
                    context.OnContentChanged(context);
                };
                
                break;
            case Button button:
                if (button.DataContext is ViewForm { IsClickRegistered: true })
                {
                    button.Click += (sender, _) =>
                    {
                        if (((Button)sender).DataContext is not ViewForm form) return;
                        form.OnClick(form);
                    };
                }
                
                if (button.DataContext is ViewForm { IsExecuteRegistered: true })
                    RegisterNewCommand(button);
                break;
        }
    }
    
    private static CommandBinding SetCommandBinding(ViewForm context, RoutedCommand command)
    {
        CommandBinding binding = new CommandBinding(command, (sender, args) =>
        {
            if (((FrameworkElement)sender).DataContext is not ViewDefinition dataContext ||
                ((Button)args.OriginalSource).DataContext is not ViewForm sourceContext) return;
            ViewEventArgsDataAction e = new();
            sourceContext.OnExecute(dataContext, e);
            if (e.DataExtracted != null)
            {
                if (_window != null) _window.Data = e.DataExtracted;
                dataContext.OnExtractionFinished();
            }

            if (e.Injected) dataContext.OnInjectionFinished();
        });

        if (context.IsCanExecuteRegistered)
        {
            binding.CanExecute += (sender, args) =>
            {
                if (((FrameworkElement)sender).DataContext is not ViewDefinition dataContext ||
                    ((Button)args.OriginalSource).DataContext is not ViewForm sourceContext) return;
                ViewEventArgsCanExecute e = new();
                sourceContext.OnCanExecute(dataContext, e);
                args.CanExecute = e.CanExecute;
            };
        }

        return binding;
    }

    private static RoutedCommand CreateNewCommand(Button button)
    {
        RoutedCommand command = new();
        Commands.Add(command);
        button.Command = command;
        return command;
    }
    
    private static void RegisterNewCommand(Button element)
    {
        if (element.DataContext is not ViewForm context) return;
        CreateNewCommand(element);
        var command = CreateNewCommand(element);
        _window?.CommandBindings.Add(SetCommandBinding(context, command));
    }
    
    private static HorizontalAlignment ConvertHorizontalAlignmentFromFormToWpf(ViewAttrBase form)
        =>  form.HorizontalAlign switch
            {
                ViewContentHorizontalAlign.Center => HorizontalAlignment.Center,
                ViewContentHorizontalAlign.Left => HorizontalAlignment.Left,
                ViewContentHorizontalAlign.Right => HorizontalAlignment.Right,
                ViewContentHorizontalAlign.Stretch => HorizontalAlignment.Stretch,
                _ => HorizontalAlignment.Left
            };
    
    private static VerticalAlignment ConvertVerticalAlignmentFromFormToWpf(ViewAttrBase form)
        =>  form.VerticalAlign switch
            {
                ViewContentVerticalAlign.Bottom => VerticalAlignment.Bottom,
                ViewContentVerticalAlign.Center => VerticalAlignment.Center,
                ViewContentVerticalAlign.Top => VerticalAlignment.Top,
                ViewContentVerticalAlign.Stretch => VerticalAlignment.Stretch,
                _ => VerticalAlignment.Top
            };
    
    private static void SetElementParams(FrameworkElement elem, ViewAttrBase form)
    {
        elem.DataContext = form;
        elem.Width = form.Width;
        elem.HorizontalAlignment = ConvertHorizontalAlignmentFromFormToWpf(form);
        elem.VerticalAlignment = ConvertVerticalAlignmentFromFormToWpf(form);
        elem.Height = form.Height;
        elem.Margin = SetThickness(form);
    }

    private static void Bind(DependencyObject element, DependencyProperty dp, object form, PropertyPath propertyPath, IValueConverter? converter = null)
    {
        var bind = new Binding
        {
            Source = form,
            Path = propertyPath,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        if (converter != null)
            bind.Converter = converter;
        BindingOperations.SetBinding(element, dp, bind);
    }
    
    private static StackPanel InitFolderSelection(ViewForm formDescription)
    {
        var text = formDescription.Content?.ToString();
        Button browseButton = new Button()
        {
            IsEnabled = true,
            Content = "Browse",
            Width = formDescription.Width / 5.0 - 12,
            Height = formDescription.Height - 4,
            Margin = new Thickness(4,2,2,2)
        };

        
        TextBox pathBox = new TextBox()
        {
            DataContext = formDescription,
            IsEnabled = false,
            Text = text,
            Width = formDescription.Width / 5 * 4 - 12,
            MinWidth = 100,
            Height = formDescription.Height - 4,
            Margin = new Thickness(2,2,4,2)
        };

        formDescription.Content ??= new ViewProp(string.Empty);
        BindContent(pathBox, formDescription);
        browseButton.Click += (_, _) =>
        {
            var folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                pathBox.Text = folderBrowser.SelectedPath;
            }
        };
        
        StackPanel panel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                pathBox,
                browseButton
            }
        };

        return panel;
    }
}