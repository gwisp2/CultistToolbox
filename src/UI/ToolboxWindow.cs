using CultistToolbox.Patch;
using CultistToolbox.UI.Tabs;
using UnityEngine;

namespace CultistToolbox.UI;

public class ToolboxWindow : MonoBehaviour
{
    private WindowController _window;
    private bool _visible;
    private float _keyUnblockTime;

    private ToolboxTab[] _tabs;
    private ToolboxTab _selectedTab;

    private void Awake()
    {
        _tabs =
        [
            new TricksTab(),
            new NonRandomItemListTab(),
            new InvestigatorMagicTab(),
            new TileListTab(),
            new CutscenesTab(),
            new MythosTab()
        ];
        _selectedTab = _tabs[0];
        _window = new WindowController(5953, "Cultist Toolbox", DrawToolbox, new Rect(10, 10, 800, 600));
        HookScenarioLoadUnload.ScenarioLoadingComplete += OnScenarioLoadingComplete;
        HookScenarioLoadUnload.ScenarioShutdown += OnScenarioShutdown;
    }

    private void OnGUI()
    {
        if (Plugin.ConfigUiKey.Value.IsDown() && Time.unscaledTime >= _keyUnblockTime)
        {
            _visible = !_visible;
            _keyUnblockTime = Time.unscaledTime + 0.1f;
        }

        if (!_visible) return;

        _window.RenderWindow();
    }

    private void DrawToolbox()
    {
        // Draw tab chooser buttons
        DrawToolboxHeader();

        // Draw tab content
        if (_selectedTab != null)
        {
            _selectedTab.Render();
        }
    }

    private void DrawToolboxHeader()
    {
        GUILayout.BeginVertical("", GUI.skin.box);
        GUILayout.BeginHorizontal();
        foreach (var tab in _tabs)
        {
            if (GUILayout.Button(tab.Name,
                    _selectedTab == tab ? Common.HighlightButtonStyle.Value : Common.ButtonStyle.Value))
            {
                _selectedTab = tab;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void OnScenarioLoadingComplete()
    {
        foreach (var toolboxTab in _tabs)
        {
            toolboxTab.OnScenarioLoaded();
        }
    }

    private void OnScenarioShutdown()
    {
        foreach (var toolboxTab in _tabs)
        {
            toolboxTab.OnScenarioShutdown();
        }
    }
}