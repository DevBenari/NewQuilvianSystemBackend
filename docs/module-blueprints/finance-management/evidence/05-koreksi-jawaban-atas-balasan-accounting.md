# Koreksi atas Jawaban Finance — Kode Uang Muka, Deposit, dan Selisih Kas

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 25 September 2026 |
| Mengoreksi | `docs/module-blueprints/finance-management/evidence/04-jawaban-atas-balasan-accounting.md`, dikirim hari yang sama |
| Sifat | **Koreksi murni, bukan perubahan arah.** Tiga permintaan Anda pada surat sebelumnya (bagian 5 butir 1, 3, 5) tetap dijawab dengan semangat yang sama — hanya jumlah dan bentuk kode kejadiannya berubah dari 4 menjadi 7, sebelum Anda sempat menyusun aturan posting apa pun berdasarkan surat lama |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`, keputusan `FIN-DEC-040` sampai `FIN-DEC-044`, seluruhnya `approved` 25 September 2026 |
| Kontrak yang berlaku | Bila berkas ini berbeda dari decision log Finance di atas, decision log yang berlaku |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka `evidence/04` lebih dulu.

---

## 1. Ringkasan satu paragraf

Mohon maaf, surat kami yang terkirim hari ini (`evidence/04`) keliru pada satu titik penting:
kode `PEMAKAIAN-UANG-MUKA-DEPOSIT` yang kami usulkan menggabungkan dua fakta keuangan yang
**lawan jurnalnya berbeda** — pemakaian uang muka untuk melunasi tagihan (tidak ada kas
bergerak) dan pengembalian uang muka secara tunai ke pasien (kas keluar). Ini persis kesalahan
yang Anda sendiri peringatkan soal `PENERIMAAN-KASIR` pada surat Anda: satu kode dengan lebih
dari satu lawan jurnal, buku besar tetap seimbang, dan kesalahannya baru ketahuan saat
pemeriksaan. Kami menemukannya sendiri lewat pemeriksaan kode lebih lanjut, **sebelum** Anda
sempat menyusun aturan posting apa pun — jadi surat ini adalah koreksi, bukan permintaan ubah
aturan yang sudah berjalan.

**Yang berubah:** dari 4 kode yang diusulkan menjadi **7 kode**. Tiga kode baru lahir dari
pemisahan yang seharusnya sudah ada sejak awal, bukan kebutuhan baru.

---

## 2. Tabel sebelum dan sesudah

| # | Kode pada `evidence/04` (lama) | Kode setelah koreksi | Perubahan |
|---|---|---|---|
| 1 | `SALDO-SUBLEDGER` | `SALDO-SUBLEDGER` | Tidak berubah |
| 2 | `PENERIMAAN-UANG-MUKA` | `PENERIMAAN-UANG-MUKA` | Tidak berubah |
| 3 | `PEMAKAIAN-UANG-MUKA-DEPOSIT` (pemakaian **dan** pengembalian jadi satu) | `PEMAKAIAN-UANG-MUKA-DEPOSIT` (**hanya** pemakaian) | **Dipersempit** — lihat bagian 3.1 |
| — | *(tidak ada)* | `PENGEMBALIAN-UANG-MUKA` (**baru**) | **Kode baru** — lihat bagian 3.2 |
| — | *(tidak ada)* | `PENGAKUAN-KELEBIHAN-BAYAR` (**baru**) | **Kode baru** — lihat bagian 3.3 |
| 4 | `SELISIH-KAS-SHIFT` | `SELISIH-KAS-SHIFT` | Pemicunya sekarang ditetapkan eksplisit — lihat bagian 3.4 |
| — | *(tidak ada)* | `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (**baru**) | **Kode baru** — lihat bagian 3.5 |

---

## 3. Penjelasan tiap perubahan

### 3.1 `PEMAKAIAN-UANG-MUKA-DEPOSIT` — dipersempit

**Sekarang khusus** untuk pelunasan piutang dari uang muka/deposit. Lawan jurnalnya **tetap**
seperti contoh yang sudah kami kirim: Debit Uang Muka Pasien, Kredit Piutang — tidak ada kas
yang bergerak sama sekali. Contoh JSON pada `evidence/04` bagian 4 untuk kode ini **tetap
berlaku apa adanya**, tidak perlu dibaca ulang.

