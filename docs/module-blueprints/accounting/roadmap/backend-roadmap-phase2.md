# Roadmap Delivery Backend — Accounting Phase 2 (gelombang mandiri)

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 11
blueprint_status: approved            # Phase 2 disetujui Rizki, 8 September 2026
roadmap_revision: 1
roadmap_status: APPROVED
approved_by: [Rizki]
approved_at: 2026-09-08
source_backend: 02c3219
source_frontend: e732424eb
decision_revision: 1.9                # ACC-DEC-044 sampai ACC-DEC-058
contracts: [ACC-API-0.7, ACC-STATE-0.2, ACC-VALIDATION-0.5, ACC-PERMISSION-0.4, ACC-INTEGRATION-0.3, ACC-XMOD-0.1, ACC-TEST-0.1]
scope_waves: [P2-3, P2-4, P2-5]
excluded_waves: [P2-0b, P2-1, P2-2, POST-MVP]
```

## Baca ini lebih dahulu

Roadmap ini **hanya** mencakup tiga gelombang yang **tidak menyentuh modul Finance sama sekali**:
jurnal berulang, tutup bulan, dan tutup tahun. Kotak masuk kejadian keuangan (`P2-1`, `P2-2`)
sengaja **tidak** direncanakan di sini, dan alasannya bukan karena terblokir teknis — melainkan
karena isinya bergantung pada `DEC-ACC-P2-002` dan ratifikasi `ACC-XM-001` yang belum ada.

### Koreksi dependency terhadap `04-prd-to-mvp.md` bagian 28

Dokumen PRD menulis `P2-3` bergantung pada `P2-0`. **Diperiksa ulang saat menyusun roadmap ini:
tidak benar.** Template jurnal berulang menunjuk `AccJournalType` yang **sudah terisi empat baris**
(`JB`, `JP`, `JU`, `SA`) sejak `ACC-TD-011` ditutup 3 September 2026. Ia tidak membutuhkan
`AccEventType` maupun `AccPostingRule`.

Dependency yang benar:

| Gelombang | Butuh apa dari `P2-0` | Keterangan |
|---|---|---|
| `P2-3` jurnal berulang | **Tidak ada** | Sepenuhnya mandiri |
| `P2-4` tutup bulan | **Tidak ada** | Sepenuhnya mandiri, hanya memperluas `AccAccountingPeriod` |
| `P2-5` tutup tahun | `AccAccountingConfiguration` dan jenis jurnal `JT` | Disebut `P2-0a` di roadmap ini |

Karena itu `P2-0` **dipecah**: bagian `P2-0a` yang mandiri masuk roadmap ini; bagian `P2-0b`
(`AccEventType`, `AccPostingRule`, `AccPostingRuleLine`) tetap di luar lingkup.

### Batas wewenang roadmap ini

Approval blueprint memberi wewenang **source model persisted**. Ia **tidak** otomatis mengizinkan:

- `dotnet ef migrations add` maupun `dotnet ef database update`;
- perubahan shared database;
- deployment.

`BE-ACC-P2-004` adalah satu-satunya task migration, dan ia **bergerbang**: menuntut instruksi
eksplisit terpisah dari owner beserta Migration Coordination Gate
([`06-shared-migration-coordination-rule.md`](../06-shared-migration-coordination-rule.md)).

### Catatan yang mengikat setiap task backend

QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md`
backend beserta dokumen engineering canonical, bukan diputuskan di roadmap ini. Prefix `Acc`
sudah `ACTIVE` di registry sejak `ACC-DEC-038`.

Pola yang wajib diikuti, diwarisi dari `BE-ACC-007`:

- Controller → Service → `ApplicationDbContext`. **Jangan** meniru `CostCenterController`.
- `AccountingServiceResult<T>` dipetakan lewat `ToActionResult`.
- Pencarian memakai `ToLower().Contains()`, **bukan** `EF.Functions.ILike`.
- Logika yang dipakai lebih dari satu service dibuat `public static` menerima
  `ApplicationDbContext`, bukan registrasi DI baru.

