# Laporan Perubahan Backend — `BE-MDF-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-005` |
| Judul | Entity kesepakatan tarif dan baris tarif |
| Slice | `MVP-0` (`EPIC MDF-01`, `MDF-03`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-008`..`010`, `MF-DEC-004`, `012`; `MDF-VAL-1.0` §2–3; `erd/data-dictionary.md` bagian 2–3; `erd/sharing-rule.md` |
| Contract version | `MDF-VAL-1.0`, terkunci 20 September 2026. `MDF-API-1.0` tidak berlaku: task ini tidak membuat endpoint |
| Dependency | `BE-MDF-002` — 🟡 sebagian: entity peran sudah ada di source, migration basis data belum dibuat |
| Klasifikasi | `LIGHT` — dua entity, dua configuration, dua `DbSet`, satu navigasi relasi, nol endpoint, nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — acceptance criteria terpetakan ke source, tetapi `dotnet build` dan uji `T-07`..`T-13` belum dijalankan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `MasterData` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE). Sub-folder `MasterData` diproteksi pengecualian resmi `EXP-QBE-MOD-002-MDF-001` dan `EXP-QBE-MOD-002-MDF-002` pada `QBE_EXCEPTIONS.json` mengikuti preseden `EXP-QBE-MOD-002-PAT-001` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` (baris `Mdf` dicatat di `BE-MDF-001`) |
| QBE ID yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` |
| QBE ID yang tidak berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada service, endpoint, kode bisnis, maupun migration |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Transaksi — aggregate root kesepakatan tarif dan baris tarif berversi |

---

## 1. Masalah yang diperbaiki

Sebelum task ini dikerjakan, sistem Medical Fee belum memiliki struktur data untuk merekam kesepakatan pembagian jasa medis antara rumah sakit dan tenaga medis (dokter maupun tenaga kesehatan). Tanpa entitas ini:
1. Tidak ada rujukan formal antara kontrak kerja tenaga medis di HR (`WfpContractHistory`) dengan hak tarif jasa medis di sistem.
2. Tidak ada wadah untuk mendefinisikan porsi persentase tarif sharing per peran pelaksana medis pada lingkup layanan atau kategori tindakan tertentu.
3. Mesin perhitungan jasa medis (`BE-MDF-010`) tidak memiliki dasar data untuk mengalokasikan pendapatan layanan kepada pelaksana medis.

Contoh konkret skenario rumah sakit:
Seorang dokter bedah konsultan (`Dr. Budi, Sp.B`) menandatangani perjanjian kerja sama (PKS). Rumah sakit menyepakati bahwa untuk tindakan operasi bedah umum, dokter tersebut memperoleh 40% dari tarif kotor tindakan sebagai operator utama (`OPERATOR`), sementara asisten bedah (`ASSISTANT`) memperoleh 15%. Kesepakatan ini harus tertaut langsung ke nomor kontrak HR dokter bersangkutan tanpa menyalin ulang masa berlakunya, dan persentase tarifnya wajib memiliki batasan tegas (0% hingga 100%).

---

## 2. Proses bisnis

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan struktur entity dan mapping EF Core untuk kesepakatan tarif sharing (`MdfSharingAgreement`) dan rincian baris aturan tarif (`MdfSharingRule`) |
| Pelaku | Belum ada pelaku langsung — task ini fokus pada lapisan persistensi/entity. Antarmuka operasional pengguna dibangun pada `BE-MDF-006` |
| Pemicu | Roadmap `MVP-0`, sesudah `BE-MDF-002` |

Aturan bisnis dan invarian yang tertanam pada bentuk data:

1. **Rujukan Kontrak HR Tanpa Duplikasi**:
   `MdfSharingAgreement` menunjuk langsung `WfpContractHistory` melalui `SourceContractHistoryId` dengan relasi FK `DeleteBehavior.Restrict`. Sistem tidak menyalin nomor kontrak atau dokumen ke tabel Medical Fee (`MDF-DES-008`, `MF-DEC-012`). Jika kontrak kerja di HR berakhir atau dihapus, kesepakatan tarif tidak boleh dibiarkan menjadi yatim piatu.
2. **Keunikan Nomor Kesepakatan**:
   `AgreementNumber` wajib unik untuk setiap data yang belum dihapus (`IsDelete = false`), diproteksi melalui partial unique index database.
3. **Validasi Batas Periode dan Status**:
   `EffectiveEnd` boleh kosong (mengikuti masa aktif kontrak sumber). Jika terisi, `EffectiveEnd` wajib lebih besar atau sama dengan `EffectiveStart`. Status kesepakatan dibatasi check constraint: `Draft`, `Active`, `Expired`, `Terminated`.
4. **Pembagian Hak Jasa Berbasis Persentase**:
   `MdfSharingRule.SharingPercentage` memiliki tipe `decimal(5,2)` dengan check constraint ketat: nilai wajib antara `0.00` dan `100.00`.
5. **Dukungan Skema Versi Tarif (Versioning)**:
   Perubahan nilai tarif sharing di masa mendatang tidak menimpa (*overwrite*) baris lama, melainkan membentuk baris baru dan menautkan baris lama melalui `SupersededByRuleId` (self-referencing FK `Restrict`). Hal ini menjamin angka riwayat perhitungan jasa medis pada periode sebelumnya tidak akan pernah bergeser.
6. **Hierarki Lingkup Tarif**:
   `TariffId` dan `TariffCategoryId` bersifat nullable pada `MdfSharingRule`. Aturan dapat berlaku spesifik per tindakan, per kategori tindakan, maupun untuk seluruh tindakan secara umum.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/erd/data-dictionary.md` bagian 2 & 3 | Kolom, tipe data, indeks, default value, dan check constraint untuk `MdfSharingAgreement` dan `MdfSharingRule` |
| `docs/module-blueprints/medical-fee/contracts/validation-matrix.md` §2 & 3 | Matriks validasi aturan bisnis 2.1–2.11 dan 3.1–3.9 (`MDF-VAL-1.0`) |
| `docs/module-blueprints/medical-fee/02-backend-architecture.md` bagian 4.2 & 4.3 | Tanggung jawab aggregate root, spesifikasi relasi, dan penempatan berkas |
| `docs/module-blueprints/medical-fee/erd/sharing-rule.md` | Diagram ERD, 4 tingkat kekhususan lingkup tarif, dan aturan versing tarif |
| `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpContractHistory.cs` | Model entitas kontrak HR yang dirujuk |
| `Areas/HealthServices/MasterData/Models/MstTariff.cs` & `MstTariffCategory.cs` | Model entitas tarif dan kategori tarif yang dirujuk |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MstMedicalFeeRole.cs` | Model peran pelaksana yang dirujuk dan penambahan navigasi koleksi |
| `Repositories/Configurations/Corporate/FinanceManagement/PettyCash/FinPettyCashBudgetConfiguration.cs` | Pola implementasi EF Core CheckConstraint dan ConcurrencyToken |
| `Repositories/ApplicationDbContext.cs` | Pendaftaran `DbSet` di bawah `#region MEDICAL FEE MANAGEMENT` |
| `docs/engineering/QBE_EXCEPTIONS.json` | Pendaftaran pengecualian jalur `MasterData` untuk prefix `Mdf` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MdfSharingAgreement.cs` | **Baru.** Entity aggregate root `MdfSharingAgreement : IdentityModel` lengkap dengan helper `MdfSharingAgreementStatuses`, `MdfPayeeTypes`, kolom `RowVersion`, dan relasi navigasi ke `WfpContractHistory` serta `MdfSharingRule` |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MdfSharingRule.cs` | **Baru.** Entity `MdfSharingRule : IdentityModel` dengan relasi ke `Agreement`, `Tariff`, `TariffCategory`, `Role`, self-referencing `SupersededByRule`, dan presisi persentase `HasPrecision(5,2)` |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MstMedicalFeeRole.cs` | **Diperbarui.** Menambahkan properti navigasi `public ICollection<MdfSharingRule> SharingRules` |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MdfSharingAgreementConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk tabel `public."MdfSharingAgreement"`: partial unique index `AgreementNumber`, check constraint `CK_MdfSharingAgreement_Status`, `CK_MdfSharingAgreement_PayeeType`, `CK_MdfSharingAgreement_Period`, indeks pencarian, dan relasi FK Restrict/Cascade |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MdfSharingRuleConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk tabel `public."MdfSharingRule"`: check constraint `CK_MdfSharingRule_Percentage`, `CK_MdfSharingRule_Period`, indeks relasi & rentang waktu, serta 5 konfigurasi foreign key |
| `Repositories/ApplicationDbContext.cs` | **Diperbarui.** Mendaftarkan `public DbSet<MdfSharingAgreement> MdfSharingAgreements` dan `public DbSet<MdfSharingRule> MdfSharingRules` di bawah `#region MEDICAL FEE MANAGEMENT` |
| `docs/engineering/QBE_EXCEPTIONS.json` | **Diperbarui.** Mendaftarkan pengecualian resmi `EXP-QBE-MOD-002-MDF-001` dan `EXP-QBE-MOD-002-MDF-002` untuk model `Mdf*` di subfolder `MasterData` sesuai keputusan arsitektur `MDF-DES-001` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini berfokus pada perancangan entity dan configuration. Endpoint kesepakatan dan baris tarif dikerjakan pada `BE-MDF-006` |
| Database | Model EF Core bertambah 2 tabel transaksi (`MdfSharingAgreement` dan `MdfSharingRule`), relasi FK ke 4 tabel eksternal, dan 5 check constraint. **Migration belum dibuat** (butuh otorisasi terpisah pada `BE-MDF-003`) |
| Keamanan/Auth | `NOT APPLICABLE` — Tidak ada endpoint atau perubahan role access |

### 3.4 Selisih terhadap blueprint

| Butir | Blueprint | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Sub-folder `MasterData` | `MDF-DES-001` menempatkan `MdfSharingAgreement` di `MedicalFeeManagement/MasterData/Models/` | Ditempatkan persis sesuai blueprint, dan didaftarkan ke `QBE_EXCEPTIONS.json` | Menjamin kepatuhan arsitektur modular tanpa menimbulkan bentrok alias `MasterData` pada checker QBE |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Task ini tidak menyentuh pembuatan endpoint API. Endpoint kesepakatan tarif sharing dijadwalkan pada task `BE-MDF-006`.

---

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: **tanpa `dotnet build` otomatis** |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang pada lingkungan eksekusi |
| Review diff dan cakupan perubahan | 4 berkas baru, 3 berkas dimodifikasi, nol file migration, nol berkas di luar wewenang | `PASS` | `git status --short`, `git diff` |
| Pemeriksaan warisan `IdentityModel` & soft delete | Kedua entity mewarisi `IdentityModel`; tidak ada hard delete | `PASS` | `MdfSharingAgreement : IdentityModel`, `MdfSharingRule : IdentityModel` |
| Konfigurasi Check Constraint & Presisi | 5 check constraint dan presisi persentase `(5, 2)` terpasang sesuai spek | `PASS` | `MdfSharingAgreementConfiguration.cs`, `MdfSharingRuleConfiguration.cs` |
| Pemeriksaan baris registry & pengecualian | Baris `Mdf` aktif; pengecualian `EXP-QBE-MOD-002-MDF-001/002` tercatat di `QBE_EXCEPTIONS.json` | `PASS` | `docs/engineering/QBE_EXCEPTIONS.json` |
| Pemeriksaan karakter baris (Line endings) | LF murni, UTF-8 tanpa BOM, nol karakter `\r` | `PASS` | `grep -c $'\r'` bernilai 0 pada seluruh berkas baru |
| Skenario uji fungsional `T-07`..`T-13` | Tidak dijalankan | `NOT RUN` | Menuntut tabel fisik basis data (`BE-MDF-003`) dan layer service (`BE-MDF-006`) yang belum dibuat |
| Migration database | Tidak dibuat | `NOT APPLICABLE` | Otorisasi pembuatan migration terpisah sesuai `AGENTS.md` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "FK `Restrict` ke kontrak HR" | Terpetakan ke source; **menunggu penegakan basis data via migration** | `MdfSharingAgreementConfiguration.cs` — `builder.HasOne(x => x.SourceContractHistory).WithMany().HasForeignKey(x => x.SourceContractHistoryId).OnDelete(DeleteBehavior.Restrict)` |
| Acceptance: "Persentase 0..100 sebagai check constraint" | Terpetakan ke source; **menunggu penegakan basis data via migration** | `MdfSharingRuleConfiguration.cs` — `table.HasCheckConstraint("CK_MdfSharingRule_Percentage", "\"SharingPercentage\" >= 0 AND \"SharingPercentage\" <= 100")` |
| Verifikasi: `T-07`..`T-13` | **Belum terpenuhi** | `NOT RUN` — bergantung pada tersedianya migration `BE-MDF-003` dan service `BE-MDF-006` |
| DoD: "Menunjuk kontrak HR, tidak menyalin" | Terpenuhi | Kolom `SourceContractHistoryId` menunjuk `WfpContractHistory`; tidak ada kolom nomor kontrak atau dokumen ganda |
| DoD umum #2: Seluruh entity mewarisi `IdentityModel`; nol hard delete | Terpenuhi | `MdfSharingAgreement` dan `MdfSharingRule` mewarisi `IdentityModel` |
| DoD umum #4: Status sebagai `string` + `static class ...Statuses` + check constraint | Terpenuhi | `MdfSharingAgreementStatuses` dan `CK_MdfSharingAgreement_Status` |
| DoD umum #5: `Guid RowVersion` pada aggregate root | Terpenuhi | `MdfSharingAgreement.RowVersion` terpasang dengan `IsConcurrencyToken()` |
| DoD umum #9: Partial unique index memakai `WHERE "IsDelete" = false` | Terpenuhi | Index `AgreementNumber` difilter `WHERE "IsDelete" = false` |
| DoD umum #10: Persentase `HasPrecision(5, 2)` | Terpenuhi | `builder.Property(x => x.SharingPercentage).HasPrecision(5, 2)` |
| DoD umum #12: EF configuration di `Repositories/Configurations/HealthServices/MedicalFeeManagement/` | Terpenuhi | Ditempatkan di folder `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/` |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas instruksi eksplisit pemilik (tanpa build otomatis) |

Catatan: Status task ditandai sebagai `🟡 SEBAGIAN` karena kompilasi `dotnet build` dan eksekusi skenario uji `T-07`..`T-13` belum dijalankan sesuai instruksi lingkungan.

---

## 7. Catatan penutup dan Langkah Selanjutnya

1. **Efek Terhadap Model EF Core**:
   Penambahan entitas `MdfSharingAgreement` dan `MdfSharingRule` beserta `MstMedicalFeeRole` sebelumnya mengakibatkan snapshot model EF Core memiliki perubahan yang belum dituangkan ke dalam migration. Hal ini wajar dan telah diantisipasi: pembuatan migration aditif formal akan dilakukan pada task `BE-MDF-003` (`AddMedicalFeeMasterData`) setelah otorisasi terpisah diberikan.
2. **Task Berikutnya**:
   - `BE-MDF-007`: Perancangan entity periode, hasil jasa medis, rincian hasil jasa, penyesuaian koreksi, dan daftar layanan belum terhitung (`MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService`).
   - `BE-MDF-003`: Pembuatan migration `AddMedicalFeeMasterData` mencakup tabel `MstMedicalFeeRole`, `MdfSharingAgreement`, dan `MdfSharingRule`.

