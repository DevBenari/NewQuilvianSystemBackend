# Laporan Perubahan Backend — `BE-RWI-055`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-055` |
| Judul | Batas waktu pengkajian dibaca dari master, bukan ditanam di kode |
| Slice | Gelombang `KEP-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-010`, `FR-KEP-011`; `RWI-RULE-021` (belum final, dan memang tidak perlu final); PRD 16.2 aturan 11; `AC-CAP012-04`; `VAL-KEP-17`, `VAL-KEP-18` |
| Contract version | API `0.3.0`, validation `0.3.0` — `approved` 3 September 2026 lewat `RWI-DEC-092` |
| Dependency | Tidak ada. Berjalan paralel dengan `BE-RWI-054`; kolom `PolicyId` yang dibuat task itu baru memperoleh relasinya di sini |
| Klasifikasi | `HEAVY` — repository 1 (0), berkas diperiksa 9–20 (1), berkas diubah > 8 (2), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), dampak schema/entity/migration (2), keamanan berkaitan (1), UI/workflow satu layar (1). Total **9** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, migration, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`. Pemilik repository membuat commit `0a63358` di tengah pengerjaan yang ikut memuat sebagian perubahan task ini; agent tidak menjalankan satu pun perintah commit |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti; migration dibuat, **belum** diterapkan ke database mana pun |

---

## 1. Masalah yang diperbaiki

Batas waktu penyelesaian pengkajian adalah **kebijakan klinis**, bukan ketetapan rekayasa. Sampai
hari ini `RWI-RULE-021` belum final karena pemilik klinisnya belum ditunjuk — dan justru itulah
alasan task ini ada. PRD 16.2 aturan 11 melarang angka SLA klinis ditanam di kode dan mewajibkannya
menjadi konfigurasi.

Sebelum perubahan ini, satu-satunya tempat angka semacam itu hidup adalah
`MstInpatientSetting.InitialAssessmentTargetHours` — satu baris pengaturan tunggal, **tanpa versi**,
dan tanpa pembedaan per jenis pengkajian maupun per jenis pelayanan. Bentuk itu menimbulkan dua
masalah sekaligus:

1. **Tidak dapat dibedakan.** Batas pengkajian awal, pengkajian ulang harian, dan pengkajian
   rencana pemulangan dipaksa memakai satu angka yang sama.
2. **Menghapus masa lalu.** Mengubah angkanya berarti mengubah penilaian seluruh pengkajian yang
   pernah dibuat.

> **Contoh nyatanya.** Batas pengkajian awal semula 24 jam, dan pengkajian Tn. Budi selesai pada
> jam ke-20 — tepat waktu. Rumah sakit lalu memperketatnya menjadi 8 jam. Dengan pengaturan yang
> tidak berversi, laporan kepatuhan bulan lalu ikut berubah dan Ns. Sari terbaca melanggar aturan
> yang **belum ada** saat ia bekerja.

---

## 2. Proses bisnis

**Tujuan.** Clinical governance mengatur sendiri batas waktu pengkajian tanpa meminta perubahan
kode, dan tanpa pernah mengubah penilaian pengkajian yang lalu.

**Pelaku.** Clinical governance lewat layar master; sistem lewat jalur pembuatan pengkajian.

**Langkah yang berurutan.**

1. Clinical governance membuka layar master kebijakan batas waktu pengkajian.
2. Ia menambahkan satu baris: jenis pengkajian, jenis pelayanan bila perlu, batas waktu dalam
   menit, dan periode berlakunya.
3. Setiap kali pengkajian baru lahir, sistem mencari kebijakan yang **berlaku pada saat itu**,
   menghitung tenggatnya, lalu menstempel keduanya pada pengkajian tersebut.
4. Ketika kebijakannya kelak berubah, baris lama diberi tanggal akhir berlaku dan baris baru
   ditambahkan. Pengkajian yang sudah ada **tidak tersentuh sama sekali**.

**Aturan pemilihan kebijakan.**

| Aturan | Isinya |
| --- | --- |
| Menurut waktu | Yang dipilih adalah kebijakan yang periodenya memuat saat pengkajian dibuat |
| Yang khusus menang | Kebijakan yang menyebut jenis pelayanan mengalahkan kebijakan tanpa jenis pelayanan |
| Bila seri | Yang paling baru berlaku yang dipakai |

> **Contoh berangka.** Master memuat `KEP-AWAL-UMUM` tanpa jenis pelayanan dengan batas 1440 menit,
> dan `KEP-AWAL-RI` khusus rawat inap dengan batas 480 menit. Pengkajian awal Tn. Budi di unit rawat
> inap memperoleh `KEP-AWAL-RI`, sehingga tenggatnya 8 jam sejak pengkajian dibuat. Pengkajian awal
> di unit lain tetap memakai 24 jam.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Master masih kosong | **Tidak satu pun pengkajian memperoleh tenggat, dan tidak satu pun dinyatakan terlambat.** Pencatatan berjalan penuh — `VAL-KEP-17` |
| Kode kebijakan kembar | Ditolak `409`; kode tidak pernah ditimpa diam-diam |
| Batas waktu nol atau negatif | Ditolak `400` |
| Akhir berlaku mendahului awal berlaku | Ditolak `400` |
| Kebijakan dinonaktifkan | Berhenti dipakai ke depan; pengkajian yang sudah memakainya **tidak tersentuh** |
| Kebijakan yang sudah dipakai hendak dihapus | Ditolak `400` beserta saran menonaktifkannya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `rules/backend/master-data-endpoint-standard.md` | Sembilan endpoint baseline dan syarat bentuknya |
| `rules/backend/role-access-rules.md` | Kontrak penamaan `[AccessAction]` dan `[AccessPermission]` |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Preflight kepemilikan prefix `Mst` |
| `data/data-dictionary.md` bagian 7 | Bentuk kolom `MstClinicalAssessmentPolicy` |
| `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs` | Pola master rawat inap yang diminta roadmap sebagai acuan |
| `Areas/HealthServices/MasterData/Controllers/MedicalRecordAccessPurposeController.cs` dan `Services/MedicalRecordAccessPurposeService.cs` | Pola master data `NEW CODE` terdekat yang sudah memisahkan controller dari `ApplicationDbContext` |
| `Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs` | Bentuk baku `/filters/metadata` |
| `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs` | Bentuk sebenarnya "jenis pelayanan" pada source |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | Pendaftaran `DbSet` dan dependency injection |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstClinicalAssessmentPolicy.cs` | **Baru.** Entity master berversi: `PolicyCode`, `PolicyName`, `AssessmentType`, `ServiceUnitType?`, `DueWithinMinutes`, `EffectiveFrom`, `EffectiveTo?`, `Description`, `IsActive`, mewarisi `IdentityModel` |
| `Repositories/Configurations/HealthServices/MasterData/MstClinicalAssessmentPolicyConfiguration.cs` | **Baru.** Nama tabel, kunci, panjang kolom, unique pada `PolicyCode`, index pemilihan `(AssessmentType, ServiceUnitType, EffectiveFrom)`, dan index `(IsActive, IsDelete)` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<MstClinicalAssessmentPolicy> MstClinicalAssessmentPolicies` |
| `Areas/HealthServices/MasterData/DTOs/ClinicalAssessmentPolicyDtos.cs` | **Baru.** Balasan detail, opsi ringan, rekap, permintaan create/update/status, dan seluruh DTO metadata penyaring |
| `Areas/HealthServices/MasterData/Services/ClinicalAssessmentPolicyService.cs` | **Baru.** Pemilik seluruh pembacaan dan perubahan master; `ResolveEffectiveAsync`, `CalculateDueAsync`, `IsMasterEmptyAsync`, dan permukaan master data |
| `Areas/HealthServices/MasterData/Controllers/ClinicalAssessmentPolicyController.cs` | **Baru.** Sembilan endpoint baseline master data |
| `Program.cs` | Pendaftaran `ClinicalAssessmentPolicyService` |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Relasi `PolicyId` → `MstClinicalAssessmentPolicy` dengan `DeleteBehavior.Restrict`, beserta index `PolicyId` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Penstempelan `DueAt` dan `PolicyId` saat pengkajian lahir, memakai jenis pelayanan unit pengkajiannya |
| `Migrations/20260906121936_AddClinicalAssessmentPolicyMaster.cs` beserta `.Designer.cs` | Migration tabel master, tiga index, index `PolicyId`, dan foreign key `Restrict` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/HealthServices/MasterData/ClinicalAssessmentPolicyMasterTests.cs` | **Berkas baru.** 14 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Sembilan endpoint baru** pada grup master data. Nol endpoint lama berubah. Balasan `POST /patient-assessments` kini membawa `DueAt` dan `PolicyId` yang benar-benar terisi bila masternya ada |
| Database | Satu tabel baru `public."MstClinicalAssessmentPolicy"`, tiga index miliknya, satu index `IX_TrxPatientAssessment_PolicyId`, dan satu foreign key `Restrict` dari `TrxPatientAssessment.PolicyId`. Migration `20260906121936_AddClinicalAssessmentPolicyMaster` **dibuat, belum diterapkan** ke database mana pun. Urutan penerapannya mengikat: sesudah `20260905090533_AddAssessmentDueAtAndPolicyId` |
| Keamanan/Auth | Satu Resource baru `ClinicalAssessmentPolicy` dengan empat action: `Read`, `Create`, `Update`, `Delete`. Seluruhnya muncul di layar Akses Role, sehingga admin dapat memberikannya. Nol hardcode nama peran, nama departemen, nama posisi, maupun `UserType` |

