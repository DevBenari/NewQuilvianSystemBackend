# Laporan Perubahan Frontend — `FE-RWI-251`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-251` |
| Judul | Susunan denah diimpor dari layar Master Bed |
| Slice | Integrasi Denah ↔ Bed Management — `FE-DNH-04` |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-011`, `012`; `RWI-DEC-306`, `314`, `321`, `323`, `325`, `326`; `RWI-AC-446`, `451`, `459`, `461` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-250` ✅; `BE-RWI-237` ✅ |
| Klasifikasi | `HEAVY` — manifest 106 kamar/168 bed, dry run, import idempoten, pemetaan relasi, dan konsekuensi environment |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Admin dengan `Room : Read` dan `Room : ImportFloorPlan` sekarang mendapat tombol **Impor Susunan
Denah** pada editor Master Bed. Layar anak menyusun manifest deterministik dari geometri shared:
106 kamar, 168 bed termasuk 8 boks bayi, kunci/lantai/nomor/nama ruang, kelas resmi, dan arah wing
bawaan berdasarkan kisi 3 × 3 posisi label.

Admin dapat memetakan unit per kamar atau sekaligus per lantai, mengoreksi kelas/wing, dan memilih
kamar existing tanpa kunci. **Periksa (Dry Run)** mengirim manifest yang sama tanpa menulis, lalu
menampilkan ringkasan dan alasan per kamar. Baris tanpa unit sengaja dikirim agar backend melaporkan
`ROOM_UNIT_MAPPING_MISSING`; baris lain tetap dapat diimpor. Operasi impor memakai Idempotency-Key
dan konfirmasi eksplisit.

Kesiapan bed tidak disimpulkan dari build mode browser. Layar membaca environment runtime frontend
dari `/api/system/version`: Production dijelaskan tidak dapat mengesahkan bed siap, Development/
Staging dapat mengikuti konfigurasi backend, dan environment unknown diarahkan memakai angka
`BedsMarkedReady` dari respons server.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Route | Route baru `/health-services/master-data/bed/floor-plan-import` merender layar anak impor |
| Entry point | Toolbar editor Master menampilkan tombol impor hanya saat `Room : ImportFloorPlan` diizinkan |
| Manifest | `floor-plan-import-manifest.js` membangun 106 row, bed A–T, penanda boks, pemetaan nama kelas resmi, dan wing 1–8 |
| Hook | `use-floor-plan-import.js` memuat kelas/unit/kamar existing, bulk unit per lantai, dry run, import, hasil per kunci, error, dan konfirmasi |
| Service | `floor-plan-layout.service.js` menambahkan `POST /rooms/floor-plan-layout/import` melalui `runBedOperation` dan intent per pemilik sesi |
| View | Tabel per lantai, relasi berlabel nama+kode, hasil baris, ringkasan kamar/bed, jumlah bed siap, dan pesan environment runtime |
| Style | CSS modul denah diperluas khusus layar mapping, tabel responsif, summary, outcome, dan action bar |
| Test | `floor-plan-import.test.mjs` mengunci jumlah manifest, kelas, wing, payload, hasil, permission, endpoint, environment, dan idempotency |

## 3. Kontrak dan keselamatan operasi

- Payload mengikuti `FloorPlanImportRequest`: `dryRun`, versi manifest, 1–300 kamar, Guid kelas,
  unit nullable, wing enum 1–8, kamar existing opsional, serta bed berhuruf dan `isForNewborn`.
- Kelas dicocokkan terhadap nama resmi master. Amandemen `RWI-DEC-326` diterapkan: Premium 801–810
  memakai VVIP, 811 LUXURY, 812–813 SUITE; tidak ada master kelas Premium baru.
- Dry run dan import memakai endpoint/permission yang sama. Dry run tidak menulis dan hasilnya
  dinormalisasi menjadi Created/Updated/Unchanged/Rejected beserta code/message per kamar.
- Unit kosong tidak diblokir frontend karena kontrak menetapkannya sebagai penolakan per baris;
  tombol import tetap tersedia setelah dry run sehingga row valid dapat diproses.
- Kamar berkunci/existing tidak diubah kelas, unit, atau susunan bed-nya oleh importer; layar
  menjelaskan invariant tersebut sebelum konfirmasi.
- `runBedOperation` memasang Idempotency-Key, mempertahankan intent pada hasil ambigu, serta membuat
  key baru setelah sukses. Manifest sama aman dikirim ulang dan backend menjadi sumber fakta 0 baru.
- Environment berasal dari identitas runtime same-origin, bukan `NODE_ENV`; angka kesiapan selalu
  berasal dari `Summary.BedsMarkedReady` server.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Geometri, occupancy statis, dan kunci netral | `REUSE` | Manifest memakai `FLOORS` dan `roomsWithFloorPlanKeys` hasil FE-RWI-246 |
