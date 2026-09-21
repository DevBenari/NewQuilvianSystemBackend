# Laporan Perubahan Backend — `BE-FIN-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-012` |
| Judul | Pantauan kejadian Accounting, baca saja — `FinanceAccountingEventsController` |
| Slice | `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-5`) |
| Trace | `FR-FIN-074`; kontrak `FIN-API-1.0`, `FIN-PERM-1.0` |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API bertversi eksplisit untuk permukaan ini; endpoint diturunkan dari `transaction-endpoint-standard.md` §2.4 dan `03-frontend-architecture.md` §3.5 |
| Dependency | `BE-FIN-011` — 🟡 sebagian 21 September 2026 (service + 3 pemanggilan selesai, QBE `PASS`), lihat [laporan](BE-FIN-011.md) |
| Klasifikasi | `LIGHT` — permukaan baru murni, tidak menyunting service/controller yang sudah ada (kecuali registrasi DI) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` (baru); `.../DTOs/AccountingEventDtos.cs` (baru); `.../Services/FinanceAccountingEventService.cs` (baru); `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN — kode selesai, QBE `PASS` (50 berkas, 0 pelanggaran); menunggu `dotnet build` dan migration `BE-FIN-010` dijalankan sebelum dapat diuji terhadap database sungguhan.** Seluruh empat endpoint baca (`filters/metadata`, `summary`, daftar, detail) selesai sesuai arketipe monitoring/read-only. |

---

## 0. Otorisasi perluasan lingkup

`NOT APPLICABLE`. Cakupan literal roadmap ("Pantauan kejadian (baca saja)", `FinanceAccountingEventsController`) dikerjakan sebagaimana tertulis — tidak ada berkas di luar submodule `AccountingIntegration` dan satu baris registrasi DI yang disentuh. Penambahan berupa permukaan teknis (filter, sort, field detail) dicatat sebagai delta kontrak pada bagian 1, bukan perluasan lingkup yang menuntut sign-off, sesuai `build-module-backend` langkah 4 ("menambah permukaan teknis boleh, mengarang kebijakan tidak").

---

## 1. Keputusan desain dan delta kontrak (didokumentasikan, bukan didiamkan)

### 1.1 Arketipe: monitoring/laporan baca-saja — hanya 4 `GET`, nol endpoint pengirim

Ditentukan lebih dulu sesuai `build-module-backend` langkah 4: kapabilitas ini bukan aggregate
ber-lifecycle (baris outbox tidak punya aksi milik penggunanya sendiri) dan bukan worklist biasa
(tidak ada aksi `POST /{id}/<aksi>` yang sah dijalankan pengguna Finance atas baris outbox).
Dipetakan ke `transaction-endpoint-standard.md` §2.4 — "Monitoring dan laporan read-only: hanya
`GET`, tanpa satu pun endpoint yang mengubah data". Roadmap DoD menegaskan hal yang sama secara
eksplisit: **"Tanpa endpoint pengirim — itu `EPIC FIN-12`"**. `FinanceAccountingEventsController`
karena itu hanya punya `GET /filters/metadata`, `GET /summary`, `GET /`, `GET /{id}` — nol `POST`,
`PATCH`, `PUT`, `DELETE`. `EPIC FIN-12` (pengiriman/retry) tetap `OPEN DECISION`, menunggu endpoint
penerima Accounting (`FIN-CAP-018`) yang belum dibangun (`erd/accounting-integration.md` §6).

### 1.2 Empat baseline baca diwarisi dari master data (§4 standar transaksi)

`GET /filters/metadata`, `GET /summary`, `GET /` (`PagedResult<T>`), `GET /{id}` — persis pola
yang sudah dipakai `FinanceReceivablesController` dan `FinanceBillingIntakeController`. Dibungkus
`ApiResponse<T>`, route bertversi (`api/v1/...`), `[Authorize]`, `[AccessController]`,
`[AccessAction]`, `[AccessPermission]` — sama persis.

### 1.3 Ringkasan memisahkan keenam `DeliveryStatus`, tidak melumpuhkannya jadi kategori umum

