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
    public class SingleVocPage : ContentPage
    {

        internal StackLayout
            voc_root = new StackLayout { Orientation = StackOrientation.Horizontal },
            voc_croot = new StackLayout { Orientation = StackOrientation.Horizontal },
            //expst = new StackLayout(),
            pics = new StackLayout();
            //alias_list = new StackLayout();
        //VocList = new StackLayout();
        ObservableCollection<wod> VocList_Items, alias_list_Items;
        ListView VocList=new ListView { }, alias_list = new ListView { };
        internal ContentView
            expst = new ContentView(),
            sent = new ContentView();
        internal ScrollView
            note_view = new ScrollView();
        internal Tuple<string, List<int>> wds;
        internal Label tivoc= new Label();
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
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    //new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) }
                },
            };
        Label kk = new Label {FontSize=18,Text=" "};
        string media_uri,_voc;
        bool first = true;
        internal void Init(string data)
        {
            tivoc.Text = data;
            tivoc.FontSize = 26;
            if (Device.OS == TargetPlatform.Android)    
                tivoc.TextColor = Color.FromRgb(255,255,204);
            wds = Voc.GetExp(Voc.words.val(data));
            expst.Content = Voc.ExpStack(wds.Item1);
            if (first)
            {
                if(Voc.setting["network_sound"]!="false")Task.Run(async () =>
                {
                    var s = await Voc.GetAsync(Voc.setting.val("sound_url") + data);
                    if (s == null) return;
                    var be = s.IndexOf(Voc.setting.val("sound_url2"));
                    if (be == -1) return;
                    s = s.Substring(be);
                    var ed = s.IndexOf(Voc.setting.val("sound_type"));
                    if (ed == -1) return;
                    s = s.Substring(0, ed + 4);
                    while (true)
                    {
                        var pos = s.Substring(1).IndexOf(Voc.setting.val("sound_url2"));
                        if (pos == -1) break;
                        s = s.Substring(pos + 1);
                    }
                    media_uri = s;
                    Device.BeginInvokeOnMainThread(() => {
                        ToolbarItems[4].Icon = "volume.png";
                        if (Voc.setting["auto_play"] == "On")
                            DependencyService.Get<IAudio>().PlayMp3File(media_uri);
                    });
                });
                pics.Children.Clear();
                if (Voc.setting["network_picture"] != "false")Task.Run(async () =>
                {
                    var s = await Voc.GetAsync("https://www.bing.com/images/search?q=" + data);
                    var v = new List<string>();
                    if (s == null) return ;
                    for (int i = 0; i < 4; i++)
                    {
                        var be = s.IndexOf(".mm.bing.net/");
                        if (be == -1 || be - 25 < 0) return ;
                        s = s.Substring(be - 25);
                        be = s.IndexOf("http");
                        if (be == -1) return ;
                        s = s.Substring(be);
                        var ed = s.IndexOf("\"");
                        if (ed == -1) return ;
                        v.Add(s.Substring(0, ed));
                        s = s.Substring(ed);
                    }
                    Device.BeginInvokeOnMainThread(() => {
                        foreach (var ss in v)
                            pics.Children.Add(new Image
                            {
                                Source = ss,
                                Margin = new Thickness(10, 0, 0, 0),
                                HeightRequest = 150,
                            });
                    });
                });
                if (Voc.setting["network_kk"] != "false")Task.Run(async () => 
                {
                    var web = await Voc.GetAsync("http://tw.dictionary.search.yahoo.com/search?p=" + data);
                    if (web == null) return;
                    int len = web.Length;
                    var beg = web.IndexOf(">KK[");
                    if (beg == -1) return;
                    web = web.Substring(beg);
                    beg = web.IndexOf("[");
                    if (beg == -1) return;
                    web = web.Substring(beg);
                    beg = web.IndexOf("]");
                    if (beg == -1) return;
                    web = web.Substring(0, beg + 1);
                    Device.BeginInvokeOnMainThread(() => {
                        kk.Text = web;
                    });
                });
                if (Voc.setting["network_sent"] != "false")Task.Run(async () =>
                {
                    var web = await Voc.GetAsync("http://cn.bing.com/dict/search?mkt=zh-cn&q=" + data);
                    
                    string str="", tmp="";
                    while (true)
                    {
                        var beg = web.IndexOf("<div class=\"sen_en\">");
                        if (beg == -1) break;
                        web = web.Substring(beg);
                        //ShowMsg(web);
                        beg = web.IndexOf("</div>");
                        if (beg == -1) break;
                        tmp = web.Substring(0, beg);
                        bool inhtml = false;
                        foreach (var  x in tmp)
                            if (x == '<') inhtml = true;
                            else if (x == '>') inhtml = false;
                            else if (!inhtml)
                            {
                                if (x == '[') str += '(';
                                else if (x == '\n') str += ' ';
                                else if (x == ']') str += ')';
                                else str += x;
                            }
                        str += "\n";
                        beg = web.IndexOf("<div class=\"sen_cn\">");
                        if (beg == -1) break;
                        web = web.Substring(beg);
                        //ShowMsg(web);
                        beg = web.IndexOf("</div>");
                        if (beg == -1) break;
                        tmp = web.Substring(0, beg);
                        inhtml = false;
                        var cn = "";
                        foreach (var  x in tmp)
                        {
                            if (x == '<') inhtml = true;
                            else if (x == '>') inhtml = false;
                            else if (!inhtml)
                            {
                                if (x == '[') cn += '(';
                                else if (x == '\n') cn += ' ';
                                else if (x == ']') cn += ')';
                                //else if (x == '“')str += L"「";
                                //else if (x == '”')str += L"」";
                                else cn += x;
                            }
                        }
                        str += Voc.s2t(cn);
                        str += "\n";
                    }
                    str=str.Replace("“", "「");
                    str = str.Replace("”", "」");
                    str = Voc.html_decode(str);
                    /*
                    str = str.Replace("&nbsp;", " ");
                    str = str.Replace("&lt;", "<");
                    str = str.Replace("&gt;", ">");
                    str = str.Replace("&amp;", "&");
                    str = str.Replace("&quot;", "\"");
                    str = str.Replace("&apos;", "'");
                    */

                    Device.BeginInvokeOnMainThread(() => {
                        var sentst=new StackLayout { Orientation = StackOrientation.Horizontal };
                        sentst.Orientation = StackOrientation.Vertical;
                        sentst.Margin = new Thickness(10, 0, 0, 0);
                        var stre=str.Split('\n');
                        for (int i = 0; i+1 < stre.Length; i += 2){
                            var t1 = stre[i];
                            var t2 = stre[i + 1];
                            if (t1 == "" || t2 == "") continue;
                            var stre2 = t1.Split(' ');
                            var stp = new StackLayout { Orientation = StackOrientation.Horizontal };
                            foreach (var t3 in stre2) {
                                if (t3 == "") continue;
                                var tx = new Label {
                                    FontSize = 18,
                                    Margin =new Thickness(5,15,0,0),
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    VerticalTextAlignment = TextAlignment.Center,
                                    Text = t3
                                };
                                var tgr = new TapGestureRecognizer();
                                tgr.Tapped += Sent_TappedAsync;
                                tx.GestureRecognizers.Add(tgr);
                                stp.Children.Add(tx);
                            }
                            sentst.Children.Add(stp);
                            stp = new StackLayout { Orientation = StackOrientation.Horizontal };
                            stp.Children.Add(new Label
                            {
                                FontSize = 16,
                                Margin = new Thickness(10, 0, 0, 0),
                                HorizontalTextAlignment = TextAlignment.Center,
                                VerticalTextAlignment = TextAlignment.Center,
                                //TODO
                                Text = t2
                            });
                            sentst.Children.Add(stp);
                        }
                        sent.Content = sentst;
                    });
                });

            }
            //Web Data Begin
            //Web Data End
            var ve = Voc.Show(data);
            //var ve = new List<Tuple<string, string>>();
            //var tp = new List<Picker>();
            //var tmp = new List<Label>();
            voc_root.Children.Clear();
            voc_croot.Children.Clear();
            foreach (var x in ve)
            {
                var tmp = new Label {
                    Text=x.Item2,
                    Margin=new Thickness(10,0,10,0),
                    FontSize=20,
                };
                var tgr = new TapGestureRecognizer();
                tgr.Tapped += Tgr_Tapped ;
                tmp.GestureRecognizers.Add(tgr);
                voc_root.Children.Add(tmp);
            }
            int id = 0;
            foreach (var x in ve)
            {
                var tmp = new Picker {
                    Margin = new Thickness(10, 5, 10, 5),
                    //TODO : Font Size
                };
                var vw = Voc.CutExp(x.Item1);
                foreach (var y in vw)
                    tmp.Items.Add(y);
                if (wds.Item2.Count == id)
                    wds.Item2.Add(0);
                if (wds.Item2[id] >= tmp.Items.Count)
                    wds.Item2[id] = tmp.Items.Count - 1;
                tmp.SelectedIndex = wds.Item2[id];
                tmp.SelectedIndexChanged += Tmp_SelectedIndexChanged;
                voc_croot.Children.Add(tmp);
                id++;
            }
            if (Voc.favorite.exists(data))
                ToolbarItems[0].Icon = "unfavorite.png";
            else
                ToolbarItems[0].Icon = "favorite.png";
            string _voc = data, _exp = Voc.GetExpSimple(Voc.words.val(_voc));


            if (first)
            {
                VocList_Items = new ObservableCollection<wod>();
                VocList.ItemsSource = VocList_Items;
                var customCell = new DataTemplate(typeof(stcell));
                customCell.SetBinding(stcell.vocProperty, "voc");
                customCell.SetBinding(stcell.expProperty, "exp");
                VocList.ItemTemplate = customCell;
                VocList.ItemSelected += VocList_ItemSelectedAsync;
                VocList.HasUnevenRows = true;

                alias_list_Items = new ObservableCollection<wod>();
                alias_list.ItemsSource = alias_list_Items;
                alias_list.ItemTemplate = customCell;
                alias_list.ItemSelected += Alias_list_ItemSelectedAsync; ;
                alias_list.HasUnevenRows = true;
            }
            if (first)
                VocList_Items.Add(new wod("", "讀取中..."));
            VocList.HeightRequest = VocList_Items.Count * stcell.Height;
            alias_list.HeightRequest = alias_list_Items.Count * stcell.Height;

            Task.Run(() =>
            {
                List<string> lis = new List<string>();
                foreach (var x in Voc.words.data)
                    if (x.Key != _voc && Voc.GetExpSimple(x.Value) == _exp)
                        lis.Add(x.Key);
                Device.BeginInvokeOnMainThread(() => {
                    alias_list_Items.Clear();
                    foreach (var x in lis)
                        alias_list_Items.Add(new wod(x, ""));
                    alias_list.HeightRequest = alias_list_Items.Count * stcell.Height;
                });
            });

            Task.Run(() =>
            {
                if (data.Length == 1 || (data.Split(' ').Length) != 1)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        VocList_Items.Clear();
                        VocList.HeightRequest = VocList_Items.Count * stcell.Height;
                    });
                    return;
                }
                string reg_string = "";
                if (data.Length > 2 && data[data.Length - 1] == 'e')
                    reg_string = ".*" + data.Substring(0, data.Length - 1) + ".*";
                else
                    reg_string = ".*" + data + ".*";
                Regex reg = new Regex(reg_string, RegexOptions.IgnoreCase);
                List<string> lis = new List<string>();
                foreach (var x in Voc.words.data)
                {
                    if (!reg.IsMatch(x.Key) || x.Key == data) continue;
                    var s2 = Voc.Show2(x.Key);
                    foreach (var y in s2)
                        if (y == data)
                        {
                            lis.Add(x.Key);
                            break;
                        }
                }
                Device.BeginInvokeOnMainThread(() => {
                    VocList_Items.Clear();
                    foreach (var x in lis)
                        VocList_Items.Add(new wod(x, Voc.GetExpSimple(Voc.words.val(x)).Trim()));
                    VocList.HeightRequest = VocList_Items.Count * stcell.Height;
                });
            });
            //if (first)
            //VocList.Children.Add(new Label { Text = "讀取中..." });
            
            var tb = new Label {
                Margin=10,
                FontSize=16,
            };
            if (Voc.note.exists(data))
                tb.Text = Voc.note.val(data);
	        else
                tb.Text = "備註(點擊修改)";
            var tb_tgr = new TapGestureRecognizer();
            tb_tgr.Tapped += Tb_tgr_Tapped; ;
            tb.GestureRecognizers.Add(tb_tgr);
            note_view.Content = tb;
            pics.Orientation = StackOrientation.Horizontal;
            first = false;
        }

        private async void Sent_TappedAsync(object sender, EventArgs e)
        {
            var tmp = sender as Label;
            if (tmp == null) return;
            var s = tmp.Text;
            if (s == null||s=="") return;
            var x = s.IndexOfAny("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
            if (x == -1) return;
            s =s.Remove(0, x);
            if (s == null || s == "") return;
            x = s.LastIndexOfAny("qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
            if (x!=s.Length-1)
                s =s.Remove(x + 1);
            if (s == null || s == "") return;
            await Navigation.PushAsync(new SingleVocPage(s));
            
            //ShowMsg(tmp->Text->Data());
            //dynamic_cast<TextBox^>(note_view->Content)->Focus(Windows::UI::Xaml::FocusState::Pointer);

        }

        private async void Alias_list_ItemSelectedAsync(object sender, SelectedItemChangedEventArgs e)
        {
            var item = (e.SelectedItem as wod);
            alias_list.SelectedItem = null;
            if (item != null && item.voc != "")
            {
                await Navigation.PushAsync(new SingleVocPage(item.voc));
            }
        }

        private async void VocList_ItemSelectedAsync(object sender, SelectedItemChangedEventArgs e)
        {
            var item = (e.SelectedItem as wod);
            VocList.SelectedItem = null;
            if (item != null&&item.voc!="")
            {
                await Navigation.PushAsync(new SingleVocPage(item.voc));
            }
        }

        private void Tb_tgr_Tapped(object sender, EventArgs e)
        {
            var tb = new Entry {
                Text = Voc.note.exists(_voc) ? Voc.note.val(_voc) : "",
                Margin = 20,
                FontSize = 16,
            };
            tb.Unfocused += Tb_Unfocused;
            note_view.Content = tb;
        }

        private void Tb_Unfocused(object sender, FocusEventArgs e)
        {
            var tmp = note_view.Content as Entry;
            if (tmp.Text != Voc.note.val(_voc))
                Voc.note.add(_voc, tmp.Text);
            else if (tmp.Text==""&&Voc.note.exists(_voc))
                Voc.note.remove(_voc);
            var tb = new Label
            {
                Margin = 10,
                FontSize=16,
            };
            if (Voc.note.exists(_voc))
                tb.Text = Voc.note.val(_voc);
            else
                tb.Text = "備註(點擊修改)";
            var tb_tgr = new TapGestureRecognizer();
            tb_tgr.Tapped += Tb_tgr_Tapped; ;
            tb.GestureRecognizers.Add(tb_tgr);
            note_view.Content = tb;

        }

        private void Tmp_SelectedIndexChanged(object sender, EventArgs e)
        {
            var senderComboBox = sender as Picker;
            if (senderComboBox == null) return;
            for (int i = 0; i < voc_croot.Children.Count; i++)
                if (voc_croot.Children[i] == senderComboBox)
                {
                    wds.Item2[i] = senderComboBox.SelectedIndex;
                    Voc.words.add(_voc,Voc.MakeExp(wds));
                    break;
                }
        }

        private async void GoRootPageAsync(object sender)
        {
            var senderTextBlock = sender as Label;
            if (senderTextBlock == null) return;
            for (int i = 0; i < voc_root.Children.Count; i++)
                if (voc_root.Children[i] == senderTextBlock)
                {
                    var s = Voc.Show2(_voc)[i];
                    if (s[0] != '-' && s[s.Length - 1] != '-')
                    {
                        s = Voc.WordRotToExp(s).Item2;
                        if (Voc.words.exists(s))
                            await Navigation.PushAsync(new SingleVocPage(s));
                    }
                    else
                        await Navigation.PushAsync(new SingleRootPage(s));
                }
            return;
        }
        private void Tgr_Tapped(object sender, EventArgs e)
        {
            GoRootPageAsync(sender);
        }
        public SingleVocPage(string param)
        {
            //param.ToLower();
            param = param.Trim();
            tivoc.Text = param;
            //NavigationPage.SetHasNavigationBar(this, false);
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "favorite.png",
                Text = "最愛",
                Command = new Command(() => {
                    if (Voc.favorite.exists(_voc))
                        Voc.favorite.remove(_voc);
                    else
                        Voc.favorite.add(_voc, "");
                    Init(_voc);
                })
            });
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "refresh.png",
                Text = "重整",
                Command = new Command(async () => {
                    await RefreshAsync();
                })
            });
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
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "mute.png",
                Text = "發音",
                Command = new Command(() => { if (media_uri != null) DependencyService.Get<IAudio>().PlayMp3File(media_uri); })

            });
            Content = new ScrollView {
                Orientation = ScrollOrientation.Vertical,
                Content = grid,
                //Padding = new Thickness(10, 10, 10, 5)
            };
            grid.Margin = new Thickness(10, 10, 10, 5);
            //grid.Children.Add(tivoc, 0, 0);
            //grid.Children.Add(kk, 0, 1);
            grid.Children.Add(tivoc, 0, 0);
            grid.Children.Add(new Grid { Children = { kk }, Padding = new Thickness(0, 0, 0, 10) }, 0, 1);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = expst }, 0, 2);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = voc_root }, 0, 3);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = voc_croot }, 0, 4);
            grid.Children.Add(note_view, 0, 5);
            grid.Children.Add(alias_list, 0, 6);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = pics }, 0, 7);
            grid.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = sent }, 0, 8);
            grid.Children.Add(VocList, 0, 9);
            if (Voc.words.exists(param))
            {
                if (_voc != null)
                    Init(param);
                else
                {
                    _voc = param;
                    Init(param);
                }
            }
            else
            {
                _voc = param;
                this.Appearing += async (s,e)=> { await RefreshAsync(true); };
            }
        }
        async Task RefreshAsync(bool first=false) {
            var web = await Voc.GetAsync("http://cn.bing.com/dict/search?mkt=zh-cn&q=" + _voc);
            if (web == null || web=="") {
                await DisplayAlert("錯誤", "網路錯誤", "了解");
                return;
            }
            string npa = _voc;
            var pb = web.IndexOf("<h1><strong>");
            if (pb != -1)
            {
                var tit = web.Substring(pb + 12);
                var pe = tit.IndexOf("<");
                if (pe != -1)
                {
                    npa = tit.Substring(0, pe);
                    if (npa.ToLower()!=_voc.ToLower()) npa = _voc;
                    if (npa != _voc)
                    {
                        Voc.note.remove(_voc);
                        Voc.words.remove(_voc);
                        _voc = npa;
                    }
                }
            }
            string disc = "", nt = "";
            var betip = web.IndexOf("<div class=\"in_tip\">");
            if (betip != -1)
            {
                web = web.Substring(betip + 20);
                var edtip = web.IndexOf("</div>");
                if (edtip != -1)
                {
                    string od = web.Substring(0, edtip);
                    foreach (var x in od)
                        if (x == ',')
                            nt += "，";
                        else if (x == '/')
                            nt += "／";
                        else
                            nt += x;
                }
            }
            while (true)
            {
                var be = web.IndexOf("<span class=\"pos\">");
                if (be == -1 || be + 18 >= web.Length) break;
                web = web.Substring(be + 18);
                var ed = web.IndexOf("</span><span class=\"def\"><span>");
                if (ed == -1 || ed + 31 >= web.Length) break;
                disc += "[" + web.Substring(0, ed) + "]";
                web = web.Substring(ed + 31);
                if (ed == -1) break;
                ed = web.IndexOf("</span></");
                if (ed == -1) break;
                string tmp = web.Substring(0, ed), str = "";
                bool inhtml = false;
                foreach (var x in tmp)
                    if (x == '[') str += '(';
                    else if (x == ']') str += ')';
                    else if (x == '<') inhtml = true;
                    else if (x == '>') inhtml = false;
                    else if (!inhtml) str += x;
                disc += str + " ";
            }
            while (true)
            {
                var be = web.IndexOf("<span class=\"pos web\">");
                if (be == -1 || be + 22 >= web.Length) break;
                web = web.Substring(be + 22);
                var ed = web.IndexOf("</span><span class=\"def\"><");
                if (ed == -1 || ed + 25 >= web.Length) break;
                disc += "[" + web.Substring(0, ed) + "]";
                web = web.Substring(ed + 25);
                if (ed == -1) break;
                ed = web.IndexOf("</span></");
                if (ed == -1) break;
                string tmp = web.Substring(0, ed), str = "";
                bool inhtml = false;
                foreach (var x in tmp)
                    if (x == '[') str += '(';
                    else if (x == ']') str += ')';
                    else if (x == '<') inhtml = true;
                    else if (x == '>') inhtml = false;
                    else if (!inhtml) str += x;
                disc += str + " ";
            }
            //ShowMsg(disc);
            //Explanation = ref new String(disc.c_str());
            string disc2 = "";
            foreach (var x in disc)
                if (x == ',')
                    disc2 += '，';
                else if (x == '/')
                    disc2 += '／';
                else
                    disc2 += x;
            if (disc2 == "")
            {
                await DisplayAlert("錯誤", "查無此字", "了解");
                return;
            }
            nt = Voc.html_decode(nt);
            disc = Voc.html_decode(disc);
            disc = Voc.s2t(disc2);
            Voc.words.add(_voc, disc);
            if (nt != "")
                Voc.note.add(_voc, Voc.s2t(nt));
            Init(_voc);
        }
        public class MyPopupPage : PopupPage
        {
            ContentPage back_page;
            public MyPopupPage(SingleVocPage page)
            {
                back_page = page;
                var input1 = new Entry { HorizontalOptions = LayoutOptions.FillAndExpand, };
                var input2 = new Entry { HorizontalOptions = LayoutOptions.FillAndExpand, };
                input1.Text = "";
                bool nf = false;
                for (int i = 0; i < page.voc_root.Children.Count; i++)
                {
                    if (nf) input1.Text += " "; nf = true;
                    input1.Text += (page.voc_root.Children[i] as Label).Text;
                }
                input2.Text = Voc.GetExpSimpleOrg(Voc.words.val(page._voc));
                var st = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Margin = 30,
                    Children = {
                    new Label { Text = "應該如何拆解?" },
                    new StackLayout {
                        Orientation = StackOrientation.Vertical,Children= {
                            input1,
                            new Button {
                                Text ="確認",
                                Command = new Command(async()=> {
                                    string a=page._voc, b=input1.Text, c="";
                                    var tmp=b.Split(' ');
                                    foreach(var x in tmp)
                                        c+=x;
                                    if (c != a) {
                                        await DisplayAlert ("錯誤", "格式錯誤", "了解");
                                        return;
                                    }
                                    Voc.words.add_ok(a,b);
                                    page.Init(page._voc);
                                    await PopupNavigation.PopAsync();
                                }),
                            }
                        }
                    },
                    new Label { Text = "\n修改解釋" },
                    new StackLayout {
                        Orientation = StackOrientation.Vertical,Children= {
                            input2,
                            new Button {
                                Text ="確認",
                                Command = new Command(async()=> {
                                    string a=page._voc, b="",c=input2.Text;
                                    foreach (var x in c)
                                        if (x == ',')
                                            b += "，";
                                        else
                                            b += x;
                                    page.wds=Tuple.Create(b,page.wds.Item2);
                                    Voc.words.add(a,Voc.MakeExp(page.wds));
                                    b = Voc.GetExpSimple(Voc.words.val(a));
                                    page.Init(page._voc);
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
