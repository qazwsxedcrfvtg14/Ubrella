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
    public class SitesPage : ContentPage
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

        internal void Init()
        {
            Task.Run(async () =>
            {
                var s = await Voc.GetAsync(Voc.setting["website"] + "sites.php");
                var v = s.Split('\n');
                Device.BeginInvokeOnMainThread(() => {
                    foreach (var x in v)
                    {
                        var msg = x.Split(',');
                        if (msg.Length != 6) continue;
                        //test_stp.Children.Add(new Label { Text = msg[0] });
                        var st = new StackLayout
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
                        var tgr = new TapGestureRecognizer();
                        tgr.Tapped += Tgr_Tapped;
                        st.GestureRecognizers.Add(tgr);
                        test_stp.Children.Add(st);
                        /*
                        foreach (var y in msg)
                        {
                            test_stp.Children.Add(new Label { Text = y });
                        }*/

                        //await Navigation.PushAsync(new SingleVocPage(input_voc.Text));
                    }
                });
            });
        }

        private async void Tgr_Tapped(object sender, EventArgs e)
        {
            var senderStackLayout = sender as StackLayout;
            var senderLabel = senderStackLayout.Children.ElementAt(0) as Label;
            await Navigation.PushAsync(new SitePage(senderStackLayout));
        }

        public SitesPage()
        {
            Title = "站點列表";
            /*
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "bookmark.png",
                Text = "隨機填空",
                Command = new Command(() => {
                    Navigation.PushAsync(new TestPage1(0, 0));
                })
            });
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "list.png",
                Text = "隨機選擇",
                Command = new Command(() => {
                    Navigation.PushAsync(new TestPage2(0, 0));
                })
            });*/
            //title = new Label { Text = "Breaking News", FontSize = 18 }; ;
            //var tb1 = new Label { Text = "這份考卷在關掉此應用程式或是按右上重整前都不會消失喔!", HorizontalTextAlignment = TextAlignment.Center };
            test_stp = new StackLayout { };
            //grid.Children.Add(title, 0, 0);
            //grid.Children.Add(tb1, 0, 1);
            grid.Children.Add(test_stp, 0, 1);
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