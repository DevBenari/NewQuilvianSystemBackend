# BE-ACC-002 — Audit mekanisme hak akses badan hukum

- **TASK ID:** `BE-ACC-002` — Audit mekanisme hak akses badan hukum
- **TASK TYPE:** Audit read-only, tanpa perubahan source aplikasi
- **COMPLEXITY:** `LIGHT`
- **CLASSIFICATION SCORE:** rendah — nol source aplikasi, nol entity, nol endpoint, nol migration
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `docs/module-blueprints/accounting/**` saja
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 5 `APPROVED`, decision revision 1.2, roadmap revisi 2 `APPROVED`
- **HEAD saat mulai:** `ca6b7e0` pada branch `rizkiG`

## Validasi baseline sebelum mulai

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `5` | `5` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |
| Backend source SHA | `aa837d7` | `ca6b7e0` | **Berbeda — diperiksa** |
| Frontend source SHA | `31a82c8` | `5336c44` | **Berbeda — tidak relevan** |

Kedua selisih SHA ditelusuri lebih dahulu sesuai aturan, sebelum satu langkah pun dikerjakan.

**Backend.** `git diff --name-only aa837d7 ca6b7e0` menghasilkan **28 berkas, seluruhnya di
`docs/module-blueprints/accounting/`**. Nol berkas source aplikasi. `ca6b7e0` adalah commit yang
memasukkan blueprint Accounting itu sendiri. Jadi source aplikasi pada `ca6b7e0` **identik** dengan
baseline `aa837d7`, dan seluruh bukti yang tercatat terhadap `aa837d7` tetap berlaku apa adanya.

**Frontend.** `31a82c8` terbukti **leluhur langsung** dari `5336c44` (`git merge-base --is-ancestor`
bernilai benar), jadi perpindahannya fast-forward murni. Task ini backend read-only dan tidak
menyentuh frontend sama sekali, sehingga tidak ada area terdampak yang perlu dihentikan.

**Kesimpulan:** tidak ada impact scan yang perlu dijalankan. Task boleh berjalan.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | Tidak berlaku — task ini tidak menyentuh source |
| Pemilik / prefix registry | Rizki / `Acc` — terdaftar, lifecycle `ACTIVE` |
| Applicability | **Tidak berlaku** — nol source aplikasi dibuat maupun disentuh |
| QBE ID yang berlaku | **Tidak ada.** `QBE-MOD-002` dan `QBE-MOD-003` mengatur pembuatan model persisted; task ini tidak membuat satu pun |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `QuilvianEngineeringSkills/agents/rules/backend/engineering/` |

### Catatan provenance governance

Skill yang termuat di sesi ini adalah versi terpasang `0.1.0`, dan teksnya masih menunjuk `.codex`
serta `docs/engineering/` repository target — keduanya sudah tidak berlaku. Versi di repo sumber
`QuilvianEngineeringSkills` sudah menunjuk `rules/backend/engineering/` milik suite.

Plugin cache terpasang **tidak memuat** `rules/GLOBAL_RULES.md` maupun `rules/backend/engineering/`
pada kedua versi (`0.1.0` dan `1.0.0`). Governance dibaca langsung dari repo sumber
`QuilvianEngineeringSkills`, yang merupakan hulu plugin tersebut — bukan dari sumber lain dan
bukan dari default agent.

Status `BLOCKED — canonical governance unavailable` **tidak** dikembalikan, karena dokumen yang
diwajibkan memang terbaca; yang bermasalah adalah saluran distribusinya. Ini dilaporkan sebagai
temuan pada `evidence/03-acc-dep-007-governance-propagation.md` bagian 10.

## FILES INSPECTED

**Source aplikasi (baca saja):** `Models/SysAccessPolicy.cs`; `Models/ApplicationUser.cs`;
`Models/ApplicationUserOrganization.cs`; `Services/Security/AccessPermissionService.cs`;
`Filters/AccessPermissionFilter.cs`; `Attributes/AccessControllerAttribute.cs`;
`Attributes/AccessPermissionAttribute.cs`; `Constants/AccessTypes.cs`;
`Controllers/AuthController.cs`;
`Areas/Corporate/HumanResource/MasterData/Organization/Models/MstLegalEntity.cs`;
`.../Models/MstDepartment.cs`; `.../Controllers/CostCenterController.cs`;
`.../Controllers/LegalEntityController.cs`.

