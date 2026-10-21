# Laporan Perubahan Frontend — `FE-RWI-250`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-250` |
| Judul | Master Bed menjadi editor denah |
| Slice | Integrasi Denah ↔ Bed Management — `FE-DNH-03` mode Master |
| Roadmap | [`frontend-roadmap-integrasi-denah.md`](../../../roadmap/frontend-roadmap-integrasi-denah.md) |
| Trace | `FR-DNH-008`, `009`, `010`; `RWI-DEC-304`, `306`, `313`, `317`, `318`, `325`; `RWI-AC-436`, `444`, `451`, `454`–`457` |
| Contract version | `0.13.0` approved |
| Dependency | `FE-RWI-247` ✅; `BE-RWI-235` ✅; `BE-RWI-236` ✅ |
| Klasifikasi | `HEAVY` — mengganti satu layar master, tujuh mutasi operasional, snapshot versi, idempotensi, dan ruang cadangan |
| Task mode | `FRONTEND` |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/roadmap/traceability di blueprint backend |
| Commit frontend saat mulai rangkaian | `fc9aea6d62418460493899cb669832f032e48328` (`HamzahV2`) |
| Commit backend rujukan | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` (`MHamzah`) |
| Tanggal | 17 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Outcome

Menu Master Bed sekarang merender editor denah per lantai, bukan tabel master lama. Data berasal dari
`GET /v1/health-services/master-data/rooms/floor-plan-layout`; respons hanya memuat jumlah hunian dan
pesanan, tanpa identitas pasien. Kamar yang sudah dipetakan membuka popup Master untuk mengubah kelas
dan wing, menambah bed atau boks, mengubah label, menutup/membuka bed, serta menghapus bed tanpa
riwayat atau menonaktifkan bed yang sudah mempunyai riwayat.

Ruang cadangan Lantai 3 dan 4 tampil abu-abu hanya pada mode Master. Admin berhak dapat
mengaktifkannya secara atomik dengan nomor kamar, kelas, unit rawat inap, arah wing, jumlah bed, dan
jumlah boks. Sesudah aktivasi, kunci ruang itu ikut geometri shared sehingga kamar aktif muncul pada
Monitoring dan mode Pilih ketika endpoint operasional mengembalikannya.

Daftar **Kamar tanpa posisi di denah** ditampilkan di bawah denah dan menaut ke Master Kamar dengan
pencarian nama. Relasi kelas dan unit selalu dipilih dari opsi berlabel nama + kode; Guid tidak pernah
ditampilkan atau diketik petugas.

## 2. Perubahan source

| Area | Implementasi |
| --- | --- |
| Route Master Bed | `bed-client.jsx` mengganti `MasterDataBedView` dengan `MasterDataBedFloorPlanView` |
| View | View baru menyusun Hero, toolbar refresh, `FloorPlanView` mode `master`, popup Master, modal konfirmasi, dan daftar kamar tanpa posisi |
| Hook | `use-floor-plan-layout.js` menangani permission, request sequence/cancel, normalisasi layout, pilihan kelas/unit/wing, snapshot versi, mutasi, refresh, serta kegagalan server |
| Service | `floor-plan-layout.service.js` menyediakan GET layout/detail dan aktivasi ruang cadangan dengan `runBedOperation`/Idempotency-Key |
| Popup Master | Form aktivasi, kelas+wing, tambah bed/boks, ubah label, hapus/nonaktif, dan buka/tutup; semua mutasi meminta alasan operasional |
| Geometri | Registry menambahkan tiga ruang cadangan berkunci stabil; hook denah shared membawa ruang aktif cadangan ke Monitoring/Pilih tanpa menampilkan ruang belum aktif |
| Akses | `Room : Read/Create/Update` dan `Bed : Create/Update/Delete` menentukan tampilan aksi; server tetap otoritatif |
| Test | `floor-plan-master-editor.test.mjs` mengunci normalisasi `4/2`, payload preserve-field, kontrak endpoint/writer, riwayat bed, dan tombol kelas berpenghuni |

## 3. Kontrak dan keselamatan operasi

- Ubah kamar membaca detail terbaru lalu `useBedMasterConfirmation` menambahkan seluruh
  `AffectedBedVersions` sebelum `PUT /rooms/{id}`. Tombol tetap dapat ditekan pada kamar berpenghuni;
  server menolak perubahan kelas dengan pesan jumlah penghuni.
- Tambah bed/boks memakai `POST /beds`; label dan huruf berikutnya dibentuk server. Frontend tidak
  menghitung `329-D` dan selalu memuat ulang layout sesudah sukses.
- Ubah label membaca detail bed terbaru dan mengirim `ExpectedBedVersion` melalui `PUT /beds/{id}`.
- Bed tanpa riwayat memakai `DELETE /beds/{id}`. Bed bersejarah tidak ditawarkan untuk dihapus;
  frontend memakai `PATCH /beds/{id}/status` dengan `isActive=false`.
- Buka/tutup memakai `PATCH /beds/{id}/availability`; frontend hanya memilih status master
  `Available` atau `Blocked`. Backend menjaga bed terisi/dipesan dan invalidasi kesiapan.
- Aktivasi ruang cadangan memakai satu intent idempoten dan kontrak
  `POST /rooms/floor-plan-layout/rooms`; kamar serta semua bed/boks dibuat atomik oleh backend.
- Semua aksi menuntut alasan, konfirmasi snapshot, menangani 409 sebagai snapshot basi, menampilkan
  pesan 422/duplikasi dari server, lalu memuat ulang lantai sesudah sukses.

## 4. Keputusan reuse / create-new

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| `FloorPlanView`, drawing, zoom, lantai, badge | `REUSE` | Mode `master` memakai renderer dan geometri shared dari FE-RWI-246/247 |
| Modal mutasi master | `REUSE` | `BedMasterMutationConfirmation` dan `useBedMasterConfirmation` menyediakan alasan, fresh snapshot, dan recovery conflict |
| Adapter writer master | `REUSE` | `requestBedMasterMutation` tetap menjadi jalur PUT/PATCH/DELETE ber-versi dan idempoten |
| Pilihan kelas/unit | `REUSE_WITH_ADAPTER` | Hook select existing dipetakan menjadi label nama + kode; unit disaring ke rawat inap bila metadata tersedia |
| View dan popup Master | `CREATE_NEW` | Tidak ada komponen existing yang menggabungkan posisi fisik, ruang cadangan, dan writer master dalam popup denah |
| Service layout | `CREATE_NEW` | Dua endpoint `floor-plan-layout` belum mempunyai adapter frontend |
| Layar tabel lama | `STOP_RENDERING` | File legacy dipertahankan sementara untuk cleanup FE-RWI-253, tetapi route aktif tidak lagi merendernya |

`UI GATE: WAIVED BY RWI-DEC-325`. Renderer denah existing dipertahankan; penambahan CSS hanya untuk
mode Master, ruang cadangan, form popup, dan daftar kamar tanpa posisi. Kontrak, permission, privasi,
stack, lint, build, serta pelaporan tetap mengikat.

## 5. State dan kegagalan

| State | Perilaku |
| --- | --- |
| Loading/error lantai | Layout dibatalkan saat ganti lantai; respons stale diabaikan; retry tersedia |
| Kamar aktif | Badge kapasitas/terisi dan popup writer diturunkan dari respons layout |
| Posisi tanpa data | Mode Master menampilkan placeholder “Belum dipetakan”; diarahkan ke Master Kamar |
| Ruang cadangan | Tampil abu-abu; form aktivasi hanya untuk `Room : Create` |
| Kamar berpenghuni | Tombol ubah tetap aktif; warning menyebut jumlah pasien/pesanan; server menjadi guard akhir |
| Bed bersejarah | Aksi bertuliskan “Nonaktifkan”, bukan “Hapus” |
| Conflict 409 | Modal snapshot dibatalkan dan pesan server ditampilkan untuk persiapan ulang |
| Validation 422/duplikasi | Input tetap ada dan pesan server ditampilkan apa adanya |
| Sukses | Modal ditutup, pesan sukses tampil, layout lantai dimuat ulang |

## 6. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Test fokus denah Master + monitoring + admission + transfer + registry | 16/16 lulus | `PASS` |
| Test baru `floor-plan-master-editor.test.mjs` | 4/4 lulus: `4/2`, preserve-field+wing, semua writer, serta kelas berpenghuni | `PASS` |
| ESLint fokus sembilan file task | Exit `0` | `PASS` |
| `npm run lint:errors` | Exit `0`, tidak ada error ESLint | `PASS` |
| `npm run build` | Exit `0`; Next.js 16.2.12 compiled, TypeScript selesai, 498/498 static pages, standalone assets siap | `PASS` |
| `npm run test:unit` | 2.952 test; 2.922 lulus, 30 gagal existing di luar scope; empat test baru lulus dan jumlah gagal tetap sama dengan baseline | `EXISTING OUT-OF-SCOPE FAILURES` |
| `git diff --check` frontend | Exit `0`; hanya warning normalisasi LF→CRLF pada file existing | `PASS` |
| HTTP route / live browser | Dev server, sesi, akun, dan fixture runtime berotorisasi tidak tersedia | `NOT RUN` |
| UAT-805, UAT-806 | Memerlukan data kamar/bed aktual dan akun MasterData | `NOT RUN` |

UAT browser tidak menahan status selesai berdasarkan keputusan pemilik 1 September 2026 pada D-FE.
`git diff --check` repository dokumen secara global menemukan whitespace pada dokumen ODC existing
di luar scope; file laporan/roadmap/trace task diperiksa terpisah.

## 7. Acceptance criteria dan DoD

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Master Bed menjadi editor denah shared | ✅ | Route aktif merender view mode Master; source test PASS |
| Kelas dan wing dapat diubah dengan alasan dan snapshot versi | ✅ | Form berlabel, delapan wing, detail fresh, `AffectedBedVersions`, `PUT Room` |
| Kamar berpenghuni tidak diblokir frontend dan pesan server dapat tampil | ✅ | Submit tidak disabled oleh occupancy; warning menyebut jumlah penghuni; server guard dipertahankan |
| Tambah bed pada 329 menghasilkan label server dan badge hasil reload `4/2` | ✅ | Frontend tidak membentuk label; fixture normalisasi `329-D`/`4/2` PASS |
| Tambah boks bayi tersedia | ✅ | Opsi Bed pasien/Boks bayi dikirim sebagai `isForNewborn` |
| Bed baru/ubah kelas tetap menunggu verifikasi kesiapan | ✅ | Copy UI menjelaskan status; frontend tidak mengarang Ready/Available, backend contract menjadi sumber |
| Hapus hanya tanpa riwayat; bersejarah dinonaktifkan | ✅ | Branch `hasUsageHistory`, permission Delete/Update, DELETE vs PATCH status; test source PASS |
| Ubah label dan buka/tutup bed memakai writer terkunci | ✅ | Detail+version, PUT, PATCH availability, alasan, konfirmasi |
| Aktivasi ruang cadangan 324/kelas/unit/wing/kapasitas | ✅ | Form lengkap, endpoint atomik idempoten, reload lintas mode melalui shared reserve geometry |
| Kamar tanpa posisi menaut ke Master Kamar | ✅ | Daftar server + tautan pencarian nama |
| Laporan, roadmap, graph, register, traceability | ✅ | Laporan ini dan pembaruan dokumen delivery |

Definition of Done `D-FE` terpenuhi. Tidak ada perubahan backend source, database, migrasi, commit,
push, merge, atau deployment.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah existing | Suite global tetap memiliki 30 kegagalan di modul lain; tidak ada kegagalan pada 16 test fokus |
| Runtime | Mutasi nyata, duplikasi server, dan race 409 aktual `NOT RUN` karena server/sesi/fixture tidak tersedia |
| Cleanup | `master-data-bed-view.jsx` legacy belum dihapus; penghapusan fisik dijadwalkan FE-RWI-253 |
| Perubahan sampingan | `NONE`; perubahan worktree existing di luar scope dipertahankan |
| Langkah berikutnya | `FE-RWI-251`: Susunan denah diimpor dari layar Master Bed |
