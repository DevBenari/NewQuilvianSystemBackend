# Laporan Perubahan Backend — `BE-LAB-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-17` |
| Judul | Metadata penyaring dan rekap untuk kelima grup |
| Slice | `S3`, `S11` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 3, gelombang `MVP-0` |
| Trace | Instruksi pemilik modul 2026-09-03; `rules/backend/master-data-endpoint-standard.md` bagian 1, 2.1, dan 2.2; `LAB-DEC-023`, `LAB-DEC-019`, `LAB-INH-011` untuk penanda keselamatan yang ikut diumumkan |
| Contract version | `LAB-API-v1` **`r4`** — amandemen aditif atas `r3`, disetujui pemilik modul 2026-09-03 |
| Dependency | `BE-LAB-01` .. `BE-LAB-06` — seluruhnya **`SELESAI`** |
| Klasifikasi | `HEAVY` — skor 9. Repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 2, database 1, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, kontrak dan artefak blueprint modul Laboratorium |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `17a331b`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — sepuluh endpoint tersedia, kontrak naik ke `r4`, checker QBE `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | Tidak ada |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Status registry | Terdaftar dan `ACTIVE`. Tidak ada entity baru, sehingga `QBE-MOD-002` dan `QBE-MOD-003` tidak berlaku pada task ini |
| Keberlakuan | `NEW CODE` untuk DTO, factory metadata, dan kesepuluh endpoint. `TOUCHED LEGACY` untuk `LabOrderController` dan `LabSpecimenController` yang sudah ada sebelum blueprint ini |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-OPT-001`, `QBE-MOD-001`, `QBE-NAM-002`, `QBE-ENUM-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` .. `QBE-ENT-003`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-DB-002`, `QBE-NAM-003` — tidak ada entity, configuration, maupun migration. `QBE-CODE-001` .. `QBE-CODE-006` — tidak ada alokasi nomor bisnis. `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DEL-001` — seluruh endpoint baca saja |
| Catatan `QBE-OPT-001` | Aturan ini menyatakan metadata dan options disediakan **hanya bila dikonsumsi**. Pemilik modul menyatakan keduanya dikonsumsi layar Laboratorium dan meminta bentuknya disamakan dengan modul Rekam Medis. Itulah keputusan konsumen yang diminta aturan tersebut |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, layar Laboratorium tidak punya sumber tunggal untuk menjawab dua
pertanyaan yang selalu muncul saat halaman daftar dibuka: **apa saja pilihan yang boleh dipilih
pengguna**, dan **berapa angka ringkasannya**.

Akibat nyatanya bagi tim frontend:

> Layar daftar pesanan laboratorium perlu menampilkan tapis status. Tanpa endpoint metadata,
> daftar status itu harus ditulis ulang di dalam kode frontend — `Requested`, `Accepted`,
> `InProcess`, dan seterusnya — beserta terjemahan Indonesianya. Begitu backend menambah satu
> status baru, layar tetap menampilkan daftar lama tanpa ada yang menyadarinya, dan petugas
> tidak pernah dapat menyaring status yang baru itu.

Masalah kedua lebih halus. Beberapa aturan keselamatan modul ini hanya hidup di dalam service:
batas kritis yang tidak boleh diubah langsung, pengaju yang tidak boleh menyetujui pengajuannya
sendiri, dan dua penanda alasan penolakan yang terkunci dari kepala instalasi. Frontend
mengetahuinya hanya lewat kesepakatan lisan.

> Kepala instalasi Pak Hendra membuka layar alasan penolakan dan melihat kolom "kesalahan
> internal rumah sakit" tampil biasa, dapat dicentang. Ia mencentangnya, menekan simpan, lalu
> menerima penolakan `403`. Menurut `LAB-FE-012`, keadaan itu sendiri sudah pelanggaran:
> pengguna harus tahu **sebelum** mencoba, bukan setelah gagal.

Endpoint metadata kini membawa penanda-penanda itu sebagai data, sehingga layar dapat
menggembok kolomnya sejak render pertama tanpa menebak.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Aspek | Isi |
| --- | --- |
| Tujuan | Setiap layar Laboratorium memperoleh pilihan penyaring, label, dan angka ringkasan dari backend, bukan dari daftar yang ditanam di frontend |
| Pelaku | Seluruh pengguna yang sudah punya hak baca pada grup yang bersangkutan. Tidak ada hak akses baru |
| Pemicu | Layar daftar dibuka |
| Hasil akhir | Layar merender penyaring dan kartu ringkasan yang selalu selaras dengan backend |

### 2.2 Langkah yang berurutan

1. Layar dibuka dan memanggil `GET /filters/metadata` lebih dulu. Panggilan ini **tidak
   menyentuh database sama sekali** — isinya murni keterangan bentuk.
2. Dari jawabannya, layar merender tapis status, tapis disiplin, pilihan urutan, pilihan ukuran
   halaman, dan mengunci ruas yang disebut terkunci.
3. Layar memanggil `GET /summary` untuk kartu angka di bagian atas halaman.
4. Barulah layar memanggil daftar utamanya.

### 2.3 Dua bentuk rekap, dan alasannya

Kelima grup tidak diperlakukan sama, dan itu disengaja:

| Grup | Rentang waktu | Alasan |
| --- | :---: | --- |
| Lab Order | **Ya** | Pesanan adalah catatan kejadian. Yang berarti bagi kepala instalasi adalah "berapa pesanan bulan ini", bukan "berapa pesanan sejak sistem menyala" |
| Lab Specimen | **Ya** | Sama, dan angka sebab pengambilan ulangnya hanya berarti bila dibatasi periode |
| Lab Value Bound | Tidak | Data induk. Yang ingin diketahui adalah berapa banyak batas yang **berlaku sekarang** |
| Lab Rejection Reason | Tidak | Data induk, alasan yang sama |
| Lab Critical Bound Approval | Tidak | Ber-scope satu batas nilai; jumlah pengajuannya sedikit dan seluruhnya relevan |

### 2.4 Contoh berangka — rekap wadah

Misalkan sepanjang September tercatat 120 wadah:

| Angka | Nilai | Artinya bagi pengguna |
| --- | ---: | --- |
| `TotalWadah` | 120 | Seluruh wadah yang direncanakan bulan itu |
| `DinyatakanLayak` | 104 | Yang lolos pemeriksaan kelayakan |
| `Ditolak` | 16 | Yang tidak layak periksa |
| `KesalahanInternalRumahSakit` | 11 | **Dari 16 penolakan itu, 11 adalah kelalaian rumah sakit** — dan pengambilan ulangnya tidak boleh ditagihkan kepada pasien |
| `KondisiPasienAtauSampel` | 4 | Boleh dipertimbangkan penagihannya oleh Billing |
| `SebabEksternal` | 1 | Sama |

Angka keempat itulah yang membuat rekap ini bukan sekadar hiasan halaman: ia menjawab
pertanyaan biaya yang selama ini hanya bisa dihitung manual satu per satu.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | :---: |
| `startDate` melewati `endDate` pada rekap pesanan atau wadah | Ditolak beserta pesan yang dapat ditampilkan apa adanya | `400` |
| Rentang waktu tidak dikirim sama sekali | Dipakai 30 hari terakhir | `200` |
| Rekap pengajuan diminta untuk batas nilai yang tidak ada | Ditolak, bukan mengembalikan rekap kosong yang menyesatkan | `404` |
| Tabelnya kosong | Seluruh angka bernilai nol | `200` |
| Pemanggil tidak memegang hak baca grup itu | Ditolak filter permission yang sudah ada | `403` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordAccessLogController.cs` — bentuk acuan yang diminta pemilik modul
- `Areas/HealthServices/MedicalRecordManagement/DTOs/MedicalRecordFilterAndSummaryDtos.cs`, `MedicalRecordAccessLogDtos.cs`
- `rules/backend/master-data-endpoint-standard.md` bagian 1, 2.1, 2.2, dan 6
- Kelima controller dan service Laboratorium beserta DTO penyaringnya
- `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabFilterAndSummaryDtos.cs` | **Baru.** Tiga bentuk bersama dan sepuluh DTO metadata/rekap, satu pasang per grup |
| `Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs` | **Baru.** Penyusun metadata kelima grup beserta label Indonesia untuk enam enum. Murni fungsi, tanpa kueri database |
| `.../Services/LabOrderService.cs` | Bertambah `GetFilterMetadata()` dan `GetSummaryAsync(start, end, ct)` |
| `.../Services/LabSpecimenService.cs` | Bertambah `GetFilterMetadata()` dan `GetSummaryAsync(start, end, ct)` |
| `.../Services/LabValueBoundService.cs` | Bertambah `GetFilterMetadata()` dan `GetSummaryAsync(ct)` |
| `.../Services/LabCriticalBoundApprovalService.cs` | Bertambah `GetFilterMetadata()` dan `GetSummaryAsync(valueBoundId, ct)` |
| `.../Services/LabRejectionReasonService.cs` | Bertambah `GetFilterMetadata()` dan `GetSummaryAsync(ct)` |
| Kelima controller Laboratorium | Masing-masing bertambah dua endpoint `GET`, beserta `[AccessAction]` dan `[AccessPermission]` yang mengikuti hak baca grupnya |
| `.../Controllers/LabOrderController.cs` | **Selain itu:** tiga komentar XML pada action dihapus atas permintaan pemilik modul — lihat bagian 7.1 |
| `Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordAccessLogController.cs` | Satu komentar XML pada action `pending-review` dihapus, alasan yang sama |
| `tests/.../LabFilterAndSummaryTests.cs` | **Baru.** 31 uji |
| `tests/.../LabValueBoundServiceTests.cs`, `LabCriticalBoundApprovalTests.cs`, `LabRejectionReasonServiceTests.cs` | Jumlah endpoint yang ditegakkan uji kontrak dinaikkan mengikuti amandemen `r4` |
| `contracts/api-contract.md` | Revision `3` → `4`; sepuluh baris endpoint ditambahkan pada kelima grup |
| `roadmap/backend-roadmap.md` | `BE-LAB-17` ditambahkan; header dan riwayat revisi diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`LAB-API-v1` naik dari `r3` ke `r4`.** Sepuluh endpoint baca ditambahkan. Amandemennya **aditif sepenuhnya**: tidak satu pun endpoint, ruas, nilai enum, atau pembungkus respons `r3` yang berubah, berganti nama, atau hilang. Tidak ada konsumen lama yang rusak |
| Database | **Tidak ada dampak schema.** Tidak ada entity, kolom, index, maupun migration. Rekap hanya membaca lewat proyeksi agregat; metadata bahkan tidak menyentuh database sama sekali |
| Keamanan/Auth | **Tidak ada hak akses baru.** Kesepuluh endpoint memakai hak baca yang sudah ada pada grupnya masing-masing — `LabOrder : Read`, `LabSpecimen : Read`, `LabValueBound : Read`, `LabCriticalBound : Read`, `LabRejectionReason : Read`. Metadata **mengumumkan** batas kewenangan yang sudah ditegakkan service, tetapi tidak pernah menjadi penegaknya: layar yang mengabaikan penanda itu tetap ditolak `403` oleh service |

