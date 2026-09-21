// TransferSystem.cs — Version Étape 4
// Logique complète : transfert Contact et Lancé.
// Règle centrale : on choisit sa cible AVANT de quitter.
// L'ancien corps meurt au moment du transfert.
// Le transfert lancé tue l'ennemi si ses PV tombent à 0.
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class TransferSystem : MonoBehaviour
{
    public static TransferSystem Instance { get; private set; }

    [Header("Système de Visée")]
    [SerializeField] private LineRenderer aimLine;        // LineRenderer sur Morthis
    [SerializeField] private float        maxAimRange   = 8f;  // Portée max de la visée
    [SerializeField] private Color        aimColorFree  = Color.white;   // Aucune cible
    [SerializeField] private Color        aimColorEnemy = Color.red;     // Cible ennemi
    [SerializeField] private Color        aimColorBody  = Color.cyan;    // Cible corps mort

    private bool    isAiming       = false;
    private Vector2 aimDirection   = Vector2.right;
    private bool    aimHitEnemy    = false;   // Pour colorer la ligne
    private bool    aimHitBody     = false;

    [Header("Transfert Contact")]
    [SerializeField] private float contactRange = 2f;
    [SerializeField] private LayerMask bodyLayer;       // Corps morts possédables
    [SerializeField] private LayerMask enemyLayer;      // Ennemis vivants (transfert lancé)

    [Header("Transfert Lancé")]
    [SerializeField] private float launchSpeed = 18f;   // Vitesse du projectile Morthis
    [SerializeField] private float launchDamage = 999f; // Dégâts — tue la plupart des ennemis
    [SerializeField] private float launchCooldown = 1f;
    private float launchCooldownTimer = 0f;

    [Header("Visuel — Morthis libre")]
    [SerializeField] private GameObject morthisProjectilePrefab; // Sprite de l'arme en vol

    [Header("Attaque au contact")]
    [SerializeField] private float attackRange  = 1.5f;
    [SerializeField] private int   attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.5f;
    private float attackCooldownTimer = 0f;

    // ---- État ----
    private BodyBase currentBody = null;
    private bool isTransferring  = false;   // Empêche les doubles transferts

    // ---- Input ----
    private ForgeFunebre_InputActions inputActions;

    // ---- Caméra ----
    private CinemachineCamera cinemachineCam;

    // ============================================================
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        inputActions = new ForgeFunebre_InputActions();

        // La ligne de visée est cachée par défaut
        if (aimLine != null)
            aimLine.enabled = false;
    }

    private void Start()
    {
        cinemachineCam = FindAnyObjectByType<CinemachineCamera>();
    }

    private void OnEnable()  => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        if (launchCooldownTimer > 0f)
            launchCooldownTimer -= Time.deltaTime;

        if (isTransferring) return;

        if (attackCooldownTimer > 0f)
        attackCooldownTimer -= Time.deltaTime;

        HandleMovementInput();
        HandleTransferInput();
    }

    // ---- Mouvement du corps actif ----
    private void HandleMovementInput()
    {
        if (currentBody == null) return;

        BodyPhysics physics = currentBody.GetBodyPhysics();
        Vector2 move        = inputActions.Player.Move.ReadValue<Vector2>();

        physics.SetMoveInput(move.x);

        if (inputActions.Player.Jump.WasPressedThisFrame())
            physics.RequestJump();

        physics.SetJumpHeld(inputActions.Player.Jump.IsPressed());
    }

    // ---- Lecture des inputs de transfert ----
    private void HandleTransferInput()
    {
        // Contact : touche C
        if (inputActions.Player.TransferContact.WasPressedThisFrame())
            TryContactTransfer();

        // Visée : clic gauche maintenu
        if (inputActions.Player.PrimaryAttack.WasPressedThisFrame())
            StartAiming();

        if (isAiming)
        {
            UpdateAim();

            // Relâche → lance
            if (inputActions.Player.SpecialAttack.WasReleasedThisFrame())
            {
                StopAiming();
                if (launchCooldownTimer <= 0f)
                {
                    StartCoroutine(LaunchProjectile(aimDirection));
                    launchCooldownTimer = launchCooldown;
                }
            }
        }

        if (inputActions.Player.PrimaryAttack.WasPressedThisFrame())
        TryMeleeAttack();
    }

    // ============================================================
    // TRANSFERT CONTACT
    // Cherche le corps mort le plus proche dans le rayon
    // ============================================================
    private void TryContactTransfer()
    {
        if (currentBody == null) return;

        Vector2 origin = (Vector2)currentBody.transform.position;

        Collider2D[] hits    = Physics2D.OverlapCircleAll(origin, contactRange, bodyLayer);
        BodyBase     target  = null;
        float        closest = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<BodyBase>(out var body)) continue;
            if (!body.CanBePossessed) continue;   // Doit être mort et libre
            if (body == currentBody) continue;

            float dist = Vector2.Distance(origin, hit.transform.position);
            if (dist < closest) { closest = dist; target = body; }
        }

        if (target != null)
            ExecuteTransfer(target);
        else
            Debug.Log("[TransferSystem] Aucun corps mort à portée.");
    }

    // ============================================================
    // TRANSFERT LANCÉ
    // Lance Morthis comme projectile vers la cible visée
    // Si l'ennemi meurt du coup → possession immédiate
    // Si l'ennemi survit → le lancer échoue, on reste dans le corps actuel
    // ============================================================
    private IEnumerator LaunchProjectile(Vector2 direction)
    {
        isTransferring = true;

        Vector2 startPos  = (Vector2)currentBody.transform.position;
        Vector2 currentPos = startPos;
        float   maxRange  = maxAimRange;
        float   travelled = 0f;

        // Instancie le visuel de l'arme en vol si un prefab est assigné
        GameObject projVisual = null;
        if (morthisProjectilePrefab != null)
            projVisual = Instantiate(morthisProjectilePrefab, startPos, Quaternion.identity);

        while (travelled < maxRange)
        {
            float step    = launchSpeed * Time.deltaTime;
            currentPos   += direction * step;
            travelled    += step;

            // Met à jour le visuel
            if (projVisual != null)
                projVisual.transform.position = currentPos;

            // Vérifie les collisions sur le chemin
            Collider2D hitEnemy = Physics2D.OverlapCircle(currentPos, 0.3f, enemyLayer);
            if (hitEnemy != null && hitEnemy.TryGetComponent<EnemyBase>(out var enemy))
            {
                // Inflige les dégâts
                bool killed = enemy.TakeDamageFromLaunch(launchDamage);

                if (killed)
                {
                    // Récupère le BodyBase sur le même GO que l'ennemi mort
                    if (hitEnemy.TryGetComponent<BodyBase>(out var newBody))
                    {
                        if (projVisual != null) Destroy(projVisual);
                        ExecuteTransfer(newBody);
                        yield break;
                    }
                }

                // L'ennemi a survécu → le lancer échoue
                Debug.Log("[TransferSystem] Lancer échoué — ennemi trop résistant.");
                if (projVisual != null) Destroy(projVisual);
                isTransferring = false;
                yield break;
            }

            // Vérifie aussi les corps morts sur le chemin
            Collider2D hitBody = Physics2D.OverlapCircle(currentPos, 0.3f, bodyLayer);
            if (hitBody != null && hitBody.TryGetComponent<BodyBase>(out var deadBody)
                && deadBody.CanBePossessed)
            {
                if (projVisual != null) Destroy(projVisual);
                ExecuteTransfer(deadBody);
                yield break;
            }

            // Vérifie les obstacles (murs)
            Collider2D hitWall = Physics2D.OverlapCircle(currentPos, 0.2f, LayerMask.GetMask("Ground"));
            if (hitWall != null)
            {
                Debug.Log("[TransferSystem] Lancer bloqué par un mur.");
                if (projVisual != null) Destroy(projVisual);
                isTransferring = false;
                yield break;
            }

            yield return null;
        }

        // Portée max atteinte sans toucher de cible
        if (projVisual != null) Destroy(projVisual);
        isTransferring = false;
    }

    // ============================================================
    // EXÉCUTION DU TRANSFERT — commun Contact et Lancé
    // L'ancien corps meurt, le nouveau est possédé immédiatement
    // ============================================================
    private void ExecuteTransfer(BodyBase target)
    {
        isTransferring = true;

        BodyBase previous = currentBody;

        // 1. L'ancien corps meurt
        if (previous != null)
            previous.DieFromTransfer();

        // 2. Possède le nouveau corps immédiatement
        currentBody = target;
        currentBody.OnPossess();

        // 3. Met à jour la caméra
        UpdateCameraTarget(currentBody.transform);

        isTransferring = false;

        EventBus.Publish(new OnTransferComplete
        {
            Previous = previous,
            Next     = currentBody
        });

        Debug.Log($"[TransferSystem] Possède : {currentBody.GetDisplayName()}");
    }

    // ---- Éjection forcée (corps qui meurt pendant la possession) ----
    // Dans ce cas — Game Over, pas d'état intermédiaire
    public void ForceEject(BodyBase dyingBody, DeathCause cause)
    {
        if (currentBody != dyingBody) return;

        currentBody = null;

        // Cherche un corps de secours en contact immédiat
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            dyingBody.transform.position, contactRange, bodyLayer);

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<BodyBase>(out var body)) continue;
            if (!body.CanBePossessed) continue;

            ExecuteTransfer(body);
            return;
        }

        // Aucun corps disponible → Game Over
        Debug.Log($"[TransferSystem] Game Over — {cause}");
        EventBus.Publish(new OnRunEnded { Success = false });
    }

    // ---- Initialisation au début de la run ----
    public void PossessInitialBody(BodyBase startBody)
    {
        currentBody = startBody;
        currentBody.OnPossess();
        UpdateCameraTarget(currentBody.transform);
    }

    // ---- Caméra ----
    private void UpdateCameraTarget(Transform target)
    {
        if (cinemachineCam != null)
            cinemachineCam.Target.TrackingTarget = target;
    }

    public BodyBase CurrentBody => currentBody;
    public float GetLaunchCooldownRatio() =>
        1f - Mathf.Clamp01(launchCooldownTimer / launchCooldown);


    private void StartAiming()
    {
        if (currentBody == null) return;
        isAiming = true;

        if (aimLine != null)
            aimLine.enabled = true;
    }

    private void UpdateAim()
    {
        if (currentBody == null) { StopAiming(); return; }

        // Direction vers la souris
        Vector2 mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 mouseWorld  = Camera.main.ScreenToWorldPoint(
                                new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorld.z        = 0f;

        Vector2 origin    = (Vector2)currentBody.transform.position;
        aimDirection      = ((Vector2)mouseWorld - origin).normalized;

        // Raycast pour trouver ce qu'on vise
        aimHitEnemy = false;
        aimHitBody  = false;
        float       lineLength = maxAimRange;

        // Vérifie si on vise un ennemi
        RaycastHit2D hitEnemy = Physics2D.Raycast(origin, aimDirection, maxAimRange, enemyLayer);
        if (hitEnemy.collider != null)
        {
            aimHitEnemy = true;
            lineLength  = hitEnemy.distance;
        }

        // Vérifie si on vise un corps mort
        RaycastHit2D hitBody = Physics2D.Raycast(origin, aimDirection, maxAimRange, bodyLayer);
        if (hitBody.collider != null && hitBody.distance < lineLength)
        {
            aimHitEnemy = false;
            aimHitBody  = true;
            lineLength  = hitBody.distance;
        }

        // Vérifie si un mur bloque avant la cible
        RaycastHit2D hitWall = Physics2D.Raycast(
            origin, aimDirection, lineLength, LayerMask.GetMask("Ground"));
        if (hitWall.collider != null)
        {
            lineLength  = hitWall.distance;
            aimHitEnemy = false;
            aimHitBody  = false;
        }

        DrawAimLine(origin, aimDirection, lineLength);
    }

    private void DrawAimLine(Vector2 origin, Vector2 direction, float length)
    {
        if (aimLine == null) return;

        Color lineColor    = aimColorFree;
        if (aimHitEnemy)   lineColor = aimColorEnemy;
        if (aimHitBody)    lineColor = aimColorBody;

        // Pointillés : on place des paires de points (début/fin de chaque tiret)
        // Les "espaces" entre les tirets sont créés en superposant deux points identiques
        float dotLength  = 0.12f;  // Longueur d'un tiret
        float gapLength  = 0.18f;  // Longueur d'un espace entre tirets
        float travelled  = 0f;
        bool  drawing    = true;   // Alterne tiret / espace

        var positions = new System.Collections.Generic.List<Vector3>();

        while (travelled < length)
        {
            float segmentLength = drawing ? dotLength : gapLength;
            float endDist       = Mathf.Min(travelled + segmentLength, length);

            if (drawing)
            {
                // Ajoute le tiret visible
                positions.Add(origin + direction * travelled);
                positions.Add(origin + direction * endDist);
            }
            else
            {
                // Ajoute deux points superposés — segment invisible de longueur 0
                Vector3 gapPoint = origin + direction * travelled;
                positions.Add(gapPoint);
                positions.Add(gapPoint);
            }

            travelled += segmentLength;
            drawing    = !drawing;
        }

        // Dégradé sur la longueur — plus transparent vers la cible
        aimLine.startColor    = new Color(lineColor.r, lineColor.g, lineColor.b, 0.9f);
        aimLine.endColor      = new Color(lineColor.r, lineColor.g, lineColor.b, 0.15f);
        aimLine.positionCount = positions.Count;
        aimLine.SetPositions(positions.ToArray());
    }

    private void StopAiming()
    {
        isAiming = false;
        if (aimLine != null)
        {
            aimLine.enabled       = false;
            aimLine.positionCount = 0;
        }
    }

    private void TryMeleeAttack()
    {
        if (currentBody == null) return;
        if (attackCooldownTimer > 0f) return;

        // Déclenche l'animation
        BodyAnimator anim = currentBody.GetComponent<BodyAnimator>();
        if (anim != null) anim.TriggerAttack();

        // Détecte les ennemis dans la portée
        Vector2 origin = (Vector2)currentBody.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out var enemy))
                enemy.TakeDamage(attackDamage);
        }

        attackCooldownTimer = attackCooldown;
        Debug.Log($"[TransferSystem] Attaque — {hits.Length} ennemi(s) touché(s)");
    }
}