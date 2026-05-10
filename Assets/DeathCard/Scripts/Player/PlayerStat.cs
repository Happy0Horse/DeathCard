using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStat : NetworkBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public UnityEvent<float, float> OnHealthChanged;

    void Awake() => currentHealth = maxHealth;

    void Start() => OnHealthChanged?.Invoke(currentHealth, maxHealth);

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        GlobalEvents.OnAnyDamageTaken?.Invoke(damage);
        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has perished.");

        if (!isLocalPlayer) return;

        NetworkClient.Send(new PlayerDiedMessage());
    }
}