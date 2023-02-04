//This is a layer for easier Data providing
//Because: ViewModel contains Data, VMStructure contains and Who-knows-what-else contains
//So, here is a single place for changing Data for all at once
//There were two ways to implement the feature:
//1. Create an interface IDataDependable and have an array of connected entities implementing it
//2. Stupidly have every typed instance requiring Data.
//I've decided choose the 1st, cuz it removes extra work and makes this class nearly immutable (cool);
using System.Collections.Generic;
using AdjuvatorTransductorumRCor.Model;

namespace WpfAdjuvatorTransductoris.Providers;

public class ViewModelDataProvider
{
    private DataModel _data;
    public DataModel Data
    {
        get => _data;
        set
        {
            _data = value;
            foreach (var dependable in _dataDependables)
            {
                dependable.Data = _data;
            }
        }
    }
    
    private List<IDataDependable> _dataDependables = new ();

    public void Connect(IDataDependable instance)
    {
        _dataDependables.Add(instance);
        instance.Data = _data;
    }

    public void Disconnect(IDataDependable instance)
    {
        _dataDependables.Remove(instance);
    }

    public ViewModelDataProvider(DataModel data)
    {
        _data = data;
    }
}