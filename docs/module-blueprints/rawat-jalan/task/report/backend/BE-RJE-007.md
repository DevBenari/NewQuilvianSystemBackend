# Laporan Perubahan Backend — `BE-RJE-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-007` |
| Judul | Jasa konsultasi |
| Slice | `MVP-2` — `EPIC RJE-03` Jasa konsultasi |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-007` |
| Trace | `FR-RJE-020`, `FR-RJE-021`; `RJ-E2E-DEC-001`, `012`, `023`, `024`; `contracts/integration-contract.md` V2-2; `02-backend-architecture.md` V2.7.4 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-003` ✅ |
| Klasifikasi | `MEDIUM` — emisi fakta baru di finalisasi klinis, domain baru di jembatan, resolver tarif dengan urutan bertingkat |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `b4d9fe4c` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — keenam acceptance criteria terbukti; urutan tarif dilengkapi keputusan pemilik `RJ-E2E-DEC-023` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing`; titik sentuh `PharmacyManagement` (finalisasi konsultasi) |
| Registry | `Bil` `ACTIVE`; `Phm` `ACTIVE / LEGACY` |
| Keberlakuan | Perluasan service `NEW CODE` (`BE-RJE-003`) dan service finalisasi yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001` (fakta diterbitkan **setelah** commit klinis), `QBE-AUD-001` |
| Tidak berlaku | Model, configuration, endpoint, permission — tidak disentuh |
| Wewenang | `RJ-E2E-DEC-024` — source dan runtime ke `QuilvianNewDevSukma`, termasuk data uji master |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, jasa konsultasi dokter tidak pernah tertagih: Billing tidak mengenal sumber
`CONSULTATION`, dan finalisasi konsultasi hanya menerbitkan fakta resep.

**Contoh:** dr. X menyelesaikan konsultasi pasien kelas yang sesuai. Sesudah task ini, invoice
kunjungan langsung memuat item `CONSULTATION` Rp871.000 dari tarif *Jasa Medik Konsultasi* untuk
kelas pasien itu.

**Temuan yang mengubah keputusan.** Urutan tarif `RJ-E2E-DEC-012` mengandalkan tarif konsultasi
berklinik. Di master ternyata **2.130 tarif konsultasi seluruhnya tidak berklinik**: semuanya
ditautkan ke **tindakan** konsultasi (113 tindakan × 19 kelas pasien). Satu-satunya rule dokter
bertarif menunjuk tarif pemeriksaan darah. Tanpa perubahan, hampir semua konsultasi akan jatuh ke
antrean. Pemilik menyetujui `RJ-E2E-DEC-023`: tambahkan jalur **rule dokter → tindakan konsultasi →
tarif**, dan wajibkan tarif dari rule berjenis konsultasi.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Setiap konsultasi Rawat Jalan yang selesai menghasilkan tepat satu item jasa konsultasi berharga katalog |
| Pelaku | Dokter (menyelesaikan konsultasi); admin master (rule layanan dokter, tarif) |
| Pemicu | Finalisasi canonical — `PATCH /doctor-consultations/{id}/complete` atau `POST /doctor-queues/{id}/finish-consultation` |
| Langkah | 1. Validasi klinis (SOAP, diagnosis utama) dan commit klinis — tidak berubah. 2. **Sesudah commit**, fakta `Consultation`/`ConsultationCharge` (qty 1, tanpa nominal) diterbitkan sebelum fakta resep. 3. Folio → jembatan → resolver → item `CONSULTATION`/`COMPLETED` |
| Urutan tarif (`DEC-012` + `DEC-023`) | (1) rule dokter aktif-berlaku yang `TariffId`-nya **tarif konsultasi**; (1b) rule dokter → `ProcedureId` → tarif konsultasi tindakan itu, kelas pasien cocok lebih dulu; (2) tarif konsultasi klinik + kelas; (3) tarif konsultasi klinik; tidak ada → `TARIFF_NOT_FOUND`, tidak pernah Rp0. Rule yang paling spesifik (klinik, kelas) menang |
| Jalur tidak normal | Konsultasi batal sebelum selesai → tidak ada fakta. Konsultasi selesai dua kali → yang kedua ditolak modul klinis (catatan sudah ditandatangani). Penyerahan fakta ke folio gagal → dilaporkan di `BillingHandoffIssues` sebagai "Jasa konsultasi: <kode>" |
| Hasil akhir | Item `CONSULTATION` pada invoice kunjungan, atau antrean rekonsiliasi dengan sebab terbaca |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ConsultationFinalizationService.cs`, `ConsultationValidationService.cs` (diagnosis utama),
`DoctorConsultationController.cs` (`complete`, `cancel`), `PatientDiagnosisController.cs`,
`DoctorServiceRuleController.cs` + DTO, `MstDoctorServiceRule.cs`, `DoctorServiceRuleStatus.cs`,
`MstTariff.cs`, `TrxDoctorConsultation.cs`, `BillingClinicalChargeBridgeService.cs`,
`BillingSourceTariffResolver.cs`; query master tarif dan rule di `QuilvianNewDevSukma`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Menerbitkan fakta `Consultation` sesudah commit; masalah penyerahannya masuk `BillingHandoffIssues` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | `CONSULTATION` tidak lagi `SOURCE_PENDING_SUPPORT`; identitas = ID konsultasi; status tagih `COMPLETED` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolveConsultationAsync` dengan urutan `DEC-012` + `DEC-023`, termasuk pengaman `IsConsultationFee` untuk tarif dari rule |
| `docs/…/00-interview-decisions.md` | `RJ-E2E-DEC-023` (urutan tarif), `RJ-E2E-DEC-024` (wewenang); `DEC-012` ditandai dilengkapi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons finalisasi tetap; `BillingHandoffIssues` kini dapat memuat masalah jasa konsultasi |
| Database | Tanpa skema. Fakta baru di `CliClinicalMilestoneFact` dengan konteks `Consultation` (sudah terdaftar sejak `BE-RJE-002`) |
| Keamanan/Auth | Fakta tanpa nominal dan tanpa isi klinis; `RuleSnapshot` hanya memuat id dokter dan klinik |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah bentuk.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje007b` | `0 Error(s)`, `230 Warning(s)`, 2 menit 20 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R5 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`. Seluruh
finalisasi lewat endpoint klinis sungguhan. Data uji master dibuat lewat API master data (rule dokter)
dan satu tarif klinik uji disalin dari tarif konsultasi yang ada (bertanda `TEST-RJE007`).

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Rule dokter (dibuat lewat `POST /doctor-service-rules`) menunjuk tindakan konsultasi; konsultasi diselesaikan | `200`; konsultasi `Completed`; fakta `Consultation` v1 `Dispatched`; efek `Synced`; item `CONSULTATION`/`COMPLETED` qty 1 **Rp871.000** = tarif tindakan itu untuk kelas pasien | 1 (jalur 1b) | `PASS` |
| R2 | Konsultasi yang sama diselesaikan lagi | `400` "Catatan ini sudah ditandatangani dan tidak dapat diubah…"; item tetap 1, fakta tetap 1 | 5 | `PASS` |
| R3 | Tanpa rule; tarif konsultasi berklinik (uji) Rp99.000 untuk klinik konsultasi | Item `CONSULTATION` **Rp99.000** dari tarif klinik | 2, 3 | `PASS` |
| R4 | Tanpa rule dan tanpa tarif klinik | Konsultasi **tetap** `Completed`; efek `ReconciliationRequired` `TARIFF_NOT_FOUND`; nol item — tidak ada Rp0 | 4 | `PASS` |
| R5 | Konsultasi dibatalkan sebelum selesai | `200`; status `Cancelled`; nol fakta, nol efek, nol item | 6 | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.** (a) Run pertama R3 dan R4 ditolak validasi klinis
`MISSING_PRIMARY_DIAGNOSIS` sebelum sampai Billing. Aturan itu benar. Diagnosis utama ditambahkan lewat
`POST /patient-diagnoses` (endpoint yang dipakai layar dokter), lalu diulang. (b) Satu perintah
pencarian tanpa path macet menunggu input dan dihentikan tanpa efek. (c) Satu skrip pembersihan tidak
jalan karena salah pengalihan input, lalu diulang dari berkas.

