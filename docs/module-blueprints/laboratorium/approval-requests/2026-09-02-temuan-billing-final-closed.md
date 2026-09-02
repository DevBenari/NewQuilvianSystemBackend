# Temuan Lintas Modul — Status `FINAL` dan `CLOSED` pada Invoice Billing

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-003` |
| `tanggal` | 2026-09-02 |
| `pengaju` | Ditemukan saat mengerjakan `BE-LAB-01` modul Laboratorium |
| `ditujukan kepada` | Pemilik modul `billing-kasir` |
| `rujukan` | `task/report/backend/BE-LAB-01.md` bagian 9.2 |
| `status` | `terbuka` — menunggu keputusan pemilik Billing |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `yang diminta` | **Keputusan**, bukan persetujuan. Pilihan A atau B pada bagian 5 |

> **Kenapa temuan Billing lahir dari task Laboratorium.** `BE-LAB-01` membutuhkan bukti test.
> Project `QuilvianSystemBackend.Tests` ternyata sudah gagal build sejak 2026-08-28, sehingga
> **tidak ada satu pun** dari 853 test di dalamnya yang pernah dijalankan sejak tanggal itu. Atas
> instruksi pemilik repository, build itu diperbaiki. Begitu suite berjalan kembali, satu test
> Billing gagal — dan kegagalannya menunjuk masalah yang sesungguhnya.

---

## 1. Ringkasan Satu Paragraf

Kontrak Billing menetapkan invoice berpindah dua langkah: finalisasi menjadikannya `FINAL`, lalu
AR/AP posting yang sukses menjadikannya `CLOSED`. Source produksi memotong langkah pertama —
invoice yang lunas penuh langsung dijadikan `CLOSED` saat finalisasi. Akibatnya jalur koreksi AR
yang mensyaratkan status `FINAL` **tidak pernah berjalan** untuk invoice lunas, tanpa error dan
tanpa log. Adjustment dan write-off atas tagihan yang sudah lunas tidak menghasilkan koreksi
piutang.

---

## 2. Bukti

### 2.1 Kontrak yang disetujui

`docs/module-blueprints/billing-kasir/contracts/state-transition-matrix.md`, baris 11 dan 12:

| Dari | Peristiwa | Ke | Pelaku | Syarat |
|---|---|---|---|---|
| `OPEN` | finalisasi | `FINAL` | Billing | semua order complete; kalkulasi current; patient responsibility settled atau exception sah |
| `FINAL` | AR/AP posting sukses | `CLOSED` | Sistem | handoff idempotent tercatat |

Kontrak menyebut `CLOSED` sebagai akibat **posting yang sukses**, bukan akibat finalisasi.

### 2.2 Source produksi

`Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`,
baris 124–131:

```csharp
// Tagihan pasien yang sudah lunas langsung berstatus CLOSED, bukan FINAL. FINAL hanya
// untuk invoice yang difinalisasi dengan sisa tanggung jawab (departure exception) -
// di situ masih ada piutang yang menunggu penyelesaian.
var isFullySettled = !isDepartureException && readiness.Outstanding <= 0;
invoice.Status = isFullySettled
    ? BillingInvoiceStatuses.Closed
    : BillingInvoiceStatuses.Final;
