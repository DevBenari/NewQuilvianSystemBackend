# Laporan Perubahan Frontend — `RJ-REG-REV-FE-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-005` |
| Judul | Kartu dokter |
| Requirement | `RJ-REG-REV-007` (REV-03) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-DOC-DEC-063`, `RJ-DOC-DEC-064` (pesan merah dipertahankan) |
| Dependency | — |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

Setelah poliklinik dipilih, jadwal dokter pada tanggal kunjungan langsung tampil sebagai **kartu** (inisial,
nama, spesialis, jam · sesi · ruang, kuota bila ada). Klik kartu memilih jadwal (`role="radio"`,
`aria-checked`, centang). Poliklinik yang tidak mewajibkan dokter menampilkan kartu **Tanpa dokter**. Lebih dari
6 kartu → kolom cari. Ganti poliklinik mengosongkan pilihan dokter. Tidak ada dropdown dokter lagi.
Keadaan: belum pilih poli (pesan merah), memuat, kosong (dengan tombol jadwal praktik di step Rujukan), galat
(Coba lagi, existing). Step Data Rujukan memakai kartu yang sama.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/outpatient-registration/outpatient-visit-step.jsx` | `ScheduleSelect` menjadi grid kartu; prop `loading` |
| `src/components/view/…/outpatient-registration/outpatient-referral-step.jsx` | Memakai kartu + `loading` |
| `src/style/…/outpatient-registration.module.css` | `.doctorCard*` (token) |

## 3. Keputusan base component

`UI GATE: 2 elemen — COMPOSE 1, WRAP 1, NEW 0` — kartu pilihan memakai pola kartu jenis kunjungan existing
(tombol kartu ber-`aria-pressed`) di dalam view; tidak ada base "selectable card". `EmergencyInlineAlert` untuk
keadaan.

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| U13 pesan merah sebelum poli dipilih | lulus | `PASS` |
| U14 kartu tampil otomatis setelah poli dipilih | lulus | `PASS` |
| U15 tidak ada dropdown jadwal dokter | lulus | `PASS` |
| U16 kartu terpilih `aria-checked=true` | lulus | `PASS` |
| N11 pendaftaran dengan kartu dokter selesai (`POST 200`) | lulus | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kartu tampil otomatis | Terpenuhi | U14 |
| 2. Tidak ada dropdown dokter kedua | Terpenuhi | U15 |
| 3. Ganti poli mengosongkan pilihan | Terpenuhi lewat kode (`onValueChange` poli existing) | kode |
| 4. Loading/kosong/galat | Terpenuhi lewat kode; kosong/galat tidak dipicu di uji | kode |
| 5. Step Rujukan memakai kartu | Terpenuhi | kode, N8 |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran |
