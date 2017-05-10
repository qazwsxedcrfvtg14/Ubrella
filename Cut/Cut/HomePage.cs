using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using System.Threading;
using XLabs.Platform.Services.Geolocation;
using System.Threading.Tasks;

namespace Cut
{
    public class HomePage : ContentPage
    {
        StackLayout st;
        internal void Init()
        {
            st.Children.Clear();
            //var _geolocator = DependencyService.Get<IGeolocator>();
            //_geolocator.StartListening(2000, 0, true);
            var _cancelSource = new CancellationTokenSource();
            Voc._geolocator.GetPositionAsync(10000, _cancelSource.Token, true)
                .ContinueWith(async t =>
                {
                    if (t.IsFaulted)
                    {
                        // PositionStatus = ((GeolocationException)t.Exception.InnerException).Error.ToString();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            st.Children.Add(new Label { Text="GPS取得失敗"});
                            //(Content as StackLayout).Children.Add(new Label { Text = "Error" });
                        });
                    }
                    else if (t.IsCanceled)
                    {
                        // PositionStatus = "Canceled";
                    }
                    else
                    {
                        double PositionLatitude = double.Parse(t.Result.Latitude.ToString("N6"));
                        double PositionLongitude = double.Parse(t.Result.Longitude.ToString("N6"));
                        var s = await Voc.GetAsync(Voc.setting["website"] + "sites.php");
                        Voc.sites_map=s;
                        var v = s.Split('\n');
                        Device.BeginInvokeOnMainThread(() => {
                            string ans = "";
                            double min_km = double.PositiveInfinity;
                            foreach (var x in v)
                            {
                                var msg = x.Split(',');
                                if (msg.Length != 6) continue;
                                //test_stp.Children.Add(new Label { Text = msg[0] });
                                double la = Double.Parse(msg[1]);
                                double lo = Double.Parse(msg[2]);
                                double km = Voc.GetDistance(PositionLatitude, PositionLongitude, la, lo);
                                if (km < min_km)
                                {
                                    min_km = km;
                                    ans = x;
                                }
                            }
                            var res = ans.Split(',');
                            if (res.Length != 6) return;
                            var txt = new Label { Text = "最近站點：" + res[5], FontSize = 24 ,Margin=new Thickness(0,0,0,10)};
                            var bind_st= new StackLayout
                            {
                                Children = {
                                    new Label { Text = res[0], FontSize=0 , IsVisible=false},
                                    new Label { Text = res[5], FontSize=24 ,Margin=new Thickness(0,0,0,0)},
                                    new Label { Text = res[4], FontSize=16 ,Margin=new Thickness(15,0,0,0)},
                                    new Label { Text = res[1]+","+res[2], FontSize=10 ,Margin=new Thickness(0,0,10,0) ,HorizontalTextAlignment= TextAlignment.End},
                                    new Label { Text = res[3], FontSize=0 , IsVisible=false},
                                },
                                Margin = new Thickness(0, 0, 0, 10)
                            };
                            txt.BindingContext = bind_st;
                            var tgr = new TapGestureRecognizer();
                            tgr.Tapped += Tgr_Tapped;
                            txt.GestureRecognizers.Add(tgr);
                            st.Children.Add(txt);
                            txt = new Label { Text = "與其距離：" + min_km.ToString() + "Km", FontSize = 24, Margin = new Thickness(0, 0, 0, 10) };
                            txt.BindingContext = bind_st;
                            txt.GestureRecognizers.Add(tgr);
                            st.Children.Add(txt);
                            Task.Run(async () =>
                            {
                                var ss = await Voc.GetAsync(Voc.setting["website"] + "pop.php?location=" + res[3]);
                                if (ss == null) ss = "";
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    txt = new Label { Text = "降雨機率：" + ss + "%", FontSize = 24, Margin = new Thickness(0, 0, 0, 10) };
                                    txt.BindingContext = bind_st;
                                    txt.GestureRecognizers.Add(tgr);
                                    st.Children.Add(txt);
                                    if (ss.Trim() == "") ss = "0";
                                    var img = new Image
                                    {
                                        Source = "icon.png",
                                        Margin = new Thickness(0, 30, 0, 20),
                                        HeightRequest = 150,
                                        Opacity = Double.Parse(ss) / 100
                                    };
                                    img.BindingContext = bind_st;
                                    tgr = new TapGestureRecognizer();
                                    tgr.Tapped += Tgr_Tapped1;
                                    img.GestureRecognizers.Add(tgr);
                                    st.Children.Add(img);
                                });
                            });
                        });
                    }
                });

        }

        private async void Tgr_Tapped1(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SitePage((sender as Image).BindingContext as StackLayout));
        }

        private async void Tgr_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SitePage((sender as Label).BindingContext as StackLayout));
        }

        public HomePage()
        {

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "refresh.png",
                Text = "重整",
                Command = new Command(() => {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Init();
                    });
                })
            });
            Title = "首頁";
            st = new StackLayout
            {
                Margin = new Thickness(10,30,10,0),
                HorizontalOptions = LayoutOptions.Center,
            };
            Content = st;
            Init();
        }
    }
}
