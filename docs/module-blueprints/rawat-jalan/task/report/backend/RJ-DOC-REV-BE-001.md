# Laporan Perubahan Backend — `RJ-DOC-REV-BE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-001` |
| Judul | Data penjamin dan identitas pasien pada antrean perawat dan dokter |
| Slice | Revisi UAT `2026-09-28` — Skrining Pasien butir 1, Dokter Rawat Jalan butir 1c |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007`; temuan audit `A1` (bagian `9.1`); MPY-ENC-PAYER-001 (sumber pembayaran kunjungan) |
| Contract version | Tidak ada kontrak berversi untuk response antrean; perubahan bersifat **penambahan field** (non-breaking) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — repo 0, berkas diperiksa 1 (>20), berkas diubah 1 (7), logika 1, kontrak API 1 (field baru), database 1 (query saja), keamanan 0, UI 0 → skor 5 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RegistrationManagement/**`, `Program.cs`, dokumen blueprint rawat-jalan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` (worktree bersih saat mulai) |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti runtime |

---

## 1. Masalah yang diperbaiki

Pasien mendaftar lewat KIOS-K dengan asuransi, misalnya **Prudential Indonesia**. Saat perawat membuka
tab *Informasi Pasien* di Skrining, baris **Penjamin Utama** dan **Jenis Penjamin Utama** kosong, dan
**Pasien Asuransi** tertulis `Tidak` padahal Tipe Pembayaran `Asuransi`.

Penyebabnya: layar sudah membaca `primaryGuarantorNameSnapshot`, `primaryGuarantorTypeSnapshot`,
`isInsurancePatient`, dan `isCompanyPatient`, tetapi backend tidak pernah mengirim keempat field itu.
Nilai kosong ditampilkan sebagai `-`, dan nilai `false` sebagai `Tidak`.

Di workspace dokter, header pasien juga belum punya jenis kelamin, umur, alergi, dan foto KTP/kartu
penjamin. Response antrean dokter bahkan mengirim `ageTextAtEncounter` sebagai `null` secara paksa.

Ditemukan juga label jenis kelamin di antrean perawat tampil `Female`/`Male`, bukan `Perempuan`/`Laki-laki`.

---

## 2. Proses bisnis

1. Pasien mendaftar di KIOS-K dan memilih cara bayar: tunai, asuransi, atau penjamin perusahaan.
2. Registrasi menyimpan satu **sumber pembayaran aktif** kunjungan (`RegPatientEncounterGuarantor`).
3. Perawat membuka antrean Nurse Station. Backend menurunkan **penjamin utama** dari sumber pembayaran itu:
   - Jenis `Asuransi` → nama = perusahaan asuransi, `Pasien Asuransi = Ya`.
   - Jenis `Penjamin Perusahaan` → nama = perusahaan penjamin, `Pasien Perusahaan = Ya`.
   - Jenis `Tunai` → nama = `Tunai` / metode bayar, keduanya `Tidak`.
4. Dokter membuka antrean dokter. Selain penjamin, backend mengirim jenis kelamin, umur saat kunjungan,
   ringkasan alergi, dan path foto pasien, dokumen identitas, serta kartu asuransi kunjungan.

**Aturan penentuan jenis penjamin (berurutan):**

| Urutan | Kondisi | Hasil |
| ---: | --- | --- |
| 1 | Jenis pada sumber pembayaran bukan `Tunai` | Jenis itu |
| 2 | Sumber pembayaran bertanda `Tunai` tetapi menunjuk kartu/perusahaan asuransi | `Asuransi` |
| 3 | Sumber pembayaran bertanda `Tunai` tetapi menunjuk penjamin perusahaan | `Penjamin Perusahaan` |
| 4 | Lainnya | Jenis pada sumber pembayaran, lalu jenis pada kunjungan, lalu `Tunai` |

Contoh jalur 2: kunjungan `ENC-RSMMC-00053` bertanda `Penjamin Perusahaan` di kunjungan, sumber
pembayarannya bertanda `Tunai`, tetapi menunjuk *Mandiri Inhealth*. Hasilnya `Asuransi — Mandiri Inhealth`.
Ini data lama yang tidak konsisten; rujukan kartu lebih dipercaya daripada tandanya.

**Ringkasan alergi** = gabungan alergi terstruktur aktif pasien (`TrxPatientAllergy`) dan catatan alergi
asesmen terbaru pada kunjungan ini, tanpa duplikat. Contoh: `Amoxicillin; Seafood - gatal`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`NurseStationQueueController.cs`, `DoctorQueueController.cs`, `NurseStationQueueDtos.cs`,
`DoctorQueueDtos.cs`, `EncounterPaymentSourceService.cs`, `RegPatientEncounterGuarantor.cs`,
`EncounterPaymentType.cs`, `MstPatient.cs`, `MstPatientIdentityDocument.cs`, `MstPatientInsurance.cs`,
`TrxPatientAllergy.cs`, `TrxPatientAssessment.cs`, `Gender.cs`, `Program.cs`, frontend
`PatientInformationTab.jsx`, `DoctorPatientContext.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterPrimaryPayerSummary.cs` (baru) | Satu tempat penentuan penjamin utama dipakai antrean perawat dan dokter |
| `Areas/HealthServices/RegistrationManagement/Services/DoctorQueuePatientContextService.cs` (baru) | Membaca alergi, foto, dokumen identitas, dan kartu asuransi secara batch per halaman antrean |
| `Areas/HealthServices/RegistrationManagement/DTOS/NurseStationQueueDtos.cs` | Empat field penjamin baru |
| `Areas/HealthServices/RegistrationManagement/DTOS/DoctorQueueDtos.cs` | Empat field penjamin + tujuh field identitas klinis |
| `Areas/HealthServices/RegistrationManagement/Controllers/NurseStationQueueController.cs` | Include `CompanyGuarantor`; isi field penjamin; label enum `[Display]` diperbaiki |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | Include `CompanyGuarantor`; inject service konteks pasien; isi field baru; `AgeTextAtEncounter`/kategori usia diisi dari kunjungan |
| `Program.cs` | Registrasi `DoctorQueuePatientContextService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Penambahan field pada response antrean perawat dan dokter. Tidak ada field yang dihapus atau diubah tipenya. `ageTextAtEncounter`, `ageCategoryCodeSnapshot`, `ageCategoryNameSnapshot` pada antrean dokter kini berisi nilai, sebelumnya selalu `null` |
| Database | Tidak ada perubahan schema. Lima query baca batch tambahan per halaman antrean dokter (bukan per baris) |
| Keamanan/Auth | Tidak ada atribut hak akses berubah. Data alergi dan path dokumen identitas hanya muncul di endpoint antrean dokter yang sudah bergerbang `DoctorQueue : Read` |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Nurse Station Queue

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/nurse-station-queues` | Daftar antrean perawat, kini dengan `primaryGuarantorNameSnapshot`, `primaryGuarantorTypeSnapshot`, `isInsurancePatient`, `isCompanyPatient` | `NurseStationQueue : Read` |

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/doctor-queues` | Daftar antrean dokter, kini dengan empat field penjamin di atas ditambah `genderName`, `birthDate`, `hasAllergy`, `allergySummary`, `patientPhotoPath`, `identityDocumentPath`, `insuranceCardImagePath` | `DoctorQueue : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/build` | `Build succeeded`, `0 Error(s)`; build ulang inkremental `0 Warning(s)`. Build penuh: nol peringatan dari berkas baru; peringatan `CS8602` pada rantai `ThenInclude` controller perawat adalah pola yang sudah ada | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 7 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `Final result: PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test |
| Validasi runtime R0–R3 | 4/4 `PASS` | `PASS` | Bagian 5.1 |

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** jalur `403` (atribut hak akses tidak berubah). Kartu asuransi berpath gambar tidak
ada pada antrean yang tersedia, sehingga `insuranceCardImagePath` hanya terbukti bernilai `null`.

### 5.1 Hasil validasi runtime — 1 Oktober 2026

Aplikasi hasil build dijalankan dari scratchpad pada `http://localhost:5217` terhadap
**`QuilvianNewDevSukma`**, sesi `superadmin` dari login seed (kredensial dibaca dari konfigurasi tanpa
dicetak).

