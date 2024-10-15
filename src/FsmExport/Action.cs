using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFG.MoM;
using FFG.MoM.Actions;
using HutongGames.PlayMaker;
using UnityEngine;

namespace MoMEssentials.FsmExport;

public class ActionE
{
    public string Type;
    public Dictionary<string, object> Parameters;

    public static ActionE FromFsmAction(FsmE fsmE, FsmStateAction action)
    {
        var result = new ActionE
        {
            Type = action.GetType().FullName,
            Parameters = new()
        };
        var actionType = action.GetType();
        foreach (var fieldInfo in actionType.GetFields())
        {
            result.Parameters[fieldInfo.Name] = ExportValue(action.Fsm, fieldInfo.GetValue(action));
        }

        return result;
    }

    public static object ExportComplexObject(Fsm fsm, object a)
    {
        var parameters = new Dictionary<string, object>();
        foreach (var fieldInfo in a.GetType().GetFields())
        {
            parameters[fieldInfo.Name] = ExportValue(fsm, fieldInfo.GetValue(a));
        }

        return new
        {
            Type = a.GetType(),
            Parameters = parameters
        };
    }

    public static object ExportValue(Fsm fsm, object value)
    {
        if (value is Enum e)
        {
            return ExportEnum(e);
        }

        if (value is MessageChoiceItem or CounterResult or DisplayMessageChallenge.ChallengeResultPairs
            or FsmEventTarget or FsmOwnerDefault)
        {
            return ExportComplexObject(fsm, value);
        }

        if (value is Texture2D texture2D)
        {
            return new
            {
                Type = texture2D.GetType(),
                Name = texture2D.name
            };
        }

        if (value is Vector3 v3)
        {
            return new
            {
                X = v3.x,
                Y = v3.y,
                Z = v3.z
            };
        }

        if (value is PlayMakerFSM component)
        {
            return new
            {
                Type = component.GetType(),
                At = component.gameObject.GetFullName()
            };
        }

        if (value is MonsterModel monsterModel)
        {
            return new
            {
                Type = value.GetType(),
                MonsterType = monsterModel.Type.name,
                NameKey = monsterModel.Name.Key,
                name = Localization.Get(monsterModel.name),
                Name = Localization.Get(monsterModel.Name.Key),
                Traits = ExportEnum(monsterModel.Traits),
                Toughness = monsterModel.Toughness
            };
        }

        if (value is AudioClip audioClip)
        {
            return new
            {
                Type = value.GetType(),
                Name = audioClip.GetName()
            };
        }

        if (value is string or int or long or null or bool or MenuType or float or double or char)
        {
            return value;
        }

        if (value is IEnumerable iEnumerable)
        {
            List<object> result = new List<object>();
            foreach (var o in iEnumerable)
            {
                result.Add(ExportValue(fsm, o));
            }

            return result;
        }

        if (value is FsmEvent fsmEvent)
        {
            return fsmEvent.Name;
        }

        if (value is NamedVariable namedVariable)
        {
            if (string.IsNullOrEmpty(namedVariable.Name))
            {
                return ExportValue(fsm, namedVariable.RawValue);
            }

            return new
            {
                Variable = namedVariable.Name, Global = PlayMakerGlobals.Instance.Variables.Contains(namedVariable),
                FsmLocal = fsm.Variables.Contains(namedVariable)
            };
        }

        if (value is MoM_LocalizationPacket.PacketInsert insert)
        {
            if (insert.Filter == LocalizationFilterType.RandomItem)
            {
                return new
                {
                    Filter = ExportEnum(insert.Filter),
                    IncludeItemTraits = ExportEnum(insert.IncludeItemTraits),
                    ExcludeItemTraits = ExportEnum(insert.ExcludeItemTraits),
                    ItemFilter = ExportEnum(insert.ItemFilter)
                };
            }
            else if (insert.Filter == LocalizationFilterType.RandomMonster)
            {
                return new
                {
                    Filter = ExportEnum(insert.Filter),
                    IncludeMonsterTraits = ExportEnum(insert.IncludeMonsterTraits),
                    ExcludeMonsterTraits = ExportEnum(insert.ExcludeMonsterTraits),
                };
            }
            else if (insert.Filter == LocalizationFilterType.SpecificLocalizationKey)
            {
                return new
                {
                    Filter = ExportEnum(insert.Filter),
                    Key = insert.SpecificLocalizationKey.Key,
                    Localized = Localization.Get(insert.SpecificLocalizationKey.Key)
                };
            }
            else if (insert.Filter == LocalizationFilterType.SpecificInvestigatorId)
            {
                return new
                {
                    Filter = ExportEnum(insert.Filter),
                    Id = ExportValue(fsm, insert.VariableInt)
                };
            }
            else
            {
                return new
                {
                    Filter = ExportEnum(insert.Filter),
                };
            }
        }

