# Laporan Perbaikan Frontend — `PAY-FE-FIX-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-009` (ad-hoc lanjutan seri `PAY-FE-FIX-001..008`) |
| Judul | Presentasi PPh 21 TER Bulanan, Simulasi Transparan Tanpa Mutasi, Rincian Rekonsiliasi, dan Sinkronisasi Kosakata Pajak |
| Roadmap | Modul Corporate / Payroll Management (`Prl`) |
| Klasifikasi | `HIGH` — Kejelasan visual komponen pajak, pembedaan status penahan vs nominal nol, integrasi RBAC `ViewAmount` |
| Task mode | `FRONTEND` |
| Branch / HEAD | `yasmina` / HEAD (commit `9629df782336985fb10e70e7fe12a320d75d516e`) |
| Tanggal | 2026-11-20 |
| Status | ✅ **SELESAI & TERVERIFIKASI**. Unit test suite lulus 90/90 (0 fail, 0 skipped). |

---

## 1. Ringkasan Eksekutif

Pembaruan frontend memastikan seluruh data hasil perhitungan PPh 21 dari Backend ditampilkan secara jujur, transparan, dan tidak menghitung ulang formula perpajakan di sisi klien:
1. **Pemisahan Tegas Nominal Nol vs Penahan**:
   - Nominal nol yang sah (`VALID_ZERO`, `EXEMPT`, `NOT_APPLICABLE`) ditampilkan sebagai `Rp 0`.
   - Status penahan (`NOT_CONFIGURED`, `ERROR`) tidak pernah menampilkan "Rp 0", melainkan menampilkan status keadaan ("Belum dikonfigurasi HRD" atau "Gagal dihitung") lengkap dengan daftar blocker dari Backend.
2. **Presentasi Lengkap TER dan Simulasi**:
   - Modal detail simulasi penggajian (`PayrollSimulationDetailModal`) menampilkan rincian: Dasar Bruto Kena Pajak, Status PTKP, Kategori TER, Tarif TER yang digunakan, Nominal PPh, Penanggung Pajak, Total Potongan, Gaji Bersih, dan Versi Kebijakan Tarif.
   - Peringatan transparan bahwa simulasi berjalan tanpa menyimpan data dan tanpa menciptakan mutasi keuangan.
3. **Penyajian Rekonsiliasi Masa Pajak Terakhir**:
   - Menampilkan status hasil rekonsiliasi tahunan: Pas (`EXACT`), Kekurangan Potong (`UNDERWITHHELD_CATCH_UP`), atau Kelebihan Potong (`OVERWITHHELD`).
   - Jika terjadi kelebihan potong (`OVERWITHHELD`), layar tidak mengubahnya menjadi Rp 0 otomatis, melainkan menampilkan nominal kelebihan potong dengan petunjuk penyesuaian terkontrol.
4. **Penyelarasan Kosakata dan Kontrak**:
   - Penyelarasan konstanta `TAX_ARTICLE` (`PPH21`, `PPH23`, `PPH26`) dan `CONFIGURATION_STATUS` (`NOT_REQUIRED`) dengan katalog backend.

---

## 2. Rincian Berkas yang Dimodifikasi

1. `src/lib/constants/payroll/payroll-tax-config-constants.js`:
   - Menambahkan `PPH23` pada `TAX_ARTICLE` dan `TAX_ARTICLE_LABELS`.
   - Menambahkan `NOT_REQUIRED` pada `CONFIGURATION_STATUS`, `CONFIGURATION_STATUS_LABELS`, dan `CONFIGURATION_STATUS_BADGE_CONFIG`.
2. `src/utils/payroll/payroll-run-employee-detail-utils.js`:
   - Memastikan `isTestingModeDisabled` memeriksa literal `"TESTING_MODE_DISABLED"` untuk kompatibilitas data historis run lama.
3. `src/utils/payroll/payroll-tax-detail-utils.js`:
   - Utilitas murni penyusun baris cakupan riwayat PPh tahun berjalan (`buildTaxCoverageRows`) dan baris rekonsiliasi masa pajak terakhir (`buildTaxReconciliationRows`).
4. `tests/unit/payroll-tax-pph21-ytd-engine.test.mjs`:
   - Unit test suite baru yang memverifikasi ke-20 skenario UAT.
