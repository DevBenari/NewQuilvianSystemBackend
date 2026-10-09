# Laporan Perubahan Backend — `DEC-FRJ-001-BE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `DEC-FRJ-001-BE` (belum ada task roadmap; ID diturunkan dari keputusan `DEC-FRJ-001`) |
| Judul | Penyempurnaan Master Fasilitas Perujuk mitra + validasi kelayakan pada Kiosk dan Pendaftaran Rawat Jalan |
| Slice | Master Fasilitas Perujuk, Kiosk, Pendaftaran Rawat Jalan |
| Roadmap | Tidak ada. Penandaan status roadmap tidak dilakukan karena task ini tidak tercantum di roadmap mana pun |
| Trace | `DEC-FRJ-001`, `RJ-DOC-DEC-073/076/080`, `RJ-VAL-PM-04`, `RJ-VAL-PM-16`, `RJ-VAL-PM-17` (baru) |
| Contract version | Delta terhadap `RJ-DOC-REFERRAL-001@1.0.0` (lihat bagian 4) — perintah langsung pemilik, 2026-10-09 |
| Dependency | `RJ-DOC-REV-BE-017`, `RJ-DOC-REV-BE-018` (selesai) |
| Klasifikasi | `HEAVY` — entity baru, migration, aturan lintas modul (Master Data → Registrasi) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`, branch `sukmagp` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `128b688b` |
| Tanggal | 2026-10-09 |
| Status | Selesai; build, migration, QBE, dan uji runtime HTTP dijalankan (lihat bagian 5) |

## 1. Masalah yang diperbaiki

Master Institusi Perujuk sebelumnya hanya punya tanda `IsPartner` dan `IsActive`. Tidak ada data perjanjian, sehingga fasilitas yang kontraknya sudah berakhir tetap bisa dipilih di Kiosk dan Pendaftaran Rawat Jalan. Filter tanggal di halaman master juga tidak dikirim ke backend.

Keputusan `DEC-FRJ-001` menetapkan bahwa master ini hanya untuk mitra resmi RS MMC. Pada transaksi baru, fasilitas boleh dipilih hanya bila aktif dan punya perjanjian yang berlaku pada tanggal layanan.

## 2. Proses bisnis

1. Petugas master membuat fasilitas mitra beserta perjanjian pertamanya. Master yang dibuat selalu mitra.
2. Perpanjangan dicatat sebagai periode baru (`POST /{id}/agreements`). Periode tidak boleh tumpang tindih. Contoh: PKS 2026-01-01 s.d. 2026-06-30 lalu perpanjangan 2026-07-01 s.d. 2026-12-31 → sah; perpanjangan 2026-06-15 s.d. 2026-12-31 → ditolak.
3. Status kerja sama dihitung, tidak disimpan. Urutan penentuannya: Nonmitra (data lama) → Nonaktif → Aktif (ada PKS berlaku hari ini) → Belum Berlaku → Kontrak Berakhir → Tanpa Perjanjian.
4. Kiosk dan Rawat Jalan hanya menerima fasilitas yang layak pada tanggal kunjungan. Pemeriksaan diulang saat submit; bila sudah tidak layak, backend menolak dengan `RJ-VAL-PM-17` dan layar meminta pemilihan ulang.
5. Koreksi rujukan boleh mempertahankan fasilitas yang sudah tercatat walau kemudian nonaktif atau kontraknya berakhir. Mengganti ke fasilitas lain wajib layak.
6. Kode, nama fasilitas, dan nomor PKS disalin ke rincian rujukan saat dicatat. Mengganti nama master tidak mengubah histori.
7. Data lama tidak dikonversi. Data lama nonmitra tidak bisa diaktifkan atau diperpanjang; ia baru menjadi mitra bila petugas menyuntingnya lewat form lengkap dengan perjanjian.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ReferralInstitutionController`, `ReferralMasterDataService`, `ReferralMasterDataDtos`, `MstReferralInstitution(Configuration)`, `RegEncounterReferral(Configuration)`, `EncounterReferralService`, `EncounterIntakeService`, `PatientEncounterController` (create), `LabPatientRegistrationService`, `KioskDeviceController` (pola periode), `AppDateTimeHelper`, migration terakhir.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstReferralInstitution.cs` | Ruas jenis, provinsi/kota, email, PIC, kode faskes eksternal, keterangan, `RowVersion`, relasi `Agreements` |
| `Areas/HealthServices/MasterData/Models/MstReferralInstitutionAgreement.cs` (baru) | Histori perjanjian (nomor, tanggal mulai/berakhir inklusif) |
| `Areas/HealthServices/MasterData/Enums/ReferralInstitutionType.cs`, `ReferralPartnershipStatus.cs` (baru) | Jenis fasilitas; status kerja sama terhitung |
| `Areas/HealthServices/MasterData/Services/ReferralPartnerEligibility.cs` (baru) | Satu definisi kelayakan, penyaring status, label |
| `Areas/HealthServices/MasterData/Services/ReferralMasterDataService.cs` | List berfilter periode/jenis/status, summary 4 indikator, detail + histori + audit, validasi server, perpanjangan, konkurensi, feed mitra layak |
| `Areas/HealthServices/MasterData/DTOs/ReferralMasterDataDtos.cs` | DTO list query, partner option query, response, request, metadata |
| `Areas/HealthServices/MasterData/Controllers/ReferralInstitutionController.cs` | `GET partner-options`, `POST {id}/agreements`, `kiosk/options` hanya mitra layak, pemetaan 409 konkurensi |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterReferralService.cs` | Validasi kelayakan pada tanggal kunjungan (`RJ-VAL-PM-17`), snapshot fasilitas |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Mengirim tanggal kunjungan ke validasi rujukan |
| `Areas/HealthServices/RegistrationManagement/Models/RegEncounterReferral.cs`, `DTOS/EncounterReferralDtos.cs` | Kolom dan ruas snapshot |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterIntakeService.cs`, `DTOS/EncounterIntakeDtos.cs` | Penanda opsional `RequirePartnerEligibility` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabPatientRegistrationDtos.cs`, `Services/LabPatientRegistrationService.cs` | Meneruskan penanda di atas (bawaan `false`; layar Laboratorium tidak berubah) |
| `Repositories/ApplicationDbContext.cs`, konfigurasi EF terkait | DbSet, index, FK, check constraint |
| `Migrations/20261009080537_AddReferralPartnerAgreementAndSnapshot.*` | Migration aditif |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lihat bagian 4. `GET /options` (Laboratorium) tidak berubah |
| Database | Migration `20261009080537_AddReferralPartnerAgreementAndSnapshot`: hanya menambah kolom, tabel, index, FK. Diterapkan ke `QuilvianNewDevSukma` pada 2026-10-09 atas izin pemilik. Tidak ada data lama yang diubah |
| Keamanan/Auth | Atribut Access tetap; endpoint baru memakai `ReferralInstitution : Read/Update`. Policy `KioskRead` dipertahankan. Validasi karakter `<`/`>`, panjang, format, enum, wilayah. Pesan error tanpa detail exception |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Referral Institution

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Tambah `CustomPeriods`, `InstitutionTypeOptions`, `PartnershipStatusOptions`; bawaan `last30days`, `createDateTime desc` | `ReferralInstitution : Read` |
| `GET` | `/summary` | Tambah `TotalPartner`, `ActivePartner`, `InactivePartner`, `ExpiredContractPartner` | `ReferralInstitution : Read` |
| `GET` | `/` | Tambah `startDate`, `endDate`, `customPeriod`, `institutionType`, `partnershipStatus`; search hanya kode/nama | `ReferralInstitution : Read` |
| `GET` | `/partner-options` (baru) | Pilihan mitra layak pada `serviceDate` (bawaan hari ini) | `ReferralInstitution : Read` |
| `GET` | `/kiosk/options` | Kini hanya mitra layak hari ini | Policy `KioskRead` |
| `GET` | `/{id}` | Tambah wilayah, kontak, status kerja sama, histori `Agreements`, audit | `ReferralInstitution : Read` |
| `POST` | `/` | Isian lengkap + perjanjian pertama; `IsPartner` dipaksa `true` | `ReferralInstitution : Create` |
| `PUT` | `/{id}` | Full update; ruas perjanjian mengoreksi perjanjian terakhir; `expectedRowVersion` → 409 bila basi | `ReferralInstitution : Update` |
| `POST` | `/{id}/agreements` (baru) | Perpanjangan; 400 tumpang tindih/nomor ganda, 409 konkurensi | `ReferralInstitution : Update` |
| `PATCH` | `/{id}/status` | Data lama nonmitra tidak dapat diaktifkan (400) | `ReferralInstitution : Update` |

