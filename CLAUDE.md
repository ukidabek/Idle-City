# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Collaboration Rules

- **Preview before editing** — when the user says "show me changes", "show me how it would look", or similar, output the proposed diff or modified code sections as text first. Only apply edits after the user explicitly approves (e.g. "looks good", "do it", "apply it").

## Coding Conventions

- **`var` everywhere** — use `var` for all local variable declarations where the type can be inferred.
- **`m_` prefix** — private instance fields use `m_camelCase` (e.g. `m_tileDatabase`, `m_currentHealth`).
- **`const` PascalCase** — `const` fields use PascalCase (e.g. `MaxTracks`), not the `k_` prefix.
- **Inline when possible** — prefer expression bodies (`=>`) for single-expression methods/properties; collapse trivial `if` bodies to one line.
- **Early return** — guard clauses at the top of methods instead of deeply nested `if` blocks.
- **Fun variable names** — use creative, characterful names for locals where context allows (e.g. `var mightyStack` instead of `var stack`). Keep public API and serialized field names descriptive and conventional.

## Project Overview

**Idle Citty** is a 2D idle city-builder game built with Unity 6000.3.9f1 (URP). The game centers around placing tiles on a grid, managing resource production/consumption chains, and unlocking structures.

## Unity Commands

There is no CLI build or test runner — all compilation, running, and testing is done through the Unity Editor. Open the project with Unity 6000.3.9f1 in `Assets/Scenes/SampleScene.unity`.

To run unit tests: **Window → General → Test Runner** inside the Unity Editor (uses `com.unity.test-framework`).

## Code Architecture

All game code lives in `Assets/_Project/Code/`. Seven subsystems:

### Map System (`Code/Map/`)

The core of the game. A `MapManager` (singleton MonoBehaviour) owns a `Dictionary<Vector3Int, TileStack>` — each grid cell holds a stack of `Tile` MonoBehaviours. `TileBuilder` populates the map at startup using configurable ground/deposit rules.

- **`Tile`** — MonoBehaviour with a `TileID` key and a list of `TileComponent` children. Components are added/removed to compose tile behavior.
- **`TileStack`** — push/pop mechanics with `OnTilePushed` / `OnTilePopped` / `StackChanged` events.
- **`TileComponent`** (abstract) — base for all behaviors. Concrete types: `Ground`, `Deposit`, `Structure`, `Producer`, `Consumer`, `NeighborValidator`, `Destroyable`.
- **`TileDatabase`** — ScriptableObject registry (`IReadOnlyDictionary<TileID, Tile>`).
- **`NeighborValidator`** — checks surrounding tiles and applies `AmountModifier`s based on `NeighborCondition` rules.

### Resource System (`Code/Resources/`)

Tick-based economy. `ResourceManager` runs at a configurable Hz, calling `Tick()` on every `Resource` ScriptableObject in a `ResourceCollection`.

- **`Resource`** — extends `BaseValue<float>` (from `com.cooki.utilities`). Clamps 0–max, tracks base gain plus modifier-driven gain, holds registered `IResourceClient` producers/consumers.
- **`Producer`** / **`Consumer`** — `TileComponent`s that implement `IResourceClient`. A `Producer` only generates if all of its `Consumer` siblings on the same tile are satisfied.
- **Modifier chain** — `AmountModifier` (abstract) → `PercentAmountModifier` / `ValueAmountModifier`. Applied via `ModiferHandler` on resources and clients.

### Camera System (`Code/Camera/`)

`CameraController` wraps Cinemachine 3.1.6 + New Input System. Drag-to-pan, scroll-to-zoom (clamped FOV), and plane raycasting for tile selection. Blocks interaction when pointer is over UI.

### UI System (`Code/UI/`)

- `TileView` — main panel shown when a tile is selected; hosts `TileComponentView` children.
- `BuildTileView` / `TileBuildDisplay` — build menu for placing structures.
- `ResourceWindow` / `ResourceView` — live resource display.
- `CostView` — renders a `Cost` struct (resource + amount) for affordability feedback.

### Logging (`Code/Logging/`)

`LoggerExtensions` provides colored, conditional logging (Editor/Development only). Components implement `ILogEnabled` and set a `LogMode` (Off / Essential / All).

### Generator (`Code/Generator/`)

`GeneratorEngine` (ScriptableObject) — noise-based procedural generation pipeline. Implements `INoiseGeneratorStep` for composable steps. Has editor tooling for preview.

## Key Architectural Patterns

- **Data-driven via ScriptableObjects** — tile definitions, costs, resource configs, and neighbor rules are all assets, not hardcoded.
- **Component composition** — behaviors attach to `Tile` as `TileComponent` children; prefer adding a new component over subclassing.
- **Dictionary collections expose `IReadOnlyDictionary`** — `MapManager` and `TileDatabase` both use this pattern; external code reads through the interface.
- **ScriptableObject events** — `TileEvent`, `TileStackEvent` (in `Code/Map/Events/`) decouple systems; prefer these over direct method calls across subsystems.
- **`com.cooki.*` utilities** — shared utilities (`BaseValue<T>`, state machine, cheats) come from local custom packages; check `Packages/` before reimplementing helpers.

## Custom Packages

| Package | Purpose |
|---|---|
| `com.cooki.utilities` | `BaseValue<T>`, general helpers |
| `com.cooki.states` | State machine |
| `com.cooki.cheats` | Dev cheat system |
