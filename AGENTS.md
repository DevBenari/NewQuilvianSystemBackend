# Aturan Pengembangan Backend Quilvian

## Cakupan

File ini berlaku untuk seluruh repository `NewQuilvianSystemBackend`. Ini adalah aplikasi existing yang besar. Jaga agar setiap task memiliki cakupan sempit serta pertahankan batas domain, kontrak API, akses data, otorisasi, dan perilaku workflow yang sudah mapan.

Aturan utamanya adalah:

> Ikuti kode yang sudah ada. Jangan menciptakan arsitektur baru.

Untuk `NEW CODE`, pola source/referensi existing hanya merupakan bukti dan TIDAK BOLEH mengesampingkan Backend Engineering Contract canonical. Urutan wewenangnya adalah: (1) wewenang task/tulis eksplisit dan aturan keselamatan repository; (2) `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; (3) `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; (4) panduan operasional `agents/rules/` yang berlaku; (5) pola source/referensi existing. Pola legacy `Trx*`, controller yang mengakses DbContext secara langsung, Count/Max/Last+1, atau persisted `SortOrder` generik yang sebanding tidak memberi wewenang untuk menggunakan pola tersebut dalam `NEW CODE`. Terapkan legacy ratchet existing tanpa penulisan ulang massal.

Sebelum implementasi, periksa controller, DTO, model, service, penggunaan akses data, validasi, aturan otorisasi, workflow, konfigurasi EF, migration, dan endpoint terdekat yang sebanding sesuai kebutuhan.

## Lapisan Operasional Tata Kelola

`AGENTS.md` tetap menjadi konstitusi repository yang otoritatif. Lapisan operasionalnya tinggal di dalam repository ini pada folder `agents/rules/`. Baca dokumen berikut hanya ketika kondisinya berlaku:

- Setiap task implementasi: `agents/rules/TASK_RULES.md`
- Setiap implementasi aplikasi backend: `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- Klasifikasi dan pemilihan model: `agents/rules/TASK_CLASSIFICATION.md`
- Task lintas repository: `agents/rules/CROSS_REPO_RULES.md`
- Sebelum penyelesaian: `agents/rules/REVIEW_RULES.md`
- Handoff/laporan lokal: `agents/rules/REPORT_TEMPLATE.md`
- Pekerjaan API/controller/DTO/contract: `agents/rules/API_RULES.md`
- Pekerjaan entity/EF/database/migration: `agents/rules/DATABASE_RULES.md`

Folder `agents/rules/` berlaku untuk agent mana pun yang mengerjakan repository ini; tidak ada lokasi aturan khusus per vendor.

Dokumen-dokumen tersebut melengkapi, bukan menggantikan, aturan keselamatan, arsitektur, branch, keamanan, validasi, database, dan cakupan tulis khusus repository dalam file ini. Pertanyaan read-only sederhana tidak memerlukan pemuatan seluruh lapisan operasional.

## Pemeriksaan Awal Kontrak Rekayasa Backend

Sebelum mengubah source aplikasi backend, tentukan Area, Module, owner/prefix registry, applicability (`NEW CODE`, `TOUCHED LEGACY`, atau `LEGACY MIGRATION`), serta QBE rule ID yang berlaku dari kontrak canonical. Module/entity operasional baru tanpa entri registry yang disetujui berstatus `BLOCKED` berdasarkan `QBE-MOD-002`; jangan menyimpulkan prefix dari foldernya. Ikuti legacy ratchet: jangan melakukan refactor massal terhadap legacy yang tidak disentuh.

## Bahasa dan Komunikasi

- Gunakan Bahasa Indonesia untuk komunikasi dengan pengguna, termasuk pembaruan kemajuan, temuan audit, peringatan, rekomendasi, hasil validasi, ringkasan review, dan respons akhir.
- Istilah teknis, nama file, class, method, property, endpoint, route, field database, enum, command, HTTP status, nama framework/library, dan identifier source code tetap dalam bentuk aslinya. Source code dan identifier mengikuti konvensi repository existing; jangan menerjemahkan identifier hanya karena bahasa komunikasi menggunakan Bahasa Indonesia.
- Code comment mengikuti konvensi repository existing; jangan melakukan penerjemahan massal terhadap comment existing. Jika tool/compiler/runtime menghasilkan error dalam Bahasa Inggris, pertahankan error asli saat perlu dikutip lalu jelaskan maknanya dalam Bahasa Indonesia.
- Label kontrak/taxonomy canonical tetap menggunakan bentuknya, misalnya `READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, dan `UNKNOWN`; penjelasannya tetap dalam Bahasa Indonesia.
- Jangan beralih ke Bahasa Inggris kecuali pengguna secara eksplisit meminta bahasa lain atau output tertentu wajib memakai Bahasa Inggris karena kontrak/tooling.

