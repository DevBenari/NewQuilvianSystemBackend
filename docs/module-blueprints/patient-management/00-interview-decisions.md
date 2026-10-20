# Patient Management — Keputusan yang Disetujui

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `revision` | **`6`** — 19 Oktober 2026: `PAT-DEC-069`–`077` — task prasyarat `BE-PAT-MIG-003A` (guard reservasi jalur pendaftaran bayi) dan amandemen desain `BE-PAT-MIG-002` hasil impact scan read-only (bagian 15). Riwayat revision `5`: 16 Oktober 2026: `PAT-DEC-068` — kunci allocator dua bagian tetap dan konfirmasi endpoint pemeriksa. Riwayat revision `4`: 16 Oktober 2026: amandemen pemilik sebelum build `BE-PAT-MIG-003` (`PAT-DEC-067`) — `Enabled = false` di Staging persis gagal tertutup dan endpoint pemeriksa `PAT-GATE-005`. Riwayat revision `3`: 14 Oktober 2026: amandemen untuk `BE-PAT-MIG-002`, bagian 6–14; diamandemen 15 Oktober 2026 dua kali dengan bukti Step 16 dan keputusan pemilik; **difinalkan dan disetujui 15 Oktober 2026, sesi ketiga** (`PAT-DEC-033`). Riwayat: revision `1` memuat `PAT-OQ-001`–`004` sebagai default `PROPOSED`; revision `2` (8 Oktober 2026) mencatat keputusan final review atas keempatnya dan menambah `PAT-OQ-005`–`007`. **Bagian 1–5 adalah isi revision `2` apa adanya dan tidak diubah** |
| Status | Bagian 15 (`BE-PAT-MIG-003A` dan amandemen `BE-PAT-MIG-002`): **`approved`** — 19 Oktober 2026, instruksi tertulis pemilik (`PAT-DEC-069`–`077`). Bagian 1–5 (`BE-PAT-MIG-001`): **`approved`** — seluruh keputusan final 8 Oktober 2026, tidak ada butir terbuka. Bagian 6–14 (`BE-PAT-MIG-002` dan task prasyarat `BE-PAT-MIG-003`): **`approved`** — 15 Oktober 2026, sesi ketiga: 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, 0 `BLOCKING`; `requirement-completeness-gate` `PASS` untuk kedua task (`requirement-completeness-assessment.md`); persetujuan desain `PAT-DEC-033`. *Riwayat: `draft` 14 Oktober sampai sesi kedua 15 Oktober 2026*. Revision `4` (`PAT-DEC-067`): **`approved`** 16 Oktober 2026. Revision `5` (`PAT-DEC-068`): **`approved`** 16 Oktober 2026 |
| Disetujui oleh | Bagian 1–5: pemilik/leader work item, lewat dua sesi `MODULE BLUEPRINT` 8 Oktober 2026 (akun Git `devbenari`). Bagian 6–14: pemilik (akun Git `devbenari`) — keputusan individual `PAT-DEC-034`–`066` dalam tiga sesi 15 Oktober 2026, dan persetujuan desain `PAT-DEC-033` lewat instruksi tertulis sesi ketiga yang menyetujui finalisasi bila gate kelengkapan lulus |
| Sumber | Bagian 1–5: keputusan tertulis pengguna 8 Oktober 2026, dicatat apa adanya (bukan hasil wawancara `grill-me`). Bagian 6–14: instruksi tertulis pemilik pada sesi `BACKEND PLANNING / GOVERNANCE ONLY` 14 Oktober 2026 dan sesi lanjutan 15 Oktober 2026 (bukti database Step 16), dicatat lewat `grill-me` *amendment pass*, ditambah impact scan source read-only |
| Cakupan | Empat work item — ditambah `BE-PAT-MIG-003A` (bagian 15, `PAT-DEC-069`). Semula tiga work item: `BE-PAT-MIG-001` (bagian 1–5), `BE-PAT-MIG-002` dan task prasyarat `BE-PAT-MIG-003` (`PAT-DEC-057`) (bagian 6–14) |

> **Catatan cakupan.** Berkas ini hanya memuat keputusan untuk dua pekerjaan migrasi data pasien
> RSMMC. Ia **bukan** blueprint lengkap modul Patient Management. Kemampuan pasien lain —
> pendaftaran, penggabungan, foto, dan seterusnya — tidak dibahas di sini dan tidak boleh
> disimpulkan dari berkas ini.

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

---

## 6. Amandemen revision `3` — `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`

> **Bagian 6 sampai 14 berstatus `approved`** sejak 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`).
> Persetujuan ini mengunci **desain** `BE-PAT-MIG-003` dan `BE-PAT-MIG-002`. Ia **tidak** memberi
> wewenang menulis source di luar sesi `TASK MODE: BACKEND` lewat `build-module-backend`, dan tidak
> memberi wewenang *commit*, deployment, eksekusi database, pemanggilan endpoint, pembuatan QR,
> atau menjalankan Canary-A — masing-masing butuh otorisasi terpisah (bagian 12).
>
> *Banner lama — jejak:* "Bagian 6 sampai 14 berstatus `draft`. Tidak ada satu butir pun di sini
> yang memberi wewenang menulis source, mengubah database, memanggil endpoint, atau membuat QR.
> `BE-PAT-MIG-002` baru boleh dibangun sesudah pemilik menyetujui keputusan `PROPOSED` pada bagian
> 10 beserta kartu task pada roadmap. Persetujuan itu dicatat sebagai `PAT-DEC-033`."

| Field | Nilai |
| --- | --- |
| Mode wawancara | `grill-me` *amendment pass*. Blueprint sudah `approved` untuk `BE-PAT-MIG-001`, sehingga keputusan baru ditambahkan sebagai revisi, bukan menimpa histori |
| Bentuk interaksi | Pemilik meminta seluruh keputusan terbuka disajikan sekaligus. Setiap pertanyaan pada bagian 10 tetap memuat pilihan, satu rekomendasi, dan `Other` |
| Snapshot source | `QuilvianIntegrationBackend` @ `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923`. `git fetch` tidak dijalankan karena izinnya ditolak pada sesi ini, sehingga "terbaru" berarti ref `origin/QuilvianIntegrationBackend` lokal — yang sama persis dengan HEAD. Amandemen 15 Oktober 2026: HEAD backend masih `a52b3c98`; frontend `QuilvianSystemFrontendDev` branch `fix/final-dev-tree-alignment` @ `2831ea635d83c0eca3f2cfbb0ae3be383e3ce204` dibaca read-only untuk konsumen QR (SHA dibaca dari berkas `.git` karena perintah Git ditolak oleh pemeriksaan kepemilikan folder; konfigurasi global tidak diubah) |
| Capability map | `01-existing-capability-map.md` belum ada (`PAT-GAP-001`). Impact scan source untuk area migrasi Patient dicatat pada bagian 9, sehingga kemungkinan duplikasi kemampuan **sudah** diperiksa untuk area ini |
| Bentuk blueprint | Tetap `SINGLE` (`PAT-DEC-002`, `shape_decided_by: USER`). Uji pemecahan: hanya ada satu rumpun kemampuan — migrasi data pasien RSMMC — dengan satu pemilik dan satu kosakata status. Tidak ada rumpun kedua |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN`. Pekerjaan ini memindahkan data ke aggregate `MstPatient` yang sudah ada di modul pemiliknya, tanpa konsep domain, status, atau relasi baru. Dampaknya ke modul lain hanya berupa bertambahnya pasien yang dapat dicari. Pemilik boleh meminta `hospital-domain-architect` bila menilai perlu |
| Kesiapan requirement | Per sesi ketiga 15 Oktober 2026: 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, **0 `BLOCKING`**, 0 `PROPOSED` (bagian 10.1). Hasil `requirement-completeness-gate`: `requirement-completeness-assessment.md` |

### 6.1 Batas scope

**Dalam satu kalimat:** `BE-PAT-MIG-002` adalah mekanisme admin khusus Staging yang membuat
`MstPatient` baru bagi pasien RSMMC kanonik yang belum ada di Quilvian, langsung dari target
preview yang sudah diaudit, dengan cara yang aman diulang dan dapat dilanjutkan dari titik
terakhir — dimulai dari Canary-A, yang manifest eksekusinya dibekukan ulang sesudah strategi QR dan
reservation guard disetujui.

| Di dalam scope | Di luar scope |
| --- | --- |
| Endpoint admin baru yang terpisah dari endpoint `BE-PAT-MIG-001` | Mengubah atau memakai ulang `POST …/admin/migration/rsmmc-pilot-mrn/reconcile` |
| Membuat `MstPatient` dari `migration.rsmmc_patient_target_preview` batch kanonik | Membangkitkan `PatientCode` atau MRN baru |
| Mencatat pemetaan baru pada `migration.rsmmc_patient_crosswalk` | Mengubah baris crosswalk historis: Pilot 715 dan 285 bukti review |
| Membuat QR untuk MRN kanonik pasien baru, dengan strategi yang tidak bertabrakan dengan 715 QR historis Pilot (`PAT-OQ-028`) | Menghapus, memindah, menimpa, atau diam-diam mengadopsi QR mana pun yang sudah ada |
| **Strategi** perlindungan reservasi `PatientCode` dan MRN yang tahan lama (`PAT-DEC-042`) — diimplementasikan di task prasyarat terpisah `BE-PAT-MIG-003` (`PAT-DEC-057`) | Membangkitkan MRN atau `PatientCode` oleh worker |
| Canary-A: manifest eksekusi `RSMMC-CANARY-A` v1 — 25 kandidat pada bagian 11 dalam urutan yang sama, sesudah syarat `PAT-DEC-056` terpenuhi | Memilih atau menjalankan Canary-B; gelombang yang lebih besar dari canary |
| Gerbang runtime terpisah `PAT-GATE-002` | Menjalankan apa pun pada sesi perencanaan ini |
| Environment `Staging` saja | Produksi, dalam bentuk apa pun |

**Di luar scope — untuk keputusan atau modul lain.**

| Butir | Alasan | Pemilik |
| --- | --- | --- |
| Pensiun atau penonaktifan endpoint `BE-PAT-MIG-001` sesudah Pilot ditutup | Kontrak `1.1.0` bagian 1 menyebutnya belum diputuskan, dan tidak memengaruhi `BE-PAT-MIG-002` | Pemilik PatientManagement |
| ~~Pembangkit `PatientCode` dan MRN normal memakai pola "nomor kosong terkecil" (`PAT-SRC-006`) — di sini hanya dijaga lewat gerbang deteksi (`PAT-OQ-020`)~~ | **Dikoreksi 15 Oktober 2026:** tabrakan terbukti (`PAT-FACT-072`, `073`), sehingga strategi perlindungannya kini **di dalam** scope desain (`PAT-DEC-042`, `PAT-OQ-030`–`032`); task tempat implementasinya `PAT-OQ-035` | Pemilik PatientManagement |
| `IsMember = false` pada pendaftaran normal tersimpan sebagai `true` (`PAT-SRC-008`, `PAT-SRC-024`). Teks lama juga menyebut `IsActive = false`; pendaftaran normal selalu mengisi `IsActive = true`, sehingga bagian itu tidak relevan (log koreksi bagian 14) | Cacat yang sama ada pada alur pendaftaran normal, bukan bagian migrasi. Untuk worker, lihat `PAT-OQ-029` | Pemilik PatientManagement |
| Deteksi pasien ganda, penggabungan pasien, membership, cetak ulang kartu | Kemampuan pasien lain | Modul masing-masing |
| Layar frontend | Mekanisme ini endpoint admin tanpa layar (`PAT-OQ-025`) | — |

### 6.2 Latar belakang dalam bahasa sederhana

Pilot sudah selesai: 715 pasien Pilot kini memakai MRN kanonik (`PAT-GATE-001` dilaporkan
pemilik `CLOSED/PASS`). Sisa pasien RSMMC — 704.506 orang yang siap — belum ada di Quilvian.
Data mereka sudah disiapkan dan diaudit pada satu tabel bernama **target preview**: setiap baris
sudah berisi `PatientCode`, MRN, nama, tanggal lahir, dan data lain yang disetujui.

Tugas `BE-PAT-MIG-002` adalah **menyalin** setiap baris preview menjadi pasien Quilvian, satu per
satu, tanpa mengarang isi apa pun dan tanpa membuat nomor baru.

**Contoh dengan data samaran.** Baris preview untuk `legacy_pid` 900001 berisi `PatientCode`
`PAT-RSMMC-12345`, MRN `00-12-34-56`, dan nama "Siti Contoh".

1. Worker memastikan MRN `00-12-34-56` sama dengan MRN rencana kanonik untuk `legacy_pid` 900001
   dan belum dipakai pasien lain.
2. Worker membuat pasien baru dengan kode dan MRN itu, lalu QR di
   `patient-qrcodes/00-12-34-56/qrcode.png`.
3. Worker mencatat pada crosswalk: `legacy_pid` 900001 → `Id` pasien baru.
4. Bila admin tidak sengaja menjalankannya ulang, worker membaca crosswalk, menemukan
   pemetaannya, lalu melaporkan `ALREADY_MIGRATED`. Tidak ada pasien kedua.

Kata **resumable** (dapat dilanjutkan) berarti: bila proses berhenti di tengah — misalnya server
mati sesudah pasien ke-10 — panggilan berikutnya tidak mulai dari nol. Pasien 1–10 dikenali
sudah selesai lewat crosswalk, lalu proses lanjut dari pasien ke-11.

---

## 7. Fakta yang dilaporkan pemilik — revision `3`

> **Penting.** Seperti bagian 3, angka di bawah dilaporkan pemilik dari audit database yang sudah
> dilakukan sebelumnya, dan dinyatakan **otoritatif** oleh pemilik. Agent **tidak mengakses
> database** pada sesi ini dan **tidak memverifikasi ulang** angka-angka ini.

### 7.1 Lingkungan dan populasi

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-030` | Database Staging | `QuilvianNewSta`. Produksi di luar cakupan dan tidak boleh disentuh |
| `PAT-FACT-031` | Batch kanonik | `45eeefba-dd39-6831-1bd2-d986851f7c1f` — sama dengan `PAT-DEC-004` |
| `PAT-FACT-032` | Batch Pilot historis | `2ce3929b-b5af-430b-a2b4-bdb3aaf6908f` |
| `PAT-FACT-033` | Sumber kanonik | 705.223 |
| `PAT-FACT-034` | Pilot | 715 — rekonsiliasi Pilot `CLOSED/PASS` |
| `PAT-FACT-035` | Bulk kanonik di luar Pilot | 704.508 |
| `PAT-FACT-036` | Hasil normalisasi | `READY_FOR_TARGET_PREVIEW` 704.506; `REVIEW_SCOPE` 2 |
| `PAT-FACT-037` | `legacy_pid` yang ditunda | `280143` dan `805596` |
| `PAT-FACT-038` | Jumlah `MstPatient` sekarang | 741 |

Pemeriksaan silang: 704.506 + 2 = 704.508, dan 704.508 + 715 = 705.223 — sama dengan
`PAT-FACT-001`. Angkanya cocok.

### 7.2 Target preview — `migration.rsmmc_patient_target_preview`

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-040` | Baris batch kanonik | 704.506 |
| `PAT-FACT-041` | `legacy_pid` unik / `PatientCode` unik / MRN akhir unik | 704.506 / 704.506 / 704.506 |
| `PAT-FACT-042` | Rentang angka `PatientCode` | 742 sampai 705.249 |
| `PAT-FACT-043` | Kode cadangan pasien tertunda — **tidak** dipakai | `280143` → `PAT-RSMMC-101416`; `805596` → `PAT-RSMMC-619077` |
| `PAT-FACT-044` | Tabrakan ke `MstPatient`: MRN / `PatientCode` / identitas / telepon | 0 / 0 / 0 / 0 |
| `PAT-FACT-045` | Identitas aman (baris / unik) | 192.278 / 192.278 |
| `PAT-FACT-046` | Telepon aman (baris / unik) | 98.612 / 98.612 |
| `PAT-FACT-047` | Kelebihan panjang kolom | 0 |
| `PAT-FACT-048` | `validation_status` | `PENDING` = 704.506 |
| `PAT-FACT-049` | Preview Pilot | Tetap 715 |

Pemeriksaan silang `PAT-FACT-042`: rentang 742–705.249 berisi 705.249 − 742 + 1 = 704.508 angka.
704.506 dipakai preview, ditambah 2 kode cadangan `PAT-FACT-043`, sama dengan 704.508. Rentangnya
utuh tanpa lubang lain.

Kolom preview yang memuat nilai yang sudah disetujui: `proposed_patient_code`,
`proposed_medical_record_number`, `patient_type`, `patient_status`, `registration_source`,
`full_name`, `birth_date`, `gender`, `religion`, `marital_status`, `blood_type`, `identity_type`,
`identity_number`, `phone_number`, `address`, `is_member`, `is_newborn`, `is_deceased`,
`is_active`.

### 7.3 Crosswalk — `migration.rsmmc_patient_crosswalk`

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-050` | Kunci utama | `legacy_pid bigint NOT NULL` |
| `PAT-FACT-051` | Kolom penting | `canonical_legacy_pid bigint NULL`, `legacy_old_rm text NULL`, `normalized_mrn text NULL`, `normalized_nik text NULL`, `quilvian_patient_id uuid NULL`, `match_status varchar NOT NULL` bawaan `UNPROCESSED`, `migration_status varchar NOT NULL` bawaan `RAW`, `review_reason text NULL`, `last_batch_id uuid NULL`, `created_at timestamptz NOT NULL`, `updated_at timestamptz NULL` |
| `PAT-FACT-052` | Foreign key | `last_batch_id` → `migration.rsmmc_patient_batch(batch_id)` |
| `PAT-FACT-053` | Indeks | Pada `normalized_mrn`, `normalized_nik`, dan `quilvian_patient_id` — ketiganya **tidak unik** |
| `PAT-FACT-054` | Keadaan crosswalk bulk kanonik | Total 704.508; punya baris 285; tanpa baris 704.223; sudah punya target 0; punya baris tanpa target 285 |
| `PAT-FACT-055` | Sebaran 285 baris | 16 `DUPLICATE_CANDIDATE`/`REVIEW_DUPLICATE`; 4 `MERGE_CANDIDATE`/`REVIEW_MERGE`; 2 `REVIEW`/`REVIEW_NEWBORN`; 263 `REVIEW`/`REVIEW_SCOPE` |
| `PAT-FACT-056` | Sifat 285 baris | Bukti review historis latihan Pilot-1000. Seluruhnya punya `review_reason`, `quilvian_patient_id` NULL, termasuk `READY_FOR_TARGET_PREVIEW`, dan `last_batch_id` menunjuk batch Pilot historis. **Tidak boleh dihancurkan** |

Pemeriksaan silang: 285 + 704.223 = 704.508, dan 16 + 4 + 2 + 263 = 285. Angkanya cocok.

**Turunan yang perlu dikonfirmasi — bukan fakta pemilik.** Bila kedua `legacy_pid` tertunda tidak
punya baris crosswalk, 704.506 baris preview terbagi menjadi 704.221 tanpa crosswalk dan 285
dengan crosswalk tanpa target. Angka ini dihitung agent; `PAT-OQ-019` meminta konfirmasinya.

### 7.4 Semantik crosswalk Pilot yang berhasil

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-060` | Pilot: crosswalk ada / punya target / pasien target ada | 715 / 715 / 715 |
| `PAT-FACT-061` | Keutuhan 1:1 | 715 pemetaan target, 715 `quilvian_patient_id` unik, grup target ganda 0 |
| `PAT-FACT-062` | `match_status` sukses | `TARGET_CREATED` |
| `PAT-FACT-063` | `migration_status` historis sukses | 710 `MIGRATED_API_AUTO`; 5 `MIGRATED_API_PILOT` |
| `PAT-FACT-064` | `review_reason` pemetaan sukses | Kosong |

### 7.5 Bukti database Step 16 — dilaporkan pemilik 15 Oktober 2026