#### Registrasi

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `/patient-encounters`, `/kiosk`, `/admin` | Fasilitas rujukan wajib layak pada tanggal kunjungan → `400 RJ-VAL-PM-17` |
| `GET`/`PUT` | `/patient-encounters/{id}/referral` | Response menambah `InstitutionCodeSnapshot`, `InstitutionNameSnapshot`, `AgreementNumberSnapshot`; ganti fasilitas wajib layak |
| `POST` | `/lab-patient-registrations/external-referral` | Ruas opsional `requirePartnerEligibility` (dikirim RJ); tidak layak → `422 RJ-VAL-PM-17` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release -p:UseSharedCompilation=false -o <scratchpad>` | 0 error | `PASS` | Log build |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `dotnet ef database update` ke `QuilvianNewDevSukma` | `Applying migration '20261009080537_…' Done.` | `PASS` | Atas izin pemilik |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 23 berkas, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran perintah |
| Runtime HTTP `https://localhost:7185` | 45/45 | `PASS` | Tabel di bawah |
| `dotnet test` | Tidak ada project test (pola Bank Darah) | `NOT RUN` | — |

| ID | Skenario | Hasil |
| --- | --- | --- |
| M0–M1 | 401 tanpa login; metadata periode/jenis/status | PASS |
| M2–M7 | Nama kosong, kota di luar provinsi, `<script>`, tanggal terbalik, telepon/email salah, jenis kosong → 400 | PASS |
| M8–M11 | Create valid (kode dinormalkan, `IsPartner` dipaksa); kode ganda 409; kontrak kedaluwarsa → Kontrak Berakhir; belum mulai → Belum Berlaku | PASS |
| M12–M18 | Filter status/periode/jenis, paging server, rentang terbalik 400, periode tidak dikenal 400, kolom list lengkap | PASS |
| M19–M20 | Summary sesuai hitungan DB | PASS |
| M21–M23 | Detail + histori + wilayah; PUT valid; PUT `rowVersion` basi 409 | PASS |
| M24–M26 | Perpanjangan tumpang tindih 400; nomor ganda 400; perpanjangan valid → Aktif | PASS |
| M27–M29 | Nonaktifkan; aktifkan/perpanjang data lama nonmitra 400 | PASS |
| M30–M33 | `partner-options` hanya mitra layak; `serviceDate` mendatang; `options` Laboratorium tetap; `kiosk/options` hanya mitra layak | PASS |
| I1–I4 | Submit RJ dan `/kiosk` dengan fasilitas belum berlaku, data lama tanpa PKS, nonaktif → 400 `RJ-VAL-PM-17`, kunjungan tidak terbentuk | PASS |
| I5–I7 | Submit mitra layak 200; snapshot tersimpan; nama master diganti + nonaktif → snapshot lama tetap | PASS |
| I8–I9 | Koreksi mempertahankan fasilitas nonaktif 200; ganti ke tidak layak 400 | PASS |
| I10 | RJ→Laboratorium dengan penanda + fasilitas tidak layak → 422 `RJ-VAL-PM-17`, kunjungan tidak terbentuk | PASS |
| I11 | Data lama `PMTEST-KSS`/`PMTEST-PKM` tidak berubah | PASS |

