# Roadmap Backend — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `2` |
| `roadmap_revision` | `2` — 8 Oktober 2026. Revision `1` dibuat pada hari yang sama dengan butir `PAT-OQ-001`–`004` berstatus `PROPOSED`; revision `2` mencatat keputusan final review dan `PAT-OQ-005`–`007` |
| Status roadmap | **`approved`** — task, acceptance criteria, dan seluruh keputusan review disetujui pemilik 8 Oktober 2026. **Tidak ada butir terbuka** |
| Area / Module / Prefix | `HealthServices` / `PatientManagement` / `Pat` — registry `ACTIVE` |
| Snapshot source backend | Branch `QuilvianStaDeploy`, commit `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` |
| Snapshot source frontend | Tidak diperiksa — tidak ada task frontend (`PAT-DEC-015`) |
| Masukan | `00-interview-decisions.md` revision `2`, `sha256:fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885` |
| Kontrak | `contracts/api-contract.md` versi `1.1.0`, `sha256:cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e` |
| Traceability | [requirement-traceability.md](requirement-traceability.md) |

Hash dihitung dengan `sha256sum` terhadap berkas apa adanya di disk, cara yang sama dengan
manifest `rekam-medis`.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-PAT-MIG-001 ✅
```

Roadmap ini berisi satu task, dan task itu tidak menunggu task lain. Prasyarat datanya — tabel
`migration.rsmmc_*` dan tabel cadangan yang sudah ada di staging — bukan task, sehingga tidak
digambar sebagai node. Prasyarat itu dicatat pada kartu task.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-PAT-MIG-001` |

---

## Slice

| Slice | Hasil yang dapat diperiksa | Task | Status |
| --- | --- | --- | --- |
| **S1 — Mekanisme rekonsiliasi MRN Pilot** | Endpoint admin sementara yang dapat mensimulasikan dan menjalankan pemindahan MRN 715 pasien Pilot, lengkap dengan test terfokus | `BE-PAT-MIG-001` | ✅ selesai 9 Oktober 2026 untuk implementasi source — **SOURCE IMPLEMENTATION: APPROVED** (review source leader `APPROVED` 9 Oktober 2026); **RUNTIME: PENDING** — rekonsiliasi belum dijalankan, `PAT-GATE-001` tetap terbuka. Riwayat: 🟡 sebagian 9 Oktober 2026 — source diport ke baseline `QuilvianIntegrationBackend` @ `179aea3f` (branch `BE-PAT-MIG-001`); test `73/73`, build `0` error, QBE `Strict` `PASS`; menunggu review source oleh pengguna/leader. Rekonsiliasi runtime belum dijalankan (`PAT-GATE-001`) |

