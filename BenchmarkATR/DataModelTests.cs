using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.PluginCommonInterface;
using BenchmarkDotNet.Attributes;
using System.Diagnostics;

namespace BenchmarkATR;

public class DataModelTests
{
    private Core? _appCore;
    private DataModel _testableDataModel;

    // Change this to test your plugin
    private const string TestablePluginName = "JSON i18next";
    private static readonly object TestableDataModelSourceInfo = "../../../../locales";

    [GlobalSetup]
    public void DataModelSetup()
    {
        _appCore ??= new Core("plugFold", "..\\..\\..\\..\\Plugins");
        IDataProvider dataProvider = _appCore.GetProvider(TestablePluginName) ??
                                     throw new NullReferenceException("Requested plugin not found");
        _testableDataModel = dataProvider.ExtractData(TestableDataModelSourceInfo);
    }

    [Benchmark]
    public void TestGetNode_SecondLevel()
    {
        _testableDataModel.Root.GetNode("first:second");
    }

    [Benchmark]
    public void TestGetNode_ThirdLevel()
    {
        _testableDataModel.Root.GetNode("first:second:third");
    }

    [Benchmark]
    public void TestGetNode_FourthLevel()
    {
        _testableDataModel.Root.GetNode("first:second:third:fourth");
    }

    [Benchmark]
    public void TestGetNode_FifthLevel()
    {
        _testableDataModel.Root.GetNode("first:second:third:fourth:fifth");
    }
}