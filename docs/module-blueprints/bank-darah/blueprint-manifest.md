# Blueprint Manifest — Bank Darah

```yaml
blueprint_id: BD-BP-001
module_name: Bank Darah
module_slug: bank-darah
module_prefix: BD
revision: 24
status: IN_PROGRESS
current_phase: BD-PH-007
created_at: 2026-09-02T00:40:53+07:00
updated_at: 2026-09-04T12:05:00+07:00
last_verified_at: 2026-09-04
last_readiness_result: NOT_READY
last_readiness_scope_note: >-
  Modul NOT_READY karena cakupan, bukan kerusakan. Gelombang MVP-0 sendiri
  READY_WITH_CONDITIONS dengan satu syarat tersisa: keempat migration dijalankan.
backend_source_sha: ba75a05
backend_branch: sukmagp
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
frontend_branch: sukmagpV2
skill_suite_version: 1.6.0
input_revision_hash: design-business-module-role-residue-2026-09-03
decision_revision: 11
capability_map_revision: 4
capability_map_status: CURRENT
capability_map_full_audit_sha: 9522caacf29371b1fddd1584e9a71ad94fe48d19
capability_map_impact_scan_sha: 5f7acaf
capability_map_scan_still_valid_at: ba75a05
capability_map_scan_validity_reason: >-
  Pergerakan 5f7acaf -> ba75a05 docs-only murni (terverifikasi git diff --name-only:
  nol berkas di luar docs/module-blueprints/bank-darah/), sehingga nol bukti kemampuan
  dapat bergeser dan impact scan 5f7acaf tetap berlaku.
capability_map_impact_scan_result: >-
  Dua baris berpindah status dan keduanya membaik: BD-CAP-005 Extend -> Ready to reuse,
  BD-CAP-018 Missing -> Ready to reuse. Nol baris memburuk. Dari 46 rujukan bukti, hanya
  MstServiceUnit.cs tersentuh dan perubahannya aditif murni; nol berkas bukti frontend berubah.
capability_map_frontend_impact_scan_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
capability_map_full_audit_recommended_before: MVP-2
capability_map_full_audit_reason: >-
  MstBloodStorageLocation dan MstBloodBankReason sudah berdiri tetapi belum punya baris
  BD-CAP-*, karena keduanya masuk scope setelah audit penuh ditulis. Menambah baris baru
  adalah pekerjaan audit penuh, bukan impact scan terbatas.
prerequisite_readiness_revision: 4
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
  - OQ-BD-017
  - OQ-BD-018
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
    status: approved
    approved_by: Sukmagp
    approved_at: 2026-09-03
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
approved_by: Sukmagp
approved_at: 2026-09-03
resolved_dependency_ids:
  - BD-DEP-004
  - BD-DEP-005
  - BD-DEP-008
  - BD-DEP-016
active_dependency_ids:
  - BD-DEP-001
  - BD-DEP-002
  - BD-DEP-003
  - BD-DEP-006
  - BD-DEP-007
  - BD-DEP-009
  - BD-DEP-010
  - BD-DEP-011
  - BD-DEP-012
  - BD-DEP-013
  - BD-DEP-014
  - BD-DEP-015
active_roadmap_revision: 2
roadmap_status: APPROVED
supersedes: null
```

## Penjelasan isi manifest

Manifest ini adalah kartu identitas modul Bank Darah. Ia menjawab satu pertanyaan: versi keputusan
mana yang sedang berlaku, dan atas dasar source code versi berapa keputusan itu dibuat.

