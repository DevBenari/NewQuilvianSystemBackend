# Blueprint Manifest — Bank Darah

```yaml
blueprint_id: BD-BP-001
module_name: Bank Darah
module_slug: bank-darah
module_prefix: BD
revision: 18
status: PARTIAL
current_phase: BD-PH-005
created_at: 2026-09-02T00:40:53+07:00
updated_at: 2026-09-03T17:00:00+07:00
last_verified_at: null
backend_source_sha: ed7fba82efb850e11bfa3b8968d5f75d01280205
backend_branch: sukmagp
frontend_source_sha: afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254
frontend_branch: sukmagpV2
skill_suite_version: 1.6.0
input_revision_hash: design-business-module-role-residue-2026-09-03
decision_revision: 9
capability_map_revision: 3
capability_map_status: CURRENT
capability_map_full_audit_sha: 9522caacf29371b1fddd1584e9a71ad94fe48d19
capability_map_impact_scan_sha: 4205d18a6d656555eedd781f14e8a18fb5ea20d1
capability_map_impact_scan_result: nol baris berpindah status; blueprint tidak perlu diubah
prerequisite_readiness_revision: 3
completeness_assessment_revision: 2
domain_architecture_revision: 6
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
closed_gap_ids:
  - ARCH-BD-GAP-01
  - ARCH-BD-GAP-02
  - ARCH-BD-GAP-03
  - ARCH-BD-GAP-04
  - ARCH-BD-GAP-05
  - ARCH-BD-GAP-06
  - ARCH-BD-GAP-07
  - ARCH-BD-GAP-08
  - ARCH-BD-GAP-09
  - ARCH-BD-GAP-10
  - OQ-BD-015
  - DEF-BD-004
contract_versions:
  - version: v1
    status: superseded
    superseded_by: v2
  - version: v2
    status: superseded
    superseded_by: v3
  - version: v3
    status: superseded
    superseded_by: v4
  - version: v4
    status: draft
    last_changed_in: v4
    covers:
      - 02-backend-architecture.md
      - 03-frontend-architecture.md
      - 04-prd-to-mvp.md
      - data/data-dictionary.md
      - contracts/api-contract.md
      - contracts/state-transition-matrix.md
      - contracts/validation-matrix.md
      - contracts/integration-contract.md
      - contracts/permission-audit-matrix.md
      - flowcharts/
      - testing/acceptance-test-matrix.md
owners:
  product_domain: pemilik proses BDRS
  api: pemilik arsitektur backend
  security: pemilik keamanan platform
  frontend: pemilik proses BDRS
approved_by: null
approved_at: null
resolved_dependency_ids:
  - BD-DEP-008
active_dependency_ids:
  - BD-DEP-001
  - BD-DEP-002
  - BD-DEP-003
  - BD-DEP-004
  - BD-DEP-005
  - BD-DEP-006
  - BD-DEP-007
  - BD-DEP-009
  - BD-DEP-010
  - BD-DEP-011
  - BD-DEP-012
  - BD-DEP-013
  - BD-DEP-014
  - BD-DEP-015
  - BD-DEP-016
active_roadmap_revision: 2
roadmap_status: FORWARD-TEST
supersedes: null
```

## Penjelasan isi manifest

Manifest ini adalah kartu identitas modul Bank Darah. Ia menjawab satu pertanyaan: versi keputusan
mana yang sedang berlaku, dan atas dasar source code versi berapa keputusan itu dibuat.

