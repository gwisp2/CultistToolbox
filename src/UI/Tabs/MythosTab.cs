using System;
using System.Collections.Generic;
using System.Linq;
using CultistToolbox.DeterministicRandom;
using CultistToolbox.Patch;
using FFG.Common.Actions;
using FFG.MoM;
using FFG.MoM.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class MythosTab() : ToolboxTab("Mythos")
{
    private static readonly AccessTools.FieldRef<object, PlayMakerFSM> MythosEventFsmField =
        AccessTools.FieldRefAccess<PlayMakerFSM>(typeof(MoM_MythosEvent), "_fsm");

    private Vector2 _scroll = Vector2.zero;

    public override void Render()
    {
        if (!HookScenarioLoadUnload.ScenarioLoaded) return;

        // Different data
        GUILayout.Label("Round: " + GameData.Round);
        GUILayout.Label("Aggression: " + GameData.Aggression);
        GUILayout.Label("Threat: " + GameData.UnspentThreat + " unspent, " + GameData.TotalThreat + " total");

        // Next mythos
        _scroll = GUILayout.BeginScrollView(_scroll);
        RenderNearestMythos();

        // Waits
        var waits = FsmWaitsTrackPatch.CurrentWaitsList;
        foreach (var wait in waits)
        {
            RenderWaitingAction(wait);
        }

        GUILayout.EndScrollView();
    }

    private void RenderNearestMythos()
    {
        // Planned mythos events
        var mythosManager =
            AccessTools.FieldRefAccess<SingletonBehaviour<MoM_MythosManager>>(
                typeof(SingletonBehaviour<MoM_MythosManager>), "s_instance")() as MoM_MythosManager;
        if (!mythosManager) return;
        var storyEventList = mythosManager.StoryEventList;
        if (storyEventList == null) return;

        foreach (var mythosEvent in storyEventList)
        {
            var fsm = MythosEventFsmField(mythosEvent);
            var mythosName = mythosEvent.Model?.Name ?? fsm.gameObject.GetFullName();
            GUILayout.BeginVertical("Mythos event: " + mythosName, GUI.skin.box);
            GUILayout.Space(20);
            if (fsm.Fsm != null)
            {
                RenderFsmTexts(fsm.Fsm);
            }

            GUILayout.EndVertical();
        }
    }

    private void RenderFsmTexts(Fsm fsm)
    {
        if (fsm == null) return;
        foreach (var state in fsm.States)
        {
            List<string> messages = [];
            foreach (var action in state.Actions)
            {
                var message = Predictor.PredictDisplayedText(action);
                if (message == null) continue;
                messages.Add(message);
            }

            if (!messages.Any()) continue;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("State " + state.Name);
            foreach (var message in messages)
            {
                GUILayout.Label(message, Common.SmallLabelStyle.Value);
            }

            GUILayout.EndVertical();
        }
    }

    private void RenderWaitingAction(FsmStateAction action)
    {
        if (action == null) return;
        GUILayout.BeginVertical(DescribeWait(action), GUI.skin.box);
        GUILayout.Space(20);
        GUILayout.Label("State: " + action.State.Name + " from " + action.Fsm.FsmComponent.gameObject.GetFullName());
        RenderFsmTexts(action.Fsm);

        GUILayout.EndVertical();
    }

    private static string DescribeWait(FsmStateAction action)
    {
        if (action is WaitFor waitFor)
        {
            return "When " + Enum.GetName(typeof(ConditionTarget), waitFor.target) + " reaches " + waitFor.value.Value;
        }
        else if (action is WaitForRoundDelay waitForRoundDelay)
        {
            return "In " + waitForRoundDelay.Delay.Value + " round(s)";
        }
        else if (action is WaitForNew waitForNew)
        {
            var op = Enum.GetName(typeof(ConditionOperator), waitForNew.Operator);
            var leftSide = DescribeFsmInt(waitForNew.LeftSide);
            var rightSide = DescribeFsmInt(waitForNew.RightSide);
            return "When " + leftSide + " " + op + " " + rightSide;
        }
        else if (action is WaitForCounter waitForCounter)
        {
            var op = Enum.GetName(typeof(ConditionOperator), waitForCounter.conditionOperator);
            var leftSide = DescribeFsmInt(waitForCounter.variable);
            var rightSide = DescribeFsmInt(waitForCounter.value);
            return "When " + leftSide + " " + op + " " + rightSide;
        }
        else
        {
            return "Unknown wait: " + action.GetType().Name;
        }
    }

    private static string DescribeFsmInt(FsmInt fsmInt)
    {
        return string.IsNullOrEmpty(fsmInt.Name) ? fsmInt.Value.ToString() : fsmInt.Name;
    }
}