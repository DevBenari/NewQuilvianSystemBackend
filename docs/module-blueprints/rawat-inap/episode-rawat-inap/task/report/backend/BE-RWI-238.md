# Laporan Perubahan Backend — `BE-RWI-238`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-238` |
| Judul | Monitoring membaca denah satu lantai |
| Slice | Satu panggilan per lantai memberi status kamar, badge, outline, ringkasan HD/LD, dan bed dengan enam status, dengan identitas tersamar sesuai hak |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-238` |
| Trace | FR-DNH-001, 002, 014; RWI-DEC-287, 299, 303, 316, 318, 320; RWI-AC-442, 443, 453 (Monitoring), 461; VAL-DNH-11, 20; GUARD-DNH-01, 05; INV-DNH-03; NFR-802, 803; DNH-AT-442, 443, 453, 904, 910 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | `BE-RWI-232` ✅, `BE-RWI-234` ✅ |
| Klasifikasi | `HEAVY` — skor 10: satu repo 0, diperiksa > 20 = 2, diubah 6 = 1, logika kompleks 2, kontrak baru 2, query existing 1, privasi terkait 1, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | `InpFloorPlanQueryService.cs`, `InpBedManagementQueryService.cs`, `InpatientFloorPlanDtos.cs`, `InpatientBedManagementDtos.cs`, `InpatientBedManagementController.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source dan build terbukti; data denah nyata menunggu D-DNH-3 |

---

## 1. Masalah yang diperbaiki

Monitoring dikelompokkan per unit tanpa saring lantai, tidak punya status kamar maupun badge, dan `HoldingContext` hanya memuat nomor episode dan nama. Denah per lantai tidak dapat digambar dari satu panggilan.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Petugas melihat denah satu lantai: status kamar, badge kapasitas/terisi, outline kamar yang masih bisa diisi, dan ringkasan HD/LD |
| Pelaku | Pemegang `InpatientBedManagement : Read` — perawat, kepala ruangan, Housekeeping (profil OperationalOnly) |
| Pemicu | Membuka Monitoring Bed atau berpindah lantai |

Langkah utama:

1. Pemesanan kedaluwarsa dalam scope digugurkan (perilaku existing Monitoring).
2. Server memuat kamar aktif berkunci dalam scope beserta bed aktifnya, lalu proyeksi enam status bed yang sama dengan Monitoring.
3. `Floors` merangkum jumlah kamar dan bed Tersedia per lantai. Tanpa `floorCode`, server memilih lantai dengan bed Tersedia terbanyak (seri → lantai berangka terkecil).
4. Untuk setiap kamar: status kamar, `Counts` bed biasa dan `CribCounts` boks rawat gabung, `HasAvailableCrib`, `IsSelectable`, `DependencySummary`, dan bed.
5. Identitas penghuni (nama, No. RM, HD/LD) mengikuti aturan penyamaran existing; profil OperationalOnly tidak menerima ketiganya maupun `DependencySummary`.

Aturan status kamar (dihitung atas bed biasa; kamar yang seluruh bed-nya boks dihitung seperti bed biasa):

| Urutan | Status | Syarat |
| --- | --- | --- |
| 1 | `Unavailable` (Tidak Tersedia) | Kamar ditutup untuk admisi, tanpa bed aktif, atau semua bed Tidak Tersedia |
| 2 | `Available` (Tersedia) | Ada bed Tersedia |
| 3 | `Preparing` (Persiapan) | Ada bed Menunggu atau Dalam Pembersihan |
| 4 | `Full` (Penuh) | Sisanya Terisi atau Dipesan — tanpa warna merah, tetap dapat diklik |

Badge: kapasitas = bed aktif yang bukan Tidak Tersedia; terisi = Terisi + Dipesan. `MstRoom.Capacity` **tidak** dipakai.

