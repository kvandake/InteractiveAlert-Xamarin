using System;
using UIKit;
using CoreGraphics;
using System.Timers;
using Foundation;

// https://github.com/vikmeup/SCLAlertView-Swift/blob/master/SCLAlertView/SCLAlertView.swift#L309
// https://github.com/dogo/SCLAlertView
namespace Service.Bit.Client.iOS
{
    public static class SCLAlertViewExtension
    {
        public static uint DefaultColorInt(this SCLAlertView alertView, SCLAlertViewStyle viewStyle)
        {
            switch (viewStyle)
            {
                case SCLAlertViewStyle.success:
                    return 0x22B573;
                case SCLAlertViewStyle.error:
                    return 0xC1272D;
                case SCLAlertViewStyle.notice:
                    return 0x727375;
                case SCLAlertViewStyle.warning:
                    return 0xFFD110;
                case SCLAlertViewStyle.info:
                    return 0x2866BF;
                case SCLAlertViewStyle.edit:
                    return 0xA429FF;
                case SCLAlertViewStyle.wait:
                    return 0xD62DA5;
                case SCLAlertViewStyle.question:
                    return 0x727375;
                default:
                    return 0;
            }
        }
    }

    // Pop Up Styles
    public enum SCLAlertViewStyle
    {
        success, error, notice, warning, info, edit, wait, question

    }

    // Animation Styles
    public enum SCLAnimationStyle
    {
        noAnimation, topToBottom, bottomToTop, leftToRight, rightToLeft
    }

    // Action Types
    public enum SCLActionType
    {
        none, selector, closure
    }

    // Button sub-class
    class SCLButton : UIButton
    {
        public SCLActionType actionType = SCLActionType.none;
        public UIColor customBackgroundColor;
        public UIColor customTextColor;
        public string initialTitle;
        public bool showDurationStatus = false;

        public SCLButton()
            : base(CGRect.Empty)
        {

        }
    }

    // Allow alerts to be closed/renamed in a chainable manner
    // Example: SCLAlertView().showSuccess(self, title: "Test", subTitle: "Value").close()
    class SCLAlertViewResponder
    {
        SCLAlertView alertview;

        // Initialisation and Title/Subtitle/Close functions
        public SCLAlertViewResponder(SCLAlertView alertview)
        {
            this.alertview = alertview;
        }

        public void setTitle(string title)
        {
            this.alertview.labelTitle.text = title;
        }

        public void setSubTitle(string subTitle)
        {
            this.alertview.viewText.text = subTitle;
        }

        public void close()
        {
            this.alertview.hideView();
        }

        public void setDismissBlock(Action dismissBlock)
        {
            this.alertview.dismissBlock = dismissBlock;
        }
    }


    // The Main Class
    public class SCLAlertView : UIViewController
    {
        static readonly nfloat kCircleHeightBackground = 62.0f;

        public struct SCLAppearance
        {
            public nfloat kDefaultShadowOpacity;
            public nfloat kCircleTopPosition;
            public nfloat kCircleBackgroundTopPosition;
            public nfloat kCircleHeight;
            public nfloat kCircleIconHeight;
            public nfloat kTitleTop;
            public nfloat kTitleHeight;
            public nfloat kTitleMinimumScaleFactor;
            public nfloat kWindowWidth;
            public nfloat kWindowHeight;
            public nfloat kTextHeight;
            public nfloat kTextFieldHeight;
            public nfloat kTextViewdHeight;
            public nfloat kButtonHeight;
            public UIColor circleBackgroundColor;
            public UIColor contentViewColor;
            public UIColor contentViewBorderColor;
            public UIColor titleColor;

            // Fonts
            public UIFont kTitleFont;
            public UIFont kTextFont;
            public UIFont kButtonFont;

            // UI Options
            public bool disableTapGesture;
            public bool showCloseButton;
            public bool showCircularIcon;
            public bool shouldAutoDismiss; // Set this false to 'Disable' Auto hideView when SCLButton is tapped
            public nfloat contentViewCornerRadius;
            public nfloat fieldCornerRadius;
            public nfloat buttonCornerRadius;
            public bool dynamicAnimatorActive;

