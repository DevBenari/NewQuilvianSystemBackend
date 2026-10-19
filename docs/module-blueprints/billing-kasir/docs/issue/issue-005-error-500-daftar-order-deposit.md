# ISSUE-BKC-005 — Daftar dan detail Order Deposit Rawat Inap gagal dengan 500 setiap ada baris data

```yaml
issue_id: ISSUE-BKC-005
module_id: billing-kasir
submodule: null  # blueprint berbentuk SINGLE; <blueprint-root> = folder modul
layar: "Billing dan Kasir > Kasir > Deposit Rawat Inap — tab \"Order Pembayaran Deposit Pasien\" (inpatient-deposit-orders-table.jsx, endpoint GET /api/v1/health-services/billing-management/billing/deposit-orders). Radius dampak juga mencakup modal Detail Order dan proses Buat Order Deposit manual pada layar yang sama (lihat bagian 6 temuan tambahan)."
sumber_laporan: "Laporan pengguna 16-10-2026 dalam dua tahap: (1) screenshot query SQL langsung ke tabel BilDepositPaymentOrder menunjukkan data UNPAID sudah ada, disertai pertanyaan kenapa tabel di layar tidak menampilkan data; (2) screenshot DevTools menunjukkan request GET ke endpoint yang sama mengembalikan HTTP 500 dengan body {\"success\":false,\"statusCode\":500,\"message\":\"Terjadi kesalahan pada server.\"}. 2 butir laporan, ditambah 3 temuan tambahan dari penelusuran root cause yang sama."
tanggal_issue: "2026-10-16"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "738e03a0 (Yasmina)"
source_sha_frontend: "8ad45262b (yasmina) — dicatat untuk pelengkap; frontend terbukti tidak mengandung bug (lihat bagian 4)"
rencana_perbaikan: ../plan-repair/plan-repair-005-error-500-daftar-order-deposit.md
```

## 1. Ringkasan

Layar "Deposit Rawat Inap" tab "Order Pembayaran Deposit Pasien" gagal memuat data setiap kali
ada **minimal satu** baris order deposit di database — bukan masalah tampilan atau filter, tapi
server selalu merespons 500 Internal Server Error begitu query menemukan baris yang cocok. Akar
masalahnya murni di backend: satu baris kode yang salah mengonversi tanggal dibuat (`CreateDateTime`)
menjadi format tampilan "jam Indonesia (WIB)", dan cara penulisannya justru melanggar aturan
bawaan .NET sehingga melempar error setiap dieksekusi. Pola kode yang sama persis, dengan
kesalahan yang sama persis, ternyata ditulis ulang di tiga tempat lain pada file yang sama —
termasuk di jalur "Buat Order Deposit" dan "Lihat Detail Order", yang berarti fitur-fitur itu pun
akan gagal total begitu dipakai, walau belum pernah dilaporkan pengguna. Frontend sudah bekerja
benar — ia sudah menampilkan pesan error ke layar persis sesuai pola base table yang dipakai
layar lain; tidak ada yang perlu diubah di sisi frontend.

## 2. Laporan asli

| No. laporan | Keluhan | Lampiran |
| ---: | --- | --- |
| 1 | "Pada halaman Deposit rawat inap di menu billing-kasir. Mengapa table belum ambil data dari table?" — disertai screenshot hasil SELECT langsung ke tabel `BilDepositPaymentOrder` yang menunjukkan baris `DPO-20261016-00001`, status `UNPAID`, sudah ada | Screenshot hasil query SQL (capture database) |
| 2 | "Perbaiki BE dan FE nya" — disertai screenshot DevTools Network yang menunjukkan `GET /api/v1/health-services/billing-management/billing/deposit-orders` mengembalikan status 500 dengan body error generik | Screenshot DevTools Network (Headers + Response body) |

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ISS-BKC-005-01 | 1 | Tabel Order Pembayaran Deposit tidak menampilkan data walau baris sudah ada di database | `BUG` | Backend | Blocker | DUGAAN (diperkuat log) | `FIX-BKC-005-01` |
| ISS-BKC-005-02 | 2 | Endpoint GET deposit-orders mengembalikan 500 Internal Server Error | `BUG` | Backend | Blocker | SUDAH-VERIFIKASI | `FIX-BKC-005-01` |
| ISS-BKC-005-T1 | — | Pola bug yang sama ada di `GetOrderDetailAsync` — modal Detail Order dan proses "Buat Order Deposit" manual akan gagal total begitu dipakai | `BUG` | Backend | High | SUDAH-VERIFIKASI | `FIX-BKC-005-02` |
| ISS-BKC-005-T2 | — | Pola bug yang sama ada di riwayat pembayaran pada Detail Order, aktif hanya pada kondisi data tertentu | `BUG` | Backend | Medium | SUDAH-VERIFIKASI | `FIX-BKC-005-03` |
| ISS-BKC-005-T3 | — | Pola bug yang sama ada di pembuatan kwitansi pembayaran deposit, aktif hanya pada kondisi data tertentu | `BUG` | Backend | Medium | SUDAH-VERIFIKASI | `FIX-BKC-005-04` |

