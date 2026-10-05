# Laporan Perubahan Backend — `BE-FIN-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-057` |
| Judul | Petugas AR dapat mencatat, melunasi, menghapus, dan membatalkan tagihan sewa, serta membaca umur piutangnya |
| Slice | `REV-13D` — `EPIC FIN-19` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-100`..`104`; `FIN-DES-075`, `076`, `077` |
| Contract version | `FIN-API-1.4` §E.1, `FIN-STATE-1.5` §E.1, `FIN-VAL-1.6` `154`..`164`, `FIN-PERM-1.6` §F.1 — seluruhnya `approved` 1 Oktober 2026 |
| Dependency | `BE-FIN-056` ✅ (skema) — terpenuhi, tabel sudah ada |
| Klasifikasi | `HIGH` — satu repository (skor 0); 6 berkas (3 baru, 3 diubah) (skor 1); 1 service baru dengan state machine 4 transisi (skor 2); 10 endpoint baru pada 1 controller baru (skor 2); database — **nol** migration (skor 0); keamanan/auth — 1 resource baru + 3 action (skor 1); UI/workflow — nol, backend saja (skor 0). Total 6 → `HIGH` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/**`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI), dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai** 1 Oktober 2026 — source lengkap sesuai kontrak, **nol** migration dibutuhkan (skema sudah ada dari `BE-FIN-056`, dikonfirmasi). `dotnet build` **PASS**, dikonfirmasi pengguna |

---

## 1. Masalah yang diperbaiki

`BE-FIN-056` membangun tabel `FinNonPatientReceivable`/`FinNonPatientReceivableSettlement`
tanpa satu pun jalan untuk mengisinya — nol service, nol endpoint. Petugas AR belum bisa mencatat
tagihan sewa parkir/tenant, melunasinya, menghapusbukukannya, atau membatalkannya sama sekali.
Task ini menutup gap itu: satu service dan satu controller baru, 10 endpoint, menegakkan
`FIN-VAL-154`..`164` dan state machine `FIN-STATE-1.5` §E.1.

---

## 2. Proses bisnis

Lima perpindahan status (`state-transition-matrix.md` §E.1), **tanpa jenjang persetujuan apa pun**
(`FIN-DEC-103`) — setiap aksi selesai dalam satu panggilan oleh staf AR:

1. **Catat tagihan baru** → `OUTSTANDING`. Nominal tagihan wajib > 0 (`FIN-VAL-155`), denda >= 0
   (`FIN-VAL-156`), periode/jatuh tempo masuk akal (`FIN-VAL-157`), kategori `PARKING`/`TENANT`
   saja (`FIN-VAL-154`).
2. **Koreksi** (`PUT /{id}`) → hanya sah bila **belum pernah** menerima pembayaran sama sekali
   (`FIN-VAL-159`) — diperiksa lewat `Settlements.Count > 0`, bukan saldo bersih, karena baris
   pelunasan bernilai nol tetap terhitung "pernah menerima pembayaran".
3. **Catat pelunasan** (`POST /{id}/settlements`) → `OUTSTANDING`/`PARTIALLY_SETTLED` sesuai
   jumlah kumulatif pelunasan dibanding total tagihan. Pelunasan boleh bernilai negatif untuk
   membetulkan kekeliruan (termasuk dari `SETTLED` kembali ke `PARTIALLY_SETTLED`). Jumlah
   kumulatif tidak boleh < 0 (`FIN-VAL-162`) atau melebihi total tagihan (`FIN-VAL-161`).
4. **Hapus buku** (`POST /{id}/write-off`) → `WRITTEN_OFF`, hanya dari `OUTSTANDING`/
   `PARTIALLY_SETTLED`, alasan wajib (`FIN-VAL-158`).
5. **Batalkan** (`POST /{id}/cancel`) → `CANCELLED`, hanya dari `OUTSTANDING` dan belum pernah
   menerima pembayaran (`FIN-VAL-160`), alasan wajib.

