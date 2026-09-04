# Platform — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `PLT-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Product/domain owner | belum ditetapkan — lihat `OQ-PLT-001` |
| Backend SHA | `ba75a05` cabang `sukmagp` |
| Frontend SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` |
| Mode pass | **Scope pass** — belum ada blueprint `platform` sebelumnya |
| Capability map | revisi `2` — `02-existing-capability-map.md`, audit penuh `ba75a05` + trace terarah `OQ-PLT-008` |
| Completeness assessment | revisi `2` — `BUSINESS_DECISION_REQUIRED` pada keempat slice |
| Pemicu | Blocker 2 pada `BE-BD-003` (Bank Darah `BD-BP-001` rev 24) |
| Tanggal | `2026-09-04` |

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
| `INV-PLT-001` | Satu nomor bisnis yang sudah terbit **tidak pernah** menunjuk lebih dari satu catatan sepanjang hidup sistem | `DEC-PLT-002` | `draft` |
| `INV-PLT-002` | Nomor yang bolong dalam deret adalah keadaan sah, bukan cacat data, dan **tidak boleh** "dirapikan" dengan mengisi celahnya | `DEC-PLT-002` | `draft` |
| `INV-PLT-003` | Selama masa peralihan, satu deret nomor hanya boleh dilayani **satu** mekanisme — yang lama **atau** yang baru, tidak pernah keduanya bersamaan | `DEC-PLT-003` | `draft` |
| `INV-PLT-004` | Sebuah deret nomor **tidak pernah** diulang dari awal, baik karena pergantian tahun maupun pergantian fasilitas | `DEC-PLT-004` | `draft` |

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
| `DEC-PLT-005` | `Decision` | **Kewenangan dibagi dua.** Pemilik kontrak engineering backend menyetujui mesin alokasi beserta invariant lintas modul (`INV-PLT-001`..`004`). Pemilik modul masing-masing menetapkan awalan dan format deretnya sendiri. Pembagian ini mengikuti kontrak yang sudah berlaku: `QBE-CODE-005` menaruh format/prefix/reset/scope di tangan modul, `QBE-CODE-006` menaruh alokasi atomik di provider bersama | pemilik platform | `draft` | belum — **peran** sudah jelas, **orangnya** belum ditunjuk | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-001` sebagian |
| `OQ-PLT-007` | `Open Question` | Siapa **nama** pemegang peran "pemilik kontrak engineering backend" yang berwenang menurunkan approval? | belum diketahui | `draft` | — | `DEC-PLT-005` menetapkan perannya, bukan orangnya. **Memblokir approval seluruh keputusan pass ini** |
| `DEC-PLT-002` | `Decision` | **Nomor bisnis tidak pernah dipakai ulang.** Sekali terbit, satu nomor menjadi milik satu catatan selamanya — walau catatannya dibatalkan atau dihapus. Nomor yang bolong adalah keadaan normal dan bukan cacat | pemilik platform | `draft` | belum — menunggu `OQ-PLT-001` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-002` |
| `DEC-PLT-003` | `Decision` | **Alokator baru dipakai kode baru sejak hari pertama, dan titik lama dipindahkan bertahap menurut risiko.** Urutan migrasi ditentukan keramaian dan peluang tabrakan: pendaftaran pasien dan nomor kunjungan lebih dulu, master data yang jarang berubah paling akhir. Dua mekanisme hidup berdampingan selama masa peralihan, dan keadaan itu **dinyatakan terbuka**, bukan didiamkan | pemilik platform | `draft` | belum — menunggu `OQ-PLT-001` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-003` |
| `DEC-PLT-004` | `Decision` | **Deret berjalan terus dan tidak pernah diulang.** Tidak ada pengulangan per tahun maupun per fasilitas. Alasan utamanya menjaga rupa nomor tetap sama selama migrasi bertahap `DEC-PLT-003` | pemilik platform | `draft` | belum — menunggu `OQ-PLT-001` | Jawaban pengguna 4 September 2026. Menutup `OQ-PLT-004` |
| `OQ-PLT-005` | `Open Question` | Panjang nomor perlu ditinjau: mayoritas 5 digit, `LegalEntity` masih 3 digit sehingga habis di `999`. Berapa panjang yang ditetapkan, dan bagaimana nasib deret yang sudah mendekati batas? | pemilik platform | `draft` | — | Turunan `DEC-PLT-004` + `FACT-PLT-009`. Memblokir `IMPLEMENTATION`, bukan `DESIGN` |
| `OQ-PLT-006` | `Open Question` | Kode fasilitas `RSMMC` ditanam di dalam awalan. Dijadikan konfigurasi, atau dibiarkan sampai fasilitas kedua benar-benar ada? | pemilik platform | `draft` | — | Turunan `FACT-PLT-010`. Memblokir `LATER SLICE` |

---

## Open Questions dan Blocker

| ID | Pertanyaan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| ~~`OQ-PLT-001`~~ | ~~Pemilik keputusan platform~~ | ✅ **Tertutup sebagian** `DEC-PLT-005` — peran jelas | — |
| `OQ-PLT-007` | **Nama** pemegang peran pemilik kontrak engineering backend | `DESIGN` — seluruh keputusan tertahan `draft` | belum diketahui |
| ~~`OQ-PLT-002`~~ | ~~Boleh tidaknya nomor dipakai ulang~~ | ✅ **Tertutup** `DEC-PLT-002` | — |
| ~~`OQ-PLT-003`~~ | ~~Cakupan penggantian~~ | ✅ **Tertutup** `DEC-PLT-003` | — |
| ~~`OQ-PLT-004`~~ | ~~Pengulangan deret~~ | ✅ **Tertutup** `DEC-PLT-004` | — |
| `OQ-PLT-005` | Panjang nomor dan nasib deret yang hampir habis | `IMPLEMENTATION` | pemilik platform |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | `LATER SLICE` | pemilik platform |
| ~~`OQ-PLT-008`~~ | ~~Deret mana yang tanpa index unik~~ | ✅ **Tertutup** 4 Sep 2026 — hanya `MstBank.BankCode` | — |
| `OQ-PLT-009` | Perlukah menelusuri nomor kembar yang mungkin sudah terlanjur terbit | `LATER SLICE` — menuntut data produksi | pemilik platform |
| `DEC-PLT-006` | Apakah pelanggaran `INV-PLT-001` selama masa peralihan diterima resmi, dan sampai kapan | `PLT-SLICE-02` | pemilik platform |

## Conflict yang ditemukan audit

| ID | Conflict | Keterangan |
| --- | --- | --- |
| `CONF-PLT-001` | Perilaku as-is **mengisi celah nomor**, sehingga nomor dipakai ulang — **bertentangan langsung** dengan `DEC-PLT-002` | Bukan alasan membatalkan keputusan. Artinya: setiap deret yang belum dimigrasikan masih melanggar `INV-PLT-001` selama masa peralihan `DEC-PLT-003`, dan keadaan itu wajib dinyatakan terbuka |

**Catatan wewenang.** Pass ini `draft`. Peran pemilik sudah jelas lewat `DEC-PLT-005`, tetapi
**orangnya belum ditunjuk** (`OQ-PLT-007`). Sampai itu terjadi, `DEC-PLT-002` sampai `DEC-PLT-005`
tetap `draft` dan tidak boleh diperlakukan sebagai approval.

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
