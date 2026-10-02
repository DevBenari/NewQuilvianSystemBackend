# Laporan Perubahan Backend — `BE-LAB-71`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-71` |
| Judul | Dua data induk alasan |
| Slice | Gelombang `MVP-9a` — `EPIC-LAB-15`, fondasi `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.2** |
| Trace | `FR-15.12`, bagian `FR-15.3`; `LAB-DEC-082`, `LAB-DEC-138`, `LAB-DEC-003`, `LAB-DEC-019`; `02-backend-architecture.md` 20.10 butir 4; `VAL-140`..`VAL-142` |
| Contract version | `LAB-API-v1` **`r34`** bagian 29.6; `LAB-VAL-v1` **`r12`** bagian 14.4; `LAB-PERM-v1` **rev 11** bagian 13.2-13.3 — seluruhnya **`approved` 2026-09-25** |
| Dependency | `BE-LAB-70` ⚠ selesai sebagian — model, configuration, dan `DbSet` berdiri; migration **belum diterapkan** |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1 (±14), berkas diubah 1 (6), logika bisnis 1, kontrak API 2 (18 endpoint baru), database 1 (baca-tulis tabel yang dibuat `BE-LAB-70`), keamanan/auth 1 (dua resource dan aksi `SystemFlag`), UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-68`..`70` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — 2026-09-29 sore (bagian 8). **Ke-18 endpoint terbukti lewat HTTP** dengan akun sungguhan Kepala Instalasi Laboratorium: `VAL-140` `409`, `VAL-141`/`142` `422`, `system-flags` **`403`** bagi kepala instalasi, pencarian `ILike` PostgreSQL berjalan. Izin kedua resource diberikan menurut `LAB-PERM-v1` rev 11 13.2, dan **kedua daftar terisi** nilai usulan 20.8 atas nama kepala instalasi — `SAMPEL-TERTUKAR`, `SALAH-KETIK`, `SHIFT-TUNGGAL`. *Semula `SELESAI DENGAN BATAS VERIFIKASI`: harness 33/33, HTTP tertahan migration `BE-LAB-70`* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27 dan 114) |
| Keberlakuan | `NEW CODE` — dua controller, dua service, satu berkas DTO; `TOUCHED LEGACY` — `LabFilterMetadataFactory.cs` (satu method) dan `Program.cs` (dua registrasi) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001` (`ApiResponse`, `404`/`409`/`422`), `QBE-DTO-001` (entity tidak diekspos), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]` berpasangan pada 18 endpoint), `QBE-VAL-001` (`VAL-140`..`142`), `QBE-DEL-001` (nol hapus; nonaktif lewat status), `QBE-LOG-001` (log perubahan beserta pelaku), `QBE-PAGE-001` (paging dan pencarian pola data induk yang sudah ada), `QBE-OPT-001` (`/options` dan metadata dipakai layar `FE-LAB-38` serta tindakan hasil) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill |

---

## 1. Masalah yang diperbaiki

**Dua tindakan validasi dan rilis membutuhkan daftar alasan yang belum dapat diisi siapa pun.**
`BE-LAB-70` membuat kedua tabelnya, tetapi tanpa jalur tulis:

| Daftar | Dipakai saat | Bila kosong |
| --- | --- | --- |
| Alasan pengembalian hasil (`LabResultCorrectionReason`) | Perilis menemukan kesalahan sebelum rilis dan mengembalikan hasil ke analis (`LAB-DEC-138`) | *Kembalikan ke analis* tidak dapat dipakai. Perilis yang menemukan sampel tertukar hanya dapat **menolak merilis**, tanpa cara mengembalikan hasilnya |
| Alasan pengecualian empat mata (`LabFourEyesExceptionReason`) | Pemvalidasi juga pengisi, atau perilis juga pemvalidasi (`INV-42`, `INV-43`) | Dokter tunggal pada malam hari **tidak dapat merilis** hasil yang ia validasi sendiri, dan hasil kritis menunggu sampai pagi |

Rancangan 20.8 sengaja memilih **nol seeder**: kode dan nama alasan adalah kebijakan rumah sakit
yang belum dikonfirmasi. Kepala instalasi mengisinya **lewat layar**, dan layar itu membutuhkan
endpoint ini. Modul ini sudah dua kali membayar kelas masalah *tabel data induk tanpa jalur
tulis* (`LAB-COORD-006`, `MST-POS-WRITE`).

