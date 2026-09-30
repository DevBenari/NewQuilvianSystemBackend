# Laporan Perubahan Backend — `BE-LAB-67`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-67` |
| Judul | Izin hasil tersendiri dan route netral disiplin |
| Slice | Gelombang `MVP-8a` — `EPIC-LAB-14`, perbaikan `S4b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6aj.1** |
| Trace | `FR-14.1`, `FR-14.2`, `FR-14.5`, `FR-14.6`; `LAB-DEC-146`, `LAB-DEC-135`, `LAB-DEC-141`, `LAB-DEC-097`, `LAB-DEC-085`; `VAL-107`, `VAL-122`; `AC-221`, `AC-222`, `AC-224` |
| Contract version | `LAB-API-v1` **`r33`** bagian 28.2 dan 28.4; `LAB-PERM-v1` **rev 10** bagian 12.2-12.6; `LAB-VAL-v1` **`r11`** `VAL-122`; `LAB-STATE-v1` **`r4`** bagian 6.3 — keempatnya **`approved` 2026-09-24** oleh pemilik modul |
| Dependency | Nol |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±15), berkas diubah 0 (2), logika bisnis 1, kontrak API **2** (tiga route dicabut), database 1 (dua `Include` pada query yang sudah ada), keamanan/auth **2**, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/` dan dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a95f07a6` (branch `yoga`) |
| Tanggal | 2026-09-29 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — **batasnya menyempit 2026-09-29 sore (bagian 8)**. Kini terbukti terhadap aplikasi sungguhan: validator lolos **lewat startup** (1572 kunci); Swagger nol memuat route lama, dan route lama menjawab `404`; **`VAL-122` `422`** pada ketiga tindakan atas pemeriksaan Patologi Anatomi. **Yang tersisa hanya uji dua akun `AC-221`/`AC-222`** — nol jabatan di dev memegang `LabExaminationResult : Update`, dan pemberiannya (langkah rilis 6aj.5) belum diinstruksikan. *Semula: seluruh kode status HTTP tertahan seeder Hemodialisa* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` sejak 2026-09-02 (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27 dan 114) |
| Keberlakuan | `TOUCHED LEGACY` — `LabExaminationController` dan `LabExaminationService` sudah ada; nol entity, nol berkas baru |
| QBE yang berlaku | `QBE-PERM-001` (metadata Access yang berlaku), `QBE-API-001` (respons dan status yang mapan), `QBE-SVC-001` (logika tetap di service), `QBE-VAL-001` (penjaga `VAL-122` melempar validasi yang sudah dipetakan ke `422`), `QBE-AUD-001` (riwayat transisi terpisah dari logger), `QBE-LOG-001` |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill. Folder `.codex/` **memang tidak ada lagi** — `AGENTS.md` baris 45 menyatakannya dicabut, jadi ketiadaannya bukan penahan |

---

## 1. Masalah yang diperbaiki

**Dokter pemesan dapat mengubah hasil laboratorium, padahal ia hanya diberi izin untuk menandai
cito.**

Hak akses Quilvian dicocokkan **per pasangan resource dan aksi**, bukan per endpoint
(`Services/Security/AccessPermissionService.cs:117-132`). Sebelum task ini, delapan tindakan pada
`LabExaminationController` memakai pasangan yang sama, `LabExamination : Update`:

| Tindakan | Siapa yang seharusnya |
| --- | --- |
| Menandai cito, membatalkan, menandai duplo | Dokter pemesan dan petugas Lab |
| Mengisi hasil Patologi Klinik, mengisi hasil Mikrobiologi, Final, Reopen, mencatat konsultasi | **Analis saja** |

Dokter pemesan **wajib** memegang `LabExamination : Update` supaya dapat menandai cito (`VAL-03`).
Satu baris kebijakan itu otomatis membuka kelima tindakan hasil. Capability map rev 5 membuktikannya
(`CAP-P14-02`), dan `LAB-DEC-146` menutupnya.

