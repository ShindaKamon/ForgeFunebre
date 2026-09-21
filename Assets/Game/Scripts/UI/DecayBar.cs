// DecayBar.cs
// Affiche la jauge de pourriture avec changement de couleur progressif.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecayBar : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Image     fillImage;       // Image de remplissage
    [SerializeField] private Image     backgroundImage; // Fond de la barre
    [SerializeField] private TextMeshProUGUI bodyNameText;
    [SerializeField] private TextMeshProUGUI timeRemainingText;

    [Header("Animation")]
    [SerializeField] private float colorLerpSpeed = 3f;

    private Color targetColor = DecaySystem.ColorSafe;

    // ============================================================
    // Appelé par HUDManager quand la jauge change
    // ratio : 0 = plein, 1 = mort
    public void UpdateBar(float ratio)
    {
        if (fillImage == null) return;

        // Inverse le fill — 1 = barre pleine, 0 = barre vide
        fillImage.fillAmount = 1f - ratio;

        // Couleur selon urgence
        if (ratio < 0.5f)
            targetColor = DecaySystem.ColorSafe;
        else if (ratio < 0.75f)
            targetColor = Color.Lerp(DecaySystem.ColorSafe, DecaySystem.ColorWarning,
                            (ratio - 0.5f) / 0.25f);
        else
            targetColor = Color.Lerp(DecaySystem.ColorWarning, DecaySystem.ColorDanger,
                            (ratio - 0.75f) / 0.25f);
    }

    private void Update()
    {
        // Lisse la transition de couleur
        if (fillImage != null)
            fillImage.color = Color.Lerp(fillImage.color, targetColor,
                                colorLerpSpeed * Time.deltaTime);
    }

    public void SetBodyName(string name)
    {
        if (bodyNameText != null)
            bodyNameText.text = name;
    }
}