namespace MoreHttpApi.Shared;

public record HttpGoodSummary(
    string GoodId,
    int Amount
);

public record HttpDistrictResources(
    Guid DistrictId,
    string DistrictName,
    HttpGoodSummary[] Goods
);

public record HttpResourceInfo(
    HttpGoodSummary[] TotalGoods,
    HttpDistrictResources[] ByDistrict
);
