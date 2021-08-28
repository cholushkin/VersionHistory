using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VersionHistory
{
    public class VersionHistoryScriptableObject : ScriptableObject
    {
        [Serializable]
        public class Version
        {
            public string Name;
            public TextAsset Text;
        }
        public List<Version> Versions;
    }
}
