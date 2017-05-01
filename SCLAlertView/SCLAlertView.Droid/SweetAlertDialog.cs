using System;
using Android.Views;
using Android.App;
using Android.Views.Animations;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Content;
using Android.OS;
using System.Collections.Generic;

namespace SCLAlertView.Droid
{
	/// <summary>
	/// https://github.com/pedant/sweet-alert-dialog
	/// </summary>
	public class SweetAlertDialog : Dialog
	{
		private View mDialogView;
		private AnimationSet mModalInAnim;
		private AnimationSet mModalOutAnim;
		private Animation mOverlayOutAnim;
		private Animation mErrorInAnim;
		private AnimationSet mErrorXInAnim;
		private AnimationSet mSuccessLayoutAnimSet;
		private Animation mSuccessBowAnim;
		private TextView mTitleTextView;
		private TextView mContentTextView;
		private String mTitleText;
		private String mContentText;
		private bool mShowCancel;
		private bool mShowContent;
		private String mCancelText;
		private String mConfirmText;
		private int mAlertType;
		private FrameLayout mErrorFrame;
		private FrameLayout mSuccessFrame;
		private FrameLayout mProgressFrame;
		private SuccessTickView mSuccessTick;
		private ImageView mErrorX;
		private View mSuccessLeftMask;
		private View mSuccessRightMask;
		private Drawable mCustomImgDrawable;
		private ImageView mCustomImage;
		private Button mConfirmButton;
		private Button mCancelButton;
		private FrameLayout mWarningFrame;
		private bool mCloseFromCancel;

		public event EventHandler CancelClick;

		public event EventHandler ConfirmClick;

		public const int NORMAL_TYPE = 0;
		public const int ERROR_TYPE = 1;
		public const int SUCCESS_TYPE = 2;
		public const int WARNING_TYPE = 3;
		public const int CUSTOM_IMAGE_TYPE = 4;
		public const int PROGRESS_TYPE = 5;

		public SweetAlertDialog(Context context) : base(context, NORMAL_TYPE)
		{

		}

		public SweetAlertDialog(Context context, int alertType) : base(context, Resource.Style.alert_dialog)
		{
			SetCancelable(true);
			SetCanceledOnTouchOutside(false);
			//mProgressHelper = new ProgressHelper(context);
			mAlertType = alertType;
			mErrorInAnim = this.CreateExitAnimation();
			// mErrorInAnim = AnimationUtils.LoadAnimation(context, Resource.Animation.error_frame_in);
			mErrorXInAnim = (AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.error_x_in);

			// 2.3.x system don't support alpha-animation on layer-list drawable
			// remove it from animation set
			if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
			{
				IList<Animation> childAnims = mErrorXInAnim.Animations;
				int idx;
				for (idx = 0; idx < childAnims.Count; idx++)
				{
					var childAnim = childAnims[idx];
					if (childAnim is AlphaAnimation)
					{
						break;
					}



				}

				if (idx < childAnims.Count)
				{
					childAnims.RemoveAt(idx);
				}
			}
			mSuccessBowAnim = AnimationUtils.LoadAnimation(context, Resource.Animation.success_bow_roate);
			mSuccessLayoutAnimSet = (AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.success_mask_layout);
			mModalInAnim = (AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.modal_in);
			mModalOutAnim = (AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.modal_out);
			mModalOutAnim.AnimationEnd += (s, e) =>
			  {
				  mDialogView.Visibility = ViewStates.Gone;
				  mDialogView.Post(() =>
				  {
					  if (mCloseFromCancel)
					  {
						  this.Cancel();
					  }
					  else
					  {
						  this.Dismiss();
					  }
				  });
			  };

			// dialog overlay fade out
			mOverlayOutAnim = new SelfAnimation(this.Window);
			mOverlayOutAnim.Duration = 120;
		}

		public string CancelText => this.mCancelText;

		public string ConfirmText => this.mConfirmText;

		public bool IsContentText => this.mShowContent;

		public string ContentText => this.mContentText;

		public bool IsShowCancelButton => this.mShowCancel;

		public int AlerType => this.mAlertType;

