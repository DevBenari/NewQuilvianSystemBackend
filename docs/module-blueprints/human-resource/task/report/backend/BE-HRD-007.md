# Laporan Perubahan Backend — `BE-HRD-007`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-007` |
| Judul | Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi Tahunan |
| Slice | `S-B2` (Administrasi Cuti, Penyesuaian Saldo, & Rekonsiliasi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-007`, `IN-08` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-006` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `LeaveAdjustmentController.cs`, `LeaveAdjustmentService.cs`, `LeaveAdjustmentDtos.cs`, `LeaveCancellationAdminController.cs`, `LeaveCancellationService.cs`, `LeaveCarryForwardController.cs`, `LeaveCarryForwardProcessorService.cs`, `LeaveFinalReconciliationController.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Akuntabilitas Penyesuaian Saldo Cuti Manual oleh Bagian HRD**:
   Penyesuaian saldo cuti manual (koreksi saldo awal tahun, kompensasi dinas luar kota, restitusi saldo hangus) rawan manipulasi jika tidak disertai alasan tertulis yang sah dan nomor referensi dokumen SK / memo direksi. Melalui `LeaveAdjustmentController`, setiap penyesuaian saldo mewajibkan pencatatan `LeaveAdjustmentReasonId`, `Reason`, dan `SourceReferenceNumber`.
2. **Pemulihan Saldo Idempoten pada Pembatalan Cuti yang Telah Disetujui**:
   Jika seorang staf medis yang telah disetujui cutinya terpaksa dipanggil kembali bertugas (*recall*) atau membatalkan cutinya karena kondisi darurat rumah sakit, saldo cuti yang sebelumnya telah dipotong wajib dipulihkan kembali ke buku saldo (`HrdLeaveBalance`) secara idempoten tanpa risiko pengembalian ganda (*double refund*).
3. **Pengelolaan Carry Forward dan Rekonsiliasi Akhir Periode**:
   Penanganan sisa cuti tahunan yang dapat dialihkan ke tahun berikutnya (*carry forward*) atau dihanguskan (*forfeited*) memerlukan proses batch otomatis yang terdokumentasi dan terkunci dari mutasi susulan.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[HR Administrator / Atasan Medis]
                  │
   ┌──────────────┴──────────────┐
   ▼                             ▼
[Penyesuaian Saldo Manual]   [Pembatalan Cuti Disetujui]
   │                             │
   ├─► Pilih Saldo Pegawai       ├─► Validasi Cuti Approved
   ├─► Masukkan Alasan Wajib     ├─► Hitung Hari Belum Terpakai
   ├─► Catat No SK / Memo HR     ├─► Terapkan Idempotent Restore
   ├─► Persetujuan Workflow      ├─► Kembalikan ke HrdLeaveBalance
   └─► Posting ke Saldo Aktif    └─► Catat Log Audit Pembatalan
```

* **Tujuan**: Menjamin bahwa seluruh mutasi saldo cuti (penambahan, pengurangan, pembatalan, dan pengalihan akhir tahun) tercatat dengan akuntabilitas tinggi dan terlindungi dari manipulasi data.
* **Pelaku**: HR Leave Administrator, Kepala Bagian SDM, Pegawai yang Bersangkutan.
* **Pemicu**: Pembukaan saldo awal tahun, koreksi administratif cuti, pemanggilan darurat staf yang sedang cuti (*duty recall*), atau penutupan periode hak cuti tahunan.
* **Langkah & Aturan**:
  1. Penyesuaian saldo manual mewajibkan pengisian `Reason` (alasan) dan referensi dokumen pendukung (`SourceReferenceNumber`).
  2. Alur posting penyesuaian saldo menggunakan kunci idempoten (`IdempotencyKey`) untuk mencegah eksekusi berulang.
  3. Pembatalan cuti disetujui memulihkan kuota saldo hanya untuk hari-hari cuti yang belum dijalankan dan mengunci status transaksi agar tidak dapat dibatalkan ulang.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Leave Management / Leave Adjustment`
Tag: `[Tags("Corporate / Human Resource / Leave Management / Leave Adjustment")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/leave/adjustments/filters/metadata` | Mengambil metadata opsi filter penyesuaian saldo | `LeaveAdjustment:Read` | - | `ApiResponse<LeaveAdjustmentFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/leave/adjustments` | Mengambil daftar transaksi penyesuaian saldo berhalaman | `LeaveAdjustment:Read` | `LeaveAdjustmentPagedQueryRequest` | `ApiResponse<PagedResult<LeaveAdjustmentListResponse>>` |
| `POST` | `api/v1/corporate/human-resource/leave/adjustments` | Membuat pengajuan penyesuaian saldo cuti manual (Wajib alasan & no SK) | `LeaveAdjustment:Create` | `CreateLeaveAdjustmentRequest` | `ApiResponse<LeaveAdjustmentActionResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/leave/adjustments/{id}/approve` | Menyetujui penyesuaian saldo cuti | `LeaveAdjustment:Approve` | `ApproveLeaveAdjustmentRequest` | `ApiResponse<LeaveAdjustmentActionResponse>` |
| `POST` | `api/v1/corporate/human-resource/leave/adjustments/{id}/post` | Memposting penyesuaian yang telah disetujui langsung ke saldo aktif | `LeaveAdjustment:Post` | `PostLeaveAdjustmentRequest` | `ApiResponse<LeaveAdjustmentActionResponse>` |
| `POST` | `api/v1/corporate/human-resource/leave/adjustments/{id}/reverse` | Membalikkan (*reversal*) penyesuaian saldo yang salah | `LeaveAdjustment:Reverse` | `ReverseLeaveAdjustmentRequest` | `ApiResponse<LeaveAdjustmentActionResponse>` (201 Created) |

### Grup: `Corporate / Human Resource / Leave Management / Leave Cancellation`
Tag: `[Tags("Corporate / Human Resource / Leave Management / Leave Cancellation")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/leave-management/leave-cancellations/{id}/apply` | Menerapkan pembatalan cuti disetujui & memulihkan saldo secara idempoten | `LeaveCancellation:Apply` | `ApplyLeaveCancellationRequest` | `ApiResponse<LeaveLifecycleActionResponse>` |

---

## 4. Bukti implementasi kode

1. **Administrasi Penyesuaian Saldo**:
   - Berkas: [LeaveAdjustmentController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/Controllers/LeaveAdjustmentController.cs)
   - Berkas: [LeaveAdjustmentService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveAdjustmentService.cs)
   - Validasi alasan wajib dan referensi dokumen pada [LeaveAdjustmentDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/DTOs/LeaveAdjustmentDtos.cs#L216-L231).

2. **Pemulihan Saldo Idempoten pada Pembatalan Cuti**:
   - Berkas: [LeaveCancellationService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveCancellationService.cs)
   - Berkas: [LeaveCancellationAdminController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/Controllers/LeaveCancellationAdminController.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Penyesuaian saldo manual oleh HR wajib menyertakan alasan penyesuaian dan referensi dokumen (`Reason`, `LeaveAdjustmentReasonId`, `SourceReferenceNumber`) -> **LULUS**.
* **Acceptance Criteria 2**: Pembatalan cuti yang telah disetujui memulihkan saldo secara idempoten (`IsIdempotent`, pengembalian ke `HrdLeaveBalance`) -> **LULUS**.
