using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPTools.Themes
{
    public sealed partial class ThemeIcon : UserControl
    {
        public ThemeIcon(ResourceDictionary res1, ResourceDictionary res2)
        {
            this.InitializeComponent();

            // Background
            (MyCanvas.Children[0] as Polygon).Fill = new SolidColorBrush(((Color)res1["SystemChromeAltMediumHighColor"]));
            (MyCanvas.Children[1] as Polygon).Fill = new SolidColorBrush(((Color)res2["SystemChromeAltMediumHighColor"]));

            // Text
            (MyCanvas.Children[2] as TextBlock).Foreground = new SolidColorBrush(((Color)res1["SystemAltHighColor"]));

            //Block 1, 2, 4 left
            foreach (var i in new int[] { 3, 7, 10 })
                (MyCanvas.Children[i] as Polygon).Fill = new SolidColorBrush(((Color)res1["SystemBaseMediumColor"]));

            //Block 1, 2, 4 right
            foreach (var i in new int[] { 4, 8, 9 })
                (MyCanvas.Children[i] as Polygon).Fill = new SolidColorBrush(((Color)res2["SystemBaseMediumColor"]));

            //Accent block
            (MyCanvas.Children[5] as Polygon).Fill = new SolidColorBrush(((Color)res1["SystemAccentColor"]));
            (MyCanvas.Children[6] as Polygon).Fill = new SolidColorBrush(((Color)res2["SystemAccentColor"]));

            foreach (var i in new int[] { 0, 3, 7, 10, 5 })
                (MyCanvas.Children[i] as FrameworkElement).RequestedTheme = ElementTheme.Light;

            foreach(var i in new int[] { 1, 2, 4, 8, 9, 6})
                (MyCanvas.Children[i] as FrameworkElement).RequestedTheme = ElementTheme.Dark;
        }
    }
}