`03-frontend-architecture.md` §3.5 secara eksplisit **MUST**: "Layar pemantauan kejadian **MUST**
membedakan `HELD_FOR_FINALIZATION` dari `HELD` dan `FAILED`, karena tindakan penggunanya berbeda:
yang pertama menunggu Billing, yang kedua menunggu Accounting, yang ketiga menunggu Finance
sendiri." `AccountingEventSummaryResponse` karena itu punya field terpisah untuk keenam status
(`Pending`, `HeldForFinalization`, `Sent`, `Acknowledged`, `Held`, `Failed`), bukan agregat
"tertahan" tunggal — memenuhi kebutuhan itu di level kontrak, bukan diserahkan ke frontend untuk
menghitung ulang.

### 1.4 Detail (`GET /{id}`) menyertakan `PayloadJson`/`ComponentsJson`/`Attempts`/`CausationId`/`LegalEntityId`; daftar (`GET /`) tidak

Delta kontrak di luar cakupan literal roadmap, ditambahkan sebagai permukaan teknis (bukan
kebijakan bisnis baru): daftar hanya memuat kolom ringkas untuk tabel/queue (identitas, status,
jumlah percobaan, kode balasan terakhir), sedangkan detail menyertakan payload lengkap dan riwayat
`FinAccountingEventAttempt` (diurutkan `AttemptNumber` menurun, terbaru dulu) supaya staf
Accounting/Auditor bisa menelusuri **mengapa** satu baris tertahan/gagal tanpa endpoint tambahan.
Aman ditampilkan karena `PayloadJson`/`ComponentsJson` sudah dijamin bebas data pasien secara
struktural sejak `BE-FIN-011` bagian 1.2 (`AccountingOutboxEventRequest` tidak punya field bebas).
`LegalEntityId` bukan data sensitif (rujukan badan hukum bersama, bukan identitas pasien).

### 1.5 Filter `Search` mencocokkan `EventNumber` **atau** `SourceTransactionId` (bukan literal roadmap, additive)

Tanpa filter pencarian, staf Accounting yang menelusuri satu transaksi Finance tertentu (mis. dari
laporan piutang) harus menyisir seluruh halaman. Dipakai `EF.Functions.ILike` (pola yang sudah ada
di `BillingInvoiceService`, modul Laboratory) — case-insensitive, `contains`. Tidak ada aturan
bisnis yang perlu diputuskan pemilik; murni permukaan teknis tambahan.

### 1.6 Opsi filter `EventTypeCode` bersumber dari katalog 17 kode `FIN-DEC-002` yang belum diratifikasi

Melanjutkan catatan `BE-FIN-010` bagian 1.1: `EventTypeCode` tidak dibatasi check constraint di
database, sehingga opsi filter ini adalah **starting point** yang bisa berubah begitu Accounting
meratifikasi katalog resminya — bukan sumber kebenaran yang mengikat.

### 1.7 `AccessController.ControllerName = "AccountingEvents"`, berbeda dari nama folder `AccountingIntegration`

Folder `AccountingIntegration` menaungi `FinAccountingEventOutbox`/`Attempt` (`BE-FIN-010`) dan
`FinanceAccountingOutboxService` (`BE-FIN-011`, bukan controller) — belum tentu satu-satunya
controller yang akan lahir di submodule ini (`EPIC FIN-12` berpotensi menambah kontrol pengiriman
di kemudian hari). Nama resource akses (`ControllerName`, argumen ke-1 `[AccessPermission]`)
karena itu mengikuti cakupan controller ini secara spesifik ("Accounting Events"), bukan nama
folder/submodule — konsisten dengan pola `Receivable`/`BillingIntake` yang menamai berdasarkan
kapabilitas, bukan lapisan fisik.

---

## 2. Ringkasan pekerjaan

### 2.1 `FinanceAccountingEventService` (baru) — seluruhnya baca

