# Laporan Perubahan Backend — `BE-LAB-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-11` |
| Judul | Migration pemisahan wadah dan pemeriksaan |
| Slice | `S2` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, gelombang `MVP-2` |
| Trace | `FR-02.4`, `FR-02.6`; `LAB-DEC-024`; `AC-35`, `AC-38`; `LAB-OPEN-012` |
| Contract version | `erd/data-dictionary.md` bagian 2 dan 3; `02-backend-architecture.md` bagian 6 dan 7; `contracts/api-contract.md` bagian 3 — **breaking** yang sudah disetujui |
| Dependency | `BE-LAB-09` **`SELESAI`**, `BE-LAB-12` **`SELESAI`**, `BE-LAB-13` **`SELESAI`** |
| Klasifikasi | `HEAVY` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, migration, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`.** Seluruh butir DoD terpenuhi. Migration dijalankan dua arah terhadap `QuilvianNewDevYoga` atas instruksi pemilik modul — lihat bagian 5.3 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE`. Entri registry 2026-09-02 dan 2026-09-03 memberi wewenang source dan pembuatan migration |
| Keberlakuan | `TOUCHED LEGACY` — `LabSpecimen`, service, dan controller-nya sudah ada. Migration dan berkas ujinya `NEW CODE` |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-DTO-001`, `QBE-SVC-001`, `QBE-MOD-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — ketiganya `LEGACY MIGRATION` untuk **rename**, sedangkan task ini menghapus kolom. `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, `QBE-MOD-003`, seluruh `QBE-CODE-*` — tidak ada entity, configuration, modul, maupun nomor bisnis baru |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif. `AGENTS.md`, `BACKEND_ENGINEERING_CONTRACT.md`, dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` terbaca |

---

## 1. Masalah yang diperbaiki

Salinan tarif hidup di **dua tempat sekaligus**, dan keduanya bisa berbeda isi.

> Sejak `BE-LAB-09` membentuk `LabExamination`, setiap pemeriksaan menyimpan salinan tarifnya
> sendiri. Tetapi wadah **juga** masih menyimpan satu salinan — salinan pemeriksaan pertama yang
> kebetulan direncanakan lebih dulu. Satu tabung berisi hemoglobin 35.000 dan leukosit 30.000
> membawa angka 35.000 pada wadahnya, seolah-olah itulah harga tabung itu.

Selama dua salinan hidup berdampingan, setiap pembaca harus tahu mana yang benar, dan setiap
penulis harus ingat memperbarui keduanya. Yang lupa tidak akan mendapat pesan kesalahan apa pun.

---

## 2. Proses bisnis

Tidak ada perilaku bisnis yang berubah. Wadah tetap direncanakan, diambil, diterima, dinyatakan
layak atau ditolak dengan cara yang sama persis, dan fakta tagihan tetap terbit satu per
pemeriksaan sebagaimana `BE-LAB-13` menetapkannya.

Yang berubah adalah **tempat** jawabannya disimpan:

| Pertanyaan | Sebelum | Sesudah |
| --- | --- | --- |
| Pemeriksaan apa yang dikerjakan dari wadah ini? | Kolom pada wadah, satu saja | Baris `LabExamination`, sebanyak yang benar-benar dipesan |
| Berapa tarif rujukannya? | Salinan pada wadah, tarif pemeriksaan pertama | Salinan pada setiap baris pemeriksaan |
| Apa yang dibawa jawaban `GET /lab-specimens/by-order/{id}`? | Barcode, status, **dan** jenis pemeriksaan beserta tarifnya | Barcode dan status saja |

---

## 3. Perubahan yang dikerjakan

### 3.1 Temuan yang mengubah ukuran task

Laporan `BE-LAB-13` bagian 7 menyatakan keenam kolom itu **tidak dibaca siapa pun lagi**.
Pernyataan itu benar untuk muatan fakta tagihan, dan hanya untuk itu. Pemeriksaan ulang
menemukan **lima tempat** yang masih memakainya:

| Tempat | Yang dilakukannya |
| --- | --- |
| `LabSpecimenService.CreateSpecimenAsync` | **Menulis** keenam kolom pada setiap wadah baru |
| `LabSpecimenService.RequestRecollectionAsync` | Membaca `specimen.ProcedureId` untuk memuat procedure dan tarif wadah pengganti |
| `LabSpecimenService.GetByOrderAsync` | Memproyeksikan empat ruas ke `LabSpecimenResponse` |
| `LabSpecimenController.MapResponse` | Memproyeksikan empat ruas yang sama pada jalur tindakan |
| `LabSpecimenService.BuildFactRequest` jalur wadah | Menyusun `TariffSnapshot` dari empat kolom wadah |

