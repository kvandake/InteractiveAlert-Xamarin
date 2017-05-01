using System;
using Android.Support.V4.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;

namespace InteractiveAlert.Droid
{
	public class SCLAlertDialogFragment : AppCompatDialogFragment
	{
		private readonly InteractiveAlertStyle alertStyle;

		private ITopContentViewHolder topViewHolder;

		protected SCLAlertDialogFragment()
		{

		}

		public SCLAlertDialogFragment(InteractiveAlertStyle alertStyle)
		{
			this.alertStyle = alertStyle;
		}

		public static string DefaultCancelText = "Cancel";

		public static string DefaultOkText = "OK";

		private string TitleText { get; set; }
		private string ContentText { get; set; }

		private bool ShowCancel { get; set; }

		private bool ShowOk { get; set; }

		public event EventHandler<DialogClickEventArgs> CancelClick;

		public event EventHandler<DialogClickEventArgs> OkClick;

		protected void SetContentText(View contentView, int textViewId, string text)
		{
			var textView = contentView.FindViewById<TextView>(textViewId);
			if (string.IsNullOrEmpty(this.TitleText))
			{
				textView.Visibility = ViewStates.Gone;
			}
			else
			{
				textView.Text = this.TitleText;
			}
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
			if (this.ShowCancel)
			{
				alertDialogBuilder.SetNegativeButton(DefaultCancelText, (sender, e) =>
				 {
					 var handler = this.CancelClick;
					 if (handler != null)
					 {
						 handler?.Invoke(this, e);
					 }
					 else
					 {
						 this.Dismiss();
					 }
				 });
			}

			if (this.ShowOk)
			{
				alertDialogBuilder.SetPositiveButton(DefaultOkText, (sender, e) =>
				{
					var handler = this.OkClick;
					if (handler != null)
					{
						handler?.Invoke(this, e);
					}
					else
					{
						this.Dismiss();
					}
				});
			}

			var contentView = (LinearLayout)LayoutInflater.From(this.Context).Inflate(Resource.Layout.alert_dialog, null);
			var buttonsLayout = contentView.FindViewById<LinearLayout>(Resource.Id.alert_dialog_buttons);
			var inputsLayout = contentView.FindViewById<LinearLayout>(Resource.Id.alert_dialog_inputs);

			buttonsLayout.Visibility = ViewStates.Gone;
			inputsLayout.Visibility = ViewStates.Gone;

			var topContentView = contentView.FindViewById<FrameLayout>(Resource.Id.alert_dialog_top);
			this.topViewHolder = TopContentFactory.CreateTopViewHolder(this.Context, topContentView, this.alertStyle);
			this.topViewHolder.ContentView.RequestLayout();

			// set text
			this.SetContentText(contentView, Resource.Id.alert_dialog_title, this.TitleText);
			this.SetContentText(contentView, Resource.Id.alert_dialog_content, this.ContentText);


			alertDialogBuilder.SetView(contentView);

			return alertDialogBuilder.Create();
		}

		public SCLAlertDialogFragment SetTitleText(string value)
		{
			this.TitleText = value;
			return this;
		}

		public SCLAlertDialogFragment SetShowOk(bool value)
		{
			this.ShowOk = value;

			return this;
		}

		public SCLAlertDialogFragment SetShowCancel(bool value)
		{
			this.ShowCancel = value;

			return this;
		}

		public SCLAlertDialogFragment SetContentText(string value)
		{
			this.ContentText = value;
			return this;
		}
	}
}