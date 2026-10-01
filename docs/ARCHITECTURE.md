# DiceFree Architecture Contract

This document is the architectural constitution for DiceFree.

It is intentionally stricter than a loose "technical direction" document. New implementation work should preserve these boundaries unless an explicit Architecture Decision Record (ADR) changes them.

The goal is not abstraction for abstraction's sake. The goal is to keep DiceFree able to grow into a large class-heavy RPG without repeating the early DiceBound pattern where unrelated responsibilities accreted into monolithic files and implicit global state.

## Design goals

DiceFree architecture optimizes for:

- a very large, uneven class/Ways graph;
- data-driven authored content;
- fast iteration on combat/content/balance;
- deterministic and testable rules;
- durable save evolution over many versions;
- eventual 1-4 player host-authoritative co-op;
- reusable content systems without giant managers;
- strong tooling/debuggability;
- safe refactoring as the game grows;
- art/content pipelines that can be automated later.

The architecture should remain practical for a small team. Do not build enterprise abstractions that have no current gameplay consumer.

## Core dependency rule

Dependencies flow from presentation/infrastructure toward stable gameplay/domain contracts, never in arbitrary circles.

A gameplay/domain module must not depend on:

- UI implementation;
- Editor tooling;
- a concrete save-file implementation;
- Steam/platform APIs;
- build/distribution tooling.

Infrastructure and presentation may depend on domain modules.

Cross-domain communication should prefer:

- typed commands;
- typed semantic facts/events;
- narrow interfaces;
- stable data contracts;

rather than reaching through scene hierarchies or querying unrelated concrete components.

Circular assembly dependencies are forbidden.

## Target runtime modules

These are target physical boundaries for Assembly Definition files, not a demand to rewrite the whole PoC in one patch.

### DiceFree.Foundation.Runtime

Lowest-level shared contracts and primitives.

Examples:

- stable/content ID helpers;
- deterministic random/time interfaces;
- semantic fact/event contracts that truly cross domains;
- small results/value objects;
- durable mutation/transaction interfaces;
- shared ownership/identity contracts.

It must not depend on higher gameplay domains.

Keep it small. "Foundation" must not become a junk drawer.

### DiceFree.Gameplay.Runtime

Core moment-to-moment actor rules.

Likely owns:

- Characters;
- Combat;
- Progression;
- future Abilities/Effects/Resources core.

Depends on Foundation.

It must not depend on UI, Editor or concrete Persistence.

### DiceFree.Items.Runtime

Owns:

- item definitions;
- item instances;
- inventory;
- equipment;
- item eligibility;
- loot/drop primitives;
- future affix/socket/runeword/set machinery.

Depends on Foundation and the minimal Gameplay stat/effect contracts it needs.

It must not depend on UI or concrete Persistence.

### DiceFree.World.Runtime

Owns world-facing runtime concepts such as:

- interactions;
- areas;
- resurrection points;
- travel surfaces;
- world/session facts;
- world object lifecycle contracts.

Depends on Foundation and Gameplay.

Reusable facts consumed by other domains should live in an appropriate contract layer rather than force reverse dependencies.

### DiceFree.Quests.Runtime

Owns:

- quest definitions;
- objective/composite logic;
- quest progress;
- credit policies;
- quest reward orchestration contracts.

It consumes semantic gameplay/world facts.

It must not own combat, inventory, UI or file I/O.

### DiceFree.AI.Runtime

Owns:

- enemy target acquisition;
- aggro;
- threat selectors;
- leash/reset decision logic;
- reusable AI policies.

Depends on Foundation, Gameplay and minimal World contracts.

AI must not own encounter persistence or UI.

### DiceFree.Persistence.Runtime

Infrastructure for durable state.

It may depend on domain snapshot/data contracts from Gameplay, Items, Quests and World.

No gameplay domain may depend on the concrete Persistence implementation.

Persistence owns:

- serialization;
- schema/versioning;
- migrations;
- checksums;
- atomic replacement;
- backups/recovery;
- provider-neutral save/cloud adapters;
- diagnostics.

Gameplay exposes state/commands; Persistence decides how durable state is stored.

### DiceFree.UI.Runtime

A presentation leaf.

It may read domain state and issue domain/application commands.

UI must not become the authority for:

- quest completion;
- item ownership;
- combat math;
- persistence;
- progression.

Closing a UI must never alter gameplay state unless the UI explicitly submitted a gameplay command.

