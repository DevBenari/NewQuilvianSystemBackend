# Laporan Perubahan Backend — `BE-FIN-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-005` |
| Judul | Pintu masuk fakta Billing yang idempoten |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-1`) |
| Trace | `FIN-DEC-005` (sisi konsumsi), `FR-FIN-010`..`013`, `FIN-DES-008`, `FIN-DES-009` |
| Contract version | `FIN-INTEGRATION-1.0` — **catatan**: dokumen `contracts/integration-contract.md` tidak memuat heading `§intake` maupun menyebut `FinBillingHandoffIntake` sama sekali; bentuk entity yang sebenarnya berasal dari `02-backend-architecture.md` §3.1/§4.1 dan `erd/data-dictionary.md` §1.1/§9.6. Dicatat sebagai selisih dokumentasi, bukan blocker |
| Dependency | `BE-FIN-004` — 🟡 sebagian 21 September 2026, lihat [laporan](BE-FIN-004.md) |
| Klasifikasi | `LIGHT` — satu repository; 2 berkas dibuat + 2 berkas registrasi diubah; tidak ada logika bisnis (murni entity+configuration); tidak ada endpoint; database hanya penambahan model (migration terpisah `BE-FIN-007`) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/BillingIntake/Models/`, `Repositories/Configurations/Corporate/FinanceManagement/BillingIntake/`, `Repositories/ApplicationDbContext.cs` (registrasi `DbSet` saja) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN — sesuai cakupan roadmap (entity+configuration), bukan kekurangan.** Lihat bagian 1 untuk narasi lengkap kenapa acceptance criteria pada roadmap tidak sepenuhnya terbukti pada slice ini |

---

## 1. Kenapa status ini `🟡`, bukan `✅` — dibaca lebih dulu

