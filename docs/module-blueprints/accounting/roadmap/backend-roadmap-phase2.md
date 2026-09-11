# Roadmap Delivery Backend — Accounting Phase 2 (gelombang mandiri)

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 11
blueprint_status: approved            # Phase 2 disetujui Rizki, 8 September 2026
roadmap_revision: 2                   # amandemen 9 Sep 2026 - ACC-DEC-064, 065, 066
roadmap_status: APPROVED
approved_by: [Rizki]
approved_at: 2026-09-09
source_backend: 02c3219
source_frontend: e732424eb
decision_revision: 2.2                # ACC-DEC-044 sampai ACC-DEC-066
contracts: [ACC-API-0.8, ACC-STATE-0.2, ACC-VALIDATION-0.6, ACC-PERMISSION-0.4, ACC-INTEGRATION-0.3, ACC-XMOD-0.1, ACC-TEST-0.1]
scope_waves: [P2-0a, P2-3, P2-4, P2-5, P2-CTRL, P2-RECON]
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
| `BE-ACC-P2-001` ✅ | Entity dan enum penutupan periode | `P2-4` | — | **`DONE`** 9 Sep 2026 |
| `BE-ACC-P2-002` ✅ | Entity dan enum jurnal berulang | `P2-3` | — | **`DONE`** 9 Sep 2026 |
| `BE-ACC-P2-003` ✅ | Entity pengaturan akuntansi dan jenis jurnal `JT` | `P2-0a` | — | **`DONE`** 9 Sep 2026 |
| `BE-ACC-P2-004` ✅ | **Migration gelombang mandiri** | ketiganya | `001` ✅, `002` ✅, `003` ✅, `011` ✅ | **`DONE`** 9 Sep 2026 — diterapkan owner |
| `BE-ACC-P2-005` 🟡 | Daftar periksa penutupan | `P2-4` | `004` ✅ | **`SEBAGIAN`** 9 Sep 2026 — test PostgreSQL belum dijalankan |
| `BE-ACC-P2-006` 🟡 | Ajukan, setujui, tolak penutupan | `P2-4` | `005` 🟡 | **`SEBAGIAN`** 9 Sep 2026 — jalan pintas `close` **sudah ditutup**; sisa: test PostgreSQL |
| `BE-ACC-P2-007` ✅ | CRUD template jurnal berulang | `P2-3` | `004` ✅ | **`DONE`** 10 Sep 2026 |
| `BE-ACC-P2-008` ✅ | Penerbitan jurnal berulang dan penjadwalnya | `P2-3` | `007` ✅ | **`DONE`** 10 Sep 2026 — konkurensi terbukti di PostgreSQL |
| `BE-ACC-P2-009` ✅ | Endpoint pengaturan akuntansi | `P2-0a` | `004` ✅ | **`DONE`** 9 Sep 2026 |
| `BE-ACC-P2-010` 🟡 | Pratinjau dan penyusunan jurnal penutup tahun | `P2-5` | `006` 🟡, `009` ✅ | **`SEBAGIAN`** 10 Sep 2026 — 6 dari 6 acceptance terbukti; sisa: test PostgreSQL belum |
| `BE-ACC-P2-011` ✅ | **Kolom control account pada daftar akun** | `P2-CTRL` | — | **`DONE`** 9 Sep 2026 |
| `BE-ACC-P2-012` 🟡 | **Penolakan jurnal manual ke control account** | `P2-CTRL` | `004` ✅, `011` ✅ | **🟡 `SEBAGIAN`** 11 Sep 2026 — uji manual PostgreSQL S1–S8 dan A–D: acceptance (1), (3), (4) terbukti di runtime; (2) kecuali jurnal `JT`; build `Release` dan uji SQLite belum diverifikasi. **Keputusan owner 11 Sep 2026:** Implementation `COMPLETE` · Developer Manual Test `COMPLETED` · Automated Verification `DEFERRED` · UAT `HANDOFF TO UAT TEAM` — tetap 🟡 karena DoD formal menuntut test hijau; **bukan penghalang** `FE-ACC-P2-007`/`008` |
| `BE-ACC-P2-013` 🟡 | **Saldo control account dari buku besar** | `P2-RECON` | `011` ✅ | **`SEBAGIAN`** 9 Sep 2026 — test PostgreSQL belum |
| `BE-ACC-P2-014` | **Perbandingan subledger dan laporan selisih** | `P2-RECON` | `013` 🟡, gelombang `P2-1` | `READY` — blokir dibuka `ACC-DEC-071` 10 Sep 2026 |

**Jalur tercepat sampai ada yang terlihat:** `001` ✅ → `004` ✅ → `005` 🟡 → `006` 🟡. **Keempatnya
sudah dikerjakan.** Tutup bulan berjalan ujung ke ujung, dengan satu catatan penting: endpoint
lama `POST /{id}/close` **masih mengizinkan `Open` → `SoftClosed`**, sehingga persetujuannya dapat
dilewati. Lihat kartu `006`.

**Bila mendahulukan control account** (`ACC-DEC-064`, satu-satunya yang menyentuh Phase 1):
`011` ✅ → `004` ✅ → `012`. Kolomnya wajib masuk migration yang sama, jadi `011` **harus** sebelum `004`.
**Keduanya selesai 9 Sep 2026; kolom `IsControlAccount` sudah ada di database, tinggal `012`.**

---

## `BE-ACC-P2-001` ✅ — Entity dan enum penutupan periode

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
| **Status** | **✅ `DONE`** — 9 September 2026. Build `Release` **0 error**, 145 warning seluruhnya pre-existing di project `Tests/`. **4 dari 4 acceptance lulus.** Nol migration, snapshot utuh, database tidak disentuh. Laporan: [`be-acc-p2-001`](../task/report/backend/be-acc-p2-001-entity-dan-enum-penutupan-periode.md) |
| **Delta — DIRATIFIKASI** | Kamus data bagian 16 menulis `Cascade`; diimplementasikan **`Restrict`** mengikuti `AccJournalApproval` — riwayat persetujuan adalah bukti audit. **Diratifikasi `ACC-DEC-069`, 10 September 2026**: kamus data bagian 16 yang disesuaikan menjadi `Restrict`, bukan kodenya. Nol perubahan source, nol migration |

## `BE-ACC-P2-002` ✅ — Entity dan enum jurnal berulang

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
| **Status** | **✅ `DONE`** — 9 September 2026. Build `Release` **0 error**, 145 warning — **angka yang sama persis** dengan sebelum task ini, jadi kesembilan berkas menyumbang nol warning baru. **4 dari 4 acceptance lulus.** Nol migration, snapshot utuh. Laporan: [`be-acc-p2-002`](../task/report/backend/be-acc-p2-002-entity-dan-enum-jurnal-berulang.md) |
| **Catatan** | Enum `RecurringFrequency` sengaja **hanya memuat `Bulanan`** — nilai tanpa penanganan akan lolos validasi lalu gagal diam-diam di penjadwal. Satu berkas di luar cakupan disunting **komentarnya saja**: `AccJournalLineConfiguration.cs` yang menyatakan dirinya "satu-satunya Cascade", kini tidak lagi benar |

