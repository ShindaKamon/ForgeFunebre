# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Le projet

Forge Funèbre est un jeu de plateforme 2D Unity où l'on incarne un esprit (**Morthis**) sans corps propre, capable de posséder des cadavres (`BodyBase`) et des ennemis vaincus pour progresser. Chaque corps possédé a une jauge de pourriture qui le détruit avec le temps — il faut changer de corps avant qu'il ne s'effondre.

- Unity **6000.4.0f1**, render pipeline **URP**, projet 2D (Sprite/Tilemap/2D Animation).
- Pathfinding via l'asset tiers **A\* Pathfinding Project** (`Assets/AstarPathfindingProject`, namespace `Pathfinding`) — ne pas modifier ces fichiers, ce sont ceux du package.
- Input géré via le nouveau Input System : `Assets/Settings/ForgeFunebre_InputActions.inputactions` (classe générée `ForgeFunebre_InputActions`).
- Caméra : Cinemachine 3.x (`Unity.Cinemachine`), la cible de suivi est mise à jour dynamiquement à chaque transfert de corps.

## Commandes

Il n'y a pas de script de build/lint/test en ligne de commande dans ce dépôt — c'est un projet Unity ouvert et testé depuis l'Éditeur (Unity Hub → 6000.4.0f1). Le package `com.unity.test-framework` est présent mais aucun dossier de tests n'existe encore ; en créer un (`Tests/`, assembly definition dédiée) avant d'ajouter des Play/Edit Mode tests.

Pour lancer/valider une modification de gameplay, ouvrir la scène `Assets/Scenes/Test_Prototype.unity` (bac à sable) ou `Strata_1`/`Strata_2` (niveaux) dans l'Éditeur et jouer.

⚠️ **Dépôt git** : ce dossier n'a pas son propre repo — il est contenu dans un dépôt git dont la racine est `C:\Users\Canette` (le profil utilisateur entier), qui suit aussi `ProjectTDB` et des dossiers systèmes. Vérifier `git status`/`git add` avec attention avant tout commit : les chemins remontent en `../ForgeFunebre/...` ou `../ProjectTDB/...` selon le cas, et un `git add -A` depuis la racine attraperait des fichiers hors de ce projet.

## Architecture

### Boucle de possession (cœur du gameplay)

`TransferSystem` (fichier `Core/TransfertSystem.cs`, classe `TransferSystem` — l'orthographe diverge volontairement/par erreur historique entre fichier et classe) est le singleton (`TransferSystem.Instance`) qui possède tout le game loop de la possession :

- **Transfert Contact** : touche dédiée, cherche le `BodyBase` mort le plus proche dans `contactRange` sur le layer `Body`.
- **Transfert Lancé** : viser (souris) + relâcher lance Morthis comme projectile ; s'il touche un ennemi et le tue (`EnemyBase.TakeDamageFromLaunch`), possession immédiate du cadavre ; sinon le lancer échoue et on reste dans le corps actuel.
- Règle centrale : on choisit sa cible **avant** de quitter l'ancien corps ; l'ancien corps meurt (`BodyBase.DieFromTransfer`) au moment exact où le nouveau est possédé (`OnPossess`) — jamais d'état "sans corps" transitoire, sauf `ForceEject` (corps qui meurt pendant qu'on l'occupe → cherche un corps de secours en contact immédiat, sinon Game Over).
- Gère aussi mouvement du corps actif, attaque au contact et mise à jour de la cible caméra Cinemachine.

### Corps possédables (`Assets/Game/Scripts/Bodies/`)

`BodyBase` (abstrait) orchestre trois composants dédiés posés sur le même GameObject (`RequireComponent`) :
- `BodyPhysics` — mouvement/saut/gravité, ne connaît pas qui le contrôle (API publique `SetMoveInput`/`RequestJump`/etc. appelée par `TransferSystem`).
- `BodyHealth` — PV, événements `OnHealthChanged`/`OnDeath`.
- `DecaySystem` — jauge de pourriture, ne tourne que pendant la possession (`StartDecay`/`StopDecay`), couleur d'urgence (`GetCurrentColor`) pour l'UI.

Les types concrets (`Bodies/Types/Porter.cs`, `Pillard.cs`) héritent de `BodyBase` et n'implémentent que `UseSpecialAbility()` + couleurs + comportement spécifique (ex. double saut du Pillard). Les stats (vitesse, PV, durée de pourriture) sont pour l'instant en dur dans le code/l'inspecteur — prévu pour migrer vers des `ScriptableObject` (`Assets/Game/ScriptableObjects/Bodies` existe mais est vide).

### Ennemis (`Assets/Game/Scripts/Enemies/`)

`EnemyBase` (abstrait) a deux phases sur le **même GameObject** :
1. **Vivant** — state machine simple (`Patrol` / `Alert` / `Attack` / `Dead`), `BodyBase` et `BodyPhysics` désactivés.
2. **Mort** — à `Die()`, le layer passe à `Body`, `BodyBase`/`BodyPhysics` sont réactivés → l'ennemi mort devient un corps possédable normal.

`GruntEnemy` illustre le pattern : patrouille manuelle avec détection de bord (raycast), puis poursuite via A* Pathfinding (`Seeker`/`Path`) une fois le joueur détecté, avec recalcul de chemin périodique (`pathUpdateRate`).

### Communication inter-systèmes

`Core/EventBus.cs` : bus d'événements statique typé par struct (convention `OnXxx`, immuables). `Subscribe<T>`/`Publish<T>`/`Unsubscribe<T>` où `T : struct`. Toujours se désabonner dans `OnDisable`/`OnDestroy`. Les nouveaux événements se déclarent en bas de ce même fichier, pas ailleurs. `EventBus.Clear()` est prévu pour un changement de scène.

`GameEnums.cs` centralise tous les enums partagés (`BodyType`, `DeathCause`, `EnvironmentType`, `SoulType`, `TransferMode`) pour éviter les dépendances circulaires — ajouter tout nouvel enum ici plutôt que dans un fichier dédié.

### UI

`HUDManager` s'abonne à `OnTransferComplete`/`OnBodyDied` sur l'`EventBus` et re-connecte dynamiquement `DecayBar`/`HealthDisplay` aux événements du corps nouvellement possédé à chaque transfert (et se désabonne de l'ancien).

### Layers Unity (importants pour les `OverlapCircle`/raycasts)

`Player`, `Enemy`, `Body`, `Ground`, `Hazard`, `Interactable` — le passage `Enemy`→`Body` au moment de la mort est ce qui rend un ennemi possédable par `TransferSystem`.

### Non implémenté / prévu (dossiers présents mais vides)

`Scripts/Souls/`, `Scripts/World/`, `ScriptableObjects/Bodies|Runes|Souls/` — architecture anticipée (âmes `Morthis`/`Verak`, runes, données de corps en ScriptableObject) mais aucun code/asset encore présent. Ne pas supposer qu'ils contiennent quoi que ce soit.
