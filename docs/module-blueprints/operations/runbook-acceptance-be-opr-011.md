# Runbook acceptance `BE-OPR-011` — siap dijalankan

Seluruh investigasi sudah selesai. Berkas ini daftar perintah berurutan: begitu satu DDL di
bawah dijalankan, matriks `200`/`403`/`401` dapat dieksekusi tanpa mencari apa pun lagi.

| Hal | Isi |
|---|---|
| Disusun | 7 Oktober 2026 |
| Sudah terbukti runtime | **`401`** tanpa login, **`403`** login tanpa izin |
| Belum terbukti | **`200`** — terhalang satu kolom |
| Target | `localhost` / `QuilvianNewDevIkbalFr` |

## Langkah 0 — DDL, satu kali, oleh pemilik basis data

Ini satu-satunya langkah yang **tidak** dapat saya jalankan: upaya saya ditolak classifier sebagai
*Modify Shared Resources*, dan saya tidak mengakalinya.

Migration `20260901073655_A0AuthorizationIntegrityProjection` **hanya menyentuh satu tabel** —
seluruh 83 barisnya sudah diperiksa — sehingga penerapan manualnya lengkap, bukan separuh.

```sql
BEGIN;

DROP INDEX IF EXISTS public."IX_AspNetUserOrganization_UserId_DepartmentId_PositionId";
DROP INDEX IF EXISTS public."IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~";

ALTER TABLE public."AspNetUserOrganization"
    ADD COLUMN "SourceAssignmentId" uuid NULL;

CREATE UNIQUE INDEX "IX_AspNetUserOrganization_SourceAssignmentId"
    ON public."AspNetUserOrganization" ("SourceAssignmentId")
    WHERE "SourceAssignmentId" IS NOT NULL AND "IsDelete" = false;

CREATE UNIQUE INDEX "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~"
    ON public."AspNetUserOrganization"
       ("UserId", "DepartmentId", "PositionId", "EffectiveStartDate") NULLS NOT DISTINCT
    WHERE "IsDelete" = false;

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260901073655_A0AuthorizationIntegrityProjection', '9.0.18');

COMMIT;
```

Tiga hal yang mudah terlewat:

1. **Nama indeks kedua berakhiran `~`.** Itu pemotongan nama oleh EF, bukan salah tulis. Harus
   sama persis, kalau tidak EF menganggapnya indeks yang berbeda.
2. **`NULLS NOT DISTINCT`** adalah terjemahan dari `.Annotation("Npgsql:NullsDistinct", false)`
   pada migration. Tanpanya keunikannya berperilaku berbeda terhadap nilai kosong.
3. **Baris `INSERT` wajib.** Tanpa mencatatnya, `dotnet ef database update` suatu saat akan
   mencoba menerapkan migration ini lagi, gagal karena kolomnya sudah ada, dan **memblokir
   seluruh rantai migration**.

## Langkah 1 — Nyalakan aplikasi

```bash
ASPNETCORE_ENVIRONMENT=Development \
Seeders__ContinueOnFailure=true \
Security__Authorization__Enabled=true \
Security__Authorization__EnforceClinicalPolicyForSuperAdmin=true \
ASPNETCORE_URLS=http://127.0.0.1:5215 \
dotnet run --project QuilvianSystemBackend.csproj --no-build --no-launch-profile \
  -p:SkipMigrationMetadata=true
```

Keduanya **env var proses**, bukan perubahan `appsettings`. `ContinueOnFailure` dibutuhkan karena
basis data dev masih kekurangan 238 migration lain dan tanpa saklar itu startup mati di
`MstNursingDiagnosisSeeder`.

`EnforceClinicalPolicyForSuperAdmin` yang membuat buktinya sah: tanpanya SuperAdmin melewati
seluruh pemeriksaan izin dan `403` tidak akan pernah muncul. Dengan saklar itu, akun uji melewati
jalur keputusan yang sama persis seperti pengguna biasa.

## Langkah 2 — Nilai yang sudah diketahui

Tidak perlu dicari lagi; ketiganya sudah terverifikasi ada di basis data:

| Hal | Nilai |
|---|---|
| Departemen uji | `DEMO-OPR-DEPT` — "Kamar Operasi (Demo)" |
| Jabatan uji | `DEMO-OPR-POS` — "Perawat Kamar Operasi (Demo)" |
| Profil tenaga `superadmin` | `5a9c1f7f-4fb9-b990-fd72-abd6bb7fb7d1` |
| Organization assignment | sudah ada, bernomor **`UJI-OPR-011`**, dibuat 6 Oktober 2026 |

Baris `UJI-OPR-011` itu dibuat lewat endpoint resmi
`POST .../workforce-profiles/{id}/organization-assignments` dan **sudah menunggu** — rekonsiliasi
projection-nya gagal hanya karena kolom di Langkah 0. Begitu kolomnya ada, assignment itu dapat
direkonsiliasi ulang tanpa dibuat lagi.

## Langkah 3 — Rekonsiliasi projection izin

Panggil ulang endpoint assignment agar `OrganizationAuthorizationProjectionService` menulis baris
`AspNetUserOrganization`:

