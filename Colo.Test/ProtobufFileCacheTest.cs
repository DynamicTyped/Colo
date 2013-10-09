using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	}
}