Database `QuilvianNewSta` / Staging; batch kanonik `45eeefba-dd39-6831-1bd2-d986851f7c1f`; batch
Pilot `2ce3929b-b5af-430b-a2b4-bdb3aaf6908f`. Agent tidak mengakses database dan tidak
memverifikasi ulang angka ini.

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-070` | Tabrakan namespace QR: baris preview kanonik `READY` / baris yang `proposed_medical_record_number`-nya sama dengan MRN `target_preview` Pilot historis / MRN Pilot historis berbeda yang bertabrakan | 704.506 / **715** / **715** — **seluruh** 715 MRN sementara Pilot kini dibutuhkan tepat 715 pasien bulk kanonik |
| `PAT-FACT-071` | Kandidat Canary-A yang bertabrakan, `legacy_pid` bulk → pemilik Pilot historis | -667995 → 797015; -667992 → 797021; -667991 → 797022; -667986 → 797033; -667984 → 797037; 29 → 797017; 98 → 797145; 169 → 797266; 317 → 817686; 620 → 892037. Jumlah 10; 15 sisanya bersih |
| `PAT-FACT-072` | Pilihan pembangkit `PatientCode` normal saat ini | Angka 742 → `PAT-RSMMC-00742`; bertabrakan dengan reservasi bulk: **TRUE** |
| `PAT-FACT-073` | Pilihan pembangkit MRN normal saat ini | Angka 27 → `00-00-00-27`; bertabrakan dengan reservasi bulk: **TRUE** |
| `PAT-FACT-074` | Nilai boolean seluruh preview kanonik | `IsMember = false` dan `IsActive = true` pada **704.506** baris |
| `PAT-FACT-075` | Default kolom `public."MstPatient"` saat ini | `IsMember boolean NOT NULL DEFAULT true`; `IsActive boolean NOT NULL DEFAULT true` |

**Contoh dampak `PAT-FACT-070`.** Pasien Pilot `797015` dulu memakai MRN sementara buatan
Quilvian; QR-nya tersimpan di folder MRN itu dan wajib dibiarkan utuh. Rencana kanonik kini
memberikan MRN yang sama kepada pasien bulk `-667995`. Bila worker membuat QR di folder bawaan
MRN itu, berkasnya sudah ada. Menimpanya merusak bukti Pilot; menolak dengan `CONFLICT` membuat
`-667995` tidak pernah dapat dimigrasi. Hal yang sama berlaku untuk 714 pasien lain.

**Inferensi `PAT-INF-001` — belum diverifikasi.** 741 pasien saat ini = 26 non-Pilot + 715 Pilot.
Pembangkit "nomor kosong terkecil" memilih MRN 27 dan `PatientCode` 742. Keduanya cocok dengan
keadaan berikut: 26 pasien non-Pilot memakai MRN 1–26; Pilot V1 dulu memakai MRN sementara 27–741
(715 nomor) dan `PatientCode` sampai 741. Bila benar, `00-00-00-27` adalah MRN sementara Pilot,
sehingga pendaftaran pasien normal berikutnya di Staging — yang menulis QR dengan
`File.WriteAllBytes` (`PAT-SRC-016`) — akan **menimpa** QR historis Pilot di folder itu. Pemilik
diminta mengonfirmasi secara read-only (`PAT-OQ-031`).

### 7.6 Register isu desain P0

| ID | Isu | Bukti | Pertanyaan yang harus diputuskan | Status |
| --- | --- | --- | --- | --- |
| `PAT-P0-001` | 715 tabrakan namespace QR historis | `PAT-FACT-070`, `071` | `PAT-OQ-028`, `033`, `034` | **Diputuskan** — `PAT-DEC-052`, `056`, `062`. *Riwayat: terbuka sampai amandemen pertama* |
| `PAT-P0-002` | Pembuatan pasien normal memakai reservasi `PatientCode` | `PAT-FACT-072`; `PAT-SRC-006` | `PAT-OQ-030`, `032`, `035` | **Diputuskan** — `PAT-DEC-054`, `055`, `057`, `066`. *Riwayat: terbuka sampai amandemen pertama* |
| `PAT-P0-003` | Pembuatan pasien normal memakai reservasi MRN — dan menurut `PAT-INF-001` dapat menimpa QR historis Pilot | `PAT-FACT-073`; `PAT-SRC-006`, `016` | `PAT-OQ-031`, `032`, `035` | **Diputuskan** — `PAT-DEC-064`, `066`; `PAT-INF-001` terkonfirmasi (`PAT-FACT-089`). *Riwayat: terbuka sampai amandemen kedua* |

Blocker yang tidak diberi label P0 oleh pemilik tetapi mengenai **100%** baris: `IsMember = false`
harus tersimpan `false` (`PAT-DEC-041`, `PAT-OQ-029`).

### 7.7 Pernyataan pemilik pada amandemen kedua 15 Oktober 2026

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-076` | Kolom preview kanonik yang menjadi sumber crosswalk baru | `legacy_old_rm`, `legacy_normalized_mrn`, `legacy_normalized_nik` — dinyatakan pemilik lewat `PAT-DEC-047`. Tipe dan isinya dikonfirmasi lewat paket bukti `PAT-OQ-019` |
| `PAT-FACT-077` | Klasifikasi telepon preview | Telepon preview berklasifikasi `SAFE_TO_STORE` (`PAT-DEC-050`). Letak kolom klasifikasinya dikonfirmasi lewat `PAT-OQ-019` |
| `PAT-FACT-078` | Pembekuan pendaftaran pasien normal di Staging | Dinyatakan pemilik **berlaku sejak 15 Oktober 2026**, termasuk jalur admin dan kiosk, sampai guard reservasi dideploy dan diverifikasi (`PAT-DEC-055`). Produksi tidak terdampak. Agent tidak memverifikasi keadaan ini; buktinya diminta sebelum runtime Canary-A (`PAT-GATE-002`) dan wajib menunjukkan **ketiga** jalur — `POST /`, `POST /kiosk`, `POST /admin` — tertutup, karena pencabutan izin `Patient : Create` saja hanya menutup jalur admin (`PAT-SRC-027`) |

**Turunan agent — bukan fakta pemilik.** Ke-25 kandidat Canary-A berada di rentang -667.995 sampai
797.554, sehingga seluruhnya muat sebagai bilangan bulat bertanda 32-bit (Int32,
-2.147.483.648 sampai 2.147.483.647). Ini cukup untuk kunci dua-bagian `PAT-DEC-048` pada
Canary-A; rentang seluruh preview diminta lewat `PAT-OQ-019`.

### 7.8 Bukti final pemilik — sesi ketiga 15 Oktober 2026

