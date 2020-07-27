using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Settings;

namespace ROR2ModManager
{
    class ApplicationSettings
    {
        //public static LocalSettingsValue<bool> ShowUpdateButtonInNavBar = new LocalSettingsValue<bool>("showUpdateInNavBar", true);
        public static LocalSettingsValue<bool> UpdateAppAutomatically = new LocalSettingsValue<bool>("AutoUpdateEnabled", true);
        public static LocalSettingsValue<int> LastNavigationUpdateButtonTeachingTipShown = new LocalSettingsValue<int>("LastNavigationUpdateButtonTeachingTipShown", 0);
        public static LocalSettingsValue<bool> UseMarqueeEffectForMods = new LocalSettingsValue<bool>("UseMarqueeEffectForMods", true);

        // Teaching tip is triggered when value is 0. The value is incremented up to 5 if AutoUpdates are off, else 10
        public static void Increment_LastNavigationUpdateButtonTeachingTipShown() => LastNavigationUpdateButtonTeachingTipShown.Value = (LastNavigationUpdateButtonTeachingTipShown.Value + 1) % (UpdateAppAutomatically.Value ? 10 : 5);
    }
}
