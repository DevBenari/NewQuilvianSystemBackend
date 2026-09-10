# Roadmap Delivery Frontend — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
module_name: BloodBankManagement
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
blueprint_root: docs/module-blueprints/bank-darah/
roadmap_revision: 7
revision_4_scope: SPLIT_BE_FE_ONLY
revision_5_scope: CROSS_MODULE_DEPENDENCY_AND_STATUS_ONLY
revision_5_note: >-
  Nol task frontend ditambah, dihapus, atau diubah cakupannya. Yang berubah hanya dua:
  FE-BD-009 naik dari BLOCKED menjadi PENDING karena BE-BD-011 selesai, dan penulisan
  ulang gerbang G4 menjadi dependency lintas modul ke PLT-SLICE-01.
revision_6_scope: BLOCKER_REFRESH_ONLY
revision_7_scope: PLATFORM_DEPENDENCY_CONCRETE
revision_6_note: >-
  Nol task frontend berubah. Penahan G4 di sisi Platform disegarkan mengikuti
  backend-roadmap.md bagian 6.1.
status: FORWARD-TEST / DRAFT
approval_gate: BLUEPRINT_APPROVED
contract_version: v4 (approved)
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
frontend_branch: sukmagpV2
backend_source_sha: 55ac6ab
backend_branch: sukmagp
decision_revision: 11
owners:
  - "Product/Domain: pemilik proses BDRS"
  - "Frontend: pemilik proses BDRS"
approved_by:
  - "Sukmagp — set kontrak v4 dan roadmap revisi 2, 2026-09-03"
approved_at: "2026-09-03"
approval_note: >-
  Approval 2026-09-03 berlaku atas roadmap revisi 2. Revisi 3 menambahkan gerbang G4
  dan revisi 4 memecah roadmap. Approval TIDAK berpindah otomatis.
supersedes: roadmap/archive/revision-3/00-delivery-plan.md
```

---

## 0. Peringatan yang tidak boleh dilewati

**Tidak ada task frontend yang boleh mendahului task backend pasangannya**, walaupun kontrak API sudah
`approved` dan terkunci pada `v4`. Kontrak yang disetujui membuka **penjadwalan**, bukan izin membangun
layar di atas endpoint yang belum ada.

**Rupa layar bersifat `DEV_DISCRETION`.** Roadmap ini mengunci **sumber data, hak akses, dan keadaan
layar yang wajib ada** — bukan warna, tata letak, atau pilihan modal versus drawer. Jangan menetapkan
keputusan produk dari dokumen ini.

**Frontend adalah konsumen murni.** Tidak ada satu pun nomor bisnis dibuat di frontend; seluruhnya
datang dari backend. Karena itu gerbang `G4` **tidak** menyentuh frontend secara langsung — ia
menyentuh lewat ketiadaan endpoint pasangannya.

**Pembaruan 9 September 2026 — `G4` berubah sifat.** Penghalangnya bukan lagi pemilik yang belum
ditunjuk (`OQ-PLT-007` ✅ tertutup, pemiliknya `Andry`), melainkan **dependency pengiriman lintas
modul**: kedelapan task frontend bertanda ⛔ menunggu gelombang `MVP-1` blueprint Platform
(`PLT-SLICE-01`) yang menutup `G4`, lalu task backend pasangannya. Rinciannya di
[backend-roadmap.md](backend-roadmap.md) bagian 6.1.

---

## 1. Cara membaca roadmap ini

| Penanda | Arti | Boleh dijadwalkan? |
| --- | --- | --- |
| ✅ | **SELESAI** — bukti tercatat di `task/report/frontend/` | Sudah selesai |
| 🟡 | **PENDING** — pasangan backend-nya sudah `SELESAI` atau tidak ada | **Ya** |
| ⛔ | **BLOCKED** — pasangan backend-nya belum ada | **Tidak** |

---

## 2. Ringkasan status

| Penanda | Jumlah | Task |
| --- | ---: | --- |
| ✅ SELESAI | 1 | `FE-BD-001` |
| 🟡 SELESAI SEBAGIAN | 1 | `FE-BD-011` — 1 dari 2 acceptance criteria |
| 🟡 PENDING | 2 | `FE-BD-006`, `FE-BD-009` |
| ⛔ BLOCKED | 8 | `FE-BD-002`, `003`, `004`, `005`, `007`, `008`, `010`, `012` |
| **Total** | **12** | |

**Satu task frontend selesai (`FE-BD-001`).** `FE-BD-011` **dikerjakan 10 September 2026** dan
berakhir 🟡 sebagian. Dua task `PENDING` tersisa dan siap dijadwalkan (`FE-BD-006`, `FE-BD-009`).

**`FE-BD-009` terbuka sejak 9 September 2026**, ketika `BE-BD-011` selesai
([laporan](../task/report/backend/BE-BD-011.md)). Kedelapan yang masih terblokir kini **seluruhnya**
tertahan `G4` lewat rantai backend — tidak ada lagi yang menunggu backend berjalur terbuka.

---

## 3. Urutan dependency

```text
════════ JALUR TERBUKA — pasangan backend sudah SELESAI ════════

