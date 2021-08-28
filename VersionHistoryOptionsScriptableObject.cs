using UnityEditor;
using UnityEngine;

namespace VersionHistory
{
    [CreateAssetMenu(fileName = "VersionHistoryOptions", menuName = "ScriptableObjects/VersionHistoryOptionsScriptableObject", order = 1)]
    public class VersionHistoryOptionsScriptableObject : ScriptableSingleton<VersionHistoryOptionsScriptableObject>
    {
        [Tooltip("User change lists (CL) have to be placed here. User generate CL using py scripts from git")]
        public string UserCLDirectory; // all txt with CL should be placed here

        [Tooltip("Directory where is resulting scriptable object with version history stored ")]
        public string OutputDirectory; // output dir for scriptable objects


        void Reset()
        {
            UserCLDirectory = "Assets/Game/VersionHistory/ChangeLists";
            OutputDirectory = "Assets/Game/VersionHistory";
        }
    }
}
