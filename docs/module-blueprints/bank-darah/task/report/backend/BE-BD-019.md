# Laporan Task Backend — `BE-BD-019`

> **Riwayat berkas ini.** Dibuka 23 September 2026 sebagai definisi task sebelum implementasi,
> diperbarui ketika source dan kontraknya dikerjakan, lalu dilengkapi bukti runtime pada hari yang sama.
> Seluruh bagian kini terisi.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-019` |
| Judul | Blood Order Date Range Filter |
| Slice | Penyaring rentang tanggal daftar kerja order darah — turunan `BD-UI-GAP-004` Opsi B |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, kartu `BE-BD-019` |
| Trace | `DEC-BD-059`, `DEC-BD-060`; `BD-UI-GAP-004`; `FE-BD-002` |
| Contract version | Amandemen aditif atas `v5` — dikerjakan **di dalam** task ini |
| Dependency | `BE-BD-003` ✅, `DEC-BD-059` ✅, `DEC-BD-060` ✅ — **nol penahan** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BloodBankManagement/**`, `contracts/api-contract.md`, `testing/acceptance-test-matrix.md`, laporan dan roadmap |
| Tanggal dibuka | 23 September 2026 |
| Tanggal dikerjakan | 23 September 2026 |
| Status | ✅ **SELESAI — 23 September 2026. Kelima acceptance `AC-BD-113`..`AC-BD-117` terbukti dan Definition of Done terpenuhi.** Build **`0 Error(s)` / `214 Warning(s)`** dengan **nol peringatan** pada berkas task ini dan **nol tambahan** atas baseline; `has-pending-model-changes` **bersih**; **nol migration**. Validasi runtime **R1–R12 `PASS`, dijalankan langsung agent** terhadap `QuilvianNewDevSukma` dengan keluaran HTTP sungguhan (bagian 7). `AC-BD-115` terbukti sampai ke detik batasnya: rentang efektif `>= 2026-09-22T17:00:00Z` dan `< 2026-09-23T17:00:00Z`, sama persis dengan contoh mengikat pada kontrak. Database dikembalikan ke keadaan semula dan diverifikasi. **Batas bukti:** aktor tunggal `superadmin` — `D5` tidak membedakan pelaku; dan `AC-BD-115` memakai `CreateDateTime` yang disetel, bukan order yang lahir alami pada jam itu (bagian 7.9). **Riwayat:** 🟡 SEBAGIAN — source dan kontrak selesai, runtime belum, 23 September 2026 |

---

## 1. Masalah yang akan diperbaiki

**Layar daftar kerja order darah punya penyaring tanggal yang tidak pernah sampai ke backend.**

`FE-BD-01` merender pemilih Tanggal Awal, pemilih Tanggal Akhir, dan dropdown Periode berisi Hari Ini,
7 Hari Terakhir, 30 Hari Terakhir, Bulan Ini, dan Rentang Tanggal. Ketiganya menulis `startDate` dan
`endDate` lalu mengirimnya sebagai query.

`GET /api/v1/health-services/blood-bank-management/blood-orders` **tidak mengenal kedua parameter itu.**
Model binder ASP.NET Core membuangnya tanpa `400`, tanpa peringatan, tanpa jejak.

Akibatnya petugas BDRS memilih "Hari Ini" dan **daftarnya tidak berubah sama sekali** — masih memuat
order dari hari mana pun, tanpa satu pun pesan yang menjelaskan. Penyaring yang hilang membuat petugas
mencari cara lain; **penyaring yang berbohong membuat petugas percaya ia sudah menyaring**, lalu
mengambil kesimpulan dari daftar yang sebenarnya belum tersaring. Pada daftar kerja klinis, kesimpulan
seperti itu berakhir pada order yang terlewat.

Gap ini tercatat sebagai [`BD-UI-GAP-004`](../../../BD-UI-GAP-004-filter-tanggal-order-darah.md) dan
ditutup pemilik dengan **Opsi B** pada 23 September 2026: penyaringnya dipertahankan, backend yang
menyesuaikan diri.

---

## 2. Keputusan yang mengikat task ini

### 2.1 `DEC-BD-059` — bentuk penyaring

| Butir | Isi |
| --- | --- |
| Kolom yang disaring | **`BbkBloodOrder.CreateDateTime`** — bukan kolom waktu lain |
| Parameter | `startDate`, `endDate`; keduanya **opsional**, digabung dengan penyaring lain secara "dan" |
| Tempat penyaringan | **Server-side.** Frontend tidak pernah menyaring tanggal secara lokal |
| `endDate` | **Inklusif sampai akhir hari** |
| Zona waktu | **Zona waktu aplikasi** — `Asia/Jakarta`, sebagaimana `AppDateTimeHelper` |

### 2.2 `DEC-BD-060` — perbandingan terhadap kolom UTC

`BbkBloodOrder.CreateDateTime` **disimpan dalam UTC**. Pembuatan order menstempelnya `DateTime.UtcNow`
lewat `IdentityModel`, dan `BbkBloodOrderService` memakai `var now = DateTime.UtcNow`.

