# Rekomendasi Skill Suite Pengembangan Modul — Dari Kebutuhan Bisnis sampai Siap Digunakan

| | |
|---|---|
| Tanggal | 2026-08-12 |
| Branch | `MHamzah` |
| Status | **Spesifikasi/rekomendasi** — tujuh skill belum dibuat atau dipasang melalui dokumen ini |
| Dokumen canonical | `docs/agency/rekomendasi-skills-pengembangan-modul.md` |
| Pemicu | Kebutuhan pola kerja yang dapat dipakai untuk IGD, pengkajian rawat inap, dan modul-modul lain |
| Model penempatan | **Hybrid 5 shared + 2 repo-local; secara fisik 6 backend + 1 frontend** |
| Cakupan perubahan | Dokumentasi saja |
| Database/runtime migration | Tidak ada |
| Breaking change | Tidak ada |

## 1. Tujuan

Dokumen ini mendefinisikan satu rangkaian skill generik untuk merancang dan mengembangkan
modul Quilvian dari awal. Rangkaian ini tidak boleh terkunci pada modul IGD. Pola yang sama
harus dapat dipakai untuk pengkajian rawat inap, ICU, kamar operasi, farmasi, laboratorium,
radiologi, inventori, SDM, dan modul lain.

"Dari awal" berarti kebutuhan bisnis dirancang secara utuh tanpa menganggap implementasi
yang sekarang sudah benar atau lengkap. Namun, project juga tidak benar-benar dianggap
kosong. Sebelum membuat tabel, endpoint, atau halaman baru, skill wajib membaca kemampuan
yang telah tersedia dan menentukan bagian yang dapat digunakan kembali.

Contohnya:

- IGD tidak membuat ulang tabel dokter apabila dokter sudah dimiliki modul master dokter.
- IGD tidak membuat ulang tabel pasien apabila pasien sudah terbentuk dari kiosk atau alur
  registrasi lain.
- Pengkajian rawat inap tidak membuat episode pasien baru apabila admission dan encounter
  sudah menjadi sumber kebenaran modul rawat inap.
- Entitas bersama diperluas atau dihubungkan melalui relasi/adapter; tidak diduplikasi hanya
  agar satu modul terasa mandiri.

Keluaran utama untuk setiap modul adalah:

1. arsitektur backend;
2. arsitektur frontend yang berfokus pada kontrak fungsional dan teknis;
3. ERD per submodul atau bounded context;
4. roadmap task backend;
5. roadmap task frontend.

Flowchart dan use-case diagram tidak termasuk keluaran. Aktor ditulis dalam tabel peran,
alur bisnis ditulis sebagai skenario dan acceptance criteria, sedangkan perubahan status
ditulis sebagai state-transition matrix.

## 2. Prinsip Kerja Bersama

Seluruh skill harus memegang prinsip berikut:

1. **Business-first.** Mulai dari tujuan, aturan, risiko, dan hasil yang harus dicapai;
   jangan mulai dari controller atau tabel yang kebetulan sudah ada.
2. **Reuse-first, bukan reuse-blind.** Cari kemampuan existing lebih dahulu, tetapi jangan
   memaksakan komponen yang salah secara domain atau tidak siap secara runtime.
3. **Evidence-based.** Setiap klaim mengenai implementasi harus memiliki bukti file, simbol,
   migration, endpoint, route, atau test yang dapat ditelusuri.
4. **Ownership jelas.** Setiap data memiliki pemilik domain. Modul pemakai mereferensikan
   atau memperluas data bersama, bukan mengambil alih kepemilikannya diam-diam.
5. **Kontrak sebelum implementasi.** Aturan status, validasi, permission, audit, API, dan
   ERD disepakati sebelum task pembangunan dijalankan.
6. **Vertical slice.** Roadmap disusun berdasarkan kemampuan bisnis yang dapat diuji dari
   ujung ke ujung, bukan sekadar daftar model lalu daftar controller.
7. **Tidak menyamakan scaffold dengan siap pakai.** Keberadaan model, tabel, atau CRUD belum
   membuktikan workflow dapat berjalan end-to-end.
8. **Keputusan manusia tetap utama.** Skill memberi analisis dan opsi; keputusan produk,
   klinis, keamanan, serta arahan atasan tidak boleh digantikan oleh asumsi agent.

## 3. Klasifikasi Kemampuan Existing

Semua hasil penelusuran project menggunakan status yang sama:

| Status | Arti |
|---|---|
| **Ready to reuse** | Sudah tersedia, terhubung, terdaftar, dan cukup aman dipakai oleh modul baru |
| **Reuse with adapter** | Dapat dipakai melalui mapping, facade, endpoint tambahan, atau lapisan integrasi |
| **Extend** | Pemilik dan fondasinya benar, tetapi field, relasi, rule, atau endpoint masih kurang |
| **Repair** | Sudah ada, tetapi memiliki defect atau blocker yang harus ditutup sebelum digunakan |
| **Missing** | Belum tersedia dan memang perlu dibuat |
| **Conflict** | Implementasi existing bertentangan dengan kebutuhan bisnis atau sumber kebenaran lain |
| **Unknown** | Bukti belum cukup; perlu keputusan manusia atau pemeriksaan environment/data aktual |

Status tidak boleh diberikan hanya karena nama tabel atau class ditemukan. Pemeriksaan
harus mencakup hubungan runtime seperti registrasi dependency injection, migration,
seed/default data, permission, pemanggilan frontend, dan test bila tersedia.