Database `QuilvianNewSta`, PostgreSQL 15.15. Seperti bagian 3 dan 7.1–7.7, angka ini dilaporkan
pemilik dan dinyatakan otoritatif; agent tidak mengakses database.

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-079` | Skema `migration.rsmmc_patient_crosswalk` | `legacy_pid bigint NOT NULL` (primary key); `canonical_legacy_pid bigint NULL`; `legacy_old_rm`, `normalized_mrn`, `normalized_nik` `text NULL`; `quilvian_patient_id uuid NULL`; `match_status varchar(30) NOT NULL DEFAULT 'UNPROCESSED'`; `migration_status varchar(30) NOT NULL DEFAULT 'RAW'`; `review_reason text NULL`; `last_batch_id uuid NULL`, foreign key ke `migration.rsmmc_patient_batch(batch_id)`; `created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP`; `updated_at timestamptz NULL` |
| `PAT-FACT-080` | Nilai status yang disetujui muat tanpa perubahan skema | `TARGET_CREATED` 14 karakter; `MIGRATED_BULK_WORKER` 20 karakter; keduanya muat di `varchar(30)` |
| `PAT-FACT-081` | Trigger kustom | Tidak ada pada `rsmmc_patient_batch`, `rsmmc_patient_crosswalk`, `rsmmc_patient_mrn_plan`, dan `rsmmc_patient_target_preview` |
| `PAT-FACT-082` | Skema `migration.rsmmc_patient_mrn_plan` | `batch_id uuid NOT NULL`; `legacy_pid bigint NOT NULL`; `final_medical_record_number varchar(50)`; `target_collision boolean NOT NULL DEFAULT false`; `target_collision_patient_id uuid NULL`; `plan_status varchar(30) NOT NULL DEFAULT 'PENDING'`; `created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP`; `updated_at timestamptz NULL`; primary key `(batch_id, legacy_pid)` |
| `PAT-FACT-083` | Skema `migration.rsmmc_patient_target_preview` — kolom yang dilaporkan | `batch_id uuid NOT NULL`; `legacy_pid bigint NOT NULL`; `source_group varchar(40) NOT NULL`; `legacy_old_rm`, `legacy_normalized_mrn`, `legacy_normalized_nik` `text`; `proposed_patient_code text NOT NULL`; `proposed_medical_record_number text NOT NULL`; `patient_type`, `patient_status`, `registration_source`, `gender`, `religion`, `marital_status`, `blood_type` `integer`; `identity_type`, `identity_number`, `phone_number`, `address` `text`; `is_member`, `is_newborn`, `is_deceased`, `is_active` `boolean`; `validation_status varchar(40) DEFAULT 'PENDING'`; `validation_reason text`; `created_at timestamptz DEFAULT CURRENT_TIMESTAMP`; primary key `(batch_id, legacy_pid)`. Sebaran kolom enum terkonfirmasi angka/integer |
| `PAT-FACT-084` | Bukti perencana MRN | Baris `FINALIZED` 705.223; `legacy_pid` unik 705.223; MRN akhir unik 705.223; format tidak sah 0; MRN numerik minimum 27; maksimum 890.187; sisa kapasitas 8 digit 99.109.812; jumlah pemilik akhir MRN numerik 27 = 1 |
| `PAT-FACT-085` | Versi PostgreSQL Staging | 15.15 |
| `PAT-FACT-086` | Rentang `legacy_pid` kanonik | Minimum -667.995; maksimum 892.270; di luar Int32 = 0 |
| `PAT-FACT-087` | Keutuhan kandidat Canary-A | 25 baris; 25 `legacy_pid` unik; 25 ada di preview; 25 `PENDING`; 25 tanpa crosswalk. Urutan manifest tetap berwenang dan tidak disort ulang |
| `PAT-FACT-088` | Pasien tertunda | `280143` dan `805596` tidak punya baris crosswalk; tetap ditunda; kode dan MRN cadangannya tetap dilindungi; tanpa nama pengganti dan tanpa migrasi otomatis |
| `PAT-FACT-089` | Riwayat MRN `00-00-00-27` | Pemilik kanonik: `legacy_pid` -667995, `PAT-RSMMC-00742`, preview `PENDING`, tanpa crosswalk. Pemilik historis Pilot: `legacy_pid` 797015, `PAT-RSMMC-00027`, MRN sementara `00-00-00-27`, MRN sekarang `00-79-70-15`. Pemilik `MstPatient` sekarang untuk `00-00-00-27`: tidak ada. Bukti cadangan immutable ada |

**Pemeriksaan silang.**
- 99.999.999 − 890.187 = 99.109.812, sama dengan sisa kapasitas yang dilaporkan.
- `PAT-FACT-089` cocok dengan pasangan tabrakan `-667995 → 797015` pada `PAT-FACT-071`.
- Kode cadangan pertama (`PAT-RSMMC-00742`) dan MRN cadangan yang akan dipilih pembangkit normal
  (`00-00-00-27`) ternyata milik **pasien yang sama**, yaitu kandidat Canary-A nomor 5.
- Turunan bagian 7.3 terkonfirmasi: kedua pasien tertunda tidak punya crosswalk, sehingga 704.506
  baris preview = 704.221 tanpa crosswalk + 285 dengan crosswalk tanpa target.

**Status inferensi `PAT-INF-001`: terkonfirmasi** oleh `PAT-FACT-089`. `00-00-00-27` memang MRN
sementara Pilot milik 797015, dan folder QR historisnya wajib tetap utuh (`PAT-DEC-039`).

**Catatan agent — bukan blocker; penilaiannya ada di `requirement-completeness-assessment.md`.**
1. Tipe kolom `full_name` dan `birth_date` tidak tercantum pada ringkasan `PAT-FACT-083`. Kontrak
   memakai `text` dan `date`, dan gerbang runtime `PREVIEW_SCHEMA_SIGNATURE` menolak eksekusi
   — sebelum mutasi apa pun — bila tipe sebenarnya berbeda.
2. Kolom perencana `target_collision` dan `target_collision_patient_id` tidak dipakai worker sebagai
   syarat. Tabrakan sebenarnya diperiksa langsung ke `MstPatient` (`PAT-MIG2-AC-07`, `08`).
3. Letak status normalisasi (`READY_FOR_TARGET_PREVIEW`) tidak dibutuhkan desain final, karena
   gerbang `DEFERRED_SET` dihitung dari perencana, preview, dan Pilot.

---

## 8. Keputusan pemilik yang dikunci — sesi 14 dan 15 Oktober 2026 (tiga sesi)

Butir di bawah dinyatakan pemilik sebagai **batasan wajib** (MUST / MUST NOT) pada instruksi
tertulisnya. Statusnya `approved` sebagai batasan, tetapi **bukan** persetujuan desain
`BE-PAT-MIG-002` secara utuh — persetujuan desain adalah `PAT-DEC-033`.

| ID | Keputusan | Status |
| --- | --- | --- |
| `PAT-DEC-017` | Task baru **`BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient Migration Worker**. Area `HealthServices`, Module `PatientManagement`, Submodule `MasterData`, prefix `Pat`. Klasifikasi bisnis `LEGACY MIGRATION` dalam arti migrasi data pasien (bagian 5) | `approved` |
| `PAT-DEC-018` | Approval revision `2` hanya mencakup `BE-PAT-MIG-001`. `BE-PAT-MIG-002` **tidak boleh diimplementasikan** sebelum requirement, keputusan, kontrak API, kartu roadmap, dan acceptance criteria-nya ditambahkan dan disetujui | `approved` |
| `PAT-DEC-019` | Batas keselamatan: hanya Staging; produksi tidak disentuh; Pilot 715 tidak diubah atau dihapus; ekstraksi mentah tidak diulang; perencana MRN tidak dijalankan ulang; `target_preview` tidak dibuat ulang; `migration.rsmmc_patient_pilot_reconcile_backup` tidak diubah; crosswalk Pilot historis tidak diubah; tidak ada QR dibuat pada fase perencanaan; tidak ada EF migration kecuali desain yang disetujui membuktikan perlu | `approved` |
| `PAT-DEC-020` | `target_preview` batch kanonik adalah **snapshot masukan yang disetujui** untuk kolom bagian 7.2. `BE-PAT-MIG-002` tidak boleh membangkitkan ulang `PatientCode`, tidak boleh memanggil pembangkit `PatientCode` normal, dan tidak boleh membangkitkan MRN baru | `approved` |
| `PAT-DEC-021` | MRN otoritatif = `migration.rsmmc_patient_mrn_plan.final_medical_record_number` dengan `plan_status = FINALIZED`. MRN usulan preview wajib divalidasi ulang terhadap perencana sebelum mutasi. `target_preview` Pilot historis **bukan** sumber identifier otoritatif | `approved` |
| `PAT-DEC-022` | Identitas utama migrasi adalah `legacy_pid`. Pasien **tidak pernah** digabung atau dikenali hanya dari NIK. Model lanjut: `legacy_pid` → crosswalk → `quilvian_patient_id` → `MstPatient.Id`. Karena `quilvian_patient_id` tidak unik di database, service yang wajib menjaga invariant dan konkurensinya | `approved` |
| `PAT-DEC-023` | Konkurensi: satu transaksi per pasien; serialisasi tingkat database yang dikunci secara deterministik per batch kanonik + `legacy_pid` — misalnya PostgreSQL *transaction advisory lock* — atau mekanisme setara yang sama durabelnya; baca ulang preview dan crosswalk **sesudah** kunci didapat; validasi ulang perencana; validasi ulang kepemilikan `PatientCode` dan MRN di `MstPatient`; tidak boleh hanya mengandalkan kunci di memori proses; tidak boleh Count+1 / Max+1; percobaan ulang tidak pernah membuat pasien kedua. Percobaan ulang sesudah *commit* sukses menghasilkan `ALREADY_MIGRATED` atau hasil aman yang setara | `approved` |
| `PAT-DEC-024` | Keselamatan QR mengikuti semantik `BE-PAT-MIG-001`: periksa path tujuan sebelum menulis; tidak menimpa; `CreateNew` atau setara; tahu apakah pemanggilan ini yang membuat berkas; pembersihan hanya bila kepemilikan terbukti dengan penanda pemanggilan + panjang berkas + SHA-256; *commit* tidak pasti tidak pernah menghapus QR otomatis; QR Pilot lama tidak pernah dihapus | `approved` |
| `PAT-DEC-025` | `MstPatient` dibuat langsung dari preview kanonik oleh service modul, **bukan** lewat API pembuatan normal yang membangkitkan ulang kode atau MRN. Controller tidak mengakses `DbContext`. Field audit aktor diisi (`CreateBy`, `CreateDateTime`). Tidak ada field pasien yang dikarang; field di luar preview hanya memakai default model yang terbukti aman, selain itu menjadi pertanyaan terbuka | `approved` |
| `PAT-DEC-026` | **Canary-A**: 25 `legacy_pid` dibekukan (bagian 11). Seluruhnya wajib: berada di preview kanonik; `validation_status = PENDING`; **tidak** punya baris crosswalk saat preflight eksekusi; tetap bebas tabrakan; perencana `FINALIZED`; MRN usulan preview sama dengan MRN akhir perencana. Pasien yang gagal preflight **tidak boleh** diganti otomatis dengan pasien lain; perubahan keadaan diklasifikasikan atau ditolak secara eksplisit. Canary-A **tidak** dijalankan pada task perencanaan ini | `approved` — status daftarnya **digantikan sebagian** oleh `PAT-DEC-040` (15 Oktober 2026): daftar 25 kini kandidat draft; syarat kelayakannya tetap berlaku |
| `PAT-DEC-027` | **Canary-B**: 285 baris crosswalk historis **bukan** bagian Canary-A. Canary-B dirancang terpisah **sesudah** Canary-A lulus runtime, untuk jalur "crosswalk ada + `quilvian_patient_id` NULL", dengan mempertahankan `match_status`, `migration_status`, dan `review_reason` historis kecuali ada kebijakan transisi yang disetujui pemilik. Tidak dipilih dan tidak dijalankan sekarang | `approved` |
| `PAT-DEC-028` | Endpoint admin **baru dan terpisah** di bawah route migrasi admin Patient yang sudah ada; endpoint `BE-PAT-MIG-001` tidak diperluas atau dialihfungsikan. Syarat minimum: gerbang `Staging` persis; izin `Patient : Update` kecuali governance membuktikan izin lain; `dryRun` bawaan `true`; `batchId` kanonik eksplisit; mode pemilihan canary/daftar disetujui yang eksplisit; `limit` maksimum yang dibatasi; eksekusi butuh aktor dikenali; wewenang implementasi source dan otorisasi runtime terpisah; respons berisi hasil per pasien dan ringkasan; perilaku berhenti diputuskan eksplisit; DTO tanpa entity EF; `dryRun` tanpa mutasi database dan tanpa tulis filesystem. Route dan bentuk DTO **tidak** difinalkan lewat asumsi — lihat `PAT-OQ-010` | `approved` |
| `PAT-DEC-029` | Gerbang runtime baru dan terpisah `PAT-GATE-002`. Persetujuan implementasi source **tidak** memberi wewenang eksekusi runtime. Urutan 12 langkah pada bagian 12. Tidak ada eksekusi 704 ribu pasien secara otomatis | `approved` |
| `PAT-DEC-030` | `legacy_pid` `280143` dan `805596` tetap ditunda dan tidak pernah diproses. Nama pengganti (*placeholder*) tidak pernah dikarang untuk nama yang kosong | `approved` |
| `PAT-DEC-031` | Bukti historis 285 baris crosswalk tidak boleh dihancurkan | `approved` |
| `PAT-DEC-032` | `MIGRATED_API_PILOT` tidak boleh dipakai ulang begitu saja untuk worker bulk. Nilai `migration_status` baru wajib menjadi keputusan pemilik yang eksplisit — lihat `PAT-OQ-008` | `approved` |
| `PAT-DEC-033` | **Persetujuan desain `BE-PAT-MIG-002` dan task prasyarat `BE-PAT-MIG-003`**: keputusan bagian 10, kontrak `contracts/api-contract.md` `1.2.0` Bagian B (termasuk B14 dan B15), serta kartu task dan acceptance criteria kedua task pada roadmap — termasuk usulan standar agent `PAT-GAP-002`–`016` yang tercatat terbuka di `requirement-completeness-assessment.md`. Diberikan lewat instruksi tertulis pemilik sesi ketiga: finalkan blueprint, kontrak, dan kartu **bila** `requirement-completeness-gate` lulus; gate lulus untuk kedua task. Persetujuan ini **tidak** memberi wewenang *commit*, deployment, eksekusi database, pemanggilan endpoint, pembuatan QR, atau runtime Canary-A. *Riwayat: `draft` — "WAITING FOR FINAL READ-ONLY EVIDENCE" sampai sesi kedua* | **`approved`** — pemilik, 15 Oktober 2026 (sesi ketiga) |
| `PAT-DEC-034` | Provenance sukses `BE-PAT-MIG-002`: `migration_status = MIGRATED_BULK_WORKER`. Menjawab `PAT-OQ-008`; nilai `match_status` dipisah ke `PAT-OQ-027` | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-035` | `migration.rsmmc_patient_target_preview` kanonik tetap **immutable** sesudah approval. `validation_status` tidak dipakai sebagai penanda kemajuan worker; kemajuan dan resume dibaca dari crosswalk + `MstPatient` + hasil worker. Menjawab `PAT-OQ-009` | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-036` | Kebijakan eksekusi Canary-A: **berhenti** pada `CONFLICT` atau `FAILED` tak terduga yang pertama. Hasil sudah-dimigrasi/idempoten bukan kegagalan tak terduga. Menjawab `PAT-OQ-011` untuk eksekusi | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-037` | Automated test **wajib** untuk `BE-PAT-MIG-002`, bukan pilihan — termasuk test persistensi terfokus yang membuktikan baris preview `IsMember = false` tersimpan fisik `false` di `MstPatient`. Menjawab `PAT-OQ-022` dan memenuhi `TEST_POLICY` bagian 3 | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-038` | `legacy_pid` bersemantik bigint bertanda / Int64. Tidak pernah diurutkan secara angka dengan cara mengubahnya menjadi teks | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-039` | Pilot 715, crosswalk Pilot, tabel cadangan Pilot, dan QR Pilot lama tetap immutable dan dipertahankan. QR Pilot lama dibiarkan **byte demi byte**: tidak dihapus, ditimpa, atau dipindah | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-040` | Daftar 25 pada bagian 11 adalah **CANARY-A CANDIDATE MANIFEST — DRAFT**: bukan manifest eksekusi dan tidak diotorisasi runtime. Manifest eksekusi dibekukan ulang terpisah sesudah strategi QR dan reservation guard disetujui pemilik. 10 tabrakan tidak diganti otomatis pada sesi perencanaan. Menggantikan sebagian `PAT-DEC-026` (status daftar); syarat kelayakan pada `PAT-DEC-026` tetap berlaku | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-041` | Seluruh 704.506 baris preview kanonik `IsMember = false` dan `IsActive = true`. `IsMember = false` **wajib** tersimpan `false`; preview tidak boleh diubah menjadi `true` untuk menghindari masalah ini. `IsActive = false` **bukan** kebutuhan bulk — koreksi atas pernyataan lama (bagian 14). Bila perubahan konfigurasi EF dibutuhkan, yang diusulkan adalah perubahan terkecil yang aman beserta impact scan pendaftaran normal (`PAT-OQ-029`) | `approved` — pemilik, 15 Oktober 2026 |
| `PAT-DEC-042` | Perlindungan reservasi wajib mencakup **`PatientCode` dan `MedicalRecordNumber`**, dan wajib tahan lama — kunci yang hanya hidup di memori proses tidak diterima. Pemilik boleh mewajibkan pembuatan pasien normal diblokir selama jendela canary, tetapi itu saja **tidak cukup** sebagai strategi final gelombang besar. Worker tetap tidak membangkitkan MRN; MRN kanonik tetap `final_medical_record_number` dengan `plan_status = FINALIZED`. Mekanismenya belum disetujui (`PAT-OQ-030`–`032`, `035`) | `approved` sebagai prinsip — pemilik, 15 Oktober 2026 |
| `PAT-DEC-043` | Tabrakan namespace QR 715 adalah **isu desain P0**. Perilaku `QR_ARTIFACT_ALREADY_EXISTS` generik tidak cukup karena memblokir 715 pasien sah secara permanen. Pemakaian ulang QR tidak boleh dipilih diam-diam; berkas yang path-nya memuat MRN target **bukan** bukti bahwa berkas itu sah. Strateginya belum disetujui (`PAT-OQ-028`) | `approved` sebagai prinsip — pemilik, 15 Oktober 2026 |
| `PAT-DEC-044` | Endpoint khusus di bawah route migrasi admin Patient yang sudah ada. Request: `batchId`, `selectionMode`, `dryRun` (bawaan `true`), `limit`. `legacyPids` **tidak** dikirim klien; manifest disimpan di server. Respons wajib menyebut identifier, versi, dan sidik jari manifest. Menjawab `PAT-OQ-010` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-045` | Build pertama hanya mendukung `selectionMode = CANARY_A`; `limit` 1–25, bawaan 25. Untuk `CANARY_A`, urutan eksekusi **persis urutan manifest server yang disetujui** — manifest tidak disort ulang berdasarkan `legacy_pid`. Urutan naik numerik Int64 hanya aturan bagi mode pemilihan umum yang mungkin dibuat di task kemudian. Menjawab `PAT-OQ-012` (pilihan A dengan koreksi) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-046` | Simulasi boleh membaca filesystem: keberadaan, metadata berkas, keadaan path, dan hash bila desain membutuhkannya. Simulasi **dilarang** membuat folder, membuat berkas, mengubah berkas, atau menghapus berkas. Menjawab `PAT-OQ-016` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-047` | Baris crosswalk baru hanya berisi nilai yang bersumber langsung dari preview kanonik yang disetujui: `legacy_pid` = `preview.legacy_pid`; `legacy_old_rm` = `preview.legacy_old_rm`; `normalized_mrn` = `preview.legacy_normalized_mrn`; `normalized_nik` = `preview.legacy_normalized_nik`; `quilvian_patient_id` = `MstPatient.Id` yang dibuat; `match_status` = `TARGET_CREATED`; `migration_status` = `MIGRATED_BULK_WORKER`; `last_batch_id` = batch kanonik; `created_at` = waktu database. NULL: `canonical_legacy_pid`, `review_reason`, dan `updated_at` pada insert awal. `normalized_nik` hanya **bukti** dan tidak boleh dipakai untuk menggabungkan atau mengenali pasien yang sama; `normalized_mrn` tetap **bukan** MRN tujuan (`PAT-DEC-005`). Menjawab `PAT-OQ-018` (pilihan kustom pemilik, berbeda dari rekomendasi A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-048` | Kunci per pasien memakai try-lock database yang aman dari tabrakan dan ber-namespace — **bukan** satu `hashtext` 32-bit. Bentuk yang disukai: `pg_try_advisory_xact_lock(key1, key2)` dua kunci, key1 = namespace tetap PAT-MIG-002/batch kanonik, key2 = `legacy_pid` bertanda bila aman masuk Int32; service **wajib** memvalidasi konversinya. Bila domain tidak dapat menjamin Int32, dipakai desain kunci 64-bit stabil yang perilaku tabrakannya direview eksplisit. Kontensi kunci → `CONCURRENT_RUN`, tidak pernah menunggu tanpa batas. Menjawab `PAT-OQ-021` (try-lock dengan koreksi) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-049` | Respons per pasien boleh memuat `legacyPid`, `PatientCode`, MRN, `QrCodePath`/path rencana, status, `fullName`, dan tanda ada/tidak; **tidak** boleh memuat nilai NIK, telepon, maupun alamat. Log aplikasi **tidak** boleh memuat `fullName`, NIK, telepon, maupun alamat. Menjawab `PAT-OQ-023` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-050` | Bila telepon preview berklasifikasi `SAFE_TO_STORE` sudah bergeser dan kini dipakai `MstPatient` lain → konflik `PHONE_ALREADY_USED`; pasien tidak dibuat. Worker tidak boleh diam-diam membuang atau mengubah nilai preview yang immutable. Menjawab `PAT-OQ-024` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-051` | `match_status = TARGET_CREATED`. Menjawab `PAT-OQ-027` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-052` | Seluruh pasien bulk `BE-PAT-MIG-002` — bukan hanya 715 tabrakan — memakai `patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png`. Isi QR tetap MRN berformat; `MstPatient.QrCodePath` menyimpan path baru; QR Pilot lama dibiarkan byte demi byte; tidak ada adopsi QR historis; `CreateNew` tanpa penimpaan; berkas tak dikenal di path `rsmmc-bulk` = konflik; simulasi memeriksa path baru yang direncanakan dan melaporkan keberadaan path bawaan historis sebagai informasi. Menjawab `PAT-OQ-028` (pilihan A, varian A1) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-053` | Konfigurasi EF `IsMember` pada `MstPatient` **tidak** diubah secara global pada task ini. Sesudah insert EF, di transaksi per pasien yang **sama**: (1) update eksplisit hanya baris pasien baru menjadi `IsMember = false`; (2) baca ulang `AsNoTracking`; (3) verifikasi `IsMember = false` dan `IsActive = true`; (4) verifikasi seluruh kolom preview yang disetujui; (5) gagal dan rollback bila ada selisih. Bukti persistensi **provider-real** (PostgreSQL) wajib; bukti SQLite saja tidak cukup untuk perilaku default PostgreSQL. Menjawab `PAT-OQ-029` (pilihan A); cara menghasilkan bukti provider-real → `PAT-OQ-036`. **Bagian (1) dihapus `PAT-DEC-075`** (19 Oktober 2026) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua); diamandemen `PAT-DEC-075` |
| `PAT-DEC-054` | Allocator `PatientCode` normal memakai **lantai tahan lama**: suffix normal minimum `705250`. Allocator wajib: berada di module service; menggantikan pembangkit "kosong terkecil" milik controller; dipakai seluruh jalur pembuatan pasien normal; menyerialkan alokasi di tingkat database; memilih suffix pertama yang belum dipakai ≥ 705.250; mengandalkan indeks unik `PatientCode` sebagai pengaman terakhir; tidak pernah memakai 742–705.249. Pembekuan operasional tetap gerbang canary tambahan. Menjawab `PAT-OQ-030` (pilihan B); cakupan environment → `PAT-OQ-037` | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-055` | Pendaftaran pasien normal di Staging — termasuk jalur admin dan kiosk — dibekukan secara operasional **sekarang**, sampai guard reservasi dideploy dan diverifikasi. Ini bukan strategi final tahan lama; Produksi tidak terdampak. Gerbang runtime worker `NORMAL_ALLOCATION_GUARD` tetap menolak eksekusi kecuali `BE-PAT-MIG-003` aktif dan terverifikasi — pembekuan saja tidak cukup. Menjawab `PAT-OQ-032` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-056` | Ke-25 `legacy_pid` kandidat dipertahankan persis. Begitu QR A1 dan reservation guard selesai, 25 nilai yang sama — dalam urutan yang sama — menjadi manifest eksekusi Canary-A: 10 menguji informasi tabrakan path bawaan historis, 15 menguji kasus path bawaan bersih, dan seluruh 25 menulis ke namespace `rsmmc-bulk`. Tidak ada penggantian otomatis. Menjawab `PAT-OQ-033` (pilihan A, bersyarat pada `PAT-DEC-052`) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-057` | Task prasyarat terpisah **`BE-PAT-MIG-003` — RSMMC Normal Patient Identifier Reservation Guard**, pemilik: perlindungan allocator `PatientCode` normal, perlindungan allocator MRN normal, dampak pembuatan pasien normal admin/kiosk, dan test terfokus terkait. `BE-PAT-MIG-003` wajib disetujui source-nya, dideploy, dan diverifikasi runtime **sebelum** Canary-A `dryRun=false`. `BE-PAT-MIG-002` tetap memuat `NORMAL_ALLOCATION_GUARD`. Menjawab `PAT-OQ-035` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-058` | Untuk Canary-B kelak, baris crosswalk historis mempertahankan `match_status`, `migration_status`, `review_reason`, dan `last_batch_id`; yang diisi hanya `quilvian_patient_id` dan `updated_at`. Kemajuan ditentukan dari `quilvian_patient_id` + `MstPatient`, bukan dengan menimpa bukti review historis. Menjawab `PAT-OQ-013` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-059` | 22 baris historis `DUPLICATE_CANDIDATE`, `MERGE_CANDIDATE`, dan `REVIEW_NEWBORN` tetap dikecualikan sampai review identitas/keselamatan pasien per baris; tidak masuk Canary-B secara otomatis. Menjawab `PAT-OQ-014` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-060` | Tidak ada task frontend untuk `BE-PAT-MIG-002`. Operator memakai admin API berwenang, Swagger, atau klien HTTP terkendali. Menjawab `PAT-OQ-025` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-061` | Mekanisme gelombang besar ditunda ke `PAT-GATE-004`, sesudah: Canary-A `PASS`, eksekusi ulang idempoten `PASS`, Canary-B `PASS`, inventaris filesystem, review penyimpanan/kapasitas, dan verifikasi reservation guard. Menjawab `PAT-OQ-026` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-062` | Inventaris filesystem read-only **wajib**: sebelum otorisasi runtime Canary-A dan sekali lagi sebelum `PAT-GATE-004`. Cakupannya `patient-qrcodes/` dan `patient-qrcodes/rsmmc-bulk/`; berkas/folder tak terduga dicatat tanpa diubah. Menjawab `PAT-OQ-034` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi kedua) |
| `PAT-DEC-063` | Skema fisik `PAT-FACT-079`–`083` diterima. SQL worker wajib **bertipe** sesuai skema: `batch_id` uuid, `legacy_pid` bigint, tujuh kolom enum integer, empat kolom boolean, kolom teks sebagai teks; **tidak ada** fallback `::text` pada implementasi bulk final. Nilai `TARGET_CREATED` dan `MIGRATED_BULK_WORKER` muat tanpa perubahan skema; tidak ada EF migration. Menjawab `PAT-OQ-019` | `approved` — pemilik, 15 Oktober 2026 (sesi ketiga) |
| `PAT-DEC-064` | Strategi **lantai MRN** untuk allocator MRN normal di Staging: minimum numerik **890.188** (MRN pertama `00-89-01-88`); memilih MRN numerik pertama yang belum dipakai ≥ 890.188; serialisasi di tingkat database; indeks unik tetap pengaman; tidak pernah mengalokasikan di bawah 890.188 selama kampanye reservasi RSMMC aktif; hanya berlaku di bawah aturan guard Staging persis (`PAT-DEC-066`). `BE-PAT-MIG-002` sendiri tidak pernah membangkitkan MRN; MRN kanonik selalu dari perencana `FINALIZED`. Menjawab `PAT-OQ-031` (pilihan B, lantai) | `approved` — pemilik, 15 Oktober 2026 (sesi ketiga) |
| `PAT-DEC-065` | Bukti persistensi provider-real `IsMember` memakai **PostgreSQL 15 sekali pakai** — terisolasi, bukan Staging bersama, bukan Produksi, provider Npgsql, konfigurasi EF yang sebenarnya, dan dihancurkan sesudah test. Test minimal membuktikan: `IsMember = false` tersimpan fisik `false`; `IsActive = true` tetap `true`; koreksi eksplisit `IsMember` di transaksi yang sama berhasil; baca ulang `AsNoTracking` cocok dengan preview; rollback tidak meninggalkan pasien maupun crosswalk; percobaan ulang yang idempoten tidak membuat target ganda; kegagalan sebelum *commit* tidak meninggalkan pemetaan pasien yang ter-*commit*. Bukti SQLite saja tidak cukup. Menjawab `PAT-OQ-036` (pilihan A) | `approved` — pemilik, 15 Oktober 2026 (sesi ketiga) |
| `PAT-DEC-066` | Guard reservasi berlaku **hanya** bila kedua syarat benar: `EnvironmentName == "Staging"` (persis, membedakan huruf besar-kecil) **dan** `RsmmcMigrationReservationGuard.Enabled == true`. Di Staging persis: konfigurasi reservasi yang hilang atau tidak sah = **gagal tertutup**; suffix `PatientCode` normal minimum 705.250; MRN numerik normal minimum 890.188. Produksi: kampanye reservasi migrasi tidak boleh mengubah perilaku penomoran normal, dan endpoint `BE-PAT-MIG-002` tetap mustahil di luar Staging persis. Development/Test: guard mati kecuali test integrasi secara eksplisit di-*host* sebagai Staging. Menjawab `PAT-OQ-037` (pilihan A, dengan syarat konfigurasi tambahan). Memperjelas `PAT-DEC-054`: lantai dan serialisasi berlaku saat guard aktif; di luar itu penomoran normal tetap aturan yang berlaku sekarang | `approved` — pemilik, 15 Oktober 2026 (sesi ketiga); perlakuan `Enabled = false` di Staging **diamandemen** `PAT-DEC-067` |
| `PAT-DEC-067` | **Amandemen `PAT-DEC-066` sebelum build `BE-PAT-MIG-003`** — instruksi pemilik 16 Oktober 2026 (`TASK MODE: BACKEND`, bagian 0 dan 9). (a) Di `EnvironmentName == "Staging"` persis, allocator yang aman reservasi **wajib** dipakai dan hanya berjalan bila `RsmmcMigrationReservationGuard.Enabled == true`; `Enabled` bernilai `false`, hilang, salah bentuk, atau tidak terbaca = **gagal tertutup** — pendaftaran normal ditolak `503` / `PATIENT_RESERVATION_GUARD_CONFIG_INVALID` sebelum insert pasien, penulisan QR, dan penulisan foto, dan **tidak pernah** jatuh ke allocator lama. `Enabled = false` eksplisit di Staging **tidak** mematikan perlindungan reservasi. (b) Di luar Staging persis, perilaku allocator normal yang sekarang tidak berubah dan konfigurasi guard tidak dibutuhkan; perilaku Produksi tidak berubah. (c) Pemeriksa `PAT-GATE-005` memakai izin admin yang disetujui dan melaporkan sekurang-kurangnya `environment`, `guardEnabled`, `guardConfigurationValid`, kode dan MRN berikutnya (angka dan format), kedua lantai, `patientCodeFloorSatisfied`, `mrnFloorSatisfied`, `overallGuardReady`; diwujudkan sebagai endpoint baca-saja khusus Staging `GET /admin/migration/rsmmc-reservation-guard` dengan `Patient : Update` (kontrak `1.2.1` B14.2). Menggantikan bunyi `PAT-DEC-066` dan `PAT-GAP-002` yang menyatakan `Enabled = false` eksplisit mematikan guard. Blueprint naik ke revision `4`, kontrak ke `1.2.1` | `approved` — pemilik, 16 Oktober 2026 |
| `PAT-DEC-068` | **Pengerasan `BE-PAT-MIG-003` sebelum bukti provider-real** — instruksi pemilik 16 Oktober 2026 (*source reconciliation / review fix pass*, bagian 4 dan 5). (a) Endpoint pemeriksa `GET /admin/migration/rsmmc-reservation-guard` **dikonfirmasi** dengan syarat: khusus `Staging` persis, dijaga `Patient : Update`, baca-saja, nol mutasi database, nol mutasi filesystem, tanpa mencadangkan nomor, tanpa data pribadi, dan hanya mengembalikan metadata guard/kesiapan/alokasi yang dibutuhkan `PAT-GATE-005`. (b) Kunci serialisasi allocator memakai namespace advisory-lock **tetap dan terdokumentasi**, bukan `hashtext()` saat runtime: `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE); satu kunci global untuk seluruh pendaftaran pasien normal; bentuk transaksi (`xact`); sama bagi seluruh instance; tidak bertabrakan dengan kunci lain di repository (seluruhnya bentuk satu kunci 64-bit) maupun dengan namespace worker `BE-PAT-MIG-002` `1173286842`. Menggantikan usulan `PAT-GAP-004` (`hashtext`). Blueprint naik ke revision `5`, kontrak ke `1.2.2` | `approved` — pemilik, 16 Oktober 2026 |
| `PAT-DEC-069` | **Task prasyarat baru `BE-PAT-MIG-003A` — Newborn Identifier Reservation Guard** (keputusan A instruksi pemilik 19 Oktober 2026). Impact scan `BE-PAT-MIG-002` menemukan jalur pendaftaran bayi `POST /newborns` yang mengalokasikan nomor di luar allocator `BE-PAT-MIG-003` (`PAT-SRC-031`). `BE-PAT-MIG-002` tetap ⛔ sampai `BE-PAT-MIG-003A` selesai, diuji, dideploy ke Staging, dan gerbang runtime yang diperluas — `PAT-GATE-005` generasi 2 — `PASS`. Generasi 1 (`CreatePatient` saja) sudah `PASS` (`PAT-FACT-090`). Implementasi `BE-PAT-MIG-002` belum diberi wewenang | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-070` | **Lantai reservasi untuk jalur bayi** (keputusan B, C, D). (a) Di `EnvironmentName == "Staging"` persis, pendaftaran bayi tidak boleh memakai `PatientCode` maupun MRN yang dicadangkan migrasi RSMMC: lantainya sama dengan pendaftaran normal, `PAT-RSMMC-705250` (705.250) dan `00-89-01-88` (890.188); nomor di bawah lantai milik perencana migrasi kanonik. (b) Di **seluruh** environment lain perilaku `NumberSeries` bayi (`PATIENT_CODE`, `PATIENT_MEDICAL_RECORD`) tidak berubah, dan Produksi persis seperti sebelum task ini; allocator normal **tidak** dipasang global pada jalur bayi. (c) Di Staging persis, `RsmmcMigrationReservationGuard__Enabled=true` → alokasi berlantai; `false`, hilang, atau salah bentuk → pendaftaran bayi **gagal tertutup** sebelum alokasi nomor dan sebelum mutasi `MstPatient` apa pun, dengan filosofi yang sama dengan `PAT-DEC-067` | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-071` | **Satu kunci untuk pendaftaran normal dan bayi** (keputusan E). Alokasi berlantai jalur bayi memakai kunci yang sudah disetujui `pg_advisory_xact_lock(1346458697, 1313821261)` (`"PATI"`/`"NORM"`, `PAT-DEC-068`), sehingga normal↔normal, normal↔bayi, dan bayi↔bayi tidak pernah berebut nomor yang sama. Tidak ada rentang alokasi tersendiri untuk bayi | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-072` | **Pemeriksa dan gerbang diperluas** (keputusan F; amandemen 10.1–10.2). `PASS` pemeriksa `GET /admin/migration/rsmmc-reservation-guard` wajib membuktikan **kedua** hal: pendaftaran normal dan pendaftaran bayi tidak dapat memakai nomor cadangan migrasi. Keluaran memuat sekurang-kurangnya `environment = Staging`, `guardState = ACTIVE`, `guardEnabled = true`, `guardConfigurationValid = true`, `normalPatientCreateProtected = true`, `newbornPatientCreateProtected = true`, kedua lantai terpenuhi, dan `overallGuardReady = true`; `overallGuardReady` **tidak boleh** `true` bila jalur bayi tidak terlindungi. Pemeriksa tetap baca-saja, khusus Staging, `Patient : Update`, tanpa membuat pasien, tanpa mencadangkan nomor, tanpa mutasi database/filesystem, tanpa data pribadi. Prasyarat worker `NORMAL_ALLOCATION_GUARD` kini berarti **seluruh** jalur pembuatan pasien non-migrasi yang berlaku aman reservasi, termasuk pendaftaran bayi. Pernyataan bahwa seluruh insert `MstPatient` bermuara pada `CreatePatient` (`PAT-SRC-023`) dicabut | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-073` | **Pembekuan operasional pembuatan pasien Staging** (keputusan G). Selama persiapan dan jendela Canary-A, pembuatan pasien di Staging tetap **dibekukan secara operasional**: pendaftaran normal, admin, kiosk, dan bayi. Pembekuan adalah kontrol operasional migrasi, **bukan** dikodekan dengan menghapus atau mematikan route yang juga dipakai Produksi. Memperluas `PAT-DEC-055` ke jalur bayi dan memperpanjang masa berlakunya sampai jendela Canary-A | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-074` | **Tanpa EF migration baru** (keputusan H). `BE-PAT-MIG-003A` tidak diharapkan membutuhkan perubahan skema. Migration upstream `20261016080745_ReviseNewbornAdmissionFlow` milik fitur bayi, bukan task ini. Bila pengerasan ternyata membutuhkan migration lain: berhenti dan laporkan `DESIGN_CHANGE_REQUIRED` | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-075` | **Amandemen `PAT-DEC-053` — `IsMember` (amandemen 10.3).** Impact scan membuktikan semantik EF saat ini: `MstPatient.IsMember` dikonfigurasi `HasDefaultValue(true).HasSentinel(true)`, sehingga entity baru dengan `IsMember = false` **dikirim eksplisit** ke PostgreSQL (`PAT-SRC-032`). Langkah `PERSIST_IS_MEMBER` — UPDATE eksplisit sesudah insert — **dihapus**. Semantik worker kelak: INSERT `MstPatient` dengan `IsMember = false` → baca ulang `AsNoTracking` di dalam transaksi → verifikasi `false` → rollback bila selisih. Worker tidak boleh mengeluarkan UPDATE khusus hanya untuk memaksa `IsMember = false`, karena kepemilikan proyeksi membership ada pada crm-membership (`PatientMembershipStatusResolver`). Audit pasca-Canary wajib memperhitungkan lifecycle dan proyeksi membership sesudah transaksi. Menggantikan bagian (1) `PAT-DEC-053`; bagian (2)–(5) dan bukti provider-real `PAT-DEC-065` tetap, dengan butir "koreksi eksplisit `IsMember` berhasil" diganti "INSERT menyimpan `IsMember = false`" | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-076` | **Kolom `MstPatient` baru pada worker (amandemen 10.4).** `Citizenship`, `NationalityCountryId`, `DeliveryCaseId`, `NewbornRegistrationKeyHash`, dan `NewbornRegistrationFingerprint` (`PAT-SRC-033`) tidak punya pemetaan legacy yang disetujui, sehingga dibiarkan `NULL` oleh `BE-PAT-MIG-002`. Nilai tidak dikarang | `approved` — pemilik, 19 Oktober 2026 |
| `PAT-DEC-077` | **Baseline pasien untuk gerbang `PATIENT_BASELINE` (amandemen 10.5).** Angka historis 741 (`PAT-FACT-038`) **bukan** nilai permanen. Pembuatan pasien Staging tetap dibekukan selama persiapan/jendela Canary-A (`PAT-DEC-073`). Sesudah seluruh prasyarat source dan migration dideploy dan pemeriksa lulus lagi, baseline `MstPatient` baru diukur **read-only** dan dicatat sebagai bukti sebelum Canary-A. Worker memakai nilai baseline tetap yang disetujui itu, **bukan** menerima jumlah berapa pun yang ada saat eksekusi | `approved` — pemilik, 19 Oktober 2026 |

---

## 9. Fakta source dan temuan impact scan

Dibaca read-only pada `QuilvianIntegrationBackend` @ `a52b3c98`. Nomor baris berlaku untuk commit
itu. Rincian komponen yang dipakai ulang dan yang dilarang dipakai ulang ada pada kartu task
`BE-PAT-MIG-002`.

