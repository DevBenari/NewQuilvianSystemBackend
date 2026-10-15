# Register Task — Modul Farmasi

Rekap task modul Farmasi yang dapat dihitung, disusun 15 Oktober 2026.

## Mengapa register ini ada

`roadmap/backend-roadmap.md` hanya mencakup **gelombang financial clearance**
(`PHA-BE-004/005/006`, `PHA-FE-002`), dan `roadmap/README.md` mencakup **gelombang depot
routing** (`PHA-BE-001/002/003`, `PHA-FE-001`). Kapabilitas Farmasi lain — telaah, penyiapan,
telaah akhir, penyerahan, retur, stok, etiket, dan otorisasi — sudah terkirim tetapi **tidak
pernah punya ID task**, sehingga modul ini hanya dapat dilaporkan sebagai estimasi source.

Register ini menyatukan keduanya dan menambahkan task turunan untuk kapabilitas yang belum
ber-ID. **Nol task dikarang**: tiap baris turunan menunjuk kontrak, dokumen, atau berkas uji yang
sudah ada sebelum register ini.

| Hal | Isi |
|---|---|
| Uji | `Tests/QuilvianSystemBackend.PharmacyTests` — **544 uji tertulis** |
| Bukti runtime | 12 dan 14 Oktober 2026, `localhost/QuilvianNewDevIkbalFr` |
| Register gabungan | [`../../MODULE-COMPLETION-NORMALIZATION.md`](../../MODULE-COMPLETION-NORMALIZATION.md) |

> **Tabrakan penomoran.** `roadmap/README.md` memakai `PHA-BE-001/002/003` untuk task depot
> routing, sementara `bug-pha-be-002-klarifikasi-tertutup.md` dan
> `bug-pha-be-003-retur-tanpa-batas-serah.md` memakai nomor yang sama untuk dua bug yang tidak
> berhubungan. Register ini memakai definisi roadmap sebagai yang sah. Kedua perbaikan bug tetap
> dihitung sebagai bukti di dalam `PHA-D-01` dan `PHA-D-03`, bukan sebagai baris sendiri, supaya
> tidak terhitung dua kali.

## Task asli

| Task ID | Outcome | Status | Bukti |
|---|---|---|---|
| `PHA-BE-001` | Resolver routing depo | **Selesai** | `task/report/backend/PHA-BE-001.md`; `DepotRoutingTests` (26) |
| `PHA-BE-002` | Skenario uji resolver | **Selesai** | `DepotRoutingTests`: layanan, nol, dan ganda |
| `PHA-BE-003` | Integrasi routing ke workflow | **Blocked — keputusan bisnis** | `PHA-OQ-014` (Product) belum dijawab: pembayaran berhasil tetapi reservasi gagal |
| `PHA-BE-004` | Salinan keadaan finansial resep | **Selesai** | `task/report/backend/PHA-BE-004.md`; `verifikasi-runtime-be-bkc-067.md`; `FinancialClearanceTests` (18) |
| `PHA-BE-005` | Empat gerbang alur kerja farmasi | **Selesai** | `task/report/backend/PHA-BE-005.md`; `PharmacyWorkflowTests` |
| `PHA-BE-006` | `FinancialClearance` pada response detail | **Selesai** | `task/report/backend/PHA-BE-006.md` |
| `PHA-FE-001` | UI routing sebelum reservasi | **Blocked — keputusan bisnis** | Bergantung `PHA-BE-003`; acceptance belum pernah ditetapkan; resolver tidak punya endpoint |
| `PHA-FE-002` | Alasan penahanan terbaca di layar kerja | **Selesai** | `FR-PHA-CLR-10`; `prescription-clearance-utils.js`, `prescription-financial-hold-alert.jsx`; `pharmacy-financial-clearance-notice.test.mjs` (12) |

## Task turunan

| Task ID | Outcome | Diturunkan dari | Status | Bukti |
|---|---|---|---|---|
| `PHA-D-01` | Telaah, penyiapan, telaah akhir | `contracts/state-transition-matrix.md` | **Selesai** | `PharmacyWorkflowTests` (33); runtime 12 Okt; perbaikan `bug-pha-be-002` (4 uji regresi) |
| `PHA-D-02` | Penyerahan obat | `contracts/api-contract.md`, `flowcharts/00-alur-utama.md` | **Selesai** | `DispenseFlowTests` (18), `DispensingGuardTests` (20); runtime: stok 570 → 561 |
| `PHA-D-03` | Retur obat | `bug-pha-be-003`, `contracts/validation-matrix.md` | **Selesai** | `DrugReturnTests` (48), `DrugReturnUpdateAndListTests` (34); perbaikan batas retur (13 uji regresi) |
| `PHA-D-04` | Etiket dan stok | `contracts/api-contract.md` | **Selesai** | `LabelAndStockTests` (14); runtime: mutasi `DrugUsage` −9 |
| `PHA-D-05` | Routing depo pada workflow | `contracts/PHA-DEPOT-ROUTING-v1.md`, `erd/pharmacy-routing.md` | **Selesai** | `DepotRoutingTests` (26); depo `DSL-RSMMC-00001` dipakai saat penyerahan |
| `PHA-D-06` | Matriks izin seluruh controller | `contracts/permission-audit-matrix.md` | **Selesai** | `PermissionMatrixTests` (18) atas 19+ controller |
| `PHA-D-07` | Konsolidasi kode modul izin | `temuan-modul-izin-terbelah.md` | **Selesai** | `ModuleCodeConsolidationTests` (16) |
| `PHA-D-08` | Tangga tahap pemenuhan resep | `contracts/state-transition-matrix.md` | **Selesai** | `PrescriptionFulfillmentStageTests` (8); runtime: `1/1/1` → `2/2/1` |
| `PHA-D-09` | Kontrak tagihan tahap 1 | `requirement-billing-charge-pra-dispense.md`, `RJ-E2E-DEC-005` | **Selesai** | runtime 14 Okt: `PRESCRIBED` / `BIL-INTEGRATION-1.3` tersinkron |
| `PHA-D-10` | UI alur kerja farmasi tengah | `gap-frontend-workflow.md` (`GAP-PHA-FE-001`) | **Selesai** | 1 Okt: tangga tahap 4→4→5→6→12→**7 Siap Diserahkan** seluruhnya `200`; penelusuran peramban **12 pemeriksaan lulus**; `bin/uji-alur-farmasi.sh` |
| `PHA-D-11` | Gerbang stok pada finalisasi resep | `PHA-DEC-018` | **Selesai** | `PrescriptionStockAvailabilityTests` (8 uji, 8 lulus); `PrescriptionValidationService.ValidateStockAvailabilityAsync` |

