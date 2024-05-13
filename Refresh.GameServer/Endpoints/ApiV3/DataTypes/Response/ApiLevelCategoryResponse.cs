using Bunkum.Core;
using Bunkum.Core.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiLevelCategoryResponse : IApiResponse, IDataConvertableFrom<ApiLevelCategoryResponse, LevelCategory>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconHash { get; set; }
    public required string FontAwesomeIcon { get; set; }
    public required string ApiRoute { get; set; }
    public required bool RequiresUser { get; set; }
    public required ApiGameLevelResponse? PreviewLevel { get; set; }
    public required bool Hidden { get; set; } = false;
    
    public static ApiLevelCategoryResponse? FromOld(LevelCategory? old, GameLevel? previewLevel,
        DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiLevelCategoryResponse
        {
            Name = old.Name,
            Description = old.Description,
            IconHash = old.IconHash,
            FontAwesomeIcon = old.FontAwesomeIcon,
            ApiRoute = old.ApiRoute,
            RequiresUser = old.RequiresUser,
            PreviewLevel = ApiGameLevelResponse.FromOld(previewLevel, dataContext),
            Hidden = old.Hidden,
        };
    }

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore)
    {
        this.PreviewLevel?.FillInExtraData(database, dataStore);
    }
    
    public static ApiLevelCategoryResponse? FromOldWithExtraData(LevelCategory? old, GameDatabaseContext database,
        IDataStore dataStore, GameLevel? previewLevel, DataContext dataContext)
    {
        if (old == null) return null;

        ApiLevelCategoryResponse response = FromOld(old, previewLevel, dataContext)!;
        response.FillInExtraData(database, dataStore);

        return response;
    }
    
    public static ApiLevelCategoryResponse? FromOld(LevelCategory? old, DataContext dataContext) => FromOld(old, null);

    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<LevelCategory> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    public static IEnumerable<ApiLevelCategoryResponse> FromOldListWithExtraData(IEnumerable<LevelCategory> oldList,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext) 
        => oldList.Select(old => FromOldWithExtraData(old, database, dataStore, null, dataContext)).ToList()!;
    
    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<LevelCategory> oldList,
        RequestContext context,
        MatchService matchService,
        GameDatabaseContext database,
        GameUser? user, DataContext dataContext)
    {
        return oldList.Select(category =>
        {
            DatabaseList<GameLevel>? list = category.Fetch(context, 0, 1, matchService, database, user, new LevelFilterSettings(context, TokenGame.Website), user);
            GameLevel? level = list?.Items.FirstOrDefault();
            
            return FromOld(category, level, dataContext);
        }).ToList()!;
    }

    public static IEnumerable<ApiLevelCategoryResponse> FromOldListWithExtraData(IEnumerable<LevelCategory> oldList,
        RequestContext context,
        MatchService matchService,
        GameDatabaseContext database,
        IDataStore dataStore,
        GameUser? user, DataContext dataContext)
    {
        IEnumerable<ApiLevelCategoryResponse> list = FromOldList(oldList, context, matchService, database, user, dataContext).ToList();

        foreach (ApiLevelCategoryResponse category in list)
        {
            category.FillInExtraData(database, dataStore);
        }
        
        return list;
    }
}