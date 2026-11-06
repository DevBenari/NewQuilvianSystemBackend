# Laporan Perubahan Frontend — `FE-OPT-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-040` |
| Judul | Kirim kunci simpan ganda |
| Slice | Amendment kesiapan — layar `FE-OPT-N05`, `N07`, `N12` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r3) — Rincian `FE-OPT-040` |
| Trace | `FR-OPT-033`, `044`, `072`; `NFR-003`; `OPT-DEC-077`; `OPT-AC-035`; `UAT-32`, `33`; risiko `R1` |
| Contract version | `OPT-API-v1.2`, `03-frontend-architecture.md` r2 bagian 4, `05-screen-brief.md` r3 bagian 6 — `approved` sisi Optik, Yoga Aji Pratama 2026-11-06, terkunci hash di manifest r2 |
| Wewenang UI | Tidak ada perubahan tampilan, sehingga wewenang UI tidak dipakai. Daur kunci mengikuti brief r3 bagian 6 |
| Approval task | Yoga Aji Pratama, 2026-11-06 ("lanjutkan langkah berikutnya", menjawab tawaran mengerjakan `FE-OPT-040`) |
| Dependency | `FE-OPT-012` ✅, `020` ✅, `030` ✅; pasangannya `BE-OPT-060` ✅ |
| Klasifikasi | `MEDIUM` — 7 berkas produksi tanpa perubahan layar, 5 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; pada repository backend hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `811f22159` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `06e40fb7` (branch `yoga`) + `BE-OPT-060` di working tree |
| Tanggal | 2026-11-06 |
| Status | Selesai dan tervalidasi (uji unit, lint, build, E2E API tiruan). Belum diuji dengan backend asli, karena data prasyarat `B1`, `B2` masih kosong. Migration diterapkan ke devYoga 2026-11-06. Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Layar hanya mengirim `Idempotency-Key` saat mendaftarkan kunjungan. Simpan resep, pesanan, dan keluhan tidak membawanya.

Sejak `BE-OPT-060`, backend menolak kiriman seperti itu dengan `400`. Tanpa task ini, ketiga form tidak bisa menyimpan sama sekali. Komentar pada dua service dan dua slice juga masih menyatakan endpoint itu tidak membaca kunci.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan:** petugas yang menekan Simpan lagi sesudah jaringan putus melihat data pertama, bukan data ganda.

**Pelaku:** RO/Sp.M (resep), petugas penjualan (pesanan), petugas atau RO/Sp.M (keluhan).

**Langkah utama:**

1. Petugas menekan Simpan. Layar membuat kunci baru untuk niat ini.
2. Bila server menjawab berhasil, layar membuka detail seperti biasa, dan kuncinya dilepas.
3. Bila jaringan putus atau server menjawab `5xx`, hasilnya tidak diketahui. Pesan galat tampil, isian tetap ada, dan kuncinya disimpan.
4. Petugas menekan Simpan lagi dengan isi yang sama. Layar mengirim kunci yang sama.
5. Bila data sudah tersimpan, server mengembalikan data pertama, dan layar membuka detail seperti simpan biasa.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Isi diubah sesudah jaringan putus | Kunci baru, karena isinya niat yang berbeda |
| Server menolak `400`/`403`/`409` | Kunci dilepas, karena tidak ada yang tersimpan; kiriman berikutnya memakai kunci baru |
| Tombol ditekan dua kali selama kiriman berjalan | Klik kedua tidak mengirim apa pun |
| Ubah draf pesanan (`PUT`) dan revisi resep | Tidak membawa kunci, dan tidak menghapus niat buat data yang belum diketahui hasilnya |