## 4. Rincian per temuan

### ISS-BKC-005-01 — Tabel Order Pembayaran Deposit tidak menampilkan data walau baris sudah ada di database

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | DUGAAN, diperkuat log server — lihat penjelasan |
| **Perbaikan** | `FIX-BKC-005-01` |

**Apa yang terjadi.** Petugas kasir membuka layar Deposit Rawat Inap, tab "Order Pembayaran
Deposit Pasien". Tabelnya tidak menampilkan baris apa pun, padahal lewat pengecekan langsung ke
database terbukti ada order deposit berstatus `UNPAID` senilai Rp 10.000.000 untuk pasien
tersebut.

**Kenapa terjadi.** `SUDAH-VERIFIKASI`: akar masalah teknisnya identik dengan ISS-BKC-005-02 di
bawah (baris kode yang sama, `BillingDepositOrderService.cs:283`). `DUGAAN`, diperkuat log: saat
laporan ini ditulis, order `DPO-20261016-00001` sudah tercatat sejak `CreateDateTime` 16:38:41,
sebelum laporan No. 1 disampaikan — berarti kemungkinan besar permintaan tabel pada saat itu
sudah terkena 500 yang sama, tetapi pengguna hanya melihat tabel kosong tanpa memperhatikan
pesan error di layar atau tab Network. Log server (`Logs/quilvian-backend-20261016.json`)
mencatat pola 500 yang berulang pada endpoint yang sama sepanjang hari (09:19, 13:03, 13:06,
13:11, 13:16, 13:17, 13:18 UTC), diselingi **satu** respons 200 pada 13:17:07 UTC — pola ini
konsisten dengan baris kode yang hanya melempar error ketika hasil pencarian **tidak kosong**:
permintaan yang kebetulan hasilnya nol baris (misalnya karena filter atau pencarian berbeda)
tetap mengembalikan 200 dengan daftar kosong, sedangkan permintaan yang menemukan baris apa pun
selalu 500.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Tidak ada desain yang disetujui yang meminta
tabel ini kosong ketika datanya ada.

**Dampak nyata.** Petugas kasir tidak bisa melihat daftar order deposit pasien rawat inap sama
sekali begitu ada data — padahal ini adalah tabel utama pada tab tersebut, dipakai untuk memilih
order mana yang akan diproses pembayarannya.

**Rekomendasi.** Perbaiki akar masalah backend pada ISS-BKC-005-02; begitu endpoint dasarnya
sembuh, butir ini otomatis ikut sembuh karena sumbernya sama.

---

### ISS-BKC-005-02 — Endpoint GET deposit-orders mengembalikan 500 Internal Server Error

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-BKC-005-01` |

**Apa yang terjadi.** Setiap kali ada minimal satu order deposit yang cocok dengan pencarian,
server membatalkan seluruh permintaan dan mengembalikan error generik "Terjadi kesalahan pada
server." — bukan data order yang diminta.

**Kenapa terjadi.** `SUDAH-VERIFIKASI` lewat log server dan pembacaan langsung source code.

Log server `Logs/quilvian-backend-20261016.json` baris sekitar waktu `2026-10-16T13:18:07Z`
(20:18:07 WIB) mencatat exception persis saat permintaan ini:

```text
System.ArgumentException: The UTC Offset for Utc DateTime instances must be 0. (Parameter 'offset')
   at System.DateTimeOffset..ctor(DateTime dateTime, TimeSpan offset)
   at ...BillingDepositOrderService.<>c__DisplayClass8_0.<SearchOrdersAsync>b__23(...)
   at System.Linq.Enumerable.ListSelectIterator`2.ToList()
   at ...BillingDepositOrderService.SearchOrdersAsync(...)
   at ...BillingDepositOrdersController.SearchDepositOrders(...)
```

