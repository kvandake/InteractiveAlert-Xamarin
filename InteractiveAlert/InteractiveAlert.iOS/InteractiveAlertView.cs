namespace InteractiveAlert
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CoreGraphics;
    using Foundation;
    using UIKit;

    // The Main Class
    // https://github.com/vikmeup/SCLAlertView-Swift/blob/master/SCLAlertView/SCLAlertView.swift#L309
    // https://github.com/dogo/SCLAlertView
    public class InteractiveAlertView : UIViewController
    {
        public static event EventHandler<InteractiveAlertViewEventArgs> Presented;
        
        public static event EventHandler<InteractiveAlertViewEventArgs> Closed;
        
        // Action Types
        public enum SCLActionType
        {
            None,
            Closure
        }

        // Animation Styles
        public enum SCLAnimationStyle
        {
            NoAnimation,
            TopToBottom,
            BottomToTop,
            LeftToRight,
            RightToLeft
        }

        private static readonly nfloat kCircleHeightBackground = 62.0f;

        private readonly SCLAppearance appearance;

        // DynamicAnimator function
        private UIDynamicAnimator animator;

        // Members declaration
        private readonly UIView baseView = new UIView();

        private readonly UIView circleBG = new UIView(new CGRect(0, 0, kCircleHeightBackground, kCircleHeightBackground));
        private UIView circleIconView;
        private readonly UIView circleView = new UIView();
        private readonly UIView contentView = new UIView();
        private Action dismissBlock;
        private double duration;
        private NSTimer durationStatusTimer;
        private NSTimer durationTimer;
        private bool keyboardHasBeenShown;

        private NSObject keyboardWillHideToken;

        private NSObject keyboardWillShowToken;
        private readonly UILabel labelTitle = new UILabel();

        private UISnapBehavior snapBehavior;
        private CGPoint? tmpCircleViewFrameOrigin;
        private CGPoint? tmpContentViewFrameOrigin;
        private readonly UITextView viewText = new UITextView();

        public InteractiveAlertView(SCLAppearance appearance)
            : base(null, null)
        {
            this.appearance = appearance;
            this.Setup();
        }

        public InteractiveAlertView(string nibNameOrNil, NSBundle bundle, SCLAppearance appearance) : base(nibNameOrNil, bundle)
        {
            this.appearance = appearance;
            this.Setup();
        }

        public InteractiveAlertView(string nibNameOrNil, NSBundle bundle) : this(nibNameOrNil, bundle, new SCLAppearance())
        {
        }

        public InteractiveAlertView() : this(new SCLAppearance())
        {
        }

        // UI Colour
        public UIColor ViewColor { get; set; }

        // UI Options
        public UIColor IconTintColor { get; set; }

        public UIView CustomSubview { get; set; }
        private List<UITextField> inputs { get; } = new List<UITextField>();
        private List<UITextView> input { get; } = new List<UITextView>();
        private List<SCLButton> buttons { get; } = new List<SCLButton>();

        private void Setup()
        {
            // Set up main view
            this.View.Frame = UIScreen.MainScreen.Bounds;
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            this.View.BackgroundColor = new UIColor(0, 0, 0, this.appearance.DefaultShadowOpacity);
            this.View.AddSubview(this.baseView);
            // Base View
            this.baseView.Frame = this.View.Frame;
            this.baseView.AddSubview(this.contentView);
            // Content View
            this.contentView.Layer.CornerRadius = this.appearance.ContentViewCornerRadius;
            this.contentView.Layer.MasksToBounds = true;
            this.contentView.Layer.BorderWidth = 0.5f;
            this.contentView.AddSubview(this.labelTitle);
            this.contentView.AddSubview(this.viewText);
            // Circle View
            this.circleBG.BackgroundColor = this.appearance.CircleBackgroundColor;
            this.circleBG.Layer.CornerRadius = this.circleBG.Frame.Size.Height / 2f;
            this.baseView.AddSubview(this.circleBG);
            this.circleBG.AddSubview(this.circleView);
            var x = (kCircleHeightBackground - this.appearance.CircleHeight) / 2f;
            this.circleView.Frame = new CGRect(x, x + this.appearance.CircleTopPosition, this.appearance.CircleHeight,
                this.appearance.CircleHeight);
            this.circleView.Layer.CornerRadius = this.circleView.Frame.Size.Height / 2f;
            // Title
            this.labelTitle.Lines = 0;
            this.labelTitle.TextAlignment = UITextAlignment.Center;
            this.labelTitle.Font = this.appearance.TitleFont;
            if (this.appearance.TitleMinimumScaleFactor < 1)
            {
                this.labelTitle.MinimumScaleFactor = this.appearance.TitleMinimumScaleFactor;
                this.labelTitle.AdjustsFontSizeToFitWidth = true;
            }
            this.labelTitle.Frame = new CGRect(12, this.appearance.TitleTop, this.appearance.WindowWidth - 24, this.appearance.TitleHeight);
            // View text
            this.viewText.Editable = false;
            this.viewText.TextAlignment = UITextAlignment.Center;
            this.viewText.TextContainerInset = UIEdgeInsets.Zero;
            this.viewText.TextContainer.LineFragmentPadding = 0;
            this.viewText.Font = this.appearance.TextFont;
            // Colours
            this.contentView.BackgroundColor = this.appearance.ContentViewColor;
            this.viewText.BackgroundColor = this.appearance.ContentViewColor;
            this.labelTitle.TextColor = this.appearance.TitleColor;
            this.viewText.TextColor = this.appearance.TitleColor;
            this.contentView.Layer.BorderColor = this.appearance.ContentViewBorderColor.CGColor;
            //Gesture Recognizer for tapping outside the textinput
            if (this.appearance.DisableTapGesture == false)
            {
                var tapGesture = new UITapGestureRecognizer(this.Tapped) {NumberOfTapsRequired = 1};
                this.View.AddGestureRecognizer(tapGesture);
            }
        }

        public void SetTitle(string title)
        {
            this.labelTitle.Text = title;
        }

        public void SetSubTitle(string subTitle)
        {
            this.viewText.Text = subTitle;
        }

        public void SetDismissBlock(Action dismissAction)
        {
            this.dismissBlock = dismissAction;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            var rv = UIApplication.SharedApplication.KeyWindow;
            var sz = rv.Frame.Size;
            var frame = rv.Frame;
            frame.Width = sz.Width;
            frame.Height = sz.Height;

            // Set background frame
            this.View.Frame = frame;
            nfloat hMargin = 12f;

            // get actual height of title text
            nfloat titleActualHeight = 0f;
            if (!string.IsNullOrEmpty(this.labelTitle.Text))
            {
                titleActualHeight = SCLAlertViewExtension.heightWithConstrainedWidth(this.labelTitle.Text,
                                        this.appearance.WindowWidth - hMargin * 2f, this.labelTitle.Font) + 10f;
                // get the larger height for the title text
                titleActualHeight = titleActualHeight > this.appearance.TitleHeight ? titleActualHeight : this.appearance.TitleHeight;
            }

            // computing the right size to use for the textView
            var maxHeight = sz.Height - 100f; // max overall height
            nfloat consumedHeight = 0f;
            consumedHeight += titleActualHeight > 0 ? this.appearance.TitleTop + titleActualHeight : hMargin;
            consumedHeight += 14;
            consumedHeight += this.appearance.ButtonHeight * this.buttons.Count;
            consumedHeight += this.appearance.TextFieldHeight * this.inputs.Count;
            consumedHeight += this.appearance.TextViewdHeight * this.input.Count;
            var maxViewTextHeight = maxHeight - consumedHeight;
            var viewTextWidth = this.appearance.WindowWidth - hMargin * 2f;
            nfloat viewTextHeight;

            // Check if there is a custom subview and add it over the textview
            if (this.CustomSubview != null)
            {
                viewTextHeight = (nfloat) Math.Min(this.CustomSubview.Frame.Height, maxViewTextHeight);
                this.viewText.Text = string.Empty;
                this.viewText.AddSubview(this.CustomSubview);
            }
            else
            {
                // computing the right size to use for the textView
                var suggestedViewTextSize = this.viewText.SizeThatFits(new CGSize(viewTextWidth, nfloat.MaxValue));
                viewTextHeight = (nfloat) Math.Min(suggestedViewTextSize.Height, maxViewTextHeight);
                // scroll management
                this.viewText.ScrollEnabled = suggestedViewTextSize.Height > maxViewTextHeight;
            }

            var windowHeight = consumedHeight + viewTextHeight;
            // Set frames
            var x = (sz.Width - this.appearance.WindowWidth) / 2f;
            var y = (sz.Height - windowHeight - this.appearance.CircleHeight / 8) / 2f;
            this.contentView.Frame = new CGRect(x, y, this.appearance.WindowWidth, windowHeight);
            this.contentView.Layer.CornerRadius = this.appearance.ContentViewCornerRadius;
            y -= kCircleHeightBackground * 0.6f;
            x = (sz.Width - kCircleHeightBackground) / 2f;
            this.circleBG.Frame = new CGRect(x, y + this.appearance.CircleBackgroundTopPosition, kCircleHeightBackground,
                kCircleHeightBackground);

            //adjust Title frame based on circularIcon show/hide flag
            var titleOffset = this.appearance.ShowCircularIcon ? 0.0f : -12.0f;
            this.labelTitle.Frame.Offset(0, titleOffset);

            // Subtitle
            y = titleActualHeight > 0f ? this.appearance.TitleTop + titleActualHeight + titleOffset : hMargin;
            this.viewText.Frame = new CGRect(hMargin, y, this.appearance.WindowWidth - hMargin * 2f, this.appearance.TextHeight);
            this.viewText.Frame = new CGRect(hMargin, y, viewTextWidth, viewTextHeight);
            // Text fields
            y += viewTextHeight + 14.0f;
            foreach (var txt in this.inputs)
            {
                txt.Frame = new CGRect(hMargin, y, this.appearance.WindowWidth - hMargin * 2, 30);
                txt.Layer.CornerRadius = this.appearance.FieldCornerRadius;
                y += this.appearance.TextFieldHeight;
            }
            foreach (var txt in this.input)
            {
                txt.Frame = new CGRect(hMargin, y, this.appearance.WindowWidth - hMargin * 2f, 70);
                //txt.layer.cornerRadius = fieldCornerRadius
                y += this.appearance.TextViewdHeight;
            }

            // Buttons
            foreach (var btn in this.buttons)
            {
                btn.Frame = new CGRect(hMargin, y, this.appearance.WindowWidth - hMargin * 2, 35);
                btn.Layer.CornerRadius = this.appearance.ButtonCornerRadius;
                y += this.appearance.ButtonHeight;
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.keyboardWillShowToken = UIKeyboard.Notifications.ObserveWillShow(this.KeyboardWillShow);
            this.keyboardWillHideToken = UIKeyboard.Notifications.ObserveWillHide(this.KeyboardWillHide);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            this.keyboardWillShowToken?.Dispose();
            this.keyboardWillHideToken?.Dispose();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            if (evt.TouchesForView(this.View)?.Count > 0)
            {
                this.View.EndEditing(true);
            }
        }

        public UITextField AddTextField(string title)
        {
            // Update view height
            this.appearance.SetWindowHeight(this.appearance.WindowHeight + this.appearance.TextFieldHeight);
            // Add text field
            var txt = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Font = this.appearance.TextFont,
                AutocapitalizationType = UITextAutocapitalizationType.Words,
                ClearButtonMode = UITextFieldViewMode.WhileEditing
            };
            txt.Layer.MasksToBounds = true;
            txt.Layer.BorderWidth = 1.0f;
            if (!string.IsNullOrEmpty(title))
            {
                txt.Placeholder = title;
            }
            this.contentView.AddSubview(txt);
            this.inputs.Add(txt);
            return txt;
        }

        public UITextView AddTextView()
        {
            // Update view height
            this.appearance.SetWindowHeight(this.appearance.WindowHeight + this.appearance.TextViewdHeight);
            // Add text view
            var txt = new UITextView {Font = this.appearance.TextFont};
            // No placeholder with UITextView but you can use KMPlaceholderTextView library 
            //txt.autocapitalizationType = UITextAutocapitalizationType.Words
            //txt.clearButtonMode = UITextFieldViewMode.WhileEditing
            txt.Layer.MasksToBounds = true;
            txt.Layer.BorderWidth = 1.0f;
            txt.Layer.CornerRadius = 4f;
            this.contentView.AddSubview(txt);
            this.input.Add(txt);
            return txt;
        }

        public SCLButton AddButton(string title, Action action, UIColor backgroundColor = null, UIColor textColor = null,
            bool showDurationStatus = false)
        {
            var btn = this.AddButton(title, backgroundColor, textColor, showDurationStatus);
            btn.ActionType = SCLActionType.Closure;
            btn.Action = action;
            btn.AddTarget(this.ButtonTapped, UIControlEvent.TouchUpInside);
            btn.AddTarget(this.ButtonTapDown, UIControlEvent.TouchDown | UIControlEvent.TouchDragEnter);
            btn.AddTarget(this.ButtonRelease,
                UIControlEvent.TouchUpInside |
                UIControlEvent.TouchUpOutside |
                UIControlEvent.TouchCancel |
                UIControlEvent.TouchDragOutside);

            return btn;
        }

        public SCLButton AddButton(string title, UIColor backgroundColor = null, UIColor textColor = null, bool showDurationStatus = false)
        {
            // Update view height
            this.appearance.SetWindowHeight(this.appearance.WindowHeight + this.appearance.ButtonHeight);
            // Add button
            var btn = new SCLButton();
            btn.Layer.MasksToBounds = true;
            btn.SetTitle(title, UIControlState.Normal);
            btn.TitleLabel.Font = this.appearance.ButtonFont;
            btn.CustomBackgroundColor = backgroundColor;
            btn.CustomTextColor = textColor;
            btn.InitialTitle = title;
            btn.ShowDurationStatus = showDurationStatus;
            this.contentView.AddSubview(btn);
            this.buttons.Add(btn);

            return btn;
        }

        private void ButtonTapped(object sender, EventArgs args)
        {
            var btn = (SCLButton) sender;
            if (btn.ActionType == SCLActionType.Closure)
            {
                btn.Action?.Invoke();
            }
            else
            {
                Console.WriteLine("Unknow action type for button");
            }

            if (this.View.Alpha != 0 && this.appearance.ShouldAutoDismiss)
            {
                this.HideView();
            }
        }

        private void ButtonTapDown(object sender, EventArgs args)
        {
            var btn = (SCLButton) sender;
            nfloat hue = 0f;
            nfloat saturation = 0;
            nfloat brightness = 0;
            nfloat alpha = 0;
            nfloat pressBrightnessFactor = 0.85f;
            btn.BackgroundColor?.GetHSBA(out hue, out saturation, out brightness, out alpha);
            brightness = brightness * pressBrightnessFactor;
            btn.BackgroundColor = UIColor.FromHSBA(hue, saturation, brightness, alpha);
        }

        private void ButtonRelease(object sender, EventArgs args)
        {
            var btn = (SCLButton) sender;
            btn.BackgroundColor = btn.CustomBackgroundColor ?? this.ViewColor ?? btn.BackgroundColor;
        }

        private void KeyboardWillShow(object sender, UIKeyboardEventArgs args)
        {
            if (this.keyboardHasBeenShown)
            {
                return;
            }

            this.keyboardHasBeenShown = true;
            var endKeyBoardFrame = args.FrameEnd.GetMinY();

            if (this.tmpContentViewFrameOrigin == null)
            {
                this.tmpContentViewFrameOrigin = this.contentView.Frame.Location;
            }

            if (this.tmpCircleViewFrameOrigin == null)
            {
                // todo location replace origin 
                this.tmpCircleViewFrameOrigin = this.circleBG.Frame.Location;
            }

            var newContentViewFrameY = this.contentView.Frame.GetMaxY() - endKeyBoardFrame;
            if (newContentViewFrameY < 0)
            {
                newContentViewFrameY = 0;
            }

            var newBallViewFrameY = this.circleBG.Frame.Y - newContentViewFrameY;
            UIView.AnimateNotify(args.AnimationDuration, 0, ConvertToAnimationOptions(args.AnimationCurve), () =>
            {
                var contentViewFrame = this.contentView.Frame;
                contentViewFrame.Y -= newContentViewFrameY;
                this.contentView.Frame = contentViewFrame;

                var circleBGFrame = this.circleBG.Frame;
                circleBGFrame.Y = newBallViewFrameY;
                this.circleBG.Frame = circleBGFrame;
            }, null);
        }

        private void KeyboardWillHide(object sender, UIKeyboardEventArgs args)
        {
            if (this.keyboardHasBeenShown)
            {
                UIView.AnimateNotify(args.AnimationDuration, 0, ConvertToAnimationOptions(args.AnimationCurve), () =>
                {
                    //This could happen on the simulator (keyboard will be hidden)
                    if (this.tmpContentViewFrameOrigin.HasValue)
                    {
                        var contentViewFrame = this.contentView.Frame;
                        contentViewFrame.Y = this.tmpContentViewFrameOrigin.Value.Y;
                        this.contentView.Frame = contentViewFrame;
                        this.tmpContentViewFrameOrigin = null;
                    }
                    if (this.tmpCircleViewFrameOrigin.HasValue)
                    {
                        var circleBGFrame = this.circleBG.Frame;
                        circleBGFrame.Y = this.tmpCircleViewFrameOrigin.Value.Y;
                        this.circleBG.Frame = circleBGFrame;
                        this.tmpCircleViewFrameOrigin = null;
                    }
                }, null);
            }

            this.keyboardHasBeenShown = false;
        }

        //Dismiss keyboard when tapped outside textfield & close SCLAlertView when hideWhenBackgroundViewIsTapped
        private void Tapped(UITapGestureRecognizer gestureRecognizer)
        {
            this.View.EndEditing(true);
            if (gestureRecognizer.View.HitTest(gestureRecognizer.LocationInView(gestureRecognizer.View), null) == this.baseView &&
                this.appearance.HideWhenBackgroundViewIsTapped)
            {
                this.HideView();
            }
        }

        public SCLAlertViewResponder ShowCustom(string title,
            string subTitle,
            UIColor color,
            UIImage icon,
            string closeButtonTitle = null,
            double duration = 0.0,
            UIColor colorStyle = null,
            UIColor colorTextButton = null,
            InteractiveAlertStyle style = InteractiveAlertStyle.Success,
            SCLAnimationStyle animationStyle = SCLAnimationStyle.TopToBottom)
        {
            colorStyle = colorStyle ?? GetDefaultColorStyle(style);
            colorTextButton = colorTextButton ?? GetDefaultColorTextButton(style) ?? UIColor.White;
            return this.ShowTitle(title, subTitle, duration, closeButtonTitle, style, color, colorTextButton, icon, animationStyle);
        }

        public SCLAlertViewResponder ShowAlert(InteractiveAlertStyle style,
            string title,
            string subTitle,
            string closeButtonTitle = null,
            double duration = 0.0,
            UIColor colorStyle = null,
            UIColor colorTextButton = null,
            UIImage circleIconImage = null,
            SCLAnimationStyle animationStyle = SCLAnimationStyle.TopToBottom)
        {
            colorStyle = colorStyle ?? GetDefaultColorStyle(style);
            colorTextButton = colorTextButton ?? GetDefaultColorTextButton(style) ?? UIColor.White;

            return this.ShowTitle(title, subTitle, duration, closeButtonTitle, style, colorStyle, colorTextButton, circleIconImage,
                animationStyle);
        }

        public SCLAlertViewResponder ShowTitle(string title,
            string subTitle,
            double duration,
            string completeText,
            InteractiveAlertStyle style,
            UIColor colorStyle = null,
            UIColor colorTextButton = null,
            UIImage circleIconImage = null,
            SCLAnimationStyle animationStyle = SCLAnimationStyle.TopToBottom)
        {
            colorStyle = colorStyle ?? UIColor.Black;
            colorTextButton = colorTextButton ?? UIColor.White;
            this.View.Alpha = 0;
            var rv = UIApplication.SharedApplication.KeyWindow;
            rv.AddSubview(this.View);
            this.View.Frame = rv.Bounds;
            this.baseView.Frame = rv.Bounds;

            // Alert colour/icon
            UIImage iconImage = null;
            // Icon style
            switch (style)
            {
                case InteractiveAlertStyle.Success:
                    iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.ImageOfCheckmark());
                    break;
                case InteractiveAlertStyle.Error:
                    iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.ImageOfCross());
                    break;
                //case InteractiveAlertStyle.Notice:
                //iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.ImageOfNotice());
                //break;
                case InteractiveAlertStyle.Warning:
                    iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.ImageOfWarning());
                    break;
                //case InteractiveAlertStyle.Info:
                //iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.imageOfInfo());
                //break;
                case InteractiveAlertStyle.Edit:
                    iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.ImageOfEdit());
                    break;
                case InteractiveAlertStyle.Wait:
                    iconImage = null;
                    break;
                //case InteractiveAlertStyle.Question:
                //iconImage = checkCircleIconImage(circleIconImage, SCLAlertViewStyleKit.imageOfQuestion());
                //break;
            }

            // Title
            if (!string.IsNullOrEmpty(title))
            {
                this.labelTitle.Text = title;
                var actualHeight =
                    SCLAlertViewExtension.heightWithConstrainedWidth(title, this.appearance.WindowWidth - 24, this.labelTitle.Font);
                this.labelTitle.Frame = new CGRect(12, this.appearance.TitleTop, this.appearance.WindowWidth - 24, actualHeight);
            }

            // Subtitle
            if (!string.IsNullOrEmpty(subTitle))
            {
                this.viewText.Text = subTitle;
                // Adjust text view size, if necessary
                var str = new NSString(subTitle);
                var font = this.viewText.Font;
                var attr = new UIStringAttributes {Font = this.viewText.Font};
                var sz = new CGSize(this.appearance.WindowWidth - 24, 90);
                var r = str.GetBoundingRect(sz, NSStringDrawingOptions.UsesLineFragmentOrigin, attr, null);
                var ht = (nfloat) Math.Ceiling(r.Size.Height);
                if (ht < this.appearance.TextHeight)
                {
                    this.appearance.WindowHeight -= this.appearance.TextHeight - ht;
                    this.appearance.SetTextHeight(ht);
                }
            }

            if

                // Done button
                (this.appearance.ShowCloseButton)
            {
                title = completeText ?? "Done";
                this.AddButton(title, this.HideView);
            }

            //hidden/show circular view based on the ui option
            this.circleView.Hidden = !this.appearance.ShowCircularIcon;
            this.circleBG.Hidden = !this.appearance.ShowCircularIcon;

            // Alert view colour and images
            this.circleView.BackgroundColor = colorStyle;
            // Spinner / icon
            if (style == InteractiveAlertStyle.Wait)
            {
                var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
                indicator.StartAnimating();
                this.circleIconView = indicator;
            }
            else
            {
                if (this.IconTintColor != null)
                {
                    this.circleIconView = new UIImageView(iconImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate));
                    this.circleIconView.TintColor = this.IconTintColor;
                }
                else
                {
                    this.circleIconView = new UIImageView(iconImage);
                }
            }

            this.circleView.AddSubview(this.circleIconView);
            var x = (this.appearance.CircleHeight - this.appearance.CircleIconHeight) / 2f;
            this.circleIconView.Frame = new CGRect(x, x, this.appearance.CircleIconHeight, this.appearance.CircleIconHeight);
            this.circleIconView.Layer.CornerRadius = this.circleIconView.Bounds.Height / 2f;
            this.circleIconView.Layer.MasksToBounds = true;

            foreach (var txt in this.inputs)
            {
                txt.Layer.BorderColor = colorStyle.CGColor;
            }

            foreach (var txt in this.input)
            {
                txt.Layer.BorderColor = colorStyle.CGColor;
            }

            foreach (var btn in this.buttons)
            {
                btn.BackgroundColor = btn.CustomBackgroundColor ?? colorStyle;
                btn.SetTitleColor(btn.CustomTextColor ?? colorTextButton ?? UIColor.White, UIControlState.Normal);
            }

            // Adding duration
            if (duration > 0)
            {
                this.duration = duration;
                this.durationTimer?.Invalidate();
                this.durationTimer = NSTimer.CreateScheduledTimer(this.duration, false, obj => { this.HideView(); });
                this.durationStatusTimer?.Invalidate();
                this.durationStatusTimer = NSTimer.CreateScheduledTimer(1.0d, true, obj => { this.UpdateDurationStatus(); });
            }

            // Animate in the alert view
            this.ShowAnimation(animationStyle);

            OnPresented(this, new InteractiveAlertViewEventArgs(this));

            // Chainable objects
            return new SCLAlertViewResponder(this);
        }

        // Show animation in the alert view
        private void ShowAnimation(
            SCLAnimationStyle animationStyle = SCLAnimationStyle.TopToBottom,
            float animationStartOffset = -400.0f,
            float boundingAnimationOffset = 15.0f,
            double animationDuration = 0.2f)
        {
            var rv = UIApplication.SharedApplication.KeyWindow;
            var animationStartOrigin = this.baseView.Frame;
            var animationCenter = rv.Center;
            switch (animationStyle)
            {
                case SCLAnimationStyle.NoAnimation:
                    this.View.Alpha = 1.0f;
                    break;
                case SCLAnimationStyle.TopToBottom:
                    animationStartOrigin.Location = new CGPoint(animationStartOrigin.X, this.baseView.Frame.Y + animationStartOffset);
                    animationCenter = new CGPoint(animationCenter.X, animationCenter.Y + boundingAnimationOffset);
                    break;
                case SCLAnimationStyle.BottomToTop:
                    animationStartOrigin.Location = new CGPoint(animationStartOrigin.X, this.baseView.Frame.Y - animationStartOffset);
                    animationCenter = new CGPoint(animationCenter.X, animationCenter.Y - boundingAnimationOffset);
                    break;
                case SCLAnimationStyle.LeftToRight:
                    animationStartOrigin.Location = new CGPoint(this.baseView.Frame.X + animationStartOffset, animationStartOrigin.Y);
                    animationCenter = new CGPoint(animationCenter.X + boundingAnimationOffset, animationCenter.Y);
                    break;
                case SCLAnimationStyle.RightToLeft:
                    animationStartOrigin.Location = new CGPoint(this.baseView.Frame.X - animationStartOffset, animationStartOrigin.Y);
                    animationCenter = new CGPoint(animationCenter.X - boundingAnimationOffset, animationCenter.Y);
                    break;
            }

            var baseViewFrame = animationStartOrigin;
            this.baseView.Frame = baseViewFrame;

            if (this.appearance.DynamicAnimatorActive)
            {
                UIView.AnimateNotify(animationDuration, () => { this.View.Alpha = 1; }, null);

                this.Animate(this.baseView, rv.Center);
            }
            else
            {
                UIView.AnimateNotify(animationDuration, () =>
                {
                    this.View.Alpha = 1;
                    this.baseView.Center = animationCenter;
                }, completion =>
                {
                    UIView.AnimateNotify(animationDuration, () =>
                    {
                        this.View.Alpha = 1;
                        this.baseView.Center = rv.Center;
                    }, null);
                });
            }
        }

        private void Animate(UIView item, CGPoint center)
        {
            if (this.snapBehavior != null)
            {
                this.animator?.RemoveBehavior(this.snapBehavior);
            }

            this.animator = new UIDynamicAnimator(this.View);
            var tempSnapBehavior = new UISnapBehavior(item, center);
            this.animator?.AddBehavior(tempSnapBehavior);
            this.snapBehavior = tempSnapBehavior;
        }

        private void UpdateDurationStatus()
        {
            this.duration = this.duration - 1;
            foreach (var btn in this.buttons.Where(x => x.ShowDurationStatus))
            {
                var txt = $"{btn.InitialTitle} {this.duration}";
                btn.SetTitle(txt, UIControlState.Normal);
            }
        }

        // Close SCLAlertView
        public void HideView()
        {
            UIView.AnimateNotify(0.2, () => { this.View.Alpha = 0; }, completion =>
            {
                //Stop durationTimer so alertView does not attempt to hide itself and fire it's dimiss block a second time when close button is tapped
                this.durationTimer?.Invalidate();
                // Stop StatusTimer
                this.durationStatusTimer?.Invalidate();
                // Call completion handler when the alert is dismissed
                this.dismissBlock?.Invoke();

                // This is necessary for SCLAlertView to be de-initialized, preventing a strong reference cycle with the viewcontroller calling SCLAlertView.
                foreach (var button in this.buttons)
                {
                    button.Action = null;
                }

                this.View.RemoveFromSuperview();
                OnClosed(this, new InteractiveAlertViewEventArgs(this));
            });
        }

        protected static UIImage checkCircleIconImage(UIImage circleIconImage, UIImage defaultImage)
        {
            return circleIconImage ?? defaultImage;
        }

        private static UIViewAnimationOptions ConvertToAnimationOptions(UIViewAnimationCurve curve)
        {
            // Looks like a hack. But it is correct.
            // UIViewAnimationCurve and UIViewAnimationOptions are shifted by 16 bits
            // http://stackoverflow.com/questions/18870447/how-to-use-the-default-ios7-uianimation-curve/18873820#18873820
            return (UIViewAnimationOptions) ((int) curve << 16);
        }

        private static UIColor GetDefaultColorTextButton(InteractiveAlertStyle style)
        {
            switch (style)
            {
                case InteractiveAlertStyle.Success:
                case InteractiveAlertStyle.Error:
                //case InteractiveAlertStyle.Notice:
                //case InteractiveAlertStyle.Info:
                case InteractiveAlertStyle.Wait:
                case InteractiveAlertStyle.Edit:
                    //case InteractiveAlertStyle.Question:
                    return UIColor.White;
                case InteractiveAlertStyle.Warning:
                    return UIColor.Black;
                default:
                    return UIColor.White;
            }
        }

        private static UIColor GetDefaultColorStyle(InteractiveAlertStyle style)
        {
            switch (style)
            {
                case InteractiveAlertStyle.Success:
                    // 0x22B573
                    return UIColor.FromRGB(34, 181, 115);
                case InteractiveAlertStyle.Error:
                    // 0xC1272D
                    return UIColor.FromRGB(193, 39, 45);
                //case InteractiveAlertStyle.Notice:
                //// 0x727375
                //return UIColor.FromRGB(114, 115, 117);
                case InteractiveAlertStyle.Warning:
                    // 0xFFD110
                    return UIColor.FromRGB(255, 209, 16);
                //case InteractiveAlertStyle.Info:
                //// 0x2866BF
                //return UIColor.FromRGB(40, 102, 191);
                case InteractiveAlertStyle.Edit:
                    // 0xA429FF
                    return UIColor.FromRGB(164, 41, 255);
                case InteractiveAlertStyle.Wait:
                    // 0xD62DA5
                    return UIColor.FromRGB(204, 45, 165);
                //case InteractiveAlertStyle.Question:
                //// 0x727375
                //return UIColor.FromRGB(114, 115, 117);
                default:
                    return UIColor.White;
            }
        }

        public class SCLAppearance
        {
            public nfloat DefaultShadowOpacity { get; set; } = 0.7f;
            public nfloat CircleTopPosition { get; set; } = 0.0f;
            public nfloat CircleBackgroundTopPosition { get; set; } = 6.0f;
            public nfloat CircleHeight { get; set; } = 56.0f;
            public nfloat CircleIconHeight { get; set; } = 20.0f;
            public nfloat TitleTop { get; set; } = 30.0f;
            public nfloat TitleHeight { get; set; } = 25.0f;
            public nfloat TitleMinimumScaleFactor { get; set; } = 1.0f;
            public nfloat WindowWidth { get; set; } = 240.0f;
            public nfloat WindowHeight { get; set; } = 178.0f;
            public nfloat TextHeight { get; set; } = 90.0f;
            public nfloat TextFieldHeight { get; set; } = 45.0f;
            public nfloat TextViewdHeight { get; set; } = 80.0f;
            public nfloat ButtonHeight { get; set; } = 45.0f;
            public UIColor CircleBackgroundColor { get; set; } = UIColor.White;

            public UIColor ContentViewColor { get; set; } = UIColor.White;

            // 0xCCCCCC
            public UIColor ContentViewBorderColor { get; set; } = UIColor.FromRGB(204, 204, 204);

            // 0x4D4D4D
            public UIColor TitleColor { get; set; } = UIColor.FromRGB(77, 77, 77);

            // Fonts
            public UIFont TitleFont { get; set; } = UIFont.SystemFontOfSize(20);

            public UIFont TextFont { get; set; } = UIFont.SystemFontOfSize(14);
            public UIFont ButtonFont { get; set; } = UIFont.SystemFontOfSize(14);

            // UI Options
            public bool DisableTapGesture { get; set; }

            public bool ShowCloseButton { get; set; } = true;

            public bool ShowCircularIcon { get; set; } = true;

            // Set this false to 'Disable' Auto hideView when SCLButton is tapped
            public bool ShouldAutoDismiss { get; set; } = true;

            public nfloat ContentViewCornerRadius { get; set; } = 5.0f;
            public nfloat FieldCornerRadius { get; set; } = 3.0f;
            public nfloat ButtonCornerRadius { get; set; } = 3.0f;
            public bool DynamicAnimatorActive { get; set; } = false;

            // Actions
            public bool HideWhenBackgroundViewIsTapped { get; set; }

            public void SetWindowHeight(nfloat kWindowHeight)
            {
                this.WindowHeight = kWindowHeight;
            }

            public void SetTextHeight(nfloat kTextHeight)
            {
                this.TextHeight = kTextHeight;
            }
        }

        // Button sub-class 
        public class SCLButton : UIButton
        {
            public SCLButton() : base(CGRect.Empty)
            {
            }

            public SCLButton(CGRect rect) : base(rect)
            {
            }

            public SCLActionType ActionType { get; set; } = SCLActionType.None;
            public UIColor CustomBackgroundColor { get; set; }
            public UIColor CustomTextColor { get; set; }
            public string InitialTitle { get; set; }
            public bool ShowDurationStatus { get; set; }
            public Action Action { get; set; }
        }

        protected static class SCLAlertViewExtension
        {
            public static nfloat heightWithConstrainedWidth(string text, nfloat width, UIFont font)
            {
                var constraintRect = new CGSize(width, nfloat.MaxValue);
                var boundingBox = new NSString(text).GetBoundingRect(constraintRect, NSStringDrawingOptions.UsesLineFragmentOrigin,
                    new UIStringAttributes {Font = font}, null);

                return boundingBox.Height;
            }
        }

        // ------------------------------------
        // Icon drawing
        // Code generated by PaintCode
        // ------------------------------------
        protected class SCLAlertViewStyleKit : NSObject
        {
            public static UIImage ImageOfCheckmark()
            {
                return RendererandCacheImage(drawCheckmark, ref Cache.imageOfCheckmarkImage);
            }

            public static UIImage ImageOfCross()
            {
                return RendererandCacheImage(drawCross, ref Cache.imageOfCrossImage);
            }

            public static UIImage ImageOfNotice()
            {
                return RendererandCacheImage(drawNotice, ref Cache.imageOfNoticeImage);
            }

            public static UIImage ImageOfWarning()
            {
                return RendererandCacheImage(drawWarning, ref Cache.imageOfWarningImage);
            }

            public static UIImage ImageOfInfo()
            {
                return RendererandCacheImage(drawInfo, ref Cache.imageOfInfoImage);
            }

            public static UIImage ImageOfEdit()
            {
                return RendererandCacheImage(drawEdit, ref Cache.imageOfEditImage);
            }

            public static UIImage ImageOfQuestion()
            {
                return RendererandCacheImage(drawQuestion, ref Cache.imageOfQuestionImage);
            }

            private static UIImage RendererandCacheImage(Action rendererAction, ref UIImage image)
            {
                if (image != null)
                {
                    return image;
                }

                UIGraphics.BeginImageContextWithOptions(new CGSize(80, 80), false, 0);
                rendererAction.Invoke();
                image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return image;
            }

            // Drawing Methods
            private static void drawCheckmark()
            {
                // Checkmark Shape Drawing
                var checkmarkShapePath = new UIBezierPath();
                checkmarkShapePath.MoveTo(new CGPoint(73.25, 14.05));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(64.51, 13.86), new CGPoint(70.98, 11.44), new CGPoint(66.78, 11.26));
                checkmarkShapePath.AddLineTo(new CGPoint(27.46, 52));
                checkmarkShapePath.AddLineTo(new CGPoint(15.75, 39.54));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(6.84, 39.54), new CGPoint(13.48, 36.93), new CGPoint(9.28, 36.93));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(6.84, 49.02), new CGPoint(4.39, 42.14), new CGPoint(4.39, 46.42));
                checkmarkShapePath.AddLineTo(new CGPoint(22.91, 66.14));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(27.28, 68), new CGPoint(24.14, 67.44), new CGPoint(25.71, 68));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(31.65, 66.14), new CGPoint(28.86, 68), new CGPoint(30.43, 67.26));
                checkmarkShapePath.AddLineTo(new CGPoint(73.08, 23.35));
                checkmarkShapePath.AddCurveToPoint(new CGPoint(73.25, 14.05), new CGPoint(75.52, 20.75), new CGPoint(75.7, 16.65));
                checkmarkShapePath.ClosePath();
                checkmarkShapePath.MiterLimit = 4;

                UIColor.White.SetFill();
                checkmarkShapePath.Fill();
            }

            private static void drawCross()
            {
                // Cross Shape Drawing
                var crossShapePath = new UIBezierPath();
                crossShapePath.MoveTo(new CGPoint(10, 70));
                crossShapePath.AddLineTo(new CGPoint(70, 10));
                crossShapePath.MoveTo(new CGPoint(10, 10));
                crossShapePath.AddLineTo(new CGPoint(70, 70));
                crossShapePath.LineCapStyle = CGLineCap.Round;
                crossShapePath.LineJoinStyle = CGLineJoin.Round;
                UIColor.White.SetStroke();
                crossShapePath.LineWidth = 14;
                crossShapePath.Stroke();
            }

            private static void drawNotice()
            {
                // Notice Shape Drawing
                var noticeShapePath = new UIBezierPath();
                noticeShapePath.MoveTo(new CGPoint(72, 48.54));
                noticeShapePath.AddLineTo(new CGPoint(72, 39.9));
                noticeShapePath.AddCurveToPoint(new CGPoint(66.38, 34.01), new CGPoint(72, 36.76), new CGPoint(69.48, 34.01));
                noticeShapePath.AddCurveToPoint(new CGPoint(61.53, 35.97), new CGPoint(64.82, 34.01), new CGPoint(62.69, 34.8));
                noticeShapePath.AddCurveToPoint(new CGPoint(60.36, 35.78), new CGPoint(61.33, 35.97), new CGPoint(62.3, 35.78));
                noticeShapePath.AddLineTo(new CGPoint(60.36, 33.22));
                noticeShapePath.AddCurveToPoint(new CGPoint(54.16, 26.16), new CGPoint(60.36, 29.3), new CGPoint(57.65, 26.16));
                noticeShapePath.AddCurveToPoint(new CGPoint(48.73, 29.89), new CGPoint(51.64, 26.16), new CGPoint(50.67, 27.73));
                noticeShapePath.AddLineTo(new CGPoint(48.73, 28.71));
                noticeShapePath.AddCurveToPoint(new CGPoint(43.49, 21.64), new CGPoint(48.73, 24.78), new CGPoint(46.98, 21.64));
                noticeShapePath.AddCurveToPoint(new CGPoint(39.03, 25.37), new CGPoint(40.97, 21.64), new CGPoint(39.03, 23.01));
                noticeShapePath.AddLineTo(new CGPoint(39.03, 9.07));
                noticeShapePath.AddCurveToPoint(new CGPoint(32.24, 2), new CGPoint(39.03, 5.14), new CGPoint(35.73, 2));
                noticeShapePath.AddCurveToPoint(new CGPoint(25.45, 9.07), new CGPoint(28.56, 2), new CGPoint(25.45, 5.14));
                noticeShapePath.AddLineTo(new CGPoint(25.45, 41.47));
                noticeShapePath.AddCurveToPoint(new CGPoint(24.29, 43.44), new CGPoint(25.45, 42.45), new CGPoint(24.68, 43.04));
                noticeShapePath.AddCurveToPoint(new CGPoint(9.55, 43.04), new CGPoint(16.73, 40.88), new CGPoint(11.88, 40.69));
                noticeShapePath.AddCurveToPoint(new CGPoint(8, 46.58), new CGPoint(8.58, 43.83), new CGPoint(8, 45.2));
                noticeShapePath.AddCurveToPoint(new CGPoint(14.4, 55.81), new CGPoint(8.19, 50.31), new CGPoint(12.07, 53.84));
                noticeShapePath.AddLineTo(new CGPoint(27.2, 69.56));
                noticeShapePath.AddCurveToPoint(new CGPoint(42.91, 77.8), new CGPoint(30.5, 74.47), new CGPoint(35.73, 77.21));
                noticeShapePath.AddCurveToPoint(new CGPoint(43.88, 77.8), new CGPoint(43.3, 77.8), new CGPoint(43.68, 77.8));
                noticeShapePath.AddCurveToPoint(new CGPoint(47.18, 78), new CGPoint(45.04, 77.8), new CGPoint(46.01, 78));
                noticeShapePath.AddLineTo(new CGPoint(48.34, 78));
                noticeShapePath.AddLineTo(new CGPoint(48.34, 78));
                noticeShapePath.AddCurveToPoint(new CGPoint(71.61, 52.08), new CGPoint(56.48, 78), new CGPoint(69.87, 75.05));
                noticeShapePath.AddCurveToPoint(new CGPoint(72, 48.54), new CGPoint(71.81, 51.29), new CGPoint(72, 49.72));
                noticeShapePath.ClosePath();
                noticeShapePath.MiterLimit = 4;

                UIColor.White.SetFill();
                noticeShapePath.Fill();
            }

            private static void drawWarning()
            {
                // Color Declarations
                var greyColor = new UIColor(0.236f, 0.236f, 0.236f, 1.000f);

                // Warning Group
                // Warning Circle Drawing
                var warningCirclePath = new UIBezierPath();
                warningCirclePath.MoveTo(new CGPoint(40.94, 63.39));
                warningCirclePath.AddCurveToPoint(new CGPoint(36.03, 65.55), new CGPoint(39.06, 63.39), new CGPoint(37.36, 64.18));
                warningCirclePath.AddCurveToPoint(new CGPoint(34.14, 70.45), new CGPoint(34.9, 66.92), new CGPoint(34.14, 68.49));
                warningCirclePath.AddCurveToPoint(new CGPoint(36.22, 75.54), new CGPoint(34.14, 72.41), new CGPoint(34.9, 74.17));
                warningCirclePath.AddCurveToPoint(new CGPoint(40.94, 77.5), new CGPoint(37.54, 76.91), new CGPoint(39.06, 77.5));
                warningCirclePath.AddCurveToPoint(new CGPoint(45.86, 75.35), new CGPoint(42.83, 77.5), new CGPoint(44.53, 76.72));
                warningCirclePath.AddCurveToPoint(new CGPoint(47.93, 70.45), new CGPoint(47.18, 74.17), new CGPoint(47.93, 72.41));
                warningCirclePath.AddCurveToPoint(new CGPoint(45.86, 65.35), new CGPoint(47.93, 68.49), new CGPoint(47.18, 66.72));
                warningCirclePath.AddCurveToPoint(new CGPoint(40.94, 63.39), new CGPoint(44.53, 64.18), new CGPoint(42.83, 63.39));
                warningCirclePath.ClosePath();
                warningCirclePath.MiterLimit = 4;

                greyColor.SetFill();
                warningCirclePath.Fill();


                // Warning Shape Drawing
                var warningShapePath = new UIBezierPath();
                warningShapePath.MoveTo(new CGPoint(46.23, 4.26));
                warningShapePath.AddCurveToPoint(new CGPoint(40.94, 2.5), new CGPoint(44.91, 3.09), new CGPoint(43.02, 2.5));
                warningShapePath.AddCurveToPoint(new CGPoint(34.71, 4.26), new CGPoint(38.68, 2.5), new CGPoint(36.03, 3.09));
                warningShapePath.AddCurveToPoint(new CGPoint(31.5, 8.77), new CGPoint(33.01, 5.44), new CGPoint(31.5, 7.01));
                warningShapePath.AddLineTo(new CGPoint(31.5, 19.36));
                warningShapePath.AddLineTo(new CGPoint(34.71, 54.44));
                warningShapePath.AddCurveToPoint(new CGPoint(40.38, 58.16), new CGPoint(34.9, 56.2), new CGPoint(36.41, 58.16));
                warningShapePath.AddCurveToPoint(new CGPoint(45.67, 54.44), new CGPoint(44.34, 58.16), new CGPoint(45.67, 56.01));
                warningShapePath.AddLineTo(new CGPoint(48.5, 19.36));
                warningShapePath.AddLineTo(new CGPoint(48.5, 8.77));
                warningShapePath.AddCurveToPoint(new CGPoint(46.23, 4.26), new CGPoint(48.5, 7.01), new CGPoint(47.74, 5.44));
                warningShapePath.ClosePath();
                warningShapePath.MiterLimit = 4;

                greyColor.SetFill();
                warningShapePath.Fill();
            }

            private static void drawInfo()
            {
                // Color Declarations
                var color0 = new UIColor(1.000f, 1.000f, 1.000f, 1.000f);

                // Info Shape Drawing
                var infoShapePath = new UIBezierPath();
                infoShapePath.MoveTo(new CGPoint(45.66, 15.96));
                infoShapePath.AddCurveToPoint(new CGPoint(45.66, 5.22), new CGPoint(48.78, 12.99), new CGPoint(48.78, 8.19));
                infoShapePath.AddCurveToPoint(new CGPoint(34.34, 5.22), new CGPoint(42.53, 2.26), new CGPoint(37.47, 2.26));
                infoShapePath.AddCurveToPoint(new CGPoint(34.34, 15.96), new CGPoint(31.22, 8.19), new CGPoint(31.22, 12.99));
                infoShapePath.AddCurveToPoint(new CGPoint(45.66, 15.96), new CGPoint(37.47, 18.92), new CGPoint(42.53, 18.92));
                infoShapePath.ClosePath();
                infoShapePath.MoveTo(new CGPoint(48, 69.41));
                infoShapePath.AddCurveToPoint(new CGPoint(40, 77), new CGPoint(48, 73.58), new CGPoint(44.4, 77));
                infoShapePath.AddLineTo(new CGPoint(40, 77));
                infoShapePath.AddCurveToPoint(new CGPoint(32, 69.41), new CGPoint(35.6, 77), new CGPoint(32, 73.58));
                infoShapePath.AddLineTo(new CGPoint(32, 35.26));
                infoShapePath.AddCurveToPoint(new CGPoint(40, 27.67), new CGPoint(32, 31.08), new CGPoint(35.6, 27.67));
                infoShapePath.AddLineTo(new CGPoint(40, 27.67));
                infoShapePath.AddCurveToPoint(new CGPoint(48, 35.26), new CGPoint(44.4, 27.67), new CGPoint(48, 31.08));
                infoShapePath.AddLineTo(new CGPoint(48, 69.41));
                infoShapePath.ClosePath();
                color0.SetFill();
                infoShapePath.Fill();
            }

            private static void drawEdit()
            {
                // Color Declarations
                var color = new UIColor(1.0f, 1.0f, 1.0f, 1.0f);

                // Edit shape Drawing
                var editPathPath = new UIBezierPath();
                editPathPath.MoveTo(new CGPoint(71, 2.7));
                editPathPath.AddCurveToPoint(new CGPoint(71.9, 15.2), new CGPoint(74.7, 5.9), new CGPoint(75.1, 11.6));
                editPathPath.AddLineTo(new CGPoint(64.5, 23.7));
                editPathPath.AddLineTo(new CGPoint(49.9, 11.1));
                editPathPath.AddLineTo(new CGPoint(57.3, 2.6));
                editPathPath.AddCurveToPoint(new CGPoint(69.7, 1.7), new CGPoint(60.4, -1.1), new CGPoint(66.1, -1.5));
                editPathPath.AddLineTo(new CGPoint(71, 2.7));
                editPathPath.AddLineTo(new CGPoint(71, 2.7));
                editPathPath.ClosePath();
                editPathPath.MoveTo(new CGPoint(47.8, 13.5));
                editPathPath.AddLineTo(new CGPoint(13.4, 53.1));
                editPathPath.AddLineTo(new CGPoint(15.7, 55.1));
                editPathPath.AddLineTo(new CGPoint(50.1, 15.5));
                editPathPath.AddLineTo(new CGPoint(47.8, 13.5));
                editPathPath.AddLineTo(new CGPoint(47.8, 13.5));
                editPathPath.ClosePath();
                editPathPath.MoveTo(new CGPoint(17.7, 56.7));
                editPathPath.AddLineTo(new CGPoint(23.8, 62.2));
                editPathPath.AddLineTo(new CGPoint(58.2, 22.6));
                editPathPath.AddLineTo(new CGPoint(52, 17.1));
                editPathPath.AddLineTo(new CGPoint(17.7, 56.7));
                editPathPath.AddLineTo(new CGPoint(17.7, 56.7));
                editPathPath.ClosePath();
                editPathPath.MoveTo(new CGPoint(25.8, 63.8));
                editPathPath.AddLineTo(new CGPoint(60.1, 24.2));
                editPathPath.AddLineTo(new CGPoint(62.3, 26.1));
                editPathPath.AddLineTo(new CGPoint(28.1, 65.7));
                editPathPath.AddLineTo(new CGPoint(25.8, 63.8));
                editPathPath.AddLineTo(new CGPoint(25.8, 63.8));
                editPathPath.ClosePath();
                editPathPath.MoveTo(new CGPoint(25.9, 68.1));
                editPathPath.AddLineTo(new CGPoint(4.2, 79.5));
                editPathPath.AddLineTo(new CGPoint(11.3, 55.5));
                editPathPath.AddLineTo(new CGPoint(25.9, 68.1));
                editPathPath.ClosePath();
                editPathPath.MiterLimit = 4;
                editPathPath.UsesEvenOddFillRule = true;
                color.SetFill();
                editPathPath.Fill();
            }

            private static void drawQuestion()
            {
                // Color Declarations
                var color = new UIColor(1.0f, 1.0f, 1.0f, 1.0f);
                // Questionmark Shape Drawing
                var questionShapePath = new UIBezierPath();
                questionShapePath.MoveTo(new CGPoint(33.75, 54.1));
                questionShapePath.AddLineTo(new CGPoint(44.15, 54.1));
                questionShapePath.AddLineTo(new CGPoint(44.15, 47.5));
                questionShapePath.AddCurveToPoint(new CGPoint(51.85, 37.2), new CGPoint(44.15, 42.9), new CGPoint(46.75, 41.2));
                questionShapePath.AddCurveToPoint(new CGPoint(61.95, 19.9), new CGPoint(59.05, 31.6), new CGPoint(61.95, 28.5));
                questionShapePath.AddCurveToPoint(new CGPoint(41.45, 2.8), new CGPoint(61.95, 7.6), new CGPoint(52.85, 2.8));
                questionShapePath.AddCurveToPoint(new CGPoint(25.05, 5.8), new CGPoint(34.75, 2.8), new CGPoint(29.65, 3.8));
                questionShapePath.AddLineTo(new CGPoint(25.05, 14.4));
                questionShapePath.AddCurveToPoint(new CGPoint(38.15, 12.3), new CGPoint(29.15, 13.2), new CGPoint(32.35, 12.3));
                questionShapePath.AddCurveToPoint(new CGPoint(49.65, 20.8), new CGPoint(45.65, 12.3), new CGPoint(49.65, 14.4));
                questionShapePath.AddCurveToPoint(new CGPoint(43.65, 31.7), new CGPoint(49.65, 26), new CGPoint(47.95, 28.4));
                questionShapePath.AddCurveToPoint(new CGPoint(33.75, 46.6), new CGPoint(37.15, 36.9), new CGPoint(33.75, 39.7));
                questionShapePath.AddLineTo(new CGPoint(33.75, 54.1));
                questionShapePath.ClosePath();
                questionShapePath.MoveTo(new CGPoint(33.15, 75.4));
                questionShapePath.AddLineTo(new CGPoint(45.35, 75.4));
                questionShapePath.AddLineTo(new CGPoint(45.35, 63.7));
                questionShapePath.AddLineTo(new CGPoint(33.15, 63.7));
                questionShapePath.AddLineTo(new CGPoint(33.15, 75.4));
                questionShapePath.ClosePath();
                color.SetFill();
                questionShapePath.Fill();
            }

            // Cache
            private static class Cache
            {
                public static UIImage imageOfCheckmarkImage;
                public static UIImage imageOfCrossImage;
                public static UIImage imageOfNoticeImage;
                public static UIImage imageOfWarningImage;
                public static UIImage imageOfInfoImage;
                public static UIImage imageOfEditImage;
                public static UIImage imageOfQuestionImage;
            }
        }

        private static void OnPresented(object sender, InteractiveAlertViewEventArgs e)
        {
            Presented?.Invoke(sender, e);
        }

        private static void OnClosed(object sender,InteractiveAlertViewEventArgs e)
        {
            Closed?.Invoke(sender, e);
        }
    }

    // Allow alerts to be closed/renamed in a chainable manner
    // Example: SCLAlertView().showSuccess(self, title: "Test", subTitle: "Value").close()
    public class SCLAlertViewResponder
    {
        // Initialisation and Title/Subtitle/Close functions
        public SCLAlertViewResponder(InteractiveAlertView alertview)
        {
            this.Alertview = alertview;
        }

        protected InteractiveAlertView Alertview { get; }

        public void SetTitle(string title)
        {
            this.Alertview.SetTitle(title);
        }

        public void SetSubTitle(string subTitle)
        {
            this.Alertview.SetSubTitle(subTitle);
        }

        public void Close()
        {
            this.Alertview.HideView();
        }

        public void SetDismissBlock(Action dismissBlock)
        {
            this.Alertview.SetDismissBlock(dismissBlock);
        }
    }

    public class InteractiveAlertViewEventArgs : EventArgs
    {
        public InteractiveAlertViewEventArgs(InteractiveAlertView alertView)
        {
            this.AlertView = alertView;
        }

        public InteractiveAlertView AlertView { get; }
    }
}