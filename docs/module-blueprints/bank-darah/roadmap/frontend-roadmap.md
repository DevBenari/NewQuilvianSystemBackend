# Roadmap Delivery Frontend — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
module_name: BloodBankManagement
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
blueprint_root: docs/module-blueprints/bank-darah/
roadmap_revision: 9
revision_9_scope: CONTRACT_V5_FE_BD_002_DEPENDENCIES
revision_9_status: APPROVED — Sukmagp 2026-09-19. Riwayat: DRAFT 2026-09-18
revision_9_note: >-
  Satu kartu berubah: FE-BD-002 memperoleh dependency BE-BD-017 dan BE-BD-018 (task backend baru
  amendment kontrak v5, DEC-BD-055..058), rujukan kontrak v5, dan baris syarat dimulai. Status
  FE-BD-002 tetap belum dikerjakan, tidak ditandai sebagian. Kartu lain, acceptance criteria, dan
  task ID tidak berubah. Hitungan frontend tetap 3 selesai, 0 sebagian, 9 belum dikerjakan,
  0 terblokir.
revision_8_scope: FINAL_BACKEND_SYNC_AND_CANONICAL_FORM
revision_8_note: >-
  Sinkronisasi terhadap backend final (16 dari 16 task backend selesai, commit 77f60c88) dan
  frontend 6640a5e7, tanggal 2026-09-18. Nol task ditambah, dihapus, dipecah, maupun dinomori
  ulang; nol source, migration, maupun database disentuh. Yang berubah:
  (1) BE-BD-013 bukan lagi future scope. FE-BD-010 memperoleh dependency BE-BD-013 karena
  endpoint complete yang dipakainya kini membawa BillingHandoff; kartu FE-BD-010 memperoleh
  baris Outcome, Kontrak, Acceptance, dan batas cakupan. Tombol kirim ulang biaya TIDAK
  ditambahkan — dicatat sebagai keputusan pemilik BD-UI-GAP-001.
  (2) FE-BD-011 memperoleh dependency BE-BD-015, sumber HeldUnitCount yang ditunggu kriteria
  keduanya. Penahan backend-nya gugur.
  (3) FE-BD-001 DITURUNKAN dari selesai ke sebagian: simpan (tambah dan ubah) pada kedua layar
  master rusak di source 6640a5e7.
  (4) Legenda, grafik, dan tabel gelombang dirapikan ke bentuk baku rules/rule-output
  (status-task-roadmap.md dan grafik-dependency-roadmap.md). Seluruh hubungan grafik revisi 7
  muncul kembali sebagai garis. Gerbang G1 yang sudah tertutup dipindah ke baris Gerbang.
  (5) Tiga coverage gap dicatat: BD-UI-GAP-001 (kirim ulang biaya), BD-UI-GAP-002 (pembaca hak
  akses frontend), BD-UI-GAP-003 (tujuh kewajiban layar mengikat belum tertulis di kartu).
revision_8_approval_note: >-
  DISETUJUI Sukmagp 2026-09-18, bersama dua keputusan. (a) BD-UI-GAP-001 ditutup dengan Opsi A:
  tidak ada tombol kirim ulang biaya di layar; FE-BD-010 wajib membaca BillingHandoff dari
  POST /complete, menampilkannya apa adanya, dan tidak pernah menyatakan Billing berhasil bila
  Kind bukan Emitted/Replayed; FE-BD-010 dilarang menyediakan resend-cost-fact sebagai aksi biasa,
  mengarang status Billing yang tetap dari GET, dan mengirim ulang ke Billing secara otomatis.
  POST /resend-cost-fact tetap kemampuan API teknis untuk pemulihan. (b) Pemetaan BD-UI-GAP-003
  disetujui tanpa koreksi dan kini tertulis pada acceptance FE-BD-002, FE-BD-005, FE-BD-008, dan
  FE-BD-009. BD-UI-GAP-002 tetap gap implementasi, bukan penahan approval. Nol task baru, nol
  task ID berubah, nol dependency berubah; revision tetap 8 karena yang disetujui adalah isi
  revisi 8 itu sendiri.
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
status: APPROVED
status_note: >-
  19 September 2026: revisi 9 DISETUJUI Sukmagp bersama kontrak v5. Riwayat: DRAFT.
  18 September 2026: revisi 9 (dependency baru FE-BD-002 dari amendment kontrak v5) berstatus
  DRAFT sampai pemilik meninjau naskahnya. Revisi 8 tetap APPROVED (Sukmagp 2026-09-18) untuk
  seluruh kartu lain. Riwayat: APPROVED; FORWARD-TEST / DRAFT sejak roadmap dipecah pada
  revisi 4 sampai approval revisi 8.
approval_gate: BLUEPRINT_APPROVED
contract_version: v5 (approved — Sukmagp 2026-09-19). Riwayat v5 draft 2026-09-18; v4 (approved, kini superseded)
frontend_source_sha: fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e
frontend_source_sha_note: >-
  18 September 2026 (jangkar bukti FE-BD-006): naik dari e24c9e4c53f64e8c8972d8fd317355099c065695
  ke fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e (feat(bank-darah): enforce permission-aware setup
  menu), commit implementasi FE-BD-006 di sukmagpV2, di-push ke origin/sukmagpV2. Satu-satunya
  commit di atas e24c9e4c; isinya persis 7 berkas FE-BD-006. Riwayat e24c9e4c:
  18 September 2026: naik dari 6640a5e7 ke e24c9e4c (commit implementasi FE-BD-011, di-push ke
  origin/sukmagpV2). Berkas Bank Darah yang berubah hanya milik FE-BD-001 (2d0ac741) dan FE-BD-011
  (e24c9e4c). Dari 10 base component yang dikutip BD-CAP-021, dua berubah secara aditif dan opt-in
  lewat merge dari cabang lain (filter-select renderOption, base-editor-form remountKey); perilaku
  default tidak berubah. Riwayat 6640a5e7: naik dari f79af16847c99961842081f707bc0c4ff6c2d93b. Rentang itu membawa puluhan commit
  merge, tetapi selisih isi berkasnya hanya SATU: src/utils/menu-sidebar/menu-items.jsx,
  18 baris dihapus lewat b98f5bdc9. Itu pekerjaan FE-BD-006 yang dulu belum di-commit —
  tiga butir menu Bank Darah yang terduplikasi di bawah grup lain dibuang. Grup Bank Darah
  -> Setup tetap memuat ketiga butir. Nol berkas Bank Darah lain berubah.
frontend_branch: sukmagpV2
backend_source_sha: 2bd9fc2addb373873494cb7342316fec143312d5
backend_source_sha_note: >-
  18 September 2026: dibaca pada 2bd9fc2a; rentang 77f60c88..2bd9fc2a hanya docs/. Riwayat 77f60c88:
  Commit penutupan BE-BD-013, sudah di-push ke origin/sukmagp. Riwayat: 55ac6ab pada
  revisi 7, dengan peringatan berkas BE-BD-005/BE-BD-011 yang waktu itu belum di-commit —
  peringatan itu sudah tidak berlaku.
backend_branch: sukmagp
decision_revision: 13
decision_note: >-
  Register memuat DEC-BD-001 sampai DEC-BD-058. DEC-BD-055..058 disetujui Sukmagp 2026-09-18
  (persetujuan proses klinis DEC-BD-055 BLOCKED). DEC-BD-016 disetujui Sukmagp 2026-09-17.
open_ui_gaps: []
open_ui_gaps_note: >-
  Riwayat: BD-UI-GAP-002 terbuka sejak 2026-09-18 pagi; tertutup pada tingkat implementasi
  2026-09-18 lewat FE-BD-006; ditutup penuh 2026-09-18 sesudah uji runtime pemilik R1-R8 PASS.
closed_ui_gaps:
  - "BD-UI-GAP-001 — ditutup Opsi A, Sukmagp 2026-09-18"
  - "BD-UI-GAP-002 — ditutup penuh lewat FE-BD-006, uji runtime pemilik Sukmagp R1-R8 PASS 2026-09-18"
  - "BD-UI-GAP-003 — diserap ke acceptance kartu, Sukmagp 2026-09-18"
owners:
  - "Product/Domain: pemilik proses BDRS"
  - "Frontend: pemilik proses BDRS"
approved_by:
  - "Sukmagp — set kontrak v4 dan roadmap revisi 2, 2026-09-03"
  - "Sukmagp — roadmap frontend revisi 8, BD-UI-GAP-001 Opsi A, pemetaan BD-UI-GAP-003, 2026-09-18"
  - "Sukmagp — roadmap frontend revisi 9 (dependency FE-BD-002 pada BE-BD-017 dan BE-BD-018), kontrak v5, 2026-09-19"
approved_at: "2026-09-19"
approval_note: >-
  Approval 2026-09-03 berlaku atas roadmap revisi 2. Revisi 3 menambahkan gerbang G4
  dan revisi 4 memecah roadmap. Approval TIDAK berpindah otomatis.
  Pada 2026-09-10 Sukmagp menyetujui backend-roadmap.md revisi 7 saja; roadmap frontend
  ini tetap FORWARD-TEST / DRAFT sampai diputuskan tersendiri.
  Revisi 8 DISETUJUI Sukmagp 2026-09-18. Approval membuka penjadwalan task; wewenang menulis
  source tetap diberikan per task lewat build-module-frontend. Riwayat: sebelum approval,
  catatan ini berbunyi "Revisi 8 JUGA belum disetujui" karena revisi itu mengubah dependency dan
  acceptance.
supersedes: roadmap/archive/revision-3/00-delivery-plan.md
```

---

## 0. Peringatan yang tidak boleh dilewati

**Roadmap ini sudah disetujui.** `Sukmagp` menyetujui revisi 8 pada 18 September 2026, bersama
keputusan `BD-UI-GAP-001` Opsi A dan pemetaan `BD-UI-GAP-003`. Approval membuka **penjadwalan**. Wewenang
menulis source tetap diberikan satu task satu wewenang lewat `build-module-frontend`. **Riwayat:** sampai
approval itu revisi 8 berstatus `FORWARD-TEST / DRAFT`, dan tidak ada task yang boleh dijalankan.

**Tidak ada task frontend yang boleh mendahului task backend pasangannya**, walaupun kontrak API sudah
`approved` dan terkunci pada `v4`. Sejak 17 September 2026 aturan ini tidak lagi menahan apa pun:
keenam belas task backend sudah ✅.

**Rupa layar bersifat `DEV_DISCRETION`.** Roadmap ini mengunci **sumber data, hak akses, dan keadaan
layar yang wajib ada**. Warna, tata letak, dan pilihan modal atau drawer tidak dikunci di sini. Jangan
menetapkan keputusan produk dari dokumen ini.

**Frontend adalah konsumen murni.** Tidak ada nomor bisnis yang dibuat di frontend; seluruhnya datang
dari backend. Frontend juga **tidak pernah** mengirim isian Billing. Konteks sumber, jenis efek,
nominal, dan identitas fakta biaya diturunkan backend.

**Pembaruan 18 September 2026 — backend final.** Keenam belas task backend ✅, termasuk `BE-BD-013`
(penyaluran biaya tindakan ke Billing). Pass ini menemukan tiga hal yang mengubah pembacaan roadmap:

1. `FE-BD-001` **diturunkan ke 🟡**. Menyimpan data pada kedua layar master gagal di sisi layar
   walaupun backend sudah menyimpannya. Rinciannya ada pada kartu `FE-BD-001`. **Diperbarui hari yang
   sama:** task ini dibuka ulang, diperbaiki, dan diverifikasi ulang lewat uji runtime pemilik, lalu
   kembali ✅.
2. `FE-BD-011` **tidak lagi tertahan backend**. Angka kantong tertahan sudah tersedia. **Diperbarui hari
   yang sama:** kriteria `FE-BD-015` diimplementasikan dan diverifikasi runtime oleh pemilik, sehingga
   `FE-BD-011` ✅.
3. `BE-BD-013` menambah satu hal yang **wajib** dihormati `FE-BD-010`, yaitu jawaban `complete` yang
   membawa `BillingHandoff`. Tombol kirim ulang biaya **tidak** disediakan: pemilik menutup
   `BD-UI-GAP-001` dengan Opsi A pada 18 September 2026.

**Riwayat — 9 September 2026:** `G4` berubah dari pemilik yang belum ditunjuk menjadi dependency
pengiriman lintas modul ke `PLT-SLICE-01`; kedelapan task frontend bertanda ⛔ menunggu gelombang
`MVP-1` Platform lalu task backend pasangannya ([backend-roadmap.md](backend-roadmap.md) bagian 6.1).
**Riwayat — 10 September 2026:** `G4` tertutup atas pernyataan pemiliknya, `Andry`; task frontend
bertanda ⛔ tetap menunggu pasangan backend. **Riwayat — 17 September 2026:** pasangan backend
terakhir ✅, sehingga nol task frontend terblokir backend.

---

## 1. Arti tanda status pada dokumen ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

**Riwayat — revisi 7 memakai arti berbeda.** Waktu itu 🟡 dipakai juga untuk "PENDING, pasangan
backend sudah selesai". Arti itu bertabrakan dengan aturan baku
`rules/rule-output/status-task-roadmap.md`, yang hanya mengizinkan 🟡 untuk task yang source-nya sudah
ada. Sejak revisi 8, task yang siap dijadwalkan tetapi belum punya source ditulis **tanpa tanda**.
Contoh: `FE-BD-002` di revisi 7 tertulis "🟡 PENDING"; di revisi 8 ia tertulis tanpa tanda, karena
nol berkas source order darah ada di frontend `6640a5e7`.

---

## Grafik Urutan Dependency

Panah `A ─> B` berarti **A harus selesai lebih dulu, baru B boleh dimulai**. Setiap task roadmap ini
muncul tepat satu kali. Grafik memuat 26 node, lebih dari batas 15. Karena itu grafik dipecah menjadi
satu ringkasan antar-slice dan satu grafik per slice.

**Ringkasan antar-slice.** Ketiga slice tidak saling menunggu. Tidak ada task frontend yang menunggu
task frontend lain, sehingga tidak ada garis di antara slice.

```text
Slice 1 — Setup master dan menu

