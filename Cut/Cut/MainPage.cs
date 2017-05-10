using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Cut
{
    public class MainPage : MasterDetailPage
    {
        MasterPage masterPage;
        public MainPage()
        {
            Voc._geolocator.StartListening(2000, 0, true);
            Navigation.PushModalAsync(new MainLoadingPage(this));
            masterPage = new MasterPage();
            Master = masterPage;
            Detail = new NavigationPage(new HomePage());
            masterPage.ListView.ItemSelected += OnItemSelected;
            
            //Detail.Icon = "hamburger.png";
            //Master.Icon = "hamburger.png";
        }
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                //Detail.Icon = "hamburger.png";
                masterPage.ListView.SelectedItem = null;
                IsPresented = false;
            }
        }
        bool navi_zero = false;
        protected override bool OnBackButtonPressed()
        {
            if (Detail.Navigation.NavigationStack.Count > 1)
            {
                navi_zero = false;
                return base.OnBackButtonPressed();
            }
            else
            {
                IsPresented = !IsPresented;
                //return base.OnBackButtonPressed();
            }
            return true;
        }
    }

}
