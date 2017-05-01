using System;

namespace InteractiveAlert
{
	public class InteractiveAlertConfig
	{
		public static string DefaultOkText { get; set; } = "Ok";

		public static string DefaultCancelText { get; set; } = "Cancel";

		public InteractiveAlertStyle Style { get; set; } = InteractiveAlertStyle.Success;

		public string Title { get; set; }

		public string Message { get; set; }

		public InteractiveActionButton OkButton { get; set; }

		public InteractiveActionButton CancelButton { get; set; }

		public bool IsCancellable { get; set; } = true;
	}
}