### DiceFree.Application.Runtime

Composition/orchestration layer.

This is the legitimate place to wire domains and infrastructure together when a feature requires cross-domain coordination.

Examples:

- application bootstrap;
- session composition;
- durable transaction coordination;
- high-level feature use-cases crossing several domains.

Domain modules must not reference Application.

Keep orchestration thin; actual rules remain in their owning domains.

### DiceFree.Editor

Editor-only tooling.

May depend on runtime assemblies.

Owns:

- scene/content setup tools;
- import validation;
- content linting;
- debug windows;
- authoring utilities.

Runtime assemblies must never depend on UnityEditor or editor tools.

### Tests

Use separate EditMode and PlayMode test assemblies.

Test assemblies may reference what they test. Test code must never be included as a production gameplay dependency.

## Assembly Definition policy

The current PoC still compiles into the predefined Unity assembly. That is acceptable only as temporary prototype debt.

Before the project expands substantially beyond the Cornberg slice, introduce .asmdef boundaries deliberately.

Rules:

1. do not create dozens of tiny assemblies merely to mirror folders;
2. establish the target dependency graph above;
3. migrate one boundary at a time and keep the project green;
4. use explicit assembly references;
5. separate Editor and test assemblies;
6. add an automated architecture check that fails on forbidden dependency directions/cycles;
7. once migration starts, move toward all first-party code being under explicit .asmdef ownership rather than leaving a large Assembly-CSharp backdoor.

A feature must not solve an asmdef cycle by merging unrelated domains into one giant assembly. Fix the ownership/dependency problem instead.

## Current architectural debt to resolve during asmdef hardening

Known prototype debt is allowed when documented.

Examples currently worth watching:

- some cross-domain runtime code directly references concrete Persistence to defer writes;
- static semantic event hubs exist for several prototype facts;
- Cornberg editor setup scripts know a great deal about one scene;
- some runtime discovery still uses Unity Find* APIs;
- the PoC scene carries more composition responsibility than the eventual production scene structure should.

These are not emergencies. They are explicit migration targets.

In particular, domain reward/item/quest logic should eventually depend on a small durable-mutation transaction contract rather than concrete ManifestationPersistence.

## MonoBehaviour, plain C# and ScriptableObject roles

### ScriptableObjects

Use ScriptableObjects primarily for authored immutable-ish definitions/configuration:

- classes;
- abilities;
- attacks;
- enemies;
- items;
- quests;
- loot policies;
- travel surfaces;
- tuning.

Do not store mutable player progress or session truth inside shared ScriptableObject assets.

Runtime state belongs to runtime instances/save-state models.

### MonoBehaviours

MonoBehaviours are Unity adapters/lifecycle owners:

- Transform/GameObject integration;
- physics/NavMesh/input hooks;
- presentation bindings;
- runtime component composition.

Keep rules that do not require Unity lifecycle APIs in plain C# where doing so improves testability.

Do not force pure-C# architecture everywhere; use it where it creates a clear benefit.

### Prefabs

Reusable world/gameplay entities should increasingly be prefab-first.

A scene should compose authored prefabs rather than duplicate large bespoke hierarchies.

Use nested prefabs/variants carefully where inheritance is genuinely helpful.

## Character form / rig architecture

Do not make "the player" synonymous with one humanoid mesh or one human skeleton.

Gameplay identity remains the manifestation/CombatActor/class state. Visual/animation body form is presentation authored by the current Way.

The long-term character presentation seam must be able to resolve a class/form profile containing concepts such as:
- body prefab / renderer set;
- one authored sex/body presentation for that class;
- rig/animator family;
- semantic attachment/socket map;
- equipment-appearance compatibility;
- baseline per-slot visual presentation;
- authored gameplay collision/selection/navigation footprint;
- animation set/overrides;
- body-specific presentation offsets;
- future form-specific locomotion when genuinely necessary.

Gameplay equipment slots remain semantic (Head, Feet, Main Hand, etc.). A particular body form decides how that slot is visually represented.

The **slot set remains the same across forms**, including radical/non-biped forms. Do not create a parallel Centaur inventory/equipment engine.

Equipment may occupy more than one semantic slot. The canonical early example is:
- a two-handed weapon occupies both **Main Hand + Off Hand**.

Model this as authored occupied-slot requirements rather than hard-coding "two-handed" checks throughout combat/UI/inventory.

