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
		//private ProgressHelper mProgressHelper;
		private FrameLayout mWarningFrame;
		private OnSweetClickListener mCancelClickListener;
		private OnSweetClickListener mConfirmClickListener;
		private bool mCloseFromCancel;

		public const int NORMAL_TYPE = 0;
		public const int ERROR_TYPE = 1;
		public const int SUCCESS_TYPE = 2;
		public const int WARNING_TYPE = 3;
		public const int CUSTOM_IMAGE_TYPE = 4;
		public const int PROGRESS_TYPE = 5;

		public interface OnSweetClickListener {
			void onClick (SweetAlertDialog sweetAlertDialog);
		}

		public SweetAlertDialog(Context context):base(context, NORMAL_TYPE) {
			
		}

		public SweetAlertDialog(Context context, int alertType):base(context,Resource.Style.alert_dialog) {
			SetCancelable(true);
			SetCanceledOnTouchOutside(false);
			//mProgressHelper = new ProgressHelper(context);
			mAlertType = alertType;
			var errorAnimSet = new AnimationSet(true);
			errorAnimSet.AddAnimation(new AlphaAnimation(0,1) { Duration = 400});
			errorAnimSet.AddAnimation(new Rotate3dAnimation(0,100,0,50,50) {Duration = 400});
			mErrorInAnim = errorAnimSet;

			// mErrorInAnim = AnimationUtils.LoadAnimation(context, Resource.Animation.error_frame_in);
			mErrorXInAnim =(AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.error_x_in);

			// 2.3.x system don't support alpha-animation on layer-list drawable
			// remove it from animation set
			if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1) {
				IList<Animation> childAnims = mErrorXInAnim.Animations;
				int idx;
				for (idx = 0; idx < childAnims.Count; idx++)
				{
					var childAnim = childAnims[idx];
					if(childAnim is AlphaAnimation)
					{
						break;
					}



				}

				if (idx < childAnims.Count) {
					childAnims.RemoveAt(idx);
				}
			}
			mSuccessBowAnim = AnimationUtils.LoadAnimation(context,  Resource.Animation.success_bow_roate);
			mSuccessLayoutAnimSet = (AnimationSet)AnimationUtils.LoadAnimation(context, Resource.Animation.success_mask_layout);
			mModalInAnim = (AnimationSet) AnimationUtils.LoadAnimation(context, Resource.Animation.modal_in);
			mModalOutAnim = (AnimationSet) AnimationUtils.LoadAnimation(context, Resource.Animation.modal_out);
			mModalOutAnim.AnimationEnd+=(s,e)=>
			{
				mDialogView.Visibility = ViewStates.Gone;
				mDialogView.Post(()=>
				{
					if (mCloseFromCancel) {
						this.Cancel();
					} else {
						this.Dismiss();
					}					
				});
			};
			      
			// dialog overlay fade out
			mOverlayOutAnim = new SimppleAnimation(this.Window);
			mOverlayOutAnim.Duration  =120;
		}

		public class SimppleAnimation : Animation
		{
			private readonly Window window;

			public SimppleAnimation(Window window)
	{
				this.window = window;
	}

		protected override void ApplyTransformation(float interpolatedTime, Transformation t)
			{
				base.ApplyTransformation(interpolatedTime, t);
				var wlp = window.Attributes;
				wlp.Alpha = 1 - interpolatedTime;
				window.Attributes  =wlp;
			}
		}

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
			//mProgressHelper.setProgressWheel((ProgressBar)FindViewById(Resource.Id.progressWheel));
			mConfirmButton.Click+=this.OnClick;
			mCancelButton.Click+=this.OnClick;

			setTitleText(mTitleText);
			setContentText(mContentText);
			setCancelText(mCancelText);
			setConfirmText(mConfirmText);
			changeAlertType(mAlertType, true);
		}

		private void restore () {
			mCustomImage.Visibility = ViewStates.Gone;
			mErrorFrame.Visibility = ViewStates.Gone;
			mSuccessFrame.Visibility = ViewStates.Gone;
			mWarningFrame.Visibility = ViewStates.Gone;
			mProgressFrame.Visibility = ViewStates.Gone;
			mConfirmButton.Visibility = ViewStates.Gone;

			mConfirmButton.SetBackgroundResource(Resource.Drawable.blue_button_background);
			mErrorFrame.ClearAnimation();
			mErrorX.ClearAnimation();
			mSuccessTick.ClearAnimation();
			mSuccessLeftMask.ClearAnimation();
			mSuccessRightMask.ClearAnimation();
		}

		private void playAnimation () {
			if (mAlertType == ERROR_TYPE) {
				mErrorFrame.StartAnimation(mErrorInAnim);
				mErrorX.StartAnimation(mErrorXInAnim);
			} else if (mAlertType == SUCCESS_TYPE) {
				mSuccessTick.startTickAnim();
				mSuccessRightMask.StartAnimation(mSuccessBowAnim);
			}
		}

		private void changeAlertType(int alertType, bool fromCreate) {
			mAlertType = alertType;
			// call after created views
			if (mDialogView != null) {
				if (!fromCreate) {
					// restore all of views state before switching alert type
					restore();
				}
				switch (mAlertType) {
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
				if (!fromCreate) {
					playAnimation();
				}
			}
		}

		public int getAlerType () {
			return mAlertType;
		}

		public void changeAlertType(int alertType) {
			changeAlertType(alertType, false);
		}


		public String getTitleText () {
			return mTitleText;
		}

		public SweetAlertDialog setTitleText (String text) {
			mTitleText = text;
			if (mTitleTextView != null && mTitleText != null) {
				mTitleTextView.Text = mTitleText;
			}
			return this;
		}

		public SweetAlertDialog setCustomImage (Drawable drawable) {
			mCustomImgDrawable = drawable;
			if (mCustomImage != null && mCustomImgDrawable != null) {
				mCustomImage.Visibility = ViewStates.Visible;
				mCustomImage.SetImageDrawable(mCustomImgDrawable);
			}
			return this;
		}

		public SweetAlertDialog setCustomImage (int resourceId) {
			return setCustomImage(this.Context.Resources.GetDrawable(resourceId));
		}

		public String getContentText () {
			return mContentText;
		}

		public SweetAlertDialog setContentText (String text) {
			mContentText = text;
			if (mContentTextView != null && mContentText != null) {
				showContentText(true);
				mContentTextView.Text = mContentText;
			}
			return this;
		}

		public bool isShowCancelButton () {
			return mShowCancel;
		}

		public SweetAlertDialog showCancelButton (bool isShow) {
			mShowCancel = isShow;
			if (mCancelButton != null) {
				mCancelButton.Visibility = mShowCancel ? ViewStates.Visible : ViewStates.Gone;
			}
			return this;
		}

		public bool isShowContentText () {
			return mShowContent;
		}

		public SweetAlertDialog showContentText (bool isShow) {
			mShowContent = isShow;
			if (mContentTextView != null) {
				mContentTextView.Visibility = mShowContent ? ViewStates.Visible : ViewStates.Gone;
			}
			return this;
		}

		public String getCancelText () {
			return mCancelText;
		}

		public SweetAlertDialog setCancelText (String text) {
			mCancelText = text;
			if (mCancelButton != null && mCancelText != null) {
				showCancelButton(true);
				mCancelButton.Text = mCancelText;
			}
			return this;
		}

		public String getConfirmText () {
			return mConfirmText;
		}

		public SweetAlertDialog setConfirmText (String text) {
			mConfirmText = text;
			if (mConfirmButton != null && mConfirmText != null) {
				mConfirmButton.Text = mConfirmText;
			}
			return this;
		}

		public SweetAlertDialog setCancelClickListener (OnSweetClickListener listener) {
			mCancelClickListener = listener;
			return this;
		}

		public SweetAlertDialog setConfirmClickListener (OnSweetClickListener listener) {
			mConfirmClickListener = listener;
			return this;
		}

		protected override void OnStart()
		{
			base.OnStart();
			mDialogView.StartAnimation(mModalInAnim);
			playAnimation();
		}

		/// <summary>
		/// The real Dialog.cancel() will be invoked async-ly after the animation finishes.
		/// </summary>
		public override void Cancel()
		{
			base.Cancel();
			dismissWithAnimation(true);
		}

		/**
     * The real Dialog.dismiss() will be invoked async-ly after the animation finishes.
     */
		public void dismissWithAnimation() {
			dismissWithAnimation(false);
		}

		private void dismissWithAnimation(bool fromCancel) {
			mCloseFromCancel = fromCancel;
			mConfirmButton.StartAnimation(mOverlayOutAnim);
			mDialogView.StartAnimation(mModalOutAnim);
		}

		protected void OnClick(object sender, EventArgs e)
		{
			var v = (Button)sender;
			if (v.Id == Resource.Id.cancel_button) {
				if (mCancelClickListener != null) {
					mCancelClickListener.onClick(this);
				} else {
					dismissWithAnimation();
				}
			} else if (v.Id == Resource.Id.confirm_button) {
				if (mConfirmClickListener != null) {
					mConfirmClickListener.onClick(this);
				} else {
					dismissWithAnimation();
				}
			}
		}
	

		//public ProgressHelper getProgressHelper () {
		//	return mProgressHelper;
		//}
	}
}