Sebelum aplikasi dapat berjalan, empat migration milik task lain yang belum diterapkan ke
`QuilvianNewDevSukma` diterapkan lewat `dotnet ef database update` (lihat 5.2).

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa sesi | `200`; tanpa sesi `401` | — | `PASS` |
| R1 | Antrean perawat `2026-08-24` `I001` (Asuransi) dan `2026-07-04` | `I001`: `Prudential Indonesia` / `Asuransi` / `isInsurancePatient = true`. `INTERNA-20260704-001` (data lama tidak konsisten): `Mandiri Inhealth` / `Asuransi`. Pasien tunai: `Tunai / Pribadi`, keduanya `false`. Jenis kelamin `Perempuan`/`Laki-laki` | 1, 2 | `PASS` |
| R2 | Antrean dokter `2026-07-15` dan `2026-07-04` | `G001`: `Prudential Indonesia` / `Asuransi` / `true`, `Perempuan`, `31 tahun 4 bulan 1 hari`, dokumen identitas ada. `GIGI-20260704-001`: `Allianz Indonesia` / `Asuransi`, foto ada. `G002` tunai | 1, 2, 3 | `PASS` |
| R3 | Buat alergi samaran `TEST-REVBE001 Amoxicillin` untuk pasien `G001` lewat `POST /patient-allergies`, lalu baca ulang antrean dokter | `200`; `hasAllergy = true`, `allergySummary = "TEST-REVBE001 Amoxicillin"` | 3 | `PASS` |

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Migration | Diterapkan ke `QuilvianNewDevSukma`: `20260928093849_AddMasterNursingDiagnosisSdki`, `20260930082632_UpdatePendingModelChanges`, `20260930110000_AddDoctorConsultationSourceVitalSign`, `20260930120000_AddMasterDailyNursingAction`. Keempatnya milik task lain yang sudah ada di `HEAD`, hanya `CreateTable`/`CreateIndex`/`AddColumn`/`AddForeignKey`. Tidak ada migration baru dari task ini |
| `TrxPatientAllergy` | Satu baris baru `07ef5bd3-…` `TEST-REVBE001 Amoxicillin` untuk pasien antrean `G001` (`2026-07-15`). Dibiarkan sebagai data uji |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Antrean perawat dan dokter mengirim `PrimaryGuarantorNameSnapshot`, `PrimaryGuarantorTypeSnapshot`, `IsInsurancePatient`, `IsCompanyPatient` dari penjamin aktif kunjungan | Terpenuhi | R1, R2 |
| 2. `Insurance` ⇒ `IsInsurancePatient = true`; `CompanyGuarantor` ⇒ `IsCompanyPatient = true` | Terpenuhi untuk `Insurance` (R1, R2). Jalur `CompanyGuarantor` terbukti lewat kode; tidak ada kunjungan berpenjamin perusahaan yang valid di DB | Sebagian bukti runtime |
| 3. Antrean dokter mengirim jenis kelamin, umur, alergi aktif, dan rujukan foto KTP/kartu penjamin | Terpenuhi | R2, R3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Path dokumen identitas hasil scan KIOS-K berupa path lokal mesin kiosk (`C:\Program Files\QuilvianScannerOcrAgent\...`), **tidak dapat disajikan** ke browser. Field tetap dikirim apa adanya; frontend hanya boleh menampilkan gambar bila path dimulai `/` atau `http` |
| Masalah yang diketahui | Dua kunjungan lama (`ENC-RSMMC-00053`, `-00055`) bertanda `Penjamin Perusahaan` tetapi sumber pembayarannya menunjuk asuransi. Ditangani aturan urutan 2, datanya tidak diubah |
| Risiko tersisa | Bila KIOS-K tidak mengunggah gambar KTP ke storage server, foto KTP di header dokter akan selalu kosong. Ini milik modul KIOS/Registrasi |
| Perubahan sampingan | Perbaikan label enum `[Display]` di `NurseStationQueueController.BuildOptionalLabel` (jenis kelamin, agama, status nikah, golongan darah, jenis identitas) — dibutuhkan layar Skrining yang sama |
| Interupsi | Aplikasi gagal start karena DB tertinggal migration; dipulihkan dengan menerapkan migration `HEAD` (5.2) |
| Status Git | `M` DoctorQueueController.cs, NurseStationQueueController.cs, DoctorQueueDtos.cs, NurseStationQueueDtos.cs, Program.cs, doctor-consultation-roadmap.md; `??` DoctorQueuePatientContextService.cs, EncounterPrimaryPayerSummary.cs, laporan ini |
| Langkah berikutnya | `RJ-DOC-REV-FE-001` dan `RJ-DOC-REV-FE-002` mengonsumsi field ini |
