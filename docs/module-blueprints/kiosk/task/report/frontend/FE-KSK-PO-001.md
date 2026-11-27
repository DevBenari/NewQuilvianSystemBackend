# Laporan Perubahan Frontend — `FE-KSK-PO-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-001` |
| Judul | Fondasi halaman publik |
| Slice | `EPIC KSK-PO-02` — halaman publik frontend (`FR-KSK-PO-040`), gelombang 1, rilis `MVP-1` |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-001` |
| Trace | `KSK-PO-FE-004`; `CAP-PO-21`, `29`; `KSK-DEC-029`, `044`, `047` |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), grup *Online Registration Public* |
| Wewenang UI | `KSK-PO-FE-004` (`DEV_DISCRETION`, ikut konvensi frontend). Keputusan komponen captcha: opsi A, dipilih Sukma Giri Pratama 2026-11-20 |
| Dependency | `BE-KSK-PO-004` ✅ |
| Klasifikasi | `MEDIUM` — route publik baru tanpa login, perubahan CSP global, komponen fitur baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: route, view, komponen fitur, hook, service, konstanta, util, style, daftar jalur publik, `next.config.js`, berkas env; laporan dan baris status di blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `508c8beb8` (`sukmagp`) |
| Commit backend yang dijadikan rujukan | `93f0fa1b` (`sukmagp`) + perubahan `BE-KSK-PO-008`/`010` yang belum di-commit |
| Tanggal | 2026-11-20 |
| Status | ✅ Selesai — kriteria 1–4 terbukti di browser dan lewat service asli |

## 1. Keadaan yang ditemukan di awal

Belum ada route publik untuk pendaftaran online. Satu-satunya halaman pasien tanpa login adalah `/priority-arrival`, yang terdaftar di **empat** daftar jalur publik. Roadmap hanya menyebut tiga: `proxy.js`, `root-layout-shell.jsx`, dan `auth-wrapper.jsx`; daftar keempat ada di `session-activity-manager.jsx`. CSP di `next.config.js` hanya mengizinkan skrip dan iframe dari origin sendiri, sehingga widget Cloudflare akan diblokir. Belum ada komponen captcha apa pun di source (`grep -rn turnstile|captcha src` → 0 hasil).

## 2. Proses bisnis dari sisi pengguna

1. Pasien membuka `/online-registration` dari HP atau komputer tanpa akun. Halaman langsung tampil tanpa diminta login dan tanpa menu aplikasi.
2. Bagian atas menampilkan identitas rumah sakit dan judul "Pendaftaran Pasien Online". Di bawahnya ada pengingat: "Pendaftaran online dibuka untuk besok sampai 30 hari ke depan. Untuk berobat hari ini, silakan daftar di kiosk rumah sakit."
3. Bar langkah menampilkan urutan yang sama dengan menu Pendaftaran Pasien RJ. Contoh Pasien Pribadi pasien baru: Data Pasien, Jenis Pasien, Data Kunjungan, General Consent, Verifikasi & Konfirmasi, Selesai.
4. Panel langkah untuk sementara hanya berisi widget verifikasi keamanan. Isi langkah menyusul di `FE-KSK-PO-002` dan `003`, dan widget berpindah ke langkah Data Pasien.
5. Bila widget tidak dapat dimuat, pasien melihat "Verifikasi keamanan belum siap, muat ulang halaman." beserta tombol "Muat ulang verifikasi".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md`, `rules/frontend/frontend-architecture.md`; `src/app/priority-arrival/**`, `priority-arrival-view.jsx`, `priority-arrival.module.css` (rujukan halaman publik); `proxy.js`, `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx`; `next.config.js` (pembentuk CSP); `InstanceAxios.jsx` (`_skipAuthRefresh`, redirect 401/403); `priority-reservation.service.js` (pola service publik); `outpatient-registration.constants.js`, `outpatient-referral.utils.js#getOutpatientRegistrationSteps`, `registration-category-utils.js`; `outpatient-registration-page.jsx`, `emergency-registration-stepper.jsx`; katalog `base-features/`; `env`, `.env`, `Dockerfile`, `docker-compose.yml` (hanya nama variabel); `tests/helpers/alias-resolver.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/online-registration/layout.jsx` | **Baru.** Tanpa shell aplikasi, `force-dynamic` (pola `priority-arrival`) |
| `src/app/online-registration/page.jsx` | **Baru.** Metadata `noindex`, `no-referrer`; merender view dengan widget captcha di panel |
| `src/components/view/online-registration/online-registration-view.jsx` | **Baru.** Identitas RS, pengingat rentang tanggal, bar langkah, panel langkah (`children`) |
| `src/components/features/online-registration/turnstile-widget.jsx` | **Baru** (keputusan A). Wadah widget, render eksplisit, `onToken`/`onExpire`/`onError`, `resetKey`, pesan belum siap + tombol muat ulang |
| `src/lib/hooks/online-registration/use-turnstile-script.js` | **Baru.** Pemuat skrip sekali per halaman, batas waktu 15 detik, status `loading`/`ready`/`failed`/`unconfigured`, percobaan ulang |
| `src/lib/services/online-registration/online-registration-public.service.js` | **Baru.** Opsi poli, jadwal, penyedia asuransi, faskes; pencocokan pasien. Semua lewat `InstanceAxios` dengan `_skipAuthRefresh` |
| `src/lib/constants/online-registration/online-registration.constants.js` | **Baru.** Route, endpoint, enum, rentang H+1 s.d. H+30, kalimat langkah publik, konfigurasi Turnstile, pesan baku |
| `src/utils/online-registration/online-registration-steps.utils.js` | **Baru.** `getOnlineRegistrationSteps` memakai `getOutpatientRegistrationSteps` menu RJ; consent hanya pasien baru |
| `src/style/online-registration/online-registration.module.css` | **Baru.** Hanya token global; ramah HP |
| `src/proxy.js`, `src/components/providers/root-layout-shell.jsx`, `src/components/ui/auth-wrapper.jsx`, `src/components/providers/session-activity-manager.jsx` | `/online-registration` sebagai jalur publik |
| `next.config.js` | `https://challenges.cloudflare.com` pada `script-src` dan `frame-src` |
| `env` (contoh) dan `.env` (dev) | `NEXT_PUBLIC_TURNSTILE_SITE_KEY` berisi kunci uji publik Cloudflare yang selalu lolos, dengan catatan wajib diganti di produksi |

