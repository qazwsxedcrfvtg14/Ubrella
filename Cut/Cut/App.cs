using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Cut
{
    
    public class App : Application
    {
        public App()
        {


            // The root page of your application
            MainPage = new MainPage();

        }
        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
