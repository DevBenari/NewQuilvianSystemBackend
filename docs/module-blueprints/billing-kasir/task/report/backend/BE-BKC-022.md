# Laporan Perubahan Backend — `BE-BKC-022`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-022` — Rupiah tanggungan penjamin per komponen biaya |
| `TASK TYPE` | Implementasi backend (penambahan invarian bisnis + field kontrak response) |
| `COMPLEXITY` | `MEDIUM` |
| `CLASSIFICATION SCORE` | 3 — dinaikkan satu tingkat karena dua faktor bernilai ≥ 1 (logika uang = 1, kontrak API = 2) |
| `MODEL` | Claude Opus 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/` |
| Gelombang | `MVP-4` (`EPIC BKC-04`) |
| Blueprint | `BIL-CASH-001` revision `0.8` — `approved` 4 September 2026 (kontrak dikunci) |
| Kontrak berlaku | `BIL-API-0.7`, `BIL-VALIDATION-0.7`, `BIL-CALCULATION-0.7` — seluruhnya `approved` |
| Backend SHA saat mulai | `fd4a605` (branch `Yasmina`) |
| Tanggal | 4 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE` |
| Status registry | **`ACTIVE`** — wewenang implementasi ada (bukan `PLANNED` yang hanya memberi wewenang penamaan) |
| Keberlakuan | `NEW CODE` (field dan penjaga baru) di dalam berkas modul yang sedang berjalan; bukan `Trx*` legacy, bukan `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis — inti task), `QBE-API-001` (boundary/response/status yang sudah mapan), `QBE-DTO-001` (DTO, bukan entity EF), `QBE-SVC-001` (logika di Module Service, bukan controller), `QBE-ENT-003` (penjaga negatif: penanda **tidak** menjadi kolom persisted presentasi) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001/002`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-002/003`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada entity, prefix, konfigurasi EF, nomor bisnis, maupun migration baru |

### Selisih governance yang ditemukan dan dilaporkan

Sesuai `AGENTS.md` bagian "Batasan sumber aturan", dua selisih dicatat apa adanya:

1. `AGENTS.md` baris 28 menyuruh membaca `rules/GLOBAL_RULES.md`. Berkas dengan nama itu **tidak ada**; yang tersedia `rules/README.md`. Aturan tetap terbaca, jadi ini bukan `BLOCKED`.
2. `AGENTS.md` baris 31 menaruh kontrak rekayasa di `rules/backend/engineering/`. Folder itu tidak ada pada suite Skill; kontraknya berada di `docs/engineering/` repository ini — dan `rules/backend/TASK_RULES.md` baris 10 memang menunjuk ke sana. Yang dipakai adalah `docs/engineering/`.

Catatan tambahan: ketiadaan `.codex/` **bukan** blocker. `AGENTS.md` baris 21–27 memetakan akar `rules/` per vendor; `.codex` adalah jalur Codex/GPT, sedangkan agent ini Claude Code dengan akar `${CLAUDE_PLUGIN_ROOT}/.claude/rules/` yang tersedia lengkap.

---

## 1. Masalah yang diperbaiki

Sebelum task ini, mesin kalkulasi sudah menghitung rupiah tanggungan penjamin **per komponen biaya**
(hasil `BE-BKC-FIX-003` yang sudah ter-commit), tetapi dua hal belum ada:

1. **Tidak ada penjaga yang memastikan rincian itu menjumlah.** Bila jumlah alokasi seluruh baris
   berbeda dari total tanggungan, perhitungan tetap diteruskan. Selisihnya baru terlihat di meja
   petugas klaim asuransi sebagai lembar tagihan yang tidak menjumlah.
2. **Tidak ada cara membedakan "penjamin menanggung Rp 0" dari "kami tidak punya rinciannya".**
   Versi kalkulasi yang tersimpan sebelum rincian per baris ada akan terbaca sebagai Rp 0 di setiap
   barisnya — angka yang tampak sungguhan padahal sebenarnya tidak ada datanya.

Keduanya sudah diputuskan pada `BKC-DEC-069` beserta keputusan arsitektur `BKC-DES-004` dan
`BKC-DES-017`, dan diformalkan sebagai `BIL-VAL-028`.

## 2. Proses bisnis

**Tujuan.** Petugas klaim asuransi menerima lembar tagihan yang setiap barisnya menyebut berapa
rupiah ditanggung penjamin, dan jumlah seluruh baris itu sama persis dengan total tanggungan.

**Pelaku.** Mesin kalkulasi menghitung; kasir dan petugas Billing membacanya lewat Menu Pembayaran;
petugas klaim asuransi membacanya lewat lembar dokumen.

**Pemicu.** Setiap perhitungan ulang tagihan — baik pratinjau maupun yang disimpan sebagai versi.

**Aturan bisnis yang ditegakkan.**

| Aturan | Perilaku |
| --- | --- |
| `BIL-VAL-028` | Jumlah alokasi tanggungan seluruh komponen **wajib sama persis** dengan total tanggungan. Ambang toleransi **nol** |
| `BKC-DES-004` | Hasil perhitungan menyatakan sendiri apakah rincian per barisnya dapat dipercaya |

**Contoh berangka.** Tagihan berisi tiga item tercover Rp 100.000, Rp 240.000, dan Rp 15.000.
Jumlah barisnya Rp 355.000. Bila total tanggungan yang dihitung mesin ternyata Rp 350.000,
perhitungan **dihentikan** — bukan diteruskan dengan selisih Rp 5.000 yang tidak dapat dijelaskan
kepada pihak asuransi.

**Kenapa toleransinya nol.** Setiap nominal per komponen sudah dibulatkan dua desimal di sumbernya
(`BillingCoverageAdapter`), sehingga selisih satu rupiah pun berarti kesalahan alokasi, bukan
pembulatan.

**Jalur tidak normal.** Perhitungan yang melanggar `BIL-VAL-028` dihentikan dengan pesan
"Rincian tanggungan penjamin per baris tidak menjumlah ke total tanggungan; hubungi tim teknis."
dan **versi kalkulasi baru tidak dibuat**. Jalur pasien tunai mengembalikan daftar alokasi kosong
dengan total tanggungan nol, sehingga `0 == 0` dan lolos tanpa perlakuan khusus.

**Hasil akhir.** Setiap versi kalkulasi yang berhasil disimpan dijamin rinciannya menjumlah, dan
membawa penanda bahwa rinciannya memang tersedia.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | Tempat `ApplyCoverageWaterfall` dan pola penjaga yang sudah ada |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` | Bentuk `BillingCoverageDecision` dan `BillingCoverageComponentOutcome` |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs` | Bentuk `CoverageCalculationResponse` dan field per baris yang sudah ada |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilCalculationVersion.cs` | Cara `BreakdownSnapshot` disimpan |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` | Pola test dan fixture adapter yang sudah ada |
| `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `AGENTS.md`, `rules/backend/*` | Preflight governance |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Dtos/BillingInvoiceDtos.cs` | Properti `IsPerItemAllocationAvailable` pada `CoverageCalculationResponse`, tanpa nilai awal `true` |
| `Billing/Services/BillingCalculationService.cs` | Penjaga `BIL-VAL-028` pada `ApplyCoverageWaterfall`; penanda diisi `true` pada hasil perhitungan baru |
| `Tests/.../BillingCalculationServiceTests.cs` | Tiga test baru, dua adapter uji baru, dua fixture basi diperbaiki |

Total: **3 berkas, 137 baris bertambah, 7 baris berubah.** Tidak ada berkas lain tersentuh.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Additive.** Satu field boolean baru pada `breakdown.coverage`. Tidak ada endpoint baru, tidak ada field yang dihapus atau berganti arti. Consumer lama yang tidak membacanya tidak terpengaruh |
| `DATABASE IMPACT` | **Nihil.** Tidak ada kolom, index, entity, maupun migration. Penanda hidup di dalam `BreakdownSnapshot` JSON yang sudah ada — sengaja **tidak** dijadikan kolom persisted, sesuai `QBE-ENT-003` |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization, authentication, maupun data yang diekspos. Pesan galat tidak memuat nomor polis, nama pasien, maupun nomor rekam medis |
| `VISUAL REFERENCE` | `NOT REQUIRED` — tidak ada perubahan tampilan |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru.** Field bertambah pada response dua endpoint yang sudah ada:

### `[Tags("Health Services / Billing Management / Billing / Invoices")]`

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id:guid}/calculation-preview` | Menghitung ulang tagihan tanpa menyimpannya | `BillingInvoice : Read` | Path `id` | `ApiResponse<CalculationResponse>` — **bertambah** `breakdown.coverage.isPerItemAllocationAvailable` |
| `POST` | `/{id:guid}/recalculate` | Menghitung ulang dan menyimpannya sebagai versi baru | `BillingInvoice : Update` | Path `id` + `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` — **bertambah** field yang sama |

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Perhitungan berhasil |
| `422` | Perhitungan melanggar batas yang dijaga — **termasuk `BIL-VAL-028` yang baru**: rincian per baris tidak menjumlah ke total tanggungan |
| `409` | Data tagihan berubah pihak lain |

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi pengguna yang berlaku sepanjang sesi: build backend dijalankan pengguna secara manual, bukan oleh agent |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis `using`/helper | **LULUS** | Manual | `System.Linq` tersedia lewat `ImplicitUsings` (dan file sudah memakai LINQ pada baris `coverableAmount`); `Money()` ada di `BillingCalculationService.cs:1051`; `ComponentOutcomes` bertipe `IReadOnlyList<BillingCoverageComponentOutcome>` |
| Verifikasi statis rujukan yatim | **LULUS** | Manual | Pencarian `FixedCoverageAdapter(decision)` dan `var decision =` pada berkas test tidak menemukan sisa rujukan |
| Cakupan diff | **LULUS** | Manual | `git diff --stat` menunjukkan tepat 3 berkas; nol migration, nol entity, nol controller |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna. Laporan ini mencatat keadaan sebenarnya, bukan mengklaim keberhasilan.