---

## Ringkasan task

| ID | Judul | Gelombang | Dependency | Status |
|---|---|---|---|---|
| `BE-ACC-P2-001` | Entity dan enum penutupan periode | `P2-4` | — | `READY` |
| `BE-ACC-P2-002` | Entity dan enum jurnal berulang | `P2-3` | — | `READY` |
| `BE-ACC-P2-003` | Entity pengaturan akuntansi dan jenis jurnal `JT` | `P2-0a` | — | `READY` |
| `BE-ACC-P2-004` | **Migration gelombang mandiri** | ketiganya | `001`,`002`,`003` | **`GATED`** |
| `BE-ACC-P2-005` | Daftar periksa penutupan | `P2-4` | `004` | `READY` |
| `BE-ACC-P2-006` | Ajukan, setujui, tolak penutupan | `P2-4` | `005` | `READY` |
| `BE-ACC-P2-007` | CRUD template jurnal berulang | `P2-3` | `004` | `READY` |
| `BE-ACC-P2-008` | Penerbitan jurnal berulang dan penjadwalnya | `P2-3` | `007` | `READY` |
| `BE-ACC-P2-009` | Endpoint pengaturan akuntansi | `P2-0a` | `004` | `READY` |
| `BE-ACC-P2-010` | Pratinjau dan penyusunan jurnal penutup tahun | `P2-5` | `006`, `009` | `READY` |

**Jalur tercepat sampai ada yang terlihat:** `001` → `004` → `005` → `006`. Empat task, dan
tutup bulan sudah berjalan penuh.

---

## `BE-ACC-P2-001` — Entity dan enum penutupan periode

| Field | Isi |
|---|---|
| Outcome | Tabel `AccPeriodClosingApproval` berdiri; `AccAccountingPeriod` bertambah dua kolom; dua enum baru |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`; `FR-P2-026` sampai `FR-P2-029` |
| Kontrak | `ACC-STATE-0.2` bagian 2, `erd/data-dictionary.md` bagian 16 dan 18 |
| Reuse | `IdentityModel`, pola `AccJournalApproval` yang sudah ada |
| Cakupan | `Models/AccPeriodClosingApproval.cs`, satu configuration, satu `DbSet`; dua kolom baru pada `AccAccountingPeriod` (`ClosingSubmittedBy`, `ClosingSubmittedAt`); `Enums/PeriodClosingAction.cs`; nilai `PendingClosingApproval = 4` pada `AccountingPeriodStatus` |
| Dependency | — |
| Acceptance | (1) `PendingClosingApproval` bernilai **4**, ditambahkan di belakang; nilai 1–3 tidak bergeser. (2) Unique `(AccountingPeriodId, ActionSequence)` terpasang. (3) Kedua kolom baru **nullable**. (4) Build lulus |
| Verifikasi | `dotnet build -c Release`; pembandingan terhadap kamus data bagian 16 dan 18 |
| Risiko/pemilik | **Menggeser nilai enum yang sudah tersimpan** akan mengubah arti data periode yang sudah ada. Owner Backend |
| DoD | Build lulus. **Nol migration**, snapshot tidak berubah, database tidak disentuh |
| Status | `READY` |

## `BE-ACC-P2-002` — Entity dan enum jurnal berulang

| Field | Isi |
|---|---|
| Outcome | Tiga tabel jurnal berulang berdiri beserta configuration dan `DbSet`-nya |
| Trace | `ACC-DEC-050`; `FR-P2-018` sampai `FR-P2-022` |
| Kontrak | `erd/data-dictionary.md` bagian 13, 14, 15 |
| Reuse | `IdentityModel`, `AccJournalType` yang sudah terisi, `MstCostCenter`, pola `AccJournalLine` |
| Cakupan | `AccRecurringJournalTemplate`, `AccRecurringJournalTemplateLine`, `AccRecurringJournalRun`; tiga configuration; tiga `DbSet`; `Enums/RecurringFrequency.cs` |
| Dependency | — |
| Acceptance | (1) **Unique `(TemplateId, AccountingPeriodId)` terpasang pada `AccRecurringJournalRun`** — ini penjaga terbit ganda. (2) `DayOfMonth` dibatasi 1–28. (3) `IsActive` berbawaan `false`. (4) Build lulus |
| Verifikasi | `dotnet build -c Release`; pembandingan terhadap kamus data |
| Risiko/pemilik | Melewatkan unique index membuat penjaga terbit ganda hanya ada di kode, dan dua proses bersamaan akan lolos keduanya. Owner Backend |
| DoD | Build lulus. **Nol migration** |
| Status | `READY` |

## `BE-ACC-P2-003` — Entity pengaturan akuntansi dan jenis jurnal `JT`

| Field | Isi |
|---|---|
| Outcome | `AccAccountingConfiguration` berdiri; seeder jenis jurnal bertambah satu baris `JT` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`; `FR-P2-032`, `FR-P2-033` |
| Kontrak | `erd/data-dictionary.md` bagian 17 dan 19 |
| Reuse | `IdentityModel`, `MstLegalEntity`, `AccChartOfAccount`, seeder jenis jurnal yang sudah ada |
| Cakupan | `Models/AccAccountingConfiguration.cs`, satu configuration, satu `DbSet`; penambahan baris `JT` pada `AccountingMasterDataSeeder` |
| Dependency | — |
| Acceptance | (1) Unique `(LegalEntityId)` terpasang. (2) Seeder tetap **idempoten** — dipanggil dua kali tetap menghasilkan 5 jenis jurnal, bukan 6. (3) `JT` bernilai `RequiresApproval = true`. (4) Build lulus |
| Verifikasi | `dotnet build -c Release`; test idempotensi seeder |
| Risiko/pemilik | Seeder yang tidak idempoten akan menggandakan jenis jurnal setiap kali dipanggil. Owner Backend |
| DoD | Build lulus. **Nol migration**, seeder belum dijalankan |
| Status | `READY` |

