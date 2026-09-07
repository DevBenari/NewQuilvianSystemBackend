# Platform — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `PLT-BP-001` |
| Blueprint revision | `1` |
| Capability map revision | `3` |
| Status | `source-audited` — audit source sudah dijalankan dan hasilnya berlaku; dokumen ini **tidak** menyatakan platform siap implementasi |
| Sumber keputusan | `00-interview-decisions.md` revisi 1 — `DEC-PLT-001`..`DEC-PLT-005` |
| Backend SHA audit penuh | `ba75a05` cabang `sukmagp` |
| Backend SHA audit terarah terakhir | **`4a1da7d`** cabang `sukmagp` — 7 September 2026, `PLT-CAP-001` dan `PLT-CAP-007` |
| Frontend SHA yang diaudit | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` — **tidak bergerak** |
| Tanggal audit | `2026-09-04` — audit penuh; **diperluas hari yang sama** oleh trace terarah `OQ-PLT-008`. **Dikoreksi 7 September 2026** oleh audit terarah `PLT-CAP-001`/`PLT-CAP-007` |
| Mode | Read-only. Nol berkas source aplikasi diubah. |

**Batas audit.** Hanya kemampuan **alokasi nomor bisnis**, sesuai `DEC-PLT-001`. Nomor antrean
harian, rename entity, pola akses DbContext, dan pola hak akses **tidak** diaudit di sini.

Setiap baris memakai tepat satu status: `Ready to reuse`, `Reuse with adapter`, `Extend`,
`Repair`, `Missing`, `Conflict`, atau `Unknown`.

---

## 1. Ringkasan temuan

| Pertanyaan | Jawaban berbasis bukti |
| --- | --- |
| Apakah ada alokator nomor atomik? | **Ada — dikoreksi 7 September 2026.** `BillingNumberSeriesService` sudah atomik, durabel, dan dipakai produksi. Yang **tidak** ada adalah alokator milik **platform**: mesin itu dimiliki Billing dan tanpa pintu masuk generik. Lihat bagian 7 |
| Apakah ada sequence database? | **Tidak ada.** Nol `Sequence`, nol `NEXTVAL`, nol `HasSequence` — diperiksa ulang di `4a1da7d`. Keatomikan dicapai lewat advisory lock, bukan sequence |
| Berapa titik pembangkitan yang ada? | **106 berkas**, sekitar 120 method berbeda |
| Di lapisan mana? | **95 di Controller**, 11 di Service |
| Berapa bentuk anti-pola yang berbeda? | **Tiga** — baca-semua lalu cari celah, `Count+1`, dan `Max/Last+1`. Ketiganya berlaku bagi 106 titik itu, **bukan** bagi `BillingNumberSeriesService` |
| Apakah keunikannya dijamin database? | **Hampir menyeluruh.** Dari 122 deret yang ditulis pembangkit, **93 terlindungi index unik tunggal**, 10+ terlindungi index unik gabungan, dan **hanya 1 tanpa perlindungan** (`MstBank.BankCode`). Lihat bagian 6 |
| Apakah frontend ikut membangkitkan nomor? | **Tidak.** Frontend konsumen murni |
| Apakah ada test yang mengujinya? | **Ada, tetapi tidak untuk yang berisiko — dikoreksi 7 September 2026.** Empat berkas test menyentuh alokator Billing; **nol** test membuktikan perilaku serentak. Lihat bagian 7 |

> ⚠️ **Koreksi bukti 7 September 2026.** Dua baris di atas dan dua baris peta (`PLT-CAP-001`,
> `PLT-CAP-007`) semula menyatakan hasil pencarian **nol**. Pernyataan itu **keliru terhadap SHA yang
> diaudit revisi 2 sendiri**. Rinciannya, beserta cara memeriksanya ulang, ada di bagian 7.

---

## 2. Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `PLT-CAP-001` | Alokasi nomor bisnis yang atomik dan tahan permintaan serentak | Billing (de facto); pemilik platform **belum ditunjuk** | `Areas/HealthServices/BillingManagement/Billing/Services/BillingNumberSeriesService.cs#AllocateNumberAsync@4a1da7d` baris 141–205 — kunci `pg_advisory_xact_lock` (176), transaksi **wajib** (172–173), baca satu baris (179), deret hanya maju (188/197). Persistensi `Models/BilNumberSeries.cs@4a1da7d`, index unik `(SequenceKey, ScopeKey)` pada `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilNumberSeriesConfiguration.cs@4a1da7d`, tabel dibuat `Migrations/20260820090951_AddRunningInvoiceAndIdempotentCharge.cs:45–72,222–226@4a1da7d`, terdaftar `Repositories/ApplicationDbContext.cs:549@4a1da7d`, DI `BillingManagementServiceCollectionExtensions.cs:26@4a1da7d`. **Nol** `Sequence`/`NEXTVAL`/`HasSequence` di seluruh repository | **`Extend`** — semula `Missing`, berpindah 7 September 2026 | Mesinnya **sudah ada dan terbukti**; yang belum ada adalah **pintu masuk generik dan kepemilikan platform**. `AllocateNumberAsync` sudah generik parameternya tetapi `private`, dan keempat method publiknya dipatok deret `BILLING_*`. Yang perlu ditambahkan: entry point generik, kepemilikan, dan pendaftaran DI di luar Billing | **Sedang** — turun dari `Tinggi`. Yang menahan `BE-BD-003` kini **keputusan kepemilikan**, bukan ketiadaan teknologi |
| `PLT-CAP-002` | Pembangkit nomor per modul yang sedang berjalan | tersebar, tanpa pemilik tunggal | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#GenerateRunningCodeAsync@ba75a05` baris 2152; sekitar 120 method serupa pada 106 berkas | **`Repair`** | Ada dan berfungsi pada beban rendah, tetapi tidak aman saat serentak. Bukan kandidat pakai-ulang | **Tinggi.** Dua permintaan bersamaan dapat memperoleh nomor sama |
| `PLT-CAP-003` | Alokasi nomor berada di lapisan service, bukan controller | Platform backend | **95 dari 106** berkas menaruhnya di `Controllers/`; hanya 11 di `Services/` @`ba75a05` | **`Repair`** | `QBE-CODE-002` melarang controller mengalokasikan nomor bisnis. Perbaikannya menyertai migrasi `DEC-PLT-003` | **Sedang.** Menyulitkan pengujian dan pemakaian ulang |
| `PLT-CAP-004` | Penjaga keunikan terakhir di database | modul masing-masing | Silang-rujuk 122 deret pembangkit terhadap seluruh index unik pada `Repositories/Configurations/**@ba75a05` — lihat bagian 6 | **`Ready to reuse`** — semula `Reuse with adapter` pada revisi 1 | **93** terlindungi index unik tunggal, **10+** lewat index unik gabungan yang memang bercakupan, dan **1** tanpa perlindungan: `MstBank.BankCode` | **Rendah.** Seluruh deret kritis klinis — nomor rekam medis, nomor kunjungan, kode pasien — **terlindungi**. Satu-satunya celah ada pada master keuangan yang jarang berubah |
| `PLT-CAP-005` | Format, awalan, dan panjang nomor | modul masing-masing (`DEC-PLT-005`) | `EncounterCodePrefix = "ENC-RSMMC-"`, `GuarantorCodePrefix = "CG-RSMMC-"`; `CodeNumberLength` dideklarasikan terpisah pada **54 berkas** @`ba75a05` | **`Repair`** | Kode fasilitas `RSMMC` **ditanam di kode**, bukan konfigurasi (`OQ-PLT-006`). Panjang tidak seragam: mayoritas `5`, `LegalEntityController` `3` (`OQ-PLT-005`) | **Sedang.** Deret 3 digit habis setelah `999` |
| `PLT-CAP-006` | Konsumsi nomor bisnis di frontend | Frontend V2 | Pencarian `generateCode`, `generateNumber`, `padStart`, `RSMMC` pada `src/**@101ec5d3`: satu-satunya kemunculan adalah **string tampilan cadangan** `"KSK-RSMMC-00001"` pada dua modal kiosk | **`Ready to reuse`** | Nol adapter. Frontend hanya menampilkan nomor yang dikirim backend | **Rendah.** Migrasi backend tidak merusak frontend selama rupa nomor dipertahankan (`DEC-PLT-004`) |
| `PLT-CAP-007` | Bukti uji untuk pembangkitan nomor | Billing (untuk alokatornya); belum ada pemilik untuk 106 titik lain | **Test ada:** `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/{BillingDeposit,BillingInvoice,BillingSettlement,CashierShift}ServiceTests.cs@4a1da7d` — 6 rujukan; `BillingSettlementServiceTests.cs:557` bahkan memeriksa hanya satu baris deret terbentuk. **Tetapi tidak menjangkau risikonya:** `Tests/.../IsolatedBillingDbContextFactory.cs:11@4a1da7d` memakai `UseInMemoryDatabase`, sehingga `IsRelational()` (baris 170) bernilai `false` dan advisory lock beserta syarat transaksi **tidak pernah dieksekusi**. Pencarian `Task.WhenAll`/`Parallel.*`/`ConcurrentBag`/`pg_advisory` pada seluruh `Tests/**@4a1da7d` → **0**; project `IntegrationTests.Postgres` (`UseNpgsql`) **nol** menyentuh NumberSeries | **`Repair`** — semula `Missing`, berpindah 7 September 2026 | Test membuktikan **aritmetika pencacah**, bukan **penguncian**. `AC-PLT-001` (N permintaan paralel → tepat N nomor unik) **belum punya bukti apa pun**. Perbaikannya: test konkurensi di atas Postgres nyata — wadahnya sudah tersedia | **Tinggi** — tetap. Jalur yang paling berisiko justru satu-satunya yang tidak pernah dieksekusi pengujian |

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
| `RISK-PLT-002` | **Direvisi pada revisi 3 — lihat bagian 7.** Bukan "nol test" melainkan **test yang tidak menjangkau risikonya**. Alokator Billing punya empat berkas test, tetapi seluruhnya berjalan di atas `UseInMemoryDatabase`, sehingga jalur kunci dan syarat transaksi **tidak pernah dieksekusi**. Untuk 106 titik pembangkit lainnya, "nol test" tetap benar | Migrasi `DEC-PLT-003` akan menyentuh 106 berkas tanpa jaring pengaman. Bahkan mesin yang paling matang pun belum punya bukti uji konkurensi |

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