| Field | Arti dalam bahasa sehari-hari |
| --- | --- |
| `blueprint_id` | Nomor identitas blueprint. Ditetapkan sekali dan tidak pernah diganti. |
| `module_prefix` | Awalan `BD` dipakai untuk penomoran keputusan, fase, dependency, dan task blueprint. **Bukan** awalan penamaan entity backend: awalan itu **`Bbk`**, terpisah, dan sejak 3 September 2026 **sudah terdaftar** di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dengan Lifecycle **`ACTIVE`** (commit `8075784`). |
| `revision` | Naik hanya bila arsitektur target, kontrak, dependency, atau keputusan yang sudah disetujui berubah secara berarti. Tidak naik hanya karena status berubah. |
| `status` | Naik `PARTIAL` → `READY` → **`IN_PROGRESS`** pada 3 September 2026. Nilai terakhir berarti ada pekerjaan aktif yang sudah diberi wewenang dan berbukti: empat task `MVP-0` sudah dijalankan dan meninggalkan laporan tracked. |
| `current_phase` | Berpindah ke `BD-PH-007` Implementasi Backend. `BD-PH-005` penyusunan blueprint target dan `BD-PH-006` perencanaan delivery keduanya `DONE` sejak approval `G1` turun. |
| `last_verified_at` | Terisi **`2026-09-04`**. Verifikasi kesiapan dijalankan dua kali pada hari itu: pass pertama di `f940ae3` memulangkan `NOT_READY` dengan dua blocker kritis, pass kedua di `5f7acaf` memulangkan `NOT_READY` **karena cakupan, bukan kerusakan** — kedua blocker kritis sudah tertutup. Rinciannya di catatan penutup. |
| `backend_source_sha` | Versi source backend yang menjadi dasar seluruh keputusan di blueprint ini. Naik `9522caa` → `9dc7637` → `db08c14` → `792acb9` → `ab39b63` → `a9bc9fd` → **`4205d18`**. Setelah `6488511` naik ke **`ec2bcac`** lewat tiga commit implementasi `MVP-0`. **Berbeda dengan seluruh pergerakan sebelumnya**, ketiganya membawa source aplikasi nyata — 28 berkas, seluruhnya milik Bank Darah sendiri. Impact scan terbatas dijalankan 3 September 2026: dari 24 berkas bukti peta kemampuan, hanya dua tersentuh dan keduanya **menguatkan** peta. Rinciannya di catatan penutup. Enam langkah pertama seluruhnya docs-only dan sudah diverifikasi `git diff --name-only`. **Langkah terakhir berbeda:** `4205d18` adalah merge `QuilvianIntegrationBackend` ke `sukmagp` yang membawa **perubahan source aplikasi nyata**. Bukti kemampuan sempat ditandai `STALE`, lalu **impact scan terbatas dijalankan pada 3 September 2026 dan penandanya dicabut** — nol baris berpindah status. Rinciannya di `02-existing-capability-map.md` §Impact scan terbatas. **Pada 4 September 2026 SHA bergerak dua kali lagi:** `ec2bcac` → `f940ae3` (merge yang membawa penataan ulang project test dan **merusak build**) → **`5f7acaf`** (perbaikan penataan test; build pulih). Peta kemampuan sempat ditandai `STALE`, lalu **impact scan terbatas dijalankan pada 4 September 2026 dan penandanya dicabut** — dua baris berpindah status dan **keduanya membaik**. Rinciannya di `02-existing-capability-map.md` §Impact scan terbatas — 4 September 2026. |
| `input_revision_hash` | Menunjuk asal keputusan: sesi wawancara Grill Me architecture gap final closure pass tanggal 2 September 2026, yang melanjutkan scope pass, closure pass, dan architecture gap closure pass di hari yang sama. |
| `closed_gap_ids` | Daftar gap arsitektur dan pertanyaan terbuka yang sudah ditutup keputusan pemilik. `ARCH-BD-GAP-01`..`09` ditutup `DEC-BD-025`..`034`; `ARCH-BD-GAP-10` ditutup `DEC-BD-037`; `OQ-BD-015` ditutup `DEC-BD-038`. **Tidak ada gap arsitektur yang masih terbuka.** |
| `roadmap_status` | Naik dari `FORWARD-TEST` ke **`APPROVED`** pada 3 September 2026. Roadmap **revisi 2** disusun sebagai forward-test di atas set kontrak `v4`, lalu ikut disetujui ketika `G1` turun. Revisi 1 ditandai `STALE` dan digantikan, bukan ditambal. |
| `decision_revision` | Naik ke `10` sampai OQ residue closure pass 3 September 2026. `DEF-BD-004` ditutup seluruhnya oleh `DEC-BD-039`..`DEC-BD-044`. **Tidak ada lagi keputusan bisnis yang memblokir**, dan sejak commit `8075784` **tidak ada lagi dependency teknis yang memblokir**. Penyelarasan pencatatan `G1` diselesaikan pada revisi 20. |
| `contract_versions` | Set kontrak desain **`v4`** berstatus **`approved`** sejak 3 September 2026 (`Sukmagp`), hasil design-business-module update pass 3 September 2026 yang menyerap role residue closure. `v1` sampai `v3` ditandai `superseded`. Set kontrak berlaku sebagai **satu himpunan**; seluruh berkas yang dicakup ikut naik ke `v4`, kecuali `contracts/integration-contract.md` yang `last_changed_in`-nya tetap `v2` karena isinya memang tidak bergerak. |
| `owners` | Pemilik per sumbu (product/domain, API, security, frontend). `approved_by`/`approved_at` kini terisi **`Sukmagp` / `2026-09-03`**: desain sudah `approved`. Nama itu berasal dari keterangan owner, bukan disimpulkan dari repository. |
| `supersedes` | Kosong karena blueprint ini tidak menggantikan blueprint lain. |

