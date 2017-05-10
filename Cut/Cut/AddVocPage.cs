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
    public class AddVocPage : ContentPage
    {
        Grid grid = new Grid
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            RowDefinitions ={
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions = {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(18, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },

            }
        };
        Entry voc, exp;
        ListView list;
        public AddVocPage()
        {
            Title = "新增資料";
            var tb1 = new Label {
                Text = "字詞",
                Margin = new Thickness(15, 10, 0, 5),
                VerticalOptions = LayoutOptions.Center,
                FontSize=18
            };
            voc = new Entry { Margin = new Thickness(15, 10, 15, 5) };
            var tb2 = new Label {
                Text = "解釋",
                Margin = new Thickness(15, 10, 0, 5),
                VerticalOptions = LayoutOptions.Center,
                FontSize = 18
            };
            exp = new Entry { Margin = new Thickness(15, 10, 15, 5) };
            var list_items= new ObservableCollection<wod> {
                new wod("新增為單字",""),
                new wod("新增為字根",""),
                new wod("新增為字首",""),
                new wod("新增為字尾",""),
            };
            list = new ListView {
                ItemsSource=list_items,
            };
            var customCell = new DataTemplate(typeof(stcell));
            customCell.SetBinding(stcell.vocProperty, "voc");
            customCell.SetBinding(stcell.expProperty, "exp");
            list.ItemTemplate = customCell;

            list.HeightRequest = list_items.Count * stcell.Height;
            list.ItemSelected += List_ItemSelectedAsync;
            grid.Children.Add(tb1, 1, 2, 0, 1);
            grid.Children.Add(voc, 2, 3, 0, 1);
            grid.Children.Add(tb2, 1, 2, 1, 2);
            grid.Children.Add(exp, 2, 3, 1, 2);
            grid.Children.Add(list, 1, 3, 2, 3);
            Content = grid;
        }

        private async void List_ItemSelectedAsync(object sender, SelectedItemChangedEventArgs e)
        {
            var ite = e.SelectedItem as wod;
            if (voc.Text == "") return;
            if (ite == null) return;
            var item = ite.voc;
            if (item == "新增為單字")
            {
                Voc.words.add(voc.Text,exp.Text);
                await Navigation.PushAsync(new SingleVocPage(voc.Text));
            }
            else if (item == "新增為字根")
            {
                Voc.root.add(voc.Text, exp.Text);
                await Navigation.PushAsync(new SingleRootPage("-"+voc.Text+ "-"));
            }
            else if (item == "新增為字首")
            {
                Voc.prefix.add(voc.Text, exp.Text);
                await Navigation.PushAsync(new SingleRootPage(voc.Text + "-"));
            }
            else if (item == "新增為字尾")
            {
                Voc.suffix.add(voc.Text, exp.Text);
                await Navigation.PushAsync(new SingleRootPage("-" + voc.Text ));
            }
            if(item != null)
                list.SelectedItem = null;
        }
    }
}