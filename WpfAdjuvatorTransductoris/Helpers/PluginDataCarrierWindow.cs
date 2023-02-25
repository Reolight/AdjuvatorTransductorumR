using System.Windows;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.ViewDescriber;

namespace WpfAdjuvatorTransductoris.Helpers;

public class PluginDataCarrierWindow : Window
{
    public DataModel? Data;

    public PluginDataCarrierWindow(ViewDefinition definition,  DataModel? model)
    {
        Data = model;
        definition.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(Data)) return;
            var viewDefinition = sender as ViewDefinition;
            Data = viewDefinition?.Data;
        };
        
        if (Data == null)
            definition.ExtractionFinished += Close;
        else
        {
            definition.InjectionFinished += Close;
            definition.Data = Data;
        }
    }
}