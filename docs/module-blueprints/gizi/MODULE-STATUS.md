# Status Modul Gizi

Diukur dari source 1 Oktober 2026, bukan disalin dari dokumen sebelumnya.

| Sumbu | Status | Angka |
|---|---|---|
| Backend | `SUBSTANTIAL` | 5 controller · 5 service · 41 endpoint · 16 tabel · 5.298 baris |
| Frontend | `SUBSTANTIAL` | 9 halaman · 10 folder view · 4 service |
| Integrasi | `PARTIAL` | membaca asesmen keperawatan dan CPPT; tidak menerbitkan fakta ke modul lain |
| Verifikasi | `STRONG` | **209 uji**, naik dari **nol** |

**Perkiraan ketuntasan: ~92%.**

| Berkas | Jumlah | Yang ditutup |
|---|---|---|
| `RequirementRuleTests` | 20 | `NutritionRequirementService` — kebutuhan nutrisi |
| `DiagnosisRuleTests` | 10 | `NutritionRequirementService` — diagnosis, plus penomoran enum |
| `OrderRuleTests` | 24 | `NutritionOrderService` |
| `DietRuleTests` | 21 | `NutritionDietService` — diet pasien |
| `ReportRuleTests` | 20 | `NutritionReportService` |
| `ProductionBatchRuleTests` | 32 | `NutritionDietService` — batch produksi dan distribusi |
| `MasterControllerTests` | 26 | `NutritionMasterController` |
| `PermissionMatrixTests` | 56 | matriks izin tingkat HTTP: 5 controller, 41 endpoint, satu kode modul, nol `AllowAnonymous` |

**Seluruh service dan controller modul kini tertutup uji**, dan sejak 5 Oktober 2026 **permission
matrix tingkat HTTP ikut tertutup** — 5 controller dan 41 endpoint dijaga `PermissionMatrixTests`.
Yang tersisa pada sumbu verifikasi hanyalah hal yang benar-benar tidak dapat dibuktikan dari
dalam modul: jawaban `403` runtime per peran, yang menuntut aplikasi berjalan, dan integrasi
ujung ke ujung ke modul lain.

## Permukaan

| Lapisan | Isi |
|---|---|
| Controller | `NutritionOrderController` (8), `NutritionDietController` (9), `NutritionMasterController` (14), `NutritionRequirementController` (6), `NutritionReportController` (4) |
| Service | `NutritionOrderService`, `NutritionDietService`, `NutritionRequirementService`, `NutritionRequirementCalculator`, `NutritionReportService` |
| Model | `GziNutritionOrder`, `GziNutritionOrderHistory`, `GziNutritionCareRecord`, `GziNutritionRequirement`, `GziNutritionDiagnosisMasters`, `GziNutritionMasters`, `GziPatientDiet`, `GziProductionBatch` |
| Uji | `Tests/QuilvianSystemBackend.NutritionTests` — 209 uji |

## Keputusan yang sudah berlaku dan terimplementasi

| ID | Isi | Bukti |
|---|---|---|
| `GIZ-DEC-011` | Diagnosis gizi dari master berkode milik Gizi, baseline **IDNT**, domain `NI`/`NC`/`NB`, tanpa isian bebas | `GziNutritionDiagnosisMasters.cs`, tiga domain di-`HasData` |
| `GIZ-DEC-012` | Lima zat gizi — energi kkal/hari, protein/lemak/karbohidrat gram/hari, cairan ml/hari — berhistori, nilai kalkulasi dan final terpisah, koreksi beralasan | `GziNutritionRequirement.cs`, lima parameter di-`HasData` |
| `GIZ-DEC-014` | Pemilik proses: Kepala Instalasi Gizi / Kepala Unit Gizi | `00-interview-decisions.md` |
| `GIZ-DEC-004` | Modul berhenti di penentuan diet; pemesanan ke dapur di luar cakupan | — |
| `GIZ-DEC-010` | Kunjungan ahli gizi memakai CPPT yang sudah ada | `TrxPatientIntegratedProgressNote` |

## Aturan yang terjaga uji

| Kode | Aturan |
|---|---|
| `GIZ004` | Kebutuhan hanya pada order yang masih berjalan; kunjungan harus milik order itu; ahli gizi harus ada |
| `GIZ005` | Diagnosis harus aktif dan dapat dipilih; tidak boleh kembar dalam satu kunjungan |
| `GIZ006` | Nilai wajib berada dalam batas master parameter |
| `GIZ013` | Kunci idempotensi yang dipakai untuk isi berbeda ditolak |
| `GIZ014` | Satu parameter sekali kirim; parameter asing ditolak |
| `GIZ015` | Satu revisi memuat seluruh parameter aktif |
| `GIZ016` | Koreksi atas hasil rumus wajib beralasan |
| `GIZ017` | Revisi kedua dan sesudahnya wajib beralasan |
| `GIZ018` | Paling banyak satu diagnosis primer per kunjungan |
| `GIZ019` | Rumus yang tidak terdaftar ditolak terang-terangan |

Ditambah penomoran `GziOrderStatus`, `GziPatientDietStatus`, `GziMealDeliveryStatus`, dan
`GziCareRecordType` — angkanya ikut tersimpan di basis data dan dibaca frontend.

## Sisa pekerjaan

| Hal | Prioritas | Catatan |
|---|---|---|
| Integrasi keluar | sedang | Gizi belum menerbitkan fakta apa pun ke modul lain; diet yang dibaca dapur masih berhenti di dalam modul. Butuh keputusan: konsumen mana yang dituju lebih dulu |
| Uji `NutritionOrderService.GetPagedAsync` pada volume besar | rendah | tangga status dan paging sudah terjaga 24 uji; perilakunya pada volume besar belum diukur |
| `GIZ-OQ-003` | rendah | isi `MstProfession` untuk ahli gizi — milik Human Resource, bukan Gizi |
| `GIZ-OQ-005` | rendah | interval skrining ulang pasien tidak berisiko |

Tiga baris yang sebelumnya ada di tabel ini — uji `NutritionOrderService`,
`NutritionDietService`, dan `NutritionReportService` — **sudah ditutup** dan karena itu dihapus:
`OrderRuleTests.cs` (24 deklarasi), `DietRuleTests.cs` (21), `ReportRuleTests.cs` (19), ditambah
`ProductionBatchRuleTests.cs` (30) dan `MasterControllerTests.cs` (26) yang menyusul. Suite Gizi
kini berjalan **209 uji** dan seluruhnya lulus.

## Yang sengaja tidak dikerjakan

**`GIZ-OQ-007` rumus kalkulasi kebutuhan nutrisi.** Ditunda atas keputusan pemilik proses
25 September 2026, bukan menunggu jawaban. V1 menerima nilai yang diinput dan difinalisasi ahli
gizi; registry `GziNutritionFormula` sudah berdiri — berkolom `FormulaCode`, `FormulaName`,
`FormulaVersion`, `SourceReference`, `ImplementationKey` — supaya rumus dapat ditambahkan
kemudian sebagai baris, tanpa membongkar tabel dan tanpa mengubah kode penyimpanan.

Mengisinya dengan rumus mana pun sekarang berarti membatalkan keputusan yang sudah disahkan.
Ketika nanti diminta, kandidat yang lazim adalah Harris-Benedict revisi atau Mifflin-St Jeor
beserta faktor aktivitas dan stres — tetapi pemilihannya, faktor-faktornya, dan sumber rujukannya
adalah keputusan instalasi gizi, dan `SourceReference` memang disiapkan untuk mencatat rujukan itu.
