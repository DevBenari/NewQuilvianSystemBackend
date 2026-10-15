# Laporan Perubahan Backend — `BE-RWI-228`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-228` |
| Judul | Pembacaan bed lebih ringan dan papan bed mengikuti penyaring kamar, kelas, dan pencarian |
| Slice | Perbaikan `PLAN-REPAIR-EPS-009` — `FIX-EPS-009-03` dan `FIX-EPS-009-04` |
| Roadmap | [`backend-roadmap-bed-management.md`](../../../roadmap/backend-roadmap-bed-management.md) bagian "Tambahan PLAN-REPAIR-EPS-009" |
| Trace | `ISS-EPS-009-03`, `ISS-EPS-009-04`, `ISS-EPS-009-T1`; kontrak bed-board `RWI-BED-BOARD-RESERVATION-001 1.0.0`; `contracts/api-contract.md` baris adapter `GET /bed-board` |
| Contract version | API `0.12.0` (Bed Management) — delta aditif: `GET /bed-board` menerima `roomId?`, `patientClassId?`, `search?`; `BedMonitoringQuery.PatientClassId?` |
| Dependency | `BE-RWI-211` (proyeksi bersama, 🟡 source tersedia) — tidak menahan task ini |
| Klasifikasi | `MEDIUM` — 7 berkas diubah dalam satu modul, logika baca dan satu parameter query aditif; tanpa database |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: 5 service, 1 controller, 1 DTO di `Areas/HealthServices/InPatientManagement/`; laporan dan register blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `86cb8ef8d3660e97776096fb33fcaf7dec7c9006` (`MHamzah`) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai (source + build + QBE). Hitung perintah SQL dan uji papan di layar `NOT RUN` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Wewenang | Source: ya. Entity, migration, eksekusi database, deployment: tidak |

---

## 1. Masalah yang diperbaiki

1. **Bolak-balik database berlebihan.** Setiap kali langkah Pilih Bed dibuka atau penyaring diganti,
   layar memanggil `GET bed-board` dan `GET available-beds` bersamaan. Satu `available-beds`
   (34 bed, 6 terisi, SuperAdmin) menjalankan ±35–45 perintah SQL: tiga data kesiapan/pemesanan/
   penempatan dimuat dua kali, nama unit dan kelas dimuat dua kali, satu query identitas per bed
   terisi/dipesan, tiga cek hak aksi pembersihan beserta daftar unitnya walaupun tidak ada bed yang
   sedang dibersihkan, dan daftar unit SuperAdmin dibaca ulang per operasi. Hari ini 300–530 ms;
   sebelum `6b8a8b20` 2–38 detik (log 12–13 Oktober).
2. **Papan mengabaikan penyaring.** `bed-board` hanya menerima `serviceUnitId`. Bed Kelas I ikut tampil
   ketika petugas memilih Kelas II, redup dengan alasan "Tidak lolos kelayakan", dan angka ringkasan
   tetap angka seluruh unit.
3. **Pesan teknis.** Konflik simpan bersamaan menjawab "Data bed berubah bersamaan. Muat ulang data:
   DbUpdateException".

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Petugas melihat papan dan daftar bed yang dapat dipilih secepat mungkin, dengan angka dan kartu yang sesuai penyaring |
| Pelaku | Petugas admisi (langkah Pilih Bed), petugas Papan Tempat Tidur, petugas Bed Management |
| Pemicu | Membuka layar, mengganti penyaring unit/kamar/kelas, mengetik pencarian |
| Langkah | (1) Server menggugurkan pemesanan kedaluwarsa. (2) Membaca kandidat bed sesuai cakupan dan penyaring. (3) Satu proyeksi memuat kamar, unit, kelas, kesiapan, penempatan, pemesanan, dan episode pemegang sekaligus. (4) Kelayakan per bed dinilai di memori dari data proyeksi. (5) Nama unit/kelas diisi dari data proyeksi |
| Aturan | Isi jawaban identik dengan sebelumnya untuk data yang sama: status enam nilai, alasan penolakan, `AvailableActions`, `HoldingContext`, penyamaran identitas untuk pengguna `OperationalOnly` |
| Jalur tidak normal | Konflik simpan bersamaan → 409 `BED_HOLD_CONFLICT` "Data bed berubah bersamaan. Muat ulang data." |
| Hasil akhir | Papan Kelas II hanya memuat kamar Kelas II dan angkanya; bolak-balik database kira-kira separuh |