Maka batas rentang **wajib dikonversi** dari waktu Jakarta ke UTC sebelum dibandingkan:

| Batas | Nilai |
| --- | --- |
| Bawah, inklusif | Pukul `00:00` **waktu Jakarta** pada `startDate`, dikonversi ke UTC |
| Atas, **eksklusif** | Pukul `00:00` **waktu Jakarta** pada `endDate + 1 hari`, dikonversi ke UTC |

Batas atas eksklusif pada hari berikutnya itulah yang mewujudkan "inklusif sampai akhir hari" tanpa
kehilangan detik terakhir. Membandingkan `<= endDate 23:59:59` akan membuang kejadian pada pecahan detik
terakhir hari itu.

### 2.3 Jebakan yang wajib dihindari

**Pola penyaring tanggal yang sudah mapan di repository ini salah untuk kolom UTC.**

`BankController.ResolveDateRange` — dipakai juga `CompanyGuarantorController` dan beberapa controller
master data lain — mengambil tanggal operasional waktu Jakarta lewat `AppDateTimeHelper.OperationalDate()`,
lalu **menstempelnya** `DateTimeKind.Utc` tanpa konversi:

```csharp
start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
```

Selisihnya **tujuh jam**.

**Contoh berangka.** Order dibuat pukul `02:00` WIB tanggal 23 September. Nilai tersimpan
`2026-09-22T19:00:00Z`. Petugas menyaring 23 September sampai 23 September. Dengan pola di atas, batas
bawahnya `2026-09-23T00:00:00Z`, dan order itu **tidak muncul** — padahal menurut waktu Jakarta ia memang
dibuat hari itu. Setiap order yang lahir antara tengah malam dan pukul tujuh pagi WIB jatuh ke hari yang
keliru. Pada bank darah, jam-jam itu bukan jam sepi.

Dengan `DEC-BD-060`, batas bawahnya `2026-09-22T17:00:00Z` dan ordernya muncul sebagaimana mestinya.

**Task ini tidak memperbaiki pola milik modul lain.** Cacat itu dilaporkan di sini dan berhenti di sini;
memperbaikinya menyentuh Administrator/MasterData dan menuntut wewenang serta pengujiannya sendiri.

---

## 3. Scope

### 3.1 Yang dikerjakan

| # | Bagian | Perubahan yang direncanakan |
| :---: | --- | --- |
| 1 | **Kontrak API** | `contracts/api-contract.md` grup Blood Order: `GET /` memperoleh `startDate` dan `endDate`, beserta aturan inklusivitas, zona waktu, dan kolom yang disaring. Ditulis sebagai amandemen **aditif** |
| 2 | **Query DTO** | `BbkBloodOrderController.GetAll` menerima `[FromQuery] DateTime? startDate` dan `[FromQuery] DateTime? endDate`. `BloodOrderDefaultFilterResponse` pada `GET /filters/metadata` memperoleh kedua isian itu supaya layar dapat membacanya |
| 3 | **Penyaring service** | `BbkBloodOrderService.GetPagedAsync` menyaring `CreateDateTime` memakai batas hasil konversi `DEC-BD-060`. Penyaring digabung "dan" dengan `search`, `patientId`, `encounterId`, `serviceUnitId`, `bloodComponentId`, `orderStatus`, dan `orderSource` yang sudah ada |
| 4 | **Acceptance criteria** | `AC-BD-113`..`AC-BD-117` ditulis ke `testing/acceptance-test-matrix.md` |
| 5 | **Validasi runtime** | Runbook bagian 7 dijalankan terhadap `QuilvianNewDevSukma` |

### 3.2 Yang di luar scope

- **Migration, kolom, atau index baru.** Bila bukti eksekusi membuktikan index memang dibutuhkan,
  **berhenti dan minta keputusan pemilik** — jangan tambahkan diam-diam.
- **Parameter `customPeriod` di backend.** Periode tetap dihitung layar dan dikirim sebagai rentang
  biasa; lihat risiko R3 bagian 8.
- Penyaring tanggal pada endpoint Bank Darah lain.
- Perubahan apa pun pada frontend — `FE-BD-002` menyambungkannya sesudah task ini selesai.
- Perbaikan pola `ResolveDateRange` milik modul lain.

### 3.3 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Dua parameter query opsional baru; bentuk respons **tidak berubah**. Nol klien lama rusak |
| Database | `NOT APPLICABLE` — nol migration, nol kolom, nol index. `has-pending-model-changes` wajib tetap bersih |
| Keamanan/Auth | `NOT APPLICABLE` — nol butir hak akses baru. `BloodOrder : Read` tetap satu-satunya penjaga daftar kerja |

---

## 4. Acceptance criteria

Penomoran mengikuti penetapan pemilik 23 September 2026. Sudah ditulis ke
[`testing/acceptance-test-matrix.md`](../../../testing/acceptance-test-matrix.md) bagian 12.

