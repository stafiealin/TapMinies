# Mobile Game Assets: An Agent's Playbook

Written from the TapMinies build. Every "don't" here is a mistake that actually happened in this
repo, not a hypothetical. Point an agent at this file before it touches art or UI.

---

## 1. The mental model

Mobile is not "PC but smaller." Four constraints drive every asset decision:

**The screen is 5–6 inches, held at arm's length, often in sunlight, often one-handed.**
If an asset doesn't read at 120×120 px, it does not exist. Before approving any sprite, view it at
thumbnail size. The `Read` tool renders images — use it. In this project, every composed portrait was
eyeballed at thumbnail *before* being wired in, which is why odd source dimensions (132×250 vs
192×192) didn't matter: they still read.

**The player's thumb covers the bottom third during play.**
Panels and buttons belong there (you open them deliberately). Critical *feedback* — damage numbers,
boss timers, health — must live in the top half where the hand isn't.

**Silhouette beats detail.**
At mobile size, interior texture detail is wasted memory. Strong outline + flat fill + high contrast
reads instantly. This is why Kenney-style vector art dominates the category, and why it survives
being scaled to any device.

**Colour is information, not decoration.**
Reserve saturation for things the player can act on. Backgrounds must be desaturated so foreground
pops. The single most common amateur failure is a beautiful background that fights the UI.

---

## 2. Sourcing decision tree

```
Need an asset
├─ Can it be COMPOSED from parts already licensed in-repo?  → do that first (zero download, zero new license)
├─ Is AI generation available and licensed for commercial use? → generate
├─ Is there a CC0 pack in the established art style?         → download, commit LICENSE beside it
└─ Otherwise                                                 → flat placeholder with CORRECT dimensions
```

**Composition is the underrated option.** This project needed 5 hero portraits and 5 enemy variants.
Rather than sourcing 10 new files, they were baked from a modular pack already in the repo
(6 body colours × 6 shapes × 17 eyes × 14 mouths). Cost: one Editor script, zero downloads,
zero new licensing surface.

**When AI generation is unavailable**, don't stall — Unity AI returned `NoSubscription` here, so the
fallback was CC0. Have the fallback ready before you need it.

**Licensing hygiene:** commit the license file *next to the assets* (`Assets/Sprites/Kenney/Kenney_License.txt`),
not in a README nobody reads. If the pack is CC-BY, add attribution to the repo the same hour you add
the art — never "later."

---

## 3. Unity import settings that actually matter

| Setting | Value for UI sprites | Why |
|---|---|---|
| Texture Type | `Sprite (2D and UI)` | Default is often `Default`; sprites won't slice/pivot correctly |
| Mip Maps | **Off** | UI is never minified. Mips cost +33% memory for nothing |
| Read/Write | **Off** in shipping | Keeps a full CPU-side copy in RAM. Only enable to composite, then turn off |
| Compression | Compressed (ASTC on mobile) | Uncompressed UI atlases blow up build size |
| Filter Mode | Bilinear (smooth art) / Point (pixel art) | Point on smooth art = jaggies; Bilinear on pixel art = mush |
| `preserveAspect` | **true** when sprite aspect ≠ rect aspect | Otherwise art stretches. Non-negotiable with mixed-size sources |
| `raycastTarget` | **false** on every non-interactive Graphic | Each raycast target costs per-touch work. Free perf |

**9-slice or suffer.** One 64×64 button sprite with borders set in the Sprite Editor, `Image Type =
Sliced`, scales to every button in the game. The alternative is a dozen fixed-size PNGs and a
relayout every time text changes.

**CanvasScaler for portrait mobile:** `ScaleWithScreenSize`, reference `1080×1920`, match `0.5`.
Anchor to edges and corners — never centre-with-a-big-offset, which breaks on other aspect ratios.

---

## 4. Do / Don't

### Assets
| Do | Don't |
|---|---|
| Give placeholders the **real final dimensions and pivot** | Ship a 400×400 colour square, then discover real art is 512×384 and relayout everything |
| Name by function: `btn_primary`, `enemy_tier3` | Name by appearance: `blue_button`, `green_guy` — colours change, roles don't |
| Bake multi-layer art into one sprite for list rows | Keep 3 stacked `Image` children per row × 50 rows = 150 draw calls |
| Verify visually at target size before wiring | Trust that "it compiled" means "it looks right" |
| Atlas UI into one Sprite Atlas page | Let every sprite be its own draw call |

