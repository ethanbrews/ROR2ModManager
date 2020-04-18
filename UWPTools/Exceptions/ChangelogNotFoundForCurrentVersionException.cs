using System;
using System.Collections.Generic;
using System.Text;

namespace UWPTools.Exceptions
{
    class ChangelogNotFoundForCurrentVersionException : ExceptionBase
    {
        public string VersionString;
    }
}