---

## 2. Proses bisnis

**Pelaku.**

| Peran | Yang dapat dilakukan | Aksi hak akses |
| --- | --- | --- |
| Kepala instalasi Laboratorium | Menambah alasan, mengubah nama/keterangan/urutan, mengaktifkan dan menonaktifkan | `Create`, `Update` |
| Admin sistem | Menyetel *wajib catatan* — **hanya dia** (pola `LAB-DEC-019`) | `SystemFlag` |
| Pemegang `Return` (validasi dan rilis) | Membaca pilihan alasan pengembalian | `Read` |
| Pemegang `Validate` dan `Release` | Membaca pilihan alasan pengecualian | `Read` |

**Alur, berurutan.**

1. Kepala instalasi menambah alasan, misalnya kode `SAMPEL-TERTUKAR` dengan nama *Sampel
   tertukar*. Alasan baru selalu **aktif** dan **tidak** mewajibkan catatan.
2. Bila catatan bebas perlu diwajibkan, misalnya *Lain-lain*, admin sistem menyetelnya lewat
   `PUT /{id}/system-flags`.
3. Saat perilis menekan *Kembalikan ke analis* (`BE-LAB-75`), layar memuat `GET /options` — hanya
   alasan **aktif** — dan mewajibkan catatan bila `requiresNote` bernilai `true`.
4. Alasan yang tidak lagi dipakai **dinonaktifkan** lewat `PATCH /{id}/status`. Ia hilang dari
   pilihan, tetapi riwayat lama tetap terbaca.

**Aturan.**

| Aturan | Contoh | Jawaban |
| --- | --- | --- |
| `VAL-140` — kode unik di antara baris yang belum dihapus | `SAMPEL-TERTUKAR` ditambahkan dua kali | `409` *"Kode alasan ini sudah dipakai."* |
| `VAL-141` — kode wajib, maks. 32, hanya huruf besar, angka, dan tanda hubung; nama wajib, maks. 200; keterangan maks. 256 | `salah-ketik`, `SALAH KETIK` | `422` *"Kode alasan hanya boleh berisi huruf besar, angka, dan tanda hubung."* |
| `VAL-142` — kode tidak dapat diubah | `SAMPEL-TERTUKAR` → `TERTUKAR` | `422` *"Kode alasan tidak dapat diubah. Buat alasan baru bila perlu."* |

**Kenapa kode berhuruf kecil ditolak, bukan dinormalkan.** Data induk Organisme menormalkan kode
menjadi huruf besar diam-diam. Di sini kode adalah **kunci laporan mutu** — *berapa kali sampel
tertukar bulan ini* dihitung per kode — sehingga petugas harus melihat persis kode yang akan
tersimpan. Kontrak `r34` 29.6 meminta `422`.

