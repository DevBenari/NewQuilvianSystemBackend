# Laporan Perubahan Backend — `BE-MCU-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-003` |
| Judul | Kolom `CoveringMcuOrderId` pada pesanan Lab dan Rad |
| Slice | `MVP-0` — fondasi data `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-003`* |
| Trace | `FR-MCU-032` (fondasi); `MCU-DEC-005`, `MCU-DEC-022`; `MCU-COORD-002` ✅ |
| Contract version | `02-backend-architecture.md` §5.4, §7, §8 urutan 2; [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) baris `LabOrder`/`RadOrder` dan DDL bagian 6 (`MCU-BP-001` r1, `approved` 2026-10-21) |
| Dependency | `BE-MCU-002` ✅ (tabel `McuOrder`) |
| Klasifikasi | `MEDIUM` — skor 4: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), dampak schema/migration (2); tanpa endpoint, tanpa perubahan perilaku |
| Task mode | `BACKEND` |
| Target tulis | Backend: `LabOrder.cs`, `LabOrderConfiguration.cs`, `RadOrder.cs`, `RadOrderConfiguration.cs`, `Migrations/` (berkas migration saja), uji MCU, laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — sama dengan saat `BE-MCU-002`; impact scan atas pergeseran dari snapshot manifest `4d3e4f98` menunjukkan model dan configuration `LabOrder`/`RadOrder` tidak berubah di rentang itu |
| Tanggal | 2026-10-22 |
| Status | Selesai — ketiga acceptance criteria terbukti. Migration **belum** diterapkan ke basis data mana pun |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` (prefix `Lab`) dan `RadiologyManagement` (prefix `Rad`) sebagai tabel yang disentuh; FK menunjuk `MedicalCheckupManagement` (prefix `Mcu`, registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` — entity Lab/Rad yang sudah ada mendapat satu kolom; tidak ada perbaikan legacy lain yang ikut dikerjakan |
| Wewenang lintas modul | Pemilik `laboratorium` dan `radiologi` menyetujui kolom ini 2026-10-21 (`MCU-COORD-002`, `MCU-EVD-002` bagian 1) |
| QBE yang berlaku | `QBE-ENT-002` (`Guid?` sesuai semantik "boleh tidak dicakup paket"), `QBE-ENT-003` (bukan kolom presentasi — ia menentukan penahanan tagihan), `QBE-CFG-002` (mapping di configuration pemilik, bukan anotasi), `QBE-DTO-001` (tidak ada DTO yang memuat kolom ini), `QBE-NAM-001` (tanpa `Trx*`) |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` di folder tak terdaftar terkena `QBE-CFG-001` dan `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Pemeriksaan Lab dan Radiologi yang termasuk paket MCU sudah dibayar sekali sebagai harga paket. Bila
pesanan Lab/Rad tidak tahu bahwa dirinya bagian dari paket, Lab dan Radiologi akan menerbitkan tagihan
per pemeriksaan seperti biasa, dan peserta tertagih dua kali.

**Contoh.** Peserta membeli *Paket Basic* Rp 350.000 yang berisi Darah Lengkap dan Thorax. Tanpa
penanda, saat spesimen Darah Lengkap diterima dan Thorax diakuisisi, Lab dan Radiologi masing-masing
mengirim tagihan sendiri — total tagihan bisa mencapai Rp 760.000 untuk paket yang harganya Rp 350.000.
Task ini membuat tempat penandanya: kolom `CoveringMcuOrderId` pada pesanan Lab dan Rad. Pemakaian
penanda untuk menahan tagihan dikerjakan `BE-MCU-020`.

---

## 2. Proses bisnis

Task ini tidak mengubah perilaku apa pun. Urutan yang akan memakai kolom ini:

1. Petugas MCU menekan *Mulai pemeriksaan* (`BE-MCU-021`).
2. MCU membuat pesanan Lab per disiplin dan pesanan Rad per komponen, dan mengisi `CoveringMcuOrderId`
   dengan order MCU peserta (`BE-MCU-020`).
3. Saat spesimen diterima atau citra diakuisisi, Lab dan Rad membaca kolom itu. Terisi → tidak
   menerbitkan fakta tagih (`MCU-DEC-022`). Kosong → ditagih seperti sekarang.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Pesanan lama, sebelum kolom ini ada | Bernilai kosong → tetap ditagih seperti sekarang; tidak ada baris yang ditulis ulang |