## 4. Daftar Skill yang Direkomendasikan

| Urutan | Skill | Fungsi utama | Sifat utama |
|---:|---|---|---|
| 1 | `grill-me` | Wawancara kritis untuk menutup kebutuhan dan keputusan | Interaktif, tidak menulis kode |
| 2 | `trace-existing-capabilities` | Menelusuri tabel, API, UI, dan alur existing lintas modul | Read-only |
| 3 | `design-business-module` | Menyusun arsitektur backend, frontend, kontrak, dan ERD | Desain, tidak menulis kode aplikasi |
| 4 | `plan-module-delivery` | Mengubah desain menjadi roadmap backend dan frontend | Perencanaan |
| 5 | `build-module-backend` | Mengerjakan satu task backend berdasarkan kontrak | Menulis kode sesuai task |
| 6 | `build-module-frontend` | Mengerjakan satu task frontend dengan batas kewenangan yang jelas | Menulis kode sesuai task |
| 7 | `verify-module-readiness` | Mengukur kesiapan nyata dan mencari gap end-to-end | Read-only secara default |

Setiap nama mengikuti format kebab-case dan sebaiknya diwujudkan sebagai satu folder skill
mandiri dengan `SKILL.md`. Slash command, apabila diperlukan, hanya menjadi wrapper tipis;
prosedur utama tidak boleh disalin ke dua tempat.

### 4.1 Pembagian lintas project yang direkomendasikan

Tujuh skill tetap merupakan satu suite, tetapi tidak boleh disalin seluruhnya ke backend
dan frontend. Pembagiannya menggunakan dua jenis ownership:

- **shared/cross-project skill** mengatur discovery, keputusan, kontrak, desain, roadmap,
  dan readiness untuk kedua repository;
- **repo-local implementation skill** menulis kode hanya pada project yang dimilikinya.

| Skill | Jenis | Lokasi canonical tahap awal | Cakupan repository |
|---|---|---|---|
| `grill-me` | Shared | `QuilvianBackend/agent-skills/grill-me/` | Keputusan bisnis dan kewenangan untuk backend serta frontend |
| `trace-existing-capabilities` | Shared | `QuilvianBackend/agent-skills/trace-existing-capabilities/` | Membaca kedua repository secara read-only |
| `design-business-module` | Shared | `QuilvianBackend/agent-skills/design-business-module/` | Satu desain target, termasuk arsitektur backend dan frontend |
| `plan-module-delivery` | Shared | `QuilvianBackend/agent-skills/plan-module-delivery/` | Satu traceability dengan roadmap backend dan frontend terpisah |
| `verify-module-readiness` | Shared | `QuilvianBackend/agent-skills/verify-module-readiness/` | Audit end-to-end kedua repository secara read-only default |
| `build-module-backend` | Repo-local | `QuilvianBackend/agent-skills/build-module-backend/` | ASP.NET Core, EF Core, API, migration, DI, audit, dan backend test |
| `build-module-frontend` | Repo-local | `QuilvianFrontEnd/agent-skills/build-module-frontend/` | Next.js, API client, state, UI, permission, dan frontend test |

Secara konseptual pembagiannya adalah **5 shared + 2 implementation skills**. Karena
workspace parent belum merupakan Git repository, lima shared skill untuk tahap awal
dititipkan dan diberi versi di backend. Selain itu, `QuilvianFrontEnd/docs/` pada kondisi
project saat ini diabaikan oleh `.gitignore`. Akibatnya struktur fisiknya menjadi **6 skill
di backend dan 1 skill di frontend**. Backend hanya menjadi *custodian* artefak dan kontrak shared,
bukan pemilik tunggal keputusan produk, klinis, keamanan, maupun desain frontend.

Folder `.claude/` pada kedua repository juga sedang diabaikan Git. Karena itu,
`agent-skills/` direkomendasikan sebagai **source package yang tracked**, sedangkan
`.claude/skills/` dan `.claude/commands/` menjadi **installation target lokal/generated**.
Pembuatan paket nanti harus memverifikasi `git check-ignore` sebelum menyatakan sebuah skill
sudah terversi.

### 4.2 Satu sumber canonical, tanpa salinan prosedur

- Satu skill hanya memiliki satu `SKILL.md` canonical.
- Dokumen pada `docs/agency/` ini adalah spesifikasi canonical. Jika ditemukan salinan
  bernama sama pada folder lain, salinan tersebut bukan sumber kedua dan tidak boleh
  diperbarui secara independen.
- Repository lain boleh memiliki slash command atau shim tipis yang dihasilkan ke lokasi
  instalasi dan menunjuk source package, versi, serta hash skill canonical; wrapper tidak
  boleh menyalin body prosedur.
- Business rule, ERD, API contract, dan readiness verdict tidak boleh mempunyai salinan
  canonical kedua di frontend.
- Artefak frontend boleh mereferensikan requirement ID, decision ID, blueprint revision,
  dan contract version, tetapi tidak menyalin aturan shared lalu mengubahnya sendiri.
- Jika tool tidak dapat mengikuti path ke sibling repository, buat generated shim minimum
  yang mendeteksi versi/hash berbeda dan gagal terkendali; jangan membuat fork manual.

