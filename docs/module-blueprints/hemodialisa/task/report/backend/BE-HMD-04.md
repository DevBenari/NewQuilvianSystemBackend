# Laporan Perubahan Backend — `BE-HMD-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-04` |
| Judul | Pengelolaan Mesin HD, Riwayat Status Mesin, dan Master Station |
| Slice | `MVP-1` — Pengelolaan Sumber Daya dan Kesiapan Unit HD |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.2 |
| Trace | `FR-HMD-031`, `FR-HMD-033`, `CAP-13`, `CAP-14`, `NFR-005`; `contracts/api-contract.md` grup Machine & Station; `contracts/state-transition-matrix.md` bagian 3; `HMD-VAL-090`, `HMD-VAL-091`, `HMD-VAL-092` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` ✅, `BE-HMD-03` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-CODE-004` |

---

## 1. Masalah yang diperbaiki

Unit HD tidak punya daftar mesin dan station. Teknisi yang menemukan kebocoran dialisat pada
mesin `M-03` tidak punya cara menandainya rusak, dan koordinator tetap bisa menjadwalkan pasien
ke mesin itu keesokan paginya.

---

## 2. Proses bisnis

**Mesin.**

1. Admin unit mendaftarkan mesin — kode unik per unit, misalnya `M-03`. Mesin baru berstatus
   `Ready` dan langsung mendapat satu baris riwayat "didaftarkan".
2. Teknisi mengubah status lewat `PATCH /{id}/status` dengan **alasan wajib**. Perpindahan yang
   sah: `Ready ↔ Blocked`, `Ready ↔ Maintenance`, `Ready ↔ NotEligible`, `Blocked ↔ Maintenance`.
3. Setiap perubahan menulis satu baris `HmdMachineStatusHistory` — status asal, status tujuan,
   alasan, petugas, dan **waktu server** — pada penyimpanan yang sama dengan perubahan mesinnya.
4. Hanya mesin `Ready`, aktif, dan `IsSchedulable` yang muncul pada daftar pilihan penjadwalan.

**Contoh.** Teknisi memindahkan `M-03` dari `Ready` ke `Blocked` dengan alasan "Kebocoran
dialisat". Mesin berubah status, `M-03` hilang dari pilihan jadwal, dan riwayat bertambah satu.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Alasan kosong | `400 HMD-VAL-091` "Alasan perubahan status wajib diisi agar riwayatnya dapat ditelusuri." |
| Mesin/station masih dipakai sesi `InProgress` | `409 HMD-VAL-090` — dibaca langsung dari database, bukan dari status yang mungkin basi |
| Perpindahan tidak ada di matriks | `422` dengan penjelasan status asal dan tujuan |
| Kode sudah dipakai | `409 HMD-VAL-092` |
| Hapus mesin/station yang masih dipakai sesi yang belum berakhir | Ditolak; penghapusan berupa penandaan `IsDelete` |
| Dua admin menyunting bersamaan | `409 HMD-VAL-902` lewat token `Version` |