| Method | Membaca | Catatan |
| --- | --- | --- |
| `GetPagedAsync` | `FinAccountingEventOutboxes` | Filter `DeliveryStatus`, `EventTypeCode`, `Search`; sort `eventOccurredAt` (default, desc), `deliveryStatus`, `amount`, `createDateTime` |
| `GetByIdAsync` | `FinAccountingEventOutboxes` + `Include(Attempts)` | 404 bila tidak ditemukan atau `IsDelete` |
| `GetSummaryAsync` | `FinAccountingEventOutboxes` (`GroupBy DeliveryStatus`) | Enam counter terpisah (bagian 1.3) |
| `GetFilterMetadataAsync` | Konstanta statis | `PageSizeOptions`, `SortableFields`, `DeliveryStatusOptions` (6), `EventTypeCodeOptions` (17, bagian 1.6) |

Tidak ada method yang memanggil `Add`/`Update`/`Remove`/`SaveChangesAsync` — diverifikasi manual,
lihat bagian 5.

### 2.2 `FinanceAccountingEventsController` (baru)

`GET /filters/metadata`, `GET /summary`, `GET /` (`PagedResult<AccountingEventResponse>`),
`GET /{id}` (`AccountingEventDetailResponse`) — route `api/v1/corporate/finance-management/accounting-events`,
persis pola `FinanceReceivablesController`/`FinanceBillingIntakeController` (`ApiResponse<T>`,
`[AccessController]`/`[AccessAction]`/`[AccessPermission]`, `try/catch KeyNotFoundException → 404`).

### 2.3 Registrasi DI

