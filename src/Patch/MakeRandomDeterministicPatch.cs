using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CultistToolbox.DeterministicRandom;
using FFG.MoM;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Random = UnityEngine.Random;

namespace CultistToolbox.Patch;

[HarmonyPatch]
public class MakeRandomDeterministicPatch
{
    private static readonly MethodInfo MyGetRandomElement =
        typeof(DeterministicRandomFacade).GetMethod(nameof(DeterministicRandomFacade.GetRandomElement));

    private static readonly MethodInfo MyGetRandomElementAndRemove =
        typeof(DeterministicRandomFacade).GetMethod(nameof(DeterministicRandomFacade.GetRandomElementAndRemove));

    private static readonly MethodInfo MyGetRandomWeightedIndex =
        typeof(DeterministicRandomFacade).GetMethod(nameof(DeterministicRandomFacade.GetRandomWeightedIndex));

    private static readonly MethodInfo MyRange =
        typeof(DeterministicRandomFacade).GetMethod(nameof(DeterministicRandomFacade.Range));

    private static readonly MethodInfo MyGetRandomWeightedIndexF =
        typeof(DeterministicRandomFacade).GetMethod(nameof(DeterministicRandomFacade.GetRandomWeightedIndexF));

    private static readonly Dictionary<(Type, string), MethodInfo> MethodRewrites = new()
    {
        { (typeof(FFGTools), "GetRandomElement"), MyGetRandomElement },
        { (typeof(FFGTools), "GetRandomElementAndRemove"), MyGetRandomElementAndRemove },
        { (typeof(FFGTools), "GetRandomWeightedIndex"), MyGetRandomWeightedIndex },
        { (typeof(Random), "Range"), MyRange },
        { (typeof(ActionHelpers), "GetRandomWeightedIndex"), MyGetRandomWeightedIndexF },
    };

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        bool found = false;
        foreach (var codeInstruction in instructions)
        {
            if (codeInstruction.opcode == OpCodes.Call)
            {
                MethodInfo operand = (MethodInfo)codeInstruction.operand;
                if (MethodRewrites.TryGetValue((operand.DeclaringType, operand.Name), out var rewrite))
                {
                    found = true;
                    var myMethod = operand.GetGenericArguments().Length == 0
                        ? rewrite
                        : rewrite.MakeGenericMethod(operand.GetGenericArguments());

                    Plugin.Logger.LogDebug($"Patched {original}: replaced call of {operand}");
                    yield return new CodeInstruction(OpCodes.Call, myMethod);
                    continue;
                }
            }

            yield return codeInstruction;
        }

        if (!found)
        {
            Plugin.Logger.LogError($"Failed to find method calls to rewrite in {original}");
        }
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return SymbolExtensions.GetMethodInfo(() => MoM_ItemManager.GetRandomItem(null, 0, 0, 0));
        yield return SymbolExtensions.GetMethodInfo(() => MoM_MapTileManager.GetRandomCurrentTile());
        yield return AccessTools.DeclaredMethod(typeof(MoM_LocalizationPacket), "ResolveInsert");
        yield return AccessTools.DeclaredMethod(typeof(MoM_InvestigatorManager), "GetRandomInvestigator");
        yield return AccessTools.DeclaredMethod(typeof(MoM_RoomManager), "GetRandomCurrentRoom");
        yield return AccessTools.DeclaredMethod(typeof(MoM_MonsterManager), "GetRandomMonster",
            [typeof(IEnumerable<MonsterModel>), typeof(MonsterTraits), typeof(MonsterTraits)]);
        yield return AccessTools.DeclaredMethod(typeof(RandomEvent), "OnEnter");
        yield return AccessTools.DeclaredMethod(typeof(RandomEvent), "GetRandomEvent");
        yield return AccessTools.DeclaredMethod(typeof(SendRandomEvent), "OnEnter");
    }
}