# Laporan Perubahan Frontend — `FE-RWI-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-032` |
| Judul | Admisi yang ditinggal dapat dilanjutkan |
| Slice | `F11 — Aksi yang hilang` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, `FE-RWI-032` |
| Trace | `RWI-DEC-076`; `03-frontend-architecture.md` 3A.6; `IA-INP-02`; skema tampilan `FE-INP-16` bagian 6 → alur bagian 3 |
| Contract version | `RWI-BED-BOARD-RESERVATION-001 1.0.0`, approved, dibuka `BE-RWI-036`. Tidak ada kontrak baru yang diminta task ini |
| Wewenang UI | Bentuk tautan `DEV_DISCRETION` |
| Dependency | `FE-RWI-020` ✅ selesai; `FE-RWI-026` ✅ selesai; `BE-RWI-036` ✅ selesai |
| Klasifikasi | `MEDIUM` — satu hook baru, tujuh berkas disunting, tanpa route baru, tanpa komponen baru, menyentuh dua titik tulis yang sudah selesai |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta roadmap dan `requirement-traceability.md` modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `24011d182` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `3a03648` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Kriteria 1, 2, 4, dan 5 terpenuhi penuh. Kriteria 3 **diterima apa adanya oleh pemilik pekerjaan**: perpindahan langkahnya benar, dan keterangan "pemesanan sebelumnya gugur" sengaja tidak ditulis karena kontrak baca backend tidak membedakan pemesanan yang lewat batas dari yang tidak pernah ada — kalimat yang dipakai dibuat benar untuk kedua keadaan. Batas itu tidak dihapus dari laporan ini; rinciannya tetap pada bagian 7.1 |

---

## 1. Keadaan yang ditemukan di awal

Alur admisi sembilan langkah sudah berdiri lengkap. Yang belum ada adalah pintu masuknya
kembali: sekali halaman ditutup, tidak ada satu pun cara membuka kembali admisi yang sama.

| Yang sudah ada | Yang belum ada |
| --- | --- |
| Alur berlangkah dengan langkah aktif tersimpan di URL — `FE-RWI-022` | Tidak ada parameter URL yang menyebutkan episode mana yang sedang dikerjakan |
| Daftar kerja episode yang dapat menyaring `Draft` — `FE-RWI-020` | Barisnya hanya dapat dibuka ke Detail Episode, tidak ke alur admisinya |
| Langkah Dokter yang membuat kunjungan dan episode — `FE-RWI-025` | Tidak ada cara mengadopsi episode yang sudah ada; membuka alur lagi berarti membuat episode kedua |
| Langkah tempat tidur beserta pemesanannya — `FE-RWI-026` | Pemesanan hanya bertahan selama layar hidup. Komentar pada `use-inpatient-admission-bed.jsx` menuliskannya apa adanya: "pemesanan hanya bertahan selama sesi layar ini hidup" |

Akibatnya nyata: petugas yang terputus di tengah alur meninggalkan episode `Draft` yatim di
server, lalu memulai admisi baru untuk pasien yang sama. Episode pertama tidak pernah
tersentuh lagi, dan tempat tidur yang sempat dipesannya menunggu masa berlakunya habis.

Satu hal berubah sejak laporan `FE-RWI-026` ditulis. Catatan di sana menyebut
`RWI-UI-GAP-003` masih terbuka. `BE-RWI-036` sudah menutupnya — bukan lewat endpoint
pemesanan baru, melainkan lewat papan tempat tidur yang kini menyebutkan
`HoldingEpisodeId`, `ReservationId`, dan `ReservationExpiresAt`.

---

## 2. Proses bisnis dari sisi pengguna

Penggunanya petugas admisi. Layar dibuka ketika sebuah admisi perlu diteruskan — giliran
kerja berganti, peramban tertutup, atau pasien pergi sebentar lalu kembali.

### 2.1 Urutan yang dilakukan petugas

