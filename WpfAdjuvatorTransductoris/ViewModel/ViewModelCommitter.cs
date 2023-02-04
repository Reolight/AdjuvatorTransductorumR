using AdjuvatorTransductorumRCor.Model;

namespace WpfAdjuvatorTransductoris.ViewModel;


/// <summary>
/// This class is relay for all changes applying to DataModel.
/// Why:
/// 1. Need to change DataModel;
/// 2. Need to save all changes in XML and in that time don't make overwork by stupidly remove and rewrite the file.
/// And as such, current class will just be a relay to call necessary methods.
/// Btw, there are no plans to introduce new instances depending on saving process. Thus the relay not have own interface like ISavingDependable
/// For a while?..
/// </summary>
public class ViewModelCommitter
{
    private DataBuilder? _builder;
    private static ViewModelCommitter? _instance;

    private ViewModelCommitter()
    {
        _instance = this;
    }
    
    public void Init(DataBuilder builder)
    {
        _builder = builder;
    }

    public static ViewModelCommitter GetInstance()
    {
        return _instance ?? new();
    }
}