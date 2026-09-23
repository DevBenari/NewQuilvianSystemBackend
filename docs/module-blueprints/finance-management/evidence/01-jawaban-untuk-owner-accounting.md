# Jawaban Finance atas Paket Kontrak Kejadian Keuangan

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 20 September 2026 |
| Menjawab | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` (15 September 2026) |
| Sifat | **Ratifikasi dan jawaban atas enam pertanyaan.** Belum meminta Accounting menulis kode apa pun sekarang |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`, keputusan `FIN-DEC-001`..`023`, seluruhnya `approved` 20 September 2026 |
| Bila berbeda | Decision log Finance di atas yang berlaku, bukan ringkasan ini |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Finance
yang lain.

---

## 1. Ringkasan satu paragraf

Finance **meratifikasi** kontrak `ACC-XMOD-0.2` apa adanya: arah satu arah
Billing → Finance → Accounting, Finance sebagai satu-satunya penerbit kejadian, dan bentuk pesan
dua belas bidang tanpa perubahan. Keenam pertanyaan Anda terjawab di bawah. Satu di antaranya
(bentuk rinci saldo subledger) berupa **usulan yang menunggu konfirmasi Anda**, bukan jawaban
final.

Finance sudah merancang kotak keluar kejadiannya dan mulai membangunnya tanpa menunggu pintu
masuk Accounting siap. Jadi ketika endpoint Anda jadi, antrean kejadian sudah terisi dan
tinggal dikirim.

---

## 2. Jawaban atas enam pertanyaan

### Pertanyaan 1 — Ratifikasi rantai Billing → Finance → Accounting

**Ya, Finance meratifikasi.**

Finance menerima perannya sebagai satu-satunya penerbit kejadian, sebagai kejadian tersendiri —
bukan meneruskan `BilArHandoff` mentah. Dasar: `FIN-DEC-001`.

Konsekuensi yang Finance terima:

- Accounting tidak pernah membaca tabel Billing maupun Finance.
- Billing tidak pernah mengirim kejadian ke Accounting langsung.
- Finance tidak pernah menerima kejadian balik dari Accounting.

### Pertanyaan 2 — Apakah bentuk dua belas bidang dapat dipenuhi Finance?

**Ya, seluruhnya. Tidak ada bidang yang tidak dimiliki Finance.**

| Bidang | Sumbernya di Finance |
|---|---|
| `EventNumber` | Dibuat tabel kotak keluar Finance |
| `EventTypeCode` | Salah satu dari 17 kode pada bagian 3 |
| `SourceModule` | Selalu `Finance` |
| `SourceTransactionId` | Nomor piutang, penerimaan, utang, pembayaran, atau mutasi kas kecil |
| `SourceVersion` | Dinaikkan setiap koreksi |
| `EventOccurredAt` | Waktu kejadian bisnis sebenarnya |
| `AccountingDate` | Tanggal pembukuan menurut aturan Finance |
| `Amount` | Nilai yang diakui Finance |
| `CurrencyCode` | `IDR`, ditegakkan check constraint di database Finance |
| `LegalEntityId` | Rujukan badan hukum |
| `CorrelationId` | Diwarisi dari rantai Billing → Finance |
| `CausationId` | Tindakan penyebab |

Bidang opsional `Components` juga dapat dipenuhi.

Catatan koreksi yang Anda sebut sudah kami perhatikan: Finance memakai bentuk `0.2`, bukan `0.1`
yang sudah usang.

### Pertanyaan 3 — Jenis kejadian apa saja yang akan diterbitkan Finance?

Tujuh belas kode berikut, diajukan **sekaligus** supaya Accounting dapat menyusun aturan posting
sekali jalan tanpa ratifikasi berulang tiap fase. Dasar: `FIN-DEC-002`.

| Kode | Dipicu oleh | Perkiraan gelombang |
|---|---|---|
| `PENGAKUAN-PIUTANG` | Piutang diakui dari fakta AR Billing | MVP-1 |
| `PENERIMAAN-KASIR` | Penerimaan dari tender Billing **sebelum/tanpa** piutang | MVP-2 |
| `PEMBALIKAN-PENERIMAAN-KASIR` | Pembalikan penerimaan kasir | MVP-3 |
| `PENERIMAAN-PIUTANG` | Uang diterima untuk piutang yang **sudah** ada | MVP-3 |
| `PENYESUAIAN-PIUTANG` | Koreksi piutang disetujui | MVP-3 |
| `PEMUTIHAN-PIUTANG` | Penghapusan piutang disetujui | MVP-3 |
| `SETORAN-BANK` | Setoran bank diposting | MVP-4 |
| `PETTY-CASH-TOP-UP` | Penambahan saldo kas kecil | MVP-5 |
| `PETTY-CASH-DISBURSEMENT` | Pencairan kas kecil | MVP-5 |
| `PETTY-CASH-RETURN` | Pengembalian sisa kas kecil | MVP-5 |
| `PETTY-CASH-REVERSAL` | Pembalikan pencairan kas kecil | MVP-5 |
| `PETTY-CASH-ADJUSTMENT` | Koreksi saldo kas kecil | MVP-5 |
| `PENGAKUAN-HUTANG-SUPPLIER` | Utang supplier diinput | Pasca-MVP |
| `PEMBAYARAN-HUTANG-SUPPLIER` | Pembayaran supplier ditandai dibayar | Pasca-MVP |
| `PENGAKUAN-HUTANG-DOKTER` | Fee dokter yang sudah disetujui menjadi utang | Pasca-MVP |
| `PEMBAYARAN-HUTANG-DOKTER` | Pembayaran dokter ditandai dibayar | Pasca-MVP |
| `PENYESUAIAN-HUTANG` | Koreksi utang disetujui | Pasca-MVP |

