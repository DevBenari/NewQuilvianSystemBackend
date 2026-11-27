# Laporan Perubahan Frontend — `FE-LAB-59`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-59` |
| Judul | Kiosk Konfirmasi Kehadiran: tanggal rencana dan penanda |
| Slice | `S20-1` penjadwalan ulang pemeriksaan — gelombang `MVP-16d` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) *Gelombang `MVP-16`* — `FE-LAB-59` |
| Trace | `FR-21.10`, `FR-21.12`; `LAB-DEC-213`, `266`; `03-frontend-architecture.md` amandemen 2026-11-25 |
| Contract version | `LAB-API-v1` **`r50`** 45.6 — **`approved` 2026-11-25** (Yoga Aji Pratama) |
| Wewenang UI | `DEV_DISCRETION`: warna penanda, letak catatan di bawah nama pemeriksaan, nama berkas. **Bukan** `DEV_DISCRETION` (dipatuhi): kiosk **tidak menolak** dan **tanpa alasan** (`LAB-DEC-266`); nol perubahan alur konfirmasi |
| Dependency | Kontrak `r50` (approved). Backend `BE-LAB-103` ✅ selesai pada kode, belum di API dev bersama |
| Klasifikasi | `LOW` — repository 0, berkas diperiksa 1 (±8), berkas diubah 1 (4 + 1 baru), logika 0 (format murni), kontrak API 1 (tiga ruas baca), database 0, keamanan 0, UI 1 (satu langkah kiosk) = **4** |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — langkah *Konfirmasi Kehadiran* kiosk pasien lama, normalisasi service, konstanta teks, gaya, uji; laporan, roadmap, dan traceability modul. Tanpa commit |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + working tree `FE-LAB-57`, `FE-LAB-58` |
| Commit backend yang dijadikan rujukan | `cce1dbf5` + working tree `BE-LAB-101`..`103` |
| Tanggal | 2026-11-26 (kode, build) — 2026-11-27 (e2e, laporan) |
| Status | ✅ **Selesai pada kode** — lint bersih, uji unit baru 5/5, **e2e 1/1** terhadap API tiruan `r50` (tiga keadaan + nol tulis tambahan). Verifikasi kiosk terhadap backend nyata **belum** — menunggu langkah rilis `MVP-16e` |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti |
| --- | --- |
| Langkah *Konfirmasi Kehadiran* menampilkan butir pesanan Lab: nomor urut, disiplin, nama pemeriksaan — tanpa tanggal rencana | `kiosk-old-patient-step-lab-attendance.jsx` (`InfoItem` per butir) |
| Normalisasi `pending-by-patient` membuang ruas selain enam ruas lama | `kiosk-old-patient-registration.service.js` `normalizeLabPendingOrderGroup` |
| Konfirmasi Kehadiran tidak mengirim permintaan tulis apa pun (mengikuti kiosk v1, `LAB-EVD-013`); sesi kiosk dibentuk sekali di langkah Tujuan Layanan | `use-kiosk-old-patient-registration.jsx` `startLaboratoryJourney` |
| Backend sejak `BE-LAB-103` menambah `plannedDate`, `isEarly`, `isOverdue` per butir — tanpa alasan atau riwayat | Kontrak `r50` 45.6; [`BE-LAB-103.md`](../backend/BE-LAB-103.md) `T-S20-16` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** pasien lama di kiosk yang memilih **Laboratorium** dan sudah memiliki pesanan Lab aktif.

1. Pasien mencari datanya, memeriksa *Review Data*, lalu menekan **Data Benar, Lanjut** dan memilih **Laboratorium**.
2. Kiosk menemukan pesanannya dan membuka *Konfirmasi Kehadiran Laboratorium*. Di bawah setiap nama pemeriksaan kini
   tertulis tanggal rencananya:
   - *Rencana 26 November 2026* — sesuai jadwal;
   - *Rencana 3 Desember 2026 (lebih awal)* — pasien datang sebelum tanggal rencana;
   - *Rencana 24 November 2026 (terlewat)* — tanggal rencana sudah lewat.
3. Pasien menekan **Konfirmasi Kehadiran** seperti biasa. Kiosk **tidak menolak** dan tidak mengalihkan pasien yang
   datang lebih awal maupun terlambat; tidak ada alasan jadwal ulang yang ditampilkan. Petugas loket yang memutuskan.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Backend lama tanpa `plannedDate` (API dev bersama sebelum `MVP-16e`) | Butir tampil seperti sebelumnya, tanpa catatan rencana — tidak menebak |
