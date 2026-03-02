# Kolaytik Liste Yönetim Servisi — Proje Rehberi

## Genel Bakış

Kolaytik, JotForm white-label altyapısı üzerine kurulu bir SaaS platformdur. Bu servis; müşterilerin JotForm formlarında kullanmak üzere kendi verilerini (şubeler, departmanlar, çalışanlar, doktorlar vb.) yönetebilecekleri bir **liste yönetim sistemi** ve bu verileri formlar üzerinde sunan bir **JotForm widget'ı** eklemektedir.

---

## Tech Stack

| Katman | Teknoloji |
|---|---|
| Backend | .NET 8 Web API |
| Panel (Frontend) | Blazor WebAssembly + MudBlazor |
| Widget | Vanilla JS / HTML |
| Veritabanı | PostgreSQL + Entity Framework Core 8 |
| Kimlik Doğrulama | JWT + 2FA (TOTP) |
| Email | MailKit (SMTP) |
| Loglama | Serilog |
| Deployment | VPS — Docker Compose |

---

## Solution Yapısı

```
Kolaytik.sln
├── Kolaytik.API/             # Web API, endpoint'ler, middleware, filters
├── Kolaytik.Core/            # Entity'ler, interface'ler, DTO'lar, enums
├── Kolaytik.Infrastructure/  # DbContext, repository implementasyonları, EF Core
└── Kolaytik.Blazor/          # Blazor WebAssembly panel
```

---

## Komutlar

```bash
dotnet build
dotnet run --project Kolaytik.API

# Migration
dotnet ef migrations add <n> --project Kolaytik.Infrastructure --startup-project Kolaytik.API
dotnet ef database update --project Kolaytik.Infrastructure --startup-project Kolaytik.API
```

---

## Mimari Prensipler

- **Repository Pattern:** `Kolaytik.Infrastructure/Repositories/` (interface + implementation)
- **Service Layer:** `Kolaytik.Core/Services/` (interface) + `Kolaytik.Infrastructure/Services/` (implementation)
- **DI Lifetimes:** Scoped (repo & service), Singleton (rate limiter, cache)
- **Soft Delete:** Hiçbir kayıt fiziksel olarak silinmez, tüm entity'lerde `IsDeleted` + `DeletedAt` alanı bulunur
- **Audit Log:** Tüm create/update işlemlerinde `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` alanları tutulur
- **Dil:** Türkçe UI, İngilizce kod (method/variable isimleri)

---

## Kullanıcı Hiyerarşisi ve Roller

```
SuperAdmin (Kolaytik ekibi)
└── Admin (Kolaytik ekibi)
    └── TenantAdmin (Müşteri firma sahibi/yöneticisi)
        └── BranchManager (Şube yöneticisi)
            └── BranchUser (Şubeye erişimi olan kullanıcı)
```

### Rol Yetki Matrisi

| Yetki | SuperAdmin | Admin | TenantAdmin | BranchManager | BranchUser |
|---|---|---|---|---|---|
| Tenant CRUD | ✅ | ✅ | ❌ | ❌ | ❌ |
| Branch CRUD | ✅ | ✅ | ✅ | ❌ | ❌ |
| Kullanıcı yönetimi | ✅ | ✅ | ✅ (kendi tenant) | ✅ (kendi şube) | ❌ |
| Tenant listesi CRUD | ✅ | ✅ | ✅ | ❌ | ❌ |
| Branch listesi CRUD | ✅ | ✅ | ✅ | ✅ | ❌ |
| Liste okuma | ✅ | ✅ | ✅ | ✅ | ✅ |
| Liste eleman ekleme/düzenleme | ✅ | ✅ | ✅ | ✅ | ✅ |
| Liste eleman silme | ✅ | ✅ | ✅ | ✅ | ❌ |
| Widget config yönetimi | ✅ | ✅ | ✅ | ❌ | ❌ |
| Raporlama | ✅ | ✅ | ❌ | ❌ | ❌ |
| 2FA zorunlu | ✅ | ✅ | ❌ | ❌ | ❌ |

