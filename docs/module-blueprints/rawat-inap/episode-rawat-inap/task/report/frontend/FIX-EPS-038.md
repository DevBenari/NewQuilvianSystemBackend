# Laporan Perubahan Frontend — `FIX-EPS-038`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-038` (mencakup `FIX-EPS-038-01` s.d. `FIX-EPS-038-08`) |
| Judul | Langkah 4 admisi bayi sekali klik, boks rawat gabung lintas lantai, dan pemesanan tanpa perintah muat ulang |
| Slice | Repair langkah 4 "Penempatan dan Pemesanan" admisi bayi baru lahir |
| Roadmap | **Tidak ada entri roadmap.** Repair ini mengikuti pola `FIX-EPS-030`/`FIX-EPS-031`: register status dipegang [PLAN-REPAIR-EPS-038](../../../docs/plan-repair/plan-repair-038-klik-ganda-penempatan-dan-boks-rawat-gabung.md) bagian 1 |
| Trace | [ISSUE-EPS-038](../../../docs/issue/issue-038-klik-ganda-penempatan-dan-boks-rawat-gabung.md); kontrak [newborn-admission-repair-022](../../../contracts/newborn-admission-repair-022.md) bagian 1 invarian 8 dan bagian 2 langkah 4; `PLAN-REPAIR-EPS-031`, `-033`, `-037` |
| Contract version | `RWI-BAYI-022-2.0.0` (disetujui); endpoint backend dipakai apa adanya, tanpa perubahan kontrak |
| Wewenang UI | Disetujui pemilik 23-10-2026: seluruh plan repair beserta rekomendasi K-01 (boks otomatis bila satu, daftar pilihan bila lebih), K-02 (Draft yatim gugur otomatis + batal manual, tanpa skrip data), K-03 (teks kartu Rawat Gabung umum) |
| Dependency | Tidak ada task backend; seluruh endpoint sudah tersedia |
| Klasifikasi | `HEAVY` — 10 berkas source, 3 hook pengendali bersama (bed, dokter, alur) dan view yang dipakai lima route admisi |
| Task mode | `FRONTEND` (backend baca-saja, kecuali laporan ini dan register issue/plan repair 038) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; backend hanya `docs/module-blueprints/rawat-inap/episode-rawat-inap/{task/report/frontend/FIX-EPS-038.md, docs/issue/issue-038-…, docs/plan-repair/plan-repair-038-…}` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Basis `2b16ec441` (`HamzahV2`). Selama pengerjaan, pemilik meng-commit perubahan source ini sebagai `9de9f17cc` ("feat: implement improvements for newborn admission flow and bed management", 23-10-2026 10.30 WIB) setelah merge `origin/QuilvianIntegrationFrontend`. Perubahan tes `tests/unit/floor-plan-admission-selection.test.mjs` yang dibuat sesudahnya di-commit pemilik sebagai `229f071bb` (10.45 WIB) |
| Commit backend yang dijadikan rujukan | `d79e3ed1` saat analisis; HEAD sekarang `b6c638ee` (`MHamzah`). Berkas backend yang dirujuk tidak berubah di antara keduanya |
| Tanggal | 2026-10-23 |
| Status | 🟡 SEBAGIAN — source, lint, unit test, dan build selesai; uji manual alur hidup (skenario S1–S10 plan repair) belum dilakukan karena agent tidak dapat login ke aplikasi |

---

## 1. Keadaan yang ditemukan di awal

Bukti lengkap ada di ISSUE-EPS-038. Ringkasnya:

