# `BUG-PHA-BE-002` — Klarifikasi yang sudah ditutup masih dapat dijawab dokter

Ditemukan 2 Oktober 2026 saat penambahan uji regresi Farmasi. **Belum diperbaiki**; task yang
menemukannya berfokus verifikasi.

| Hal | Isi |
|---|---|
| Prioritas usulan | **P1** — membuka kembali telaah yang sudah beres, tanpa jejak yang jelas bagi petugas |
| Pemilik | modul Farmasi |
| Berkas | `Areas/HealthServices/PharmacyManagement/Services/PrescriptionReviewService.cs` |
| Terjaga uji | `Tests/QuilvianSystemBackend.PharmacyTests/PharmacyWorkflowTests.cs` → `Klarifikasi_yang_sudah_ditutup_MASIH_dapat_dijawab_dokter` |

## Masalah

`RespondClarificationAsync` memeriksa status klarifikasi dan menolak yang sudah selesai:

```csharp
if (entity.Status is PrescriptionClarificationStatus.Closed
    or PrescriptionClarificationStatus.Cancelled)
    throw new InvalidOperationException("Klarifikasi sudah ditutup.");
```

Tetapi `CloseClarificationAsync` **tidak pernah menyetel salah satu dari kedua status itu**:

```csharp
entity.Status = request.Accepted
    ? PrescriptionClarificationStatus.AcceptedByPharmacist
    : PrescriptionClarificationStatus.Rejected;
entity.ClosedAt = now;
```

Penjaga itu karena itu tidak pernah menyala lewat jalur penutupan normal. `Closed` dan
`Cancelled` hanya dapat muncul dari jalur lain, yang pada alur biasa tidak terjadi.

## Akibatnya

Jawaban dokter yang masuk **setelah** apoteker menutup klarifikasi tetap diterima, dan
`RespondClarificationAsync` kemudian memundurkan telaahnya:

```csharp
entity.PrescriptionReview.Status = PrescriptionReviewStatus.RevisedByDoctor;
```

Jadi telaah yang sudah disetujui atau sudah ditutup apoteker kembali menjadi `RevisedByDoctor`.
`ClosedAt` dan `ClosedByUserId` pada klarifikasi tetap terisi, sehingga pada layar ia tampak
sudah beres sementara telaahnya sudah mundur — petugas tidak punya petunjuk mengapa.

Keadaan ini tidak membutuhkan niat buruk: cukup dokter membuka layar lama, atau permintaan
tertunda yang tiba terlambat.

## Acceptance ketika nanti diperbaiki

1. **Klarifikasi yang sudah ditutup tidak menerima respons dokter lagi.** Penutupan dalam bentuk
   apa pun — `AcceptedByPharmacist`, `Rejected`, `Closed`, `Cancelled`, atau keberadaan
   `ClosedAt` — harus menutup pintu respons.
2. **Status telaah tidak mundur dari keadaan selesai karena respons terlambat.** Telaah yang
   sudah `Approved` atau sudah ditutup tidak boleh kembali menjadi `RevisedByDoctor`.
3. **Respons yang sama dikirim ulang aman.** Pengiriman kedua dengan isi identik tidak mengubah
   apa pun dan tidak melahirkan baris maupun transisi kedua.

## Yang perlu diputuskan lebih dulu

Pemeriksaannya sebaiknya memakai `ClosedAt` atau daftar status tertutup yang lengkap? Keduanya
menutup celah ini, tetapi daftar status menuntut setiap status baru kelak ikut didaftarkan,
sementara `ClosedAt` berlaku sendiri. Pilihan itu milik pemilik modul, bukan diputuskan dari task
pengujian.

## Catatan

Uji yang menjaganya **sengaja dinamai sesuai perilaku sebenarnya**, bukan sesuai perilaku yang
diharapkan. Ia lulus hari ini dan akan gagal begitu bug ini diperbaiki — kegagalan itulah
penanda bahwa perbaikannya benar-benar mengubah perilaku, dan ujinya harus diperbarui bersama
perbaikan tersebut.
