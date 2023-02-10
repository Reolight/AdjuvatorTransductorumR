using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AdjuvatorTransductorumRCor.ViewDescriber;

/// <summary>
/// Defines basic attributes for all View Descriptors
/// </summary>
public partial class ViewAttrBase : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _width;
    private int _height;
    private int _margLeft;
    private int _margRight;
    private int _margTop;
    private int _margBottom;
    
    private bool _isEnabled = true;
    private bool _isVisible = true;

    private static Regex nameValidator = new Regex("[^a-zA-Z0-9]");
    /// <summary>
    /// Form name. Use unic names or you'll encounter with searching by name problem
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (nameValidator.IsMatch(value))
                throw new ArgumentException($"Exceptopn in plugin window description: [{value}] is not appropriate name");
            _name = value;
            OnPropertyChanged();
        }
    }

    public ViewContentHorizontalAlign HorizontalAlign { get; set; }
    public ViewContentVerticalAlign VerticalAlign { get; set; }
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    public int MarginLeft
    {
        get => _margLeft;
        set => _margLeft = value >= 0 ? value : 0;
    }

    public int MarginRight
    {
        get => _margRight;
        set => _margRight = value >= 0 ? value : 0;
    }

    public int MarginTop
    {
        get => _margTop;
        set => _margTop = value >= 0 ? value : 0;
    }

    public int MarginBottom
    {
        get => _margBottom;
        set => _margBottom = value >= 0 ? value : 0;
    }

    public int Margin
    {
        set
        {
            MarginLeft = value;
            MarginRight = value;
            MarginBottom = value;
            MarginTop = value;
        }
    }

    public int Width
    {
        get => _width;
        set => _width = value >= 0 ? value : 0;
    }
    
    public int Height 
    { 
        get => _height;
        set => _height = value >= 0 ? value : 0;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}