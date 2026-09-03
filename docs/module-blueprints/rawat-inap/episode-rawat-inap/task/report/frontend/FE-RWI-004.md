# FE-RWI-004 — Admin dapat mengelola butir daftar periksa administrasi

- TASK ID: `FE-RWI-004`
- TASK TYPE: Implementasi layar frontend
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; tujuh berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh penjagaan akses tanpa mengubahnya 1; satu layar dengan alur berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. `NewQuilvianSystemBackend` (branch `MHamzah`) hanya dibaca; tulisan ke backend terbatas pada laporan ini dan penanda status pada roadmap frontend
- TASK/CONTRACT VERSION: roadmap frontend revision `2` (`APPROVED` 24 Agustus 2026); api contract `0.4.0` bagian Inpatient Clearance Item — keenam endpoint berstatus ✅ **Tersedia** sejak 26 Agustus 2026
- FILES INSPECTED: roadmap frontend bagian `FE-RWI-004`; api contract bagian Inpatient Clearance Item; `InpatientClearanceItemController.cs`, `InpatientClearanceItemService.cs`, dan `InpatientClearanceItemDtos.cs` backend; `PagedResult`; fondasi `FE-RWI-002`; layar `FE-RWI-003` yang baru selesai; `DataFilter`, `DataTable`, `FilterSelect`, `StatusBadge`, `ConfirmModal`, `BaseEditorForm`, `BaseButton`, `ToastStack`, `InformationAlert`, `AccessDeniedGate`, `RegionPagination`; pola layar daftar master data tempat tidur
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-clearance-item-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-clearance-item-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-clearance-items.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-clearance-item-view.jsx` (baru); `src/app/health-services/inpatient-management/clearance-items/page.jsx` (baru); `tests/unit/inpatient-clearance-item.test.mjs` (baru); `tests/e2e/inpatient-clearance-item.spec.mjs` (baru); `src/utils/menu-sidebar/menu-items.jsx` (diubah)

## Yang dibangun

Satu layar di `/health-services/inpatient-management/clearance-items`, muncul sebagai menu **Butir
Administrasi** di bawah Rawat Inap. Keenam kemampuan ada di satu halaman: daftar bersaring, detail,
tambah, ubah, ubah status aktif, dan tandai terhapus. Tiga yang terakhir memakai modal — roadmap
memang membuka pilihan itu pada baris **Wewenang UI**.

Batas isian disalin apa adanya dari `CreateInpatientClearanceItemRequest`: kode butir 50 karakter,
nama 200, keterangan 500, urutan tampil 0–9999.

- IMPLEMENTATION: (1) Daftar memakai `DataFilter` + `DataTable` + `RegionPagination`, sama dengan layar master data lain; penyaring **Wajib/Tidak wajib** dan **Status** diterjemahkan menjadi query `isMandatory` dan `isActive` yang memang dikenal `GetAll`. Mengubah penyaring apa pun mengembalikan pembacaan ke halaman pertama, supaya petugas tidak melihat halaman kosong hanya karena halaman aktifnya melampaui hasil baru. (2) Form tambah dan ubah memakai `BaseEditorForm` di dalam `Modal`, jadi label, pesan galat per isian, dan penanganan angka sama persis dengan form master data lain — tidak ada kontrol baru yang dibuat. (3) **Ubah** membaca ulang butirnya lewat endpoint detail sebelum menyunting, supaya yang diubah bukan salinan baris daftar yang mungkin sudah usang. (4) Menonaktifkan dan menghapus keduanya lewat `ConfirmModal`; hapus meminta alasan dan mengirimkannya sebagai `deleteReason` supaya jejaknya tercatat di log server. (5) Seluruh panggilan lewat `inpatientClearanceItemService` milik fondasi `FE-RWI-002`.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Mengonsumsi keenam endpoint `Inpatient Clearance Item` pada api contract `0.4.0` apa adanya.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Layar bergantung penuh pada `AccessPermission("InpatientClearanceItem", "Read"/"Create"/"Update"/"Delete")` di server.
- VISUAL REFERENCE: NOT REQUIRED — roadmap menyatakan wewenang UI bebas, termasuk pemakaian modal. Yang dipakai komponen yang sudah ada; tidak ada komponen, warna, atau CSS baru.
- WEWENANG UI YANG DIPAKAI: "Bebas, termasuk pemakaian modal atau halaman terpisah". Dipilih **satu halaman dengan modal**, bukan empat route terpisah seperti layar tempat tidur, karena keenam kemampuan jadi terlihat sekaligus dan dapat dibuktikan dalam satu alur e2e.

## Butir tidak wajib tidak berarti otomatis

Roadmap menandai satu risiko: butir bawaan `DISCHARGE-MED` bertanda **tidak wajib** karena modul
Farmasi di luar scope, dan layar tidak boleh menyarankan bahwa penandaannya terjadi otomatis.

Yang dikerjakan untuk itu: sebuah keterangan tetap di atas daftar berbunyi *"Butir wajib menahan
penutupan episode selama belum ditandai. Butir tidak wajib tetap harus ditandai petugas, hanya saja
tidak menahan penutupan. Tidak ada butir yang tertandai dengan sendirinya oleh sistem."* Kalimat
yang sama diringkas pada keterangan isian **Wajib** di dalam form. Kolom daftarnya memakai kata
**Wajib** dan **Tidak wajib** — bukan tanda centang, yang mudah dibaca sebagai "sudah selesai".

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Enam kemampuan tersedia: daftar, detail, tambah, ubah, ubah status aktif, tandai terhapus | **LULUS** | e2e menjalankan keenamnya berurutan dalam satu alur dan mencocokkan **permintaan yang benar-benar terkirim** dengan keenam endpoint kontrak |
| 2. Penanda wajib atau tidak wajib terbaca jelas pada daftar | **LULUS** | e2e membaca kata `Wajib` pada baris `ADM-DOC` dan `Tidak wajib` pada baris `DISCHARGE-MED`, langsung dari DOM |
| 3. `ItemCode` kembar ditolak dengan pesan server apa adanya | **LULUS** | e2e menolak dengan 409 berisi kalimat asli `DuplicateCodeMessage`, dan kalimat itu yang muncul di layar. Isian yang sudah diketik tetap ada; setelah kodenya diganti, penyimpanan berhasil |
| 4. Hanya admin master data yang dapat membukanya | **SEBAGIAN** | Sama persis dengan `FE-RWI-003`: penolakan server memunculkan layar Akses Ditolak, tetapi menu tidak dapat disembunyikan per peran karena frontend belum punya penyaring menu berbasis hak akses. Rinciannya ada pada [laporan FE-RWI-003](FE-RWI-003.md) |

- VALIDATION: e2e `tests/e2e/inpatient-clearance-item.spec.mjs` di browser sungguhan | PASS, 2/2 | TASK | keenam kemampuan pada satu test, kode kembar pada test kedua
- VALIDATION: uji kekosongan pada asersi endpoint | PASS | TASK | asersi PATCH sengaja diubah menjadi `/activate`; test **gagal** dengan pesan "ubah status tidak terpanggil". Asersinya membaca permintaan yang benar-benar tercatat, bukan sekadar keberadaan tombol. Spec dipulihkan dari salinan
- VALIDATION: penjagaan 404 pada layar butir | PASS | TASK | seluruh rute butir di luar keenam endpoint kontrak sengaja dibalas 404 oleh tiruan; nol respons 404 tercatat sepanjang alur
- VALIDATION: `node --test tests/unit/inpatient-clearance-item.test.mjs` | PASS, 11/11 | TASK | pembacaan dan pengiriman butir, kelengkapan payload, keterangan kosong menjadi `null`, batas urutan tampil, penanda wajib sebagai kata, terjemahan penyaring menjadi query, bentuk `PagedResult`, pesan kode kembar, keenam pemanggilan endpoint, larangan jalur HTTP baru, dan jalur gagal yang tidak mengosongkan form
- VALIDATION: seluruh unit test kecuali satu berkas yang rusak sejak merge | PASS, 60/60 | TASK | `auth-security.test.mjs` dikecualikan — kerusakannya milik merge `6bba90ae1`, tercatat pada [laporan FE-RWI-003](FE-RWI-003.md)
- VALIDATION: e2e `inpatient-setting.spec.mjs` dan `bed-status-toggle.spec.mjs` | PASS, 3/3 | TASK | dijalankan ulang bersama; layar `FE-RWI-001` dan `FE-RWI-003` tidak terpengaruh perubahan menu maupun berkas baru
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK | seluruh repository, tanpa error
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/clearance-items` terbaca pada keluaran build
- VALIDATION: `git diff --check` | PASS | TASK | tidak ada whitespace error
- VALIDATION: `npm run test:unit` | NOT RUN | EXISTING ISSUE | script-nya rusak sejak merge `6bba90ae1`; bentuk glob dipakai sebagai gantinya
- VALIDATION: `npx playwright test` dengan konfigurasi bawaan repository | NOT RUN | ENVIRONMENT ISSUE | belum ada `playwright.config` dan versi binary browser tidak cocok; e2e dijalankan lewat Edge sistem memakai konfigurasi sementara yang dihapus setelah selesai