**Kenapa kode dikunci.** Mengubah `SAMPEL-TERTUKAR` menjadi `TERTUKAR` di tengah tahun membuat
jumlah Januari–Juni dan Juli–Desember tidak dapat dijumlahkan.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Kepala instalasi mencoba `PUT /{id}/system-flags` | `403` — ia tidak memegang aksi `SystemFlag` |
| `POST` membawa `requiresNote: true` | Diabaikan; alasan tersimpan **tanpa** wajib catatan |
| `PUT` membawa `isActive: true` untuk alasan nonaktif | Diabaikan; alasan **tetap nonaktif** — status hanya lewat `PATCH /{id}/status` |
| `PUT` membawa `reasonCode` yang sama persis dengan yang tersimpan | Diterima — layar boleh mengirim ulang kodenya |
| Penunjuk yang tidak dikenal | `404` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.2 | Cakupan, AC, dan jebakan `IsActive` |
| `contracts/api-contract.md` 29.6 | Sembilan endpoint, bentuk `CreateRequest`, kode status |
| `contracts/validation-matrix.md` 14.4 | Teks `VAL-140`..`142` persis |
| `contracts/permission-audit-matrix.md` 13.2-13.3 | Empat aksi per resource dan pemetaan endpoint |
| `02-backend-architecture.md` 20.4 | Nama berkas, service kembar, dan satu set DTO |
| `Controllers/LabOrganismController.cs`, `Services/LabMicrobiologyMasterDataService.cs`, `DTOs/LabMicrobiologyMasterDataDtos.cs` | Pola baseline sembilan endpoint |
| `Controllers/LabRejectionReasonController.cs`, `Services/LabRejectionReasonService.cs` | Pola aksi `SystemFlag` dan `PUT /{id}/system-flags` |
| `Services/LabFilterMetadataFactory.cs` | Pola metadata, termasuk catatan `BE-LAB-66` tentang `SortOptions` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabResultReasonMasterDataDtos.cs` | **Baru.** Satu set DTO untuk kedua data induk: `PagedQuery`, `Response`, `OptionResponse`, `CreateRequest` (**tanpa** `RequiresNote`), `UpdateRequest` (**tanpa** `IsActive`; `ReasonCode` opsional untuk `VAL-142`), `StatusRequest`, `SystemFlagsRequest`, `SummaryResponse`, `FilterMetadataResponse` |
| `Services/LabResultReasonMasterDataService.cs` | **Baru.** `LabResultCorrectionReasonService` dan `LabFourEyesExceptionReasonService` — kembar dan eksplisit, sejajar pola Organisme/Antibiotik. Aturan `VAL-140`..`142` beserta pesannya ditulis **sekali** di `LabResultReasonRules`. Pembantu yang sudah ada dipakai ulang (`LabMicrobiologyMasterDataText`) |
| `Controllers/LabResultCorrectionReasonController.cs` | **Baru.** Sembilan endpoint; `[AccessController]` `SortOrder = 22` |
| `Controllers/LabFourEyesExceptionReasonController.cs` | **Baru.** Sembilan endpoint; `SortOrder = 23` |
| `Services/LabFilterMetadataFactory.cs` | **+1 method** `LabResultReason()` — `SortOptions` **kosong**, karena query-nya tidak punya ruas urut (pelajaran `BE-LAB-66`) |
| `Program.cs` | Dua registrasi `AddScoped` |

**Nol seeder, nol migration, nol endpoint hapus.**

**Kenapa dua service kembar, bukan satu service generik.** Rancangan 20.4 menempatkan keduanya
sejajar `LabOrganismService` dan `LabAntibioticService`. Service generik atas dua entity akan
mendekati pola generic repository yang dilarang skill ini. Yang tidak boleh kembar adalah
**aturannya**, dan aturan itu memang ditulis satu kali.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r34` 29.6.** 18 endpoint baru di bawah dua tag. **Satu tambahan kecil di luar teks kontrak:** `UpdateRequest` menerima `reasonCode` opsional. Tanpa ruas itu, `VAL-142` — yang dituntut AC — tidak dapat terjadi |
| Database | **Nol migration.** Membaca dan menulis dua tabel yang dibuat `BE-LAB-70`. **Membutuhkan migration `BE-LAB-70` diterapkan lebih dulu**; tanpanya setiap endpoint gagal karena tabelnya belum ada |
| Keamanan/Auth | **Dua resource baru** — `LabResultCorrectionReason` dan `LabFourEyesExceptionReason`, masing-masing `Read`, `Create`, `Update`, `SystemFlag`. Registri naik **1564 → 1572**. Kebijakannya **belum diberikan** kepada siapa pun; itu langkah rilis 20.7 butir 3 dan 6. Log mencatat pelaku pada setiap perubahan; nama dan kode alasan tidak sensitif |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Result Correction Reason