```

Penyimpangannya disengaja dan berkomentar. Yang tidak terlihat pada comment itu adalah akibatnya
di bagian 2.3.

### 2.3 Jalur yang ikut mati

`Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs`,
baris 150, di dalam `RecordCorrectionIfLinkedAsync` — jalur yang menerbitkan koreksi AR sesudah
adjustment atau write-off diposting:

```csharp
if (invoice is null || invoice.Status != BillingInvoiceStatuses.Final) return;
```

Invoice lunas tidak pernah berstatus `Final`, sehingga baris ini **selalu** keluar lebih awal
untuk invoice semacam itu. Keluarnya berupa `return` biasa: tidak ada exception, tidak ada log
peringatan, tidak ada penanda apa pun bahwa sesuatu dilewati.

### 2.4 Test yang memergokinya

`QuilvianSystemBackend.Tests/BillingManagement/BillingFinalizationServiceTests.cs`
baris 31 dan 35, pada test
`NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`:

```text
Assert.Equal() Failure: Strings differ
Expected: "FINAL"
Actual:   "CLOSED"
```

Test ini **sejalan dengan kontrak**. Ia bukan test usang yang tertinggal.

---

## 3. Dampak Bisnis

Yang hilang bukan status di layar, melainkan **koreksi piutang**.

> **Contoh berangka.** Tn. Budi dirawat dengan tagihan Rp2.000.000 dan membayar lunas. Invoice
> difinalisasi, statusnya menjadi `CLOSED`, AR handoff terbentuk sebesar Rp2.000.000.
>
> Tiga hari kemudian ketahuan satu tindakan salah tagih senilai Rp300.000. Petugas membuat
> adjustment Rp300.000 atas invoice itu. Adjustment-nya tercatat, tetapi
> `RecordCorrectionIfLinkedAsync` keluar diam-diam karena status invoice bukan `FINAL`.
>
> Hasilnya: pembukuan AR tetap mencatat Rp2.000.000, sementara nilai tagihan yang benar
> Rp1.700.000. Selisih Rp300.000 tidak pernah dikoreksi dan tidak pernah dilaporkan kepada
> siapa pun. Ia baru muncul saat rekonsiliasi manual — kalau memang ada yang merekonsiliasi.

Skala keterpaparannya adalah **seluruh invoice yang dibayar lunas** — dengan kata lain, jalur
yang paling umum, bukan kasus tepi.

Butir yang belum diketahui dan perlu dijawab pemilik Billing: **berapa banyak adjustment dan
write-off yang sudah diposting atas invoice berstatus `CLOSED` sejak perilaku ini berlaku.**
Angka itu menentukan apakah dibutuhkan backfill koreksi, bukan sekadar perbaikan ke depan.

---

## 4. Sejak Kapan Ini Tidak Terpantau

| Tanggal | Peristiwa |
|---|---|
| 2026-08-28 | `BillingSettlementServiceTests.cs` terakhir disentuh (commit `058e070`). Sesudahnya constructor `BillingSettlementService` bertambah parameter tanpa berkas test ikut disesuaikan |
| 2026-08-28 .. 2026-09-02 | `QuilvianSystemBackend.Tests` gagal build. **Seluruh 853 test** di dalamnya tidak pernah berjalan |
| 2026-09-02 | Build diperbaiki saat mengerjakan `BE-LAB-01`. Suite berjalan: 852 lulus, 1 gagal — kegagalan inilah temuannya |

Pelajaran yang layak dicatat terlepas dari keputusan A atau B: **satu error build pada project
test menyembunyikan 853 test sekaligus**, dan tidak ada yang memberi tahu selama lima hari.
Menjalankan build project test di CI akan menutup kelas masalah ini, bukan hanya kejadian kali
ini.

---

## 5. Yang Diminta — Pilih A atau B

### Pilihan A — source mengikuti kontrak

Finalisasi selalu menghasilkan `FINAL`. Hanya AR/AP posting yang sukses yang memindahkan invoice
ke `CLOSED`.

| Aspek | Isi |
|---|---|
| Yang berubah | `BillingFinalizationService.cs` baris 128–130 |
| Yang perlu ditelusuri lebih dulu | Apakah jalur yang memindahkan `FINAL` → `CLOSED` sesudah posting sukses **memang sudah ada**. Bila belum ada, invoice lunas akan berhenti di `FINAL` selamanya, dan itu masalah baru |
| Akibat | Test yang gagal langsung hijau; `RecordCorrectionIfLinkedAsync` kembali bekerja |
| Risiko | Perilaku finansial produksi berubah. Invoice lama berstatus `CLOSED` tetap perlu diputuskan nasibnya |

### Pilihan B — kontrak mengikuti source

`state-transition-matrix.md` direvisi supaya invoice lunas penuh boleh langsung `CLOSED`.

| Aspek | Isi |
|---|---|
| Yang berubah | Kontrak Billing, **dan** penjaga pada `RecordCorrectionIfLinkedAsync` baris 150 wajib ikut menerima `CLOSED` |
| Akibat | Test perlu diperbarui mengikuti kontrak baru |
| Risiko | **Bila penjaga baris 150 tidak ikut diperbaiki, lubang koreksi AR menjadi permanen dan resmi.** Kedua perubahan itu satu paket, tidak boleh dikerjakan setengah |
| Wewenang | Revisi kontrak memerlukan persetujuan pemilik modul Billing |

**Butir tambahan yang berlaku pada kedua pilihan:** tentukan apakah dibutuhkan backfill koreksi
AR atas adjustment dan write-off yang terlanjur diposting ke invoice `CLOSED`.

---

## 6. Yang Sengaja Tidak Dikerjakan

| Tindakan | Alasan |
|---|---|
| Mengubah test agar mengharapkan `CLOSED` | Itu akan membuat suite hijau sambil mengunci penyimpangan terhadap kontrak beserta lubang koreksi AR-nya. Persis kebalikan dari gunanya test itu ada |
| Mengubah `BillingFinalizationService.cs` | Perilaku finansial produksi milik pemilik Billing, bukan milik task Laboratorium. Pilihan A juga menuntut penelusuran yang belum dilakukan |
| Mengubah `state-transition-matrix.md` | Kontrak modul lain; revisinya lewat skill dan pemilik modulnya sendiri |
| Menandai test sebagai `Skip` | Menyembunyikan temuan sama saja dengan mengembalikan keadaan lima hari terakhir |

Satu-satunya berkas Billing yang diubah adalah `BillingSettlementServiceTests.cs`, sebatas
melengkapi argumen constructor yang hilang supaya project test kembali dapat di-build. Tidak ada
perilaku produksi yang tersentuh.
