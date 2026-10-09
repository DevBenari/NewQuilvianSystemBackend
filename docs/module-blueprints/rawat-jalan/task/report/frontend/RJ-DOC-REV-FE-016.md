# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-016` |
| Judul | Konfigurasi dan penanda |
| Slice | Amendment AQ — antrean prioritas, member, dan privasi layar publik |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `17` |
| Trace | `RJ-DOC-DEC-055`, `057`, `058`, `060` |
| Kontrak | Delta aditif `RJ-DOC-REV-BE-015`/`016` (backend sudah ✅) |
| Dependency | `RJ-DOC-REV-BE-015` ✅, `RJ-DOC-REV-BE-016` ✅ |
| Task mode | `CROSS-REPO MODE`, frontend sesudah backend (`RJ-DOC-DEC-060`) |
| Target tulis | `V2QuilvianSystemFrontendDev` (`sukmagpV2`); laporan dan tanda status di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d116c51fe` (`sukmagpV2`), belum di-commit |
| Tanggal | 2026-10-08 |
| Status | Selesai — ESLint `0 error`, `npm run build` `PASS`, uji layar `11/11 PASS` |

## 1. Keadaan yang ditemukan di awal

- Form Membership Tier sudah punya *Antrean Prioritas* dan *Level Prioritas*, tetapi belum ada *Queue Audience* dan *Public Display Mode*.
- Form Display Antrian belum punya pilihan kelompok antrean.
- Kartu antrean perawat menampilkan "Prioritas" di baris tombol; kartu dokter tidak menampilkan apa pun. Flag `isPriorityQueue` sebelumnya selalu `false` dari backend.
- Kiosk dan Pendaftaran Rawat Jalan hanya menampilkan `queueCode` dari backend; tidak ada logika nomor di React. Tidak perlu diubah.

## 2. Proses bisnis dari sisi pengguna

1. Admin membuka *Tingkat Keanggotaan* → Tambah/Perbarui. Bagian *Benefit Membership* kini memuat *Antrean Prioritas*, *Queue Audience* (Regular/Member, bawaan Member), dan *Public Display Mode* (Default, Full Name, Masked Name, Queue Number Only; bawaan Default). Detail tier menampilkan keduanya.
2. Admin membuka *Display Antrian* → Tambah/Perbarui dan memilih *Audience Display*: All (bawaan), Regular Only, atau Member Only. Penyaringan dilakukan backend.
3. Perawat dan dokter melihat label **Prioritas** dan/atau **Member** di baris tersendiri di bawah identitas pasien pada kartu antrean, hanya bila backend menandainya. Pasien reguler tidak mendapat baris tambahan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Form/editor/detail Membership Tier, form/editor/detail/slice Queue Display Device, `QueuePatientCard` perawat dan dokter beserta display utils dan CSS module, `base-form-control` (`BaseSelectField`), `filter-select`, `base-editor-field`, layar tiket kiosk dan pendaftaran Rawat Jalan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/administrator/master-data/membership-tiers/membership-tier-editor-config.jsx` | Field select `queueAudience`, `publicDisplayMode`, opsi fallback, nilai bawaan |
| `.../membership-tiers/membership-tier-grouped-form.config.jsx` | Kedua field masuk bagian *Benefit Membership* |
| `.../membership-tiers/membership-tier-detail-config.jsx` | Nilai enum mentah disembunyikan dari detail |
| `src/utils/administrator/master-data/membership-tiers/membership-tier-editor-utils.js` | Dikirim sebagai angka |
| `.../membership-tiers/membership-tier-display-utils.js` | Opsi dari `filters/metadata` dinormalisasi |
| `.../membership-tiers/membership-tier-detail-utils.js` | Label dan urutan detail |
| `src/lib/constants/administrator/master-data/queue-display-device/queue-display-device-constants.js` | Opsi fallback audience |
| `src/utils/administrator/master-data/queue-display-device/queue-display-device-editor-utils.jsx` | Field *Audience Display*, alias, payload |
| `src/lib/hooks/administrator/master-data/queue-display-device/use-administrator-queue-display-device-editor.jsx` | Opsi dari metadata atau fallback |
| `src/lib/state/slice/administrator/master-data/master-data-queue-display-device-slice.jsx` | `queueAudienceMode` diizinkan lewat sanitizer payload (tanpanya field terbuang — ditemukan lewat uji layar) |
| `.../queue-display-device/queue-display-device-detail-utils.jsx` | Baris *Audience Display* di detail |
| `src/utils/health-services/registration-management/{nurse-station-management,doctor-queue}/*-display-utils.js` | Getter `getIsMemberQueue` |
| `src/components/features/health-services/{nurse-station-management,doctor-queue-features}/QueuePatientCard.jsx` | Baris label Prioritas/Member |

### 3.3 Kepatuhan arsitektur frontend

Tidak ada route, slice, service, komponen, atau CSS baru. Opsi select diambil dari metadata backend dengan fallback lokal bernilai sama dengan enum backend. Tidak ada aturan bisnis member/prioritas/privasi di React; layar hanya membaca flag backend.