| `isEarly`/`isOverdue` bukan boolean `true` | Diabaikan; hanya *Rencana {tanggal}* |
| Zona waktu perangkat kiosk berbeda | Tanggal kalender diformat dalam UTC dari tengah malam UTC — tidak bergeser sehari |
| Penanda dari backend | Dibaca apa adanya; kiosk **tidak** menghitung *lebih awal*/*terlewat* dari jam perangkat |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-59`; kontrak `r50` 45.6; `kiosk-old-patient-view.jsx`, `kiosk-old-patient-step-lab-attendance.jsx`,
`use-kiosk-old-patient-registration.jsx` (`startLaboratoryJourney`, `handleReviewContinue`),
`kiosk-old-patient-registration.service.js`, `kiosk-old-patient-service-target.constants.js`,
`kiosk-old-patient-view.module.css`, `kiosk-home-view.jsx`; [`BE-LAB-103.md`](../backend/BE-LAB-103.md).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | `normalizeLabPendingOrderGroup` meneruskan `plannedDate` (10 karakter pertama), `isEarly`, `isOverdue` (hanya `true` sungguhan; camelCase/PascalCase) |
| `src/utils/kiosk/registration/kiosk-lab-plan-utils.jsx` | **Baru** — fungsi murni `formatKioskPlannedDate` (`2026-12-03` → *3 Desember 2026*, zona UTC) dan `describeKioskLabPlan` (teks + nada `planned`/`early`/`overdue`; tanpa tanggal → kosong) |
| `src/lib/constants/kiosk/registration/kiosk-old-patient-service-target.constants.js` | Teks `plannedPrefix` *Rencana*, `earlySuffix` *(lebih awal)*, `overdueSuffix` *(terlewat)* |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-lab-attendance.jsx` | `InfoItem` menerima `note`; butir pesanan menampilkan catatan rencana di bawah nama pemeriksaan. Tombol dan alur konfirmasi **tidak berubah** |
| `src/style/kiosk/registration/kiosk-old-patient-view.module.css` | `.ticketModernInfoNote` (teal), varian *lebih awal* (biru) dan *terlewat* (jingga) |
| `tests/unit/kiosk-lab-plan-utils.test.mjs` | **Baru** — 5 uji |
| `tests/e2e/lab-kiosk-planned-date.spec.mjs` | **Baru** — 1 skenario API tiruan (tiga keadaan + nol tulis) |

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → service` tidak berubah: service menormalkan ruas baru, view memanggil util murni untuk teks.
Teks pasien di berkas konstanta kiosk (bahasa sehari-hari, tanpa istilah teknis). Nol state, thunk, atau komponen dasar
baru; nol perubahan pada hook dan alur konfirmasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah — *Memeriksa pesanan laboratorium Anda...* di langkah Tujuan Layanan |
| Kosong | Tanpa `plannedDate`: butir tanpa catatan rencana. Tanpa pesanan: jalur pendaftaran Lab biasa (tidak berubah) |
| Gagal | Tidak berubah — gagal membaca pesanan dianggap tanpa pesanan supaya pasien tetap dapat mendaftar |
| Tanpa hak akses | Tidak berubah — rute kiosk dijaga `isKioskUser`; endpoint `KioskReadPolicy` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/kiosk/pending-by-patient/{patientId}` | Pesanan aktif pasien; kini membaca `plannedDate`, `isEarly`, `isOverdue` per butir | `KioskReadPolicy` |

Tidak ada endpoint tulis baru. Konfirmasi Kehadiran tetap tanpa permintaan tulis.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 6 berkas task (service, util, konstanta, view, uji unit, spec) | 0 error, 0 peringatan | `PASS` | Keluaran eslint |
| Uji unit `kiosk-lab-plan-utils` | **5/5** — format panjang tanpa geser zona; tiga keadaan; PascalCase dan nilai non-boolean; tanpa tanggal → kosong; teks tidak pernah memuat alasan atau kata penolakan | `PASS` | Keluaran uji |
| Uji unit seluruh frontend | 5.453 lulus, **152 gagal** dari 5.605 (baseline 2026-11-09: 46 dari 4.612; naik sesudah merge integrasi). Tidak ada yang menyentuh kiosk atau berkas `FE-LAB-57`..`59`: 14 berkas uji yang membaca berkas kiosk/Lab yang diubah → **232/233**, satu-satunya gagal `FE-LAB-49 AC-282` (sudah gagal sebelum `FE-LAB-57`). Gagal berbau Lab lainnya, `FE-RWI-089 AC-6`, membaca lima berkas Rawat Inap yang tidak berubah di working tree | `PASS` / `EXISTING / ENVIRONMENT ISSUE` | `unit-all.log` |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 107s` | `PASS` | Keluaran build 2026-11-26 14:14; seluruh berkas task diubah sebelum build itu (mtime 14:10–14:11) |
| E2E `lab-kiosk-planned-date.spec.mjs` | **1/1**: cari pasien → *Data Benar, Lanjut* → Laboratorium; `pending-by-patient` dipanggil **sekali**; *Rencana 26 November 2026*, *Rencana 3 Desember 2026 (lebih awal)*, *Rencana 24 November 2026 (terlewat)* tampil persis; **Konfirmasi Kehadiran** aktif; sesudah ditekan *Kehadiran Dikonfirmasi* tampil dan permintaan non-GET tambahan = **0** | `PASS` | `pw-fe59.log`, `pw-fe59-trace.log` |
| Pemeriksaan visual (bingkai trace Chromium) | Tiga catatan rapi di bawah nama pemeriksaan dengan tiga warna; tombol Kembali/Konfirmasi tidak berubah; layar sukses seperti sebelumnya | `PASS` | `trace59b/sebelum.jpeg`, `sesudah.jpeg` |
| `test-results/`, `playwright-report/` ter-track | Tidak berubah (`--output` ke scratchpad) | `PASS` | `git status` |

Uji manual: **`NOT FEASIBLE`** terhadap backend nyata — build menunjuk API dev bersama yang belum memuat kode
`BE-LAB-103`, sehingga `pending-by-patient` belum mengirim `plannedDate`. Satu-satunya kontrol yang tersentuh —
**Konfirmasi Kehadiran** — diperiksa di Chromium sungguhan terhadap API tiruan `r50`: tetap aktif dan tanpa tulis.

**Susulan 2026-11-27 (langkah rilis `MVP-16e`):** kiosk diuji terhadap **backend nyata** (backend lokal → devYoga) dengan
pesanan uji `LAB-RSMMC-000031` (rencana 2026-12-02). Backend mengirim persis `plannedDate`, `isEarly: true`,
`isOverdue: false` tanpa alasan; layar menampilkan *Rencana 2 Desember 2026 (lebih awal)*; Konfirmasi Kehadiran aktif; nol
tulis sesudah konfirmasi. Sesi superadmin dengan `/auth/me` ditandai akun kiosk (pola `LAB-EVD-013`) dan
`kiosk-scan-sessions` ditiru — belum dengan perangkat kiosk asli. Rincian: `backend-roadmap.md` 6av.4.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-351` / `T-S20-16` sisi layar — tanggal rencana setiap pesanan, *(lebih awal)*, *(terlewat)* | Terpenuhi (API tiruan) | E2E tiga keadaan |
| Konfirmasi tanpa tulis tambahan | Terpenuhi | E2E: 0 permintaan non-GET sesudah konfirmasi |
| `LAB-DEC-266` — kiosk tidak menolak, tanpa alasan | Terpenuhi | Tombol aktif pada ketiga keadaan; uji unit teks tanpa alasan/penolakan |
| Jebakan: menolak atau mengalihkan pasien *lebih awal* | Terhindar | Nol perubahan hook dan alur |
| Kiosk akun asli terhadap backend nyata | **Belum** | Menunggu `MVP-16e` |
| DoD — laporan `FE-LAB-59.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Terhadap API dev bersama yang belum memuat `r50`, butir tampil seperti sebelumnya (tanpa catatan rencana) — tidak menebak |
| Masalah yang diketahui | `NONE` dalam scope |
| Dependency backend | `BE-LAB-103` selesai pada kode; langkah rilis `MVP-16e` sebelum verifikasi kiosk asli |
| Perubahan sampingan | `NONE`. Perubahan SDM di working tree (`src/lib/services/hr/*`, `src/utils/hr/hr-api-utils.js`, `tests/unit/hr-a52b-api-client.test.mjs`) milik sesi lain — tidak disentuh |
| Interupsi | Sesi 2026-11-26 terhenti batas pemakaian sesudah build. Dilanjutkan 2026-11-27 dari `git status`/diff: kode dan build utuh. Run e2e pertama gagal karena spec menebak label tombol review (`/^Lanjut/`; label sebenarnya *Data Benar, Lanjut*) — diperbaiki **di spec**, kode layar tidak berubah |
| Status Git | Berkas task: ` M` `kiosk-old-patient-step-lab-attendance.jsx`, `kiosk-old-patient-service-target.constants.js`, `kiosk-old-patient-registration.service.js`, `kiosk-old-patient-view.module.css`; `??` `kiosk-lab-plan-utils.jsx`, `tests/unit/kiosk-lab-plan-utils.test.mjs`, `tests/e2e/lab-kiosk-planned-date.spec.mjs`. Berkas `FE-LAB-57`/`58` tetap di working tree. Tanpa stage/commit |
| Langkah berikutnya | Gelombang `MVP-16d` selesai pada kode. Langkah rilis `MVP-16e`: verifikasi peramban dengan akun Gilang, dr. Bima, dan akun kiosk |