## `BE-ACC-P2-003` ✅ — Entity pengaturan akuntansi dan jenis jurnal `JT`

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
| **Status** | **✅ `DONE`** — 9 September 2026. Build `Release` **0 error**, 145 warning seluruhnya pre-existing — **angka yang sama persis** dengan sebelum task ini. **4 dari 4 acceptance lulus**, dibuktikan **8 uji baru** (`Failed: 0, Passed: 8`) yang sekaligus menjadi uji pertama modul Accounting. QBE checker `PASS`, `VIOLATION: 0`. Nol migration, snapshot utuh, database tidak disentuh, seeder belum dijalankan. Laporan: [`be-acc-p2-003`](../task/report/backend/be-acc-p2-003-entity-pengaturan-akuntansi-dan-jenis-jurnal-jt.md) |
| **Temuan di luar cakupan** | `SwaggerDocumentationTests` milik `MedicalRecordManagement` **tidak dapat lulus pada Release** karena `.csproj` sengaja mematikan `GenerateDocumentationFile` di sana; ketiganya lulus pada `Debug`. Dilaporkan, tidak diperbaiki |

## `BE-ACC-P2-004` ✅ — Migration gelombang mandiri

| Field | Isi |
|---|---|
| Outcome | **Lima** tabel baru dan **tiga** kolom baru diterapkan ke database |
| Trace | `02-backend-architecture.md` bagian 18, rencana migration urutan 1, 3, dan 4 |
| Kontrak | Kamus data bagian 13–19 |
| Cakupan | Satu migration `AddAccountingPhase2Independent` memuat: `AccPeriodClosingApproval`, tiga tabel jurnal berulang, `AccAccountingConfiguration`, dua kolom pada `AccAccountingPeriod`, dan **satu kolom `IsControlAccount` pada `AccChartOfAccount`** (`ACC-DEC-064`). **Tidak memuat** tabel kejadian maupun aturan posting |
| Dependency | `BE-ACC-P2-001` ✅ selesai, `002` ✅ selesai, `003` ✅ selesai, **`011` belum** |
| Acceptance | (1) Snapshot bertambah **tanpa satu pun deletion**. (2) Dapat dijalankan tanpa mematikan layanan — seluruhnya tabel baru dan kolom nullable. (3) `Down` mengembalikan keadaan semula. (4) `CONTAMINATION GUARD` `CLEAN` |
| Verifikasi | Pemeriksaan berkas migration dan snapshot sebelum diterapkan |
| Risiko/pemilik | **Snapshot kehilangan blok modul lain** — pola kerusakan `ACC-DEP-001` yang pernah terjadi. Periksa jumlah tabel snapshot sebelum dan sesudah. Owner |
| DoD | Migration dibuat **dan** diterapkan owner, snapshot 0 deletion |
| **Status** | **✅ `DONE`** — 9 September 2026. Migration `20260909060515_AddAccountingPhase2Independent` **dibuat dan diterapkan owner sendiri**; agent nol perintah `dotnet ef`. Migration Coordination Gate **lulus 7 dari 7**. **4 dari 4 acceptance lulus**: snapshot **565 → 570 entity, nol deletion** (`CONTAMINATION GUARD` `CLEAN`), 5 `CreateTable` + 3 `AddColumn`, kedua kolom periode `nullable` dan `IsControlAccount` ber-`defaultValue: false`, `Down` lengkap. Laporan: [`be-acc-p2-004`](../task/report/backend/be-acc-p2-004-migration-gelombang-mandiri.md) |
| **Risiko terbuka** | Ketiga berkas migration **belum di-commit**. Selama belum masuk `origin/QuilvianIntegrationBackend`, modul lain yang membuat migration berikutnya bekerja dari baseline tanpa 5 tabel ini — jendela `ACC-DEP-001` sedang terbuka |

## `BE-ACC-P2-005` 🟡 — Daftar periksa penutupan

| Field | Isi |
|---|---|
| Outcome | Satu endpoint yang menghitung dua penghalang dan lima peringatan penutupan |
| Trace | `ACC-DEC-051`; `FR-P2-023`, `FR-P2-024`, `FR-P2-025` |
| Kontrak | `ACC-API-0.7` grup Accounting Period, `GET /{id}/closing-checklist`; `ACC-VALIDATION-0.5` bagian 4 |
| Reuse | `AccountingLegalEntityGuard`, `AccountingServiceResult<T>` |
| Cakupan | `Services/AccPeriodClosingService.cs`, satu endpoint, `PeriodClosingChecklistDto`, `PeriodClosingBlockerDto` |
| Dependency | `BE-ACC-P2-004` ✅ |
| Acceptance | (1) **Dihitung saat diminta, bukan disimpan** — dua panggilan berturut-turut setelah satu jurnal disahkan menghasilkan angka berbeda. (2) Jurnal `Draft`, `PendingApproval`, dan `Approved` semuanya terhitung sebagai penghalang; `Posted` tidak. (3) Kejadian **Tertahan** muncul sebagai **peringatan**, bukan penghalang. (4) `[AccessPermission("Period","Read")]` terpasang. **(5) Penghalang ketiga — shift kasir belum ditutup (`ACC-DEC-065`) — disediakan tempatnya pada respons, tetapi bernilai kosong sampai gelombang `P2-1` berdiri.** Lihat baris Catatan |
| Verifikasi | Test integrasi PostgreSQL: siapkan periode berisi 3 jurnal belum sah, panggil endpoint, sahkan satu, panggil lagi, pastikan angkanya turun |
| **Catatan `ACC-DEC-065`** | Penghalang ketiga memvalidasi keberadaan kejadian `CASH_SHIFT_CLOSED`, dan kotak masuk kejadian baru berdiri pada `P2-1` yang di luar lingkup roadmap ini. **Aman ditunda:** selama `P2-1` belum ada, nol kejadian kas mengalir, sehingga tidak ada shift yang dapat menahan penutupan. Yang wajib dikerjakan sekarang hanyalah **menyediakan tempatnya pada respons** — supaya frontend tidak perlu diubah bentuknya saat penghalang itu diaktifkan |
| Risiko/pemilik | Menyimpan hasil hitungan akan membuat penutupan ditolak berdasarkan keadaan yang sudah berubah. **Melewatkan tempat penghalang ketiga** akan memaksa perubahan bentuk respons dan layar saat `P2-1` datang. Owner Backend |
| DoD | Endpoint berjalan, test hijau, laporan task tertulis. Penghalang ketiga **tercatat sebagai pekerjaan menyusul**, bukan didiamkan |
| **Status** | **🟡 `SEBAGIAN`** — 9 September 2026. Endpoint `GET /{id}/closing-checklist` berjalan; build `Release` **0 error**, 145 warning (nol warning baru). **5 dari 5 acceptance terbukti** lewat **8 uji baru** (`Failed: 0, Passed: 8`); seluruh project Sqlite `Failed: 3, Passed: 466` — **nol regresi**. QBE `PASS`. **Yang membuat 🟡 dan bukan ✅:** kolom Verifikasi menuntut **test integrasi PostgreSQL**, dan itu **`NOT RUN`** — `QUILVIAN_BILLING_TEST_DB` tidak diset dan fixture-nya `fail-closed`; mengarahkannya ke database dev dilarang fixture itu sendiri. Skenarionya dijalankan di SQLite. Laporan: [`be-acc-p2-005`](../task/report/backend/be-acc-p2-005-daftar-periksa-penutupan.md) |
| **Delta kontrak** | (1) Hak akses terpasang `("AccountingPeriod","Read")`, **bukan** `("Period","Read")` — argumen pertama wajib sama persis dengan `ControllerName`, kalau menyimpang hasilnya `403` permanen. (2) DTO memakai akhiran `Response` mengikuti konvensi source. (3) Empat bidang di luar kartu — `State`, `UnavailableReason`, `IsComplete`, `NotYetAvailableCount` — ditambahkan supaya butir yang **belum dapat diperiksa** tidak terbaca sebagai nol yang aman. **Menunggu ratifikasi** |
| **Yang wajib diketahui** | **8 dari 9 butir belum dapat diperiksa** karena bergantung pada gelombang `P2-1`. `CanSubmitClosing` bisa bernilai benar sementara pemeriksaannya belum lengkap — `IsComplete` menyatakan itu, dan layar `FE-ACC-P2-001` wajib menampilkannya |