Kolom **Cakupan** `BE-FIN-005` pada roadmap hanya menyebut `FinBillingHandoffIntake` + configuration
— persis pola `BE-FIN-002`. Tetapi kolom **Acceptance criteria** dan **Verifikasi** (`UAT-04`) pada
baris yang sama ditulis di level perilaku ("fakta sama dua kali → satu piutang"; "yang berhasil
tidak dapat diulang") yang **tidak bisa dibuktikan** oleh entity+configuration saja. Penelusuran
terhadap dokumen sumber menunjukkan kenapa:

1. **`UAT-04` mensyaratkan `UAT-03` sebagai prasyarat**, dan `UAT-03` sendiri mensyaratkan
   "petugas membuka layar fakta masuk dan menjalankan pengolahan" yang menghasilkan **satu
   piutang** (`FinReceivable`). `FinReceivable` belum ada — itu `BE-FIN-006`, yang secara eksplisit
   mencantumkan `BE-FIN-005` sebagai *dependency*-nya (datang **setelah**, bukan bersamaan).
2. **Migration untuk tabel ini (`AddFinanceBillingIntake`) adalah task terpisah, `BE-FIN-007`**,
   bukan bagian `BE-FIN-005` — sama seperti pemisahan `BE-FIN-002`/`BE-FIN-003`.
3. **Belum ada satu pun task yang secara eksplisit memiliki `FinanceBillingIntakeService`** —
   service yang benar-benar membaca `BilArHandoff`, membuat piutang, dan menandai ACK.
   `02-backend-architecture.md` merancang service ini (`BillingIntake/Services/
   FinanceBillingIntakeService.cs`), tetapi **tidak satu pun** baris Cakupan pada tabel task
   `BE-FIN-005`..`BE-FIN-009` menyebutnya secara eksplisit — `BE-FIN-009` hanya mencantumkan
   `FinanceBillingIntakeController`, `FinanceReceivablesController` (controller, bukan service).
   **Ini gap pada roadmap yang perlu diklarifikasi pemilik blueprint sebelum `BE-FIN-009`
   dikerjakan** — tanpa `FinanceBillingIntakeService` yang eksplisit dijadwalkan, controller pada
   `BE-FIN-009` tidak punya apa pun untuk dipanggil.

Karena itu, pada slice `BE-FIN-005` ini, yang benar-benar dapat dibuktikan **hanya** bagian yang
mekanis-ditegakkan database, bukan seluruh perilaku bisnis pada AC roadmap:

| Bagian AC roadmap | Terbukti pada slice ini? | Bagaimana |
| --- | --- | --- |
| "Fakta sama dua kali → satu piutang" (bagian mencegah baris intake kedua) | **Ya, sebagian** — partial unique index `IX_FinBillingHandoffIntake_Identity` pada `(HandoffType, SourceHandoffKey) WHERE "IsDelete" = false` mencegah dua baris intake untuk fakta yang sama secara mekanis di database | Konfigurasi EF; belum dapat diuji live karena migration belum ada |
| "Fakta sama dua kali → **satu piutang**" (bagian piutang benar-benar satu) | **Tidak** — `FinReceivable` belum ada (`BE-FIN-006`) | — |
| "Kegagalan tersimpan dan dapat diulang" | **Tidak** — perilaku ini adalah logika `FinanceBillingIntakeService` (belum ada task pemiliknya, lihat poin 3 di atas), bukan bentuk kolom | Kolom pendukungnya (`Status`, `RetryCount`, `ErrorMessage`) sudah disiapkan di skema |
| "Yang berhasil tidak dapat diulang" | **Tidak** — idem, logika state machine `CONSUMED`/`ACKNOWLEDGED` → tolak `422` belum ada pemiliknya | Kolom pendukungnya (`Status`) sudah disiapkan |

Pola pelaporan ini — menyempitkan AC roadmap ke apa yang benar-benar terbukti pada slice
entity+configuration, dan mencatat sisanya sebagai belum terpenuhi apa adanya — mengikuti persis
preseden `BE-FIN-002`.

---

## 2. Proses bisnis

`NOT APPLICABLE` untuk task ini secara langsung — tidak ada service atau endpoint yang berjalan.
Proses bisnis yang **akan** dilayani tabel ini (dijelaskan untuk konteks pembaca):

Billing mengirim fakta (tagihan final untuk penjamin, penerimaan, koreksi pasca-final) lewat
`BilArHandoff`/`BilApHandoff`/`BilCollectionHandoff`/`BilHandoffAdjustment`. Setiap fakta akan
dicatat **satu baris** `FinBillingHandoffIntake` dengan kunci idempotensi `SourceHandoffKey`
(disalin dari `HandoffKey` milik Billing). Bila fakta yang sama datang lagi, baris kedua akan
ditolak database sebelum sempat diproses ulang. Baris yang gagal diproses tetap tersimpan
berstatus `ERROR` dengan pesan penyebabnya — bukan hilang — sehingga petugas dapat mengulanginya
setelah penyebabnya diperbaiki (mis. penjamin yang belum terdaftar). Baris yang sudah `CONSUMED`
atau `ACKNOWLEDGED` tidak boleh diproses ulang, mencegah piutang dobel dari satu tagihan.
**Alur pemrosesan sungguhan ini belum dibangun** — itu wewenang task mendatang setelah gap pada
bagian 1 poin 3 diklarifikasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/integration-contract.md` — ditelusuri penuh; tidak ada heading `§intake` atau penyebutan `FinBillingHandoffIntake` (dicatat sebagai selisih dokumentasi pada bagian metadata)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §1.1 (kepemilikan bounded context), §1.2 (tabel kepemilikan data), §2.3 (`FIN-DES-008`, `009`, `023`), §3.1 (class diagram), §4.1 (rincian class `FinBillingHandoffIntake`), daftar service (`FinanceBillingIntakeService`, `FinanceReceivableService`)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §1.1 (kolom) dan §9.6 (DDL lengkap `FinBillingHandoffIntake`)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `EPIC FIN-02`, `FR-FIN-010`..`013`, `UAT-03`, `UAT-04`
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` — `FIN-CAP-001` (`BilArHandoff` ready to reuse), `FIN-CAP-008` (konsumen Finance untuk `BilArHandoff`/`BilApHandoff` — **Missing**, nol hasil grep saat audit kapabilitas)
- `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs` dan `BilArHandoffConfiguration.cs` — bentuk sumber fakta, `HandoffKey` sebagai kunci idempotensi milik Billing
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` — dikonfirmasi tidak ada konsumen Finance apa pun untuk handoff ini saat ini (komentar baris 12-15 di berkas itu sendiri menyatakan hal yang sama)
- `Areas/Corporate/FinanceManagement/MasterData/Models/MstPettyCashCategory.cs` + `MstPettyCashCategoryConfiguration.cs` — template entity+configuration status-as-string yang dipakai ulang (sama seperti `BE-FIN-002`)
- `Repositories/Configurations/Corporate/FinanceManagement/PettyCash/FinPettyCashBudgetConfiguration.cs` — pola `RowVersion` dengan `.IsConcurrencyToken()`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` | **Baru.** Entity + `FinBillingHandoffTypes`/`FinBillingHandoffIntakeStatuses` (status sebagai string, `FIN-DES-004`) |
| `Repositories/Configurations/Corporate/FinanceManagement/BillingIntake/FinBillingHandoffIntakeConfiguration.cs` | **Baru.** Check constraint `HandoffType`/`Status`, unique partial index identitas fakta, index status, `RowVersion` sebagai `IsConcurrencyToken()` |
| `Repositories/ApplicationDbContext.cs` | Registrasi `DbSet<FinBillingHandoffIntake>` + using baru untuk namespace `BillingIntake.Models` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint pada task ini |
| Database | Satu entity baru siap-migration. **Belum ada migration** (`BE-FIN-007`, digabung dengan tabel piutang `BE-FIN-006`) |
| Keamanan/Auth | `NOT APPLICABLE` |

**Ambiguitas kontrak yang dicatat, bukan diputuskan sepihak**: `data-dictionary.md` §1.1 menandai
`SourceHandoffId` dan `CorrelationId` masing-masing dengan anotasi "Index" pada kolom Index,
tetapi DDL bernama §9.6 hanya mendefinisikan **dua** index (`IX_FinBillingHandoffIntake_Identity`
dan `IX_FinBillingHandoffIntake_Status`) — tidak ada index bernama untuk `SourceHandoffId` atau
`CorrelationId` sendiri. Mengikuti preseden `BE-BKC-033` (§3.5 pada `docs/module-blueprints/
billing-kasir/task/report/backend/BE-BKC-033.md`) untuk ambiguitas serupa: **daftar DDL bernama
yang diikuti** karena lengkap dan spesifik, bukan anotasi kolom umum. Bila kelak `FinanceBillingIntakeService`
sering query per `SourceHandoffId`/`CorrelationId`, index tambahan itu sebaiknya ditambahkan saat
service itu dibangun (data akses pattern-nya baru jelas saat itu), bukan dikarang sekarang.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan** | `NOT RUN` | Atas instruksi eksplisit pengguna pada sesi ini (berlaku sejak `BE-FIN-002`); risiko build yang belum diverifikasi kini mencakup 5 task berturut-turut (`BE-FIN-002`..`005`) |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `Files evaluated: 19`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan di bawah |
| Review manual: kolom/tipe/default/check constraint/index pada configuration dicocokkan satu-per-satu terhadap DDL `data-dictionary.md` §9.6 | Cocok persis — `HandoffType`/`Status` `varchar(30)`, `Status` default `'NEW'`, `RetryCount` default `0`, `ErrorMessage` `varchar(1000)`, kedua check constraint dan kedua index bernama sama persis | `PASS` | Perbandingan manual pada sesi ini, bagian 3.1–3.2 |
| Review manual: `SourceHandoffId` tidak dijadikan FK | Dikonfirmasi — tidak ada `HasOne`/`HasForeignKey` pada configuration untuk kolom ini, sesuai `02-backend-architecture.md` §4.1 "Tidak punya navigation ke tabel Billing" | `PASS` | `FinBillingHandoffIntakeConfiguration.cs` |
| Review governance: submodule `BillingIntake` sudah terdaftar registry sebelum file model ini ditulis | Terpenuhi — baris `FinanceManagement / BillingIntake / Billing Intake` sudah ditambahkan `BE-FIN-001` (21 September 2026) | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; checker `QBE-MOD-002`/`003` `0 VIOLATION` |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 19
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis.

Uji manual: `NOT APPLICABLE` — tidak ada endpoint/UI untuk task murni entity+configuration ini.

**Tidak dijalankan:** `dotnet build` (lihat tabel), migration/eksekusi database (`NOT APPLICABLE`,
migration adalah `BE-FIN-007` terpisah).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Fakta sama dua kali → satu piutang | 🟡 **Sebagian** — pencegahan baris intake kedua terbukti mekanis (unique index); "satu piutang" tidak dapat dibuktikan karena `FinReceivable` belum ada (`BE-FIN-006`) | Bagian 1 |
| Kegagalan tersimpan dan dapat diulang | **Belum** — kolom pendukung sudah ada; logika retry adalah milik `FinanceBillingIntakeService` yang belum punya task pemilik eksplisit | Bagian 1 poin 3 |
| Yang berhasil tidak dapat diulang | **Belum** — idem, logika state machine belum ada | Bagian 1 poin 3 |
| `UAT-04` | **Belum dapat dijalankan** — bergantung pada `UAT-03`, `BE-FIN-006`, `BE-FIN-007`, dan service yang belum ada | Bagian 1 |
| DoD: `Serializable` | `NOT APPLICABLE` pada slice ini — belum ada transaksi/service yang perlu isolation level; berlaku untuk task pemilik `FinanceBillingIntakeService` nanti | — |
| DoD: `Idempotency-Key` (header HTTP) | `NOT APPLICABLE` pada slice ini — belum ada endpoint; `SourceHandoffKey` (kolom data, bukan header) sudah tersedia sebagai kunci idempotensi | Model `FinBillingHandoffIntake.cs` |

**Kesimpulan status**: `🟡` bukan karena ada pekerjaan yang gagal, melainkan karena AC dan
Verifikasi pada baris roadmap `BE-FIN-005` ditulis di level epic/wave (`MVP-1` "Selesai bila
`UAT-03` dan `UAT-04` lulus"), sementara Cakupan task ini sendiri sengaja sempit
(entity+configuration). Kriteria yang genuinely menjadi tanggung jawab slice ini — bentuk kolom,
check constraint, dan index identitas — seluruhnya terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Gap roadmap yang perlu diklarifikasi pemilik blueprint sebelum `BE-FIN-009` dimulai**: tidak ada task yang secara eksplisit memiliki `FinanceBillingIntakeService` pada Cakupan-nya (lihat bagian 1 poin 3). Tanpa kejelasan ini, `BE-FIN-009` ("API intake dan piutang") tidak punya service untuk dipanggil controller-nya |
| Masalah yang diketahui | `contracts/integration-contract.md` tidak memuat bentuk `FinBillingHandoffIntake` sama sekali walau roadmap merujuknya sebagai `FIN-INTEGRATION-1.0 §intake` — bentuk sebenarnya hanya ada di `02-backend-architecture.md`/`erd/data-dictionary.md`. Disarankan pemilik blueprint menambahkan section intake yang hilang itu ke `integration-contract.md` agar rujukan roadmap valid |
| Risiko tersisa | Rendah untuk task ini sendiri (murni tambahan skema, aditif). Risiko build tetap bertumpuk sejak `BE-FIN-002` — lihat peringatan pada laporan `BE-FIN-004` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | `git status --short` menunjukkan berkas `BE-FIN-002`..`004` yang sudah ada, ditambah 2 berkas baru (`FinBillingHandoffIntake.cs`, `FinBillingHandoffIntakeConfiguration.cs`) dan 1 berkas berubah (`ApplicationDbContext.cs`) untuk task ini |
| Langkah berikutnya | (1) Klarifikasi pemilik blueprint atas gap `FinanceBillingIntakeService` (bagian 1 poin 3) sebelum `BE-FIN-009`. (2) `dotnet build` mencakup `BE-FIN-002`..`005`. (3) Lanjut `BE-FIN-006` (`FinReceivable` dan tabel piutang lain) — dependency-nya `BE-FIN-005`, yang sudah terpenuhi untuk bagian yang menjadi tanggung jawabnya |
