# `BE-IGD-044` — Histori penugasan dokter IGD tersimpan

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-044` |
| Gelombang | `MVP-5` · `EPIC IGD-04` · slice `IGD-S06` |
| Status | 🟡 **SEBAGIAN — 17 September 2026.** Acceptance 1, 2, dan 3 terpetakan ke source. Acceptance 4, 5, dan 6 menunggu migration yang **dijalankan Rizki**, sesuai batas eksekusi kartu. `dotnet build` belum dijalankan agent |
| Branch | `rizkiG`, di atas `81366bf3` |
| Requirement | `FR-IGD-017`, `FR-IGD-019` |
| Keputusan | `IGD-DEC-082` (**`approved` 17 September 2026**), `IGD-DEC-073`, `IGD-DEC-107`, `IGD-DEC-116`, `IGD-DEC-130` |
| Kontrak | State §6 `approved` (`IGD-DEC-108`); kamus data §4; API `0.7.0` |
| Migration | **Belum dibuat.** Agent berhenti sebelum `dotnet ef migrations add` — bahan lengkapnya di bagian 5 |

---

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `EmergencyInstallationManagement` / Emergency |
| Submodule | — (entity transaksi modul, bukan master data) |
| Pemilik / prefix pada registry | `EmergencyInstallationManagement / Emergency`, prefix **`Emg`** (= *Emergency*) |
| Status registry | **`ACTIVE / LEGACY`** — terdaftar, memberi wewenang penamaan **dan** implementasi |
| Keberlakuan | **`NEW CODE`** — entity, configuration, dan tabel baru |
| Pengecualian QBE | `QBE_EXCEPTIONS.json` kosong; nol pengecualian dipakai |
| Sumber governance | `AGENTS.md` backend; `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 19 dan 43 |
| Sisa `agents/rules/` di repo target | **Tidak ada** — sudah dicabut, tidak dipakai |

### 1.1 QBE ID yang benar-benar berlaku

| QBE ID | Ketentuan | Pemenuhan |
| --- | --- | --- |
| `QBE-ENT-001` | Entity persisted mewarisi `IdentityModel` | ✅ `EmgDoctorAssignment : IdentityModel` |
| `QBE-ENT-002` | Guid, field, navigation, nullability mengikuti semantik domain | ✅ `EffectiveTo` nullable karena kekosongannya **bermakna**; `AssignmentReason` nullable karena wajibnya bersyarat |
| `QBE-ENT-003` | Dilarang field persisted yang murni presentasi | ✅ Nol `SortOrder`, nol `IsActive` |
| `QBE-NAM-001` | Dilarang `Trx*` | ✅ Nol `TrxEmergencyDoctorAssignment` di source |
| `QBE-NAM-002` | Memakai prefix registry yang disetujui | ✅ `Emg` |
| `QBE-NAM-004` | Dilarang prefix baru atau disimpulkan dari nama folder | ✅ Prefix dibaca dari registry |
| `QBE-CFG-001` | `IEntityTypeConfiguration<T>` beserta mapping, key, index, relasi | ✅ `EmgDoctorAssignmentConfiguration` |
| `QBE-MOD-001` | Capability di bawah Area/Module pemiliknya | ✅ |
| `QBE-MOD-002`, `QBE-MOD-003` | Entri registry disetujui **sebelum** model pertama | ✅ Diperiksa lebih dulu; entri sudah ada sejak sebelum task ini |
| `QBE-DEL-001` | Menghormati lifecycle delete/cancel beserta audit aktornya | ✅ `IsDelete`/`IsCancel` dari `IdentityModel`, dan **bukan** pengganti `EffectiveTo` |

**Tidak berlaku pada task ini.** `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`,
`QBE-DTO-001`, `QBE-LOG-001`, `QBE-PAGE-001`, `QBE-OPT-001` — seluruhnya lapisan
service/controller, dan itu lingkup `BE-IGD-045`. `QBE-CODE-001` sampai `006` tidak berlaku:
tabel ini tidak punya nomor bisnis. `QBE-DB-001`/`002` dan `QBE-NAM-003` hanya untuk
`LEGACY MIGRATION`.