**Pemicunya kini eksplisit:** baris `BilDepositMovement` bertipe `ALLOCATION` di Billing —
tempat fakta "uang muka dipakai" benar-benar tercatat pertama kali, lengkap dengan kunci
anti-ganda sendiri.

### 3.2 `PENGEMBALIAN-UANG-MUKA` — kode baru

Dipakai **khusus** saat Finance benar-benar mengembalikan uang tunai ke pasien. Lawan jurnal:
Debit Uang Muka Pasien, Kredit Kas.

**Pemicunya dua sumber:**
1. `BilDepositMovement` bertipe `RELEASE` — pengembalian deposit top-up yang tidak jadi dipakai.
2. `BilRefundCase` berstatus `EXECUTED`, **khusus** bila sumbernya `RefundableCredit.SourceType
   = ALLOCATION_EXCESS` (kelebihan bayar — lihat 3.3). Kategori refund Billing lain
   (`SETTLEMENT`, `REFERRED_OUTPATIENT_ADMIN`) **sengaja tidak kami masukkan** — sifat lawan
   jurnalnya belum kami teliti dan berpotensi bukan penarikan Uang Muka Pasien sama sekali
   (mungkin koreksi piutang/pendapatan). Bila Anda butuh cakupan itu juga, mohon beri tahu kami
   supaya kami gali dulu sebelum mengusulkan kodenya.

**Contoh pesan.** Pasien membatalkan sisa rawat inap, deposit Rp 3.000.000 yang belum terpakai
dikembalikan tunai.

```json
{
  "EventNumber": "EVT-2026-11-07734",
  "EventTypeCode": "PENGEMBALIAN-UANG-MUKA",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-RET-2026-11-00112",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-28T09:30:00+07:00",
  "AccountingDate": "2026-11-28",
  "Amount": 3000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "7d6c5b4a-3f2e-4d1c-9b8a-7f6e5d4c3b2a",
  "CausationId": "7d6c5b4a-3f2e-4d1c-9b8a-7f6e5d4c3b2a",
  "Components": "TOTAL"
}
```

### 3.3 `PENGAKUAN-KELEBIHAN-BAYAR` — kode baru

Kami tarik kembali satu kalimat pada surat lama yang menyatakan "sisi kelebihan terima memakai
`PENERIMAAN-UANG-MUKA`" — itu tidak mungkin secara waktu. Billing baru mengakui kelebihan bayar
**setelah** uang dialokasikan ke tagihan (`BilRefundableCredit`, `SourceType ALLOCATION_EXCESS`),
bukan pada detik uang diterima. Saat uang masuk, Finance belum tahu sebagiannya akan jadi
kelebihan.

**Kode ini terbit saat kelebihannya diakui**, memindahkan nilainya dari lawan jurnal penerimaan
asli (biasanya Piutang atau Pendapatan) ke Uang Muka Pasien — **tanpa** menyentuh kas, karena
kasnya sudah didebit penuh saat penerimaan pertama kali terjadi.

**Contoh.** Pasien membayar tunai Rp 500.000 untuk tagihan Rp 450.000. Rp 500.000 sudah masuk
sebagai penerimaan biasa. Setelah alokasi, Billing mengakui Rp 50.000 sebagai kelebihan bayar:

```json
{
  "EventNumber": "EVT-2026-11-08812",
  "EventTypeCode": "PENGAKUAN-KELEBIHAN-BAYAR",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-RFC-2026-11-00045",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-20T11:00:00+07:00",
  "AccountingDate": "2026-11-20",
  "Amount": 50000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "2a1b0c9d-8e7f-4a6b-9c5d-4e3f2a1b0c9d",
  "CausationId": "2a1b0c9d-8e7f-4a6b-9c5d-4e3f2a1b0c9d",
  "Components": "TOTAL"
}
```

Bila pasien kemudian minta Rp 50.000 itu dikembalikan tunai (bukan dipakai untuk tagihan lain),
Finance mengirim `PENGEMBALIAN-UANG-MUKA` (3.2) — karena saldo itu sekarang sudah berada di akun
Uang Muka Pasien, bukan lagi di akun asalnya.

