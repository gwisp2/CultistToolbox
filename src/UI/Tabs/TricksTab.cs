using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class TricksTab() : ToolboxTab("Tricks")
{
    public override void Render()
    {
        if (GUILayout.Button("Skip the puzzle"))
        {
            PuzzleSkipper.SkipCurrentPuzzle();
        }
    }
}