# Phantasy Star Generation 1 — Save State Editor

*Created by Fuindin.*

A Windows Forms (.NET 10) editor for **PCSX2** save states (`.p2s`) of *Phantasy Star
Generation:1* (the PS2 remake of Phantasy Star; Japanese release **SLPM-623.62**). Its
look follows the game's own menus: dark windows with cyan frames, a neon-green plate,
lime stat labels and gold equipped items.

It edits:

* **Meseta**
* Each character's **level, EXP, next-level EXP, HP and MP (current and max), Attack, Defense,
  Magic Power, Magic Defense, Hit and Dexterity**
* Each character's **own item bag**: add, replace or remove unequipped items

It also **displays** each character's equipped gear and learned magic. It always writes a new
**`*_edited.p2s`** file and never modifies your original.

## Usage

1. `File ▸ Open…` and pick `PhantasyStarGeneration1.p2s`.
2. Pick Alisa, Tyrone, Myau or Lutz on the left.
3. Edit Meseta (MONEY) at the top and the character's tabs (**STATUS**, **EQUIP & ITEMS**, **MAGIC**).
4. `File ▸ Save As…` writes a new `*_edited.p2s`.
5. Load the edited state in PCSX2.

## How it works

`.p2s` files are ZIP containers whose regions use **Zstandard (ZIP method 93)**. The game's
PS2 RAM lives in the `eeMemory.bin` entry. `SaveFormat/P2SArchive.cs` parses the container
manually, decompresses with `ZstdSharp`, and repacks it. Untouched entries are reused
verbatim, and only `eeMemory.bin` is recompressed. This is shared with the PSG2 editor.

### Verified memory map (`eeMemory.bin`)

These offsets were anchored against Alisa's in-game Status screen (Lv 32, HP 255/255,
MP 187/187, EXP 119198, NEXT 133902) and all four characters' ITEM menus.

* **Character array**: base `0x191130`, stride `0x84`, **4 records** in roster order
  Alisa, Tyrone, Myau, Lutz (u32 id `0..3` at `+0x04`).
* **Per-record u32 fields**: `+0x08`/`+0x0C` HP cur/max, `+0x10`/`+0x14` MP cur/max,
  `+0x18` Attack, `+0x1C` Defense, `+0x20` Magic Power, `+0x24` Magic Defense, `+0x28` Hit,
  `+0x2C` Dexterity, `+0x30` Level, `+0x34` EXP, `+0x38` Next-level EXP.
* **Magic**: `+0x44`, one byte per learned spell id, 0-terminated (up to 32).
* **Equipped bitmask**: u32 at `+0x64`. Bit *i* set means bag slot *i* is equipped.
* **Item bag**: `+0x68`, u16 item ids, 0-terminated (the record has room for 14).
* **Meseta**: u32 at absolute `0x1910B0`, in the party header just before the array.
  Verified against the MAIN MENU "MONEY" readout (56302 M).

Offsets and limits can be overridden at runtime with a `psg1_layout.json` next to the
executable. Its property names match `Game/Psg1Layout.cs`, for example
`{ "ItemCapacity": 12, ... }`. A file you supply must include the `Fields` and `Members` lists.

> **Attack / Defense / Dexterity are base values.** The Status screen adds equipment
> bonuses: Alisa stores ATT 116 / DEF 119 / DEXT 106 and shows 178 / 168 / 140 with her
> Ceramic Sword, Light Suit, Ceramic Shield and Frozhe Crystal. HP, MP, M.POW, M.DEF, HIT,
> Level, EXP and NEXT match the screen exactly.

### Item IDs

All items share one ID space: shields are low (`0x01–0x0F`), followed by armor, weapons,
crystals, and usable items from `0x54`. These 23 IDs are confirmed:

| ID | Item | ID | Item | ID | Item |
|----|------|----|------|----|------|
| 0x03 | Ceramic Shield | 0x21 | Wild Loincloth | 0x58 | Atlas |
| 0x07 | Protector | 0x2C | Ceramic Sword | 0x59 | Antidote |
| 0x0A | Plasma Barrier | 0x31 | Battle Axe | 0x5C | Escape Cloth |
| 0x0D | Leather Gloves | 0x3F | Wooden Staff | 0x5F | Soothing Flute |
| 0x12 | Light Suit | 0x41 | Long Mace | 0x60 | Searchlight |
| 0x18 | Steel Armor | 0x45 | Bronze Claw | 0x61 | TranCarpet |
| 0x1D | Silk Mantle | 0x4F | Frozhe Crystal | 0x6B | LandMaster |
| | | 0x54 | Trimate | 0x6C | FlowMover |

Name more items, without rebuilding, by editing **`psg1_items.json`** next to the exe.
Keys can be decimal or `0x`-hex. Unknown IDs show as `Item #0xNN`, and you can still add
them by typing the hex ID.

### Spell IDs

Spells are stored in ascending ID order and shown on the Status screen left to right, top to
bottom. These names were confirmed against Alisa's, Myau's and Lutz's MAGIC panels:

| ID | Spell | ID | Spell | ID | Spell | ID | Spell |
|----|-------|----|-------|----|-------|----|-------|
| 0x01 | Heal | 0x07 | Warla | 0x0D | Tandre | 0x15 | Lucsta |
| 0x02 | Raheal | 0x08 | Muwarla | 0x0E | Gitandre | 0x16 | Powma |
| 0x03 | Giheal | 0x09 | Flaeli | 0x0F | Frozhe | 0x17 | Telele |
| 0x04 | Doheal | 0x0A | Giflaeli | 0x11 | Bindwa | 0x18 | Rapt |
| 0x05 | Curala | 0x0B | Hewn | 0x12 | Hypnos | 0x19 | Surut |
| 0x06 | Rebirth | 0x0C | Gihewn | 0x13 | Silence | 0x1A | Tolupa |

`psg1_spells.json` next to the exe can add or rename spells.

## Limits and not-yet-mapped

* **Equipped items are locked.** The editor won't remove or swap gear that is equipped,
  so the bitmask stays consistent. Equip new gear in-game.
* **Bag size** is capped at **10**, the largest bag seen in a real save. The record has
  room for 14; raise `ItemCapacity` in `psg1_layout.json` if the game accepts more.
* **Key items** (Road Pass, Passport, …) are not mapped yet.

## Project layout

* `SaveFormat/P2SArchive.cs`: `.p2s` (zip + zstd) reader/writer.
* `Game/Psg1Layout.cs`: offsets, field definitions and roster (JSON-overridable).
* `Game/Psg1Items.cs`: item and spell name tables.
* `Game/Psg1Character.cs`, `Game/Psg1Save.cs`: the read/write model.
* `MainForm.*`: the UI. Static layout is in `.Designer.cs`; theme and behaviour are in `.cs`.
* `Theme.cs`: the PSG1 menu palette and chamfered-window painting.
* `portraits/`: per-character portraits (`<Name>.png`), copied next to the exe.
