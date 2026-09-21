// BodyPhysics.cs
// Gère le mouvement physique d'un corps possédé.
// Ce script ne sait PAS qui le contrôle — il reçoit des ordres
// via des méthodes publiques appelées par le système de possession.
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class BodyPhysics : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 14f;

    [Header("Détection du sol")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Feel du saut")]
    [SerializeField] private float fallMultiplier = 2.5f;
    // Rend la chute plus rapide que la montée — donne un saut
    // plus "solide", moins flottant
    [SerializeField] private float lowJumpMultiplier = 2f;
    // Si le joueur relâche le saut tôt, le personnage retombe
    // plus vite — permet des sauts courts ou longs

    // ---- État interne ----
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public bool isGrounded;
    private bool jumpRequested;
    private float horizontalInput;

    // ---- Modificateur de vitesse (pour les Résonances coop) ----
    private float speedModifier = 1f;

    // ---- Événements ----
    public event Action OnLanded;

    // ============================================================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy != null && enemy.isAlive) return;

        if (rb.bodyType == RigidbodyType2D.Static) return;

        CheckGround();
        ApplyMovement();
        ApplyJump();
        ApplyBetterGravity();
        PreventWallSlide();
    }

    // ---- Détection du sol ----
    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        
        // OverlapCircle invisible centré sur groundCheckPoint
        // (un empty GO qu'on placera sous les pieds du corps)
        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );

        // Déclenche l'événement quand on touche le sol
        if (isGrounded && !wasGrounded)
            OnLanded?.Invoke();
    }

    // ---- Mouvement horizontal ----
    private void ApplyMovement()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

        float targetSpeed = horizontalInput * moveSpeed * speedModifier;
        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        // Flip du sprite selon la direction
        if (horizontalInput > 0.01f)       spriteRenderer.flipX = false;
        else if (horizontalInput < -0.01f) spriteRenderer.flipX = true;
    }

    // ---- Saut ----
    private void ApplyJump()
    {
        if (!jumpRequested) return;
        if (!isGrounded) { jumpRequested = false; return; }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpRequested = false;
    }

    // ---- Gravité améliorée (Better Jumping) ----
    // Technique classique Unity : on ajoute de la gravité supplémentaire
    // pendant la chute, et si le bouton saut est relâché pendant la montée
    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // En chute : gravité × fallMultiplier
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (fallMultiplier - 1)
                * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            // En montée avec bouton relâché : retombe plus vite
            rb.linearVelocity += Vector2.up
                * Physics2D.gravity.y
                * (lowJumpMultiplier - 1)
                * Time.fixedDeltaTime;
        }
    }

    // ---- Séparé de ApplyBetterGravity pour la lisibilité ----
    private bool jumpHeld = false;

    // ============================================================
    // API PUBLIQUE — appelée par le système de contrôle
    // Ces méthodes sont le seul point d'entrée pour contrôler ce corps
    // ============================================================

    // Appelée chaque Update par le contrôleur actif
    public void SetMoveInput(float horizontal)
    {
        horizontalInput = horizontal;
    }

    // Appelée quand le bouton saut est pressé
    public void RequestJump()
    {
        jumpRequested = true;
    }

    // Appelée chaque frame pour le better jumping
    public void SetJumpHeld(bool held)
    {
        jumpHeld = held;
    }

    // Applique un knockback (dégâts, explosion, etc.)
    public void ApplyKnockback(Vector2 direction, float force)
    {
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    // Modificateur de vitesse pour les Résonances (valeur 0.2 = +20%)
    public void ModifySpeed(float delta)
    {
        speedModifier = Mathf.Clamp(speedModifier + delta, 0.1f, 3f);
    }

    // Reset à la possession d'un nouveau corps
    public void ResetModifiers()
    {
        speedModifier = 1f;
        horizontalInput = 0f;
        jumpRequested = false;
    }

    private void PreventWallSlide()
    {
        // Si le corps est en l'air ET appuie contre un mur
        // → annule la vélocité X pour éviter le collage
        if (isGrounded) return;

        // Détecte un mur à gauche ou à droite
        Vector2 direction  = horizontalInput > 0 ? Vector2.right : Vector2.left;
        bool    touchesWall = horizontalInput != 0 && Physics2D.OverlapCircle(
            (Vector2)transform.position + direction * 0.45f,
            0.1f,
            LayerMask.GetMask("Ground")
        );

        if (touchesWall)
        {
            // Stoppe le déplacement horizontal sans bloquer la chute
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    // ---- Getters ----
    public bool IsGrounded => isGrounded;
    public float SpeedModifier => speedModifier;
}