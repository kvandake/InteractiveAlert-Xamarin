using System;

namespace InteractiveAlert
{
	public static class InteractiveAlerts
	{
		private static readonly Lazy<IInteractiveAlerts> InstanceLazy = new Lazy<IInteractiveAlerts>(() => new InteractiveAlertsImpl());
		public static IInteractiveAlerts Instance => InstanceLazy.Value;
	}
}