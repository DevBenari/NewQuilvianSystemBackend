# Laporan Perubahan Backend — `BE-FIN-105`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-105` |
| Judul | Registrasi: skema tautan kartu pasien–penjamin ke pegawai dan salinan snapshot kunjungan |
| Slice | `REV-19` Slice A — Fondasi HR Benefit dan Registrasi (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` |
| Trace | `FIN-DEC-197`, `198`, `199`; `FR-FIN-241`; `data/data-dictionary.md` 9.4, 9.5; `02-backend-architecture.md` `R.6`, `R.8` |
| Contract version | `FIN-INTEGRATION-1.8` (keputusan Registrasi `R1`..`R3` **approved** oleh pemilik Registrasi, 6 Oktober 2026) |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa >20 (2), berkas diubah 8 (2), menyentuh alur registrasi inti (1), kontrak API berubah: respons kartu (1), dampak schema: dua tabel (2), data sensitif (1), lintas modul (1) |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("mulai kerjakan BE-FIN-105", 13 Oktober 2026), lintas modul ke Patient Management dan Registration |
| Target tulis | Model dan konfigurasi `MstPatientCompanyGuarantor` dan `RegPatientEncounterGuarantor`; `PatientEncounterController.BuildPaymentSourceAsync`; `EncounterPaymentSourceService`; DTO dan pemetaan respons `PatientCompanyGuarantorController`; laporan ini; baris status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a73f5593` (branch `Yasmina`). Working tree juga memuat perubahan `BE-FIN-104` yang belum di-commit; tidak saling tumpang tindih |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — source selesai; `dotnet build` **NOT RUN** (pemilik meminta tanpa build); file migration **belum dibuat** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | HealthServices / PatientManagement (MasterData) dan HealthServices / RegistrationManagement |
| Registry | `PatientManagement operational — Pat — ACTIVE`; `RegistrationManagement / Registration — Reg — ACTIVE / LEGACY`; prefix `Mst` untuk master (`Master / Reference — ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` — tidak ada entity baru; hanya kolom ditambahkan pada dua tabel yang ada. Nama tabel tidak diubah |
| Entity operasional baru | Tidak ada, sehingga `QBE-MOD-002`/`003` tidak terpicu |
| Relasi lintas area | FK `MstPatientCompanyGuarantor.EmployeeId → MstEmployee` dan `FamilyMemberId → WfpFamilyMember`, `Restrict`, **tanpa** navigation property. Governance tidak melarangnya, dan pola yang sama sudah dipakai `SlidingScaleExecutionConfiguration` (`HealthServices → MstEmployee`). Snapshot kunjungan sengaja tanpa FK (kamus data 9.5) |
| QBE yang berlaku | `QBE-CFG-001`/`002` (mapping di `IEntityTypeConfiguration`), `QBE-DTO-001` (kolom hanya muncul di DTO respons), `QBE-LOG-001` (tidak ada kolom sensitif baru di log), `QBE-NAM-001` (tanpa rename) |

---

## 1. Masalah yang diperbaiki

Kartu penjamin perusahaan pasien saat ini hanya menyimpan **nomor dan nama karyawan sebagai teks**. Contoh: kartu Sari
(nama samaran) bertuliskan "NIP 19870012, Budi". Sistem tidak tahu Budi yang mana di data HR, dan tidak tahu Sari itu anaknya.
Akibatnya Finance tidak bisa menerbitkan piutang ke pegawai yang pasti benar, dan HR tidak bisa memotong plafon orang
yang tepat. Task ini menyiapkan tempat penyimpanan tautan yang pasti, tanpa mengubah cara kartu disimpan hari ini.

## 2. Proses bisnis

**Tujuan.** Kartu pasien dapat membawa tautan ke pegawai HR, ke anggota keluarganya, dan ke kode hubungan. Setiap kunjungan
yang memakai kartu itu menyalin ketiganya, sehingga Billing dan Finance tahu persis siapa pemilik benefitnya.

| Langkah | Pelaku | Hasil |
| ---: | --- | --- |
| 1. Kartu internal diisi tautan HR (oleh pemeriksaan kelayakan — **`BE-FIN-106`**, bukan task ini) | Sistem, dari pilihan petugas Registrasi | `EmployeeId`, `FamilyMemberId`, `BenefitRelationship` terisi |
| 2. Petugas mendaftarkan kunjungan memakai kartu itu | Petugas Registrasi | Kunjungan menyalin ketiga tautan sebagai snapshot |
| 3. Petugas mengganti penanggung kunjungan ke kartu lain | Petugas Registrasi / Billing | Snapshot diganti dengan tautan kartu baru |
| 4. Petugas mengganti penanggung ke tunai atau asuransi | Petugas Registrasi / Billing | Snapshot dikosongkan bersama snapshot penjamin perusahaan lainnya |

**Aturan dengan contoh.**

| Kartu | Isi tautan | Diterima database? |
| --- | --- | --- |
| Kartu karyawan perusahaan rekanan (kartu lama) | Ketiganya kosong | Ya — perilaku lama tidak berubah |
| Budi sendiri | `EmployeeId` Budi, `FamilyMemberId` kosong, `SELF` | Ya |
| Sari, anak Budi | `EmployeeId` Budi, `FamilyMemberId` Sari, `CHILD` | Ya |
| `SELF` tetapi `FamilyMemberId` terisi | — | **Tidak** (`CK_MstPatientCompanyGuarantor_BenefitLink`) |
| `CHILD` tanpa `FamilyMemberId` | — | **Tidak** |
| `FamilyMemberId` terisi tanpa `EmployeeId` | — | **Tidak** |

**Hasil akhir.** Kunjungan Sari menyimpan "pegawai Budi, anggota keluarga Sari, `CHILD`" walaupun data HR berubah di kemudian hari.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`DATABASE_RULES`, `REPORT_TEMPLATE`, `TEST_POLICY`, `BACKEND_ENGINEERING_CONTRACT`);
registry kepemilikan modul; kamus data 9.4–9.5; `00-interview-decisions.md` (`FIN-DEC-197`..`199`, `204`);
model dan konfigurasi `MstPatientCompanyGuarantor`, `RegPatientEncounterGuarantor`; `PatientEncounterController.cs`
(`BuildPaymentSourceAsync`, respons kunjungan); `EncounterPaymentSourceService.cs`; `EncounterIntakeService.cs` (jalur tunai/asuransi
saja); `EncounterInsuranceService.cs` dan `BillingPayerEditService.cs` (pembaca, tidak menulis snapshot);
`PatientCompanyGuarantorController.cs` dan `PatientCompanyGuarantorDtos.cs` (request, respons, payload log);
`SlidingScaleExecutionConfiguration.cs` (pola FK lintas area); `MstEmployee.cs`, `WfpFamilyMember.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` | + `EmployeeId` (`Guid?`), `FamilyMemberId` (`Guid?`), `BenefitRelationship` (`string(50)?`) |
| `Repositories/Configurations/HealthServices/MstPatientCompanyGuarantorConfiguration.cs` | + `CK_MstPatientCompanyGuarantor_BenefitLink`; FK `FK_MstPatientCompanyGuarantor_EmployeeId` dan `_FamilyMemberId` (`Restrict`); indeks terfilter `IX_MstPatientCompanyGuarantor_EmployeeId` dan `_FamilyMemberId` |
| `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs` | + `EmployeeIdSnapshot`, `FamilyMemberIdSnapshot`, `BenefitRelationshipSnapshot` |
| `Repositories/Configurations/HealthServices/RegPatientEncounterGuarantorConfiguration.cs` | Pemetaan tiga kolom; indeks terfilter `IX_RegPatientEncounterGuarantor_EmployeeIdSnapshot`; tanpa FK |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | `BuildPaymentSourceAsync` menyalin tiga tautan dari kartu saat registrasi |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs` | Ganti penanggung ke kartu perusahaan menyalin tiga tautan; `ClearCompanyGuarantorSnapshots` mengosongkannya |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientCompanyGuarantorDtos.cs` | `PatientCompanyGuarantorResponse` (dan turunannya `DetailResponse`) + tiga field hanya-baca |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientCompanyGuarantorController.cs` | `MapResponse` dan `MapDetailResponse` mengisi tiga field |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Respons daftar dan rincian kartu (`/patient-company-guarantors`) mendapat tiga field baru yang **hanya-baca**. Request simpan kartu **tidak** berubah, sehingga klien tidak bisa mengisinya. Tidak ada endpoint baru |
| Database | Enam kolom nullable, satu check constraint, dua FK, tiga indeks. **File migration belum dibuat; belum diterapkan.** Kartu dan kunjungan lama tetap sah tanpa backfill (`FIN-DEC-186`) |
| Keamanan dan privasi | Ketiga kolom bertanda sensitif. Tidak ada log baru; log yang ada di controller kartu dan service ganti penanggung tidak memuat kolom baru (payload create memakai `PatientCompanyGuarantorCreateResponse` yang terpisah; payload update dan hapus memilih field secara eksplisit) |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perubahan respons:

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

Base URL: `api/v1/health-services/patient-management/master-data/patient-company-guarantors`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `` dan `/{id}` (respons `PatientCompanyGuarantorResponse` / `DetailResponse`) | + `EmployeeId` (`Guid?`), `FamilyMemberId` (`Guid?`), `BenefitRelationship` (`string?`); selalu `null` untuk kartu yang belum ditautkan | Tetap (tidak berubah) |
| `POST`, `PUT` | — | **Tidak berubah.** Field tautan tidak diterima | Tetap |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Tanpa entity baru; FK lintas area mengikuti pola yang sudah ada | `PASS` | Bagian preflight |
| Pencarian seluruh jalur penulis snapshot penjamin perusahaan | Dua jalur: registrasi (`PatientEncounterController`) dan ganti penanggung (`EncounterPaymentSourceService`); `EncounterIntakeService` hanya tunai/asuransi; Billing dan Clinical hanya membaca | `PASS` | `grep` `EmployeeNumberSnapshot =`, `new RegPatientEncounterGuarantor` |
| Request simpan kartu tidak memuat field tautan | Field hanya ada di kelas respons; pembuatan kartu memakai penugasan field eksplisit | `PASS` | `PatientCompanyGuarantorDtos.cs`, controller baris 493 |
| Payload log tidak memuat kolom sensitif baru | Terpenuhi | `PASS` | Review seluruh panggilan `_loggerService` di controller kartu |
| Review diff dan scope | 8 berkas, 117 baris ditambah, 1 baris diubah (`ToTable`); akhir baris CRLF dipertahankan | `PASS` | `git diff --stat` |
| `dotnet restore` / `dotnet build` | — | `NOT RUN` | Pemilik meminta tanpa build. **Wajib dijalankan sebelum task ditandai ✅** |
| Pembuatan file migration | `20261013055825_AddEmployeeLinkToPatientCompanyGuarantor` — dibuat **di luar sesi agent** pukul 12:58 dengan nama `ProbeCheck2`, lalu atas permintaan pemilik diganti nama oleh agent (nama berkas `.cs` dan `.Designer.cs`, nama class, dan ID pada atribut `[Migration]`; timestamp dipertahankan agar urutan tetap). Isinya cocok dengan task ini: 6 kolom, 3 indeks, `CK_MstPatientCompanyGuarantor_BenefitLink`, 2 FK | `DONE` (oleh pemilik; diganti nama oleh agent) | Berkas migration; **belum diterapkan** ke database |
| Uji manual registrasi dan ganti penanggung | — | `NOT RUN` | Memerlukan build dan migration |

Uji manual: `REQUIRED` — registrasi dengan kartu lama (snapshot kosong), registrasi dengan kartu bertautan (setelah
`BE-FIN-106` atau pengisian manual di database dev), ganti penanggung ke tunai (snapshot kosong).
`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah yang perlu dijalankan pemilik** (nama mengikuti `02-backend-architecture.md` `R.8`, migration 2):

```bash
dotnet build
dotnet ef migrations add AddEmployeeLinkToPatientCompanyGuarantor
```

Migration ini bebas urutan terhadap `AddHrBenefitBalanceMovementAndConstraints` (`BE-FIN-103`/`104`). Bila pemilik lebih
suka satu migration untuk ketiga task, cukup jalankan satu kali `migrations add` setelah semuanya ter-build. Semua kolom
nullable dan tanpa backfill, sehingga aman tanpa downtime; constraint baru tidak bisa gagal pada data lama karena
ketiga kolom kartu lama bernilai `NULL`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kartu lama tanpa tautan tetap terbaca dan tersimpan seperti semula | Terpenuhi di source | Kolom nullable; request dan penugasan simpan kartu tidak berubah; constraint menerima tiga kolom kosong |
| 2. Ketiga kolom kartu kosong semua, atau terisi konsisten (`SELF` tanpa `FamilyMemberId`), ditegakkan database | Terpenuhi di source, belum diterapkan ke database | `CK_MstPatientCompanyGuarantor_BenefitLink` |
| 3. Kunjungan dengan kartu bertautan menyalin ketiga snapshot persis; kartu tanpa tautan menghasilkan snapshot kosong | Terpenuhi di source | `BuildPaymentSourceAsync` dan jalur ganti penanggung menyalin nilai apa adanya |
| 4. Snapshot dikosongkan bersama snapshot lain saat sumber pembayaran dilepas | Terpenuhi di source | `ClearCompanyGuarantorSnapshots` |
| 5. Tidak ada kolom sensitif yang masuk log | Terpenuhi | Review payload log (bagian 5) |
| DoD: preflight selesai | Terpenuhi | Bagian preflight |
| DoD: file migration siap | Terpenuhi | `20261013055825_AddEmployeeLinkToPatientCompanyGuarantor` |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| Verifikasi roadmap: `dotnet build` atas permintaan pemilik | **Belum** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap roadmap | Risiko roadmap menyebut kemungkinan "dikerjakan sebagai `Guid` tanpa FK" bila preflight menolak FK lintas area. Preflight **tidak** menolak (tidak ada aturan yang melarang dan sudah ada preseden), sehingga diikuti kamus data 9.4: FK `Restrict` |
| Delta terhadap kamus data | Indeks kartu dibuat **terfilter** (`IS NOT NULL`) karena hampir semua kartu tidak bertaut; ditambah indeks `FamilyMemberId` agar FK tidak memicu pemindaian penuh saat anggota keluarga dihapus HR |
| Temuan di luar lingkup (tidak diperbaiki) | `EncounterPaymentSourceService` jalur ganti penanggung ke kartu perusahaan menyalin `EffectiveStartDate`/`EffectiveEndDate` kartu (kolom `date`, `Kind=Unspecified`) apa adanya ke kolom snapshot `timestamptz`. `PatientEncounterController` mengonversinya dengan `ToUtcDate` dan berkomentar bahwa tanpa konversi itu `SaveChanges` gagal (500). Kemungkinan cacat yang sama ada di jalur ganti penanggung bila kartu punya masa berlaku; perlu dikonfirmasi saat uji manual dan diperbaiki pemilik Registrasi/Billing |
| Temuan di luar lingkup (tidak diperbaiki) | Log `CreatePatientCompanyGuarantor` dan `SetPatientCompanyGuarantorPrimaryFromKiosk` memuat seluruh `PatientCompanyGuarantorCreateResponse`, termasuk nama pasien dan nomor karyawan. Bukan dari task ini; dicatat untuk pemilik Patient Management (`QBE-LOG-001`) |
| Risiko tersisa | Kode belum pernah dikompilasi. Konfirmasi pemilik Registrasi atas `FIN-DEC-204` dan `209` belum turun; tidak memengaruhi task ini (aturan "harus terdaftar" ada di `BE-FIN-106`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Satu percobaan menjalankan skrip edit lewat heredoc gagal di parser shell sebelum menulis apa pun; diulang dengan skrip berkas |
| Status Git | ` M` delapan berkas pada 3.2; `??` laporan ini. Perubahan `BE-FIN-104` yang belum di-commit tetap ada dan tidak disentuh |
| `requirement-traceability.md` | Tidak ada pada blueprint ini; traceability dipegang tabel di roadmap |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan membuat migration. `BE-FIN-106` (pencarian pegawai, aturan "harus terdaftar", pengisian tautan kartu) kini hanya menunggu build/migration `BE-FIN-103` dan task ini |
