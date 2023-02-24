using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;

namespace MSUnitTests;

[TestClass]
public class CoreFuncTests
{
    [TestMethod]
    public void XmlLoaderTest()
    {
        var dataModel = DataModelXmlReader.LoadProject("test");
        Assert.AreEqual("world", dataModel?.Root?.GetValue("loc.json:hello:en"));
    }
}