**Station** mengikuti pola yang sama: `Available ↔ Maintenance/Blocked`, alasan wajib, dan
tidak dapat dipindah ke `Maintenance`/`Blocked` selama ada sesi berlangsung di station itu.
Station tidak punya tabel riwayat (kamus data tidak mendefinisikannya); alasan dan waktu
perubahan terakhir disimpan pada `StatusReason` dan `LastStatusChangedAt`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Machine & Station, `state-transition-matrix.md` bagian 3, `validation-matrix.md`
- Pola master data: `master-data-endpoint-standard.md` suite skill; `MstBed` dan controller master Bank Darah
- `HmdMachine`, `HmdMachineStatusHistory`, `HmdStation` beserta konfigurasinya

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdResourceService.cs` | Bagian mesin (baris 94–450) dan station (baris 454–720): daftar, ringkasan, opsi, detail, riwayat status, buat, ubah, ubah status, hapus |
| `Controllers/HmdMachineController.cs` | Baru, 10 endpoint |
| `Controllers/HmdStationController.cs` | Baru, 9 endpoint |
| `Controllers/HmdHttp.cs` | Baru, dipakai seluruh controller: pemetaan hasil ke `200/201/400/403/404/409/422/423` dalam amplop `ApiResponse<T>` dan pencatatan logger |
| `DTOs/HmdResourceDtos.cs`, `DTOs/HmdCommonDtos.cs` | Baru — request, response, metadata filter |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 12 endpoint kontrak terpenuhi. **Delta**: (a) status mesin memakai `PATCH /{id}/status` sesuai `api-contract.md`, bukan `PUT` seperti contoh bukti roadmap; (b) endpoint baca baseline master data yang belum tertulis di kontrak — mesin `filters/metadata`, `summary`, `options`; station `filters/metadata`, `summary`, `options`, `{id}` |
| Database | Tidak ada perubahan schema. Baris riwayat ditulis pada transaksi yang sama dengan perubahan status |
| Keamanan/Auth | `HemodialysisMachine : Read/Create/Update/ChangeStatus/Delete` dan `HemodialysisStation : Read/Create/Update/ChangeStatus/Delete`, diatur admin lewat layar Akses Role |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Master Data / Hemodialysis Machine

Base: `/api/v1/health-services/hemodialysis-management/master-data/hemodialysis-machines`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter, urutan, dan ukuran halaman untuk layar daftar mesin | `HemodialysisMachine : Read` |
| `GET` | `/summary` | Jumlah mesin per status (siap, diblokir, perawatan, tidak laik) | `HemodialysisMachine : Read` |
| `GET` | `/` | Daftar mesin berhalaman dengan pencarian dan filter | `HemodialysisMachine : Read` |
| `GET` | `/options` | Mesin yang boleh dipilih untuk jadwal — hanya `Ready`, aktif, dan dapat dijadwalkan | `HemodialysisMachine : Read` |
| `GET` | `/{id}` | Rincian satu mesin | `HemodialysisMachine : Read` |
| `GET` | `/{id}/status-history` | Riwayat perubahan status beserta alasan dan petugasnya | `HemodialysisMachine : Read` |
| `POST` | `/` | Mendaftarkan mesin baru (`201`) | `HemodialysisMachine : Create` |
| `PUT` | `/{id}` | Mengubah data mesin | `HemodialysisMachine : Update` |
| `PATCH` | `/{id}/status` | Mengubah status kelaikan dengan alasan wajib dan mencatat riwayat | `HemodialysisMachine : ChangeStatus` |
| `DELETE` | `/{id}` | Menandai mesin terhapus bila tidak dipakai sesi yang belum berakhir | `HemodialysisMachine : Delete` |

#### Health Services / Hemodialysis Management / Master Data / Hemodialysis Station

Base: `/api/v1/health-services/hemodialysis-management/master-data/hemodialysis-stations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter layar daftar station | `HemodialysisStation : Read` |
| `GET` | `/summary` | Jumlah station per status | `HemodialysisStation : Read` |
| `GET` | `/` | Daftar station berhalaman | `HemodialysisStation : Read` |
| `GET` | `/options` | Station yang tersedia untuk dipilih | `HemodialysisStation : Read` |
| `GET` | `/{id}` | Rincian satu station | `HemodialysisStation : Read` |
| `POST` | `/` | Mendaftarkan station (`201`) | `HemodialysisStation : Create` |
| `PUT` | `/{id}` | Mengubah data station | `HemodialysisStation : Update` |
| `PATCH` | `/{id}/status` | Mengubah status station dengan alasan wajib | `HemodialysisStation : ChangeStatus` |
| `DELETE` | `/{id}` | Menandai station terhapus bila tidak dipakai sesi yang belum berakhir | `HemodialysisStation : Delete` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif seluruh controller Hemodialisa | 0 masalah atribut; argumen `[AccessPermission]` sama persis dengan `ControllerName` dan `[AccessAction]`; seluruh endpoint kontrak mesin/station ada dengan hak akses yang sama | `PASS` | Skrip audit 22 September 2026 (lihat `BE-HMD-19`) |
| Pemeriksaan source riwayat mesin | `ChangeMachineStatusAsync` menambah `HmdMachineStatusHistory` pada `SaveChanges` yang sama | `PASS` | `HmdResourceService.cs` baris 356–410 |
| Pemeriksaan source opsi penjadwalan | `IsSchedulable && MachineStatus == Ready` | `PASS` | `HmdResourceService.cs` baris 172 |
| Pemeriksaan source station `Maintenance` | Ditolak `409` bila ada sesi `InProgress` di station itu | `PASS` | `HmdResourceService.cs` baris 652–695 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `MachineStatusLifecycleTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026 (verifikasi berat dijalankan pemilik sendiri).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `M-03` dari `Ready` ke `Blocked` dengan alasan → status berubah dan 1 baris riwayat terbentuk | Terpenuhi | `ChangeMachineStatusAsync` |
| 2. Mesin `Blocked`, `Maintenance`, atau `NotEligible` tidak muncul pada pilihan penjadwalan | Terpenuhi | `GetMachineOptionsAsync` baris 172; penjadwalan juga menolaknya `422 HMD-VAL-034` (`HmdScheduleService.cs` baris 669) |
| 3. Station menjadi `Maintenance` divalidasi tidak ada sesi aktif di station itu | Terpenuhi | `ChangeStationStatusAsync` |
| Bukti roadmap: perubahan tanpa alasan ditolak `400` | Terpenuhi | `400 HMD-VAL-091` |
| DoD: controller, DTO, service, dan riwayat status berfungsi | Terpenuhi pada source; runtime `NOT RUN` | — |
| DoD: transisi status teruji unit/integration test | **Dikecualikan atas keputusan pengguna 22 September 2026** | Diganti pemeriksaan source |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Station tidak punya tabel riwayat status terpisah — mengikuti kamus data |
| Risiko tersisa | Mesin yang diblokir setelah sesi dijadwalkan tidak membatalkan jadwal itu; sesi tertahan pada pemeriksaan tepat waktu saat Mulai ditekan (`BE-HMD-13`) |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik; `BE-HMD-06` memakai data mesin/station ini |