✅ BE-BD-001 (master komponen & alasan)  ──> ✅ FE-BD-001 (setup master)          SELESAI
✅ BE-BD-014 (master lokasi penyimpanan) ──> 🟡 FE-BD-011 (lokasi penyimpanan)    SEBAGIAN 1/2 AC
                    G1 ✅ saja           ──> 🟡 FE-BD-006 (registrasi menu)       PENDING


════════ JALUR TERBUKA LEWAT BACKEND YANG SIAP DIKERJAKAN ════════

✅ BE-BD-005 (pemeriksaan golongan darah)   SELESAI
       └── ✅ BE-BD-011 (penyelesaian konflik)   SELESAI
                  └── 🟡 FE-BD-009 (penyelesaian konflik di layar pemeriksaan)
                             TERBUKA sejak 9 September 2026 — BE-BD-011 selesai

════════ JALUR TERTAHAN G4 ════════

⛔ BE-BD-003 ──> ⛔ FE-BD-002 (order darah + pemenuhan + pembatalan)
⛔ BE-BD-004 ──> ⛔ FE-BD-003 (permintaan PMI + penerimaan)
⛔ BE-BD-012 ──> ⛔ FE-BD-010 (daftar & pencatatan tindakan Bank Darah)
⛔ BE-BD-015 ──> ⛔ FE-BD-012 (penyimpanan & perpindahan lokasi kantong)
⛔ BE-BD-006 ──> ⛔ FE-BD-004 (alokasi kantong + pembatalan alokasi)
⛔ BE-BD-009 ──> ⛔ FE-BD-007 (penyelesaian PendingReview — tiga tombol tiga penjaga)
⛔ BE-BD-010 ──> ⛔ FE-BD-008 (koreksi dua langkah + daftar tunggakan bukti darurat)

⛔ FE-BD-005 (golongan darah + bukti + pemberian + jalur darurat)
       dep: BE-BD-005 ✅  +  BE-BD-007 ⛔  +  BE-BD-008 ⛔
       └── SEBAGIAN terbuka: bagian pencatatan golongan darah mengikuti BE-BD-005,
           tetapi bagian bukti kecocokan dan pemberian tertahan G4.
           JANGAN dipecah tanpa persetujuan pemilik — lihat catatan pada task
