using AdjuvatorTransductorumRCor.Model;

namespace WpfAdjuvatorTransductoris.Providers;

public interface IDataDependable
{
    public DataModel? Data { get; set; }
}