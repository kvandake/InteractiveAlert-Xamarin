using System;

using UIKit;
using Service.Bit.Client.iOS;

namespace SCLAlertViewSample
{
    public partial class ViewController : UIViewController
    {
        private UIStackView stackView;

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
                Frame  = this.View.Bounds,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.FillProportionally
            };

            this.View.AddSubview(this.stackView);
            this.AttachButton("Success", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Success, "Success", "Success subTitle"));
            this.AttachButton("Wait", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Wait, "Wait", "Wait subTitle"));
            this.AttachButton("Warning", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Warning, "Warning", "Warning subTitle"));
            this.AttachButton("Question", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Question, "Question", "Question subTitle"));
            this.AttachButton("Notice", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Notice, "Notice", "Notice subTitle"));
            this.AttachButton("Info", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Info, "Info", "Info subTitle"));
            this.AttachButton("Error", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Error, "Error", "Error subTitle"));
            this.AttachButton("Edit", () => this.ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle.Edit, "Edit", "Edit subTitle"));
            this.AttachButton("Edit with textField", () => this.ShowSimpleAlertViewWithTextField(SCLAlertView.SCLAlertViewStyle.Edit, "Edit with textField", "Edit with textField subTitle"));
            this.AttachButton("Edit with textView", () => this.ShowSimpleAlertViewWithTextView(SCLAlertView.SCLAlertViewStyle.Edit, "Edit with textView", "Edit with textView subTitle"));
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void AttachButton(string title, Action clickAction)
        {
            var button = new UIButton(UIButtonType.System);
            button.TouchUpInside += (s, e) =>
            {
                clickAction();
            };
            button.SetTitle(title, UIControlState.Normal);

            this.stackView.AddArrangedSubview(button);
        }

        private void ShowSimpleAlertView(SCLAlertView.SCLAlertViewStyle style, string title, string subTitle = null)
        {
            var alertView = new SCLAlertView();
            alertView.AddButton("Custom Cancel", () => alertView.HideView());

            alertView.ShowAlert(style, title, subTitle, "Cancel");
        }

        private void ShowSimpleAlertViewWithTextField(SCLAlertView.SCLAlertViewStyle style, string title, string subTitle = null)
        {
            var alertView = new SCLAlertView();
            alertView.AddButton("Custom Cancel", () => alertView.HideView());
            alertView.AddTextField("textfield title");

            alertView.ShowAlert(style, title, subTitle, "Cancel");
        }

        private void ShowSimpleAlertViewWithTextView(SCLAlertView.SCLAlertViewStyle style, string title, string subTitle = null)
        {
            var alertView = new SCLAlertView();
            alertView.AddButton("Custom Cancel", () => alertView.HideView());
            alertView.AddTextView();

            alertView.ShowAlert(style, title, subTitle, "Cancel");
        }
    }
}
