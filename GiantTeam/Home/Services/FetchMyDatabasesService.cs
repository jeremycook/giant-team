using GiantTeam.Home.Data;
using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Home.Services;

public class FetchMyDatabasesService
{
    private readonly HomeDbContext homeDbContext;

    public FetchMyDatabasesService(HomeDbContext homeDbContext)
    {
        this.homeDbContext = homeDbContext;
    }

    public async Task<MySchema.Database[]> FetchMyDatabasesAsync()
    {
        return await homeDbContext.My.Databases
            .OrderBy(o => o.Name)
            .ToArrayAsync();
    }
}