## Peringatan yang melekat

Audit kemampuan existing sudah dijalankan pada 2 September 2026 dan hasilnya ada di
`02-existing-capability-map.md`. Peringatan "scope dikunci tanpa audit" **sudah dicabut**.

Peta kemampuan itu terikat pada backend `9522caa` dan frontend `afbb8ab`, dengan impact scan terbatas
terakhir di `4205d18`.

Pada 4 September 2026 peta sempat ditandai `STALE` lagi: backend bergerak dua kali dalam sehari
(`ec2bcac` → `f940ae3` → **`5f7acaf`**) dan frontend sekali (`afbb8ab` → **`101ec5d3`**), dan
pergerakan backend membawa **source aplikasi nyata**. **Impact scan terbatas sudah dijalankan pada
hari yang sama dan penandanya dicabut.** Peta kini revisi **4** dan berstatus `CURRENT`.

Hasilnya: dari 46 rujukan bukti, hanya `MstServiceUnit.cs` yang tersentuh dan perubahannya **aditif
murni**; nol berkas bukti frontend berubah. Dua baris berpindah status dan **keduanya membaik** —
`BD-CAP-005` `Extend` → `Ready to reuse`, dan `BD-CAP-018` `Missing` → `Ready to reuse`. Nol baris
memburuk.

⚠️ **Satu catatan yang tetap berlaku.** `MstBloodStorageLocation` dan `MstBloodBankReason` sudah
berdiri tetapi **belum punya baris `BD-CAP-*`**, karena keduanya masuk scope setelah audit penuh
ditulis. Peta karena itu tidak lagi menggambarkan **seluruh** kemampuan Bank Darah yang sudah ada.
Menambah baris baru adalah pekerjaan audit penuh, bukan impact scan, dan sebaiknya dikerjakan sebelum
gelombang `MVP-2` disusun.

Blueprint tidak memberi wewenang implementasi. Menulis dokumen di sini tidak sama dengan izin
mengubah controller, service, entity, migration, database, atau melakukan deployment.

### Catatan pass bertanggal

> Blok di bawah adalah **rekaman historis per pass**, bukan keadaan sekarang. Setiap entri benar pada
> tanggalnya. Keadaan terkini ada pada field YAML di atas dan pada catatan rekonsiliasi paling bawah.
> Khususnya, tiga hal di blok historis **sudah tidak berlaku**: rujukan `BD-DEP-008` sebagai pemblokir,
> `Bbk` sebagai placeholder, dan setiap kalimat "Approval manusia belum diklaim" beserta penyebutan set
> kontrak `v4` sebagai `draft`. Ketiganya benar pada tanggalnya masing-masing, dan seluruhnya ditutup
> pada 3 September 2026.

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

**Satu pengetatan gerbang yang sempat menunggu penegasan, dan kini sudah ditegaskan.** `DEC-BD-042`
menuntut hasil keputusan bukti kecocokan tersimpan. Menyimpannya tanpa memeriksanya di gerbang akan
menciptakan lubang *fail-open* — bukti bertanda "tidak cocok" membuka gerbang hanya karena ia ada.
Rancangan `v4` karena itu menuntut hasil **cocok** pada predikat gerbang pemberian (`VAL-BD-079`).
Pemilik proses **menegaskannya pada 3 September 2026** lewat `DEC-BD-046`; lihat catatan penutup.

