# Platform — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `PLT-BP-001` |
| Blueprint revision | `1` |
| Capability map revision | `2` |
| Status | `source-audited` — audit source sudah dijalankan dan hasilnya berlaku; dokumen ini **tidak** menyatakan platform siap implementasi |
| Sumber keputusan | `00-interview-decisions.md` revisi 1 — `DEC-PLT-001`..`DEC-PLT-005` |
| Backend SHA audit penuh | `ba75a05` cabang `sukmagp` |
| Frontend SHA yang diaudit | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` |
| Tanggal audit | `2026-09-04` — audit penuh; **diperluas hari yang sama** oleh trace terarah `OQ-PLT-008` |
| Mode | Read-only. Nol berkas source aplikasi diubah. |

**Batas audit.** Hanya kemampuan **alokasi nomor bisnis**, sesuai `DEC-PLT-001`. Nomor antrean
harian, rename entity, pola akses DbContext, dan pola hak akses **tidak** diaudit di sini.

Setiap baris memakai tepat satu status: `Ready to reuse`, `Reuse with adapter`, `Extend`,
`Repair`, `Missing`, `Conflict`, atau `Unknown`.

---

## 1. Ringkasan temuan

| Pertanyaan | Jawaban berbasis bukti |
| --- | --- |
| Apakah ada alokator nomor atomik bersama? | **Tidak ada.** Nol `Sequence`, nol `NEXTVAL`, nol provider bersama |
| Berapa titik pembangkitan yang ada? | **106 berkas**, sekitar 120 method berbeda |
| Di lapisan mana? | **95 di Controller**, 11 di Service |
| Berapa bentuk anti-pola yang berbeda? | **Tiga** — baca-semua lalu cari celah, `Count+1`, dan `Max/Last+1` |
| Apakah keunikannya dijamin database? | **Hampir menyeluruh.** Dari 122 deret yang ditulis pembangkit, **93 terlindungi index unik tunggal**, 10+ terlindungi index unik gabungan, dan **hanya 1 tanpa perlindungan** (`MstBank.BankCode`). Lihat bagian 6 |
| Apakah frontend ikut membangkitkan nomor? | **Tidak.** Frontend konsumen murni |
| Apakah ada test yang mengujinya? | **Nol** |

---

## 2. Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `PLT-CAP-001` | Alokasi nomor bisnis yang atomik dan tahan permintaan serentak | belum ada pemilik | Pencarian `Sequence`, `NEXTVAL`, `INumberSeriesService`, `NumberSeriesService` pada seluruh `Areas/`, `Repositories/`, `Services/` @`ba75a05` mengembalikan **nol hasil** | **`Missing`** | Seluruh kemampuan harus dibangun baru. `QBE-CODE-006` menuntut alokasi atomik ber-scope yang durabel beserta observability retry | **Tinggi.** Inilah yang memblokir `BE-BD-003` dan tiga modul lain |
| `PLT-CAP-002` | Pembangkit nomor per modul yang sedang berjalan | tersebar, tanpa pemilik tunggal | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#GenerateRunningCodeAsync@ba75a05` baris 2152; sekitar 120 method serupa pada 106 berkas | **`Repair`** | Ada dan berfungsi pada beban rendah, tetapi tidak aman saat serentak. Bukan kandidat pakai-ulang | **Tinggi.** Dua permintaan bersamaan dapat memperoleh nomor sama |
| `PLT-CAP-003` | Alokasi nomor berada di lapisan service, bukan controller | Platform backend | **95 dari 106** berkas menaruhnya di `Controllers/`; hanya 11 di `Services/` @`ba75a05` | **`Repair`** | `QBE-CODE-002` melarang controller mengalokasikan nomor bisnis. Perbaikannya menyertai migrasi `DEC-PLT-003` | **Sedang.** Menyulitkan pengujian dan pemakaian ulang |
| `PLT-CAP-004` | Penjaga keunikan terakhir di database | modul masing-masing | Silang-rujuk 122 deret pembangkit terhadap seluruh index unik pada `Repositories/Configurations/**@ba75a05` — lihat bagian 6 | **`Ready to reuse`** — semula `Reuse with adapter` pada revisi 1 | **93** terlindungi index unik tunggal, **10+** lewat index unik gabungan yang memang bercakupan, dan **1** tanpa perlindungan: `MstBank.BankCode` | **Rendah.** Seluruh deret kritis klinis — nomor rekam medis, nomor kunjungan, kode pasien — **terlindungi**. Satu-satunya celah ada pada master keuangan yang jarang berubah |
| `PLT-CAP-005` | Format, awalan, dan panjang nomor | modul masing-masing (`DEC-PLT-005`) | `EncounterCodePrefix = "ENC-RSMMC-"`, `GuarantorCodePrefix = "CG-RSMMC-"`; `CodeNumberLength` dideklarasikan terpisah pada **54 berkas** @`ba75a05` | **`Repair`** | Kode fasilitas `RSMMC` **ditanam di kode**, bukan konfigurasi (`OQ-PLT-006`). Panjang tidak seragam: mayoritas `5`, `LegalEntityController` `3` (`OQ-PLT-005`) | **Sedang.** Deret 3 digit habis setelah `999` |
| `PLT-CAP-006` | Konsumsi nomor bisnis di frontend | Frontend V2 | Pencarian `generateCode`, `generateNumber`, `padStart`, `RSMMC` pada `src/**@101ec5d3`: satu-satunya kemunculan adalah **string tampilan cadangan** `"KSK-RSMMC-00001"` pada dua modal kiosk | **`Ready to reuse`** | Nol adapter. Frontend hanya menampilkan nomor yang dikirim backend | **Rendah.** Migrasi backend tidak merusak frontend selama rupa nomor dipertahankan (`DEC-PLT-004`) |
| `PLT-CAP-007` | Bukti uji untuk pembangkitan nomor | belum ada pemilik | Pencarian `GenerateCodeAsync`, `GenerateRunningCodeAsync`, `Task.WhenAll`, `Parallel.For` pada `Tests/**@ba75a05` mengembalikan **nol hasil** | **`Missing`** | Tidak ada satu pun test yang membuktikan nomor tidak kembar, apalagi saat serentak | **Tinggi.** Perilaku yang paling berisiko justru yang paling tidak diuji |

