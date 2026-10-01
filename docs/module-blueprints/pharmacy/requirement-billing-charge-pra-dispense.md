# Requirement lintas modul: Billing menerima charge resep pada state pra-dispense

Dicatat 1 Oktober 2026 oleh pemilik modul Farmasi. **Pemilik keputusan: tim Billing/Kasir.**
Berkas ini permintaan beserta buktinya, bukan pengumuman perubahan.

| Hal | Isi |
|---|---|
| Pemohon | modul Farmasi |
| Pemilik keputusan | modul Billing/Kasir |
| Status | **Menunggu approval owner Billing** |
| Berkas terdampak | `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` |
| Bukti runtime | `verifikasi-runtime-be-bkc-067.md` |

## Masalahnya

`BillingChargeSourceAdapter` menetapkan `["PHARMACY"] = Policy(["DISPENSED"], [], [])`, dan menolak
status lain dengan *"Jumlah obat yang diserahkan belum final."* Syarat itu mengunci dirinya sendiri:

```
item invoice PHARMACY  --butuh-->  resep DISPENSED (tahap 9)
resep DISPENSED        --butuh-->  financial clearance  (PrescriptionDispensingService.cs:629)
clearance              --butuh-->  surat CLEARED
surat CLEARED          --butuh-->  item invoice PHARMACY (BilConsumerHandoffService.cs:303)
```

Telaah resep ikut terkunci oleh gerbang yang sama (`PrescriptionReviewService.cs:65`, tahap 4 → 5),
dan `BilPrescriptionClearanceRecoveryService.cs:43` tidak menolong karena ia pun mulai dari item
invoice ber-`SourceDomain = "PHARMACY"`.

Akibat nyatanya: **tidak satu pun resep rawat jalan pernah dapat ditagih maupun diserahkan.** Enam
status pra-penyerahan yang jujur dicoba lewat `POST invoices/from-source` — `SUBMITTED`,
`PRESCRIBED`, `FINALIZED`, `READY`, `VERIFIED`, `PREPARED` — seluruhnya 422.

Akar bisnisnya: obat rawat jalan **dibayar sebelum diserahkan**, sehingga tagihannya harus sudah
berdiri ketika pasien berada di kasir. Syarat `DISPENSED` menganggap tagihan terbit sesudah
penyerahan, yang hanya benar untuk rawat inap.

## Perubahan yang diminta

```diff
- ["PHARMACY"] = Policy(["DISPENSED"], [], []),
+ ["PHARMACY"] = Policy(["SUBMITTED", "DISPENSED"], [], []),
```

beserta pencabutan penjaga khusus di `ValidateAndNormalize`:

```diff
- if (domain == "PHARMACY" && status != "DISPENSED")
-     throw new BillingInvoiceValidationException("Jumlah obat yang diserahkan belum final.");
```

`SUBMITTED` berarti resep sudah difinalkan dokter — tahap pemenuhan 2 `WaitingForPayment`.
`DISPENSED` tetap diterima dan tetap bermakna: ia yang memperbarui jumlah final bila yang diserahkan
berbeda dari yang diresepkan, lewat `SourceVersion` lebih tinggi pada baris yang sama.
`SourceDetailId` selalu PrescriptionId, jadi tidak ada baris baru.

Nama statusnya sendiri bukan harga mati. Yang dibutuhkan Farmasi adalah **ada** status pra-dispense
yang diterima; bila owner Billing lebih suka istilah lain, Farmasi menyesuaikan pengirimnya.

## Bukti bahwa perubahan ini cukup

Dengan policy di atas, rantai penuh terbukti berjalan 1 Oktober 2026:

charge PHARMACY `SUBMITTED` → invoice `BIL-20261001-00000002` → kalkulasi v1 → settlement → tender
tunai `SUCCEEDED` → invoice `CLOSED` → surat `CLEARED / PAID / INVOICE_SETTLED` → consumer Farmasi →
resep tahap **2 → 4**.

Rincian lengkap beserta ID tiap langkah ada di `verifikasi-runtime-be-bkc-067.md`.

## Dampak bila tidak di-approve

`PrescriptionBillingChargeProducer` di Farmasi mengirim `SourceStatus = "SUBMITTED"`. Tanpa
perubahan policy ini, pengirimannya ditolak 422 dan dicatat sebagai keluhan pada
`BillingHandoffIssues` — finalisasi konsultasi tetap sah, tetapi tagihan obat tidak berdiri dan
resep kembali tertahan pada tahap 2. Jadi producer Farmasi **bergantung** pada approval ini untuk
berfungsi, meskipun ia tidak membuat Farmasi gagal.

## Yang sengaja tidak diminta

Reversal, amandemen resep, void baris obat, dan rincian per obat pada invoice tidak termasuk
permintaan ini. `NormalVoidFromStatuses` dan `VoidStatuses` untuk PHARMACY dibiarkan kosong seperti
sekarang.
