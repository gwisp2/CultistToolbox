using System.Collections.Generic;
using CultistToolbox.Patch;
using UnityEngine;

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