### Regresi yang wajib diperhatikan — dua fixture test yang sengaja diperbaiki

Penjaga `BIL-VAL-028` **membuat dua test yang sudah ada menjadi gagal**, dan itu benar secara
desain: keduanya membuat `BillingCoverageDecision` dengan total tanggungan lebih dari nol tetapi
daftar alokasi kosong — bentuk yang justru dilarang aturan baru ini. Fixture-nya ditulis sebelum
alokasi per komponen ada, jadi yang basi adalah fixture-nya, bukan penjaganya.

| Test | Keadaan lama | Perbaikan |
| --- | --- | --- |
| `CoverageWaterfallAppliesPrimaryThenExcessThenPatient` | Total Rp 60.000 dengan alokasi kosong | Memakai `AllocatingCoverageAdapter` yang menurunkan alokasi dari komponen nyata |
| `AdministrationFeeCoverableFlagGatesWhetherInsurerCanCoverIt` | Total Rp 120.000 dengan alokasi kosong | Sama; pada cabang admin fee tidak coverable, penjaga cap tetap berjalan lebih dulu sehingga pesan "melebihi biaya" tidak berubah |

**Seluruh nilai yang di-assert kedua test itu tidak berubah satu rupiah pun** — yang berubah hanya
cara fixture menyusun decision-nya.