## `BE-ACC-P2-004` — Migration gelombang mandiri

| Field | Isi |
|---|---|
| Outcome | **Lima** tabel baru dan dua kolom baru diterapkan ke database |
| Trace | `02-backend-architecture.md` bagian 18, rencana migration urutan 1, 3, dan 4 |
| Kontrak | Kamus data bagian 13–19 |
| Cakupan | Satu migration `AddAccountingPhase2Independent` memuat: `AccPeriodClosingApproval`, tiga tabel jurnal berulang, `AccAccountingConfiguration`, dan dua kolom pada `AccAccountingPeriod`. **Tidak memuat** tabel kejadian maupun aturan posting |
| Dependency | `BE-ACC-P2-001`, `002`, `003` |
| Acceptance | (1) Snapshot bertambah **tanpa satu pun deletion**. (2) Dapat dijalankan tanpa mematikan layanan — seluruhnya tabel baru dan kolom nullable. (3) `Down` mengembalikan keadaan semula. (4) `CONTAMINATION GUARD` `CLEAN` |
| Verifikasi | Pemeriksaan berkas migration dan snapshot sebelum diterapkan |
| Risiko/pemilik | **Snapshot kehilangan blok modul lain** — pola kerusakan `ACC-DEP-001` yang pernah terjadi. Periksa jumlah tabel snapshot sebelum dan sesudah. Owner |
| DoD | Migration dibuat **dan** diterapkan owner, snapshot 0 deletion |
| Status | **`GATED`** — menuntut instruksi eksplisit terpisah dari owner. **Jangan dijalankan** hanya karena task sebelumnya selesai |

## `BE-ACC-P2-005` — Daftar periksa penutupan

