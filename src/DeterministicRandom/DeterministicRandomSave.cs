using System.Collections.Generic;

namespace CultistToolbox.DeterministicRandom;

public class DeterministicRandomSave
{
    public class ContextSave
    {
        public string ActionId;
        public int ActionCallIndex;
    }

    public string Seed;
    public int DefaultContextCallIndex;
    public List<ContextSave> ActionContexts;
}