**Delta terhadap roadmap:**

| Roadmap | Source | Alasan |
| --- | --- | --- |
| Jalur publik di tiga daftar | Empat daftar, ditambah `session-activity-manager.jsx` | `/priority-arrival` juga ada di sana; tanpa itu pelacak sesi menganggap halaman publik sebagai halaman ber-sesi |
| `NEXT_PUBLIC_TURNSTILE_SITE_KEY` di contoh env | Juga di `.env` dev | Dev server membaca `.env`; tanpa nilai, widget tidak dapat dibuktikan termuat. Nilainya kunci uji publik; token darinya ditolak secret produksi |
| Berkas di cakupan | Ditambah komponen `TurnstileWidget`, hook pemuat skrip, view, util langkah, dan CSS module | Kriteria 3 menuntut widget termuat di halaman. Komponen captcha disetujui sebagai `NEW` (opsi A); view dan util menjaga `page.jsx` tetap tipis |

### 3.3 Kepatuhan arsitektur frontend

| Butir | Hasil |
| --- | --- |
| `page.jsx` hanya entry point dan metadata | Ya |
| Boundary Server/Client minimal | `page.jsx` dan `layout.jsx` server; `"use client"` hanya pada view, widget, dan hook |
| Request lewat `InstanceAxios`, tanpa Axios baru | Ya, dengan `_skipAuthRefresh` |
| Endpoint dan pesan tidak tersebar sebagai string | Semua di `online-registration.constants.js` |
| Alias `@/` | Ya |
| Token desain | Warna, spasi, radius, bayangan, dan tipografi lewat token. Ukuran tata letak literal (lebar 960 px, logo 64/48 px, breakpoint 640 px) mengikuti pola `priority-arrival.module.css` |

