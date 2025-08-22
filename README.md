# Custom Difficulty Mod For Deltarune
Mod that adds difficulty options to Deltarune. Make the game easy, hard, or brutal.

<img src="https://github.com/user-attachments/assets/1583688c-a3fc-4c69-bc3c-c215248cdec8" width="480" />

## Download
[Latest release](https://github.com/Emmehehe/CustomDifficultyModForDeltarune/releases/tag/1.1.2)

## Installation

### Windows

**Quick start**
1) Download and unzip the release
2) Double-click `install-windows.cmd`

**What it does**
- Finds your DELTARUNE install (or lets you choose)
- Downloads UndertaleModTool CLI if missing
- Backs up originals to `ModBackups/<timestamp>`
- Patches all chapters; safe to re-run

**CLI (alternative)**
```powershell
.\install-windows.cmd
````

**If blocked**

* SmartScreen: “More info → Run anyway” or right-click → Properties → Unblock
* Permission denied under Program Files/Steam: run as Administrator

### macOS

**Quick start**

1. Download and unzip the release
2. Double-click `install-macos.command`

**What it does**

* Detects your DELTARUNE install (or prompts)
* Downloads UndertaleModTool CLI if needed
* Backs up to `ModBackups/<timestamp>`
* Patches all chapters; safe to re-run

**CLI (alternative)**

```bash
./install-macos.command
```

**If blocked**

* Right-click → Open, or allow in System Settings → Privacy & Security

### Advanced Options

#### Windows (`install-windows.ps1`)
- `-Uninstall` - Restore from the most recent backup
- `-NoBackup` - Skip creating backups before patching
- `-GameDir <path>` - Specify DELTARUNE folder path (skips auto-detect)
- `-UtmtCli <path>` - Path to UndertaleModCli.exe (skips download/search)

Example: `.\install-windows.ps1 -NoBackup -GameDir "D:\Games\DELTARUNE"`

#### macOS (`install-macos.command`)
- `--uninstall` - Restore from the most recent backup
- `--no-backup` - Skip creating backups before patching
- `--app <path>` - Path to DELTARUNE.app (skips picker)
- `--utmt <path>` - Path to UndertaleModCli (skips download/search)

Example: `./install-macos.command --no-backup --app /Applications/DELTARUNE.app`

### Manual Installation

If the installation scripts don't work, you can manually install the mod using UndertaleModTool CLI:

1. Download [UndertaleModTool CLI v0.8.3.0](https://github.com/UnderminersTeam/UndertaleModTool/releases/tag/0.8.3.0) for your platform
2. Extract the CLI tool to a folder
3. Apply the mod scripts to each chapter's data file:

#### Windows Manual Commands
```batch
# First apply modmenu to all chapters
UndertaleModCli.exe load "chapter1_windows\data.win" --scripts "src\modmenu_ch1to4.csx" --verbose false --output "chapter1_windows\data.win"
UndertaleModCli.exe load "chapter2_windows\data.win" --scripts "src\modmenu_ch1to4.csx" --verbose false --output "chapter2_windows\data.win"
UndertaleModCli.exe load "chapter3_windows\data.win" --scripts "src\modmenu_ch1to4.csx" --verbose false --output "chapter3_windows\data.win"
UndertaleModCli.exe load "chapter4_windows\data.win" --scripts "src\modmenu_ch1to4.csx" --verbose false --output "chapter4_windows\data.win"

# Then apply custom difficulty to all chapters
UndertaleModCli.exe load "chapter1_windows\data.win" --scripts "src\customdifficulty_ch1to4.csx" --verbose false --output "chapter1_windows\data.win"
UndertaleModCli.exe load "chapter2_windows\data.win" --scripts "src\customdifficulty_ch1to4.csx" --verbose false --output "chapter2_windows\data.win"
UndertaleModCli.exe load "chapter3_windows\data.win" --scripts "src\customdifficulty_ch1to4.csx" --verbose false --output "chapter3_windows\data.win"
UndertaleModCli.exe load "chapter4_windows\data.win" --scripts "src\customdifficulty_ch1to4.csx" --verbose false --output "chapter4_windows\data.win"
```

#### macOS Manual Commands
```bash
# First apply modmenu to all chapters
./UndertaleModCli load chapter1_mac/game.ios --scripts src/modmenu_ch1to4.csx --verbose false --output chapter1_mac/game.ios
./UndertaleModCli load chapter2_mac/game.ios --scripts src/modmenu_ch1to4.csx --verbose false --output chapter2_mac/game.ios
./UndertaleModCli load chapter3_mac/game.ios --scripts src/modmenu_ch1to4.csx --verbose false --output chapter3_mac/game.ios
./UndertaleModCli load chapter4_mac/game.ios --scripts src/modmenu_ch1to4.csx --verbose false --output chapter4_mac/game.ios

