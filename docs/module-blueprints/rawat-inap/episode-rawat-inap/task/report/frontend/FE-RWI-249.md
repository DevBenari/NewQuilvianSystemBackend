# Laporan Perubahan Frontend — `FE-RWI-249`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-249` |
| Judul | Tujuan transfer dipilih lewat denah |
| Slice | Integrasi Denah ↔ Bed Management — `FE-BM-02` mode Pilih Transfer |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-005`, `007`; `RWI-DEC-312`, `316`, `325`; `RWI-AC-436`, `450`, `453` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-248` ✅; `BE-RWI-239` ✅ melalui prasyarat; perilaku kelas khusus `BE-RWI-233` ✅ |
| Klasifikasi | `HEAVY` — satu consumer bersama dipakai tiga layar, dua snapshot versi harus segar, dan hasil mutasi dapat ambigu |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Model | Codex |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`, upstream `origin/HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Tab Bed Transfer, bagian transfer pada detail episode, dan Workspace Keperawatan sekarang memakai
consumer bersama `InpatientBedTransfer` yang memilih tujuan melalui denah. Denah memanggil
`GET .../bed-occupancies/floor-plan` dengan `purpose=Transfer` dan `episodeId`; lokasi asal tetap
diambil otomatis dari `GET .../transfer-context`.

Popup kamar hanya menawarkan **Pilih bed ini** untuk `Eligibility.IsEligible=true`. Pilihan kategori
berasal langsung dari `AllowedManualCategories`: kelas biasa memperoleh satu kategori server,
sedangkan perbandingan `Special` menampilkan `DownGrade`, `UpGrade`, dan `SameGrade` tanpa default
buatan frontend. Alasan 10–500 karakter tetap wajib.

Tepat sebelum modal konfirmasi, frontend memuat ulang konteks asal dan lantai tujuan secara paralel.
Payload `POST .../placements/transfer` menggunakan `sourcePlacementId`, versi placement asal, versi
bed asal, dan versi bed tujuan dari respons terbaru. Sesudah sukses, konteks, denah, dan daftar episode
dimuat ulang; pesan hasil menjelaskan bed asal Menunggu Pembersihan dan tujuan Terisi.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Hook transfer | `use-inpatient-bed-transfer.js` mengganti query tujuan `available-beds` dengan `useFloorPlan({ purpose: "Transfer", episodeId })`; `transfer-context` tetap menjadi sumber asal |
| Fresh snapshot | Persiapan konfirmasi memuat ulang konteks dan denah, mencari `bedId` terpilih pada snapshot baru, memeriksa ulang eligibility, lalu membentuk request dengan seluruh versi terbaru |
| Kategori kelas | Opsi dibaca dari `AllowedManualCategories`; auto-select hanya jika server memberi tepat satu opsi, sehingga kelas khusus tetap menawarkan tiga pilihan |
| Consumer bersama | `inpatient-bed-transfer.jsx` merender `FloorPlanSelection`; consumer yang sama sudah dipakai tab Bed Transfer, detail episode, dan Workspace Keperawatan |
| Kegagalan | 409 membersihkan tujuan, memuat ulang konteks/denah, dan meminta pilih ulang; 422/server rejection mempertahankan tujuan, kategori, alasan, serta modal untuk koreksi |
| Refresh sukses | Context, denah, daftar episode, monitoring induk, dan callback consumer disegarkan sesudah commit berhasil |
| Jalur legacy | `nursing-transfer-form-panel.jsx` tidak lagi merender board kartu; jika diaktifkan kembali, komponen hanya menerima `bedBoard.floorPlan` dan menampilkan peringatan bila state denah belum disediakan |
| Test | Test source/contract diperbarui untuk endpoint denah dan ditambah `floor-plan-transfer-selection.test.mjs` |

## 3. Kontrak dan keselamatan operasi

- `GET /floor-plan` selalu memakai `purpose=Transfer` serta episode yang memiliki placement aktif;
  status episode dan eligibility tetap diputuskan backend.
- Frontend tidak menghitung kelas, isolasi, status bed, atau eligibility. Outline, aksi pilih,
  `ComparisonResult`, dan kategori manual seluruhnya berasal dari server.
- `BedName` menjadi label tujuan; `BedCode` tidak dipakai sebagai label bed hasil denah.
- Modal baru dibentuk setelah `transfer-context` dan lantai terpilih selesai dimuat ulang. Snapshot
  yang hilang atau sudah tidak eligible membatalkan pilihan sebelum request mutation.
- Kegagalan 422 tidak menghapus input petugas. Ini mencakup server yang menolak kategori manual
  tidak cocok, misalnya pencatatan `DownGrade` pada perbandingan Kelas 2 → Kelas 1.
- Konflik 409 dianggap perubahan hunian definitif: target dan kategorinya dibersihkan, denah dimuat
  ulang, sedangkan mutasi baru hanya dapat dilakukan setelah petugas memilih dan mengonfirmasi ulang.
- Hasil jaringan ambigu tetap ditangani `useBedOperation`/`BedOperationRecovery` dengan intent,
  body, dan idempotency key yang sama.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Denah dan popup mode Pilih | `REUSE` | `FloorPlanSelection` dan `FloorPlanRoomPopup` dari `FE-RWI-248`; geometri/CSS denah tidak diubah |
