using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<int> OnRequestMoveSelection;
    public static Action<CardData> OnRequestTrapSelection;
    public static Action<CardData> OnRequestUtilityAction; 
    public static Action<CardData> OnRequestAttackMode;
    public static Action OnCancelCurrentAction;
}