**Contoh.** dr. Andi memesan pemeriksaan Kalium untuk pasien IGD dan menandainya cito. Dengan izin
yang sama, ia dapat membuka `PUT /lab-examinations/{id}/result` dan mengganti nilai `6,4` yang diisi
analis menjadi `4,8`. Sistem menerimanya, sebab dari sisi hak akses keduanya tindakan yang sama.

**Masalah kedua lebih kecil tetapi menahan Patologi Klinik.** Final, Reopen, dan konsultasi hanya
tersedia di route `/result/microbiology/*`, padahal logikanya tidak pernah bergantung pada disiplin.
Patologi Klinik tidak punya jalur untuk menyatakan penulisan hasil selesai.

---

## 2. Proses bisnis

**Tujuan.** Hanya analis yang boleh menulis dan menyelesaikan hasil. Dokter pemesan tetap dapat
menandai cito.

**Pelaku.**

| Jabatan | Yang dapat dilakukan sesudah task ini |
| --- | --- |
| Analis Patologi Klinik dan Mikrobiologi | Mengisi hasil, Final, Reopen, konsultasi — **bila admin memberinya `LabExaminationResult : Update`** |
| Dokter pemesan | Menandai cito. Menulis hasil **ditolak `403`** |
| Petugas Lab administrasi | Batal dan duplo sesuai izinnya. Menulis hasil **ditolak `403`** |
| Dokter Lab / patolog | Laporan Patologi Anatomi seperti sebelumnya — jalurnya tidak disentuh |

**Alur normal, berurutan.**

1. Analis mengisi hasil lewat `PUT /{id}/result` (Patologi Klinik) atau `PUT /{id}/result/microbiology`.
2. Sesudah yakin, analis menekan **Final** lewat `POST /{id}/result/finalize`. Final berarti
   *"saya selesai menulis"*. **Final bukan rilis** (`LAB-DEC-097`): hasil belum terlihat oleh dokter
   pemesan, dan respons selalu membawa `isReleased` dan `deliveryBlockedReason`.
3. Bila ada yang perlu dikoreksi sebelum validasi, analis menekan **Reopen** lewat
   `POST /{id}/result/reopen` dengan alasan wajib. Satu baris `LabTransitionHistory` tercatat
   dengan aksi `LabExamination.ReopenResult`.
4. Bila hasil dikonsultasikan ke dokter lain, analis mencatat kepada siapa dan kapan lewat
   `PUT /{id}/result/consultation`.

**Jalur tidak normal.**

| Keadaan | Jawaban sistem |
| --- | --- |
| Pengguna tanpa `LabExaminationResult : Update` — misalnya dokter pemesan — memanggil salah satu dari kelima tindakan | `403` |
| Final, Reopen, atau konsultasi pada pemeriksaan **Patologi Anatomi** | `422` — *"Hasil Patologi Anatomi diselesaikan lewat laporan Patologi Anatomi."* (`VAL-122`) |
| Reopen atas hasil yang belum pernah Final | `422` (`VAL-107`, tidak berubah) |
| Frontend lama memanggil `/result/microbiology/finalize` | `404` — route itu **tidak ada lagi** |

**Kenapa `VAL-122` perlu.** Order Patologi Anatomi punya baris `LabExamination`, tetapi hasilnya
disimpan di laporan per pesanan (`LAB-DEC-085`). Tanpa penjaga ini, Final atas baris PA menjawab
*"hasil belum diisi"*. Pesan itu benar secara teknis tetapi menyesatkan, sebab patolognya **sudah**
mengisi laporan di tempat lain.

**Disiplin dibaca dari order.** Order lama yang disiplinnya kosong memakai disiplin katalog
pemeriksaannya, sumber yang juga dipakai untuk menurunkan disiplin order (`LAB-DEC-048`).

