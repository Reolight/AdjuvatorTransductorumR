using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;

namespace MSUnitTests;

[TestClass]
public class CoreFuncTests
{
    [TestMethod]
    public void XmlLoaderTest()
    {
        var dataModel = DataModelXmlWriter.LoadDataModelFromXml("test");
        Assert.AreEqual("world", dataModel?.Root?.GetValue("loc.json:hello:en"));
    }
}