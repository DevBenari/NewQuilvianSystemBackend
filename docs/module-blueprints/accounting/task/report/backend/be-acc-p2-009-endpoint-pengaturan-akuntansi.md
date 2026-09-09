# Laporan Perubahan Backend — `BE-ACC-P2-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-009` |
| Judul | Endpoint pengaturan akuntansi |
| Slice | Gelombang `P2-0a` |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-009` |
| Trace | `ACC-DEC-054`, turunan `ACC-DEC-053`; `FR-P2-032`; `ACC-DEC-037`, `ACC-DEC-022` |
| Contract version | `ACC-API-0.8` grup Configuration; `ACC-VALIDATION-0.6` bagian 5 |
| Dependency | `BE-ACC-P2-004` ✅ — tabel `AccAccountingConfiguration` sudah ada di database |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (5 berkas), logika bisnis 1, kontrak API 1, database 1, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `1812446` |
| Tanggal | 9 September 2026 |
| Status | **✅ `DONE`** — 4 dari 4 acceptance lulus, nol migration, database tidak disentuh |

---

## 1. Masalah yang diperbaiki

`BE-ACC-P2-003` sudah mendirikan tabel tempat akun laba ditahan disimpan, dan `BE-ACC-P2-004`
sudah menerapkannya ke database. Tetapi **belum ada satu pun cara mengisinya** — tabelnya berdiri
kosong, dan tidak ada endpoint yang dapat menuliskannya.

Akibatnya `BE-ACC-P2-010` tutup tahun tidak dapat dikerjakan: ia menuntut akun laba ditahan sudah
ditetapkan, dan menolak `422` bila belum.

### Kenapa pemeriksaannya harus keras

Akun laba ditahan adalah tujuan seluruh selisih pendapatan dikurangi beban pada akhir tahun.
Salah menunjuknya **tidak menimbulkan error sama sekali**:

- jurnal penutupnya tetap seimbang, sehingga seluruh pemeriksaan jurnal lolos;
- laporan tetap terbit;
- yang terjadi hanya laba mendarat di akun yang keliru — dan angka itu terbawa ke tahun
  berikutnya sebagai saldo awal.

Kesalahan seperti itu baru ketahuan saat audit, ketika membalikkannya berarti mengoreksi dua tahun
buku sekaligus. Satu-satunya kesempatan menangkapnya adalah **saat penetapan**, dan itulah yang
dikerjakan task ini.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pemilik proses akuntansi | Membuat akun `3-2001 Laba Ditahan` di daftar akun — berjenis Ekuitas, menerima transaksi |
| 2 | Pemilik proses akuntansi | Membuka layar pengaturan akuntansi |
| 3 | Sistem | Menampilkan `isConfigured: false` — belum ditetapkan |
| 4 | Pemilik proses akuntansi | Memilih `3-2001`, menyimpan |
| 5 | Sistem | Memeriksa lima hal, lalu menyimpan satu baris pengaturan |
| 6 | Sistem | Panggilan berikutnya menampilkan akunnya lengkap dengan kode dan nama |

Langkah 1 **wajib lebih dahulu**. Pengaturan hanya boleh menunjuk akun yang sudah ada — mengisi
pengaturan sebelum akunnya dibuat akan menghasilkan penunjuk yang menggantung.

### 2.2 Lima pemeriksaan saat menetapkan

| Yang diperiksa | Kode | Pesan bagi pengguna |
| --- | :---: | --- |
| Akun ada di daftar akun | `422` | "Akun yang ditunjuk tidak ditemukan pada daftar akun." |
| Akun milik badan hukum yang sama | `422` | "Akun laba ditahan harus milik badan hukum yang sama." |
| **Akun berjenis Ekuitas** | `422` | "Akun laba ditahan harus akun berjenis Ekuitas." |
| **Akun menerima transaksi** | `422` | "Akun laba ditahan harus akun yang menerima transaksi, bukan akun induk." |
| Akun masih aktif | `422` | "Akun laba ditahan harus akun yang masih aktif." |

Seluruhnya `422`, bukan `400`: bentuk permintaannya benar — sebuah `Guid` yang sah — yang salah
adalah akun yang ditunjuknya.

### 2.3 Contoh berangka

Rumah Sakit Uji punya empat akun ekuitas:

| Kode | Nama | Jenis | Menerima transaksi | Hasil bila ditunjuk |
| --- | --- | --- | :---: | --- |
| `3-0000` | Ekuitas | Equity | Tidak (akun induk) | **Ditolak** — akun induk |
| `3-1001` | Modal Disetor | Equity | Ya | Diterima secara teknis |
| `3-2001` | Laba Ditahan | Equity | Ya | **Diterima** — inilah yang benar |
| `1-1001` | Kas Besar | Asset | Ya | **Ditolak** — bukan Ekuitas |

Baris `3-1001` menunjukkan batas kemampuan sistem: `Modal Disetor` lolos seluruh pemeriksaan
karena ia memang akun ekuitas yang menerima transaksi. **Yang membedakannya dari `Laba Ditahan`
adalah kebijakan akuntansi, bukan data** — dan `ACC-DEC-054` memang menyerahkan pemilihan kode
akun pastinya kepada pemilik proses akuntansi. Sistem menutup kesalahan yang dapat dilihatnya;
sisanya tetap keputusan manusia.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Badan hukum belum punya pengaturan | `GET` menjawab **`200`** dengan `isConfigured: false` — bukan `404` |
| Ditetapkan dua kali | Baris yang **sama** diperbarui; tetap satu baris |
| Badan hukum tidak tunggal | Ditolak `AccountingLegalEntityGuard`, seperti seluruh endpoint Accounting |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `009` dan `010`; `contracts/api-contract.md` grup Configuration;
`contracts/validation-matrix.md` bagian 5; `ACC-DEC-054`, `ACC-DEC-053`, `ACC-DEC-037`,
`ACC-DEC-022`; `AccAccountingConfiguration.cs` beserta configuration-nya; `AccChartOfAccount.cs`;
`AccountType.cs`; `ChartOfAccountController.cs` dan `JournalTypeController.cs` sebagai pola
`[AccessController]`; `AccountingLegalEntityGuard.cs`; `Program.cs` blok DI Accounting.

### 3.2 Berkas yang berubah

Lima berkas — empat baru, satu diperbarui.

| Berkas | Perubahan |
| --- | --- |
| `.../MasterData/Configuration/Controllers/AccountingConfigurationController.cs` | **Baru.** Dua endpoint |
| `.../MasterData/Configuration/Services/AccAccountingConfigurationService.cs` | **Baru.** Baca, tetapkan, dan lima pemeriksaan |
| `.../MasterData/Configuration/DTOs/AccountingConfigurationDtos.cs` | **Baru.** `AccountingConfigurationResponse`, `UpdateAccountingConfigurationRequest` |
| `Tests/.../AccountingManagement/AccAccountingConfigurationTests.cs` | **Baru.** 14 uji acceptance |
| `Program.cs` | **Satu baris** `AddScoped<AccAccountingConfigurationService>()` dan satu `using` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru**, sesuai `ACC-API-0.8` grup Configuration. Nol endpoint yang sudah ada berubah |
| Database | **Nol migration.** Menulis ke `AccAccountingConfiguration` yang sudah diterapkan `BE-ACC-P2-004` ✅ |
| Keamanan/Auth | Controller baru dengan `ControllerName = "AccountingConfiguration"`; dua action `Read` dan `Update`. Nol pelonggaran |

### 3.4 Keputusan yang perlu dijelaskan

**Bentuknya `PUT` yang menetapkan, bukan pasangan `POST` dan `PUT`.** Ini yang mewujudkan
acceptance (3): satu badan hukum satu pengaturan. Dengan bentuk menetapkan, aturan itu **mustahil
dilanggar dari sisi pemanggil** — tidak ada jalan membuat baris kedua. Penjaga terakhirnya tetap
unique index `(LegalEntityId)` di database, sesuai `BE-ACC-P2-003`.

**Badan hukum diambil dari route, bukan dari badan permintaan.** Sehingga tidak mungkin ada
permintaan yang alamatnya satu badan hukum tetapi isinya badan hukum lain.

**Penilaian akun laba ditahan dibuat `public static`.** `BE-ACC-P2-010` memakainya lewat
`AmbilAkunLabaDitahanAsync`, supaya tutup tahun dan layar pengaturan memberi jawaban yang sama.
Kalau keduanya menilai sendiri-sendiri, tutup tahun dapat menolak "belum ditetapkan" sementara
layar menampilkan akun yang sudah terisi. Pola `public static` menerima `ApplicationDbContext`
mengikuti catatan mengikat roadmap.

**`GET` menjawab `200` untuk badan hukum yang belum punya pengaturan.** Belum diisi adalah keadaan
wajar bagi rumah sakit yang baru memakai modul ini. Menjadikannya `404` memaksa layar menampilkan
kegagalan untuk sesuatu yang normal, dan membuat "belum diisi" tidak dapat dibedakan dari "gagal
dibaca" — dua hal yang sangat berbeda artinya bagi penggunanya.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Configuration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/configuration/{legalEntityId}` | Menampilkan pengaturan akuntansi satu badan hukum. Menjawab `200` dengan `isConfigured: false` bila belum ditetapkan | `AccountingConfiguration : Read` |
| `PUT` | `/api/v1/corporate/accounting/configuration/{legalEntityId}` | Menetapkan akun laba ditahan. `422` bila akun bukan Ekuitas, bukan penerima transaksi, milik badan hukum lain, tidak aktif, atau tidak ditemukan | `AccountingConfiguration : Update` |

