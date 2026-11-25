# Frontend Roadmap — Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Roadmap ID | `KSK-PO-RM-FE-001` |
| Revision | `1.1` — menambah `FE-KSK-PO-009` (revisi blueprint 2.1, `KSK-DEC-057`) |
| Status | `approved` — Sukma Giri Pratama, 2026-11-19 (`KSK-DEC-055`); r1.1 Sukma Giri Pratama, 2026-11-25 (`KSK-DEC-058`). Wewenang implementasi `GRANTED` untuk seluruh task; pembuatan dan penerapan migration tetap memerlukan konfirmasi terpisah |
| Blueprint | `KSK-BP-001` revisi `2`, `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`), bentuk `SINGLE` |
| Cakupan | `EPIC KSK-PO-05` s.d. `KSK-PO-07`, rilis `MVP-1` s.d. `MVP-3` (`04-prd-to-mvp.md` PO-20) |
| SHA baseline | FE `5f18e9f04` (`sukmagpV2`), BE `5e777833` (`sukmagp`); r1.1: FE `7bba5cd1c`, BE `356bf463` |
| Kontrak masukan | `KSK-CONTRACT-v2` `approved`; r1.1: `KSK-CONTRACT-v2.1` `approved` (`KSK-DEC-058`) (hash per berkas sama dengan [online-registration-backend-roadmap.md](online-registration-backend-roadmap.md)) |
| Masukan desain | `03-frontend-architecture.md` PO-FE.1–7 (`8a5befef750f`), `testing/acceptance-test-matrix.md` PO-T (`d97bb3d86834`), flowchart `pendaftaran-pasien-online.md`, `check-in-pasien-online.md`, `pendaftaran-online-petugas-dan-penutupan.md` |
| Wewenang UI | `KSK-PO-FE-001` s.d. `005`. Rupa, tata letak, bentuk modal, dan kalimat email/WhatsApp `DEV_DISCRETION`; letak menu terkunci (`KSK-DEC-054`) |
| Owner | Sukma Giri Pratama |
| Nama berkas | `online-registration-frontend-roadmap.md`, karena `roadmap/frontend-roadmap.md` sudah dimiliki revisi 1 (`KSK-RM-FE-001`) |

> **Batas dokumen.**
> 1. Roadmap ini **bukan** izin menulis kode. Task dikerjakan lewat `build-module-frontend` setelah roadmap disetujui dan task diminta secara eksplisit.
> 2. Setiap task menjalankan gerbang keputusan base component (`REUSE`/`EXTEND`/`COMPOSE`/`WRAP`/`NEW`) sebelum JSX/CSS pertama. `NEW` dan `EXTEND` yang mengubah perilaku bawaan menunggu keputusan pemilik.
> 3. Warna dan ukuran memakai token CSS; tidak ada nilai literal (memori `warna-css-wajib-token`).
> 4. Kontrak sudah `approved` dan terkunci hash-nya, sehingga service dan state boleh disiapkan sebelum backend selesai. Verifikasi manual tetap menunggu task backend pada kolom `Dependency`.
> 5. Bukti: `npm run lint` pada berkas yang diubah, `npm run build`, dan uji browser/Playwright (layar HP untuk halaman publik, ukuran layar kiosk, layar petugas). Tes otomatis baru opsional; Jest tidak dipakai.
> 6. Commit, push, dan deployment tetap wewenang pemilik.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD terbukti |
| 🟡 | Source ada, kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-KSK-PO-004 ✅ [BE] ─> FE-KSK-PO-001 ✅ ─> FE-KSK-PO-002 ✅ ─┬─> FE-KSK-PO-003 ✅
                                                               │
BE-KSK-PO-005 ✅ [BE] ─────────────────────────────────────────┘

BE-KSK-PO-007 ✅ [BE] ─> FE-KSK-PO-005 ✅ ─┬─> FE-KSK-PO-006 ✅
                                           │
FE-KSK-PO-004 ✅ ──────────────────────────┘

BE-KSK-PO-009 ✅ [BE] ─> FE-KSK-PO-007 ✅ ─┬─> FE-KSK-PO-008 ✅
                                           │
BE-KSK-PO-010 ✅ [BE] ─────────────────────┘

BE-KSK-PO-011 ✅ [BE] ─┬─> FE-KSK-PO-009 ✅
                       │
