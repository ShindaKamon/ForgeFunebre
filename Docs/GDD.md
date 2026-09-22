# FORGE FUNÈBRE

Game Design Document — Version 0.5 — Prototype

*Metroidvania 2D · Action-Plateforme · Die & Retry*

# 1. Vision du Jeu

## 1.1 Concept Central

Tu es Morthis, une volonté ancienne piégée dans une lame maudite. Incapable d'agir directement sur le monde, tu possèdes des cadavres pour gravir La Châsse — une tour-tombeau inversée de 7 strates — et découvrir la vérité sur ta propre mort.

**Règle fondamentale :**

- Un corps ne devient possédable QUE lorsqu'il est mort
- L'ancien corps meurt immédiatement au moment du transfert
- Pas d'état intermédiaire — possession directe du nouveau corps

## 1.2 Piliers de Design

| Pilier | Description | Implication |
| --- | --- | --- |
| Possession stratégique | Chaque corps a des capacités uniques | Choisir le bon corps au bon moment |
| Ressource limitée | La pourriture consomme le corps | Créer de la tension permanente |
| Die & Retry | La mort est un outil d'apprentissage | Runs courts et informatifs |
| Narration environnementale | L'histoire se lit dans les corps | Chaque corps a une histoire |

## 1.3 Références

- Hollow Knight — exploration verticale et ambiance
- Dead Cells — die & retry et progression méta
- Carrion — possession et fluidité du mouvement
- Control — narration environnementale et pouvoirs

# 2. Narration & Univers

## 2.1 Contexte

La Châsse est une tour funéraire inversée — elle s'enfonce dans la terre plutôt que de s'élever vers le ciel. Construite par le Forgeron pour contenir les âmes les plus dangereuses, elle est devenue une prison pour Morthis après sa trahison.

Morthis n'est pas un héros. C'est une volonté qui survit par nécessité, qui utilise les morts comme outils et qui cherche à comprendre pourquoi elle a été condamnée à cette existence.

## 2.2 Personnages Principaux

| Personnage | Rôle | Arc narratif |
| --- | --- | --- |
| Morthis | Protagoniste — volonté dans une lame | Découvrir la vérité sur sa trahison |
| Verak | Co-protagoniste (coop) — volonté dans une chaîne | Comprendre son lien avec Morthis |
| Le Forgeron | Antagoniste principal | Créateur de La Châsse, gardien des secrets |
| Les Gardiens | Antagonistes secondaires | Âmes condamnées devenues gardiennes |

## 2.3 Les 7 Strates

| Strate | Nom | Ambiance | Ennemi type |
| --- | --- | --- | --- |
| 1 | Fondations Grasses | Souterrain organique, racines | Grunt / Porteur |
| 2 | Cryptes Marchandes | Commerce interdit, reliques | Pillard / Guard |
| 3 | Arènes Englouties | Gladiateurs morts, sang séché | Gladiator |
| 4 | Sanctuaire Renversé | Religion corrompue, idoles | Devout |
| 5 | Laboratoires de Peau | Expériences, corps modifiés | Alchemist |
| 6 | Jardins de Cendres | Nature morte, spores toxiques | Specter |
| 7 | Chambre du Forgeron | Métal vivant, chaleur intense | Giant |

# 3. Mécaniques de Jeu

## 3.1 Système de Transfert

Le transfert est la mécanique centrale du jeu. Morthis peut quitter un corps et en posséder un autre selon deux modes :

### Transfert Contact (touche C)

- Portée courte — 2 unités
- Instantané — pas de projectile
- Cible : corps mort à proximité
- Idéal pour les situations d'urgence

### Transfert Lancé (clic gauche)

- Portée longue — configurable (défaut : 8 unités)
- Lance Morthis comme projectile
- Peut tuer un ennemi vivant et posséder son corps
- Cooldown entre les lancers
- Ligne de visée avec indication de cible (blanc/rouge/cyan)

## 3.2 Types de Corps

| Corps | Capacité spéciale | Stat principale | Faiblesse |
| --- | --- | --- | --- |
| Porteur | Force — brise les obstacles | Résistance élevée | Lent |
| Pillard | Double saut + Dash | Mobilité maximale | Fragile |
| Guard | Bouclier — bloque les projectiles | Défense | Attaque faible |
| Gladiator | Attaque tournoyante AoE | Dégâts | Pas de défense |
| Devout | Soin passif lent | Pourriture lente | Faible aux coups |
| Alchemist | Lance des potions | Attaque à distance | Corps fragile |
| Specter | Traverser les murs | Furtivité | Pas d'attaque |
| Giant | Séisme — détruit le sol | PV massifs | Très lent |

## 3.3 Système de Pourriture (Decay)

Chaque corps possédé se dégrade avec le temps. La barre de pourriture descend en permanence et change de couleur selon l'état :

- Vert (0-50%) — corps en bonne condition
- Orange (50-75%) — corps en danger
- Rouge (75-100%) — mort imminente

Mécaniques liées à la pourriture :

- Tuer un ennemi ajoute du temps de pourriture
- Certains environnements accélèrent la dégradation
- Le système d'Imprégnation récompense le temps passé dans un corps

## 3.4 Système d'Imprégnation

Plus Morthis reste longtemps dans un corps, plus des bonus passifs s'accumulent. Ces bonus persistent même après le transfert sous forme de traces mnémoniques.

| Durée | Bonus |
| --- | --- |
| 30 secondes | Mémoire musculaire — +10% vitesse |
| 60 secondes | Réflexes — +1 point d'armure |
| 120 secondes | Fusion partielle — capacité spéciale améliorée |
| 300 secondes | Imprégnation totale — nouveau corps disponible en méta |

