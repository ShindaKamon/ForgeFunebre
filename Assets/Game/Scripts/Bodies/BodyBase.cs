// BodyBase.cs — Version Étape 5
// Orchestre DecaySystem et BodyHealth.
// La logique de pourriture et de PV est déléguée
// aux composants spécialisés.
using UnityEngine;

[RequireComponent(typeof(BodyPhysics))]
[RequireComponent(typeof(DecaySystem))]
[RequireComponent(typeof(BodyHealth))]
public abstract class BodyBase : MonoBehaviour
{
    [Header("Identité")]
    [SerializeField] private BodyType bodyType;
    [SerializeField] private string   bodyDisplayName = "Corps inconnu";

    [Header("Conditions spéciales")]
    [SerializeField] private EnvironmentType[] fatalEnvironments;

    [Header("État initial")]
    [SerializeField] private bool startsAsDead = false;

    // ---- État ----
    private bool isPossessed = false;
    private bool isDead      = false;

    // ---- Références ----
    protected BodyPhysics    bodyPhysics;
    protected SpriteRenderer spriteRenderer;
    private   DecaySystem    decaySystem;
    private   BodyHealth     bodyHealth;

    // ============================================================
    protected virtual void Awake()
    {
        bodyPhysics    = GetComponent<BodyPhysics>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        decaySystem    = GetComponent<DecaySystem>();
        bodyHealth     = GetComponent<BodyHealth>();

        // Connecte les événements des sous-systèmes
        decaySystem.OnDecayDepleted += () => Die(DeathCause.Decay);
        bodyHealth.OnDeath          += () => Die(DeathCause.Damage);
    }

    protected virtual void Start()
    {
        if (startsAsDead)
            InitializeAsDead();
    }

    // Initialise le corps directement en état cadavre possédable
    private void InitializeAsDead()
    {
        isDead = true;
        gameObject.layer = LayerMask.NameToLayer("Body");
        decaySystem.StopDecay();

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.4f, 0.15f, 0.15f);

        // Laisse la physique normale une frame pour qu'il tombe sur le sol
        // puis on le fige via coroutine
        StartCoroutine(FreezeAfterLanding());
    }

    // ---- Environnement ----
    public void CheckEnvironment(EnvironmentType environment)
    {
        if (isDead) return;
        foreach (var fatal in fatalEnvironments)
            if (fatal == environment) { Die(DeathCause.EnvironmentalCondition); return; }
    }

    // ---- Dégâts — délègue à BodyHealth ----
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        bodyHealth.TakeDamage(amount);
    }

    // ---- Mort ----
    protected virtual void Die(DeathCause cause)
    {
        if (isDead) return;
        isDead = true;

        decaySystem.StopDecay();

        if (isPossessed)
            TransferSystem.Instance?.ForceEject(this, cause);

        bodyPhysics.SetMoveInput(0f);

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f);

        EventBus.Publish(new OnBodyDied { Body = this, Cause = cause });
        Invoke(nameof(Deactivate), 3f);
    }

    // Mort au transfert volontaire
    public void DieFromTransfer()
    {
        // Si le corps était déjà mort (startsAsDead) on skip la logique de mort
        // mais on remet quand même l'apparence cadavre
        isPossessed = false;

        decaySystem.StopDecay();
        bodyPhysics.SetMoveInput(0f);

        // Remet en état cadavre — Kinematic + trigger + couleur grise
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        gameObject.layer = LayerMask.NameToLayer("Body");

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f); // Gris cadavre

        // Si pas encore marqué mort — le marque et programme la désactivation
        if (!isDead)
        {
            isDead = true;
            EventBus.Publish(new OnBodyDied { Body = this, Cause = DeathCause.Sacrifice });
            Invoke(nameof(Deactivate), 3f);
        }
        // Si déjà mort (startsAsDead) — disparaît aussi après délai
        else
        {
            Invoke(nameof(Deactivate), 3f);
        }
    }

    private void Deactivate() => gameObject.SetActive(false);

    // ---- Possession ----
    public virtual void OnPossess()
    {
        isPossessed = true;
        isDead = false;

        Rigidbody2D rb    = GetComponent<Rigidbody2D>();
        rb.bodyType       = RigidbodyType2D.Dynamic;
        Collider2D col    = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;

        gameObject.layer  = LayerMask.NameToLayer("Player");

        decaySystem.StartDecay();
        bodyHealth.ResetHealth();

        if (spriteRenderer != null)
            spriteRenderer.color = GetPossessedColor();
    }

    public virtual void OnRelease()
    {
        isPossessed = false;
        decaySystem.StopDecay();
        bodyPhysics.SetMoveInput(0f);
        bodyPhysics.ResetModifiers();

        gameObject.layer = LayerMask.NameToLayer("Body");

        if (spriteRenderer != null)
            spriteRenderer.color = GetBaseColor();

        EventBus.Publish(new OnBodyReleased { Body = this });
    }

    // ---- Couleurs ----
    protected virtual Color GetBaseColor()      => Color.white;
    protected virtual Color GetPossessedColor() => new Color(0.6f, 0.9f, 1f);

    // ---- Capacité spéciale ----
    public abstract void UseSpecialAbility();

    // ---- Getters ----
    public BodyType   GetBodyType()        => bodyType;
    public string     GetDisplayName()     => bodyDisplayName;
    public bool       IsPossessed          => isPossessed;
    public bool       IsDead               => isDead;
    public BodyPhysics GetBodyPhysics()    => bodyPhysics;
    public DecaySystem GetDecaySystem()    => decaySystem;
    public BodyHealth  GetBodyHealth()     => bodyHealth;

    public bool CanBePossessed
    {
        get
        {
            if (isDead && !isPossessed) return true;
            EnemyBase enemy = GetComponent<EnemyBase>();
            if (enemy != null && enemy.DyingFromLaunch && !isPossessed) return true;
            return false;
        }
    }

    private System.Collections.IEnumerator FreezeAfterLanding()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        // Reste Dynamic pour que la gravité agisse
        rb.bodyType       = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        
        // Attend quelques frames que la physique démarre
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Attend l'atterrissage — détecte via BodyPhysics
        float timeout = 3f; // Sécurité — max 3 secondes d'attente
        while (!bodyPhysics.IsGrounded && timeout > 0f)
        {
            timeout -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Attend encore 2 frames pour que le corps soit
        // bien stabilisé sur le sol avant de figer
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Fige la position exacte au sol
        rb.bodyType        = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        

        // Le isTrigger APRÈS le Kinematic — évite le saut résiduel
        yield return new WaitForFixedUpdate();
        if (col != null) col.isTrigger = true;
    }

    public void AddDecayTime(float seconds) => decaySystem.AddTime(seconds);
}