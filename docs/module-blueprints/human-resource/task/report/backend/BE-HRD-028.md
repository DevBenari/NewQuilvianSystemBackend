# Laporan Perubahan Backend — `BE-HRD-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-028` |
| Judul | Kunci Formulir Master Data Pegawai dan Permohonan Perubahan Profil Atas Nama (On-Behalf) |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-019`, `FR-HRD-020`, `HRD-DEC-047`, `HRD-DEC-055`, `VR-WFC-10` |
| Dependency | `BE-HRD-023` |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | Controller, Service, DTOs, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

1. **Jalan Pintas Perubahan Struktur Organisasi (`HRD-DEC-047`)**: Sebelumnya, form edit Master Data Pegawai (`EmployeeController.UpdateEmployee`) mengizinkan pengubahan langsung `PrimaryDepartmentId` dan `PrimaryPositionId`. Akibatnya, pemindahan unit atau perubahan jabatan bisa terjadi tanpa persetujuan, tanpa nomor SK, dan tanpa berkas fisik pendukung. Lebih berbahaya lagi, perubahan unit langsung mengubah hak akses pengguna di modul klinis.
2. **Ketiadaan Jalur Permohonan Atas Nama (`HRD-DEC-055`)**: Pegawai medis atau lapangan sering kali berhalangan mengajukan pembaruan biodata secara mandiri di ESS, sehingga staf HR perlu dapat mengajukan permohonan atas nama mereka dengan alasan dan surat kuasa/bukti pendukung.
3. **Celah Verifikasi Sendiri (`VR-WFC-10`)**: Belum ada aturan anti-self-verification / approval pada alur permohonan perubahan profil pegawai.

---

## 2. Proses Bisnis Rumah Sakit

* **Penguncian Formulir Master Data Pegawai**:
  - Pada `EmployeeController.UpdateEmployee`, jika terdapat perbedaan nilai antara `request.PrimaryDepartmentId` atau `request.PrimaryPositionId` dengan data yang tersimpan di database, server menolak permintaan dengan status `400 Bad Request`.
  - Pesan penolakan dengan tegas mengarahkan pengguna untuk memproses mutasi/rotasi melalui alur resmi Surat Keputusan (SK) Kepegawaian (Personnel Action Order).
* **Permohonan Perubahan Profil Atas Nama (`POST /on-behalf`)**:
  - Staf HR dapat mengajukan permohonan pembaruan data atas nama pegawai dengan mencantumkan alasan pengajuan atas nama (`OnBehalfReason`) dan mengunggah berkas kuasa/bukti pendukung (`EvidenceFile`).
  - Permohonan dicatat dengan flag `IsSubmittedOnBehalf = true`, `SubmittedByWorkforceProfileId`, dan timestamp pemberitahuan.
* **Penegakan Anti-Self-Verification / Approval (`VR-WFC-10`)**:
  - Pada tahap verifikasi (`DecideVerificationAsync`) dan persetujuan (`ApproveAsync`), sistem memastikan pemohon (`RequestedByUserId`) maupun staf HR yang mengajukan atas nama (`CreateBy`) dilarang keras menjadi verifikator atau penyetujui permohonannya sendiri.

---

## 3. Rincian Perubahan yang Dikerjakan

1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs` — Menambahkan guard pencegah perubahan langsung `PrimaryDepartmentId` dan `PrimaryPositionId`.
2. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/EmployeeProfileChangeDtos.cs` — Menambahkan properti on-behalf pada DTO respons dan kontrak request `CreateOnBehalfProfileChangeRequest`.
3. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/EmployeeProfileChangeService.cs`:
   - Menambahkan guard anti-self-verification pada `DecideVerificationAsync`.
   - Menambahkan guard anti-self-approval pada `ApproveAsync`.
   - Menambahkan metode `CreateOnBehalfDraftAsync` dengan penyimpanan bukti kuasa.
   - Memperbarui pemetaan proyeksi respons `GetPagedAsync` dan `BuildResponse`.
4. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Controllers/EmployeeProfileChangeController.cs` — Menambahkan endpoint `POST api/v1/corporate/human-resource/employee-profile-changes/on-behalf`.

---

## 4. Verifikasi dan Kepatuhan

* Pemisahan tugas (*separation of duties*) antara pembuat pengajuan dan penyetuju ditegakkan di level backend, mencegah manipulasi data biodata dan rekening pegawai rumah sakit.
