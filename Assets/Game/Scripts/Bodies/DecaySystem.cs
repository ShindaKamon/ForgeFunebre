// DecaySystem.cs
// Gère la jauge de pourriture d'un corps possédé.
// Séparé de BodyBase pour pouvoir être tweaké indépendamment.
// Ne tourne que quand le corps est possédé.
using UnityEngine;
using System;

public class DecaySystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float decayDuration = 40f;

    // ---- État ----
    private float   decayTimer;
    private bool    isActive   = false;  // Tourne seulement si possédé
    private bool    isPaused   = false;  // Pour les zones spéciales plus tard

    // ---- Événements ----
    public event Action<float> OnDecayUpdated;  // float = ratio 0(plein) → 1(mort)
    public event Action        OnDecayDepleted; // Jauge à zéro → mort

    // ---- Couleurs de la barre selon l'urgence ----
    public static readonly Color ColorSafe    = new Color(0.2f, 0.8f, 0.2f);  // Vert
    public static readonly Color ColorWarning = new Color(1f,   0.6f, 0f);    // Orange
    public static readonly Color ColorDanger  = new Color(0.9f, 0.1f, 0.1f); // Rouge

    // ============================================================
    private void Update()
    {
        if (!isActive || isPaused) return;

        decayTimer -= Time.deltaTime;
        float ratio = 1f - (decayTimer / decayDuration); // 0=plein, 1=mort

        // Notifie la UI
        OnDecayUpdated?.Invoke(ratio);
        EventBus.Publish(new OnDecayChanged { Ratio = ratio });

        if (decayTimer <= 0f)
        {
            isActive = false;
            OnDecayDepleted?.Invoke();
        }
    }

    // ---- API publique ----

    // Appelé par BodyBase.OnPossess()
    public void StartDecay()
    {
        decayTimer = decayDuration;
        isActive   = true;
    }

    // Appelé par BodyBase.OnRelease() ou mort
    public void StopDecay()
    {
        isActive = false;
    }

    // Ajoute du temps (Runes, power-ups)
    public void AddTime(float seconds)
    {
        decayTimer = Mathf.Clamp(decayTimer + seconds, 0f, decayDuration);
    }

    public void SetPaused(bool paused) => isPaused = paused;

    // Getters
    public float GetRatio()        => 1f - (decayTimer / decayDuration);
    public float GetTimeRemaining() => Mathf.Max(0f, decayTimer);
    public bool  IsActive          => isActive;

    // Retourne la couleur correspondant au niveau d'urgence
    public Color GetCurrentColor()
    {
        float ratio = GetRatio();
        if (ratio < 0.5f) return ColorSafe;
        if (ratio < 0.75f) return Color.Lerp(ColorSafe, ColorWarning, (ratio - 0.5f) / 0.25f);
        return Color.Lerp(ColorWarning, ColorDanger, (ratio - 0.75f) / 0.25f);
    }

    public void SetDuration(float duration)
    {
        decayDuration = duration;
    }
}