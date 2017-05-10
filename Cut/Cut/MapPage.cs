using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;
using XLabs.Platform.Services.Geolocation;
using System.Threading;

namespace Cut
{
    public class MapPage : ContentPage
    {
        Map map;
        StackLayout stack;
        internal void Init()
        {
            map.Pins.Clear();
            Task.Run(async () =>
            {
                //1,25.045355,121.531109,Taipei,三創科技園區
                var s = await Voc.GetAsync(Voc.setting["website"] + "sites.php");
                var v=s.Split('\n');
                Device.BeginInvokeOnMainThread(() => {
                    foreach (var x in v)
                    {
                        var msg=x.Split(',');
                        if (msg.Length != 6) continue;
                        //test_stp.Children.Add(new Label { Text = msg[0] });

                        var position = new Xamarin.Forms.Maps.Position(Double.Parse(msg[1]), Double.Parse(msg[2])); // Latitude, Longitude
                        var pin = new Pin
                        {
                            Type = PinType.Place,
                            Position = position,
                            Label = msg[5],
                            Address = msg[4]
                        };
                        pin.BindingContext = new StackLayout
                        {
                            Children = {
                                    new Label { Text = msg[0], FontSize=0 , IsVisible=false},
                                    new Label { Text = msg[5], FontSize=24 ,Margin=new Thickness(0,0,0,0)},
                                    new Label { Text = msg[4], FontSize=16 ,Margin=new Thickness(15,0,0,0)},
                                    new Label { Text = msg[1]+","+msg[2], FontSize=10 ,Margin=new Thickness(0,0,10,0) ,HorizontalTextAlignment= TextAlignment.End},
                                    new Label { Text = msg[3], FontSize=0 , IsVisible=false},
                                },
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        pin.Clicked += Pin_Clicked;
                        map.Pins.Add(pin);
                        /*
                        foreach (var y in msg)
                        {
                            test_stp.Children.Add(new Label { Text = y });
                        }*/
                    }
                });
            });
        }

        private async void Pin_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SitePage((sender as Pin).BindingContext as StackLayout));
        }

        public MapPage()
        {
            Title = "站點地圖";

            //var _geolocator = DependencyService.Get<IGeolocator>();
            //_geolocator.StartListening(2000, 0, true);
            var _cancelSource = new CancellationTokenSource();
            Voc._geolocator.GetPositionAsync(10000, _cancelSource.Token, true)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // PositionStatus = ((GeolocationException)t.Exception.InnerException).Error.ToString();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            //return;
                            map = new Map(
                                MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(23.973875, 120.982024), Distance.FromMiles(100))){
                                    IsShowingUser = true,
                                    HeightRequest = 100,
                                    WidthRequest = 960,
                                    VerticalOptions = LayoutOptions.FillAndExpand
                            };
                            stack.Children.Add(map);
                            Init();
                        });
                    }
                    else if (t.IsCanceled)
                    {
                        // PositionStatus = "Canceled";
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            //return;
                            string PositionStatus = t.Result.Timestamp.ToString("G");
                            string PositionLatitude =  t.Result.Latitude.ToString("N6");
                            string PositionLongitude = t.Result.Longitude.ToString("N6");
                            map = new Map(
                            MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(Double.Parse(PositionLatitude), Double.Parse(PositionLongitude)), Distance.FromMiles(100))){
                                IsShowingUser = true,
                                HeightRequest = 100,
                                WidthRequest = 960,
                                VerticalOptions = LayoutOptions.FillAndExpand
                            };
                            stack.Children.Add(map);
                            Init();
                        });
                    }
                });
            /*map = new Map(
            MapSpan.FromCenterAndRadius(
                    new Xamarin.Forms.Maps.Position(23.973875, 120.982024), Distance.FromMiles(100)))
            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand
            };*/
            /*
            var position = new Position(25.045355, 121.531109); // Latitude, Longitude
            var pin = new Pin
            {
                Type = PinType.Place,
                Position = position,
                Label = "工三創",
                Address = "火龍果"
            };
            map.Pins.Add(pin);
            */
            stack = new StackLayout { Spacing = 0 };
            Content = stack;
/*
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = grid
            };
*/
        }

    }
}