## `BE-ACC-P2-006` 🟡 — Ajukan, setujui, dan tolak penutupan

| Field | Isi |
|---|---|
| Outcome | Tiga endpoint penutupan berjalan dengan prinsip empat mata ditegakkan |
| Trace | `ACC-DEC-052`, `ACC-DEC-055`, `ACC-DEC-016`, `ACC-DEC-027`; `FR-P2-026`, `027`, `028`, `029` |
| Kontrak | `ACC-API-0.7`, `ACC-STATE-0.2` bagian 2, `ACC-PERMISSION-0.4` |
| Reuse | Pola persetujuan `AccJournalService`, `AccountingLegalEntityGuard` |
| Cakupan | `POST /{id}/submit-closing`, `POST /{id}/approve-closing`, `POST /{id}/reject-closing`; perluasan `AccPeriodClosingService`; permission `Period : Approve` |
| Dependency | `BE-ACC-P2-005` 🟡 |
| Acceptance | (1) Pengajuan ditolak `409` selama masih ada penghalang. (2) **Penyetuju sama dengan pengaju ditolak `403`.** (3) Penolakan tanpa alasan ditolak `400` dan mengembalikan periode ke `Open`. (4) Periode yang sudah `SoftClosed` sebelum Phase 2 **tetap sah tanpa riwayat persetujuan**. (5) Ketiga endpoint membawa `[AccessPermission]` yang benar |
| Verifikasi | Test integrasi PostgreSQL untuk kelima acceptance, terutama (2) dan (4) |
| Risiko/pemilik | **Sudah diverifikasi 8 September 2026: peran adalah DATA, bukan kode.** `Models/ApplicationRole.cs` adalah `IdentityRole<Guid>` biasa, dan `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` menyediakan `POST /policies`. Menambah peran ketujuh cukup pengisian data lewat layar Administrator yang sudah ada — **nol perubahan platform, nol ketergantungan Security**. Yang tersisa hanya memastikan peran itu benar-benar diisi sebelum acceptance (2) diuji. Owner Backend |
| DoD | Tiga endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **🟡 `SEBAGIAN`** — 9 September 2026. Tiga endpoint berjalan, ditambah `GET /{id}/closing-history`. Build `Release` **0 error**, 145 warning (nol baru). **5 dari 5 acceptance terbukti** lewat **16 uji baru** (`Failed: 0, Passed: 16`); seluruh project Sqlite `Failed: 3, Passed: 482` — **nol regresi**. QBE `PASS`. **Yang membuat 🟡:** (1) test integrasi PostgreSQL **`NOT RUN`** — tidak ada database test; (2) **jalan pintas belum ditutup**, lihat baris berikutnya. Laporan: [`be-acc-p2-006`](../task/report/backend/be-acc-p2-006-ajukan-setujui-tolak-penutupan.md) |
| **✅ Jalan pintas DITUTUP** | `ACC-STATE-0.2` melarang `Open` → `SoftClosed` pada Phase 2, dan endpoint lama `POST /{id}/close` dulu masih mengizinkannya. **Diperbaiki 9 Sep 2026 atas instruksi owner:** `PeriksaPerpindahanTutup` kini hanya menerima `SoftClosed` → `Closed`; `Open` dan `PendingClosingApproval` ditolak `409` beserta pesan yang menunjukkan jalan yang benar. Diuji `AccPeriodCloseBypassTests` (6 uji). **Dampak frontend:** tombol Tutup Sementara dan Tutup Permanen pada `accounting-period-view.jsx` kini `409` untuk periode `Open` — disengaja, penggantinya `FE-ACC-P2-002`. Nol berkas frontend disentuh |
| **Delta kontrak** | `GET /{id}/closing-history` di luar kartu — riwayat persetujuan adalah bukti audit yang tanpa endpoint ini tersimpan tetapi tak pernah terlihat. Hak akses terpasang `("AccountingPeriod", ...)`, bukan `("Period", ...)`. Hak akses `Approve` **baru**, terpisah dari `Close` |
| **Prasyarat pemakaian** | Peran `Accounting Director` **belum terisi** di layar Administrator. Tanpa itu tidak ada yang dapat menyetujui, dan periode tertahan di `PendingClosingApproval` |

## `BE-ACC-P2-007` ✅ — CRUD template jurnal berulang