Penempatan shared skill pada backend bersifat keputusan operasional tahap awal, bukan
pernyataan bahwa semua skill bersifat backend. Bila kelak tersedia repository engineering
yang terversi dan dapat diakses kedua project, lima shared skill dapat dipindahkan ke sana
tanpa memindahkan dua implementation skill.

## 5. Kontrak Setiap Skill

### 5.1 `grill-me`

#### Tujuan

Mewawancarai pemilik kebutuhan secara kritis sampai batas modul, aturan bisnis, jalur
normal, jalur gagal, dan kewenangan keputusan cukup jelas untuk dirancang.

#### Tahapan wawancara

1. tujuan bisnis, masalah, outcome, dan batas scope;
2. aktor, tanggung jawab, permission, dan pemilik keputusan;
3. titik masuk, prasyarat, titik keluar, dan definisi selesai;
4. status, transisi, pembatalan, koreksi, dan reopening;
5. jalur normal, exception, downtime, duplikasi, dan data terlambat;
6. integrasi upstream/downstream serta sumber kebenaran data;
7. audit, privasi, logging, pelaporan, retensi, dan risiko;
8. acceptance criteria dan bukti keberhasilan.

#### Aturan

- Jangan menanyakan hal yang dapat ditemukan secara aman dari source code; serahkan itu
  kepada `trace-existing-capabilities`.
- Jangan menerima istilah seperti "aktif", "selesai", "terintegrasi", atau "dibatalkan"
  tanpa definisi yang dapat diuji.
- Tandai setiap jawaban sebagai **fact**, **decision**, **assumption**, **conflict**, atau
  **open question**.
- Jangan memulai desain final atau implementasi saat keputusan berisiko tinggi masih
  terbuka.
- Jalankan dua kali bila perlu: scope pass sebelum audit existing dan closure pass sesudah
  gap serta konflik ditemukan.

#### Pertanyaan kewenangan frontend

Wawancara juga harus menentukan:

- siapa yang menetapkan menu dan urutannya;
- siapa yang menyetujui tampilan;
- apakah mockup bersifat wajib atau referensi;
- bagian mana yang boleh diputuskan developer;
- siapa yang menetapkan route dan penamaan;
- bagaimana konflik antara arahan atasan, kontrak API, dan keamanan dieskalasikan.

#### Keluaran

`00-interview-decisions.md`, berisi scope, glossary, aktor, business rules, invariants,
status dan transisinya, skenario normal/exception, keputusan, asumsi, pertanyaan terbuka,
acceptance criteria, serta frontend decision-authority matrix.

Dokumen keputusan memakai `decision_id` stabil, owner, status `draft`, `approved`,
`rejected`, atau `superseded`, sumber/evidence, allowed range, approver, dan tanggal keputusan. Keputusan UI
yang belum diarahkan diberi status `DEV_DISCRETION`; ia tidak boleh dikunci berdasarkan
preferensi agent.

### 5.2 `trace-existing-capabilities`

#### Tujuan

Membaca kemampuan project secara global agar modul baru menyambung ke alur yang sudah ada
dan tidak menciptakan data ganda.

#### Pemeriksaan backend

- model/entity, enum, `DbSet`, configuration, dan data ownership;
- PK, FK, cardinality, nullability, unique constraint, index, dan delete behavior;
- migration serta seeder/default data;
- DTO, mapper, controller, endpoint, service, dan validasi business rule;
- dependency injection dan konfigurasi runtime;
- authentication, permission, audit, soft-delete, logging, dan privacy;
- unit, integration, contract, serta end-to-end test.

#### Pemeriksaan frontend

- route, menu aktif, permission guard, dan halaman yang benar-benar dapat dicapai;
- API client/service, query, state, cache, form, validation, dan mapping enum/master data;
- komponen reusable, pola error/loading/empty/retry, dan pencegahan duplicate submit;
- test serta perbedaan antara implementasi aktif, dormant, mock, dan dummy.

#### Penelusuran alur lintas modul

Skill tidak berhenti pada tabel. Contoh alur yang harus ditelusuri:

`kiosk/registrasi -> patient master -> encounter/admission -> modul klinis -> billing/discharge`

Untuk setiap kebutuhan, tentukan pemilik data dan beri salah satu status pada Bagian 3.
Jika hanya migration yang ditemukan, jangan menyimpulkan migration sudah diterapkan pada
database aktual. Jika pemeriksaan database tidak diizinkan, beri status **Unknown**.

#### Keluaran

`01-existing-capability-map.md`, berisi inventory, alur existing, kandidat reuse/adapter/
extension, gap, konflik, risiko duplikasi, bukti file dan baris, serta rekomendasi keputusan.

Capability map juga mencatat root dan commit SHA backend serta frontend yang diperiksa.
Setiap bukti menggunakan format `repository + relative path + line/symbol + commit SHA`.
Jika salah satu SHA berubah sebelum desain atau implementasi, skill berikutnya wajib
menjalankan impact scan dan tidak boleh menganggap hasil trace masih mutakhir.

### 5.3 `design-business-module`

#### Tujuan

Menyusun desain ideal berdasarkan bisnis, lalu menimpakannya dengan capability map untuk
menentukan mana yang reused, extended, repaired, atau dibuat baru.

Desain membedakan secara tegas:

- **as-is contract**: perilaku yang benar-benar dibuktikan oleh controller, DTO, OpenAPI,
  frontend consumer, migration, dan runtime wiring saat discovery;
