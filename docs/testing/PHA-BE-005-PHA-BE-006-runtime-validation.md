# PHA-BE-005 & PHA-BE-006 — Validasi Runtime

Status: **PASS** · 23 September 2026 · Pharmacy Backend

## Ringkasan untuk Pembaca Umum

Sejak 24 Agustus 2026 resep rawat jalan macet permanen: tidak ada satu pun jalur kode yang
memindahkan resep keluar dari keadaan "menunggu pembayaran". Empat endpoint yang dulu bisa
menyatakan resep lunas sudah dihapus permanen — karena siapa pun yang boleh mengubah resep dapat
menyatakannya lunas — dan penggantinya belum pernah dibangun.

Pengujian ini membuktikan penggantinya bekerja. Ketika Billing menerbitkan surat bahwa sebuah
resep sudah beres secara finansial, resep itu **berpindah sendiri** ke antrean apoteker begitu
layarnya dibuka, tanpa petugas menekan apa pun. Ketika izinnya dicabut, pekerjaan Farmasi
berhenti di tempat — tidak ditarik mundur, dan tidak ada tombol yang bisa menerobosnya. Ketika
izin dipulihkan, pekerjaan dilanjutkan dari titik terakhir tanpa mengulang apa pun.

Satu hal yang paling penting terbukti di sini: yang melepaskan resep adalah **surat dari
Billing**, bukan pernyataan siapa pun di Farmasi.

---

## Environment test

| Hal | Nilai |
| --- | --- |
| Database | `localhost` / `QuilvianNewDevIkbalFr` (database uji; database tim tidak tersentuh) |
| Koneksi backend | env `ConnectionStrings__DefaultConnection`; worktree tidak memuat `appsettings.Development.json` dan `appsettings.json` tidak memuat `ConnectionStrings`, sehingga tidak ada fallback yang dapat menunjuk database lain |
| Worktree | `C:\tmp\phm`, branch `Ikbal` |
| Build | `dotnet build -p:SkipMigrationMetadata=true` beserta targets tambahan yang mengembalikan snapshot — `0 error`, 223 warning (sama dengan bawaan repo) |
| Hak akses | **menyala**; `Security:Authorization:Enabled=false` hanya ada pada `appsettings.Development.json` yang tidak dibaca worktree, sehingga pengujian berjalan lebih ketat, bukan lebih longgar |
| Akun | `superadmin`, login `200` |

## Migration yang diterapkan

| Migration | Keterangan |
| --- | --- |
| `20260922060000_AddPrescriptionFinancialProjection` | Tabel `PhmPrescriptionFinancialProjection`: 26 kolom, PK, FK ke `PhmPrescription` (`ON DELETE RESTRICT`), 4 index — `PrescriptionId` **unik** |
| 21 migration rentang `20260916000000`–`20260922015126` | Menutup selisih skema database uji terhadap branch: 39 tabel baru (termasuk `BilPrescriptionClearanceHandoff`), 174 index, 44 kolom tambahan pada tabel yang sudah ada |

Keadaan akhir database: **680 tabel, 23 baris `__EFMigrationsHistory`** (dari 641 / 2). Fixture
lama utuh: 2 resep, 2 batch, 5 baris ledger. Tidak ada `DROP TABLE`, `DROP COLUMN`, `DELETE`,
maupun `TRUNCATE` sepanjang pembaruan.

Sebelum diterapkan, seluruh skrip diperiksa terhadap database: 44 `ADD COLUMN` → **0 bentrok**;
39 `CREATE TABLE` → **0 yang sudah ada**. Migration `20260911000000` sengaja dikecualikan karena
kolomnya sudah ada di baseline.

## Perbaikan yang diperlukan agar backend dapat berjalan

`BilConsumerHandoffService` tidak terdaftar pada DI container, padahal dituntut **9 berkas
Billing** lewat konstruktor. Validasi service provider karena itu menolak membangun aplikasi dan
backend **tidak dapat start sama sekali** — bukan hanya modul Billing. Keadaan ini sudah ada pada
`origin/Ikbal` sebelum pekerjaan ini, dan kode Farmasi tidak memakai service tersebut. Ditambahkan
satu baris `AddScoped<BilConsumerHandoffService>()`; nol aturan bisnis disentuh.

## Endpoint yang dites

| Method | Path |
| --- | --- |
| `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` |
| `GET` | `/api/v1/health-services/pharmacy-management/prescriptions/{id}` |
| `POST` | `/api/v1/health-services/pharmacy-management/prescription-reviews/by-prescription/{id}/start` |