| Field | Isi |
|---|---|
| Outcome | Satu endpoint yang menghitung dua penghalang dan lima peringatan penutupan |
| Trace | `ACC-DEC-051`; `FR-P2-023`, `FR-P2-024`, `FR-P2-025` |
| Kontrak | `ACC-API-0.7` grup Accounting Period, `GET /{id}/closing-checklist`; `ACC-VALIDATION-0.5` bagian 4 |
| Reuse | `AccountingLegalEntityGuard`, `AccountingServiceResult<T>` |
| Cakupan | `Services/AccPeriodClosingService.cs`, satu endpoint, `PeriodClosingChecklistDto`, `PeriodClosingBlockerDto` |
| Dependency | `BE-ACC-P2-004` |
| Acceptance | (1) **Dihitung saat diminta, bukan disimpan** — dua panggilan berturut-turut setelah satu jurnal disahkan menghasilkan angka berbeda. (2) Jurnal `Draft`, `PendingApproval`, dan `Approved` semuanya terhitung sebagai penghalang; `Posted` tidak. (3) Kejadian **Tertahan** muncul sebagai **peringatan**, bukan penghalang. (4) `[AccessPermission("Period","Read")]` terpasang |
| Verifikasi | Test integrasi PostgreSQL: siapkan periode berisi 3 jurnal belum sah, panggil endpoint, sahkan satu, panggil lagi, pastikan angkanya turun |
| Risiko/pemilik | Menyimpan hasil hitungan akan membuat penutupan ditolak berdasarkan keadaan yang sudah berubah. Owner Backend |
| DoD | Endpoint berjalan, test hijau, laporan task tertulis |
| Status | `READY` |

## `BE-ACC-P2-006` — Ajukan, setujui, dan tolak penutupan

