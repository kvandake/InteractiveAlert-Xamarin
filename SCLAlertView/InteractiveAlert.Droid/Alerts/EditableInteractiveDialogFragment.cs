using System;
using Android.Widget;
using Android.Views;
using Android.Runtime;

namespace InteractiveAlert.Droid
{
	public class EditableInteractiveDialogFragment : BaseInteractiveDialogFragment<EditableInteractiveAlertConfig>
	{
		protected EditableInteractiveDialogFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public EditableInteractiveDialogFragment()
		{

		}

		protected override bool OnSetBottomView(ViewGroup viewGroup)
		{
			var editText = new EditText(this.Context);
			editText.SetLines(this.Config.SingleLine ? 1 : 10);
			editText.SetSingleLine(this.Config.SingleLine);
			editText.Hint = this.Config.Placeholder;
			var lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
			editText.LayoutParameters = lp;
			editText.TextChanged += (s, e) =>
			{
				this.Config.Text = e.Text?.ToString();
			};

			viewGroup.AddView(editText);
			viewGroup.Invalidate();

			return true;
		}
	}
}