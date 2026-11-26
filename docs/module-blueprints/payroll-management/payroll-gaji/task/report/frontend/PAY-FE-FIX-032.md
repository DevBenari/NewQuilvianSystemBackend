# PAY-FE-FIX-032 — Popup PPh sementara yang proporsional dan aksi Sahkan Bersyarat (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `17d75c43d` (working tree belum di-commit). Backend: PAY-BE-FIX-032. Deploy BE dulu.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild). Status UI: paling tinggi sebagian.

## Revisi 1 — popup PPh sementara
**Akar masalah** (dari CSS): `modal-confirm.css` memberi `.region-confirm-modal .modal-dialog { max-width: 430px }` dan varian wide/lg hanya 550 px, sehingga grid informasi, nominal, dan daftar pegawai terhimpit.

**Perbaikan** (hanya modal ini; ConfirmModal lain dan CSS global tidak diubah):
- Class khusus pada `payroll-run.module.css`: `.pphModal:global(.region-confirm-modal) :global(.modal-dialog)` (lebar `min(100% - space-5, 820px)`, spesifisitas lebih tinggi dari aturan global), `.pphBody` (tinggi maks `90dvh - 7rem`, scroll vertikal, tanpa scroll horizontal; footer saudara body sehingga tombol tetap terlihat), `.pphInfoGrid` (2 kolom, 1 kolom ≤575,98 px), `.pphNowrap` (Rupiah dan nomor run tidak dipecah), `.pphTableWrap`. Semua nilai memakai token `var(--…)`.
- Popup dipindah ke `payroll-run-tax-modals.jsx` (`ReviewSubmitModal`) memakai `ConfirmModal` (`modalClassName`, `bodyClassName`). Susunan: subjudul "Pengajuan untuk pemeriksaan awal — pajak belum final"; A Ringkasan Payroll; B Ringkasan Estimasi; C Pegawai Terdampak (`DataTable` tanpa paginasi: No., Nama, Status PPh, Estimasi PPh, Keterangan); D Peringatan; E alasan wajib + kotak centang + tombol Buka Detail Run; footer Tutup / Ajukan untuk Review. Estimasi tidak tersedia ditampilkan "Estimasi tidak tersedia", bukan Rp 0.

## Revisi 2 — Sahkan Bersyarat
- Tombol **Sahkan Bersyarat** pada Persetujuan Penggajian dan halaman detail: hanya untuk run Diajukan berPPh sementara yang belum disahkan bersyarat dan pengguna berizin Approve; kelayakan diputuskan Backend (`availableActions` memuat `ApproveConditionally`, alasan penahan dari `approveConditionallyBlockReasons`). Tombol **Sahkan** final tetap tidak ditawarkan.
- Modal `ConditionalApproveModal`: ringkasan (Payroll Run, periode, penerima, total penghasilan, **PPh valid (resmi) dari Backend**, **PPh estimasi** terpisah, pegawai terdampak, histori belum lengkap, status pembayaran "Tidak dibuka oleh pengesahan ini"), tabel pegawai (status PPh, PPh resmi, estimasi, keterangan), peringatan (tidak mengunci, tidak mengonfirmasi cicilan Finance, tidak membuat instruksi/slip/laporan PPh), informasi Finance, tindak lanjut wajib, alasan wajib, kotak centang pernyataan, tombol Tutup / Buka Detail Payroll / Sahkan Bersyarat. Hook mengirim `POST /approve` `{ approvalMode: "Conditional", reason, acknowledgeProvisionalTax: true, expectedRowVersion }`; pernyataan tidak pernah dikirim otomatis; tidak optimistis.
- Setelah berhasil: badge "Disahkan Bersyarat" + referensi pada daftar (tambahan, bukan status "Disahkan"), panel "PPh 21 Belum Final" di detail memuat blok "Pengesahan Bersyarat" (referensi, waktu, PPh valid vs estimasi, versi kebijakan, histori belum lengkap, rekonsiliasi "Menunggu pelengkapan histori PPh", tindak lanjut, alasan), riwayat status berlabel "Pengesahan bersyarat (PPh belum final)". Tidak ada badge "PPh Final".
- Tabel pegawai pada detail run: kolom baru **Status PPh** (FINAL_VALID/PROVISIONAL_ESTIMATE/INCOMPLETE_NO_ESTIMATE/NOT_APPLICABLE); kolom "Pajak & Iuran" tidak lagi menampilkan PPh belum final sebagai Rp 0 (hanya iuran + "PPh belum final").

## File
| File | Perubahan |
| --- | --- |
| `src/components/view/payroll/run/payroll-run-tax-modals.jsx` (baru) | `ReviewSubmitModal`, `ConditionalApproveModal` |
| `src/components/view/payroll/run/payroll-run-row-action-modal.jsx` | memakai modal baru, READY_COPY |
| `src/components/view/payroll/run/payroll-run-table-columns.jsx` | badge/tombol, kolom Status PPh |
| `src/components/view/payroll/run/payroll-run-provisional-tax-panel.jsx` | blok pengesahan bersyarat |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx`, `payroll-run-list-view.jsx` | tombol dan izin |
| `src/lib/hooks/payroll/use-payroll-run-row-action.js`, `use-payroll-run-detail.js` | payload, flag |
| `src/lib/constants/payroll/payroll-run-constants.js`, `payroll-constants.js` | aksi, badge, transisi, teks, label riwayat |
| `src/utils/payroll/payroll-run-utils.js` | normalisasi, evaluasi, bagian ringkasan |
| `src/style/payroll/payroll-run.module.css` | class khusus popup |
| `tests/unit/payroll-conditional-approval.test.mjs` (baru, 11 tes); lima berkas tes lama disesuaikan |

## UI GATE
REUSE: `ConfirmModal`, `DataTable`, `StatusBadge`, `BaseCheckboxField`, `BaseButton`, `PayrollNoticeList`, `PayrollActionError`. Satu penambahan CSS Module domain existing (`payroll-run.module.css`) berbasis token karena base component tidak menyediakan lebar khusus; tanpa komponen base baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 599 tes, 594 lulus, 5 gagal: 3 gagal lama (navigasi, layar baru, menu-rute) + 2 (`route dan butir menu terdaftar dengan hak Read`, `layar: halaman, menu, dan pengalihan slip gaji lama`) yang **bukan dari pekerjaan ini**: perubahan Anda yang belum di-commit pada `src/utils/menu-sidebar/menu-items.jsx` (butir menu "Kebijakan Iuran BPJS" dihapus) membuat asersi teks menu tersebut gagal. ESLint: 0 error (61 peringatan lama). `check-css-usage.mjs`: `OK — 6 berkas diperiksa, 0 temuan`.

## Skenario UI — statis
Lebar/class khusus, grid dua kolom → satu kolom, scroll body dengan footer tetap, nowrap Rupiah dan nomor run, DataTable pegawai, alasan + checkbox: VERIFIED STATIC (CSS dan teks source). Sahkan Bersyarat hanya dari aksi Backend, payload, tombol: VERIFIED STATIC. Tampilan, responsif tablet/mobile, scroll nyata: **NOT RUN**.

## Risiko
- Tampilan belum dilihat; BE belum dibuild; kemungkinan perlu penyesuaian lebar/tinggi setelah dilihat di browser.
- Pengesahan bersyarat administratif saja; pembayaran/posting Finance belum diaktifkan (keputusan owner; lihat laporan BE).
- Izin `PayrollRun:ReadTaxSummary` baru perlu dicentang di Akses Role bila Finance akan membaca ringkasan.
