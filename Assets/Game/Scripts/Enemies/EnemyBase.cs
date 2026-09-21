// EnemyBase.cs
// Classe abstraite pour tous les ennemis.
// Un ennemi a deux phases distinctes :
// Phase 1 — VIVANT : se comporte comme un ennemi (IA, attaque)
// Phase 2 — MORT   : devient un BodyBase possédable
// Ces deux phases sont gérées par deux components séparés
// sur le même GameObject.
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BodyBase))]   // Le corps possédable est déjà là, inactif
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats Ennemi")]
    [SerializeField] protected float maxHealth  = 3f;
    [SerializeField] protected float moveSpeed  = 2f;
    [SerializeField] protected int   damage     = 1;

    [Header("Détection")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange    = 1f;
    [SerializeField] protected LayerMask playerLayer;

    [Header("Attaque")]
    [SerializeField] protected float attackCooldown = 1f;
    protected float attackCooldownTimer = 0f;

    // ---- État ----
    protected float         currentHealth;
    public bool          isAlive = true;
    protected Rigidbody2D   rb;
    protected SpriteRenderer spriteRenderer;
    protected BodyBase      bodyComponent;  // Activé à la mort
    public bool DyingFromLaunch { get; private set; } = false;

    // ---- State Machine simple ----
    protected enum EnemyState { Patrol, Alert, Attack, Dead }
    protected EnemyState currentState = EnemyState.Patrol;
    protected EnemyAnimator enemyAnimator;

    // ============================================================
    protected virtual void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        spriteRenderer  = GetComponent<SpriteRenderer>();
        bodyComponent   = GetComponent<BodyBase>();
        enemyAnimator   = GetComponent<EnemyAnimator>();
        currentHealth   = maxHealth;

         if (playerLayer.value == 0)
            playerLayer = LayerMask.GetMask("Player");

        if (bodyComponent != null)
            bodyComponent.enabled = false;

        BodyPhysics bp = GetComponent<BodyPhysics>();
        if (bp != null) bp.enabled = false; 
    }

    protected virtual void Update()
    {
        if (!isAlive) return;
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;
        UpdateStateMachine();
    }

    // ---- State Machine — à override dans les sous-classes ----
    protected virtual void UpdateStateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: OnPatrol(); break;
            case EnemyState.Alert:  OnAlert();  break;
            case EnemyState.Attack: OnAttack(); break;
        }
    }

    protected virtual void OnPatrol() { }
    protected virtual void OnAlert()  { }
    protected virtual void OnAttack() { }

    // ---- Dégâts normaux (depuis les attaques du corps possédé) ----
    public virtual void TakeDamage(float amount)
    {
        if (!isAlive) return;
        currentHealth -= amount;
        enemyAnimator?.TriggerHurt();
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0f) Die();
    }

    // ---- Dégâts depuis le transfert lancé ----
    // Retourne true si l'ennemi est mort du coup
    public bool TakeDamageFromLaunch(float amount)
    {
        if (!isAlive) return false;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            DyingFromLaunch = true;
            Die();
            return true;   // Mort → possession possible
        }

        return false;      // Survie → lancer échoué
    }

    // ---- Mort de l'ennemi ----
    protected virtual void Die()
    {
        isAlive      = false;
        currentState = EnemyState.Dead;
        enemyAnimator?.TriggerDeath();

        // Arrête le mouvement
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Kinematic; // Kinematic comme BodyBase

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        // Change le layer vers Body — devient détectable par TransferSystem
        gameObject.layer = LayerMask.NameToLayer("Body");

        BodyPhysics bp = GetComponent<BodyPhysics>();
        if (bp != null) bp.enabled = true;

        // Active le BodyBase — l'ennemi mort devient possédable
        if (bodyComponent != null)
            bodyComponent.enabled = true;

        // Feedback visuel
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.4f, 0.15f, 0.15f); // Rouge sombre = mort
    }

    // ---- Flash rouge aux dégâts ----
    private System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original          = spriteRenderer.color;
        spriteRenderer.color    = Color.red;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color    = original;
    }

    // ---- Détection du joueur ----
    protected bool PlayerInRange(float range)
    {
        BodyBase current = TransferSystem.Instance?.CurrentBody;
        if (current == null) return false;
        
        float dist = Vector2.Distance(transform.position, current.transform.position);

        return dist <= range;
    }

    protected Transform GetPlayerTransform()
    {
        return TransferSystem.Instance?.CurrentBody?.transform;
    }

    // ---- Debug Gizmos — visualisation des zones de détection/attaque ----
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}