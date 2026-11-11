# Laporan Perubahan Backend — `BE-HRD-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-024` |
| Judul | DTOs dan Domain Service SK Kepegawaian (Personnel Action Order) |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-013`, `FR-HRD-014`, `HRD-DEC-046`, `HRD-DEC-048`, `VR-WFC-03`, `VR-WFC-04`, `VR-WFC-05`, `VR-WFC-12` |
| Dependency | `BE-HRD-023` |
| Klasifikasi | `HEAVY` |
| Task mode | `BACKEND` |
| Target tulis | DTOs, Services, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

Proses mutasi, promosi, rotasi, demosi, dan penugasan sementara pegawai rumah sakit memerlukan mesin bisnis yang menjamin bahwa perubahan unit, jabatan, dan atasan tidak diterapkan seketika tanpa validasi persetujuan. Diperlukan penomoran resmi berurutan (`SK-RSMMC-yyyyMMdd-0001`), larangan menyetujui SK sendiri (`VR-WFC-05`), kewajiban berkas fisik pindaian SK bertanda tangan sebelum diajukan (`VR-WFC-04`), pengecekan periode kehadiran yang sudah dikunci (`VR-WFC-03`), serta penerapan penempatan baru secara atomik saat disetujui.

---

## 2. Proses Bisnis Rumah Sakit

1. **Pembuatan Draft SK**: Staf HR membuat draft SK untuk pegawai dengan menentukan jenis SK, unit tujuan, jabatan tujuan, atasan tujuan, dan tanggal efektif.
2. **Unggah Pindaian SK Fisik**: Berkas fisik SK yang telah ditandatangani basah/resmi diunggah ke sistem.
3. **Pengajuan (Submit)**: Staf HR mengajukan draft ke Direktur SDM / Pejabat Penyetuju. Validasi `VR-WFC-04` memastikan berkas fisik telah diunggah.
4. **Persetujuan (Approval)**:
   - Validasi `VR-WFC-05` memastikan pembuat draft dilarang menyetujui SK-nya sendiri.
   - Validasi `VR-WFC-03` menolak tanggal efektif berlaku surut jika periode kehadiran/payroll bulan tersebut telah ditutup.
   - Secara atomik, sistem menonaktifkan penempatan aktif lama pegawai dan membuat penempatan aktif baru dengan tautan langsung ke SK pengesahan.
5. **Pembatalan / Penolakan**: SK dapat ditolak dengan alasan tercatat, atau dibatalkan dengan mencantumkan nomor SK pembatal pengganti (`VR-WFC-12`).

---

## 3. Rincian Perubahan yang Dikerjakan

### 3.1 Berkas Baru
1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/PersonnelActionOrderDtos.cs` — Berisi DTO request/response lengkap untuk filter, ringkasan, daftar, detail, buat draft, perbarui, penolakan, pembatalan, dan lampiran.
2. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/IHrdPersonnelActionOrderService.cs` — Antarmuka kontrak domain service.
3. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs` — Implementasi lengkap alur hidup SK Kepegawaian dan aturan bisnis.

---

## 4. Verifikasi Aturan Bisnis

* Nomor SK dihasilkan dengan format standar `SK-RSMMC-yyyyMMdd-0001` dengan advisory lock concurrency guard.
* Validasi anti-self approval (`VR-WFC-05`) diterapkan ketat: jika `order.SubmittedByUserId == userId || order.CreateBy == userId`, eksepsi ditolak.
* Seluruh penutupan penempatan lama dan pembentukan penempatan baru dieksekusi di dalam `BeginTransactionAsync`.
