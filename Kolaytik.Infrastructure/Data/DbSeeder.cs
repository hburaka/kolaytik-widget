using BCrypt.Net;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kolaytik.Infrastructure.Data;

/// <summary>
/// Development ortamı için örnek veri oluşturur. İdempotent — tekrar çalıştırılabilir.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        await SeedSectorsAsync(db);
        var superAdmin = await SeedSuperAdminAsync(db);
        await SeedDemoTenantAsync(db, superAdmin.Id);

        await db.SaveChangesAsync();
    }

    // ── Sektörler ─────────────────────────────────────────────────────────────

    private static async Task SeedSectorsAsync(ApplicationDbContext db)
    {
        if (await db.Sectors.AnyAsync()) return;

        db.Sectors.AddRange(
            new Sector { Name = "Teknoloji" },
            new Sector { Name = "Sağlık" },
            new Sector { Name = "Perakende" },
            new Sector { Name = "Eğitim" },
            new Sector { Name = "Lojistik" },
            new Sector { Name = "Finans" },
            new Sector { Name = "Diğer" }
        );

        await db.SaveChangesAsync();
    }

    // ── SuperAdmin ────────────────────────────────────────────────────────────

    private static async Task<User> SeedSuperAdminAsync(ApplicationDbContext db)
    {
        var existing = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "admin@kolaytik.com");

        if (existing is not null) return existing;

        var admin = new User
        {
            Email           = "admin@kolaytik.com",
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role            = UserRole.SuperAdmin,
            Status          = UserStatus.Active,
            EmailVerifiedAt = DateTime.UtcNow
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
        return admin;
    }

    // ── Demo Tenant + TenantAdmin + Branch + Listeler ─────────────────────────

    private static async Task SeedDemoTenantAsync(ApplicationDbContext db, Guid creatorId)
    {
        if (await db.Tenants.AnyAsync()) return;

        var sector = await db.Sectors.FirstAsync(s => s.Name == "Teknoloji");

        // Tenant
        var tenant = new Tenant
        {
            Name           = "Demo Firma A.Ş.",
            SectorId       = sector.Id,
            AuthorizedName = "Ahmet Yönetici",
            Email          = "info@demofirma.com",
            Phone          = "+90 212 000 0000",
            Status         = TenantStatus.Active
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        // TenantAdmin
        var tenantAdmin = new User
        {
            Email           = "tenant@kolaytik.com",
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword("Tenant123!"),
            Role            = UserRole.TenantAdmin,
            TenantId        = tenant.Id,
            Status          = UserStatus.Active,
            EmailVerifiedAt = DateTime.UtcNow
        };
        db.Users.Add(tenantAdmin);
        await db.SaveChangesAsync();

        // Branch
        var branch = new Branch
        {
            TenantId = tenant.Id,
            Name     = "İstanbul Merkez",
            IsActive = true
        };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        // Liste 1: Çalışanlar (tenant geneli)
        await SeedCalisanlarListesiAsync(db, tenant.Id, tenantAdmin.Id);

        // Liste 2 & 3: Şehirler → İlçeler (cascade)
        await SeedSehirIlceListesiAsync(db, tenant.Id, tenantAdmin.Id);
    }

    // ── Çalışanlar listesi ────────────────────────────────────────────────────

    private static async Task SeedCalisanlarListesiAsync(
        ApplicationDbContext db, Guid tenantId, Guid createdBy)
    {
        var list = new List
        {
            TenantId    = tenantId,
            Name        = "Çalışanlar",
            Slug        = "calisanlar",
            Description = "Firma çalışan listesi",
            CreatedBy   = createdBy
        };
        db.Lists.Add(list);
        await db.SaveChangesAsync();

        var employees = new[]
        {
            ("Ayşe Yılmaz",   "ayse.yilmaz"),
            ("Mehmet Demir",   "mehmet.demir"),
            ("Fatma Kaya",     "fatma.kaya"),
            ("Ali Çelik",      "ali.celik"),
            ("Zeynep Arslan",  "zeynep.arslan"),
            ("Emre Şahin",     "emre.sahin"),
            ("Elif Yıldız",    "elif.yildiz"),
            ("Hasan Kurt",     "hasan.kurt")
        };

        var items = employees.Select((e, i) => new ListItem
        {
            ListId     = list.Id,
            TenantId   = tenantId,
            Label      = e.Item1,
            Value      = e.Item2,
            OrderIndex = i + 1,
            CreatedBy  = createdBy
        }).ToList();

        db.ListItems.AddRange(items);
        await db.SaveChangesAsync();
    }

    // ── Şehirler → İlçeler cascade listesi ───────────────────────────────────

    private static async Task SeedSehirIlceListesiAsync(
        ApplicationDbContext db, Guid tenantId, Guid createdBy)
    {
        // Şehirler listesi
        var sehirList = new List
        {
            TenantId  = tenantId,
            Name      = "Şehirler",
            Slug      = "sehirler",
            CreatedBy = createdBy
        };
        db.Lists.Add(sehirList);

        // İlçeler listesi
        var ilceList = new List
        {
            TenantId  = tenantId,
            Name      = "İlçeler",
            Slug      = "ilceler",
            CreatedBy = createdBy
        };
        db.Lists.Add(ilceList);
        await db.SaveChangesAsync();

        // Şehir → İlçe verisi
        var data = new Dictionary<(string label, string value), string[]>
        {
            [("İstanbul", "istanbul")] = ["Kadıköy", "Beşiktaş", "Şişli", "Beyoğlu", "Üsküdar"],
            [("Ankara",   "ankara")]   = ["Çankaya", "Keçiören", "Mamak", "Etimesgut"],
            [("İzmir",    "izmir")]    = ["Konak", "Karşıyaka", "Bornova", "Buca"]
        };

        int sehirOrder = 1;
        int ilceOrder  = 1;

        foreach (var (sehir, ilceler) in data)
        {
            var sehirItem = new ListItem
            {
                ListId     = sehirList.Id,
                TenantId   = tenantId,
                Label      = sehir.label,
                Value      = sehir.value,
                OrderIndex = sehirOrder++,
                CreatedBy  = createdBy
            };
            db.ListItems.Add(sehirItem);
            await db.SaveChangesAsync();

            foreach (var ilce in ilceler)
            {
                var slug = ilce.ToLowerInvariant()
                    .Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
                    .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');

                var ilceItem = new ListItem
                {
                    ListId     = ilceList.Id,
                    TenantId   = tenantId,
                    Label      = ilce,
                    Value      = slug,
                    OrderIndex = ilceOrder++,
                    CreatedBy  = createdBy
                };
                db.ListItems.Add(ilceItem);
                await db.SaveChangesAsync();

                // Şehir → İlçe ilişkisi
                db.ListItemRelations.Add(new ListItemRelation
                {
                    ParentItemId = sehirItem.Id,
                    ChildItemId  = ilceItem.Id
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
