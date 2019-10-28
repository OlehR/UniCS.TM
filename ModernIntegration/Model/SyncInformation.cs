﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModernIntegration.Model
{
    public enum SyncStatus
    {
        StartedPartialSync = 1,
        StartedFullSync = 2,
        SyncFinishedSuccess = 3,
        SyncFinishedError = 4,
        SyncReceiptSaved =5
    }

    public class SyncInformation
    {
        public SyncStatus Status { get; set; }

        public string StatusDescription { get; set; }
        public object SyncData { get; set; }
    }
}
