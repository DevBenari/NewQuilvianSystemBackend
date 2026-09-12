# Platform — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `PLT-BP-001` |
| Revision | `4` — amendment pass: Area dan prefix registry ditetapkan, 9 September 2026. Revisi 3 memuat penunjukan pemilik dan turunnya approval |
| Status | ✅ **`approved`** — `OQ-PLT-007` tertutup (**`Andry`**), lalu `DEC-PLT-002`..`005`, `DEC-PLT-007`, `DEC-PLT-008` diturunkan approval-nya berikut `INV-PLT-001`..`004`. Amendment pass 9 September 2026 menambah `DEC-PLT-009` (Area `Platform`) dan `DEC-PLT-010` (prefix `Num`), menutup `OQ-PLT-012` dan `OQ-PLT-013`. **Yang masih terbuka:** `OQ-PLT-014` (baris registry belum dicatat — menahan implementasi, bukan perencanaan), `DEC-PLT-006`, `OQ-PLT-005`, `OQ-PLT-006`, `OQ-PLT-009` |
| Product/domain owner | belum ditetapkan — lihat `OQ-PLT-001` |
| Pemilik kontrak engineering backend | **`Andry`** — ditunjuk **eksplisit** oleh pemilik kebutuhan pada 9 September 2026, menutup `OQ-PLT-007`. Penunjukan ini **membalikkan secara sadar** pernyataan 7 September 2026 yang menyatakan belum ada penunjukan; pernyataan lama **tidak dihapus**, lihat riwayatnya pada baris `OQ-PLT-007`. **Approval diturunkan hari yang sama** atas `DEC-PLT-002`..`005`, `DEC-PLT-007`, `DEC-PLT-008` |
| Backend SHA | **`4a1da7d`** cabang `sukmagp` — semula `ba75a05`; disegarkan 7 September 2026 |
| Frontend SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` — **tidak bergerak** |
| Mode pass | **Scope pass** 4 September 2026 — belum ada blueprint `platform` sebelumnya. **Closure pass** 7 September 2026 — terbatas pada `PLT-SLICE-01`; `PLT-SLICE-02`/`03`/`04` sengaja tidak dibahas |
| Capability map | revisi **`3`** — `02-existing-capability-map.md`, audit penuh `ba75a05` + trace terarah `OQ-PLT-008` + **audit terarah `PLT-CAP-001`/`PLT-CAP-007` di `4a1da7d`** |
| Completeness assessment | revisi `2` — `BUSINESS_DECISION_REQUIRED` pada keempat slice; **tidak berubah** oleh audit 7 September 2026 |
| Pemicu | Blocker 2 pada `BE-BD-003` (Bank Darah `BD-BP-001` rev 24) |
| Tanggal | `2026-09-04`; bukti disegarkan `2026-09-07`; pemilik ditunjuk `2026-09-09` |

> **Penyegaran bukti 7 September 2026 — nol keputusan bisnis berubah.** Audit terarah memindahkan dua
> baris peta kemampuan (`PLT-CAP-001` → `Extend`, `PLT-CAP-007` → `Repair`) dan menambah dua
> pertanyaan terbuka baru, `OQ-PLT-010` dan `OQ-PLT-011`. **`DEC-PLT-001` sampai `DEC-PLT-005` tidak
> disentuh** dan seluruhnya tetap `draft` menunggu `OQ-PLT-007`. `INV-PLT-001`..`004` dan
> `AC-PLT-001`..`006` juga tidak diubah — yang bertambah hanya catatan sejauh mana masing-masing
> sudah terbukti oleh mesin yang ternyata sudah ada. Lihat peta kemampuan bagian 7.

---

## Scope dan Outcome

**Satu kalimat batas scope.** Blueprint ini hanya membahas **cara sistem menerbitkan nomor
bisnis** — nomor kunjungan, nomor order, nomor permintaan, dan sejenisnya — sebagai kemampuan
bersama milik platform, bukan milik satu modul.

Batas ini dikunci pengguna pada 4 September 2026 dan **belum pernah diperluas**.

### Di dalam scope

| Butir | Alasan masuk |
| --- | --- |
| Aturan penerbitan nomor bisnis: format, awalan, panjang, dan kapan penomoran diulang dari awal | Menentukan bentuk nomor yang dipakai seluruh modul |
| Apakah sebuah nomor boleh dipakai ulang | Invariant lintas modul yang berdampak audit |
| Cakupan penomoran: satu deret untuk seluruh rumah sakit, per unit, per tahun, atau per fasilitas | Menentukan bentuk alokatornya |
| Perilaku ketika dua petugas menerbitkan nomor pada saat bersamaan | Perilaku kegagalan yang harus ditetapkan manusia |
| Perilaku ketika penyimpanan gagal setelah nomor terlanjur diambil | Menentukan apakah nomor hangus atau kembali |
| Siapa yang berwenang menetapkan format nomor sebuah modul | Ownership |
| Nasib mekanisme yang sedang berjalan (`GenerateRunningCodeAsync`) dan nomor yang sudah terbit | Menentukan apakah ini penggantian atau pendampingan |

### Di luar scope — untuk modul lain

| Butir | Pemilik | Alasan dikeluarkan |
| --- | --- | --- |
| Aturan bisnis internal modul yang memakai nomor | Modul masing-masing | Blueprint ini hanya menyediakan nomornya, bukan mengatur maknanya |
| Nomor antrean harian (`GenerateQueueNumberAsync`) | RegistrationManagement | Konsep berbeda: per hari per unit, diulang tiap hari, dan bukan identitas dokumen |
| Rename entity `Trx*` menjadi `Reg*` | Pemilik kontrak engineering backend | `LEGACY MIGRATION` yang menuntut kampanye tersendiri (`BE-RWI-035` temuan #1) |
| Controller mengakses `ApplicationDbContext` langsung (`QBE-SVC-001`) | Pemilik kontrak engineering backend | Dikeluarkan pengguna dari scope pass ini |
| Pola hak akses dan `AccessAction` | Pemilik keamanan platform | Dikeluarkan pengguna dari scope pass ini |

---

## Business Rules dan Invariants

| ID | Invariant | Asal | Status |
| --- | --- | --- | --- |
| `INV-PLT-001` | Satu nomor bisnis yang sudah terbit **tidak pernah** menunjuk lebih dari satu catatan sepanjang hidup sistem | `DEC-PLT-002` | ✅ `approved` |
| `INV-PLT-002` | Nomor yang bolong dalam deret adalah keadaan sah, bukan cacat data, dan **tidak boleh** "dirapikan" dengan mengisi celahnya | `DEC-PLT-002` | ✅ `approved` |
| `INV-PLT-003` | Selama masa peralihan, satu deret nomor hanya boleh dilayani **satu** mekanisme — yang lama **atau** yang baru, tidak pernah keduanya bersamaan | `DEC-PLT-003` | ✅ `approved` |
| `INV-PLT-004` | Sebuah deret nomor **tidak pernah** diulang dari awal, baik karena pergantian tahun maupun pergantian fasilitas | `DEC-PLT-004` | ✅ `approved` |

**Contoh penerapan.** Order darah `BD-000042` diterbitkan untuk Ny. R, lalu order itu dibatalkan
petugas BDRS. Nomor `BD-000042` **tetap** menjadi milik catatan pembatalan itu. Order berikutnya
terbit sebagai `BD-000043`, bukan mengisi ulang `BD-000042`. Dengan begitu surat ke PMI, tagihan,
dan rekam medis yang pernah menyebut `BD-000042` selamanya menunjuk peristiwa yang sama.

**Akibat teknis yang menguntungkan.** Justru pengisian celah itulah yang memaksa mekanisme sekarang
memuat seluruh nomor ke memori. Deret yang hanya maju cukup satu operasi alokasi di database.

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `FACT-PLT-001` | `Fact` | Mekanisme penomoran yang berjalan sekarang adalah `GenerateRunningCodeAsync<TEntity>`: memuat **seluruh** kode berawalan tertentu ke memori, menyusun `HashSet`, lalu memindai dari 1 mencari celah pertama yang kosong | — | — | — | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#GenerateRunningCodeAsync@ba75a05` baris 2152 |
| `FACT-PLT-002` | `Fact` | Mekanisme itu **tidak atomik**: nol lock, nol transaksi, nol sequence database. Dua permintaan serentak dapat memperoleh nomor yang sama | — | — | — | Pembacaan langsung baris 2152-2161 |
| `FACT-PLT-003` | `Fact` | Mekanisme itu **digandakan pada tiga controller**, bukan komponen bersama | — | — | — | `PatientController.cs:2447`, `KioskScanSessionController.cs:1118`, `PatientEncounterController.cs:2152` @`ba75a05` |
| `FACT-PLT-004` | `Fact` | Alokasi nomor berada di **controller**, melanggar `QBE-CODE-002`; polanya melanggar `QBE-CODE-003` | — | — | — | `BACKEND_ENGINEERING_CONTRACT.md` |
| `FACT-PLT-005` | `Fact` | Yang menahan akibat terburuk hanya index unik, sehingga tabrakan muncul sebagai kegagalan `500`, bukan nomor kembar tersimpan | — | — | — | `BE-RWI-035` temuan #3 |
| `FACT-PLT-006` | `Fact` | Empat blueprint sudah merencanakan pemakaian `number-series`: `bank-darah`, `billing-kasir`, `rawat-inap`, `rawat-jalan` | — | — | — | Pencarian pada `docs/module-blueprints/**` @`ba75a05` |
| `FACT-PLT-007` | `Fact` | Nomor yang sudah terbit dilindungi index unik pada kolomnya masing-masing | — | — | — | `BE-RWI-035` temuan #3 |
| `FACT-PLT-008` | `Fact` | Pola pembangkitan kode **jauh lebih luas dari tiga controller**: ditemukan **±120 method pembangkit** berbeda, di antaranya **47 bernama `GenerateCodeAsync`**, tersebar pada **54 berkas** yang masing-masing menyimpan konstanta `CodeNumberLength` sendiri. Ini salinan berulang, bukan komponen bersama | — | — | — | Hitungan `grep` pada `Areas/**` @`ba75a05` |
| `FACT-PLT-009` | `Fact` | Panjang nomor **tidak seragam**: mayoritas `5`, tetapi `LegalEntityController` memakai `3` — deretnya habis setelah `999` | — | — | — | `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/LegalEntityController.cs:34@ba75a05` |
| `FACT-PLT-010` | `Fact` | Kode fasilitas **ditanam di dalam awalan**: `ENC-RSMMC-`, `CG-RSMMC-`, `PC-RSMMC-`. Penambahan fasilitas kedua menuntut perubahan kode, bukan konfigurasi | — | — | — | Konstanta awalan pada `PatientEncounterController.cs@ba75a05` |
| `FACT-PLT-011` | `Fact` | Nomor yang terbit sekarang **tidak memuat tahun**; deretnya naik terus tanpa pernah diulang | — | — | — | `prefix + nextNumber.ToString().PadLeft(...)` |
| `DEC-PLT-001` | `Decision` | Scope pass ini dikunci hanya pada alokasi nomor bisnis; kapabilitas platform lain dikeluarkan | pengguna | `approved` | pengguna / `2026-09-04` | Jawaban batas scope 4 September 2026 |
| `DEC-PLT-005` | `Decision` | **Kewenangan dibagi dua.** Pemilik kontrak engineering backend menyetujui mesin alokasi beserta invariant lintas modul (`INV-PLT-001`..`004`). Pemilik modul masing-masing menetapkan awalan dan format deretnya sendiri. Pembagian ini mengikuti kontrak yang sudah berlaku: `QBE-CODE-005` menaruh format/prefix/reset/scope di tangan modul, `QBE-CODE-006` menaruh alokasi atomik di provider bersama | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-001` sebagian |
| `OQ-PLT-007` | `Open Question` | Siapa **nama** pemegang peran "pemilik kontrak engineering backend" yang berwenang menurunkan approval? | **authority engineering/architecture** — dipertegas 7 September 2026 | `OPEN` | — | `DEC-PLT-005` menetapkan perannya, bukan orangnya. **Memblokir approval seluruh keputusan pass ini.** **Dipertegas 7 September 2026:** pemilik kebutuhan menyatakan peran ini **belum ditetapkan**, dan menolak penunjukan berbasis jejak commit. `Sukma Giri Pratama` (`sukmagp`) dinyatakan sebagai **module owner / pattern implementer**, **bukan** pemilik kontrak engineering backend — walaupun ia penulis terbanyak dokumen engineering canonical (8 dari 16 perubahan) dan penurun approval kontrak `v4` Bank Darah. Penunjukannya **wajib eksplisit** dari authority engineering/architecture, bukan disimpulkan dari aktivitas repository |
| `DEC-PLT-007` | `Decision` | **Mesin alokasi nomor bersama diekstrak dari mekanisme `BillingNumberSeriesService` yang sudah ada, bukan dibangun dari nol.** Konfigurasi khusus Billing — awalan, jumlah digit, kebijakan pengulangan, dan kunci deret `BILLING_*` — **tetap dimiliki Billing**. Yang berpindah ke platform hanya mesin alokasinya beserta invariant lintas modul | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 7 September 2026. Menutup `OQ-PLT-010`. Bukti pendukung: audit terarah `PLT-CAP-001` (`Extend`) pada `4a1da7d` membuktikan mesinnya dapat dipisahkan tanpa membawa satu pun aturan bisnis Billing — yang Billing-specific seluruhnya duduk di tepi. Pola `pg_advisory_xact_lock` juga sudah terpakai di 24 lokasi lintas modul |
| `DEC-PLT-008` | `Decision` | **Nomor yang sudah dialokasikan dianggap hangus bila transaksi bisnisnya dibatalkan.** Nomor itu **tidak** diterbitkan lagi walaupun tidak ada satu pun catatan yang menempel padanya, sehingga deret berlubang. Celah semacam itu adalah keadaan sah menurut `INV-PLT-002` | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 7 September 2026. Menutup `OQ-PLT-011`. **Berbeda dari perilaku mesin yang ada sekarang** — lihat `CONF-PLT-002` |
| `DEC-PLT-009` | `Decision` | **Modul penomoran bersama tinggal di Area `Platform` yang baru**, bukan dititipkan ke salah satu dari empat Area yang sudah ada. Module/pemilik: `NumberSeriesManagement / Number Series`; Category: `SHARED PLATFORM CAPABILITY` | pemilik kontrak engineering backend | ✅ `approved` | **`Andry`** / `2026-09-09` | **Amendment pass 9 September 2026.** Menutup `OQ-PLT-012`. Alasan: keempat Area yang ada (`Administrator`, `Corporate`, `HealthServices`, `SelfServices`) seluruhnya domain bisnis atau audiens pengguna, sedangkan alokator ini melayani `HealthServices` dan `Corporate` sekaligus. Preseden `Wfl` yang berkategori `SHARED PLATFORM CAPABILITY` tetapi duduk di `Corporate/HumanResource` **sengaja tidak ditiru** |
| `FACT-PLT-013` | `Fact` | **Bentuk tulis `Category` pada registry menyimpang dari bunyi `DEC-PLT-009`, dan itu kebutuhan mekanis — bukan perubahan keputusan.** Baris registry ditulis `BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY`, bukan `SHARED PLATFORM CAPABILITY` saja. Sebabnya `Invoke-QbeConformanceCheck.ps1` baris 271 menguji `(ConvertTo-SemanticToken $row.Category) -match '^businessdomain'`. Disimulasikan langsung: `SHARED PLATFORM CAPABILITY` → token `sharedplatformcapability` → **ditolak**; dengan awalan → `businessdomainsharedplatformcapability` → **diterima**. Tanpa awalan itu, `NumNumberSeries` akan terblokir `QBE-MOD-002` walaupun barisnya sudah ada. Polanya mengikuti perbaikan `Mst` 2026-09-04 yang menghadapi penghalang identik. **Label semantik, prefix, pemilik, dan scope tidak berubah** | — | — | — | Verifikasi langsung terhadap checker, 9 September 2026 |
| `FACT-PLT-014` | `Fact` | **Lifecycle ditulis langsung `ACTIVE`, melewati `PLANNED`.** Diminta eksplisit pemilik, dan sejalan dengan checker: token lifecycle yang diterima hanya `active` dan `legacy`; `PLANNED` disimulasikan dan **ditolak**. Konsekuensinya wewenang penamaan dan implementasi entity `Num*` terbuka bersamaan — berbeda dari `Bbk` yang sempat singgah di `PLANNED` selama beberapa jam | — | — | — | Verifikasi langsung terhadap checker, 9 September 2026 |
| `DEC-PLT-010` | `Decision` | **Prefix entity modul ini `Num`**, kepanjangan *Number Series*. Entity pertamanya `NumNumberSeries` | pemilik kontrak engineering backend | ✅ `approved` | **`Andry`** / `2026-09-09` | **Amendment pass 9 September 2026.** Menutup `OQ-PLT-013`. Alasan: prefix menamai **konsep pemilik**, sesuai pola `Wfl` = *Workflow*. Belum dipakai baris registry mana pun. `Plt` ditolak karena menamai Area sehingga kemampuan platform berikutnya akan berbagi prefix dan isi tabel berhenti terbaca dari namanya; `Seq` ditolak karena bertabrakan makna dengan objek `SEQUENCE` PostgreSQL yang justru **ditolak** desain `PLT-SLICE-01` |
| `DEC-PLT-002` | `Decision` | **Nomor bisnis tidak pernah dipakai ulang.** Sekali terbit, satu nomor menjadi milik satu catatan selamanya — walau catatannya dibatalkan atau dihapus. Nomor yang bolong adalah keadaan normal dan bukan cacat | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-002` |
| `DEC-PLT-003` | `Decision` | **Alokator baru dipakai kode baru sejak hari pertama, dan titik lama dipindahkan bertahap menurut risiko.** Urutan migrasi ditentukan keramaian dan peluang tabrakan: pendaftaran pasien dan nomor kunjungan lebih dulu, master data yang jarang berubah paling akhir. Dua mekanisme hidup berdampingan selama masa peralihan, dan keadaan itu **dinyatakan terbuka**, bukan didiamkan | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-003` |
| `DEC-PLT-004` | `Decision` | **Deret berjalan terus dan tidak pernah diulang.** Tidak ada pengulangan per tahun maupun per fasilitas. Alasan utamanya menjaga rupa nomor tetap sama selama migrasi bertahap `DEC-PLT-003` | pemilik platform | ✅ `approved` | **`Andry`** / `2026-09-09` — pemilik kontrak engineering backend, ditunjuk hari yang sama lewat penutupan `OQ-PLT-007` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-004` |
| `OQ-PLT-005` | `Open Question` | Panjang nomor perlu ditinjau: mayoritas 5 digit, `LegalEntity` masih 3 digit sehingga habis di `999`. Berapa panjang yang ditetapkan, dan bagaimana nasib deret yang sudah mendekati batas? | pemilik platform | `draft` | — | Turunan `DEC-PLT-004` + `FACT-PLT-009`. Memblokir `IMPLEMENTATION`, bukan `DESIGN` |
| `OQ-PLT-006` | `Open Question` | Kode fasilitas `RSMMC` ditanam di dalam awalan. Dijadikan konfigurasi, atau dibiarkan sampai fasilitas kedua benar-benar ada? | pemilik platform | `draft` | — | Turunan `FACT-PLT-010`. Memblokir `LATER SLICE` |

