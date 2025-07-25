using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log($"{name} recibió {amount} de daño. Vida actual: {currentHealth}");

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"{name} se curó {amount}. Vida actual: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{name} ha muerto.");
        // Agregá lógica de muerte o animación acá
        Destroy(gameObject);
    }
}
