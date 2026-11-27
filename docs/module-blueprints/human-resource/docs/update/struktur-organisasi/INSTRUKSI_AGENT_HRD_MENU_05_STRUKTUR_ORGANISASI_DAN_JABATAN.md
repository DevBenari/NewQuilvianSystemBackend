# MASTER PROMPT - IMPLEMENTASI LANGSUNG HRD MENU 05: STRUKTUR ORGANISASI & JABATAN

> **AMANDEMEN 24 November 2026 — `HRD-DEC-257` (`ISSUE-HRD-008`, `PLAN-REPAIR-HRD-008`; nomor lama `HRD-DEC-248`).** Atas masukan analis bisnis, pengguna memutuskan: (1) submenu **Unit Kerja** (menu "Unit Organisasi") digabung ke **Struktur Organisasi** — halaman `/hr/master-data/organization-unit` kini menjadi menu Struktur Organisasi berisi daftar unit, tambah/ubah unit, dan mode `Daftar | Hierarki`; (2) **Department** dan **Position** keluar dari tab Struktur Organisasi menjadi menu sendiri di `/hr/master-data/organization/departments` dan `/hr/master-data/organization/positions`; (3) `/hr/master-data/organization` menjadi halaman pengalih. Bagian dokumen ini yang bertentangan dengan amandemen ditandai *(Diamandemen `HRD-DEC-257`)* dan tidak lagi mengikat; teks aslinya dipertahankan untuk jejak sejarah. Skema layar yang berlaku: `PLAN-REPAIR-HRD-008` bagian 3; arsitektur: `03-frontend-architecture.md` bagian 34.7.


**Jenis:** Instruksi eksekusi Agent Fullstack Quilvian V2, bukan laporan implementasi dan bukan prompt audit-only.  
**Target Menu:** Kepegawaian > Menu 05 - Struktur Organisasi & Jabatan.  
**Scope:** 6 Submenu target: Unit Kerja, Struktur Organisasi, Jabatan, Rumpun Jabatan, Uraian Tugas, Atasan Langsung.  
**Tanggal snapshot:** 16 November 2026.  
**Frontend:** `DevBenari/QuilvianSystemFrontendDev` branch `HamzahV2` @ `4672040faeb9742d58f9a00f4dccb1d5eaf1e7f3`.  
**Backend:** `DevBenari/NewQuilvianSystemBackend` branch `MHamzah` @ `bdae40837d8ec00f0c23471e0d971dd98e009f42`.  
**Engineering Skills:** `DevBenari/QuilvianEngineeringSkills` branch `main` @ `201af64552b27873fd9af83c6722cf11b441bb1e`.  
**Task numbering:** task finishing baru memakai `FIN-HRD-05-BE-001..004` dan `FIN-HRD-05-FE-001..006`, terpisah dari roadmap reguler dan tetap `PROPOSED / FINISHING_REGISTER_PENDING` sampai benar-benar diregistrasikan sesuai governance.  
**Prinsip utama:** pertahankan source-of-truth existing; jangan membangun master paralel. Pekerjaan ini adalah finishing terarah dengan perubahan terbesar pada Uraian Tugas, bukan rebuild Struktur Organisasi.

---

# 1. Role

Anda adalah **Senior Fullstack Engineer Quilvian V2 + analis proses bisnis HR rumah sakit + UI/UX engineer + QA/UAT engineer** yang diberi mandat untuk **langsung mengimplementasikan** finishing Menu 05 setelah preflight governance selesai. Jangan berhenti pada analisis, daftar rekomendasi, atau validasi ulang requirement yang sudah dikunci oleh owner pada percakapan ini.

Tanggung jawab Anda:

1. Menjaga arsitektur existing dan menggunakan capability yang sudah tersedia sebagai source-of-truth.
2. Mengerjakan backend terlebih dahulu, kemudian frontend, kemudian integrasi/UAT.
3. Menambahkan capability baru hanya untuk gap yang nyata, terutama Uraian Tugas berversi/effective-dated.
4. Menggunakan API real dan persistence real; dilarang mengganti backend yang belum siap dengan mock, dummy, hardcode, fake success, local-only state, atau fallback data contoh.
5. Mematuhi `AGENTS.md`, `CLAUDE.md`, Backend Engineering Contract, Prefix Registry, dan aturan Quilvian Engineering Skills.
6. Memanggil runtime skill `quilvian-engineering-skills:build-module-backend` sebelum perubahan source backend dan `quilvian-engineering-skills:build-module-frontend` sebelum perubahan source frontend. Bila skill runtime tidak tersedia, berhenti pada layer itu dan laporkan persis `BLOCKED — Quilvian runtime skill unavailable`.
7. Tidak menjalankan `git commit`, `git push`, `git pull`, `git merge`, `git rebase`, deploy, pembuatan/eksekusi migration, atau eksekusi database tanpa izin eksplisit terpisah yang diwajibkan governance.
8. Tidak membuat tabel/entity/route paralel yang menduplikasi `MstOrganizationUnit`, `MstPosition`, `MstJobFamily`, atau `WfpManagerAssignment`.

Batas domain yang wajib dijaga:

- **Menu 05** mendefinisikan dan menyajikan struktur tempat pegawai bekerja: unit, hierarki, jabatan, rumpun, uraian tugas, dan konteks reporting.
- **Menu 07** menjadi domain perubahan penempatan/mutasi/promosi pegawai pada struktur tersebut.
- **Menu 06** menjadi domain bukti administratif/SK untuk perubahan yang mensyaratkannya.
- Menu 05 tidak boleh menjadi jalur bypass untuk memindahkan pegawai, mengganti atasan aktual secara langsung, atau mengubah unit/jabatan pegawai tanpa workflow yang sah.

---

# 2. Task

## 2.1 Target bisnis dan hasil implementasi

Hasil akhir harus membuat pengguna HRD dapat:

1. Mengelola **Unit Kerja** memakai master unit organisasi existing dengan hubungan parent-child yang jelas dan aman.
2. Melihat **Struktur Organisasi** dalam mode daftar existing dan mode hierarki/tree tambahan tanpa merusak Department/Position CRUD yang sudah ada.
3. Mengelola dan melihat **Jabatan** dari `MstPosition` existing, serta membuka Uraian Tugas dari konteks jabatan tanpa membuat master jabatan kedua.
4. Mengelola **Rumpun Jabatan** dari `MstJobFamily` existing tanpa menduplikasi tabel atau memaksakan relasi baru yang tidak disetujui source/canonical contract.
5. Mengelola **Uraian Tugas** sebagai capability nyata per jabatan, memiliki histori versi dan periode efektif; bukan hanya mengisi `MstPosition.Description`.
6. Melihat **Atasan Langsung** dari `WfpManagerAssignment` real sebagai read model struktur pelaporan, tanpa menjadikan Menu 05 sumber mutasi relasi atasan.
7. Mendapat loading/empty/error/success yang jujur, RBAC backend+frontend, data tetap benar setelah refresh, dan tidak ada fake success.

## 2.2 Keputusan bisnis yang dikunci

Keputusan berikut **APPROVED oleh user pada percakapan 16 November 2026**, tetapi belum otomatis berarti sudah tertulis pada canonical repository; bila canonical belum sinkron, rekonsiliasikan sesuai mode dokumen yang diizinkan sebelum atau bersamaan dengan implementasi tanpa mengubah keputusan ini:

- **DEC-M05-01 — No rebuild:** Struktur Organisasi existing dinilai sudah cukup bagus. Jangan melakukan redesign/rewrite besar.
- **DEC-M05-02 — Unit Kerja reuse:** `MstOrganizationUnit` adalah source-of-truth Unit Kerja. Dilarang membuat `MstUnitKerja` baru.
- **DEC-M05-03 — Hierarki reuse:** gunakan `ParentOrganizationUnitId` dan `LevelNumber` existing untuk tree/hierarchy. Mode Hierarki adalah enhancement P2; mode daftar existing tetap dipertahankan.
- **DEC-M05-04 — Jabatan reuse:** `MstPosition` tetap source-of-truth definisi jabatan. Jangan menyimpan occupant/pegawai di master jabatan.
- **DEC-M05-05 — Rumpun reuse:** `MstJobFamily` tetap source-of-truth Rumpun Jabatan. Jangan membuat master rumpun paralel.
- **DEC-M05-06 — Uraian Tugas dedicated:** Uraian Tugas adalah gap utama dan harus menjadi capability khusus yang terkait ke jabatan, mempunyai histori versi/periode efektif, serta minimal memuat tujuan jabatan, tugas pokok, tanggung jawab, wewenang, dan hubungan kerja. `MstPosition.Description` tidak boleh dianggap sebagai pengganti lengkap.
- **DEC-M05-07 — Atasan Langsung reuse:** `WfpManagerAssignment` tetap source-of-truth relasi atasan aktual. Menu 05 hanya melihat/memahami struktur reporting; perubahan orang/atasan aktual wajib melalui workflow penempatan/mutasi yang sah.
- **DEC-M05-08 — Backend first:** backend dikerjakan dahulu, frontend sesudah kontrak API dan persistence jelas.
- **DEC-M05-09 — Real data only:** dilarang dummy/mock/hardcode/fake success.
- **DEC-M05-10 — UI consistency:** gunakan Base Components dan pola halaman existing; jangan membuat design system atau komponen paralel.
- **DEC-M05-11 — Relasi JobFamily/JobLevel:** jangan menambah FK master baru hanya agar UI terlihat lengkap. Jika relasi master tidak terbukti canonical, tampilkan hanya informasi yang memang tersedia dari source real atau tandai tidak tersedia; jangan mengarang kontrak.

## 2.3 Instruksi rinci per Submenu

### Submenu 1 - Unit Kerja