## Verifikasi manual

- MANUAL TEST: NOT FEASIBLE — menekan tombolnya terhadap backend sungguhan memerlukan akun berhak `InpatientClearanceItem : Create/Update/Delete`, dan akun itu tidak tersedia. Rutenya sudah dipastikan hidup: `GET /api/v1/health-services/master-data/inpatient-clearance-items` pada backend yang menyala menjawab **401** tanpa token. Seluruh kontrol interaktif — tombol Detail, Ubah, Nonaktifkan, Hapus, tombol Tambah, ketiga penyaring, modal konfirmasi beserta isian alasan, dan tombol simpan pada kedua mode form — dijalankan di browser sungguhan lewat e2e dengan API tiruan, dan efeknya terhadap permintaan yang terkirim diperiksa satu per satu.

## Delta terhadap roadmap

| Butir | Roadmap | Kenyataan | Alasan |
| --- | --- | --- | --- |
| Reuse state | "Sama dengan `FE-RWI-003`" — pola slice master data | Memakai slice fondasi `FE-RWI-002`, sama seperti `FE-RWI-003` | Konsisten dengan keputusan yang sudah dicatat pada laporan `FE-RWI-003` |
| Alasan hapus | Tidak disebut roadmap | Layar meminta alasan dan mengirim `deleteReason` | `DeleteInpatientClearanceItemRequest` menyediakan field-nya dan controller mencatatnya ke log. Mengosongkannya berarti membuang jejak yang sudah disiapkan backend |