```

**Yang boleh paralel.** `FE-BD-006` dan `FE-BD-009` tidak saling bergantung dan boleh dikerjakan
dua orang berbeda. `FE-BD-011` sudah dikerjakan 10 September 2026.

---

## 4. Task

### ✅ `FE-BD-001` — Setup master dapat dikelola petugas

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI (2026-09-07).** Laporan tracked: [FE-BD-001](../task/report/frontend/FE-BD-001.md). Pasangan backend `BE-BD-001` **`SELESAI`** dengan 18 endpoint terbukti. ESLint `0 Error(s)`, UI GATE 10 elemen `REUSE` |
| **Outcome** | Petugas mengelola katalog komponen darah dan daftar alasan terkendali lewat layar |
| **Layar** | `FE-BD-08`, `FE-BD-09` |
| **Kontrak** | api-contract `v4` — Blood Component, Blood Bank Reason |
| **Reuse** | `BD-CAP-021` — sepuluh komponen dasar `base-features/` **terverifikasi tidak berubah** pada `101ec5d3` |
| **Dependency** | `G1` ✅, `BE-BD-001` ✅ |
| **Acceptance** | Master CRUD dapat dijalankan dari layar |
| **Risk/owner** | Rendah / BDRS |
| **DoD** | Rupa `DEV_DISCRETION`; **sumber data terkunci** pada endpoint kontrak `v4`. Dilarang membuat komponen dasar tandingan |
| **⚠️ Catatan pemakaian** | Keempat migration `MVP-0` **belum dijalankan**. Layar dapat dibangun dan diuji terhadap kontrak, tetapi **belum dapat dipakai di lingkungan mana pun** sampai migration diterapkan — dan eksekusi itu kini lintas modul |

---

### 🟡 `FE-BD-011` — Lokasi penyimpanan darah dikelola, akibat penonaktifan terbaca

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SELESAI SEBAGIAN 10 September 2026.** Bukti: [laporan](../task/report/frontend/FE-BD-011.md). Layar `FE-BD-10` berdiri penuh — 14 berkas baru mengikuti bentuk baku master data, nol komponen baru (12 elemen seluruhnya `REUSE`). `npm run lint` **`0 errors, 608 warnings`** — nol dari berkas task ini; `npm run build` **`✓ Compiled successfully in 27.1s`** dengan keempat route terdaftar; `node --test tests/unit` **434 lulus, 0 gagal**. **1 dari 2 acceptance terpenuhi:** `FE-BD-014` ✅ terbukti lewat penanda `IsBloodBankHaltedByEmptyActiveLocation`; **`FE-BD-015` ⛔ belum** — angka kantong tertahan **tidak ada di backend**, entity `BbkBloodUnitPlacement` menunggu `BE-BD-015`. Uji manual `NOT FEASIBLE`: migration `20260903083142_AddMstBloodStorageLocation` belum dijalankan |
| **Outcome** | Petugas mengelola lokasi penyimpanan darah, dan akibat penonaktifan sebuah lokasi terbaca jelas sebelum dikonfirmasi |
| **Layar** | `FE-BD-10` |
| **Kontrak** | api-contract `v4` — Blood Storage Location |
| **Reuse** | `BD-CAP-021` |
| **Dependency** | `G1` ✅, `BE-BD-014` ✅ |
| **Acceptance** | `FE-BD-014` keadaan kosong menyatakan modul berhenti, bukan sekadar "tidak ada data"; `FE-BD-015` konfirmasi penonaktifan **menyebut jumlah kantong tertahan** |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Tombol hapus **tidak** disediakan — penonaktifan memakai `PATCH /status`, sesuai `DEC-BD-037` yang menyatakan penonaktifan **tidak** memindahkan kantong |

---

### 🟡 `FE-BD-006` — Seluruh layar Bank Darah terjangkau dari menu

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **PENDING — SIAP DIJADWALKAN.** Dependency-nya hanya `G1` |
| **Outcome** | Setiap layar Bank Darah dapat dicapai dari menu, dan butir menu hanya tampil bagi peran yang berhak |
| **Scope** | `menu-items.jsx` |
| **Dependency** | `G1` ✅ — **nol dependency backend** |
| **Acceptance** | Butir menu mengarah ke layar yang hak aksesnya benar |
| **Risk/owner** | Rendah / BDRS |
| **DoD** | Registrasi menu menjadi acceptance salah satu task layar, bukan pekerjaan yang berdiri sendiri tanpa layar |
| **Catatan urutan** | Boleh dikerjakan lebih dulu, tetapi butir menu yang menunjuk layar belum ada **wajib disembunyikan**, bukan menampilkan halaman kosong |

---

### 🟡 `FE-BD-009` — Penyelesaian konflik di dalam layar pemeriksaan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **PENDING — SIAP DIJADWALKAN** sejak 9 September 2026. Blocker-nya hilang: `BE-BD-011` selesai ([laporan](../task/report/backend/BE-BD-011.md)), termasuk endpoint `POST /conflict-resolution` beserta butir hak akses `BloodGroupExam : ResolveConflict` yang terpisah dari `Validate` |
| **Kenapa dibedakan** | Ini satu-satunya task frontend terblokir yang **tidak** menunggu provider number-series. `BE-BD-005` lalu `BE-BD-011` sudah dikerjakan, dan task ini terbuka persis seperti yang diperkirakan |
| **Outcome** | Validator klinis menyelesaikan konflik golongan darah **di dalam layar pemeriksaan**, bukan lewat daftar kerja tersendiri |
| **Layar** | `FE-BD-06` |
| **Kontrak** | api-contract `v4` |
| **Reuse** | `BD-CAP-021` |
| **Dependency** | `G1` ✅, `BE-BD-011` ✅ |
| **Risk/owner** | Tinggi / klinis |
| **DoD** | **Bukan** daftar kerja keempat — penyelesaian konflik hidup di layar pemeriksaan |

---

### ⛔ `FE-BD-002` — Petugas mengelola order darah, pemenuhan, dan pembatalan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-003` tertahan `G4` |
| **Outcome** | Petugas membuat order darah, melihat pemenuhannya, dan membatalkannya dengan alasan berkategori |
| **Layar** | `FE-BD-01`, `FE-BD-02` |
| **Kontrak** | api-contract `v4` — Blood Order |
| **Reuse** | `BD-CAP-021` |
| **Dependency** | `G1` ✅, `BE-BD-003` ⛔ |
| **Acceptance** | Order ganda tertahan beserta alasannya (`FE-BD-003`); kategori alasan pembatalan **sesuai peran** |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Empat keadaan layar digambar: kosong, memuat, berisi, gagal |

