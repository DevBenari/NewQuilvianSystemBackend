# Aturan Struktur Backend

Aturan ini ditelusuri langsung dari source `NewQuilvianSystemBackend`. Blueprint **MUST**
menyebutkan lokasi file memakai pola di bawah, bukan menebak.

## 1. Peta lokasi file

| Jenis file | Lokasi | Contoh nyata |
| --- | --- | --- |
| Model transaksi (`Trx*`) | `Areas/<Domain>/<SubDomain>/Models/` | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` |
| Model master (`Mst*`) | `Areas/<Domain>/MasterData/Models/` | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` |
| Controller | `Areas/<Domain>/<SubDomain>/Controllers/` | `Areas/HealthServices/ClinicalManagement/Controllers/` |
| DTO | `Areas/<Domain>/<SubDomain>/DTOs/` | `.../EmergencyInstallationManagement/DTOs/EmergencyTriageDtos.cs` |
| Enum | `Areas/<Domain>/<SubDomain>/Enums/` | `.../EmergencyInstallationManagement/Enums/EmergencyTriageStatus.cs` |
| Service | `Areas/<Domain>/<SubDomain>/Services/` | `.../EmergencyInstallationManagement/Services/` |
| **EF Core Configuration** | **`Repositories/Configurations/<Domain>/<SubDomain>/`** | `Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyTriageConfiguration.cs` |
| Migration | `Migrations/` | `Migrations/<timestamp>_<NamaPerubahan>.cs` |

Dua hal yang paling sering salah:

1. **Configuration tidak berada di dalam `Areas/`.** Ia terpisah di bawah
   `Repositories/Configurations/`. Blueprint yang lupa menyebut ini membuat implementer
   menaruh file di tempat yang salah.
2. **Master tidak berada di dalam folder submodulnya.** `MstEmergency*` tinggal di
   `Areas/HealthServices/MasterData/Models/`, bukan di
   `Areas/HealthServices/EmergencyInstallationManagement/Models/`.

## 2. Penamaan

| Awalan | Arti | Contoh |
| --- | --- | --- |
| `Mst` | Data induk yang relatif stabil | `MstEmergencyTriageLevel`, `MstAllowanceType` |
| `Trx` | Data transaksi hasil aktivitas | `TrxEmergencyVisit`, `TrxAttendance` |
| `Wfp` | Transaksi payroll tenaga kerja | `WfpTransportAllowance` |

Nama file Configuration adalah nama entity ditambah akhiran `Configuration`, misalnya
`TrxEmergencyTriageConfiguration.cs`.

## 3. Base class audit — `IdentityModel`

Seluruh model mewarisi `QuilvianSystemBackend.Models.IdentityModel`, yang menyediakan:

```csharp
CreateDateTime, CreateBy, UpdateDateTime, UpdateBy,
DeleteDateTime, DeleteBy, CancelDateTime, CancelBy,
IsCancel, IsDelete
```

Konsekuensi untuk blueprint:

- Penghapusan bersifat penandaan (`IsDelete`), bukan penghapusan sungguhan. Desain **MUST NOT**
  mengandalkan baris benar-benar hilang dari tabel.
- Kesepuluh kolom itu **MUST NOT** diulang pada setiap tabel di kamus data. Cukup ditulis satu
  kali: *"Seluruh tabel mewarisi `IdentityModel`."*

## 4. Konvensi teknis lain

| Hal | Konvensi |
| --- | --- |
| Atribut tabel | `[Table("TrxEmergencyTriage", Schema = "public")]` |
| Pembungkus respons | `ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)` |
| Grup Swagger | `[Tags("Health Services / Emergency Installation Management / Emergency Triage")]` |
| Route | `api/v1/<domain-kebab>/<subdomain-kebab>/<resource-kebab>` |
| Hak akses | `[AccessController]` di kelas; `[AccessAction]` dan `[AccessPermission("Resource", "Action")]` di setiap endpoint |
| Service | Tanpa interface, didaftarkan `AddScoped<TService>()`, di-inject langsung ke constructor controller |
| Kapan memakai service | Hanya untuk aturan bisnis, transaksi database, atau perubahan status kompleks. CRUD sederhana boleh memakai `ApplicationDbContext` langsung di controller |
| Penghapusan relasi | `DeleteBehavior.Restrict` untuk relasi klinis, agar histori transaksi tidak ikut terhapus berantai |
| Query list | `AsNoTracking`, projection ke DTO, filter, dan pagination |
| Logging | Dicatat untuk Create, Update, workflow status, dan Delete. GET tidak dicatat. Payload log hanya `EntityId`, controller, action, status — **MUST NOT** memuat diagnosis, keluhan, atau data medis |

## 5. Pola standar dan utang teknis

Pola standar yang **MUST** dipakai modul baru:

| Aspek | Pola standar |
| --- | --- |
| Folder controller | `Controllers/` (jamak) |
| Nama domain | `HealthServices` (jamak), konsisten di `Areas/` maupun `Repositories/Configurations/` |
| Namespace | Mengikuti path folder, tanpa ruas tambahan |

Penyimpangan yang sudah ada di source saat aturan ini ditulis:

| Penyimpangan | Keadaan nyata | Pola standar |
| --- | --- | --- |
| Folder controller IGD | `Areas/HealthServices/EmergencyInstallationManagement/Controller/` (tunggal) | `Controllers/` |
| Nama domain di Configurations | `Repositories/Configurations/HealthService/` (tunggal) | `HealthServices` |
| Namespace master IGD | `...MasterData.EmergencyInstallationManagement.Models` padahal foldernya `MasterData/Models/` | Namespace mengikuti folder |

Cara menuliskannya di blueprint:

```text
Areas/HealthServices/EmergencyInstallationManagement/
└── Controllers/     # saat ini bernama Controller (tunggal) — utang teknis, jangan ditiru
```

Aturan penanganannya:

- Modul baru **MUST NOT** meniru penyimpangan ini.
- Implementer **MUST NOT** merapikan penyimpangan diam-diam di tengah task lain, karena itu
  menyentuh source di luar scope task dan berisiko memecah build.
- Perapian **MUST** menjadi task tersendiri pada roadmap, dengan approval pemilik arsitektur
  backend.

Sumber keputusan: `docs/agency/update-skills/03-revisi-design-business-module.md`, DEC-RSK-003.