```bash
# login lebih dulu, simpan cookie
curl -s -c ck.txt -X POST http://127.0.0.1:5215/api/v1/Auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"<email superadmin>","password":"<password>"}'

# PUT assignment yang sudah ada akan memicu rekonsiliasi
curl -s -b ck.txt -X PUT \
  "http://127.0.0.1:5215/api/v1/corporate/human-resource/workforce-profiles/5a9c1f7f-4fb9-b990-fd72-abd6bb7fb7d1/organization-assignments/<idAssignment>" \
  -H 'Content-Type: application/json' -d '{...sama seperti saat dibuat...}'
```

**Verifikasi berhasil:** `select count(*) from "AspNetUserOrganization";` harus **≥ 1**, dan
barisnya memuat `DepartmentId` + `PositionId` milik `DEMO-OPR-DEPT`/`DEMO-OPR-POS`. Selama angkanya
masih `0`, jangan lanjut — seluruh langkah berikutnya akan menghasilkan `403` dan menyesatkan.

## Langkah 4 — Berikan izin SEBAGIAN, tahan sisanya

Inilah inti buktinya. Tanpa kontras, `403` tidak membedakan apa pun.

Ambil `ControllerAccessId` dan `ActionAccessId` untuk **`OperatingRoomCase:Read`**:

```sql
select a."Id" as "ActionAccessId", c."Id" as "ControllerAccessId"
from "SysActionAccess" a
join "SysControllerAccess" c on c."Id" = a."ControllerAccessId"
where c."ControllerName" = 'OperatingRoomCase' and a."ActionName" = 'Read'
  and a."IsActive" and not a."IsDelete";
```

Lalu berikan **hanya** pasangan itu:

```bash
curl -s -b ck.txt -X POST \
  http://127.0.0.1:5215/api/v1/administrator/setting/role-access/policies \
  -H 'Content-Type: application/json' -d '{
    "DepartmentId": "<id DEMO-OPR-DEPT>",
    "PositionId": "<id DEMO-OPR-POS>",
    "Permissions": [
      { "ControllerAccessId": "<...>", "ActionAccessId": "<...>", "IsAllowed": true }
    ]
  }'
```

**`OperatingRoomMaterial:Read` sengaja TIDAK diberikan.** Itu yang membuat baris ketiga matriks
bermakna.

## Langkah 5 — Matriks acceptance

| Permintaan | Izin | Harapan |
|---|---|---|
| `GET .../operating-room-management/reports/operations` tanpa login | — | **401** |
| `GET .../operating-room-management/reports/operations` dengan login | `OperatingRoomCase:Read` **diberikan** | **200** |
| `GET .../operating-room-management/reports/materials` dengan login | `OperatingRoomMaterial:Read` **ditahan** | **403** |
| `GET .../operating-room-management/cases` dengan login | `OperatingRoomCase:Read` **diberikan** | **200** |

```bash
for ep in reports/operations reports/materials cases; do
  printf "%-20s tanpa login -> %s | dengan login -> %s\n" "$ep" \
    "$(curl -s -o /dev/null -w '%{http_code}' \
        http://127.0.0.1:5215/api/v1/health-services/operating-room-management/$ep)" \
    "$(curl -s -o /dev/null -w '%{http_code}' -b ck.txt \
        http://127.0.0.1:5215/api/v1/health-services/operating-room-management/$ep)"
done
```

Baris ketiga itu yang paling berharga: **`200` dan `403` dari akun yang sama, pada aplikasi yang
sama, dalam detik yang sama** — satu-satunya yang berbeda pasangan izinnya. Itu membuktikan
otorisasinya selektif, bukan menolak atau mengizinkan semuanya.

## Langkah 6 — Bersihkan

Tiga baris data uji yang dapat dihapus tanpa akibat sesudah bukti tersimpan:

| Hal | Penanda |
|---|---|
| `SysAccessPolicy` yang dibuat Langkah 4 | departemen `DEMO-OPR-DEPT` |
| `WfpOrganizationAssignment` | `AssignmentNumber = 'UJI-OPR-011'` |
| `AspNetUserOrganization` hasil rekonsiliasi | `SourceAssignmentId` menunjuk baris di atas |

Nol akun dibuat dan nol password diubah sepanjang runbook ini, jadi tidak ada yang perlu
dipulihkan pada sisi identitas.

## Yang membuat ini tertahan, dan mengapa bukan masalah Operasi

`AspNetUserOrganization` bukan tabel yang diisi manusia: ia **projection** yang ditulis
`OrganizationAuthorizationProjectionService`, dan satu-satunya penulisnya ada di baris 247 berkas
itu. Model `ApplicationUserOrganization` memuat `SourceAssignmentId`; tabelnya di basis data dev
tidak. Setiap pemicunya karena itu gagal dengan `42703`, dan tiga jalur sah sudah dicoba:

| Jalur | Hasil |
|---|---|
| `POST external-users` dengan `CreateLoginAccount` | **500** — `42703` |
| `POST workforce-profiles/{id}/organization-assignments` | **500** — `42703` saat rekonsiliasi |
| `OperatingRoomDemoSeeder` | mati sebelum langkah akun, tabel master modul lain hilang |

`AccessPermissionService` sendiri **tidak** ikut gagal, karena ia memproyeksikan hanya kolom yang
dipakainya. Itulah sebabnya `403` dapat dibuktikan sementara `200` tidak: pipeline penegakannya
sehat, yang rusak jalur penulisan datanya.
