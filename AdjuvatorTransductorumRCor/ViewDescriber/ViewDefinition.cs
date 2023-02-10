using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using AdjuvatorTransductorumRCor.Model;

namespace AdjuvatorTransductorumRCor.ViewDescriber;

public class ViewDefinition : ViewAttrBase
{
    private List<ViewForm> _forms = new();

    private DataModel? _data;
    public DataModel? Data
    {
        get => _data;
        set
        {
            _data = value;
            OnPropertyChanged();
        }
    }
    
    public event Action? InjectionFinished;
    public event Action? ExtractionFinished;

    public List<ViewForm> Forms
    {
        get => new(_forms);
    }

    public ViewDefinition(string name, ViewForm[]? forms)
    {
        Name = name;
        if (forms == null) return;
        int maxWidth = 0;
        foreach (var form in forms)
        {
            AddForm(form);
            int formWidth = form.MarginRight + form.Width + form.MarginLeft;
            if (formWidth > maxWidth) maxWidth = formWidth;
            Height += form.Height + form.MarginTop + form.MarginBottom;
        }

        Width = maxWidth;
    }

    public void AddForm(ViewForm form)
    {
        _forms.Add(form);
        form.Parent = this;
    }

    public void OnExtractionFinished()
    {
        ExtractionFinished!.Invoke();
    }

    public void OnInjectionFinished()
    {
        InjectionFinished!.Invoke();
    }

    public ViewForm? FindViewByName(string name) => _forms.FirstOrDefault(form => form.Name == name);
}