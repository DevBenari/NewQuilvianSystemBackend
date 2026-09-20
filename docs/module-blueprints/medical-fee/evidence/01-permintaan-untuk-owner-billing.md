# Permintaan Medical Fee kepada Owner Billing

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner modul Medical Fee |
| Untuk | Owner modul Billing dan Kasir (`BIL-CASH-001`) |
| Tanggal | 20 September 2026 |
| Sifat | **Dua permintaan perubahan beserta alasannya.** Belum meminta Billing menulis kode sekarang |
| Menutup | `MF-CQ-08` (bagian 2) dan `MF-CQ-05` (bagian 3) |
| Dasar | `docs/module-blueprints/medical-fee/00-interview-decisions.md`, keputusan `MF-DEC-001` dan `MF-DEC-017`, `approved` 20 September 2026 |
| Yang memblokir | Permintaan pada bagian 2 menahan **seluruh** jalur nilai jasa; bagian 3 hanya menahan rumpun entri kasir |

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Medical Fee.

---

## 1. Mengapa Medical Fee menghubungi Billing

Rumah sakit sedang membangun modul Medical Fee untuk menghitung jasa tenaga medis. Saat ini
perhitungan itu dikerjakan manual: menurut transcript meeting 15 Juli 2026, prosesnya menyentuh
±220 dokter per bulan dan memakan ±4 hari kerja, dengan risiko yang diakui sendiri oleh tim —
dokter dobel dibayar atau justru terlewat, karena rekonsiliasinya lewat Excel di luar sistem.

Dua hal di Billing menjadi kunci, dan keduanya perlu persetujuan Anda.

---

## 2. Permintaan pertama — siapa yang menentukan `DoctorShare`

**Ini permintaan yang paling besar dampaknya, dan yang paling perlu Anda timbang.**

### 2.1 Keadaan sekarang

Diverifikasi langsung dari source pada commit `09101d05`:

| Fakta | Bukti |
|---|---|
| `BilInvoiceItem.DoctorShare` diisi dari isian permintaan, bukan dari aturan perhitungan | `BillingInvoiceService.cs:1855` — `item.DoctorShare = request.DoctorShare` |
| Satu-satunya batas yang berlaku: tidak boleh melebihi nilai kotor baris | `BillingInvoiceService.cs:1880-1881` |
| Nilai itulah yang mengalir ke Finance saat tagihan difinalisasi | `BillingArApHandoffService.cs:97-105` |

Artinya angka yang menentukan penghasilan seorang dokter hari ini bergantung pada ketelitian
pengetikan petugas, dan tidak ada aturan yang memeriksanya selain "jangan melebihi nilai baris".

### 2.2 Yang diminta

`MF-DEC-001`: **Medical Fee menjadi sumber angka itu.** Aturan perhitungan hidup di Medical Fee,
dan `DoctorShare` diisi dari hasil perhitungannya — bukan lagi diketik petugas.

Contoh konkret: tarif tindakan Rp 1.000.000 dengan tarif sharing dokter 40% menghasilkan
`DoctorShare` Rp 400.000 secara otomatis, berdasarkan kesepakatan tarif yang tercatat pada
kontrak dokter yang bersangkutan.

### 2.3 Yang TIDAK berubah

Supaya jelas batasnya:

| Tetap milik Billing | Keterangan |
|---|---|
| Perhitungan tagihan pasien | Medical Fee tidak pernah menyentuhnya |
| Diskon dan persetujuannya | Termasuk diskon dokter beserta alur persetujuan oleh dokter yang bersangkutan |
| Finalisasi tagihan | Tidak berubah |
| Bentuk `BilInvoiceItem` selain `DoctorShare` | Tidak disentuh |
| Nilai yang mengalir ke Finance | Tetap dihitung Billing dengan rumus yang sama |

Medical Fee juga **tidak** mengubah perlakuan diskon. Audit kami menemukan bahwa diskon promo
yang diberikan ke pasien tidak mengurangi bagian dokter, dan hanya diskon bertipe dokter yang
menguranginya — dan Medical Fee mengikuti perilaku itu apa adanya (`MF-DEC-013`).

### 2.4 Pertanyaan untuk Anda

| # | Pertanyaan |
|---:|---|
| 1 | Apakah Billing setuju `DoctorShare` tidak lagi diketik petugas, melainkan diisi dari hasil perhitungan Medical Fee? |
| 2 | Bila setuju, bagaimana bentuk teknis yang Billing inginkan — Medical Fee memanggil Billing saat nilai siap, Billing menarik dari Medical Fee saat item dicatat, atau bentuk lain? |
| 3 | Apa yang sebaiknya terjadi bila Medical Fee **belum punya aturan tarif** untuk layanan tertentu — `DoctorShare` diisi nol, dibiarkan kosong, atau tetap boleh diketik sebagai pengecualian? |

