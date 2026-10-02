# Laporan Perubahan Backend — `BE-IGD-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-048` |
| Judul | Pengisian data lama penugasan dokter IGD |
| Slice | `IGD-S06` · `EPIC IGD-04` · `MVP-5` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.11 |
| Trace | `FR-IGD-017`, `FR-IGD-019`; `IGD-DEC-082`, `IGD-DEC-116`, `IGD-DEC-130`, **`IGD-DEC-136`** (menutup `IGD-OQ-092`); audit [2026-09-17-audit-be-igd-044-igd-oq-092.md](../../../evidence/2026-09-17-audit-be-igd-044-igd-oq-092.md) |
| Contract version | API `0.8.0` — `draft` (naik dari `0.7.0`). Dicatat sebagai **relaxed nullability change for legacy response** — **bukan** perubahan semantics penetapan/pengalihan baru. Dasar kenaikan versi ada di 3.3 |
| Dependency | `BE-IGD-044` ✅ (tabel ada), `BE-IGD-045` ✅ terverifikasi runtime. Rekonsiliasi schema **disetujui pemilik 21 September 2026** |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 2, database 2, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` — target tulis: source backend dan `docs/module-blueprints/igd/**`. Wewenang **menempel SQL pada migration, build, dan menjalankan migration** diberikan pemilik pada 21 September 2026 ("lakukan build dan migrations"), dengan larangan tambahan **tidak menginstal apa pun**. **Tidak** ada wewenang commit, push, merge, atau pindah branch |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `8d6ce7b5` pada branch `rizkiG` — **working tree belum di-commit** |
| Tanggal | 21 September 2026 |
| Status | ✅ **21 September 2026 — Implementation Complete, Build Verified, Migration Applied (dev), Data Migration Verified (salinan basis data terpisah dengan kandidat sintetis), Service Runtime Verified (probe terhadap service asli).** Kriteria A–E terbukti; siklus `Up → Down → Up`, kedua guard `Down()`, dan idempotensi diuji. **Tidak dijalankan:** uji HTTP dengan token, tampilan layar untuk baris legacy (dev **0** baris legacy; frontend belum di-build pemilik). **UAT belum dan tidak diklaim.** Bukan sekadar "dev bersih": pembuktian datanya ada pada salinan terpisah (5.1) |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, lifecycle `ACTIVE / LEGACY` — entri **ada**, nol blocker `QBE-MOD-002` |
| Keberlakuan | `TOUCHED LEGACY` — entity `EmgDoctorAssignment` sudah applied sejak 17 September 2026. Bukan `LEGACY MIGRATION` (tidak ada rename `Trx*`) |
| QBE ID yang berlaku | `QBE-ENT-002` (nullability mengikuti semantik domain), `QBE-CFG-001/002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Governance | `AGENTS.md`, `rules/backend/*`, kontrak rekayasa, dan registry terbaca — bukan `BLOCKED` |

---

## 1. Masalah yang diperbaiki

Tabel `EmgDoctorAssignment` diterapkan pada 17 September 2026 dalam keadaan **kosong**, dan
kolom `AssignedByUserId`-nya `NOT NULL`. Akibatnya dua hal:

1. Kunjungan IGD lama yang encounter-nya sudah punya dokter **tidak punya riwayat**. `GET /active`
   menjawab `404` untuk kunjungan itu, dan layar riwayat dokter (`FE-IGD-027`) kosong.
2. Pengisian data lama tidak dapat dilakukan jujur. Baris lama tidak punya pelaku penugasan yang
   dapat dibuktikan, sedangkan kolomnya wajib dan ber-foreign key.

`IGD-DEC-136` (18 September 2026) memutuskan pelaku boleh kosong **hanya** untuk baris hasil
pengisian data lama. Audit schema mendapati keenam artefak (model, konfigurasi EF, migration,
snapshot, kamus data, DTO) masih `NOT NULL`, sehingga task ini **bukan lagi data-only**: schema
harus dilonggarkan lebih dulu. Pemilik menyetujui rekonsiliasi itu pada 21 September 2026.

*Contoh.* Kunjungan IGD milik pasien A dibuat 1 Agustus 2026 dan dokternya ditetapkan lewat
endpoint lama Registrasi. Tidak ada baris di `EmgDoctorAssignment`. Setelah migration `BE-IGD-048`
berjalan, ada satu baris berjalan: dokter itu, mulai `1 Agustus 2026`, pelaku `null`, ditampilkan
sebagai **"Data historis"**.

---

## 2. Proses bisnis

**Pengisian data lama (sekali, lewat migration).**

1. Migration melonggarkan `AssignedByUserId` menjadi boleh kosong.
2. Untuk setiap kunjungan IGD yang **tidak dihapus**, tertaut encounter yang **tidak dihapus**,
   encounter-nya **punya dokter**, dan **belum punya penugasan berjalan**, migration menyisipkan
   satu baris riwayat berjalan.
3. Isi baris: dokter dari `RegPatientEncounter.DoctorId`; mulai dari **`EmgVisit.ArrivalDateTime`**
   (waktu kedatangan pasien di IGD — **historical fallback**, bukan waktu penetapan dokter yang
   terbukti; lihat 3.5.2); `EffectiveTo` kosong; **pelaku kosong**; alasan berisi penanda tetap
   `Data historis - pengisian BE-IGD-048`.
4. Kunjungan yang sudah punya penugasan berjalan — misalnya dibuat lewat `BE-IGD-045` — **dilewati**.
   Karena itu menjalankan pernyataan penyisipan sekali lagi tidak menambah baris apa pun.

**Membaca riwayat.** `GET /` dan `GET /active` mengembalikan baris legacy dengan
`assignedByUserId = null` dan `assignedByName = "Data historis"`. Nama itu dihasilkan **backend**;
frontend tidak perlu menebak.

**Penetapan dan pengalihan baru — tidak berubah sama sekali.** `POST /` dan `POST /{id}/handover`
tetap mengisi pelaku dari token pengguna terautentikasi. Request tidak punya ruas pelaku.

**Jalur tidak normal.** Encounter tanpa dokter, kunjungan yang belum tertaut encounter, serta
kunjungan/encounter yang sudah ditandai hapus **tidak** mendapat baris. Basis data dev pada
18 September 2026 punya **0 kandidat**: di sana migration menyisipkan 0 baris, dan itu benar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `backend-roadmap.md` (kartu `BE-IGD-048`, register); `MODULE-STATUS.md`;
`00-interview-decisions.md` (`IGD-DEC-136`); audit `2026-09-17-audit-be-igd-044-igd-oq-092.md`;
`EmgDoctorAssignment.cs`; `EmgDoctorAssignmentConfiguration.cs`; `EmergencyDoctorAssignmentDtos.cs`;
`EmergencyDoctorAssignmentService.cs`; `EmergencyDoctorAssignmentController.cs`;
`EmgVisit.cs`; `RegPatientEncounter.cs`; `IdentityModel.cs`; migration
`20260917072515_AddEmergencyDoctorAssignment` (**hanya dibaca**); `ApplicationDbContextModelSnapshot.cs`
(blok `EmgDoctorAssignment`, dua tempat); `erd/data-dictionary.md`; `erd/emergency-episode.md`;
`contracts/api-contract.md`; migration `20260826090500_ImplementIgdFullPatientJourney` (preseden
`gen_random_uuid()` dan pola `migrationBuilder.Sql`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgDoctorAssignment.cs` | `AssignedByUserId`: `[Required] Guid` → `Guid?`. Remarks menjelaskan `null` hanya untuk baris hasil pengisian data lama |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgDoctorAssignmentConfiguration.cs` | Relasi `AssignedByUser` diberi `.IsRequired(false)`; `OnDelete(Restrict)` **tetap** |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyDoctorAssignmentDtos.cs` | `EmergencyDoctorAssignmentResponse.AssignedByUserId`: `Guid` → `Guid?`. Request **tidak disentuh** |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` | Konstanta `NamaPelakuDataHistoris = "Data historis"`; `ProyeksiResponse` menghasilkan nama itu bila `AssignedByUserId == null`. Tetap **satu** expression, satu kueri |
| `docs/module-blueprints/igd/erd/data-dictionary.md` | Bagian 4: kolom "Wajib" → **Bersyarat**, catatan `IGD-DEC-136`, FK opsional |
| `docs/module-blueprints/igd/erd/emergency-episode.md` | Diagram Mermaid: `AssignedByUserId` diberi `"nullable, legacy only"` (tanpa ini diagram bertentangan dengan kamus data) |
| `docs/module-blueprints/igd/contracts/api-contract.md` | `contract_version` `0.7.0` → `0.8.0`; bagian 3.2: tipe `uuid?`, aturan `"Data historis"`, dampak konsumen, contoh baris legacy |
| Roadmap, `MODULE-STATUS.md`, `requirement-traceability.md`, laporan ini | Penandaan status — lihat bagian 6 |

**Tidak disentuh (sesuai batas pemilik):** route API, controller, semantics `TetapkanAsync`/`AlihkanAsync`
(baris `AssignedByUserId = actorUserId` tetap, dan `Guid` → `Guid?` konversi implisit), `Program.cs`,
frontend, Rawat Inap, migration `20260917072515`, snapshot. **Nol berkas di `Migrations/` dibuat atau diubah.**

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `assignedByUserId` pada response `GET /` dan `GET /active` menjadi **nullable**. Nol route, nol ruas dihapus, nol perubahan request. **Hasil audit aturan versioning (dikoreksi 21 September 2026).** Tidak ada pasal tertulis yang berbunyi "setiap perubahan kontrak wajib menaikkan versi": pencarian pada `blueprint-manifest.md`, `contracts/*.md`, dan `rules/` suite skill tidak menemukannya. Yang ada adalah **praktik tanpa satu pun pengecualian**: `0.4.0` (bukan aditif) → `0.5.0` (penyelarasan **teks** murni) → `0.6.0` (aditif) → `0.7.0` (aditif) — setiap perubahan kontrak menaikkan versi dan dicatat sifatnya pada manifest `contract_versions` — dan manifest menyebut penyelarasan kontrak dikerjakan "beserta kenaikan versi dan hash". Berdasarkan praktik itu, bukan pasal, versi **dinaikkan ke `0.8.0`**. Bila pemilik menilai praktik tidak cukup, kembalikan ke `0.7.0` dengan catatan pada bagian 3.2 — perubahannya dua berkas. **Sifat perubahan: "relaxed nullability change for legacy response"** — tidak ada ruas baru dan tidak ada ruas dihapus, tetapi jaminan "selalu GUID" dicabut. **Bukan** perubahan semantics penetapan/pengalihan baru: baris transaksi baru bentuknya tidak berubah. Konsumen bertipe non-nullable **harus direvisi** sebelum menampilkan baris legacy — dicatat pada api-contract 3.2 |
| Database | Schema: `AssignedByUserId` `NOT NULL` → `NULL`, FK tetap `Restrict`. Data: sisip baris legacy. **Belum ada migration** — dibuat dan dijalankan Rizki |
| Keamanan/Auth | **Tidak berubah.** Pelaku baru tetap dari klaim token (`GetCurrentUserId()` pada controller); request tidak dapat menentukannya. Yang bergeser: jaminan "setiap baris punya pelaku" pindah dari `NOT NULL` basis data ke service — harga yang sudah dicatat pada kartu roadmap |

### 3.4 Kelima koreksi desain pemilik

| # | Koreksi | Hasil |
| ---: | --- | --- |
| 1 | `Down()` jangan `DELETE WHERE AssignedByUserId IS NULL` | **Diterapkan, dan direvisi pada review final 21 September 2026.** Keadaan temporal domain — `EffectiveTo`, bukan `UpdateDateTime` — menjadi penentu; `Down()` fail-safe dengan tiga tahap (guard, `DELETE` terbatas, guard sebelum `AlterColumn`). Lihat 3.5. Sumber aktual diaudit: `AssignmentReason` pada baris normal selalu `NULL` (`TetapkanAsync`) atau teks ketikan petugas (`AlihkanAsync`), jadi penanda tetap tidak dapat bertabrakan |
| 2 | Audit service; nama "Data historis" dari backend, satu kueri | **Diterapkan.** Kondisi `x.AssignedByUserId == null` berada di expression tree yang sama, diterjemahkan EF menjadi `CASE` pada satu `SELECT` dengan `LEFT JOIN` yang sudah ada. Nol pemanggilan tambahan. Empat pemakai proyeksi (`AmbilRiwayatAsync`, `AmbilAktifAsync`, `AmbilSatuAsync` untuk `POST /` dan handover) ikut benar |
| 3 | Acceptance "jalankan dua kali" salah | **Diganti** dengan siklus `Up → verifikasi → Down → verifikasi → Up → verifikasi` di basis data terpisah; idempotensi `NOT EXISTS` diuji **terpisah** (3.7) |
| 4 | Dev 0 kandidat ≠ bukti backfill | **Diterima.** Verifikasi bermakna hanya di basis data terpisah dengan ≥ 1 kandidat; data sintetis **dilarang** di `QuilvianNewDevRizki`. Laporan ini **tidak** mengklaim backfill teruji |
| 5 | Audit versioning kontrak | **Dilakukan.** Tidak ada pasal tertulis; ada praktik tanpa pengecualian → `0.8.0`, dicatat "relaxed nullability change for legacy response" (3.3) |

**Koreksi atas rancangan saya sendiri.** Kartu roadmap `BE-IGD-048` dan audit 17 September bagian I.1
memakai `COALESCE(UpdateBy, CreateBy)` sebagai pelaku. Itu **ditolak `IGD-DEC-136`** ("dilarang
mengarang pelaku"). Desain final di bawah menyisipkan `AssignedByUserId = NULL` untuk **semua**
baris legacy, tanpa `JOIN` ke `AspNetUsers`. Draft I.1 gugur.

### 3.5 Desain final `Up()` dan `Down()`

Migration disusun **dari dua sumber**: EF menghasilkan bagian schema dari selisih model, dan empat blok
SQL ditambahkan **manual** oleh Rizki — satu pada `Up()`, tiga pada `Down()`. Urutannya mengikat.

**Marker persis untuk baris hasil `BE-IGD-048`** — dipakai identik pada `Up()` dan `Down()`:

| Ruas | Nilai persis |
| --- | --- |
| `AssignmentReason` | `Data historis - pengisian BE-IGD-048` — huruf `D` kapital, satu spasi di kedua sisi tanda hubung ASCII `-` (bukan tanda pisah panjang), 36 karakter, di bawah batas 500 |
| `AssignedByUserId` | `NULL` |
| `CreateBy` | `00000000-0000-0000-0000-000000000000` (`Guid.Empty`) |
| `EffectiveTo` | `NULL` saat disisipkan — **berubah menjadi terisi hanya bila pengalihan nyata menutupnya** |

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // [DIHASILKAN EF — biarkan] Kemungkinan besar HANYA satu operasi:
    //     AlterColumn<Guid>(name: "AssignedByUserId", schema: "public", table: "EmgDoctorAssignment",
    //                       type: "uuid", nullable: true, oldClrType: typeof(Guid), oldType: "uuid").
    //     Bila EF juga menghasilkan DropForeignKey/AddForeignKey untuk
    //     FK_EmgDoctorAssignment_AspNetUsers_AssignedByUserId, itu wajar; operasi lain tidak.

    // [MANUAL — WAJIB SESUDAH operasi EF di atas; kolom harus sudah boleh NULL]
    migrationBuilder.Sql("""
        INSERT INTO public."EmgDoctorAssignment" (
            "Id", "EmergencyVisitId", "DoctorId", "EffectiveFrom", "EffectiveTo",
            "AssignedByUserId", "AssignmentReason",
            "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
            "IsCancel", "IsDelete")
        SELECT
            gen_random_uuid(), v."Id", e."DoctorId",
            v."ArrivalDateTime", NULL,   -- EffectiveFrom: historical fallback (3.5.2), BUKAN waktu penetapan yang terbukti
            NULL, 'Data historis - pengisian BE-IGD-048',
            NOW(),
            '00000000-0000-0000-0000-000000000000'::uuid,
            '00000000-0000-0000-0000-000000000000'::uuid,
            '00000000-0000-0000-0000-000000000000'::uuid,
            '00000000-0000-0000-0000-000000000000'::uuid,
            FALSE, FALSE
        FROM public."EmgVisit" v
        JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
        WHERE v."EncounterId" IS NOT NULL
          AND e."DoctorId" IS NOT NULL
          AND NOT v."IsDelete"
          AND NOT e."IsDelete"
          AND NOT EXISTS (
              SELECT 1 FROM public."EmgDoctorAssignment" a
              WHERE a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL);
        """);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // [MANUAL 1 — guard A. WAJIB PALING AWAL, sebelum DELETE dan sebelum operasi EF]
    // Baris hasil BE-IGD-048 yang EffectiveTo-nya terisi sudah ditutup pengalihan dokter nyata.
    // Itu histori klinis yang sudah dipakai: rollback berhenti, tidak menghapusnya.
    migrationBuilder.Sql("""
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1 FROM public."EmgDoctorAssignment"
                WHERE "AssignedByUserId" IS NULL
                  AND "AssignmentReason" = 'Data historis - pengisian BE-IGD-048'
                  AND "CreateBy" = '00000000-0000-0000-0000-000000000000'::uuid
                  AND "EffectiveTo" IS NOT NULL) THEN
                RAISE EXCEPTION 'Down BE-IGD-048 dihentikan: ada penugasan hasil pengisian data lama yang sudah ditutup (EffectiveTo terisi) oleh pengalihan dokter nyata. Riwayat itu tidak boleh dihapus.';
            END IF;
        END $$;
        """);

    // [MANUAL 2 — DELETE hanya baris BE-IGD-048 yang masih untouched: belum pernah ditutup]
    migrationBuilder.Sql("""
        DELETE FROM public."EmgDoctorAssignment"
        WHERE "AssignedByUserId" IS NULL
          AND "AssignmentReason" = 'Data historis - pengisian BE-IGD-048'
          AND "CreateBy" = '00000000-0000-0000-0000-000000000000'::uuid
          AND "EffectiveTo" IS NULL;
        """);

    // [MANUAL 3 — guard C. WAJIB SEBELUM AlterColumn NOT NULL; jangan bergantung pada perilaku EF]
    migrationBuilder.Sql("""
        DO $$
        BEGIN
            IF EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" WHERE "AssignedByUserId" IS NULL) THEN
                RAISE EXCEPTION 'Down BE-IGD-048 dihentikan: masih ada baris dengan AssignedByUserId NULL yang bukan hasil pengisian data lama BE-IGD-048.';
            END IF;
        END $$;
        """);

    // [DIHASILKAN EF — biarkan; berada SESUDAH ketiga blok di atas]
    //     AlterColumn<Guid>(..., nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
    //                       oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true).
}
```

Nama migration yang diusulkan: **`BackfillEmergencyDoctorAssignment`** (mengikuti nama rancangan
pada audit I; isinya kini juga melonggarkan schema).

#### 3.5.1 Bukti dari source: handover menutup `EffectiveTo`

Penentu "sudah dipakai" pada `Down()` adalah **keadaan temporal domain**, bukan kolom audit.
Dibuktikan dari source aktual, `EmergencyDoctorAssignmentService.cs`:

| Baris | Kode | Arti |
| ---: | --- | --- |
| 277 | `lama.EffectiveTo = effectiveFrom;` | **`AlihkanAsync` menutup baris lama** dengan mengisi `EffectiveTo` — komentar baris 275–276: *"Baris lama DITUTUP, bukan ditimpa"*; baris 278–279 ikut mengisi `UpdateDateTime`/`UpdateBy` |
| 244 | `if (lama.EffectiveTo != null)` → `409` | Baris yang sudah ditutup **tidak dapat dialihkan lagi**; ini pagar sisi service |
| 287 | `EffectiveTo = null` pada baris baru | Baris pengganti lahir berjalan |
| 197 | `EffectiveTo = null` pada `TetapkanAsync` | Penetapan pertama lahir berjalan |

Pencarian `EffectiveTo =` pada seluruh `Areas/` **tidak menemukan penulis lain** untuk
`EmgDoctorAssignment` — tidak ada `ExecuteUpdate`, `SetProperty`, atau service lain yang menutup
penugasan. (Kemunculan `EffectiveTo` pada Radiologi, Billing, dan kebijakan asesmen milik entity
lain.) Jadi: **`EffectiveTo IS NOT NULL` pada baris ber-penanda `BE-IGD-048` berarti tepat satu hal —
pengalihan nyata sudah terjadi sesudah migration.** Sebaliknya `EffectiveTo IS NULL` berarti baris itu
masih persis seperti saat disisipkan.

Catatan: pengalihan juga mengisi `UpdateDateTime`. `UpdateDateTime` **tidak lagi dipakai** sebagai
penentu (koreksi pemilik): ia kolom audit, sedangkan `EffectiveTo` adalah fakta domain yang juga
dijaga unique bersyarat `IX_EmgDoctorAssignment_EmergencyVisitId_Active`.

**Mengapa `Down()` tiga tahap dan predikatnya empat syarat.**

| Tahap / syarat | Alasan |
| --- | --- |
| Guard A — `EffectiveTo IS NOT NULL` | **Fail-safe.** Baris legacy yang sudah ditutup pengalihan nyata adalah sejarah klinis sungguhan. Rollback berhenti dengan galat, tidak menghapus |
| `DELETE` — `AssignmentReason = 'Data historis - pengisian BE-IGD-048'` | Penanda deterministik. Tidak ada jalur kode lain yang menulis teks ini: `TetapkanAsync` selalu `NULL`, `AlihkanAsync` selalu teks ketikan petugas |
| `DELETE` — `AssignedByUserId IS NULL` | Semua baris legacy memang berpelaku kosong; tidak ada baris legacy berpelaku |
| `DELETE` — `CreateBy = Guid.Empty` | Cap audit sistem pada baris hasil migration. Baris buatan pengguna selalu memuat GUID pengguna |
| `DELETE` — `EffectiveTo IS NULL` | Hanya baris yang masih untouched. Setelah guard A lulus, syarat ini menjamin `DELETE` tidak menyentuh baris tertutup |
| Guard C — sisa `AssignedByUserId IS NULL` | Menghentikan rollback bila ada `NULL` yang **bukan** milik `BE-IGD-048`, sebelum `AlterColumn` mencoba `NOT NULL`. Tanpanya EF memasang `defaultValue` GUID kosong yang **bukan pengguna** dan kegagalan baru muncul di foreign key |

**Akibatnya, yang perlu diketahui pemilik.** Bila ada baris legacy yang sudah ditutup pengalihan
nyata, `Down()` **berhenti** dengan galat guard A. Seluruh migration diperkirakan dibatalkan atomik (EF
menjalankan migration Npgsql dalam transaksi) dan tabel tidak berubah — **perkiraan itu wajib dibuktikan** pada uji
guard di basis data terpisah (3.7 langkah 5b), bukan dianggap benar. Untuk kasus itu rollback butuh
keputusan manusia tentang riwayat yang sudah dipakai; itu memang tujuannya.

**Empat sifat `Up()` yang sengaja dipertahankan.**

1. **Idempoten.** `NOT EXISTS` memakai `EffectiveTo IS NULL` **tanpa** menyaring `IsDelete`,
   persis seperti unique bersyarat `IX_EmgDoctorAssignment_EmergencyVisitId_Active` (pelajaran
   `BE-IGD-047`). Menjalankannya lagi tidak dapat melanggar `23505`.
2. **Aman terhadap FK dokter.** `RegPatientEncounter.DoctorId` sudah ber-FK `Restrict` ke `MstDoctor`
   (`FK_TrxPatientEncounter_MstDoctor_DoctorId`, dipertahankan rename 10 September), jadi `DoctorId`
   yang disalin tidak mungkin yatim.
3. **Cap audit `Guid.Empty`.** `CreateBy` bertipe `Guid` non-nullable dengan bawaan `Guid.Empty`
   (`IdentityModel`). Migration bukan pengguna; mengisinya dengan pelaku tebakan sama saja mengarang.
4. **Tidak menyentuh `RegPatientEncounter`.** `DoctorId` encounter tidak dipindah dan tidak diubah.

#### 3.5.2 `EffectiveFrom` legacy — audit timestamp dan pilihan (review final pemilik, 21 September 2026)

Rumus awal `COALESCE(UpdateDateTime, CreateDateTime)` dari kartu roadmap **dicabut**. Audit seluruh
field waktu pada dua tabel, dari source:

| Field | Sumber | Nullable | Yang sebenarnya diwakili | Layak? |
| --- | --- | :---: | --- | :---: |
| **`EmgVisit.ArrivalDateTime`** | `EmergencyVisitController` baris 240: `request.ArrivalDateTime == default ? now : request.ArrivalDateTime` | **Tidak** (`timestamp with time zone NOT NULL`) | **Waktu kedatangan pasien di IGD = awal episode IGD.** Field yang sama dipakai `EmergencyDoctorAssignmentService` baris 173 dan 261 sebagai batas bawah `EffectiveFrom` untuk penugasan **baru** | **Dipilih** |
| `EmgVisit.CreateDateTime` | Cap audit `IdentityModel` | Tidak | Kapan baris kunjungan IGD dibuat. Bisa lebih lambat dari kedatangan bila diisi susulan | Prioritas 2 — **tidak tercapai** (lihat bawah) |
| `RegPatientEncounter.RegisteredAt` / `EncounterDate` | `EncounterIntakeService` baris 325/347 dan `PatientEncounterController` baris 590/610, diisi `now` (atau tanggal target) **saat berkas registrasi dibuat** | Tidak | Waktu **registrasi**, bukan kedatangan. Pada IGD registrasi dapat menyusul triase, dan kunjungan boleh `WaitingForTriage` tanpa encounter (`RegistrationCompletedAt`). Bisa jauh lebih lambat dari awal episode | Tidak — lebih jauh dari awal episode |
| `RegPatientEncounter.CreateDateTime` | Cap audit | Tidak | Kapan baris encounter dibuat | Tidak — sama, milik registrasi |
| `RegPatientEncounter.UpdateDateTime` | Diisi pada **setiap** edit encounter — `PatientEncounterController` saja punya **8** tempat yang mengisinya (jenis edit tidak dirinci satu per satu di sini), ditambah `SelaraskanDokterEncounterAsync` milik `BE-IGD-045` | **Ya** | **Waktu edit terakhir apa pun.** Bukan waktu penetapan dokter: dokter bisa sudah terisi sejak registrasi lalu encounter diedit berkali-kali sesudahnya | **Ditolak** |
| `EmgVisit.RegistrationCompletedAt`, `TreatmentStartedAt`, `VisitCompletedAt` | Peristiwa siklus kunjungan | Ya | Selesai registrasi, mulai penanganan, selesai kunjungan — semuanya **sesudah** kedatangan dan tidak menandai penetapan dokter | Tidak |
| `EmgVisit.TraumaDateTime` | Kolom klinis | Ya | Waktu kejadian trauma, **mendahului** kedatangan | Tidak — bukan bagian episode IGD |

**Pilihan: `EmgVisit.ArrivalDateTime`.** Alasan:

1. **Memenuhi prioritas 1 pemilik** — ia *memang* timestamp kedatangan IGD, dan awal episode IGD.
2. **Paling konservatif dan paling awal yang sah.** Tidak ada timestamp lain pada dua tabel yang lebih dekat ke awal episode dan sekaligus tidak nullable. Penetapan dokter tidak mungkin terjadi sebelum pasien tiba.
3. **Selaras dengan invarian domain.** Kamus data §4 dan validation bagian 3 aturan 4 melarang `EffectiveFrom` mendahului waktu kedatangan. Memakai `ArrivalDateTime` memenuhinya **secara konstruksi** (`effectiveFrom < ArrivalDateTime` ditolak; sama dengan lolos), sehingga baris legacy tidak melanggar aturan yang berlaku bagi baris baru. Rumus lama dapat melanggarnya.
4. **Tidak dapat null, jadi deterministik.** Prioritas 2 (`CreateDateTime`) **tidak dipasang sebagai `COALESCE`**: `ArrivalDateTime` `NOT NULL`, sehingga cadangan itu tidak akan pernah tercapai. Kode mati hanya menyesatkan pembaca migration. Bila kolom kelak menjadi nullable, cadangannya `EmgVisit.CreateDateTime` — milik kunjungan IGD sendiri, bukan milik registrasi.
5. **Tidak bergantung pada urutan penulisan lain.** `UpdateDateTime` tidak dipakai dalam bentuk apa pun, termasuk sebagai cadangan.

**Yang tidak boleh diklaim.** `EffectiveFrom` pada baris hasil `BE-IGD-048` adalah **historical fallback**,
**bukan** waktu penetapan dokter yang terbukti. Artinya: "dokter ini tercatat sebagai penanggung
jawab sejak awal episode, karena waktu penetapan sesungguhnya tidak tersimpan". Konsekuensinya
untuk `GET /active?at=T`: pertanyaan pada waktu `T` antara kedatangan dan penetapan sesungguhnya akan
dijawab dokter itu, dan itu **tidak dapat dibuktikan**. Baris legacy dikenali dari
`assignmentReason = 'Data historis - pengisian BE-IGD-048'` dan `assignedByUserId = null`. Catatan yang
sama ditambahkan pada kamus data §4 dan api-contract 3.2.

**SQL final untuk `EffectiveFrom`** (potongan dari `INSERT … SELECT` pada 3.5, tanpa `COALESCE` dan tanpa
`UpdateDateTime`):

```sql
SELECT
    gen_random_uuid(), v."Id", e."DoctorId",
    v."ArrivalDateTime",          -- EffectiveFrom
    NULL,                         -- EffectiveTo
    NULL, 'Data historis - pengisian BE-IGD-048',
    …
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
```

**Pemeriksaan pra-`Up` (read-only) — anomali waktu kedatangan, harapan `0`.** `ArrivalDateTime` dapat
diubah lewat `PUT` (`EmergencyVisitController` baris 321, tanpa penjaga masa depan yang terlihat), jadi
nilai di masa depan tidak mustahil. Baris berjalan dengan `EffectiveFrom` di masa depan janggal bagi
`GET /active?at=`. Periksa **sebelum** migration dijalankan di setiap basis data:

```sql
SELECT v."Id", v."ArrivalDateTime"
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE NOT v."IsDelete" AND NOT e."IsDelete" AND e."DoctorId" IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" a
                  WHERE a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL)
  AND (v."ArrivalDateTime" > NOW() OR v."ArrivalDateTime" < TIMESTAMPTZ '2000-01-01');
```

Hasil bukan `0` bukan alasan menahan migration — itu temuan data kunjungan, dan keputusan
menanganinya milik pemilik.

### 3.6 Ekspektasi diff snapshot

Sesudah `migrations add`, `ApplicationDbContextModelSnapshot.cs` **seharusnya** berubah tepat pada
**dua tempat**, keduanya blok `EmgDoctorAssignment`:

| Tempat | Sebelum | Sesudah |
| --- | --- | --- |
| Blok properti (snapshot baris 66849; Designer lama baris 66852) | `b.Property<Guid>("AssignedByUserId")` | `b.Property<Guid?>("AssignedByUserId")` |
| Blok relasi (snapshot baris 104016–104020; Designer lama baris 104019–104023) | `.OnDelete(DeleteBehavior.Restrict)` diikuti `.IsRequired();` | `.OnDelete(DeleteBehavior.Restrict);` — baris `.IsRequired();` hilang |

**Ya: hanya nullability `EmgDoctorAssignment.AssignedByUserId`.** Nol properti, index, atau
entity lain. Diperiksa langsung pada snapshot: index `IX_EmgDoctorAssignment_AssignedByUserId`
tidak berubah, dan `.IsRequired(false)` konfigurasi menghasilkan **penghapusan** `.IsRequired()`
pada snapshot, bukan penambahan baris baru.

**Peringatan — jangan tertukar dengan Rawat Inap.** `AssignedByUserId` muncul **tiga kali** pada
snapshot dan Designer: satu milik `EmgDoctorAssignment` (yang berubah) dan dua milik Rawat Inap — `InpDoctorAssignment`
(Designer lama baris 68395 properti, 104491 relasi) dan `InpNurseAssignment` (68720 properti, 104600
relasi), diverifikasi dari nama entity pembungkusnya. **Kedua entity Rawat Inap tidak boleh berubah.** Bila diff menyentuh baris-baris itu, berhenti — model Rawat Inap
tidak disentuh task ini.

#### 3.6.1 Ekspektasi diff migration

| Berkas | Ekspektasi |
| --- | --- |
| `Migrations/<stempel>_BackfillEmergencyDoctorAssignment.cs` (baru) | `Up()` dari EF: **satu** `AlterColumn<Guid>(nullable: true)` pada `EmgDoctorAssignment.AssignedByUserId` (kemungkinan bersama `DropForeignKey`/`AddForeignKey` untuk FK `AspNetUsers` itu — FK-nya sendiri tidak berubah). `Down()` dari EF: `AlterColumn<Guid>(nullable: false, defaultValue: Guid.Empty, oldNullable: true)`. Ditambah manual: **satu** `Sql` pada `Up()` (`INSERT`) dan **tiga** pada `Down()` (guard A, `DELETE`, guard C) — urutan 3.5. **Tidak boleh ada**: `CreateTable`, `DropTable`, `AddColumn`, `DropColumn`, `CreateIndex`, `DropIndex`, atau operasi pada tabel lain |
| `Migrations/<stempel>_BackfillEmergencyDoctorAssignment.Designer.cs` (baru) | Cuplikan model penuh. Bandingkan dengan `20260917072515_AddEmergencyDoctorAssignment.Designer.cs` (yang terbuka di IDE Anda): selisih yang diharapkan **hanya dua hunk yang sama** dengan tabel snapshot di atas, ditambah atribut `[Migration]` dan nama kelas. Ini pemeriksaan kedua yang tidak bergantung pada snapshot |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Dua hunk di atas, dan tidak lebih |
| `Migrations/20260917072515_AddEmergencyDoctorAssignment*.cs` | **Tidak berubah** |

**Ini ekspektasi dari pembacaan source, bukan hasil `migrations add`** — perintah itu tidak
dijalankan. Bila `git diff --stat` snapshot setelah itu menunjukkan selain dua hunk ini, **berhenti**:
snapshot pernah kehilangan blok modul lain lewat resolusi merge (memori proyek; audit BE-IGD-044
mencatat +102/−0 saat itu), dan `migrations add` akan memasukkan selisih itu ke migration ini.
Sebelum `migrations add`, disarankan menjalankan `dotnet ef migrations has-pending-model-changes`
untuk memastikan model dan snapshot sudah selaras.

### 3.7 Prosedur uji di basis data terpisah (dijalankan Rizki)

Semua di **salinan** basis data. `QuilvianNewDevRizki` tidak ditulis sebelum lolos.

1. **Buat salinan.** Mis. `CREATE DATABASE "QuilvianIgdBe048Test" TEMPLATE "QuilvianNewDevRizki";`
   (tanpa koneksi aktif ke sumber) atau `pg_dump`/`pg_restore`.
2. **Sisipkan kandidat sintetis pada salinan itu saja**, minimal dua, plus satu kontrol:

   ```sql
   -- HANYA di salinan. Dua kunjungan tertaut encounter, tanpa dokter, tanpa penugasan.
   WITH pilih AS (
       SELECT v."Id" AS visit_id, e."Id" AS enc_id,
              ROW_NUMBER() OVER (ORDER BY v."Id") AS rn
       FROM public."EmgVisit" v
       JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
       WHERE NOT v."IsDelete" AND NOT e."IsDelete" AND e."DoctorId" IS NULL
         AND NOT EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" a WHERE a."EmergencyVisitId" = v."Id")
       LIMIT 2)
   UPDATE public."RegPatientEncounter" e
   SET "DoctorId" = (SELECT d."Id" FROM public."MstDoctor" d WHERE NOT d."IsDelete" AND d."IsActive" ORDER BY d."Id" LIMIT 1),
       "UpdateDateTime" = CASE WHEN p.rn = 1 THEN NOW() ELSE NULL END
   FROM pilih p WHERE e."Id" = p.enc_id;
   ```

   Kandidat 1 sengaja diberi `UpdateDateTime` **jauh lebih baru** dari kedatangan, dan kandidat 2
   `UpdateDateTime` kosong. Keduanya harus menghasilkan `EffectiveFrom = EmgVisit.ArrivalDateTime` —
   bukti bahwa `UpdateDateTime` **tidak** dipakai (kueri F). **Kontrol:**
   kunjungan yang sudah punya penugasan berjalan (ada dari uji `BE-IGD-045`) harus **tidak**
   bertambah barisnya. Nama kolom disesuaikan bila ternyata berbeda.
3. **Buat migration sekali** dari repo (langkah 3.8), tempelkan **empat** blok SQL (satu `Up`, tiga `Down`).
4. `Up` pada salinan → **verifikasi A–E** (di bawah).
5. **Uji guard `Down()` lebih dulu (5b), lalu `Down` yang sah (5a).**
   - **5b — guard harus menolak.** Tutup satu baris legacy pada salinan, menirukan pengalihan nyata:
     `UPDATE public."EmgDoctorAssignment" SET "EffectiveTo" = NOW() WHERE "Id" = (SELECT "Id" FROM public."EmgDoctorAssignment" WHERE "AssignmentReason" = 'Data historis - pengisian BE-IGD-048' LIMIT 1);`
     Jalankan `Down` → **harapan: gagal** dengan pesan *"Down BE-IGD-048 dihentikan: ada penugasan hasil
     pengisian data lama yang sudah ditutup…"*. Verifikasi **tidak ada yang berubah**: jumlah baris
     ber-penanda sama seperti sebelum `Down`, kolom `AssignedByUserId` masih boleh `NULL`
     (`information_schema.columns`), dan `__EFMigrationsHistory` masih memuat migration ini. Ini yang
     membuktikan perkiraan atomik pada 3.5. Lalu kembalikan: `UPDATE … SET "EffectiveTo" = NULL` pada baris yang sama.
   - **5a — `Down` yang sah** ke `20260917072515_AddEmergencyDoctorAssignment` → **verifikasi**: baris
     ber-penanda `0`; kolom kembali `NOT NULL` (`information_schema.columns`); jumlah baris non-legacy tak berubah.
6. `Up` lagi → verifikasi A–E, **hasil sama dengan langkah 4**.
7. **Idempotensi terpisah:** jalankan blok `INSERT` **sebagai pernyataan SQL lepas** pada salinan
   (bukan lewat migration) → harapannya `INSERT 0 0`.
8. Lolos semuanya, baru terapkan ke dev; ekspektasi di sana: kolom nullable + `0` baris disisipkan.

**Kueri verifikasi (read-only).**

```sql
-- A. Nol kunjungan berdokter tanpa penugasan berjalan            -> 0
SELECT COUNT(*) AS "MissingAssignment"
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE NOT v."IsDelete" AND NOT e."IsDelete" AND e."DoctorId" IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" a
                  WHERE a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL);

-- B. Nol kunjungan dengan lebih dari satu penugasan berjalan     -> 0 row
SELECT "EmergencyVisitId", COUNT(*) AS "ActiveAssignmentCount"
FROM public."EmgDoctorAssignment" WHERE "EffectiveTo" IS NULL
GROUP BY "EmergencyVisitId" HAVING COUNT(*) > 1;

-- C. Dokter penugasan berjalan sinkron dengan encounter          -> 0 row
SELECT v."Id" AS visit_id, e."DoctorId" AS encounter_doctor, a."DoctorId" AS assignment_doctor
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
JOIN public."EmgDoctorAssignment" a ON a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL
WHERE NOT v."IsDelete" AND NOT e."IsDelete"
  AND e."DoctorId" IS DISTINCT FROM a."DoctorId";

-- E. Jumlah tersisip (bandingkan dengan jumlah kandidat sebelum Up)
SELECT COUNT(*) AS "Tersisip"
FROM public."EmgDoctorAssignment"
WHERE "AssignmentReason" = 'Data historis - pengisian BE-IGD-048';

-- Tambahan: bentuk baris legacy (semua harus lolos)
SELECT COUNT(*) FILTER (WHERE a."AssignedByUserId" IS NOT NULL) AS "PelakuTerisi_HarusNol",
       COUNT(*) FILTER (WHERE a."EffectiveTo" IS NOT NULL)      AS "SudahDitutup_HarusNol"
FROM public."EmgDoctorAssignment" a
WHERE a."AssignmentReason" = 'Data historis - pengisian BE-IGD-048';

-- F. EffectiveFrom baris legacy = waktu kedatangan, BUKAN UpdateDateTime  -> 0 row
SELECT a."Id", a."EffectiveFrom", v."ArrivalDateTime"
FROM public."EmgDoctorAssignment" a
JOIN public."EmgVisit" v ON v."Id" = a."EmergencyVisitId"
WHERE a."AssignmentReason" = 'Data historis - pengisian BE-IGD-048'
  AND a."EffectiveFrom" IS DISTINCT FROM v."ArrivalDateTime";
```

Kandidat sebelum `Up` dihitung dengan `WHERE` yang sama seperti `INSERT`. Yang **dilewati** dicatat
dari selisih kunjungan berdokter dengan yang sudah punya penugasan berjalan.

### 3.8 Perintah untuk Rizki (owner) — *sudah dijalankan; hasilnya di 5.1*

Dijalankan dari `NewQuilvianSystemBackend`. Build sudah lulus (bagian 5); `--no-build` memakainya
kembali dan menghindari build 4,5 menit — **hanya bila source tidak berubah sejak build itu**.

```bash
# 0. Pra-syarat: model dan snapshot selaras (lihat 3.6)
dotnet ef migrations has-pending-model-changes --no-build

# 1. Buat migration (bagian schema dihasilkan EF)
dotnet ef migrations add BackfillEmergencyDoctorAssignment --no-build

# 2. Periksa: git diff --stat Migrations/ApplicationDbContextModelSnapshot.cs  → tepat dua hunk (3.6)
# 3. Tempel EMPAT blok SQL 3.5: satu pada Up() SESUDAH operasi EF; tiga pada Down() SEBELUM operasi EF
#    (guard A → DELETE → guard C). Bandingkan .Designer.cs baru dengan yang lama (3.6.1)

# 4. Build ulang (migration baru harus terkompilasi)
dotnet build -p:RunAnalyzers=false

# 5. Uji siklus pada basis data TERPISAH (3.7); koneksinya diberikan lewat --connection
dotnet ef database update --connection "<KONEKSI_DB_TERPISAH>" --no-build
dotnet ef database update 20260917072515_AddEmergencyDoctorAssignment --connection "<KONEKSI_DB_TERPISAH>" --no-build
dotnet ef database update --connection "<KONEKSI_DB_TERPISAH>" --no-build

# 6. Baru sesudah lolos: dev
dotnet ef database update --no-build
```

`<KONEKSI_DB_TERPISAH>` sengaja tidak diisi — laporan ini tidak memuat connection string.

---

## 4. Dokumentasi endpoint

Tidak ada endpoint dibuat atau diubah rutenya. **Bentuk response** dua endpoint berubah (3.3).

#### Health Services / Emergency Installation Management / Emergency Doctor Assignment

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Riwayat penugasan dokter satu kunjungan | `EmergencyDoctorAssignment : Read` | `assignedByUserId` boleh `null`; `assignedByName = "Data historis"` pada baris legacy |
| `GET` | `/active` | Dokter aktif sekarang / pada waktu `at` | `EmergencyDoctorAssignment : Read` | Sama |
| `POST` | `/` | Menetapkan dokter pertama | `EmergencyDoctorAssignment : Create` | Nol — pelaku tetap dari token |
| `POST` | `/{id}/handover` | Mengalihkan dokter; alasan wajib | `EmergencyDoctorAssignment : Update` | Nol — pelaku tetap dari token |

Metadata `[AccessController]`/`[AccessAction]`/`[AccessPermission]` **tidak diubah**.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` (source awal, 21 September 2026, 4 menit 34 detik) | `0 Error(s)`, `207 Warning(s)` | `PASS` | Keluaran perintah. Jumlah warning **sama** dengan baseline 17 September (207); nol warning menyebut `EmgDoctorAssignment` atau `EmergencyDoctorAssignment*` |
| Build ulang sesudah migration final ditempel: `BaseOutputPath=obj/efbuild/ dotnet build … -p:RunAnalyzers=false` (5 menit 41 detik; direktori keluaran terpisah karena backend pemilik sedang berjalan dan mengunci `.exe`) | `0 Error(s)`, `207 Warning(s)` | `PASS` | Keluaran perintah. DLL dipastikan memuat string migration baru dan lebih baru daripada edit terakhir file migration |
| Pembacaan: seluruh pemakai `AssignedByUserId` | Dua penulisan (`= actorUserId`) mengonversi `Guid`→`Guid?` implisit; satu pembacaan pada proyeksi | `PASS` | Pencarian teks pada folder modul |
| Pembacaan: FK `RegPatientEncounter.DoctorId → MstDoctor` | Ada, `Restrict` | `PASS` | Migration `20260531153210` |
| Pembacaan: `gen_random_uuid()` dan `migrationBuilder.Sql` | Preseden pada migration IGD `20260826090500` | `PASS` | Berkas migration |
| `dotnet ef migrations add BackfillEmergencyDoctorAssignment` | Dijalankan pemilik (`20260921032943`); hanya berisi `AlterColumn` | `PASS` — tetapi **kurang**: empat blok SQL belum tertempel | Migration + diff snapshot: tepat dua hunk (+2/−3); Designer vs migration lama: selisih hanya nama dan dua hunk yang sama |
| Penerapan pertama oleh pemilik ke dev (`database update --no-build`) | Hanya perubahan schema; backfill tidak ikut | `PASS` — tetapi **tidak sesuai desain** | Temuan saya pada sesi ini; diperbaiki lewat siklus di 5.1 |
| Dua pemeriksaan ekspektasi | Snapshot 2 hunk; Designer 2 hunk | `PASS` | `git diff`, `diff` |
| Siklus dan uji pada salinan basis data terpisah, dev, dan probe service | Lihat 5.1 | `PASS` | Keluaran perintah yang dicatat pada 5.1 |
| Uji HTTP `GET /` dan `GET /active` dengan token | Tidak dijalankan — tidak ada kredensial uji | `NOT RUN` | Digantikan probe service asli (5.1 langkah H) |
| Tampilan layar baris legacy | Tidak dijalankan | `NOT FEASIBLE` | Dev 0 baris legacy; frontend revisi terbaru belum di-build pemilik |

**Tidak ada automated test** (proyek test dihapus 11 September 2026, `IGD-DEC-110`).

### 5.1 Bukti verifikasi data dan runtime (21 September 2026)

**Kesalahan yang ditemukan lebih dulu.** File migration hasil `migrations add` hanya memuat
`AlterColumn`; keempat blok SQL tidak tertempel, dan sudah terlanjur diterapkan ke dev. Ini
memenuhi kekhawatiran desain: `Up()` tanpa backfill, `Down()` tanpa guard. Diperbaiki dengan urutan
**Down → tempel → build → Up**, dan seluruh siklus dibuktikan lebih dulu di salinan terpisah.

**Alat dan batas.** Tidak ada `psql`/`pg_dump` di mesin, dan pemilik melarang instalasi apa pun. Yang
dipakai: (a) pelari SQL kecil di *scratchpad* (di luar repo) memakai paket `Npgsql` yang **sudah ada di
cache NuGet** — diperiksa: nol paket baru terunduh; (b) kontainer PostgreSQL 16 dari image `postgres:16`
yang **sudah ada** di mesin (`--pull never`), sehingga tidak ada yang diunduh. Docker Desktop saya
nyalakan lalu hentikan lagi. `CREATE DATABASE … TEMPLATE` di server **tidak mungkin**: dev punya 5
koneksi aktif dari backend pemilik. Karena itu salinan dibuat lokal: struktur penuh (703 tabel, nol
galat) plus data hanya enam tabel yang relevan; **`AspNetUsers` sengaja tidak disalin** (berisi hash
kata sandi dan tidak dibutuhkan). Kontainer dan volumenya sudah dihapus. Dev (PostgreSQL 15.15) hanya
dibaca sebelum dan sesudah pengujian lokal, dan terbukti tidak berubah oleh uji lokal.

**Skenario sintetis (hanya pada salinan lokal):** 3 kandidat — dua dokter sintetis (satu dengan
`UpdateDateTime` encounter jauh lebih baru dari kedatangan, satu `NULL`), satu dengan penugasan
dihapus lokal; 1 kontrol negatif (kunjungan ditandai terhapus, penugasan dihapus); 4 kontrol yang sudah
punya penugasan berjalan, dua di antaranya berriwayat handover.

| Langkah | Hasil |
| --- | --- |
| A. Prasyarat: kandidat sebelum `Up`; anomali waktu kedatangan | 3 kandidat tepat sesuai skenario; anomali `0` |
| B. `Down()` baru pada salinan (keadaan awal = schema-only) | Selesai; riwayat kembali `20260917072515`; kolom `NOT NULL`; 8 baris utuh |
| C. `Up` pertama lewat EF | Selesai; **3** baris tersisip, total 6 → 9. **A=0** (kunjungan berdokter tanpa penugasan), **B=0** (lebih dari satu berjalan), **C=0** (dokter tak sinkron), **F=0** (`EffectiveFrom` ≠ kedatangan). Semua baris legacy: `AssignedByUserId NULL`, `CreateBy` kosong, `EffectiveTo NULL`, dokter = dokter encounter. Kunjungan terhapus **tidak** disentuh |
| D. **Idempotensi** — blok `INSERT` diekstrak langsung dari file migration lalu dijalankan sebagai pernyataan lepas | `0` baris terdampak; total tetap 9 |
| E. **Guard A** — satu baris legacy ditutup (`EffectiveTo` diisi) lalu `Down` lewat EF | **Gagal** (exit 1) dengan pesan *"Down BE-IGD-048 dihentikan: ada penugasan hasil pengisian data lama yang sudah ditutup…"*; riwayat, kolom, dan jumlah baris **tidak berubah** |
| E2. **Guard C + atomik** — satu baris `NULL` bukan milik BE-IGD-048, lalu `Down` | **Gagal** (exit 1) dengan pesan *"…masih ada baris dengan AssignedByUserId NULL yang bukan hasil pengisian data lama BE-IGD-048."*; `DELETE` yang sudah jalan **ikut dibatalkan** (baris ber-marker tetap 2, total tetap 9) — perkiraan atomik pada 3.5 **terbukti** |
| F. **Down sah** lalu **Up kedua** | Down: riwayat `20260917072515`, kolom `NOT NULL`, total **6**, marker **0**. Up kedua: hasil **identik** dengan Up pertama (3 baris; A=B=C=F=0) |
| G. **Dev sebenarnya**: `Down` (versi schema-only) lalu `Up` final | Kedua langkah selesai (exit 0). Riwayat teratas = `20260921032943_BackfillEmergencyDoctorAssignment`; kolom **nullable**; total **8** (tetap); tersisip **0** — benar, dev **0** kandidat; A=B=C=F=0 |
| H. **Probe service asli** (bukan HTTP) — `EmergencyDoctorAssignmentService.AmbilRiwayatAsync` dan `AmbilAktifAsync` dipanggil dari DLL hasil build terhadap salinan lokal | Baris legacy: `assignedByUserId = null`, `assignedByName = "Data historis"`. **Satu kueri per panggilan** (`GET /` = 1, `GET /active` = 1; nol `N+1`), SQL-nya satu `SELECT` dengan `CASE WHEN AssignedByUserId IS NULL THEN 'Data historis' … END` dan `LEFT JOIN AspNetUsers`. Terhadap **dev** (read-only): baris normal tetap memuat nama asli pelaku ("SuperAdmin") dan alasan handover — perilaku lama utuh |

**Dua catatan jujur.** (1) Backend masih mengirim `assignmentReason` bermarker pada baris legacy;
penyembunyiannya di layar ada di frontend (`FE-IGD-027` 8.4), yang belum di-build pemilik. (2) Nama
pelaku pada baris normal tampak `null` pada salinan lokal semata karena `AspNetUsers` tidak disalin;
pada dev nama itu muncul.

---

## 6. Acceptance criteria dan Definition of Done

Rekonsiliasi schema (persetujuan pemilik 21 September 2026):

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Model `AssignedByUserId` → `Guid?`, `[Required]` dicabut | Terpenuhi | `EmgDoctorAssignment.cs` |
| Konfigurasi EF: relasi opsional, FK `Restrict` | Terpenuhi | `EmgDoctorAssignmentConfiguration.cs` |
| DTO response `Guid?` | Terpenuhi | `EmergencyDoctorAssignmentDtos.cs` |
| Proyeksi service menghasilkan `"Data historis"`, satu kueri | Terpenuhi, **terbukti runtime** lewat probe service asli | `EmergencyDoctorAssignmentService.cs`; 5.1 langkah H (satu kueri per panggilan, SQL `CASE`) |
| Kamus data dan API contract diselaraskan, versi dinaikkan | Terpenuhi | `data-dictionary.md`, `emergency-episode.md`, `api-contract.md` `0.8.0` |
| Route tidak berubah; semantics baru tidak berubah | Terpenuhi | Controller tak disentuh; `TetapkanAsync`/`AlihkanAsync` tak diubah |

Acceptance kartu roadmap:

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| A | Nol kunjungan berdokter tanpa penugasan | **Terpenuhi** | `0` pada salinan terpisah (3 kandidat) **dan** pada dev — 5.1 langkah C, F, G |
| B | Nol kunjungan dengan > 1 penugasan berjalan | **Terpenuhi** | `0` pada ketiganya; dijaga pula unique bersyarat |
| C | Dokter aktif sinkron dengan `RegPatientEncounter.DoctorId` | **Terpenuhi** | `0` baris `IS DISTINCT FROM` pada ketiganya |
| D | Idempotensi *(dikoreksi pemilik)*: siklus `Up→Down→Up` di basis data terpisah, plus `INSERT` lepas → `0` baris | **Terpenuhi** | Siklus B → C → F, hasil Up kedua identik; `INSERT` lepas `0` baris (langkah D); ditambah dua guard `Down()` (langkah E, E2) |
| E | Jumlah tersisip dan terlewati dicatat | **Terpenuhi** | Salinan: **3 tersisip**; **terlewati**: 4 kunjungan yang sudah punya penugasan berjalan (dan 1 kunjungan terhapus, dikecualikan filter). Dev: **0 tersisip**, 0 kandidat, 0 terlewati |

**Definition of Done.** Terpenuhi untuk task ini, dengan pengecualian yang disebut apa adanya: uji HTTP
dengan token dan tampilan layar untuk baris legacy **belum dijalankan** (5. Verifikasi); UAT belum.
Kartu roadmap melarang menyatakan selesai "karena dev bersih" — di sini pembuktiannya berasal dari
salinan terpisah berkandidat, bukan dari dev.

**Yang tidak tercakup task ini.** `BE-IGD-044` acceptance 5 (langkah mundur migration **asli**
`20260917072515`, yaitu `DropTable`, di basis data terpisah) tidak dijalankan di sini: yang diuji
adalah `Down()` milik `BE-IGD-048`. Acceptance 4 `BE-IGD-044` kini terpenuhi lewat task ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kartu roadmap dan audit 17 September memakai `COALESCE(UpdateBy, CreateBy)` sebagai pelaku; itu ditolak `IGD-DEC-136` dan tidak dipakai (3.4) |
| Masalah yang diketahui | (1) ~~`EffectiveFrom` legacy = `UpdateDateTime` encounter~~ — **dicabut pada review final 21 September 2026**; kini `EmgVisit.ArrivalDateTime`, historical fallback (3.5.2). Sisa risikonya: `ArrivalDateTime` dapat diedit lewat `PUT`, sehingga anomali (masa depan) dicek pra-`Up` (3.5.2). (2) Judul bagian 3 `api-contract.md` masih "Rencana (belum tersedia)" padahal `BE-IGD-045` selesai — **di luar cakupan**, tidak diubah. (3) **Tidak ada `IGD-DEC-137`, dengan sengaja.** `IGD-DEC-136` sudah normatif: `AssignedByUserId` boleh `NULL` hanya untuk baris legacy yang pelakunya tak terbukti, dan pelaku transaksi baru tetap wajib dari token. Persetujuan pemilik 21 September 2026 hanya **mengizinkan implementasi** keputusan yang sama, bukan aturan bisnis baru; karena itu `00-interview-decisions.md` tidak disentuh. Keputusan baru baru dibuat bila kelak muncul aturan yang belum tercakup `IGD-DEC-136`. (4) **Marker tampil di layar.** `emergency-triage-doctor-section.jsx` baris 107–109 menampilkan `assignmentReason` di bawah label *"Alasan pengalihan:"*, sehingga baris legacy akan terbaca *"Alasan pengalihan: Data historis - pengisian BE-IGD-048"* — label kurang tepat untuk baris yang bukan pengalihan. **Diselesaikan 21 September 2026 di sisi frontend** (`FE-IGD-027` bagian 8.4): baris dengan `assignedByUserId` kosong kini menampilkan `Sumber: Data historis` dan **tidak** menampilkan `assignmentReason`; alasan handover normal tetap tampil. Marker basis data **tidak berubah** — tetap dipakai `Down()` |
| Risiko tersisa | Konsumen frontend: satu-satunya pembaca `assignedByUserId` yang ditemukan (`emergency-triage-doctor-section.jsx`, fungsi `tampilkanPenugas`, pembacaan saja, frontend tidak diubah) **sudah aman terhadap `null`** — menampilkan "Data historis" — sehingga `FE-IGD-027` tidak perlu direvisi. Konsumen lain di luar repository frontend ini tidak diperiksa. `Down()` menolak bila ada baris legacy yang sudah dialihkan (3.5) |
| Perubahan sampingan | Berkas `Migrations/20260921032943_BackfillEmergencyDoctorAssignment.cs` diedit (SQL ditempel) setelah dibuat pemilik dan sudah terlanjur diterapkan; migration `20260917072515`, `Program.cs`, dan Rawat Inap **tidak** disentuh. Artefak sementara saya sudah dibersihkan: kontainer dan volume lokal, `obj/efbuild`. Sisa di *scratchpad* di luar repo (pelari SQL, probe, berkas keluaran) — tanpa kredensial |
| Interupsi | Satu jeda sesi saat build; dilanjutkan dari keluaran build yang tersimpan, tanpa mengulang penyuntingan |
| **Tindakan pada basis data** | Dev (`QuilvianNewDevRizki`, milik pemilik): `Down` migration schema-only lalu `Up` migration final, oleh saya atas perintah pemilik — kolom `AssignedByUserId` nullable, **0** baris data berubah. Dev lain **tidak** disentuh. Basis data lokal sementara sudah dihapus |
| Perhatian pemilik | `bin/Debug/net9.0/QuilvianSystemBackend.dll` **basi** (dibangun sebelum migration diedit; backend pemilik sedang berjalan sehingga tidak saya timpa). Bangun ulang sebelum perintah `dotnet ef … --no-build` berikutnya, kalau tidak EF memakai migration versi schema-only |
| Status Git | Akhir sesi (`rizkiG` `8d6ce7b5`). **Backend, milik task ini:** 4 source `M` (model, konfigurasi EF, DTO, service); `Migrations/ApplicationDbContextModelSnapshot.cs` `M` (dua hunk); `Migrations/20260921032943_BackfillEmergencyDoctorAssignment.cs` dan `.Designer.cs` `??`; `data-dictionary.md`, `emergency-episode.md`, `api-contract.md`, `blueprint-manifest.md` `M`; laporan ini `??`; roadmap, `MODULE-STATUS.md`, `requirement-traceability.md` `M` (juga sudah `M` sebelumnya oleh pekerjaan lain). **Frontend (`RizkiV2` `c9134eaac`):** satu berkas `M` — `emergency-triage-doctor-section.jsx` (`FE-IGD-027` 8.4). Sudah `M` **sebelum** task ini (bukan milik saya, tidak dicek isinya selain baris yang disunting): `00-interview-decisions.md`, `MODULE-STATUS.md`, `backend-roadmap.md`, `frontend-roadmap.md`, `requirement-traceability.md`, `FE-IGD-027.md`, dan `evidence/2026-09-18-verifikasi-runtime-fe-igd-027.md` `??`. Tidak ada stage/commit |
| Langkah berikutnya | (1) Rizki membangun ulang backend biasa (`dotnet build -p:RunAnalyzers=false`) dan `git add` migration baru beserta perubahan lain bila ingin di-commit — saya tidak melakukan stage/commit. (2) Build frontend revisi terbaru `FE-IGD-027`, lalu uji layar bila kelak ada baris legacy. (3) `BE-IGD-044` acceptance 5 (uji `Down` migration asli di basis data terpisah) masih terbuka |
