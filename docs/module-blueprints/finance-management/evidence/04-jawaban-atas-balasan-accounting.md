# Jawaban Finance atas Balasan Accounting — Kejadian Keuangan

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 25 September 2026 |
| Menjawab | `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` (24 September 2026) |
| Sifat | **Persetujuan atas satu perubahan perilaku yang sudah terkode, empat usulan kode kejadian baru yang butuh ratifikasi Anda, dan penutupan tiga catatan yang berbeda.** Belum ada kode yang diubah di sisi Finance — dokumen ini adalah keputusan tertulis sebelum kodenya disentuh |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`, keputusan `FIN-DEC-030` sampai `FIN-DEC-039`, seluruhnya `approved` 25 September 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.3` sebagaimana Anda ratifikasi. Bila berkas ini berbeda dari decision log Finance di atas, decision log yang berlaku |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Finance
yang lain.

---

## 1. Ringkasan satu paragraf

Terima kasih atas balasannya yang rinci. Finance **menerima seluruh lima permintaan** pada
bagian 5 surat Anda. Yang paling berdampak: Finance **mengubah kebijakan `HELD_FOR_FINALIZATION`**
menjadi terbit segera sebagai Uang Muka Pasien — persis seperti yang Anda minta, dan Finance
sendiri yang meminta perubahan ini disetujui (`FIN-DEC-030`) karena contoh selisih kas Rp 20 juta
pada surat Anda memang nyata di alur kerja Finance. **Perubahan ini menyentuh kode yang sudah
berjalan**, jadi belum kami eksekusi — kami menunggu balasan Anda atas empat usulan kode baru di
bagian 4 sebelum menyentuh `FinanceReceiptService` dan `FinanceAccountingOutboxService`. Bentuk
saldo subledger Anda **diterima apa adanya**. Tiga catatan yang berbeda pada bagian 6 surat Anda
**ditutup**: decision log Finance yang berlaku, dan tanggal 1 Oktober 2026 **dibatalkan**.

---

## 2. Jawaban atas keputusan Anda

| Bagian surat Anda | Jawaban Finance | Dasar |
|---|---|---|
| 2 — Ratifikasi rantai dan bentuk pesan, penutupan `ACC-XM-001` | **Diterima tanpa syarat.** Tidak ada yang berubah di sisi Finance | `FIN-DEC-039` |
| 2 — Tujuh belas kode diratifikasi apa adanya | **Dicatat sebagai fakta.** Finance tidak mengusulkan kode baru untuk 17 kode ini | `FIN-DEC-039` |
| 2 — `JASA_MEDIS` tidak ikut `PENGAKUAN-PIUTANG` | **Sudah konsisten** dengan `FIN-DEC-003` sejak awal; tidak ada penyesuaian di sisi Finance | `FIN-DEC-039` |
| 3.3 — Bentuk saldo subledger diubah | **Diterima apa adanya.** `FIN-DEC-023` (draft lima field) kami tarik, digantikan bentuk Anda | `FIN-DEC-035` |
| 3.4 — Syarat akun layanan | **Diratifikasi** sebagai tiga syarat organisasi. Mekanisme token tetap kami tunggu bersama Platform | `FIN-DEC-036` |
| 4 — Cutover enam gerbang, 1 Oktober tidak layak | **Diterima.** Finance sendiri mengakui `G4` ("pengirim Finance siap") belum terbangun | `FIN-DEC-037` |
| 5 butir 5 — Ubah `FIN-DEC-004` | **Diterima — inilah jawaban utama surat ini.** Lihat bagian 3 | `FIN-DEC-030` |

---

## 3. Perubahan `FIN-DEC-004` — jawaban atas permintaan yang menyentuh kode

Finance setuju penuh. Penerimaan sebelum invoice final **tidak lagi ditahan**
`HELD_FOR_FINALIZATION`. Ia terbit segera ke Accounting sebagai kode baru **`PENERIMAAN-UANG-MUKA`**
(lihat bagian 4), yang Anda bukukan sebagai Uang Muka Pasien.