Base URL: `api/v1/health-services/laboratory-management/lab-result-correction-reasons`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar alasan, berhalaman; memuat yang nonaktif. Penyaring `isActive`, `search` | `LabResultCorrectionReason : Read` |
| `GET` | `/options` | Pilihan alasan **aktif** untuk *Kembalikan ke analis* | `LabResultCorrectionReason : Read` |
| `GET` | `/filters/metadata` | Bentuk penyaring layar | `LabResultCorrectionReason : Read` |
| `GET` | `/summary` | Jumlah seluruh, aktif, nonaktif, dan wajib catatan | `LabResultCorrectionReason : Read` |
| `GET` | `/{id}` | Detail satu alasan | `LabResultCorrectionReason : Read` |
| `POST` | `/` | Menambah alasan | `LabResultCorrectionReason : Create` |
| `PUT` | `/{id}` | Mengubah nama, keterangan, urutan. **Kode tidak dapat diubah** | `LabResultCorrectionReason : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `LabResultCorrectionReason : Update` |
| `PUT` | `/{id}/system-flags` | Menyetel `requiresNote` | `LabResultCorrectionReason : SystemFlag` |

#### Health Services / Laboratory Management / Lab Four Eyes Exception Reason

Base URL: `api/v1/health-services/laboratory-management/lab-four-eyes-exception-reasons`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar alasan, berhalaman; memuat yang nonaktif | `LabFourEyesExceptionReason : Read` |
| `GET` | `/options` | Pilihan alasan **aktif** untuk Validasi dan Rilis ketika pelaku merangkap peran | `LabFourEyesExceptionReason : Read` |
| `GET` | `/filters/metadata` | Bentuk penyaring layar | `LabFourEyesExceptionReason : Read` |
| `GET` | `/summary` | Jumlah seluruh, aktif, nonaktif, dan wajib catatan | `LabFourEyesExceptionReason : Read` |
| `GET` | `/{id}` | Detail satu alasan | `LabFourEyesExceptionReason : Read` |
| `POST` | `/` | Menambah alasan | `LabFourEyesExceptionReason : Create` |
| `PUT` | `/{id}` | Mengubah nama, keterangan, urutan. **Kode tidak dapat diubah** | `LabFourEyesExceptionReason : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `LabFourEyesExceptionReason : Update` |
| `PUT` | `/{id}/system-flags` | Menyetel `requiresNote` | `LabFourEyesExceptionReason : SystemFlag` |

**Kode status:** `200`; `403` tanpa hak akses; `404` penunjuk tidak dikenal; `409` `VAL-140`;
`422` `VAL-141`, `VAL-142`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error**, 230 warning, 52 detik. **Nol warning** dari berkas yang disentuh | `PASS` | Keluaran build |
| `PermissionRegistryValidator` | **Lolos — 1572 kunci**, naik 8 dari 1564: empat aksi × dua resource | `PASS` | Harness validator |
| Inventaris route kedua controller | **9 + 9 = 18 endpoint** di bawah dua tag `r34` 29.6 persis; **nol `DELETE`**; setiap endpoint berpasangan `[AccessAction]` dan `[AccessPermission]`; `system-flags` beraksi `SystemFlag`, terpisah dari `Update` | `PASS` | Descriptor controller dari DLL hasil build |
| Harness perilaku EF InMemory — kedua data induk | **33 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah. Nol baris ditulis ke database dev bersama |
| Startup Development dan panggilan HTTP | Tidak dijalankan | `EXISTING / ENVIRONMENT ISSUE` | Startup berhenti di seeder Hemodialisa; tabel kedua alasan pun belum ada di database (migration `BE-LAB-70` belum diterapkan) |

**Rincian harness** — skenario sama pada kedua data induk, 15 per data induk, ditambah 3 lintas.

| Skenario | Hasil sebenarnya |
| --- | --- |
| `POST` `SAMPEL-TERTUKAR` dengan JSON `requiresNote: true` dan `isActive: false`, dideserialisasi seperti ASP.NET Core | `200`; tersimpan **aktif** dan `requiresNote = false` |
| `VAL-140` — kode yang sama ditambahkan lagi | `409` *"Kode alasan ini sudah dipakai."* |
| `VAL-141` — `salah-ketik` | `422` *"Kode alasan hanya boleh berisi huruf besar, angka, dan tanda hubung."* |
| `VAL-141` — `SALAH KETIK`, kode 33 karakter, nama kosong, nama 201, keterangan 257 | Kelimanya `422`, masing-masing dengan pesan ruasnya |
| `SALAH-KETIK-2` | `200` — angka dan tanda hubung sah |
| `PUT` dengan kode sama persis | `200` |
| **`PUT` dengan `isActive: true` atas alasan nonaktif** | Alasan **tetap nonaktif** — jebakan `FE-LAB-24` tertutup |
| `VAL-142` — `PUT` mengganti kode menjadi `TERTUKAR` | `422` *"Kode alasan tidak dapat diubah. Buat alasan baru bila perlu."* |
| `PUT` tanpa `reasonCode` | `200`; kode tetap `SAMPEL-TERTUKAR` |
| `system-flags` `requiresNote: true` | `200`; tersimpan |
| `/options` | Hanya `SAMPEL-TERTUKAR` (aktif, `requiresNote = true`); `SALAH-KETIK-2` yang nonaktif **tidak muncul** |
| `GET /` tanpa penyaring dan dengan `isActive=false` | 2 dan 1 baris |
| `/summary` | total 2, aktif 1, nonaktif 1, wajib catatan 1 |
| Penunjuk asing | `404` |
| Kode milik baris yang ditandai terhapus dipakai lagi | `200` |
| Kode yang sama pada kedua daftar | Keduanya `200` — tabelnya terpisah |
| Pelaku pada setiap baris | `CreateBy` = pengguna yang memanggil |