# Then apply custom difficulty to all chapters
./UndertaleModCli load chapter1_mac/game.ios --scripts src/customdifficulty_ch1to4.csx --verbose false --output chapter1_mac/game.ios
./UndertaleModCli load chapter2_mac/game.ios --scripts src/customdifficulty_ch1to4.csx --verbose false --output chapter2_mac/game.ios
./UndertaleModCli load chapter3_mac/game.ios --scripts src/customdifficulty_ch1to4.csx --verbose false --output chapter3_mac/game.ios
./UndertaleModCli load chapter4_mac/game.ios --scripts src/customdifficulty_ch1to4.csx --verbose false --output chapter4_mac/game.ios
```

**Important Notes:**
- Make sure to backup your game files before applying the mod manually
- The mod scripts (`modmenu_ch1to4.csx` and `customdifficulty_ch1to4.csx`) must be applied in that order
- Paths are relative to your DELTARUNE installation directory
- On macOS, the chapter folders are inside `DELTARUNE.app/Contents/Resources/`

## How set options??
1. Open menu in a dark world.
2. Difficulty options are found in the 'MODS' section.

## What options do?
#### Damage Multi
Multiply all incoming damage by this value.
- Attacks that are scripted to leave a character at 1 HP, or other threshold, still do so.
- For weird attacks that deal damage as a percentage of the current HP, instead uses exponential logic to determine damage scaling. e.g. An attack that normally does 50% of your HP in vanilla, instead does 70.7% with double damage, and 25% with half damage. The calculation is thus: `dmgratio = vanilladmgratio^(1/dmgmulti)`.
- For any damage over time effects, either tick faster, or apply more damage, or combination of both - where appropriate - proportionate with the multiplier that has been set.

#### Hit.All
When switched on, hits that target a single character instead hits the entire party. 
- For no-hit runs, combine this setting with `Damage Multi=INF`.

#### I-Frames
Multiply the number of i-frames received after damage by this value.

#### TP Gain
Multiply TP gain from all sources (except item use) by this value.

#### Battle Rewards
Multiply post-battle rewards by this value.

#### Down Deficit
When a party member is downed, their HP is set to -50% max HP. This option lets you override that. 

#### Downed Regen
When downed, party members regen 12.5% max HP every turn. This option lets you override that.

#### Victory Res
When a battle is won, all downed party members are healed up to 12.5% max HP. This option lets you override that.
- OFF: Can also be switched off entirely by reducing past 0%.

<details> 
  <summary><strong>CHAPTER 3 SPOILERS...</strong></summary>

  > #### Gameboard Dmg X
  > Multiplier for the damage in the chapter 3 game boards.
  > - Only shows up in the menu in chapter 3.
  > - INHERIT - Can be set to inherit from the 'Damage Multi' setting by reducing past 0%.
  > - Attacks that are scripted to leave a character at 1 HP, or other threshold, still do so.
  >
  > #### Reward Ranking
  > When this option is turned on, the 'Battle Rewards' option also affects the ranking that you get from battles in the chapter 3 game boards.
  > - Only shows up in the menu in chapter 3.
</details>

## Compatibility
Will my vanilla saves work with this mod and vice-versa?
> **Yes.** This just reads and writes to a new .ini file. So no change to vanilla save data.

Is this compatible with X mod?
> **I can't garauntee anything.** This mod makes specific line edits to the dark world menu & various damage scripts, so those areas will be the source of any potential conflicts. Any mods that don't affect those areas should be ok?
