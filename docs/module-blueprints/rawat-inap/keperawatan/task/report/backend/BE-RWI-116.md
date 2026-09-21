# Laporan Perubahan Backend — `BE-RWI-116`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-116` |
| Judul | Cek ganda obat high-alert |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-116` |
| Trace | `FR-KEP-066`; `VAL-KEP-31a`–`c`; `INV-KEP-05`; `RWI-DEC-117`; state matrix 5.5; api-contract 7.11 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-115` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — keselamatan obat, penjaga dua orang |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — lima kriteria terpetakan; `dotnet build` lolos; verifikasi proses bisnis `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement` |
| Registry / prefix | `Phm` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001` |
| Hak akses baru | `MedicationAdministration : DoubleCheck` (`AccessType = Update`) |
| Database | Nol migration tambahan; check constraint `CK_PhmMedicationAdministration_DoubleCheckerDiffers` dari K4 menjadi lapis kedua |

## 1. Masalah yang diperbaiki

Obat high-alert — insulin, antikoagulan, elektrolit pekat — wajib diperiksa perawat kedua sebelum dianggap diberikan.
Tanpa penegakan server, pencatat dapat mengonfirmasi sendiri, dan cek ganda kehilangan arti.

## 2. Proses bisnis

1. Insulin aspart 6 unit dicatat Ns. Siti → dosis tetap `Due` dengan `DoubleCheckStatus = Pending` (salinan
   `IsHighAlertSnapshot` butir resep saat dosis lahir). Dosis muncul di `GET /double-check-worklist?serviceUnitId=`.
2. Ns. Dewi (unit yang sama) → `PATCH /{id}/double-check` `Decision=Confirm` → `Administered` + `Confirmed`.
3. **Jalur tidak normal:**
   - Siti mengonfirmasi sendiri → `403` "Cek ganda harus dilakukan perawat lain." — dibandingkan pada akun **dan** pegawai.
   - Dewi menolak tanpa catatan → `400` "Isi alasan penolakan."
   - Dewi menolak "dosis dihitung dari GDS pasien lain" → revisi `DoubleCheckRejected` menyimpan isian pemberian;
     dosis kembali `Due` + `Rejected` tanpa isian pemberian; Siti mencatat ulang → `Pending` lagi.
   - Dosis tidak sedang `Pending` → `409`.
   - Daftar tunggu diminta pengguna yang tidak ditempatkan di unit itu → `403`; tanpa `serviceUnitId` → `400`.
4. Obat bukan high-alert → `NotRequired`, langsung `Administered`, tidak pernah masuk daftar tunggu.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

State matrix 5.5, validation matrix 6.5, api-contract 7.11, `InpatientClinicalContextService.IsEmployeeAssignedToUnitAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.DoubleCheck.cs` | Baru — `DoubleCheckAsync`, `GetDoubleCheckWorklistAsync` |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Recording.cs` | Pencatatan high-alert → `Pending`; pencatatan ulang setelah penolakan mengosongkan jejak pemeriksa (jejak tetap di revisi) |
| `Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs` | `PATCH /{id}/double-check`, `GET /double-check-worklist` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai 7.11; item daftar membawa `CanDoubleCheck` supaya layar menyembunyikan tombol bagi pencatatnya sendiri |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Kewenangan "perawat kedua" = butir `DoubleCheck` di layar Akses Role; kode hanya menjaga orang berbeda dan penempatan unit — nol nama peran |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Medication Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/double-check` | Konfirmasi atau tolak | `MedicationAdministration : DoubleCheck` |
| `GET` | `/double-check-worklist` | Dosis `Pending` di satu unit | `MedicationAdministration : DoubleCheck` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran kriteria 1–5 | Terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi proses bisnis termasuk konfirmasi oleh orang yang sama | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. High-alert `Pending` dan tidak langsung `Administered` | Terpenuhi | `RecordAsync`, `RecordNewAdministrationAsync` |
| 2. Perawat kedua bukan pencatat | Terpenuhi | `DoubleCheckAsync` → `403`; check constraint K4 |
| 3. `Confirmed` atau `Rejected`; `Rejected` menghalangi pemberian | Terpenuhi | Penolakan mengembalikan `Due` tanpa isian pemberian |
| 4. Bukan high-alert `NotRequired` | Terpenuhi | Cabang non high-alert |
| 5. Daftar tunggu per unit | Terpenuhi | `GetDoubleCheckWorklistAsync` |
| DoD: `dotnet build`, verifikasi proses bisnis | `dotnet build` lolos; **verifikasi proses bisnis dikecualikan atas keputusan pemilik 17 September 2026** | `PASS` sebagian; sisanya `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Butir yang tanda high-alert-nya berubah setelah dosis terbentuk tetap mengikuti salinan saat dosis lahir |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` |
| Langkah berikutnya | Frontend cek ganda |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
