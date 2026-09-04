# Laporan Perubahan Backend — `BE-LAB-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-01` |
| Judul | Kolom disiplin pada pesanan laboratorium |
| Slice | `S15` — monitoring per disiplin (`roadmap/backend-roadmap.md` bagian 7) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-0` |
| Trace | `FR-10.3`; `LAB-DEC-025`; `INV-21`, `INV-22` (`03-domain-architecture.md`); `erd/data-dictionary.md` bagian `LabOrder` |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02 |
| Dependency | — (tidak ada). `BE-LAB-15` dan `BE-LAB-07` yang justru bergantung pada task ini |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 2, database 2, keamanan 0, UI/workflow 0. Duduk di batas atas `MEDIUM` karena dua faktor bernilai 2 berada di ranah kontrak dan schema |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs`, `Migrations/`, `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/`, dan `docs/module-blueprints/laboratorium/task/report/backend/` beserta pembaruan bukti pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `2d1e88b` — *updates BE modul lab*, 2026-09-02. Roadmap menyebut snapshot `c87d9c0`; selisihnya diperiksa dan tidak menyentuh permukaan task ini (lihat bagian 7) |
| Tanggal | 2026-09-02 |
| Status | **Selesai.** Source, test, pembuatan migration, dan eksekusi migration ke database dev pemilik seluruhnya tuntas dan terverifikasi. Seluruh butir DoD terpenuhi |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, sebuah pesanan laboratorium tidak menyimpan **disiplin** apa pun.

Rumah sakit ini menjalankan tiga disiplin laboratorium yang berjalan sejajar: Patologi Klinik,
Patologi Anatomi, dan Mikrobiologi. Masing-masing punya daftar pasien, petugas, dan alur hasil
sendiri. Keputusan `LAB-DEC-025` menetapkan ketiganya masuk ke dalam scope modul, sementara Bank
Darah tetap di luar.

Akibat nyatanya bagi pengguna: kepala instalasi tidak dapat membuka daftar pantau "Mikrobiologi"
dan melihat hanya pesanan mikrobiologi. Seluruh pesanan tercampur menjadi satu daftar, karena
tidak ada satu pun ruas data yang membedakan ketiganya.

> **Contoh.** Satu pasien menjalani Hemoglobin (Patologi Klinik), kultur darah (Mikrobiologi),
> dan biopsi kulit (Patologi Anatomi) pada hari yang sama. Sebelum perubahan ini, ketiga pesanan
> itu terlihat identik bagi sistem. Petugas mikrobiologi harus membaca satu per satu nama
> pemeriksaannya untuk tahu mana yang menjadi pekerjaannya.

Task ini menyediakan **fondasi datanya**: pesanan kini menyimpan disiplinnya sendiri, dan
disiplin itu terkunci sejak pesanan dibuat. Layar daftar pantau per disiplin adalah pekerjaan
`BE-LAB-15`, yang memang menjadikan task ini sebagai dependency-nya.

---

## 2. Proses bisnis

**Tujuan.** Setiap pesanan laboratorium membawa satu disiplin yang tidak berubah seumur hidup
pesanan itu.

**Pelaku.** Dokter atau petugas yang memesan pemeriksaan laboratorium untuk satu kunjungan
pasien.

**Pemicu.** Dokter memutuskan pasien perlu pemeriksaan laboratorium.

**Langkah yang berurutan:**

1. Petugas membuka pemesanan laboratorium untuk satu kunjungan pasien — Rawat Jalan, Rawat Inap,
   atau IGD. Ketiganya melewati jalur yang sama persis.
2. Petugas memilih pemeriksaan yang dipesan dan **memilih disiplinnya**: Patologi Klinik,
   Patologi Anatomi, atau Mikrobiologi.
3. Sistem memeriksa kunjungan memang ada, pemeriksaan memang pemeriksaan laboratorium yang
   aktif, dan disiplin yang dikirim memang salah satu dari ketiganya.
4. Pesanan tersimpan berstatus `Requested` beserta disiplinnya, dan satu baris riwayat
   `Order.Request` diterbitkan dengan nama pelakunya.
5. Sejak titik ini, disiplin pesanan **tidak dapat berpindah**. Tidak ada endpoint yang
   mengubahnya, dan lapisan penyimpanan menolak setiap upaya mengubahnya seandainya kelak ada
   yang menulis jalur baru.

**Aturan yang berlaku:**

| Aturan | Isi |
| --- | --- |
| `INV-21` | Sebuah pesanan wajib memiliki tepat satu disiplin, dan disiplin itu tidak berubah setelah pesanan dibuat |
| `LAB-DEC-025` | Hanya tiga disiplin yang dikenal. Bank Darah tetap di luar scope |

**Status yang dihasilkan.** Tidak ada status baru. Pesanan tetap lahir berstatus `Requested`
seperti sebelumnya; task ini tidak menyentuh satu pun perpindahan status.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Petugas mengirim angka disiplin di luar ketiganya, misalnya `99` | Ditolak `400` dengan pesan "Disiplin laboratorium tidak dikenal." Tidak ada baris yang tersimpan |
| Pemanggil lama tidak mengirim disiplin sama sekali | **Tetap dilayani.** Pesanan terbentuk tanpa disiplin. Ini disengaja — lihat bagian 3.3 |
| Ada yang mencoba mengubah disiplin pesanan yang sudah tersimpan | Penyimpanan menolak dengan `InvalidOperationException`; nilai lama tidak berubah |
| Pesanan yang sudah ada sebelum kolom ini dibuat | Disiplinnya kosong. Kolom sengaja boleh kosong supaya penambahan kolom tidak merusak data lama |

