using System.Text.Json;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kolaytik.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Core.Entities.List> Lists => Set<Core.Entities.List>();
    public DbSet<ListItem> ListItems => Set<ListItem>();
    public DbSet<ListItemRelation> ListItemRelations => Set<ListItemRelation>();
    public DbSet<WidgetConfig> WidgetConfigs => Set<WidgetConfig>();
    public DbSet<WidgetConfigLevel> WidgetConfigLevels => Set<WidgetConfigLevel>();
    public DbSet<EntityTranslation> EntityTranslations => Set<EntityTranslation>();
    public DbSet<LocalizationRecord> LocalizationRecords => Set<LocalizationRecord>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<WidgetEvent> WidgetEvents => Set<WidgetEvent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete global filter — BaseEntity türevleri için
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Branch>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApiKey>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Core.Entities.List>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ListItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WidgetConfig>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Ticket>().HasQueryFilter(e => !e.IsDeleted);

        // Soft delete filter — bağımlı (join/log) tablolar için eşleşen filtreler
        // EF Core uyarısını giderir: required end of relationship must have matching filter
        modelBuilder.Entity<ListItemRelation>()
            .HasQueryFilter(e => !e.ParentItem.IsDeleted && !e.ChildItem.IsDeleted);
        modelBuilder.Entity<UserBranch>()
            .HasQueryFilter(e => !e.Branch.IsDeleted && !e.User.IsDeleted);
        modelBuilder.Entity<WidgetConfigLevel>()
            .HasQueryFilter(e => !e.List.IsDeleted);
        modelBuilder.Entity<WidgetEvent>()
            .HasQueryFilter(e => !e.ApiKey.IsDeleted);
        modelBuilder.Entity<TicketMessage>()
            .HasQueryFilter(e => !e.Sender.IsDeleted);
        modelBuilder.Entity<RefreshToken>()
            .HasQueryFilter(e => !e.User.IsDeleted);

        // Enum → string dönüşümleri
        modelBuilder.Entity<Tenant>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(e => e.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<WidgetConfigLevel>()
            .Property(e => e.ElementType)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(e => e.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<WidgetEvent>()
            .Property(e => e.EventType)
            .HasConversion<string>();

        modelBuilder.Entity<AuditLog>()
            .Property(e => e.Action)
            .HasConversion<string>();

        // Unique kısıtlamalar
        modelBuilder.Entity<User>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<ApiKey>()
            .HasIndex(e => e.KeyHash)
            .IsUnique();

        // Bileşik PK — UserBranch
        modelBuilder.Entity<UserBranch>()
            .HasKey(e => new { e.UserId, e.BranchId });

        // ApiKey.AllowedDomains → jsonb (ValueComparer ile değişiklik takibi)
        var domainsComparer = new ValueComparer<string[]?>(
            (a, b) => a == null && b == null || (a != null && b != null && a.SequenceEqual(b)),
            v => v == null ? 0 : v.Aggregate(0, (acc, s) => HashCode.Combine(acc, s.GetHashCode())),
            v => v == null ? null : v.ToArray());

        modelBuilder.Entity<ApiKey>()
            .Property(e => e.AllowedDomains)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null),
                domainsComparer);

        // ListItem.Metadata → jsonb
        modelBuilder.Entity<ListItem>()
            .Property(e => e.Metadata)
            .HasColumnType("jsonb");

        // AuditLog JSON alanları → jsonb
        modelBuilder.Entity<AuditLog>()
            .Property(e => e.OldValues)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AuditLog>()
            .Property(e => e.NewValues)
            .HasColumnType("jsonb");

        // ListItemRelation ilişkileri (self-referencing, cascade off)
        modelBuilder.Entity<ListItemRelation>()
            .HasOne(e => e.ParentItem)
            .WithMany(e => e.ParentRelations)
            .HasForeignKey(e => e.ParentItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ListItemRelation>()
            .HasOne(e => e.ChildItem)
            .WithMany(e => e.ChildRelations)
            .HasForeignKey(e => e.ChildItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // List.Creator FK
        modelBuilder.Entity<Core.Entities.List>()
            .HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // ListItem.Creator FK
        modelBuilder.Entity<ListItem>()
            .HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket.Creator FK
        modelBuilder.Entity<Ticket>()
            .HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Slug için index
        modelBuilder.Entity<Core.Entities.List>()
            .HasIndex(e => new { e.TenantId, e.Slug })
            .IsUnique();

        // RefreshToken
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(e => e.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