## Identitas Repository dan Alur Kerja Branch

- Repository: `NewQuilvianSystemBackend`
- Branch development aktif ditentukan per module atau work item oleh pemegang modul yang tercatat dalam `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, atau melalui instruksi task/blueprint yang secara eksplisit telah disetujui oleh pemegang tersebut.
- Upstream yang diharapkan: `origin/<active-development-branch>`.
- Repository referensi frontend: `QuilvianSystemFrontendDev` (temukan dari konteks workspace yang diberi wewenang; laporkan dependency yang hilang alih-alih menebak path).

Sebelum implementasi aplikasi backend, verifikasi branch dan upstream backend saat ini menggunakan command Git read-only, wewenang task/tulis, keadaan Git yang bersih atau dapat dipulihkan, serta kesesuaiannya dengan branch yang ditetapkan pemegang modul atau task/blueprint yang disetujui. Jika penetapan branch yang otoritatif tidak tersedia, berhenti dan minta penetapannya. Jika branch atau upstream saat ini berbeda dari penetapan tersebut, ketidaksesuaian itu adalah blocker. Jangan menciptakan nama branch atau mengganti branch secara otomatis.

Untuk feature branch lokal yang baru dibuat, ketiadaan upstream sebelum publikasi pertama yang diberi wewenang adalah keadaan valid dan bukan blocker bagi implementasi, build, atau validasi lokal. Upstream menjadi relevan ketika task terpisah memberi wewenang publikasi. Untuk feature branch existing yang telah diberi wewenang, jangan otomatis melakukan switch, merge, atau rebase hanya untuk menyinkronkannya; periksa dan laporkan kebutuhan sinkronisasi kecuali task secara eksplisit memberi wewenang tindakan Git tersebut.

Implementasi feature backend normal harus mengikuti perlindungan branch tujuan yang ditetapkan pemegang modul atau task yang disetujui: feature branch yang diberi wewenang, implementasi dan validasi lokal, commit/push yang diberi wewenang secara terpisah, Pull Request bila diwajibkan branch tujuan, pemeriksaan QBE Strict GitRange yang diwajibkan, lalu merge ke branch tujuan yang ditetapkan. Jangan melemahkan persyaratan Pull Request, pemeriksaan QBE wajib, atau perlindungan branch tujuan yang berlaku.

Implementasi source dan publikasi Git adalah operasi terpisah. Kecuali diminta secara eksplisit, jangan melakukan commit, push, pull, merge, rebase, switch atau checkout branch, reset, force checkout, stash, cherry-pick, membuat pull request, atau deployment.

Untuk pekerjaan yang mencakup backend dan frontend, pemilihan branch serta wewenang tulis setiap repository mengikuti penetapan pemegang modul yang relevan dan target tulis eksplisit task. Nama branch agent yang tetap tidak boleh menggantikan kepemilikan modul atau memberikan wewenang tulis lintas repository.

## Platform Backend Saat Ini

- Jenis aplikasi: ASP.NET Core Web API
- Project SDK: `Microsoft.NET.Sdk.Web`
- Target framework: `.NET 9` (`net9.0`)
- Baseline SDK dari `global.json`: `9.0.316`, dengan roll forward ke patch terbaru
- Entity Framework Core dan ASP.NET Core Identity: `9.0.18`
- Provider PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL` `9.0.4`
- Solution utama: `QuilvianSystemBackend.sln`
- Project utama: `QuilvianSystemBackend.csproj`
- Komposisi aplikasi dan registrasi dependency: `Program.cs`
- DbContext aplikasi: `Repositories/ApplicationDbContext.cs`
- EF migration: `Migrations/`

