# Requirement Traceability — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 11
revision_11_scope: ACCEPTANCE_REHOME_AC_BD_071_AND_VERIFICATION_POLICY
revision_5_scope: CROSS_MODULE_DEPENDENCY_AND_STATUS_ONLY
revision_6_scope: BLOCKER_REFRESH_ONLY
revision_7_scope: PLATFORM_DEPENDENCY_CONCRETE
revision_8_scope: ACCEPTANCE_REHOME_BE_BD_012
revision_9_scope: ACCEPTANCE_FORWARD_BE_BD_004
revision_10_scope: ACCEPTANCE_FORWARD_BE_BD_015
revision_11_note: >-
  AC-BD-071 berpindah pemilik dari BE-BD-006 ke BE-BD-009, pemilik endpoint reallocate.
  Nol kriteria bertambah terbukti, nol kehilangan bukti; hitungan tetap 48 dari 102.
  Kebijakan verifikasi repository dicatat pada backend-roadmap.md bagian 0.1.
status: FORWARD-TEST / DRAFT
status_note: >-
  backend-roadmap.md revisi 7 disetujui Sukmagp 2026-09-10 dan gerbang G4 tertutup pada
  hari yang sama. Dokumen penelusuran ini mengikuti roadmap frontend yang masih DRAFT;
  isinya sudah diperbarui untuk mencerminkan penutupan G4.
contract_version: v4 (approved)
backend_source_sha: 55ac6ab
backend_source_sha_note: >-
  Rentang 55ac6ab..f0d6855 mengubah nol berkas .cs. PERINGATAN: working tree memuat 17
  berkas .cs milik BE-BD-005 dan BE-BD-011 yang belum ter-commit.