FE-KSK-PO-003 ✅ ──────┘
```

`[BE]` = task backend pada [online-registration-backend-roadmap.md](online-registration-backend-roadmap.md), cermin baca-saja. Blok keempat menyambung `FE-KSK-PO-003` dari blok pertama.

Pasangan prasyarat → task: 12 (`BE-004→FE-001`, `FE-001→FE-002`, `FE-002→FE-003`, `BE-005→FE-003`, `BE-007→FE-005`, `FE-005→FE-006`, `FE-004→FE-006`, `BE-009→FE-007`, `FE-007→FE-008`, `BE-010→FE-008`, `BE-011→FE-009`, `FE-003→FE-009`), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — untuk `FE-KSK-PO-004`; prasyarat backend `[BE]` untuk lainnya | `FE-KSK-PO-004`; `FE-KSK-PO-001` (setelah `BE-KSK-PO-004`); `FE-KSK-PO-005` (setelah `BE-KSK-PO-007`); `FE-KSK-PO-007` (setelah `BE-KSK-PO-009`) |
| 2 | Gelombang 1 yang ditunggunya | `FE-KSK-PO-002` (`FE-KSK-PO-001`); `FE-KSK-PO-006` (`FE-KSK-PO-005`, `FE-KSK-PO-004`); `FE-KSK-PO-008` (`FE-KSK-PO-007`, `BE-KSK-PO-010`) |
| 3 | `FE-KSK-PO-002`, `BE-KSK-PO-005` | `FE-KSK-PO-003` |
| 4 (r1.1) | `FE-KSK-PO-003`, `BE-KSK-PO-011` | `FE-KSK-PO-009` |

**Gelombang rilis produk:** `MVP-1` = `FE-KSK-PO-001` s.d. `003`; `MVP-2` = `FE-KSK-PO-004` s.d. `006`; `MVP-3` = `FE-KSK-PO-007`, `008`; r1.1 `FE-KSK-PO-009` dirilis bersama `BE-KSK-PO-011`.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-KSK-PO-001` | Fondasi halaman publik | `FR-KSK-PO-040`; `KSK-PO-FE-004` | Grup publik `GET options/*`, `patient-match` | Pola `src/app/priority-arrival/` | Route, layout, daftar jalur publik, CSP, service, konstanta | `BE-KSK-PO-004` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-002` | Langkah Data Pasien, Jenis Pasien, Data Kunjungan, Penjamin | `FR-KSK-PO-041`, `042`; `KSK-DEC-029`, `031`, `043`, `047` | Grup publik | Komponen langkah RJ murni | View dan hook publik | `FE-KSK-PO-001` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-003` | Data Rujukan, Consent, Verifikasi, Kirim, Selesai | `FR-KSK-PO-041`, `042`; `KSK-DEC-034`, `035`, `042`, `044` | Grup publik `POST` | `outpatient-general-consent-step`, `buildWhatsAppUrl` | View dan hook publik, widget Turnstile | `FE-KSK-PO-002`, `BE-KSK-PO-005` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-004` | Varian langkah pindai kartu identitas | `FR-KSK-PO-051`; `KSK-DEC-048`, `053`; `CAP-PO-37` | — (frontend murni) | `kiosk-new-patient-step-scan-identity.jsx` | Prop opsional `lockedFieldNames`, `allowedIdentityTypes` | — | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-005` | Layar Check-in: kode/Lupa Kode, ringkasan, check-in pasien lama, tiket | `FR-KSK-PO-050`; `KSK-DEC-033`, `036`, `040`, `046` | Grup *Online Registration Check-In*; `POST patient-encounters/kiosk` | Cetak tiket, inactivity timeout, kartu beranda | Route kiosk, view, hook, service, `kiosk-home-view.jsx` | `BE-KSK-PO-007` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-006` | Check-in pasien baru | `FR-KSK-PO-051`; `KSK-DEC-032`, `039`, `048`, `053` | Endpoint kiosk pasien, dokumen identitas, kartu asuransi | Helper kiosk Pasien Baru, `findExistingKioskPatientByIdentity` | View dan hook check-in | `FE-KSK-PO-005`, `FE-KSK-PO-004` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-007` | Daftar Pendaftaran Online dan butir menu | `FR-KSK-PO-060`; `KSK-DEC-050`; `KSK-PO-FE-005` | Grup *Online Registration* `GET` | Pola layar Reservasi Prioritas | Route, slice, service, hook, view, `menu-items.jsx` | `BE-KSK-PO-009` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-008` | Detail, Ubah Jadwal, Ubah Kontak, Batalkan | `FR-KSK-PO-061`; `KSK-DEC-051`, `052` | Grup *Online Registration* `GET {id}`, `PUT`, `POST cancel` | Detail RP, `ConfirmModal` | Route detail, view, modal, hook | `FE-KSK-PO-007`, `BE-KSK-PO-010` | Lihat kartu | Lihat kartu | Lihat kartu | Lihat kartu |
| `FE-KSK-PO-009` | Data Rujukan: faskes mitra pada tanggal kunjungan | `FR-KSK-PO-043`; `KSK-DEC-057` | `v2.1`: `GET /public/options/referral-institutions` (`visitDate`); `KSK-VAL-PO-073`, `074` | `use-online-registration-referral-institutions.js`, langkah rujukan RJ | Hook pencarian faskes, service publik, langkah Data Rujukan | `FE-KSK-PO-003`, `BE-KSK-PO-011` | Baris uji `KSK-DEC-057` (E2E) | Uji browser HP 375 px | — Pemilik: implementer | Kriteria terbukti |

## Kartu task

### ✅ `FE-KSK-PO-001` — Fondasi halaman publik

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — `lint:errors` PASS (0 error), `build` PASS (854/854 halaman, `ƒ /online-registration`); browser privat 9/9 PASS (halaman terbuka tanpa login, halaman lain dan awalan mirip dialihkan ke `/login`, CSP memuat Cloudflare pada `script-src`/`frame-src`, skrip + iframe Turnstile termuat dan token uji terbit, konsol tanpa pelanggaran CSP, HP 375 px tanpa gulir mendatar); jalur gagal widget 2/2 PASS; service asli ke backend uji 11/11 PASS (opsi poli/jadwal/asuransi/faskes, `400` hari ini, `404` pencocokan, urutan langkah = menu RJ). Keputusan komponen captcha `NEW` opsi A disetujui user. Catatan go-live: site key belum ada di `build.args` Docker — [laporan](../task/report/frontend/FE-KSK-PO-001.md) |
| Outcome | `src/app/online-registration/page.jsx` dan `layout.jsx` tanpa navbar/sidebar; `/online-registration` terdaftar di `src/proxy.js`, `root-layout-shell.jsx`, `auth-wrapper.jsx`; `next.config.js` mengizinkan `https://challenges.cloudflare.com` pada `script-src` dan `frame-src`; `online-registration-public.service.js` untuk opsi dan pencocokan; konstanta urutan langkah diturunkan dari `outpatient-registration.constants.js`; `NEXT_PUBLIC_TURNSTILE_SITE_KEY` di contoh env |
| Requirement/decision | `FR-KSK-PO-040`; `KSK-PO-FE-004`; `CAP-PO-21`, `29` |
| Kontrak | Grup *Online Registration Public* |
| Reuse | `src/app/priority-arrival/` (layout publik), `InstanceAxios` |
| Cakupan | Berkas di atas saja |
| Dependency | `BE-KSK-PO-004` |
| Acceptance criteria | (1) Browser tanpa sesi login membuka `/online-registration` tanpa dialihkan ke login. (2) Halaman lain tetap memerlukan login. (3) Widget Turnstile dapat dimuat tanpa pelanggaran CSP di konsol. (4) Service memanggil opsi poli/jadwal dan mendapat data dari `BE-KSK-PO-004` |
| Verifikasi | Browser jendela privat; konsol bebas pelanggaran CSP; lint + build |
| Risiko/pemilik | Lupa satu dari tiga daftar jalur publik → redirect login. Pemilik: implementer |
| DoD | Kriteria 1–4 terbukti |