---

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Clinical Assessment Policy

Base URL: `api/v1/health-services/master-data/clinical-assessment-policies`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi halaman: penyaring, pengurutan, pilihan enum, dan metadata form | `ClinicalAssessmentPolicy : Read` |
| `GET` | `/summary` | Rekap jumlah kebijakan, termasuk penanda master kosong | `ClinicalAssessmentPolicy : Read` |
| `GET` | `/` | Daftar kebijakan dengan pencarian, penyaringan, pengurutan, dan halaman | `ClinicalAssessmentPolicy : Read` |
| `GET` | `/options` | Pilihan ringan untuk kotak isian pada layar lain | `ClinicalAssessmentPolicy : Read` |
| `GET` | `/{id}` | Detail satu kebijakan | `ClinicalAssessmentPolicy : Read` |
| `POST` | `/` | Menambah kebijakan baru | `ClinicalAssessmentPolicy : Create` |
| `PUT` | `/{id}` | Mengubah seluruh field bisnis satu kebijakan | `ClinicalAssessmentPolicy : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan kebijakan | `ClinicalAssessmentPolicy : Update` |
| `DELETE` | `/{id}` | Menandai kebijakan terhapus; ditolak bila masih dipakai pengkajian | `ClinicalAssessmentPolicy : Delete` |

**Kode status yang perlu ditangani layar.**

| Kode | Artinya bagi pengguna |
| --- | --- |
| `200` | Berhasil |
| `400` | Isian tidak masuk akal, atau kebijakan masih dipakai sehingga tidak dapat dihapus |
| `404` | Data tidak ditemukan atau sudah dihapus |
| `409` | Kode kebijakan sudah dipakai kebijakan lain |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| Uji keadaan **master terisi**: `MasterMenyimpanBatasWaktuPerJenisPengkajianDanPelayanan` | Tersimpan lengkap beserta jenis pelayanan dan periode berlakunya | `PASS` | Uji |
| Uji keadaan **master kosong**: `MasterKosong_TidakMenghasilkanTenggatDanDinyatakanApaAdanya` | `IsMasterEmptyAsync` benar; tenggat kosong; rekap berbunyi "belum ditetapkan" | `PASS` | Uji |
| Uji keadaan **kebijakan berubah di tengah**: `KebijakanBerversi_DipilihMenurutSaatYangDitanyakan` | 10 September dijawab 1440 menit, 25 September dijawab 480 menit | `PASS` | Uji |
| `KebijakanKhusus_MengalahkanKebijakanUmum` | Rawat inap memperoleh `KEP-KHUSUS`, poliklinik memperoleh `KEP-UMUM` | `PASS` | Uji |
| `KebijakanDinonaktifkan_TidakMengubahPengkajianYangSudahAda` | `PolicyId` dan `DueAt` pengkajian tidak bergerak | `PASS` | Uji |
| `KebijakanYangDipakai_TidakDapatDihapus` | `400`; pesannya menyarankan menonaktifkan | `PASS` | Uji |
| `KebijakanYangBelumDipakai_DihapusSecaraSoftDelete` | `IsDelete` benar, `IsActive` salah, `DeleteDateTime` terisi; baris tidak hilang | `PASS` | Uji |
| `KodeKebijakanKembar_Ditolak409`, `BatasWaktuNol_Ditolak400`, `RentangTanggalTerbalik_Ditolak400` | Ketiganya ditolak dengan kode yang benar | `PASS` | Uji |
| `SembilanEndpointBaseline_Terpasang` | Kesembilan verb dan path ada, dan tidak ada endpoint lain | `PASS` | Uji |
| `MetadataPenyaring_MenjanjikanYangBenarBenarDidukung` | Pengurutan, arah, ukuran halaman, dan pilihan enum sesuai yang didukung `GET /` | `PASS` | Uji |
| `DaftarUtama_MenyaringDanBerhalaman` | Penyaring jenis pengkajian dan jenis pelayanan mempersempit hasil dengan benar | `PASS` | Uji |
| `BentukTabelMaster_SesuaiKamusData` | Nama tabel, schema, unique `PolicyCode`, index pemilihan, dan nullability sesuai | `PASS` | Uji |
| Kontrak penamaan hak akses: `SetiapAction_PunyaPasanganAtributYangNamanyaSama` | Ketiga nilai cocok huruf demi huruf pada seluruh action | `PASS` | Uji `NursingAssessmentAccessContractTests` |
| `TidakAdaEndpointAnonim`, `SourceBaru_TidakMemakaiHardcodeRole` | Nol endpoint anonim; nol anti-pola `IsInRole`, `UserType ==`, maupun nama peran | `PASS` | Uji `NursingAssessmentAccessContractTests` |
| **Uji hak akses memakai peran non-SuperAdmin lewat HTTP** | Tidak dijalankan | `NOT FEASIBLE` | Project uji SQLite memanggil controller **langsung**, sehingga `AccessPermissionFilter` dilewati — dinyatakan sendiri pada `ControllerTestHarness`. Repository tidak memiliki harness HTTP yang menyalakan filter itu. Penggantinya dijelaskan pada bagian 7 |
| Uji migration maju dan mundur terhadap PostgreSQL sungguhan | Tidak dijalankan | `NOT RUN` | Wewenang eksekusi database tidak diberikan task ini |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Master menyimpan batas waktu per jenis pengkajian per jenis pelayanan | Terpenuhi | `MstClinicalAssessmentPolicy.cs`; uji `MasterMenyimpanBatasWaktuPerJenisPengkajianDanPelayanan` dan `BentukTabelMaster_SesuaiKamusData` |
| 2. Kebijakan berversi; penilaian memakai kebijakan yang aktif saat pengkajian dibuat | Terpenuhi | `ResolveEffectiveAsync` menerima `atUtc`; `CalculateDueAsync` dipanggil dengan waktu kelahiran pengkajian; uji `KebijakanBerversi_DipilihMenurutSaatYangDitanyakan` dan `KebijakanDinonaktifkan_TidakMengubahPengkajianYangSudahAda` |
| 3. Master kosong tidak menahan pencatatan dan tidak menghasilkan penanda terlambat | Terpenuhi | Uji `MasterKosong_TidakMenghasilkanTenggatDanDinyatakanApaAdanya`; uji `PengkajianTanpaKebijakan_TersimpanDenganTenggatKosong` pada `BE-RWI-054` |
| 4. Endpoint memakai pasangan `[AccessAction]` dan `[AccessPermission]` bernama sama persis, dan diuji dengan peran non-SuperAdmin | **Terpenuhi sebagian** | Bagian penamaan **terbukti** lewat uji refleksi yang menyandingkan ketiga nilainya huruf demi huruf. Bagian "diuji dengan peran non-SuperAdmin" **belum terbukti**: repository tidak memiliki harness HTTP yang menyalakan `AccessPermissionFilter`. Lihat bagian 7 |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Satu tabel master | Terpenuhi |
| Configuration | Terpenuhi |
| `DbSet` | Terpenuhi |
| Migration | Terpenuhi — `20260906121936_AddClinicalAssessmentPolicyMaster` |
| Endpoint pengelolaan | Terpenuhi — sembilan endpoint baseline |
| Uji tiga keadaan master lulus | Terpenuhi |
| Uji hak akses non-SuperAdmin lulus | **Belum terpenuhi** — `NOT FEASIBLE` pada perkakas uji yang ada; penggantinya dijelaskan pada bagian 7 |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Preflight QBE | Area `HealthServices`, Module `Master / Reference`, prefix `Mst`, Lifecycle `ACTIVE` — sudah terdaftar pada registry, sehingga `QBE-MOD-002` dan `QBE-MOD-003` **tidak** memblokir. Keberlakuan `NEW CODE`. Aturan yang ditegakkan: `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-CFG-001` (configuration lengkap), `QBE-SVC-001` (controller tidak menyentuh `ApplicationDbContext`), `QBE-DTO-001` (nol entity EF dikembalikan sebagai kontrak), `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001` (soft delete beserta aktornya), `QBE-LOG-001`. `QBE-NAM-001` terpenuhi — nol prefix `Trx*` pada kode baru |
| Delta kontrak | **`ServiceUnitTypeId uuid?` menjadi `ServiceUnitType` enum nullable.** `data/data-dictionary.md` bagian 7 menuliskan kolom itu sebagai penunjuk ke tabel jenis pelayanan. Source **tidak memiliki** tabel itu: jenis pelayanan adalah enum `ServiceUnitType` yang melekat pada `MstServiceUnit`. Bentuk yang dipakai mengikuti source, sesuai presedensi "bila source berbeda, source yang berlaku dan selisihnya dilaporkan" |
| Delta kontrak | **Dua kolom di luar kamus data:** `PolicyName` dan `Description`. Keduanya ditambahkan karena layar master menuntut nama yang terbaca manusia; kamus data hanya menyebut kode. Dicatat sebagai delta, bukan diselundupkan |
| Delta kontrak | **`DELETE /{id}` dibuat**, dan itu bagian dari sembilan endpoint baseline master data yang wajib. Ia soft delete, dan menolak kebijakan yang masih dipakai |
| Butir terbuka | **Uji hak akses non-SuperAdmin.** `ControllerTestHarness` menyatakan sendiri bahwa pemeriksaan hak akses berada di lapisan yang dilewati uji ini. Penggantinya adalah uji refleksi yang menutup **penyebab sebenarnya** kegagalan `BE-RWI-034`: ketidakcocokan nama antara `[AccessAction]` dan `[AccessPermission]`, yang menghasilkan `403` permanen dan tidak terlihat saat diuji memakai SuperAdmin. Membangun harness HTTP adalah pekerjaan lintas modul yang menuntut wewenang tersendiri |
| Risiko tersisa | Selama master ini **kosong**, seluruh pemantauan kepatuhan pengkajian tidak menyala. Itu perilaku yang dirancang, bukan cacat — tetapi ia berarti rilis ke produksi menuntut clinical governance mengisi sekurang-kurangnya satu baris. `RWI-RULE-021` menahan **produksi**, bukan pembangunan |
| Risiko tersisa | Penghapusan kebijakan hanya diperiksa terhadap `TrxPatientAssessment`. Bila kelak tabel lain ikut menunjuk kebijakan ini, pemeriksaannya wajib ikut diperluas |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md` — status Git diperiksa, pekerjaan yang sudah mendarat diverifikasi, lalu build dan uji dijalankan ulang. Nol penyuntingan ganda |
| Status Git | Berkas task ini sebagian sudah ikut tercommit pemilik repository pada `0a63358`; sisanya masih di working tree. **Agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `BE-RWI-058` membaca master ini menjadi keadaan tenggat; `BE-RWI-064` memakainya membedakan "sudah tepat waktu" dari "batas waktu belum ditetapkan" |