| ID | Temuan | Bukti | Akibat untuk `BE-PAT-MIG-002` |
| --- | --- | --- | --- |
| `PAT-SRC-001` | Source `BE-PAT-MIG-001` sudah ada di Integration lewat commit `ef3adbe4` (9 Oktober 2026) | `git log`; enam berkas `Rsmmc*` di `Areas/HealthServices/PatientManagement/MasterData/` | Komponennya dapat dinilai untuk dipakai ulang |
| `PAT-SRC-002` | Base route, `[Authorize]`, `[AccessController(ControllerName = "Patient")]`, dan `[Tags]` | `PatientController.cs` baris 42–53 | Endpoint baru berada di grup Swagger yang sama |
| `PAT-SRC-003` | Endpoint `BE-PAT-MIG-001` memakai `[AccessAction("Update", "Reconcile RSMMC Pilot MRN", …, SortOrder = 3)]` dan `[AccessPermission("Patient", "Update")]` | `PatientController.cs` baris 1057–1069 | Pola izin dapat diikuti; endpoint lama tidak disentuh |
| `PAT-SRC-004` | Gerbang environment `Staging` persis, perbandingan ordinal | `RsmmcPilotEnvironmentGate.cs` baris 19–25 | `REUSE` apa adanya |
| `PAT-SRC-005` | Penyimpan QR: pemeriksaan awal, `FileMode.CreateNew`, bukti kepemilikan penanda + panjang + SHA-256, pembersihan terbatas | `RsmmcPilotQrArtifactStore.cs` baris 136–251; factory `PatientController.cs` baris 1115–1131 | `REUSE` apa adanya lewat factory controller |
| `PAT-SRC-006` | Pembangkit normal memakai pola **"nomor kosong terkecil"**. `GeneratePatientCodeAsync` → `GenerateRunningCodeAsync` (awalan `PAT-RSMMC-`, 5 digit) dan `GenerateMedicalRecordNumberAsync` membaca **seluruh** nomor yang dipakai, lalu mencari angka positif terkecil yang belum dipakai | `PatientController.cs` baris 58–59, 1478–1509, 2520–2554 | Pendaftaran pasien biasa di Staging selama migrasi akan memakai kode/MRN yang dicadangkan preview, termasuk kode cadangan pasien tertunda. Lihat `PAT-OQ-020` |
| `PAT-SRC-007` | Indeks unik `PatientCode` dan `MedicalRecordNumber` tanpa syarat; indeks unik `IdentityNumber` bersyarat (`IS NOT NULL AND IsDelete = false`); `PhoneNumber` hanya indeks biasa | `MstPatientConfiguration.cs` baris 208–220 | Jaring pengaman terakhir terhadap pasien ganda: insert kedua dengan kode atau MRN yang sama ditolak database. Telepon tidak dijaga database |
| `PAT-SRC-008` | `IsMember` dan `IsActive` dikonfigurasi `HasDefaultValue(true)` dengan `ValueGeneratedOnAdd`. EF Core tidak mengirim nilai `false` — nilai bawaan C# — saat insert, sehingga database mengisi `true` | `MstPatientConfiguration.cs` baris 95–96 dan 143–144; `Migrations/ApplicationDbContextModelSnapshot.cs` baris 95275–95298 | Seluruh 704.506 baris preview `IsMember = false` (`PAT-FACT-074`), sehingga **setiap** insert EF biasa akan diam-diam menyimpan `IsMember = true`. `IsActive` tidak terdampak karena seluruh preview `IsActive = true` dan EF mengirim nilai `true`. Lihat `PAT-OQ-029`. *Dikoreksi 15 Oktober 2026 — teks lama menyebut `is_active = false` juga (bagian 14).* **Tidak berlaku lagi** sejak `0c6e3145` (`PAT-SRC-030`, `PAT-SRC-032`) |
| `PAT-SRC-009` | Enum yang punya default database (`Religion`, `MaritalStatus`, `BloodType`, `RegistrationSource`) memakai `Unknown = 0`; `PatientType` dan `PatientStatus` tidak punya nilai 0 | `Enums/Religion.cs`, `Enums/MaritalStatus.cs`, `Enums/BloodType.cs`, `Areas/HealthServices/MasterData/Enums/Patient*.cs` | Tidak ada kehilangan nilai enum. `PatientRegistrationSource.Migration = 6` tersedia |
| `PAT-SRC-010` | SQL pembaca Pilot meng-cast kolom ke `text` dan menyaring `lower(batch_id::text)` serta `legacy_pid::text = {1}` | `RsmmcPilotMigrationSource.cs` baris 71–102 | Indeks tidak terpakai. Cukup untuk 715 baris, **tidak** cukup untuk pemeriksaan ulang per pasien atas tabel ±705 ribu baris. Tidak dipakai ulang apa adanya |
| `PAT-SRC-011` | `LegacyPidComparer` memakai `NumberStyles.None`, sehingga angka negatif dianggap bukan angka dan diurutkan **sesudah** seluruh angka positif | `RsmmcPilotMrnReconciliationService.cs` baris 873–899 | Kandidat Canary-A memuat 5 `legacy_pid` negatif. Urutan versi Pilot berbeda dari `ORDER BY` `bigint` PostgreSQL. Tidak dipakai ulang apa adanya |
| `PAT-SRC-012` | Profil batch Pilot mengharapkan 715 pasien Pilot dan 715 baris cadangan | `RsmmcPilotApprovedBatchProfile.cs` baris 21–25 | Khusus `BE-PAT-MIG-001`; worker bulk butuh profil sendiri |
| `PAT-SRC-013` | Antarmuka pembaca Pilot sengaja hanya-baca, dan dijaga test `AntarmukaSumberMigrasi_TidakPunyaOperasiTulis` | `RsmmcPilotMigrationSource.cs` baris 30–52; laporan `BE-PAT-MIG-001` bagian 5.1 | Penulisan crosswalk wajib berada di komponen baru, bukan menambah operasi tulis ke pembaca Pilot |
| `PAT-SRC-014` | Pola repository untuk kunci database: `SELECT pg_advisory_xact_lock(hashtext({0}))` di dalam transaksi, hanya pada Npgsql | `AccJournalService.cs` baris 1439–1448; `FinanceOpeningItemBatchService.cs` baris 965 | Kunci per pasien mengikuti pola yang sudah terbukti |
| `PAT-SRC-015` | `GetFileStoragePaths()` membuat folder saat dipanggil; factory QR memanggilnya secara malas | `PatientController.cs` baris 1115–1131; laporan `BE-PAT-MIG-001` bagian 3.2 | Simulasi yang memeriksa keberadaan QR tujuan butuh cara baca yang tidak membuat folder (`PAT-OQ-016`) |
| `PAT-SRC-016` | Pembuatan pasien normal memanggil kedua pembangkit, menulis QR lewat `SavePatientQrCodeFile` yang menimpa berkas, dan membersihkan QR dengan `DeletePhysicalFileIfExists` tanpa bukti kepemilikan | `PatientController.cs` baris 516–517 dan 586, serta jalur gagal sesudahnya | Alur pembuatan normal **tidak** dipakai |
| `PAT-SRC-017` | Tidak ada `HasQueryFilter` pada `MstPatient`; `SaveChangesAsync` hanya menstempel versi `InpEpisode` | `Repositories/ApplicationDbContext.cs` baris 1242–1262 | Insert `MstPatient` tidak terkena efek samping lain |
| `PAT-SRC-018` | Project `Tests/QuilvianSystemBackend.PatientManagementTests/` (SQLite di memori, tiruan tabel `migration.rsmmc_*`) masih ada; kini ada 4 project test di `Tests/` | `Tests/`; `Infrastructure/TestDatabase.cs` | Lokasi test bila pemilik memintanya (`PAT-OQ-022`). Selisih governance `PAT-RSK-007` tetap ada |
| `PAT-SRC-019` | Konsumen backend `QrCodePath`: kolom hanya ditulis saat pembuatan pasien (`PatientController.cs` baris 597) dan oleh rekonsiliasi `BE-PAT-MIG-001`, lalu dikembalikan **apa adanya** oleh `MapResponse` (baris 2627) dan `MapDetailResponse` (baris 2696) lewat `PatientDtos.cs` baris 99 dan 449. Tidak ada kode backend yang menyusun path QR pasien dari MRN di luar helper pembuatan (`SavePatientQrCodeFile` mulai baris 1133; factory baris 1115–1131). `UpdatePatient` dan `DeletePatient` tidak menyentuh berkas QR. `QrCodePath` pada `TrxMedicalCertificate` adalah milik entity lain dan tidak berkaitan | Impact scan 15 Oktober 2026 | Konsumen backend memakai path yang tersimpan |
| `PAT-SRC-020` | Konsumen frontend (`QuilvianSystemFrontendDev` @ `2831ea63`, working tree): cetak kartu kiosk pasien baru, layanan registrasi kiosk pasien lama, dan utilitas cetak kartu memakai `qrCodePath`/`qrCodeUrl` dari respons API lalu mengubahnya menjadi URL berkas backend (`kiosk-new-patient-step-print-card.jsx` baris 222–242 dan 442; `kiosk-old-patient-registration.service.js` baris 117–119 dan 373; `kiosk-patient-card-print-utils.jsx` baris 196–197). `base-patient-card.jsx` (baris 69, 149–200) dan `registration-success-step.jsx` IGD (baris 489–490) membuat QR di sisi klien dari MRN, tanpa berkas. Tidak ada kode frontend yang menyusun `patient-qrcodes/<MRN>/qrcode.png` | Impact scan 15 Oktober 2026 | Strategi `PAT-OQ-028` pilihan A tidak menuntut perubahan frontend |
| `PAT-SRC-021` | Folder upload disajikan seluruhnya: `UseStaticFiles` dengan `PhysicalFileProvider(UploadRootPath)` dan `RequestPath` bawaan `/uploads` | `Program.cs` baris 1710–1741 | Subfolder baru seperti `patient-qrcodes/rsmmc-bulk/` dapat diakses tanpa konfigurasi tambahan |
| `PAT-SRC-022` | `RsmmcPilotQrArtifactStore` menyusun folder dari `layout.QrFolderName` + MRN yang sudah dibersihkan; `SanitizePathSegment` hanya menyisakan huruf, angka, `-`, dan `_` | `RsmmcPilotQrArtifactStore.cs` baris 256–266; `PatientController.cs` baris 1859–1871 | Folder dasar lain dapat dipakai tanpa mengubah kode store; MRN berformat tidak mungkin menghasilkan nama folder `rsmmc-bulk` |
| `PAT-SRC-023` | Jalur yang meng-insert `MstPatient`: hanya `CreatePatient` (rute biasa, kiosk, admin — `PatientController.cs` baris 464–465, 757; entity baris 519, insert baris 637) dan `OperatingRoomDemoSeeder` (baris 443; hanya bila `Seeders:RunOperatingRoomDemoSeed` aktif dan bukan `Production`; kode `DEMO-OPR-PT`, MRN `DEMO-OPR-RM-001`). Allocator nomor hanya `GeneratePatientCodeAsync` dan `GenerateMedicalRecordNumberAsync`, tanpa kunci; indeks unik menjadi satu-satunya pengaman | Impact scan 15 Oktober 2026; `Program.cs` baris 2013–2022 | Titik pasang reservation guard hanya satu jalur pendaftaran; seeder demo dapat menggeser hitungan `PATIENT_BASELINE` bila diaktifkan. **Diganti sebagian `PAT-SRC-031`** (19 Oktober 2026): jalur bayi `NewbornPatientRegistrationService` juga meng-insert `MstPatient` dengan allocator sendiri |
| `PAT-SRC-024` | `CreatePatientRequest.IsMember` bawaannya `false`, sedangkan EF tidak mengirim `false` saat insert, sehingga pendaftaran normal yang tidak mengirim `isMember` kini tersimpan `IsMember = true`. `UpdatePatient` menulis `entity.IsMember = request.IsMember` (baris 849) dan EF mengirimnya pada update, sehingga perilaku create dan update berbeda | `PatientDtos.cs` baris 386; `PatientController.cs` baris 849 | Mengubah konfigurasi EF (`PAT-OQ-029` pilihan B) mengubah perilaku pendaftaran normal |
| `PAT-SRC-025` | Tidak ada `Database.Migrate()` saat startup, dan tidak ada trigger atau function database di `Migrations/` | `Program.cs`; pencarian `CREATE TRIGGER`/`CREATE OR REPLACE FUNCTION` | Trigger database (`PAT-OQ-030`/`031` pilihan D) adalah pola baru di repository |
| `PAT-SRC-026` | Seluruh advisory lock di repository memakai bentuk **satu kunci** `pg_advisory_xact_lock(hashtext(…))`; tidak ada pemakaian bentuk dua kunci `(int, int)`. Menurut dokumentasi PostgreSQL, ruang kunci satu-kunci 64-bit dan dua-kunci 32-bit tidak saling tumpang tindih | Pencarian `advisory_*lock` pada `*.cs` dan `*.sql`, 15 Oktober 2026 | Kunci dua-bagian `PAT-DEC-048` tidak dapat bertabrakan dengan kunci modul lain yang ada; key1 cukup unik di antara pemakai dua-kunci (saat ini tidak ada) |
| `PAT-SRC-027` | `POST /` dan `POST /kiosk` (`CreatePatient`) dijaga `[Authorize(Policy = KioskReadPolicy)]` — peran SuperAdmin, Administrator, Kiosk, atau klaim kiosk — dan `[AccessAction("Create", …)]`, yang hanya metadata (`AccessActionAttribute` bukan filter). Hanya `POST /admin` (`CreatePatientForAdmin`) dijaga `[AccessPermission("Patient", "Create")]` | `PatientController.cs` baris 464–477 dan 757–766; `Attributes/AccessActionAttribute.cs`; `Program.cs` baris 1154–1175 | Pembekuan pendaftaran lewat pencabutan izin `Patient : Create` saja **tidak** menutup jalur `/` dan `/kiosk`. Contoh mekanisme pada `PAT-OQ-032` pilihan A dikoreksi (bagian 14) |
| `PAT-SRC-028` | `CreatePatient` sudah membuka transaksi database sendiri (`BeginTransactionAsync`, baris 510), memanggil `GeneratePatientCodeAsync` dan `GenerateMedicalRecordNumberAsync` di dalam transaksi itu (baris 516–517), lalu *commit* (baris 639). Kedua pembangkit memuat seluruh nomor `MstPatient` termasuk yang ditandai terhapus, lalu memilih angka kosong terkecil; MRN di atas 99.999.999 ditolak | `PatientController.cs` baris 510–639, 1478–1509, 2520–2554 | Kunci serialisasi `BE-PAT-MIG-003` dapat berlaku di dalam transaksi yang sudah ada, tanpa struktur transaksi baru (`PAT-GAP-004`) |
| `PAT-SRC-029` | Default database kolom enum `MstPatient`: `RegistrationSource`, `Religion`, `MaritalStatus`, `BloodType` ber-default `0` — sama dengan nilai bawaan C#, sehingga nilai yang dikirim tidak pernah hilang. `PatientType` (default `General` = 1) dan `PatientStatus` (default `Active` = 1) tidak punya anggota bernilai 0. `Gender` dan `BirthDate` *nullable* tanpa default. `FullName` wajib, maksimum 200 | `MstPatient.cs`; `MstPatientConfiguration.cs` baris 26–72; berkas enum `PatientType`, `PatientStatus`, `PatientRegistrationSource`, `Religion`, `MaritalStatus`, `BloodType`, `Gender` | Cacat default EF `PAT-SRC-008` hanya mengenai `IsMember`. Kolom enum aman selama nilai yang bukan anggota enum — termasuk 0 untuk `PatientType`/`PatientStatus` — ditolak `PREVIEW_VALUE_INVALID` (`PAT-GAP-010`) |
| `PAT-SRC-030` | Baseline Integration `0c6e3145` (16 Oktober 2026, 41 commit sesudah `a52b3c98`): `MstPatientConfiguration` kini `entity.Property(x => x.IsMember).HasDefaultValue(true).HasSentinel(true)` dan `MstPatient.IsMember` ber-default CLR `false` (`BE-CRM-015`, crm-membership). Upstream juga menambah `Citizenship`, `NationalityCountryId`, dan filter pencarian MRN pada `PatientController`, tanpa mengubah `CreatePatient` maupun pembangkit nomor | `git diff a52b3c98 0c6e3145` | Dasar `PAT-SRC-008` (nilai `false` hilang saat insert EF) **tidak lagi berlaku** pada baseline baru. Langkah `PERSIST_IS_MEMBER` `BE-PAT-MIG-002` (`PAT-DEC-053`) menjadi berlebih tetapi tetap aman; desain dan kontrak `BE-PAT-MIG-002` wajib di-*impact scan* sebelum build-nya (`PAT-STALE-009`). Tidak mengubah `BE-PAT-MIG-003` |
| `PAT-SRC-031` | **Jalur insert `MstPatient` kedua — pendaftaran bayi.** Commit upstream `3a0189c2` (16 Oktober 2026) menambah `POST /newborns` (`PatientController.CreateNewbornPatient`, `[AccessPermission("Patient", "Create")]`) → `NewbornPatientRegistrationService.RegisterAsync`. Di `c6537ee4`: transaksi dibuka di `ApplicationDbContext` pemanggil; kunci `pg_advisory_xact_lock(hashtext('PAT_NEWBORN_KEY_<hash>'))` lalu `hashtext('PAT_NEWBORN_CASE_<id>')`; nomor dialokasikan `AllocatePatientCodeAsync` / `AllocateMedicalRecordNumberAsync` lewat `NumberSeriesAllocator` (deret `PATIENT_CODE` awalan `PAT-RSMMC` 5 digit dan `PATIENT_MEDICAL_RECORD` awalan `MR` 8 digit, `NEVER`), dengan batas bawah `GetLegacyMaximumAsync` = digit tertinggi nomor yang ada bila baris deret belum ada, atau 0 bila sudah ada. Allocator platform memakai koneksi dan transaksinya sendiri (kunci `hashtext('NUM_SERIES_<deret>_GLOBAL')`), lalu `MstPatient` di-insert dan transaksi bayi di-*commit*. Tidak ada lantai 705.250/890.188 dan tidak ada kunci `(PATI, NORM)`; dengan data Staging saat ini nomor bayi pertama dapat menjadi `PAT-RSMMC-00742` — milik canonical `-667995` (Canary-A no. 5). Jalur ini tidak menulis QR | `NewbornPatientRegistrationService.cs` baris 38–39, 84–89, 170–171, 174–220, 243–250, 274–328; `NumberSeriesAllocator.cs` baris 84–169; `PatientController.cs` baris 834–885; impact scan 19 Oktober 2026 | Dasar `BE-PAT-MIG-003A` (`PAT-DEC-069`–`071`). Menggantikan sebagian `PAT-SRC-023`: jalur insert `MstPatient` kini `CreatePatient` (tiga rute), `NewbornPatientRegistrationService`, dan `OperatingRoomDemoSeeder` |
| `PAT-SRC-032` | **Semantik EF `IsMember` saat ini, terbukti probe.** Probe baca-saja EF Core 9.0.18 atas model aplikasi (metadata Npgsql + SQL INSERT yang ditangkap tanpa basis data): `IsMember` default `true`, sentinel `true`, `ValueGenerated.OnAdd`. Entity baru `IsMember = false` → kolom `IsMember` **dikirim** pada INSERT; `IsMember = true` → kolom tidak dikirim dan default database `true` berlaku. Hasil tersimpan sama dengan nilai entity pada kedua kasus | Impact scan `BE-PAT-MIG-002` 19 Oktober 2026; `MstPatientConfiguration.cs` baris 99; `MstPatient.cs` baris 92 | `PAT-SRC-008` tidak berlaku lagi. Dasar `PAT-DEC-075` |
| `PAT-SRC-033` | **Kolom `MstPatient` baru sejak desain `BE-PAT-MIG-002`.** `Citizenship` (int, *nullable*) dan `NationalityCountryId` (FK *nullable*) dari baseline `0c6e3145`; `DeliveryCaseId`, `NewbornRegistrationKeyHash` (64), dan `NewbornRegistrationFingerprint` (64) dari migration `20261016080745_ReviseNewbornAdmissionFlow`, beserta indeks unik berfilter `UX_MstPatient_DeliveryCase_BirthOrder` dan `UX_MstPatient_NewbornRegistrationKeyHash` (`IS NOT NULL`). Indeks unik `IdentityNumber` tetap berfilter `"IdentityNumber" IS NOT NULL AND "IsDelete" = false` | `MstPatientConfiguration.cs` baris 121–130, 256–264; `Migrations/20261016080745_ReviseNewbornAdmissionFlow.cs` | Nilai `NULL` dari worker tidak bertabrakan dengan indeks berfilter. Dasar `PAT-DEC-076`. Migration itu wajib sudah diterapkan di Staging sebelum runtime build yang membawanya |

---

## 10. Pertanyaan terbuka dan usulan — keadaan per 15 Oktober 2026

Bagian ini ditulis ulang pada amandemen 15 Oktober 2026 supaya setiap pertanyaan memakai bentuk
yang sama. Pertanyaan yang digantikan **tidak dihapus**: teks lamanya tetap ada dan statusnya
`SUPERSEDED`. Rekomendasi bukan approval.

| Status | Arti |
| --- | --- |
| `OWNER_APPROVED` | Sudah diputuskan pemilik; berlaku sebagai keputusan pada revision `3` |
| `BLOCKING` | Belum diputuskan, dan menahan persetujuan desain `PAT-DEC-033`, build, atau runtime Canary-A |
| `PROPOSED` | Belum diputuskan, tetapi hanya menahan slice yang lebih belakang (Canary-B, gelombang besar) atau tidak menahan apa pun |
| `SUPERSEDED` | Digantikan pertanyaan atau keputusan lain; teks lamanya disimpan sebagai jejak |

### 10.1 Ringkasan

