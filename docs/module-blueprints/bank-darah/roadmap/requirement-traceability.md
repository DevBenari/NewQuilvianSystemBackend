# Requirement Traceability — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 8
revision_5_scope: CROSS_MODULE_DEPENDENCY_AND_STATUS_ONLY
revision_6_scope: BLOCKER_REFRESH_ONLY
revision_7_scope: PLATFORM_DEPENDENCY_CONCRETE
revision_8_scope: ACCEPTANCE_REHOME_BE_BD_012
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

Dokumen ini menjawab satu pertanyaan: **apakah setiap kebutuhan yang sudah diputuskan punya task yang
mengerjakannya, dan punya cara membuktikan bahwa ia benar-benar dikerjakan.**

Ia **tidak** menilai kesiapan modul, **tidak** menyetujui apa pun, dan **tidak** menggantikan
`verify-module-readiness`. Kalau sebuah baris di sini menunjuk task yang belum jalan, itu berarti
penelusurannya utuh tetapi buktinya belum ada — dua hal yang berbeda.

---

## 1. Kebutuhan bisnis ke task

| Kebutuhan | Keputusan | Task backend | Task frontend | Keadaan |
| --- | --- | --- | --- | --- |
| Katalog komponen darah terkendali | `DEC-BD-024`, `DEC-BD-032` | ✅ `BE-BD-001` | ✅ `FE-BD-001` | Backend & frontend **terbukti** ([FE-BD-001](../task/report/frontend/FE-BD-001.md)) |
| Daftar alasan berkategori | `DEC-BD-044`, `DEC-BD-024` | ✅ `BE-BD-001` | ✅ `FE-BD-001` | Backend & frontend **terbukti** ([FE-BD-001](../task/report/frontend/FE-BD-001.md)) |
| Kewenangan unit memesan darah dari konfigurasi | `DEC-BD-012` | ✅ `BE-BD-002` | — | Penegakan **terbukti** di ✅ `BE-BD-003` — `VAL-BD-013`, [laporan](../task/report/backend/BE-BD-003.md) |
| Lokasi penyimpanan darah dikelola | `DEC-BD-035`, `DEC-BD-037` | ✅ `BE-BD-014` | 🟡 `FE-BD-011` | Backend **terbukti**. Frontend **dikerjakan 10 September 2026** ([laporan](../task/report/frontend/FE-BD-011.md)) — layar `FE-BD-10` berdiri, build lulus, tetapi **1 dari 2 acceptance**: `FE-BD-015` menunggu angka kantong tertahan dari `BE-BD-015` |
| Hak akses per tindakan | `DEC-BD-039`..`047` | 🟡 `BE-BD-016` 20/39 | — | Sisa lahir bersama controller pemakainya; `BloodOrder : Update` tidak punya endpoint kontrak `v4` |
| **Pemeriksaan golongan darah** | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026` | ✅ `BE-BD-005` | ⛔ `FE-BD-005` | Backend **terbukti** ([BE-BD-005](../task/report/backend/BE-BD-005.md)). `FE-BD-005` **tetap tertahan** — dependency-nya juga `BE-BD-007`/`BE-BD-008` yang tertahan rantai dependency (`G4` sendiri tertutup 10 September 2026), dan roadmap frontend melarang memecahnya tanpa persetujuan pemilik |
| **Penyelesaian konflik golongan darah** | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` | ✅ `BE-BD-011` | 🟡 `FE-BD-009` | Backend **terbukti** ([BE-BD-011](../task/report/backend/BE-BD-011.md)); `FE-BD-009` kini terbuka |
| Order darah dan pembatalannya | `DEC-BD-004/005/006/044` | ✅ `BE-BD-003` | 🟡 `FE-BD-002` | Backend **terbukti** 11 September 2026 ([BE-BD-003](../task/report/backend/BE-BD-003.md)); `FE-BD-002` kehilangan penahan backend-nya |
| Permintaan PMI dan penerimaan | `DEC-BD-002/003/008/020` | 🟡 `BE-BD-004` | ⛔ `FE-BD-003` | Backend **selesai sebagian 11 September 2026** ([BE-BD-004](../task/report/backend/BE-BD-004.md)): permintaan, penerimaan termasuk kelebihan, dan kantong `Received` terbukti; 6 dari 9 kriteria. **Riwayat:** siap dijadwalkan sejak `BE-BD-003` selesai 11 September 2026; tertahan `G4` sampai 10 September 2026 |
| Penyimpanan dan perpindahan kantong | `DEC-BD-036`, `DEC-BD-037` | ⛔ `BE-BD-015` | ⛔ `FE-BD-012` | Tertahan lewat `BE-BD-004` |
| Alokasi kantong | `DEC-BD-003/007/029` | ⛔ `BE-BD-006` | ⛔ `FE-BD-004` | Tertahan lewat `BE-BD-015` |
| Bukti kecocokan dan pemberian | `DEC-BD-013/027/028/038/042` | ⛔ `BE-BD-007` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-006` |
| Pemberian jalur darurat | `DEC-BD-017/038/040` | ⛔ `BE-BD-008` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-007` |
| Penyelesaian kantong `PendingReview` | `DEC-BD-019/028/043/045` | ⛔ `BE-BD-009` | ⛔ `FE-BD-007` | Tertahan lewat `BE-BD-006/007` |
| Koreksi pencatatan pemberian | `DEC-BD-030/034/041` | ⛔ `BE-BD-010` | ⛔ `FE-BD-008` | Tertahan lewat `BE-BD-007` |
| Tindakan Bank Darah tercatat | `DEC-BD-021`, `DEC-BD-034`, `DEC-BD-048`, `DEC-BD-049` | 🟡 `BE-BD-012` | ⛔ `FE-BD-010` | Backend **siap dijadwalkan kembali** sejak roadmap revisi 8, 11 September 2026 — aturan tarif, sumber unit/kelas, dan kriterianya sudah diputuskan. **Riwayat:** backend **⛔ 11 September 2026** ([BE-BD-012](../task/report/backend/BE-BD-012.md)): aturan pemilihan tarif, sumber unit dan kelas, dan rumah kedua kriterianya belum diputuskan. Nol source ditulis. **Riwayat:** siap dijadwalkan sejak `BE-BD-003` selesai 11 September 2026. **Riwayat:** tertahan `G4` sampai 10 September 2026 |
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
| `AC-BD-023`, `AC-BD-032` | 🟡 `BE-BD-004` | **Sebagian** — penerimaan dicatat, kantong berlebih ditandai `IsExcess` beserta alasan "Kiriman melebihi permintaan.", dan kantong susulan sesudah `ClosedEncounter` membawa rujukan asal. Perpindahan kantong ke `PendingReview` terjadi sesudah disimpan, milik `BE-BD-015` |
| `AC-BD-033` | 🟡 `BE-BD-004` | **Belum** — penolakan `VAL-BD-033` hidup pada endpoint alokasi `BE-BD-006`. Pada `BE-BD-004` belum ada satu jalur pun yang dapat mengalokasikan kantong. **Riwayat baris gabungan:** belum diuji — task siap dijadwalkan sejak 11 September 2026; tertahan `G4` sampai 10 September 2026 |
| `AC-BD-060/061/063/066/067/068/069/070` | ⛔ `BE-BD-015` | Tertahan |
| `AC-BD-062`, `AC-BD-065` | ⛔ `BE-BD-015` | Diteruskan dari `BE-BD-014` |
| `AC-BD-043/044/045/046/071` | ⛔ `BE-BD-006` | Tertahan |
| `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` | ⛔ `BE-BD-007` | Tertahan |
| `AC-BD-020/021/074/075/081/082/083/084/085` | ⛔ `BE-BD-008` | Tertahan |
| `AC-BD-007/008/024/025/029/092/093/094` | ⛔ `BE-BD-009` | Tertahan |
| `AC-BD-047/048/049/050/086/087/088` | ⛔ `BE-BD-010` | Tertahan |
| `AC-BD-098/099/100/101/102` | 🟡 `BE-BD-012` | Belum diuji — kriteria baru roadmap revisi 8, dasar `DEC-BD-048`/`049`; task siap dijadwalkan. Skenario ujinya di `testing/acceptance-test-matrix.md` §10 |
| `AC-BD-026/058` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka. Dipindah dari `BE-BD-012` pada roadmap revisi 8. **Riwayat baris `BE-BD-012`:** tidak dapat diuji pada `BE-BD-012` — keduanya menuntut fakta biaya ke Billing (`DEC-BD-016` `OPEN`, milik `BE-BD-013`) dan pemberian/koreksi (`BE-BD-007`, `BE-BD-010`). Matriks acceptance sendiri menandai `AC-BD-026` tertunda `DEC-BD-016` ([laporan](../task/report/backend/BE-BD-012.md) bagian 6). **Riwayat:** belum diuji — task siap dijadwalkan sejak 11 September 2026. **Riwayat:** tertahan `G4` sampai 10 September 2026 |
| `AC-BD-027` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka |

