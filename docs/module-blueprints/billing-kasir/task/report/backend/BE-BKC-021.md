# Laporan Perubahan Backend — `BE-BKC-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-021` |
| Judul | Penyempitan gating approval pada mesin kalkulasi coverage |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026, blueprint revisi `0.5 approved`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-021` (baris 351–360) |
| Trace | `BKC-DEC-062` (amendemen atas `BKC-DEC-042`); `FR-BKC-007`; acceptance `BIL-AT-027` (bagian kalkulasi resmi); `NFR-004` (regresi nol) |
| Contract version | Tidak ada perubahan kontrak API — perubahan murni logika internal `RegistrationBillingCoverageAdapter.ResolveAsync` |
| Dependency | `BE-BKC-001` saja. Independen dari `BE-BKC-018`–`020`, TAPI dampaknya **GLOBAL** ke semua invoice produksi — bukan hanya slice katalog tarif ini |
| Klasifikasi | Rubrik murni: skor 2 (repo 0, berkas diperiksa 0, berkas diubah 0, **logika bisnis 2/Kompleks**, kontrak API 0, database 0, keamanan 0, UI/workflow 0) → `LIGHT`. **Dinaikkan manual ke `MEDIUM`** — ukuran diff kecil (2 kondisi dihapus dari satu method), tapi radius dampak GLOBAL ke kalkulasi finansial semua invoice produksi, ditandai eksplisit "Risiko tertinggi di slice ini" pada roadmap |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs`, `QuilvianSystemBackend.Tests/BillingManagement/BillingCalculationServiceTests.cs`, `docs/module-blueprints/billing-kasir/` |
| Model | Claude Sonnet 5 |
| Governance Preflight | Area `HealthServices`; Module/pemilik `BillingManagement / Billing`; prefix `Bil` (registry, Lifecycle `ACTIVE`). Tidak ada entity/module baru. Keberlakuan `TOUCHED LEGACY` — mengubah kondisi pada method yang sudah lama berjalan dan dipakai SEMUA invoice, bukan menulis kode baru |
| QBE ID berlaku | `QBE-VAL-001` (invarian bisnis coverage — gate mana yang dipertahankan vs dihapus divalidasi eksplisit lewat test); tidak ada QBE lain yang relevan (tidak ada entity/DTO/API baru) |
| Otorisasi eksplisit | Pengguna diberi tahu risiko (kutipan roadmap: "Risiko tertinggi di slice ini... rekomendasi menginformasikan Payer/Insurance + Finance/AR sebelum deploy ke produksi, meski blueprint tidak mewajibkannya sebagai blocker") lewat pertanyaan konfirmasi eksplisit sebelum implementasi dimulai. Pengguna memilih **"Lanjut implementasi source+test sekarang"** — deploy/migration/eksekusi database tetap TIDAK termasuk wewenang task ini |
| Commit backend saat dikerjakan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source (2 kondisi dihapus) dan 4 test baru (1 `Theory` 3 kombinasi + 2 `Fact` pembuktian gate yang dipertahankan) selesai ditulis. Regresi pada test existing ditelusuri **lewat pembacaan source** (bukan eksekusi — lihat § 5). **Build/test sengaja tidak dijalankan** oleh sesi ini sesuai instruksi eksplisit pengguna sebelumnya yang berlaku sepanjang sesi ini. Belum boleh ditandai selesai sampai hasil verifikasi pengguna dikonfirmasi |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, `RegistrationBillingCoverageAdapter` (mesin yang menentukan berapa banyak
tagihan yang ditanggung asuransi pada **setiap** kalkulasi invoice, bukan cuma form testing)
menggeser sebuah item ke "unresolved" — artinya tidak dihitung sebagai tertanggung asuransi sama
sekali, dan Subtotal Asuransi di Menu Pembayaran jadi lebih kecil dari seharusnya — setiap kali
rule coverage-nya menandai `IsNeedApproval=true` atau `IsNeedGuaranteeLetter=true`, **walaupun**
`CoverageStatus` rule itu sendiri sudah eksplisit `"Covered"`.

