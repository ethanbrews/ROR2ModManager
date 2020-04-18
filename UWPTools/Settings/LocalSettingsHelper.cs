using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using Windows.Storage;

namespace UWPTools.Settings
{
    class LocalSettingsValue<T>
    {
        private readonly T defaultValue;
        public string Key { private set; get; }

        public LocalSettingsValue(string key, T defaultValue)
        {
            this.Key = key;
            this.defaultValue = defaultValue;
        }

        public T Value
        {
            set => ApplicationData.Current.LocalSettings.Values[Key] = value;
            get => (T)Convert.ChangeType(ApplicationData.Current.LocalSettings.Values[Key], typeof(T));
        }
    }
}