| ID | Topik | Status | Diselesaikan otomatis oleh Step 16? | Keputusan / pengganti |
| --- | --- | --- | --- | --- |
| `PAT-OQ-008` | Nilai status crosswalk untuk pasien yang berhasil dibuat | `OWNER_APPROVED` | Tidak | `PAT-DEC-034`; `match_status` → `PAT-OQ-027` |
| `PAT-OQ-009` | Preview tetap atau ikut berubah | `OWNER_APPROVED` | Tidak | `PAT-DEC-035` |
| `PAT-OQ-010` | Route dan bentuk request | `OWNER_APPROVED` | Tidak | `PAT-DEC-044` |
| `PAT-OQ-011` | Berhenti atau lanjut saat konflik | `OWNER_APPROVED` | Tidak | `PAT-DEC-036` |
| `PAT-OQ-012` | Mode pemilihan dan batas `limit` | `OWNER_APPROVED` (dengan koreksi) | Tidak | `PAT-DEC-045` |
| `PAT-OQ-013` | Transisi crosswalk historis pada Canary-B | `OWNER_APPROVED` | Tidak | `PAT-DEC-058` |
| `PAT-OQ-014` | 22 baris berisiko pasien ganda | `OWNER_APPROVED` | Tidak | `PAT-DEC-059` |
| `PAT-OQ-015` | Path QR tujuan sudah ditempati berkas lama | `SUPERSEDED` | **Ya** — rekomendasi lama terbukti memblokir 715 pasien | `PAT-OQ-028` |
| `PAT-OQ-016` | Simulasi boleh membaca filesystem | `OWNER_APPROVED` | Sebagian — kebutuhannya diperkuat | `PAT-DEC-046` |
| `PAT-OQ-017` | `is_member`/`is_active` bernilai `false` | `SUPERSEDED` | **Ya** — datanya terjawab | `PAT-DEC-041`, `PAT-OQ-029` |
| `PAT-OQ-018` | Kolom crosswalk yang diisi | `OWNER_APPROVED` (kustom) | Tidak | `PAT-DEC-047` |
| `PAT-OQ-019` | Skema fisik tabel migrasi | `OWNER_APPROVED` | Sebagian | `PAT-DEC-063` |
| `PAT-OQ-020` | Pengaman dari pembangkit nomor normal | `SUPERSEDED` | **Ya** — tabrakan terbukti; deteksi saja tidak cukup | `PAT-OQ-030`, `031`, `032`, `035` |
| `PAT-OQ-021` | Bentuk kunci per pasien | `OWNER_APPROVED` (dengan koreksi) | Tidak | `PAT-DEC-048` |
| `PAT-OQ-022` | Automated test | `OWNER_APPROVED` | Tidak | `PAT-DEC-037` |
| `PAT-OQ-023` | Data pribadi pada respons dan log | `OWNER_APPROVED` | Tidak | `PAT-DEC-049` |
| `PAT-OQ-024` | Tabrakan telepon | `OWNER_APPROVED` | Tidak | `PAT-DEC-050` |
| `PAT-OQ-025` | Pekerjaan frontend | `OWNER_APPROVED` | Tidak | `PAT-DEC-060` |
| `PAT-OQ-026` | Mekanisme gelombang besar | `OWNER_APPROVED` | Tidak | `PAT-DEC-061` |
| `PAT-OQ-027` | Nilai `match_status` | `OWNER_APPROVED` | Tidak | `PAT-DEC-051` |
| `PAT-OQ-028` | **P0** — strategi QR untuk 715 tabrakan namespace | `OWNER_APPROVED` | Tidak — Step 16 yang menjadikannya P0 | `PAT-DEC-052` |
| `PAT-OQ-029` | Mekanisme persistensi `IsMember = false` | `OWNER_APPROVED` | Tidak | `PAT-DEC-053`; bukti provider-real → `PAT-OQ-036` |
| `PAT-OQ-030` | **P0** — perlindungan reservasi `PatientCode` | `OWNER_APPROVED` | Tidak | `PAT-DEC-054`; cakupan environment → `PAT-OQ-037` |
| `PAT-OQ-031` | **P0** — perlindungan reservasi MRN | `OWNER_APPROVED` | Tidak | `PAT-DEC-064` |
| `PAT-OQ-032` | Pemblokiran pembuatan pasien normal di Staging | `OWNER_APPROVED` | Tidak | `PAT-DEC-055` |
| `PAT-OQ-033` | Manifest eksekusi Canary-A | `OWNER_APPROVED` (bersyarat) | Tidak | `PAT-DEC-056` |
| `PAT-OQ-034` | Inventaris filesystem folder QR | `OWNER_APPROVED` | Tidak | `PAT-DEC-062` |
| `PAT-OQ-035` | Penempatan task reservation guard | `OWNER_APPROVED` | Tidak | `PAT-DEC-057` |
| `PAT-OQ-036` | Bentuk bukti persistensi provider-real `IsMember = false` | `OWNER_APPROVED` | Tidak — muncul dari `PAT-DEC-053` | `PAT-DEC-065` |
| `PAT-OQ-037` | Cakupan environment guard reservasi `BE-PAT-MIG-003` | `OWNER_APPROVED` | Tidak — muncul dari `PAT-DEC-054`/`057` | `PAT-DEC-066` |

Jumlah per sesi ketiga 15 Oktober 2026: **27 `OWNER_APPROVED`, 3 `SUPERSEDED`, 0 `BLOCKING`, 0 `PROPOSED`**. Riwayat: amandemen kedua — 23 `OWNER_APPROVED`, 3 `SUPERSEDED`, 4 `BLOCKING`; amandemen pertama — 4 `OWNER_APPROVED`, 3 `SUPERSEDED`, 16 `BLOCKING`, 5 `PROPOSED`.

### 10.2 `PAT-OQ-008` — Nilai status crosswalk untuk pasien yang berhasil dibuat

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Saat worker berhasil membuat pasien, nilai apa yang ditulis pada `match_status` dan `migration_status` baris crosswalk barunya? |
| Mengapa penting | Nilai ini satu-satunya penanda asal-usul pada crosswalk. Audit memakainya untuk menghitung hasil worker dan memisahkannya dari Pilot, dan gerbang `PATIENT_BASELINE` menghitung dari nilai ini |
| Pilihan A | `match_status = TARGET_CREATED`, `migration_status = MIGRATED_BULK_WORKER`. Konsekuensi: arti sukses sama dengan Pilot (`PAT-FACT-062`) tetapi asal-usulnya terpisah; harus dipastikan tidak ada CHECK constraint yang menolak nilai baru (`PAT-OQ-019`) |
| Pilihan B | `MIGRATED_API_AUTO`, sama dengan 710 pasien Pilot. Konsekuensi: tercampur dengan pasien Pilot yang dibuat lewat API normal; audit tidak dapat memisahkan dua asal-usul |
| Pilihan C | Other — nilai atau batasan lain |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak. Pemilik memutuskannya langsung pada 15 Oktober 2026 |
| Status | **`OWNER_APPROVED`** untuk `migration_status = MIGRATED_BULK_WORKER` (`PAT-DEC-034`). Nilai `match_status` tidak disebut pemilik, sehingga dipisah menjadi `PAT-OQ-027` |

### 10.3 `PAT-OQ-009` — Preview tetap atau ikut berubah

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Apakah `target_preview` tetap tidak pernah diubah, atau worker ikut mengubah `validation_status`? |
| Mengapa penting | Preview adalah bukti audit masukan. Mengubahnya berarti mengubah bukti; tidak mengubahnya berarti kemajuan harus dibaca dari tempat lain |
| Pilihan A | Preview tidak pernah diubah; kemajuan dibaca dari crosswalk dan `MstPatient`. Konsekuensi: `validation_status` tetap `PENDING` 704.506 sesudah migrasi dan artinya tetap "status audit preview"; satu operasi tulis lebih sedikit per pasien |
| Pilihan B | Worker mengubah `validation_status`, misalnya menjadi `MIGRATED`. Konsekuensi: kemajuan terbaca dari satu tabel, tetapi bukti audit berubah, nilai baru harus disetujui, dan ada operasi tulis kedua yang dapat gagal sendiri |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-035`): preview immutable sesudah approval; `validation_status` bukan penanda kemajuan; kemajuan dibaca dari crosswalk + `MstPatient` + hasil worker |

### 10.4 `PAT-OQ-010` — Route dan bentuk request

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Route dan bentuk request apa yang dipakai endpoint baru? |
| Mengapa penting | Menentukan kontrak yang dibangun dan cara operator memilih pasien. Daftar pasien yang dikirim klien membuka peluang salah ketik atau penyisipan pasien lain |
| Pilihan A | `POST …/patients/admin/migration/rsmmc-bulk-patients/migrate` dengan body `batchId`, `selectionMode` (`CANARY_A`), `dryRun`, dan `limit`. Daftar `legacy_pid` disimpan di server sebagai manifest eksekusi beku, tidak dikirim klien; respons menyebut sidik jari manifest. Konsekuensi: operator tidak dapat menyisipkan pasien; mengganti pasien butuh rilis source yang direview — sejalan dengan larangan penggantian otomatis |
| Pilihan B | Sama, tetapi daftar `legacyPids` dikirim di body dan server menolak bila tidak persis sama dengan manifest beku. Konsekuensi: operator melihat daftarnya pada request, tetapi ada dua sumber daftar yang harus dijaga tetap sama |
| Pilihan C | Other — misalnya controller terpisah. Konsekuensi: controller terpisah tidak dapat memakai fungsi format QR milik `PatientController` yang bersifat `private` tanpa mengubah legacy |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak. Isi konstanta manifest di server kini menunggu `PAT-OQ-033` |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-044`). Respons wajib menyebut identifier, versi, dan sidik jari manifest |

### 10.5 `PAT-OQ-011` — Berhenti atau lanjut saat konflik

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bila eksekusi menemui konflik atau kegagalan, apakah proses berhenti atau lanjut ke pasien berikutnya? |
| Mengapa penting | Menentukan berapa banyak kerusakan yang dapat terjadi sebelum manusia melihat masalahnya |
| Pilihan A | Simulasi menilai seluruh pasien terpilih dan tidak pernah berhenti. Eksekusi berhenti pada `CONFLICT`, `FAILED`, atau *commit* tidak pasti yang pertama. `ALREADY_MIGRATED` tidak menghentikan proses dan tidak menghabiskan `limit`. Konsekuensi: aman untuk canary; pada gelombang besar proses dapat sering berhenti |
| Pilihan B | Eksekusi melewati pasien bermasalah dan melanjutkan sisanya. Konsekuensi: lebih cepat, tetapi satu penyebab umum — misalnya folder QR tidak dapat ditulis — dapat menghasilkan ratusan kegagalan sebelum ada yang melihat |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** untuk eksekusi Canary-A (`PAT-DEC-036`): berhenti pada `CONFLICT` atau `FAILED` tak terduga yang pertama; hasil sudah-dimigrasi/idempoten bukan kegagalan tak terduga. Bagian "simulasi tidak pernah berhenti" tetap usulan kontrak dan ikut `PAT-DEC-033`. Kebijakan gelombang besar tetap terbuka (`PAT-OQ-026`). Tafsiran yang dicatat untuk dikonfirmasi lewat `PAT-DEC-033`: `MIGRATED_QR_INCONSISTENT` adalah hasil sudah-dimigrasi yang dilaporkan, bukan `CONFLICT`/`FAILED`, sehingga tidak menghentikan proses |

### 10.6 `PAT-OQ-012` — Mode pemilihan dan batas `limit`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Mode pemilihan apa yang dibangun pada build pertama, dan berapa batas `limit`? |
| Mengapa penting | Membatasi kerusakan maksimum per panggilan, dan mencegah kemampuan gelombang besar hadir sebelum canary terbukti |
| Pilihan A | Hanya `selectionMode = CANARY_A`; `limit` 1–25, bawaan 25; urutan `legacy_pid` naik secara angka Int64; hanya pasien yang akan dibuat yang menghabiskan `limit`; mode lain ditolak `400`. Konsekuensi: Canary-B dan gelombang besar butuh rilis tambahan |
| Pilihan B | `limit` 1–100 seperti Pilot, dan mode gelombang umum sudah dibangun tetapi dikunci konfigurasi. Konsekuensi: lebih sedikit rilis nanti, tetapi kemampuan memproses ribuan pasien sudah ada di Staging sebelum canary terbukti |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak. Batas atas 25 mengikuti ukuran manifest; bila manifest eksekusi `PAT-OQ-033` berubah ukuran, batasnya ikut |
| Status | **`OWNER_APPROVED`** — pilihan A **dengan koreksi** (`PAT-DEC-045`): hanya `CANARY_A`; `limit` 1–25, bawaan 25; untuk `CANARY_A` urutan eksekusi persis urutan manifest server yang disetujui, **tanpa** sort ulang berdasarkan `legacy_pid`. Urutan naik Int64 pada teks pilihan A hanya berlaku bagi mode pemilihan umum di task kemudian; contoh urutan naik pada draft sebelumnya tidak berlaku lagi untuk `CANARY_A` |

### 10.7 `PAT-OQ-013` — Transisi crosswalk historis pada Canary-B

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Saat Canary-B membuat pasien untuk baris crosswalk historis yang target-nya NULL, kolom historis mana yang boleh berubah? |
| Mengapa penting | 285 baris itu bukti review historis yang tidak boleh dihancurkan (`PAT-DEC-031`) |
| Pilihan A | Pertahankan `match_status`, `migration_status`, `review_reason`, dan `last_batch_id`; isi hanya `quilvian_patient_id` dan `updated_at`. Konsekuensi: bukti utuh; kolom status tetap berbunyi misalnya `REVIEW_SCOPE` walau pasien sudah dibuat, sehingga audit worker menghitung dari `quilvian_patient_id` |
| Pilihan B | Pemilik membuat snapshot bukti 285 baris yang tidak boleh diubah, lalu worker mengubah status ke nilai provenance dan `last_batch_id` ke batch kanonik, dengan `review_reason` dipertahankan. Konsekuensi: status mencerminkan keadaan nyata, tetapi butuh operasi database terpisah yang berwenang |
| Pilihan C | Other |
| Rekomendasi | A sebagai titik awal |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A untuk Canary-B kelak (`PAT-DEC-058`) |

### 10.8 `PAT-OQ-014` — 22 baris berisiko pasien ganda pada Canary-B

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Apakah 22 baris historis berisiko pasien ganda — 16 `DUPLICATE_CANDIDATE`, 4 `MERGE_CANDIDATE`, 2 `REVIEW_NEWBORN` — ikut Canary-B? |
| Mengapa penting | Membuat pasien untuk kandidat duplikat dapat menghasilkan dua rekam medis untuk satu orang. Itu masalah keselamatan pasien, bukan sekadar kerapian data |
| Pilihan A | Ditahan sampai pemilik proses identitas pasien meninjau alasannya satu per satu. Konsekuensi: 22 pasien tertunda lebih lama |
| Pilihan B | Ikut Canary-B karena normalisasi terbaru menyatakannya `READY_FOR_TARGET_PREVIEW`. Konsekuensi: risiko rekam medis ganda |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-059`): 22 baris tetap dikecualikan sampai review per baris |

### 10.9 `PAT-OQ-015` — Path QR tujuan sudah ditempati berkas lama (digantikan)

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bila berkas QR tujuan sudah ada — misalnya QR lama Pilot — apa yang dilakukan worker? |
| Mengapa penting | QR lama Pilot tidak boleh disentuh, dan berkas tujuan yang sudah ada tidak boleh ditimpa |
| Pilihan A (lama) | `CONFLICT` jenis `QR_ARTIFACT_ALREADY_EXISTS`; pasien tidak dibuat; berkas lama tidak disentuh; kebijakan populasi besar diputuskan sebelum gelombang besar. Konsekuensi yang kini terbukti: 715 pasien sah terblokir permanen |
| Pilihan B (lama) | Memakai berkas yang sudah ada bila isinya persis sama dengan PNG yang akan dibentuk. Konsekuensi: mengadopsi berkas yang tidak dibuat pemanggilan ini |
| Pilihan C (lama) | Other — misalnya nama berkas lain |
| Rekomendasi lama | A |
| Diselesaikan Step 16? | **Ya.** Step 16 membuktikan seluruh 715 MRN sementara Pilot dibutuhkan tepat 715 pasien bulk (`PAT-FACT-070`), sehingga `CONFLICT` generik akan memblokir 715 pasien sah secara permanen |
| Status | **`SUPERSEDED`** oleh `PAT-OQ-028`. `CONFLICT` tetap dipakai sebagai perilaku gagal tertutup untuk berkas tak dikenal pada path strategi yang dipilih |

### 10.10 `PAT-OQ-016` — Simulasi boleh membaca filesystem

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bolehkah simulasi membaca filesystem untuk memeriksa apakah berkas QR tujuan sudah ada? |
| Mengapa penting | Tanpa itu, konflik QR baru terlihat saat eksekusi dan memaksa proses berhenti. Dengan itu, bukti simulasi yang ditinjau pemilik lebih lengkap |
| Pilihan A | Simulasi boleh **membaca** keberadaan berkas QR tujuan tanpa membuat folder atau berkas, memakai penentu lokasi yang tidak membuat folder (`PAT-SRC-015`). Konsekuensi: butuh komponen pemeriksa baca-saja; `PAT-DEC-028` hanya melarang menulis |
| Pilihan B | Simulasi tidak menyentuh filesystem sama sekali, seperti Pilot. Konsekuensi: konflik QR baru terlihat saat eksekusi, lalu proses berhenti |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Sebagian — kebutuhannya diperkuat: 10 dari 25 kandidat Canary-A menyentuh namespace QR historis (`PAT-FACT-071`). Pemeriksa juga dapat melaporkan keberadaan QR historis di path bawaan sebagai informasi |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-046`): boleh membaca keberadaan, metadata berkas, keadaan path, dan hash bila desain membutuhkannya; dilarang membuat folder dan membuat, mengubah, atau menghapus berkas |

### 10.11 `PAT-OQ-017` — `is_member`/`is_active` bernilai `false` (digantikan)

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana menangani baris preview dengan `is_member` atau `is_active` bernilai `false`, mengingat EF menyimpan `false` sebagai `true` untuk dua kolom itu? |
| Mengapa penting | Data pasien tersimpan salah tanpa ada galat |
| Pilihan A (lama) | Verifikasi baca ulang sebelum *commit*, ditambah pemilik menghitung sebaran data lebih dulu. Konsekuensi yang kini terbukti: karena 704.506 baris `is_member = false`, verifikasi saja akan menggagalkan **setiap** pasien |
| Pilihan B (lama) | Mengubah konfigurasi EF `MstPatient` supaya `false` selalu dikirim |
| Pilihan C (lama) | Insert `MstPatient` lewat SQL berparameter |
| Pilihan D (lama) | Other |
| Rekomendasi lama | A |
| Diselesaikan Step 16? | **Ya** untuk datanya: seluruh 704.506 baris `IsMember = false` dan `IsActive = true` (`PAT-FACT-074`). **Koreksi:** `is_active = false` bukan kebutuhan bulk; masalah nyatanya hanya `IsMember = false` |
| Status | **`SUPERSEDED`** oleh `PAT-DEC-041` (fakta dan kewajiban) dan `PAT-OQ-029` (mekanisme) |

### 10.12 `PAT-OQ-018` — Kolom crosswalk yang diisi untuk baris baru

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Kolom crosswalk mana yang diisi untuk baris baru? |
| Mengapa penting | Mengisi kolom tanpa sumber yang disetujui berarti mengarang data, dan `normalized_nik` membuka jalan pencocokan berbasis NIK |
| Pilihan A | Hanya `legacy_pid`, `quilvian_patient_id`, `match_status` (`PAT-OQ-027`), `migration_status` (`PAT-DEC-034`), `last_batch_id` = batch kanonik, dan `created_at` = waktu database. Kolom lain NULL: `canonical_legacy_pid`, `legacy_old_rm`, `normalized_mrn`, `normalized_nik`, `review_reason`, `updated_at`. Konsekuensi: tidak ada inferensi identitas; kolom `normalized_*` kosong untuk baris worker |
| Pilihan B | Juga mengisi `canonical_legacy_pid = legacy_pid` dan `legacy_old_rm` dari sumber mentah. Konsekuensi: lebih lengkap, tetapi butuh sumber baca dan aturan yang belum disetujui |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan kustom pemilik (`PAT-DEC-047`), **berbeda** dari rekomendasi A: `legacy_old_rm`, `normalized_mrn`, dan `normalized_nik` diisi dari kolom preview `legacy_old_rm`, `legacy_normalized_mrn`, `legacy_normalized_nik`; `canonical_legacy_pid`, `review_reason`, dan `updated_at` tetap NULL pada insert awal. `normalized_nik` hanya bukti, tidak pernah dipakai mencocokkan pasien |

### 10.13 `PAT-OQ-019` — Skema fisik tabel migrasi

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Skema fisik tabel migrasi apa yang dipakai build? |
| Mengapa penting | Tanpa tipe kolom dan constraint, SQL harus meng-cast ke `text` sehingga lambat, dan nilai baru dapat ditolak CHECK constraint |
| Pilihan A | Pemilik menyerahkan keluaran read-only — `\d+` tiap tabel dan query distribusinya — sebelum build. Konsekuensi: parameter bertipe, indeks terpakai |
| Pilihan B | Build memakai cast `::text` seperti Pilot. Konsekuensi: lambat dan tidak siap untuk gelombang besar (`PAT-SRC-010`) |
| Pilihan C | Other — misalnya agent diberi otorisasi baca database terpisah |
| Rekomendasi | A |
| Diselesaikan Step 16? | Sebagian. Sudah terjawab: `legacy_pid` bersemantik bigint bertanda / Int64 (`PAT-DEC-038`) dan sebaran boolean preview (`PAT-FACT-074`, `075`). Masih dibutuhkan: tipe `batch_id` dan kolom enum pada preview/perencana; cara preview menyimpan enum (angka atau nama); kunci unik `(batch_id, legacy_pid)`; CHECK constraint pada `match_status`/`migration_status`; letak kelompok sumber dan status normalisasi; keberadaan batch kanonik di `migration.rsmmc_patient_batch`; apakah dua `legacy_pid` tertunda punya baris crosswalk |
| Status | **`OWNER_APPROVED`** — sesi ketiga 15 Oktober 2026 (`PAT-DEC-063`): skema fisik `PAT-FACT-079`–`083` diterima; SQL bertipe, tanpa fallback `::text`. *Riwayat: `BLOCKING` sampai sesi kedua* |

### 10.14 `PAT-OQ-020` — Pengaman dari pembangkit nomor normal (digantikan)

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana melindungi kode dan MRN yang dicadangkan preview dari pembangkit nomor normal? |
| Mengapa penting | Pembangkit normal memakai pola "nomor kosong terkecil" (`PAT-SRC-006`) |
| Pilihan A (lama) | Gerbang deteksi di setiap panggilan worker, ditambah jeda pendaftaran di Staging selama migrasi. Konsekuensi yang kini terbukti: deteksi hanya mengetahui sesudah reservasi terpakai; ia tidak mencegah |
| Pilihan B (lama) | Mengubah pembangkit normal agar melompati rentang cadangan |
| Pilihan C (lama) | Other |
| Rekomendasi lama | A |
| Diselesaikan Step 16? | **Ya.** Tabrakan terbukti untuk `PatientCode` (742) dan MRN (27) (`PAT-FACT-072`, `073`), dan pemilik mewajibkan perlindungan yang tahan lama (`PAT-DEC-042`) |
| Status | **`SUPERSEDED`** oleh `PAT-OQ-030`, `031`, `032`, dan `035`. Gerbang deteksi dari pilihan A lama tetap dipakai sebagai lapis tambahan |

