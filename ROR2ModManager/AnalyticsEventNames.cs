using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROR2ModManager
{
    class AnalyticsEventNames
    {
        public static readonly string AnalyticsToggled = "AnalyticsToggledInSettings";
        public static readonly string CrashalyticsToggled = "CrashalyticsToggledInSettings";
        public static readonly string ProfileInstalled = "NewProfileInstalled";
        public static readonly string ProfileImported = "NewProfileImported";
        public static readonly string ProfileStarted = "ProfileStarted";
        public static readonly string AppUpdateTriggeredByUser = "AppUpdateTriggeredByUser";
        public static readonly string DwrandazUpdateManager = "Dwrandaz.AutoUpdateManager";
        public static readonly string ExceptionCaught = "ExceptionCaught";
        public static readonly string ToggledModsMarquee = "ModsMarqueeEffectToggled";
    }
}
