# Kontrak Engineering Backend Quilvian

Dokumen ini adalah kontrak canonical untuk `NEW CODE`, `TOUCHED LEGACY`, dan `LEGACY MIGRATION`. Kontrak ini mengikat developer, Codex, Skills, dan checker di masa depan. Istilah normatif MUST, MUST NOT, SHOULD, dan MAY berlaku sebagaimana didefinisikan di bawah. Kontrak ini sendiri tidak memberi wewenang implementasi, migration, maupun deployment.

## Wewenang, keberlakuan, dan ratchet

`NEW CODE` MUST patuh. `TOUCHED LEGACY` SHOULD memperbaiki pelanggaran yang berlaku hanya bila aman, terbatas cakupannya, dan diberi wewenang. `UNTOUCHED LEGACY` MUST NOT memicu penulisan ulang massal. `LEGACY MIGRATION` adalah kampanye terbatas yang dinyatakan eksplisit. [Module Ownership & Prefix Registry](MODULE_OWNERSHIP_PREFIX_REGISTRY.md) yang sudah disetujui adalah wewenang untuk penamaan operasional.

## Aturan canonical

| ID | Ketentuan | Kelayakan otomasi |
|---|---|---|
| QBE-ENT-001 | MUST / NEW CODE: entity domain yang dipersistensi mewarisi `IdentityModel`. | AUTOMATABLE |
| QBE-ENT-002 | SHOULD / NEW CODE: Guid, field domain, navigation, dan nullability mengikuti semantik domain. | PARTIALLY_AUTOMATABLE |
| QBE-ENT-003 | MUST NOT / NEW CODE: menambah field persisted yang murni kebutuhan presentasi. | REVIEW_ONLY |
| QBE-NAM-001 | MUST NOT / NEW CODE: memakai `Trx*` untuk entity, file, configuration, atau DbSet operasional. | AUTOMATABLE |
| QBE-NAM-002 | MUST / NEW CODE: memakai prefix registry yang disetujui; prefix yang tidak dikenal memblokir pembuatan. | PARTIALLY_AUTOMATABLE |
| QBE-NAM-003 | MUST / LEGACY MIGRATION: menormalkan source dan tabel fisik secara bersamaan. | REVIEW_ONLY |
| QBE-NAM-004 | MUST NOT / NEW CODE: menciptakan atau memakai prefix baru tanpa entri registry yang disetujui, termasuk menyimpulkannya dari nama folder, nama task, atau nama layar. | REVIEW_ONLY |
| QBE-CFG-001 | MUST / NEW CODE: menyediakan `IEntityTypeConfiguration<T>` beserta mapping, key, index, dan relasi. | AUTOMATABLE |
| QBE-CFG-002 | SHOULD / TOUCHED LEGACY: memperbaiki configuration secara aman dalam cakupan. | PARTIALLY_AUTOMATABLE |
| QBE-MOD-001 | MUST / NEW CODE: menempatkan capability di bawah Area/Module/Submodule pemiliknya. | PARTIALLY_AUTOMATABLE |
| QBE-MOD-002 | MUST / NEW CODE: modul operasional punya entri registry yang disetujui sebelum entity pertama dibuat. | PARTIALLY_AUTOMATABLE |
| QBE-MOD-003 | MUST / NEW CODE: folder Area/Module/Submodule baru — atau yang sudah ada namun belum terdaftar — yang akan memuat model persisted wajib didaftarkan lebih dulu di registry (Area, Module/pemilik, Category, Prefix, Lifecycle) sebelum file model pertama dibuat. | PARTIALLY_AUTOMATABLE |
| QBE-SVC-001 | MUST / NEW CODE: Module Service memiliki CRUD/orkestrasi domain; controller tidak mengakses context secara langsung. | PARTIALLY_AUTOMATABLE |
| QBE-API-001 | MUST / NEW CODE: memakai boundary API, response, status, dan validasi yang sudah mapan. | PARTIALLY_AUTOMATABLE |
| QBE-PERM-001 | MUST / NEW CODE: memakai metadata Access yang berlaku. | PARTIALLY_AUTOMATABLE |
| QBE-LOG-001 | MUST / NEW CODE: menghasilkan log/event perubahan state yang menyertakan aktor. | REVIEW_ONLY |
| QBE-CODE-001 | MUST / NEW CODE: Service memiliki kebutuhan dan format kode; alokasinya deterministik dan aman di database. | REVIEW_ONLY |
| QBE-CODE-002 | MUST NOT / NEW CODE: controller membuat atau mengalokasikan nomor bisnis. | AUTOMATABLE |
| QBE-CODE-003 | MUST NOT / NEW CODE: memakai Count+1, Max/Last+1 tanpa proteksi, counter statis/lokal, atau lock process-local sebagai satu-satunya alokator. | AUTOMATABLE |
| QBE-CODE-004 | MUST / NEW CODE: kode bisnis unik memiliki unique constraint/index database sesuai scope-nya. | PARTIALLY_AUTOMATABLE |
| QBE-CODE-005 | MUST / NEW CODE: modul memiliki format, prefix, reset, dan scope kodenya sendiri. | REVIEW_ONLY |
| QBE-CODE-006 | MUST / NEW CODE: provider bersama mendukung alokasi atomik ber-scope yang durabel beserta observability retry. | REVIEW_ONLY |
| QBE-VAL-001 | MUST / NEW CODE: memvalidasi request dan invarian bisnis. | PARTIALLY_AUTOMATABLE |
| QBE-TXN-001 | SHOULD / NEW CODE: mentransaksikan konsistensi lintas record/workflow. | REVIEW_ONLY |
| QBE-DTO-001 | MUST / NEW CODE: tidak mengekspos entity EF sebagai kontrak API. | PARTIALLY_AUTOMATABLE |
| QBE-ENUM-001 | SHOULD / NEW CODE: menjaga enum yang dibutuhkan tetap dimiliki modul. | REVIEW_ONLY |
| QBE-PAGE-001 | SHOULD / NEW CODE: capability list memakai paging/search/sort yang sudah mapan. | PARTIALLY_AUTOMATABLE |
| QBE-OPT-001 | SHOULD / NEW CODE: menyediakan options/metadata hanya bila memang dikonsumsi. | REVIEW_ONLY |
| QBE-DEL-001 | MUST / NEW CODE: menghormati lifecycle delete/cancel beserta audit aktornya. | REVIEW_ONLY |
| QBE-DB-001 | MUST / LEGACY MIGRATION: mengaudit dependensi fisik sebelum rename. | REVIEW_ONLY |
| QBE-DB-002 | MUST NOT / LEGACY MIGRATION: memakai DROP+CREATE yang destruktif bila rename yang menjaga data masih aman. | PARTIALLY_AUTOMATABLE |
| QBE-AUD-001 | MUST / ALL: menjaga audit database tetap terpisah dari application logging. | PARTIALLY_AUTOMATABLE |

