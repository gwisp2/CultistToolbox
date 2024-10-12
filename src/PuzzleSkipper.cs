using System.Linq;
using System.Reflection;
using FFG.MoM;
using UnityEngine;

namespace MoMEssentials;

public class PuzzleSkipper : MonoBehaviour
{
    // Get method invoker for OnPuzzleCompleted in PuzzleViewBase using Harmony library
    private static readonly MethodInfo OnPuzzleCompleted = typeof(PuzzleViewBase)
        .GetMethod("PuzzleClosedEvent", BindingFlags.Instance | BindingFlags.NonPublic);

    private void Update()
    {
        if (Plugin.ConfigSkipPuzzleShortcut.Value.IsDown())
        {
            var controller = UI.Utilities.FindComponents<PuzzleViewController>().FirstOrDefault();
            var currentPuzzle = controller?.CurrentPuzzle;
            if (currentPuzzle)
            {
                OnPuzzleCompleted.Invoke(currentPuzzle, [true]);
            }
        }
    }
}