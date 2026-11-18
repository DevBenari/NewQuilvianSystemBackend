# MASTER INSTRUKSI AGENT — FINISHING ULANG HRD MENU 07: PENEMPATAN, MUTASI & PROMOSI

**Versi:** Revisi pascaaudit source dan laporan Agent Antigravity — 18 November 2026 (WIB)  
**Sifat:** **PROMPT EKSEKUSI PERBAIKAN SOURCE NYATA**, bukan dokumen audit-only, bukan klaim sudah selesai, bukan rencana untuk diwawancarai ulang.  
**Menu target:** Kepegawaian > **07. Penempatan, Mutasi & Promosi**; **tepat lima Submenu** berdasarkan registry pengguna.  
**Target penomoran FIN yang DIPERTAHANKAN:** `FIN-HRD-07-BE-001` s.d. `FIN-HRD-07-BE-009` (sembilan task) dan `FIN-HRD-07-FE-001` s.d. `FIN-HRD-07-FE-007` (tujuh task). Jangan buat namespace nomor baru untuk perbaikan ini.  
**Urutan kerja WAJIB:** preflight izin/aturan singkat → **BACKEND dahulu** → penguncian kontrak API → **FRONTEND** → pengujian regresi dan UAT live (bila diizinkan dan tersedia) → laporan berbukti.  
**File master yang harus DIPERBARUI di workspace BE bila penulisan dokumen diizinkan:** `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/INSTRUKSI_AGENT_HRD_MENU_07_PENEMPATAN_MUTASI_DAN_PROMOSI.md`. **Perbarui file existing ini**, jangan menambah master prompt kedua.  
**Laporan existing yang HARUS direkonsiliasi sesudah implementasi:** `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md`. Status laporan saat audit adalah **klaim Agent, bukan UAT PASS**.  
**Snapshot GitHub read-only untuk analisis:** FE `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2` HEAD `f7a1f0917eb82e95b0a6d02842362637a8d2b089`; BE `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` HEAD `90e36deefe012801469de880631322fd40293117`; Engineering Skills `DevBenari/QuilvianEngineeringSkills` @ `main` HEAD `201af64552b27873fd9af83c6722cf11b441bb1e`. **Agent harus cek ulang HEAD dan perubahan lokal sebelum mulai.**  
**Batas kejujuran:** `IMPLEMENTED IN SOURCE` bukan `VERIFIED WORKING`; `CSS CHECK PASS` bukan `BUILD PASS`; `READY FOR UAT` bukan `UAT PASS`; laporan `100%` tidak sah bila ditemukan P0, test wajib belum dijalankan, atau bukti database tidak ada.

---

## 1. Role

Anda adalah **Senior ASP.NET Core 9 / EF Core / PostgreSQL Engineer, Next.js 16 App Router Engineer, Analis Proses Bisnis Kepegawaian Rumah Sakit, Application Security Engineer, dan QA/UAT Engineer** untuk Quilvian V2. **Kerjakan langsung perbaikan kode**, jangan mengulang analisis sebagai output akhir. Ruang tugas Anda ialah **memperbaiki implementasi Menu 07 yang sudah sebagian dibangun**, menjaga integrasi dengan Menu 04/05/06/08, bukan membangun modul baru paralel.

### 1.1 Repository, branch, mode, target tulis

