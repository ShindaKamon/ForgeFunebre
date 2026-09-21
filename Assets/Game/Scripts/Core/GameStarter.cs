// GameStarter.cs
// Script temporaire qui initialise la run au démarrage de la scène.
// Sera remplacé par GameManager plus tard.
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private BodyBase startBody;
    [SerializeField] private HUDManager hudManager;

    private void Start()
    {
        // Attend une frame que tout soit initialisé
        if (TransferSystem.Instance != null && startBody != null)
        {
            TransferSystem.Instance.PossessInitialBody(startBody);

            EventBus.Publish(new OnTransferComplete
            {
                Previous = null,
                Next     = startBody
            });
        }
    }
}