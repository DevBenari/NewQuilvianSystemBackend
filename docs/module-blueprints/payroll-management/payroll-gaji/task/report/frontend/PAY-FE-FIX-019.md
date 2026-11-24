# Laporan Perubahan Frontend — `PAY-FE-FIX-019`

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-019` (ad-hoc lanjutan `PAY-FE-FIX-018`); pasangan `PAY-BE-FIX-019` |
| Judul | Filter dan kolom Grade Gaji / Struktur Gaji pada daftar kebijakan tunjangan Shift, Siaga (On Call), dan Risiko |
| Klasifikasi | `MEDIUM` |
| Wewenang UI | Permintaan pemilik 23 Nov 2026. Semua `REUSE`: `ResourceFilterSelect`, `FilterSelect`, `DataFilter`, `DataTable`; tanpa CSS baru. `UI GATE: PASS (semua REUSE)` |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source, tes teks, lint selesai; **tampilan belum dilihat** (`VISUAL CHECK: NOT FEASIBLE`) |

## Hasil untuk pengguna

Daftar ketiga kebijakan punya filter **Grade Gaji** dan **Struktur Gaji** (sebelum filter Status; Struktur terkunci sampai grade dipilih dan dikosongkan bila grade berganti), serta kolom **Grade Gaji** dan **Struktur Gaji** tepat setelah kolom nama. Grade saja tampil "Semua struktur"; tanpa cakupan (data lama) tampil "-". Bentuknya sama dengan daftar kebijakan transportasi. Kolom Grade Pegawai sengaja tidak ditambahkan (hanya di form dan detail).

## Berkas

Baru: `src/lib/hooks/payroll/master-data/use-allowance-policy-scope-filter-selects.js`, `src/components/view/payroll/master-data/shared/allowance-policy-scope-resource-filter.jsx`.
Diubah: `allowance-policy-scope-fields.js` (filter, default, kunci, reset, kolom), `allowance-policy-scope-utils.js` (`resolveAllowancePolicyStructureLabel`), constants, list hook, dan list view ketiga modul, serta `payroll-allowance-policy-scope.test.mjs` (+3 tes).

## Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test` pada tes cakupan, `payroll-allowance-policies-crud-safety`, `payroll-transport-allowance-policy` | **32 lulus, 0 gagal** (tes teks) |
| `npx eslint` | 0 error; 6 peringatan lama `react-hooks/set-state-in-effect` pada pola efek yang tidak saya ubah |
| `check-css-usage.mjs` (5 berkas) | `OK — 5 berkas diperiksa, 0 temuan` |
| Build / tampilan | `NOT RUN` / `VISUAL CHECK: NOT FEASIBLE` |

## Risiko

Urutan deploy: DDL `PAY-BE-FIX-018` → backend → frontend. Query `salaryGradeId` pada backend lama diabaikan (filter tidak berefek, tidak error). Pilihan filter memuat grade/struktur nonaktif (filter riwayat), sama dengan transportasi.
