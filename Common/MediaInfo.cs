using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Centapp.CartoonCommon
{
    class MediaInfo
    {
        public decimal RequiredGigaBytes { get; set; }
        public bool IsBackupAvailable { get; set; }
        public object AvailableGigaBytes { get; set; }
    }
}