**Hitungan bukti per 11 September 2026, roadmap revisi 8: 34 dari 102 terbukti.** Lima kriteria baru `AC-BD-098` sampai `AC-BD-102` milik `BE-BD-012` belum diuji. **3 tidak dapat diuji** karena `DEC-BD-016` terbuka — `AC-BD-026`, `AC-BD-027`, `AC-BD-058`, ketiganya milik `BE-BD-013` — dan **65 sisanya belum terbukti penuh**.

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
| ~~**Aturan pemilihan tarif tindakan Bank Darah**~~ — **DITUTUP 11 September 2026 oleh `DEC-BD-049`**, roadmap revisi 8 | Backend memilih tarif memakai predikat kecocokan `InsuranceCoverageService`; tarif tanpa kelas sebagai cadangan; tanpa kandidat ditolak `422` | **Tidak lagi menahan.** Rincian `00-interview-decisions.md` §8.28. **Riwayat baris:** — |
| **Riwayat — aturan pemilihan tarif tindakan Bank Darah** — **BARU 11 September 2026** | Kamus data hanya menulis `TariffId` "Tarif dirujuk"; `BD-CAP-008` hanya pola salinan. Source memuat dua aturan yang bertentangan: Laboratorium memilih tarif terbaru per tindakan tanpa melihat kelas, sedangkan `InsuranceCoverageService` memilih yang paling spesifik terhadap unit, klinik, dan kelas kunjungan. Ditemukan builder `BE-BD-012` | **Menahan `BE-BD-012`.** Menentukan tarif = kebijakan harga, milik Billing (`DEC-BD-021`). Perlu keputusan pemilik Billing bersama pemilik proses BDRS |
| ~~**Sumber `ServiceUnitId` dan `PatientClassId` tindakan**~~ — **DITUTUP 11 September 2026 oleh `DEC-BD-048`** | Keduanya diambil dari kunjungan order; client tidak mengirimnya. **Riwayat:** kamus data hanya menulis "Unit" dan "Kelas" | **Tidak lagi menahan** |
| ~~**`AC-BD-026`/`058` tidak dapat dibuktikan pada `BE-BD-012`**~~ — **DITUTUP 11 September 2026**, roadmap revisi 8 | Keduanya dipindah ke `BE-BD-013`; `BE-BD-012` memakai `AC-BD-098` sampai `AC-BD-102`. **Riwayat:** keduanya menuntut fakta biaya ke Billing serta pemberian dan koreksi | **Tidak lagi menahan** |
| **Pesan penolakan "tarif tidak ditemukan" tanpa kode `VAL-BD-*`** — **BARU 11 September 2026** | `DEC-BD-049` menolak pencatatan bila tidak ada tarif yang cocok, tetapi matriks validasi `v4` belum punya kode dan kalimatnya | **Tidak menahan task.** Builder memakai pesan tanpa kode dan mencatatnya sebagai delta, seperti nomor kantong ganda pada `BE-BD-004`. Kode resminya dapat ditambahkan `design-business-module` |
| **Scope riwayat untuk tindakan** — **BARU 11 September 2026** | `AC-BD-101` menuntut transisi dan audit penyelesaian tersimpan, sedangkan kamus data mendaftar empat nilai `BbkTransitionHistory.Scope` tanpa tindakan | **Tidak menahan task.** Builder dapat menambah nilai `Scope` tindakan — kolomnya `string(30)`, tanpa perubahan schema — dan mencatatnya sebagai delta kontrak |
| **Tiga kriteria `BE-BD-004` menunjuk state task lain** — **BARU 11 September 2026** | `AC-BD-023`/`032` menuntut kantong `PendingReview`, yang menurut matriks §3 `v2` lahir sesudah penyimpanan (`BE-BD-015`); `AC-BD-033` menuntut endpoint alokasi (`BE-BD-006`). Ditemukan builder `BE-BD-004` | **Menahan `BE-BD-004` di 🟡 dan, lewat dependency, `BE-BD-015`.** Keputusan pemilik roadmap: teruskan ketiganya ke task tempat penegakannya hidup, mengikuti preseden `BE-BD-002` → `BE-BD-003` dan `BE-BD-014` → `BE-BD-015` |
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
| — ✅ selesai | 6 — per 11 September 2026. **Riwayat:** 5 |
| — 🟡 selesai sebagian | 2 — `BE-BD-016`, `BE-BD-004`. **Riwayat:** 1 |
| — 🟡 pending, siap dijadwalkan | 1 — `BE-BD-012`, sejak roadmap revisi 8. **Riwayat:** 0 sesudah `BE-BD-012` ⛔; sebelumnya `BE-BD-012`; sebelumnya `BE-BD-003` |
| — ⛔ blocked | 6 — `BE-BD-006`..`010`, `015`. **Riwayat:** 7 termasuk `BE-BD-012`; sebelumnya 6; sebelumnya 8 |
| — future scope | 1 |
| Task frontend seluruhnya | 12 |
| — ✅ selesai | 1 |
| — 🟡 pending, siap dijadwalkan | 3 |
| — ⛔ blocked | 8 |
| **Total task** | **28** (27 dalam gelombang + 1 future scope) |
| **Dapat dijadwalkan hari ini** | **2** — `BE-BD-003` (backend, terbuka sejak `G4` tertutup 10 September 2026) dan `FE-BD-009` (frontend). `FE-BD-011` dan `FE-BD-006` sudah dikerjakan 10 September 2026, keduanya berakhir 🟡 sebagian |
| Acceptance criteria seluruhnya | 102 — ditambah `AC-BD-098` sampai `AC-BD-102` pada roadmap revisi 8. **Riwayat:** 97 |
| — terbukti | 34 — per 11 September 2026 sesudah `BE-BD-004`. **Riwayat:** 17 |
| — tidak dapat diuji (keputusan terbuka) | 1 |
| — belum terbukti penuh | 65 — termasuk `AC-BD-023`/`032` yang terbukti sebagian dan lima kriteria baru `BE-BD-012`. Tidak dapat diuji kini **3** — `AC-BD-026/027/058`, seluruhnya milik `BE-BD-013`. **Riwayat:** 62; sebelumnya 79 |
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
