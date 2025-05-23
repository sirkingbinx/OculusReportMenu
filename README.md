# OculusReportMenu ![all](https://img.shields.io/github/downloads/sirkingbinx/OculusReportMenu/total)
This basic mod lets you access the Oculus Report Menu on SteamVR and Oculus Rift / Quest Link. Activate by pressing your right thumbstick in and clicking your left secondary button.

## Installation
### From Mod Manager
You can install this mod with [MonkeModManager](https://github.com/the-graze/monkemodmanager), look for "OculusReportMenu" under "Tweaks / Tools" <br>
![image](https://github.com/user-attachments/assets/21879ee0-dd12-446f-9a3b-8782fae407fb)

> Note: You can install this with [MonkeModManager](https://github.com/the-graze/monkemodmanager) or [my own mod manager](https://github.com/sirkingbinx/binxmodmanager). Use the MMM source to download

### From Releases
Look for the latest release, and download ``OculusReportMenu.dll``. Drag this into your plugins folder.

### From Source
> [!NOTE]
> Must have VS2022.

Build with CTRL + B.

## Config
You can change the buttons you use with OculusReportMenu if your controller doesn't have the buttons used by default. Check your config and change it yourself.
Here is a guide to the keybinds:
| *                        | Left Controller   | Right Controller |
| ---------------------| ----------------------- | ------------------------|
| Primary            |LP                          |RP                          |
| Secondary        |LS                          |RS                          |
| Grip                   |LG                         |RG                          |
| Trigger               |LT                         |RT                          |
| Thumbstick In |LJ                            |RJ                          |
| None | NAN | NAN |

Here is the default configuration:
```
openButton1 = RJ # right thumbstick in
openButton2 = LS # left secondary in
```

If you want to bind this to one button:
```
openButton1 = LP
openButton2 = NAN
```

You can also adjust the sensitivity for when it detects triggers and grips.
```
Sensitivity = 0.5
```

## Why?
Every major mod menu that Gorilla Tag modders use include an "anti-report" that will remove them from the lobby the moment it detects a user pressing the report button over their name. The Oculus report menu bypasses this by placing a leaderboard in front of you so you can report them without their anti-report kicking in.

This has never been avaliable to PCVR users, and it is super easy to implement in a small mod. This mod provides the same functionality and is completely legal and cannot get you banned.

## Usage
Press keybinds you set (by default, it is right stick click + left secondary) to open it. Press big X button to close.