        if (value is MoM_LocalizationPacket packet)
        {
            int nArgs = packet.CalculateArguments();
            var usedInserts = packet.Inserts.Where((_, i) => i < nArgs);
            return new
            {
                Type = packet.GetType().FullName,
                Key = packet.Key,
                Content = Localization.Get(packet.Key),
                Inserts = usedInserts.Select(insert => ExportValue(fsm, insert)).ToList()
            };
        }

        if (value is ItemModel itemModel)
        {
            return new
            {
                Type = value.GetType().FullName,
                NameKey = itemModel.Name.Key,
                Name = Localization.Get(itemModel.Name.Key),
                Traits = GetFlagNames(itemModel.Traits),
            };
        }

        if (value is RoomModel roomModel)
        {
            return new
            {
                Type = value.GetType().FullName,
                NameKey = roomModel.Name.Key,
                Name = Localization.Get(roomModel.Name.Key),
            };
        }

        if (value is LocalizationPacket packet2)
        {
            return new
            {
                Type = value.GetType().FullName,
                Key = packet2.Key,
                StringValue = Localization.Get(packet2.Key)
            };
        }

        if (value is TileModel tileModel)
        {
            return new
            {
                Type = value.GetType().FullName,
                NameKey = tileModel.Name.Key,
                Name = Localization.Get(tileModel.Name.Key),
                Shape = Enum.GetName(typeof(MapTileShape), tileModel.Shape),
            };
        }

        if (value is MoM_SpawnPoint spawnPoint)
        {
            return new
            {
                Type = value.GetType().FullName,
                HostTile = ExportValue(fsm, spawnPoint.HostTile?.Model)
            };
        }

        if (value is GameObject gameObject)
        {
            return new
            {
                Type = typeof(GameObject).ToString(),
                FullName = gameObject.GetFullName()
            };
        }

        Plugin.Logger.LogWarning("Unsupported type: " + value.GetType().FullName);
        return new
        {
            Type = value.GetType().FullName,
        };
    }

    public static object ExportEnum(Enum e)
    {
        var type = e.GetType();
        if (!type.IsDefined(typeof(FlagsAttribute), false))
            return Enum.GetName(type, e);
        else return GetFlagNames(e);
    }

    public static List<string> GetFlagNames(Enum flags)
    {
        if (!flags.GetType().IsDefined(typeof(FlagsAttribute), false))
            throw new ArgumentException("Type must be a flags enum");

        return Enum.GetValues(flags.GetType())
            .Cast<Enum>()
            .Where(value => !Equals((int)(object)value, 0) && flags.HasFlag(value))
            .Select(value => Enum.GetName(flags.GetType(), value))
            .ToList();
    }
}

public class TransitionE
{
    public string EventName;
    public string DestinationStateName;
}

public class StateE
{
    public string Name;
    public List<TransitionE> Transitions;
    public List<ActionE> Actions;

    public static StateE FromFsmState(FsmE fsmE, FsmState state)
    {
        var exported = new StateE();
        exported.Name = state.Name;
        exported.Transitions = state.Transitions.Select(transition => new TransitionE()
        {
            DestinationStateName = transition.ToState,
            EventName = transition.EventName
        }).ToList();
        exported.Actions = state.Actions.Select(action => ActionE.FromFsmAction(fsmE, action)).ToList();
        return exported;
    }
}

public class FsmE
{
    public string OwnerFullName;
    public List<StateE> States;

    public static FsmE FromFsm(Fsm fsm)
    {
        var exported = new FsmE();
        exported.OwnerFullName = fsm.Owner?.gameObject?.GetFullName();
        exported.States = fsm.States.Select(s => StateE.FromFsmState(exported, s)).ToList();
        return exported;
    }
}