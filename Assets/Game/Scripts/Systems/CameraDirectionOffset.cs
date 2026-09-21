// CameraDirectionOffset.cs
// Inverse le Screen X de la caméra selon la direction du joueur.
// Donne l'impression que la caméra "regarde devant" en permanence.
using UnityEngine;
using Unity.Cinemachine;

public class CameraDirectionOffset : MonoBehaviour
{
    [SerializeField] private CinemachineCamera    cinemachineCam;
    [SerializeField] private float                offsetAmount   = 0.15f; // Décalage en unités monde
    [SerializeField] private float                smoothSpeed    = 3f;    // Vitesse de transition

    private CinemachinePositionComposer composer;
    private float                        targetOffsetX = 0f;

    private void Awake()
    {
        if (cinemachineCam != null)
            composer = cinemachineCam.GetComponent<CinemachinePositionComposer>();
    }

    private void Update()
    {
        if (TransferSystem.Instance == null || composer == null) return;

        BodyBase currentBody = TransferSystem.Instance.CurrentBody;
        if (currentBody == null) return;

        // Récupère la direction du corps actif via son SpriteRenderer
        SpriteRenderer sr = currentBody.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Si le sprite est flippé → regarde à gauche → offset négatif
        targetOffsetX = sr.flipX ? -offsetAmount : offsetAmount;

        // Lisse la transition pour éviter un saut brutal de caméra
        Vector3 currentOffset = composer.TargetOffset;
        currentOffset.x       = Mathf.Lerp(
                                    currentOffset.x,
                                    targetOffsetX,
                                    smoothSpeed * Time.deltaTime);
        composer.TargetOffset = currentOffset;
    }
}