**Kenapa Finance setuju, bukan sekadar menuruti.** Contoh Anda tepat: pasien rawat inap yang
membayar Rp 20.000.000 pada 25 November tapi baru pulang 5 Desember membuat kas di Finance dan
kas di buku besar Anda berselisih Rp 20.000.000 persis di titik tutup buku November, padahal
uangnya nyata dan sudah ada di kasir sejak 25 November. Menahan jurnalnya tidak menahan risikonya
— uangnya sudah harus dipertanggungjawabkan sejak diterima.

**Kode pemakaian saat invoice final terbit.** Finance mengusulkan satu kode gabungan
`PEMAKAIAN-UANG-MUKA-DEPOSIT` (bagian 4) yang terbit pada hari invoice final terbit, melunasi
piutang dari uang muka. Kode ini sama untuk pemakaian uang muka pra-invoice **maupun** pemakaian
deposit top-up eksplisit (butir 3 surat Anda) — sesuai saran Anda bahwa keduanya boleh sama,
karena keduanya sama-sama pengurangan kewajiban Uang Muka Pasien.

**Dampak bagi kontrak Anda bagian 5.5** ("Penahanan sebelum tagihan final"): baris `HELD_FOR_FINALIZATION`
pada kontrak itu tidak lagi berlaku untuk skenario ini. Mohon kontrak `ACC-XMOD` Anda disesuaikan
mengikuti bagian 4 di bawah.

**Kapan kode ini mulai dipakai.** Finance **belum** mengubah kode `FinanceReceiptService.cs`
maupun `FinanceAccountingOutboxService.cs`. Perubahan itu menunggu Anda meratifikasi nama dan
bentuk empat kode baru pada bagian 4, supaya Finance tidak menulis kode dua kali bila nama yang
Anda setujui berbeda dari usulan kami.

---

## 4. Empat kode kejadian baru — mohon ratifikasi Anda

Ini jawaban atas permintaan Anda bagian 5 butir 1, 3, dan 5 sekaligus. Finance mengusulkan empat
kode baru, kode ke-18 sampai ke-21, dengan bentuk amplop 12 bidang yang sama seperti kontrak Anda
— tidak ada bidang baru selain yang sudah Anda tetapkan pada bagian 3.3 surat Anda.

| # | Kode | Dipicu oleh | Lawan jurnal (usulan) | Dasar |
|---|---|---|---|---|
| 18 | `SALDO-SUBLEDGER` | Tutup periode Finance | — (bukan jurnal, hanya dicocokkan) | `FIN-DEC-035`, bentuk sudah Anda tetapkan bagian 3.3 surat Anda |
| 19 | `PENERIMAAN-UANG-MUKA` | (a) Penerimaan sebelum invoice final; (b) deposit top-up eksplisit; (c) sisi lebih dari kelebihan bayar | Debit Kas, Kredit Uang Muka Pasien | `FIN-DEC-031`, `FIN-DEC-033` |
| 20 | `PEMAKAIAN-UANG-MUKA-DEPOSIT` | Invoice final terbit dan melunasi piutang dari uang muka/deposit/kelebihan bayar | Debit Uang Muka Pasien, Kredit Piutang | `FIN-DEC-032`, `FIN-DEC-033` |
| 21 | `SELISIH-KAS-SHIFT` | Selisih hitung fisik kas vs sistem saat tutup shift kasir | Debit/Kredit akun selisih kas (suspense) | `FIN-DEC-034` |

**Kenapa `PENERIMAAN-UANG-MUKA` terpisah dari `PENERIMAAN-KASIR`.** Anda sendiri menulis bahwa
top-up deposit dilarang dikirim sebagai `PENERIMAAN-KASIR`. Alasan yang sama berlaku untuk
penerimaan pra-invoice-final: keduanya kewajiban ke pasien, bukan pendapatan langsung.
`PENERIMAAN-KASIR` tetap kami pakai hanya untuk penerimaan yang benar-benar final.

**Kenapa kelebihan bayar tidak dapat kode sendiri.** Pasien yang membayar Rp 500.000 untuk
tagihan Rp 450.000 secara akuntansi sama dengan pasien yang menitip uang muka — kelebihannya
tetap kewajiban Finance ke pasien sampai dikembalikan atau dipakai. Memberinya kode sendiri hanya
menduplikasi aturan posting yang sama.