**Hasil akhir.** Setiap pesanan baru yang membawa disiplin dapat disaring per disiplin, dan
disiplinnya dapat dipercaya karena tidak pernah berubah setelah tercatat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:**

- `AGENTS.md`; `CLAUDE.md`
- `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `docs/engineering/QBE_EXCEPTIONS.json`
- `rules/GLOBAL_RULES.md`; `rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`; `rules/backend/API_RULES.md`; `rules/backend/DATABASE_RULES.md`; `rules/backend/REVIEW_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`; `rules/rule-output/lokasi-laporan-task.md`; `rules/rule-output/bentuk-blueprint.md`

**Blueprint:**

- `roadmap/backend-roadmap.md`; `roadmap/traceability.md`
- `contracts/api-contract.md`; `contracts/validation-matrix.md`; `contracts/permission-audit-matrix.md`
- `00-interview-decisions.md` (BR-21, `LAB-DEC-025`, daftar acceptance criteria)
- `02-backend-architecture.md` bagian 1 dan 4.1; `03-domain-architecture.md` (`INV-21`, `INV-22`, A2.11)
- `erd/data-dictionary.md`; `erd/laboratory-operations.md`; `testing/acceptance-test-matrix.md` bagian 1b dan 7e

**Source:**

- `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`; `.../Models/TrxLabTransitionHistory.cs`
- `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs`
- `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs`
- `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs`; `.../Services/LabSpecimenService.cs`
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`
- `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs`; `.../LaboratoryManagement/TrxLabSpecimenConfiguration.cs`; `Repositories/ApplicationDbContext.cs`
- `Areas/HealthServices/MasterData/Models/MstProcedure.cs`; `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`; `Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs`
- `tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratorySpecimenLifecycleTests.cs`; `QuilvianSystemBackend.Tests/HealthServices/OperatingRoomManagement/OperatingRoomModelConfigurationTests.cs`; `QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/PatientEncounterTestWorld.cs` — sebagai pola test

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | Menambah enum `LabDiscipline` berisi tepat tiga nilai: `ClinicalPathology = 1`, `AnatomicalPathology = 2`, `Microbiology = 3`. Bank Darah sengaja tidak ada. Nilai lama pada enum lain tidak disentuh |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Menambah properti `LabDiscipline? Discipline` beserta penjelasan mengapa boleh kosong dan di mana `INV-21` ditegakkan |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Memetakan `Discipline` sebagai enum `int`, memasang `PropertySaveBehavior.Throw` sesudah simpan (penegak `INV-21`), dan menambah index `IX_LabOrder_Discipline` untuk penyaringan per disiplin |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `LabOrderDetailResponse` bertambah ruas `Discipline` bertipe `string?` sesuai `LAB-API-v1` r3. `CreateLabOrderRequest` bertambah ruas `Discipline` yang **tidak wajib** — lihat bagian 3.3 |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `CreateAsync` memvalidasi nilai disiplin dan menyimpannya; `GetDetailAsync` dan `MapDetailResponse` menampilkannya; log `LabOrder.Create` ikut mencatat disiplinnya |
| `Migrations/20260902042242_AddLabOrderDiscipline.cs` | **Baru.** `Up` menambah kolom `Discipline` bertipe `integer` boleh kosong pada `public."LabOrder"` dan membuat index `IX_LabOrder_Discipline`. `Down` membuang keduanya |
| `Migrations/20260902042242_AddLabOrderDiscipline.Designer.cs` | **Baru.** Berkas hasil generate yang menyertai migration |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bertambah 5 baris: satu properti `Discipline` dan satu index. Tidak ada operasi lain yang ikut terbawa |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabOrderDisciplineTests.cs` | **Baru.** Sembilan kasus uji yang membuktikan seluruh acceptance criteria task ini |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingSettlementServiceTests.cs` | **Di luar scope `BE-LAB-01`**, dikerjakan atas instruksi eksplisit pemilik repository pada sesi yang sama. `CreateService` melengkapi argumen `BillingFinalizationService` yang hilang sehingga project test kembali dapat di-build. Lihat bagian 7 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `LabOrderDetailResponse` bertambah ruas `discipline` — **persis** yang dikunci `LAB-API-v1` r3. Selain itu, `CreateLabOrderRequest` bertambah ruas `discipline` yang tidak wajib; ini **selisih terhadap kontrak** dan dijelaskan di bawah. Tidak ada endpoint yang ditambah, dihapus, diganti nama, atau berubah kode statusnya. `LabOrderListResponse` sengaja **tidak** disentuh, karena kontrak tidak menambahkan ruas apa pun padanya |
| Database | Satu kolom baru `public."LabOrder"."Discipline"` bertipe `integer` boleh kosong, ditambah index `IX_LabOrder_Discipline`. Migration **sudah dibuat dan sudah diterapkan** ke `QuilvianNewDevYoga`, database dev pemilik, atas wewenang eksplisit pada sesi ini. Jalur `Down` ikut dibuktikan. Lihat bagian 5.1 |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada permission baru; endpoint yang tersentuh tetap memakai `LabOrder : Create` dan `LabOrder : Read` yang sudah terdaftar. Tidak ada data sensitif baru yang disimpan |

