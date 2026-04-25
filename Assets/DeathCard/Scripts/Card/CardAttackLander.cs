using UnityEngine;

public class CardAttackLander : MonoBehaviour
{
    [SerializeField] private Collider weaponCollider;
    private float _activeDamage;

    void Awake()
    {
        if (weaponCollider == null) return;

        weaponCollider.isTrigger = true;
        weaponCollider.enabled = false;

        if (!weaponCollider.gameObject.GetComponent<TriggerProxy>())
        {
            var proxy = weaponCollider.gameObject.AddComponent<TriggerProxy>();
            proxy.parentLander = this;
        }
    }

    public void PrepareAttack(float damageValue) => _activeDamage = damageValue;
    public void EnableHitbox() => weaponCollider.enabled = true;
    public void DisableHitbox() => weaponCollider.enabled = false;

    public void HandleCollision(Collider other)
    {
        if (other.transform.root == transform.root) return;

        if (other.TryGetComponent(out PlayerStat target))
        {
            target.TakeDamage(_activeDamage);
            weaponCollider.enabled = false;
        }
    }
}