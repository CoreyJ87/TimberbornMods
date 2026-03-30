namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class WeatherHandler(
    WeatherService weatherService,
    HazardousWeatherService hazardousWeatherService,
    TemperateWeatherDurationService temperateWeatherDurationService,
    WindService windService,
    GameCycleService gameCycleService
) : IMoreHttpApiHandler
{
    public string Endpoint => "weather";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => await context.HandleAsync(GetWeatherAsync);

    async Task<HttpWeatherInfo> GetWeatherAsync()
    {
        var isHazardous = weatherService.IsHazardousWeather;
        string? hazardousType = null;
        if (isHazardous)
        {
            hazardousType = hazardousWeatherService.CurrentCycleHazardousWeather?.Id;
        }

        var cycle = gameCycleService.Cycle;
        var cycleDay = gameCycleService.CycleDay;
        var temperateDuration = temperateWeatherDurationService.TemperateWeatherDuration;
        var hazardousDuration = hazardousWeatherService.HazardousWeatherDuration;
        var totalDays = temperateDuration + hazardousDuration;
        var hazardousStart = weatherService.HazardousWeatherStartCycleDay;

        var windStrength = windService.WindStrength;

        return new(
            isHazardous,
            hazardousType,
            cycle,
            cycleDay,
            totalDays,
            hazardousStart,
            temperateDuration,
            hazardousDuration,
            windStrength
        );
    }
}