1. Petugas membuka **Daftar Kerja Episode**, lalu menyaring status **Sedang disiapkan**.
2. Setiap baris `Draft` kini punya tombol **Lanjutkan Admisi** di samping **Detail
   Episode**. Baris berstatus lain tidak punya tombol itu sama sekali.
3. Menekan tombol membuka alur admisi pada langkah yang tepat, bukan pada langkah pertama.
   Langkah tujuannya ditentukan satu hal: apakah episode itu masih memegang pemesanan
   tempat tidur menurut server.
4. Di atas alur muncul keterangan bahwa admisi ini sedang dilanjutkan, beserta nomor
   episodenya dan keadaan pemesanannya.
5. Petugas meneruskan pekerjaannya. Pasien, penjamin, unit layanan, kelas perawatan, dan
   DPJP sudah terisi dari admisi yang tersimpan — tidak ada yang perlu diketik ulang.

### 2.2 Dua tujuan pelanjutan

| Keadaan episode | Langkah tujuan | Alasannya |
| --- | --- | --- |
| Masih memegang pemesanan aktif | **Konfirmasi** | Tempat tidurnya sudah dipesan, jadi tinggal ditinjau lalu ditutup. Mengembalikannya ke Pilih Bed justru berbahaya: satu episode hanya boleh memegang satu pemesanan aktif, dan pemesanan kedua akan ditolak `409` |
| Tidak memegang pemesanan | **Pilih Bed** | Pekerjaan yang tersisa memang memilih tempat tidur |

Pada tujuan Konfirmasi, sisa waktu pemesanan terbaca pada ringkasan beserta nama tempat
tidurnya, dan hitung mundurnya berjalan seperti pada alur admisi biasa.

### 2.3 Contoh konkret

Episode `RWI-2026-000009` ditinggal petugas malam sesudah memesan Bed 07 di kamar Melati 2.
Petugas pagi membuka daftar kerja, menyaring **Sedang disiapkan**, dan melihat baris itu
bertanda **Memegang pemesanan** dengan keterangan "Bed 07 — Melati 2" serta "Sisa sekitar
12 menit." Ia menekan **Lanjutkan Admisi**, dan layar membuka langkah **Konfirmasi**
lengkap dengan nama pasien, penjamin, kelas, DPJP, dan hitung mundur tempat tidurnya.

Episode `RWI-2026-000011` ditinggal lebih awal, sebelum langkah tempat tidur. Barisnya
bertanda **Tanpa pemesanan aktif**, dan menekan tombol yang sama membuka langkah **Pilih
Bed** dengan seluruh isian sebelumnya sudah terisi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; `rules/GLOBAL_RULES.md`;
`rules/frontend/base-component-decision-gate.md`; `rules/frontend/ui-consistency-checklist.md`;
`rules/frontend/REPORT_TEMPLATE.md`; roadmap `FE-RWI-032`; alur admisi
(`inpatient-admission-view.jsx`, `use-inpatient-admission-flow.jsx`,
`use-inpatient-admission-doctor.jsx`, `use-inpatient-admission-bed.jsx`,
`use-inpatient-admission-confirmation.jsx`, `use-inpatient-consent-print.jsx`);
daftar kerja (`inpatient-episode-worklist-view.jsx`, `use-inpatient-episode-worklist.jsx`);
serta source backend read-only `InpatientBedOccupancyController.cs`,
`InpBedOccupancyService.cs`, `InpatientEpisodeDtos.cs`, dan `InpatientEpisodeReadDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-resume.jsx` | **Baru.** Membaca `GET /episodes/{id}`, memastikan statusnya `Draft`, lalu membaca penjamin dari kunjungan jangkarnya. Tidak menulis apa pun |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Menambah kunci URL `episodeId`, route alur admisi, pembangun tautan pelanjutan, kalimat tetap alur pelanjutan, dan pemetaan dua langkah tujuan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` | Membaca `episodeId` dari URL dan mempertahankannya lintas langkah; membersihkannya ketika petugas memilih jalur masuk baru atau keluar dari alur |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | Menerima `resumeEpisode`, mengisi isian langkah Dokter dari jawaban server, dan **mengunci titik tulis 1** supaya pelanjutan tidak pernah membuat kunjungan dan episode kedua |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | Menerima `adoptExistingReservation`; pemesanan yang masih dipegang episode dibaca dari papan. Sumber pemesanan disusun ulang menjadi "keputusan layar menang atas jawaban papan" |
| `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | Menambah `findBoardReservationForEpisode` |
| `src/utils/health-services/inpatient-management/inpatient-episode-worklist-utils.jsx` | Menambah `resolveEpisodeResumeStep` |
| `src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx` | Tombol **Lanjutkan Admisi** pada baris `Draft` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Menyambungkan controller pelanjutan, keadaan memuat/gagal/ditolak, keterangan pelanjutan, dan penjamin hasil pembacaan kunjungan |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya tetap `view → hook → service → InstanceAxios`. Keputusan murni berada di
`utils`, kalimat baku di `constants`, pembacaan API di `hook`. Tidak ada route baru, package
baru, base component baru, Redux slice baru, Axios instance baru, maupun abstraksi baru.
Pembacaan kunjungan memakai `fetchInpatientEncounterDetail` dan
`buildInpatientPayerSummaryFromEncounter` yang sudah dipakai halaman cetak persetujuan
`FE-RWI-028` — bukan jalur kedua yang ditulis ulang.