Ini keliru secara bisnis: butuh approval atau surat jaminan (SJP) adalah **proses administratif**
sebelum klaim diajukan ke asuransi, bukan penolakan coverage. Banyak rule asuransi di dunia nyata
memang mewajibkan approval/SJP untuk tindakan tertentu **sambil tetap** menanggung biayanya penuh
begitu disetujui. Dengan gating lama, setiap rule seperti ini membuat item-nya salah dihitung
sebagai "tidak tercover" pada tampilan Subtotal Asuransi — padahal semestinya tercover, hanya
butuh langkah approval terpisah.

Contoh konkret: rule asuransi BPJS untuk "MRI Kepala" mewajibkan approval dokter penjamin
(`IsNeedApproval=true`) tapi `CoverageStatus="Covered"`, `CoveragePercent=80`. Sebelum perbaikan
ini, item MRI senilai Rp1.500.000 akan tercatat `PrimaryAmount=0` (tidak tercover sama sekali) dan
seluruhnya jatuh ke `unresolved`. Setelah perbaikan, item ini benar tercatat `PrimaryAmount=Rp1.200.000`
(80%), sama seperti rule yang tidak butuh approval — kebutuhan approval-nya tetap tercatat sebagai
informasi (lewat `BE-BKC-020`), tapi tidak lagi menggagalkan perhitungan coverage.

---

## 2. Proses bisnis

**Tujuan**: item dengan rule coverage berstatus `Covered` tidak lagi jatuh ke `unresolved` hanya
karena butuh approval/surat jaminan — untuk **SEMUA invoice**, bukan cuma dari form testing katalog
tarif.

**Pelaku**: tidak ada aktor manusia langsung — perubahan ini otomatis berlaku pada setiap kalkulasi
invoice berikutnya untuk pasien dengan penjamin asuransi.

**Pemicu**: setiap pemanggilan `BillingCalculationService.RecalculateAsync`/preview kalkulasi pada
invoice dengan penjamin asuransi yang punya rule coverage cocok.

**Langkah** (di dalam `ResolveAsync`, per komponen tagihan yang coverable):

1. Cari rule coverage yang paling spesifik cocok dengan komponen (tarif/obat/kategori/tindakan).
   Bila tidak ada rule yang cocok → tetap `unresolved` (tidak berubah oleh task ini).
2. **[BERUBAH]** Bila rule ditemukan: cek apakah `CoverageStatus == "NeedApproval"` **atau** ada
   limit bulanan (`MaxAmountPerMonth`/`MaxQuantityPerMonth`) — kedua kondisi ini **tetap**
   menggeser komponen ke `unresolved`, tidak berubah oleh task ini.
3. **[DIHAPUS]** Kondisi `rule.IsNeedApproval || rule.IsNeedGuaranteeLetter` **tidak lagi** ikut
   memeriksa langkah 2 — rule dengan flag ini tapi `CoverageStatus="Covered"` sekarang lanjut ke
   langkah berikutnya seperti rule biasa.
4. Bila `CoverageStatus == "NotCovered"` → tetap sesuai perilaku lama (tidak berubah).
5. Selain itu, nominal tertanggung dihitung dari `CoveragePercent`/co-payment/limit per-kunjungan
   seperti biasa (tidak berubah), lalu ditambahkan ke `PrimaryAmount`.

**Aturan yang berlaku**: hanya DUA gate yang boleh menggeser komponen ke `unresolved` sekarang:
`CoverageStatus == "NeedApproval"` (status rule itu sendiri belum diputuskan — beda dari flag
approval administratif) dan limit bulanan (butuh pemeriksaan pemakaian kumulatif yang belum
tersedia pada adapter ini). Ini **bukan** pelepasan gating penuh — scope-nya sengaja dipersempit
sesuai `02-backend-architecture.md`.

**Status yang dihasilkan**: tidak ada status baru — hanya nominal `PrimaryAmount`/`UnresolvedAmount`
pada `BillingCoverageDecision` yang berubah untuk kombinasi rule tertentu.

**Jalur tidak normal**: tidak berubah — jalur `SelfPay`, `Unresolved` (encounter/payment source tidak
valid), dan `NotCovered` semuanya identik dengan sebelumnya.

