# `BUG-PHA-BE-002` — Klarifikasi yang sudah ditutup masih dapat dijawab dokter

Ditemukan 2 Oktober 2026 saat penambahan uji regresi Farmasi. **Selesai** 2 Oktober 2026.

| Hal | Isi |
|---|---|
| Status | ✅ **Selesai** — diperbaiki beserta 4 uji regresi |
| Prioritas | **P1** — membuka kembali telaah yang sudah beres, tanpa jejak yang jelas bagi petugas |
| Pemilik | modul Farmasi |
| Berkas | `Areas/HealthServices/PharmacyManagement/Services/PrescriptionReviewService.cs` |
| Terjaga uji | `Tests/QuilvianSystemBackend.PharmacyTests/PharmacyWorkflowTests.cs` → `Klarifikasi_yang_sudah_ditutup_tidak_dapat_dijawab_lagi` dan 3 uji lainnya |

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

## Keputusan pemilik modul — 2 Oktober 2026

**Penjaga utamanya `ClosedAt != null`.** Jangan bergantung hanya pada daftar status tertutup;
status boleh dipakai sebagai validasi tambahan, bukan sebagai satu-satunya pemeriksaan.

Alasannya: `ClosedAt` terisi pada setiap jalur penutupan dan berlaku sendiri tanpa menuntut
daftar status diperbarui setiap kali status baru ditambahkan. Daftar status yang tertinggal satu
nilai akan membuka kembali celah yang sama tanpa ada yang menyadarinya.

Bentuk yang diminta pada `RespondClarificationAsync`:

```csharp
if (entity.ClosedAt != null)
    throw new InvalidOperationException("Klarifikasi sudah ditutup.");

// Pemeriksaan status dipertahankan sebagai lapisan tambahan, bukan pengganti.
if (entity.Status is PrescriptionClarificationStatus.Closed
    or PrescriptionClarificationStatus.Cancelled)
    throw new InvalidOperationException("Klarifikasi sudah ditutup.");
```

## Perbaikan

`ClosedAt != null` dipasang sebagai penjaga pertama pada `RespondClarificationAsync`, dan
pemeriksaan status dipertahankan di bawahnya sebagai lapisan tambahan — ia menangkap baris yang
berstatus tertutup tetapi stempel waktunya belum terisi.

## Uji regresi

`Tests/QuilvianSystemBackend.PharmacyTests/PharmacyWorkflowTests.cs`, 4 uji:

| Uji | Yang dijaga |
|---|---|
| `Klarifikasi_yang_sudah_ditutup_tidak_dapat_dijawab_lagi` | penutupan `AcceptedByPharmacist` menutup pintu respons |
| `Klarifikasi_yang_ditolak_apoteker_juga_tidak_dapat_dijawab_lagi` | `Rejected` pun mengisi `ClosedAt`, tanpa perlu didaftarkan tersendiri |
| `Telaah_tidak_mundur_karena_jawaban_dokter_yang_terlambat` | akibat yang sebenarnya dijaga — status telaah tidak kembali ke `RevisedByDoctor` |
| `Jawaban_yang_sama_dikirim_ulang_sebelum_penutupan_aman` | pengiriman ulang tidak melahirkan transisi kedua |