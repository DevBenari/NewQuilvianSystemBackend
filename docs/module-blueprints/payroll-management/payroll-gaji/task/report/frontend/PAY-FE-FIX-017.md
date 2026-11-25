# Laporan Perubahan Frontend — `PAY-FE-FIX-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-017` (ad-hoc lanjutan seri `PAY-FE-FIX`) |
| Judul | Form Kebijakan Tunjangan Transportasi: tampilkan masa berlaku Grade/Struktur Gaji terpilih dan petakan penolakan masa berlaku ke field tanggal |
| Slice | Payroll Management → Data Induk → Kebijakan Tunjangan Transportasi (tambah/ubah) |
| Roadmap | `NOT APPLICABLE` (seri ad-hoc) |
| Contract version | Memakai endpoint existing: `GET .../human-resource/master-data/salary-grades/{id}` dan `.../salary-structures/{id}` (izin `Read` yang sama dengan daftar pilihan). **Tanpa perubahan backend** |
| Wewenang UI | Permintaan pemilik 23 Nov 2026 (tangkapan layar galat "Tanggal akhir berlaku kebijakan wajib diisi karena Grade Gaji berakhir pada 01/08/2030" + permintaan menampilkan tanggal berakhir Grade Gaji) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source, tes teks, dan lint selesai; **tampilan belum dilihat** (`VISUAL CHECK: NOT FEASIBLE`) |

## 1. Akar masalah

Galat itu **benar menurut aturan backend** (`TransportAllowancePolicyScopeRules.ValidateWindow`): masa berlaku kebijakan tidak boleh melampaui masa berlaku Grade/Struktur Gaji, dan bila master punya tanggal berakhir maka tanggal berakhir kebijakan wajib diisi. Masalahnya di layar: petugas tidak diberi tahu masa berlaku master sebelum menekan Simpan, kalender tidak dibatasi, dan penolakan hanya tampil sebagai toast (backend menjawab 400 berupa kalimat tanpa daftar `errors`, sehingga tidak ada field yang ditandai).

## 2. Perubahan

- **Revisi 23 Nov 2026 (permintaan pemilik):** informasi masa berlaku tidak lagi berupa kotak peringatan di atas form, melainkan keterangan kecil di bawah field: di bawah **Grade Gaji** ("Masa berlaku Grade Gaji: 01 Agustus 2025 sampai 01 Agustus 2030."), di bawah **Struktur Gaji** (bila dipilih), di bawah **Mulai Berlaku** ("Tidak boleh sebelum …") dan **Selesai Berlaku** ("Wajib diisi dan tidak boleh melewati …"). Komponen kotak informasi dihapus.
- Kalender Mulai/Selesai Berlaku dibatasi mengikuti master (`min`/`max`): mulai = yang paling akhir dari grade/struktur, berakhir = yang paling awal, sama dengan aturan backend.
- Field Mulai/Selesai Berlaku mendapat petunjuk teks tetap.
- Penolakan masa berlaku dari backend kini ditandai pada field tanggal yang bersangkutan (bukan hanya toast). Kalimat lain tidak dipetakan.
- Kegagalan memuat masa berlaku tidak menutup form; informasi tidak tampil dan backend tetap menegakkan aturan saat simpan.

## 3. Berkas

Baru: `src/lib/services/payroll/payroll-transport-policy-scope-window-service.js`, `src/lib/hooks/payroll/master-data/transport-allowance-policy/use-transport-policy-scope-window.js`.
Diubah: `transport-allowance-policy-utils.jsx` (`normalizeScopeWindow`, `resolveScopeWindowLimits`, `applyScopeWindowDateLimits`, `buildScopeWindowNotice`, `mapScopeWindowFieldErrors`), `transport-allowance-policy-constants.jsx` (petunjuk dua field tanggal), `use-master-data-transport-allowance-policy-editor.jsx`, `transport-allowance-policy-form-view.jsx` (pemakai `InformationAlert` dan `payroll-run.module.css` yang sudah ada; tanpa CSS baru), `tests/unit/payroll-transport-allowance-policy.test.mjs` (+2 tes).

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test` pada tes transport policy + saldo awal | **31 lulus, 0 gagal** (tes teks/util; bukan tampilan) |
| `npx eslint` berkas baru dan util | 0 temuan. Hook editor lama masih memuat 2 peringatan `react-hooks/set-state-in-effect` pada baris yang tidak saya ubah |
| `check-css-usage.mjs` pada form view | `OK — 1 berkas diperiksa, 0 temuan` |
| Build / dev server / tampilan | `NOT RUN` / `VISUAL CHECK: NOT FEASIBLE` |

## 5. Risiko

- Pembatasan kalender dan informasi belum dilihat di peramban (perilaku `FilterDatePicker` dengan `min/max`).
- Endpoint detail master butuh izin `SalaryGrade/SalaryStructure : Read`; tanpa izin, informasi tidak tampil (aturan backend tetap berlaku).
- Pada mode ubah, kebijakan lama yang sudah melewati masa berlaku master tetap ditolak backend saat disimpan; layar kini menandai field yang harus dikoreksi.