| Pesanan biasa poliklinik, IGD, rawat inap | Tidak pernah mengisi kolom → perilaku tagih tidak berubah |
| Pemeriksaan tambahan di luar paket (`MCU-DEC-036`) | Dipesan tanpa penanda → ditagih biasa |
| Order MCU dihapus padahal masih mencakup pesanan | Ditolak basis data (FK `Restrict`) |
| Pihak luar mencoba mengirim kolom lewat API Lab/Rad | Tidak mungkin: tidak ada DTO maupun action yang memuatnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`02-backend-architecture.md` §5.4/§7/§8, `erd/data-dictionary.md` (baris `LabOrder`/`RadOrder` dan DDL
bagian 6), `LabOrder.cs`, `RadOrder.cs`, `LabOrderConfiguration.cs`, `RadOrderConfiguration.cs`,
`McuOrder.cs`, `McuOrderComponentConfiguration.cs` (FK arah sebaliknya, agar tidak bentrok),
`IdentityModel.cs`, harness uji MCU dan Radiologi, `RadOrderService.cs` (konstruktor), kontrak
engineering dan registry.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Tambah `Guid? CoveringMcuOrderId` beserta penjelasan |
| `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs` | Tambah `Guid? CoveringMcuOrderId` beserta penjelasan |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Kolom boleh kosong, FK `Restrict` ke `McuOrder` tanpa navigation, index |
| `Repositories/Configurations/HealthServices/RadiologyManagement/RadOrderConfiguration.cs` | Sama dengan Lab |
| `Migrations/20261022021727_AddCoveringMcuOrderToLabRadOrders.cs` + `.Designer.cs` (baru) | 2 `AddColumn` boleh kosong, 2 `CreateIndex`, 2 `AddForeignKey`; `Down()` membalik keenamnya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Hanya penambahan kolom, index, dan FK di atas (dibandingkan dengan model `BE-MCU-002`) |
| `Tests/QuilvianSystemBackend.McuTests/CoveringMcuOrderColumnTests.cs` (baru) | 7 uji: bentuk kolom/FK/index, nilai kosong untuk pesanan yang tidak mengisinya, larangan DTO dan action |
| `Tests/QuilvianSystemBackend.RadiologyTests/` (4 berkas) | Penyesuaian harness: konstruksi `RadOrderService` diberi argumen `clinicalMilestoneFactProducer: null!` (lihat §7) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint, DTO, maupun respons yang berubah |
| Database | Dua kolom `uuid NULL` tanpa nilai bawaan pada `LabOrder` dan `RadOrder`; PostgreSQL menambah kolom seperti ini tanpa menulis ulang tabel. **Migration belum diterapkan**; ia bergantung pada migration `AddMedicalCheckupManagement` (`BE-MCU-002`) yang juga belum diterapkan |
| Keamanan/Auth | Tidak ada. Kolom tidak dapat ditulis dari luar |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task skema tanpa endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | Berhasil, 0 error (87 detik; diulang sesudah migration, berhasil) | `PASS` | Keluaran build |
| `dotnet ef migrations add AddCoveringMcuOrderToLabRadOrders --no-build` | Isi hanya 2 kolom, 2 index, 2 FK `Restrict` | `PASS` | §3.2 |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration" | `PASS` | Keluaran perintah |
| Snapshot dibandingkan dengan model `20261021073245_AddMedicalCheckupManagement.Designer.cs` | Selisih hanya 2 properti, 2 index, 2 FK `CoveringMcuOrderId` | `PASS` | `diff` |
| `dotnet ef migrations script AddMedicalCheckupManagement AddCoveringMcuOrderToLabRadOrders --no-build` (offline) | 6 pernyataan; terlampir di §8 | `PASS` | Lampiran |
| Pencocokan SQL ↔ DDL kamus bagian 6 | Nama kolom, tipe `uuid`, nama index `IX_*_CoveringMcuOrderId`, nama FK `FK_*_McuOrder_CoveringMcuOrderId`, `ON DELETE RESTRICT` — sama persis; tidak ada `DEFAULT` maupun `UPDATE` | `PASS` | §8 |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 24 lulus (17 lama + 7 baru) | `PASS` | `Passed! - Failed: 0, Passed: 24` |
| `dotnet test Tests/QuilvianSystemBackend.RadiologyTests` (sebelum penyesuaian harness) | Gagal kompilasi `CS7036` di 5 titik: `RadOrderService` meminta `clinicalMilestoneFactProducer` | `EXISTING / ENVIRONMENT ISSUE` | Parameter itu masuk lewat commit `096d2282` (merge); `RadOrderService.cs` tidak berubah di working tree |
| `dotnet test Tests/QuilvianSystemBackend.RadiologyTests` (sesudah penyesuaian harness) | 106 lulus | `PASS` | `Passed! - Failed: 0, Passed: 106` |
| Uji Lab yang ada | Repository tidak memiliki proyek uji Lab | `NOT RUN` | `find -name '*.csproj'`: enam proyek uji, tidak satu pun Lab. Sebagai gantinya: build, `has-pending-model-changes`, dan pembentukan model EF penuh (termasuk `LabOrder`) di SQLite pada uji MCU |
| Checker QBE `-Path` (5 berkas task + kontrol) | Berkas task nol temuan; kontrol `ZzzKontrol` terkena `QBE-CFG-001`/`QBE-MOD-002` | `PASS` | Keluaran checker |

Uji manual: `NOT APPLICABLE` — tidak ada perilaku pengguna.

