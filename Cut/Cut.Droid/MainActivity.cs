using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics.Drawables;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Forms;
using System.IO;
using XLabs.Platform.Device;
using XLabs.Platform.Services.Media;
using Tesseract;
using Tesseract.Droid;
using TinyIoC;
using XLabs.Ioc.TinyIOC;
namespace Cut.Droid
{
    [Activity(Label = "Ubrella", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : XLabs.Forms.XFormsApplicationDroid
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (!Resolver.IsSet)
            {
                this.SetIoc();
            }
            else
            {
                if (Resolver.Resolve<IXFormsApp>() is IXFormsApp<XFormsApplicationDroid> app) app.AppContext = this;
            }
            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.FormsMaps.Init(this, bundle);
            Xamarin.Forms.Forms.ViewInitialized += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.View.StyleId))
                {
                    e.NativeView.ContentDescription = e.View.StyleId;
                }
            };

            LoadApplication(new App());
            //ActionBar.SetIcon(new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
            ActionBar.SetIcon(new ColorDrawable(Android.Graphics.Color.Transparent));

        }

        /// <summary>
        /// Sets the IoC.
        /// </summary>
        private void SetIoc()
        {

            var app = new XFormsAppDroid();

            app.Init(this);

            var documents = app.AppDataDirectory;
            var pathToDatabase = Path.Combine(documents, "xforms.db");
            var container = TinyIoCContainer.Current;
            container.Register<IDevice>(AndroidDevice.CurrentDevice);
            container.Register<ITesseractApi>((cont, parameters) =>
            {
                return new TesseractApi(ApplicationContext, Tesseract.Droid.AssetsDeployment.OncePerInitialization);
            });
            Resolver.SetResolver(new TinyResolver(container));
            //Resolver.SetResolver(resolverContainer.GetResolver());

            //.Register<ISecureStorage>(t => new KeyVaultStorage(t.Resolve<IDevice>().Id.ToCharArray()))
            //.Register<ICacheProvider>(
            //t => new SQLiteSimpleCache(new SQLitePlatformAndroid(),
            //new SQLiteConnectionString(pathToDatabase, true), t.Resolve<IJsonSerializer>()));


        }

    }
}

