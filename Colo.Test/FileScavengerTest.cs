using System.IO;
using Colo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Colo.Test
{
    [TestClass]
    public class FileScavengerTest
    {
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			var path = Path.Combine(context.DeploymentDirectory, CachingSection.GetSection.Path);
			Directory.CreateDirectory(path);
		}

        [TestMethod]
        public void SetTest()
        {
            var fscp = new FileScavengerCacheProvider();
            var key = UnitTestHelper.GenrateFileName();
            var d = new DummyData(){ Age = 32, Name="blah"};
            fscp.Set(key, d);

            Assert.IsTrue(true);

        }

        [TestMethod]
        public void SaveTestNoType()
        {
            var fscp = new FileScavengerCacheProvider();
            var key = FileScavengerCacheProvider.CombinePathAndFileName(UnitTestHelper.GenrateFileName());
            using (new FileStream(key, FileMode.Create))
            {
            }
            Assert.IsTrue(File.Exists(key));
            fscp.Set(key);
        }

        [TestMethod]
        public void GetItemInCacheTest()
        {
            var fscp = new FileScavengerCacheProvider();
            var key = UnitTestHelper.GenrateFileName();
            var d = new DummyData() { Age = 32, Name = "blah" };
            fscp.Set(key, d);

            var r = fscp.Get<DummyData>(key);            
            Assert.IsNull(r);
        }

        [TestMethod]
        public void GetItemNotInCacheTest()
        {
            var fscp = new FileScavengerCacheProvider();
            Assert.IsNull(fscp.Get<DummyData>(UnitTestHelper.GenrateFileName()));
        }

        [TestMethod]
        public void FileCleanupTest()
        {            
            var fscp = new FileScavengerCacheProvider();
            var key = UnitTestHelper.GenrateFileName();

            using (new FileStream(key, FileMode.Create))
            {
            }
            Assert.IsTrue(File.Exists(key));

            var d = new DummyData() { Age = 32, Name = "blah" };
            fscp.Set(key, d);

			fscp.Invalidate(key);
            
			// Check that the file got cleaned up
            Assert.IsFalse(File.Exists(key));
        }

        [TestMethod]
        public void CacheDefaultTest()
        {
            var dummy = new DummyData();
            var cd = BaseCacheProvider.GetCacheDefaults(dummy);
            Assert.AreEqual(45, cd.CacheLife);
            Assert.IsTrue(cd.Enabled);
        }

        [TestMethod]
        public void CacheDefaultWithEnumerableTest()
        {
            var dummies = new[] {new DummyData(), new DummyData(), new DummyData()};
            var cd = BaseCacheProvider.GetCacheDefaults(dummies, dummies.First().GetType());
            Assert.AreEqual(45, cd.CacheLife);
            Assert.IsTrue(cd.Enabled);
        }


        [TestMethod]
        public void CacheDefaultDisabledTest()
        {
            var dummy = new DummyData2();
            var cd = BaseCacheProvider.GetCacheDefaults(dummy);
            Assert.AreEqual(21, cd.CacheLife);
            Assert.IsFalse(cd.Enabled);
        }

        [TestMethod]
        public void CacheDefaulDisabledtWithEnumerableTest()
        {
            var dummies = new[] { new DummyData2(), new DummyData2(), new DummyData2() };
            var cd = BaseCacheProvider.GetCacheDefaults(dummies, dummies.First().GetType());
            Assert.AreEqual(21, cd.CacheLife);
            Assert.IsFalse(cd.Enabled);
        }
    }
}
