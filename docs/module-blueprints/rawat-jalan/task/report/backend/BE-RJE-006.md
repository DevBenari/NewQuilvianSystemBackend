# Laporan Perubahan Backend — `BE-RJE-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-006` |
| Judul | Kunjungan tanpa dokter berhenti di `Billing` |
| Slice | `MVP-1` — `EPIC RJE-08` Status kunjungan tanpa dokter |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-006` |
| Trace | `FR-RJE-070`; `RJ-E2E-DEC-004`, `007`, `013`, `021`; `02-backend-architecture.md` V2.8; `contracts/state-transition-matrix.md` V2-D |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | — (gelombang 1) |
| Klasifikasi | `LIGHT` — satu cabang status dan satu pesan pada satu controller (titik sentuh Registration) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d98ff3ea` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kedua acceptance criteria terbukti; preflight dampak tercatat (bagian 2.1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (titik sentuh) |
| Registry | `Reg` `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — `NurseStationQueueController` memakai `ApplicationDbContext` langsung (utang teknis yang sudah ada, **tidak** dirapikan di task ini) |
| QBE yang berlaku | `QBE-API-001` (bentuk respons dipertahankan), `QBE-LOG-001` (alur log yang ada tidak diubah) |
| Tidak berlaku | `QBE-SVC-001` — tidak menambah akses context baru; pola lama dibiarkan sesuai legacy ratchet. `QBE-ENT/CFG/NAM/MOD-*` — tanpa model |
| Wewenang | `RJ-E2E-DEC-021` — source dan runtime ke `QuilvianNewDevSukma`, termasuk penyesuaian data uji antrean |

---

## 1. Masalah yang diperbaiki

Pasien Rawat Jalan yang hanya perlu skrining perawat (tanpa dokter) langsung ditutup menjadi
`Completed` begitu perawat menekan Selesai Skrining. Kunjungan tertutup **sebelum** ditagih, padahal
penutupan kunjungan bukan wewenang Rawat Jalan (`RJ-E2E-DEC-007`).

**Contoh:** pasien kontrol tekanan darah di poli tanpa dokter. Sebelumnya status kunjungannya langsung
*Selesai*; sesudah task ini statusnya *Billing* (8) dan menunggu kasir, sama seperti pasien yang
diperiksa dokter berhenti di *Konsultasi Selesai*.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Rawat Jalan tidak pernah menutup kunjungan; kunjungan tanpa dokter diserahkan ke Billing |
| Pelaku | Perawat poli |
| Pemicu | `POST /nurse-station-queues/{id}/finish-screening` pada antrean yang tidak memerlukan dokter |
| Langkah | 1. Pengkajian aktif diselesaikan dan tanda vital dicatat (tidak berubah). 2. Antrean perawat menjadi `Completed` (tidak berubah). 3. **Kunjungan menjadi `Billing`**, `CompletedAt` **tidak** diisi. 4. Dokumen rekam medis yang masih draf tetap dikunci (`RM-DEC-003`), dengan waktu kunci = waktu skrining selesai |
| Aturan | Penutupan ke `Completed` menunggu `RJ-E2E-DEC-004` (`POST-MVP`) |
| Perubahan status | `InNurseScreening` → `Billing` (tanpa dokter); `InNurseScreening` → `WaitingForDoctor` (dengan dokter, tidak berubah) |
| Jalur tidak normal | Transaksi gagal → seluruh perubahan dibatalkan (perilaku yang sudah ada) |
| Hasil akhir | Kunjungan muncul di daftar kunjungan aktif kasir dengan status `Billing` |

### 2.1 Preflight dampak (wajib menurut kartu task)

Pembaca `EncounterStatus.Completed`/`CompletedAt` yang terdampak karena kunjungan tanpa dokter kini
**tidak** lagi `Completed`:

| Pembaca | Perilaku sesudah perubahan | Penilaian |
| --- | --- | --- |
| `MedicalRecordAccessAuditService.KunjunganMasihBerjalan` | Kunjungan dianggap masih berjalan; akses rekam medis tidak dimintai keperluan sampai kunjungan ditutup | Sama persis dengan kunjungan jalur dokter hari ini (`ConsultationCompleted` juga tidak termasuk status selesai). Menyamakan dua jalur, bukan celah baru. Tetap bergantung `RJ-E2E-DEC-004` |
| `BbkEncounterStatusReader`, `HmdServiceSupport` (status tertutup) | Order darah/hemodialisis masih menahan order baru sampai kunjungan ditutup | Sama dengan jalur dokter |
| `LabOrderService` (`VAL-67`) | Order Lab baru masih boleh dibuat | Sama dengan jalur dokter |
| `MedicalRecordBackfillService` | Tidak lagi memilih kunjungan ini sebagai "selesai" untuk pengisian kunci | Dokumen sudah dikunci saat skrining; tidak ada yang tertinggal |
| `KioskEncounterClosureService` | Menyaring `CompletedAt == null` dan status bukan `Completed` | Hanya kunjungan kiosk yang belum *check-in*; kunjungan yang sudah diskrining tidak tersentuh |
| `BillingInvoiceService` pilihan kunjungan aktif | Memuat `Billing` | Dampak positif — kasir menemukan kunjungan (R3) |
| `PatientEncounterController` (`PATCH` status) | Tidak berubah | Tetap jalan penutupan manual sampai `RJ-E2E-DEC-004` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`NurseStationQueueController.cs` (`FinishScreening`), `ClinicalDocumentIntegrityService.cs`
(`LockOpenDocumentsForEncounterAsync`), seluruh pembaca pada bagian 2.1, `EncounterStatus.cs`,
`QueueStatus.cs`, `TrxQueue.cs`, `BillingInvoiceService.GetActiveEncounterOptionsAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/NurseStationQueueController.cs` | Cabang tanpa dokter: `EncounterStatus.Billing` menggantikan `Completed`; `Encounter.CompletedAt` tidak lagi diisi; penguncian dokumen dipertahankan dengan komentar yang diperjelas; pesan respons menjadi "Screening perawat selesai dan kunjungan diserahkan ke Billing." |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tetap. Nilai `encounterStatus` pada respons berubah `9` → `8` untuk kunjungan tanpa dokter, dan kalimat pesannya berubah. Frontend tidak membandingkan kalimat pesan (dicari di `src/`, nol hasil) |
| Database | `NOT APPLICABLE` — tanpa skema; status `Billing (8)` sudah ada di enum |
| Keamanan/Auth | Lihat bagian 2.1 (definisi "kunjungan masih berjalan" untuk akses rekam medis). Atribut hak akses tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Nurse Station Queue

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/finish-screening` | Menyelesaikan skrining perawat; kunjungan tanpa dokter kini diserahkan ke Billing | tidak berubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje006b` | `0 Error(s)`, `230 Warning(s)`, 2 menit 6 detik | `PASS` | Sama dengan baseline. 10 warning `CS8602` di berkas yang sama berada di baris 520-an dan 1064-an — sudah ada sebelumnya, bukan baris task |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R3 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi hasil build pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`.
Data dev tidak memiliki antrean aktif yang **tidak** memerlukan dokter (107 antrean aktif, semuanya
`IsDoctorRequired = true`). Karena itu satu antrean berstatus *sedang diskrining* diubah
`IsDoctorRequired = false` lewat SQL sebagai data uji, lalu skrining diselesaikan lewat API sungguhan.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Selesai skrining, antrean tanpa dokter | `200` "Screening perawat selesai dan kunjungan diserahkan ke Billing."; kunjungan `4` → **`8` (`Billing`)**; `CompletedAt` tetap kosong; antrean `Completed` (`10`), `ScreeningCompletedAt` terisi | 1 | `PASS` |
| R2 | Selesai skrining, antrean dengan dokter | `200` "…pasien dikirim ke dokter."; kunjungan `4` → `5` (`WaitingForDoctor`); `CompletedAt` kosong — tidak berubah | 2 | `PASS` |
| R3 | Kunjungan R1 di pilihan kunjungan aktif kasir (`GET encounter-options?search=<nomor>`) | Ditemukan, status `Billing` | — | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.** (a) Run pertama R1 lulus dengan pesan lama "…kunjungan
diselesaikan." yang menyesatkan; pesan diperbaiki, dibangun ulang, dan R1–R2 diulang dengan pasangan
antrean baru. (b) Pemeriksaan R3 pertama tanpa kata kunci hanya mengembalikan 25 opsi (batas bawaan
`limit`), sehingga kunjungan tidak terlihat; diulang dengan pencarian nomor kunjungan.

**Tidak dijalankan:** bukti penguncian dokumen pada kunjungan tanpa dokter — kedua kunjungan uji tidak
memiliki dokumen klinis draf (0 baris integritas), sehingga penguncian terpanggil tanpa sasaran.
Perilaku penguncian tidak diubah task ini.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Antrean `d3b7a3ca-3972-4608-aa8f-e554549374ae` (run pertama) | `IsDoctorRequired = false` (diubah uji); skrining selesai; kunjungannya `Billing` |
| Antrean `18139171-de5f-4f8e-8123-13f533f15d24` (run kedua) | `IsDoctorRequired = false` (diubah uji); skrining selesai; kunjungan `18e791a8-…` (`ENC-RSMMC-00015`) `Billing` |
| Dua antrean dengan dokter | Skrining selesai; kunjungan `WaitingForDoctor` |

**Dampak ke pengujian berikutnya:** kedua kunjungan `Billing` di atas dapat dipakai sebagai data uji
kunjungan tanpa dokter pada `BE-RJE-014` (ringkasan tanpa invoice) dan penutupan kunjungan kelak.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tanpa dokter → `Billing`, `CompletedAt` kosong (`UAT-20`) | Terpenuhi | R1 |
| 2. Dengan dokter → `WaitingForDoctor` tidak berubah (`UAT-21`) | Terpenuhi | R2 |
| Preflight: pembaca `Completed`/`CompletedAt` dan dampaknya tercatat | Terpenuhi | Bagian 2.1 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 10 warning `CS8602` lama di berkas yang sama (bukan baris task) |
| Masalah yang diketahui | Kunjungan tanpa dokter, seperti jalur dokter, tetap "berjalan" sampai `RJ-E2E-DEC-004` diputuskan (bagian 2.1) |
| Risiko tersisa | Laporan atau layar yang menghitung kunjungan "selesai" dari status `Completed` akan menghitung kunjungan tanpa dokter sebagai belum selesai — konsisten dengan jalur dokter |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | Satu jeda sesi (batas penggunaan) setelah R3; dilanjutkan dari keadaan terverifikasi tanpa mengulang build |
| Status Git | 1 berkas source `M` dan dokumen blueprint. Belum di-stage atau di-commit |
| Langkah berikutnya | Gelombang 1 selesai. Gelombang 4: `BE-RJE-005`, `BE-RJE-007`, `BE-RJE-010`, `BE-RJE-014` |
