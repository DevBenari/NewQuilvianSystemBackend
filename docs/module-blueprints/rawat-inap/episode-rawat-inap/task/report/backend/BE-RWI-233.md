# Laporan Perubahan Backend — `BE-RWI-233`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-233` |
| Judul | Kelas khusus menerima kategori transfer apa pun |
| Slice | Transfer ke atau dari ICU, Isolasi, ODC, dan Perinatologi tidak tertahan perbandingan naik/turun kelas |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-233` |
| Trace | FR-DNH-007; RWI-DEC-312, 319; RWI-AC-450, 458; VAL-DNH-13; INV-DNH-08; DNH-AT-450, 458 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 7: satu repo 0, diperiksa 9–20 = 1, diubah 4 = 1, logika sedang 1, mengubah nilai kontrak 2, query existing 1, auth 0, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | `InpBedClassComparisonService.cs`, bagian transfer `InpBedOccupancyService*`, snapshot `InpBedManagementQueryService.SnapshotAsync` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source dan build terbukti; bukti runtime dengan kelas resmi menunggu langkah data D-DNH-1 |

---

## 1. Masalah yang diperbaiki

Perbandingan kelas berhenti di `Unknown` untuk setiap pasangan kelas tanpa bukti urutan resmi, dan transfer lalu ditolak 422 `CLASS_ORDER_UNVERIFIED`. ICU, Isolasi, ODC, dan Perinatologi tidak punya urutan angka terhadap Kelas 1–3, sehingga transfer klinis yang paling mendesak — misalnya pasien Kelas 3 yang memburuk dan harus masuk ICU — justru yang paling sering tertahan.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Transfer yang melibatkan kelas khusus tersimpan dengan kategori pilihan petugas; antarkelas biasa tetap mengikuti urutan resmi |
| Pelaku | Perawat/kepala ruangan/DPJP dengan hak `InpatientBedOccupancy : Transfer` |
| Pemicu | Petugas memilih bed tujuan dan kategori transfer (Down/Up/Same Grade) |

Langkah utama:

1. Server membaca kelas asal (penempatan aktif) dan kelas kamar tujuan.
2. Urutan pemeriksaan baru (`CompareAsync`): kelas tidak ditemukan/nonaktif → `Unknown`; asal **atau** tujuan berjenis ICU (7), Isolation (10), Odc (14), atau Perinatology (15) → `Special` tanpa memeriksa digest urutan; Id sama → `SameGrade`; selain itu aturan urutan terverifikasi lama.
3. `Special`: ketiga kategori diterima, tidak ada `TRANSFER_CATEGORY_MISMATCH` maupun `CLASS_ORDER_UNVERIFIED`.
4. Snapshot lokasi penempatan tujuan menyimpan `ComparisonResult` (mis. `Special`) di samping `TransferCategory` pilihan petugas.
5. `available-beds` mengisi `AllowedManualCategories` ketiga kategori untuk bed kelas khusus.

| Asal → tujuan | `ComparisonResult` | Kategori yang diterima | Contoh hasil |
| --- | --- | --- | --- |
| Kelas 3 → ICU | `Special` | Down, Up, Same | Pilih Up Grade → tersimpan |
| ICU → ICU | `Special` | Down, Up, Same | Pilih Same Grade → tersimpan |
| Kelas 2 → Kelas 1 (urutan resmi, lebih tinggi) | `UpGrade` | Up saja | Pilih Down Grade → 422 `TRANSFER_CATEGORY_MISMATCH` |
| Kelas biasa, digest urutan tidak cocok | `Unknown` | — | 422 `CLASS_ORDER_UNVERIFIED` |
| Kelas 2 → HCU/NICU | aturan biasa | — | HCU dan NICU **bukan** kelas khusus |

Jalur tidak normal: penolakan terjadi di dalam transaksi koordinator sebelum penempatan ditulis — tidak ada perubahan. HD/LD tidak dibaca (INV-DNH-05).

Hasil akhir: penempatan tujuan ber-snapshot `{"TransferCategory":2,…,"ComparisonResult":"Special"}`; penempatan awal tetap berbentuk snapshot lama (tanpa `ComparisonResult`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.7, kamus data 21.3/22.1, API 14.1/14.3/14.4, validasi VAL-DNH-13; `InpBedClassComparisonService.cs`, `InpBedOccupancyService.BedManagement.cs`, `InpBedOccupancyService.cs` (`SearchAvailableBedsAsync`, `TransferCoreAsync`), `InpBedManagementQueryService.SnapshotAsync`, `PatientClassType.cs`, `InpBedTransferCategory.cs`, DTO occupancy.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedClassComparisonService.cs` | `SpecialClassTypes`, `IsSpecialClass`, `AllowedCategories`; `Special` diperiksa sebelum `SameGrade` (baris 54) |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.BedManagement.cs` | Validasi kategori dilewati untuk `Special` (baris 58); hasil perbandingan diteruskan ke `TransferCoreAsync` |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | `TransferCoreAsync` menerima `comparisonResult` dan meneruskannya ke snapshot (baris 1040); `available-beds` memakai `AllowedCategories` (baris 331) |
| `Areas/HealthServices/InPatientManagement/Services/InpBedManagementQueryService.cs` | `SnapshotAsync` menambah `ComparisonResult` hanya bila ada (baris 252) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada nilai: `ComparisonResult` dapat `Special`; `AllowedManualCategories` berisi tiga kategori |
| Database | Tidak ada perubahan schema; isi JSON snapshot transfer bertambah satu properti |
| Keamanan/Auth | `NOT APPLICABLE` — hak `InpatientBedOccupancy : Transfer` tetap |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/placements/transfer` | Transfer; kelas khusus menerima kategori apa pun | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` (header `Idempotency-Key`) | `BedPlacementResponse` |
| `GET` | `/available-beds` | Bed tersedia; bed kelas khusus menawarkan tiga kategori | `InpatientBedOccupancy : Read` | `AvailableBedQuery` | `AvailableBedPagedResult` |

Kode status: 422 `CLASS_ORDER_UNVERIFIED` dan `TRANSFER_CATEGORY_MISMATCH` hanya untuk antarkelas biasa.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | InPatientManagement / `Inp` ACTIVE; `TOUCHED LEGACY`; QBE-VAL-001, API-001, ENUM-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| DNH-AT-450 | Kelas 3 → ICU: `Special` → kategori bebas; Kelas 2 → Kelas 1 Down tetap 422 | `PASS` | `InpBedClassComparisonService.cs:54`; `InpBedOccupancyService.BedManagement.cs:58` |
| DNH-AT-458 | Tanpa konfigurasi urutan cocok, transfer antarkelas biasa tetap 422 `CLASS_ORDER_UNVERIFIED`; kelas khusus tidak terdampak | `PASS` | Urutan pemeriksaan `CompareAsync` |
| Snapshot menyimpan `Special` | Properti `ComparisonResult` ditambahkan pada snapshot transfer | `PASS` | `InpBedManagementQueryService.cs:252` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — kelas resmi berjenis ICU/Isolation/Odc/Perinatology belum diterapkan (D-DNH-1) dan backend lokal belum dijalankan ulang.

**Tidak dijalankan:** uji runtime transfer nyata (menunggu D-DNH-1).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kelas 3 → ICU dengan Up, Down, atau Same Grade tersimpan; ICU → ICU juga bebas | Terpenuhi | `Special` sebelum `SameGrade`; validasi kategori dilewati |
| Snapshot lokasi tujuan menyimpan `Special` dan kategori pilihan petugas | Terpenuhi | `SnapshotAsync(…, comparisonResult)`; `TransferCategory` tetap |
| Kelas 2 → Kelas 1 dicatat Down Grade tetap 422 `TRANSFER_CATEGORY_MISMATCH` | Terpenuhi | Jalur kelas biasa tidak berubah |
| Kelas biasa dengan digest tidak cocok tetap 422 `CLASS_ORDER_UNVERIFIED`; HCU dan NICU bukan kelas khusus | Terpenuhi | `SpecialClassTypes` hanya 7, 10, 14, 15 |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `InpBedOccupancyService.cs` juga berisi perubahan paralel issue 022 (saring unit registrasi pada `available-beds`) yang belum di-commit; perubahan task ini berada di bagian lain dan tidak menyentuhnya |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Kelas khusus yang salah jenis pada master membebaskan kategori; MasterData menjaga `PatientClassType` (D-DNH-1) |
| Perubahan sampingan | `NONE` khusus task ini (lihat `BE-RWI-232` untuk pengarsipan Designer migration) |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | D-DNH-1 (kelas resmi + `ClassOrder` satu jendela); `FE-RWI-249` memakai `AllowedManualCategories` |
