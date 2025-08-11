# Custom Difficulty Mod For Deltarune
Mod that adds difficulty options to Deltarune. Make the game easy, hard, or brutal.

<img src="https://github.com/user-attachments/assets/9a01549b-1ce8-4e94-ad12-e4dc05a860d4" width="480" />

## Download
Check [releases](https://github.com/Emmehehe/CustomDifficultyModForDeltarune/releases).

## Installation
1. (optional) Backup your `DELTARUNE\` folder.
2. Copy `diffops_installfiles\` (for Windows & MacOS) & `diffops_installer_windows.bat` (for Windows only) into your `DELTARUNE\` folder.
3. Run `diffops_installer_windows.bat` (if on Windows).
   - If you're on MacOS, you could try the script from [this pull request](https://github.com/Emmehehe/CustomDifficultyModForDeltarune/pull/2) ([branch for DL](https://github.com/Emmehehe/CustomDifficultyModForDeltarune/tree/add-installer-for-macos)). Let me know if it works.
   - Or you can manually apply the scripts to each chapter file using [UndertaleModTool](https://github.com/UnderminersTeam/UndertaleModTool/releases). Scripts > Run other script...
4. Done! You can remove the installer & install files now if you want.

## How set options??
1. Open menu in a dark world.
2. Difficulty options are found in the 'MODS' section.

## What options do?
#### Damage Multi
Multiply all incoming damage by this value.
- Default: 100%
- Attacks that are scripted to leave a character at 1 HP, or other threshold, still do so.
- For weird attacks that deal damage as a percentage of the current HP, instead uses exponential logic to determine damage scaling. e.g. An attack that normally does 1/2 your HP in vanilla, instead does 70.7% with double damage, and 1/4 with half damage. The calculation is thus: `dmgratio = vanilladmgratio^(1/dmgmulti)`.
- For any damage over time effects, either tick faster, or apply more damage, or combination of both - where appropriate - proportionate with the multiplier that has been set.

#### Down Deficit
When a character is 'downed', their health is put as far into the negatives as 50% of their max HP. This option lets you override that. 
- Default: 50%

#### Victory Res
When a battle is won, the game automatically resurrects any downed characters and heals them to 12.5% of their max HP. This option lets you override that.
- Default: 12.5%
- OFF: Can also be switched off entirely by reducing past 0%.

#### Downed Regen
Each turn that a character remains downed, they regen 12.5% of their max HP. This option lets you override that.
- Default: 12.5%

#### Hit.All
When switched on, hits that target a single character instead hits the entire party. 
- Default: OFF
- For no-hit runs, combine this setting with `Damage Multi=INF`.
- Warning: This option has not had a thorough test. I've only tested one encounter per chapter.

<details> 
  <summary><strong>CHAPTER 3 SPOILERS...</strong></summary>

  > #### Gameboard Dmg X
  > Multiplier for the damage in the chapter 3 game boards.
  > - Only shows up in the menu in chapter 3.
  > - Default: INHERIT
  > - INHERIT - Can also be set to inherit from the 'Damage Multi' setting by reducing past 0%.
  > - Attacks that are scripted to leave a character at 1 HP, or other threshold, still do so.
</details>

## Compatibility
Will my vanilla saves work with this mod and vice-versa?
> **Yes.**
> This just reads and writes to a new .ini file. So no change to vanilla save data.

Is this compatible with X mod?
> **I can't garauntee anything.** This mod makes specific line edits to the dark world menu & various damage scripts, so those areas will be the source of any potential conflicts. Any mods that don't affect those areas should be ok?