### ✅ `FE-KSK-PO-002` — Langkah Data Pasien, Jenis Pasien, Data Kunjungan, Penjamin

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20; rework 2026-11-23: isian No. RM pasien lama terformat `00-00-00-01` (ESLint 3 berkas 0 error, browser 375 px 11/11 PASS termasuk pencocokan dengan nilai berformat; laporan bagian 9) — ESLint 12 berkas 0 error/0 warning, `lint:errors` PASS, `build` PASS (854/854, `ƒ /online-registration`); Playwright HP 375 px tiga jenis pasien 38/38 PASS (urutan langkah = menu RJ untuk Pribadi/Asuransi/Rujukan, validasi `KSK-VAL-PO-001` s.d. `009`, `020`, `026`, pencocokan pasien lama + captcha sekali pakai → nama tersamar, `404` pesan umum, kartu milik pasien tersamar dan kartu asuransi baru, hari ini dan > H+30 tidak dapat dipilih, kartu jadwal dengan sisa kuota, URL/storage tanpa data pasien, semua request hanya grup publik); regresi menu RJ petugas 13/13 PASS (satu pasien lama per jenis sampai Verifikasi, tanpa menyimpan). UI GATE 8 elemen: REUSE 3, EXTEND 3 (prop opsional, bawaan tetap), COMPOSE 2, NEW 0. Penjamin Pasien Rujukan baru dapat diuji di layar sesudah Data Rujukan (`FE-KSK-PO-003`); langkah `FE-KSK-PO-003` sementara berisi penanda "disiapkan pada tahap berikutnya" — [laporan](../task/report/frontend/FE-KSK-PO-002.md) |
| Outcome | Langkah Data Pasien publik (lama: NIK/No. RM + tanggal lahir + captcha → nama tersamar; baru: NIK, nama, tanggal lahir, jenis kelamin; kolom No. WA/email); Jenis Pasien; Data Kunjungan (tanggal H+1 s.d. H+30, poli, kartu jadwal dengan sisa kuota); Penjamin (kartu milik pasien tersamar, kartu asuransi baru, Tunai). Urutan langkah per jenis pasien sama dengan menu RJ |
| Requirement/decision | `FR-KSK-PO-041`, `042`; `KSK-DEC-029`, `031`, `035`, `043`, `047`; `KSK-PO-FE-001`, `002`; `KSK-DSN-PO-004`, `012`, `013` |
| Kontrak | Grup publik `GET options/*`, `POST patient-match`; `KSK-VAL-PO-001` s.d. `012`, `020` s.d. `026` (sisi `FE`) |
| Reuse | `outpatient-visit-type-step`, `outpatient-payer-step`, `outpatient-schedule-card`, `outpatient-practice-schedule-modal` lewat props; `outpatient-visit-step` mendapat varian sumber data, bukan salinan |
| Cakupan | `src/components/view/online-registration/**`, `use-online-registration.js`, komponen RJ yang perlu varian props |
| Dependency | `FE-KSK-PO-001` |
| Acceptance criteria | AC 1 dan AC 2 (tampilan). Tambahan: (1) menu Pendaftaran Pasien RJ petugas berperilaku sama seperti sebelumnya (regresi komponen bersama); (2) tidak ada data pasien di URL maupun browser storage; (3) tanggal hari ini tidak dapat dipilih |
| Verifikasi | Playwright di ukuran HP untuk tiga jenis pasien; regresi menu RJ petugas untuk satu pasien per jenis; lint + build |
| Risiko/pemilik | Perubahan komponen bersama RJ. Pemilik: implementer |
| DoD | Kriteria terbukti; tabel keputusan base component dan baris `UI GATE` di laporan |

