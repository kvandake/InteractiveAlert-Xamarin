using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;

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
				var dialogAlert = new SCLAlertDialog(Core.InteractiveAlertStyle.Warning);
				dialogAlert.SetTitleText("Good job!");
				dialogAlert.SetContentText("You clicked the button!");
				dialogAlert.Show(this.SupportFragmentManager, "success");
				dialogAlert.SetShowOk(true);
				dialogAlert.SetShowCancel(true);
				return;

				new SweetAlertDialog(this, SweetAlertDialog.ERROR_TYPE)
					.SetTitleText("Good job!")
					.SetContentText("You clicked the button!")
					.Show();
			};
		}
	}
}