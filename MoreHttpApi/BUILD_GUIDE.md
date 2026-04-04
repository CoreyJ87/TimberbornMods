# Building & Testing MoreHttpApi

Step-by-step guide for building this mod on Windows.

## Prerequisites

- **Timberborn** installed via Steam
- **.NET 10 SDK** (for GameAssemblyPublicizer) — https://dotnet.microsoft.com/download
- **.NET SDK** also covers building the mod itself (netstandard2.1)
- **eMka.ModSettings** mod installed in Timberborn (from Steam Workshop or mod.io)
- **Harmony** mod installed in Timberborn
- **TimberUi** mod installed in Timberborn

## Step 1: Clone the repo

```
git clone https://github.com/CoreyJ87/TimberbornMods.git
cd TimberbornMods
git checkout feature/more-http-api-endpoints
```

## Step 2: Find your paths

You need two paths. Open Steam, right-click Timberborn > Manage > Browse Local Files to find the game folder.

**Game assemblies path** — the `Managed` folder inside the game install:
```
<SteamLibrary>\steamapps\common\Timberborn\Timberborn_Data\Managed
```
Example: `D:\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed`

**Mods folder** — where Timberborn loads mods from:
```
C:\Users\<YourUsername>\Documents\Timberborn\Mods\
```
You can verify this in-game: go to Options > Mods > the path shown at the top.

**Mod Settings scripts path** — eMka.ModSettings DLLs, in the Steam Workshop folder:
```
<SteamLibrary>\steamapps\workshop\content\1062090\3283831040\version-1.0\Scripts
```
(The workshop ID `3283831040` is for eMka.ModSettings. Verify by checking that folder exists.)

## Step 3: Update CommonProperties.targets

Edit `Targets\CommonProperties.targets` and update these two lines to YOUR paths:

```xml
<AssemblyPath>D:\YOUR\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed</AssemblyPath>
```

```xml
<GameModsFolder>C:\Users\YOUR_USERNAME\Documents\Timberborn\Mods\</GameModsFolder>
```

**Important:** Keep the trailing backslash on `GameModsFolder`.

## Step 4: Update GameAssemblyPublicizer paths

Edit `GameAssemblyPublicizer\Program.cs` and update the hardcoded paths at the top:

```csharp
const string GameAssembliesPath = @"D:\YOUR\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed";
ImmutableArray<string> SpecialFolders = [
    @"D:\YOUR\SteamLibrary\steamapps\workshop\content\1062090\3283831040\version-1.0\Scripts",
];
```

These must match your actual Steam library location.

## Step 5: Run GameAssemblyPublicizer

This generates "publicized" copies of the game DLLs so the mod can access internal game APIs:

```
cd GameAssemblyPublicizer
dotnet run
```

You should see output like:
```
Publicizing Timberborn.WeatherSystem.dll to ...\out\common
Publicizing Timberborn.WindSystem.dll to ...\out\common
...
```