**Selisih terhadap kontrak yang perlu keputusan pemilik.**

`LAB-API-v1` r3 menuliskan satu kalimat: *"Delapan endpoint pesanan yang sudah ada tetap berlaku
apa adanya. `LabOrderDetailResponse` bertambah satu ruas: `discipline`."* Kontrak itu **tidak
menyebutkan** dari mana nilai disiplin datang pada saat pesanan dibuat.

Sementara itu roadmap `BE-LAB-01` mensyaratkan verifikasi *"buat pesanan berdisiplin
Mikrobiologi"*, dan `03-domain-architecture.md` bagian A2.11 menyatakan `LabOrder.Discipline`
**diisi petugas** — karena `MstProcedure` belum punya penanda disiplin sampai `BE-EXT-01`
dikerjakan pemilik `master-data`. Tanpa jalur masukan, kolom ini akan selalu kosong dan task
kehilangan seluruh manfaatnya.

Yang diambil: ruas `discipline` ditambahkan pada `CreateLabOrderRequest` sebagai ruas **tidak
wajib**. Konsekuensinya:

| Pilihan | Akibat |
| --- | --- |
| Tidak wajib (**yang dipakai**) | Pemanggil lama yang belum mengirim disiplin tetap dilayani persis seperti sebelumnya, sehingga janji "delapan endpoint tetap berlaku apa adanya" tidak dilanggar. Harganya: `INV-21` bagian "wajib memiliki tepat satu disiplin" belum ditegakkan penuh |
| Wajib | `INV-21` tegak penuh, tetapi setiap pemanggil lama langsung ditolak `400`. Itu perubahan yang merusak dan memerlukan revisi kontrak `LAB-API-v1` r4 yang disetujui pemilik modul |

**Rekomendasi:** naikkan ruas ini menjadi wajib lewat revisi kontrak tersendiri, bersamaan
dengan `BE-LAB-07`/`BE-EXT-01` yang membuat disiplin dapat diturunkan otomatis dari katalog
pemeriksaan. Sampai itu terjadi, kekosongan disiplin adalah keadaan yang sah dan tercatat.

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Dua endpoint yang sudah ada berubah muatannya, tanpa berubah alamat,
metode, kode status, maupun hak aksesnya.

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pesanan laboratorium. Kini dapat menerima `discipline` — Patologi Klinik, Patologi Anatomi, atau Mikrobiologi. Respons detailnya memuat `discipline` | `LabOrder : Create` |
| `GET` | `/{id}` | Melihat detail satu pesanan. Respons kini memuat `discipline`, atau kosong untuk pesanan yang dibuat sebelum kolom ini ada | `LabOrder : Read` |

Nilai `discipline` pada respons berupa teks: `ClinicalPathology`, `AnatomicalPathology`, atau
`Microbiology` — mengikuti pola `orderStatus` dan `statusBeforeHold` yang sudah berlaku pada
keluarga endpoint ini.