## 7. Audit terarah `PLT-CAP-001` dan `PLT-CAP-007` — 7 September 2026

**Pemicu.** Pemeriksaan silang menemukan bahwa klaim bukti kedua baris itu tidak cocok dengan isi
repository. Audit terarah dijalankan untuk memastikannya, dan hasilnya memindahkan dua status.

**Batas audit.** Hanya `PLT-CAP-001` dan `PLT-CAP-007`. Kelima baris lain **tidak** dinilai ulang.

### 7.1 Kekeliruan yang dikoreksi

Revisi 2 menyatakan pencarian `NumberSeriesService` mengembalikan **nol hasil** pada `ba75a05`.
Diperiksa ulang dengan perintah yang sama, pada SHA yang sama:

```
git grep -l "NumberSeriesService" ba75a05 -- Areas/ Repositories/ Services/   →  6 berkas
```

Alokator itu **sudah ada** ketika audit penuh ditulis. Yang benar-benar nol adalah
`Sequence`/`NEXTVAL`/`HasSequence` — dan itu tetap benar sampai `4a1da7d`. Kekeliruannya adalah
menyamakan "tidak ada sequence database" dengan "tidak ada alokasi atomik". Keduanya berbeda:
keatomikan di sini dicapai lewat kunci penasihat PostgreSQL, bukan lewat sequence.

