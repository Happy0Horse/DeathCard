using System.Collections.Generic;
using UnityEngine;

public class PlacedTrap : MonoBehaviour
{
    private PlayerAnimation.TrapType _type;
    private float _damage;
    private int _explosionRange;
    private HexCell _parentCell;
    private Material _originalMaterial;

    public void Initialize(PlayerAnimation.TrapType type, float dmg, int range, HexCell cell, Material trapMat)
    {
        _type = type;
        _damage = dmg;
        _explosionRange = range;
        _parentCell = cell;

        gameObject.layer = 2;

        Renderer rend = _parentCell.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            _originalMaterial = rend.sharedMaterial;
            rend.material = trapMat;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root) return;

        if (other.TryGetComponent(out PlayerStat target))
        {
            TriggerTrap();
        }
    }

    private void TriggerTrap()
    {
        if (_type == PlayerAnimation.TrapType.Explosive)
        {
            Explode();
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
            foreach (var hit in colliders)
            {
                if (hit.TryGetComponent(out PlayerStat victim)) victim.TakeDamage(_damage);
            }
        }

        Cleanup();
    }

    private void Explode()
    {
        float radius = _explosionRange;
        Collider[] victims = Physics.OverlapSphere(transform.position, radius);

        HashSet<PlayerStat> processed = new HashSet<PlayerStat>();
        foreach (var v in victims)
        {
            if (v.TryGetComponent(out PlayerStat stat) && !processed.Contains(stat))
            {
                stat.TakeDamage(_damage);
                processed.Add(stat);
            }
        }
        Debug.Log($"Explosive trap triggered! Radius: {radius}");
    }

    private void Cleanup()
    {
        Renderer rend = _parentCell.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material = _originalMaterial;
        Destroy(gameObject);
    }
}