Enam endpoint pesanan lainnya — `GET /`, `PUT /{id}/start-process`, `PUT /{id}/complete`,
`PUT /{id}/hold`, `PUT /{id}/resume`, dan `PUT /{id}/cancel` — tidak berubah sama sekali.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `186 Warning(s)` — seluruh warning adalah CS1573/CS1574/CS1587 komentar XML yang sudah ada sebelum task ini dan tidak satu pun berasal dari berkas yang diubah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lolos | `PASS` | `Files evaluated: 9`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` |
| `dotnet ef migrations has-pending-model-changes` sesudah migration dibuat | Tidak ada selisih tersisa | `PASS` | `No changes have been made to the model since the last migration.` |
| Membuat pesanan berdisiplin Mikrobiologi; `discipline` terisi pada respons detail | Sesuai harapan | `PASS` | `MembuatPesananMikrobiologi_MengisiDisiplinPadaResponsDetail` |
| Membuat pesanan dari kunjungan Rawat Jalan, Rawat Inap, dan IGD | Ketiganya lulus lewat jalur yang sama | `PASS` | `MembuatPesananDariTigaJenisKunjungan_BerjalanSamaDanMengisiDisiplin` — tiga kasus `[InlineData]` |
| **Gagal** — mengubah disiplin sesudah pesanan tersimpan | Ditolak, nilai lama tidak berubah | `PASS` | `MengubahDisiplinSetelahPesananDibuat_Ditolak` |
| **Gagal** — mengirim angka disiplin di luar ketiganya | Ditolak `ArgumentException` → `400`, nol baris tersimpan | `PASS` | `MembuatPesananDenganDisiplinTidakDikenal_Ditolak` |
| Pemanggil lama tanpa `discipline` tetap dilayani | Pesanan terbentuk, `discipline` kosong | `PASS` | `MembuatPesananTanpaDisiplin_TetapBerhasilDanDisiplinKosong` |
| Pemetaan model: enum `int`, boleh kosong, ber-index, tolak-ubah | Sesuai harapan | `PASS` | `Discipline_TerpetakanSebagaiEnumIntBerIndexDanTolakUbah` |
| Enum memuat tepat tiga disiplin tanpa Bank Darah | Sesuai harapan | `PASS` | `LabDiscipline_MemuatTepatTigaDisiplinTanpaBankDarah` |
| Seluruh test `LabOrderDisciplineTests`, dijalankan di project semestinya | Hijau | `PASS` | `dotnet test --filter FullyQualifiedName~LabOrderDisciplineTests` → `Failed: 0, Passed: 9, Skipped: 0, Total: 9` pada `QuilvianSystemBackend.Tests.dll` |
| Build project `QuilvianSystemBackend.Tests` | Berhasil sesudah diperbaiki | `PASS` | `0 Error(s)`, `15 Warning(s)`. Sebelum diperbaiki: `BillingSettlementServiceTests.cs(727,20): error CS7036`. Lihat bagian 7 |
| Seluruh suite `QuilvianSystemBackend.Tests` | 852 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | `Failed: 1, Passed: 852, Total: 853, Duration: 33 s`. Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, milik modul Billing dan tidak berkaitan dengan Laboratorium. Lihat bagian 7 |
| Project test tidak menyentuh database mana pun | Terbukti | `PASS` | Penelusuran `QuilvianSystemBackend.Tests`: 15 pemakaian `UseInMemoryDatabase`, nol `UseNpgsql`, nol connection string, nol `Database.Migrate`, nol `EnsureCreated` |
| Eksekusi migration `Up` ke `QuilvianNewDevYoga` | Berhasil | `PASS` | Lihat bagian 5.1 |
| Eksekusi migration `Down` ke `QuilvianNewDevYoga` | Berhasil, database kembali persis ke keadaan semula | `PASS` | Lihat bagian 5.1 |
| Eksekusi `Up` ulang sesudah `Down` | Berhasil | `PASS` | Lihat bagian 5.1 |
| Empat migration tertunda milik modul lain tetap tidak tersentuh | Terbukti | `PASS` | `dotnet ef migrations list` sesudah eksekusi: `AddRadiologyManagement`, `AddCompanyGuarantorToPatientEncounterGuarantor`, `RenameClinicalMilestoneFactToCliPrefix`, dan `RepairCanonicalModelSnapshotBaseline` seluruhnya masih `(Pending)` |
| Uji lewat HTTP sungguhan beserta `[Authorize]` dan `[AccessPermission]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Hak akses yang dipakai memang tidak berubah, sehingga risikonya rendah |

Uji manual lewat antarmuka: `NOT FEASIBLE` — memerlukan aplikasi berjalan; tidak diminta task ini.

### 5.1 Bukti eksekusi migration

**Wewenang.** Pemilik repository menunjuk `QuilvianNewDevYoga` sebagai targetnya dan memilih
"hanya migration Laboratorium saja" pada sesi ini.

**Gerbang yang ditemukan sebelum eksekusi.** `dotnet ef migrations list` menunjukkan **lima**
migration tertunda, bukan satu — dan riwayatnya tidak berurutan: `RepairPostCanonicalIntegration`
(30 Agustus) dan `AddBilTenderKwitansiNumber` (1 September) sudah diterapkan, sementara
`AddRadiologyManagement` (28 Agustus) belum. `dotnet ef database update` akan menerapkan
kelima-limanya, termasuk pembuatan tabel modul Radiology dan **rename tabel** Clinical Milestone
Fact — jauh melampaui wewenang yang diberikan, dan besar kemungkinan gagal di tengah karena
migration yang lebih baru sudah lebih dulu diterapkan.

Karena itu yang dijalankan hanya DDL milik migration ini, dalam satu transaksi:

```sql
ALTER TABLE public."LabOrder" ADD "Discipline" integer;
CREATE INDEX "IX_LabOrder_Discipline" ON public."LabOrder" ("Discipline");
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260902042242_AddLabOrderDiscipline', '9.0.18');
```

