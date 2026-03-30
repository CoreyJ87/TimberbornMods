namespace MoreHttpApi.Shared;

public record HttpDistrict(
    Guid EntityId,
    string Name,
    int Adults,
    int Children,
    int Bots
);

public record HttpDistrictInfo(
    HttpDistrict[] Districts
);
