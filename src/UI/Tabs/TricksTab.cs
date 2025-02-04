using CultistToolbox.Patch;
using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class TricksTab() : ToolboxTab("Tricks")
{
    public override void Render()
    {
        if (!HookScenarioLoadUnload.ScenarioLoaded) return;

        if (GUILayout.Button("Skip the puzzle"))
        {
            PuzzleSkipper.SkipCurrentPuzzle();
        }
    }
}