> BranchUser birden fazla şubeye atanabilir (UserBranches tablosu).
> 2FA: SuperAdmin ve Admin için zorunlu, diğerleri için opsiyonel.
> Rol hiyerarşisi zorunlu değildir. Üst rol, alt rollerin tüm yetkilerini kapsar. Küçük firmalarda sadece TenantAdmin açmak yeterlidir, ihtiyaç duydukça BranchManager ve BranchUser eklenebilir.

---

## Veri Hiyerarşisi

```
Tenant (Firma)
├── Branch (Şube) — OPSİYONEL
│   ├── Lists
│   │   └── ListItems
│   └── ...
└── Lists (branch_id = null → firma geneli)
    └── ListItems
```

- Branch zorunlu değildir. Tek lokasyonlu firmalar doğrudan tenant altında liste oluşturabilir.
- `branch_id = null` olan listeler tenant geneline aittir.

---

## Veritabanı Şeması

```sql
Sectors                    -- admin panelinden yönetilen sektör listesi
  id uuid PK
  name varchar
  is_active bool

Tenants
  id uuid PK
  name varchar              -- firma adı
  sector_id uuid FK -> Sectors
  tax_number varchar nullable
  address varchar nullable
  authorized_name varchar nullable   -- yetkili kişi adı
  phone varchar nullable
  email varchar nullable
  status varchar            -- Active | Passive | Suspended
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool
  deleted_at timestamptz

Branches
  id uuid PK
  tenant_id uuid FK -> Tenants
  name varchar
  is_active bool
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool
  deleted_at timestamptz

Users
  id uuid PK
  email varchar UNIQUE
  password_hash varchar
  role varchar              -- SuperAdmin | Admin | TenantAdmin | BranchManager | BranchUser
  tenant_id uuid FK nullable -> Tenants
  is_2fa_enabled bool
  two_factor_secret varchar nullable
  status varchar            -- Active | Passive | EmailNotVerified
  email_verified_at timestamptz nullable
  last_login_at timestamptz nullable
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool
  deleted_at timestamptz

UserBranches              -- many-to-many
  user_id uuid FK -> Users
  branch_id uuid FK -> Branches
  PRIMARY KEY (user_id, branch_id)

ApiKeys
  id uuid PK
  tenant_id uuid FK -> Tenants
  branch_id uuid FK nullable -> Branches   -- null = tenant key, dolu = branch key
  name varchar
  key_hash varchar UNIQUE                  -- plain key saklanmaz, SHA-256 hash'i saklanır
  key_prefix varchar                       -- key'in ilk 8 karakteri, panelde gösterim için
  is_active bool
  rate_limit_per_minute int                -- varsayılan 60
  rate_limit_per_day int                   -- varsayılan 10000
  allowed_domains jsonb nullable           -- ["jotform.com", "firma.com"] boşsa tüm domainler izinli
  created_at timestamptz
  last_used_at timestamptz nullable
  is_deleted bool
  deleted_at timestamptz

Lists
  id uuid PK
  tenant_id uuid FK -> Tenants
  branch_id uuid FK nullable -> Branches   -- null = tenant geneli
  name varchar
  slug varchar
  description varchar nullable
  created_by uuid FK -> Users
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool
  deleted_at timestamptz

ListItems
  id uuid PK
  list_id uuid FK -> Lists
  tenant_id uuid FK -> Tenants             -- hızlı sorgu için denormalize
  label varchar
  value varchar
  metadata jsonb nullable                  -- ekstra alanlar (telefon, adres, kod vs.)
  order_index int
  is_active bool
  created_by uuid FK -> Users
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool
  deleted_at timestamptz

ListItemRelations          -- generic parent-child ilişkisi (sınırsız seviye cascade)
  id uuid PK
  parent_item_id uuid FK -> ListItems
  child_item_id uuid FK -> ListItems
  created_at timestamptz

WidgetConfigs              -- kayıtlı widget konfigürasyonları
  id uuid PK
  tenant_id uuid FK -> Tenants
  name varchar              -- "Şube-Çalışan Seçici" gibi
  width varchar             -- "25%" | "50%" | "75%" | "100%"
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool

WidgetConfigLevels         -- widget'ın her seviyesi ayrı satır
  id uuid PK
  widget_config_id uuid FK -> WidgetConfigs
  order_index int           -- 1, 2, 3... seviye sırası (sınırsız)
  list_id uuid FK -> Lists
  element_type varchar      -- Dropdown | RadioButton | CheckboxGroup | MultiSelectDropdown
  label varchar
  placeholder varchar nullable
  is_required bool
  max_selections int nullable   -- sadece çoklu seçim tipleri için

EntityTranslations         -- List ve ListItem çeviri desteği
  id uuid PK
  entity_type varchar       -- "List" | "ListItem"
  entity_id uuid
  field_name varchar        -- "name" (List için) | "label" (ListItem için)
  language_code varchar     -- "en-US" (Türkçe entity'nin kendisinde saklanır)
  value varchar
  created_at timestamptz
  updated_at timestamptz

LocalizationRecords        -- Panel UI metin override'ları (admin panelinden yönetilir)
  id uuid PK
  key varchar
  language_code varchar     -- "tr-TR" | "en-US"
  value varchar
  created_at timestamptz
  updated_at timestamptz

Tickets                    -- geri bildirim / talep sistemi
  id uuid PK
  tenant_id uuid FK -> Tenants
  created_by uuid FK -> Users
  subject varchar
  status varchar            -- Open | InProgress | Resolved | Closed
  priority varchar          -- Low | Medium | High
  created_at timestamptz
  updated_at timestamptz
  is_deleted bool

TicketMessages             -- ticket içindeki mesajlar
  id uuid PK
  ticket_id uuid FK -> Tickets
  sender_id uuid FK -> Users
  message text
  created_at timestamptz

WidgetEvents               -- raporlama için widget kullanım logları
  id uuid PK
  api_key_id uuid FK -> ApiKeys
  tenant_id uuid FK -> Tenants
  widget_config_id uuid FK nullable -> WidgetConfigs
  list_id uuid FK -> Lists
  event_type varchar        -- "loaded" | "selected"
  selected_item_id uuid nullable FK -> ListItems
  ip_address varchar
  created_at timestamptz

AuditLogs
  id uuid PK
  user_id uuid FK nullable -> Users
  entity_type varchar
  entity_id uuid
  action varchar            -- Created | Updated | Deleted
  old_values jsonb nullable
  new_values jsonb nullable
  ip_address varchar
  created_at timestamptz
```