**Dua kode yang mudah tertukar, dan bedanya penting bagi aturan posting Anda:**

- `PENERIMAAN-KASIR` terbit saat pasien membayar di kasir dan **belum ada piutang Finance sama
  sekali**. Lawan jurnalnya bergantung kebijakan pengakuan — piutang, uang muka pasien, atau
  pendapatan — dan pilihan itu milik aturan posting Anda, bukan Finance.
- `PENERIMAAN-PIUTANG` terbit saat penjamin melunasi piutang yang **sudah** diakui sebelumnya.
  Lawan jurnalnya akun kontrol piutang.

Menyamakan keduanya akan membuat piutang berkurang dua kali atau tidak berkurang sama sekali.

**Permintaan kecil yang menyentuh aturan posting Anda.** Finance memilih memisahkan pengakuan
jasa medis dokter dari pengakuan piutang (Opsi B pada analisis kami, `FIN-DEC-003`). Karena itu
kejadian `PENGAKUAN-PIUTANG` dari Finance **tidak akan pernah** membawa komponen `JASA_MEDIS`.
Contoh aturan posting yang memuat `JASA_MEDIS` di dalam `PENGAKUAN-PIUTANG` perlu disesuaikan,
supaya jasa medis tidak terbukukan dua kali ketika `PENGAKUAN-HUTANG-DOKTER` menyusul.

### Pertanyaan 4 — Bagaimana akun layanan Finance login?

**Usulan Finance: service account internal lewat mekanisme autentikasi yang sudah ada di
repository**, bukan skema OAuth baru. Dasar: `FIN-DEC-007`.

Alasannya sederhana: Billing, Finance, dan Accounting hidup dalam satu aplikasi modular
monolith, sehingga membangun skema autentikasi antar-service yang baru menambah infrastruktur
tanpa menambah keamanan yang setara.

**Ini usulan, bukan keputusan final.** Keputusannya milik tiga pihak: Finance, Accounting, dan
Platform. Finance siap mengikuti bila Platform menghendaki mekanisme lain.

### Pertanyaan 5 — Bentuk rinci pesan saldo subledger per periode

**Usulan Finance**, mengikuti pola penamaan dua belas bidang yang sudah diratifikasi. Dasar:
`FIN-DEC-023`.

| Bidang | Tipe | Contoh |
|---|---|---|
| `LegalEntityId` | `Guid` | Id badan hukum |
| `AccountingPeriodCode` | `string(20)` | `2026-09` |
| `ControlAccountCode` | `string(50)` | Kode akun kontrol milik Accounting |
| `SubledgerBalance` | `decimal(18,2)` | `425000000.00` |
| `AsOfDate` | `date` | `2026-09-30` |

**Ini bagian yang paling butuh koreksi Anda.** Nama bidangnya kami karang mengikuti pola, bukan
mengikuti kebutuhan sisi Accounting yang sebenarnya. Silakan ubah; Finance mengikuti.

Catatan jadwal: fitur ini ada di urutan paling belakang rencana Finance, karena baru berguna
setelah pengiriman kejadian aktif. Jadi jawaban atas pertanyaan ini tidak menahan pekerjaan
Finance yang lain.

### Pertanyaan 6 — Sejak tanggal berapa transaksi dikirim, dan bagaimana saldo sebelumnya?

**Jawaban Finance:** cutover berlaku sejak **tanggal go-live integrasi**, tidak retroaktif.
Dasar: `FIN-DEC-008`.

| Hal | Ketentuan |
|---|---|
| Yang dikirim | Hanya transaksi Finance yang terjadi **setelah** endpoint aktif |
| Transaksi sebelum tanggal itu | Tidak dikirim, dan tidak direkonstruksi dari histori Finance |
| Saldo sebelum cutover | **Diinput Accounting sebagai saldo awal manual, satu kali** |
| Tanggal pastinya | Belum ditetapkan — menunggu endpoint Accounting tersedia |