---

## Open Questions dan Blocker

| ID | Pertanyaan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| ~~`OQ-PLT-001`~~ | ~~Pemilik keputusan platform~~ | ✅ **Tertutup sebagian** `DEC-PLT-005` — peran jelas | — |
| ~~`OQ-PLT-007`~~ | ~~**Nama** pemegang peran pemilik kontrak engineering backend~~ ✅ **TERTUTUP 9 September 2026 — `Andry`.** Ditunjuk **eksplisit** oleh pemilik kebutuhan, persis bentuk yang dituntut baris ini. **Riwayatnya dipertahankan apa adanya:** pada 7 September 2026 pertanyaan yang sama dijawab `BELUM ADA`, dan penunjukan berbasis jejak commit **ditolak** — `Sukma Giri Pratama` (`sukmagp`) dinyatakan module owner / pattern implementer, bukan pemegang peran ini. Penutupan 9 September 2026 **tidak** membatalkan penolakan itu; ia menambahkan penunjukan eksplisit yang sebelumnya memang belum ada | ✅ Tidak lagi memblokir. **Tetapi penunjukan bukan approval** — `DEC-PLT-002`..`008` tetap `draft` sampai `Andry` menurunkan approval-nya | — |
| ~~`OQ-PLT-002`~~ | ~~Boleh tidaknya nomor dipakai ulang~~ | ✅ **Tertutup** `DEC-PLT-002` | — |
| ~~`OQ-PLT-003`~~ | ~~Cakupan penggantian~~ | ✅ **Tertutup** `DEC-PLT-003` | — |
| ~~`OQ-PLT-004`~~ | ~~Pengulangan deret~~ | ✅ **Tertutup** `DEC-PLT-004` | — |
| `OQ-PLT-005` | Panjang nomor dan nasib deret yang hampir habis | `IMPLEMENTATION` | pemilik platform |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | `LATER SLICE` | pemilik platform |
| ~~`OQ-PLT-008`~~ | ~~Deret mana yang tanpa index unik~~ | ✅ **Tertutup** 4 Sep 2026 — hanya `MstBank.BankCode` | — |
| `OQ-PLT-009` | Perlukah menelusuri nomor kembar yang mungkin sudah terlanjur terbit | `LATER SLICE` — menuntut data produksi | pemilik platform |
| `DEC-PLT-006` | Apakah pelanggaran `INV-PLT-001` selama masa peralihan diterima resmi, dan sampai kapan | `PLT-SLICE-02` | pemilik platform |
| ~~`OQ-PLT-010`~~ | ~~Bolehkah mesin `BillingNumberSeriesService` dinaikkan menjadi milik platform?~~ | ✅ **Terjawab** 7 September 2026 → `DEC-PLT-007` (**ekstrak mesin bersama**, konfigurasi Billing tetap milik Billing). Keputusannya masih `draft` menunggu `OQ-PLT-007` | — |
| ~~`OQ-PLT-011`~~ | ~~Nomor hangus atau dipakai ulang bila transaksi dibatalkan?~~ | ✅ **Terjawab** 7 September 2026 → `DEC-PLT-008` (**hangus**). Keputusannya masih `draft` menunggu `OQ-PLT-007`. Menutup dimensi 05 penilaian kelengkapan yang `MISSING`, dan melahirkan `CONF-PLT-002` | — |
| — | *Tidak ada Open Question baru dari closure pass 7 September 2026.* Scope pass itu dikunci pada `OQ-PLT-007`, `OQ-PLT-010`, dan `OQ-PLT-011` saja. Observasi mengenai dampak `DEC-PLT-008` terhadap Billing disimpan sebagai **catatan desain tertunda** pada `NOTE-PLT-001` di bawah, **bukan** sebagai Open Question dan **bukan** blocker | — | — |
| ~~`OQ-PLT-012`~~ | ~~Area registry modul Platform~~ ✅ **Tertutup 9 September 2026** → `DEC-PLT-009`: **Area `Platform` baru**, Module `NumberSeriesManagement / Number Series`, Category `SHARED PLATFORM CAPABILITY` | Tidak lagi memblokir | — |
| ~~`OQ-PLT-013`~~ | ~~Prefix entity modul Platform~~ ✅ **Tertutup 9 September 2026** → `DEC-PLT-010`: prefix **`Num`** = *Number Series* | Tidak lagi memblokir | — |
| ~~`OQ-PLT-014`~~ | ~~Baris registry hasil `DEC-PLT-009`/`DEC-PLT-010` belum dicatat~~ ✅ **TERTUTUP 9 September 2026.** Baris `Platform \| NumberSeriesManagement / Number Series \| BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY \| Num \| ACTIVE` dicatat pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, lengkap dengan kepanjangan prefix dan catatan perubahan lifecycle. **Terbukti diterima checker**, bukan diyakini: `Resolve-RegistryOwnership` atas `Areas/Platform/NumberSeriesManagement/Models/NumNumberSeries.cs` memulangkan `Resolved: True` | Tidak lagi memblokir. Gerbang `P1` roadmap Platform ✅ tertutup | — |
| `FACT-PLT-012` | **Temuan saat pendaftaran registry 9 September 2026 — dilaporkan, bukan didiamkan.** Registry yang **benar-benar ditegakkan** ada di `NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, bukan di suite skill: `Invoke-QbeConformanceCheck.ps1` baris 200 membacanya dari sana. Salinan di suite skill adalah dokumen tata kelola yang dirujuk `AGENTS.md`, dan salinan pada plugin cache terpasang **tertinggal satu entri** — ia belum memuat perbaikan `Mst` 2026-09-04. Ketiganya perlu disinkronkan pemilik registry | Tidak memblokir `PLT-SLICE-01` | Pemilik registry engineering |

## Conflict yang ditemukan audit

| ID | Conflict | Keterangan |
| --- | --- | --- |
| `CONF-PLT-001` | Perilaku as-is **mengisi celah nomor**, sehingga nomor dipakai ulang — **bertentangan langsung** dengan `DEC-PLT-002` | Bukan alasan membatalkan keputusan. Artinya: setiap deret yang belum dimigrasikan masih melanggar `INV-PLT-001` selama masa peralihan `DEC-PLT-003`, dan keadaan itu wajib dinyatakan terbuka |
| `CONF-PLT-002` | **Baru 7 September 2026.** `DEC-PLT-008` menuntut nomor **hangus** saat transaksi dibatalkan. Mesin `BillingNumberSeriesService` yang ada sekarang melakukan **kebalikannya**: kenaikan pencacah ikut dibatalkan bersama transaksi, sehingga nomor **dipakai ulang** pada percobaan berikutnya (`AllocateNumberAsync@4a1da7d` baris 197, di dalam transaksi caller) | **Bukan blocker.** Ini selisih perilaku yang tercatat supaya tidak hilang, bukan pertentangan yang menahan approval. Konsekuensinya satu: ekstraksi `DEC-PLT-007` **bukan pemindahan apa adanya** — mesin bersama harus mengubah cara pencacahnya disimpan supaya kenaikan bertahan walau transaksi bisnis batal. Cara mewujudkannya adalah **pekerjaan desain**, diserahkan ke `design-business-module`. **Satu-satunya blocker approval `PLT-SLICE-01` tetap `OQ-PLT-007`** |

**Catatan wewenang — diperbarui 9 September 2026.** Peran pemilik sudah jelas lewat `DEC-PLT-005`,
dan **orangnya kini sudah ditunjuk**: `Andry` (`OQ-PLT-007` tertutup). Yang berubah hanya satu hal —
sekarang ada orang yang berwenang menurunkan approval.

**Approval turun pada hari yang sama.** `Andry` menyetujui `DEC-PLT-002`, `DEC-PLT-003`,
`DEC-PLT-004`, `DEC-PLT-005`, `DEC-PLT-007`, dan `DEC-PLT-008` pada 9 September 2026, sehingga
`INV-PLT-001` sampai `INV-PLT-004` ikut naik dari `draft`.

Penunjukan dan approval tetap dicatat sebagai **dua peristiwa terpisah** yang kebetulan jatuh di hari
yang sama — bukan satu. Penunjukan menjawab *siapa yang boleh menyetujui*; approval menjawab *sudah
disetujui*. Menggabungkannya akan menghapus jejak bahwa selama 4–8 September 2026 keputusan-keputusan
ini memang tidak punya pemilik yang berwenang mengesahkannya.

**Yang tetap terbuka, dan bukan bagian approval ini:** `DEC-PLT-006` (diterima-tidaknya pelanggaran
`INV-PLT-001` selama masa peralihan — milik `PLT-SLICE-02`), `OQ-PLT-005` (panjang nomor),
`OQ-PLT-006` (kode fasilitas di dalam awalan), dan `OQ-PLT-009` (penelusuran nomor kembar yang
mungkin sudah terbit). Keempatnya **tidak** menahan `PLT-SLICE-01`.

---

## Catatan desain tertunda

Bagian ini menyimpan pengamatan berbasis bukti yang **belum** menjadi pertanyaan dan **belum** menjadi
keputusan. Isinya **tidak memblokir apa pun** dan **tidak menuntut jawaban siapa pun pada pass ini**.
Gunanya satu: supaya temuan yang mahal ditemukan tidak hilang sebelum desain dimulai.

| ID | Catatan | Untuk siapa | Kapan dipakai |
| --- | --- | --- | --- |
| `NOTE-PLT-001` | `DEC-PLT-008` menetapkan nomor **hangus** saat transaksi dibatalkan, sedangkan mesin yang menjadi fondasi teknis (`BillingNumberSeriesService`) saat ini **memakai ulang** nomor tersebut — lihat `CONF-PLT-002`. Empat deret Billing yang sudah jalan produksi memakai mesin itu: invoice, deposit, shift kasir, dan kwitansi. Karena itu, seberapa jauh perubahan perilaku menyentuh keempat deret tersebut **perlu dinilai saat desain**. Pengamatan ini murni dari bukti source pada `4a1da7d`; ia **tidak** mengandung usulan, **tidak** menuntut keputusan pemilik Billing, dan **tidak** menahan approval `PLT-SLICE-01` | `design-business-module` saat menyusun `PLT-SLICE-01` | Setelah `OQ-PLT-007` tertutup dan `DEC-PLT-002`..`008` naik dari `draft` |

**Kenapa dicatat sebagai catatan, bukan sebagai Open Question.** Closure pass 7 September 2026 dikunci
pada tiga pertanyaan saja: `OQ-PLT-007`, `OQ-PLT-010`, dan `OQ-PLT-011`. Menaikkan pengamatan ini
menjadi Open Question keempat berarti memperluas scope pass tanpa persetujuan pemilik kebutuhan, dan
menambah blocker yang tidak diminta. Bila kelak pengamatan ini memang perlu diputuskan manusia, ia
diangkat menjadi Open Question bernomor pada pass tersendiri — bukan di sini.

---

## Acceptance Criteria

Kriteria di bawah ditulis supaya dapat diuji, bukan dinilai dari kesan.

| ID | Kriteria | Cara mengujinya | Asal |
| --- | --- | --- | --- |
| `AC-PLT-001` | Dua permintaan nomor pada deret yang sama, dikirim bersamaan, menghasilkan **dua nomor berbeda** | Uji konkurensi: N permintaan paralel pada satu deret; hitung nomor unik yang terbit harus tepat N | `FACT-PLT-002` |
| `AC-PLT-002` | Nomor milik catatan yang dibatalkan atau dihapus **tidak pernah** terbit lagi | Terbitkan nomor, batalkan catatannya, terbitkan lagi; nomor baru harus lebih besar, bukan mengisi celah | `INV-PLT-001` |
| `AC-PLT-003` | Alokasi **tidak** memuat seluruh nomor yang sudah ada ke memori | Ukur jumlah baris yang dibaca saat alokasi; harus tetap sama ketika isi tabel bertambah besar | `FACT-PLT-001` |
| `AC-PLT-004` | Satu deret dilayani **tepat satu** mekanisme selama masa peralihan | Telusuri tiap deret yang sudah dimigrasikan; tidak boleh ada jalur lama yang masih menerbitkan deret itu | `INV-PLT-003` |
| `AC-PLT-005` | Deret **tidak** kembali ke awal saat tahun berganti | Terbitkan nomor sebelum dan sesudah pergantian tahun; nomor kedua harus lebih besar | `INV-PLT-004` |
| `AC-PLT-006` | Nomor yang sudah terbit sebelum migrasi **tetap sah dan tidak berubah bentuk** | Bandingkan nomor lama sebelum dan sesudah modulnya dimigrasikan | `DEC-PLT-003`, `DEC-PLT-004` |
