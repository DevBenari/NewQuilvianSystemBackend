# Bukti `RHB-OQ-015` — Persetujuan pemilik ClinicalManagement / Billing

| Field | Isi |
| --- | --- |
| Pertanyaan terbuka | `RHB-OQ-015` — persetujuan pemilik ClinicalManagement/Billing agar Rehab Medik membuat `TrxPatientProcedure` dan memancarkan fakta charge (preseden Hemodialisa) |
| Yang disetujui | (1) Rehab Medik membuat satu `TrxPatientProcedure` per intervensi sesi terapi yang final (INT-RHB-08) dan memancarkan fakta kelayakan charge lewat `ClinicalMilestoneFactProducer` (INT-RHB-09), mengikuti pola `HmdBillingHandoffService`. (2) Nilai enum baru `PatientProcedureSource.RehabilitationSession = 5` di ClinicalManagement. (3) `DoctorId` dan `InstructingDoctorId` pada tindakan = dokter penanggung jawab episode (`RhbEpisode.ResponsibleDoctorId`, Sp.KFR); terapis tercatat sebagai pelaksana sesi. |
| Pemilik modul yang menyetujui | Pemilik ClinicalManagement / Billing (nama tidak disebutkan dalam konfirmasi) |
| Disampaikan oleh | Sukma Giri Pratama (owner Rehab Medik) |
| Tanggal konfirmasi | 2026-11-09 |
| Bentuk bukti | Jawaban owner Rehab Medik di sesi kerja agent, 2026-11-09, atas tiga pertanyaan bernomor: izin = *"Sudah disetujui"*; `DoctorId` = *"Dokter PJ episode"*; sumber tindakan = *"RehabilitationSession = 5 baru"*. |
| Batas bukti | Bukan dokumen yang ditulis atau ditandatangani pemilik ClinicalManagement/Billing sendiri; nama pemberi persetujuan belum dicatat. Bila nanti ada artefak (pesan, notulen, ID tiket), letakkan di folder ini dan tambahkan baris riwayat. |
| Dampak | `RHB-OQ-015` ditutup. `BE-RHB-013` (handoff billing sesi) tidak lagi terblokir keputusan; `FE-RHB-009` tetap menunggu `BE-RHB-013`. |

## Riwayat

| Tanggal | Perubahan |
| --- | --- |
| 2026-11-09 | Dibuat dari konfirmasi owner |
