using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPTools.Themes
{
    class ThemeManager
    {

        public Dictionary<string, Tuple<string, ResourceDictionary>> Themes { get; private set; }

        public ThemeManager() =>
            Themes = new Dictionary<string, Tuple<string, ResourceDictionary>>();

        public void AddTheme(string key, string displayName, ResourceDictionary resources) =>
            Themes.Add(key, new Tuple<string, ResourceDictionary>(displayName, resources));

        public FrameworkElement GetIconForTheme(string key)
        {
            return null;
        }
    }
}