Project test terpisah belum terdeteksi ketika aturan ini dibuat. Periksa kembali workspace sebelum menganggap test masih tidak tersedia.

Jangan melakukan upgrade SDK, target framework, package, atau dependency infrastruktur kecuali task secara eksplisit mengharuskannya.

## Peta Arsitektur yang Ada

- API root dan infrastruktur: `Controllers/`, `DTOs/`, `Models/`, `Services/`, `Repositories/`, `Responses/`, `Attributes/`, `Filters/`, `Middlewares/`, dan `Shared/`
- API domain: `Areas/`
  - `Areas/Administrator/`
  - `Areas/Corporate/`
  - `Areas/HealthServices/`
  - `Areas/SelfServices/`
- Domain Human Resource mencakup batas source yang sebenarnya di:
  - `Areas/Corporate/HumanResource/WorkforceCore/`
  - `Areas/Corporate/HumanResource/WorkforceProfileManagement/`
  - `Areas/Corporate/HumanResource/WorkflowManagement/`
  - `Areas/Corporate/HumanResource/LeaveManagement/`
  - `Areas/Corporate/HumanResource/OvertimeManagement/`
  - `Areas/Corporate/HumanResource/SchedulingManagement/`
  - `Areas/SelfServices/HumanResource/`
- DTO, model, controller, dan service biasanya ditempatkan di dalam domain pemilik ketika pola tersebut tersedia.
- Kontrak response shared mencakup `Responses/ApiResponse.cs` dan `Responses/PagedResult.cs`.

Hormati batas-batas tersebut. Jangan memindahkan class antardomain, membuat arsitektur root baru, atau memperkenalkan layer generik hanya karena terlihat lebih bersih. Repository saat ini menggunakan `ApplicationDbContext` secara langsung di banyak controller dan domain service; jangan menciptakan abstraksi repository ketika implementasi terdekat tidak menggunakannya.

## Konvensi Controller dan API

Sebelum membuat atau mengubah endpoint, periksa controller terdekat dan pertahankan konvensi yang sudah mapan, termasuk sesuai kebutuhan:

- `[ApiController]` dan `ControllerBase`;
- versioned route yang dimulai dengan `api/v1/...`;
- penamaan route Area/domain;
- HTTP verb dan route template;
- request binding dan validasi DTO;
- envelope sukses dan gagal `ApiResponse<T>`;
- bentuk pagination `PagedResult<T>`;
- status code, error message, filter, sort order, dan default pagination;
- tag Swagger;
- pola cancellation dan EF asynchronous; serta
- konvensi soft-delete dan status aktif.

Jangan menyimpulkan route dari URL frontend. Konfirmasikan route dan action controller yang sebenarnya terlebih dahulu.

## Disiplin Kontrak API

Requirement frontend tidak otomatis mendefinisikan ulang kontrak backend. Sebelum mengubah API, periksa route controller, action, HTTP verb, request DTO, response DTO, nilai enum atau status, validasi, otorisasi, perilaku pagination/filter, serta workflow atau aturan bisnis existing.

Pertahankan backward compatibility ketika memungkinkan. Jangan mengganti nama, menghapus, atau merusak endpoint, field, response envelope, nilai enum/status, atau action existing kecuali task secara eksplisit mengharuskan breaking change dan konsumennya telah dinilai.