### ✅ `FE-KSK-PO-003` — Data Rujukan, Consent, Verifikasi, Kirim, Selesai

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — ESLint 0 error/0 warning, `lint:errors` PASS, `build` PASS (854/854, `ƒ /online-registration`); Playwright HP 375 px dengan kunci uji Turnstile dan SMTP uji: kiriman 28/29 pada run penuh (S1k gagal karena skrip membaca email base64 mentah), S1 ulang 13/13, kartu asuransi baru 2/2; data backend lewat API petugas 7/7 (consent tersimpan, 2 berkas rujukan, kartu baru, status email); regresi menu RJ petugas 14/14; uji FE-002 ulang 38/38. Terbukti: tombol Kirim "Mengirim..." terkunci dan klik ganda = 1 kiriman; `409` kuota → Data Kunjungan (Rujukan → Data Rujukan, tempat jadwal dipilih) tanpa menghapus isian; `409` ganda nyata → Data Kunjungan; berkas 6 MB/`.exe`/> 3 ditolak; email SMTP uji berisi kode tanpa NIK; tombol WhatsApp `wa.me/62…` berisi kode, tanggal, poli; kode tidak di URL/storage. UI GATE 7 elemen: REUSE 2, EXTEND 2 (ekspor + prop opsional `ReferralDocumentsField`), COMPOSE 3, NEW 0; Data Rujukan dan Verifikasi dirangkai dari bagian langkah RJ, bukan langkah RJ utuh (alasan di laporan) — [laporan](../task/report/frontend/FE-KSK-PO-003.md) |
| Outcome | Data Rujukan (cari faskes, tanggal, nomor, dokter, catatan, alasan, 1–3 berkas ≤ 5 MB); General Consent pasien baru (komponen RJ); Verifikasi dengan widget Turnstile dan Kirim multipart; Selesai (nomor, kode berkelompok, ringkasan, status email, tombol "Kirim kode ke WhatsApp saya", tombol salin) |
| Requirement/decision | `FR-KSK-PO-041`, `042`; `KSK-DEC-034`, `035`, `041`, `042`, `044`; `KSK-PO-FE-003`; `KSK-DSN-PO-011` |
| Kontrak | Grup publik `POST`, `GET options/referral-institutions`; `KSK-VAL-PO-027` s.d. `034` |
| Reuse | `outpatient-general-consent-step`, `outpatient-referral-step` (varian sumber data), `outpatient-verification-step`, `buildWhatsAppUrl` (`priority-reservation.utils.js:190-207`) |
| Cakupan | View dan hook publik |
| Dependency | `FE-KSK-PO-002`, `BE-KSK-PO-005` |
| Acceptance criteria | AC 5, 6, 14, 15 (tampilan dan kiriman). Tambahan: (1) tombol Kirim terkunci selama proses; (2) penolakan `409` kuota mengembalikan pasien ke Data Kunjungan tanpa menghapus isian; (3) kode tidak disimpan di storage |
| Verifikasi | Playwright di ukuran HP dengan kunci uji Turnstile; email SMTP uji; tombol WhatsApp membuka `wa.me` dengan kode |
| Risiko/pemilik | Ukuran tanda tangan dan berkas di jaringan seluler. Pemilik: implementer |
| DoD | Kriteria terbukti |