---

## Lokalizasyon Mimarisi

### 3 Katmanlı Çeviri Sistemi (FormSepeti pattern'i)

```
DB (LocalizationRecords) -> .resx dosyaları (bellekte) -> ResourceManager
```

### UI String Çevirileri
- Dosyalar: `Kolaytik.Blazor/Resources/SharedResource.tr-TR.resx`, `SharedResource.en-US.resx`
- DB Override: `LocalizationRecords` tablosu (admin panelinden yönetilebilir)
- Fallback zinciri: DB -> .resx -> ResourceManager
- Cache: 5 dk (MemoryCache)

### Data Çevirileri (Entity)
- Tablo: `EntityTranslations` (EntityType + EntityId + FieldName + LanguageCode)
- Desteklenen entity'ler: `List`, `ListItem`
- Türkçe: entity'nin kendi alanında saklı, DB lookup atlanır
- Cache: 10 dk (MemoryCache)

### Desteklenen Diller
- `tr-TR` (varsayılan), `en-US`

---

## API Key Yapısı

- **Tenant Key** (`branch_id = null`): Firma geneline ait. Widget'a girilince firmanın tüm listeleri erişilebilir.
- **Branch Key** (`branch_id = dolu`): Şubeye özel. Widget'a girilince yalnızca o şubenin listeleri görünür.

