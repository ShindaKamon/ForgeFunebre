// HUDManager.cs
// Gère l'affichage du HUD — barre de pourriture et PV.
// S'abonne aux événements du corps actif via TransferSystem.
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private DecayBar    decayBar;
    [SerializeField] private HealthDisplay healthDisplay;

    private BodyBase currentBody = null;

    // ============================================================
    private void OnEnable()
    {
        EventBus.Subscribe<OnTransferComplete>(OnTransferComplete);
        EventBus.Subscribe<OnBodyDied>(OnBodyDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnTransferComplete>(OnTransferComplete);
        EventBus.Unsubscribe<OnBodyDied>(OnBodyDied);
    }

    // ---- Nouveau corps possédé ----
    private void OnTransferComplete(OnTransferComplete evt)
    {
        // Désabonne de l'ancien corps
        if (currentBody != null)
        {
            currentBody.GetDecaySystem().OnDecayUpdated  -= decayBar.UpdateBar;
            currentBody.GetBodyHealth().OnHealthChanged  -= healthDisplay.UpdateHealth;
        }

        currentBody = evt.Next;
        if (currentBody == null) return;

        // Abonne au nouveau corps
        currentBody.GetDecaySystem().OnDecayUpdated += decayBar.UpdateBar;
        currentBody.GetBodyHealth().OnHealthChanged += healthDisplay.UpdateHealth;

        // Met à jour immédiatement
        decayBar.UpdateBar(currentBody.GetDecaySystem().GetRatio());
        healthDisplay.UpdateHealth(
            currentBody.GetBodyHealth().GetCurrentHealth(),
            currentBody.GetBodyHealth().GetMaxHealth()
        );

        // Affiche le nom du corps
        decayBar.SetBodyName(currentBody.GetDisplayName());
    }

    private void OnBodyDied(OnBodyDied evt)
    {
        if (evt.Body != currentBody) return;
        decayBar.UpdateBar(1f);
    }
}