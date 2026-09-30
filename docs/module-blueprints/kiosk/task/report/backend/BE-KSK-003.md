# Laporan Perubahan Backend — `BE-KSK-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-003` |
| Judul | Route kiosk menerima Penjamin Perusahaan |
| Slice | EPIC KSK-04 — Penjamin Utama (backend), gelombang `MVP-3` |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/backend-roadmap.md` — kartu `BE-KSK-003` |
| Trace | `FR-KSK-030/031`; `KSK-DEC-013`; `KSK-GUA-002`; `RWI-ENC-PAYER-001` `1.1.0` (§7, §11); `contracts/api-contract.md` §Patient Encounter |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30); `RWI-ENC-PAYER-001` `1.1.0` (Muhammad Hamzah, 2026-09-30) |
| Dependency | `KSK-OQ-005` ✅ — ditutup 30 Sep 2026 (`encounter-company-guarantor-contract.md` §7 dan §11 memuat `1.1.0`) |
| Klasifikasi | `LIGHT` — 2 berkas source; tanpa entity, migration, atau endpoint baru |
| Task mode | `BACKEND` — dinyatakan pengguna 2026-09-30: "lanjut kerjakan {KSK-OQ-005 ✅} ─> BE-KSK-003 module kiosk" |
| Target tulis | `PatientEncounterController.cs`, `PatientEncounterDtos.cs` (komentar XML), laporan ini, sel status `api-contract.md`, baris status roadmap/traceability/`MODULE-STATUS.md` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `051a658a` (branch `sukmagp`). Argumen route kiosk sudah masuk commit itu; perbaikan defect dan komentar DTO belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 5 dari 5 acceptance criteria terbukti |

---

## 1. Masalah yang diperbaiki

Pasien karyawan perusahaan rekanan belum dapat mendaftar lewat Kiosk memakai Penjamin Perusahaan. Route kiosk menolak `paymentType = 3` dengan pesan "Penjamin Perusahaan hanya tersedia pada registrasi petugas", sehingga pasien itu harus antre di loket. Contohnya, karyawan PT rekanan yang datang ke poliklinik harus pindah ke petugas admisi hanya untuk memilih penjaminnya.

Saat task diuji, muncul defect lama yang lebih besar. Penjamin Perusahaan gagal tersimpan (`500`) di **kedua** route, termasuk `/admin`, setiap kali relasi pasien–perusahaan memiliki tanggal berlaku. Di tabel `RegPatientEncounterGuarantor` belum pernah ada satu pun baris `PaymentType = 3`. Defect yang sama mengintai Asuransi yang tanggal berlakunya terisi.

---

## 2. Proses bisnis

| Hal | Isi |
| --- | --- |
| Pelaku | Pasien lewat akun perangkat Kiosk (policy `KioskRead`); petugas admisi lewat `/admin` (`PatientEncounter : Create`) |
| Aturan | Route kiosk menerima Tunai, Asuransi, dan Penjamin Perusahaan. Validasinya **sama persis** dengan `/admin` (`KSK-DEC-013`, `RWI-ENC-PAYER-001` `1.1.0`) |
| Jalur normal | `paymentType = 3` + `patientCompanyGuarantorId` milik pasien yang aktif, eligible, dan berlaku pada tanggal kunjungan → kunjungan dan sumber pembayarannya tersimpan, snapshot data perusahaan ikut tersimpan |
| Jalur tidak normal | Relasi kedaluwarsa, belum berlaku, tidak eligible, tidak aktif, bukan milik pasien, atau id kosong → `400` dengan pesan existing; tidak ada baris yang tersimpan |
| Batas | Tidak ada tipe pembayaran, field, atau hak akses baru. Validasi perusahaan tidak berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; suite `rules/backend/` (termasuk `TEST_POLICY.md`, `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`); kartu task; `rawat-inap/episode-rawat-inap/contracts/encounter-company-guarantor-contract.md` §7/§11; `PatientEncounterController.cs` (route admin/kiosk, `ValidateCreateRequestAsync`, `LoadValidPatientCompanyGuarantorAsync`, pembentukan sumber pembayaran, `ToUtcDate`); `PatientEncounterDtos.cs`; `PatientEncounterNumberService.cs`; blok policy `KioskRead` di `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | **(a)** `CreateEncounterForKiosk` memanggil `CreateEncounterCoreAsync(..., allowCompanyGuarantor: true, ...)` beserta komentar `KSK-DEC-013` / `RWI-ENC-PAYER-001` `1.1.0`. Bagian ini sudah ada di commit `051a658a` saat task dilanjutkan, lalu diverifikasi apa adanya. **(b)** Perbaikan defect atas persetujuan pengguna di sesi ini: snapshot `EffectiveStartDateSnapshot`/`EffectiveEndDateSnapshot` untuk Penjamin Perusahaan **dan** Asuransi kini melewati `ToUtcDate`. Ditambah overload `ToUtcDate(DateTime?)` di samping helper existing |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | Komentar XML `PatientCompanyGuarantorId` (tampil di Swagger) tidak lagi berbunyi "route kiosk tetap Tunai/Asuransi" |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /kiosk` (dan alias `/`) menerima `paymentType = 3`, sesuai `KSK-CONTRACT-v1` dan `RWI-ENC-PAYER-001` `1.1.0`. Sel status di `api-contract.md` diubah menjadi "Tersedia". **Delta dari kartu task:** kartu menyebut cakupan "satu argumen + komentar", tetapi AC 1 tidak dapat terpenuhi tanpa perbaikan snapshot tanggal. Perbaikan itu juga mengubah perilaku `/admin` dari `500` menjadi `200` untuk relasi bertanggal, yang memang dijanjikan kontrak `1.0.0`. Bentuk request/response tidak berubah |
| Database | `NOT APPLICABLE` untuk schema: tanpa entity, configuration, atau migration. `has-pending-model-changes` bersih |
| Keamanan/Auth | Wewenang route kiosk melebar ke Penjamin Perusahaan sesuai keputusan pemilik kontrak. Pagar tetap validasi existing: relasi harus milik `PatientId` yang sama, aktif, eligible, dan dalam masa berlaku. Metadata akses tidak berubah; `PatientEncounter.CreateEncounterForKiosk` tetap di himpunan fallback |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Prefix registry | `Reg` — `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — method existing di controller lama; tidak ada kode, entity, atau prefix baru |
| QBE yang berlaku | `QBE-API-001` (response `ApiResponse`, status existing), `QBE-VAL-001` (validasi tetap di backend, tidak dilonggarkan), `QBE-PERM-001` (metadata Access existing tidak disentuh), `QBE-TXN-001` (transaksi existing; skenario `400` tidak menyimpan apa pun) |
| Tidak diperbaiki (temuan legacy) | Controller memakai `ApplicationDbContext` langsung (`QBE-SVC-001` berlaku untuk `NEW CODE`; bukan cakupan task). Policy `KioskRead` memakai `IsInRole` hardcode di `Program.cs`; policy ini adalah desain yang disetujui (`KSK-DSN-006`) dan dicatat sebagai temuan, bukan diubah |
| Sisa `agents/rules/` di repo target | Tidak ada |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/kiosk` (dan alias `/`) | Membuat kunjungan dari Kiosk dengan satu sumber pembayaran: Tunai, Asuransi, atau Penjamin Perusahaan | Policy `KioskRead`; `[AccessAction("Create", ...)]` existing (fallback kompatibilitas) |
| `POST` | `/admin` | Jalur petugas; kontrak tidak berubah, defect simpan snapshot tanggal diperbaiki | `PatientEncounter : Create` |

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -nodeReuse:false -p:UseSharedCompilation=false` (setelah perbaikan) | Exit `0`, 0 error. Satu-satunya warning di berkas task adalah `CS8619` di `PatientEncounterController.cs:2372`, yang sudah ada sebelum task (baris 2370 pada build pertama) | `PASS` | Keluaran build |
| `ef.dll migrations has-pending-model-changes` terhadap assembly Release | "No changes have been made to the model since the last migration." (dijalankan sebelum dan sesudah perbaikan) | `PASS` | Keluaran perintah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` untuk controller dan DTO | 2 × `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` | Keluaran QBE |
| `bash tools/authorization-verifier/verify-authorization.sh --configuration Release` | Exit `0`; `AUTHORIZATION VERIFIER: PASS`; metadata gap 0; fallback 69 cocok persis; naked baru 0; `approved-compatibility-fallback.txt` tidak berubah dan tetap memuat `PatientEncounter.CreateEncounterForKiosk` | `PASS` | Keluaran verifier |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test |

### Uji runtime

Lingkungan: app dari `bin/Release/net9.0`, `ASPNETCORE_ENVIRONMENT=Development`, `Runtime__Role=Web`, port lokal 5199, DB `QuilvianNewDevSukma`. Pengguna mengizinkan penulisan data uji dan pembersihannya pada sesi ini.

Akun: seed SuperAdmin dari `appsettings.Development.json` (kredensial tidak dicetak), token dikirim lewat header `Bearer`. Akun ini lolos policy `KioskRead`. Password akun perangkat `kiosk-sukma` tidak tersedia di sesi ini dan tidak ditebak. Perilaku yang diuji adalah flag per route (`allowCompanyGuarantor`), bukan jenis akun. Perbedaan akun Kiosk vs non-Kiosk untuk policy ini sudah dibuktikan di `BE-KSK-001` R13.

Data samaran (prefix GUID `b5b5b5b5-…`): pasien `KSKTEST-RM-07` (P7) dengan relasi perusahaan **c1** (berlaku 29 Sep 2026 – 30 Sep 2027), **c2** (berakhir 29 Sep 2026 → kedaluwarsa pada 30 Sep 2026), dan asuransi **a7** (tanpa tanggal). Keempatnya sudah ada di DB, dibuat oleh sesi sebelumnya pukul 10:31. Sesi ini menambah **c3** (salinan c1, `IsEligible = false`) dan **a8** (asuransi non-primer, berlaku 1 Jan 2026 – 31 Des 2027). Service unit `fddbe4ae-…` tidak mewajibkan antrean.

**Run pertama (sebelum perbaikan defect).** Skenario Penjamin Perusahaan yang sah (kiosk, alias `/`, dan `/admin`) seluruhnya `500` "Database gagal menyimpan transaksi kunjungan pasien". Log server: `DbUpdateException → ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`. Sumbernya adalah `EffectiveStartDate/EndDate` relasi (kolom `date`) yang disalin mentah ke `EffectiveStartDateSnapshot/EndDateSnapshot` (`timestamptz`). Skenario `400`, Tunai, dan Asuransi tanpa tanggal sudah sesuai harapan. Transaksi di-rollback, sehingga tidak ada baris separuh. Pengguna memilih untuk memperbaiki snapshot Penjamin Perusahaan dan Asuransi di task ini.

**Run kedua (sesudah perbaikan dan build ulang)** — hasil yang dipakai sebagai bukti:

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| R0 | Keadaan DB sebelum run kedua: `RegPatientEncounter` 177, `RegPatientEncounterGuarantor` 177, `TrxQueue` 173 | Dicatat | `PASS` |
| R1 | `POST /kiosk`, `paymentType 3`, c1 | `200`, `ENC-RSMMC-00178`; DB: `RegPatientEncounter.PaymentType = 3`, `IsFromKiosk = true`, `RegistrationSource = 2`; `RegPatientEncounterGuarantor.PatientCompanyGuarantorId = c1`, snapshot 29/09/2026 – 30/09/2027, `IsEligible = true` | `PASS` |
| R1b | `POST /` (alias), `paymentType 3`, c1 | `200`, `ENC-RSMMC-00179`; baris DB setara R1 | `PASS` |
| R2a | `POST /kiosk`, c2 kedaluwarsa | `400` "Penjamin perusahaan sudah kedaluwarsa pada tanggal kunjungan." | `PASS` |
| R2b | `POST /kiosk`, c3 tidak eligible | `400` "Penjamin perusahaan pasien tidak eligible." | `PASS` |
| R3 | `POST /kiosk`, `paymentType 3` tanpa `patientCompanyGuarantorId` | `400` "PatientCompanyGuarantorId wajib diisi untuk pembayaran Penjamin Perusahaan." | `PASS` |
| R4a | `POST /kiosk` Tunai | `200`, `ENC-RSMMC-00180`, `PaymentType 1` | `PASS` |
| R4b | `POST /kiosk` Asuransi a7 (tanpa tanggal) | `200`, `ENC-RSMMC-00181`, `PaymentType 2` | `PASS` |
| R4e | `POST /kiosk` Asuransi a8 (bertanggal) — bukti perbaikan Asuransi | `200`, `ENC-RSMMC-00182`; snapshot 01/01/2026 – 31/12/2027 | `PASS` |
| R4c | `POST /admin` Penjamin Perusahaan c1 | `200`, `ENC-RSMMC-00183`, `IsFromKiosk = false`, `RegistrationSource = 1` | `PASS` |
| R4d | `POST /admin` c2 kedaluwarsa | `400`, pesan sama dengan R2a — validasi admin tidak berubah | `PASS` |
| R5 | `POST /kiosk` tanpa token | `401` | `PASS` |
| R6 | Jumlah baris sesudah run kedua | `RegPatientEncounter` 183, `RegPatientEncounterGuarantor` 183 (= 177 + 6 skenario `200`); `TrxQueue` 173. Lima skenario `400`/`401` tidak menyimpan baris | `PASS` |
| R-akhir | Pembersihan (pilihan pengguna: hapus kunjungan uji saja) | Dihapus: 10 kunjungan P7 (`00174`–`00183`, termasuk `00174`/`00175` sisa sesi 10:32) beserta 10 baris guarantor, 0 antrean, relasi c3, dan asuransi a8. Satu-satunya FK ke kunjungan uji adalah `RegPatientEncounterGuarantor`. Sesudahnya: `MstPatient` 17, `RegPatientEncounter` 173, `RegPatientEncounterGuarantor` 173, `TrxQueue` 173, `MstPatientCompanyGuarantor` 3, nomor terakhir `ENC-RSMMC-00173`. P7, c1, c2, a7 **dibiarkan** untuk `FE-KSK-008` | `PASS` |

Catatan data: `RegPatientEncounter` memuat dua kunjungan lama milik pasien nyata dengan `PaymentType = 3` (`ENC-RSMMC-00053`, `00055`), tetapi baris guarantor-nya tidak bertipe `3`. Data ini tidak disentuh.

Efek tulis selama app berjalan: seeder web idempoten dan worker yang selalu aktif (sama seperti `BE-KSK-001/002`), serta log info aplikasi per kunjungan yang terbentuk. Log aplikasi tidak dihapus. Salinan `appsettings.Development.json` di `bin/Release/net9.0` (gitignored) dihapus setelah run.

Uji manual: `NOT APPLICABLE` — layar Penjamin Utama dikerjakan di `FE-KSK-008`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Kiosk `paymentType 3` + relasi aktif/eligible/berlaku → `200`; `RegPatientEncounter.PaymentType = 3`, `RegPatientEncounterGuarantor.PatientCompanyGuarantorId` terisi | Terpenuhi (setelah perbaikan defect) | R1, R1b, R6 |
| 2. Relasi kedaluwarsa atau tidak eligible → `400` pesan existing; tidak ada baris tersimpan | Terpenuhi | R2a, R2b, R6 |
| 3. `paymentType 3` tanpa `patientCompanyGuarantorId` → `400` "PatientCompanyGuarantorId wajib diisi…" | Terpenuhi | R3 |
| 4. Regresi: Tunai dan Asuransi lewat kiosk tetap `200`; route `/admin` tidak berubah | Terpenuhi, dengan catatan: kontrak dan validasi `/admin` tidak berubah (R4d), tetapi perilakunya untuk relasi bertanggal berubah dari `500` menjadi `200` karena perbaikan defect yang disetujui | R4a, R4b, R4e, R4c, R4d |
| 5. Verifier exit `0`; `PatientEncounter.CreateEncounterForKiosk` tetap di himpunan fallback | Terpenuhi | §5 verifier |
| DoD: AC 1–5 terbukti di laporan ini | Terpenuhi | §5–§6 |
| DoD: `KSK-OQ-005` tercatat tertutup | Terpenuhi — `00-interview-decisions.md`, `MODULE-STATUS.md` | §Metadata |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `CS8619` di `PatientEncounterController.cs:2372` sudah ada sebelumnya |
| Masalah yang diketahui | Pola yang sama (tanggal kolom `date` dipakai tanpa `Kind`) masih ada di modul Billing: `BillingPayerEditService.cs:1338-1339` dan `BillingCompanyGuarantorInvoiceDocumentService.cs:238-241`. Belum diverifikasi apakah nilainya ditulis ke kolom `timestamptz`. Ini di luar wewenang task; perlu dicek pemilik Billing |
| Risiko tersisa | Wewenang Kiosk melebar ke Penjamin Perusahaan. Pagarnya hanya validasi relasi existing, dan pemilik kontrak sudah menyetujuinya (`KSK-DEC-013`). Uji runtime memakai SuperAdmin, bukan akun perangkat Kiosk (lihat §5) |
| Perubahan sampingan | `NONE` di source. Artefak `bin/Release` (gitignored) untuk verifier |
| Interupsi | Sesi sebelumnya (30 Sep 2026 ±10:31) sudah membuat data samaran dan kunjungan `00174`/`00175` tanpa meninggalkan laporan. Sesi ini melanjutkan dari keadaan Git/DB yang terverifikasi, lalu mengulang seluruh skenario |
| Status Git | ` M Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`; ` M Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs`; perubahan dokumen di `docs/module-blueprints/kiosk/`. Belum di-stage atau di-commit |
| Langkah berikutnya | `FE-KSK-008` kini hanya menunggu `FE-KSK-007` (⛔ `KSK-OQ-004`). Owner Billing memeriksa dua lokasi pada "Masalah yang diketahui" |