**Gerbang keputusan base component**

UI GATE: 5 elemen — REUSE 2, EXTEND 0, COMPOSE 2, WRAP 0, NEW 1

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kerangka halaman publik (latar, lebar, jarak) | Tidak ada base; pola `priority-arrival` | `priority-arrival.module.css` | COMPOSE | CSS module token baru |
| Kepala halaman (logo, nama RS, judul, keterangan) | `Hero`; pola brand `priority-arrival` | `Hero` dipakai 537 view internal; halaman publik dan menu RJ tidak memakainya | COMPOSE | Pola brand `priority-arrival` (opsi A, tanpa keberatan user) |
| Pengingat rentang tanggal | `InformationAlert` | `base-features/information-alert.jsx` | REUSE | `variant="info"`, `closable={false}` |
| Bar langkah | `EmergencyRegistrationStepper` | Dipakai `outpatient-registration-page.jsx` | REUSE | Langkah dari util online |
| Widget captcha Turnstile | Tidak ada | `grep -rn turnstile\|captcha src` → 0; tidak ada base yang merender iframe pihak ketiga; tidak dapat dirangkai | NEW | Opsi A disetujui user 2026-11-20 |

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat skrip captcha | Wadah widget kosong sampai skrip siap |
| Site key kosong | `InformationAlert` peringatan "Verifikasi keamanan belum siap, muat ulang halaman." tanpa memuat skrip |
| Skrip gagal/batas waktu | Pesan yang sama + tombol "Muat ulang verifikasi" |
| Token kedaluwarsa/galat | Diteruskan ke pemanggil lewat `onExpire`/`onError` (dipakai `FE-KSK-PO-002`/`003`) |
| HP 375 px | Tanpa gulir mendatar; bar langkah menyusut menjadi nomor |

## 5. Endpoint yang dikonsumsi

Grup *Online Registration Public* (`api/v1/health-services/registration-management/online-registrations/public`): `GET options/clinics`, `GET options/doctor-schedules`, `GET options/insurance-providers`, `GET options/referral-institutions`, `POST patient-match`. Kirim pendaftaran (`POST` root) disiapkan di konstanta untuk `FE-KSK-PO-003`.

## 6. Verifikasi