`FinanceAccountingEventService` didaftarkan `AddScoped` di
`BillingManagementServiceCollectionExtensions.cs`, bersebelahan dengan
`FinanceAccountingOutboxService`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (baris `BE-FIN-012`) dan bagian 4 (`MVP-5`)
- `docs/module-blueprints/finance-management/03-frontend-architecture.md` §3.5, §7 — kebutuhan layar pemantauan kejadian dan larangan data pasien/`DoctorId` di layar ini
- `docs/module-blueprints/finance-management/erd/accounting-integration.md` §4 (`HELD_FOR_FINALIZATION`), §6 ("Layar pemantauan outbox — Boleh dibangun sekarang")
- `.../references/transaction-endpoint-standard.md` §2.4 (arketipe monitoring), §4 (baseline baca yang diwarisi)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs`, `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` dan `Services` masing-masing — dibaca penuh sebagai template pola `ApiResponse<T>`/`PagedResult<T>`/`AccessController`
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs`, `FinAccountingEventAttempt.cs` (`BE-FIN-010`) — sumber field DTO
- `Repositories/ApplicationDbContext.cs` — dikonfirmasi nama `DbSet` (`FinAccountingEventOutboxes`, `FinAccountingEventAttempts`)
- Grep `EF.Functions.ILike` — dikonfirmasi pola pencarian case-insensitive sudah dipakai modul lain (`BillingInvoiceService`, beberapa service Laboratory) sebelum dipakai di sini

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/AccountingEventDtos.cs` | **Baru.** `AccountingEventQuery`, `AccountingEventResponse`, `AccountingEventDetailResponse`, `AccountingEventAttemptResponse`, `AccountingEventSummaryResponse`, `AccountingEventFilterMetadataResponse` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingEventService.cs` | **Baru.** `GetPagedAsync`, `GetByIdAsync`, `GetSummaryAsync`, `GetFilterMetadataAsync` — seluruhnya baca |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | **Baru.** 4 endpoint `GET`, nol endpoint tulis |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `AddScoped<FinanceAccountingEventService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Baru** — 4 endpoint `GET` di bawah `api/v1/corporate/finance-management/accounting-events` (bagian 4) |
| Database | Tidak ada perubahan skema — hanya membaca tabel yang sudah dibuat `BE-FIN-010` (migration belum dijalankan) |
| Keamanan/Auth | `[Authorize]` + `[AccessPermission]` per endpoint (`AccountingEvents`/`Read`); hak akses ditentukan admin lewat Manajemen Role, tidak ada hardcode peran. Detail menyertakan `PayloadJson`/`ComponentsJson` — aman karena sudah bebas data pasien secara struktural (bagian 1.4) |

---

## 4. Dokumentasi endpoint

| Method | Path | Query/Body | Response | Permission |
| --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | — | `AccountingEventFilterMetadataResponse` | `AccountingEvents.Read` |
| `GET` | `/summary` | — | `AccountingEventSummaryResponse` (6 counter status + total) | `AccountingEvents.Read` |
| `GET` | `/` | `AccountingEventQuery` (`DeliveryStatus?`, `EventTypeCode?`, `Search?`, `SortBy`, `SortDirection`, `PageNumber`, `PageSize`) | `PagedResult<AccountingEventResponse>` | `AccountingEvents.Read` |
| `GET` | `/{id:guid}` | — | `AccountingEventDetailResponse` (+`PayloadJson`, `ComponentsJson`, `Attempts[]`) atau `404` | `AccountingEvents.Read` |

Tidak ada `POST`/`PATCH`/`PUT`/`DELETE` — sesuai DoD roadmap.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan bagian bawah | `PASS` | Dijalankan latar belakang, hasil disalin apa adanya |
| Review manual: `FinanceAccountingEventService` nol pemanggilan `Add`/`Update`/`Remove`/`SaveChangesAsync` | Dikonfirmasi — hanya query `AsNoTracking()` | `PASS` | `FinanceAccountingEventService.cs` |
| Review manual: `FinanceAccountingEventsController` nol atribut `[HttpPost]`/`[HttpPatch]`/`[HttpPut]`/`[HttpDelete]` | Dikonfirmasi — hanya 4 `[HttpGet]` | `PASS` | `FinanceAccountingEventsController.cs` |
| Review manual: ringkasan memisahkan `HELD_FOR_FINALIZATION`/`HELD`/`FAILED` | Dikonfirmasi — 6 field counter terpisah | `PASS` | `AccountingEventSummaryResponse`, bagian 1.3 |
| Review manual: `PayloadJson`/`ComponentsJson` yang ditampilkan detail tidak memuat data pasien | Dikonfirmasi by construction — mewarisi jaminan struktural `BE-FIN-011` bagian 1.2, tidak ada transformasi tambahan di sini | `PASS` | Bagian 1.4 |
| "Uji integrasi" (kolom Verifikasi roadmap) | **Tidak dijalankan** — backend tidak memelihara automated test project (`TEST_POLICY.md`); tidak ada database sungguhan (migration `BE-FIN-010` belum jalan) untuk diuji end-to-end | `NOT RUN` | `BE-FIN-010` bagian 5 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — migration `BE-FIN-010` belum dijalankan, tidak ada database untuk
diuji end-to-end lewat HTTP sungguhan.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh setelah berkas `BE-FIN-012` dibuat):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 50
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
| Kejadian tertahan dan antreannya terlihat | **Terpenuhi** — `DeliveryStatus` sebagai filter list, 6 counter terpisah di summary, `HoldReason`/`AttemptCount`/`LastAttemptAt`/`LastResponseCode` di setiap baris | Bagian 1.3, 2.1, 4 |
| **Tanpa** endpoint pengirim — itu `EPIC FIN-12` | **Terpenuhi** — nol endpoint tulis di controller ini | Bagian 1.1, 2.2 |
| Cakupan `FinanceAccountingEventsController` | **Terpenuhi** | Bagian 2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Belum dapat diuji terhadap database sungguhan — migration `BE-FIN-010` belum dijalankan |
| Masalah yang diketahui | Tidak ada yang baru di luar yang sudah dicatat `BE-FIN-005`..`011` |
| Risiko tersisa | **Rendah** — permukaan baca murni, tidak menyentuh alur transaksi yang sudah ada; risiko utama tetap sama seperti `BE-FIN-010`/`011` (migration belum jalan) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 3 berkas baru (`FinanceAccountingEventsController.cs`, `AccountingEventDtos.cs`, `FinanceAccountingEventService.cs`), 1 berkas disunting (`BillingManagementServiceCollectionExtensions.cs`) — di atas berkas `BE-FIN-002`..`011` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` oleh pengguna mencakup task ini. (2) Otorisasi eksekusi migration `BE-FIN-010` supaya `UAT-07`/`17`..`19` dapat dibuktikan sungguhan lewat HTTP nyata. (3) `MVP-5` (`EPIC FIN-11`) selesai kode-nya di ketiga task (`BE-FIN-010`..`012`); `EPIC FIN-12` (pengiriman) tetap `OPEN DECISION` menunggu endpoint Accounting |

