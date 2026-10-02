# Laporan Perubahan Backend — `BE-RWI-132`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-132` |
| Judul | Kueri Status Kasir Bangsal Bebas Rupiah |
| Slice | Gelombang INT-BE-2 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-132` |
| Trace | `FR-INT-011` s.d. `013`; `RWI-DEC-160`, `RWI-AC-240`; API §1, Validation `VAL-INT-006` |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-127` |
| Klasifikasi | `MEDIUM` — Pemisahan DTO tampilan status operasional bebas rupiah dari rincian finansial |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/DTOs/`, `Areas/HealthServices/InPatientManagement/Services/`, `Areas/HealthServices/InPatientManagement/Controllers/`, `Program.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Endpoint status operasional steril rupiah dan endpoint rincian finansial berizin terpasang lengkap. |

---

## 1. Masalah yang Diselesaikan
Menampilkan rincian nominal uang langsung di layar kerja bangsal rawat inap melanggar batas privasi finansial pasien dan berpotensi memicu kebocoran data. Sebaliknya, staf bangsal/perawat hanya memerlukan informasi operasional: apakah tagihan masih pending di kasir, lencana warna status, kendala blocker yang harus diselesaikan, dan apakah pasien sudah boleh dipulangkan secara fisik.

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **DTO Model**:
   - `InpatientBillingStatusResponseDto`: Berisi data operasional murni tanpa satu pun nominal rupiah (`EpisodeId`, `EncounterId`, `PatientName`, `MedicalRecordNumber`, `FolioStatus`, `ClearanceStatus`, `OperationalStatusText`, `StatusColor`, `CanPhysicallyDischarge`, `BlockerReasons`, `LastCheckedAtUtc`).
   - `InpatientBillingDetailsResponseDto`: Berisi akumulasi angka finansial lengkap (`TotalCharges`, `CoveredAmount`, `PatientExcess`, `DepositPaid`, `OutstandingAmount`, `ClearanceStatus`, `Items`).
   - `BillingDetailItemDto`: Breakdown per kategori tagihan (`Category`, `Description`, `Amount`).
2. **Layanan Kueri `IInpatientBillingQueryService` & `InpatientBillingQueryService`**:
   - `GetOperationalBillingStatusAsync`: Melakukan query `AsNoTracking` ke entitas `InpEpisode`, `BilFolio`, menentukan teks status operasional dan warna lencana (`amber`, `green`, `red`, `purple`, `gray`), mengecek status `CanPhysicallyDischarge`, serta menyusun daftar alasan kendala (*blocker reasons*).
   - `GetFinancialDetailsAsync`: Mengambil breakdown biaya riil dari `BilChargeLine` dan saldo deposit melalui `BillingDepositService`.
3. **Controller `InpatientBillingOperationalController`**:
   - Swagger Tag: `[Tags("Inpatient Billing Operational")]`.
   - Route: `api/v1/health-services/inpatient-management/episodes`.
   - `GET {episodeId}/billing-status`: Memeriksa hak akses perawat (`InpatientBillingOperational : Read`), mengembalikan status operasional tanpa rupiah.
   - `GET {episodeId}/billing-details`: Memeriksa izin finansial `InpatientBilling:View` atau peran kasir/keuangan. Pengguna tanpa izin secara tegas ditolak HTTP 403 Forbidden dengan pesan `VAL-INT-006`: *"Akses ditolak: Anda tidak memiliki hak akses untuk melihat rincian finansial dan nominal rupiah tagihan rawat inap."*.
4. **Pendaftaran DI `Program.cs`**:
   - Mendaftarkan `IInpatientBillingQueryService` dan `InpatientBillingQueryService` sebagai `Scoped`.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBillingSummaryDtos.cs` | Baru — Model DTO status operasional bebas rupiah & detail finansial |
| `Areas/HealthServices/InPatientManagement/Services/IInpatientBillingQueryService.cs` | Baru — Interface service kueri status kasir |
| `Areas/HealthServices/InPatientManagement/Services/InpatientBillingQueryService.cs` | Baru — Implementasi kueri status operasional & kalkulasi finansial |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBillingOperationalController.cs` | Baru — Controller REST Swagger untuk status operasional bangsal |
| `Program.cs` | Diperbarui — Registrasi DI query service |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Bebas Nominal Rupiah pada Status Bangsal):** Endpoint `GET /billing-status` terbukti tidak memuat field angka uang.
- **AC-2 (Proteksi Izin Finansial `VAL-INT-006`):** Endpoint `GET /billing-details` menolak staf bangsal biasa dengan respons HTTP 403 Forbidden.
- **AC-3 (Daftar Kendala Blocker):** Informasi blocker tampil jelas membantu perawat memandu keluarga pasien ke kasir.
