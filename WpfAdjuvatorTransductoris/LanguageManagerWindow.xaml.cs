using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;

namespace WpfAdjuvatorTransductoris;

// TODO: Check if language changes saving properly
public partial class LanguageManagerWindow
{
    private DataModel _data;

    private ObservableCollection<string> _langsNamePerComboBox = new();
    internal class LangPair
    {
        public string FullName { get; set; } = string.Empty;
        public string LangCode { get; set; } = string.Empty;
    } 
    
    private ObservableCollection<LangPair> LanguageNamePairsCollection { get; set; } = new();
    
    public LanguageManagerWindow(DataModel data)
    {
        InitializeComponent();
        _data = data;
        SetLanguagePairsCollection();  
        LanguagesListView.ItemsSource = LanguageNamePairsCollection;
        SetLangComboBoxInitialState();
        LangsComboBox.ItemsSource = _langsNamePerComboBox;
    }

    private void SetLanguagePairsCollection()
    {
        LanguageNamePairsCollection.Clear();
        _data.Languages.ForEach(lang => 
            LanguageNamePairsCollection.Add(new LangPair
            {
                LangCode = lang,
                FullName = LanguagesCode.FullLanguageNames[lang]
            }));
    }

    private void SetLangComboBoxInitialState()
    {
        _langsNamePerComboBox.Clear();
        foreach (var name in LanguagesCode
                     .FullLanguageNames.Values
                     .Except(LanguageNamePairsCollection.Select(pair => pair.FullName))
                )
        {
            _langsNamePerComboBox.Add(name);
        }
    }

    private void FilterAvailableLangs(string filter)
    {
        LanguagesCode.FullLanguageNames //smart filter for langs
            .Where(keyValuePair => keyValuePair.Key.Contains(filter)) //where lang contains entered in TextBox letters
            .Select(keyValuePair => keyValuePair.Value) //selecting val from dictionary
            .Except(LanguageNamePairsCollection.Select(pair => pair.FullName)).ToList() //except those are exist in proj
            .ForEach(fullName => _langsNamePerComboBox.Add(fullName));//adding selected in ComboBox 
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        _langsNamePerComboBox.Clear();
        FilterAvailableLangs(textBox.Text);
    }

    private void LangsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox || e.AddedItems.Count == 0) return;
        var s = e.AddedItems[0]?.ToString();
        if (s is { } code)
            LangCodeTextBox.Text = LanguagesCode.ShortLanguageCodes[code]; 
    }

    private void LanguagesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView langListBox) return;
        var selectedItem = langListBox.SelectedItem as LangPair;
        if (selectedItem is not { } langPairItem) return;
        LangCodeTextBox.Text = langPairItem.LangCode;
    }

    private static bool IsNewLangFieldsFilled(LanguageManagerWindow manager) => manager.LangCodeTextBox.Text.Length == 2 && 
                                                                                (string.IsNullOrWhiteSpace(manager.LangsComboBox.Text) ||
                                                                                 manager.LangsComboBox.Items.Count == 1); 
    
    //can execute if code is entered (2 letters) and either ComboBox contains lang as text or contains only 1 item
    //making select an useless action
    private void AddLanguage_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is not LanguageManagerWindow manager) return;
        e.CanExecute = IsNewLangFieldsFilled(manager) && LanguagesListView.Items.Cast<LangPair>()
            .All(pair => pair.LangCode != manager.LangCodeTextBox.Text);
    }

    private void AddLanguage_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not LanguageManagerWindow manager) return;
        manager._data.Redactor.AddLanguage(manager.LangCodeTextBox.Text);
        SetLanguagePairsCollection();
    }

    private void RenameLanguage_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is not  LanguageManagerWindow manager) return;
        e.CanExecute = IsNewLangFieldsFilled(manager)
                       && manager.LanguagesListView.SelectedIndex >= 0
                       && ((LangPair)manager.LanguagesListView.SelectedItem).LangCode != manager.LangCodeTextBox.Text;
    }

    private void RenameLanguage_Executed(object sender, ExecutedRoutedEventArgs _)
    {
        if (sender is not  LanguageManagerWindow manager) return;
        var oldName = ((LangPair)manager.LanguagesListView.SelectedItem).LangCode; //can not be executed with null Sel..Item
        var newName = manager.LangCodeTextBox.Text;
        _data.Redactor.RenameLanguage(oldName, newName);
        // DataModelXmlWriter.CommitAddRemoveRenameLanguage(oldName, newLangName: newName);
        // DataModelXmlWriter.SaveData();
        SetLanguagePairsCollection();
    }

    private void DeleteLanguage_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is not LanguageManagerWindow manager) return;
        e.CanExecute = manager.LanguagesListView.SelectedIndex >= 0;
    }

    private void DeleteLanguage_Executed(object sender, ExecutedRoutedEventArgs _)
    {
        if (sender is not LanguageManagerWindow manager) return;
        var userConfirmation = MessageBox.Show(
            $"Are you sure you want to delete {((LangPair)manager.LanguagesListView.SelectedItem).FullName} language?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (userConfirmation != MessageBoxResult.Yes) return;
        string langCode = ((LangPair)manager.LanguagesListView.SelectedItem).LangCode;
        manager._data.Redactor.RemoveLanguage(langCode);
        // DataModelXmlWriter.CommitAddRemoveRenameLanguage(langCode, true);
        // DataModelXmlWriter.SaveData();
        SetLanguagePairsCollection();
    }
}