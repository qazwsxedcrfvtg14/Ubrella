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

namespace Cut
{
    public class TestPage1 : ContentPage
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
            Margin=5
        };
        Label title;
        StackLayout test_stp;
        int ac = 0, wa = 0;
        bool lck=false;
        string ans;
        void Init()
        {
            int cnt = 0;
            foreach (var x in Voc.favorite.data)
            {
                if (x.Value.Trim() == "")
                    cnt += 64;
                else
                    cnt += int.Parse(x.Value);
            }
            int rd = new Random().Next(cnt);
            cnt = 0;
            foreach (var x in Voc.favorite.data)
            {
                if (x.Value.Trim() == "")
                    cnt += 64;
                else
                    cnt += int.Parse(x.Value);
                if (cnt > rd)
                {
                    ans = x.Key;
                    break;
                }
            }
            if (ans == null)
            {
                title.Text = "請在我的最愛中加入單字";
                return;
            }
            //ve.RemoveRange(1, ve.Count - 1);
            //random_shuffle(ve.begin(), ve.end());

            title.Text = "正確：" + ac.ToString() + " 錯誤：" + wa.ToString();
            test_stp.Children.Clear();
            //while (!event_token_queue.empty()) event_token_queue.pop();
            
            var block = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = {
                    Voc.ExpStack(Voc.GetExpSimple(Voc.words.val(ans)))
                }
            };
            var tit = new Label
            {
                Text = ans,
                IsVisible = false
            };
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += Tgr_Tapped;
            tit.GestureRecognizers.Add(tgr);
            block.Children.Add(tit);
            var res = new Label
            {
                Text = "錯誤",
                FontSize = 16
            };
            var tgr2 = new TapGestureRecognizer();
            tgr2.Tapped += Tgr2_Tapped;
            tgr2.NumberOfTapsRequired = 2;
            res.GestureRecognizers.Add(tgr2);
            block.Children.Add(res);
            var an = new Entry();
            an.TextChanged += Ans_TextChanged;
            an.Placeholder = "請輸入答案";
            block.Children.Add(an);
            block.Margin = new Thickness(0, 15, 0, 0);
            test_stp.Children.Add(block);
        }
        private void Tgr2_Tapped(object sender, EventArgs e)
        {
            if (!lck && Voc.favorite.exists(ans))
            {
                int x;
                if (Voc.favorite.val(ans).Trim() != "")
                    x = int.Parse(Voc.favorite.val(ans));
                else
                    x = 64;
                Voc.favorite.add(ans,Math.Max(1, (int)(x + 16)).ToString());
                lck = true;
            }
            //throw new NotImplementedException();
            for (int i = 0; i < test_stp.Children.Count; i++)
            {
                var block = (test_stp.Children[i]) as StackLayout;
                var tb = (block.Children[2]) as Label;
                var tb2 = sender as Label;
                if (tb2 != tb) continue;
                var target = (block.Children[1]) as Label;
                target.IsVisible = !target.IsVisible;
            }

        }

        private async void Tgr_Tapped(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            var senderTextBlock = sender as Label;
            if (senderTextBlock == null) return;
            await Navigation.PushAsync(new SingleVocPage(senderTextBlock.Text));
        }

        private void Ans_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < test_stp.Children.Count; i++)
            {
                var block = (test_stp.Children[i] as StackLayout);
                var tb = (block.Children[3] as Entry);
                var tb2 = sender as Entry;
                if (tb2 != tb) continue;
                if ((block.Children[2] as Label).Text == "正確") ac--;
                if ((block.Children[2] as Label).Text == "錯誤") wa--;
                if ((block.Children[1] as Label).Text == tb2.Text)
                {
                    (block.Children[2] as Label).Text = "正確";
                    (block.Children[2] as Label).TextColor = Color.Green;
                    ac++;
                    if (!lck && Voc.favorite.exists(ans))
                    {
                        int x;
                        if (Voc.favorite.val(ans).Trim() != "")
                            x = int.Parse(Voc.favorite.val(ans));
                        else
                            x = 64;
                        Voc.favorite.add(ans, Math.Max(1, (int)(x / 2)).ToString());
                        lck = true;
                    }
                    Device.StartTimer(new TimeSpan(10000000), () => {
                        Navigation.PushAsync(new TestPage1(ac,wa));
                        return false;
                    });
                }
                else
                {
                    (block.Children[2] as Label).Text = "錯誤";
                    (block.Children[2] as Label).TextColor = Color.Red;
                    wa++;
                }
                title.Text = "正確：" + ac.ToString() + " 錯誤：" + wa.ToString();
                //if(block->)
            }
        }

        public TestPage1(int _ac, int _wa)
        {
            ac = _ac;
            wa = _wa;
            wa++;
            Title = "單字測驗";
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "next.png",
                Text = "下題",
                Command = new Command(() => {
                    Navigation.PushAsync(new TestPage1(ac, wa));
                })
            });
            title = new Label { Text = "正確：0 錯誤：0" ,FontSize=18}; ;
            //var tb1 = new Label { Text = "這份考卷在關掉此應用程式或是按右上重整前都不會消失喔!", HorizontalTextAlignment = TextAlignment.Center };
            var tb2 = new Label {
                Text = "想看小抄請對【正確】/【錯誤】字樣上連點兩下",
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize=15
            };
            test_stp = new StackLayout { };
            grid.Children.Add(title, 0, 0);
            //grid.Children.Add(tb1, 0, 1);
            grid.Children.Add(tb2, 0, 2);
            grid.Children.Add(test_stp, 0, 3);
            Content = new ScrollView { Content = grid };
            Init();
        }

    }
}