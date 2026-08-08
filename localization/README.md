# Translation contribution guide

Stat The Relics loads `localization/<language-code>.json`, where the filename is the lowercase language code reported by Slay the Spire 2's `LocManager`. The active code is checked whenever a relic tooltip is formatted, so changing the game language also changes the Mod language without a restart.

## Supported language codes

The following languages are bundled with Slay the Spire 2 `v0.110.1` and are accepted by the current game `LocManager`:

| Code | Language |
| --- | --- |
| `deu` | German — Deutsch |
| `eng` | English |
| `esp` | Spanish (Latin America) — Español (Latinoamérica) |
| `fra` | French — Français |
| `ita` | Italian — Italiano |
| `jpn` | Japanese — 日本語 |
| `kor` | Korean — 한국어 |
| `pol` | Polish — Polski |
| `ptb` | Portuguese (Brazil) — Português Brasileiro |
| `rus` | Russian — Русский |
| `spa` | Spanish (Spain/Castilian) — Español (Castellano) |
| `tha` | Thai — ไทย |
| `tur` | Turkish — Türkçe |
| `zhs` | Chinese (Simplified) — 中文（简体） |
| `zht` | Chinese (Traditional) — 繁體中文 |

These codes were verified against the language directories in the game PCK and the mappings in `LocManager`. The game is actively developed, so this table should be updated if a future release adds another bundled language.

## Add or update a translation

1. Copy [eng.json](./eng.json) to `<code>.json`, using the exact lowercase code from the table. For example, Simplified Chinese uses `zhs.json`.
2. Translate the JSON values only. English keys are stable identifiers and must remain unchanged and case-sensitive.
3. Keep the file valid UTF-8 JSON and do not create duplicate keys.
4. Preserve placeholders such as `{0}` and `{1}`. A translation may reorder them, but it must not remove or rename them.
5. Preserve formatting tags such as `[purple]`, `[/purple]`, and escaped newlines such as `\n` when they appear.
6. Do not add trailing commentary inside values. Keep labels short enough to fit a relic tooltip.
7. Submit the new or updated JSON file together with a note naming the language and the person who reviewed it.

Example:

```json
{
  "Cards Drawn": "抽到的牌",
  "No stats are available for this relic": "该遗物暂无可用统计数据",
  "StatTheRelics data was saved by mod version {0}, but the current mod version is {1}. No relic stats are available for this save.": "存档统计来自 Mod {0}，当前版本为 {1}，因此无法读取该存档的遗物统计。"
}
```

## Fallback behavior

- A missing language file falls back to `eng.json`.
- A missing or blank value falls back to the English key built into the Mod.
- An invalid JSON file falls back to `eng.json` and writes an error to the game log.
- Card, relic, potion, and other game-provided names are already supplied by the game and should not be added to this file.

Partial translations work, but a complete file is preferred so players do not see mixed languages.

## Test a translation

Place the file next to the installed Mod:

```text
StatTheRelics/
├── StatTheRelics.dll
└── localization/
    ├── eng.json
    └── <code>.json
```

Launch the game, enable Stat The Relics, select the matching game language, and hover several relics during a run. Also test a relic with multiple stat lines and a run-history tooltip. Switch between English and the translated language while the game is open to confirm both directions update correctly.

Before submitting, verify that the JSON parses successfully and compare its keys with `eng.json` so none are missing or renamed.
