# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-010` |
| Judul | Layar Daftar Pasien Rawat Jalan (baca) dan butir menu |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `11.2` |
| Trace | `RJ-DOC-FE-005`, `006`, `007`, `009`; `RJ-DOC-DEC-012`..`014`; desain `03-frontend-architecture.md` DP-FE.1–DP-FE.7 |
| Kontrak | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — `GET /`, `GET /summary`, `GET /filters/metadata` |
| Dependency | `[BE] RJ-DOC-REV-BE-009` ✅ |
| Wewenang | `RJ-DOC-DEC-025` |
| Repository / branch | `V2QuilvianSystemFrontendDev` @ `sukmagpV2`, baseline `b7e9b7fd4` (bersih sebelum task) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI |

## 1. Keputusan base component

Modul referensi visual: `administrator/master-data/bank` (pola halaman list baku), dengan pola
hook + service dari layar registrasi (`nurse-station-queue`, `doctor-queue`) sesuai DP-FE.6.

| Elemen | Base component | Status | Bukti |
| --- | --- | --- | --- |
| Kepala halaman + label cakupan | `Hero` (`eyebrow`) | `REUSE` | `hero.jsx` |
| Lima kartu ringkasan | `SummaryGrid` | `REUSE` | `summary-grid.jsx` |
| Kartu Menggantung yang dapat diklik | `SummaryGrid` prop `href` (sudah ada, merender `Link` + `summaryCardInteractive`) | `REUSE` | `summary-grid.jsx:241-306` |
| Pilihan tampilan Hari Ini / Aktif / Menggantung / Rentang | `DataFilter` prop `tabs` | `REUSE` | `data-filter.jsx:382-400` |
| Pencarian, reset | `DataFilter` | `REUSE` | — |
| Saringan klinik, dokter, status, jumlah baris | `FilterSelect` (opsi pertama bernilai kosong, konvensi `BANK_STATUS_OPTIONS`) | `REUSE` | — |
| Tanggal awal/akhir (mode rentang) | `FilterDatePicker` | `REUSE` | — |
| Tabel + paginasi + memuat/kosong | `DataTable` + `Pagination` | `REUSE` | — |
| Sel dua baris (pasien/No. RM, no. kunjungan/Menggantung) | Kelas `regionNameCell`, `primaryCell`, `supportingCell` | `REUSE` | `administrator-region-table.module.css:59` |
| Status kunjungan | `StatusBadge` (`pending`/`info`/`active`/`inactive`) | `REUSE` | — |
| Rentang belum lengkap, galat | `InformationAlert` (`info`, `danger`) | `REUSE` | — |
| Akses ditolak / tanpa cakupan | `AccessDeniedGate` | `REUSE` | `access-denied-utils.jsx` (mengenali 401/403) |

`UI GATE: 12 elemen — REUSE 12, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

Tidak ada berkas style baru; tidak ada nilai visual literal.

## 2. Berkas

| Berkas | Status |
| --- | --- |
| `src/app/health-services/registration-management/outpatient-encounters/page.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-list-client.jsx` | Baru — `Suspense` untuk `useSearchParams` |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-list-view.jsx` | Baru |
| `src/components/view/health-services/registration-management/outpatient-encounters/outpatient-encounter-table-columns.jsx` | Baru |
| `src/lib/hooks/health-services/registration-management/outpatient-encounters/use-outpatient-encounter-list.jsx` | Baru |
| `src/lib/services/health-services/registration-management/outpatient-encounter.service.js` | Baru |
| `src/lib/constants/health-services/registration-management/outpatient-encounter-constants.js` | Baru |
| `src/utils/health-services/registration-management/outpatient-encounter/outpatient-encounter-display-utils.js` | Baru |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui — satu butir setelah "Skrining Pasien", `requiredPermission { OutpatientEncounter, Read }` |

**Perilaku utama:**
- Mode bawaan Hari Ini.
- Kartu Menggantung menuju `?view=hanging`, dan tab disinkronkan ke URL.
- Mode rentang tidak memanggil API sebelum kedua tanggal terisi.
- Filter Dokter hanya dirender bila `scope.canReadAll`.
- Status "sedang memuat" diturunkan dari kunci request, sehingga tidak ada `setState` sinkron di effect.
- 401/403 dari server diteruskan ke `AccessDeniedGate` beserta pesan backend.

**Delta terhadap desain:** tidak ada.
- Kolom yang dipakai: No, Tanggal, No. Kunjungan (+ penanda Menggantung), Pasien / No. RM, Klinik, Dokter, Penjamin, Status. Ini masuk `DEV_DISCRETION` (`RJ-DOC-FE-009`).
- Kolom aksi Batalkan menyusul di `FE-011`.

## 3. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| `npx eslint <8 berkas baru> src/utils/menu-sidebar/menu-items.jsx` | `0 error, 0 warning`. Tiga warning `react-hooks/set-state-in-effect` pada draf pertama diperbaiki |
| `next build` | `PASS` — `✓ Compiled successfully`; route `/health-services/registration-management/outpatient-encounters` terbentuk |
| Grep anti-regresi `ui-consistency-checklist` | Tidak ada `<button>`/`btn-*`, `<table>` mentah, `fw-*`/`fs-*` di berkas JSX baru; tidak ada berkas style baru |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — perilaku diverifikasi lewat uji layar di bawah |