**Rekap: 19 task, 17 selesai, 0 sedang proses, 2 blocked keputusan bisnis, 0 belum mulai.**

Memakai rumus rekap 23 modul (`Sisa = Total − Selesai`, blocked ikut penyebut): **17/19 = 89,5%**.
Bila blocker keputusan bisnis dikeluarkan dari penyebut: **17/17 = 100%**.

## `PHA-D-11` — keputusan yang disahkan tetapi gerbangnya belum terpasang

`PHA-DEC-018` disahkan product owner **19 Agustus 2026**: *"Penyimpanan resep gagal jika jumlah
obat melebihi available stock; dokter harus memilih obat/jumlah pengganti."* Gerbangnya baru
terpasang **15 Oktober 2026**. Selama rentang itu `PrescriptionValidationService` tidak memiliki
satu pun rujukan stok — dari 16 service resep, hanya `PrescriptionDispensingService` yang
menyentuh stok, yaitu pada langkah paling akhir.

Akibatnya resep dapat difinalkan dan **dibayar** untuk obat yang stoknya tidak mencukupi, lalu
gagal di meja farmasi ketika penyerahan mencoba menahan stok. Gerbang ini memindahkan penolakan
ke titik yang masih dapat diperbaiki dokter.

| Hal | Isi |
|---|---|
| Kode temuan | `INSUFFICIENT_DRUG_STOCK`, severity `Error` |
| Pesan | *"Stok {obat} tidak mencukupi: diminta {n}, tersedia {m}. Pilih obat atau jumlah pengganti."* |
| Titik penegakan | `ValidateForConsultationAsync` → `ConsultationValidationService` → `ConsultationFinalizationService` (`ErrorCount > 0` ⇒ finalisasi ditolak) |
| Cakupan | resep berstatus `Draft` saja, sama seperti `ValidateHeader` dan gerbang penanda keselamatan |
| Definisi stok tersedia | `QuantityOnHand − QuantityReserved` atas saldo `DrugStockStatus.Available` — **identik** dengan yang dipakai penyerahan |
| Penjumlahan | butir biasa (`Quantity`) + bahan racikan (`TotalQuantity`) atas obat yang sama dijumlahkan |

**Yang sengaja tidak ditambahkan**, karena akan menjadi aturan baru yang belum diputuskan
(`PHA-OQ-002`):

- Tanggal `ExpiryDate` batch tidak diperiksa. Status `Expired`, `Quarantine`, dan `Damaged`
  dengan sendirinya tidak terhitung, tetapi saldo yang statusnya masih `Available` padahal
  batch-nya lewat tanggal tetap terhitung — persis seperti pada penyerahan.
- Stok dijumlahkan **lintas depo**. Pemilihan depo adalah `PHA-BE-003` yang masih tertahan
  `PHA-OQ-014`, jadi gerbang ini hanya menjawab pertanyaan yang sudah pasti: apakah rumah sakit
  memiliki obat sebanyak itu sama sekali.

### Catatan cara menjalankan ujinya

`PharmacyTests` masih tidak dapat dikompilasi karena **17 error** pada tiga berkas uji milik
Billing/Finance; berkas Farmasi nol error. Kedelapan uji `PHA-D-11` karena itu dijalankan lewat
proyek sementara di luar repo yang mengompilasi hanya `Infrastructure/` dan
`PrescriptionStockAvailabilityTests.cs`. Hasilnya **8 lulus, 0 gagal**. Begitu blocker Billing
hilang, uji ini ikut terbawa `dotnet test` biasa tanpa perubahan apa pun.

## Dependency eksternal — bukan pekerjaan internal Farmasi

| Hal | Pemilik | Dampak |
|---|---|---|
| `PharmacyTests` tidak dapat dikompilasi | **Billing / Finance** | **17 error** pada 3 berkas uji milik mereka di dalam proyek uji Farmasi per 15 Okt (sebelumnya 34); **nol** error pada berkas Farmasi. 544 uji tertulis karena itu tidak dapat dijalankan lewat `dotnet test` biasa |
| Master tarif jasa konsultasi tidak ada | **Billing** | Nol baris `MstTariff` dengan `IsConsultationFee = true`; invoice tertahan `BIL-FIN-021` |
| `LAB-OPEN-012` | **Lab** | Menahan penutupan E2E |
| `PHA-OQ-014` | **Product** | Menahan `PHA-BE-003` dan `PHA-FE-001` |

`PrescriptionBillingChargeProducer.cs` dihapus integration dan isinya diserap
`BillingChargeSourceAdapter` milik Billing. Kontrak `PRESCRIBED` / `BIL-INTEGRATION-1.3` tetap
berlaku dan terbukti runtime, jadi `PHA-D-09` tetap selesai; kepemilikan kodenya saja yang pindah.
