using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// Basis data SQLite di memori, dibentuk dari model aplikasi. Disalin dari harness Optik
// (Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/TestDatabase.cs).
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private TestDatabase(SqliteConnection connection) => _connection = connection;

    public static TestDatabase Create()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var db = new TestDatabase(connection);
        using (var ctx = db.CreateContext()) ctx.Database.EnsureCreated();
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = OFF;";
            pragma.ExecuteNonQuery();
        }
        return db;
    }

    public ApplicationDbContext CreateContext(params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, ModelUjiSqlite>();
        if (interceptors.Length > 0) builder.AddInterceptors(interceptors);
        return new ApplicationDbContext(builder.Options);
    }

    public void Dispose() => _connection.Dispose();
}

public sealed class ModelUjiSqlite(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    private static readonly string[] Khusus = ["num_nonnulls", "::", " ~ ", " ~* ", " !~ ", " !~* "];

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var e in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var n in e.GetDeclaredCheckConstraints().Where(x => KhasPostgres(x.Sql)).Select(x => x.ModelName).ToList())
                e.RemoveCheckConstraint(n);
            foreach (var p in e.GetDeclaredProperties())
            {
                if (KhasPostgres(p.GetDefaultValueSql())) p.SetDefaultValueSql(null);
                if (KhasPostgres(p.GetComputedColumnSql())) p.SetComputedColumnSql(null);
                if ((p.ClrType == typeof(DateTimeOffset) || p.ClrType == typeof(DateTimeOffset?)) && p.GetValueConverter() is null)
                    p.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter());
            }
            foreach (var i in e.GetDeclaredIndexes().Where(x => x.IsUnique && x.GetFilter() is not null).ToList())
                i.IsUnique = false;
        }
    }

    private static bool KhasPostgres(string? sql) => sql is not null && Khusus.Any(k => sql.Contains(k, StringComparison.OrdinalIgnoreCase));
}

// Penyimpan pengguna tiruan: peran selalu kosong; superadmin ditentukan lewat UserType.
public sealed class TanpaPeranStore(ApplicationDbContext ctx) : IUserRoleStore<ApplicationUser>
{
    public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult<IList<string>>(new List<string>());
    public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) => throw new NotSupportedException();
    public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) => throw new NotSupportedException();
    public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) => Task.FromResult(false);
    public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct) => throw new NotSupportedException();
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id.ToString());
    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) => throw new NotSupportedException();
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) => throw new NotSupportedException();
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => throw new NotSupportedException();
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => throw new NotSupportedException();
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => throw new NotSupportedException();
    public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => ctx.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId), ct);
    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => throw new NotSupportedException();
    public void Dispose() { }
}

public sealed class LingkunganUji : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "ScopeTest";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

// Pengganti dependensi yang tidak dipakai jalur cakupan. Setiap pemanggilan berarti uji salah sasaran.
public class Pengganti : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
        throw new InvalidOperationException($"Pengganti tidak boleh dipanggil: {targetMethod?.Name}");

    public static T Buat<T>() where T : class => Create<T, Pengganti>();
}

public static class Pelaku
{
    public static IHttpContextAccessor Sebagai(Guid? userId)
    {
        var accessor = new HttpContextAccessor();
        if (userId.HasValue)
        {
            var identity = new ClaimsIdentity(new[] { new Claim("user_id", userId.Value.ToString()) }, "Uji");
            accessor.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        }
        return accessor;
    }

    public static AccessPermissionService Izin(ApplicationDbContext ctx)
    {
        var userManager = new UserManager<ApplicationUser>(
            new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        return new AccessPermissionService(ctx, userManager, config, new LingkunganUji());
    }
}
