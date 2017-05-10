using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Cut
{
    public class stcell : ViewCell
    {
        Label _voc, _exp;
        static public new double Height = 40.5;
        public static readonly BindableProperty vocProperty = BindableProperty.Create("voc", typeof(string), typeof(stcell), "voc");
        public static readonly BindableProperty expProperty = BindableProperty.Create("exp", typeof(string), typeof(stcell), "exp");
        public string voc
        {
            get { return (string)GetValue(vocProperty); }
            set { SetValue(vocProperty, value); }
        }

        public string exp
        {
            get { return (string)GetValue(expProperty); }
            set { SetValue(expProperty, value); }
        }

        public stcell()
        {
            _voc = new Label {
                FontSize = 18 ,
                Margin = new Thickness(0,0,10,0),
                VerticalOptions = LayoutOptions.Center,
            };
            if (Device.OS == TargetPlatform.Android)
                _voc.TextColor = Color.FromRgb(135 ,206 , 250);
            _exp = new Label {
                FontSize = 18,
                LineBreakMode = LineBreakMode.TailTruncation,
                VerticalOptions = LayoutOptions.Center
            };
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star) },
                },
                Padding = new Thickness(10, 0, 0, 0),
                HeightRequest = Height-0.5
            };
            grid.Children.Add(_voc, 0, 0);
            grid.Children.Add(_exp, 1, 0);
            View = grid;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                _voc.Text = voc;
                _exp.Text = exp;
            }
        }
    }
    public class wod
    {
        public string voc { get; private set; }
        public string exp { get; private set; }
        public wod(string _voc, string _exp)
        {
            voc = _voc;
            exp = _exp;
        }
    }
}
