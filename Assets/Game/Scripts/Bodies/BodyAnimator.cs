// BodyAnimator.cs
// Contrôle les animations du corps possédé.
// Se connecte à BodyPhysics pour lire la vélocité
// et à BodyBase pour les états possession/mort.
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BodyPhysics))]
public class BodyAnimator : MonoBehaviour
{
    private Animator     animator;
    private BodyPhysics  bodyPhysics;
    private Rigidbody2D  rb;
    private BodyBase     bodyBase;
    private int lastHealth = -1;

    // Hash des paramètres — plus performant que les strings
    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int HurtHash       = Animator.StringToHash("Hurt");
    private static readonly int DeathHash      = Animator.StringToHash("Death");
    private static readonly int AttackHash     = Animator.StringToHash("Attack");


    private void Awake()
    {
        animator    = GetComponent<Animator>();
        bodyPhysics = GetComponent<BodyPhysics>();
        rb          = GetComponent<Rigidbody2D>();
        bodyBase    = GetComponent<BodyBase>();
    }

    private void Start()
    {
        if (bodyBase != null)
        {
            bodyBase.GetBodyHealth().OnDeath         += OnDeath;
            bodyBase.GetBodyHealth().OnHealthChanged += OnHurt;
        }
        else
        {
            Debug.LogWarning($"[BodyAnimator:{name}] Pas de BodyBase trouvé !");
        }
    }

    private void Update()
    {
        if (rb == null) return;

        // Vitesse horizontale → transition Idle/Run
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(SpeedHash, speed);

        // IsGrounded → transition Jump/Fall/Land
        animator.SetBool(IsGroundedHash, bodyPhysics.IsGrounded);
    }

    private void OnDeath()
    {
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(SpeedHash, speed);
        animator.SetBool(IsGroundedHash, bodyPhysics.IsGrounded);
    }

    private void OnHurt(int current, int max)
    {
        // Déclenche Hurt uniquement si la vie a diminué
        if (lastHealth == -1) { lastHealth = current; return; }
        if (current < lastHealth)
            animator.SetTrigger(HurtHash);
        lastHealth = current;
    }

    private void OnDestroy()
    {
        // OnDestroy plutôt que OnDisable — évite les désabonnements intempestifs
        if (bodyBase != null)
        {
            bodyBase.GetBodyHealth().OnDeath         -= OnDeath;
            bodyBase.GetBodyHealth().OnHealthChanged -= OnHurt;
        }
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(AttackHash);
    }
}