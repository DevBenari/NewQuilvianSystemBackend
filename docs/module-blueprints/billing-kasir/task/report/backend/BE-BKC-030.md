# Laporan Perubahan Backend — `BE-BKC-030`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-030` — Perluasan perutean ke jalur `NotCovered` (revisi `0.9`) |
| `TASK TYPE` | Implementasi backend (satu cabang lagi ditambahkan ke akumulator yang sudah ada) |
| `COMPLEXITY` | `MEDIUM` — mesin perhitungan uang disentuh (skor ≥ 1), tidak ada kontrak API bertambah field (revisi `0.9` nol field baru), tidak ada migration |
| `CLASSIFICATION SCORE` | 2 |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `MODULE BLUEPRINT MODE` (persetujuan desain + penulisan kontrak) diikuti `BACKEND MODE` (implementasi source), berurutan dalam satu alur kerja |
| `WRITE TARGET` | `docs/module-blueprints/billing-kasir/**` (kontrak/roadmap) lalu `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` |
| Gelombang | Revisi `0.9`, sesudah `MVP-11` |
| Trace | `BKC-DEC-089` (menutup `BKC-OQ-093`); `BKC-DES-026`, `BKC-DES-027` |
| Blueprint | `BIL-CASH-001` revisi `0.9` — **disetujui 5 September 2026 pada task ini** |
| Kontrak berlaku | `BIL-API-0.8`, `BIL-TEST-0.8`, `BIL-CALCULATION-0.8` — ditulis dan `approved` pada task ini |
| Dependency | `BE-BKC-029` (source selesai); `BKC-GATE-06` (ditutup pada task ini) |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` — satu cabang ditambahkan pada akumulator yang sudah ada (`nonBillableResidual`, diperkenalkan `BE-BKC-028`). Bukan `Trx*` legacy, bukan `LEGACY MIGRATION`, **tidak ada migration** (nol perubahan skema, dikonfirmasi eksplisit oleh dokumen arsitektur) |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis — perluasan cakupan `BIL-VAL-043` yang sudah ada, tidak ada aturan baru) |
| QBE ID yang **tidak** berlaku | `QBE-API-001`/`QBE-DTO-001` (nol field kontrak baru — dikonfirmasi "Hanya satu class yang berubah" oleh dokumen), `QBE-ENT-*`/`QBE-CFG-*`/`QBE-DB-*` (nol entity/kolom/migration), `QBE-MOD-002/003`, `QBE-NAM-*` |

Selisih governance sama seperti `BE-BKC-028`/`029`; tidak ada selisih baru.

---

## 0. Bagian gerbang: penutupan `BKC-GATE-06` (`MODULE BLUEPRINT MODE`)

Sebelum implementasi source, task ini menutup tiga penyebab `BKC-GATE-06` yang sebelumnya
memblokir `BE-BKC-030`, atas instruksi eksplisit pengguna ("Saya approve untuk case diatas",
menjawab pertanyaan pengguna tentang kenapa `BE-BKC-030` masih `BLOCKED`) dan pilihan eksplisit
untuk menulis kontrak yang hilang lebih dulu, baru lanjut implementasi:

| Penyebab | Tindakan | Bukti |
| --- | --- | --- |
| `BKC-DES-026`/`027` berstatus `draft` | Disetujui Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`), 5 September 2026 | `02-backend-architecture.md` § amendment revisi `0.9`, tiga baris status diperbarui |
| Kontrak `BIL-API-0.8`/`BIL-TEST-0.8` belum ditulis | Ditulis — amendment baru pada `contracts/api-contract.md` (field `coverage.unresolvedAmount` yang maknanya bergeser lagi, "menyisakan nol jalur") dan `testing/acceptance-test-matrix.md` (`BIL-AT-062`–`063`, regresi baru, bukti keluar) | Kedua berkas, § "Amendment lanjutan ... revisi `0.9`" |
| `BIL-AT-062`/`063` belum ada isinya | Ditulis lengkap dengan skenario, jenis test, dan bukti yang diharapkan | `testing/acceptance-test-matrix.md` baris `BIL-AT-062`/`063` |

**Berkas blueprint lain yang turut diperbarui** (konsistensi, bukan keputusan baru):
`data/data-dictionary.md` (keterangan peran `UnresolvedCoverageAmount` diperbarui + koreksi status
`draft` yang tertinggal dari penguncian kontrak 4 September), `blueprint-manifest.md`
(`contract_versions` api/testing naik ke `0.8`, `contract_lock_note`, artifact register, approval
note `BKC-DES-026`–`027`), `roadmap/backend-roadmap.md` dan `roadmap/requirement-traceability.md`
(gerbang `BKC-GATE-06` ditandai tertutup).

