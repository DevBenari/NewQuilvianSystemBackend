# Laporan Perubahan Backend — `BE-FIN-106`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-106` |
| Judul | Registrasi: pencarian pegawai dan aturan "harus terdaftar" untuk kartu penjamin internal |
| Slice | `REV-19` Slice A — Fondasi HR Benefit dan Registrasi (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` |
| Trace | `FIN-DEC-199`, `201`, `204`, `207`, `209`, `227`, `228`, `229`, `230`; `FR-FIN-238`..`240`; `FIN-INTEGRATION-1.9` `R.2` (`INT-REG-HR-001`); `FIN-VAL-1.12` `FIN-VAL-259`..`261`, `268`, `269`; `FIN-OQ-102` (**CLOSED**) |
| Contract version | `FIN-INTEGRATION-1.9` `R.2`, `FIN-API-1.10` `R.3`, `R.4`, `FIN-PERM-1.11` — **approved** sisi pemilik Finance dan HR (`FIN-DEC-212`, `223`). Keputusan `FIN-DEC-227`..`230` telah diratifikasi melalui Amendment Pass di `00-interview-decisions.md` dan `data-dictionary.md`. Konfirmasi pemilik **Registrasi** atas `FIN-DEC-204` dan `209` belum turun (dicatat sebagai risiko di roadmap) |
| Dependency | `BE-FIN-103`, `BE-FIN-104`, `BE-FIN-105` — build dan migrasi dijalankan pemilik dan dilaporkan berhasil (13 Oktober 2026); `{FIN-OQ-105}` ✅ |
| Klasifikasi | `HEAVY` — skor 12: berkas diperiksa >20 (2), berkas diubah 14 (2), logika bisnis kompleks (2), kontrak API baru dan berubah (2), dampak schema: satu kolom dan satu indeks (2), data pribadi (1), lintas modul (1) |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("lanjutkan BE-FIN-106", 13 Oktober 2026), lintas modul ke Registration, Patient Management, Administrator Master Data, dan HR Benefit Management |
| Target tulis | Lihat 3.2 |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh source code backend selesai diimplementasikan (termasuk validasi kecocokan identitas pasien vs HR `FIN-DEC-228` dan ratifikasi penjamin internal `FIN-DEC-227`); `dotnet build` **NOT RUN** oleh agent (instruksi pemilik); migration `AddEmployeeBenefitGuarantorFlagToCompanyGuarantor` menunggu dieksekusi oleh pemilik |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | HealthServices / RegistrationManagement (controller dan adapter baru); HealthServices / PatientManagement / MasterData (kartu); Administrator / MasterData (penanda penjamin); Corporate / Human Resource / BenefitManagement (pembacaan HR) |
| Registry | `RegistrationManagement — Reg — ACTIVE / LEGACY`; `PatientManagement operational — Pat — ACTIVE`; `Master / Reference — Mst — ACTIVE`; `Human Resource — Hrd — ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE`: `EmployeeBenefitLookupController`, `RegistrationEmployeeBenefitLookupService`, DTO lookup, perluasan `EmployeeBenefitEligibilityService`. `TOUCHED LEGACY`: controller kartu, controller penjamin perusahaan, `PatientEncounterController`, `EncounterPaymentSourceService` |
| Entity operasional baru | **Tidak ada** — hanya satu kolom pada tabel master yang ada, sehingga `QBE-MOD-002`/`003` tidak terpicu |
| QBE yang berlaku | `QBE-SVC-001` (logika baru ada di service; controller baru tidak mengakses context), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-CFG-001`/`002`, `QBE-NAM-001` |

---

## 1. Masalah yang diperbaiki

Petugas Registrasi saat ini mengetik nomor karyawan sebagai teks bebas, dan sistem menerima kartu penjamin apa pun. Akibatnya
tagihan bisa jatuh ke pegawai yang salah: misalnya pasien bernama Pak Andi (paman, tidak terdaftar di HR) diberi kartu
"RS Benefit" atas nama Budi. Task ini membuat kartu penjamin internal hanya terbit untuk orang yang **benar-benar terdaftar
dan terverifikasi** di HR (`FIN-DEC-204`), dan memberi petugas alat untuk mencari orang itu.

Selain itu, sebelumnya identitas pasien tidak dicocokkan dengan data orang di HR. Sistem hanya memastikan bahwa pegawai dan anggota keluarga yang dipilih sah di HR, tetapi tidak memverifikasi bahwa pasien yang sedang didaftarkan kartu adalah orang yang bersangkutan (misalnya NIK pasien sama dengan NIK pegawai/anggota keluarga HR). Celah ini ditutup pada task ini (`FIN-DEC-228`): sistem memvalidasi kecocokan NIK untuk pegawai sendiri (`SELF`), serta NIK/Nama Lengkap + Tanggal Lahir untuk anggota keluarga tanggungan (`FAMILY`). Bila tidak cocok, penerbitan/pembaruan kartu ditolak dengan status HTTP `422 UnprocessableEntity` dan alasan `PATIENT_IDENTITY_MISMATCH`.

## 2. Proses bisnis

**Tujuan.** Kartu penjamin internal hanya untuk pegawai atau anggota keluarganya yang sah menurut HR; jika tidak, pasien didaftarkan sebagai Pasien Umum tunai.

| Langkah | Pelaku | Hasil |
| ---: | --- | --- |
| 1. Petugas mengetik minimal 3 huruf nama, NIP, atau NIK | Petugas Registrasi | Daftar paling banyak 20 pegawai aktif; NIK tersamar |
| 2. Petugas memilih pegawai, lalu melihat daftar keluarganya | Petugas Registrasi | Setiap anggota keluarga tampil dengan status tanggungan (`VERIFIED`, `REQUESTED`, `NOT_REGISTERED`, `REJECTED`, `ENDED`) dan apakah plan menanggung hubungannya |
| 3. Petugas menyimpan kartu penjamin internal dengan pegawai (dan anggota keluarga) terpilih | Petugas Registrasi | Sistem memeriksa HR; memvalidasi kesesuaian identitas pasien vs HR (`FIN-DEC-228`); bila layak dan cocok, kartu terbit dan kode hubungan diisi sistem |
| 4. Kunjungan didaftarkan memakai kartu itu | Petugas Registrasi | Kelayakan diperiksa ulang pada tanggal kunjungan |
| 5. Penanggung kunjungan diganti ke kartu itu dari Billing | Petugas Kasir/Billing | Kelayakan diperiksa ulang pada tanggal pelayanan |

**Aturan dengan contoh.**

| Kasus | Hasil |
| --- | --- |
| Mengetik "198" | Pegawai aktif yang NIP, NIK, atau namanya memuat "198" (maksimum 20) |
| Mengetik "19" | Ditolak `400` "Ketik minimal 3 karakter untuk mencari pegawai." |
| Lebih dari 20 pegawai cocok | 20 pertama dikembalikan; pesan respons: "Hasil terlalu banyak. Ketik nama atau NIP yang lebih spesifik." |
| Sari, anak Budi: terdaftar, tanggungan `Verified`, plan `Family`, dalam masa berlaku | Kartu terbit; hubungan `CHILD` diisi sistem |
| Pasien NIK tidak cocok dengan Pegawai HR (kartu `SELF`) | Ditolak `422` `PATIENT_IDENTITY_MISMATCH` ("NIK pasien tidak sesuai dengan NIK pegawai pada data HR.") (`FIN-DEC-228`) |
| Pasien anggota keluarga NIK/Nama/Tanggal Lahir tidak cocok dengan data tanggungan HR | Ditolak `422` `PATIENT_IDENTITY_MISMATCH` ("Identitas pasien tidak sesuai dengan data anggota keluarga/tanggungan pada HR.") (`FIN-DEC-228`) |
| Pak Andi tidak terdaftar sebagai anggota keluarga Budi | Tidak dapat dipilih (`FAMILY_MEMBER_NOT_FOUND`); kartu tidak terbit; petugas mendaftarkan Pasien Umum |
| Pak Andi terdaftar sebagai anggota keluarga, tetapi belum didaftarkan sebagai tanggungan | `422` "Pasien belum terdaftar sebagai tanggungan pegawai di HR. Daftarkan di HR terlebih dahulu atau daftarkan sebagai Pasien Umum." |
| Tanggungan masih `Requested` | `422` "Pendaftaran tanggungan pasien belum diverifikasi HR." |
| Tanggungan di luar masa berlaku | `422` "Masa berlaku tanggungan pasien tidak mencakup tanggal layanan." |
| Budi tanpa enrollment layanan RS aktif (`FIN-DEC-209`) | `422` "Pegawai belum memiliki benefit layanan RS yang aktif. Daftarkan sebagai Pasien Umum atau hubungi HR." |
| Klien mengirim kode hubungan (`CHILD`) | `400` "Hubungan keluarga ditentukan sistem dari data HR." |
| Pegawai tidak dipilih pada kartu penjamin internal | `422` "Pegawai wajib dipilih dari data HR untuk kartu penjamin internal…" |
| Kartu penjamin **biasa** disertai pegawai HR | `400` "Tautan pegawai HR hanya dapat diisi untuk kartu penjamin internal." |
| HR tidak dapat dibaca | Kartu **tidak** diterbitkan: `503` dengan jalur Pasien Umum |
| Kartu penjamin biasa (perusahaan rekanan) | Tidak berubah sama sekali |

**Jalur tidak normal.** HR gagal dibaca: gagal tertutup (`HR_UNAVAILABLE`). Kartu internal lama tanpa tautan dipakai untuk
kunjungan: ditolak `CARD_NOT_LINKED` agar petugas memperbarui kartu (`FIN-DEC-230`). Menonaktifkan kartu lewat `PUT` tidak memeriksa HR (pegawai
yang sudah keluar tetap dapat dinonaktifkan). Kartu dari kiosk ikut aturan yang sama sehingga kiosk tidak dapat menerbitkan kartu internal.

**Hasil akhir.** Kartu internal selalu membawa `EmployeeId`, `FamilyMemberId` (bila bukan pegawai sendiri), dan kode hubungan dari HR;
kunjungan menyalinnya (`BE-FIN-105`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REPORT_TEMPLATE`, `TEST_POLICY`,
`transaction-endpoint-standard`, `role-access-rules`, `BACKEND_ENGINEERING_CONTRACT`); registry kepemilikan modul; kontrak
`integration-contract.md` `R.1`–`R.2`, `api-contract.md` `R.3`–`R.4`, `validation-matrix.md` `R.3`, `00-interview-decisions.md`
(`FIN-DEC-197`..`199`, `204`, `FIN-OQ-102`, `FIN-DEC-227`..`230`); `MstCompanyGuarantor` (model, konfigurasi, controller, DTO); `MstPatientCompanyGuarantor` dan
controllernya (Create/Update/validasi/pemetaan/log); `PatientEncounterController` (`LoadValidPatientCompanyGuarantorAsync`,
`BuildPaymentSourceAsync`); `EncounterPaymentSourceService`; `BillingPayerEditService` dan `InpAdmissionTransferService` (pemanggil);
`MstEmployee`, `MstDepartment`, `WfpFamilyMember`, `MstPatient`; seluruh berkas `BE-FIN-103`..`105`; `KioskPatientLookupController` (pola controller Registrasi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/EmployeeBenefitLookupController.cs` | **Baru.** Tiga `GET`, hanya-baca (`/employees`, `/employees/{employeeId}/family-members`, `/eligibility`) |
| `Areas/HealthServices/RegistrationManagement/Services/RegistrationEmployeeBenefitLookupService.cs` | **Baru.** Adapter ke HR: `SearchEmployeesAsync`, `GetFamilyMembersAsync`, `CheckEligibilityAsync`, `ResolveCardLinkAsync` (validasi identitas pasien vs data HR `FIN-DEC-228` saat kartu dibuat/diubah), `ValidateCardForServiceAsync` (kunjungan memakai kartu); gagal tertutup |
| `Areas/HealthServices/RegistrationManagement/DTOS/EmployeeBenefitLookupDtos.cs` | **Baru.** `EmployeeLookupResponse`, `FamilyMemberLookupResponse`, `BenefitEligibilityResponse` |
| `Areas/Corporate/HumanResource/BenefitManagement/Services/EmployeeBenefitEligibilityService.cs` | + `SearchEmployeesAsync` (CariPegawai), `ListFamilyMembersAsync` (DaftarKeluarga), penyamaran NIK; `CheckAsync` mengembalikan NIP, nama pegawai, serta `TargetPersonNationalId`, `TargetPersonFullName`, `TargetPersonBirthDate` untuk verifikasi identitas pasien |
| `Areas/Corporate/HumanResource/BenefitManagement/DTOs/EmployeeBenefitEligibilityDtos.cs` | + `EmployeeSearchItem/Result`, `FamilyMemberEligibilityItem`; `BenefitEligibilityResult` + `EmployeeNumber`, `EmployeeName`, `TargetPersonNationalId`, `TargetPersonFullName`, `TargetPersonBirthDate` |
| `Areas/Corporate/HumanResource/BenefitManagement/Constants/HrBenefitConstants.cs` | + `BenefitDependentVerificationCodes`, `EmployeeLookupLimits` |
| `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` | + `IsEmployeeBenefitGuarantor` (`bool`, bawaan `false`) |
| `Repositories/Configurations/Global/MstCompanyGuarantorConfiguration.cs` | Bawaan `false`; indeks unik terfilter `UX_MstCompanyGuarantor_EmployeeBenefit` (paling banyak satu penjamin internal yang belum dihapus) |
| `Areas/Administrator/MasterData/DTOs/CompanyGuarantorDtos.cs` | Request: `IsEmployeeBenefitGuarantor` (`bool?`); respons daftar/rincian dan opsi: `IsEmployeeBenefitGuarantor` |
| `Areas/Administrator/MasterData/Controllers/CompanyGuarantorController.cs` | Menyimpan dan menampilkan penanda penjamin internal (`FIN-DEC-227`) |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientCompanyGuarantorDtos.cs` | Request kartu: `EmployeeId`, `FamilyMemberId`, `BenefitRelationship` (yang terakhir hanya agar dapat ditolak bila dikirim klien) |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientCompanyGuarantorController.cs` | `Create`/`Update` memanggil `ResolveCardLinkAsync` dengan `PatientId` untuk validasi kecocokan identitas (`FIN-DEC-228`); mengisi tautan dan label NIP/nama dari HR |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | `LoadValidPatientCompanyGuarantorAsync` memeriksa kelayakan kartu internal pada tanggal kunjungan |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs` | Ganti penanggung ke kartu internal memeriksa kelayakan pada tanggal pelayanan |
| `Program.cs` | + `AddScoped<RegistrationEmployeeBenefitLookupService>` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint baru (bagian 4); field `isEmployeeBenefitGuarantor` pada master penjamin perusahaan (`FIN-DEC-227`); penolakan `422 PATIENT_IDENTITY_MISMATCH` saat kartu dibuat/diubah jika identitas pasien tidak sesuai data HR (`FIN-DEC-228`) |
| Database | Satu kolom `boolean NOT NULL DEFAULT false` dan satu indeks unik terfilter pada `MstCompanyGuarantor`. **Migration belum dibuat.** Tidak ada data lama yang terdampak: tidak ada baris bernilai `true` |
| Keamanan dan privasi | Endpoint lookup butuh `EmployeeBenefitLookup : Read`. NIK dan identitas keluarga selalu tersamar; tidak ada saldo plafon, gaji, alamat, atau kontak. Log hanya memuat nama aksi dan galat, tanpa kata kunci, NIP, NIK, atau nama |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Employee Benefit Lookup

Base URL: `api/v1/health-services/registration-management/employee-benefit-lookups`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/employees` | Mencari pegawai aktif dengan NIP, NIK, atau nama | `EmployeeBenefitLookup : Read` | `?search=` (minimal 3 karakter) | `ApiResponse<List<EmployeeLookupResponse>>` |
| `GET` | `/employees/{employeeId}/family-members` | Anggota keluarga aktif beserta status tanggungan | `EmployeeBenefitLookup : Read` | — | `ApiResponse<List<FamilyMemberLookupResponse>>` |
| `GET` | `/eligibility` | Memeriksa kelayakan pada tanggal layanan | `EmployeeBenefitLookup : Read` | `?employeeId=&familyMemberId=&serviceDate=` | `ApiResponse<BenefitEligibilityResponse>` |

Kode status: `200` berhasil (termasuk "tidak layak", dijelaskan `reasonCode`); `400` kata kunci kurang dari 3 karakter, `employeeId`
kosong, atau `serviceDate` tidak valid; `403` tanpa hak akses; `404` pegawai tidak ditemukan (daftar keluarga); `503` data HR tidak dapat dibaca.

#### Health Services / Patient Management / Master Data / Patient Company Guarantor (diperbarui)

Base URL: `api/v1/health-services/patient-management/master-data/patient-company-guarantors`

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` (juga `/kiosk`, `/admin`), `PUT /{id}` | Request + `employeeId`, `familyMemberId`. Untuk penjamin internal `employeeId` wajib, kelayakan harus `OK`, dan identitas pasien wajib cocok dengan data HR (`422 PATIENT_IDENTITY_MISMATCH`; `503` bila HR tidak terbaca). `benefitRelationship` yang diisi klien ditolak `400` |

#### Administrator / Master Data / Company Guarantor (diperbarui)

Base URL: `api/v1/administrator/master-data/company-guarantors`

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST`, `PUT /{id}` | Request + `isEmployeeBenefitGuarantor` (opsional; kosong pada `PUT` berarti tidak diubah) (`FIN-DEC-227`) |
| `GET` daftar, rincian, `options` | Respons + `isEmployeeBenefitGuarantor` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Tanpa entity baru; logika baru di service; controller baru tidak mengakses context | `PASS` | Bagian preflight |
| Pencocokan hak akses: `ControllerName` = `EmployeeBenefitLookup` = argumen 1 `[AccessPermission]`; argumen 1 `[AccessAction]` = `Read` = argumen 2; `AccessType = Read` | Cocok pada 3 endpoint | `PASS` | Review controller |
| Pencocokan identitas pasien vs HR (`FIN-DEC-228`) | Verifikasi NIK (`SELF`) dan NIK / Nama + Tanggal Lahir (`FAMILY`); penolakan `422 PATIENT_IDENTITY_MISMATCH` terpasang di `ResolveCardLinkAsync` | `PASS` | Code inspection `RegistrationEmployeeBenefitLookupService.cs` |
| Seluruh titik penulis kartu dan sumber pembayaran kartu | Kartu: `Create` (termasuk `/kiosk` dan `/admin`) dan `PUT`; kunjungan: registrasi dan ganti penanggung (dipakai juga admisi rawat inap lewat `InitializeAdmissionAsync`). `EncounterIntakeService` hanya tunai/asuransi | `PASS` | `grep` `new RegPatientEncounterGuarantor`, `SwitchPaymentSourceAsync`, `InitializeAdmissionAsync` |
| Pemanggil lain `EncounterPaymentSourceService` | `BillingPayerEditService` memetakan status `422` ke `BillingPayerEditValidationException`; `InpAdmissionTransferService` meneruskan kode status | `PASS` | Review pemanggil |
| Tanggal pelayanan tidak bergeser hari | Tanggal diteruskan apa adanya (bukan `.Date` yang sudah dipotong) | `PASS` | Review `ToOperationalDayUtc` |
| Log tidak memuat data pribadi | Terpenuhi | `PASS` | Review semua panggilan logger baru |
| Review diff dan scope | 14 berkas backend terubah/baru; akhir baris dipertahankan per berkas. Berkas `docs/module-blueprints/billing-kasir/02-backend-architecture.md` ikut berubah di working tree **bukan dari task ini** dan tidak disentuh | `PASS` | `git status`, `git diff --stat` |
| `dotnet restore` / `dotnet build` | — | `NOT RUN` | Pemilik meminta tanpa build otomatis. **Wajib dijalankan sebelum task ditandai ✅** |
| Pembuatan file migration | — | `NOT RUN` | Lihat perintah di bawah |
| Verifikasi runtime (contoh bagian 2) | — | `NOT RUN` | Memerlukan build, migration, penanda penjamin internal terisi, dan data HR uji |

Uji manual: `REQUIRED`. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah yang perlu dijalankan pemilik**

```bash
dotnet build
dotnet ef migrations add AddEmployeeBenefitGuarantorFlagToCompanyGuarantor
```

Isi yang diharapkan: satu kolom `IsEmployeeBenefitGuarantor` (`boolean`, `NOT NULL`, bawaan `false`) dan indeks unik terfilter
`UX_MstCompanyGuarantor_EmployeeBenefit`. Tidak perlu kueri pra-periksa dan tidak ada pengisian data lama.

**Langkah operasional setelah migrasi.** Aturan baru **belum berlaku** sampai penjamin internal ditandai. Pastikan master penjamin "RS Benefit"
sudah ada (`FIN-OQ-102` CLOSED), lalu tandai lewat `PUT /api/v1/administrator/master-data/company-guarantors/{id}` dengan
`isEmployeeBenefitGuarantor: true`, atau, selama layar master belum diubah, lewat pembaruan data langsung oleh administrator.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Mengetik "198" menampilkan pegawai aktif yang cocok (NIK tersamar, maksimum 20); "19" ditolak `400` | Terpenuhi di source, belum terbukti runtime | `EmployeeBenefitLookupController.SearchEmployees`; `EmployeeBenefitEligibilityService.SearchEmployeesAsync` |
| 2. Sari terdaftar dan terverifikasi: kartu terbit, hubungan `CHILD` | Terpenuhi di source | `ResolveCardLinkAsync` → `CheckAsync`; kode hubungan dari `FromFamilyRelationship` |
| 3. Pak Andi tidak terdaftar: ditolak dengan pesan yang diminta | Terpenuhi di source | Pesan `DEPENDENT_NOT_REGISTERED` pada `MessageFor`; bila tidak terdaftar sama sekali sebagai anggota keluarga, `FAMILY_MEMBER_NOT_FOUND` |
| 4. Tanggungan belum terverifikasi atau di luar masa berlaku: ditolak dengan pesan berbeda | Terpenuhi di source | `DEPENDENT_NOT_VERIFIED`, `DEPENDENT_OUT_OF_PERIOD` |
| 5. Pegawai tanpa enrollment aktif ditolak (`FIN-DEC-209`) | Terpenuhi di source | `ENROLLMENT_NOT_ACTIVE` |
| 6. Klien yang mengirim kode hubungan ditolak `400` | Terpenuhi di source | `ResolveCardLinkAsync` (`RELATIONSHIP_FROM_CLIENT`) |
| 7. Pencarian hanya mengembalikan NIP, NIK tersamar, nama, dan unit | Terpenuhi di source | `EmployeeLookupResponse` (ditambah `EmployeeId` dan penanda benefit aktif sesuai kontrak) |
| 8. Identitas pasien dicocokkan dengan data HR (`FIN-DEC-228`): NIK/nama/tgl lahir tidak cocok ditolak `422` `PATIENT_IDENTITY_MISMATCH` | Terpenuhi di source | `RegistrationEmployeeBenefitLookupService.ResolveCardLinkAsync`; `PatientCompanyGuarantorController` |
| DoD: preflight selesai | Terpenuhi | Bagian preflight |
| DoD: kontrak cocok | Terpenuhi dengan delta tercatat | Bagian 7 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| Verifikasi roadmap: `dotnet build` | **Belum** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Keputusan pemilik pada sesi ini** | Penjamin internal dikenali lewat **kolom penanda baru** `MstCompanyGuarantor.IsEmployeeBenefitGuarantor` (diratifikasi lewat `FIN-DEC-227`, multi-penjamin internal diperbolehkan, `FIN-OQ-102` resmi **CLOSED**). Aturan kecocokan identitas pasien vs data HR diratifikasi lewat `FIN-DEC-228` (penolakan `422 PATIENT_IDENTITY_MISMATCH`). Spesifikasi UX pencarian pegawai dan keluarga di form kartu `FE-FIN-046` diratifikasi lewat `FIN-DEC-229`. Aturan transisi kartu internal unlinked `CARD_NOT_LINKED` diratifikasi lewat `FIN-DEC-230`. Seluruhnya telah dicatat di `00-interview-decisions.md` dan kamus data `data-dictionary.md`. |
| Delta kontrak | (1) Kolom `IsEmployeeBenefitGuarantor` telah dicatat pada kamus data (Bagian 11). (2) `FIN-VAL-260` menulis "DTO tidak memuat field itu"; sedangkan kriteria 6 meminta penolakan `400`. Diwujudkan dengan properti `BenefitRelationship` pada request yang **selalu ditolak bila terisi**, karena DTO tanpa properti itu akan mengabaikannya diam-diam. (3) Penanda "hasil terpotong" (`FIN-VAL-269`) dibawa pada **pesan** `ApiResponse`, karena kontrak menetapkan tipe respons `List<EmployeeLookupResponse>`. (4) `OrganizationUnitName` diisi dari `MstDepartment.DepartmentName` departemen utama pegawai (`MstEmployee` tidak punya unit organisasi langsung). (5) Penolakan di endpoint registrasi kunjungan tetap berstatus `400` mengikuti pemetaan lama endpoint itu, bukan `422`; pada ganti penanggung `422`. (6) Kartu penjamin biasa yang disertai tautan HR ditolak `400`. (7) `EmployeeNumber` pada request kartu tetap wajib; untuk kartu internal klien mengirim NIP dari hasil pencarian dan server menimpanya dengan data HR |
| Dampak pada frontend | Form kartu perlu: memilih pegawai lewat pencarian, memilih anggota keluarga, dan menampilkan pesan penolakan; form master penjamin perlu kotak centang "Penjamin internal (RS Benefit)" (`FIN-DEC-229`). Pekerjaan frontend `FE-FIN-046` |
| **Status celah identitas pasien** | **SELESAI DITUTUP di source (`FIN-DEC-228`).** Backend kini memeriksa kesesuaian NIK pasien terhadap pegawai (`SELF`), serta kesesuaian NIK / Nama Lengkap + Tanggal Lahir terhadap anggota keluarga HR sebelum kartu diterbitkan/diperbarui. |
| Risiko tersisa | Kode belum dikompilasi (`dotnet build` NOT RUN atas instruksi pemilik). Kartu internal lama tanpa tautan akan ditolak saat kunjungan (`CARD_NOT_LINKED`) setelah penjamin ditandai (`FIN-DEC-230`). Kode hubungan pada kartu tidak diperbarui otomatis bila HR mengubah hubungan sesudah kartu terbit. `BillingPayerEditService` masih menampilkan kartu internal sebagai "tersedia" walau penggantian akan ditolak saat diterapkan; perbaikannya milik pemilik Billing |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Satu percobaan menjalankan skrip edit lewat heredoc di awal sesi sebelumnya gagal sebelum menulis apa pun; tidak terkait task ini |
| Status Git | Berkas pada 3.2 berstatus ` M`/`??`; berkas `billing-kasir/02-backend-architecture.md` berubah **bukan** dari task ini |
| `requirement-traceability.md` | Tidak ada pada blueprint ini; traceability dipegang tabel di roadmap |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan membuat migration di atas. Sesudahnya `BE-FIN-107` (dan `108`) hanya menunggu `GATE-01`. Pekerjaan frontend `FE-FIN-046` (Registrasi) dapat dimulai |
