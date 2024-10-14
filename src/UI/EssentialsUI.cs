using MoMEssentials.Patch;
using UnityEngine;

namespace MoMEssentials.UI;

public class EssentialsUI : MonoBehaviour
{
    private readonly ItemListUI _itemListUI = new();
    private readonly TileListUI _tileListUI = new();
    private readonly InvestigatorMagicUI _investigatorMagicUI = new();
    private bool _visible;
    private float _keyUnblockTime = 0;

    private void Awake()
    {
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

        _itemListUI.OnGUI();
        _tileListUI.OnGUI();
        _investigatorMagicUI.OnGUI();
        _tileListUI.OnGUISecondPass();
        _itemListUI.OnGUISecondPass();
    }

    private void OnScenarioLoadingComplete()
    {
        _tileListUI.Update();
        _itemListUI.Update();
    }
}