**Temuan didokumentasikan, tidak diperbaiki di sini:** `contracts/api-contract.md` memiliki empat
baris (kode `404`/`403` dan satu baris `Trace`/`Tests`) yang tampak berada di ujung berkas tanpa
header/konteks amendment yang jelas — kemungkinan artefak edit sebelumnya. Dicatat sebagai
ketidaksesuaian dokumen apa adanya pada berkas itu sendiri; **tidak dirapikan** supaya tidak
bercampur dengan perubahan berbasis `BKC-DEC-089`. `blueprint-manifest.md.artifact_hashes` (SHA256
per berkas) **tidak dihitung ulang** — empat berkas berubah isinya pada task ini, tetapi
perhitungan hash didelegasikan ke pemeliharaan manifest tersendiri (`/manage-module-blueprint`),
bukan ditulis tangan berisiko salah oleh task ad-hoc ini; ditandai `STALE` secara eksplisit pada
`artifact_hashes_note`.

## 1. Masalah yang diselesaikan

`BE-BKC-028` merutekan **satu** jalur (jalur 5, residual perhitungan tanggungan) ke write-off.
Jalur (2) — aturan yang secara eksplisit menyatakan `CoverageStatus = NotCovered` **dan**
`IsAllowExcessPaymentByPatient = false` — sengaja ditinggalkan di jalur lama (`unresolved`),
diajukan sebagai `BKC-OQ-093`. Pemilik memutuskan (`BKC-DEC-089`) kedua jalur sama-sama berarti
"penjamin tidak membayar, DAN kontrak yang sama melarang menagihkannya ke pasien" — nasib uangnya
identik, sehingga keduanya wajib berjalan di jalur penanggungan yang sama.

## 2. Proses bisnis

Tidak ada proses bisnis baru — task ini murni memperluas SATU cabang lagi pada mesin kalkulasi
yang sudah ada. Tidak ada endpoint, DTO, atau alur kerja yang berubah bentuknya.

**Aturan bisnis.** Titik tangkap `BKC-DES-022` diperlebar syaratnya (bukan dipindah, bukan
digandakan): akumulator `nonBillableResidual` yang sama kini menerima **dua** cabang di dalam
`ResolveAsync`: (a) jalur (2), ketika `rule.CoverageStatus == "NotCovered"` dan
`rule.IsAllowExcessPaymentByPatient == false` — seluruh `component.Amount` masuk
`nonBillableResidual`; (b) jalur (5), tidak berubah dari `BE-BKC-028`. **Satu akumulator, dua
cabang** — bukan dua akumulator, bukan kategori write-off ketiga (`BKC-DEC-089` secara eksplisit
menolak membedakan perlakuan keduanya).

**Konsekuensi yang harus dinyatakan terbuka.** Sesudah amendment ini, **tidak ada satu jalur pun**
di `ResolveAsync` yang mengisi `unresolved` — nilainya selalu `0` pada setiap versi kalkulasi baru
(`BKC-DES-027`). Field `UnresolvedAmount`/kolom `UnresolvedCoverageAmount` **tetap ada, tidak
dihapus, tidak diganti nama** — versi kalkulasi lama tetap memuat angka lamanya sebagai bukti
perhitungan yang sudah terjadi.

**Hasil akhir.** Yang dibayar pasien tidak bergeser satu rupiah pun. Nasib nominal jalur (2)
berubah: dari angka bernama tanpa tindak lanjut (revisi `0.8`), menjadi selisih yang punya jalur
penanggungan lewat mekanisme write-off yang sama dengan jalur (5) (`BE-BKC-029`, tidak perlu
diubah satu baris pun).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` § amendment revisi `0.9` | Spesifikasi persis titik tangkap, bukti as-is (posisi baris jalur (2)/(5) dalam satu `foreach` yang sama), tabel keputusan yang tidak berubah |
| `Areas/.../Billing/Services/BillingCoverageAdapter.cs` | Lokasi satu-satunya perubahan — dikonfirmasi dokumen: "Hanya satu class yang berubah pada amendment ini" |
| `Tests/.../BillingCalculationServiceTests.cs` | Pola test integrasi `RegistrationBillingCoverageAdapter` yang sudah dipakai `BE-BKC-024`/`025`/`028`; test `NotCovered`+`true` existing (`RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion`) dikonfirmasi tidak tersentuh perubahan ini (cabang `true` tidak disentuh) |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingCoverageAdapter.cs` | Cabang `NotCovered` di `ResolveAsync`: `unresolved += notCoveredUnresolved` → `nonBillableResidual += notCoveredNonBillable` (akumulator yang SAMA dengan jalur (5)); outcome komponen jalur ini kini `UnresolvedAmount=0`, `NonBillableResidualAmount=component.Amount`. Komentar pada deklarasi `unresolved`/`nonBillableResidual` dan pada jalur (1) diperbarui untuk menyatakan konsekuensi `BKC-DES-027` secara eksplisit |
| `Tests/.../BillingCalculationServiceTests.cs` | Dua test baru: `RegistrationCoverageAdapterNotCoveredRuleWithExcessDisallowedBecomesNonBillableResidual` (`BIL-AT-062`) dan `RegistrationCoverageAdapterCombinesNotCoveredAndResidualIntoSingleNonBillableAmount` (`BIL-AT-063`, dua item satu tagihan, jalur (2)+(5) sekaligus) |