Uji jalur Kiosk dengan akun perangkat `kiosk-test` (kredensial dari pemilik lewat env, tidak dicatat): 11/11 `PASS`.

| ID | Skenario | Hasil |
| --- | --- | --- |
| K1–K4 | Login perangkat; `kiosk/options` tepat 8 mitra layak (tanpa kontrak berakhir/nonaktif), membawa `agreementEndDate`, pencarian | PASS |
| K5–K7 | Dokter perujuk per fasilitas 200; list admin 403; `partner-options` (feed petugas) 403 | PASS |
| K8–K9 | Submit `POST /kiosk` fasilitas kontrak berakhir/nonaktif → 400 `RJ-VAL-PM-17`, kunjungan tidak terbentuk | PASS |
| K10–K11 | Submit mitra layak 200; snapshot kode + PKS, sumber Kiosk, rujukan belum lengkap; kunjungan uji dibatalkan | PASS |

Keadaan DB sesudah uji (2026-10-09, atas permintaan pemilik):

- Seluruh data uji master perujuk (`PMTEST-*`, `FRJTEST-*`, `FRJUI-*`) dan dokter perujuknya di-soft delete. `PMTEST-KSS` dan `PMTEST-PKM` masih dirujuk kunjungan uji lama (27 dan 4), sehingga dihapus lewat SQL soft delete; tidak ada hard delete dan histori kunjungan tetap terbaca.
- Diganti 10 data mitra contoh lingkungan RS MMC Kuningan (`MMC-FRJ-001` s.d. `010`) lewat API: 8 Aktif, 1 Kontrak Berakhir (`009`), 1 Nonaktif (`010`). Nomor PKS (`PKS/RSMMC-KNG/xxx/yyyy`), telepon, dan PIC adalah data contoh yang belum terverifikasi.
- Satu kunjungan uji Kiosk (dibatalkan) merujuk `MMC-FRJ-001`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nonmitra tidak dapat dibuat/diaktifkan sebagai master mitra | Terpenuhi | M8, M28, M29 |
| Master nonaktif/tanpa perjanjian valid tidak dapat dipilih pada transaksi baru | Terpenuhi | M30, I1–I4, I10 |
| Validasi kelayakan saat submit | Terpenuhi | I1–I4, I9, I10 |
| Histori dan data lama tetap utuh; snapshot transaksi aman | Terpenuhi | I6–I8, I11 |
| Kode unik dengan constraint DB; validasi tanggal dan overlap | Terpenuhi | M9, M5, M24 |
| Summary, filter tanggal, search, page size, pagination dari backend | Terpenuhi | M12–M20 |
| Concurrency | Terpenuhi | M23 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Data mitra `MMC-FRJ-*` adalah contoh development; nomor PKS, telepon, dan PIC wajib diganti data resmi bagian Kerja Sama sebelum dipakai di luar dev |
| Server dev 7184 | Masih biner lama; perlu dijalankan ulang agar endpoint baru tersedia |
| Roadmap | Tidak diperbarui: task ini tidak tercantum di roadmap |
| Laporan frontend | `task/report/frontend/DEC-FRJ-001-FE.md` |