---

## 3. Kontrak as-is — bagaimana nomor terbit hari ini

**Proses bisnisnya.** Petugas menekan Simpan pada pendaftaran pasien. Sebelum baris tersimpan,
controller memanggil method pembangkit nomor miliknya sendiri. Method itu membaca nomor yang sudah
ada di tabel, menghitung nomor berikutnya, lalu menempelkannya pada baris yang akan disimpan.

**Tiga bentuk yang ditemukan, dan bedanya nyata:**

| Bentuk | Jumlah kemunculan | Cara kerja | Kelemahannya |
| --- | ---: | --- | --- |
| Baca-semua lalu cari celah | `ToListAsync` **56** · `ToHashSet` **22** | Memuat **seluruh** nomor berawalan tertentu ke memori, menyusun himpunan, lalu memindai dari 1 mencari celah kosong pertama | Beban tumbuh seiring isi tabel. Mengisi celah berarti nomor **dipakai ulang** — bertentangan dengan `DEC-PLT-002` |
| `Count + 1` | **21** | Menghitung jumlah baris berawalan tertentu, lalu menambah satu | Paling rapuh. Begitu satu baris terhapus permanen, hitungan turun sementara nomor tertinggi tetap, sehingga nomor berikutnya **menabrak nomor yang sudah ada** |
| `Max/Last + 1` | **1** | Mengambil nomor tertinggi lalu menambah satu | Paling mendekati benar, tetapi tetap tanpa penguncian |

**Contoh nyata dari source** — `Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs#GenerateDoctorCodeAsync@ba75a05`:

```csharp
var existingCount = await _dbContext.Set<MstDoctor>()
    .IgnoreQueryFilters()
    .CountAsync(x => x.DoctorCode.StartsWith(prefix));

var nextNumber = existingCount + 1;
```

`QBE-CODE-003` melarang pola ini dengan menyebut namanya: *"MUST NOT / NEW CODE: memakai Count+1,
Max/Last+1 tanpa proteksi, counter statis/lokal, atau lock process-local sebagai satu-satunya
alokator."*

**Tidak ada endpoint yang menerbitkan nomor secara langsung.** Penomoran selalu menjadi efek
samping dari `POST /` pembuatan data, dan tidak ada permukaan API yang dapat dipanggil untuk
meminta nomor tanpa membuat data. Ini fakta as-is, bukan penilaian.

---

## 4. Ketidakcocokan dan risiko yang perlu keputusan manusia

| ID | Temuan | Kenapa penting |
| --- | --- | --- |
| `CONF-PLT-001` | Perilaku as-is **mengisi celah**, sehingga nomor dipakai ulang. Ini **bertentangan langsung** dengan `DEC-PLT-002` yang baru diputuskan | Setiap deret yang belum dimigrasikan masih melanggar invariant `INV-PLT-001` selama masa peralihan. Keadaan itu harus dinyatakan terbuka, bukan didiamkan |
| `RISK-PLT-001` | **Direvisi pada revisi 2 — lihat bagian 6.** Angka "277 dari 422" pada revisi 1 memakai pembagi yang keliru: ia menghitung seluruh index yang menyebut `Code`/`Number`, bukan deret yang benar-benar ditulis pembangkit nomor. Dengan pembagi yang benar, **hanya satu deret yang benar-benar tanpa perlindungan** | Mitigasi `BE-RWI-035` ternyata **berlaku jauh lebih luas** dari dugaan revisi 1 |
| `RISK-PLT-002` | Nol test menutupi pembangkitan nomor | Migrasi `DEC-PLT-003` akan menyentuh 106 berkas tanpa jaring pengaman apa pun |

---

## 6. Trace terarah — jawaban `OQ-PLT-008`

**Pertanyaan yang dijawab.** Deret mana yang **tidak** dilindungi index unik, sehingga urutan migrasi
`DEC-PLT-003` dapat disusun berdasarkan risiko nyata, bukan perkiraan.

