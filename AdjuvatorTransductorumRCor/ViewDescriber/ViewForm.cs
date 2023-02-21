namespace AdjuvatorTransductorumRCor.ViewDescriber;

public class ViewForm : ViewAttrBase
{
    private static int _count = 0;
    public readonly ViewTypes ViewType;

    // Events are descending, which means that front-events are invoked first and than 
    // they invokes these events (NB: object here is ViewForm!)
    // Is[EVENT]Registered is used for bypass creation of redundant event handlers
    public event Action<object, EventArgs>? Click;
    public bool IsClickRegistered => Click != null;
    
    public event Action<object, EventArgs>? ContentChanged;
    public bool IsContentChangedRegistered => ContentChanged != null;
    
    public event Action<object, ViewEventArgsDataAction>? Execute;
    public bool IsExecuteRegistered => Execute != null;
    
    public event Action<object, ViewEventArgsCanExecute>? CanExecute;
    public bool IsCanExecuteRegistered => CanExecute != null;

    // Makes events accessible from initializer syntax
    public Action<object, EventArgs> ClickEvent
    {
        set => Click += value;
    }
    
    public Action<object, EventArgs> ContentChangedEvent
    {
        set => ContentChanged += value;
    }
    
    public Action<object, ViewEventArgsDataAction> ExecuteEvent
    {
        set => Execute += value;
    }
    public Action<object, ViewEventArgsCanExecute> CanExecuteEvent
    {
        set => CanExecute += value;
    }
    
    public ViewProperty? Content;
    
    // Parent window. Will be initialized upon adding to the window.
    private ViewDefinition _parent = null!;
    public ViewDefinition Parent { 
        get => _parent ?? throw new NullReferenceException
            ($"Parent window is not set for form {Name}");
        internal set => _parent = value;
    } 
    
    public int Id { get; } = _count++;
    public ViewForm(ViewTypes viewType)
    {
        ViewType = viewType;
        Content = null;
        Margin = 4;
        Width = 125;
        Height = 25;
        Name = $"{ViewType}{Id}";
    }

    public virtual void OnClick(object arg1)
    {
        Click?.Invoke(arg1, EventArgs.Empty);
    }

    public virtual void OnContentChanged(object arg1)
    {
        ContentChanged?.Invoke(arg1, EventArgs.Empty);
    }

    public virtual void OnExecute(object sender, ViewEventArgsDataAction args)
    {
        Execute?.Invoke(sender, args);
    }

    public virtual void OnCanExecute(object arg1, ViewEventArgsCanExecute arg2)
    {
        CanExecute?.Invoke(arg1, arg2);
    }
}