Dua pertanyaan terbuka yang sempat menyertai `v4` — `OQ-BD-017` dan `OQ-BD-018` — **keduanya sudah
ditutup** pada 3 September 2026 oleh `DEC-BD-045` dan `DEC-BD-046`.

**Aktivasi modul 3 September 2026 — `G2b` tertutup.** Commit `8075784` menaikkan Lifecycle registri
Bank Darah dari `PLANNED` ke **`ACTIVE`**. Changelog registry menyatakan aktivasi itu "membuka wewenang
implementasi entity operasional `Bbk*` sesuai `QBE-MOD-002`", dengan catatan bahwa eksekusi database di
luar dev pemilik dan deployment tetap wewenang terpisah.

Dengan itu **seluruh dependency teknis Bank Darah tertutup**: `BD-DEP-008` penamaan dan `BD-DEP-016`
aktivasi. Tidak ada lagi gerbang registry yang menahan.

**Pencatatan `G1` sempat bertentangan, dan pertentangan itu sudah selesai.** Changelog registry menyebut
aktivasi didasarkan pada "persetujuan owner Bank Darah dan **approval blueprint `BD-BP-001` contract
`v4`**", sementara blueprint sendiri masih menulis `approved_by: null` dan set kontrak `draft`. Skill
sengaja **tidak** mengisinya sendiri waktu itu: nama penyetuju dan tanggalnya hanya diketahui owner, dan
mengarangnya berarti memalsukan rekam keputusan manusia.

Owner menyampaikan keterangannya pada 3 September 2026, dan itulah yang dicatat pada revisi 20. Lihat
catatan penutup di bagian paling bawah dokumen ini.

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
**`G2b` aktivasi modul (`PLANNED` → `ACTIVE`) — waktu itu masih terbuka; ditutup commit `8075784` di
hari yang sama**. Pemeriksaan
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

**Sinkronisasi approval `G1` — 3 September 2026, revisi 20.** Owner menyatakan approval turun atas nama
**`Sukmagp`** bertanggal **`2026-09-03`**. Keterangan itu menutup pertentangan pencatatan yang dicatat
sebelumnya: changelog registry ternyata benar, dan yang tertinggal memang pencatatan di sisi blueprint.

Yang berubah pada pass ini seluruhnya berupa **penyelarasan pencatatan**, bukan perubahan rancangan:

| Perubahan | Dari | Menjadi |
| --- | --- | --- |
| `approved_by` / `approved_at` | `null` / `null` | `Sukmagp` / `2026-09-03` |
| Set kontrak `v4` | `draft` | **`approved`** |
| `status` modul | `PARTIAL` | **`READY`** |
| `current_phase` | `BD-PH-005` | **`BD-PH-007`** |
| `roadmap_status` | `FORWARD-TEST` | **`APPROVED`** |
| `BD-PH-005`, `BD-PH-006` | `IN_PROGRESS` | **`DONE`** |
| `BD-PH-007` | `BLOCKED` | **`READY`** |

**Nol perubahan arsitektur, kontrak, dependency, atau keputusan.** Revisi naik ke 20 karena status
approval set kontrak adalah bagian dari keputusan yang disetujui, bukan sekadar penanda status.

⚠️ **Satu batas approval yang perlu dibaca apa adanya.** `G1` sebagaimana didefinisikan roadmap §B
berbunyi "Owner menyetujui blueprint & set kontrak `v4`". Approval ini karena itu menutup **blueprint dan
set kontrak**, dan **tidak** otomatis mengubah status register keputusan: `DEC-BD-001` sampai `DEC-BD-044`
pada `00-interview-decisions.md` tetap berstatus `draft` dengan `approved_by` kosong, persis seperti
sebelumnya. Menaikkannya menuntut pernyataan owner tersendiri. Itu **tidak** menahan task mana pun —
builder membaca kontrak, bukan register keputusan — tetapi dicatat supaya tidak dikira sudah ikut naik.