Jika frontend dan backend tidak selaras, laporkan ketidaksesuaian. Selama pekerjaan backend, frontend dapat menjelaskan perilaku consumer saat ini, tetapi tidak otomatis mengesampingkan aturan bisnis atau keamanan backend.

## Aturan DTO, Model, dan Validasi

- Simpan kontrak transport di folder `DTOs/` milik domain ketika pola domain tersebut tersedia.
- Jangan mengekspos entity EF hanya untuk menghindari pembuatan response DTO yang sudah menjadi pola.
- Pertahankan perilaku nullable, tipe date/time, tipe identifier, default value, dan validation attribute.
- DTO existing umumnya menggunakan data annotation seperti `[Required]`, `[MaxLength]`, dan `[Range]`; periksa kontrak terdekat sebelum memilih perilaku validasi.
- Simpan persistence model di folder `Models/` milik domain dan pertahankan pola table/schema, base-model, relationship, audit, soft-delete, serta status aktif existing.
- Perlakukan perubahan entity, perubahan kontrak API, pembuatan migration, dan eksekusi database sebagai keputusan terpisah.

## Aturan Entity Framework dan Akses Data

Aplikasi menggunakan `Repositories/ApplicationDbContext.cs`, integrasi ASP.NET Core Identity, Entity Framework Core, dan PostgreSQL melalui Npgsql.

Sebelum mengubah perilaku persistence:

1. Periksa model yang relevan serta registrasi/konfigurasi `ApplicationDbContext`.
2. Periksa pola query dan mutation terdekat di controller atau service.
3. Pertahankan konvensi tracking versus `AsNoTracking`, relationship loading, transaction, concurrency, soft-delete, dan audit sesuai kebutuhan.
4. Hindari penulisan ulang query yang luas atau perubahan schema di luar domain yang diminta.

Jangan otomatis membuat, menghapus, mereset, atau menulis ulang migration. Pembuatan migration memerlukan wewenang task eksplisit. Menjalankan migration atau `Update-Database` terhadap database apa pun memerlukan instruksi eksplisit terpisah. Perubahan model tidak dengan sendirinya memberi wewenang untuk salah satu tindakan tersebut.

## Keselamatan Database

Jangan pernah menjalankan operasi database destruktif secara otomatis. Jangan melakukan drop database atau table, truncate business table, mass-delete record, reset migration, menimpa konfigurasi environment, atau memperbarui database production/shared kecuali diberi wewenang secara eksplisit dengan target yang jelas dan terbatas.

Jangan menjalankan command database hanya untuk memvalidasi perubahan source. Laporkan ketika validasi database masih tertunda.

## Aturan Otorisasi dan Pengguna Saat Ini

Pertahankan model keamanan existing. Periksa penggunaan sebenarnya dari:

- `[Authorize]`;
- metadata controller seperti `[AccessController]`;
- metadata action seperti `[AccessAction]` dan `[AccessPermission]`;
- `AccessTypes`;
- resolusi current-user dan claim yang terautentikasi;
- pemeriksaan role dan permission;
- kepemilikan self-service;
- otorisasi workflow actor dan delegated-actor; serta
- ketersediaan action yang dikembalikan backend.

Jangan pernah menyelesaikan masalah visibilitas frontend dengan melemahkan otorisasi backend. Jangan menerima identifier actor, workforce, atau user secara sembarang ketika pola self-service existing menurunkan kepemilikan dari pengguna yang terautentikasi.

## Alur Kerja dan Aturan Bisnis

Backend tetap otoritatif untuk transisi workflow, otorisasi actor, `AvailableActions`, approval dan rejection, validasi, transisi status, serta aturan bisnis domain.

Sebelum mengubah perilaku workflow, periksa controller pemilik, DTO, constant/enum status, lifecycle service, integration service, resolusi actor, penanganan idempotency, dan dampak downstream. Jangan memindahkan keputusan sensitif keamanan atau kritis bisnis menjadi asumsi frontend.

