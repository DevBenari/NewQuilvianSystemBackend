# Laporan Perubahan Backend — `BE-RWI-056`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-056` |
| Judul | Pengkajian awal dan pengkajian ulang tidak lagi saling menimpa |
| Slice | Gelombang `KEP-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-005`, `FR-KEP-006`; PRD 16.2 aturan 3; `AC-CAP012-02`; `VAL-KEP-08`, `VAL-KEP-11`; `UAT-KEP-06`, `UAT-KEP-07` |
| Contract version | State transition `0.3.0` bagian 1; validation `0.3.0` — `approved` 3 September 2026 lewat `RWI-DEC-092` |
| Dependency | `BE-RWI-054` — selesai, lihat laporannya |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah ≤ 3 (0), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), dampak schema/index/migration (2), keamanan tidak ada (0), workflow terbatas (1). Total **5** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, migration, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026; **dilanjutkan dan ditutup 8 September 2026** |
| Status | ✅ **SELESAI 8 September 2026.** Keempat acceptance criteria terbukti penuh. Kriteria 4 — yang sebelumnya tertahan karena penilaian **risiko jatuh** tidak dapat dibedakan antara *belum diisi* dan *tidak berisiko* — kini terbukti, **tanpa** mengubah bentuk `HasFallRisk` dan **tanpa** menggeser satu langkah pun perilaku poliklinik, medical check-up, maupun IGD. Rinciannya pada bagian 3.4, 6, dan 7 |

---

## 1. Masalah yang diperbaiki

Pengkajian keperawatan tidak dikerjakan sekali lalu selesai. Perawat mengisi pengkajian awal saat
pasien masuk kamar, lalu mengisi pengkajian ulang setiap hari sesudahnya. Sebelum perubahan ini,
tidak ada satu pun aturan yang menjaga bahwa keduanya tersimpan sebagai catatan yang berbeda pada
satu perawatan — dan tidak ada yang menghalangi lahirnya **dua pengkajian awal** untuk satu pasien.

Akibatnya dua-duanya berbahaya:

1. **Riwayat menjadi kabur.** Dua pengkajian awal pada satu perawatan membuat pertanyaan "berapa
   nilai nyeri pasien saat ia baru masuk" punya dua jawaban.
2. **Perawat tidak diarahkan.** Ketika perawat keliru memilih "pengkajian awal" pada hari ketiga,
   sistem menerimanya diam-diam alih-alih mengarahkannya ke pengkajian ulang.

> **Contoh nyatanya.** Ns. Sari mengisi pengkajian awal Tn. Budi pada hari Senin dengan skala nyeri
> 7. Hari Selasa Ns. Dewi membuka layar yang sama dan — karena tidak diarahkan apa pun — memilih
> "pengkajian awal" lagi, lalu mengisi skala nyeri 3. Rekam medis kini memuat dua "pengkajian awal"
> dengan angka berbeda, dan tidak ada yang tahu mana yang benar-benar awal.

---

## 2. Proses bisnis

**Tujuan.** Satu perawatan memiliki tepat satu pengkajian awal yang hidup; sisanya adalah
pengkajian ulang. Nilai pengkajian awal tidak boleh berubah karena pengkajian berikutnya dibuat.

**Pelaku.** Perawat pelaksana dan kepala ruangan.

**Langkah yang berurutan.**

1. Perawat membuat pengkajian awal saat pasien baru masuk kamar. Tersimpan.
2. Esok harinya perawat membuat pengkajian ulang harian. Tersimpan sebagai **catatan tersendiri**;
   pengkajian awal tidak tersentuh.
3. Saat perawat hendak menyelesaikan pengkajian, sistem memeriksa bagian penilaian yang wajib
   terisi. Bila ada yang kosong, penolakannya menyebut bagiannya satu per satu.

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Perawat membuat pengkajian awal **kedua** pada perawatan yang sama | Ditolak: *"Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang."* | `409` |
| Pengkajian awal sebelumnya **dibatalkan**, lalu perawat membuat yang baru | **Diterima.** Pembatalan memang jalan keluar dari pengkajian yang salah | `200` |
| Menyelesaikan pengkajian rawat inap yang skrining gizinya belum diisi | Ditolak: *"Pengkajian belum dapat diselesaikan. Bagian berikut masih kosong: skrining gizi."* | `400` |
| Pengkajian poliklinik, medical check-up, atau IGD | **Tidak tersentuh aturan mana pun di atas** | — |

