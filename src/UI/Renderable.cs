namespace MoMEssentials.UI;

public abstract class Renderable
{
    public virtual void RenderFirstPass()
    {
    }

    public virtual void RenderSecondPass()
    {
    }

    public void RenderPass(int pass)
    {
        switch (pass)
        {
            case 0:
                RenderFirstPass();
                break;
            case 1:
                RenderSecondPass();
                break;
        }
    }
}