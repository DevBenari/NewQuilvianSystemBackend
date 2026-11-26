# Laporan Perubahan Frontend — `FE-KSK-PO-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-PO-009` |
| Judul | Data Rujukan: faskes mitra pada tanggal kunjungan |
| Slice | `EPIC KSK-PO-01` — halaman publik (`FR-KSK-PO-043`), roadmap r1.1, rilis `MVP-1` (bersama `BE-KSK-PO-011`) |
| Roadmap | [online-registration-frontend-roadmap.md](../../../roadmap/online-registration-frontend-roadmap.md) — kartu `FE-KSK-PO-009` |
| Trace | `KSK-DEC-057`, `058`; `KSK-DSN-PO-018`; desain baris "Data Rujukan" di [03-frontend-architecture.md](../../../03-frontend-architecture.md) |
| Contract version | `KSK-CONTRACT-v2.1` — `approved` (Sukma Giri Pratama, 2026-11-25, `KSK-DEC-058`): `GET /public/options/referral-institutions` (`search`, `visitDate`); `KSK-VAL-PO-073`, `074` |
| Wewenang UI | Bunyi pesan kosong dari desain. Tidak ada elemen tampilan baru |
| Dependency | `FE-KSK-PO-003` ✅, `BE-KSK-PO-011` ✅ 2026-11-25 |
| Klasifikasi | `LOW` — perubahan perilaku pencarian dan pengosongan isian di halaman yang sudah ada, tanpa layar baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: hook pencarian faskes, hook halaman online, service publik, konstanta pesan; laporan dan baris status blueprint kiosk (repository backend) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7bba5cd1c` (`sukmagpV2`) + `FE-KSK-PO-005`, `006`, dan rework `FE-KSK-PO-002` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `356bf463` (`sukmagp`) + `BE-KSK-PO-011` yang belum di-commit |
| Tanggal | 2026-11-25 |
| Status | ✅ Selesai — tiga perilaku pada baris uji `KSK-DEC-057` terbukti di browser HP 375 px terhadap backend uji nyata; tampilan `073` diuji dengan kiriman tiruan |

## 1. Keadaan yang ditemukan di awal

- Langkah Data Rujukan halaman publik mencari faskes hanya dengan kata kunci (`search`). Sejak `BE-KSK-PO-011`, backend mewajibkan `visitDate`, sehingga pencarian lama selalu mendapat `400` "Pilih tanggal kunjungan lebih dulu."
- Pesan kosong masih umum: "Faskes tidak ditemukan."
- Faskes yang sudah dipilih tetap tersimpan walaupun pasien kembali ke Data Kunjungan dan mengganti tanggal.
- Penolakan kirim selain `409` sudah tampil di langkah Verifikasi dalam kotak "Pendaftaran belum terkirim", dengan pesan dari backend apa adanya.

## 2. Proses bisnis dari sisi pengguna

1. Pasien Rujukan memilih tanggal kunjungan di langkah Data Kunjungan, misalnya 26 November 2026, lalu lanjut ke Data Rujukan.
2. Di kolom **Faskes Perujuk**, pasien mengetik minimal 3 huruf. Contoh: "setiabudi" menampilkan "Puskesmas Kecamatan Setiabudi (MMC-FRJ-001)". Hanya faskes mitra yang perjanjiannya berlaku pada 26 November yang muncul.
3. Bila faskes yang dicari bukan mitra pada tanggal itu, daftar kosong dan tertulis: "Faskes mitra tidak ditemukan untuk tanggal kunjungan ini. Bila faskes perujuk Anda tidak ada, silakan daftar di loket pendaftaran." Contoh: "kebayoran" (perjanjian sudah berakhir) dan "semanggi" (mitra nonaktif).
4. Bila pasien kembali ke Data Kunjungan lalu mengganti tanggal menjadi 27 November, pilihan faskes dikosongkan. Saat menekan Lanjut tanpa memilih ulang, muncul "Pilih faskes perujuk." Pencarian berikutnya memakai tanggal 27 November.
5. Bila pasien kembali tanpa mengganti tanggal, faskes yang sudah dipilih tetap ada.
6. Bila backend menolak kiriman karena faskes tidak lagi bermitra pada tanggal kunjungan (`KSK-VAL-PO-073`), misalnya perjanjian berakhir di antara saat memilih dan saat mengirim, langkah Verifikasi menampilkan "Pendaftaran belum terkirim — Faskes perujuk ini belum bekerja sama dengan rumah sakit untuk tanggal kunjungan Anda. Pilih faskes lain, atau daftar di loket pendaftaran." Pasien menekan **Kembali** tiga kali (Verifikasi → General Consent → Penjamin → Data Rujukan) untuk mengganti faskes. Isian lain tetap utuh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`online-registration-referral-step.jsx` (pemakai `institutions` dan `emptyText`), `online-registration-verification-step.jsx` (tampilan pesan kirim), `use-online-registration-visit-options.js` (pola `visitDate`), `online-registration.utils.js` (`isOnlineVisitDateAllowed`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/online-registration/online-registration-public.service.js` | `searchOnlineRegistrationReferralInstitutions({ search, visitDate })` mengirim query `visitDate` |
| `src/lib/hooks/online-registration/use-online-registration-referral-institutions.js` | Menerima `{ visitDate }`. Pencarian membawa tanggal itu; tanpa tanggal yang sah, request tidak dikirim dan muncul "Pilih tanggal kunjungan lebih dulu." (`074` di sisi browser). Hasil pencarian dibuang setiap kali tanggal berubah |
| `src/lib/hooks/online-registration/use-online-registration.js` | Hook pencarian dipanggil sesudah `visitDate` dibaca dan diberi tanggal itu. Efek baru mengosongkan `referralInstitutionId` dan `referralInstitutionOption` saat tanggal kunjungan berubah (bukan saat pertama tampil) |
| `src/lib/constants/online-registration/online-registration.constants.js` | `REFERRAL_SEARCH_EMPTY` berbunyi pesan faskes mitra; `REFERRAL_VISIT_DATE_REQUIRED` baru |