| Field | Isi |
|---|---|
| Outcome | Tujuh endpoint pengelolaan template berjalan |
| Trace | `ACC-DEC-050`, `ACC-DEC-019`; `FR-P2-018`, `FR-P2-022` |
| Kontrak | `ACC-API-0.7` grup Recurring Journal, `ACC-VALIDATION-0.5` bagian 3 |
| Reuse | Pola penyimpanan induk-baris `AccJournalService`, `AccountingLegalEntityGuard` |
| Cakupan | `Services/AccRecurringJournalService.cs`, `RecurringJournalController`, tujuh endpoint, DTO terkait |
| Dependency | `BE-ACC-P2-004` ✅ |
| Acceptance | (1) Template tidak seimbang ditolak `400`. (2) Baris berakun `Expense` tanpa cost center ditolak `400`. (3) Baris berisi debit **dan** kredit sekaligus ditolak `400`. (4) Template baru berstatus tidak aktif. (5) Tujuh endpoint membawa `[AccessPermission]` |
| Verifikasi | Test `UnitTests.Sqlite` untuk validasi; test integrasi untuk penyimpanan induk-baris |
| Risiko/pemilik | **Jebakan EF yang sudah memakan waktu sekali:** mengganti baris anak dengan `RemoveRange` + `navigasi.Clear()` lalu menambah lewat navigasi terlacak membuat EF mengirim `UPDATE`, bukan `INSERT`. Tambahkan lewat `DbSet.AddRange`, dan `SaveChanges` penghapusan lebih dahulu di dalam satu transaction. Owner Backend |
| DoD | Tujuh endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **✅ `DONE`** — 10 September 2026. Tujuh endpoint berjalan pada grup baru `api/v1/corporate/accounting/recurring-journals`. Build `Release` seluruh solution **0 error**, 200 warning seluruhnya pre-existing — **nol dari berkas task ini**. **5 dari 5 acceptance terbukti** lewat **32 uji baru** (`Failed: 0, Passed: 32`); seluruh project Sqlite `Failed: 0, Passed: 604` — **nol regresi**. QBE `PASS`, `VIOLATION: 0`. Nol migration, snapshot utuh. Laporan: [`be-acc-p2-007`](../task/report/backend/be-acc-p2-007-crud-template-jurnal-berulang.md) |
| **Jebakan EF yang dicatat kartu: DITUTUP** | Penghapusan baris disimpan **lebih dahulu** dengan `SaveChanges` tersendiri, lalu baris baru ditambahkan lewat `DbSet.AddRange` — bukan lewat navigation yang terlacak; keduanya dalam satu transaction. Yang membuktikannya bukan jumlah baris yang benar (itu tetap benar walaupun jebakan terjadi) melainkan **`Id` baris yang berganti seluruhnya**, diuji dengan nomor baris yang sengaja dibuat **sama persis** karena itulah yang memancing EF mencocokkan |
| **Delta kontrak** | **Hak akses `Activate` terpisah dari `Update`** untuk `activate`/`deactivate` — mengaktifkan template menyalakan penulisan jurnal otomatis ke buku besar, jauh lebih berat daripada menyuntingnya; preseden `BE-ACC-P2-006` yang memisahkan `Approve` dari `Close`. Nol konsumen rusak, layar template belum ada. **Menunggu ratifikasi.** Ditambah: penamaan DTO `...Response`, empat bidang ringkasan (`LineCount`, `TotalAmount`, `RunCount`, `IsBalanced`), dan tiga penolakan di luar kartu — kode template kembar, masa berlaku terbalik, dan pengaktifan template yang barisnya tidak lagi layak |
| **Batas yang perlu diketahui** | `RecurringFrequency` hanya memuat `Bulanan`, disengaja sejak `002`. Template dapat menunjuk akun yang **benar tetapi keliru**, dan tidak ada yang dapat menangkapnya selain orang yang memeriksa — itulah alasan template lahir tidak aktif |

## `BE-ACC-P2-008` ✅ — Penerbitan jurnal berulang dan penjadwalnya

| Field | Isi |
|---|---|
| Outcome | Template aktif menerbitkan jurnal `Draft` sendiri setiap periode, dan tidak pernah dua kali |
| Trace | `ACC-DEC-050`; `FR-P2-019`, `FR-P2-020`, `FR-P2-021` |
| Kontrak | `ACC-API-0.7` `POST /{id}/generate`, `ACC-VALIDATION-0.5` bagian 3 |
| Reuse | **`LeaveAccrualSchedulerHostedService`** sebagai pola: `IServiceScopeFactory`, `IOptions<...SchedulerOptions>` dengan tombol `Enabled`, penjaga tanggal terakhir diproses |
| Cakupan | `AccRecurringJournalSchedulerHostedService`, endpoint penerbitan manual, perluasan `AccRecurringJournalService` |
| Dependency | `BE-ACC-P2-007` ✅ |
| Acceptance | (1) **Penerbitan dua kali untuk periode yang sama hanya menghasilkan satu jurnal**, dan yang kedua ditolak **oleh database**, bukan hanya oleh kode. (2) Periode tidak menerima pencatatan ⇒ dilewati, bukan gagal. (3) Template nonaktif tidak menerbitkan apa pun. (4) Penjadwal dapat dimatikan lewat konfigurasi |
| Verifikasi | **Test integrasi PostgreSQL wajib**: dua penerbitan bersamaan pada dua koneksi terpisah, pastikan tepat satu berhasil dan satu ditolak unique index. Meniru cara `GAP-ACC-004` dibuktikan pada `BE-ACC-010` |
| Risiko/pemilik | Menguji penjaga terbit ganda hanya berurutan **tidak membuktikan apa pun** — yang dijaga justru dua proses bersamaan. Owner Backend |
| DoD | Penjadwal berjalan, test konkurensi hijau, laporan task tertulis |
| **Status** | **✅ `DONE`** — 10 September 2026. Endpoint penerbitan manual dan hosted service penjadwal berjalan. Build `Release` **0 error**, 200 warning seluruhnya pre-existing — **nol dari berkas task ini**. **4 dari 4 acceptance terbukti** lewat **14 uji baru** (`Failed: 0, Passed: 14`); seluruh project Sqlite `Failed: 0, Passed: 604` — **nol regresi**. QBE `PASS`, `VIOLATION: 0`. Nol migration. Laporan: [`be-acc-p2-008`](../task/report/backend/be-acc-p2-008-penerbitan-jurnal-berulang-dan-penjadwalnya.md) |
| **Acceptance (1) dibuktikan di PostgreSQL sungguhan** | Kartu menegaskan menguji berurutan *"tidak membuktikan apa pun"*. Karena itu pengujiannya dijalankan langsung ke `QuilvianNewDevRizki` dengan **dua koneksi terpisah yang berangkat bersamaan**, disinkronkan barrier. Hasilnya: koneksi A **berhasil**, koneksi B **ditolak** constraint `IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId`, tersimpan **tepat 1 baris**. Satu template uji dan satu baris penerbitan dibuat lalu **dihapus seluruhnya**; nol DDL, nol migration |
| **Delta kontrak** | `GenerateRecurringJournalRequest` membawa `accountingDate`, **bukan** `accountingPeriodId` — periode adalah turunan tanggal di seluruh modul ini, dan menerimanya langsung membuka jalan jurnal tercatat di periode yang tidak sesuai tanggalnya. Ditambah: bawaan penjadwal **`Enabled = false`** (berbeda dari `LeaveAccrualScheduler` yang ditiru — penjadwal ini menulis ke buku besar), dan hasil siklus dikembalikan sebagai objek supaya "dilewati, bukan gagal" dapat diuji tanpa membaca log |
| **Batas yang perlu diketahui** | **Penjadwal bawaannya MATI.** Jurnal berulang tidak akan terbit sampai `Accounting:RecurringJournalScheduler:Enabled` diisi `true`. Penjaga `_tanggalTerakhirDiproses` hanya berlaku dalam satu proses; lebih dari satu instance akan mengerjakan siklus yang sama berulang — tidak berbahaya, karena unique index tetap menjaga, tetapi boros |

