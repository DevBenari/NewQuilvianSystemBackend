# PAY-FE-FIX-029 — Jadwal Angsuran Potongan: kontrak Finance AR dan penghapusan Simulasi (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `b05275ecd` (working tree belum di-commit). Backend: PAY-BE-FIX-029 (branch `Yasmina`, HEAD `261bb431`). Deploy BE dulu, baru FE.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; endpoint baru belum dibuild). Status tugas UI: paling tinggi sebagian.

## Perubahan 1 — sumber Finance AR
- Halaman memanggil `restitutionService.getFinanceSchedules` → `GET /hr-deductions/schedules/finance` (cocok dengan `[Route("api/hr-deductions")]` + `schedules/finance` di controller; tes statis).
- Paginasi server-side (`DataTable` + `RegionPagination`), 25 baris/halaman; ganti filter selalu kembali ke halaman 1.
- Filter: Pegawai (`ResourceFilterSelect` resource `employees`), Periode (bulan ±24, nilai `yyyy-MM`), Status angsuran (bawaan **Belum lunas** = `OUTSTANDING`; opsi DIJADWALKAN/TERBAYAR_SEBAGIAN/TERBAYAR/TERTUNGGAK/DIBATALKAN dan "Semua status"). Filter prioritas dihapus (Finance tidak punya prioritas; tidak dikarang).
- Kolom: No (dari `DataTable`), Angsuran (kode, no. piutang, ke-n dari N), Pegawai (nama, NIP/nomor, penanda Dokter), Periode, Terjadwal, Tunggakan Dibawa, Terbayar, Masih Terutang, Status Angsuran + status perjanjian, Payroll (nomor run + status, atau "Belum masuk Payroll Run").
- Ringkasan dari `summary` server (himpunan yang sama dengan tabel): Angsuran, Perjanjian, Total Angsuran Terutang, Total Sisa Pokok Piutang (sekali per piutang), Tertunggak; subjudul mengikuti cakupan filter. Tidak ada label "Total Angsuran / Bulan"; tidak ada penjumlahan ulang di klien.
- Data HRD lama: tabel kedua "Jadwal Potongan HRD Lama" hanya bila `legacyItems` ada; `InformationAlert` bila salinan disembunyikan.
- Loading, galat, kosong, dan tanpa akses dibedakan.

## Perubahan 2 — Simulasi dihapus
Dihapus: tombol hero "Simulasi Batas 50%", tombol dan kolom Aksi "Simulasi" per baris, modal, state, handler, import, `DeductionCapSimulationModal.jsx` (berkas dihapus), `simulateDeductions` (service), thunk/state/reducer simulasi (slice). Deskripsi header tanpa kata simulasi. **Koreksi Salah Potong dipertahankan** (izin `RestitutionClaim`:`Correct`, `PastPeriodCorrectionModal`, toast sama). Dashboard Potongan & Klaim tidak diubah (tetap `getSchedules({scheduleStatus:"Active"})`).

## File
| File | Perubahan |
| --- | --- |
| `src/components/view/hr/restitution/deduction-schedules-view.jsx` | Ditulis ulang |
| `src/utils/hr/deduction-schedule-utils.js` (baru) | Fungsi murni: filter bawaan, parameter, normalisasi, ringkasan, label periode |
| `src/lib/services/hr/restitution-service.js` | + `getFinanceSchedules`, − `simulateDeductions` |
| `src/lib/state/slice/hr/restitution/restitution-slice.js` | − jejak simulasi |
| `src/lib/constants/hr/restitution/restitution-constants.js` | + `INSTALLMENT_STATUS`, filter, label perjanjian |
| `src/components/features/hr/restitution/DeductionCapSimulationModal.jsx` | Dihapus |
| `tests/unit/payroll-deduction-schedules-finance.test.mjs` (baru) | 11 tes |

## UI GATE
Semua REUSE: `Hero`, `BaseButton`, `DataFilter`, `DataTable`, `FilterSelect`, `ResourceFilterSelect`, `SummaryGrid`, `InformationAlert`, `HrStatusBadge`, `ToastStack`, `RegionPagination`, kelas `tableStyles`/`panelStyles` yang sudah ada. Tanpa CSS atau komponen baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 553 tes, 550 lulus, 3 gagal lama (navigasi, layar baru, menu-rute; sama dengan baseline sebelumnya); 11 tes baru lulus. ESLint (5 berkas): 0 error. `check-css-usage.mjs`: `OK — 1 berkas diperiksa, 0 temuan`.

## Skenario (20) — statis
1-4 (angsuran Finance tampil, tanpa sinkronisasi/Payroll Run, dokter, status perjanjian): VERIFIED STATIC di sisi FE; NEEDS RUNTIME UAT (butuh BE). 5-6 (legacy tampil, duplikat disembunyikan): VERIFIED STATIC. 7-9 (ringkasan konsisten, saldo sekali, label tanpa "/ Bulan"): VERIFIED STATIC (tes perilaku). 10-12 (filter periode/status/pegawai): VERIFIED STATIC (tes parameter). 13 (bawaan bukan hanya "Active"): VERIFIED STATIC. 14-16 (tombol/baris/modal simulasi hilang): VERIFIED STATIC. 17 (Koreksi Salah Potong utuh): VERIFIED STATIC. 18 (dashboard tidak regresi): VERIFIED STATIC. 19 (loading/galat/kosong): VERIFIED STATIC. 20 (responsif/tampilan): NOT RUN.

## Risiko dan butir terbuka
- Tampilan belum dilihat; endpoint BE belum dibuild.
- Filter pegawai memakai resource `employees`: dokter tanpa MstEmployee tidak bisa difilter.
- Dashboard masih membaca legacy: NEEDS CONFIRMATION.
- FE dan BE harus dideploy bersamaan (FE memanggil endpoint baru; BE menghapus `engine/simulate`).
