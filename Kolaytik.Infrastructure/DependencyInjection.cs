using Kolaytik.Core.Interfaces.Repositories;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Kolaytik.Infrastructure.Repositories;
using Kolaytik.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kolaytik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddMemoryCache();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IListService, ListService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IWidgetService, WidgetService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