- **to-be contract**: perilaku target yang sudah memiliki versi dan persetujuan manusia.

Frontend dan backend sama-sama tunduk pada to-be contract yang telah dikunci. Frontend
tidak otomatis mengikuti perubahan controller terbaru, dan backend tidak boleh mengubah
payload/status diam-diam hanya karena frontend belum dibangun.

#### Arsitektur backend wajib memuat

- bounded context, tanggung jawab modul, dan data ownership;
- aggregate root, invariant, transaction boundary, dan rollback behavior;
- pemisahan API, application/workflow service, domain rule, dan persistence;
- integrasi sinkron/asinkron, idempotency, retry, timeout, dan failure handling;
- state-transition matrix dan aturan koreksi/pembatalan;
- concurrency, unique constraint, dan pencegahan transaksi ganda;
- authentication, permission, audit, logging, privacy, dan retention;
- API contract, error contract, pagination/filtering, dan versioning bila dibutuhkan;
- dependency injection, migration, seeding, observability, dan strategi test.

#### Arsitektur frontend wajib memuat

- kebutuhan fungsional dan tindakan yang harus dapat dilakukan pengguna;
- kontrak API, model data, status, validasi, permission, serta error mapping;
- aset existing yang dapat digunakan kembali;
- strategi server state/cache, invalidation, loading, empty, error, retry, dan offline bila
  relevan;
- pencegahan duplicate submit dan perlindungan data sensitif;
- batas teknis, strategi test, dan dependency pada backend;
- decision-authority matrix, keputusan developer, rekomendasi opsional, serta pertanyaan
  yang perlu persetujuan atasan.

Arsitektur frontend **bukan** instruksi visual sepihak. Skill tidak menetapkan sidebar,
urutan menu, route final, tab versus modal/drawer/page, warna, layout, component library,
atau styling kecuali hal tersebut sudah menjadi requirement atau arahan yang disetujui.
Skill boleh menawarkan opsi dan trade-off; developer menerapkannya berdasarkan instruksi
atasan, product owner, UI/UX lead, design system, dan konvensi project.

#### ERD per modul

ERD dibagi per bounded context/submodul agar tetap terbaca. Setiap entity wajib menampilkan:

- penanda **Existing**, **Extend**, **New**, atau **Adapter/View**;
- owner domain;
- primary key dan foreign key;
- cardinality dan optionality/nullability;
- unique constraint dan index penting;
- delete behavior;
- audit fields dan concurrency field bila diperlukan.

ERD tidak boleh memperkenalkan `PatientIGD`, `DoctorIGD`, atau salinan entitas bersama lain
tanpa alasan ownership yang sah.

Contoh pembagian IGD:

- master dan setting IGD;
- registrasi/visit dan hubungan ke patient/encounter;
- triage dan retriage;
- pengkajian serta dokumentasi klinis;
- observasi, resusitasi, dan tindakan;
- transfer dan disposition;
- integrasi farmasi, laboratorium, radiologi, billing, dan rawat inap.

Contoh pembagian pengkajian rawat inap:

- admission/episode dan hubungan ke patient/encounter;
- pengkajian awal perawat;
- pengkajian dokter;
- pengkajian risiko dan fungsional;
- diagnosis, masalah, dan rencana asuhan;
- monitoring dan pengkajian ulang;
- transfer internal dan discharge.

#### Keluaran

- `02-backend-architecture.md`;
- `03-frontend-architecture.md`;
- ERD context dan ERD per submodul;
- API, integration, state-transition, validation, permission, dan audit contract.

Setiap contract memiliki `contract_version`, status `draft/approved/superseded`, owner,
`approved_by`, `approved_at`, `input_revision`, `input_hash`, dan compatibility impact.
Perubahan kontrak setelah approval wajib memicu diff, impact scan kedua repository, serta
persetujuan API/contract owner.

### 5.4 `plan-module-delivery`

#### Tujuan

Mengubah desain yang telah disetujui menjadi roadmap backend dan frontend yang saling
terhubung melalui kontrak API.

Setiap task wajib memiliki:

| Field | Isi minimum |
|---|---|
| ID dan judul | ID stabil serta outcome bisnis yang jelas |
| Requirement | Aturan/skenario yang dilayani |
| Dependency | Task, keputusan, data, atau API yang harus tersedia |
| Reuse | Entity, endpoint, service, atau component existing yang digunakan |
| Dampak | Schema, migration, API, permission, audit, dan UI yang terpengaruh |
| Lokasi | Perkiraan file/area yang akan disentuh |
| Acceptance criteria | Perilaku yang dapat diuji, termasuk exception |
| Test | Unit, integration, contract, UI, atau E2E yang dibutuhkan |
| Risiko/blocker | Konflik, keputusan terbuka, data migration, dan integrasi |
| Definition of Done | Kondisi objektif agar task boleh dinyatakan selesai |

Roadmap memakai vertical slice. Task frontend ditulis sebagai outcome, bukan keputusan
tampilan. Contoh:

> Sediakan akses yang memiliki permission ke antrean triage dan pengkajian pasien;
> penempatan navigasi mengikuti brief frontend yang telah disetujui.

Hindari task seperti "buat menu IGD di urutan ketiga dan gunakan drawer" apabila keputusan
tersebut belum diberikan oleh atasan atau desain resmi.

#### Keluaran