| Temuan | Bukti di source basis `2b16ec441` |
| --- | --- |
| Klik pertama "Lanjut ke Pemesanan" membuat episode, lalu URL berisi `episodeId` memicu gerbang pelanjutan yang menyembunyikan seluruh langkah; `NewbornPlacementStep` dipasang ulang dan state lokal `inBookingView` hilang | `inpatient-admission-view.jsx:1174`, `inpatient-newborn-admission-steps.jsx:370`, `:452-459` |
| Boks rawat gabung dicari pada denah satu lantai (lantai terakhir di `localStorage`), padahal papan `available-beds`/`bed-board` kamar ibu lintas lantai sudah diambil tetapi tidak dibaca | `use-inpatient-admission-bed.jsx:320-359`, `inpatient-bed-utils.jsx:366-412` |
| Tombol "Pesan Boks Bayi & Lanjut" memanggil `bed.reserve` milik render lama sesudah `await prepareBooking`; scope `useBedOperation` berganti sehingga muncul "Sesi berubah. Muat ulang data…" walau server sudah memesan | `inpatient-newborn-admission-steps.jsx:414-447`, `use-bed-operation.js:24-26` |
| Draf sesi `newborn:active` dipakai bersama semua admisi bayi; kunci lama tidak dibersihkan | `use-inpatient-admission-flow.jsx:147-151`, `:186-220` |
| Kandidat ibu hanya halaman pertama (50 kasus) | `use-inpatient-newborn-admission.js:49-52`, `:77-112` |
| Isi pengganti "VIP", "Bed Ibu", dan teks kartu "Lantai 3" | `inpatient-newborn-admission-steps.jsx:226-231`, `:411`; `inpatient-newborn-admission-constants.js:21-23` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas admisi rawat inap atau petugas bangsal bayi. **Pemicu:** bayi lahir dan tim
klinis memutuskan rencana perawatan.

### 2.1 Perinatologi atau NICU

1. Petugas sampai di langkah 4, memilih tempat di denah (misalnya `NICU-1`), lalu menekan
   **Lanjut ke Pemesanan** satu kali.
2. Tombol berbunyi "Menyimpan admisi..."; kunjungan dan episode `Draft` bayi dibuat server.
3. Nomor episode langsung dicatat di alamat halaman, dan kartu **Pesan Tempat Tidur** tampil tanpa
   layar berkedip kosong dan tanpa banner "Melanjutkan admisi".
4. Petugas menekan **Pesan Tempat Tidur**, mengonfirmasi, lalu **Lanjut ke Persetujuan Wali**.

### 2.2 Rawat gabung dengan ibu

1. Langkah 4 menampilkan kartu konfirmasi boks bayi di kamar ibu, berapa pun lantai kamarnya.
2. Bila kamar ibu punya tepat satu boks kosong, boks itu terpilih otomatis. Bila lebih dari satu,
   petugas memilih dari daftar **Boks Bayi** (K-01).
3. Petugas menekan **Pesan Boks Bayi & Lanjut** satu kali. Layar membuat episode (bila belum ada),
   lalu memesan boks memakai pengendali yang sudah mengenal episode baru, dan maju ke Persetujuan
   Wali dengan hitung mundur 15 menit.

