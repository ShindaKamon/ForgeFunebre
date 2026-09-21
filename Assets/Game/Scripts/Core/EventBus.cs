// Assets/_Game/Scripts/Core/EventBus.cs
// Système d'événements typés — permet aux systèmes de communiquer
// sans se connaître directement. Fondation de toute l'architecture.
using System;
using System.Collections.Generic;

public static class EventBus
{
    // Dictionnaire : type d'événement → liste de listeners
    private static readonly Dictionary<Type, List<Delegate>> listeners
        = new Dictionary<Type, List<Delegate>>();

    // S'abonner à un événement
    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        Type type = typeof(T);
        if (!listeners.ContainsKey(type))
            listeners[type] = new List<Delegate>();
        listeners[type].Add(callback);
    }

    // Se désabonner (important dans OnDestroy !)
    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        Type type = typeof(T);
        if (listeners.ContainsKey(type))
            listeners[type].Remove(callback);
    }

    // Publier un événement
    public static void Publish<T>(T eventData) where T : struct
    {
        Type type = typeof(T);
        if (!listeners.ContainsKey(type)) return;

        // Copie la liste pour éviter les erreurs si un listener se désinscrit pendant l'appel
        var snapshot = new List<Delegate>(listeners[type]);
        foreach (var listener in snapshot)
            (listener as Action<T>)?.Invoke(eventData);
    }

    // Nettoyage complet (appelé au changement de scène)
    public static void Clear()
    {
        listeners.Clear();
    }
}

// ---- Définition des événements ----
// Convention : structs immuables, nommés en OnXxx

public struct OnBodyReleased     { public BodyBase Body; }
public struct OnBodyDied         { public BodyBase Body; public DeathCause Cause; }
public struct OnDecayChanged     { public float Ratio; }        // 0 = plein, 1 = mort
public struct OnTransferComplete { public BodyBase Previous; public BodyBase Next; }
public struct OnMorthisDied      { public DeathCause Cause; }
public struct OnRunStarted       { }
public struct OnRunEnded         { public bool Success; }
public struct OnFragmentCollected { public string FragmentId; }