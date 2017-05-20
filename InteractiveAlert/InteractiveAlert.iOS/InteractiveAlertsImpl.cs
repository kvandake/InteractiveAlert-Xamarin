namespace InteractiveAlert
{
    using System;

    public class InteractiveAlertsImpl : IInteractiveAlerts
    {
        public IDisposable ShowAlert(InteractiveAlertConfig alertConfig)
        {
            var alertView = this.CreateAlertView(alertConfig);

            return new DisposableAction(alertView.HideView);
        }

        public IDisposable ShowAlert(EditableInteractiveAlertConfig alertConfig)
        {
            var alertView = this.CreateAlertView(alertConfig);
            if (alertConfig.SingleLine)
            {
                var textField = alertView.AddTextField(alertConfig.Placeholder);
                textField.Text = alertConfig.Text;
                textField.EditingChanged += (s, e) =>
                {
                    alertConfig.Text = textField.Text;
                };
            }
            else
            {
                var textView = alertView.AddTextView();
                textView.Text = alertConfig.Text;
                textView.Changed += (s, e) =>
                {
                    alertConfig.Text = textView.Text;
                };
            }

            return new DisposableAction(alertView.HideView);
        }

        protected InteractiveAlertView CreateAlertView(InteractiveAlertConfig alertConfig)
        {
            var appearance = new InteractiveAlertView.SCLAppearance();
            appearance.ShowCloseButton = alertConfig.CancelButton != null;
            appearance.ShouldAutoDismiss = alertConfig.IsCancellable;

            var alertView = new InteractiveAlertView(appearance);

            alertView.SetDismissBlock(alertConfig.CancelButton?.Action);
            if (alertConfig.OkButton != null)
            {
                alertView.AddButton(alertConfig.OkButton.Title, alertConfig.OkButton.Action);
            }

            alertView.ShowAlert(alertConfig.Style, alertConfig.Title, alertConfig.Message, alertConfig.CancelButton?.Title);

            return alertView;
        }
    }
}