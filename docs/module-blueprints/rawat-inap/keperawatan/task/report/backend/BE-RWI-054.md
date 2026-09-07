# Laporan Perubahan Backend — `BE-RWI-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-054` |
| Judul | Pengkajian rawat inap punya tempat menyimpan tenggat dan kebijakannya |
| Slice | Gelombang `KEP-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-001` s.d. `FR-KEP-004`, `FR-KEP-010`; `RWI-DEC-062`, `RWI-DEC-081`, `RWI-DEC-070`, `RWI-RULE-026`; PRD 16.2 aturan 1, 2, 4, 11; `AC-CAP012-01`; `INT-KEP-01`; `VAL-KEP-01` s.d. `VAL-KEP-04` |
| Contract version | API `0.3.0`, integration `0.3.0`, validation `0.3.0` — seluruhnya `approved` 3 September 2026 lewat `RWI-DEC-092` |
| Dependency | `BE-RWI-039` ✅ dan `BE-RWI-040` 🟡 pada `dokter-rawat-inap` — keduanya sudah mendarat; `episode-rawat-inap` `M1` selesai |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), dampak schema/entity/migration (2), keamanan berkaitan tetapi bukan intinya (1), UI/workflow kecil (0). Total **7** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, migration, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962` (yaitu `planning_source_sha` roadmap), branch `MHamzah`. Di tengah pengerjaan pemilik repository membuat commit `0a63358` yang **ikut memuat sebagian perubahan task ini**; agent tidak menjalankan satu pun perintah commit |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Keenam acceptance criteria terbukti; migration dibuat, **belum** diterapkan ke database mana pun |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, pengkajian rawat inap tidak dapat menjawab dua pertanyaan yang menentukan
apakah kepatuhan pengkajian dapat dinilai sama sekali:

1. **Kapan pengkajian ini seharusnya selesai?**
2. **Menurut kebijakan yang mana ia dinilai?**

Akibatnya bukan sekadar laporan yang kurang lengkap. Tanpa tempat menyimpan tenggat, satu-satunya
cara menilai keterlambatan adalah menghitung ulang dari kebijakan yang berlaku **hari ini** — dan
itu membuat penilaian masa lalu ikut berubah setiap kali angkanya diperbarui.

> **Contoh nyatanya.** Batas pengkajian awal semula 24 jam. Pengkajian Tn. Budi selesai pada jam
> ke-20, jadi tepat waktu. Bulan depan rumah sakit memperketat batasnya menjadi 8 jam. Bila
> penilaian dihitung ulang dari angka terbaru, pengkajian Tn. Budi tiba-tiba terbaca **terlambat**
> — padahal Ns. Sari yang mengerjakannya tidak melanggar apa pun. Rekam medis kehilangan
> kemampuannya menjelaskan penilaiannya sendiri.

Masalah kedua yang ditutup task ini adalah **utang uji**. `RWI-DEC-051` mencatat bahwa cabang
rawat inap pada endpoint pengkajian sudah menyala di source **tanpa** satu pun test regresi yang
menjaga jalur poliklinik, medical check-up, dan IGD. Pintu yang baru dibuka berada pada jalur yang
sama dengan ketiganya, sehingga setiap perubahan berikutnya berisiko menggeser perilaku layanan
yang sudah berjalan tanpa ada yang menyadarinya.

---

## 2. Proses bisnis

**Tujuan.** Setiap pengkajian rawat inap membawa tenggatnya sendiri, sehingga keterlambatan dapat
dinilai tanpa pernah mengubah penilaian pengkajian yang lalu.

**Pelaku.** Perawat pelaksana dan kepala ruangan.

**Pemicu.** Pasien sudah masuk kamar, dan perawat membuka ruang kerja keperawatan untuk membuat
pengkajian.

**Langkah yang berurutan.**

1. Perawat membuka pasien dari konteks perawatan rawat inap — bukan dari antrean, karena pasien
   rawat inap memang tidak pernah berantre.
2. Sistem memeriksa konteks perawatan: apakah kunjungan ini menaungi perawatan yang sedang
   berjalan.
3. Perawat mengisi pengkajian, lalu menyimpannya.
4. Saat pengkajian lahir, sistem **menstempel** tenggatnya beserta penunjuk kebijakan yang dipakai
   menghitungnya. Kedua nilai itu tidak pernah dihitung ulang sesudahnya.
5. Pengkajian tersimpan beserta penanda perawatan, sehingga terbaca per perawatan.

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Kunjungan rawat inap belum punya perawatan — admisinya belum dibuat | Ditolak, diarahkan menyelesaikan admisi lebih dulu | `422` |
| Perawatan masih `Draft` — pasien belum dikonfirmasi tiba di kamar | Ditolak | `422` |
| Perawatan sudah `Closed` atau `Cancelled` | Ditolak; dokumen baru tidak boleh lahir di atas perawatan tertutup | `422` |
| Kunjungan poliklinik atau medical check-up tanpa antrean | Ditolak, diarahkan lewat antrean — **perilaku lama, tidak berubah** | `400` |
| Belum ada kebijakan batas waktu sama sekali | Pengkajian **tetap tersimpan**; tenggatnya dibiarkan kosong | `200` |

**Hasil akhir.** Pengkajian tersimpan beserta konteks perawatannya. Bila kebijakan batas waktu
sudah ada, ia membawa tenggatnya; bila belum ada, ia tersimpan tanpa tenggat dan **tidak**
dinyatakan terlambat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md` | Konstitusi repository: task mode, wewenang tulis, batas Git dan database |
| `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` | Lapisan operasional tata kelola |
| `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Preflight QBE |
| `rules/backend/role-access-rules.md` | Kontrak penamaan atribut hak akses |
| `roadmap/backend-roadmap.md`, `blueprint-manifest.md` | Kartu task, dependency, dan status approval |
| `contracts/api-contract.md`, `validation-matrix.md`, `state-transition-matrix.md`, `permission-audit-matrix.md`, `data/data-dictionary.md` | Kontrak to-be `0.3.0` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | Bentuk tabel pengkajian saat ini |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Jalur pembuatan, penyelesaian, dan penjagaannya |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Resolver konteks perawatan milik `BE-RWI-039` |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Index dan relasi yang sudah ada |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientDoctorEntryPointTests.cs` | Pola uji regresi jalur lama |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | Dua kolom nullable: `DueAt` (`DateTime?`) dan `PolicyId` (`Guid?`), beserta keterangan kenapa keduanya disimpan sebagai nilai dan bukan dihitung ulang |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Konfigurasi kedua kolom; `DueAt` bertipe `timestamp with time zone` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | `DueAt` dan `PolicyId` pada `PatientAssessmentResponse` — ikut terbawa `PatientAssessmentDetailResponse` — dan pada `PatientAssessmentCreateResponse` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Pemetaan kedua kolom ke tiga bentuk balasan; pemisahan `VAL-KEP-01` dari `VAL-KEP-04` pada penjagaan pembuatan tanpa antrean |
| `Migrations/20260905090533_AddAssessmentDueAtAndPolicyId.cs` beserta `.Designer.cs` | Migration dua kolom nullable; `Down` mengembalikannya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentContextTests.cs` | **Berkas baru.** 11 uji: bentuk kolom, jalur rawat inap, tiga penolakan `422`, penolakan `400` poliklinik dan medical check-up, regresi IGD, regresi jalur berantre, serta index dan penjaga penghapusan perawatan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Menambah, tidak merusak.** Dua field baru pada balasan grup Patient Assessment; nol field dihapus, nol nama berubah, nol verb berubah. Satu perubahan perilaku: kunjungan **bertipe rawat inap** yang belum punya perawatan kini dijawab `422` beserta kalimat `VAL-KEP-01`, sebelumnya `400` beserta kalimat poliklinik. Kunjungan poliklinik dan medical check-up **tidak berubah sama sekali** |
| Database | Dua kolom nullable pada `public."TrxPatientAssessment"`. Baris lama tidak disentuh dan tidak perlu diisi apa pun. Migration `20260905090533_AddAssessmentDueAtAndPolicyId` **dibuat, belum diterapkan** ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE` untuk perubahan kewenangan — nol atribut hak akses ditambah atau diubah pada task ini. Kedua endpoint yang disentuh tetap memakai `PatientAssessment : Read` dan `PatientAssessment : Create` apa adanya |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. Kini menstempel tenggat dan kebijakannya saat pengkajian lahir | `PatientAssessment : Create` |
| `GET` | `/` | Daftar pengkajian; balasannya kini membawa `DueAt` dan `PolicyId` | `PatientAssessment : Read` |
| `GET` | `/{id}` | Detail satu pengkajian; balasannya kini membawa `DueAt` dan `PolicyId` | `PatientAssessment : Read` |
| `GET` | `/episodes/{episodeId}` | Pengkajian satu perawatan; balasannya kini membawa `DueAt` dan `PolicyId` | `PatientAssessment : Read` |