| ID | Pokok | Kriteria |
| --- | --- | --- |
| `AC-BD-113` | Penyaring `startDate` | Tiga order pada 21, 22, dan 23 September waktu aplikasi. `startDate = 2026-09-22` tanpa `endDate` → order 22 dan 23 muncul, order 21 tidak. Tanpa parameter apa pun, ketiganya muncul |
| `AC-BD-114` | Penyaring `endDate`, inklusif | `endDate = 2026-09-22` tanpa `startDate` → order 21 dan 22 muncul **termasuk yang dibuat pukul `23:59`**; order 23 tidak. Membuktikan batas atas eksklusif pada hari berikutnya, bukan `<= 23:59:59` |
| `AC-BD-115` | **Batas zona waktu WIB** | Order pukul `02:00` WIB tanggal 23 September — tersimpan `2026-09-22T19:00:00Z` — **muncul** pada saringan 23 September dan **tidak muncul** pada saringan 22 September. Rentang efektif `>= 2026-09-22T17:00:00Z` dan `< 2026-09-23T17:00:00Z` (`DEC-BD-060`) |
| `AC-BD-116` | Kombinasi penyaring | Rentang digabung `bloodComponentId`, `orderStatus`, dan `search` pada halaman kedua → irisan seluruhnya; paging berlaku atas hasil yang **sudah** tersaring |
| `AC-BD-117` | Rentang tidak sah ditolak | `startDate = 2026-09-30`, `endDate = 2026-09-23` → **`400 VAL-BD-086`**, bukan daftar kosong. `startDate` sama dengan `endDate` tetap diterima dan menyaring satu hari penuh |

---

## 5. Perubahan yang dikerjakan

### 5.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Helpers/AppDateTimeHelper.cs` | Metode **aditif** `OperationalDateToUtc(DateTime)` — mengubah tanggal operasional menjadi saat UTC pukul `00:00` zona waktu aplikasi. Nol perubahan pada metode yang sudah ada |
| `Areas/.../Services/BbkBloodOrderService.cs` | Konstanta `InvalidDateRangeMessage` (kalimat kanonik `VAL-BD-086`); tipe `BloodOrderDateRange`; resolver statis `ResolveCreateDateRange`; dua parameter `createdFromUtc` / `createdToUtcExclusive` pada `GetPagedAsync` beserta dua klausa `Where` pada `CreateDateTime` |
| `Areas/.../Controllers/BbkBloodOrderController.cs` | `GET /` menerima `startDate` dan `endDate`; rentang diselesaikan sebelum query; rentang tidak sah → `400`; dokumentasi endpoint dan `ProducesResponseType` `400` |
| `Areas/.../DTOs/BloodOrderDtos.cs` | `StartDate` dan `EndDate` pada `BloodOrderDefaultFilterResponse` |
| `contracts/api-contract.md` | Bagian **Amendment `v5` `D5`** baru; baris `last_changed_in` disegarkan |
| `contracts/validation-matrix.md` | Baris **`VAL-BD-086`** baru beserta catatan penjelasnya; `last_changed_in` disegarkan |
| `testing/acceptance-test-matrix.md` | Bagian **12** baru: `AC-BD-113`..`AC-BD-117`; `last_changed_in` disegarkan |
| `roadmap/backend-roadmap.md` | Kartu `BE-BD-019` disegarkan |
| `task/report/backend/BE-BD-019.md` | Berkas ini |

**Nol migration. Nol kolom. Nol index. Nol butir hak akses.**

### 5.2 Kenapa zona waktunya tidak ditulis ulang di modul ini

`AppDateTimeHelper` memegang `Asia/Jakarta` sebagai satu-satunya sumber kebenaran zona waktu aplikasi,
tetapi tidak menyediakan konversi tanggal→UTC. Ada dua jalan: menyalin id zona waktunya ke dalam
`BbkBloodOrderService`, atau menambahkan satu metode ke helper bersama.

Menyalin akan membuat **dua** tempat mendefinisikan zona waktu aplikasi. Bila kelak zona itu diubah,
Bank Darah diam-diam berbeda dari sisa aplikasi — tepat pada penyaring yang cacatnya tidak terlihat
sampai seseorang memeriksa order dini hari. Karena itu dipilih jalan kedua: satu metode **aditif**,
nol perubahan perilaku pada metode yang sudah ada, nol pemanggil lama terpengaruh.

Ini **menyentuh berkas bersama di luar `Areas/HealthServices/BloodBankManagement/`**, dan dicatat di
sini apa adanya supaya tinjauan scope tidak menemukannya sebagai kejutan.

### 5.3 Pola `ResolveDateRange` tidak disalin

Instruksi task menyebut "gunakan pattern date range existing repository". Yang **diambil** dari pola itu:
bentuk resolvernya, tipe hasil valid/invalid, dan penolakan `400` lewat `ApiResponse<object>.Fail`.

