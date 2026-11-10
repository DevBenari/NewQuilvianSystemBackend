# Laporan Perubahan Frontend — `FE-NV-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-003` |
| Judul | Login TV dialihkan ke papan |
| Slice | `MVP-1` — perangkat dan login TV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-003` |
| Trace | `FR-NV-018`; `NV-DEC-009`, `030`, `032`; `NV-AC-016`, `023`–`027`; `UAT-11`; `03-frontend-architecture.md` §7 |
| Contract version | `0.1.1` — `approved` (Rizki Gunawan, 2026-11-02) |
| Wewenang UI | Route `/nurse-view-display` `DEV_DISCRETION` (`NV-FE-005`); kerangka sementara, isi papan menyusul di `FE-NV-005` |
| Dependency | `BE-NV-003` ✅ |
| Klasifikasi | `HEAVY` — lima berkas bersama yang dilalui seluruh login, syarat lead `NV-DEC-030` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1e6ecf29d`; source di-commit Rizki sebagai `5bb240c65` |
| Commit backend yang dijadikan rujukan | `d936447e` |
| Tanggal | 2026-11-02 |
| Status | ✅ **SELESAI 3 November 2026** — regresi 9/9 dan uji TV 9/9 `PASS` (2 Nov); halaman detail admin Kiosk, Display Antrian, dan Display Nurse View terbuka normal dari sidebar sesudah `next dev` start ulang (3 Nov, `rute-admin-2026-11-03T01-57-26-338Z`). `npx eslint` 0 error. Verifikasi developer, bukan UAT. Riwayat: 🟡 SEBAGIAN 2 November 2026 karena rute admin `404` di dev server belum terbukti penyebabnya |

---

## 1. Keadaan yang ditemukan di awal

Route `/nurse-view-display` belum ada, dan akun perangkat masuk ke dashboard admin. Pola `/queue-display` ternyata
terdaftar di lima berkas, bukan dua seperti daftar `NV-DEC-030`:

| Berkas | Akibat bila route tidak didaftarkan | Bukti |
| --- | --- | --- |
| `root-layout-shell.jsx` | TV tampil dengan sidebar dan navbar admin | `SHELLLESS_PATH_PREFIXES` |
| `auth-wrapper.jsx` | TV yang menyala saat jaringan putus terhapus sesinya dan jatuh ke login | `checkAuthStatus` memanggil `clearIdentity` pada galat jaringan |
| `session-activity-manager.jsx` | Pengelolaan sesi admin ikut berjalan di layar TV | `OPEN_ROUTE_PREFIXES` |

Rizki merestui tiga berkas itu, satu baris per berkas (`NV-DEC-032`).

---

## 2. Proses bisnis dari sisi pengguna

1. Teknisi membuka aplikasi di TV dan mengisi akun perangkat yang dicatat dari layar Display Nurse View.
2. TV langsung masuk `/nurse-view-display`, tanpa sidebar. Untuk sementara layar menampilkan lantai, nama, dan kode
   perangkat, serta "Papan lantai sedang disiapkan."
3. Layar memeriksa perangkat setiap 60 detik.
4. Membuka alamat lain di TV, misalnya `/` atau `/kiosk`, langsung dikembalikan ke papan.

**Jalur tidak normal:**

