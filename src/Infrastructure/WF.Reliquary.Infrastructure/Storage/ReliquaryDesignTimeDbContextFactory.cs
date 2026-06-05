using Microsoft.EntityFrameworkCore.Design;

namespace WF.Reliquary.Infrastructure.Storage;

internal sealed class ReliquaryDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReliquaryDbContext>
{
    public ReliquaryDbContext CreateDbContext(string[] args)
    {
        return new ReliquaryDbContext(ReliquaryDbContext.BuildOptions("design-time.db"));
    }
}
