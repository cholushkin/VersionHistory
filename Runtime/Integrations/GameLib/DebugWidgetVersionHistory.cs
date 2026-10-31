// todo: hook up a scroll rect auto-scroll to top when ApplyState is called
// idea: allow switching between "All" and "Included Only" directly from a toggle in the widget UI

#if GAMELIB_PRESENT
using GameLib;
using GameLib.VersionHistory.Runtime;
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetVersionHistory : DebugWidgetImageAndText
    {
        public VersionHistory.VersionHistory VersionHistory;

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            ApplyState();
        }

        public void ApplyState()
        {
            if (VersionHistory == null) return;

            string formattedText = ChangelogRuntimeTMPGenerator.Generate(VersionHistory, includeAll: false);

            SetText(formattedText, Color.white);
        }
    }
}
#endif