- `roadmap/backend-roadmap.md`;
- `roadmap/frontend-roadmap.md`;
- `roadmap/requirement-traceability.md`.

Task backend dan frontend boleh berjalan paralel hanya bila contract version yang digunakan
sudah `approved`. Setiap task menyimpan blueprint revision dan contract version; task harus
berhenti jika input tersebut berubah atau berstatus `superseded`.

### 5.5 `build-module-backend`

#### Tujuan dan pagar pengaman

- Kerjakan satu task roadmap pada satu waktu.
- Baca decision log, capability map, arsitektur, ERD, dan kontrak sebelum mengubah kode.
- Gunakan entity bersama melalui relasi atau adapter; jangan menduplikasinya.
- Terapkan workflow dan invariant, bukan CRUD saja.
- Lengkapi model/configuration, DTO, workflow service, controller, DI, migration, seed,
  permission, logging/audit, dan test sesuai dampak task.
- Jaga kompatibilitas endpoint yang sudah digunakan modul lain.
- Hubungkan perubahan ke requirement ID dan ERD.
- Verifikasi blueprint revision, contract version, approval, dan source hash sebelum menulis
  kode; hentikan task bila input stale atau belum disetujui.
- Berhenti dan eskalasi jika task memerlukan keputusan bisnis atau kewenangan baru.

Skill tidak boleh menandai task selesai hanya karena build berhasil. Definition of Done dan
acceptance criteria task tetap menjadi tolok ukur.

### 5.6 `build-module-frontend`

#### Tujuan dan pagar pengaman

- Kerjakan satu task roadmap pada satu waktu.
- Baca requirement, API contract, capability map, design system, dan instruksi atasan atau
  mockup yang berlaku.
- Pisahkan requirement wajib dari ruang keputusan developer.
- Ikuti pola route, API client, state, form, permission, dan component existing project.
- Jangan hardcode master data atau enum yang seharusnya berasal dari backend.
- Tangani permission, loading, empty, error, retry, stale data, dan duplicate submit.
- Jangan mengirim data sensitif ke log atau menampilkannya kepada role yang tidak berhak.
- Uji payload, error mapping, dan transisi status terhadap kontrak backend.
- Verifikasi blueprint revision, contract version, approval, dan source hash sebelum menulis
  kode; hentikan task bila input stale atau belum disetujui.

Struktur menu, route, layout, navigasi, komponen, dan tampilan adalah keputusan developer
di bawah arahan atasan serta standar project. Skill tidak boleh mengubahnya karena preferensi
pribadi. Skill tetap wajib menghentikan atau menandai pilihan yang menyebabkan route rusak,
permission bocor, payload salah, transaksi ganda, data sensitif terekspos, atau state ilegal.

### 5.7 `verify-module-readiness`

#### Tujuan

Menentukan kesiapan sebenarnya dari requirement sampai workflow end-to-end. Skill bersifat
read-only secara default; permintaan audit tidak otomatis mengizinkan perbaikan.

#### Pemeriksaan

- requirement dan keputusan terhadap implementasi;
- ERD terhadap entity, configuration, migration, dan schema yang dapat dibuktikan;
- API contract terhadap DTO, controller, service, validation, dan error response;
- kompatibilitas frontend-backend;
- dependency injection, seed/default data, permission, logging, audit, dan privacy;
- state transition, rollback, idempotency, concurrency, dan failure path;
- automated test serta acceptance criteria;
- integrasi upstream/downstream dan kesiapan operasional.

Verifier MUST menolak keputusan **Ready** apabila commit SHA kedua repository berubah tanpa
impact scan sejak discovery/design, contract lock tidak cocok, approval belum lengkap, atau
ditemukan dua artefak yang sama-sama mengaku canonical.

Validasi frontend menilai fungsi, kontrak, aksesibilitas, keamanan, dan kepatuhan terhadap
instruksi atasan yang terdokumentasi. Skill tidak menilai selera visual seperti tab versus
drawer atau warna tanpa mockup, design system, atau acceptance standard yang sah.

#### Keluaran

- progress terpisah untuk foundation, backend, frontend, integration, dan verification;
- bukti untuk setiap klaim;
- blocker dan gap kritis;
- roadmap tersisa;
- keputusan **Ready**, **Ready with conditions**, atau **Not ready**.

Persentase harus dijelaskan dengan bobot dan denominator. Nilai "jumlah file sudah ada"
harus dipisahkan dari nilai "siap dipakai end-to-end".

## 6. Urutan Pemakaian dan Gerbang Persetujuan

1. Jalankan `grill-me` untuk scope pass.
2. Jalankan `trace-existing-capabilities` pada backend, frontend, dan alur upstream/downstream.
3. Jalankan kembali `grill-me` untuk menutup konflik dan keputusan yang ditemukan.
4. Jalankan `design-business-module`.
5. Minta review manusia atas arsitektur, ERD, aturan status, dan pembagian kewenangan.
6. Jalankan `plan-module-delivery` setelah desain disetujui.
7. Jalankan `build-module-backend` dan `build-module-frontend` per task serta dependency.
8. Jalankan `verify-module-readiness` sebelum modul dinyatakan siap.

Handoff antar-skill menggunakan urutan artefak berikut:

```text
decision draft
  -> capability map + closure questions
  -> decision closure
  -> target contract/version
  -> human approval
  -> backend/frontend roadmap
  -> backend/frontend implementation
  -> contract-drift + integration/E2E verification
```

