// Porter.cs
// Premier corps concret — lent, résistant, peut briser les murs.
// Hérite de BodyBase qui gère déjà PV, pourriture, possession.
using UnityEngine;

public class Porter : BodyBase
{
    [Header("Porter — Capacité spéciale")]
    //[SerializeField] private float breakRange = 1.2f;
    [SerializeField] private LayerMask breakableLayer;

    protected override void Awake()
    {
        base.Awake();
        // Le Porteur est lent — on ajuste la vitesse via BodyPhysics
        // Ces valeurs seront dans un ScriptableObject plus tard
    }

    // Capacité : brise les murs/blocs destructibles proches
    public override void UseSpecialAbility()
    {
        // TODO Étape 7 : briser les murs destructibles
        // Nécessite le script Breakable.cs (pas encore créé)
        // Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        // Vector2 origin    = (Vector2)transform.position + direction * 0.5f;
        // Collider2D hit = Physics2D.OverlapCircle(origin, breakRange, breakableLayer);
        // if (hit != null && hit.TryGetComponent<Breakable>(out var breakable))
        //     breakable.Break();

        Debug.Log("[Porter] Capacité spéciale — bris de mur (pas encore implémenté)");
    }

    protected override Color GetBaseColor()      => new Color(0.55f, 0.45f, 0.35f); // Marron
    protected override Color GetPossessedColor() => new Color(0.7f, 0.85f, 1f);     // Bleu clair
}