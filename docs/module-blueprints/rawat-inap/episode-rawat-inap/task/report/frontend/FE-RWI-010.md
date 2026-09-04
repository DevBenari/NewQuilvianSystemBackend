# FE-RWI-010 — Pasien dapat dipindahkan, dan yang tidak berwenang melihatnya nonaktif

- TASK ID: `FE-RWI-010`
- TASK TYPE: Implementasi aksi frontend beserta aturan tampil tombol berpenjaga
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; tiga berkas source diubah 0; logika kompleks 2; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga kewenangan per pasien tanpa mengubahnya 1; satu alur berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /bed-occupancies/placements/transfer` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-010`; `03-frontend-architecture.md` bagian 3, 4.3A, 5.2, 5.3, dan 5.4; `InpatientBedOccupancyController.cs` bagian `TransferPatient` dan `FromFailure`; `InpBedOccupancyService.cs` baris 859–1000 tempat `GUARD-INP-01` dan kedelapan aturan kelayakan dipanggil; `InpatientBedOccupancyDtos.cs` (`TransferPatientRequest`, `BedPlacementResponse`); `InpatientSharedDtos.cs` (`PlacementEligibilityFailureResponse`); hasil `FE-RWI-005` (`use-inpatient-bed-board.jsx`, `inpatient-bed-board.jsx`, `inpatient-bed-utils.jsx`); hasil `FE-RWI-007` (`inpatient-placement-utils.jsx`, `placement-failure-list.jsx`); `confirm-modal.jsx`
- FILES CHANGED: `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (bagian perpindahan); `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` (aksi perpindahan); `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (bagian perpindahan beserta konfirmasinya); `tests/unit/inpatient-episode-detail.test.mjs`; `tests/e2e/inpatient-episode-detail.spec.mjs`

## `GUARD-INP-01` hanya berlaku bagi pemohon berperan dokter

Ini yang paling mudah dikerjakan terlalu ketat. Kepala ruangan, perawat pelaksana, dan
supervisor **tetap boleh** memindahkan pasien tanpa menjadi DPJP — `RWI-DEC-012` yang tidak
dicabut, dengan risikonya diterima secara sadar sebagai `RWI-RISK-001`. Yang ditolak hanyalah
**dokter yang bukan DPJP aktif episode itu**, dan tidak ada kolom keterangan yang dapat
dipakai melewatinya.

`resolveTransferAuthority` karena itu menyalin cabang service apa adanya: penjaga dokter baru
diperiksa setelah dipastikan pemohonnya memang seorang dokter, yaitu klaim `doctor_id`-nya
terisi.