### Test baru

| Test | Membuktikan |
| --- | --- |
| `RincianTanggunganPerBarisYangTidakMenjumlahMenghentikanPerhitungan` | `BIL-VAL-028` menghentikan perhitungan dan **tidak** membuat versi kalkulasi baru (selisih Rp 10.000 dari total Rp 60.000) |
| `PerhitunganBaruMenyatakanRincianPerBarisTersedia` | Perhitungan baru selalu menyatakan rinciannya tersedia |
| `SnapshotLamaTanpaPenandaTerbacaSebagaiRincianTidakTersedia` | `BIL-AT-050` — snapshot lama tanpa properti ini terbaca `false` tanpa galat |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-029` — rincian menjumlah ke total | **Tercakup** oleh penjaga + test jalur gagal; angka penuh menunggu test dijalankan |
| `BIL-AT-030` — kunci alokasi tidak tertukar antar baris pajak | **Sudah terpenuhi sebelum task ini** — kunci `(ComponentId, ComponentType)` sudah ada lewat `BE-BKC-FIX-003` |
| `BIL-AT-049` — komponen pajak ganda tanpa `PolicyId` | **Sudah terpenuhi sebelum task ini**, alasan sama |
| `BIL-AT-050` — snapshot lama terbaca `false` | **Tercakup** test baru |
| `BIL-AT-052` — penjumlahan per baris sama dengan total | **Tercakup** penjaga `BIL-VAL-028` |
| Ambang toleransi nol | **Terpenuhi** — perbandingan kesamaan langsung, bukan rentang |
| Formula tanggungan tidak berubah | **Terpenuhi** — tidak ada aritmetika yang disentuh, hanya penjaga yang ditambahkan |
| Tidak ada migration | **Terpenuhi** |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum tercapai.** Yang tersisa hanya menjalankan build dan test.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Penjaga baru ini menyentuh mesin perhitungan uang **seluruh** tagihan berpenjamin. Bila ada jalur produksi yang selama ini menghasilkan alokasi tidak menjumlah, jalur itu akan mulai gagal `422` — dan itu memang tujuannya, tetapi wajib diperhatikan saat pengujian pertama |
| `KNOWN ISSUES` | `BillingCalculationContract.Version` **sengaja tidak dinaikkan** dari `"BIL-CALCULATION-0.4"`. Kontrak API menyatakan konstanta itu informasi investigasi saja dan **MUST NOT** dipakai program sebagai penentu ketersediaan rincian; penanda otoritatifnya adalah `isPerItemAllocationAvailable`. Menaikkannya ke `0.6` sekarang akan menyiratkan field anomali (`BE-BKC-025`) sudah ada padahal belum. Diusulkan dinaikkan oleh task yang menuntaskan permukaan `0.6` |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi |
| `INCIDENTAL CHANGES` | `NONE` |
| `INTERRUPTIONS` | `NONE` |
| `GIT STATUS` | 3 berkas source/test berubah, belum di-stage. Working tree juga memuat 13 berkas dokumentasi blueprint dari pekerjaan sebelumnya pada sesi yang sama — atas persetujuan eksplisit pengguna untuk melanjutkan di atas tree apa adanya. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet build` dan `dotnet test` secara manual. Bila lulus, task ini selesai dan `BE-BKC-024` dapat dimulai (juga `READY_FOR_TASK_APPROVAL`, tanpa gerbang). Bila `dotnet test` menemukan test existing lain yang ikut gagal karena `BIL-VAL-028`, laporkan — kemungkinan besar fixture-nya basi seperti dua yang sudah diperbaiki, bukan penjaganya yang salah |
