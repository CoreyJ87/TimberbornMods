# Game Statistics Endpoint - Investigation Notes

## Goal
Expose the game's built-in statistics (shown on the wonder completion screen) via a new `/MoreHttpApi/statistics` endpoint.

## Known Stats (from completion screen)
- Beavers born
- Water drunk
- Tails painted
- Trees cut
- Teeth chipped
- Bots manufactured
- Dynamite detonated
- Beavers Contaminated
- Beavers Injured
- Beavers Stung
- Died of Hunger
- Tunnels Created
- Science Produced

## Relevant Game Namespaces
These exist in the game assemblies and are already in `CommonGlobalUsings.targets`:
- `Timberborn.SettlementStatistics`
- `Timberborn.PopulationStatisticsSystem`
- `Timberborn.PopulationWorkStatistics`

The wonder completion UI lives in:
- `Timberborn.WonderCompletion`
- `Timberborn.GameWonderCompletion`
- `Timberborn.GameWonderCompletionUI`

## Next Steps
1. On Windows PC, open ILSpy/dnSpy and load the game DLLs from `Timberborn_Data/Managed/`
2. Look at types in `Timberborn.SettlementStatistics` - find the service class and its properties/methods
3. Check if it's a singleton injectable via `[Bind]` or needs to be accessed differently
4. Create a `StatisticsHandler.cs` in `Handlers/` that injects the service and returns all stats
5. Add corresponding models and dashboard card
