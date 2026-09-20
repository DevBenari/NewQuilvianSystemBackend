# BE-BKC-065 — Backfill data tagihan lama yang sudah lunas

## Ringkasan untuk pembaca umum

Ini langkah terakhir dari rangkaian perbaikan gap `FINAL`→`CLOSED`. Dry-run (`BE-BKC-064`) sudah
mengukur ada tepat satu tagihan lama yang sudah lunas tapi masih berstatus "Final". Task ini
memindahkannya ke "Closed" — **satu baris data**, dieksekusi manual oleh pengguna sendiri lewat
SQL langsung (bukan EF Core migration), sesuai permintaan eksplisit saat proses berjalan.

**Hasil: berhasil. Satu invoice (`BIL-20260903-00000004`) berpindah ke `CLOSED`, `ClosedAt` terisi
waktu pelunasan sebenarnya (2026-09-10 11:45:53 +0700, bukan waktu eksekusi script), dan perubahan
sudah di-commit permanen oleh pengguna.**

---

- TASK ID: BE-BKC-065
- TASK TYPE: Backfill data (satu baris), dieksekusi manual — **bukan** EF Core migration
- COMPLEXITY: LIGHT (nol source, nol skema; satu skrip SQL data-only)
- CLASSIFICATION SCORE: NOT APPLICABLE — task ini tidak menyentuh source aplikasi
- MODEL: Claude Sonnet 5
- TASK MODE: Eksekusi database dijalankan **pengguna sendiri** di luar sesi agent (bukan `BACKEND MODE`)
- WRITE TARGET: NOT APPLICABLE untuk source. Database: satu tabel (`BilInvoice`), satu baris, dieksekusi pengguna dengan otorisasi eksplisit per pesan
- FILES INSPECTED: Model `BilInvoice.cs`, `BilTender.cs`/`BilSettlement.cs` (kolom `SettledAt` untuk penurunan `ClosedAt`), `BilPaymentAllocation.cs`; migration existing (`20260915074405_RevisiTablePettyCash.cs`) sebagai referensi pola `migrationBuilder.Sql(...)` data-only; `Models/IdentityModel.cs` (nullability `UpdateBy`)
- FILES CHANGED: **NONE** pada source maupun `Migrations/`. Perubahan hanya pada data (satu baris `BilInvoice`).
- IMPLEMENTATION:
  1. **Percobaan pertama** (dengan otorisasi eksplisit pengguna, khusus task ini): `dotnet ef migrations add BackfillClosedInvoicesFromFullySettledFinal`. **Gagal** — build internal yang dipicu perintah itu tidak lulus, sebelum sempat membuat satu file pun (dikonfirmasi `git status` bersih di `Migrations/`, nol file sisa). Root cause build **belum didiagnosis** — di luar scope task ini, lihat `KNOWN ISSUES`.
  2. Pengguna meminta jalur SQL langsung, **tanpa `dotnet ef` sama sekali** — perubahan scope yang disetujui di tengah percakapan.
  3. Agent menyusun skrip SQL baca-tulis (`BEGIN`/dua `UPDATE`/`SELECT` verifikasi, dibungkus transaksi supaya dapat ditinjau sebelum permanen), kriterianya **identik** dengan dry-run `BE-BKC-064` dan rumus `CalculateOutstandingAsync`. Diserahkan ke pengguna sebagai teks, **tidak dieksekusi oleh agent**.
  4. Pengguna menjalankan skrip di tool database miliknya sendiri, meninjau hasil verifikasi, lalu **commit secara eksplisit** — dikonfirmasi lewat pesan langsung ("sudah saya commit").