- **Tujuan RS dan aktor:** HR Admin/HR Master Data mengelola unit struktural tempat fungsi rumah sakit berjalan. Read access dapat diberikan ke role yang membutuhkan struktur organisasi, tetapi create/update/status tetap mengikuti permission `OrganizationUnit` existing.
- **Input/output utama:** nama unit, kode unit, tipe unit, parent unit, legal entity, hospital site, level, status operasional, periode efektif, deskripsi, status aktif. Output list harus memperlihatkan hubungan parent-child secara jelas dan jumlah child bila tersedia.
- **Alur normal:** buka daftar -> filter/search -> lihat detail -> tambah/edit unit sesuai permission -> pilih parent yang valid -> simpan -> backend memvalidasi parent dan periode -> refetch list/detail -> data tetap benar setelah reload.
- **Validasi wajib:** parent tidak boleh menunjuk diri sendiri; parent tidak boleh menjadi descendant dari node yang sedang dipindahkan; tidak boleh membentuk cycle; `EffectiveEndDate` tidak boleh lebih awal dari `EffectiveStartDate`; `LevelNumber` tidak boleh menjadi angka arbitrer yang bertentangan dengan hierarchy; inactive/deleted parent tidak boleh dipilih untuk assignment baru kecuali aturan existing eksplisit mengizinkan historical reference.
- **Reuse wajib:** `MstOrganizationUnit` dan `OrganizationUnitController`; jangan membuat entity Unit Kerja baru.
- **Status source:** `IMPLEMENTED, NOT VERIFIED` secara runtime. Source model memiliki `ParentOrganizationUnitId`, `UnitType`, `LevelNumber`, effective dates dan child navigation. Controller sudah mempunyai summary/list/options dengan permission `OrganizationUnit : Read`.
- **Perbaikan yang harus dikerjakan:** harden invariants hierarchy pada create/update bila belum ada; sediakan read model hierarchy yang dapat dipakai UI tree tanpa mengambil data dummy atau memaksa frontend melakukan pagination sweep tidak aman.
- **Frontend:** pertahankan route `/hr/master-data/organization-unit`, layout Hero + SummaryCards + filters + DataTable + pagination. Pastikan kolom/context parent terlihat dan tambah navigasi ke hierarki bila relevan. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **FIN terkait:** `FIN-HRD-05-BE-001`, `FIN-HRD-05-FE-001`, `FIN-HRD-05-FE-002`, `FIN-HRD-05-FE-006`.
- **Selesai bila:** hierarchy aman dari cycle, data real ter-render, perubahan persisten, permission benar, error tidak berubah menjadi success toast, dan UAT-05-01 s.d. UAT-05-04 lulus ketika environment tersedia.

### Submenu 2 - Struktur Organisasi

- **Tujuan RS dan aktor:** memberikan gambaran struktur formal organisasi rumah sakit tanpa mengganti master Department/Position yang sudah ada. HR dapat melihat relasi unit dan posisi; pengguna read-only dapat melihat struktur sesuai permission.
- **Existing behavior yang dipertahankan:** `/hr/master-data/organization` sekarang adalah halaman “Manajemen Organization” dengan tab Department dan Position serta summary/filter/table. Ini **tidak boleh dihapus atau didesain ulang total**. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **Enhancement yang disetujui:** tambahkan mode visual `Daftar | Hierarki` atau kontrol setara. Mode Daftar mempertahankan tab Department/Position existing. Mode Hierarki memakai tree `MstOrganizationUnit` dari endpoint/read model real, bukan data hardcoded. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **Hierarki target:** root hospital/legal entity -> Directorate -> Division/Installation/Department -> Section/Team/Unit sesuai `UnitType` yang benar-benar tersedia. Jangan memaksakan level yang tidak ada di database.
- **Interaksi:** klik node menampilkan ringkasan unit (nama, kode, tipe, parent, status, child count, periode efektif) dan tombol `Lihat Detail Unit` yang membawa pengguna ke detail Unit Kerja existing; bukan modal edit baru yang menggandakan form.
- **Keamanan:** mode hierarki adalah read-only kecuali pengguna masuk ke capability Unit Kerja yang memang memiliki permission update. Tidak ada drag-and-drop pindah organisasi pada finishing ini. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **Backend:** endpoint/read model hierarchy harus cycle-safe, dapat difilter tanggal efektif, legal entity/hospital site bila source mendukung, serta mengembalikan DTO minimum yang diperlukan UI.
- **FIN terkait:** `FIN-HRD-05-BE-001`, `FIN-HRD-05-FE-002`, `FIN-HRD-05-FE-006`.
- **Prioritas:** P2. Jika P0/P1 belum stabil, selesaikan P0/P1 dahulu. P2 tidak boleh menghalangi Uraian Tugas.
- **Selesai bila:** halaman existing tidak regresi, mode daftar tetap berjalan, mode hierarchy bersumber API real, node detail benar, state loading/empty/error jelas, responsive dan keyboard navigation minimal tersedia.

### Submenu 3 - Jabatan

- **Tujuan RS dan aktor:** HR Master Data mengelola definisi jabatan yang dapat digunakan oleh penempatan pegawai. Jabatan adalah role/slot organisasi, bukan identitas pegawai.
- **Source-of-truth:** `MstPosition` existing dengan `PositionCode`, `PositionName`, `DepartmentId`, `Description`, `IsActive`, serta relasi competency requirement. Gunakan `OrganizationController` existing untuk list/detail/create/update Position.
- **Batas penting:** jangan menambahkan nama pegawai/occupant ke master Position sebagai state permanen. Occupant aktual dibaca dari assignment; perubahan occupant bukan pekerjaan master Jabatan.
- **Perbaikan UI:** pertahankan tab Position pada `/hr/master-data/organization`; tambahkan affordance yang jelas untuk membuka Uraian Tugas posisi. Pada detail Position tampilkan ringkasan kode, nama, department, status, deskripsi dan status Uraian Tugas (ada aktif/belum ada) dari API real. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **Rumpun/level/grade:** jangan membuat relasi schema baru hanya karena `WfpPositionAssignment` memiliki `JobFamilyId`, `JobLevelId`, atau grade. Bila canonical memutuskan klasifikasi itu milik assignment, pertahankan demikian. UI tidak boleh menampilkan label rumpun/level palsu pada master Position.
- **Integrasi Uraian Tugas:** satu Position dapat mempunyai beberapa versi historis Uraian Tugas, tetapi pada satu tanggal acuan maksimal satu versi efektif.
- **FIN terkait:** `FIN-HRD-05-BE-002`, `FIN-HRD-05-BE-003`, `FIN-HRD-05-FE-003`, `FIN-HRD-05-FE-004`, `FIN-HRD-05-FE-006`.
- **Selesai bila:** master Position existing tetap kompatibel, Uraian Tugas dapat dibuka dari Position, tidak terjadi duplicate master, dan data tetap real setelah reload.

### Submenu 4 - Rumpun Jabatan

- **Tujuan RS dan aktor:** mengelola klasifikasi/rumpun jabatan untuk kebutuhan HR yang memang menggunakan Job Family. HR Master Data menjadi pengelola; role lain dapat read sesuai permission.
- **Source-of-truth:** `MstJobFamily` dan `JobFamilyController` existing. Endpoint sudah mempunyai metadata filter, summary, list, options, detail, create, update, status dan delete dengan permission `JobFamily`.
- **Perlakuan:** **REUSE / FINISH**, bukan redesign dan bukan schema baru. Pertahankan `/hr/master-data/job-family` serta komposisi Hero + SummaryCards + DataTable + filter/pagination existing.
- **Validasi finishing:** pastikan seluruh data berasal API real, loading/empty/error tidak memunculkan contoh data, create/update/status benar-benar refetch, dan delete mengikuti aturan referential integrity existing. Jangan memaksa delete jika masih dipakai assignment; tangani conflict server secara jujur.
- **Hubungan dengan Jabatan:** jika source/canonical menyediakan mapping yang sah, tampilkan usage/reference secara read-only. Jika tidak, jangan menambahkan FK baru tanpa keputusan arsitektur; Rumpun Jabatan tetap boleh berdiri sebagai master yang dipakai pada `WfpPositionAssignment`.
- **FIN terkait:** `FIN-HRD-05-FE-003`, `FIN-HRD-05-FE-006`. Backend tidak memperoleh task khusus JobFamily karena controller/model existing sudah substantif; perubahan backend hanya dilakukan bila test membuktikan bug dalam scope FIN yang sama dan mengikuti governance.
- **Selesai bila:** UI existing konsisten, API real, error/refetch benar, tidak ada mock/fake success, dan tidak ada master rumpun baru.

### Submenu 5 - Uraian Tugas

