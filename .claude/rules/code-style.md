---
paths:
  - "src/**/*.cs"
---

# C# code style

Same conventions as `../warden`: Valheim's own style, so the code reads naturally next to the game
classes it extends.

## Naming

- Instance fields: `m_camelCase` (`m_nview`, `m_previewText`). Static fields: `s_camelCase`
  (`s_emblemKey`, `s_painted`). Constants: `PascalCase` (`DesignWidth`, `MaxTextLength`).
- The plugin's `_harmony` is the one exception; do not spread that style.
- Localization keys: `heraldry_*`.
- Types are `internal` by default; components that implement vanilla interfaces (`EmblemBanner`) are `public class`.

## Structure

- One namespace, `Heraldry`, as a block, not the file-scoped form.
- `using` directives at the top, `System*` first, then the rest alphabetically.
- Four-space indentation, Allman braces, single-line guard clauses without braces.
- Expression-bodied members for trivial getters and interface stubs.
- Tuning values are named constants near the top of the class with a short comment grouping them
  (as in `EmblemPainter`).
- Prefer `UnityEngine.Object` truthiness (`if (!renderer)`) over `== null` for Unity objects; `?.` and
  `??` bypass the destroyed-object check, so don't use them on a Unity object that may be destroyed.

## Comments

- Give new classes a `/// <summary>` saying what the class is for and the one non-obvious thing about it.
- Inline comments explain why (the game quirk, the reason for a number), never narrate the next line.

## Logging and failure handling

- Log through `Jotunn.Logger` (`LogWarning` for skipped content or degraded features, `LogError` for a
  failure to set up a banner). No `Debug.Log`. No diagnostic logging left in committed code.
- A missing vanilla prefab or resource logs a warning and skips that feature, never throws. Each
  banner is set up in its own try/catch in `BannerPieces.Register`, so one broken banner never stops
  the others.
