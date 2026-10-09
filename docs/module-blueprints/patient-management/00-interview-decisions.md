# Patient Management — Keputusan yang Disetujui

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `revision` | `2` — 8 Oktober 2026. Revision `1` memuat `PAT-OQ-001`–`004` sebagai default `PROPOSED`; revision `2` mencatat keputusan final review atas keempatnya dan menambah `PAT-OQ-005`–`007` |
| Status | **`approved`** — seluruh keputusan final 8 Oktober 2026. **Tidak ada butir terbuka** |
| Disetujui oleh | Pemilik/leader work item, lewat dua sesi `MODULE BLUEPRINT` 8 Oktober 2026 (akun Git `devbenari`) |
| Sumber | Keputusan tertulis pengguna pada sesi tersebut, dicatat apa adanya. Berkas ini **bukan** hasil wawancara `grill-me` |
| Cakupan | Satu work item: `BE-PAT-MIG-001` |

> **Catatan cakupan.** Berkas ini hanya memuat keputusan untuk satu pekerjaan migrasi data. Ia
> **bukan** blueprint lengkap modul Patient Management. Kemampuan pasien lain — pendaftaran,
> penggabungan, foto, dan seterusnya — tidak dibahas di sini dan tidak boleh disimpulkan dari
> berkas ini.

---

## 1. Latar belakang dalam bahasa sederhana

Rumah sakit sedang memindahkan data pasien lama RSMMC ke Quilvian di lingkungan *staging*
(lingkungan uji coba sebelum produksi). Setiap pasien punya **nomor rekam medis** (*medical
record number*, disingkat **MRN**). Nomor ini dipakai petugas untuk mencari berkas pasien, dan
dicetak sebagai QR pada kartu pasien.

Seluruh nomor pasien RSMMC sudah direncanakan dan dikunci pada satu tabel perencana. Masalahnya:

1. Uji coba pertama (**Pilot V1**) memasukkan **715 pasien** lewat API pembuatan pasien biasa.
2. API itu membuat MRN sendiri, sehingga 715 pasien tersebut mendapat nomor buatan Quilvian —
   **bukan** nomor yang sudah direncanakan.
3. Nomor-nomor buatan itu ternyata **dibutuhkan pasien RSMMC lain** menurut rencana.
4. Karena itu, 715 pasien Pilot wajib dipindahkan ke nomor yang benar **lebih dulu**, sebelum sisa
   pasien dimigrasikan. Kalau urutannya dibalik, pasien berikutnya akan bertabrakan nomor.

**Contoh dengan data samaran.** Pasien "Budi Contoh" (`legacy_pid` 100001) saat ini bernomor
`01-23-45-67`, dibuat otomatis oleh Quilvian. Rencana kanonik menetapkan nomornya `00-98-76-54`.
Rekonsiliasi mengubah nomornya menjadi `00-98-76-54` dan membuat QR baru untuk nomor itu.
`Id`, `PatientCode`, nama, tanggal lahir, kunjungan, dan seluruh data lain tidak berubah. QR lama
di folder `01-23-45-67` dibiarkan tetap ada.

---

## 2. Keputusan