- **Backend Governance Preflight**: NOT APPLICABLE untuk source (nol source disentuh). Untuk database: `AGENTS.md` bagian Keselamatan Database mensyaratkan eksekusi database sebagai wewenang terpisah dari implementasi source — dipenuhi lewat konfirmasi eksplisit pengguna per langkah (bukan asumsi wewenang tunggal untuk seluruh task).
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: NONE
- DATABASE IMPACT: **Satu baris `BilInvoice` diperbarui** (`Status`, `ClosedAt`, `RowVersion`, `UpdateDateTime`, `UpdateBy`). Nol perubahan skema — tidak ada kolom, index, maupun tabel yang disentuh. **Tidak tercatat di `__EFMigrationsHistory`** karena bukan EF Core migration — lihat `KNOWN ISSUES` butir 2.
- SECURITY IMPACT: NONE
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | `dotnet ef migrations add BackfillClosedInvoicesFromFullySettledFinal` | **Build gagal** | FAILED, ditinggalkan | Ditinggalkan sesuai permintaan pengguna; tidak didiagnosis pada task ini |
  | Eksekusi SQL backfill | **Berhasil — 1 baris** | VERIFIED (dijalankan pengguna) | Screenshot hasil: `Updated Rows: 1`; baris `BIL-20260903-00000004` menjadi `CLOSED`, `ClosedAt = 2026-09-10 11:45:53.693 +0700` |
  | Query verifikasi (`invoices_now_closed`) | **1** | VERIFIED (dijalankan pengguna) | Cocok persis dengan prediksi dry-run `BE-BKC-064` (`candidates_final_zero_outstanding = 1`) |
  | Commit transaksi | **Dikonfirmasi** | VERIFIED (pernyataan eksplisit pengguna: "sudah saya commit") | Perubahan permanen, bukan transaksi terbuka |
- WARNINGS:
  1. **`ClosedAt` terverifikasi benar secara substantif**: nilainya `2026-09-10 11:45:53`, bukan waktu eksekusi script (`2026-09-18`) — membuktikan derivasi dari `BilTender.SettledAt`/`BilPaymentAllocation.CreateDateTime` bekerja sesuai rancangan, bukan sekadar `NOW()`.
  2. Backfill ini **hanya berlaku untuk database tempat dry-run `BE-BKC-064` diukur**. Bila modul ini di-deploy ke database lain (staging/production terpisah), `BE-BKC-064` dan `BE-BKC-065` **MUST diulang** di sana — angka `1` tidak otomatis berlaku di tempat lain.
- KNOWN ISSUES:
  1. **Build `dotnet ef migrations add` gagal, root cause belum didiagnosis.** Ini kemungkinan besar berasal dari error compile pada source `BE-BKC-060`–`063` (enam berkas yang belum pernah dibangun sejak ditulis). **MUST** didiagnosis dan diperbaiki sebelum modul ini dianggap tuntas — task terpisah, di luar scope `BE-BKC-065`.
  2. **Tidak ada jejak di `__EFMigrationsHistory`.** Karena backfill dieksekusi lewat SQL langsung, bukan EF Core migration, tidak ada baris riwayat migration yang mencatat perubahan ini. Siapa pun yang memeriksa riwayat migration di kemudian hari tidak akan melihat jejak backfill ini — satu-satunya jejak adalah laporan ini, `UpdateBy = Guid.Empty` pada baris yang bersangkutan, dan percakapan yang mengonfirmasinya. Bila kelak dibutuhkan jejak formal EF Core, itu keputusan terpisah (migration kosong yang hanya mencatat penanda, dibuat setelah `KNOWN ISSUES` butir 1 selesai).
  3. Backfill ini murni untuk **satu database** (yang dipakai dry-run). Database lain (bila ada) belum tersentuh — lihat WARNINGS butir 2.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: PASS (verifikasi langsung oleh pengguna pada data nyata — bentuk pengujian paling kuat untuk backfill data)
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: Percobaan `dotnet ef migrations add` gagal di tengah jalan (build error internal); dipulihkan dengan mengganti pendekatan ke SQL langsung atas instruksi eksplisit pengguna, bukan mengulang percobaan `dotnet ef` yang sama
- GIT STATUS: Tidak berubah oleh task ini — nol source, nol berkas migration ditambahkan
- NEXT RECOMMENDED STEP: (1) **Diagnosis kegagalan build** — prioritas tertinggi, karena memblokir bukan hanya migration tapi juga validasi `BE-BKC-060`–`063` yang masih "source lengkap, build belum diverifikasi". (2) Setelah build hijau, jalankan verifikasi proses bisnis end-to-end (`BIL-AT-121`–`134`) pada environment ter-autentikasi. (3) Bila modul ini akan di-deploy ke database lain, ulangi `BE-BKC-064`+`065` di sana. (4) Pertimbangkan `KNOWN ISSUES` butir 2 (jejak `__EFMigrationsHistory`) sebagai task terpisah bila dibutuhkan kepatuhan audit formal. Dengan ini, **gelombang `MVP-24` dan `MVP-25` (`BE-BKC-060`–`065`) selesai secara data**, menyisakan satu blocker teknis (build) yang harus ditutup sebelum modul dianggap tuntas.
