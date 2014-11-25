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
    /// <summary>
    /// I cheated.  This is a copy of ProtobufFileCacheTest, but for ProtobufMemoryCacheProvider.
    /// </summary>
	[TestClass]
	public class ProtobufMemoryCacheTest
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
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
            Assert.IsNotNull(pfcp.Get<DummyData>(file));
        }

		[TestMethod]
		public void SaveCacheListTypeT()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, new List<DummyData>() { data }, typeof(DummyData));
            Assert.IsNotNull(pfcp.Get<List<DummyData>>(file, typeof(DummyData)));
		}

		[TestMethod]
		public void SaveCacheWhenTNotProtoContract()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyDataNoContract() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
            Assert.IsNull(pfcp.Get<DummyDataNoContract>(file));
        }

		[TestMethod]
		public void SaveCacheWhenTNotProtoContractAndSupressWarnings()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyDataNoContractSuppressWarning() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);
            Assert.IsNull(pfcp.Get<DummyDataNoContractSuppressWarning>(file));
		}

		[TestMethod]
		public void GetCacheSimpleTypeT()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, data);

			var result = pfcp.Get<DummyData>(file);
			Assert.AreEqual(data, result);
		}

		[TestMethod]
		public void GetCacheListTypeT()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			var data = new DummyData() { Age = 21, Name = "Jenny" };
			pfcp.Set(file, new List<DummyData>() { data }, typeof(DummyData));

			var result = pfcp.Get<List<DummyData>>(file, typeof(DummyData));
			Assert.AreEqual(data, result.First());
		}

		[TestMethod]
		public void AddorExecuteAndAddTest()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			const int thisAge = 18;
			const string thisName = "Sally";
			Func<DummyData> execute = () =>
				new DummyData() { Age = thisAge, Name = thisName };


			pfcp.GetOrExecuteAndAdd(file, execute);
			var memoryData = pfcp.Get<DummyData>(file);
			Assert.AreEqual(thisAge, memoryData.Age);
			Assert.AreEqual(thisName, memoryData.Name);

		}

        //[TestMethod]
        //public void DisableCacheTest()
        //{
        //    var pfcp = new ProtobufMemoryCacheProvider();
        //    var file = UnitTestHelper.GenrateFileName();
        //    const string thisAge = "18";
        //    const string thisName = "Sally";
        //    Func<DummyData2> execute = () =>
        //        new DummyData2() { Age = thisAge, Name = thisName };


        //    Assert.IsNull(pfcp.Get<DummyData2>(file), "Pre-check");
        //    pfcp.GetOrExecuteAndAdd(file, execute);
        //    Assert.IsNull(pfcp.Get<DummyData2>(file), "Post-check");
        //}

        [TestMethod]
	    public void DisableIEnumerableTest()
	    {
            var pfcp = new ProtobufMemoryCacheProvider();
            var file = UnitTestHelper.GenrateFileName();
            const string thisAge = "18";
            const string thisName = "Sally";
            Func<List<DummyData2>> execute = () =>
                new List<DummyData2>()
                {
                    new DummyData2(){Age = thisAge, Name = thisName}
                };


            pfcp.GetOrExecuteAndAdd(file, execute, typeof(DummyData2));
            Assert.IsNull(pfcp.Get<List<DummyData2>>(file));

	    }

		[TestMethod]
		public void AddorExecuteAndAddWithListTest()
		{
			var pfcp = new ProtobufMemoryCacheProvider();
			var file = UnitTestHelper.GenrateFileName();
			const int thisAge = 18;
			const string thisName = "Sally";
			Func<List<DummyData>> execute = () =>
				new List<DummyData>() { new DummyData() { Age = thisAge, Name = thisName } };

			pfcp.GetOrExecuteAndAdd(file, execute, typeof(DummyData));
			var memoryData = pfcp.Get<List<DummyData>>(file, typeof(DummyData));
			Assert.AreEqual(thisAge, memoryData.First().Age);
			Assert.AreEqual(thisName, memoryData.First().Name);

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