## `BE-ACC-P2-009` ✅ — Endpoint pengaturan akuntansi

| Field | Isi |
|---|---|
| Outcome | Dua endpoint penetapan akun laba ditahan berjalan |
| Trace | `ACC-DEC-054`; `FR-P2-032` |
| Kontrak | `ACC-API-0.7` grup Configuration, `ACC-VALIDATION-0.5` bagian 5 |
| Reuse | `AccountingLegalEntityGuard`, `AccChartOfAccountService.HitungSaldoAsync` bila diperlukan |
| Cakupan | `AccountingConfigurationController`, dua endpoint, DTO terkait |
| Dependency | `BE-ACC-P2-004` ✅ |
| Acceptance | (1) Akun bukan `Equity` ditolak `422`. (2) Akun induk ditolak `422`. (3) Satu badan hukum hanya punya satu pengaturan. (4) Kedua endpoint membawa `[AccessPermission]` |
| Verifikasi | Test `UnitTests.Sqlite` untuk ketiga penolakan |
| Risiko/pemilik | Owner Backend |
| DoD | Dua endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **✅ `DONE`** — 9 September 2026. Dua endpoint berjalan. Build `Release` **0 error**, 145 warning (nol baru). **4 dari 4 acceptance lulus** lewat **14 uji baru** (`Failed: 0, Passed: 14`); seluruh project Sqlite `Failed: 3, Passed: 504` — **nol regresi**. QBE `PASS`, `VIOLATION: 0`. Verifikasi yang diminta kartu adalah `UnitTests.Sqlite`, dan itulah yang dijalankan — **tidak ada verifikasi yang tertinggal**. Nol migration. Laporan: [`be-acc-p2-009`](../task/report/backend/be-acc-p2-009-endpoint-pengaturan-akuntansi.md) |
| **Delta kontrak — DIRATIFIKASI** | Dua penolakan **di luar** tulisan kartu: akun milik badan hukum lain, dan akun nonaktif. Keduanya menutup kesalahan yang sama-sama **tidak menimbulkan error** — laba terbuang ke buku besar badan hukum lain (`ACC-DEC-037`), dan tutup tahun gagal tepat di akhir tahun buku. `GET` menjawab `200` dengan `isConfigured: false` saat belum ditetapkan, bukan `404`. **Diratifikasi `ACC-DEC-069`, 10 September 2026** |
| **Batas yang perlu diketahui** | Sistem **tidak dapat** membedakan `Laba Ditahan` dari `Modal Disetor` — keduanya akun ekuitas yang menerima transaksi, jadi keduanya lolos. Yang membedakan adalah kebijakan akuntansi, dan `ACC-DEC-054` memang menyerahkannya ke pemilik proses |

## `BE-ACC-P2-010` 🟡 — Pratinjau dan penyusunan jurnal penutup tahun

