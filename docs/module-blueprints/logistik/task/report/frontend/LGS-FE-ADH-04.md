# LGS-FE-ADH-04 — Integrasi frontend finishing persetujuan (LGS-DEC-003 sampai 006)

- TASK ID: `LGS-FE-ADH-04`. Dependency: `LGS-BE-ADH-04` (source selesai, belum dikompilasi)
- WRITE TARGET: `QuilvianSystemFrontendDev`, branch `Ikbalv2`, HEAD `1e9adc790`
- MODEL: Claude Opus 5.5

## Status per task

| Task | Status | Ringkasan |
| --- | --- | --- |
| FE-01 Integrasi | COMPLETED_SOURCE | Kewenangan seluruhnya dari respons backend; halaman verifikasi susulan baru; lencana CITO dan keadaan verifikasi; riwayat verifikasi |
| FE-02 Skema tampilan | COMPLETED_SOURCE | Persetujuan: Header → Ringkasan → Filter → Tabel → Detail → Konfirmasi. Verifikasi CITO: Header → Ringkasan CITO → Filter/Daftar → Detail → Otorisasi Awal → Catatan Pemeriksaan → Benarkan/Catat Temuan → Histori Audit |

## File

Baru:

- `src/components/view/.../stock-request-view/stock-request-emergency-review-view.jsx`
- `src/components/view/.../stock-request-view/components/stock-request-emergency-review-panel.jsx`
- `src/app/health-services/pharmacy-management/stock-requests/emergency-reviews/page.jsx` dan `stock-request-emergency-review-client.jsx`

Diubah:

- `src/lib/constants/.../stock-request-constant.jsx`:
  - route verifikasi susulan;
  - keadaan dan hasil verifikasi (angka sama dengan backend);
  - label "CITO Diotorisasi", "Menunggu Verifikasi Susulan", "Verifikasi Selesai", "Temuan Penyimpangan";
  - label aksi riwayat baru.
- `src/lib/services/.../stock-request.service.js`: `verifyEmergencyStockRequest`
- `src/lib/hooks/.../use-stock-request-detail.jsx`: `verifyEmergency(outcome, note)`
- `src/lib/hooks/.../use-stock-request-list.jsx`: saringan `emergencyReviewState`
- `src/lib/hooks/.../use-stock-request-approval-summary.jsx`: set penghitung dapat dipilih (`EMERGENCY_REVIEW_COUNTERS`)
- `src/utils/.../stock-request-approval-utils.js`:
  - `resolveDecisionActions` mensyaratkan `hasApproveAuthority === true` dan `hasUnitAuthority === true`;
  - `resolveEmergencyReviewActions`, `getEmergencyReviewLabel/Variant`, `isCitoAuthorized`, `isStatusPreservingHistory`.
- `src/components/view/.../stock-request-detail-view.jsx`: lencana CITO, panel verifikasi susulan, riwayat yang tidak memindahkan status
- `src/components/view/.../stock-request-list-view.jsx`: keadaan verifikasi di kolom status; tombol ke halaman verifikasi dari halaman Persetujuan
- `tests/unit/stock-request-approval.test.mjs`: 20 test

Menu tidak diubah. Halaman verifikasi dibuka dari halaman Validasi Atasan, sehingga 67 entri menu (`LGS-DEC-001`) tetap utuh.

## Keadaan yang ditampilkan

| Keadaan | Sumber |
| --- | --- |
| Menunggu Persetujuan, Disetujui, Ditolak, Selesai, Dipenuhi Sebagian, Dibatalkan | `status` dan `isPartiallyFulfilled` |
| CITO Diotorisasi | `emergencyReviewState > 0` |
| Menunggu Verifikasi Susulan, Verifikasi Selesai, Temuan Penyimpangan | `emergencyReviewState` 1, 2, 3 |

Tidak ada keadaan baru yang dibuat di layar.

Penanganan keadaan halaman:

- Loading, kosong, dan error dengan "Coba lagi" tersedia.
- `403` saat memuat ditangani `AccessDeniedGate`.
- `403`, `409`, dan `422` saat bertindak tampil sebagai pesan, lalu detail dimuat ulang.
- Mencoba ulang aman: setiap aksi memakai kunci idempotensi baru, dan aksi yang gagal tidak tersimpan.

## Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run build`, `npm run dev` | NOT RUN |
| ESLint | 0 error; 4 warning dari pola yang sudah ada sebelumnya |
| `stock-request-approval.test.mjs` + `logistics-module-navigation.test.mjs` | PASS 28/28 (dijalankan) |
| Test yang menyentuh menu, stok, dan permintaan (598 test) | 23 gagal, sama persis dengan baseline |

MANUAL TEST: NOT FEASIBLE pada tahap ini.
