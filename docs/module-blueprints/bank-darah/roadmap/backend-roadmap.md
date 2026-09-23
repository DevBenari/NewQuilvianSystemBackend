# Roadmap Delivery Backend — Modul Bank Darah

## Metadata

```yaml
module_id: bank-darah
module_name: BloodBankManagement
entity_prefix: Bbk
blueprint_id: BD-BP-001
blueprint_shape: SINGLE
blueprint_root: docs/module-blueprints/bank-darah/
roadmap_revision: 12
revision_12_scope: CONTRACT_V5_BLOOD_ORDER_NEW_TASKS
revision_12_status: APPROVED — Sukmagp 2026-09-19. Riwayat: DRAFT 2026-09-18
revision_12_note: >-
  Amendment kontrak v5 untuk FE-BD-002. Pemilik Sukmagp memutuskan jalur C lalu B pada
  2026-09-18: set kontrak direvisi v4 -> v5 (DEC-BD-055..058), lalu dua task backend BARU
  ditetapkan skill perencanaan. BE-BD-017: golongan darah diminta pada BbkBloodOrder beserta
  migration ADD COLUMN nullable dan VAL-BD-085 (AC-BD-103..106). BE-BD-018: errors terstruktur
  VAL-BD-001, CancellationReasonCategory turunan, ketergantungan hak akses BloodOrder : Cancel ->
  BloodBankReason : Read, dan proyeksi komponen + jumlah diberikan pada GET /blood-orders
  (AC-BD-107..112). Keduanya semula BLOCKED oleh gerbang baru G5 (approval naskah v5), BE-BD-017
  juga oleh persetujuan proses klinis DEC-BD-055; keduanya tertutup 2026-09-19 dan kedua task kini
  siap dijadwalkan dengan urutan kanonik BE-BD-017 -> migration -> BE-BD-018 -> FE-BD-002. BE-BD-003 TIDAK dibuka ulang dan seluruh bukti
  BE-BD-001..016 tidak ditulis ulang. Nol task lama berubah ID, acceptance, atau dependency.
revision_11_scope: ACCEPTANCE_REHOME_AC_BD_071_AND_VERIFICATION_POLICY
revision_11_note: >-
  Koreksi tata kelola saja, dijalankan SEBELUM BE-BD-006 dikerjakan. Nol source backend,
  nol source frontend, nol migration, nol test, nol task dieksekusi. Dua hal berubah dan
  satu hal dicatat.
  PERTAMA — satu acceptance criteria berpindah pemilik. AC-BD-071 (kantong PendingReview di
  lokasi nonaktif dicoba dialihkan ke pasien lain, ditolak VAL-BD-064) DILEPAS dari BE-BD-006
  dan diteruskan ke BE-BD-009. Alasannya bukan selera, melainkan endpoint: "pengalihan" pada
  AC-BD-071 adalah POST /api/v1/health-services/blood-bank-management/blood-units/{id}/reallocate,
  dan endpoint itu milik BE-BD-009 — contracts/api-contract.md baris reallocate memakai hak akses
  BloodUnit : ResolveReallocate, 03-frontend-architecture.md menempatkan tombol Alihkan pada
  penyelesaian PendingReview, dan arsip revisi 3 sudah mencantumkan reallocate di scope BE-BD-009.
  Nol task baru dibuat: pemilik reallocate memang sudah ada.
  KEDUA — BE-BD-006 tetap pemilik penuh Blood Unit Allocation dan Cancel Allocation, dan dengan
  AC-BD-071 keluar, task itu kini dapat diselesaikan penuh TANPA mengimplementasikan reallocate.
  KETIGA — kebijakan verifikasi repository yang berlaku dicatat resmi di bagian 0.1. Bukti test
  historis milik task yang sudah SELESAI dipertahankan apa adanya dan TIDAK ditulis ulang.
  Keputusan Sukmagp 2026-09-12. Nol task baru, nol dependency, nol kontrak, nol aturan bisnis
  berubah. Kartu BE-BD-007, BE-BD-008, dan BE-BD-010 sengaja tidak disentuh.
revision_10_scope: ACCEPTANCE_FORWARD_BE_BD_015
revision_10_note: >-
  Satu acceptance criteria bertambah pemilik, nol lainnya berubah. Verifikasi final bagian
  alokasi AC-BD-060, AC-BD-068, dan AC-BD-070 diteruskan dari BE-BD-015 ke BE-BD-006, karena
  endpoint allocate lahir di sana. AC-BD-060 dan AC-BD-068 sudah tercantum pada BE-BD-006 sebelum
  revisi ini (terlihat sejak arsip revisi 3) sehingga tidak diduplikasi; AC-BD-070 ditambahkan. Bukti tingkat gerbang yang sudah
  dibuktikan BE-BD-015 (VAL-BD-063, VAL-BD-064, gerbang terbuka kembali sesudah perpindahan)
  dipertahankan dan tidak diulang. Preseden revisi 9 (BE-BD-004 -> BE-BD-015/BE-BD-006).
  Keputusan Sukmagp 2026-09-11. Akibatnya BE-BD-015 selesai dan BE-BD-006 siap dijadwalkan.
  Nol task baru, nol dependency, kontrak, maupun aturan bisnis berubah. Kartu BE-BD-007
  sampai BE-BD-010 sengaja tidak disentuh.
revision_9_scope: ACCEPTANCE_FORWARD_BE_BD_004
revision_9_note: >-
  Tiga acceptance criteria berpindah task, nol lainnya berubah. AC-BD-023 dan AC-BD-032 (bagian
  perpindahan kantong ke PendingReview) diteruskan dari BE-BD-004 ke BE-BD-015, dan AC-BD-033
  (penolakan VAL-BD-033 pada alokasi) ke BE-BD-006, karena penegakannya hidup pada kemampuan
  penyimpanan dan alokasi milik kedua task itu. Preseden BE-BD-002 -> BE-BD-003 dan BE-BD-014 ->
  BE-BD-015. Keputusan Sukmagp 2026-09-11. Akibatnya BE-BD-004 selesai dan BE-BD-015 siap
  dijadwalkan. Nol task baru, nol dependency, kontrak, maupun aturan bisnis berubah.
revision_8_scope: ACCEPTANCE_REHOME_BE_BD_012
revision_8_note: >-
  Satu task berubah acceptance criteria-nya, satu task menerima dua kriteria pindahan. BE-BD-012
  kini memakai AC-BD-098 sampai AC-BD-102 (pencatatan tindakan, resolusi tarif, salinan tarif,
  penyelesaian, tanpa Billing); AC-BD-026 dan AC-BD-058 dipindah ke BE-BD-013 karena keduanya
  baru dapat dibuktikan ketika fakta biaya terkirim ke Billing. Dasarnya keputusan pemilik
  DEC-BD-048 dan DEC-BD-049 (Sukmagp, 2026-09-11). Nol task baru, nol dependency berubah, nol
  kontrak berubah. BE-BD-012 kembali siap dijadwalkan.
revision_4_scope: SPLIT_BE_FE_ONLY
revision_5_scope: CROSS_MODULE_DEPENDENCY_AND_STATUS_ONLY
revision_5_note: >-
  Revisi 5 tidak menambah, menghapus, memecah, atau mengubah satu pun task, acceptance
  criteria, kontrak, maupun dependency antar-task Bank Darah. Yang berubah hanya dua:
  penandaan status BE-BD-005/BE-BD-011 menjadi selesai, dan penulisan ulang gerbang G4
  dari penghalang organisasi menjadi dependency pengiriman lintas modul ke PLT-SLICE-01.
  Nol requirement yatim ditemukan saat perencanaan ulang; seluruh kemampuan sudah punya
  task sejak revisi 2.
revision_6_scope: BLOCKER_REFRESH_ONLY
revision_7_scope: PLATFORM_DEPENDENCY_CONCRETE
revision_7_note: >-
  Nol task Bank Darah berubah. Blueprint PLT-SLICE-01 disetujui 9 September 2026 dan
  roadmap Platform terbit, sehingga dependency G4 berpindah dari "menunggu modul Platform"
  menjadi "menunggu task PLT-BE-003". Urutan aman ditambahkan: tunggu PLT-BE-004 lulus di
  PostgreSQL sebelum sembilan task Bank Darah dijadwalkan.
revision_6_note: >-
  Nol task Bank Darah berubah, lagi. Yang diperbarui hanya penahan G4 di sisi Platform:
  OQ-PLT-012 dan OQ-PLT-013 tertutup 9 September 2026 lewat DEC-PLT-009 (Area Platform)
  dan DEC-PLT-010 (prefix Num), sehingga penahan terdekat berpindah menjadi approval
  blueprint PLT-SLICE-01 yang masih DRAFT. OQ-PLT-014 ditambahkan sebagai penahan
  implementasi, bukan penahan perencanaan.
status: APPROVED
status_note: >-
  19 September 2026: revisi 12 DISETUJUI Sukmagp bersama kontrak v5; G5 tertutup. Riwayat: DRAFT.
  18 September 2026: revisi 12 (amendment kontrak v5, BE-BD-017 dan BE-BD-018) berstatus DRAFT
  sampai pemilik meninjau naskahnya. Revisi 11 tetap APPROVED (Sukmagp 2026-09-12) dan seluruh
  task BE-BD-001..016 beserta buktinya tidak berubah. Riwayat: APPROVED.
  Revisi 7 disetujui Sukmagp 2026-09-10. Pada hari yang sama gerbang G4 ditutup atas
  pernyataan pemiliknya, Andry, setelah PLT-BE-003 dan PLT-BE-004 selesai. Nol task,
  acceptance criteria, kontrak, maupun dependency antar-task berubah; yang berubah hanya
  status gerbang dan status task.
approval_gate: BLUEPRINT_APPROVED
contract_version: v5 (approved — Sukmagp 2026-09-19). Riwayat v5 draft 2026-09-18; v4 (approved, kini superseded)
backend_source_sha: 2bd9fc2addb373873494cb7342316fec143312d5
backend_source_sha_note: >-
  18 September 2026: dibaca pada 2bd9fc2a (sama dengan origin/sukmagp, working tree bersih).
  Rentang 77f60c88..2bd9fc2a hanya docs/, nol berkas source. Riwayat 55ac6ab:
  Rentang 55ac6ab..f0d6855 mengubah nol berkas .cs, sehingga SHA ini masih menggambarkan
  source yang ter-commit. PERINGATAN: working tree memuat 17 berkas .cs milik BE-BD-005
  dan BE-BD-011 yang BELUM ter-commit, sehingga source aktual sudah melampaui SHA mana pun
  di dokumen ini.
backend_branch: sukmagp
frontend_source_sha: fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e
frontend_source_sha_note: >-
  18 September 2026: commit FE-BD-006 di origin/sukmagpV2. Riwayat: f79af16847c99961842081f707bc0c4ff6c2d93b.
frontend_branch: sukmagpV2
decision_revision: 13
domain_architecture_revision: 6
owners:
  - "Product/Domain: pemilik proses BDRS"
  - "API/Arsitektur backend: pemilik arsitektur backend"
  - "Security/Privacy: pemilik keamanan platform"
approved_by:
  - "Sukmagp — set kontrak v4 dan roadmap revisi 2, 2026-09-03"
  - "Sukmagp — roadmap backend revisi 7, 2026-09-10"
  - "Sukmagp — acceptance criteria BE-BD-012 (AC-BD-098..102), pemindahan AC-BD-026/058 ke BE-BD-013, DEC-BD-048/049, 2026-09-11"
  - "Sukmagp — penerusan AC-BD-023/032 ke BE-BD-015 dan AC-BD-033 ke BE-BD-006 (roadmap revisi 9), 2026-09-11"
  - "Sukmagp — penerusan verifikasi final bagian alokasi AC-BD-060/068/070 dari BE-BD-015 ke BE-BD-006 (roadmap revisi 10), 2026-09-11"
  - "Sukmagp — pelepasan AC-BD-071 dari BE-BD-006 ke pemilik reallocate BE-BD-009 dan pencatatan kebijakan verifikasi repository (roadmap revisi 11), 2026-09-12"
  - "Sukmagp — kontrak v5, roadmap backend revisi 12, BE-BD-017 dan BE-BD-018, DEC-BD-055..058 termasuk persetujuan proses klinis DEC-BD-055, 2026-09-19"
approved_at: "2026-09-19"
approval_note: >-
  Approval 2026-09-03 berlaku atas roadmap revisi 2. Revisi 3 menambahkan gerbang G4
  dan revisi 4 memecah roadmap menjadi backend dan frontend; approval tidak berpindah
  otomatis. Pada 2026-09-10 Sukmagp menyetujui roadmap BACKEND revisi 7. Approval itu
  tidak menjangkau frontend-roadmap.md, yang tetap FORWARD-TEST / DRAFT sampai
  diputuskan tersendiri. Pada 2026-09-12 Sukmagp menyetujui roadmap BACKEND revisi 11,
  yang cakupannya tata kelola saja: perpindahan pemilik AC-BD-071 dan pencatatan kebijakan
  verifikasi. Approval itu tetap tidak menjangkau frontend-roadmap.md.
supersedes: roadmap/archive/revision-3/00-delivery-plan.md
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Sinkronisasi dokumentasi 14 September 2026.** Status BE-BD-006 selesai dan 9/9 mengikuti ringkasan hasil Claude yang diteruskan pemilik; bukan hasil eksekusi baru dalam review ini. Adendum dipulihkan pada [BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9 dengan batas bukti primer, build berparameter, dan otorisasi. Audit output pemilik menemukan 30 deklarasi permission unik; baseline 39 belum direkonsiliasi ulang ke seluruh endpoint. Revisi roadmap 11, kontrak v4, dependency, dan acceptance tidak diubah oleh sinkronisasi ini.


**Roadmap ini tidak memberi wewenang menulis source.** Approval membuka **penjadwalan** task.
Wewenang menulis diberikan terpisah, satu task satu wewenang, lewat `build-module-backend`.

**Migration, eksekusi database di luar dev pemilik, deployment, dan publikasi Git tetap wewenang
tersendiri** yang diminta per tindakan.

**Preflight QBE dan kesesuaian engineering diselesaikan pada waktu eksekusi** dari `AGENTS.md`
backend target dan dokumen engineering canonical — bukan di dokumen ini.

**Gerbang `G4` tertutup 10 September 2026.** `BE-BD-003` ✅ **selesai 11 September 2026** ([laporan](../task/report/backend/BE-BD-003.md)), sehingga `BE-BD-004` dan `BE-BD-012` kini siap dijadwalkan. `BE-BD-004` ✅ **selesai 11 September 2026** ([laporan](../task/report/backend/BE-BD-004.md)) — keenam kriteria yang tetap miliknya terbukti; `AC-BD-023`/`032` diteruskan ke `BE-BD-015` dan `AC-BD-033` ke `BE-BD-006` pada roadmap revisi 9, sehingga `BE-BD-015` terbuka. `BE-BD-015` ✅ **selesai 11 September 2026 — roadmap revisi 10** ([laporan](../task/report/backend/BE-BD-015.md)) — seluruh kriteria yang tetap miliknya terbukti; verifikasi final bagian alokasi `AC-BD-060/068/070` diteruskan ke `BE-BD-006`, sehingga `BE-BD-006` terbuka. `BE-BD-006` ✅ **SELESAI 14 September 2026** ([laporan](../task/report/backend/BE-BD-006.md)) — **kesembilan runtime acceptance terbukti** lewat panggilan API sungguhan ditambah verifikasi read-only database; `dotnet build` `0 Error(s)` / `193 Warning(s)`; migration terterapkan (`migrations list` 0 pending); index unik terfilter terverifikasi ada di PostgreSQL; QBE Strict `PASS`. **Akibatnya `BE-BD-007` kini siap dijadwalkan.** `BE-BD-007` ✅ **SELESAI 16 September 2026** ([laporan](../task/report/backend/BE-BD-007.md)) — **kedua belas business acceptance terbukti runtime** ditambah **ketujuh kode `VAL-BD-*` terbukti terkirim** dengan pesan persis `validation-matrix.md`; otorisasi dibuktikan dengan aktor non-SuperAdmin sungguhan; `dotnet build` `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS`; nol migration baru. Satu bug source ditemukan lewat bukti runtime dan diperbaiki minimal atas persetujuan pemilik. **Akibatnya `BE-BD-008`, `BE-BD-009`, dan `BE-BD-010` kini siap dijadwalkan.** `BE-BD-008` ✅ **SELESAI 16 September 2026** ([laporan](../task/report/backend/BE-BD-008.md)). `BE-BD-009` ✅ **SELESAI 17 September 2026** ([laporan](../task/report/backend/BE-BD-009.md)) — kesembilan acceptance terbukti runtime termasuk `AC-BD-071` (`422 VAL-BD-064` lewat gerbang alokasi yang sama), kelima kode `VAL-BD-016/064/080/081/082` persis kontrak, tiga butir hak akses terbukti terpisah dengan aktor non-SuperAdmin, migration `0 pending`, build `0 Error(s)`, QBE Strict `PASS`. `BE-BD-010` tetap 🟡 siap dijadwalkan. **Riwayat `BE-BD-006`:** 🟡 PARTIAL — READY FOR RUNTIME VALIDATION per 14 September 2026: implementation, migration artifact, dan static verification selesai, kesembilan runtime acceptance `PENDING`; dikerjakan 12 September 2026 — source lengkap, `dotnet build` `NOT RUN` atas instruksi pemilik, migration belum dibuat, kesembilan kriteria `NOT EXECUTED`; 🟡 siap dijadwalkan sejak roadmap revisi 10. **Riwayat `BE-BD-015`:** 🟡 selesai sebagian — 9 dari 12 kriteria penuh, bagian alokasi `AC-BD-060/068/070` menunggu endpoint `BE-BD-006`. `BE-BD-012` ✅ **selesai 11 September 2026** ([laporan](../task/report/backend/BE-BD-012.md)). Task bertanda ⛔
tetap tidak boleh dijadwalkan — kini karena task pendahulunya belum selesai, bukan karena gerbang.
Rinciannya di bagian 2. **Riwayat:** sampai 10 September 2026 gerbang ini menahan sembilan dari lima
belas task backend.

### 0.1 Kebijakan verifikasi repository yang berlaku

**Dicatat pada roadmap revisi 11, 12 September 2026.** Bagian ini tidak mengubah satu pun
acceptance criteria. Ia menjawab satu pertanyaan yang selama ini dijawab berbeda-beda antar task:
**apa yang sah dipakai sebagai bukti bahwa sebuah task backend Bank Darah benar-benar bekerja.**

Ini bukan pelonggaran standar. Standarnya tetap sama — setiap acceptance criteria wajib punya bukti
yang dapat ditunjuk. Yang berubah hanya **bentuk buktinya**, karena bentuk repository-nya memang
berubah.

**Keadaan repository per 12 September 2026, hasil pemeriksaan langsung:**

| Yang diperiksa | Temuan |
| --- | --- |
| Folder `Tests/` di root repository backend | **Tidak ada** |
| Project di `QuilvianSystemBackend.sln` | **Satu** — `QuilvianSystemBackend.csproj`, project produksi |
| Project test terpisah | **Tidak ada** — sejalan dengan `AGENTS.md` backend, yang sudah mencatat "Project test terpisah belum terdeteksi ketika aturan ini dibuat" |

**Catatan keadaan workstation 14 September 2026 (dilaporkan):** sisa lokal `Tests/obj` ditemukan walaupun tidak ada source test tracked. Build polos dilaporkan gagal; build dengan `-p:DefaultItemExcludes="Tests/**"` dilaporkan berhasil. Tabel keadaan 12 September di atas tetap histori. Tidak ada cleanup dijalankan dalam sinkronisasi dokumen ini, dan larangan membuat test otomatis baru tetap berlaku.

**Aturan yang berlaku sejak sekarang:**

| Aturan | Isi |
| --- | --- |
| Folder `Tests/` di root | **DILARANG.** Jangan membuatnya, jangan memulihkannya, jangan memindahkan apa pun ke dalamnya |
| Project atau berkas test otomatis baru | **JANGAN DIBUAT.** Ini termasuk project xUnit/NUnit/MSTest baru, berkas `*Tests.cs` baru, dan fixture database uji baru |
| `dotnet test` | **TIDAK DIPERSYARATKAN.** Sebuah task tidak boleh dinyatakan gagal, tertunda, atau "belum terbukti" hanya karena `dotnet test` tidak dijalankan. Perintah itu juga tidak boleh dilaporkan lulus kalau tidak benar-benar dijalankan dan sukses |

**Bentuk bukti yang sah menggantikannya — seluruhnya wajib nyata, bukan klaim:**

| Bentuk bukti | Contoh konkret yang diharapkan |
| --- | --- |
| Bukti kompilasi/build produksi | `dotnet build QuilvianSystemBackend.csproj` selesai dengan `0 Error(s)`, beserta angka warning dan pembandingnya terhadap baseline. Ini bukti minimum yang selalu ada |
| Inspeksi EF dan skema | Nama migration yang lahir, hasil `has-pending-model-changes`, jumlah migration yang diterapkan, bentuk tabel/kolom/index yang benar-benar terbentuk, dan relasi pada `ApplicationDbContextModelSnapshot.cs` |
| Kesesuaian QBE | Preflight QBE dan dokumen engineering canonical, diselesaikan pada waktu eksekusi dari `AGENTS.md` backend target — persis seperti bagian 0 sudah menyatakan |
| Verifikasi manual terkendali: API | Panggilan endpoint sungguhan beserta HTTP method, path, payload samaran, status code, dan potongan response. Contoh: `POST /blood-units/{id}/allocate` atas kantong `Received` → `422` dengan kode `VAL-BD-063` |
| Verifikasi manual terkendali: DB | Query `SELECT` baca-saja atas database dev pemilik untuk memastikan baris, status, dan riwayat benar-benar berubah seperti yang dijanjikan |
| Batas keamanannya | Verifikasi manual hanya dijalankan **bila aman dieksekusi**. Eksekusi migration di luar dev pemilik, deployment, dan publikasi Git tetap wewenang tersendiri per tindakan, sebagaimana bagian 0 |

**Bukti historis dipertahankan, bukan ditulis ulang.** Laporan task yang sudah ✅ **SELESAI** —
`BE-BD-001`, `002`, `003`, `004`, `005`, `011`, `012`, `014`, `015`, dan sebagian `BE-BD-016` —
memuat angka test seperti "561/561", "231/231 Sqlite", dan "19/19 uji PostgreSQL". Angka-angka itu
**tetap berlaku sebagai catatan sejarah** atas apa yang benar-benar dijalankan pada saat task itu
dikerjakan. Revisi 11 **tidak menghapus, tidak mengedit, dan tidak mendiskreditkan** satu pun dari
angka itu. Yang dinyatakan hanya ini: **task berikutnya tidak dituntut menghasilkan angka sejenis.**

---

## 1. Cara membaca roadmap ini

Setiap task memakai tepat satu penanda:

| Penanda | Arti | Boleh dijadwalkan? |
| --- | --- | --- |
| ✅ | **SELESAI** — bukti penerimaan tercatat di `task/report/backend/` | Sudah selesai |
| 🟡 | **PENDING** — seluruh prasyaratnya terpenuhi, tinggal dikerjakan | **Ya** |
| ⛔ | **BLOCKED** — ada prasyarat yang belum tersedia | **Tidak** |

Penanda ⛔ **bukan** tanda rencana gagal. Ia menyatakan satu hal yang jujur: prasyaratnya belum ada,
dan menjalankannya sekarang akan menghasilkan kode yang melanggar kontrak sendiri.

---

## 2. Gerbang

| Gate | Isi | Pemilik | Keadaan |
| --- | --- | --- | --- |
| `G1` | Approval blueprint & set kontrak `v4` | Pemilik proses BDRS + arsitektur backend | ✅ **TERTUTUP** 2026-09-03 oleh `Sukmagp` |
| `G2a` | Pendaftaran prefix `Bbk` di registry | Pemilik registry engineering | ✅ **TERTUTUP** 2026-09-03, commit `ed7fba8` |
| `G2b` | Lifecycle registri `PLANNED` → `ACTIVE` | Pemilik registry engineering | ✅ **TERTUTUP** 2026-09-03, commit `8075784` |
| `G4` | Provider number-series yang dapat dipakai Bank Darah | Pemilik platform + pemilik kontrak engineering backend — `Andry` | ✅ **TERTUTUP** 2026-09-10 oleh `Andry` — bukti `PLT-BE-003` dan `PLT-BE-004`; riwayatnya di 2.1 |
| **`G5`** (revisi 12) | Approval **naskah** set kontrak `v5` — amendment Blood Order `DEC-BD-055`..`058` | `Sukmagp` | ✅ **TERTUTUP** 2026-09-19 oleh `Sukmagp`. **Riwayat:** ⛔ terbuka sejak 2026-09-18, menahan `BE-BD-017` dan `BE-BD-018` |

**`G4` tertutup 10 September 2026.** Pemilik gerbang, `Andry`, menyatakan gerbang ini tertutup.
Keterangan itu disampaikan `Sukmagp` pada hari yang sama, tanpa dokumen persetujuan tertulis yang
dilampirkan.

| Bukti | Keadaan |
| --- | --- |
| Provider bersama berdiri | ✅ `PLT-BE-003` — `NumberSeriesAllocator`, 9 September 2026 |
| Durabilitas dan antrean terbukti di PostgreSQL | ✅ `PLT-BE-004` — 6 dari 6 uji lulus di `QuilvianNewDevSukma`, 10 September 2026, commit `23fb65a` |
| Tabel `NumNumberSeries` ada di database pengembangan | ✅ di `QuilvianNewDevSukma`. `QuilvianNewDevTim01`, staging, dan production **belum** |

**Akibatnya:** `BE-BD-003` naik ke 🟡 dan siap dijadwalkan. `BE-BD-004` dan `BE-BD-012` tetap ⛔, kini
karena menunggu `BE-BD-003` — bukan lagi karena nomor. Enam task sesudahnya tetap ⛔ lewat rantai yang
sama. **Nol task, acceptance criteria, kontrak, maupun dependency antar-task berubah.**

Contoh supaya jelas: order darah wajib bernomor unik seperti `ORD-00000001`. Sebelum 10 September 2026
Bank Darah tidak punya mesin pemberi nomor yang sah. Sekarang punya, dan sudah terbukti bahwa dua
puluh permintaan nomor serentak menghasilkan dua puluh nomor berbeda, dan nomor dari pekerjaan yang
batal tidak diterbitkan lagi.

### 2.1 `G4` — kenapa sempat terbuka (riwayat)

Kontrak `v4` (`02-backend-architecture.md:487–488`) mewajibkan `OrderNumber`, `RequestNumber`, dan
`ProcedureNumber` dialokasikan provider number-series atomik, dan **melarang** `Count+1`/`Max+1`
(`QBE-CODE-002/003`). Kontrak menyebut provider itu *"yang sudah ada"*.

**Frasa itu terbantah bukti.** Audit terarah Platform pada `4a1da7d` menemukan — **tabel di bawah
adalah temuan 7 September 2026 dan sengaja dipertahankan apa adanya sebagai riwayat; keadaan
terkininya ada pada blok Pembaruan tepat sesudahnya:**

| Andaian | Kenyataan |
| --- | --- |
| Provider tinggal dipakai | Mesin atomik memang ada — `BillingNumberSeriesService` — tetapi **milik Billing**. Keempat method publiknya dipatok kunci deret `BILLING_*`; method generiknya `private`. **Nol pintu masuk untuk Bank Darah** |
| — | Menaikkannya menjadi milik bersama adalah `DEC-PLT-007`, berstatus **`draft`** |
| — | `PLT-SLICE-01` berstatus `BUSINESS_DECISION_REQUIRED`, terhalang `OQ-PLT-007`: Backend Engineering Contract Owner belum ditunjuk |

**Pembaruan 9 September 2026 — penghalang paling keras sudah hilang.** `OQ-PLT-007` **tertutup**:
pemilik kontrak engineering backend adalah **`Andry`**, ditunjuk eksplisit oleh pemilik kebutuhan.

Yang **belum** berubah, dan inilah sebab `G4` masih ⛔:

| Butir | Keadaan |
| --- | --- |
| `DEC-PLT-002`..`008` | ✅ **`approved`** oleh `Andry` 9 September 2026 — baris ini sudah tidak berlaku, dipertahankan sebagai riwayat |
| Provider bersama | **Belum ada satu baris pun.** `BillingNumberSeriesService` masih dipatok `BILLING_*` dengan method generik `private` |
| Roadmap Platform | ✅ **Ada sejak 9 September 2026** — 5 task backend + 1 frontend. Baris ini dipertahankan sebagai riwayat; keadaan terkini ada di bagian 6.1 |

**Jalan yang dipilih pemilik 9 September 2026: jalan pertama.**

1. ✅ **DIPILIH** — `PLT-SLICE-01` selesai lebih dulu, lalu Bank Darah memanggil provider bersama.
   Ini jalan yang sejalan `QBE-CODE-006`, yang mewajibkan alokasi atomik ber-scope duduk di
   **provider bersama**.
2. ❌ Bank Darah membuat deret sendiri (`BbkNumberSeries`). **Ditolak** — bertabrakan dengan
   `QBE-CODE-006`, dan Backend Engineering Contract berada **di atas** dokumen modul pada urutan
   presedensi `AGENTS.md`. `DEC-PLT-005` mempertegasnya: modul berhak menetapkan prefix dan format,
   **mesin alokasinya tidak**.
3. ❌ Amendment kontrak `v4`. **Ditolak** — `QBE-CODE-002/003` tetap melarang `Count+1`/`Max+1`.

**`PmiBagNumber` tidak terkena `G4`.** Nomor kantong datang dari PMI, bukan dibuat server
(`ASM-BD-003`, `02-backend-architecture.md:415`).

---

## 3. Ringkasan status

| Penanda | Jumlah | Task |
| --- | ---: | --- |
| ✅ SELESAI | **18** | `BE-BD-001`, `BE-BD-002`, `BE-BD-003`, `BE-BD-004`, `BE-BD-005`, `BE-BD-006`, `BE-BD-007`, `BE-BD-008`, `BE-BD-009`, `BE-BD-010`, `BE-BD-011`, `BE-BD-012`, `BE-BD-013`, `BE-BD-014`, `BE-BD-015`, `BE-BD-016`, **`BE-BD-017`**, **`BE-BD-018`**. **`BE-BD-017` dan `BE-BD-018` ✅ SELESAI 23 September 2026** — amendment kontrak `v5` tuntas: `AC-BD-103` sampai `AC-BD-112` seluruhnya terbukti; build `0 Error(s)` / `214 Warning(s)`; migration `BE-BD-017` terterapkan di `QuilvianNewDevSukma` nol tertunda; `BE-BD-018` nol migration; QBE Strict `PASS`; validasi runtime dijalankan pemilik dengan dua aktor non-SuperAdmin ([`BE-BD-017`](../task/report/backend/BE-BD-017.md), [`BE-BD-018`](../task/report/backend/BE-BD-018.md)). Bukti runtime berupa atestasi pemilik tanpa log primer. **Riwayat:** 16 sampai 23 September 2026. **`BE-BD-013` ✅ SELESAI 17 September 2026** — `DEC-BD-016` disetujui `Sukmagp` hari yang sama; `AC-BD-026`/`027`/`058` terbukti runtime lewat API sungguhan + verifikasi database; build `0 Error(s)` / `214 Warning(s)` (nol dari berkas task); `has-pending-model-changes` bersih, nol migration; QBE Strict `PASS` ([laporan](../task/report/backend/BE-BD-013.md)). **Riwayat:** 15 tanpa `BE-BD-013`. **`BE-BD-016` selesai 17 September 2026 — 38 butir identik source/kontrak/database ([laporan](../task/report/backend/BE-BD-016.md)).** **Riwayat:** 14 tanpa `BE-BD-016`. **`BE-BD-010` selesai 17 September 2026 — ketujuh acceptance terbukti runtime, migration `0 pending`, QBE Strict `PASS` ([laporan](../task/report/backend/BE-BD-010.md)).** **Riwayat:** 13 tanpa `BE-BD-010`. **`BE-BD-009` selesai 17 September 2026 — kesembilan acceptance dan kelima kode kontrak `VAL-BD-016/064/080/081/082` terbukti runtime, migration terterapkan `0 pending`, QBE Strict `PASS` ([laporan](../task/report/backend/BE-BD-009.md)).** **Riwayat:** 12 tanpa `BE-BD-009`. **`BE-BD-008` selesai 16 September 2026 — kesembilan acceptance dan kelima kode kontrak terbukti runtime, migration terterapkan `0 pending`, QBE Strict `PASS` ([laporan](../task/report/backend/BE-BD-008.md)).** **Riwayat:** 11 tanpa `BE-BD-008`. **`BE-BD-007` selesai 16 September 2026 — kedua belas business acceptance dan ketujuh kode kontrak terbukti runtime ([laporan](../task/report/backend/BE-BD-007.md)); build `0 Error(s)`, QBE Strict `PASS`.** **Riwayat:** 10 tanpa `BE-BD-007`. **`BE-BD-006` selesai 14 September 2026 — kesembilan runtime acceptance terbukti** ([laporan](../task/report/backend/BE-BD-006.md)). `BE-BD-001` dan `BE-BD-014` sempat diturunkan ke 🟡 pada 14 September 2026 karena regresi registrasi DI, lalu **dikembalikan ✅ pada hari yang sama** sesudah smoke validation lolos. `BE-BD-015` selesai pada roadmap revisi 10 ([laporan](../task/report/backend/BE-BD-015.md)); `BE-BD-004` selesai pada roadmap revisi 9 ([laporan](../task/report/backend/BE-BD-004.md)); `BE-BD-012` selesai 11 September 2026 ([laporan](../task/report/backend/BE-BD-012.md)). **Riwayat:** 8 sebelum revisi 10 |
| 🟡 SELESAI SEBAGIAN | **0** | Tidak ada. **Riwayat:** 2 — `BE-BD-017` dan `BE-BD-018` pada 23 September 2026, selama beberapa jam di antara penutupan bukti statik dan masuknya hasil validasi runtime; keduanya ✅ pada hari yang sama. **Riwayat (HISTORY):** 1 — `BE-BD-016` sampai ✅ 17 September 2026 — **30 deklarasi unik terhadap baseline 39** menurut audit source pemilik 14 September 2026; bukan bukti seeder DB/otorisasi runtime. **Riwayat:** 2 — `BE-BD-016` dan `BE-BD-015` (9 dari 12 kriteria) sebelum penerusan revisi 10; 1 — `BE-BD-016` sesudah revisi 9; sebelumnya 2, termasuk `BE-BD-004` — 6 dari 9 kriteria sebelum penerusan revisi 9 |
| 🟡 PENDING — siap dijadwalkan | **0** | Tidak ada sejak 23 September 2026, ketika `BE-BD-017` dan `BE-BD-018` berpindah ke SELESAI SEBAGIAN. **Riwayat:** 2 — **`BE-BD-017`** lalu **`BE-BD-018`**, siap dijadwalkan sejak 19 September 2026 (kontrak `v5` disetujui, `G5` tertutup), belum dikerjakan. **Riwayat:** 0 sampai 19 September 2026. **Riwayat:** 1 — `BE-BD-010` sampai ✅ 17 September 2026 — **READY, belum diimplementasikan.** Dependency tertutup; `OQ-BD-014` ditutup `DEC-BD-051` dan `VAL-BD-049` terdefinisi lewat `DEC-BD-052` (17 September 2026). Tidak dikerjakan pada sesi `BE-BD-009`. **Riwayat:** 2 — `BE-BD-009`, `BE-BD-010`, sampai `BE-BD-009` ✅ 17 September 2026. **Riwayat:** 3 termasuk `BE-BD-008`, sampai `BE-BD-008` ✅ 16 September 2026 **Riwayat:** 1 — `BE-BD-007`, penahannya `BE-BD-006` gugur 14 September 2026. **Riwayat `BE-BD-006` pada baris ini:** 🟡 PARTIAL — READY FOR RUNTIME VALIDATION per 14 September 2026, runtime acceptance 0 dari 9 `PENDING` ([laporan](../task/report/backend/BE-BD-006.md)), kini ✅. build, migration, dan QBE Strict lolos 13 September 2026; penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS`. **Riwayat:** dikerjakan 12 September 2026, source lengkap, build dan migration menunggu pemilik. **Riwayat:** 🟡 PENDING — siap dijadwalkan sejak roadmap revisi 10, 11 September 2026; sejak roadmap revisi 11 dapat diselesaikan penuh secara mandiri tanpa `reallocate`. **Riwayat:** 0 sesudah `BE-BD-015` 🟡 selesai sebagian; 1 — `BE-BD-015`, siap dijadwalkan sejak roadmap revisi 9; sebelumnya 0 sesudah `BE-BD-012` ✅; sebelumnya 1 — `BE-BD-012`, siap dijadwalkan kembali sejak roadmap revisi 8; sebelumnya terbuka setelah `BE-BD-003` selesai, lalu ⛔ pada hari yang sama |
| ⛔ BLOCKED | 0 | Tidak ada sejak 19 September 2026. **Riwayat:** 2 — `BE-BD-017` (menunggu `G5` dan persetujuan klinis `DEC-BD-055`) dan `BE-BD-018` (menunggu `G5`), 18 September 2026. **Riwayat:** 0 sejak `BE-BD-007` ✅ 16 September 2026 sampai revisi 12. **Riwayat:** 3 — `BE-BD-008`, `009`, `010`, seluruhnya lewat `BE-BD-007`, sampai `BE-BD-007` ✅ 16 September 2026 **Riwayat:** 4 termasuk `BE-BD-007`, sampai `BE-BD-006` ✅ 14 September 2026; 5 termasuk `BE-BD-006`, lewat rantai yang berawal dari `BE-BD-015`; sebelumnya 6 termasuk `BE-BD-015`; sebelumnya 7, termasuk `BE-BD-012` yang menunggu tiga keputusan ([laporan](../task/report/backend/BE-BD-012.md)) |
| — Future scope | 0 | Tidak ada. **Riwayat:** 1 — `BE-BD-013`, sampai `DEC-BD-016` disetujui dan task ✅ 17 September 2026 |
| **Total** | **18** | **18 selesai, nol tersisa — seluruh task backend Bank Darah tuntas 23 September 2026.** **Riwayat:** 16 selesai + 2 selesai sebagian; sebelumnya 16 selesai + 2 siap dijadwalkan. **Riwayat:** 16 selesai + 2 terblokir (18 September 2026); 16 sampai revisi 11 |

