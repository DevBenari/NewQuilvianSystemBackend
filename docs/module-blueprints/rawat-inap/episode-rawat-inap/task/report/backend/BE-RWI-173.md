# Laporan Perubahan Backend — `BE-RWI-173`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-173` |
| Judul | Master butir persiapan bedah |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-173` |
| Trace | `FR-RWF-045`; `RWI-DEC-173` butir 3; API 11.4; `VAL-RWF-94`; backend 12.7, 12.12 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-172` — ✅ (model `MstSurgicalPreparationItem`) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 1, logika 1, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/{DTOs,Services,Controllers,Seeders}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` |
| Prefix registry | `Mst` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Migration/database: tidak. Data awal: lewat seeder startup yang menolak `Production` |

---

## 1. Masalah yang diperbaiki

Catatan Pra-Operasi bangsal butuh daftar butir checklist (misalnya "Gelang identitas terpasang").
Tanpa layar master, butir itu harus ditanam di kode, dan rumah sakit tidak dapat menambah atau
menonaktifkan butir tanpa rilis aplikasi.

## 2. Proses bisnis

1. Admin Master Data membuka layar butir persiapan bedah: kartu ringkasan (jumlah aktif, wajib,
   tidak wajib, kelompok), daftar dengan saringan kelompok dan aktif.
2. Admin menambah butir: kode (diisi admin sesuai kontrak), kelompok (empat kelompok baku
   `RWI-DEC-173`), nama, wajib/tidak, urutan tampil, keterangan. Butir baru selalu aktif.
3. Admin mengubah butir dengan `RowVersion` dari `GET /{id}`. Bila admin lain menyimpan lebih
   dulu, simpanan kedua ditolak 409 — tidak ada perubahan yang tertimpa diam-diam.
4. Admin menonaktifkan butir. Butir nonaktif tidak ikut ke versi pra-operasi **baru**; versi lama
   tetap memuatnya karena nama dan sifat wajibnya sudah disalin (`OprWardPreOpItem`).
5. Contoh: 14 butir aktif, 9 wajib. Butir "Hasil radiologi terlampir bila diminta" dinonaktifkan
   pukul 10.00 → versi pra-operasi yang dibuat pukul 11.00 hanya memuat 13 butir.
6. Jalur tidak normal: kode kembar → 409 `MST-SPI-001`; `RowVersion` basi → 409; menghapus butir yang
   sudah dipakai pra-operasi → 400 dengan saran menonaktifkan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BloodComponentController.cs`, `BloodComponentService.cs`, `BloodComponentDtos.cs`,
`BloodComponentSeeder.cs`, `HemodialysisMasterDataSeeder.cs`, `MstSurgicalPreparationItem.cs`,
`MstSurgicalPreparationItemConfiguration.cs`, `Program.cs`; standar endpoint master data; kontrak
API 11.4; kamus data 19.7.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/DTOs/SurgicalPreparationItemDtos.cs` | Baru — ringkasan, respons, option, request tambah/ubah/status, metadata penyaring dan form |
| `Areas/HealthServices/MasterData/Services/SurgicalPreparationItemService.cs` | Baru — daftar, ringkasan, option, detail, tambah, ubah ber-`RowVersion`, status, hapus lunak dengan cek pemakaian `OprWardPreOpItem` |
| `Areas/HealthServices/MasterData/Controllers/SurgicalPreparationItemController.cs` | Baru — sembilan endpoint baseline |
| `Areas/HealthServices/MasterData/Seeders/SurgicalPreparationItemSeeder.cs` | Baru — data awal `E8`: 14 butir dalam empat kelompok; idempoten per kode termasuk baris terhapus; menolak `Production` |
| `Program.cs` | Registrasi `SurgicalPreparationItemService` dan pemanggilan seeder startup |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 11.4. **Delta kontrak:** empat endpoint baseline tambahan (`filters/metadata`, `summary`, `options`, `DELETE`) dan permission `SurgicalPreparationItem : Delete`, mengikuti standar endpoint master data. `PATCH /{id}/status` menerima `RowVersion` opsional |
| Database | Memakai tabel dari `BE-RWI-172`; tidak ada migration baru. Data awal lewat seeder (bukan migration data) |
| Keamanan/Auth | Baris registry `SurgicalPreparationItem : Read/Create/Update/Delete` lahir dari atribut endpoint |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Surgical Preparation Item

Base URL: `api/v1/health-services/master-data/surgical-preparation-items`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Penyaring, pengurutan, pilihan kelompok, isian form | `SurgicalPreparationItem : Read` |
| `GET` | `/summary` | Kartu ringkasan | `SurgicalPreparationItem : Read` |
| `GET` | `/` | Daftar; saringan `search`, `groupName`, `isActive`, `isMandatory` | `SurgicalPreparationItem : Read` |
| `GET` | `/options` | Butir aktif per kelompok dan urutan | `SurgicalPreparationItem : Read` |
| `GET` | `/{id}` | Detail termasuk `RowVersion` | `SurgicalPreparationItem : Read` |
| `POST` | `/` | Tambah; kode kembar → 409 `MST-SPI-001` | `SurgicalPreparationItem : Create` |
| `PUT` | `/{id}` | Ubah dengan `RowVersion`; basi → 409 | `SurgicalPreparationItem : Update` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `SurgicalPreparationItem : Update` |
| `DELETE` | `/{id}` | Hapus lunak; dipakai pra-operasi → 400 | `SurgicalPreparationItem : Delete` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Service pemilik `DbContext`; argumen `[AccessPermission]` cocok dengan `ControllerName` dan `[AccessAction]` | `PASS` | Review source |
| Review diff dan scope | Sesuai kartu + delta baseline yang dicatat | `PASS` | Daftar 3.2 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi API dan pemeriksaan data awal | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tambah, ubah, dan nonaktif berfungsi dengan `RowVersion` | Terpenuhi (source) | `UpdateAsync` dan `UpdateStatusAsync` membandingkan `RowVersion`; token konkurensi pada configuration |
| 2. Saringan kelompok dan aktif berfungsi | Terpenuhi (source) | `ApplyFilters` (`groupName`, `isActive`, `isMandatory`) |
| 3. Data awal empat kelompok tersedia di lingkungan uji | Terpenuhi (source) | `SurgicalPreparationItemSeeder` dipanggil saat startup non-produksi |
| 4. Baris registry `SurgicalPreparationItem : Read/Create/Update` lahir dari atribut | Terpenuhi (source) | Atribut pada controller; ditambah `Delete` (delta baseline) |
| DoD build tanpa error | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Isi data awal adalah butir V1 dari contoh keputusan dan PRD; butir lain berupa usulan. **Wajib disahkan pemilik klinis sebelum produksi** (gerbang produksi kartu) |
| Masalah yang diketahui | Kelompok tidak dibatasi pada empat nilai baku — kelompok lain tetap diterima (kamus data tidak membatasi); ejaan kelompok baku dinormalkan |
| Risiko tersisa | Butir yang dihapus admin tidak dihidupkan ulang seeder (pemeriksaan termasuk baris terhapus) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` `SurgicalPreparationItemController.cs`, `SurgicalPreparationItemDtos.cs`, `SurgicalPreparationItemService.cs`, `SurgicalPreparationItemSeeder.cs`; `M` `Program.cs` |
| Langkah berikutnya | Pengesahan isi butir oleh pemilik klinis; build oleh pemilik |
