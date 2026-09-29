# Usulan Empat Kode Kejadian Baru — Potongan Piutang, Retur Pembelian, dan Deposit Retur

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 26 September 2026 |
| Sifat | **Usulan kode kejadian baru**, bukan koreksi atas surat sebelumnya. Berdiri sendiri — tidak perlu membuka `evidence/04`, `05`, atau `06` lebih dulu |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-055`, `057`, `058`, `061`, `062`, seluruhnya `approved`. Arsitekturnya `02-backend-architecture.md` AMENDMENT REVISI 5 (`FIN-DES-045`..`050`), `approved` 26 September 2026 |
| Menutup | Sisi Finance dari `FIN-OQ-026`. Ratifikasi Anda yang masih ditunggu |
| Kontrak yang berlaku | `ACC-XMOD-0.3` sebagaimana sudah Anda ratifikasi. Bentuk amplop 12 field dipakai apa adanya, `Components = TOTAL` (`FIN-DEC-038`), tidak ada field baru |

---

## 1. Ringkasan satu paragraf

Finance mengusulkan **empat** kode kejadian baru, nomor ke-26 sampai ke-29 pada katalog (setelah
`PPN-MASUKAN-PEMBELIAN` di nomor 25, `evidence/06`). Dua untuk **potongan piutang** — PPh 23 dan
biaya administrasi bank yang dipotong penjamin saat membayar, beserta pembaliknya. Dua untuk
**retur pembelian** ke supplier — saat retur diakui, dan saat kredit retur itu dipakai untuk
melunasi utang supplier. Keempatnya punya satu kesamaan: **tidak ada kas yang bergerak**, sehingga
tidak boleh menumpang pada kode kas yang sudah ada.

Keempat kode diusulkan **sebelum** kodenya ditulis. Finance akan mulai menulis baris kotak keluar
berstatus `PENDING` sejak transaksinya terjadi, tetapi **worker pengiriman untuk keempat kode ini
tidak akan diaktifkan sebelum Anda meratifikasi** — pola yang sama dengan `PPN-MASUKAN-PEMBELIAN`.

---

## 2. Kode yang diusulkan

| # | Kode | Dipicu oleh | `SourceTransactionId` | `Amount` | Lawan jurnal (usulan) |
|---|---|---|---|---|---|
| 26 | `POTONGAN-PIUTANG-NON-TUNAI` | Potongan PPh 23 atau biaya admin bank dicatat bersama pelunasan piutang penjamin | Nomor potongan (`DED-…`) | Nilai potongan | Debit PPh 23 Dibayar di Muka (atau Beban Administrasi Bank), Kredit Piutang |
| 27 | `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` | Pelunasan yang membawa potongan dibalik — manual, atau otomatis karena tender Billing dibatalkan | Nomor potongan pembalik | Nilai potongan asli, **positif** | Kebalikan kode 26 |
| 28 | `RETUR-PEMBELIAN` | Retur barang ke supplier dikonfirmasi | Nomor retur (`RTR-…`) | Nilai retur | Debit Piutang Retur Supplier, Kredit Persediaan/Pembelian |
| 29 | `PEMAKAIAN-DEPOSIT-RETUR` | Pembayaran supplier yang sebagian/seluruhnya dilunasi dari kredit retur ditandai sudah dibayar | Nomor pembayaran (`PAY-…`) | Porsi yang dilunasi kredit retur | Debit Utang Supplier, Kredit Piutang Retur Supplier |

### 2.1 Kenapa potongan piutang butuh kode sendiri (kode 26, 27)

Penjamin sering membayar tagihan dikurangi PPh 23 dan biaya transfer. Contoh: piutang
Rp 10.000.000, yang masuk ke rekening RS Rp 9.745.000. Selisih Rp 255.000 **bukan** kekurangan
bayar — Rp 230.000 adalah PPh 23 yang dipotong penjamin (RS memegang bukti potongnya sebagai
kredit pajak), Rp 25.000 biaya bank. Keputusan owner Finance (`FIN-DEC-055`): piutang dinyatakan
**lunas penuh**.

Bila potongan itu dikirim dengan kode pelunasan piutang biasa, buku besar mencatat **kas masuk
Rp 10.000.000** padahal yang masuk Rp 9.745.000. Kode 26 memisahkan bagian non-tunainya.

**Kenapa pembaliknya kode tersendiri (kode 27), bukan kode 26 bernilai negatif.** Mengikuti pola
yang sudah Anda terima untuk penerimaan (`PEMBALIKAN-PENERIMAAN-KASIR`,
`PEMBALIKAN-PENERIMAAN-UANG-MUKA`): setiap kode punya pasangan pembalik, dan nilai selalu positif.
Dengan begitu `SELISIH-KAS-SHIFT` dan `SALDO-SUBLEDGER` tetap satu-satunya kode yang boleh
bernilai negatif, seperti yang sudah disepakati.

### 2.2 Kenapa retur butuh dua kode (kode 28, 29)

Retur pembelian menciptakan **hak tagih RS ke supplier** (kredit retur). Kredit itu tidak
dicairkan tunai, melainkan dipakai untuk melunasi utang ke supplier yang sama di pembayaran
berikutnya — bisa sebagian, bisa lintas beberapa faktur.

- **Kode 28** mencatat lahirnya hak tagih itu saat retur dikonfirmasi.
- **Kode 29** mencatat pemakaiannya saat pembayaran dilunasi.

**Satu perubahan pada kejadian yang sudah berjalan, yang perlu Anda ketahui.** Kejadian pembayaran
utang supplier hari ini bernilai **seluruh utang yang dilunasi**. Untuk pembayaran yang memakai
kredit retur, nilainya **turun sebesar porsi kredit retur**, dan porsi itu pindah ke kode 29.
Tanpa pemisahan ini, buku besar mencatat kas keluar untuk uang yang tidak pernah ditransfer.
Pembayaran yang **tidak** memakai kredit retur tidak berubah sama sekali.

---

## 3. Contoh pesan

### 3.1 Potongan piutang (kode 26)

Piutang PT Asuransi Contoh Rp 10.000.000; masuk Rp 9.745.000; PPh 23 Rp 230.000; biaya bank
Rp 25.000. Terbit **tiga** kejadian: pelunasan piutang Rp 9.745.000 (kode yang sudah berjalan),
dan dua kode 26. Salah satunya:

```json
{
  "EventNumber": "EVT-2026-11-18401",
  "EventTypeCode": "POTONGAN-PIUTANG-NON-TUNAI",
  "SourceModule": "Finance",
  "SourceTransactionId": "DED-2026-11-00031",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-20T10:05:00+07:00",
  "AccountingDate": "2026-11-20",
  "Amount": 230000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "7d6c5b4a-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "CausationId": "7d6c5b4a-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "Components": "TOTAL"
}
```

Biaya bank Rp 25.000 terbit sebagai kejadian kode 26 **terpisah** dengan `SourceTransactionId`
`DED-2026-11-00032`. Jenis potongannya (PPh 23 atau biaya bank) tidak ada di amplop — lihat
pertanyaan nomor 3 pada bagian 5.

### 3.2 Pembayaran supplier dengan kredit retur (kode 29)

Pembayaran Rp 10.000.000 ke PT Contoh Farma; Rp 2.500.000 dari kredit retur; yang ditransfer
Rp 7.500.000.

| Kode | `Amount` | Akibat di buku besar (usulan) |
|---|---|---|
| Pembayaran utang supplier (kode yang sudah berjalan) | Rp 7.500.000 | Debit Utang Supplier, Kredit Kas |
| `PEMAKAIAN-DEPOSIT-RETUR` | Rp 2.500.000 | Debit Utang Supplier, Kredit Piutang Retur Supplier |
| **Jumlah pengurang utang** | **Rp 10.000.000** | |

Bila seluruh pembayaran dilunasi kredit retur, **tidak ada** kejadian pembayaran utang — hanya
kode 29.

---

## 4. Yang belum kami putuskan, dan sengaja kami serahkan ke Anda

Kami **tidak** mengusulkan akun persisnya. Lawan jurnal pada bagian 2 adalah usulan awal untuk
memudahkan Anda menilai, bukan keputusan final — sama seperti seluruh kode Finance sebelumnya.

---

## 5. Yang Finance butuhkan dari Accounting

| # | Butuh | Menahan apa |
|---|---|---|
| 1 | Ratifikasi atau koreksi **nama** dan **lawan jurnal** keempat kode | Aktivasi worker pengiriman keempat kode. **Tidak** menahan pembangunannya di Finance |
| 2 | Konfirmasi urutan katalog 26–29, atau penomoran ulang bila ada kode modul lain yang masuk lebih dulu | Konsistensi katalog resmi |
| 3 | Apakah kode 26 cukup satu untuk PPh 23 **dan** biaya bank, atau Anda butuh dua kode karena akun debitnya berbeda | Bila dua kode: kami pecah kode 26 sebelum aktivasi, tanpa mengubah tabel |
| 4 | Satu hal yang **sudah ada sebelum surat ini**, kami sampaikan supaya tidak mengejutkan Anda saat pengiriman diaktifkan: sebagian kejadian yang sudah ditulis Finance hari ini memakai nama pendek (`AP_CREATED`, `AP_PAYMENT`, `AR_PAYMENT`, `AR_WRITEOFF`), bukan nama dari 17 kode yang Anda ratifikasi (`PENGAKUAN-HUTANG-SUPPLIER` dan seterusnya) | Tidak menahan apa pun sekarang — worker pengiriman belum hidup. Finance akan menyelaraskan namanya sebelum pengiriman diaktifkan |

---

## 6. Keadaan pekerjaan Finance saat ini

| Hal | Keadaan pada 26 September 2026 |
|---|---|
| Keputusan bisnis | `FIN-DEC-055`, `057`, `058`, `061`, `062` — seluruhnya `approved` |
| Rancangan arsitektur | `approved` 26 September 2026 (`FIN-DES-045`..`050`), kontrak dikunci |
| Kode keempat jenis kejadian | **Belum ditulis.** Masuk rencana kerja `BE-FIN-035`, `036`, `040` |
| Ratifikasi `PPN-MASUKAN-PEMBELIAN` (`evidence/06`) | Masih menunggu Anda (`FIN-OQ-020`) |
| Ratifikasi tujuh kode uang muka/deposit/kas (`evidence/05`) | Masih menunggu Anda (`FIN-OQ-017`) |

Kami sadar ini surat ketiga yang menunggu jawaban Anda. Bila lebih mudah, ketiganya dapat dijawab
dalam satu balasan — urutannya tidak penting bagi kami.

---

## 7. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | Keputusan dan `FIN-OQ-026` |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` bagian 5.9 | Tabel keempat kode beserta contoh berangka |
| `docs/module-blueprints/finance-management/02-backend-architecture.md` AMENDMENT REVISI 5 | Kapan tepatnya tiap kejadian ditulis |
| `docs/module-blueprints/finance-management/evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md` | Surat kode ke-25 — pola yang diikuti surat ini |