`psql` tidak terpasang pada mesin ini, sehingga eksekusinya lewat runner Npgsql sementara di luar
repository yang membaca connection string dari `appsettings.Development.json` saat berjalan.
Runner itu menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`, dan tidak
menulis credential ke berkas mana pun.

**Urutan bukti yang direkam:**

| Langkah | Kolom `Discipline` | Index `IX_LabOrder_Discipline` | Baris `__EFMigrationsHistory` |
| --- | --- | --- | --- |
| Sebelum apa pun dijalankan | TIDAK ADA | TIDAK ADA | TIDAK ADA |
| Sesudah `Up` | `integer, nullable=YES` | `CREATE INDEX ... USING btree ("Discipline")` | ADA |
| Sesudah `Down` | TIDAK ADA | TIDAK ADA | TIDAK ADA |
| Sesudah `Up` ulang | `integer, nullable=YES` | `CREATE INDEX ... USING btree ("Discipline")` | ADA |

Keadaan akhir database: **termigrasi**. `dotnet ef migrations list` kini menampilkan
`20260902042242_AddLabOrderDiscipline` tanpa penanda `(Pending)`, sementara keempat migration
milik modul lain tetap `(Pending)`.

**Catatan keadaan data.** `public."LabOrder"` berisi **nol baris** pada database ini, sehingga
risiko "menambah kolom pada tabel berisi data" yang dicatat roadmap tidak terwujud di sini. Pada
database yang sudah berisi pesanan, kolom `nullable` tanpa nilai bawaan tetap merupakan operasi
metadata pada PostgreSQL dan tidak menulis ulang baris yang ada.

**Tidak dijalankan:**

- **Empat migration tertunda milik modul lain.** Bukan wewenang task ini, dan pemilik repository
  secara eksplisit memilih agar keempatnya tidak disentuh.
- **Eksekusi ke database selain `QuilvianNewDevYoga`.** Tidak ada database lain yang disentuh,
  termasuk `QuilvianNewDevTim01`.
- **Test index fisik dan foreign key lewat test otomatis.** Provider InMemory tidak membuat index maupun foreign key
  sungguhan. Yang dibuktikan test adalah index terdaftar pada model EF; keberadaan fisiknya
  dibuktikan terpisah lewat eksekusi migration pada bagian 5.1, yang membacanya langsung dari
  `pg_indexes`.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-11` — pesanan lab dapat dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD dengan alur kerja yang sama | **Terpenuhi** | `MembuatPesananDariTigaJenisKunjungan_BerjalanSamaDanMengisiDisiplin`, tiga kasus. Ketiganya menghasilkan status awal `Requested`, disiplin terisi, dan tepat satu baris riwayat `Order.Request`. Tidak ada cabang khusus per jenis kunjungan. Menjawab baris `AC-11` keempat pada `testing/acceptance-test-matrix.md` bagian 1b: kolom baru terisi dan tidak memaksa cabang baru |
| `AC-41` — pesanan menyimpan disiplinnya, **dan** daftar pantau dapat disaring per disiplin | **Terpenuhi sebagian** | Bagian pertama terpenuhi: `MembuatPesananMikrobiologi_MengisiDisiplinPadaResponsDetail` membuktikan pesanan menyimpan disiplinnya dan menampilkannya kembali. Bagian kedua — tiga daftar pantau — **bukan** cakupan task ini; itu `BE-LAB-15`, yang memang menjadikan `BE-LAB-01` sebagai dependency-nya |
| Disiplin tidak dapat diubah setelah pesanan dibuat (`INV-21`) | **Terpenuhi** | `MengubahDisiplinSetelahPesananDibuat_Ditolak`. Penegakannya struktural: tidak ada endpoint yang mengubah disiplin, **dan** lapisan penyimpanan menolaknya lewat `PropertySaveBehavior.Throw` |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kolom ada | **Terpenuhi** | `LabOrder.Discipline`; snapshot model memuat properti dan indexnya |
| Migration jalan maju dan mundur | **Terpenuhi** | Dibuktikan terhadap `QuilvianNewDevYoga`: `Up` → kolom dan index ada; `Down` → keduanya hilang dan database kembali persis ke keadaan semula; `Up` ulang → keduanya ada lagi. Rinciannya pada bagian 5.1 |
| DTO respons memuat `discipline` | **Terpenuhi** | `LabOrderDetailResponse.Discipline`; dibuktikan pada respons `CreateAsync` maupun `GetDetailAsync` |
| Uji integrasi hijau | **Terpenuhi** | `Failed: 0, Passed: 9` |
| Tidak ada endpoint lain yang berubah perilakunya | **Terpenuhi** | Enam endpoint pesanan lainnya tidak tersentuh diff. `LabOrderListResponse` tidak berubah, sehingga `GET /` menghasilkan muatan yang sama persis. `POST /` tetap melayani pemanggil yang tidak mengirim `discipline` |

**Seluruh butir DoD terpenuhi.** Dua hal berikut tetap disebut apa adanya karena keduanya
**bukan** butir DoD `BE-LAB-01`, melainkan batas cakupannya:

1. **`AC-41` baru separuh.** Bagian daftar pantau memang milik `BE-LAB-15`.
2. **`INV-21` bagian "wajib memiliki tepat satu disiplin" belum tegak penuh**, karena ruas
   `discipline` pada permintaan pembuatan sengaja dibuat tidak wajib demi menjaga kontrak
   `LAB-API-v1` r3. Lihat bagian 3.3.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build backend menghasilkan 186 warning, seluruhnya CS1573/CS1574/CS1587 tentang komentar XML pada modul Inpatient, Medical Record, Pharmacy, dan Registration. Tidak satu pun berasal dari berkas yang diubah task ini, dan jumlahnya tidak bertambah |
