# Requirement Traceability — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 7
revision_5_scope: CROSS_MODULE_DEPENDENCY_AND_STATUS_ONLY
revision_6_scope: BLOCKER_REFRESH_ONLY
revision_7_scope: PLATFORM_DEPENDENCY_CONCRETE
status: FORWARD-TEST / DRAFT
contract_version: v4 (approved)
backend_source_sha: 55ac6ab
backend_source_sha_note: >-
  Rentang 55ac6ab..f0d6855 mengubah nol berkas .cs. PERINGATAN: working tree memuat 17
  berkas .cs milik BE-BD-005 dan BE-BD-011 yang belum ter-commit.
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
decision_revision: 11
acceptance_criteria_range: AC-BD-001 .. AC-BD-097
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
| Kewenangan unit memesan darah dari konfigurasi | `DEC-BD-012` | ✅ `BE-BD-002` | — | Penegakan diteruskan ke ⛔ `BE-BD-003` |
| Lokasi penyimpanan darah dikelola | `DEC-BD-035`, `DEC-BD-037` | ✅ `BE-BD-014` | 🟡 `FE-BD-011` | Backend **terbukti**. Frontend **dikerjakan 10 September 2026** ([laporan](../task/report/frontend/FE-BD-011.md)) — layar `FE-BD-10` berdiri, build lulus, tetapi **1 dari 2 acceptance**: `FE-BD-015` menunggu angka kantong tertahan dari `BE-BD-015` |
| Hak akses per tindakan | `DEC-BD-039`..`047` | 🟡 `BE-BD-016` 17/39 | — | Sisa lahir bersama controller pemakainya |
| **Pemeriksaan golongan darah** | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026` | ✅ `BE-BD-005` | ⛔ `FE-BD-005` | Backend **terbukti** ([BE-BD-005](../task/report/backend/BE-BD-005.md)). `FE-BD-005` **tetap tertahan** — dependency-nya juga `BE-BD-007`/`BE-BD-008` yang tertahan `G4`, dan roadmap frontend melarang memecahnya tanpa persetujuan pemilik |
| **Penyelesaian konflik golongan darah** | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` | ✅ `BE-BD-011` | 🟡 `FE-BD-009` | Backend **terbukti** ([BE-BD-011](../task/report/backend/BE-BD-011.md)); `FE-BD-009` kini terbuka |
| Order darah dan pembatalannya | `DEC-BD-004/005/006/044` | ⛔ `BE-BD-003` | ⛔ `FE-BD-002` | Tertahan `G4` |
| Permintaan PMI dan penerimaan | `DEC-BD-002/003/008/020` | ⛔ `BE-BD-004` | ⛔ `FE-BD-003` | Tertahan `G4` |
| Penyimpanan dan perpindahan kantong | `DEC-BD-036`, `DEC-BD-037` | ⛔ `BE-BD-015` | ⛔ `FE-BD-012` | Tertahan lewat `BE-BD-004` |
| Alokasi kantong | `DEC-BD-003/007/029` | ⛔ `BE-BD-006` | ⛔ `FE-BD-004` | Tertahan lewat `BE-BD-015` |
| Bukti kecocokan dan pemberian | `DEC-BD-013/027/028/038/042` | ⛔ `BE-BD-007` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-006` |
| Pemberian jalur darurat | `DEC-BD-017/038/040` | ⛔ `BE-BD-008` | ⛔ `FE-BD-005` | Tertahan lewat `BE-BD-007` |
| Penyelesaian kantong `PendingReview` | `DEC-BD-019/028/043/045` | ⛔ `BE-BD-009` | ⛔ `FE-BD-007` | Tertahan lewat `BE-BD-006/007` |
| Koreksi pencatatan pemberian | `DEC-BD-030/034/041` | ⛔ `BE-BD-010` | ⛔ `FE-BD-008` | Tertahan lewat `BE-BD-007` |
| Tindakan Bank Darah tercatat | `DEC-BD-021`, `DEC-BD-034` | ⛔ `BE-BD-012` | ⛔ `FE-BD-010` | Tertahan `G4` |
| Layar terjangkau dari menu | — | — | 🟡 `FE-BD-006` | Siap |
| Penyaluran biaya ke Billing | `DEC-BD-016` **OPEN** | — `BE-BD-013` | — | **Future scope** |

---

## 2. Acceptance criteria ke task

| Rentang AC | Task | Keadaan bukti |
| --- | --- | --- |
| `AC-BD-055`, `AC-BD-056` | ✅ `BE-BD-001` | **Terbukti** — laporan tracked, 56 test lulus |
| `AC-BD-015`, `AC-BD-016` | ✅ `BE-BD-002` | **Terbukti** — 8 test lulus |
| `AC-BD-064` | ✅ `BE-BD-014` | **Terbukti** — 25 test lulus |
| `AC-BD-013` | ⛔ `BE-BD-003` | Diteruskan dari `BE-BD-002`; menunggu jalur order |
| `AC-BD-030/034/035/077/078` | ✅ `BE-BD-005` | **Terbukti** — 29 test pada `BloodGroupExamServiceTests` + `BloodBankRoleAccessContractTests`. `AC-BD-077/078` terbukti pada tingkat penegakan atribut hak akses; batasnya dicatat di [laporan](../task/report/backend/BE-BD-005.md) bagian 6 |
| `AC-BD-036/037/051/053/054/079/080` | ✅ `BE-BD-011` | **Terbukti** — 9 test penyelesaian konflik. `AC-BD-037` terbukti pada tingkat penegakan atribut hak akses; batasnya dicatat di [laporan](../task/report/backend/BE-BD-011.md) bagian 8 |
| `AC-BD-001/002/003/004/010/011/017/095/096/097` | ⛔ `BE-BD-003` | Tertahan `G4` |
| `AC-BD-005/006/009/022/023/031/032/033/059` | ⛔ `BE-BD-004` | Tertahan `G4` |
| `AC-BD-060/061/063/066/067/068/069/070` | ⛔ `BE-BD-015` | Tertahan |
| `AC-BD-062`, `AC-BD-065` | ⛔ `BE-BD-015` | Diteruskan dari `BE-BD-014` |
| `AC-BD-043/044/045/046/071` | ⛔ `BE-BD-006` | Tertahan |
| `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` | ⛔ `BE-BD-007` | Tertahan |
| `AC-BD-020/021/074/075/081/082/083/084/085` | ⛔ `BE-BD-008` | Tertahan |
| `AC-BD-007/008/024/025/029/092/093/094` | ⛔ `BE-BD-009` | Tertahan |
| `AC-BD-047/048/049/050/086/087/088` | ⛔ `BE-BD-010` | Tertahan |
| `AC-BD-026/058` | ⛔ `BE-BD-012` | Tertahan `G4` |
| `AC-BD-027` | — `BE-BD-013` | **Tidak dapat diuji** — `DEC-BD-016` terbuka |

**Hitungan bukti.** Dari 97 acceptance criteria, **17 sudah terbukti** — 5 dari gelombang master
(`AC-BD-015/016/055/056/064`) ditambah 12 dari pemeriksaan golongan darah
(`AC-BD-030/034/035/036/037/051/053/054/077/078/079/080`) pada 9 September 2026. **1 tidak dapat
diuji** karena keputusan terbuka (`AC-BD-027`), dan **79 sisanya belum diuji, seluruhnya menunggu
`G4`** — tidak ada lagi acceptance criteria yang menunggu task yang sudah dapat dijadwalkan.

Tiga dari ketujuh belas — `AC-BD-037`, `AC-BD-077`, `AC-BD-078` — terbukti pada tingkat **penegakan
atribut hak akses**, bukan pada percobaan panggilan ujung-ke-ujung oleh pengguna berperan berbeda.
Pembuktian penuhnya menuntut migration dijalankan dan dua akun ber-hak-akses berbeda, dan **tidak
boleh** memakai SuperAdmin karena `HasAccessAsync` meloloskannya sebelum satu baris hak akses dibaca.

---

## 3. Coverage gap yang diakui

| Gap | Keadaan | Akibat |
| --- | --- | --- |
| **Provider number-series** (`BD-DEP-017` / `G4`) | **Berubah sifat 9 September 2026.** `OQ-PLT-007` ✅ tertutup — pemiliknya `Andry`; `DEC-PLT-002`..`005`, `007`, `008` ✅ `approved`; blueprint `PLT-SLICE-01` ✅ ada sebagai `DRAFT`. **Yang tersisa:** `OQ-PLT-012`/`OQ-PLT-013` (Area dan prefix registry Platform) masih terbuka dan memblokir perencanaan Platform, roadmap Platform belum ada, dan providernya nol baris kode | **79 acceptance criteria belum dapat diuji.** Tetap gap terbesar modul ini — tetapi kini **dependency pengiriman yang dapat dijadwalkan**, bukan penghalang organisasi. Menunggu gelombang `MVP-1` Platform |
| **Asal `SampleIdentifier`** — **DITUTUP 9 September 2026** | Builder `BE-BD-005` menemukan pertentangan nyata: `03-domain-architecture.md:283` menulis *"terbitan sistem"*, sedangkan `03-frontend-architecture.md:218` dan `00-interview-decisions.md:214` memperlakukannya sebagai isian petugas. Dilaporkan sebelum kode ditulis; **pemilik memutuskan `SampleIdentifier` ditulis petugas** | Gap tertutup. `BE-BD-005` **tidak** terkena `G4`. **Sisa pekerjaan dokumentasi:** frasa `BD-DOM-10` pada `03-domain-architecture.md` perlu dikoreksi agar tidak menyesatkan pembaca berikutnya |
| **Kategori alasan penyelesaian konflik golongan darah** — **BARU** | `BbkBloodGroupConflictResolution.ReasonCode` wajib dan merujuk `MstBloodBankReason`, tetapi kontrak `v4` tidak menetapkan kategori mana yang berlaku. Kesepuluh kategori yang ada seluruhnya menyangkut order dan kantong | **Tidak menahan task.** `BE-BD-011` menuntut alasan yang ada dan aktif tanpa memaksakan kategori. Keputusan pemilik proses diperlukan bila kategori khusus dikehendaki |
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
| **Approval blueprint `PLT-SLICE-01`** | Blueprint Platform masih `DRAFT` dengan `contract_version: v1 (draft)` dan `approved_by: []` | **`PLT-SLICE-01`** → lalu `G4` → 9 task backend, 8 task frontend. **Inilah penahan terdekat sekarang** | Pemilik kontrak engineering backend — `Andry` |
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

**Hanya `OQ-PLT-007` yang memblokir task.** Delapan lainnya menyangkut scope di luar rilis pertama,
detail implementasi yang nilainya datang dari konfigurasi, atau satu baris seeder — dipertahankan
supaya tidak hilang, bukan sebagai penahan.

---

## 5. Ringkasan hitungan

| Dimensi | Angka |
| --- | ---: |
| Task backend seluruhnya | 16 |
| — ✅ selesai | 5 |
| — 🟡 selesai sebagian | 1 |
| — 🟡 pending, siap dijadwalkan | 0 |
| — ⛔ blocked | 9 |
| — future scope | 1 |
| Task frontend seluruhnya | 12 |
| — ✅ selesai | 1 |
| — 🟡 pending, siap dijadwalkan | 3 |
| — ⛔ blocked | 8 |
| **Total task** | **28** (27 dalam gelombang + 1 future scope) |
| **Dapat dijadwalkan hari ini** | **2** — `FE-BD-006`, `FE-BD-009`. **Seluruhnya frontend**; nol task backend dapat dijadwalkan tanpa menutup `G4`. `FE-BD-011` sudah dikerjakan 10 September 2026 dan berakhir 🟡 sebagian |
| Acceptance criteria seluruhnya | 97 |
| — terbukti | 17 |
| — tidak dapat diuji (keputusan terbuka) | 1 |
| — belum diuji | 79 |
| Keputusan bisnis | `DEC-BD-001`..`047` |
| Gerbang tertutup | `G1`, `G2a`, `G2b` |
| Gerbang terbuka | **`G4`** |

**Penelusuran utuh.** Setiap kebutuhan yang sudah diputuskan punya task pemilik, dan setiap task punya
acceptance criteria. **Nol requirement yatim** — dipastikan ulang pada perencanaan 9 September 2026,
dan itulah sebabnya revisi 5 tidak menambah satu task pun.

Yang kurang bukan penelusurannya, melainkan **bukti** — dan sebagian besar bukti itu menunggu satu
gerbang yang kini **sudah punya pemilik dan sudah punya blueprint**, tetapi belum punya kodenya.
Perbedaan itu penting: sebelum 9 September 2026 tidak ada yang dapat dijadwalkan untuk menutup `G4`;
sekarang ada, yaitu gelombang `MVP-1` blueprint Platform.