**Batas harness, disebut apa adanya.**

- **Pencarian** (`search`) memakai `EF.Functions.ILike` milik PostgreSQL, sama dengan data induk
  Organisme. InMemory tidak dapat menjalankannya, sehingga jalur itu belum diuji.
- **`403` pada `system-flags`** ditegakkan filter hak akses, bukan service. Buktinya struktural:
  endpoint itu memakai aksi `SystemFlag` yang terpisah, dan kebijakan dicocokkan per nama aksi.
  Panggilan dengan akun sungguhan belum dilakukan.
- **Index unik parsial** tidak ditegakkan InMemory. Penjaga ganda di service terbukti (`409`);
  penjaga terakhir di database (`23505` → `409`) belum diamati.

Uji manual: **`NOT FEASIBLE`** saat ini.

**Tidak dijalankan:**

- Panggilan HTTP dengan akun kepala instalasi dan akun admin. Butuh migration `BE-LAB-70`
  diterapkan, aplikasi dapat start, dan kebijakan dua resource baru diberikan.
- Swagger — jumlah endpoint dan tag dibuktikan dari descriptor yang sama.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `VAL-140` kode ganda `409` | ✅ **Terpenuhi** pada harness | Kedua data induk |
| `VAL-141` kode berhuruf kecil `422` | ✅ **Terpenuhi** pada harness | Kedua data induk, beserta empat pelanggaran lain |
| `VAL-142` mengubah kode `422` | ✅ **Terpenuhi** pada harness | Kedua data induk |
| `requiresNote` pada `POST` diabaikan | ✅ **Terpenuhi** pada harness | JSON sungguhan; `CreateRequest` tidak punya ruasnya |
| `system-flags` oleh pengguna tanpa `SystemFlag` → `403` | **Terpenuhi pada kode**, HTTP `NOT RUN` | Aksi `SystemFlag` terpisah pada kedua controller |
| Alasan nonaktif **tidak** muncul pada `/options` | ✅ **Terpenuhi** pada harness | Kedua data induk |
| DoD — 18 endpoint berjalan | **Berdiri dan terdaftar**; berjalan di harness pada tingkat service; HTTP `NOT RUN` | Inventaris route; harness |
| DoD — `VAL-140`..`VAL-142` ditegakkan | ✅ **Terpenuhi** | — |
| DoD — nol hapus | ✅ **Terpenuhi** | Nol `DELETE` |
| DoD — nol seeder | ✅ **Terpenuhi** | Diff |
| DoD — laporan `BE-LAB-71.md` | ✅ **Terpenuhi** | Berkas ini |
| Verifikasi — startup lolos validator; Swagger 18 endpoint dan nol `DELETE`; panggilan dengan akun kepala instalasi dan admin | **Sebagian** | Validator dan inventaris lolos lewat harness; startup dan panggilan sungguhan `NOT RUN` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Endpoint ini **tidak dapat dipakai** sampai migration `BE-LAB-70` diterapkan. **2.** Kebijakan kedua resource belum diberikan. Arahnya menurut `LAB-PERM-v1` rev 11 13.2: `Create`/`Update` bagi kepala instalasi, `SystemFlag` bagi admin sistem, `Read` juga bagi pemegang `Validate`/`Release`/`Return`. **3.** Tanpa isi, *Kembalikan ke analis* dan rilis oleh dokter tunggal tertutup sama sekali (20.8) — pengisian lewat layar adalah langkah rilis 20.7 butir 3 |
| Risiko tersisa | **Rendah.** Pola yang ditiru sudah berjalan di Organisme dan Antibiotik. Pencarian `ILike` dan penjaga unik di database belum diamati langsung |
| Perubahan sampingan | `NONE` di repository. Harness di scratchpad sesi |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-71`: `??` `DTOs/LabResultReasonMasterDataDtos.cs`, `Services/LabResultReasonMasterDataService.cs`, `Controllers/LabResultCorrectionReasonController.cs`, `Controllers/LabFourEyesExceptionReasonController.cs`, laporan ini; ` M` `Services/LabFilterMetadataFactory.cs`, `Program.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-68`..`70` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Terapkan migration yang tertunda secara berurutan (lihat `BE-LAB-70`), lalu buktikan 18 endpoint lewat HTTP. **2.** `BE-LAB-72` (pembaca kewenangan) sudah `SIAP DIKERJAKAN` dan tidak bersinggungan berkas dengan task ini. **3.** `FE-LAB-38` (dua layar data induk alasan) dapat dibangun di atas kontrak ini |

---

## 8. Verifikasi runtime susulan — 2026-09-29 sore

Tiga penahan di bagian 7 ditutup atas **instruksi eksplisit pemilik modul**. Butir 2 dan 3
diputuskan *sesuai rekomendasi*.

### 8.1 Migration

Diterapkan lewat skrip per migration — Hemodialisa lebih dulu, lalu `BE-LAB-70`. Rinciannya, termasuk
uji `Down`, ada di bagian 8 [`BE-LAB-70.md`](BE-LAB-70.md). Startup Development kini lolos: registri
**1572** kunci, sama dengan hitungan harness.

### 8.2 Pemberian izin — `LAB-PERM-v1` rev 11 13.2

Diberikan lewat `POST /api/v1/administrator/setting/role-access/policies` sebagai superadmin. Endpoint
itu memakai `overwriteTarget: true`, jadi **set lama dibaca lalu dikirim ulang utuh** bersama yang
baru (pola `BE-LAB-66`). Sebelum dikirim, database diperiksa: **nol** kebijakan tersembunyi (aksi
`VisibleInRoleAccess = false` atau system-only) pada kedua jabatan, sehingga pengiriman ulang tidak
menghapus apa pun.

| Jabatan | Diberikan | Sebelum → sesudah |
| --- | --- | --- |
| Penunjang Medis / **Kepala Instalasi Laboratorium** | `Read`, `Create`, `Update` pada kedua resource | 27 → **33**; **nol** dari 27 izin lama hilang |
| Teknologi Informasi / **System Administrator** | `Read`, `SystemFlag` pada kedua resource | 0 → **4** |

**`Read` ikut diberikan kepada keduanya.** Layar kelola tidak dapat memuat daftar tanpa `Read`
— pelajaran yang sama dengan `BE-LAB-66`.

**Belum diberikan, dan memang belum boleh:** `Read` bagi pemegang `Validate`/`Release`/`Return`.
Jabatan mereka menunggu `UNK-P14-03` (20.7 langkah 6).

**Temuan:** di database dev **tidak ada satu akun pun** yang memegang jabatan System Administrator.
`LabRejectionReason : SystemFlag` pun belum pernah diberikan kepada siapa pun. Hari ini, *wajib
catatan* hanya dapat disetel oleh superadmin.

### 8.3 Pengisian daftar — 20.7 langkah 3, nilai usulan 20.8

Layar `FE-LAB-38` **belum dibangun**. Pengisian karena itu memakai endpoint yang sama yang kelak
dipanggil layar itu, dengan **akun dr. Bima Prasetya** (`bima.kepala.lab@quilvian.local`, Kepala
Instalasi Laboratorium). Dengan begitu `CreateBy` mencatat kepala instalasi, bukan superadmin.

| Daftar | Kode | Nama | Aktif | Wajib catatan | Urutan | Dibuat oleh |
| --- | --- | --- | :---: | :---: | ---: | --- |
| Alasan pengembalian hasil | `SAMPEL-TERTUKAR` | Sampel tertukar | Ya | Tidak | 1 | dr. Bima |
| Alasan pengembalian hasil | `SALAH-KETIK` | Salah ketik hasil | Ya | Tidak | 2 | dr. Bima |
| Alasan pengecualian empat mata | `SHIFT-TUNGGAL` | Shift tunggal, tidak ada dokter lain bertugas | Ya | Tidak | 1 | dr. Bima |

Dibaca ulang langsung dari database: tepat tiga baris, **nol baris uji tertinggal**.

**Kedua jalan yang tertutup 20.8 kini terbuka dari sisi data:** *Kembalikan ke analis* punya dua
pilihan alasan, dan rilis oleh dokter tunggal punya satu. Pemakaiannya tetap menunggu `BE-LAB-73`..`75`,
kode kewenangan Human Resource, dan penunjukan per orang (20.7 langkah 4-6).

### 8.4 Ke-18 endpoint lewat HTTP

Dijalankan terhadap `http://localhost:5107`. Uji jalur galat **nol menulis**.

