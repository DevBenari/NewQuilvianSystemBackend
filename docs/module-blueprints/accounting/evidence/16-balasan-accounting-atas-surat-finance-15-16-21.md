# Balasan Accounting atas Surat Finance 15, 16, dan 21

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tanggal | 1 Oktober 2026 |
| Menjawab | `finance-management/evidence/15-balasan-finance-atas-ratifikasi-accounting.md` (bagian 7, enam butir), `16-permintaan-perluasan-validasi-kotak-masuk-accounting.md`, dan `21-balasan-finance-atas-aturan-saldo-dan-nomor-jurnal.md` |
| Sifat | **Ratifikasi, konfirmasi, dan lima pertanyaan balik** |
| Dasar keputusan | `docs/module-blueprints/accounting/00-interview-decisions.md` revision 17, keputusan `ACC-DEC-125` sampai `ACC-DEC-131`, seluruhnya `approved` sisi Accounting 1 Oktober 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.6` ([cross-module-contract.md](../contracts/cross-module-contract.md)). Bila berkas ini berbeda dari decision log Accounting, decision log yang berlaku |

Berkas ini berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Accounting yang lain.

---

## 1. Ringkasan satu paragraf

Terima kasih atas ketiga surat — terutama dua koreksi di bagian 6 surat 15, yang Anda temukan dengan
membaca source. Keenam butir yang Anda minta kami jawab: **dua kode jurnal baru diratifikasi**,
**koreksi pemicu penanda shift kami setujui**, **dua kode penanda kami terima** dengan bentuk pilihan
A, potongan jenis "lain-lain" **tidak** kami beri kode, perluasan `SETTLEMENT` kami setujui, dan
refund `REFERRED_OUTPATIENT_ADMIN` kami tahan untuk diputuskan bersama. Kesanggupan Anda di surat 21
kami terima. Ada **lima pertanyaan balik** di bagian 6 — empat di antaranya menyangkut pesan saldo
dan kas kasir, dan sebaiknya terjawab sebelum pengirim Anda dinyalakan.

---

## 2. Jawaban atas enam butir surat 15 bagian 7

| # | Yang Anda minta | Jawaban Accounting | Dasar |
|---:|---|---|---|
| 1 | Ratifikasi empat kode baru | **Dua kode jurnal diratifikasi** apa adanya: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (debit Piutang, kredit Uang Muka Pasien) dan `PPN-MASUKAN-RETUR-PEMBELIAN` (debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN`). Pengirim keduanya boleh Anda aktifkan bersama pengirim kode lain. **Dua kode penanda** — lihat butir 6 | `ACC-DEC-125` |
| 2 | Konfirmasi penanda shift terbit pada `CLOSED` **dan** `REVIEWED` | **Setuju, dan terima kasih atas koreksinya.** Kalimat "saat `REVIEWED`" di surat kami keliru. Tertutup final = `CLOSED` (kas pas) atau `REVIEWED` (selisih disahkan). `CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT` tidak menerbitkan penanda dan tetap menahan tutup bulan kami | `ACC-DEC-126` |
| 3 | Kode potongan piutang jenis "lain-lain", atau pernyataan dua jenis cukup | **Dua jenis cukup.** PPh 23 dan biaya administrasi bank; tidak ada kode ketiga. Penolakan Anda atas jenis "lain-lain" di validasi kami dukung. Bila kelak ada kasus nyata, mohon usulkan kode bernama dengan satu akun debit yang jelas | `ACC-DEC-128` |
| 4 | Akun debit refund `REFERRED_OUTPATIENT_ADMIN` | **Belum kami tetapkan.** Perlakuan sementara Anda — kegagalan yang terlihat, nol kejadian, tidak memakai `PENGEMBALIAN-UANG-MUKA` — kami terima dan menurut kami tepat. Akunnya diputuskan bersama owner Billing dan pemilik proses akuntansi kami, dan menjadi syarat gerbang G6 | `ACC-DEC-129` |
| 5 | Penilaian atas perluasan `SETTLEMENT` | **Setuju, tanpa kode baru.** `SETTLEMENT` masuk `PENGAKUAN-KELEBIHAN-BAYAR` lalu `PENGEMBALIAN-UANG-MUKA`. Ketiga syarat `PENGAKUAN-KELEBIHAN-BAYAR` tetap berlaku | `ACC-DEC-130` |
| 6 | Surat 16 — nilai nol untuk kedua kode penanda | **Pilihan A diterima**, dikerjakan sebagai bagian gerbang G6 — rinciannya bagian 4 | `ACC-DEC-127` |

**Tentang kredit retur tunai.** Pernyataan Anda bahwa supplier tidak pernah mencairkan kredit retur
secara tunai (`FIN-DEC-069`) kami catat; tidak ada kode debit Kas / kredit Piutang Retur Supplier.

---

## 3. Katalog sesudah surat ini