**Governance dan tooling (baca saja):** `AGENTS.md`;
`tooling/qbe/Invoke-QbeConformanceCheck.ps1`; `.github/workflows/qbe-conformance.yml`;
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` pada suite skill dan pada
`origin/QuilvianIntegrationBackend`.

**Blueprint:** `blueprint-manifest.md`; `MODULE-STATUS.md`; `05-prerequisite-readiness.md`;
`04-prd-to-mvp.md` bagian 20; `contracts/permission-audit-matrix.md` bagian 5;
`roadmap/backend-roadmap.md`; `roadmap/requirement-traceability.md`;
laporan `BE-ACC-001`.

## FILES CHANGED

Dua berkas dibuat, lima berkas blueprint diperbarui. **Nol berkas source aplikasi disentuh.**

| Berkas | Jenis |
|---|---|
| `docs/module-blueprints/accounting/evidence/02-legal-entity-authority.md` | **Baru** — deliverable `BE-ACC-002` |
| `docs/module-blueprints/accounting/evidence/03-acc-dep-007-governance-propagation.md` | **Baru** — laporan `ACC-DEP-007` untuk lead |
| `docs/module-blueprints/accounting/05-prerequisite-readiness.md` | Diperbarui — `ACC-DEP-007` dikoreksi, `ACC-DEP-008` ditambahkan |
| `docs/module-blueprints/accounting/04-prd-to-mvp.md` | Diperbarui — status dua pertanyaan memblokir |
| `docs/module-blueprints/accounting/roadmap/backend-roadmap.md` | Diperbarui — `BE-ACC-001` dan `BE-ACC-002` `DONE` |
| `docs/module-blueprints/accounting/roadmap/requirement-traceability.md` | Diperbarui — `GAP-ACC-005` |
| `docs/module-blueprints/accounting/MODULE-STATUS.md` | Diperbarui — fase, delivery state, blocker |

Ditambah `blueprint-manifest.md` untuk menyesuaikan hash artefak yang berubah.

## IMPLEMENTATION

Tidak ada implementasi. Task ini audit.

Penelusuran dijalankan sebagai lima pemeriksaan yang saling menutup, dirancang supaya kesimpulan
"tidak ada" tidak dapat disebabkan oleh kelalaian mencari:

| # | Yang diperiksa | Hasil |
|---:|---|---|
| 1 | Tabel kebijakan hak akses punya dimensi badan hukum? | Tidak — hanya `DepartmentId`, `PositionId`, controller, action |
| 2 | Ada jalur data pengguna ke badan hukum? | Tidak — 6 tabel diperiksa seluruh kolomnya, termasuk arah sebaliknya |
| 3 | Token login membawa badan hukum? | Tidak — 20 klaim, nol menyebut badan hukum |
| 4 | Badan hukum diturunkan dari identitas saat kueri? | Tidak — 9 controller mengambilnya dari URL, 17 dari isian, **0** dari identitas |
| 5 | Ada penyaringan tingkat baris di lapisan EF? | Tidak — nol `HasQueryFilter` di seluruh repository |

Pemeriksaan 5 sengaja ditambahkan supaya kesimpulan tidak salah: seandainya ada
`HasQueryFilter`, penyaringan bisa terjadi diam-diam di lapisan bawah dan seluruh temuan 1–4
akan menyesatkan. Ternyata tidak ada.

## BLUEPRINT STATUS/EVIDENCE

Bukti utama: `evidence/02-legal-entity-authority.md`, 13 bagian, memuat path berkas, nama kelas,
nama method, nomor baris, dan lima perintah yang dapat dijalankan ulang siapa pun untuk
memverifikasi setiap temuan.

## API CONTRACT IMPACT

`NONE`. Tidak ada controller, endpoint, maupun DTO yang dibuat atau diubah.

Catatan untuk kontrak yang sudah ada: `ACC-API-0.1` mengandaikan penolakan `403` atas badan hukum
yang bukan hak pengguna. Andaian itu **belum punya dasar teknis**. Kontraknya tidak diubah — ia
tetap target yang sah — tetapi acceptance-nya tidak dapat dirumuskan sampai `ACC-DEP-008`
terselesaikan.

## DATABASE IMPACT

`NONE`. Diverifikasi satu per satu:

| Pemeriksaan | Hasil |
|---|---|
| Berkas di `Models/` Accounting | Tidak ada (tidak berubah sejak `BE-ACC-001`) |
| `Migrations/` berubah | Tidak |
| `Migrations/ApplicationDbContextModelSnapshot.cs` berubah | Tidak |
| `Repositories/ApplicationDbContext.cs` berubah | Tidak |
| `Program.cs` berubah | Tidak |
| `dotnet ef migrations add` dijalankan | Tidak |
| `dotnet ef database update` dijalankan | Tidak |
| Shared database disentuh | Tidak |

## SECURITY IMPACT

Tidak ada perubahan keamanan yang dibuat. Task ini justru **menemukan** persoalan keamanan yang
sudah ada sebelumnya.

`LegalEntityId` berfungsi sebagai penyaring yang dikirim pengguna, bukan sebagai batas kewenangan.
Pengguna yang punya hak baca sebuah menu dapat mengirim badan hukum mana pun, atau menghilangkan
parameternya dan memperoleh seluruh badan hukum.

Cakupannya **lebih luas dari Accounting**: 40 model menyimpan `LegalEntityId`, sebagian besar
milik Human Resource, termasuk `MstSalaryStructure`, `MstPayrollPeriod`, dan `TrxExpenseClaim`.

Ini dilaporkan sebagai temuan, **bukan** sebagai penilaian tingkat keparahan. Menilai risikonya
dan menentukan penanganannya adalah wewenang owner keamanan platform. Tidak ada perbaikan yang
dikerjakan dari sisi Accounting, sesuai batas scope task.

## VISUAL REFERENCE

`NOT REQUIRED`.

## VALIDATION

Task read-only, jadi validasinya adalah keterulangan bukti, bukan build.

| Pemeriksaan | Hasil |
|---|---|
| 17 hash artefak canonical dihitung ulang | **17/17 cocok** |
| Selisih SHA backend ditelusuri | 28 berkas, seluruhnya dokumentasi |
| Selisih SHA frontend ditelusuri | Fast-forward murni, tidak relevan untuk task backend |
| Checker QBE dijalankan | `TOOL ERROR: Canonical governance missing`, exit 2 — konsisten dengan `ACC-DEP-007` |
| Lima perintah verifikasi ulang di bukti | Dijalankan, hasilnya sesuai yang ditulis |
| Source aplikasi berubah | **Nol berkas** |

`dotnet build` dan `dotnet test` **tidak** dijalankan, dan memang tidak diperlukan: nol berkas
`.cs` disentuh, sehingga hasilnya dijamin identik dengan `BE-ACC-001` (0 error, 956 test lulus).
Menjalankannya hanya akan menghasilkan bukti yang menyesatkan seolah ada kode yang diuji.

## Acceptance criteria

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | Berkas evidence berisi path dan simbol nyata beserta SHA | ✅ | 13 bagian, 5 temuan bernomor baris, SHA `ca6b7e0` tercatat beserta alasan perbedaannya dari `aa837d7` |
| 2 | Menyatakan tegas apakah mekanismenya ada, atau belum ada dan perlu keputusan owner keamanan | ✅ | Bagian 2 dan 13: **tidak ada**, `MISSING`, milik owner keamanan platform |

## Definition of Done

| Butir | Hasil |
|---|:---:|
| Evidence tertulis | ✅ `evidence/02-legal-entity-authority.md` |
| Pertanyaan memblokir pada PRD ke MVP diperbarui statusnya | ✅ `04-prd-to-mvp.md` bagian 20 |
| Register diperbarui | ✅ roadmap backend + requirement traceability |

## WARNINGS

Tidak ada warning build, karena tidak ada build yang dijalankan.

## KNOWN ISSUES

### 1. `ACC-DEP-008` — mekanisme hak akses badan hukum tidak ada

Temuan utama task ini. Menahan `BE-ACC-007` sampai `BE-ACC-014`. Milik owner keamanan platform.
Berlaku sistem-luas, bukan khusus Accounting.

### 2. `ACC-DEP-007` — akar masalahnya ternyata berbeda dari yang tercatat

Catatan sebelumnya menyebut `4db8909` sebagai akarnya. Itu benar untuk 28 Agustus, tetapi
perbaikannya **sudah pernah masuk** lewat PR #63 (`c9692d0`, 31 Agustus) dan **dibatalkan** merge
`3d14cac` (1 September), lalu dibawa kembali ke integration oleh PR #68.

Pembatalan itu terbukti bukan kecelakaan: versi "kita" identik dengan versi "dasar", sehingga git
seharusnya mengambil versi "mereka" tanpa konflik sama sekali. Hasilnya justru kembali ke versi
lama.

Dampaknya menguntungkan: perbaikannya jauh lebih murah dari yang diduga — cukup terapkan ulang
lima baris path pada checker. Laporan lengkap di
`evidence/03-acc-dep-007-governance-propagation.md`. **Milik lead; tidak ditambal dari sini.**

### 3. Suite skill terpasang tertinggal dari repo sumbernya

Kedua versi plugin (`0.1.0` dan `1.0.0`) tidak memuat `rules/GLOBAL_RULES.md` maupun
`rules/backend/engineering/`. Versi `1.0.0` yang lebih baru justru memuat lebih sedikit. Agent
yang hanya membaca plugin cache akan menyimpulkan prefix `Acc` belum terdaftar dan memblokir task
Accounting secara keliru — persis yang sempat terjadi pada sesi `BE-ACC-001`.

### 4. Utang teknis yang sudah ada sebelumnya, tidak diperbaiki

Sesuai instruksi owner, dicatat tetapi tidak disentuh, dan diklasifikasikan
**pre-existing platform/technical debt**:

- sisa folder `agents/rules/` masih terlacak git di backend (7 berkas) padahal `AGENTS.md`
  baris 53 menyatakan sudah dicabut;
- dua project bernama `QuilvianSystemBackend.Tests` di solution (akar 815 test, `Tests/` 141 test).

### 5. Penomoran berkas evidence

Folder `evidence/` kini memuat dua berkas berawalan `02-`:
`02-frontend-rebaseline-impact-scan.md` dan `02-legal-entity-authority.md`. Nama kedua ditulis
persis seperti yang dikunci roadmap `BE-ACC-002`, jadi **tidak** diubah sepihak. Bila owner ingin
dirapikan, itu perubahan roadmap, bukan perubahan task ini.

## MANUAL TEST

`NOT APPLICABLE` — tidak ada perilaku runtime yang berubah.

## INCIDENTAL CHANGES

`NONE`. Tidak ada perubahan sampingan pada source aplikasi.

## INTERRUPTIONS

`NONE`.

## GIT STATUS

```
?? Areas/Corporate/AccountingManagement/                  (dari BE-ACC-001, belum di-commit)
?? Tests/QuilvianSystemBackend.Tests/AccountingManagement/ (dari BE-ACC-001, belum di-commit)
 M docs/module-blueprints/accounting/04-prd-to-mvp.md
 M docs/module-blueprints/accounting/05-prerequisite-readiness.md
 M docs/module-blueprints/accounting/MODULE-STATUS.md
 M docs/module-blueprints/accounting/blueprint-manifest.md
 M docs/module-blueprints/accounting/roadmap/backend-roadmap.md
 M docs/module-blueprints/accounting/roadmap/requirement-traceability.md
?? docs/module-blueprints/accounting/evidence/02-legal-entity-authority.md
?? docs/module-blueprints/accounting/evidence/03-acc-dep-007-governance-propagation.md
?? docs/module-blueprints/accounting/task/
```

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

Dua hal, dan **keduanya bukan pekerjaan coding**:

1. **Teruskan `evidence/03-acc-dep-007-governance-propagation.md` ke lead.** Perbaikannya lima
   baris dan sudah pernah ditinjau; yang dibutuhkan hanya keputusan siapa menerapkannya kembali.
2. **Teruskan `evidence/02-legal-entity-authority.md` ke owner keamanan platform.** Ia yang
   menentukan mekanismenya. Accounting tidak membuat sendiri.

Sesudah itu, task backend berikutnya yang sah dikerjakan adalah **`BE-ACC-003`** — entity daftar
akun dan jenis jurnal. Ia `EXECUTION_READY` dan **tidak** tertahan `ACC-DEP-008`, karena
menyimpan kolom `LegalEntityId` berbeda dari menegakkannya.

**Menunggu instruksi eksplisit owner.** Approval roadmap bukan perintah jalan.
