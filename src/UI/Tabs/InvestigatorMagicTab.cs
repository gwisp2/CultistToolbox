using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CultistToolbox.Patch;
using FFG.MoM;
using FFG.MoM.Actions;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CultistToolbox.UI.Tabs;

public class InvestigatorMagicTab() : ToolboxTab("Investigators")
{
    private static bool _scenarioHasGetInvestigatorId;
    private static ScenarioVariant _scenarioVariant;

    private static readonly FieldInfo SModelField =
        typeof(GameData).GetField("s_model", BindingFlags.NonPublic | BindingFlags.Static);

    private const int MinInvestigators = 2;
    private const int MaxInvestigators = 5;
    private InvestigatorModel _selectedInvestigator;
    private InvestigatorModel _investigatorToRemove;
    private Vector2 _scrollPosition;
    private const int ButtonsPerRow = 3;
    private bool _isAddingInvestigator;

    public override void OnScenarioLoaded()
    {
        _scenarioHasGetInvestigatorId = Utilities.EnumerateAllActions<GetInvestigatorId>().Any();
        _scenarioVariant = CurrentScenarioVariantPatch.CurrentScenarioVariant;
    }

    private float CalculateButtonWidth()
    {
        float windowWidth = 800;
        float padding = 30f; // Adjust this value based on your UI's padding
        float labelWidth = 150f; // Estimated width for the investigator name label
        float availableWidth = windowWidth - padding - labelWidth;
        return availableWidth / 2; // Divide by 2 for 'Remove' and 'Replace' buttons
    }

    public override void Render()
    {
        if (!GameData.IsInitialized) return;

        if (_scenarioHasGetInvestigatorId && GameData.ScenarioVariant == _scenarioVariant)
        {
            GUILayout.Label("Warning: this scenario remembers investigator names and will break if you change them!",
                Common.WarningLabelStyle.Value);
        }

        var investigators = MoM_InvestigatorManager.Investigators;
        if (investigators == null || investigators.Count == 0) return;

        GUILayout.Label("Current investigators:");
        float buttonWidth = CalculateButtonWidth();

        for (int i = 0; i < investigators.Count; i++)
        {
            var investigator = investigators[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get(investigator.Name.Key), GUILayout.Width(150));
            if (investigators.Count > MinInvestigators && GUILayout.Button("Remove", GUILayout.Width(buttonWidth)))
            {
                _investigatorToRemove = investigator;
            }

            if (GUILayout.Button("Replace", GUILayout.Width(buttonWidth)))
            {
                _selectedInvestigator = investigator;
                _isAddingInvestigator = false;
            }

            GUILayout.EndHorizontal();

            if (_investigatorToRemove == investigator)
            {
                DrawRemoveConfirmation();
            }

            if (_selectedInvestigator == investigator && !_isAddingInvestigator)
            {
                DrawInvestigatorChooser(investigators);
            }
        }

        GUILayout.Space(10);
        if (investigators.Count < MaxInvestigators && GUILayout.Button("Add Investigator"))
        {
            _isAddingInvestigator = true;
            _selectedInvestigator = null;
        }

        if (_isAddingInvestigator)
        {
            DrawInvestigatorChooser(investigators);
        }
    }

    private void DrawRemoveConfirmation()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(150); // Align with the investigator name
        GUILayout.Label($"Are you sure you want to remove {Localization.Get(_investigatorToRemove.Name.Key)}?");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(150); // Align with the investigator name
        if (GUILayout.Button("Yes", GUILayout.Width(100)))
        {
            var investigators = MoM_InvestigatorManager.Investigators;
            var newInvestigators = investigators.ToList();
            newInvestigators.Remove(_investigatorToRemove);
            UpdateInvestigatorList(newInvestigators);
            _investigatorToRemove = null;
        }

        if (GUILayout.Button("No", GUILayout.Width(100)))
        {
            _investigatorToRemove = null;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawInvestigatorChooser(List<InvestigatorModel> currentInvestigators)
    {
        GUILayout.Space(10);
        GUILayout.Label(_isAddingInvestigator
            ? "Choose a new investigator to add:"
            : "Choose a new investigator to replace:");

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

        var availableInvestigators = MoMDBManager.DB.GetAvailableInvestigators()
            .Where(i => !currentInvestigators.Contains(i))
            .OrderBy(i => Localization.Get(i.Name.Key))
            .ToList();

        float buttonWidth = CalculateButtonWidth();

        for (int i = 0; i < availableInvestigators.Count; i += ButtonsPerRow)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < ButtonsPerRow && i + j < availableInvestigators.Count; j++)
            {
                var newInvestigator = availableInvestigators[i + j];
                if (GUILayout.Button(Localization.Get(newInvestigator.Name.Key)))
                {
                    if (_isAddingInvestigator)
                    {
                        AddInvestigator(newInvestigator);
                    }
                    else
                    {
                        ReplaceInvestigator(_selectedInvestigator, newInvestigator);
                    }

                    _selectedInvestigator = null;
                    _isAddingInvestigator = false;
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Cancel"))
        {
            _selectedInvestigator = null;
            _isAddingInvestigator = false;
        }
    }

    private void ReplaceInvestigator(InvestigatorModel oldInvestigator, InvestigatorModel newInvestigator)
    {
        var investigators = MoM_InvestigatorManager.Investigators;
        var newInvestigators = investigators.ToList();
        int index = newInvestigators.IndexOf(oldInvestigator);
        newInvestigators[index] = newInvestigator;
        UpdateInvestigatorList(newInvestigators);
    }

    private void AddInvestigator(InvestigatorModel newInvestigator)
    {
        var investigators = MoM_InvestigatorManager.Investigators;
        var newInvestigators = investigators.ToList();
        newInvestigators.Add(newInvestigator);
        UpdateInvestigatorList(newInvestigators);
    }


    private static void UpdateInvestigatorList(List<InvestigatorModel> newInvestigators)
    {
        MoM_InvestigatorManager.SetInvestigators(newInvestigators);
        UpdateInvestigatorImages();

        // Replace investigator list in GameData (so it will be saved)
        var gameDataModel = (GameDataModel)SModelField.GetValue(null);
        if (gameDataModel != null)
        {
            gameDataModel.Investigators = newInvestigators.ToList();
            gameDataModel.InvestigatorIds = newInvestigators.Join(i => i.Id.ToString(), ",");
        }
    }


    private static void UpdateInvestigatorImages()
    {
        foreach (var transition in Utilities.FindComponents<MoM_GamePhaseTransition>())
        {
            // Remove investigator images
            foreach (Transform child in transition.InvestigatorTable.gameObject.transform)
            {
                Object.Destroy(child.gameObject);
            }

            // Add investigator images
            int index;
            for (index = 0; index < MoM_InvestigatorManager.Investigators.Count; ++index)
            {
                UITexture component = transition.InvestigatorTable.gameObject.AddChild(transition.InvestigatorPrefab)
                    .GetComponent<UITexture>();
                if (component == null)
                    break;
                component.mainTexture = MoM_InvestigatorManager.Investigators[index].LoadImage();
            }

            // Temporarily show phase transition screen because without it the table repositions incorrectly 
            transition.TweenInvestigator.gameObject.SetActive(true);
            transition.TweenInvestigator.GetComponent<UIWidget>().alpha = 1f;
            transition.InvestigatorTable.Reposition();
            transition.TweenInvestigator.gameObject.SetActive(false);
        }
    }
}