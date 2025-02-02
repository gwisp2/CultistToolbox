namespace CultistToolbox.UI.Tabs;

public abstract class ToolboxTab(string name)
{
    public string Name { get; } = name;

    public virtual void OnScenarioLoaded()
    {
    }

    public abstract void Render();
}