---

### ⛔ `FE-BD-003` — Petugas mengelola permintaan PMI dan penerimaan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-004` tertahan `G4` dan `BE-BD-003` |
| **Outcome** | Petugas membuat permintaan ke PMI dan mencatat penerimaan, termasuk penerimaan berlebih |
| **Layar** | `FE-BD-03` |
| **Kontrak** | api-contract `v4` — Provider Request |
| **Dependency** | `G1` ✅, `BE-BD-004` ⛔ |
| **Acceptance** | Penerimaan termasuk kelebihan tercatat dan tidak membuat sisa negatif |
| **Risk/owner** | Sedang / BDRS |

---

### ⛔ `FE-BD-012` — Penyimpanan dan perpindahan lokasi kantong

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-015` tertahan lewat `BE-BD-004` |
| **Outcome** | Petugas menempatkan dan memindahkan kantong, dan kantong yang tertahan tersaring jelas |
| **Layar** | `FE-BD-04`, `FE-BD-05` (parsial) |
| **Kontrak** | api-contract `v4` |
| **Dependency** | `G1` ✅, `BE-BD-015` ⛔ |
| **Acceptance** | `FE-BD-010` saringan `Received` dan lokasi nonaktif **wajib**; `FE-BD-011` kolom lokasi beserta penandanya |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | **Bukan** daftar kerja keempat — saringan menempel pada daftar yang sudah ada |

---

### ⛔ `FE-BD-004` — Petugas mengalokasikan kantong dan membatalkan alokasi

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-006` tertahan lewat `BE-BD-015` |
| **Layar** | `FE-BD-05` (parsial) |
| **Dependency** | `G1` ✅, `BE-BD-006` ⛔ |
| **Acceptance** | Daftar `PendingReview` **wajib ada** (`FE-BD-002`) |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Worklist #2 tersedia |

---