Total: **2 berkas source/test berubah**. Nol DTO, nol controller, nol entity, nol migration —
persis seperti yang dinyatakan dokumen arsitektur revisi `0.9`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nol field baru.** Satu field **maknanya** bergeser lagi tanpa berganti nama: `coverage.unresolvedAmount` dari "menyisakan satu jalur" (revisi `0.8`) menjadi "menyisakan nol jalur — selalu `0`". Disosialisasikan lewat `contracts/api-contract.md` § amendment revisi `0.9` |
| `DATABASE IMPACT` | **Nihil.** Migration `BE-BKC-027` yang sudah ada sudah cukup — tidak ada kolom/index/migration tambahan |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization |
| `VISUAL REFERENCE` | `NOT REQUIRED` — layar kasir sudah menjumlahkan `unresolvedAmount + nonBillableResidualAmount` sejak `BE-BKC-028`, menerima nominal jalur (2) tanpa perubahan satu baris pun |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru maupun berubah bentuknya.** Field yang maknanya bergeser (§ 3.3) terbawa
endpoint kalkulasi yang sama dengan `BE-BKC-022`/`024`/`025`/`028` (`GET .../calculation-preview`,
`POST .../recalculate`).

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna: build backend dijalankan manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis "satu akumulator, dua cabang" | **LULUS** | Manual | Kedua cabang (jalur 2 dan jalur 5) menulis ke variabel `nonBillableResidual` yang sama secara tekstual — tidak ada akumulator kedua yang diperkenalkan |
| Verifikasi statis cabang `IsAllowExcessPaymentByPatient = true` pada jalur (2) tidak tersentuh | **LULUS** | Manual + test existing `RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion` (tidak diubah, masih menguji kombinasi `NotCovered`+`true`) | Kode: `!rule.IsAllowExcessPaymentByPatient ? component.Amount : 0` — pola identik dengan sebelumnya, hanya tujuan penjumlahannya yang berubah |
| Verifikasi arity `BillingCoverageComponentOutcome`/`BillingCoverageDecision` | **LULUS** | Manual | Tidak ada field baru pada kedua record (berbeda dari `BE-BKC-028`) — tidak ada titik konstruksi yang perlu disesuaikan |
| Cakupan diff | **LULUS** | Manual | 1 berkas source + 1 berkas test berubah; nol migration, nol entity, nol controller, nol DTO |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna.

### Test baru

| Test | Membuktikan |
| --- | --- |
| `RegistrationCoverageAdapterNotCoveredRuleWithExcessDisallowedBecomesNonBillableResidual` | `BIL-AT-062` — rule `NotCovered` dengan `IsAllowExcessPaymentByPatient = false`: `primaryAmount=0`, `patientAmount=0`, `unresolvedAmount=0`, `nonBillableResidualAmount=100.000`, `hasNonBillableResidual=true`. Uji pasangan langsung dengan test existing yang membuktikan cabang `true` tidak tersentuh |
| `RegistrationCoverageAdapterCombinesNotCoveredAndResidualIntoSingleNonBillableAmount` | `BIL-AT-063` — satu tagihan dua item: item 1 lewat jalur (2) `NotCovered` (Rp 100.000, seluruhnya residual), item 2 lewat jalur (5) `Covered` 70% (Rp 100.000, residual Rp 30.000). Hasil: **satu** `nonBillableResidualAmount = 130.000` gabungan, `primaryAmount = 70.000` (dari item 2 saja), `unresolvedAmount = 0` — membuktikan kedua cabang benar-benar memakai akumulator yang sama |

