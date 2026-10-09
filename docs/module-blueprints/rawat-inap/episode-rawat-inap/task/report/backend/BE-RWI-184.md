# Laporan Perubahan Backend — `BE-RWI-184`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-184` |
| Judul | Laporan transfer ruangan (`P2`) |
| Slice | `POST-MVP` / `RWF-W7` (`P2`) |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-184` |
| Trace | `FR-RWF-087`; `RWI-DEC-205`, `214`, `220` (4); `INV-RWF-34`; `AC-RWF-086`, `100`; `UAT-RWF-20`; API 11.7; `VAL-RWF-90` |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-154` [IB] — ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 1, logika 1, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/{DTOs,Helpers,Services,Controllers}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001` (ekspor dicatat) |
| Wewenang | Source: ya. Database: tidak; tanpa paket NuGet baru |

---

## 1. Masalah yang diperbaiki

Kepala ruangan dan manajemen tidak punya laporan perpindahan pasien antarruang per periode, dan
koreksi salah catat penempatan tidak dapat dibedakan dari transfer sungguhan.

## 2. Proses bisnis

1. Pengguna dengan `InpatientReport : ReadRoomTransfer` memilih periode (wajib, paling lama 31 hari),
   saringan unit asal, unit tujuan, kelas, dan apakah koreksi ikut tampil (bawaan ya).
2. Laporan dibaca dari linimasa penempatan bed: **Transfer** = penempatan yang pendahulunya berakhir
   karena `Transfer`; **Koreksi** = penempatan dengan `CorrectsPlacementId`.
3. Contoh `UAT-RWF-20`: periode 1–7 Okt → baris Budi, Melati 302/2 kelas 2 → ICU 01, 3 Okt 10.15,
   alasan "Perburukan, butuh ventilator", dicatat Ns. Siti, jenis Transfer; baris koreksi kelas
   pasien lain berjenis Koreksi.
4. Pengguna dengan `InpatientReport : ExportRoomTransfer` mengekspor `.xlsx` dengan saringan yang
   sama; setiap ekspor dicatat logger (pelaku, periode, jumlah baris — tanpa isi baris).
5. Jalur tidak normal: periode kosong atau > 31 hari → 400 "Pilih periode paling lama 31 hari";
   tanpa permission laporan → 403 apa pun nama perannya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedPlacement.cs`, `InpBedPlacementEndReason.cs`, `InpBedOccupancyService.cs` (`TransferAsync`,
`ApplyPlacementCorrectionAsync`), `MstPatientClass.cs`, `MstRoom.cs`, `MstBed.cs`, berkas proyek
(`.csproj`, tidak ada pustaka Excel); kontrak 11.7.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/DTOs/InpatientReportDtos.cs` | Baru — `RoomTransferReportQuery`, `RoomTransferReportRow`, `RoomTransferEntryKinds` |
| `InPatientManagement/Helpers/InpXlsxWriter.cs` | Baru — penulis `.xlsx` minimal (`ZipArchive` + `XmlWriter`) |
| `InPatientManagement/Services/InpRoomTransferReportService.cs` | Baru — validasi periode, laporan, ekspor |
| `InPatientManagement/Controllers/InpatientReportController.cs` | Baru — dua endpoint |
| `Program.cs` | Registrasi `InpRoomTransferReportService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 11.7. **Delta:** baris memuat `PlacementId` (aditif); saringan `ClassId` mencocokkan kelas asal **atau** tujuan |
| Database | `NOT APPLICABLE` — baca saja |
| Keamanan/Auth | Permission baru `InpatientReport : ReadRoomTransfer`, `: ExportRoomTransfer` (`AccessType` Read) |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Report

Base URL: `api/v1/health-services/inpatient-management/reports`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/room-transfers` | Laporan per periode ≤ 31 hari, paging | `InpatientReport : ReadRoomTransfer` |
| `GET` | `/room-transfers/export` | Ekspor `.xlsx`, saringan sama tanpa paging; dicatat | `InpatientReport : ExportRoomTransfer` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff dan scope | Baca saja; tanpa dependensi baru | `PASS` | Daftar 3.2 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi API dengan data transfer dan koreksi; membuka berkas `.xlsx` | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Laporan satu minggu memuat waktu, No. RM, pasien, asal, tujuan, alasan, pencatat, jenis | Terpenuhi (source) | `RoomTransferReportRow`, `ReadRowsAsync` |
| 2. Periode > 31 hari → pesan `VAL-RWF-90` | Terpenuhi (source) | `ValidatePeriod` |
| 3. Tanpa permission → 403 | Terpenuhi (source) | Atribut endpoint |
| 4. Ekspor hanya bagi `ExportRoomTransfer` dan tercatat | Terpenuhi (source) | Atribut + `AuditAsync` |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Waktu pada ekspor ditulis Asia/Jakarta (UTC+7); respons API tetap UTC |
| Masalah yang diketahui | Ekspor dibatasi 20.000 baris per berkas |
| Risiko tersisa | Format `.xlsx` ditulis sendiri (SpreadsheetML standar); perlu dibuka di Excel saat uji |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` `InpatientReportDtos.cs`, `InpXlsxWriter.cs`, `InpRoomTransferReportService.cs`, `InpatientReportController.cs`; `M` `Program.cs` |
| Langkah berikutnya | Admin memberikan dua permission laporan; uji ekspor |
