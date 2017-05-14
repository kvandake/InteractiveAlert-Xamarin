using System;

using UIKit;
using InteractiveAlert.Sample;

namespace InteractiveAlert.iOS.Sample
{
    public partial class ViewController : UIViewController
    {
        private UIStackView stackView;
        private SampleViewModel viewModel;

        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Frame = this.View.Bounds,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.FillProportionally
            };

            this.View.AddSubview(this.stackView);
            this.viewModel = new SampleViewModel(InteractiveAlerts.Instance);
            foreach (var item in this.viewModel.Items)
            {
                var button = new UIButton(UIButtonType.System);
                button.TouchUpInside += (s, e) =>
                {
                    item.Command();
                };
                button.SetTitle(item.Title, UIControlState.Normal);
                this.stackView.AddArrangedSubview(button);
            }
        }
    }
}
