using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using System.Threading;
using XLabs.Platform.Services.Geolocation;

namespace Cut
{
    public class ReadMePage : ContentPage
    {
        ScrollView st;
        internal void Init()
        {
        }

        private async void Tgr_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SitePage((sender as Label).BindingContext as StackLayout));
        }

        public ReadMePage()
        {
            Title = "Home";
            int fontsize = 16;
            st=new ScrollView
            {
                Content = new StackLayout
                {
                    Margin = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    Children = {
                    new Label {
                        Text = "請按左上角的漢堡鍵(三條橫線)開啟選單",
                        FontSize=fontsize
                    },
                    new Label {
                        Text = "0.家：使用說明",
                        FontSize=fontsize
                    },
                    new Label {
                        Text = "1.當你開始使用此APP時就代表你已同意使用條款",
                        FontSize=fontsize
                    },
                    new Button {
                        Text = "使用條款",
                        HorizontalOptions = LayoutOptions.Center,
                        Command = new Command(async()=> {
                            await DisplayAlert("使用條款",
                                "暫無詳細條款\n",
                                "了解");
                        })
                    }
                }
                }
            };
        Content = st;
        }
    }
}