| Consumer transfer | `EXTEND` | `InpatientBedTransfer` existing tetap menjadi satu consumer untuk tiga layar |
| Hook transfer | `REUSE_WITH_ADAPTER` | Permission, episode picker, transfer-context, payload, idempotency, modal, dan error flow existing dipertahankan; sumber target diganti ke denah |
| Board kartu tujuan | `STOP_RENDERING` | Tidak lagi dirender oleh consumer transfer maupun panel legacy |
| Service baru | `NOT CREATED` | `bed-occupancy.service.getFloorPlan` dari `FE-RWI-248` dipakai ulang |
| Komponen baru | `NOT CREATED` | Seluruh UI memakai komponen denah dan base form existing |

`UI GATE: WAIVED BY RWI-DEC-325`. Tampilan/geometri floor plan tidak diubah. Kontrak, permission,
privasi, stack, lint, build, dan pelaporan tetap mengikat.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Belum memilih episode | Petugas memilih pasien dengan hunian aktif pada tab Bed Transfer |
| Loading | Context asal dan denah menampilkan status loading; aksi konfirmasi ditahan |
| Bed tidak eligible | Tetap terlihat di popup beserta seluruh `Eligibility.Failures`; tidak ada tombol pilih |
| Kelas biasa | Satu kategori dari server dipilih otomatis |
| Kelas khusus | Tiga kategori tampil; petugas wajib memilih salah satunya |
| Refresh pra-konfirmasi | Context dan lantai dibaca ulang; versi payload berasal dari snapshot itu |
| Target berubah/hilang | Pilihan dibersihkan dan petugas diminta memilih ulang |
| Server 422 | Detail kegagalan ditampilkan; kategori dan alasan petugas tetap utuh |
| Server 409 | Modal ditutup, target/kategori dibersihkan, denah dimuat ulang |
| Hasil belum diketahui | Operation recovery memeriksa receipt sebelum mengizinkan retry same-key |
| Sukses | Asal Menunggu Pembersihan, tujuan Terisi; context, denah, episode, dan layar induk direfresh |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus transfer, denah, bed management, detail episode, admission regression, dan nursing consumer | 45/45 lulus | `PASS` |
| Test baru `floor-plan-transfer-selection.test.mjs` | 3/3 lulus: tiga consumer, fresh context/denah, serta pemulihan 409/422 | `PASS` |
| Kategori `Special` pada `inpatient-bed-management.test.mjs` | `AllowedManualCategories` menghasilkan nilai `1`, `2`, `3` | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone assets siap | `PASS` |
| `git diff --check` | Exit `0`; hanya warning normalisasi LF→CRLF pada file existing | `PASS` |
| `npm run test:unit` | 2.948 test; 2.918 lulus, 30 gagal existing di luar scope; jumlah gagal sama dengan baseline FE-RWI-248 | `EXISTING OUT-OF-SCOPE FAILURES` |
| HTTP route / live browser | Dev server/sesi/kredensial berotorisasi tidak tersedia | `NOT RUN` |
| UAT-804 dengan episode aktif, kelas khusus, serta race aktual | Membutuhkan akun dan fixture runtime rumah sakit | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 pada D-FE.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tiga consumer transfer memilih tujuan lewat denah | ✅ | Ketiganya merender consumer bersama; source test 3/3 PASS |
| Asal otomatis dari placement aktif | ✅ | `transfer-context` tetap dipakai dan dibaca ulang sebelum konfirmasi |
| Kelas 3 → ICU menawarkan Up, Down, dan Same Grade | ✅ | `Special` membaca ketiga `AllowedManualCategories`; unit test PASS |
| Pilihan kategori apa pun untuk kelas khusus dikirim sebagai enum manual | ✅ | `buildBedTransferRequest` menerima kategori yang diizinkan dan mengirim `manualTransferCategory` |
| Kelas 2 → Kelas 1 dengan kategori salah ditolak tanpa menghapus input | ✅ | Error 422 tidak menjalankan reset target/category/reason; modal menampilkan failure server |
| Alasan wajib | ✅ | Validasi 10–500 karakter sebelum modal dan pada request builder |
| Konfirmasi memakai versi respons terbaru | ✅ | `Promise.all` refresh context/denah mendahului request builder; test fokus PASS |
| Sukses menghasilkan pesan asal Menunggu Pembersihan/tujuan Terisi dan reload denah | ✅ | Copy sukses serta refresh context/denah/episode/callback terpasang |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, atau deployment.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global tetap memiliki 30 kegagalan di modul lain; tidak ada kegagalan pada 45 test fokus |
| Panel legacy nursing | File masih dipertahankan untuk kompatibilitas, tetapi consumer aktif Workspace Keperawatan adalah `InpatientBedTransfer` |
| Batas runtime | UAT kelas khusus, server rejection nyata, dan race 409 aktual `NOT RUN` karena server/sesi/fixture tidak tersedia |
| Perubahan sampingan | `NONE`; perubahan worktree existing di luar scope dipertahankan |
| Langkah berikutnya | `FE-RWI-250`: Master Bed menjadi editor denah |