Satu keputusan rancangan layak disebut. Pemesanan hasil pemulihan **diturunkan saat render**,
bukan disalin ke state lewat effect. Rancangan pertama memakai effect dan langsung ditandai
ESLint sebagai `react-hooks/set-state-in-effect` — jumlah warning naik dari 571 menjadi 572.
Perbaikannya bukan mematikan aturan, melainkan mengubah bentuknya: `localReservation`
menyimpan keputusan layar, `reservationDecided` menandai keputusan itu sudah pernah diambil,
dan selama belum ada keputusan yang berlaku adalah jawaban papan. Susunan ini sekaligus
menutup jebakan yang lebih berbahaya: tanpa penanda keputusan, pemesanan yang baru saja
**dibatalkan** petugas akan diadopsi kembali dari papan yang belum sempat dimuat ulang,
sehingga pembatalannya terlihat seolah gagal.

### 3.4 Gerbang keputusan base component

`UI GATE: PASS` — tiga elemen, seluruhnya `REUSE`, tidak ada `NEW` maupun `EXTEND`.

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Tombol **Lanjutkan Admisi** | `BaseButton` | sudah dipakai dua kali pada sel Aksi yang sama | `REUSE` |
| Keterangan keadaan pelanjutan | `InformationAlert` | sudah di-import `inpatient-admission-view.jsx`; `variant` `info`/`warning`/`danger` didukung | `REUSE` |
| Keadaan gagal membaca episode | `InformationAlert` + `BaseButton` | pola yang sama dipakai daftar kerja untuk `loadError` | `REUSE` |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membaca admisi yang akan dilanjutkan..." Tidak ada satu pun langkah yang dirender selama episodenya belum terbaca, supaya petugas tidak bekerja di atas admisi yang belum tentu boleh dilanjutkan |
| Kosong | Tidak berlaku — pelanjutan selalu menunjuk satu episode tertentu. Daftar kerja yang kosong tetap memakai kalimatnya sendiri |
| Gagal | Pesan server ditampilkan apa adanya beserta tombol **Coba Lagi**. Kegagalan membaca kunjungan **tidak** menggagalkan pelanjutan; yang hilang hanya ringkasan penjaminnya |
| Ditolak | Episode yang statusnya bukan `Draft` menampilkan "Admisi ini sudah tidak berstatus Sedang disiapkan..." dan alur tidak dirender sama sekali |
| Tanpa hak akses | Penolakan `401`/`403` tetap ditangani lapisan autentikasi dan Axios yang sudah ada. Task ini tidak menambah pemeriksaan hak akses baru |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Membaca admisi yang akan dilanjutkan beserta pasien, unit layanan, kelas, DPJP, isolasi, dan catatannya | `InpatientEpisode : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Mengetahui apakah episode ini masih memegang pemesanan, tempat tidurnya, dan batas waktunya | `InpatientBedOccupancy : Read` |

#### Health Services / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-encounters/admin/{id}` | Membaca penjamin yang tersimpan pada kunjungan jangkar episode | `PatientEncounter : Read` |