### ✅ `FE-KSK-PO-004` — Varian langkah pindai kartu identitas

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-19 — `lint:errors` PASS (0 error), `build` PASS (854/854 halaman), render 8/8 PASS (HTML tanpa prop baru identik dengan `HEAD`), uji browser Playwright 2/2 PASS (NIK terkunci tidak berubah saat diketik). `test:unit` tidak punya berkas uji (kondisi lama). Uji di perangkat kiosk dilakukan bersama `FE-KSK-PO-006` — [laporan](../task/report/frontend/FE-KSK-PO-004.md) |
| Outcome | `kiosk-new-patient-step-scan-identity.jsx` menerima prop opsional `lockedFieldNames` (kolom OCR yang tidak dapat diubah) dan `allowedIdentityTypes` (jenis kartu yang ditawarkan). Bawaan: kosong dan semua jenis, sehingga kiosk Pasien Baru tidak berubah |
| Requirement/decision | `FR-KSK-PO-051`; `KSK-DEC-048`, `053`; `CAP-PO-37`; `GATE-PO-14` |
| Kontrak | — (frontend murni) |
| Reuse | Komponen langkah pindai kiosk yang ada |
| Cakupan | `kiosk-new-patient-step-scan-identity.jsx` dan helper terkait |
| Dependency | — |
| Acceptance criteria | (1) Dengan `lockedFieldNames={["nik"]}`, kolom NIK tidak dapat diubah. (2) Dengan `allowedIdentityTypes={["KTP","KIA"]}`, SIM tidak ditawarkan. (3) Kiosk Pasien Baru tanpa prop baru berperilaku sama persis |
| Verifikasi | Uji browser di kiosk Pasien Baru (regresi) dan halaman uji sementara di scratchpad yang tidak di-commit; lint + build |
| Risiko/pemilik | Regresi kiosk Pasien Baru. Pemilik: implementer |
| DoD | Kriteria 1–3 terbukti |