Baris kode penyebabnya, `Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositOrderService.cs:283`:

```csharp
CreatedAt = new DateTimeOffset(ord.CreateDateTime, TimeSpan.FromHours(7)),
```

Rantai sebabnya:

1. Kolom `CreateDateTime` pada tabel `BilDepositPaymentOrder` bertipe `timestamp with time zone`
   (`Repositories/Configurations/HealthServices/BillingManagement/Billing/BilDepositPaymentOrderConfiguration.cs:33`).
2. Aplikasi ini tidak mengaktifkan `EnableLegacyTimestampBehavior` Npgsql (tidak ditemukan di
   `Program.cs`), sehingga perilaku modern berlaku: Npgsql selalu mengembalikan nilai kolom
   `timestamp with time zone` sebagai `DateTime` dengan `Kind = DateTimeKind.Utc`.
3. Constructor `.NET` `DateTimeOffset(DateTime dateTime, TimeSpan offset)` **mewajibkan** `offset`
   bernilai nol ketika `dateTime.Kind == DateTimeKind.Utc` — melanggarnya melempar
   `ArgumentException` persis seperti pesan di log.
4. Baris 283 memaksa offset `TimeSpan.FromHours(7)` (niatnya menampilkan jam WIB) pada
   `ord.CreateDateTime` yang Kind-nya sudah pasti `Utc` — sehingga **selalu** melempar exception,
   untuk setiap baris, tanpa terkecuali, kapan pun ada hasil query yang tidak kosong.
5. Karena ini terjadi di dalam `rawItems.Select(x => ...).ToList()` (baris 253–286), satu baris
   bermasalah membatalkan seluruh daftar, bukan hanya baris itu sendiri.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Tidak ada desain yang menghendaki endpoint ini
gagal; maksud kode aslinya adalah menampilkan jam dibuat dalam zona WIB, tapi caranya salah.

**Dampak nyata.** Sejak order deposit pertama dibuat hari ini, endpoint pencarian order deposit
rawat inap tidak bisa dipakai sama sekali oleh siapa pun — bukan hanya satu pengguna atau satu
browser, karena bug-nya di server, bukan di client.

**Rekomendasi.** Ganti cara membentuk `DateTimeOffset` dari `ord.CreateDateTime` memakai pola yang
sudah mapan dan terbukti aman di 20+ tempat lain pada repository ini — lihat solusi terpilih pada
plan repair.

---