---

## 4. Urutan dependency

```text
✅ BE-BD-001 (master komponen darah + alasan terkendali)   SELESAI
✅ BE-BD-002 (flag IsAvailableForBloodOrder pada MstServiceUnit)   SELESAI
✅ BE-BD-014 (master lokasi penyimpanan darah)   SELESAI
✅ BE-BD-016 (seeder resource & action hak akses)   SELESAI 17 Sep 2026 — 38 butir kanonik, source = kontrak = database
       └── sisa baseline diperiksa ke pemakai sah; bukan membuat permission dummy

════════ JALUR TERBUKA — tidak menyentuh number-series ════════

✅ BE-BD-005 (pemeriksaan golongan darah)   SELESAI
       │      dep: G1 ✅, G2b ✅ · BbkBloodGroupExam memuat PatientId, bukan BloodOrderId
       └── ✅ BE-BD-011 (penyelesaian konflik golongan darah)   SELESAI
                  dep: G1 ✅, G2b ✅, BE-BD-005 ✅
                  └──> membuka FE-BD-009

════════ JALUR BEKAS G4 — provider nomor tersedia; G4 ✅ tertutup 10 Sep 2026 ════════

✅ BE-BD-003 (order darah)   SELESAI 11 Sep 2026; OrderNumber dari provider
       │      dep: G1 ✅, G2b ✅, BE-BD-001 ✅, BE-BD-002 ✅, G4 ✅
       ├── ✅ BE-BD-012 (tindakan Bank Darah)   SELESAI 11 Sep 2026
       │
       └── ✅ BE-BD-004 (permintaan PMI + penerimaan + kantong lahir)   SELESAI 11 Sep 2026 — 3 AC diteruskan (revisi 9)
                  └── ✅ BE-BD-015 (penyimpanan & perpindahan kantong)   SELESAI 11 Sep 2026 — 3 AC diteruskan (revisi 10)
                             dep: BE-BD-004 ✅, BE-BD-014 ✅
                             └── ✅ BE-BD-006 (alokasi kantong)   SELESAI 14 Sep 2026 — 9 skenario dilaporkan lulus
                                        └── ✅ BE-BD-007 (bukti kecocokan + pemberian)   SELESAI 16 Sep 2026 — 12/12 AC + 7/7 kode kontrak terbukti runtime
                                                   │      dep: BE-BD-005 ✅, BE-BD-006 ✅
                                                   ├── ✅ BE-BD-008 (jalur darurat)   SELESAI 16 Sep 2026 — 9/9 AC + 5/5 kode kontrak terbukti runtime
                                                   ├── ✅ BE-BD-009 (penyelesaian PendingReview)   SELESAI 17 Sep 2026 — 9/9 AC + 5/5 kode kontrak terbukti runtime
                                                   │          dep: BE-BD-006 ✅, BE-BD-007 ✅
                                                   └── ✅ BE-BD-010 (koreksi dua tahap)   SELESAI 17 Sep 2026 — 7/7 AC + kode kontrak terbukti runtime

════════ FUTURE SCOPE ════════

✅ BE-BD-013 (penyaluran biaya ke Billing)   SELESAI 17 Sep 2026 — DEC-BD-016 disetujui · 3/3 AC terbukti runtime

════════ AMENDMENT KONTRAK v5 — revisi 12, APPROVED 19 Sep 2026 ════════

BE-BD-003 ✅ ─┬─> BE-BD-017 ✅ (golongan darah diminta + migration)
BE-BD-005 ✅ ─┤
G5 ✅ ─────────┤
DEC-BD-055 ✅ ─┘

BE-BD-003 ✅ ─┬─> BE-BD-018 ✅ (errors VAL-BD-001, kategori pembatalan, daftar kerja)
BE-BD-010 ✅ ─┤
G5 ✅ ─────────┘

G5         = naskah set kontrak v5 disetujui pemilik — tertutup 19 Sep 2026
DEC-BD-055 = persetujuan proses klinis atas DEC-BD-055 — disetujui 19 Sep 2026
```