> **Kenapa pengkajian awal yang dibatalkan tidak menghalangi.** Perawat yang salah memilih pasien
> membatalkan pengkajiannya, lalu membuat yang benar. Bila baris yang dibatalkan ikut dihitung,
> perawatan itu tidak akan pernah bisa punya pengkajian awal lagi — dan pasiennya kehilangan
> dokumen yang paling menentukan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/state-transition-matrix.md` bagian 1 dan `contracts/validation-matrix.md` | Mesin status pengkajian, `VAL-KEP-08` dan `VAL-KEP-11` |
| `04-prd-to-mvp.md` bagian `EPIC KEP-02` beserta `UAT-KEP-06` dan `UAT-KEP-07` | Skenario yang wajib terbukti |
| `02-backend-architecture.md` bagian 4.1 | Pencabutan unique index |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Penjagaan pembuatan dan penyelesaian yang sudah ada, termasuk `ValidateMedicalAssessmentRuleAsync` sebagai pola |
| `Areas/HealthServices/ClinicalManagement/Enums/FallRiskStatus.cs`, `NutritionRiskStatus.cs` | Bentuk nilai "belum diisi" pada kedua penilaian risiko |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Index yang sudah ada, termasuk milik `BE-RWI-040` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `ValidateSingleInitialNursingAssessmentAsync` — penjaga satu pengkajian awal aktif per perawatan, dijawab `409`; `BagianPengkajianKeperawatanYangKosong` — pemeriksaan isian wajib saat penyelesaian, khusus pengkajian yang menempel pada perawatan rawat inap; konstanta kalimat `VAL-KEP-11` |
| `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs` | Index parsial **non-unique** `IX_TrxPatientAssessment_Episode_Type_Active` pada `(InpEpisodeId, AssessmentType)` dengan penyaring `"AssessmentType" = 0 AND "IsDelete" = false` |
| `Migrations/20260906122534_AddInitialNursingAssessmentPartialIndex.cs` beserta `.Designer.cs` | Migration index parsial; `Down` menghapusnya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentSeparationTests.cs` | **Berkas baru.** 7 uji pada 6 September, **ditambah 5 uji** pada 8 September untuk kriteria 4 — totalnya 12 |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` *(8 September)* | `CalculateFallRiskStatus` menerima **kategori yang dinyatakan perawat** dan penanda pengkajian rawat inap; `NormalizeAssessmentData` tidak lagi menimpa "belum diisi" menjadi "tidak berisiko"; kedua overload `CalculateAssessmentValues` menerima penanda itu; penjagaan isian wajib dipasang pada **pintu kedua** penyelesaian, yaitu pembuatan dengan `completeImmediately` |
| `Tests/.../NursingAssessmentContextTests.cs`, `NursingAssessmentIntegrityTests.cs`, `NursingAssessmentMonitoringTests.cs` *(8 September)* | Pembangun permintaan rawat inap kini **menyatakan** `FallRiskStatus.NoRisk`, karena pernyataan "tidak berisiko" memang harus ditulis dan bukan disimpulkan dari diamnya permintaan |

### 3.4 Bagaimana kriteria 4 akhirnya dapat ditegakkan *(8 September 2026)*

Revisi laporan sebelumnya menyimpulkan bahwa `UAT-KEP-07` tidak dapat ditegakkan tanpa menjadikan
`CreatePatientAssessmentRequest.HasFallRisk` nullable, dan bahwa perubahan itu merusak kontrak
permintaan bersama poliklinik, medical check-up, dan IGD. **Kesimpulan keduanya tepat; yang keliru
adalah anggapan bahwa itu satu-satunya jalan.**

Pembedanya sudah ada sejak awal dan hanya tidak pernah dibaca:

| Bukti | Isi |
| --- | --- |
| `CreatePatientAssessmentRequest.FallRiskStatus` dan `UpdatePatientAssessmentRequest.FallRiskStatus` | **Sudah ada**, bertipe `FallRiskStatus`, berbawaan `Unknown`. Keduanya **diabaikan** perhitungan lama, yang menurunkan kategori sepenuhnya dari `HasFallRisk` |
| `FallRiskStatus.Unknown = 0` | **Sudah ada** pada enum. Nilai "belum diisi" tidak perlu dibuat |
| `NutritionRiskStatus` | Diambil **apa adanya** dari permintaan. Skrining gizi karena itu sejak dulu dapat kosong, sedangkan risiko jatuh tidak — asimetri itulah sebab sebenarnya, bukan bentuk `HasFallRisk` |
| `contracts/validation-matrix.md` `VAL-KEP-09` | Berbunyi *"Skor terisi tetapi kategorinya tidak → Kategori risiko jatuh belum dipilih."* Kontrak yang disetujui **memang** memperlakukan kategori risiko jatuh sebagai pilihan perawat yang terpisah dari skornya |
| Frontend `inpatient-nursing-workspace-utils.js` | `getMissingAssessmentGroups` sudah memperlakukan `fallRiskStatus === 0` sebagai "Risiko Jatuh belum diisi", dengan komentar *"Backend PatientAssessmentController menolak jika fallRiskStatus atau nutritionRiskStatus masih Unknown."* |
| Frontend `assessment-section.jsx` | Formulir rawat inap memulai `fallRiskStatus: 0` dan **mengirimkannya** apa adanya pada payload pembuatan dan pembaruan |

Jadi frontend rawat inap sudah membangun perilaku ini dan sudah mengirim field-nya; backend yang
belum membacanya. Perbaikannya karena itu **nol perubahan bentuk data dan nol perubahan frontend**.

**Aturan barunya, selengkapnya:**

| Keadaan | Sebelum | Sesudah |
| --- | --- | --- |
| Rawat inap, penanda risiko mati, kategori **tidak** dinyatakan | `NoRisk` — pernyataan klinis yang tidak pernah dibuat perawat | `Unknown`; penyelesaian ditolak `400` menyebut "penilaian risiko jatuh" |
| Rawat inap, penanda risiko mati, kategori dinyatakan `NoRisk` | `NoRisk` | `NoRisk`; penyelesaian **diterima** |
| Rawat inap, penanda risiko menyala | Diturunkan dari skor | **Tidak berubah** — tetap diturunkan dari skor |
| **Poliklinik, medical check-up, IGD** — apa pun isinya | `NoRisk` | **`NoRisk`, tidak bergeser satu langkah pun** |

Kategori yang dinyatakan dipakai **hanya** sebagai jawaban atas pertanyaan "sudah diisi atau
belum". Ia tidak pernah dipakai untuk menaikkan tingkat risiko: permintaan yang menyatakan
`HighRisk` tanpa satu pun penanda risiko tetap dirapikan menjadi `NoRisk`, sehingga tidak ada jalan
bagi klien untuk mengarang tingkat risiko yang tidak didukung isian.

**Pintu kedua penyelesaian ikut ditutup.** `POST /` dengan `completeImmediately = true` melahirkan
pengkajian yang langsung berstatus `Completed` **tanpa** melewati `PATCH /{id}/complete`. Menjaga
satu pintu saja berarti aturan isian wajib dapat dilewati hanya dengan menyalakan satu flag.
Penyaringnya sama persis dengan pintu pertama — keberadaan perawatan — sehingga poliklinik, medical
check-up, dan IGD tetap tidak tersentuh.

---

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua sebab penolakan baru** pada endpoint yang sudah ada, keduanya hanya berlaku bagi pengkajian rawat inap: `409` pengkajian awal kedua, dan `400` isian wajib kosong. Nol endpoint baru, nol field berubah. Jalur poliklinik, medical check-up, dan IGD tidak menerima satu pun aturan baru |
| Database | Satu index parsial non-unique. Nol kolom, nol tabel. Migration `20260906122534_AddInitialNursingAssessmentPartialIndex` **dibuat, belum diterapkan** ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE` — nol atribut hak akses ditambah atau diubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. Kini menolak pengkajian awal kedua pada satu perawatan dengan `409` | `PatientAssessment : Create` |
| `PATCH` | `/{id}/complete` | Menyelesaikan pengkajian. Kini menolak `400` bila bagian penilaian wajib masih kosong pada pengkajian rawat inap | `PatientAssessment : Update` |

