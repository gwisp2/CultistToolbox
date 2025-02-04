using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CultistToolbox.Patch;
using FFG.MoM;
using FFG.MoM.Actions;
using UnityEngine;

namespace CultistToolbox.UI.Tabs;

public class CutscenesTab() : ToolboxTab("Cutscenes")
{
    private class CutsceneInfo
    {
        public Cutscene Cutscene;
        public bool IsVictory;

        protected bool Equals(CutsceneInfo other)
        {
            return Cutscene == other.Cutscene && IsVictory == other.IsVictory;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CutsceneInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Cutscene * 397) ^ IsVictory.GetHashCode();
            }
        }
    }

    private Vector2 _scroll;
    private List<CutsceneInfo> _cutscenesInScenario = [];
    private const int ButtonsPerRow = 3;

    public override void OnScenarioLoaded()
    {
        var gameOverActions = Utilities.EnumerateAllActions<SetGameOver>();
        _cutscenesInScenario = gameOverActions
            .Select(a => new CutsceneInfo() { Cutscene = a.EndCutscene, IsVictory = a.IsVictory?.Value ?? false })
            .Distinct()
            .ToList();
    }

    public override void OnScenarioShutdown()
    {
        _cutscenesInScenario = [];
    }

    public override void Render()
    {
        if (!HookScenarioLoadUnload.ScenarioLoaded) return;

        // Show cutscenes used in this scenario
        GUILayout.Label("Cutscenes in this scenario");
        foreach (var cutsceneInfo in _cutscenesInScenario)
        {
            var cutsceneName = Enum.GetName(typeof(Cutscene), cutsceneInfo.Cutscene);
            var victorySuffix = cutsceneInfo.IsVictory ? " (victory)" : " (defeat)";
            if (GUILayout.Button(cutsceneName + victorySuffix))
            {
                ShowCutsceneLater(cutsceneInfo.Cutscene);
            }
        }

        // Show all cutscenes
        GUILayout.Label("All cutscenes");
        _scroll = GUILayout.BeginScrollView(_scroll);
        var cutscenes = Enum.GetValues(typeof(Cutscene)).Cast<Cutscene>().ToList();
        ShowCutsceneButtons(cutscenes, ButtonsPerRow);
        GUILayout.EndScrollView();
    }

    private void ShowCutsceneButtons(List<Cutscene> cutscenes, int buttonsPerRow)
    {
        int nRows = cutscenes.Count / buttonsPerRow + (cutscenes.Count % buttonsPerRow > 0 ? 1 : 0);
        for (int i = 0; i < nRows; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < buttonsPerRow && i * buttonsPerRow + j < cutscenes.Count; j++)
            {
                var cutscene = cutscenes[i * buttonsPerRow + j];
                var cutsceneName = Enum.GetName(typeof(Cutscene), cutscene);
                if (GUILayout.Button(cutsceneName))
                {
                    ShowCutsceneLater(cutscene);
                }
            }

            GUILayout.EndHorizontal();
        }
    }

    private void ShowCutsceneLater(Cutscene cutscene)
    {
        var window = Plugin.PluginObject.GetComponent<ToolboxWindow>();
        window.StartCoroutine(ShowCutscene(cutscene));
    }

    private IEnumerator ShowCutscene(Cutscene cutscene)
    {
        yield return new WaitForFixedUpdate();
        MoM_LevelLoader.EndGame(cutscene);
    }
}