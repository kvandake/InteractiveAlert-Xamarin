using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using InteractiveAlert.Droid;
using InteractiveAlert;
using InteractiveAlert.Sample;

namespace InteractiveAlert.Droid.Sample
{
    [Activity(Label = "InteractiveAlert.Droid.Sample", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/MyTheme")]
    public class MainActivity : AppCompatActivity
    {
        private SampleViewModel viewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            InteractiveAlerts.Init(() => this);
            this.viewModel = new SampleViewModel(InteractiveAlerts.Instance);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var linearLayout = this.FindViewById<LinearLayout>(Resource.Id.main_linear_layout);
            foreach (var item in this.viewModel.Items)
            {
                var button = new Button(this);
                button.Text = item.Title;
                button.Click += (s, e) => { item.Command(); };
                linearLayout.AddView(button);
            }
        }
    }
}