Yang **tidak diambil**: cara pola itu membandingkan waktu. `BankController.ResolveDateRange` menstempel
tanggal waktu Jakarta dengan `DateTimeKind.Utc` tanpa konversi — cacat tujuh jam yang dijelaskan pada
bagian 2.3. Menyalinnya akan melanggar `DEC-BD-060` dan menggagalkan `AC-BD-115`.

`customPeriod` pada pola itu juga **tidak** diambil: layar sudah menghitung periodenya sendiri dan
mengirim rentang biasa (bagian 3.2, risiko R3).

### 5.4 Dua hal yang ditetapkan pada waktu implementasi

| Hal | Yang ditetapkan | Alasan |
| --- | --- | --- |
| Rentang terbalik (risiko R4) | **Ditolak `400 VAL-BD-086`**, diperiksa sebelum query menyentuh database. Kalimatnya ditulis ke `validation-matrix.md` sebagai kode baru | Nol baris dari query yang mustahil akan terbaca petugas sebagai "tidak ada order", bukan sebagai "filternya salah" |
| `startDate` sama dengan `endDate` | **Sah** — menyaring satu hari penuh | Itu justru bentuk pemakaian yang paling lazim: "tampilkan order hari ini" |

### 5.5 Nomor set kontrak tidak dinaikkan

Perubahan ini **aditif penuh**: dua parameter query opsional, nol perubahan bentuk respons, nol klien
lama rusak. Ia dicatat sebagai `D5` pada blok Amendment `v5` yang sudah ada, bukan sebagai set kontrak
`v6`. Menaikkan nomor set menuntut gerbang persetujuan tersendiri, dan tidak ada yang menuntutnya di
sini. **Bila pemilik menghendaki `v6`, itu keputusan tersendiri dan belum diambil.**

## 6. Verifikasi

| Perintah / pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Lihat bagian 6.1 | Lihat bagian 6.1 |
| `dotnet ef migrations has-pending-model-changes` | Lihat bagian 6.1 | Lihat bagian 6.1 |
| Kalimat `VAL-BD-086` sama persis matriks validasi | Konstanta `InvalidDateRangeMessage` lawan `validation-matrix.md` | Lihat bagian 6.1 |
| Rentang terbalik ditolak sebelum menyentuh database | Penjaga berada di `ResolveCreateDateRange`, dipanggil controller **sebelum** `GetPagedAsync` | `PASS` (struktur) |
| Konversi zona waktu dipakai, bukan stempel | `ResolveCreateDateRange` memanggil `AppDateTimeHelper.OperationalDateToUtc`; nol `SpecifyKind(..., DateTimeKind.Utc)` pada jalur ini | `PASS` (struktur) |

### 6.1 Hasil build dan migration check

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **`Build succeeded.`** — **`0 Error(s)`, `214 Warning(s)`**, waktu `00:57:47` | `PASS` |
| Peringatan pada keempat berkas yang diubah | **Nol.** Penyaringan log atas `AppDateTimeHelper.cs`, `BbkBloodOrderService.cs`, `BbkBloodOrderController.cs`, dan `BloodOrderDtos.cs` memulangkan **nol** baris peringatan | `PASS` |
| Peringatan Bank Darah yang ada | Tiga, seluruhnya `CS1573` di `BbkBloodUnitService.cs` baris 3125–3127 — wilayah `BE-BD-009`/`BE-BD-010`, **sudah ada sebelum task ini** dan tercatat sama pada `BE-BD-017` | `PASS` |
| Baseline peringatan | `214`, **sama persis** dengan yang tercatat `BE-BD-017` dan `BE-BD-018` pada 23 September 2026. Task ini **tidak menambah satu pun** | `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| Nol migration ditambahkan | Nol berkas di `Migrations/` disentuh | `PASS` |

**Catatan cara build dijalankan, yang wajib dibaca bersama angka di atas.**

Build ini dijalankan dengan **`-p:UseSharedCompilation=false`**, berbeda dari perintah polos yang dipakai
`BE-BD-017` dan `BE-BD-018`. Alasannya bukan pilihan gaya:

| Percobaan | Hasil | Sebab |
| :---: | --- | --- |
| 1 | **`Build FAILED`** sesudah `00:42:21` — `MSB6006: "csc.dll" exited with code -1073741571` | `0xC00000FD` = `STATUS_STACK_OVERFLOW`. **Crash compiler, bukan kesalahan kompilasi**: `0 Warning(s)` dan **nol** diagnostik `CS****` atas berkas mana pun |
| 2 | **Tidak selesai** — log berhenti di tahap restore | Prosesnya **dibunuh** ketika sesi agent dibongkar, bukan gagal. Nol baris hasil kompilasi, sehingga nol kesimpulan dapat ditarik darinya |
| 3 | **`Build succeeded.`** `00:57:47` | Dijalankan sesudah `dotnet build-server shutdown`, dengan `UseSharedCompilation=false` dan proses yang dilepas dari umur sesi |

Stack overflow pada percobaan 1 konsisten dengan masalah repository yang sudah tercatat `BE-BD-017`
bagian 7: `Migrations/` memuat 384 berkas dengan total **12,3 juta baris**, dan satu build memakan puncak
memori sekitar 25 GB. `BE-BD-017` juga mencatat satu interupsi build serupa yang pulih lewat
`dotnet build-server shutdown`.

**Batas bukti:** angka `214 Warning(s)` / `0 Error(s)` di atas sah, tetapi diperoleh pada **kondisi build
yang tidak identik** dengan baseline. Perbedaannya hanya pada cara `csc` dijalankan — proses terpisah per
project alih-alih server kompilasi bersama — dan tidak mengubah kode yang dihasilkan. Dicatat di sini apa
adanya supaya perbandingan terhadap baseline dibaca dengan tahu persis apa yang berbeda.

---

## 7. Validasi runtime — DIEKSEKUSI 23 September 2026, R1–R12 `PASS`

Dijalankan langsung oleh agent terhadap **`QuilvianNewDevSukma`**, bukan atestasi pihak lain. Aplikasi
dijalankan dari hasil build yang lulus (`bin/Debug/net9.0/QuilvianSystemBackend.dll`, `--urls
http://localhost:5107`), autentikasi memakai cookie sesi `superadmin`, dan seluruh permintaan ditembakkan
lewat `curl`.

