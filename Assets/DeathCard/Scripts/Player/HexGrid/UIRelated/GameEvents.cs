using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<int> OnRequestMoveSelection;
    public static Action<int> OnRequestTrapSelection;
    public static Action OnRequestUtilityAction; 
    public static Action<CardData> OnRequestAttackMode;
    public static Action OnCancelCurrentAction;
}
