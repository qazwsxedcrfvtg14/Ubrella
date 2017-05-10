using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using XLabs.Ioc;  // Using for SimpleContainer 
using XLabs.Platform.Services.Geolocation;  // Using for Geolocation 
using XLabs.Platform.Device; // Using for Device 
using AudioToolbox;
using AVFoundation;

namespace Cut.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : XLabs.Forms.XFormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.IOS.Popup.Init();
            global::Xamarin.Forms.Forms.Init();


            NSError error;
            AVAudioSession instance = AVAudioSession.SharedInstance();
            instance.SetCategory(new NSString("AVAudioSessionCategoryPlayback"), AVAudioSessionCategoryOptions.MixWithOthers, out error);
            instance.SetMode(new NSString("AVAudioSessionModeDefault"), out error);
            instance.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out error);

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