Pilihan ini diambil supaya transaksi di sekitar tanggal cutover tidak terjurnal dua kali.
Konsekuensinya: Accounting perlu satu kali pekerjaan input saldo awal untuk piutang, utang, dan
kas.

---

## 3. Yang Finance jamin dari sisinya

| Hal | Jaminan | Cara menegakkannya |
|---|---|---|
| Data pasien | Nama pasien, nomor rekam medis, nomor kunjungan, dan `DoctorId` **tidak pernah** masuk pesan | Aturan validasi di Finance sebelum baris kejadian tersimpan |
| Mata uang | Hanya `IDR` | Check constraint di database Finance, bukan hanya pemeriksaan kode |
| Pencegahan ganda lapis pertama | `EventNumber` unik | Unique index |
| Pencegahan ganda lapis kedua | (`SourceModule`, `SourceTransactionId`, `EventTypeCode`, `SourceVersion`) unik | Unique index |
| Koreksi | Selalu menaikkan `SourceVersion`, tidak pernah memakai versi yang sama | Aturan validasi Finance |
| Kejadian tidak hilang | Baris kejadian ditulis **di transaksi database yang sama** dengan fakta bisnisnya | Pola transactional outbox |
| Balasan `422` | Finance menandainya tertahan dan **tidak** mengirim kejadian baru | Aturan penanganan balasan |

Butir terakhir menjawab kekhawatiran yang Anda tulis: Finance tidak akan memperlakukan `422`
sebagai gagal lalu mengirim ulang sebagai kejadian baru.

---

## 4. Satu hal yang Finance minta diketahui

Finance menahan sebagian kejadian penerimaan, dan ini disengaja.

Billing memungkinkan pasien membayar **sebelum** tagihannya difinalisasi. Uangnya nyata dan
langsung tercatat di Finance, tetapi kejadian akuntansinya kami tahan sampai tagihan final.

| Keadaan | Di Finance | Kejadian ke Accounting |
|---|---|---|
| Tender berhasil, tagihan masih terbuka | Penerimaan tercatat penuh | **Ditahan** |
| Tagihan kemudian final | Tidak berubah | Dilepas, siap dikirim |
| Tender berhasil, tagihan sudah final | Penerimaan tercatat | Langsung siap dikirim |

Akibatnya bagi Anda: akan ada jeda antara uang diterima kasir dan jurnalnya muncul di buku
besar. Jeda itu pilihan sadar Finance (`FIN-DEC-004`), bukan keterlambatan teknis.

Bila Accounting lebih menghendaki kejadian terbit segera sebagai uang muka pasien, itu keputusan
yang menyentuh bagan akun Anda — silakan sampaikan, Finance akan menyesuaikan.

---

## 5. Keadaan pekerjaan Finance

| Hal | Keadaan pada 20 September 2026 |
|---|---|
| Keputusan bisnis Finance | 23 keputusan, seluruhnya disetujui owner |
| Arsitektur backend Finance | 24 keputusan arsitektur, seluruhnya disetujui owner |
| Kotak keluar kejadian | Sudah dirancang lengkap; belum ditulis kodenya |
| Pengiriman ke Accounting | **Sengaja belum dibangun** — menunggu endpoint Anda |
| Kejadian yang sudah terkirim | Nol |

Finance **tidak** menunggu Accounting untuk mulai bekerja. Yang menunggu hanya bagian
pengirimannya.

---

## 6. Yang Finance butuhkan dari Accounting

| Butuh | Kapan dibutuhkan | Menahan apa |
|---|---|---|
| Koreksi atau konfirmasi nama bidang saldo subledger (bagian 2, pertanyaan 5) | Tidak mendesak | Fitur tutup periode Finance, urutan paling belakang |
| Ratifikasi tujuh belas kode kejadian | Sebelum kejadian pertama dikirim | Kejadian berjenis belum terdaftar akan tertahan di sisi Anda |
| Penyesuaian aturan posting agar `JASA_MEDIS` tidak dobel | Sebelum `PENGAKUAN-HUTANG-DOKTER` pertama | Rumpun utang dokter Finance, sudah pasca-MVP |
| Kepastian mekanisme autentikasi bersama Platform | Sebelum pengiriman diaktifkan | Pengaktifan pengiriman |
| Kabar ketika endpoint penerima sudah dibangun | Kapan saja | Pengaktifan pengiriman |

Tidak satu pun dari kelima hal di atas menahan pekerjaan Finance yang sedang berjalan.

---

## 7. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | 23 keputusan bisnis Finance beserta owner dan tanggalnya |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` | Kontrak integrasi lengkap, termasuk kewajiban Finance atas setiap kode balasan |
| `docs/module-blueprints/finance-management/02-backend-architecture.md` | Bentuk kotak keluar dan dua lapis pencegahan ganda |
| `docs/module-blueprints/finance-management/04-prd-to-mvp.md` | Urutan gelombang pengerjaan Finance |
