# Laporan Perubahan Backend — `BE-RWI-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-042` |
| Judul | Resep dan pesanan penunjang menyimpan konteks perawatan |
| Slice | `DOK-MVP-1` — fondasi konteks, kolom, tabel visite, pelonggaran |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-042` |
| Trace | `02-backend-architecture.md` §4.5, §4.7, §4.8; `data/data-dictionary.md` §7 s.d. §9; `AC-CAP015-01`; `AC-CAP023-03`; `INV-DOK-01`; `RWI-RULE-024`, `RWI-DEC-046` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-039` — **selesai**, lihat [laporan](BE-RWI-039.md) |
| Klasifikasi | `HEAVY`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 1, database 2, keamanan/auth 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; model dan configuration `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`, `Migrations/`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | 🟡 **Sebagian.** Lima dari enam acceptance criteria terbukti; kriteria 6 — migration maju dan mundur berhasil pada ketiga modul — **belum terbukti** karena tidak ada PostgreSQL yang tersedia |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / tiga modul pemilik: `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement` |
| Pemilik / prefix registry | `PharmacyManagement / Phm` `ACTIVE`; `LaboratoryManagement / Lab` `ACTIVE`; `RadiologyManagement / Rad` **`PLANNED`** |
| Baris registry `Rad` | **Masih `PLANNED` padahal entity `Rad*` sudah ada dan berjalan.** Task ini hanya menambah satu kolom pada entity yang sudah ada, sehingga `QBE-MOD-002` tidak terpicu — larangannya berlaku pada **pembuatan entity pertama**, bukan pada penambahan kolom. Barisnya tetap **wajib dinaikkan menjadi `ACTIVE`** oleh pemilik registry; dicatat sebagai utang terbuka di bagian 7 |
| Applicability | `TOUCHED LEGACY` untuk `TrxPrescription`; `NEW CODE` untuk enum jenis resep yang baru dibuat |
| QBE berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-MOD-001`, `QBE-ENUM-001`, `QBE-API-001`, `QBE-PAGE-001` |
| Archetype | Perubahan bentuk data pada tiga aggregate milik tiga modul, ditambah satu penyaring pada permukaan baca yang sudah ada |
| Database authority | Pembuatan migration `PROVIDED` oleh acceptance criteria task. **Eksekusi migration tidak diberikan dan tidak dilakukan** |
| Frontend | Diperiksa read-only untuk memastikan penyaring baru bersifat opsional dan tidak merusak pemanggil lama |

---

## 1. Masalah yang diperbaiki

Resep dan pesanan pemeriksaan sudah menempel pada kunjungan, tetapi tidak pada perawatannya.
Selama pasien hanya punya satu perawatan, itu tidak terasa. Begitu pasien dirawat dua kali, atau
begitu satu layar harus menampilkan "pesanan perawatan ini saja", kepemilikan perawatannya tidak
dapat dibuktikan tanpa penelusuran berlapis.

Masalah kedua menyentuh keselamatan dan uang sekaligus. **Obat pulang tidak dapat dibedakan dari
obat harian.** Petugas farmasi menerima keduanya dalam bentuk yang sama, padahal keduanya
diperlakukan berbeda: obat harian disiapkan untuk pemakaian di ruangan, obat pulang diserahkan
kepada pasien beserta penjelasan pemakaiannya di rumah.

Masalah ketiga: resep yang dikirim ulang karena sambungan terputus dapat melahirkan dua resep.

Masalah keempat, dan ini selisih yang sudah lama ada: **daftar pesanan laboratorium tidak dapat
disaring kunjungan**, sedangkan daftar pesanan radiologi sudah bisa sejak awal. Akibatnya layar
yang hanya membutuhkan pesanan satu kunjungan terpaksa mengambil **seluruh pesanan laboratorium
rumah sakit** lalu menyaringnya sendiri di sisi klien.

---

## 2. Proses bisnis

**Tujuan.** Resep dan pesanan pemeriksaan dapat dibuktikan miliknya perawatan mana, sehingga
pesanan perawatan A tidak dapat diproses sebagai milik perawatan B.

**Pelaku.** Dokter yang meresepkan dan memesan pemeriksaan; petugas farmasi dan petugas
laboratorium yang membacanya.

**Pemicu.** Resep atau pesanan dibuat dari konteks perawatan pasien.

**Langkah yang berurutan.**

1. Resep lahir dari satu catatan dokter. Konteks perawatannya **diwarisi dari catatan itu**, bukan
   ditanyakan ulang — sehingga resep tidak pernah menunjuk perawatan yang berbeda dari catatannya.
2. Jenis resep ditentukan: biasa, harian, atau obat pulang. Bawaannya biasa.
3. Bila permintaan membawa kunci permintaan, kiriman ulang dengan kunci yang sama tidak melahirkan
   resep kedua.
4. Pesanan laboratorium dan radiologi menyimpan konteks perawatan yang sama.
5. Daftar pesanan laboratorium dapat disaring kunjungan, persis seperti daftar radiologi.

**Aturan yang berlaku.**

- **Ketiga kolom konteks nullable.** Resep dan pesanan poliklinik, IGD, dan medical check-up
  memang tidak punya perawatan rawat inap.
- **Jenis resep bernilai bawaan biasa**, sehingga seluruh baris resep yang sudah ada terbaca
  sebagai resep biasa dan tidak perlu disentuh.
- **Status pemenuhan hanya dibaca.** Sub-modul rawat inap tidak pernah menandai obat sudah
  diserahkan; itu wewenang Farmasi.
- **Penyaring kunjungan bersifat opsional.** Pemanggil lama yang tidak mengirimnya menerima daftar
  yang sama persis seperti sebelumnya.

**Status yang dihasilkan.** Tidak ada status baru. Lifecycle resep dan pesanan tetap milik modul
pemiliknya.

**Jalur tidak normal.** Bila kolom konteks terisi tetapi tidak cocok dengan perawatan milik
kunjungannya, penjagaannya berada pada service konteks klinis dari `BE-RWI-039` dan menghasilkan
penolakan `400`. Pemasangan penjagaan itu pada jalur pemesanan adalah `BE-RWI-052`.

**Hasil akhirnya.** Kepemilikan perawatan dapat dibuktikan pada resep, pesanan laboratorium, dan
pesanan radiologi; obat pulang dapat disaring petugas farmasi di layar mereka sendiri.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs` dan configuration-nya
- `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`
- `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`, service, dan controller-nya
- `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs`, service, dan controller-nya —
  dipakai sebagai pembanding penyaring kunjungan yang sudah ada