### 3.4 Selisih dan keputusan yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | Metadata Lab Order dan Lab Specimen menyatakan **tidak** menyaring di sisi server | `GET /lab-orders` terbukti tidak punya satu pun parameter `[FromQuery]`, dan daftar wadah selalu ber-scope satu pesanan lewat route. Standar melarang metadata menjanjikan penyaring yang tidak diproses daftar, sehingga keduanya mengaku apa adanya lewat `SupportsServerSideFiltering` bernilai salah dan `QueryParameters` kosong. **Menambahkan penyaringan sungguhan pada kedua daftar itu belum berpemilik task** |
| 2 | Daftar pilihan enum tetap disediakan walaupun server belum menyaring | Layar tetap membutuhkannya untuk menerjemahkan angka status menjadi teks, dan untuk menyaring di sisi klien. Yang dilarang adalah menjanjikan pemrosesan server, bukan menyediakan daftar nilainya |
| 3 | Standar master data juga menyebut `/options`, `GET /{id}`, dan `DELETE /{id}` | Ketiganya **tidak** dikerjakan; instruksi pemilik modul menyebut metadata dan summary saja. Selisih terhadap baseline sembilan endpoint tetap terbuka sebagaimana sudah dicatat pada laporan `BE-LAB-06` bagian 3.4 butir 1 |
| 4 | Rekap master data tanpa rentang waktu | Lihat bagian 2.3. Standar menuntut "total, aktif, nonaktif" untuk master data, dan itulah yang diberikan; rentang waktu justru akan menyesatkan pada data induk |
| 5 | `LabCriticalBoundApprovalController.GetFilterMetadata` menerima `valueBoundId` yang tidak dipakainya | Route grup ini bersarang di bawah satu batas nilai, sehingga parameternya wajib ada agar terdokumentasi Swagger dan route-nya cocok. Isinya memang tidak bergantung pada batas nilai mana pun |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status, disiplin, urutan, dan ukuran halaman | `LabOrder : Read` |
| `GET` | `/summary` | Rekap pesanan per status dan per disiplin pada satu rentang waktu | `LabOrder : Read` |

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status wadah, sebab ambil ulang, urutan, dan ukuran halaman | `LabSpecimen : Read` |
| `GET` | `/summary` | Rekap wadah per status dan per sebab ambil ulang pada satu rentang waktu | `LabSpecimen : Read` |