### 7.2 Bagaimana mesin yang sudah ada bekerja

**Prosesnya, dalam bahasa sehari-hari.** Sebelum sebuah nomor diambil, sistem memasang "palang"
atas nama deret yang diminta. Selama palang terpasang, permintaan lain untuk deret **yang sama**
menunggu. Sistem lalu membaca satu baris pencacah, menaikkannya satu, dan memulangkan nomornya.
Palang terlepas sendiri ketika transaksi selesai.

| Sifat | Bukti | Hasil |
| --- | --- | --- |
| Transaksi **wajib** | baris 172–173 — melempar `InvalidOperationException` bila caller lupa | Gagal keras di awal, bukan diam-diam menerbitkan nomor tanpa perlindungan |
| Kunci atomik | baris 176 — `pg_advisory_xact_lock(hashtext(lockKey))`, `lockKey = "BIL_NUMBER_{sequenceKey}_{scopeKey}"` | Satu palang per deret per cakupan; deret berbeda tidak saling menunggu |
| Nol pemuatan ke memori | baris 179 — `SingleOrDefaultAsync` | Memenuhi `AC-PLT-003` |
| Deret hanya maju | baris 188 `CurrentValue = 1`, baris 197 `checked { CurrentValue++ }` | Nol pengisian celah; memenuhi `INV-PLT-001` dan `INV-PLT-002` |
| Jaring terakhir | index unik `(SequenceKey, ScopeKey)` | Andai palang gagal, database menolak baris kedua |

