using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AdjuvatorTransductorumRCor.ViewDescriber;

public class ViewProp : INotifyPropertyChanged
{
    private object? _property;

    public object? Property
    {
        get => _property;
        set
        {
            if (value != null && (Validate is null || Validate.Invoke(value)))
                _property = value;
        }
    }

    /// This function is invoked upon assigning new value to the Property
    public Func<object, bool>? Validate;
    
    /// <summary>
    /// Casts Property to a string if Property is a string. Otherwise returns base.ToString() or "empty property".
    /// </summary> 
    public override string? ToString()
    {
        if (Property is string stringProp)
            return stringProp;
        return base.ToString();
    }

    public ViewProp(object value)
    {
        _property = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}