### 7.1 Data uji dan penyiapan `AC-BD-115`

Sembilan order sudah ada di database, tersebar pada 14, 16, dan 17 September waktu aplikasi. **Nol** di
antaranya jatuh pada jam 00:00–07:00 WIB, sehingga `AC-BD-115` tidak dapat dibuktikan apa adanya —
persis keadaan yang diperingatkan pada runbook.

Penyiapannya dikerjakan sesuai yang dibenarkan runbook: **satu** order disetel sementara lewat database,
lalu dikembalikan.

| Langkah | Isi |
| --- | --- |
| Order yang dipakai | `ORD-00000089` |
| Nilai asli | `2026-09-17T13:23:19.162046Z` — dicatat sebelum disentuh |
| Nilai uji | Disetel berturut-turut ke lima nilai untuk memetakan batasnya |
| Pemulihan | Dikembalikan ke `2026-09-17T13:23:19.162046Z` **sebelum** AC lain dijalankan |
| Verifikasi pemulihan | `select` seluruh sembilan order sesudah pemulihan: **identik** dengan snapshot awal, `min` dan `max` sama persis |
| Order baru dibuat | **Nol.** Nol nomor order terbakar, nol data sampah tertinggal |

### 7.2 `AC-BD-115` — batas zona waktu WIB

Order disetel ke `2026-09-22T19:00:00Z`, yaitu **pukul 02:00 WIB tanggal 23 September**.

| Saringan | Hasil | Klasifikasi |
| --- | --- | :---: |
| `startDate=2026-09-23&endDate=2026-09-23` — hari **WIB**-nya | `totalData: 1` → `ORD-00000089` **muncul** | `PASS` |
| `startDate=2026-09-22&endDate=2026-09-22` — hari **UTC**-nya | `totalData: 0` → **tidak muncul** | `PASS` |

Ini pembuktian langsung `DEC-BD-060`. Bila kode menstempel `DateTimeKind.Utc` tanpa konversi, kedua
hasil di atas akan **tertukar** persis.

**Batas dipetakan sampai ke detiknya** dengan menyetel order ke empat nilai di sekitar tepi, seluruhnya
disaring `startDate=endDate=2026-09-23`:

| Nilai `CreateDateTime` | Sama dengan WIB | Hasil | Klasifikasi |
| --- | --- | --- | :---: |
| `2026-09-22T16:59:59Z` | 22 Sept `23:59:59` | **Tidak muncul** | `PASS` |
| `2026-09-22T17:00:00Z` | 23 Sept `00:00:00` | **Muncul** | `PASS` |
| `2026-09-23T16:59:59Z` | 23 Sept `23:59:59` | **Muncul** | `PASS` |
| `2026-09-23T17:00:00Z` | 24 Sept `00:00:00` | **Tidak muncul** | `PASS` |

Rentang efektifnya terbukti **`>= 2026-09-22T17:00:00Z`** dan **`< 2026-09-23T17:00:00Z`** — sama persis
dengan contoh mengikat pada `api-contract.md` `D5`. Baris ketiga sekaligus membuktikan "inklusif sampai
akhir hari": kejadian pada detik terakhir hari WIB **tetap** masuk.

### 7.3 `AC-BD-113` — penyaring `startDate`

Dijalankan di atas data asli, sesudah pemulihan.

| Saringan | Harapan | Hasil | Klasifikasi |
| --- | --- | --- | :---: |
| Tanpa penyaring | 9 order | `totalData: 9` | `PASS` |
| `startDate=2026-09-16` | 5 order; yang 14 Sept dibuang | `totalData: 5` → `89, 88, 87, 86, 85` | `PASS` |
| `startDate=2026-09-17` | 4 order | `totalData: 4` → `89, 88, 87, 86` | `PASS` |