**Gelombang eksekusi amendment `v5` (revisi 12).**

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | Seluruh prasyaratnya ✅ sejak 19 September 2026 | `BE-BD-017`, `BE-BD-018` |

**Urutan pelaksanaan kanonik — ditetapkan pemilik 19 September 2026.** Keduanya tidak saling bergantung
secara data dan tetap dua task terpisah, tetapi dikerjakan berurutan:

| Urutan | Langkah |
| ---: | --- |
| 1 | `BE-BD-017` |
| 2 | Penerapan migration `BE-BD-017` ke database pengembangan yang diberi wewenang eksplisit |
| 3 | `BE-BD-018` |
| 4 | `FE-BD-002` (roadmap frontend) |

**Riwayat (18 September 2026):** keduanya tanpa nomor gelombang — `BE-BD-018` menunggu `G5`, `BE-BD-017`
menunggu `G5` dan persetujuan klinis `DEC-BD-055`.

**Riwayat 9 September 2026 - jalur terbuka saat itu sudah habis.** `BE-BD-005` dan `BE-BD-011` selesai 9 September 2026 tanpa menyentuh
penomoran sama sekali, persis seperti yang direncanakan. Akibatnya **tidak ada lagi task backend yang
dapat dijadwalkan tanpa menutup `G4` lebih dulu** — kesembilan sisanya tertahan gerbang itu.

**Riwayat - diperbarui 10 September 2026 — `G4` tertutup.** Jalur yang tadinya tertahan kini dapat dimulai
dari ujungnya: `BE-BD-003` siap dijadwalkan, lalu `BE-BD-004` dan `BE-BD-012` terbuka begitu
`BE-BD-003` selesai, dan seterusnya mengikuti rantai di atas.

**Riwayat 11 September 2026 — roadmap revisi 10 (anotasi lama sudah diganti pada grafik terbaru).** `BE-BD-015` ✅ dan `BE-BD-006` 🟡 siap
dijadwalkan. Anotasi `dep:` di bawah `BE-BD-007` dan `BE-BD-009` pada grafik di atas masih menulis
`BE-BD-006 ⛔` — itu keadaan sebelum revisi 10, sengaja tidak disentuh karena pass ini dilarang
menyentuh `BE-BD-007` sampai `BE-BD-010`. Keadaan `BE-BD-006` yang berlaku ada pada node-nya dan
kartunya: 🟡. Bagi keempat task itu artinya tidak berubah: mereka tetap ⛔ lewat rantai dependency
yang kini berawal dari `BE-BD-006`.

**Yang tidak boleh paralel.** Seluruh cabang di bawah `BE-BD-003` berurutan dan tidak dapat
dipotong: kantong tidak dapat disimpan sebelum lahir, tidak dapat dialokasikan sebelum tersimpan, dan
tidak dapat diberikan sebelum dialokasikan.

---

## 5. Task