- **Tujuan RS dan aktor:** HR dapat mendefinisikan mandat suatu jabatan secara formal dan historis. Uraian Tugas adalah definisi **per jabatan**, bukan dokumen bebas per pegawai. HR Master Data dapat membuat dan mengubah draft/versi; read role dapat melihat versi efektif sesuai permission.
- **Gap:** pencarian source saat pembahasan tidak menemukan capability HR khusus yang memadai; `MstPosition.Description` terlalu sempit untuk mewakili tujuan, tugas pokok, tanggung jawab, wewenang dan hubungan kerja. Klasifikasi: `MISSING / DEDICATED CAPABILITY NOT VERIFIED`.
- **Model domain yang harus dibangun:** persistence dedicated yang mereferensikan `PositionId`, mempunyai `VersionNumber` atau version identity setara, `EffectiveStartDate`, `EffectiveEndDate` nullable, status teknis minimum (`Draft`/`Active` atau nama setara sesuai contract), audit create/update/actor, dan section content. Gunakan naming/prefix canonical dari `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; jangan otomatis menamai entity baru `Mst*` atau `Hrd*` tanpa melewati contract/QBE.
- **Konten minimum:** `Purpose/Tujuan Jabatan`; ordered items untuk `MainDuty/Tugas Pokok`; `Responsibility/Tanggung Jawab`; `Authority/Wewenang`; `WorkRelation/Hubungan Kerja`. Implementasi boleh memakai child rows typed + sequence atau struktur relational lain yang lolos Backend Engineering Contract; dilarang menyimpan seluruh capability sebagai satu string `Description` 250 karakter.
- **Effective dating:** satu Position tidak boleh mempunyai dua versi `Active` yang overlap pada tanggal yang sama. Aktivasi versi baru harus menutup/menetapkan akhir versi sebelumnya secara konsisten, atau menolak konflik 409 sesuai desain service. History tidak boleh ditimpa diam-diam.
- **Draft behavior:** menyimpan draft tidak mengubah versi efektif yang sedang dipakai. Edit terhadap versi historis yang sudah berlaku tidak boleh merusak audit; koreksi substansial dibuat sebagai versi baru menurut contract.
- **API minimum:** metadata/filter bila pola master data membutuhkannya; summary; paged list; detail by id; current/effective by position/date; history by position; create draft/version; update draft; activate/publish; deactivate/close sesuai lifecycle yang disetujui contract. Method dan route final harus mengikuti API Rules; route usulan: `/api/v1/corporate/human-resource/master-data/job-descriptions`.
- **RBAC:** resource permission baru hanya dibuat jika Prefix/permission registry dan governance membenarkan; minimal Read/Create/Update/Activate atau action setara. Frontend tidak boleh mengandalkan hidden button saja; backend wajib enforce.
- **Concurrency/idempotency:** double submit activate tidak boleh membuat dua versi efektif. Gunakan transaction dan concurrency guard yang sesuai PostgreSQL/EF pattern existing; jangan Count/Max/Last+1 yang dilarang contract untuk nomor bisnis. Jika VersionNumber memerlukan generator, gunakan pola concurrency-safe.
- **Frontend:** route usulan `/hr/master-data/job-description` dengan list, detail, editor, dan version history; juga dapat dibuka dari detail Position.
- **FIN terkait:** `FIN-HRD-05-BE-002`, `FIN-HRD-05-BE-003`, `FIN-HRD-05-BE-004`, `FIN-HRD-05-FE-003`, `FIN-HRD-05-FE-004`, `FIN-HRD-05-FE-006`.
- **Selesai bila:** persistence real, history berversi, tidak overlap, RBAC benar, reload tetap menyimpan data, UI tidak memakai dummy, dan UAT-05-07 s.d. UAT-05-10 serta UAT-05-13/15/16 dapat lulus.

### Submenu 6 - Atasan Langsung

- **Tujuan RS dan aktor:** memberi visibilitas siapa atasan langsung/utama pegawai dan apakah relasi tersebut dapat menyetujui request, sehingga HR memahami reporting chain yang digunakan workflow.
- **Source-of-truth:** `WfpManagerAssignment`. Entity existing memiliki `ManagerWorkforceProfileId`, `OrganizationUnitId`, `ManagerPositionId`, `ManagerType`, effective dates, `IsPrimaryManager`, `CanApproveRequests`, approval fields, dan navigation real.
- **Existing API:** `GET /api/v1/corporate/human-resource/workforce-core/manager-assignments` melalui `WorkforceCoreCrossEmployeeController` dengan permission `WfpManagerAssignment : ReadAll`; query service menggunakan EF database dan include manager/department/position.
- **Existing FE:** `/hr/workforce-core/manager-assignments`, title “Relasi Atasan”, menggunakan service real dan menampilkan Pegawai, Atasan Langsung, Jenis, Atasan Utama, Boleh Menyetujui, Masa Berlaku, Status.
- **Perlakuan Menu 05:** reuse route/view existing. Menu 05 boleh memberi entry/shortcut label `Atasan Langsung` ke capability ini dan boleh menambah read-only context chain, tetapi **tidak boleh** membuat manager table/entity kedua.
- **Mutation guard:** jangan menambahkan tombol `Ganti Atasan` yang melakukan PUT langsung dari Menu 05. Bila pengguna perlu perubahan, arahkan ke workflow penempatan/mutasi/SK yang sah. Historical manager assignment tetap effective-dated.
- **FIN terkait:** `FIN-HRD-05-BE-004`, `FIN-HRD-05-FE-005`, `FIN-HRD-05-FE-006`.
- **Selesai bila:** list memakai data real, permission `ReadAll` dihormati, tidak ada bypass mutation, shortcut/navigasi tidak menduplikasi view, dan UAT-05-11/12/13 lulus.

## 2.4 Register FIN Backend dan Frontend - nomor terpisah dari roadmap

> Semua status awal di bawah adalah `PROPOSED / FINISHING_REGISTER_PENDING`. Jangan mengklaim task sudah terdaftar di roadmap hanya karena dokumen ini menyebut ID. Untuk registrasi, gunakan mode dokumentasi yang diizinkan dan pertahankan task `BE-HRD-*` / `FE-HRD-*` existing sebagai baseline historis.

### Backend - BACKEND MODE

| FIN Task ID | Jenis | Prioritas | Submenu + target kode | Baseline roadmap existing | Dependencies | Mode / target | Acceptance / UAT | Status |
|---|---|---|---|---|---|---|---|---|
| `FIN-HRD-05-BE-001` | `FIX/INTEGRATION` | P1 | Unit Kerja + Struktur Organisasi: harden hierarchy `MstOrganizationUnit`, cycle guard, effective hierarchy read model/API | `BE-HRD-029` (ancestor traversal reuse); master task khusus `OrganizationUnit` = `NONE_VERIFIED` | Existing `OrganizationUnitController`; `MstOrganizationUnit` | BACKEND MODE; `Areas/Corporate/HumanResource/MasterData/Organization/**` | parent/self/descendant invalid ditolak; hierarchy real; no schema duplicate; UAT-05-01..04 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-BE-002` | `ADD` | P0 | Uraian Tugas: persistence versioned/effective-dated per `MstPosition`, section content + audit | `NONE_VERIFIED` | Prefix Registry, Backend Engineering Contract, `MstPosition` | BACKEND MODE; Organization master-data domain + EF config path canonical | satu versi efektif per tanggal; history immutable; persistence real; UAT-05-07..09 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-BE-003` | `ADD/SECURITY` | P0 | Uraian Tugas API/service/DTO/RBAC + activation/version transaction + 409 conflict | `NONE_VERIFIED` | `FIN-HRD-05-BE-002` | BACKEND MODE; controller/service/DTO/permission canonical | list/detail/history/effective/create/update/activate; 403/400/409 benar; UAT-05-07..10,15,16 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-BE-004` | `INTEGRATION/UAT` | P1 | Atasan Langsung + Menu 05 integration guard dan automated/integration tests lintas hierarchy/job description/manager read model | `BE-HRD-003`, `BE-HRD-023`, `BE-HRD-026` | `FIN-HRD-05-BE-001..003`; `WfpManagerAssignment` existing | BACKEND MODE; WorkforceCore read path + tests; no direct mutation path baru | Menu05 hanya read manager; no duplicate source; reload/concurrency/RBAC tests; UAT-05-11..16 | `PROPOSED / FINISHING_REGISTER_PENDING` |

### Frontend - FRONTEND MODE

| FIN Task ID | Jenis | Prioritas | Submenu + target kode | Baseline roadmap existing | Dependencies | Mode / target | Acceptance / UAT | Status |
|---|---|---|---|---|---|---|---|---|
| `FIN-HRD-05-FE-001` | `FIX` | P1 | Unit Kerja: finishing existing list/detail/form, parent context, honest states | `NONE_VERIFIED` | `FIN-HRD-05-BE-001` | FRONTEND MODE; `/hr/master-data/organization-unit` | data real, parent jelas, no fake success, reload persisten; UAT-05-01..03 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-FE-002` | `ADD` | P2 | Struktur Organisasi: tambah mode Hierarki pada halaman existing tanpa merusak Department/Position mode | `NONE_VERIFIED` | `FIN-HRD-05-BE-001`, `FIN-HRD-05-FE-001` | FRONTEND MODE; `/hr/master-data/organization` | list existing tidak regresi, tree API real, node detail + responsive; UAT-05-04 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-FE-003` | `FIX/INTEGRATION` | P1 | Jabatan + Rumpun Jabatan: finishing detail/navigation, link Uraian Tugas, konsistensi API real | `NONE_VERIFIED` | `FIN-HRD-05-BE-003` untuk status Uraian Tugas; existing Organization/JobFamily API | FRONTEND MODE; `/hr/master-data/organization`, `/hr/master-data/job-family` | tidak ada master duplicate/FK palsu; state real; UAT-05-05..06 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-FE-004` | `ADD` | P0 | Uraian Tugas: list, detail, editor, version history, activation flow | `NONE_VERIFIED` | `FIN-HRD-05-BE-002`, `FIN-HRD-05-BE-003` | FRONTEND MODE; new proposed `/hr/master-data/job-description` | field lengkap, version history, 403/409, anti double submit, persistence; UAT-05-07..10,13,15,16 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-FE-005` | `INTEGRATION` | P1 | Atasan Langsung: reuse manager assignment view + Menu05 navigation/readonly context, mutation routed away | `FE-HRD-002` | existing `WfpManagerAssignment` API; `FIN-HRD-05-BE-004` | FRONTEND MODE; `/hr/workforce-core/manager-assignments` + menu sidebar | same real view reused; no direct Ganti Atasan; UAT-05-11..13 | `PROPOSED / FINISHING_REGISTER_PENDING` |
| `FIN-HRD-05-FE-006` | `UAT` | P1 | Integrasi UI Menu05: states, responsive, a11y, error, reload, RBAC, cross-navigation | `FE-HRD-002` + existing pages | `FIN-HRD-05-FE-001..005`; backend ready | FRONTEND MODE; all affected screens/tests | desktop/tablet/mobile, keyboard, 403/409/5xx, reload, no stale data; UAT-05-01..16 | `PROPOSED / FINISHING_REGISTER_PENDING` |

### 2.4.1 Detail eksekusi FIN Backend

**`FIN-HRD-05-BE-001`**
- Reuse `MstOrganizationUnit`; jangan ubah menjadi entity baru.
- Audit create/update existing lalu implementasikan guard parent-self, descendant-cycle, effective date, dan level consistency jika belum dipenuhi.
- Tambahkan read endpoint hierarchy atau read service setara yang mengembalikan DTO minimal untuk tree. Gunakan permission `OrganizationUnit : Read` kecuali permission canonical mengharuskan resource khusus.
- Endpoint hierarchy harus dapat mengambil seluruh tree yang relevan secara aman tanpa frontend looping semua halaman. Gunakan query EF yang bounded dan hindari N+1.
- Tidak ada drag/drop mutasi organisasi dalam scope.

**`FIN-HRD-05-BE-002`**
- Desain persistence Uraian Tugas mengikuti `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
- Referensi wajib ke `MstPosition.Id` tanpa menggandakan Position.
- Simpan header version + ordered typed items/sections atau struktur normalized setara.
- Enforce unique/effective overlap invariant pada service + database constraint/index jika contract mengizinkan.
- Migration creation/execution mengikuti permission terpisah; jangan otomatis menjalankannya.

**`FIN-HRD-05-BE-003`**
- Implement service layer; jangan menaruh business workflow baru secara liar di controller bila contract meminta service.
- Sediakan DTO terpisah untuk list/detail/editor/history agar payload tidak berlebihan.
- Conflict versi/overlap -> 409; invalid date/field -> 400/422 sesuai API rules; no permission -> 403; not found -> 404.
- Activation harus transaksional; double-submit atau repeated request tidak boleh membuat dua versi efektif.
- Audit actor dan timestamp harus berasal dari mekanisme canonical, bukan request body yang dipercaya begitu saja.

**`FIN-HRD-05-BE-004`**
- Jangan membuat write endpoint baru untuk Atasan Langsung pada master Organization.
- Pertahankan `GET .../workforce-core/manager-assignments` sebagai read source Menu05.
- Tambahkan test untuk role read/denied, period/effective behavior, hierarchy cycle, job description version overlap, activation concurrency/idempotency, dan persistence setelah context baru.
- Bila contract existing mengharuskan report/trace update, lakukan hanya pada mode/path yang diizinkan.

### 2.4.2 Detail eksekusi FIN Frontend

**`FIN-HRD-05-FE-001`** mempertahankan `MasterDataOrganizationUnitView` dan base components existing; perbaiki hanya area yang dibutuhkan untuk parent/hierarchy clarity dan state yang jujur.

**`FIN-HRD-05-FE-002`** menambahkan visual hierarchy sebagai mode sekunder; jangan mengganti tab Department/Position yang sudah bekerja. Reuse API real `FIN-HRD-05-BE-001`.

**`FIN-HRD-05-FE-003`** menambahkan navigasi/indicator Uraian Tugas pada Position detail dan menjaga Rumpun Jabatan existing. Dilarang memalsukan JobFamily-to-Position mapping yang tidak ada.

**`FIN-HRD-05-FE-004`** membangun UI Uraian Tugas lengkap dengan list, detail, edit draft, riwayat versi dan activation confirmation. UI baru wajib memakai base components yang lolos decision gate.

**`FIN-HRD-05-FE-005`** mereuse `ManagerAssignmentsView`; navigasi Menu05 mengarah ke route existing, tidak membuat view copy-paste. Aksi perubahan relasi tidak dibuat di Menu05.

**`FIN-HRD-05-FE-006`** menjalankan integrasi dan visual UAT seluruh screen; jika browser/headed environment tidak tersedia, laporkan `NOT RUN`, bukan PASS.

## 2.5 Skema Tampilan Global

| Submenu | Halaman/route | Status | FIN FE owner | Aktor | Area utama | Aksi utama | API/source | UAT |
|---|---|---|---|---|---|---|---|---|
| Unit Kerja | `/hr/master-data/organization-unit` | `EXISTING_REUSE/EXTEND` *(Diamandemen `HRD-DEC-257`)* | `FIN-HRD-05-FE-001` | HR Master Data / read role | Hero, summary, filter, table, detail/form | cari, filter, detail, tambah/edit sesuai permission | `organization-units` existing + hierarchy read model | UAT-05-01..03 |
| Struktur Organisasi | `/hr/master-data/organization` | `EXISTING_REUSE + NEW_P2_VIEW_MODE` *(Diamandemen `HRD-DEC-257`)* | `FIN-HRD-05-FE-002` | HR/read role | existing Department/Position list + tree mode | switch daftar/hierarki, klik node, lihat detail unit | Organization existing + hierarchy API | UAT-05-04 |
| Jabatan | `/hr/master-data/organization` tab Position | `EXISTING_EXTEND` | `FIN-HRD-05-FE-003` | HR Master Data | position table/detail | create/edit existing, buka Uraian Tugas | Organization Position API + Job Description API | UAT-05-05 |
| Rumpun Jabatan | `/hr/master-data/job-family` | `EXISTING_REUSE/FINISH` | `FIN-HRD-05-FE-003` | HR Master Data | Hero, summary, filter, table, detail/form | CRUD/status existing sesuai permission | `job-families` existing | UAT-05-06 |
| Uraian Tugas | `/hr/master-data/job-description` | `NEW_PROPOSED` | `FIN-HRD-05-FE-004` | HR Master Data/read role | summary, list, detail editor, version history | buat draft, edit, aktifkan versi, lihat histori | Job Description API dari `FIN-HRD-05-BE-003` | UAT-05-07..10,13,15,16 |
| Atasan Langsung | `/hr/workforce-core/manager-assignments` | `EXISTING_REUSE` | `FIN-HRD-05-FE-005` | HR/read-all role | list relasi atasan + filters | lihat/filter; navigasi ke workflow bila perlu perubahan | `GET .../manager-assignments` | UAT-05-11..13 |
| Seluruh Menu05 | semua route di atas | `CROSS_SCREEN_UAT` | `FIN-HRD-05-FE-006` | multi-role | state/error/responsive/a11y | reload, denial, retry, keyboard, viewport test | seluruh API real | UAT-05-01..16 |


#### Skema Tampilan - FIN-HRD-05-FE-001

- **Route/halaman:** `EXISTING_REUSE/EXTEND` pada `/hr/master-data/organization-unit`; source page `src/app/hr/master-data/organization-unit/page.jsx`, view `src/components/view/hr/master-data/organization-unit/master-data-organization-unit-view.jsx`. Tidak membuat route Unit Kerja kedua.
- **Aktor & akses:** HR Master Data dengan permission `OrganizationUnit : Read/Create/Update/...` sesuai source backend; user read-only hanya dapat melihat. Tombol create/edit/status disembunyikan/disabled berdasarkan access metadata, tetapi backend tetap wajib menolak aksi tanpa permission.
- **Layout / wireframe:** pertahankan komposisi existing dan tambahkan clarity parent/hierarchy tanpa redesign massal.

```text
DESKTOP
+--------------------------------------------------------------------------------+
| Breadcrumb > HRD > Struktur Organisasi & Jabatan > Unit Kerja                  |
| UNIT KERJA                                             [+ Tambah Unit]         |
| Kelola hierarki unit organisasi rumah sakit                                    |
| [Total Unit] [Aktif] [Nonaktif] [Operasional] [Root Unit]                     |
|--------------------------------------------------------------------------------|
| [Cari................] [Tipe Unit v] [Parent v] [Status v] [Periode] [Reset]   |
|--------------------------------------------------------------------------------|
| Kode | Nama Unit | Tipe | Parent Unit | Level | Child | Berlaku | Status | Aksi |
|--------------------------------------------------------------------------------|
| ... data real dari API ...                                                     |
|--------------------------------------------------------------------------------|
| 1-25 dari N                                                 [< 1 2 3 >]         |
+--------------------------------------------------------------------------------+

DETAIL / FORM
+--------------------------------------------------------------+
| Detail / Edit Unit Kerja                                     |
| Kode Unit       [read-only generated]                        |
| Nama Unit       [____________________________]               |
| Tipe Unit       [Directorate/Division/... v]                 |
| Parent Unit     [__________________________ v]               |
| Legal Entity    [__________________________ v]               |
| Hospital Site   [__________________________ v]               |
| Unit Operasional [x]                                         |
| Tgl Mulai       [dd/mm/yyyy]  Tgl Akhir [dd/mm/yyyy]         |
| Deskripsi       [........................................]    |
|                                         [Batal] [Simpan]     |
+--------------------------------------------------------------+
```

- **Komponen & reuse:** `Hero`, `SummaryCards`, `DataTable`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `BaseButton`, `StatusBadge`, pagination/base detail/form yang sudah terbukti di module. Gate = `REUSE/EXTEND`; jangan membuat table/button/filter baru jika base component sudah memenuhi kebutuhan.
- **Data / field & validasi:** list minimal: kode, nama unit, tipe, parent, level, child count, periode efektif, status. Form memakai field existing dari DTO/model: legal entity, hospital site, parent, unit name/type, operational flag, dates, description, active state. Parent tidak boleh self/descendant; invalid cycle harus menghasilkan inline/error server yang jelas. Jangan memasukkan nilai default dummy.
- **Aksi & interaksi:** `Tambah Unit` hanya untuk create permission; row action `Lihat`, `Edit` sesuai permission. `Simpan` disabled saat request berjalan untuk mencegah double submit; success hanya setelah server sukses lalu refetch. `Reset` menghapus filter tanpa mengubah database. Jika update parent konflik/cycle, tetap di form dan tampilkan error server.
- **Sumber API & mapping:** existing base `/api/v1/corporate/human-resource/master-data/organization-units`; reuse metadata, summary, list, options, detail/create/update/status/delete yang benar-benar ada. Hierarchy read endpoint dari `FIN-HRD-05-BE-001` dipakai hanya untuk parent/tree context, bukan mengganti CRUD existing.
- **State & feedback:** skeleton/loading pada summary/table/form; empty “Belum ada unit sesuai filter”; 401/403 -> AccessDeniedGate; 400/422 -> field/global validation; 409 -> conflict hierarchy; 5xx/network -> error panel + retry; success -> toast + refetch server; navigasi cepat tidak boleh menampilkan stale detail unit sebelumnya.
- **Responsive & aksesibilitas:** desktop tabel penuh; tablet filter wrap 2 kolom; mobile filter collapsible/stack dan table horizontal-scroll atau card adaptation sesuai pattern existing. Label form terasosiasi input, focus kembali ke field error pertama, tombol touch target memadai, status tidak hanya dibedakan warna.
- **UAT visual:** `UAT-05-01` list + filter data real; `UAT-05-02` create/edit parent valid dan persistence reload; `UAT-05-03` parent self/descendant ditolak dengan pesan; screenshot desktop/tablet/mobile bila browser headed tersedia.

#### Skema Tampilan - FIN-HRD-05-FE-002

- **Route/halaman:** `EXISTING_REUSE + NEW_P2_VIEW_MODE` pada `/hr/master-data/organization`. Jangan buat route baru untuk “tree” jika view mode dapat ditempatkan aman pada halaman existing. Mode list Department/Position harus tetap default/accessible. *(Diamandemen `HRD-DEC-257`: lihat catatan amandemen di awal dokumen.)*
- **Aktor & akses:** role dengan `Organization : Read` dan/atau `OrganizationUnit : Read` dapat melihat hierarchy sesuai kontrak. Tidak ada drag-drop/edit hierarchy pada mode ini. Mutasi unit tetap melalui Unit Kerja sesuai permission.
- **Layout / wireframe:** tambahkan switch mode di area Hero/header, lalu tree read-only dan panel node detail.

```text
+--------------------------------------------------------------------------------+
| Manajemen Organization                                      [Daftar] [Hierarki]|
| Kelola departemen, posisi, dan gambaran hierarki organisasi                    |
+--------------------------------------------------------------------------------+

MODE DAFTAR (EXISTING, JANGAN DIREGRESIKAN)
[Department] [Position]
[Summary Cards]
[Filter] [Table existing] [Pagination]

MODE HIERARKI (NEW P2)
+----------------------------------------------+---------------------------------+
| Pohon Organisasi                             | Detail Node                     |
|                                              |                                 |
| RS / Root                                    | Nama  : Rawat Inap              |
|  ├─ Direktorat Pelayanan                     | Kode  : ORG-...                 |
|  |   ├─ IGD                                  | Tipe  : Installation            |
|  |   └─ Rawat Inap   <selected>              | Parent: Pelayanan               |
|  |       ├─ Ruang Lt.3                       | Child : 6                       |
|  |       └─ Ruang Lt.4                       | Status: Aktif                   |
|  └─ Umum & Keuangan                          | [Lihat Detail Unit]             |
+----------------------------------------------+---------------------------------+
```

- **Komponen & reuse:** reuse `Hero`, base tab/toggle/button jika tersedia, status badge, loading/error patterns. Tree component baru hanya boleh dibuat setelah `base-component-decision-gate`; jika tidak ada base tree, buat feature component lokal scoped, bukan global library baru. Gate = `COMPOSE/NEW_LOCAL_ONLY` bila benar-benar perlu.
- **Data / field & validasi:** node: `id`, `unitCode`, `unitName`, `unitType`, `levelNumber`, `parentOrganizationUnitId`, child count/children, effective dates, isActive. Jangan menggabungkan Department/Position seolah sama dengan OrganizationUnit jika backend tidak memetakan demikian. Node inactive/historical diberi label yang jelas saat filter tanggal menampilkannya.
- **Aksi & interaksi:** switch `Daftar/Hierarki`; expand/collapse node; keyboard Enter/Space untuk node; klik node memperbarui panel detail; `Lihat Detail Unit` navigasi ke detail Unit Kerja. Tidak ada reparent drag/drop, tidak ada direct edit.
- **Sumber API & mapping:** mode Daftar tetap menggunakan Organization API existing. Mode Hierarki memakai endpoint read hierarchy `FIN-HRD-05-BE-001`. Jangan membangun tree dari array dummy atau mengulang fetch per node tanpa batas.
- **State & feedback:** tree skeleton; empty root -> pesan konfigurasi belum tersedia; 403 -> access denied; network/5xx -> retry; broken/cyclic data dari server -> tampilkan error aman dan log diagnostic, jangan infinite recursion; perubahan filter tanggal refetch tree dari server.
- **Responsive & aksesibilitas:** desktop split 60/40 atau proporsional; tablet tree di atas detail; mobile tree full width lalu detail card. Support keyboard expand/collapse, `aria-expanded`, visible focus, text label selain icon.
- **UAT visual:** `UAT-05-04`: mode daftar tetap sama, switch hierarchy berhasil, node real dapat expand/click, detail sesuai API, error state aman, dan viewport desktop/tablet/mobile terbukti.

#### Skema Tampilan - FIN-HRD-05-FE-003

- **Route/halaman:** dua consumer existing: `/hr/master-data/organization` tab Position (`EXISTING_EXTEND`) dan `/hr/master-data/job-family` (`EXISTING_REUSE/FINISH`). Tidak membuat route Jabatan/Rumpun kedua.
- **Aktor & akses:** HR Master Data sesuai permission `Organization` dan `JobFamily`. Read-only role tidak melihat create/update/delete. Link `Lihat Uraian Tugas` hanya tampil bila user boleh membaca Job Description; bila tidak, jangan bocorkan isi capability.
- **Layout / wireframe:** Position mempertahankan list existing; detail ditambah status Uraian Tugas. JobFamily mempertahankan komposisi existing.

```text
POSITION TAB
[Department] [Position*]
[Total Position] [Aktif] [Nonaktif]
[Cari] [Department] [Status]                          [+ Tambah Position]
----------------------------------------------------------------------------
| Kode | Jabatan | Department | Status | Uraian Tugas | Aksi                |
----------------------------------------------------------------------------
| ... real ...                         | Aktif/Belum   | [Detail]            |
----------------------------------------------------------------------------

DETAIL POSITION
+---------------------------------------------------------------+
| Kepala Ruangan                                      [Aktif]   |
| Kode: POS-... | Department: Rawat Inap                        |
| Deskripsi: ...                                                  |
| Uraian Tugas: [Versi aktif v2 / Belum tersedia] [Buka]        |
+---------------------------------------------------------------+

JOB FAMILY (EXISTING)
[Hero] [Summary Cards]
[Cari] [Status] [Periode] [Reset]                [+ Tambah Rumpun]
| Kode | Nama Rumpun | Deskripsi | Status | Aksi |
```

- **Komponen & reuse:** `MasterDataOrganizationView`, `MasterDataJobFamilyView`, `Hero`, `SummaryCards`, `DataTable`, base filter/button/status components existing. Gate = `EXTEND`, tidak membuat halaman paralel.
- **Data / field & validasi:** Position list/detail memakai field real existing; indicator Uraian Tugas berasal API current/effective Job Description, bukan inferensi dari `Description`. JobFamily tetap memakai code/name/description/isActive dan metadata filter existing. Jangan tampilkan JobFamily pada Position jika relasi canonical tidak tersedia.
- **Aksi & interaksi:** create/edit Position dan JobFamily tetap sesuai flow existing; `Buka Uraian Tugas` navigasi ke detail/list Uraian Tugas dengan filter `positionId`; delete JobFamily yang conflict harus menampilkan error server dan tidak menghapus row lokal secara optimistik.
- **Sumber API & mapping:** Organization API `/api/v1/corporate/human-resource/master-data/organization/*`; JobFamily API `/api/v1/corporate/human-resource/master-data/job-families`; Job Description read API `FIN-HRD-05-BE-003` untuk status/link. Gunakan service/hook existing, tambah service baru hanya untuk capability baru.
- **State & feedback:** existing loading/empty/error dipertahankan; indicator Job Description punya skeleton kecil atau status unknown saat request pending; 403 read job description tidak boleh dianggap “Belum ada”; 409 delete conflict ditampilkan; success mutation refetch server.
- **Responsive & aksesibilitas:** tabel existing tetap responsive; pada mobile indicator Uraian Tugas tidak menghilangkan nama jabatan; tombol/link punya accessible name; modal/detail existing focus-trap sesuai base component.
- **UAT visual:** `UAT-05-05` Position existing tetap CRUD/read sesuai permission dan link Uraian Tugas benar; `UAT-05-06` JobFamily list/detail/create/update/status real dan error conflict jujur; screenshot regression sebelum/sesudah bila tooling tersedia.


#### Skema Tampilan - FIN-HRD-05-FE-004

- **Route/halaman:** `NEW_PROPOSED` `/hr/master-data/job-description` sebagai capability Uraian Tugas. Boleh memakai detail route child seperti `/hr/master-data/job-description/[id]` jika pattern Next.js existing mengharuskannya; jangan membuat dua list route untuk capability yang sama.
- **Aktor & akses:** HR Master Data dengan permission read/create/update/activate sesuai resource canonical; read role hanya melihat versi efektif/histori yang diizinkan. Tombol `Aktifkan Versi` tidak boleh tampil bagi role tanpa permission dan backend tetap enforce 403.
- **Layout / wireframe:** list + detail/editor + version history. Gunakan panel/section yang jelas, bukan satu textarea besar.

```text
LIST
+--------------------------------------------------------------------------------+
| Uraian Tugas                                             [+ Buat Uraian]        |
| Kelola tujuan, tugas, tanggung jawab, dan wewenang jabatan                     |
| [Total Jabatan] [Punya Uraian Aktif] [Draft] [Belum Memiliki]                  |
|--------------------------------------------------------------------------------|
| [Cari Jabatan] [Department] [Status Versi] [Tanggal Acuan] [Reset]             |
|--------------------------------------------------------------------------------|
| Jabatan | Department | Versi Aktif | Berlaku Sejak | Status | Aksi              |
| ... data real ...                                                            |
+--------------------------------------------------------------------------------+

DETAIL / EDITOR
+--------------------------------------------------------------------------------+
| Uraian Tugas - Kepala Ruangan                       [Draft / Aktif]             |
| Position: POS-... | Department: Rawat Inap | Version: v2                       |
| Berlaku: [01/01/2027] s.d. [optional]                                          |
|--------------------------------------------------------------------------------|
| TUJUAN JABATAN                                                                 |
| [textarea / rich plain structured text sesuai base component]                  |
|--------------------------------------------------------------------------------|
| TUGAS POKOK                                                   [+ Tambah Butir]  |
| 1. [........................................................] [up][down][delete] |
| 2. [........................................................] [up][down][delete] |
|--------------------------------------------------------------------------------|
| TANGGUNG JAWAB                                                [+ Tambah Butir]  |
| ...                                                                            |
|--------------------------------------------------------------------------------|
| WEWENANG                                                      [+ Tambah Butir]  |
| ...                                                                            |
|--------------------------------------------------------------------------------|
| HUBUNGAN KERJA                                                 [+ Tambah Butir] |
| ...                                                                            |
|--------------------------------------------------------------------------------|
| [Riwayat Versi]                                         [Batal] [Simpan Draft] |
|                                                    [Aktifkan Versi]             |
+--------------------------------------------------------------------------------+

VERSION HISTORY
| Versi | Tanggal Mulai | Tanggal Akhir | Status | Dibuat Oleh | Aksi Lihat |
```

- **Komponen & reuse:** gunakan `Hero`, `SummaryCards`, `DataTable`, `BaseButton`, filter/base form components, status badge, confirmation dialog/drawer yang sudah ada setelah inspection. Ordered section editor boleh menjadi feature component lokal bila base generic tidak cukup; wajib lolos component decision gate. Jangan menambahkan editor WYSIWYG dependency baru tanpa approval.
- **Data / field & validasi:** position wajib; tujuan jabatan wajib dan tidak boleh whitespace; minimal satu Tugas Pokok sebelum aktivasi; responsibilities/authorities/work relations mengikuti requirement service; tanggal mulai wajib; tanggal akhir optional tetapi harus >= mulai; item memiliki sequence stabil. Draft boleh belum lengkap sesuai service, tetapi aktivasi harus menolak incomplete. Position code/name read-only setelah terikat untuk mencegah salah konteks.
- **Aksi & interaksi:** `Buat Uraian` -> pilih Position yang belum memiliki draft aktif atau buat versi baru dari existing; `Simpan Draft`; tambah/hapus/reorder item; `Aktifkan Versi` membuka confirmation berisi position, version, tanggal efektif dan dampak penutupan versi lama; history read-only; edit versi aktif mengikuti rule backend (bila immutable, UI tawarkan `Buat Versi Baru`, bukan edit langsung).
- **Sumber API & mapping:** service baru menuju `/api/v1/corporate/human-resource/master-data/job-descriptions`; list/detail/effective/history/create/update/activate dari `FIN-HRD-05-BE-003`; Position options reuse Organization API existing. Tidak ada localStorage sebagai persistence bisnis.
- **State & feedback:** initial skeleton; empty list; 403; 404 detail; 400/422 validation; 409 overlap/concurrency; 5xx/network; success setelah HTTP sukses; tombol submit disabled selama request; on success refetch detail+history; jika aktivasi gagal, status UI tetap draft dan tidak ada success toast palsu.
- **Responsive & aksesibilitas:** desktop 2-column metadata/section bila aman; tablet/mobile stack satu kolom; action bar tetap terbaca tanpa menutupi field; reorder harus mempunyai alternatif keyboard selain drag-only; field group menggunakan heading/fieldset; error message dihubungkan ke input; confirmation focus-trap.
- **UAT visual:** `UAT-05-07` create draft; `UAT-05-08` activate versi dan tutup versi lama; `UAT-05-09` overlap/409; `UAT-05-10` RBAC 403; `UAT-05-13` reload persistence; `UAT-05-15` server failure no fake success; `UAT-05-16` double-submit/concurrency.

#### Skema Tampilan - FIN-HRD-05-FE-005

- **Route/halaman:** `EXISTING_REUSE` `/hr/workforce-core/manager-assignments`, page source `src/app/hr/workforce-core/manager-assignments/page.jsx`, view `src/components/view/hr/workforce-core/manager-assignments-view.jsx`. Menu 05 hanya menambah entry/shortcut atau context navigation; jangan copy view ke route baru.
- **Aktor & akses:** role dengan `WfpManagerAssignment : ReadAll` dapat melihat. Tidak ada create/update/delete relasi manager dari Menu 05. Jika role tidak memiliki ReadAll, frontend harus menghasilkan access denied dan backend 403.
- **Layout / wireframe:** pertahankan existing list dan filter; bila ditambah breadcrumb/context, tidak mengubah domain mutasi.

```text
+--------------------------------------------------------------------------------+
| Breadcrumb > HRD > Struktur Organisasi & Jabatan > Atasan Langsung             |
| RELASI ATASAN                                                                  |
| Rekapitulasi atasan langsung dan rantai komando persetujuan kerja              |
|--------------------------------------------------------------------------------|
| [Cari pegawai] [Jenis Atasan v] [Status v] [Reset]                             |
|--------------------------------------------------------------------------------|
| Pegawai | Atasan Langsung | Jenis | Utama | Boleh Menyetujui | Berlaku | Status|
| ... real data dari WorkforceCoreCrossEmployeeService ...                       |
|--------------------------------------------------------------------------------|
| [Lihat Penempatan / Proses Perubahan melalui workflow yang sah]                |
+--------------------------------------------------------------------------------+
```

- **Komponen & reuse:** reuse `ManagerAssignmentsView`, base cross-employee list, `FilterSelect`, `StatusBadge`, pagination/search existing. Gate = `REUSE`, bukan clone.
- **Data / field & validasi:** tampilkan field yang sudah ada: employee display/profile code, manager display/profile code, manager type, primary manager, can approve, effective range, active status, dan context department/position jika response existing menyediakannya. Jangan menambahkan data sensitif.
- **Aksi & interaksi:** search/filter/reset/list detail bila existing. Tidak menambahkan tombol direct `Ganti Atasan`. Bila business navigation ke perubahan diperlukan, link menuju workflow Menu07/Menu06 yang benar setelah route terbukti; jika route belum canonical, tampilkan info tanpa membuat URL tebakan.
- **Sumber API & mapping:** `WorkforceCoreCrossEmployeeService.getManagerAssignments` -> `GET /api/v1/corporate/human-resource/workforce-core/manager-assignments`; backend permission `WfpManagerAssignment : ReadAll`.
- **State & feedback:** existing loading/empty/error; 403 bukan empty list; filter change refetch; network error tidak menghapus last good data lalu mengklaim kosong; retry tersedia menurut base pattern.
- **Responsive & aksesibilitas:** table horizontal scroll/card adaptation sesuai base component; boolean “Utama/Boleh Menyetujui” memiliki text label, bukan icon tanpa nama; filter dapat diakses keyboard.
- **UAT visual:** `UAT-05-11` manager list real dan filter; `UAT-05-12` tidak ada direct mutation/bypass dan role denied aman; `UAT-05-13` navigasi/reload tidak mengubah data.

#### Skema Tampilan - FIN-HRD-05-FE-006

- **Route/halaman:** seluruh consumer Menu05: `/hr/master-data/organization-unit`, `/hr/master-data/organization`, `/hr/master-data/job-family`, `/hr/master-data/job-description`, `/hr/workforce-core/manager-assignments`. `NO_NEW_VISUAL_LAYOUT` untuk task UAT ini; layout yang diuji adalah layout pada `FIN-HRD-05-FE-001..005`.
- **Aktor & akses:** minimal HR Admin/Master Data role, read-only role yang sah, dan role tanpa permission. Bila akun test untuk semua role tidak tersedia, jangan fabricate hasil; tandai test terkait `NOT RUN`.
- **Layout / wireframe:** task ini menguji konsistensi lintas layar, bukan menambah visual baru.

```text
MENU 05 NAVIGATION FLOW
Unit Kerja ---------> detail/edit existing
   |                     |
   +----> Struktur Organisasi --[Hierarki]--> Detail Unit
   |
Jabatan ------------> Uraian Tugas --------> Version History
Rumpun Jabatan ------> existing master
Atasan Langsung -----> existing Workforce Core read view

STATE MATRIX PER SCREEN
Loading -> Content / Empty
        -> 403 Access Denied
        -> 400/422 Validation
        -> 409 Conflict
        -> 5xx/Network + Retry
Success mutation -> Refetch -> Reload must match server
```

- **Komponen & reuse:** tidak membuat komponen baru kecuali fixture/test harness yang diizinkan policy. Validasi bahwa setiap screen memakai base components dan design tokens sesuai `rules/frontend/`.
- **Data / field & validasi:** cek semua field/kolom yang ditetapkan pada skema FE-001..005; pastikan tidak ada lorem ipsum, sample person, generated fake row, fallback mock atau hardcoded status yang mengganti response backend.
- **Aksi & interaksi:** test search/filter/reset/create/update/draft/activate/link/navigation/retry/permission denial; test double-click submit; test back-forward browser dan perpindahan record cepat untuk stale data.
- **Sumber API & mapping:** seluruh service harus menunjuk endpoint real branch `MHamzah`; verifikasi request/response mapping, pagination, date format, enum/status, dan error mapping. API unavailable -> error state, bukan dummy fallback.
- **State & feedback:** wajib menguji loading, empty, 401/403, 400/422, 409, 5xx/network, success, reload persistence, stale cache, double submit. Sukses hanya boleh setelah server sukses.
- **Responsive & aksesibilitas:** headed browser desktop, tablet, mobile; keyboard-only untuk navigation/form dasar; focus visible; modal focus trap; label/input/error association; status tidak color-only; horizontal overflow tidak memotong aksi penting.
- **UAT visual:** seluruh `UAT-05-01` sampai `UAT-05-16`; kumpulkan screenshot/bukti hanya jika test benar-benar dijalankan. Jangan menaikkan status menjadi PASS berdasarkan source inspection saja.

## 2.6 Integrasi lintas Submenu dan dampak lintas modul

1. `MstOrganizationUnit` tetap dipakai oleh organisasi, assignment, authorization projection, workforce planning, dan workflow routing. Finishing hierarchy tidak boleh mengubah identity unit secara destruktif.
2. `MstPosition` tetap dipakai assignment, competency, workflow/approval routing, workforce planning dan domain lain. Uraian Tugas harus menjadi child/reference capability, bukan mengganti Position.
3. `MstJobFamily` tetap existing master; hubungan ke assignment tidak boleh dipindahkan tanpa keputusan arsitektur terpisah.
4. `WfpManagerAssignment` memengaruhi approval routing dan Employee360. Menu05 tidak boleh membuat manager relation bayangan yang berbeda dari workflow.
5. Perubahan unit/jabatan/atasan pegawai tetap melalui flow penempatan/SK canonical. Jangan membuka mutation langsung dari Menu05.
6. Jika hierarchy view memperlihatkan unit historical/inactive, tanggal acuan dan status harus eksplisit agar user tidak salah menganggap node historical sebagai struktur aktif sekarang.
7. Jika job description dipakai kelak untuk recruitment/performance/competency, integrasi itu **out of scope** finishing ini kecuali sudah ada contract existing yang secara langsung mewajibkan read-only reference; jangan menambah side effect lintas modul tanpa task baru.

## 2.7 UAT Menu 05

| UAT ID | Prioritas | Skenario | Hasil yang wajib |
|---|---|---|---|
| `UAT-05-01` | P1 | Buka Unit Kerja, search/filter | Data API real, pagination/summary konsisten |
| `UAT-05-02` | P1 | Create/update Unit dengan parent valid | Tersimpan server dan tetap ada setelah reload |
| `UAT-05-03` | P0 | Set parent ke diri sendiri/descendant | Ditolak backend; UI tidak fake success |
| `UAT-05-04` | P2 | Switch Organization Daftar -> Hierarki | Mode existing tidak regresi; tree real; node detail benar |
| `UAT-05-05` | P1 | Position detail + buka Uraian Tugas | Position source existing; link/filter position benar |
| `UAT-05-06` | P1 | JobFamily CRUD/status dan error conflict | Data real; conflict tidak diubah menjadi sukses |
| `UAT-05-07` | P0 | Buat Uraian Tugas draft | Header + sections tersimpan real |
| `UAT-05-08` | P0 | Aktifkan versi baru | Maksimal satu versi efektif pada tanggal yang sama; history preserved |
| `UAT-05-09` | P0 | Buat periode overlap | HTTP 409 atau kontrak conflict setara; UI tetap aman |
| `UAT-05-10` | P0 | Role tanpa permission akses/mutasi Job Description | 403 backend + UI AccessDenied; tidak ada data leak |
| `UAT-05-11` | P1 | Buka/filter Atasan Langsung | Data real dari `WfpManagerAssignment` |
| `UAT-05-12` | P0 | Coba ubah manager dari Menu05 | Tidak ada direct mutation path; user diarahkan ke workflow sah |
| `UAT-05-13` | P1 | Reload seluruh screen setelah mutation | State sama dengan server; no local-only persistence |
| `UAT-05-14` | P1 | Desktop/tablet/mobile + keyboard | Tidak overflow kritis; focus/label/status aksesibel |
| `UAT-05-15` | P0 | Simulasikan API 5xx/network fail | Error jujur; tidak ada fallback dummy/fake success |
| `UAT-05-16` | P0 | Double submit/concurrent activate versi | Tidak membuat dua versi efektif/duplicate write |

---

# 3. Context

## 3.1 Repository, branch, governance, dan snapshot

Snapshot yang telah diperiksa untuk instruksi ini:

- FE `DevBenari/QuilvianSystemFrontendDev` @ `HamzahV2`: `4672040faeb9742d58f9a00f4dccb1d5eaf1e7f3`.
- BE `DevBenari/NewQuilvianSystemBackend` @ `MHamzah`: `bdae40837d8ec00f0c23471e0d971dd98e009f42`.
- Skills `DevBenari/QuilvianEngineeringSkills` @ `main`: `201af64552b27873fd9af83c6722cf11b441bb1e`.

Governance source yang telah dibaca:

- FE `AGENTS.md` blob `accf4c6ec0c09334a27a20cda975363f77c8b961`.
- FE `CLAUDE.md` blob `67e82669de9f584b1fe64573f60eaa9820b207c7`.
- BE `AGENTS.md` blob `4eb563b6807558557d9eeb89175d26b8577fe880`.
- BE `CLAUDE.md` blob `08be6bd86c87e15952155a44bf8bd4ad5f42a2fe`.
- Skills `agents/rules/GLOBAL_RULES.md` blob `50d1503907dafb09c12c25784b7cd05c1e587d6b`.

Sebelum menyentuh source pada runtime Agent, **baca ulang** file governance di working tree dan rules logical path yang disebut AGENTS. Jika HEAD berubah dari snapshot di atas, lakukan source re-check untuk area yang berubah dan laporkan delta; jangan bekerja berdasarkan SHA lama secara membabi buta.

## 3.2 Source evidence Menu 05

| Capability | Evidence | Blob/source snapshot | Status untuk instruksi |
|---|---|---|---|
| Unit Kerja model | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs` | `17452eaec0bc58da920b7cedb89d533fc44f1709` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Unit Kerja API | `.../Controllers/OrganizationUnitController.cs`, route `api/v1/.../organization-units`, permission `OrganizationUnit` | `784ec143223877281890aeea4a75f4aed705068c` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Department/Position API | `.../Controllers/OrganizationController.cs`, route `api/v1/.../organization` | `526a504a42ed74f8fb86d14ca48ee7aa183c3ad9` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Position model | `.../Models/MstPosition.cs` | `35cbb7ef81c1ab7fd5f7722ae8c7320b8b821343` | `IMPLEMENTED, NOT VERIFIED` runtime |
| JobFamily model/API | `MstJobFamily.cs`; `JobFamilyController.cs` route `.../job-families` | model `854f0d39811b0e953ffb874799677e0d9aff8aa5`; controller `418d1bb0028ea3aa74fb8aff0d25cfe4d0ca92ad` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Manager model | `WorkforceCore/Models/WfpManagerAssignment.cs` | `e839be370681fd75f6cfc21fe37b73153acc31e0` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Manager cross-employee API | `WorkforceCoreCrossEmployeeController.cs` | `ff4ae4936f435a6cba0c46a384d297a886552de0` | `IMPLEMENTED, NOT VERIFIED` runtime |
| Manager query persistence | `WorkforceCoreCrossEmployeeQueryService.cs` | `3d74a32ffd6f808d879d7bbe6d67e11b020b698f` | EF real query source seen |
| FE sidebar | `src/utils/menu-sidebar/menu-items.jsx` | `0a46dc47bbefa6d36e9cf83eb16736ca8872893c` | menu capabilities tersebar |
| FE Organization | `src/app/hr/master-data/organization/page.jsx`; `master-data-organization-view.jsx` | `3ba669...`; `78cc6a...` | existing Department/Position list |
| FE Organization Unit | `src/app/hr/master-data/organization-unit/page.jsx`; view | `4936f8...`; `3aeaa4...` | existing real page |
| FE JobFamily | `src/app/hr/master-data/job-family/page.jsx`; view | `20d143...`; `701c90...` | existing real page |
| FE Manager Assignments | `src/app/hr/workforce-core/manager-assignments/page.jsx`; view | `cfa955...`; `7a7c36...` | existing real page |
| FE Workforce Core service | `src/lib/services/hr/workforce-core-cross-employee-service.js` | `bb369d9105f00db29b388fe55d8da6ae2ae72f8a` | calls real `manager-assignments` API |
| Dedicated Uraian Tugas | pencarian source saat analisis tidak menemukan capability khusus yang memadai | `NOT VERIFIED / MISSING dedicated capability` | ADD |

## 3.3 Roadmap/canonical evidence yang relevan

- `backend-roadmap.md` blob `6dfac4e47e3d92308a0cab60f4b57248cbcd438b`.
- `frontend-roadmap.md` blob `a08f951fe772a401a2b0ed35406933ec668bef43`.
- `requirement-traceability.md` blob `0199d83f93068fa7ed5bf7422803d6ced776b52a`.
- `01-existing-capability-map.md` blob `cc42d41762d2f6638f798f0ddfd682c6305480cb`.
- `00-interview-decisions.md` blob `73dd1f4dcc78c67e4e14f5a1fe05cec9524326a7`.

Baseline roadmap yang terbukti relevan dan **tidak boleh di-rename**:

- `BE-HRD-003` — endpoint daftar lintas pegawai untuk organization/position/manager assignments.
- `BE-HRD-023` — approval/audit fields pada Wfp assignments.
- `BE-HRD-026` — Employee360 memakai `WfpManagerAssignment` untuk hierarki manajerial.
- `BE-HRD-029` — traversal ancestor `MstOrganizationUnit.ParentOrganizationUnitId`, bukti bahwa hierarchy unit existing dipakai untuk routing.
- `FE-HRD-002` — enam route Workforce Core cross-employee termasuk `manager-assignments`.

Tidak ditemukan folder finishing Menu05 pada `docs/module-blueprints/human-resource/docs/update` pada snapshot ini; karena itu FIN Menu05 dimulai `001` secara mandiri. Bila saat Agent menjalankan instruksi ini sudah ada dokumen/FIN Menu05 baru, **jangan menimpa atau menomori ulang**; reconcile dengan register terbaru dan pertahankan ID yang telah sah.

## 3.4 Matriks status existing, repair, add

| Target Submenu | Mapping existing | Klasifikasi | Perlakuan |
|---|---|---|---|
| Unit Kerja | `MstOrganizationUnit`, OrganizationUnit API, FE organization-unit | `IMPLEMENTED, NOT VERIFIED` | REUSE + hierarchy hardening/finishing |
| Struktur Organisasi | Organization Department/Position + OrganizationUnit parent-child | `PARTIAL` | pertahankan list; tambah tree P2 |
| Jabatan | `MstPosition` via Organization API | `IMPLEMENTED, NOT VERIFIED` | REUSE + link Uraian Tugas |
| Rumpun Jabatan | `MstJobFamily`, JobFamily API/FE | `IMPLEMENTED, NOT VERIFIED` | REUSE/FINISH, no duplicate |
| Uraian Tugas | dedicated capability belum terbukti | `MISSING / NOT VERIFIED` | ADD versioned/effective-dated capability |
| Atasan Langsung | `WfpManagerAssignment` + cross employee API + FE | `IMPLEMENTED, NOT VERIFIED` / Menu05 integration `PARTIAL` | REUSE read model; no direct mutation |

## 3.5 Kontrak keamanan dan source-of-truth

1. Authorization frontend hanya convenience; backend `[AccessPermission]`/policy canonical adalah enforcement.
2. `WfpManagerAssignment` adalah satu-satunya source manager aktual pada scope ini.
3. `MstOrganizationUnit`, `MstPosition`, `MstJobFamily` tidak digandakan.
4. `Uraian Tugas` baru harus punya foreign key/reference ke Position, bukan menyimpan copy code/name sebagai source-of-truth.
5. History/version tidak boleh dihapus atau ditimpa tanpa audit rules.
6. No fake success. HTTP fail harus menghasilkan fail UI.
7. No dummy data untuk kondisi backend unavailable.
8. Jangan memindahkan pegawai atau mengganti atasan melalui Menu05.

---

# 4. Reasoning / Execution Workflow

## 4.1 Preflight governance singkat

1. Pastikan working tree berada pada repository dan branch target yang benar; baca HEAD SHA aktual dan bandingkan dengan snapshot dokumen.
2. Baca FE/BE `AGENTS.md`, `CLAUDE.md`, serta `rules/GLOBAL_RULES.md`.
3. Untuk backend, baca `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, dan output/report rules yang berlaku.
4. Untuk frontend, baca `rules/frontend/frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `page-composition-patterns.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `test-policy.md`, dan output/report rules.
5. Periksa roadmap/traceability/report yang paling baru untuk memastikan FIN Menu05 belum collision. Jika ada FIN baru yang diterbitkan setelah snapshot, pertahankan ID existing dan reconcile; jangan overwrite.
6. Requirement Menu05 pada dokumen ini sudah dikunci. Jangan menjalankan interview ulang. Bila runtime skill secara mekanis membutuhkan completeness gate, berikan dokumen ini sebagai requirement locked dan lanjutkan; jangan meminta user mengulangi keputusan yang sudah tertulis.

## 4.2 Fase sinkronisasi canonical bila diperlukan

Jika keputusan Menu05 belum tercermin pada blueprint/canonical dan governance mewajibkan sinkronisasi sebelum source change:

- gunakan `MODULE BLUEPRINT MODE` hanya pada path dokumentasi yang diizinkan;
- catat DEC-M05-01..11 sebagai keputusan owner yang berasal dari instruksi ini, tanpa mengubah maknanya;
- registrasikan FIN register sebagai sub-register finishing hanya jika aturan output/roadmap mengizinkan;
- task reguler `BE-HRD-*`/`FE-HRD-*` tidak boleh di-rename;
- setelah sinkronisasi kontrak, **lanjutkan implementasi source**. Jangan berhenti dengan dokumen saja.

Jika mode dokumentasi tidak diizinkan, tandai `CANONICAL_SYNC_PENDING` tetapi lanjutkan source task yang contract-nya sudah cukup jelas dan aman; tahan hanya bagian yang benar-benar memerlukan contract baru.

## 4.3 Fase Backend - kerjakan dahulu

Urutan:

1. Invoke `quilvian-engineering-skills:build-module-backend`.
2. Kerjakan `FIN-HRD-05-BE-001` hierarchy guard/read model dengan reuse source existing.
3. Kerjakan desain/persistence `FIN-HRD-05-BE-002` sesuai prefix registry dan database rules.
4. Kerjakan service/API/RBAC `FIN-HRD-05-BE-003`.
5. Kerjakan integration/read guard/tests `FIN-HRD-05-BE-004`.
6. Jalankan build/QBE/unit/integration tests yang diwajibkan rules dan diizinkan environment.
7. Untuk migration: **jangan create/run otomatis** jika governance meminta izin eksplisit terpisah. Laporkan `MIGRATION_PERMISSION_REQUIRED` dan lanjutkan pekerjaan non-migration yang aman. Runtime acceptance persistence ditandai BLOCKED/NOT RUN sampai database schema tersedia.
8. Catat setiap endpoint final, DTO final, permission final, schema/model name final, dan perbedaan dari route usulan dokumen ini.

Backend acceptance minimum sebelum frontend Uraian Tugas dianggap ready:

- API nyata tersedia/terkompilasi.
- DTO request/response stabil.
- RBAC enforced.
- Version/effective overlap guard tersedia.
- Tidak ada local/in-memory persistence.
- Failure mapping 400/403/404/409/5xx jelas.
- Hierarchy endpoint tidak recursive infinite pada data invalid.

## 4.4 Fase Frontend - setelah backend contract siap

1. Invoke `quilvian-engineering-skills:build-module-frontend`.
2. Implement `FIN-HRD-05-FE-001` pada route existing Unit Kerja.
3. Implement `FIN-HRD-05-FE-003` untuk Position/JobFamily finishing yang tidak menunggu UI Uraian Tugas kompleks; link Uraian Tugas diaktifkan setelah API tersedia.
4. Implement `FIN-HRD-05-FE-004` Uraian Tugas lengkap dan real API.
5. Implement `FIN-HRD-05-FE-005` dengan reuse manager assignment view; jangan clone.
6. Implement `FIN-HRD-05-FE-002` hierarchy P2 setelah P0/P1 stabil.
7. Jalankan `FIN-HRD-05-FE-006` lintas screen.
8. Gunakan base components. New local feature component hanya setelah decision gate dan hanya bila tidak ada base component yang memadai.
9. Dilarang menambahkan mock adapter/fallback demo ketika backend error.

## 4.5 Validation dan UAT

- Backend: build/lint/QBE/test sesuai rules. Catat command dan hasil aktual; warning tidak disembunyikan.
- Frontend: lint/build/test sesuai policy; browser UAT menggunakan headed browser jika environment tersedia.
- UAT harus memakai data test yang aman dan tidak memuat data pasien/pegawai nyata sensitif.
- Untuk setiap test, status hanya `PASS`, `FAIL`, `BLOCKED`, atau `NOT RUN` berdasarkan eksekusi aktual.
- Fix hasil FAIL yang berada dalam scope dan retest sebelum menyebut selesai.
- `PASS` source/build tidak sama dengan `UAT ACCEPTED`.

## 4.6 Evidence dan report

Untuk setiap FIN task yang benar-benar dikerjakan:

1. catat file/symbol berubah;
2. catat baseline roadmap lama;
3. catat command/test result;
4. catat UAT yang terkait;
5. catat status jujur;
6. tulis report tracked hanya bila `AGENTS.md`/output rules mengizinkan ID/path FIN tersebut;
7. jika registrasi report FIN belum diizinkan, gunakan `REPORT_REGISTRATION_BLOCKED` dan jangan membuat laporan palsu pada path random.

---

# 5. Stop conditions

Hentikan **hanya bagian yang terdampak** dan laporkan blocker spesifik bila salah satu kondisi berikut terjadi. Lanjutkan task independen yang tidak terblokir.

1. Runtime engineering skill tidak tersedia -> laporkan persis `BLOCKED — Quilvian runtime skill unavailable` untuk layer tersebut.
2. `AGENTS.md`, Backend Engineering Contract, Prefix Registry, atau rules canonical tidak dapat dibaca -> `BLOCKED — canonical governance unavailable`.
3. Branch/HEAD bukan target dan perubahan source yang terbaca berbeda material dari snapshot -> lakukan re-audit area berubah sebelum menulis; jangan meneruskan asumsi lama.
4. Sudah ada FIN Menu05 terdaftar dengan ID/semantik berbeda -> jangan overwrite; reconcile dan minta owner hanya bila collision tidak dapat diselesaikan dari canonical register.
5. New Uraian Tugas membutuhkan migration creation/execution sementara izin terpisah belum diberikan -> `MIGRATION_PERMISSION_REQUIRED`; jangan menjalankan migration otomatis.
6. Prefix/naming untuk entity baru belum dapat ditentukan dari Registry/Contract -> tahan creation entity dan laporkan `BLOCKED - OWNERSHIP_PREFIX_UNRESOLVED`; jangan mengarang nama schema.
7. Relasi JobFamily/JobLevel ke master Position tidak didukung contract -> jangan membuat FK baru; tetap tampilkan hanya data existing.
8. Route workflow perubahan Atasan/penempatan belum terbukti -> jangan membuat URL tebakan; pertahankan Menu05 read-only.
9. Browser/account test tidak tersedia -> UAT terkait `NOT RUN`; jangan mengklaim PASS.
10. API backend untuk Uraian Tugas belum siap -> frontend tidak boleh membuat mock/fake success; screen boleh disusun setelah contract stabil tetapi integration state tetap `BLOCKED/NOT VERIFIED`.

Jangan berhenti karena pertanyaan kosmetik minor yang dapat mengikuti design system existing. Gunakan engineering discretion untuk spacing, icon existing, microcopy, dan komposisi minor selama tidak mengubah keputusan bisnis.

---

# 6. Output

Agent harus menghasilkan hasil kerja nyata, bukan hanya analisis. Output akhir eksekusi minimal memuat:

1. **Ringkasan implementasi** per `FIN-HRD-05-BE-*` dan `FIN-HRD-05-FE-*` dengan status jujur.
2. **Daftar file source berubah** dan alasan perubahan.
3. **Kontrak API final** Uraian Tugas dan hierarchy bila endpoint baru dibuat: method, route, permission, request/response ringkas, error 400/403/404/409.
4. **Data model final** Uraian Tugas, termasuk version/effective invariant dan foreign key ke Position; sebutkan migration status secara jujur.
5. **Skema UI final** yang benar-benar diimplementasikan dan delta terhadap skema dokumen ini jika ada perubahan akibat source/governance.
6. **Hasil validation** build/lint/QBE/unit/integration/frontend tests dengan command dan PASS/FAIL/NOT RUN aktual.
7. **Hasil UAT-05-01..16** dengan status dan evidence; screenshot hanya jika benar-benar dijalankan.
8. **Daftar blocker/pending** yang masih memerlukan izin migration, owner decision, environment, atau akun test.
9. **Traceability** ke baseline roadmap existing (`BE-HRD-003`, `BE-HRD-023`, `BE-HRD-026`, `BE-HRD-029`, `FE-HRD-002`) tanpa mengganti ID task lama.
10. **Pernyataan eksplisit anti-duplikasi:** konfirmasi bahwa tidak dibuat master baru yang menduplikasi `MstOrganizationUnit`, `MstPosition`, `MstJobFamily`, atau `WfpManagerAssignment`.

## Definition of Done Menu 05

Menu 05 baru boleh disebut selesai apabila:

- Unit Kerja menggunakan hierarchy existing dengan invariant aman.
- Struktur Organisasi existing tidak regresi; enhancement tree P2 selesai atau secara eksplisit dipisahkan sebagai P2 pending tanpa menahan P0/P1.
- Jabatan tetap memakai `MstPosition` dan mempunyai navigasi Uraian Tugas yang real.
- Rumpun Jabatan tetap memakai `MstJobFamily` dan tidak memiliki duplicate master/fake data.
- Uraian Tugas mempunyai persistence real, versi/effective dating, RBAC, history, dan UI real API.
- Atasan Langsung memakai `WfpManagerAssignment` real dan tidak mempunyai bypass mutation dari Menu05.
- Tidak ada dummy/mock/hardcode/fake success production path.
- Build/test yang diwajibkan selesai atau blocker dilaporkan jujur.
- UAT runtime tidak diklaim PASS bila tidak dijalankan.
- FIN register/report/roadmap status tidak dipalsukan sebagai registered/verified sebelum evidence tersedia.

**Instruksi terakhir kepada Agent:** setelah preflight governance, **KERJAKAN source secara langsung sesuai urutan Backend -> Frontend -> UAT**. Jangan kembali hanya dengan “hasil audit”, “saran”, atau “rencana implementasi” karena requirement dan planning Menu 05 sudah disetujui owner pada dokumen ini.