**Hasil akhir**: Subtotal Asuransi pada Menu Pembayaran mencerminkan coverage yang benar untuk rule
yang butuh approval/SJP tapi statusnya sudah `Covered` — untuk seluruh invoice yang memakai
penjamin asuransi dengan rule seperti ini, sejak task ini diterapkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-021`;
`Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` (seluruh isi);
**regresi**: seluruh test project ditelusuri untuk pemakaian `RegistrationBillingCoverageAdapter`
(kelas konkret, bukan mock/fixed adapter) dan seed `MstInsuranceCoverageRule` dengan
`IsNeedApproval`/`IsNeedGuaranteeLetter` — ditemukan pada `QuilvianSystemBackend.Tests/
BillingManagement/{BillingCalculationServiceTests.cs, BillingAllocationServiceTests.cs,
BillingDiscountServiceTests.cs, BillingInvoiceServiceTests.cs}` (4 file, persis sesuai catatan
roadmap "`BillingCalculationServiceTests.cs` dan 3 file test lain").

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Billing/Services/BillingCoverageAdapter.cs` | Hapus `rule.IsNeedApproval \|\| rule.IsNeedGuaranteeLetter` dari kondisi gating `unresolved`; pertahankan `CoverageStatus=="NeedApproval"` dan limit bulanan sebagai gate. Komentar penjelas ditambahkan pada kondisi yang berubah, mengutip `BKC-DEC-062` |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingCalculationServiceTests.cs` | 4 test baru: `Theory` 3 kombinasi (`IsNeedApproval`/`IsNeedGuaranteeLetter` true/false) membuktikan item tetap tercover penuh; 2 `Fact` membuktikan `CoverageStatus="NeedApproval"` dan `MaxAmountPerMonth` tetap menggeser ke `unresolved` (gate yang dipertahankan) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint/DTO/response shape yang berubah. Perubahan murni logika internal method yang sudah ada |
| Database | `NOT APPLICABLE` — tidak ada model/migration/schema yang disentuh |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan authorization/authentication. **Dampak finansial** (bukan keamanan) adalah risiko utama task ini — lihat § 7 |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint apa pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` / `dotnet test` | **Sengaja tidak dijalankan oleh sesi ini** | `NOT RUN` | Instruksi eksplisit pengguna berlaku sepanjang sesi ini ("jangan lakukan build dibackground biar saya cek sendiri") — pengguna memverifikasi sendiri secara manual |
| Regresi test existing (analisis statis, bukan eksekusi) | Ditelusuri lewat pembacaan source langsung, BUKAN dijalankan | `NOT RUN` (evidence tetap dicatat di bawah) | Lihat rincian di bawah |
| `RegistrationCoverageAdapterCoversItemEvenWhenRuleNeedsApprovalOrGuaranteeLetter` (`Theory`, 3 kombinasi) | Ditulis, **belum dijalankan** | `NOT RUN` | Membuktikan `PrimaryAmount=80.000`, `UnresolvedAmount=0` untuk kombinasi `(true,false)`, `(false,true)`, `(true,true)` pada rule `Covered` |
| `RegistrationCoverageAdapterStillGatesRuleWithNeedApprovalCoverageStatus` | Ditulis, **belum dijalankan** | `NOT RUN` | Membuktikan gate `CoverageStatus="NeedApproval"` TIDAK berubah — `PrimaryAmount=0`, `UnresolvedAmount=100.000` |
| `RegistrationCoverageAdapterStillGatesRuleWithMonthlyLimit` | Ditulis, **belum dijalankan** | `NOT RUN` | Membuktikan gate `MaxAmountPerMonth` TIDAK berubah — `PrimaryAmount=0`, `UnresolvedAmount=100.000` |

**Analisis regresi statis (dilakukan sebagai pengganti eksekusi test, per instruksi "jangan build"
pengguna)** — ditelusuri satu per satu pada 4 file yang memakai `RegistrationBillingCoverageAdapter`
konkret:

1. `BillingCalculationServiceTests.cs`, test `RegistrationCoverageAdapterUsesApprovedGenericPrimaryRule`
   (baris ~209–249, sudah ada sebelum task ini): rule yang di-seed **tidak** menyetel
   `IsNeedApproval`/`IsNeedGuaranteeLetter` (default `false` pada model `MstInsuranceCoverageRule`,
   dikonfirmasi lewat pembacaan `MstInsuranceCoverageRule.cs`). Kondisi yang dihapus sebelumnya
   selalu `false || false` untuk test ini — **tidak ada perubahan hasil**.
