# Pertanyaan untuk Owner Billing — Peristiwa Kas yang Belum Punya Penerbit

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Owner / penggarap modul Billing dan Kasir |
| Tanggal | 9 September 2026 |
| Sifat | Enam pertanyaan. **Tidak** meminta perubahan kode apa pun sekarang |
| Sumber lengkap | [`evidence/10-billing-arap-handoff-scan.md`](10-billing-arap-handoff-scan.md) bagian 11 dan 17 |

Berkas ini sengaja ringkas dan berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen
blueprint Accounting.

---

## 1. Kenapa kami bertanya

Modul Accounting sedang menyiapkan pembukuan otomatis: kejadian keuangan dari modul lain diterima,
lalu menjadi jurnal di buku besar.

Kami sudah memindai modul Billing dan menemukan **tiga jalur keluar** yang sudah berdiri:
`BIL-INT-007` piutang, `BIL-INT-008` jasa dokter, dan `BIL-INT-009` koreksi keduanya. Ketiganya
sudah dijawab pada kiriman pertama, dan **jawabannya sudah kami pakai** — terima kasih.

Yang belum terjawab: **sebelas peristiwa keuangan lain di Billing yang belum punya jalur keluar
sama sekali.** Seluruhnya berdampak pada buku besar.

### Akibatnya bila dibiarkan

Accounting hanya akan menerima sisi **pengakuan pendapatan**. Setiap pergerakan kas tidak terlihat,
sehingga di buku besar **piutang tumbuh selamanya dan kas tidak pernah bergerak** — dan empat akun
neraca tidak akan pernah ada isinya.

---

## 2. Sebelas peristiwa itu

Kolom "Jurnal seharusnya" adalah **usulan kami**, bukan keputusan. Silakan dikoreksi.

| # | Peristiwa | Entity Billing | Penanda | Jurnal seharusnya |
|---:|---|---|---|---|
| 1 | Pasien atau penjamin melunasi faktur | `BilSettlement` + `BilTender` | `Purpose = INVOICE_PAYMENT`, `Status = SETTLED` | **D** Kas/Bank · **K** Piutang |
| 2 | Pasien menyetor deposit | `BilSettlement` + `BilDepositMovement` | `Purpose = DEPOSIT_TOP_UP`, `MovementType = TOP_UP` | **D** Kas/Bank · **K** Utang Deposit Pasien |
| 3 | Deposit dipakai membayar faktur | `BilDepositMovement` | `MovementType = ALLOCATION` | **D** Utang Deposit · **K** Piutang |
| 4 | Sisa deposit dikembalikan | `BilDepositMovement` | `MovementType = RELEASE` | **D** Utang Deposit · **K** Kas |
| 5 | Pembayaran melebihi tagihan | `BilRefundableCredit` | `Status = AVAILABLE` | **D** Piutang · **K** Utang Kelebihan Bayar |
| 6 | Kelebihan bayar dikembalikan | `BilRefundCase` + `BilRefundLine` | `Status = EXECUTED` | **D** Utang Kelebihan Bayar · **K** Kas |
| 7 | Piutang dihapuskan | `BilWriteOffCase` | `Status = POSTED` | **D** Beban Piutang Tak Tertagih · **K** Piutang |
| 8 | Penyesuaian faktur sesudah final | `BilAdjustment` | `Status = POSTED`, `Direction` | Mengikuti arahnya |
| 9 | Selisih kas fisik saat tutup shift | `BilCashierShift` + `BilCashVarianceReview` | `Status = CLOSED_WITH_VARIANCE` lalu `REVIEWED` | Kurang: **D** Selisih Kas · **K** Kas. Lebih: kebalikannya |
| 10 | Pengeluaran kas kecil | `BilPettyCashVoucher` | Sudah terbayar | **D** Beban sesuai `CategoryId` · **K** Kas Kecil |
| 11 | Pengisian ulang kas kecil | `BilPettyCashBudgetMovement` | Penambahan | **D** Kas Kecil · **K** Kas |

**Catatan:** sepuluh dari sebelas peristiwa ini **tidak pernah sampai ke Finance**, karena tidak
ada handoff-nya. Jadi pertanyaannya berbeda dari kiriman pertama yang membahas jalur AR/AP.

---

## 3. Enam pertanyaan