## Layanan dan Pemrosesan Latar Belakang

Gunakan pola domain service terdekat. Repository berisi domain service, query service, lifecycle/integration service, dan hosted scheduler service. Pertahankan pola registrasi dependency, transaction, retry, idempotency, logging, serta cancellation ketika relevan.

Jangan menjalankan hosted service, scheduler, atau aplikasi backend kecuali task secara eksplisit memerlukan eksekusi runtime.

## Keselamatan Rahasia dan Konfigurasi

Jangan pernah mengekspos atau menempatkan password, API key, connection string, token, private key, SMTP credential, user-secret value, maupun konfigurasi sensitif lainnya dalam laporan, source comment, respons agent, atau Git commit.

Ketika pemeriksaan konfigurasi diperlukan, laporkan struktur dan nama key saja. Jangan mencetak nilai secret dari `appsettings*`, user secrets, environment variable, konfigurasi deployment, atau material data-protection.

## Mode Pekerjaan Lintas Repository

Hanya repository yang secara eksplisit diberi wewenang oleh mode task aktif yang boleh diubah. Jika mode atau target tulis tidak ada atau ambigu, gunakan `AUDIT MODE` sebagai default.

### AUDIT MODE

- Backend: read-only.
- Frontend: read-only.
- Tidak ada perubahan source.

Gunakan untuk audit arsitektur, pemetaan gap, pemeriksaan kontrak API, analisis lintas repository, dan perencanaan.

### MODULE BLUEPRINT MODE

- Source aplikasi frontend: read-only.
- Source aplikasi backend: read-only.
- Repository skill: read-only.
- `docs/module-blueprints/**` backend: target tulis.

Gunakan mode ini hanya untuk membuat atau memperbarui artefak module-blueprint persisten, termasuk manifest, status, dokumentasi bisnis dan arsitektur berbasis bukti, decision, contract, dokumentasi ERD, Mermaid flow, roadmap, dan artefak readiness. Mode ini tidak pernah memberi wewenang implementasi aplikasi, perubahan controller/service/entity, migration atau tindakan database, perubahan dependency/package, deployment, maupun publikasi Git. Jangan mengarang keputusan bisnis atau mengubah source hanya agar sesuai dengan blueprint.

Kondisi masuknya adalah identitas/tujuan module yang jelas, backend sebagai host blueprint yang disetujui, dan cakupan tulis terbatas pada `docs/module-blueprints/**`. `AUDIT MODE` tetap sepenuhnya read-only; `BACKEND MODE` dan `FRONTEND MODE` tetap menjadi wewenang implementasi eksplisit yang terpisah.

### FRONTEND MODE

- Frontend: target tulis.
- Backend: source of truth yang strict read-only.

Backend boleh diperiksa untuk kontrak aktual dan perilaku bisnis, tetapi tidak ada file backend atau keadaan Git yang boleh diubah.

### BACKEND MODE

- Backend: target tulis.
- Frontend: referensi read-only.

Frontend boleh diperiksa untuk memahami caller saat ini, route, penanganan Redux, alur UI, penggunaan request, dan ekspektasi response. Jangan mengubah source atau konfigurasi frontend tracked.

Implementasi backend hanya diperbolehkan ketika prompt secara eksplisit mendeklarasikan `TASK MODE: BACKEND` atau menyebut backend sebagai target tulis eksplisit dalam `CROSS-REPO MODE`.

### CROSS-REPO MODE

Gunakan hanya untuk task lintas repository yang dikoordinasikan secara eksplisit. Ubah hanya repository yang secara eksplisit dideklarasikan sebagai target tulis. Jangan pernah menganggap kedua repository dapat ditulis. Utamakan perubahan berurutan backend-first atau frontend-first ketika memungkinkan.

## Sumber Kebenaran dan Batas Keselamatan Lintas Repository