**Keadaan sesudah task ini dirilis.** **Tidak ada satu pun analis** yang dapat menulis hasil sampai
admin memberi `LabExaminationResult : Update` lewat layar Akses Role. Pemberian itu **tidak boleh
disalin otomatis** dari pemegang `LabExamination : Update`, sebab penyalinan juga memberikannya
kepada dokter pemesan dan membuka lagi masalah di bagian 1 (`LAB-PERM-v1` rev 10 bagian 12.5).
Urutannya adalah langkah rilis `backend-roadmap.md` 6aj.5, bukan pekerjaan kode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Wewenang runtime QBE dan status registry `Lab` |
| `roadmap/backend-roadmap.md` 6aj.1 | Cakupan, DoD, dan verifikasi yang diminta |
| `contracts/api-contract.md` 28.2-28.4 | Route pengganti, hak akses, dan kode status |
| `contracts/permission-audit-matrix.md` 12.2-12.6 | Resource baru, pemetaan endpoint, nama aksi riwayat Reopen |
| `contracts/validation-matrix.md` baris 536 | Teks pesan `VAL-122` persis |
| `testing/acceptance-test-matrix.md` baris 588-591 | Isi `AC-221`..`AC-224` |
| `Services/Security/PermissionRegistryDescriptor.cs` | Cara registri membaca `[AccessAction]` untuk resource turunan |
| `Services/Security/PermissionRegistryValidator.cs` | Syarat yang harus lolos saat startup |
| `Services/Security/AccessPermissionService.cs` | Pencocokan izin per nama aksi, dan perlakuan superadmin |
| `Models/LabOrder.cs`, `MasterData/Models/MstProcedure.cs`, `Enums/LaboratoryEnums.cs` | Sumber disiplin untuk penjaga `VAL-122` |
| `Program.cs` baris 1432-1490 | Urutan seeder startup dan posisi validator registri |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/LabExaminationController.cs` | Kelima tindakan hasil beralih ke `[AccessPermission("LabExaminationResult", "Update")]`, masing-masing berpasangan `[AccessAction]` dengan teks yang **identik**. Tiga route `/result/microbiology/finalize\|reopen\|consultation` diganti `/result/finalize\|reopen\|consultation` tanpa alias. `FinalizeMicrobiologyResult` → `FinalizeResult`, `ReopenMicrobiologyResult` → `ReopenResult` |
| `Services/LabExaminationService.cs` | `FinalizeMicrobiologyResultAsync` → `FinalizeResultAsync`, `ReopenMicrobiologyResultAsync` → `ReopenResultAsync`. Logikanya dipakai apa adanya (capability map `CAP-P14-03`, `CAP-P14-08`). Ditambah penjaga `EnsureNotAnatomicalPathology` (`VAL-122`) pada ketiga method; Final dan konsultasi kini ikut memuat `LabOrder`. Aksi riwayat Reopen **untuk baris baru** menjadi `LabExamination.ReopenResult`, dan nama aksi logger menjadi netral |

**Nol berkas di luar dua berkas itu disentuh.** Nol entity, nol migration, **nol seeder kebijakan**.

**Kenapa kelima `[AccessAction]` memakai teks yang sama.** Registri menampilkan satu pasangan
resource-aksi sebagai **satu baris** di layar Akses Role, memakai teks `[AccessAction]` endpoint
**pertama** yang terbaca (`PermissionRegistryDescriptor.cs:517`). Bila teksnya berbeda-beda, yang
tampil bergantung pada urutan pembacaan. Admin bisa membaca *"Mengisi hasil"* padahal yang ia
berikan juga mencakup Final dan Reopen. Teks tunggalnya menyebut keempat kemampuan:
*"Mengisi hasil, menyatakan penulisan selesai, membuka kembali, dan mencatat konsultasi hasil
Patologi Klinik dan Mikrobiologi"*.

**Baris riwayat lama tidak diubah.** Baris `LabTransitionHistory` yang sudah bernama
`LabExamination.ReopenMicrobiologyResult` tetap seperti adanya (`LAB-PERM-v1` rev 10 bagian 12.6).
Pembaca riwayat perlu mengenali kedua nama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Memecah kompatibilitas, sesuai `r33` 28.4.** Tiga route dicabut dan diganti; bentuk request dan response tidak berubah. Konsumen satu-satunya adalah frontend `lab-microbiology-result-constants.jsx`, yang diperbarui `FE-LAB-35`. **Keduanya wajib dirilis bersama** |
| Database | **Nol migration, nol perubahan schema.** Dua query yang sudah ada ditambah `Include(x => x.LabOrder)`. Satu efek pada database dev bersama: startup yang dicoba 2026-09-29 menjalankan `AccessMenuSeeder` sampai selesai sebelum berhenti di seeder Hemodialisa. Registri Akses Role di sana karenanya kemungkinan besar sudah memuat `LabExaminationResult : Update`. Ini sinkronisasi registri yang biasa terjadi setiap startup, **bukan pemberian izin**. Barisnya tidak dihitung langsung |
| Keamanan/Auth | **Inti task ini.** Resource baru `LabExaminationResult` (resource turunan di bawah modul `HEALTH_SERVICE_LABORATORY_MANAGEMENT`). Kelima tindakan hasil kini **menolak** pemegang `LabExamination : Update` saja. Cito, batal, dan duplo tidak berubah. Jalur laporan Patologi Anatomi tidak disentuh (`LAB-DEC-090` utuh). Payload log tetap tidak memuat nilai hasil, organisme, maupun nama pihak yang dikonsultasikan |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/{id}/result` | Mengisi hasil Patologi Klinik | **`LabExaminationResult : Update`** — sebelumnya `LabExamination : Update` |
| `PUT` | `/{id}/result/microbiology` | Mengisi hasil Mikrobiologi beserta isolat dan antibiogram | **`LabExaminationResult : Update`** — sebelumnya `LabExamination : Update` |
| `POST` | `/{id}/result/finalize` | Menyatakan penulisan hasil selesai — **bukan rilis** — untuk Patologi Klinik dan Mikrobiologi. Menggantikan `/result/microbiology/finalize` | **`LabExaminationResult : Update`** |
| `POST` | `/{id}/result/reopen` | Membuka kembali penulisan sebelum validasi; alasan wajib. Menggantikan `/result/microbiology/reopen` | **`LabExaminationResult : Update`** |
| `PUT` | `/{id}/result/consultation` | Mencatat kepada siapa hasil dikonsultasikan dan kapan. Menggantikan `/result/microbiology/consultation` | **`LabExaminationResult : Update`** |

