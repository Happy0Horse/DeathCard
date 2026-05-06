using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<int, Action> OnRequestMoveSelection;
    public static Action<CardData, Action> OnRequestTrapSelection;
    public static Action<CardData, Action> OnRequestAttackMode;
    public static Action<CardData, Action> OnRequestUtilityAction;
    public static Action OnCancelCurrentAction;
}