| Masalah yang diketahui | Dua hal, keduanya milik modul Billing dan keduanya terbuka **sebagai akibat** perbaikan build. Rinciannya pada bagian 9 |
| Risiko tersisa | **Pertama**, `QuilvianNewDevYoga` sudah dimigrasi, tetapi **database lain belum** — menjalankan kode ini terhadap database yang belum menerima migration akan gagal pada setiap query `LabOrder`. Berkas migrationnya tersedia, dan penerapannya ke database lain adalah wewenang tersendiri. **Kedua**, disiplin masih boleh kosong, sehingga daftar pantau `BE-LAB-15` kelak perlu memutuskan bagaimana menampilkan pesanan tanpa disiplin. **Ketiga**, `INV-22` — kesesuaian jenis pemeriksaan dengan disiplin pesanan — belum dapat ditegakkan sampai `BE-EXT-01` menambahkan penanda disiplin pada `MstProcedure`; ini sudah tercatat pada roadmap sebagai dependency `BE-LAB-07`, bukan temuan baru. **Keempat**, di luar Laboratorium: `QuilvianNewDevYoga` punya empat migration tertunda dengan riwayat tidak berurutan. Selama itu dibiarkan, `dotnet ef database update` polos pada database ini berbahaya bagi siapa pun yang menjalankannya |
| Perubahan sampingan | `NONE` dalam arti tidak ada perubahan tak sengaja. Diff `Migrations/ApplicationDbContextModelSnapshot.cs` diperiksa baris per baris: tepat 5 baris tambahan, seluruhnya tentang `Discipline`. Satu berkas **di luar scope** memang diubah secara sadar atas instruksi eksplisit — `BillingSettlementServiceTests.cs`, lihat bagian 9.1 — dan disebut apa adanya, bukan disamarkan sebagai bagian dari `BE-LAB-01` |
| Interupsi | `NONE` |
| Selisih snapshot source | Roadmap menyebut Backend SHA `c87d9c0`; pekerjaan ini berjalan di atas `2d1e88b`. Permukaan yang disentuh diperiksa langsung terhadap source saat ini dan **cocok** dengan kontrak as-is: delapan endpoint `lab-orders` masih utuh, dan `LabOrderDetailResponse` memang belum punya ruas `discipline`. Karena itu selisih SHA tidak menahan task ini |
| Status Git | Lihat di bawah |
| Langkah berikutnya | **1.** Putuskan apakah `discipline` dinaikkan menjadi ruas wajib lewat revisi `LAB-API-v1` r4, sebaiknya bersamaan dengan `BE-EXT-01`. **2.** `BE-LAB-15` kini tidak lagi tertahan `BE-LAB-01`; penahannya tinggal `BE-LAB-14`. **3.** Terbitkan task Billing untuk temuan `FINAL`/`CLOSED` pada bagian 9.2 — itu temuan keselamatan finansial, bukan sekadar test merah. **4.** Bereskan empat migration tertunda pada `QuilvianNewDevYoga` bersama pemilik modul masing-masing, supaya riwayat migrationnya kembali berurutan. **5.** Terapkan migration ini ke database lain yang membutuhkannya, lewat wewenang tersendiri |

**Keluaran `git status --short` di akhir pekerjaan:**

```text
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs
 M Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs
 M Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M QuilvianSystemBackend.Tests/BillingManagement/BillingSettlementServiceTests.cs
 M Repositories/Configurations/HealthServices/LabOrderConfiguration.cs
 M docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md
 M docs/module-blueprints/laboratorium/roadmap/traceability.md
?? Migrations/20260902042242_AddLabOrderDiscipline.Designer.cs
?? Migrations/20260902042242_AddLabOrderDiscipline.cs
?? QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/
?? docs/module-blueprints/laboratorium/approval-requests/2026-09-02-temuan-billing-final-closed.md
?? docs/module-blueprints/laboratorium/task/
```

Tidak ada `git add`, `commit`, `push`, `merge`, maupun `rebase` yang dijalankan.

**Pembaruan register yang ikut ditulis.** `rules/rule-output/lokasi-laporan-task.md` mewajibkan
build skill memperbarui bukti pada roadmap dan traceability modulnya. Yang disentuh hanya itu:

| Berkas | Perubahan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Blok status `SELESAI SEBAGIAN` beserta tautan laporan pada bagian 3; baris `BE-LAB-01` pada tabel bagian 7; penahan `BE-LAB-15` dikurangi karena `BE-LAB-01` sudah dicabut |
| `roadmap/traceability.md` | Baris `FR-10.3` berpindah ke `SELESAI` beserta bukti; satu baris riwayat revisi ditambahkan dan kepala dokumen dinaikkan ke revision `9`. Sekaligus tercatat bahwa revision 6 sampai 8 memang tidak pernah masuk tabel riwayat — utang pembukuan milik pemilik blueprint, bukan akibat task ini |
| `approval-requests/2026-09-02-temuan-billing-final-closed.md` | **Baru** (`LAB-REQ-003`). Temuan lintas modul yang ditujukan kepada pemilik `billing-kasir`, mengikuti pola permintaan lintas modul yang sudah dipakai blueprint ini. Ditulis atas instruksi eksplisit pemilik repository. Bersifat operasional, bukan artefak desain, sehingga tidak masuk daftar hash manifest |

Tidak ada artefak blueprint lain yang disentuh: kontrak, kamus data, ERD, flowchart, matriks uji,
dan dokumen arsitektur tidak berubah satu baris pun.

---

