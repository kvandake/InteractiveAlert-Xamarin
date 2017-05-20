namespace InteractiveAlert
{
    using System;

    using Android.Support.V7.App;

    using InteractiveAlert.Droid;

    public class InteractiveAlertsImpl : IInteractiveAlerts
    {
        private const string DefaultDialogTag = "InteractiveAlert";

        protected internal Func<AppCompatActivity> TopActivityFunc { get; set; }

        public InteractiveAlertsImpl(Func<AppCompatActivity> topActivityFunc)
        {
            this.TopActivityFunc = topActivityFunc;
        }

        public IDisposable ShowAlert(EditableInteractiveAlertConfig alertConfig)
        {
            var activity = this.TopActivityFunc();
            var dialogAlert = EditableInteractiveDialogFragment.NewInstance<EditableInteractiveDialogFragment>(alertConfig);
            dialogAlert.Show(activity.SupportFragmentManager, DefaultDialogTag);
            return new DisposableAction(dialogAlert.Dismiss);
        }

        public IDisposable ShowAlert(InteractiveAlertConfig alertConfig)
        {
            var activity = this.TopActivityFunc();
            var dialogAlert = InteractiveDialogFragment.NewInstance<InteractiveDialogFragment>(alertConfig);
            dialogAlert.Show(activity.SupportFragmentManager, DefaultDialogTag);
            return new DisposableAction(dialogAlert.Dismiss);
        }
    }
}