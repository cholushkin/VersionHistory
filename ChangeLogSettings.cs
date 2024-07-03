using System;
using UnityEngine;


namespace VersionHistory
{
    [Serializable]
    public class ChangeLogSettings
    {
        public string IntroText = "All significant updates to this product will be recorded in this file.";
        public string CompanyName = Application.companyName;
        public string CopyrightString =  "© 20xx {0}. All rights reserved.";
        public bool GenerateMarkdownChangeLog;
        public string  MarkdownChangeLogPath;
    }
}
