# Laporan Perubahan Backend — `BE-RWI-255`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-255` |
| Judul | Endpoint monitoring EWS per bangsal |
| Slice | Repair 015 keperawatan; Slice 1 pada [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-015.md](../../../roadmap/roadmap-repair-015.md), kartu `BE-RWI-255` |
| Trace | `FIX-KEP-015-04` menutup `ISS-KEP-015-01` (bagian daftar peringatan); [plan repair 015](../../../docs/plan-repair/plan-repair-015-kalkulasi-ews-pemesanan-diet-gizi-dan-retur-obat.md) bagian 3.C dan kartu `FIX-KEP-015-04` |
| Contract version | Endpoint tambahan di luar dokumen kontrak `api-contract.md` sub-modul `episode-rawat-inap`; bentuknya ditetapkan plan repair 015 yang disetujui pemilik 2026-11-02. Delta kontrak dicatat pada bagian 3.3 |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — satu endpoint baca, satu query, dua DTO; tanpa perubahan database |
| Task mode | `BACKEND` (ditambah wewenang dokumen blueprint) |
| Target tulis | `NewQuilvianSystemBackend` branch `MHamzah`: source rawat inap dan `docs/module-blueprints/rawat-inap/**` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Source masuk pada `da737fb9` (2026-11-02); diverifikasi pada HEAD `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai: kelima acceptance criteria terpetakan ke source, `dotnet build` lulus. Uji manual lewat layar dijalankan bersama `FE-RWI-284` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, server sudah menghitung skor peringatan dini (*Early Warning Score*, EWS) dan
menyalakan penanda "perlu lapor dokter" setiap kali perawat menyimpan tanda vital. Namun tidak
ada satu tempat pun yang menampilkan **daftar pasien** yang penandanya menyala. Kepala ruangan
harus membuka pasien satu per satu untuk tahu siapa yang memburuk.

Contoh akibatnya: di Ruang Mawar ada 20 pasien. Tn. J (samaran) mendapat EWS 15 pada pukul
02.10. Tanpa daftar per bangsal, perawat jaga baru tahu bila kebetulan membuka berkas Tn. J.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Pasien rawat inap yang tanda vital **terakhirnya** berisiko Tinggi atau Kritis tidak terlewat |
| Pelaku | Perawat bangsal menyimpan tanda vital; kepala ruangan atau perawat jaga membaca daftar; dokter menerima laporan |
| Pemicu | Pengguna membuka Monitoring Rawat Inap → tab "EWS Perlu Lapor Dokter" |
| Prasyarat | Pasien berstatus episode `Admitted` atau `DischargePending`; tanda vital tersimpan dengan tautan episode (`InpEpisodeId`) |

Langkah utama:

1. Perawat menyimpan tanda vital. Server menghitung skor dan golongan:
   skor 0–2 `Low`, 3–4 `Medium`, 5–6 `High`, 7 ke atas `Critical`
   (`PatientVitalSignCalculation.cs:182-191`).
2. Server menyalakan penanda lapor dokter bila golongan `High`/`Critical`, bila satu parameter
   masuk batas kritis, atau bila perawat mencentangnya sendiri
   (`PatientVitalSignController.cs:564`).
3. Server menautkan tanda vital ke episode rawat inap yang masih terbuka pada kunjungan itu
   (`PatientVitalSignController.cs:493`, `:579-583`).
4. Pengguna memanggil `GET /monitoring/ews-alerts` dengan bangsal dan golongan minimum.
5. Server mengambil episode rawat inap aktif, lalu untuk tiap episode mengambil **satu** tanda
   vital terakhir yang masih aktif, tidak dibatalkan, dan bukan salah input
   (`InpCensusQueryService.cs`, method `GetEwsAlertsPagedAsync`).
6. Server menyaring: golongan minimum `High` menampilkan golongan `High`/`Critical` **atau**
   penanda lapor dokter menyala; golongan minimum `Critical` menampilkan golongan `Critical`
   atau tanda vital kritis.
7. Hasil diurutkan dari golongan dan skor tertinggi, lalu dipotong per halaman.

Aturan bisnis yang berlaku:

| Aturan | Contoh |
| --- | --- |
| Hanya tanda vital terakhir yang dinilai | Ny. L EWS 6 pukul 01.40, lalu EWS 2 pukul 03.00. Ny. L tidak tampil, walaupun penanda pada pengukuran 01.40 masih menyala |
| Pengukuran salah input dilewati | Tanda vital Tn. J pukul 02.10 (EWS 15) dibatalkan sebagai salah input. Daftar memakai pengukuran sebelumnya, misalnya EWS 8 pukul 22.00 |
| Hanya episode aktif | Pasien dengan episode `Closed` atau `Cancelled` tidak tampil |
| Hanya rawat inap | Tanda vital IGD dan poliklinik tidak punya tautan episode rawat inap, jadi tidak pernah tampil |
| Skor tidak dihitung ulang | Daftar membaca golongan dan penanda apa adanya dari server |

Jalur tidak normal:

| Keadaan | Hasil |
| --- | --- |
| Pengguna tanpa hak `InpatientMonitoring : Read` | Ditolak dengan kode 403, artinya pengguna tidak punya hak akses |
| Bangsal tidak dipilih | Semua bangsal rawat inap aktif ikut dinilai |
| Tidak ada pasien berisiko | Daftar kosong dengan `totalData` 0 |
| Episode belum punya tanda vital | Episode itu tidak tampil |

Hasil akhir: daftar baca-saja. Endpoint ini tidak mengirim pemberitahuan dan tidak memadamkan
penanda. Keduanya menunggu protokol eskalasi `FIX-KEP-015-08` (keputusan K-03).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` backend, `rules/backend/TASK_RULES.md`, `TEST_POLICY.md`, `REVIEW_RULES.md`,
  `REPORT_TEMPLATE.md`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`,
  `engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (entri `InPatientManagement` / `Inp`, `ACTIVE`).
