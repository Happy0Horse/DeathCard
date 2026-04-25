using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    public HexViewManager hexViewManager;

    public CardAttackLander lander;

    public enum AttackMode { Melee, Spin, Ora, Shot }

    [Header("Animation mode")]
    public AttackMode CurrentMode = AttackMode.Melee;

    [Header("Animation keys")]
    public string attackAnimationTrigger = "";
    public string spinAnimationTrigger = "";
    public string oraAnimationTrigger = "";
    public string rangedAttackTrigger = "";

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
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartCoroutine(PlayAttackSequence());
            }
        }
    }

    void OnEnable() => GameEvents.OnRequestAttackMode += SetupAttack;
    void OnDisable() => GameEvents.OnRequestAttackMode -= SetupAttack;

    private void SetupAttack(CardData data)
    {
        CurrentMode = data.attackMode;

        if (lander != null) lander.PrepareAttack(data.damage);

        animator.SetFloat("AttackRange", data.range);
        Debug.Log("Prepared for animation: " + CurrentMode + " with damage: " + data.damage);
    }

    private IEnumerator PlayAttackSequence()
    {
        hexViewManager.IsLocked = true;
        animator.SetTrigger(GetAnimationTrigger());

        yield return null;

        float duration = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        hexViewManager.IsLocked = false;
    }

    private string GetAnimationTrigger()
    {
        switch (CurrentMode)
        {
            case AttackMode.Melee: return attackAnimationTrigger;
            case AttackMode.Spin: return spinAnimationTrigger;
            case AttackMode.Ora: return oraAnimationTrigger;
            case AttackMode.Shot: return rangedAttackTrigger;
            default: return attackAnimationTrigger;
        }
    }
}
