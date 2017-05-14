using System;

namespace InteractiveAlert
{
    public interface IInteractiveAlerts
    {
        IDisposable ShowAlert(InteractiveAlertConfig alertConfig);

        IDisposable ShowAlert(EditableInteractiveAlertConfig alertConfig);
    }
}