## 2. Berkas yang disentuh

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgDoctorAssignment.cs` | **Baru** |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgDoctorAssignmentConfiguration.cs` | **Baru** |
| `Repositories/ApplicationDbContext.cs` | +1 baris `DbSet<EmgDoctorAssignment> EmgDoctorAssignments` |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs` | +navigation `DoctorAssignments` |

Configuration ditemukan otomatis lewat `ApplyConfigurationsFromAssembly`
(`ApplicationDbContext.cs:902`); **nol baris registrasi manual**, dan **nol perubahan
`Program.cs`**.

## 3. Dua hal yang sengaja **tidak** ditiru dari `InpDoctorAssignment`

| Milik `Inp` | Alasan tidak ditiru |
| --- | --- |
| `IsActive` | Dilarang `IGD-DEC-130`. Dua penanda untuk satu fakta pasti berbeda suatu hari. Dihapus **sebelum** model dibuat, bukan ditambahkan lalu dicabut |
| `SequenceNumber` | Kamus data §4 tidak memuatnya, dan urutan riwayat sudah terbaca dari `EffectiveFrom`. Menambahkannya berarti mendesain ulang kontrak secara sepihak |

## 4. Acceptance criteria

| # | Kriteria | Status | Bukti |
| ---: | --- | :-: | --- |
| 1 | Nama class, tabel, configuration, dan DbSet memakai `EmgDoctorAssignment`; nol `TrxEmergencyDoctorAssignment` di source | ✅ | Atribut `Table` bernilai `EmgDoctorAssignment` schema `public`; `EmgDoctorAssignmentConfiguration`; `DbSet` bernama `EmgDoctorAssignments` |
| 2 | Kolom, tipe, panjang sesuai kamus data §4 — termasuk **nol** properti `IsActive` | ✅ | Tujuh kolom domain persis §4; `AssignmentReason` dibatasi 500; nol `IsActive` pada model maupun configuration |
| 3 | Unique index bersyarat menolak baris berjalan kedua, memakai penyaring `EffectiveTo IS NULL` | ✅ | `IX_EmgDoctorAssignment_EmergencyVisitId_Active`, unique, penyaring pada kolom `EffectiveTo` bernilai NULL |
| 4 | Pengisian data lama: tepat satu baris aktif per kunjungan yang encounter-nya punya dokter | 🟡 | SQL siap di bagian 5.2, **belum dijalankan**. **Satu keputusan terbuka — bagian 6** |
| 5 | Langkah mundur migration tertulis **dan diuji di basis data terpisah** | 🟡 | Keterangan di bagian 5.3; eksekusi milik Rizki |
| 6 | `ApplicationDbContextModelSnapshot.cs` hanya bertambah blok `EmgDoctorAssignment` | 🟡 | Baru dapat diperiksa sesudah `migrations add` dijalankan Rizki |

## 5. Bahan migration — untuk dijalankan Rizki

Agent **berhenti di sini** sesuai batas eksekusi kartu.

### 5.1 Perintah

```bash
cd NewQuilvianSystemBackend
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
dotnet ef migrations add AddEmergencyDoctorAssignment
# tinjau berkas migration + ApplicationDbContextModelSnapshot.cs lebih dulu
dotnet ef database update
```

Sesudah `migrations add`, periksa acceptance 6: diff `ApplicationDbContextModelSnapshot.cs`
harus **hanya** memuat blok `EmgDoctorAssignment`. Bila blok modul lain ikut berubah,
**hentikan** — itu gejala yang sama dengan snapshot yang pernah kehilangan blok modul lain.

### 5.2 Pengisian data lama — disisipkan ke `Up()` sesudah `CreateTable`

```sql
INSERT INTO public."EmgDoctorAssignment"
    ("Id", "EmergencyVisitId", "DoctorId", "EffectiveFrom", "EffectiveTo",
     "AssignedByUserId", "AssignmentReason",
     "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
     "IsCancel", "IsDelete")
SELECT
    gen_random_uuid(),
    v."Id",
    e."DoctorId",
    COALESCE(e."UpdateDateTime", e."CreateDateTime"),
    NULL,
    <AKTOR>,
    NULL,
    NOW(),
    <AKTOR>,
    '00000000-0000-0000-0000-000000000000',
    '00000000-0000-0000-0000-000000000000',
    '00000000-0000-0000-0000-000000000000',
    FALSE,
    FALSE
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE v."EncounterId" IS NOT NULL
  AND e."DoctorId" IS NOT NULL
  AND NOT v."IsDelete"
  AND NOT e."IsDelete";
