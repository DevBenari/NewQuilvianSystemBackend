# PAY-FE-FIX-030 — Pengajuan dengan PPh Sementara: modal, Persetujuan Penggajian, detail run (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina` (dikonfirmasi pemilik; `yasmin` tidak ada di GitHub), HEAD `b05275ecd` (working tree belum di-commit). Backend: PAY-BE-FIX-030 (`Yasmina`, `261bb431`). Deploy BE dulu, lalu FE.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; endpoint baru belum dibuild). Status tugas UI: paling tinggi sebagian.

## Perubahan
- **Modal pengajuan** (`payroll-run-row-action-modal.jsx`): cabang baru status `review` berjudul "Pengajuan dengan PPh Sementara". Ringkasan: Payroll Run, Penerima Terdampak, Penahan PPh, Status Pajak BELUM FINAL, PPh Sementara (Estimasi), Estimasi Gaji Bersih, Metode Estimasi, Versi Kebijakan, Bulan Histori Belum Lengkap (mis. "Januari-November"); teks informasi sesuai keputusan; alasan wajib; tombol Batal, Buka Detail Run, **Ajukan untuk Review**. Bila ada penahan lain, modal tetap "Belum Siap Diajukan" tanpa aksi khusus.
- **Penentuan layak**: `evaluateRunDecision` memberi `review` hanya bila Backend memuat aksi `SubmitForReview` pada `availableActions`; tidak ada pencarian kata "PPh" pada pesan (tes menolak pola itu). Array aksi kosong dari Backend tidak memakai fallback status; `SubmitForReview` tidak punya fallback sama sekali.
- **Hook** (`use-payroll-run-row-action.js`): endpoint `POST /submit` dengan `submissionMode: "ReviewOnly"`, `reason`, `acknowledgeProvisionalTax: true`, `expectedRowVersion`; alasan wajib; satu aksi pada satu waktu.
- **Persetujuan Penggajian** (`payroll-run-table-columns.jsx`): badge tambahan "PPh Belum Final" di bawah status Diajukan, keterangan "Sahkan ditahan: PPh belum final", kolom "PPh Sementara" (nominal estimasi berlabel "Estimasi, belum final"; "Estimasi Tidak Tersedia" bila tidak ada), tombol Sahkan **tidak ditawarkan** untuk run PPh sementara. Tinjau dan Kembalikan tetap.
- **Aksi** (`resolveRunActions`): `canApprove` selalu false bila `isTaxProvisional` atau `canApproveFinal === false`, walau izin Approve ada dan "Approve" tercantum (izin dan kesiapan bisnis dua syarat).
- **Detail run**: panel baru `payroll-run-provisional-tax-panel.jsx` ("PPh 21 Belum Final"): peringatan, ringkasan, jejak pengajuan (waktu, alasan, bulan), langkah berikutnya (lengkapi Saldo Awal PPh, kembalikan, hitung ulang, ajukan normal). Estimasi tidak dijumlahkan ke total resmi.
- Normalisasi aditif gagal-tertutup (`isTaxProvisional`, `canSubmitForReview`, `canApproveFinal`, `provisionalTax`, `reviewOnlySubmission`, per penerima `provisionalTax`).

## File
| File | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-run-constants.js` | aksi, transisi, badge, salinan |
| `src/utils/payroll/payroll-run-utils.js` | normalisasi, `formatTaxMonths`, `buildProvisionalTaxItems`, aksi, evaluasi |
| `src/lib/hooks/payroll/use-payroll-run-row-action.js` | payload review-only |
| `src/components/view/payroll/run/payroll-run-row-action-modal.jsx` | modal review |
| `src/components/view/payroll/run/payroll-run-table-columns.jsx` | badge, kolom, Sahkan ditahan |
| `src/components/view/payroll/run/payroll-run-provisional-tax-panel.jsx` (baru) | panel detail |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | memasang panel |
| `tests/unit/payroll-provisional-tax-review.test.mjs` (baru) | 10 tes |
| `tests/unit/payroll-run-row-actions.test.mjs`, `payroll-run-screens.test.mjs` | asersi disesuaikan ke kontrak baru (Sahkan tidak untuk PPh sementara, `isReview`, tanda tangan `BuildDecisionBlockReasonsAsync`, kunci `canSubmitForReview`) |

## UI GATE
Semua REUSE: `ConfirmModal`, `PayrollInfoGrid`, `PayrollNoticeList`, `PayrollSection`, `InformationAlert`, `StatusBadge`, `BaseButton`, kelas `styles.cellStack`/`modalStack`/`errorActions` yang sudah ada. Tanpa CSS atau base component baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 580 tes, 577 lulus, 3 gagal lama (navigasi, layar baru, menu-rute; sama dengan baseline); 10 tes baru lulus. ESLint (modul run, hook, konstanta, util): 0 error. `check-css-usage.mjs`: `OK — 4 berkas diperiksa, 0 temuan`.

## Skenario (UI terkait) — statis
Modal review hanya saat Backend menyatakan layak: VERIFIED STATIC. Penahan lain tetap "Belum Siap Diajukan": VERIFIED STATIC. Sahkan tidak tersedia (daftar dan aksi): VERIFIED STATIC. Badge dan kolom Persetujuan: VERIFIED STATIC (teks source), tampilan NOT RUN. Panel detail: VERIFIED STATIC. Responsif dan tampilan: NOT RUN.

## Risiko dan butir terbuka
- Tampilan belum dilihat; BE belum dibuild. Run Desember harus dihitung ulang agar opsi review muncul.
- Pengajuan review hanya dari modal daftar (Perhitungan Gaji); halaman detail menampilkan panel tetapi belum punya tombol "Ajukan untuk Review" (di luar permintaan modal; NEEDS CONFIRMATION bila diinginkan).
- Pernyataan "belum final" dikirim dari tombol konfirmasi setelah teks tampil; bukan kotak centang terpisah.
- Tabel pegawai pada detail run belum memuat kolom PPh sementara per penerima (data sudah ada pada `employees[].provisionalTax`).
