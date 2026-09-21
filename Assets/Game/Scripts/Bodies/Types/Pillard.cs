// Pillard.cs
// Corps agile — rapide, double saut, mais fragile et se décompose vite.
using UnityEngine;

public class Pillard : BodyBase
{
    [Header("Pillard — Double Saut")]
    [SerializeField] private int maxAirJumps = 1;  // 1 saut supplémentaire en l'air
    private int airJumpsRemaining;

    // Stats overrides — seront dans un ScriptableObject plus tard
    // moveSpeed = 7f (plus rapide que le Porteur)
    // decayDuration = 30f (se décompose plus vite)
    // maxHealth = 3 (plus fragile)

    protected override void Awake()
    {
        base.Awake();
        airJumpsRemaining = maxAirJumps;
        
        // Subscribe à l'événement OnLanded de BodyPhysics
        bodyPhysics.OnLanded += OnLanded;
    }

    private void OnDestroy()
    {
        // Unsubscribe pour éviter les fuites mémoire
        if (bodyPhysics != null)
            bodyPhysics.OnLanded -= OnLanded;
    }

    public override void OnPossess()
    {
        base.OnPossess();
        airJumpsRemaining = maxAirJumps; // Reset à chaque possession
    }

    // Le Pillard peut sauter une fois de plus en l'air
    // Cette méthode est appelée par un futur InputHandler
    public bool TryAirJump()
    {
        if (airJumpsRemaining <= 0) return false;

        airJumpsRemaining--;
        GetBodyPhysics().RequestJump();
        return true;
    }

    // Capacité : dash rapide dans la direction du mouvement
    public override void UseSpecialAbility()
    {
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        GetBodyPhysics().ApplyKnockback(direction, 12f);
        Debug.Log("[Pillard] Dash !");
    }

    // Reset du double saut quand on touche le sol
    // Appelé via un event de BodyPhysics (à connecter)
    public void OnLanded()
    {
        airJumpsRemaining = maxAirJumps;
    }

    protected override Color GetBaseColor()      => new Color(0.3f, 0.5f, 0.3f); // Vert sombre
    protected override Color GetPossessedColor() => new Color(0.5f, 1f, 0.5f);   // Vert clair
}