**Tidak ada balapan saat baris pencacah pertama kali dibuat.** Ini pertanyaan paling tajam, dan
jawabannya melegakan: palang diturunkan dari **nama deret**, bukan dari ada-tidaknya baris. Jadi
palang sudah terpasang sebelum sistem memeriksa keberadaan barisnya.

> **Contoh.** Petugas A dan B sama-sama menerbitkan invoice pertama hari itu, bersamaan. A masuk
> lebih dulu, memegang palang `BIL_NUMBER_BILLING_INVOICE_20260907`, melihat baris belum ada,
> membuatnya dengan nilai `1`, lalu menyimpan. B **menunggu** sampai A selesai, baru membaca — dan
> melihat baris yang sudah ada, sehingga naik ke `2`. Tidak pernah ada dua nomor `1`.

**Pola ini konvensi rumah, bukan gaya lokal Billing.** `pg_advisory_xact_lock` dipakai pada
**24 lokasi** lintas HR Overtime, Billing, Clinical, Registration, dan System (`grep@4a1da7d`).

### 7.3 Kenapa statusnya `Extend`, bukan yang lain

| Status | Kenapa tidak dipilih |
| --- | --- |
| `Missing` | Keliru. Mesinnya ada, jalan, termigrasi, dan dipakai empat service |
| `Ready to reuse` | Keliru. Nol pintu masuk generik — keempat method publiknya dipatok deret `BILLING_*` |
| `Reuse with adapter` | Tidak cukup. `AllocateNumberAsync` bersifat `private`; adapter tak dapat menjangkaunya tanpa mengubah service |
| **`Extend`** | **Tepat.** Kemampuannya ada dan terbukti; yang kurang pintu masuk generik, kepemilikan, dan pendaftaran di luar Billing |

### 7.4 Seberapa terikat mesin itu pada Billing

| Lapisan | Billing-specific? | Dapat dipisahkan? |
| --- | --- | --- |
| Mesin alokasi `AllocateNumberAsync` | **Tidak** — parameternya sudah generik | ✅ Bersih |
| Options `Billing:*` dan awalan | **Ya** | Tinggal di Billing |
| Tipe exception | **Tidak** — disuntikkan pemanggil sebagai `Func<string, Exception>` | ✅ Sudah terpisah |
| Sequence key `BILLING_*` | **Ya**, dipatok `const` | Tinggal di Billing |
| Reset policy | **Tidak** — konsep umum | ✅ Dapat ikut |
| Nama tabel `BilNumberSeries` | **Ya secara penamaan**, netral secara isi | Menuntut keputusan kepemilikan |
| Pendaftaran DI `AddBillingManagement()` | **Ya** | Menuntut pendaftaran baru |

**Kesimpulannya:** mesin dapat dipisahkan **tanpa membawa satu pun aturan bisnis Billing**. Yang
Billing-specific seluruhnya duduk di **tepi**, bukan di dalam algoritmanya. Penghalang sebenarnya
bukan keterikatan logika, melainkan **kepemilikan**.

### 7.5 Kesesuaian terhadap keputusan dan kriteria Platform

| Butir | Putusan |
| --- | --- |
| `INV-PLT-001` satu nomor satu catatan | ✅ Terpenuhi |
| `INV-PLT-002` celah itu sah | ✅ Terpenuhi |
| `INV-PLT-004` deret tak pernah diulang | ⚠️ Terpenuhi **bila** `ResetPolicy = NEVER` |
| `AC-PLT-001` N paralel → N nomor unik | ⚠️ Mekanisme benar, **nol bukti uji** |
| `AC-PLT-002` nomor catatan batal tak terbit lagi | ✅ Terpenuhi — pembatalan catatan nol menurunkan pencacah |
| `AC-PLT-003` tak memuat semua ke memori | ✅ Terpenuhi |
| `AC-PLT-005` tak balik ke awal saat tahun berganti | ⚠️ Terpenuhi bila `NEVER` |
| `INV-PLT-003`, `AC-PLT-004` satu deret satu mekanisme | ⚪ Tata kelola migrasi, di luar jangkauan bukti source |

