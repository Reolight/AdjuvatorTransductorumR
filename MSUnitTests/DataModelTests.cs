using System.Diagnostics;
using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;
using AdjuvatorTransductorumRCor.PluginCommonInterface;

namespace MSUnitTests;

[TestClass]
public class DataModelTests
{
    private static readonly Core AppCore;
    private static readonly DataModel TestableDataModel;

    // Change this to test your plugin
    private const string TestablePluginName = "JSON i18next";
    private static readonly object TestableDataModelSourceInfo = "./locales";
    private readonly Stopwatch _stopwatch = new();

    static DataModelTests()
    {
        AppCore = new();
        IDataProvider dataProvider = AppCore.GetProvider(TestablePluginName) ??
                                     throw new NullReferenceException("Requested plugin not found");
        TestableDataModel = dataProvider.ExtractData(TestableDataModelSourceInfo);
    }

    private object WrapperWatcher(Func<object> func)
    {
        _stopwatch.Reset();
        _stopwatch.Start();
        var obj = func();
        _stopwatch.Stop();
        return obj;
    }

    [TestMethod]
    public void TestGetNode_SecondLevel()
    {
        var node = (DataModelBase)WrapperWatcher(() => TestableDataModel.Root.GetNode("first:second"));
        Console.WriteLine($"TestGetNode_SecondLevel: {_stopwatch.ElapsedMilliseconds}");
        Assert.IsNotNull(node);
    }

    [TestMethod]
    public void TestGetNode_ThirdLevel()
    {
        var node = (DataModelBase)WrapperWatcher(() =>
            TestableDataModel.Root.GetNode("first:second:third"));
        Console.WriteLine($"TestGetNode_ThirdLevel: {_stopwatch.ElapsedMilliseconds}");
        Assert.IsNotNull(node);
    }

    [TestMethod]
    public void TestGetNode_FourthLevel()
    {
        var node = (DataModelBase)WrapperWatcher(() =>
            TestableDataModel.Root.GetNode("first:second:third:fourth"));
        Console.WriteLine($"TestGetNode_FourthLevel: {_stopwatch.ElapsedMilliseconds}");
        Assert.IsNotNull(node);
    }

    [TestMethod]
    public void TestGetNode_FifthLevel()
    {
        var node = (DataModelBase)WrapperWatcher(() =>
            TestableDataModel.Root.GetNode("first:second:third:fourth:fifth"));
        Console.WriteLine($"TestGetNode_ThirdLevel: {_stopwatch.ElapsedMilliseconds}");
        Assert.IsNotNull(node);

    }
}