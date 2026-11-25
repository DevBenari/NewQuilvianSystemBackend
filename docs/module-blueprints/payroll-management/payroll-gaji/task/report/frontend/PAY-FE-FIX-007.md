# Laporan Perubahan Frontend — `PAY-FE-FIX-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-007` (ad-hoc, bukan kartu roadmap) |
| Judul | Penghapusan tampilan, route, form, kebijakan, dan notice iuran BPJS dari halaman Payroll dan HRD-Payroll |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas master prompt pemilik 2026-11-20 |
| Dependency | Backend `PAY-BE-FIX-007` (kontrak DTO berubah; keduanya harus dideploy bersamaan) |
| Branch / HEAD | `yasmina` / `9629df782336985fb10e70e7fe12a320d75d516e` (tidak berpindah branch) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai; lint dan uji unit terkait dijalankan; `npm run build` NOT RUN; tampilan di peramban `VISUAL CHECK: NOT FEASIBLE` (dev server dilarang task); `MANUAL TEST: NOT FEASIBLE`** |

## Tabel keputusan base component (UI GATE)

Task ini **hanya menghapus** elemen UI. Tidak ada elemen baru: semua status `REUSE` (komponen yang sudah ada dipakai apa adanya: `InformationAlert`, `DataTable`, `Hero`, `StatusBadge`, `BaseTextField`, `BaseCheckboxField`, `ConfirmModal`). Tidak ada CSS baru, tidak ada class baru. `UI GATE: PASS (tanpa elemen NEW/EXTEND)`.

## Perubahan

### Dihapus
| Berkas | Alasan |
| --- | --- |
| `src/app/payroll/policies/bpjs/page.jsx` | Route Kebijakan Iuran BPJS |
| `src/lib/hooks/payroll/use-payroll-module-mode.js` | Tidak ada pembaca tersisa |
| `src/lib/hooks/system/use-bpjs-aware-config.js`, `src/utils/system/bpjs-feature-config-utils.js` | Hanya melayani form master tunjangan/transport/dashboard workforce |

### Diubah
| Area | Perubahan |
| --- | --- |
| Menu | Hapus "Kebijakan Iuran BPJS"; "PPh dan BPJS" → "Laporan PPh 21"; `adapt-payroll-menu-items` kini hanya menyembunyikan menu klaim COB (HRD/ESS) |
| Kebijakan | `payroll-policy-*` (constants, service, 2 hook, utils, list view, form modal, table columns, css comment): hanya PPh; tanpa `from-setting`, kelompok risiko JKK, status BPJS |
| Profil asuransi | `workforce-insurance-config-editor`, `payroll-tax-config-utils/constants`: hanya asuransi swasta ("Asuransi Swasta"); payload tanpa kolom BPJS |
| Workforce | `workforce-profile-dashboard` (kolom nomor BPJS dihapus), `workforce-resource-editor`, `employee-workforce-editor-config` (field BPJS dihapus); konfigurasi dokter dan pengguna eksternal diperiksa: tidak memuat field BPJS |
| Kesiapan Data | `payroll-data-readiness-*`, `payroll-configuration-status-lines`: tanpa status/notice BPJS atau iuran |
| Profil Penggajian | Detail dan tabel: "Konfigurasi Pajak"; tanpa "Profil Iuran" / "Konfigurasi Iuran" |
| Payroll Run | Banner mode pengujian BPJS → pemberitahuan netral "Finance Handoff produksi ditahan"; rincian penerima: panel "Asuransi", baris historis hanya bila ada |
| Laporan | "Laporan PPh 21"; kolom/kartu "Iuran Pegawai" hanya bila ada nilai (run lama) |
| Master data | Jenis Tunjangan dan Kebijakan Transport tanpa "Tunjangan Tetap" dan "Masuk Dasar Upah BPJS" |
| HR payroll | `payroll-settings-view` tanpa seksi/parameter BPJS; `payroll-service.js` tanpa `calculateBpjs`; layar lama tanpa route (`PayrollEmployeeDetailModal`, approvals, worksheet, runs) dibersihkan dari BPJS |

### Dipertahankan (di luar Payroll)
`use-system-features`, `system-features-*`, `bpjs-feature-guard`, sidebar `useSystemFeatures`, layar HRD Benefit/ESS/K3 beserta guard-nya; pendaftaran, kiosk, penjamin pasien, billing, dan insurance tariff (penjamin pasien BPJS/JKN) **tidak disentuh**.

## Validasi

| Item | Hasil |
| --- | --- |
| `npx eslint` pada 54 berkas yang diubah | 0 galat, 25 peringatan (24 sudah ada: `set-state-in-effect`, `exhaustive-deps`, `no-img-element`; 1 peringatan baru `kind` di `use-payroll-policy-list.js` sudah diperbaiki) |
| `node --test` berkas terkait | `bpjs-feature-deactivation` (ditulis ulang) 12/12, `payroll-data-readiness-no-bpjs` (ditulis ulang) 5/5, `payroll-data-readiness` 19/19, `payroll-expense-report` 18/18, `payroll-policy` 13/13, `payroll-tax-policy-auto-effective` 8/8, `payroll-run-employee-detail` 14/14, `payroll-compensation-profile` 26/26, `payroll-transport-allowance-policy` 6/6, `payroll-tax-config` 29/30 |
| Kegagalan tersisa pada berkas terkait (tidak disebabkan task ini) | `payroll-tax-config` "kosakata kode FE sama dengan katalog BE" (`PPH23` ada di BE, tidak di FE); `payroll-module-contract` 2 (rute di luar `/payroll`, komponen mentah di `workforce-config-status-header.jsx`); `payroll-run-screens` 1 (regex izin menu Payroll Run) |
| Suite penuh `node --test tests/unit/*.test.mjs` | 5230 uji, 5137 lulus, 93 gagal pada berkas modul lain (rawat inap, billing, hemodialisa, bedah, sidebar master data, dst.) dan uji berbasis `cwd`; baseline HEAD murni **tidak** dijalankan ulang, jadi 93 tidak dibandingkan satu per satu |
| `check-css-usage.mjs` | 65 temuan pada 30 berkas; semuanya pada baris lama (class tidak terdefinisi pada dashboard workforce, `BaseButton variant="outline"` pada layar HR lama); task ini tidak menambah baris JSX baru yang melanggar. Target `0 temuan` **tidak tercapai** karena utang lama |
| `npm run build` / dev server | **NOT RUN** (dilarang) |

## Risiko

1. FE dan BE harus dideploy bersamaan (field DTO dihapus/diganti `isFinanceHandoffHeld`).
2. FE gagal-tertutup: bila BE lama menjawab tanpa `isFinanceHandoffHeld`, banner "ditahan" tampil (tidak membuka Finance Handoff).
3. Tampilan belum dilihat di peramban.