**Tidak berubah:** `POST /{id}/cancel`, `PUT /{id}/urgency`, dan `PUT /{id}/duplo` tetap
`LabExamination : Update`. `GET /{id}/result`, `GET /{id}/result/microbiology`, dan
`GET /{id}/confirming-doctor-options` tetap `LabExamination : Read`.

**Dicabut, tanpa alias:** `POST /{id}/result/microbiology/finalize`,
`POST /{id}/result/microbiology/reopen`, `PUT /{id}/result/microbiology/consultation`.
Alasannya: dua jalur untuk satu tindakan harus diuji dan dijaga selamanya
(`02-backend-architecture.md` 19.9).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error**, 230 warning, 97 detik. **Nol warning menyebut `LabExaminationController` atau `LabExaminationService`** | `PASS` | Keluaran build disaring pada kedua nama berkas |
| Startup `ASPNETCORE_ENVIRONMENT=Development` | **Berhenti** di `HemodialysisMasterDataSeeder.EnsureChecklistItemsAsync` (`Program.cs:1467`): `42P01 relation "public.HmdChecklistItem" does not exist`. Migration `20260922044002_AddHemodialysisManagement` belum diterapkan di database dev bersama. Sepuluh seeder sebelumnya selesai, termasuk `AccessMenuSeeder` dan kelima seeder Laboratorium | `EXISTING / ENVIRONMENT ISSUE` | Log startup. **Milik modul Hemodialisa, tidak disentuh.** Menerapkan migration modul lain ke database bersama adalah wewenang terpisah |
| `PermissionRegistryValidator.ValidateAndReport(throwOnFailure: true)` | **Lolos** — *"Permission registry valid. 1541 identitas kanonik dari 1541 kemampuan terdaftar."* | `PASS` | Karena startup berhenti **sebelum** validator (`Program.cs:1482`), kelas yang sama dijalankan lewat harness sekali pakai di luar repository. Harness itu memuat seluruh controller dari DLL hasil build ini. Validatornya hanya membaca descriptor controller, tanpa database, jadi hasilnya sama dengan yang akan dilihat startup |
| Inventaris route `LabExaminationController` | 14 route. **Kelima tindakan hasil `LabExaminationResult,Update`**; `cancel`, `urgency`, `duplo` **`LabExamination,Update`**; kelima pembacaan `LabExamination,Read`; tambah pemeriksaan `LabExamination,Create` | `PASS` | Descriptor controller dari DLL hasil build |
| Route `result/microbiology/(finalize\|reopen\|consultation)` di **seluruh** aplikasi | **Nol** | `PASS` | Pencarian pola atas semua descriptor aplikasi. Halaman Swagger sendiri belum dibuka karena aplikasi tidak berjalan |
| Registri Akses Role untuk resource baru | Resource `LabExaminationResult` terdaftar di modul `HEALTH_SERVICE_LABORATORY_MANAGEMENT`, terlihat di layar Akses Role. Aksinya `Update` — *"Update Lab Examination Result"* dengan deskripsi gabungan di bagian 3.2 | `PASS` | `PermissionRegistryDescriptor.Build` |
| Sisa pemakaian nama lama di source | **Nol.** Kemunculan tersisa hanya di dua komentar yang sengaja menyebut nama lama, dan di berkas log lama yang di-gitignore | `PASS` | Pencarian `FinalizeMicrobiologyResult`, `ReopenMicrobiologyResult`, dan route lama pada `*.cs`/`*.json`/`*.http` |
| Cakupan berkas | `git status --short`: 2 berkas source `M`, ditambah dokumen blueprint task ini. Nol seeder, nol migration | `PASS` | Keluaran perintah |
| `AC-221` / `AC-222` — dua akun lewat HTTP | Belum dijalankan | `NOT RUN` | Dua penahan: aplikasi tidak dapat start (baris kedua), dan **belum ada satu pun jabatan yang memegang `LabExaminationResult : Update`**. Memberikannya adalah langkah rilis 6aj.5 dan penulisan ke database bersama, jadi perlu instruksi eksplisit |
| `VAL-122` — Final atas baris Patologi Anatomi menjawab `422` | Belum dijalankan lewat HTTP | `NOT RUN` | Aplikasi tidak berjalan. Superadmin melewati pemeriksaan izin (`EnforceClinicalPolicyForSuperAdmin: false`), jadi uji ini bisa langsung dilakukan dengan superadmin begitu aplikasi hidup. Penolakan terjadi sebelum ada penulisan |
| `AC-224` — regresi laporan Patologi Anatomi | Belum dijalankan lewat HTTP | `NOT RUN` | Secara statis tidak ada berkas laporan PA dalam diff |