### Key Güvenliği
- Key oluşturulduğunda 256-bit random string üretilir, kullanıcıya bir kez gösterilir
- DB'de plain text saklanmaz, SHA-256 hash'i tutulur
- Panelde sadece `key_prefix` (ilk 8 karakter) gösterilir — örn. `a3f9bc12...`
- Key rotasyonu: müşteri istediğinde yeni key üretilir, eskisi iptal edilir
- Key'e opsiyonel domain whitelist bağlanabilir (`allowed_domains`)
- Key'e özel rate limit tanımlanabilir (`rate_limit_per_minute`, `rate_limit_per_day`), varsayılan 60/dk, 10000/gün

---

## Auth Yapısı

- JWT Bearer token
- Token süresi: 8 saat (user), 4 saat (admin rolleri)
- Refresh token desteği
- Login: E-mail + şifre
- 2FA: TOTP (Google Authenticator uyumlu) — SuperAdmin ve Admin için zorunlu, diğerleri için opsiyonel
- Rate limiting: 5 başarısız giriş -> 5 dakika kilitleme

### Tenant Durum Yönetimi
- Tenant `Passive` veya `Suspended` olduğunda tüm kullanıcı girişleri engellenir
- Widget endpoint'leri pasif tenant'a cevap vermez
- Kullanıcı seviyesinde de ayrıca `Active | Passive | EmailNotVerified` durumu vardır

---

## Widget Çalışma Mantığı

### Sınırsız Seviye Cascade Yapısı

Widget konfigürasyonu `WidgetConfigs` + `WidgetConfigLevels` tablolarında saklanır. Seviye sayısı sınırsız, her seviye bir öncekinin seçimine göre `ListItemRelations` üzerinden filtrelenir.

### Konfigürasyon (Form tasarım ekranında)

```
API Key: [xxx]

Seviye 1: [Şehir]     Tip: [Dropdown]        Etiket: "Şehir Seçiniz"     Zorunlu: Evet
Seviye 2: [İlçe]      Tip: [Dropdown]        Etiket: "İlçe Seçiniz"      Zorunlu: Evet
Seviye 3: [Mahalle]   Tip: [CheckboxGroup]   Etiket: "Mahalle Seçiniz"   Zorunlu: Hayır
                                              Maks. Seçim: 3

[+ Seviye Ekle]

Genişlik: [50%]
```

### Element Tipleri
- `Dropdown` — tekli seçim, placeholder gösterilir
- `RadioButton` — tekli seçim
- `CheckboxGroup` — çoklu seçim, max_selections uygulanır
- `MultiSelectDropdown` — çoklu seçim, placeholder gösterilir, max_selections uygulanır

### Widget Public Endpoint'leri

```
GET /api/widget/config?api_key=xxx&config_id=yyy
GET /api/widget/items?api_key=xxx&list_id=yyy
GET /api/widget/items?api_key=xxx&list_id=yyy&parent_item_id=zzz
```

Widget endpoint'leri JWT gerektirmez, sadece `api_key` ile çalışır.

**Güvenlik Katmanları:**
- API key plain text saklanmaz, SHA-256 hash'i DB'de tutulur. İstek geldiğinde hash'lenerek karşılaştırılır.
- Her istekte tenant scope kontrolü zorunludur — key hangi tenant'a aitse sadece o tenant'ın verisi döner.
- Tenant `Passive` veya `Suspended` ise, API key iptal edildiyse anında 403 döner.
- `allowed_domains` tanımlıysa `Origin` header kontrol edilir, whitelist dışı domainlerden gelen istekler engellenir.
- API key bazında `rate_limit_per_minute` ve `rate_limit_per_day` limitleri uygulanır.
- Sayfalama zorunludur, tek seferde maksimum 100 eleman döner.
- Başarısız / şüpheli istekler `WidgetEvents` tablosuna loglanır.
- Key brute force koruması: kısa sürede çok sayıda geçersiz key denemesi yapan IP geçici olarak engellenir.

---

## ListItemRelations — Generic İlişki Yapısı

