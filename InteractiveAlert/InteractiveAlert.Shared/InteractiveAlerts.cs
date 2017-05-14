namespace InteractiveAlert
{
    using System;

#if __ANDROID__
    using Android.Support.V7.App;
#endif

    public static class InteractiveAlerts
    {
        private static Lazy<IInteractiveAlerts> instanceLazy;

        internal static Lazy<IInteractiveAlerts> InstanceLazy => instanceLazy ?? (instanceLazy = new Lazy<IInteractiveAlerts>(() =>
        {
#if __IOS__
            return new InteractiveAlertsImpl();
#elif __ANDROID__
            throw new ArgumentException("In android, you must call InteractiveAlerts.Init(Activity) from your first activity OR InteractiveAlerts.Init(App) from your custom application OR provide a factory function to get the current top activity via UserDialogs.Init(() => supply top activity)");
#else
            throw new ArgumentException("This is the PCL library, not the platform library.  You must install the nuget package in your main executable/application project");
#endif
        }));

#if __ANDROID__

        public static void Init(Func<AppCompatActivity> topActivityFunc)
        {
            instanceLazy = new Lazy<IInteractiveAlerts>(() => new InteractiveAlertsImpl(topActivityFunc));
        }
#endif

        public static IInteractiveAlerts Instance => InstanceLazy.Value;
    }
}