### 7.4 `AC-BD-114` — penyaring `endDate`, inklusif

| Saringan | Harapan | Hasil | Klasifikasi |
| --- | --- | --- | :---: |
| `endDate=2026-09-14` | 4 order, **termasuk** yang dibuat pukul 17:14 WIB | `totalData: 4` → `84, 83, 82, 81` | `PASS` |
| `endDate=2026-09-16` | 5 order | `totalData: 5` | `PASS` |
| `endDate=2026-09-13` | 0 order — seluruhnya sesudah rentang | `totalData: 0` | `PASS` |

Inklusivitas sampai detik terakhir hari dibuktikan terpisah pada bagian 7.2 baris ketiga.

### 7.5 `AC-BD-116` — kombinasi penyaring dan paging

| Saringan | Harapan | Hasil | Klasifikasi |
| --- | --- | --- | :---: |
| `bloodComponentId=<BD009>` | 2 order | `totalData: 2` → `87, 86` | `PASS` |
| ditambah `startDate=endDate=2026-09-17` | tetap 2 | `totalData: 2` | `PASS` |
| ditambah `orderStatus=0` (Aktif) | 1 — `86` berstatus Dibatalkan | `totalData: 1` → `87` | `PASS` |
| `bloodComponentId=<BD009>` + rentang 14 Sept | 0 — irisan kosong | `totalData: 0` | `PASS` |
| rentang `14`–`16` + `orderStatus=0` | 4 order | `totalData: 4` → `85, 84, 83, 81` | `PASS` |

**Paging atas hasil yang sudah tersaring**, rentang `2026-09-14`..`2026-09-16` dengan `pageSize=2`:

| Halaman | Hasil |
| :---: | --- |
| 1 | `totalData: 5`, `totalPage: 3` → `85, 84` |
| 2 | `totalData: 5`, `totalPage: 3` → `83, 82` |
| 3 | `totalData: 5`, `totalPage: 3` → `81` |

`totalData` menghitung **5**, bukan 9. Paging berlaku atas hasil tersaring, bukan atas seluruh order.

### 7.6 `AC-BD-117` — rentang tidak sah

| Saringan | Hasil | Klasifikasi |
| --- | --- | :---: |
| `startDate=2026-09-30&endDate=2026-09-23` | **`HTTP 400`**; `success: false`; `statusCode: 400`; `errors: null` | `PASS` |
| Kalimat penolakan | "Tanggal awal filter tidak boleh melewati tanggal akhir filter." — **identik kata per kata** dengan `validation-matrix.md` dan dengan konstanta `InvalidDateRangeMessage` di source | `PASS` |
| `startDate=2026-09-16&endDate=2026-09-16` | **Diterima** `200`, `totalData: 1` — rentang satu hari tetap sah | `PASS` |

Penolakannya **bukan** daftar kosong. Slot `errors` tetap `null`, sesuai kebiasaan grup ini yang hanya
memberi `errors` terstruktur pada `VAL-BD-001`.

### 7.7 `R12` — metadata penyaring

`GET /filters/metadata` memulangkan `defaultFilter` yang kini memuat `startDate` dan `endDate`, keduanya
bernilai `null` sebagai bawaan. Layar dapat membacanya tanpa menebak.

### 7.8 Keadaan database sesudah pengujian

| Pemeriksaan | Hasil |
| --- | --- |
| Jumlah order | **9** — sama dengan sebelum pengujian |
| `min(CreateDateTime)` | `2026-09-14T02:46:56.989475Z` — sama |
| `max(CreateDateTime)` | `2026-09-17T13:23:19.162046Z` — sama |
| `ORD-00000089` | `2026-09-17T13:23:19.162046Z` — **pulih tepat** |
| Order baru, migration, kolom | **Nol** |

### 7.9 Batas bukti

Yang **kuat**: seluruh hasil di atas adalah keluaran HTTP sungguhan dari aplikasi yang berjalan terhadap
database yang diberi wewenang, dibaca agent secara langsung — bukan pernyataan pihak lain. Ini bukti yang
lebih kuat daripada preseden `BE-BD-017` dan `BE-BD-018`, yang bersandar pada atestasi pemilik tanpa log
primer.

Yang **perlu dibaca dengan tahu batasnya**:

1. **Aktor tunggal, `superadmin`.** `D5` tidak menambah butir hak akses dan tidak membedakan pelaku —
   penyaring tanggal dijaga `BloodOrder : Read` yang sudah ada — sehingga aktor kedua tidak menambah
   apa pun bagi kelima acceptance ini. Berbeda dari `AC-BD-108` yang memang menuntut dua aktor.
