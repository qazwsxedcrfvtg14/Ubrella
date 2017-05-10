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
    public class UserPage : ContentPage
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
                var s = await Voc.GetAsync(Voc.setting["website"] + "check.php?user=" + Voc.setting["user"]);
                Device.BeginInvokeOnMainThread(() => {
                    bool ok = true;
                    foreach (var y in s.Split('\n'))
                    {
                        var v = y.Split(',');
                        if (v.Length == 2 && v[0].Trim() != "")
                        {
                            foreach (var x in Voc.sites_map.Split('\n'))
                            {
                                var vv = x.Split(',');
                                if (vv.Length != 6) continue;
                                if (vv[0] == v[0])
                                {
                                    test_stp.Children.Add(
                                        new Label { Text = "你上次在" + vv[5] + "的第" + v[1].Trim() + "格借的傘忘記還了!", FontSize = 20, Margin = new Thickness(10, 0, 0, 0) }
                                        );
                                    ok = false;
                                }
                            }
                        }
                    }
                    if(ok)
                    test_stp.Children.Add(
                        new Label { Text = "恭喜你，你沒有忘記還傘！", FontSize = 20, Margin = new Thickness(10, 0, 0, 0) }
                        );
                });
            });
        }
        public UserPage()
        {
            string id = Voc.setting["user"];
            Title = "使用者ID:"+id;
            test_stp = new StackLayout { };
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