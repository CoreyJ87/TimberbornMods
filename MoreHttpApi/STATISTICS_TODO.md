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

## Implementation

### ILSpy Findings
- `IncrementalStatisticCollector` is the central service (bound as singleton in `SettlementStatisticsConfigurator`)
- `GetOrDefault(string) : int` returns the value for a given stat ID
- `StatisticIds` has string constants for all stat IDs
- Individual `*StatisticCollector` classes are event listeners that call `Increment()` on the central collector

### Files Created
- `MoreHttpApi.Shared/StatisticsModels.cs` — `HttpStatistic` and `HttpStatisticsInfo` records
- `MoreHttpApi/Handlers/StatisticsHandler.cs` — endpoint at `/MoreHttpApi/statistics`

### API
- `GET /MoreHttpApi/statistics` — returns all settlement statistics as `{ Statistics: [{ Id, Value }, ...] }`

### Remaining
- [ ] Add dashboard card in the frontend (if applicable)
- [ ] Test in-game after building the mod
