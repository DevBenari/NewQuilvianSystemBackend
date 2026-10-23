# Laporan Perubahan Frontend — `FE-RWI-263`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-263` |
| Judul | Pulihkan Konfirmasi Masuk dan Pembatalan Reservasi pada Modal Denah |
| Modul / Sub-modul | `rawat-inap` / `episode-rawat-inap` |
| Planning ID | `RPR-026-DEL-20261017-01` |
| Kontrak | `0.13.0`; placement/cancel existing |
| Issue / Fix | `ISSUE-EPS-026` / `FIX-EPS-026-01` |
| Tanggal | 17 Oktober 2026 |
| Status | **✅ Selesai** |

## Outcome

Modal kamar pada denah Monitoring Bed kembali menyediakan **Konfirmasi Masuk** dan **Batalkan Pesanan** pada bed `Reserved/Dipesan`. Kedua tindakan memakai ulang orkestrasi transaksi canonical sehingga permission, refresh data terbaru, optimistic concurrency, idempotensi, kegagalan 409/422, recovery outcome ambigu, serta toast tidak diduplikasi di pop-up.

Konfirmasi pemilik pada 17 Oktober 2026: **"saya sudah coba di testing tadi sudah bisa"**. Sesuai instruksi lanjutan pemilik, pengujian runtime dan `npm run build` berikutnya dilakukan mandiri oleh pemilik; agent fokus pada implementasi source dan perbaikan.

## Keputusan Reuse / Create-New

| Elemen | Keputusan | Bukti implementasi |
| --- | --- | --- |
| Tombol aksi | `BaseButton` — REUSE | Tombol placement/cancel memakai loading, disabled, size, dan variant canonical |
| Dialog | `ConfirmModal` — REUSE | Dialog konfirmasi masuk dan pembatalan beralasan |
| Kegagalan placement | `PlacementFailureList` — REUSE | Seluruh alasan server tetap dapat dirender |
| Recovery dan notifikasi | `BedOperationRecovery`, `ToastStack` — REUSE | Outcome ambigu dan toast mengikuti hook existing |
| Orkestrasi transaksi | `useInpatientBedBoardActions` — REUSE_WITH_ADAPTER | Hasil `rooms[].beds` diratakan menjadi `{ beds }` tanpa mengubah kontrak canonical denah |
| Pop-up kamar | EXTEND | Props presentasional untuk permission, busy/loading, dan callback aksi |
| Base component baru | TIDAK ADA | Tidak dibutuhkan |

## Perubahan Source

1. `floor-plan-monitoring.jsx`
   - memasang `useInpatientBedBoardActions`;
   - menambahkan adapter refresh `rooms[].beds → { beds }`;
   - meneruskan permission, busy/loading, serta callback placement/cancel ke pop-up;
   - merender `ConfirmModal`, `PlacementFailureList`, `BedOperationRecovery`, dan `ToastStack` existing;
   - menutup pop-up kamar setelah transaksi sukses.
2. `floor-plan-room-popup.jsx`
   - hanya menampilkan dua aksi pada mode view ketika `statusCode === "Reserved"`, `episodeId` dan `reservationId` tersedia, serta permission terkait bernilai allowed;
   - loading hanya tampil pada tombol/bed yang dipilih dan seluruh transaksi lain dinonaktifkan saat busy;
   - pop-up denah memakai atribut `hidden` selama dialog transaksi aktif agar dialog Bootstrap tidak tertutup stacking layer denah;
   - focus trap pop-up dijeda saat dialog anak aktif dan fokus dikembalikan ke trigger setelah pembatalan.
3. `tests/unit/floor-plan-monitoring.test.mjs`
   - menambahkan fixture reservasi dengan episode, reservation, dan operational version;
   - mengunci keberadaan kedua tombol, privacy guard, adapter refresh, dialog failure, recovery, toast, alasan wajib, dan koordinasi layer dialog.

Tidak ada perubahan pada source backend, endpoint, DTO, permission, database, migrasi, maupun aturan bisnis.

## Bukti Acceptance Criteria

| AC | Hasil | Bukti |
| --- | --- | --- |
| `RWI-AC-R026-01` | ✅ | Guard `Reserved + episodeId + reservationId + canPlace`; tombol Konfirmasi Masuk |
| `RWI-AC-R026-02` | ✅ | Guard reservasi + `canCancel`; `ConfirmModal requireReason` |
| `RWI-AC-R026-03` | ✅ | Metadata wajib berasal dari `holdingContext`; denied/unknown menghasilkan `canPlace/canCancel=false` |
| `RWI-AC-R026-04` | ✅ | `refreshActions()` menunggu `state.refresh()` lalu meratakan `fresh.rooms[].beds` sebelum hook membuka dialog |
| `RWI-AC-R026-05` | ✅ | Hook existing mengirim `episodeId`, `bedId`, `expectedBedVersion`; callback sukses menutup kamar dan refresh |
| `RWI-AC-R026-06` | ✅ | Penanganan 409/422/outcome ambigu tetap milik hook + failure/recovery canonical |
| `RWI-AC-R026-07` | ✅ | Mode selection tetap memakai cabang awal; cleaning/history/availability existing dipertahankan; tidak ada perubahan CSS denah |
| `RWI-AC-R026-08` | ✅ dengan catatan baseline | Test fokus/lint/build lulus; UAT dikonfirmasi pemilik. Suite unit global masih memiliki failure lama di area lain dan tidak memuat failure pada test task ini |

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint terarah pada 2 source + 1 test task | **LULUS**, 0 error dan 0 warning |
| `node --import ./tests/helpers/register.mjs --test tests/unit/floor-plan-monitoring.test.mjs` | **LULUS**, 3/3 test |
| `npm run lint:errors` | **LULUS** |
| `npm run build` | **LULUS**; Next.js compiled, TypeScript selesai, 498 halaman statis dihasilkan, standalone assets siap |
| `git diff --check` | **LULUS**; hanya peringatan normalisasi LF/CRLF pada worktree Windows |
| `npm run test:unit` global | **BASELINE TIDAK HIJAU** karena test lama di area lain (contoh petty-cash, post-op, surgery-booking, ward-pre-op); test `floor-plan-monitoring` task ini tetap 3/3 lulus |
| UAT runtime | **LULUS menurut pemilik sistem**: "sudah bisa" (17 Oktober 2026) |

## Batas Perubahan dan Catatan Worktree

- File CSS denah tidak diubah oleh task ini; perubahan existing pada file tersebut milik pekerjaan lain tetap dipertahankan.
- Perubahan existing pada master-room popup, hook admisi transfer, dan floor-plan layout tidak disentuh/ditimpa.
- Tidak ada commit, push, merge, tag, deploy, atau perubahan backend.

