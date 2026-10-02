# Matriks Validasi (Validation Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |

---

## 1. Matriks Validasi Bisnis & Pesan Penolakan Sistem

| Kode Validasi | Target Endpoint / Operasi Bisnis | Kondisi Pelanggaran | Aturan Acuan | HTTP Status | Pesan Kesalahan Sistem (Bahasa Indonesia) |
|---|---|---|---|:---:|---|
| **`VAL-INT-001`** | `ConfirmPhysicalDischargeCommand` | Perawat mencoba memulangkan pasien saat status clearance kasir masih `Pending` atau `Revoked` tanpa otorisasi override. | `RWI-DEC-158`, `RWI-AC-238` | `422 Unprocessable Entity` | *"Pelepasan fisik pasien ditolak: Tagihan kasir belum disetujui (Clearance Pending/Revoked). Pasien hanya dapat dilepaskan setelah kasir menerbitkan persetujuan lunas atau melalui otorisasi Supervisor Override."* |
| **`VAL-INT-002`** | `TransferBedCommand` / `CorrectOccupancyCommand` | Supervisor mencoba melakukan mutasi kamar atau koreksi kelas saat folio tagihan di kasir sudah berstatus `CLOSED` / `LOCKED`. | `RWI-DEC-157`, `RWI-AC-237` | `422 Unprocessable Entity` | *"Mutasi kamar ditolak: Tagihan kasir pasien sudah berstatus CLOSED. Data hunian kamar tidak dapat diubah kembali. Hubungi bagian Kasir/Keuangan bila diperlukan pembukaan kembali tagihan."* |
| **`VAL-INT-003`** | `TransferBedCommand` / `CorrectOccupancyCommand` | Kolom alasan mutasi atau koreksi kamar dikosongkan atau kurang dari 10 karakter. | `RWI-DEC-157`, `RWI-AC-237` | `400 Bad Request` | *"Alasan perubahan kamar wajib diisi minimal 10 karakter untuk keperluan jejak rekam audit."* |
| **`VAL-INT-004`** | `SupervisorOverrideCommand` | Kolom alasan kedaruratan klinis dikosongkan atau kurang dari 20 karakter. | `RWI-DEC-158`, `RWI-DEC-015` | `400 Bad Request` | *"Alasan supervisor override wajib diisi minimal 20 karakter dengan menyebutkan kondisi darurat medis atau rumah sakit rujukan secara jelas."* |
| **`VAL-INT-005`** | `SupervisorOverrideCommand` | Pengguna tidak memiliki peran Supervisor Bangsal atau PIN otorisasi salah. | `RWI-DEC-158`, `RWI-AC-238` | `403 Forbidden` | *"Otorisasi ditolak: Anda tidak memiliki hak wewenang Supervisor Rawat Inap atau PIN otorisasi yang dimasukkan salah."* |
| **`VAL-INT-006`** | `GetBillingDetailsQuery` | Pengguna mencoba mengakses endpoint rincian nominal rupiah tanpa permission `InpatientBilling:View`. | `RWI-DEC-160`, `RWI-AC-240` | `403 Forbidden` | *"Akses ditolak: Anda tidak memiliki hak akses untuk melihat rincian finansial dan nominal rupiah tagihan rawat inap."* |
| **`VAL-INT-007`** | `EnqueueOutboxCommand` | Kunci idempoten outbox kosong atau tidak mematuhi format `SourceDomain:SourceType:SourceDetailId:Version`. | `RWI-DEC-161`, `RWI-AC-241` | `400 Bad Request` | *"Format IdempotencyKey tidak valid. Kunci harus mengikuti format baku gabungan domain, tipe, ID detail, dan versi."* |
| **`VAL-INT-008`** | `ConfirmPhysicalDischargeCommand` | Jam kepulangan fisik (`PhysicallyLeftAt`) dimasukkan lebih awal dari jam masuk admisi atau jam mulai hunian kamar terakhir. | `RWI-DEC-159`, `RWI-AC-239` | `422 Unprocessable Entity` | *"Jam kepulangan fisik tidak valid: Waktu keluar fisik tidak boleh mendahului waktu pasien mulai menempati tempat tidur."* |
