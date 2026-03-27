# Testing Guide

## Unified test project
All automated tests now live under `top_speed_net/TopSpeed.Tests`.

## Run shared/domain tests
`dotnet test top_speed_net/TopSpeed.Tests/TopSpeed.Tests.csproj -c Debug -f net10.0 -v normal`

## Run game-level tests
`dotnet test top_speed_net/TopSpeed.Tests/TopSpeed.Tests.csproj -c Debug -f net472 -v normal`

## Run both
Run both commands above. `net10.0` validates shared logic; `net472` validates game-facing behavior.