Penolakan `073` tidak memerlukan kode baru: jalur kirim sudah meneruskan pesan backend ke kotak "Pendaftaran belum terkirim" di Verifikasi, dan tombol Kembali sudah ada.

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Penerapan |
| --- | --- |
| Tidak menambah arsitektur paralel | Memperluas hook dan service yang ada; tanpa Redux, tanpa hook baru |
| View tidak memanggil HTTP | View → hook → service, tidak berubah |
| Privasi (PO-FE.7) | `visitDate` bukan data pasien; isian tetap hanya di memori komponen |
| Teks Bahasa Indonesia | Pesan dari konstanta, sama persis dengan desain dan validation matrix |

### 3.4 Gerbang keputusan base component

`UI GATE: PASS — REUSE 2, COMPOSE 0, EXTEND 0, NEW 0`

| Elemen | Kandidat | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kolom Faskes Perujuk dengan pencarian server | `EmergencySelectField` (`serverSide`, `searchable`, `emptyText`) | Dipakai sejak `FE-KSK-PO-003` | REUSE | Hanya teks `emptyText` yang berubah |
| Pesan penolakan kirim | `EmergencyInlineAlert` "Pendaftaran belum terkirim" | `FE-KSK-PO-003` | REUSE | Tanpa diubah |

Tidak ada elemen berstatus selain `REUSE`, sehingga tidak ada pilihan yang perlu diputuskan.

## 4. State yang ditangani di layar

| Keadaan | Penanganan |
| --- | --- |
| Kata kunci kurang dari 3 huruf | Petunjuk "Ketik minimal 3 huruf nama atau kode faskes." (tidak berubah) |
| Memuat | Indikator muat pada kolom (tidak berubah) |
| Kosong | Pesan faskes mitra (baru) |
| Tanggal kunjungan belum sah | "Pilih tanggal kunjungan lebih dulu."; request tidak dikirim |
| Gagal jaringan / `429` | Pesan backend atau pesan umum (tidak berubah) |
| Tanggal kunjungan berubah | Pilihan faskes dan daftar hasil dikosongkan |
| Kirim ditolak `073` | Kotak "Pendaftaran belum terkirim" di Verifikasi; tombol Kirim aktif lagi sesudah captcha baru |

## 5. Endpoint yang dikonsumsi

### [Tags("Health Services / Registration Management / Online Registration Public")]