- Plan repair 015 bagian 3.C dan kartu `FIX-KEP-015-04`; roadmap repair 015.
- `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs`
- `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs`
- `Areas/HealthServices/InPatientManagement/DTOs/InpatientMonitoringDtos.cs`
- `Areas/HealthServices/ClinicalManagement/Services/PatientVitalSignCalculation.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs`
- `Areas/HealthServices/ClinicalManagement/Services/InpatientVitalSignService.cs`
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientVitalSignDtos.cs`
- Enum `EwsRiskLevel` dan `PatientVitalSignStatus`.

### 3.2 Berkas yang berubah

Source task ini sudah masuk commit `da737fb9` (2 November 2026). Verifikasi 9 November 2026 tidak
menemukan kekurangan, sehingga **tidak ada perubahan source baru** pada sesi ini.

| Berkas (isi commit `da737fb9`) | Perubahan |
| --- | --- |
| `InPatientManagement/Controllers/InpatientMonitoringController.cs` | Action `GetEwsAlerts` pada `GET ews-alerts` beserta metadata hak akses |
| `InPatientManagement/Services/InpCensusQueryService.cs` | Method `GetEwsAlertsPagedAsync`: episode aktif → tanda vital terakhir yang sah → saring → urutkan → halaman |
| `InPatientManagement/DTOs/InpatientMonitoringDtos.cs` | `InpatientEwsAlertQuery`, `EwsAlertItemResponse`, `EwsAlertPagedResult` |
| `ClinicalManagement/DTOs/PatientVitalSignDtos.cs` | `PatientVitalSignSeriesItem` mendapat `EwsMonitoringRecommendation` dan `NeedDoctorNotification` untuk dipakai `FE-RWI-283` |
| `ClinicalManagement/Services/InpatientVitalSignService.cs` | Mengisi dua field tambahan itu pada deret tanda vital |

Berkas dokumen yang ditulis sesi ini: laporan ini, roadmap repair 015, register plan repair 015,
dan dokumen MVP.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /monitoring/ews-alerts` (additive). Field tambahan pada deret tanda vital bersifat additive. Query `Search` ditambahkan di luar rangka plan untuk pencarian nama/RM, mengikuti pola daftar pantau lain. Dokumen `episode-rawat-inap/contracts/api-contract.md` **belum** memuat endpoint ini; pencatatannya masuk Slice 6 MVP |
| Database | Tidak ada perubahan schema maupun migration |
| Keamanan/Auth | Memakai `[AccessAction("Read", ...)]` dan `[AccessPermission("InpatientMonitoring", "Read")]`, sama persis dengan sepuluh daftar pantau lain pada controller ini. Tidak ada hardcode peran |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/ews-alerts` | Daftar pasien rawat inap aktif yang tanda vital terakhirnya berisiko Tinggi atau Kritis, atau perlu dilaporkan ke dokter | `InpatientMonitoring : Read` | Query `ServiceUnitId` (bangsal, opsional), `MinimumRiskLevel` (`High` bawaan, atau `Critical`), `Search`, `PageNumber` (bawaan 1), `PageSize` (bawaan 25) | `ApiResponse<EwsAlertPagedResult>`: episode, nomor RM, nama pasien, bangsal, kamar, bed, waktu ukur, skor, golongan, tanda kritis, penanda lapor dokter, anjuran, dan nama pengukur |

Kode status:

| Kode | Arti bagi pengguna |
| --- | --- |
| 200 | Daftar berhasil diambil, termasuk bila kosong |
| 401 | Pengguna belum masuk (*login*) |
| 403 | Pengguna tidak punya hak "Read" pada Inpatient Monitoring |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` (`ACTIVE`), keberlakuan `TOUCHED LEGACY` pada controller/service yang ada; QBE yang berlaku: QBE-SVC-001 (query di service, bukan controller), QBE-API-001 (`ApiResponse<T>`), QBE-PERM-001, QBE-DTO-001 (DTO, bukan entity), QBE-PAGE-001 (paging). Tidak ada entity baru, jadi QBE-ENT/NAM/CFG tidak berlaku | `PASS` | Review source pada bagian 3.2 |
| `dotnet build QuilvianSystemBackend.csproj -o <scratch>` (9 November 2026) | `0 Error(s)`, `12 Warning(s)`; seluruh warning adalah peringatan paket NuGet (`NU1902`/`NU1903` SixLabors.ImageSharp) dan peringatan lama | `PASS` | Keluaran build |
| AC 1 — tiga pasien Mawar, golongan minimum Tinggi, urut skor | Filter `EwsRiskLevel >= High \|\| NeedDoctorNotification`; urutan golongan lalu skor menurun. Golongan naik searah skor, jadi urutannya sama dengan urutan skor. Tn. M (EWS 2) tidak tampil selama tidak ada parameter kritis atau centang manual | `PASS` (penelusuran source) | `GetEwsAlertsPagedAsync`, blok `Where` dan `OrderByDescending` |
| AC 2 — pengukuran ulang membuat pasien hilang | Subquery `Latest` mengambil satu baris terakhir per episode menurut `ObservationDateTime` | `PASS` (penelusuran source) | Subquery `Latest` |
| AC 3 — pengukuran salah input dilewati | Subquery mengecualikan `Cancelled` dan `EnteredInError`, sama dengan pola `PatientVitalSignController.cs:299` | `PASS` (penelusuran source) | Subquery `Latest` |
| AC 4 — episode `Closed` tidak tampil | Hanya `Admitted` dan `DischargePending` | `PASS` (penelusuran source) | Filter `EpisodeStatus` |
| AC 5 — pasien IGD/poli tidak tampil | Query berangkat dari `InpEpisode`; tanda vital hanya ikut bila `InpEpisodeId` sama | `PASS` (penelusuran source) | Subquery `Latest` |
| Data DB dev `QuilvianNewDevHamzah` (baca-saja) | 67 baris tanda vital, **semuanya tanpa tautan episode rawat inap** (sumber IGD, poli, dan konsultasi dokter). Belum ada tanda vital rawat inap, jadi endpoint saat ini mengembalikan daftar kosong. Itu keadaan data, bukan cacat | `NOT RUN` untuk bukti runtime | Query `count(*)` per `VitalSignSource` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan bersama `FE-RWI-284` dengan membuat tanda vital uji melalui
ruang kerja perawat; hasilnya ditambahkan ke laporan ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC 1 — Tn. J (15) dan Ny. L (6) tampil, Tn. M (2) tidak, urut skor tertinggi | Terpenuhi | Bagian 5 baris AC 1 |
| AC 2 — Ny. L hilang setelah diukur ulang EWS 2 | Terpenuhi | Bagian 5 baris AC 2 |
| AC 3 — pengukuran `EnteredInError` dilewati | Terpenuhi | Bagian 5 baris AC 3 |
| AC 4 — episode `Closed` tidak tampil | Terpenuhi | Bagian 5 baris AC 4 |
| AC 5 — pasien IGD dan poliklinik tidak tampil | Terpenuhi | Bagian 5 baris AC 5 |
| DoD — `dotnet build` pada project aplikasi | Terpenuhi | `0 Error(s)` |
| DoD — laporan `task/report/backend/BE-RWI-255.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Working tree backend memuat perubahan modul HR milik pekerjaan lain (`Program.cs`, `ApplicationDbContext.cs`, migration `20261109140000_*` yang belum dilacak). Build sesi ini ikut mengompilasinya dan tetap lulus. Berkas itu tidak disentuh |
| Masalah yang diketahui | `api-contract.md` belum memuat endpoint ini (Slice 6) |
| Risiko tersisa | Penyaringan dan paginasi dikerjakan di memori, sama seperti daftar pantau lain di service ini. Aman selama jumlah episode aktif setara kapasitas tempat tidur (DB dev: 189 bed) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada perubahan source rawat inap. Berkas baru/berubah sesi ini hanya dokumen di `docs/module-blueprints/rawat-inap/` |
| Langkah berikutnya | `FE-RWI-284` (tab Monitoring) beserta uji manual kelima kriteria dengan data uji |
