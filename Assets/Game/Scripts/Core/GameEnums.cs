// GameEnums.cs
// Toutes les énumérations partagées du projet.
// Centralisées ici pour éviter les dépendances circulaires.
// Quand tu as besoin d'un nouvel enum, ajoute-le ici.

public enum BodyType
{
    Porter,
    Pillard,
    Guard,
    Gladiator,
    Devout,
    Alchemist,
    Specter,
    Giant
}

public enum DeathCause
{
    Decay,                  // Jauge de pourriture à zéro
    Damage,                 // Plus de PV
    EnvironmentalCondition, // Eau, lumière, acide...
    Sacrifice               // Éjection volontaire forcée
}

public enum EnvironmentType
{
    Water,
    Sunlight,
    AcidGas,
    BloodContact,
    Fire
}

public enum SoulType
{
    Morthis,
    Verak
}

public enum TransferMode
{
    Contact,
    Launch,
    Plunge,
    Resonance,
    Echo
}