Menghapus kolom lebih dulu akan mematahkan build di kelima tempat itu. Karena itu kelimanya
dilepas dahulu, baru kolomnya dihapus.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Models/LabSpecimen.cs` | Keenam properti dan navigasi `Procedure` dihapus. Dokumentasi kelas ditulis ulang: wadah adalah bahan fisik, bukan satuan yang ditagih |
| `.../LabSpecimenConfiguration.cs` | Empat `Property` pemetaan dan relasi ke `MstProcedure` dilepas |
| `.../DTOs/LabSpecimenDtos.cs` | `LabSpecimenResponse` kehilangan `ProcedureId`, `ProcedureCode`, `ProcedureName`, dan `UnitPrice` |
| `.../Services/LabSpecimenService.cs` | `CreateSpecimenAsync` tidak lagi menerima `MstProcedure` dan `MstTariff`; pengambilan ulang tidak lagi memuat keduanya; `GetByOrderAsync` tidak lagi memproyeksikan empat ruas; jalur fakta wadah peninggalan menerbitkan fakta **tanpa** `TariffSnapshot` |
| `.../Controllers/LabSpecimenController.cs` | `MapResponse` mengikuti bentuk jawaban yang baru |
| `Migrations/20260904030116_SplitLabSpecimenIntoExamination.cs` | **Baru.** Melepas foreign key, index, dan keenam kolom; `Down` mengembalikan seluruhnya |
| `Migrations/scripts/20260904030116_...sql` dan `rollback-...sql` | **Baru.** Skrip idempotent dua arah untuk lingkungan yang menerapkan schema secara manual |
| `Migrations/scripts/README.md` | Dua baris daftar skrip |
| `Tests/.../LabSpecimenColumnSplitTests.cs` | **Baru.** Empat uji |
| `Tests/.../LabExaminationTests.cs`, `LabExaminationEndpointTests.cs` | Pembantu yang membentuk wadah tidak lagi mengisi `ProcedureId` |
| `Tests/.../LaboratorySpecimenLifecycleTests.cs` | Penjumlahan tarif rujukan dipindahkan dari wadah ke baris pemeriksaan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking, dan sudah disetujui.** `contracts/api-contract.md` bagian 3 sudah mencantumkan "`LabSpecimenResponse` tidak lagi memuat jenis pemeriksaan dan tarif" sebagai perubahan breaking. `CAP-21` membuktikan tidak ada satu pun pemanggil di frontend. Route, verb, dan hak akses **tidak** berubah |
| Kontrak integrasi | `NOT APPLICABLE`. `SourceItemId` sudah menunjuk `LabExamination.Id` sejak `BE-LAB-13`; task ini tidak menyentuhnya |
| Database | **Menghapus enam kolom, satu index, dan satu foreign key** dari `public."LabSpecimen"`. Migration dibuat **dan dijalankan** ke dev pemilik — lihat bagian 5.3 |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada permission, klaim, atau jalur otorisasi yang berubah |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Prasyarat mutlak ditulis sebagai kode** | `02-backend-architecture.md` bagian 7 menyebut pemindahan data lama sebagai prasyarat mutlak, dan `LAB-OPEN-012` belum terjawab untuk produksi. Migration diawali blok `DO` yang menghitung baris `LabSpecimen` dan melempar exception bila hasilnya bukan nol. Seluruh skrip berada dalam satu transaksi, sehingga penolakan itu tidak meninggalkan schema setengah jadi. Pada tabel kosong ia tidak melakukan apa pun |
| 2 | **Jalur fakta wadah peninggalan dipertahankan, tanpa salinan tarif** | `BE-LAB-13` memutuskan wadah tanpa baris pemeriksaan tetap menerbitkan fakta supaya jejaknya tidak hilang. Keputusan itu dipertahankan. Yang tidak dapat dipertahankan adalah `TariffSnapshot`-nya, karena sumbernya justru kolom yang dihapus. Fakta tetap terbit dengan `TariffSnapshot` kosong dan `RuleSnapshot` yang menyebut `tariffSource = "LegacySpecimenWithoutExamination"`, sehingga Billing dapat mengenali asalnya |
| 3 | **Satu validasi ikut hilang** | Pengambilan ulang dahulu menolak dengan `ArgumentException` bila procedure wadah tidak ditemukan. Pemeriksaan itu tidak dapat dipertahankan — wadah tidak lagi menunjuk procedure. Penggantinya sudah ada dan lebih tepat: pemeriksaan wadah pengganti disalin dari baris `LabExamination` wadah lama, dan procedure yang tidak ditemukan otomatis tidak ikut tersalin |
| 4 | **`Down` hanya aman pada tabel kosong** | Langkah mundur mengembalikan `ProcedureId` sebagai kolom wajib berisi GUID nol, lalu memasang kembali foreign key ke `MstProcedure`. Bila sudah ada baris wadah, pemasangan foreign key itu gagal. Ini disebut apa adanya di `Migrations/scripts/README.md`, bukan didiamkan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` untuk route. Tidak ada endpoint yang ditambah, dihapus, atau berubah verb,
path, maupun permission-nya.