**Nol endpoint baru** ditambahkan slice ini, sesuai `PHA-API-CLEARANCE-v1`.

## Data uji

Resep `RX-20260923-00001` (`2af8cd78-…`), dibuat lewat API sehingga keadaan awalnya berasal dari
kode: `PrescriptionStatus=Draft`, `PaymentStatus=NotBilled`, `FulfillmentStatus=WaitingForPayment`.

Surat clearance dibuat sebagai **baris sungguhan** pada `BilInvoice` dan
`BilPrescriptionClearanceHandoff` — bukan disalin langsung ke proyeksi Farmasi — supaya yang
teruji memang cara Farmasi mengonsumsinya:

| Versi | ClearanceStatus | FinancialOutcome | ReasonCode |
| --- | --- | --- | --- |
| 1 | `CLEARED` | `PAID` | `INVOICE_SETTLED` |
| 2 | `CLEARED` | `INSURANCE_APPROVED` | `INVOICE_SETTLED` |
| 3 | `REVOKED` | — | `PAYMENT_REVERSED` |
| 4 | `CLEARED` | `PAID` | `INVOICE_SETTLED` |

## Skenario 1–7

### Skenario 1 — Resep tanpa keadaan finansial tetap terbaca

`GET /prescriptions/{id}` sebelum ada surat apa pun.

* **Expected:** `200` beserta penanda belum diketahui; bukan `404` dan bukan galat.
* **Actual:** `200`, `clearanceStatus: UNKNOWN`, `isKnown: false`, `isCleared: false`,
  `holdReasonCode: PHA_CLR_UNKNOWN`, `holdReason: "Keadaan pembayaran resep ini belum diketahui.
  Obat belum boleh diserahkan."`, `syncState: NEVER_SYNCED`, `financialVersion: 0`.
* **Status: PASS**

### Skenario 2 — Surat pelepasan memindahkan resep ke antrean

Surat v1 `CLEARED/PAID` terbit, lalu `GET /prescriptions/{id}`.

* **Expected:** resep berpindah dari menunggu pembayaran ke antrean apoteker tanpa tindakan
  petugas; salinan terisi beserta nomor versinya.
* **Actual:** `FulfillmentStatus` **2 → 4** (`WaitingForPayment` → `QueuedAtPharmacy`),
  `PaymentStatus` **1 → 5** (`NotBilled` → `Paid`), proyeksi `CLEARED / PAID / v1 / SYNCED`.
* **Status: PASS** — menutup kemacetan yang berjalan sejak 24 Agustus 2026.

### Skenario 3 — Hasil penjaminan tidak menjadi lunas tunai

Surat v2 `CLEARED/INSURANCE_APPROVED`, lalu `GET /prescriptions/{id}`.

* **Expected:** tercatat sebagai disetujui penjamin, **bukan** lunas tunai (`PHA-DEC-065`).
* **Actual:** `financialOutcome: INSURANCE_APPROVED`, `financialVersion: 2`,
  `PaymentStatus = 6` (Disetujui Asuransi) — **bukan** 5 (Lunas).
* **Status: PASS**

### Skenario 4 — Surat bernomor versi lebih lama tidak dikonsumsi

Setelah proyeksi berada pada v3 (`REVOKED`), surat v1 dan v2 yang berstatus `CLEARED` **masih ada**
di tabel Billing. Layar resep dibuka tiga kali berturut-turut.

* **Expected:** salinan tidak berubah; tidak ada galat; tidak ada percobaan ulang; resep yang sudah
  dicabut **tidak** kembali terbaca boleh dikerjakan.
* **Actual:** proyeksi tetap `REVOKED / v3`, tepat **1 baris** proyeksi, `FulfillmentStatus` tetap 4,
  `RetryCount = 0`, `ErrorMessage` kosong.
* **Status: PASS**

> Catatan: percobaan menyisipkan surat kedua bernomor versi sama ditolak Billing sendiri lewat
> index unik `IX_BilPrescriptionClearanceHandoff_Prescription_Version` pada
> `(PrescriptionId, FinancialVersion)`. Skenario basi karena itu diuji dengan cara di atas, yang
> justru lebih dekat dengan kenyataan: surat lama tetap tersimpan dan berulang kali terbaca.

### Skenario 5 — Pencabutan ditahan di tempat, tidak ditarik mundur