Contoh: unit "Rawat Inap" 34 bed (12 Kelas II, 2 terisi). Penyaring Kelas II →
`GET bed-board?serviceUnitId=…&patientClassId=…` menjawab 12 bed (10 tersedia, 2 terisi), sama
dengan cakupan `available-beds`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedManagementQueryService.cs`, `InpBedAccessService.cs`, `InpBedOccupancyService.cs`,
`InpBedOccupancyService.BedManagement.cs`, `InpBedMutationCoordinator.cs`, `InpBedReadinessService.cs`
(pemanggil `ProjectAsync`), `InpatientBedOccupancyController.cs`, `InpatientBedManagementController.cs`,
`InpatientBedManagementDtos.cs`, `InpatientBedOccupancyDtos.cs`, `Services/Security/AccessPermissionService.cs`,
konfigurasi indeks `InpBedPlacementConfiguration.cs`/`InpBedReadinessConfiguration.cs`; frontend
`use-inpatient-bed-board.jsx`, `inpatient-bed-utils.jsx` (`buildBedBoardQuery`),
`use-inpatient-episode-worklist.jsx` (pemanggil `bed-board` tanpa penyaring).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/InpBedManagementQueryService.cs` | `ProjectManyAsync` kini memanggil `ProjectBatchAsync` (baru, `internal`) yang mengembalikan baris status beserta kesiapan, penempatan, pemesanan, unit, dan kelas yang dimuatnya (`BedProjectionBatch`). Kamar yang sudah di-`Include` tanpa pelacakan dipakai langsung. Identitas pemegang dinilai di memori lewat `CanReadLoadedIdentityAsync`. Unit hak aksi pembersihan dimuat hanya bila ada bed bebas dalam keadaan kesiapan yang relevan. `MonitoringAsync` menyaring `PatientClassId` dan mengambil nama unit dari proyeksi |
| `Services/InpBedAccessService.cs` | Daftar unit aktif SuperAdmin disimpan sekali per permintaan; method baru `CanReadLoadedIdentityAsync` dengan aturan yang sama persis dengan `CanReadIdentityAsync` |
| `Services/InpBedOccupancyService.cs` | `SearchAvailableBedsAsync` dan `EvaluatePlacementEligibilityAsync` memakai data proyeksi, tidak memuat ulang kesiapan/pemesanan/penempatan; `FillServiceUnitAndClassNamesAsync` dihapus karena namanya kini dari proyeksi |
| `Services/InpBedOccupancyService.BedManagement.cs` | `GetBedBoardAsync` menerima `roomId`, `patientClassId`, `search` opsional (Guid kosong diperlakukan tidak dikirim) |
| `Controllers/InpatientBedOccupancyController.cs` | `GET bed-board` membaca tiga parameter query opsional baru; atribut akses tidak berubah |
| `DTOs/InpatientBedManagementDtos.cs` | `BedMonitoringQuery.PatientClassId` opsional |
| `Services/InpBedMutationCoordinator.cs` | Pesan 409 `BED_HOLD_CONFLICT` tanpa nama tipe exception |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `GET /bed-board` menerima `roomId?`, `patientClassId?`, `search?`; `GET /bed-management/monitoring` menerima `patientClassId?`. Tanpa parameter baru jawabannya tidak berubah. Bentuk response tidak berubah. Amandemen dokumen kontrak dicatat sebagai `FIX-EPS-009-06` |
| Database | Tidak ada perubahan schema/migration. Jumlah perintah SQL per permintaan turun |
| Keamanan/Auth | Aturan penyamaran identitas dipindah ke pemeriksaan di memori dengan urutan dan syarat yang sama: SuperAdmin atau penugasan unit non-`OperationalOnly` → hak `InpatientEpisode:Read` → episode tidak dihapus dan unitnya dalam cakupan `InpatientBedOccupancy:Read`. `[AccessAction]`/`[AccessPermission]` endpoint tidak berubah |

**Perkiraan bolak-balik database** (SuperAdmin, 34 kandidat, 6 terisi, tanpa bed sedang dibersihkan;
satu cek hak ≈ 3–4 perintah):