Ketiganya **sudah ada** sebelum task ini dan tidak berubah. Tidak ada endpoint tulis yang
dipanggil: pelanjutan hanya membaca, dan seluruh penulisan tetap milik titik tulis yang
sudah ada.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa keluaran | `PASS` | Keluaran perintah |
| `npm run lint` | `571 problems (0 errors, 571 warnings)` — **sama persis** dengan garis dasar, dan **nol** warning pada kesembilan berkas task ini | `PASS` | Keluaran perintah |
| `npm run build` | `✓ Compiled successfully in 27.1s`; route `/health-services/inpatient-management/admissions` dan `.../episodes` terdaftar; `postbuild` selesai | `PASS` | Keluaran perintah |
| Enam grep anti-regresi UI | Nol tombol non-base, nol `<table>` mentah, nol nilai visual literal, nol `!important`. Satu-satunya hit `fw-semibold` adalah komentar lama yang justru melarangnya | `PASS` | Keluaran grep |
| Test `.mjs` | Tidak ditulis dan tidak dijalankan | `NOT REQUIRED` | Pemilik pekerjaan menyatakan pengujian `.mjs` tidak diperlukan |
| Uji manual di peramban | Tidak dijalankan | `NOT REQUIRED` | Pemilik pekerjaan menyatakan cukup bukti source |

Uji manual: `NOT REQUIRED`.

`AUTOMATED TEST: SKIPPED (opsional) — pemilik pekerjaan menyatakan pengujian .mjs tidak diperlukan.`

**Peringatan yang muncul dan sudah ditutup.** Satu warning ESLint baru sempat muncul
(`react-hooks/set-state-in-effect` pada `use-inpatient-admission-bed.jsx`) akibat rancangan
pertama menyalin pemesanan dari papan ke state lewat effect. Ditutup dengan mengubah
rancangannya menjadi nilai turunan; sesudah itu jumlah warning kembali ke garis dasar 571
dengan nol warning pada berkas task ini.

**Tidak dijalankan:** `npm run test`, `npm run test:unit`, `npm run test:e2e`, dan
`npm run test:uat`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode `Draft` tanpa pemesanan dilanjutkan ke langkah **Pilih Bed** | **Terpenuhi** | `resolveEpisodeResumeStep` mengembalikan `bed-selection` ketika baris tidak punya pemesanan pada indeks papan; `buildInpatientAdmissionResumeRoute` menuliskannya sebagai `step=bed-selection` |
| 2. Episode `Draft` dengan pemesanan aktif dilanjutkan ke langkah **Konfirmasi**, dan sisa waktu pemesanannya terbaca | **Terpenuhi** | `resolveEpisodeResumeStep` mengembalikan `confirmation`; `findBoardReservationForEpisode` memulihkan pemesanannya dari papan, lalu `remainingMs` dan `countdownLabel` milik `use-inpatient-admission-bed` mengalir ke `InpatientAdmissionConfirmationStep` lewat prop `reservation` dan `remainingMs` yang sudah ada |
| 3. Episode `Draft` yang pemesanannya sudah gugur dilanjutkan ke langkah **Pilih Bed** disertai keterangan bahwa pemesanan sebelumnya gugur | **Terpenuhi sebagian** | Perpindahan langkahnya **terpenuhi** — tujuannya `bed-selection`, sama seperti kriteria 1. Keterangannya **tidak** menyebut kata "gugur"; alasannya pada bagian 7.1 |
| 4. Langkah yang sudah lewat tidak meminta pengguna mengetik ulang data yang sudah tersimpan | **Terpenuhi** | Pasien dari `episode.patientId`; unit layanan, DPJP, catatan, dan kebutuhan isolasi mengisi ulang isian langkah Dokter; kelas perawatan dan nama pasien dibaca ulang langkah Konfirmasi dari `GET /episodes/{id}`; penjamin dibaca dari kunjungan jangkarnya |
| 5. Episode selain `Draft` tidak menawarkan pelanjutan | **Terpenuhi** | Dua lapis. Lapis pertama: `resolveEpisodeResumeStep` mengembalikan nilai kosong untuk status selain `Draft`, sehingga tombolnya tidak dirender. Lapis kedua: `use-inpatient-admission-resume` memeriksa ulang status dari jawaban server dan menolak melanjutkan walaupun URL diketik langsung |

