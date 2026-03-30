namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class TimeHandler(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService
) : IMoreHttpApiHandler
{
    public string Endpoint => "time";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => await context.HandleAsync(GetTimeAsync);

    async Task<HttpTimeInfo> GetTimeAsync()
    {
        var hours = dayNightCycle.HoursPassedToday;
        var hourInt = (int)hours;
        var minutes = (int)((hours - MathF.Floor(hours)) * 60);
        var formatted = $"{hourInt:D2}:{minutes:D2}";

        var daytimeLength = dayNightCycle.DaytimeLengthInHours;
        var isDaytime = hours < daytimeLength;

        return new(
            dayNightCycle.PartialDayNumber,
            (int)dayNightCycle.PartialDayNumber,
            hours,
            daytimeLength,
            dayNightCycle.NighttimeLengthInHours,
            isDaytime,
            formatted,
            gameCycleService.Cycle,
            gameCycleService.CycleDay
        );
    }
}