## Entity, penamaan, dan PostgreSQL

`SortOrder` presentasi yang dipersistensi secara generik dilarang untuk kode baru; urutan bisnis yang sesungguhnya memakai field semantik. `SortOrder` pada DTO, form, permission, dan UI tetap sah. `Mst*` adalah master/reference dan tidak deprecated.

Nama entity, file, configuration, DbSet, dan tabel adalah satu paket. Untuk `LabOrder`: `LabOrder.cs`, `LabOrderConfiguration`, `LabOrders`, dan `public."LabOrder"`. DbSet adalah bentuk jamak dari nama entity; nama tabel tunggal PascalCase sama persis dengan nama entity; schema `public`.

Nama entity operasional baru berbentuk `<PrefixPemilikDisetujui><KonsepBisnis>` tanpa pengulangan nama pemilik, misalnya `RegPatientEncounter`, `EmgVisit`, `WflInstance`, `LabOrder`.

## Pendaftaran modul dan prefix sebelum model dibuat

Sebelum membuat model persisted pertama pada sebuah folder Area/Module/Submodule — baik folder yang benar-benar baru maupun folder yang sudah ada tetapi belum tercatat — pemilik dan prefixnya MUST sudah terdaftar di [MODULE_OWNERSHIP_PREFIX_REGISTRY.md](MODULE_OWNERSHIP_PREFIX_REGISTRY.md).

Urutannya mengikat: **daftarkan dulu, baru buat model.**

1. Tentukan Area, Module/pemilik, dan Submodule tempat capability itu tinggal (QBE-MOD-001).
2. Periksa registry. Bila pemiliknya sudah terdaftar, pakai prefix yang tercatat (QBE-NAM-002).
3. Bila belum terdaftar, ajukan entri registry berisi Area, Module/pemilik, Category, Prefix, dan Lifecycle beserta kepanjangan prefixnya — contoh `Wfp` = *Workforce Profile*.
4. Setelah entri disetujui dan tercatat, barulah file model pertama dibuat memakai prefix tersebut.

Tanpa entri yang disetujui, pembuatan entity operasional berstatus `BLOCKED` berdasarkan QBE-MOD-002 dan QBE-MOD-003. Prefix MUST NOT disimpulkan dari nama folder, nama task ClickUp, atau nama layar (QBE-NAM-004). Prosedur lengkap beserta contoh terisi ada pada bagian *Prosedur pendaftaran modul/prefix baru* di registry.

## Boundary API/service dan nomor bisnis

Alur baru adalah Controller → Module Service → DbContext/infrastruktur bersama/integrasi eksternal. Alur kode adalah Module Service → formatter/definisi milik modul → provider number-series PostgreSQL bersama yang atomik → PostgreSQL. Provider mendukung SequenceKey, ScopeKey, alokasi atomik, dan scope NEVER/YEARLY/MONTHLY/DAILY/domain. Default-nya unik dan monotonik per scope; gaplessness yang ketat memerlukan aturan legal/domain yang disetujui terpisah. Unique constraint database wajib bila sebuah kode bersifat unik.

## Normalisasi legacy dan pengecualian

Normalisasi legacy `Trx*` belum selesai sampai class, file, configuration, DbSet, seluruh referensi, dan tabel `public."Trx..."` dinormalkan bersama-sama. Setiap batch mengaudit dampak FK, index, constraint, raw SQL, migration, dan consumer, lalu memverifikasi jumlah baris, integritas, build, smoke API/frontend, serta rollback. Pengecualian MUST menyebutkan QBE ID, alasan, dan cakupannya, serta tidak boleh diam-diam menjadi konvensi baru.
