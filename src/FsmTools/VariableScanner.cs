using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFG.Common.Actions;
using FFG.MoM.Actions;
using HutongGames.PlayMaker;

namespace MoMEssentials.FsmTools;

public class VariableScanner
{
    public class VariableUsage
    {
        public IFsmStateAction Action;
        public VariableUsageTypes UsageTypes;
    }

    public class VariableInfo
    {
        public Fsm Owner;
        public string Name => Variable.Name;
        public NamedVariable Variable;
        public List<VariableUsage> Usages = new();
        public bool IsGlobal => Owner == null;

        public VariableUsageTypes GetUsageTypes()
        {
            return Usages.Aggregate(VariableUsageTypes.None, (current, usage) => current | usage.UsageTypes);
        }
    }

    private readonly Dictionary<NamedVariable, VariableInfo> _variables = new();

    public VariableScanner()
    {
    }

    public Dictionary<NamedVariable, VariableInfo> GetVariables()
    {
        return _variables;
    }

    public void ScanAll(IEnumerable<Fsm> fsms)
    {
        foreach (var fsm in fsms)
        {
            Scan(fsm);
        }
    }

    public void Scan(Fsm fsm)
    {
        foreach (var variable in fsm.Variables.GetAllNamedVariables())
        {
            RememberVariable(variable, fsm);
        }

        foreach (var state in fsm.States)
        {
            foreach (var action in state.Actions)
            {
                switch (action)
                {
                    case SetFlag setFlag:
                        RememberVariable(setFlag.boolVariable, fsm, action, VariableUsageTypes.Write);
                        RememberVariable(setFlag.boolValue, fsm, action, VariableUsageTypes.Read);
                        break;
                    case GetFlag getFlag:
                        RememberVariable(getFlag.boolVariable, fsm, action, VariableUsageTypes.Read);
                        break;
                    case CheckCounter checkCounter:
                        RememberVariable(checkCounter.variable, fsm, action, VariableUsageTypes.Read);
                        break;
                    case SetCounter setCounter:
                        RememberVariable(setCounter.variable, fsm, action, VariableUsageTypes.Write);
                        RememberVariable(setCounter.value, fsm, action, VariableUsageTypes.Write);
                        break;
                    case IncrementCounter incrementCounter:
                        RememberVariable(incrementCounter.variable, fsm, action, VariableUsageTypes.Write);
                        break;
                    case TestInt testInt:
                        RememberVariable(testInt.LeftSide, fsm, action, VariableUsageTypes.Read);
                        RememberVariable(testInt.RightSide, fsm, action, VariableUsageTypes.Read);
                        break;
                    case CheckCondition checkCondition:
                        // TODO: handle special variables (number of players, etc...)
                        RememberVariable(checkCondition.variable, fsm, action, VariableUsageTypes.Read);
                        break;
                    case CheckConditionNew checkConditionNew:
                        RememberVariable(checkConditionNew.LeftSide, fsm, action, VariableUsageTypes.Read);
                        RememberVariable(checkConditionNew.RightSide, fsm, action, VariableUsageTypes.Read);
                        break;
                    case WaitForFlag waitForFlag:
                        RememberVariable(waitForFlag.boolVariable, fsm, action, VariableUsageTypes.Read);
                        break;
                    case RandomInt randomInt:
                        RememberVariable(randomInt.storeResult, fsm, action, VariableUsageTypes.Write);
                        RememberVariable(randomInt.min, fsm, action, VariableUsageTypes.Read);
                        RememberVariable(randomInt.max, fsm, action, VariableUsageTypes.Read);
                        break;
                    case WaitForRoundDelay waitForRoundDelay:
                        RememberVariable(waitForRoundDelay.Delay, fsm, action, VariableUsageTypes.Read);
                        break;
                    case SpawnMonster spawnMonster:
                        RememberVariable(spawnMonster.Health, fsm, action, VariableUsageTypes.Read);
                        break;
                    case SetMonsterHP setMonsterHp:
                        RememberVariable(setMonsterHp.HitPoints, fsm, action, VariableUsageTypes.Read);
                        break;
                    case IncreaseThreat increaseThreat:
                        RememberVariable(increaseThreat.value, fsm, action, VariableUsageTypes.Read);
                        break;
                    case DisplayMessageChallenge displayMessageChallenge:
                        RememberVariable(displayMessageChallenge.Sum, fsm, action, VariableUsageTypes.Write);
                        break;
                    default:
                        ExtractVariables(fsm, action, action, 2);
                        break;
                }
            }
        }
    }

    private void ExtractVariables(Fsm owner, IFsmStateAction action, object obj, int depth)
    {
        if (obj == null) return;
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (typeof(NamedVariable).IsAssignableFrom(field.FieldType))
            {
                var value = (NamedVariable)field.GetValue(obj);
                if (value != null)
                {
                    RememberVariable(value, owner, action, VariableUsageTypes.Unknown);
                }
            }
            else if (depth > 0)
            {
                ExtractVariables(owner, action, field.GetValue(obj), depth - 1);
            }
        }
    }

    private void RememberVariable(NamedVariable variable, Fsm owner, IFsmStateAction action = null,
        VariableUsageTypes usageTypes = VariableUsageTypes.None)
    {
        if (string.IsNullOrEmpty(variable.Name)) return;
        if (!_variables.TryGetValue(variable, out var info))
        {
            _variables[variable] = info = new VariableInfo
            {
                Owner = owner,
                Variable = variable,
            };
        }

        if (action != null)
        {
            info.Usages.Add(new VariableUsage()
            {
                Action = action, UsageTypes = usageTypes
            });
        }
    }
}