            // Actions
            public bool hideWhenBackgroundViewIsTapped;

            public SCLAppearance(float kDefaultShadowOpacity = 0.7f,
                                 float kCircleTopPosition = 0.0f,
                                 float kCircleBackgroundTopPosition = 6.0f,
                                 float kCircleHeight = 56.0f,
                                 float kCircleIconHeight = 20.0f,
                                 float kTitleTop = 30.0f,
                                 float kTitleHeight = 25.0f,
                                 float kWindowWidth = 240.0f,
                                 float kWindowHeight = 178.0f,
                                 float kTextHeight = 90.0f,
                                 float kTextFieldHeight = 45.0,
                                 float kTextViewdHeight = 80.0f,
                                 float kButtonHeight = 45.0f,
                                 UIFont kTitleFont = UIFont.SystemFontOfSize(20),
                                 float kTitleMinimumScaleFactor = 1.0f,
                                 UIFont kTextFont = UIFont.SystemFontOfSize(14),
                                 UIFont kButtonFont = UIFont.SystemFontSize(14),
                                 bool showCloseButton = true,
                                 bool showCircularIcon = true,
                                 bool shouldAutoDismiss = true,
                                 float contentViewCornerRadius = 5.0f,
                                 float fieldCornerRadius = 3.0f,
                                 float buttonCornerRadius = 3.0f,
                                 bool hideWhenBackgroundViewIsTapped = false,
                                 UIColor circleBackgroundColor = UIColor.white,
                                 UIColor contentViewColor = UIColor.FromRGB(0xFFFFFF),
                                 UIColor contentViewBorderColor = UIColor.FromRGB(0xCCCCCC),
                                 UIColor titleColor: UIColor = UIColor.FromRGB(0x4D4D4D),
                                 bool dynamicAnimatorActive = false,
                                 bool disableTapGesture = false)
            {

                this.kDefaultShadowOpacity = kDefaultShadowOpacity;
                this.kCircleTopPosition = kCircleTopPosition;
                this.kCircleBackgroundTopPosition = kCircleBackgroundTopPosition;
                this.kCircleHeight = kCircleHeight;
                this.kCircleIconHeight = kCircleIconHeight;
                this.kTitleTop = kTitleTop;
                this.kTitleHeight = kTitleHeight;
                this.kWindowWidth = kWindowWidth;
                this.kWindowHeight = kWindowHeight;
                this.kTextHeight = kTextHeight;
                this.kTextFieldHeight = kTextFieldHeight;
                this.kTextViewdHeight = kTextViewdHeight;
                this.kButtonHeight = kButtonHeight;
                this.circleBackgroundColor = circleBackgroundColor;
                this.contentViewColor = contentViewColor;
                this.contentViewBorderColor = contentViewBorderColor;
                this.titleColor = titleColor;


                this.kTitleFont = kTitleFont;
                this.kTitleMinimumScaleFactor = kTitleMinimumScaleFactor;
                this.kTextFont = kTextFont;
                this.kButtonFont = kButtonFont;


                this.disableTapGesture = disableTapGesture;
                this.showCloseButton = showCloseButton;
                this.showCircularIcon = showCircularIcon;
                this.shouldAutoDismiss = shouldAutoDismiss;
                this.contentViewCornerRadius = contentViewCornerRadius;
                this.fieldCornerRadius = fieldCornerRadius;
                this.buttonCornerRadius = buttonCornerRadius;


                this.hideWhenBackgroundViewIsTapped = hideWhenBackgroundViewIsTapped;
                this.dynamicAnimatorActive = dynamicAnimatorActive;
            }

            public void setkWindowHeight(nfloat kWindowHeight)
            {
                this.kWindowHeight = kWindowHeight;
            }

            public void setkTextHeight(nfloat kTextHeight)
            {
                this.kTextHeight = kTextHeight;
            }
        }

        SCLAppearance appearance;

        // UI Colour
        UIColor viewColor;