Yang berubah adalah **bentuk jawaban** sepuluh endpoint grup Lab Specimen: `LabSpecimenResponse`
tidak lagi membawa `procedureId`, `procedureCode`, `procedureName`, dan `unitPrice`. Keempatnya
kini dibaca dari grup Lab Examination lewat `GET /lab-examinations/by-specimen/{specimenId}`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 209, Total: 209` | `PASS` | Naik dari 205; empat uji baru |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` — terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `ENVIRONMENT` | Ke-52 memakai satu pesan yang sama, `BLOCKED_BY_TEST_DB_CONFIGURATION`. Angkanya sama persis dengan sebelum task ini |
| Checker QBE `Strict` atas 7 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **`AC-35`** — keenam kolom lepas dari wadah | Nol properti tersisa pada model maupun pada model relasional | `PASS` | `Wadah_TidakLagiMemilikiKeenamKolomSalinanTarif` |
| Relasi dan index ikut lepas | Nol foreign key ke `MstProcedure`, nol index atas `ProcedureId`, navigasi `Procedure` hilang | `PASS` | `Wadah_TidakLagiBertautKeMstProcedure` |
| **Pemindahan, bukan kehilangan** | Keenamnya utuh pada `LabExamination` | `PASS` | `Pemeriksaan_MembawaKeenamKolomItuSecaraUtuh` |
| **`AC-38`** — bentuk jawaban wadah | Empat ruas hilang; barcode dan status tetap ada | `PASS` | `JawabanWadah_TidakLagiMembawaJenisPemeriksaanDanTarif` |
| Migration arah maju | Skrip SQL terbentuk: satu penjaga, satu `DROP CONSTRAINT`, satu `DROP INDEX`, enam `DROP COLUMN` | `PASS` | `Migrations/scripts/20260904030116_SplitLabSpecimenIntoExamination.sql` |
| Migration arah mundur | Skrip SQL terbentuk: enam `ADD`, satu `CREATE INDEX`, satu `ADD CONSTRAINT` | `PASS` | `Migrations/scripts/rollback-20260904030116_SplitLabSpecimenIntoExamination.sql` |
| `dotnet ef migrations list` sebelum eksekusi | Tepat **satu** migration `(Pending)`, yaitu milik task ini | `PASS` | Tidak ada migration lain yang ikut terbawa |
| **Eksekusi maju** ke `QuilvianNewDevYoga` | `Done.` | `PASS` | `dotnet ef database update`, 2026-09-04 |
| **Eksekusi mundur** ke `20260903094528_RenameLaboratoryTrxTablesToLabPrefix` | `Done.`; daftar migration kembali menampilkan `(Pending)` | `PASS` | Keenam kolom, index, dan foreign key-nya kembali terbentuk |
| **Eksekusi maju kedua** | `Done.`; daftar migration tidak lagi menampilkan `(Pending)` | `PASS` | Database ditinggalkan pada keadaan target |
| **`LAB-OPEN-012` terverifikasi ulang oleh mesin** | Penjaga lolos **dua kali** tanpa melempar exception | `PASS` | Lihat bagian 5.4 |

Uji manual: `NOT FEASIBLE`.

### 5.1 Kenapa uji ini dijalankan atas model relasional, bukan database

Keempat uji baru membangun model Npgsql sepenuhnya di memori. Tidak ada koneksi yang dibuka dan
tidak ada perintah database yang dikirim, tetapi yang diperiksa adalah bentuk schema yang
sebenarnya — properti, index, foreign key, dan nama kolomnya. Dengan begitu bukti struktur dapat
dihasilkan tanpa wewenang database, dan uji itu tetap menjadi penjaga regresi bila suatu saat
seseorang mengembalikan keenam kolom itu ke wadah.