- `data/data-dictionary.md` §7 s.d. §9, `02-backend-architecture.md` §4.5, §4.7, §4.8
- `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs` | Tiga kolom: `InpEpisodeId`, `PrescriptionOrderType`, `IdempotencyKey` |
| `Areas/HealthServices/PharmacyManagement/Enums/PrescriptionOrderType.cs` | **Baru.** `Routine`, `Daily`, `Discharge` |
| `Repositories/Configurations/HealthServices/TrxPrescriptionConfiguration.cs` | Bentuk ketiga kolom, foreign key ke perawatan, dua index, satu unique parsial kunci permintaan |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Satu kolom: `InpEpisodeId` |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Bentuk kolom, foreign key, index perawatan-waktu |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Penyaring kunjungan opsional pada daftar pesanan |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Meneruskan penyaring kunjungan, bentuknya sama persis dengan controller radiologi |
| `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs` | Satu kolom: `InpEpisodeId` |
| `Repositories/Configurations/HealthServices/RadiologyManagement/RadOrderConfiguration.cs` | Bentuk kolom, foreign key, index perawatan-waktu |
| `Migrations/20260903094734_AddPrescriptionInpatientContext.cs` | **Baru.** Migration modul Farmasi |
| `Migrations/20260903095022_AddLabOrderInpatientContext.cs` | **Baru.** Migration modul Laboratorium |
| `Migrations/20260903095444_AddRadOrderInpatientContext.cs` | **Baru.** Migration modul Radiologi |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/SupportingOrderAndPrescriptionContextTests.cs` | **Baru.** Uji penyaring kunjungan, konteks perawatan, dan penyaringan jenis resep |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientClinicalSchemaTests.cs` | Uji bentuk ketiga kolom konteks dan enum jenis resep |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu penambahan yang tidak merusak.** `GET /api/v1/health-services/laboratory-management/lab-orders` menerima parameter kueri `encounterId` yang **opsional**. Pemanggil lama yang tidak mengirimnya menerima daftar yang sama persis. Tidak ada endpoint, method, maupun bentuk balasan yang berubah |
| Database | **Lima kolom baru pada tiga tabel milik tiga modul.** `TrxPrescription` menerima tiga kolom — satu di antaranya wajib dengan bawaan `Routine`; `LabOrder` dan `RadOrder` masing-masing satu kolom nullable. Ditambah empat index, satu unique parsial, dan tiga foreign key. **Tiga migration terpisah, satu per modul pemilik**, sehingga tiap pemilik dapat meninjau dan menerapkan miliknya sendiri. **Ketiganya belum diterapkan ke database mana pun** |
| Keamanan/Auth | `NOT APPLICABLE`. Metadata hak akses pada endpoint daftar pesanan laboratorium tidak disentuh: `[AccessAction("Read", …)]` dan `[AccessPermission("LabOrder", "Read")]` tetap apa adanya. Penyaring kunjungan tidak melonggarkan kewenangan; ia hanya mempersempit hasil |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders?encounterId={guid}` | Melihat daftar pesanan laboratorium; bila `encounterId` diisi, hanya pesanan kunjungan itu yang terbaca. Parameter opsional | `LabOrder : Read` |

Bentuknya sengaja dibuat sama persis dengan `GET /api/v1/health-services/radiology-management/rad-orders?encounterId={guid}`
yang sudah ada, supaya kedua daftar pesanan penunjang dipanggil dengan cara yang sama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Ketiga kolom konteks terbentuk nullable | `TrxPrescription`, `LabOrder`, dan `RadOrder` seluruhnya nullable | `PASS` | `InpatientClinicalSchemaTests.KolomKonteksResepDanPesananPenunjang_Terbentuk` |
| Jenis resep memuat rutin, harian, obat pulang, bawaan rutin | Tiga nilai, bawaan `Routine` bernilai `0` | `PASS` | Uji yang sama |
| Daftar pesanan laboratorium disaring kunjungan | Tanpa penyaring `2` baris; disaring perawatan A `1` baris; disaring perawatan B `1` baris; pesanan A tidak terbaca dari B | `PASS` | `SupportingOrderAndPrescriptionContextTests.DaftarPesananLaboratorium_DapatDisaringKunjungan` |
| Konteks perawatan tersimpan pada pesanan laboratorium | Terisi sesuai perawatannya | `PASS` | `…PesananLaboratorium_MenyimpanKonteksPerawatan` |
| Obat pulang tersaring tersendiri menurut jenisnya | Tiga resep berbeda jenis tersimpan; penyaringan jenis obat pulang mengembalikan tepat `1` | `PASS` | `…ResepObatPulang_TersaringMenurutJenisnya` |
| Daftar pesanan radiologi sudah dapat disaring kunjungan dan tidak diubah | Penyaring sudah ada sejak sebelum task ini; berkas service dan controller radiologi tidak disentuh selain penambahan kolom pada model | `PASS` | `RadOrderService.GetListAsync` dan `RadOrderController.GetList`; diff task ini pada modul radiologi hanya berisi satu kolom dan configuration-nya |
| Pembangkitan SQL migration arah maju dan mundur | Keduanya dihasilkan tanpa galat; ketiga migration modul ikut di dalamnya | `PASS` | `dotnet ef migrations script` dua arah |
| **Uji migration maju dan mundur per modul terhadap PostgreSQL** | **Tidak dijalankan** | `NOT RUN` | Lihat "Tidak dijalankan" |
| `dotnet test` seluruh berkas uji SQLite | `Failed: 0, Passed: 219` | `PASS` | Keluaran perintah |

Uji manual: `NOT FEASIBLE`. Menjalankan aplikasi backend memerlukan wewenang eksekusi runtime yang
terpisah.

**Tidak dijalankan:**

- **Uji migration maju-mundur per modul terhadap PostgreSQL sungguhan.** Lingkungan kerja tidak
  memiliki PostgreSQL lokal dan Docker Desktop tidak berjalan, sehingga container sekali pakai
  tidak dapat dinyalakan. Mengarahkannya ke database bersama dilarang tegas, dan itu tidak
  dilakukan.
- Eksekusi ketiga migration ke database mana pun.
- Uji "pesanan perawatan A tidak dapat **diproses** sebagai milik perawatan B" pada jalur
  pemrosesan. Yang diuji di sini adalah **pembacaannya** — pesanan A tidak terbaca dari konteks B.
  Penolakan pada jalur pemrosesan membutuhkan pemasangan service konteks pada jalur pemesanan,
  yang merupakan pekerjaan `BE-RWI-052`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ketiga kolom konteks terbentuk, seluruhnya nullable | Terpenuhi | `…KolomKonteksResepDanPesananPenunjang_Terbentuk`; ketiga migration |
| 2. Jenis resep memuat rutin, harian, dan obat pulang, dengan bawaan rutin | Terpenuhi | Uji yang sama; `…ResepObatPulang_TersaringMenurutJenisnya` |
| 3. Baris resep lama menerima bawaan rutin dan **tidak disentuh** | Terpenuhi pada bentuknya | Migration menambahkan kolom bernilai bawaan `0` dan tidak memuat satu pun perintah `UPDATE` terhadap baris lama |
| 4. Daftar pesanan laboratorium dapat disaring kunjungan | Terpenuhi | `…DaftarPesananLaboratorium_DapatDisaringKunjungan` |
| 5. Daftar pesanan radiologi **sudah** dapat disaring kunjungan dan tidak diubah | Terpenuhi | Penyaringnya sudah ada sejak sebelum task ini dan tidak disentuh |
| 6. Migration maju dan mundur berhasil pada ketiga modul | **Belum terpenuhi** | Ketiga migration ada dan SQL kedua arahnya dihasilkan tanpa galat, tetapi belum dijalankan terhadap PostgreSQL |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | **Belum** — kriteria 6 belum terbukti |
| Tiga migration | Terpenuhi — satu per modul pemilik: `AddPrescriptionInpatientContext`, `AddLabOrderInpatientContext`, `AddRadOrderInpatientContext` |
| Baris registry `Rad` sudah dinaikkan **atau** tercatat sebagai utang terbuka pada laporan | Terpenuhi lewat pencatatan utang terbuka — lihat bagian 7. Registry adalah dokumen milik pemiliknya dan tidak diubah task ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas task ini |
| **Utang terbuka registry** | Baris `RadiologyManagement / Radiology / Rad` pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` masih berstatus `PLANNED`, padahal entity `RadOrder`, `RadStudy`, dan kerabatnya sudah ada dan berjalan. Barisnya **wajib dinaikkan menjadi `ACTIVE`** oleh pemilik registry supaya registry menggambarkan keadaan sebenarnya. Task ini tidak menaikkannya karena registry adalah wewenang pemiliknya, dan penambahan kolom pada entity yang sudah ada memang tidak terhalang olehnya |
| Masalah yang diketahui | Kolom konteks sudah ada tetapi **belum diisi** satu pun jalur pemesanan. Pengisiannya adalah `BE-RWI-050` untuk resep dan `BE-RWI-052` untuk pesanan penunjang |
| Risiko tersisa | Ketiga migration belum pernah dijalankan. Sebelum diterapkan, pemilik masing-masing modul wajib menjalankan uji maju-mundur terhadap PostgreSQL |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu interupsi eksekusi saat rangkaian tiga migration dibuat. Pemulihan dilakukan dengan memeriksa daftar migration yang benar-benar ada dan keadaan berkas model, lalu melanjutkan dari keadaan terverifikasi. Tidak ada migration ganda maupun penyuntingan ganda |
| Status Git | Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Menaikkan baris registry `Rad` menjadi `ACTIVE`, lalu menjalankan uji migration maju-mundur per modul terhadap PostgreSQL sekali pakai |