Uji manual: **`NOT FEASIBLE`** saat ini — aplikasi tidak dapat start pada database dev bersama
karena penahan milik modul Hemodialisa.

**Tidak dijalankan:**

- Analyzer — build memakai `-p:RunAnalyzers=False`, sesuai praktik tetap repository ini. Warning
  compiler tetap dibaca dan disaring.
- Semua panggilan HTTP, termasuk halaman Swagger. Penyebabnya tunggal: startup berhenti di seeder
  Hemodialisa. Kodenya tidak menjadi penyebab.
- Pemberian `LabExaminationResult : Update` kepada jabatan mana pun. Itu langkah rilis milik admin
  sistem, dan daftar jabatannya menunggu `UNK-P14-01`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-221` — pemegang `LabExamination : Update` saja menerima **`403`** pada kelima tindakan hasil; `PUT /urgency` tetap `200` | **Terpenuhi pada kode, belum terbukti lewat HTTP** | Kelimanya memakai `LabExaminationResult,Update`, dan `urgency` tetap `LabExamination,Update` (inventaris route) |
| `AC-222` — pemegang `LabExaminationResult : Update` saja: isi hasil `200`; batal, cito, duplo `403` | **Terpenuhi pada kode, belum terbukti lewat HTTP** | Idem. Tidak ada tindakan yang memegang kedua resource sekaligus |
| `AC-224` — laporan Patologi Anatomi berjalan seperti sebelumnya | **Terpenuhi pada kode, belum terbukti lewat HTTP** | Tidak ada berkas laporan PA dalam diff |
| `VAL-122` — Final atas baris PA menjawab `422` dengan pesan yang mengarah ke laporan PA | **Terpenuhi pada kode, belum terbukti lewat HTTP** | `EnsureNotAnatomicalPathology` di ketiga method. Pesannya sama persis dengan `validation-matrix.md` baris 536. `LabExaminationValidationException` sudah dipetakan controller ke `422` |
| DoD — kelima tindakan beralih izin | ✅ **Terpenuhi** | Inventaris route |
| DoD — ketiga tindakan lama tidak berubah | ✅ **Terpenuhi** | `cancel`, `urgency`, `duplo` tetap `LabExamination,Update` |
| DoD — route lama **tidak ada lagi** | ✅ **Terpenuhi** | Nol dari seluruh descriptor aplikasi |
| DoD — `VAL-122` ditegakkan | ✅ **Terpenuhi pada kode** | Lihat baris `VAL-122` di atas |
| DoD — **nol seeder atau migration kebijakan** | ✅ **Terpenuhi** | Diff dua berkas |
| DoD — laporan task | ✅ **Terpenuhi** | Berkas ini |
| Verifikasi — startup Development lolos `PermissionRegistryValidator` | **Terpenuhi sebagian** | Validatornya lolos, tetapi dijalankan lewat harness. Startup sungguhan berhenti **sebelum** validator karena penahan Hemodialisa |
| Verifikasi — kode status dibuktikan terhadap aplikasi yang berjalan | **Belum terpenuhi** | Bagian 5 |

**Yang belum terpenuhi disebut apa adanya:** semua kode status HTTP. Penahannya ada di luar
Laboratorium. Kodenya lengkap dan sesuai kontrak.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** di kedua berkas yang disentuh. `BE-LAB-66` mencatat 224; keenam tambahannya berasal dari berkas lain |
| Masalah yang diketahui | **1. Nama resource di layar Akses Role.** Resource turunan tampil dengan namanya sendiri, `LabExaminationResult`, dan deskripsi resource mewarisi deskripsi controller, *"Pengelolaan pemeriksaan terpesan laboratorium"*. Ini perilaku platform (`PermissionRegistryDescriptor.cs:496-504`) dan sesuai `LAB-PERM-v1` rev 10 bagian 12.2. Yang membedakannya bagi admin adalah teks aksinya. **2. Sudah ada sebelum task ini dan tidak diubah:** baris `LabExamination : Update` tampil sebagai *"Cancel Lab Examination"*, padahal ia juga membuka cito dan duplo, karena `cancel` endpoint pertama yang terbaca. Kini ketiga tindakan itu satu-satunya isi pasangan tersebut, sehingga teks yang tepat adalah *"batal, cito, dan duplo"*. Mengubahnya berarti menyentuh tiga atribut di luar cakupan task ini |
| Risiko tersisa | **Tinggi pada rilisnya, rendah pada kodenya.** Begitu dirilis, **nol analis** dapat menulis hasil sampai admin memberi `LabExaminationResult : Update` (6aj.5). `FE-LAB-35` **wajib dirilis bersama**; tanpanya halaman Mikrobiologi memanggil tiga route yang sudah `404`. Kode status belum dibuktikan lewat HTTP |
| Perubahan sampingan | `NONE` di repository. Harness validator dibuat di scratchpad sesi, di luar repository. Startup yang gagal menulis log ke `Logs/` (di-gitignore) dan menjalankan seeder startup biasa ke database dev bersama (bagian 3.3) |
| Interupsi | **Ada.** Sesi sebelumnya berhenti sesudah implementasi, tepat sebelum build. Dilanjutkan 2026-09-29 dari keadaan terverifikasi: `git diff` dicocokkan butir demi butir dengan 6aj.1, `r33` 28.2-28.4, `LAB-PERM-v1` 12.2-12.6, dan `VAL-122` sebelum build. Nol suntingan ganda |
| Status Git | ` M Areas/HealthServices/LaboratoryManagement/Controllers/LabExaminationController.cs`; ` M Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs`; ditambah laporan ini, `roadmap/backend-roadmap.md`, dan `roadmap/traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Pemilik modul Hemodialisa atau database: terapkan migration `AddHemodialysisManagement` di database dev. Sesudah itu jalankan ulang startup: validator lolos lewat startup sungguhan, Swagger nol memuat route lama, dan `VAL-122` → `422` dengan superadmin. **2.** Untuk `AC-221`/`AC-222`: sediakan dua akun uji, lalu instruksikan pemberian `LabExaminationResult : Update` kepada satu jabatan uji di database dev. **3.** `BE-LAB-68` kini tidak punya penahan kode dan dapat dikerjakan. **4.** `FE-LAB-35` harus berpindah ke route baru sebelum rilis `MVP-8a` |

