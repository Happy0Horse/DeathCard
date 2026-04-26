using System.Collections.Generic;
using UnityEngine;

public class CardAttackLander : MonoBehaviour
{
    [SerializeField] private List<Collider> weaponColliders = new List<Collider>();
    private float _activeDamage;
    private HashSet<PlayerStat> _hitTargets = new HashSet<PlayerStat>();

    [Header("Multi-Hit Settings")]
    public bool useMultiHitMode = false;
    public float hitCooldown = 0.1f;
    private Dictionary<PlayerStat, float> _cooldownTracker = new Dictionary<PlayerStat, float>();

    void Awake()
    {
        foreach (var col in weaponColliders)
        {
            if (col == null) continue;
            col.isTrigger = true;
            col.enabled = false;

            if (!col.gameObject.GetComponent<TriggerProxy>())
            {
                var proxy = col.gameObject.AddComponent<TriggerProxy>();
                proxy.parentLander = this;
            }
        }
    }

    public void PrepareAttack(float damageValue, bool multiHit)
    {
        _activeDamage = damageValue;
        useMultiHitMode = multiHit;
    }
    public void EnableHitbox()
    {
        _hitTargets.Clear();
        _cooldownTracker.Clear();
        foreach (var col in weaponColliders) col.enabled = true;
    }
    public void DisableHitbox()
    {
        foreach (var col in weaponColliders) col.enabled = false;
    }
    public void ResetHitTracker() => _hitTargets.Clear();

    public void HandleCollision(Collider other)
    {
        if (other.transform.root == transform.root) return;

        if (other.TryGetComponent(out PlayerStat target))
        {
            if (useMultiHitMode)
            {
                if (!_cooldownTracker.ContainsKey(target) || Time.time >= _cooldownTracker[target] + hitCooldown)
                {
                    target.TakeDamage(_activeDamage);
                    _cooldownTracker[target] = Time.time;
                }
            }
            else
            {
                if (!_hitTargets.Contains(target))
                {
                    target.TakeDamage(_activeDamage);
                    _hitTargets.Add(target);
                }
            }
        }
    }
}