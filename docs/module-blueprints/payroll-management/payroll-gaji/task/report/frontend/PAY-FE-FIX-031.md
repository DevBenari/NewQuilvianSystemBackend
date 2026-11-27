# PAY-FE-FIX-031 — Modal pengajuan review, Hitung Ulang untuk Review, dan aksi pada halaman detail (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `323330779` (working tree belum di-commit). Backend: PAY-BE-FIX-031 (`Yasmina`, `42798e42`). Deploy BE dulu.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild; run PR-202612-E08FC5 tidak diakses). Status UI: paling tinggi sebagian.

## Perubahan
- **Kondisi A** (siap normal): tidak berubah ("Ajukan Payroll Run?", "Ya, Ajukan").
- **Kondisi B** ("Ajukan dengan PPh Sementara"): ringkasan (run, penerima terdampak, penahan PPh, BELUM FINAL, estimasi PPh/gaji bersih, metode, versi kebijakan, bulan histori belum lengkap), **daftar pegawai terdampak** dengan estimasi masing-masing, alasan wajib, **kotak centang** "Saya memahami bahwa PPh masih sementara dan Payroll Run belum dapat disahkan." (tombol Ajukan untuk Review nonaktif sampai dicentang; `acknowledgeProvisionalTax` tidak pernah dikirim otomatis), tombol Batal / Buka Detail Run / Ajukan untuk Review.
- **Kondisi C** ("Perhitungan Payroll Perlu Diperbarui"): muncul bila Backend mengirim `reviewRecalculationRequired` (kode RECALCULATION_REQUIRED_FOR_TAX_REVIEW). Teks penjelasan dampak (memperbarui hasil dan snapshot aktif, tanpa histori fiktif), tombol Tutup / Buka Detail Run / **Hitung Ulang untuk Review**. Tombol nonaktif dengan penjelasan bila pengguna tanpa izin `PayrollRun:Calculate` atau Backend tidak menyatakan Calculate tersedia. Alurnya: konfirmasi → `POST /calculate` (endpoint existing, `expectedRowVersion`) → **detail lama dibuang dan diambil ulang** → evaluasi ulang → modal B bila Backend menyatakan layak. Tidak ada hitung diam-diam saat modal dibuka atau saat GET; tidak optimistis; satu aksi pada satu waktu (lock).
- **Kondisi D**: tetap "Belum Siap Diajukan"; alasan memakai `submitForReviewBlockReasons` (bila ada) alih-alih alasan pengajuan normal.
- **Halaman detail run**: tombol "Hitung Ulang untuk Review" dan "Ajukan untuk Review" memakai hook dan modal yang sama dengan daftar; tampil hanya dari `canRecalculateForReview` / `canSubmitForReview` yang diturunkan dari `availableActions`, `reviewRecalculationRequired`, dan izin Calculate/Submit. Setelah aksi, detail dimuat ulang.
- Normalisasi aditif gagal-tertutup: `submitForReviewBlockCodes`, `reviewRecalculationRequired`, `taxReviewState`.

## File
| File | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-run-constants.js` | teks B/C, kode alasan, transisi calculate |
| `src/utils/payroll/payroll-run-utils.js` | normalisasi, status `recalculate`, alasan review-only |
| `src/lib/hooks/payroll/use-payroll-run-row-action.js` | `recalculateForReview`, pernyataan wajib, izin Calculate |
| `src/components/view/payroll/run/payroll-run-row-action-modal.jsx` | kondisi B (`ReviewSubmitModal`) dan C |
| `src/lib/hooks/payroll/use-payroll-run-detail.js` | `canSubmitForReview`, `canRecalculateForReview` |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | tombol dan modal bersama |
| `tests/unit/payroll-review-recalculation.test.mjs` (baru) | 8 tes |
| `tests/unit/payroll-provisional-tax-review.test.mjs`, `payroll-run-row-actions.test.mjs` | asersi disesuaikan (penilaian terstruktur; tanda tangan `BuildDecisionBlockReasonsAsync`) |

## UI GATE
Semua REUSE: `ConfirmModal`, `BaseCheckboxField`, `PayrollInfoGrid`, `PayrollNoticeList`, `PayrollActionError`, `BaseButton`, kelas existing. Tanpa CSS atau komponen baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 588 tes, 585 lulus, 3 gagal lama (navigasi, layar baru, menu-rute; sama dengan baseline); 8 tes baru lulus. ESLint: 0 error (2 peringatan lama pada `use-payroll-run-detail.js`, bukan dari perubahan ini). `check-css-usage.mjs`: `OK — 2 berkas diperiksa, 0 temuan`.

## Skenario (UI) — statis
Modal C untuk snapshot lama, B dengan checkbox dan daftar pegawai, D dengan alasan review-only: VERIFIED STATIC (teks source dan tes perilaku utilitas). Hitung ulang → ambil ulang detail: VERIFIED STATIC (urutan dalam hook). Tombol detail dari aksi Backend: VERIFIED STATIC. Alur nyata PR-202612-E08FC5, tampilan, responsif: **NOT RUN**.

## Risiko
- Tampilan belum dilihat; BE belum dibuild; efek pada run asli belum dibuktikan.
- Tabel pegawai pada detail run belum punya kolom PPh sementara per penerima (data ada pada `employees[].provisionalTax`; daftar pegawai terdampak ada di modal dan panel).
- Peringatan: pengguna tanpa izin Calculate hanya mendapat penjelasan; petugas berwenang harus menghitung ulang.