| Pilihan kelas/unit | `REUSE_WITH_ADAPTER` | Hook select existing dipetakan menjadi label nama + kode; unit non-rawat-inap disaring |
| Kamar existing | `REUSE_WITH_ADAPTER` | `RoomsWithoutFloorPlanKey` dari endpoint layout dinormalisasi menjadi opsi berlabel |
| Idempotency/recovery | `REUSE` | `getBedOperationIntent` dan `runBedOperation` menjadi satu jalur operasi mutasi |
| Identitas environment | `REUSE` | `useAppVersion` membaca `/api/system/version` yang sudah authoritative |
| View, hook, dan penyusun manifest | `CREATE_NEW` | Belum ada layar yang menggabungkan geometri, mapping 106 row, dry run, dan hasil per kamar |
| `Hero`, `BaseButton`, `InformationAlert`, `ConfirmModal`, `AccessDeniedGate` | `REUSE` | Komponen dasar existing memenuhi heading, aksi, alert, konfirmasi, dan access gate |

`UI GATE: WAIVED BY RWI-DEC-325`. Floor plan tidak diubah; CSS baru terbatas pada layar anak impor.
Kontrak, permission, privasi, lint, build, test, dan pelaporan tetap mengikat.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Permission | Tanpa salah satu `Room : Read/ImportFloorPlan`, `AccessDeniedGate` menutup layar; tombol toolbar juga tidak dirender |
| Loading opsi | Kelas, unit, dan kamar existing dimuat dari service yang sudah ada; error ditampilkan tanpa mengarang relasi |
| Kelas tidak cocok | Review ditahan, row difokuskan, admin memilih kelas resmi berlabel |
| Unit kosong | Tetap masuk dry run dan ditampilkan sebagai Rejected dengan alasan backend |
| Dry run | Ringkasan dan outcome per row tampil; pesan menegaskan belum ada data yang ditulis |
| Import | Modal konfirmasi tampil; baris rejected tidak ditulis; hasil final mengganti hasil review |
| Conflict 409 | Review lama dibuang dan pesan server ditampilkan agar admin memeriksa ulang |
| Hasil ambigu | Intent/key tetap dipertahankan oleh operation service untuk recovery aman |
| Environment unknown | UI tidak menganggap non-production; operator diarahkan ke `BedsMarkedReady` server |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus impor + editor + registry | 12/12 lulus | `PASS` |
| Test baru `floor-plan-import.test.mjs` | 6/6 lulus | `PASS` |
| Manifest | 106 kamar unik, 168 bed, 8 boks; kelas resmi dan 13 contoh wing eksak | `PASS` |
| ESLint fokus file task | Exit `0` | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 499/499 static pages, standalone siap | `PASS` |
| `npm run test:unit` | 2.958 test; 2.928 lulus, 30 gagal existing di luar scope; enam test baru lulus dan jumlah gagal tetap baseline | `EXISTING OUT-OF-SCOPE FAILURES` |
| `git diff --check` scope frontend | Exit `0` | `PASS` |
| HTTP route / live browser | Dev server, sesi, akun, dan fixture runtime berotorisasi tidak tersedia | `NOT RUN` |
| UAT-807, UAT-808 | Eksekusi aktual memerlukan akun/fixture dan wewenang D-DNH-3 | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 pada D-FE.
Runtime D-DNH-3 backend yang sudah tercatat membuktikan run kedua 0 baru; task ini tidak mengulangi
mutasi data karena user hanya memberikan wewenang tulis frontend.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tombol hanya untuk `Room : ImportFloorPlan` | ✅ | Permission hook + conditional toolbar + source test |
| Manifest geometri 106 kamar, 168 bed, 8 boks | ✅ | Unit test eksak dan kunci unik |
| Kelas cocok ke master resmi tanpa Premium baru | ✅ | Mapping RWI-DEC-326 + test Pascal/camel case |
| Delapan wing dan default 3 × 3 | ✅ | Opsi 1–8; contoh 330/319/301/309/318 dan delapan contoh lain PASS |
| Mapping unit per kamar dan bulk per lantai | ✅ | Row select + `assignFloorUnit` berdasarkan `floorCode` |
| Kamar existing opsional | ✅ | Opsi seluruh `RoomsWithoutFloorPlanKey`, payload nullable |
| Dry run menampilkan ringkasan dan alasan per kamar | ✅ | Summary Created/Updated/Unchanged/Rejected + outcome/message row |
| Unit kosong ditolak tanpa menutup impor row lain | ✅ | Payload mempertahankan null; import tetap tersedia setelah review |
| Import idempoten dan aman diulang | ✅ | `runBedOperation` + Idempotency-Key; hasil Unchanged/0 Created dapat ditampilkan |
| Kamar existing tidak diubah | ✅ | Copy konfirmasi dan kontrak backend dipertahankan |
| Kesiapan per environment | ✅ | `useAppVersion`; Production/non-production/unknown; `BedsMarkedReady` server |
| Semua kamar lintas unit tetap tampil per lantai | ✅ | Filter UI hanya berdasarkan `floorCode`, tidak berdasarkan unit |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, deployment, ataupun eksekusi importer terhadap data nyata.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global memiliki 30 kegagalan modul lain; tidak ada kegagalan pada test task |
| Runtime | UAT-807/808 frontend aktual `NOT RUN`; operasi data tetap membutuhkan wewenang D-DNH-3 |
| Perubahan sampingan | `NONE`; worktree existing di luar scope dipertahankan |
| Langkah berikutnya | `FE-RWI-252`: Panel High/Low Dependency |