**Nol endpoint baru dibuat task ini.**

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| Skenario **dua pengkajian berurutan**: `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` | Dua baris; skala nyeri pengkajian pertama tetap 7 sebelum dan sesudah pengkajian kedua dibuat | `PASS` | Uji |
| Skenario **pengkajian awal kedua**: `PengkajianAwalKedua_Ditolak409DenganArahan` | `409` beserta kalimat `VAL-KEP-11` apa adanya; tetap satu baris | `PASS` | Uji |
| Skenario **pengkajian awal dibatalkan lalu diulang**: `PengkajianAwalDibatalkan_PengkajianAwalBerikutnyaDiterima` | Diterima; dua baris, satu di antaranya `Cancelled` | `PASS` | Uji |
| Skenario isian wajib kosong: `MenyelesaikanPengkajianTanpaSkriningGizi_Ditolak400` | `400`; pesannya menyebut "skrining gizi"; status tidak menjadi `Completed` | `PASS` | Uji |
| Regresi: `PoliklinikBerantre_TidakTersentuhAturanSatuPengkajianAwal` | Dua pengkajian awal pada dua kunjungan rawat jalan tetap diterima | `PASS` | Uji |
| Regresi: `PengkajianPoliklinik_TetapDapatDiselesaikanTanpaSkriningGizi` | `200`; status menjadi `Completed` seperti sebelumnya | `PASS` | Uji |
| Pemeriksaan **index memang parsial**: `IndexParsialPerawatanJenis_TerbentukDanNonUnique` | Index bernama benar, non-unique, penyaringnya menyebut `AssessmentType` dan `IsDelete`; index pencarian milik `BE-RWI-040` tetap ada tanpa penyaring | `PASS` | Uji |
| **8 September 2026** — `dotnet build QuilvianSystemBackend.sln` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| **8 September 2026** — `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 453, Skipped: 0, Total: 453` | `PASS` | Keluaran perintah |
| Penolakan penyelesaian karena **risiko jatuh** belum diisi (`UAT-KEP-07`): `MenyelesaikanPengkajianTanpaRisikoJatuh_Ditolak400` | Tersimpan `Unknown`, bukan `NoRisk`; `400`; pesannya menyebut "penilaian risiko jatuh" dan **tidak** menyebut skrining gizi yang memang terisi; status tidak menjadi `Completed` | `PASS` | Uji |
| Dua bagian kosong disebut **satu per satu**: `MenyelesaikanPengkajianDuaBagianKosong_KeduanyaDisebutSatuPerSatu` | Pesannya memuat `penilaian risiko jatuh, skrining gizi` apa adanya | `PASS` | Uji |
| Perawat menyatakan pasiennya **tidak berisiko**: `RisikoJatuhDinyatakanTidakBerisiko_PengkajianDapatDiselesaikan` | Tersimpan `NoRisk`; penyelesaian `200` | `PASS` | Uji |
| **Pintu kedua** penyelesaian: `PengkajianLangsungSelesai_TanpaRisikoJatuh_Ditolak400` | `400`; **nol baris** tersimpan | `PASS` | Uji |
| Regresi jalur bersama: `PengkajianPoliklinik_TanpaRisikoJatuh_TetapTersimpanNoRisk` | Pengkajian poliklinik tanpa sebutan risiko jatuh tetap tersimpan `NoRisk`, dan `InpEpisodeId`-nya kosong | `PASS` | Uji |
| Bentuk index parsial dibaca langsung dari **katalog PostgreSQL** | `CREATE INDEX "IX_TrxPatientAssessment_Episode_Type_Active" … WHERE (("AssessmentType" = 0) AND ("IsDelete" = false))` — **non-unique**, penyaringnya persis seperti scope | `PASS` | `SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'TrxPatientAssessment'` pada container PostgreSQL 16 sekali pakai |
| Migration **maju** terhadap PostgreSQL sungguhan | `Database.Migrate()` dari nol berhasil; `20260906122534_AddInitialNursingAssessmentPartialIndex` tercatat pada `__EFMigrationsHistory` | `PASS` | Container `postgres:16` sekali pakai, database `quilvian_kep_test`, dibuang sesudah uji. **Nol database bersama, dev, staging, atau production tersentuh** |
| Migration **mundur** (`Down`) terhadap PostgreSQL sungguhan | Tidak dijalankan | `NOT RUN` | Tidak diminta DoD task ini |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pengkajian awal dan ulang tersimpan sebagai record terpisah; nilai record pertama sama persis sebelum dan sesudah record kedua dibuat | Terpenuhi | Uji `PengkajianAwalDanUlang_DuaRecordDanNilaiPertamaUtuh` |
| 2. Pengkajian awal kedua pada satu perawatan ditolak `409` dengan pesan yang benar | Terpenuhi | Uji `PengkajianAwalKedua_Ditolak409DenganArahan`; kalimatnya dibandingkan persis dengan `VAL-KEP-11` |
| 3. Pengkajian awal yang dibatalkan tidak menghalangi pembuatan berikutnya | Terpenuhi | Uji `PengkajianAwalDibatalkan_PengkajianAwalBerikutnyaDiterima` |
| 4. Menyelesaikan pengkajian dengan isian wajib kosong ditolak `400` dan menyebut bagian yang kosong satu per satu | **Terpenuhi** | Terbukti untuk **skrining gizi** (`MenyelesaikanPengkajianTanpaSkriningGizi_Ditolak400`), untuk **risiko jatuh** sebagaimana dituntut `UAT-KEP-07` (`MenyelesaikanPengkajianTanpaRisikoJatuh_Ditolak400`), untuk **keduanya sekaligus disebut satu per satu** (`MenyelesaikanPengkajianDuaBagianKosong_KeduanyaDisebutSatuPerSatu`), dan pada **pintu kedua** penyelesaian (`PengkajianLangsungSelesai_TanpaRisikoJatuh_Ditolak400`). Jalur bersama tidak bergeser: `PengkajianPoliklinik_TanpaRisikoJatuh_TetapTersimpanNoRisk` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Aturan pembuatan pada service | Terpenuhi |
| Satu index parsial **non-unique** | Terpenuhi |
| Satu migration | Terpenuhi — `20260906122534_AddInitialNursingAssessmentPartialIndex` |
| Uji empat skenario lulus | **Terpenuhi** — keempatnya lulus, termasuk skenario "risiko jatuh belum terisi" yang sebelumnya tidak dapat dibuktikan |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Kekurangan yang dulu menahan status, dan bagaimana ia ditutup** | **Ditutup 8 September 2026.** Jalan keluarnya bukan menjadikan `HasFallRisk` nullable, melainkan membaca field `FallRiskStatus` yang **sudah ada** pada kedua request dan selama ini diabaikan — sejalan dengan `VAL-KEP-09` yang memang memperlakukan kategori risiko jatuh sebagai pilihan perawat, dan sejalan dengan frontend rawat inap yang sudah mengirimkannya. Nol perubahan bentuk data, nol perubahan frontend, nol pergeseran pada jalur poliklinik, medical check-up, dan IGD. Rinciannya pada bagian 3.4. Catatan aslinya disimpan apa adanya di bawah ini |
| *Catatan asli 6 September 2026, disimpan apa adanya* | `UAT-KEP-07` menuntut penyelesaian pengkajian ditolak ketika **risiko jatuh** belum terisi. Itu **tidak dapat ditegakkan hari ini**, dan sebabnya ada pada bentuk data yang sudah berjalan: `CreatePatientAssessmentRequest.HasFallRisk` bertipe `bool` yang bawaannya `false`, dan `CalculateFallRiskStatus` mengubah `false` menjadi `FallRiskStatus.NoRisk`. Perawat yang **tidak mengisi** bagian risiko jatuh karena itu tersimpan sebagai pernyataan klinis "tidak berisiko" — pernyataan yang tidak pernah ia buat. Membedakan keduanya menuntut `HasFallRisk` menjadi nullable, dan itu perubahan yang merusak kontrak permintaan bersama poliklinik, medical check-up, dan IGD. **Dicatat sebagai temuan, tidak diperbaiki tanpa wewenang** (`API_RULES.md`) |
| Keputusan yang diambil, beserta buktinya | Bagian yang ditegakkan dipilih dari bukti yang sudah disetujui, bukan dikarang: `UAT-KEP-07` menyebut risiko jatuh, dan `FR-KEP-007` menempatkan nyeri, risiko jatuh, serta gizi sebagai tiga pengukuran yang wajib terbaca sebagai perkembangan. Nyeri **sengaja tidak** ikut diperiksa karena `HasPain` selalu bernilai benar atau salah sehingga tidak pernah dapat "kosong"; memaksakannya akan menolak pengkajian pasien yang memang tidak nyeri |
| Butir terbuka | `VAL-KEP-08` berbunyi "isian wajib **menurut kebijakan aktif**", sedangkan `MstClinicalAssessmentPolicy` yang dibangun `BE-RWI-055` hanya menyimpan batas waktu. Daftar isian wajib yang dapat diatur admin **belum ada kontraknya**. Menambahkannya berarti mengarang kebijakan klinis, dan itu dilarang. Butir ini diangkat ke pemilik klinis lewat `/qv-grill`. **Masih terbuka**; dua bagian yang ditegakkan hari ini — risiko jatuh dan skrining gizi — dipilih dari `UAT-KEP-07` dan `FR-KEP-007` yang sudah disetujui, bukan dikarang |
| Butir terbuka | **`VAL-KEP-09` belum terpasang** — permintaan yang mengisi *skor* risiko jatuh tetapi tidak memilih *kategorinya* belum ditolak dengan kalimat "Kategori risiko jatuh belum dipilih." Aturan itu tidak termasuk acceptance criteria `BE-RWI-056`, sehingga dicatat sebagai temuan dan **tidak** dikerjakan tanpa wewenang task tersendiri |
| Temuan bagi pemilik `BE-RWI-065` | `POST /` dengan `completeImmediately = true` melahirkan pengkajian berstatus `Completed` **tanpa** mendaftarkannya ke mesin keutuhan — pendaftaran hanya terpasang pada `PATCH /{id}/complete`. Itu melanggar kriteria 2 `BE-RWI-065` (*"tidak boleh ada pengkajian `Completed` yang tidak punya baris keutuhan"*). Task ini hanya menutup sisi **isian wajib** pada pintu itu; sisi **pendaftaran keutuhan** adalah milik `BE-RWI-065` dan **dicatat sebagai temuan, tidak diperbaiki tanpa wewenang** |
| Delta kontrak | `IX_TrxPatientAssessment_Episode_Type_Active` dibuat sebagai index **kedua** pada pasangan kolom yang sama, bukan mengubah index milik `BE-RWI-040`. Index lama dipakai `dokter-rawat-inap` untuk mencari kajian menurut jenisnya; menempelkan penyaring `AssessmentType = 0` padanya akan mematikan kegunaan itu |
| Delta kontrak *(8 September)* | Field `fallRiskStatus` pada `POST /` dan `PUT /{id}` berubah dari **diabaikan** menjadi **bermakna**, khusus bagi pengkajian yang menempel pada perawatan rawat inap: ia menjawab "sudah diisi atau belum". **Nol field ditambah, nol tipe berubah, nol field dihapus**; klien yang mengirimkannya hari ini tidak perlu berubah sedikit pun, dan frontend rawat inap memang sudah mengirimkannya. Dua sebab penolakan yang sudah dicatat pada bagian 3.3 kini juga berlaku pada `POST /` ketika `completeImmediately = true` |
| Risiko tersisa *(8 September)* | Pengkajian rawat inap **lama** yang terlanjur tersimpan `NoRisk` padahal perawatnya tidak pernah mengisi bagian itu **tidak** dibedakan surut — task ini tidak menyentuh satu baris pun yang sudah ada. Pembedaan hanya berlaku bagi pengkajian yang lahir sesudah perubahan ini berjalan |
| Risiko tersisa *(8 September)* | `PUT /{id}` yang **tidak** menyertakan `fallRiskStatus` akan mengembalikan pengkajian rawat inap ke keadaan "belum diisi". Itu perilaku yang **sudah berlaku** bagi `nutritionRiskStatus` sejak dulu — keduanya field pengganti utuh, bukan patch — sehingga bukan kelas masalah baru. Frontend rawat inap memuat nilai lama ke formulir sebelum menyimpan, sehingga jalur nyatanya aman; klien API pihak ketiga yang mengirim PUT sebagian **tidak** aman, dan itu berlaku bagi kedua field |
| Risiko tersisa | Aturan satu pengkajian awal dijaga **service**, bukan unique index. Dua permintaan yang tiba benar-benar bersamaan karena itu masih dapat lolos berdua. Unique index sengaja tidak dipakai — `02-backend-architecture.md` `0.3` bagian 4.1 mencabutnya karena akan ikut menghitung baris yang dibatalkan. Bila kelak lomba itu terbukti terjadi, jalan keluarnya unique index **parsial yang juga menyaring status batal**, dan itu keputusan bentuk data yang perlu diputuskan terpisah |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini masih di working tree; **agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | 1. Bawa temuan pendaftaran keutuhan pada jalur `completeImmediately` ke pemilik `BE-RWI-065`. 2. Bawa `VAL-KEP-09` dan daftar isian wajib yang dapat diatur admin ke `/qv-grill` bersama pemilik klinis. 3. `BE-RWI-065` dan `BE-RWI-057` sudah selesai lebih dulu dan **tidak** perlu dikerjakan ulang: keduanya lulus pada uji regresi hari ini |