### ⛔ `FE-BD-005` — Golongan darah, bukti kecocokan, pemberian, dan jalur darurat

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED SEBAGIAN** — dependency-nya tiga task dengan keadaan berbeda |
| **Rincian dependency** | `BE-BD-005` ✅ **SELESAI** · `BE-BD-007` ⛔ **BLOCKED** · `BE-BD-008` ⛔ **BLOCKED** |
| **⚠️ Jangan dipecah sendiri** | Bagian pencatatan golongan darah secara teknis mengikuti `BE-BD-005` yang terbuka, tetapi bagian bukti kecocokan dan pemberian tertahan `G4`. Memecah task ini menjadi dua **mengubah scope roadmap** dan menuntut persetujuan pemilik. Roadmap ini **tidak** memecahnya sendiri |
| **Outcome** | Petugas mencatat golongan darah, bukti kecocokan beserta hasilnya, lalu memberikan kantong; jalur darurat terbaca jelas dan berbeda dari jalur normal |
| **Layar** | `FE-BD-05`, `FE-BD-06` (parsial) |
| **Kontrak** | api-contract `v4` |
| **Dependency** | `G1` ✅, `BE-BD-005` ✅, `BE-BD-007` ⛔, `BE-BD-008` ⛔ |
| **Acceptance** | `FE-BD-021` hasil tidak cocok **menutup tombol Berikan** dengan pesan yang benar; `FE-BD-018` peran penerbit dipilih sendiri; `FE-BD-013` gerbang pemberian terbaca |
| **Risk/owner** | **Tinggi / klinis & BDRS** |

---

### ⛔ `FE-BD-007` — Penyelesaian `PendingReview`, tiga tombol tiga penjaga

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-009` tertahan lewat `BE-BD-006`/`007` |
| **Layar** | `FE-BD-05` |
| **Dependency** | `G1` ✅, `BE-BD-009` ⛔ |
| **Acceptance** | `FE-BD-020` — ketiga tombol punya penjaga hak akses **terpisah**, sesuai `DEC-BD-043` dan `DEC-BD-045` |
| **Risk/owner** | Sedang / BDRS |

---

### ⛔ `FE-BD-008` — Koreksi dua langkah dan daftar tunggakan bukti darurat

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-010` tertahan lewat `BE-BD-007` |
| **Layar** | `FE-BD-05`, `FE-BD-04` |
| **Dependency** | `G1` ✅, `BE-BD-010` ⛔ |
| **Acceptance** | Koreksi menuntut **dua langkah**; daftar tunggakan bukti darurat tersedia (worklist #3) |
| **Risk/owner** | Sedang / BDRS |

---

### ⛔ `FE-BD-010` — Daftar dan pencatatan tindakan Bank Darah

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **BLOCKED** — `BE-BD-012` tertahan `G4` |
| **Layar** | `FE-BD-07` |
| **Dependency** | `G1` ✅, `BE-BD-012` ⛔ |
| **Risk/owner** | Sedang / BDRS |
| **Catatan** | Tindakan dicatat **tanpa** penyaluran biaya ke Billing — penyaluran adalah `BE-BD-013` future scope |

---

## 5. Coverage gap requirement ke test

| Gap | Keadaan | Akibat pada rencana |
| --- | --- | --- |
| Layar Bank Darah belum ada sama sekali | `BD-CAP-020` `Missing` — nol route, nol slice Redux, nol pemanggilan ke-27 endpoint yang sudah jadi | Seluruh layar dibangun baru. Bukan gap yang menahan; ini memang scope-nya |
| Bukti uji frontend | Nol test frontend Bank Darah | Ditetapkan per task saat eksekusi mengikuti `test-policy.md` frontend |
| Rupa layar | `DEV_DISCRETION` | Sengaja tidak dikunci. Yang dikunci sumber data, hak akses, dan keadaan layar wajib |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Butir | Alasan |
| --- | --- |
| Task backend | Ada di [backend-roadmap.md](backend-roadmap.md) |
| Penelusuran requirement → test | Ada di [requirement-traceability.md](requirement-traceability.md) |
| Keputusan menu, route, tab/modal/drawer, warna, tata letak | `DEV_DISCRETION` — bukan wewenang roadmap |
| Layar penyaluran biaya Billing | Mengikuti `BE-BD-013` yang berada di future scope |