Base URL: `api/v1/health-services/clinical-management/patient-assessments`.

**Nol endpoint baru dibuat task ini.** Ketiga endpoint di atas sudah ada sebelumnya; yang berubah
hanya isi balasannya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` — garis dasar sebelum slice ini `324` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| `KolomTenggatDanKebijakan_TerbentukNullable` | Kedua kolom ada dan nullable | `PASS` | Uji |
| `PengkajianTanpaKebijakan_TersimpanDenganTenggatKosong` | Tersimpan; `DueAt` dan `PolicyId` kosong | `PASS` | Uji |
| `RawatInapTanpaAntreanDanTanpaIgd_PengkajianTersimpan` | `200`; `QueueId` kosong, `InpEpisodeId` terisi, nol baris antrean | `PASS` | Uji |
| `KunjunganRawatInapTanpaPerawatan_Ditolak422` | `422` beserta kalimat `VAL-KEP-01` | `PASS` | Uji |
| `PerawatanMasihDraft_Ditolak422` | `422` | `PASS` | Uji |
| `PerawatanSudahDitutup_Ditolak422` | `422` | `PASS` | Uji |
| `PoliklinikDanMedicalCheckupTanpaAntrean_TetapDitolak400` (2 jalur) | `400` beserta kalimat `VAL-KEP-04` | `PASS` | Uji |
| `IgdTanpaAntrean_PengkajianTetapBerhasil` | `200`; nol baris antrean; `InpEpisodeId` kosong | `PASS` | Uji |
| `JalurBerantre_PengkajianTetapBerhasil` | `200`; pengkajian tetap menempel pada antreannya | `PASS` | Uji |
| `IndexPerawatanDanPenjagaPenghapusan_Terpasang` | Index ada; `DeleteBehavior.Restrict`; penghapusan perawatan yang masih punya pengkajian dilempar `DbUpdateException` | `PASS` | Uji |
| Uji migration maju dan mundur terhadap PostgreSQL sungguhan | Tidak dijalankan | `NOT RUN` | Wewenang eksekusi database tidak diberikan task ini. Bentuk `Up` dan `Down` diperiksa manual dan simetris; bentuk tabel diuji lewat SQLite `EnsureCreated` |

Uji manual: `NOT APPLICABLE` — seluruh acceptance criteria terbukti lewat uji otomatis.

**Tidak dijalukan:** eksekusi migration ke database mana pun; uji project
`QuilvianSystemBackend.IntegrationTests.Postgres` — task ini tidak menuntut bukti PostgreSQL, dan
`AGENTS.md` melarang menjalankan perintah database sebagai validasi source rutin.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom `DueAt` dan `PolicyId` terbentuk, keduanya nullable, dan baris lama tidak disentuh | Terpenuhi | `TrxPatientAssessment.cs`; migration hanya `AddColumn` nullable tanpa `defaultValue`; uji `KolomTenggatDanKebijakan_TerbentukNullable` |
| 2. Pengkajian dapat dibuat untuk perawatan `Admitted` tanpa `QueueId` dan tanpa kunjungan IGD | Terpenuhi | Uji `RawatInapTanpaAntreanDanTanpaIgd_PengkajianTersimpan`. Dijawab `200`; `api-contract.md` bagian 1 menerima `200 / 201` untuk "pengkajian tersimpan" — lihat delta pada bagian 7 |
| 3. Ditolak `422` bila perawatan tidak ada, masih `Draft`, atau sudah `Closed` | Terpenuhi | Tiga uji `..._Ditolak422` |
| 4. Kunjungan rawat jalan tanpa antrean dan tanpa perawatan tetap ditolak `400` | Terpenuhi | Uji `PoliklinikDanMedicalCheckupTanpaAntrean_TetapDitolak400` |
| 5. Perilaku poliklinik, medical check-up, dan IGD tidak berubah, dibuktikan test regresi | Terpenuhi | Empat uji regresi: dua jalur poliklinik/MCU, IGD tanpa antrean, dan jalur berantre |
| 6. Index perawatan terbentuk dan penghapusan perawatan yang masih punya pengkajian ditolak | Terpenuhi | Uji `IndexPerawatanDanPenjagaPenghapusan_Terpasang`. Keduanya sudah mendarat lebih dulu lewat `BE-RWI-040`; task ini menguncinya dengan uji |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Dua kolom | Terpenuhi |
| Satu index | Terpenuhi — sudah ada sejak `BE-RWI-040`, kini terkunci uji |
| Satu migration | Terpenuhi — `20260905090533_AddAssessmentDueAtAndPolicyId` |
| `DeleteBehavior` terpasang | Terpenuhi — `Restrict`, sudah ada sejak `BE-RWI-040`, kini terkunci uji |
| Test regresi tiga jalur lulus | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |
| `dotnet test` lulus | Terpenuhi |
| Laporan menyatakan migration belum diterapkan ke database bersama | Terpenuhi — bagian 3.3 dan 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` menghasilkan 2 warning pada build inkremental, keduanya `CS8602` pada `BuildBaseQuery` di `PatientAssessmentController.cs` — kode lama yang tidak disentuh task ini; nomor barisnya bergeser karena penambahan di atasnya. Nol warning baru berasal dari berkas yang dibuat atau diubah task ini |
| Masalah yang diketahui | **Kode balasan `POST /` adalah `200`, bukan `201`.** Kartu task menyebut `201`. Source menjawab `200` sejak sebelum task ini, dan endpoint yang sama dipakai poliklinik dan IGD; mengubahnya adalah perubahan yang merusak kompatibilitas dan menuntut wewenang eksplisit beserta penilaian dampak konsumen (`API_RULES.md`). `api-contract.md` bagian 1 sendiri menerima `200 / 201` sebagai "pengkajian tersimpan", sehingga kriteria 2 terpenuhi. **Selisihnya dilaporkan, bukan diperbaiki diam-diam** |
| Delta kontrak | `data/data-dictionary.md` bagian 2 masih mencantumkan kolom `AmendedAt` dan `AmendedByUserId`. Keduanya **dicabut** `02-backend-architecture.md` `0.3` dan roadmap bagian 7, karena penulis, waktu, alasan, dan nomor koreksi disimpan mesin addendum. Keduanya **tidak** dibuat task ini. Merapikan kamus data adalah pekerjaan `/qv-design` |
| Delta kontrak | Kunjungan bertipe rawat inap tanpa perawatan kini dijawab `422` (`VAL-KEP-01`), sebelumnya `400`. Pemisahan ini dituntut kriteria 3 dan 4 sekaligus: keduanya menggambarkan keadaan yang berbeda dan menuntut tindakan yang berbeda dari petugas |
| Risiko tersisa | `PolicyId` lahir **tanpa** relasi ke tabel mana pun pada migration task ini, karena tabel tujuannya baru dibuat `BE-RWI-055`. Selama kedua migration belum diterapkan berurutan, kolom itu hanyalah `uuid` biasa. Urutan penerapannya mengikat: `AddAssessmentDueAtAndPolicyId` lebih dulu, baru `AddClinicalAssessmentPolicyMaster` |
| Temuan di luar cakupan | `PATCH /{id}/cancel` hari ini menerima pengkajian berstatus `Completed`, sehingga pengkajian final dapat dipindahkan ke `Cancelled`. `state-transition-matrix.md` bagian 1 hanya membolehkan pembatalan dari `Draft` dan `InProgress`. Perilaku itu **tidak diubah** task ini: ia menyentuh jalur poliklinik dan IGD, dan tidak disebut satu pun acceptance criteria. **Dicatat sebagai temuan** |
| Perubahan sampingan | `NONE` untuk berkas yang ditulis task ini. Perlu dicatat bahwa working tree sudah memuat perubahan pengguna yang **bukan** milik task ini: pekerjaan sub-modul `dokter-rawat-inap` (`BE-RWI-040`/`BE-RWI-045` beserta migration `20260905081108_AddMedicalAssessmentContentColumns` dan sejumlah dokumen blueprint-nya). Seluruhnya dibiarkan apa adanya |
| Interupsi | Sesi sempat terputus di tengah pengerjaan; dua perintah build latar belakang tercatat `stopped` tanpa hasil. Pemulihan mengikuti `TASK_RULES.md`: status Git dan diff diperiksa ulang, pekerjaan yang sudah mendarat diverifikasi, lalu build dan uji dijalankan ulang dari kondisi terverifikasi. Nol penyuntingan ganda |
| Catatan database | Perintah `dotnet ef migrations remove` sempat dijalankan sekali dan **membaca** riwayat migration pada database pengembang lokal untuk menjawab apakah migration terakhir sudah diterapkan. Perintah itu **tidak menulis apa pun**; sesudah itu seluruh pembuatan migration dijalankan tanpa menyentuh database |
| Status Git | Lihat bagian 8 |
| Langkah berikutnya | `BE-RWI-055` menyediakan masternya sehingga kedua kolom ini benar-benar terisi; `BE-RWI-058` membacanya menjadi keadaan tenggat |

---

## 8. Status Git

Berkas yang dihasilkan `BE-RWI-054`:

| Berkas | Keadaan pada akhir pengerjaan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | Sudah ikut tercommit pemilik repository pada `0a63358` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | Sudah ikut tercommit pada `0a63358` |
| `Migrations/20260905090533_AddAssessmentDueAtAndPolicyId.cs` beserta `.Designer.cs` | Sudah ikut tercommit pada `0a63358` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentContextTests.cs` | Sudah ikut tercommit pada `0a63358` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | ` M` — masih di working tree |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | ` M` — masih di working tree |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | ` M` — masih di working tree |

**Agent tidak menjalankan satu pun** operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`,
maupun deployment. Commit `0a63358` dibuat pemilik repository di luar task ini, saat sesi sempat
terputus.