		public string TitleText => this.mTitleText;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.alert_dialog);
			mDialogView = this.Window.DecorView.FindViewById(Android.Resource.Id.Content);
			mTitleTextView = (TextView)FindViewById(Resource.Id.title_text);
			mContentTextView = (TextView)FindViewById(Resource.Id.content_text);
			mErrorFrame = (FrameLayout)FindViewById(Resource.Id.error_frame);
			mErrorX = (ImageView)mErrorFrame.FindViewById(Resource.Id.error_x);
			mSuccessFrame = (FrameLayout)FindViewById(Resource.Id.success_frame);
			mProgressFrame = (FrameLayout)FindViewById(Resource.Id.progress_dialog);
			mSuccessTick = (SuccessTickView)mSuccessFrame.FindViewById(Resource.Id.success_tick);
			mSuccessLeftMask = mSuccessFrame.FindViewById(Resource.Id.mask_left);
			mSuccessRightMask = mSuccessFrame.FindViewById(Resource.Id.mask_right);
			mCustomImage = (ImageView)FindViewById(Resource.Id.custom_image);
			mWarningFrame = (FrameLayout)FindViewById(Resource.Id.warning_frame);
			mConfirmButton = (Button)FindViewById(Resource.Id.confirm_button);
			mCancelButton = (Button)FindViewById(Resource.Id.cancel_button);
			mConfirmButton.Click += (s, e) =>
			{
				var confirmHandler = this.ConfirmClick;
				if (confirmHandler != null)
				{
					confirmHandler?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					this.DismissWithAnimation();
				}
			};

			mCancelButton.Click += (s, e) =>
			{
				var cancelHandler = this.CancelClick;
				if (cancelHandler != null)
				{
					cancelHandler?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					this.DismissWithAnimation();
				}
			};

