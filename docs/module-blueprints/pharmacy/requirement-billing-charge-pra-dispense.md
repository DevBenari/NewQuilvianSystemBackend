# Requirement lintas modul: Billing menerima charge resep pada state pra-dispense

Dicatat 1 Oktober 2026 oleh pemilik modul Farmasi. **Pemilik keputusan: tim Billing/Kasir.**
Berkas ini permintaan beserta buktinya, bukan pengumuman perubahan.

| Hal | Isi |
|---|---|
| Pemohon | modul Farmasi |
| Pemilik keputusan | modul Billing/Kasir |
| Status | ✅ **DIPENUHI owner Billing** — lihat bagian "Penutupan" di bawah |
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

---

## Penutupan — 6 Oktober 2026

**Permintaan ini sudah dipenuhi owner Billing, dengan bentuk yang lebih baik dari yang diminta.**

`BillingChargeSourceAdapter` kini berbunyi:

```csharp
// RJ-E2E-DEC-005: obat ditagih dua tahap. PRESCRIBED = tahap 1 saat resep difinalkan
// dokter, supaya gerbang "lunas sebelum serah" farmasi dapat dilalui; masih boleh
// dibatalkan normal selama belum diproses farmasi. DISPENSED = jumlah aktual yang
// diserahkan dan tetap final — koreksinya berupa adjustment. PRESCRIBED hanya sah
// pada kontrak 1.3 (lihat ValidateAndNormalize).
["PHARMACY"] = Policy(["PRESCRIBED", "DISPENSED"], ["PRESCRIBED"], ["CANCELLED"]),
```

Dua hal yang **berbeda** dari usulan Farmasi, dan keduanya lebih baik:

1. **Nama statusnya `PRESCRIBED`, bukan `SUBMITTED`.** Usulan Farmasi memakai kosakata
   internalnya sendiri. Billing memilih nama yang netral dan menjelaskan keadaannya
   ("diresepkan"), bukan nama langkah pada alur Farmasi.
2. **Dibatasi kontrak `BIL-INTEGRATION-1.3`.** Usulan Farmasi melonggarkan aturan untuk semua
   pemanggil. Billing mengikatnya pada versi kontrak baru, sehingga pemanggil lama tetap
   terikat aturan lama dan perilakunya tidak berubah diam-diam. Ini pengamanan yang tidak
   terpikirkan saat permintaan ditulis.

### Akibatnya bagi Farmasi, dan mengapa ia tidak langsung menyala

Pemenuhan ini tidak otomatis membuat rantainya jalan. Farmasi masih mengirim `SUBMITTED` pada
kontrak `1.2`, sehingga setiap tagihan tahap 1 tetap tertolak — kini bukan karena aturannya
belum ada, melainkan karena kedua sisi memakai kosakata berbeda. Gejalanya identik dengan
sebelumnya, pesan galatnya pun sama, sehingga mudah disalahbaca sebagai "Billing belum setuju".

Diselaraskan 6 Oktober 2026 pada `PrescriptionBillingChargeProducer`:

| | Sebelum | Sesudah |
|---|---|---|
| `SourceStatus` | `SUBMITTED` | `PRESCRIBED` |
| `ContractVersion` | `BIL-INTEGRATION-1.2` | `BIL-INTEGRATION-1.3` |

Konstanta `SubmittedSourceStatus` ikut berganti nama menjadi `PrescribedSourceStatus`, karena
nama lama akan menyesatkan pembaca berikutnya.

**Nol baris Billing disentuh dalam penyelarasan ini.**

### Pelajaran yang dikunci uji

Kedua konstanta itu sebelumnya **konsisten di dalam Farmasi** dan lulus seluruh uji Farmasi,
tetapi ditolak Billing pada setiap pengiriman. Mengunci nilai konstanta saja tidak pernah cukup:
kegagalan seperti ini tidak terlihat dari dalam satu modul.

`BillingChargeProducerTests` karena itu kini melewatkan tagihan Farmasi ke
**`ContractBillingChargeSourceAdapter` yang sebenarnya**, bukan ke tiruan:

| Uji | Yang dibuktikan |
|---|---|
| `Tagihan_tahap_satu_diterima_validator_Billing_yang_sebenarnya` | `PRESCRIBED` + `1.3` lolos |
| `Status_atau_kontrak_lama_ditolak_validator_Billing` | ketiga kombinasi lama ditolak — pembuktian negatif, supaya uji di atas tidak lulus karena validatornya permisif |
| `Tagihan_tahap_dua_tetap_diterima_pada_kontrak_lama_dan_baru` | `DISPENSED` tetap jalan pada `1.2` maupun `1.3`; penyelarasan tahap 1 tidak mempersempit jalur yang sudah ada |

### Yang masih belum terbukti

Rantai penuh **tagihan → kasir → surat clearance → penyerahan** belum diuji ulang di runtime
sesudah penyelarasan ini, karena startup pada basis data dev masih mati di
`MstNursingDiagnosisSeeder` —
[`blocker-startup-seeder-tabel-hilang.md`](../../engineering/blocker-startup-seeder-tabel-hilang.md).
Yang sudah terbukti: validator Billing menerima bentuk tagihannya, dan rantai hilirnya sendiri
sudah pernah terbukti ujung ke ujung pada
[`verifikasi-runtime-be-bkc-067.md`](verifikasi-runtime-be-bkc-067.md).