**Approval tidak memberi wewenang eksekusi.** Ia membuka penjadwalan task lewat `build-module-backend`.
Migration, eksekusi database di luar dev pemilik, deployment, dan publikasi Git tetap wewenang terpisah
yang harus diminta per tindakan.

**OQ residue closure pass — 3 September 2026, revisi 21.** `grill-me` menutup dua pertanyaan terbuka
terakhir yang menempel pada set kontrak `v4`, dan register keputusan naik ke revisi 10.

| Decision | Menutup | Isi |
| --- | --- | --- |
| `DEC-BD-045` | `OQ-BD-017` | `BloodUnit : ResolveNotUsable` dipegang **kewenangan operasional BDRS** — peran yang sama dengan `ResolveReturn`. Ketiga butir hak akses **tetap terpisah**; yang sama perannya, bukan butirnya |
| `DEC-BD-046` | `OQ-BD-018` | Bukti kecocokan bertanda `Incompatible` **menahan** pemberian jalur normal. Gerbang memeriksa **isi** bukti, bukan keberadaannya. `VAL-BD-079` ditegaskan berlaku |

**Set kontrak tetap `v4` dan tetap `approved`.** Kedua keputusan menegaskan rancangan yang sudah ada,
bukan mengubahnya: nol entity, nol kolom, nol enum, nol endpoint, nol butir hak akses, nol kode
validasi, dan nol acceptance criteria baru. Keduanya bahkan sudah punya skenario ujinya di
`testing/acceptance-test-matrix.md` sejak `v4`. Pemeriksaan berkas per berkas ada di
`00-interview-decisions.md` §8.24.

Revisi blueprint naik ke 21 karena baseline keputusan bergerak — dua keputusan baru menutup dua
pertanyaan yang manifest ini sendiri lacak — bukan karena arsitektur atau kontrak berubah.

Enam berkas set kontrak sempat memuat kalimat penunjuk yang menyebut kedua pertanyaan sebagai terbuka.
Keenamnya **sudah diserap** pada hari yang sama; lihat catatan penutup di bawah.

**Contract reconciliation pass — 3 September 2026, revisi tetap 21.** `design-business-module`
menyerap `DEC-BD-045` dan `DEC-BD-046` ke dalam enam berkas yang masih memuat kalimat penunjuk usang:
`contracts/permission-audit-matrix.md`, `contracts/state-transition-matrix.md`,
`contracts/validation-matrix.md`, `data/data-dictionary.md`, `02-backend-architecture.md`, dan
`04-prd-to-mvp.md`.

**Revisi blueprint sengaja tidak naik.** Pass ini tidak mengubah satu pun aturan, arsitektur, kontrak,
dependency, atau keputusan — ia hanya mengganti kalimat yang menyebut dua pertanyaan sebagai terbuka
menjadi kalimat yang menyebut jawabannya. Angka yang diperiksa sebelum dan sesudah pass **identik**:

| Yang diperiksa | Sebelum | Sesudah |
| --- | --- | --- |
| Contract version keenam berkas | `v4` `approved` | `v4` `approved` |
| `last_changed_in` | `v4` (`integration-contract.md` tetap `v2`) | tidak bergerak |
| Entity `Bbk*` pada kamus data | 27 | 27 |
| Endpoint pada `api-contract.md` | 58 | 58 |
| Kode `VAL-BD-*` | 53 | 53 |
| `AC-BD-*` pada matriks acceptance | 92 | 92 |

Satu tambahan yang perlu dibaca pelaksana `BE-BD-016`: `permission-audit-matrix.md` kini memuat
peringatan tegas bahwa **peran yang sama tidak berarti butir hak akses yang sama**. Peran operasional
BDRS menerima `BloodUnit : ResolveReturn` dan `BloodUnit : ResolveNotUsable` sebagai **dua baris
seeder**, bukan satu butir gabungan — karena jaminan `AC-BD-093` hanya bekerja bila butirnya terpisah.