| Contoh kamar | Hasil |
| --- | --- |
| Bed Terisi/Dipesan/Tersedia | `Available`, `Counts` {3, 2, 1}, `IsSelectable` true |
| Bed Terisi/Dipesan/Terisi | `Full`, `Counts` {3, 3, 0}, `IsSelectable` false |
| Dua penghuni: satu High, satu Low | `DependencySummary` {1, 1, 0}, `IsHighDependency` true |
| Akun Housekeeping membuka kamar yang sama | `HoldingContext` null, `DependencySummary` null |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.7, API 14.2/14.3, transisi 12.1, hak akses 12.2; `InpBedManagementQueryService.cs`, `InpBedAccessService.cs` (`UnitsAsync`, `CanReadLoadedIdentityAsync`), `InpatientBedManagementController.cs`, `InpatientBedManagementDtos.cs`, `InpBedMutationCoordinator.ExpireScopedAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs` | Baru: `GetMonitoringAsync` (baris 55), muat data satu kali (lantai bawaan baris 145), status kamar (baris 191), ringkasan HD/LD (baris 221, 249) |
| `Areas/HealthServices/InPatientManagement/Services/InpBedManagementQueryService.cs` | `HoldingContext` + No. RM dan HD/LD (baris 156); HD/LD episode dimuat sekali per batch (`EpisodeCareDependency`, baris 182); saring `FloorCode` pada Monitoring (baris 206) |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientFloorPlanDtos.cs` | Baru: `FloorPlanResponse`, `FloorPlanRoomResponse`, `FloorPlanBedResponse`, dll. |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedManagementDtos.cs` | `BedMonitoringQuery.FloorCode`; `BedHoldingContext.MedicalRecordNumber`, `CareDependencyLevel` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedManagementController.cs` | `GET floor-plan` (baris 26) |
| `Program.cs` | DI `InpFloorPlanQueryService` (baris 717) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru; saring `floorCode` dan dua field `HoldingContext` aditif pada Monitoring dan konsumen proyeksi lain (`available-beds`, `transfer-context`) |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | Action existing `InpatientBedManagement : Read`; penyamaran `CanReadLoadedIdentityAsync` existing berlaku untuk No. RM dan HD/LD; `DependencySummary` hanya untuk pembaca yang boleh membaca identitas unit itu |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah satu lantai untuk Monitoring Bed | `InpatientBedManagement : Read` | `FloorPlanMonitoringQuery` {`floorCode?`} | `FloorPlanResponse` |
| `GET` | `/monitoring` | Monitoring per unit; kini dengan saring lantai dan No. RM/HD-LD penghuni | `InpatientBedManagement : Read` | `BedMonitoringQuery` + `floorCode?` | `BedMonitoringResponse` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Inp` ACTIVE; `NEW CODE` service/DTO; SVC-001, DTO-001, PERM-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| DNH-AT-442 | Status kamar dan badge dari status bed; `Capacity` master tidak dibaca | `PASS` | `InpFloorPlanQueryService.cs:191-219, 241-246` |
| DNH-AT-443 | Ringkasan {High, Low, Unassessed} dari episode penghuni bed Terisi | `PASS` | Baris 249–262 |
| DNH-AT-453 (Monitoring) | `IsSelectable` = ada bed biasa Tersedia | `PASS` | Baris 233–235 |
| DNH-AT-904 privasi | `HoldingContext` (nama, No. RM, HD/LD) hanya bila `CanReadLoadedIdentityAsync`; `DependencySummary` null untuk OperationalOnly | `PASS` | `InpBedManagementQueryService.cs:150-158`; `InpFloorPlanQueryService.cs:202-221` |
| DNH-AT-910 beban query | Jumlah query tetap: kamar, bed, proyeksi batch (kamar/unit/kelas/kesiapan/penempatan/pesanan/episode) — tidak per bed; cek hak akses di-cache per permintaan | `PASS` (penelusuran source) | `LoadAsync` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — DB Hamzah belum punya kamar berkunci (impor D-DNH-3 belum dijalankan) dan backend lokal belum dijalankan ulang.

**Tidak dijalankan:** pengukuran jumlah query runtime dan contract test JSON profil Housekeeping.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Terisi/Dipesan/Tersedia → `Available`, `Counts` {3, 2, 1}, `IsSelectable` true; Terisi/Dipesan/Terisi → `Full` | Terpenuhi | Status kamar dan `Count` |
| 1 HD dan 1 LD → `DependencySummary` {1, 1, 0}, `IsHighDependency` true | Terpenuhi | `Dependency` |
| Tanpa `floorCode`, lantai dengan bed Tersedia terbanyak dikembalikan beserta ringkasan seluruh lantai | Terpenuhi | `LoadAsync` baris 145–152 |
| OperationalOnly: JSON tanpa nama, No. RM, HD/LD, dan `DependencySummary` | Terpenuhi | Penyamaran existing + gerbang unit |
| Kapasitas tidak dari `MstRoom.Capacity`; jumlah query per lantai tetap | Terpenuhi | Bagian 5 |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Temuan hardcode existing (tidak diubah): pintas SuperAdmin di `InpBedAccessService` memakai `IsInRole("SuperAdmin")` dan `UserType == 1` |
| Masalah yang diketahui | Hitungan boks pada badge menunggu keputusan RWI-OQ-144 di sisi tampilan; API sudah memisahkan `Counts` dan `CribCounts` |
| Risiko tersisa | `Floors` dihitung dari proyeksi seluruh lantai dalam scope; untuk 168 bed tetap satu batch |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | `FE-RWI-246`, `FE-RWI-247` |
