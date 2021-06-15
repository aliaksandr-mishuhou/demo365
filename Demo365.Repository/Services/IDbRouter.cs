namespace Demo365.Repository.Services
{
    public interface IDbRouter
    {
        string GetConnectionString(DbRouterSettings settings);
    }
}