**Penyegaran `backend_source_sha` — `c12cc57` → `6488511`.** Dua commit dokumen menyusul
(`7f2ce82` sinkronisasi OQ residue, `6488511` rekonsiliasi kontrak). Impact scan dijalankan:
`git diff --name-only c12cc57 HEAD` di luar `docs/` mengembalikan **nol berkas**, seluruhnya 13 dokumen
blueprint Bank Darah. Peta kemampuan tetap `CURRENT`; tidak ada impact scan penuh yang dibutuhkan.

**Keadaan sesudah pass ini.** Tidak ada satu pun pertanyaan terbuka yang menempel pada set kontrak
`v4`, tidak ada gerbang yang menahan, dan tidak ada fase yang `BLOCKED`. Yang tersisa seluruhnya
pekerjaan implementasi, dimulai dari gelombang `MVP-0`.

**Blueprint consistency pass — 3 September 2026, revisi 22.** Pelaksanaan `BE-BD-014` menemukan
pertentangan antara kamus data dan kontrak engineering canonical, dan pass ini menyelesaikannya di
sisi blueprint.

**Yang dipertentangkan.** `data/data-dictionary.md` menetapkan kolom `SortOrder` (`int`, wajib,
bawaan `0`, "urutan tampil pilihan") pada `MstBloodStorageLocation`, sementara
`BACKEND_ENGINEERING_CONTRACT.md` menyatakan `SortOrder` presentasi yang dipersistensi secara
generik **dilarang untuk kode baru**. `QBE_EXCEPTIONS.json` tidak memuat satu pun pengecualian.

**Yang diputuskan.** Requirement `SortOrder` **dicabut** dari blueprint. Urutan pilihan diturunkan
dari field semantik yang sudah ada — `StorageLocationCode` lalu `StorageLocationName`. Untuk kulkas
darah, urutan menurut kode justru lebih wajar dibaca petugas daripada angka yang diketik admin.

| Artefak | Yang berubah |
| --- | --- |
| `data/data-dictionary.md` | Baris kolom `SortOrder` dan barisnya pada DDL dihapus; catatan pencabutan bertanggal ditambahkan |
| `contracts/api-contract.md` | Keterangan `PUT /{id}` menjadi "Ubah kode, nama, keterangan" |
| `02-backend-architecture.md` | Class diagram, baris dokumentasi entity §F, dan tabel status model §H diselaraskan; alasan pencabutan dicatat pada §L "Yang sengaja tidak dibuat" |
| `03-frontend-architecture.md` | Skema layar `FE-BD-10` tidak lagi menuntut kolom dan isian "urutan" |

**Set kontrak tetap `v4` dan tetap `approved`.** Pencabutan ini menghapus satu kolom presentasi yang
belum pernah diimplementasikan dan belum punya satu pun konsumen: `BE-BD-014` memang tidak
membuatnya, dan `FE-BD-011` belum dibangun. Nol perubahan aturan bisnis, nol perubahan keputusan,
nol perubahan invariant, nol migration.

Revisi blueprint naik ke 22 karena kamus data adalah bagian kontrak yang disetujui, dan mencabut
sebuah kolom darinya adalah perubahan material — bukan sekadar penyelarasan kalimat.

⚠️ **Bila kelak urutan manual benar-benar dibutuhkan, jalurnya bukan menambah kolom diam-diam.**
Urutannya mengikat: daftarkan pengecualian pada `QBE_EXCEPTIONS.json` beserta QBE ID, alasan, dan
cakupannya; baru setelah itu satu migration aditif menambahkan kolomnya.

**Satu catatan preseden yang belum terselesaikan, dan sengaja tidak diselesaikan di sini.**
`MstMedicalRecordAccessPurpose` — kode baru yang belum lama masuk — **memiliki** `SortOrder` yang
dipersistensi. Preseden rumah karena itu tidak seragam. Menyelaraskannya berada di luar scope modul
Bank Darah dan merupakan keputusan pemilik kontrak engineering, bukan keputusan blueprint ini.

**Sinkronisasi implementasi `MVP-0` — 3 September 2026, revisi 23.** Empat task backend dijalankan
berturut-turut, masing-masing meninggalkan laporan tracked di `task/report/backend/`.