**UI GATE**

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Select Queue Audience / Public Display Mode | `BaseSelectField` via config editor | `base-form-control.jsx`, dipakai `tierType` | REUSE | Config field |
| Select Audience Display | `BaseSelectField` via config editor | dipakai `layoutType` | REUSE | Config field |
| Baris detail | Detail config existing | `membership-tier-detail-utils`, `queue-display-device-detail-utils` | REUSE | Label dan urutan |
| Label Prioritas/Member di kartu | `.callInfo` dan `.callInfo em` existing | Kedua CSS module | COMPOSE | Baris sendiri di bawah identitas (opsi A); di baris tombol label terpotong tombol *Konsultasi* |

`UI GATE: REUSE 3 / COMPOSE 1 / NEW 0`. Tidak ada nilai warna/ukuran baru.

## 4. State yang ditangani di layar

Metadata belum termuat: opsi fallback. Update: nilai tersimpan termuat (U5); request update tanpa field lama tidak menimpa nilai (backend: kosong = nilai lama). Kartu antrean reguler tidak berubah tinggi.

## 5. Endpoint yang dikonsumsi

#### Administrator / Master Data / Membership Tier

`GET filters/metadata` (`queueAudienceOptions`, `publicDisplayModeOptions`), `POST`, `PUT /{id}`, `GET /{id}`.

#### Administrator / Master Data / Queue Display Device

`GET filters/metadata` (`queueAudienceModeOptions`), `POST`, `PUT /{id}`, `GET`.

#### Health Services / Registration Management / Nurse Station Queue, Doctor Queue

`GET` — field `isPriorityQueue`, `isMemberQueue`.

## 6. Verifikasi

| Perintah / skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` pada 15 berkas | `0 error`, 1 warning lama (`react-hooks/set-state-in-effect` baris 900 hook editor display, tidak disentuh) | `PASS` / `EXISTING WARNING` |
| `npm run test:unit` | 2443 pass, 8 fail — 8 kegagalan yang sama dengan baseline `HEAD` (Lab, Hemodialisa, tindakan, Bank Darah, resep, Keuangan) | `UNRELATED EXISTING ISSUE` |
| `npm run build` | `Compiled successfully`, 472 halaman | `PASS` |
| `AUTOMATED TEST` baru | `SKIPPED (opsional)` — tidak ada pola test komponen untuk layar ini | — |

**Uji layar Playwright** — FE dev `localhost:3000`, request `7184` dialihkan ke backend uji `https://localhost:7185`
(build `BE-015/016`), DB `QuilvianNewDevSukma`, login SuperAdmin seed. Script `ui_fe016.mjs`, `ui_badges.mjs` di scratchpad.

| ID | Skenario | Hasil |
| --- | --- | --- |
| U1 | Form tier menampilkan kedua field | `PASS` |
| U2 | Opsi dari metadata backend | Regular, Member / Default, Full Name, Masked Name, Queue Number Only — `PASS` |
| U3 | Simpan tier baru | `POST 200`, `queueAudience=2`, `publicDisplayMode=4` — `PASS` |
| U4 | Detail tier | "Member", "Queue Number Only" tampil — `PASS` |
| U5 | Form perbarui memuat nilai tersimpan | `PASS` |
| U6 | Simpan perbarui mempertahankan nilai | `PUT 200`, `2/4` — `PASS` |
| U7 | Form display menampilkan *Audience Display* | `PASS` |
| U8 | Opsi audience | All, Regular Only, Member Only — `PASS` |
| U9 | Simpan display Member Only | `POST 200`, `queueAudienceMode=3`, tersimpan "Member Only" — `PASS` (gagal sebelum perbaikan sanitizer slice) |
| U10 | Kartu perawat pasien prioritas+member | Label Prioritas dan Member tampil penuh — `PASS` |
| U11 | Kartu dokter pasien yang sama sesudah skrining | Label tampil, tidak tertutup tombol — `PASS` |

Data uji (tier `UJI-AQ-UI-*`/`UJI-AQ-BADGE-*`, membership, display, kunjungan) dibersihkan lewat endpoint
(`DELETE`/`cancel`, `200`). Pendaftaran ulang uji badge mendapat `B003` karena `B001` sudah terpakai oleh kunjungan
yang dibatalkan — bukti tambahan nomor tidak dipakai ulang.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Field tersimpan dan terbaca ulang | Terpenuhi | U3–U6, U9 |
| Badge mengikuti flag backend | Terpenuhi | U10, U11; getter membaca `isPriorityQueue`/`isMemberQueue` |
| Tidak ada aturan bisnis baru di frontend | Terpenuhi | Tidak ada logika nomor/klasifikasi; kiosk dan pendaftaran tidak diubah |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Server dev backend pemilik di `7184` masih biner lama yang membaca tabel `TrxQueue`; sesudah migration tabelnya `RegQueue`, sehingga fitur antrean di `7184` gagal sampai server dijalankan ulang dari source terbaru |
| Masalah yang diketahui | Teks "0x panggil" terbungkus dua baris pada viewport sempit — perilaku lama |
| Perubahan sampingan | `NONE` |
| Status Git frontend | 15 berkas ` M`, belum di-commit |
| Langkah berikutnya | Commit BE/FE atas izin pemilik; isi data induk membership MMC yang terverifikasi |
