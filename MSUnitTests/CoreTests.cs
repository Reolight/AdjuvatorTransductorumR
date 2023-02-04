using AdjuvatorTransductorumRCor;

namespace MSUnitTests
{
    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void CoreIsNotNull() {
            Core core = new Core();
            Assert.IsNotNull(core);
        }

        [TestMethod]
        public void CoreLoadedPlugin_simpleTest() {
            Core core = new Core();
            Assert.IsTrue(core.PluginsCount > 0);
        }

        [TestMethod]
        public void CoreLoadedPlugin_middleTest() {
            Core core = new Core();
            string? path = Path.GetDirectoryName(typeof(Core).Assembly.Location);
            Assert.IsNotNull(path, "Coudn't perform a test with null path...");
            path += "\\Plugin\\";
            DirectoryInfo dir = new DirectoryInfo(path);
            int count = dir.GetFiles().Count(f => f.Extension == ".dll");
            core.GetPluginInfo().ToList().ForEach(s => Console.WriteLine(s));
            Assert.IsTrue(count == core.PluginsCount);
        }
    }
}