---

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Submodule | — |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, lifecycle `ACTIVE / LEGACY` |
| Status registry | Terdaftar pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 — Area `HealthServices`, Module `ClinicalManagement / Clinical`, jenis `BUSINESS DOMAIN / MODULE`, prefix `Cli`, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` — `TrxPatientAssessment` dan `PatientAssessmentController` adalah legacy yang sudah berjalan. **Nol entity baru, nol tabel baru, nol kolom baru, nol nilai enum baru** |
| QBE ID yang berlaku | `QBE-VAL-001` — penolakan menyebut bagian yang kosong satu per satu, bukan pesan teknis; `QBE-CODE-004` — tidak ada kode bisnis yang dialokasikan slice ini; `QBE-DTO-001` — bentuk DTO tidak berubah; `QBE-API-001` — nol endpoint baru, hanya sebab penolakan baru pada endpoint yang sudah ada; `QBE-MOD-001`, `QBE-MOD-002` — modul terdaftar; `QBE-ENUM-001` — nol nilai enum baru |
| QBE ID yang **tidak** berlaku | `QBE-NAM-001`, `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `NEW CODE` bernama baru dan bukan `LEGACY MIGRATION`. `QBE-ENT-001` s.d. `QBE-ENT-003` — nol entity dibuat |
| Legacy ratchet | Ditaati. Prefix `Trx*` pada `TrxPatientAssessment` **tidak** diganti; mengganti nama tabel legacy adalah pekerjaan `LEGACY MIGRATION` tersendiri yang menuntut wewenang database, dan tidak diberikan task ini |
| Hardcode role access | **Nol ditemukan** pada jalur yang disentuh. Nol atribut `[AccessAction]` atau `[AccessPermission]` ditambah atau diubah; penjagaan yang dipasang seluruhnya berupa kelayakan **data** dan **status**, bukan kewenangan |
