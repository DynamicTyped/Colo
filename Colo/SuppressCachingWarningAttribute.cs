using System;

namespace Colo
{
    /// <summary>
    /// Prevents warnings for objects without proto contract
    /// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
	public sealed class SuppressCachingWarningAttribute : Attribute
	{
	}
}