### ISS-BKC-005-T1 — Pola bug yang sama ada di `GetOrderDetailAsync`: Detail Order dan "Buat Order Deposit" manual akan gagal total begitu dipakai

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri akar masalah ISS-BKC-005-02 di file yang sama; pola kodenya identik dan memakai kolom `CreateDateTime` yang Kind-nya sama-sama selalu `Utc` |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-BKC-005-02` |

**Apa yang terjadi.** Fitur ini belum pernah dilaporkan gagal oleh pengguna, tapi akan gagal
dengan 500 yang sama persis begitu dipakai: membuka modal "Detail Order" dari tabel manapun
order-nya berhasil ditampilkan, dan proses "Buat Order Deposit" manual dari form (bukan dari
admisi rawat inap otomatis).

**Kenapa terjadi.** `SUDAH-VERIFIKASI`. `BillingDepositOrderService.cs:425`, di dalam method
`GetOrderDetailAsync`:

```csharp
CreatedAt = new DateTimeOffset(order.CreateDateTime, TimeSpan.FromHours(7)),
```

Baris kode dan akar masalahnya identik dengan ISS-BKC-005-02. Method `GetOrderDetailAsync` ini
dipanggil dari dua jalur: (1) langsung sebagai endpoint "Detail Order" yang diklik dari tabel, dan
(2) di akhir method `CreateOrderAsync` (`BillingDepositOrderService.cs:151`,
`return await GetOrderDetailAsync(order.Id, cancellationToken);`) — artinya proses "Buat Order
Deposit" manual lewat endpoint `POST /api/v1/health-services/billing-management/billing/deposit-orders`
akan berhasil menyimpan baris barunya ke database, **tapi gagal 500 saat membentuk respons** untuk
dikirim balik ke layar. Order deposit awal admisi rawat inap (dibuat lewat
`InpEpisodeService.OpenAdmissionAsync`) tidak lewat jalur ini sehingga tidak terdampak saat
pembuatannya — tapi tetap terdampak begitu detailnya dibuka lewat jalur (1).

**Apakah ini menyimpang dari desain?** Ya, `BUG`.

**Dampak nyata.** Petugas tidak akan pernah bisa membuat order deposit tambahan secara manual
dari layar ini (misalnya untuk top-up deposit di luar alur admisi otomatis), dan tidak akan
pernah bisa membuka detail order apa pun dari tabel — begitu bug ISS-BKC-005-02 diperbaiki
sendirian tanpa memperbaiki ini, pengguna akan langsung menemukan 500 baru di tempat lain.

**Rekomendasi.** Perbaiki dalam satu paket bersama ISS-BKC-005-02 — lihat plan repair.

---

### ISS-BKC-005-T2 — Pola bug yang sama ada di riwayat pembayaran pada Detail Order (kondisional)

| | |
| --- | --- |
| **Kenapa dicantumkan** | Pola kode identik ditemukan di file yang sama saat menelusuri ISS-BKC-005-T1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-BKC-005-03` |

**Apa yang terjadi.** Saat menampilkan riwayat pembayaran pada Detail Order, baris riwayat bisa
gagal ditampilkan untuk kondisi data tertentu.

**Kenapa terjadi.** `SUDAH-VERIFIKASI`. `BillingDepositOrderService.cs:369`:

```csharp
PaidAt = mov != null ? mov.OccurredAt : new DateTimeOffset(p.CreateDateTime, TimeSpan.FromHours(7)),
```

Jalur bermasalah (`new DateTimeOffset(p.CreateDateTime, TimeSpan.FromHours(7))`) hanya tereksekusi
ketika `mov` (baris `BilDepositMovement` terkait) bernilai `null` — artinya baris pembayaran
(`BilDepositOrderPayment`) ada, tapi baris buku besar deposit (`BilDepositMovement`) yang
dirujuknya tidak ditemukan. Ini kondisi data yang seharusnya jarang terjadi (indikasi data tidak
konsisten), tapi kalau terjadi, akan melempar `ArgumentException` yang sama persis.

**Apakah ini menyimpang dari desain?** Ya, `BUG` — menampilkan tanggal fallback seharusnya tidak
pernah membuat seluruh halaman gagal dimuat.

**Dampak nyata.** Lebih rendah dari T1 karena memerlukan kondisi data spesifik, tapi tetap
berpotensi memblokir tampilan Detail Order untuk order tertentu di masa depan, termasuk kalau
ada proses reversal/pembalikan pembayaran yang meninggalkan data tidak konsisten.

**Rekomendasi.** Perbaiki dengan pola yang sama, sekaligus (biaya tambahannya minimal karena
sudah menyentuh file yang sama).

---

### ISS-BKC-005-T3 — Pola bug yang sama ada di pembuatan kwitansi pembayaran deposit (kondisional)

