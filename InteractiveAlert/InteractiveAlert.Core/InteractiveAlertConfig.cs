namespace InteractiveAlert
{
    public class InteractiveAlertConfig
    {
        public static string DefaultOkText { get; set; } = "Ok";

        public static string DefaultCancelText { get; set; } = "Cancel";

        private InteractiveActionButton okButton;
        private InteractiveActionButton cancelButton;

        public InteractiveAlertStyle Style { get; set; } = InteractiveAlertStyle.Success;

        public string Title { get; set; }

        public string Message { get; set; }


        public InteractiveActionButton OkButton
        {
            get { return this.okButton; }
            set
            {
                this.okButton = value;
                if (string.IsNullOrEmpty(this.okButton.Title))
                {
                    this.okButton.Title = DefaultOkText;
                }
            }
        }

        public InteractiveActionButton CancelButton
        {
            get { return this.cancelButton; }

            set
            {
                this.cancelButton = value;
                if (string.IsNullOrEmpty(this.cancelButton.Title))
                {
                    this.cancelButton.Title = DefaultCancelText;
                }
            }
        }

        public bool IsCancellable { get; set; } = true;
    }
}