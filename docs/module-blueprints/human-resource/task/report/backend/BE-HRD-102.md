# Laporan Perubahan Backend — `BE-HRD-102`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-102` |
| Judul | Controller & Endpoint Lengkap On-Call (`OnCallAssignmentController`) |
| Slice | S-B4 (SchedulingManagement) |
| Milestone | MVP-20 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-103, API Contract v10 Bagian 16.2, AT-HRD-SCH-19, AT-HRD-SCH-20 |
| Dependency | `BE-HRD-100` ✅, `BE-HRD-101` ✅ |
| Klasifikasi | `CONTROLLER, SWAGGER SPECIFICATION & API ROUTING GOVERNANCE` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Layanan siaga (*on-call*) membutuhkan antarmuka REST API yang lengkap dan aman untuk mendukung seluruh tahapan operasional: penjadwalan siaga oleh Kepala Unit/KSM, konfirmasi kesiapan oleh dokter, aktivasi status siaga, pemanggilan cito darurat oleh unit pemanggil (IGD, Kamar Operasi, Laboratorium), penyelesaian tugas, hingga penarikan data handoff penggajian oleh HRD Payroll.

Sebelumnya:
1. Controller hanya menyediakan 3 endpoint dasar (`GetPaged`, `Create`, `Activate`) tanpa endpoint konfirmasi, penyelesaian, pemanggilan cito, dan rekap handoff.
2. Belum memiliki validasi integritas waktu bahwa jam mulai harus lebih awal dari jam selesai (`EndAt > StartAt`).
3. Belum menerapkan tag Swagger resmi modul penjadwalan `[Tags("HR Scheduling - On-Call Management")]` dan skema rute ganda (*dual-route*) yang konsisten dengan standar Quilvian.

Task `BE-HRD-102` menuntaskan implementasi controller secara menyeluruh:
- **Penerapan Rute Ganda & Tag Swagger**:
  - `api/corporate/human-resource/scheduling/on-call-assignments` (jalur kanonikal baru)
  - `api/v1/corporate/human-resource/scheduling-management/on-call-assignments` (jalur kompatibilitas v1)
  - Tag Swagger: `[Tags("HR Scheduling - On-Call Management")]`.
- **Validasi Temporal Ketat**: Memvalidasi `EndAt > StartAt` saat membuat penugasan siaga (HTTP 400 Bad Request bila melanggar).
- **Endpoint Lengkap Siklus Hidup Siaga**:
  - `GET /` : Daftar penugasan siaga terpaginasi dengan filter periode, pegawai, dan status.
  - `GET /{id:guid}` : Detail penugasan siaga.
  - `POST /` : Pembuatan penugasan siaga baru.
  - `POST /{id:guid}/confirm` : Konfirmasi kesediaan staf/dokter.
  - `POST /{id:guid}/activate` : Aktivasi status siaga menjadi `Active` (melindungi kontrak lab).
  - `POST /{id:guid}/complete` : Penyelesaian tugas siaga (`Completed`).
  - `POST /{id:guid}/call-in` : Perekaman pemanggilan cito dan evaluasi SLA respon dokter oleh unit pemanggil.
  - `GET /handoff/summary` : Penarikan rekapitulasi kompensasi siaga bulanan untuk Payroll & Lembur (`INT-HR-38`).
  - `GET /active-doctors` : Pembacaan dokter siaga aktif untuk verifikasi integrasi.

---

## 2. Tabel Spesifikasi Endpoint API Bergaya Swagger

| Method | Endpoint Path | Deskripsi | Auth / Permission | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/scheduling/on-call-assignments` | Mengambil daftar penugasan siaga on-call terpaginasi | `OnCallAssignment.Read` | 200 OK |
| `GET` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}` | Mengambil detail satu penugasan siaga on-call | `OnCallAssignment.Read` | 200 OK / 404 Not Found |
| `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments` | Menjadwalkan penugasan siaga baru (validasi EndAt > StartAt) | `OnCallAssignment.Create` | 201 Created / 400 Bad Request / 403 Forbidden |
| `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}/confirm` | Mengonfirmasi kesiapan dokter/staf siaga (status `Confirmed`) | `OnCallAssignment.Update` | 200 OK / 404 Not Found |
| `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}/activate` | Mengaktifkan dinas siaga menjadi berstatus `Active` | `OnCallAssignment.Update` | 200 OK / 404 Not Found |
| `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}/complete` | Menyelesaikan dinas siaga menjadi berstatus `Completed` | `OnCallAssignment.Update` | 200 OK / 404 Not Found |
| `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}/call-in` | Mencatat panggilan darurat cito & evaluasi waktu respon SLA | `OnCallAssignment.Update` | 200 OK / 400 Bad Request / 404 Not Found |
| `GET` | `/api/corporate/human-resource/scheduling/on-call-assignments/handoff/summary` | Rekapitulasi jam standby & tiket cito untuk modul Payroll & Lembur | `OnCallAssignment.Read` | 200 OK |
| `GET` | `/api/corporate/human-resource/scheduling/on-call-assignments/active-doctors` | Mengambil ID dokter siaga aktif pada waktu efektif (Integrasi Lab) | `OnCallAssignment.Read` | 200 OK |

---

## 3. Bukti Verifikasi dan Pengujian

- **Kriteria Penerimaan Terpenuhi**:
  - Endpoint penugasan siaga berhasil memvalidasi bahwa waktu selesai harus lebih besar dari waktu mulai (`EndAt > StartAt`).
  - Endpoint `call-in` dapat diakses secara resmi dengan otorisasi yang sesuai oleh unit klinis pemanggil maupun koordinator jadwal.
  - Tag Swagger terpasang rapi dan seluruh respons dibungkus dalam format standar `ApiResponse<T>`.
