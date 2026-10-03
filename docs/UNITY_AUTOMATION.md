# Unity automation for agents

DiceFree Unity validation and build work should use Unity CLI connected to the
licensed Editor through Unity's Pipeline package. This lets Codex discover the
exact project, wait for import/compilation, call Editor commands, and read
structured results without asking Axel to click validation menus.

## Prerequisites and first-time setup

- Install the Unity CLI using Unity's supported installer and verify it with
  `unity --version` and `unity doctor`.
- Use the Unity Editor version recorded in `ProjectSettings/ProjectVersion.txt`.
- Sign in once with `unity auth login` if `unity auth status --json` reports no
  valid session. Complete browser consent yourself; never ask Axel to paste a
  password, token, serial, or credential into chat.
- Check the license using `unity auth status --json` and `unity license status`.
  Never return, reset, or copy a working license automatically.
- DiceFree includes `com.unity.pipeline` in `Packages/manifest.json`. The CLI
  setup command `unity pipeline install --project-path <project>` is the
  supported installation path for other checkouts. After installation,
  `unity pipeline list` should report the connected Editor and installed
  package.
- `unity mcp configure codex --project-path <project>` configures Unity's
  official Codex MCP integration. `unity skill install codex --local` installs
  Unity's CLI guidance into the project for compatible Codex CLI sessions.
  Prefer the official CLI commands over hand-editing global agent config.

## Open and wait for the exact Editor

Always use a clean, disposable worktree for validation that can write project
files or save data. Resolve and pass the full project path so another open Unity
project cannot receive the command:

```powershell
$project = 'T:\TEMP\DiceFree-validation-worktree'
unity open $project --editor-path 'C:\Path\To\Unity.exe'
unity status --project-path $project --until-ready --timeout 600 --format json
unity pipeline list --format json
```

The status result identifies the project, Editor version, PID, and connection
port. Wait for `state: ready` before running commands. If `unity status` cannot
read the Editor discovery file because the agent runs under another Windows
account, run the official CLI in the authorized local context; do not replace
this readiness check with an arbitrary delay.

## Discover and run DiceFree commands

```powershell
unity list --project-path $project
unity command --project-path $project --query dicefree
unity command dicefree.architecture.validate --project-path $project --format json
unity command dicefree.architecture.policy --project-path $project --format json
unity command dicefree.managed-references.validate --project-path $project --format json
```

Issue #36 Play Mode checks cross Editor state transitions, so they return a
task ID instead of holding the CLI connection open. Poll the status command with
that ID until `status` is `passed` or `failed`:

```powershell
$start = unity command dicefree.issue36.route12 --project-path $project --format json | ConvertFrom-Json
$taskId = $start.data.result.taskId
unity command dicefree.issue36.status --task_id $taskId --project-path $project --format json
```

The same task flow applies to:

- `dicefree.issue36.route12`
- `dicefree.issue36.runner`
- `dicefree.issue36.save-reload`

Results report success, final marker, elapsed seconds, error, isolated temp save
root, Editor log path, and tested Git SHA when supplied. The save/reload command
uses one new temporary root for both phases; the Runner command also creates a
fresh isolated root. Do not point these checks at normal user profiles. The
route diagnostic does not write a save.

Synchronous architecture and managed-reference validators return their
structured result directly. Never report a check as passing from command
submission alone; require `success: true` and its expected final marker.

## Builds, tests, and logs

Use the connected Editor and installed CLI's own help as the syntax authority:

```powershell
unity build --help
unity test --help
unity logs --help
unity command --help
```

Use `unity build` and `unity test` for their supported targets and report files.
Use `unity logs` or the `logPath` returned by a command to inspect failures.
Avoid printing raw Editor startup command lines: they can include temporary
authentication arguments. Filter logs to relevant markers and errors, and never
commit logs, credentials, generated `Library/`, or temporary save roots.

## Worktree and failure rules

- Fetch and verify the intended branch and SHA before editing.
- Use an isolated worktree for validation and builds; inspect `git status` after
  Unity import and restore unrelated generated project-setting changes before
  committing.
- Keep save roots in the operating system temp directory. Preserve a failed
  validation root long enough to inspect it, then clean it only when no longer
  needed.
- Prefer this order: connected Unity CLI + Pipeline Editor; Unity CLI headless
  run only where proven; direct `Unity.exe -batchmode -executeMethod` as a
  legacy/CI fallback; human menu operation only if the official CLI path is
  unavailable or needs a one-time authentication/consent action.
- Ask for human help only for a specific authentication/consent prompt or when
  the official CLI/Pipeline route is unavailable. Do not ask Axel to click a
  validation menu as a routine agent workflow.

## DiceFree Issue #36 commands

The Editor-only Pipeline bridge currently provides:

| Command | Result |
| --- | --- |
| `dicefree.architecture.validate` | Assembly boundaries and source ownership |
| `dicefree.architecture.policy` | Architecture policy self-tests |
| `dicefree.managed-references.validate` | Managed-reference and migration validation |
| `dicefree.issue36.route12` | Focused route-12 traversal diagnostic |
| `dicefree.issue36.runner` | Existing dedicated Runner Returns harness |
| `dicefree.issue36.save-reload` | Existing two-phase save/reload harness |
| `dicefree.issue36.status` | Structured result for a long-running Issue #36 task |

Manual Editor menu tools remain available for a person, but agents should use
the Pipeline commands above. This bridge is Editor-only; it does not add
runtime gameplay behavior or change the save schema.

## Official references

- [Unity CLI](https://docs.unity.com/en-us/unity-cli)
- [Unity Pipeline package](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package)
- [Unity CLI reference](https://docs.unity.com/en-us/unity-cli/unity-cli-reference)