```

`EffectiveFrom` memakai `UpdateDateTime` dan jatuh ke `CreateDateTime` bila kosong — persis
bunyi acceptance 4. Kunjungan tanpa dokter tidak mendapat baris. `<AKTOR>` menunggu keputusan
bagian 6.

### 5.3 Langkah mundur

`Down()` bawaan EF berupa `DropTable` sudah benar dan **tidak perlu disunting**: tabelnya baru,
jadi menjatuhkannya mengembalikan keadaan sebelum migration tanpa menyentuh data modul lain.
Yang tetap wajib adalah **menjalankannya di basis data terpisah** dan mencatat hasilnya —
pelajaran `BE-IGD-026` dan `BE-IGD-031`, dan itulah syarat selesai acceptance 5.

## 6. Keputusan terbuka yang menahan acceptance 4 — `IGD-OQ-092`

**`AssignedByUserId` wajib dan ber-foreign key ke `AspNetUsers` dengan `Restrict`.** Baris hasil
pengisian data lama tidak punya pelaku yang sesungguhnya: penetapan dokternya terjadi sebelum
tabel ini ada.

Tiga calon nilai, dan ketiganya punya konsekuensi:

| Pilihan | Akibat |
| --- | --- |
| (a) `UpdateBy` encounter, jatuh ke `CreateBy` | Paling jujur secara audit, **tetapi** baris yang kedua kolomnya bernilai uuid nol akan **melanggar foreign key** dan menggagalkan seluruh migration |
| (b) Sama seperti (a), ditambah `JOIN` ke `AspNetUsers` sehingga baris tanpa pelaku sah **dilewati** | Migration aman, **tetapi** melanggar bunyi acceptance 4: sebagian kunjungan berdokter tidak mendapat baris |
| (c) Satu akun sistem yang ditunjuk pemilik | Migration aman dan acceptance 4 terpenuhi penuh, **tetapi** menyatakan pelaku yang tidak pernah melakukannya |

**Agent tidak memilih sendiri** — ketiganya keputusan produk tentang bagaimana riwayat klinis
lama dinyatakan, bukan detail teknis. Dicatat sebagai `IGD-OQ-092` untuk Product/Domain Owner
IGD.

Rekomendasi: **(b)**, lalu jumlah baris yang terlewati dilaporkan, dan acceptance 4 ditandai
terpenuhi-dengan-pengecualian yang tercatat. Alasannya, (b) tidak pernah mengarang pelaku dan
tidak pernah menggagalkan migration; kunjungan yang terlewati tetap dapat diberi dokter lewat
`BE-IGD-045` seperti kunjungan baru.

## 7. Validasi

| Jenis | Hasil |
| --- | --- |
| Governance preflight | ✅ Dijalankan; registry, prefix, dan keberlakuan tercatat di bagian 1 |
| Keseimbangan struktur berkas yang disentuh | ✅ Diperiksa |
| `dotnet build` | **Belum** — batas eksekusi; perintah di 5.1 |
| `dotnet ef migrations add` | **Belum** — batas eksekusi |
| Uji langkah mundur di basis data terpisah | **Belum** — acceptance 5 |
| Automated test | Tidak ada — proyek test backend dihapus (`IGD-DEC-110`) |
| Kueri basis data oleh agent | **Nol**, sesuai batas eksekusi |

## 8. Definition of Done

| Butir | Status |
| ---: | --- |
| Acceptance 1 sampai 6 terpetakan | 🟡 1–3 selesai; 4–6 menunggu migration Rizki |
| Laporan tracked ada | ✅ berkas ini |
| Roadmap dan traceability diperbarui | ✅ |
| QBE preflight dan kesesuaian engineering | ✅ bagian 1 |
| Butir 10 DoD gelombang — approval pemilik modul lain | ✅ **terbuka sejak 17 September 2026** oleh `IGD-DEC-082` `approved`. Catatan: approver yang disebut keputusan itu adalah Clinical Governance, dan peran itu masih `OPEN`; approval datang dari Product/Domain Owner, pola `IGD-DEC-107` |
| UAT | **Tidak diklaim** |

## 9. Task berikutnya

`BE-IGD-045` — controller dan service `EmergencyDoctorAssignment`: `GET /`, `GET /active?at=`,
`POST /`, `POST /{id}/handover`, beserta proyeksi `doctorName` dan `assignedByName`
(`IGD-DEC-129`). **Menunggu migration `BE-IGD-044` diterapkan lebih dulu**, karena tanpa tabelnya
tidak ada yang dapat diuji.