Visible wearable equipment normally replaces the form's baseline presentation for its occupied visible slot(s); empty slots fall back to the class form's baseline appearance.

An equipment appearance does not automatically reshape itself for every body form. Compatibility is explicit data.

Default lineage rule: an appearance authored for a class/form is compatible with that form and its descendants unless a descendant explicitly overrides compatibility because its body plan diverges. Unrelated forms do not silently inherit it.

If a mesh was authored for a Wood-Elf lineage, another radically different form does not silently scale/morph it; that form needs compatible authored equipment/appearance content.

Do not let gameplay systems query literal bone names.

Use semantic attachment concepts which a rig/form adapter maps to concrete transforms.

Rig reuse is encouraged for compatible forms, but architecture must permit:
- compatible Human / Wood Elf / High Elf-like forms sharing a humanoid rig family when practical;
- different-sized humanoids;
- separate Orc-like rig families when proportions/animation require it;
- eventual non-biped forms such as a possible Centaur class with a different rig family.

A descendant Way may switch to another form/rig family entirely. Parent form identity does not constrain all descendants permanently.

A non-human class should not require cloning the combat, inventory, quest or persistence engines.

### Physical footprint, reach, and navigation forgiveness

Class/form collision footprint and attack reach are **separate authored concerns**.

A larger body may have a larger gameplay hitbox/agent footprint, but it does not automatically gain longer basic-attack or melee-skill reach. Attack ranges remain authored by the class/attack/ability data.

Likewise, a large playable form must not become progression-locked by ordinary world geometry simply because its ideal physical footprint cannot pass a doorway or narrow authored route.

Production navigation should support deliberate **player-form forgiveness** where needed, for example through:
- authored traversal clearance wider than decorative geometry;
- local temporary agent/collision accommodation at known bottlenecks;
- body-form-aware doorway/portal traversal;
- other explicit accessibility helpers.

Do not globally shrink all large-form hitboxes just to fit the world, and do not silently let the renderer clip through everything. Keep the normal form footprint meaningful, but ensure required player routes remain traversable for every intended playable form.

### Transformation presentation

Advancement may optionally specify a transformation presentation/effect.

The gameplay/class fork must not require one universal transformation animation. An advancement can use:
- an authored transformation sequence;
- a flash/fade/model swap;
- another bespoke presentation;
- no special sequence at all.

The transformation presentation is optional content layered over the advancement result, not the owner of class-state mutation.

## Scene architecture

Cornberg is currently a vertical-slice PoC and does not need an immediate scene rewrite.

Production direction:

- a small bootstrap/composition scene;
- long-lived application/session systems with explicit lifetime;
- UI separated from world content;
- world/region/chunk scenes loaded additively where appropriate;
- gameplay entities primarily prefab-authored.

Avoid uncontrolled DontDestroyOnLoad singletons.

Prefer explicit lifetime ownership and additive loading when the production world begins to span multiple areas.

Large monolithic scenes should be avoided because they create merge conflicts and unclear ownership.

## Service discovery and globals

Do not use global service locators as the default architecture.

Prefer:

- serialized references for local composition;
- constructor/configuration injection for plain C#;
- explicit application/bootstrap wiring for services;
- registries with explicit lifetime where dynamic discovery is genuinely required.

Unity FindObject*/FindObjects* calls are acceptable in Editor tooling, diagnostics and tiny PoC paths, but should not become repeated production hot-path service discovery.

Static state/events must have explicit reset/lifetime semantics so Enter Play Mode, tests and session changes cannot leak state.

## Semantic events and commands

Important cross-system communication uses typed semantic facts.

Examples:

- ActorDefeated;
- AreaEntered;
- ConversationCompleted;
- ItemAcquired;
- QuestCompleted.

Facts describe what happened.

Consumers independently decide:

- XP credit;
- quest credit;
- achievement credit;
- Codex credit;
- loot behavior;
- persistence recipients.

Do not make one consumer's eligibility rule silently define another consumer's rule.

Commands express intent and may fail validation.

Presentation should issue commands rather than mutate domain state directly.

## Determinism seams

Any gameplay-relevant randomness or time that needs reliable automated validation must have an injectable seam.

Examples:

- loot rolls;
- ambient bark selection;
- proc rolls;
- encounter timers where simulated time matters.

Do not write flaky tests against UnityEngine.Random or wall-clock time when deterministic injection is practical.

The production implementation may still use Unity/system random/time behind the interface.

## Data-driven content rules

