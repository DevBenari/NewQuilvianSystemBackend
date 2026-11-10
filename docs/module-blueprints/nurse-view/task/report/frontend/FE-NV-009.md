# Laporan Perubahan Frontend — `FE-NV-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-009` |
| Judul | Login tablet dialihkan ke layar tablet bed |
| Slice | `MVP-6` — `EPIC NV-05` (gelombang frontend 1, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-009` |
| Trace | `FR-NV-036`; `NV-DEC-030`, `032`, `046`, `056`; `NV-CAP-21`; `G-09`; `NV-AC-041`; `UAT-12`, `14`, `26`; state §6; `03-frontend-architecture.md` R2.7 |
| Contract version | `0.2.1` `approved` (`NV-DEC-057`); api §6 `GET /current` tidak berubah sejak `0.2.0` |
| Wewenang UI | Tata letak kepala layar `DEV_DISCRETION` (`NV-FE-010`) |
| Dependency | `BE-NV-010` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 9 (2), logika sedang (1), kontrak yang sudah ada (1), keamanan berkaitan — pengalihan login (1), UI satu layar (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | Lima berkas bersama (restu `NV-DEC-056`) dan berkas baru `src/app/nurse-view-bed-tablet/`, `src/lib/constants/nurse-view-bed-tablet/` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7d32e7cec` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `589fb513` (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | ✅ **SELESAI 10 November 2026** — AC 5/5 terbukti di layar. Regresi login lima akun (SuperAdmin, perawat, LOKET, Kiosk, display antrean) sama dengan baseline `FE-NV-003` 2 Nov: tujuan, profil, sidebar, dan probe identik, kecuali `nurseViewCurrent` 404 → 403 dan dua klaim baru bernilai `false` (diharapkan). Uji layar tablet P1, P2, P8, P9, P10 `PASS` (putaran bersih 11/11). Perbaikan selama uji: P10 semula 63 detik (> 60) karena muat ulang tablet tanpa panggilan tiap 60 detik; batas atas `NURSE_VIEW_BED_TABLET_MAX_REFRESH_SECONDS` diturunkan 60 → 45 (`NV-FE-009` `DEV_DISCRETION`), sesudahnya 45 detik. `npx eslint` 0 error. Perubahan satu baris itu belum di-commit dan belum masuk build produksi |

---

## 1. Keadaan yang ditemukan di awal

Login TV Nurse View sudah dialihkan ke `/nurse-view-display` (`FE-NV-003`). Akun tablet bed
(profil `NurseViewBedTablet`, `BE-NV-010`) belum punya tujuan, sehingga jatuh ke dashboard.

## 2. Proses bisnis dari sisi pengguna

1. Petugas IT login di tablet bed memakai akun tablet.
2. Tablet langsung masuk `/nurse-view-bed-tablet` tanpa sidebar. Kepala layar berisi "Bed 326-C" dan
   nama kamar, **tanpa** nama atau inisial pasien.
3. Bila tablet membuka alamat lain (mis. `/dashboard`), ia dikembalikan ke `/nurse-view-bed-tablet`.
4. Bila tablet dinonaktifkan admin, muat ulang berikutnya (paling lambat 60 detik) mendapat `403` →
   cookie dihapus, logout, kembali ke layar login.

Akun lain tetap ke tujuan masing-masing: perawat → dashboard, Kiosk → `/kiosk`, display antrean →
`/queue-display`, TV Nurse View → `/nurse-view-display`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`proxy.js`, `use-base-login.jsx`, `root-layout-shell.jsx`, `auth-wrapper.jsx`,
`session-activity-manager.jsx`, `src/app/nurse-view-display/*`, `use-nurse-view-floor-board.jsx`,
laporan `FE-NV-003` (pola cabang TV), `AuthController` (klaim `profile_type`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Tidak disentuh |
| --- | --- | --- |
| `src/proxy.js` | +26: konstanta `NURSE_VIEW_BED_TABLET_PATH`; entri daftar publik; `lockNurseViewBedTabletAccount` (aktif hanya bila cookie `isNurseViewBedTabletAccount` atau `profileType=NurseViewBedTablet`); dua baris pemanggilan sesudah kunci TV | Cabang Kiosk, display antrean, TV |
| `src/lib/hooks/auth/use-base-login.jsx` | +33/−1: import konstanta tablet; `isNurseViewBedTabletUserInfo`, `syncNurseViewBedTabletSessionCookie`; di `redirectAfterLogin` cookie disetel atau dihapus, dan `router.replace` memilih path tablet. Baris −1 adalah `router.replace` lama yang kini juga memilih path tablet; jalur TV tetap sama | Penentuan `target` Kiosk dan display antrean |
| `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx` | Masing-masing tepat satu baris `"/nurse-view-bed-tablet"` | Selebihnya |
| `src/lib/constants/nurse-view-bed-tablet/nurse-view-bed-tablet-constants.jsx` | **Baru.** Path, nama cookie, profil, interval, endpoint, teks layar |
| `src/app/nurse-view-bed-tablet/page.jsx`, `layout.jsx`, `nurse-view-bed-tablet-client.jsx` | **Baru.** Route tipis; view dimuat tanpa SSR (pola TV) |

Cookie tablet dihapus pada setiap login akun bukan tablet, sehingga browser bekas tablet tidak salah
mengalihkan pegawai (pola N9 `FE-NV-003`). Layar tablet sendiri dikerjakan bersama `FE-NV-011`
(laporan terpisah).

### 3.3 Kepatuhan arsitektur frontend

Alur `app` → `components/view` → `hooks` → `services` → `InstanceAxios`, tanpa Axios instance
baru dan tanpa Redux baru. `globals.css` tidak diubah. Tabel keputusan base component untuk layar
tablet ada di laporan `FE-NV-011`.

`UI GATE: lihat FE-NV-011 — layar yang sama.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat layar tablet..." |
| Bed kosong | "Bed belum berpenghuni" |
| Ditolak (`401`/`403`) | "Mengalihkan ke layar login..." lalu layar login |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Bed Tablet Runtime

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/bed-tablet-runtime/current` | Kepala layar, keadaan bed, panggilan aktif | Policy `NurseViewBedTabletAccess` |

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` lima berkas bersama + berkas baru | 0 error, 1 warning lama (`root-layout-shell.jsx:118`, `react-hooks/set-state-in-effect`, di baris yang tidak disentuh) |
| Review diff berkas bersama | Sesuai §3.2 |
| Uji login lima akun (`rekam-login.mjs sesudah`) dan uji layar tablet `uji-tablet-layar.mjs` | Login 5/5 sama dengan baseline; tablet 11/11 | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-003-20261102/sesudah-2026-11-10T02-29-40-958Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-009-011-20261110/layar-2026-11-10T02-27-14-957Z/` |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest.`
`MANUAL TEST: PASS — 10 Nov 2026. Diuji di `next dev` port 3000 dengan backend build 9 Nov 15.59 (`dotnet run --no-build`), peramban Edge headless.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 regresi redirect lima akun | ✅ Terbukti 10 Nov 2026 | Perawat, SuperAdmin, LOKET → `/`; Kiosk → `/kiosk`; display → `/queue-display` (sama dengan baseline); TV → `/nurse-view-display` (uji strip T1); tablet → `/nurse-view-bed-tablet` (P1) |
| AC-2 tablet mengetik `/dashboard` → kembali; tanpa sidebar | ✅ Terbukti 10 Nov 2026 | P1 tanpa sidebar; P2 `/dashboard`, `/nurse-view-display`, `/` → `/nurse-view-bed-tablet` |
| AC-3 jaringan putus tidak menghapus sesi | ✅ Terbukti 10 Nov 2026 | P8: putus 12 detik → tetap di layar tablet, cookie sesi ada |
| AC-4 tablet dinonaktifkan → login ≤ 60 detik | ✅ Terbukti 10 Nov 2026 sesudah perbaikan | P10: 45 detik (sebelumnya 63 detik dengan batas 60) |
| AC-5 tanpa identitas pasien; bed kosong → "Bed belum berpenghuni" | ✅ Terbukti 10 Nov 2026 | P1 (nama, inisial, No. RM nol); P9 (301-A) |
| DoD — lint 0 errors | Terpenuhi | §6 |
| DoD — rekaman login lima akun | Terpenuhi 10 Nov 2026 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-003-20261102/sesudah-2026-11-10T02-29-40-958Z/` (JSON + tangkapan layar) |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | **Regresi redirect seluruh aplikasi** (syarat lead `NV-DEC-030`/`056`) — baru terbukti lewat uji lima akun |
| Status Git | Source sudah di-commit Rizki (`98d029d54`); perbaikan `nurse-view-bed-tablet-constants.jsx` (60 → 45) `M` |
| Langkah berikutnya | Rizki meng-commit perbaikan satu baris dan membangun ulang frontend |
