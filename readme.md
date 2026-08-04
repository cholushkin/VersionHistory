# GameLib Version History Module

![screenshot](Documentation/Screenshot_1.jpg)

An automated pipeline for Unity that extracts Git commit history, parses Conventional Commits, and safely bridges repository data into both **Runtime Assets** (for in-game debug overlays) and **Markdown** (for standard `CHANGELOG.md` files).

Designed with strict Editor/Runtime separation, non-blocking asynchronous execution, and perfect alignment with Semantic Versioning conventions.

---

## 🌟 Key Features

* **Strict Separation of Concerns**: Editor settings and Git processing never leak into the game build. Runtime assets (`VersionManifest`) contain purely uncoupled data.
* **Non-Blocking Git Execution**: Uses native C# asynchronous background processes and `UniTask` to fetch massive Git histories without freezing the Unity Editor.
* **Conventional Commits Native**: Regex-based parser accurately detects commit scopes `feat(ui):`, breaking changes `refactor!:`, and maps them to human-readable categories.
* **Multi-Module Support**: Process the main game project and isolated submodules concurrently from a single centralized configuration.

---

## 🏗 Architecture & Components

The module is divided into two distinct assembly layers (`.asmdef`):

### 1. Runtime Layer (`GameLib.VersionHistory.Runtime`)
* **`VersionManifest`**: A simple, data-only `ScriptableObject` containing `VersionEntry` and `ChangeItem` lists. Loadable at runtime via DI (e.g., `IConfigService`) to populate in-game development tools and debug widgets.

### 2. Editor Layer (`GameLib.VersionHistory.Editor`)
* **`VersionBuilderSettings`**: The configuration asset defining target manifests, markdown export paths, and Git repository locations.
* **`GitRepositoryService`**: Interfaces with the local Git CLI asynchronously to fetch tags (`--sort=-v:refname`) and raw commit logs.
* **`ConventionalCommitParser`**: Transforms raw Git strings into strongly-typed `ParsedCommit` structs.
* **`ChangelogMarkdownGenerator`**: Translates the synced `VersionManifest` into a formatted `CHANGELOG.md`.

> **Note:** Data is injected from Editor to Runtime strictly via `internal` setters and `[InternalsVisibleTo]`, bypassing reflection hacks while maintaining perfect encapsulation.

---

## 🚀 How to Use

### Step 1: Setup Assets
1. Create a **Version Manifest** in your runtime data folder: `Create > GameLib > VersionHistory > Version Manifest`.
2. Create a **Builder Settings** in your editor data folder: `Create > GameLib > VersionHistory > Builder Settings`.

### Step 2: Configuration
Select the `VersionBuilderSettings` asset:
* Set the **Git Repository Path** (e.g., `./` for root, or `./Assets/Libs/MySubmodule` for a submodule).
* Drag and drop your newly created `VersionManifest` into the **Target Manifests** list.
* Configure the Markdown export path and intro text.

### Step 3: Generation
Use the custom inspector buttons at the bottom of the `VersionBuilderSettings` asset:
* Click **1. Sync Git History** to read Git tags and populate the `VersionManifest`.
* Click **2. Export Markdown** to generate the `CHANGELOG.md` to your disk.

---

## 📝 Commit Mapping Rules

The parser follows strict [Conventional Commits](https://www.conventionalcommits.org/). Commits are grouped in the UI and Markdown according to these rules:

| Commit Keyword | Mapped Category | Notes |
| :--- | :--- | :--- |
| `feat` / `added` | **Features** | Minor version bump. |
| `fix` / `bugfix` | **Bug Fixes** | Patch version bump. |
| `perf` | **Performance** | Performance optimizations. |
| `!` or `BREAKING CHANGE` | **Breaking Changes** | Major version bump. Bypasses base type. |
| `docs` | **Documentation** | |
| `build` / `ci` | **Build & CI** | |
| `chore`, `refactor`, `style` | **Internal** | *Auto-excluded* from public changelogs by default to reduce noise. |

---

## ⚙️ Dependencies
* **UniTask** (`com.cysharp.unitask`): For asynchronous process handling.
* Local Git installation (must be accessible via command line/environment path).