using System;
using Android.Content;
using Android.Widget;
using Android.Views;

namespace SCLAlertView.Droid
{
	public abstract class BaseTopContentViewHolder : ITopContentViewHolder
	{
		private readonly Lazy<ViewGroup> lazyContentView;

		public BaseTopContentViewHolder(Context context, ViewGroup root)
		{
			this.Context = context;
			this.lazyContentView = new Lazy<ViewGroup>(() =>
			{
				return (ViewGroup)LayoutInflater.From(context).Inflate(ContentId, root);
			});
		}

		public ViewGroup ContentView => this.lazyContentView.Value;

		protected abstract int ContentId { get; }

		protected Context Context { get; }

		public virtual void OnStart()
		{

		}

		public virtual void OnPause()
		{

		}
	}
}