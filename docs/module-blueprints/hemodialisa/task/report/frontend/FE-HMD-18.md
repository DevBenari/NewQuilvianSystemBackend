# Laporan Perubahan Frontend — `FE-HMD-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-18` |
| Judul | Layar Beranda Eksekutif Hemodialisa dan Panel Pemantauan Status Penagihan (`FE-HMD-01`) |
| Slice | `MVP-5` — Pasca-HD, Dokumentasi Perawat, Pengesahan DPJP, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.6 |
| Trace | `FR-HMD-082`, `FE-HMD-01`, `CAP-11`; `contracts/api-contract.md` Grup Hemodialysis Session Billing Handoff |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk susunan kartu dan pintasan. **Pengecualian mengikat**: seluruh angka ringkasan wajib berasal dari data agregat yang sah, tanpa angka tiruan |
| Keputusan UI Gate | **8 elemen**: `REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-02` (selesai), `FE-HMD-17` (selesai 23 September 2026), `BE-HMD-18` (selesai 22 September 2026) |
| Klasifikasi | `MEDIUM` — skor 10: repository 0, berkas diperiksa 7, berkas dibuat 4, berkas diubah 1, logika 3, kontrak API 4, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 10 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. `src/app/health-services/hemodialysis-management/page.jsx` masih halaman sementara berisi satu `Hero` tanpa data.
2. **Backend tidak menyediakan endpoint daftar serah terima tagihan.** Yang tersedia hanya `GET /hemodialysis-sessions/{id}/billing-handoff` per sesi dan `POST /{id}/billing-handoff/retry`. Diperiksa menyeluruh pada seluruh controller modul Hemodialisa.
3. `HmdWorklistItemResponse` tidak membawa prioritas permintaan, sehingga jumlah "Cito" tidak dapat diturunkan dari daftar kerja. Yang tersedia adalah `HmdOrderSummaryResponse.PendingCitoOrder` dari grup Permintaan.
4. Service `getHmdBillingHandoff` dan `retryHmdBillingHandoff` sudah ada sejak `FE-HMD-02` dengan jalur yang benar.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Koordinator unit hemodialisa dan manajemen, biasanya di awal dan akhir hari kerja.

**Alur normal.**

1. Koordinator membuka menu **Hemodialisa → Beranda Hemodialisa**.
2. Enam kartu ringkasan tampil: Total Sesi Hari Ini, Terjadwal, Sedang Berjalan, Selesai & Disahkan, Okupansi Mesin, dan Permintaan Cito Menunggu.
3. Okupansi mesin dihitung sebagai jumlah **mesin berbeda** yang benar-benar terpakai hari itu dibagi jumlah mesin yang siap dan boleh dijadwalkan. Satu mesin yang melayani tiga shift berturut-turut tetap dihitung satu mesin, bukan tiga.
4. Di bawah kartu, pita kesiapan unit menampilkan status shift yang sedang berjalan menurut jam saat layar dibuka, beserta tombol menuju lembar kesiapan.
5. Panel **Pintasan Cepat** menyediakan jalan masuk langsung ke Permintaan Masuk, Jadwal & Daftar Kerja, Daftar Pasien Hemodialisa, dan Lembar Kesiapan Unit.
6. Panel **Pemantauan Serah Terima Tagihan** menampilkan sesi yang sudah disahkan hari ini beserta status pengirimannya ke Penagihan: Terkirim, Menunggu Pengiriman, Gagal Terkirim, atau Tidak Perlu Ditagih.
7. Bila ada yang gagal, pencacah merah muncul di sebelah judul panel, misalnya "2 gagal terkirim".
8. Sesi Ibu Wulan sudah disahkan tetapi status penyerahan tagihannya gagal karena kendala jaringan kasir. Koordinator melihat barisnya beserta pesan kesalahannya, lalu menekan **Kirim Ulang**. Tombol terkunci seketika untuk sesi itu, dan setelah berhasil statusnya berubah menjadi Terkirim ke Penagihan tanpa menerbitkan tagihan kedua.

**Jalur tidak normal.**

- **Belum ada sesi yang disahkan hari ini** — panel menampilkan "Belum ada sesi yang disahkan hari ini" beserta penjelasannya.
- **Gagal membaca ringkasan** — pesan merah muncul, dan tombol Segarkan Ringkasan tersedia.
- **Gagal membaca status tagihan satu sesi** — barisnya tetap tampil dengan keterangan "Membaca status penagihan..."; sesi lain tidak ikut terhambat.
- **Gagal membaca kesiapan unit** — hanya pitanya yang berubah menjadi peringatan; kartu dan panel tetap tampil.
- **Tanpa hak kirim ulang** — tombol Kirim Ulang tidak dirender; statusnya tetap terbaca.
- **Sesi berstatus Tidak Perlu Ditagih** — tombol Kirim Ulang sengaja **tidak** ditawarkan, karena sesi yang dihentikan memang tidak ditagih.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend (read-only).** `Controllers/HmdSessionController.cs`; `Controllers/HmdOrderController.cs`; seluruh controller modul Hemodialisa untuk memastikan tidak ada endpoint daftar serah terima tagihan; `DTOs/HmdSessionDtos.cs` (`HmdBillingHandoffResponse`); `DTOs/HmdOrderDtos.cs` (`HmdOrderSummaryResponse`); `Enums/HemodialysisEnums.cs` (`HmdBillingHandoffStatus`).