Setiap tahap wajib membaca manifest blueprint dan menolak input yang stale, belum
disetujui, atau sudah `superseded`. Kegagalan satu keputusan lokal tidak harus membekukan
seluruh modul; blokir hanya task/contract yang bergantung padanya selama dependency dapat
dipisahkan dengan aman.

Gerbang wajib:

- implementasi tidak dimulai saat invariant klinis/bisnis kritis belum diputuskan;
- schema tidak dibuat sebelum ownership dan reuse mapping disetujui;
- frontend tidak mengunci tampilan yang masih menjadi keputusan atasan;
- disposition, discharge, close, pembayaran, atau state final lain tidak boleh dibuat
  tanpa prasyarat dan rollback yang eksplisit;
- modul tidak dinyatakan selesai sebelum integrasi dan acceptance scenario diuji.

## 7. Matriks Kewenangan Frontend

| Area keputusan | Pemilik utama | Peran skill |
|---|---|---|
| Business rule dan legal state transition | Product/domain owner bersama backend | Menemukan gap dan menjaga traceability |
| API payload, error contract, dan compatibility | Backend/API contract owner | Memvalidasi kesesuaian |
| Authentication, permission, dan privacy | Security/domain owner | Menandai pelanggaran; tidak boleh diabaikan |
| Menu, urutan, nama, dan visibilitas | Atasan/product/UI lead bila diarahkan; developer bila didelegasikan | Memberi opsi, bukan keputusan sepihak |
| Route | Konvensi project/atasan; developer jika belum ditentukan | Memeriksa konflik dan broken navigation |
| Layout dan pola interaksi | Developer, kecuali ada mockup/brief mengikat | Memberi trade-off opsional |
| Component, hooks, state, dan responsive implementation | Developer mengikuti standar project | Memeriksa maintainability dan kontrak |
| Warna, typography, dan visual language | Design system/UI lead | Memeriksa standar yang terdokumentasi |
| Prioritas delivery | Atasan/product owner | Menampilkan dependency dan risiko |

Jika arahan atasan bertentangan dengan keamanan, privasi, invariant bisnis, atau kontrak API,
skill tidak boleh diam-diam memilih salah satu. Catat konflik, jelaskan dampaknya, dan minta
keputusan dari pihak yang berwenang.

## 8. Struktur Artefak per Modul

Saat rangkaian skill kelak digunakan pada sebuah modul, struktur hasil yang direkomendasikan
adalah:

```text
docs/module-blueprints/<module-name>/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── erd/
│   ├── 00-context-erd.md
│   ├── 01-<submodule>.md
│   ├── 02-<submodule>.md
│   └── data-dictionary.md
├── contracts/
│   ├── api-contract.md
│   ├── state-transition-matrix.md
│   ├── validation-matrix.md
│   ├── integration-contract.md
│   └── permission-audit-matrix.md
├── roadmap/
│   ├── backend-roadmap.md
│   ├── frontend-roadmap.md
│   └── requirement-traceability.md
└── testing/
    └── acceptance-test-matrix.md
```

Struktur tersebut adalah kontrak keluaran saat skill digunakan, bukan file yang dibuat
oleh pekerjaan dokumentasi ini. Untuk tahap awal, path tersebut berada di
`QuilvianBackend/docs/module-blueprints/<module-name>/` agar terversi dalam satu repository.

`blueprint-manifest.md` sekurang-kurangnya memuat:

| Field | Fungsi |
|---|---|
| `blueprint_id` | Identitas stabil lintas seluruh artefak dan task |
| `revision` | Revisi blueprint saat ini |
| `status` | `draft`, `approved`, atau `superseded` |
| `owners`, `approved_by`, dan `approved_at` | Authority dan bukti persetujuan |
| `backend_commit_sha` | Snapshot backend yang digunakan discovery/design |
| `frontend_commit_sha` | Snapshot frontend yang digunakan discovery/design |
| `contract_versions` | Versi API/integration/state contract yang berlaku |
| `artifact_hashes` | Deteksi drift atau perubahan di luar handoff resmi |
| `input_revisions` dan `input_hashes` | Versi serta hash artefak upstream yang menjadi dasar |

Frontend menyimpan implementation notes, test, dan kode miliknya di repository frontend,
tetapi mereferensikan `blueprint_id`, revision, requirement ID, dan contract version ini.

## 9. Bentuk Roadmap Standar

### Roadmap backend

| Fase | Fokus |
|---|---|
| B0 — Discovery lock | Scope, keputusan, ownership, reuse map, dan kontrak disetujui |
| B1 — Foundation | Schema/extension, configuration, migration, seed, permission, dan DI |
| B2 — Core workflow | Aggregate, invariant, state transition, API, audit, dan unit test |
| B3 — Integration | Upstream/downstream, adapter, idempotency, retry, dan contract test |
| B4 — Operational | Query operasional, reporting, observability, privacy, dan failure handling |
| B5 — Readiness | Integration/E2E test, data verification, performance, dan acceptance closure |

### Roadmap frontend

