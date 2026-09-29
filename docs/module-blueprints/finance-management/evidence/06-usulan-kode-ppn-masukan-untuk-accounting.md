# Usulan Kode Kejadian Baru — PPN Masukan Pembelian

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 25 September 2026 |
| Sifat | **Usulan kode kejadian baru, bukan koreksi atas surat sebelumnya.** Ini permintaan terpisah dari `evidence/04`/`05` — dipicu rumpun kapabilitas baru (Purchasing/AP) yang sedang dirancang Finance, bukan balasan atas surat Anda |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`, keputusan `FIN-DEC-045`, `FIN-DEC-046`, `FIN-DEC-053`, seluruhnya `approved` 25 September 2026 |
| Menutup | Sisi Finance dari `FIN-OQ-020`. Ratifikasi Anda yang masih ditunggu |
| Kontrak yang berlaku | `ACC-XMOD-0.3` sebagaimana sudah Anda ratifikasi. Bentuk amplop 12 field dipakai apa adanya, tidak ada field baru |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka `evidence/04`/`05` lebih
dulu.

---

## 1. Ringkasan satu paragraf

Finance sedang merancang rumpun kapabilitas baru — siklus Purchasing/AP penuh (Purchase Order,
Tukar Faktur, Purchasing Invoice, Retur Pembelian) — yang sebelumnya tidak ada satu barisnya
pun di sistem. Purchasing Invoice akan mencatat PPN Masukan (Pajak Masukan) dari pembelian ke
supplier. Kami sengaja mengusulkan kodenya **sejak sebelum kode ditulis**, bukan menyusulkan
setelah rumpun ini berjalan — kami sudah menetapkan sebagai syarat internal bahwa rumpun ini
tidak boleh mulai dibangun sebelum kode PPN Masukan diratifikasi, persis pola yang sudah kami
jalankan untuk kode uang muka/deposit sebelumnya.

**Satu kode diusulkan:** `PPN-MASUKAN-PEMBELIAN`, kode ke-25 pada katalog (setelah tujuh kode
`evidence/04`/`05` yang menempati posisi 18-24).

---

## 2. Kode yang diusulkan

| # | Kode | Dipicu oleh | Lawan jurnal (usulan) | Dasar |
|---|---|---|---|---|
| 25 | `PPN-MASUKAN-PEMBELIAN` | Purchasing Invoice disetujui dan dicatat sebagai utang supplier | Debit PPN Masukan (aset — kredit pajak), Kredit Utang Supplier (bagian PPN dari nilai invoice) | `FIN-DEC-046` |

**Kenapa Finance mengusulkan ini sebagai kejadian terpisah, bukan menumpangkannya pada kejadian
utang supplier yang sudah ada.** PPN Masukan bukan bagian dari nilai barang/jasa yang dibeli —
ia adalah kredit pajak yang mengurangi kewajiban PPN Keluaran RS ke negara pada periode SPT
Masa yang sama. Menggabungkannya ke dalam satu kode "pengakuan utang supplier" akan membuat
Anda tidak bisa memisahkan bagian mana dari jurnal itu yang sebenarnya milik akun PPN Masukan.

**Nama yang kami pilih.** "PEMBELIAN" ditambahkan secara eksplisit di akhir kode untuk
membedakannya dari PPN Keluaran, bila kelak modul lain (mis. penjualan farmasi ke pihak luar,
bila ada) membutuhkan kode PPN Keluaran sendiri — mengikuti pola penamaan kode Anda yang selalu
menyebut konteks pemicunya (`PENGEMBALIAN-UANG-MUKA`, `SELISIH-KAS-SHIFT`).

### Contoh pesan

Purchasing Invoice dari supplier senilai Rp 11.000.000 (harga barang Rp 10.000.000, PPN 11%
Rp 1.100.000, dibulatkan menjadi Rp 1.100.000 untuk contoh ini).

```json
{
  "EventNumber": "EVT-2026-11-15234",
  "EventTypeCode": "PPN-MASUKAN-PEMBELIAN",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-PINV-2026-11-00312",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-15T13:20:00+07:00",
  "AccountingDate": "2026-11-15",
  "Amount": 1100000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "4b3a2c1d-0e9f-4a8b-9c7d-6e5f4a3b2c1d",
  "CausationId": "4b3a2c1d-0e9f-4a8b-9c7d-6e5f4a3b2c1d",
  "Components": "TOTAL"
}
```

`Amount` pada kejadian ini adalah **nilai PPN-nya saja** (Rp 1.100.000), bukan nilai invoice
penuh (Rp 11.000.000) — nilai pokok barang/jasa dicatat lewat kejadian pengakuan utang supplier
yang terpisah (kode belum diusulkan, menyusul saat rumpun Purchasing/AP dirancang penuh lewat
`/design-business-module`). `Components = TOTAL`, konsisten dengan seluruh kode Finance yang
sudah berjalan (`FIN-DEC-038`) — belum ada pemecahan komponen pada rilis ini.

---

## 3. Yang belum kami putuskan, dan sengaja kami serahkan ke Anda

Kami **tidak** mengusulkan akun PPN Masukan yang persis, maupun apakah kredit pajak ini
langsung mengurangi kewajiban PPN Keluaran bulan berjalan atau menunggu rekonsiliasi SPT Masa —
itu wewenang Accounting, konsisten dengan bagaimana Anda menetapkan lawan jurnal untuk seluruh
kode Finance sebelumnya. Lawan jurnal pada tabel bagian 2 adalah usulan awal kami untuk
memudahkan Anda menilai, bukan keputusan final.

---

## 4. Keadaan pekerjaan Finance saat ini

| Hal | Keadaan pada 25 September 2026 |
|---|---|
| Keputusan bisnis rumpun Purchasing/AP | Tujuh keputusan (`FIN-DEC-045`..`051`), seluruhnya `approved`, plus empat keputusan penutup (`FIN-DEC-052`..`055`) |
| Kode `PPN-MASUKAN-PEMBELIAN` | **Diusulkan, belum diratifikasi** — Finance belum menulis satu baris kode pun untuk rumpun Purchasing/AP |
| Audit kemampuan existing | Belum dijalankan — `/trace-existing-capabilities` menyusul setelah surat ini terkirim |
| Rancangan arsitektur (entity, endpoint) | Belum dimulai — menunggu audit kemampuan, lalu `/design-business-module` |

**Ratifikasi Anda atas kode ini adalah syarat keras**, bukan syarat yang boleh menyusul setelah
rumpun Purchasing/AP mulai dibangun — ini keputusan eksplisit owner Finance (`FIN-DEC-046`),
berbeda dari pola kode kejadian lain di blueprint ini yang boleh diusulkan bertahap.

---

## 5. Yang Finance butuhkan dari Accounting

| # | Butuh | Kapan dibutuhkan | Menahan apa |
|---|---|---|---|
| 1 | Ratifikasi atau koreksi nama dan lawan jurnal `PPN-MASUKAN-PEMBELIAN` | Sebelum rumpun Purchasing/AP masuk `/plan-module-delivery` | `FIN-DEC-046` — seluruh EPIC Purchasing/AP tertahan |
| 2 | Konfirmasi urutan kode (apakah tetap ke-25, atau berubah bila ada kode lain yang diusulkan modul lain di antara waktu ini) | Sebelum katalog difinalisasi | Konsistensi urutan katalog resmi |

---

## 6. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | `FIN-DEC-045`, `046`, `053` beserta owner dan bukti; `FIN-OQ-020` |
| `docs/module-blueprints/finance-management/Keuangan.md` | Evidence pemicu rumpun Purchasing/AP — bagian 6 (Set Purchasing Invoice: PPN %/nominal, COA PPN) |
| `docs/module-blueprints/finance-management/evidence/04-jawaban-atas-balasan-accounting.md`, `05-koreksi-jawaban-atas-balasan-accounting.md` | Surat sebelumnya — pola penamaan dan bentuk pesan yang diikuti berkas ini |
