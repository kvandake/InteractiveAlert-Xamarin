using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using InteractiveAlert.Droid;
using InteractiveAlert;

namespace SCLAlertView.Droid.Sample
{
	[Activity(Label = "SCLAlertView.Droid.Sample", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/MyTheme")]
	public class MainActivity : AppCompatActivity
	{
		int count = 1;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);
			button.Click += (s, e) =>
			{
				var config = new InteractiveAlertConfig
				{
					OkButton = new InteractiveActionButton(),
					CancelButton = new InteractiveActionButton(),
					Message = "You clicked the button!",
					Title = "Good job!",
					Style = InteractiveAlertStyle.Success
				};
				var dialogAlert = InteractiveDialogFragment.NewInstance<InteractiveDialogFragment>(config);
				dialogAlert.Show(this.SupportFragmentManager, "success");
			};

			Button editableButton = FindViewById<Button>(Resource.Id.editable_button);
			editableButton.Click += (s, e) =>
			{
				var config = new EditableInteractiveAlertConfig
				{
					OkButton = new InteractiveActionButton(),
					CancelButton = new InteractiveActionButton(),
					Message = "You clicked the button!",
					Title = "Good job!",
					Style = InteractiveAlertStyle.Success
				};
				var dialogAlert = EditableInteractiveDialogFragment.NewInstance<EditableInteractiveDialogFragment>(config);
				dialogAlert.Show(this.SupportFragmentManager, "success");
			};
		}
	}
}