When done, `GameAssemblyPublicizer\out\common\` should have a bunch of DLLs in it.

## Step 6: Build the mod

```
cd ..
dotnet build MoreHttpApi\MoreHttpApi.csproj
```

If it builds successfully, the build system automatically copies the compiled mod to:
```
<GameModsFolder>\MoreHttpApi\version-1.0\
```

### If you get build errors

The most likely issue is a property name mismatch on a game type. These are the spots
that might need adjusting based on the actual game API:

- `windService.WindStrength` in `WeatherHandler.cs` — could be `CurrentWindStrength` or similar
- `inventory.AllGoods` in `ResourceHandler.cs` — could be `Stock`, `Goods`, etc.
- `node.PowerOutput` / `node.PowerInput` / `node.Active` in `PowerHandler.cs`
- `entity.GetComponent<MechanicalNode>()` in `PowerHandler.cs`
- `entity.GetComponent<Inventory>()` in `ResourceHandler.cs`

To figure out the right names, use a .NET decompiler (ILSpy, dnSpy, or dotPeek)
to open the relevant DLL from `Timberborn_Data\Managed\` and look at the actual type.
For example, to check WindService properties, decompile `Timberborn.WindSystem.dll`.

## Step 7: Launch Timberborn and enable the mod

1. Launch Timberborn
2. Go to Mods
3. Enable **More HTTP API** (also make sure TimberUi, Harmony, eMka.ModSettings are enabled)
4. Load a save or start a new game

## Step 8: Configure the mod (optional but recommended)

In the mod settings for "More HTTP API":
- **Auto-start**: Enable this so the HTTP server starts automatically when you load a game
- **Port**: Default is 8080, change if needed

If you don't enable auto-start, you need to place an HTTP Adapter building in-game first.

## Step 9: Test the endpoints

With the game running and a save loaded, open a browser or terminal:

```
curl http://localhost:8080/MoreHttpApi/ping
curl http://localhost:8080/MoreHttpApi/weather
curl http://localhost:8080/MoreHttpApi/time
curl http://localhost:8080/MoreHttpApi/water
curl http://localhost:8080/MoreHttpApi/resources
curl http://localhost:8080/MoreHttpApi/power
curl http://localhost:8080/MoreHttpApi/districts
curl http://localhost:8080/MoreHttpApi/characters
```

Or just open `http://localhost:8080/MoreHttpApi/weather` in your browser.

### Expected responses

**GET /MoreHttpApi/weather**
```json
{
  "IsHazardousWeather": false,
  "HazardousWeatherType": null,
  "Cycle": 3,
  "CycleDay": 5,
  "TotalCycleDays": 20,
  "HazardousWeatherStartDay": 16,
  "TemperateWeatherDuration": 16,
  "HazardousWeatherDuration": 4,
  "WindStrength": 0.72
}
```

**GET /MoreHttpApi/time**
```json
{
  "PartialDayNumber": 12.65,
  "TotalDayNumber": 12,
  "HoursPassedToday": 10.5,
  "DaytimeLengthInHours": 16.0,
  "NighttimeLengthInHours": 8.0,
  "IsDaytime": true,
  "FormattedTime": "10:30",
  "Cycle": 3,
  "CycleDay": 5
}
```

**GET /MoreHttpApi/water**
```json
{
  "StreamGauges": [
    {
      "EntityId": "some-guid",
      "Name": "Stream Gauge",
      "WaterLevel": 3.72,
      "HighestWaterLevel": 5.1,
      "LowestWaterLevel": null
    }
  ]
}
```

**GET /MoreHttpApi/resources**
```json
{
  "TotalGoods": [
    { "GoodId": "Log", "Amount": 1247 },
    { "GoodId": "Plank", "Amount": 893 },
    { "GoodId": "Bread", "Amount": 412 }
  ],
  "ByDistrict": []
}
```

**GET /MoreHttpApi/power**
```json
{
  "Grids": [{
    "CurrentPowerSupply": 600,
    "CurrentPowerDemand": 450,
    "Nodes": [
      { "EntityId": "...", "Name": "Power Wheel", "PowerOutput": 200, "PowerInput": 0, "IsActive": true },
      { "EntityId": "...", "Name": "Lumber Mill", "PowerOutput": 0, "PowerInput": 50, "IsActive": true }
    ]
  }]
}
```

**GET /MoreHttpApi/districts**
```json
{
  "Districts": [
    { "EntityId": "...", "Name": "Main District", "Adults": 42, "Children": 8, "Bots": 5 }
  ]
}
```

## Troubleshooting

**"Connection refused" when curling** — The HTTP server isn't running. Either enable
auto-start in mod settings, or place an HTTP Adapter building in-game.

**Mod doesn't appear in mod list** — Make sure the build output landed in the right
mods folder. Check `<GameModsFolder>\MoreHttpApi\version-1.0\` has a `manifest.json`
and a `.dll` file.

**Game crashes on load** — Check the Timberborn log at
`%APPDATA%\..\LocalLow\Mechanistry\Timberborn\Player.log` for the error. Most likely
a type mismatch in one of the new handlers — see the "build errors" section above.
