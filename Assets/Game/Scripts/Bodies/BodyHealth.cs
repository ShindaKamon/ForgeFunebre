// BodyHealth.cs
// Gère les PV d'un corps possédé.
// Séparé de BodyBase pour la même raison que DecaySystem.
using UnityEngine;
using System;

public class BodyHealth : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int maxHealth = 5;

    // ---- État ----
    private int  currentHealth;

    // ---- Événements ----
    public event Action<int, int> OnHealthChanged;  // (current, max)
    public event Action           OnDeath;

    // ============================================================
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Getters
    public int   GetCurrentHealth() => currentHealth;
    public int   GetMaxHealth()     => maxHealth;
    public float GetRatio()         => (float)currentHealth / maxHealth;
    public bool  IsDead             => currentHealth <= 0;

    public void SetMaxHealth(int value)
    {
        maxHealth     = value;
        currentHealth = value;
    }
}