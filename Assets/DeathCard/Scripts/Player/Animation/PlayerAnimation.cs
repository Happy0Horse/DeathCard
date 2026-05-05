using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    public HexViewManager hexViewManager;
    public CardAttackLander lander;

    public enum AttackMode { Melee, Spin, Ora, Shot }
    public enum TrapType { Spikes, Stun, Explosive }
    public enum UtilityType { GalaxyVoid, Heal, Shield }

    [Header("Animation mode")]
    public AttackMode CurrentMode = AttackMode.Melee;

    [Header("Animation keys")]
    public string attackAnimationTrigger = "";
    public string spinAnimationTrigger = "";
    public string oraAnimationTrigger = "";
    public string rangedAttackTrigger = "";

    private System.Action _onAttackConfirmed;

    void Start()
    {
        animator = GetComponent<Animator>();
        hexViewManager = FindFirstObjectByType<HexViewManager>();

        if (lander == null) lander = GetComponent<CardAttackLander>();
    }

    void Update()
    {
        if (hexViewManager == null) return;

        if (hexViewManager.CurrentView == HexViewManager.ViewMode.FirstPerson)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && _onAttackConfirmed != null)
            {
                StartCoroutine(PlayAttackSequence());
            }
        }
    }

    void OnEnable() => GameEvents.OnRequestAttackMode += SetupAttack;
    void OnDisable() => GameEvents.OnRequestAttackMode -= SetupAttack;

    private void SetupAttack(CardData data, System.Action onComplete)
    {
        if (data.category != CardData.CardCategory.Attack) return;

        CurrentMode = data.attackMode;
        _onAttackConfirmed = onComplete;

        if (lander != null)
        {
            lander.PrepareAttack(data.damage, data.isMultiHit);
        }

        animator.SetFloat("AttackRange", data.range);
    }

    private IEnumerator PlayAttackSequence()
    {
        hexViewManager.IsLocked = true;

        _onAttackConfirmed?.Invoke();
        _onAttackConfirmed = null;

        string trigger = GetAnimationTrigger();
        animator.SetTrigger(trigger);

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(trigger) ||
                                         !animator.IsInTransition(0));

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration + 0.6f);

        lander.DisableHitbox();
        hexViewManager.IsLocked = false;

        if (hexViewManager != null)
        {
            hexViewManager.ExitFirstPerson();
        }
    }

    private string GetAnimationTrigger()
    {
        return CurrentMode switch
        {
            AttackMode.Melee => attackAnimationTrigger,
            AttackMode.Spin => spinAnimationTrigger,
            AttackMode.Ora => oraAnimationTrigger,
            AttackMode.Shot => rangedAttackTrigger,
            _ => attackAnimationTrigger
        };
    }
}