using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CultistToolbox;

public static class Utilities
{
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.RandomRangeInt(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static IEnumerable<MoM_LocalizationPacket.PacketInsert> GetUsedInserts(
        this MoM_LocalizationPacket packet
    )
    {
        int nArgs = packet.CalculateArguments();
        return packet.Inserts.Where((_, index) => index < nArgs);
    }

    public static IEnumerable<MoM_LocalizationPacket.PacketInsert> GetUsedInserts(
        this MoM_LocalizationPacket packet,
        LocalizationFilterType type
    )
    {
        return packet.GetUsedInserts().Where(insert => insert.Filter == type);
    }

    public static string GetProductIcons(ProductModel model)
    {
        return Localization.Get("ICON_PRODUCT_" + model.ProductCode) ?? model.ProductCode;
    }

    public static IEnumerable<T> EnumerateAllActions<T>()
    {
        return EnumerateAllFsm().SelectMany(fsm => fsm.States).SelectMany(s => s.Actions).Where(a => a is T).Cast<T>();
    }

    private static IEnumerable<Fsm> EnumerateAllFsm()
    {
        return FindComponents<PlayMakerFSM>().Select(component => component.Fsm).Where(fsm => fsm != null);
    }

    public static IEnumerable<T> FindComponents<T>(bool includeInactive = true)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var sceneObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
            foreach (var gameObject in sceneObjects)
            {
                var componentInRootObject = gameObject.GetComponent<T>();
                if (componentInRootObject != null)
                {
                    yield return componentInRootObject;
                }

                foreach (var component in gameObject.GetComponentsInChildren<T>(includeInactive))
                {
                    yield return component;
                }
            }
        }
    }
}