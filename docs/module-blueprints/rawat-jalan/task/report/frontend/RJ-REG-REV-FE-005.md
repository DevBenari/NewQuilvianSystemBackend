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

## Revisi 2026-10-15 — kartu jadwal dokter bergaya kartu pilih DPJP + kuota di pojok kanan atas

Masukan user: kartu Jadwal Dokter pada step Data Kunjungan dibuat seperti kartu pilih DPJP Rawat Inap, dan kuota ditampilkan di pojok kanan atas.

| Hal | Isi |
| --- | --- |
| Berkas | `outpatient-visit-step.jsx` (komponen lokal `DoctorScheduleCard`); `outpatient-registration.module.css` (blok kartu jadwal dokter) |
| Perubahan | Kartu kini menampilkan foto dokter 76×92 dari `doctorPhotoUrl`. Bila foto tidak ada, tidak aman, atau gagal dimuat, kartu menampilkan inisial. Di sebelah foto: nama, spesialis berwarna primer, detail jadwal, dan tombol aksi selebar kartu (*Pilih Jadwal* / *✓ Terpilih*; *Pilih Tanpa Dokter* untuk opsi tanpa dokter). Badge `Kuota N` berada di pojok kanan atas. Grid kartu memakai lebar minimum 280 px. Interaksi `radiogroup`/`radio`, pencarian, dan nilai form tidak berubah. |
| Keputusan base component | Tata letak kartu: `COMPOSE`, meniru `InpatientDoctorSelectionCard` dengan token design system. CSS Rawat Inap tidak dipakai karena berisi nilai literal. Badge kuota: `StatusBadge status="info"` `REUSE`. Foto/inisial: pola `outpatient-schedule-card.jsx` `REUSE`. `UI GATE: PASS` (tanpa komponen base baru) |
| Kompatibilitas | Modal *Reschedule Appointment* masih memakai `.doctorCard`, `.doctorCardBody > strong/small`, dan `.doctorCardCheck`. Aturan itu dipertahankan, jadi tampilan modal tidak rusak. Kartunya hanya ikut lebar grid baru. |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| Grep anti-regresi (warna literal, `rgba`, `!important`, `font-size`/`font-weight`) pada blok CSS yang diubah | kosong — `PASS` |
| MANUAL TEST | Belum diuji di layar. User perlu memeriksa step Data Kunjungan (foto/inisial, badge kuota, pilih/terpilih, pencarian bila lebih dari 6 jadwal) dan modal Reschedule. |
| AUTOMATED TEST | `SKIPPED (opsional)` — perubahan tampilan |

### Revisi lanjutan 2026-10-15 — subtitle kartu hanya jam praktik

Atas masukan user, subtitle kartu jadwal dokter kini hanya menampilkan jam praktik (`timeLabel`, contoh `13:00 - 16:00`). Sesi, ruang, dan nomor ruang tidak lagi tampil di kartu, tetapi tetap ikut dalam pencarian (`searchText`). Opsi *Tanpa dokter* tetap menampilkan keterangannya. ESLint berkas tersentuh exit 0 — `PASS`. MANUAL TEST belum dilakukan.

### Revisi lanjutan 2026-10-15 — badge kuota tidak menabrak teks, kartu responsif

Masukan user: badge kuota menimpa nama dokter yang panjang (contoh *dr. Maya Permata Sari*), dan kartu harus responsif di semua resolusi.