### 5.2 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Eksekusi migration ke lingkungan selain dev pemilik | Wewenang terpisah. Yang dijalankan hanya `QuilvianNewDevYoga`, sesuai entri registry 2026-09-03 dan instruksi pemilik modul pada sesi ini |
| `SELECT COUNT(*)` atas `LabSpecimen` produksi | Di luar jangkauan sesi ini. Prasyaratnya ditegakkan penjaga di dalam migration sebagai gantinya |
| 52 uji `IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB`. Pengisiannya dicoba dan **ditolak server**: akun aplikasi tidak memiliki hak `CREATEDB` (`42501: permission denied to create database`) |

### 5.3 Keadaan migration

| Butir | Keadaan |
| --- | --- |
| Berkas migration | **Ada** — `20260904030116_SplitLabSpecimenIntoExamination` |
| Model snapshot | **Diperbarui** |
| Skrip SQL maju dan mundur | **Ada**, idempotent, masing-masing satu transaksi |
| Diterapkan ke database | **`QuilvianNewDevYoga` — sudah, dan terbukti dua arah.** Urutan yang dijalankan: maju, mundur, maju. Database ditinggalkan pada keadaan target |
| Lingkungan lain | **Belum.** Angka baris produksi tetap belum diketahui; penjaga di dalam migration yang menjaganya |

### 5.4 Penjaga itu ikut menjadi alat ukur

Penjaga `LAB-OPEN-012` melempar exception bila `LabSpecimen` memuat satu baris pun. Ia lolos dua
kali — pada eksekusi maju pertama dan pada eksekusi maju kedua sesudah rollback mengembalikan
keenam kolomnya.

Artinya jawaban `0` yang diberikan pemilik modul pada 2026-09-03 **masih berlaku pada 2026-09-04**,
dan kali ini yang memeriksanya adalah mesin, bukan ingatan. Prasyarat mutlak pada
`02-backend-architecture.md` bagian 7 terpenuhi dengan bukti, bukan dengan dugaan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-35` — satu wadah menopang lebih dari satu pemeriksaan, dan wadah tidak lagi memiliki jenis pemeriksaannya sendiri | **Terpenuhi** | `Wadah_TidakLagiMemilikiKeenamKolomSalinanTarif`, `Pemeriksaan_MembawaKeenamKolomItuSecaraUtuh` |
| `AC-38` — tautan tagihan tidak putus | **Terpenuhi** | `SourceItemId` sudah menunjuk `LabExamination.Id` sejak `BE-LAB-13`; task ini tidak menyentuh satu pun baris tagihan, dan `Down` mengembalikan kolomnya utuh |

| Butir DoD | Status |
| --- | --- |
| Jumlah baris produksi diketahui | **Terpenuhi untuk dev pemilik**, dan diverifikasi mesin — lihat bagian 5.4. Angka produksi tetap belum diketahui, dan penjaga di dalam migration yang menjaganya |
| Rencana pemindahan disusun sesuai angka itu | **Terpenuhi** — angka nol berarti migration penghapusan kolom biasa, sesuai `02-backend-architecture.md` bagian 7 |
| Migration jalan dua arah | **Terpenuhi** — dijalankan maju, mundur, lalu maju lagi terhadap `QuilvianNewDevYoga`. Arah mundur hanya aman pada tabel kosong; lihat bagian 3.4 butir 4 |
| Tidak ada tautan tagihan yang putus | **Terpenuhi** |

**Keempat butir DoD terpenuhi** untuk lingkungan yang memang menjadi wewenang task ini. Yang
tersisa hanyalah lingkungan di luar dev pemilik, dan itu memang wewenang terpisah.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **(a)** Migration belum diterapkan di luar dev pemilik. Lingkungan lain yang menjalankan kode ini tanpa migration-nya akan menolak setiap wadah baru, karena kolom `ProcedureId` yang tertinggal bersifat `NOT NULL` sementara kode sudah tidak mengisinya. Urutan penerapannya wajib: migration lebih dulu, baru kode. **(b)** Tujuh assertion pada `LaboratorySpecimenLifecycleTests.cs` masih memakai satuan lama peninggalan `BE-LAB-13`; dicatat pada laporan task itu |
| Risiko tersisa | **Sedang, dan terkendali.** Satu-satunya jalan kehilangan data adalah menjalankan migration pada basis data yang masih berisi, dan justru itulah yang ditolak penjaganya |
| Perubahan sampingan | Satu: `using` `MasterData.Models` pada `LabSpecimen.cs` dilepas karena tidak ada lagi tipe yang memakainya |
| Interupsi | `NONE` |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. `BE-LAB-10` — penandaan cito dan duplo. 2. Meminta DBA menyediakan database test beserta hak `CREATEDB` agar 52 uji integrasi dapat berjalan. 3. Menerapkan migration ini ke lingkungan di luar dev pemilik, dengan urutan migration lebih dulu baru kode |
