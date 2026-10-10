# Laporan Perubahan Backend — `BE-FIN-092`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-092` |
| Judul | Model entitas dan migration skema basis data perjanjian cicilan dan pelunasan internal terpasang rapi tanpa downtime |
| Slice | `REV-18B1` — Fondasi Skema & Intake Serah Terima |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-193`, `212`; `FIN-DEC-165`, `177`; `FIN-DES-099`, `102` |
| Contract version | `data/data-dictionary.md` Bagian 1-5 (kamus field lengkap, `approved` 5 Oktober 2026); `02-backend-architecture.md` O.4-O.6, P.11 |
| Dependency | Tidak ada (roadmap: `—`). `REV-18B1` boleh mulai setelah Persetujuan Rilis REV-18 — **sudah diberikan** 6 Oktober 2026 |
| Klasifikasi | `MEDIUM` — 4 entity baru + 4 configuration baru dalam module yang sudah ada (`Fin`, `ACTIVE`), 2 konstanta baru pada entity existing, 4 `DbSet` baru. Nol endpoint, nol DTO, nol service (di luar scope task ini — lihat bagian 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Models/`, `Repositories/Configurations/Corporate/FinanceManagement/Receivable/`, `Repositories/ApplicationDbContext.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis. Migration BELUM dibuat** — instruksi eksplisit pengguna "tanpa build dan migration automatis" diikuti harfiah pada task ini (berbeda dari `BE-FIN-091`, di mana file migration sempat dibuat sebelum instruksi serupa dipertegas). `dotnet build`, `dotnet ef migrations add`, dan `dotnet ef database update` **semuanya TIDAK DIJALANKAN** |

---

## 0. Backend Governance Preflight

**Area/Module:** `Corporate` / `FinanceManagement/Receivable`, prefix `Fin`, status registry `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15). Module sudah terdaftar — QBE-MOD-002/003 tidak memblokir pembuatan 4 entity baru ini.

**Applicability:** `NEW CODE` penuh untuk 4 entity + 4 configuration baru (`FinReceivableInstallmentPlan`, `FinReceivableInstallment`, `FinBenefitSettlement`, `FinBenefitSettlementItem`). `TOUCHED LEGACY` untuk 2 baris yang disisipkan ke `FinReceivableMovement.cs` (menambah konstanta, tidak mengubah struktur) dan ke `ApplicationDbContext.cs` (menambah `DbSet`).

**Cakupan yang DIBATASI secara sengaja.** Tabel Task REV-18 Backend mencantumkan cakupan `BE-FIN-092` persis: *"4 entitas baru ..., 2 nilai mutasi baru; 1 migration"* — **tidak** menyebut service, controller, atau DTO. `02-backend-architecture.md` bagian O.5.6-O.5.10 dan O.6 memang mendeskripsikan `FinanceReceivableInstallmentPlanService`, `FinanceBenefitSettlementService`, `FinanceReceivableClearanceService`, dan tiga controller — tetapi ketiganya eksplisit menjadi cakupan `BE-FIN-093`, `BE-FIN-096`, dan `BE-FIN-099` (semuanya mencantumkan `BE-FIN-092` sebagai dependency pada tabel task REV-18 Backend). Task ini **hanya** mengerjakan lapisan model + configuration + migration ("fondasi skema", sesuai nama gelombang `REV-18B1`), bukan layer service/API di atasnya.

**QBE ID yang berlaku:** QBE-ENT-001 (seluruh entity baru mewarisi `IdentityModel`), QBE-ENT-002 (nullability mengikuti semantik: `ApprovedBy`/`RejectedBy`/dst nullable, field wajib tidak), QBE-CFG-001 (`IEntityTypeConfiguration<T>` lengkap untuk keempatnya), QBE-NAM-001/002 (prefix `Fin` terdaftar, nol `Trx*`), QBE-CODE-002/003 (nomor `PlanNumber`/`SettlementNumber` **tidak** dialokasikan task ini — itu tugas service `BE-FIN-093`/`BE-FIN-099`; task ini hanya menyediakan kolomnya), QBE-DTO-001 (`NOT APPLICABLE` — nol DTO pada task ini), QBE-AUD-001 (audit tetap dari `IdentityModel`, terpisah dari application logging).

---

## 1. Proses bisnis

Task ini **tidak mengimplementasikan alur bisnis** — ia menyediakan fondasi skema basis data yang akan dipakai dua alur bisnis oleh task berikutnya:

1. **Perjanjian cicilan piutang pegawai** (`BE-FIN-093`): staf Finance mengajukan perjanjian angsuran atas piutang `EMPLOYEE_BENEFIT`, pihak lain menyetujui (maker-checker keras — pengaju **MUST NOT** jadi penyetuju), lalu jadwal angsuran (`FinReceivableInstallment`) terbit otomatis.
2. **Pelunasan internal porsi benefit RS** (`BE-FIN-099`): staf Finance menutup piutang `PAYER` ("RS Benefit") secara berkala per periode akuntansi dan per penjamin internal — bukan lewat penerimaan uang (`FinReceipt`), bukan lewat penghapusan buku.