### ✅ `FE-KSK-PO-005` — Layar Check-in: kode/Lupa Kode, ringkasan, pasien lama, tiket

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — ESLint berkas task 0 error, `lint:errors` PASS, `build` PASS (856/856, `○ /kiosk/registration/online-check-in`); uji browser kiosk 1920×1080 dan 1080×1920 36/36 PASS: 26 skenario layar dan jalur gagal (kartu beranda hanya aktif dengan `kioskCanAppointment`, validasi `040`, `404` seragam, `409` dengan tanggal untuk kode dan Lupa Kode, ketukan ganda 1 request, batas waktu 120 detik → beranda, ringkasan, `053`, `046`, tiket + Cetak Ulang, paket pasien baru) dan 10 skenario jalur sukses nyata (data uji dipindah ke tanggal hari ini dengan izin pemilik (satu `UPDATE` 9 baris)): kode → antrean `P005`, Lupa Kode NIK → antrean `P010`, tiket layar = tiket thermal, cetak otomatis 1×, pendaftaran status 2. Belum: cetak di printer thermal kiosk fisik — [laporan](../task/report/frontend/FE-KSK-PO-005.md) |
| Outcome | Route `/kiosk/registration/online-check-in`; pilihan "Saya punya kode" atau "Lupa Kode"; verifikasi; ringkasan; check-in pasien lama lewat `POST patient-encounters/kiosk` dengan `onlineRegistrationId` dan token; tiket dicetak; kartu "Check-in Pasien Online" di beranda diberi route; pembersihan sesi dan batas waktu tanpa sentuhan |
| Requirement/decision | `FR-KSK-PO-050`; `KSK-DEC-033`, `036`, `040`, `046` |
| Kontrak | Grup *Online Registration Check-In*; `POST patient-encounters/kiosk`; `KSK-VAL-PO-040` s.d. `048`, `053` s.d. `055` |
| Reuse | `kiosk-old-patient-step-ticket.jsx`, `thermal-ticket-print.utils.js`, `use-kiosk-inactivity-timeout`, gaya beranda kiosk |
| Cakupan | `src/app/kiosk/registration/online-check-in/`, `src/components/view/kiosk/registration/online-check-in/**`, `use-kiosk-online-check-in.js`, `kiosk-online-check-in.service.js`, `kiosk-home-view.jsx` |
| Dependency | `BE-KSK-PO-007` |
| Acceptance criteria | AC 7, 8, 10, 13 (tampilan). Tambahan: (1) kartu beranda hanya aktif pada perangkat dengan `kioskCanAppointment`; (2) printer gagal → tiket tampil di layar dengan arahan Cetak Antrean |
| Verifikasi | Uji browser dengan akun perangkat kiosk (cookie sesi kiosk), ukuran layar kiosk; lint + build |
| Risiko/pemilik | Token habis saat pasien lambat. Pemilik: implementer |
| DoD | Kriteria terbukti |

### ✅ `FE-KSK-PO-006` — Check-in pasien baru

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — ESLint berkas task 0 error, `lint:errors` PASS, `build` PASS (856/856, `○ /kiosk/registration/online-check-in`); uji browser kiosk dengan backend uji 7185 nyata dan agen pemindai tiruan (payload OCR yang dibaca parser kiosk) 15/15 PASS: langkah Pindai Kartu hanya KTP/KIA (`050`), NIK tidak terbaca `051` dan berbeda `052` menonaktifkan Check-in, NIK terkunci, master baru dari kartu + kontak online lalu dokumen identitas, NIK lama memakai master yang ada, kegagalan dokumen identitas di tengah lalu coba lagi tanpa master ganda (master 0 → 1), dua check-in pasien baru `200` sampai tiket; regresi alur pasien lama `FE-KSK-PO-005` 11/11 PASS. Dikecualikan: uji dengan pemindai Plustek fisik — [laporan](../task/report/frontend/FE-KSK-PO-006.md) |
| Outcome | Untuk paket pasien baru: langkah pindai dengan NIK terkunci dan hanya KTP/KIA; bandingkan NIK dengan `identityNumberForScan`; cari pasien lewat `findExistingKioskPatientByIdentity`; bila belum ada, buat master dan dokumen identitas lewat endpoint kiosk Pasien Baru; buat kartu asuransi baru bila ada; lalu check-in |
| Requirement/decision | `FR-KSK-PO-051`; `KSK-DEC-032`, `039`, `048`, `053` |
| Kontrak | `POST patients/kiosk`, `patient-identity-documents/kiosk`, `patient-insurances/kiosk` (existing); `POST patient-encounters/kiosk`; `KSK-VAL-PO-050` s.d. `052` |
| Reuse | Helper submit kiosk Pasien Baru, `FE-KSK-PO-004` |
| Cakupan | View dan hook check-in |
| Dependency | `FE-KSK-PO-005`, `FE-KSK-PO-004` |
| Acceptance criteria | AC 9, 17 (tampilan dan alur). Tambahan: mencoba ulang setelah kegagalan di tengah tidak membuat master pasien ganda |
| Verifikasi | Uji browser kiosk dengan agen pemindai (atau payload uji yang sama dengan uji kiosk Pasien Baru); lint + build |
| Risiko/pemilik | Kegagalan sebagian (master terbuat, kunjungan gagal). Pemilik: implementer |
| DoD | Kriteria terbukti |

