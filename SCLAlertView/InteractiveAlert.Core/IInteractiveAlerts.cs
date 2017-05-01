using System;

namespace InteractiveAlert
{
	public interface IInteractiveAlerts
	{
		void ShowAlert(InteractiveAlertConfig alertConfig);

		void ShowAlert(EditableInteractiveAlertConfig alertConfig);
	}
}