| Bagian | Sebelum | Sesudah |
| --- | ---: | ---: |
| Cek hak + daftar unit cakupan | 5–6 | 5–6 |
| Gugurkan pemesanan kedaluwarsa | 1 | 1 |
| Kandidat bed | 1 | 1 |
| Proyeksi (kamar, unit, kelas, kesiapan, penempatan, pemesanan, episode) | 7 | 6 (kamar dari `Include`) |
| Hak aksi pembersihan × 3 + daftar unit | 12–15 | 0 |
| Identitas pemegang | 4–5 + 6 | 3–4 |
| Muat ulang kesiapan/pemesanan/penempatan | 3 | 0 |
| Nama unit dan kelas | 2 | 0 |
| **Jumlah `available-beds`** | **±35–45** | **±16–18** |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board?serviceUnitId=&roomId=&patientClassId=&search=` | Papan bed per unit dan kamar; tiga parameter terakhir opsional (baru) | `InpatientBedOccupancy : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Tidak berubah kontraknya; lebih sedikit query | `InpatientBedOccupancy : Read` |

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-management/monitoring` | Monitoring; `patientClassId` opsional (baru) | `InpatientBedManagement : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false -p:BaseOutputPath=<scratchpad>/be-bin/ -p:UseAppHost=false` | Build succeeded, 0 error, 284 warning, 94 detik | `PASS` | Keluaran perintah; jumlah warning sama dengan build sebelum task ini (284), warning pada berkas yang disentuh hanya CS1573/CS8604 lama di baris yang tidak berubah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` untuk 7 berkas | 0 violation, 0 review, 0 info pada setiap berkas | `PASS` | Keluaran perintah |
| Review kesetaraan jawaban | Filter kesiapan/pemesanan/penempatan, sumber nama unit/kelas, urutan aksi, dan aturan identitas identik dengan kode lama | `PASS` (review) | Bagian 3.2–3.3 |
| Review pemanggil `bed-board` | `use-inpatient-bed-board.jsx` mengirim penyaring dan mengharapkannya diterapkan; `use-inpatient-episode-worklist.jsx` tanpa penyaring → tidak berubah | `PASS` (review) | Source frontend |
| Hitung perintah SQL lewat log `Microsoft.EntityFrameworkCore.Database.Command` | Tidak dijalankan | `NOT RUN` | Backend lokal tidak berjalan; skenario `PLAN-REPAIR-EPS-009` bagian 8 butir 4 |
| Uji papan Kelas II di layar | Tidak dijalankan | `NOT RUN` | Bagian 8 butir 2 |

Uji manual: `NOT FEASIBLE` — backend lokal tidak berjalan dan frontend kehilangan `node_modules`.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:** hitung SQL dan uji layar, dikecualikan atas keputusan pemilik 10 September 2026
(uji runtime oleh pemilik). Keluaran build di scratchpad; `bin/Debug` milik pemilik tidak berubah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FIX-03` AC 1 — JSON `available-beds`, `bed-board`, `monitoring` identik untuk data yang sama | Terpenuhi pada source (review); runtime `NOT RUN` | Proyeksi memakai filter dan sumber yang sama; `ProjectManyAsync` publik mengembalikan baris yang sama |
| `FIX-03` AC 2 — `available-beds` ≤ 20 perintah SQL, tanpa query per bed | Terpenuhi pada source; hitung runtime `NOT RUN` | Tabel bagian 3.3; tidak ada lagi `await` query di dalam perulangan bed (identitas di memori, aksi pembersihan di-cache per aksi) |
| `FIX-03` AC 3 — `OperationalOnly` tetap tidak melihat nama pasien/nomor episode | Terpenuhi pada source | `CanReadLoadedIdentityAsync` memeriksa penugasan `OperationalOnly` lebih dulu, sama dengan `CanReadIdentityAsync` |
| `FIX-03` AC 4 — 409 tanpa "DbUpdateException" | Terpenuhi | `InpBedMutationCoordinator.cs` blok `catch (DbUpdateException)` |
| `FIX-04` AC 1 — `bed-board` Kelas II hanya kamar Kelas II dan angkanya | Terpenuhi pada source | `MonitoringAsync`: `x.Room!.PatientClassId == query.PatientClassId`; counts dihitung dari baris tersaring |
| `FIX-04` AC 2 — tanpa parameter jawabannya sama | Terpenuhi | Parameter opsional; Guid kosong diperlakukan tidak dikirim |
| `FIX-04` AC 3 — pencarian sama dengan `available-beds` | Terpenuhi pada source | Kode/nama bed atau nama kamar, tanpa beda huruf besar-kecil |
| DoD: build, QBE | Terpenuhi | Bagian 5 |
| DoD: bukti runtime | Belum dijalankan | Dikecualikan atas keputusan pemilik |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `bed-board` dan `available-beds` masih dipanggil bersamaan dan masing-masing menjalankan pengguguran pemesanan kedaluwarsa; satu perintah ringan, dibiarkan |
| Masalah yang diketahui | `HistoryAsync` (riwayat penggunaan bed) masih memeriksa identitas dan nama pasien per baris; di luar keluhan pemilik. `ISS-EPS-009-T2`: lingkungan tanpa akun sistem expiry menjawab 503 |
| Risiko tersisa | Kesalahan penyalinan aturan identitas akan membocorkan nama pasien; sudah direview baris per baris, perlu uji akun `OperationalOnly` oleh pemilik |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan sesi: 7 berkas source task ini + `InpEpisodeService.cs` (`BE-RWI-229`) + dokumen `PLAN-REPAIR-EPS-009` |
| Langkah berikutnya | `FIX-EPS-009-06` (amandemen kontrak `bed-board` dan skema); pemilik: build ulang backend, jalankan, lalu uji bagian 8 butir 2 dan 4 |