| Field | Isi |
|---|---|
| Outcome | Tiga endpoint penutupan berjalan dengan prinsip empat mata ditegakkan |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`, `ACC-DEC-016`, `ACC-DEC-027`; `FR-P2-026`, `027`, `028`, `029` |
| Kontrak | `ACC-API-0.7`, `ACC-STATE-0.2` bagian 2, `ACC-PERMISSION-0.4` |
| Reuse | Pola persetujuan `AccJournalService`, `AccountingLegalEntityGuard` |
| Cakupan | `POST /{id}/submit-closing`, `POST /{id}/approve-closing`, `POST /{id}/reject-closing`; perluasan `AccPeriodClosingService`; permission `Period : Approve` |
| Dependency | `BE-ACC-P2-005` |
| Acceptance | (1) Pengajuan ditolak `409` selama masih ada penghalang. (2) **Penyetuju sama dengan pengaju ditolak `403`.** (3) Penolakan tanpa alasan ditolak `400` dan mengembalikan periode ke `Open`. (4) Periode yang sudah `SoftClosed` sebelum Phase 2 **tetap sah tanpa riwayat persetujuan**. (5) Ketiga endpoint membawa `[AccessPermission]` yang benar |
| Verifikasi | Test integrasi PostgreSQL untuk kelima acceptance, terutama (2) dan (4) |
| Risiko/pemilik | **Sudah diverifikasi 8 September 2026: peran adalah DATA, bukan kode.** `Models/ApplicationRole.cs` adalah `IdentityRole<Guid>` biasa, dan `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` menyediakan `POST /policies`. Menambah peran ketujuh cukup pengisian data lewat layar Administrator yang sudah ada — **nol perubahan platform, nol ketergantungan Security**. Yang tersisa hanya memastikan peran itu benar-benar diisi sebelum acceptance (2) diuji. Owner Backend |
| DoD | Tiga endpoint berjalan, test hijau, laporan task tertulis |
| Status | `READY` |

## `BE-ACC-P2-007` — CRUD template jurnal berulang

| Field | Isi |
|---|---|
| Outcome | Tujuh endpoint pengelolaan template berjalan |
| Trace | `ACC-DEC-050`, `ACC-DEC-019`; `FR-P2-018`, `FR-P2-022` |
| Kontrak | `ACC-API-0.7` grup Recurring Journal, `ACC-VALIDATION-0.5` bagian 3 |
| Reuse | Pola penyimpanan induk-baris `AccJournalService`, `AccountingLegalEntityGuard` |
| Cakupan | `Services/AccRecurringJournalService.cs`, `RecurringJournalController`, tujuh endpoint, DTO terkait |
| Dependency | `BE-ACC-P2-004` |
| Acceptance | (1) Template tidak seimbang ditolak `400`. (2) Baris berakun `Expense` tanpa cost center ditolak `400`. (3) Baris berisi debit **dan** kredit sekaligus ditolak `400`. (4) Template baru berstatus tidak aktif. (5) Tujuh endpoint membawa `[AccessPermission]` |
| Verifikasi | Test `UnitTests.Sqlite` untuk validasi; test integrasi untuk penyimpanan induk-baris |
| Risiko/pemilik | **Jebakan EF yang sudah memakan waktu sekali:** mengganti baris anak dengan `RemoveRange` + `navigasi.Clear()` lalu menambah lewat navigasi terlacak membuat EF mengirim `UPDATE`, bukan `INSERT`. Tambahkan lewat `DbSet.AddRange`, dan `SaveChanges` penghapusan lebih dahulu di dalam satu transaction. Owner Backend |
| DoD | Tujuh endpoint berjalan, test hijau, laporan task tertulis |
| Status | `READY` |

## `BE-ACC-P2-008` — Penerbitan jurnal berulang dan penjadwalnya

| Field | Isi |
|---|---|
| Outcome | Template aktif menerbitkan jurnal `Draft` sendiri setiap periode, dan tidak pernah dua kali |
| Trace | `ACC-DEC-050`; `FR-P2-019`, `FR-P2-020`, `FR-P2-021` |
| Kontrak | `ACC-API-0.7` `POST /{id}/generate`, `ACC-VALIDATION-0.5` bagian 3 |
| Reuse | **`LeaveAccrualSchedulerHostedService`** sebagai pola: `IServiceScopeFactory`, `IOptions<...SchedulerOptions>` dengan tombol `Enabled`, penjaga tanggal terakhir diproses |
| Cakupan | `AccRecurringJournalSchedulerHostedService`, endpoint penerbitan manual, perluasan `AccRecurringJournalService` |
| Dependency | `BE-ACC-P2-007` |
| Acceptance | (1) **Penerbitan dua kali untuk periode yang sama hanya menghasilkan satu jurnal**, dan yang kedua ditolak **oleh database**, bukan hanya oleh kode. (2) Periode tidak menerima pencatatan ⇒ dilewati, bukan gagal. (3) Template nonaktif tidak menerbitkan apa pun. (4) Penjadwal dapat dimatikan lewat konfigurasi |
| Verifikasi | **Test integrasi PostgreSQL wajib**: dua penerbitan bersamaan pada dua koneksi terpisah, pastikan tepat satu berhasil dan satu ditolak unique index. Meniru cara `GAP-ACC-004` dibuktikan pada `BE-ACC-010` |
| Risiko/pemilik | Menguji penjaga terbit ganda hanya berurutan **tidak membuktikan apa pun** — yang dijaga justru dua proses bersamaan. Owner Backend |
| DoD | Penjadwal berjalan, test konkurensi hijau, laporan task tertulis |
| Status | `READY` |

## `BE-ACC-P2-009` — Endpoint pengaturan akuntansi

| Field | Isi |
|---|---|
| Outcome | Dua endpoint penetapan akun laba ditahan berjalan |
| Trace | `ACC-DEC-054`; `FR-P2-032` |
| Kontrak | `ACC-API-0.7` grup Configuration, `ACC-VALIDATION-0.5` bagian 5 |
| Reuse | `AccountingLegalEntityGuard`, `AccChartOfAccountService.HitungSaldoAsync` bila diperlukan |
| Cakupan | `AccountingConfigurationController`, dua endpoint, DTO terkait |
| Dependency | `BE-ACC-P2-004` |
| Acceptance | (1) Akun bukan `Equity` ditolak `422`. (2) Akun induk ditolak `422`. (3) Satu badan hukum hanya punya satu pengaturan. (4) Kedua endpoint membawa `[AccessPermission]` |
| Verifikasi | Test `UnitTests.Sqlite` untuk ketiga penolakan |
| Risiko/pemilik | Owner Backend |
| DoD | Dua endpoint berjalan, test hijau, laporan task tertulis |
| Status | `READY` |

## `BE-ACC-P2-010` — Pratinjau dan penyusunan jurnal penutup tahun

| Field | Isi |
|---|---|
| Outcome | Dua endpoint tutup tahun berjalan; jurnal penutup lahir sebagai `Draft` berjenis `JT` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`, `ACC-DEC-029`; `FR-P2-030` sampai `FR-P2-034` |
| Kontrak | `ACC-API-0.7` grup Year End Closing, `ACC-VALIDATION-0.5` bagian 5 |
| Reuse | `AccChartOfAccountService.HitungSaldoAsync`, `AccJournalService` untuk pembuatan jurnal, jalur pengesahan jurnal yang sudah ada |
| Cakupan | `Services/AccYearEndClosingService.cs`, dua endpoint, DTO pratinjau |
| Dependency | `BE-ACC-P2-006`, `BE-ACC-P2-009` |
| Acceptance | (1) **Pratinjau tidak membuat apa pun** — dipanggil sepuluh kali, nol jurnal terbentuk. (2) Ada periode belum tertutup ⇒ `409` beserta daftar periodenya. (3) Akun laba ditahan belum ditetapkan ⇒ `422`. (4) Jurnal penutup **seimbang** dan menolkan seluruh akun `Revenue` serta `Expense`. (5) Penyusunan kedua untuk tahun yang sama ⇒ `409`. (6) Jurnal penutup dapat dibalik lewat jalur pembalikan yang sudah ada |
| Verifikasi | Test integrasi PostgreSQL memakai contoh berangka pada [`flowcharts/04-tutup-tahun.md`](../flowcharts/04-tutup-tahun.md): pendapatan Rp 1.300.000.000, beban Rp 900.000.000, laba Rp 400.000.000 |
| Risiko/pemilik | **Salah hitung tutup tahun tidak menimbulkan error** — jurnalnya tetap seimbang, hanya angkanya salah, dan terbawa ke tahun berikutnya sebagai saldo awal. Acceptance (4) wajib memeriksa saldo tiap akun menjadi nol, bukan hanya memeriksa jurnalnya seimbang. Owner Backend |
| DoD | Dua endpoint berjalan, test hijau, laporan task tertulis |
| Status | `READY` |

