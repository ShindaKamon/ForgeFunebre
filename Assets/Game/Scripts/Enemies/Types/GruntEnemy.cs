// GruntEnemy.cs — Version A* Pathfinding
// Patrouille manuelle + chasse via A* quand le joueur détecté
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class GruntEnemy : EnemyBase
{
    [Header("Patrouille")]
    [SerializeField] private float patrolDistance   = 3f;
    [SerializeField] private float waitTimeAtEnd    = 1f;

    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateRate   = 0.5f;  // Recalcule le chemin toutes les 0.5s
    [SerializeField] private float nextWpDistance   = 0.5f;  // Distance pour passer au waypoint suivant
    [SerializeField] private float chaseSpeed       = 3f;    // Vitesse de chasse

    [Header("Détection de bord")]
    [SerializeField] private float     edgeCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;

    // ---- Patrouille ----
    private Vector2 patrolStart;
    private Vector2 patrolEnd;
    private bool    movingRight = true;
    private float   waitTimer   = 0f;
    private bool    isWaiting   = false;

    // ---- A* ----
    private Seeker   seeker;
    private Path     path;
    private int      currentWaypoint = 0;
    private float    pathTimer       = 0f;

    // ============================================================
    protected override void Awake()
    {
        base.Awake();
        seeker      = GetComponent<Seeker>();
        patrolStart = (Vector2)transform.position - Vector2.right * patrolDistance;
        patrolEnd   = (Vector2)transform.position + Vector2.right * patrolDistance;

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    // ---- Patrouille manuelle — pas de A* ----
    protected override void OnPatrol()
    {
        Debug.Log($"[Grunt:{name}] OnPatrol — PlayerInRange={PlayerInRange(detectionRange)}");
        if (PlayerInRange(detectionRange))
        {
            currentState = EnemyState.Alert;
            // Lance le premier calcul de chemin
            UpdatePath();
            return;
        }

        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) isWaiting = false;
            return;
        }

        if (IsAtEdge())
        {
            TurnAround();
            return;
        }

        float targetX   = movingRight ? patrolEnd.x : patrolStart.x;
        float direction = movingRight ? 1f : -1f;

        if ((movingRight  && transform.position.x >= targetX) ||
            (!movingRight && transform.position.x <= targetX))
        {
            TurnAround();
            return;
        }

        rb.linearVelocity    = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        spriteRenderer.flipX = movingRight;
    }

    // ---- Alerte — suit le joueur via A* ----
    protected override void OnAlert()
    {
        Debug.Log($"[Grunt:{name}] OnAlert appelé");
        Transform player = GetPlayerTransform();

        if (player == null)
        {
            currentState = EnemyState.Patrol;
            path         = null;
            return;
        }

        if (PlayerInRange(attackRange))
        {
            currentState = EnemyState.Attack;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Recalcule le chemin périodiquement
        pathTimer += Time.deltaTime;
        if (pathTimer >= pathUpdateRate)
        {
            pathTimer = 0f;
            UpdatePath();
        }

        // Suit le chemin A*
        FollowPath();
    }

    // ---- Attaque ----
    protected override void OnAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        Transform player = GetPlayerTransform();

        if (player == null || !PlayerInRange(attackRange * 1.5f))
        {
            currentState = EnemyState.Alert;
            return;
        }
        if (attackCooldownTimer > 0f)
            return;

        enemyAnimator?.TriggerAttack();

        BodyHealth health = player.GetComponent<BodyHealth>();
        if (health == null) health = player.GetComponentInParent<BodyHealth>();
        if (health != null) health.TakeDamage(damage);

        currentState = EnemyState.Alert;
    }

    // ---- Calcule un nouveau chemin vers le joueur ----
    private void UpdatePath()
    {
        Transform player = GetPlayerTransform();
        if (player == null || !seeker.IsDone()) return;

        seeker.StartPath(rb.position, player.position, OnPathComplete);
    }

    // ---- Callback quand le chemin est calculé ----
    private void OnPathComplete(Path p)
    {
        if (p.error) return;
        path            = p;
        currentWaypoint = 0;
    }

    // ---- Suit le chemin waypoint par waypoint ----
    private void FollowPath()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // Direction vers le prochain waypoint
        Vector2 target    = (Vector2)path.vectorPath[currentWaypoint];
        Vector2 direction = (target - rb.position).normalized;

        // Déplacement horizontal uniquement — la gravité gère le vertical
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        Debug.Log($"[Grunt] direction.x={direction.x:F2} flipX={spriteRenderer.flipX}");

        spriteRenderer.flipX = direction.x > 0;

        // Passe au waypoint suivant si assez proche
        float distance = Vector2.Distance(rb.position, target);
        if (distance < nextWpDistance)
            currentWaypoint++;
    }

    // ---- Détection de bord (patrouille uniquement) ----
    private bool IsAtEdge()
    {
        Vector2 checkPos = (Vector2)transform.position
                         + Vector2.right * (movingRight ? edgeCheckDistance : -edgeCheckDistance)
                         + Vector2.down  * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(
            checkPos, Vector2.down, edgeCheckDistance, groundLayer);

        Debug.DrawRay(checkPos, Vector2.down * edgeCheckDistance,
            hit.collider != null ? Color.green : Color.red);

        return hit.collider == null;
    }

    private void TurnAround()
    {
        movingRight          = !movingRight;
        isWaiting            = true;
        waitTimer            = waitTimeAtEnd;
        rb.linearVelocity    = new Vector2(0f, rb.linearVelocity.y);
        spriteRenderer.flipX = !movingRight;
    }
}