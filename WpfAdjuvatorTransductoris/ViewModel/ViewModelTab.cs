using System;
using AdjuvatorTransductorumRCor.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Data;
using WpfAdjuvatorTransductoris.Annotations;
using WpfAdjuvatorTransductoris.Helpers;

namespace WpfAdjuvatorTransductoris.ViewModel;

public sealed class ViewModelTab : INotifyPropertyChanged
{
    #region DATA
    private static readonly DataTable DummyTable = new(); 
    public readonly string Name;
    public string Address;
    private List<string> _languages;
    private DataModelNode _file;
    private bool _hasChanges;
    private DataRowCollection _rows;
    private DataTable _table;
    public DataTable Table
    {
        get => _table;
        set
        {
            _table = value;
            OnPropertyChanged(nameof(Table));
        }
    }

    #endregion

    #region CONSTRUCTOR

    public ViewModelTab(DataModelNode file, List<string> languages)
    {
        _languages = languages;
        _file = file;
        
        _table = new DataTable();
        var keyColumn = Table.Columns.Add("Key", typeof(string));
        keyColumn.DefaultValue = string.Empty;
        _table.RowChanged += (_, _) => HasChanges = true;
        _table.RowDeleted += (_, _) => HasChanges = true;
        _rows = Table.Rows;

        Address = DataAddress.Compress(file.GetAddress());
        var addressBasedName = Regexes.NameCompressorRestriction.Replace(Address, "_");
        Name = addressBasedName;
        
        UpdateTable();
    }
    
    #endregion

    #region RENDER

    private void UpdateColumns()
    {
        // Wipes rows with old schema
        _rows.Clear();
        
        // Because columns count doesn't update upon changing even if OnPropertyChanged or UpdateSource are triggered...
        // Here is seems to be a single way, how to update columns count:
        // Triggering OnPropertyChanged by changing reference
        var tempTable = Table;
        Table = DummyTable;
        
        // Calculation of differences between languages columns and languages variable
        var columnNames = tempTable.Columns.Cast<DataColumn>()
            .Select(col => col.ColumnName)
            .Where(name => name != "Key").ToList();
        var addedLanguages = _languages.Except(columnNames);
        var removedLanguages = columnNames.Except(_languages);

        // Those 2 cycles apply changes to temp table (which should be updated)
        foreach (string language in addedLanguages)
        {
            var addedColumn = tempTable.Columns.Add(language);
            addedColumn.DefaultValue = string.Empty;
        }
        
        foreach (string language in removedLanguages)
        {
            tempTable.Columns.Remove(language);
        }

        // Changing reference back. So, it works finally.
        Table = tempTable;
    }

    // This method is called upon first render & language count update
    private void UpdateTable()
    {
        UpdateColumns();
        UpdateRows();
        AcceptChanges();
    }
    
    private void UpdateRows()
    {
        _file.GetNodes().ToList().ForEach(node =>
        {
            if (node is not DataModelLeaf value || _languages.Count == 0)
                return;
            var row = Table.NewRow();
            row["Key"] = node.Name;
            _languages.ForEach(lang => row[lang] = value.GetValue(lang));
            _rows.Add(row);
        });
    }

    public void SetLanguages(List<string> languages)
    {
        _languages = languages;
        UpdateTable();
    }
    
    #endregion
    
    #region CHANGES_TRACKER

    private void RemoveRedundantNodes(DataBuilder builder)
        => _file.GetNodes()
            .Select(node => node.Name)
            .Except(_rows
                .OfType<DataRow>()
                .Where(row => row.RowState != DataRowState.Deleted)
                .Select(row => (string)row["Key"])
            ).ToList()
            .ForEach(red =>
            {
#if DEBUG
                Console.WriteLine($@"removing {red}");
#endif
                DataModelXmlWriter.CommitRemoveNode($"{Address}:{red}");
                builder.RemoveValue(red);
            });

    private void ApplyChangesToData(DataBuilder builder)
        => _rows.OfType<DataRow>()
            .Where(row => row.RowState != DataRowState.Unchanged && row.RowState != DataRowState.Deleted)
            .Select(row => row.ItemArray.Cast<string>().ToArray()) //cast problems avoided by string.Empty
            .Where(row => !string.IsNullOrWhiteSpace(row[0]))
            .ToList()
            .ForEach(row =>
            {
                var i = 1;
                if (_languages.Count == 0) throw new Exception("Languages here is null");
                foreach (var lang in _languages)
                {
#if DEBUG
                    Console.WriteLine($@"Adding to {row[0]}:{lang} - {row[i]}");
#endif
                    DataModelXmlWriter.CommitNewValue($"{Address}:{row[0]}", lang, row[i]); //there is no 
                    builder.AddValue(row[0], lang, row[i++]); //here is active node
                }
            });

    private void AcceptChanges()
    {
        Table.AcceptChanges();
        HasChanges = false;
    }
    
    internal void Save(DataBuilder builder)
    {
        builder.Focus(Address);
        RemoveRedundantNodes(builder);
        ApplyChangesToData(builder);
        AcceptChanges();
    }

    #endregion
    
    #region HAS_CHANGES
    
    // Should asterisk be drawn
    public bool HasChanges
    {
        get => _hasChanges;
        private set
        {
            _hasChanges = value;
            OnPropertyChanged();
        }
    }
    
    public class HasChangesToAsteriskConverter : IValueConverter
    {
        // No converter back (unnecessary)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "*" : string.Empty;
        }
    }
    
    #endregion

    #region ON_PROPERTY_CHANGED

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    #endregion
}
