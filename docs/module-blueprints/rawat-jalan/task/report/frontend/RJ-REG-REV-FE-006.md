# Laporan Perubahan Frontend — `RJ-REG-REV-FE-006`

> **Dicabut 15 Okt 2026.** Fitur pada laporan ini sudah dihapus atas keputusan pemilik; lihat
> [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian 7. Isi di bawah dipertahankan sebagai catatan historis.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-006` |
| Judul | Menu Tracking Jadwal Booking |
| Requirement | `RJ-REG-REV-008` (REV-10) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-007` |
| Dependency | `RJ-REG-REV-BE-005` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

Menu baru **Rawat Jalan → Tracking Jadwal Booking** (`OutpatientAppointment : Read`).
- Daftar: Hero, DataFilter (cari, tanggal awal/akhir — bawaan hari ini s.d. +30 hari, poliklinik, dokter,
  status, jumlah baris, reset), DataTable (tanggal & jadwal, pasien, poli/dokter, antrean, status
  `StatusBadge` + "Dijadwal ulang n×"), paginasi server.
- Aksi baris: **Detail** dan **Ubah Jadwal** (hanya bila server `canReschedule` dan izin `Reschedule`).
- Modal Ubah Jadwal: tanggal baru, kartu jadwal dokter poli yang sama yang menerima appointment pada tanggal itu,
  alasan wajib (≤ 250). Galat backend (kuota, bentrok, sudah datang) tampil di modal.
- Detail (`BaseDetailView`): data booking + tabel riwayat perubahan (waktu, tanggal/dokter/antrean lama → baru,
  alasan, pelaku); tombol Ubah Jadwal di hero.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/…/outpatient-appointments/page.jsx`, `[id]/page.jsx` | Route baru |
| `src/components/view/…/outpatient-appointments/*.jsx` | List, detail, kolom, modal reschedule |
| `src/lib/hooks/…/outpatient-appointments/use-outpatient-appointments.jsx` | List, detail, reschedule |
| `src/lib/services/…/outpatient-appointment.service.js` | Service |
| `src/lib/constants/…/outpatient-appointment-constants.js` | Konstanta |
| `src/utils/…/outpatient-appointment/outpatient-appointment-utils.jsx` | Tanggal lokal, validasi, filter jadwal |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir menu |

## 3. Keputusan base component

`UI GATE: 8 elemen — REUSE 7, COMPOSE 1, NEW 0` — `Hero`, `DataFilter`, `FilterDatePicker`, `FilterSelect`,
`DataTable`, `RowActionMenu`, `StatusBadge`, `BaseDetailView`, `ConfirmModal`, `BaseDateField`,
`BaseTextAreaField`; kartu jadwal = COMPOSE dengan kelas kartu `FE-005`.

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| T1 daftar memuat booking uji "Belum Datang" | lulus | `PASS` |
| T2 menu di sidebar | lulus | `PASS` |
| T3 validasi tanggal/jadwal/alasan | lulus | `PASS` |
| T4 kartu jadwal untuk tanggal terpilih | lulus | `PASS` |
| T5 reschedule `POST 200` | lulus | `PASS` |
| T6 detail menampilkan riwayat | lulus | `PASS` |
| T7 390 px tanpa scroll horizontal halaman | lulus | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000. Perbaikan sesudah uji visual: tabel riwayat tanpa footer paginasi palsu ("0 dari 0"); isi modal rata kiri.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Menu sesuai izin | Terpenuhi (`requiredPermission`; tanpa izin lewat kode) | T2 |
| 2. Detail terbuka | Terpenuhi | T6 |
| 3. Reschedule valid tersimpan dan tampil | Terpenuhi | T5, T6 |
| 4. Penolakan backend tampil | Terpenuhi lewat kode (`submitError`); BE R5 | kode |
| 5. Tidak ada antrean ganda | Terpenuhi | BE R9, R11 |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | Admin mencentang `OutpatientAppointment : Read/Reschedule`; UAT |