frontend_source_sha: f79af16847c99961842081f707bc0c4ff6c2d93b
decision_revision: 12
acceptance_criteria_range: AC-BD-001 .. AC-BD-102
task_range_backend: BE-BD-001 .. BE-BD-016
task_range_frontend: FE-BD-001 .. FE-BD-012
```

---

## 0. Apa yang dijaga dokumen ini

> **Pembaruan dokumentasi 14 September 2026.** Hasil penutupan BE-BD-006 mengikuti ringkasan Claude yang diteruskan pemilik, bukan pengujian ulang dalam review ini. Rincian dan keterbatasan lampiran primer ada di [BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9. Audit permission hanya membuktikan 30 deklarasi unik; tidak membuktikan hasil seeder atau akses non-SuperAdmin.

Dokumen ini menjawab satu pertanyaan: **apakah setiap kebutuhan yang sudah diputuskan punya task yang
mengerjakannya, dan punya cara membuktikan bahwa ia benar-benar dikerjakan.**

Ia **tidak** menilai kesiapan modul, **tidak** menyetujui apa pun, dan **tidak** menggantikan
`verify-module-readiness`. Kalau sebuah baris di sini menunjuk task yang belum jalan, itu berarti
penelusurannya utuh tetapi buktinya belum ada — dua hal yang berbeda.

---

## 1. Kebutuhan bisnis ke task

| Kebutuhan | Keputusan | Task backend | Task frontend | Keadaan |
| --- | --- | --- | --- | --- |
| Katalog komponen darah terkendali | `DEC-BD-024`, `DEC-BD-032` | ✅ `BE-BD-001` | ✅ `FE-BD-001` | Backend & frontend **terbukti** ([FE-BD-001](../task/report/frontend/FE-BD-001.md)). **14 September 2026:** sempat 🟡 karena registrasi DI `BloodComponentService` hilang lewat merge `27d737cd`, lalu **kembali ✅** pada hari yang sama sesudah dipulihkan dan smoke validation lolos ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.4) |
| Daftar alasan berkategori | `DEC-BD-044`, `DEC-BD-024` | ✅ `BE-BD-001` | ✅ `FE-BD-001` | Backend & frontend **terbukti** ([FE-BD-001](../task/report/frontend/FE-BD-001.md)). **14 September 2026:** sempat 🟡 karena registrasi DI `BloodBankReasonService` hilang lewat merge yang sama, lalu **kembali ✅**. Daftar alasan terbukti dipakai runtime — `TBD006-BATALALOK` menutup `AC-BD-043`/`044`, dan alasan berkategori salah ditolak `422` pada `AC-BD-045` |
| Kewenangan unit memesan darah dari konfigurasi | `DEC-BD-012` | ✅ `BE-BD-002` | — | Penegakan **terbukti** di ✅ `BE-BD-003` — `VAL-BD-013`, [laporan](../task/report/backend/BE-BD-003.md) |
| Lokasi penyimpanan darah dikelola | `DEC-BD-035`, `DEC-BD-037` | ✅ `BE-BD-014` | 🟡 `FE-BD-011` | Backend **terbukti**. Frontend **dikerjakan 10 September 2026** ([laporan](../task/report/frontend/FE-BD-011.md)) — layar `FE-BD-10` berdiri, build lulus, tetapi **1 dari 2 acceptance**: `FE-BD-015` menunggu angka kantong tertahan dari `BE-BD-015`. **Diperbarui 11 September 2026:** angkanya kini tersedia — `HeldUnitCount` pada `GET /blood-storage-locations/{id}` ([BE-BD-015](../task/report/backend/BE-BD-015.md)); layar belum memakainya. **Diperbarui 14 September 2026 — dua hal.** Pertama, backend sempat 🟡 karena registrasi DI `BloodStorageLocationService` hilang lewat merge `27d737cd`, lalu **kembali ✅** sesudah dipulihkan. Kedua, **`HeldUnitCount` terbukti bernilai `0` tepat pada balasan penonaktifan lokasi** walau pesan pada balasan yang sama menyebut ada 1 kantong tertahan — sehingga pada skenario yang dilaporkan angka yang ditunggu `FE-BD-011` masih salah di balasan penonaktifan. Milik `BE-BD-015`, **belum diperbaiki** ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6) |
| Hak akses per tindakan | `DEC-BD-039`..`047` | 🟡 `BE-BD-016` **30 deklarasi / baseline 39** | - | Audit source pemilik 14 September 2026: 30 pasangan unik, termasuk `BloodUnit : Allocate`. Verifikasi atribut pasangan, pendaftaran DB, otorisasi biasa, dan rekonsiliasi baseline belum ditutup. `BloodOrder : Update` tetap butir tanpa endpoint v4 yang memerlukan keputusan. [BE-BD-016](../task/report/backend/BE-BD-016.md) bagian 9. **Riwayat:** 29 setelah BE-BD-015, 28 setelah BE-BD-012, 25 setelah BE-BD-004, 20 setelah BE-BD-003. |
| **Pemeriksaan golongan darah** | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026` | ✅ `BE-BD-005` | ⛔ `FE-BD-005` | Backend **terbukti** ([BE-BD-005](../task/report/backend/BE-BD-005.md)). `FE-BD-005` **tetap tertahan** — dependency-nya juga `BE-BD-007`/`BE-BD-008` yang tertahan rantai dependency (`G4` sendiri tertutup 10 September 2026), dan roadmap frontend melarang memecahnya tanpa persetujuan pemilik |
| **Penyelesaian konflik golongan darah** | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` | ✅ `BE-BD-011` | 🟡 `FE-BD-009` | Backend **terbukti** ([BE-BD-011](../task/report/backend/BE-BD-011.md)); `FE-BD-009` kini terbuka |
| Order darah dan pembatalannya | `DEC-BD-004/005/006/044` | ✅ `BE-BD-003` | 🟡 `FE-BD-002` | Backend **terbukti** 11 September 2026 ([BE-BD-003](../task/report/backend/BE-BD-003.md)); `FE-BD-002` kehilangan penahan backend-nya |
| Permintaan PMI dan penerimaan | `DEC-BD-002/003/008/020` | ✅ `BE-BD-004` | ⛔ `FE-BD-003` | Backend **terbukti** — selesai 11 September 2026 pada roadmap revisi 9 ([BE-BD-004](../task/report/backend/BE-BD-004.md)): permintaan, penerimaan termasuk kelebihan, dan kantong `Received`; keenam kriteria yang tetap miliknya terbukti. `FE-BD-003` kehilangan penahan backend-nya. **Riwayat:** selesai sebagian — 6 dari 9 kriteria sebelum penerusan `AC-BD-023/032/033`. **Riwayat:** siap dijadwalkan sejak `BE-BD-003` selesai 11 September 2026; tertahan `G4` sampai 10 September 2026 |
| Penyimpanan dan perpindahan kantong | `DEC-BD-036`, `DEC-BD-037` | ✅ `BE-BD-015` | ⛔ `FE-BD-012` | Backend **terbukti** — selesai 11 September 2026 pada roadmap revisi 10 ([BE-BD-015](../task/report/backend/BE-BD-015.md)): penempatan pertama, perpindahan hanya-tambah, penolakan lokasi nonaktif, penonaktifan tanpa pemindahan, dan gerbang alokasi baca-saja terbukti; kesembilan kriteria yang tetap miliknya terbukti penuh. Verifikasi final bagian alokasi `AC-BD-060/068/070` diteruskan ke `BE-BD-006`. `FE-BD-012` kehilangan penahan backend-nya; statusnya diputuskan pada roadmap frontend. **Riwayat:** selesai sebagian — 9 dari 12 kriteria sebelum penerusan revisi 10. **Riwayat:** siap dijadwalkan sejak roadmap revisi 9; tertahan lewat `BE-BD-004` |
| Alokasi kantong | `DEC-BD-003/007/029` | ✅ `BE-BD-006` | ⛔ `FE-BD-004` | Backend **terbukti 14 September 2026** ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.3) — **kesembilan runtime acceptance dibuktikan lewat panggilan API sungguhan** ditambah verifikasi read-only database: alokasi, pembatalan dua arah (`Available` lawan `PendingReview`), ketiga jalur penolakan alasan, penolakan kantong `Received`/berlebih/di lokasi nonaktif, keberhasilan sesudah dipindah ke lokasi aktif, dan perebutan dua petugas yang menyisakan **tepat satu** alokasi aktif. `FE-BD-004` kehilangan penahan backend-nya. **Riwayat:** PARTIAL — READY FOR RUNTIME VALIDATION per 14 September 2026, runtime acceptance 0 dari 9 `PENDING`. **Riwayat:** build, migration, dan QBE Strict lolos 13 September 2026; penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS` — belum terbukti ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 5.3): build `0 Error(s)`, migration `20260913070556_AddBbkBloodUnitAllocation` scope bersih, tetapi tabelnya belum ada di database karena migration modul lain tertunda. **Riwayat:** dikerjakan 12 September 2026 — source lengkap, belum terbukti ([BE-BD-006](../task/report/backend/BE-BD-006.md)): entity alokasi, index unik terfilter satu alokasi aktif, endpoint `allocate` dan `cancel-allocation`, dan pemakaian gerbang `BE-BD-015`. Build, migration, dan pembuktian sembilan kriteria menunggu pemilik. **Riwayat:** **siap dijadwalkan** sejak roadmap revisi 10, 11 September 2026 — `BE-BD-015` ✅. Memegang verifikasi final bagian alokasi `AC-BD-060/068/070`. **Diperbarui roadmap revisi 11, 12 September 2026:** `AC-BD-071` dilepas ke `BE-BD-009`, sehingga task ini **dapat diselesaikan penuh tanpa membangun `reallocate`**. **Riwayat:** tertahan lewat `BE-BD-015` |
| Bukti kecocokan dan pemberian | `DEC-BD-013/027/028/038/042` | ✅ `BE-BD-007` | ⛔ `FE-BD-005` | **Backend terbukti 16 September 2026** ([BE-BD-007](../task/report/backend/BE-BD-007.md)) — 12/12 business acceptance dan 7/7 kode kontrak terbukti runtime. `FE-BD-005` tetap ⛔ menurut roadmap frontend. **Riwayat:** siap dijadwalkan 14 September 2026, belum ada source; sebelumnya tertahan lewat `BE-BD-006` |
| Pemberian jalur darurat | `DEC-BD-017/038/040` | 🟡 `BE-BD-008` | ⛔ `FE-BD-005` | **Siap dijadwalkan 16 September 2026** — penahannya `BE-BD-007` ✅. Belum ada source. **Riwayat:** tertahan lewat `BE-BD-007` |
| Penyelesaian kantong `PendingReview` | `DEC-BD-019/028/043/045` | 🟡 `BE-BD-009` | ⛔ `FE-BD-007` | **Siap dijadwalkan 16 September 2026** — `BE-BD-007` ✅. **Riwayat:** tertahan lewat `BE-BD-007`; `BE-BD-006` sendiri **✅ sejak 14 September 2026**. **Menerima `AC-BD-071`** pada roadmap revisi 11 karena task ini pemilik endpoint `reallocate` |
| Koreksi pencatatan pemberian | `DEC-BD-030/034/041` | 🟡 `BE-BD-010` | ⛔ `FE-BD-008` | **Siap dijadwalkan 16 September 2026** — penahannya `BE-BD-007` ✅. Belum ada source. **Riwayat:** tertahan lewat `BE-BD-007` |
| Tindakan Bank Darah tercatat | `DEC-BD-021`, `DEC-BD-034`, `DEC-BD-048`, `DEC-BD-049` | ✅ `BE-BD-012` | ⛔ `FE-BD-010` | Backend **terbukti 11 September 2026** ([BE-BD-012](../task/report/backend/BE-BD-012.md)) — tindakan tercatat bernomor, tarif dipilih backend, salinan beku, penyelesaian beraudit, nol jalur Billing. `FE-BD-010` kehilangan penahan backend-nya. **Riwayat:** siap dijadwalkan kembali sejak roadmap revisi 8, 11 September 2026 — aturan tarif, sumber unit/kelas, dan kriterianya sudah diputuskan. **Riwayat:** backend **⛔ 11 September 2026** ([BE-BD-012](../task/report/backend/BE-BD-012.md)): aturan pemilihan tarif, sumber unit dan kelas, dan rumah kedua kriterianya belum diputuskan. Nol source ditulis. **Riwayat:** siap dijadwalkan sejak `BE-BD-003` selesai 11 September 2026. **Riwayat:** tertahan `G4` sampai 10 September 2026 |
| Layar terjangkau dari menu | — | — | 🟡 `FE-BD-006` | **Dikerjakan 10 September 2026** ([laporan](../task/report/frontend/FE-BD-006.md)) — ketiga layar Setup terjangkau tepat sekali dan cocok kontrak; **1 dari 2 acceptance**: visibilitas menurut hak akses belum ada karena `filterMenuItemsByRole` masih stub |
| Penyaluran biaya ke Billing | `DEC-BD-016` **OPEN** | — `BE-BD-013` | — | **Future scope**. Menerima `AC-BD-026` dan `AC-BD-058` dari `BE-BD-012` pada roadmap revisi 8 |

---

## 2. Acceptance criteria ke task