### 7.1 Kenapa kriteria 3 hanya terpenuhi sebagian

Kriteria 3 menuntut layar menuliskan bahwa **pemesanan sebelumnya gugur**. Menuliskannya
berarti layar harus dapat membedakan dua keadaan:

- episode yang **pernah** memesan tempat tidur lalu pemesanannya lewat batas waktu; dan
- episode yang **belum pernah** memesan apa pun.

Kontrak baca hari ini tidak dapat membedakan keduanya. Dibaca langsung dari source backend
pada commit `3a03648`:

| Yang diperiksa | Hasil |
| --- | --- |
| `GET /bed-occupancies/bed-board` | Hanya memuat pemesanan `Active` yang batas waktunya belum lewat — penyaringnya `ReservationStatus == Active && ExpiresAt > cutoff` pada `InpBedOccupancyService`. Pemesanan yang sudah lewat tidak pernah muncul |
| Operasi baca pemesanan per episode | **Tidak ada.** `InpatientBedOccupancyController` hanya punya tiga `HttpGet`: `available-beds`, `bed-board`, dan `placements/by-episode/{episodeId}` — yang terakhir untuk penempatan, bukan pemesanan |
| `InpatientEpisodeDetailResponse` dan `InpatientEpisodeListItemResponse` | **Nol** kolom pemesanan pada keduanya |

Karena itu kedua keadaan di atas sama-sama tampil sebagai "tidak ada pemesanan yang
berlaku". Menuliskan "pemesanan sebelumnya sudah gugur" pada episode yang sebenarnya belum
pernah memesan berarti memberi tahu petugas sesuatu yang tidak diketahui server.

Kalimat yang dipakai karena itu dibuat benar untuk kedua keadaan sekaligus:

> Admisi dilanjutkan. Episode ini sedang tidak memegang pemesanan tempat tidur, jadi pilih
> tempat tidurnya kembali. Pemesanan sebelumnya, bila memang pernah ada, sudah tidak
> berlaku.

Roadmap menandai gerbang skema `RWI-UI-GAP-003` sudah ditutup `BE-RWI-036` untuk task ini.
Penutupan itu benar untuk kriteria 1, 2, dan 4 — dan memang itulah yang membuat pemulihan
pemesanan mungkin. Untuk kriteria 3 penutupannya **belum lengkap**, dan itu dilaporkan di
sini alih-alih ditutup dengan tebakan dari sisi frontend. Yang dibutuhkan satu kontrak baca
yang menyebutkan pemesanan terakhir sebuah episode beserta status akhirnya.

### 7.2 Definition of Done

DoD roadmap: "Kelima kriteria lulus; e2e ada dan lulus."

