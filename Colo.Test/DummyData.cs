using System;
using ProtoBuf;

namespace Colo.Test
{
    [ProtoContract]
    public class DummyData
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int Age { get; set; }

        public override bool Equals(object obj)
        {
            var compare = obj as DummyData;
            if (null != compare)
            {
                return Age == compare.Age && string.Equals(Name, compare.Name, StringComparison.CurrentCulture);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Age.GetHashCode() + Name.GetHashCode();
        }
    }

    [ProtoContract]
    public class DummyData2
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Age { get; set; }
    }

    public class DummyDataNoContract
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [SuppressCachingWarning]
    public class DummyDataNoContractSuppressWarning
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