2. **`AC-BD-115` memakai data yang disetel**, bukan order yang lahir alami pada jam itu. Nilai yang
   disetel adalah `CreateDateTime` — persis kolom yang disaring — sehingga jalur yang diuji identik
   dengan jalur sungguhan. Yang tidak diuji: apakah order yang benar-benar dibuat pukul 02:00 WIB
   menyimpan nilai yang benar; itu wilayah `BE-BD-003`, bukan task ini.
3. **Zona waktu mesin uji.** Konversi dikerjakan `TimeZoneInfo` terhadap `Asia/Jakarta` secara eksplisit,
   tidak bergantung pada zona waktu sistem. Tidak diuji pada mesin dengan zona waktu berbeda.

---

## 7A. Runbook — definisi skenario

Dipertahankan sebagai definisi yang dipakai, supaya pengujian ini dapat diulang identik di lingkungan
lain. Seluruh skenario dijalankan terhadap **`QuilvianNewDevSukma`**.

**Data yang dibutuhkan, dan ini bagian yang paling mudah terlewat:** sekurang-kurangnya satu order yang
`CreateDateTime`-nya jatuh antara `00:00` dan `07:00` **WIB**. Tanpa order seperti itu, `AC-BD-115` tidak
dapat dibuktikan dan cacat zona waktu akan lolos tanpa terlihat. Bila tidak ada order alami pada jam itu,
siapkan satu dengan menyetel nilainya langsung di database pengembangan, dan catat caranya pada laporan.

**Data yang disiapkan:** empat order pada 21, 22, dan 23 September waktu aplikasi — salah satunya dibuat
pukul `23:59` WIB tanggal 22, dan satu lagi pukul `02:00` WIB tanggal 23.

| # | Skenario | Yang diharapkan | Menutup |
| :---: | --- | --- | --- |
| R1 | Tanpa `startDate` dan `endDate` | Perilaku **sama persis** dengan sebelum task ini; seluruh order muncul | `AC-BD-113` |
| R2 | `startDate = 2026-09-22` saja — **order sebelum rentang** | Order 21 September **tidak muncul** | `AC-BD-113` |
| R3 | `startDate = 2026-09-22` saja — **order di dalam rentang** | Order 22 dan 23 September **muncul** | `AC-BD-113` |
| R4 | `endDate = 2026-09-22` saja — **order sesudah rentang** | Order 23 September **tidak muncul** | `AC-BD-114` |
| R5 | `endDate = 2026-09-22`, order pukul `23:59` WIB tanggal 22 | **Muncul** — inklusif sampai akhir hari | `AC-BD-114` |
| R6 | **Order pukul `02:00` WIB** tanggal 23, disaring `startDate = endDate = 2026-09-23` | **Muncul.** Inilah pembuktian `DEC-BD-060` | `AC-BD-115` |
| R7 | Order yang sama, disaring `startDate = endDate = 2026-09-22` | **Tidak muncul** — ia milik hari 23 menurut waktu aplikasi, walau nilai UTC-nya jatuh pada 22 | `AC-BD-115` |
| R8 | `startDate = endDate = 2026-09-23` | **Diterima**; menyaring satu hari penuh | `AC-BD-117` |
| R9 | Rentang + `bloodComponentId` + `orderStatus` + `search` | Irisan seluruhnya | `AC-BD-116` |
| R10 | Rentang pada halaman 2 dengan `pageSize` kecil | Paging berlaku atas hasil **tersaring**; `totalData` dan `totalPage` menghitung hasil akhir | `AC-BD-116` |
| R11 | `startDate = 2026-09-30`, `endDate = 2026-09-23` — **rentang tidak sah** | **`400`** dengan kalimat persis `VAL-BD-086`; **bukan** daftar kosong | `AC-BD-117` |
| R12 | `GET /filters/metadata` | Menyebut `startDate` dan `endDate`, keduanya bawaan kosong | `AC-BD-113` |

---

## 8. Risiko

