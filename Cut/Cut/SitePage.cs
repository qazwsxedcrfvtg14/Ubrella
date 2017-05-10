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

namespace Cut
{
    public class SitePage : ContentPage
    {
        Grid grid = new Grid
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            RowDefinitions ={
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = 5
        };
        StackLayout test_stp;
        StackLayout hum_stp;
        WrapLayout slot_stp;
        StackLayout site;
        internal void Init()
        {
            test_stp.Children.Clear();
            Task.Run(async () =>
            {
                var s = await Voc.GetAsync(Voc.setting["website"] + "siteinfo.php?q=" + (site.Children.ElementAt(0) as Label).Text);
                Device.BeginInvokeOnMainThread(() => {
                    test_stp.Children.Add(
                        new Label { Text = s, FontSize = 20, Margin = new Thickness(10, 0, 0, 0) }
                        );
                });
            });
            hum_stp.Children.Clear();
            Task.Run(async () =>
            {
                var s = await Voc.GetAsync(Voc.setting["website"] + "pop.php?location=" + (site.Children.ElementAt(4) as Label).Text);
                if (s == null) s = "";
                Device.BeginInvokeOnMainThread(() => {
                    hum_stp.Children.Add(
                        new Label { Text = "降雨機率：" + s+"%", FontSize = 30, Margin = new Thickness(10, 0, 0, 0) }
                        );
                    if (s.Trim() == "") s = "0";
                    hum_stp.Children.Add(new Image
                    {
                        Source = "icon.png",
                        Margin = new Thickness(0, 30, 0, 0),
                        HeightRequest = 150,
                        Opacity = Double.Parse(s) / 100
                    });
                });
            });
            slot_stp.Children.Clear();
            Task.Run(async () =>
            {
                var s = await Voc.GetAsync(Voc.setting["website"] + "search.php?station=" + (site.Children.ElementAt(0) as Label).Text);
                Device.BeginInvokeOnMainThread(() => {
                    foreach (var x in s.Split('\n'))
                    {
                        var v=x.Split(',');
                        if (v.Length != 2) continue;
                        if(v[1]=="1")
                        slot_stp.Children.Add(
                            new Image { Source = "ok.png", Margin = new Thickness(10), HeightRequest = 50 }
                            );
                        else
                        slot_stp.Children.Add(
                            new Image { Source = "error.png", Margin = new Thickness(10), HeightRequest = 50 }
                            );
                    }
                });
            });
        }
        public SitePage(StackLayout _site)
        {
            site = _site;
            string id = (site.Children.ElementAt(0) as Label).Text;
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "favorite.png",
                Text = "最愛",
                Command = new Command(() => {
                    if (Voc.favorite.exists(id))
                        Voc.favorite.remove(id);
                    else
                        Voc.favorite.add(id, "");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Voc.favorite.exists(id))
                            ToolbarItems[0].Icon = "unfavorite.png";
                        else
                            ToolbarItems[0].Icon = "favorite.png";
                    });
                })
            });
            if (Voc.favorite.exists(id))
                ToolbarItems[0].Icon = "unfavorite.png";
            else
                ToolbarItems[0].Icon = "favorite.png";
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
            Title = (site.Children.ElementAt(1)as Label).Text;
            test_stp = new StackLayout { };
            hum_stp = new StackLayout {HorizontalOptions= LayoutOptions.Center };
            slot_stp = new WrapLayout {};
            //grid.Children.Add(title, 0, 0);
            //grid.Children.Add(tb1, 0, 1);
            grid.Children.Add(test_stp, 0, 1);
            grid.Children.Add(hum_stp, 0, 2);
            grid.Children.Add(slot_stp, 0, 3);
            grid.Margin = new Thickness(10, 10, 10, 5);
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = grid
            };
            Init();
        }

    }
}