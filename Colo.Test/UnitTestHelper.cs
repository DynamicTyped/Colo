using System;

namespace Colo.Test
{
    public static class UnitTestHelper
    {
        public static string GenrateFileName()
        {
            return Guid.NewGuid() + ".dat";
        }
    }
}
