namespace JsonFileCache.Tests
{
    [TestClass]
    public class JsonFileCacheTests
    {
        private string _testPath;
        private JsonFileCache<string> _cache;

        [TestInitialize]
        public void TestInitialize()
        {
            _testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) + "/";
            _cache = new JsonFileCache<string>(_testPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(_testPath, true);
        }

        [TestMethod]
        public void TestCacheItem()
        {
            _cache.CacheItem("test", "testValue");
            Assert.IsTrue(_cache.LocalCache.ContainsKey("test"));
            Assert.AreEqual("testValue", _cache.LocalCache["test"]);
        }

        [TestMethod]
        public void TestGetLastItemUpdate()
        {
            _cache.CacheItem("test", "testValue");
            var lastUpdate = _cache.GetLastItemUpdate("test");
            Assert.IsTrue(DateTime.Now.Subtract(lastUpdate).TotalSeconds <= 1);
        }

        [TestMethod]
        public void TestLoadCache()
        {
            _cache.CacheItem("test", "testValue");
            var loadedCache = _cache.LoadCache();
            Assert.IsTrue(loadedCache.ContainsKey("test"));
            Assert.AreEqual("testValue", loadedCache["test"]);
        }
    }
}