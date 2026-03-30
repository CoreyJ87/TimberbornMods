namespace MoreHttpApi.Shared;

public record HttpWeatherInfo(
    bool IsHazardousWeather,
    string? HazardousWeatherType,
    int Cycle,
    int CycleDay,
    int TotalCycleDays,
    int HazardousWeatherStartDay,
    int TemperateWeatherDuration,
    int HazardousWeatherDuration,
    float WindStrength
);