| Kejadian | Yang dilihat di TV |
| --- | --- |
| Admin menonaktifkan perangkat | Kembali ke layar login paling lambat satu siklus; terukur 59 detik |
| Login perangkat nonaktif dengan sandi benar | "Perangkat display dinonaktifkan. Hubungi IT." |
| Jaringan putus saat TV dinyalakan | Tetap di papan, "Tidak dapat menghubungi server. Sistem mencoba lagi otomatis." dan tombol Coba lagi |
| Akun manusia membuka alamat papan | Dikeluarkan ke layar login |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-base-login.jsx`, `proxy.js`, `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx`,
`app/page.js`, `InstanceAxios.jsx`, `login-slice.jsx`, `app-notification-provider.jsx`, `app/queue-display/*`,
`queue-display-screen-service.jsx`, `AuthController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/nurse-view-display/{layout,page,nurse-view-display-client}.jsx` | **Baru.** Route tanpa shell, pola `queue-display` |
| `src/components/view/nurse-view-display/nurse-view-display-screen-view.jsx` | **Baru.** Kerangka layar |
| `src/lib/hooks/nurse-view-display/use-nurse-view-display-runtime.jsx` | **Baru.** Panggilan berkala `/current`; `401`/`403` → logout lalu login |
| `src/lib/services/nurse-view-display/nurse-view-display-service.jsx` | **Baru.** Panggilan API |
| `src/lib/constants/nurse-view-display/nurse-view-display-constants.jsx` | **Baru.** Path, nama cookie, endpoint |
| `src/style/nurse-view-display/nurse-view-display-screen.module.css` | **Baru.** Memakai token |

**Diff ringkas per berkas bersama (syarat kartu):**

| Berkas bersama | Yang ditambahkan | Yang tidak disentuh |
| --- | --- | --- |
| `src/proxy.js` | Konstanta `NURSE_VIEW_DISPLAY_PATH`, `LOGIN_PATH`; entri daftar publik; fungsi `lockNurseViewAccount` yang hanya bekerja bila cookie `isNurseViewAccount` atau `profileType=NurseViewDevice` ada; dua baris pemanggilan di awal `proxy` | `redirectDeviceAccountFromRoot`, cabang Kiosk dan display antrean |
| `src/lib/hooks/auth/use-base-login.jsx` | `isNurseViewUserInfo`, `syncNurseViewSessionCookie`; di `redirectAfterLogin` cookie disetel atau dihapus, dan `router.replace` memilih path Nurse View bila akunnya perangkat | Baris penentuan `target` Kiosk dan display antrean, alur login sementara display |
| `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx` | Masing-masing tepat satu baris `"/nurse-view-display"` | Selebihnya |

Cookie dihapus pada setiap login akun bukan perangkat, termasuk lewat sidik jari, sehingga browser bekas TV tidak salah
mengalihkan pegawai (N9).

### 3.3 Kepatuhan arsitektur frontend

Alur `app` → `components/view` → `hooks` → `services` → `InstanceAxios`, tanpa Axios instance baru dan tanpa Redux baru.
Tidak ada pola baru. `globals.css` tidak diubah.

**Tabel keputusan base component:**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Layar memuat | `LoadingScreen` | `features/loading/loadingScreen.jsx` | `REUSE` | Prop `text` |
| Tombol Coba lagi | `BaseButton` | `base-features/base-button.jsx` | `REUSE` | `variant="primary"` |
| Panel kerangka | — | Sementara, diganti `FE-NV-005` | `COMPOSE` | CSS module dengan token |

`UI GATE: PASS — tanpa komponen baru.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat display Nurse View..." |
| Kosong | `NOT APPLICABLE`, isi papan di `FE-NV-005` |
| Gagal | Pesan koneksi beserta tombol Coba lagi, dan mencoba lagi otomatis |
| Tanpa hak akses | "Mengalihkan ke layar login..." lalu layar login |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/nurse-view-management/display-runtime/current` | Identitas perangkat, interval muat ulang | policy `NurseViewDisplayRead` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` berkas yang diubah | 0 error; 1 warning `set-state-in-effect` pada baris lama `root-layout-shell.jsx` | `PASS`, warning `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| N1 login TV ke `/nurse-view-display` tanpa sidebar | 18 cek | `PASS` | `tv-2026-11-02T09-11-28-274Z` |
| N2 akun dikunci: `/`, daftar perangkat, `/kiosk`, `/queue-display`, `/unauthorized` dialihkan; muat ulang tetap | 7 cek | `PASS` | Idem |
| N3 panggilan berkala `/current` | 5 cek | `PASS` | Idem |
| N4 jaringan putus: tetap di papan, Coba lagi memulihkan | 5 cek | `PASS` | Idem |
| N5 dinonaktifkan: kembali ke login 59 detik, cookie terhapus | 9 cek | `PASS` | Idem |
| N6 pesan login nonaktif di toast | 8 cek | `PASS` | Idem |
| N7 Atur Ulang Login: sandi baru masuk papan | 9 cek | `PASS` | Idem |
| N8 akun manusia membuka papan → login | 3 cek | `PASS` | Idem |
| N9 cookie basi di browser bekas TV, perawat tetap ke dashboard | 3 cek | `PASS` | Idem |
| Regresi redirect perawat, SuperAdmin, LOKET, Kiosk, display | 5/5 sama dengan baseline | `PASS` | `perbandingan-2026-11-02T09-07-48-904Z` |
| Grep anti-regresi | Warna literal 0, `!important` 0, tombol mentah 0, komentar baru 0 | `PASS` | Keluaran perintah |
| Rute lain tidak terganggu: Kiosk, Display Antrian, Display Nurse View dibuka dari sidebar sampai halaman detail | 3/3 tombol aksi tampil, tanpa "Coming Soon", 0 galat halaman; juga `200` untuk `/health-services/billing-management/billing/charge-reconciliations` yang semula `404` | `PASS` | `rute-admin-2026-11-03T01-57-26-338Z` |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-003-20261102/`.

**Penyimpangan.** Persiapan admin dijalankan lewat endpoint yang dipanggil layar, karena halaman detail admin `404` (§8).
Layar login dan layar TV tetap diuji dari browser.

`MANUAL TEST: DONE — Playwright (Edge headless, ignoreHTTPSErrors) 2026-11-02; 9/9 skenario TV dan 9/9 regresi PASS.`

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`

**Tidak dijalankan:** `npm run build` (`NOT RUN`: dev server aktif dan memakai folder `.next` yang sama; DoD kartu tidak
memintanya).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nurse View → `/nurse-view-display` | Terpenuhi | N1 |
| `NV-AC-016` TV kembali ke login | Terpenuhi | N5 |
| `NV-AC-023` perawat → dashboard, tanpa cookie `isNurseViewAccount` | Terpenuhi | Perbandingan, N9 |
| `NV-AC-024` Kiosk → `/kiosk` | Terpenuhi | Perbandingan |
| `NV-AC-025` display antrean → `/queue-display` | Terpenuhi | Perbandingan |
| `NV-AC-026` SuperAdmin → dashboard; papan ditolak | Terpenuhi | Perbandingan, N8 |
| `NV-AC-027` pesan gagal di layar sama | Terpenuhi, dengan pengecualian `NV-DEC-031` | Perbandingan, N6 |
| `UAT-11` | Terverifikasi developer; UAT belum dijalankan | Perbandingan |
| DoD — lint lulus | Terpenuhi | §6 |
| DoD — rekaman login empat akun | Terpenuhi | §6 |
| DoD — diff ringkas berkas bersama | Terpenuhi | §3.2 |
| Tidak mengganggu rute lain | Terpenuhi | §6 baris terakhir, §8. Route `/nurse-view-display` tetap ada dan rute lain menjawab normal |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` `NOT RUN` |
| Masalah yang diketahui | **Ditutup 3 Nov: rute admin `404` di dev server.** `next dev` yang start pukul 15.10 dan 15.23 tanggal 2 Nov mencocokkan banyak rute ke `/_not-found`; yang start pukul 08.53 tanggal 3 Nov normal. **Dugaan cache Turbopack terbukti keliru**: cache tidak dihapus (7,7 GB) dan rute tetap kembali normal, tanpa perubahan kode rute. Penyebab pastinya tidak terbukti. Yang teramati: RAM bebas saat kedua start bermasalah sangat rendah (0,7 GB pada pukul 15.11). Klasifikasi `EXISTING / ENVIRONMENT ISSUE`: rute dan kode yang sama berjalan normal pada start berikutnya |
| Dependency backend | `BE-NV-003` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Permintaan menghentikan `next dev` milik Rizki ditolak pengaman sesi; tidak dilakukan. Percobaan start kedua gagal sendiri karena server Rizki masih memegang kunci |
| Status Git | Source task ini ada di commit `5bb240c65`; laporan di `61b5e3f35` (frontend) dan `6489c32c` (backend). Pembaruan status ini belum di-commit |
| Langkah berikutnya | `BE-NV-005` + `FE-NV-005` |