| Fase | Fokus |
|---|---|
| F0 — Direction lock | Functional brief, kewenangan UI, route/menu decision, dan API dependency |
| F1 — Data foundation | API client, types, mapping, cache/state, permission, dan shared components |
| F2 — Core workflow | User outcome utama per role berdasarkan API contract |
| F3 — Exception handling | Error, empty, retry, cancellation, correction, conflict, dan duplicate submit |
| F4 — Quality | Accessibility, responsive behavior, privacy, unit/component/integration test |
| F5 — Acceptance | Contract test, E2E utama, UAT, telemetry, dan closure gap |

Fase bukan sprint tetap. Task backend dan frontend dapat berjalan paralel setelah kontrak
yang dibutuhkan stabil. Dependency dan acceptance criteria, bukan nomor fase semata, yang
menentukan task boleh dimulai.

## 10. Contoh Penerapan Lintas Modul

### IGD

Capability trace harus mencari patient dari kiosk/registrasi, doctor master, encounter,
service unit, vital sign, CPPT, prescription, procedure, billing, bed, dan admission.
Desain IGD kemudian hanya menambah ownership yang benar-benar milik IGD seperti visit IGD,
triage/retriage, observasi IGD, resusitasi, transfer, dan disposition—setelah status setiap
kemampuan existing diputuskan.

### Pengkajian rawat inap

Capability trace harus mencari patient, admission/encounter, dokter/perawat, kamar/bed,
vital sign, diagnosis, procedure, CPPT, dan discharge. Modul pengkajian boleh menambah form,
jawaban, risk score, care plan, dan reassessment yang dimiliki domainnya, tetapi tidak
membuat ulang patient, tenaga kesehatan, atau episode rawat inap.

Dua contoh ini menjadi uji generalisasi minimum. Jika sebuah skill hanya menghasilkan
keluaran yang benar untuk IGD tetapi gagal memodelkan pengkajian rawat inap, skill tersebut
masih terlalu spesifik.

## 11. Rekomendasi Pendukung

Selain lima keluaran utama, setiap blueprint sebaiknya memiliki:

- glossary dan decision log;
- requirement traceability matrix;
- state-transition dan validation matrix;
- API dan integration contract;
- permission, audit, privacy, serta data-retention matrix;
- data dictionary dan ownership map;
- acceptance-test matrix dengan jalur normal dan exception;
- deployment, migration, rollback, seed, dan observability checklist.

Artefak pendukung dibuat hanya jika relevan. Jangan menghasilkan dokumen kosong untuk
memenuhi struktur folder.

## 12. Bentuk Implementasi Skill yang Disarankan

Ketika disetujui untuk benar-benar dibuat, setiap skill menjadi paket mandiri dengan
pembagian berikut:

```text
QuilvianBackend/agent-skills/
├── grill-me/
│   └── SKILL.md
├── trace-existing-capabilities/
│   └── SKILL.md
├── design-business-module/
│   └── SKILL.md
├── plan-module-delivery/
│   └── SKILL.md
├── verify-module-readiness/
│   └── SKILL.md
└── build-module-backend/
    └── SKILL.md

QuilvianFrontEnd/agent-skills/
└── build-module-frontend/
    └── SKILL.md
```

Struktur di atas adalah source package yang harus masuk version control. Saat digunakan,
installer/sync step menempatkan package atau shim yang diperlukan ke `.claude/skills/`
tanpa menjadikan hasil generated tersebut sumber canonical. Alternatifnya, tim boleh
mengubah aturan `.gitignore` secara eksplisit agar folder skill tertentu di `.claude/`
menjadi tracked; keputusan itu harus berlaku konsisten pada kedua repository.

Ketentuannya:

- frontmatter hanya memiliki `name` dan `description`;
- description menjelaskan fungsi sekaligus kondisi pemicu;
- body memakai instruksi imperatif dan tetap ringkas;
- detail template/matrix ditempatkan dalam `references/` hanya bila benar-benar digunakan;
- script ditambahkan hanya untuk validasi yang berulang dan deterministik;
- jangan menambah README, changelog, atau dokumentasi duplikat di dalam paket skill;
- validasi setiap skill dan uji pada minimal satu skenario IGD serta satu skenario non-IGD.

### 12.1 Wrapper frontend untuk shared skill

Jika shared skill perlu dipanggil dari sesi frontend, hasilkan command atau shim tipis ke
installation target lokal:

```text
QuilvianFrontEnd/.claude/commands/
├── grill-me.md
├── trace-existing-capabilities.md
├── design-business-module.md
├── plan-module-delivery.md
└── verify-module-readiness.md
```

Setiap wrapper hanya menyatakan nama skill canonical, relative path source package sibling
repository, versi/hash yang diharapkan, serta cara melapor bila path tidak tersedia.
Wrapper tidak memiliki prosedur interview, discovery, desain, planning, atau verification
sendiri. Karena `.claude/` saat ini di-ignore, keberadaan wrapper lokal bukan bukti bahwa
source package sudah dibagikan atau terversi.

Command frontend `QuilvianFrontEnd/.claude/commands/grill-me.md` yang sekarang ada harus
diaudit ketika suite diimplementasikan. Isinya masih frontend-oriented, mencampur discovery
API dengan interview, dan memuat kebijakan lama bahwa backend tidak pernah diubah. Jangan
menjadikannya prosedur canonical kedua; migrasikan kebutuhan yang masih valid ke shared
`grill-me`/`trace-existing-capabilities`, lalu jadikan command tersebut wrapper tipis.

### 12.2 Promosi menjadi skill global

