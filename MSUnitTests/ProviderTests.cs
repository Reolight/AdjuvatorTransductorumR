using AdjuvatorTransductorumRCor;
using AdjuvatorTransductorumRCor.Model;

namespace MSUnitTests
{
    [TestClass]
    public class ProviderTests
    {
        DataModel? dataModel;
        private Core _core;
        public ProviderTests()
        {
            _core = new();
            string error = string.Empty;
            //dataModel = _core.GetDataModel("JSON\\locales\\", "JSON Local accessor (i18next)");
            DirectoryInfo injected = new DirectoryInfo("locales");
            if (injected.Exists ) 
                injected.Delete(true);
        }

        [TestMethod]
        public void ProviderSimpleTest()
        {
            Console.WriteLine($"Address: {dataModel?.OriginalAddress}. Root: {dataModel?.Root} ");
            Assert.IsNotNull(dataModel);
        }

        [TestMethod]
        public void DoesLangNotNull()
        {
            Assert.IsNotNull(dataModel?.Root?.GetNode("loc.json"));
        }

        [TestMethod]
        public void DoesKeyNotNull()
        {
            Assert.IsNotNull(dataModel?.Root?.GetNode("loc.json:hello"));
        }

        [TestMethod]
        public void doesHelloContainsWorld_langEN()
        {
            Assert.AreEqual("world", dataModel?.Root?.GetValue("loc.json:hello:en"));
        }

        [TestMethod]
        public void DoesHelloContainsWorld_langRU()
        {
            Assert.AreEqual("мир", dataModel?.Root?.GetValue("loc.json:hello:ru"));
        }

        [TestMethod]
        public void doesGetValueWorks_langJA()
        {
            Assert.AreEqual("世界", dataModel?.Root?.GetValue("loc.json:こんにちわ:ja"));
        }

        [TestMethod]
        public void LanguageCountTest()
        {
            Assert.AreEqual(3, dataModel?.Languages.Count);
        }

        [TestMethod]
        public void ComplexTests3()
        {
            Console.WriteLine( dataModel?.Redactor.ActiveNode.Name );
            dataModel.Redactor.AddNode("wut.json", NodeTypes.File);
            string second = dataModel.Redactor.ActiveNode.Name;
            Console.WriteLine(second);
            Assert.AreEqual("wut.json", second);
        }

        [TestMethod]
        public void ComplexTest4()
        {
            Assert.IsNotNull(dataModel);
            Console.WriteLine(dataModel.Redactor.ActiveNode.Name);
            dataModel.Redactor.AddNode("case.json", NodeTypes.File);
            string second = dataModel.Redactor.ActiveNode.Name;
            Console.WriteLine(second);
            dataModel.Redactor.AddValue("case", "en", "strange case");
            Console.WriteLine(dataModel.Redactor.ActiveNode.Name);
            List<string> keys = dataModel.Redactor.ActiveNode.GetNodes().Select(node => node.Name).ToList();
            Assert.IsTrue(keys.Contains("case") && keys.Count == 1);
        }

        [TestMethod]
        public void InjectorTest()
        {
            //_core.InjectDataModel(dataModel!, "locales", "JSON Local accessor (i18next)");
            var file = new FileInfo("locales\\ru\\loc.json");
            Assert.IsTrue(file.Exists && file.Length > 0);
        }

        [TestMethod]
        public void SaveTest()
        {
            Assert.IsNotNull(dataModel);
            Assert.IsTrue(DataModelXmlWriter.SaveDataModelAsXml(dataModel, "test"));
        }
    }
}
