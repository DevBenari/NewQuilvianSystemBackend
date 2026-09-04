# Laporan Perubahan Backend — `BE-LAB-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-03` |
| Judul | Riwayat dan pengajuan perubahan batas kritis |
| Slice | `S3` — pengelolaan batas nilai (`roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3 |
| Trace | `FR-03.4`, `FR-03.5`; `LAB-DEC-023` (BR-19); `LAB-STATE-v1` r2 bagian 4; `LAB-API-v1` r3 grup Lab Critical Bound Approval; `erd/data-dictionary.md` bagian 7, 8, 11.4, dan 11.5 |
| Contract version | `LAB-API-v1` r3 dan `LAB-STATE-v1` r2 — `approved`, dikunci 2026-09-02. Task ini **tidak** menyentuh satu pun endpoint; keduanya dipakai sebagai target bentuk data dan daur hidup, bukan sebagai permukaan yang diubah |
| Dependency | `BE-LAB-02` — **`SELESAI`** 2026-09-02, [laporan](BE-LAB-02.md). Kedua tabel task ini menunjuk ke `LabValueBound` yang dibuat di sana. `BE-LAB-04` dan `BE-LAB-05` bergantung pada task ini |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 0, database 2, keamanan 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Repositories/Configurations/HealthServices/LaboratoryManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/`, `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/`, dan `docs/module-blueprints/laboratorium/` beserta pembaruan bukti pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `d8d67c3` — *Merge remote-tracking branch 'origin/QuilvianIntegrationBackend' into yoga*, 2026-09-02, branch `yoga`. Roadmap menyebut snapshot `c87d9c0`; selisihnya diperiksa dan tidak menyentuh permukaan task ini (lihat bagian 7) |
| Tanggal | 2026-09-02 |
| Status | **Selesai.** Source, test, pembuatan migration, dan eksekusi migration ke database dev pemilik seluruhnya tuntas dan terverifikasi dua arah. Seluruh butir DoD terpenuhi. Dua tambahan di luar kamus data dicatat apa adanya pada bagian 3.3 |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-02`, sistem sudah punya tempat menyimpan batas nilai — termasuk **batas kritis**,
angka yang menentukan kapan seorang pasien dinyatakan dalam bahaya. Tetapi tabel itu terbuka
lebar: siapa pun yang boleh mengubah batas nilai boleh mengubah batas kritis, langsung berlaku,
tanpa jejak.

Inilah kejanggalan yang ditemukan `LAB-DEC-023`. `MstLabRejectionReason` sudah punya dua kolom
yang **terkunci** dari kepala instalasi karena menentukan siapa menanggung biaya ambil ulang.
Sementara batas kritis — yang menentukan keselamatan pasien, bukan biaya — justru boleh diubah
bebas. Perlindungan atas angka biaya lebih ketat daripada perlindungan atas angka keselamatan.

**Contoh yang dicegah, dan alasannya sulit terdeteksi:**

> Kepala instalasi merasa peringatan nilai kritis terlalu sering muncul dan mengganggu pekerjaan
> harian. Ia menaikkan batas kritis atas Kalium dari 6,0 menjadi 8,0 mmol/L.
>
> Sejak saat itu, pasien dengan Kalium 7,2 mmol/L **tidak lagi** memicu kewajiban pelaporan
> nilai kritis. Tidak ada aturan yang dilanggar, tidak ada baris yang mencurigakan, dan tidak
> ada satu pun jejak yang bisa ditelusuri kemudian. Yang berubah hanya satu angka.

Ada juga masalah kedua yang lebih sederhana: **tidak ada riwayat sama sekali**. Ketika batas
normal Kalium bergeser dari 5,1 ke 5,3 karena laboratorium mengganti alat, tidak ada tempat yang
merekam siapa mengubahnya, kapan, dari berapa, dan mengapa. Hasil pemeriksaan lama menjadi
mustahil dinilai ulang, karena batas yang berlaku saat itu tidak diketahui lagi.

Task ini menyediakan **fondasi datanya**: satu tabel pengajuan yang menahan perubahan batas
kritis sampai pihak klinis memutuskan, dan satu tabel riwayat permanen untuk seluruh perubahan.
Endpoint pengelolaan dan penyetujuannya adalah pekerjaan `BE-LAB-04` dan `BE-LAB-05`, yang
menjadikan task ini sebagai dependency-nya.

---

## 2. Proses bisnis

**Tujuan.** Setiap perubahan batas nilai meninggalkan riwayat permanen, dan perubahan batas
kritis tidak berlaku sebelum disetujui pihak klinis.

**Pelaku.** Kepala instalasi laboratorium sebagai pengaju, dan pemegang kewenangan persetujuan
batas kritis sebagai pemutus. Keduanya wajib orang yang berbeda.

**Pemicu.** Kepala instalasi hendak mengubah isi sebuah batas nilai.

**Langkah yang berurutan:**

1. Kepala instalasi menentukan kolom mana yang hendak ia ubah.
2. Sistem memilah menurut `LAB-DEC-023`:

   | Yang diubah | Perlu persetujuan | Riwayat |
   |---|:---:|:---:|
   | Satuan hasil | Tidak | Ya |
   | Batas normal bawah dan atas | Tidak | Ya |
   | Daftar pilihan sah dan penanda di luar rujukan | Tidak | Ya |
   | Batas waktu penyelesaian cito | Tidak | Ya |
   | **Batas kritis bawah dan atas** | **Ya** | Ya |
   | **Penanda pilihan yang dianggap kritis** | **Ya** | Ya |

3. Untuk kolom **tanpa** persetujuan: perubahan langsung berlaku, dan satu baris riwayat
   diterbitkan dengan penyetuju **kosong**.
4. Untuk kolom **dengan** persetujuan: tidak ada yang berubah pada batas nilai. Yang terbentuk
   adalah satu **pengajuan** berstatus `Submitted`, memuat nilai usulan beserta alasannya.
   Selama itu batas yang berlaku **tidak bergerak sedikit pun**.
5. Pemutus dari pihak klinis menyetujui, menolak, atau pengajunya sendiri menariknya. Ketiganya
   status terminal.
6. Bila disetujui: batas kritis pada batas nilai diperbarui, dan satu baris riwayat diterbitkan
   dengan penyetuju **terisi**.

**Aturan yang berlaku:**

| Aturan | Isi |
| --- | --- |
| `LAB-DEC-023` | Batas kritis hanya berubah lewat pengajuan yang disetujui pihak klinis |
| `LAB-STATE-v1` r2 §4 | `Submitted` → `Approved`, `Rejected`, atau `Withdrawn`. Ketiganya terminal |
| `VAL-33` | Pengaju tidak boleh menyetujui pengajuannya sendiri — ditolak `403` |
| `VAL-32` | Pengajuan kedua saat yang pertama belum diputuskan ditolak `409` |
| `VAL-28` | Mengubah batas kritis lewat jalur ubah biasa ditolak `422` |
| `AC-34` | Riwayat memuat kolom yang berubah, nilai lama, nilai baru, pelaku, penyetuju, waktu, dan alasan |

**Status yang dihasilkan.** Satu daur hidup baru, `LabBoundChangeStatus`, dengan empat nilai
sesuai `LAB-STATE-v1` r2. Tidak ada status lama yang disentuh.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Pengajuan masih `Submitted` | Batas kritis yang berlaku **tidak berubah sama sekali**. Terbukti langsung terhadap database, lihat bagian 5.1 |
| Ada yang mencoba mengubah baris riwayat yang sudah tersimpan | Ditolak lapisan penyimpanan dengan `InvalidOperationException`; nilai lama tidak berubah |
| Batas nilai dihapus sementara masih punya pengajuan atau riwayat | Ditolak database (`Restrict`). Terbukti langsung, lihat bagian 5.1 |
| Perubahan batas normal | Riwayat terbit dengan penyetuju kosong — dan kekosongan itu sah, bukan data yang belum diisi |
| Dua pemutus menyetujui pengajuan yang sama bersamaan | Ditolak token konkurensi `Version`; hanya satu yang berhasil |

**Hasil akhir.** Batas kritis kini punya tempat penahanan sebelum berlaku, dan seluruh perubahan
batas nilai punya tempat rekam yang tidak dapat dihapus maupun diubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:**

- `AGENTS.md`; `CLAUDE.md`
- `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `docs/engineering/QBE_EXCEPTIONS.json`
- `tooling/qbe/Invoke-QbeConformanceCheck.ps1`
- `rules/GLOBAL_RULES.md`; `rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`; `rules/backend/DATABASE_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`

**Blueprint:**

- `roadmap/backend-roadmap.md` bagian 3 dan 7; `roadmap/traceability.md`
- `contracts/state-transition-matrix.md` bagian 4 beserta daftar transisi tidak sah
- `contracts/api-contract.md` grup Lab Critical Bound Approval
- `00-interview-decisions.md` (BR-19, `LAB-DEC-023`, `AC-33`, `AC-34`)
- `01-existing-capability-map.md` (`CAP-04` pola riwayat, `CAP-17` pola konkurensi)
- `02-backend-architecture.md` bagian 3.2 dan 597
- `erd/data-dictionary.md` bagian 7, 8, 11.4, dan 11.5

**Source:**

- `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs`
- `Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs`; `.../Models/TrxLabTransitionHistory.cs` (pola `CAP-04`)
- `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` (pola `Version`, `CAP-17`)
- `Areas/HealthServices/MasterData/Models/MstProcedure.cs`
- `Models/IdentityModel.cs`; `Repositories/ApplicationDbContext.cs`
- `Repositories/Configurations/HealthServices/LaboratoryManagement/TrxLabTransitionHistoryConfiguration.cs`; `.../LabValueBoundConfiguration.cs`
- `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | Menambah enum `LabBoundChangeStatus` berisi empat nilai sesuai `LAB-STATE-v1` r2 bagian 4: `Submitted = 1`, `Approved = 2`, `Rejected = 3`, `Withdrawn = 4`. Nilai enum lama tidak disentuh |
| `Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundChangeRequest.cs` | **Baru.** Entity pengajuan perubahan batas kritis. Nilai usulan disimpan terpisah dari nilai yang berlaku, justru supaya keduanya tidak mungkin tertukar |
| `Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundHistory.cs` | **Baru.** Entity riwayat permanen. Satu baris menjawab tujuh hal yang dituntut `AC-34` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundChangeRequestConfiguration.cs` | **Baru.** Memetakan tabel `public."LabValueBoundChangeRequest"`, enum sebagai `int`, dua kolom usulan ber-presisi `18,4`, token konkurensi `Version`, dua index, dan relasi `Restrict` ke `LabValueBound` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundHistoryConfiguration.cs` | **Baru.** Memetakan tabel `public."LabValueBoundHistory"`, memasang tolak-ubah pada delapan kolom faktanya, index gabungan `ValueBoundId` + `OccurredAt`, dan relasi `Restrict` |
| `Repositories/ApplicationDbContext.cs` | Menambah dua `DbSet` di dalam region `HEALTH SERVICE - Laboratory Management`. Tidak ada region lain yang tersentuh |
| `Migrations/20260902085636_AddLabValueBoundApprovalAndHistory.cs` | **Baru.** `Up` membuat dua tabel beserta dua foreign key `Restrict` dan tiga index. `Down` membuang kedua tabel |
| `Migrations/20260902085636_AddLabValueBoundApprovalAndHistory.Designer.cs` | **Baru.** Berkas hasil generate yang menyertai migration |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bertambah 373 baris kumulatif bersama `BE-LAB-02`, **nol baris terhapus**. Diperiksa: seluruhnya milik keempat entity baru |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundApprovalTests.cs` | **Baru.** Sembilan kasus uji yang membuktikan acceptance criteria task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Task ini tidak menambah, menghapus, mengganti nama, maupun mengubah satu pun endpoint, DTO, atau kode status. Lima endpoint grup Lab Critical Bound Approval pada `LAB-API-v1` r3 tetap berstatus **Rencana (belum tersedia)**; membangunnya adalah cakupan `BE-LAB-05` |
| Database | Dua tabel baru `public."LabValueBoundChangeRequest"` dan `public."LabValueBoundHistory"`, dua foreign key `Restrict` ke `LabValueBound`, dan tiga index. Migration **sudah dibuat dan sudah diterapkan** ke `QuilvianNewDevYoga` atas wewenang eksplisit pada sesi ini. Jalur `Down` ikut dibuktikan. Lihat bagian 5.1 |
| Keamanan/Auth | **Berkaitan, tetapi bukan intinya.** Tidak ada permission baru, tidak ada endpoint, dan tidak ada kode authorization yang ditulis. Yang disediakan task ini adalah bentuk data yang membuat aturan keselamatan dapat ditegakkan kemudian: `RequestedByUserId` dan `DecidedByUserId` berdiri sebagai dua kolom terpisah **justru** supaya `VAL-33` — pengaju tidak boleh menyetujui pengajuannya sendiri — dapat diperiksa dengan membandingkan keduanya. `CAP-16` sudah membuktikan sistem permission yang ada tidak dapat menegakkan aturan itu: `AccessPermissionService.HasAccessAsync` hanya menjawab boleh atau tidak dan tidak pernah membandingkan pelaku sebelumnya. Penegakannya wajib ditulis di dalam service `BE-LAB-05` |

**Dua tambahan di luar daftar kolom kamus data.**

Keduanya penambahan, bukan pengurangan — tidak ada satu pun kolom yang disebut
`erd/data-dictionary.md` yang saya hilangkan.

**Pertama: kolom `Version` pada `LabValueBoundChangeRequest`.**

Kamus data bagian 7 tidak menyebutnya. Tetapi baris **Reuse** pada roadmap `BE-LAB-03` menyebut
secara eksplisit: *"`CAP-04` sebagai pola riwayat; `CAP-17` `Version` sebagai pola perlindungan
konkurensi."* `CAP-17` adalah `LabOrder.Version` dan `TrxLabSpecimen.Version` yang sudah berjalan
di modul ini.

Alasannya konkret. Dua pemutus yang membuka pengajuan yang sama lalu menyetujuinya hampir
bersamaan akan sama-sama berhasil tanpa token konkurensi, dan keduanya menulis batas kritis ke
batas nilai yang sama. Yang terjadi kemudian bergantung pada urutan penulisan — persis kelas
kesalahan yang `CAP-17` ada untuk mencegahnya.

`LabValueBoundHistory` sengaja **tidak** diberi `Version`: ia hanya ditambah, tidak pernah
diperbarui, sehingga tidak ada yang bisa bertabrakan.

**Kedua: tolak-ubah pada kolom fakta `LabValueBoundHistory`.**

Roadmap menuliskan outcome task ini sebagai *"setiap perubahan batas menghasilkan riwayat
**permanen**"*. Pola `CAP-04` yang sudah berjalan — `TrxLabTransitionHistory` — mewujudkan
"permanen" hanya lewat ketiadaan jalur update di service.

Di sini itu dinilai belum cukup, dan alasannya bukan teori: riwayat inilah satu-satunya bukti
bahwa sebuah batas kritis pernah bernilai lain. Riwayat yang dapat diubah bukan riwayat. Karena
itu delapan kolom faktanya — `ValueBoundId`, `ChangedField`, `OldValue`, `NewValue`,
`ActorUserId`, `ApprovedByUserId`, `ChangeReason`, dan `OccurredAt` — dipasangi
`PropertySaveBehavior.Throw`, sehingga siapa pun yang kelak menulis jalur ubah baru akan ditolak
lapisan penyimpanan, bukan diam-diam berhasil.

Preseden pendekatan ini ada di modul yang sama: `BE-LAB-01` memasang penjaga serupa pada
`LabOrder.Discipline` untuk `INV-21`, dengan alasan yang sama persis.

Kolom audit bawaan sengaja **tidak** ikut dikunci, sehingga perilaku soft delete dan jejak audit
repository tetap berjalan seperti biasa.

**Selisih kecil yang tidak perlu keputusan.** Blok DDL pada kamus data menuliskan `timestamp`
untuk `RequestedAt`, `DecidedAt`, dan `OccurredAt`. Yang dihasilkan adalah
`timestamp with time zone`, karena itulah pemetaan bawaan Npgsql untuk `DateTime` dan itulah
yang dipakai seluruh kolom waktu di repository ini, termasuk kolom audit dan
`TrxLabTransitionHistory.OccurredAt`. Blok DDL kamus data sendiri menyatakan dirinya "bentuk
tabel sebagaimana dihasilkan EF Core, bukan skrip untuk dijalankan", sehingga tidak ada kontrak
yang dilanggar.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Cakupannya berhenti pada entity,
configuration, `DbSet`, dan migration, persis seperti tertulis pada roadmap.

Lima endpoint grup **Health Services / Laboratory Management / Lab Critical Bound Approval** pada
`LAB-API-v1` r3 tetap berstatus **Rencana (belum tersedia)** dan dibangun `BE-LAB-05`. Enam
endpoint grup Lab Value Bound dibangun `BE-LAB-04`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `186 Warning(s)` — jumlahnya sama persis dengan baseline sebelum task ini |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Lolos | `PASS` | Dijalankan dua kali, terakhir pada tree final: `Files evaluated: 17`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` |
| `dotnet ef migrations has-pending-model-changes` sesudah migration dibuat | Tidak ada selisih tersisa | `PASS` | `No changes have been made to the model since the last migration.` |
| `AC-33` — pengajuan tertahan `Submitted` sementara batas lama tidak berubah | Sesuai harapan | `PASS` | `PengajuanPerubahanBatasKritis_TertahanSementaraBatasLamaTidakBerubah`. Dibuktikan ulang terhadap database sungguhan: sesudah usulan 8,0 masuk, `CriticalHigh` terbaca tetap `6.0000` — bagian 5.1 |
| `AC-34` — perubahan batas normal menerbitkan riwayat tanpa penyetuju | Sesuai harapan | `PASS` | `PerubahanBatasNormal_MenerbitkanSatuBarisRiwayatTanpaPenyetuju` |
| `AC-34` — perubahan batas kritis menerbitkan riwayat beserta penyetujunya | Sesuai harapan | `PASS` | `PerubahanBatasKritisYangDisetujui_MenerbitkanRiwayatBesertaPenyetujunya`, memeriksa ketujuh hal yang dituntut dalam satu baris |
| Satu batas nilai dapat punya beberapa baris riwayat berurutan waktu | Sesuai harapan | `PASS` | `SatuBatasNilai_DapatMemilikiBeberapaBarisRiwayatBerurutanWaktu` |
| **Gagal** — mengubah baris riwayat yang sudah tersimpan | Ditolak, nilai lama tidak berubah | `PASS` | `MengubahBarisRiwayatYangSudahTersimpan_Ditolak` |
| **Gagal** — menghapus batas nilai yang masih punya pengajuan | Ditolak foreign key `Restrict` | `PASS` | Bagian 5.1, probe yang di-rollback |
| Penamaan kedua entity tidak memakai awalan `Trx` | Sesuai harapan | `PASS` | `KeduaEntityBaru_TidakMemakaiAwalanTrx`, memeriksa nama class **dan** nama tabelnya |
| Enum memuat empat status sesuai state matrix | Sesuai harapan | `PASS` | `LabBoundChangeStatus_MemuatEmpatStatusSesuaiStateMatrix` |
| Pemetaan `LabValueBoundChangeRequest` sesuai kamus data, termasuk token konkurensi | Sesuai harapan | `PASS` | `LabValueBoundChangeRequest_TerpetakanSesuaiKamusData` |
| Pemetaan `LabValueBoundHistory` dan perilaku tolak-ubah delapan kolom faktanya | Sesuai harapan | `PASS` | `LabValueBoundHistory_TerpetakanSesuaiKamusDataDanBerperilakuTolakUbah` |
| Seluruh test `LabValueBoundApprovalTests` | Hijau | `PASS` | `dotnet test --filter FullyQualifiedName~LabValueBoundApprovalTests` → `Failed: 0, Passed: 9, Skipped: 0, Total: 9` |
| Uji ulang pada kondisi tree final, sesudah migration dibuat dan diterapkan — seluruh test Laboratorium | Hijau | `PASS` | `dotnet test --filter FullyQualifiedName~LaboratoryManagement` → `Failed: 0, Passed: 28, Skipped: 0, Total: 28`, yaitu 9 milik `BE-LAB-01`, 10 milik `BE-LAB-02`, dan 9 milik task ini. Dijalankan paling akhir supaya bukti test mencerminkan isi worktree yang diserahkan |
| Seluruh suite `QuilvianSystemBackend.Tests` | 871 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | `Failed: 1, Passed: 871, Total: 872, Duration: 38 s`. Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` milik modul Billing — temuan `FINAL`/`CLOSED` yang sudah tercatat pada `BE-LAB-01` bagian 9.2. Baseline sesudah `BE-LAB-02` adalah `Passed: 862, Failed: 1`; selisihnya tepat **+9**, yaitu test baru task ini, sehingga tidak ada regresi |
| Eksekusi migration `Up` ke `QuilvianNewDevYoga` | Berhasil | `PASS` | Bagian 5.1 |
| Eksekusi migration `Down` ke `QuilvianNewDevYoga` | Berhasil, database kembali persis ke keadaan semula | `PASS` | Bagian 5.1 |
| Eksekusi `Up` ulang sesudah `Down` | Berhasil | `PASS` | Bagian 5.1 |
| `LabValueBound` milik `BE-LAB-02` tidak tersentuh sepanjang eksekusi | Terbukti | `PASS` | Jumlah kolomnya terbaca tetap **23** pada keempat titik pengukuran bagian 5.1 |
| Empat migration tertunda milik modul lain tetap tidak tersentuh | Terbukti | `PASS` | `dotnet ef migrations list` sesudah eksekusi: `AddRadiologyManagement`, `AddCompanyGuarantorToPatientEncounterGuarantor`, `RenameClinicalMilestoneFactToCliPrefix`, dan `RepairCanonicalModelSnapshotBaseline` seluruhnya masih `(Pending)` |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT APPLICABLE` | Task ini tidak menghasilkan satu pun endpoint |

Uji manual lewat antarmuka: `NOT APPLICABLE` — tidak ada layar maupun endpoint yang dihasilkan
task ini.

**Tidak dijalankan:**

- **`VAL-28`, `VAL-32`, dan `VAL-33`.** Ketiganya validasi pada endpoint pengajuan dan
  persetujuan, yang merupakan cakupan `BE-LAB-04` dan `BE-LAB-05`. Bentuk data yang membuat
  ketiganya dapat ditegakkan sudah tersedia — khususnya dua kolom pelaku yang terpisah untuk
  `VAL-33`, dan index `RequestStatus` untuk memeriksa pengajuan yang masih terbuka pada `VAL-32`.
- **Transisi status `Submitted` → `Approved`/`Rejected`/`Withdrawn`.** Perpindahan status adalah
  pekerjaan service `BE-LAB-05`. Yang dibuktikan di sini adalah keadaan awal `Submitted` dan
  bahwa batas yang berlaku tidak bergerak selama pengajuan belum diputuskan.
- **Tabrakan dua pemutus secara sungguhan.** Token konkurensi terbukti terpasang pada model;
  membuktikan tabrakannya memerlukan service dan dua permintaan bersamaan, yang keduanya baru
  ada pada `BE-LAB-05`.
- **Empat migration tertunda milik modul lain**, dan **eksekusi ke database selain
  `QuilvianNewDevYoga`.** Bukan wewenang task ini.

### 5.1 Bukti eksekusi migration

**Wewenang.** Pemilik repository memilih "buat migration dan jalankan ke `QuilvianNewDevYoga`"
untuk task ini, sebagai konfirmasi tersendiri — wewenang `BE-LAB-02` tidak diperlakukan berlaku
otomatis, sesuai `CLAUDE.md`.

**Gerbang yang ditemukan sebelum eksekusi.** Sama seperti dua task sebelumnya, `QuilvianNewDevYoga`
masih memiliki empat migration tertunda milik modul lain dengan riwayat tidak berurutan, sehingga
`dotnet ef database update` polos akan menerapkan jauh lebih banyak daripada yang diberi wewenang.

Karena itu yang dijalankan hanya SQL milik migration ini, dan SQL-nya **tidak ditulis tangan**
melainkan dihasilkan EF sendiri:

```text
dotnet ef migrations script 20260902082722_AddLabValueBoundAndOption 20260902085636_AddLabValueBoundApprovalAndHistory
dotnet ef migrations script 20260902085636_AddLabValueBoundApprovalAndHistory 20260902082722_AddLabValueBoundAndOption
```

Urutan migration diperiksa lebih dulu terhadap assembly yang **sudah dibangun ulang** — pemeriksaan
pertama sempat memakai assembly lama sehingga migration baru belum terlihat, dan itu diulang.
`AddLabValueBoundAndOption` adalah migration tepat sebelum migration ini, tanpa satu pun migration
lain di antaranya.

Eksekusinya lewat runner Npgsql sementara di luar repository yang membaca connection string dari
`appsettings.Development.json` saat berjalan. Runner menolak berjalan bila nama database tujuannya
bukan `QuilvianNewDevYoga`, menjalankan setiap berkas dalam satu transaksi, dan tidak pernah
menuliskan maupun mencetak credential.

**Urutan bukti yang direkam:**

| Langkah | Kedua tabel baru | Index | Foreign key | Baris `__EFMigrationsHistory` | Kolom `LabValueBound` |
| --- | --- | --- | --- | --- | --- |
| Sebelum apa pun dijalankan | TIDAK ADA | TIDAK ADA | TIDAK ADA | TIDAK ADA | 23 |
| Sesudah `Up` | ADA, seluruh kolom sesuai kamus data | 3 index + 2 primary key | 2 | ADA | 23 |
| Sesudah `Down` | TIDAK ADA | TIDAK ADA | TIDAK ADA | TIDAK ADA | 23 |
| Sesudah `Up` ulang | ADA | 3 index + 2 primary key | 2 | ADA | 23 |

Foreign key yang terbaca langsung dari `pg_constraint` sesudah `Up`:

```text
FK_LabValueBoundChangeRequest_LabValueBound_ValueBoundId → "LabValueBound"("Id") ON DELETE RESTRICT
FK_LabValueBoundHistory_LabValueBound_ValueBoundId       → "LabValueBound"("Id") ON DELETE RESTRICT
```

**Probe perilaku yang di-rollback.** Sesudah `Up`, satu blok uji dijalankan terhadap database
sungguhan **di dalam transaksi yang selalu dibatalkan**, untuk membuktikan perilaku yang tidak
dapat dibuktikan provider InMemory. Hasilnya:

```text
kritis atas sesudah pengajuan:   6.0000   (usulan 8,0 sudah tersimpan, batas berlaku tidak bergerak)
status pengajuan:                1        (Submitted)
baris riwayat:                   1
hapus batas nilai bertautan:     DITOLAK oleh update or delete on table "LabValueBound"
                                 violates foreign key constraint
                                 "FK_LabValueBoundChangeRequest_LabValueBound_ValueBoundId"
```

Baris pertama adalah `AC-33` yang terbukti pada database sungguhan, bukan hanya pada model:
usulan 8,0 sudah tersimpan sebagai pengajuan, sementara batas kritis yang berlaku tetap 6,0 —
sehingga pasien dengan Kalium 7,2 mmol/L masih memicu kewajiban pelaporan nilai kritis.

Sesudah rollback, ketiga tabel Laboratorium yang tersentuh probe kembali berisi **nol baris**.
Tidak ada satu pun baris bisnis yang ditinggalkan.

**Keadaan akhir database:** termigrasi. `dotnet ef migrations list` menampilkan
`20260902085636_AddLabValueBoundApprovalAndHistory` tanpa penanda `(Pending)`, sementara keempat
migration milik modul lain tetap `(Pending)`.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-33` — perubahan batas normal oleh kepala instalasi langsung berlaku, sedangkan perubahan batas kritis tertahan sebagai pengajuan sampai disetujui pihak klinis | **Terpenuhi untuk cakupan task ini** | `PengajuanPerubahanBatasKritis_TertahanSementaraBatasLamaTidakBerubah` membuktikan pengajuan lahir `Submitted` dengan pemutus kosong, dan batas kritis yang berlaku tidak bergerak. Dibuktikan ulang terhadap database sungguhan pada bagian 5.1. Bagian "langsung berlaku" dan penegakan `VAL-28` terjadi pada endpoint pengubahan, yang merupakan cakupan `BE-LAB-04` |
| `AC-34` — setiap perubahan batas nilai menyimpan kolom yang berubah, nilai lama, nilai baru, pelaku, penyetuju, waktu, dan alasan | **Terpenuhi** | Ketujuh hal itu diperiksa satu per satu oleh `PerubahanBatasKritisYangDisetujui_MenerbitkanRiwayatBesertaPenyetujunya`, dan pasangannya `PerubahanBatasNormal_MenerbitkanSatuBarisRiwayatTanpaPenyetuju` membuktikan penyetuju memang kosong ketika perubahan tidak menempuh persetujuan. Riwayatnya juga terbukti permanen lewat `MengubahBarisRiwayatYangSudahTersimpan_Ditolak` |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kedua entity ada dengan nama benar | **Terpenuhi** | `LabValueBoundChangeRequest` dan `LabValueBoundHistory`, keduanya berawalan `Lab` sesuai registry. `KeduaEntityBaru_TidakMemakaiAwalanTrx` memeriksa nama class **dan** nama tabelnya — risiko `QBE-NAM-001` yang disebut roadmap tidak terwujud. Nama tabel dibaca ulang dari database sesudah `Up`, bukan hanya dari source |
| Riwayat memuat kolom, nilai lama, nilai baru, pelaku, waktu, dan alasan | **Terpenuhi** | Keenam kolom itu ada, ditambah `ApprovedByUserId` sebagai kolom ketujuh yang dituntut `AC-34`. Diperiksa `LabValueBoundHistory_TerpetakanSesuaiKamusDataDanBerperilakuTolakUbah` beserta pengujian penyimpanannya |
| `AC-34` terbukti | **Terpenuhi** | Tiga kasus uji riwayat, dan satu baris riwayat lengkap yang tersimpan pada database sungguhan di bagian 5.1 |

**Seluruh butir DoD terpenuhi.** Dua hal berikut disebut apa adanya karena keduanya batas
cakupan, bukan pekerjaan yang tertinggal:

1. **`AC-33` baru separuh secara keseluruhan.** Bagian "batas kritis tertahan" tuntas di sini.
   Bagian "batas normal langsung berlaku" beserta penegakan `VAL-28` adalah cakupan `BE-LAB-04`.
2. **Larangan menyetujui pengajuan sendiri (`VAL-33`) belum ditegakkan.** Task ini menyediakan
   dua kolom pelaku yang terpisah supaya aturan itu dapat diperiksa; penulisan aturannya di dalam
   service adalah cakupan `BE-LAB-05`, dan roadmap sudah menandainya berisiko **Tinggi**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build backend menghasilkan 186 warning, seluruhnya CS1573/CS1574/CS1587 tentang komentar XML pada modul lain. Tidak satu pun berasal dari berkas yang diubah task ini, dan jumlahnya tidak bertambah dari baseline |
| Masalah yang diketahui | Di luar Laboratorium, satu test Billing tetap merah sejak sebelum task ini — temuan `FINAL`/`CLOSED` yang sudah diajukan lewat `approval-requests/2026-09-02-temuan-billing-final-closed.md`. Dua temuan `BE-LAB-02` yang menunggu keputusan pemilik — celah `NULL` pada index unik `VAL-21` dan `LabValueBound.SortOrder` — masih terbuka dan tidak tersentuh task ini |
| Risiko tersisa | **Pertama**, `QuilvianNewDevYoga` sudah dimigrasi tetapi **database lain belum**; menjalankan kode ini terhadap database yang belum menerima migration akan gagal pada setiap query kedua tabel ini. **Kedua**, ketiga tabel batas nilai masih kosong dan belum punya jalur pengisian sampai `BE-LAB-04` dan `BE-LAB-05` selesai. **Ketiga**, `VAL-33` — larangan menyetujui pengajuan sendiri — adalah invariant keselamatan yang **belum** ditegakkan di mana pun; sampai `BE-LAB-05` menuliskannya di service, tabel ini tidak memberi perlindungan apa pun terhadapnya. **Keempat**, `QuilvianNewDevYoga` masih punya empat migration tertunda dengan riwayat tidak berurutan, sehingga `dotnet ef database update` polos tetap berbahaya |
| Perubahan sampingan | `NONE`. Snapshot model bertambah 373 baris kumulatif tanpa satu pun baris terhapus. `Migrations/scripts/` yang muncul saat penelusuran diperiksa dan ternyata berkas lama yang sudah ter-commit sejak `6be893f`, bukan hasil pekerjaan ini |
| Interupsi | `NONE`. Satu pemeriksaan urutan migration sempat memakai assembly lama karena dijalankan dengan `--no-build` sesudah migration dibuat; kekeliruan itu terdeteksi, assembly dibangun ulang, dan pemeriksaannya diulang sebelum satu pun perintah database dijalankan |
| Selisih snapshot source | Roadmap menyebut Backend SHA `c87d9c0`; pekerjaan ini berjalan di atas `d8d67c3`. Permukaan yang disentuh diperiksa langsung terhadap source saat ini dan cocok: `LabValueBound` ada dengan bentuk yang dibuat `BE-LAB-02`, `TrxLabTransitionHistory` masih menjadi pola `CAP-04` yang berlaku, dan `LabOrder.Version` masih menjadi pola `CAP-17`. Karena itu selisih SHA tidak menahan task ini |
| Status Git | Lihat di bawah |
| Langkah berikutnya | **1.** `BE-LAB-04` kini tidak lagi tertahan — kedua penahannya, `BE-LAB-02` dan `BE-LAB-03`, sudah selesai. **2.** `BE-LAB-05` secara teknis dapat dibangun, tetapi **tidak dapat dinyatakan siap pakai** sebelum manajemen rumah sakit menetapkan siapa pemegang `LabCriticalBound : Approve` — lihat `04-prd-to-mvp.md` bagian 15. **3.** Dua temuan `BE-LAB-02` masih menunggu keputusan pemilik. **4.** Terapkan kedua migration Laboratorium ke database lain yang membutuhkannya, lewat wewenang tersendiri. **5.** Bereskan empat migration tertunda pada `QuilvianNewDevYoga` bersama pemilik modul masing-masing |

**Keluaran `git status --short` di akhir pekerjaan:**

```text
 M Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
 M docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md
 M docs/module-blueprints/laboratorium/roadmap/traceability.md
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundChangeRequest.cs
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundHistory.cs
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueOption.cs
?? Migrations/20260902082722_AddLabValueBoundAndOption.Designer.cs
?? Migrations/20260902082722_AddLabValueBoundAndOption.cs
?? Migrations/20260902085636_AddLabValueBoundApprovalAndHistory.Designer.cs
?? Migrations/20260902085636_AddLabValueBoundApprovalAndHistory.cs
?? QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundApprovalTests.cs
?? QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundTests.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundChangeRequestConfiguration.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundConfiguration.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundHistoryConfiguration.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueOptionConfiguration.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-02.md
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-03.md
```

Berkas milik `BE-LAB-02` masih tampak sebagai perubahan yang belum di-commit karena tidak ada
satu pun operasi Git yang dijalankan pada sesi ini. Tidak ada `git add`, `commit`, `push`,
`merge`, maupun `rebase`.

**Pembaruan register yang ikut ditulis.**

| Berkas | Perubahan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Blok status `SELESAI` beserta tautan laporan pada bagian 3; baris `BE-LAB-03` pada tabel bagian 7; penahan `BE-LAB-04` dicabut |
| `roadmap/traceability.md` | Baris `FR-03.4` dan `FR-03.5` diperbarui buktinya |

Tidak ada artefak blueprint lain yang disentuh: kontrak, kamus data, ERD, matriks uji, dan
dokumen arsitektur tidak berubah satu baris pun.

---

## 8. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement` |
| Submodule | — (tidak ada) |
| Pemilik/prefix pada registry | `LaboratoryManagement / Laboratory`, prefix `Lab`, Category `BUSINESS DOMAIN / MODULE` |
| Status registry | `ACTIVE` sejak 2026-09-02. Wewenangnya mencakup source **dan pembuatan migration**; eksekusi database di luar dev pemilik dan deployment tetap wewenang terpisah |
| Keberlakuan | `NEW CODE` |
| Sumber tata kelola | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` seluruhnya terbaca. `QBE_EXCEPTIONS.json` berisi nol pengecualian, sehingga tidak ada temuan yang disupresi |

### QBE ID yang berlaku

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| QBE-ENT-001 | Kedua entity mewarisi `IdentityModel` |
| QBE-ENT-002 | `Guid` untuk kunci dan foreign key; nullability mengikuti semantik domain — `DecidedByUserId` dan `DecidedAt` boleh kosong karena pengajuan yang belum diputuskan memang belum punya pemutus, dan `ApprovedByUserId` boleh kosong karena perubahan batas normal memang tidak menempuh persetujuan |
| QBE-ENT-003 | Tidak ada kolom presentasi yang dipersistensi pada kedua entity ini. `SortOrder` sengaja tidak ada — riwayat diurutkan `OccurredAt`, dan pengajuan diurutkan waktu pengajuannya |
| QBE-NAM-001 | Tidak ada nama `Trx*` yang dibuat. Ini risiko yang disebut roadmap secara khusus, dan diuji tersendiri |
| QBE-NAM-002 | Kedua entity memakai prefix registry yang disetujui, `Lab` |
| QBE-NAM-004 | Prefix diambil dari keputusan registry, bukan disimpulkan dari nama folder atau nama task |
| QBE-CFG-001 | Kedua entity punya `IEntityTypeConfiguration<T>` tersendiri beserta mapping, key, index, dan relasinya |
| QBE-MOD-001 | Kedua entity ditempatkan di bawah Area/Module pemiliknya |
| QBE-MOD-002 | Entri registry sudah `ACTIVE` sebelum berkas model pertama dibuat |
| QBE-MOD-003 | Kedua folder sudah terdaftar dan sudah memuat model persisted sebelumnya |
| QBE-ENUM-001 | `LabBoundChangeStatus` dimiliki modul Laboratorium, diletakkan bersama enum modul lainnya |
| QBE-DEL-001 | Soft delete `IdentityModel` dihormati; kolom audit sengaja tidak ikut dikunci tolak-ubah supaya perilakunya tidak berubah |
| QBE-AUD-001 | Kolom audit datang dari `IdentityModel` dan terpisah dari application logging. Riwayat bisnis pada `LabValueBoundHistory` adalah fakta domain, bukan application log, dan karena itu berdiri sebagai tabelnya sendiri |

### QBE ID yang tidak berlaku

| QBE ID | Alasan |
| --- | --- |
| QBE-NAM-003, QBE-DB-001, QBE-DB-002 | Khusus `LEGACY MIGRATION`. Task ini murni `NEW CODE`; tidak ada rename tabel fisik |
| QBE-SVC-001, QBE-API-001, QBE-DTO-001, QBE-PAGE-001, QBE-OPT-001, QBE-PERM-001 | Task ini tidak menghasilkan controller, service, DTO, endpoint list, options, maupun permission. Seluruhnya cakupan `BE-LAB-04` dan `BE-LAB-05` |
| QBE-CODE-001 sampai QBE-CODE-006 | Tidak ada nomor bisnis yang dialokasikan pada kedua entity ini |
| QBE-VAL-001 | Validasi request adalah pekerjaan endpoint. Yang ditegakkan task ini adalah invarian di lapisan penyimpanan |
| QBE-LOG-001 | Tidak ada perpindahan state yang dijalankan task ini; yang dibuat adalah tempat merekamnya |
| QBE-TXN-001 | Tidak ada workflow multi-record yang ditulis task ini. Transaksi yang menyatukan pembaruan batas kritis dengan penerbitan riwayat adalah pekerjaan `BE-LAB-05` |
| QBE-CFG-002 | Tidak ada configuration legacy yang disentuh |