#### Health Services / Laboratory Management / Lab Value Bound

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan bentuk hasil, jenis kelamin, urutan, ukuran halaman, dan penanda bahwa batas kritis hanya berubah lewat pengajuan | `LabValueBound : Read` |
| `GET` | `/summary` | Rekap batas nilai: aktif, nonaktif, per bentuk hasil, dan yang menunggu persetujuan batas kritis | `LabValueBound : Read` |

#### Health Services / Laboratory Management / Lab Critical Bound Approval

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status pengajuan beserta dua penanda keselamatan | `LabCriticalBound : Read` |
| `GET` | `/summary` | Rekap pengajuan untuk **satu** batas nilai, per status | `LabCriticalBound : Read` |

#### Health Services / Laboratory Management / Lab Rejection Reason

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan urutan, ukuran halaman, dan daftar ruas yang terkunci bagi kepala instalasi | `LabRejectionReason : Read` |
| `GET` | `/summary` | Rekap alasan penolakan: aktif, nonaktif, berpenanda kesalahan internal, dan wajib catatan | `LabRejectionReason : Read` |

Kode status yang berlaku untuk seluruhnya: `200` berhasil; `400` rentang tanggal terbalik pada
kedua rekap yang menerimanya; `401` belum login; `403` tanpa hak baca; `404` hanya pada rekap
pengajuan bila batas nilainya tidak ada.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)`, `23 Warning(s)` | `PASS` | Keluaran perintah; tidak ada warning dari berkas baru |
| `dotnet test ...UnitTests.InMemory --filter "FullyQualifiedName~LabFilterAndSummary"` | `Failed: 0, Passed: 31, Total: 31` | `PASS` | Keluaran perintah |
| Seluruh suite `QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 1044, Total: 1045` | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| Kesepuluh endpoint memakai route, verb, dan hak akses yang benar | Seluruhnya cocok | `PASS` | `KesepuluhEndpoint_MemakaiRouteVerbDanHakAksesYangBenar`, sepuluh baris `InlineData` |
| Kesepuluh endpoint baca saja | Tidak ada `POST`, `PUT`, `PATCH`, maupun `DELETE` | `PASS` | `EndpointMetadataDanSummary_SelaluBacaSaja` |
| Metadata memuat **seluruh** nilai enum | Jumlahnya dibandingkan langsung dengan `Enum.GetValues` | `PASS` | Empat uji: pesanan, wadah, batas nilai, pengajuan |
| Label Indonesia benar | `Dibatalkan`, `Patologi Klinik`, `Dinyatakan layak`, `Angka`, `Laki-laki`, `Diajukan` | `PASS` | Uji yang sama |
| Ukuran halaman dan arah urut seragam lintas kelima grup | `10, 25, 50, 100` dan `asc, desc` | `PASS` | `SeluruhMetadata_MemakaiUkuranHalamanDanArahUrutYangSeragam` |
| **Metadata pesanan jujur** — mengaku belum menyaring | Ketiga penanda salah/kosong, dan `GetList` terbukti tanpa `[FromQuery]` | `PASS` | `MetadataPesanan_MengakuBelumMenyaringDiSisiServer` |
| **Metadata batas nilai dan alasan jujur** — mengaku menyaring | Keduanya benar, dan `GetList` keduanya terbukti punya `[FromQuery]` | `PASS` | `MetadataBatasNilaiDanAlasanPenolakan_MengakuMenyaringDiSisiServer` |
| Parameter yang diumumkan benar-benar ada pada penyaingnya | Setiap nama dicocokkan dengan properti DTO penyaring yang sesungguhnya | `PASS` | Dua uji, untuk batas nilai dan alasan penolakan |
| Penanda keselamatan ikut diumumkan | `VAL-28`, `VAL-32`, `VAL-33`, `VAL-37`, dan ketiadaan jalur hapus | `PASS` | `MetadataMembawaPenandaKeselamatanYangSudahDitegakkanService` |
| Rekap alasan penolakan | 4 total, 3 aktif, 1 nonaktif, 1 kesalahan internal, 1 wajib catatan; baris terhapus diabaikan | `PASS` | `RekapAlasanPenolakan_MenghitungAktifNonaktifDanKeduaPenanda` |
| Rekap tabel kosong | Seluruh angka nol, bukan galat | `PASS` | `RekapAlasanPenolakan_TabelKosongMenghasilkanNolBukanGalat` |
| Rekap batas nilai | 3 total, 2 aktif, 2 angka, 1 pilihan, 2 pilihan hasil, 1 menunggu persetujuan, 2 pemeriksaan berbeda | `PASS` | `RekapBatasNilai_MenghitungAktifBentukHasilDanPengajuanTertunda` |
| Rekap pesanan menghormati rentang waktu | Pesanan di luar rentang tidak terhitung; pencacahan per status dan per disiplin benar | `PASS` | `RekapPesanan_MenghitungPerStatusDanPerDisiplinDalamRentangWaktu` |
| Rekap pengajuan ber-scope satu batas nilai | Pengajuan milik batas nilai lain tidak ikut terhitung | `PASS` | `RekapPengajuan_BerScopeSatuBatasNilaiSaja` |
| Rekap pengajuan menolak batas nilai yang tidak ada | `KeyNotFoundException`, dipetakan controller menjadi `404` | `PASS` | `RekapPengajuan_BatasNilaiYangTidakAdaDitolak` |

Uji manual: `NOT FEASIBLE`. Menembak endpoint sungguhan menuntut aplikasi berjalan beserta
databasenya; wewenang eksekusi runtime tidak diminta pada task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Seluruhnya terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi. Tidak ada uji baru yang ditempatkan di sana |
| Suite `QuilvianSystemBackend.UnitTests.Sqlite` | Tidak tersentuh task ini; terakhir dijalankan pada `BE-LAB-09` dengan hasil `176 lulus, 0 gagal` |
| Perintah database apa pun | Task ini tidak menyentuh schema; tidak ada migration yang dibuat maupun dijalankan |

### 5.1 Satu uji yang sempat gagal, dan apa yang diajarkannya

`RekapPengajuan_BerScopeSatuBatasNilaiSaja` gagal pada percobaan pertama dengan
`KeyNotFoundException`. Sebabnya: pemuatan batas nilai pada `LabCriticalBoundApprovalService`
menyertakan navigasi ke `MstProcedure`, sementara data ujinya hanya menaruh `ProcedureId`
karangan tanpa baris jenis pemeriksaan yang sesungguhnya.

Perbaikannya bukan melonggarkan pemeriksaan di service, melainkan **membenahi data ujinya** agar
menyimpan `MstProcedure` sungguhan. Ini kegagalan yang berguna: ia membuktikan bahwa rekap
pengajuan memang memvalidasi keberadaan batas nilai lebih dulu, persis seperti yang dijanjikan
jalur `404`-nya.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

`NOT APPLICABLE`. Task ini lahir dari instruksi langsung pemilik modul pada 2026-09-03, bukan
dari acceptance criteria blueprint. Tidak ada `AC` yang menuntut maupun terdampak olehnya; yang
diperiksa adalah kesesuaian terhadap `master-data-endpoint-standard.md` dan keutuhan kontrak
`r3`.

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Sepuluh endpoint tersedia | **Terpenuhi** | `KesepuluhEndpoint_MemakaiRouteVerbDanHakAksesYangBenar` |
| Kontrak dinaikkan ke `r4` | **Terpenuhi** | `contracts/api-contract.md` revision `4`, sepuluh baris ditambahkan pada kelima grup |
| Seluruh uji lulus | **Terpenuhi** | 31 uji baru lulus; suite penuh menyisakan satu kegagalan Billing yang sudah ada sebelumnya |
| Checker QBE lolos | **Terpenuhi** | `Final result: PASS`, `VIOLATION: 0` |

Tidak ada butir DoD yang belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution menghasilkan 23 warning, seluruhnya sudah ada sebelum task ini |
| Masalah yang diketahui | `GET /lab-orders` dan daftar wadah belum menyaring di sisi server. Metadata keduanya menyatakannya apa adanya, sehingga tidak ada cacat kontrak — tetapi layar daftar pesanan yang besar akan memuat seluruh baris sekaligus. Menambahkan penyaringan dan pagination pada kedua daftar itu **belum berpemilik task** |
| Risiko tersisa | **Rendah.** Seluruh endpoint baca saja, aditif, dan memakai hak akses yang sudah ada. Metadata mengumumkan batas kewenangan tetapi tidak pernah menjadi penegaknya — layar yang mengabaikannya tetap ditolak service |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.2 |
| Langkah berikutnya | 1. Menambahkan penyaringan dan pagination sungguhan pada `GET /lab-orders`. 2. `BE-LAB-16` — endpoint pemeriksaan terpesan. 3. `BE-LAB-10` — penandaan cito per pemeriksaan. 4. `BE-LAB-07` masih menunggu `BE-EXT-01` |

### 7.1 Penghapusan deskripsi endpoint Swagger

Di luar cakupan `BE-LAB-17`, atas permintaan langsung pemilik modul, empat komentar XML pada
method action dihapus karena teksnya tampil sebagai deskripsi pada baris endpoint Swagger:

| Berkas | Endpoint |
| --- | --- |
| `LabOrderController` | `PUT /{id}/start-process`, `PUT /{id}/complete`, `PUT /{id}/cancel` |
| `MedicalRecordAccessLogController` | `GET /pending-review` |

Isinya dipertahankan sebagai komentar biasa `//`, sehingga penjelasan aturan bisnisnya tidak
hilang dari source. Ini melanjutkan pembersihan sebelas blok sejenis pada ketiga controller
Laboratorium lain yang tercatat pada laporan [`BE-LAB-09`](BE-LAB-09.md) bagian 7.3.

