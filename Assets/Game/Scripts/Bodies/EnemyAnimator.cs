// EnemyAnimator.cs
// Gère les animations des ennemis.
// Se connecte à EnemyBase pour les états et événements.
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator  animator;
    private EnemyBase enemyBase;
    private Rigidbody2D rb;

    private static readonly int SpeedHash  = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HurtHash   = Animator.StringToHash("Hurt");
    private static readonly int DeathHash  = Animator.StringToHash("Death");

    private void Awake()
    {
        animator  = GetComponent<Animator>();
        enemyBase = GetComponent<EnemyBase>();
        rb        = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (enemyBase == null || !enemyBase.isAlive) return;

        // Vitesse horizontale → Walk/Idle
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat(SpeedHash, speed);
    }

    public void TriggerAttack() => animator.SetTrigger(AttackHash);
    public void TriggerHurt()   => animator.SetTrigger(HurtHash);
    public void TriggerDeath()  => animator.SetTrigger(DeathHash);
}