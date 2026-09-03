# FE-RWI-005 — Petugas dapat melihat tempat tidur yang benar-benar dapat dipakai

- TASK ID: `FE-RWI-005`
- TASK TYPE: Implementasi layar frontend
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; enam berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh penjagaan akses tanpa mengubahnya 1; satu layar terbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. `NewQuilvianSystemBackend` (branch `MHamzah`) hanya dibaca; tulisan ke backend terbatas pada laporan ini dan penanda status roadmap
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` bagian Bed Occupancy — `GET /available-beds` dan `GET /bed-board` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-005`; api contract bagian Bed Occupancy; `InpatientBedOccupancyDtos.cs` (`AvailableBedQuery`, `AvailableBedResponse`, `BedBoardResponse` beserta turunannya); `InpBedOccupancyService.cs` bagian pemeriksaan kelayakan; registry `health-service-select-resources.js` dan `select-resource-registry.js`; `DataFilter`, `ResourceFilterSelect`, `useSelectResource`, `StatusBadge`, `InformationAlert`; fondasi `FE-RWI-002`
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` (baru); `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx` (baru); `src/app/health-services/inpatient-management/bed-board/page.jsx` (baru); `tests/unit/inpatient-bed-board.test.mjs` (baru); `tests/e2e/inpatient-bed-board.spec.mjs` (baru); `src/utils/menu-sidebar/menu-items.jsx` (diubah)

## Yang dibangun

Layar `/health-services/inpatient-management/bed-board`, muncul sebagai menu **Papan Tempat Tidur**.
Papannya membaca dua endpoint sekaligus: `GET /bed-board` untuk susunan unit layanan → kamar →
tempat tidur beserta keadaannya, dan `GET /available-beds` untuk daftar tempat tidur yang benar-benar
boleh dipakai.

Komponen papannya sengaja dibuat dapat dipakai ulang. Layar `FE-RWI-006` memakai komponen yang sama
dengan `episodeId` terisi, sehingga tidak ada dua versi papan yang bisa berselisih.

- IMPLEMENTATION: (1) Papan disusun apa adanya dari `BedBoardResponse`; layar tidak menambah, membuang, atau mengurutkan ulang isinya. (2) Penanda "boleh dipilih" ditentukan **hanya** dengan mencocokkan `bedId` terhadap daftar yang dikembalikan `/available-beds`. Tidak ada perbandingan jenis kelamin, kebutuhan isolasi, atau penghuni kamar di sisi layar. (3) Tempat tidur yang tidak boleh dipakai tetap ditampilkan sebagai baris redup, lengkap dengan alasannya sebagai teks. (4) Penyaring unit layanan, kamar, kelas, dan kata kunci diteruskan ke server sebagai query, bukan dikerjakan di layar.
- API CONTRACT IMPACT: Tidak mengubah kontrak.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Layar bergantung pada `AccessPermission("InpatientBedOccupancy", "Read")` di server.
- VISUAL REFERENCE: NOT REQUIRED — roadmap menyatakan bentuk penandaan bebas.
- WEWENANG UI YANG DIPAKAI: "Bentuk penandaan bebas — baris redup, ikon, kelompok terpisah, atau penyaring yang dapat dimatikan". Dipilih **baris redup beserta alasan tertulis**, bukan ikon, karena kriteria 4 menuntut alasannya terbaca petugas.

## Kenapa alasan "tidak lolos kelayakan" sengaja tidak menyebut aturannya

Aturan Kelayakan Penempatan ada sembilan dan seluruhnya milik server. Papan hanya punya kolom yang
memang dikirim server, dan itu cukup untuk menjelaskan tiga keadaan: terisi (beserta nama pasien dan
nomor episodenya), sedang dipesan (beserta nomor episodenya), dan status tempat tidur seperti
`Maintenance`.

Untuk sisanya — tempat tidur yang tampak tersedia tetapi tidak diloloskan server — layar menulis
apa adanya bahwa server tidak memasukkannya ke hasil pencarian, dan alasan lengkapnya muncul bila
penempatan tetap dicoba. Menebak "mungkin jenis kelaminnya tidak cocok" akan melahirkan aturan
kedua di layar, dan aturan itulah yang akan salah lebih dulu ketika server berubah.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Hasil `GET /available-beds` ditampilkan apa adanya; layar tidak menyaring ulang | **LULUS** | Test kode membuktikan `isForMale`, `requiresIsolation`, dan kata `gender` **tidak muncul sama sekali** pada utils, hook, maupun komponen papan; satu-satunya penentu adalah `buildSelectableBedIndex` yang membaca daftar server |
| 2. Tempat tidur yang tersaring keluar ditampilkan sebagai baris nonaktif disertai alasannya | **LULUS** | e2e: kelima baris tampil, empat di antaranya `data-selectable="false"` dan tetap terbaca |
| 3. Tempat tidur yang tidak layak tidak dapat dipilih | **LULUS** | e2e memeriksa atribut `data-selectable` tiap baris; tombol pilih disetel `disabled` |
| 4. Alasan penolakan terbaca petugas, bukan hanya tersembunyi di balik ikon | **LULUS** | e2e membaca teks "Terisi" + "Ny. Sari", "Dipesan" + "RI-0002", "Maintenance", dan "Tidak lolos kelayakan" langsung dari DOM |
| 5. Papan mengelompokkan per unit layanan dan kamar | **LULUS** | e2e menemukan judul unit "Rawat Inap Melati" dan kamar "Melati 1" sebagai kelompok terpisah |

- VALIDATION: e2e `tests/e2e/inpatient-bed-board.spec.mjs` di browser sungguhan | PASS, 1/1 | TASK | pengelompokan, penanda, alasan, dan penerusan penyaring ke server
- VALIDATION: `node --test tests/unit/inpatient-bed-board.test.mjs` | PASS, 6/6 | TASK | pembacaan papan, indeks tempat tidur terpilih, keempat bentuk alasan, penyusunan query, dan bukti tidak adanya aturan kelayakan kedua
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/bed-board` terbaca pada keluaran build
- VALIDATION: seluruh unit test kecuali berkas yang rusak sejak merge | PASS, 82/82 | TASK
- VALIDATION: `git diff --check` | PASS | TASK
- MANUAL TEST: NOT FEASIBLE — memerlukan akun berhak `InpatientBedOccupancy : Read` dan data kamar serta tempat tidur yang sudah siap. Gerbang kesiapan data master `RWI-DEC-063` masih terbuka. Perilaku layar diverifikasi di browser sungguhan lewat e2e dengan API tiruan.
- WARNINGS: Gerbang `RWI-DEC-063` belum tertutup, sehingga papan ini belum dapat diuji dengan data kamar dan tempat tidur yang sebenarnya.
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task.
- DEPENDENCY BACKEND: `BE-RWI-010` — `GET /available-beds` dan `GET /bed-board` berstatus ✅ `Tersedia`. Tidak ada perubahan backend.
- INCIDENTAL CHANGES: `test-results/.last-run.json` dipulihkan setelah Playwright dijalankan; konfigurasi Playwright sementara dihapus.
- INTERRUPTIONS: NONE
- GIT STATUS: Berkas baru dan satu berkas menu diubah, **belum di-stage dan belum di-commit**.
- NEXT RECOMMENDED STEP: Tutup gerbang `RWI-DEC-063` supaya papan dapat diuji dengan data nyata.
