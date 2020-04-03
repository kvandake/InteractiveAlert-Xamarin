using System;

namespace InteractiveAlert
{
    public static class InteractiveAlerts
    {
        private static readonly Lazy<IInteractiveAlerts> InstanceLazy = new Lazy<IInteractiveAlerts>(() => throw new ArgumentException("This is the PCL library, not the platform library.  You must install the nuget package in your main executable/application project"));
        public static IInteractiveAlerts Instance => InstanceLazy.Value;
    }
}
