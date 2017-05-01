using System;
using Android.Runtime;

namespace InteractiveAlert.Droid
{
	public class InteractiveDialogFragment : BaseInteractiveDialogFragment<InteractiveAlertConfig>
	{
		public InteractiveDialogFragment()
		{

		}

		protected InteractiveDialogFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}
	}
}