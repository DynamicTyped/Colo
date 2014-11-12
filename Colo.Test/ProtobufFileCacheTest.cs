using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Colo.Configuration;
using Colo.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Colo.Test
{
	[TestClass]
	public class ProtobufFileCacheTest
	{
		[ClassInitialize]
		public static void ClassInit(TestContext context)
		{
			var path = Path.Combine(context.DeploymentDirectory, CachingSection.GetSection.Path);
			Directory.CreateDirectory(path);
		}

		[TestMethod]
		public void SaveCacheSimpleTypeT()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
			Assert.IsTrue(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
		}

		[TestMethod]
		public void SaveCacheListTypeT()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, new List<DummyData>() { data }, typeof(DummyData));
			Assert.IsTrue(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
		}

		[TestMethod]
		public void SaveCacheWhenTNotProtoContract()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyDataNoContract() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
			Assert.IsFalse(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
		}

		[TestMethod]
		public void SaveCacheWhenTNotProtoContractAndSupressWarnings()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyDataNoContractSuppressWarning() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
			Assert.IsFalse(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
		}

		[TestMethod]
		public void GetCacheSimpleTypeT()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);

			var result = pfcp.Get<DummyData>(file);
			Assert.AreEqual(data, result);
		}

		[TestMethod]
		public void GetCacheListTypeT()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, new List<DummyData>() { data }, typeof(DummyData));

			var result = pfcp.Get<List<DummyData>>(file, typeof(DummyData));
			Assert.AreEqual(data, result.First());
		}

		[TestMethod]
		public void AddorExecuteAndAddTest()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			const int thisAge = 18;
			const string thisName = "Sally";
			Func<DummyData> execute = () =>
				new DummyData() { Age = thisAge, Name = thisName };


			pfcp.GetOrExecuteAndAdd(file, execute);
			Assert.IsTrue(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
			var memoryData = pfcp.Get<DummyData>(file);
			Assert.AreEqual(thisAge, memoryData.Age);
			Assert.AreEqual(thisName, memoryData.Name);

		}

        [TestMethod]
        public void DisableCacheTest()
        {
            var pfcp = new ProtobufFileCacheProvider();
            var file = UnitTestHelper.GenrateFileName();
            const string thisAge = "18";
            const string thisName = "Sally";
            Func<DummyData2> execute = () =>
                new DummyData2() { Age = thisAge, Name = thisName };


            pfcp.GetOrExecuteAndAdd(file, execute);
            Assert.IsFalse(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));


        }

        [TestMethod]
	    public void DisableIEnumerableTest()
	    {
            var pfcp = new ProtobufFileCacheProvider();
            var file = UnitTestHelper.GenrateFileName();
            const string thisAge = "18";
            const string thisName = "Sally";
            Func<List<DummyData2>> execute = () =>
                new List<DummyData2>()
                {
                    new DummyData2(){Age = thisAge, Name = thisName}
                };


            pfcp.GetOrExecuteAndAdd(file, execute, typeof(DummyData2));
            Assert.IsFalse(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
	    }

		[TestMethod]
		public void AddorExecuteAndAddWithListTest()
		{
			var pfcp = new ProtobufFileCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			const int thisAge = 18;
			const string thisName = "Sally";
			Func<List<DummyData>> execute = () =>
				new List<DummyData>() { new DummyData() { Age = thisAge, Name = thisName } };

			pfcp.GetOrExecuteAndAdd(file, execute, typeof(DummyData));
			Assert.IsTrue(File.Exists(Path.Combine(CachingSection.GetSection.Path, file)));
			var memoryData = pfcp.Get<List<DummyData>>(file, typeof(DummyData));
			Assert.AreEqual(thisAge, memoryData.First().Age);
			Assert.AreEqual(thisName, memoryData.First().Name);

		}

		[TestMethod]
		public void GetReturnsNullWhenFileIsDeleted()
		{
			var provider = new ProtobufFileCacheProvider();
			var key = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 10, Name = "Foo" };
			var path = Path.Combine(FileScavengerCacheProvider.Path, key); 
			
			// Create the cache entry
			provider.Set(key, data);

			// Delete the file and make sure it's gone
			File.Delete(path);
			Assert.IsFalse(File.Exists(path));

			// Try to get the cache entry and ensure it's null
			var result = provider.Get<DummyData>(key);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void SetCreatesFile()
		{
			// Validates assumptions about where cache files are stored. This proves various other tests.
			var provider = new ProtobufFileCacheProvider();
			var key = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 10, Name = "Foo" };
			var path = Path.Combine(FileScavengerCacheProvider.Path, key); 
			
			// Create a cache entry and make sure the file exists
			provider.Set(key, data);
			Assert.IsTrue(File.Exists(path));
		}

		[TestMethod]
		public void SetCreatesFileWhenExistingFileWasDeleted()
		{
			var provider = new ProtobufFileCacheProvider();
			var key = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 10, Name = "Foo" };
			var path = Path.Combine(FileScavengerCacheProvider.Path, key); 
			
			// Create initial cache entry
			provider.Set(key, data);

			// Delete the file
			File.Delete(path);
			Assert.IsFalse(File.Exists(path));

			// Try to set another cache entry with the same key
			provider.Set(key, data);
			Assert.IsTrue(File.Exists(path));
		}

		[TestMethod]
		public void SetOverwritesFileThatIsNotInCache()
		{
			var provider = new ProtobufFileCacheProvider();
			var key = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 10, Name = "Foo" };
			var path = Path.Combine(FileScavengerCacheProvider.Path, key);
			
			// Create a conflicting file
			using (File.Create(path)) { }
			Assert.IsTrue(File.Exists(path));

			// Add the cache entry
			provider.Set(key, data);
			var result = provider.Get<DummyData>(key);
			Assert.AreEqual(data, result);
		}

	    [TestMethod]
	    public void TestCacheDefaultValueWalking()
	    {
            // Configuration section
            var cachingSection = CachingSection.GetSection;

            // Test with type T
	        var test = new CacheDefaultTest1C();
	        var default1 = BaseCacheProvider.GetCacheDefaults(test);
            var assert1 = (CacheDefaultValueElement)cachingSection.CacheDefaults["Colo.Test.Helpers.CacheDefaultTest1B"];
            Assert.AreSame(default1, assert1);

            // Test with type parameter (should revert to type T since 2B isn't enabled)
            var default2 = BaseCacheProvider.GetCacheDefaults(test, typeof(CacheDefaultTest2B));
            var assert2 = (CacheDefaultValueElement)cachingSection.CacheDefaults["Colo.Test.Helpers.CacheDefaultTest2A"];
            Assert.AreSame(default2, assert2);
            Assert.IsFalse(default2.Enabled);

            var default3 = BaseCacheProvider.GetCacheDefaults(test, typeof(CacheDefaultTest2C));
            var assert3 = (CacheDefaultValueElement)cachingSection.CacheDefaults["Colo.Test.Helpers.CacheDefaultTest2C"];
            Assert.AreSame(default3, assert3);

            // Test with an enumerable type
	        var test4 = Enumerable.Empty<CacheDefaultTest1C>();
            var default4 = BaseCacheProvider.GetCacheDefaults(test4);
            var assert4 = (CacheDefaultValueElement)cachingSection.CacheDefaults["Colo.Test.Helpers.CacheDefaultTest1B"];
            Assert.AreSame(default4, assert4);
        }
	}
}