**Selisih yang perlu diketahui.** `Program.cs` sekitar baris 642-669 memuat komentar yang
menyatakan pemuatan komentar XML ke Swagger itu **disengaja**, dengan alasan beberapa perubahan
perilaku tidak terlihat dari bentuk permintaan maupun responsnya. Permintaan pemilik modul
berlawanan dengan catatan tersebut. Permintaan pemilik modul yang berlaku, dan catatan pada
`Program.cs` menjadi utang penyelarasan.

### 7.2 Status Git

Branch `yoga`, HEAD `17a331b`. Tidak ada operasi Git yang dijalankan dari sesi ini.

```text
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabCriticalBoundApprovalController.cs
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabRejectionReasonController.cs
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabValueBoundController.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabCriticalBoundApprovalService.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabRejectionReasonService.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabValueBoundService.cs
 M Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordAccessLogController.cs
 M docs/module-blueprints/laboratorium/contracts/api-contract.md
 M docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md
?? Areas/HealthServices/LaboratoryManagement/DTOs/LabFilterAndSummaryDtos.cs
?? Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs
?? tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabFilterAndSummaryTests.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-17.md
```

Berkas `BE-LAB-09` yang belum di-commit — entity `LabExamination`, configurationnya, migration
`20260903071535`, dan berkas uji Laboratorium yang berpindah jalur — tetap ada dan tidak
disentuh task ini.