Slice 2 — Order darah, permintaan PMI, dan tindakan

Slice 3 — Kantong darah, pemberian, dan penyelesaiannya
```

### Slice 1 — Setup master dan menu

```text
BE-BD-001 ✅ [BE] ─> FE-BD-001 ✅

BE-BD-014 ✅ [BE] ─┬─> FE-BD-011 ✅
                   │
BE-BD-015 ✅ [BE] ─┘

FE-BD-006 ✅
```

### Slice 2 — Order darah, permintaan PMI, dan tindakan

```text
BE-BD-003 ✅ [BE] ─┬─> FE-BD-002
BE-BD-017 ✅ [BE] ─┤
BE-BD-018 ✅ [BE] ─┘

BE-BD-004 ✅ [BE] ─> FE-BD-003

BE-BD-012 ✅ [BE] ─┬─> FE-BD-010 ✅
                   │
BE-BD-013 ✅ [BE] ─┘
```

### Slice 3 — Kantong darah, pemberian, dan penyelesaiannya

```text
BE-BD-015 ✅ [BE] ─> FE-BD-012 ✅

BE-BD-006 ✅ [BE] ─> FE-BD-004 ✅

BE-BD-005 ✅ [BE] ─┐
                   │
BE-BD-007 ✅ [BE] ─┴─┬─> FE-BD-005 🟡
                     │
BE-BD-008 ✅ [BE] ───┘

BE-BD-011 ✅ [BE] ─> FE-BD-009 ✅

BE-BD-009 ✅ [BE] ─> FE-BD-007 ✅