### ✅ `FE-KSK-PO-007` — Daftar Pendaftaran Online dan butir menu

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — ESLint 0 error/0 warning, `lint:errors` PASS, `build` PASS (855/855, `○ /health-services/registration-management/online-registrations`); Playwright 1366 px 22/22 PASS + paginasi ketat PASS (10 + 7 baris, `pageNumber=2`): menu satu grup tepat di bawah Reservasi Prioritas dan terjangkau akun ber-hak; bawaan hari ini s.d. 30 hari; filter status; cari nomor/NIK/nama; kata kunci < 3 tidak dikirim (`068`); rentang 76 hari ditolak (`069`); kosong, gagal + Coba lagi, Atur ulang; kode tidak pernah tampil; akun tanpa hak: menu tersembunyi dan `403` → `AccessDeniedGate`. UI GATE 10 elemen: REUSE 9, COMPOSE 1, NEW 0. Akun uji tanpa hak baru (`TEST-PO007 noread`) dibuat lewat API karena `TEST-PO009 noaccess` sudah diberi Read oleh uji BE-010 — [laporan](../task/report/frontend/FE-KSK-PO-007.md) |
| Outcome | Route `/health-services/registration-management/online-registrations`; ringkasan per status; filter rentang tanggal dan status; pencarian; tabel; `online-registration-slice`, service, hook, konstanta; butir menu "Pendaftaran Online" di grup yang sama dengan "Reservasi Prioritas" dengan `requiredPermission` `OnlineRegistration : Read`; `AccessDeniedGate` |
| Requirement/decision | `FR-KSK-PO-060`; `KSK-DEC-050`; `KSK-PO-FE-005` |
| Kontrak | Grup *Online Registration* `GET`, `GET summary`; `KSK-VAL-PO-068`, `069` |
| Reuse | Pola `priority-reservations` (list client/view, kolom, slice), base component `Hero`, `SummaryCards`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `Pagination`, `AccessDeniedGate`, `ToastStack` |
| Cakupan | Berkas di atas dan `menu-items.jsx` |
| Dependency | `BE-KSK-PO-009` |
| Acceptance criteria | AC 18. Tambahan: keadaan memuat, kosong, dan gagal sesuai PO-FE.3 |
| Verifikasi | Uji browser dengan akun ber-hak dan tanpa hak; lint + build; checklist konsistensi UI |
| Risiko/pemilik | — Pemilik: implementer |
| DoD | Kriteria terbukti; butir menu terjangkau |

### ✅ `FE-KSK-PO-008` — Detail, Ubah Jadwal, Ubah Kontak, Batalkan

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-20 — ESLint 0 error/0 warning, `lint:errors` PASS, `build` PASS (855/855, `ƒ …/online-registrations/[id]`); Playwright 1366 px 19 + 3 butir PASS dengan perubahan nyata ke backend uji: pindah Rab 25 Nov dr. Dewi → Rab 2 Des dr. Arif (poli sama, kuota 14→15 dan 8→7, riwayat lama → baru), jadwal penuh dan `409` kuota (tiruan) ditolak di dialog, ubah kontak + riwayat, RowVersion basi nyata → `409` + muat ulang, batal dengan alasan (< 5 karakter ditolak) → Dibatalkan dan tombol hilang; berkas rujukan dibuka (`200 application/pdf`); akun Read saja tanpa tombol aksi, akun tanpa hak `Akses Ditolak`, id tak dikenal `Pendaftaran tidak ditemukan.`. Bentuk dialog `ConfirmModal` (`DEV_DISCRETION`). Delta kontrak: pilihan jadwal dari `public/options/doctor-schedules`, bukan `doctor-schedule-quotas` (alasan di laporan) — [laporan](../task/report/frontend/FE-KSK-PO-008.md) |
| Outcome | Route detail; bagian pasien, kunjungan, rujukan (buka berkas), kontak, riwayat; aksi Ubah Jadwal (tanggal + jadwal poli yang sama + sisa kuota), Ubah Kontak, Batalkan (alasan wajib, `ConfirmModal`); tombol disembunyikan sesuai hak dan `can*`; `409` memuat ulang |
| Requirement/decision | `FR-KSK-PO-061`; `KSK-DEC-051`, `052`; `KSK-DSN-PO-016` |
| Kontrak | Grup *Online Registration* `GET {id}`, `GET {id}/referral-documents/{documentId}`, `PUT {id}/schedule`, `PUT {id}/contact`, `POST {id}/cancel`; `GET patient-encounters/doctor-schedule-quotas`; `KSK-VAL-PO-060` s.d. `072` |
| Reuse | `priority-reservation-detail-view.jsx`, `priority-reservation-action-modals.jsx`, `ConfirmModal` |
| Cakupan | Route detail, view, modal, hook |
| Dependency | `FE-KSK-PO-007`, `BE-KSK-PO-010` |
| Acceptance criteria | AC 19, 20 (tampilan). Tambahan: bentuk modal/drawer `DEV_DISCRETION` dicatat di laporan |
| Verifikasi | Uji browser: pindah tanggal, jadwal penuh, poli lain, ubah kontak, batal, `RowVersion` basi |
| Risiko/pemilik | — Pemilik: implementer |
| DoD | Kriteria terbukti |

