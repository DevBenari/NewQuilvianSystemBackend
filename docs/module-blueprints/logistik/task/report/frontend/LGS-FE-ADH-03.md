# LGS-FE-ADH-03 — Penyesuaian layar persetujuan dengan kewenangan backend (LGS-DEC-002)

- TASK ID: `LGS-FE-ADH-03` (task ad-hoc). Lanjutan `LGS-FE-ADH-02`
- DEPENDENCY: `LGS-BE-ADH-03` (source selesai; belum dikompilasi)
- TASK MODE: FRONTEND (dinyatakan eksplisit oleh pemrakarsa)
- WRITE TARGET: `QuilvianSystemFrontendDev`, branch `Ikbalv2`, HEAD `1e9adc790`
- MODEL: Claude Opus 5.5

## File diubah

- `src/utils/health-services/pharmacy-management/stock-request-approval-utils.js`
  - `resolveDecisionActions` memakai `hasUnitAuthority`, `unitAuthorityMessage`, dan `hasEmergencyAuthority` dari backend.
  - Mengembalikan `notice` tambahan.
  - `resolveStatusLabel` menandai permintaan lama dengan "(aturan peralihan)".
- `src/lib/constants/.../stock-request-constant.jsx`: `STOCK_REQUEST_LEGACY_LABEL`
- `src/lib/hooks/.../use-stock-request-list.jsx`: opsi `fulfillableOnly`
- `src/components/view/.../stock-request-list-view.jsx`: varian gudang memakai `fulfillableOnly`, bukan saringan status Disetujui
- `src/components/view/.../stock-request-detail-view.jsx`: pemberitahuan permintaan lama
- `src/components/view/.../components/stock-request-decision-panel.jsx`: menampilkan `notice`
- `tests/unit/stock-request-approval.test.mjs`: 15 test (sebelumnya 12)

## Perilaku

| Keadaan | Tampilan |
| --- | --- |
| Atasan unit peminta dengan izin Approve | Setujui dan Tolak |
| Berizin Approve tetapi bukan atasan unit peminta | Tanpa tombol; pesan dari backend tampil apa adanya, mis. "Unit layanan … belum ditautkan ke unit organisasi" |
| Petugas CITO berizin eksplisit, bukan atasan unit | Hanya Otorisasi CITO, dengan catatan mengapa Setujui/Tolak tidak ada |
| SuperAdmin tanpa jabatan berizin CITO | Tanpa tombol CITO: `hasEmergencyAuthority` harus bernilai `true`; nilai kosong dianggap tidak berwenang |
| Permintaan lama (aturan peralihan) | Label "Menunggu Persetujuan (aturan peralihan)", pemberitahuan di detail, ikut tampil di antrean gudang |

## Validasi

| Pemeriksaan | Hasil | Catatan |
| --- | --- | --- |
| `npm run build`, `npm run dev` | NOT RUN | Belum ada izin khusus |
| ESLint file yang diubah | PASS, 0 error | 2 warning yang sudah ada sebelumnya: `set-state-in-effect` pada hook daftar dan `preserve-manual-memoization` pada `stock-request-resource-select.jsx` |
| `stock-request-approval.test.mjs` + `logistics-module-navigation.test.mjs` | PASS 23/23 | |
| Test yang menyentuh menu, stok, dan permintaan (593 test) | 23 gagal, sama persis dengan baseline | Tidak ada kegagalan baru |

- MANUAL TEST: NOT FEASIBLE. Dev server belum diizinkan.
- GIT STATUS: belum di-commit.
