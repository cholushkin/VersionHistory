using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
