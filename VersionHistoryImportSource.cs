using UnityEngine;

namespace VersionHistory
{
    [CreateAssetMenu(fileName = "VersionHistoryImportSettings",
        menuName = "ScriptableObjects/VersionHistoryImportSettings", order = 1)]

    public class VersionHistoryImportSource : ScriptableObject
    {
        [Tooltip("Name of module we manage version history of")]
        public string ModuleName;

        [Tooltip("User change lists (CL) have to be placed here. User generate CL using py scripts from git")]
        public string UserCLDirectory; // all txt with CL should be placed here

        [Tooltip("Directory where is resulting scriptable object with version history stored ")]
        public string OutputDirectory; // output dir for scriptable objects

        void Reset()
        {
            UserCLDirectory = "Assets/Game/VersionHistory/ChangeLists";
            OutputDirectory = "Assets/Game/VersionHistory";
            ModuleName = "Game";
        }
    }
}