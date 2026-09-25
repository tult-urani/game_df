# Repository Guidelines

## Project Structure & Module Organization

- `LaMuralla/` is the Unity 6 iOS game project. Runtime Unity code lives in `Assets/_Project/Unity/`; shared gameplay/domain code is in `Assets/_Project/Core/`.
- `core/LaMuralla.Core/` mirrors the engine-independent C# domain library; `core/LaMuralla.Core.Tests/` contains its xUnit tests.
- `config/` is the source of truth for tower, enemy, economy, wave, and map data. Do not duplicate its values in C# or documentation.
- `tools/` contains Python validation, balancing, and documentation-generation scripts. `docs/` holds generated design tables and design references; map-specific notes are in `docs/maps/`.
- Art, audio, UI resources, animation controllers, and scenes belong under `LaMuralla/Assets/_Project/Resources/`, `Art/`, and `Scenes/`. Keep Unity `.meta` files alongside their assets.

## Build, Test, and Development Commands

Run commands from the repository root unless noted otherwise:

```bash
dotnet test core/LaMuralla.Core.Tests/LaMuralla.Core.Tests.csproj
python3 tools/gen_docs.py --check
python3 tools/path_check.py --check
python3 tools/balance_sim.py
```

The first runs domain tests; the next two validate generated docs and map geometry. Run `python3 tools/gen_docs.py` after editing `config/*.json`; do not hand-edit generated table sections. Use Unity `6000.5.4f1` to open `LaMuralla/`. Export iOS with `LaMuralla.EditorTools.BuildScript.BuildIOS` (output: `ios-build/`).

## Coding Style & Naming Conventions

Use four spaces in C#; retain nullable annotations and `PascalCase` for types, methods, and public members. Use `camelCase` for locals and private fields. Keep gameplay logic platform-independent in Core and presentation/MonoBehaviour concerns in Unity. Use lowercase, hyphenated JSON/map filenames such as `m03-dos-rios.json`; preserve existing JSON formatting.

## Testing Guidelines

Use xUnit `[Fact]` tests named `Behavior_Condition_ExpectedResult` (for example, `GoalHealth_WhenHit_Decrements`). Add or update tests with every Core rule or config-schema change. Run the full test project and relevant Python checks before submitting. Maps need engine/controller validation; do not treat `balance_sim.py` as the final authority for multi-lane balancing.

## Commit & Pull Request Guidelines

Git history is minimal, so use concise imperative commit subjects, e.g. `Add boss wave validation`. Keep each commit focused. Pull requests should state the gameplay/config impact, list validation commands run, link the issue when applicable, and include screenshots or a short recording for scene, UI, animation, or asset changes.

## Configuration & Assets

When data changes, update `config/` first, regenerate affected documentation, and commit the generated output only when it changes. Never delete or regenerate Unity `.meta` files casually: their GUIDs maintain asset references.
