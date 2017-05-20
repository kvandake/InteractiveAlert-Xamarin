namespace InteractiveAlert.Sample
{
    using System;
    using System.Collections.Generic;

    public class SampleViewModel
    {
        private readonly IInteractiveAlerts interactiveAlerts;

        public SampleViewModel()
        {
            this.interactiveAlerts = InteractiveAlerts.Instance;
            this.PopulateItems();
        }

        public IList<AlertConfigItem> Items { get; set; }

        private void PopulateItems()
        {
            this.Items = new List<AlertConfigItem>
            {
                this.SuccessConfig(),
                this.ErrorConfig(),
                this.WarningConfig(),
                this.WaitConfig(),
                this.EditableMultiLineConfig(),
                this.EditableSingleLineConfig()
            };
        }

        public AlertConfigItem SuccessConfig() => this.CreateAlertConfigItem("Success alert", "Success alert!", InteractiveAlertStyle.Success);

        public AlertConfigItem ErrorConfig() => this.CreateAlertConfigItem("Error alert", "Error alert!", InteractiveAlertStyle.Error);

        public AlertConfigItem WarningConfig() => this.CreateAlertConfigItem("Warning alert", "Warning alert!", InteractiveAlertStyle.Warning);

        public AlertConfigItem WaitConfig() => this.CreateAlertConfigItem("Wait alert", "Wait alert!", InteractiveAlertStyle.Wait);

        public AlertConfigItem EditableMultiLineConfig() => this.CreateEditableAlertConfigItem("Editable multiline alert", "Editable alert!", false, InteractiveAlertStyle.Edit);

        public AlertConfigItem EditableSingleLineConfig() => this.CreateEditableAlertConfigItem("Editable singleline alert", "Editable alert!", true, InteractiveAlertStyle.Edit);

        protected AlertConfigItem CreateAlertConfigItem(string title, string alertMessage, InteractiveAlertStyle style)
        {
            var config = new AlertConfigItem
            {
                Title = title
            };

            config.Command = () =>
            {
                var alertConfig = new InteractiveAlertConfig
                {
                    OkButton = new InteractiveActionButton(),
                    CancelButton = new InteractiveActionButton(),
                    Message = alertMessage,
                    Title = "Good job!",
                    Style = style
                };

                this.interactiveAlerts.ShowAlert(alertConfig);
            };

            return config;
        }

        protected AlertConfigItem CreateEditableAlertConfigItem(string title, string alertMessage, bool singleLine, InteractiveAlertStyle style)
        {
            var config = new AlertConfigItem
            {
                Title = title
            };

            config.Command = () =>
            {
                var alertConfig = new EditableInteractiveAlertConfig
                {
                    OkButton = new InteractiveActionButton(),
                    CancelButton = new InteractiveActionButton(),
                    Message = alertMessage,
                    Title = "Good job!",
                    Style = style,
                    SingleLine = singleLine
                };

                this.interactiveAlerts.ShowAlert(alertConfig);
            };

            return config;
        }

        public class AlertConfigItem
        {
            public string Title { get; set; }

            public Action Command { get; set; }
        }
    }
}