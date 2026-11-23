# Laporan Perubahan Backend — `BE-HRD-448`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-448` |
| Judul | Kerangka Dashboard SDM, hak akses, dan panel ringkasan pegawai |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-73` · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-448` |
| Trace | `FR-PKB-01` (panel 1–3), `FR-PKB-04`; `HRD-DEC-235`, `HRD-ASM-88`, `HRD-ASM-89`, `HRD-ASM-90` |
| Contract version | `v29` — API 35.1; State 33.1; Permission 33.1–33.2 — **approved** 23 November 2026 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — controller, service, DTO, konstanta baru; hak akses baru |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/**`, `Program.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 5/5 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`, registri `IsValid=True` dengan `HrdWorkforceDashboard.Read`; uji API DB dev `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | Human Resource, `Hrd`; tanpa entity baru |
| Keberlakuan | `NEW CODE` (controller, service, DTO, konstanta; arketipe transaksi **read-only**) |
| QBE yang berlaku | `QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-VAL-001`, `QBE-LOG-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0; `transaction-endpoint-standard.md` (read-only); `role-access-rules.md` |

---

## 1. Masalah yang diperbaiki

Manajer SDM tidak punya satu halaman ringkasan tenaga kerja. Untuk mengetahui jumlah pegawai aktif, yang masuk dan keluar bulan ini, serta komposisi profesi, HR harus membuka beberapa layar dan menghitung sendiri.

## 2. Proses bisnis

1. Rina (Manajer SDM, memegang `HrdWorkforceDashboard : Read`) membuka Dashboard SDM.
2. Sistem menghitung saat itu juga (tanpa snapshot, `HRD-ASM-89`) dengan tanggal operasional WIB:
   - **Pegawai aktif**: `MstEmployee.IsActive = true`, tidak terhapus. Pegawai yang sedang skorsing tetap aktif, karena skorsing hanya mengunci akun login (`HRD-ASM-90`).
   - **Masuk bulan ini**: `JoinDate` di bulan operasional berjalan (aktif maupun tidak).
   - **Keluar bulan ini**: `ResignDate` di bulan operasional berjalan.
   - **Distribusi profesi**: pegawai aktif per profesi; tanpa profesi = "Belum diisi"; urut jumlah terbanyak.
   - **Aktif / Nonaktif**: menurut `IsActive`.
3. Akun tanpa hak → `403` (penegakan atribut standar).

**Contoh berangka.** 3 pegawai aktif (dr. Dodi masuk 2 Desember 2026), 1 pegawai nonaktif (Budi keluar 10 Desember 2026); dibuka 20 Desember 2026 → `activeCount = 3`, `newThisMonthCount = 1`, `leftThisMonthCount = 1`, `inactiveCount = 1`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdEmployee360Controller.cs` (pola atribut), `MasterData/Workforce/Models/MstEmployee.cs`, `MasterData/CompetencyAndCredential/Models/MstProfession.cs`, `Helpers/AppDateTimeHelper.cs`, `Shared/HumanResource/Helpers/HrCrossEmployeeLookup.cs`, `EmployeeRelationManagement/Workers/EmployeeRelationWorkers.cs` (skorsing menonaktifkan akun, bukan `MstEmployee`), `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdWorkforceDashboardController.cs` (baru) | Route `api/v1/corporate/human-resource/workforce-dashboard`, `[AccessController]` `HUMAN_RESOURCE_WORKFORCE_CORE` / `HrdWorkforceDashboard`, `GET summary` (dan `GET drilldown`, lihat `BE-HRD-451`), `[AccessAction("Read")]` + `[AccessPermission("HrdWorkforceDashboard","Read")]` |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/IHrdWorkforceDashboardService.cs` (baru) | Interface dan hasil drill-down |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdWorkforceDashboardService.cs` (baru) | Panel 1–3 (`AsNoTracking`, batas bulan UTC dari tanggal operasional), distribusi profesi |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/HrdWorkforceDashboardDtos.cs` (baru) | `WorkforceDashboardSummaryResponse` beserta sub-DTO, `WorkforceDashboardDrilldownQuery`, `WorkforceDashboardDrilldownItem` |
| `Areas/Corporate/HumanResource/WorkforceCore/Constants/WorkforceDashboardPanels.cs` (baru) | Kode panel, kelompok, jenis dokumen, validasi kelompok |
| `Program.cs` | `AddScoped<IHrdWorkforceDashboardService, HrdWorkforceDashboardService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /workforce-dashboard/summary` sesuai kontrak 35.1 |
| Database | Tidak ada; baca-saja |
| Keamanan/Auth | Hak baru `HrdWorkforceDashboard : Read` — wajib diberikan admin ke Manajer SDM dan Admin HRD. Respons tanpa gaji, NIK, disiplin, atau data kesehatan |

**Delta:** batas bulan dan tanggal operasional memakai `AppDateTimeHelper.OperationalDate()` (WIB) yang diubah menjadi batas UTC tengah malam, karena kolom tanggal pegawai berjenis `timestamptz` dan aplikasi tidak memakai mode timestamp lama Npgsql.

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Lima panel Dashboard SDM | `HrdWorkforceDashboard : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` penuh Paket B | 0 error, 611 warning; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Validator registri hak akses | `IsValid=True`, 2.662 identitas; `HrdWorkforceDashboard.Read (Read)` terdaftar; 0 endpoint tanpa hak | `PASS` | Skrip `dotnet fsi` |
| Penelusuran: skorsing | `ItAccessRevocationSyncWorker` menonaktifkan `ApplicationUser`, bukan `MstEmployee` → tetap dihitung aktif | `PASS` | `EmployeeRelationWorkers.cs` baris 376–450 |
| Uji API `/summary` di DB dev | Belum dijalankan | `NOT RUN` | Butuh izin menyalakan backend |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. 3 aktif (1 masuk bulan ini), 1 nonaktif keluar bulan ini → 3/1/1/1 (`AT-HRD-PKB-01`) | Terpenuhi | Empat hitungan pada `GetSummaryAsync` |
| 2. 2 perawat, 1 dokter, 1 tanpa profesi → tiga baris dengan "Belum diisi", urut terbanyak (`AT-HRD-PKB-02`) | Terpenuhi | `GetProfessionDistributionAsync` |
| 3. Pegawai skorsing tetap aktif (`AT-HRD-PKB-03`) | Terpenuhi | Hitungan memakai `MstEmployee.IsActive` |
| 4. Tanpa hak → `403`; hak tampil di Akses Role; registri bersih (`AT-HRD-PKB-09` bagian backend) | Terpenuhi | Atribut akses; snapshot registri |
| 5. Respons tanpa gaji, NIK, disiplin, kesehatan | Terpenuhi | DTO hanya memuat angka dan nama profesi |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Pegawai yang sudah keluar tetapi masih `IsActive = true` akan terhitung aktif — masalah data, tidak dikoreksi di task ini |
| Risiko tersisa | Hak `HrdWorkforceDashboard : Read` belum diberikan ke peran mana pun |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `BE-HRD-449`, `BE-HRD-450` |