### 2.3 Jalur tidak normal

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Kamar ibu tidak punya boks (di luar VIP 301–305) | "Kamar ibu (Kelas 1 · 204) tidak memiliki boks bayi…" + tombol **Ubah ke Perawatan Perinatologi** |
| Boks ada tetapi dipakai/dibersihkan | "Boks 301-BOKS di VIP 301 saat ini berstatus "Terisi"…" + **Periksa Ulang** dan **Ubah ke Perawatan Perinatologi** |
| Kamar ibu belum terbaca (kandidat ibu tidak termuat) | "Kamar penempatan ibu belum terbaca…" + **Periksa Ulang**; layar juga mencari ulang kasus ibu memakai No. RM ibu |
| Kamar ibu tidak aktif (penempatan ibu selesai) | Banner merah "Kamar Penempatan Ibu Closed / Tidak Aktif" + **Ubah ke Mode Neonatal Terpisah** |
| Jawaban pemesanan hilang tetapi server sudah memesan | Papan dibaca ulang; pemesanan milik episode ini diadopsi dengan pesan "Pemesanan tempat tidur untuk admisi ini sudah tercatat di server, jadi tidak perlu dipesan ulang." |
| Hasil operasi tidak pasti (jaringan putus) | Panel pemulihan existing **Periksa dan Ulangi Operasi yang Sama** kini juga tampil pada rawat gabung |
| Muat ulang setelah episode dibuat | Episode dibaca ulang dari URL (alur pelanjutan); tidak ada episode kedua |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend dan backend; `rules/frontend/frontend-architecture.md`,
`base-component-decision-gate.md`, `ui-consistency-checklist.md`, `test-policy.md`,
`REPORT_TEMPLATE.md`; ISSUE/PLAN-REPAIR 033, 037, 038; kontrak 022; source
`use-inpatient-bed-board.jsx`, `use-floor-plan.js`, `use-inpatient-admission-resume.jsx`,
`use-bed-operation.js`, `bed-operation.service.js`, `inpatient-bed-utils.jsx`,
`inpatient-admission-bed-step.jsx`, `base-form-control.jsx`, `bed-operation-recovery.jsx`;
backend `InpBedOccupancyService.BedManagement.cs` (filter `roomId`/`isForNewborn` pada `bed-board`),
`InpNewbornAdmissionRules.cs`, `InpatientBedOccupancyController.cs`, `InpatientEpisodeController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan | FIX |
| --- | --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | `isOwnEpisode`; gerbang render dan dua banner pelanjutan tidak berlaku untuk episode milik layar; admisi bayi menulis `episodeId` ke URL juga di langkah Pilih Bed | 01 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | State `createdEpisodeId` diisi saat `POST /episodes` berhasil, dikosongkan saat pasien berganti | 01 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | `bookingViewOpen`/`openBookingView`/`closeBookingView` (terikat pasien, tertutup saat pilihan dilepas); mode `isRoomingInNewborn` memakai papan `bed-board`/`available-beds` dan tidak memuat denah; `requestReservation` + efek pemesanan tertunda; adopsi pemesanan dari papan pada galat; `prepareBooking` mengutamakan bed yang dikirim; `episodeId` ikut dikembalikan | 02, 03, 04 |
| `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` | `NewbornPlacementStep` memakai `resolveNewbornPlacementView` (state lokal `inBookingView` dihapus); `RoomingInPlacementConfirmation` ditulis ulang: boks dari `pickRoomingInCrib`, tiga keadaan, pilihan boks bila > 1, `BedOperationRecovery`, isi pengganti "Belum terbaca"; tombol pesan dua tahap tanpa `bed.reserve` sesudah `await` | 02, 03, 04, 07 |
| `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js` | Fungsi murni `resolveNewbornPlacementView` dan `pickRoomingInCrib` | 02, 03 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` | Draf bayi tidak disimpan/dipulihkan pada kunci `newborn:active`; kunci itu dihapus saat halaman dibuka; kunci pasien lama dihapus saat berpindah ke kunci episode | 05 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` | `knownCandidates` + pencarian ulang kandidat ibu lewat No. RM bila tidak ada di halaman pertama | 06 |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Pesan `reservationRecovered` | 04 |
| `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js` | Kartu Rawat Gabung: badge "Kamar Ibu", butir "Di kamar ibu yang memiliki boks bayi" (K-03) | 08 |
| `tests/unit/inpatient-newborn-admission.test.mjs` | Tes FIX-EPS-033 berbasis teks diganti tes perilaku; 4 tes baru (`pickRoomingInCrib`, K-01, regresi pemesanan dua tahap, kunci draf, teks kartu) | 02–08 |
| `tests/unit/floor-plan-admission-selection.test.mjs` | Asersi `FE-RWI-248` yang mencocokkan teks `isBookingReady` diganti asersi mekanisme baru dengan maksud yang sama (commit pemilik `229f071bb`) | 02 |

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi dipertahankan: view → hook → service; normalisasi baru berada di `src/utils` sebagai
  fungsi murni; tidak ada panggilan Axios baru dan tidak ada endpoint baru.
- Tidak ada arsitektur state baru: penanda sub-tampilan dan pemesanan tertunda tinggal di pengendali
  bed yang sudah dipasang di level view.
- `use-bed-operation.js` (dipakai Bed Management) sengaja tidak diubah.

**Gerbang base component**

```
UI GATE: 7 elemen — REUSE 7, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kartu konfirmasi ibu/boks | `BaseDetailCard` | `base-features/base-detail-card`, sudah dipakai langkah ini | REUSE | Baris "Boks Bayi" dan "Status Kesiapan Boks" |
| Status kesiapan boks | `StatusBadge` | `base-features/status-badge` (`success`/`warning`/`danger`/`pending`) | REUSE | Satu badge per keadaan |
| Pilihan boks bila > 1 (K-01) | `BaseSelectField` | `base-form-control.jsx`, `onChange(name, value, option)` | REUSE | Wajib, placeholder "Pilih boks bayi" |
| Pesan keadaan (a)/(b)/(c), memuat, gagal | `InformationAlert` | `base-features/information-alert` | REUSE | Kalimat sesuai skema 3.2 |
| Tombol Periksa Ulang / Ubah ke Perinatologi / Pesan | `BaseButton` | `base-features/base-button` | REUSE | Baris tombol memakai kelas `bedBookingActions` dan `actionRightGroup` yang sudah ada |
| Pemulihan operasi tidak pasti | `BedOperationRecovery` | `features/health-services/inpatient-management/bed-operation-recovery.jsx` | REUSE | Sama dengan langkah Booking |
| Pilih Bed / Booking Neonatal | `InpatientAdmissionBedSelectionStep`, `InpatientAdmissionBedBookingStep` | `inpatient-admission-bed-step.jsx` | REUSE | Tidak diubah |

