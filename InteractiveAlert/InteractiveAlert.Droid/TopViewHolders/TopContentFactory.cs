using System;
using Android.Content;
using Android.Views;

namespace InteractiveAlert.Droid
{
    public static class TopContentFactory
    {
        public static ITopContentViewHolder CreateTopViewHolder(Context context, ViewGroup root, InteractiveAlertStyle alertStyle)
        {
            switch (alertStyle)
            {
                case InteractiveAlertStyle.Success:
                    return new SuccessTopContentViewHolder(context, root);
                case InteractiveAlertStyle.Error:
                    return new ErrorTopContentViewHolder(context, root);
                case InteractiveAlertStyle.Wait:
                    return new WaitTopContentViewHolder(context, root);
                case InteractiveAlertStyle.Warning:
                    return new WarningTopContentViewHolder(context, root);
                case InteractiveAlertStyle.Edit:
                    return new EditTopContentViewHolder(context, root);
                default:
                    return null;
            }
        }
    }
}