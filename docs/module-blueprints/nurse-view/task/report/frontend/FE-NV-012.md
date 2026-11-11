# Laporan Perubahan Frontend — `FE-NV-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-012` |
| Judul | Konsol Panggilan: pilih lantai, daftar, aksi, butir menu, hapus prototipe |
| Slice | `MVP-8` — `EPIC NV-06`, `NV-08` (gelombang frontend 2, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-012` |
| Trace | `FR-NV-039`, `041`, `049`, `050`, `052`; `NV-DEC-040`, `043`, `044`, `051`, `053`; `NV-FE-008`; state §7; `03-frontend-architecture.md` R2.2, R2.3 `FE-NVW-08`, R2.5, R2.8, R2.9 |
| Contract version | `0.2.1` `approved` (`NV-DEC-057`); api §7 dan §10 tidak berubah sejak `0.2.0` |
| Wewenang UI | Nama dan urutan butir menu; tata letak, warna, bentuk dialog Tutup (`DEV_DISCRETION`) |
| Dependency | `BE-NV-012` ✅, `FE-NV-010` 🟡 |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 11 (2), logika sedang — kunci aksi, ganti grup lantai (1), kontrak yang sudah ada (1), keamanan berkaitan — butir menu berizin (1), UI satu layar (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | Berkas baru di `src/{app,components/view,lib,utils,style}/health-services/nurse-view-management/`; `menu-items.jsx` (berkas bersama, restu `NV-DEC-056`); penghapusan `src/lib/helpers/nurseCallContext.jsx` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7d32e7cec` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `589fb513` + `BE-NV-012`–`014` belum di-commit (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 10 November 2026** — AC-1–9 terbukti di layar: uji dua perawat (Sari, Dewi) + DOKTER + tablet, 10/11 `PASS`: panggilan baru tampil 0,27 detik; urutan 326-C, 325-A, 325-B; inisial · JK · umur tanpa nama/No. RM; ambil alih dan lepas; klaim bersamaan → satu pengklaim, yang kalah melihat pesan; Tutup wajib alasan dan keterangan; DOKTER tanpa butir menu dan "Akses Ditolak"; Sari memilih Lantai 4 dan mengklaim. Belum: AC-10 — `src/lib/helpers/nurseCallContext.jsx` masih ada (rujukan 0), menunggu `git rm` oleh Rizki. Catatan tampilan: `StatusBadge` menampilkan "… Menuju" (huruf kapital awal kata). Revisi 11 Nov 2026: toast sukses aksi dihapus agar tidak menutupi tombol baris bawah (§3.4); uji ulang 10/11, hanya K10 |

---

## 1. Keadaan yang ditemukan di awal

Endpoint konsol sudah ada (`BE-NV-012`). Belum ada layar maupun butir menu. Prototipe lama
`src/lib/helpers/nurseCallContext.jsx` (161 baris, berbasis `localStorage`) masih ada dan tidak
di-`import` berkas mana pun.

## 2. Proses bisnis dari sisi pengguna

**Contoh normal.**

1. Sari membuka menu **Rawat Inap › Konsol Panggilan Pasien** (hanya tampil bagi pemegang
   `NurseViewPatientCall : Read`).
2. Pertama kali, konsol meminta "Pilih lantai yang Anda jaga". Sari memilih Lantai 3; pilihan diingat
   di komputer itu.
3. Judul menjadi besar: **Konsol Panggilan · Lantai 3**. Tabel memuat 331-C (DARURAT), 305-A, 318-B,
   berurutan dari backend, dengan inisial · jenis kelamin · umur, lama menunggu yang berjalan per
   detik, "×2" bila ditekan ulang, dan status.
4. Sari menekan **Saya tangani** pada 331-C. Tombol terkunci selama permintaan berjalan; sesudahnya
   toast "Panggilan Anda tangani." dan status "Sari menuju".
5. Di layar Dewi, item itu menampilkan **Ambil alih**, bukan Saya tangani.
6. **Tutup** membuka dialog: alasan wajib dari tiga pilihan; "Lainnya" wajib keterangan 5–250
   karakter.

**Jalur tidak normal:**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Dua perawat mengklaim bersamaan | Yang kalah melihat toast "Panggilan sudah berubah" dengan pesan backend (`VAL-NV-33`/`38`); daftar dimuat ulang |
| Tidak ada daftar baru > 120 detik | Pita "Data terakhir HH:MM — koneksi terputus. Daftar dimuat ulang otomatis."; daftar lama tetap tampil |
| Lantai tersimpan sudah tidak dikenal (`422`) | Pilihan lantai dihapus; konsol kembali meminta pilih lantai |
| Pengguna tanpa izin membuka route | Halaman "Ups! Akses Ditolak" |
| Ambang belum disahkan | Info "Ambang lama menunggu belum disahkan…"; lama menunggu tetap tampil tanpa warna |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Modul rujukan visual: `master-data-nurse-view-bed-tablet-view.jsx` (`FE-NV-008`). Base:
`data-table.jsx` (`pagination`, `sortLatestFirst`), `status-badge.jsx` (status `danger`),
`confirm-modal.jsx` (`children` menggantikan `message`), `base-form-control.jsx`
(`BaseSelectField`, `BaseTextAreaField`), `data-filter.jsx` (search hanya bila `onSearchChange`),
`access-denied-gate.jsx` dan `utils/access-denied-utils.jsx`. Pola realtime:
`use-admission-queue-realtime.jsx`. Backend: `NvwPatientCallController`, `NvwPatientCallDtos`,
`NvwHttp.Map` (kode galat di `errors.code`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/nurse-view-management/patient-call-console/page.jsx`, `patient-call-console-client.jsx` | **Baru.** Route tipis; view dimuat tanpa SSR karena pilihan lantai dibaca dari `localStorage` |
| `src/components/view/health-services/nurse-view-management/patient-call-console/nurse-view-patient-call-console-view.jsx` | **Baru.** `Hero`, `DataFilter` + `FilterSelect` lantai, `InformationAlert`, `DataTable`, `ToastStack`, dalam `AccessDeniedGate` |
| `…/nurse-view-patient-call-console-table-columns.jsx` | **Baru.** Kolom Bed, Pasien, Jenis, Menunggu, Status, Aksi; tombol mengikuti `AvailableActions` |
| `…/nurse-view-patient-call-close-modal.jsx` | **Baru.** `ConfirmModal` + `BaseSelectField` + `BaseTextAreaField` |
| `src/lib/hooks/health-services/nurse-view-management/patient-call-console/use-nurse-view-patient-call-console.jsx` | **Baru.** Lantai (ingat di PC, `try`/`catch`), daftar, cadangan 15 detik, `online`, realtime join/leave per lantai, data basi > 120 detik, kunci aksi, dialog Tutup, toast |
| `src/lib/services/health-services/nurse-view-management/patient-call-console-service.jsx` | **Baru.** `GET /floors`, `GET /active`, `POST /{id}/{aksi}` lewat `InstanceAxios` |
| `src/lib/constants/health-services/nurse-view-management/patient-call-console/patient-call-console-constants.jsx` | **Baru.** Endpoint, kunci penyimpanan, interval, alasan tutup, label aksi, teks layar |
| `src/utils/health-services/nurse-view-management/patient-call-console/patient-call-console-utils.jsx` | **Baru.** Normalisasi, format, validasi Tutup, baca/tulis lantai, selisih daftar (dipakai bunyi) |
| `src/style/health-services/nurse-view-management/patient-call-console.module.css` | **Baru.** Tata letak sel dan dialog; hanya token |
| `src/utils/menu-sidebar/menu-items.jsx` | +7: butir "Konsol Panggilan Pasien" di grup Rawat Inap sesudah "Daftar Pantau", `requiredPermission` `NurseViewPatientCall : Read`, ikon yang sudah diimpor |
| `src/lib/helpers/nurseCallContext.jsx` | **Belum terhapus** — perintah hapus ditolak pengaman sesi agent (§8) |

### 3.3 Kepatuhan arsitektur frontend

Alur `app` → `components/view` → `hooks` → `services` → `InstanceAxios`, tanpa Redux (data satu
layar). Definisi kolom di berkas `-table-columns.jsx` terpisah. `AccessDeniedGate` menerima
`{ statusCode, message }` sehingga `401`/`403` selalu dikenali.

**Tabel keputusan base component:**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kepala halaman dengan nama lantai besar | `Hero` | `base-features/hero.jsx` | `REUSE` | `title` "Konsol Panggilan · Lantai 3" |
| Pilih lantai + tombol bunyi | `DataFilter`, `FilterSelect`, `BaseButton` | Pola `FE-NV-008` | `REUSE` | Tanpa search dan reset |
| Daftar panggilan | `DataTable` | `pagination={false}`, `sortLatestFirst={false}` | `REUSE` | Urutan dari backend |
| Jenis, lama menunggu, status | `StatusBadge` | Status `danger`/`warning`/`info`/`pending`/`active` ada di source | `REUSE` | Warna lama menunggu hanya bila `WaitLevel` bukan `None` |
| Tombol aksi | `BaseButton` | `size="sm"`, `loading`, `disabled` | `REUSE` | — |
| Dialog Tutup | `ConfirmModal`, `BaseSelectField`, `BaseTextAreaField` | Pola dialog nonaktif `FE-NV-008` | `COMPOSE` | Isian alasan dan keterangan di `children` |
| Info pilih lantai, data basi, galat | `InformationAlert` | — | `REUSE` | — |
| Notifikasi aksi | `ToastStack` | — | `REUSE` | — |
| Akses ditolak | `AccessDeniedGate` | — | `REUSE` | — |

`UI GATE: 9 elemen — REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

**Keputusan: dialog Tutup**

- **A. `ConfirmModal` dengan `BaseSelectField` dan `BaseTextAreaField` di `children` — Rekomendasi
  (dijalankan).** Sama dengan dialog nonaktif tablet; validasi alasan wajib dan "Lainnya" wajib
  keterangan dikerjakan hook.
- **B. `ConfirmModal requireReason`.** Hanya satu kotak alasan bebas; tidak bisa memaksa tiga
  pilihan alasan sesuai `NV-DEC-053`.

### 3.4 Revisi 11 November 2026 — konsol tanpa toast sukses

**Temuan** (diagnosis K6, §6): toast sukses setiap aksi baris muncul 3,5 detik di kanan bawah
(`ToastStack` bawaan) dan menutupi tombol aksi baris bawah tabel. Perawat yang segera mengklik baris lain
tertahan sampai toast hilang; dalam uji, klaim perawat lain masuk lebih dulu. Rizki meminta kekurangan ini
dikerjakan (11 Nov 2026).

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/nurse-view-management/patient-call-console/use-nurse-view-patient-call-console.jsx` | `runAction` tidak lagi menambah toast sukses. Toast galat dan konflik `409` tetap, lalu daftar dimuat ulang (arsitektur R2: "`409` → pesan + muat ulang") |
| `src/lib/constants/health-services/nurse-view-management/patient-call-console/patient-call-console-constants.jsx` | Hapus `actionSuccessTitle` dan `actionSuccess` (tidak dirujuk lagi) |

Umpan balik sukses tetap terlihat di baris itu sendiri: "‹nama› menuju" sesudah Saya tangani/Ambil alih,
`Menunggu` sesudah Lepas, baris hilang sesudah Tutup. Pilihan lain yang ditimbang, memindah toast ke kanan
atas, ditolak karena saat daftar panjang digulir toast juga menutupi kolom Aksi baris teratas.
`UI GATE: PASS — tanpa elemen UI baru; satu elemen dihapus.`

**Verifikasi.** `npx eslint` 0 error, 0 warning; CRLF; rujukan `actionSuccess` 0. Uji konsol 11 Nov:
- skrip lama `uji-konsol-layar-v1.mjs`, yang mengklik bersamaan tanpa menunggu toast: **10/11**, K6 lulus;
- skrip v2: **10/11**, tanpa toast yang perlu ditunggu (0,006 detik);
- log: kedua putaran mencatat dua `claim` bersamaan berselisih 1–5 ms (v1 Sari `409`, v2 Dewi `409`);
- nol galat dan nol 5xx;
- satu-satunya gagal K10 (berkas prototipe).

Commit `7b9bea21a` (`RizkiV2`); build produksi belum memuat revisi ini.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| `PilihLantai` | "Pilih lantai yang Anda jaga untuk menampilkan panggilan pasien." |
| `Memuat` | "Mengambil panggilan aktif..." (kerangka tabel) |
| `Kosong` | "Tidak ada panggilan aktif di Lantai 3." |
| `Berisi` | Tabel berurutan |
| `DataBasi` | Pita "Data terakhir HH:MM — koneksi terputus. …" |
| Gagal | "Daftar panggilan gagal dimuat." + tombol Coba lagi |
| Tanpa hak akses | "Ups! Akses Ditolak" |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Patient Call

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/patient-calls/floors` | Pilihan lantai | `NurseViewPatientCall : Read` |
| `GET` | `…/patient-calls/active?floorCode=` | Daftar | `NurseViewPatientCall : Read` |
| `POST` | `…/patient-calls/{id}/claim`, `/take-over`, `/release` | Aksi klaim | `NurseViewPatientCall : Update` |
| `POST` | `…/patient-calls/{id}/close` | Tutup dengan alasan | `NurseViewPatientCall : Update` |

Realtime: `JoinNurseViewCallConsole(floorCode)` / `LeaveNurseViewCallConsole` dan event
`NurseViewPatientCallChanged` (`FE-NV-010`).

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` seluruh berkas konsol + `menu-items.jsx` | 0 error, 0 warning |
| Grep anti-regresi | Warna literal 0; typography di CSS konsol 0; `<button>`/`<table>` mentah 0; Bootstrap 0; `!important` 0; inline style 0 |
| Pencarian `nurseCallContext` / `NurseCallProvider` di `src/` selain berkas itu sendiri | **0 temuan** |
| Penghapusan berkas prototipe | **Ditolak** pengaman sesi agent — belum terhapus |
| Uji layar dua perawat + DOKTER + tablet (`uji-fe-nv-012-20261110/uji-konsol-layar.mjs`) | 10/11; satu-satunya gagal K10 = AC-10 | `PASS` untuk AC-1–9 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-10T02-25-30-975Z/` |
| Uji ulang sesudah merge backend (backend 14.41 dari DLL 14.37), 10 Nov 15.21 dan 15.24 WIB | 9/11 dua kali: K10 (AC-10, berkas masih ada) dan K6. K6 gagal karena skenario bersamaan tidak terjadi: log backend hanya mencatat **satu** `claim` (Dewi), klik Sari tidak terkirim (tombol tak bisa diklik ±30 detik, galatnya ditelan `.catch` skrip), dan tangkapan layar Sari menunjukkan baris sudah "Ambil alih". Sebab klik tidak terkirim dipastikan 11 Nov (baris berikut). K1–K5, K7–K9, K11 lulus | `PASS` untuk AC-1–3, 5–9; AC-4 dari putaran pagi | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-10T08-21-31-891Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-10T08-24-12-138Z/` |
| Uji ulang 11 Nov 2026 (backend 08.21 dari DLL 10 Nov 14.37, `next dev`) dan diagnosis K6 | 08.44 WIB 9/11 (K6, K10). Skrip v2 mencatat galat klik, bukan menelannya. Playwright melaporkan `toast-container bottom-0 end-0 … intercepts pointer events`: toast sukses "Saya tangani" Sari dari K4 (umur ±2,6 detik, hilang sesudah 3,5 detik) menutupi tombol baris 325-B di kanan bawah. Klik Sari tertahan, klaim Dewi masuk lebih dulu, lalu daftar Sari dimuat ulang dan tombol berganti "Ambil alih". Skrip v2 menunggu toast hilang (3,2 detik) lalu kedua klik terkirim dalam 0,05 detik. Hasil 08.49 WIB **10/11**: K6 `PASS`, log mencatat dua `claim` pada panggilan yang sama berselisih 1 ms (Dewi `200`, Sari `409`), Sari melihat pesan dan daftarnya menampilkan pengklaim. Satu-satunya gagal K10 (berkas masih ada) | `PASS` untuk AC-1–9 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-11T01-47-41-747Z/` (diagnosis), `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-11T01-49-44-735Z/`; skrip lama disimpan `uji-konsol-layar-v1.mjs` |
| Sesudah revisi §3.4 (tanpa toast sukses), 11 Nov 09.05 dan 09.06 WIB | v1 (tanpa menunggu toast) 10/11 dan v2 10/11; K6 lulus di keduanya; log: dua `claim` bersamaan per putaran (selisih 1–5 ms, satu `200` satu `409`), nol galat, nol 5xx; hanya K10 gagal | `PASS` untuk AC-1–9 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-11T02-05-43-344Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-11T02-06-22-658Z/` |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest.`
`MANUAL TEST: PASS untuk AC-1–9 — 10 Nov 2026. Diuji di `next dev` port 3000 dengan backend build 9 Nov 15.59 (`dotnet run --no-build`), peramban Edge headless.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 urutan 331-C (Darurat), 305-A, 318-B | ✅ Terbukti 10 Nov 2026 | K2: 326-C, 325-A, 325-B |
| AC-2 di layar Dewi: Ambil alih; sesudahnya "Dewi menuju" | ✅ Terbukti 10 Nov 2026 | K4 (label tampil "… Menuju" karena `StatusBadge`) |
| AC-3 Lepas → `Menunggu`, lama menunggu dari tekan pertama | ✅ Terbukti 10 Nov 2026 | K5: 7 detik tampil vs 8 detik sejak tekan |
| AC-4 klaim bersamaan → pesan `VAL-NV-33`/`38` + muat ulang | ✅ Terbukti 10 Nov 2026; diulang 11 Nov 2026 | K6 (11 Nov: Dewi `200`, Sari `409` berselisih 1 ms) |
| AC-5 Tutup tanpa alasan / Lainnya tanpa keterangan ditolak | ✅ Terbukti 10 Nov 2026 | K7: alasan 4 + keterangan tersimpan |
| AC-6 tanpa izin: butir menu tidak tampil, route ditolak; Sari tanpa roster bisa klaim lantai 4 | ✅ Terbukti 10 Nov 2026 | K8, K9 |
| AC-7 hanya inisial · JK · umur | ✅ Terbukti 10 Nov 2026 | K3 |
| AC-8 ambang kosong → tanpa warna | ✅ Terbukti 10 Nov 2026 | K1: info ambang belum disahkan; tanpa warna |
| AC-9 panggilan baru tampil dalam hitungan detik | ✅ Terbukti 10 Nov 2026 | K2: 0,27 detik |
| AC-10 berkas prototipe tidak ada; pencarian 0 temuan | 🟡 **Sebagian** — rujukan 0 (K10), berkas masih ada | Menunggu `git rm` oleh Rizki |
| DoD — lint 0 errors | Terpenuhi | §6 |
| DoD — rekaman konsol, TV, tablet bersamaan | Terpenuhi 10 Nov 2026 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-012-20261110/layar-2026-11-10T02-25-30-975Z/` (konsol dua perawat + tablet); TV di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T02-06-54-206Z/` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Butuh tindakan Rizki | Hapus prototipe: `git rm src/lib/helpers/nurseCallContext.jsx` (berkas ter-track Git, tidak dirujuk berkas lain). Agent tidak mencoba cara lain sesudah penolakan |
| Risiko | Konsol dipakai untuk lantai yang salah → nama lantai tampil di judul `Hero`. Pemberian izin lewat Akses Role menimpa seluruh policy: salin, tambah, pulihkan |
| Temuan UX (11 Nov 2026) | (1) Toast sukses menutupi tombol aksi baris bawah → **diperbaiki** (§3.4, commit `7b9bea21a`). (2) **Dicabut — keliru.** Catatan sebelumnya menyebut tabel berkedip "Mengambil data..." tiap muat ulang karena sinyal. Hook ternyata mengembalikan `listLoading: listLoading && !list`, dan `list` hanya dikosongkan saat ganti lantai atau `422`, sehingga baris tetap tampil selama muat ulang. Kesimpulan awal diambil dari kode `DataTable` tanpa membaca nilai yang dikirim hook |
| Status Git | Sudah di-commit Rizki; revisi §3.4 di-commit dan di-push agent atas permintaan Rizki, `7b9bea21a` (`RizkiV2`); `nurseCallContext.jsx` masih ter-track |
| Langkah berikutnya | Rizki `git rm src/lib/helpers/nurseCallContext.jsx`; sesudah itu task ini ✅ |