- IMPLEMENTATION: (1) Bagian perpindahan menempel pada layar detail episode dan memakai **hook papan tempat tidur yang sama persis** dengan layar penempatan, lengkap dengan penyaring dan penanda kelayakannya — tidak ada daftar kedua. (2) Alasan medis diperiksa di layar sebelum permintaan dikirim, dengan pemeriksaan yang setara `HasMeaningfulReason` di service: alasan yang hanya berisi spasi dan tanda baca dihitung kosong. (3) Konfirmasi sebelum kirim menyebut tempat tidur asal **dan** tujuan, sesuai bagian 5.3, dan papan dibaca ulang tepat sebelum dialognya tampil sesuai bagian 5.2. (4) Penolakan dibaca `parsePlacementFailure` dan ditampilkan `PlacementFailureList` — keduanya milik `FE-RWI-007`, sehingga kode dan kalimat penolakan penempatan dan perpindahan identik. (5) Penjaga `transferInFlight` menahan klik kedua pada saat itu juga.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Payload memakai nama kolom `TransferPatientRequest` apa adanya: `episodeId`, `targetBedId`, `transferReason`.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization. Membuat `GUARD-INP-01` terlihat di layar; server tetap satu-satunya penentu.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Bentuk layar bebas". Dipilih satu bagian pada layar detail episode, bukan halaman tersendiri, supaya tempat tidur asal dan riwayat penempatan terbaca pada layar yang sama saat petugas memilih tujuan. Kalimat keterangan tombol nonaktif pada bagian 3 arsitektur **tidak** termasuk `DEV_DISCRETION` dan disalin kata per kata.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Tombol pindah hanya aktif bagi DPJP aktif episode tersebut; bila bukan, nonaktif disertai keterangan | **LULUS** | e2e dengan klaim dokter lain: tombol `disabled`, keterangan berbunyi persis "Anda bukan DPJP episode ini", dan tombol **Pilih** pada baris tempat tidur pun tidak dirender sama sekali sehingga tidak ada jalan memutar. Test unit menutup kelima peran: dokter lain ditolak, DPJP aktif boleh, dan perawat, kepala ruangan, serta supervisor tetap boleh tanpa menjadi DPJP |
| 2. Perpindahan wajib disertai alasan medis | **LULUS** | e2e menekan Pindahkan lalu mengiyakan konfirmasi tanpa mengisi alasan; pesan "Alasan medis perpindahan wajib diisi." muncul dan **nol** permintaan `POST /placements/transfer` terkirim. Test unit menolak alasan berisi spasi dan tanda baca saja |
| 3. Daftar tempat tidur tujuan memakai penyaring yang sama dengan layar penempatan — tidak ada daftar kedua | **LULUS** | Test unit membuktikan layar memakai `useInpatientBedBoard` dan `InpatientBedBoard`, serta **tidak** memanggil `bedOccupancyService` sendiri maupun menyebut `available-beds`; hook detail juga tidak menyebut `available-beds` maupun `bed-board`. e2e memastikan penandaan kelayakannya berasal dari jawaban server: `BD-001` yang terisi bertanda tidak dapat dipilih, `BD-002` dapat dipilih |
| 4. Penolakan 422 ditampilkan dengan pesan yang sama seperti penempatan | **LULUS** | e2e menolak perpindahan dengan `ROOM_GENDER_MIXED`; kalimat lengkap "Kamar Melati 1 sedang dihuni pasien Laki-laki, sehingga tidak dapat menerima pasien Perempuan." tampil **apa adanya**, termasuk nama kamarnya. Penyajinya `PlacementFailureList` yang sama dengan jalur penempatan |
| 5. Isian tidak hilang ketika ditolak | **LULUS** | e2e memeriksa nilai kotak alasan sesudah penolakan 422 dan menemukannya utuh. Test kode membuktikan jalur `catch` `handleTransfer` tidak menyentuh `setTransferReason`, dan pengosongan hanya terjadi pada jalur berhasil |

- VALIDATION: e2e `tests/e2e/inpatient-episode-detail.spec.mjs` | PASS, 10/10 | TASK | termasuk tombol nonaktif bagi dokter bukan DPJP dan penolakan pencampuran kamar lewat jalur perpindahan
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-episode-detail.test.mjs` | PASS, 16/16 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 106, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs`, rusak sejak sebelum task ini
- MANUAL TEST: NOT FEASIBLE — memerlukan akun dokter yang benar-benar menjadi DPJP satu episode dan akun dokter lain pada episode yang sama, ditambah pasien yang sudah menempati tempat tidur. Tidak tersedia tanpa database tim. Seluruh kontrol — pemilihan tempat tidur tujuan, kotak alasan, tombol pindah, dan dialog konfirmasi — dijalankan di browser sungguhan (Edge) lewat e2e dengan klaim `doctor_id` yang berbeda per kasus
- WARNINGS: Perpindahan yang berhasil belum diuji ujung ke ujung terhadap server sungguhan; yang diuji adalah bentuk permintaannya (`targetBedId` dan `transferReason` terkirim benar) dan jalur penolakannya. Perilaku "penempatan lama tidak jadi ditutup bila pembukaan yang baru gagal" sepenuhnya milik service dan tidak dapat dibuktikan dari layar
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task
- DEPENDENCY BACKEND: `BE-RWI-019` — `POST /placements/transfer` berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan
- INTERRUPTIONS: NONE
- GIT STATUS: Perubahan pada utils, hook, dan view detail episode beserta test-nya. **Belum di-stage dan belum di-commit**
- NEXT RECOMMENDED STEP: Ketika `FE-RWI-014` penutupan dikerjakan, pakai ulang `parsePlacementFailure` dan `PlacementFailureList` yang sama supaya penanganan 409/422 tetap satu tempat — sesuai catatan penutup `FE-RWI-007`
