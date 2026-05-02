using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTrap : MonoBehaviour
{
    private PlayerAnimation.TrapType _type;
    private float _damage;
    private int _explosionRange;
    private float _duration;
    private HexCell _parentCell;
    private GameObject _visualInstance;
    private Material _originalMaterial;
    private bool _hasTriggered = false;

    public void Initialize(PlayerAnimation.TrapType type, float dmg, int range, float duration, HexCell cell, GameObject prefab, Material trapMat)
    {
        _type = type;
        _damage = dmg;
        _explosionRange = range;
        _duration = duration;
        _parentCell = cell;

        gameObject.layer = 2;

        if (_parentCell != null)
        {
            Renderer rend = _parentCell.GetComponentInChildren<Renderer>();
            if (rend != null && trapMat != null)
            {
                _originalMaterial = rend.sharedMaterial;
                rend.material = trapMat;
            }
        }

        if (prefab != null)
        {
            _visualInstance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;
        if (other.transform.root == transform.root) return;

        if (other.TryGetComponent(out PlayerStat target))
        {
            _hasTriggered = true;
            TriggerTrap(target);
        }
    }

    private void TriggerTrap(PlayerStat target)
    {
        if (_visualInstance != null && _visualInstance.TryGetComponent(out Animator anim))
        {
            anim.SetTrigger("Trigger");
        }

        if (_type == PlayerAnimation.TrapType.Explosive)
        {
            Explode();
        }
        else
        {
            ApplyEffects(target);
        }

        float delay = (_type == PlayerAnimation.TrapType.Explosive) ? 0.5f : 0.5f;
        Invoke(nameof(Cleanup), delay);
    }

    private void ApplyEffects(PlayerStat target)
    {
        if (target == null) return;

        target.TakeDamage(_damage);

        if (_type == PlayerAnimation.TrapType.Stun)
        {
            DebuffSystem debuffs = target.GetComponent<DebuffSystem>();
            if (debuffs != null)
            {
                debuffs.AddDebuff(DebuffSystem.DebuffType.Stun, _duration);
            }
        }
    }

    private void Explode()
    {
        float cellWidth = 2f;
        float calculatedRadius = (_explosionRange * cellWidth) + 0.5f;

        Collider[] victims = Physics.OverlapSphere(transform.position, calculatedRadius);
        HashSet<PlayerStat> processed = new HashSet<PlayerStat>();

        foreach (var v in victims)
        {
            if (v.TryGetComponent(out PlayerStat stat) && !processed.Contains(stat))
            {
                ApplyEffects(stat);
                processed.Add(stat);
            }
        }
    }

    private void Cleanup()
    {
        if (_parentCell != null && _originalMaterial != null)
        {
            Renderer rend = _parentCell.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material = _originalMaterial;
        }
        Destroy(gameObject);
    }
}