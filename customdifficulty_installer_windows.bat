@echo off
echo Installing mod menu and custom difficulty for all chapters...
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter1_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\modmenu_ch1to4.csx" --verbose false --output "chapter1_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter2_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\modmenu_ch1to4.csx" --verbose false --output "chapter2_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter3_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\modmenu_ch1to4.csx" --verbose false --output "chapter3_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter4_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\modmenu_ch1to4.csx" --verbose false --output "chapter4_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter1_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\customdifficulty_ch1to4.csx" --verbose false --output "chapter1_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter2_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\customdifficulty_ch1to4.csx" --verbose false --output "chapter2_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter3_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\customdifficulty_ch1to4.csx" --verbose false --output "chapter3_windows\data.win"
"customdifficulty_installfiles\UTMT_CLI_v0.8.3.0-Windows\UndertaleModCli.exe" load "chapter4_windows\data.win" --scripts "customdifficulty_installfiles\Scripts\customdifficulty_ch1to4.csx" --verbose false --output "chapter4_windows\data.win"
echo Finished installation.
pause