BE-BD-010 ✅ [BE] ─> FE-BD-008
```

`[BE]` = task backend pada [backend-roadmap.md](backend-roadmap.md), cermin baca-saja. Tandanya disalin
dari sana. `BE-BD-015` muncul pada dua slice karena ditunggu dua task frontend. Hubungan antartask
backend, misalnya `BE-BD-005` → `BE-BD-011`, milik grafik backend dan tidak digambar ulang di sini.

**Pemeriksaan kecocokan dengan kolom `Dependency`.** Grafik memuat **15 pasangan** prasyarat → task:
3 pada Slice 1, 4 pada Slice 2, dan 8 pada Slice 3. Tabel register di bawah juga memuat 15 entri
dependency. Grafik bebas siklus, karena seluruh garis berjalan dari backend ke frontend.

**Yang berubah dari grafik revisi 7.** Seluruh sebelas hubungan backend → frontend lama muncul kembali
sebagai garis. Ada dua garis baru: `BE-BD-013` → `FE-BD-010` dan `BE-BD-015` → `FE-BD-011`. Alasannya
tertulis pada kartu masing-masing. Satu garis lama tidak digambar lagi, yaitu "`G1` ✅ saja ──>
`FE-BD-006`". `G1` adalah gerbang seluruh roadmap yang sudah tertutup 3 September 2026. Ia kini tercatat
pada baris **Gerbang** di setiap kartu, bukan sebagai garis.

### Gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | ✅ **Terpenuhi 23 September 2026** — `BE-BD-017` dan `BE-BD-018` `[BE]` keduanya selesai. **Riwayat:** keduanya 🟡 siap dijadwalkan sejak 19 September 2026; sebelumnya tertahan gerbang `G5` (revisi 9 draft) | `FE-BD-002` — tanpa nomor gelombang sampai kedua prasyaratnya ✅; sesudahnya kembali ke gelombang 1 |
| 1 | Prasyarat backend-nya seluruhnya ✅ `[BE]` | `FE-BD-001`, `FE-BD-011`, `FE-BD-006`, `FE-BD-003`, `FE-BD-010`, `FE-BD-012`, `FE-BD-004`, `FE-BD-005`, `FE-BD-009`, `FE-BD-007`, `FE-BD-008` — boleh paralel — revisi 8 **disetujui 18 September 2026** |

**Revisi 9:** `FE-BD-002` keluar sementara dari gelombang 1 karena menunggu dua task backend baru — sejak
19 September 2026 keduanya siap dijadwalkan tetapi belum dikerjakan; riwayatnya tertahan gerbang `G5`. Task lain tetap gelombang 1. **Riwayat (revisi 8):** seluruh task berada pada
gelombang 1, karena tidak ada task frontend yang menunggu task frontend lain.
Tidak ada task yang tertahan gerbang atau keputusan.

### Urutan pengerjaan yang disarankan

Tabel ini **saran prioritas**, bukan dependency. Urutannya mengikuti perjalanan fisik kantong darah.
Dengan begitu, setiap layar dapat diuji memakai data yang dihasilkan layar sebelumnya.

| Urutan | Task | Alasan |
| ---: | --- | --- |
| 1 | ✅ `FE-BD-001` — buka ulang | **Selesai 18 September 2026.** Alasan urutannya: layar master rusak saat menyimpan. Komponen darah dan alasan terkendali dipakai hampir semua layar sesudahnya. Perbaikannya kecil dan tidak menunggu siapa pun |
| 2 | ✅ `FE-BD-011` | **Selesai 18 September 2026.** Alasan urutannya: sisa satu kriteria, dan angkanya sudah ada di backend. Lokasi aktif adalah syarat semua penyimpanan kantong |
| 3 | `FE-BD-002` | Order darah adalah pintu masuk seluruh alur |
| 4 | `FE-BD-003` | Kantong lahir dari penerimaan PMI |
| 5 | `FE-BD-012` ✅ | Kantong disimpan sebelum dapat dialokasikan. **Selesai 24 September 2026**, sesudah `BE-BD-020` ✅ membuka saringan `inactiveLocation` ([laporan](../task/report/frontend/FE-BD-012.md)) |
| 6 | `FE-BD-004` ✅ | Alokasi dan daftar `PendingReview`. **Selesai 24 September 2026**, dikerjakan sebelum `FE-BD-012` karena `FE-BD-012` ditahan menunggu `BE-BD-020`; layar daftar kantong minimal dibangun di sini atas keputusan pemilik `G1` ([laporan](../task/report/frontend/FE-BD-004.md)) |
| 7 | `FE-BD-005` 🟡 | Golongan darah, bukti kecocokan, pemberian, dan jalur darurat. Juga membangun kerangka layar pemeriksaan `FE-BD-06`. **Sebagian per 24 September 2026**: bukti, pemberian, dan jalur darurat pada detail kantong; runtime `R0`–`R9` **14 dari 14 `PASS`** di Chromium terhadap backend sungguhan; layar `FE-BD-06` tidak dibangun atas keputusan pemilik ([laporan](../task/report/frontend/FE-BD-005.md)) |
| 8 | `FE-BD-009` ✅ | Menambah penyelesaian konflik ke layar pemeriksaan yang sama. **Selesai 24 September 2026**: karena `FE-BD-005` tidak membangun layar `FE-BD-06`, task ini membangun kerangka minimalnya sendiri (menu, route, daftar, detail) atas keputusan pemilik `B1`; runtime 11 dari 11 `PASS` ([laporan](../task/report/frontend/FE-BD-009.md)) |
| 9 | `FE-BD-007` ✅ | Tiga tombol penyelesaian `PendingReview`. **Selesai 24 September 2026**: `FE-BD-020` terbukti runtime; tombol Alihkan mengikuti kontrak apa adanya, dan status `Reallocated` tanpa jalan keluar dicatat sebagai backlog (`G1`) ([laporan](../task/report/frontend/FE-BD-007.md)) |
| 10 | `FE-BD-008` | Koreksi dua langkah dan tunggakan bukti darurat |
| 11 | `FE-BD-010` ✅ | Tindakan dan penampilan hasil penyerahan biaya. Cakupannya **terkunci** sejak `BD-UI-GAP-001` ditutup Opsi A, 18 September 2026. **Dikerjakan lebih awal dan selesai 23 September 2026** atas keputusan pemilik, sesudah `FE-BD-012` ditahan menunggu `BE-BD-020` ([laporan](../task/report/frontend/FE-BD-010.md)) |
| — | `FE-BD-006` kriteria kedua | ✅ **Selesai 18 September 2026** — dikerjakan ulang atas keputusan pemilik, terbukti otomatis dan lewat uji runtime pemilik R1–R8 `PASS` ([laporan](../task/report/frontend/FE-BD-006.md)). **Riwayat:** terimplementasi dan terbukti otomatis, menunggu bukti runtime; sebelumnya: Menyusul sesudah pembaca hak akses frontend (`BD-UI-GAP-002`) berdiri. Pembaca itu diputuskan dan dibangun pada task disetujui **pertama** yang benar-benar membutuhkannya, mengikuti `base-component-decision-gate`. Ini gap implementasi, bukan penahan approval |

---

## 2. Register status

| Task | Outcome | Slice | Dependency | Status | Laporan |
| --- | --- | --- | --- | :---: | --- |
| `FE-BD-001` | Setup master dapat dikelola petugas | 1 | `BE-BD-001` ✅ [BE] | ✅ 1 dari 1 — diverifikasi ulang 18 September 2026. **Riwayat:** 🟡 0 dari 1 — simpan rusak | [FE-BD-001](../task/report/frontend/FE-BD-001.md) |
| `FE-BD-011` | Lokasi penyimpanan dikelola, akibat penonaktifan terbaca | 1 | `BE-BD-014` ✅ [BE], `BE-BD-015` ✅ [BE] | ✅ 2 dari 2 — 18 September 2026. **Riwayat:** 🟡 1 dari 2 kriteria | [FE-BD-011](../task/report/frontend/FE-BD-011.md) |
| `FE-BD-006` | Seluruh layar Bank Darah terjangkau dari menu | 1 | — | ✅ 2 dari 2 — 18 September 2026, uji runtime pemilik R1–R8 `PASS`. **Riwayat:** 🟡 1 dari 2 terbukti penuh, kriteria kedua terbukti otomatis dan menunggu bukti runtime; 🟡 1 dari 2 kriteria | [FE-BD-006](../task/report/frontend/FE-BD-006.md) |
| `FE-BD-002` | Order darah, pemenuhan, dan pembatalan | 2 | `BE-BD-003` ✅ [BE], `BE-BD-017` ✅ [BE], `BE-BD-018` ✅ [BE], `BE-BD-019` ✅ [BE] | ✅ **selesai 23 September 2026** — validasi runtime R1–R7 seluruhnya `PASS` di browser sungguhan | [laporan](../task/report/frontend/FE-BD-002.md) |
| `FE-BD-003` | Permintaan PMI dan penerimaan | 2 | `BE-BD-004` ✅ [BE] | ✅ **selesai 23 September 2026** — empat acceptance layar `PASS` runtime; `AC-BD-014` backend-only | [laporan](../task/report/frontend/FE-BD-003.md) |
| `FE-BD-010` | Daftar, pencatatan, dan penyelesaian tindakan | 2 | `BE-BD-012` ✅ [BE], `BE-BD-013` ✅ [BE] | ✅ **selesai 23 September 2026** — validasi runtime 16 dari 16 `PASS` di browser sungguhan; `lint`/`build` `PASS`; keempat acceptance dan DoD terbukti | [laporan](../task/report/frontend/FE-BD-010.md) |
| `FE-BD-012` | Penyimpanan dan perpindahan lokasi kantong | 3 | `BE-BD-015` ✅ [BE] | ✅ **selesai 24 September 2026** — validasi ulang runtime **13 dari 13** `PASS` di browser sungguhan **terhadap backend sungguhan** (validasi pertama 9 dari 9); regresi `FE-BD-004` 16 dari 16; `lint:errors`/`build` `PASS`; acceptance `A1`–`A9` dan DoD terbukti | [laporan](../task/report/frontend/FE-BD-012.md) |
| `FE-BD-004` | Alokasi dan pembatalan alokasi | 3 | `BE-BD-006` ✅ [BE] | ✅ **selesai 24 September 2026** — validasi runtime 16 dari 16 `PASS` di browser sungguhan (validasi ulang 24 September 2026); `lint:errors`/`build` `PASS`; kedelapan acceptance `G5` dan DoD terbukti | [laporan](../task/report/frontend/FE-BD-004.md) |
| `FE-BD-005` | Golongan darah, bukti, pemberian, jalur darurat | 3 | `BE-BD-005` ✅ [BE], `BE-BD-007` ✅ [BE], `BE-BD-008` ✅ [BE] | 🟡 **sebagian, 24 September 2026** — `lint:errors`/`build` `PASS`, `test:unit` 1595 test 1588 lulus (12 baru lulus, 7 gagal lama), runtime `R0`–`R9` **14 dari 14 `PASS`** di Chromium terhadap backend sungguhan. 4 butir terpenuhi (outcome bukti dan pemberian, `FE-BD-018`, `FE-BD-012`, `FE-BD-005`), 1 sebagian (`FE-BD-021`), 4 belum terpenuhi (golongan darah `FE-BD-06`, `FE-BD-013`, `FE-BD-007`, `FE-BD-008` — keputusan pemilik: frontend tidak menghitung gerbang, backend tidak memulangkannya). Cacat UX pemilih dialog yang ditemukan runtime sudah diperbaiki dan dibuktikan ulang (`R3`, `R9` `PASS`); gap backend dicatat sebagai backlog atas keputusan pemilik. **Riwayat:** cacat UX pemilih dialog belum diperbaiki; runtime `NOT RUN` karena cookie sesi belum tersedia; belum dikerjakan | [laporan](../task/report/frontend/FE-BD-005.md) |
| `FE-BD-009` | Penyelesaian konflik di layar pemeriksaan | 3 | `BE-BD-011` ✅ [BE] | ✅ **selesai 24 September 2026** — runtime **11 dari 11** `PASS` di Chromium terhadap backend sungguhan; `lint:errors`/`build` `PASS` (371 halaman); `test:unit` 1605 test, 1598 lulus (10 baru lulus, 7 gagal lama); kedua acceptance dan DoD terbukti. **Riwayat:** belum dikerjakan | [laporan](../task/report/frontend/FE-BD-009.md) |
| `FE-BD-007` | Penyelesaian `PendingReview`, tiga tombol tiga penjaga | 3 | `BE-BD-009` ✅ [BE] | ✅ **selesai 24 September 2026** — runtime **13 dari 13** skenario `PASS` terhadap backend sungguhan (`R2a`–`R2g` membuktikan `FE-BD-020`); `lint:errors`/`build` `PASS` (371 halaman); `test:unit` 1611 test, 1604 lulus (6 baru lulus, 7 gagal lama). **Riwayat:** belum dikerjakan | [laporan](../task/report/frontend/FE-BD-007.md) |
| `FE-BD-008` | Koreksi dua langkah dan tunggakan bukti darurat | 3 | `BE-BD-010` ✅ [BE] | belum dikerjakan | — |

**Hitungan per 24 September 2026, sesudah `FE-BD-007` ✅:** 12 task = **10 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, `FE-BD-003`, `FE-BD-010`, `FE-BD-004`, `FE-BD-012`, `FE-BD-009`, **`FE-BD-007`**) +
**1 sebagian** (`FE-BD-005`) + **1 belum dikerjakan** (`FE-BD-008`) + **0 terblokir** pada register ini.

**Riwayat — hitungan per 24 September 2026, sesudah `FE-BD-009` ✅:** 12 task = **9 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, `FE-BD-003`, `FE-BD-010`, `FE-BD-004`, `FE-BD-012`, **`FE-BD-009`**) + **1 sebagian**
(`FE-BD-005`) + **2 belum dikerjakan** (`FE-BD-007`, `FE-BD-008`) + **0 terblokir** pada register ini.

**Riwayat — hitungan per 24 September 2026, sesudah `FE-BD-005` 🟡:** 12 task = **8 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, `FE-BD-003`, `FE-BD-010`, `FE-BD-004`, `FE-BD-012`) + **1 sebagian**
(**`FE-BD-005`**) + **3 belum dikerjakan** (`FE-BD-009`, `FE-BD-007`, `FE-BD-008`) + **0 terblokir** pada register ini.

**Riwayat — hitungan per 24 September 2026, sesudah `FE-BD-012` ✅:** 12 task = **8 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, `FE-BD-003`, `FE-BD-010`, `FE-BD-004`, **`FE-BD-012`**) + **0 sebagian** +
**4 belum dikerjakan** (`FE-BD-005`, `FE-BD-009`, `FE-BD-007`, `FE-BD-008`) + **0 terblokir** pada register ini.

**Riwayat — hitungan per 24 September 2026, sesudah `FE-BD-004` ✅:** 12 task = **7 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, `FE-BD-003`, `FE-BD-010`, **`FE-BD-004`**) + **0 sebagian** +
**5 belum dikerjakan** (`FE-BD-012`, `FE-BD-005`, `FE-BD-009`, `FE-BD-007`, `FE-BD-008`) + **0 terblokir** pada
register ini. `FE-BD-012` tercatat tertahan menunggu `BE-BD-020` pada `requirement-traceability.md`.

**Riwayat — hitungan per 23 September 2026, sesudah `FE-BD-003` ✅:** 12 task = **5 selesai** (`FE-BD-001`,
`FE-BD-011`, `FE-BD-006`, `FE-BD-002`, **`FE-BD-003`**) + **0 sebagian** + **7 belum dikerjakan** +
**0 terblokir**.
`FE-BD-002` ditutup dengan validasi runtime R1–R7 seluruhnya `PASS`, dijalankan di browser sungguhan
terhadap backend berisi `BE-BD-017`/`018`/`019`.

**Riwayat — hitungan per 18 September 2026, sesudah `FE-BD-006` ✅:** 12 task = **3 selesai**
(`FE-BD-001`, `FE-BD-011`, `FE-BD-006`) + **0 sebagian** + **9 belum dikerjakan** + **0 terblokir**.
Slice 1 selesai. Task berikutnya menurut urutan yang disetujui waktu itu: **`FE-BD-002`** — sejak
revisi 9 baru dapat dimulai sesudah `BE-BD-017` dan `BE-BD-018` ✅.

**Riwayat — sesudah `FE-BD-011` ✅:** 12 task = 2 selesai (`FE-BD-001`, `FE-BD-011`) + 1 sebagian
(`FE-BD-006`) + 9 belum dikerjakan + 0 terblokir. Task berikutnya waktu itu: `FE-BD-002`.

**Riwayat — sesudah `FE-BD-001` ✅:** 12 task = 1 selesai (`FE-BD-001`) + 2 sebagian (`FE-BD-006`,
`FE-BD-011`) + 9 belum dikerjakan + 0 terblokir. Task berikutnya waktu itu: `FE-BD-011`.

**Riwayat — pagi 18 September 2026:** 12 task = 0 selesai + 3 sebagian (`FE-BD-001`, `FE-BD-006`,
`FE-BD-011`) + 9 belum dikerjakan + 0 terblokir. Seluruh 12 task dapat dijadwalkan sejak revisi 8
disetujui 18 September 2026. Task pertama yang dibuka ulang: `FE-BD-001`.

**Riwayat — ringkasan revisi 7 (17 September 2026):** ✅ 1 (`FE-BD-001`); 🟡 sebagian 2 (`FE-BD-011`,
`FE-BD-006`); 🟡 PENDING 9 (`FE-BD-009`, `002`, `003`, `004`, `005`, `007`, `008`, `010`, `012`); ⛔ 0.
Sebelumnya ⛔ 1 (`FE-BD-008`, menunggu `BE-BD-010` sampai ✅ 17 September 2026), dan sebelumnya lagi ⛔ 7
(`FE-BD-003`, `004`, `005`, `007`, `008`, `010`, `012`). Pada 10 September 2026 `FE-BD-011` dan
`FE-BD-006` dikerjakan dan keduanya berakhir 🟡. `FE-BD-009` terbuka sejak 9 September 2026 ketika
`BE-BD-011` selesai ([laporan](../task/report/backend/BE-BD-011.md)).

---

## 3. Task

### ✅ `FE-BD-001` — Setup master dapat dikelola petugas

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 18 September 2026 — dibuka ulang, diperbaiki, dan diverifikasi ulang.** Kriteria tunggalnya terbukti penuh (**1 dari 1**). Perbaikannya: kedua hook editor memakai named import `unwrapApiData`, mengikuti pola `blood-storage-locations` dan `hr/master-data/job-level`. Total +6 / −4 baris di frontend `sukmagpV2`, di atas snapshot awal baru `beba89e3` yang disetujui pemilik, lalu **ter-commit dan ter-push** sebagai `2d0ac741` (`fix(bank-darah): restore FE-BD-001 master save flow`) ke `origin/sukmagpV2`. **Riwayat:** belum di-commit saat laporan ditulis. Validasi: 10 test baru `tests/unit/blood-bank-master-editor-save.test.mjs` lulus; suite `tests/unit/` **747 lulus, 0 gagal** (`npm run test:unit` sendiri `EXISTING / ENVIRONMENT ISSUE` — pola glob tidak diperluas Node `v20.20.0` di Windows); `npm run lint:errors` **`0 errors`**; `npm run build` **`✓ Compiled successfully in 5.2min`**. **Uji runtime oleh pemilik `Sukmagp` 18 September 2026** pada komponen darah dan alasan terkendali — tambah, ubah, smoke daftar/detail, validasi isian wajib, dan penolakan duplikat oleh backend seluruhnya `PASS`; backend menyimpan, tidak ada "Gagal Menyimpan" palsu, layar berpindah ke detail, dan isi detail sesuai hasil simpan. Nol butir DoD dikecualikan. Risiko sisa yang bukan kriteria: jeda 800 ms sesudah sukses. Bukti: [laporan](../task/report/frontend/FE-BD-001.md) bagian 9. **Riwayat:** 🟡 **SEBAGIAN — diturunkan dari ✅ pada 18 September 2026.** Kriteria tunggalnya belum terbukti penuh (**0 dari 1**). Daftar, detail, dan penonaktifan berdiri. **Simpan (tambah dan ubah) rusak pada kedua layar.** Buktinya dari source frontend `6640a5e7`: `use-master-data-blood-components-editor.jsx` baris 153 dan 179, serta `use-master-data-blood-bank-reasons-editor.jsx` baris 153 dan 179, memanggil `utils.unwrapApiData(...)`. Padahal `utils` di sana adalah objek default export `blood-components-utils.jsx` / `blood-bank-reasons-utils.jsx`, dan objek itu **tidak memuat** `unwrapApiData`. Fungsi itu hanya ada sebagai named export. Akibatnya, sesudah backend berhasil menyimpan, layar menampilkan toast "Berhasil", lalu langsung toast "Gagal Menyimpan", dan tidak berpindah ke halaman detail. **Contoh:** petugas menambah komponen `PRC`. Datanya tersimpan di server, tetapi layar menyatakan gagal. Petugas mencoba lagi, lalu ditolak karena kodenya sudah dipakai. Kedua berkas terakhir berubah pada `7e90e0477`, commit task ini sendiri. Cacat yang sama sudah dilaporkan builder `FE-BD-011` untuk layar alasan terkendali ([laporan FE-BD-011](../task/report/frontend/FE-BD-011.md) bagian 8, "Temuan di luar cakupan" butir 2). Pass ini menemukan cacat yang sama pada layar komponen darah. Cacat lolos karena uji runtime 7 September 2026 `NOT FEASIBLE`: migration belum dijalankan. **Penutupan:** buka ulang task ini lewat `build-module-frontend` dengan laporan dan task ID yang sama. **Riwayat:** ✅ **SELESAI (2026-09-07).** Laporan tracked: [FE-BD-001](../task/report/frontend/FE-BD-001.md). Pasangan backend `BE-BD-001` **`SELESAI`** dengan 18 endpoint terbukti. ESLint `0 Error(s)`, UI GATE 10 elemen `REUSE` |
| **Outcome** | Petugas mengelola katalog komponen darah dan daftar alasan terkendali lewat layar |
| **Layar** | `FE-BD-08`, `FE-BD-09` |
| **Kontrak** | api-contract `v4` — Blood Component, Blood Bank Reason |
| **Reuse** | `BD-CAP-021` — sepuluh komponen dasar `base-features/` **terverifikasi tidak berubah** pada `101ec5d3` |
| **Gerbang** | `G1` ✅ — tertutup 3 September 2026 |
| **Dependency** | `BE-BD-001` ✅ [BE] |
| **Acceptance** | Master CRUD dapat dijalankan dari layar |
| **Risk/owner** | Rendah / BDRS |
| **DoD** | Rupa `DEV_DISCRETION`; **sumber data terkunci** pada endpoint kontrak `v4`. Dilarang membuat komponen dasar tandingan |
| **Catatan pemakaian** | Migration Bank Darah **sudah diterapkan** di `QuilvianNewDevSukma` sejak 10 September 2026. Karena itu uji runtime penutupan kini dapat dijalankan di database itu. Database lain tetap wewenang tersendiri. **Riwayat:** keempat migration `MVP-0` belum dijalankan, sehingga layar belum dapat dipakai di lingkungan mana pun |

---

### ✅ `FE-BD-011` — Lokasi penyimpanan darah dikelola, akibat penonaktifan terbaca

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 18 September 2026.** Kedua kriteria terbukti (**2 dari 2**). `FE-BD-015`: konfirmasi penonaktifan mengambil ulang `GET /{id}` lalu menyebut `HeldUnitCount` yang sebenarnya. Angka yang hilang atau `GET` yang gagal tidak membuka konfirmasi, dan `PATCH` tidak dipakai sebagai sumber angka. Perubahan +169 / −22 baris pada utils, hook detail, dan view detail di atas `2d0ac741`, lalu **ter-commit dan ter-push** sebagai `e24c9e4c` (`feat(bank-darah): close FE-BD-011 storage deactivation flow`) ke `origin/sukmagpV2`. **Riwayat:** belum di-commit saat laporan ditulis. Validasi: 12 test baru lulus; suite `tests/unit/` **759 lulus, 0 gagal** (`npm run test:unit` sendiri `EXISTING / ENVIRONMENT ISSUE` — pola glob tidak diperluas Node `v20.20.0` di Windows); `npm run lint:errors` **`0 errors`**; `npm run build` **`✓ Compiled successfully in 37.9s`**. **Uji runtime pemilik `Sukmagp` 18 September 2026** seluruhnya `PASS`: angka lebih dari 0 sama dengan `GET` dan bukan dari `PATCH`; batal tanpa perubahan; angka 0; `GET` gagal atau offline; status basi dari tab lain; klik berulang. Nol butir DoD dikecualikan. Bukti: [laporan](../task/report/frontend/FE-BD-011.md) bagian 10. **Riwayat:** 🟡 **SELESAI SEBAGIAN — 1 dari 2 (tidak berubah).** **Penahan backend-nya gugur** (diperiksa 18 September 2026 pada backend `77f60c88`). `GET /api/v1/health-services/master-data/blood-storage-locations/{id}` membawa `HeldUnitCount`, yang dihitung saat dibaca (`BloodStorageLocationController.cs` baris 148–171). Dokumentasi controller itu sendiri menyebut angka ini untuk konfirmasi penonaktifan `FE-BD-015`. Layar belum memakainya: nol pemakaian `heldUnitCount` pada frontend `6640a5e7`. Sisa pekerjaannya murni frontend dan dapat dijadwalkan. **Batas yang perlu diketahui builder:** balasan `PATCH /{id}/status` tetap mengembalikan `HeldUnitCount` `0`, karena controller memetakan ulang entity tanpa membawa angka dari service. Ini cacat sisa milik `BE-BD-015`, yang sudah ✅ dan tidak dibuka ulang oleh pass ini ([BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6). Karena itu konfirmasi **wajib** membaca angka dari `GET /{id}` **sebelum** penonaktifan, bukan dari balasan PATCH. **Contoh:** lokasi "Kulkas BDRS-2" berisi 3 kantong. Detailnya memulangkan `HeldUnitCount = 3`, dan konfirmasi berbunyi "3 kantong akan tertahan di lokasi ini dan tidak dipindahkan sistem." **Riwayat:** 🟡 **SELESAI SEBAGIAN 10 September 2026.** Bukti: [laporan](../task/report/frontend/FE-BD-011.md). Layar `FE-BD-10` berdiri penuh — 14 berkas baru mengikuti bentuk baku master data, nol komponen baru (12 elemen seluruhnya `REUSE`). `npm run lint` **`0 errors, 608 warnings`** — nol dari berkas task ini; `npm run build` **`✓ Compiled successfully in 27.1s`** dengan keempat route terdaftar; `node --test tests/unit` **434 lulus, 0 gagal**. **1 dari 2 acceptance terpenuhi:** `FE-BD-014` ✅ terbukti lewat penanda `IsBloodBankHaltedByEmptyActiveLocation`; **`FE-BD-015` ⛔ belum** — angka kantong tertahan **tidak ada di backend**, entity `BbkBloodUnitPlacement` menunggu `BE-BD-015`. Uji manual `NOT FEASIBLE`: migration `20260903083142_AddMstBloodStorageLocation` belum dijalankan |
| **Outcome** | Petugas mengelola lokasi penyimpanan darah, dan akibat penonaktifan sebuah lokasi terbaca jelas sebelum dikonfirmasi |
| **Layar** | `FE-BD-10` |
| **Kontrak** | api-contract `v4` — Blood Storage Location |
| **Reuse** | `BD-CAP-021` |
| **Gerbang** | `G1` ✅ — tertutup 3 September 2026 |
| **Dependency** | `BE-BD-014` ✅ [BE], `BE-BD-015` ✅ [BE]. `BE-BD-015` ditambahkan pada revisi 8: ia sumber `HeldUnitCount` yang dituntut kriteria `FE-BD-015`. Laporan task ini sendiri sudah menyebut `BE-BD-015` sebagai penahan kriteria itu |
| **Acceptance** | `FE-BD-014` keadaan kosong menyatakan modul berhenti, bukan sekadar "tidak ada data"; `FE-BD-015` konfirmasi penonaktifan **menyebut jumlah kantong tertahan** |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Tombol hapus **tidak** disediakan — penonaktifan memakai `PATCH /status`, sesuai `DEC-BD-037` yang menyatakan penonaktifan **tidak** memindahkan kantong |

---

### ✅ `FE-BD-006` — Seluruh layar Bank Darah terjangkau dari menu

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 18 September 2026 — 2 dari 2 acceptance.** Uji runtime pemilik `Sukmagp` R1–R8 seluruhnya `PASS`: akses penuh, sebagian, tanpa akses, ganti pengguna di tab yang sama, kegagalan endpoint (gagal-tertutup, tanpa error mentah), URL langsung tanpa hak (`403` + `AccessDeniedGate`), SuperAdmin mengikuti pasangan dari endpoint, dan regresi Lab `SystemFlag`. Validasi otomatis tetap: unit **785/785**, `lint:errors` 0, `npm run lint` **`0 errors, 679 warnings`** (nol dari berkas task), build **`✓ Compiled successfully in 63s`**, `git diff --check` bersih. Nol DoD dikecualikan. Nol perubahan source sesudah uji runtime. **Ter-commit** sebagai `fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e` (`feat(bank-darah): enforce permission-aware setup menu`) di atas titik awal `e24c9e4c5`, ter-push ke `origin/sukmagpV2` (laporan §9; riwayat: belum di-commit saat penutupan). `BD-UI-GAP-002` ditutup penuh. Bukti: [laporan](../task/report/frontend/FE-BD-006.md) §6.3 dan §7. **Riwayat (18 September 2026, sebelum uji runtime):** 🟡 **SELESAI SEBAGIAN — dikerjakan ulang 18 September 2026 atas keputusan pemilik `Sukmagp` (paket A #2–#6, perluasan scope ke sidebar bersama dan slice `authPermission`, gagal-tertutup).** Kriteria kedua kini **terimplementasi dan terbukti otomatis**, tetapi **bukti runtime di browser belum ada**, sehingga belum ✅. Tiga butir Setup memperoleh `requiredPermission` (`BloodComponent`/`BloodBankReason`/`BloodStorageLocation` : `Read`); sidebar menyaringnya lewat keputusan ketat baru di atas pembaca kewenangan yang **sudah ada sejak `622a46f41`** (8 September 2026, masuk `sukmagpV2` lewat merge `d7059b563`, tidak ditemukan saat sinkronisasi roadmap). Validasi: unit **785 lulus, 0 gagal** (759 baseline + 26 baru, bentuk direktori Windows — `npm run test:unit` menjalankan nol test karena glob); `npm run lint:errors` keluar `0`; `npm run lint` **`0 errors, 679 warnings`**, nol dari berkas task; `npm run build` **`✓ Compiled successfully in 63s`**; `git diff --check` bersih. DoD yang belum: bukti runtime R1–R8 ([laporan](../task/report/frontend/FE-BD-006.md) §6.3). Source frontend belum di-commit di atas `e24c9e4c5`. **Riwayat (sebelum 18 September 2026, pengerjaan ulang):** 🟡 **SELESAI SEBAGIAN — 1 dari 2 (tidak berubah).** Perubahan `menu-items.jsx` yang dulu dicatat "belum di-commit" kini sudah ter-commit di `b98f5bdc9` (10 September 2026) dan ada di `6640a5e7`. Grup Bank Darah → Setup memuat tepat tiga butir, tanpa duplikat. **Kriteria kedua tetap belum terpenuhi**, karena `filterMenuItemsByRole` masih stub pada `6640a5e7`. **Yang berubah:** sumber hak akses pengguna yang sedang login kini **ada di backend**, yaitu `GET /api/v1/Auth/permissions`. Jawabannya `EffectivePermissionSet`, berisi pasangan `Resource` / `Action` dengan penamaan yang sama dengan `[AccessPermission]`. Endpoint ini masuk `sukmagp` sesudah `d07dcf3` dan ada di `77f60c88`. Frontend belum memakainya: nol pemakai pada `6640a5e7`. Jadi penahannya kini **bukan backend**. Yang kurang adalah pembaca hak akses di frontend (`BD-UI-GAP-002`). Pembaca itu komponen berstatus `NEW` menurut `base-component-decision-gate`, dan statusnya diputuskan pemilik saat build. **Riwayat:** 🟡 **SELESAI SEBAGIAN 10 September 2026.** Bukti: [laporan](../task/report/frontend/FE-BD-006.md). `npm run lint` **`0 errors, 608 warnings`** — nol dari berkas task ini; `npm run build` **`✓ Compiled successfully in 33.9s`**; `node --test tests/unit` **434 lulus, 0 gagal**. **1 dari 2 acceptance terpenuhi.** Bagian *mengarah ke layar* ✅: ketiga butir Setup menunjuk route yang terbukti ada, susunannya cocok `03-frontend-architecture.md` §2, dan **tiga entri duplikat dibuang** — sebelumnya ketiga layar muncul dua kali (3 dari hanya 4 path terduplikat di seluruh berkas). Bagian *hanya tampil bagi peran yang berhak* ⛔ **belum**: `filterMenuItemsByRole` adalah **stub** yang seluruh logikanya dikomentari, dan frontend **tidak punya katalog permission** pengguna berjalan — tidak dapat dikerjakan dengan menyunting `menu-items.jsx`. Uji manual `NOT FEASIBLE`: menuntut aplikasi berjalan beserta sesi login |
| **Outcome** | Setiap layar Bank Darah dapat dicapai dari menu, dan butir menu hanya tampil bagi peran yang berhak |
| **Scope** | `menu-items.jsx` |
| **Gerbang** | `G1` ✅ — tertutup 3 September 2026 |
| **Dependency** | — **nol dependency backend** |
| **Acceptance** | Butir menu mengarah ke layar yang hak aksesnya benar |
| **Risk/owner** | Rendah / BDRS |
| **DoD** | Registrasi menu menjadi acceptance salah satu task layar, bukan pekerjaan yang berdiri sendiri tanpa layar |
| **Catatan urutan** | Boleh dikerjakan lebih dulu, tetapi butir menu yang menunjuk layar belum ada **wajib disembunyikan**, bukan menampilkan halaman kosong |

---

### ✅ `FE-BD-002` — Petugas mengelola order darah, pemenuhan, dan pembatalan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — 23 September 2026. Ketiga acceptance terbukti; validasi runtime R1–R7 seluruhnya `PASS`, nol skenario gagal.** Dijalankan langsung agent di browser sungguhan (Playwright/Chromium) terhadap backend lokal berisi `BE-BD-017`/`018`/`019` di atas `QuilvianNewDevSukma` — bukan uji ber-mock, bukan atestasi pihak lain. Bukti: kolom daftar persis delapan dan kolom kosong permanen dihapus; penyaring tanggal benar-benar menembak backend (`?startDate=2026-09-14&endDate=2026-09-16` → 9 baris menjadi 5, penghitung layar `5 dari 5`); detail `ORD-00000090` menampilkan `Golongan Darah Diminta: A Positif` **terpisah** dari `Golongan Darah Hasil Pemeriksaan (Sah)`; pilihan golongan darah tepat sembilan tanpa `Tidak diinformasikan`; penahanan ganda `422 VAL-BD-001` dengan `duplicateComponentIds` dan nama komponen bentrok tampil; kategori pembatalan dibaca dari backend lalu ditampilkan dan dipakai memanggil `?category=`; reset mengosongkan seluruh penyaring termasuk teks pencarian dan kembali ke halaman 1. Build `exit 0`, `✓ Compiled successfully in 41s`, 367/367 halaman; lint `0 error`; `git diff --check` bersih. Total 8 berkas frontend diubah, **belum di-commit**. **Batas bukti:** aktor tunggal `superadmin` sehingga kategori pembatalan **klinis** belum pernah dilihat di layar; `R3` membuat ordernya lewat API karena otomasi form gagal; paging lintas halaman tak teruji karena data uji (9) lebih kecil dari ukuran halaman terkecil (10) ([laporan](../task/report/frontend/FE-BD-002.md) bagian 6.3). **Riwayat:** belum dikerjakan — siap dijadwalkan, nol penahan sejak 23 September 2026. **Riwayat:** belum dikerjakan — menunggu `BE-BD-017` dan `BE-BD-018` (revisi 9, 19 September 2026). |
| **Outcome** | Petugas membuat order darah, melihat pemenuhannya, dan membatalkannya dengan alasan berkategori |
| **Layar** | `FE-BD-01`, `FE-BD-02` |
| **Kontrak** | api-contract **`v5`** — Blood Order, termasuk Amendment `v5` (`requestedBloodGroup`, `errors` `VAL-BD-001`, `cancellationReasonCategory`, `bloodComponentId` + `components[]`/`totalIssuedQuantity`), ditambah `GET /blood-group-exams/patient/{patientId}/valid` untuk golongan darah hasil pemeriksaan (`03-frontend-architecture.md` §3). **Penyaring tanggal `startDate`/`endDate` menyusul lewat amandemen `BE-BD-019`** (`DEC-BD-059`, `DEC-BD-060`). **Riwayat:** `v4` |
| **Syarat dimulai** | **(1)** naskah kontrak `v5` disetujui (gerbang `G5`) — ✅ **terpenuhi 19 September 2026**; **(2)** `BE-BD-017` dan `BE-BD-018` ✅, dengan bukti: golongan darah diminta tersimpan dan terbaca kembali, `422 VAL-BD-001` membawa `errors` terstruktur, `cancellationReasonCategory` benar untuk dokter peminta dan petugas, `GET /blood-orders` mendukung saringan komponen beserta angka diminta/diberikan; **(3)** migration `BE-BD-017` diberi wewenang eksplisit dan diterapkan ke database pengembangan yang dipakai uji; **(4)** roadmap dan traceability sinkron; **(5)** preflight frontend dan backend bersih kembali (`HEAD == origin`, working tree bersih) |
| **Catatan backend** | Sepuluh endpoint tersedia per `BE-BD-003`: tujuh dari kontrak, ditambah `filters/metadata`, `summary`, dan `status-history`. **Tidak ada** endpoint suntingan order — `BloodOrder : Update` tidak punya endpoint kontrak `v4`. Rincian di [laporan](../task/report/backend/BE-BD-003.md) bagian 4 dan 7 |
| **Reuse** | `BD-CAP-021` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-003` ✅ [BE], **`BE-BD-017` ✅ [BE]**, **`BE-BD-018` ✅ [BE]**, **`BE-BD-019` ✅ [BE] — selesai dan terbukti runtime 23 September 2026**. **Ketergantungan `BE-BD-019` bersifat PARSIAL:** ia menahan **hanya** bagian penyaring tanggal pada layar `FE-BD-01`. Seluruh bagian `FE-BD-002` yang lain — pembuatan order, golongan darah diminta, penahanan ganda, kategori pembatalan, pemenuhan, daftar kerja, dan pembatalan — **tidak tertahan** dan dapat diselesaikan serta divalidasi runtime lebih dulu. Asalnya `BD-UI-GAP-004` ditutup Opsi B (`DEC-BD-059`, `DEC-BD-060`, `Sukmagp` 23 September 2026). **Riwayat:** `BE-BD-017` dan `BE-BD-018` keduanya ⛔ [BE] pada 18 September 2026. **Riwayat:** `BE-BD-003` ✅ [BE] saja |
| **Acceptance** | Order ganda tertahan beserta alasannya (kewajiban layar `FE-BD-003`); kategori alasan pembatalan **sesuai peran**; **kewajiban layar `FE-BD-001`** — golongan darah yang **diminta** pada order terlihat jelas berbeda dari golongan darah **hasil pemeriksaan** pada layar `FE-BD-02`. Contoh: order meminta PRC golongan A+, sedangkan hasil pemeriksaan sah pasien B+; keduanya tampil dengan label berbeda, bukan satu kolom "Golongan darah". Kewajiban ini diserap dari `BD-UI-GAP-003`, disetujui 18 September 2026 |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Empat keadaan layar digambar: kosong, memuat, berisi, gagal |