---

## Coverage gap yang diketahui

| Gap | Isi | Dampak |
|---|---|---|
| `ACC-TD-016` | **Nol test backend Accounting yang ada sekarang.** `BE-ACC-P2-007` dan `BE-ACC-P2-010` menyentuh `AccJournalService` yang sudah dipakai MVP, tanpa jaring regresi apa pun | Perubahan pada jalur jurnal MVP tidak akan tertangkap. Keputusan owner; disebut di sini karena roadmap ini yang menambah kode ke service tersebut |
| `ACC-TEST-0.1` | Matriks acceptance belum punya kolom bukti yang **sudah ada**, hanya "bukti yang diharapkan" | UAT `UAT-P2-11` sampai `UAT-P2-23` tidak punya tempat tinggal saat dijalankan |
| `DEC-ACC-P2-005` | Isi template jurnal berulang: nominal tetap atau rumus | `BE-ACC-P2-007` mengasumsikan **nominal tetap**. Bila kelak berubah jadi rumus, baris template bertambah kolom |
| `DEC-ACC-P2-006` | Koreksi sesudah jurnal penutup tahun sah | `BE-ACC-P2-010` acceptance (6) mengasumsikan pembalikan jurnal biasa. Belum diratifikasi |

## Yang sengaja tidak ada di roadmap ini

| Yang ditunda | Alasan |
|---|---|
| `AccEventType`, `AccPostingRule`, `AccPostingRuleLine` | Isinya menunggu `DEC-ACC-P2-002` |
| Kotak masuk kejadian, percobaan ulang, pengabaian | Gelombang `P2-1` dan `P2-2` |
| Penyambungan sungguhan ke Finance | `POST-MVP`, menunggu ratifikasi `ACC-XM-001` dan modul Finance berdiri |