### Verifikasi layar (Playwright headless, login lewat layar)

**Lingkungan:**
- Backend Release di `https://localhost:7185` dengan CORS tambahan `http://localhost:3001` lewat environment.
- Frontend hasil `next build` dengan `NEXT_PUBLIC_API_QUILVIAN` diarahkan ke 7185, dijalankan `next start -p 3001`.
- Port 3000/7184 dipakai server dev pemilik dan tidak disentuh.
- Akun: SuperAdmin dan akun uji `UJI-RJDP` dari `BE-009`.
- Tangkapan layar ada di scratchpad sesi (`fe010-*.png`).

| ID | Skenario | SuperAdmin | Dokter uji | Perawat uji | Tanpa cakupan |
| --- | --- | :---: | :---: | :---: | :---: |
| L | Login lewat layar | PASS | PASS | PASS | PASS |
| M | **AT-DP-23** butir menu tepat di bawah "Skrining Pasien" (pencarian menu sidebar) | PASS | PASS | PASS | PASS |
| H | Hero + label cakupan ("Semua klinik" / "Pasien Anda" / "Klinik cluster Anda") | PASS | PASS | PASS | — |
| T | Tab bawaan Hari Ini, request `mode=today` | PASS | PASS | PASS | — |
| F | Filter Dokter hanya untuk lihat semua | PASS¹ | PASS | PASS | — |
| S | Lima kartu ringkasan | PASS | PASS | PASS | — |
| E | Tabel berisi / keadaan kosong | PASS (8 baris) | PASS (6) | PASS (1) | — |
| G | Kartu Menggantung → `?view=hanging`, tab aktif, request `hangingOnly=true` | PASS (20 baris, total 141) | PASS (1) | PASS | — |
| C | Cari `ENC-RSMMC-00146` di tab Menggantung | PASS | — | — | — |
| R | Tab Rentang tanpa tanggal → pesan info, tanpa panggilan API | PASS | — | — | — |
| B | Buka ulang tanpa query → Hari Ini | PASS | — | — | — |
| D | **AT-DP-04** layar akses ditolak berisi "Akun Anda belum terhubung…" (API `403` ×3) | — | — | — | PASS |

¹ **Catatan F.** Deteksi otomatis sempat keliru dua arah:
- Tombol judul kolom tabel dan tombol grup menu sidebar "Dokter" ikut terhitung (diketahui lewat penelusuran DOM).
- Setelah dikecualikan, pemicu `FilterSelect` ternyata bukan `<button>`.

Kebenarannya dipastikan dari tangkapan layar: filter Dokter tampil untuk SuperAdmin (`fe010-superadmin-today.png`) dan tidak tampil untuk dokter/perawat (`fe010-dokter-today.png`, `fe010-perawat-today.png`).

**Tambahan AT-DP-23 (akun uji `UJI-RJDP Tanpa Hak`, jabatan Staff Finance tanpa hak `OutpatientEncounter`):**
- Butir menu tidak tampil: PASS.
- Membuka URL langsung menampilkan layar "Akses Ditolak": PASS.

**Keadaan kosong dan gagal (SuperAdmin):**
- K1, kosong: tab Aktif + pencarian `ZZZTIDAKADA999` menampilkan "Data kunjungan rawat jalan tidak ditemukan." dan "Tidak ada kunjungan aktif pada saringan ini.": PASS.
- K2, gagal: respons daftar dipaksa `500` lewat route interception Playwright, dan pesan server tampil di `InformationAlert` merah: PASS.

**Perbaikan selama verifikasi:** Run pertama memperlihatkan dua cacat tampilan, dan keduanya sudah diperbaiki sebelum run akhir:
- Nama pasien dan No. RM menempel. Diperbaiki dengan memakai `regionNameCell`.
- Kolom Status terpotong di lebar 1440 px. Diperbaiki dengan merampingkan `minWidth`.

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AT-DP-23` menu di bawah Skrining Pasien; tanpa `Read` tidak tampil | Terbukti | M ×4, tambahan tanpa hak |
| Skema DP-FE.3 empat keadaan (memuat, kosong, gagal, berisi) + `403` | Terbukti: memuat (tangkapan perawat), berisi (E), `403` (D), kosong (K1), gagal (K2) | Tangkapan layar `fe010-*.png` |
| Mode bawaan Hari Ini | Terbukti | T |
| Filter Dokter hanya `scope.canReadAll` | Terbukti | F + tangkapan layar |

## 5. Risiko dan catatan

1. ~~Build produksi di `.next` menunjuk API `https://localhost:7185`.~~ Sudah dikembalikan: `npm run build` final pada `FE-011` memakai `.env` asli.
2. **Server dev pemilik belum memuat perubahan.** Backend Debug di port 7184 dijalankan sebelum task ini, jadi belum memuat endpoint baru. Fitur baru terlihat setelah backend itu di-restart atau di-build ulang.
3. Data uji tambahan: pegawai `UJI-RJDP Tanpa Hak` (jabatan Staff Finance, hak jabatan tidak diubah).

## 6. Task berikutnya

`RJ-DOC-REV-FE-011` — pembatalan dari layar.