`WRITTEN_OFF` dan `CANCELLED` adalah status akhir — seluruh aksi berikutnya ditolak `422`
(`FIN-VAL-163`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` AMENDMENT REVISI 13 §K.4/K.6 — bentuk service/controller, daftar
  10 endpoint, atribut akses
- `contracts/api-contract.md` §E.1 — field persis 9 DTO, kode status, "yang tidak dilakukan"
- `contracts/state-transition-matrix.md` §E.1 — tabel 11 baris perpindahan status, tabel
  perbedaan sengaja dari piutang pasien
- `contracts/validation-matrix.md` `FIN-VAL-154`..`164` — pesan penolakan persis per aturan
- `contracts/permission-audit-matrix.md` §F.1 — pemetaan 10 endpoint ke 3 action (`Read`/
  `Create`/`Update`) pada resource `FinanceNonPatientReceivable`
- `Areas/.../Receivable/Services/FinanceReceivableService.cs` — pola DI, `GenerateNumber`,
  `BeginTransactionAsync`/`AcquireLockAsync` (advisory lock Postgres), `EnsureCurrent`/`Stale`
  (concurrency), `ReceivableAgingBuckets` (dipakai ulang persis, bukan diduplikasi)
- `Areas/.../Receivable/Controllers/FinanceReceivablesController.cs` — pola `[AccessController]`/
  `[AccessAction]`/`[AccessPermission]`, `Failure`/`IsHandled`, `CurrentUserId()`
- `Areas/.../Receivable/Services/FinanceReceivableInvoiceBatchService.cs` — pola `IsCancel`/
  `CancelBy`/`CancelDateTime` (`IdentityModel`) untuk aksi cancel
- `Models/IdentityModel.cs` — dikonfirmasi kolom `IsCancel`/`CancelBy`/`CancelDateTime` tersedia
  pada base class, dipakai untuk aksi `Cancel`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Receivable/DTOs/FinanceNonPatientReceivableDtos.cs` **(baru)** | 15 DTO: `NonPatientReceivableQuery`, `NonPatientReceivableAgingQuery`, `NonPatientReceivableSummaryQuery`, `CreateNonPatientReceivableRequest`, `UpdateNonPatientReceivableRequest`, `CreateNonPatientReceivableSettlementRequest`, `WriteOffNonPatientReceivableRequest`, `CancelNonPatientReceivableRequest`, `NonPatientReceivableResponse`, `NonPatientReceivableDetailResponse`, `SettlementRowResponse`, `NonPatientReceivableSummaryResponse`, `NonPatientReceivableFilterMetadataResponse` |
| `Areas/.../Receivable/Services/FinanceNonPatientReceivableService.cs` **(baru)** | `GetPagedAsync`, `GetByIdAsync`, `GetAgingAsync` (reuse `ReceivableAgingBuckets`), `GetSummaryAsync`, `GetFilterMetadataAsync`, `CreateAsync`, `UpdateAsync`, `AddSettlementAsync`, `WriteOffAsync`, `CancelAsync`; 3 exception type (`NonPatientReceivableBadRequestException`/`ValidationException`/`ConflictException`) |
| `Areas/.../Receivable/Controllers/FinanceNonPatientReceivablesController.cs` **(baru)** | 10 endpoint, `[AccessController("CORPORATE_FINANCE_MANAGEMENT_NON_PATIENT_RECEIVABLE", ..., ControllerName = "FinanceNonPatientReceivable", SortOrder = 32)]`, 3 `[AccessAction]` (`Read`/`Create`/`Update`) |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `services.AddScoped<FinanceNonPatientReceivableService>();` ditambahkan bersebelahan dengan `FinanceReceivableInvoiceBatchService` |

**Nol** perubahan pada `FinNonPatientReceivable.cs`/`FinNonPatientReceivableSettlement.cs`,
configuration, `ApplicationDbContext.cs`, atau `Migrations/**` — sesuai kondisi `jika dibutuhkan`
pada permintaan pengguna: skema `BE-FIN-056` sudah mencukupi seluruh kebutuhan task ini tanpa
kolom atau tabel tambahan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif murni.** 10 endpoint baru di bawah `api/v1/corporate/finance-management/non-patient-receivables` — nol endpoint lama disentuh |
| Database | **Nol.** Tidak ada migration pada task ini — dikonfirmasi lewat `git status --short` sebelum dan sesudah penulisan, tidak ada berkas `Migrations/**` baru selain yang sudah ada dari `BE-FIN-056` |
| Keamanan/Auth | **Satu resource baru**: `FinanceNonPatientReceivable`, tiga action (`Read`/`Create`/`Update`) — persis mengikuti `permission-audit-matrix.md` §F.1. `FIN-DEC-103` membuat write-off dan cancel memakai action `Update` yang sama (bukan action terpisah dengan persetujuan berjenjang), **disengaja**, bukan kelalaian |

**Keputusan teknis yang didokumentasikan (bukan kebijakan bisnis baru):**

1. **Penyimpanan alasan write-off/cancel.** `api-contract.md` §E.1 mewajibkan `Reason` pada
   request `WriteOffNonPatientReceivableRequest`/`CancelNonPatientReceivableRequest`, tetapi
   `NonPatientReceivableResponse` **tidak** punya kolom `Reason` khusus — satu-satunya kolom teks
   bebas pada model adalah `Note`. Diputuskan: alasan ditambahkan sebagai baris baru ke `Note`
   (format `[Write-off] <alasan>` / `[Cancel] <alasan>`, dipotong ke 500 karakter dari akhir bila
   terlalu panjang), mengikuti pola "tidak pernah menghapus, selalu menambah" yang berlaku di
   seluruh blueprint ini (lihat dokumentasi `FinNonPatientReceivableSettlement.cs`). Dicatat di
   sini sebagai delta implementasi — bila owner ingin kolom `Reason` terpisah dan dapat dicari,
   itu kebutuhan skema baru untuk task lanjutan.
2. **`NonPatientReceivableSummaryQuery`/`NonPatientReceivableFilterMetadataResponse` field-level.**
   `api-contract.md` §E.1 hanya menyebut nama kedua DTO ini tanpa merinci field (berbeda dari 9
   DTO lain yang field-nya dirinci penuh pada tabel "Request dan response"). Diisi mengikuti pola
   sibling yang sudah ada (`ReceivableFilterMetadataResponse`/`GetSummaryAsync` pada
   `FinanceReceivableService`): `NonPatientReceivableSummaryQuery` diberi saringan `Category`
   opsional (konsisten dengan `GET /aging` pada kapabilitas yang sama), dan
   `NonPatientReceivableFilterMetadataResponse` diberi `PageSizeOptions`/`SortableFields`/
   `CategoryOptions`/`StatusOptions`. Ini pengisian celah teknis, bukan keputusan bisnis.
3. **Validasi `FIN-VAL-154` (kategori) diterapkan pada `PUT` juga**, bukan hanya `POST` seperti
   tertulis literal pada baris tabel `validation-matrix.md`. `UpdateNonPatientReceivableRequest`
   mewarisi field `Category` penuh dari `CreateNonPatientReceivableRequest` (sesuai kontrak:
   "Sama dengan Create, ditambah `ExpectedRowVersion`"), sehingga membiarkan nilai kategori tidak
   sah lolos lewat `PUT` adalah celah integritas data, bukan interpretasi bisnis baru. Dicatat di
   sini agar tidak disangka penyimpangan kontrak yang disengaja.

---

## 4. Dokumentasi endpoint

`[Tags("Corporate / Finance Management / Non Patient Receivable")]`

Base URL: `api/v1/corporate/finance-management/non-patient-receivables`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Isi pilihan saringan (kategori, status) | `FinanceNonPatientReceivable : Read` | Tersedia |
| `GET` | `/summary` | Ringkasan nominal per status | `FinanceNonPatientReceivable : Read` | Tersedia |
| `GET` | `/aging` | Umur piutang sewa per kelompok, bersaring kategori | `FinanceNonPatientReceivable : Read` | Tersedia |
| `GET` | `/` | Daftar tagihan sewa, bersaring kategori/status/periode/jatuh tempo | `FinanceNonPatientReceivable : Read` | Tersedia |
| `GET` | `/{id}` | Rincian satu tagihan beserta riwayat pelunasannya | `FinanceNonPatientReceivable : Read` | Tersedia |
| `POST` | `/` | Mencatat tagihan sewa baru untuk satu periode | `FinanceNonPatientReceivable : Create` | Tersedia |
| `PUT` | `/{id}` | Mengoreksi tagihan yang belum menerima pembayaran sama sekali | `FinanceNonPatientReceivable : Update` | Tersedia |
| `POST` | `/{id}/settlements` | Mencatat pembayaran yang diterima dari penyewa | `FinanceNonPatientReceivable : Update` | Tersedia |
| `POST` | `/{id}/write-off` | Menghapus piutang sewa yang tidak tertagih | `FinanceNonPatientReceivable : Update` | Tersedia |
| `POST` | `/{id}/cancel` | Membatalkan tagihan yang salah dicatat | `FinanceNonPatientReceivable : Update` | Tersedia |

Kode status: `200` berhasil; `201` tercipta (`POST /`, `POST /{id}/settlements`); `400` isian
tidak lengkap/tidak masuk akal; `403` tanpa hak akses; `404` tidak ditemukan; `409`
`ExpectedRowVersion` basi; `422` langkah tidak sah menurut `state-transition-matrix.md` §E —
persis sesuai `api-contract.md` §E.1.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| Review diff/scope | 4 berkas (3 baru, 1 diubah), persis sesuai `02-backend-architecture.md` §K.4/K.6 | `PASS` | `git status --short` |
| Review field DTO vs kontrak | Seluruh 13 DTO dibandingkan satu per satu dengan `api-contract.md` §E.1 "Request dan response" — cocok persis kecuali dua DTO yang field-nya didesain (§3.3 butir 2) | `PASS` | Perbandingan manual |
| Review state machine vs kontrak | Kelima perpindahan status, termasuk pelunasan negatif dan `SETTLED` → `PARTIALLY_SETTLED`, dibandingkan baris demi baris dengan `state-transition-matrix.md` §E.1 (11 baris) | `PASS` | Perbandingan manual |
| Review pesan penolakan vs kontrak | `FIN-VAL-154`..`164` — teks pesan pada service disalin persis dari `validation-matrix.md`, bukan diparafrasekan | `PASS` | Perbandingan manual |
| Review hak akses vs kontrak | 10 endpoint dipetakan ke 3 action, cocok persis dengan `permission-audit-matrix.md` §F.1 | `PASS` | Perbandingan manual |
| Review invariant — service tidak memanggil `FinanceReceivableService`/`FinanceReceiptService`/`FinanceAccountingOutboxService` | Nol `using`, nol field, nol pemanggilan ke ketiga service itu pada `FinanceNonPatientReceivableService.cs` | `PASS` | Baca penuh berkas — nol referensi |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Receivable`, prefix `Fin` `ACTIVE` — sudah terdaftar (sama dengan `BE-FIN-056`), applicability `NEW CODE` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Registrasi DI | `FinanceNonPatientReceivableService` terdaftar `AddScoped` di `BillingManagementServiceCollectionExtensions.cs`, bersebelahan `FinanceReceivableInvoiceBatchService` | `PASS` | Baca berkas setelah edit |

Uji manual/runtime: `NOT FEASIBLE` — server tidak dijalankan pada task ini; tidak ada akses
lingkungan API berjalan.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** `dotnet ef migrations add` (tidak dibutuhkan — nol perubahan skema); uji
manual/runtime (server tidak dijalankan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (dari `01-backend-roadmap.md` baris `BE-FIN-057`) | Status | Bukti |
| --- | --- | --- |
| Kelima status berpindah sesuai `FIN-STATE-1.5` §E.1 | Terpenuhi (source) | §2, §5 — perbandingan baris demi baris |
| `FIN-VAL-154`..`164` ditegakkan | Terpenuhi (source) | §5 |
| Kelompok umur **sama persis** dengan umur piutang pasien | Terpenuhi | `GetAgingAsync` memanggil `ReceivableAgingBuckets` yang sama, nol duplikasi definisi |
| Service **tidak pernah** memanggil `FinanceReceivableService`/`FinanceReceiptService`/`FinanceAccountingOutboxService` | Terpenuhi | §5 |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |
| **Nol** migration (skema sudah di `BE-FIN-056`) | Terpenuhi | §3.2, §3.3 — dikonfirmasi `git status --short` |
| Laporan task tracked ada | Terpenuhi | Laporan ini |

Seluruh kriteria terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` dikonfirmasi PASS oleh pengguna 1 Oktober 2026, menutup risiko "nol verifikasi compiler" yang sempat tercatat |
| Masalah yang diketahui | Dua keputusan teknis pada §3.3 (penyimpanan alasan ke `Note`; desain field `SummaryQuery`/`FilterMetadataResponse`) perlu ditinjau owner bila menuntut bentuk berbeda |
| Risiko tersisa | **Sedang** — selaras dengan peringatan `FIN-OQ-044` pada kontrak (pelunasan di sini **tidak** tersambung ke kas harian/kejadian akuntansi); **MUST** disampaikan ke pemilik sebelum kapabilitas ini dipakai pada data sungguhan (lihat baris 5 gerbang terbuka `01-backend-roadmap.md`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup 4 berkas task ini (3 baru, 1 diubah — lihat §3.2) |
| Langkah berikutnya | 8 task frontend (`FE-FIN-016`..`023`) — belum dimulai sama sekali pada sesi ini |