| ID | Keputusan |
| --- | --- |
| `PAT-DEC-001` | Penempatan modul: Area **HealthServices**, Module **PatientManagement**, prefix **`Pat`**. Registry mencatat status `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 24). Pekerjaan ini **tidak** dipindah ke modul `rekam-medis` atau modul lain |
| `PAT-DEC-002` | Slug blueprint `patient-management`. Bentuk blueprint **`SINGLE`**, sehingga `<blueprint-root>` = `docs/module-blueprints/patient-management/`. Bentuk ini mengikuti jalur roadmap dan laporan yang disetujui pengguna (`shape_decided_by: USER`) |
| `PAT-DEC-003` | Task ID **`BE-PAT-MIG-001`**, judul **RSMMC Pilot MRN Reconciliation**, klasifikasi task **`LEGACY MIGRATION`** (migrasi data pasien lama). Lihat catatan istilah pada bagian 5 |
| `PAT-DEC-004` | Batch migrasi kanonik: `45eeefba-dd39-6831-1bd2-d986851f7c1f` |
| `PAT-DEC-005` | Sumber MRN tujuan **satu-satunya** adalah `migration.rsmmc_patient_mrn_plan.final_medical_record_number`. Kolom `migration.rsmmc_patient_crosswalk.normalized_mrn` **bukan** MRN tujuan, karena audit crosswalk membuktikan isinya bukti historis Pilot V1 yang tidak konsisten |
| `PAT-DEC-006` | Pemetaan identitas pasien: `crosswalk.legacy_pid` → `crosswalk.quilvian_patient_id` → `public."MstPatient"."Id"` |
| `PAT-DEC-007` | Identitas tidak berubah: `MstPatient.Id`, `PatientCode`, serta seluruh relasi dan *foreign key* (rujukan antartabel) tetap seperti sekarang |
| `PAT-DEC-008` | QR baru wajib dibuat dari MRN kanonik — isi QR, folder, dan `QrCodePath` mengikuti nomor baru. File dan folder QR lama **tidak boleh dihapus** selama rekonsiliasi |
| `PAT-DEC-009` | Tabel `migration.rsmmc_patient_pilot_reconcile_backup` adalah cadangan permanen untuk pemulihan. Tabel ini bukti yang **tidak boleh diubah**: tidak dibuat ulang, tidak di-*update*, tidak dikosongkan, tidak dihapus |
| `PAT-DEC-010` | Rekonsiliasi Pilot wajib selesai **sebelum** migrasi sisa pasien RSMMC |
| `PAT-DEC-011` | Mekanismenya adalah endpoint admin **sementara**, **khusus staging**, wajib login. Masukannya `batchId`, `dryRun`, `limit`. `limit` bawaan **25**, rentang **1 sampai 100**. Urutan `legacy_pid` naik. Satu pasien satu transaksi. Berhenti pada kegagalan tak terduga yang pertama |
| `PAT-DEC-012` | **Batas wewenang.** `BE-PAT-MIG-001` hanya memberi wewenang **implementasi source**. Yang tidak termasuk: membuat EF migration, menerapkan EF migration, menjalankan perintah migration database, mengubah data lewat SQL langsung, memanggil endpoint rekonsiliasi, menjalankan rekonsiliasi 715 pasien, deployment, dan eksekusi di produksi. Eksekusi runtime dan perubahan database memerlukan **otorisasi eksplisit terpisah** sesudah review source, build, test, dan deployment staging |
| `PAT-DEC-013` | Tabel `migration.rsmmc_*` yang sudah ada di staging dipakai sebagai infrastruktur migrasi apa adanya. Task ini tidak membuat atau mengubah strukturnya |
| `PAT-DEC-014` | Pemilik **meminta automated test terfokus secara eksplisit** (kriteria `AC-27`). Permintaan ini memenuhi syarat pengecualian `rules/backend/TEST_POLICY.md` bagian 3 pada suite Skill — acceptance criteria pada kartu task yang disetujui pemilik sesudah kebijakan itu berlaku |
| `PAT-DEC-015` | Tidak ada pekerjaan frontend. Mekanisme ini endpoint admin sementara tanpa layar |
| `PAT-DEC-016` | 704.508 pasien RSMMC yang belum dibuat di Quilvian berada **di luar cakupan** task ini |

---

## 3. Fakta yang dilaporkan pengguna

> **Penting.** Seluruh angka di bawah dilaporkan pengguna dari audit database yang sudah
> dilakukan sebelumnya. Agent **tidak mengakses database** pada sesi ini dan **tidak
> memverifikasi** angka-angka ini. Angka dicatat sebagai prasyarat yang diklaim, bukan bukti
> yang diperiksa ulang.

### 3.1 Tabel perencana MRN — `migration.rsmmc_patient_mrn_plan`

| ID | Fakta | Nilai |
| --- | --- | ---: |
| `PAT-FACT-001` | Jumlah baris perencana | 705.223 |
| `PAT-FACT-002` | Baris berstatus `FINALIZED` | 705.223 |
| `PAT-FACT-003` | Baris yang sudah punya `final_medical_record_number` | 705.223 |
| `PAT-FACT-004` | MRN akhir yang berbeda satu sama lain | 705.223 |
| `PAT-FACT-005` | Tabrakan pada MRN tujuan akhir | 16 |
| `PAT-FACT-006` | MRN akhir hasil pembangkitan baru | 96.442 |
| `PAT-FACT-007` | MRN akhir yang mempertahankan nomor lama | 608.781 |

Pemeriksaan silang: 96.442 + 608.781 = 705.223. Angkanya cocok.

### 3.2 Pasien Pilot V1 di `public."MstPatient"`

| ID | Fakta | Nilai |
| --- | --- | ---: |
| `PAT-FACT-010` | Pasien Pilot | 715 |
| `PAT-FACT-011` | `legacy_pid` unik / `MstPatient.Id` unik / MRN sekarang unik / MRN tujuan unik | 715 / 715 / 715 / 715 |
| `PAT-FACT-012` | Sudah benar | 0 |
| `PAT-FACT-013` | Perlu ganti MRN | 715 |
| `PAT-FACT-014` | MRN tujuan kosong / format MRN tujuan tidak sah | 0 / 0 |
| `PAT-FACT-015` | MRN tujuan mempertahankan nomor lama / hasil pembangkitan kanonik | 383 / 332 |
| `PAT-FACT-016` | MRN tujuan sudah dipakai pasien lain | 0 |
| `PAT-FACT-017` | MRN sekarang dimiliki pasien lain | 0 |

Pemeriksaan silang: 383 + 332 = 715. Karena `PAT-FACT-016` bernilai 0, tidak ada dua pasien Pilot
yang perlu saling bertukar nomor. Satu pasien dapat diproses tanpa menunggu pasien lain.

### 3.3 QR, rujukan antartabel, dan cadangan

| ID | Fakta | Nilai |
| --- | --- | ---: |
| `PAT-FACT-020` | Pasien Pilot yang punya `QrCodePath` | 715 |
| `PAT-FACT-021` | `QrCodePath` yang memuat MRN sekarang | 715 |
| `PAT-FACT-022` | *Foreign key* yang merujuk `MstPatient` | 57 |
| `PAT-FACT-023` | *Foreign key* yang merujuk `MedicalRecordNumber` | 0 — seluruhnya merujuk `MstPatient.Id` |
| `PAT-FACT-024` | Kolom salinan MRN berupa teks yang cocok dengan 715 MRN Pilot sekarang | 0 |
| `PAT-FACT-025` | Baris tabel cadangan / `legacy_pid` unik / `Id` unik / MRN lama unik / MRN rencana unik / path QR tercadang | 715 / 715 / 715 / 715 / 715 / 715 |
| `PAT-FACT-026` | Baris cadangan yang MRN lamanya sudah sama dengan MRN rencana | 0 |

`PAT-FACT-022` sampai `PAT-FACT-024` menjadi dasar `PAT-DEC-007`. Karena tidak ada rujukan yang
memakai MRN, mengganti MRN tidak memutus relasi apa pun selama `MstPatient.Id` tetap.

---

## 4. Keputusan hasil review — `APPROVED`, final 8 Oktober 2026

Pada revision `1`, `PAT-OQ-001` sampai `PAT-OQ-004` adalah pertanyaan terbuka dengan default
aman berstatus `PROPOSED`. Pada review 8 Oktober 2026, pemilik memutuskan keempatnya dan
menambah tiga keputusan keamanan baru. ID-nya tidak diubah, karena ID tidak pernah dipakai ulang
atau diganti nama. Awalan `OQ` hanya menunjukkan asalnya sebagai pertanyaan; **seluruhnya kini
sudah diputuskan**.

| ID | Keputusan final | Status |
| --- | --- | --- |
| `PAT-OQ-001` | Lokasi automated test `AC-27`: project xUnit khusus PatientManagement di `Tests/QuilvianSystemBackend.PatientManagementTests/`. Rinciannya pada bagian 4.1 | **`APPROVED` dengan governance guard** |
| `PAT-OQ-002` | Endpoint hanya boleh berjalan di environment bernama persis `Staging`, untuk `dryRun=true` maupun `dryRun=false`. Rinciannya pada bagian 4.2 | **`APPROVED`** |
| `PAT-OQ-003` | Bila `dryRun` tidak dikirim, nilainya `true`. Tidak boleh ada default implisit `false` | **`APPROVED`** |
| `PAT-OQ-004` | Pasien yang MRN-nya sudah kanonik tidak dihitung dalam `limit`. Seluruh set Pilot tetap diperiksa statusnya. `QR_INCONSISTENT` berbeda dari `ALREADY_RECONCILED`. Rinciannya pada bagian 4.3 | **`APPROVED` dengan klarifikasi QR** |
| `PAT-OQ-005` | Path QR tujuan untuk MRN kanonik wajib diperiksa **sebelum** helper QR yang ada dipanggil. Bila artefaknya sudah ada: jangan ditimpa, jangan panggil helper yang dapat menimpa, laporkan status `CONFLICT`, QR lama tetap tidak tersentuh | **`APPROVED`** — keputusan baru |
| `PAT-OQ-006` | Gerbang staging **tidak boleh** memakai pola umum `!environment.IsProduction()` sebagai satu-satunya pemeriksaan. Wajib memakai daftar izin positif (*positive allowlist*) yang gagal tertutup. Environment yang diizinkan: `Staging` saja | **`APPROVED`** — keputusan baru |
| `PAT-OQ-007` | Membaca tabel `migration.rsmmc_*` tidak boleh memicu EF migration baru. Rinciannya pada bagian 4.4 | **`APPROVED`** — keputusan baru |

### 4.1 `PAT-OQ-001` — lokasi test dan governance guard

| Hal | Keputusan |
| --- | --- |
| Lokasi | `Tests/QuilvianSystemBackend.PatientManagementTests/` |
| Kerangka | Project xUnit **khusus** PatientManagement |
| Syarat | Dipakai hanya bila `quilvian-engineering-skills:build-module-backend` dan `rules/backend/TEST_POLICY.md` mengizinkannya **pada saat build** |
| Dilarang | Menaruh test PatientManagement di project `PharmacyTests`, `NutritionTests`, atau `OperatingRoomTests` hanya untuk menghindari kebijakan |
| Solution | Mengikuti konvensi repository saat ini. Ketiga project test yang ada **tidak** terdaftar di `QuilvianSystemBackend.sln`, sehingga project baru juga tidak didaftarkan |
| Bila kebijakan melarang | **Jangan di-*bypass*.** Hentikan bagian pembuatan test, laporkan aturan persis yang memblokir, laporkan alternatif lokasi/strategi test yang canonical, lalu minta keputusan sebelum mengubah arsitektur test |

**Penilaian perencanaan saat ini — bukan pengganti pemeriksaan saat build.**
`TEST_POLICY.md` bagian 2 melarang folder test, project test, xUnit, dan dependency test **"tanpa
permintaan eksplisit pemilik pada task aktif"**. Bagian 3 menghitung dua hal sebagai permintaan
eksplisit: instruksi langsung pada task aktif, dan acceptance criteria pada kartu task yang
disetujui pemilik sesudah kebijakan berlaku. `BE-PAT-MIG-001` memenuhi keduanya: `AC-27`
disetujui pada kartu task, dan `PAT-OQ-001` menyebut lokasi serta kerangkanya secara eksplisit.
Karena itu, menurut bacaan sekarang, project test ini **diizinkan**.

Yang tetap dilarang `TEST_POLICY.md` walau ada izin: entri project test pada solution (sejalan
dengan konvensi di atas) dan workflow CI test backend baru.

### 4.2 `PAT-OQ-002` dan `PAT-OQ-006` — gerbang environment

| Hal | Keputusan |
| --- | --- |
| Environment yang diizinkan | Hanya **`Staging`** |
| Berlaku untuk | `dryRun=true` **dan** `dryRun=false` |
| Login dan hak akses | Tetap wajib. Gerbang environment adalah lapisan tambahan, bukan pengganti |
| Cara memeriksa | Daftar izin positif yang gagal tertutup. `!environment.IsProduction()` tidak boleh menjadi satu-satunya pemeriksaan |
| Nama yang ditolak | Semua nama selain `Staging` persis: `Production`, `Development`, kosong, salah ketik, dan nama khusus yang tidak dikenal |
| Test | Test harness boleh mensimulasikan `Staging` tanpa mengubah aturan runtime pada kode produksi |

"Persis" berarti sama huruf demi huruf, termasuk huruf besar-kecilnya. Pemeriksaan bawaan
`IHostEnvironment.IsStaging()` membandingkan tanpa membedakan huruf besar-kecil, sehingga
pemeriksaan bawaan itu saja **tidak cukup** untuk memenuhi keputusan ini.

| Nama environment | Hasil | Alasan |
| --- | --- | --- |
| `Staging` | Diizinkan | Persis sama |
| `staging` | Ditolak | Huruf besar-kecil berbeda |
| `Stagging` | Ditolak | Salah ketik |
| `Production` | Ditolak | Produksi |
| `Development` | Ditolak | Bukan `Staging` |
| `Staging-Pilot` | Ditolak | Nama khusus yang tidak dikenal |
| kosong atau tidak diset | Ditolak | Gagal tertutup |

Bukti ejaan nama yang dipakai repository: `.github/workflows/generate-migration-artifact.yml`
baris 105 menyetel `ASPNETCORE_ENVIRONMENT: Staging`. Nama environment pada server staging yang
sedang berjalan **tidak** diperiksa, karena konfigurasi deployment berada di luar wewenang sesi
ini.

### 4.3 `PAT-OQ-004` — klasifikasi set Pilot dan `limit`

**Set Pilot** adalah seluruh baris yang lolos syarat 1 sampai 5 pada `AC-01`: batch cocok,
`FINALIZED`, crosswalk cocok, `quilvian_patient_id` terisi, dan `MstPatient.Id` cocok. Setiap
panggilan — simulasi maupun eksekusi — memeriksa status **seluruh** set Pilot.

| Status | Syarat | Dihitung dalam `limit`? | Diubah? |
| --- | --- | --- | --- |
| Layak direkonsiliasi | `MedicalRecordNumber` ≠ `final_medical_record_number` | **Ya** — hanya status ini | Ya, pada `dryRun=false` |
| `ALREADY_RECONCILED` | `MedicalRecordNumber` = `final_medical_record_number`, **dan** `QrCodePath` sesuai MRN kanonik | Tidak | Tidak |
| `QR_INCONSISTENT` | `MedicalRecordNumber` = `final_medical_record_number`, **tetapi** `QrCodePath` tidak sesuai MRN kanonik | Tidak | **Tidak** — wajib dilaporkan, tidak boleh diperbaiki diam-diam, dan bukan `ALREADY_RECONCILED` |

**Contoh.** Dari 715 pasien Pilot, misalkan 700 sudah kanonik dengan QR benar, 3 sudah kanonik
tetapi QR-nya menunjuk folder lama, dan 12 belum kanonik. Panggilan dengan `limit` 25 memproses
**12** pasien yang layak — bukan 25, karena hanya ada 12. Ringkasannya melaporkan 700
`ALREADY_RECONCILED` dan 3 `QR_INCONSISTENT`, beserta daftar ketiga pasien itu. Ketiga pasien
`QR_INCONSISTENT` tidak menghabiskan jatah `limit` dan tidak disentuh.

### 4.4 `PAT-OQ-007` — membaca tabel migrasi tanpa EF migration

`BE-PAT-MIG-001` **tidak** memberi wewenang untuk:

- membuat EF migration;
- menerapkan EF migration;
- mengubah skema database;
- mengubah data tabel migrasi secara langsung selama implementasi.

Mekanisme baca dipilih saat build, mengikuti kontrak engineering canonical dan infrastruktur yang
sudah ada, tanpa mengubah skema. Bukti mekanisme baca berparameter yang sudah dipakai source pada
snapshot `103b45cc`, dicatat sebagai bahan pertimbangan — **bukan** keputusan:

| Pola | Contoh di source |
| --- | --- |
| `Database.SqlQuery<T>` | `Areas/HealthServices/LaboratoryManagement/Services/LabOrderNumberService.cs` baris 152; `LabReportNumberService.cs` baris 195 |
| `FromSqlInterpolated` | `Areas/HealthServices/RegistrationManagement/Services/KioskPatientLookupService.cs` baris 179 |

Apa pun pilihannya, nilai masukan wajib dikirim sebagai parameter, bukan disambung ke teks SQL.
Buktinya bahwa model EF tidak berubah dicatat pada laporan task.

---

## 5. Catatan istilah — `LEGACY MIGRATION`

Kata `LEGACY MIGRATION` dipakai dengan **dua arti berbeda**, dan keduanya tidak boleh dicampur:

| Dipakai di | Artinya |
| --- | --- |
| Klasifikasi task ini (`PAT-DEC-003`) | Memindahkan **data pasien** lama RSMMC ke Quilvian |
| `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` baris 7, 18, 42–43 | Kampanye terbatas untuk **menormalkan nama** source dan tabel fisik warisan. `QBE-NAM-003`, `QBE-DB-001`, dan `QBE-DB-002` mengatur *rename* |

Task ini tidak me-*rename* source maupun tabel. Karena itu, kelas keberlakuan QBE untuk kode
barunya kemungkinan besar `NEW CODE` (berkas baru), atau `TOUCHED LEGACY` bila `PatientController`
ikut diubah. Penetapan finalnya dilakukan pada **QBE preflight saat build**, dari `AGENTS.md` dan
dokumen engineering canonical. Klasifikasi task tetap `LEGACY MIGRATION` sesuai keputusan
pengguna; catatan ini hanya mencegah aturan *rename* diterapkan pada pekerjaan yang bukan *rename*.