Surat v3 `REVOKED/PAYMENT_REVERSED`, lalu `GET /prescriptions/{id}`.

* **Expected:** salinan menjadi dicabut; keadaan pemenuhan **tidak** turun; racikan tidak
  dikembalikan menjadi bahan (`PHA-DEC-069`).
* **Actual:** proyeksi `REVOKED / v3`, `isCleared: false`, `holdReasonCode: PHA_CLR_ON_HOLD`,
  `FulfillmentStatus` **tetap 4**.
* **Status: PASS**

### Skenario 6 — Gerbang menolak saat izin dicabut

`POST /prescription-reviews/by-prescription/{id}/start` ketika salinan `REVOKED`.

* **Expected:** ditolak `400` dengan kalimat `PHA-VAL-CLEARANCE-v1`; **bukan** `500`.
* **Actual:** `400`, `"Resep ini ditahan karena ada perubahan tagihan. Pekerjaan dilanjutkan
  setelah kasir menyelesaikannya."` — kata demi kata sesuai matrix.
* **Status: PASS**

### Skenario 7 — Pemulihan melanjutkan dari titik terakhir

Surat v4 `CLEARED/PAID`, lalu `GET` dan `POST .../start`.

* **Expected:** salinan kembali menyatakan boleh; pekerjaan dilanjutkan **tanpa** mengulang
  antrean, telaah, maupun penyiapan.
* **Actual:** proyeksi `CLEARED / PAID / v4`, `FulfillmentStatus` tetap 4 (tidak diulang dari awal),
  `POST .../start` → `200 "Telaah resep berhasil dimulai."`
* **Status: PASS**

## Pemetaan galat

| Gerbang | Jalur | Kode HTTP |
| --- | --- | --- |
| Mulai telaah | `InvalidOperationException` → controller | `400` |
| Mulai & selesai penyiapan | `InvalidOperationException` → controller | `400` |
| Selesai telaah obat akhir | `InvalidOperationException` → controller | `400` |
| Siapkan & eksekusi penyerahan | `PrescriptionDispensingConflictException` | `409` |

Keempat controller diverifikasi menangkap exception-nya, sehingga penolakan gerbang tidak pernah
muncul sebagai `500`.

## Batas pengujian

**Satu langkah disimulasikan, dan bukan bagian yang diuji.** Resep rawat jalan dibuat langsung
pada `WaitingForPayment`, sedangkan `PrescriptionWorkflowService.Submit` menuntut
`WaitingForClinicalFinalization` — sehingga **tidak ada jalur kode** yang membuat resep rawat
jalan menjadi `Submitted`. Itu lubang lama yang terpisah dari slice ini. `PrescriptionStatus`
karena itu disetel lewat SQL untuk mewakili **langkah dokter**; bagian finansialnya tidak
disentuh sama sekali dan tetap datang dari surat Billing.

**Satu percobaan sempat tidak sah dan diulang.** Uji surat basi yang pertama gagal karena karakter
non-ASCII pada perintah SQL (`invalid byte sequence 0x97`); suratnya tidak pernah tersimpan,
sehingga "tidak ada perubahan" saat itu tidak membuktikan apa pun. Dicatat di sini supaya hasil
yang dilaporkan hanya yang benar-benar teruji.

**Belum diuji runtime:** gerbang penyiapan, telaah obat akhir, dan penyerahan. Ketiganya terpasang
dan terverifikasi pada source, tetapi skenario runtime-nya menuntut resep yang berjalan sampai
tahap tersebut.

## Keputusan pemilik tentang `REVOKED`

Perilaku yang berlaku, ditetapkan pemilik modul 23 September 2026:

* `PaymentStatus` pada resep adalah **histori pembayaran** — ia tidak ditulis ulang ketika izin
  dicabut, dan nilainya tetap mencerminkan pembayaran yang pernah terjadi.
* `FinancialClearance` adalah **sumber keputusan** apakah Farmasi boleh melanjutkan proses.

Keduanya karena itu boleh berbeda bunyi, dan perbedaan itu disengaja. Keselamatan tidak bergantung
pada `PaymentStatus`: seluruh gerbang membaca salinan finansial.

Trace: `PHA-DEC-063`–`069`, `PHA-STATE-CLEARANCE-v1`, `PHA-VAL-CLEARANCE-v1`,
`PHA-API-CLEARANCE-v1`, `PHA-AT-CLR-01`, `02`, `03`, `10`, `11`.