| Fase | Mode resmi | Repository/branch | Target tulis terbatas |
|---|---|---|---|
| 0 — hanya bila keputusan/kontrak canonical betul-betul perlu sinkronisasi dan izin dokumen tersedia | `MODULE BLUEPRINT MODE` | BE `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | Hanya `docs/module-blueprints/human-resource/` yang relevan dengan Menu 07; **source aplikasi read-only** pada fase ini |
| 1 — **WAJIB PERTAMA** | `BACKEND MODE` | BE `DevBenari/NewQuilvianSystemBackend` @ `MHamzah` | `Areas/Corporate/HumanResource/WorkforceCore/{Controllers,Services,DTOs,Models,Workers}/**`, `Program.cs` bila memang diperlukan, file test scoped; dependencies `LifecycleManagement`/`MasterData` hanya untuk bug terbukti dan setelah ownership gate. FE **read-only** |
| 2 — setelah API/DTO backend stabil | `FRONTEND MODE` | FE `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2` | `src/app/hr/workforce-core/**`, `src/components/view/hr/workforce-core/**`, `src/lib/services/hr/**` terkait, hook/utility/style yang memang dipakai, `src/utils/menu-sidebar/menu-items.jsx`, dan test scoped. BE **read-only** |
| 3 — hanya bila pemilik memberikan izin eksplisit lintas repo | `CROSS-REPO MODE` | FE dan BE pada branch pin di atas | Hanya file pada fase 1–2 yang diperbolehkan; tidak membuka wewenang universal |
| 4 — QA dan dokumentasi | Mode test dan mode dokumen yang sah | Repo terkait | Test/report berbukti; dokumen laporan hanya jika mode dan target tulis mengizinkan |

**Mandat ini adalah permintaan implementasi pada workspace Agent yang sah, bukan perintah perubahan repository oleh penyusun prompt.** Sebelum menulis apa pun, verifikasi working tree, branch, upstream, dan izin mode serta target tulis. Jangan otomatis `git checkout`, `git fetch`, `git pull`, `git commit`, `git push`, `git merge`, `git rebase`, `git reset`, `git stash`, membuat PR, deploy, atau membuat/menjalankan migration dan SQL write tanpa izin tersendiri yang eksplisit. Tidak ada otorisasi otomatis menjalankan database produksi.

### 1.2 Engineering Skills wajib dipanggil (bukan sekadar dibaca)

1. Baca `AGENTS.md` dan `CLAUDE.md` **kedua repo**, canonical `agents/rules/GLOBAL_RULES.md` dan `agents/rules/README.md` di Engineering Skills `main`.
2. Untuk **setiap fase penulisan backend**, invoke registered runtime Skill **`quilvian-engineering-skills:build-module-backend`** lebih dahulu. Untuk **setiap fase penulisan frontend**, invoke **`quilvian-engineering-skills:build-module-frontend`** lebih dahulu. Jika runtime Skill tidak tersedia: laporkan persis **`BLOCKED — Quilvian runtime skill unavailable`** pada fase terkait; jangan mengaku telah memanggilnya atau fallback manual ke `SKILL.md`.
3. Backend: taati `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, serta `rules/backend/{TASK_RULES,TASK_CLASSIFICATION,API_RULES,DATABASE_RULES,CROSS_REPO_RULES,REVIEW_RULES,TEST_POLICY,REPORT_TEMPLATE}.md` yang relevan. Tentukan owner dan ratchet `NEW CODE`/`TOUCHED LEGACY`; jangan membuat entity persisted baru, sequence/table baru, atau migrasi tanpa gate QBE dan izin.
4. Frontend: taati `rules/frontend/{frontend-architecture,base-component-catalog,base-component-decision-gate,page-composition-patterns,design-tokens,ui-consistency-checklist,test-policy,REPORT_TEMPLATE}.md`; dokumentasi `rules/rule-output/`; UAT `rules/testing/`.
5. **Jangan berhenti setelah membaca aturan, mengulang wawancara, atau membuat rencana baru.** Baseline Menu 07 dan perbaikan dalam dokumen ini telah disetujui; setelah preflight sah, langsung ubah source, jalankan test yang diizinkan, dan buktikan hasilnya.

---

## 2. Task

### 2.1 Tujuan, batas bisnis dan definisi hasil

Rumah sakit harus dapat mengetahui **pada tanggal acuan** pegawai bekerja di unit mana, jabatan apa, kepada siapa melapor, berdasarkan SK/batch apa, siapa pembuat/pengaju/penyetuju, dan apakah perubahan sudah **sah serta efektif**. Data harus benar-benar berasal dari database dan API riil. Jangan memakai dummy data, `setTimeout` untuk sukses palsu, optimistic success tanpa konfirmasi server, UUID acak untuk FK, atau menyetel status hanya dalam memori frontend.

**Lima Submenu resmi (pertahankan label dan urutannya):**

1. **Penempatan Awal**
2. **Perpindahan Unit**
3. **Rotasi Promosi**
4. **Uraian Tugas**
5. **Atasan Langsung**

**Batas kepemilikan modul:** Master Jabatan/Uraian Tugas tetap dikelola Menu 05; master pegawai Menu 04; penerbitan SK dan arsip berada pada capability Menu 06 yang direuse; kontrak awal dan tanggal masuk kerja dari Menu 08/onboarding; gaji dan nominal dari jalur Payroll tersendiri. Di Menu 07 gunakan **contextual views**, bukan master/approval/endpoint duplikat.

**Keputusan canonical yang mengikat:** `HRD-DEC-031`, `HRD-DEC-036`, `HRD-DEC-047`, `HRD-DEC-048`, `HRD-DEC-049`, `HRD-DEC-050`, `HRD-DEC-054`, `HRD-DEC-055`, `HRD-DEC-056`, `HRD-DEC-057` pada `docs/module-blueprints/human-resource/00-interview-decisions.md`. Maknanya di antaranya: maker-checker; sumber penempatan berbasis tanggal efektif; satu SK satu event; satu primary per jenis dan tanggal; backdate tidak boleh melewati periode absensi Closed; batch data awal disetujui oleh aktor berbeda; gaji tidak dimutasi lewat SK/batch; scan SK bertanda tangan; SK Applied immutable dan reversal melalui event/SK baru.

### 2.2 Keputusan prioritas audit — bukan daftar yang cukup dicentang tanpa mengubah kode

| ID temuan | Prioritas | Fakta source yang perlu diperbaiki | Task FIN pemilik |
|---|---|---|---|
| `AUD-HRD07-01` | **P0** | FE `initial-placement-batch-view.jsx` memakai `isPending` pada disabled button tanpa definisi → ReferenceError ketika evaluasi kondisi tersebut tercapai | `FE-002`, `FE-007` |
| `AUD-HRD07-02` | **P0** | Backend `ValidateBatchByIdAsync` hanya menolak `Executed`, masih bisa menulis ulang `PendingApproval`/`Approved` menjadi `Validated`/`Draft`; API `POST /{id}/validate` mengubah DB tetapi permission hanya `Read` | `BE-006` |
| `AUD-HRD07-03` | **P0** | FE Uraian Tugas meminta flat keys seperti `effectivePositionId`, `jobDescriptionId`, `duties`, sedangkan BE mengirim `positionId`, nested `jobDescription`, `items`, `versionHistory` | `BE-007`, `BE-008`, `FE-005` |
| `AUD-HRD07-04` | **P0** | Route `/personnel-actions/create?category=Promotion|Rotation` tidak dibaca oleh editor; initial `actionCategory` selalu `Mutation` | `FE-004` |
| `AUD-HRD07-05` | **P0** | `CancelOrderAsync` menerima `Scheduled` walau tidak ada `cancel-request` yang sah; jalur direct `POST /cancel` dapat melewati permintaan pembatalan | `BE-002`, `FE-003`, `FE-004` |
| `AUD-HRD07-06` | **P0** | Worker `ProcessScheduledOrdersAsync` dapat menerapkan SK yang memiliki permintaan pembatalan menunggu keputusan | `BE-002`, `BE-005` |
| `AUD-HRD07-07` | **P0** | `GenerateOrderNumberAsync` membaca list sebelum `lock` in-memory; tidak aman untuk multi-thread/multi-instance DB | `BE-003` |
| `AUD-HRD07-08` | **P0** | Jalur SK `ValidatePlacementCombinationAsync` hanya jelas menolak manager=self, sedangkan jalur manager-assignment langsung memiliki `WouldCreateCycleAsync`; cycle lewat SK masih mungkin | `BE-004`, `BE-008` |
| `AUD-HRD07-09` | **P1** | Query effective job memakai `IsActive` dan fallback `profile.PrimaryPositionId`, berisiko salah untuk `asOfDate` historis | `BE-007`, `FE-005` |
| `AUD-HRD07-10` | **P1** | Eksekusi batch dengan `EffectiveStartDate` masa depan langsung mengubah mirror `PrimaryPositionId/PrimaryDepartmentId` | `BE-005`, `BE-006` |
| `AUD-HRD07-11` | **P1** | Worker dan validasi beberapa tempat menggunakan `DateTime.UtcNow.Date`, bukan kalender tanggal bisnis RS yang dikonfigurasi | `BE-005` |
| `AUD-HRD07-12` | **P0 (privacy)** | `GetBatchByIdAsync` mengembalikan `BaseSalary = r.BaseSalary`; endpoint baca memakai `Read`, tidak ada amount gate spesifik di jalur ini | `BE-003`, `BE-006`, `FE-002` |
| `AUD-HRD07-13` | **P1** | Onboarding `WfpNewHireAssignmentService` masih fallback `DepartmentId = organizationUnitId` jika master departemen tidak ditemukan, padahal keduanya entity berbeda | `BE-006`, `BE-009` |
| `AUD-HRD07-14` | **P0** | `employee-managers-view.jsx` mengarah ke editor SK tanpa kategori; editor default `Mutation`, sehingga penggantian atasan bisa tersimpan sebagai mutasi | `BE-008`, `FE-006` |
| `AUD-HRD07-15` | **P1** | Guard once-per-employee batch hanya memeriksa batch berstatus `Executed` saat execute; dua batch dapat sama-sama `Approved` untuk pegawai yang sama | `BE-006` |
| `AUD-HRD07-16` | **P1** | Worker menelan error per SK tanpa log detail dan tanpa outcome yang terobservasi; status, transaction dan idempotensi lintas instance belum terbukti | `BE-005`, `BE-009` |
| `AUD-HRD07-17` | **P1** | Daftar direct assignment masih membuka `POST/PUT/PATCH/DELETE`; guard perlu diuji terhadap assignment terjadwal, status utama, dan jalan pintas bypass SK | `BE-004` |
| `AUD-HRD07-18` | **P1** | Laporan Agent mengklaim 100% meskipun build dan UAT nyata belum terbukti; tabel laporan mencantumkan endpoint yang tidak tepat | `BE-009`, `FE-007` |

**Larangan khusus:** Jangan menghapus fitur lama yang sudah benar. Jangan menyimpulkan seluruh bug dengan grep saja: telusuri call chain aktual, bentuk response, status DB, dan permission. Ketika audit menemukan source berbeda dari snapshot, perbaiki menurut kontrak canonical terbaru, catat diff dan SHA baru. Tidak perlu melakukan wawancara ulang atas 18 gap di atas.

### 2.3 Instruksi rinci per Submenu

### Submenu 1 - Penempatan Awal

**Bisnis RS dan aktor.** HR Admin menyiapkan data penempatan awal pegawai baru maupun baseline pegawai existing; HRD yang berbeda menyetujui; pelaksana resmi menjalankan batch setelah approval; atasan/unit tujuan dan Payroll memperoleh informasi sesuai hak akses. Contoh: 10 pegawai existing belum memiliki `WfpPositionAssignment` yang konsisten. HR membuat batch, validasi semua baris, ajukan, HRD lain menyetujui, lalu eksekusi atomik. Satu pegawai tidak boleh berhasil lewat dua batch awal.

**Input dan output yang harus nyata.** Input pegawai (`WorkforceProfileId`), unit organisasi, departemen, posisi, manager bila ada, tanggal efektif, bukti dan alasan sesuai master existing. Output `HrdInitialPlacementBatch` status resmi, rincian validasi per baris, penempatan primer efektif dan riwayat. `BaseSalary` bukan sumber penetapan gaji batch, dan jangan dipaparkan melalui API `Read`.

**Alur wajib:** `Draft → Validated → PendingApproval → Approved → Executed`; hanya aktor yang berhak melakukan transisi, dan pembuat/pengaju **tidak boleh** menjadi approver. Batch yang sudah diajukan/disetujui **immutable** terhadap validasi/edit tanpa proses revisi resmi yang tercatat. Pada akhir transisi, status dan data bertahan setelah reload serta re-query backend.

**Source reuse:**
- BE `WorkforceCore/Services/HrdInitialPlacementBatchService.cs` (`ValidateBatchByIdAsync`, `SubmitBatchAsync`, `ApproveBatchAsync`, `ExecuteBatchAsync`); `Controllers/HrdInitialPlacementBatchController.cs`; `Models/HrdInitialPlacementBatch*`; `Services/WfpNewHireAssignmentService.cs`.
- FE `src/app/hr/workforce-core/initial-placements/page.jsx`, `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`, `src/lib/services/hr/initial-placement-batch-service.js`.
- API existing: `/api/v1/corporate/human-resource/workforce-core/initial-batches` (detail di tabel Kontrak API, **bukan** `/initial-placement-batches`).

**Perbaikan backend wajib:**
1. Jadikan `ValidateBatchByIdAsync` **state-aware**: menolak mutasi untuk `PendingApproval`, `Approved`, `Executed`, atau status terminal yang relevan; jangan downgrade approval ke `Validated`. Jika revisi diizinkan, buat alur return/revision resmi dengan audit dan izin, jangan reset diam-diam. Pengujian meliputi direct HTTP call oleh role `Read`.
2. `POST /{id}/validate` adalah operasi yang menulis database: pindahkan ke permission aksi write (`Update` atau action validasi tersendiri yang telah didaftarkan sesuai access governance) dan validasi actor di server. Pemanggilan unauthorized harus 403; authorized memiliki log actor.
3. Validasi once-per-employee **sebelum approval dan saat execute**, terhadap batch lain dengan status `Approved`/`Executed` dan status lain yang menyebabkan konflik. Tangani race dua approver di dua sesi/instance melalui penguncian DB/transaksi atau constraint yang resmi. Jika perlu migration yang belum diotorisasi: tandai blocker schema, lanjut perbaikan yang tidak perlu schema.
4. Validasi setiap row DB/live tepat sebelum eksekusi. Transaksi **all-or-nothing**: bila 1/10 baris invalid, 0 penempatan baru tersimpan; retry tidak menggandakan primary, manager, atau history. Jangan merusak baseline atau menimpa penempatan efektif lain.
5. Data awal masa depan **boleh terjadwal** namun jangan langsung mengganti `MstWorkforceProfile.Primary*` mirror; mirror dihitung dari assignment efektif per tanggal bisnis. Jika alur batch tidak mendukung future effective date yang aman, tahan eksekusi dan tampilkan alasan jujur sampai mekanisme scheduler tersedia.
6. Hapus `BaseSalary` dari DTO biasa, baik list maupun detail dan responses validasi yang dapat dibaca role HR biasa; atau mask di server berdasarkan permission amount eksplisit. Jangan hanya menghapus field FE, jangan menyimpan nilai salary input tanpa jalur Payroll yang disetujui.
7. Perbaiki `WfpNewHireAssignmentService`: `DepartmentId` harus berasal dari `MstOrganizationUnit.DepartmentId`, `MstPosition.DepartmentId`, atau relasi master sah, tidak boleh memakai `organizationUnitId` sebagai `DepartmentId`. Bila tidak valid, fail closed/response validasi spesifik. Pastikan kontrak awal Menu 08, batch awal Menu 07 dan Payroll tidak membuat penempatan/gaji ganda serta tidak mengubah mirror sebelum efektif.

**Perbaikan frontend wajib:**
1. Tangani bug `isPending` dengan state yang terdefinisi dan guard yang benar. Tombol **Validasi Integritas** hanya aktif untuk status yang boleh divalidasi BE; jangan hanya menambahkan `const isPending` tanpa menyelesaikan kebenaran state machine.
2. Setelah `submit`, `approve`, `execute`, reload detail dan ringkasan dari API; error harus menampilkan respons backend, tidak mengarang sukses. Tombol harus berdasarkan status **dan izin user**; unauthorized 403 tidak boleh tetap tersedia sebagai aksi seolah berhasil.
3. Jika batch sudah `PendingApproval` atau `Approved`, tombol validasi **tidak tersedia**; jika status berubah di tab lain, action harus mendapat conflict response dan UI me-refresh.
4. Pindahkan salary dari output yang terbaca pengguna biasa; status `Approved` sebelum execute dibedakan jelas dari `Executed`; tampilkan keterangan deferred future-date bila implementasinya memerlukan antrean.

**Skema halaman target:**

```text
Penempatan Awal
[Hero] Penempatan Awal Pegawai     [Buat Batch] [Template]
[SummaryGrid] Draft | Perlu Validasi | Menunggu Persetujuan | Disetujui | Selesai
[DataFilter] Pencarian pegawai | Unit | Status | Tanggal efektif
[DataTable] Batch / Pegawai / Status / Masalah validasi / Tanggal efektif / Aksi
[Panel detail] Ringkasan batch + error per baris + timeline approval
[Actions by role/state] Validasi -> Ajukan -> Setujui (aktor lain) -> Eksekusi
[InformationAlert] Hasil server dan alasan tindakan ditolak
```

**Acceptance:** tidak ada `ReferenceError`; read-only user tidak bisa mengubah batch via `/validate`; approval tidak dapat di-downgrade; status konsisten lintas refresh; gaji tidak bocor; future effective date tidak aktif prematur; dua eksekusi paralel tidak menggandakan primary.

### Submenu 2 - Perpindahan Unit

**Bisnis RS dan aktor.** Mutasi dari unit asal ke tujuan (misalnya Rawat Inap → ICU) memerlukan satu SK resmi, scan bertanda tangan, approval oleh aktor berbeda, cek periode absensi, kompetensi/kredensial sesuai unit, tanggal efektif dan histori. Perubahan unit tidak boleh memintas pengesahan melalui endpoint CRUD penempatan langsung.

**Alur:** Buat draf SK kategori `Mutation` → unggah scan → submit → approver berbeda memutuskan → jika tanggal efektif sekarang/masa lalu yang diizinkan maka `Applied`, jika masa depan `Scheduled` → worker apply tepat pada tanggal efektif. Pembatalan Scheduled memerlukan **permintaan**, pemeriksaan oleh pihak lain, keputusan; Applied tidak boleh cancel langsung, melainkan counter-event baru.

**Source reuse:** FE `src/app/hr/workforce-core/unit-transfers/page.jsx`, `src/components/view/hr/workforce-core/unit-transfers-view.jsx`, `personnel-action-order-{create,list,detail}-view.jsx`; BE `HrdPersonnelActionOrder{Controller,Service}.cs` dan `WfpOrganizationAssignmentController.cs`.

**Perbaikan backend wajib:**
1. `POST /personnel-action-orders/{id}/cancel-request`: simpan permintaan dengan actor/time/reason dan state pending yang terbukti. `POST /{id}/cancel` hanya boleh menyetujui pembatalan Scheduled **yang benar-benar punya permintaan pending**, actor approval berbeda, belum efektif, dan izin benar. Tolak direct call tanpa permintaan, pemohon menyetujui sendiri, permintaan ganda, sudah efektif, atau SK `Applied`.
2. Worker harus **mengecualikan atau menahan** SK Scheduled dengan permintaan pembatalan yang masih terbuka; setelah keputusan tolak/cabut, lanjut sesuai kebijakan waktu efektif (jangan diam-diam menerapkan terlambat tanpa keputusan). Sertakan status konflik/escalation bagi HRD.
3. Backend direct `POST/PUT/PATCH/DELETE` organisasi/jabatan/atasan harus menolak bypass pada primary yang aktif maupun future-scheduled/ber-SK; jangan hanya larangan edit aktif dan membiarkan jalur create/patch mengubah primary secara tidak sah. Jika stand-alone assignment resmi ada, gunakan engine approval resminya.
4. Cek unit/departemen/jabatan tujuan sebagai referensi sah, credential requirement klinis ke domain pemilik (jangan mengesahkan kompetensi klinis otomatis); periksa periode kehadiran Closed pada create/submit/approve/apply sesuai keputusan, bukan sekadar saat approve.
5. Terapkan atomic close-old H-1/open-new tanggal efektif, satu primary per tanggal, histori SK, idempotent retry, permission and concurrency.

**Perbaikan frontend wajib:** Halaman `UnitTransfersView` tetap reuse list SK dengan kategori Mutation terkunci. Tampilkan `unit asal → unit tujuan`, tanggal efektif, nomor SK, status Draft/PendingApproval/Scheduled/Applied/CancellationPending/Cancelled, tombol kontekstual sesuai role. `Ajukan Pembatalan` dan `Setujui/Tolak Pembatalan` harus berbeda, masing-masing menuju endpoint yang tepat, tidak boleh memanggil `/cancel` langsung sebagai pemohon.

**Skema halaman target:**

```text
Perpindahan Unit
[Hero] Mutasi Unit Pegawai               [+ Ajukan Mutasi]
[SummaryGrid] Draf | Menunggu HRD | Dijadwalkan | Sudah Berlaku
[DataFilter] Pegawai | Unit asal/tujuan | Status | Tanggal efektif
[DataTable] SK | Pegawai | Asal -> Tujuan | Berlaku | Status | Aksi
[Detail SK] Sebelum/Sesudah | Scan SK | Pembuat/Penyetuju | Audit
[Cancel request] Alasan + status menunggu persetujuan, bukan direct cancel
```

**Acceptance:** tanpa request, `/cancel` mengembalikan 400/409/403 sesuai kasus; pemohon tidak bisa menyetujui; worker tidak apply ketika cancellation pending; keputusan maker-checker dan riwayat tetap persisten; personel tidak berpindah unit sebelum tanggal efektif.

### Submenu 3 - Rotasi Promosi

**Bisnis RS dan aktor.** Rotasi antarjabatan setara dan promosi ke jabatan lain harus memakai kategori transaksi yang benar serta satu SK; dampak grade/gaji bukan perubahan otomatis dari form SK dan harus melalui Payroll. Pengalihan manager/unit/jabatan terjadi atomik sesuai tanggal efektif.

**Source reuse:** FE `src/app/hr/workforce-core/rotation-promotions/page.jsx`, `rotation-promotions-view.jsx`, `personnel-action-order-{create,list,detail}-view.jsx`; BE `HrdPersonnelActionOrderService.cs` dan `HrdPersonnelActionOrderController.cs`.

**Perbaikan frontend P0:** `rotation-promotions-view.jsx` mengirim `?category=Rotation` atau `?category=Promotion`, tetapi `personnel-action-order-create-view.jsx` masih `actionCategory: "Mutation"` dan tidak membaca query. Terapkan pembacaan query sesuai pola Next.js App Router existing (memperhatikan Suspense/hydration), validasi **allowlist enum** (`Mutation`, `Rotation`, `Promotion`, dan kategori lain yang memang disetujui). Gunakan sebagai initial form value; jangan ter-overwrite setelah user mengubah form dengan sengaja. Invalid query harus aman (pilihan default yang jujur, bukan salah kategori diam-diam). Setelah submit, kategori yang tersimpan di database harus sama dengan yang dipilih.

**Perbaikan backend wajib:**
1. `ActionCategory` divalidasi server terhadap allowlist; jangan bergantung pada query FE. Kategori dan tipe event harus konsisten sebelum submit/approve/apply.
2. Penomoran SK harus aman pada **dua request paralel dan dua instance aplikasi**. Ganti pola `Max + 1` + `lock` proses lokal dengan nomor atomik di database/number-series existing (advisory lock/transaksi/unique+retry bounded sesuai governance). **Jangan** membuat migrasi otomatis; bila unik constraint baru diperlukan dan belum diberi izin, laporkan terblokir secara spesifik sambil menyelesaikan pekerjaan independen.
3. **Larangan langsung mengubah gaji** tetap berlaku: `TargetBaseSalary` tidak boleh disimpan atau dipaparkan pada DTO biasa. Jika gaji promosi berubah, hubungkan ke alur penetapan gaji berpersetujuan, bukan mutasi otomatis.
4. Periksa atomicity pergantian posisi, unit dan atasan, overlapping effectivity, double submit, rollback, unique primary as-of, serta penolakan backdate ketika attendance Closed.
5. Selaraskan alur pembatalan Scheduled dan reversal Applied dengan Submenu 2; hindari dua implementasi paralel.

**Skema halaman target:**

```text
Rotasi Promosi
[Hero] Rotasi & Promosi Jabatan                [+ Buat SK]
[Tab] Rotasi Jabatan | Promosi Jabatan | Semua Riwayat
[DataFilter] Kategori terkunci dari tab | Status | Tanggal | Pegawai
[DataTable] SK | Pegawai | Dari -> Ke | Tanggal efektif | Status
[Form SK] Jenis aksi dari tab URL | Data pegawai | Sebelum/Sesudah
          Scan tanda tangan | Alasan | Submit approval (bukan ubah gaji)
```

**Acceptance:** tombol Rotasi menyimpan `ActionCategory=Rotation`, tombol Promosi menyimpan `Promotion`, URL Mutation menyimpan `Mutation` (test klik UI, request network, response, dan reload DB). Nomor SK berbeda saat concurrent multi-user; tidak ada salary assignment baru hanya karena SK promosi.

### Submenu 4 - Uraian Tugas

**Bisnis RS dan aktor.** HRD/kepala unit perlu melihat **uraian tugas jabatan yang berlaku bagi pegawai pada tanggal tertentu**, termasuk riwayat versi. Sumber master ada pada Menu 05; Menu 07 **read-only berdasarkan penempatan**. Pegawai yang belum punya penempatan efektif harus mendapat empty state jujur, bukan menampilkan jabatan masa kini untuk tanggal lampau.

**Source reuse:** FE `src/app/hr/workforce-core/employee-job-descriptions/page.jsx`, `src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx`, `src/lib/services/hr/workforce-core-cross-employee-service.js`; BE `WorkforceCoreCrossEmployeeController.GetEffectiveJobDescription`, `WorkforceCoreCrossEmployeeQueryService.GetEmployeeEffectiveJobDescriptionAsync`, `WorkforceCoreCrossEmployeeDtos.EmployeeEffectiveJobDescriptionResponse`, `MasterData/Organization/DTOs/JobDescriptionDtos.cs`.

**Kontrak API nyata yang wajib diselaraskan:**
- Endpoint `GET /api/v1/corporate/human-resource/workforce-core/employees/{workforceProfileId}/effective-job-description?asOfDate=YYYY-MM-DD` dengan permission `WorkforceCoreCrossEmployee:Read`.
- Response backend object: `workforceProfileId`, `employeeName`, `profileCode`, `positionId`, `positionName`, `departmentId`, `departmentName`, `organizationUnitId`, `organizationUnitName`, `asOfDate`, **`jobDescription` object nested** dengan `id`, `versionNumber`, `effectiveStartDate`, `effectiveEndDate`, `purpose`, `items[{id,sectionType,sequence,content}]`, dan **`versionHistory[{id,versionNumber,effectiveStartDate,effectiveEndDate,status,...}]`**.
- Source FE yang terbaca masih menunggu nama **flat** `effectivePositionId`, `effectivePositionName`, `effectiveDepartmentName`, `jobDescriptionId`, `purpose`, `duties[{dutyOrder,dutyDescription,performanceStandard}]`, dan `versionHistory[*].purpose/isActive` yang **tidak dijamin tersedia**.

**Perbaikan backend wajib:**
1. Query `asOfDate` harus mencari riwayat penempatan yang efektif pada tanggal acuan (`EffectiveStartDate <= date` dan `EffectiveEndDate >= date` atau null, `!IsDelete`) tanpa membuang histori hanya karena `IsActive=false` saat ini. Validasi apakah entitas memiliki status lain (cancelled/void) dan exclude yang tidak sah. Pilih deterministik bila data historis overlapping, beri error/integrity flag jika ambiguous sesuai kontrak.
2. **Jangan fallback ke `profile.PrimaryPositionId` saat tidak ada penempatan sah pada tanggal yang diminta**; mirror adalah data hari ini, bukan sumber historis. Kembalikan `positionId=null`, `jobDescription=null`, dan empty state yang benar jika memang belum ada.
3. Untuk versi master uraian tugas, tampilkan yang efektif pada `asOfDate` dan history yang benar; jangan mengarang `purpose`, `isActive` pada history jika DTO tidak menyediakannya. Jika perlu memperkaya DTO, verifikasi domain ownership Menu 05, jangan mengubah master persisted sembarangan.
4. Uji tanggal lampau saat jabatan lama telah ditutup (`IsActive=false`), tanggal sebelum pegawai memiliki jabatan, hari transisi dan future date.

**Perbaikan frontend wajib:**
1. Buat **adapter/selector kontrak yang eksplisit** di service FE atau view scoped, bukan cek field yang tidak ada. Contoh mapping berbasis DTO nyata: `effectivePositionId <- positionId`, `employeeCode <- profileCode`, `jobDescriptionId <- jobDescription?.id`, `purpose <- jobDescription?.purpose`, `versionNumber <- jobDescription?.versionNumber`, `duties <- jobDescription?.items` lalu render `sectionType`, `sequence`, `content` tanpa mengarang `performanceStandard`. **Lebih baik** gunakan nama DTO asli langsung bila konsisten dengan UI; pilih satu metode dan buat unit test adapter.
2. Halaman harus dapat membedakan: tidak ada pegawai dipilih; API loading; API error/403; belum ada effective position; ada position tetapi belum ada master uraian tugas; ada master dan butir; riwayat kosong. Semua status berasal dari data server.
3. Riwayat versi gunakan `status` asli dan kolom yang memang ada dalam `JobDescriptionHistoryResponse`; jangan membaca `ver.purpose` dan `ver.isActive` seolah fakta jika belum tersedia. Detail versi boleh dimuat melalui endpoint resmi Menu 05 bila dibutuhkan, dengan permission sesuai.

**Skema halaman target:**

```text
Uraian Tugas
[Hero] Uraian Tugas Pegawai              [Buka Master Menu 05]
[Filter] Pilih Pegawai (API real) | Tanggal Acuan
[Context] Unit + Departemen + Jabatan berlaku pada tanggal acuan
[Tab] Uraian Tugas Berlaku | Riwayat Versi
[Detail] Tujuan jabatan, kelompok butir (Tugas/Tanggung Jawab/Wewenang/Relasi)
[Empty/Error] Penempatan belum efektif / Master belum ada / Tidak berizin
```

**Acceptance:** jabatan efektif terbaca dari API dengan key benar; tanggal lampau tidak menampilkan jabatan sekarang bila historinya kosong; versi dan `items` tampil; 403 berbeda dengan empty; tombol ke master menavigasi route nyata yang benar.

### Submenu 5 - Atasan Langsung

**Bisnis RS dan aktor.** Rantai atasan menentukan pengawasan staf dan sebagian approval SDM; kesalahan siklus dapat membuat permintaan persetujuan tak pernah selesai. Perubahan atasan akibat SK peristiwa direkam bersama SK; perubahan atasan mandiri mengikuti workflow khusus `HRD-DEC-036` (jangan dipaksakan menjadi `Mutation` tanpa dasar).

**Source reuse:** FE `src/app/hr/workforce-core/employee-managers/page.jsx`, `src/components/view/hr/workforce-core/employee-managers-view.jsx` dan `personnel-action-order-create-view.jsx`; BE `WorkforceCoreCrossEmployeeController.GetManagerAssignments`, `WfpManagerAssignmentController.ValidateRequestAsync/WouldCreateCycleAsync`, `HrdPersonnelActionOrderService.ValidatePlacementCombinationAsync`.

**Perbaikan backend wajib:**
1. Terapkan deteksi **cycle** untuk jalur SK `HrdPersonnelActionOrderService` saat membuat, mengajukan, menyetujui, dan menerapkan SK; bukan hanya self-manager. Reuse satu validator domain yang terverifikasi, uji tanggal efektif dan perubahan lebih dari satu langkah pada rantai. Hindari exception yang hanya terjadi setelah data setengah tersimpan.
2. Status atasan utama pada tanggal tertentu berasal dari assignment efektif, bukan sekadar `IsActive` hari ini. Toleransi acting/secondary tanpa mengubah primary secara tidak sah. Cegah dua atasan primary bersamaan dan self-reference.
3. Pisahkan **SK event** (`HRD-DEC-048`) dari **stand-alone manager assignment** (`HRD-DEC-036`): bila backend belum punya workflow standalone resmi yang aman, implementasikan melalui mekanisme approval existing yang benar sesuai canonical; jangan membuat kategori SK baru secara asumtif, jangan bypass approval. Jika diperlukan keputusan owner baru yang benar-benar material, tahan hanya jalur itu, lanjutkan perbaikan lain.
4. Periksa `WfpManagerAssignmentController` direct CRUD agar tidak ada bypass terhadap primary yang sudah terdaftar atau scheduled; test role server-side, bukan label UI.

**Perbaikan frontend wajib:**
1. Tombol `+ Ajukan Perubahan Atasan` **jangan** langsung membuka `/personnel-actions/create` yang default `Mutation` tanpa konteks. Arahkan ke workflow manager-only yang sah atau form SK berparameter jelas sesuai tipe event yang disetujui dan benar-benar ditangani backend. Jangan mengklaim sukses jika endpoint belum tersedia.
2. Daftar atasan dan visual diagram memakai response API real. Tampilkan primary/secondary, tanggal efektif, dapat-approve, unit dan penanda pending; kosong dan 403 terpisah. Jangan mengarang hierarchy pada frontend dengan data yang belum dibaca.
3. Gunakan interface/permission yang tepat pada tiap aksi, disabled/loading selama submit dan reset states setelah berhasil.

**Skema halaman target:**

```text
Atasan Langsung
[Hero] Atasan Langsung Pegawai         [+ Ajukan Perubahan Atasan]
[SummaryGrid] Relasi aktif | Atasan utama | Relasi tambahan | Perlu keputusan
[DataFilter] Pegawai | Atasan | Unit | Tipe | As-of date
[DataTable] Pegawai | Atasan | Jenis | Berlaku sejak | Jalur approval | Status
[Detail] Hierarki atasan -> pegawai, effective history dan sumber SK/workflow
[Form] Atasan baru, alasan, tanggal efektif, bukti, approval berbeda aktor
```

**Acceptance:** hubungan A→B→C tidak boleh diubah menjadi C→A lewat SK; direct endpoint juga ditolak; perubahan manager-only tidak dicatat sebagai Mutation palsu; data tetap konsisten untuk tanggal masa depan dan masa lalu.

### 2.4 Integrasi lintas Submenu, tanpa scope creep

- **Menu 04 (Data Induk):** `PrimaryPositionId`/`PrimaryDepartmentId` pada `MstWorkforceProfile` merupakan **mirror**, bukan sumber kebenaran historis; tidak boleh berubah sebelum tanggal efektif.
- **Menu 05 (Struktur Organisasi & Jabatan):** master `MstOrganizationUnit`, `MstDepartment`, `MstPosition`, master `JobDescription` dan version history **direuse**; jangan membuat master jabatan terpisah untuk Menu 07.
- **Menu 06 (Administrasi Kepegawaian):** SK, bukti scan, workflow dan history **direuse**; nomor dan approval tidak boleh ganda.
- **Menu 08 (Kontrak & Masa Kerja):** `RecordSignedAsync` memanggil `WfpNewHireAssignmentService`; perbaikan di service itu harus diregression-test agar kontrak pertama, penempatan awal dan payroll tidak terputus atau menimbulkan data ganda.
- **Payroll:** `WfpSalaryAssignment` tidak boleh dibuat/diubah dari SK mutasi, promosi atau batch penempatan; source nominal hanya dapat dibaca dengan permission khusus, dan tidak boleh ada amount leak pada response umum.
- **Scheduling/Kehadiran:** tanggal efektif perpindahan tidak boleh menghasilkan jadwal atau identitas unit baru sebelum sah; backdate ditolak jika periode attendance Closed; jangan mengedit jadwal klinis otomatis di luar domain owner.
- **Kredensial klinis:** perubahan unit ke IGD/ICU/OK tidak berarti otorisasi klinis otomatis. Gunakan status resmi sesuai batas domain dan eskalasi jika validasi kredensial belum tersedia.

### 2.5 Penugasan implementasi — pertahankan 9 task backend FIN yang sudah ada

Semua task di bawah **harus dijalankan**, bukan sekadar checklist. Pekerjaan yang sudah benar harus dipertahankan; fokus pada gap per task.

#### `FIN-HRD-07-BE-001` — Maker-checker dan guard scan SK (P0)

**File utama:** `HrdPersonnelActionOrderService.cs`, `HrdPersonnelActionOrderController.cs`. Pastikan submit menolak SK tanpa signed scan (dokumen benar-benar tersedia dan aman), approve menolak creator maupun submitter, permission `Approve` di backend, check identity delegated actor dan concurrency. Reuse validasi, bukan cukup hide button. **Test:** creator/submitter mendapat 400/403; user lain berpermission berhasil; scan kosong/gagal tidak berubah status; dua approval paralel hanya sekali.

#### `FIN-HRD-07-BE-002` — State machine pembatalan SK, worker tidak mendahului approval (P0)

**File utama:** `HrdPersonnelActionOrderService.cs`, `HrdPersonnelActionOrderController.cs`, model/DTO existing dan worker. Perbaiki `RequestCancelOrderAsync`/`CancelOrderAsync`: `Scheduled` hanya dapat dibatalkan setelah request sah dan persetujuan orang berbeda; worker wajib menunda saat cancellation pending. `Applied` tetap immutable; reversal via counter-event baru yang punya referensi SK asal. Penolakan/cabut cancellation harus benar-benar terwakili bila canonical mengizinkan. Gunakan transaction + conflict/RowVersion. **Test:** direct `/cancel` tanpa request gagal; request oleh A approval oleh A gagal; A→B berhasil; worker pending tidak apply; applied tidak terhapus.

#### `FIN-HRD-07-BE-003` — Privacy gaji dan nomor SK atomik lintas instance (P0)

**File utama:** `HrdPersonnelActionOrderService.cs`, DTO SK dan response batch gaji terkait. Hilangkan `Max+1` yang dikunci hanya oleh `private static lock`, gunakan number series resmi/advisory lock DB yang aman dan unique constraint jika telah ada; transaksi dan bounded retry. Jangan membuat GUID random sebagai nomor. TargetBaseSalary tetap null pada input/output SK; batch Read response tidak mengungkap BaseSalary. Jangan membuat schema tanpa izin. **Test:** 20 create paralel/two app instances menghasilkan nomor unik; gaji tidak bocor pada HTTP response user non-payroll; promosi tidak menciptakan salary assignment.

#### `FIN-HRD-07-BE-004` — Tutup bypass direct CRUD serta cycle validator (P0)

**File utama:** `WfpOrganizationAssignmentController.cs`, `WfpPositionAssignmentController.cs`, `WfpManagerAssignmentController.cs`, validator shared scoped. Lanjutkan guard yang sudah dibuat untuk update/status/primary/delete dan perluas pada create primary pertama/masa depan/overlap bila memerlukan approval. Pastikan tidak ada jalur direct CRUD yang menyalahi `HRD-DEC-031/036/047/048/055`; direct manager validator dan SK memakai aturan cycle yang konsisten. **Test:** PUT/PATCH/DELETE dan create-bypass blocked, termasuk API langsung Postman/curl, role dengan permission Create biasa, dan future scheduled.

#### `FIN-HRD-07-BE-005` — Scheduled apply worker, timezone, mirror dan idempotensi (P0)

**File utama:** `PersonnelActionOrderScheduledApplyWorker.cs`, `HrdPersonnelActionOrderService.cs`, `Program.cs`. Pertahankan worker terdaftar, tapi gunakan tanggal bisnis hospital site dari konfigurasi dan clock yang testable; hindari `UtcNow.Date` untuk penentuan hari efektif. Filter cancellation pending. Lock/claim per SK jika multi-instance, pastikan exactly-once business effect saat retry; transaction rollback tidak bocor ke next SK; log `orderId`, status dan error tanpa data sensitif. Periode attendance closed = stop + escalated status jujur, bukan `Notes` berulang tanpa lifecycle. Mirror dari assignment efektif saja. **Test:** WIB 00:01 vs UTC hari sebelumnya; double tick; concurrent workers; error satu SK tidak menggagalkan semua; future-dated mirror tetap lama.

#### `FIN-HRD-07-BE-006` — Batch approval integrity, validation permission, efektif-date, integrasi onboarding (P0)

**File utama:** `HrdInitialPlacementBatchService.cs`, `HrdInitialPlacementBatchController.cs`, `WfpNewHireAssignmentService.cs`, DTO/model yang terbukti diperlukan. `ValidateBatchByIdAsync` hanya menulis pada draft/validated sah, tidak downgrade pending/approved; ubah permission POST validate menjadi write. Cegah dua batch approved/executed untuk pegawai sama; transaksikan submit/approve/execute dengan concurrency. Hapus leak BaseSalary, cegah duplicate primary, dan sinkron mirror saat efektif. Mapping `DepartmentId` harus FK departemen benar atau fail closed. Test integrasi dengan kontrak pertama sesuai Menu 08, no duplicate salary assignment. **Test:** direct POST validate dengan Read-only 403; downgrade disallow; two batch concurrent; 1 invalid row => 0 apply; future-date mirror; DB FK valid.

#### `FIN-HRD-07-BE-007` — Uraian tugas per as-of date yang benar (P0)

**File utama:** `WorkforceCoreCrossEmployeeQueryService.cs` dan service JobDescription Menu 05 yang direuse read-only. Gunakan data histori `WfpPositionAssignment` efektif pada target date tanpa fallback mirror masa kini. Jangan hilangkan penempatan lama valid karena `IsActive=false`; bedakan cancelled/deleted; gunakan master JD efektif versi sah. Return explicit null/empty, bukan posisi palsu. **Test:** pergantian jabatan 2025/2026, tanggal sebelum penempatan, tanggal transisi, versi JD berubah, arsip history.

#### `FIN-HRD-07-BE-008` — Kontrak response DTO, rantai atasan, perizinan (P0)

**File utama:** `WorkforceCoreCrossEmployeeController.cs`, `WorkforceCoreCrossEmployeeDtos.cs`, `WorkforceCoreCrossEmployeeQueryService.cs`, `HrdPersonnelActionOrderService.cs`, `WfpManagerAssignmentController.cs`. Kunci response `EmployeeEffectiveJobDescriptionResponse` dan nested `JobDescriptionDetailResponse` untuk konsumen FE; tambahkan field hanya bila benar-benar dibutuhkan dan sesuai governance. Pastikan cycle via SK tertolak, role `WorkforceCoreCrossEmployee:Read` dan `WfpManagerAssignment:ReadAll` benar. Sediakan entrypoint manager-only melalui workflow valid tanpa menyamakan dengan Mutation. **Test:** kontrak JSON FE/BE, cycle 3 node, as-of manager, 403, invalid GUID, maker checker.

#### `FIN-HRD-07-BE-009` — Regression/integration tests dan bukti QA BE (P0)

**File:** test scoped + laporan task jika mode diizinkan. Jalankan test otomatis atas state machine, privilege escalation, concurrency, transaction, client errors, data seeding test yang nonproduksi dan tidak menyentuh DB operasional. Jalankan build/compile hanya bila diizinkan oleh aturan dan instruksi pengguna saat eksekusi. Bila sebelumnya ada mandat `SKIP dotnet build`, periksa apakah berlaku untuk sesi ini; **jangan klaim build PASS** bila tidak dijalankan: laporkan `NOT RUN (user mandate)`; hasil build lama tidak menutup perubahan source baru. Sertakan pemeriksaan response HTTP dan query persisted melalui lingkungan test yang sah. Target semua P0 minimal kode+test tertutup; jika blocked, status `BLOCKED` eksplisit.

### 2.6 Penugasan implementasi — pertahankan 7 task frontend FIN yang sudah ada

**Frontend dikerjakan setelah kontrak backend stabil**, bukan sebagai tampilan dummy untuk menutupi endpoint yang belum berfungsi.

#### `FIN-HRD-07-FE-001` — Sidebar, routes, izin (P1)

Recheck `src/utils/menu-sidebar/menu-items.jsx` untuk tepat lima Submenu 07, tidak menduplikasi Menu 05/06; route terdaftar, active navigation, requiredPermission sesuai backend. Pertahankan route legacy yang diakses Menu 06. Uji user HRD yang berizin dan user tanpa izin.

#### `FIN-HRD-07-FE-002` — Penempatan Awal runtime error dan workflow (P0)

Perbaiki `isPending` undefined pada `initial-placement-batch-view.jsx` dan seluruh state/action guard. Validasi hanya untuk Draft/Validated yang sah; submit, approve, execute berdasarkan state dan role backend; error 403/409 terlihat jelas. Jangan memaksa reload penuh kalau refetch API sudah ada; pastikan state persisted setelah refresh. Hilangkan nominal gaji dari UI biasa. Test render dengan batch aktif, snapshot invalid/pending/approved/executed dan mutation role separation.

#### `FIN-HRD-07-FE-003` — Perpindahan Unit dan pembatalan berpersetujuan (P0)

Pertahankan `UnitTransfersView` yang mengunci kategori `Mutation` pada list. Pastikan tombol create benar mengirim `Mutation`; detail menyediakan request-cancel dan action approval terpisah, bukan self-approval. Tampilkan blokir Closed attendance/clinical requirements sebagai respons server. Uji Scheduled/cancellation-pending/Applied dan return/403/409.

#### `FIN-HRD-07-FE-004` — Rotasi Promosi kategori URL dan SK form (P0)

Di `personnel-action-order-create-view.jsx` baca allowlisted query `category`, isi `actionCategory` awal sesuai tab, dan jaga pilihan user. Implementasi mengikuti pola Next.js App Router existing (`searchParams` dan boundary client/server yang benar), tanpa hydration warning. Uji kategori `Rotation`, `Promotion`, `Mutation`, invalid, klik kembali dari detail; network POST dan database harus benar. Tidak ada form gaji langsung.

#### `FIN-HRD-07-FE-005` — Uraian Tugas contract adapter dan empty states (P0)

Perbaiki komponen/service dengan DTO nyata: `positionId/positionName/profileCode`, nested `jobDescription` beserta `items`, dan `versionHistory.status`. Jangan gunakan `effectivePositionId`, `jobDescriptionId`, `duties` atau `purpose` flat tanpa adapter eksplisit yang dites. `items` dikelompokkan berdasarkan `sectionType` dan urutan `sequence`; jangan buat-buat standar kinerja. Tidak ada effective-position saat as-of date harus menjadi empty state, bukan kesalahan palsu. Akses ditolak != data kosong. Reuse Master Uraian Tugas Menu 05 via route benar.

#### `FIN-HRD-07-FE-006` — Atasan Langsung action benar dan diagram (P0)

Jangan route ke create SK default `Mutation` untuk aksi perubahan atasan mandiri. Sambungkan ke workflow manager-only existing/yang diimplementasi sah atau contextual SK sesuai keputusan; kategori, permission, alasan dan effective date jelas. Hubungan A→B→C dengan upaya C→A harus ditolak backend dan ditampilkan error asli. Summary dan diagram tetap memakai API, tidak membuat dummy manager tree.

#### `FIN-HRD-07-FE-007` — Base components, CSS, responsive, unit/E2E dan QA FE (P0)

Gunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `InformationAlert`, `BaseButton`, `BaseGroupedEditorView/Field`, `StatusBadge`, `AccessDeniedGate`, `ConfirmModal`, `ToastStack` dan token existing sesuai aturan; jangan tulis base component baru tanpa gate. Periksa aksesibilitas label, keyboard/focus, mobile/tablet. Jalankan `check-css-usage.mjs` dan lint/test/build menurut izin, **sertakan output command**, jangan hanya menyebut sudah PASS. Tambahkan test untuk ReferenceError, query category, adapter nested DTO, feedback error API, permission UI serta reload. UAT headed browser bila akses tersedia.

---

## 3. Context

### 3.1 Bukti source dan lokasi laporan (dibaca pada snapshot audit)

| Area | Source spesifik yang telah diperiksa | Status saat audit |
|---|---|---|
| Laporan Agent | `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md` | Klaim 100%, 34 UAT `READY`, bukan bukti PASS |
| BE SK | `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdPersonnelActionOrderService.cs`, `Controllers/HrdPersonnelActionOrderController.cs` | Source tersedia; cancellation/numbering/cycle/date repair |
| BE batch | `.../Services/HrdInitialPlacementBatchService.cs`, `.../Controllers/HrdInitialPlacementBatchController.cs` | Source tersedia; permission/state mutation/duplicate/mirror repair |
| BE per pegawai | `.../Services/WorkforceCoreCrossEmployeeQueryService.cs`, `.../DTOs/WorkforceCoreCrossEmployeeDtos.cs`, `.../Controllers/WorkforceCoreCrossEmployeeController.cs` | Query as-of dan nested DTO perlu FE-BE reconciliation |
| BE assignment | `.../Controllers/WfpOrganizationAssignmentController.cs`, `WfpPositionAssignmentController.cs`, `WfpManagerAssignmentController.cs` | Guard direct CRUD sebagian ada; cek bypass dan cycle SK |
| BE onboarding | `.../Services/WfpNewHireAssignmentService.cs`; `LifecycleManagement/Services/HrdEmploymentContractService.cs` | Contract-signing path perlu regression, DepartmentId fallback |
| BE scheduler | `.../Workers/PersonnelActionOrderScheduledApplyWorker.cs`, `Program.cs` | Worker terdaftar; timezone/pending-cancel/concurrency repair |
| FE batch | `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` | **P0 ReferenceError** `isPending` |
| FE SK form | `src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx` | **P0 URL query category diabaikan** |
| FE JD | `src/components/view/hr/workforce-core/employee-job-descriptions-view.jsx`, `src/lib/services/hr/workforce-core-cross-employee-service.js` | **P0 JSON response mismatch** |
| FE managers | `src/components/view/hr/workforce-core/employee-managers-view.jsx` | **P0 CTA membuka form Mutation tanpa konteks** |
| FE routes | `src/app/hr/workforce-core/{initial-placements,unit-transfers,rotation-promotions,employee-job-descriptions,employee-managers}/page.jsx` | Kelima route ADA, tidak perlu remake |
| FE sidebar | `src/utils/menu-sidebar/menu-items.jsx` | Lima label Menu 07 ADA, tidak perlu duplicate |

**Penting:** Laporan Agent mungkin menyebut path API yang keliru. Verifikasi dari controller attributes, bukan menyalin tabel laporan. FE menggunakan `InstanceAxios` dengan base URL `/v1/...` sedangkan route backend umumnya berawalan `api/v1/...` melalui konfigurasi Axios existing. Jangan menggandakan `/api` pada FE bila baseURL sudah mengandungnya.

### 3.2 Kontrak API aktual — verifikasi kembali signature sebelum mengubah

**[Tags(Corporate / Human Resource / Workforce Core / Initial Placement Batch)]**

| Method | Endpoint backend | Fungsi/aksi izin |
|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/template` | Template; `HrdInitialPlacementBatch:Read` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/validate` | Validasi baris input; kontrak permission perlu ditinjau bila tidak mutasi DB |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches` | Create draft; `Create` |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/initial-batches` | List; `Read` |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}` | Detail; `Read`; **tidak boleh membocorkan BaseSalary** |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/validate` | **Mutasi status/hasil validasi**; wajib permission write dan state guard |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/submit` | Ajukan; `Update` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/approve` | Setujui; `Approve` + actor berbeda |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute` | Terapkan; `Approve` (atau action execute canonical bila sudah disetujui), perlu transaksi + idempotensi |

**[Tags(Corporate / Human Resource / Workforce Core / Personnel Action Order)]**

| Method | Endpoint backend | Fungsi/aksi izin |
|---|---|---|
| `GET/POST` | `/api/v1/corporate/human-resource/personnel-action-orders` | List / create SK; `Read` / `Create` |
| `GET` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}` | Detail; `Read` |
| `PUT` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}` | Edit draft; `Update` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/attachments` | Upload signed scan; `Update` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/submit` | Submit; `Update` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/approve` | Approve; `Approve`, anti self-approval |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/reject` | Reject; `Reject` |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/cancel-request` | Ajukan pembatalan; `Cancel`, wajib actor+reason |
| `POST` | `/api/v1/corporate/human-resource/personnel-action-orders/{id}/cancel` | Penyelesaian pembatalan; `Cancel`, **guard pending maker-checker** |

**[Tags(Corporate / Human Resource / Workforce Core / Cross Employee)]**

| Method | Endpoint backend | Permission |
|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/workforce-core/manager-assignments` | `WfpManagerAssignment:ReadAll` |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/employees/{workforceProfileId}/effective-job-description?asOfDate=YYYY-MM-DD` | `WorkforceCoreCrossEmployee:Read` |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/salary-assignments/{id}/amount` | `WfpSalaryAssignment:ViewAmount` — sumber pola izin nominal untuk audit kebocoran |

**Jangan mengarang endpoint manager-only.** Jika jalur stand-alone belum ada, tetapkan kontrak dengan canonical/reuse yang sah di backend-first, barulah FE memakainya. Bila memerlukan kontrak bisnis baru di luar keputusan approved, blok hanya sub-action itu.

### 3.3 Source-of-truth, data dan identitas

- `MstWorkforceProfile` adalah profil pegawai dan mirror untuk kondisi terkini. Riwayat efektif asli: `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`; peristiwa SK: `HrdPersonnelActionOrder`; batch: `HrdInitialPlacementBatch` dan rows; sejarah: `WfpEmploymentHistory`.
- Simpan `effectiveDate`, actor maker/checker, `RowVersion`/concurrency token yang sudah ada, attachment metadata, event audit dan referensi SK terkait. **Tidak** boleh membuat state penting hanya di `useState` frontend atau catatan `Notes` tanpa state persistence.
- Semua daftar/filter/opsi pegawai, unit, posisi, atasan, master uraian tugas dan riwayat diambil dari layanan nyata. Jika gagal, tampilkan error yang benar, jangan fallback data dummy.
- Hari bisnis RS bersifat berdasarkan zona waktu hospital site, bukan mengasumsikan `UTC.Date` setara tanggal lokal pada jam 00:00 WIB.
- Perubahan UI tidak dapat menggantikan permission server. Cek negative tests dengan HTTP langsung, bukan hanya tombol tidak muncul.

---

## 4. Reasoning / Execution Workflow

> Bagian ini adalah **workflow eksekusi operasional yang diaudit**, bukan permintaan mengungkap chain-of-thought privat. Keputusan bisnis sudah terkunci; Agent wajib **mulai implementasi** setelah preflight singkat dan izin scope sah.

### 4.1 Preflight singkat dan aman

1. Baca `AGENTS.md`, `CLAUDE.md` repo FE/BE, Engineering Skills `main` dan aturan sesuai task; verifikasi registered Skills benar-benar bisa dipanggil.
2. Periksa `git branch --show-current`, `git rev-parse HEAD`, `git status --short`, `git rev-parse --abbrev-ref --symbolic-full-name '@{upstream}'` di kedua working tree menggunakan **perintah read-only**. Jika branch/upstream salah, jangan checkout otomatis; laporkan blocker dan tunggu koreksi pihak berwenang.
3. Bandingkan source lokal terhadap SHA snapshot di header secara read-only; **jangan menimpa uncommitted changes** dan jangan membatalkan kerja developer lain. Jika source lebih baru, rekonsiliasi temuan audit yang telah benar-benar diperbaiki, lanjutkan sisanya.
4. Periksa canonical HRD-DEC dan endpoint/DTO dalam source lokal; ini verifikasi singkat, **bukan** sesi requirement planning ulang.
5. Bila canonical perlu update yang sudah disetujui, laksanakan hanya pada `MODULE BLUEPRINT MODE` dengan izin dokumen yang sah; implementasi independen tidak boleh tertunda.
6. Pastikan akses test DB nonproduksi terotorisasi dan tidak memuat data rahasia; jangan menjalankan migration/seed tanpa izin eksplisit.

### 4.2 Urutan wajib implementasi BACKEND → FRONTEND → integration

**Backend fase 1** (seluruh 9 FIN BE): prioritaskan **`BE-006` state batch/permission**, **`BE-002` cancellation**, **`BE-003` numbering/privacy**, **`BE-008` cycle/DTO**, **`BE-005` worker/timezone**, lalu `BE-004`, `BE-007`, `BE-001`, `BE-009` sesuai dependensi. Nomor task tetap asli, urutan pengerjaan dapat mengikuti risiko P0.

Untuk setiap task:
- Baca source paling dekat dan kontrak current; ubah service/controller/DTO/entity yang benar-benar diperlukan.
- Terapkan validasi **di backend**, permission+actor, state transition yang sah, transaction/idempotency, audit dan error respons yang dapat dipakai FE. Buktikan dengan unit/integration test scoped.
- Bila perlu constraint/model/migration yang belum diizinkan, jangan bypass; catat blocker yang tepat dan selesaikan hal independen.
- Kunci API/response setelah BE diperbaiki; dokumentasikan contract drift yang perlu FE konsumsi.

**Frontend fase 2** (seluruh 7 FIN FE): kerjakan FE-002 error render lebih dahulu, FE-004 category, FE-005 adapter, FE-006 manager CTA, FE-003 cancellation UI, FE-001 sidebar consistency, dan FE-007 QA. Pertahankan App Router dan base-components existing; jangan membuat mock API atau menulis ulang seluruh halaman.

**Integrasi fase 3**: jalankan FE terhadap API backend sah, uji 2 role terpisah, beberapa status, HTTP error, reload, as-of history, scheduled date, double submit dan permission. Jika CORS/backend env belum tersedia, jangan menyebut UAT PASS; `NOT RUN` disertai blocker, dan selesaikan unit/contract tests yang bisa dijalankan.

### 4.3 Kontrak UI — base components, keamanan dan a11y

- Gunakan base UI existing (`Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `InformationAlert`, `BaseButton`, `BaseGroupedEditorView`, `BaseGroupedEditorField`, `BaseDetailView` bila sesuai, `StatusBadge`, `ConfirmModal`, `ToastStack`, `AccessDeniedGate`) dan CSS modules/tokens; jangan membuat base component kembar.
- `Loading`, `error`, `403`, `404`, `409/conflict`, `empty`, `no permissions`, `success after server persistence` harus dibedakan. Pilihan tanggal tidak boleh beda timezone secara tersembunyi.
- Setiap tombol mutasi harus disabled saat in-flight, server memastikan idempotency. Modal konfirmasi menunjukkan dampak permanen dan actor; gunakan keyboard focus, aria label, contrast, responsive tablet/mobile.
- Query/route parameter tidak boleh mengganti permission backend. Escape/sanitize output backend sesuai library existing; jangan mengirim atau mencatat nominal salary ke console/browser log user biasa.
- Jangan menyamarkan API error dengan placeholder sukses, jangan menyembunyikan kegagalan integrasi dengan label “data belum tersedia”.

### 4.4 Rencana pengujian UAT utama — 34 skenario yang WAJIB dilaksanakan jika environment tersedia

**Untuk setiap case catat:** `ID`, prasyarat/akun/role, aksi klik atau HTTP, expected, observed, bukti layar/network/DB test, `PASS|FAIL|BLOCKED|NOT RUN`, dan build/commit SHA. Gunakan Chrome headed dan klik nyata sesuai `rules/testing/`, bukan klaim hanya karena source ada.

| ID | Area | Skenario dan expected |
|---|---|---|
| `HRD07-UAT-01` | Penempatan Awal | Daftar/template memuat pegawai riil sesuai role dan status; tanpa dummy fallback |
| `HRD07-UAT-02` | Penempatan Awal | Draft valid → Validated, 1 baris invalid mencegah submit |
| `HRD07-UAT-03` | Penempatan Awal | HR A submit → PendingApproval; HR A tidak bisa approve batch miliknya |
| `HRD07-UAT-04` | Penempatan Awal | HR B approve → Approved, audit actor/time; reload tetap sama |
| `HRD07-UAT-05` | Penempatan Awal | Execute sukses → Executed; satu pegawai tidak membuat dua primary |
| `HRD07-UAT-06` | Penempatan Awal | Seluruh perubahan rollback bila satu row gagal; salary nonpayroll tidak terlihat |
| `HRD07-UAT-07` | Perpindahan Unit | Draft Mutation memuat data pegawai dan asal/tujuan sesuai tanggal efektif |
| `HRD07-UAT-08` | Perpindahan Unit | Scan tidak ada → submit ditolak backend; scan valid bisa diajukan |
| `HRD07-UAT-09` | Perpindahan Unit | Maker tidak dapat approve, approver sah menandai Scheduled future date |
| `HRD07-UAT-10` | Perpindahan Unit | Scheduled tidak mengubah unit saat ini dan tidak memberi akses prematur |
| `HRD07-UAT-11` | Perpindahan Unit | Backdate ke attendance Closed ditolak dan tidak ada mutasi DB |
| `HRD07-UAT-12` | Perpindahan Unit | Direct CRUD bypass organisasi/job utama ditolak tanpa event resmi |
| `HRD07-UAT-13` | Rotasi Promosi | Buka Rotasi dan `create?category=Rotation` → category form/simpan benar |
| `HRD07-UAT-14` | Rotasi Promosi | Buka Promosi dan `create?category=Promotion` → category form/simpan benar |
| `HRD07-UAT-15` | Rotasi Promosi | Pilih jabatan/departemen mismatch ditolak server dengan error spesifik |
| `HRD07-UAT-16` | Rotasi Promosi | Setelah approve, assignment posisi + organisasi + manager berubah atomik |
| `HRD07-UAT-17` | Rotasi Promosi | Gaji tidak berubah/terbuat dari SK; nominal tidak bocor ke response biasa |
| `HRD07-UAT-18` | Rotasi Promosi | Cancel SK Applied ditolak; counter-event baru tidak menghapus SK lama |
| `HRD07-UAT-19` | Rotasi Promosi | Concurrent submit/approve tidak menghasilkan nomor SK ganda / history duplikat |
| `HRD07-UAT-20` | Uraian Tugas | Pegawai dan asOfDate terpilih → jabatan efektif yang benar muncul |
| `HRD07-UAT-21` | Uraian Tugas | Version JD terpilih sesuai asOfDate; items tampil dari nested DTO |
| `HRD07-UAT-22` | Uraian Tugas | Tanggal sebelum penempatan → empty jujur, bukan jabatan saat ini |
| `HRD07-UAT-23` | Uraian Tugas | Riwayat versi sesuai DTO, Master Menu 05 tetap read/write owner |
| `HRD07-UAT-24` | Atasan Langsung | Daftar manager menampilkan pegawai, primary/secondary, unit/tanggal efektif |
| `HRD07-UAT-25` | Atasan Langsung | Atasan=self ditolak oleh backend |
| `HRD07-UAT-26` | Atasan Langsung | Cycle A→B→C→A ditolak melalui SK dan direct API |
| `HRD07-UAT-27` | Atasan Langsung | Manager-only change menggunakan workflow sah, bukan Mutation palsu |
| `HRD07-UAT-28` | Atasan Langsung | Data manager tetap sesuai histori as-of setelah reload |
| `HRD07-UAT-29` | Lintas Modul | Worker Scheduled apply tepat tanggal bisnis setempat |
| `HRD07-UAT-30` | Lintas Modul | Pembatalan pending menahan worker, maker-checker di API berlaku |
| `HRD07-UAT-31` | Lintas Modul | API unauthorized role mendapat 403; tidak ada write diam-diam |
| `HRD07-UAT-32` | Lintas Modul | Respons API gagal tidak berubah menjadi UI success/empty palsu |
| `HRD07-UAT-33` | Lintas Modul | Responsive desktop/tablet/mobile, navigasi/keyboard/focus dan CSS tokens |
| `HRD07-UAT-34` | Lintas Modul | Reload browser dan regression onboarding/Payroll/roster tidak merusak data |

### 4.5 Regression test khusus temuan audit — wajib selain 34 UAT

| ID | Tingkat | Input/aksi test | Expected backend/frontend |
|---|---|---|---|
| `HRD07-REG-01` | P0 | Render Penempatan Awal dengan batch yang belum executed | Tidak ada `ReferenceError: isPending is not defined` |
| `HRD07-REG-02` | P0 | Role `Read` POST `/initial-batches/{id}/validate` | 403, tidak ada DB write |
| `HRD07-REG-03` | P0 | `PendingApproval` dan `Approved` dikirim ke validate | Ditolak, status tidak mundur |
| `HRD07-REG-04` | P0 | Dua approver menyetujui batch berbeda untuk pegawai sama secara paralel | Maksimum satu approved/executed sah |
| `HRD07-REG-05` | P0 | SK Scheduled call `/cancel` tanpa `/cancel-request` | Ditolak, status tetap Scheduled |
| `HRD07-REG-06` | P0 | A request cancel, A approve cancel | Ditolak; B berizin dapat menyelesaikan |
| `HRD07-REG-07` | P0 | Scheduled dengan cancellation pending disapu worker | Tidak di-apply, tercatat dan eskalasi |
| `HRD07-REG-08` | P0 | 2 instance paralel masing-masing membuat SK | Semua nomor unik, tanpa silent duplicate |
| `HRD07-REG-09` | P0 | Klik tab Rotasi/Promosi lalu submit | Payload dan DB `ActionCategory` tepat |
| `HRD07-REG-10` | P0 | Backend mengirim nested `jobDescription` dengan `items` | FE merender jabatan + tujuan + butir + versi benar |
| `HRD07-REG-11` | P0 | Manager hierarchy A→B→C, buat C sebagai atasan A via SK | Ditolak sebelum apply, tidak ada cycle |
| `HRD07-REG-12` | P0 | Klik `+ Ajukan Perubahan Atasan` | Alur manager-only atau event yang valid, bukan Mutation default |
| `HRD07-REG-13` | P0 | Detail batch berisi old `BaseSalary` dan user tanpa `ViewAmount` membaca API | Tidak ada nominal dalam JSON, screenshot, console |
| `HRD07-REG-14` | P1 | Pegawai mutasi hari depan; baca mirror sebelum tanggal efektif | Unit/jabatan hari ini tetap lama |
| `HRD07-REG-15` | P1 | AsOf 2025, kini penempatan 2026; old assignment IsActive=false | Mengembalikan posisi 2025 sesuai histori, bukan 2026 |
| `HRD07-REG-16` | P1 | WIB 00:01 dengan UTC masih hari sebelumnya | Worker menginterpretasikan hari bisnis sesuai site |
| `HRD07-REG-17` | P1 | Onboarding memakai org unit tanpa dept relation | Fail-closed dengan pesan, tidak memakai OrganizationUnitId sebagai DepartmentId |
| `HRD07-REG-18` | P1 | Worker gagal di satu SK kemudian memproses SK berikut | Error tercatat, state tak tertinggal, SK lain aman |
| `HRD07-REG-19` | P1 | User berhak baca tetapi tidak approve SK/batch | UI tidak memberi misleading action dan server 403 |
| `HRD07-REG-20` | P1 | DB rollback saat batch row ke-3 dari lima gagal | 0 penempatan parsial, retry aman |

### 4.6 Gate validasi untuk setiap task dan pelaporan

**Per task FIN** catat minimal: `task ID`, 5 Submenu yang terdampak, file touched (path & diff), DTO/API sebelum-sesudah, test added, command test dan hasil (timestamp, PASS/FAIL/NOT RUN), izin/mode, blocker dan residual risk. Jika standar menetapkan test khusus, penuhi sesuai `TEST_POLICY`; jangan menonaktifkan test untuk memaksakan green.

**Command disesuaikan project dan izin, jangan dibuat-buat:**
- Backend: gunakan SDK `.NET 9` yang benar, targeted build/test/lint/static analyzer sesuai governance dan mandat user saat eksekusi. **Jika user secara eksplisit meminta SKIP build**, hormati dan tulis `NOT RUN`, jangan mengutip build lama sebagai hasil perubahan terbaru.
- Frontend: gunakan npm sesuai lockfile existing; jalankan lint/targeted unit/E2E dan script `check-css-usage.mjs` yang memang ada; browser headed jika tersedia. Jangan install dependencies atau ubah lockfile tanpa izin.
- Database: integration test hanya pada database test/staging yang **secara eksplisit diizinkan**, bukan dengan SQL write pada database produksi; gunakan transaction cleanup/test fixture yang aman. Bila tidak ada environment, gunakan unit/contract tests dan laporkan `DB INTEGRATION NOT RUN`.
- Jalankan setelah perbaikan FE/BE: `git diff --check` dan pemeriksaan diff scoped (read-only); jangan mengubah file lain untuk mempercantik build.

**Definition of Done (wajib semuanya untuk mengklaim Menu 07 siap UAT):**
1. Tidak ada P0 dari tabel 18 temuan yang belum diperbaiki/diblok sesuai alasan sah.
2. Lima Submenu dapat dibuka dan memuat data real, tidak ada runtime exception, endpoint benar, permission backend cocok.
3. Maker-checker SK dan batch, cancellation worker, unique SK number, manager cycle, tanggal efektif dan mirror diuji pada skenario negatif dan concurrency yang relevan.
4. Job description FE dan DTO backend identik secara kontrak; histori as-of bisa dipercaya.
5. Tidak ada gaji terpapar dari endpoint non-payroll atau perubahan salary langsung dari SK/batch.
6. Setiap FIN BE-001..009 dan FE-001..007 memiliki bukti test (PASS/FAIL/NOT RUN) dan linked source changes.
7. **UAT `PASS` hanya setelah browser/headed atau API integration nyata;** `READY FOR UAT` tidak berarti `PASS`. Jika tidak ada akses test, status terakhir adalah `IMPLEMENTED, NOT VERIFIED`, bukan selesai 100%.
8. Laporan lama direkonsiliasi: ubah klaim 100% menjadi berdasarkan bukti mutakhir, termasuk daftar kegagalan tersisa. Jika dokumen belum boleh ditulis pada mode source, siapkan perubahan dan laporkan `DOC UPDATE BLOCKED BY MODE` tanpa menyentuh file di luar target.

---

## 5. Stop conditions

**DILARANG berhenti** setelah audit ulang, membuat tabel gap, menyelesaikan hanya `isPending`, hanya memperbaiki CSS, atau menulis laporan baru tanpa implementasi source. Apabila ditemukan satu bug tambahan dalam scope, perbaiki dan uji; jangan meminta ulang persetujuan untuk requirement yang sudah dikunci.

**HENTIKAN hanya area terdampak** bila:

- branch/upstream berbeda dari mandat; kode belum dapat dimasukkan ke working tree tanpa menimpa perubahan lain;
- `AGENTS.md`, backend engineering contract, registry atau canonical rules tidak dapat dibaca: `BLOCKED — canonical governance unavailable`;
- registered runtime skill implementasi tidak tersedia: **`BLOCKED — Quilvian runtime skill unavailable`**;
- operasi perlu izin yang belum ada: schema/migration, DB production/staging write, deployment, Git mutation, atau target file di luar scope;
- keputusan canonical material belum ada (mis. kategori manager-only yang benar-benar memerlukan keputusan baru) — tahan hanya fitur itu dan lanjutkan yang independen;
- akses test browser/server/akun belum tersedia — tandai `NOT RUN` untuk test terkait, bukan menutup pekerjaan kode.

Jangan membuat/dijalankan migration atau SQL write, jangan auto commit/push/pull, dan jangan mengklaim kebijakan keamanan telah lulus hanya dari keberadaan `[Authorize]`. Jangan memperluas scope ke Payroll penuh atau sistem klinis tanpa izin. Jangan menciptakan role, status, API, data, atau persetujuan bisnis baru yang tidak didukung canonical.

---

## 6. Output

**Output Agent WAJIB setelah mengubah source (bukan sekadar opini):**

1. **Ringkasan implementasi riil** dengan SHA awal, branch, mode, registered Skills yang benar-benar dipanggil, file yang benar-benar diubah, simbol dan diff ringkas per task FIN.
2. **Matriks tepat lima Submenu**: Penempatan Awal, Perpindahan Unit, Rotasi Promosi, Uraian Tugas, Atasan Langsung; isi `IMPLEMENTED/REPAIR/PARTIAL/BLOCKED/NOT VERIFIED` berdasarkan kode dan bukti, bukan rasio buatan.
3. **Matriks seluruh 16 FIN**: `BE-001..009`, `FE-001..007`; status per task dan alasan termasuk bug P0/P1 yang telah ditutup serta bukti uji.
4. **Matriks 18 temuan `AUD-HRD07-01..18`** dengan `FIXED / NOT FIXED / BLOCKED`, file:line, test/response aktual; plus 34 `HRD07-UAT` dan 20 `HRD07-REG` jika environment tersedia.
5. **Kontrak API final** dengan method, URL, permission, request/response JSON aman, termasuk schema DTO nested JD dan status cancellation; error 401/403/409.
6. **Bukti validasi yang nyata**: perintah+hasil build/lint/unit/integration/CSS, waktu, browser click/headed, network responses dan re-query DB test; gunakan `NOT RUN` jika tidak dilakukan, tanpa menyembunyikan kegagalan.
7. **Perbarui laporan existing** `LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md` melalui mode dokumen sah, mempertahankan riwayat; koreksi endpoint yang salah dan klaim `100%` bila belum memenuhi DoD. Jika governance mengizinkan, tulis report FIN ke lokasi `docs/module-blueprints/human-resource/task/report/{backend,frontend}/` dan rekonsiliasi roadmap/traceability.
8. Tampilkan `git status --short` setelah pekerjaan dan jelaskan file di luar scope yang tetap untouched; **jangan lakukan commit atau push** tanpa instruksi baru.
9. Berikan **satu rekomendasi langkah selanjutnya yang konkret** untuk blocker tersisa dan checkpoint `MEMORY_PENDING_SYNC` jika memori proyek belum terotorisasi untuk disimpan.

**PERINTAH FINAL AGENT — EKSEKUSI SEKARANG:**

> Setelah preflight, verifikasi mode/izin dan pemanggilan registered runtime Skills sesuai governance, **LANGSUNG KERJAKAN PERBAIKAN SOURCE MENU 07**, dimulai dari **BACKEND**, dilanjutkan **FRONTEND**, lalu regression test dan UAT. Pertahankan lima Submenu dan 16 task FIN, perbaiki seluruh temuan P0/P1 yang terbukti, gunakan database/API nyata, dan jangan mengakhiri sesi hanya dengan audit, planning, atau laporan tanpa perubahan kode. Laporkan semua kondisi tidak teruji atau terblokir secara jujur.