Modul referensi visual: langkah Booking Bed admisi (`inpatient-admission-bed-step.jsx`) dan kartu
konfirmasi rawat gabung FIX-EPS-037.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Rawat gabung: "Mencari boks bayi di kamar ibu…" dan badge "Memeriksa boks bayi"; tombol pesan mati. Neonatal: tombol "Menyimpan admisi..." tanpa layar kosong |
| Kosong | Kamar tanpa boks → keadaan (a); kamar ibu belum terbaca → keadaan (c) |
| Gagal | Pesan server pada kotak merah + **Periksa Ulang**; galat pemesanan yang ternyata sudah tercatat diadopsi; hasil tidak pasti memakai panel pemulihan |
| Tanpa hak akses | Tidak berubah: pesan `403` dari server ditampilkan apa adanya pada kotak galat |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru; pemakaiannya berubah sebagai berikut.

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/bed-board?roomId=<kamar ibu>&isForNewborn=true` | Semua boks di kamar ibu beserta status, pemegang pemesanan, dan `operationalVersion` (lintas lantai) | `InpatientBedOccupancy : Read` |
| `GET` | `/available-beds?roomId=<kamar ibu>&isForNewborn=true&…` | Boks yang layak menurut server; sebelum episode memakai konteks bayi, sesudahnya `episodeId` | `InpatientBedOccupancy : Read` |
| `POST` | `/reservations` | Memesan boks/bed (header `Idempotency-Key`) | `InpatientBedOccupancy : Create` |
| `GET` | `/floor-plan` | Tetap untuk Perinatologi/NICU; **tidak lagi dipanggil** pada rawat gabung | `InpatientBedOccupancy : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat episode `Draft` bayi (tanpa perubahan) | `InpatientEpisode : Create` |
| `GET` | `/{id}` | Membaca ulang episode saat URL memuat `episodeId` (tanpa perubahan) | `InpatientEpisode : Read` |
| `GET` | `/newborn-mother-candidates?search=<No. RM ibu>` | Pencarian ulang kasus ibu yang tidak ada di halaman pertama | `InpatientEpisode : Read` |

Kode status: `409` pada `POST /reservations` kini lebih dulu dicek terhadap papan — bila tempat itu
dipegang episode ini sendiri, pemesanan diadopsi; bila tidak, tetap tampil "Bed baru saja dipilih
petugas lain. Denah dimuat ulang."

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 10 berkas yang diubah | 0 error, 6 warning; keenamnya ada juga di versi HEAD basis (aturan sama, baris bergeser) | `PASS` / `EXISTING WARNING` | Lint ulang `git show 2b16ec441:<berkas>` lewat `--stdin` |
| `npm run lint:errors` (seluruh repo, pada `9de9f17cc`) | Selesai tanpa error | `PASS` | Exit code 0 |
| `node --test tests/unit/inpatient-newborn-admission.test.mjs` | 22/22 lulus (sebelumnya 18/18) | `PASS` | Keluaran runner |
| 15 berkas tes yang menyentuh berkas admisi | 105/108 lulus; 3 gagal (`FE-CRM-007`, `FE-RWI-058`, `FE-RWI-227`) | `EXISTING / ENVIRONMENT ISSUE` | Ketiganya juga gagal pada salinan baseline (working tree dengan 11 berkas ini dikembalikan ke HEAD basis) |
| `npm run test:unit` (seluruh suite, pada `9de9f17cc`) | 3616/3659 lulus, 43 gagal | `EXISTING / ENVIRONMENT ISSUE` | Dibanding baseline: kegagalan `FE-RWI-248` akibat perubahan ini sudah diperbaiki; dua kegagalan `FE-IGD-017 K1` berasal dari commit upstream `2dd0f030b` (tampilan perawat IGD) yang ikut ter-merge selama pengerjaan, bukan dari berkas ini |
| `npm run build` | "Compiled successfully in 67s", 517 halaman, `prepare-standalone` siap | `PASS` | Exit code 0; dijalankan berdampingan dengan `next dev` milik user tanpa menghentikannya |
| Render SSR `NewbornPlacementStep` (komponen asli, data samaran, 11 skenario) | 11/11 lulus | `PASS` | Lihat tabel di bawah |
| Grep anti-regresi checklist UI pada baris yang ditambahkan | 0 warna literal, 0 tipografi, 0 `<button>`, 0 `<table>`, 0 `!important`, 0 inline style baru | `PASS` | Satu inline style lama berwarna hex (`#0284c7`) ikut terhapus |

