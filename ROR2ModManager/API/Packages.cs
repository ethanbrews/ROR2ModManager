using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ROR2ModManager.API
{
    [Serializable]
    public class Version
    {
        public string name { get; set; }
        public string full_name { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string version_number { get; set; }
        public List<object> dependencies { get; set; }
        public string download_url { get; set; }
        public int downloads { get; set; }
        public DateTime date_created { get; set; }
        public string website_url { get; set; }
        public bool is_active { get; set; }
        public string uuid4 { get; set; }
    }

    [Serializable]
    public class Package : IEquatable<Package>, INotifyPropertyChanged
    {
        public string name { get; set; }
        public string full_name { get; set; }
        public string owner { get; set; }
        public string package_url { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
        public string uuid4 { get; set; }
        public int rating_score { get; set; }
        public bool is_pinned { get; set; }
        public bool is_deprecated { get; set; }
        public List<Version> versions { get; set; }

        [DefaultValue(false)]
        public bool _is_selected { get; set; }

        [DefaultValue(false)]
        public bool _is_dependency { get; set; }

        [DefaultValue(false)]
        public bool _is_selected_by_user { get; set; }

        public string _selected_version { get; set; }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            var vs = new List<System.Version>();
            foreach(var v in versions)
            {
                vs.Add(System.Version.Parse(v.version_number));
            }
            _selected_version = vs.Max().ToString();

        }

        /// <summary>
        /// Invokes [PropertyChanged] for [_selected_version], [_is_dependency] and [_is_selected]
        /// </summary>
        public void markDirty()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("_selected_version"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("_is_selected"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("_is_dependency"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("_is_selected_by_user"));
        }

        public bool Equals(Package other)
        {
            return full_name == other.full_name;
        }

        public override int GetHashCode()
        {
            return full_name.GetHashCode();
        }

        public LWPackageData ConvertToLW()
        {
            return new LWPackageData
            {
                full_name = this.full_name,
                version = this._selected_version
            };
        }

    }
}