**Rule pada test baru memakai `ItemType = "ServiceCategory"` + `TariffCategoryId`, bukan
`"Procedure"` seperti beberapa test lama di berkas yang sama.** Dipilih secara sengaja: kategori
item selalu terisi (`item.CategoryId` non-nullable), sehingga pencocokan `Matches()` deterministik
tanpa bergantung pada `TariffId`/`ProcedureId` yang tidak pernah diisi `SeedInvoiceAsync`. Test
`BIL-AT-063` butuh dua kategori berbeda (satu per item) yang hanya dapat dijamin lewat jalur ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| 1. Aturan `NotCovered` penanda `false`: seluruh nominal masuk selisih, nominal menggantung nol | **Tercakup** (`BIL-AT-062`) |
| 2. Aturan `NotCovered` penanda `true`: seluruh nominal tetap porsi pasien | **Tercakup** — test existing tidak diubah |
| 3. Satu tagihan dari kedua jalur menghasilkan satu nominal gabungan, satu plafon, satu pengajuan | **Tercakup** (`BIL-AT-063`) — satu `nonBillableResidualAmount`; satu plafon otomatis benar karena `CalculateNonBillableResidualRemainingAsync` (`BE-BKC-029`) membaca kolom gabungan yang sama, tidak diubah |
| 4. Kedua cabang wajib akumulator yang sama, tidak boleh dipecah dua variabel | **Terpenuhi di kode** — diverifikasi statis (§ 5) |
| 5. Field/kolom nominal menggantung dipertahankan apa adanya, nilainya selalu nol pada versi baru | **Terpenuhi di kode** — `unresolved` tidak dihapus, hanya tidak lagi diisi jalur mana pun |
| `BKC-DES-026`–`027` `approved`; kontrak `BIL-API-0.8`/`BIL-TEST-0.8` ditulis; `BIL-AT-062`–`063` ada dan lulus | **Approval/penulisan kontrak terpenuhi** (§ 0); "lulus" menunggu `dotnet test` pengguna |
| Nol perubahan skema | **Terpenuhi** — dikonfirmasi tidak ada migration baru |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum sepenuhnya tercapai.** Yang tersisa hanya menjalankan build dan test.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Sama seperti `BE-BKC-027`–`029`: kode `BE-BKC-027`–`030` bergantung pada kolom `BilCalculationVersion.NonBillableResidualAmount` yang migration-nya **belum dijalankan** ke basis data mana pun (`BKC-GATE-09` masih tertutup) |
| `KNOWN ISSUES` | Empat baris orphan pada `contracts/api-contract.md` (§ 0) ditemukan, dicatat, tidak dirapikan — di luar scope. `artifact_hashes` pada `blueprint-manifest.md` stale untuk empat berkas yang diedit task ini — ditandai eksplisit, perhitungan ulang didelegasikan `/manage-module-blueprint` |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi dan migration `BE-BKC-027` sudah dijalankan |
| `INCIDENTAL CHANGES` | Task ini mencakup dua mode berurutan (`MODULE BLUEPRINT MODE` lalu `BACKEND MODE`) atas instruksi eksplisit pengguna — bukan penyimpangan sepihak dari disiplin governance sesi ini; pengguna memilih opsi ini secara eksplisit lewat `AskUserQuestion` sebelum task dimulai |
| `INTERRUPTIONS` | Tidak ada |
| `GIT STATUS` | 2 berkas source/test + 8 berkas blueprint/kontrak berubah untuk task ini, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`–`029` yang belum di-build/test resmi oleh pengguna. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet test` mencakup seluruh perubahan yang menumpuk (`BE-BKC-022` s.d. `030`). Migration `BE-BKC-027` **MUST** dijalankan ke basis data (otorisasi terpisah, `BKC-GATE-09`) sebelum kode `BE-BKC-027`–`030` dapat berfungsi di lingkungan mana pun. Seluruh task backend blueprint `MVP-11` + revisi `0.9` kini selesai secara source. Task backend berikutnya sesuai urutan: `BE-BKC-023` (`BLOCKED` `BKC-GATE-03`, Security), `BE-BKC-031` (verifikasi data PPN, `READY`, bukan task kode), atau `BE-BKC-032` (penutup, menunggu seluruh task lain) |

## Update 6 September 2026 — migration `BE-BKC-027` dieksekusi, peringatan di atas tidak lagi berlaku

`dotnet build`/`dotnet test` dikonfirmasi lulus pengguna; `BKC-GATE-03` (Security) ditutup 5
September 2026; migration `BE-BKC-027` dieksekusi ke database dev dan dibuktikan langsung lewat
query read-only (kolom baru ada secara fisik). **Kode task ini tidak lagi akan gagal runtime.**
Tidak ada lagi gerbang governance yang menahan gelombang ini. Detail audit lengkap:
`task/report/backend/BE-BKC-032.md`.