Content identity uses stable semantic IDs.

Never use:

- display names;
- scene object names;
- localized text;
- array index position;

as durable identity.

Authoring data should be inspectable in assets rather than hidden in switch statements.

Avoid class-specific/enemy-specific hard-coded branches when the behavior is really reusable data.

Custom code remains valid for genuinely unusual mechanics.

## Persistence boundary

Persistence is a storage concern, not the owner of gameplay rules.

Requirements:

- explicit ownership domain: Echo / manifestation / session;
- explicit schema and record versions;
- explicit migrations;
- stable IDs;
- unknown data preservation where practical;
- atomic operations for ownership-changing/fork operations;
- recoverable writes/backups;
- no reward replay during load.

Cross-domain durable operations should use a transaction/use-case boundary.

A domain component should never call file-system APIs.

## Networking boundary

Gameplay code must not depend directly on Steam APIs.

Future networking is host-authoritative:

client intent -> host simulation -> authoritative semantic outcome -> eligible durable consumer(s).

Design new gameplay rules so the host can own them later.

Do not prematurely implement networking abstractions around every local method, but avoid rules that depend on "the local player is always the only actor."

## UI boundary

UI is replaceable presentation.

Rules:

- UI reads state;
- UI sends commands/intents;
- domain logic determines validity;
- UI strings never become stable IDs;
- game remains testable without rendering UI.

Prototype OnGUI panels are acceptable for Cornberg proving work. They are not the final UI architecture.

## Content/build boundary

Source repository content and player build content are different things.

The repository contains:

- source code;
- Unity project assets;
- source art;
- authoring tools;
- tests;
- documentation.

The playable build contains only build output/runtime content selected by Unity's build/content pipeline.

Do not create a "player files only" Git branch.

Build outputs are artifacts/releases, not source branches.

## Resources / Addressables

Do not put general content into Resources merely for convenience. Resources content is forced into the player build.

Use normal serialized/direct references for the current fixed Cornberg-sized content.

Introduce Addressables/AssetBundles only when real streaming/download/content-scale requirements justify them.

## Debuggability is a feature requirement

A feature is not fully architected if its state is impossible to inspect.

Prefer:

- Inspector-readable definitions;
- developer diagnostics;
- validation tools;
- Profile Inspector integration for durable data;
- focused test harnesses;
- encounter/ability debug tooling as those systems mature.

Avoid invisible implicit state.

## Testing policy

Use the cheapest reliable level of test:

- plain/EditMode tests for deterministic rules/data validation;
- PlayMode tests for GameObject/physics/NavMesh/lifecycle integration;
- standalone smoke tests for build/runtime/save boundaries;
- fixture saves for migration/recovery.

Every durable schema change requires migration coverage.

Every reusable policy should have boundary/negative tests, not just the happy path.

Do not weaken an old gameplay assertion merely to make a new feature pass.

## Architecture review triggers

These are review triggers, not arbitrary hard limits:

- a class starts owning multiple unrelated domains;
- a file grows beyond roughly 500 lines;
- a method grows beyond roughly 60 lines;
- a component needs a large number of unrelated serialized dependencies;
- a new system needs references in both directions between two modules;
- a feature requires checking specific quest/item/enemy IDs in an otherwise generic system;
- a UI class starts calculating authoritative gameplay results;
- a domain class starts opening files/saving directly;
- a scene setup tool becomes the only canonical representation of runtime content;
- a static singleton accumulates mutable session state.

When a trigger appears, stop and reconsider ownership before adding more code.

Do not game line counts. Responsibility and dependency clarity matter more than numbers.

## Architecture Decision Records

Material architectural changes require a short ADR under `docs/adr/`.

Use ADRs for decisions that are expensive to reverse, such as:

- repository/art-source layout;
- assembly/module boundaries;
- save ownership/schema principles;
- network authority;
- world/scene streaming model;
- asset delivery strategy.

An ADR records:

- context;
- decision;
- consequences;
- status.

Do not use ADRs for every balance value or ordinary feature decision.

## Change rule

When implementation pressure conflicts with this contract:

1. first try to fit the feature cleanly into the existing ownership model;
2. if the architecture is wrong, write/update an ADR and change it deliberately;
3. never smuggle a cross-layer shortcut in as "temporary" without documenting the debt;
4. add a regression/architecture test where the rule can be mechanically checked.

The architecture exists to help shipping the game, not to win a purity contest.