| Hal | Isi |
| --- | --- |
| Penyebab | Badge diposisikan absolut. Ruang yang disediakan di kanan nama (`--space-8`, 32 px) lebih sempit daripada badge (sekitar 70 px). |
| Perubahan JSX | Badge dipindah ke dalam baris nama (`.doctorCardTitleRow`) dan mengikuti alur normal, bukan absolut. |
| Perubahan CSS | Baris nama memakai `flex-wrap`, sehingga badge turun ke bawah nama bila ruang tidak cukup. Nama, spesialis, dan jam memakai `overflow-wrap: anywhere`. Grid memakai `minmax(min(260px, 100%), 1fr)` agar kartu tidak melebar melewati layar sempit. Pada lebar ≤480 px, foto mengecil menjadi 56×68 px dan padding kartu dirapatkan. Tombol aksi boleh dua baris. Kelas `.doctorCardMetaQuota` dihapus. |
| Uji tata letak headless (Chromium, CSS asli `globals.css` + `status-badge.css` + modul RJ, 4 kartu termasuk nama/spesialis sangat panjang) | Lebar 320, 375, 414, 768, 1024, 1280, 1440, dan 1920 px. Tidak ada elemen kartu yang saling tumpang-tindih, tidak ada elemen yang keluar kartu, dan tidak ada scroll horizontal — `PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| MANUAL TEST | Belum diuji di layar aplikasi dengan font asli. User perlu memeriksa step Data Kunjungan. |

### Revisi lanjutan 2026-10-15 — badge kuota tetap di pojok kanan atas, nama turun bila bertabrakan

Masukan user: badge kuota harus tetap di pojok kanan atas. Bila nama dokter bertabrakan dengan badge, namanya yang turun ke bawah.

| Hal | Isi |
| --- | --- |
| Perubahan CSS | `.doctorCardTitleRow` memakai `flex-direction: row-reverse` + `flex-wrap: wrap`. Badge memakai `order: -1` dan `margin-left: auto`, sehingga selalu menempati kanan atas lebih dulu. Nama (`flex: 1 1 auto`, rata kiri) tetap di samping badge bila muat satu baris; bila tidak muat, nama turun ke baris di bawah badge selebar kolom. Urutan DOM tidak berubah. |
| Uji tata letak headless (8 lebar 320–1920 px, 4 kartu) | Badge selalu berjarak sama dari tepi kanan dan atas kartu (9 px pada ≤480 px, 13 px di atasnya). Nama berada di samping atau di bawah badge, tidak pernah tumpang-tindih. Tidak ada elemen keluar kartu dan tidak ada scroll horizontal — `PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| MANUAL TEST | Belum diuji di layar aplikasi dengan font asli. |

### Revisi lanjutan 2026-10-15 — badge float di pojok kanan atas, kata nama yang menabrak turun

Masukan user: tampilan dikembalikan seperti semula, dengan badge di pojok kanan atas dan nama di samping foto. Bila nama menabrak badge (contoh *dr. Maya Permata Sari*), kata yang menabrak dipotong ke baris bawah, seperti *dr. Bagus Purnama / Sanjaya*.

| Hal | Isi |
| --- | --- |
| Perubahan JSX | Badge kuota didahulukan di DOM, sebelum nama, di dalam `.doctorCardTitleRow`. |
| Perubahan CSS | Pendekatan `row-reverse` diganti float. `.doctorCardQuota` memakai `float: right` dengan margin kiri `--space-2`. `.doctorCardTitleRow` memakai `display: flow-root`. Nama mengalir di kiri badge, dan hanya kata yang tidak muat yang turun ke baris berikutnya. Baris sesudah badge kembali selebar kolom. |
| Uji tata letak headless (8 lebar 320–1920 px, 5 kartu, kotak per baris teks via `getClientRects`) | Badge selalu di pojok kanan atas. Tidak ada baris nama atau jam yang tumpang-tindih dengan badge, foto, atau tombol. Tidak ada elemen keluar kartu dan tidak ada scroll horizontal — `PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| MANUAL TEST | Belum diuji di layar aplikasi dengan font asli. |

### Revisi lanjutan 2026-10-15 — badge kuota dihapus

Atas keputusan user (`RJ-REG-DEC-010`), badge kuota dihapus dari kartu jadwal dokter. Yang dihapus: komponen `StatusBadge`, kelas `.doctorCardTitleRow`, dan kelas `.doctorCardQuota`. Nama dokter kembali menjadi `strong` biasa. Penanda jadwal penuh dikerjakan terpisah sebagai `RJ-REG-REV-BE-006` + `RJ-REG-REV-FE-009`. ESLint berkas tersentuh exit 0 — `PASS`.
