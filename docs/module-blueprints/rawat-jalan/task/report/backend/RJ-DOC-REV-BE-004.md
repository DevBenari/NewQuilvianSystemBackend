# Laporan Perubahan Backend — `RJ-DOC-REV-BE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-004` |
| Judul | Surat dokter: persistence, API, dan riwayat |
| Slice | Revisi UAT `2026-09-28` — Dokter Rawat Jalan butir 6a–6d |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007`; temuan audit `A2` (endpoint `/doctor-certificates` dipanggil frontend tetapi tidak ada) |
| Contract version | Kontrak baru `doctor-certificates` mengikuti payload frontend yang sudah ada (`buildDoctorCertificatePayload`) |
| Dependency | Tidak ada |
| Klasifikasi | `HEAVY` — repo 0, diperiksa 1, diubah 2 (11 berkas), logika 2, API 2 (endpoint baru), database 2 (tabel baru), keamanan 1 (hak akses baru), UI 0 → skor 10 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `Repositories/**`, `Migrations/**`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Registry | `ClinicalManagement / Clinical` — prefix `Cli`, `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` (entity, service, controller baru); `TOUCHED LEGACY` (`ApplicationDbContext`, `Program.cs`) |
| QBE yang berlaku | QBE-ENT-001, QBE-NAM-001/002, QBE-CFG-001, QBE-MOD-001, QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-LOG-001, QBE-CODE-001/002/003/004, QBE-VAL-001, QBE-DTO-001, QBE-DEL-001, QBE-PAGE-001 |
| Arketipe transaksi | Aggregate ber-lifecycle sederhana: `Issued` → `Cancelled` |

---

## 1. Masalah yang diperbaiki

Tab Surat Dokter memanggil `POST /doctor-certificates`, tetapi endpoint itu **tidak pernah ada** di
backend. Layar menelan galatnya supaya finalisasi konsultasi tidak terblokir, sehingga dokter mengira
surat tersimpan padahal tidak. Akibatnya:

- surat sakit/sehat/rujukan tidak pernah tersimpan dan tidak bisa dicetak ulang;
- riwayat surat dokter (butir 6b) tidak mungkin dibuat;
- tujuan rujukan hanya teks bebas, tidak terhubung ke master unit/klinik (butir 6c);
- nama dokter pada surat diketik dari layar, bukan Dokter Penanggung Jawab kunjungan (butir 6d).

---

## 2. Proses bisnis

1. Dokter mengisi surat pada tab Surat Dokter, lalu menekan *Preview Surat*.
2. Saat *Selesai Konsultasi*, layar mengirim surat ke `POST /doctor-certificates`.
3. Backend memvalidasi:
   - jenis surat dikenali (`sickLeave`, `health`, `inpatientReferral`);
   - kunjungan ada, dan antrean (bila dikirim) memang milik kunjungan itu;
   - **surat sakit**: tanggal mulai dan selesai wajib, selesai tidak boleh sebelum mulai;
   - **surat sehat**: data pemeriksaan wajib ada;
   - **surat rujukan**: unit atau klinik tujuan **wajib dipilih dari master** dan masih aktif.
4. Backend mengisi **dokter = Dokter Penanggung Jawab kunjungan** (`RegPatientEncounter.DoctorId`),
   mengabaikan `doctorId` kiriman layar. Identitas pasien, nama klinik, dan nama tujuan rujukan
   disimpan sebagai snapshot.
5. Backend menerbitkan nomor surat dari deret milik modul: `SKS-000001` (sakit), `SKH-000001` (sehat),
   `SRJ-000001` (rujukan). Nomor buatan layar diabaikan.
6. Lama istirahat dihitung backend dari tanggal: contoh 1–3 Oktober = **3 hari**.
7. Surat dapat diubah selama belum dibatalkan. Jenis surat tidak dapat diubah — batalkan lalu terbitkan baru.
8. Surat dibatalkan lewat `POST /{id}/cancel` dengan alasan wajib. Surat tidak pernah dihapus.
9. Riwayat surat per pasien/kunjungan dibaca lewat `GET /?patientId=…`, terbaru di atas, termasuk
   yang dibatalkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorConsultationController.cs` (pola controller/akses), `BbkBloodOrderService.cs` (pola alokasi nomor),
`NumberSeriesAllocator.cs`, `NumberAllocationRequest.cs`, `CliClinicalMilestoneFactConfiguration.cs`,
`ApplicationDbContext.cs`, `MstClinic.cs`, `MstServiceUnit.cs`, `MstDoctor.cs`, `RegPatientEncounter.cs`,
registry prefix, standar endpoint transaksi, frontend `use-doctor-certificate.js`,
`doctor-certificate.utils.js`, `doctor-certificate.constants.js`, `doctor-queue.service.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/DoctorCertificateEnums.cs` (baru) | `DoctorCertificateType`, `DoctorCertificateStatus` |
| `Areas/HealthServices/ClinicalManagement/Models/CliDoctorCertificate.cs` (baru) | Entity surat dokter |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliDoctorCertificateConfiguration.cs` (baru) | Tabel, panjang kolom, index unik nomor surat, FK kunjungan/pasien |
| `Areas/HealthServices/ClinicalManagement/DTOs/DoctorCertificateDtos.cs` (baru) | Request/response mengikuti payload layar |
| `Areas/HealthServices/ClinicalManagement/Services/DoctorCertificateService.cs` (baru) | Validasi, nomor surat, snapshot, riwayat, batal, audit |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorCertificateController.cs` (baru) | Tujuh endpoint |
| `Repositories/ApplicationDbContext.cs` | DbSet `CliDoctorCertificates` |
| `Program.cs` | Registrasi `DoctorCertificateService` |
| `Migrations/20261001040508_AddCliDoctorCertificate.cs` + `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs` | Migration tabel baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `doctor-certificates`. Delta terhadap payload layar: `certificateNumber`, `doctorId`, `doctorName`, `clinicName`, `medicalRecordNumber`, dan `sickLeave.durationDays` diabaikan — ditetapkan backend. Field baru `inpatientReferral.targetServiceUnitId`/`targetClinicId` menggantikan teks bebas `targetServiceUnit` |
| Database | Tabel baru `public."CliDoctorCertificate"`, `4` index (1 unik), `2` FK `Restrict`. Diterapkan ke `QuilvianNewDevSukma` |
| Keamanan/Auth | Hak akses baru `DoctorCertificate : Read/Create/Update/Cancel`. **Admin wajib mencentangnya** untuk posisi dokter di layar Akses Role sebelum layar Surat Dokter dapat menyimpan |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Certificate

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/doctor-certificates?patientId=&encounterId=&certificateType=&includeCancelled=` | Riwayat surat dokter pasien/kunjungan (wajib salah satu filter) | `DoctorCertificate : Read` |
| `GET` | `/doctor-certificates/referral-targets?search=` | Pilihan unit layanan dan klinik aktif untuk tujuan rujukan, dapat dicari | `DoctorCertificate : Read` |
| `GET` | `/doctor-certificates/active-by-queue/{queueId}` | Surat terbit terakhir pada antrean | `DoctorCertificate : Read` |
| `GET` | `/doctor-certificates/{id}` | Detail surat, termasuk tanda tangan | `DoctorCertificate : Read` |
| `POST` | `/doctor-certificates` | Menerbitkan surat | `DoctorCertificate : Create` |
| `PUT` | `/doctor-certificates/{id}` | Mengubah surat yang belum dibatalkan | `DoctorCertificate : Update` |
| `POST` | `/doctor-certificates/{id}/cancel` | Membatalkan surat dengan alasan | `DoctorCertificate : Cancel` |

`GET /filters/metadata` dan `GET /summary` standar transaksi **tidak** dibuat: layar Surat Dokter
tidak memakainya. Dicatat sebagai delta, bukan didiamkan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -o <scratchpad>/build` (`--no-incremental`) | `0 Error(s)`, `244 Warning(s)`; **nol** peringatan dari berkas task | `PASS` | Filter keluaran build per nama berkas |
| `dotnet ef migrations add AddCliDoctorCertificate` | Satu `CreateTable` + `4` `CreateIndex`, tanpa operasi lain | `PASS` | — |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `dotnet ef database update` ke `QuilvianNewDevSukma` | `Applying migration '20261001040508_AddCliDoctorCertificate'. Done.` | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `17` berkas, `VIOLATION 0`, `REVIEW 0`, `Final result: PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test |
| Runtime R0–R13 | Seluruhnya sesuai harapan sesudah dua perbaikan (5.1) | `PASS` | Bagian 5.1 |

### 5.1 Hasil validasi runtime — 1 Oktober 2026

Aplikasi scratchpad `localhost:5217`, DB `QuilvianNewDevSukma`, sesi `superadmin`. Antrean uji `G002`
(`2026-07-15`, DPJP `dr. Maya Permata Sari`).

| No | Skenario | Hasil sebenarnya | Klasifikasi |
| ---: | --- | --- | :---: |
| R0 | Login; tanpa sesi | `200`; `401` | `PASS` |
| R1 | `GET /referral-targets?search=rawat` | `200`, `30` pilihan: unit `Rawat Inap`, `Rawat Jalan`, …, klinik | `PASS` |
| R2 | Surat sakit 1–3 Okt, `doctorId` palsu dan nomor `SKS/CLIENT/001` dari klien | Percobaan awal `500` — `Cannot write DateTime with Kind=Unspecified` → diperbaiki. Sesudahnya `200`, nomor dari backend, `doctorName = dr. Maya Permata Sari`, `durationDays = 3` | `PASS` |
| R3 | Selesai istirahat sebelum mulai | `400` "Tanggal selesai istirahat tidak boleh sebelum tanggal mulai." | `PASS` |
| R4 | Rujukan dengan teks bebas tanpa pilihan master | `400` "Unit/Tujuan rujukan wajib dipilih dari daftar." | `PASS` |
| R5 | Rujukan ke unit `Rawat Inap` | `200`, `targetServiceUnit = Rawat Inap` | `PASS` |
| R6 | Ubah surat sakit jadi 1–2 Okt | `200`, `durationDays = 2` | `PASS` |
| R7 | Ubah jenis surat sakit → sehat | `400` "Jenis surat tidak dapat diubah…" | `PASS` |
| R8 | `active-by-queue/{G002}` | `200`, surat sakit | `PASS` |
| R9 | Riwayat `?patientId=` | `200`, `2` surat, terbaru di atas | `PASS` |
| R10 | Batal tanpa alasan; batal dengan alasan | `400`; `200` | `PASS` |
| R11 | Ubah surat yang sudah dibatalkan | `400` "…sudah dibatalkan tidak dapat diubah." | `PASS` |
| R12 | Riwayat tanpa filter | `400` "Pasien atau kunjungan wajib dipilih…" | `PASS` |
| R13 | Surat sehat sesudah perbaikan prefix | `200`, nomor `SKH-000001` | `PASS` |

Dua perbaikan selama uji: (1) seluruh tanggal kalender kini disimpan sebagai tengah malam UTC;
(2) prefix nomor `SKS-` menghasilkan `SKS--000003` karena alokator sudah menambah pemisah — diganti `SKS`.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Migration | `20261001040508_AddCliDoctorCertificate` diterapkan |
| `CliDoctorCertificate` | `3` surat uji pasien antrean `G002`: surat sakit **dibatalkan** (`TEST-REVBE004 salah input`), surat rujukan, surat sehat `SKH-000001` |
| Deret nomor | `CLI_DOCTOR_CERT_SICK` dan `_REFERRAL` sudah di nomor `3` (dua nomor terpakai oleh percobaan `500` sebelum perbaikan); nomor berikutnya `SKS-000004`/`SRJ-000004`. Deret unik dan monoton, tidak dijanjikan tanpa lubang |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Entity + migration surat dokter | Terpenuhi | 3.2, 5 |
| 2. `POST`, `PUT /{id}`, `GET active-by-queue/{queueId}`, dan riwayat per pasien | Terpenuhi | R2, R6, R8, R9 |
| 3. Dokter yang tercatat adalah Dokter Penanggung Jawab kunjungan | Terpenuhi | R2 — `doctorId` palsu diabaikan |
| 4. Tujuan rujukan disimpan sebagai rujukan master beserta snapshot namanya | Terpenuhi | R1, R4, R5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Hak akses `DoctorCertificate` baru — posisi dokter belum memegangnya sampai admin mencentang di Akses Role |
| Masalah yang diketahui | `GET /filters/metadata` dan `/summary` belum dibuat (tidak dikonsumsi). Kegagalan validasi model (`[Required]`) berbalas `400` tanpa `message` envelope `ApiResponse` — perilaku global yang sudah ada |
| Risiko tersisa | Tanda tangan disimpan sebagai data URL di kolom `text` (maks ±2 MB per surat); bila volume besar, pertimbangkan storage berkas |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` ApplicationDbContext.cs, ApplicationDbContextModelSnapshot.cs, Program.cs (+ berkas `REV-BE-001`); `??` tujuh berkas baru task ini, dua berkas migration, laporan |
| Langkah berikutnya | `RJ-DOC-REV-FE-006` memakai endpoint ini, riwayat, dan pilihan tujuan rujukan |
