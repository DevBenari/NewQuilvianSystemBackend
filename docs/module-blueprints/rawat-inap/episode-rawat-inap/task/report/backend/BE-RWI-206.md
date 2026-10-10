# Laporan Perubahan Backend — `BE-RWI-206`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-206` |
| Judul | Endpoint Antrean & Detail Transfer Pasien IGD / Rawat Jalan untuk Admisi Rawat Inap |
| Slice | Slice Transfer IGD / RJ; Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/backend-roadmap-admisi-transfer-igd.md`](../../../roadmap/backend-roadmap-admisi-transfer-igd.md) — kartu `BE-RWI-206` |
| Trace | `FR-RI-187`, `FR-RI-188`, `FR-RI-190`; `RWI-DEC-274`, `RWI-DEC-276`; `RWI-AC-396`, `RWI-AC-397`, `RWI-AC-401`; API Contract `0.12.0` (11.7.1, 11.7.2) |
| Contract version | `0.12.0` **`approved`** (10 Oktober 2026) |
| Dependency | — (Task fondasi mandiri) |
| Klasifikasi | `MEDIUM` — Kueri antrean transfer lintas modul IGD ke Rawat Inap, kalkulasi lama menunggu & penanda overdue |
| Task mode | `BACKEND` |
| Target tulis | `InpatientAdmissionTransferDtos.cs`, `InpAdmissionTransferService.cs`, `InpatientAdmissionTransferController.cs`, `Program.cs` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-396` dan `RWI-AC-397` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Sesuai kebutuhan Analisis Bisnis (Tahap A dan B alur admisi transfer IGD):
1. **Penerimaan Disposisi Medis IGD (`RWI-DEC-274`, `RWI-AC-396`):** Petugas admisi rawat inap membutuhkan tampilan antrean terpadu untuk melihat pasien IGD yang telah diputuskan rawat inap oleh dokter IGD (`EmgDisposition` status `Confirmed` dan belum `Executed`).
2. **Kalkulasi Waktu Tunggu & Peringatan Overdue (`RWI-DEC-276`, `RWI-AC-397`):** Menghitung selisih waktu (`WaitingMinutes`) antara waktu instruksi dokter IGD (`DecidedAt`) dengan waktu sekarang. Jika melebihi ambang batas toleransi 60 menit, sistem menandai `IsOverdue = true` untuk memprioritaskan penanganan pasien darurat.
3. **Data Lengkap untuk Pre-fill Stepper (`RWI-AC-401`):** Menyediakan endpoint detail rujukan transfer untuk mengisi otomatis data demografi pasien, diagnosa kerja dari IGD, dan usulan DPJP pada antarmuka admisi.

---

## 2. Rincian Perubahan Source Code

### 2.1 `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`
- Mendefinisikan DTO `AdmissionTransferQuery` dengan parameter `SourceType`, `Status`, `OverdueOnly`, `Search`, `PageNumber`, dan `PageSize`.
- Mendefinisikan DTO `InpatientAdmissionTransferItemResponse` yang mencakup data identitas pasien, nomor kunjungan IGD, DPJP, diagnosa, durasi menunggu dalam menit, dan status overdue.

### 2.2 `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`
- Mengimplementasikan `GetPagedTransfersAsync`:
  - Mengambil data dari `EmgDisposition` dengan join ke `EmergencyVisit`, `Patient`, `DecidedByDoctor`, dan `DestinationServiceUnit`.
  - Memfilter jenis disposisi yang mensyaratkan rawat inap atau memiliki unit tujuan rawat inap.
  - Memfilter status default `EmergencyDispositionStatus.Confirmed` dan `ExecutedAt == null`.
  - Mendukung pencarian teks berdasarkan Nama Pasien, No. Rekam Medis, atau No. Kunjungan IGD.
  - Menghitung durasi menunggu `waitingMinutes = (int)(DateTime.UtcNow - decidedAt).TotalMinutes` dan mengevaluasi `isOverdue = waitingMinutes > 60`.
  - Mengurutkan berdasarkan waktu instruksi dokter terlama (`DecidedAt` ascending).
- Mengimplementasikan `GetTransferByIdAsync` untuk mengambil detail satu rujukan transfer.

### 2.3 `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionTransferController.cs`
- Membuat controller API bergaya Swagger dengan tag `[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]`.
- Melindungi endpoint dengan izin akses `InpatientAdmissionTransfer : Read`.
- Endpoint `GET /api/v1/health-services/inpatient-management/admission-transfers` untuk antrean.
- Endpoint `GET /api/v1/health-services/inpatient-management/admission-transfers/{id}` untuk detail rujukan.

### 2.4 `Program.cs`
- Mendaftarkan `InpAdmissionTransferService` sebagai scoped service di Dependency Injection container.

---

## 3. Bukti Verifikasi & Traceability
- Kueri antrean transfer berhasil membaca pasien IGD aktif berstatus `Confirmed`.
- Pasien yang sudah selesai dieksekusi atau dibatalkan otomatis tidak muncul pada antrean.
- Penanda `IsOverdue` aktif secara otomatis jika waktu instruksi dokter melebihi 60 menit.
