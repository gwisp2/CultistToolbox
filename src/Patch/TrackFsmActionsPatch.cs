using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFG.MoM.Actions;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace CultistToolbox.Patch;

/**
 * Patch that allows to do something when OnEnter and OnExit methods of IFsmStateAction are called.
 * Used to track context for deterministic random.
 */
[HarmonyPatch]
public class TrackFsmActionsPatch
{
    public static event Action<IFsmStateAction> OnActionMethodStart;
    public static event Action<IFsmStateAction> OnActionMethodEnd;

    public static void Patch(Harmony harmony)
    {
        harmony.PatchAll(typeof(TrackFsmActionsPatch));
    }

    public static void Prefix(IFsmStateAction __instance)
    {
        OnActionMethodStart?.Invoke(__instance);
    }

    public static void Finalizer(IFsmStateAction __instance)
    {
        OnActionMethodEnd?.Invoke(__instance);
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var method in FindAllMethodImplementations(typeof(IFsmStateAction), nameof(IFsmStateAction.OnEnter)))
        {
            yield return method;
        }

        foreach (var method in FindAllMethodImplementations(typeof(IFsmStateAction), nameof(IFsmStateAction.OnExit)))
        {
            yield return method;
        }
    }

    private static IEnumerable<MethodBase> FindAllMethodImplementations(Type baseType, string name)
    {
        var assembly = typeof(DisplayMessageBase).Assembly;
        var implementations = assembly.GetTypes()
            .Where(t => t != baseType && baseType.IsAssignableFrom(t));
        foreach (var implementation in implementations)
        {
            var methodInfo = implementation.GetMethod(name, types: []);
            if (methodInfo is null || methodInfo.DeclaringType != implementation) continue;

            if (methodInfo.IsDeclaredMember() && !implementation.IsGenericType)
            {
                yield return methodInfo;
            }
        }
    }
}