Task ini hanya memastikan **wadah datanya** benar: tabel, kolom, index, dan check constraint terpasang sesuai kamus data, siap dipakai kedua service itu tanpa perubahan skema susulan.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/data/data-dictionary.md` (seluruh Bagian 1-8, dibaca penuh)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (O.1-O.6, P.11 — dibaca penuh)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` (`L.7.1`, `L.7.2`)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (prefix `Fin`)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs`, `FinReceivableMovement.cs` (pola existing, navigasi satu-arah)
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs` (pola check constraint, index terfilter)
- `Repositories/ApplicationDbContext.cs` (pola registrasi `DbSet`, `ApplyConfigurationsFromAssembly`)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallmentPlan.cs` | **Baru.** Entity perjanjian angsuran, 18 kolom + `RowVersion`, navigasi `Receivable` (satu arah) dan `Installments` (koleksi anak) |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallment.cs` | **Baru.** Entity baris jadwal angsuran per periode gaji |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinBenefitSettlement.cs` | **Baru.** Entity penutupan berkala porsi benefit RS, navigasi `AccountingEvent` (nullable) dan `Items` (koleksi anak) |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinBenefitSettlementItem.cs` | **Baru.** Entity baris piutang yang ditutup oleh satu pelunasan — memuat catatan temuan (lihat bagian 3) |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs` | Tambah `FinReceivableMovementTypes.PotonganGaji` dan `.PelunasanInternal`. Nol kolom diubah |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInstallmentPlanConfiguration.cs` | **Baru.** 4 check constraint, 1 FK (`Restrict`, ke `FinReceivable`), 1 index unique (`PlanNumber`), 1 index unique terfilter (satu perjanjian aktif per piutang), 1 index majemuk |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInstallmentConfiguration.cs` | **Baru.** 3 check constraint, 1 FK (`Restrict`, ke `FinReceivableInstallmentPlan`), 1 index unique majemuk, 1 index majemuk |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinBenefitSettlementConfiguration.cs` | **Baru.** 3 check constraint, 1 FK nullable (`Restrict`, ke `FinAccountingEventOutbox`), 1 index unique (`SettlementNumber`), 1 index unique terfilter (satu pelunasan aktif per periode+penjamin), 2 index pendukung |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinBenefitSettlementItemConfiguration.cs` | **Baru.** 1 check constraint, 2 FK (`Restrict`, ke `FinBenefitSettlement` dan `FinReceivable`), 1 index unique majemuk, 1 index biasa (lihat temuan bagian 3) |
| `Repositories/ApplicationDbContext.cs` | 4 `DbSet` baru disisipkan setelah `FinReceivableMovements` |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint pada task ini, sesuai batasan cakupan bagian 0 |
| Database | **4 tabel baru, nol tabel existing diubah strukturnya** (`FinReceivableMovement` hanya bertambah nilai konstanta C#, nol kolom). **Migration BELUM dibuat** — lihat Status di atas. Nama migration yang sudah ditetapkan blueprint: `AddFinanceReceivableInstallmentAndBenefitSettlement` (`02-backend-architecture.md` P.11) |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint, nol `[Authorize]`/`[AccessAction]` disentuh. `AgreementDocumentPath` pada `FinReceivableInstallmentPlan` ditandai sensitif sesuai kamus data (memuat identitas pegawai); tidak ada log yang mencetaknya pada task ini (tidak ada logger yang menyentuh entity ini sama sekali) |

---

## 3. Temuan yang dicatat, bukan didiamkan

**`FinBenefitSettlementItem.ReceivableId` — invarian unique-terfilter tidak dapat diwujudkan sebagai database constraint.** Kamus data (`data-dictionary.md` baris 136) menulis: *"Unique terfilter untuk induk yang `Status <> 'DIBATALKAN'`"*. Partial index Postgres hanya boleh memakai predikat atas kolom **tabel yang sama** — `Status` ada pada tabel induk `FinBenefitSettlement`, bukan pada `FinBenefitSettlementItem` sendiri. Tidak ada cara native EF Core/Postgres untuk membuat filtered unique index yang merujuk kolom tabel lain.

**Keputusan yang diambil:** index biasa (non-unique) dipasang pada `ReceivableId` untuk performa kueri. Penegakan invarian "satu kartu piutang `MUST NOT` ditutup dua kali oleh pelunasan yang tidak dibatalkan" **diserahkan ke layer service** (`FinanceBenefitSettlementService`, `BE-FIN-099`) lewat penguncian transaksi — mengikuti pola `AcquireLockAsync`/`pg_advisory_xact_lock` yang sudah berjalan di `FinanceBillingIntakeService`. Dicatat sebagai komentar `<summary>` pada `FinBenefitSettlementItem.cs` dan pada configuration-nya, supaya pengerjaan `BE-FIN-099` tidak mengira constraint ini sudah ditegakkan database.

Dua index unique terfilter **lain** (pada `FinReceivableInstallmentPlan.ReceivableId` dan `FinBenefitSettlement.(AccountingPeriodCode, DebtorReferenceId)`) **berhasil diwujudkan** apa adanya karena filter predikatnya merujuk kolom pada tabel itu sendiri — tidak ada masalah yang sama di sana.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna ("Tanpa build dan migration automatis") |
| `dotnet ef migrations add AddFinanceReceivableInstallmentAndBenefitSettlement` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna yang sama — berbeda dari `BE-FIN-091`, di mana perintah setara sempat dijalankan sebelum instruksi "tanpa migration" dipertegas. Pada task ini instruksinya sudah eksplisit sejak awal, diikuti harfiah |
| `dotnet ef database update` | **TIDAK DIJALANKAN** | `NOT RUN` | Di luar wewenang task — eksekusi database adalah hak eksklusif pemilik repository |
| Review diff manual (4 model + 4 configuration + 2 berkas existing) | Struktur, nullability, index, dan check constraint dicocokkan baris per baris terhadap kamus data Bagian 1-5 | `PASS` (review manual, bukan compiler) | Bagian 2.2 dan 3 di atas |

**Tidak dijalankan:** seluruh command `dotnet`/EF. **Konsekuensi yang harus disadari:** karena `dotnet ef migrations add` tidak dijalankan, source pada task ini **belum pernah divalidasi compiler**. Risiko kesalahan sintaksis atau referensi (nama property, namespace) lebih tinggi dibanding `BE-FIN-091`, yang sempat lolos compile internal EF sebelum instruksi "tanpa build" dipertegas pengguna. Lihat bagian 6.

Uji manual: `NOT APPLICABLE` — task ini tidak memiliki UI maupun endpoint untuk diuji manual; isinya murni model dan configuration.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `L.7.1` — Migration tidak membutuhkan pengisian data lama (4 tabel baru, nol baris lama disentuh) | **Belum terpenuhi — migration belum dibuat.** Desain sudah memenuhi (4 tabel baru murni, nol kolom pada tabel existing), tetapi kriteria ini butuh file migration nyata untuk "diterapkan" dan dibuktikan, dan itu belum ada | Struktur entity pada bagian 2.2 |
| `L.7.2` — Dua nilai jenis mutasi baru tidak butuh migration | **Terpenuhi.** Diverifikasi ulang ke `FinReceivableMovementConfiguration.cs`: hanya memasang check constraint untuk `Balance` dan `FundingSource`, bukan `MovementType` — konsisten dengan catatan kamus data baris 160-163 | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs` diff |
| `L.7.3`..`L.7.5` (resource hak akses, endpoint bebas tanggungan) | `NOT APPLICABLE` pada task ini — ketiganya menguji permukaan API yang menjadi cakupan `BE-FIN-093`/`BE-FIN-096`/`BE-FIN-099`, bukan `BE-FIN-092` | — |

