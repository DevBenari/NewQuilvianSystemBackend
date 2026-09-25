# Laporan Perubahan Backend — `BE-FIN-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-011` |
| Judul | Penulisan outbox ikut transaksi pemanggil — `FinanceAccountingOutboxService` |
| Slice | `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-5`) |
| Trace | `FIN-DES-017`..`019`; `FR-FIN-070`..`072`; kontrak `FIN-INTEGRATION-1.0` |
| Contract version | `NOT APPLICABLE` — tidak ada Controller/DTO API pada task ini |
| Dependency | `BE-FIN-010` — 🟡 sebagian 21 September 2026 (entity+configuration+migration tertulis, belum dijalankan), lihat [laporan](BE-FIN-010.md) |
| Klasifikasi | `HEAVY` — mengubah alur transaksi 2 service yang sudah selesai (`BE-FIN-008`/`009`); **perluasan lingkup atas otorisasi eksplisit** (bagian 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` (baru); `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (disunting); `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (disunting); `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** `FinanceAccountingOutboxService` selesai penuh sesuai Cakupan literal roadmap. Tiga pemanggil nyata ditambahkan atas otorisasi eksplisit (bagian 0): `FinanceBillingIntakeService.ProcessArIntakeAsync` (`PENGAKUAN-PIUTANG`), `FinanceReceivableService.DecideAdjustmentAsync` approve (`PENYESUAIAN-PIUTANG`), `DecideWriteOffAsync` approve (`PEMUTIHAN-PIUTANG`). `dotnet build` pengguna (22 September 2026) sempat gagal 2 error milik berkas ini (`legalEntityId.Value` pada `Guid` non-nullable) — sudah diperbaiki, dan build ulang PASS dikonfirmasi pengguna 23 September 2026 (lihat Pembaruan bagian 7). Beberapa keterbatasan desain tetap dicatat eksplisit (bagian 1), tidak menahan status |

---

## 0. Otorisasi perluasan lingkup

Cakupan literal roadmap `BE-FIN-011` hanya menyebut satu berkas: `FinanceAccountingOutboxService`.
Tanpa pemanggil nyata, kotak keluar tidak akan pernah terisi kejadian sungguhan, dan
acceptance criteria `BE-FIN-010`/`011` ("Fakta dan kejadian tersimpan atau batal bersama") serta
`UAT-17` ("tepat satu kejadian pengakuan piutang") tidak dapat dibuktikan. Pertanyaan ini diajukan
eksplisit kepada pemilik repository sebelum menyunting berkas manapun di luar Cakupan literal;
jawaban yang diterima: **"Service + wiring ke alur piutang yang sudah ada"** — memanggil dari
`FinanceBillingIntakeService.ProcessArIntakeAsync` dan `FinanceReceivableService` (approve
koreksi/write-off), meski menyentuh dua berkas yang sudah selesai di `BE-FIN-008`/`009`. Pola
otorisasi ini identik dengan "Bangun semuanya" pada `BE-FIN-009`.

---

## 1. Keputusan desain dan keterbatasan (didokumentasikan, bukan didiamkan)

### 1.1 `LegalEntityId` diselesaikan lewat `AccountingLegalEntityGuard` (dipakai ulang, bukan dikarang)

`FinReceivable`/`BilArHandoff` tidak pernah menyimpan `LegalEntityId` — digrep di seluruh
`Areas/HealthServices` dan `Areas/Corporate/FinanceManagement`, nol hasil. Skema outbox
(`erd/data-dictionary.md` §7.1) mewajibkannya sebagai "Rujukan shared". Ditemukan
`Areas/Corporate/AccountingManagement/Services/AccountingLegalEntityGuard.cs` — mekanisme MVP
yang sudah dipakai seluruh service Accounting untuk persoalan identik (`ACC-DEC-041`/`043`:
tepat satu `MstLegalEntity.IsDefault` sebagai satu-satunya badan hukum yang dipakai selama
penyaringan per pengguna belum ada). `FinanceAccountingOutboxService.StageEventAsync` memakainya
langsung (`AccountingLegalEntityGuard.AmbilBadanHukumUtamaAsync`) alih-alih membuat mekanisme
Finance sendiri yang akan berkonflik konsep dengan punya Accounting. Bila tidak tepat satu badan
hukum default, `StageEventAsync` **throw** — karena berada di transaksi yang sama dengan fakta
bisnisnya (FIN-DES-017), ini otomatis membatalkan penyimpanan fakta juga, persis konsekuensi yang
dituntut `FR-FIN-070` ("piutang tanpa kejadian tidak boleh terjadi") — bukan perilaku yang
ditoleransi secara diam-diam, melainkan akibat langsung dari invariant yang sama.

### 1.2 `PayloadJson` dibangun oleh service, bukan diterima dari pemanggil

`AccountingOutboxEventRequest` **tidak** memiliki field `PayloadJson` bertipe bebas.
`StageEventAsync` membangun `PayloadJson` sendiri dari 12 field wajib `ACC-XMOD-0.2` (§5.2
`integration-contract.md`) + `Components` opsional. Ini desain yang lebih ketat dari sekadar
"pemanggil wajib tidak menyisipkan data pasien" (yang bergantung disiplin tiap pemanggil) —
pemanggil secara struktural **tidak diberi jalan** memasukkan field bebas apa pun ke payload,
sehingga `FR-FIN-073` (data pasien/`DoctorId` tidak pernah ikut) terjamin di level tipe, bukan di
level konvensi. Ini keputusan desain yang mengunci lebih ketat dari kontrak minimum, bukan
penyimpangan darinya — dicatat di sini untuk transparansi karena bukan pola yang eksplisit
diminta roadmap.

### 1.3 `SourceVersion` dihitung otomatis (`ResolveNextSourceVersionAsync`)

Roadmap DoD: "Koreksi memakai versi baru, bukan menimpa" (`FR-FIN-072`). `SourceVersion` pada
`AccountingOutboxEventRequest` bertipe nullable — `null` berarti "hitung otomatis": query
`MAX(int.Parse(SourceVersion)) + 1` atas baris `FinAccountingEventOutbox` yang sudah ada untuk
pasangan `(SourceTransactionId, EventTypeCode)` yang sama. Ini **bukan** pola Count/Max/Last+1
yang dilarang `QBE-CODE-002/003` untuk **nomor tampilan** (mis. `ReceivableNumber`) — perbedaannya:
(a) ini versi internal, bukan nomor yang dilihat pengguna; (b) pemanggilnya (`DecideAdjustmentAsync`/
`DecideWriteOffAsync`) sudah memegang `pg_advisory_xact_lock` atas `ReceivableId` yang sama
**sebelum** memanggil `StageEventAsync`, sehingga race yang jadi alasan larangan itu tidak berlaku
di sini — perhitungannya sudah di bawah kunci eksklusif. Keterbatasan yang diketahui: query ini
hanya melihat baris yang **sudah tersimpan** di database, tidak baris yang baru `Add()` tapi belum
`SaveChangesAsync` pada unit-of-work yang sama — aman untuk seluruh pemanggil task ini (masing-masing
hanya memanggil `StageEventAsync` sekali per transaksi), tapi perlu diperhatikan bila kelak ada
pemanggil yang memanggilnya dua kali sebelum `SaveChangesAsync`.

### 1.4 Arah koreksi (DEBIT/CREDIT) tidak terbawa ke kejadian

`FinAccountingEventOutbox.Amount` selalu nilai positif (`adjustment.Amount`, sudah divalidasi > 0
sejak `BE-FIN-008`) — skema outbox tidak punya kolom arah/tanda, dan `EventTypeCode`
`PENYESUAIAN-PIUTANG` yang sama dipakai untuk kedua arah (`FinReceivableAdjustmentDirections.Debit`
maupun `Credit`). Ini melanjutkan keterbatasan yang sudah dicatat `BE-FIN-008` ("matematika arah
DEBIT diinferensi, bukan didokumentasikan") — Accounting perlu tahu arah ini untuk memetakan ke
akun debit/kredit yang benar saat aturan posting dirancang, tetapi kontrak `ACC-XMOD-0.2` yang
terkunci belum punya field untuk itu. **Tidak diselesaikan** pada task ini — memerlukan keputusan
bersama Accounting di luar wewenang implementasi.

### 1.5 `HELD_FOR_FINALIZATION` dibangun sebagai kapabilitas, belum ada pemanggil

`AccountingOutboxEventRequest.RequiresFinalization` (FIN-DES-018) diimplementasikan penuh di
`StageEventAsync`, tetapi ketiga pemanggil task ini (`PENGAKUAN-PIUTANG`, `PENYESUAIAN-PIUTANG`,
`PEMUTIHAN-PIUTANG`) semuanya bukan kejadian penerimaan (`accounting-integration.md` §4 mengikat
status ini khusus pada kejadian penerimaan sebelum tagihan final) — ketiganya memanggil dengan
`RequiresFinalization = false` (default). Kapabilitas ini baru akan punya pemanggil nyata saat
`FinReceipt`/Collection dibangun (`MVP-2`/`3`, **BLOCKED** menunggu owner Billing).

### 1.6 `AccountingOutboxException` tidak dipetakan eksplisit di `FinanceReceivablesController`

Bila `StageEventAsync` gagal (mis. badan hukum default tidak tepat satu — sangat jarang, butuh
intervensi master data), pengecualian menembus `catch` generik `DecideAdjustmentAsync`/
`DecideWriteOffAsync` (rollback, lalu `throw`), lalu **tidak** tertangkap `IsHandled` di
`FinanceReceivablesController` — jatuh ke penanganan default ASP.NET Core (umumnya `500`). Ini
disengaja: memperluas `IsHandled`/`Failure` controller berada di luar Cakupan `BE-FIN-011`
("`FinanceAccountingOutboxService`" saja), dan kegagalan ini seharusnya sangat jarang. Berbeda
dengan jalur `FinanceBillingIntakeService.ProcessAsync`, yang **sudah** menangkap seluruh
pengecualian tak terduga dan mengubahnya jadi status `ERROR` yang terlihat (`FR-FIN-011`) — jalur
itu otomatis menangani `AccountingOutboxException` dengan baik tanpa perubahan tambahan.

---

## 2. Ringkasan pekerjaan

### 2.1 `FinanceAccountingOutboxService` (baru)

Satu method publik, `StageEventAsync`: memvalidasi request minimal, meresolusi
`LegalEntityId` (1.1) dan `SourceVersion` (1.3), membangun `PayloadJson`/`ComponentsJson` (1.2),
lalu **hanya** `_dbContext.Set&lt;FinAccountingEventOutbox&gt;().Add(entity)` — **tidak** memanggil
`SaveChangesAsync`, `BeginTransactionAsync`, `CommitAsync`, atau `RollbackAsync` sendiri. Komentar
kelas menegaskan kontrak ini eksplisit sebagai kesalahan paling mudah terjadi menurut roadmap
("Yang mudah salah": service membuka transaksi sendiri).

### 2.2 Tiga titik pemanggilan

| Pemanggil | EventTypeCode | Kapan | SourceTransactionId |
| --- | --- | --- | --- |
| `FinanceBillingIntakeService.ProcessArIntakeAsync` | `PENGAKUAN-PIUTANG` | Setiap `FinReceivable` baru berhasil diakui dari fakta AR Billing | `receivable.ReceivableNumber` |
| `FinanceReceivableService.DecideAdjustmentAsync` (approve) | `PENYESUAIAN-PIUTANG` | Koreksi piutang **disetujui** (bukan diajukan, bukan ditolak) | `receivable.ReceivableNumber` |
| `FinanceReceivableService.DecideWriteOffAsync` (approve) | `PEMUTIHAN-PIUTANG` | Penghapusan piutang **disetujui** | `receivable.ReceivableNumber` |

Ketiganya dipanggil **sebelum** `SaveChangesAsync` milik method yang sudah ada, di dalam
transaksi (`BeginTransactionAsync`/`CommitAsync`) yang sudah ada sejak `BE-FIN-008`/`009` — tidak
ada transaksi baru yang dibuka untuk task ini.

### 2.3 Registrasi DI

`FinanceAccountingOutboxService` didaftarkan `AddScoped` di
`BillingManagementServiceCollectionExtensions.cs`, lalu di-inject ke konstruktor
`FinanceReceivableService` dan `FinanceBillingIntakeService` — keduanya berbagi `ApplicationDbContext`
scoped yang sama lewat DI standar ASP.NET Core, sehingga `Add()` di service outbox dan
`SaveChangesAsync()` di service pemanggil beroperasi pada satu unit-of-work yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (baris `BE-FIN-011`) dan bagian 4 (`MVP-5`)
- `docs/module-blueprints/finance-management/erd/accounting-integration.md` §3 (dua lapis anti-dobel), §4 (`HELD_FOR_FINALIZATION`), §5 (larangan data pasien) — dibaca ulang dari riset `BE-FIN-010`
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §5.2 (12 field wajib), §5.4 (katalog 17 kode)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `FR-FIN-070`..`072`, `UAT-17`..`19`
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`, `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (`BE-FIN-008`/`009`) — dibaca penuh untuk menentukan titik sisip yang tidak mengubah perilaku yang sudah ada
- Grep `LegalEntityId` di `Areas/HealthServices` (nol hasil) dan `Areas/Corporate` (157 berkas, seluruhnya HumanResource/AccountingManagement) — mengonfirmasi Finance/Billing tidak punya sumber sendiri
- `Areas/Corporate/AccountingManagement/Services/AccountingLegalEntityGuard.cs` — mekanisme yang dipakai ulang (bagian 1.1)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` — pola `IsHandled`/`Failure`, dikonfirmasi `AccountingOutboxException` tidak masuk daftar (bagian 1.6)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` | **Baru.** `StageEventAsync` + `AccountingOutboxEventRequest`, `AccountingEventComponent`, `AccountingOutboxException` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | Constructor +`FinanceAccountingOutboxService`; `DecideAdjustmentAsync`/`DecideWriteOffAsync` memanggil `StageEventAsync` pada cabang `approve` |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Constructor +`FinanceAccountingOutboxService`; `ProcessArIntakeAsync` memanggil `StageEventAsync` setelah `FinReceivables.Add` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `using` baru + `AddScoped&lt;FinanceAccountingOutboxService&gt;()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller/DTO API baru |
| Database | Tidak ada perubahan skema — task ini hanya menulis BARIS ke tabel yang sudah dibuat `BE-FIN-010` (belum ada di database sungguhan, migration belum dijalankan) |
| Keamanan/Auth | `NOT APPLICABLE`. Catatan kepatuhan: `PayloadJson` dijamin bebas data pasien secara struktural (bagian 1.2) |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Dijalankan pengguna (22 September 2026, `-c Release`). Dua dari lima error build ada di `FinanceAccountingOutboxService.cs` — `CS1061 'Guid' does not contain a definition for 'Value'` baris 49 dan 63 (`legalEntityId` sudah non-nullable `Guid` sejak pola `?? throw` pada baris 40; `.Value` sisa dari draf sebelumnya). Diperbaiki: kedua `.Value` dihapus, dipakai `legalEntityId` langsung. Tiga error lain (`BilConsumerHandoffService.cs`, `BillingFinancialExceptionService.cs` x2) berada di luar cakupan task ini — lihat catatan bagian 7 | `NOT RUN OLEH SAYA — 2 DARI 5 ERROR MILIK TASK INI, SUDAH DIPERBAIKI` | Pesan build pengguna; `FinanceAccountingOutboxService.cs` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan bagian bawah | `PASS` | Dijalankan latar belakang, hasil disalin apa adanya |
| Review manual: `StageEventAsync` tidak memanggil `SaveChangesAsync`/`BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` | Dikonfirmasi — hanya `Add()` | `PASS` | `FinanceAccountingOutboxService.cs` |
| Review manual: ketiga pemanggil menaruh `StageEventAsync` sebelum `SaveChangesAsync` milik transaksi yang sudah ada, tidak membuka transaksi baru | Dikonfirmasi pada ketiga titik (bagian 2.2) | `PASS` | `FinanceReceivableService.cs`, `FinanceBillingIntakeService.cs` |
| "Uji integrasi rollback" (kolom Verifikasi roadmap) | **Tidak dijalankan** — backend tidak memelihara automated test project (`TEST_POLICY.md`); diverifikasi lewat review kode alih-alih: bila `SaveChangesAsync` gagal setelah `StageEventAsync` (mis. `DbUpdateConcurrencyException`), baris `Add()` yang belum tersimpan otomatis batal bersama seluruh perubahan lain dalam transaksi yang sama, karena EF Core hanya menandai perubahan dan belum benar-benar menulis ke database sampai `SaveChangesAsync` — tidak ada tulisan parsial yang mungkin terjadi | `REVIEW ONLY` | Bagian 2.1, kode `StageEventAsync` |
| `UAT-17`/`18`/`19` | **Tidak dapat dijalankan** — memerlukan database sungguhan dengan migration `BE-FIN-010` diterapkan, di luar wewenang task ini | `NOT RUN` | Bagian 0, `BE-FIN-010` bagian 5 |
| Review manual: `PayloadJson` hanya memuat 12 field wajib + Components, nol field bebas dari pemanggil | Dikonfirmasi — `AccountingOutboxEventRequest` tidak punya properti `PayloadJson` | `PASS` | Bagian 1.2, kode `AccountingOutboxEventRequest` |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — migration `BE-FIN-010` belum dijalankan, tidak ada database untuk
diuji end-to-end.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh setelah wiring `BE-FIN-011` selesai):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 47
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Service **tidak** membuka transaksi sendiri | **Terpenuhi** | Bagian 2.1, review manual bagian 5 |
| Kejadian tertahan tidak terkirim | **Terpenuhi secara struktural** — `DeliveryStatus` dihitung benar dari `RequiresFinalization`; belum ada pemanggil yang mengaktifkannya (bagian 1.5) karena belum ada worker pengirim (`BE-FIN-012`+`EPIC FIN-12`, di luar lingkup) | Bagian 1.5 |
| DoD: koreksi memakai versi baru, bukan menimpa | **Terpenuhi** — `ResolveNextSourceVersionAsync` (bagian 1.3) | Bagian 1.3 |
| Cakupan `FinanceAccountingOutboxService` | **Terpenuhi**, diperluas ke 3 titik pemanggilan atas otorisasi eksplisit (bagian 0) | Bagian 2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi `dotnet build` PASS (termasuk 2 error yang sempat dilaporkan 22 September, sudah diperbaiki dan lulus build ulang), migration `BE-FIN-010` diterapkan, dan endpoint diuji langsung. Status task dinaikkan menjadi ✅ SELESAI |
| Peringatan | (1) Arah DEBIT/CREDIT koreksi tidak terbawa ke kejadian Accounting (bagian 1.4) — perlu keputusan bersama Accounting. (2) `AccountingOutboxException` jatuh ke `500` generik di `FinanceReceivablesController` (bagian 1.6) |
| Masalah yang diketahui | Tidak ada yang baru di luar yang sudah dicatat `BE-FIN-005`..`010`. Build pengguna 22 September 2026 juga melaporkan 3 error **di luar cakupan task ini**, milik modul `billing-kasir` (task `BE-BKC-062`/`067` dari sesi lain, belum pernah dikompilasi sebelumnya): `BilConsumerHandoffService.cs(249)` dan `BillingFinancialExceptionService.cs(465, 720)`. Diperbaiki atas otorisasi eksplisit pemilik repository meski di luar wewenang `BE-FIN-*` — didokumentasikan di laporan pemilik aslinya masing-masing ([BE-BKC-067](../../../../billing-kasir/task/report/backend/BE-BKC-067.md), [BE-BKC-062](../../../../billing-kasir/task/report/backend/be-bkc-062-penyelarasan-jalur-deposit-dan-pengecualian-finansial.md)), bukan di sini |
| Risiko tersisa | **Sedang** — belum bisa diverifikasi end-to-end (menunggu migration `BE-FIN-010` dijalankan); logika transaksi sudah direview manual tapi belum diuji terhadap database sungguhan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 1 berkas baru (`FinanceAccountingOutboxService.cs`), 3 berkas disunting (`FinanceReceivableService.cs`, `FinanceBillingIntakeService.cs`, `BillingManagementServiceCollectionExtensions.cs`) — di atas berkas `BE-FIN-002`..`010` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` oleh pengguna mencakup task ini. (2) Otorisasi eksekusi migration `BE-FIN-010` supaya `UAT-17`..`19` dapat dibuktikan sungguhan. (3) `BE-FIN-012` (`FinanceAccountingEventsController`, pantauan baca-saja) dapat mulai — bergantung pada tabel yang sudah ada, tidak perlu menunggu wiring tambahan |
