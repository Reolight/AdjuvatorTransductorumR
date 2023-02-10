using AdjuvatorTransductorumRCor.Model;

namespace AdjuvatorTransductorumRCor.ViewDescriber;

public class ViewEventArgsDataAction : EventArgs
{
    public DataModel? DataExtracted;
    public bool Injected = false;
}