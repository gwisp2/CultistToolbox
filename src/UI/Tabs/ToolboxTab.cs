namespace CultistToolbox.UI.Tabs;

public abstract class ToolboxTab(string name)
{
    public string Name { get; } = name;

    public virtual void OnScenarioLoaded()
    {
    }

    public virtual void OnScenarioShutdown()
    {
    }

    public abstract void Render();
}