Lima shared skill belum langsung dipasang ke user-global skill directory. "Generik lintas
modul Quilvian" belum membuktikan bahwa skill aman untuk project lain. Promosi dilakukan
setelah:

1. lulus forward-test pada IGD;
2. lulus forward-test pada pengkajian rawat inap;
3. lulus minimal satu project non-Quilvian;
4. core workflow dapat dipisahkan dari path, framework, policy, dan output profile Quilvian;
5. tersedia repository/profile yang terversi untuk konfigurasi khusus project.

Setelah promosi, `grill-me`, `trace-existing-capabilities`, `design-business-module`,
`plan-module-delivery`, dan `verify-module-readiness` boleh menjadi global core dengan
project profile. `build-module-backend` dan `build-module-frontend` tetap repo-local karena
sangat terikat stack, aturan, test command, laporan, dan guardrail masing-masing project.

Dokumen ini tidak memasang ketujuh skill tersebut. Pembuatan paket skill adalah pekerjaan
lanjutan terpisah; lokasi tahap awalnya sudah direkomendasikan oleh dokumen ini, sedangkan
pembuatan file skill tetap memerlukan task implementasi tersendiri.

## 13. Acceptance Criteria Skill Suite

Rangkaian skill dianggap layak dipakai apabila:

1. menghasilkan desain yang dapat dipakai pada IGD dan sedikitnya satu modul berbeda;
2. membedakan desain ideal dari keadaan source code saat ini;
3. menemukan dan memakai ulang entity lintas modul tanpa membuat duplikasi;
4. menyertakan bukti untuk klaim mengenai kemampuan existing;
5. menghasilkan arsitektur backend, arsitektur frontend, ERD per submodul, serta dua roadmap;
6. menjaga kewenangan menu/tampilan developer dan atasan;
7. menghubungkan requirement -> desain -> ERD/API -> task -> test;
8. tidak menganggap CRUD, migration, atau build sukses sebagai bukti kesiapan end-to-end;
9. menyatakan asumsi, konflik, dan unknown secara jujur;
10. tidak menghasilkan flowchart atau use-case diagram;
11. hanya memiliki satu prosedur canonical untuk setiap skill;
12. menempatkan lima shared skill dan backend builder di backend serta frontend builder di
    frontend pada tahap awal;
13. mendeteksi revision, contract version, approval, dan source SHA yang stale;
14. membuktikan kompatibilitas backend-frontend melalui contract serta integration/E2E test.

## 14. Dampak Teknis Pekerjaan Dokumentasi Ini

### Endpoint dan kontrak aplikasi

Tidak ada endpoint, DTO, model, service, migration, configuration, atau kontrak runtime yang
diubah. Dokumen ini hanya menjadi dasar persetujuan sebelum paket skill benar-benar dibuat.

### File yang disentuh

| File | Perubahan |
|---|---|
| `docs/agency/rekomendasi-skills-pengembangan-modul.md` | Draft existing yang belum tracked diperbarui — pembagian hybrid 5 shared + 2 repo-local, canonical ownership, handoff, dan anti-drift |
| `docs/hamzah/report/rekomendasi-skills-pengembangan-modul.md` | Baru — laporan revisi dokumentasi |

### Dampak ke frontend

Tidak ada kode frontend yang berubah. Bagian frontend pada dokumen menetapkan batas
kewenangan: skill mendefinisikan kontrak fungsional/teknis dan guardrail, sedangkan menu
serta tampilan mengikuti keputusan developer di bawah instruksi atasan dan standar project.

### Cara menguji

Karena perubahan hanya berupa Markdown:

1. pastikan satu dokumen rekomendasi diperbarui dan satu laporan perubahan dibuat;
2. pastikan tujuh nama skill dan urutan handoff tercantum;
3. pastikan keluaran arsitektur backend, arsitektur frontend, ERD, dan kedua roadmap ada;
4. pastikan flowchart/use-case dikecualikan;
5. pastikan aturan reuse tabel existing dan kewenangan frontend tertulis eksplisit;
6. pastikan pembagian 5 shared + 2 repo-local dan lokasi fisik 6 backend + 1 frontend konsisten;
7. pastikan hanya ada satu prosedur canonical dan wrapper frontend dinyatakan tipis;
8. pastikan manifest, revision, contract version, approval, SHA, dan stale-input guard tersedia.

### Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Pemeriksaan struktur dan isi Markdown | **Lulus** — heading utama bernomor 1–14 lengkap dan unik; seluruh code fence seimbang; output wajib serta pengecualian diagram tetap tersedia |
| Pemeriksaan pembagian skill | **Lulus** — tepat 7 skill: 5 shared + 2 repo-local; source package menjadi 6 backend + 1 frontend |
| Pemeriksaan lokasi source/install | **Lulus** — kandidat `agent-skills/` kedua repository tidak di-ignore, sedangkan `.claude/` dinyatakan sebagai installation target ignored/generated |
| Pemeriksaan file | **Lulus** — draft existing yang belum tracked diperbarui dan satu laporan dibuat; tidak ada source aplikasi yang diubah |
| Review independen | **Lulus setelah revisi** — canonical ownership, thin wrapper, contract/SHA/approval guard, dan source-vs-install boundary konsisten |
| `dotnet build` | Tidak dijalankan — perubahan dokumentasi saja dan user sebelumnya meminta tidak melakukan build |
| Implementasi/validasi paket skill | Belum — di luar scope dokumen ini |