# 4. Progression & Méta-jeu

## 4.1 Structure d'une Run

- Le joueur commence toujours dans la Strate 1
- Chaque strate contient 3-5 salles + une salle de boss
- La mort renvoie au début de la run (pas de checkpoint)
- Les Runes collectées persistent entre les runs

## 4.2 Système de Runes (Méta-progression)

Les Runes sont des fragments d'âme collectés pendant les runs. Elles débloquent des améliorations permanentes qui persistent entre les parties.

| Catégorie | Exemples d'améliorations |
| --- | --- |
| Morthis | Portée de transfert +1, cooldown réduit, ligne de visée améliorée |
| Corps | Pourriture plus lente, PV de départ augmentés, capacités débloquées |
| Monde | Corps pré-placés supplémentaires, ennemis plus lents au départ |
| Narration | Fragments d'histoire, voix de Morthis, lore des strates |

## 4.3 Progression Narrative

- Chaque run révèle un fragment de l'histoire de Morthis
- Les corps possédés peuvent contenir des mémoires (flashbacks courts)
- Le Forgeron commente la progression de Morthis
- La vérité se révèle progressivement sur 10-15 runs

# 5. Mode Coopératif Local

## 5.1 Verak — La Chaîne

En mode coop local, un second joueur contrôle Verak — une volonté piégée dans une chaîne. Verak a ses propres mécaniques de possession, distinctes de celles de Morthis.

| Aspect | Morthis (Lame) | Verak (Chaîne) |
| --- | --- | --- |
| Mode de transfert | Lancé (projectile) | Lasso (capture à distance) |
| Portée | 8 unités | 6 unités mais multidirectionnel |
| Capacité unique | Tue pour posséder | Immobilise avant de posséder |
| Synergie | Attaque frontale | Flanquement et contrôle |

## 5.2 Système de Résonance

Quand Morthis et Verak possèdent des corps compatibles, des effets de Résonance s'activent automatiquement :

| Combinaison | Effet de Résonance |
| --- | --- |
| Porteur + Giant | Tremblement de terre — tous les ennemis à l'écran sont étourdis |
| Pillard + Specter | Invisibilité partagée pendant 5 secondes |
| Guard + Devout | Bouclier sacré — absorbe les dégâts pour les deux |
| Gladiator + Alchemist | Explosion alchimique — AoE massive |

# 6. Aspects Techniques

## 6.1 Stack Technique

| Composant | Technologie |
| --- | --- |
| Moteur | Unity 6 (6000.x) — Universal 2D Template |
| Rendu | URP (Universal Render Pipeline) |
| Input | Input System (nouveau) |
| Caméra | Cinemachine 3 |
| Pathfinding | A* Pathfinding Project (Aron Granberg) |
| Niveaux | 2D Tilemap + Tile Palette |
| UI | TextMeshPro |
| Assets | Cainos (Warrior, Bringer of Death, TX Tileset Ground) |

## 6.2 Architecture des Scripts

| Dossier | Contenu |
| --- | --- |
| Core/ | GameManager, TransferSystem, EventBus, GameEnums, GameStarter |
| Bodies/ | BodyBase, BodyPhysics, DecaySystem, BodyHealth, BodyAnimator |
| Bodies/Types/ | Porter, Pillard, Guard, Gladiator... |
| Enemies/ | EnemyBase, EnemyAnimator |
| Enemies/Types/ | GruntEnemy + types futurs |
| Systems/ | CameraDirectionOffset, ObjectPooler |
| UI/ | HUDManager, DecayBar, HealthDisplay |
| World/ | LevelExit, Breakable (prévu) |

## 6.3 Patterns Architecturaux

- EventBus statique — communication découplée entre systèmes
- Singleton — TransferSystem, GameManager
- ScriptableObjects — données des corps et ennemis (prévu)
- Object Pooling — gestion des projectiles et effets visuels
- State Machine — IA des ennemis (Patrol / Alert / Attack / Dead)

# 7. État Actuel du Prototype

## 7.1 Fonctionnalités Implémentées

| Fonctionnalité | État |
| --- | --- |
| Transfert Contact | ✅ Fonctionnel |
| Transfert Lancé avec visée | ✅ Fonctionnel |
| Corps Porteur et Pillard | ✅ Fonctionnel |
| Système de Pourriture | ✅ Fonctionnel |
| Système de PV | ✅ Fonctionnel |
| HUD (Decay bar + PV) | ✅ Fonctionnel |
| IA ennemis basique (GruntEnemy) | ✅ Fonctionnel |
| Pathfinding A* | ✅ Intégré |
| Animations joueur (Idle/Run/Jump/Fall/Attack) | ✅ Fonctionnel |
| Animations ennemis | 🔧 En cours |
| Strate 1 — niveau tutoriel | 🔧 En cours |
| Méta-progression (Runes) | ⬜ Prévu |
| Mode Coop | ⬜ Prévu |
| Strates 2-7 | ⬜ Prévu |
| Boss | ⬜ Prévu |
| Sauvegarde | ⬜ Prévu |

## 7.2 Prochaines Étapes

- Étape 6 — Finaliser le niveau tutoriel Strate 1
- Étape 7 — WeaponSoulBase + MorthisSoul (système d'armes)
- Étape 8 — Ennemis supplémentaires + Breakable.cs
- Étape 9 — Méta-progression (Runes + SaveSystem JSON)
- Étape 10 — Coop local (Verak + ResonanceSystem)

*FORGE FUNÈBRE — GDD v0.5*