| # | Pertanyaan | Kenapa penting bagi kami |
|---:|---|---|
| **7** | Dari sebelas peristiwa di atas, mana yang akan **Billing terbitkan sendiri**, mana lewat **Finance**, dan mana yang memang sengaja **tidak** dijurnal otomatis? | Menentukan siapa penerbitnya. Kiriman pertama sudah menetapkan Finance untuk AR/AP, tetapi sepuluh peristiwa ini tidak melewati Finance |
| **8** | Billing bersedia menambah handoff untuk peristiwa kas, atau lebih memilih Accounting yang membaca tabelnya? | Menentukan siapa membangun apa. Kami **tidak** akan membaca tabel Billing tanpa persetujuan Anda |
| **9** | Peristiwa kas dijurnal **per transaksi**, atau **diringkas per shift kasir**? | **Paling berdampak.** Per transaksi bisa puluhan ribu baris jurnal per bulan; per shift hanya beberapa baris per hari, penelusuran tetap terjaga lewat nomor shift |
| **10** | Selisih kas kasir dijurnal saat `CLOSED_WITH_VARIANCE`, atau menunggu `REVIEWED`? | Menentukan kapan selisih diakui sebagai beban |
| **11** | Penghapusan piutang diterbitkan **Billing** (yang punya alur persetujuannya) atau **Finance** (yang punya saldo piutangnya)? | Satu-satunya peristiwa yang pemiliknya benar-benar ambigu bagi kami |
| **12** | Kas kecil memang bagian dari pembukuan yang sama, atau dikelola terpisah di luar buku besar? | Bila terpisah, peristiwa 10 dan 11 gugur dan tidak perlu dibahas lagi |

---

## 4. Yang sudah disepakati, tidak perlu dibahas ulang

Dari kiriman pertama, 9 September 2026:

- Rantainya **Billing → Finance → Accounting**. Accounting **tidak** berlangganan langsung ke Billing.
- Finance menerbitkan **kejadian tersendiri**, bukan meneruskan `BilArHandoff`.
- `ACKNOWLEDGED` berarti **Finance sudah mencatat AR/AP** — bukan sudah dijurnal. Itu urusan
  Finance, bukan Accounting.
- Utang jasa dokter layak diakui pada **`READY` + `ReadyAt`**, bukan `CREATED`.
- `BIL-INTEGRATION-0.4` **tidak berubah** arah maupun bentuk dasarnya.

Kami juga sudah menindaklanjuti masukan Anda soal correlation/causation: kontrak pesan Accounting
kini mewajibkan `CorrelationId` dan `CausationId`, supaya penelusuran jurnal kembali ke faktur
Billing tidak terputus di Finance.

---

## 5. Dua hal yang kami jaga dari sisi kami

| Hal | Ketentuan kami |
|---|---|
| **Data pribadi** | Accounting **tidak menyimpan** nama pasien, nomor rekam medis, nomor kunjungan, maupun `DoctorId`. Kami hanya menyimpan modul asal dan nomor transaksi asal. `RecipientName` dan `Purpose` pada kas kecil yang Anda tandai SENSITIF juga **tidak** akan kami simpan |
| **Kas kecil terpisah** | Kami membaca komentar pada `BilPettyCashVoucher` bahwa kas kecil dan kas fisik shift kasir adalah dua uang berbeda (`PC-DEC-001`). Kami akan memakai **dua akun kas terpisah**, bukan satu |

---

## 6. Cara memverifikasi sendiri temuan kami

Seluruhnya read-only, dijalankan di `NewQuilvianSystemBackend`:

```bash
# Jalur keluar Billing ke keuangan hanya tiga?
grep -rn "Handoffs.Add\|StageHandoffs" --include="*.cs" Areas/

# Peristiwa kas menghasilkan handoff?
grep -rn "Handoff" --include="*.cs" Areas/HealthServices/BillingManagement/ \
  | grep -i "settlement\|deposit\|refund\|writeoff\|payment"
```

Perintah kedua menghasilkan **nol kecocokan** — itulah dasar seluruh pertanyaan di atas.

---

## 7. Yang tidak kami minta

Kami **tidak** meminta perubahan kode Billing sekarang, dan **tidak** meminta `BIL-INTEGRATION-0.4`
diubah. Yang kami butuhkan hanya **jawaban**, supaya rancangan Accounting dibuat sekali dan benar,
bukan dua kali.

Bila sebagian pertanyaan ternyata bukan wewenang Anda, cukup sebutkan siapa yang berwenang.