### ✅ `FE-KSK-PO-009` — Data Rujukan: faskes mitra pada tanggal kunjungan

| Field | Isi |
| --- | --- |
| Status | ✅ 2026-11-25 — ESLint berkas task 0 error 0 warning, `lint:errors` PASS, `build` PASS (856/856, `ƒ /online-registration`); uji browser HP 375 px terhadap backend uji 7185 14/14 PASS: pencarian faskes membawa `visitDate`, hanya mitra berlaku muncul (Setiabudi muncul; Kebayoran kedaluwarsa dan Karet Semanggi nonaktif tidak), pesan kosong faskes mitra, tanggal diganti 26 → 27 Nov mengosongkan pilihan faskes dan pencarian ulang memakai tanggal baru, kembali tanpa ganti tanggal tidak mengosongkan, penolakan `073` tampil di Verifikasi lalu pasien kembali ke Data Rujukan, alur Pasien Pribadi tetap. `073` diuji dengan respons tiruan (jalur normal tidak dapat memilih non-mitra; `073` nyata dibuktikan `BE-KSK-PO-011`). Tanpa test otomatis (opsional) — [laporan](../task/report/frontend/FE-KSK-PO-009.md) |
| Outcome | Pencarian faskes di langkah Data Rujukan mengirim `visitDate` dari langkah Data Kunjungan; hasil kosong menampilkan "Faskes mitra tidak ditemukan untuk tanggal kunjungan ini. Bila faskes perujuk Anda tidak ada, silakan daftar di loket pendaftaran."; pilihan faskes dikosongkan bila tanggal kunjungan berubah sesudah faskes dipilih; penolakan kirim `KSK-VAL-PO-073` tampil dan pasien dapat kembali ke Data Rujukan |
| Requirement/decision | `FR-KSK-PO-043`; `KSK-DEC-057`, `058` |
| Kontrak | `KSK-CONTRACT-v2.1`: `GET /public/options/referral-institutions` (`search`, `visitDate`); `KSK-VAL-PO-073`, `074` |
| Reuse | `use-online-registration-referral-institutions.js`, langkah Data Rujukan RJ yang dipakai `FE-KSK-PO-003` |
| Cakupan | Hook pencarian faskes, service publik, hook/langkah Data Rujukan halaman publik |
| Dependency | `FE-KSK-PO-003`, `BE-KSK-PO-011` |
| Acceptance criteria | Baris uji `KSK-DEC-057` (E2E) di PO-T.1: pencarian hanya faskes mitra pada tanggal kunjungan; ubah tanggal sesudah memilih faskes → pilihan dikosongkan; pesan kosong khusus faskes mitra. Tambahan: alur Pasien Pribadi/Asuransi tidak berubah |
| Verifikasi | Uji browser HP 375 px terhadap backend uji; lint + build |
| Risiko/pemilik | Harus dirilis bersama `BE-KSK-PO-011`. Pemilik: implementer |
| DoD | Kriteria terbukti |

## Coverage gap

Tidak ada. Setiap acceptance criteria tampilan pada PO-T punya task frontend; aturan backend murni (AC 3, 4, 11, 12) dibuktikan task backend.
