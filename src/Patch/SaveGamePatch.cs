using System;
using System.Collections;
using CultistToolbox.DeterministicRandom;
using FFG.Common;
using HarmonyLib;
using Newtonsoft.Json;
using File = System.IO.File;

namespace CultistToolbox.Patch;

[HarmonyPatch]
public class SaveGamePatch
{
    public class Save
    {
        public long Timestamp;
        public DeterministicRandomSave Random;
    }

    private static readonly AccessTools.FieldRef<object, string> FilePathRef =
        AccessTools.FieldRefAccess<string>(typeof(GameSerializer), "kFilePath");

    [HarmonyPatch(typeof(GameSerializer), "CoroutineImplSaveGame", [typeof(string), typeof(string)])]
    [HarmonyPostfix]
    public static void PostCoroutineImplSaveGame(string filePath, ref IEnumerator __result)
    {
        __result = CoroutineImplSaveGameWrapper(__result);
    }

    [HarmonyPatch(typeof(GameSerializer), "CoroutineImplLoadGame")]
    [HarmonyPostfix]
    public static void PostCoroutineImplLoadGame(ref IEnumerator __result)
    {
        __result = CoroutineImplLoadGameWrapper(FilePathRef(), __result);
    }

    [HarmonyPatch(typeof(GameSerializer), "DeleteSaveFile")]
    [HarmonyPostfix]
    public static void PostDeleteSaveFile()
    {
        DeleteSave();
    }

    private static IEnumerator CoroutineImplSaveGameWrapper(IEnumerator inner)
    {
        // Run original enumerator code
        while (inner.MoveNext())
            yield return inner.Current;

        // Run postfix
        var save = new Save()
        {
            Timestamp = DetermineLastGameSaveTimestamp(),
            Random = DeterministicRandomFacade.Save()
        };
        StoreSave(save);
    }

    private static IEnumerator CoroutineImplLoadGameWrapper(string filePath, IEnumerator inner)
    {
        // Run original enumerator code
        while (inner.MoveNext())
            yield return inner.Current;

        // Run postfix
        var save = LoadSave();
        DeterministicRandomFacade.Load(save?.Random);
    }

    private static string MakeCultistToolboxSavePath()
    {
        return FilePathRef() + "-CultistToolbox.json";
    }

    private static long DetermineLastGameSaveTimestamp()
    {
        return File.GetLastWriteTime(FilePathRef()).ToFileTimeUtc();
    }

    private static void StoreSave(Save save)
    {
        if (save == null)
            return;
        File.WriteAllText(MakeCultistToolboxSavePath(), JsonConvert.SerializeObject(save, Formatting.Indented));
    }

    private static void DeleteSave()
    {
        var savePath = MakeCultistToolboxSavePath();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    private static Save LoadSave()
    {
        var savePath = MakeCultistToolboxSavePath();
        if (!File.Exists(savePath))
            return null;
        try
        {
            var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(savePath));
            if (save.Timestamp == DetermineLastGameSaveTimestamp()) return save;
            Plugin.Logger.LogError(
                $"Outdated plugin-related data from {savePath}: game was player without this plugin?");
            return null;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to load plugin-related data from {savePath}");
            Plugin.Logger.LogError(e);
            return null;
        }
    }
}