### Delta kontrak yang dicatat

| Delta | Alasan |
| --- | --- |
| **Dua penolakan di luar tulisan kartu** — akun milik badan hukum lain, dan akun nonaktif | Kartu menyebut dua penolakan (bukan Ekuitas, akun induk); kontrak menyebut dua yang sama. Keduanya yang ditambahkan menutup kesalahan sejenis yang **sama-sama tidak menimbulkan error**: akun badan hukum lain membuang laba ke buku besar orang lain (`ACC-DEC-037`), dan akun nonaktif membuat tutup tahun gagal pada akhir tahun buku. **Menunggu ratifikasi** |
| `GET` menjawab `200`, bukan `404`, saat belum ditetapkan | Kontrak tidak menyebut perilaku ini. Lihat bagian 3.4 |
| `AccountingConfigurationResponse` / `UpdateAccountingConfigurationRequest`, bukan `...Dto` | Konvensi source modul memakai akhiran `Response`/`Request` |
| `[Tags("Corporate / Accounting / Master Data / Configuration")]` memakai ` / ` | Kontrak menulis ` - `; seluruh controller Accounting yang sudah ada memakai ` / `. Mengikuti source supaya pengelompokan Swagger tidak pecah |
| Bidang `IsConfigured`, `RetainedEarningsAccountCode`, `...Name`, `...Type` | Tanpa kode dan nama akun, layar hanya menerima `Guid` dan harus memanggil daftar akun lagi hanya untuk menampilkan satu baris |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -c Release --no-incremental` | `0 Error(s)`, `145 Warning(s)` | `PASS` | Angka sama persis — nol warning baru |
| 14 uji `BE-ACC-P2-009` di `UnitTests.Sqlite` | `Failed: 0, Passed: 14` | `PASS` | `AccAccountingConfigurationTests` |
| Seluruh project `UnitTests.Sqlite`, 507 uji | `Failed: 3, Passed: 504` | `EXISTING / ENVIRONMENT ISSUE` | **Nol regresi**; 490 → 504 tepat `+14` |
| QBE checker atas 5 berkas | `VIOLATION: 0`, `PASS` | `PASS` | Keluaran checker |

### Rincian uji

| Uji | Yang dibuktikan |
| --- | --- |
| `AkunBukanEkuitas_Ditolak422` (4 kasus: `Asset`, `Liability`, `Revenue`, `Expense`) | **Acceptance 1**, dan nol baris tersimpan saat ditolak |
| `AkunIndukYangTidakMenerimaTransaksi_Ditolak422` | **Acceptance 2** |
| `AkunMilikBadanHukumLain_Ditolak422` | Delta — laba tidak dapat dibuang ke buku besar badan hukum lain |
| `AkunNonaktif_Ditolak422` | Delta — tutup tahun tidak gagal di akhir tahun buku |
| `AkunTidakDitemukan_Ditolak422` | Penunjuk menggantung ditolak |
| `MenetapkanDuaKali_TetapSatuBaris` | **Acceptance 3** — baris yang **sama** diperbarui, `Id`-nya tidak berubah |
| `BelumAdaPengaturan_MenjawabBerhasilDenganIsConfiguredSalah` | `GET` tidak memperlakukan "belum diisi" sebagai kegagalan |
| `SesudahDitetapkan_PengaturanTerbacaLengkap` | Kode, nama, dan jenis akun ikut terbaca |
| `PenilaianUntukTutupTahun_SamaDenganEndpoint` | `AmbilAkunLabaDitahanAsync` sejalan dengan endpoint — mencegah `010` dan layar berselisih |
| `KeduaEndpoint_MembawaHakAksesYangBenar` (2 kasus) | **Acceptance 4**, termasuk kecocokan dengan `ControllerName` |

**Tidak dijalankan:**

- **Test integrasi PostgreSQL** — kolom Verifikasi kartu ini meminta `UnitTests.Sqlite`, dan itulah
  yang dijalankan. Berbeda dari `005` dan `006` yang memang meminta PostgreSQL, jadi task ini
  **tidak** kekurangan verifikasi yang diminta.
- Migration apa pun — nol dampak schema.
- Pemanggilan endpoint terhadap database sungguhan.

Uji manual: `NOT FEASIBLE` — layarnya `FE-ACC-P2-005`, belum dibuat.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Akun bukan `Equity` ditolak `422` | **Terpenuhi** | `AkunBukanEkuitas_Ditolak422`, empat jenis akun diuji |
| 2 | Akun induk ditolak `422` | **Terpenuhi** | `AkunIndukYangTidakMenerimaTransaksi_Ditolak422` |
| 3 | Satu badan hukum hanya punya satu pengaturan | **Terpenuhi** | `MenetapkanDuaKali_TetapSatuBaris`; bentuk `PUT` menetapkan + unique index `(LegalEntityId)` |
| 4 | Kedua endpoint membawa `[AccessPermission]` | **Terpenuhi** | `KeduaEndpoint_MembawaHakAksesYangBenar` |

**Empat dari empat terpenuhi.**

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Dua endpoint berjalan | **Ya** |
| Test hijau | **Ya** — 14 uji, seluruhnya di `UnitTests.Sqlite` sesuai permintaan kartu |
| Laporan task tertulis | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `✅` |
| `requirement-traceability-phase2.md` diperbarui | **Ya** — `FR-P2-032` |

**Nol butir DoD dikecualikan.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 145 warning solution, seluruhnya pre-existing. Nol dari task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Temuan yang perlu diperbaiki | **`NONE`** — nol cacat ditemukan pada kode yang disentuh task ini |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **Sistem tidak dapat membedakan `Laba Ditahan` dari `Modal Disetor`.** Keduanya akun ekuitas yang menerima transaksi, jadi keduanya lolos. Yang membedakan adalah kebijakan akuntansi, dan `ACC-DEC-054` memang menyerahkannya ke pemilik proses. Disebut di sini supaya tidak dikira sistem sudah menjaminnya | Pemilik proses akuntansi |
| 2 | Dua penolakan tambahan belum ada di kontrak `ACC-API-0.8` | Rizki — ratifikasi |
| 3 | Satu baris ditambahkan ke `Program.cs`, mengikuti blok DI Accounting yang menyatakan "satu baris per service modul" | Rizki |
| 4 | ~~Jenis jurnal `JT` belum terisi~~ — **SELESAI** 9 Sep 2026. **Diisi 9 September 2026** lewat `AccJournalTypeService.SeedAsync` sebagai `superadmin`: `Inserted: 1, Skipped: 4`, master menjadi 5 baris. Prasyarat `BE-ACC-P2-010` yang ini sudah terpenuhi | Selesai |
| 5 | `SwaggerDocumentationTests` tidak dapat lulus pada Release | Owner Backend |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Akun ekuitas yang keliru tetap dapat ditetapkan** | Lihat masalah nomor 1. Mitigasinya bukan kode, melainkan pengisian data oleh orang yang memahami daftar akunnya |
| Pengaturan belum terisi | Selama belum ditetapkan, `BE-ACC-P2-010` akan menolak `422`. Itu perilaku yang dikehendaki, bukan cacat |

### Status Git

```text
 M Program.cs