| Task | Status | Bukti |
| --- | --- | --- |
| `BE-BD-001` | `SELESAI SEBAGIAN` | `MstBloodComponent` selesai — 9 endpoint, migration, seeder, 26 test. `MstBloodBankReason` **belum dikerjakan** |
| `BE-BD-002` | **`SELESAI`** | Kolom `IsAvailableForBloodOrder` pada `MstServiceUnit`, bawaan menolak di tiga lapisan, 8 test |
| `BE-BD-014` | **`SELESAI`** | `MstBloodStorageLocation` — 9 endpoint, migration, seeder dua lokasi aktif, 25 test |
| `BE-BD-016` | `SELESAI SEBAGIAN` | 8 dari 39 butir hak akses terdaftar, 12 test kontrak. Sisanya terikat task pembuat controller |

**Kemajuan delivery: 2 selesai penuh, 2 selesai sebagian, dari 27 task.** Angka itu dihitung dari
keberadaan laporan `task/report/**`, bukan diperkirakan.

**Impact scan terbatas atas pergerakan source.** `6488511` → `ec2bcac` membawa 28 berkas source
aplikasi — seluruhnya milik Bank Darah sendiri, hasil ketiga task di atas. Dari 24 berkas bukti yang
dikutip `02-existing-capability-map.md`, hanya **dua** tersentuh:

| Berkas | Kemampuan | Putusan |
| --- | --- | --- |
| `MstServiceUnit.cs` | `BD-CAP-005` berstatus `Extend` | **Menguatkan peta, bukan membatalkannya.** Peta menulis "sudah memakai pola tanda kemampuan per unit: `IsAvailableForRegistration`, `IsAvailableForKiosk`". `BE-BD-002` menambahkan `IsAvailableForBloodOrder` mengikuti pola itu persis — yaitu `Extend` yang diramalkan peta |
| `ApplicationDbContextModelSnapshot.cs` | — | Berkas hasil bangkitan `dotnet ef`, bukan bukti kemampuan |

**Nol baris kemampuan berpindah status.** Peta tetap `CURRENT` dan tidak ditandai `STALE`.

Satu catatan untuk `trace-existing-capabilities` bila kelak dijalankan: `BD-CAP-005` sekarang
**sudah terpenuhi**, bukan lagi rencana. Memindahkan statusnya adalah wewenang skill itu, bukan skill
ini, dan tidak mendesak karena kesimpulan blueprint tidak berubah karenanya.

**Register keputusan naik ke revisi 11.** `DEC-BD-047` menutup `CONF-BD-006` — butir
`BloodUnit : Compatibility` dicabut dari baris peran Petugas BDRS umum dan hanya dipegang petugas
berwenang validasi. Konflik itu ditemukan pelaksanaan `BE-BD-016`, dan penyerapannya ke
`contracts/permission-audit-matrix.md` sudah dikerjakan `design-business-module` pada hari yang sama.
Set kontrak tetap `v4` `approved`.

Revisi blueprint naik ke 23 karena dua hal material sekaligus: baseline keputusan bergerak, dan status
modul berpindah dari rencana menjadi pekerjaan berjalan yang berbukti.

---

**Verifikasi kesiapan dan pemulihan build — 4 September 2026, revisi 24.** Dua hal terjadi berurutan
pada hari yang sama, dan keduanya material.