## 8. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | Tidak ada |
| Pemilik / prefix pada registry | Laboratory / `Lab` |
| Status registry | `ACTIVE` — dinaikkan dari `PLANNED` pada 2026-09-02 oleh Muhammad Hamzah lewat `LAB-REQ-002`. Wewenangnya mencakup source dan **pembuatan** migration; eksekusi database di luar dev pemilik dan deployment tetap wewenang terpisah |
| Keberlakuan | `TOUCHED LEGACY` untuk `LabOrder` beserta configurationnya yang sudah ada; `NEW CODE` untuk enum `LabDiscipline`, kolom `Discipline`, ruas DTO, dan migration |
| Sumber tata kelola yang dibaca | `AGENTS.md` `2d1e88b`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `docs/engineering/QBE_EXCEPTIONS.json` (kosong, `exceptions: []`); akar `rules/` terpasang berisi 32 berkas |

### QBE ID yang berlaku

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| `QBE-ENT-002` | `Discipline` boleh kosong karena semantik domainnya memang begitu untuk baris lama; tipenya enum, bukan teks bebas |
| `QBE-ENT-003` | Disiplin adalah klasifikasi bisnis dari `LAB-DEC-025`, bukan kebutuhan presentasi |
| `QBE-NAM-002` | Enum baru memakai prefix pemilik yang terdaftar: `LabDiscipline` |
| `QBE-NAM-004` | Prefix `Lab` diambil dari baris registry, bukan disimpulkan dari nama folder |
| `QBE-CFG-001`, `QBE-CFG-002` | Pemetaan, konversi enum, index, dan perilaku sesudah-simpan dideklarasikan pada `LabOrderConfiguration` yang sudah ada — tidak dibuat configuration kedua untuk entity yang sama |
| `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` | Capability tinggal di modul pemiliknya; baris registrynya `ACTIVE` sebelum kolom pertama dibuat |
| `QBE-SVC-001` | Penetapan dan pembacaan disiplin ada di `LabOrderService`; controller tidak menyentuh `ApplicationDbContext` |
| `QBE-API-001` | Route, `ApiResponse<T>`, kode status, dan gaya DTO mengikuti keluarga endpoint yang sudah ada |
| `QBE-DTO-001` | Yang diekspos DTO, bukan entity EF |
| `QBE-VAL-001` | Nilai disiplin di luar ketiganya ditolak sebelum penyimpanan |
| `QBE-ENUM-001` | `LabDiscipline` dimiliki modul Laboratorium, berada di `LaboratoryEnums.cs` bersama enum modul lainnya |
| `QBE-LOG-001` | Log `LabOrder.Create` memuat disiplin beserta `ActorUserId` |
| `QBE-AUD-001` | Riwayat `TrxLabTransitionHistory` tetap terpisah dari application logging; task ini tidak menggabungkan keduanya |
| `QBE-PERM-001` | Tidak ada permission baru; `[AccessPermission("LabOrder", ...)]` yang sudah ada tetap berlaku |

### QBE ID yang tidak berlaku

| QBE ID | Alasan |
| --- | --- |
| `QBE-NAM-001` | Tidak ada entity, file, configuration, atau DbSet `Trx*` baru yang dibuat |
| `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` | Task ini bukan `LEGACY MIGRATION`; tidak ada tabel yang dinamai ulang |
| `QBE-CODE-001` .. `QBE-CODE-006` | Tidak ada kode bisnis maupun nomor yang dialokasikan |
| `QBE-PAGE-001` | Tidak ada capability list baru |
| `QBE-OPT-001` | Tidak ada endpoint options/metadata yang dibuat |
| `QBE-DEL-001` | Lifecycle delete/cancel tidak disentuh |
| `QBE-ENT-001` | `LabOrder` sudah mewarisi `IdentityModel` sejak sebelum task ini |
| `QBE-TXN-001` | Satu `SaveChangesAsync` tunggal; tidak ada konsistensi lintas record yang perlu dibungkus transaksi |

---

## 9. Temuan di luar scope — modul Billing

Bagian ini ada karena pemilik repository memerintahkan perbaikan build project test pada sesi
yang sama dengan `BE-LAB-01`. Perbaikan itu membuka satu temuan kedua yang jauh lebih penting
daripada masalah build-nya sendiri. Keduanya **bukan** milik Laboratorium dan tidak mengubah satu
baris pun perilaku modul Laboratorium.

### 9.1 Build project test — **sudah diperbaiki**

| Field | Isi |
| --- | --- |
| Gejala | `QuilvianSystemBackend.Tests` gagal build: `BillingSettlementServiceTests.cs(727,20): error CS7036` |
| Sebab | Constructor `BillingSettlementService` bertambah parameter `BillingFinalizationService`, tetapi pembantu `CreateService` pada berkas test belum ikut disesuaikan sehingga hanya mengirim enam dari tujuh argumen |
| Bukti bahwa ini sudah ada sebelum `BE-LAB-01` | Berkas test terakhir disentuh commit `058e070` (2026-08-28); diff `BE-LAB-01` tidak menyentuh satu pun berkas Billing |
| Perbaikan | `CreateService` kini membangun `BillingFinalizationService` memakai pola yang **sudah dipakai** dua berkas test tetangga — `BillingFinalizationServiceTests.CreateService` dan `BillingArApHandoffServiceTests.CreateFinalizationService`: `new BillingFinalizationService(db, new ContractBillingChargeSourceAdapter(), new BillingArApHandoffService(db, logger), logger)`. Tidak ada pola baru yang diperkenalkan |
| Hasil | Project test kembali build: `0 Error(s)`. Suite berjalan: **852 lulus, 1 gagal** |
| Wewenang | Instruksi eksplisit pemilik repository pada sesi ini |