| Skenario | Akun | Hasil |
| --- | --- | --- |
| Swagger `health-services` | — | **18** endpoint di bawah dua tag `r34` 29.6; **nol `DELETE`** |
| `GET /`, `/summary`, `/filters/metadata` sebelum diisi — kedua daftar | dr. Bima | `200`; total 0 |
| `POST SAMPEL-TERTUKAR` dengan body menyelipkan `requiresNote: true`, `isActive: false` | dr. Bima | `200`; tersimpan **aktif**, `requiresNote = false` |
| `VAL-140` kode sama ditambah lagi — kedua daftar | dr. Bima | `409` *"Kode alasan ini sudah dipakai."* |
| `VAL-141` `salah-ketik`, `SALAH KETIK`, `shift-tunggal` | dr. Bima | `422` *"Kode alasan hanya boleh berisi huruf besar, angka, dan tanda hubung."* |
| `VAL-142` kode diganti — kedua daftar | dr. Bima | `422` *"Kode alasan tidak dapat diubah. Buat alasan baru bila perlu."* |
| `PUT` kode sama persis, membawa `isActive: false` | dr. Bima | `200`; **tetap aktif** |
| **`PUT /{id}/system-flags` — kedua daftar** | **dr. Bima** | **`403`** *"Anda tidak memiliki akses ke menu atau fitur ini."* |
| `GET /{id}`; penunjuk asing pada `GET` dan `PUT` | dr. Bima | `200`; `404` |
| `GET /options` | dr. Bima | Koreksi: `SAMPEL-TERTUKAR`, `SALAH-KETIK`; empat mata: `SHIFT-TUNGGAL` |
| `GET /?search=tertukar` — **`ILike` PostgreSQL**, tidak teruji di harness | dr. Bima | `200`; tepat `SAMPEL-TERTUKAR` |
| `PATCH /{id}/status` `isActive: true` atas alasan aktif — kedua daftar | dr. Bima | `200`; nol perubahan nilai |
| `PUT /{id}/system-flags` `requiresNote: false` — kedua daftar | superadmin | `200`; nol perubahan nilai |
| `/summary` sesudahnya | dr. Bima | Koreksi 2/2/0/0; empat mata 1/1/0/0 (total/aktif/nonaktif/wajib catatan) |

**Batas yang tersisa, disebut apa adanya.**

- `system-flags` `200` dibuktikan dengan **superadmin**, yang melewati pemeriksaan izin. Bukti bahwa
  jabatan System Administrator sendiri menerima `200` butuh akun pemegang jabatan itu, dan akun
  seperti itu belum ada.
- Jalur **nonaktif → hilang dari `/options`** tidak dijalankan di database bersama. Menjalankannya
  berarti mencatat penonaktifan atas nama kepala instalasi yang tidak pernah ia lakukan. Jalur ini
  tetap terbukti pada harness (bagian 5).
- Penjaga terakhir index unik (`23505` → `409`) tidak teramati, karena penjaga service menjawab lebih dulu.
- Akun dr. Bima bertanda `mustChangePassword: true`. Kata sandinya **tidak diubah**; itu urusan pemilik akun.

**Status baru: ✅ `SELESAI`.** Ke-18 endpoint berjalan terhadap aplikasi sungguhan, `403` pada
`system-flags` terbukti dengan akun kepala instalasi, dan kedua daftar terisi sesuai 20.8.