**Tidak dijalankan:** penerapan migration ke devYoga atau basis data lain (di luar wewenang task).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Seluruh baris lama bernilai `NULL` dan tidak ditulis ulang | Terpenuhi | SQL: `ADD "CoveringMcuOrderId" uuid` tanpa `DEFAULT`, tanpa `UPDATE`; model: properti boleh kosong tanpa nilai bawaan (uji `Kolom_boleh_kosong_berindex_dan_FK_Restrict_ke_McuOrder`); pesanan yang ditulis tanpa kolom ini terbaca `NULL` (uji `Pesanan_yang_ditulis_tanpa_kolom_ini_tetap_NULL`) |
| (2) Tidak ada endpoint Lab/Rad yang menerima kolom ini dari luar | Terpenuhi | Uji refleksi: tidak satu pun tipe di namespace `*.DTOs` Lab/Rad memuat `CoveringMcuOrderId`; tidak satu pun action controller Lab/Rad mengikat entity `LabOrder`/`RadOrder` langsung. `grep` kolom di `Areas/` hanya menemukan model dan configuration |
| (3) Uji Lab/Rad yang ada tetap lulus | Terpenuhi untuk Radiologi; Lab tidak punya proyek uji | Radiologi 106 lulus sesudah penyesuaian harness yang tidak berkaitan dengan task; untuk Lab lihat baris `NOT RUN` di §5 |
| DoD: kriteria terbukti; migration belum diterapkan kecuali diizinkan | Terpenuhi | Tidak ada eksekusi basis data |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Harness uji Radiologi tidak terkompilasi sejak merge `096d2282` menambah parameter `ClinicalMilestoneFactProducer` pada konstruktor `RadOrderService`. Disesuaikan dengan pola harness yang sudah ada (`null!` untuk dependency yang tidak dipakai uji itu); seluruh 106 uji lulus, artinya tidak satu pun uji melewati jalur fakta tagih. `Tests/` diabaikan `.gitignore`, sehingga penyesuaian ini hanya ada di mesin lokal kecuali di-`git add -f` |
| Masalah yang diketahui | Tidak ada proyek uji Lab; regresi Lab pada task ini bertumpu pada build dan pembentukan model EF |
| Risiko tersisa | Migration ini memuat FK ke `McuOrder`, sehingga wajib diterapkan **sesudah** `AddMedicalCheckupManagement`, dan bila dibalik harus dibalik **sebelum** migration itu. Kolom belum dipakai siapa pun — penahanan tagihan baru berlaku di `BE-MCU-020` |
| Perubahan sampingan | Kontrol negatif QBE `Areas/HealthServices/ZzzControlManagement/` dibuat dan dihapus pada langkah yang sama; `Test-Path` memastikan tidak tersisa |
| Interupsi | `NONE` (skrip SQL offline pertama gagal karena migration baru belum terkompilasi untuk `--no-build`; berhasil sesudah build ulang) |
| Status Git | Perubahan task: ` M` `LabOrder.cs`, `RadOrder.cs`, `LabOrderConfiguration.cs`, `RadOrderConfiguration.cs`, `ApplicationDbContextModelSnapshot.cs`; `??` dua berkas migration `20261022021727_*`; laporan ini; tanda roadmap/traceability. Perubahan `BE-MCU-001`/`002` yang belum di-commit tetap ada dan tidak disentuh |
| Langkah berikutnya | `BE-MCU-020` kini terbuka (bergantung pada task ini). `BE-MCU-004`, `BE-MCU-005`, `BE-MCU-010` tetap siap. Penerapan migration `BE-MCU-002` + `003` ke devYoga menunggu izin pemilik |

---

## 8. Lampiran — skrip SQL migration offline

Dihasilkan `dotnet ef migrations script AddMedicalCheckupManagement AddCoveringMcuOrderToLabRadOrders --no-build`.
**Bukan untuk dijalankan langsung**: penerapan memakai mekanisme migration proyek atas izin pemilik.

```sql
START TRANSACTION;
ALTER TABLE public."RadOrder" ADD "CoveringMcuOrderId" uuid;

ALTER TABLE public."LabOrder" ADD "CoveringMcuOrderId" uuid;

CREATE INDEX "IX_RadOrder_CoveringMcuOrderId" ON public."RadOrder" ("CoveringMcuOrderId");

CREATE INDEX "IX_LabOrder_CoveringMcuOrderId" ON public."LabOrder" ("CoveringMcuOrderId");

ALTER TABLE public."LabOrder" ADD CONSTRAINT "FK_LabOrder_McuOrder_CoveringMcuOrderId" FOREIGN KEY ("CoveringMcuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT;

ALTER TABLE public."RadOrder" ADD CONSTRAINT "FK_RadOrder_McuOrder_CoveringMcuOrderId" FOREIGN KEY ("CoveringMcuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261022021727_AddCoveringMcuOrderToLabRadOrders', '9.0.18');

COMMIT;
```