- WARNINGS: (1) e2e memakai API tiruan; yang terbukti perilaku layar dan bentuk permintaan, bukan bahwa database tim menerima perubahan. (2) Daftar butir pada database tim mungkin masih kosong sampai seeder `BE-RWI-002` dijalankan; layar menampilkan keadaan kosong apa adanya, bukan galat.
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task. Tiga kerusakan repository yang ditemukan saat `FE-RWI-003` — `npm run test:unit` yang tidak dapat dijalankan, `auth-security.test.mjs` yang mengimpor berkas `.jsx` tidak ada, dan `route-smoke.spec.mjs` yang menuntut 219 route sementara kenyataannya 467 — **masih terbuka** dan tidak disentuh task ini.
- RISIKO: Kriteria 4 mewarisi kekurangan yang sama dengan `FE-RWI-003`. Setiap layar Rawat Inap berikutnya akan mewarisinya juga selama penyaring menu berbasis hak akses belum ada.
- DEPENDENCY BACKEND: `FE-RWI-002` selesai; `BE-RWI-005` terbukti hidup — keenam endpoint `Inpatient Clearance Item` berstatus ✅ `Tersedia` pada api contract `0.4.0`, dan rutenya menjawab 401 tanpa token pada aplikasi yang menyala. Tidak ada perubahan backend yang dibutuhkan maupun dilakukan.
- INCIDENTAL CHANGES: `test-results/.last-run.json` sempat berubah karena Playwright dijalankan, lalu dipulihkan dengan `git checkout --` pada berkas itu saja. Konfigurasi Playwright sementara dan folder hasil percobaan e2e sudah dihapus.
- INTERRUPTIONS: NONE
- GIT STATUS: Enam berkas baru dan satu berkas diubah (`menu-items.jsx`) untuk task ini. Berkas `FE-RWI-003` juga masih menunggu di working tree karena belum di-commit. Seluruhnya **belum di-stage dan belum di-commit**. Backend hanya menerima laporan ini dan penanda status pada `roadmap/frontend-roadmap.md`. Tidak ada stage, commit, push, pull, merge, rebase, atau deploy yang dilakukan.
- NEXT RECOMMENDED STEP: Slice **F1 — Fondasi dan master** selesai seluruhnya. Sebelum masuk `FE-RWI-005`, tutup dulu gerbang kesiapan data master `RWI-DEC-063`: tanpa kamar dan tempat tidur yang penandanya benar, papan tempat tidur dapat ditulis tetapi tidak dapat diuji dengan data nyata.