---

## 8. Verifikasi runtime susulan — 2026-09-29 sore

Penahan Hemodialisa ditutup atas instruksi eksplisit pemilik modul. Migration
`AddHemodialysisManagement` diterapkan ke `QuilvianNewDevYoga` (rinciannya di bagian 8
[`BE-LAB-70.md`](BE-LAB-70.md)). Aplikasi dijalankan pada `http://localhost:5107`, dan uji di bawah
**nol menulis** — setiap penolakan terjadi sebelum penulisan. `Version` dan status Final kedua
pemeriksaan uji dibaca sebelum dan sesudah; hasilnya identik.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Startup Development sungguhan | Seluruh seeder lolos; *"Permission registry valid. 1572 identitas kanonik"* — validator kini berjalan **lewat startup**, bukan harness | `PASS` |
| Swagger `health-services` | Route `result/microbiology/(finalize\|reopen\|consultation)` **nol**; penggantinya `POST /{id}/result/finalize`, `POST /{id}/result/reopen`, `PUT /{id}/result/consultation` ada | `PASS` |
| Route lama dipanggil langsung | `404` | `PASS` |
| **`VAL-122`** — `finalize`, `reopen`, `consultation` atas pemeriksaan **Patologi Anatomi** (Histopatologi Biopsi Besar) | Ketiganya `422` *"Hasil Patologi Anatomi diselesaikan lewat laporan Patologi Anatomi."* | `PASS` |
| `AC-221` / `AC-222` — dua akun | Tidak dijalankan | `NOT RUN` |
| `AC-224` — regresi laporan PA lewat HTTP | Tidak dijalankan | `NOT RUN` |

**Kenapa `AC-221`/`AC-222` masih `NOT RUN`.** Di database dev, **nol jabatan** memegang
`LabExaminationResult : Update`. Jabatan Kepala Instalasi pun tidak memegang `LabExamination`
maupun `LabExaminationResult`, sehingga `403`-nya tidak membuktikan apa pun tentang pemisahan
izin. Memberi `LabExaminationResult : Update` kepada jabatan analis adalah langkah rilis 6aj.5
yang belum diinstruksikan.

**Status tetap ⚠ `SELESAI DENGAN BATAS VERIFIKASI`**, dengan batas yang menyempit: seluruh kode
status yang dapat dibuktikan tanpa izin baru kini terbukti. Yang tersisa hanya uji dua akun.
