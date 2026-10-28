# Laporan Perubahan Backend — `RJ-DOC-REV-BE-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-022` |
| Judul | Katalog resep berbasis stok depo |
| Slice | Amendment SF — Katalog Resep dari stok Farmasi Rawat Jalan |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian 21.1 |
| Trace | `RJ-DOC-DEC-084`..`088`, fakta `F-SF-1`..`4` (`00-interview-decisions.md`, *Amendment SF*) |
| Contract version | Kontrak bagian 21.0 roadmap (parameter opsional `fromPharmacyStock`), disetujui `RJ-DOC-DEC-087`/`088` |
| Dependency | Data `DEC-084`..`086` sudah dieksekusi `2026-10-28` |
| Klasifikasi | `MEDIUM` — skor 6: dua repository (2), 9–20 berkas diperiksa (1), 3 berkas diubah (0), logika sedang (1), kontrak diperluas tanpa merusak (1), query persistence yang ada (1), keamanan tidak berubah (0), UI satu workflow (0) |
| Task mode | `CROSS-REPO MODE`, target tulis backend (task ini) |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/ClinicalManagement/**`, `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs`, dokumen blueprint `rawat-jalan` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `0415bfc0` (branch `sukmagp`) |
| Tanggal | 2026-10-28 |
| Status | ✅ Selesai, belum di-commit |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (controller katalog) dan `PharmacyManagement` (pembacaan stok) |
| Pemilik / prefix registry | `Cli` dan `Phm`, keduanya `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` (controller dan DTO lama); method baru di `DrugStockService` adalah `NEW CODE` |
| Entity baru | Tidak ada |
| QBE berlaku | QBE-SVC-001 (pembacaan stok baru ada di `DrugStockService`, bukan query baru di controller), QBE-API-001, QBE-DTO-001, QBE-PERM-001 (atribut akses lama dipertahankan), QBE-VAL-001, QBE-PAGE-001 |

---

## 1. Masalah yang diperbaiki

Tab Resep Dokter Rawat Jalan menampilkan seluruh obat di master obat (3.015 obat berkode `OBT`), tanpa melihat isi
gudang Farmasi. Dokter dapat memilih obat yang stoknya kosong. Kegagalan baru terlihat saat resep difinalkan,
ketika reservasi stok menolak dengan pesan "Stok … tidak mencukupi di depo pelayanan".

Contoh: dokter memilih *Paracetamol 500 mg* lalu menyimpan resep. Bila depo Farmasi Rawat Jalan sedang kosong, resep
gagal ditahan dan dokter harus mengulang. Sesudah perubahan, katalog sudah menunjukkan "stok 1.290" atau "stok habis"
sebelum dokter memilih.

---

## 2. Proses bisnis

1. Dokter membuka tab Resep pada kunjungan Rawat Jalan.
2. Layar meminta katalog dengan `fromPharmacyStock=true`.
3. Backend menentukan depo lewat routing yang sama dengan reservasi resep (`PharmacyDepotRoutingService`): klinik
   kunjungan lebih dulu, lalu unit layanan. Untuk Rawat Jalan (`SU-OPD-001`), depo terpilih adalah
   `GDG-023 FARMASI RAWAT JALAN` (`RJ-DOC-DEC-085`).
4. Katalog hanya memuat obat yang punya saldo di depo itu. Filter lama (kode `OBT`, bukan alkes, dapat diresepkan,
   pencarian, racikan) tetap berlaku.
5. Untuk tiap obat di halaman, backend menghitung **stok tersedia** = jumlah `QuantityOnHand − QuantityReserved` pada
   baris berstatus `Available` dengan batch yang belum kedaluwarsa. Aturan ini sama dengan kandidat FEFO saat
   pengeluaran.
   Contoh: batch A 100 (ditahan 20), batch B 50 kedaluwarsa kemarin, batch C 30 berstatus karantina → stok tersedia
   `80`.
6. Stok tersedia `0` → `canSelect = false`, `readinessStatus = "OutOfStock"`, dan pesan pertama
   "Stok obat di FARMASI RAWAT JALAN habis." Obatnya tetap tampil.

Jalur tidak normal:

- Depo tidak dapat ditentukan (misalnya kunjungan Laboratorium) → `400` dengan pesan routing, contoh
  "Depo Farmasi untuk layanan pasien belum dikonfigurasi." Katalog master **tidak** ditampilkan sebagai gantinya.
- Konfigurasi depo ganda → `400` "Konfigurasi Depo Farmasi ganda. Hubungi administrator."
- Tanpa `fromPharmacyStock` (Rawat Inap, IGD, order alkes perawat) → perilaku lama identik, ruas stok bernilai `null`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `PrescribingDrugController.cs`;
`PrescribingDrugDtos.cs`; `DrugStockService.cs`; `PharmacyDepotRoutingService.cs`;
`PrescriptionStockReservationService.cs`; `PhmDrugStock.cs`; `PhmDrugBatch.cs`; `DrugStockEnums.cs`;
`DrugStorageLocationSeeder.cs`; `DrugStorageLocationController.cs` (aksi hapus); `Program.cs` (registrasi service);
konsumen frontend `prescribing-drug.service.js`, `use-doctor-drug-catalog.js`, `use-inpatient-drug-catalog.jsx`,
`order-alat-kesehatan-panel.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` | Bagian baru *1d*: `QueryStockedDrugIds` (query obat bersaldo di satu lokasi, dapat digabung), `GetAvailableQuantitiesAsync` (stok tersedia per obat, aturan FEFO), `GetStorageLocationNameAsync` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PrescribingDrugController.cs` | Parameter `fromPharmacyStock`; routing depo; filter obat bersaldo; penempelan stok dan penutupan pilihan lewat `ApplyStock`; log memuat `fromPharmacyStock` dan `stockLocationId`; metadata parameter di `filters/metadata` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PrescribingDrugDtos.cs` | `PrescribingDrugResponse` + `StockStorageLocationId`, `StockStorageLocationName`, `AvailableStockQuantity`, `IsStockAvailable` (semua nullable); `PrescribingDrugPagedResponse` + `StockStorageLocationId`, `StockStorageLocationName` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Penambahan saja: satu query parameter opsional dan enam ruas respons nullable. Tanpa parameter, respons sama (R3, R7) |
| Database | Tidak ada perubahan schema maupun migration. EF: "No changes have been made to the model since the last migration." Data `DEC-084`..`086` dieksekusi terpisah sebelum task (lihat bagian 7) |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessPermission("PrescribingDrug", "Read")]` tidak berubah; tidak ada data baru di luar stok depo |

**Delta terhadap kontrak bagian 21.0:** ruas `stockStorageLocationId` dan `stockStorageLocationName` juga ditaruh di
tingkat halaman (`PrescribingDrugPagedResponse`), supaya layar dapat menampilkan nama depo walaupun hasil pencarian
kosong.

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Prescribing Drug

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs?encounterId=&fromPharmacyStock=true` | Katalog obat resep dari stok depo Farmasi kunjungan, beserta stok tersedia per obat | `PrescribingDrug : Read` |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs/filters/metadata` | Metadata filter; kini mencantumkan `fromPharmacyStock` | `PrescribingDrug : Read` |

Contoh ruas tambahan pada satu item:

```json
{
  "drugCode": "OBT…",
  "drugName": "PARACETAMOL 500 MG TABLET",
  "stockStorageLocationId": "62fb8336-bdc9-82b3-e383-80bb512a122a",
  "stockStorageLocationName": "FARMASI RAWAT JALAN",
  "availableStockQuantity": 1290.0,
  "isStockAvailable": true,
  "canSelect": true
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -p:UseSharedCompilation=false -o <scratchpad>/be-build` | `Build succeeded`, 0 Error, 417 warning lama | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <3 berkas>` | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Keluaran perintah |
| `dotnet test` | Tidak ada project test (pola Bank Darah) | `NOT RUN` | — |
| Runtime HTTP ke backend uji `https://localhost:7185`, DB `QuilvianNewDevSukma` | **9/9 PASS** (tabel di bawah); uji hanya membaca | `PASS` | `rt_be022.py` di scratchpad sesi |

| ID | Skenario | Hasil |
| --- | --- | --- |
| R0 | Login SuperAdmin ke backend uji | PASS |
| R1 | Kunjungan RJ + `fromPharmacyStock=true` → `200`; depo `FARMASI RAWAT JALAN`; 25 item; stok tiap item sama dengan query baca-saja DB | PASS |
| R2 | `totalData` 3.015 = jumlah obat `OBT` resep yang bersaldo di depo (query DB) | PASS |
| R3 | Tanpa parameter → `200`; `totalData` 3.015 = master; ruas stok `null`; urutan item sama dengan R1 | PASS |
| R4 | Pencarian "paracetamol" + stok → stok tiap baris sama dengan DB (contoh PARACETAMOL 500 MG TABLET `1290`) | PASS |
| R5 | Bahan racikan (`isCompoundIngredientAllowed=true`) + stok → `200`, ruas stok terisi | PASS |
| R6 | Kunjungan Laboratorium + stok → `400` "Depo Farmasi untuk layanan pasien belum dikonfigurasi."; tanpa parameter `200` | PASS |
| R7 | Kunjungan IGD tanpa parameter → `200`, ruas stok `null` | PASS |
| R8 | Stok > 0 tidak mengubah `canSelect` dibanding katalog lama | PASS |

Uji manual: `REQUIRED` — owner menguji stok `0` sendiri (`RJ-DOC-DEC-086`).

**Tidak dijalankan:** skenario stok `0` dan obat tanpa saldo di depo pada runtime. Seluruh 7.862 obat sengaja diberi
stok lebih dari nol (`RJ-DOC-DEC-086`), dan membuat stok `0` hanya untuk uji akan menulis baris kartu stok permanen.
Kedua cabang dibuktikan lewat kode (bagian 6).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kunjungan Rawat Jalan + `fromPharmacyStock=true` → `200`, `stockStorageLocationName` = `FARMASI RAWAT JALAN`, `availableStockQuantity` sama dengan query baca-saja saldo | Terpenuhi | R1, R4 |
| 2. Saldo obat dibuat `0` → obat tetap tampil, `isStockAvailable=false`, `canSelect=false` | Terpenuhi lewat kode; runtime `NOT RUN` atas pilihan owner | `ApplyStock`: stok `≤ 0` → `IsStockAvailable=false`, `CanSelect=false`, pesan stok habis di urutan pertama. `QueryStockedDrugIds` tidak menyaring jumlah, jadi saldo nol tetap tampil. `GetAvailableQuantitiesAsync` mengembalikan `0` untuk obat tanpa baris layak |
| 3. Obat tanpa saldo di depo tidak tampil; `totalData` hanya menghitung obat bersaldo | Terpenuhi (cabang pengecualian lewat kode) | R2; filter `stockedDrugIds.Contains(x.Id)` dipasang sebelum `CountAsync` |
| 4. Tanpa parameter → respons sama dengan sebelum perubahan | Terpenuhi | R3, R6 (tanpa parameter), R7 |
| 5. Encounter tanpa depo → `400` pesan routing | Terpenuhi | R6 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Analyzer IDE melaporkan peringatan lama pada controller (jumlah parameter `GetDrugs`, kompleksitas `ApplyFilters`/`ApplySorting`); tidak ditangani karena di luar scope |
| Masalah yang diketahui | Reservasi resep (`PrescriptionStockReservationService`) tidak mengecualikan batch kedaluwarsa, sedangkan katalog mengecualikannya. Katalog bisa lebih ketat daripada reservasi, tidak sebaliknya. Dicatat, tidak diubah |
| Risiko tersisa | Depo Rawat Inap/IGD belum tertaut unit layanan; bila kelak layar itu ikut memakai `fromPharmacyStock`, katalognya akan `400` sampai tautan dibuat |
| Data yang dieksekusi sebelum task | `DEC-084`: dihapus permanen 5 alokasi, 5 item, 3 pemakaian obat, 1 reservasi, 8 mutasi, 3 saldo, 3 batch `TEST-RJE008-*`, lokasi `DSL-RSMMC-00001`. `DEC-085`: `GDG-023.ServiceUnitId = SU-OPD-001`. `DEC-086`: 7.862 saldo pembuka lewat `POST /drug-stock/opening-balance` (7.862 sukses, 0 gagal, 304 detik); total kartu stok = total saldo |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Areas/HealthServices/ClinicalManagement/Controllers/PrescribingDrugController.cs`, `M Areas/HealthServices/ClinicalManagement/DTOs/PrescribingDrugDtos.cs`, `M Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs`, `M docs/module-blueprints/rawat-jalan/00-interview-decisions.md`, `M docs/module-blueprints/rawat-jalan/roadmap/doctor-consultation-roadmap.md`, `M docs/module-blueprints/rawat-jalan/roadmap/requirement-traceability.md`, `?? docs/module-blueprints/rawat-jalan/task/report/backend/RJ-DOC-REV-BE-022.md` |
| Langkah berikutnya | `RJ-DOC-REV-FE-022`; restart backend dev 7184 agar memuat parameter baru |