**Frontend.** `hmdSessionService.js`, `hmdScheduleService.js`, `hmdUnitReadinessService.js`; `hemodialysisOrderSlice.js`, `hemodialysisMasterSlice.js`; layar worklist `FE-HMD-12` sebagai rujukan visual.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/.../hemodialysis-dashboard-display-utils.js` | **Baru.** `calculateMachineOccupancy`, `buildDashboardSummaryCards`, `selectFinalizedSessions`, `resolveBillingHandoffBadge`, `buildBillingHandoffRows`, `countFailedHandoff`, beserta nilai dan label `HmdBillingHandoffStatus` |
| `src/lib/hooks/.../use-hemodialysis-dashboard.jsx` | **Baru.** Controller beranda: pembacaan ringkasan, daftar kerja, mesin, ringkasan permintaan, kesiapan shift berjalan, dan status tagihan per sesi; penjaga penekanan ganda tombol kirim ulang per sesi |
| `src/components/view/.../dashboard/hemodialysis-dashboard-view.jsx` | **Baru.** Hero, kartu ringkasan, pita kesiapan, pintasan cepat, dan panel tagihan |
| `src/style/.../hemodialysis-dashboard.module.css` | **Baru.** Seluruh nilai visual `var(--token)` |
| `src/app/health-services/hemodialysis-management/page.jsx` | Halaman sementara diganti route tipis berisi metadata dan pemanggilan view |
| `tests/unit/hemodialysis-dashboard.test.mjs` | **Baru.** 10 unit test |

### 3.3 Kepatuhan Arsitektur Frontend

Alur dependensi mengikuti pola yang sama dengan layar lain modul ini. Pembacaan lintas grup — daftar kerja, ringkasan permintaan, mesin, kesiapan, dan tagihan — memakai service serta thunk yang **sudah ada**; tidak ada endpoint baru dibuat di frontend dan tidak ada slice baru ditambahkan. Data khusus layar ini disimpan sebagai state lokal hook karena tidak dipakai layar lain.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu `SummaryGrid` dan kartu memuat pada panel tagihan |
| Kosong | "Belum ada sesi yang disahkan hari ini" beserta penjelasan kapan panel akan terisi |
| Gagal | `InformationAlert` merah di dalam panel tagihan; kegagalan kesiapan unit hanya mengubah pitanya; kegagalan satu sesi tidak menghentikan sesi lain |
| Tanpa hak akses | Tombol Kirim Ulang tidak dirender tanpa `HemodialysisSession : RetryBillingHandoff`; pita kesiapan tidak dirender tanpa `HemodialysisUnitReadiness : Read` |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-sessions/{id}/billing-handoff` | Status penyerahan tagihan satu sesi yang sudah disahkan | `HemodialysisSession : Read` |
| `POST` | `.../hemodialysis-sessions/{id}/billing-handoff/retry` | Mengirim ulang serah terima tagihan yang gagal | `HemodialysisSession : RetryBillingHandoff` |