**Reset policy bukan pertentangan arsitektur.** `DEC-PLT-004` menetapkan deret berjalan terus.
Mesin memenuhi itu persis ketika `ResetPolicy = NEVER`: cakupannya menjadi tetap `GLOBAL` sehingga
satu baris melayani selamanya. Keberadaan pilihan `YEARLY`/`MONTHLY`/`DAILY` tidak memaksa siapa pun
memakainya. Satu hal yang perlu diketahui pemilik: `NEVER` menghasilkan bentuk `PREFIX-00001`,
sedangkan kebijakan lain menyisipkan cakupan menjadi `PREFIX-scope-00001`.

**Kepemilikan format tetap di modul, sejalan `DEC-PLT-005`.** `Prefix` dan `SequenceDigits` sudah
berupa konfigurasi milik modul, bukan angka di dalam mesin. Satu batas nyata: `SequenceDigits`
divalidasi **4–12**, sehingga panjang 3 yang disebut `OQ-PLT-005` **ditolak** mesin ini.

### 7.6 Kedalaman bukti persistensi

Tiga hal ini sengaja dibedakan supaya tidak dibaca berlebihan:

| Tingkat | Putusan |
| --- | --- |
| Source model ada | ✅ **Terbukti** — model, konfigurasi, `DbSet`, `ApplyConfigurationsFromAssembly` |
| Migration ada | ✅ **Terbukti** — `CreateTable("BilNumberSeries")` + `CreateIndex(unique: true)`. Ditemukan lewat **nama tabel**; nama migrationnya sendiri tidak menyebut NumberSeries |
| Database sudah diterapkan | ❌ **Tidak dapat diklaim** — menuntut bukti runtime yang tidak tersedia dalam audit read-only |

### 7.7 Satu perilaku yang menuntut keputusan manusia

Bila transaksi bisnis gagal **setelah** nomor dialokasikan, kenaikan pencacah ikut dibatalkan,
sehingga nomor itu **diterbitkan lagi** pada percobaan berikutnya. Apakah ini melanggar Platform?
Dipisahkan supaya tidak salah baca:

| Sumber | Terhadap perilaku ini |
| --- | --- |
| `INV-PLT-001` | **Tidak dilanggar** — transaksi batal berarti nol catatan tersimpan, jadi nol nomor menunjuk dua catatan |
| `DEC-PLT-002` teks aslinya | **Tidak dilanggar** — ia bicara tentang catatan yang **dibatalkan atau dihapus**, bukan transaksi yang gagal |
| Penilaian kelengkapan dimensi 05 | **Bertentangan** — di sana disimpulkan nomor "hangus". Dimensi itu sendiri berstatus `MISSING` |

Jadi ini **bukan pelanggaran terbukti**, melainkan wilayah yang memang **belum diputuskan**.

> ✅ **Diputuskan closure pass 7 September 2026 — `DEC-PLT-008`: nomor dianggap hangus.** Pemilik
> kebutuhan memilih perilaku yang **berbeda** dari mesin yang ada. Akibatnya lahir `CONF-PLT-002`:
> ekstraksi `DEC-PLT-007` **bukan pemindahan apa adanya**, karena mesin bersama harus mengubah cara
> pencacahnya disimpan supaya kenaikan bertahan walau transaksi bisnis dibatalkan. Cara mewujudkannya
> adalah pekerjaan desain, bukan audit. Dampaknya terhadap empat deret Billing yang sudah produksi
> disimpan sebagai catatan desain tertunda `NOTE-PLT-001` — **bukan** Open Question dan **bukan**
> blocker.

### 7.8 Opsi arsitektur — sudah dipilih pemilik pada 7 September 2026

Opsi di bawah disajikan audit tanpa memilih. **Pemilik kebutuhan memilih opsi ketiga** pada closure
pass 7 September 2026, direkam sebagai `DEC-PLT-007` berstatus `draft`.

| Opsi | Didukung bukti? | Konsekuensi | Pilihan pemilik |
| --- | --- | --- | --- |
| Reuse langsung | ❌ Tidak | Nol pintu masuk generik | — |
| Adapter di atas yang ada | ❌ Tidak | Method generiknya `private` | — |
| **Ekstrak mesin bersama** | ✅ Paling didukung | Algoritma sudah generik; menuntut keputusan kepemilikan tabel dan DI baru | ✅ **DIPILIH** — `DEC-PLT-007` |
| Repair/refactor di tempat | ✅ Layak | Lebih murah, tetapi modul lain bergantung pada Billing — bertabrakan dengan semangat `DEC-PLT-005` | — |
| Bangun baru | ⚠️ Mungkin, tetapi mengulang | Menulis ulang pola yang sudah terbukti di 24 lokasi. Sah bila pemilik memutuskan tabel `Bil*` tak boleh dipindah | — |

