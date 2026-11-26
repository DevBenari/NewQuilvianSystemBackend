# Laporan Perubahan Frontend — `PAY-FE-FIX-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-018` (ad-hoc lanjutan seri `PAY-FE-FIX`); pasangan `PAY-BE-FIX-018` |
| Judul | Form kebijakan tunjangan Shift, Siaga (On Call), dan Risiko: Grade Gaji → Struktur Gaji → Grade Pegawai, seperti kebijakan transportasi |
| Slice | Payroll Management → Data Induk → Kebijakan Tunjangan (Shift, Siaga, Risiko), halaman tambah, ubah, dan detail |
| Roadmap | `NOT APPLICABLE` (seri ad-hoc) |
| Contract version | Mengonsumsi field aditif `PAY-BE-FIX-018` (`salaryGradeId`, `salaryStructureId`, `employeeGradeId` + nama). **Bergantung pada DDL pemilik dan backend yang dibuild** |
| Wewenang UI | Permintaan pemilik 23 Nov 2026 (tangkapan layar form transportasi sebagai acuan bentuk). Tanpa komponen/CSS baru: `REUSE` `BaseEditorView`, `InformationAlert`, registry select `salaryGrades`/`salaryStructures`/`employeeGrades` |
| Klasifikasi | `HEAVY` |
| Task mode | `FRONTEND` |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source, tes teks, dan lint selesai; **tampilan belum dilihat** (`VISUAL CHECK: NOT FEASIBLE`) |

## 1. Hasil untuk pengguna

Ketiga form (Shift, Siaga, Risiko) kini memiliki, sesudah Tipe Kepegawaian, tiga field seperti pada transportasi:
1. **Grade Gaji** (opsional; kosong = semua grade).
2. **Struktur Gaji** (opsional, terkunci sampai grade dipilih, hanya struktur aktif milik grade itu; kosong = semua struktur grade tersebut). Berganti grade mengosongkan struktur.
3. **Grade Pegawai** (opsional; kosong = semua grade).

Sesudah grade/struktur dipilih tampil keterangan masa berlaku di bawah field Grade/Struktur Gaji dan kedua tanggal, dan kalender Mulai/Selesai Berlaku dibatasi mengikutinya (sama dengan `PAY-FE-FIX-017`, revisi keterangan per-field); galat masa berlaku dari backend ditandai pada field tanggal. Halaman detail menampilkan **nama** Grade Gaji, Struktur Gaji, dan Grade Pegawai.

## 2. Berkas

Baru: `src/lib/constants/payroll/master-data/allowance-policy-scope-fields.js`, `src/utils/payroll/master-data/allowance-policy-scope-utils.js`, `src/components/view/payroll/master-data/shared/allowance-policy-scope-window-notice.jsx`, `tests/unit/payroll-allowance-policy-scope.test.mjs`.
Diubah: `{shift,on-call,hazard}-allowance-policy-constants.jsx` (field, payload, validasi UUID, rincian, reset struktur), editor hook ketiganya (`fields`, `optionHandlerMap`, informasi masa berlaku), form view ketiganya (`optionHandlerMap`, `afterHero`), dan form view transportasi (kini memakai komponen informasi bersama; perilaku tidak berubah).

## 3. Gerbang UI

Semua elemen `REUSE` (field select lewat registry yang sudah ada, `InformationAlert`, `payroll-run.module.css` yang sudah ada); tidak ada `NEW`/`EXTEND`, tidak ada CSS Module baru. `UI GATE: PASS (semua REUSE)`.
`check-css-usage.mjs` pada 5 berkas JSX form/notice: `OK — 5 berkas diperiksa, 0 temuan`.

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-allowance-policy-scope.test.mjs` | **6 lulus, 0 gagal** (tes teks: urutan field, payload null, reset struktur, label tersimpan, kecocokan entity/DTO/controller/konfigurasi dan DDL laporan BE-018) |
| Tes terkait kebijakan tunjangan lain (`payroll-allowance-policies-crud-safety`, `payroll-transport-allowance-policy`, `payroll-master-hidden-currency-code`, `bpjs-feature-deactivation`, `payroll-module-contract`) | 66 lulus, **2 gagal** pada `payroll-module-contract`: rute menu di luar `/payroll/` dan komponen mentah di `workforce-config-status-header.jsx`. Keduanya di luar berkas yang saya ubah (entri menu saya berada di `/payroll/opening-tax-balances`); **belum dibandingkan terhadap HEAD bersih** |
| `npx eslint` pada berkas baru/diubah | 0 error; 6 peringatan `react-hooks/set-state-in-effect` pada pola efek lama di hook editor/detail (tidak saya ubah) |
| Build / dev server / tampilan | `NOT RUN` / `VISUAL CHECK: NOT FEASIBLE` |
| `MANUAL TEST` | `NOT FEASIBLE` — DDL belum diterapkan dan backend belum dibuild |

## 5. Risiko dan batas

- **Urutan deploy: DDL (laporan `PAY-BE-FIX-018` §3) → backend → frontend.** Backend yang jalan sebelum DDL membuat daftar/detail/simpan ketiga kebijakan gagal.
- Cakupan ini hanya disimpan dan divalidasi; mesin payroll belum memakai Shift/Siaga/Risiko (PAY-BE-FIX-012), jadi gaji tidak berubah.
- Filter daftar berdasarkan Grade/Struktur Gaji untuk ketiga kebijakan belum dibuat (hanya form dan detail).
- Kolom tabel daftar belum menampilkan cakupan baru.
- Tidak ada penguncian/larangan mengosongkan grade dan tidak ada cek tumpang tindih (khas transportasi karena sudah punya konsumen).