- Kelima kriteria lulus — **empat penuh, satu diterima apa adanya**. Pemilik pekerjaan
  menutup task ini pada 1 September 2026 dengan kriteria 3 dalam bentuk yang dijelaskan pada
  bagian 7.1: perpindahan langkahnya terpenuhi, keterangan "gugur" tidak ditulis karena
  server tidak mengetahuinya. Keputusan itu dicatat di sini supaya pembaca berikutnya tahu
  bahwa yang kurang adalah kontrak backend, bukan pekerjaan layar yang terlewat.
- E2E ada dan lulus — **dikecualikan atas keputusan pengguna 1 September 2026**, sejalan
  dengan bagian "Keputusan penutupan verifikasi" pada roadmap.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning ESLint baru sempat muncul lalu ditutup dengan mengubah rancangan; jumlah warning kembali ke garis dasar 571 |
| Masalah yang diketahui | Kriteria 3 tidak dapat menuliskan "pemesanan gugur"; lihat bagian 7.1. Ketika papan tempat tidur tidak terbaca dari daftar kerja, tujuan pelanjutan selalu **Pilih Bed** — bila ternyata pemesanannya masih hidup, alur admisi membacanya sendiri dari papan sehingga petugas tetap tidak dapat memesan tempat tidur kedua |
| Dependency backend | `BE-RWI-036` ✅ selesai dan dipakai. Untuk menutup kriteria 3 dibutuhkan kontrak baca pemesanan per episode yang belum ada |
| Perubahan sampingan | `NONE` |
| Interupsi | Dua arahan pengguna diterima di tengah pekerjaan, keduanya menyatakan pengujian `.mjs` tidak diperlukan. Diterapkan tanpa membatalkan pekerjaan yang sudah berjalan |
| Status Git frontend | Sembilan berkas milik task ini berubah atau baru pada branch `HamzahV2`; tidak ada `git add`, commit, push, pull, merge, rebase, maupun deploy. Rinciannya pada bagian 8.1 |
| Status Git backend | Hanya berkas laporan ini, `frontend-roadmap.md`, dan `requirement-traceability.md` modul Rawat Inap yang disentuh. Tidak ada source backend yang diubah |
| Langkah berikutnya | Putuskan bersama pemilik Backend/API apakah kontrak baca pemesanan per episode akan dibuka; sesudah itu kriteria 3 dapat ditutup penuh tanpa mengubah rancangan yang sudah ada |

### 8.1 Berkas milik task ini, dipisahkan dari pekerjaan paralel

Working tree saat laporan ini ditulis juga memuat pekerjaan lain yang sedang berjalan
(`FE-RWI-030`, papan tempat tidur). Berkas-berkas itu **tidak** disentuh task ini dan
sengaja dibiarkan apa adanya.

| Berkas | Milik |
| --- | --- |
| `use-inpatient-admission-resume.jsx` (baru) | `FE-RWI-032` |
| `inpatient-admission-flow-constants.jsx` | `FE-RWI-032` |
| `use-inpatient-admission-flow.jsx` | `FE-RWI-032` |
| `use-inpatient-admission-doctor.jsx` | `FE-RWI-032` |
| `use-inpatient-admission-bed.jsx` | `FE-RWI-032` |
| `inpatient-episode-worklist-utils.jsx` | `FE-RWI-032` |
| `inpatient-admission-view.jsx` | `FE-RWI-032` |
| `inpatient-episode-worklist-view.jsx` | `FE-RWI-032` |
| `inpatient-bed-utils.jsx` | **Berbagi.** `findBoardReservationForEpisode` milik `FE-RWI-032`; penambahan kolom reservation pada `normalizeBedBoard` sudah ada di working tree sebelum task ini dan justru menjadi prasyaratnya |
| `inpatient-bed-board.jsx`, `inpatient-bed-board-view.jsx`, `use-inpatient-bed-board-actions.jsx` | **Bukan** task ini — pekerjaan paralel `FE-RWI-030` |