			SetTitleText(mTitleText);
			SetContentText(mContentText);
			SetCancelText(mCancelText);
			SetConfirmText(mConfirmText);
			ChangeAlertType(mAlertType, true);
		}

		protected Animation CreateExitAnimation()
		{
			var exitAnimSet = new AnimationSet(true);
			exitAnimSet.AddAnimation(new AlphaAnimation(0, 1) { Duration = 400 });
			exitAnimSet.AddAnimation(new Rotate3dAnimation(0, 100, 0, 50, 50) { Duration = 400 });

			return exitAnimSet;
		}

		public void ChangeAlertType(int alertType)
		{
			this.ChangeAlertType(alertType, false);
		}

		public SweetAlertDialog SetTitleText(string text)
		{
			this.mTitleText = text;
			if (this.mTitleTextView != null && this.mTitleText != null)
			{
				this.mTitleTextView.Text = this.mTitleText;
			}

			return this;
		}

		public SweetAlertDialog setCustomImage(Drawable drawable)
		{
			mCustomImgDrawable = drawable;
			if (mCustomImage != null && mCustomImgDrawable != null)
			{
				mCustomImage.Visibility = ViewStates.Visible;
				mCustomImage.SetImageDrawable(mCustomImgDrawable);
			}
			return this;
		}

		public SweetAlertDialog SetCustomImage(int resourceId)
		{
			return setCustomImage(this.Context.Resources.GetDrawable(resourceId));
		}

		public SweetAlertDialog SetContentText(string text)
		{
			mContentText = text;
			if (mContentTextView != null && mContentText != null)
			{
				ShowContentText(true);
				mContentTextView.Text = mContentText;
			}
			return this;
		}

		public SweetAlertDialog ShowCancelButton(bool isShow)
		{
			mShowCancel = isShow;
			if (mCancelButton != null)
			{
				mCancelButton.Visibility = mShowCancel ? ViewStates.Visible : ViewStates.Gone;
			}
			return this;
		}

		public SweetAlertDialog ShowContentText(bool isShow)
		{
			mShowContent = isShow;
			if (mContentTextView != null)
			{
				mContentTextView.Visibility = mShowContent ? ViewStates.Visible : ViewStates.Gone;
			}
			return this;
		}

		public SweetAlertDialog SetCancelText(string text)
		{
			this.mCancelText = text;
			if (this.mCancelButton != null && this.mCancelText != null)
			{
				this.ShowCancelButton(true);
				this.mCancelButton.Text = mCancelText;
			}

			return this;
		}

		public SweetAlertDialog SetConfirmText(string text)
		{
			this.mConfirmText = text;
			if (this.mConfirmButton != null && this.mConfirmText != null)
			{
				this.mConfirmButton.Text = this.mConfirmText;
			}

			return this;
		}

		public SweetAlertDialog SetCancelClickListener(EventHandler handler)
		{
			this.CancelClick += handler;

			return this;
		}

		public SweetAlertDialog SetConfirmClickListener(EventHandler handler)
		{
			this.ConfirmClick += handler;

			return this;
		}

		protected override void OnStart()
		{
			base.OnStart();
			this.mDialogView.StartAnimation(mModalInAnim);
			this.PlayAnimation();
		}

		/// <summary>
		/// The real Dialog.cancel() will be invoked async-ly after the animation finishes.
		/// </summary>
		public override void Cancel()
		{
			base.Cancel();
			this.DismissWithAnimation(true);
		}

		/// <summary>
		/// The real Dialog.dismiss() will be invoked async-ly after the animation finishes.
		/// </summary>
		public void DismissWithAnimation()
		{
			this.DismissWithAnimation(false);
		}

		private void PlayAnimation()
		{
			if (mAlertType == ERROR_TYPE)
			{
				mErrorFrame.StartAnimation(mErrorInAnim);
				mErrorX.StartAnimation(mErrorXInAnim);
			}
			else if (mAlertType == SUCCESS_TYPE)
			{
				mSuccessTick.startTickAnim();
				mSuccessRightMask.StartAnimation(mSuccessBowAnim);
			}
		}

		private void ChangeAlertType(int alertType, bool fromCreate)
		{
			mAlertType = alertType;
			// call after created views
			if (mDialogView != null)
			{
				if (!fromCreate)
				{
					// restore all of views state before switching alert type
					Restore();
				}
				switch (mAlertType)
				{
					case ERROR_TYPE:
						mErrorFrame.Visibility = ViewStates.Visible;
						break;
					case SUCCESS_TYPE:
						mSuccessFrame.Visibility = ViewStates.Visible;
						// initial rotate layout of success mask
						mSuccessLeftMask.StartAnimation(mSuccessLayoutAnimSet.Animations[0]);
						mSuccessRightMask.StartAnimation(mSuccessLayoutAnimSet.Animations[1]);
						break;
					case WARNING_TYPE:
						mConfirmButton.SetBackgroundResource(Resource.Drawable.red_button_background);
						mWarningFrame.Visibility = ViewStates.Visible;
						break;
					case CUSTOM_IMAGE_TYPE:
						setCustomImage(mCustomImgDrawable);
						break;
					case PROGRESS_TYPE:
						mProgressFrame.Visibility = ViewStates.Visible;
						mConfirmButton.Visibility = ViewStates.Gone;
						break;
				}
				if (!fromCreate)
				{
					PlayAnimation();
				}
			}
		}


		private void Restore()
		{
			this.mCustomImage.Visibility = ViewStates.Gone;
			this.mErrorFrame.Visibility = ViewStates.Gone;
			this.mSuccessFrame.Visibility = ViewStates.Gone;
			this.mWarningFrame.Visibility = ViewStates.Gone;
			this.mProgressFrame.Visibility = ViewStates.Gone;
			this.mConfirmButton.Visibility = ViewStates.Gone;

			this.mConfirmButton.SetBackgroundResource(Resource.Drawable.blue_button_background);
			this.mErrorFrame.ClearAnimation();
			this.mErrorX.ClearAnimation();
			this.mSuccessTick.ClearAnimation();
			this.mSuccessLeftMask.ClearAnimation();
			this.mSuccessRightMask.ClearAnimation();
		}

		private void DismissWithAnimation(bool fromCancel)
		{
			this.mCloseFromCancel = fromCancel;
			this.mConfirmButton.StartAnimation(mOverlayOutAnim);
			this.mDialogView.StartAnimation(mModalOutAnim);
		}

		protected class SelfAnimation : Animation
		{
			private readonly Window window;

			public SelfAnimation(Window window)
			{
				this.window = window;
			}

			protected override void ApplyTransformation(float interpolatedTime, Transformation t)
			{
				base.ApplyTransformation(interpolatedTime, t);
				var wlp = this.window.Attributes;
				wlp.Alpha = 1 - interpolatedTime;
				this.window.Attributes = wlp;
			}
		}
	}
}