Render SSR memakai loader SWC bawaan Next dan `react-dom/server` di folder scratch sesi, bukan di
repository:

| Skenario | Hasil | Tombol "Pesan Boks Bayi & Lanjut" |
| --- | --- | --- |
| N1 Neonatal, bed dipilih, belum masuk pemesanan | Pilih Bed + "Lanjut ke Pemesanan" | — |
| N2 Neonatal, `bookingViewOpen` + bed | Kartu Booking "Pesan Tempat Tidur" | — |
| N3 Neonatal, `bookingViewOpen` tanpa bed (ISSUE-033) | Tetap Pilih Bed | — |
| R1 Rawat gabung, satu boks + bed penunggu kosong | `301-BOKS` "Tersedia & Siap Dipesan"; bed `301-B` tidak tampil sebagai boks | aktif |
| R2 Dua boks kosong (K-01) | Daftar "Pilih boks bayi" | mati sampai dipilih |
| R3 Boks terisi | "Boks Bayi di Kamar Ibu Belum Siap" + status "Terisi" | mati |
| R4 Kamar tanpa boks | "Kamar Ibu Tidak Memiliki Boks Bayi", "Kelas 1 · 204" | mati |
| R5 Kandidat ibu belum terbaca | "Data Kamar Ibu Belum Terbaca", "Belum terbaca", tanpa "VIP" | mati |
| R6 Kamar ibu tidak aktif | Banner Closed + badge "Kamar Ibu Tidak Aktif", tanpa pesan (c) | mati |
| R7 Sudah dipesan | "Dipesan Sementara (14:52)", "Lanjut ke Persetujuan Wali" | tidak tampil |
| R8 Papan memuat | "Mencari boks bayi di kamar ibu…" | mati |

Uji manual: `NOT FEASIBLE` — agent tidak dapat login ke aplikasi dan tidak memiliki data kasus
persalinan, kamar VIP berboks, maupun backend yang berjalan. Render SSR tidak menjalankan efek
React, sehingga perilaku yang bergantung efek dan jaringan — tidak berkedipnya langkah setelah
episode dibuat (FIX-01), pemesanan tertunda (FIX-04), `sessionStorage` (FIX-05), dan pencarian
ulang kandidat (FIX-06) — hanya terbukti di source, belum di layar.

