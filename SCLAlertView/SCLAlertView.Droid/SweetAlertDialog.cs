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
	public class SweetAlertDialog : Dialog , View.OnClickListener
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
		private ProgressHelper mProgressHelper;
		private FrameLayout mWarningFrame;
		private OnSweetClickListener mCancelClickListener;
		private OnSweetClickListener mConfirmClickListener;
		private bool mCloseFromCancel;

		public static readonly int NORMAL_TYPE = 0;
		public static readonly int ERROR_TYPE = 1;
		public static readonly int SUCCESS_TYPE = 2;
		public static readonly int WARNING_TYPE = 3;
		public static readonly int CUSTOM_IMAGE_TYPE = 4;
		public static readonly int PROGRESS_TYPE = 5;

		public interface OnSweetClickListener {
			void onClick (SweetAlertDialog sweetAlertDialog);
		}

		public SweetAlertDialog(Context context):base(context, NORMAL_TYPE) {
			
		}

		public SweetAlertDialog(Context context, int alertType):base(context,Resource.Style.alert_dialog) {
			SetCancelable(true);
			SetCanceledOnTouchOutside(false);
			mProgressHelper = new ProgressHelper(context);
			mAlertType = alertType;
			mErrorInAnim = OptAnimationLoader.loadAnimation(context, Resource.Animation.error_frame_in);
			mErrorXInAnim = (AnimationSet)OptAnimationLoader.loadAnimation(context, Resource.Animation.error_x_in);
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
			mSuccessBowAnim = OptAnimationLoader.loadAnimation(context,  Resource.Animation.success_bow_roate);
			mSuccessLayoutAnimSet = (AnimationSet)OptAnimationLoader.loadAnimation(context, Resource.Animation.success_mask_layout);
			mModalInAnim = (AnimationSet) OptAnimationLoader.loadAnimation(context, Resource.Animation.modal_in);
			mModalOutAnim = (AnimationSet) OptAnimationLoader.loadAnimation(context, Resource.Animation.modal_out);
			mModalOutAnim.AnimationEnd+=(s,e)=>
			{
				mDialogView.Visibility = ViewStates.Gone;
				mDialogView.Post(()=>
				{
					if (mCloseFromCancel) {
						SweetAlertDialog.cancel();
					} else {
						SweetAlertDialog.dismiss();
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
	

		protected void onCreate(Bundle savedInstanceState) {
			base.onCreate(savedInstanceState);
	
			SetContentView(R.layout.alert_dialog);

			mDialogView = this.Window.getDecorView().findViewById(android.R.id.content);
			mTitleTextView = (TextView)findViewById(R.id.title_text);
			mContentTextView = (TextView)findViewById(R.id.content_text);
			mErrorFrame = (FrameLayout)findViewById(R.id.error_frame);
			mErrorX = (ImageView)mErrorFrame.findViewById(R.id.error_x);
			mSuccessFrame = (FrameLayout)findViewById(R.id.success_frame);
			mProgressFrame = (FrameLayout)findViewById(R.id.progress_dialog);
			mSuccessTick = (SuccessTickView)mSuccessFrame.findViewById(R.id.success_tick);
			mSuccessLeftMask = mSuccessFrame.findViewById(R.id.mask_left);
			mSuccessRightMask = mSuccessFrame.findViewById(R.id.mask_right);
			mCustomImage = (ImageView)findViewById(R.id.custom_image);
			mWarningFrame = (FrameLayout)findViewById(R.id.warning_frame);
			mConfirmButton = (Button)findViewById(R.id.confirm_button);
			mCancelButton = (Button)findViewById(R.id.cancel_button);
			mProgressHelper.setProgressWheel((ProgressWheel)findViewById(R.id.progressWheel));
			mConfirmButton.setOnClickListener(this);
			mCancelButton.setOnClickListener(this);

			setTitleText(mTitleText);
			setContentText(mContentText);
			setCancelText(mCancelText);
			setConfirmText(mConfirmText);
			changeAlertType(mAlertType, true);

		}

		private void restore () {
			mCustomImage.setVisibility(View.GONE);
			mErrorFrame.setVisibility(View.GONE);
			mSuccessFrame.setVisibility(View.GONE);
			mWarningFrame.setVisibility(View.GONE);
			mProgressFrame.setVisibility(View.GONE);
			mConfirmButton.setVisibility(View.VISIBLE);

			mConfirmButton.setBackgroundResource(R.drawable.blue_button_background);
			mErrorFrame.clearAnimation();
			mErrorX.clearAnimation();
			mSuccessTick.clearAnimation();
			mSuccessLeftMask.clearAnimation();
			mSuccessRightMask.clearAnimation();
		}

		private void playAnimation () {
			if (mAlertType == ERROR_TYPE) {
				mErrorFrame.startAnimation(mErrorInAnim);
				mErrorX.startAnimation(mErrorXInAnim);
			} else if (mAlertType == SUCCESS_TYPE) {
				mSuccessTick.startTickAnim();
				mSuccessRightMask.startAnimation(mSuccessBowAnim);
			}
		}

		private void changeAlertType(int alertType, boolean fromCreate) {
			mAlertType = alertType;
			// call after created views
			if (mDialogView != null) {
				if (!fromCreate) {
					// restore all of views state before switching alert type
					restore();
				}
				switch (mAlertType) {
					case ERROR_TYPE:
						mErrorFrame.setVisibility(View.VISIBLE);
						break;
					case SUCCESS_TYPE:
						mSuccessFrame.setVisibility(View.VISIBLE);
						// initial rotate layout of success mask
						mSuccessLeftMask.startAnimation(mSuccessLayoutAnimSet.getAnimations().get(0));
						mSuccessRightMask.startAnimation(mSuccessLayoutAnimSet.getAnimations().get(1));
						break;
					case WARNING_TYPE:
						mConfirmButton.setBackgroundResource(R.drawable.red_button_background);
						mWarningFrame.setVisibility(View.VISIBLE);
						break;
					case CUSTOM_IMAGE_TYPE:
						setCustomImage(mCustomImgDrawable);
						break;
					case PROGRESS_TYPE:
						mProgressFrame.setVisibility(View.VISIBLE);
						mConfirmButton.setVisibility(View.GONE);
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
				mTitleTextView.setText(mTitleText);
			}
			return this;
		}

		public SweetAlertDialog setCustomImage (Drawable drawable) {
			mCustomImgDrawable = drawable;
			if (mCustomImage != null && mCustomImgDrawable != null) {
				mCustomImage.setVisibility(View.VISIBLE);
				mCustomImage.setImageDrawable(mCustomImgDrawable);
			}
			return this;
		}

		public SweetAlertDialog setCustomImage (int resourceId) {
			return setCustomImage(getContext().getResources().getDrawable(resourceId));
		}

		public String getContentText () {
			return mContentText;
		}

		public SweetAlertDialog setContentText (String text) {
			mContentText = text;
			if (mContentTextView != null && mContentText != null) {
				showContentText(true);
				mContentTextView.setText(mContentText);
			}
			return this;
		}

		public boolean isShowCancelButton () {
			return mShowCancel;
		}

		public SweetAlertDialog showCancelButton (boolean isShow) {
			mShowCancel = isShow;
			if (mCancelButton != null) {
				mCancelButton.setVisibility(mShowCancel ? View.VISIBLE : View.GONE);
			}
			return this;
		}

		public boolean isShowContentText () {
			return mShowContent;
		}

		public SweetAlertDialog showContentText (boolean isShow) {
			mShowContent = isShow;
			if (mContentTextView != null) {
				mContentTextView.setVisibility(mShowContent ? View.VISIBLE : View.GONE);
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
				mCancelButton.setText(mCancelText);
			}
			return this;
		}

		public String getConfirmText () {
			return mConfirmText;
		}

		public SweetAlertDialog setConfirmText (String text) {
			mConfirmText = text;
			if (mConfirmButton != null && mConfirmText != null) {
				mConfirmButton.setText(mConfirmText);
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

		protected void onStart() {
			mDialogView.startAnimation(mModalInAnim);
			playAnimation();
		}

		/**
     * The real Dialog.cancel() will be invoked async-ly after the animation finishes.
     */
		@Override
		public void cancel() {
			dismissWithAnimation(true);
		}

		/**
     * The real Dialog.dismiss() will be invoked async-ly after the animation finishes.
     */
		public void dismissWithAnimation() {
			dismissWithAnimation(false);
		}

		private void dismissWithAnimation(boolean fromCancel) {
			mCloseFromCancel = fromCancel;
			mConfirmButton.startAnimation(mOverlayOutAnim);
			mDialogView.startAnimation(mModalOutAnim);
		}

		@Override
		public void onClick(View v) {
			if (v.getId() == R.id.cancel_button) {
				if (mCancelClickListener != null) {
					mCancelClickListener.onClick(SweetAlertDialog.this);
				} else {
					dismissWithAnimation();
				}
			} else if (v.getId() == R.id.confirm_button) {
				if (mConfirmClickListener != null) {
					mConfirmClickListener.onClick(SweetAlertDialog.this);
				} else {
					dismissWithAnimation();
				}
			}
		}

		public ProgressHelper getProgressHelper () {
			return mProgressHelper;
		}
	}
}
