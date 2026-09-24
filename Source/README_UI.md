# Warcraft Platform Manager — source and validation

## What changed

- The main launcher is a fixed 760 × 552 window without scrollbars. Game folder and map/save controls occupy the left column; plugin selection and game options occupy the right. The Play action and status stay visible at the bottom. The header and its text are drawn from WinForms labels, without a banner image. Settings is a fixed 510 × 520 window without scrolling; the updater retains its proportional DPI-aware layout.
- Buttons use Windows text measurement, a single centered line, keyboard focus cues, and translated hover tips; map and folder paths remain available in hover tips. The settings dialog defaults to English consistently with the main window when no language has been saved.

- The header is drawn by WinForms with fixed English copy, even when Vietnamese or Chinese is selected. No old raster banner containing Vietnamese text is included. All controls outside the banner use `lang.ini` (EN/VN/CN).
- Buttons draw a single line with ellipsis rather than wrap and clip; short labels plus hover tips explain actions. The dark ComboBox paints over the light native arrow. The selected map shows its name and parent folder; hovering reveals the complete source path.
- Map launch keeps the original file and its save identity intact. A unique ASCII alias under `<War3>/Maps/WPM/` is reserved with `FileMode.CreateNew`; collisions cause retries and never overwrite an existing map. The loader receives this short relative path. Temporary files are deleted on game exit or launch failure. A long original source path is warned about, and an excessively deep game directory is rejected with a useful message.
- The plugin picker scans the game root for installed DLLs, the game `plugin/warcraft3` directory, the app `Profiles` folder, the game's `Profiles` folder, WorldEdit profile folders, and preserved plugin backups. The Rescan button refreshes the list. Switching plugins backs up existing root DLLs before cleaning them; an unchanged DLL set is backed up only once. These root-DLL snapshots do not include a full WorldEdit/YDWE installation. An Installed selection does not modify those DLLs.
- Save slots can be backed up, moved into a per-map `_Trash`, and restored with automatic name collision handling. Every successful game sync saves the previous slot bytes into `_History` and restores the game's original root `dz_w3_plugin.ini`. Failure to sync leaves the game root INI for manual recovery.

## Windows verification

1. Install the .NET 8 SDK with Windows desktop support, then run `dotnet build Phanmemwar3.csproj -c Release`.
2. Run `dotnet run --project tests/MapSaveManagerSmoke/Smoke.csproj` to check map isolation, short-name collisions without overwrites, backups, deletion and restore collisions, sync, history, and root INI restoration.
3. Open the fixed 760 × 552 launcher at 100%, 125%, and 150% display scaling and check that every card, button, tooltip, and the footer is visible. Windows work areas smaller than the scaled window need a larger screen or lower display scaling. Check Browse, Delete, Restore, YDWE, Play, ComboBox arrows, and keyboard focus. Try EN/VN/CN; the header should remain English.
4. On Warcraft III 1.27a, select a compatible plugin, an RPG map with a long filename, and a save slot. Start the game, verify it opens inside the map, save and exit, then check the selected slot and root INI. Repeat with an existing installed plugin and a Clean profile. Check that `Maps/WPM` contains no newly staged file after exit.

## Map path sources and limitations

The [YDWE launcher source](https://github.com/actboy168/YDWE/blob/master/Development/Core/YDWEStartup/LaunchWarcraft3.cpp) documents a War3 `-loadfile` failure when the map argument is 54 characters or longer and copies the map to a short relative path. [Microsoft's MAX_PATH documentation](https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation) describes the separate Windows 260-character limit; opt-in long-path behavior depends on the application manifest **and** Windows configuration. This project opts in through `app.manifest` but does not change system registry settings. Moving or renaming the *original* map changes its path-based save directory; the temporary launch alias does not.

This sandbox has no .NET SDK, Windows Forms runtime, Warcraft III, or YDWE binaries. C# parsing, translation-key consistency, layout measurements, XML validity, and ZIP integrity can be checked here, but a compiled Windows build and live gameplay require the above Windows steps.