Ratifikasi tetap bukan aturan posting: kejadian berkode sah tersimpan **Tertahan** sampai aturan
postingnya kami susun di atas bagan akun rumah sakit yang sah (gerbang G2).

| Kelompok | Kode | Keadaan |
|---|---|---|
| Sudah di katalog (26 kode) | 17 kode pertama dan 9 kode tambahan | Tidak berubah, kecuali satu nama di bawah |
| Ganti nama | `PEMAKAIAN-DEPOSIT-RETUR` → **`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`** | Nama final Anda kami pakai (`FIN-DEC-066`) |
| Selisih kas — nama final | `SELISIH-KAS-KURANG`, `SELISIH-KAS-LEBIH` | Masuk katalog (`ACC-DEC-099`, `FIN-DEC-064`); keduanya bernilai positif |
| Potongan piutang — nama final | `POTONGAN-PPH23-PIUTANG`, `POTONGAN-BIAYA-BANK-PIUTANG`, `PEMBALIKAN-POTONGAN-PPH23-PIUTANG`, `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | Masuk katalog (`ACC-DEC-104`, `FIN-DEC-065`) |
| Kode jurnal baru | `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, `PPN-MASUKAN-RETUR-PEMBELIAN` | Diratifikasi (`ACC-DEC-125`) |
| Kode penanda | `PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Nama dan pemicu disepakati; **aktif sesudah G6** (`ACC-DEC-127`) |

Jumlahnya **34 kode aktif**, ditambah dua kode penanda yang menunggu G6. Nomor urut tetap tidak
mengikat; identitas kejadian adalah kodenya.

**Contoh — uang muka dibatalkan sesudah terpakai.** Uang muka Rp 20.000.000 dipakai melunasi tagihan,
lalu tendernya dibatalkan. Dua kejadian: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` Rp 20.000.000
(debit Piutang, kredit Uang Muka Pasien) dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` Rp 20.000.000 (debit
Uang Muka Pasien, kredit Kas). Bersihnya debit Piutang, kredit Kas; Uang Muka Pasien kembali nol.
Catatan Anda bahwa skenario ini belum dapat terjadi sampai Billing menutup gap-nya kami terima;
aturan postingnya tetap kami siapkan.

---

## 4. Jawaban atas surat 16 — nilai nol untuk kode penanda

**Pilihan A kami terima:** kotak masuk akan menerima `Amount = 0` **hanya** untuk daftar tertutup
kode penanda yang bernama, dan kedua kode itu tidak punya aturan posting sehingga tidak tertahan
`POSTING_RULE_MISSING`. Seluruh kode lain tetap ditolak seperti sekarang. Keputusan Anda menolak
nilai simbolis Rp 1 kami hargai; kami juga tidak menginginkannya di buku besar.

**Kapan dikerjakan.** Penegakan "shift kasir belum ditutup menahan tutup bulan" (`ACC-DEC-065`) sudah
kami pindahkan menjadi **syarat gerbang cutover G6** (`ACC-DEC-124`): ia wajib jadi sebelum Anda
mengirim kejadian sungguhan. Penerimaan nilai nol untuk penanda dikerjakan bersamanya, bukan
sekarang. Jadi untuk saat ini:

| Hal | Keadaan |
|---|---|
| Baris kotak keluar kedua kode penanda | Silakan terus ditulis sejak shift tertutup final |
| Pengirim kedua kode penanda | **Mohon tetap tidak diaktifkan** sampai kami menyatakan G6 siap |
| Bila terkirim hari ini | Ditolak `400` "Nilai kejadian harus lebih besar dari nol." |

**Satu pelurusan atas surat 16 bagian 2.** Aturan nomor 2 yang Anda baca — pesan saldo ditolak `409`
"jalurnya dibangun pada `BE-ACC-P2-028`" — **sudah tidak berlaku**: jalur pesan saldo selesai
dibangun 28 September 2026 dan `SALDO-SUBLEDGER` kini diterima. Pilihan B tetap tidak kami ambil,
karena jalur itu mewajibkan rincian periode dan kode akun control, yang tidak dimiliki penanda shift.

---

## 5. Tanggapan atas surat 21

Keempat kesanggupan Anda kami terima: saldo tiap periode termasuk `0.00`, nilai menurut saldo normal,
`AccountingDate` tanggal akhir periode dengan snapshot tanggal 1 pukul 00.05 WIB, serta
`AccountingEventId` sebagai rujukan tetap. Bahwa Finance tidak membutuhkan nomor jurnal terkini kami
catat — tidak ada jalur baca yang perlu kami sediakan.

Dua tafsir Anda di surat 15 bagian 9 **benar**: balasan `200` untuk kiriman ulang adalah sukses,
sama seperti `201`; dan `AccountingReceiptNumber` diisi dari `AccountingEventId`. `Components`
bernilai `null` memang kami terima.

Daftar kode akun control definitif kami kirim begitu bagan akun rumah sakit disahkan (G2), seperti
yang Anda tunggu. Sebelum itu ada tiga hal pada rencana snapshot Anda yang perlu kami tanyakan —
bagian 6 butir 2 sampai 4.

---

## 6. Yang Accounting butuhkan dari Finance

| # | Pertanyaan | Kenapa penting | Menahan |
|---:|---|---|---|
| 16.1 | **`PENERIMAAN-KASIR` per kuitansi atau per shift?** Keputusan kami bersama owner Billing (`ACC-DEC-062`) menetapkan kas kasir diringkas **satu kejadian per shift**, dengan `SourceTransactionId` nomor shift. Kami membaca `FinanceReceiptService` dan menemukan satu kejadian **per kuitansi**. Apakah itu disengaja? | Dengan 300 pembayaran sehari, per kuitansi berarti 300 jurnal sehari di buku besar; bila aturan postingnya Buat Draft, 300 draft harus disetujui satu per satu. `ACC-DEC-062` belum kami ubah; kami ingin memutuskannya bersama Anda | G4 |
| 16.2 | **Saldo per akun, bukan per kelompok.** Surat 21 menyebut empat baris saldo: Kas Kasir, Kas Kecil, Piutang, Utang Supplier. Pesan saldo menunjuk **kode akun** (`ControlAccountCode`), dan aturan kami meminta saldo untuk **setiap** akun yang ditandai control account. Bila bagan akun sah menandai lebih dari empat akun — misalnya piutang pasien pribadi dan piutang penjamin terpisah, atau beberapa akun utang supplier — sanggupkah snapshot Anda mengirim satu baris per akun? Dan **utang honor dokter**, yang juga calon control account, tidak ada di daftar snapshot Anda | Akun control tanpa saldo membuat periodenya tertahan "belum menerima saldo" | G2, G4 |
| 16.3 | **Saldo negatif.** Surat 21 menyatakan nilai negatif pada `SALDO-SUBLEDGER` ditolak tanpa pengecualian. Kontrak mengizinkan negatif untuk akun yang saldonya sedang tidak wajar — misalnya piutang yang lebih bayar. Bila itu terjadi, bagaimana Finance melaporkannya? | Tanpa saldo yang terkirim, periode kami tertahan dan tidak ada jalan pengecualian | G4 |
| 16.4 | **Shift yang masih terbuka saat snapshot.** Snapshot pukul 00.05 hanya menghitung shift yang sudah tertutup. Sesudah shift malam ditutup, apakah Finance otomatis menyatakan ulang saldo Kas Kasir dengan `SourceVersion` lebih tinggi? | Tanpa pernyataan ulang, saldo Kas Kasir berselisih dengan buku besar dan tutup bulan tertahan | G4 |
| 16.5 | **Shift yang belum pernah ditutup.** Penanda pembalik menjawab shift yang dibuka kembali. Tetapi untuk menyatakan "masih ada shift yang belum ditutup", kami perlu tahu shift mana yang **dibuka**. Apakah Finance dapat mengirim penanda pembukaan shift, atau menyatakan per hari daftar shift yang ada? | Rancangan penegakan `ACC-DEC-065` | G6 |

Jawaban boleh digabung dalam satu surat. Tidak satu pun menahan pembangunan di sisi Anda.

---

## 7. Keadaan gerbang cutover

| Gerbang | Isi | Keadaan 1 Oktober 2026 |
|---|---|---|
| G1 | Kotak masuk Accounting dibangun dan diuji | Development selesai dan sudah di integration; UAT belum |
| G2 | Bagan akun sah + aturan posting setiap kode aktif | Belum — data bagan akun rumah sakit sedang dikumpulkan |
| G3 | Akun layanan aktif, mekanismenya diputuskan | Mekanisme kredensial masih terbuka bersama Platform |
| G4 | Pengirim Finance siap | Perbaikan Anda berjalan; menunggu jawaban 16.1 sampai 16.4 |
| G5 | Saldo awal manual siap | Bergantung G2 |
| G6 | Deposit, kelebihan bayar, selisih kas, uang muka, refund, **dan penegakan shift kasir** | Kode disepakati kecuali refund `REFERRED_OUTPATIENT_ADMIN`; penegakan shift dan nilai nol penanda belum dibangun; menunggu jawaban 16.5 |

---

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/accounting/00-interview-decisions.md` bagian *Jawaban atas surat Finance 15, 16, dan 21* | `ACC-DEC-125` sampai `ACC-DEC-131` beserta alasan dan contohnya |
| `docs/module-blueprints/accounting/contracts/cross-module-contract.md` bagian 3a, 8a, dan 13 | Katalog kode, aturan pesan saldo, dan gerbang cutover |
| `docs/module-blueprints/accounting/evidence/14` dan `15` | Dua surat kami sebelumnya |
