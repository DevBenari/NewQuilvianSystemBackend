# Laporan Perubahan Backend — `BE-RWI-179`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-179` |
| Judul | Biaya operasi saat kasus selesai |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-179` |
| Trace | `FR-RWF-047`; `RWI-DEC-192`, `196`, `207`; `INV-RWF-29`, `30`; `AC-RWF-044`, `092`, `099`; `UAT-RWF-05`; backend 12.5, 12.7, 12.8; API 11.9; `keperawatan` kamus 12.14 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`); Billing untuk `OPERATING_ROOM` disetujui `RWI-DEC-191` |
| Dependency | `BE-RWI-172` ✅; `BE-RWI-178` ✅; `BE-RWI-155` [IB] ✅ |
| Klasifikasi | `HEAVY` — skor 9: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 1, database 1, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/OperatingRoomManagement/**`, `Areas/HealthServices/BillingManagement/**`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` dan uji dengan Billing sungguhan **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `OperatingRoomManagement` (efek dan delivery); `BillingManagement` (sumber `OPERATING_ROOM`) |
| Prefix registry | `Opr`, `Bil` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (`OperatingRoomCompletionEffects`); `TOUCHED LEGACY` (integrasi, eksekusi, recovery OK; jembatan, resolver, adapter Billing) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak ada perubahan schema |

---

## 1. Masalah yang diperbaiki

Kiriman biaya OK ke Billing sudah disiapkan sebagai outbox, tetapi tujuannya ditahan, dan
finalisasi laporan operasi menyiapkan tagihan "procedure" yang akan menggandakan baris tindakan bila
order tindakannya juga ditagih. Kasus yang kemudian batal pun sudah punya baris kiriman.

## 2. Proses bisnis

1. Serah terima diterima → kasus `Completed` → **sesudah commit**, `OperatingRoomCompletionEffects`:
   (a) setiap order tindakan kasus diselesaikan lewat `ExecuteFromOperatingRoomAsync` — satu-satunya
   pengirim baris tindakan; (b) komponen OK disiapkan: `ANESTHESIA` bila catatan anestesi final,
   `OR_RENT` dari durasi catatan operasi, `MATERIAL-{usageId}` untuk bahan `Used`; (c) setiap delivery
   dikirim sebagai fakta klinis `OPERATING_ROOM` lewat `ClinicalMilestoneFactProducer` →
   `BillingFolioService` → jembatan invoice, dengan `EncounterId` kasus OK (`RWI-DEC-207`).
2. Billing menetapkan harga dari `MstTariff`: jasa anestesi (`SurgeryComponentType = AnesthesiaService`),
   sewa kamar operasi (`OperatingRoomRent`), bahan per `DrugId`. Tarif per jam memakai `ChargeRounding`.
3. Contoh berangka: operasi 95 menit, sewa kamar Rp500.000/jam bulat ke atas → 2 jam = Rp1.000.000;
   proporsional → 1,58 jam = Rp790.000. Jasa anestesi per layanan Rp750.000 → 1 × Rp750.000. Dua
   bahan (kassa 10 pcs Rp2.000, benang 2 pcs Rp45.000) → Rp20.000 dan Rp90.000. Ditambah satu baris
   tindakan dari order.
4. Tarif komponen tidak ada → efek Billing `TARIFF_NOT_FOUND` ("tarif belum ada"), invoice tidak dapat
   difinalkan (`BIL-FIN-021`, dari `BE-RWI-155`).
5. Billing gagal → delivery `Failed` dengan kodenya; penyelesaian kasus tetap sah; `RetryAsync` OK
   mengantrekan dan langsung mencoba kirim ulang.
6. Kasus `Cancelled`/`Rejected` → efek tidak berjalan dan delivery tidak dikirim (`CASE_NOT_COMPLETED`).
   Komponen lama `procedure` tidak pernah dikirim (`SUPERSEDED_BY_ORDER`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OperatingRoomIntegrationService.cs`, `OperatingRoomExecutionService.cs`, `OperatingRoomRecoveryService.cs`,
`OprMaterialUsage.cs`, `OprAnesthesiaRecord.cs`, `OprExecutionRecord.cs`, `OperatingRoomMaterialService.cs`,
`ClinicalMilestoneFactProducer.cs`, `BillingSourceContract.cs`, `BillingClinicalChargeBridgeService.cs`,
`BillingSourceTariffResolver.cs`, `BillingChargeSourceAdapter.cs`, `BillingChargeReconciliationService.cs`,
`BillingInvoiceService.cs`, `BillingFinalizationService.cs`, `MstTariff.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `OperatingRoomManagement/Services/OperatingRoomCompletionEffects.cs` | Baru — efek kasus selesai, idempoten, tidak pernah melempar |
| `OperatingRoomManagement/Services/OperatingRoomIntegrationService.cs` | Billing keluar dari `BlockedDestinations`; `DeliverPendingBillingAsync`, `DeliverToBillingAsync`; `RetryAsync` langsung mengirim ulang |
| `OperatingRoomManagement/Services/OperatingRoomExecutionService.cs` | Penyiapan tagihan "procedure" saat finalisasi laporan dicabut |
| `OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs` | Memanggil efek sesudah commit penerimaan yang membuat kasus `Completed` |
| `BillingManagement/Operational/Constants/BillingSourceContract.cs` | Sumber `OPERATING_ROOM` / `OperatingRoomCharge` |
| `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Domain `OPERATING_ROOM` dijembatani; status tagih `COMPLETED` |
| `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolveOperatingRoomAsync` (komponen, per jam, pembulatan, bahan per `DrugId`) |
| `BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | Kebijakan domain `OPERATING_ROOM` |
| `BillingManagement/Billing/Services/BillingChargeReconciliationService.cs`, `BillingInvoiceService.cs` | Pemetaan domain; domain dimiliki jembatan (tidak boleh dicatat manual) |
| `Program.cs` | Registrasi `OperatingRoomCompletionEffects` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET …/integration/reconciliation` kini `BlockedDestinations = []`; tidak ada endpoint baru |
| Database | `NOT APPLICABLE` — memakai `OprIntegrationDelivery`, `CliClinicalMilestoneFact`, dan tabel folio yang ada |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Health Services / Operating Room Management / Execution

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `api/v1/health-services/operating-room-management/cases/{caseId}/execution/handovers/{handoverId}/accept` | Kini juga memicu efek kasus selesai sesudah commit | `OperatingRoomHandover : Receive` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff dan scope | Satu baris tindakan satu pengirim; komponen hanya saat `Completed` | `PASS` | Daftar 3.2 |
| Pemeriksaan bentrokan nama tipe (OK × Clinical × Billing) | Tidak ada | `PASS` | Sesi 2 Oktober 2026 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis dengan Billing sungguhan dan contoh berangka | Belum dijalankan | `NOT RUN` | Butuh build, database, dan tarif komponen |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kasus `Completed` → order `Completed` dan satu baris tindakan; anestesi, sewa kamar, bahan masing-masing satu baris | Terpenuhi (source) | `ApplyAsync`, `DeliverToBillingAsync`, `ResolveOperatingRoomAsync` |
| 2. Efek dijalankan ulang → tidak ada baris ganda (`INV-RWF-29`) | Terpenuhi (source) | Order dilewati; delivery berkunci; fakta isi sama → `Replayed` |
| 3. `Cancelled`/`Rejected` → nol biaya (`INV-RWF-30`) | Terpenuhi (source) | Efek hanya pada `Completed`; staging "procedure" dicabut |
| 4. Tarif komponen tidak ada → "tarif belum ada", invoice tidak dapat difinalkan | Terpenuhi (source) | `TARIFF_NOT_FOUND` + `BIL-FIN-021` |
| 5. Billing gagal → dicatat dan dicoba ulang, kasus tetap selesai | Terpenuhi (source) | Delivery `Failed`; `RetryAsync`; efek dibungkus `try/catch` |
| 6. `UAT-RWF-05`: biaya muncul sesudah `Completed`, tidak saat dipesan | Terpenuhi (source) | Tidak ada staging sebelum `Completed` |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nilai `SourceContext` mengikuti kontrak: `OPERATING_ROOM` (huruf besar), berbeda gaya dari sumber lama (`Procedure`, `Laboratory`) |
| Masalah yang diketahui | Koreksi pemakaian bahan sesudah kasus selesai tidak otomatis menyesuaikan tagihan (jalur adjustment Billing). Order yang butuh approval belum disetujui tidak diselesaikan otomatis dan tercatat sebagai kegagalan efek |
| Risiko tersisa | Tarif komponen operasi wajib diisi pemilik tarif (`02-backend-architecture.md` 12.12) |
| Perubahan sampingan | Konstruktor `OperatingRoomExecutionService` kehilangan parameter `OperatingRoomIntegrationService` |
| Interupsi | `NONE` |
| Status Git | `??` `OperatingRoomCompletionEffects.cs`; `M` `OperatingRoomIntegrationService.cs`, `OperatingRoomExecutionService.cs`, `OperatingRoomRecoveryService.cs`, `BillingSourceContract.cs`, `BillingClinicalChargeBridgeService.cs`, `BillingSourceTariffResolver.cs`, `BillingChargeSourceAdapter.cs`, `BillingChargeReconciliationService.cs`, `BillingInvoiceService.cs`, `Program.cs` |
| Langkah berikutnya | Pemilik tarif mengisi tarif komponen; uji contoh SC Obgyn sesudah build |