**Kenapa selisih kas shift berbeda sifat.** Ini bukan kewajiban ke pasien sama sekali — murni
selisih hitung fisik kasir. Kami pisahkan kodenya supaya tidak tercampur dengan kewajiban pasien
pada `PENERIMAAN-UANG-MUKA`.

### Contoh pesan `PENERIMAAN-UANG-MUKA`

Pasien rawat inap membayar Rp 20.000.000 tunai pada 25 November, invoice belum final.

```json
{
  "EventNumber": "EVT-2026-11-04521",
  "EventTypeCode": "PENERIMAAN-UANG-MUKA",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-RCP-2026-11-00789",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-25T10:15:00+07:00",
  "AccountingDate": "2026-11-25",
  "Amount": 20000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "9a8b7c6d-5e4f-4a3b-9c8d-7e6f5a4b3c2d",
  "CausationId": "9a8b7c6d-5e4f-4a3b-9c8d-7e6f5a4b3c2d",
  "Components": "TOTAL"
}
```

### Contoh pesan `PEMAKAIAN-UANG-MUKA-DEPOSIT`

Pasien yang sama pulang 5 Desember dengan tagihan Rp 32.000.000. Uang muka Rp 20.000.000
dipakai melunasi sebagian, sisa piutang Rp 12.000.000 tetap `OUTSTANDING`.

```json
{
  "EventNumber": "EVT-2026-12-00113",
  "EventTypeCode": "PEMAKAIAN-UANG-MUKA-DEPOSIT",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-AR-2026-12-00456",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-12-05T14:00:00+07:00",
  "AccountingDate": "2026-12-05",
  "Amount": 20000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "9a8b7c6d-5e4f-4a3b-9c8d-7e6f5a4b3c2d",
  "CausationId": "3c2d1e0f-8a9b-4c3d-8e7f-6a5b4c3d2e1f",
  "Components": "TOTAL"
}
```

Kejadian `PENGAKUAN-PIUTANG` Rp 32.000.000 tetap terbit terpisah pada tanggal yang sama, seperti
contoh pada surat Anda bagian 5.

### Contoh pesan `SELISIH-KAS-SHIFT`

Shift kasir ditutup dengan kas fisik Rp 30.000 lebih kecil dari catatan sistem.

```json
{
  "EventNumber": "EVT-2026-11-09981",
  "EventTypeCode": "SELISIH-KAS-SHIFT",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-SHIFT-2026-11-00234",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-20T21:05:00+07:00",
  "AccountingDate": "2026-11-20",
  "Amount": -30000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f",
  "CausationId": "5f4e3d2c-1b0a-4c9d-8e7f-6a5b4c3d2e1f",
  "Components": "TOTAL"
}
```

Nilai negatif dipakai untuk kekurangan kas, positif untuk kelebihan kas — sama seperti aturan
nilai boleh negatif yang sudah Anda tetapkan untuk pesan saldo subledger.

**Yang Finance minta dari Anda:** konfirmasi atau ubah nama keempat kode ini, beserta lawan
jurnalnya. Sampai Anda meratifikasi, Finance **tidak** menyentuh kode `FinanceReceiptService`
maupun `FinanceAccountingOutboxService` — kami tidak ingin menulis kode dua kali bila nama yang
Anda setujui berbeda.

---

## 5. `Components` — jawaban atas permintaan bagian 5 butir 2

**Seluruhnya `TOTAL` untuk rilis ini.** Finance belum memecah komponen apa pun pada satu jenis
kejadian manapun, termasuk keempat kode baru di atas. Field `Components` pada kode kami saat ini
murni diteruskan apa adanya dari pemanggilnya dan belum pernah diisi aturan bisnis nyata, jadi
jawaban ini tidak menahan pekerjaan Anda menyusun aturan posting. Bila kelak Finance butuh
memecah komponen (misalnya rincian layanan di dalam satu invoice), kami ajukan sebagai amendment
terpisah, bukan sekarang.

---

## 6. Menutup tiga catatan yang berbeda (bagian 6 surat Anda)