?? Areas/Corporate/AccountingManagement/MasterData/Configuration/Controllers/
?? Areas/Corporate/AccountingManagement/MasterData/Configuration/DTOs/
?? Areas/Corporate/AccountingManagement/MasterData/Configuration/Services/
?? Tests/.../AccountingManagement/AccAccountingConfigurationTests.cs
```

Ditambah berkas `005`, `006`, dan dua perbaikan yang juga belum di-commit.
**Nol commit, push, stage, merge, atau rebase dilakukan.** **Nol berkas frontend disentuh.**

### Langkah berikutnya

**`BE-ACC-P2-010`** — pratinjau dan penyusunan jurnal penutup tahun. Kedua dependency-nya kini
selesai: `006` 🟡 dan `009` ✅.

Dua hal yang **wajib** disiapkan lebih dahulu, keduanya pengisian data, bukan kode:

1. ~~**Jenis jurnal `JT`** harus benar-benar ada di database~~ — **SUDAH TERPENUHI**
   9 September 2026. **Diisi 9 September 2026** lewat `AccJournalTypeService.SeedAsync` sebagai `superadmin`: `Inserted: 1, Skipped: 4`, master menjadi 5 baris.
2. **Akun laba ditahan harus ditetapkan** lewat endpoint yang baru saja dibuat task ini, dan
   akunnya harus dibuat dulu di daftar akun. **Ini satu-satunya prasyarat yang tersisa.**

Tanpa keduanya, `010` akan menolak `422` — dan itu justru bukti bahwa pemeriksaannya bekerja.