| | |
| --- | --- |
| **Kenapa dicantumkan** | Pola kode identik ditemukan di file yang sama saat menelusuri ISS-BKC-005-T1 |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-BKC-005-04` |

**Apa yang terjadi.** Saat mencetak kwitansi pembayaran deposit, proses bisa gagal untuk kondisi
data tertentu.

**Kenapa terjadi.** `SUDAH-VERIFIKASI`. `BillingDepositOrderService.cs:746`:

```csharp
PaymentDateTime = movement != null ? movement.OccurredAt : new DateTimeOffset(payment.CreateDateTime, TimeSpan.FromHours(7)),
```

Sama seperti T2: jalur bermasalah hanya tereksekusi ketika `movement` (baris `BilDepositMovement`
terkait pembayaran ini) tidak ditemukan — kondisi data yang seharusnya jarang, tapi tetap mungkin
terjadi.

**Apakah ini menyimpang dari desain?** Ya, `BUG`.

**Dampak nyata.** Berpotensi memblokir pencetakan kwitansi untuk pembayaran tertentu di masa
depan.

**Rekomendasi.** Perbaiki dengan pola yang sama, sekaligus.

## 5. Tanya-jawab pelapor

Tidak ada pertanyaan eksplisit dari pelapor pada laporan ini — kedua laporan berupa pernyataan
masalah ("kenapa tabel tidak menampilkan data", "perbaiki BE dan FE nya"), bukan pertanyaan.

## 6. Temuan tambahan

Lihat ISS-BKC-005-T1, ISS-BKC-005-T2, dan ISS-BKC-005-T3 pada bagian 4 di atas.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada. Root cause sudah terverifikasi penuh lewat log server dan source
code, tidak ada bagian yang memerlukan konfirmasi tambahan dari pelapor.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Apakah keempat titik bug (ISS-BKC-005-02, T1, T2, T3) diperbaiki dalam satu task builder, atau dipisah per titik? | Satu task gabungan / dipisah 4 task | Satu task gabungan — keempatnya adalah satu akar masalah yang sama (pola kode identik di satu file), memperbaikinya terpisah hanya menambah overhead tanpa mengurangi risiko | Tidak ada yang terblokir; ini hanya memengaruhi pengelompokan task di roadmap |

Catatan: permintaan pengguna "Perbaiki BE dan FE nya" sudah merupakan wewenang perbaikan yang
jelas untuk sisi BE. Tidak ada keputusan bisnis/klinis yang dibutuhkan — murni perbaikan teknis
dengan pola pengganti yang sudah terbukti aman dan dipakai konsisten di tempat lain pada
repository yang sama.

## 8. Catatan pola

Pola `new DateTimeOffset(<kolom DateTime bertipe timestamptz>, TimeSpan.FromHours(7))` adalah
kesalahan yang **akan selalu meledak** kapan pun dipakai pada kolom yang dipetakan dari
`timestamp with time zone` di PostgreSQL lewat Npgsql modern (perilaku default repository ini).
Penelusuran `grep -rn "new DateTimeOffset(" Areas Services` menunjukkan repository ini sudah
punya pola aman yang dipakai konsisten di 20+ lokasi lain, dua varian setara:

- `new DateTimeOffset(DateTime.SpecifyKind(x.CreateDateTime, DateTimeKind.Utc))` — dipakai di
  `AccAccountingEventService.cs`, `FinanceReceivableBillingDataService.cs`,
  `BillingDrugReturnService.cs`, `BillingClinicalChargeBridgeService.cs`,
  `BillingCalculationService.cs`, `PatientBillingSummaryService.Breakdown.cs`, dan lain-lain.
- `new DateTimeOffset(x.CreateDateTime, TimeSpan.Zero)` — dipakai di
  `FinanceReceivableInvoiceBatchService.cs`, `AttendanceSelfServiceCaptureService.cs`, dan
  lain-lain.

Keduanya setara secara hasil (offset nol, karena nilainya memang sudah UTC) dan sama-sama tidak
pernah melempar exception. Usulan pencegahan: pola `new DateTimeOffset(<DateTime>, TimeSpan.FromHours(N))`
dengan `N != 0` pada kolom bertipe `timestamp with time zone` sebaiknya dihindari di seluruh
codebase — konversi ke zona tampilan (WIB) semestinya menjadi tanggung jawab client/frontend saat
merender, bukan dibentuk ulang di response DTO backend. Ini sejalan dengan temuan bagian 4 bahwa
frontend (`inpatient-deposit-orders-table.jsx:54-61`) sudah benar melakukan ini sendiri lewat
`Intl.DateTimeFormat("id-ID", { dateStyle: "medium" })` tanpa opsi `timeZone` eksplisit — otomatis
memakai zona waktu lokal browser pengguna.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-16 | Issue dibuat dari laporan 2 butir, ditemukan 3 temuan tambahan dengan akar masalah sama | `diagnose-module-issue` |