**Contoh:** jaringan putus sesudah keluhan KLM-0001 tersimpan. Dedi menekan Simpan lagi dengan isi yang sama. Layar menampilkan KLM-0001, dan tidak ada keluhan kedua.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend, `optical-visit-slice.jsx` (pola acuan), tiga slice dan dua service terkait, `optical-request-rules.js`, tiga hook form, spec E2E Optik, uji unit slice Optik, serta `05-screen-brief.md` r3 dan `03-frontend-architecture.md` r2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/optical-management/optical-request-rules.js` | Tambah `fingerprintOpticalPayload`, `resolveOpticalIntent`, `opticalIntentAfterFailure`, `withOpticalIdempotencyKey` |
| `src/lib/services/health-services/optical-management/optical-clinical.service.js` | `createOpticalPrescription`, `copyExternalOpticalPrescription` menerima kunci; `registerOpticalVisit` memakai helper yang sama; komentar disegarkan |
| `src/lib/services/health-services/optical-management/optical-order.service.js` | `createOpticalOrder`, `createOpticalWarrantyClaim` menerima kunci; komentar disegarkan |
| `src/lib/state/slice/health-services/optical-management/optical-prescription-slice.jsx` | State `createIntent`; thunk buat dan salin memakai daur kunci |
| `src/lib/state/slice/health-services/optical-management/optical-order-slice.jsx` | State `createIntent`; thunk buat memakai daur kunci. Aksi ber-versi tidak berubah |
| `src/lib/state/slice/health-services/optical-management/optical-warranty-claim-slice.jsx` | State `createIntent`; thunk catat memakai daur kunci. Aksi ber-versi tidak berubah |
| `src/lib/state/slice/health-services/optical-management/optical-visit-slice.jsx` | Sidik isi memakai fungsi bersama; nama ekspor dan perilaku tetap |
| `tests/unit/optical-clinical-slices.test.mjs`, `optical-order-slices.test.mjs` | 10 uji baru |
| `tests/e2e/optical-prescription-screen.spec.mjs`, `optical-order-screen.spec.mjs`, `optical-warranty-claim-screen.spec.mjs` | 4 skenario jaringan putus |

Hook form dan view tidak berubah. Thunk tetap memulangkan data yang sama, sehingga `.unwrap()` dan perpindahan ke detail bekerja seperti semula.

### 3.3 Kepatuhan arsitektur frontend

| Aspek | Keadaan |
| --- | --- |
| Pola | Mengikuti `optical-visit-slice`: niat disimpan di Redux, dan sidik isi membedakan niat |
| Abstraksi baru | Hanya empat fungsi kecil di berkas aturan Optik yang sudah ada, agar tiga slice tidak menyalin logika yang sama |
| Komponen | Tidak ada komponen baru atau yang diubah |

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku | Bukti |
| --- | --- | --- |
| Memuat | Tombol Simpan nonaktif selama kiriman berjalan, seperti semula | E2E: `toBeEnabled` sesudah gagal |
| Jaringan putus | Pesan galat tampil, isian tetap, Simpan bisa ditekan lagi | E2E `FE-OPT-040` |
| `503` | Hasil tidak diketahui; kunci disimpan | Uji unit |
| `400` | Pesan server tampil; kunci dilepas | Uji unit |
| Kiriman ulang berhasil (`isReplay = true`) | Membuka detail seperti simpan biasa | E2E `UAT-33` |
| Privasi | Kunci berupa UUID acak; tidak ada penulisan baru ke `console` | Diff |

---

## 5. Endpoint yang dikonsumsi

Base URL: `api/v1/health-services/optical-management`. Layar kini mengirim header `Idempotency-Key` (UUID) pada kelima pembuat data.

#### Health Services / Optical Management / Opt Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/visits` | Daftarkan kunjungan (sudah membawa kunci sebelum task ini) | `OptVisit : Create` |

#### Health Services / Optical Management / Opt Prescription

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/prescriptions` | Simpan resep dari pemeriksaan | `OptPrescription : Create` |
| `POST` | `/prescriptions/external` | Salin resep luar | `OptPrescription : Create` |

#### Health Services / Optical Management / Opt Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/orders` | Simpan draf pesanan baru | `OptOrder : Create` |