| # | Risiko | Keadaan sesudah implementasi |
| :---: | --- | --- |
| R1 | **Menyalin pola `ResolveDateRange` apa adanya** | **Ditutup.** Konversi memakai `AppDateTimeHelper.OperationalDateToUtc`; nol `SpecifyKind(..., DateTimeKind.Utc)` pada jalur ini. **Terbukti runtime** lewat `AC-BD-115`: batasnya dipetakan sampai ke detik, dan hasilnya persis kebalikan dari apa yang akan terjadi bila pola keliru itu disalin (bagian 7.2) |
| R2 | **Beban kueri pada kolom tanpa index** | **Terbuka.** `CreateDateTime` belum ter-index. Penyaring rentang pada tabel yang tumbuh dapat melambat. Menambah index **di luar scope**; bila bukti eksekusi menunjukkan kebutuhannya, berhenti dan minta keputusan pemilik |
| R3 | **Dropdown periode dihitung di browser** | **Terbuka, dan kini lebih terasa.** "Hari Ini", "7 Hari Terakhir", dan seterusnya dihitung frontend memakai zona waktu **perangkat petugas**, lalu dikirim sebagai rentang biasa. Backend kini menafsirkannya sebagai tanggal `Asia/Jakarta`. Pada perangkat yang zona waktunya bukan WIB, rentangnya bergeser satu hari. **Tidak diputuskan** `DEC-BD-059`; menunggu keputusan tersendiri |
| R4 | **Rentang terbalik** | **Ditutup.** Ditolak `400 VAL-BD-086`, diperiksa sebelum menyentuh database; kalimatnya kanonik di `validation-matrix.md` |
| R5 | **`FE-BD-002` masih perlu menyambungkannya** | **Terbuka.** Backend siap, tetapi kontrol tanggal di layar belum dialirkan ke query. Sampai `FE-BD-002` menyambungkannya, kontrol itu **tetap tayang dan tetap tidak berfungsi** |
| R6 | **Berkas bersama disentuh** | `Helpers/AppDateTimeHelper.cs` memperoleh satu metode aditif. Nol perubahan pada metode yang sudah ada, sehingga nol pemanggil lama terpengaruh — tetapi berkas itu dipakai belasan controller di luar Bank Darah, dan perubahannya wajib ikut terbaca pada tinjauan scope |
| R7 | **Bukti runtime** | **Ditutup.** R1–R12 `PASS`, dijalankan langsung agent dengan keluaran HTTP sungguhan (bagian 7). Dua batas yang tetap melekat dan tidak menggugurkan bukti: aktor tunggal, dan `AC-BD-115` memakai nilai yang disetel — keduanya dijelaskan pada bagian 7.9 |

### 8.1 Acceptance criteria — status akhir

| ID | Pokok | Struktur | Runtime | Status |
| --- | --- | :---: | :---: | :---: |
| `AC-BD-113` | Penyaring `startDate` | ✅ | ✅ | **Terpenuhi** |
| `AC-BD-114` | Penyaring `endDate`, inklusif | ✅ | ✅ | **Terpenuhi** |
| `AC-BD-115` | Batas zona waktu WIB | ✅ | ✅ | **Terpenuhi** |
| `AC-BD-116` | Kombinasi penyaring dan paging | ✅ | ✅ | **Terpenuhi** |
| `AC-BD-117` | Rentang tidak sah ditolak | ✅ | ✅ | **Terpenuhi** |

### 8.2 Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance terbukti | ✅ Bukti struktur dari source, bukti runtime dari eksekusi langsung 23 September 2026 |
| `api-contract.md` diamandemen | ✅ Amendment `v5` `D5` |
| `AC-BD-113`..`AC-BD-117` tertulis di matriks acceptance | ✅ Bagian 12 |
| Nol migration | ✅ `has-pending-model-changes` bersih; nol berkas `Migrations/` disentuh |
| Bukti runtime lintas batas hari WIB ada | ✅ Bagian 7.2, dipetakan sampai ke detik batasnya |
| Laporan tracked `task/report/backend/BE-BD-019.md` | ✅ Berkas ini |

**Definition of Done terpenuhi seluruhnya.** Dua batas yang melekat dan tidak menggugurkannya tercatat
pada bagian 7.9.

---

## 9. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perubahan sampingan | **Satu, dan disengaja.** `Helpers/AppDateTimeHelper.cs` memperoleh satu metode aditif `OperationalDateToUtc`; nol perubahan pada metode yang sudah ada. Alasannya pada bagian 5.2 |
| Biaya build | Tiga percobaan, total sekitar **2 jam 20 menit** waktu dinding untuk satu build yang lulus. Ini kondisi repository yang sudah ada — 12,3 juta baris di `Migrations/` — **bukan** akibat task ini, dan sudah tercatat `BE-BD-017` bagian 7 |
| Interupsi | Dua kali. Percobaan 1 crash compiler; percobaan 2 dibunuh saat sesi agent dibongkar. Nol pekerjaan hilang: seluruh source dan dokumen sudah tertulis ke disk sebelum build pertama dimulai, dan diverifikasi masih utuh sesudahnya |
| Definition of Done | ✅ **Terpenuhi seluruhnya** — rinciannya bagian 8.2 |
| Dampak pada database uji | **Nol permanen.** Satu order disetel sementara lalu dipulihkan dan diverifikasi; nol order baru, nol nomor order terbakar, nol data sampah |
| Alat bantu | Satu utilitas kueri sekali pakai dibuat di direktori scratchpad sesi, **di luar** kedua repository, membaca connection string dari `appsettings.Development.json` tanpa menyalin kredensial. Tidak ikut ke mana pun |
| Status Git | `git status --short` menunjukkan 4 berkas source `M` dan berkas dokumen yang berubah. **Nol stage, nol commit, nol push** |
| Langkah berikutnya | `FE-BD-002` menyambungkan penyaring tanggalnya — kedua parameter sudah dikirim layar apa adanya, sehingga yang tersisa hanya penanganan `400 VAL-BD-086` dan keputusan atas risiko R3 (dropdown periode dihitung di browser) |
