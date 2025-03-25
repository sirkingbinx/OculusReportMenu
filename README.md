# Oculus Report Menu for PCVR ![all](https://img.shields.io/github/downloads/bingus-dev/OculusReportMenu/total) ![latest release)](https://img.shields.io/github/downloads/bingus-dev/OculusReportMenu/latest/total?style=flat&label=downloads%20(latest%20release)) ![latest beta)](https://img.shields.io/github/downloads-pre/bingus-dev/OculusReportMenu/latest/total?style=flat&label=downloads%20(latest%20rc))

This basic mod lets you access the Oculus Report Menu on SteamVR and Oculus Rift / Quest Link. Activate by pressing your right thumbstick in and clicking your left secondary button.

## Installation
### From MonkeModManager
You can install this mod with [MonkeModManager](https://github.com/the-graze/monkemodmanager), look for "OculusReportMenu" under "Tweaks / Tools" <br>
![image](https://github.com/user-attachments/assets/21879ee0-dd12-446f-9a3b-8782fae407fb)

> Note: You can install this with [Graze / new UI MMM](https://github.com/the-graze/monkemodmanager), [Ngbatz / old UI MMM](https://github.com/ngbatzyt/monkemodmanager), or [MechanicMonke](https://github.com/bingus-dev/MechanicMonke)

### From Releases
Look for the latest release, and download ``OculusReportMenu.dll``. Drag this into your plugins folder.

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
| None | none | none

Here is the default configuration:
```
openButton1 = RJ # right thumbstick in
openButton2 = LS # left secondary in
```

If you want to bind this to one button:
```
openButton1 = LP
openButton2 = none
```

## Why?
Every major mod menu that Gorilla Tag modders use include an "anti-report" that will remove them from the lobby the moment it detects a user pressing the report button over their name. The Oculus report menu bypasses this by placing a leaderboard in front of you so you can report them without their anti-report kicking in.

This has never been avaliable to PCVR users, and it is super easy to implement in a small mod. This mod provides the same functionality and is completely legal and cannot get you banned.

## Usage
Press your secondary button on the left controller to open the menu. Close it with the "X" button at the top of the screen.