        // UI Options
        public UIColor iconTintColor;
        public UIView customSubview;

        // Members declaration
        UIView baseView = new UIView();
        UILabel labelTitle = new UILabel();
        UITextView viewText = new UITextView();
        UIView contentView = new UIView();
        UIView circleBG = new UIView(new CGRect(x: 0, y: 0, width: kCircleHeightBackground, height: kCircleHeightBackground));
        UIView circleView = new UIView();
        UIView circleIconView;
        double duration;
        NSTimer durationStatusTimer;
        NSTimer durationTimer;
        Action dismissBlock;
        private UITextField[] inputs;
        private UITextView[] input;
        internal SCLButton[] buttons;
        private SCLAlertView selfReference;



        public SCLAlertView(SCLAppearance appearance)
            : base(null, null)
        {
            this.appearance = appearance;
            this.setup();
        }

        public SCLAlertView(string nibNameOrNil, NSBundle bundle, SCLAppearance appearance)
            : base(nibNameOrNil, bundle)
        {
            this.appearance = appearance;
            this.setup();
        }

        public SCLAlertView(string nibNameOrNil, NSBundle bundle)
            : this(nibNameOrNil, bundle, new SCLAppearance())
        {

        }



        public SCLAlertView()
        : this(new SCLAppearance())
        {

        }

        private void setup()
        {
            // Set up main view
            this.View.Frame = UIScreen.MainScreen.Bounds;
            this.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            this.View.BackgroundColor = new UIColor(red: 0, green: 0, blue: 0, alpha: appearance.kDefaultShadowOpacity);
            this.View.AddSubview(baseView);
            // Base View
            baseView.Frame = this.View.Frame;
            baseView.AddSubview(contentView);
            // Content View
            contentView.Layer.CornerRadius = appearance.contentViewCornerRadius;
            contentView.Layer.MasksToBounds = true;
            contentView.Layer.BorderWidth = 0.5f;
            contentView.AddSubview(labelTitle);
            contentView.AddSubview(viewText);
        // Circle View
        circleBG.backgroundColor = appearance.circleBackgroundColor
        circleBG.layer.cornerRadius = circleBG.frame.size.height / 2
        baseView.addSubview(circleBG)
        circleBG.addSubview(circleView)
        let x = (kCircleHeightBackground - appearance.kCircleHeight) / 2
        circleView.frame = CGRect(x: x, y: x + appearance.kCircleTopPosition, width: appearance.kCircleHeight, height: appearance.kCircleHeight)
        circleView.layer.cornerRadius = circleView.frame.size.height / 2
        // Title
        labelTitle.numberOfLines = 0
        labelTitle.textAlignment = .center
        labelTitle.font = appearance.kTitleFont
        if (appearance.kTitleMinimumScaleFactor < 1)
            {
                labelTitle.minimumScaleFactor = appearance.kTitleMinimumScaleFactor
            labelTitle.adjustsFontSizeToFitWidth = true
        }
            labelTitle.frame = CGRect(x: 12, y: appearance.kTitleTop, width: appearance.kWindowWidth - 24, height: appearance.kTitleHeight)
        // View text
        viewText.isEditable = false
        viewText.textAlignment = .center
        viewText.textContainerInset = UIEdgeInsets.zero
        viewText.textContainer.lineFragmentPadding = 0;
            viewText.font = appearance.kTextFont
        // Colours
        contentView.backgroundColor = appearance.contentViewColor
        viewText.backgroundColor = appearance.contentViewColor
        labelTitle.textColor = appearance.titleColor
        viewText.textColor = appearance.titleColor
        contentView.layer.borderColor = appearance.contentViewBorderColor.cgColor
        //Gesture Recognizer for tapping outside the textinput
        if appearance.disableTapGesture == false {
                let tapGesture = UITapGestureRecognizer(target: self, action: #selector(SCLAlertView.tapped(_:)))
            tapGesture.numberOfTapsRequired = 1
    
                self.view.addGestureRecognizer(tapGesture)
        }
        }
    }
}