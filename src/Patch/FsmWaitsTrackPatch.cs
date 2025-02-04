using System.Collections.Generic;
using System.Linq;
using FFG.MoM.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace CultistToolbox.Patch;

[HarmonyPatch]
public class FsmWaitsTrackPatch
{
    private static readonly HashSet<FsmStateAction> CurrentWaits = new();

    public static List<FsmStateAction> CurrentWaitsList
    {
        get
        {
            foreach (var fsmStateAction in CurrentWaits.ToList())
            {
                if (!fsmStateAction.Active) CurrentWaits.Remove(fsmStateAction);
            }

            return CurrentWaits.ToList();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaitForRoundDelay), "OnEnter")]
    [HarmonyPatch(typeof(WaitFor), "OnEnter")]
    [HarmonyPatch(typeof(WaitForCounter), "OnEnter")]
    public static void PostRegisterHandler(FsmStateAction __instance)
    {
        CurrentWaits.Add(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameData), "Shutdown")]
    public static void PostShutdown()
    {
        CurrentWaits.Clear();
    }
}