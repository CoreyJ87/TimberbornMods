namespace MoreHttpApi.Shared;

public record HttpTimeInfo(
    float PartialDayNumber,
    int TotalDayNumber,
    float HoursPassedToday,
    float DaytimeLengthInHours,
    float NighttimeLengthInHours,
    bool IsDaytime,
    string FormattedTime,
    int Cycle,
    int CycleDay
);