**Tidak dijalankan:** pengaman "tarif dari rule wajib tarif konsultasi" dengan rule salah konfigurasi
yang sudah ada (`DSR-RSMMC-00001`) — tidak ada konsultasi berjalan untuk dokter rule itu; pengaman
diperiksa lewat kode. Catatan: `BillingHandoffIssues` pada R4 kosong karena penyerahan fakta ke folio
berhasil; kegagalan tarif terjadi di sisi Billing dan terlihat di antrean rekonsiliasi.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Konsultasi | 3 selesai (`6681b29b-…`, `c5698551-…`, `2495025d-…`), 1 dibatalkan (`f107e317-…`) — catatan `TEST-RJE007` |
| Diagnosis | 2 diagnosis utama uji (catatan `TEST-RJE007`) |
| Invoice | 2 invoice baru dengan item `CONSULTATION` (Rp871.000 dan Rp99.000) |
| Master | Rule `DSR-RSMMC-00002` dan tarif `TEST-RJE007-774532fe` — **dinonaktifkan** sesudah uji (`IsActive = false`) supaya tidak menagih konsultasi sungguhan berikutnya |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Rule dokter bertarif → tarif rule | Terpenuhi (jalur rule → tindakan, `DEC-023`) | R1 |
| 2. Tanpa rule, tarif klinik + kelas → dipakai | Terpenuhi (jalur klinik; master tidak punya tarif klinik+kelas, diuji dengan tarif klinik) | R3 |
| 3. Tanpa keduanya, tarif klinik → dipakai | Terpenuhi | R3 |
| 4. Tanpa tarif sama sekali → antrean `TARIFF_NOT_FOUND`, konsultasi tetap selesai | Terpenuhi | R4 |
| 5. Selesai ditekan dua kali → satu item | Terpenuhi | R2 |
| 6. Konsultasi batal sebelum selesai → tanpa fakta | Terpenuhi | R5 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Masalah yang diketahui | Master rule layanan dokter belum terisi di data dev. Sampai admin mengisi rule dokter + klinik → tindakan konsultasi, konsultasi masuk antrean `TARIFF_NOT_FOUND`. Ini keadaan data, bukan cacat kode |
| Risiko tersisa | Rule lama `DSR-RSMMC-00001` menunjuk tarif non-konsultasi; kini diabaikan pengaman, tetapi sebaiknya dibenahi admin |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | Satu pertanyaan kepada pemilik (`RJ-E2E-DEC-023`); satu perintah macet dihentikan tanpa efek |
| Status Git | 3 berkas source `M` dan dokumen blueprint. Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-008` (obat dua tahap), `BE-RJE-009` (pembatalan), `BE-RJE-010`, `BE-RJE-014` |
