using System.Collections.Generic;
using System.IO;
using System.Linq;
using CultistToolbox.FsmExport;
using CultistToolbox.Patch;
using FFG.MoM;
using UnityEngine;
using UnityEngine.UIElements;

namespace CultistToolbox.UI;

public class EssentialsUI : MonoBehaviour
{
    private WindowController _menuWindow;
    private readonly ItemListUI _itemListUI = new();
    private readonly TileListUI _tileListUI = new();
    private readonly InvestigatorMagicUI _investigatorMagicUI = new();
    private readonly FsmToolsUI _fsmToolsUI = new();
    private readonly List<Renderable> _renderables = new();
    private bool _visible;
    private float _keyUnblockTime = 0;

    private void Awake()
    {
        _menuWindow = new WindowController(Windows.MenuWindowId, "Menu", DrawMenu, Windows.MenuWindowRect);
        HookScenarioLoadingCompletePatch.ScenarioLoadingComplete += OnScenarioLoadingComplete;
    }

    private void OnGUI()
    {
        if (Plugin.ConfigUiKey.Value.IsDown() && Time.unscaledTime >= _keyUnblockTime)
        {
            _visible = !_visible;
            _keyUnblockTime = Time.unscaledTime + 0.1f;
        }

        if (!_visible) return;

        _menuWindow.RenderWindow();

        for (int i = 0; i < 2; i++)
        {
            foreach (var renderable in _renderables)
            {
                renderable.RenderPass(i);
            }
        }
    }

    private void DrawMenu()
    {
        DrawToggleButton("Tiles", _tileListUI);
        DrawToggleButton("Items", _itemListUI);
        DrawToggleButton("Investigators", _investigatorMagicUI);
        DrawToggleButton("FSM tools", _fsmToolsUI);
        if (GUILayout.Button("Export FSM"))
        {
            // Get all FSMs
            var sce = new ScenarioE();
            foreach (var playMakerFsm in Utilities.FindComponents<PlayMakerFSM>())
            {
                var fsm = playMakerFsm.Fsm;
                if (fsm == null) continue;
                if (!fsm.States.Select(s => s.Actions).Any()) continue;
                sce.Fsms[playMakerFsm.gameObject.GetFullName()] = FsmE.FromFsm(fsm);
            }
            foreach (var mMapTile in Utilities.FindComponents<MoM_MapTile>())
            {
                var renderer = mMapTile.GetComponent<MeshRenderer>();
                if (renderer == null) continue;
                sce.Tiles.Add(new TileE()
                {
                    FullName = mMapTile.gameObject.GetFullName(),
                    Extents = XY.FromVec3(renderer.bounds.extents),
                    Position = XY.FromVec3(renderer.bounds.center),
                    RotationAngle = mMapTile.transform.localRotation.eulerAngles.z,
                    TextureName = renderer.sharedMaterial?.mainTexture?.name
                });
            }

            var targetDir = Application.persistentDataPath + "/fsm-export";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var scenarioVariantName = GameData.ScenarioVariant.name;
            File.WriteAllText(targetDir + $"/{scenarioVariantName}.json", Newtonsoft.Json.JsonConvert.SerializeObject(sce));
        }
    }

    private void DrawToggleButton(string text, Renderable renderable)
    {
        if (GUILayout.Button(text, GUILayout.ExpandWidth(true)))
        {
            ToggleRenderable(renderable);
        }
    }

    private void ToggleRenderable(Renderable r)
    {
        if (_renderables.Contains(r))
        {
            _renderables.Remove(r);
        }
        else
        {
            _renderables.Add(r);
        }
    }

    private void OnScenarioLoadingComplete()
    {
        _tileListUI.Update();
        _itemListUI.Update();
        _investigatorMagicUI.Update();
    }
}