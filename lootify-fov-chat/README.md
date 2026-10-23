# Lootify FOV Chat

Plugin CounterStrikeSharp pentru comanda `!fov` din chat.

## Cerinte

- Server CS2 cu Metamod si CounterStrikeSharp
- .NET 10 SDK pentru compilare sau GitHub Actions

## Build

`dotnet publish src/LootifyFov.csproj -c Release -o publish`

Sau incarca toate fisierele pe GitHub si ruleaza Actions > Build Lootify FOV > Run workflow. Descarca artifactul `LootifyFov-plugin`.

## Instalare

Copiaza `LootifyFov.dll` in `game/csgo/addons/counterstrikesharp/plugins/LootifyFov/`, apoi reporneste serverul.

## Comenzi chat

- `!fov` - solicita FOV 120
- `!fov 100` - solicita FOV 100
- `!fov reset` - revine la FOV 90

FOV-ul este individual, reaplicat la spawn. Valorile peste limitele motorului CS2 pot sa nu aiba efectul asteptat. Pluginul nu a fost verificat pe un server real. Offset disclaimer: pluginul nu foloseste offseturi hardcodate; actualizarile CS2/API pot modifica comportamentul.