Base URL: `api/v1/health-services/registration-management/online-registrations/public`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/options/referral-institutions` | Cari faskes perujuk mitra pada tanggal kunjungan | Tanpa login | Query `search` (≥ 3 huruf), `visitDate` (`yyyy-MM-dd`) | `ApiResponse<List<{ id, code, name }>>` | `200`, `400` (`074`), `429` |
| `POST` | `` | Kirim pendaftaran (tidak berubah) | Tanpa login, captcha | Multipart | Tidak berubah | Tambahan yang ditampilkan: `400` `073` |

Contoh request yang terekam saat uji: `GET …/options/referral-institutions?search=setiabudi&visitDate=2026-11-26`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas task | 0 error, 0 warning | `PASS` | Keluaran eslint |
| `npm run lint:errors` | Exit 0 (`eslint . --quiet`) | `PASS` | `scratchpad/fe-po009-lint.log` |
| `npm run build` | Exit 0; 856/856 halaman, `ƒ /online-registration` | `PASS` | `scratchpad/fe-po009-build.log` |
| Grep anti-regresi pada diff | Tanpa warna literal, tanpa `console` | `PASS` | Grep |
| Uji browser HP 375 px, `/online-registration` di dev server 3000, backend uji 7185 nyata | 14/14 PASS (rincian di bawah) | `PASS` | `scratchpad/fe-po009/browser.cjs`, `run1.log`, `r1-empty-list-375.png`, `r3-referral-375.png`, `r4-073-375.png` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — repository tanpa test runner; uji browser di atas sebagai bukti | — | — |

Rincian uji browser:

| ID | Skenario | Hasil |
| --- | --- | --- |
| R1a | Rujukan, kunjungan 26 Nov, cari "setiabudi": request membawa `visitDate=2026-11-26`; Puskesmas Kecamatan Setiabudi muncul | PASS |
| R1b | Semua request opsi faskes membawa `visitDate` | PASS |
| R1c | "kebayoran" (perjanjian kedaluwarsa): tanpa hasil, pesan faskes mitra tampil, pesan lama tidak tampil | PASS |
| R1d | "semanggi" (mitra nonaktif): tanpa hasil, pesan sama | PASS |
| R1e | Faskes dipilih | PASS |
| R2 | Kembali ke Data Kunjungan tanpa mengganti tanggal: faskes tetap terpilih | PASS |
| R3a | Tanggal diganti 26 → 27 Nov: kolom faskes kembali ke placeholder | PASS |
| R3b | Lanjut tanpa memilih ulang: "Pilih faskes perujuk." | PASS |
| R3c | Pencarian ulang membawa `visitDate=2026-11-27`; daftar lama tidak terbawa | PASS |
| R3d | Data Rujukan 375 px tanpa gulir mendatar (`overflow=0`) | PASS |
| R4a | Kirim ditolak `073` (respons tiruan dengan pesan persis backend): pesan di Verifikasi berjudul "Pendaftaran belum terkirim"; satu `POST` | PASS |
| R4b | Kembali 3 kali sampai Data Rujukan; faskes masih terisi untuk diganti | PASS |
| R5 | Regresi Pasien Pribadi: Data Kunjungan → General Consent tetap; tanpa request opsi faskes | PASS |
| R6 | Tanpa `pageerror` | PASS |

`MANUAL TEST`: dijalankan lewat Playwright pada viewport HP 375 px; tangkapan layar diperiksa (pesan kosong terbaca utuh di dalam daftar pilihan, kotak `073` di atas tombol Kirim).

Penolakan `073` diuji dengan respons tiruan karena halaman tidak dapat memilih faskes non-mitra lewat jalur normal: daftar hanya memuat mitra, dan pilihan dikosongkan saat tanggal berubah. Penolakan `073` nyata dari backend sudah dibuktikan `BE-KSK-PO-011` (S2).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Baris uji `KSK-DEC-057` (1) E2E: pencarian hanya faskes mitra pada tanggal kunjungan | Terpenuhi | R1a–R1d |
| Baris uji `KSK-DEC-057` (3) E2E: ubah tanggal sesudah memilih faskes → pilihan dikosongkan; pencarian ulang memakai tanggal baru | Terpenuhi | R3a–R3c, R2 (tanpa salah kosong) |
| Pesan kosong khusus faskes mitra | Terpenuhi | R1c, R1d, `r1-empty-list-375.png` |
| Penolakan `073` tampil; pasien dapat kembali ke Data Rujukan | Terpenuhi | R4a, R4b |
| Tambahan: alur Pasien Pribadi/Asuransi tidak berubah | Terpenuhi untuk Pribadi (R5). Asuransi memakai langkah Data Kunjungan yang sama dan tidak memanggil pencarian faskes | R5, kode |
| DoD: lint dan build | Terpenuhi | Bagian 6 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Harus dirilis bersama `BE-KSK-PO-011`: frontend ini mengirim `visitDate` yang diabaikan backend lama (aman), tetapi frontend lama terhadap backend baru gagal mencari faskes |
| Data uji | Tidak ada data baru: pencarian hanya membaca, kiriman `073` ditiru di browser. Dua pasien uji di layar tidak pernah dikirim |
| Perubahan sampingan | `NONE`. Dev server 3000 milik user (PID 404) dipakai apa adanya; percobaan agent menyalakan dev server kedua berhenti sendiri karena port sudah dipakai |
| Status Git | Empat berkas frontend diubah, belum di-commit. Tidak ada commit, push, atau migration |
| Langkah berikutnya | Commit bersama `BE-KSK-PO-011` oleh pemilik; pembersihan data uji modul |
