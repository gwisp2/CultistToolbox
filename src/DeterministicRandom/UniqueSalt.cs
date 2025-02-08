using System;
using System.Runtime.CompilerServices;
using FFG.MoM;
using HutongGames.PlayMaker;

namespace CultistToolbox.DeterministicRandom;

public static class UniqueSalt
{
    public static string Of(object value)
    {
        if (value is IFsmStateAction action)
        {
            return Of(action);
        }

        if (value is Fsm fsm)
        {
            return Of(fsm);
        }

        if (value is ItemModel itemModel)
        {
            return itemModel.Id + "_" + itemModel.Name.Key;
        }

        if (value is MonsterModel monsterModel)
        {
            return monsterModel.Id + "_" + monsterModel.Name.Key;
        }

        if (value is RoomModel roomModel)
        {
            return roomModel.Id + "_" + roomModel.Name.Key;
        }

        if (value is MoM_MapTile mapTile && mapTile.Model)
        {
            return mapTile.Model.Name.Key;
        }

        if (value is MythosEventModel eventModel)
        {
            return eventModel.Id + "_" + eventModel.Name;
        }

        Plugin.Logger.LogWarning("UniqueSalt.of: unknown type:" + value.GetType());

        return RuntimeHelpers.GetHashCode(value).ToString();
    }

    public static string Of(IFsmStateAction action)
    {
        if (action is not FsmStateAction fsmStateAction)
        {
            return RuntimeHelpers.GetHashCode(action).ToString();
        }

        var fsmSalt = Of(fsmStateAction.Fsm);
        var stateName = fsmStateAction.State.Name;
        var actionIndex = Array.IndexOf(fsmStateAction.State.Actions, fsmStateAction);
        return $"{fsmSalt}@{stateName}@{actionIndex}";
    }

    public static string Of(Fsm fsm)
    {
        var fsmComponent = fsm.FsmComponent;
        if (fsmComponent == null)
        {
            // Not sure if that is actually possible
            return RuntimeHelpers.GetHashCode(fsm).ToString();
        }

        var fullName = fsmComponent.gameObject.GetFullName();
        var fsmName = fsm.Name;
        var templateName = fsmComponent.FsmTemplate ? fsmComponent.FsmTemplate.name : "";
        return $"{fullName}@{fsmName}@{templateName}";
    }
}