İki liste arasındaki ilişki tamamen generic'tir. Sistem A ve B listelerinin ne olduğunu bilmek zorunda değildir. Aynı tablo üzerinden sınırsız seviye cascade zinciri kurulabilir.

Örnekler: Şehir -> İlçe -> Mahalle, Firma -> Departman -> Ekip -> Çalışan

İlişki kurulmak zorunda değildir — kurulmamışsa widget tek dropdown olarak çalışır.

---

## Ticket Sistemi

- Tenant kullanıcıları ticket oluşturur
- Durum: `Open | InProgress | Resolved | Closed`
- Öncelik: `Low | Medium | High`
- Her iki taraf (tenant + Kolaytik admin) yorum ekleyebilir
- Durum değişikliklerinde e-mail bildirimi

---

## Raporlama (Sadece SuperAdmin / Admin)

### Dashboard
- Toplam tenant sayısı, aktif/pasif/askıda dağılımı
- Toplam liste, eleman, widget event sayısı
- Son 30 günde yeni kayıt olan tenantlar
- Sektör bazında müşteri dağılımı

### Tenant Detay Raporu
- Liste ve eleman sayısı
- Widget yüklenme ve seçim sayısı
- En çok kullanılan listeler ve elemanlar
- API key kullanım istatistikleri
- Son aktivite tarihleri

### Widget Kullanım Raporu
- Günlük/haftalık/aylık yüklenme sayısı
- Liste bazında kullanım dağılımı
- En çok seçilen elemanlar

### Ticket Raporu
- Açık/çözülen ticket sayısı
- Ortalama çözüm süresi
- Tenant bazında ticket dağılımı

---

## Diğer Özellikler

| Özellik | Açıklama |
|---|---|
| Soft Delete | Tüm entity'lerde `IsDeleted` + `DeletedAt` |
| Audit Log | `AuditLogs` tablosu, tüm CRUD işlemlerini kaydeder |
| Excel/CSV Import | Liste elemanları toplu olarak import edilebilir |
| Widget Arama | Uzun listelerde arama/filtreleme desteği |
| Rate Limiting | Widget endpoint'leri + login koruması |
| Metadata | `ListItems.metadata` jsonb — eleman bazında ekstra alanlar |

---

## Backend Conventions

- **Validation:** FluentValidation — tüm request DTO'ları validate edilir
- **Error handling:** Global exception middleware, standart `ApiResponse<T>` wrapper
- **Input sanitization:** Tüm string input'lar sanitize edilir
- **CORS:** Sadece Blazor paneli ve widget origin'leri whitelist'te
- **Sensitive data:** `appsettings.Development.json` gitignore'da, connection string placeholder olarak commit edilir
- **Custom Validation Attributes:** `[NoXss]`, `[NoSqlInjection]`, `[StrongPassword]`

---

## Frontend (Blazor) Conventions

- **Component kütüphanesi:** MudBlazor
- **HTTP:** `HttpClient` factory pattern
- **Auth state:** Custom `AuthenticationStateProvider`
- **Tema:** MudBlazor theme ile marka renkleri tanımlanır
- **Icon library:** MudBlazor Icons
- **Lokalizasyon:** `IStringLocalizer<T>` + `.resx` dosyaları + DB override

---

## Docker Compose Yapısı

```yaml
services:
  api:        # Kolaytik.API
  blazor:     # Kolaytik.Blazor (nginx static)
  postgres:   # PostgreSQL
  nginx:      # Reverse proxy
```

### Production pg_dump (Yedekleme)

```json
"DatabaseBackup": {
  "PgDumpPath": "/usr/bin/pg_dump",
  "BackupPath": "/var/backups/kolaytik",
  "MaxBackupCount": 10
}
```

---

## Sonraki Adımlar

1. .NET solution yapısının oluşturulması
2. EF Core entity'leri ve DB migration'ları
3. Auth sistemi (JWT + 2FA)
4. Liste CRUD API endpoint'leri
5. Widget public endpoint'leri + rate limiting
6. Blazor panel (login, liste yönetimi)
7. JotForm widget (Vanilla JS)
8. Docker Compose kurulumu
