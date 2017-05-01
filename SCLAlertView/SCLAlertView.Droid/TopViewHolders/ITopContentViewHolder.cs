using System;
using Android.Widget;
using Android.Views;

namespace SCLAlertView.Droid
{
	public interface ITopContentViewHolder
	{
		ViewGroup ContentView { get; }

		void OnStart();

		void OnPause();
	}
}