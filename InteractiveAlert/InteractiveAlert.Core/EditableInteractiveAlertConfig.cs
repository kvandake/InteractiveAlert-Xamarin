using System;

namespace InteractiveAlert
{
	public class EditableInteractiveAlertConfig : InteractiveAlertConfig
	{
		public bool SingleLine { get; set; } = true;

		public string Text { get; set; }

		public string Placeholder { get; set; }
	}
}