using System;

using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace InteractiveAlert.Droid
{
	public class BaseInteractiveDialogFragment<TConfig> : AppCompatDialogFragment where TConfig : InteractiveAlertConfig
	{
		protected TConfig Config { get; set; }

		private ITopContentViewHolder topViewHolder;

		protected BaseInteractiveDialogFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		protected BaseInteractiveDialogFragment()
		{

		}

		public override void OnStart()
		{
			base.OnStart();
			this.topViewHolder?.OnStart();
		}

		public override void OnPause()
		{
			base.OnPause();
			this.topViewHolder?.OnPause();
		}

		public override Android.App.Dialog OnCreateDialog(Android.OS.Bundle savedInstanceState)
		{
			AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this.Activity);
			var cancelConfig = this.Config.CancelButton;
			if (cancelConfig != null)
			{
				var cancelTitle = cancelConfig.Title ?? InteractiveAlertConfig.DefaultCancelText;
				alertDialogBuilder.SetNegativeButton(cancelTitle, (sender, e) =>
				{
					var handler = cancelConfig.Action;
					if (handler != null)
					{
						handler();
					}
					else
					{
						this.Dismiss();
					}
				});
			}

			var okConfig = this.Config.OkButton;
			if (okConfig != null)
			{
				var okTitle = okConfig.Title ?? InteractiveAlertConfig.DefaultOkText;
				alertDialogBuilder.SetPositiveButton(okTitle, (sender, e) =>
				{
					var handler = okConfig.Action;
					if (handler != null)
					{
						handler();
					}
					else
					{
						this.Dismiss();
					}
				});
			}

			var contentView = (LinearLayout)LayoutInflater.From(this.Context).Inflate(Resource.Layout.alert_dialog, null);
			var bottomView = contentView.FindViewById<LinearLayout>(Resource.Id.alert_dialog_bottom);
			this.OnSetContentView(contentView);

			// try set bottom view
			bottomView.Visibility = this.OnSetBottomView(bottomView) ? ViewStates.Visible : ViewStates.Gone;

			var topContentView = contentView.FindViewById<FrameLayout>(Resource.Id.alert_dialog_top);
			this.topViewHolder = TopContentFactory.CreateTopViewHolder(this.Context, topContentView, this.Config.Style);
			this.topViewHolder.ContentView.RequestLayout();

			// set text
			this.SetContentText(contentView, Resource.Id.alert_dialog_title, this.Config.Title);
			this.SetContentText(contentView, Resource.Id.alert_dialog_content, this.Config.Message);
			alertDialogBuilder.SetView(contentView);

		    var dialog = alertDialogBuilder.Create();
		    dialog.SetCanceledOnTouchOutside(cancel: this.Config.IsCancellable);
		    dialog.SetCancelable(flag: this.Config.IsCancellable);

		    return dialog;
        }

		protected void SetContentText(View contentView, int textViewId, string text)
		{
			var textView = contentView.FindViewById<TextView>(textViewId);
			if (string.IsNullOrEmpty(text))
			{
				textView.Visibility = ViewStates.Gone;
			}
			else
			{
				textView.Text = text;
			}
		}

		protected virtual void OnSetContentView(ViewGroup viewGroup)
		{

		}

		protected virtual bool OnSetBottomView(ViewGroup viewGroup)
		{
			return false;
		}

		public static T NewInstance<T>(TConfig alertConfig) where T : BaseInteractiveDialogFragment<TConfig>
		{
			var dialogFragment = (T)Activator.CreateInstance(typeof(T));
			dialogFragment.Config = alertConfig;

			return dialogFragment;
		}
	}
}