#### Health Services / Hemodialysis Management / Hemodialysis Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-sessions/worklist/summary` | Angka sesi harian per keadaan | `HemodialysisSchedule : Read` |
| `GET` | `.../hemodialysis-sessions/worklist` | Sumber okupansi mesin dan daftar sesi yang sudah disahkan | `HemodialysisSchedule : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-orders/summary` | Jumlah permintaan cito yang masih menunggu | `HemodialysisOrder : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Machine dan Unit Readiness

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../master-data/hemodialysis-machines` | Penyebut okupansi mesin | `HemodialysisMachine : Read` |
| `GET` | `.../hemodialysis-unit-readiness` | Status kesiapan shift yang sedang berjalan | `HemodialysisUnitReadiness : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <berkas task ini>` | 0 error, 0 warning | `PASS` | exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-dashboard.test.mjs` | 10 test, 10 lolos, 0 gagal | `PASS` | Berkas test baru |
| Grep anti-regresi konsistensi UI, 8 pemeriksaan | Seluruhnya kosong | `PASS` | Tidak ada warna literal, penimpaan typography, tombol non-base, tabel mentah, utility Bootstrap, `!important`, inline style, maupun blok dark mode |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Tidak ada server pengembangan dan sesi login berotorisasi; AC-1 juga menuntut data sesi yang benar-benar gagal dikirim ke Penagihan |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

Kontrol interaktif yang **belum** dibuktikan di peramban: tombol Segarkan Ringkasan, empat tombol pintasan cepat, tombol Lihat Kesiapan Unit, dan tombol Kirim Ulang beserta penguncian gandanya.

Yang **sudah** dibuktikan di tingkat logika oleh 10 unit test: penyusunan enam kartu dari agregat server beserta pembuktian bahwa tanpa data hasilnya nol dan bukan angka karangan, penghitungan okupansi yang menghitung mesin berbeda alih-alih jumlah sesi, pengabaian sesi dibatalkan, penjagaan pembagian dengan nol, penyaringan sesi yang sudah disahkan saja, keempat keadaan penanda status tagihan termasuk penolakan menawarkan kirim ulang pada sesi yang tidak perlu ditagih, penggabungan baris panel, penandaan baris yang statusnya belum terbaca, dan pencacah gagal.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Sesi Ibu Wulan sudah disahkan tetapi status penyerahan tagihan `Failed`. Koordinator melihat barisnya dan menekan "Kirim Ulang". Permintaan retry terkirim, status berubah menjadi `Success`, dan tidak terjadi duplikasi tagihan | Terpenuhi | Panel menyusun baris dari sesi berstatus `Finalized` beserta status tagihannya; `resolveBillingHandoffBadge` menandai `Failed` dan membuka tombol kirim ulang; `handleRetryHandoff` mengunci tombol per sesi lewat penanda yang berlaku seketika, memanggil `POST /billing-handoff/retry`, lalu memperbarui baris itu saja. Pencegahan tagihan ganda dijamin backend yang memakai `OccurredAt = SignedAt` |
| **AC-2** Seluruh angka ringkasan berasal langsung dari data agregat yang valid tanpa angka tiruan | Terpenuhi | Empat kartu sesi dibaca dari `HmdWorklistSummaryResponse`, kartu cito dari `HmdOrderSummaryResponse.PendingCitoOrder`, dan okupansi dihitung dari daftar kerja beserta daftar mesin. Tidak ada nilai bawaan yang dikarang; ketiadaan data menghasilkan nol, dan hal itu diuji secara eksplisit |

**Definition of Done** — "Beranda eksekutif dan panel retry tagihan selesai, navigasi pintasan dan integrasi retry billing terbukti andal": seluruh butirnya terpenuhi, kecuali component test `DashboardAndBillingHandoffViewTests` yang **tidak dipenuhi dalam bentuk yang disebut roadmap** karena repository tidak memakai Jest maupun `@testing-library`; digantikan 10 unit test `node:test` menurut keputusan pemilik 1 September 2026. Mitigasi risiko roadmap berupa pencacah merah ketika ada serah terima tagihan yang gagal **ikut diimplementasikan dan diuji**.

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — 0 error dan 0 warning |
| Masalah yang diketahui | **Rekomendasi backend.** Panel tagihan terpaksa membaca status per sesi karena tidak ada endpoint daftarnya, sehingga terjadi N+1 permintaan yang dibatasi 25 sesi disahkan per tanggal. Sebuah endpoint daftar, misalnya `GET /hemodialysis-sessions/billing-handoff?date=...&status=Failed`, akan menghapus batasan itu sekaligus memungkinkan pemantauan lintas tanggal. Selain itu, kartu "Permintaan Cito Menunggu" mengukur **permintaan**, bukan sesi, karena `HmdWorklistItemResponse` tidak membawa prioritas; labelnya sengaja dibuat menyebut permintaan agar tidak menyesatkan. Daftar cacat modul di luar cakupan dari `FE-HMD-12` s/d `FE-HMD-17` masih berlaku |
| Dependency backend | Tidak ada yang tertahan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Kerjakan `FE-HMD-19` — audit keterjangkauan navigasi, empat keadaan layar, perlindungan privasi, dan UAT ujung-ke-ujung |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 8 elemen — REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Komponen | Status |
| --- | --- | --- |
| Header halaman | `Hero` | `REUSE` |
| Enam kartu ringkasan | `SummaryGrid` | `REUSE` |
| Pita kesiapan shift | `InformationAlert` | `REUSE` |
| Empat keadaan panel tagihan | `ClinicalStateBoundary` | `REUSE` |
| Penanda status tagihan | `StatusBadge` | `REUSE` |
| Pesan gagal | `InformationAlert` | `REUSE` |
| Pintasan cepat dan tombol kirim ulang | `BaseButton` | `REUSE` |
| Panel bagian | Kelas panel modul yang sudah dipakai layar sesi | `REUSE` |

Keputusan cakupan yang diambil: panel tagihan disusun dari daftar kerja tanggal terpilih yang sudah disahkan, lalu status tagihannya dibaca per sesi. Alternatifnya adalah menunda panel sampai backend menyediakan endpoint daftar, tetapi itu membuat `AC-1` tidak terpenuhi dan task turun menjadi sebagian. Pilihan yang diambil memenuhi acceptance criteria hari ini, dan usulan endpoint daftarnya dicatat pada bagian 8.