Menjalankan rekonsiliasi pada data staging **bukan** bagian slice ini. Itu gerbang terpisah
`PAT-GATE-001`.

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-PAT-MIG-001` | 715 pasien Pilot V1 dapat dipindahkan dengan aman ke MRN kanonik RSMMC, termasuk QR baru, tanpa mengubah identitas maupun relasi | `PAT-DEC-001`–`PAT-DEC-016`; `PAT-OQ-001`–`PAT-OQ-007`; `PAT-MIG-AC-01`–`PAT-MIG-AC-28` | `api-contract.md` `1.1.0` | `PatientController` (route, tag, izin `Patient : Update`, logika QR dengan pemeriksaan sebelum helper); `IdentityModel`; pola project test di `Tests/` | Endpoint, service, DTO, dan test terfokus di bawah PatientManagement | — | 28 butir, lihat kartu | Lihat kartu | `PAT-RSK-001`–`PAT-RSK-008`; pemilik: pemilik/leader PatientManagement | Lihat kartu |

---

## Kartu task

### ✅ `BE-PAT-MIG-001` — RSMMC Pilot MRN Reconciliation

| Field | Isi |
| --- | --- |
| **Task ID** | `BE-PAT-MIG-001` |
| **Judul** | RSMMC Pilot MRN Reconciliation |
| **Status** | ✅ **SELESAI 9 Oktober 2026 untuk cakupan task ini — implementasi source.** **SOURCE IMPLEMENTATION: APPROVED** — review source leader `APPROVED` 9 Oktober 2026 atas port di branch `BE-PAT-MIG-001` (`QuilvianIntegrationBackend` @ `179aea3f`); 28 dari 28 acceptance criteria terpetakan ke source dan terverifikasi (test `73/73`, build `0` error, QBE `Strict` *working tree* `PASS`); seluruh butir DoD fase implementasi terpenuhi. **RUNTIME: PENDING** — rekonsiliasi **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION**; `PAT-GATE-001` tetap **terbuka**; tanda ini **bukan** berarti 715 pasien sudah direkonsiliasi, dan migrasi RSMMC secara keseluruhan belum selesai. Bukti: [laporan bagian 10](../task/report/backend/BE-PAT-MIG-001.md). **Riwayat 🟡 SEBAGIAN 9 Oktober 2026, sesudah port semantik ke Integration.** Implementasi yang sudah direview dipindahkan maknanya ke branch `BE-PAT-MIG-001` di atas `QuilvianIntegrationBackend` @ `179aea3f`: enam berkas source baru identik dengan rujukan, kontrak SQL `IDENTICAL`, perubahan `PatientController`/`Program.cs`/`.gitignore` sama maknanya tanpa menimpa perubahan Integration. Validasi ulang di branch itu: `dotnet test` project `QuilvianSystemBackend.PatientManagementTests` `73` lulus, `0` gagal, `0` dilewati (72 test rujukan + 1 test delta `PatientQrPayloadBuilder`); `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)` — `256 Warning(s)`, nol dari berkas baru; QBE `Strict` *working tree* 16 berkas `VIOLATION: 0`, `PASS`. Butir DoD "source sudah direview pengguna/leader" **belum terpenuhi**. Rekonsiliasi runtime **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION** (`PAT-GATE-001`). Bukti: [laporan bagian 9](../task/report/backend/BE-PAT-MIG-001.md). **Riwayat 8 Oktober 2026, sesudah koreksi owner review:** ke-28 acceptance criteria terpetakan ke source dan test. Dua koreksi pemilik selesai: bukti kepemilikan QR memakai penanda + panjang + SHA-256 dan *commit* tidak pasti tidak pernah menghapus QR; positif palsu `QBE-CODE-002` ditutup dengan rename `GenerateQrCodePngBytes` → `RenderQrCodePngBytes`. `dotnet test` project `QuilvianSystemBackend.PatientManagementTests` `72` lulus, `0` gagal, `0` dilewati; `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)` — `242 Warning(s)`, nol dari berkas baru; QBE `Strict` *working tree* `VIOLATION: 0`, `PASS`. Butir DoD "source sudah direview pengguna/leader" **belum terpenuhi**. Rekonsiliasi runtime **NOT EXECUTED** — menunggu `PAT-GATE-001`. Riwayat: disetujui pemilik 8 Oktober 2026 sebagai `SIAP DIKERJAKAN`; pengerjaan pertama 66/66 test dengan satu positif palsu QBE `ReportOnly`. Bukti: [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| **Klasifikasi** | `LEGACY MIGRATION` — migrasi data pasien lama. Lihat `00-interview-decisions.md` bagian 5 tentang perbedaannya dengan arti `LEGACY MIGRATION` pada kontrak QBE |
| **Area / Module / Prefix** | `HealthServices` / `PatientManagement` / `Pat` |
| **Outcome** | 715 pasien Pilot V1 yang sudah ada dapat dipindahkan dengan aman dari MRN sementara buatan Quilvian ke rencana MRN RSMMC berstatus `FINALIZED`, termasuk pembuatan QR baru, sambil mempertahankan identitas pasien dan seluruh relasi yang ada |
| **Asal** | Pilot V1 memakai API pembuatan pasien biasa, sehingga MRN 715 pasien dibuat Quilvian dan tidak cocok dengan rencana kanonik. MRN itu dibutuhkan pasien RSMMC lain, sehingga rekonsiliasi wajib mendahului migrasi sisa pasien (`PAT-DEC-010`) |
| **Requirement/decision** | `PAT-DEC-001` sampai `PAT-DEC-016`; keputusan review `PAT-OQ-001` sampai `PAT-OQ-007`; requirement `PAT-MIG-AC-01` sampai `PAT-MIG-AC-28` |
| **Kontrak** | [`contracts/api-contract.md`](../contracts/api-contract.md) versi `1.1.0` — `approved` final; endpoint berstatus **Rencana (belum tersedia)** |
| **Reuse** | `REUSE`: base route dan `[Tags]` `PatientController.cs` baris 42 dan 52; konvensi izin `Patient : Update` baris 771–778 dan 912–919; `IdentityModel` untuk field audit. `REUSE WITH ADAPTER`: `SavePatientQrCodeFile` baris 1046–1172, karena baris 1144 menimpa file diam-diam dan bertentangan dengan `AC-13`. Path QR tujuan wajib diperiksa **sebelum** helper ini dipanggil; bila artefaknya sudah ada, helper tidak dipanggil (`PAT-OQ-005`). `REUSE`: `BuildPatientQrPayload` baris 1386–1401 untuk isi QR. Pola project test: `Tests/QuilvianSystemBackend.PharmacyTests/` (`PAT-OQ-001`) |
| **Larangan pakai ulang** | `GenerateMedicalRecordNumberAsync` baris 1403 (`AC-16`); `GeneratePatientCodeAsync` baris 2468 (`AC-17`); pola `!environment.IsProduction()` pada `Services/Security/AccessPermissionService.cs` baris 38 sebagai satu-satunya pemeriksaan, karena gagal terbuka dan melanggar `AC-05` (`PAT-OQ-006`); `IHostEnvironment.IsStaging()` saja, karena tidak membedakan huruf besar-kecil sedangkan `PAT-OQ-002` menuntut nama persis `Staging` |
| **Cakupan** | Satu endpoint `POST /admin/migration/rsmmc-pilot-mrn/reconcile` di bawah base route Patient; service rekonsiliasi beserta DTO request/response di dalam `Areas/HealthServices/PatientManagement/`; pemeriksaan path QR tujuan sebelum helper QR dipanggil; gerbang environment berbasis daftar izin positif berisi `Staging` saja; test terfokus sesuai `AC-27` di project xUnit khusus `Tests/QuilvianSystemBackend.PatientManagementTests/`, tanpa entri `.sln` (`PAT-OQ-001`). Penempatan persis berkas source mengikuti pola terdekat dan diputuskan saat build |
| **Governance guard test** | Sebelum membuat project test, agent build memeriksa ulang `rules/backend/TEST_POLICY.md` dan aturan `build-module-backend`. Penilaian perencanaan: **diizinkan** oleh `TEST_POLICY` bagian 3 (`00-interview-decisions.md` bagian 4.1). Bila pemeriksaan saat build menyatakan sebaliknya: **jangan di-*bypass***, hentikan bagian pembuatan test, laporkan aturan persis yang memblokir beserta alternatif canonical, lalu minta keputusan. Dilarang menaruh test ini di `PharmacyTests`, `NutritionTests`, atau `OperatingRoomTests`. Bagian source endpoint tidak ikut berhenti |
| **Di luar cakupan** | Membuat atau menerapkan EF migration; perintah migration database; SQL langsung yang mengubah data; memanggil endpoint; menjalankan rekonsiliasi 715 pasien; deployment; eksekusi produksi; 704.508 pasien yang belum dibuat (`AC-23`); perubahan pada tabel `migration.rsmmc_*`; menghapus QR lama; frontend; `appsettings*`; `Program.cs` kecuali registrasi dependency yang terbukti dibutuhkan dan dicatat |
| **Dependency** | Tidak ada task prasyarat. **Prasyarat data, bukan task, dilaporkan pengguna dan tidak diverifikasi agent:** tabel `migration.rsmmc_patient_mrn_plan`, `migration.rsmmc_patient_crosswalk`, dan `migration.rsmmc_patient_pilot_reconcile_backup` sudah ada di staging dengan isi sesuai `PAT-FACT-001`–`PAT-FACT-026` |
| **Batas wewenang database/migration** | Task ini **hanya** memberi wewenang implementasi source (`PAT-DEC-012`). Tidak ada pembuatan EF migration, penerapan EF migration, eksekusi database, SQL langsung, pemanggilan endpoint, eksekusi rekonsiliasi, deployment, atau eksekusi produksi. Tabel `migration.rsmmc_*` belum dipetakan di source, sehingga pembacaannya wajib tidak menimbulkan kebutuhan EF migration dan tidak mengubah skema (`PAT-OQ-007`). Mekanisme bacanya dipilih saat build dengan nilai masukan berparameter. Eksekusi runtime menunggu `PAT-GATE-001` |
| **Keputusan yang melekat** | MRN tujuan hanya dari perencana (`PAT-DEC-005`). `Id`, `PatientCode`, dan relasi tetap (`PAT-DEC-007`). QR lama tidak dihapus (`PAT-DEC-008`). Satu pasien satu transaksi; berhenti pada kegagalan tak terduga pertama (`PAT-DEC-011`). Konflik MRN tujuan dan konflik artefak QR tujuan diperlakukan sebagai kegagalan tak terduga yang menghentikan proses, karena audit mencatat nol konflik (`api-contract.md` bagian 4.2); konflik artefak QR dilaporkan berstatus `CONFLICT` (`PAT-OQ-005`). Hanya environment bernama persis `Staging` yang diizinkan, untuk kedua mode (`PAT-OQ-002`, `PAT-OQ-006`). `dryRun` yang tidak dikirim berarti `true` (`PAT-OQ-003`). `limit` hanya menghitung pasien layak; `ALREADY_RECONCILED` dan `QR_INCONSISTENT` dilaporkan untuk seluruh set Pilot, tidak dihitung dalam `limit`, dan `QR_INCONSISTENT` tidak diperbaiki (`PAT-OQ-004`) |
| **QBE** | QBE preflight dan kesesuaian engineering **diselesaikan saat build**, dari `AGENTS.md` backend dan `docs/engineering/`. Perkiraan awal: berkas baru berstatus `NEW CODE`, dan `PatientController` menjadi `TOUCHED LEGACY` bila ikut diubah. `QBE-NAM-003`, `QBE-DB-001`, dan `QBE-DB-002` mengatur *rename* dan kemungkinan besar tidak berlaku, karena task ini tidak me-*rename* apa pun. Ini dipastikan pada preflight build |
| **Laporan** | [`../task/report/backend/BE-PAT-MIG-001.md`](../task/report/backend/BE-PAT-MIG-001.md) — ditulis `build-module-backend` 8 Oktober 2026; diperbarui 9 Oktober 2026 dengan bukti port ke Integration (bagian 9) |

#### Acceptance criteria

1. **`AC-01`** — Rekonsiliasi hanya memilih baris yang memenuhi seluruh syarat berikut:
   `batch_id` perencana sama dengan permintaan; `plan_status` perencana = `FINALIZED`;
   `legacy_pid` crosswalk sama dengan `legacy_pid` perencana; `quilvian_patient_id` crosswalk
   terisi; `MstPatient.Id` sama dengan `quilvian_patient_id`; dan `MstPatient.MedicalRecordNumber`
   sekarang berbeda dengan `planner.final_medical_record_number`.
2. **`AC-02`** — MRN tujuan kanonik hanya diambil dari
   `rsmmc_patient_mrn_plan.final_medical_record_number`. `crosswalk.normalized_mrn` tidak boleh
   dipakai sebagai MRN tujuan.
3. **`AC-03`** — `dryRun=true` tidak melakukan satu pun perubahan database permanen dan tidak
   menulis satu pun file QR.
4. **`AC-04`** — `dryRun` mengembalikan baris yang deterministik, diurutkan menurut `legacy_pid`,
   dan minimal memuat: `legacyPid`, `patientId`, `PatientCode`, `FullName`, MRN sekarang, MRN
   akhir, path QR sekarang, path QR rencana, dan status.
5. **`AC-05`** — `dryRun=false` hanya diizinkan di environment yang terbukti bukan produksi.
   Eksekusi di produksi selalu ditolak. Environment yang tidak dikenal wajib ditolak (*fail
   closed*).
6. **`AC-06`** — Endpoint wajib login, dan memakai konvensi otorisasi Patient Update yang sudah
   ada atau izin lain yang sudah ada dan setara atau lebih ketat.
7. **`AC-07`** — Untuk setiap pasien yang direkonsiliasi, yang dipertahankan: `MstPatient.Id`,
   `PatientCode`, seluruh data demografi, seluruh relasi/FK yang ada, `CreateDateTime`, dan
   `CreateBy`.
8. **`AC-08`** — Hanya field rekonsiliasi berikut yang boleh berubah: `MedicalRecordNumber`,
   `QrCodePath`, `UpdateDateTime`, dan `UpdateBy`.
9. **`AC-09`** — QR baru wajib dibuat dari MRN akhir kanonik. Isi QR serta path/folder-nya wajib
   sesuai MRN akhir itu.
10. **`AC-10`** — File/folder QR Pilot lama wajib tidak tersentuh selama rekonsiliasi.
11. **`AC-11`** — Bila pembuatan QR gagal, `MstPatient` wajib tetap tidak berubah.
12. **`AC-12`** — Bila `SaveChanges`/*commit* database gagal sesudah QR baru dibuat, hanya artefak
    QR yang baru dibuat yang boleh dibersihkan secara *best-effort*. QR lama tidak boleh dihapus.
13. **`AC-13`** — Artefak QR tujuan yang tak terduga sudah ada untuk MRN akhir tidak boleh
    ditimpa diam-diam. Keadaan itu wajib menghasilkan konflik yang aman atau penanganan eksplisit
    yang setara.
14. **`AC-14`** — Operasi wajib dapat dilanjutkan dan idempoten, artinya aman dipanggil berulang
    tanpa hasil ganda. Bila MRN sudah kanonik, pasien diklasifikasikan `ALREADY_RECONCILED`.
15. **`AC-15`** — Bila MRN sudah kanonik tetapi `QrCodePath` tidak sesuai MRN kanonik, laporkan
    `QR_INCONSISTENT`, bukan menebak perbaikannya diam-diam.
16. **`AC-16`** — Pembangkit MRN normal tidak boleh dipanggil untuk pasien Pilot ini.
17. **`AC-17`** — Tidak boleh ada `PatientCode` baru yang dibangkitkan.
18. **`AC-18`** — Tidak boleh ada `MstPatient` baru yang dibuat untuk 715 pasien Pilot.
19. **`AC-19`** — Tidak boleh ada `MstPatient` yang dihapus lalu dibuat ulang.
20. **`AC-20`** — Crosswalk tidak boleh diubah.
21. **`AC-21`** — Tabel perencana MRN kanonik tidak boleh diubah.
22. **`AC-22`** — Tabel cadangan *rollback* tidak boleh diubah.
23. **`AC-23`** — 704.508 pasien yang belum dibuat berada di luar cakupan task ini.
24. **`AC-24`** — Eksekusi dibatasi paling banyak `request.limit` baris, dengan urutan
    `legacy_pid` yang deterministik.
25. **`AC-25`** — Pemrosesan memakai satu transaksi per pasien, sehingga kegagalan tidak
    membatalkan pasien yang sudah selesai sebelumnya.
26. **`AC-26`** — Pada perilaku eksekusi awal, proses berhenti pada kegagalan tak terduga yang
    pertama, lalu mengembalikan/melaporkan pasien yang gagal secara aman.
27. **`AC-27`** — Test terfokus wajib mencakup: *dry run*; gerbang otorisasi/environment;
    validasi `limit`; konflik MRN; pelestarian identitas; pelestarian `PatientCode`; perilaku QR;
    idempotensi; kegagalan QR; kegagalan database sesudah QR; keutuhan crosswalk; keutuhan
    perencana; keutuhan cadangan; serta perilaku `limit` dan urutan yang deterministik.
28. **`AC-28`** — Validasi build wajib mencakup `dotnet build -p:RunAnalyzers=false` dan test
    terfokus yang sesuai.

Penomoran `AC-xx` di atas adalah penomoran pengguna. Pada traceability, butir yang sama ditulis
`PAT-MIG-AC-xx` supaya unik di seluruh repository.

#### Verifikasi

| No | Bukti | Bentuk yang dicatat pada laporan task |
| ---: | --- | --- |
| V1 | Kepatuhan governance dan aturan | QBE preflight: Area, Module, prefix, kelas keberlakuan, dan QBE ID yang benar-benar berlaku; kecocokan `[AccessAction]` dan `[AccessPermission]` |
| V2 | Inspeksi source terhadap `QuilvianStaDeploy` terkini | SHA saat build; bila berbeda dari `103b45cc`, kutipan baris pada kontrak diperiksa ulang lebih dulu |
| V3 | Test terfokus lulus | Perintah test yang dijalankan beserta jumlah lulus/gagal apa adanya, untuk seluruh topik `AC-27` |
| V4 | Build backend lulus | Keluaran `dotnet build -p:RunAnalyzers=false`, jumlah error dan warning apa adanya |
| V5 | Inspeksi `git diff` | Daftar berkas berubah beserta alasannya; tidak ada perubahan di luar cakupan |
| V6 | Tidak ada artefak QR masuk Git | `git status --short` tidak memuat berkas di bawah `patient-qrcodes/` maupun file `.png` hasil runtime |
| V7 | Tidak ada eksekusi database selama implementasi | Pernyataan eksplisit: tidak ada EF migration dibuat/diterapkan, tidak ada perintah database |
| V8 | Tidak ada pemanggilan endpoint selama implementasi | Pernyataan eksplisit pada laporan |
| V9 | Tidak ada commit, push, atau deploy tanpa otorisasi terpisah | `git status --short` dan pernyataan eksplisit pada laporan |
| V10 | Verifikasi kontrak API dan proses bisnis | Method, path, request/response, kode status, dan izin yang benar-benar terpasang; penelusuran langkah `api-contract.md` bagian 4.2 pada source, termasuk default `dryRun=true`, pencocokan persis `Staging`, dan pemeriksaan QR sebelum helper |
| V11 | Model EF tidak berubah (`PAT-OQ-007`) | Review diff: tidak ada berkas di `Migrations/` yang berubah, dan tidak ada pemetaan ke `migration.rsmmc_*` yang membuat model menuntut EF migration. Perintah EF tooling apa pun, termasuk yang hanya memeriksa, tidak dijalankan tanpa izin eksplisit |

Baris status automated test pada laporan ditulis sebagai `AUTOMATED TEST: <perintah> — PASS/FAIL`,
karena pemilik memintanya secara eksplisit (`PAT-DEC-014`).

#### Definition of Done — fase implementasi

- [x] Endpoint diimplementasikan sesuai kontrak `1.1.0`
- [x] Test terfokus lulus
- [x] Build backend lulus
- [x] Tidak ada perubahan berkas yang tidak dimaksud
- [x] Laporan task implementasi lengkap di `../task/report/backend/BE-PAT-MIG-001.md`
- [x] Source sudah direview pengguna/leader
- [x] **Belum ada** rekonsiliasi runtime yang dijalankan

Butir "source sudah direview pengguna/leader" adalah tindakan manusia. Selama butir itu belum
terpenuhi, task paling tinggi berstatus 🟡, walaupun build dan test sudah lulus.

---

## Register status

| Task | Judul | Status | Laporan |
| --- | --- | --- | --- |
| `BE-PAT-MIG-001` | RSMMC Pilot MRN Reconciliation | ✅ 9 Oktober 2026 — **SOURCE IMPLEMENTATION: APPROVED** (review source leader `APPROVED`); **RUNTIME: PENDING** (`PAT-GATE-001` terbuka). Riwayat: 🟡 9 Oktober 2026 — 28 dari 28 AC terbukti di source/test pada baseline Integration `179aea3f` (test `73/73`, build `0` error, QBE `Strict` `PASS`); review source belum; runtime belum (`PAT-GATE-001`) | [BE-PAT-MIG-001](../task/report/backend/BE-PAT-MIG-001.md) |

---

## Gerbang dan butir terbuka

### Keputusan review — seluruhnya `APPROVED` 8 Oktober 2026

Pada `roadmap_revision` `1`, `PAT-OQ-001` sampai `PAT-OQ-004` masih `PROPOSED`. Seluruhnya kini
final, dan tiga keputusan baru ditambahkan. **Jumlah pertanyaan terbuka: 0.**

| ID | Keputusan final | Menahan apa |
| --- | --- | --- |
| `PAT-OQ-001` | Project xUnit khusus `Tests/QuilvianSystemBackend.PatientManagementTests/`, tanpa entri `.sln`, dengan governance guard | Tidak menahan. Bersyarat pada pemeriksaan `TEST_POLICY` saat build; bila ditolak, hanya bagian test yang berhenti dan keputusan diminta |
| `PAT-OQ-002` | Hanya `Staging` persis, untuk kedua mode `dryRun` | Tidak menahan |
| `PAT-OQ-003` | `dryRun` yang tidak dikirim = `true`; tidak ada default implisit `false` | Tidak menahan |
| `PAT-OQ-004` | Pasien kanonik tidak dihitung dalam `limit`; `QR_INCONSISTENT` dilaporkan, tidak diperbaiki | Tidak menahan |
| `PAT-OQ-005` | Path QR tujuan diperiksa sebelum helper; bila ada, `CONFLICT` tanpa menimpa | Tidak menahan |
| `PAT-OQ-006` | Daftar izin positif yang gagal tertutup; bukan `!environment.IsProduction()` saja | Tidak menahan |
| `PAT-OQ-007` | Tidak ada EF migration atau perubahan skema untuk membaca `migration.rsmmc_*` | Tidak menahan |

### Gerbang sesudah task

| ID | Gerbang | Pemilik | Keadaan |
| --- | --- | --- | --- |
| `PAT-GATE-001` | Otorisasi eksplisit untuk deployment staging, pemanggilan endpoint, dan eksekusi rekonsiliasi 715 pasien | Pemilik/leader PatientManagement | **Terbuka.** Baru boleh diminta sesudah `BE-PAT-MIG-001` lulus review source, build, dan test. Pembaruan 9 Oktober 2026: prasyarat itu kini terpenuhi (review source leader `APPROVED`), tetapi otorisasi eksekusinya **belum** diberikan — runtime tetap `PENDING` |

### Gap dokumen

| ID | Gap | Keadaan |
| --- | --- | --- |
| `PAT-GAP-001` | Blueprint ini hanya memuat berkas minimum untuk satu work item. 11 dari 14 berkas pasti bentuk `SINGLE` belum dibuat — rinciannya di `blueprint-manifest.md` | **Terbuka**, sengaja. Tidak menahan `BE-PAT-MIG-001`, tetapi `verify-module-readiness` untuk modul PatientManagement secara utuh akan mencatatnya |

---

## Risiko

| ID | Risiko | Penanganan yang diwajibkan | Pemilik |
| --- | --- | --- | --- |
| `PAT-RSK-001` | Tabel `migration.rsmmc_*` belum dipetakan di source. Menambah entity EF biasa dapat memunculkan perubahan model yang menuntut EF migration | Diputuskan `PAT-OQ-007`: tidak ada EF migration dan tidak ada perubahan skema. Mekanisme baca berparameter dipilih saat build; bukti V11 | Developer build |
| `PAT-RSK-002` | `SavePatientQrCodeFile` menimpa file yang sudah ada (baris 1144) | Diputuskan `PAT-OQ-005`: periksa path tujuan sebelum helper dipanggil; bila ada, `CONFLICT`. Sisa risiko: jeda singkat antara pemeriksaan dan penulisan — cara menutupnya `DEV_DISCRETION`, dicatat pada laporan | Developer build |
| `PAT-RSK-003` | Pola `!environment.IsProduction()` di source gagal terbuka | Diputuskan `PAT-OQ-006`: daftar izin positif berisi `Staging` saja (`AC-05`, `PAT-OQ-002`) | Developer build |
| `PAT-RSK-004` | Test berbasis EF Sqlite belum tentu dapat meniru skema `migration` PostgreSQL atau query khusus PostgreSQL | Desain test dan batasannya dicatat apa adanya pada laporan. Topik `AC-27` yang tidak dapat dibuktikan disebut terbuka, bukan ditandai lulus | Developer build |
| `PAT-RSK-005` | Pilihan A pada `api-contract.md` bagian 7 membuka kemampuan ubah MRN massal bagi setiap pemegang izin `Patient : Update` di staging | Rekomendasi pilihan B; pilihan akhir dicatat pada laporan | Pemilik/leader |
| `PAT-RSK-006` | Dua admin memanggil endpoint pada waktu hampir bersamaan, sehingga pasien yang sama berpotensi diproses dua kali | Syarat `AC-01` diperiksa ulang di dalam transaksi per pasien sebelum mengubah data | Developer build |
| `PAT-RSK-007` | `TEST_POLICY` menyatakan folder `Tests/` sudah dihapus, padahal branch ini masih men-*track* 40 berkas di tiga project test yang tidak terdaftar di `.sln` | Dicatat sebagai selisih governance. Tidak diubah oleh task ini. Project test baru dibuat hanya lewat governance guard `PAT-OQ-001` | Pemilik suite Skill |
| `PAT-RSK-008` | Nama environment pada server staging yang berjalan tidak diperiksa. Bila ejaannya bukan persis `Staging` — misalnya `staging` — endpoint akan menolak di sana | Gagal tertutup, sehingga aman. Terlihat pada simulasi pertama sesudah `PAT-GATE-001`. Perbaikannya ada di konfigurasi deployment, **bukan** dengan melonggarkan pencocokan | Pemilik/leader, saat `PAT-GATE-001` |