| Field | Isi |
|---|---|
| Outcome | Dua endpoint tutup tahun berjalan; jurnal penutup lahir sebagai `Draft` berjenis `JT` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`, `ACC-DEC-029`; `FR-P2-030` sampai `FR-P2-034` |
| Kontrak | `ACC-API-0.7` grup Year End Closing, `ACC-VALIDATION-0.5` bagian 5 |
| Reuse | `AccChartOfAccountService.HitungSaldoAsync`, `AccJournalService` untuk pembuatan jurnal, jalur pengesahan jurnal yang sudah ada |
| Cakupan | `Services/AccYearEndClosingService.cs`, dua endpoint, DTO pratinjau |
| Dependency | `BE-ACC-P2-006` 🟡, `BE-ACC-P2-009` ✅ |
| Acceptance | (1) **Pratinjau tidak membuat apa pun** — dipanggil sepuluh kali, nol jurnal terbentuk. (2) Ada periode belum tertutup ⇒ `409` beserta daftar periodenya. (3) Akun laba ditahan belum ditetapkan ⇒ `422`. (4) Jurnal penutup **seimbang** dan menolkan seluruh akun `Revenue` serta `Expense`. (5) Penyusunan kedua untuk tahun yang sama ⇒ `409`. (6) Jurnal penutup dapat dibalik lewat jalur pembalikan yang sudah ada |
| Verifikasi | Test integrasi PostgreSQL memakai contoh berangka pada [`flowcharts/04-tutup-tahun.md`](../flowcharts/04-tutup-tahun.md): pendapatan Rp 1.300.000.000, beban Rp 900.000.000, laba Rp 400.000.000 |
| Risiko/pemilik | **Salah hitung tutup tahun tidak menimbulkan error** — jurnalnya tetap seimbang, hanya angkanya salah, dan terbawa ke tahun berikutnya sebagai saldo awal. Acceptance (4) wajib memeriksa saldo tiap akun menjadi nol, bukan hanya memeriksa jurnalnya seimbang. Owner Backend |
| DoD | Dua endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **🟡 `SEBAGIAN`** — 10 September 2026. Dua endpoint berjalan pada grup baru `api/v1/corporate/accounting/year-end-closing`. Build `Release` seluruh solution **0 error**, 200 warning seluruhnya pre-existing — **nol warning dari keenam berkas task ini**. **6 dari 6 acceptance terbukti** lewat **20 uji baru** (`Failed: 0, Passed: 20`); seluruh project Sqlite `Failed: 0, Passed: 558` — **nol regresi**. QBE checker `PASS`, `VIOLATION: 0`. Nol migration, snapshot utuh, nol perintah database. **Yang membuat 🟡:** test integrasi PostgreSQL **`NOT RUN`** — fixture-nya menerapkan migration sendiri ke basis data dev pemilik, dan itu wewenang terpisah yang sengaja dihentikan. Laporan: [`be-acc-p2-010`](../task/report/backend/be-acc-p2-010-pratinjau-dan-penyusunan-jurnal-penutup-tahun.md) |
| **Bukti acceptance (4)** | Tidak berhenti pada "debit sama dengan kredit" — itu justru **selalu** benar, termasuk pada jurnal penutup yang angkanya salah. Uji menyusun, mengajukan, menyetujui dengan pelaku berbeda, lalu **mengesahkan** jurnal penutupnya, baru memeriksa saldo **tiap akun satu per satu** memakai `AccChartOfAccountService.HitungSaldoAsync` — penghitung milik buku besar, bukan penghitung khusus uji. Angkanya contoh berangka `flowcharts/04-tutup-tahun.md`: pendapatan Rp 1.300.000.000, beban Rp 900.000.000, laba Rp 400.000.000 |
| **Delta kontrak — DIRATIFIKASI** | **`JT` kini diterima periode `SoftClosed`.** Tanpa ini task mustahil berjalan: tutup tahun menuntut seluruh periode tahun itu tertutup, sementara tanggal jurnal penutup wajib berada di dalam tahun yang ditutup — sehingga jurnalnya lahir sebagai draft yang tak pernah dapat diajukan maupun disahkan. **Diratifikasi `ACC-DEC-067`, 10 September 2026**; `ACC-STATE` naik `0.2` → `0.3`. Ditambah: controller tersendiri `YearEndClosingController` (dituntut `ACC-PERMISSION-0.4`, sebab argumen pertama `[AccessPermission]` wajib sama dengan `ControllerName`), penamaan DTO `...Response` mengikuti preseden `009`, penolakan ganda tidak berlaku pada pratinjau, dan dua penolakan `422` di luar kartu — periode terakhir tutup permanen, dan akun laba ditahan yang berubah tidak layak sesudah ditetapkan |
| **Batas yang perlu diketahui** | **Penjaga jurnal penutup ganda belum dijamin database.** Pemeriksaan dan pembuatan berada dalam satu transaction, sehingga dua permintaan berurutan pasti tertangkap; dua permintaan **bersamaan** pada koneksi berbeda masih dapat lolos keduanya. Penutupnya unique constraint, dan itu menuntut migration — di luar wewenang task ini |

---

## Coverage gap yang diketahui

| Gap | Isi | Dampak |
|---|---|---|
| ~~`ACC-TD-016`~~ | ~~**Nol test backend Accounting yang ada sekarang.**~~ **TIDAK BERLAKU LAGI 10 September 2026.** Project `UnitTests.Sqlite` kini berisi **604 uji hijau**, di antaranya seluruh acceptance `BE-ACC-P2-003`, `005`, `006`, `007`, `008`, `009`, `010`, `011`, dan `013`. Jaring regresinya berdiri: `BE-ACC-P2-007`, `008`, dan `010` sama-sama menyentuh `AccJournalService`, dan ketiganya dijalankan bersama seluruh uji lain tanpa satu pun regresi |
| `ACC-TEST-0.1` | Matriks acceptance belum punya kolom bukti yang **sudah ada**, hanya "bukti yang diharapkan" | UAT `UAT-P2-11` sampai `UAT-P2-23` tidak punya tempat tinggal saat dijalankan |
| `DEC-ACC-P2-005` | Isi template jurnal berulang: nominal tetap atau rumus | `BE-ACC-P2-007` **sudah dibangun** dengan **nominal tetap**, 10 Sep 2026. Bila kelak berubah jadi rumus, baris template bertambah kolom dan menuntut migration. **Masih terbuka** |
| ~~`DEC-ACC-P2-006`~~ | ~~Koreksi sesudah jurnal penutup tahun sah~~ | **DITUTUP 10 September 2026 — `ACC-DEC-068`.** Alurnya ditetapkan tiga langkah: balik jurnal penutup, buka kembali Desember beserta alasan tertulis lalu tutup lagi, susun ulang jurnal penutupnya. Tanpa mekanisme "buka kembali tahun buku". Sudah terbukti berjalan pada acceptance (6) `BE-ACC-P2-010` |
| ~~`DEC-ACC-P2-009`~~ | ~~`AccPostingRule` butuh dimensi kedua (cara pembayaran)~~ | **DITUTUP 9 September 2026 — `TIDAK DIPERLUKAN`.** Jawaban owner Billing nomor 9 menetapkan kas **diringkas per shift kasir**, sehingga satu kejadian membawa beberapa cara bayar sekaligus — dan mekanisme **komponen** `ACC-DEC-058` sudah menanganinya apa adanya, tanpa kolom tambahan. Bukti: [`evidence/10`](../evidence/10-billing-arap-handoff-scan.md) bagian 25 |

## Amandemen 9 September 2026 — `ACC-DEC-064`, `065`, `066`

Ketiga keputusan diambil **sesudah** roadmap revisi 1 disetujui. Amandemen ini menambah **empat
task backend**, memperbaiki acceptance `BE-ACC-P2-005`, dan memperluas cakupan `BE-ACC-P2-004`.

Dua gelombang baru:

| Gelombang | Isi | Catatan |
|---|---|---|
| `P2-CTRL` | Control account: penandanya, dan penolakan jurnal manual ke sana | **Menyentuh artefak Phase 1** |
| `P2-RECON` | Rekonsiliasi control account | Blokirnya dibuka `ACC-DEC-071` 10 Sep 2026; `BE-ACC-P2-014` kini menunggu gelombang `P2-1`, bukan menunggu keputusan |

---

## `BE-ACC-P2-011` ✅ — Kolom control account pada daftar akun

| Field | Isi |
|---|---|
| Outcome | `AccChartOfAccount` punya penanda `IsControlAccount`, dan penanda itu dapat diisi lewat API |
| Trace | `ACC-DEC-064`; turunan `ACC-DEC-062` |
| Kontrak | `erd/data-dictionary.md` bagian 1, `ACC-API-0.8` grup Chart of Account |
| Reuse | `AccChartOfAccountService` dan DTO-nya yang sudah ada |
| Cakupan | Satu kolom `bool` pada `AccChartOfAccount`, satu index, penambahan bidang pada `CreateChartOfAccountDto` dan `UpdateChartOfAccountDto`, serta pemetaannya di service |
| Dependency | — |
| Acceptance | (1) Kolom `bool` **tidak nullable**, bawaan `false`. (2) Index `(IsControlAccount)` terpasang. (3) Kedua DTO memuat bidangnya. (4) **Akun yang sudah ada tetap bernilai `false`** tanpa perlu pengisian data. (5) Build lulus |
| Verifikasi | `dotnet build -c Release -p:RunAnalyzers=false`; pembandingan terhadap kamus data bagian 1 |
| Risiko/pemilik | **Ini menyentuh entity Phase 1 yang sudah berjalan.** Kolom wajib berbawaan `false` supaya 2 akun yang sudah ada di database tidak berubah perilakunya. Owner Backend |
| DoD | Build lulus. **Nol migration** — kolomnya ikut `BE-ACC-P2-004` |
| **Status** | **✅ `DONE`** — 9 September 2026. Build `Release` **0 error**, 145 warning seluruhnya pre-existing — angka sama persis dengan sebelum task ini. **5 dari 5 acceptance lulus**, dibuktikan **5 uji baru** (`Failed: 0, Passed: 5`). Seluruh project Sqlite `Failed: 3, Passed: 458` — **nol regresi**, ketiga kegagalan pre-existing di `MedicalRecordManagement`. QBE checker `PASS`, `VIOLATION: 0`. Nol migration, snapshot utuh, database tidak disentuh. Laporan: [`be-acc-p2-011`](../task/report/backend/be-acc-p2-011-kolom-control-account-pada-daftar-akun.md) |
| **Delta kontrak** | Kartu menyebut **dua** DTO; diimplementasikan **tiga** — `ChartOfAccountListResponse` ikut diberi bidangnya, sebab tanpa itu penandanya menjadi hanya-tulis dan `FE-ACC-P2-007` mustahil menampilkannya. `ChartOfAccountOptionResponse` dan penyaring daftar **sengaja ditunda** ke `012`/`013` |
| **✅ Peringatan DITUTUP** | `UpdateChartOfAccountRequest.IsControlAccount` kini `bool?`: kosong berarti **pertahankan nilai tersimpan**, dan melepas penanda menuntut pernyataan tegas `false`. Diperbaiki 9 Sep 2026 atas instruksi owner, supaya permintaan lama yang hanya mengubah nama akun tidak membuka kembali akun kas ke jurnal manual. Isi peringatan aslinya: |
| ~~Peringatan asli~~ | `PUT` yang tidak mengirim `IsControlAccount` akan **melepas** penandanya — perilaku `PUT` yang memang sudah berlaku (`IsPostable` sama), tetapi akibatnya di sini membuka kembali akun kas ke jurnal manual. Bentuk `PATCH /{id}/control-account` tersendiri adalah **keputusan kontrak yang belum diambil**. Menunggu owner |

## `BE-ACC-P2-012` 🟡 — Penolakan jurnal manual ke control account

| Field | Isi |
|---|---|
| Outcome | Jurnal manual yang menunjuk control account ditolak; jalur otomatis tetap lolos |
| Trace | `ACC-DEC-064`; diperluas `ACC-DEC-072` (koreksi) dan `ACC-DEC-073` (template), 11 Sep 2026 |
| Kontrak | `ACC-VALIDATION-0.6` bagian 3b, **diamandemen usulan `ACC-VALIDATION-0.7`**; `ACC-API-0.11` (usulan) untuk `IsControlAccount` pada `/options` dan kode `422` pada grup Journal dan Recurring Journal |
| Reuse | `AccJournalService` dan `AccountingServiceResult<T>` yang sudah ada |
| Cakupan | Penambahan pemeriksaan pada jalur simpan dan ajukan `AccJournalService` |
| **Perluasan cakupan 11 Sep 2026** | Tiga tambahan dari keputusan owner, **tanpa migration**: (1) jalur penyesuaian `ReverseAsync` (`ACC-DEC-072`); (2) simpan, ubah, dan aktifkan pada `AccRecurringJournalService` — kode milik `BE-ACC-P2-007` (`ACC-DEC-073`); (3) bidang `IsControlAccount` pada `ChartOfAccountOptionResponse`, yang ditunda `BE-ACC-P2-011` ke task ini. **Pengecualian dikenali dari asal-usul jurnal, bukan dari kode jenis** — Form Jurnal menerima jenis apa pun, termasuk `JB` dan `JT` |
| Dependency | `BE-ACC-P2-004` ✅ (kolomnya **sudah** ada di database), `BE-ACC-P2-011` ✅ |
| Acceptance | (1) Baris jurnal **manual** ke akun ber-`IsControlAccount = true` ditolak `422`, dan pesannya menyebut akun mana. (2) **Jurnal dari kejadian akuntansi, dari template berulang, dan jurnal penutup tahun TIDAK terkena aturan ini** — justru merekalah jalur yang sah. (3) Akun non-control tetap dapat dijurnal manual seperti biasa. (4) Jurnal manual yang sudah ada sebelumnya tidak ikut ditolak saat diubah, kecuali barisnya menyentuh control account |
| Verifikasi | Test integrasi PostgreSQL untuk keempat acceptance, terutama (2) |
| Risiko/pemilik | **Acceptance (2) adalah yang paling berbahaya bila keliru.** Bila aturan ini ikut mengenai jalur otomatis, seluruh posting dari kejadian akan tertolak dan Phase 2 mati total — tanpa error yang menjelaskan sebabnya. Owner Backend |
| DoD | Endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **🟡 `SEBAGIAN`** — 11 September 2026. Keempat acceptance **terpetakan ke source**: (1) `AccJournalService.AlasanControlAccountAsync` dipasang pada `CreateManualAsync`, `UpdateAsync`, `SubmitAsync`, jalur penyesuaian `ReverseAsync`, serta simpan, ubah, dan aktifkan template; pesannya menyebut kode dan nama akun. (2) Pengecualian diturunkan dari **asal-usul**, bukan kode jenis — `BerasalDariJalurOtomatisAsync`: baris penerbitan template, cermin pembalikan penuh, dan bentuk jurnal `JT` hasil tutup tahun. (3) Akun non-control tidak tersentuh. (4) `UpdateAsync` hanya menolak baris yang menyentuh control account. **Uji manual terhadap PostgreSQL dev, 11 September 2026** — S1–S8 oleh Rizki lewat Swagger, A–D oleh agent lewat API: seluruhnya `PASS`. S6 `reverse` menjawab `200` — **sesuai kontrak dan implementasi**, diputuskan Rizki (dokumentasi saja; penegasan `200` diajukan dalam `ACC-API-0.11`). **Acceptance (1) terbukti di runtime** — Simpan (S1), Ubah (S3), Ajukan (C); **(3) terbukti** — S2, S4, B; **(4) terbukti** — S3, A (baris tidak berubah sesudah `PUT` gagal), B (ubah non-control `200`); **(2) sebagian** — pembalikan penuh saat dibuat dan diajukan ulang (S6, D1) serta draft template (D2) lolos, tetapi jurnal penutup tahun `JT` belum diuji di runtime dan jalur kejadian akuntansi belum ada di kode. **Yang tetap membuat 🟡:** build `Release`, **15 uji SQLite baru** (`AccControlAccountJournalGuardTests`), dan regresi `UnitTests.Sqlite` **belum diverifikasi**; celah `JT` pada (2); kontrak `ACC-VALIDATION-0.7`/`ACC-API-0.11` belum diratifikasi. `JB/2026/09/00002` menunggu persetujuan pengguna kedua sebelum pembuktian penetralan S4. **Keputusan owner 11 September 2026 — status dipisah:** Implementation `COMPLETE` sesuai cakupan yang dibangun · Developer Manual Test `COMPLETED` (S1–S8, A–D) · Automated Verification `DEFERRED` — build `Release`, 15 `AccControlAccountJournalGuardTests`, dan regresi `UnitTests.Sqlite` dijalankan pada pipeline quality/UAT terpisah, tidak dikejar untuk closure · UAT `HANDOFF TO UAT TEAM`. Tanda tetap 🟡 karena DoD formal kartu ini ("test hijau") belum terpenuhi — status `DONE` tidak dipalsukan — tetapi **bukan penghalang** pekerjaan frontend berikutnya. Skenario yang hanya dapat disiapkan lewat setup database invasif (jurnal `JT`, penetralan S4 yang menunggu pengguna kedua) diserahkan sebagai UAT follow-up, tanpa SQL langsung. Laporan: [`be-acc-p2-012`](../task/report/backend/be-acc-p2-012-penolakan-jurnal-manual-ke-control-account.md) |

## `BE-ACC-P2-013` 🟡 — Saldo control account dari buku besar

| Field | Isi |
|---|---|
| Outcome | Satu endpoint yang menghitung saldo tiap control account dari baris jurnal berstatus `Posted` |
| Trace | `ACC-DEC-066` |
| Kontrak | `ACC-API-0.8` grup Reconciliation — **delta kontrak, endpoint baru** |
| Reuse | **`AccChartOfAccountService.HitungSaldoAsync`** yang sudah `public static`, dipakai apa adanya |
| Cakupan | `Services/AccControlAccountReconciliationService.cs`, satu endpoint `GET /reconciliation/gl-balances` |
| Dependency | `BE-ACC-P2-011` ✅ |
| Acceptance | (1) Hanya akun ber-`IsControlAccount = true` yang muncul. (2) Saldo dihitung **hanya dari baris berstatus `Posted`**. (3) Disaring per badan hukum dan per tanggal. (4) `[AccessPermission]` terpasang |
| Verifikasi | Test integrasi PostgreSQL memakai contoh berangka |
| Risiko/pemilik | Menghitung dari baris selain `Posted` akan menghasilkan saldo yang tidak pernah cocok dengan subledger, dan selisihnya akan disalahartikan sebagai cacat data. Owner Backend |
| DoD | Endpoint berjalan, test hijau, laporan task tertulis |
| **Status** | **🟡 `SEBAGIAN`** — 9 September 2026. Endpoint `GET /reconciliation/gl-balances` berjalan. Build `Release` **0 error**, 145 warning (nol baru). **4 dari 4 acceptance terbukti** lewat **9 uji baru** (`Failed: 0, Passed: 9`); seluruh project Sqlite `Failed: 3, Passed: 513` — **nol regresi**. QBE `PASS`. **Yang membuat 🟡:** kolom Verifikasi menuntut **test integrasi PostgreSQL**, dan itu **`NOT RUN`** — contoh berangkanya dijalankan di SQLite. Laporan: [`be-acc-p2-013`](../task/report/backend/be-acc-p2-013-saldo-control-account-dari-buku-besar.md) |
| **Delta pemakaian ulang** | `HitungSaldoAsync` **tidak diubah sedikit pun**, tetapi tidak dipanggil per akun: memanggilnya per akun berarti satu query per control account (N+1), dan fungsi itu juga tidak menerima batas tanggal yang dituntut acceptance (3). Dipakai satu query pengelompokan dengan rumus yang sama, dan **kesamaan angkanya diuji** lewat `AngkanyaSamaDenganHitungSaldoAsync` |
| **Delta kontrak** | Grup Reconciliation belum ada di `ACC-API-0.8` — sudah diantisipasi kartu ini. Bidang tambahan `BalanceInNormalBalance` (akun bersaldo normal kredit tampil positif) dan `PostedLineCount`. **Menunggu ratifikasi** — **diajukan 11 Sep 2026** sebagai `ACC-API-0.11` grup Reconciliation dan `ACC-PERMISSION-0.6` baris `AccountingReconciliation : Read` |
| **Rintangan `ACC-TD-001`** | Empat uji yang menyimpan baris jurnal semula gagal — check constraint `CK_AccJournalLine_TepatSatuSisiTerisi` **mustahil dipenuhi di SQLite** karena EF menyimpan `decimal` sebagai TEXT. Diatasi dengan `PRAGMA ignore_check_constraints` pada koneksi uji saja; **nol perubahan pada model aplikasi** |
| **Prasyarat pemakaian** | **Nol akun bertanda control account** di database. Laporan ini akan kosong sampai Kas Kasir, Kas Kecil, Piutang, dan Hutang dibuat di daftar akun lalu ditandai |

## `BE-ACC-P2-014` — Perbandingan subledger dan laporan selisih

| Field | Isi |
|---|---|
| Outcome | Saldo control account dibandingkan dengan saldo subledger, dan selisihnya dilaporkan |
| Trace | `ACC-DEC-066` |
| Dependency | `BE-ACC-P2-013` 🟡 |
| **Status** | `READY` — blokirnya dibuka 10 September 2026 |
| **Pemblokirnya** | **`DEC-ACC-P2-011` — DITUTUP** oleh `ACC-DEC-071`, 10 September 2026 |
| Keputusan yang mengikat | Finance menerbitkan saldo subledger **final per periode akuntansi** sebagai kejadian, memuat `LegalEntity`, `AccountingPeriod`, `ControlAccount`, `SubledgerBalance`, `AsOfDate`. Accounting **tidak** memanggil API Finance dan **tidak** membaca tabelnya |
| Cakupan yang kini pasti | (1) Menerima dan menyimpan saldo subledger per control account per periode. (2) Membandingkannya dengan saldo buku besar pada periode yang sama. (3) Melaporkan selisihnya. (4) Menyumbang penghalang penutupan periode — lihat `ACC-GAP-013` |
| Dependency baru | **Gelombang `P2-1`** (kotak masuk kejadian), karena saldo subledger tiba sebagai kejadian. Sisi pembandingnya dapat ditulis lebih dulu memakai `BE-ACC-P2-013` yang sudah berdiri |
| Yang tetap dapat berjalan sendiri | **`BE-ACC-P2-013`** — sisi buku besarnya utuh dan sudah berdiri |
| Catatan | Menebak bentuk sumber saldo subledger dulu ditolak justru supaya pembandingnya tidak dibongkar ulang. Bentuknya kini ditetapkan `ACC-DEC-071`, jadi alasan menunggu sudah hilang |

## Yang sengaja tidak ada di roadmap ini

| Yang ditunda | Alasan |
|---|---|
| `AccEventType`, `AccPostingRule`, `AccPostingRuleLine` | Isinya menunggu `DEC-ACC-P2-002` |
| Kotak masuk kejadian, percobaan ulang, pengabaian | Gelombang `P2-1` dan `P2-2` |
| Penyambungan sungguhan ke Finance | `POST-MVP`, menunggu ratifikasi `ACC-XM-001` dan modul Finance berdiri |