**Pertama, build sempat rusak dan sudah pulih.** Penataan ulang project test
(`4339e91` + merge `f103fff`) memindahkan seluruh project test ke folder `Tests/`, lalu
`QuilvianSystemBackend.csproj` mengecualikannya lewat satu baris `DefaultItemExcludes` berisi
`Tests\**`. Kelima berkas test Bank Darah **tertinggal** di folder lama `QuilvianSystemBackend.Tests\`
— nama yang **tidak cocok** dengan pola itu, karena pola MSBuild berjangkar di awal jalur. Akibatnya
SDK Web menyapu kelima berkas itu ke dalam project aplikasi, yang tidak punya paket xUnit.

| Keadaan | Hasil `dotnet build` |
| --- | --- |
| `f940ae3` | **`Build FAILED` — 217 Error(s)**, seluruhnya `CS0246` Xunit pada kelima berkas |
| `5f7acaf` | **`Build succeeded` — 0 Error(s)**, 210 Warning(s) |

Commit `5f7acaf` — *"fix(test): organize bank darah tests by module"* — memindahkan kelimanya ke
`Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/` dengan namespace disesuaikan.
Git mencatatnya sebagai rename **`R099`**: isinya 99% utuh, **bukan** dihapus dan ditulis ulang.

**Kedua, bukti pengujian akhirnya terbukti, bukan sekadar diklaim.** Sebelum ini keempat laporan task
menyandarkan penerimaannya pada 101 pengujian yang **tidak pernah** dijalankan lewat project resmi —
laporan-laporan itu sendiri mencatatnya jujur sebagai dijalankan lewat project verifikasi sementara,
karena `PatientEncounterTestWorld.cs` lebih dulu merusak project test. Berkas itu kini sudah pindah
dengan selamat ke `Tests/QuilvianSystemBackend.UnitTests.InMemory/`.

| Yang dijalankan di `5f7acaf` | Hasil |
| --- | --- |
| `dotnet test --filter FullyQualifiedName~BankDarah` | **`Failed: 0, Passed: 101`** |
| `dotnet test QuilvianSystemBackend.Tests.csproj` | **`Failed: 0, Passed: 212`** |

Angka **101 persis sama** dengan jumlah yang diklaim keempat laporan (26 + 8 + 25 + 30 + 12). Klaim
yang sebelumnya tidak dapat diperiksa kini terverifikasi.

**Dua dependency berpindah status — inilah yang menaikkan revisi.**

| Dependency | Semula | Kini | Ditutup oleh |
| --- | --- | --- | --- |
| `BD-DEP-005` katalog komponen darah | `MISSING` | **terpenuhi** | `BE-BD-001` — `MstBloodComponent` beserta `MstBloodBankReason` |
| `BD-DEP-004` kewenangan unit memesan darah | `EXTEND` | **terpenuhi** | `BE-BD-002` — kolom `IsAvailableForBloodOrder` |

**Koreksi status task.** `BE-BD-001` naik dari `SELESAI SEBAGIAN` menjadi **`SELESAI`**: sisa
`MstBloodBankReason` beserta sepuluh kategori alasannya sudah dikerjakan pada commit `7d00647`.
Kemajuan delivery karena itu menjadi **3 selesai penuh dan 1 selesai sebagian, dari 27 task** —
sebelumnya tercatat 2 dan 2.

**Putusan kesiapan dibedakan menurut cakupan**, karena menyamakan keduanya menyesatkan ke dua arah:

| Cakupan | Putusan | Syarat tersisa |
| --- | --- | --- |
| Modul Bank Darah | **`NOT_READY`** | Gelombang `MVP-1`..`MVP-4` beserta 12 task frontend. Nol dari 15 entity `Bbk*` operasional ada |
| Gelombang `MVP-0` | **`READY_WITH_CONDITIONS`** | **Satu syarat:** keempat migration dijalankan. Pemilik: pemilik database. Selama belum, ketiga master tidak dapat dipakai di lingkungan mana pun — risikonya terkurung, bukan menyebar |

`BD-PH-009` Verifikasi kesiapan karena itu berpindah dari `NOT_STARTED` ke **`IN_PROGRESS`**: fase itu
sudah dijalankan dan memulangkan putusan, tetapi belum dapat ditutup `DONE`.

**Satu risiko yang dicatat supaya tidak terulang.** Kerusakan di atas tidak berasal dari Bank Darah,
tetapi Bank Darah yang menjadi korbannya, dan gejalanya menyesatkan — error muncul di project
**aplikasi**, bukan di project test. Modul lain yang menaruh test di luar `Tests/` akan tersapu dengan
cara yang sama. Menindaklanjutinya adalah wewenang pemilik kontrak engineering backend, bukan
keputusan blueprint ini.

Revisi naik ke 24 karena dua dependency berpindah status secara material. Set kontrak **tetap `v4`
`approved`** dan tidak tersentuh: tidak ada arsitektur target, kontrak, maupun keputusan yang berubah.