### ✅ `BE-BD-001` — Katalog komponen darah dan daftar alasan terkendali dapat dikelola

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 3 September 2026, **dipulihkan 14 September 2026**. Bukti: [laporan](../task/report/backend/BE-BD-001.md). `MstBloodComponent` 9 endpoint + seeder PRC/TC/FFP + 26 test; `MstBloodBankReason` 9 endpoint + seeder satu alasan tiap sepuluh kategori + 30 test. **Riwayat saat task selesai: dua migration dibuat, belum dijalankan**. **Diturunkan ✅ → 🟡 pada 14 September 2026** atas keputusan pemilik: registrasi DI `BloodComponentService` dan `BloodBankReasonService` hilang pada cabang berjalan lewat merge `27d737cd` (11 September 2026), sehingga kedua controller menjawab `500` pada setiap action dan DoD runtime-nya tidak lagi terpenuhi. **Dikembalikan ke ✅ pada hari yang sama** sesudah registrasi dipulihkan dan smoke validation lolos — sembilan endpoint/request smoke master yang dilaporkan menjawab `200`; daftar method/path belum dilampirkan, sehingga ini bukan klaim seluruh endpoint tiga controller telah diuji. Rincian: [BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.4 **Pembaruan yang dilaporkan 14 September 2026:** `migrations list` pada `QuilvianNewDevSukma` 0 pending dari 180; hanya berlaku untuk database dev yang disebut, bukan semua lingkungan (BE-BD-006 bagian 9.2). |
| **Outcome** | Petugas dapat mengelola katalog komponen darah dan daftar alasan yang dipakai seluruh modul, tanpa satu pun nilai ditanam di kode |
| **Trace** | `DEC-BD-024`, `DEC-BD-032`, `DEC-BD-044`, `BD-DOM-13/14` |
| **Kontrak** | api-contract `v4` — Blood Component, Blood Bank Reason |
| **Reuse** | `BD-CAP-011/012/013` |
| **Dependency** | `G1` ✅ |
| **Acceptance** | `AC-BD-055`, `AC-BD-056` — keduanya **terbukti** |
| **DoD** | CRUD berjalan; seed minimum terisi; seluruh kategori alasan terseed — **terpenuhi** |

---

### ✅ `BE-BD-002` — Unit pelayanan dapat dikonfigurasi berwenang memesan darah

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 3 September 2026. Bukti: [laporan](../task/report/backend/BE-BD-002.md). Satu `AddColumn` `defaultValue: false`, **nol index** dibuat maupun diubah, 8 test lulus. **Riwayat saat task selesai: migration belum dijalankan** **Pembaruan yang dilaporkan 14 September 2026:** `migrations list` pada `QuilvianNewDevSukma` 0 pending dari 180; hanya berlaku untuk database dev yang disebut, bukan semua lingkungan (BE-BD-006 bagian 9.2). |
| **Outcome** | Kewenangan memesan darah datang dari konfigurasi per unit, bukan dari daftar yang ditanam di kode |
| **Trace** | `DEC-BD-012`, `BD-DOM-18` |
| **Kontrak** | integration-contract |
| **Reuse** | `BD-CAP-005` — `Extend` `MstServiceUnit` + `IsAvailableForBloodOrder` |
| **Dependency** | `G1` ✅, pemilik Master Data |
| **Acceptance** | `AC-BD-015`, `AC-BD-016` **terbukti**. `AC-BD-013` **diteruskan ke `BE-BD-003`** karena penegakannya ada di jalur order darah |
| **DoD** | Unit tak dikonfigurasi ditolak — penegakan menyusul di `BE-BD-003` |

---

### ✅ `BE-BD-014` — Lokasi penyimpanan darah dapat dikelola, termasuk dinonaktifkan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 3 September 2026, **dipulihkan 14 September 2026**. Bukti: [laporan](../task/report/backend/BE-BD-014.md). 9 endpoint, seeder 2 lokasi aktif, 25 test lulus. `MstDrugStorageLocation` **nol berkas disentuh**. **Riwayat saat task selesai: migration belum dijalankan**. Tiga gap dicatat di laporan bagian 8. **Diturunkan ✅ → 🟡 pada 14 September 2026** atas keputusan pemilik: registrasi DI `BloodStorageLocationService` hilang pada cabang berjalan lewat merge `27d737cd` (11 September 2026), sehingga controller-nya menjawab `500` pada setiap action. **Dikembalikan ke ✅ pada hari yang sama** sesudah registrasi dipulihkan dan smoke validation lolos. **Satu temuan baru dicatat, belum diperbaiki:** `HeldUnitCount` bernilai `0` pada balasan penonaktifan lokasi walau pesannya menyebut ada kantong tertahan — [BE-BD-006](../task/report/backend/BE-BD-006.md) bagian 9.6 **Pembaruan yang dilaporkan 14 September 2026:** `migrations list` pada `QuilvianNewDevSukma` 0 pending dari 180; hanya berlaku untuk database dev yang disebut, bukan semua lingkungan (BE-BD-006 bagian 9.2). |
| **Outcome** | Lokasi penyimpanan darah dapat dikelola dan dinonaktifkan, dan akibat penonaktifan terbaca jelas |
| **Trace** | `DEC-BD-035`, `DEC-BD-037`, `BD-DOM-24` |
| **Kontrak** | api-contract `v4` — Blood Storage Location; validation |
| **Reuse** | `BD-CAP-011/012/013` |
| **Dependency** | `G1` ✅ |
| **Acceptance** | `AC-BD-064` **terbukti**. `AC-BD-062/065/066/067` **diteruskan ke `BE-BD-015`** karena menuntut penempatan kantong |
| **DoD** | Lokasi nonaktif hilang dari pilihan; penonaktifan **tidak** memindahkan kantong |

---

### ✅ `BE-BD-016` — Seluruh resource dan action hak akses terdaftar

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 17 September 2026** ([laporan](../task/report/backend/BE-BD-016.md) bagian 10) — inventaris current **38 butir** identik pada source (78 endpoint, 8 controller), `api-contract.md`, dan database `AccessMenuSeeder` (38 aktif, 0 ganda, 0 yatim); `BloodUnit : Resolve` dan `BloodOrder : Update` absen; `AC-BD-078` runtime `403 VAL-BD-069`; kode penolakan `VAL-BD-037`/`VAL-BD-069` ditambahkan pada dua atribut tanpa mengubah keputusan izin; build `0 Error(s)`. **Riwayat (HISTORY):** 🟡 **SELESAI SEBAGIAN** - audit source pemilik 14 September 2026 menemukan **30 deklarasi unik terhadap baseline roadmap 39**, termasuk `BloodUnit : Allocate` dari BE-BD-006. Verifikasi atribut pasangan, seeder database, dan otorisasi non-SuperAdmin belum dibuktikan audit regex tersebut ([BE-BD-016](../task/report/backend/BE-BD-016.md) bagian 9). **Riwayat 11 September 2026:** 🟡 **SELESAI SEBAGIAN** — **29 dari 39** butir terdaftar per 11 September 2026, naik dari 28 setelah `BloodUnit : Store` lahir bersama endpoint penyimpanan di `BE-BD-015` ([laporan](../task/report/backend/BE-BD-015.md)). **Riwayat:** 28 dari 39 pada hari yang sama, naik dari 25 setelah `BloodBankProcedure : Read`, `Create`, dan `Update` lahir bersama controller-nya di `BE-BD-012` ([laporan](../task/report/backend/BE-BD-012.md)). **Riwayat:** 25 dari 39 pada hari yang sama, naik dari 20 setelah `BloodProviderRequest : Read`, `Create`, `Process`, `Update`, dan `BloodUnit : Read` lahir bersama controller-nya di `BE-BD-004` ([laporan](../task/report/backend/BE-BD-004.md)). **Riwayat:** 20 dari 39 pada hari yang sama, naik dari 17 setelah `BloodOrder : Read`, `Create`, dan `Cancel` lahir bersama controller-nya di `BE-BD-003` ([laporan](../task/report/backend/BE-BD-003.md)). `BloodOrder : Update` **tidak** lahir karena kontrak `v4` tidak punya endpoint yang memakainya. **Riwayat:** 17 dari 39 per 9 September 2026, naik dari 12 setelah kelima butir `BloodGroupExam` lahir bersama controller-nya di `BE-BD-005` dan `BE-BD-011`. Bukti: [laporan](../task/report/backend/BE-BD-016.md), [BE-BD-005](../task/report/backend/BE-BD-005.md) |
| **Kenapa belum penuh** | **Tertutup 17 September 2026.** Baseline 39 = 38 butir kanonik + `BloodOrder : Update`, yang tidak punya endpoint `v4`; delapan butir `BloodUnit` sisanya lahir sah pada `BE-BD-007`..`010`. **Riwayat (HISTORY):** Selisih aritmetis terhadap baseline 39 adalah **9**. Daftar selisih harus dicocokkan ulang ke kontrak dan endpoint sah; `BloodOrder : Update` sudah tercatat tidak punya endpoint v4. Tidak ada klaim bahwa seluruh selisih wajib diimplementasikan atau sudah terdaftar di DB. **Riwayat teks lama (bukan hitungan terkini):** Alasannya **arsitektural, bukan kelalaian**. Sisa 19 butir: 18 menunjuk controller yang belum ada, dan `BloodOrder : Update` tidak punya endpoint pada kontrak `v4`; mendaftarkannya sekarang berarti membuat butir hak akses yang tidak menjaga apa pun |
| **Outcome** | Setiap tindakan Bank Darah punya butir hak akses yang dapat diberikan kepada peran |
| **Trace** | `DEC-BD-039`..`DEC-BD-047` |
| **Kontrak** | permission-audit-matrix `v4` |
| **Dependency** | `G1` ✅ |
| **Sisa pekerjaan** | **Tidak ada.** **Riwayat (HISTORY):** Audit `[AccessController]`/`[AccessAction]` terhadap 30 deklarasi, verifikasi hasil seeder dan akses non-SuperAdmin, lalu rekonsiliasi baseline ke kontrak. Pemakai sah berikutnya lahir pada task BE-BD-007/008/009/010 sesuai scope; jangan membuat action dummy. **Riwayat teks lama:** 18 butir lahir bersama task pembuat controller-nya masing-masing; `BloodOrder : Update` menunggu keputusan pemilik kontrak |
| **Temuan** | `CONF-BD-006` ditemukan task ini dan ditutup `DEC-BD-047` pada hari yang sama |

---

### ✅ `BE-BD-005` — Golongan darah pasien diperiksa dan divalidasi

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 9 September 2026. Bukti: [laporan](../task/report/backend/BE-BD-005.md). 3 entity + 1 enum + 8 endpoint; `dotnet build` solution `0 Error(s)`; 134 test Bank Darah lulus, 29 di antaranya baru; `UnitTests.Sqlite` 177 lulus. **Riwayat saat task selesai: satu migration dibuat, belum dijalankan.** Kelima `AC` terbukti; batas pembuktian `AC-BD-077/078` dicatat di laporan bagian 6 **Pembaruan yang dilaporkan 14 September 2026:** `migrations list` pada `QuilvianNewDevSukma` 0 pending dari 180; hanya berlaku untuk database dev yang disebut, bukan semua lingkungan (BE-BD-006 bagian 9.2). |
| **Kenapa tidak terkena `G4`** | Dua alasan yang keduanya diperiksa ke bukti: **(a)** dependency-nya hanya `G1` dan `G2b`, keduanya tertutup — tidak ada `BE-BD-003` maupun `BE-BD-004` di sana; **(b)** `BbkBloodGroupExam` memuat `PatientId`, **bukan** `BloodOrderId`, dan nol field-nya dialokasikan number-series |
| **Outcome** | Petugas mencatat sampel, hasil pemeriksaan golongan darah, lalu validator klinis memvalidasinya. Hasil yang belum tervalidasi tidak pernah dipakai klinis |
| **Trace** | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026`, `DEC-BD-039`; `BD-AGG-04`, `BD-XINV-04` |
| **Kontrak** | api-contract `v4` — Blood Group Exam; state-transition; validation |
| **Reuse** | `BD-CAP-016` — enum `BloodType` dipakai apa adanya |
| **Scope** | `BbkBloodGroupExam` + `BbkBloodGroupSample`; alur sampel → hasil → **validasi rutin**; deteksi konflik → `IsConflictHeld` (`BD-DOM-21`); migration |
| **Dependency** | `G1` ✅, `G2b` ✅ — **nol dependency task** |
| **Acceptance** | `AC-BD-030/034/035/077/078` |
| **Verification** | Hasil tak tervalidasi tak dipakai klinis; konflik menahan gerbang |
| **Risk/owner** | **Tinggi / klinis.** Butir `Validate` terpisah dari `ResolveConflict` |
| **⚠️ Yang wajib dicek builder** | **SUDAH DIPERIKSA DAN DITUTUP.** Builder menemukan pertentangan nyata: `03-domain-architecture.md:283` menulis *"Identifier sampel terbitan sistem"*, sedangkan `03-frontend-architecture.md:218` dan `00-interview-decisions.md:214` memperlakukannya sebagai isian petugas. Builder berhenti dan melapor sebelum menulis kode, dan **pemilik memutuskan 9 September 2026: `SampleIdentifier` ditulis petugas**. Karena itu nol field pada slice ini memerlukan provider nomor dan `G4` benar-benar tidak mengenainya. Frasa `BD-DOM-10` perlu dikoreksi — lihat laporan bagian 7 |
| **DoD** | Seluruh AC lulus; butir hak akses `Validate` dan `ResolveConflict` terdaftar; laporan tracked ditulis |

---

### ✅ `BE-BD-011` — Konflik golongan darah diselesaikan validator klinis

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 9 September 2026. Bukti: [laporan](../task/report/backend/BE-BD-011.md). 1 entity append-only + 1 endpoint `POST /conflict-resolution`; 9 test penyelesaian konflik lulus. **Riwayat saat task selesai: migration belum dijalankan.** Ketujuh `AC` terbukti; batas pembuktian `AC-BD-037` dicatat di laporan bagian 8 **Pembaruan yang dilaporkan 14 September 2026:** `migrations list` pada `QuilvianNewDevSukma` 0 pending dari 180; hanya berlaku untuk database dev yang disebut, bukan semua lingkungan (BE-BD-006 bagian 9.2). |
| **Outcome** | Konflik hasil golongan darah diselesaikan lewat pemeriksaan ulang oleh validator klinis, bukan lewat penimpaan data |
| **Trace** | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-039` |
| **Kontrak** | api-contract `v4`; state-transition; validation |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-005` ✅ |
| **Acceptance** | `AC-BD-036/037/051/053/054/079/080` |
| **Risk/owner** | Tinggi / klinis |
| **Membuka** | `FE-BD-009` |

---

### ✅ `BE-BD-003` — Order darah dibuat, ganda tertahan, dibatalkan dua peran

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 11 September 2026** — [laporan](../task/report/backend/BE-BD-003.md). Build `0 Error(s)`, 498 test lulus (79 order darah), 4 uji PostgreSQL lulus, migration `20260910153119_AddBbkBloodOrder` diterapkan ke `QuilvianNewDevSukma`. **Diverifikasi ulang 11 September 2026** pada commit `8e30aa9`: build `0 Error(s)` dengan `210 Warning(s)` sama dengan baseline, 498/498 test (79 order darah), 231/231 test Sqlite, 4/4 uji PostgreSQL, migration `138/138` terterapkan dengan pending 0 — [laporan bagian 5.1](../task/report/backend/BE-BD-003.md). **Riwayat:** 🟡 PENDING — siap dijadwalkan sejak 10 September 2026. `G4` tertutup: provider `NumberSeriesAllocator` berdiri (`PLT-BE-003`) dan durabilitasnya terbukti di PostgreSQL (`PLT-BE-004`, 6 dari 6 lulus). **Riwayat:** ⛔ BLOCKED oleh `G4` secara langsung sampai 10 September 2026 |
| **Yang memblokir** | **Nihil sejak 10 September 2026.** `BbkBloodOrder.OrderNumber` (`02-backend-architecture.md:164`, `:387`) wajib dialokasikan provider number-series, dan provider itu kini ada: `NumberSeriesAllocator` pada `Areas/Platform/NumberSeriesManagement/Services/`. Bank Darah menetapkan awalan dan formatnya; mesin alokasinya bukan milik Bank Darah (`DEC-PLT-005`). **Riwayat:** provider yang dapat dipanggil Bank Darah belum ada sampai 9 September 2026 |
| **Pekerjaan yang tetap aman** | Nol. Nomor order lahir bersama entity-nya; memisahkannya berarti membuat order tanpa identitas bisnis |
| **Outcome** | Order darah dibuat elektronik maupun manual; order ganda tertahan; pembatalan menuntut alasan berkategori sesuai peran; pemenuhan dihitung |
| **Trace** | `DEC-BD-004/005/006/044`; `BD-AGG-01`, `BD-XINV-01`, `INV-BD-035` |
| **Kontrak** | api-contract `v4` — Blood Order; state-transition; validation |
| **Reuse** | `BD-CAP-002/007/009/010` — **catatan:** `BD-CAP-009` kini merujuk `LabTransitionHistory.cs`, bukan `TrxLabTransitionHistory.cs` |
| **Scope** | `BbkBloodOrder` + `BbkBloodOrderLine`; service deteksi ganda (`BD-DOM-17`); `BloodOrder : Cancel` terpisah dari `Update`; `BbkEncounterStatusReader`; migration |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-001` ✅, `BE-BD-002` ✅, **`G4` ✅** — tertutup 10 September 2026 |
| **Acceptance** | `AC-BD-001/002/003/004/010/011/017/095/096/097` + `AC-BD-013` yang diteruskan dari `BE-BD-002` |
| **Risk/owner** | Sedang / BDRS |

---

### ✅ `BE-BD-004` — Permintaan PMI dibuat, penerimaan dicatat, kantong lahir `Received`

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 11 September 2026 — roadmap revisi 9.** Keenam kriteria yang tetap miliknya — `AC-BD-005/006/009/022/031/059` — terbukti penuh ([laporan](../task/report/backend/BE-BD-004.md) bagian 6); build, test, uji PostgreSQL, dan migration sudah lulus pada pengerjaannya. `AC-BD-023` dan `AC-BD-032` diteruskan ke `BE-BD-015`, `AC-BD-033` ke `BE-BD-006`, atas persetujuan `Sukmagp` 11 September 2026 — mengikuti preseden `BE-BD-002` → `BE-BD-003` dan `BE-BD-014` → `BE-BD-015`. Nol source, test, maupun migration ditulis untuk penerusan ini. **Riwayat:** 🟡 **SELESAI SEBAGIAN 11 September 2026** — [laporan](../task/report/backend/BE-BD-004.md). Seluruh pekerjaan di dalam scope selesai: build `0 Error(s)` dengan `210 Warning(s)` sama dengan baseline, 561/561 test (63 permintaan PMI), 231/231 test Sqlite, 9/9 uji PostgreSQL, migration `20260911032311_AddBbkProviderRequestAndBloodUnit` diterapkan ke `QuilvianNewDevSukma` (`139/139`, pending 0). **6 dari 9 kriteria terbukti penuh** (`AC-BD-005/006/009/022/031/059`). `AC-BD-023` dan `AC-BD-032` terbukti sampai kantong lahir `Received`; perpindahannya ke `PendingReview` menunggu `BE-BD-015`. `AC-BD-033` menunggu endpoint alokasi `BE-BD-006`. Menjadi ✅ bila pemilik roadmap meneruskan ketiganya, mengikuti preseden `BE-BD-002` dan `BE-BD-014`. **Riwayat:** 🟡 PENDING — siap dijadwalkan sejak 11 September 2026; BLOCKED oleh `BE-BD-003` sampai 11 September 2026, dan oleh `G4` secara langsung sampai 10 September 2026 |
| **Yang memblokir** | **Nihil sejak 11 September 2026** — `BE-BD-003` ✅. **Riwayat:** `BE-BD-003`, sesuai kolom Dependency. `BbkProviderRequest.RequestNumber` (`:183`, `:401`) wajib dari provider number-series, dan provider itu **sudah ada** sejak `G4` tertutup. **`PmiBagNumber` tidak termasuk** — nomor kantong datang dari PMI (`ASM-BD-003`) |
| **Outcome** | Permintaan ke PMI dicatat; penerimaan termasuk kelebihan tercatat; kantong lahir berstatus `Received` dan belum dapat dialokasikan |
| **Trace** | `DEC-BD-002/003/008/020/025/036`; `BD-AGG-02`, `BD-XINV-02/03` |
| **Kontrak** | api-contract `v4` — Provider Request; state-transition; validation |
| **Scope** | `BbkProviderRequest` + `BbkProviderReceipt` + `BbkBloodUnit`; sisa ≥ 0 dijaga token `Version`; kelebihan → `IsExcess`; migration |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-003` ✅, **`G4` ✅** — tertutup 10 September 2026 |
| **Acceptance** | `AC-BD-005/006/009/022/031/059` — roadmap revisi 9. **Riwayat:** `AC-BD-005/006/009/022/023/031/032/033/059` sampai revisi 8; `AC-BD-023/032` kini milik `BE-BD-015`, `AC-BD-033` milik `BE-BD-006` |
| **Risk/owner** | Sedang / BDRS |

---

### ✅ `BE-BD-012` — Tindakan Bank Darah dicatat tanpa penyaluran biaya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 11 September 2026** — [laporan](../task/report/backend/BE-BD-012.md). Build `0 Error(s)` (`-p:RunAnalyzers=false`); 600/600 test `QuilvianSystemBackend.Tests` (39 tindakan Bank Darah), 231/231 test Sqlite, 13/13 uji PostgreSQL Bank Darah (4 tindakan); migration `20260911072451_AddBbkBloodBankProcedure` diterapkan ke `QuilvianNewDevSukma` (`140/140`, pending 0), `has-pending-model-changes` bersih. **Kelima kriteria `AC-BD-098` sampai `AC-BD-102` terbukti**; nol butir DoD dikecualikan. `UnitTests.InMemory` 896/905 — 9 kegagalan Billing baseline, di luar task. Dua tafsiran menunggu konfirmasi pemilik tanpa menahan kriteria: kunjungan tanpa kelas ditolak `422`, dan "order sah" tidak dibatasi status bisnis order ([laporan](../task/report/backend/BE-BD-012.md) bagian 7). Delta kontrak: `VAL-BD-084`, `Scope` `BloodBankProcedure`, klarifikasi urutan `DEC-BD-049`. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN** sejak roadmap revisi 8, 11 September 2026. Ketiga penahan tertutup pada hari yang sama: aturan tarif oleh `DEC-BD-049`, sumber unit dan kelas oleh `DEC-BD-048`, dan acceptance criteria diganti `AC-BD-098` sampai `AC-BD-102`. Belum ada source; laporan yang ada mencatat pemberhentian sebelum keputusan turun. **Riwayat:** ⛔ **BLOCKED 11 September 2026 — menunggu tiga keputusan** ([laporan](../task/report/backend/BE-BD-012.md)). Builder berhenti sebelum satu baris source ditulis; build, test, dan migration `NOT RUN`. **(1)** Aturan pemilihan tarif tindakan — kontrak tidak menetapkannya, dan source memuat dua aturan yang memberi angka berbeda; pemilik Billing bersama BDRS (`DEC-BD-021`). **(2)** Sumber `ServiceUnitId` dan `PatientClassId` — pemilik proses BDRS. **(3)** `AC-BD-026` dan `AC-BD-058` menuntut fakta biaya ke Billing (`DEC-BD-016` `OPEN`) serta pemberian dan koreksi (`BE-BD-007`, `BE-BD-010`), sehingga tidak dapat dibuktikan pada task ini — pemilik roadmap lewat `plan-module-delivery`. **Riwayat:** 🟡 PENDING — siap dijadwalkan sejak 11 September 2026 setelah `BE-BD-003` ✅. **Riwayat:** BLOCKED oleh `BE-BD-003` sampai 11 September 2026, dan oleh `G4` secara langsung sampai 10 September 2026 |
| **Yang memblokir** | **Nihil sejak 11 September 2026** — `BE-BD-003` ✅. **Riwayat:** `BE-BD-003`, sesuai kolom Dependency. `BbkBloodBankProcedure.ProcedureNumber` (`:336`, `:443`) wajib dari provider number-series, dan provider itu **sudah ada** sejak `G4` tertutup |
| **Outcome** | Tindakan Bank Darah tercatat beserta snapshot tarifnya, **tanpa** penyaluran biaya ke Billing |
| **Trace** | `DEC-BD-021`, `DEC-BD-034`, **`DEC-BD-048`**, **`DEC-BD-049`**; `BD-AGG-05` |
| **Kontrak** | api-contract `v4` — Blood Bank Procedure; kamus data `BbkBloodBankProcedure`; state-transition §5; validation §5 |
| **Scope** | `BbkBloodBankProcedure` dengan snapshot tarif; **tanpa** penyaluran Billing. Unit dan kelas pasien diambil dari kunjungan order (`DEC-BD-048`). Tarif dipilih backend memakai predikat kecocokan `InsuranceCoverageService` — paling spesifik menang, tarif tanpa kelas sebagai cadangan, tanpa kandidat ditolak `422` (`DEC-BD-049`). `ProcedureNumber` dari `NumberSeriesAllocator`; migration |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-003` ✅, **`G4` ✅** — tertutup 10 September 2026 |
| **Acceptance** | `AC-BD-098/099/100/101/102` — roadmap revisi 8. **`AC-BD-102` **HISTORICAL / SUPERSEDED oleh `DEC-BD-016` + `BE-BD-013`**** (keputusan pemilik 18 September 2026): bukti historis task ini saat `DEC-BD-016` terbuka, bukan syarat saat ini; tindakan `Completed` kini menghasilkan tepat satu fakta biaya ([BE-BD-013](../task/report/backend/BE-BD-013.md)). **Riwayat:** `AC-BD-026/058` sampai revisi 7; keduanya kini milik `BE-BD-013` |
| **Risk/owner** | Sedang / BDRS |

---

### ✅ `BE-BD-015` — Kantong disimpan, dipindahkan, riwayatnya tak pernah ditimpa

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 11 September 2026 — roadmap revisi 10.** Seluruh kriteria yang tetap miliknya — `AC-BD-023/032/061/062/063/065/066/067/069` — terbukti penuh, dan bagian gerbang `AC-BD-060/068/070` terbukti ([laporan](../task/report/backend/BE-BD-015.md) bagian 7); build, test, uji PostgreSQL, migration, dan model-check sudah lulus pada pengerjaannya. Verifikasi final bagian alokasi `AC-BD-060/068/070` diteruskan ke `BE-BD-006` atas persetujuan `Sukmagp` 11 September 2026 — mengikuti preseden revisi 9. Seluruh butir DoD milik task ini terpenuhi. Nol source, test, maupun migration ditulis untuk penerusan ini. **Riwayat:** 🟡 **SELESAI SEBAGIAN 11 September 2026** — [laporan](../task/report/backend/BE-BD-015.md). Seluruh scope selesai: `BbkBloodUnitPlacement` + index unik terfilter + `CurrentPlacementId`, `POST`/`PUT /{id}/storage-location`, `GET /{id}/placements`, dan gerbang alokasi baca-saja. Build `0 Error(s)` (`-p:RunAnalyzers=false`); 643/643 test (43 penyimpanan), 231/231 test Sqlite, 19/19 uji PostgreSQL Bank Darah (6 penyimpanan, termasuk perpindahan serentak dua petugas); migration `20260911090848_AddBbkBloodUnitPlacement` diterapkan ke `QuilvianNewDevSukma` (`141/141`, pending 0), `has-pending-model-changes` bersih. **9 dari 12 kriteria terbukti penuh** (`AC-BD-023/032/061/062/063/065/066/067/069`). `AC-BD-060`, `068`, dan `070` terbukti pada tingkat gerbang (`VAL-BD-063/064`, gerbang terbuka kembali sesudah perpindahan); bagian "dicoba dialokasikan" menuntut endpoint alokasi `BE-BD-006`, dan `060`/`068` memang tercantum juga pada `BE-BD-006`. Butir DoD yang belum terpenuhi hanya "seluruh AC terbukti". Menjadi ✅ bila pemilik roadmap meneruskan bagian alokasi ketiganya ke `BE-BD-006`, mengikuti preseden revisi 9. `UnitTests.InMemory` 896/905 — 9 kegagalan Billing baseline, di luar task. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN** sejak roadmap revisi 9, 11 September 2026. Seluruh dependency tertutup: `BE-BD-004` ✅ dan `BE-BD-014` ✅; kantong `BbkBloodUnit` sudah ada di `QuilvianNewDevSukma`. Menerima `AC-BD-023` dan `AC-BD-032` dari `BE-BD-004`. Belum ada source. **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-004`** — bukan karena butuh nomor. **Catatan 11 September 2026:** kantong kini ada — `BbkBloodUnit` lahir bersama `BE-BD-004` 🟡 dan tabelnya sudah di `QuilvianNewDevSukma`. Yang masih menahan adalah status `BE-BD-004` yang belum ✅, bukan ketiadaan kantong; lihat kartu `BE-BD-004` |
| **Yang memblokir** | **Nihil sejak 11 September 2026** — `BE-BD-004` ✅ pada roadmap revisi 9. **Riwayat:** kantong belum ada sampai `BE-BD-004` menciptakannya; sesudah itu status `BE-BD-004` yang belum ✅ |
| **Outcome** | Kantong ditempatkan pada lokasi, dipindahkan, dan riwayat penempatannya hanya dapat ditambah |
| **Trace** | `DEC-BD-036/037`; `BD-DOM-25`; `INV-BD-025/026/027/028`; `ARCH-BD-POS-04/05/06` |
| **Kontrak** | api-contract `v4` — storage-location, placements; state-transition; validation |
| **Scope** | `BbkBloodUnitPlacement` + filtered-unique `IsCurrent` + `BbkBloodUnit.CurrentPlacementId` dalam satu transaksi; `POST`/`PUT /{id}/storage-location`; `GET /{id}/placements`; migration |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-004` ✅, `BE-BD-014` ✅ |
| **Acceptance** | `AC-BD-061/063/066/067/069` + `AC-BD-062/065` diteruskan dari `BE-BD-014` + **`AC-BD-023/032` diteruskan dari `BE-BD-004`** pada roadmap revisi 9 — roadmap revisi 10. Untuk `AC-BD-023/032`, bagian penerimaan sudah terbukti di `BE-BD-004`; yang dibuktikan di sini adalah perpindahan kantong ke `PendingReview` sesudah disimpan dan kemunculannya di daftar kantong `PendingReview`. **Bagian gerbang `AC-BD-060/068/070` tetap dibuktikan di sini dan buktinya dipertahankan:** kantong `Received` ditolak gerbang alokasi `VAL-BD-063`; lokasi current nonaktif menutup gerbang `VAL-BD-064`; perpindahan dari lokasi nonaktif ke aktif menambah riwayat penempatan beserta pelaku dan waktu, lalu gerbang terbuka kembali. **Verifikasi final bagian alokasinya milik `BE-BD-006`** sejak revisi 10. **Riwayat:** `AC-BD-060/061/063/066/067/068/069/070` + `AC-BD-062/065` + `AC-BD-023/032` sampai revisi 9 |
| **Risk/owner** | Sedang / BDRS. Riwayat append-only; nol background job; nol batch update |
| **Catatan urutan** | **Wajib mendahului `BE-BD-006`** — kantong tak dapat dialokasikan sebelum tersimpan |

---

### ✅ `BE-BD-006` — Kantong dialokasikan satu aktif, alokasi keliru dibatalkan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 14 September 2026.** Kesembilan runtime acceptance **terbukti** lewat panggilan API sungguhan ditambah verifikasi read-only database ([laporan](../task/report/backend/BE-BD-006.md) bagian 9.3): `AC-BD-033` `422 VAL-BD-033`; `AC-BD-043` kantong kembali `Tersedia`; `AC-BD-044` kantong `Menunggu keputusan` sesudah order asal dibatalkan; `AC-BD-045` tiga jalur penolakan alasan (`400`/`400`/`422`); `AC-BD-046` `422 VAL-BD-023` atas kantong `Issued`; `AC-BD-060` `422 VAL-BD-063`; `AC-BD-068` `422 VAL-BD-064`; `AC-BD-070` alokasi berhasil sesudah dipindah ke lokasi aktif; `VAL-BD-018c` dua permintaan serentak → `200` + `409` dengan **tepat satu** baris alokasi aktif di database. `dotnet build` `0 Error(s)` / `193 Warning(s)`; `migrations list` **0 pending** dari 180; `has-pending-model-changes` bersih; index unik terfilter `IX_BbkBloodUnitAllocation_ActiveUnit … WHERE ("AllocationStatus" = 0)` **terverifikasi ada di PostgreSQL**; QBE Strict `PASS`. **Regresi DI dipulihkan atas persetujuan pemilik** (laporan 9.4). **Temuan lain masih terbuka:** envelope `VAL-BD-016`, `HeldUnitCount`, dan sisa lokal `Tests/obj` (laporan 9.6). `AC-BD-046` memakai fixture database minimal atas **dua kali persetujuan eksplisit pemilik**; `BE-BD-007` **tidak** diimplementasikan untuk itu. **Riwayat:** 🟡 **PARTIAL — READY FOR RUNTIME VALIDATION — 14 September 2026**; runtime acceptance 0 dari 9, validasi dihentikan sebelum skenario dijalankan; isu Attendance Scheduler dan timeout PostgreSQL transient di luar scope. **Riwayat:** 🟡 **SELESAI SEBAGIAN 13 September 2026 — build, migration, dan QBE Strict lolos, penerapan database terblokir**. Source ter-commit pada `02b70618`, migration pada `6a7b193d`. Build Debug `0 Error(s)` / `191 Warning(s)` (dijalankan pemilik); migration `20260913070556_AddBbkBloodUnitAllocation` terbentuk dengan scope bersih — satu tabel, empat index termasuk index unik terfilter `IX_BbkBloodUnitAllocation_ActiveUnit`, dua FK `Restrict`, nol operasi modul lain; `has-pending-model-changes` bersih. **Penerapan database: `BLOCKED — UNRELATED PENDING MIGRATIONS`** — migration berstatus `(Pending)` dan tidak dapat diterapkan ke `QuilvianNewDevSukma` tanpa ikut menerapkan migration modul lain. Karena itu **0 dari 9 acceptance criteria terbukti**; kesembilannya `BLOCKED`. QBE Strict **`PASS`** — `GitRange` `origin/QuilvianIntegrationBackend` (`719b1c82d73748b194a43ea511886ae398c80f8a`)..`HEAD`, 11 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, exit code `0` — mencakup source dan migration. **Riwayat:** QBE Strict `NOT RUN` — tiga berkas migration belum ter-commit. Butir DoD yang belum terpenuhi: penerapan database dan pembuktian seluruh AC. **Riwayat:** 🟡 **SELESAI SEBAGIAN 12 September 2026 — source lengkap, validasi belum dijalankan** ([laporan](../task/report/backend/BE-BD-006.md)). Seluruh source di dalam scope ditulis: entity `BbkBloodUnitAllocation` beserta index unik terfilter `IX_BbkBloodUnitAllocation_ActiveUnit`, endpoint `POST /{id}/allocate` dan `POST /{id}/cancel-allocation`, hak akses `BloodUnit : Allocate`, dan pemakaian gerbang `EvaluateAllocationGateAsync` milik `BE-BD-015` tanpa duplikasi. **`dotnet build` `NOT RUN`, migration BELUM dibuat, database BELUM disentuh** — atas instruksi eksplisit pemilik pada task ini bahwa build dijalankan manual olehnya. Karena itu **0 dari 9 acceptance criteria terbukti**; kesembilannya berstatus `NOT EXECUTED`, bukan gagal. QBE `DEFERRED — REQUIRES USER COMMIT`. Butir DoD yang belum terpenuhi: build, migration, dan pembuktian seluruh AC. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN (READY)**, dipertegas pada roadmap revisi 11, 12 September 2026. Seluruh dependency tertutup: `G1` ✅, `G2b` ✅, `BE-BD-015` ✅. **Sejak revisi 11 task ini dapat diselesaikan penuh secara mandiri, tanpa mengimplementasikan endpoint `reallocate`** — satu-satunya kriteria yang menuntut `reallocate`, yaitu `AC-BD-071`, sudah dilepas ke `BE-BD-009`. Belum ada source. **Riwayat:** 🟡 PENDING sejak roadmap revisi 10, 11 September 2026. Wewenang menulis tetap diberikan terpisah lewat `build-module-backend`; preflight QBE dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend (bagian 0). **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-015`** sampai roadmap revisi 10 — pada akhirnya karena `BE-BD-015` 🟡 selesai sebagian menunggu bagian alokasi `AC-BD-060/068/070`, yang hanya dapat dibuktikan endpoint task ini |
| **Yang memblokir** | **Nihil.** Dependency tertutup sejak 11 September 2026 (`BE-BD-015` ✅), dan runtime acceptance kesembilan skenario terbukti 14 September 2026. **Riwayat:** runtime acceptance sembilan skenario `PENDING` sampai 14 September 2026 |
| **Trace** | `DEC-BD-003/007/029/036/037`; `BD-AGG-03` |
| **Kontrak** | api-contract `v4`; state-transition; validation |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-015` ✅ — selesai 11 September 2026, roadmap revisi 10 ([laporan](../task/report/backend/BE-BD-015.md)) |
| **Acceptance (final, roadmap revisi 11)** | Sembilan butir, **tanpa `AC-BD-071`**: `AC-BD-043` · `AC-BD-044` · `AC-BD-045` · `AC-BD-046` · `AC-BD-060` · `AC-BD-068` · `AC-BD-070` · `AC-BD-033` + konkurensi `VAL-BD-018c`. Rinciannya: `AC-BD-043` batalkan alokasi kantong belum diberikan pada order aktif → kantong kembali `Available` beserta riwayat; `AC-BD-044` batalkan alokasi ketika order asal sudah berakhir → kantong `PendingReview`, bukan `Available`; `AC-BD-045` batalkan alokasi tanpa alasan terkendali → ditolak `VAL-BD-016`; `AC-BD-046` batalkan alokasi kantong yang sudah `Issued` → ditolak `VAL-BD-023`; `AC-BD-060` kantong `Received` dialokasikan → ditolak `VAL-BD-063`; `AC-BD-068` kantong di lokasi nonaktif dialokasikan → ditolak `VAL-BD-064`; `AC-BD-070` kantong yang sudah dipindahkan ke lokasi aktif dialokasikan → berhasil; `AC-BD-033` kantong berlebih dialokasikan langsung → ditolak `VAL-BD-033` (diteruskan dari `BE-BD-004` pada revisi 9); `VAL-BD-018c` dua petugas memperebutkan satu kantong → tepat satu alokasi aktif menang, yang kalah ditolak. **`AC-BD-071` DILEPAS ke `BE-BD-009` pada revisi 11.** **Riwayat:** `AC-BD-043/044/045/046/060/068/070/071` + konkurensi + `AC-BD-033` sampai revisi 10; `AC-BD-043/044/045/046/060/068/071` + konkurensi + `AC-BD-033` sampai revisi 9 |
| **`AC-BD-071` dilepas ke `BE-BD-009`** (revisi 11) | **Satu kriteria keluar, nol requirement hilang.** `AC-BD-071` berbunyi: kantong `PendingReview` di lokasi nonaktif dicoba **dialihkan** ke pasien lain → **ditolak** `VAL-BD-064`. Kata "dialihkan" di situ bukan kiasan — ia adalah endpoint `POST /api/v1/health-services/blood-bank-management/blood-units/{id}/reallocate`, yang pada kontrak `v4` dijaga hak akses **`BloodUnit : ResolveReallocate`** ([api-contract](../contracts/api-contract.md)). Endpoint itu **bukan milik task ini**: ia salah satu dari tiga jalur penyelesaian `PendingReview` milik `BE-BD-009`, bersama `return-to-provider` dan `mark-not-usable`. Karena itu `AC-BD-071` mustahil dibuktikan di `BE-BD-006` tanpa lebih dulu membangun `reallocate` — yaitu mengerjakan scope task lain. **Yang dibuktikan `BE-BD-006` dan sudah cukup:** gerbang `EvaluateAllocationGateAsync` menolak kantong di lokasi nonaktif dengan `VAL-BD-064` lewat endpoint `allocate` — itulah `AC-BD-068`. Kontrak `v4` [02-backend-architecture.md](../02-backend-architecture.md) §F.4 menetapkan gerbang yang sama dipakai `allocate` **dan** `reallocate`, sehingga ketika `BE-BD-009` membangun `reallocate` ia memakai gerbang yang gerbangnya sudah terbukti di sini. **Nol task baru dibuat** — pemilik `reallocate` memang sudah ada sejak arsip revisi 3 |
| **Verifikasi final yang diteruskan dari `BE-BD-015`** (revisi 10) | Tiga kriteria, **tanpa requirement baru** — bunyi dan bukti yang diharapkan tetap mengikuti `testing/acceptance-test-matrix.md` §7. **`AC-BD-060`:** kantong `Received` dicoba dialokasikan lewat endpoint `allocate` → ditolak `VAL-BD-063`. **`AC-BD-068`:** kantong pada lokasi nonaktif dicoba dialokasikan → ditolak `VAL-BD-064`. **`AC-BD-070`:** sesudah dipindahkan ke lokasi aktif, kantong dapat dialokasikan → berhasil. **Sudah terbukti di `BE-BD-015` dan tidak perlu dibuktikan ulang:** gerbang `EvaluateAllocationGateAsync` menghasilkan `VAL-BD-063` dan `VAL-BD-064`; perpindahan dari lokasi nonaktif ke aktif menambah riwayat penempatan beserta pelaku dan waktu; gerbang terbuka kembali sesudahnya ([laporan](../task/report/backend/BE-BD-015.md) bagian 7). **Syarat agar bukti itu berlaku** — bukan aturan baru, melainkan kontrak `v4` `02-backend-architecture.md` §F.4, yang menetapkan `EvaluateAllocationGate` dipakai `allocate` dan `reallocate`: endpoint `allocate` memanggil gerbang `EvaluateAllocationGateAsync` yang sudah ada; bila tidak, gerbang itu tidak menjaga apa pun ([laporan](../task/report/backend/BE-BD-015.md) bagian 9) |
| **Risk/owner** | Sedang / BDRS |

---

### ✅ `BE-BD-007` — Bukti kecocokan dicatat, kantong diberikan lewat gerbang tiga syarat

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026** ([laporan](../task/report/backend/BE-BD-007.md)). **Kedua belas business acceptance terbukti runtime** lewat panggilan API sungguhan ditambah verifikasi database: `AC-BD-018` `422`; `AC-BD-019`/`039` evidence tersimpan lalu pemberian berhasil dengan `CompatibilityEvidenceIdUsed` menunjuk evidence yang benar; `VAL-BD-079` `422` dan bukti `Incompatible` tetap tersimpan; `AC-BD-038` `422` dengan bukti lama utuh; `AC-BD-040` *fail-closed* `422` atas validity `NULL`; `AC-BD-041` `422` dan bukti pasien A tetap tersimpan; `AC-BD-042` pemberian berhasil ke pasien B memakai evidence pasien B; `AC-BD-072` `422`; `AC-BD-073` pemberian berhasil dengan alokasi aktif dipertahankan; `AC-BD-089` kelima kolom evidence benar; `AC-BD-090` `403` dan `AC-BD-091` `200` dengan **aktor non-SuperAdmin sungguhan**. **Ketujuh kode `VAL-BD-017/018/019/020/020b/065/078/079` terbukti terkirim** pada slot `errors.code` dengan pesan persis `validation-matrix.md`, diverifikasi terprogram terhadap dokumen kontrak. Operasi yang ditolak terbukti tidak mengubah database. `dotnet build` `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS` (15 berkas, nol temuan, nol suppression); nol migration baru; nol perubahan skema. **Satu bug source ditemukan lewat bukti runtime dan diperbaiki minimal atas persetujuan pemilik** — blok tanpa kondisi yang membuat setiap pencatatan bukti kecocokan gagal `400` (laporan bagian 3). **Perluasan `AccessPermissionAttribute`/`AccessPermissionFilter` bersifat aditif dan opt-in**; balasan generic pemakai lama terbukti tidak berubah (laporan bagian 6.1). **Akibatnya `BE-BD-008`, `BE-BD-009`, dan `BE-BD-010` kini siap dijadwalkan.** Belum ter-commit atas instruksi pemilik; cleanup fixture `TEST-BD007` menunggu persetujuan. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN 14 September 2026.** Kedua dependency-nya tertutup: `BE-BD-005` ✅ dan `BE-BD-006` ✅ ([laporan](../task/report/backend/BE-BD-006.md)). Belum ada source; wewenang menulis tetap diberikan terpisah lewat `build-module-backend`. **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-006`** sampai 14 September 2026; `BE-BD-005` sudah ✅ sejak semula, sehingga yang tersisa murni rantai dependency yang berawal dari `BE-BD-003`. **Riwayat:** sampai 10 September 2026 rantai itu tertahan `G4` |
| **Outcome** | Bukti kecocokan dicatat beserta hasilnya; pemberian melewati gerbang tiga syarat yang dinilai ulang, bukan diwarisi dari alokasi |
| **Trace** | `DEC-BD-013/027/028/038/042`; `BD-AGG-03`; `ARCH-BD-POS-01/02/07`; `INV-BD-019/020/029` |
| **Kontrak** | api-contract `v4` — compatibility-evidence, issue; state-transition; validation |
| **Scope** | `BbkCompatibilityEvidence` + `EvidenceResult` + `ValidatedByUserId`; `EvaluateIssuanceGate`; migration |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-005` ✅, `BE-BD-006` ✅ — seluruhnya tertutup. **Riwayat:** `BE-BD-006` ⛔ sampai 14 September 2026 |
| **Acceptance** | `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` — **kedua belas terbukti runtime 16 September 2026** ([laporan](../task/report/backend/BE-BD-007.md) bagian 4) |
| **Risk/owner** | **Tinggi / klinis & BDRS.** Gerbang *fail-closed* dan dinilai ulang; pemberian bersifat terminal |

---

### ✅ `BE-BD-008` — Pemberian jalur darurat tercatat penuh

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026** ([laporan](../task/report/backend/BE-BD-008.md)). **Kesembilan acceptance terbukti runtime** lewat panggilan API sungguhan ditambah verifikasi database: `AC-BD-020` pemberian darurat berhasil, ditandai tanpa bukti, dan **muncul di daftar kerja #3**; `AC-BD-021` aktor tak berwenang ditolak `403`; `AC-BD-074` kantong di lokasi nonaktif berhasil diberikan lewat otorisasi darurat setelah jalur normal ditolak `VAL-BD-065` sebagai kontrol; `AC-BD-075` `422 VAL-BD-066`; `AC-BD-081` peran `AttendingPhysician` tersimpan; `AC-BD-082` peran `BloodBankDoctor` tersimpan; `AC-BD-083` `403 VAL-BD-072`; `AC-BD-084` `422 VAL-BD-070`; `AC-BD-085` `422 VAL-BD-071`. **Kelima kode `VAL-BD-021/066/070/071/072` terkirim** pada slot `errors.code` dengan pesan persis `validation-matrix.md`, dibandingkan secara terprogram terhadap dokumen kontrak; nol partial state mutation dari penolakan. Otorisasi dibuktikan dengan **aktor non-SuperAdmin sungguhan**. `dotnet build` `0 Error(s)` / `198 Warning(s)` — nol warning baru. Migration `20260916051204_AddBbkEmergencyAuthorization` scope bersih (satu tabel, tiga index, dua FK `Restrict`, nol operasi modul lain), `has-pending-model-changes` bersih, terterapkan ke `QuilvianNewDevSukma` dengan **`0 pending`**. QBE Strict `PASS`. Aturan bukti kecocokan **direuse** dari `BE-BD-007` lewat ekstraksi helper, bukan disalin. Belum ter-commit atas instruksi pemilik; cleanup fixture `TEST-BD008` menunggu persetujuan. **Dua catatan kontrak dilaporkan tanpa mengubah requirement:** tegangan `VAL-BD-021` versus `VAL-BD-072`, dan trace kartu ini menulis `BD-DOM-09` padahal konsep domainnya `BD-DOM-08` (laporan bagian 3.2 dan 10). **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN 16 September 2026**, penahannya `BE-BD-007` gugur. **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-007`** sampai 16 September 2026 |
| **Trace** | `DEC-BD-017/038/040`, **`DEC-BD-050`**; `BD-DOM-09` — **koreksi:** konsep domain jalur darurat adalah **`BD-DOM-08`** Otorisasi Darurat; `BD-DOM-09` adalah Pemeriksaan Golongan Darah. Dicatat sebagai koreksi trace, bukan perubahan requirement |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-007` ✅. **Riwayat:** `BE-BD-007` ⛔ sampai 16 September 2026 |
| **Acceptance** | `AC-BD-020/021/074/075/081/082/083/084/085` — **kesembilan terbukti runtime 16 September 2026** ([laporan](../task/report/backend/BE-BD-008.md) bagian 4). `AC-BD-021` ditolak `403` dengan `VAL-BD-072`, **sesuai kontrak** sesudah `DEC-BD-050` |
| **Risk/owner** | **Tinggi / klinis** |

---

### ✅ `BE-BD-009` — Kantong `PendingReview` diselesaikan lewat tiga wewenang terpisah

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 17 September 2026** ([laporan](../task/report/backend/BE-BD-009.md)). **Kesembilan acceptance terbukti runtime** lewat API sungguhan + verifikasi database: `AC-BD-007` `422` pesan `VAL-BD-033`, kantong tetap `PendingReview`; `AC-BD-008` ketujuh kantong di daftar kerja #2; `AC-BD-024` `200`, rantai alokasi pasien asal (dibatalkan beserta alasan) → alokasi pasien tujuan + transisi `PendingReview → Reallocated`; `AC-BD-025`/`029` `400 VAL-BD-016` (kosong, tidak dikirim, spasi, teks bebas, kode tidak dikenal); **`AC-BD-071` `422 VAL-BD-064`** lewat `EvaluateAllocationGateAsync` yang sama dengan `allocate`, nol alokasi tertulis; `AC-BD-092` `200` `ReturnedToProvider` dengan alasan, pelaku, waktu; `AC-BD-093` `403 VAL-BD-080`; `AC-BD-094` `200`, bukti pasien asal `IsSuperseded = true` + `SupersededReason`. Jalur `mark-not-usable` `200` `NotUsable`. **Kelima kode `VAL-BD-016/064/080/081/082` persis** HTTP, `errors.code`, dan pesan `validation-matrix.md`. Otorisasi dua aktor non-SuperAdmin membuktikan ketiga butir terpisah. Migration `20260917021029_AddBbkCompatibilityEvidenceSupersededReason` (satu kolom nullable) terterapkan, `0 pending`; build `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS`. Satu cacat source ditemukan runtime (`[Required]` pada DTO baru menelan amplop `VAL-BD-016`) dan diperbaiki pada berkas milik task ini. api-contract diselaraskan: `VAL-BD-016` = `400`. **Gap tersisa:** belum ada jalur keluar dari `Reallocated` pada matriks (keputusan kontrak). Belum ter-commit atas instruksi pemilik; cleanup `TEST-BD009` menunggu persetujuan. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN 16 September 2026.** Kedua penahannya kini tertutup: `BE-BD-006` ✅ 14 September 2026 dan `BE-BD-007` ✅ 16 September 2026 ([laporan](../task/report/backend/BE-BD-007.md)). Belum ada source. **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-006` dan `BE-BD-007`.** Anotasi `BE-BD-006 ⛔` pada baris dependency di bawah adalah keadaan sebelum roadmap revisi 10; keadaan `BE-BD-006` yang berlaku kini 🟡 **PENDING — siap dijadwalkan**. Penahan `BE-BD-007` masih nyata, sehingga status task ini tidak berubah |
| **Trace** | `DEC-BD-019/028/043`; `DEC-BD-045` |
| **Scope** | **Tiga jalur penyelesaian kantong `PendingReview`, tiga butir hak akses berbeda** — `POST /api/v1/health-services/blood-bank-management/blood-units/{id}/reallocate` (`BloodUnit : ResolveReallocate`), `.../return-to-provider` (`BloodUnit : ResolveReturn`), `.../mark-not-usable` (`BloodUnit : ResolveNotUsable`). Ditegaskan pada roadmap revisi 11 dari bukti yang sudah ada, bukan dari keputusan baru: [api-contract](../contracts/api-contract.md) `v4` mencantumkan ketiga endpoint itu, [03-frontend-architecture.md](../03-frontend-architecture.md) menempatkan ketiga tombolnya pada layar penyelesaian `PendingReview`, dan [arsip revisi 3](archive/revision-3/00-delivery-plan.md) sudah menulis `reallocate` di scope task ini. Jalur `reallocate` **wajib memanggil gerbang `EvaluateAllocationGate`** — kontrak `v4` [02-backend-architecture.md](../02-backend-architecture.md) §F.4 |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-006` ✅, `BE-BD-007` ✅. **Riwayat:** `BE-BD-006` ⛔, `BE-BD-007` ⛔ |
| **Acceptance** | `AC-BD-007/008/024/025/029/092/093/094` + **`AC-BD-071` diteruskan dari `BE-BD-006`** pada roadmap revisi 11 |
| **`AC-BD-071` diterima dari `BE-BD-006`** (revisi 11) | **Satu kriteria masuk, tanpa requirement baru** — bunyi dan bukti yang diharapkan tetap mengikuti [acceptance-test-matrix](../testing/acceptance-test-matrix.md) §7. **`AC-BD-071`:** kantong `PendingReview` yang berada di lokasi penyimpanan **nonaktif** dicoba dialihkan ke pasien lain lewat endpoint `reallocate` → **ditolak** `VAL-BD-064`. Dasarnya `INV-BD-028` dan `DEC-BD-036`/`DEC-BD-037`: pengalihan adalah pengikatan kantong ke baris kebutuhan pasien — yaitu **alokasi dengan nama lain** — sehingga gerbang alokasi berlaku sama kerasnya. **Sudah terbukti di task lain dan tidak perlu dibuktikan ulang:** gerbang `EvaluateAllocationGateAsync` menghasilkan `VAL-BD-064` untuk kantong di lokasi nonaktif ([BE-BD-015](../task/report/backend/BE-BD-015.md) bagian 7), dan penolakan itu terbukti lewat endpoint `allocate` di `BE-BD-006` (`AC-BD-068`). **Sisa yang dibuktikan di sini:** bahwa endpoint `reallocate` benar-benar memanggil gerbang yang sama, sehingga kantong `PendingReview` di lokasi nonaktif ikut tertolak. **Kenapa di sini, bukan di `BE-BD-006`:** `AC-BD-071` menuntut endpoint `reallocate`, dan endpoint itu lahir di task ini |
| **Risk/owner** | Sedang / BDRS. Ketiga butir wewenang tetap terpisah |

---

### ✅ `BE-BD-010` — Koreksi pencatatan pemberian dua tahap

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 17 September 2026** ([laporan](../task/report/backend/BE-BD-010.md)). **Ketujuh acceptance terbukti runtime** lewat API sungguhan + verifikasi database: `AC-BD-050` `403 VAL-BD-024`; `AC-BD-048` `422 VAL-BD-025`; `AC-BD-049` `422 VAL-BD-049`; `AC-BD-086` koreksi `Requested`, pemenuhan tidak bergerak; `AC-BD-088` `422 VAL-BD-073` walau pengaju memegang kedua butir; `AC-BD-087` koreksi `Approved`, pemenuhan (3, 1) → (2, 2), kantong tetap `Issued`; `AC-BD-047` pemberian asal terbaca utuh. Kode `VAL-BD-016/024/025/049/073/074/075/076/077` persis `validation-matrix.md`; nol partial mutation; keputusan serentak `200` + `422 VAL-BD-075`; Billing tidak disentuh. Empat aktor non-SuperAdmin. Migration `20260917055332_AddBbkIssuanceCorrection` terterapkan `0 pending`; build `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS`. Dua keputusan pemilik sebelum implementasi tercatat sebagai `DEC-BD-053` (isian penjaga request-only `VAL-BD-025`/`049`) dan `DEC-BD-054` (koreksi `Approved` dikeluarkan dari `BD-DOM-17`). Belum ter-commit. **Riwayat:** 🟡 **READY / PENDING — belum diimplementasikan (per 17 September 2026).** Seluruh dependency tertutup, `OQ-BD-014` **CLOSED** lewat `DEC-BD-051`, dan `VAL-BD-049` terdefinisi lewat `DEC-BD-052`. Nol source, nol migration; ketujuh acceptance belum terbukti. **Riwayat:** 🟡 **PENDING — SIAP DIJADWALKAN 16 September 2026.** Penahannya `BE-BD-007` gugur ([laporan](../task/report/backend/BE-BD-007.md)). Belum ada source; wewenang menulis tetap diberikan terpisah lewat `build-module-backend`. Hanya penanda status yang berubah — acceptance criteria, dependency, dan scope tidak disentuh. **Riwayat:** ⛔ **BLOCKED lewat `BE-BD-007`** sampai 16 September 2026 |
| **Trace** | `DEC-BD-030/034/041/051/052/053/054`; `BD-DOM-23`, `BD-DOM-17`; `INV-BD-021/024/033` |
| **Dependency** | `G1` ✅, `G2b` ✅, `BE-BD-007` ✅; keputusan `OQ-BD-014` ✅ (`DEC-BD-051`) dan definisi `VAL-BD-049` ✅ (`DEC-BD-052`). **Riwayat:** `BE-BD-007` ⛔ sampai 16 September 2026 |
| **Acceptance** | `AC-BD-047/048/049/050/086/087/088` |
| **Risk/owner** | Sedang / BDRS |
| **Catatan** | **`OQ-BD-014` ditutup `DEC-BD-051` (17 September 2026):** sesudah koreksi disetujui kantong tetap `Issued`; koreksi hanya mengoreksi rekam pencatatan dan angka pemenuhan; penanganan fisik kantong di luar scope task ini; **nol state transition baru**. `VAL-BD-049` (`AC-BD-049`) kini terdefinisi `422` lewat `DEC-BD-052`. **Riwayat:** `OQ-BD-014` menahan detail implementasi jalur koreksi, **bukan** bentuknya |

---

### ✅ `BE-BD-017` — Golongan darah diminta tercatat pada order darah

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — 23 September 2026. Keempat acceptance `AC-BD-103`..`AC-BD-106` terbukti; Definition of Done terpenuhi.** Source ter-commit `7ac6f610`. `dotnet build` **`0 Error(s)` / `214 Warning(s)`** dengan **nol peringatan** pada kelima berkas order; migration `20260921032324_AddBbkBloodOrderRequestedBloodGroup` **terterapkan** di `QuilvianNewDevSukma` dengan **nol tertunda**; `has-pending-model-changes` bersih; QBE Strict **`PASS`** (8 berkas, `VIOLATION 0`); validasi runtime **R1–R9 `PASS`** ([laporan](../task/report/backend/BE-BD-017.md) bagian 5.1). **Batas bukti yang tetap melekat:** bukti runtime berupa **atestasi pemilik tanpa log primer** — preseden penutupan `FE-BD-006`; `verify-module-readiness` berwenang menilainya ulang. **Riwayat:** 🟡 SEBAGIAN — 1 dari 4 terbukti penuh, 23 September 2026 sebelum validasi runtime. **Riwayat:** 🟡 PENDING — siap dijadwalkan, belum dikerjakan (roadmap revisi 12 disetujui `Sukmagp` 19 September 2026); `G5` tertutup dan persetujuan proses klinis `DEC-BD-055` diberikan hari itu; nol source, nol migration. **Riwayat:** ⛔ **BLOCKED — belum dikerjakan (roadmap revisi 12, `DRAFT`, 18 September 2026).** Menunggu dua hal: **(1)** gerbang `G5` — naskah set kontrak `v5` disetujui pemilik; **(2)** persetujuan proses klinis atas `DEC-BD-055`, yang di register berstatus **BLOCKED** karena pemilik proses klinis belum pernah disebutkan namanya (`00-interview-decisions.md` §8.32). Nol source, nol migration |
| **Yang memblokir** | **Nihil sejak 19 September 2026.** **Riwayat:** `G5` · persetujuan proses klinis `DEC-BD-055` — dilepas oleh tanda tangan pemilik proses klinis, **atau** pernyataan tertulis `Sukmagp` bahwa ia bertindak sebagai pemilik proses klinis untuk keputusan ini |
| **Pekerjaan yang tetap aman** | Nol untuk source. `BE-BD-018` tidak bergantung pada task ini dan dapat berjalan begitu `G5` tertutup |
| **Outcome** | Setiap order darah **baru** mencatat golongan darah yang diminta — sebagai keterangan permintaan, terpisah dari golongan darah hasil pemeriksaan. Order lama jujur tanpa nilai |
| **Trace** | `DEC-BD-055`; `FE-BD-001`, `ASM-BD-004`; `INV-BD-011`, `INV-BD-014`; `BD-AGG-01` |
| **Kontrak** | api-contract **`v5`** — Blood Order, Amendment `v5` D1; `data/data-dictionary.md` `BbkBloodOrder.RequestedBloodGroup`; `validation-matrix.md` `VAL-BD-085` |
| **Reuse** | Enum `BloodType` yang sudah ada (`Enums/BloodType.cs`); jalur validasi, deteksi ganda, dan alokasi nomor `BE-BD-003` |
| **Scope** | Kolom `RequestedBloodGroup` (`BloodType?`) pada `BbkBloodOrder`, **bukan** baris; isian wajib `requestedBloodGroup` pada `CreateBloodOrderRequest`, `CreateManualBloodOrderRequest`, `ConfirmDuplicateOrderRequest`; tolak kosong / `NotDisclosed` / di luar enum dengan `400 VAL-BD-085` **sebelum** deteksi ganda dan alokasi nomor; `requestedBloodGroup` + label pada `BloodOrderDetailDto`; satu migration `ADD COLUMN` nullable tanpa default, tanpa index, **tanpa backfill** |
| **Di luar scope** | Membaca nilai ini pada `BbkBloodUnitService`, `BbkBloodGroupExamService`, atau gerbang klinis mana pun; mengisi order lama; enum kedua; kolom per baris |
| **Dependency** | `BE-BD-003` ✅, `BE-BD-005` ✅ (golongan darah sah untuk membuktikan `AC-BD-106`), gerbang **`G5`** ✅, persetujuan klinis **`DEC-BD-055`** ✅ (keduanya 19 September 2026). **Riwayat:** `G5` ⛔, `DEC-BD-055` ⛔ |
| **Acceptance** | `AC-BD-103`, `AC-BD-104`, `AC-BD-105`, `AC-BD-106` |
| **Verifikasi** | QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend. Build project aplikasi; `has-pending-model-changes` bersih sesudah migration dibuat; review diff/scope; verifikasi kontrak API dan proses bisnis lewat panggilan API sungguhan terhadap database pengembangan yang **diberi wewenang eksplisit**. Automated test tidak diwajibkan (`TEST_POLICY`) |
| **Wewenang database** | **Pembuatan** migration termasuk wewenang build task ini. **Penerapan** ke database mana pun **tidak** termasuk dan wajib diminta terpisah per database |
| **Risk/owner** | Sedang, bersinggungan dengan keselamatan klinis / pemilik proses BDRS + pemilik proses klinis |
| **DoD** | Keempat acceptance terbukti; `VAL-BD-085` terkirim dengan kalimat persis `validation-matrix.md`; nol pembacaan `RequestedBloodGroup` di luar layanan order; migration terterapkan di database pengembangan yang diberi wewenang dengan `0 pending`; laporan tracked `task/report/backend/BE-BD-017.md` |

---

### ✅ `BE-BD-018` — Order darah terbaca layar: penahanan ganda, kategori pembatalan, dan daftar kerja

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — 23 September 2026. Keenam acceptance `AC-BD-107`..`AC-BD-112` terbukti; Definition of Done terpenuhi.** Source ter-commit `7ac6f610`. `dotnet build` **`0 Error(s)` / `214 Warning(s)`** dengan **nol peringatan** pada berkas task ini; **nol migration** sesuai DoD dan `has-pending-model-changes` bersih; QBE Strict **`PASS`** (8 berkas, `VIOLATION 0`); validasi runtime **R1–R10 `PASS`** dengan **dua aktor non-SuperAdmin** ([laporan](../task/report/backend/BE-BD-018.md) bagian 5.1); catatan koreksi `BE-BD-003` §2.3 ditambahkan. **Batas bukti yang tetap melekat:** bukti runtime berupa **atestasi pemilik tanpa log primer**, dan `AC-BD-112` adalah **potret sesaat** — kebijakan akses baru dapat memunculkan kembali cacatnya kapan saja. **Riwayat:** 🟡 SEBAGIAN — nol dari 6 terbukti runtime, 23 September 2026 sebelum validasi runtime. **Riwayat:** 🟡 PENDING — siap dijadwalkan, belum dikerjakan (roadmap revisi 12 disetujui `Sukmagp` 19 September 2026); urutan kanonik sesudah `BE-BD-017`; nol source. **Riwayat:** ⛔ **BLOCKED — belum dikerjakan (roadmap revisi 12, `DRAFT`, 18 September 2026).** Menunggu gerbang `G5`: naskah set kontrak `v5` disetujui pemilik. `DEC-BD-056`/`057`/`058` sudah disetujui `Sukmagp`. Nol source |
| **Yang memblokir** | **Nihil sejak 19 September 2026.** **Riwayat:** `G5` |
| **Pekerjaan yang tetap aman** | Seluruhnya sejak 19 September 2026, mengikuti urutan kanonik. **Riwayat:** nol untuk source sebelum `G5`. Tidak bergantung pada `BE-BD-017`; keduanya menyentuh `BbkBloodOrderService`, `BbkBloodOrderController`, dan `BloodOrderDtos.cs`, sehingga disarankan **dijalankan berurutan** — bukan karena ketergantungan logis, melainkan supaya dua sesi builder tidak menyunting berkas yang sama bersamaan |
| **Outcome** | Layar order darah dapat mengenali penahanan order ganda beserta komponennya, mengetahui kategori alasan pembatalan yang sah dari backend, dan menampilkan daftar kerja `FE-BD-01` lengkap dari satu request |
| **Trace** | `DEC-BD-056`, `DEC-BD-057`, `DEC-BD-058`; `DEC-BD-044`, `DEC-BD-054`; `BD-DOM-17`, `BD-XINV-01`; `INV-BD-035` |
| **Kontrak** | api-contract **`v5`** — Blood Order, Amendment `v5` D2/D3/D4; `validation-matrix.md` `VAL-BD-001` (bentuk `errors`); `permission-audit-matrix.md` §2 |
| **Reuse** | Konvensi slot `errors` `BbkBloodUnitController.MapFailure`; `BloodOrderResult.DuplicateComponentIds` yang sudah dihitung; aturan `IsRequestingDoctorAsync` milik `CancelAsync`; perhitungan pemenuhan `GetFulfillmentAsync` |
| **Scope** | **D2** — `422 VAL-BD-001` dari `POST /` dan `/manual` membawa `errors { code, duplicateComponentIds }`; kegagalan lain tidak berubah. **D3** — `CancellationReasonCategory` turunan pada `BloodOrderDetailDto` (semua jawaban yang memulangkan detail), memakai fungsi yang sama dengan `CancelAsync`, `null` bila tidak dapat dibatalkan. **Hak akses** — pemeriksaan bahwa kebijakan akses pengembangan pemegang `BloodOrder : Cancel` juga memegang `BloodBankReason : Read`; nol butir baru. **D4** — query `bloodComponentId`; `Components[]` + `TotalIssuedQuantity` pada `BloodOrderListDto`, dihitung berkelompok per halaman dengan aturan `BD-DOM-17` yang sama. **Dokumentasi** — catatan koreksi bertanggal pada laporan `BE-BD-003` §2.3 tanpa mengubah status ✅ maupun buktinya (`00-interview-decisions.md` §8.32) |
| **Di luar scope** | Mengubah bentuk kegagalan order selain `VAL-BD-001`; menyimpan kategori pembatalan atau penghitung pemenuhan; menyertakan daftar alasan di detail order; membuat butir hak akses baru; migration kecuali bukti eksekusi membuktikan index diperlukan — bila begitu, berhenti dan minta keputusan pemilik |
| **Dependency** | `BE-BD-003` ✅, `BE-BD-010` ✅ (koreksi `Approved` yang dihormati `BD-DOM-17`), gerbang **`G5`** ✅ (19 September 2026). **Riwayat:** `G5` ⛔ |
| **Acceptance** | `AC-BD-107`, `AC-BD-108`, `AC-BD-109`, `AC-BD-110`, `AC-BD-111`, `AC-BD-112` |
| **Verifikasi** | QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend. Build project aplikasi; review diff/scope; verifikasi kontrak API lewat panggilan sungguhan dengan aktor non-SuperAdmin (dokter peminta dan petugas BDRS); pembuktian `AC-BD-111` lewat pencatatan kueri atau bukti setara; `has-pending-model-changes` bersih tanpa migration. Automated test tidak diwajibkan (`TEST_POLICY`) |
| **Risk/owner** | Sedang, berdampak pada performa daftar kerja / pemilik proses BDRS + pemilik arsitektur backend |
| **DoD** | Keenam acceptance terbukti; bentuk `errors` sama persis dengan `api-contract.md`; angka daftar sama dengan `GET /{id}/fulfillment`; nol migration; catatan koreksi laporan `BE-BD-003` ada; laporan tracked `task/report/backend/BE-BD-018.md` |

---

### ✅ `BE-BD-019` — Blood Order Date Range Filter

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — 23 September 2026. Kelima acceptance `AC-BD-113`..`AC-BD-117` terbukti dan Definition of Done terpenuhi.** Build **`0 Error(s)` / `214 Warning(s)`** (`00:57:47`) dengan **nol peringatan** pada keempat berkas task ini; baseline peringatan **tidak bertambah**. `has-pending-model-changes` **bersih**, **nol migration** sesuai DoD. `api-contract.md` memperoleh Amendment `v5` `D5`; `VAL-BD-086` baru pada matriks validasi; `AC-BD-113`..`AC-BD-117` pada bagian 12 matriks acceptance. **Validasi runtime R1–R12 `PASS`, dijalankan langsung agent** terhadap `QuilvianNewDevSukma` dengan keluaran HTTP sungguhan — bukan atestasi pihak lain ([laporan](../task/report/backend/BE-BD-019.md) bagian 7). `AC-BD-115` terbukti sampai ke detik batasnya: rentang efektif **`>= 2026-09-22T17:00:00Z`** dan **`< 2026-09-23T17:00:00Z`**, sama persis dengan contoh mengikat pada kontrak; hasilnya kebalikan persis dari apa yang akan terjadi bila pola `ResolveDateRange` yang keliru disalin. Database dikembalikan ke keadaan semula dan diverifikasi: 9 order, `min`/`max` sama, nol order baru. **Batas bukti:** aktor tunggal `superadmin` — `D5` tidak membedakan pelaku dan tidak menambah butir hak akses; `AC-BD-115` memakai `CreateDateTime` yang disetel, bukan order yang lahir alami pada jam itu (bagian 7.9). **Catatan build:** percobaan pertama gagal `MSB6006 csc.dll exited with code -1073741571` (`STATUS_STACK_OVERFLOW`) sesudah 42 menit — crash compiler, bukan kesalahan kode; build yang lulus dijalankan sesudah `dotnet build-server shutdown` dengan `-p:UseSharedCompilation=false`, sehingga kondisinya **tidak identik** dengan baseline `BE-BD-017`/`BE-BD-018` (bagian 6.1). **Riwayat:** 🟡 SEBAGIAN — source dan kontrak selesai, runtime belum, 23 September 2026. **Riwayat:** 🟡 siap dijadwalkan, belum dikerjakan, dibuka 23 September 2026 |
| **Yang memblokir** | **Nihil** |
| **Outcome** | Petugas BDRS dapat mempersempit daftar kerja order darah ke rentang tanggal tertentu, dan penyaring tanggal yang sudah tayang di layar `FE-BD-01` akhirnya berfungsi |
| **Asal** | `BD-UI-GAP-004` — layar merender pemilih tanggal dan dropdown periode, sementara `GET /blood-orders` tidak pernah menerima parameter tanggal apa pun. Pemilik memilih **Opsi B**: penyaringnya dipertahankan, backend yang menyesuaikan diri |
| **Trace** | `DEC-BD-059`, `DEC-BD-060`; `BD-UI-GAP-004`; `FE-BD-002` |
| **Kontrak** | api-contract — grup Blood Order. Amandemen **aditif** pada `GET /`; nol endpoint baru, nol butir hak akses baru, nol perubahan bentuk respons |
| **Reuse** | Pola `ResolveDateRange` pada `BankController` sebagai **acuan bentuk, bukan acuan perilaku** — perbandingan zona waktunya keliru untuk kolom UTC dan **tidak boleh disalin** (`DEC-BD-060`). `AppDateTimeHelper` untuk zona waktu aplikasi |
| **Scope** | **(1) Kontrak** — `api-contract.md` grup Blood Order memperoleh `startDate` dan `endDate` pada `GET /`, beserta aturan inklusivitas dan zona waktunya. **(2) Query DTO** — kedua parameter ditambahkan ke tanda tangan `GetAll` dan ke `BloodOrderDefaultFilterResponse` pada `GET /filters/metadata`. **(3) Penyaring service** — `GetPagedAsync` menyaring `BbkBloodOrder.CreateDateTime` dengan batas yang **dikonversi dari `Asia/Jakarta` ke UTC**, batas atas eksklusif pada `endDate + 1 hari`. **(4) Acceptance criteria** — `AC-BD-113`..`AC-BD-117` ditulis ke `testing/acceptance-test-matrix.md`. **(5) Validasi runtime** — dijalankan terhadap `QuilvianNewDevSukma` |
| **Di luar scope** | Migration, kolom, atau index baru — kecuali bukti eksekusi membuktikan index memang dibutuhkan; bila begitu, **berhenti dan minta keputusan pemilik**. Parameter `customPeriod` di backend — periode tetap dihitung layar dan dikirim sebagai rentang biasa. Penyaring tanggal pada endpoint Bank Darah lain. Perubahan apa pun pada frontend. Perbaikan pola `ResolveDateRange` milik modul lain |
| **Dependency** | `BE-BD-003` ✅, `DEC-BD-059` ✅, `DEC-BD-060` ✅ |
| **Acceptance** | `AC-BD-113`, `AC-BD-114`, `AC-BD-115`, `AC-BD-116`, `AC-BD-117` — rumusannya pada [laporan task](../task/report/backend/BE-BD-019.md) bagian 4 |
| **Verifikasi** | QBE preflight dan kesesuaian engineering dari `AGENTS.md` backend. Build aplikasi; review diff/scope; `has-pending-model-changes` bersih tanpa migration; verifikasi lewat panggilan sungguhan dengan aktor non-SuperAdmin, termasuk **order yang dibuat antara 00:00–07:00 WIB** sebagai pembuktian `DEC-BD-060` |
| **Risk/owner** | Sedang — salah zona waktu menyembunyikan order dari daftar kerja klinis tanpa jejak / pemilik proses BDRS + pemilik arsitektur backend |
| **DoD** | Kelima acceptance terbukti; `api-contract.md` diamandemen; `AC-BD-113`..`AC-BD-117` tertulis di matriks acceptance; nol migration; bukti runtime lintas batas hari WIB ada; laporan tracked `task/report/backend/BE-BD-019.md` |

---

## 6. Gerbang yang masih terbuka

| Gate | Pemilik | Menahan |
| --- | --- | --- |
| ~~**`G5`** naskah set kontrak `v5` disetujui (**baru, revisi 12**)~~ — **TERTUTUP 19 September 2026** | `Sukmagp` — pemilik modul dan kontrak | **Tidak lagi menahan.** **Riwayat:** menahan `BE-BD-017`, `BE-BD-018`, dan melalui keduanya `FE-BD-002`. Arah keempat keputusan sudah disetujui 18 September 2026; yang ditunggu persetujuan atas **naskah** kontrak `v5` |
| ~~**Persetujuan proses klinis `DEC-BD-055`** (**baru, revisi 12**)~~ — **DISETUJUI 19 September 2026** | `Sukmagp`, yang menyatakan berwenang sebagai pemilik proses klinis untuk keputusan ini | **Tidak lagi menahan.** **Riwayat:** menahan `BE-BD-017`; pemilik proses klinis belum pernah disebutkan namanya |
| ~~**`G4`** provider number-series~~ | ✅ **TERTUTUP 10 September 2026** — pemiliknya, `Andry`, menyatakan setuju; bukti `PLT-BE-003` dan `PLT-BE-004` | **Tidak lagi menahan.** Semula menahan 9 task backend dan 8 task frontend. **Nol gerbang terbuka per 10 September 2026** |

### 6.1 `G4` kini dependency lintas modul yang konkret

**Yang berubah 9 September 2026.** Sebelumnya `G4` tertahan hal yang tidak dapat dijadwalkan siapa
pun: pemilik kontrak engineering backend belum ditunjuk. Penghalang itu **hilang**, dan `G4`
berubah sifat — dari penghalang organisasi menjadi **dependency pengiriman lintas modul yang
punya nama, blueprint, dan gelombang**.

| Langkah menutup `G4` | Keadaan per 10 September 2026 |
| --- | --- |
| `OQ-PLT-007` — pemilik ditunjuk | ✅ **Tertutup** — `Andry` |
| `DEC-PLT-002`..`005`, `007`, `008` + `INV-PLT-001`..`004` | ✅ **`approved`** oleh `Andry` |
| Blueprint `PLT-SLICE-01` | ✅ **Ada, `DRAFT`** — `docs/module-blueprints/platform/` |
| `OQ-PLT-012`/`OQ-PLT-013` — Area dan prefix registry Platform | ✅ **Tertutup** 9 September 2026 → `DEC-PLT-009` Area `Platform`, `DEC-PLT-010` prefix `Num` |
| Approval blueprint `PLT-SLICE-01` | ✅ **Turun** 9 September 2026 oleh `Sukma Giri Pratama` — kontrak `v1` `approved` |
| Roadmap Platform | ✅ **Ada** — `docs/module-blueprints/platform/roadmap/`, 5 task backend + 1 frontend |
| `OQ-PLT-014` — baris registry `Platform`/`Num` dicatat dan `ACTIVE` | ✅ **Tertutup** 9 September 2026 — gerbang `P1` Platform tertutup; terbukti diterima checker |
| `PLT-BE-002` — tabel `NumNumberSeries` | ✅ **Selesai** 9 September 2026 |
| `PLT-BE-003` — alokator nomor durabel | ✅ **Selesai** 9 September 2026 — **`G4` tertutup secara kemampuan** |
| `PLT-BE-004` — bukti durabilitas & antrean di PostgreSQL | ✅ **Selesai** 10 September 2026 — 6 dari 6 lulus di `QuilvianNewDevSukma` (`RJ-BIL-DEC-019`); `AC-PLT-003/004/005/012` terbukti |
| Provider terimplementasi | ✅ **Ada** — `NumberSeriesAllocator`, 19 uji lulus |
| **`G4` tertutup** | ✅ **Ya — 10 September 2026.** Secara kemampuan sejak `PLT-BE-003`, secara bukti sejak `PLT-BE-004`, dan dinyatakan tertutup oleh pemiliknya, `Andry`. Tabel `NumNumberSeries` ada di `QuilvianNewDevSukma`; lingkungan lain belum |

**Dependency `G4` kini menunjuk satu task, bukan satu modul.** Gerbang ini tertutup ketika
**`PLT-BE-003`** (`NumberSeriesAllocator`) berdiri — lihat
[roadmap Platform](../../platform/roadmap/backend-roadmap.md) bagian 6.1.

**Urutan yang disarankan pemilik Platform, dan alasannya.** Secara teknis `G4` tertutup begitu
`PLT-BE-003` ada. Tetapi menjadwalkan `BE-BD-003` sebelum **`PLT-BE-004`** lulus berarti membangun
order darah di atas alokator yang durabilitasnya belum dibuktikan di PostgreSQL sungguhan. Bila
`AC-PLT-003` ternyata gagal, perbaikannya menyentuh mesin yang sudah dipakai order darah. Urutan
aman: `PLT-BE-001` → `002` → `003` → `004` lulus → baru sembilan task Bank Darah dijadwalkan.

✅ **Syarat urutan ini terpenuhi 10 September 2026.** `PLT-BE-004` lulus 6 dari 6 di PostgreSQL
`QuilvianNewDevSukma`, sehingga `BE-BD-003` tidak lagi dibangun di atas klaim durabilitas yang belum
diperiksa. Paragraf di atas dipertahankan sebagai riwayat.

**Dependency yang berlaku sampai 10 September 2026 (riwayat):** kesembilan task backend bertanda ⛔ menunggu gelombang
**`MVP-1` blueprint Platform** (`EPIC-PLT-01` + `EPIC-PLT-02`), bukan menunggu penunjukan siapa
pun. Rinciannya di `docs/module-blueprints/platform/04-prd-to-mvp.md` bagian 5.

**Diperbarui 12 September 2026, roadmap revisi 11 — koreksi tata kelola, nol eksekusi.** Pemilik
melepas `AC-BD-071` dari `BE-BD-006` ke `BE-BD-009`, pemilik endpoint `reallocate`, dan mencatat
kebijakan verifikasi repository di bagian 0.1. **Yang dapat dijadwalkan tetap `BE-BD-006`** — tidak
berubah — tetapi kini dengan satu perbedaan penting: **`BE-BD-006` dapat mencapai ✅ penuh secara
mandiri**, karena satu-satunya kriterianya yang menuntut endpoint `reallocate` sudah keluar. Sebelum
revisi 11, `BE-BD-006` akan berakhir 🟡 selesai sebagian dengan `AC-BD-071` menggantung — pola yang
sudah tiga kali terjadi pada `BE-BD-002`, `BE-BD-004`, dan `BE-BD-015`, dan yang setiap kali menuntut
revisi roadmap tambahan untuk menutupnya. Revisi 11 memutus pola itu di depan, bukan di belakang.
Jalur kritis tetap `BE-BD-006` → `BE-BD-007`. Nol status task berubah: `BE-BD-006` tetap 🟡,
`BE-BD-009` tetap ⛔ karena penahan `BE-BD-007` masih nyata. Nol source, nol migration, nol test,
nol task dieksekusi pada pass ini.

**Diperbarui 14 September 2026 — `BE-BD-006` ✅ SELESAI, kesembilan runtime acceptance terbukti.** Backend dijalankan atas wewenang eksplisit pemilik, dan kesembilan skenario dibuktikan lewat panggilan API sungguhan ditambah verifikasi read-only database ([laporan](../task/report/backend/BE-BD-006.md) bagian 9.2 dan 9.3). Data uji samaran bertanda `TEST-BD006-20260914094559` dibuat lewat endpoint resmi; nol data dev non-uji diubah. `AC-BD-046` memakai fixture database minimal atas **dua kali persetujuan eksplisit pemilik**, dan `BE-BD-007` **tidak** diimplementasikan untuk keperluan itu. **Akibatnya `BE-BD-007` naik ke 🟡 siap dijadwalkan**; `BE-BD-008` sampai `BE-BD-010` tetap ⛔ lewat `BE-BD-007`. **Satu perbaikan lintas-task dikerjakan atas persetujuan pemilik:** tiga registrasi DI master Bank Darah yang hilang lewat merge `27d737cd` dipulihkan di `Program.cs` (+8 baris), sehingga `BE-BD-001` dan `BE-BD-014` sempat turun ke 🟡 lalu kembali ✅ pada hari yang sama. `BE-BD-016` **30 deklarasi unik terhadap baseline 39** setelah rekonsiliasi output audit pemilik; `BloodUnit : Allocate` sudah ada dari BE-BD-006, bukan dibuat pada pass runtime ini. **Riwayat pencatatan yang dikoreksi:** pass runtime sebelumnya menyebut tetap 29/39 karena tidak membuat action baru. Tiga temuan lain dicatat tanpa diperbaiki: envelope `VAL-BD-016` tidak seragam, `HeldUnitCount` `0` pada penonaktifan lokasi, dan folder `Tests/` yang membuat `dotnet build` polos gagal. **Riwayat — 14 September 2026 sebelumnya: `BE-BD-006` 🟡 PARTIAL — READY FOR RUNTIME VALIDATION.** Pemeriksaan ulang bukti dan pembaruan laporan saja; nol source, nol migration, nol build, nol database disentuh ([laporan](../task/report/backend/BE-BD-006.md) bagian 5.5). Source (`02b70618`) dan migration (`6a7b193d`) masih utuh pada `HEAD` `baa1851b`, dan nol commit sesudahnya menyentuh kode. Implementation, migration artifact, dan static verification selesai; build evidence tersedia; QBE Strict `PASS`. **Yang tersisa hanya runtime acceptance** — kesembilan skenario `PENDING`. Backend sempat dijalankan pemilik untuk validasi runtime lalu dihentikan sebelum skenario dijalankan; isu Attendance Scheduler `FK_HrdAttendanceProcessingRun_AspNetUsers_TriggeredByUserId` dan timeout konektivitas PostgreSQL transient dicatat **di luar scope**. `BE-BD-007` sampai `BE-BD-010` tetap ⛔ sampai `BE-BD-006` ✅. Nol task backend baru dapat dijadwalkan; pekerjaan backend berikutnya adalah melanjutkan validasi runtime `BE-BD-006`.

**Riwayat — diperbarui 13 September 2026 — QBE Strict `BE-BD-006` `PASS`.** Sesudah pemilik meng-commit source (`02b70618`) dan migration (`6a7b193d`), QBE Strict **`PASS`** — `GitRange` `origin/QuilvianIntegrationBackend` (`719b1c82d73748b194a43ea511886ae398c80f8a`)..`HEAD`, 11 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, exit code `0` ([laporan](../task/report/backend/BE-BD-006.md) bagian 5.4). Kesebelas berkas yang dievaluasi adalah seluruh perubahan kode task ini. **Task tetap 🟡** — satu-satunya penahan yang tersisa adalah penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS`, dan kesembilan acceptance criteria runtime tetap `BLOCKED`. Nol task backend dapat dijadwalkan.

**Riwayat — diperbarui 13 September 2026 — bukti build dan migration `BE-BD-006`.** Build Debug pemilik `0 Error(s)` / `191 Warning(s)`; source ter-commit pada `02b70618`. Migration `20260913070556_AddBbkBloodUnitAllocation` terbentuk dengan scope bersih dan `has-pending-model-changes` bersih ([laporan](../task/report/backend/BE-BD-006.md) bagian 5.3). **Penerapan database `BLOCKED — UNRELATED PENDING MIGRATIONS`**: migration berstatus `(Pending)` di belakang migration modul lain yang juga tertunda, sehingga menerapkannya berarti ikut mengubah skema modul lain tanpa wewenang. **Task tetap 🟡**, kini dengan satu penahan yang bernama jelas dan berada di luar `BE-BD-006`. **Nol task backend dapat dijadwalkan**: `BE-BD-007` sampai `BE-BD-010` tetap ⛔ lewat `BE-BD-006`, dan yang membuka jalurnya adalah penyelesaian migration tertunda oleh pemilik database, bukan pekerjaan source.

**Riwayat — diperbarui 12 September 2026 sesudah `BE-BD-006` dikerjakan.** Source alokasi kantong dan pembatalan alokasi lengkap ([laporan](../task/report/backend/BE-BD-006.md)): tabel `BbkBloodUnitAllocation` beserta index unik terfilter satu-alokasi-aktif, endpoint `POST /{id}/allocate` dan `POST /{id}/cancel-allocation`, butir hak akses `BloodUnit : Allocate`, dan pemakaian gerbang `EvaluateAllocationGateAsync` milik `BE-BD-015` tanpa satu baris pun diduplikasi. **Task belum ✅ dan sengaja tidak ditandai begitu:** pemilik meminta build dijalankan manual olehnya, sehingga `dotnet build` `NOT RUN`, migration belum dibuat, database belum disentuh, dan **0 dari 9 acceptance criteria terbukti** — kesembilannya `NOT EXECUTED`, bukan gagal. QBE `DEFERRED — REQUIRES USER COMMIT` karena source belum ter-commit. **Yang dapat dijalankan berikutnya adalah build manual pemilik**, lalu lanjutan task ini dari keadaan terverifikasi; urutan pastinya ada di bagian 8 laporan. `BE-BD-007` tetap ⛔ — rantai dependency-nya menunggu `BE-BD-006` ✅, bukan hanya source-nya ada. Satu risiko klinis dicatat dan **tidak** dikarang jalan keluarnya: kecocokan komponen kantong terhadap baris kebutuhan **tidak** diperiksa, karena kontrak `v4` tidak memuat aturan maupun kode galatnya — keputusannya milik pemilik proses BDRS.

**Riwayat — diperbarui 11 September 2026, roadmap revisi 10:** pemilik meneruskan verifikasi final bagian alokasi `AC-BD-060`/`068`/`070` dari `BE-BD-015` ke `BE-BD-006`; `060`/`068` sudah tercantum di `BE-BD-006` dan tidak diduplikasi, `070` ditambahkan. Bukti tingkat gerbang `BE-BD-015` dipertahankan. Akibatnya `BE-BD-015` ✅ ([laporan](../task/report/backend/BE-BD-015.md)). **Yang dapat dijadwalkan kini `BE-BD-006`** — jalur kritis `BE-BD-006` → `BE-BD-007`. Di frontend, `FE-BD-012` kehilangan penahan backend-nya; statusnya tetap diputuskan pada [frontend-roadmap.md](frontend-roadmap.md), yang tidak disentuh pass ini.

**Riwayat — diperbarui 11 September 2026 sesudah `BE-BD-015`:** `BE-BD-015` 🟡 selesai sebagian ([laporan](../task/report/backend/BE-BD-015.md)) — seluruh scope selesai, 9 dari 12 kriteria penuh. **Nol task backend dapat dijadwalkan**: `BE-BD-006` menunggu `BE-BD-015` ✅, sedangkan `BE-BD-015` menunggu bagian alokasi `AC-BD-060/068/070` yang hanya dapat dibuktikan endpoint `BE-BD-006`. Jalan keluarnya keputusan pemilik roadmap: teruskan bagian itu ke `BE-BD-006`, seperti revisi 9. Di frontend, angka kantong tertahan yang ditunggu `FE-BD-011` kini tersedia di backend.

**Riwayat — diperbarui 11 September 2026, roadmap revisi 9:** pemilik meneruskan `AC-BD-023`/`032` ke `BE-BD-015` dan `AC-BD-033` ke `BE-BD-006`, sehingga `BE-BD-004` ✅. **Yang dapat dijadwalkan kini `BE-BD-015`** — jalur kritis `BE-BD-015` → `BE-BD-006` → `BE-BD-007`. Di frontend, `FE-BD-003` kehilangan penahan backend-nya.

**Riwayat — diperbarui 11 September 2026 sesudah `BE-BD-012`:** `BE-BD-012` ✅ **selesai** ([laporan](../task/report/backend/BE-BD-012.md)). **Nol task backend dapat dijadwalkan** sampai pemilik roadmap memutuskan penerusan tiga kriteria `BE-BD-004`; sesudahnya `BE-BD-015` terbuka di jalur kritis. Di frontend, `FE-BD-010` kehilangan penahan backend-nya.

**Riwayat — diperbarui 11 September 2026, roadmap revisi 8:** `BE-BD-012` 🟡 **siap dijadwalkan kembali**. Pemilik memutuskan aturan tarif dan sumber unit/kelas (`DEC-BD-048`, `DEC-BD-049`) dan mengganti kriterianya dengan `AC-BD-098` sampai `AC-BD-102`. `BE-BD-015` tetap menunggu keputusan penerusan tiga kriteria `BE-BD-004`.

**Riwayat — diperbarui 11 September 2026 sesudah `BE-BD-012`:** `BE-BD-012` ⛔ — builder berhenti sebelum implementasi karena tiga keputusan ([laporan](../task/report/backend/BE-BD-012.md)). **Nol task backend dapat dijadwalkan** sampai pemilik roadmap memutuskan penerusan kriteria `BE-BD-004` dan rumah kriteria `BE-BD-012`, dan pemilik Billing/BDRS memutuskan aturan tarif.

**Riwayat — diperbarui 11 September 2026 sesudah `BE-BD-004`:** `BE-BD-004` 🟡 selesai sebagian — seluruh scope-nya selesai, tiga kriteria menunggu `BE-BD-015` dan `BE-BD-006`. Yang dapat dijadwalkan kini `BE-BD-012`. `BE-BD-015` terbuka begitu pemilik roadmap meneruskan ketiga kriteria itu ([laporan](../task/report/backend/BE-BD-004.md) bagian 6).

**Riwayat — yang dapat dijadwalkan per 11 September 2026 sebelum `BE-BD-004`:** `BE-BD-004` dan `BE-BD-012` di backend — keduanya terbuka setelah `BE-BD-003` ✅. `BE-BD-004` berada di jalur kritis `BE-BD-004` → `BE-BD-015` → `BE-BD-006` → `BE-BD-007`, sehingga disarankan lebih dulu. Di frontend, `FE-BD-002` kehilangan penahan backend-nya.

**Riwayat — yang dapat dijadwalkan per 10 September 2026:** `BE-BD-003` di backend, dan `FE-BD-009` di
frontend — lihat [frontend-roadmap.md](frontend-roadmap.md). **Riwayat:** pada 9 September 2026 nol
task backend dapat berjalan sendiri, karena `BE-BD-005` dan `BE-BD-011` sudah menghabiskan jalur
terbuka.

**Blocker yang bukan gerbang** — dicatat supaya tidak hilang, tidak satu pun menahan task:

| ID | Ringkasan | Terdampak |
| --- | --- | --- |
| ~~`DEC-BD-016`~~ | Persetujuan pemilik Billing atas konteks sumber biaya | **Ditutup 17 September 2026** — `approved` `Sukmagp`; `BE-BD-013` ✅ ([laporan](../task/report/backend/BE-BD-013.md)). **Riwayat:** `BE-BD-013` future scope |
| `OQ-BD-012` | Jam masa berlaku bukti kecocokan per komponen | Nilainya dari konfigurasi master saat eksekusi |
| ~~`OQ-BD-014`~~ | Keadaan kantong setelah dikoreksi | **Ditutup `DEC-BD-051`** — tidak lagi menahan `BE-BD-010` |
| `DEF-BD-003` | Apakah semua komponen menuntut bukti kecocokan sama | Aturan per komponen saat implementasi |

---

## 7. Yang sengaja tidak ada di roadmap ini

| Butir | Alasan |
| --- | --- |
| ✅ `BE-BD-013` penyaluran biaya ke Billing | **SELESAI 17 September 2026** ([laporan](../task/report/backend/BE-BD-013.md)) — `DEC-BD-016` disetujui `Sukmagp`; ketiga acceptance terbukti runtime; tidak lagi termasuk butir yang sengaja tidak ada. **Riwayat:** future scope; `DEC-BD-016` `OPEN DECISION`. Acceptance `AC-BD-026`, `AC-BD-027`, `AC-BD-058` — `026` dan `058` dipindah dari `BE-BD-012` pada revisi 8 |
| Integrasi HCLAB | `DEC-BD-022` menempatkannya di luar MVP |
| Integrasi PMI otomatis | `DEC-BD-002` — permintaan dicatat, pengiriman manual |
| Task frontend | Ada di [frontend-roadmap.md](frontend-roadmap.md) |
| Penelusuran requirement → test | Ada di [requirement-traceability.md](requirement-traceability.md) |