**DoD roadmap** ("Migration file created, snapshot konsisten, laporan task tracked"): **belum terpenuhi** — migration file sengaja tidak dibuat atas instruksi eksplisit pengguna pada invocation ini.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Source task ini **belum pernah dikompilasi**. Nama property, tipe, dan referensi antar-file sudah diperiksa manual berulang kali terhadap kamus data dan pola existing, tetapi kesalahan kecil (typo nama property, namespace yang terlewat) baru akan terlihat saat `dotnet build` atau `dotnet ef migrations add` dijalankan — keduanya sengaja belum saya jalankan |
| Masalah yang diketahui | Invarian unique-terfilter lintas tabel pada `FinBenefitSettlementItem.ReceivableId` tidak dapat diwujudkan sebagai database constraint — lihat bagian 3. Perlu ditindaklanjuti layer service `BE-FIN-099` |
| Risiko tersisa | (1) Risiko compile error belum nol — lihat Peringatan. (2) Migration belum ada sama sekali, sehingga `BE-FIN-093`, `BE-FIN-096`, `BE-FIN-099`, `BE-FIN-100` (semuanya depend langsung/tidak langsung pada task ini) belum bisa lanjut ke eksekusi database sampai migration dibuat dan diterapkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | `git status --short`: 4 file model baru, 4 file configuration baru, `ApplicationDbContext.cs` dan `FinReceivableMovement.cs` dimodifikasi (lihat bagian 2.2 untuk detail lengkap) |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build` untuk memvalidasi compile. (2) Bila bersih, pemilik atau instruksi eksplisit berikutnya menjalankan `dotnet ef migrations add AddFinanceReceivableInstallmentAndBenefitSettlement` untuk menghasilkan file migration (nama sudah ditetapkan blueprint, **bukan** saya yang menebak). (3) `dotnet ef database update` tetap wewenang eksklusif pemilik. (4) Setelah migration diterapkan, `BE-FIN-093` dan `BE-FIN-099` dapat mulai |