**Tidak dijalankan:** `npm run test:e2e` dan `test:uat` (tidak diminta, environment tidak tersedia);
skenario S1–S10 plan repair (butuh login dan data).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (plan repair bagian 4) | Status | Bukti |
| --- | --- | --- |
| FIX-01 AC1 — satu klik "Lanjut ke Pemesanan" membuka Booking; satu `POST /episodes` | Terpenuhi di source; menunggu UAT | Gerbang `isOwnEpisode`; `prepareBooking` mengembalikan episode yang ada |
| FIX-01 AC2 — langkah tidak kosong, tanpa banner pelanjutan | Terpenuhi di source; menunggu UAT | `inpatient-admission-view.jsx` gerbang dan dua banner `!isOwnEpisode`; tes regex view |
| FIX-01 AC3 — URL memuat `episodeId` walau pemesanan gagal | Terpenuhi di source; menunggu UAT | Efek URL tidak lagi melewati `bed-selection` untuk bayi |
| FIX-01 AC4 — muat ulang tidak membuat episode kedua | Terpenuhi di source; menunggu UAT | Ikutan AC3; jalur pelanjutan existing |
| FIX-01 AC5 — pelanjutan dari daftar kerja tetap berbanner | Terpenuhi di source; menunggu UAT | `createdEpisodeId` hanya diisi `POST /episodes`, bukan adopsi episode |
| FIX-01 AC6 — regresi jalur lain | Belum terbukti | Lint/build/unit PASS; uji layar belum |
| FIX-02 AC1–AC4 — `resolveNewbornPlacementView` | Terpenuhi | Tes unit 22/22; render N1–N3 |
| FIX-02 AC5 — Kembali dari Booking ke Pilih Bed langkah 4 | Terpenuhi di source; menunggu UAT | `handleBookingBack` → `closeBookingView` |
| FIX-03 AC1 — `301-BOKS` terbaca walau lantai terakhir "2" | Terpenuhi | Sumber papan tidak bergantung lantai; tes unit + render R1 |
| FIX-03 AC2–AC5 — keadaan (a)/(b), bed dewasa tidak tampil, tes `pickRoomingInCrib` | Terpenuhi | Tes unit + render R1, R3, R4 |
| FIX-03 AC6 — tanpa permintaan `floor-plan` pada rawat gabung | Terpenuhi di source; menunggu UAT | `useFloorPlan` `enabled` `!isRoomingInNewborn` |
| FIX-04 AC1–AC4 — satu klik, tanpa "Sesi berubah", tahan klik ganda dan jawaban hilang | Terpenuhi di source; menunggu UAT | Handler tanpa `bed.reserve(` (tes regresi); efek pemesanan tertunda; adopsi di `catch` |
| FIX-05 AC1–AC3 — draf sesi bayi | Terpenuhi di source; menunggu UAT | `storageKey` kosong untuk `newborn:active`; efek migrasi kunci; tes regex |
| FIX-06 AC1 — kasus ibu lama tetap terbaca setelah muat ulang | Terpenuhi di source; menunggu UAT | Efek pencarian ulang No. RM + `knownCandidates` |
| FIX-07 AC1 — "Belum terbaca", bukan "VIP" | Terpenuhi | Render R5 |
| FIX-08 — teks kartu tanpa kamar/lantai tetap (K-03) | Terpenuhi | Tes unit kartu Rawat Gabung |

**Definition of Done yang belum terpenuhi:** uji manual alur hidup S1–S10; revisi dokumen hulu
(`05-skema-tampilan.md` amandemen langkah 4 dan teks kartu di `plan-repair-031`) belum dikerjakan
karena di luar wewenang tulis task ini.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 6 `EXISTING WARNING` lint pada hook yang disentuh (exhaustive-deps dan set-state-in-effect lama), tidak bertambah |
| Masalah yang diketahui | (1) Mengubah rencana perawatan di langkah 4 sesudah episode dibuat ("Ubah ke Perawatan Perinatologi") hanya mengubah draf di layar; `NewbornCareMode` episode di server tetap — perilaku lama, di luar scope repair ini. (2) Unit layanan episode tidak ikut diselaraskan server saat bed diganti (ISS-EPS-038-T4, kelas sudah diselaraskan) — dicatat, tidak diubah. (3) `BaseSelectField` mode transfer pada `inpatient-admission-bed-step.jsx` memanggil `onChange(bedId)` dengan argumen nama field — cabang itu tidak terpakai pada admisi berdenah; tidak diubah |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` di repository. Salinan baseline dan skrip render ada di folder scratch sesi, di luar repository |
| Interupsi | Pemilik meng-commit perubahan source (`9de9f17cc`) dan merge upstream di tengah validasi; validasi diulang pada HEAD baru dan kegagalan upstream dipisahkan |
| Status Git | Frontend `HamzahV2` @ `229f071bb`: bersih (seluruh perubahan task ini sudah di-commit pemilik pada `9de9f17cc` dan `229f071bb`; agent tidak melakukan commit). Backend `MHamzah` @ `b6c638ee`: berkas issue/plan repair 038 dan laporan ini belum di-track; `docs/pre-deploy-reports/` bukan dari task ini |
| Langkah berikutnya | Pemilik menjalankan skenario S1–S10 PLAN-REPAIR-EPS-038 bagian 8 di localhost; bila lulus, tanda 🟡 pada register diubah menjadi ✅ |