| Rentang AC | Task | Keadaan bukti |
| --- | --- | --- |
| `AC-BD-055`, `AC-BD-056` | ✅ `BE-BD-001` | **Terbukti** — laporan tracked, 56 test lulus |
| `AC-BD-015`, `AC-BD-016` | ✅ `BE-BD-002` | **Terbukti** — 8 test lulus |
| `AC-BD-064` | ✅ `BE-BD-014` | **Terbukti** — 25 test lulus |
| `AC-BD-013` | ✅ `BE-BD-003` | **Terbukti** — `AC_BD_013_UnitTanpaKewenanganMemesanDarah_Ditolak` (elektronik dan manual) dan `Controller_UnitTanpaKewenangan_Menjadi403_VAL_BD_013`. Lulus kembali pada verifikasi ulang 11 September 2026, commit `8e30aa9` ([laporan](../task/report/backend/BE-BD-003.md) bagian 5.1) |
| `AC-BD-030/034/035/077/078` | ✅ `BE-BD-005` | **Terbukti** — 29 test pada `BloodGroupExamServiceTests` + `BloodBankRoleAccessContractTests`. `AC-BD-077/078` terbukti pada tingkat penegakan atribut hak akses; batasnya dicatat di [laporan](../task/report/backend/BE-BD-005.md) bagian 6 |
| `AC-BD-036/037/051/053/054/079/080` | ✅ `BE-BD-011` | **Terbukti** — 9 test penyelesaian konflik. `AC-BD-037` terbukti pada tingkat penegakan atribut hak akses; batasnya dicatat di [laporan](../task/report/backend/BE-BD-011.md) bagian 8 |
| `AC-BD-001/002/003/004/010/011/017/095/096/097` | ✅ `BE-BD-003` | **Terbukti** — 79 test `BloodOrderServiceTests` ditambah 4 uji PostgreSQL; rincian per kriteria di [laporan](../task/report/backend/BE-BD-003.md) bagian 6. Keduanya lulus kembali pada verifikasi ulang 11 September 2026, commit `8e30aa9` — 79/79 dan 4/4 (bagian 5.1). `AC-BD-004` dibuktikan lewat perpindahan order ke `Expired` dan hilangnya penahanan; tafsirannya menunggu konfirmasi pemilik proses |
| `AC-BD-005/006/009/022/031/059` | ✅ `BE-BD-004` | **Terbukti 11 September 2026** — 63 test `ProviderRequestServiceTests` ditambah 5 uji PostgreSQL; rincian per kriteria di [laporan](../task/report/backend/BE-BD-004.md) bagian 6. `AC-BD-022` dibuktikan lewat service; pemicu otomatisnya belum ada |
| `AC-BD-023`, `AC-BD-032` | ✅ `BE-BD-015` | **Terbukti 11 September 2026** — kantong disimpan lalu masuk `PendingReview` beserta sebabnya, asal dan riwayatnya utuh, dan muncul di daftar kerja #2 hanya sesudah disimpan ([laporan](../task/report/backend/BE-BD-015.md) bagian 7). **Riwayat:** sebagian — diteruskan dari `BE-BD-004` pada roadmap revisi 9. **Sudah terbukti di `BE-BD-004`:** penerimaan dicatat, kantong berlebih ditandai `IsExcess` beserta alasan "Kiriman melebihi permintaan.", dan kantong susulan sesudah `ClosedEncounter` membawa rujukan asal. **Sisa yang dibuktikan `BE-BD-015`:** perpindahan kantong ke `PendingReview` sesudah disimpan dan kemunculannya di daftar kantong `PendingReview`. **Riwayat:** milik 🟡 `BE-BD-004` sampai revisi 8 |
| `AC-BD-033` | ✅ `BE-BD-006` | **Terbukti 14 September 2026** — kantong berlebih `TEST-BD006-…-07` (`IsExcess`, `Menunggu keputusan`, lokasi aktif) dicoba dialokasikan lewat `POST /{id}/allocate` → **`422`** "Kantong ini menunggu keputusan dan tidak dapat langsung dialokasikan." ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.3). **Riwayat:** belum terbukti — runtime `PENDING`. **Riwayat:** belum terbukti — `BLOCKED`. Build dan migration lolos 13 September 2026, tetapi penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS`, sehingga endpoint belum dapat dipanggil sungguhan ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** Belum terbukti — `NOT EXECUTED`. Penolakan `VAL-BD-033` sudah ada di `AllocateAsync` sejak 12 September 2026, belum dipanggil sungguhan ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** diteruskan dari `BE-BD-004` pada roadmap revisi 9; `BE-BD-006` siap dijadwalkan sejak revisi 10. Penolakan `VAL-BD-033` hidup pada endpoint alokasi `BE-BD-006`. **Riwayat:** milik `BE-BD-004` sampai revisi 8, yang belum punya satu jalur pun untuk mengalokasikan kantong. **Riwayat baris gabungan:** belum diuji — task siap dijadwalkan sejak 11 September 2026; tertahan `G4` sampai 10 September 2026 |
| `AC-BD-061/063/066/067/069` | ✅ `BE-BD-015` | **Terbukti 11 September 2026** — 43 test `BloodUnitStorageServiceTests` ditambah 6 uji PostgreSQL; rincian per kriteria di [laporan](../task/report/backend/BE-BD-015.md) bagian 7. **Riwayat baris gabungan:** belum diuji — task siap dijadwalkan sejak roadmap revisi 9; sebelumnya tertahan |
| `AC-BD-060/068/070` | ✅ `BE-BD-006` | **Terbukti penuh 14 September 2026** — verifikasi final bagian alokasi selesai lewat endpoint `allocate` ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.3): `AC-BD-060` kantong `Received` → **`422 VAL-BD-063`**; `AC-BD-068` kantong di `TBD006-LOC1` yang dinonaktifkan → **`422 VAL-BD-064`**; `AC-BD-070` kantong yang sama dipindahkan ke lokasi aktif → alokasi **`200`**, gerbang terbuka kembali tanpa satu pun kantong disunting. Bukti tingkat gerbang milik `BE-BD-015` dipertahankan. **Riwayat:** sebagian — bagian alokasinya runtime `PENDING`. **Riwayat:** bagian alokasinya `BLOCKED`. Build dan migration lolos 13 September 2026, tetapi penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS` ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** Sebagian, dan bagian alokasinya masih `NOT EXECUTED`. Sejak 12 September 2026 endpoint `allocate` ada dan memanggil gerbang `EvaluateAllocationGateAsync`, tetapi belum pernah dipanggil sungguhan ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** verifikasi final bagian alokasi diteruskan dari `BE-BD-015` pada roadmap revisi 10. **Sudah terbukti di `BE-BD-015` (tingkat gerbang), dipertahankan:** kantong `Received` tertutup gerbang `VAL-BD-063`; kantong di lokasi nonaktif tertutup `VAL-BD-064`; perpindahan dari lokasi nonaktif ke aktif menambah riwayat penempatan beserta pelaku dan waktu, lalu gerbang terbuka kembali ([laporan](../task/report/backend/BE-BD-015.md) bagian 7). **Sisa yang dibuktikan `BE-BD-006`:** lewat endpoint `allocate` — kantong `Received` ditolak (`060`), kantong di lokasi nonaktif ditolak (`068`), dan kantong yang sudah dipindahkan ke lokasi aktif berhasil dialokasikan (`070`). `060`/`068` sudah tercantum pada `BE-BD-006` sebelum revisi 10 — terlihat sejak arsip revisi 3 — sehingga tidak diduplikasi; `070` ditambahkan pada revisi 10. **Riwayat:** milik 🟡 `BE-BD-015` sampai revisi 9, terbukti pada tingkat gerbang |
| `AC-BD-062`, `AC-BD-065` | ✅ `BE-BD-015` | **Terbukti 11 September 2026** — lokasi nonaktif ditolak `VAL-BD-060` untuk penyimpanan baru. Diteruskan dari `BE-BD-014` |
| `AC-BD-043/044/045/046` | ✅ `BE-BD-006` | **Terbukti 14 September 2026** ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.3). `AC-BD-043` pembatalan saat order aktif → kantong kembali **`Tersedia`**, baris alokasi tersimpan sebagai `Cancelled` beserta kode dan salinan teks alasan. `AC-BD-044` order asal `ORD-00000082` dibatalkan lebih dulu → pembatalan alokasi membawa kantong ke **`Menunggu keputusan`**, bukan `Tersedia`. `AC-BD-045` tiga jalur: kode kosong **`400`**, kode tidak dikenal **`400`** dengan pesan kanonis, kategori salah **`422`** — catatan keseragaman envelope di bagian 8.2. `AC-BD-046` kantong berstatus `Issued` → **`422 VAL-BD-023`**, dan database membuktikan penolakan itu **tidak menggerakkan apa pun**; fixture `Issued`-nya dibuat atas dua kali persetujuan eksplisit pemilik. **Riwayat:** belum terbukti — runtime `PENDING`. **Riwayat:** belum terbukti — `BLOCKED`. Build dan migration lolos 13 September 2026, tetapi penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS`, sehingga endpoint belum dapat dipanggil sungguhan ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** Belum terbukti — `NOT EXECUTED`. Source-nya ada sejak 12 September 2026 (`CancelAllocationAsync`, `IsAllocationOriginActiveAsync`), tetapi build, migration, dan panggilan API belum dijalankan ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 6). **Riwayat:** belum diuji — task siap dijadwalkan sejak roadmap revisi 10, dan dapat diselesaikan penuh secara mandiri sejak roadmap revisi 11. **Riwayat baris gabungan:** `AC-BD-043/044/045/046/071` milik `BE-BD-006` sampai revisi 10; `AC-BD-071` dilepas ke `BE-BD-009` pada revisi 11. **Riwayat:** tertahan |
| `AC-BD-071` | ⛔ `BE-BD-009` | **Belum diuji** — dilepas dari `BE-BD-006` ke `BE-BD-009` pada roadmap revisi 11, 12 September 2026. Alasannya endpoint, bukan selera: `AC-BD-071` menguji **pengalihan** kantong `PendingReview`, yaitu `POST /blood-units/{id}/reallocate` dengan hak akses `BloodUnit : ResolveReallocate` ([api-contract](../contracts/api-contract.md)), dan endpoint itu lahir di `BE-BD-009` — bukan di `BE-BD-006`, yang hanya melahirkan `allocate` dan `cancel-allocation`. **Sudah terbukti dan dipertahankan:** gerbang `EvaluateAllocationGateAsync` menolak kantong di lokasi nonaktif dengan `VAL-BD-064` ([BE-BD-015](../task/report/backend/BE-BD-015.md) bagian 7). **Sisa yang dibuktikan `BE-BD-009`:** endpoint `reallocate` memanggil gerbang yang sama, sesuai kontrak `v4` [02-backend-architecture.md](../02-backend-architecture.md) §F.4. **Riwayat:** milik `BE-BD-006` sampai revisi 10 |
| `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` | ✅ `BE-BD-007` | **Kedua belas terbukti runtime 16 September 2026** ([laporan](../task/report/backend/BE-BD-007.md) bagian 4) lewat panggilan API sungguhan ditambah verifikasi database. `AC-BD-090`/`091` dibuktikan dengan aktor non-SuperAdmin sungguhan, bukan SuperAdmin. **Riwayat:** belum diuji tetapi tidak lagi tertahan sejak `BE-BD-006` ✅ 14 September 2026; sebelumnya tertahan |
| `AC-BD-020/021/074/075/081/082/083/084/085` | ⛔ `BE-BD-008` | Tertahan |
| `AC-BD-007/008/024/025/029/092/093/094` | ⛔ `BE-BD-009` | Tertahan lewat `BE-BD-007`. **Menerima `AC-BD-071`** pada roadmap revisi 11 — barisnya tersendiri di atas |
| `AC-BD-047/048/049/050/086/087/088` | ⛔ `BE-BD-010` | Tertahan |
| `AC-BD-098/099/100/101/102` | ✅ `BE-BD-012` | **Terbukti 11 September 2026** — 39 test `BloodBankProcedureServiceTests` ditambah 4 uji PostgreSQL; rincian per kriteria di [laporan](../task/report/backend/BE-BD-012.md) bagian 6. **Riwayat:** belum diuji — kriteria baru roadmap revisi 8, dasar `DEC-BD-048`/`049`. Skenario ujinya di `testing/acceptance-test-matrix.md` §10 |
| `AC-BD-026/058` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka. Dipindah dari `BE-BD-012` pada roadmap revisi 8. **Riwayat baris `BE-BD-012`:** tidak dapat diuji pada `BE-BD-012` — keduanya menuntut fakta biaya ke Billing (`DEC-BD-016` `OPEN`, milik `BE-BD-013`) dan pemberian/koreksi (`BE-BD-007`, `BE-BD-010`). Matriks acceptance sendiri menandai `AC-BD-026` tertunda `DEC-BD-016` ([laporan](../task/report/backend/BE-BD-012.md) bagian 6). **Riwayat:** belum diuji — task siap dijadwalkan sejak 11 September 2026. **Riwayat:** tertahan `G4` sampai 10 September 2026 |
| `AC-BD-027` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka |

**Diperbarui 14 September 2026 sesudah `BE-BD-006`: 56 dari 102 terbukti.** Delapan kriteria bertambah
terbukti — `AC-BD-033`, `AC-BD-043`, `AC-BD-044`, `AC-BD-045`, `AC-BD-046`, `AC-BD-060`, `AC-BD-068`,
dan `AC-BD-070` — seluruhnya lewat panggilan API sungguhan ditambah verifikasi read-only database, bukan
lewat test otomatis. **3 tidak dapat diuji** karena `DEC-BD-016` terbuka, dan **43 sisanya belum
terbukti penuh**. `AC-BD-071` tetap milik ⛔ `BE-BD-009` dan tetap belum terbukti. Bentuk buktinya
mengikuti kebijakan verifikasi [backend-roadmap.md](backend-roadmap.md) bagian 0.1; **angka test pada
laporan task yang sudah ✅ SELESAI tetap dipertahankan apa adanya.**

**Riwayat — diperbarui 12 September 2026, roadmap revisi 11: hitungan tetap 48 dari 102.** Koreksi tata kelola
saja. `AC-BD-071` hanya berpindah pemilik, dari 🟡 `BE-BD-006` ke ⛔ `BE-BD-009`; ia **belum terbukti
sebelum maupun sesudah** perpindahan, sehingga nol kriteria bertambah terbukti dan nol kehilangan
bukti. **3 tidak dapat diuji** dan **51 belum terbukti penuh**, sama seperti sebelumnya. Yang berubah
secara nyata hanya satu hal: `BE-BD-006` kini dapat mencapai ✅ **penuh** tanpa `reallocate`, karena
kriteria yang menuntut endpoint itu sudah tidak lagi miliknya. **Bentuk bukti yang sah** untuk task
sesudah ini mengikuti kebijakan verifikasi pada [backend-roadmap.md](backend-roadmap.md) bagian 0.1 —
bukti build produksi, inspeksi EF/skema, QBE, dan verifikasi manual API/DB terkendali; **angka test
pada laporan task yang sudah ✅ SELESAI dipertahankan apa adanya dan tidak ditulis ulang.**

**Riwayat — diperbarui 11 September 2026, roadmap revisi 10: hitungan tetap 48 dari 102.** Penerusan hanya
memindahkan pemegang verifikasi final `AC-BD-060`, `AC-BD-068`, dan `AC-BD-070` ke `BE-BD-006`; tidak ada
kriteria yang bertambah terbukti maupun hilang buktinya. Ketiganya tetap **belum dihitung terbukti** —
bagian gerbangnya terbukti di `BE-BD-015`, bagian alokasinya menunggu `BE-BD-006`. **3 tidak dapat diuji**
dan **51 belum terbukti penuh**, sama seperti sebelumnya.

**Riwayat — hitungan bukti per 11 September 2026 sesudah `BE-BD-015`: 48 dari 102 terbukti.** Sembilan kriteria
`BE-BD-015` — `AC-BD-023/032/061/062/063/065/066/067/069` — terbukti penuh, menambah 39 yang sudah ada.
`AC-BD-060`, `AC-BD-068`, dan `AC-BD-070` **terbukti pada tingkat gerbang** dan belum dihitung. **3 tidak
dapat diuji** karena `DEC-BD-016` terbuka, dan **51 sisanya belum terbukti penuh**.

**Riwayat — diperbarui 11 September 2026, roadmap revisi 9: hitungan tetap 39 dari 102.** Penerusan hanya
memindahkan pemilik `AC-BD-023`, `AC-BD-032`, dan `AC-BD-033`; tidak ada kriteria yang bertambah
terbukti maupun hilang buktinya. `AC-BD-023`/`032` tetap terbukti sebagian — kini ditutup `BE-BD-015` —
dan `AC-BD-033` tetap belum, kini milik `BE-BD-006`.

**Hitungan bukti per 11 September 2026 sesudah `BE-BD-012`: 39 dari 102 terbukti.** Kelima kriteria `BE-BD-012` — `AC-BD-098` sampai `AC-BD-102` — terbukti, menambah 34 yang sudah ada. **3 tidak dapat diuji** karena `DEC-BD-016` terbuka — `AC-BD-026`, `AC-BD-027`, `AC-BD-058`, ketiganya milik `BE-BD-013` — dan **60 sisanya belum terbukti penuh**.

**Riwayat — hitungan bukti per 11 September 2026, roadmap revisi 8: 34 dari 102 terbukti.** Lima kriteria baru `AC-BD-098` sampai `AC-BD-102` milik `BE-BD-012` belum diuji. **3 tidak dapat diuji** karena `DEC-BD-016` terbuka — `AC-BD-026`, `AC-BD-027`, `AC-BD-058`, ketiganya milik `BE-BD-013` — dan **65 sisanya belum terbukti penuh**.

**Riwayat — hitungan bukti per 11 September 2026 sesudah `BE-BD-004`: 34 dari 97 terbukti.** Enam kriteria `BE-BD-004` — `AC-BD-005/006/009/022/031/059` — terbukti, menambah 28 yang sudah ada. `AC-BD-023` dan `AC-BD-032` **terbukti sebagian** dan belum dihitung; `AC-BD-033` belum. **1 tidak dapat diuji** (`AC-BD-027`), dan **62 sisanya belum terbukti penuh**.

**Riwayat — hitungan bukti per 11 September 2026 sebelum `BE-BD-004`: 28 dari 97 terbukti.** Kesebelas acceptance criteria `BE-BD-003` — `AC-BD-001/002/003/004/010/011/013/017/095/096/097` — terbukti, menambah 17 yang sudah ada. **1 tidak dapat diuji** (`AC-BD-027`), dan **68 sisanya belum diuji** — seluruhnya milik task sesudah `BE-BD-003`.

**Riwayat — hitungan bukti 10 September 2026.** Dari 97 acceptance criteria, **17 sudah terbukti** — 5 dari gelombang master
(`AC-BD-015/016/055/056/064`) ditambah 12 dari pemeriksaan golongan darah
(`AC-BD-030/034/035/036/037/051/053/054/077/078/079/080`) pada 9 September 2026. **1 tidak dapat
diuji** karena keputusan terbuka (`AC-BD-027`), dan **79 sisanya belum diuji**.

**Diperbarui 10 September 2026:** `G4` tertutup, sehingga **11** dari ketujuh puluh sembilan —
`AC-BD-001/002/003/004/010/011/013/017/095/096/097` — kini milik task yang siap dijadwalkan, yaitu
`BE-BD-003`. **68** sisanya menunggu rantai dependency sesudahnya. **Riwayat:** sampai 10 September
2026 ketujuh puluh sembilan seluruhnya menunggu `G4`.

Tiga dari ketujuh belas — `AC-BD-037`, `AC-BD-077`, `AC-BD-078` — terbukti pada tingkat **penegakan
atribut hak akses**, bukan pada percobaan panggilan ujung-ke-ujung oleh pengguna berperan berbeda.
Pembuktian penuhnya menuntut migration dijalankan dan dua akun ber-hak-akses berbeda, dan **tidak
boleh** memakai SuperAdmin karena `HasAccessAsync` meloloskannya sebelum satu baris hak akses dibaca.

---

## 3. Coverage gap yang diakui

| Gap | Keadaan | Akibat |
| --- | --- | --- |
| **Lampiran bukti primer sesi penutupan BE-BD-006** | Status selesai mengikuti laporan Claude yang diteruskan pemilik. Adendum 9 dipulihkan dari ringkasan; raw request/response, output SQL, dan log terbaru tidak ada di ZIP | Review ini tidak dapat mengonfirmasi eksekusi secara independen. Lampirkan keluaran asli sebelum mengklaim audit primer lengkap; bukan alasan membuat log pengganti atau menandai skenario gagal |
| ~~**Registrasi DI tiga service master Bank Darah hilang**~~ — **DITEMUKAN DAN DITUTUP 14 September 2026** | `BloodComponentService`, `BloodStorageLocationService`, dan `BloodBankReasonService` tidak terdaftar di `Program.cs`, sehingga controller master terdampak kegagalan aktivasi DI; sembilan endpoint/request smoke dilaporkan menjawab **`500`**, tanpa daftar method/path pada paket review. Riwayat blob membuktikan ketiganya **pernah terdaftar** sejak 3 September 2026 dan hilang bersamaan pada merge `27d737cd` (11 September 2026) — sisi `sukmagp` memilikinya, sisi `QuilvianIntegrationBackend` tidak, dan hasil merge mengambil versi tanpa registrasi. **Regresi cabang berjalan, bukan cacat sejak awal** ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.4) | **Tidak lagi menahan.** Ketiganya dipulihkan (+8 baris `Program.cs`); smoke validation dilaporkan lolos pada sembilan endpoint/request (`200`); bukan klaim seluruh endpoint tiga controller diuji. `BE-BD-001` dan `BE-BD-014` sempat turun ke 🟡 lalu kembali ✅ pada hari yang sama. **Pada akhir sesi Claude, perubahan dilaporkan belum ter-commit.** Status Git kini harus dibaca dari workstation; pemulihan clone/deployment lain memerlukan perubahan tersedia dan digunakan di lingkungan tersebut |
| **Envelope `VAL-BD-016` tidak seragam** — **BARU 14 September 2026** | Kode alasan kosong ditolak `[Required]` pada DTO **sebelum** service dipanggil, sehingga balasannya `ProblemDetails` mentah tanpa `message`; kode alasan tidak dikenal ditolak service dengan `ApiResponse` berisi pesan kanonis. Dua bentuk balasan untuk satu kode aturan yang sama ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6) | **Tidak menahan task** — `AC-BD-045` tetap terbukti, penolakannya konsisten `400`. Perlu keputusan pemilik kontrak; merapikannya menyentuh seluruh DTO Bank Darah yang memakai `[Required]` |
| **`HeldUnitCount` bernilai `0` pada penonaktifan lokasi** — **BARU 14 September 2026** | Diuji terkendali: lokasi aktif berisi 1 kantong → `heldUnitCount` **1** (benar); lokasi dinonaktifkan → pada **balasan yang sama** pesan menyebut "Ada 1 kantong" sementara `heldUnitCount` **0**. Milik `BE-BD-015` ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6) | **Menahan satu acceptance `FE-BD-011`** — angka kantong tertahan pada balasan penonaktifan terbukti salah menurut skenario yang dilaporkan. **Belum diperbaiki**; di luar scope `BE-BD-006` |
| **Folder `Tests/` ada dan mematahkan `dotnet build` polos** — **BARU 14 September 2026** | `backend-roadmap.md` bagian 0.1 menyatakan folder `Tests/` di root "Tidak ada" dan "DILARANG". Kenyataannya folder itu **ada**, berisi sisa `obj/` lima project test yang sudah dihapus — nol `.cs` source, nol `.csproj`, di-ignore `.gitignore:378`, 0 berkas ter-track. `dotnet build` polos **gagal 26 error** (`CS0579`, `CS0400`) karena csproj root menyapu `**/*.cs` ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6) | **Tidak menahan task** — build lolos memakai `-p:DefaultItemExcludes="Tests/**"`. **Nol berkas dihapus.** Catatan keadaan workstation sudah ditambahkan pada bagian 0.1 tanpa menghapus observasi historis; cleanup sisa `Tests/obj` serta pembuktian build polos masih terpisah dan belum dilakukan dalam review ini |
| ~~**Aturan pemilihan tarif tindakan Bank Darah**~~ — **DITUTUP 11 September 2026 oleh `DEC-BD-049`**, roadmap revisi 8 | Backend memilih tarif memakai predikat kecocokan `InsuranceCoverageService`; tarif tanpa kelas sebagai cadangan; tanpa kandidat ditolak `422` | **Tidak lagi menahan.** Rincian `00-interview-decisions.md` §8.28. **Riwayat baris:** — |
| **Riwayat — aturan pemilihan tarif tindakan Bank Darah** — **BARU 11 September 2026** | Kamus data hanya menulis `TariffId` "Tarif dirujuk"; `BD-CAP-008` hanya pola salinan. Source memuat dua aturan yang bertentangan: Laboratorium memilih tarif terbaru per tindakan tanpa melihat kelas, sedangkan `InsuranceCoverageService` memilih yang paling spesifik terhadap unit, klinik, dan kelas kunjungan. Ditemukan builder `BE-BD-012` | **Menahan `BE-BD-012`.** Menentukan tarif = kebijakan harga, milik Billing (`DEC-BD-021`). Perlu keputusan pemilik Billing bersama pemilik proses BDRS |
| ~~**Sumber `ServiceUnitId` dan `PatientClassId` tindakan**~~ — **DITUTUP 11 September 2026 oleh `DEC-BD-048`** | Keduanya diambil dari kunjungan order; client tidak mengirimnya. **Riwayat:** kamus data hanya menulis "Unit" dan "Kelas" | **Tidak lagi menahan** |
| ~~**`AC-BD-026`/`058` tidak dapat dibuktikan pada `BE-BD-012`**~~ — **DITUTUP 11 September 2026**, roadmap revisi 8 | Keduanya dipindah ke `BE-BD-013`; `BE-BD-012` memakai `AC-BD-098` sampai `AC-BD-102`. **Riwayat:** keduanya menuntut fakta biaya ke Billing serta pemberian dan koreksi | **Tidak lagi menahan** |
| ~~**Pesan penolakan "tarif tidak ditemukan" tanpa kode `VAL-BD-*`**~~ — **DITUTUP 11 September 2026 oleh `BE-BD-012`** | Kode **`VAL-BD-084`** `422` dicatat pada `validation-matrix.md` §5 sebagai delta kontrak, melanjutkan `VAL-BD-083`. **Riwayat:** `DEC-BD-049` menolak pencatatan bila tidak ada tarif yang cocok, tetapi matriks validasi `v4` belum punya kode dan kalimatnya | **Tidak lagi menahan** |
| ~~**Scope riwayat untuk tindakan**~~ — **DITUTUP 11 September 2026 oleh `BE-BD-012`** | Nilai kelima `BloodBankProcedure` dicatat pada `data-dictionary.md` §`BbkTransitionHistory`, tanpa perubahan schema. **Riwayat:** kamus data mendaftar empat nilai `Scope` tanpa tindakan | **Tidak lagi menahan** |
| **Kunjungan tanpa kelas pasien pada tindakan** — **BARU 11 September 2026** | `BE-BD-012` menolak `422` karena kamus data mewajibkan `PatientClassId`. Kalimat `DEC-BD-049` "hanya tarif tanpa kelas yang cocok" dapat dibaca sebagai "tetap boleh dicatat" ([laporan](../task/report/backend/BE-BD-012.md) bagian 7 nomor 4) | **Tidak menahan task** — tidak ada kriteria yang memuat keadaan ini. Perlu konfirmasi pemilik; bila dikehendaki, satu migration aditif menjadikan kolomnya boleh kosong |
| **Tafsiran "order sah" untuk tindakan** — **BARU 11 September 2026** | `BE-BD-012` menerima order yang ada, tidak dihapus, dan tidak ber-flag batal, tanpa membatasi status bisnisnya — tindakan bisa sudah dikerjakan sebelum order berhenti (`VAL-BD-026`) | **Tidak menahan task.** Perlu konfirmasi pemilik proses BDRS |
| ~~**Tiga kriteria `BE-BD-015` menuntut endpoint alokasi**~~ — **DITUTUP 11 September 2026**, roadmap revisi 10 | `Sukmagp` meneruskan verifikasi final bagian alokasi `AC-BD-060`, `068`, `070` ke `BE-BD-006`; `060`/`068` sudah tercantum di sana, `070` ditambahkan. Bukti gerbang `BE-BD-015` dipertahankan. **Riwayat:** ketiganya berbunyi "dicoba dialokasikan". `BE-BD-015` membuktikan gerbangnya — `VAL-BD-063/064`, tertutup lalu terbuka kembali — tetapi endpoint `allocate` milik `BE-BD-006`. Ditemukan builder `BE-BD-015` ([laporan](../task/report/backend/BE-BD-015.md) bagian 9) | **Tidak lagi menahan.** `BE-BD-015` ✅; `BE-BD-006` siap dijadwalkan. **Riwayat:** menahan `BE-BD-015` di 🟡 dan, lewat dependency, `BE-BD-006` |
| ~~**Tiga kriteria `BE-BD-004` menunjuk state task lain**~~ — **DITUTUP 11 September 2026**, roadmap revisi 9 | `Sukmagp` meneruskan `AC-BD-023`/`032` ke `BE-BD-015` dan `AC-BD-033` ke `BE-BD-006`. **Riwayat:** `AC-BD-023`/`032` menuntut kantong `PendingReview`, yang menurut matriks §3 `v2` lahir sesudah penyimpanan (`BE-BD-015`); `AC-BD-033` menuntut endpoint alokasi (`BE-BD-006`). Ditemukan builder `BE-BD-004` | **Tidak lagi menahan.** `BE-BD-004` ✅; `BE-BD-015` siap dijadwalkan. **Riwayat:** menahan `BE-BD-004` di 🟡 dan, lewat dependency, `BE-BD-015` |
| **Kantong yang datang sesudah permintaan `Fulfilled`** — **BARU 11 September 2026** | Matriks §2 menyebut `Fulfilled` terminal; hanya `ClosedEncounter` yang menerima kantong susulan. `BE-BD-004` karena itu menolak penerimaan pada permintaan `Fulfilled` | **Tidak menahan task.** Dapat bertentangan dengan semangat `DEC-BD-025` bila PMI mengirim kantong tambahan dalam kiriman terpisah. Perlu keputusan pemilik proses BDRS |
| **Kategori alasan pembatalan permintaan PMI** — **BARU 11 September 2026** | Kontrak menuntut alasan terkendali tanpa menetapkan kategori | **Tidak menahan task.** `BE-BD-004` menerima alasan aktif mana pun, sama seperti `BE-BD-011` |
| **Pemicu `ClosedEncounter` otomatis** — **BARU 11 September 2026** | Perpindahannya ada di `BbkProviderRequestService.CloseForEncounterEndAsync` dan terbukti (`AC-BD-022`), tetapi pemicu otomatisnya belum ada — sama dengan kedaluwarsa order | Satu task pemicu dapat melayani keduanya |
| **`AC-BD-014` tanpa task pemilik** — **BARU 11 September 2026** | "Permintaan tanpa jumlah kantong" (`VAL-BD-007`) tidak tercantum pada kartu task backend mana pun. `BE-BD-004` menegakkan `VAL-BD-007` pada order tanpa jumlah yang dapat diminta, karena jumlahnya diturunkan dari baris order | Pemilik roadmap memutuskan rumahnya |
| **`BloodOrder : Update` tanpa endpoint** — **BARU 11 September 2026** | Matriks hak akses menyebut butir ini sebagai pasangan yang dipisahkan dari `Cancel` (`DEC-BD-044`), tetapi api-contract `v4` tidak punya endpoint suntingan order. Ditemukan builder `BE-BD-003` | **Tidak menahan task.** Butir tidak didaftarkan; contract test menandainya `TANPA-ENDPOINT-V4`. Keputusan pemilik kontrak diperlukan: buat endpoint suntingan beserta aturannya, atau cabut butir dari matriks |
| **Pemicu kedaluwarsa order otomatis** — **BARU 11 September 2026** | Perpindahan `Active` → `Expired` ada di `BbkBloodOrderService.ExpireAsync` dan terbukti (`AC-BD-004/017`), tetapi pemicu otomatis yang membaca sinyal kunjungan tidak termasuk scope `BE-BD-003` dan belum ada | Order pada kunjungan yang sudah berakhir tetap tercatat `Active` sampai pemicunya ada. **Penahanan ganda tidak terdampak**, karena order baru pada kunjungan yang berakhir sudah ditolak. Perlu task tersendiri |
| **Tafsiran "kunjungan sah"** — **BARU 11 September 2026** | `BE-BD-003` menolak order baru pada kunjungan yang sudah berakhir, sebagai turunan `DEC-BD-006`, `ASM-BD-002`, dan pesan `VAL-BD-004` | Perlu konfirmasi pemilik proses BDRS |
| ~~**Provider number-series**~~ (`BD-DEP-017` / `G4`) | ✅ **TERTUTUP 10 September 2026.** `NumberSeriesAllocator` berdiri lewat `PLT-BE-003` pada 9 September 2026, dan durabilitasnya terbukti lewat `PLT-BE-004` pada 10 September 2026 — 6 dari 6 lulus di PostgreSQL. Pemiliknya, `Andry`, menyatakan gerbang tertutup. **Riwayat:** **Berubah sifat 9 September 2026.** `OQ-PLT-007` ✅ tertutup — pemiliknya `Andry`; `DEC-PLT-002`..`005`, `007`, `008` ✅ `approved`; blueprint `PLT-SLICE-01` ✅ ada sebagai `DRAFT`. **Yang tersisa:** `OQ-PLT-012`/`OQ-PLT-013` (Area dan prefix registry Platform) masih terbuka dan memblokir perencanaan Platform, roadmap Platform belum ada, dan providernya nol baris kode | **Tidak lagi menahan.** Acceptance criteria yang tersisa kini menunggu task Bank Darah sendiri, bukan gerbang. **Riwayat:** **79 acceptance criteria belum dapat diuji.** Tetap gap terbesar modul ini — tetapi kini **dependency pengiriman yang dapat dijadwalkan**, bukan penghalang organisasi. Menunggu gelombang `MVP-1` Platform |
| **Asal `SampleIdentifier`** — **DITUTUP 9 September 2026** | Builder `BE-BD-005` menemukan pertentangan nyata: `03-domain-architecture.md:283` menulis *"terbitan sistem"*, sedangkan `03-frontend-architecture.md:218` dan `00-interview-decisions.md:214` memperlakukannya sebagai isian petugas. Dilaporkan sebelum kode ditulis; **pemilik memutuskan `SampleIdentifier` ditulis petugas** | Gap tertutup. `BE-BD-005` **tidak** terkena `G4`. **Sisa pekerjaan dokumentasi:** frasa `BD-DOM-10` pada `03-domain-architecture.md` perlu dikoreksi agar tidak menyesatkan pembaca berikutnya |
| **Kategori alasan penyelesaian konflik golongan darah** — **BARU** | `BbkBloodGroupConflictResolution.ReasonCode` wajib dan merujuk `MstBloodBankReason`, tetapi kontrak `v4` tidak menetapkan kategori mana yang berlaku. Kesepuluh kategori yang ada seluruhnya menyangkut order dan kantong | **Tidak menahan task.** `BE-BD-011` menuntut alasan yang ada dan aktif tanpa memaksakan kategori. Keputusan pemilik proses diperlukan bila kategori khusus dikehendaki |
| **Visibilitas menu menurut hak akses** — **BARU 10 September 2026** | Frontend **tidak punya katalog permission** pengguna yang sedang login. `filterMenuItemsByRole` adalah **stub**: Admin dan Manajer dikembalikan menu utuh, sedangkan untuk peran lain seluruh logika filternya dikomentari, sehingga fungsinya memulangkan daftar yang sama persis. `AccessDeniedGate` hanya reaktif — ia menampilkan pesan setelah backend memulangkan `403`. Nol slice dan nol endpoint menyediakan hak akses pengguna berjalan. Ditemukan builder `FE-BD-006`; bukti pada [laporannya](../task/report/frontend/FE-BD-006.md) §1.2 | **Menahan satu acceptance `FE-BD-006`** — butir menu tetap tampil bagi pengguna yang tidak berhak, dan penolakan baru terjadi ketika layarnya dibuka. **Data tidak bocor**, karena penolakannya ditegakkan backend; yang rusak adalah pengalamannya. **Bukan pekerjaan Bank Darah**: menutupnya menuntut sumber permission pengguna, pemetaan tiap butir menu ke `Resource : Action`, lalu penyaringan — kemampuan lintas modul. Catatan tambahan: stub itu menanam nama peran di kode (`Admin`, `Manajer`, dan pada blok terkomentari `Perawat`, `Dokter`), sehingga perbaikannya sebaiknya membaca hak akses yang diberikan, bukan nama peran |
| Penyaluran biaya Billing | `DEC-BD-016` `OPEN DECISION` | `AC-BD-027` tidak dapat diuji; `BE-BD-013` di luar gelombang mana pun |
| Jam masa berlaku bukti per komponen | `OQ-BD-012` | **Tidak** menahan task; nilainya dari konfigurasi master. Selama kosong, gerbang menolak |
| Keadaan kantong setelah dikoreksi | `OQ-BD-014` | Menahan detail implementasi `BE-BD-010`, bukan bentuknya |
| Rumah slice resmi `BR-BD-020` | Penilaian kelengkapan requirement masih revisi 2 | Tidak menahan task; diperlakukan sebagai perluasan `BD-SLICE-03/04/10` |
| Dua master baru tanpa baris `BD-CAP-*` | `MstBloodStorageLocation`, `MstBloodBankReason` | Audit penuh peta kemampuan disarankan sebelum `MVP-2` |
| Rujukan pola Laboratorium bergeser | `BD-CAP-008` kini `LabSpecimen.cs` + `LabExamination.cs`; `BD-CAP-009` kini `LabTransitionHistory.cs` | Sudah diperbarui pada peta kemampuan revisi 5. Builder `BE-BD-003`/`004` wajib memakai nama baru |

---

## 4. Keputusan yang masih terbuka dan pengaruhnya

| ID | Ringkasan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| ~~`OQ-PLT-007`~~ | ~~Backend Engineering Contract Owner belum ditunjuk~~ ✅ **Tertutup 9 September 2026 — `Andry`** | Tidak lagi memblokir | — |
| ~~`OQ-PLT-012`~~ | ~~Area registry modul Platform~~ ✅ **Tertutup 9 Sep 2026** → `DEC-PLT-009`: Area `Platform` baru | Tidak lagi memblokir | — |
| ~~`OQ-PLT-013`~~ | ~~Prefix entity modul Platform~~ ✅ **Tertutup 9 Sep 2026** → `DEC-PLT-010`: prefix `Num` | Tidak lagi memblokir | — |
| ~~**Approval blueprint `PLT-SLICE-01`**~~ | ✅ **Turun 9 September 2026** — kontrak `v1` `approved` oleh `Sukma Giri Pratama`. Semula blueprint Platform masih `DRAFT` | Tidak lagi memblokir; `G4` tertutup 10 September 2026 | — |
| ~~`OQ-PLT-014`~~ | ~~Baris registry `Platform`/`Num`~~ ✅ **Tertutup 9 Sep 2026** — dicatat dan `ACTIVE` | Tidak lagi memblokir; `PLT-BE-002` terbuka | — |
| **Baru** | Kategori alasan penyelesaian konflik golongan darah belum ditetapkan kontrak `v4` | **Tidak memblokir** — `BE-BD-011` menerima alasan aktif mana pun | Pemilik proses BDRS |
| `DEC-BD-016` | Persetujuan pemilik Billing atas konteks sumber biaya | `BE-BD-013` future scope | Pemilik BillingManagement |
| `OQ-BD-011` | Mekanik label golongan darah | Slice label | Pemilik proses klinis |
| `DEF-BD-003` | Apakah semua komponen menuntut bukti kecocokan sama | Aturan per komponen saat implementasi | Pemilik proses klinis |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong | Kegunaan `RETURNED_TO_PROVIDER` | Pemilik proses BDRS |
| `OQ-BD-012` | Jam masa berlaku bukti kecocokan per komponen | Implementasi gerbang pemberian | Pemilik proses klinis |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru setelah dikoreksi | Implementasi jalur koreksi | Pemilik proses BDRS |
| `OQ-BD-016` | Apakah bukti pendukung koreksi menuntut lampiran | Bentuk kolom bukti pendukung | Pemilik proses BDRS |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada | Penelusuran bukti ke kebutuhan | Pemilik kebutuhan |

**Nol keputusan terbuka yang memblokir task per 10 September 2026.** **Riwayat:** sebelumnya hanya `OQ-PLT-007` yang memblokir task. Delapan lainnya menyangkut scope di luar rilis pertama,
detail implementasi yang nilainya datang dari konfigurasi, atau satu baris seeder — dipertahankan
supaya tidak hilang, bukan sebagai penahan.

---

## 5. Ringkasan hitungan

| Dimensi | Angka |
| --- | ---: |
| Task backend seluruhnya | 16 |
| — ✅ selesai | 10 — per 14 September 2026: `BE-BD-006` bergabung sesudah kesembilan runtime acceptance-nya terbukti. `BE-BD-001` dan `BE-BD-014` sempat turun ke 🟡 pada hari yang sama karena regresi DI, lalu kembali ✅. **Riwayat:** 9 per 11 September 2026, roadmap revisi 10: `BE-BD-015` bergabung. **Riwayat:** 8 sesudah revisi 9, ketika `BE-BD-004` bergabung; 7 sesudah `BE-BD-012`; sebelumnya 6; sebelumnya 5 |
| — 🟡 selesai sebagian | 1 — `BE-BD-016`. **Riwayat:** 2 — `BE-BD-016`, `BE-BD-015` sebelum revisi 10; 1 — `BE-BD-016` sesudah revisi 9; sebelumnya 2 — `BE-BD-016`, `BE-BD-004`; sebelumnya 1 |
| — 🟡 pending, siap dijadwalkan | 3 — `BE-BD-008`, `BE-BD-009`, `BE-BD-010`, ketiganya terbuka setelah `BE-BD-007` ✅ 16 September 2026. **Riwayat:** 1 — `BE-BD-007`, penahannya `BE-BD-006` gugur 14 September 2026. **Riwayat:** 1 — `BE-BD-006` **PARTIAL — READY FOR RUNTIME VALIDATION** per 14 September 2026, runtime acceptance 0 dari 9 `PENDING`, kini ✅. **Riwayat:** 🟡 pending, siap dijadwalkan — `BE-BD-006` sejak roadmap revisi 10; 0 sesudah `BE-BD-015` 🟡; 1 — `BE-BD-015`, sejak roadmap revisi 9; sebelumnya 0 sesudah `BE-BD-012` ✅; sebelumnya 1 — `BE-BD-012` sejak roadmap revisi 8; sebelumnya 0 sesudah `BE-BD-012` ⛔; sebelumnya `BE-BD-012`; sebelumnya `BE-BD-003` |
| — ⛔ blocked | 0 — tidak ada, sejak `BE-BD-007` ✅ 16 September 2026. **Riwayat:** 3 — `BE-BD-008`..`010`, seluruhnya lewat `BE-BD-007`. **Riwayat:** 4 — `BE-BD-007`..`010`, sampai `BE-BD-006` ✅ 14 September 2026; 5 termasuk `BE-BD-006`; sebelumnya 6 termasuk `BE-BD-015`; sebelumnya 7 termasuk `BE-BD-012`; sebelumnya 6; sebelumnya 8 |
| — future scope | 1 |
| Task frontend seluruhnya | 12 |
| — ✅ selesai | 1 |
| — 🟡 pending, siap dijadwalkan | 3 |
| — ⛔ blocked | 8 |
| **Total task** | **28** (27 dalam gelombang + 1 future scope) |
| **Dapat dijadwalkan hari ini** | Backend: **`BE-BD-007`** per 14 September 2026 — `BE-BD-006` ✅ menutup penahannya, dan jalur kritisnya berlanjut `BE-BD-007` → `BE-BD-008`/`009`/`010`. `BE-BD-008` sampai `BE-BD-010` tetap ⛔ lewat `BE-BD-007`. **Riwayat:** backend **nol task baru** per 14 September 2026 — melanjutkan validasi runtime `BE-BD-006` 🟡. Frontend: `FE-BD-002` dan `FE-BD-009`; `FE-BD-003`, `FE-BD-010`, dan kini `FE-BD-012` kehilangan penahan backend-nya, roadmap frontend masih `DRAFT` dan tidak disentuh pass ini. **Riwayat:** backend `BE-BD-006` sejak roadmap revisi 10, 11 September 2026. **Riwayat:** backend **nihil** sesudah `BE-BD-015` 🟡 — `BE-BD-006` menunggu `BE-BD-015` ✅, yang menunggu keputusan penerusan bagian alokasi `AC-BD-060/068/070`. **Riwayat:** backend `BE-BD-015` sejak roadmap revisi 9, 11 September 2026. **Riwayat:** backend nihil sesudah `BE-BD-012` — `BE-BD-015` menunggu keputusan penerusan tiga kriteria `BE-BD-004`. **Riwayat:** **2** — `BE-BD-003` (backend, terbuka sejak `G4` tertutup 10 September 2026) dan `FE-BD-009` (frontend). `FE-BD-011` dan `FE-BD-006` sudah dikerjakan 10 September 2026, keduanya berakhir 🟡 sebagian |
| Acceptance criteria seluruhnya | 102 — ditambah `AC-BD-098` sampai `AC-BD-102` pada roadmap revisi 8. **Riwayat:** 97 |
| — terbukti | **56** — per 14 September 2026 sesudah `BE-BD-006`. Delapan kriteria bertambah: `AC-BD-033`, `AC-BD-043`, `AC-BD-044`, `AC-BD-045`, `AC-BD-046`, `AC-BD-060`, `AC-BD-068`, `AC-BD-070`. **Riwayat:** 48 per 11 September 2026 sesudah `BE-BD-015`. **Riwayat:** 39 sesudah `BE-BD-012`; 34 sesudah `BE-BD-004`; sebelumnya 17 |
| — tidak dapat diuji (keputusan terbuka) | 3 — `AC-BD-026/027/058`, seluruhnya milik `BE-BD-013`. **Riwayat:** 1 |
| — belum terbukti penuh | **43** — per 14 September 2026; `AC-BD-060/068/070` **keluar dari daftar ini** karena verifikasi final alokasinya sudah terbukti di `BE-BD-006`. **Riwayat:** 51 — termasuk `AC-BD-060/068/070`, yang bagian gerbangnya terbukti di `BE-BD-015` dan verifikasi final alokasinya milik `BE-BD-006` sejak roadmap revisi 10. **Riwayat:** 60 sesudah `BE-BD-012`, termasuk `AC-BD-023`/`032` yang terbukti sebagian; 65 sebelum `BE-BD-012`; sebelumnya 62; sebelumnya 79 |
| Keputusan bisnis | `DEC-BD-001`..`047` |
| Gerbang tertutup | `G1`, `G2a`, `G2b`, **`G4`** — yang terakhir 10 September 2026 |
| Gerbang terbuka | **Nol** |

**Penelusuran utuh.** Setiap kebutuhan yang sudah diputuskan punya task pemilik, dan setiap task punya
acceptance criteria. **Nol requirement yatim** — dipastikan ulang pada perencanaan 9 September 2026,
dan itulah sebabnya revisi 5 tidak menambah satu task pun.

Yang kurang bukan penelusurannya, melainkan **bukti** — dan sebagian besar bukti itu menunggu satu
gerbang yang kini **sudah punya pemilik dan sudah punya blueprint**, tetapi belum punya kodenya.
Perbedaan itu penting: sebelum 9 September 2026 tidak ada yang dapat dijadwalkan untuk menutup `G4`;
sekarang ada, yaitu gelombang `MVP-1` blueprint Platform.

**Diperbarui 10 September 2026.** Gelombang `MVP-1` Platform selesai dan `G4` tertutup. Bukti yang
tersisa kini menunggu **task Bank Darah sendiri**, dimulai `BE-BD-003` — bukan lagi menunggu modul lain.