Untuk task frontend, source backend saat ini bersifat otoritatif atas kontrak API dan perilaku bisnis backend. Untuk task backend, kode frontend merupakan referensi consumer dan tidak otomatis mengesampingkan aturan backend.

Jika task backend menemukan defect frontend, jangan diam-diam mengubah frontend. Laporkan. Jika task frontend menemukan defect backend, jangan diam-diam mengubah backend. Hentikan bagian tersebut dan laporkan masalah kecuali mode aktif secara eksplisit memberi wewenang atas repository tambahan.

## Kendali Cakupan

Jaga perubahan tetap memiliki cakupan ketat. Kecuali diminta secara eksplisit, jangan memformat atau mengganti nama file yang tidak terkait, menata ulang folder, melakukan upgrade dependency, menulis ulang arsitektur, memodernisasi source yang tidak terkait, atau memperbaiki warning yang tidak terkait.

Laporkan temuan di luar cakupan tanpa mengubahnya.

## Keselamatan Git

Agent tidak boleh otomatis melakukan commit, push, pull, merge, rebase, switch branch, reset, force checkout, stash, atau cherry-pick. Jangan melakukan stage file, membuat pull request, atau deployment kecuali task secara eksplisit meminta operasi terpisah tersebut.

Selalu periksa `git status --short` pada akhir implementasi dan bedakan perubahan yang dibuat task saat ini dari perubahan pengguna yang sudah ada sebelumnya. Jangan pernah membuang atau menimpa pekerjaan yang tidak terkait.

## Validasi Backend

Validasi harus proporsional terhadap cakupan yang diminta. Validasi dapat mencakup `dotnet build`, targeted test, atau test solution/project ketika diminta atau secara wajar diperlukan dan tidak dilarang task.

Sebelum menjalankan test, periksa apakah project test yang relevan tersedia. Jangan pernah melaporkan build atau test sebagai lulus kecuali command benar-benar selesai dengan sukses. Eksekusi migration database, deployment, dan infrastruktur runtime tetap merupakan operasi yang memerlukan wewenang terpisah.

## Alur Kerja Implementasi

Untuk task implementasi:

1. Ikuti referensi governance kondisional di atas, dimulai dari `agents/rules/TASK_RULES.md`.
2. Jaga task tetap terbatas dan terapkan seluruh aturan khusus repository dalam file ini.
3. Laporkan file yang berubah, validasi aktual, keadaan migration, risiko, dan `git status --short`.

Jangan melakukan stage, commit, atau push kecuali diminta secara eksplisit.

## Pelaporan Task Modul

Setelah task roadmap selesai diimplementasikan dan divalidasi, buat atau perbarui satu laporan tracked pada modul yang dikerjakan:

- backend: `docs/module-blueprints/<module-slug>/task/report/backend/<TASK-ID>.md`;
- frontend: `docs/module-blueprints/<module-slug>/task/report/frontend/<TASK-ID>.md`.

Pertahankan task ID persis seperti roadmap. Bila task dikerjakan ulang, perbarui file yang sama. Laporan tracked tersebut menjadi satu-satunya artefak laporan task; jangan membuat handoff atau laporan sesi terpisah. Task frontend hanya memperoleh wewenang lintas repository yang sempit untuk laporan frontend dan tautan buktinya pada roadmap serta `requirement-traceability.md` modul yang sama, bukan untuk source backend atau artefak blueprint lain.

Catat acceptance criteria dan bukti seperti domain, controller, endpoint, HTTP method, request dan response DTO, enum/status, otorisasi, perilaku bisnis/workflow, file yang berubah, hasil build/test aktual, keadaan migration, risiko, dan status Git. Jangan menyertakan secret.

## Kebutuhan yang Tidak Jelas

Ketika jawaban dapat ditetapkan dari source repository, periksa source sebelum bertanya. Minta klarifikasi hanya ketika keputusan contract, keamanan, database, atau bisnis yang sebenarnya tidak dapat ditentukan secara aman. Jangan pernah mengarang aturan bisnis.
