// HealthDisplay.cs
// Affiche les PV sous forme de cœurs ou de texte.
// Version simple — texte. On ajoutera les icônes plus tard.
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Transform       heartsContainer; // Parent des icônes cœur
    [SerializeField] private GameObject      heartPrefab;     // Icône cœur (optionnel)

    // ============================================================
    // Appelé par HUDManager quand les PV changent
    public void UpdateHealth(int current, int max)
    {
        // Affichage texte simple
        if (healthText != null)
            healthText.text = $"{current} / {max}";

        // Affichage par icônes cœur (si le prefab est assigné)
        if (heartsContainer != null && heartPrefab != null)
            UpdateHearts(current, max);
    }

    private void UpdateHearts(int current, int max)
    {
        // Supprime les anciens cœurs
        foreach (Transform child in heartsContainer)
            Destroy(child.gameObject);

        // Crée les nouveaux
        for (int i = 0; i < max; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            Image img        = heart.GetComponent<Image>();
            if (img != null)
                // Cœur plein = blanc, cœur vide = gris sombre
                img.color = i < current
                    ? new Color(0.9f, 0.2f, 0.2f)   // Rouge — PV restant
                    : new Color(0.2f, 0.2f, 0.2f);  // Gris — PV perdu
        }
    }
}