2. `BillingDiscountServiceTests.cs`: memakai `RegistrationBillingCoverageAdapter` lewat helper
   `CreateCalculationService`, tapi **tidak ada satu pun** `MstInsuranceCoverageRule` yang di-seed
   di seluruh file (dikonfirmasi lewat pencarian menyeluruh) — jalur yang tereksekusi adalah
   `SelfPay()` atau "rule tidak ditemukan → unresolved", keduanya **tidak tersentuh** perubahan ini.
3. `BillingInvoiceServiceTests.cs`: `RegistrationBillingCoverageAdapter` dipakai lewat helper
   `CreateCalculationService`; satu-satunya kemunculan `IsNeedApproval` di file ini adalah pada test
   `CatalogChargeCoveragePreviewIsCoveredEvenWhenRuleNeedsApprovalOrGuaranteeLetter` milik
   `BE-BKC-020` (ditambahkan sesi ini) — test itu memakai `InsuranceCoverageService` (Clinical
   Management, kelas **berbeda sepenuhnya**, bukan `RegistrationBillingCoverageAdapter`), jadi
   **tidak terpengaruh** perubahan ini sama sekali.
4. `BillingAllocationServiceTests.cs`: tidak memakai `RegistrationBillingCoverageAdapter` konkret
   maupun menyeed `MstInsuranceCoverageRule` (dikonfirmasi tidak ada hasil pencarian untuk kedua
   pola tersebut) — **tidak terdampak**.

**Kesimpulan analisis**: berdasarkan pembacaan source, **nol** test existing yang bergantung pada
perilaku gating lama (`IsNeedApproval`/`IsNeedGuaranteeLetter` menggeser ke `unresolved`). Ini
adalah evidence sementara pengganti eksekusi nyata — **eksekusi `dotnet test` sesungguhnya oleh
pengguna tetap wajib** sebelum task ini dianggap terverifikasi, sesuai DoD "Regression evidence
eksplisit (bukan cuma 'test baru lulus')".

Uji manual: `NOT FEASIBLE` pada sesi ini — perubahan logika internal, tidak ada UI/endpoint untuk
diklik-coba secara langsung; dampaknya baru terlihat di Menu Pembayaran (frontend) untuk invoice
dengan penjamin asuransi yang punya rule butuh approval.