| Field | Arti dalam bahasa sehari-hari |
| --- | --- |
| `blueprint_id` | Nomor identitas blueprint. Ditetapkan sekali dan tidak pernah diganti. |
| `module_prefix` | Awalan `BD` dipakai untuk penomoran keputusan, fase, dependency, dan task blueprint. **Bukan** awalan penamaan entity backend: awalan itu **`Bbk`**, terpisah, dan sejak 3 September 2026 **sudah terdaftar** di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dengan Lifecycle `PLANNED`. |
| `revision` | Naik hanya bila arsitektur target, kontrak, dependency, atau keputusan yang sudah disetujui berubah secara berarti. Tidak naik hanya karena status berubah. |
| `status` | `PARTIAL` berarti sebagian slice sudah siap dirancang sementara slice lain terblokir keputusan bisnis. |
| `current_phase` | Fase yang sedang berjalan, yaitu `BD-PH-005` Penyusunan Blueprint Target. |
| `last_verified_at` | Masih kosong karena belum ada verifikasi kesiapan yang dijalankan. |
| `backend_source_sha` | Versi source backend yang menjadi dasar seluruh keputusan di blueprint ini. Naik `9522caa` → `9dc7637` → `db08c14` → `792acb9` → `ab39b63` → `a9bc9fd` → **`4205d18`**. Enam langkah pertama seluruhnya docs-only dan sudah diverifikasi `git diff --name-only`. **Langkah terakhir berbeda:** `4205d18` adalah merge `QuilvianIntegrationBackend` ke `sukmagp` yang membawa **perubahan source aplikasi nyata**. Bukti kemampuan sempat ditandai `STALE`, lalu **impact scan terbatas dijalankan pada 3 September 2026 dan penandanya dicabut** — nol baris berpindah status. Rinciannya di `02-existing-capability-map.md` §Impact scan terbatas. |
| `input_revision_hash` | Menunjuk asal keputusan: sesi wawancara Grill Me architecture gap final closure pass tanggal 2 September 2026, yang melanjutkan scope pass, closure pass, dan architecture gap closure pass di hari yang sama. |
| `closed_gap_ids` | Daftar gap arsitektur dan pertanyaan terbuka yang sudah ditutup keputusan pemilik. `ARCH-BD-GAP-01`..`09` ditutup `DEC-BD-025`..`034`; `ARCH-BD-GAP-10` ditutup `DEC-BD-037`; `OQ-BD-015` ditutup `DEC-BD-038`. **Tidak ada gap arsitektur yang masih terbuka.** |
| `roadmap_status` | Kembali `FORWARD-TEST` pada roadmap **revisi 2** (3 September 2026), yang disusun ulang di atas set kontrak `v4`. Revisi 1 ditandai `STALE` dan digantikan, bukan ditambal. |
| `decision_revision` | Naik ke `8` pada Role & authority closure pass 3 September 2026. `DEF-BD-004` ditutup `DEC-BD-039` sampai `DEC-BD-041`. **Tidak ada lagi keputusan bisnis yang memblokir.** Pemblokir tersisa dua dan keduanya tindakan manusia: `G1` approval kontrak `v4`, dan `G2b` keputusan aktivasi modul dari `PLANNED` ke `ACTIVE`. |
| `contract_versions` | Set kontrak desain **`v4`** berstatus `draft`, hasil design-business-module update pass 3 September 2026 yang menyerap role residue closure. `v1` sampai `v3` ditandai `superseded`. Set kontrak berlaku sebagai **satu himpunan**; seluruh berkas yang dicakup ikut naik ke `v4`, kecuali `contracts/integration-contract.md` yang `last_changed_in`-nya tetap `v2` karena isinya memang tidak bergerak. |
| `owners` | Pemilik per sumbu (product/domain, API, security, frontend). `approved_by`/`approved_at` kosong: desain masih `draft`. |
| `supersedes` | Kosong karena blueprint ini tidak menggantikan blueprint lain. |

## Peringatan yang melekat

Audit kemampuan existing sudah dijalankan pada 2 September 2026 dan hasilnya ada di
`02-existing-capability-map.md`. Peringatan "scope dikunci tanpa audit" **sudah dicabut**.

