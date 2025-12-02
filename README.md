# OculusReportMenu 3.0
The goal of OculusReportMenu 3.0 is to make it less reliant on the constantly changing GorillaMetaReport class by creating our own leaderboard to report with. This is an experimental idea I had, don't expect this to be implemented.

## To-do
- [ ] Build an Asset Bundle to start the leaderboard with, ideally, a big board simular to how it currently is
- [ ] [Create a `MonoBehaviour` to mimic GorillaMetaReport](#important-functionality-to-implement)
- [ ] Rework bindings so the system is less confusing
- [ ] Get SteamVR Linux support (switch to .NET standard over .NET framework)

## Important functionality to implement
- Use `DuplicateScoreboard()` method to keep support with other mods that change the leaderboard (such as GorillaFriends and Scoreboard Tweaks)
