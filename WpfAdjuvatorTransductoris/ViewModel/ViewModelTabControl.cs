using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using AdjuvatorTransductorumRCor.Model;
using WpfAdjuvatorTransductoris.Helpers;
using WpfAdjuvatorTransductoris.Providers;
using System.Linq;

namespace WpfAdjuvatorTransductoris.ViewModel;

public class ViewModelTabControl : IDataDependable
{
    private DataModel? _data;
    private readonly Stack<string> _recentlyClosed = new();

    public DataModel? Data
    {
        get => _data;
        set
        {
            _data = value;
            Tabs.Clear();
        }
    }

    public readonly ObservableCollection<TabItem> Tabs = new();

    private TabItem TabAssemble(ViewModelTab tabView)
    {
        DataGrid data = new DataGrid();

        BindingOperations.SetBinding(data, ItemsControl.ItemsSourceProperty, new Binding
        {
            Source = tabView,
            Path = new PropertyPath("Table"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        var asterisk = new TextBlock
        {
            Text = tabView.HasChanges ? "*" : string.Empty,
            FontSize = 10,
            Margin = new Thickness(2, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Top
        };

        BindingOperations.SetBinding(asterisk, TextBlock.TextProperty, new Binding
        {
            Source = tabView,
            Path = new PropertyPath("HasChanges"),
            Mode = BindingMode.OneWay,
            Converter = new ViewModelTab.HasChangesToAsteriskConverter()
        });

        var closeButton = new Button
        {
            Name = Regexes.NameCompressorRestriction.Replace(tabView.Address, "_"),
            BorderThickness = new Thickness(0),
            Margin = new Thickness(4, 2, 4, 2),
            Height = 15,
            Width = 15,
            Content = new TextBlock
            {
                Text = "x",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0)
            }
        };

        closeButton.Click += (sender, _) => Close(Tabs.First(tab => tab.Name == ((Button)sender).Name));

        return new TabItem
        {
            DataContext = tabView,
            Name = Regexes.NameCompressorRestriction.Replace(tabView.Address, "_"),
            Content = data,
            IsSelected = true,
            Header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = tabView.Name,
                        Height = 15,
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    
                    asterisk,
                    closeButton
                }
            } 
        };
    }

    public void CreateTab(Queue<string> address)
    {
        if (Data is null) return;
        if (Data.Root?.GetNode(address) is not { NodeType: NodeTypes.File } file) return;
        ViewModelTab tabView = new ViewModelTab((DataModelNode)file, Data.Languages);

        Data.LanguagesChanged += (sender, _) =>
        {
            if (sender is DataModel data)
                tabView.SetLanguages(data.Languages);
        };
        
        Tabs.Add(TabAssemble(tabView));
    }

    public void SaveTab(ViewModelTab tab)
    {
        if (Data is null) return;
        tab.Save(Data.Redactor);
        Data.Redactor.CommitChanges();
    }

    public void SaveMultipleTabs(ViewModelTab?[] tab)
    {
        if (Data is null) return;
        foreach (var item in tab)
            item?.Save(Data.Redactor);
        Data.Redactor.CommitChanges();
    }

    internal TabItem? GetTab(string fullAddress)
        => Tabs.FirstOrDefault(tab => ((ViewModelTab)tab.DataContext).Address == fullAddress);

    internal TabItem? GetTab(string address, string name)
        => GetTab(DataAddress.SmartCompress(address, name));

    internal TabItem? GetTab(Queue<string> address, string name)
    {
        address.Enqueue(name);
        return GetTab(DataAddress.Compress(address));
    }

    /// <summary>
    /// Searches within active tabs those with matching address including nesting to those address.
    /// </summary>
    /// <param name="address">Address</param>
    /// <returns>Collection of tabs with matching address.</returns>
    internal IEnumerable<TabItem> GetTabsByAddress(string address)
        => Tabs.Where(tab =>
        {
            var viewModelContext = (ViewModelTab)tab.DataContext;
            return DataAddress.Contains(address, viewModelContext.Address);
        });
    
    internal void ForcedClose(string tabCompressedAddress)
    {
        var active = GetTabsByAddress(tabCompressedAddress);
        foreach (var activeTab in active) 
            Tabs.Remove(activeTab);
    }

    private void Close(TabItem tab)
    {
        var vm = (ViewModelTab)tab.DataContext;
        if (vm.HasChanges) {
            var res = MessageBox.Show("Are you sure?", $"Close {vm.Name}", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                _recentlyClosed.Push(vm.Address);
                Tabs.Remove(tab);
            }
            else return;
        }

        _recentlyClosed.Push(vm.Address);
        Tabs.Remove(tab);
    }

    internal void RenameNestedTabs(Queue<string> parentAddress, Queue<string> newAddress)
    {
        var activeChildTabs = GetTabsByAddress(DataAddress.Compress(parentAddress));
        foreach (var tabContext in activeChildTabs.Select(tab => (ViewModelTab)tab.DataContext))
        {
            tabContext.Address = DataAddress.Compress(
                DataAddress.ReplaceAddress(DataAddress.Split(tabContext.Address), newAddress));
        }
    }
}