**Satu penyesuaian penting atas pilihan itu.** Ketika opsi ini ditulis, ia diasumsikan sebagai
pemindahan yang mempertahankan perilaku. `DEC-PLT-008` mengubah asumsi itu: mesin bersama harus
**hangus saat rollback**, sedangkan mesin sekarang memakai ulang. Jadi `PLT-SLICE-01` memuat dua
pekerjaan, bukan satu — memindahkan kepemilikan **dan** mengubah cara pencacah disimpan. Lihat
`CONF-PLT-002`.

---

## 5. Pertanyaan penutup

| ID | Pertanyaan | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `OQ-PLT-007` | Siapa **nama** pemegang peran pemilik kontrak engineering backend? | belum diketahui | `DESIGN` — seluruh keputusan masih `draft` |
| ~~`OQ-PLT-008`~~ | ~~Deret mana yang tidak punya index unik~~ | — | ✅ **Terjawab** pada bagian 6: hanya `MstBank.BankCode` |
| `OQ-PLT-009` | Untuk deret yang selama ini mengisi celah, apakah nomor kembar yang mungkin sudah terlanjur terbit perlu ditelusuri? | pemilik platform | `LATER SLICE` — menuntut akses data produksi |
| ~~`OQ-PLT-010`~~ | ~~Bolehkah mesin `BillingNumberSeriesService` dinaikkan menjadi milik platform?~~ | — | ✅ **Terjawab** closure pass 7 September 2026 → `DEC-PLT-007`: **ekstrak mesin bersama**, konfigurasi Billing tetap milik Billing. Ini memilih **opsi ketiga** pada bagian 7.8. Keputusannya `draft` menunggu `OQ-PLT-007` |
| ~~`OQ-PLT-011`~~ | ~~Nomor hangus atau dipakai ulang bila transaksi dibatalkan?~~ | — | ✅ **Terjawab** closure pass 7 September 2026 → `DEC-PLT-008`: **hangus**. Keputusannya `draft` menunggu `OQ-PLT-007` |

**Nol Open Question baru dari closure pass 7 September 2026.** Pass itu dikunci pada `OQ-PLT-007`,
`OQ-PLT-010`, dan `OQ-PLT-011` saja. Pengamatan tentang dampak `DEC-PLT-008` terhadap empat deret
Billing yang sudah produksi disimpan sebagai **catatan desain tertunda `NOTE-PLT-001`** pada
`00-interview-decisions.md` — bukan Open Question, bukan blocker, dan tidak menuntut keputusan
pemilik Billing pada pass ini.

**Batas audit ini.** Temuan revisi 1 dan 2 berasal dari pembacaan source pada `ba75a05` dan
`101ec5d3`; koreksi revisi 3 dari pembacaan pada `4a1da7d`. Apakah nomor kembar benar-benar sudah
pernah terjadi **tidak dapat dijawab dari source** — itu menuntut pemeriksaan data produksi, dan
dicatat sebagai `OQ-PLT-009`. Apakah tabel `BilNumberSeries` sudah benar-benar terpasang di database
mana pun juga **tidak dapat dijawab dari source**.

## Pemicu peta menjadi usang

Peta ini terikat pada backend `4a1da7d` dan frontend `101ec5d3`. Bila salah satu berubah, tandai
peta `STALE` lalu jalankan pemindaian dampak terbatas pada berkas berikut sebelum dipakai lagi:

`BillingNumberSeriesService.cs` · `BilNumberSeries.cs` · `BilNumberSeriesConfiguration.cs` ·
`ApplicationDbContext.cs` · `BillingManagementServiceCollectionExtensions.cs` ·
`Migrations/20260820090951_AddRunningInvoiceAndIdempotentCharge.cs` ·
`ApplicationDbContextModelSnapshot.cs` · keempat berkas test Billing pada
`Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/` ·
`IsolatedBillingDbContextFactory.cs` · `PatientEncounterController.cs` · `PatientController.cs` ·
`KioskScanSessionController.cs` · `LegalEntityController.cs`.
