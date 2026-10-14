# Laporan Perubahan Backend — `BE-BKC-FIX-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-019` |
| Judul | Seluruh jalur tindakan yang lahir/berakhir `Completed` menerbitkan fakta tagih; fakta yang hilang dipulihkan tanpa eksekusi ulang |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-07`; temuan `ISS-BKC-001-06` (sisi producer) |
| Keputusan | Kontrak milestone tindakan yang sudah berlaku (`RJ-BIL-DEC-002`: tindakan Completed = pemicu tagih) — tidak ada kebijakan baru |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error. Uji seluruh entry point di database `NOT RUN` |

## 1. Masalah

- `POST /patient-procedures` dengan `ExecuteImmediately = true` menyimpan tindakan langsung `Completed` tanpa menerbitkan fakta.
- `POST /patient-procedures/emergency-nursing-actions` mencatat tindakan keperawatan IGD langsung `Completed` tanpa fakta.
- Execute resmi atas tindakan yang sudah dikerjakan berhenti di `AlreadyExecuted` **sebelum** penerbitan fakta, sehingga menekan execute lagi tidak memulihkan apa pun.

## 2. Proses bisnis sesudah perbaikan

1. Tindakan yang tersimpan dalam keadaan `Completed` lewat jalur mana pun memanggil `PatientProcedureExecutionService.EnsureCompletedFactAsync` **sesudah commit**.
2. Metode itu menerbitkan fakta `Procedure`/`ProcedureCharge` hanya bila **belum ada satu pun** fakta tindakan tersebut; waktu kejadian = waktu selesai yang tersimpan.
3. Execute resmi dan penyelesaian dari Kamar Operasi atas tindakan yang sudah selesai: tidak ada yang dikerjakan ulang, tetapi fakta yang belum pernah terbit dipulihkan sekali.
4. Gagal Billing tidak membatalkan catatan klinis; tindakan gratis/tidak billable tetap diterbitkan dan Billing menandainya tidak ditagih.

> **Contoh.** Perawat IGD mencatat "Pemasangan infus" (Completed 10.20). Sebelumnya tidak ada fakta; kini satu fakta terbit 10.20 dan, bersama `BE-BKC-FIX-016`, tampil di invoice IGD. Kiriman ulang berkunci sama mengembalikan tindakan yang sama dan tidak menerbitkan fakta kedua.

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `ClinicalManagement/Services/PatientProcedureExecutionService.cs` | `EnsureCompletedFactAsync(entity|id)`; dipanggil pada cabang `AlreadyExecuted` di `ExecuteAsync` dan `ExecuteFromOperatingRoomAsync` |
| `ClinicalManagement/Controllers/PatientProcedureController.cs` | `CreateProcedure`: sesudah commit, tindakan `ExecuteImmediately` menerbitkan fakta. `CreateEmergencyNursingAction`: sesudah tersimpan, fakta diterbitkan |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response tidak berubah |
| Database | Tidak ada migration |
| Keamanan | Tidak berubah |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` | **0 error**, warning baru 0 |
| Uji seluruh entry point Completed + replay + kegagalan Billing di database | `NOT RUN` (butuh PostgreSQL; producer menolak berjalan di dalam transaksi, perilaku yang sudah diuji di modul asal) |

## 5. Acceptance criteria (`FIX-BKC-001-07`)

| AC | Bukti |
| --- | --- |
| 1. `ExecuteImmediately=true` → satu fakta setelah commit; execute biasa tetap satu fakta | `PatientProcedureController.CreateProcedure` sesudah `CommitAsync`; jalur execute tidak berubah |
| 2. Tindakan already executed tidak dijalankan ulang, fakta hilang dipulihkan idempoten | Cabang `AlreadyExecuted` memanggil pemulihan yang memeriksa keberadaan fakta lebih dulu |
| 3. Order belum dikerjakan/FoC tidak menagih; gagal kirim tidak menghapus catatan klinis | Pemulihan hanya untuk `IsExecuted && Completed`; FoC → resolver `NOT_BILLABLE`; emisi sesudah commit |

## 6. Risiko dan catatan

- Pemeriksaan "belum ada fakta" sengaja tidak menerbitkan revisi bila fakta sudah ada — perubahan isi tindakan sesudah selesai tidak ditagih ulang lewat jalur ini.
- Tindakan lama yang `Completed` tanpa fakta tidak pulih sendiri; pulih saat execute ditekan lagi atau lewat backfill terpisah.
