# Laporan Perubahan Backend — `BE-RWI-183`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-183` |
| Judul | Serah terima klinis saat transfer antarunit (`E7`, `P2`) |
| Slice | `POST-MVP` / `RWF-W5` (`P2`) |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-183` |
| Trace | `FR-RWF-071`; `RWI-DEC-182`, `189`; `INV-RWF-33`; `AC-RWF-071`, `072`; `UAT-RWF-14`; API 11.8; state 9.5; `VAL-RWF-91`, `92`; kamus data 19.9 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-154` [IB] — ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `InpBedOccupancyService.cs`, configuration, `ApplicationDbContext.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` dan migration `E7` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (pemilik dokumen); dipanggil `InPatientManagement` |
| Prefix registry | `Cli` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (model, service, controller); `TOUCHED LEGACY` (`InpBedOccupancyService.TransferAsync`) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001` |
| Wewenang | Source: ya. Migration `E7`: tidak (keputusan pengguna) |

---

## 1. Masalah yang diperbaiki

Saat pasien dipindah ke unit lain (misalnya bangsal ke ICU), serah terima klinis tercatat di kertas
dan sistem menampilkan "Integrasi belum tersedia". Unit tujuan tidak punya potret kondisi pasien
saat dipindah.

## 2. Proses bisnis

1. Transfer antarunit tersimpan seperti biasa. **Sesudah commit**, satu dokumen "Belum dikirim"
   lahir untuk penempatan tujuan. Pindah bed di unit yang sama dan koreksi salah catat tidak membuat
   dokumen. Gagal dibuat → transfer tetap sah; dokumen dibuat ulang saat daftar dibaca.
2. Perawat unit asal melengkapi SOAP, barang yang diserahkan, dan instruksi khusus, lalu mengirim.
   Server membekukan potret: GCS dan tanda vital terakhir, nyeri, risiko jatuh, balance cairan 24 jam.
3. Perawat unit tujuan (hak `: Receive`) menerima, atau menolak beralasan. Penerima/penolak ≠
   pengirim (422 `CLI-TRH-001`); menerima butuh pasien menempati bed aktif di unit tujuan (422
   `CLI-TRH-002`). Dokumen ditolak boleh dikirim ulang.
4. Contoh `UAT-RWF-14`: Budi pindah Melati → ICU → satu dokumen; pindah bed 302/1 → 302/2 di Melati →
   tidak ada dokumen.
5. Status dokumen tidak pernah menahan transfer, keluar ruangan, atau penutupan episode.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedOccupancyService.cs` (`TransferAsync`), `InpBedPlacement.cs`, `InpBedPlacementEndReason.cs`,
`TrxPatientVitalSign.cs`, `TrxPatientAssessment.cs`, `CliFluidBalanceEntry.cs`, `FluidDirection.cs`,
`ClinicalMeasurementStatus.cs`, `InpPatientLocationQuery.cs`; kontrak 11.8, state 9.5, kamus 19.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `ClinicalManagement/Enums/CliTransferHandoverStatus.cs` | Baru |
| `ClinicalManagement/Models/CliTransferHandover.cs` | Baru |
| `Repositories/.../ClinicalManagement/CliTransferHandoverConfiguration.cs` | Baru — `UX_CliTransferHandover_ToPlacement`, dua index status, `CK_CliTransferHandover_TwoAccounts`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | `DbSet` `CliTransferHandovers` |
| `ClinicalManagement/DTOs/TransferHandoverDtos.cs` | Baru — request, respons, potret |
| `ClinicalManagement/Services/CliTransferHandoverService.cs` | Baru — buat sesudah transfer, buat ulang yang tertinggal, draf, kirim, terima/tolak |
| `ClinicalManagement/Controllers/TransferHandoverController.cs` | Baru — lima endpoint |
| `InPatientManagement/Services/InpBedOccupancyService.cs` | Memanggil pembuatan dokumen sesudah commit transfer antarunit |
| `Program.cs` | Registrasi `CliTransferHandoverService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 11.8 |
| Database | Tabel baru. **Migration `E7` belum dibuat** (`P2`, `RWF-W5`) |
| Keamanan/Auth | Permission baru `TransferHandover : Read/Send/Receive`; `Receive` terpisah (`RWI-DEC-189`) |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Transfer Handover

Base URL: `api/v1/health-services/clinical-management/transfer-handovers`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Dokumen per episode atau unit, saringan status | `TransferHandover : Read` |
| `GET` | `/{id}` | Detail sembilan bagian | `TransferHandover : Read` |
| `PUT` | `/{id}/draft` | Lengkapi bagian pengirim | `TransferHandover : Send` |
| `PATCH` | `/{id}/send` | Kirim; potret dibekukan | `TransferHandover : Send` |
| `PATCH` | `/{id}/accept` | Terima atau tolak beralasan | `TransferHandover : Receive` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Prefix `Cli`; service pemilik `DbContext` | `PASS` | Review source |
| Pemeriksaan bentrokan nama tipe (Clinical × Inpatient) | Tidak ada | `PASS` | Sesi 2 Oktober 2026 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Migration `E7` | Tidak dibuat | `NOT RUN` | Keputusan pengguna |
| Proses bisnis transfer | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bangsal → ICU satu dokumen; unit sama tidak | Terpenuhi (source) | `CreateForTransferAsync` (unit berbeda, idempoten per penempatan tujuan) |
| 2. Pembuatan dokumen gagal → transfer tetap sah | Terpenuhi (source) | Dipanggil sesudah `CommitAsync`, dibungkus `try/catch`; `EnsureMissingDocumentsAsync` |
| 3. Kirim membekukan potret klinis | Terpenuhi (source) | `BuildSnapshotAsync` |
| 4. Terima/tolak beralasan oleh pemegang `: Receive` | Terpenuhi (source) | `AcceptAsync`, atribut endpoint |
| 5. Koreksi salah catat tidak membuat dokumen | Terpenuhi (source) | `CorrectsPlacementId` dan alasan akhir `Transfer` diperiksa |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pembuatan ulang dokumen yang tertinggal berjalan saat daftar dibaca (per episode atau unit), dibatasi episode yang masih hadir |
| Masalah yang diketahui | Penolak dicatat pada kolom penerima; alasan tolak terakhir dipertahankan saat dikirim ulang |
| Risiko tersisa | `P2`; mengubah alur transfer yang juga diubah `BE-RWI-154` |
| Perubahan sampingan | Konstruktor `InpBedOccupancyService` bertambah `CliTransferHandoverService` dan `ILogger` |
| Interupsi | `NONE` |
| Status Git | `??` enum, model, configuration, DTO, service, controller serah terima transfer; `M` `InpBedOccupancyService.cs`, `ApplicationDbContext.cs`, `Program.cs` |
| Langkah berikutnya | Migration `E7`; admin memberikan `TransferHandover : Send/Receive` |
