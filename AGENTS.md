# Repository Guidelines

## Project Structure & Module Organization
- App code lives in `KAP_InventoryManager/` using MVVM.
- Models: `KAP_InventoryManager/Model/` and `Model/Interfaces/`
- ViewModels: `KAP_InventoryManager/ViewModel/` (includes `ModalViewModels/` and `InventoryPanelViewModels/`).
- Views (XAML): `KAP_InventoryManager/View/` (includes `Modals/` and `InventoryPanelViews/`).
- Data access: `KAP_InventoryManager/Repositories/`
- UI assets and styles: `KAP_InventoryManager/Images/`, `Fonts/`, `Styles/`, and `CustomControls/`.
- Config: `KAP_InventoryManager/appsettings.json`

## Build, Test, and Development Commands
- Restore NuGet packages: `nuget restore KAP_InventoryManager.sln`
- Build (CLI): `msbuild KAP_InventoryManager.sln /t:Build /p:Configuration=Debug`
- Run (after build): `KAP_InventoryManager\bin\Debug\KAP_InventoryManager.exe`
- Preferred IDE: Visual Studio 2019/2022 with “.NET desktop development”. Use Build → Rebuild Solution and F5 to run.

## Coding Style & Naming Conventions
- C# with 4‑space indentation; UTF‑8 files.
- PascalCase for public types/members; camelCase for locals/parameters.
- Keep MVVM boundaries: no code‑behind business logic; bind via `ViewModelCommand`.
- File naming: `*View.xaml`, `*ViewModel.cs`, `*Repository.cs`, `*Model.cs`.
- XAML: define shared styles in `Styles/*.xaml`; prefer bindings and converters (see `Converters/TextTruncateConverter.cs`).

## Testing Guidelines
- No test project currently. If adding tests, create `KAP_InventoryManager.Tests` with MSTest or NUnit.
- Name tests `<ClassName>Tests.cs`; method names `MethodName_State_ExpectedBehavior`.
- Run via Visual Studio Test Explorer or `vstest.console.exe` (for MSTest). Keep tests deterministic and UI‑independent.

## Commit & Pull Request Guidelines
- Commits: concise imperative subject (e.g., "Add invoice total validation").
- Scope changes logically; separate refactors from behavior changes.
- PRs: include summary, rationale, and screenshots for UI changes; link related issues; note any config/migration impacts.

## Security & Configuration Tips
- Do not commit secrets. If `appsettings.json` needs sensitive values, document placeholders and use user‑local overrides.
- Keep data access within `Repositories/`; validate and sanitize inputs at ViewModel boundaries.