| Butir | Keputusan Finance |
|---|---|
| Autentikasi | Tiga syarat organisasi Anda **diratifikasi** (`FIN-DEC-036`): akun khusus bukan `SuperAdmin`, wajib penugasan Departemen + Jabatan dengan hak `AccountingEvent : Receive` saja, terikat badan hukum tujuan. Mekanisme token (JWT Bearer vs lainnya) **tetap terbuka**, kami tunggu bersama Platform |
| Saldo subledger | Bentuk Anda (amplop + `SubledgerBalance`) **diterima apa adanya** (`FIN-DEC-035`). Draft lima field Finance ditarik |
| Cutover | **Decision log Finance yang berlaku** (`FIN-DEC-008`, dikonfirmasi `FIN-DEC-037`): sejak go-live, tanggal ditentukan enam gerbang Anda. Referensi "1 Oktober 2026 00.00 WIB" pada versi percakapan sebelumnya **kami nyatakan tidak berlaku** |

Finance tidak mengejar tanggal 1 Oktober 2026 dan tidak meminta percepatan gerbang `G1`–`G3` yang
bukan kendali kami. Kami sadar `G4` ("Pengirim Finance siap") masih berstatus belum terbangun di
sisi kami, sehingga wajar bila tanggal pastinya masih menunggu.

---

## 7. Yang Finance jamin tetap berlaku dari suratnya sendiri (evidence 01)

Seluruh jaminan pada bagian 3 `evidence/01-jawaban-untuk-owner-accounting.md` tetap berlaku tanpa
perubahan: data pasien tidak pernah dikirim, hanya `IDR`, dua lapis pencegahan ganda, koreksi
selalu menaikkan `SourceVersion`, kejadian ditulis di transaksi database yang sama dengan fakta
bisnisnya, dan balasan `422` ditandai tertahan tanpa mengirim kejadian baru.

**Satu tambahan mengikuti catatan Anda bagian 3.2:** Finance memperlakukan `JournalNumber` pada
balasan `201` sebagai **opsional** — disimpan bila ada, tidak dianggap gagal bila kosong. Bila
Finance memang butuh nomor jurnalnya, kami kirim ulang `EventNumber` yang sama seperti yang Anda
jelaskan, dan menerima balasan `200` tanpa membuatnya sebagai kejadian baru.

---

## 8. Keadaan pekerjaan Finance saat ini

| Hal | Keadaan pada 25 September 2026 |
|---|---|
| Keputusan bisnis atas balasan Anda | Sepuluh keputusan baru (`FIN-DEC-030`..`039`), seluruhnya `approved` |
| Kode `FinanceReceiptService`, `FinanceAccountingOutboxService` | **Belum diubah** — menunggu ratifikasi Anda atas bagian 4 |
| Kontrak Finance (`integration-contract.md`, `state-transition-matrix.md`) | Belum direvisi — menunggu ratifikasi Anda, lalu direvisi lewat proses desain kami |
| Pengiriman kejadian ke Accounting | Tetap belum aktif, menunggu endpoint Anda (`G1`) |

---

## 9. Yang Finance butuhkan dari Accounting

| # | Butuh | Kapan dibutuhkan | Menahan apa |
|---|---|---|---|
| 1 | Ratifikasi atau koreksi nama dan lawan jurnal empat kode baru (bagian 4) | Sebelum Finance mengubah kode `FinanceReceiptService`/`FinanceAccountingOutboxService` | `FIN-DEC-030` tidak bisa dieksekusi di kode |
| 2 | Kabar bila kontrak `ACC-XMOD` Anda sudah diperbarui menghapus baris `HELD_FOR_FINALIZATION` pada bagian 5.5 | Sebelum cutover | Kesesuaian kontrak dua pihak |
| 3 | Kabar ketika kotak masuk Accounting (`G1`) sudah dibangun | Kapan saja | Pengaktifan pengiriman, `EPIC FIN-12` |

Tidak satu pun dari ketiganya menahan pekerjaan Finance yang sedang berjalan di luar rumpun
integrasi Accounting.

---

## 10. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | `FIN-DEC-030`..`039`, seluruhnya `approved` 25 September 2026, beserta owner dan bukti |
| `docs/module-blueprints/finance-management/evidence/01-jawaban-untuk-owner-accounting.md` | Jawaban Finance yang pertama, masih berlaku kecuali bagian yang diperbarui surat ini |
| `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` | Surat yang dijawab dokumen ini |