Pertanyaan ketiga penting: selama masa peralihan pasti ada layanan yang aturannya belum
terdaftar, dan perilaku untuk kasus itu menentukan apakah tagihan tetap bisa difinalisasi.

---

## 3. Permintaan kedua — pelaksana pada entri bebas kasir

### 3.1 Keadaan sekarang

Kontrak charge Billing (`BIL-INTEGRATION-0.4`) mengenal tujuh sumber: `PROCEDURE`,
`LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSUMABLE`, `ADHOC`, dan `ADHOC_CATALOG`.

Lima sumber pertama berasal dari modul klinis, yang menyimpan sendiri siapa yang mengerjakan.
Dua terakhir — entri bebas kasir dan entri dari katalog tarif — **tidak menyimpan pelaksana
sama sekali**, karena memang diketik langsung di loket.

### 3.2 Yang diminta

`MF-DEC-017`: entri bebas kasir **boleh** menghasilkan jasa tenaga medis, **dengan syarat kasir
menunjuk pelaksananya saat input**.

Yang dibutuhkan: satu rujukan tenaga medis pada entri `ADHOC` dan `ADHOC_CATALOG`.

| Bidang | Tipe | Wajib | Kegunaan |
|---|---|---|---|
| Rujukan tenaga medis pelaksana | `Guid` | Wajib bila layanannya berjasa | Menentukan siapa yang berhak atas jasa |
| Peran pelaksana | `string` | Opsional untuk entri kasir | Bila kosong, dianggap peran utama |

Nama bidang dan bentuknya terserah Billing.

### 3.3 Mengapa tidak dibiarkan tanpa pelaksana

Alternatifnya adalah entri bebas kasir tidak pernah menghasilkan jasa. Owner Medical Fee memilih
tidak menempuh itu, karena layanan berjasa yang terlanjur diinput lewat entri bebas akan
kehilangan jasanya tanpa jejak — persis jenis kehilangan yang modul ini justru ingin hentikan.

Risiko yang diterima sudah dicatat: data yang berujung pada penghasilan orang menjadi bergantung
pada ketelitian kasir.

### 3.4 Pertanyaan untuk Anda

| # | Pertanyaan |
|---:|---|
| 4 | Apakah Billing bersedia menambahkan rujukan pelaksana pada entri `ADHOC` dan `ADHOC_CATALOG`? |
| 5 | Apakah pengisiannya wajib untuk seluruh entri bebas, atau hanya bila kasir menandai layanan itu berjasa? |

---

## 4. Dampak bila permintaan ini belum turun

| Permintaan | Yang tertahan | Yang tetap jalan |
|---|---|---|
| Bagian 2 — sumber `DoctorShare` | **Seluruh jalur nilai jasa kembali ke Billing.** Medical Fee dapat menghitung, tetapi hasilnya belum mengalir ke tagihan | Aturan tarif sharing, perhitungan jasa, verifikasi, persetujuan, periode, dan penyerahan ke Finance |
| Bagian 3 — pelaksana entri kasir | Rumpun jasa dari entri bebas kasir | Seluruh rumpun lain, termasuk jasa dari operasi dan tindakan klinis |

Kedua rumpun yang tertahan sudah ditandai `OPEN DECISION` pada rencana Medical Fee dan
dikeluarkan dari gelombang pengerjaan, sehingga tidak ada pekerjaan yang dimulai lalu dibongkar.

---

## 5. Yang Medical Fee janjikan

| Hal | Janji |
|---|---|
| Tabel Billing | Medical Fee tidak pernah menulis langsung ke tabel Billing mana pun |
| Perhitungan tagihan | Tidak pernah disentuh |
| Diskon | Perlakuan diskon mengikuti perilaku Billing yang sudah ada, tidak diubah |
| Nilai ke Finance | Tetap dihitung Billing; Medical Fee hanya menentukan komponen jasanya |
| Aturan tarif | Tercatat lengkap beserta kontrak sumbernya, sehingga angka yang muncul selalu dapat dijelaskan asalnya |

---

## 6. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/medical-fee/00-interview-decisions.md` | 17 keputusan bisnis beserta owner dan tanggalnya |
| `docs/module-blueprints/medical-fee/01-existing-capability-map.md` | Bukti source untuk seluruh klaim pada berkas ini |
| `docs/module-blueprints/medical-fee/contracts/integration-contract.md` | Bentuk kontrak lengkap |
| `Referensi_Meeting_FIN-OQ_dan_Medical_Fee.pdf` | Rangkuman transcript meeting RS MMC |