**Cara menjawabnya.** Setiap method pembangkit nomor diurai untuk memperoleh pasangan
entity + kolom yang ditulisnya, lalu pasangan itu diadu dengan seluruh index unik pada
`Repositories/Configurations/**`. Tipe generik (`Set<TEntity>`) diselesaikan manual dari titik
pemanggilnya.

| Pemeriksaan | Hasil |
| --- | --- |
| Method pembangkit ditemukan | **122** |
| Terlindungi **index unik kolom tunggal** | **93** |
| Terlindungi **index unik gabungan** (unik dalam cakupan tertentu) | **10+** |
| **Benar-benar tanpa perlindungan** | **1** |

### Satu-satunya deret tanpa perlindungan

| Entity | Kolom | Pembangkit | Bukti |
| --- | --- | --- | --- |
| `MstBank` | `BankCode` | `Areas/Administrator/MasterData/Controllers/BankController.cs#GenerateBankCodeAsync@ba75a05` | **Nol berkas konfigurasi** untuk `MstBank` di seluruh `Repositories/`; hanya `DbSet<MstBank>` pada `ApplicationDbContext.cs:80`. Nol atribut `[Index]` pada `Areas/Administrator/MasterData/Models/MstBank.cs`. Nol unique constraint di mana pun |

### Deret kritis justru seluruhnya terlindungi

Ini temuan yang paling menenangkan, dan sengaja diperiksa satu per satu karena dampaknya klinis:

| Deret | Perlindungan |
| --- | --- |
| `MstPatient.MedicalRecordNumber` | **Index unik** — nomor rekam medis |
| `MstPatient.PatientCode` | **Index unik** |
| `TrxPatientEncounter.EncounterNumber` | **Index unik** — nomor kunjungan |
| `TrxPatientEncounterGuarantor.PaymentSourceNumber` | **Index unik** |
| `TrxKioskScanSession.SessionCode` | **Index unik** |

### Keunikan bercakupan — bukan cacat, melainkan rancangan

Sepuluh lebih deret memakai index unik gabungan, dan itu **memang disengaja**: kodenya unik di dalam
cakupannya, bukan di seluruh rumah sakit.

| Entity | Kolom | Cakupan keunikan |
| --- | --- | --- |
| `MstCostCenter` | `CostCenterCode` | per `LegalEntityId` |
| `MstHospitalSite` | `SiteCode` | per `LegalEntityId` |
| `MstOrganizationUnit` | `UnitCode` | per `LegalEntityId` |
| `MstPosition` | `PositionCode` | per `DepartmentId` |
| `MstWorkLocation` | `LocationCode` | per `HospitalSiteId` |
| `MstSpecialization` | `SpecializationCode` | per `ProfessionId`, disaring `IsDelete = false` |
| `MstProvince`, `MstCity`, `MstDistrict` | kode wilayah | per induk wilayahnya |

**Konsekuensi untuk urutan migrasi.** Risiko nyata jauh lebih terpusat daripada dugaan revisi 1.
`MstBank` adalah master keuangan yang jarang diubah dan berisi sedikit baris, sehingga peluang dua
petugas menambah bank pada saat yang sama sangat kecil. Artinya **tidak ada deret berisiko tinggi
yang tanpa perlindungan** — dan urutan migrasi sebaiknya ditentukan oleh **keramaian**, bukan oleh
ketiadaan index.

### Koreksi terhadap revisi 1

Revisi 1 menyatakan "277 dari 422 index bukan unik" dan menyimpulkan mitigasi `BE-RWI-035` hanya
berlaku sebagian. **Pembagi itu keliru.** Ia menghitung seluruh index yang kebetulan menyebut kata
`Code` atau `Number` — termasuk index pencarian biasa yang memang tidak perlu unik, dan termasuk
kolom yang tidak pernah disentuh pembangkit nomor. Setelah diadu terhadap deret yang benar-benar
ditulis pembangkit, hasilnya berbalik: perlindungan hampir menyeluruh.

---

## 5. Pertanyaan penutup

| ID | Pertanyaan | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `OQ-PLT-007` | Siapa **nama** pemegang peran pemilik kontrak engineering backend? | belum diketahui | `DESIGN` — seluruh keputusan masih `draft` |
| ~~`OQ-PLT-008`~~ | ~~Deret mana yang tidak punya index unik~~ | — | ✅ **Terjawab** pada bagian 6: hanya `MstBank.BankCode` |
| `OQ-PLT-009` | Untuk deret yang selama ini mengisi celah, apakah nomor kembar yang mungkin sudah terlanjur terbit perlu ditelusuri? | pemilik platform | `LATER SLICE` — menuntut akses data produksi |

**Batas audit ini.** Seluruh temuan berasal dari pembacaan source pada `ba75a05` dan `101ec5d3`.
Apakah nomor kembar benar-benar sudah pernah terjadi **tidak dapat dijawab dari source** — itu
menuntut pemeriksaan data produksi, dan dicatat sebagai `OQ-PLT-009`.