### 10.15 `PAT-OQ-021` — Bentuk kunci per pasien

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bentuk kunci database per pasien apa yang dipakai? |
| Mengapa penting | Kunci mencegah dua proses mengerjakan pasien yang sama bersamaan; bentuknya menentukan apakah permintaan menunggu atau langsung berhenti |
| Pilihan A | `pg_try_advisory_xact_lock(hashtext('PAT-MIG-002\|<batchId>\|<legacy_pid>'))` di dalam transaksi pasien. Bila kunci sedang dipegang proses lain → `CONFLICT` jenis `CONCURRENT_RUN`, proses berhenti tanpa menunggu. Konsekuensi: tidak ada permintaan yang menggantung; dua proses bersamaan menghasilkan konflik yang terlihat |
| Pilihan B | `pg_advisory_xact_lock` yang menunggu, persis pola `AccJournalService`. Konsekuensi: mengikuti pola repository apa adanya, tetapi permintaan HTTP dapat menunggu tanpa batas bila transaksi lain macet |
| Pilihan C | Other. `SELECT … FOR UPDATE` saja tidak cukup, karena pada jalur "crosswalk belum ada" tidak ada baris yang dapat dikunci |
| Rekomendasi | A. Catatan: `hashtext` menghasilkan angka 32-bit, sehingga dua kunci berbeda dapat jatuh ke angka yang sama — akibatnya hanya "sibuk palsu" yang jarang, bukan data salah |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — try-lock database **dengan koreksi** (`PAT-DEC-048`): bukan satu `hashtext` 32-bit; `pg_try_advisory_xact_lock(key1, key2)` dua kunci, key1 = namespace tetap PAT-MIG-002/batch kanonik, key2 = `legacy_pid` bertanda bila aman masuk Int32 dengan konversi yang divalidasi; selain itu desain 64-bit yang direview. Kontensi → `CONCURRENT_RUN` tanpa menunggu tanpa batas. Usulan nilai key1: kontrak Bagian B4.3 langkah 2 |

### 10.16 `PAT-OQ-022` — Automated test

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Apakah `BE-PAT-MIG-002` wajib disertai automated test? |
| Mengapa penting | `TEST_POLICY` melarang test kecuali diminta eksplisit, sedangkan jalur gagal kasus A–J dan persistensi `IsMember` sulit dibuktikan tanpa test |
| Pilihan A | Test terfokus di project yang sudah ada, `Tests/QuilvianSystemBackend.PatientManagementTests/`, dicantumkan sebagai acceptance criteria eksplisit; tanpa entri `.sln` dan tanpa workflow CI baru. Konsekuensi: memenuhi `TEST_POLICY` bagian 3; SQLite tidak dapat membuktikan advisory lock maupun SQL PostgreSQL |
| Pilihan B | Tanpa automated test. Konsekuensi: bukti hanya build, QBE, penelusuran source, dan simulasi |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — wajib (`PAT-DEC-037`), termasuk test persistensi terfokus `IsMember = false`. Lokasi mengikuti preseden `PAT-OQ-001`: project test PatientManagement yang sudah ada |

### 10.17 `PAT-OQ-023` — Data pribadi pada respons dan log

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Data pribadi apa yang boleh muncul pada respons dan log? |
| Mengapa penting | Preview memuat NIK, telepon, dan alamat; respons dan log tidak boleh membocorkannya |
| Pilihan A | Respons per pasien memuat `legacyPid`, `PatientCode`, MRN, path QR, status, dan `fullName`; NIK, telepon, dan alamat hanya ditampilkan sebagai tanda ada atau tidak. Log tanpa nama, NIK, telepon, dan alamat. Konsekuensi: pemilik tetap dapat meninjau bukti simulasi dengan nama |
| Pilihan B | Tanpa nama sama sekali. Konsekuensi: lebih privat, tetapi peninjauan bukti lebih sulit |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-049`); log aplikasi juga tidak boleh memuat `fullName` |

### 10.18 `PAT-OQ-024` — Tabrakan telepon

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bila telepon preview sudah dipakai pasien lain, apakah pasien tetap dibuat? |
| Mengapa penting | Audit pemilik memasukkan telepon sebagai dimensi tabrakan (`PAT-FACT-044`), sedangkan database tidak mewajibkan telepon unik |
| Pilihan A | `CONFLICT` jenis `PHONE_ALREADY_USED`; pasien tidak dibuat. Konsekuensi: syarat "tetap bebas tabrakan" terjaga; pasien dapat tertahan karena nomor keluarga yang dipakai bersama |
| Pilihan B | Hanya peringatan; pasien tetap dibuat. Konsekuensi: tidak ada pasien tertahan, tetapi telepon ganda masuk |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-050`): telepon `SAFE_TO_STORE` yang bergeser dan kini dipakai pasien lain → `PHONE_ALREADY_USED`, pasien tidak dibuat; nilai preview tidak boleh dibuang atau diubah diam-diam |

### 10.19 `PAT-OQ-025` — Pekerjaan frontend

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Apakah `BE-PAT-MIG-002` membutuhkan pekerjaan frontend? |
| Mengapa penting | Menentukan apakah ada task frontend dan kontrak tampilan |
| Pilihan A | Tidak ada; endpoint admin tanpa layar. Konsekuensi: operator memakai Swagger atau klien HTTP |
| Pilihan B | Ada layar pemantau migrasi. Konsekuensi: task frontend baru dan kontrak tampilan |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak. Impact scan 15 Oktober 2026 menambah bukti: konsumen frontend memakai `qrCodePath` tersimpan (`PAT-SRC-020`), sehingga strategi QR `PAT-OQ-028` pilihan A tidak menuntut perubahan frontend |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-060`) |

### 10.20 `PAT-OQ-026` — Mekanisme gelombang besar

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana mekanisme gelombang besar — ukuran, cadence, dan cara menjalankannya? |
| Mengapa penting | 704.506 pasien tidak dapat dijalankan dengan pola canary; butuh keputusan waktu, kapasitas, dan cara jalan |
| Pilihan A | Ditunda ke `PAT-GATE-004`, sesudah Canary-A dan Canary-B lulus. Saat itu diputuskan: panggilan HTTP berulang atau *hosted worker*; ukuran gelombang; kapasitas penyimpanan ±704 ribu folder QR; dan jendela waktu. Konsekuensi: build pertama tetap kecil |
| Pilihan B | Diputuskan sekarang. Konsekuensi: keputusan diambil tanpa data canary |
| Pilihan C | Other |
| Rekomendasi | A. Gambaran skala, ilustrasi yang belum diukur: bila satu pasien butuh 0,1 detik, 704.506 pasien butuh ±20 jam kerja bersih |
| Diselesaikan Step 16? | Tidak. Step 16 menambah syarat: reservation guard final (`PAT-OQ-030`, `031`) wajib ada sebelum gelombang besar |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-061`), dengan syarat pembuka `PAT-GATE-004`: Canary-A `PASS`, eksekusi ulang idempoten `PASS`, Canary-B `PASS`, inventaris filesystem, review penyimpanan/kapasitas, dan verifikasi reservation guard |

### 10.21 Pertanyaan baru dari Step 16

#### `PAT-OQ-027` — Nilai `match_status`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Nilai `match_status` apa yang ditulis untuk baris crosswalk pasien yang berhasil dibuat worker? |
| Mengapa penting | `PAT-DEC-034` hanya mengunci `migration_status`, sedangkan `match_status` adalah kolom `NOT NULL` yang juga harus diisi |
| Pilihan A | `TARGET_CREATED`, sama dengan arti sukses Pilot (`PAT-FACT-062`). Konsekuensi: pencarian "pasien punya target" konsisten untuk Pilot dan bulk |
| Pilihan B | Nilai baru, misalnya `TARGET_CREATED_BULK`. Konsekuensi: asal-usul tercatat di dua kolom; tidak perlu karena `migration_status` sudah membedakannya |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-051`) |

#### `PAT-OQ-028` — P0: strategi QR untuk 715 tabrakan namespace QR historis

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Di mana dan bagaimana QR pasien bulk dibuat, bila 715 di antaranya punya MRN yang foldernya sudah berisi QR historis Pilot yang wajib dibiarkan utuh? |
| Mengapa penting | Isu desain **P0** (`PAT-P0-001`, `PAT-DEC-043`). `CONFLICT` generik memblokir 715 pasien sah secara permanen; menimpa atau menghapus melanggar `PAT-DEC-039`; pemakaian ulang tidak boleh dipilih diam-diam |
| Bukti impact scan | Konsumen memakai `QrCodePath` yang tersimpan dan tidak menyusun path dari MRN: backend hanya mengembalikan nilai kolom (`PAT-SRC-019`); frontend mengubah `qrCodePath` dari API menjadi URL atau membuat QR sendiri dari MRN (`PAT-SRC-020`); folder upload disajikan seluruhnya sehingga subfolder baru dapat diakses (`PAT-SRC-021`); penyimpan QR `BE-PAT-MIG-001` dapat dipakai apa adanya dengan folder dasar lain (`PAT-SRC-022`) |
| Pilihan A — artefak bulk di namespace yang tidak bertabrakan (diminta pemilik untuk dievaluasi pertama) | **Seluruh** pasien bulk — bukan hanya 715 — mendapat QR di `patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png`. Isi QR tetap MRN berformat; `QrCodePath` menunjuk path baru; QR historis tidak disentuh byte demi byte. Dibuat dengan `RsmmcPilotQrArtifactStore` apa adanya memakai folder dasar `patient-qrcodes/rsmmc-bulk`. `CONFLICT` tetap berlaku bila path namespace sudah terisi (gagal tertutup). Konsekuensi: syarat pemilik terbukti terpenuhi oleh scan; satu aturan untuk semua pasien bulk tanpa percabangan; path pasien bulk berbeda dari pola pasien normal (`patient-qrcodes/<MRN>/`); untuk 715 MRN akan ada dua berkas QR berisi sama (historis dan bulk); skrip operasional di luar repository yang mungkin mengandaikan pola lama tidak diperiksa. Varian nama: A1 per MRN seperti di atas (deterministik, terbaca manusia); A2 per `legacy_pid` (deterministik, pasti unik, tetapi tidak terbaca per MRN); A3 per `PatientId` (berubah tiap percobaan, sehingga QR yatim dari percobaan gagal tidak lagi terdeteksi) |
| Pilihan B — `VERIFIED_HISTORICAL_QR_REUSE` / adopsi | Untuk 715 tabrakan, `QrCodePath` pasien bulk menunjuk berkas QR historis yang sudah ada, **hanya** bila seluruh aturan berikut lulus: (1) asal-usul — path sama dengan path QR lama satu baris tabel cadangan Pilot yang MRN lamanya sama dengan MRN target; (2) tidak dirujuk `QrCodePath` pasien mana pun; (3) berkas ada, berkas biasa, ukuran > 0; (4) SHA-256 berkas sama dengan inventaris hash yang dibuat dan disetujui pemilik sebelum migrasi, dan isi QR pada inventaris itu sudah didekode di luar aplikasi sama dengan MRN berformat — path yang memuat MRN **bukan** bukti; (5) worker tidak pernah mengubah berkas; (6) jejak adopsi — path, SHA-256, rujukan inventaris — disimpan tahan lama. Konsekuensi: berkas bukti historis menjadi artefak hidup, sehingga alur lain yang menulis QR di folder MRN (pembuatan normal memakai `File.WriteAllBytes`) dapat menimpanya kelak; mendekode QR di aplikasi butuh dependency baru karena QRCoder hanya membuat QR, dan itu di luar wewenang; belum ada kolom tahan lama untuk jejak adopsi tanpa perubahan skema; dua jalur logika untuk pasien bulk |
| Pilihan C | Tunda 715 tabrakan dari seluruh gelombang sampai strategi lain diputuskan. Konsekuensi: bukan strategi final; 715 pasien tetap tidak termigrasi |
| Pilihan D | Other |
| Rekomendasi | **A, varian A1** — syarat pemilik terbukti terpenuhi oleh scan, artefak historis tetap menjadi bukti yang tidak dirujuk siapa pun, dan komponen QR `BE-PAT-MIG-001` dipakai ulang tanpa perubahan kode |
| Diselesaikan Step 16? | Tidak. Step 16 yang menjadikannya P0 |
| Status | **`OWNER_APPROVED`** — pilihan A, varian A1 (`PAT-DEC-052`), untuk **seluruh** pasien bulk. Tanpa adopsi QR historis; berkas tak dikenal di path `rsmmc-bulk` = konflik; simulasi memeriksa path baru dan melaporkan path bawaan historis sebagai informasi |

#### `PAT-OQ-029` — Mekanisme persistensi `IsMember = false`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana worker memastikan `IsMember = false` dari preview benar-benar tersimpan `false` di `MstPatient`? |
| Mengapa penting | Seluruh 704.506 baris preview `IsMember = false` (`PAT-FACT-074`), sedangkan EF tidak mengirim `false` sehingga database mengisi `true` (`PAT-SRC-008`, `PAT-FACT-075`). Tanpa mekanisme, setiap pasien tersimpan salah — atau, dengan verifikasi baca ulang, setiap pasien gagal |
| Pilihan A | Penulisan eksplisit di worker: sesudah insert EF, di transaksi yang sama, satu `ExecuteUpdate` berparameter menyetel `IsMember = false` untuk `Id` pasien yang baru dibuat; lalu baca ulang tanpa *tracking* dan bandingkan seluruh kolom asal preview. Konsekuensi: konfigurasi EF dan pendaftaran normal tidak berubah; ada satu pernyataan `UPDATE` tambahan atas baris yang baru di-insert di transaksi yang sama — bukan atas pasien lama |
| Pilihan B | Perubahan konfigurasi EF terkecil: `IsMember` diberi `.ValueGeneratedNever()`; default database tetap ada. Konsekuensi: EF selalu mengirim nilai. Pendaftaran normal yang tidak mengirim `isMember` (DTO bawaan `false`, `PAT-SRC-024`) yang sekarang tersimpan `true` akan tersimpan `false` — perubahan perilaku kiosk dan admin; model snapshot berubah; risiko pemeriksaan *pending model changes* EF 9 saat `database update` harus dibuktikan saat build |
| Pilihan C | Insert seluruh kolom `MstPatient` lewat SQL berparameter. Konsekuensi: melewati konvensi EF untuk tabel inti pasien; rapuh terhadap perubahan skema |
| Pilihan D | Other |
| Rekomendasi | A. Dibuktikan oleh test persistensi wajib (`PAT-DEC-037`) dan audit fisik pada Canary-A |
| Diselesaikan Step 16? | Tidak. Step 16 menjawab datanya, bukan mekanismenya |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-053`): konfigurasi EF global tidak diubah; update eksplisit baris baru, baca ulang `AsNoTracking`, verifikasi `IsMember = false`, `IsActive = true`, dan seluruh kolom preview, rollback bila berbeda. Bukti persistensi provider-real (PostgreSQL) **wajib**; SQLite saja tidak cukup. Cara menghasilkan bukti itu belum diputuskan → `PAT-OQ-036` |

#### `PAT-OQ-030` — P0: perlindungan reservasi `PatientCode`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Mekanisme tahan lama apa yang mencegah pembuatan pasien normal memakai `PatientCode` yang dicadangkan preview — 742 sampai 705.249, termasuk `PAT-RSMMC-101416` dan `PAT-RSMMC-619077`? |
| Mengapa penting | Isu desain **P0** (`PAT-P0-002`). Pembangkit normal memilih `PAT-RSMMC-00742`, reservasi pertama (`PAT-FACT-072`). Sesudah migrasi penuh, kode kosong terkecil adalah `PAT-RSMMC-101416` — cadangan pasien tertunda. Kunci di memori proses saja tidak cukup (`PAT-DEC-042`) |
| Pilihan A | Alokasi normal yang sadar reservasi: allocator di module service, di bawah `pg_advisory_xact_lock`, memilih angka terkecil yang tidak dipakai `MstPatient` dan tidak dicadangkan (preview kanonik + kode cadangan tertunda), dihitung di SQL. Konsekuensi: pendaftaran normal bergantung pada tabel `migration.*` dan harus gagal tertutup bila tabel itu tidak terbaca. Karena rentang cadangan utuh 742–705.249 (`PAT-FACT-042`), hasilnya sama dengan pilihan B |
| Pilihan B | Lantai alokasi tahan lama: allocator normal tidak pernah memberi angka di bawah 705.250; memilih angka terkecil ≥ lantai yang belum dipakai, di bawah `pg_advisory_xact_lock`, dengan indeks unik sebagai jaring terakhir. Konsekuensi: hasil sama dengan A tanpa membaca tabel migrasi saat runtime; kode pasien normal berikutnya `PAT-RSMMC-705250`; nilai lantai harus disimpan tahan lama — konfigurasi atau data; tabel deret baru berarti perubahan skema |
| Pilihan C | Hanya pemblokiran pembuatan pasien (`PAT-OQ-032`). Konsekuensi: perlindungan runtime, bukan strategi final — pemilik menolaknya sebagai satu-satunya mekanisme |
| Pilihan D | Other — misalnya trigger database `BEFORE INSERT` pada `MstPatient` yang menolak kode cadangan kecuali dari worker. Konsekuensi: menjaga seluruh jalur insert termasuk SQL langsung, tetapi pola baru di repository (tidak ada trigger di `Migrations/`, `PAT-SRC-025`) dan butuh migration |
| Rekomendasi | B, ditambah C selama jendela canary. Penempatannya mengikuti `PAT-OQ-035` |
| Diselesaikan Step 16? | Tidak. Step 16 membuktikan masalahnya |
| Status | **`OWNER_APPROVED`** — pilihan B (`PAT-DEC-054`): lantai suffix normal 705.250 di module service, menggantikan pembangkit controller, dipakai seluruh jalur pembuatan normal, diserialkan di database, indeks unik sebagai pengaman terakhir; diimplementasikan di `BE-PAT-MIG-003` (`PAT-DEC-057`). Cakupan environment-nya belum diputuskan → `PAT-OQ-037` |

#### `PAT-OQ-031` — P0: perlindungan reservasi MRN

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Mekanisme tahan lama apa yang mencegah pembuatan pasien normal memakai MRN akhir `FINALIZED` perencana, termasuk MRN dua pasien tertunda? |
| Mengapa penting | Isu desain **P0** (`PAT-P0-003`). Pembangkit normal memilih `00-00-00-27`, yang dicadangkan (`PAT-FACT-073`). Inferensi kuat `PAT-INF-001`: `00-00-00-27` kemungkinan besar MRN sementara Pilot, sehingga pembuatan pasien normal hari ini juga akan **menimpa** QR historis Pilot di folder itu, karena `SavePatientQrCodeFile` memakai `File.WriteAllBytes` (`PAT-SRC-016`) — melanggar `PAT-DEC-039`. Worker sendiri tetap tidak membangkitkan MRN |
| Pilihan A | Alokasi MRN normal yang sadar reservasi: angka terkecil yang tidak dipakai `MstPatient` dan bukan MRN akhir `FINALIZED` batch kanonik — seluruh 705.223 baris, termasuk Pilot dan tertunda — dihitung di SQL di bawah `pg_advisory_xact_lock`. Konsekuensi: tidak ada nomor yang terbuang; pendaftaran normal bergantung pada tabel perencana dan harus gagal tertutup bila tabel itu tidak terbaca |
| Pilihan B | Lantai MRN: allocator normal tidak pernah memberi angka ≤ MRN akhir numerik tertinggi perencana. Konsekuensi: sederhana, tetapi MRN cadangan tersebar (608.781 mempertahankan nomor lama, `PAT-FACT-007`); bila MRN tertinggi mendekati 99.999.999, ruang nomor normal sempit. Butuh data pemilik: MRN akhir numerik tertinggi |
| Pilihan C | Hanya pemblokiran pembuatan pasien (`PAT-OQ-032`). Konsekuensi: perlindungan runtime saja |
| Pilihan D | Other — misalnya trigger database, dengan konsekuensi yang sama seperti `PAT-OQ-030` pilihan D |
| Rekomendasi | A, kecuali data pemilik menunjukkan MRN akhir tertinggi cukup rendah sehingga B aman. Pemilik diminta menyerahkan secara read-only: MRN akhir numerik tertinggi, dan konfirmasi apakah `00-00-00-27` termasuk 715 MRN sementara Pilot. Ditambah C selama jendela canary. Penempatannya mengikuti `PAT-OQ-035` |
| Diselesaikan Step 16? | Tidak. Step 16 membuktikan masalahnya |
| Status | **`OWNER_APPROVED`** — sesi ketiga 15 Oktober 2026 (`PAT-DEC-064`): pilihan B, lantai MRN numerik 890.188 (`00-89-01-88`), berdasarkan bukti `PAT-FACT-084` dan `089`. *Riwayat: `BLOCKING` sampai sesi kedua* |

