using System.Linq;
using HutongGames.PlayMaker;
using MoMEssentials.FsmTools;
using UnityEngine;

namespace MoMEssentials.UI;

public class FsmToolsUI : Renderable
{
    private WindowController _window;
    private Vector2 _scrollPosition;

    public FsmToolsUI()
    {
        _window = new WindowController(Windows.FsmToolsWindowId, "FSM Tools", this.DrawWindowContent,
            Windows.FsmToolsWindowRect);
    }

    private void DrawWindowContent()
    {
        if (!GameData.IsInitialized) return;

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        // Get all FSMs
        var fsms = Utilities.FindComponents<PlayMakerFSM>().Select(f => f.Fsm).Where(f => f != null);
        var scanner = new VariableScanner();
        scanner.ScanAll(fsms);
        var variables = scanner.GetVariables();
        foreach (var keyValuePair in variables.OrderBy(kv => kv.Key.Name))
        {
            VariableScanner.VariableInfo variableInfo = keyValuePair.Value;
            var variable = variableInfo.Variable;
            GUILayout.Label(
                $"{keyValuePair.Key.Name}: {variable.VariableType} = {variable.RawValue} / {variableInfo.GetUsageTypes().ToFlagString()}");
            foreach (var variableInfoUsage in variableInfo.Usages)
            {
                if (variableInfoUsage.UsageTypes.HasFlag(VariableUsageTypes.Unknown))
                {
                    var action = (FsmStateAction)variableInfoUsage.Action;
                    GUILayout.Label($"{action.GetType().FullName}");
                }
            }
        }

        GUILayout.EndScrollView();
    }

    public override void RenderFirstPass()
    {
        _window.RenderWindow();
    }
}