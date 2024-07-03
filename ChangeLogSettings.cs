using System;
using UnityEngine;


namespace VersionHistory
{
    [Serializable]
    public class ChangeLogSettings
    {
        public bool GenerateMarkdownChangeLog;
        public string  MarkdownChangeLogPath;
    }
}
