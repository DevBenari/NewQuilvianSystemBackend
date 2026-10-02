# Laporan Perubahan Backend — `BE-RJE-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-013` |
| Judul | `BillingHandoffIssues` pada `finish-consultation` |
| Slice | `MVP-4` — `EPIC RJE-07` |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-013` |
| Trace | `FR-RJE-062`; `RJ-E2E-FE-004`; `RJ-E2E-DEC-017` (kontrak `1.0.1` approved), `026`; `contracts/api-contract.md` V2.1 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.1` — `approved` |
| Dependency | Kontrak `1.0.1` ✅ |
| Klasifikasi | `LOW` — satu field tambahan pada respons yang sudah ada |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2af16d93` (`sukmagp`) + perubahan `BE-RJE-011` yang belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — ketiga acceptance criteria terbukti |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (antrean dokter) |
| Registry | `Reg` `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `DoctorQueueController.FinishConsultation`, `DoctorQueueActionResponse` |
| QBE yang berlaku | `QBE-VAL-001` (kontrak respons aditif), `QBE-SVC-001` (nilai disalin dari service finalisasi, tanpa logika baru di controller) |
| Tidak berlaku | Model, migration, endpoint baru, permission |
| Wewenang | `RJ-E2E-DEC-026` |

---

## 1. Masalah yang diperbaiki

Layar antrean dokter menyelesaikan konsultasi lewat `POST /doctor-queues/{id}/finish-consultation`.
Jalur itu memanggil finalisasi canonical, tetapi membuang daftar masalah penyerahan tagihan yang
dihasilkannya. Dokter yang menekan *Selesai* dari antrean tidak pernah tahu bila jasa konsultasi
atau resepnya gagal diserahkan ke Billing.

**Contoh:** konsultasi selesai, tetapi fakta jasa konsultasinya tertahan karena revisi sebelumnya
belum pasti. Respons sekarang tetap `200` "Konsultasi dokter selesai.", ditambah
`billingHandoffIssues: ["Jasa konsultasi: CLIN_FACT_RECONCILIATION_REQUIRED"]`.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Dokter yang menyelesaikan dari antrean melihat masalah penyerahan tagihan, tanpa konsultasinya gagal |
| Pelaku | Dokter (layar antrean) |
| Langkah | Finalisasi canonical berjalan seperti sebelumnya → daftar `BillingHandoffIssues` hasil finalisasi disalin apa adanya ke respons antrean |
| Jalur tidak normal | Masalah penyerahan tidak mengubah kode status: tetap `200`, konsultasi `Completed`. Aksi antrean lain mengembalikan daftar kosong |
| Hasil akhir | Frontend (`FE-RJE-002`) dapat menampilkan masalah itu |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorQueueController.FinishConsultation` dan `BuildActionResponse`, `DoctorQueueDtos.cs`,
`ConsultationFinalizationService` (`ConsultationFinalizationOperationResult.Data.BillingHandoffIssues`),
`ClinicalMilestoneFactProducer.DecideNextStep` (CASE C), kontrak V2.1.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/DoctorQueueDtos.cs` | `DoctorQueueActionResponse.BillingHandoffIssues: List<string>`, bawaan kosong |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | `FinishConsultation` menyalin `result.Data.BillingHandoffIssues` ke respons |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif sesuai `1.0.1`: field baru `billingHandoffIssues` (array string). Semua field lama tetap |
| Database | Tidak ada |
| Keamanan/Auth | Tidak berubah. Isi daftar hanya nama pelayanan dan kode, tanpa isi klinis |

---

## 4. Dokumentasi endpoint

### Health Services / Registration Management / Doctor Queues

Base URL: `api/v1/health-services/registration-management/doctor-queues`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/finish-consultation` | Menyelesaikan konsultasi dari antrean dokter | `DoctorQueue : FinishConsultation` (tidak berubah) | `DoctorQueueActionRequest` (tidak berubah) | `DoctorQueueActionResponse` **+ `billingHandoffIssues: string[]`** | **Tersedia** |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje013` | `0 Error(s)`, `230 Warning(s)` (= baseline); tidak ada warning baru | `PASS` | Warning di berkas antrean adalah warning lama |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R3 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 30 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Antrean `3e3297b9…` (sedang konsultasi). Fakta jasa konsultasi sebelumnya berstatus belum pasti dan menunggu rekonsiliasi (ditanam sintetis), lalu *Selesai* ditekan dari antrean | `200` "Konsultasi dokter selesai."; `billingHandoffIssues = ["Jasa konsultasi: CLIN_FACT_RECONCILIATION_REQUIRED"]`; antrean `Completed`; konsultasi `Completed` di database | 1 | `PASS` |
| R2 | Antrean `8edc1a13…` tanpa masalah penyerahan | `200`; `billingHandoffIssues = []` | 2 | `PASS` |
| R3 | Bentuk respons dan endpoint antrean lain | Respons *Selesai* memuat seluruh field lama beserta field baru (46 kunci); `GET /doctor-queues` tetap `200` | 3 | `PASS` |

**Kondisi yang disiapkan lewat SQL.**
- SOAP kedua konsultasi uji diisi (bertanda `TEST-RJE013`), karena jalur antrean tidak mengirim
  SOAP; diagnosis utama ditambahkan lewat API.
- R1 memakai fakta konsultasi sintetis `OutcomeUnknown` yang menunggu rekonsiliasi. Tanpa gangguan
  sungguhan, cara ini yang membuat producer menolak revisi baru lewat jalur CASE C yang memang ada
  di kode.

**Percobaan yang tidak dipakai sebagai bukti.** `GET /doctor-queues/{id}` dijawab `404` karena route
itu memang tidak ada di controller; tidak terkait perubahan ini.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Konsultasi | `ac5027c5…` dan `b1582356…` `Completed`; antrean `3e3297b9…`, `8edc1a13…` `Completed` |
| Fakta | Fakta sintetis `TEST-RJE013-FACT-*` (menunggu rekonsiliasi) atas konsultasi `ac5027c5…` — akan muncul di antrean `BE-RJE-012` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Billing gagal saat Selesai → `200` memuat daftar masalah; konsultasi selesai | Terpenuhi | R1 |
| 2. Tanpa masalah → daftar kosong | Terpenuhi | R2 |
| 3. Konsumen lama tidak rusak | Terpenuhi (perubahan aditif) | R3 |
| DoD: laporan | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Risiko tersisa | Masalah berjenis `OutcomeUnknown` dianggap aman secara klinis dan **tidak** masuk daftar; fakta itu dikirim ulang otomatis oleh pekerja `BE-RJE-011`. Tanda ⛔ pada `FE-RJE-002` di traceability sudah basi (kontrak `1.0.1` approved), tetapi berada di luar wewenang penandaan task backend ini |
| Perubahan sampingan | `NONE`. Data uji: bagian 5.2 |
| Status Git | Belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-012` (API antrean rekonsiliasi dan kebijakan) |