**Kenapa tidak langsung dikoreksi di penerimaan aslinya saja.** Karena penerimaan asli mungkin
sudah jatuh di periode yang Anda tutup, dan mengoreksinya akan menabrak toleransi selisih nol
Anda persis seperti contoh Rp 20 juta pada surat Anda sebelumnya.

### 3.4 `SELISIH-KAS-SHIFT` — pemicu sekarang eksplisit

Kodenya tidak berubah, tapi sekarang kami tetapkan: kejadian terbit **saat selisihnya disahkan**
(`BilCashierShift.Status = REVIEWED`), bukan sekadar saat kas fisik dihitung. `AccountingDate`
memakai **tanggal shift**, bukan tanggal pengesahan, supaya selisihnya jatuh di periode
operasional yang benar.

**Satu hal yang perlu kita sepakati bersama:** bila pengesahan selisih terjadi setelah Anda
menutup periode shift-nya, kejadian tetap kami kirim, tapi kemungkinan tidak bisa Anda jurnalkan
ke periode yang sudah tertutup. Mohon masukan Anda soal batas waktu pengesahan yang wajar
sebelum tutup buku bulanan.

### 3.5 `PEMBALIKAN-PENERIMAAN-UANG-MUKA` — kode baru

Saat tender yang melahirkan `PENERIMAAN-UANG-MUKA` di-reverse Billing, pembalikannya **tidak**
lagi memakai `PEMBALIKAN-PENERIMAAN-KASIR` — itu akan mencampur lawan jurnal Uang Muka Pasien
dengan lawan jurnal `PENERIMAAN-KASIR` yang berbeda. Finance memilih kode pembalikan berdasarkan
kode penerimaan **aslinya**, bukan menghitung ulang status finalisasi tagihan saat pembalikan
terjadi (yang bisa saja sudah berubah).

`PEMBALIKAN-PENERIMAAN-KASIR` yang sudah ada **tidak berubah** dan tetap dipakai untuk
membalikkan `PENERIMAAN-KASIR`.

---

## 4. Yang tetap dari surat sebelumnya

Seluruh bagian lain `evidence/04` **tidak berubah**: ratifikasi `ACC-XMOD-0.3`, bentuk saldo
subledger (`SALDO-SUBLEDGER`), syarat akun layanan, penutupan konflik cutover,
`Components = TOTAL`, dan `JournalNumber` opsional pada balasan `201`. Kode
`PENERIMAAN-UANG-MUKA` juga tidak berubah — tetap seperti contoh pada `evidence/04` bagian 4.

---

## 5. Yang Finance butuhkan dari Accounting

| # | Butuh | Kapan dibutuhkan | Menahan apa |
|---|---|---|---|
| 1 | Ratifikasi atau koreksi nama dan lawan jurnal **tujuh** kode (bukan empat) — gabungan bagian 4 `evidence/04` dan bagian 3 surat ini | Sebelum Finance mengubah kode `FinanceReceiptService`/`FinanceAccountingOutboxService` | `FIN-DEC-030` dan turunannya tidak bisa dieksekusi di kode |
| 2 | Masukan soal batas waktu pengesahan selisih kas shift sebelum tutup buku (bagian 3.4) | Sebelum gerbang G6 dianggap tuntas | Kesepakatan operasional tutup bulan |
| 3 | Konfirmasi apakah refund Billing kategori `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` (di luar cakupan `PENGEMBALIAN-UANG-MUKA` saat ini) perlu kejadian tersendiri | Kapan saja, tidak mendesak | Cakupan lengkap gerbang G6 |

Mohon abaikan tabel kode pada `evidence/04` bagian 4 dan gunakan tabel bagian 2 surat ini sebagai
rujukan yang berlaku.

---

## 6. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | `FIN-DEC-040`..`044`, seluruhnya `approved` 25 September 2026, beserta owner dan bukti |
| `docs/module-blueprints/finance-management/evidence/04-jawaban-atas-balasan-accounting.md` | Surat yang dikoreksi berkas ini — bagian selain kode kejadian tetap berlaku |
| `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` | Surat asal yang dijawab kedua evidence Finance |