Peta kemampuan itu terikat pada backend `9522caa` dan frontend `afbb8ab`. Backend sudah bergerak ke
`db08c14` (lewat `9dc7637`), dan pemindaian dampak terbatas sudah dijalankan pada 2 September 2026:
seluruh perbedaannya hanya dokumen blueprint Bank Darah, nol berkas source aplikasi. Peta **tidak**
ditandai `STALE`. Bila SHA berubah lagi, ulangi pemindaian yang sama sebelum peta dipakai.

Blueprint tidak memberi wewenang implementasi. Menulis dokumen di sini tidak sama dengan izin
mengubah controller, service, entity, migration, database, atau melakukan deployment.

### Catatan pass bertanggal

> Blok di bawah adalah **rekaman historis per pass**, bukan keadaan sekarang. Setiap entri benar pada
> tanggalnya. Keadaan terkini ada pada field YAML di atas dan pada catatan rekonsiliasi paling bawah.
> Khususnya: rujukan `BD-DEP-008` sebagai pemblokir dan `Bbk` sebagai placeholder mencerminkan keadaan
> sebelum 3 September 2026; keduanya **sudah tidak berlaku**.

**Design pass 2 September 2026.** `design-business-module` menghasilkan set kontrak `v1` (`draft`) pada
`02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `data/`, `contracts/`,
`flowcharts/`, dan `testing/`. Seluruhnya memakai **prefix placeholder `Bbk`** yang belum disahkan
(`BD-DEP-008`), sehingga pembuatan entity operasional tetap `BLOCKED`. `04-prd-to-mvp.md` masih memuat
pertanyaan memblokir (`BD-DEP-008`, `DEF-BD-004`) sehingga **belum boleh** diteruskan ke
`plan-module-delivery` sampai keduanya tuntas. Approval manusia belum diklaim.

**Design update pass 2 September 2026 — set kontrak `v2`.** Menyerap empat keputusan Storage Location
dan gerbang pemberian (`DEC-BD-035` sampai `DEC-BD-038`) dari register keputusan revisi 7 dan arsitektur
domain revisi 6.

Yang bertambah pada desain: master `MstBloodStorageLocation`, entity `BbkBloodUnitPlacement`, dua nilai
status kantong di depan (`Received`, `Stored`), kolom `BbkBloodUnit.CurrentPlacementId`, kolom
`BbkEmergencyAuthorization.BypassScope` beserta enum `BbkEmergencyBypassScope`, action hak akses
`Store`, resource `BloodStorageLocation`, layar `FE-BD-10`, epic `EPIC BD-11`, dan gelombang `MVP-1b`.

Yang **tidak** bertambah, dan itu disengaja: nol batas integrasi baru, nol daftar kerja operasional
baru, nol background job, dan nol perubahan pada batas Billing.

Dua catatan yang mengikat pembaca berikutnya:

- **`MstDrugStorageLocation` sengaja tidak dipakai ulang** walaupun sudah ada dan punya tipe
  `ColdStorage`. Penolakannya `DEC-BD-035`, dan alasannya dicatat pada `02-backend-architecture.md` §D
  dan §L serta `contracts/integration-contract.md` §1b — supaya tidak tersambung tanpa sengaja nanti.
- **Master lokasi penyimpanan wajib terisi sebelum go-live.** Tanpa satu pun lokasi aktif, tidak ada
  kantong yang dapat disimpan, dialokasikan, maupun diberikan (`INV-BD-025`). Ini konsekuensi
  *fail-closed* yang disengaja, bukan cacat rancangan.

`roadmap/00-delivery-plan.md` **ditandai `STALE`**: ia disusun sebelum Storage Location masuk. Kedua
pemblokir lama (`BD-DEP-008`, `DEF-BD-004`) **tidak berubah** oleh pass ini; keduanya tetap menahan
penerusan ke `plan-module-delivery`. Approval manusia belum diklaim.

**Role & authority closure pass 3 September 2026 — register keputusan revisi 8.** `DEC-BD-039` sampai
`DEC-BD-041` menutup `DEF-BD-004`, pemblokir bisnis terakhir. Sejak pass ini **tidak ada satu pun
keputusan bisnis yang masih memblokir** `DESIGN` maupun `IMPLEMENTATION` pada scope yang dinilai;
pemblokir yang tersisa hanya `BD-DEP-008`, pendaftaran prefix entity di registry — administratif, bukan
keputusan bisnis.

Yang ditetapkan: validasi hasil golongan darah dipecah menjadi dua wewenang (rutin oleh petugas BDRS
berwenang validasi, konflik oleh validator klinis yang ditunjuk); otorisasi darurat dapat diterbitkan
Dokter BDRS **atau** DPJP pasien dengan kelengkapan rekam yang wajib; dan koreksi pencatatan pemberian
menjadi proses **dua tahap** — petugas BDRS mengajukan, Dokter BDRS menyetujui.

**Design update pass 3 September 2026 — set kontrak `v3`.** Menyerap `DEC-BD-039`, `DEC-BD-040`, dan
`DEC-BD-041` dari register keputusan revisi 8. Peringatan sinkronisasi yang dicatat pada revisi 11
**sudah dijawab**: seluruh artefak kini menggambarkan koreksi sebagai proses dua tahap.

Yang bertambah pada desain: dua butir hak akses (`BloodGroupExam : ResolveConflict`,
`BloodUnit : ApproveCorrection`), dua enum (`BbkEmergencyAuthorizerRole`, `BbkCorrectionStatus`), dua
kolom wajib pada `BbkEmergencyAuthorization` (peran penerbit dan kondisi kedaruratan), lifecycle beserta
enam kolom pada `BbkIssuanceCorrection`, tiga endpoint koreksi menggantikan satu, dan lima kewajiban
layar baru (`FE-BD-016` sampai `FE-BD-019`).

Yang **tidak** bertambah: nol entity baru, nol migration baru, nol tabel baru. Perubahan `v3` menyentuh
dua tabel yang belum pernah dibuat, sehingga seluruhnya larut ke migration yang sudah direncanakan.

Temuan pass itu — `DEF-BD-004` baru tertutup tiga dari enam wewenang — **sudah ditindaklanjuti**; lihat
catatan berikutnya.

**Role residue closure pass 3 September 2026 — register keputusan revisi 9.** `DEC-BD-042`,
`DEC-BD-043`, dan `DEC-BD-044` menutup sisa `DEF-BD-004`. Dengan itu **keenam wewenangnya sudah
dipetakan**, dan tidak ada satu pun keputusan bisnis yang masih memblokir pada scope yang dinilai.

Yang ditetapkan: bukti kecocokan dinyatakan petugas BDRS berwenang validasi, dengan pelaksana
pemeriksaan **boleh** berbeda dari validator (diizinkan, tidak diwajibkan); penyelesaian `PendingReview`
dipecah menjadi **tiga butir hak akses terpisah** menurut arah risikonya, bukan satu `Resolve` global;
dan pembatalan order darah dapat dilakukan dokter peminta maupun petugas BDRS, keduanya wajib beralasan
terkendali dan berjejak.

Peringatan sinkronisasi pada revisi 13 **sudah dijawab** oleh set kontrak `v4`; lihat catatan berikut.

**Design update pass 3 September 2026 — set kontrak `v4`.** Menyerap `DEC-BD-042`, `DEC-BD-043`, dan
`DEC-BD-044` dari register keputusan revisi 9. Dengan ini seluruh rangkaian keputusan Bank Darah —
`DEC-BD-001` sampai `DEC-BD-044` — sudah turun ke kontrak.

Yang bertambah pada desain: satu kolom dan satu penggantian nama pada `BbkCompatibilityEvidence`
(`EvidenceResult`, `CheckedByUserId` → `ValidatedByUserId`), enum `BbkCompatibilityResult`, empat butir
hak akses (`ResolveReallocate`, `ResolveReturn`, `ResolveNotUsable`, `BloodOrder : Cancel`), dua
kategori alasan pembatalan pada `MstBloodBankReason`, enam kode validasi (`VAL-BD-078`..`083`), dan dua
kewajiban layar (`FE-BD-020`, `FE-BD-021`).

Yang **dihapus**, dan ini perlu diperhatikan seeder: butir `BloodUnit : Resolve` **tidak lagi dipakai**.
Membiarkannya hidup berdampingan dengan ketiga penggantinya akan menjadi jalan pintas yang membatalkan
pemisahan `DEC-BD-043`.

Yang **tidak** bertambah: nol entity baru, nol tabel baru, nol migration baru, nol endpoint baru.
`DEC-BD-043` seluruhnya berupa pergantian penjaga pada endpoint yang sudah ada.

⚠️ **Satu pengetatan gerbang yang belum ditegaskan pemilik proses.** `DEC-BD-042` menuntut hasil
keputusan bukti kecocokan tersimpan. Menyimpannya tanpa memeriksanya di gerbang akan menciptakan lubang
*fail-open* — bukti bertanda "tidak cocok" membuka gerbang hanya karena ia ada. Rancangan `v4` karena
itu menuntut hasil **cocok** pada predikat gerbang pemberian (`VAL-BD-079`). Ini penurunan dari
`DEC-BD-042`, bukan aturan baru, tetapi pemilik proses belum menegaskannya — `OQ-BD-018`. Bila hasil
keputusan dikehendaki bersifat keterangan saja, pengetatan ini dicabut.

Dua pertanyaan terbuka, keduanya **tidak memblokir rancangan**: `OQ-BD-017` nama peran konkret pemegang
`BloodUnit : ResolveNotUsable` (menahan satu baris seeder), dan `OQ-BD-018` di atas.

Pemblokir yang tersisa tinggal **dua, keduanya tindakan manusia**: `G1` approval kontrak `v4`, dan
`G2b` keputusan aktivasi modul (`BD-DEP-016`). Approval manusia belum diklaim.

**Rekonsiliasi 3 September 2026.** Seluruh artefak Bank Darah disapu dan diselaraskan atas penutupan
`BD-DEP-008`: sembilan berkas yang masih menyatakan "prefix belum terdaftar" atau "BD-DEP-008 blocker
terbuka" sudah diperbarui, dan pemisahan `G2a` (tertutup) / `G2b` (terbuka) kini konsisten di manifest,
status modul, catatan prasyarat, arsitektur backend, kamus data, PRD, roadmap, dan register keputusan.
`BD-DEP-016` diterbitkan sebagai penerus pemblokir, dan Lifecycle registri **tidak** diubah.

⚠️ **Pemeriksaan status 3 September 2026 — bukti kemampuan menjadi `STALE`.** Backend bergerak ke
`4205d18` lewat merge `QuilvianIntegrationBackend`. Berbeda dengan seluruh pergerakan SHA sebelumnya
pada modul ini, merge ini membawa perubahan source aplikasi nyata: Laboratory (`LabOrder` beserta DTO,
service, enum, configuration), InPatient (tiga controller episode/discharge/bed), Billing, MasterData
(Bed, ClearanceItem), lima migration, dan snapshot model.

`02-existing-capability-map.md` terikat `9522caa` dan karena itu ditandai **`STALE`**. Dua kemampuan
menuntut tinjauan lebih dulu: `BD-CAP-014` — `LabOrderController` dipakai sebagai pola route, grup
Swagger, dan bentuk respons yang dicontoh seluruh `api-contract.md`; dan `BD-CAP-003` — pembaca status
kunjungan menempel pada bentuk episode dan kepulangan. Dampak pada area lain rendah, dan
**`MstServiceUnit` tidak tersentuh**, sehingga titipan kolom `BE-BD-002` tetap aman.

**`BD-DEP-008` ditutup 3 September 2026** lewat commit `ed7fba8`. Registry memuat baris
`HealthServices | BloodBankManagement / Blood Bank | BUSINESS DOMAIN / MODULE | Bbk | PLANNED`.
Prefix yang disahkan **persis `Bbk`**, sama dengan yang diajukan blueprint sejak `v1`, sehingga seluruh
nama `Bbk*` pada kontrak `v4` tetap berlaku dan tidak ada penggantian nama sebagai satu paket.

⚠️ **Lifecycle `PLANNED` bukan izin implementasi, dan itu menggantikan sebagian gerbang lama.** Registry
menyatakan sendiri bahwa persetujuannya "hanya memberi wewenang penamaan dan kepemilikan" dan **tidak**
memberi wewenang implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul
berstatus `PLANNED` — dengan `InsuranceManagement`/`Ins`/`PLANNED` sebagai contoh yang disebut langsung.

Gerbang `G2` pada roadmap revisi 2 karena itu pecah menjadi dua: **`G2a` penamaan — tertutup**, dan
**`G2b` aktivasi modul (`PLANNED` → `ACTIVE`) — masih terbuka**. Pemeriksaan
`tooling/qbe/Invoke-QbeConformanceCheck.ps1` menunjukkan checker membaca registry untuk kepemilikan
prefix tetapi **tidak** tampak menegakkan Lifecycle; artinya yang menahan adalah teks governance-nya,
bukan mesinnya. *Checker lolos* tidak sama dengan *diberi wewenang*.

**Impact scan terbatas sudah dijalankan pada 3 September 2026 dan penanda `STALE` dicabut.**
Rinciannya ada di `02-existing-capability-map.md` §Impact scan terbatas. Ringkasnya: dari 24 berkas
bukti yang dikutip peta, hanya **satu** yang tersentuh merge (`LabOrder.cs`), dan perubahannya aditif —
seluruh field yang dikutip `BD-CAP-007` dan `BD-CAP-010` utuh. `LabOrderController.cs`, `InpEpisode.cs`,
`EncounterStatus.cs`, dan `BillingSourceContract.cs` tidak berubah sama sekali.

**Nol baris kemampuan berpindah status, dan blueprint tidak perlu diubah.** Yang bergeser hanya basis
migration, kini `20260902042242_AddLabOrderDiscipline`.

Dua temuan justru **menguatkan** blueprint: enum `LabDiscipline` yang baru menyatakan dengan kata-kata
Laboratory sendiri bahwa Bank Darah berada di luar scope-nya — menguatkan `DEC-BD-015`, `DEC-BD-018`,
dan batas `BD-CTX-09`; dan tim InPatient memecah butir hak akses dengan alasan yang sama persis dengan
`DEC-BD-043`/`DEC-BD-044`, sehingga rancangan hak akses `v4` terbukti mengikuti konvensi rumah.

**Delivery roadmap revisi 2 — 3 September 2026.** `plan-module-delivery` menyusun ulang
`roadmap/00-delivery-plan.md` di atas set kontrak `v4`, register keputusan revisi 9, dan arsitektur
domain revisi 6. Statusnya `FORWARD-TEST / DRAFT`; seluruh task gated `G1` karena kontrak masih `draft`.

Tiga perubahan yang membuat revisi 1 diganti, bukan ditambal: Storage Location naik dari coverage gap
menjadi P0 dan **mendahului** alokasi; gerbang pemberian diperluas `DEC-BD-038`; dan gerbang `G3`
revisi 1 **dihapus** karena `DEF-BD-004` sudah tertutup penuh.

Satu temuan yang mengubah urutan kerja: `MstBloodStorageLocation`, `MstBloodComponent`, dan
`MstBloodBankReason` seluruhnya memakai prefix `Mst` yang sudah sah, sehingga **tidak terblokir
`BD-DEP-008`**. Gelombang `MVP-0` — seluruh master ditambah seeder hak akses — karena itu dapat
berjalan segera setelah approval, sementara registry masih diurus.