### Unity / code
| Do | Don't |
|---|---|
| Use **TextMeshPro** | Use legacy `Text` — deprecated. *(This repo uses legacy Text; it's outstanding debt)* |
| Drive visuals from `ScriptableObject` data | Hardcode `Palette[]` colour arrays in a controller *(this repo did, now fixed)* |
| Fully qualify `UnityEngine.UI.Image` | Write bare `Image` — collides with the `UnityEngine.UI.Image` **namespace** in Unity 6. Cost: 3 failed compiles here |
| `transform.Find()` for possibly-inactive objects | `GameObject.Find()` — **silently skips inactive objects** and returns null. Cost: 1 NullReference here |
| Commit `.meta` files | Gitignore them — Unity's entire reference system is GUIDs in `.meta`. Losing them detaches every reference |

### Events & lifecycle (the subtle one)
| Do | Don't |
|---|---|
| Guard save/analytics handlers with an `initialized` flag | Subscribe to events that your own **init sequence fires** |

This was the nastiest real bug in this project. `StageManager.Initialize()` raises `OnStageChanged`
(correct for gameplay). But `GameSaveController` subscribed to that event — so loading a save
triggered an autosave *mid-load*, before hero levels were restored, silently overwriting a good save
with defaults. It looked like "load is broken"; it was actually "save fired too early."

**Rule: any handler that writes persistent state must be inert until initialization completes.**

---

## 5. Agent-specific failure modes

Things an autonomous agent gets wrong that a human wouldn't:

1. **Declaring a data field and never wiring it.** `HeroData.portrait` existed for a full milestone
   while the UI drew hardcoded colour squares. The schema looked done; the feature wasn't.
   *Check: grep every serialized field for a second usage site.*

2. **Treating "no compile errors" as "verified."** Compilation says nothing about whether the sprite
   is assigned, the anchor is right, or the thing is visible. Always assert on runtime state.

3. **Not noticing missing variety.** Stage 1 and stage 47 rendered identically for three milestones.
   Systems were correct; the *experience* was flat. Ask "what does the player see change?"

4. **Fixed offsets across variable-size inputs.** Compositing used `eyeY = 108` for every body, but
   bodies ranged 165–250 px tall. It happened to work — because the source pack keeps faces in a
   consistent relative zone. That was luck. Prefer proportional offsets (`h * 0.56f`).

5. **Destructive verification.** Tests here advanced the *real* save file from stage 9 to 21.
   Snapshot persistent state before testing against it, or point tests at a temp path.

---

## 6. Pre-commit checklist

- [ ] Every sprite viewed at target size, not just "imported without error"
- [ ] `preserveAspect` on any Image whose sprite aspect ≠ rect aspect
- [ ] `raycastTarget = false` on decorative Graphics
- [ ] Mipmaps off, Read/Write off on shipping UI textures
- [ ] License file committed beside third-party art
- [ ] `.meta` files staged
- [ ] No hardcoded colours/sizes that belong in data
- [ ] Runtime state asserted, not just compilation
- [ ] Persistent user state (saves) unchanged, or restored after testing

---

## 7. Generating assets with no external input

Everything below was produced for this project with zero downloads and zero AI generation — just
arithmetic written to `Color[]` / PCM buffers and saved through `AssetDatabase`.

**Raster (see the UI/VFX generator pass):**
- 9-slice panels and buttons from a rounded-box **signed distance function**. `alpha =
  clamp01(0.5 - dist)` gives free 1px anti-aliasing; a vertical gradient sells "lit from above."
- Set `TextureImporter.spriteBorder` in the same pass, and keep **border > corner radius** or the
  corners smear when stretched.
- Glow/spark textures are radial falloff, `pow(1 - r, k)`. Tune `k` for tightness.
- Generate **white**, tint via `Image.color`. One sprite serves every colour in the game.

**Audio (see the SFX synthesis pass):**
- A WAV is a 44-byte header plus PCM samples. Oscillator + noise + `exp(-t*k)` decay covers most
  arcade SFX — this is exactly what sfxr/bfxr do.
- Always `tanh` soft-clip and fade the last ~5ms, or every sound ends on a click.
- Randomise pitch per playback (±8%). Identical repeated samples fatigue the ear within seconds in a
  tap game — this single line does more for feel than any individual sound.

**What procedural genuinely can't do:** organic character art, illustration, real music, and
characterful typography. Recombining a modular art pack is the practical ceiling. Budget for those
four categories; generate everything else.

---

## 8. What this project still owes

1. **Legacy `Text` throughout** — migrate to TextMeshPro (already in the project) before the UI grows.
   Deliberately *not* half-migrated: a consistent old API beats a split codebase.
2. **No background/environment art** — flat URP clear colour.
3. **No visual regression check.** Everything here is verified by asserting runtime state; the
   Screen-Space-Overlay canvas can't be captured by the available scene-capture tooling, so layout
   is unverified by eye. Worth a manual look before shipping.