#### Health Services / Optical Management / Opt Warranty Claim

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/warranty-claims` | Catat keluhan | `OptWarrantyClaim : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `node`/`dotnet` berjalan | `PASS` | `tasklist` |
| `npm run lint:errors` | Tanpa error | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; 2 menit 11 detik | `PASS` | Keluaran build |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.480 uji: 4.404 lulus, 72 gagal, sisanya tidak dijalankan. Jumlah gagal sama dengan baseline 2026-11-05, dan tidak satu pun milik Optik | `PASS` untuk task ini; 72 `UNRELATED EXISTING ISSUE` | Keluaran di scratchpad sesi |
| Uji unit slice Optik | 37 lulus, 0 gagal (10 baru) | `PASS` | Keluaran perintah |
| E2E skenario `FE-OPT-040` | 4 lulus | `PASS` | Keluaran Playwright |
| E2E seluruh layar Optik atas build produksi | 150 lulus, 0 gagal (146 lama + 4 baru); 2,6 menit | `PASS` | Keluaran Playwright |

**Cara E2E menguji jaringan putus.** Peramban sungguhan menjalankan build produksi. Kiriman `POST` pertama diputus lewat `route.abort("connectionfailed")`, lalu kunci setiap kiriman dibaca dari header permintaan. Hasilnya:

| Form | Kiriman 1 dan 2 (isi sama) | Kiriman sesudah isi diubah | Data yang sampai ke server tiruan |
| --- | --- | --- | --- |
| Resep internal | Kunci sama | Kunci baru | Satu |
| Resep luar | Kunci sama | — | Satu |
| Pesanan | Kunci sama | — | Satu |
| Keluhan (`UAT-33`, jawaban `isReplay = true`) | Kunci sama | Kunci baru | Satu |

**Uji manual: `NOT FEASIBLE`.** Data prasyarat `B1`, `B2` masih kosong: belum ada akun RO/Sp.M berizin dan belum ada barang Optik, sehingga form tidak bisa disimpan dengan data asli. Saat task ini ditulis, migrationnya juga belum diterapkan; penerapan ke devYoga menyusul 2026-11-06. Perilaku kontrol diverifikasi lewat E2E peramban di atas.

**Tidak dijalankan:** kiriman layar ke backend asli, dan penerapan migration.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Hasil tidak diketahui + isi sama: kunci yang sama dipakai lagi | Terpenuhi | Uji unit (jaringan putus, `503`); E2E keempat form |
| (2) Isi diubah: kunci baru | Terpenuhi | Uji unit; E2E resep internal dan keluhan |
| (3) Server menjawab berhasil atau menolak `400`/`403`/`409`: kunci dibuang | Terpenuhi untuk berhasil dan `400` lewat uji unit. `403` dan `409` memakai aturan yang sama (`isOpticalOutcomeUnknown`, status di bawah 500), dan uji aturannya sudah ada | Uji unit |
| (4) Jawaban `IsReplay = true` membuka data pertama seperti simpan biasa | Terpenuhi | E2E `UAT-33` |
| (5) `UAT-32`, `33` lulus di E2E API tiruan | Terpenuhi | E2E `FE-OPT-040` |
| (6) Mode Ubah pesanan (`PUT`) tidak mengirim kunci baru | Terpenuhi | Uji unit `FE-OPT-040 ubah draf pesanan` |
| DoD: laporan dan traceability diperbarui | Terpenuhi | Laporan ini; roadmap r3; traceability r3 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Urutan rilis:** frontend ini terbit lebih dulu atau bersama `BE-OPT-060`. Backend lama mengabaikan header itu, sehingga frontend aman terbit lebih dulu. `BE-OPT-060` butuh migrationnya diterapkan di lingkungan tujuan lebih dulu (devYoga sudah) |
| Masalah yang diketahui | 72 uji unit gagal sudah ada sebelum task ini, di luar Optik, dan tidak disentuh |
| Risiko tersisa | Rantai layar ke backend asli belum pernah berjalan untuk kunci ini. Niat tersimpan di Redux, sehingga hilang bila halaman dimuat ulang penuh; kiriman sesudah itu memakai kunci baru dan bisa membuat data ganda bila kiriman sebelumnya ternyata tersimpan. Polanya sama dengan pendaftaran kunjungan |
| Perubahan sampingan | `NONE`. `test-results/` dan `playwright-report/` tidak berubah, karena keluaran Playwright diarahkan ke scratchpad |
| Interupsi | `NONE` |
| Status Git | `M` 7 berkas `src/` dan 5 berkas `tests/`. Belum di-commit |
| Langkah berikutnya | Uji layar dengan backend asli sesudah `B1`, `B2` terisi |
