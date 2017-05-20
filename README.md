# ACR User Dialogs for Xamarin and Windows

A cross platform library that allows you to call for interactive alerts from a shared/portable library.

### Support:

* Android
* iOS
* Portable library

[![NuGet](https://img.shields.io/badge/InteractiveAlerts-0.1.3-brightgreen.svg)](https://www.nuget.org/packages/InteractiveAlerts/)

## Setup

InteractiveAlerts setup as [Acr.UserDialogs](https://github.com/aritchie/userdialogs).

#### iOS and Windows

    The library configures.

#### Android Initialization (In your main activity)

    InteractiveAlerts.Init(this);
    OR InteractiveAlerts.Init(() => provide your own top level activity provider)
    OR MvvmCross - InteractiveAlerts.Init(() => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity)
    OR Xamarin.Forms - InteractiveAlerts.Init(() => (Activity)Forms.Context)

### MvvmCross

# Screenshots from Interactive alerts

## iOS

Success alert:

![iOS Success](docs/screenshots/ios-success.png)

Error alert:

![iOS Error](docs/screenshots/ios-error.png)

Warning alert:

![iOS Warning](docs/screenshots/ios-warning.png)

Wait alert:

![iOS Wait](docs/screenshots/ios-wait.png)

## Android

![Android Success](docs/screenshots/android-success.png) 

Error alert:

![Android Error](docs/screenshots/android-error.png)

Warning alert:

![Android Warning](docs/screenshots/android-warning.png)

Wait alert:

![Android Wait](docs/screenshots/android-wait.png)

## Powered by
* Acr.UserDialogs - https://github.com/aritchie/userdialogs
* iOS - https://github.com/vikmeup/SCLAlertView-Swift
* Android - https://github.com/pedant/sweet-alert-dialog