Dev server FE di `http://localhost:3000` (saat itu kosong; dijalankan untuk uji lalu dihentikan) dan backend uji `https://localhost:7185` dari source saat ini dengan kunci uji Turnstile. Request browser ke `7184` dialihkan Playwright ke `7185`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | 0 error | `PASS` | Keluaran lint |
| ESLint berkas yang disentuh | 0 error, 1 warning lama di `root-layout-shell.jsx:120` (`react-hooks/set-state-in-effect`) | `EXISTING WARNING` | Keluaran ESLint |
| `npm run build` | Compiled successfully, 854/854 halaman, `ƒ /online-registration`, standalone siap | `PASS` | Log build |
| P1 kriteria 1 | Konteks browser tanpa cookie: `/online-registration` `200`, tetap di route itu, judul tampil, tanpa sidebar, 6 langkah | `PASS` | `browser-check.cjs` |
| P2 kriteria 2 | Tanpa login: `/health-services/registration-management/outpatient-registration` dan `/home` dialihkan ke `/login`; awalan mirip `/online-registrations-x` juga dialihkan | `PASS` | `browser-check.cjs` |
| P3 kriteria 3 | Header CSP memuat `https://challenges.cloudflare.com` pada `script-src` dan `frame-src`; skrip termuat, iframe Cloudflare ada, token uji terbit; konsol tanpa pesan "Content Security Policy"/"Refused to …" | `PASS` | `browser-check.cjs`, tangkapan layar 1280 px dan 375 px |
| P4 tampilan HP | 375 px tanpa gulir mendatar | `PASS` | `browser-check.cjs` |
| W1–W2 jalur gagal widget | Cloudflare diblokir → pesan belum siap + tombol muat ulang; blokir dilepas → tombol memuat ulang skrip dan token terbit | `PASS` | `widget-fail.cjs` |
| S1–S6 kriteria 4 (service asli lewat `InstanceAxios` ke 7185) | Poli besok `200` (4 poli, hanya `id`/`name`); jadwal Poli Paru `200` dengan sisa kuota; penyedia asuransi `200`; faskes `200`; tanggal hari ini `400` pesan H+1 s.d. H+30; pencocokan pasien tak dikenal `404` pesan seragam | `PASS` | `service-check.mjs` |
| S7–S8 urutan langkah | Pribadi baru/lama, Asuransi lama, Rujukan baru: urutan nomor langkah identik dengan `getOutpatientRegistrationSteps`; judul Rujukan "Data Pasien > Jenis Pasien > Data Kunjungan > Data Rujukan > Penjamin > General Consent > Verifikasi & Konfirmasi > Selesai" | `PASS` | `service-check.mjs` |
| Grep konsistensi UI | 0 warna literal, 0 tombol/tabel mentah, 0 utilitas Bootstrap, 0 `!important`; tipografi hanya token | `PASS` | Bagian 3.3 |
| `npm run test:unit` | — | `AUTOMATED TEST: SKIPPED (opsional)` | Tidak ada test baru (kebijakan test opsional); runner `npm run test:unit` di mesin ini gagal mengurai glob dengan Node 20 dan baseline-nya sudah gagal di modul lain |

Run Playwright pertama 7/9 karena skrip uji: iframe Turnstile berada di shadow DOM sehingga selector tidak menemukannya, dan redirect login dicek terlalu cepat. Cara cek diperbaiki, hasil 9/9.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tanpa sesi login membuka `/online-registration` tanpa dialihkan ke login | Terpenuhi | P1 |
| (2) Halaman lain tetap memerlukan login | Terpenuhi | P2 |
| (3) Widget Turnstile termuat tanpa pelanggaran CSP di konsol | Terpenuhi | P3, W1–W2 |
| (4) Service memanggil opsi poli/jadwal dan mendapat data dari `BE-KSK-PO-004` | Terpenuhi | S1–S2 |
| DoD: kriteria 1–4 terbukti | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Go-live | `NEXT_PUBLIC_TURNSTILE_SITE_KEY` belum ada di `build.args` `docker-compose.yml` maupun `ARG` `Dockerfile`; semua `NEXT_PUBLIC_*` ditanam saat build. Tanpa itu, image produksi tidak punya site key dan widget menampilkan pesan belum siap. Termasuk prasyarat IT `KSK-PO-OQ-016` (di luar cakupan task ini) |
| Risiko | Kunci uji di `env`/`.env` harus diganti site key RS di produksi. Bila terlewat, token ditolak secret produksi (fail-closed) dan pasien tidak dapat mengirim |
| Lingkungan | Dev server FE di 3000 dan backend 7185 dijalankan untuk uji lalu dihentikan; backend pengguna di 7184 sedang mati dan tidak disentuh. `.env` diubah (satu baris variabel publik baru) |
| Perubahan sampingan | `NONE`. Skrip uji dan tangkapan layar ada di scratchpad `fe-po001/` |
| Status Git | 7 berkas diubah (`.env`, `env`, `next.config.js`, 4 berkas daftar jalur publik) dan 8 folder baru di bawah `src/`. Tidak ada commit |
| Langkah berikutnya | `FE-KSK-PO-002` (langkah Data Pasien, Jenis Pasien, Data Kunjungan, Penjamin) memakai service, util langkah, dan `TurnstileWidget` ini |
