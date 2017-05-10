using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using XLabs.Forms.Controls;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Cut
{
    public class SingleRootPage : ContentPage
    {

        internal StackLayout
            voc_root = new StackLayout { Orientation = StackOrientation.Vertical };
            //alias_list = new StackLayout(),
            //VocList = new StackLayout();
        ObservableCollection<wod> VocList_Items, alias_list_Items;
        ListView VocList = new ListView { }, alias_list = new ListView { };
        Grid grid = new Grid
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    //new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) }
                },
        };
        string _voc, _exp;
        public SingleRootPage(string param)
        {
            //param.ToLower();
            param = param.Trim();
            //NavigationPage.SetHasNavigationBar(this, false);
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "edit.png",
                Text = "修改",
                Command = new Command(async () => {
                    var page = new MyPopupPage(this);
                    await PopupNavigation.PushAsync(page);
                    //await Navigation.PushPopupAsync(new MyPopupPage());
                })
            });
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "delete.png",
                Text = "刪除",
                Command = new Command(async () =>
                {
                    var answer = await DisplayAlert("刪除", "真的要刪除嗎?", "是，沒錯！", "否，手滑了");
                    if (answer == true)
                    {
                        Voc.words.remove(_voc);
                        await Navigation.PopAsync();
                    }
                })
            });


            //Title = param;
            var tivoc = new Label { };
            tivoc.Text = param;
            tivoc.FontSize = 26;
            if (Device.OS == TargetPlatform.Android)
                tivoc.TextColor = Color.FromRgb(255, 255, 204);
            _voc = param;
            _exp = Voc.GetRootExp(param);
            voc_root.Children.Clear();
            foreach (var x in Voc.CutExp(_exp))
            {
                voc_root.Children.Add(new Label
                {
                    Text = x,
                    Margin = 8,
                    FontSize=20,
                });
            }

            VocList_Items = new ObservableCollection<wod>();
            VocList.ItemsSource = VocList_Items;
            var customCell = new DataTemplate(typeof(stcell));
            customCell.SetBinding(stcell.vocProperty, "voc");
            customCell.SetBinding(stcell.expProperty, "exp");
            VocList.ItemTemplate = customCell;
            VocList.ItemSelected += VocList_ItemSelected; ;
            VocList.HasUnevenRows = true;

            alias_list_Items = new ObservableCollection<wod>();
            alias_list.ItemsSource = alias_list_Items;
            alias_list.ItemTemplate = customCell;
            alias_list.ItemSelected += Alias_list_ItemSelected; ;
            alias_list.HasUnevenRows = true;

            alias_list_Items.Clear();
            if (_voc[0] == '-' && _voc[_voc.Length - 1] == '-')
            {
                foreach (var x in Voc.root.data)
                    if ("-" + x.Key + "-" != _voc && Voc.GetRootExp("-" + x.Key + "-") == _exp)
                        alias_list_Items.Add(new wod(x.Key,""));
            }
            else if (_voc[0] != '-' && _voc[_voc.Length - 1] == '-')
            {
                foreach (var x in Voc.prefix.data)
                    if (x.Key + "-" != _voc && Voc.GetRootExp(x.Key + "-") == _exp)
                        alias_list_Items.Add(new wod(x.Key, ""));
            }
            else if (_voc[0] == '-' && _voc[_voc.Length - 1] != '-')
            {
                foreach (var x in Voc.suffix.data)
                    if ("-" + x.Key != _voc && Voc.GetRootExp("-" + x.Key) == _exp)
                        alias_list_Items.Add(new wod(x.Key, ""));
            }
            alias_list.HeightRequest = alias_list_Items.Count * stcell.Height;

            VocList_Items.Add(new wod("", "讀取中..."));
            VocList.HeightRequest = VocList_Items.Count * stcell.Height;
            Task.Run(() =>
            {
                string data = "";
                string reg_string = "";

                if (_voc[0] == '-' && _voc[_voc.Length - 1] == '-')
                    data = _voc.Substring(1, _voc.Length - 2);
                else if (_voc[0] != '-' && _voc[_voc.Length - 1] == '-')
                    data = _voc.Substring(0, _voc.Length - 1);
                else if (_voc[0] == '-' && _voc[_voc.Length - 1] != '-')
                    data = _voc.Substring(1, _voc.Length - 1);

                reg_string = ".*" + data + ".*";
                if (data.Length == 1)
                {
                    if (_voc[0] == '-')
                        reg_string = ".*" + data;
                    else
                        reg_string = data + ".*";
                }

                Regex reg = new Regex(reg_string, RegexOptions.IgnoreCase);
                int cnt = 0;
                bool brk = false;
                List<string> lis=new List<string>();
                foreach (var x in Voc.words.data)
                {
                    if (brk) break;
                    if (!reg.IsMatch(x.Key) || x.Key == data) continue;
                    var s2 = Voc.Show2(x.Key);
                    foreach (var y in s2)
                    {
                        if (y == _voc)
                        {
                            lis.Add(x.Key);
                            
                            if (++cnt == 30) brk = true;
                            break;
                        }
                    }
                }

                Device.BeginInvokeOnMainThread(() => {
                    VocList_Items.Clear();
                    foreach (var x in lis)
                        VocList_Items.Add(new wod(x, Voc.GetExpSimple(Voc.words.val(x)).Trim()));
                    VocList.HeightRequest = VocList_Items.Count * stcell.Height;
                });
            });
            Content = new ScrollView {
                Orientation = ScrollOrientation.Vertical,
                Content = grid,
                //Padding = new Thickness(10, 10, 10, 5)
            };
            grid.Margin = new Thickness(10, 10, 10, 5);
            grid.Children.Add(tivoc, 0, 0);
            grid.Children.Add(alias_list, 0, 1);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = voc_root }, 0, 2);
            grid.Children.Add(VocList, 0, 3);
            
        }

        private async void Alias_list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = (e.SelectedItem as wod);
            if (item != null) alias_list.SelectedItem = null;
            if (item != null && item.voc != "")
            {
                var s = item.voc;
                if (_voc[0] == '-')
                    s = "-" + s;
                if (_voc[_voc.Length - 1] == '-')
                    s = s + "-";
                await Navigation.PushAsync(new SingleRootPage(s));
                
            }

        }

        private async void VocList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = (e.SelectedItem as wod);
            if (item != null) VocList.SelectedItem = null;
            if (item != null && item.voc != "")
            {
                await Navigation.PushAsync(new SingleVocPage(item.voc));
                
            }
        }
        
        public class MyPopupPage : PopupPage
        {
            ContentPage back_page;
            public MyPopupPage(SingleRootPage page)
            {
                back_page = page;
                var input1 = new Entry { HorizontalOptions = LayoutOptions.FillAndExpand, };
                input1.Text = Voc.GetRootExpOrg(page._voc);
                var st = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Margin = 30,
                    Children = {
                    new Label { Text = "\n修改解釋" },
                    new StackLayout {
                        Orientation = StackOrientation.Vertical,Children= {
                            input1,
                            new Button {
                                Text ="確認",
                                Command = new Command(async()=> {
                                    string _voc=page._voc;
                                    if (_voc[0] == '-' && _voc[_voc.Length - 1] == '-')
                                        Voc.root.add(_voc.Substring(1, _voc.Length - 2),input1.Text);
                                    else if (_voc[0] != '-' && _voc[_voc.Length - 1] == '-')
                                        Voc.prefix.add(_voc.Substring(0, _voc.Length - 1),input1.Text);
                                    else if (_voc[0] == '-' && _voc[_voc.Length - 1] != '-')
                                        Voc.suffix.add(_voc.Substring(1, _voc.Length - 1),input1.Text);
                                    await PopupNavigation.PopAsync();
                                }),
                            }
                        }
                    },
                }
                };
                Content = st;
            }

            protected override void OnAppearing()
            {
                back_page.Content.IsVisible = false;
                base.OnAppearing();
            }

            protected override void OnDisappearing()
            {
                base.OnDisappearing();
                back_page.Content.IsVisible = true;
            }

            protected override bool OnBackButtonPressed()
            {
                // Prevent hide popup
                //return base.OnBackButtonPressed();
                return true;
            }
        }
    }

}