#### `PAT-OQ-032` — Pemblokiran pembuatan pasien normal di Staging

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana pembuatan pasien normal di Staging dihentikan selama jendela canary, dan sejak kapan? |
| Mengapa penting | Sampai `PAT-OQ-030`/`031` terpasang, **setiap** pendaftaran normal di Staging memakai reservasi dan — menurut `PAT-INF-001` — dapat menimpa QR historis Pilot. Pemilik menyatakan pemblokiran selama jendela canary boleh diwajibkan, tetapi bukan strategi final |
| Pilihan A | Segera, tanpa menunggu build: pemilik membekukan pendaftaran pasien normal di Staging secara operasional, misalnya mencabut sementara izin `Patient : Create` dari seluruh peran lewat layar Akses Role. Saat build, gerbang worker `NORMAL_ALLOCATION_GUARD` menolak eksekusi bila guard reservasi belum aktif. Konsekuensi: tanpa perubahan kode pendaftaran untuk jendela canary; bergantung disiplin operasional; memengaruhi semua pengguna Staging |
| Pilihan B | Saklar pemeliharaan tahan lama di kode: seluruh jalur pembuatan `MstPatient` (`CreatePatient` beserta rute kiosk dan admin) memeriksa penanda migrasi aktif dan menolak permintaan. Konsekuensi: perlindungan ditegakkan kode; menyentuh jalur pendaftaran normal; penandanya harus tahan lama |
| Pilihan C | Tanpa pemblokiran; menunggu `PAT-OQ-030`/`031` selesai. Konsekuensi: Canary-A runtime menunggu guard final; reservasi tetap terbuka sampai saat itu |
| Pilihan D | Other |
| Rekomendasi | A segera; B hanya bila guard `PAT-OQ-030`/`031` tidak selesai sebelum runtime Canary-A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-055`): pendaftaran normal di Staging, termasuk admin dan kiosk, dibekukan sekarang sampai guard reservasi dideploy dan diverifikasi; bukan strategi final; Produksi tidak terdampak. `NORMAL_ALLOCATION_GUARD` tetap menolak eksekusi kecuali `BE-PAT-MIG-003` aktif dan terverifikasi. **Catatan agent:** contoh "mencabut izin `Patient : Create` lewat layar Akses Role" pada pilihan A tidak cukup, karena `POST /` dan `POST /kiosk` tidak dijaga izin itu (`PAT-SRC-027`); pembekuan harus menutup ketiga jalur |

#### `PAT-OQ-033` — Manifest eksekusi Canary-A

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Bagaimana manifest eksekusi Canary-A dibekukan ulang, mengingat 10 dari 25 kandidat bertabrakan dengan namespace QR historis? |
| Mengapa penting | Daftar 25 hanyalah kandidat draft (`PAT-DEC-040`). Canary harus membuktikan jalur yang akan dipakai 704.506 pasien, termasuk jalur 715 tabrakan |
| Pilihan A | Ke-25 kandidat dipakai apa adanya sebagai manifest eksekusi, sesudah strategi QR `PAT-OQ-028` disetujui: 10 tabrakan sengaja menguji jalur P0, 15 menguji jalur bersih. Konsekuensi: kedua jalur terbukti sekaligus tanpa penggantian; menunggu `PAT-OQ-028` |
| Pilihan B | 10 tabrakan diganti 10 baris bersih dari kelompok sumber yang sama (menjaga 7/6/6/6), lalu jalur tabrakan diuji di canary terpisah. Konsekuensi: Canary-A tidak menguji P0; butuh pemilihan ulang oleh pemilik |
| Pilihan C | Dua tahap: Canary-A1 = 15 bersih; Canary-A2 = 10 tabrakan sesudah `PAT-OQ-028` disetujui. Konsekuensi: jalur bersih terbukti lebih dulu; dua otorisasi runtime |
| Pilihan D | Other |
| Rekomendasi | A — bila `PAT-OQ-028` pilihan A disetujui, path namespace membuat 10 baris itu tidak lagi bertabrakan. Agent tidak mengganti satu pun nilai |
| Diselesaikan Step 16? | Tidak. Step 16 yang mengungkap 10 tabrakan |
| Status | **`OWNER_APPROVED`** — pilihan A, bersyarat pada `PAT-DEC-052` (`PAT-DEC-056`): ke-25 nilai yang sama, dalam urutan yang sama, menjadi manifest eksekusi sesudah QR A1 dan reservation guard selesai; tanpa penggantian otomatis |

#### `PAT-OQ-034` — Inventaris filesystem folder QR

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Apakah pemilik membuat inventaris read-only folder `patient-qrcodes` di Staging sebelum gelombang besar? |
| Mengapa penting | Angka 715 pada Step 16 dihitung dari database (MRN preview dibanding MRN preview Pilot), bukan dari isi folder. Folder lain bisa saja ada: pasien terhapus, pembuatan gagal, atau seeder |
| Pilihan A | Ya — sebelum `PAT-GATE-004`, dan disarankan sebelum Canary-A: daftar folder di bawah `patient-qrcodes` (dan `patient-qrcodes/rsmmc-bulk` bila `PAT-OQ-028` A) dicocokkan dengan 704.506 MRN bulk, lalu hasilnya dicatat. Konsekuensi: konflik tak terduga terlihat sebelum eksekusi; tidak ada berkas yang berubah |
| Pilihan B | Tidak; mengandalkan pemeriksaan simulasi per pasien (`PAT-OQ-016`). Konsekuensi: hanya terlihat untuk pasien yang disimulasikan |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-062`): inventaris read-only wajib sebelum otorisasi runtime Canary-A dan sebelum `PAT-GATE-004`, mencakup `patient-qrcodes/` dan `patient-qrcodes/rsmmc-bulk/` |

#### `PAT-OQ-035` — Penempatan task reservation guard

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Di task mana reservation guard `PatientCode` dan MRN (`PAT-OQ-030`, `031`) dibangun? |
| Mengapa penting | Guard mengubah alur pendaftaran pasien normal — kiosk dan admin — sehingga dampaknya jauh lebih luas daripada worker migrasi dan butuh review terpisah |
| Pilihan A | Task prasyarat baru, usulan nama `BE-PAT-MIG-003`, yang harus ✅ sebelum runtime Canary-A. `BE-PAT-MIG-002` tetap fokus pada worker dan memuat gerbang `NORMAL_ALLOCATION_GUARD`. Konsekuensi: review terpisah sesuai dampaknya; kartu baru dibuat sesudah keputusan ini, bukan sekarang |
| Pilihan B | Di dalam `BE-PAT-MIG-002`. Konsekuensi: satu task lebih besar yang menyentuh worker dan pendaftaran normal sekaligus; review lebih berat |
| Pilihan C | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak |
| Status | **`OWNER_APPROVED`** — pilihan A (`PAT-DEC-057`): task prasyarat `BE-PAT-MIG-003` — RSMMC Normal Patient Identifier Reservation Guard; wajib disetujui source, dideploy, dan diverifikasi runtime sebelum Canary-A `dryRun=false` |

### 10.22 Pertanyaan baru dari amandemen kedua 15 Oktober 2026

Kedua pertanyaan ini **bukan** dibuat ulang dari pertanyaan lama. Keduanya muncul langsung dari
keputusan pemilik pada sesi ini, dan tidak dapat diputuskan agent karena menyangkut wewenang
eksekusi database dan perilaku Produksi.

#### `PAT-OQ-036` — Bentuk bukti persistensi provider-real `IsMember = false`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Dengan cara apa, dan kapan, bukti PostgreSQL sungguhan bahwa `IsMember = false` tersimpan fisik dihasilkan? |
| Mengapa penting | `PAT-DEC-053` mewajibkan bukti provider-real dan menyatakan SQLite tidak cukup. Setiap cara yang memakai PostgreSQL sungguhan adalah eksekusi database, yang menurut `AGENTS.md` butuh wewenang eksplisit terpisah. Pilihannya juga menentukan isi Definition of Done build |
| Pilihan A | Test integrasi terfokus di `Tests/QuilvianSystemBackend.PatientManagementTests/` terhadap basis data PostgreSQL **sekali pakai yang tidak dibagi** — lokal atau kontainer — memakai model EF dan Npgsql aplikasi; connection string dari environment variable; bila bukti diminta tetapi database tidak tersedia, test **gagal**, tidak dilewati. Konsekuensi: bukti tersedia sebelum runtime tanpa menyentuh Staging; butuh wewenang membuat dan menghapus basis data sekali pakai; bila memakai kontainer otomatis, dependency seperti Testcontainers butuh izin eksplisit (`TEST_POLICY` bagian 2) |
| Pilihan B | Pemilik menjalankan probe bertransaksi di Staging: insert pasien uji dengan kode dan MRN di luar seluruh reservasi, update `IsMember = false`, baca ulang, lalu `ROLLBACK` — pola yang sama dengan bukti kompatibilitas PostgreSQL Pilot (laporan `BE-PAT-MIG-001` bagian 9.7). Konsekuensi: provider dan skema persis Staging; mutasi sementara pada basis data bersama yang lalu dibatalkan; butuh otorisasi eksplisit dan dijalankan pemilik, bukan agent |
| Pilihan C | Bukti dari eksekusi Canary-A sendiri: verifikasi baca ulang di dalam transaksi menolak dan me-rollback bila salah, lalu audit fisik sesudah eksekusi. Konsekuensi: tanpa wewenang tambahan, tetapi bukti provider-real pertama baru muncul saat penulisan sungguhan ke Staging |
| Pilihan D | Other |
| Rekomendasi | A, bila pemilik mengizinkan basis data sekali pakai. Pilihan C tetap berlaku sebagai pengaman di runtime |
| Diselesaikan Step 16? | Tidak — muncul dari `PAT-DEC-053` |
| Status | **`OWNER_APPROVED`** — sesi ketiga 15 Oktober 2026 (`PAT-DEC-065`): pilihan A, PostgreSQL 15 sekali pakai. *Riwayat: `BLOCKING` baru pada sesi kedua* |

**Contoh.** Dengan pilihan A, test membuat basis data kosong `pat_mig_proof_<acak>`, membentuk
skema dari model EF aplikasi sehingga default `IsMember = true` ikut ada, menjalankan langkah
insert-update-baca ulang, lalu memeriksa `SELECT "IsMember" FROM "MstPatient"` bernilai `false`.
Basis data itu dihapus sesudah test. Staging tidak disentuh.

#### `PAT-OQ-037` — Cakupan environment guard reservasi `BE-PAT-MIG-003`

| Unsur | Isi |
| --- | --- |
| Pertanyaan | Di environment mana guard reservasi pendaftaran normal — lantai `PatientCode` 705.250 dan guard MRN — aktif? |
| Mengapa penting | `BE-PAT-MIG-003` mengubah alur pendaftaran pasien normal pada kode yang sama untuk semua environment. Bila aktif global, sesudah dideploy ke Produksi, pasien normal Produksi mendapat kode mulai `PAT-RSMMC-705250`; dan bila guard MRN memakai pilihan sadar reservasi (`PAT-OQ-031` A), pendaftaran Produksi bergantung pada tabel `migration.*` yang mungkin tidak ada di sana. Pemilik menetapkan Produksi di luar cakupan dan tidak boleh disentuh (`PAT-DEC-019`) |
| Pilihan A | Aktif hanya di environment bernama persis `Staging`, memakai daftar izin positif seperti `PAT-OQ-006`. Environment lain memakai perilaku pendaftaran yang ada sampai ada keputusan migrasi Produksi terpisah. Konsekuensi: Produksi tidak berubah; perilaku pendaftaran berbeda antar-environment; `NORMAL_ALLOCATION_GUARD` tetap gagal tertutup bila guard tidak aktif |
| Pilihan B | Aktif di semua environment. Konsekuensi: satu perilaku di mana pun, dan Produksi ikut terlindungi bila kelak migrasi Produksi memakai rencana yang sama; tetapi nomor pasien normal Produksi berubah sejak deploy — itu menyentuh Produksi |
| Pilihan C | Aktif bila konfigurasi menyatakan profil reservasi migrasi aktif, bukan berdasarkan nama environment. Konsekuensi: fleksibel; risiko salah konfigurasi; konfigurasi deployment di luar wewenang agent |
| Pilihan D | Other |
| Rekomendasi | A |
| Diselesaikan Step 16? | Tidak — muncul dari `PAT-DEC-054` dan `PAT-DEC-057` |
| Status | **`OWNER_APPROVED`** — sesi ketiga 15 Oktober 2026 (`PAT-DEC-066`): pilihan A — hanya Staging persis — ditambah syarat konfigurasi `RsmmcMigrationReservationGuard.Enabled == true` dan gagal tertutup bila konfigurasi hilang/tidak sah. *Riwayat: `BLOCKING` baru pada sesi kedua.* **Diamandemen 16 Oktober 2026 (`PAT-DEC-067`):** `Enabled = false` di Staging persis gagal tertutup, tidak mematikan guard |

**Contoh.** Dengan pilihan B, sesudah deploy ke Produksi, pasien normal berikutnya di Produksi
mendapat `PAT-RSMMC-705250` alih-alih nomor kosong terkecil yang biasa. Dengan pilihan A,
Produksi tetap memberi nomor seperti sekarang, sedangkan Staging memberi `PAT-RSMMC-705250`.

---

## 11. Manifest `RSMMC-CANARY-A` versi `1` — isi final, belum diotorisasi runtime

> **Bukan manifest eksekusi.** Sampai 14 Oktober 2026 bagian ini berjudul "Manifest Canary-A —
> dibekukan pemilik". Pada 15 Oktober 2026 pemilik mereklasifikasinya (`PAT-DEC-040`): daftar di
> bawah adalah **kandidat draft**, tidak diotorisasi untuk runtime. Manifest eksekusi dibekukan
> ulang secara terpisah sesudah strategi QR (`PAT-OQ-028`) dan reservation guard
> (`PAT-OQ-030`–`032`) disetujui (`PAT-OQ-033`). Daftar ini dipertahankan persis sebagai jejak
> audit; agent tidak menghitung ulang, tidak memilih, dan tidak mengganti satu pun nilai.
>
> **Amandemen kedua 15 Oktober 2026 (`PAT-DEC-056`).** Ke-25 nilai yang sama, dalam urutan tabel
> di bawah, menjadi manifest eksekusi `RSMMC-CANARY-A` versi `1` begitu strategi QR A1
> (`PAT-DEC-052`) dan reservation guard `BE-PAT-MIG-003` selesai. Sampai saat itu daftar ini tetap
> kandidat. 10 baris bertabrakan menguji informasi path bawaan historis, 15 menguji path bawaan
> bersih, dan seluruh 25 menulis ke namespace `rsmmc-bulk`.
>
> **Sesi ketiga 15 Oktober 2026.** Isi manifest `RSMMC-CANARY-A` versi `1` **final** — ke-25 nilai
> dan urutannya di bawah — dan dikunci bersama desain (`PAT-DEC-033`, `PAT-DEC-056`). Keutuhannya
> dikonfirmasi pemilik: 25 baris, 25 unik, 25 ada di preview, 25 `PENDING`, 25 tanpa crosswalk
> (`PAT-FACT-087`). Manifest ini **belum** boleh dieksekusi: simulasi dan eksekusinya menunggu
> `BE-PAT-MIG-003` dan `BE-PAT-MIG-002` ✅, `PAT-GATE-005` `PASS`, dan otorisasi `PAT-GATE-002`.
> *Judul lama — jejak: "CANARY-A CANDIDATE MANIFEST — DRAFT".*

Kolom "Tabrakan namespace QR historis" diisi dari bukti Step 16 pemilik (`PAT-FACT-071`): pasien
bulk itu punya MRN usulan yang sama dengan MRN sementara milik seorang pasien Pilot, yang
`legacy_pid`-nya tercantum.

| No | `legacy_pid` | Tabrakan namespace QR historis — pemilik Pilot |
| ---: | ---: | --- |
| 1 | -667992 | **Ya** — Pilot `797021` |
| 2 | -667991 | **Ya** — Pilot `797022` |
| 3 | -667986 | **Ya** — Pilot `797033` |
| 4 | 797423 | Tidak |
| 5 | -667995 | **Ya** — Pilot `797015` |
| 6 | -667984 | **Ya** — Pilot `797037` |
| 7 | 797554 | Tidak |
| 8 | 1 | Tidak |
| 9 | 888 | Tidak |
| 10 | 2907 | Tidak |
| 11 | 169 | **Ya** — Pilot `797266` |
| 12 | 33257 | Tidak |
| 13 | 53055 | Tidak |
| 14 | 420806 | Tidak |
| 15 | 711838 | Tidak |
| 16 | 797403 | Tidak |
| 17 | 728756 | Tidak |
| 18 | 337098 | Tidak |
| 19 | 490899 | Tidak |
| 20 | 0 | Tidak |
| 21 | 29 | **Ya** — Pilot `797017` |
| 22 | 824 | Tidak |
| 23 | 98 | **Ya** — Pilot `797145` |
| 24 | 317 | **Ya** — Pilot `817686` |
| 25 | 620 | **Ya** — Pilot `892037` |

Jumlah: 10 bertabrakan, 15 bersih.

| Sebaran kelompok sumber yang diharapkan | Jumlah |
| --- | ---: |
| `NOREG_WITHOUT_RM` | 7 |
| `NOREG_WITH_RM` | 6 |
| `REG_WITHOUT_RM` | 6 |
| `REG_WITH_RM` | 6 |
| **Total** | **25** |

**Urutan eksekusi** — persis urutan kolom No 1–25 di atas, sesuai manifest server yang disetujui
(`PAT-DEC-045`): -667992, -667991, -667986, 797423, -667995, -667984, 797554, 1, 888, 2907, 169,
33257, 53055, 420806, 711838, 797403, 728756, 337098, 490899, 0, 29, 824, 98, 317, 620. Manifest
**tidak** disort ulang berdasarkan `legacy_pid`.

*Teks lama, dicabut oleh `PAT-DEC-045`:* "Urutan pemrosesan kandidat — `legacy_pid` naik secara angka
Int64 … -667995, -667992, …, 797554." Urutan naik Int64 kini hanya aturan bagi mode pemilihan umum
di task kemudian.

**Sidik jari manifest (berurutan).** SHA-256 dari teks kanonik — `legacy_pid` dalam urutan No
1–25, dipisah koma, tanpa spasi, tanpa baris baru, UTF-8:
`c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad`. Inilah nilai yang ditampilkan
respons sebagai `manifestSha256` untuk `RSMMC-CANARY-A` versi `1`, karena urutan kini bagian dari
manifest.

*Sidik jari himpunan lama — jejak, tidak dipakai lagi:* SHA-256 dari `legacy_pid` yang diurutkan naik,
`3b5affd9558d24da2fe8164c5c9d16ea230b89b4be0404889a55ad22193434aa`. Nilai ini tidak membedakan
dua manifest yang isinya sama tetapi urutannya berbeda.

Pemeriksaan agent atas daftar: 25 nilai, 25 unik, 5 negatif, satu bernilai 0. Kelompok sumber
**per `legacy_pid`** tidak diberikan pemilik dan tidak ditebak.

---

## 12. Gerbang runtime

| ID | Gerbang | Prasyarat | Keadaan |
| --- | --- | --- | --- |
| `PAT-GATE-001` | Eksekusi rekonsiliasi Pilot 715 | `BE-PAT-MIG-001` | **`CLOSED/PASS`** — dilaporkan pemilik 14 Oktober 2026. Rincian bukti runtime belum tercatat di blueprint (`PAT-STALE-002` pada manifest) |
| `PAT-GATE-002` | Runtime Canary-A `BE-PAT-MIG-002` — dua otorisasi terpisah: (a) deployment Staging + simulasi; (b) eksekusi `dryRun=false` | `PAT-DEC-033`; `BE-PAT-MIG-002` ✅ untuk source; review source leader; **`PAT-GATE-005` `PASS`** (`BE-PAT-MIG-003` dideploy dan terverifikasi, `PAT-DEC-057`); manifest eksekusi `RSMMC-CANARY-A` v1 (`PAT-DEC-056`); bukti pembekuan pendaftaran normal (`PAT-DEC-055`); inventaris filesystem read-only (`PAT-DEC-062`); bukti persistensi provider-real PostgreSQL 15 sekali pakai (`PAT-DEC-065`, dihasilkan saat build) sebelum (b) | **Belum dapat diminta** |
| `PAT-GATE-003` | Desain dan runtime Canary-B | `PAT-GATE-002` `PASS`, termasuk uji ulang idempoten; kebijakan `PAT-DEC-058` dan `PAT-DEC-059` | **Dicadangkan** — belum dirancang |
| `PAT-GATE-004` | Gelombang lebih besar dari canary | Syarat `PAT-DEC-061`: Canary-A `PASS`, eksekusi ulang idempoten `PASS`, Canary-B `PASS`, inventaris filesystem kedua (`PAT-DEC-062`), review penyimpanan/kapasitas, verifikasi reservation guard | **Dicadangkan** — belum dirancang |
| `PAT-GATE-005` | Deployment Staging dan verifikasi runtime `BE-PAT-MIG-003` | `BE-PAT-MIG-003` ✅ untuk source — termasuk uji PostgreSQL provider-real (`PAT-DEC-065`) — dan direview leader; otorisasi deployment terpisah; konfigurasi Staging `RsmmcMigrationReservationGuard.Enabled = true` diatur pemilik (`PAT-DEC-066`) | **Belum dapat diminta.** Lulus bila, di Staging persis, pemeriksa baca-saja allocator melaporkan guard aktif, konfigurasi sah, `PatientCode` normal berikutnya ≥ `PAT-RSMMC-705250`, dan MRN normal berikutnya ≥ `00-89-01-88` (`PAT-DEC-064`). Pembekuan pendaftaran normal (`PAT-DEC-055`) baru boleh dicabut pemilik sesudah gerbang ini `PASS` |

**Urutan 12 langkah (`PAT-DEC-029`).**

| No | Langkah | Wewenang yang dibutuhkan | Bukti yang dicatat |
| ---: | --- | --- | --- |
| 1 | Implementasi source | `PAT-DEC-033` + sesi `TASK MODE: BACKEND` lewat `build-module-backend` | Laporan `task/report/backend/BE-PAT-MIG-002.md` |
| 2 | Test, build, QBE | Bagian dari langkah 1; test **wajib** (`PAT-DEC-037`) | Keluaran perintah apa adanya |
| 3 | Review source leader | Keputusan tertulis leader | Bagian review pada laporan |
| 3a | `BE-PAT-MIG-003` dideploy dan diverifikasi runtime | `PAT-GATE-005` | Hasil pemeriksa allocator di Staging |
| 4 | Deploy ke Staging | Otorisasi deployment terpisah — `PAT-GATE-002` (a) | Commit yang dideploy |
| 5 | Simulasi Canary-A | `PAT-GATE-002` (a) | Respons lengkap: gerbang, 25 baris dalam urutan manifest, identifier/versi/sidik jari manifest, informasi path bawaan historis |
| 6 | Pemilik meninjau bukti simulasi | Pemilik | Catatan keputusan |
| 7 | Otorisasi eksplisit eksekusi | Pemilik — `PAT-GATE-002` (b) | Catatan otorisasi |
| 8 | Eksekusi Canary-A `dryRun=false` | `PAT-GATE-002` (b) | Respons lengkap |
| 9 | Audit database dan QR sesudah eksekusi | Pemilik, read-only | Angka audit `PAT-MIG2-AC-36` |
| 10 | Ulang Canary-A yang sama untuk membuktikan idempotensi | `PAT-GATE-002` (b) | Seluruh baris `ALREADY_MIGRATED`, jumlah data tidak berubah |
| 11 | Sesudah `PASS`, rancang Canary-B | `PAT-GATE-003` | Revisi blueprint baru |
| 12 | Sesudah kedua jalur `PASS`, pertimbangkan gelombang lebih besar | `PAT-GATE-004` | Revisi blueprint baru |