---

### ✅ `FE-BD-003` — Petugas mengelola permintaan PMI dan penerimaan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — 23 September 2026. Empat acceptance layar terbukti runtime, `PASS` seluruhnya, nol skenario gagal.** Dijalankan langsung agent di browser sungguhan (Playwright/Chromium) terhadap backend lokal di atas `QuilvianNewDevSukma`; aksi buat permintaan dan catat penerimaan dilakukan **lewat layar**, bukan lewat API. Bukti: `AC-BD-005` minta 3 terima 2 → `Diterima sebagian`, sisa 1; `AC-BD-006` permintaan kedua ditolak `422` dan kalimat backend tampil utuh — backend nol `errors` terstruktur sehingga nol yang diparsing layar; `AC-BD-009` diterima 0, status `Diminta`, riwayat penerimaan kosong; `AC-BD-031` minta 2 datang 3 → `Terpenuhi`, sisa **0 bukan −1**, berlebih 1, peringatan diturunkan dari `totalExcessQuantity` bukan dari kalimat. **`AC-BD-014` dicatat BACKEND-ONLY** — terbukti tidak terjangkau dari UI: order darah tanpa baris ditolak `400` oleh model validation `[MinLength(1)]`, sehingga penjaga `VAL-BD-007` pada jalur PMI tidak punya jalan menyala dari layar; **nol workaround dibuat**. Lint `0 error`; build `exit 0`, `✓ Compiled successfully in 36.1s`, **368/368** halaman (naik dari 367); `git diff --check` bersih. 9 berkas frontend baru + 1 diubah; **nol komponen `base-features/` baru**. Keputusan `DEC-BD-061` dan `DEC-BD-062` menutup dua gap bentuk layar sebelum satu baris kode ditulis. ([laporan](../task/report/frontend/FE-BD-003.md)). **Riwayat:** belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026). |
| **Outcome** | Petugas membuat permintaan ke PMI dan mencatat penerimaan, termasuk penerimaan berlebih |
| **Layar** | `FE-BD-03` |
| **Kontrak** | api-contract `v4` — Provider Request |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-004` ✅ [BE] — selesai 11 September 2026, roadmap backend revisi 9. `BE-BD-015` ✅ [BE] — pemilik `AC-BD-023`/`032` sejak revisi 9, jalur `PendingReview` yang ditampilkan layar. Keputusan `DEC-BD-061` dan `DEC-BD-062` ✅ (`Sukmagp` 23 September 2026) menutup dua gap bentuk layar |
| **Acceptance** | **Diperluas 23 September 2026 (`Sukmagp`).** Layar wajib menutup: **`AC-BD-005`** minta 3 PRC diterima 2 → `Diterima sebagian`, sisa 1; **`AC-BD-006`** permintaan baru untuk kebutuhan yang sama ditolak `422` `VAL-BD-006` beserta alasannya; **`AC-BD-009`** permintaan terkirim tetapi belum diterima fisik → stok tidak bertambah dan layar tidak menyatakannya diterima; **`AC-BD-014`** permintaan tanpa jumlah kantong ditolak `VAL-BD-007`; **`AC-BD-031`** minta 2 datang 3 → `Terpenuhi`, sisa **0 bukan −1**, ketiga kantong tercatat. **`AC-BD-022`/`AC-BD-023` dicatat sebagai integrasi**, bukan kriteria layar: efek `ClosedEncounter` dan `PendingReview` diturunkan backend (`BE-BD-015`), dan layar hanya wajib menampilkannya apa adanya. **Riwayat:** "Penerimaan termasuk kelebihan tercatat dan tidak membuat sisa negatif" |
| **Risk/owner** | Sedang / BDRS |

---

### ✅ `FE-BD-010` — Daftar dan pencatatan tindakan Bank Darah

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 23 September 2026** ([laporan](../task/report/frontend/FE-BD-010.md)). Source lengkap di frontend `a5f4be551` (dasar kerja `1bdaf6bd6`): 14 berkas baru dan 1 diubah, mencakup layar daftar, layar detail, pencatatan, dan penyelesaian. **Validasi runtime `PASS` 16 dari 16** di browser Chromium sungguhan atas hasil `next build` standalone (`npx playwright test tests/e2e/blood-bank-procedure-screen.spec.mjs`, 24,7 detik): `R1`/`R1b` pencatatan mengirim tepat tiga isian dan nol harga/tarif/unit/kelas; `R2` `Emitted` dan `Replayed` dinyatakan berhasil; `R3` `RejectedByBilling`, `OutcomeUnknown`, dan `ReconciliationRequired` tetap **Selesai** dengan nol klaim keberhasilan; `R3d` `BillingHandoff` kosong hanya menyatakan tindakan selesai; `R3e` muat ulang tidak memunculkan status Billing dari `GET`; `R4a`/`R4b`/`R4c` kelima butir hak akses menahan tombolnya; `R5a` `complete` tanpa badan permintaan; `R5b` nol `resend-cost-fact` dan nol kirim ulang otomatis. Validasi lain: `npm run lint:errors` `PASS`, `npm run build` `PASS` (kode keluar `0`, kedua route terdaftar di `routes-manifest.json`), `npm run test:unit` 1561 dari 1568 lulus — kedelapan test baru milik task ini lulus, dan ketujuh kegagalan lain terbukti sudah ada sebelum task ini. **Keempat acceptance criteria dan DoD terbukti**, nol butir tersisa. **Riwayat:** 🟡 selesai sebagian 23 September 2026 — source lengkap, runtime belum terbukti. **Riwayat:** Belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026); nol source tindakan di frontend `6640a5e7`. **Riwayat (18 September 2026, sebelum approval):** disarankan dijalankan sesudah `BD-UI-GAP-001` diputuskan, karena bila pemilik memilih menambah tombol kirim ulang, cakupan task ini ikut bertambah. **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED — `BE-BD-012` menunggu `BE-BD-003`; tertahan `G4` sampai 10 September 2026 |
| **Outcome** | Petugas Bank Darah melihat daftar tindakan, mencatat tindakan atas satu order, lalu menyatakannya selesai. Hasil penyerahan biaya ke Billing pada saat itu terbaca jujur |
| **Layar** | `FE-BD-07` |
| **Trace** | `DEC-BD-021`, `DEC-BD-034`, `DEC-BD-048`, `DEC-BD-049`, **`DEC-BD-016`** (approved 17 September 2026) |
| **Kontrak** | api-contract `v4` — Blood Bank Procedure: `GET /`, `GET /{id}`, `POST /`, `POST /{id}/complete`, termasuk delta 17 September 2026 (`complete` membawa `BillingHandoff`) |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-012` ✅ [BE] — selesai 11 September 2026 ([laporan](../task/report/backend/BE-BD-012.md)); **`BE-BD-013` ✅ [BE]** — selesai 17 September 2026 ([laporan](../task/report/backend/BE-BD-013.md)). `BE-BD-013` ditambahkan pada revisi 8 karena ia mengubah arti jawaban `complete`, endpoint yang dipanggil task ini |
| **Acceptance** | (1) Daftar, pencatatan, dan penyelesaian tindakan dapat dijalankan dari layar `FE-BD-07` memakai endpoint kontrak. (2) Sesudah `POST /{id}/complete`, layar **membaca `BillingHandoff`** dari jawaban dan **menampilkan hasil penyerahan apa adanya**, termasuk pesan dari backend. (3) Layar **tidak pernah** menyatakan penyerahan ke Billing berhasil bila `BillingHandoff.Kind` bukan `Emitted` atau `Replayed`. Bila `BillingHandoff` kosong, layar hanya menyatakan tindakan selesai. (4) Layar **MUST NOT** menyediakan `resend-cost-fact` sebagai aksi biasa, **MUST NOT** mengarang status Billing yang tetap dari `GET` (selalu `null`), dan **MUST NOT** mengirim ulang ke Billing secara otomatis. Butir (2)–(4) adalah keputusan pemilik `Sukmagp` 18 September 2026 (`BD-UI-GAP-001` Opsi A). **Dasarnya kontrak:** `complete` tetap `200` walau Billing menolak atau hasilnya belum pasti, karena `200` berarti *tindakan tersimpan selesai*, bukan *tagihan terkirim*. **Contoh:** Billing menolak karena folio kunjungan sudah ditutup. Jawabannya `200`, `BillingHandoff.Kind = RejectedByBilling`, pesan "penyerahan fakta biaya ke Billing memerlukan tinjauan". Layar menyatakan tindakan selesai **dan** menampilkan pesan itu, bukan "tindakan selesai dan tagihan terkirim". Rupa penampilannya — toast, banner, atau badge — `DEV_DISCRETION` |
| **Di luar cakupan** | (a) **Kolom atau halaman status penyerahan biaya yang tetap.** Tidak dapat dibangun: `GET /` dan `GET /{id}` selalu memulangkan `BillingHandoff` `null`, dan status penyerahan tinggal di ledger Billing (`integration-contract.md` §3.2). Membangunnya menuntut perubahan backend. (b) **Tombol kirim ulang biaya** (`POST /{id}/resend-cost-fact`). **Diputuskan tidak ada** — `BD-UI-GAP-001` Opsi A, `Sukmagp` 18 September 2026. Endpoint itu tetap kemampuan API teknis untuk pemulihan, dipakai di luar layar ini. **Riwayat:** menunggu keputusan pemilik `BD-UI-GAP-001`. (c) **Pembalikan biaya saat koreksi.** Tidak ada, sesuai `DEC-BD-034` dan `INV-BD-024`. (d) **Kirim ulang otomatis ke Billing.** Dilarang, keputusan pemilik yang sama |
| **Risk/owner** | Sedang / BDRS |
| **Catatan** | Sejak 17 September 2026 penyelesaian tindakan menyerahkan **tepat satu** fakta biaya `BloodBank` / `BloodBankCharge` ke Billing, berapa pun kantong yang diberikan. Frontend tidak mengirim satu pun isian Billing. **Riwayat:** tindakan dicatat **tanpa** penyaluran biaya ke Billing — penyaluran adalah `BE-BD-013` future scope |