**Tidak dijalankan:** seluruh build/test (keputusan sadar pengguna); eksekusi database
(`NOT APPLICABLE`); deploy ke produksi (di luar wewenang task ini — lihat § 7 untuk rekomendasi
notifikasi stakeholder sebelum deploy).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-027` (bagian kalkulasi resmi) — rule `Covered`+`IsNeedApproval` tetap dihitung tercover pada kalkulasi resmi (bukan cuma preview `BE-BKC-020`) | Terpenuhi (source + test, **belum dijalankan** — lihat § 5) | `RegistrationCoverageAdapterCoversItemEvenWhenRuleNeedsApprovalOrGuaranteeLetter` |
| Regresi nol pada `BillingCalculationServiceTests.cs` dan 3 file test lain (`CAP-05`, `NFR-004`) | Dianalisis statis: nol test terdampak (lihat § 5 rincian per file). **Belum dikonfirmasi lewat eksekusi nyata** | Analisis 4 file di § 5 |
| `CoverageStatus="NeedApproval"` dan limit bulanan TIDAK ikut berubah (review manual) | Terpenuhi — dibuktikan eksplisit lewat 2 test baru, bukan cuma diasumsikan | `RegistrationCoverageAdapterStillGatesRuleWithNeedApprovalCoverageStatus`, `RegistrationCoverageAdapterStillGatesRuleWithMonthlyLimit` |
| DoD: Regression evidence eksplisit; before/after behavior terdokumentasi; build lulus | **Belum sepenuhnya terpenuhi** — regression evidence tertulis lengkap (§ 5) tapi berbasis analisis statis, bukan eksekusi; before/after terdokumentasi lengkap (§ 1, § 2, diff kode); build **belum dijalankan** | Lihat § 5 dan § 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Ditemukan BOM (`﻿`) tidak sengaja tertambah di awal `BillingCoverageAdapter.cs` akibat encoding default script patch — dideteksi lewat `git diff` dan diperbaiki sebelum laporan ini ditulis, sehingga diff akhir bersih (hanya menyentuh baris yang relevan) |
| Masalah yang diketahui | **Sebelum task ini dieksekusi**, pengguna diberi tahu eksplisit lewat pertanyaan konfirmasi bahwa `BKC-DEC-062` disetujui Product/Domain Owner TANPA konfirmasi terpisah dari Payer/Insurance + Finance/AR, dan disarankan menginformasikan mereka sebelum **deploy ke produksi**. Pengguna memilih lanjut implementasi sekarang; **notifikasi stakeholder tersebut belum dilakukan** dan tetap jadi prasyarat yang direkomendasikan sebelum deploy — dicatat di sini sebagai pengingat eksplisit, sesuai instruksi roadmap "Wajib dibaca sebelum eksekusi" |
| Risiko tersisa | Perubahan ini mengubah kalkulasi finansial untuk **SEMUA invoice produksi** dengan penjamin asuransi yang punya rule butuh approval/SJP — bukan hanya data uji. Regression evidence pada § 5 bersifat analisis statis (pembacaan source), BUKAN eksekusi nyata `dotnet test`; ada kemungkinan (walau kecil, berdasarkan penelusuran menyeluruh) analisis ini melewatkan sesuatu yang hanya terlihat saat dieksekusi. **Wajib** dijalankan `dotnet test --filter "FullyQualifiedName~BillingCalculationServiceTests\|FullyQualifiedName~BillingDiscountServiceTests\|FullyQualifiedName~BillingAllocationServiceTests\|FullyQualifiedName~BillingInvoiceServiceTests"` sebelum mempertimbangkan deploy. Notifikasi Finance/AR/Payer (lihat baris di atas) belum dilakukan |
| Perubahan sampingan | BOM yang tidak sengaja tertambah pada `BillingCoverageAdapter.cs` — sudah diperbaiki sebelum laporan ini ditulis (lihat *Peringatan* di atas), tidak tersisa di diff akhir |
| Interupsi | Sebelum implementasi dimulai, sesi ini secara sadar berhenti dan mengajukan pertanyaan konfirmasi eksplisit kepada pengguna mengenai risiko task ini (bukan interupsi teknis, melainkan jeda disengaja untuk memastikan pengguna sadar akan catatan risiko roadmap sebelum melanjutkan) — pengguna menjawab "Lanjut implementasi source+test sekarang", dicatat pada Metadata § *Otorisasi eksplisit* |
| Status Git | `On branch Yasmina, up to date with 'origin/Yasmina'`. Modified (13 file, gabungan `BE-BKC-018`–`021` pada sesi yang sama): `Areas/HealthServices/BillingManagement/Billing/{Controllers/BillingInvoicesController.cs, Dtos/BillingInvoiceDtos.cs, Models/BilInvoiceItem.cs, Services/BillingChargeSourceAdapter.cs, Services/BillingCoverageAdapter.cs, Services/BillingInvoiceService.cs}`, `Migrations/ApplicationDbContextModelSnapshot.cs`, `QuilvianSystemBackend.Tests/BillingManagement/{BillingCalculationServiceTests.cs, BillingInvoiceServiceTests.cs}`, `Repositories/Configurations/.../BilInvoiceItemConfiguration.cs`, `docs/module-blueprints/billing-kasir/{contracts/api-contract.md, roadmap/backend-roadmap.md, roadmap/requirement-traceability.md}`. Untracked: `Migrations/20260903015730_AddTariffIdToBilInvoiceItem.{cs,Designer.cs}`, `QuilvianSystemBackend.Tests/BillingManagement/BillingChargeSourceAdapterTests.cs`, `docs/module-blueprints/billing-kasir/task/report/backend/{BE-BKC-018.md,BE-BKC-019.md,BE-BKC-020.md,BE-BKC-021.md}`. Belum staged/commit |
| Langkah berikutnya | 1) **Pengguna menjalankan `dotnet build` lalu full regression `dotnet test --filter "FullyQualifiedName~Billing"`** — ini prasyarat mutlak, lebih kritis dari task lain di slice ini mengingat dampak globalnya. 2) **Sebelum deploy ke produksi**: informasikan Payer/Insurance dan Finance/AR sesuai rekomendasi eksplisit roadmap (`BKC-DEC-062`) — belum dilakukan pada sesi ini. 3) Setelah keempat task (`BE-BKC-018`–`021`) terverifikasi build/test, seluruh scope backend slice "Entri manual katalog tarif + coverage per item" selesai — task frontend terkait (`FE-BKC-014`–`016`) baru bisa mulai setelah kontrak backend ini terbukti stabil |
