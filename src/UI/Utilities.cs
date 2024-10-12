using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoMEssentials.UI;

public static class Utilities
{
    public static Vector2 GetMousePosition()
    {
        Vector2 mousePosition = UnityInput.Current.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;
        return mousePosition;
    }

    public static IEnumerable<T> EnumerateAllActions<T>()
    {
        return EnumerateAllFsm().SelectMany(fsm => fsm.States).SelectMany(s => s.Actions).Where(a => a is T).Cast<T>();
    }

    private static IEnumerable<Fsm> EnumerateAllFsm()
    {
        return FindComponents<PlayMakerFSM>().Select(component => component.Fsm).Where(fsm => fsm != null);
    }

    public static IEnumerable<T> FindComponents<T>()
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

                foreach (var component in gameObject.GetComponentsInChildren<T>(true))
                {
                    yield return component;
                }
            }
        }
    }
}