---

### ✅ `FE-BD-012` — Penyimpanan dan perpindahan lokasi kantong

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 24 September 2026** ([laporan](../task/report/frontend/FE-BD-012.md)). Source lengkap di frontend, **belum di-commit** di atas `99cabdbe8`: 8 berkas diubah dan 3 baru — preset **Belum Disimpan** (`unitStatus=0`) dan **Lokasi Nonaktif** (`inactiveLocation=true`, api-contract `v5` `D6` dari `BE-BD-020` ✅) pada `FE-BD-04`, tombol **Tetapkan Lokasi**/**Pindahkan Lokasi** dan **Riwayat Penempatan** pada `FE-BD-05`, dijaga `AvailableActions` + `BloodUnit : Store` + `BloodStorageLocation : Read`. **Keputusan pemilik `Sukmagp` 24 September 2026:** `D1` riwayat tanpa kolom pelaku, gap `placedByName` dicatat; `D2` dua preset eksklusif; `D3` opsi lokasi lewat service tanpa Redux; `D4` acceptance `A1`–`A9`. **Validasi ulang runtime 13 dari 13 `PASS`** (atas permintaan pemilik, spec diperkuat untuk `A3` Tetapkan, `A5`, `A6`, `A7-422`; nol source fitur diubah; validasi pertama 9 dari 9) di Chromium sungguhan terhadap **backend sungguhan** hasil build `8bc7b512` (`QuilvianNewDevSukma`, fixture dipulihkan dan diverifikasi identik), termasuk `409` dan `422 VAL-BD-060` asli dari backend; regresi `FE-BD-004` 16 dari 16; `lint:errors` `PASS`; `test:unit` 7 test baru lulus, 7 kegagalan lama tetap; `build` `PASS` (370 halaman). Nol butir DoD dikecualikan. **Riwayat:** belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026), lalu tertahan menunggu `BE-BD-020` 23–24 September 2026. Nol source kantong di frontend `6640a5e7`. `BE-BD-015` ✅ selesai 11 September 2026 (roadmap backend revisi 10). **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED — `BE-BD-015` tertahan lewat `BE-BD-004` |
| **Outcome** | Petugas menempatkan dan memindahkan kantong, dan kantong yang tertahan tersaring jelas |
| **Layar** | `FE-BD-04`, `FE-BD-05` (parsial) |
| **Kontrak** | api-contract `v4` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-015` ✅ [BE] ([laporan](../task/report/backend/BE-BD-015.md)). **Riwayat:** `BE-BD-015` 🟡 selesai sebagian 11 September 2026, sebelum roadmap backend revisi 10 |
| **Acceptance** | `FE-BD-010` saringan `Received` dan lokasi nonaktif **wajib**; `FE-BD-011` kolom lokasi beserta penandanya |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | **Bukan** daftar kerja keempat — saringan menempel pada daftar yang sudah ada |

---

### ✅ `FE-BD-004` — Petugas mengalokasikan kantong dan membatalkan alokasi

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 24 September 2026** ([laporan](../task/report/frontend/FE-BD-004.md)). Source lengkap di frontend, **belum di-commit** di atas `a5f4be551`: 14 berkas baru dan 1 diubah — layar daftar kantong `FE-BD-04` beserta saringan `PendingReview` (daftar kerja #2), layar detail `FE-BD-05` sebatas alokasi dan pembatalan alokasi, dan butir menu **Kantong Darah** berpenjaga `BloodUnit : Read`. **Keputusan pemilik `Sukmagp` 24 September 2026:** `G1` layar kantong minimal dibangun di task ini, sedangkan penetapan/perpindahan lokasi dan saringan `inactiveLocation` tetap `FE-BD-012`; `G2` butir menu; `G3` nol validasi komponen/golongan darah di frontend; `G4` nol task backend baru, celah jumlah alokasi per baris dicatat sebagai risiko; `G5` delapan acceptance. **Validasi runtime `PASS` 16 dari 16** di Chromium sungguhan atas hasil `next build` standalone (`npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1`, 42,5 detik, validasi ulang 24 September 2026; jalan pertama 14 dari 14), dengan akun non-SuperAdmin: `R6a` daftar termuat dan klik dua kali membuka detail; `R4f` tanpa `BloodUnit : Read` halaman menampilkan akses ditolak dan butir menu tersembunyi; `R1` alokasi mengirim tepat `bloodOrderLineId` + `version`; `R1b` baris beda komponen tetap ditawarkan; `R2` pembatalan memakai `category=AllocationCancellation`, mengirim tepat `reasonCode` + `version`, dan status akhir `Tersedia`/`Menunggu keputusan` dibaca dari backend; `R4a`–`R4e` gerbang `BloodUnit : Allocate`, `BloodOrder : Read`, `BloodBankReason : Read`; `R5a` `409` menutup dialog dan memuat ulang; `R5b` `422` tampil apa adanya; `R6` `unitStatus=5`; `R6b` butir menu; `R7` nol tombol dan nol panggilan penyelesaian `PendingReview`. Validasi lain: `npm run lint:errors` `PASS`, `npm run build` `PASS` (370 halaman, kedua route terdaftar), `npm run test:unit` 1569 dari 1576 lulus — kedelapan test baru lulus, ketujuh kegagalan sama persis dengan yang tercatat pada `FE-BD-010`. **Kedelapan acceptance `G5` dan DoD terbukti**, nol butir tersisa. **Riwayat:** belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026); nol source di frontend `6640a5e7`. **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED — `BE-BD-006` tertahan lewat `BE-BD-015` |
| **Layar** | `FE-BD-05` (parsial) |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-006` ✅ [BE]. **Riwayat:** `BE-BD-006` ⛔ |
| **Acceptance** | Daftar `PendingReview` **wajib ada** (`FE-BD-002`) |
| **Risk/owner** | Sedang / BDRS |
| **DoD** | Worklist #2 tersedia |

---

### `FE-BD-005` 🟡 — Golongan darah, bukti kecocokan, pemberian, dan jalur darurat

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **Sebagian — 24 September 2026** ([laporan](../task/report/frontend/FE-BD-005.md)). Atas keputusan pemilik `Sukmagp` 24 September 2026, dikerjakan pada detail kantong yang sudah ada: catat bukti kecocokan, berikan, dan jalur darurat. Gerbang pemberian dinilai backend, dan penolakannya ditampilkan apa adanya. `lint:errors` `PASS`, `build` `PASS` (370 halaman), `test:unit` 1595 test — 1588 lulus, termasuk 12 test baru; 7 kegagalan lama sama dengan baseline. **Validasi runtime `R0`–`R9`: 14 dari 14 `PASS`** di Chromium terhadap backend sungguhan (`QuilvianNewDevSukma`), termasuk `422 VAL-BD-079/020/065/066`, `409`, dan gerbang hak akses. Cacat UX pemilih dialog yang ditemukan (daftar terbuka lagi sesudah dipilih) sudah diperbaiki lewat markup pembungkus saja, lalu `lint:errors`/`build` `PASS` dan `R3`/`R9` diulang `PASS`. Gap backend proyeksi gerbang pemberian dan gerbang golongan darah dicatat sebagai **backlog**, tidak dibuka sebagai task (keputusan pemilik 24 September 2026). **Belum terpenuhi:** layar `FE-BD-06` golongan darah (di luar cakupan keputusan), `FE-BD-013` gerbang terisi sesuai keadaan kantong, `FE-BD-007` penanda konflik, dan `FE-BD-008` penanda kedaluwarsa sebelum Berikan — backend tidak memulangkan data gerbang, dan frontend dilarang menghitungnya. `FE-BD-021` sebagian: tombol Berikan tertutup dengan pesan `VAL-BD-079` sesudah vonis pertama backend. DoD kartu `NOT APPLICABLE` (kartu tanpa baris DoD). **Riwayat:** Belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026). Nol source di frontend `6640a5e7`. Ketiga pasangan backend ✅. **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED SEBAGIAN — dependency-nya tiga task dengan keadaan berbeda |
| **Rincian dependency** | `BE-BD-005` ✅ **SELESAI** · `BE-BD-007` ✅ **SELESAI** 16 September 2026 · `BE-BD-008` ✅ **SELESAI** 16 September 2026. **Riwayat:** `BE-BD-007` ⛔ · `BE-BD-008` ⛔ |
| **⚠️ Jangan dipecah sendiri** | Memecah task ini menjadi dua **mengubah scope roadmap** dan menuntut persetujuan pemilik. Roadmap ini **tidak** memecahnya. Sejak ketiga pasangan backend ✅, alasan lama untuk memecahnya pun hilang. **Riwayat:** bagian pencatatan golongan darah secara teknis mengikuti `BE-BD-005` yang terbuka, tetapi bagian bukti kecocokan dan pemberian tertahan rantai `BE-BD-007`/`BE-BD-008` |
| **Outcome** | Petugas mencatat golongan darah, bukti kecocokan beserta hasilnya, lalu memberikan kantong; jalur darurat terbaca jelas dan berbeda dari jalur normal |
| **Layar** | `FE-BD-05`, `FE-BD-06` (parsial) |
| **Kontrak** | api-contract `v4` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-005` ✅ [BE], `BE-BD-007` ✅ [BE], `BE-BD-008` ✅ [BE] |
| **Acceptance** | `FE-BD-021` hasil tidak cocok **menutup tombol Berikan** dengan pesan yang benar; `FE-BD-018` peran penerbit dipilih sendiri; `FE-BD-013` gerbang pemberian terbaca. **Diserap dari `BD-UI-GAP-003`, disetujui 18 September 2026:** kewajiban layar `FE-BD-007` — penanda konflik golongan darah terlihat dan menahan tombol yang menuntut golongan darah sah; `FE-BD-008` — penanda bukti kecocokan yang lewat masa berlaku terlihat **sebelum** tombol Berikan ditekan; `FE-BD-012` — penolakan karena lokasi nonaktif (`VAL-BD-065`) menyebut **lokasi**, bukan bukti kecocokan. Contoh `FE-BD-012`: kantong di "Kulkas BDRS-2" yang sudah dinonaktifkan ditolak diberikan; pesannya meminta petugas memindahkan kantong, bukan mencatat bukti baru |
| **Catatan layar bersama** | Layar pemeriksaan `FE-BD-06` dibagi dengan `FE-BD-009`. Builder `FE-BD-011` mencatat bahwa `FE-BD-009` membutuhkan kerangka layar yang dibangun task ini ([laporan FE-BD-011](../task/report/frontend/FE-BD-011.md) bagian 8, langkah berikutnya butir 4). Karena itu urutan yang disarankan menaruh task ini lebih dulu. Ini **bukan** dependency keras: kartu `FE-BD-009` tetap boleh membangun kerangka itu sendiri bila dijalankan lebih dulu. **Kewajiban layar `FE-BD-019`** dipegang task **terakhir** di antara task ini dan `FE-BD-009` yang menyentuh layar `FE-BD-06` (pemetaan `BD-UI-GAP-003` yang disetujui). Menurut urutan yang disetujui itu `FE-BD-009`; kewajiban ini baru berpindah ke kartu ini bila `FE-BD-009` selesai lebih dulu |
| **Risk/owner** | **Tinggi / klinis & BDRS** |

---

### ✅ `FE-BD-009` — Penyelesaian konflik di dalam layar pemeriksaan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 24 September 2026** ([laporan](../task/report/frontend/FE-BD-009.md)). Keputusan pemilik `Sukmagp` 24 September 2026: `B0` cakupan = konflik golongan darah (bukan penyelesaian kantong, milik `FE-BD-007`); `B1` task ini membangun kerangka minimal `FE-BD-06` — butir menu **Pemeriksaan Golongan Darah**, route, daftar, detail; `B2` hanya Validasi dan Penyelesaian konflik, pencatatan sampel/hasil tidak termasuk; `B3` seluruh alasan aktif tanpa kategori, kategori alasan konflik jadi **backlog**; `B4` pemeriksaan ulang ditunjuk validator dari seluruh pemeriksaan pasien tanpa saringan frontend; `B5` nama pasien untuk tampilan dari `GET /patients/{id}`. Kedua acceptance dan DoD terbukti: tindakan hanya di detail `FE-BD-06`; `FE-BD-019` lewat dua pemeriksaan hak akses terpisah. `npm run lint:errors` `PASS` (0 error; 4 warning `set-state-in-effect` berpola sama dengan hook referensi); `npm run build` `PASS`, 371 halaman; `npm run test:unit` 1605 test — 1598 lulus termasuk 10 test baru, 7 kegagalan lama sama dengan baseline. **Validasi runtime `R0`–`R8`: 11 dari 11 `PASS`** di Chromium terhadap backend sungguhan (`QuilvianNewDevSukma`), termasuk `422` saat menunjuk pihak konflik, isi permintaan tepat tiga isian, dan `R4a`–`R4c` pemisahan tombol. Data uji permanen pada pasien IKBAL YULIYANTO dibiarkan atas keputusan pemilik. **Riwayat:** Belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026). Nol source di frontend `6640a5e7`. Blocker backend-nya hilang sejak 9 September 2026: `BE-BD-011` selesai ([laporan](../task/report/backend/BE-BD-011.md)), termasuk endpoint `POST /conflict-resolution` dan butir hak akses `BloodGroupExam : ResolveConflict` yang terpisah dari `Validate`. **Riwayat:** 🟡 PENDING — SIAP DIJADWALKAN sejak 9 September 2026 |
| **Kenapa dibedakan (riwayat)** | Dulu ini satu-satunya task frontend terblokir yang **tidak** menunggu provider number-series. `BE-BD-005` lalu `BE-BD-011` sudah dikerjakan, dan task ini terbuka persis seperti yang diperkirakan |
| **Outcome** | Validator klinis menyelesaikan konflik golongan darah **di dalam layar pemeriksaan**, bukan lewat daftar kerja tersendiri |
| **Layar** | `FE-BD-06` |
| **Kontrak** | api-contract `v4` |
| **Reuse** | `BD-CAP-021` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-011` ✅ [BE] |
| **Acceptance** | Penyelesaian konflik hidup di layar pemeriksaan, bukan daftar kerja keempat (kewajiban layar `FE-BD-009`). **Kewajiban layar `FE-BD-019`**, diserap dari `BD-UI-GAP-003` dan disetujui 18 September 2026: tombol Validasi (`BloodGroupExam : Validate`) dan tindakan Penyelesaian konflik (`BloodGroupExam : ResolveConflict`) tampil **terpisah** menurut hak akses. Contoh: petugas BDRS berwenang validasi melihat tombol Validasi, tetapi tidak melihat tindakan Penyelesaian konflik. **Aturan pemilik yang disetujui:** `FE-BD-019` dipegang task terakhir di antara `FE-BD-005` dan task ini yang menyentuh layar `FE-BD-06`. Menurut urutan yang disetujui, itu task ini; bila task ini selesai lebih dulu, kewajibannya berpindah ke `FE-BD-005` dan perpindahannya dicatat pada kedua kartu |
| **Catatan hak akses** | Tombol Validasi dan tindakan Penyelesaian konflik wajib tampil terpisah menurut hak akses (`03-frontend-architecture.md` §3, `FE-BD-019`). Untuk itu layar harus tahu hak akses pengguna **sebelum** menampilkan tombol. Sumbernya ada di backend (`GET /api/v1/Auth/permissions`), tetapi frontend belum punya pembacanya — lihat `BD-UI-GAP-002`. **Koreksi 18 September 2026:** pembacanya sudah ada sejak `622a46f41` (`permission-slice.jsx`, `use-permission.jsx`), dan `FE-BD-006` menambahkan keputusan ketat `selectPermissionDecision` / `useEffectivePermissions` yang dapat dipakai ulang di sini — terbukti runtime lewat `FE-BD-006` ✅ 18 September 2026, ter-commit `fbe29f6d1` di `origin/sukmagpV2` (riwayat: belum di-commit; belum terbukti runtime). Memakainya pada layar ini tetap pekerjaan task ini |
| **Risk/owner** | Tinggi / klinis |
| **DoD** | **Bukan** daftar kerja keempat — penyelesaian konflik hidup di layar pemeriksaan |

---

### ✅ `FE-BD-007` — Penyelesaian `PendingReview`, tiga tombol tiga penjaga

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 24 September 2026** ([laporan](../task/report/frontend/FE-BD-007.md)). `FE-BD-020` terbukti: tiga tombol **Alihkan**, **Kembalikan ke PMI**, dan **Tidak Layak** masing-masing dijaga butirnya sendiri (`ResolveReallocate`, `ResolveReturn`, `ResolveNotUsable`) ditambah kelayakan `AvailableActions` — runtime `R2a`–`R2g` `PASS`. Keputusan pemilik `Sukmagp` 24 September 2026: `G1` tombol Alihkan sesuai kontrak; status `Reallocated` tanpa jalan keluar ke pemberian dicatat **backlog kontrak/backend**, tanpa jalan pintas frontend (terbukti `R6`: backend hanya menawarkan pindah lokasi); `G2` penyebab menunggu keputusan dari `IsExcess` dan `ReasonNote` transisi terakhir ke `PendingReview`; `G5` Kembalikan ke PMI dan Tidak Layak memakai dialog bahaya bertuliskan permanen. `npm run lint:errors` `PASS` (0 error; 3 warning lama di berkas `FE-BD-004`/`005`); `npm run build` `PASS`, 371 halaman; `npm run test:unit` 1611 test — 1604 lulus termasuk 6 test baru, 7 kegagalan lama sama dengan baseline. **Validasi runtime 13 dari 13 skenario `PASS`** di Chromium terhadap backend sungguhan (`QuilvianNewDevSukma`), termasuk `422 VAL-BD-064` pada pengalihan kantong di lokasi nonaktif dan isi permintaan tepat per endpoint. Run 1 memuat satu kegagalan **cacat spec** (balapan waktu) sebelum tombol konfirmasi ditekan; diperbaiki dan diulang `PASS`. Baris DoD `NOT APPLICABLE` (kartu tanpa DoD). **Riwayat:** Belum dikerjakan — siap dijadwalkan (revisi 8 disetujui 18 September 2026). Nol source di frontend `6640a5e7`. `BE-BD-009` ✅ 17 September 2026. **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED — `BE-BD-009` tertahan lewat `BE-BD-006`/`007` |
| **Layar** | `FE-BD-05` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-009` ✅ [BE]. **Riwayat:** `BE-BD-009` ⛔ |
| **Acceptance** | `FE-BD-020` — ketiga tombol punya penjaga hak akses **terpisah**, sesuai `DEC-BD-043` dan `DEC-BD-045` |
| **Catatan hak akses** | `AvailableActions` pada detail kantong menyatakan **kelayakan**, bukan izin (`BbkBloodUnitService.cs`, komentar `AvailableActionsFor`). Karena itu `FE-BD-020` tidak dapat dipenuhi hanya dari `AvailableActions`. Layar membutuhkan hak akses pengguna, dan pembacanya belum ada di frontend — lihat `BD-UI-GAP-002`. **Koreksi 18 September 2026:** pembacanya sudah ada sejak `622a46f41`, dan `FE-BD-006` menambahkan keputusan ketat `selectPermissionDecision` / `useEffectivePermissions` yang dapat dipakai ulang di sini — terbukti runtime lewat `FE-BD-006` ✅ 18 September 2026, ter-commit `fbe29f6d1` di `origin/sukmagpV2` (riwayat: belum di-commit; belum terbukti runtime). Memakainya pada layar ini tetap pekerjaan task ini |
| **Risk/owner** | Sedang / BDRS |

---

### `FE-BD-008` — Koreksi dua langkah dan daftar tunggakan bukti darurat

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan — siap dijadwalkan** (revisi 8 disetujui 18 September 2026). Nol source di frontend `6640a5e7`. `BE-BD-010` ✅ ([laporan](../task/report/backend/BE-BD-010.md)). **Riwayat:** 🟡 PENDING (refresh dependency 17 September 2026); ⛔ BLOCKED — menunggu `BE-BD-010`, yang **READY tetapi belum diimplementasikan** (per 17 September 2026; `OQ-BD-014` ditutup `DEC-BD-051`); `BE-BD-010` tertahan lewat `BE-BD-007` |
| **Layar** | `FE-BD-05`, `FE-BD-04` |
| **Gerbang** | `G1` ✅ |
| **Dependency** | `BE-BD-010` ✅ [BE]. **Riwayat:** `BE-BD-010` 🟡 READY — belum diimplementasikan; sebelumnya ⛔ |
| **Acceptance** | Koreksi menuntut **dua langkah**; daftar tunggakan bukti darurat tersedia (worklist #3). **Diserap dari `BD-UI-GAP-003`, disetujui 18 September 2026:** kewajiban layar `FE-BD-016` — koreksi yang menunggu persetujuan tampil sebagai **menunggu**, dan angka pemenuhan order **tidak berubah** sampai keputusan turun; `FE-BD-017` — tombol Setujui dan Tolak **tersembunyi** pada koreksi yang diajukan pengguna yang sedang membuka layar, ditentukan oleh perbandingan pelaku, bukan hak akses. Contoh: order meminta 2 kantong dan 2 sudah diberikan; petugas A mengajukan koreksi atas satu kantong; layar tetap menulis 2 dari 2 sampai dokter BDRS memutuskan, dan petugas A sendiri tidak melihat tombol Setujui |
| **Catatan biaya** | Koreksi yang disetujui **tidak** membalik biaya tindakan (`DEC-BD-034`, `AC-BD-058` terbukti pada `BE-BD-013`). Layar koreksi tidak boleh menjanjikan pembatalan tagihan |
| **Risk/owner** | Sedang / BDRS |

---

## 4. Slice

| Slice | Outcome | Task | Keadaan |
| --- | --- | --- | --- |
| **1 — Setup master dan menu** | Master Bank Darah dapat disiapkan, dan layar terjangkau dari menu | `FE-BD-001`, `FE-BD-011`, `FE-BD-006` | ✅ **Selesai 18 September 2026** — `FE-BD-001`, `FE-BD-011`, dan `FE-BD-006` ✅; `FE-BD-006` ditutup sesudah uji runtime pemilik R1–R8 `PASS` ([laporan](../task/report/frontend/FE-BD-006.md)). **Riwayat:** `FE-BD-001` dan `FE-BD-011` ✅, `FE-BD-006` sebagian — kriteria keduanya terbukti otomatis, menunggu bukti runtime; kriteria kedua `FE-BD-006` menunggu pembaca hak akses frontend (`BD-UI-GAP-002`); `FE-BD-001` ✅, `FE-BD-011` dan `FE-BD-006` sebagian; sebelumnya ketiganya sebagian, nol selesai |
| **2 — Order darah, permintaan PMI, dan tindakan** | Pintu masuk permintaan darah dan penutup biaya berjalan dari layar | `FE-BD-002`, `FE-BD-003`, `FE-BD-010` | ✅ **Selesai 23 September 2026** — ketiganya ✅ dan terbukti runtime; `FE-BD-010` ditutup dengan 16 dari 16 pemeriksaan runtime `PASS` ([laporan](../task/report/frontend/FE-BD-010.md)). **Riwayat:** 🟡 hampir selesai — `FE-BD-010` menunggu bukti runtime. **Riwayat:** Belum dikerjakan |
| **3 — Kantong darah, pemberian, dan penyelesaiannya** | Kantong disimpan, dialokasikan, diberikan, diselesaikan, dan dikoreksi dari layar | `FE-BD-012`, `FE-BD-004`, `FE-BD-005`, `FE-BD-009`, `FE-BD-007`, `FE-BD-008` | 🟡 **Berjalan per 24 September 2026** — 4 dari 6 selesai: `FE-BD-004` ✅ ([laporan](../task/report/frontend/FE-BD-004.md)), `FE-BD-012` ✅ ([laporan](../task/report/frontend/FE-BD-012.md)), `FE-BD-009` ✅ ([laporan](../task/report/frontend/FE-BD-009.md), runtime 11 dari 11 `PASS`), dan `FE-BD-007` ✅ ([laporan](../task/report/frontend/FE-BD-007.md), runtime 13 dari 13 `PASS`). Sebagian: `FE-BD-005` 🟡 ([laporan](../task/report/frontend/FE-BD-005.md), runtime 14 dari 14 `PASS`; 4 butir kartu tertahan data backend). Belum dikerjakan: `FE-BD-008`. **Riwayat:** 3 dari 6 selesai; belum dikerjakan `FE-BD-007`, `FE-BD-008`. **Riwayat:** 2 dari 6 selesai; belum dikerjakan `FE-BD-009`, `FE-BD-007`, `FE-BD-008`. **Riwayat:** Belum dikerjakan: `FE-BD-005`, `FE-BD-009`, `FE-BD-007`, `FE-BD-008`; 1 dari 6 — `FE-BD-012` tertahan menunggu `BE-BD-020`; Belum dikerjakan |

---

## 5. Coverage gap requirement ke test

| Gap | Keadaan | Akibat pada rencana |
| --- | --- | --- |
| ~~**`BD-UI-GAP-001` — tombol kirim ulang biaya**~~ — **DITUTUP 18 September 2026, keputusan pemilik `Sukmagp`: Opsi A** (dicatat sebagai gap pada hari yang sama) | Backend menyediakan `POST /blood-bank-procedures/{id}/resend-cost-fact` (`BE-BD-013`). Endpoint ini jalur pemulihan, bukan langkah lifecycle, dan hak aksesnya sama dengan `complete` (`BloodBankProcedure : Update`). **Tidak ada** dokumen yang disetujui mewajibkan tombolnya di layar: `03-frontend-architecture.md` tidak memuatnya, `AC-BD-026/027/058` seluruhnya uji backend dan sudah terbukti, dan `DEC-BD-016` tidak menyebut layar. Laporan `BE-BD-013` menulis "layar kirim ulang **bila dikehendaki**". **Diputuskan: tidak ada tombol kirim ulang biaya di layar.** | **Ditutup.** Acceptance `FE-BD-010` kini terkunci pada butir (2)–(4) kartunya. `POST /resend-cost-fact` tetap kemampuan API teknis untuk pemulihan. Nol perubahan pada `03-frontend-architecture.md` dan kontrak. **Riwayat:** keputusan pemilik terbuka; Opsi B (tombol) akan menuntut delta arsitektur `FE-BD-07` dan approval ulang |
| ~~**`BD-UI-GAP-002` — pembaca hak akses di frontend**~~ — **DITUTUP PENUH 18 September 2026 lewat `FE-BD-006`**, sesudah uji runtime pemilik `Sukmagp` R1–R8 `PASS`. **Riwayat:** BARU 18 September 2026, menggantikan catatan "katalog permission tidak ada"; lalu tertutup pada tingkat implementasi 18 September 2026 dengan bukti runtime menunggu | **Koreksi 18 September 2026:** pembaca frontend **sudah ada** sejak commit `622a46f41` (8 September 2026, pemakai pertama layar Lab `LabRejectionReason : SystemFlag`) dan masuk `sukmagpV2` lewat merge `d7059b563` sesudah `6640a5e7` diperiksa — sinkronisasi roadmap tidak menemukannya. Semantiknya longgar (belum termuat/gagal = boleh, `isSuperAdmin` = boleh), sehingga `FE-BD-006` menambahkan keputusan ketat `selectPermissionDecision`/`useEffectivePermissions`, invalidasi sesi, dan penolak permintaan ganda tanpa mengubah perilaku Lab ([laporan](../task/report/frontend/FE-BD-006.md) §1.1, §3.3). **Riwayat:** Backend kini punya sumbernya: `GET /api/v1/Auth/permissions` → `EffectivePermissionSet` berisi pasangan `Resource` / `Action` (ada di `77f60c88`, masuk sesudah `d07dcf3`). Frontend `6640a5e7` belum memakainya. Seluruh frontend masih memakai pola reaktif `AccessDeniedGate`, yaitu menampilkan pesan sesudah backend menolak `403`. `filterMenuItemsByRole` masih stub | Menahan kriteria kedua `FE-BD-006`. Juga dibutuhkan kewajiban mengikat `FE-BD-019` (`FE-BD-009`), `FE-BD-020` (`FE-BD-007`), dan aturan `03-frontend-architecture.md` §4 "tombol yang tak berhak **MUST** disembunyikan". Membuat pembaca bersama berstatus `NEW` menurut `base-component-decision-gate`, sehingga diputuskan pemilik saat build task pertama yang membutuhkannya. **Tidak** menjadi dependency keras, karena sumber backend-nya sudah ada. **Keputusan pemilik 18 September 2026:** tetap gap implementasi, **bukan** penahan approval roadmap. Pembacanya diputuskan dan dibangun pada task disetujui pertama yang benar-benar membutuhkannya |
| ~~**`BD-UI-GAP-003` — kewajiban layar mengikat belum tertulis di kartu**~~ — **DISERAP 18 September 2026 ke roadmap revisi 8 yang disetujui** | Tujuh kewajiban berlabel "mengikat" pada `03-frontend-architecture.md` belum disebut pada baris Acceptance kartu mana pun: `FE-BD-001` golongan darah diminta vs hasil, `FE-BD-007` penanda konflik menahan, `FE-BD-008` penanda bukti lewat masa berlaku, `FE-BD-012` pesan penolakan menyebut lokasi, `FE-BD-016` koreksi menunggu tidak mengubah angka pemenuhan, `FE-BD-017` tombol keputusan tersembunyi pada koreksi sendiri, dan `FE-BD-019` Validasi terpisah dari Penyelesaian konflik | Kewajiban itu tetap mengikat builder lewat arsitektur. Yang kurang hanya jejaknya di roadmap. **Usulan pemilik task**, mengikuti layar yang dibangun: `FE-BD-001` → `FE-BD-002`; `FE-BD-007`, `FE-BD-008`, `FE-BD-012` → `FE-BD-005`; `FE-BD-016`, `FE-BD-017` → `FE-BD-008`; `FE-BD-019` → task terakhir dari `FE-BD-005` / `FE-BD-009` yang menyentuh layar `FE-BD-06`. **Disetujui `Sukmagp` 18 September 2026 tanpa koreksi**, dan kini tertulis pada baris Acceptance kartu `FE-BD-002`, `FE-BD-005`, `FE-BD-008`, dan `FE-BD-009`. Nol task baru, nol task ID berubah. **Riwayat:** usulan ini menunggu approval bersama revisi 8 |
| **Batas yang bukan gap: status penyerahan biaya tidak dapat dibaca ulang** — **BARU 18 September 2026** | `BillingHandoff` hanya ada pada jawaban `complete` dan `resend-cost-fact`; `GET` selalu `null` (`api-contract.md` delta 17 September 2026) | Frontend hanya dapat menampilkan hasil penyerahan **saat itu juga**. Daftar "tindakan yang biayanya belum sampai" tidak dapat dibangun tanpa perubahan backend. Tidak dijadwalkan |
| **Cacat sisa backend: `HeldUnitCount` `0` pada balasan penonaktifan** | Balasan `PATCH /blood-storage-locations/{id}/status` tetap memetakan ulang entity tanpa angka dari service di `77f60c88`. Milik `BE-BD-015` ✅, tidak dibuka ulang | **Tidak** menahan `FE-BD-011`: konfirmasi membaca `GET /{id}` sebelum penonaktifan, yang angkanya benar. **Riwayat (14 September 2026):** dicatat sebagai penahan satu acceptance `FE-BD-011` |
| Layar operasional Bank Darah belum ada | Per `6640a5e7`, frontend memuat **hanya tiga layar master** (komponen darah, alasan terkendali, lokasi penyimpanan) beserta slice dan hook-nya. Nol route, nol slice Redux, dan nol pemanggilan ke endpoint `api/v1/health-services/blood-bank-management/**`. **Riwayat:** `BD-CAP-020` `Missing` — nol route, nol slice Redux, nol pemanggilan ke-27 endpoint yang sudah jadi | Seluruh layar operasional dibangun baru. Ini memang scope-nya, bukan gap yang menahan |
| Bukti uji frontend | Nol test frontend Bank Darah | Ditetapkan per task saat eksekusi mengikuti `test-policy.md` frontend |
| Rupa layar | `DEV_DISCRETION` | Sengaja tidak dikunci. Yang dikunci sumber data, hak akses, dan keadaan layar wajib |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Butir | Alasan |
| --- | --- |
| Task backend | Ada di [backend-roadmap.md](backend-roadmap.md) |
| Penelusuran requirement → test | Ada di [requirement-traceability.md](requirement-traceability.md) |
| Keputusan menu, route, tab/modal/drawer, warna, tata letak | `DEV_DISCRETION` — bukan wewenang roadmap |
| Layar atau kolom status penyerahan biaya Billing yang tetap | Tidak dapat dibangun dengan kontrak saat ini, karena `GET` tindakan tidak membawa `BillingHandoff`. Hasil penyerahan hanya ditampilkan saat `complete`, sebagai bagian `FE-BD-010`. **Riwayat:** "Layar penyaluran biaya Billing mengikuti `BE-BD-013` yang berada di future scope" — tidak berlaku sejak `BE-BD-013` ✅ 17 September 2026 |
| Tombol kirim ulang biaya | **Diputuskan tidak ada** — `BD-UI-GAP-001` Opsi A, `Sukmagp` 18 September 2026. `POST /resend-cost-fact` tetap kemampuan API teknis untuk pemulihan. **Riwayat:** menunggu keputusan pemilik; tidak ditambahkan diam-diam |
| Task frontend baru untuk Billing | Tidak dibuat. Endpoint `complete` sudah milik layar `FE-BD-07`, sehingga penampilan hasilnya melekat pada `FE-BD-010`. Endpoint baru di backend bukan alasan membuat task baru |
