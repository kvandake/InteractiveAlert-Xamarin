using System;
using Android.Support.V7.App;

namespace InteractiveAlert
{
	public static class InteractiveAlerts
	{
		private static Lazy<IInteractiveAlerts> _instanceLazy;
		public static IInteractiveAlerts Instance => _instanceLazy.Value;

		public static void Init(Func<AppCompatActivity> topActivityFunc)
		{
			_instanceLazy = new Lazy<IInteractiveAlerts>(() => new InteractiveAlertsImpl(topActivityFunc));
		}
	}
}