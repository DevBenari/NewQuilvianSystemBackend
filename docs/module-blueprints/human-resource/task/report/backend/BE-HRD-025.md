# Laporan Perubahan Backend — `BE-HRD-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-025` |
| Judul | Controller Web API Surat Keputusan Kepegawaian (Personnel Action Order) |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-013`, `FR-HRD-014`, `HRD-DEC-046`, `HRD-DEC-048` |
| Dependency | `BE-HRD-024` |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | Controller, Program.cs, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

Diperlukan endpoint REST API resmi yang aman, terotorisasi, terdokumentasi rapi di Swagger UI, dan dilengkapi mekanisme upload berkas pindaian dokumen fisik untuk seluruh siklus hidup Surat Keputusan Kepegawaian.

---

## 2. Proses Bisnis Rumah Sakit

* **Rute API**: `api/v1/corporate/human-resource/personnel-action-orders`
* **Area/Tag Swagger**: `Corporate / Human Resource / Workforce Core / Personnel Action Order`
* **Hak Akses**: Terhubung ke `[AccessController]` dan `[AccessPermission("HrdPersonnelActionOrder", ...)]` tanpa string role statis yang rawan celah keamanan.
* **Fitur Utama**:
  - `GET /filters/metadata` — Mengambil pilihan filter dinamis untuk antarmuka frontend.
  - `GET /summary` — Mengambil metrik ringkasan kartu statistik (Draft, Diajukan, Disetujui, Ditolak).
  - `GET /` — Mengambil daftar berpaginasi SK dengan penyaringan status dan pencarian nama/kode pegawai.
  - `GET /{id}` — Mengambil detail lengkap dokumen SK, perbandingan unit/jabatan asal vs target, dan lampiran.
  - `POST /` — Membuat draft SK baru.
  - `PUT /{id}` — Memperbarui draft SK.
  - `POST /{id}/attachments` — Mengunggah berkas fisik pindaian SK bertanda tangan basah/resmi (format PDF/JPG/PNG, batas 10 MB).
  - `GET /{id}/attachments/{attachmentId}` — Mengunduh berkas fisik pindaian SK secara aman.
  - `POST /{id}/submit` — Mengajukan draft SK untuk persetujuan berjenjang.
  - `POST /{id}/approve` — Menyetujui SK dan menerapkan penempatan secara otomatis.
  - `POST /{id}/reject` — Menolak SK dengan alasan tercatat.
  - `POST /{id}/cancel` & `POST /{id}/cancel-request` — Membatalkan SK.

---

## 3. Rincian Perubahan yang Dikerjakan

1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdPersonnelActionOrderController.cs` — Berisi 13 aksi endpoint RESTful yang mencakup seluruh alur SK Kepegawaian.
2. `NewQuilvianSystemBackend/Program.cs` — Mendaftarkan `IHrdPersonnelActionOrderService` dan `HrdPersonnelActionOrderService` ke dalam kontainer Dependency Injection ASP.NET Core (`builder.Services.AddScoped`).

---

## 4. Verifikasi dan Kepatuhan

* Swagger Group: Menggunakan `[Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]`.
* Akses Otorisasi: Menggunakan `[AccessAction]` dan `[AccessPermission]` sesuai standar arsitektur Quilvian.
