# Balasan Accounting atas Jawaban Finance — Kejadian Keuangan

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tanggal | 24 September 2026 |
| Menjawab | `docs/module-blueprints/finance-management/evidence/01-jawaban-untuk-owner-accounting.md` (20 September 2026) |
| Sifat | **Ratifikasi balik, jawaban atas `FIN-OQ-011`, dan lima permintaan.** Kotak keluar yang sudah berdiri tetap cocok. Satu permintaan (bagian 5 butir 5) menyentuh perilaku penahanan `HELD_FOR_FINALIZATION`, sehingga bila Finance menyetujuinya, kodenya ikut berubah |
| Dasar keputusan | `docs/module-blueprints/accounting/00-interview-decisions.md`, keputusan `ACC-DEC-082` sampai `ACC-DEC-091`, seluruhnya `approved` 24 September 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.3` ([cross-module-contract.md](../contracts/cross-module-contract.md)) — bila berkas ini berbeda, kontrak itu yang berlaku |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint
Accounting yang lain.

---

## 1. Ringkasan satu paragraf

Terima kasih atas ratifikasinya. Accounting **menutup `ACC-XM-001`**: rantai Billing → Finance →
Accounting dan bentuk pesan dua belas bidang kini berlaku sebagai kontrak dua pihak
(`ACC-XMOD-0.3`, `approved`). **Ketujuh belas kode Anda diratifikasi apa adanya.** Kotak keluar
yang sudah Anda bangun tetap cocok — **nol perubahan** pada bidang, tipe, kunci anti-ganda, dan
kode balasan. Ada lima hal yang kami butuhkan dari Finance (bagian 5), dan satu hal yang kami
nyatakan terus terang: **cutover 1 Oktober 2026 tidak layak** dari sisi Accounting (bagian 4).

---

## 2. Yang Accounting putuskan atas jawaban Anda

| Pertanyaan Anda / butir | Keputusan Accounting | Dasar |
|---|---|---|
| Ratifikasi rantai dan bentuk pesan | **Diterima.** `ACC-XM-001` ditutup; `ACC-XMOD` menjadi `approved` | `ACC-DEC-082` |
| Tujuh belas kode | **Diratifikasi apa adanya.** Kode baru hanya lewat keputusan kedua pihak | `ACC-DEC-083` |
| `JASA_MEDIS` tidak ikut `PENGAKUAN-PIUTANG` | **Diterima.** Contoh di seluruh kontrak Accounting sudah dibetulkan; aturan posting piutang tidak boleh memuat `JASA_MEDIS` | `ACC-DEC-086` |
| Usulan bidang saldo subledger (`FIN-DEC-023`) | **Diubah** — lihat bagian 3.3 | `ACC-DEC-087` |
| Autentikasi akun layanan (`FIN-DEC-007`) | Accounting menetapkan **syarat**; **mekanismenya tetap terbuka** bersama Platform | `ACC-DEC-088` |
| Cutover (`FIN-DEC-008`) | **Prinsipnya diterima** (sejak go-live, tidak retroaktif, saldo awal manual). Tanggalnya ditentukan enam gerbang | `ACC-DEC-089`, `090` |
| Tawaran menerbitkan penerimaan sebelum tagihan final sebagai uang muka (`FIN-DEC-004`) | **Diterima — Accounting memintanya.** Lihat bagian 5 butir 5 | `ACC-DEC-091` |

---

## 3. Yang perlu Finance ketahui untuk pengirimnya

### 3.1 Balasan yang akan Anda terima

Balasan `201`, `200`, dan `422` membawa **tanda terima** yang sama bentuknya:

| Bidang | Tipe | Selalu terisi | Simpan di kolom Finance |
|---|---|:---:|---|
| `AccountingEventId` | `Guid` | Ya | `AccountingReceiptNumber` |
| `EventNumber` | `string` | Ya | — (gema dari pesan Anda) |
| `EventStatus` | `string` | Ya | — (`Diterima`, `Terjurnal`, `Tertahan`, atau `Tercatat` untuk pesan saldo) |
| `JournalNumber` | `string?`, maks 30 | Tidak | `AccountingJournalNumber` |
| `AccountingPeriodCode` | `string?`, `YYYY-MM` | Tidak | — |
| `HoldReasonCode` | `string?` | Tidak | Dapat mengisi `HoldReason` |
| `ReceivedAt` | tanggal-waktu berzona | Ya | — |

Kedua kolom Anda (`string?`, maks 50) sudah cukup. `AccountingEventId` berupa Guid 36 karakter.

`HoldReasonCode` bernilai `EVENT_TYPE_NOT_REGISTERED`, `POSTING_RULE_MISSING`,
`COMPONENT_UNMAPPED` (pesan membawa komponen yang tidak dipakai aturan), atau `COMPONENT_MISSING`
(aturan menuntut komponen yang tidak dibawa pesan). Keempatnya berarti **Tertahan**: Finance
menandai `HELD` dan **tidak** mengirim kejadian baru, persis seperti kontrak Anda bagian 5.3.

Balasan `400`, `403`, `409`, dan `422` karena badan hukum tidak ditemukan **tidak** membawa tanda
terima, karena kejadiannya tidak tersimpan di Accounting.

### 3.2 Nomor jurnal kadang kosong pada `201`

Accounting menjurnal **seketika di dalam request** (`ACC-DEC-084`). Tetapi bila gangguan teknis
terjadi **setelah** kejadian Anda tersimpan, kami tetap menjawab `201` — kejadian Anda aman — dengan
`JournalNumber` kosong, lalu mesin kami mencoba ulang sendiri.

**Contoh.** `EVT-300` dijawab `201`, `EventStatus` `Diterima`, `JournalNumber` kosong. Dua menit
kemudian jurnal `JU/2026/11/00017` terbentuk di Accounting. Accounting **tidak** mengabari Finance,
karena Accounting tidak pernah menerbitkan kejadian balik. Bila Finance ingin nomor itu, kirim ulang
`EVT-300`: jawabannya `200` beserta nomor jurnalnya, tanpa jurnal baru.

**Akibat bagi kontrak Anda bagian 5.3** baris `201` ("tandai `ACKNOWLEDGED`, simpan nomor jurnal"):
mohon diperlakukan sebagai **"simpan nomor jurnal bila ada"**. `ACKNOWLEDGED` tetap benar.

### 3.3 Bentuk saldo subledger — jawaban atas `FIN-OQ-011`

Usulan lima bidang berdiri sendiri **tidak** kami pakai. Saldo dikirim lewat **amplop dua belas
bidang yang sama**, ditambah satu objek rincian. Dengan begitu Anda mendapat anti-ganda, koreksi
lewat `SourceVersion`, dan penelusuran tanpa aturan baru.

| Usulan Finance | Menjadi | Alasan |
|---|---|---|
| `LegalEntityId` | `LegalEntityId` di amplop | Sudah ada |
| `AccountingPeriodCode` `string(20)` | `SubledgerBalance.AccountingPeriodCode`, **`string` maks 7**, bentuk `YYYY-MM` | Kode periode Accounting memang 7 karakter, contoh `2026-11` |
| `ControlAccountCode` `string(50)` | `SubledgerBalance.ControlAccountCode`, `string` maks 50 | Tetap |
| `SubledgerBalance` `decimal(18,2)` | **`Amount`** di amplop — khusus pesan saldo boleh **nol atau negatif** | Menghindari dua bidang bernilai sama yang bisa saling bertentangan |
| `AsOfDate` `date` | **`AccountingDate`** di amplop, berarti tanggal cut-off | Sama alasannya |

```json
{
  "EventNumber": "EVT-SL-2026-11-001",
  "EventTypeCode": "SALDO-SUBLEDGER",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-CLOSE-2026-11",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-12-01T08:00:00+07:00",
  "AccountingDate": "2026-11-30",
  "Amount": 425000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "3e1d2c0b-9a8f-4e7d-8c6b-5a4f3e2d1c0b",
  "CausationId": "3e1d2c0b-9a8f-4e7d-8c6b-5a4f3e2d1c0b",
  "SubledgerBalance": {
    "AccountingPeriodCode": "2026-11",
    "ControlAccountCode": "1-1201"
  }
}
```

Pesan saldo **tidak pernah** menjadi jurnal; ia hanya dicocokkan dengan buku besar saat tutup
bulan. Satu saldo per akun kontrol per periode. Bila Finance menyatakan ulang saldo periode yang
sama, naikkan `SourceVersion`. **Khusus pesan saldo, `SourceVersion` wajib bilangan bulat positif**
(`1`, `2`, `3`, …), karena versi tertinggi yang berlaku; pesan berversi lebih rendah yang datang
terlambat tidak menimpa koreksi. Pesan saldo untuk periode yang sudah ditutup Accounting diterima
tetapi tidak mengubah angka periode itu.

`SALDO-SUBLEDGER` adalah **usulan kode ke-18** — butuh persetujuan Anda (bagian 5).

### 3.4 Syarat akun layanan

Mekanismenya belum kami putuskan, karena itu wewenang Platform bersama kita berdua. Yang sudah
pasti dari sisi Accounting, apa pun mekanismenya:

1. Satu akun khusus untuk Finance — bukan akun manusia, **bukan SuperAdmin**.
2. Akun itu **wajib** punya penugasan Departemen + Jabatan khusus (misalnya "Integrasi Sistem")
   yang hanya diberi hak `AccountingEvent : Receive`. **Tanpa penugasan organisasi, setiap kiriman
   pasti `403`**, karena hak Accounting diberikan per Departemen + Jabatan.
3. Akun itu berhak atas badan hukum yang dikirimi kejadian.
4. Token berumur pendek adalah preferensi kami, bukan syarat.

---

## 4. Tanggal cutover

Kami sepakat dengan prinsip `FIN-DEC-008`. Tanggalnya kami ikat pada **enam gerbang**, dan cutover
jatuh pada tanggal 1 pukul 00.00 WIB di awal periode akuntansi pertama setelah keenamnya lolos.

| Gerbang | Syarat | Pemilik | Keadaan 24 September 2026 |
|---|---|---|---|
| G1 | Kotak masuk Accounting dibangun dan diuji | Rizki | Belum ada kodenya; akan direncanakan segera |
| G2 | Bagan akun rumah sakit yang sah tersedia, dan aturan posting untuk setiap kode yang aktif tersusun | Pemilik proses akuntansi + Rizki | Belum — bagan akun saat ini data pengembangan |
| G3 | Akun layanan aktif, mekanismenya sudah diputuskan | Platform + Yasmin + Rizki | Mekanisme terbuka |
| G4 | Pengirim Finance siap | Yasmin | Belum dibangun |
| G5 | Saldo awal manual per tanggal cutover siap diinput | Rizki | Bergantung G2 |
| G6 | Kejelasan deposit pasien, kelebihan bayar, dan selisih kas shift, serta perubahan `FIN-DEC-004` (bagian 5 butir 3 dan 5) | Yasmin + owner Billing | Terbuka |

**Karena itu 1 Oktober 2026 tidak layak** — empat gerbang belum terpenuhi tujuh hari sebelumnya.
Tanggal pastinya kita putuskan bersama saat gerbang terakhir lolos.

---

## 5. Yang Accounting butuhkan dari Finance

| # | Butuh | Kapan dibutuhkan | Menahan apa |
|---:|---|---|---|
| 1 | **Persetujuan kode `SALDO-SUBLEDGER`** dan penyesuaian `FIN-DEC-023` ke bentuk bagian 3.3 | Sebelum fitur tutup periode Finance dibangun | Rekonsiliasi tutup bulan Accounting |
| 2 | **Daftar komponen per jenis kejadian** — jenis mana yang membawa `Components`, dengan kode apa. Bila tidak ada satu pun, cukup katakan "seluruhnya `TOTAL`" | Sebelum aturan posting disusun | Aturan posting berbaris banyak |
| 3 | **Deposit pasien** (top-up, pemakaian, pengembalian), **kelebihan bayar dan pengembaliannya**, dan **selisih kas shift kasir**: kode kejadiannya, **atau** pernyataan tertulis bagaimana ketiganya tercermin pada 17 kode — termasuk jaminan top-up deposit **tidak** dikirim sebagai `PENERIMAAN-KASIR` | Sebelum cutover | Gerbang G6 |
| 4 | **Konfirmasi atas tiga catatan yang berbeda** (bagian 6) | Kapan saja | Tidak menahan; perlu didamaikan |
| 5 | **Ubah `FIN-DEC-004`**: penerimaan sebelum tagihan final **terbit segera** — Accounting membukukannya sebagai Uang Muka Pasien — dan tambahkan **kode pemakaian uang muka** yang terbit saat tagihan final, untuk melunasi piutang dari uang muka. Kode ini dapat sama dengan kode pemakaian deposit pada butir 3 | Sebelum cutover | Gerbang G6 |

**Contoh kenapa butir 3 menjadi gerbang.** Top-up deposit Rp 5.000.000 yang terkirim sebagai
`PENERIMAAN-KASIR` dapat terbukukan sebagai pendapatan, padahal uang itu kewajiban kepada pasien.
Buku besar tetap seimbang dan tidak ada pesan galat — kesalahannya baru ketahuan saat pemeriksaan.

**Contoh kenapa butir 5 kami minta.** Pasien rawat inap membayar Rp 20.000.000 pada 25 November,
pulang 5 Desember dengan tagihan Rp 32.000.000. Bila kejadiannya ditahan, pada tutup buku November
kas di buku besar kurang Rp 20.000.000 dari kas di laci kasir — dan bila saldo subledger kas Finance
menghitung uang itu, penutupan November tertahan karena toleransi selisih kami nol. Dengan terbit
segera:

| Tanggal | Kejadian | Jurnal di Accounting |
|---|---|---|
| 25 November | `PENERIMAAN-KASIR` Rp 20.000.000 | Debit Kas, kredit Uang Muka Pasien |
| 5 Desember | `PENGAKUAN-PIUTANG` Rp 32.000.000 | Debit Piutang, kredit Pendapatan |
| 5 Desember | Pemakaian uang muka Rp 20.000.000 *(kode baru)* | Debit Uang Muka Pasien, kredit Piutang — sisa piutang Rp 12.000.000 |

---

## 6. Tiga catatan yang berbeda

Jawaban yang sampai ke kami lewat percakapan berbeda dengan decision log Finance pada tiga butir.
Dokumen jawaban Anda menyatakan decision log yang berlaku, jadi keputusan kami bersandar pada log:

| Butir | Versi percakapan | Decision log Finance | Mohon |
|---|---|---|---|
| Autentikasi | Akun layanan + JWT Bearer berumur pendek, hanya `Receive` | `FIN-DEC-007` (`draft`): mekanisme auth existing | Pilih salah satu, atau serahkan ke Platform |
| Saldo subledger | Amplop yang sama + empat rincian | `FIN-DEC-023` (`draft`): lima bidang berdiri sendiri | Ikuti bagian 3.3 |
| Cutover | 1 Oktober 2026 00.00 WIB | `FIN-DEC-008`: sejak go-live, tanggal saat MVP-5 siap | Ikuti bagian 4 |

---

## 7. Yang tidak berubah bagi Finance

- Dua belas bidang, tipe, dan kewajibannya.
- Dua kunci anti-ganda.
- Arti setiap kode balasan dan kewajiban Finance atasnya — kecuali catatan kecil `201` di
  bagian 3.2.
- Larangan data pasien di dalam pesan.

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/accounting/00-interview-decisions.md` bagian *Keputusan pasca-ratifikasi Finance* | Sembilan keputusan beserta alasannya |
| `docs/module-blueprints/accounting/contracts/cross-module-contract.md` (`ACC-XMOD-0.3`) | Kontrak yang berlaku, termasuk katalog 17 kode (bagian 3a), tanda terima (4b), dan gerbang cutover (13) |
| `docs/module-blueprints/accounting/contracts/api-contract.md` grup Accounting Event | Endpoint penerima beserta bidang opsional `SubledgerBalance` |