### 9.2 `FINAL` versus `CLOSED` — **belum diperbaiki, perlu keputusan pemilik Billing**

Satu-satunya test yang gagal adalah
`BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`:
invoice yang sudah lunas penuh diharapkan berstatus `FINAL`, kenyataannya `CLOSED`.

**Ini bukan test yang usang.** Kontrak modul Billing yang disetujui —
`docs/module-blueprints/billing-kasir/contracts/state-transition-matrix.md` baris 11 dan 12 —
menetapkan perpindahan **dua langkah**:

| Dari | Peristiwa | Ke | Pelaku |
| --- | --- | --- | --- |
| `OPEN` | finalisasi | `FINAL` | Billing |
| `FINAL` | AR/AP posting sukses | `CLOSED` | Sistem |

Sementara `BillingFinalizationService.cs` baris 127–130 memotong langkah pertama: invoice yang
lunas penuh langsung dilompatkan ke `CLOSED` pada saat finalisasi, disertai comment yang
menyatakan itu memang disengaja. Jadi yang menyimpang dari kontrak adalah **source produksi**,
bukan test-nya.

**Akibat yang lebih serius daripada test merah.**
`BillingArApHandoffService.RecordCorrectionIfLinkedAsync` — jalur yang menerbitkan koreksi AR
sesudah adjustment atau write-off diposting — dibuka dengan penjaga berikut pada baris 150:

```csharp
if (invoice is null || invoice.Status != BillingInvoiceStatuses.Final) return;
```

Karena invoice yang lunas penuh tidak pernah berstatus `Final`, setiap adjustment atau write-off
atas invoice semacam itu **keluar diam-diam tanpa menerbitkan koreksi AR apa pun**. Tidak ada
exception, tidak ada log kegagalan, tidak ada yang memberi tahu siapa pun.

> **Contoh.** Tagihan Rp2.000.000 dibayar lunas, invoice menjadi `CLOSED`. Esoknya ketahuan satu
> tindakan salah tagih dan dibuat adjustment Rp300.000. Koreksi AR-nya tidak pernah terbentuk.
> Pembukuan piutang tetap memakai angka lama, dan selisihnya baru ketahuan saat rekonsiliasi —
> kalau memang ada yang merekonsiliasi.

**Dua jalan keluar yang mungkin, dan keduanya keputusan pemilik Billing:**

| Pilihan | Isi | Konsekuensi |
| --- | --- | --- |
| A — source mengikuti kontrak | Finalisasi selalu menghasilkan `FINAL`; hanya AR/AP posting yang sukses yang memindahkannya ke `CLOSED` | Test yang gagal langsung hijau. `RecordCorrectionIfLinkedAsync` kembali bekerja. Perlu ditelusuri siapa yang memindahkan `FINAL` → `CLOSED` sesudah posting, dan apakah jalur itu memang sudah ada |
| B — kontrak diubah mengikuti source | `state-transition-matrix.md` direvisi supaya lunas penuh boleh langsung `CLOSED` | Penjaga pada `RecordCorrectionIfLinkedAsync` **wajib** ikut menerima `CLOSED`, kalau tidak, lubang koreksi AR di atas menjadi permanen dan resmi. Revisi kontrak perlu persetujuan pemilik modul |

**Yang sengaja tidak dilakukan.** Test tidak diubah agar mengharapkan `CLOSED`. Mengubahnya akan
membuat suite hijau sambil mengunci penyimpangan terhadap kontrak beserta lubang koreksi AR-nya —
persis kebalikan dari gunanya test itu ada. Kegagalan ini dibiarkan terlihat sampai pemilik
Billing memutuskan A atau B.

**Cakupan kerja yang dibutuhkan** melampaui `BE-LAB-01` maupun perbaikan build: ia menyentuh
perilaku finansial produksi dan kontrak modul lain. Karena itu ia diserahkan sebagai task Billing
tersendiri, bukan diselesaikan di sini.

Temuan ini ditulis lengkap beserta bukti, contoh berangka, dan kedua pilihan perbaikannya pada
[`approval-requests/2026-09-02-temuan-billing-final-closed.md`](../../../approval-requests/2026-09-02-temuan-billing-final-closed.md)
(`LAB-REQ-003`), mengikuti pola permintaan lintas modul yang sudah dipakai blueprint ini.
Dokumen itu dapat diteruskan apa adanya kepada pemilik `billing-kasir`.

### 9.3 Catatan yang berlaku di luar Billing

Satu error build pada project test menyembunyikan **853 test sekaligus** selama lima hari, dan
tidak ada yang memberi tahu. Menjalankan build project test pada CI menutup kelas masalah ini,
bukan hanya kejadian kali ini. Ini catatan untuk pemilik repository, bukan bagian dari
`BE-LAB-01`.