Tidak ada langkah yang berjalan otomatis sesudah langkah sebelumnya. Persetujuan source
(langkah 3) **tidak** memberi wewenang langkah 4 dan seterusnya.

---

## 13. Kesiapan, blocker, dan langkah berikutnya

### 13.1 Keadaan — sesi ketiga 15 Oktober 2026

| Hal | Isi |
| --- | --- |
| Keputusan | `PAT-DEC-017`–`032` (14 Oktober 2026); `PAT-DEC-034`–`043` (sesi pertama); `PAT-DEC-044`–`062` (sesi kedua); `PAT-DEC-063`–`066` dan `PAT-DEC-033` (sesi ketiga) — seluruhnya `approved` |
| Isu desain P0 | Ketiganya diputuskan: `PAT-P0-001` (`PAT-DEC-052`), `PAT-P0-002` (`PAT-DEC-054`, `066`), `PAT-P0-003` (`PAT-DEC-064`, `066`) |
| Pertanyaan | 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, **0 `BLOCKING`**, 0 `PROPOSED` (bagian 10.1) |
| `requirement-completeness-gate` | **`PASS`** untuk `BE-PAT-MIG-003` lalu `BE-PAT-MIG-002` — keduanya `READY_FOR_DOMAIN_DESIGN` (`requirement-completeness-assessment.md`, `PAT-RCG-001`). Usulan standar `PAT-GAP-002`–`016` dikunci lewat `PAT-DEC-033` |
| Amandemen 16 Oktober 2026 | `PAT-DEC-067` — revision `4`, kontrak `1.2.1`: `Enabled = false` di Staging persis gagal tertutup; endpoint pemeriksa `PAT-GATE-005`. Tidak membuka pertanyaan baru; jumlah `BLOCKING` tetap 0 |
| Amandemen kedua 16 Oktober 2026 | `PAT-DEC-068` — revision `5`, kontrak `1.2.2`: kunci allocator dua bagian tetap; endpoint pemeriksa dikonfirmasi. Baseline source `BE-PAT-MIG-003` disinkronkan ke Integration `0c6e3145` (`PAT-SRC-030`). Jumlah `BLOCKING` tetap 0 |
| Kontrak | `1.2.0` `approved` |
| Task | `BE-PAT-MIG-003` dan `BE-PAT-MIG-002` — kartu dibuka (`approved`), belum ada source |
| Tindakan operasional | Pembekuan pendaftaran pasien normal di Staging tetap berlaku (`PAT-FACT-078`) sampai `PAT-GATE-005` `PASS`. Pembekuan harus menutup `POST /`, `POST /kiosk`, **dan** `POST /admin` (`PAT-SRC-027`) |

### 13.2 Bukti yang diminta pada amandemen kedua — **diterima** sesi ketiga

| Butir yang diminta | Dipenuhi oleh |
| --- | --- |
| `PAT-OQ-019` butir 1 — skema preview | `PAT-FACT-083`; tipe `full_name`/`birth_date` dijaga gerbang `PREVIEW_SCHEMA_SIGNATURE` (`PAT-GAP-009`) |
| `PAT-OQ-019` butir 2 — skema perencana | `PAT-FACT-082` |
| `PAT-OQ-019` butir 3 — skema crosswalk, panjang status, trigger | `PAT-FACT-079`–`081` |
| `PAT-OQ-019` butir 4 — tabel batch dan baris kanonik | Foreign key `PAT-FACT-079`; baris kanonik dijaga gerbang `BATCH_ROW_EXISTS` saat runtime |
| `PAT-OQ-019` butir 5 — cara penyimpanan enum | `PAT-FACT-083` — `integer` |
| `PAT-OQ-019` butir 6 — kelompok sumber | Kolom `source_group` (`PAT-FACT-083`); sebaran 7/6/6/6 dijaga `MANIFEST_INTEGRITY` saat runtime (`PAT-GAP-011`) |
| `PAT-OQ-019` butir 7 — status normalisasi | Tidak dibutuhkan (`PAT-GAP-013`) |
| `PAT-OQ-019` butir 8 — crosswalk dua pasien tertunda | `PAT-FACT-088` — tidak ada |
| `PAT-OQ-019` butir 9 — rentang `legacy_pid` | `PAT-FACT-086` |
| `PAT-OQ-031` butir 1–3 — MRN maksimum, kapasitas, format | `PAT-FACT-084` |
| `PAT-OQ-031` butir 4 — riwayat `00-00-00-27` | `PAT-FACT-089` |
| `PAT-OQ-036`, `PAT-OQ-037` | `PAT-DEC-065`, `PAT-DEC-066` |

<details>
<summary>Daftar permintaan asli amandemen kedua — jejak</summary>

**`PAT-OQ-019` — satu paket bukti skema read-only final dari pemilik:**

1. `\d+ migration.rsmmc_patient_target_preview`: tipe seluruh kolom — termasuk `batch_id`, `legacy_pid`, `proposed_patient_code`, `proposed_medical_record_number`, tujuh kolom enum, empat kolom boolean, `validation_status`, serta `legacy_old_rm`, `legacy_normalized_mrn`, `legacy_normalized_nik` (`PAT-FACT-076`) dan kolom klasifikasi telepon `SAFE_TO_STORE` bila ada (`PAT-FACT-077`); primary key dan indeks unik, terutama `(batch_id, legacy_pid)`.
2. `\d+ migration.rsmmc_patient_mrn_plan`: tipe `batch_id`, `legacy_pid`, `plan_status`, `final_medical_record_number`; indeks unik `(batch_id, legacy_pid)`.
3. `\d+ migration.rsmmc_patient_crosswalk`: panjang `varchar` `match_status` dan `migration_status` — harus muat `MIGRATED_BULK_WORKER` (20 karakter); CHECK constraint yang menerima `TARGET_CREATED` dan `MIGRATED_BULK_WORKER`; panjang `legacy_old_rm`, `normalized_mrn`, `normalized_nik`; default `created_at`; trigger bila ada.
4. `\d+ migration.rsmmc_patient_batch` dan bukti baris batch kanonik ada.
5. Cara preview menyimpan tujuh kolom enum — angka atau nama — beserta daftar nilai berbeda per kolom.
6. Letak kelompok sumber (`NOREG_WITHOUT_RM` dan seterusnya) dan sebaran 7/6/6/6 untuk 25 kandidat.
7. Letak status normalisasi (`READY_FOR_TARGET_PREVIEW` / `REVIEW_SCOPE`).
8. Apakah `280143` dan `805596` punya baris crosswalk.
9. Nilai minimum dan maksimum `legacy_pid` pada preview kanonik dan perencana, untuk memastikan konversi Int32 kunci `PAT-DEC-048`.

**`PAT-OQ-031` — bukti MRN read-only dari pemilik:**

1. Nilai numerik maksimum `final_medical_record_number` `FINALIZED` pada batch kanonik.
2. Sisa kapasitas 8 digit, yaitu 99.999.999 dikurangi nilai maksimum itu.
3. Validasi bahwa seluruh nilai `FINALIZED` berformat numerik `00-00-00-00` — jumlah yang tidak sesuai harus 0.
4. Bukti langsung kepemilikan dan riwayat MRN `00-00-00-27`: apakah ia MRN sementara Pilot (tabel cadangan), siapa pemilik rencana kanoniknya, dan apakah folder QR historisnya ada.

**`PAT-OQ-036` — keputusan pemilik:** cara menghasilkan bukti persistensi provider-real beserta
otorisasinya (bagian 10.22).

**`PAT-OQ-037` — keputusan pemilik:** cakupan environment guard reservasi `BE-PAT-MIG-003`
(bagian 10.22).

</details>

### 13.3 Urutan implementasi dan runtime yang wajib — instruksi pemilik sesi ketiga

Tidak ada langkah yang berjalan otomatis sesudah langkah sebelumnya. Setiap langkah butuh wewenangnya
sendiri.

| No | Langkah | Wewenang yang dibutuhkan |
| ---: | --- | --- |
| 1 | Implementasi source `BE-PAT-MIG-003` | Sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend`, dengan branch yang ditetapkan pemilik |
| 2 | Uji PostgreSQL provider-real untuk allocator — PostgreSQL 15 sekali pakai (`PAT-DEC-065`, kontrak B15) | Bagian dari sesi build; server sekali pakai disediakan pemilik atau developer |
| 3 | Build, QBE, dan review source `BE-PAT-MIG-003` | Review leader |
| 4 | Deploy dan verifikasi guard di Staging — `PAT-GATE-005` | Otorisasi deployment terpisah; konfigurasi `Enabled = true` oleh pemilik |
| 5 | Implementasi source `BE-PAT-MIG-002` | Sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend` |
| 6 | Test — termasuk bukti provider-real `IsMember` — build, QBE, dan review source `BE-PAT-MIG-002` | Bagian dari sesi build; review leader |
| 7 | Deploy `BE-PAT-MIG-002` ke Staging — `PAT-GATE-002` (a) | Otorisasi deployment terpisah |
| 8 | Inventaris filesystem read-only `patient-qrcodes/` dan `patient-qrcodes/rsmmc-bulk/` (`PAT-DEC-062`) | Pemilik, read-only |
| 9 | Simulasi Canary-A `dryRun=true` — `PAT-GATE-002` (a) | Otorisasi pemanggilan endpoint |
| 10 | Otorisasi runtime pemilik — `PAT-GATE-002` (b) | Pemilik, sesudah meninjau bukti simulasi |
| 11 | Eksekusi Canary-A `dryRun=false`, lalu audit dan eksekusi ulang idempoten | `PAT-GATE-002` (b) |

Tabel 12 langkah `PAT-DEC-029` pada bagian 12 tetap berlaku untuk rincian bukti tiap langkah
runtime; tabel ini menetapkan urutan lintas kedua task.

*Langkah berikutnya versi amandemen kedua — selesai:* (1) pemilik menyerahkan bukti `PAT-OQ-019` dan
`031` serta memutuskan `036` dan `037`; (2) agent memasukkan bukti dan menyiapkan `PAT-DEC-033`;
(3) `requirement-completeness-gate`, `design-business-module`, dan `plan-module-delivery`.

---

## 14. Log koreksi revision `3`

Sampai sesi kedua 15 Oktober 2026 revision `3` belum disetujui, sehingga koreksi dilakukan langsung pada teks draft. Koreksi sesi ketiga dilakukan bersamaan dengan finalisasi. Setiap koreksi
dicatat di sini supaya jejaknya tetap terbaca.

| Tanggal | Letak | Teks lama | Koreksi | Dasar |
| --- | --- | --- | --- | --- |
| 15 Oktober 2026 | Bagian 6.1, `PAT-SRC-008`, `PAT-OQ-017`, kontrak B4.3/B10, AC-33, `PAT-RSK-019` | Menyiratkan pasien preview dengan `is_active = false` mungkin ada dan harus ditangani | Seluruh preview `IsActive = true`; `IsActive = false` **bukan** kebutuhan bulk. Masalah nyatanya hanya `IsMember = false` | `PAT-FACT-074`, `PAT-DEC-041` |
| 15 Oktober 2026 | Bagian 11 | Judul "Manifest Canary-A — dibekukan pemilik" | "CANARY-A CANDIDATE MANIFEST — DRAFT"; daftar tetap persis | `PAT-DEC-040` |
| 15 Oktober 2026 | Bagian 10 | 19 pertanyaan dalam bentuk pilihan bebas | Bentuk seragam per pertanyaan; `PAT-OQ-015`, `017`, `020` menjadi `SUPERSEDED`; `PAT-OQ-008`, `009`, `011`, `022` menjadi `OWNER_APPROVED`; `PAT-OQ-027`–`035` ditambahkan | Keputusan pemilik dan bukti Step 16 |
| 15 Oktober 2026 | Bagian 6.1 | Pembangkit nomor normal "hanya dijaga lewat gerbang deteksi" | Perlindungan tahan lama wajib (`PAT-DEC-042`); gerbang deteksi menjadi lapis tambahan | `PAT-FACT-072`, `073` |
| 15 Oktober 2026, sesi kedua | Bagian 11; kontrak B3.1; `PAT-OQ-012` contoh | Urutan eksekusi kandidat "naik secara angka Int64" | Urutan eksekusi `CANARY_A` = urutan manifest persis, tanpa sort ulang | `PAT-DEC-045` |
| 15 Oktober 2026, sesi kedua | Bagian 11; AC-09 | Sidik jari himpunan `3b5affd9…434aa` sebagai sidik jari manifest | Sidik jari berurutan `c43207eb…79ad`; yang lama disimpan sebagai jejak | `PAT-DEC-044`, `PAT-DEC-045` |
| 15 Oktober 2026, sesi kedua | `PAT-OQ-021` rekomendasi; kontrak B4.3/B5 | `pg_try_advisory_xact_lock(hashtext(…))` satu kunci 32-bit | Dua kunci: namespace + `legacy_pid` Int32 tervalidasi | `PAT-DEC-048` |
| 15 Oktober 2026, sesi kedua | Gerbang `NORMAL_ALLOCATION_GUARD` | Lulus bila guard reservasi aktif **atau** pembekuan aktif | Lulus hanya bila `BE-PAT-MIG-003` aktif dan terverifikasi; pembekuan saja tidak cukup | `PAT-DEC-055` |
| 15 Oktober 2026, sesi kedua | `PAT-OQ-018`; kontrak B9 | Rekomendasi A: `normalized_mrn`/`normalized_nik` NULL | Diisi dari kolom preview, sebagai bukti saja | `PAT-DEC-047` |
| 15 Oktober 2026, sesi kedua | AC-44 dan AC-45 `BE-PAT-MIG-002` | Guard reservasi sebagai AC worker | Dipindah ke kartu `BE-PAT-MIG-003` (`PAT-MIG3-AC-xx`); ID lama dipertahankan sebagai jejak | `PAT-DEC-057` |
| 15 Oktober 2026, sesi kedua | `PAT-OQ-032` pilihan A | Contoh mekanisme pembekuan: "mencabut sementara izin `Patient : Create` dari seluruh peran lewat layar Akses Role" | Contoh itu hanya menutup `POST /admin`; `POST /` dan `POST /kiosk` dijaga policy `KioskRead`, bukan izin itu. Pembekuan harus menutup ketiga jalur | `PAT-SRC-027` |
| 15 Oktober 2026, sesi ketiga | Kontrak B2.2, field `items[]` | "urut `legacy_pid` naik secara angka" | Dalam urutan manifest — sisa teks lama yang bertentangan dengan `PAT-DEC-045` | `PAT-DEC-045` |
| 15 Oktober 2026, sesi ketiga | Kontrak B4.2 `NORMAL_ALLOCATION_GUARD`, B14, B14.1, B14.2; kartu `BE-PAT-MIG-003` | "MRN berikutnya bukan MRN akhir `FINALIZED`", mekanisme MRN dan cakupan environment "menunggu" | MRN berikutnya ≥ `00-89-01-88`; guard aktif hanya di `Staging` persis dengan `Enabled == true`; konfigurasi hilang/tidak sah di Staging persis → gagal tertutup | `PAT-DEC-064`, `066` |
| 15 Oktober 2026, sesi ketiga | Kontrak B9 dan B10 | "Bentuk SQL final menunggu `PAT-OQ-019`"; "cara penyimpanan di preview: `PAT-OQ-019`" | SQL bertipe tanpa `::text` (B9.1); enum `integer`, NULL atau nilai tidak dikenal → `PREVIEW_VALUE_INVALID` | `PAT-DEC-063`; `PAT-GAP-010` |
| 15 Oktober 2026, sesi ketiga | Kontrak B14 contoh; register P0 | "Bila `PAT-INF-001` benar" | Terkonfirmasi (`PAT-FACT-089`) | `PAT-FACT-089` |
| 15 Oktober 2026, sesi ketiga | Bagian 11 | Judul "CANARY-A CANDIDATE MANIFEST — DRAFT" | Manifest `RSMMC-CANARY-A` versi `1` — isi final, belum diotorisasi runtime; daftar tetap persis | `PAT-DEC-033`, `056`; `PAT-FACT-087` |
| 16 Oktober 2026 | `PAT-DEC-066`; kontrak B14.2, B14.3; `PAT-GAP-002`; `PAT-MIG3-AC-07`, `08`, `12`, `14`; `PAT-RSK-036` | "`Enabled = false` yang ditulis eksplisit mematikan guard"; pemeriksa hanya service internal | `Enabled = false` di Staging persis gagal tertutup; pemeriksa juga endpoint admin baca-saja khusus Staging dengan nama field instruksi pemilik | `PAT-DEC-067` |
| 16 Oktober 2026, kedua | Kontrak B14.1 dan B14.3; `PAT-GAP-004`; kartu `BE-PAT-MIG-003` | `pg_advisory_xact_lock(hashtext('quilvian.patient.identifier-allocation'))` | `pg_advisory_xact_lock(1346458697, 1313821261)` — dua kunci Int32 tetap: namespace `0x50415449` (`"PATI"`, PATIENT_IDENTIFIER_ALLOCATION) dan kunci `0x4E4F524D` (`"NORM"`, NORMAL_PATIENT_CREATE) | `PAT-DEC-068` |
| 19 Oktober 2026 | `PAT-SRC-023`; kontrak B14.1 | "Jalur yang meng-insert `MstPatient`: hanya `CreatePatient` …"; "Ketiganya bermuara pada satu method `CreatePatient`" | Ditambah jalur bayi `NewbornPatientRegistrationService` (`PAT-SRC-031`) | `PAT-DEC-072` |
| 19 Oktober 2026 | `PAT-DEC-053` bagian (1); kontrak B4.3 langkah 8 `PERSIST_IS_MEMBER`; `PAT-SRC-008` | UPDATE eksplisit `IsMember = false` sesudah insert | Dihapus; INSERT + baca ulang + verifikasi + rollback | `PAT-DEC-075`, `PAT-SRC-032` |
| 19 Oktober 2026 | Kontrak B4.2 `PATIENT_BASELINE` | "Jumlah `MstPatient` = 741 + jumlah crosswalk `MIGRATED_BULK_WORKER`" | Baseline tetap yang diukur ulang read-only dan disetujui sebelum Canary-A | `PAT-DEC-077` |

**DESIGN COMPLETE — SOURCE IMPLEMENTATION MAY BEGIN WITH BE-PAT-MIG-003**

*Penutup lama — jejak: "WAITING FOR FINAL READ-ONLY EVIDENCE — PAT-OQ-019 / PAT-OQ-031".*

---

## 15. Amandemen revision `6` — `BE-PAT-MIG-003A` dan hasil impact scan `BE-PAT-MIG-002`

**Latar belakangnya, dalam bahasa sederhana.** `BE-PAT-MIG-003` melindungi nomor pasien yang sudah
dipesan migrasi RSMMC dari pendaftaran pasien biasa. Sesudah task itu selesai, tim lain menambah
fitur baru: pendaftaran **bayi baru lahir** dari kasus persalinan (`POST /newborns`). Fitur ini
membagikan `PatientCode` dan MRN dengan caranya sendiri — melalui deret nomor platform — dan sama
sekali tidak melewati perlindungan `BE-PAT-MIG-003`.

*Contoh.* Seorang bidan mendaftarkan bayi "By. Ny. Sari" di Staging. Karena deret nomor bayi
dimulai dari nomor tertinggi yang sudah ada (741), bayi itu mendapat `PAT-RSMMC-00742` — padahal
kode itu sudah dipesan untuk pasien lama RSMMC `legacy_pid` -667995, pasien ke-5 Canary-A. Saat
worker kelak membuat pasien -667995, kodenya sudah terpakai dan Canary-A berhenti.

Karena itu pemilik membuka task prasyarat `BE-PAT-MIG-003A` (`PAT-DEC-069`) dan mengunci keputusan
`PAT-DEC-069`–`077` lewat instruksi tertulis 19 Oktober 2026 (`TASK: BE-PAT-MIG-003A`, bagian 1 dan 10).

### 15.1 Fakta runtime yang dilaporkan pemilik

| ID | Fakta | Nilai |
| --- | --- | --- |
| `PAT-FACT-090` | Runtime `BE-PAT-MIG-003` — dilaporkan pemilik 19 Oktober 2026 | Dideploy ke DEV dan STAGING; Staging `RsmmcMigrationReservationGuard__Enabled = true`; health `PASS`. `PAT-GATE-005` generasi 1 `PASS`: `guardState = ACTIVE`, `nextPatientCode` 705250 / `PAT-RSMMC-705250`, `nextMrn` 890188 / `00-89-01-88`, `overallGuardReady = true`. Generasi 1 hanya membuktikan jalur `CreatePatient` |
| `PAT-FACT-091` | Impact scan `BE-PAT-MIG-002` (read-only, 19 Oktober 2026) | `BLOCKED` — jalur bayi melewati lantai reservasi (`PAT-SRC-031`); langkah `PERSIST_IS_MEMBER` usang (`PAT-SRC-032`); lima kolom `MstPatient` baru belum tercatat pada pemetaan (`PAT-SRC-033`); `PATIENT_BASELINE` 741 bertabrakan dengan dibukanya kembali pendaftaran |

### 15.2 Yang berubah dan yang tidak

| Hal | Sesudah revision `6` |
| --- | --- |
| Jalur pembuatan `MstPatient` | Tiga rute `CreatePatient`, pendaftaran bayi, dan seeder demo Operating Room (`PAT-SRC-031`) |
| Perlindungan reservasi di Staging persis | Berlaku untuk pendaftaran normal **dan** bayi, dengan lantai, konfigurasi, dan kunci yang sama (`PAT-DEC-070`, `071`) |
| Di luar Staging persis | Tidak berubah: pendaftaran normal memakai algoritma lama, pendaftaran bayi memakai `NumberSeries` seperti sebelumnya (`PAT-DEC-070` b) |
| Pemeriksa `PAT-GATE-005` | Generasi 2 melaporkan perlindungan kedua jalur; `overallGuardReady` mensyaratkan keduanya (`PAT-DEC-072`) |
| Pembekuan operasional | Seluruh pembuatan pasien Staging, termasuk bayi, sampai jendela Canary-A selesai (`PAT-DEC-073`) |
| Skema | Tidak ada EF migration baru (`PAT-DEC-074`) |
| Desain worker `BE-PAT-MIG-002` | `PERSIST_IS_MEMBER` dihapus (`PAT-DEC-075`); lima kolom baru `NULL` (`PAT-DEC-076`); `PATIENT_BASELINE` memakai baseline tetap yang diukur ulang (`PAT-DEC-077`). Tetap ⛔ (`PAT-DEC-069`) |

**BE-PAT-MIG-003A APPROVED FOR SOURCE IMPLEMENTATION — BE-PAT-MIG-002 REMAINS BLOCKED**
