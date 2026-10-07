# Laporan Perubahan Backend — `BE-RWI-168`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-168` |
| Judul | Pemakaian alat dan tagihannya (`K9` bagian pemakaian alat) |
| Slice | `MVP-2` / `RWF-W4` — Pemakaian Alat |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-168` |
| Trace | `FR-RWF-062` s.d. `066`, `FR-RWF-069`; `RWI-DEC-179`, `RWI-DEC-192` butir 4; `INV-RWF-13`, `14`; `INT-RWF-06`, `07`, `08`; `AC-RWF-061` s.d. `064`; `UAT-RWF-06`, `UAT-RWF-27` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.3; integrasi 9.2; backend 12.5, 12.7 |
| Dependency | `BE-RWI-167` ✅; `BE-RWI-153` [IB] ✅ (dikerjakan pada sesi yang sama); `BE-RWI-155` [IB] ✅ |
| Klasifikasi | `HEAVY` — skor 12: repository 0, berkas diperiksa 2, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/**`, `Areas/HealthServices/BillingManagement/**` (jembatan folio), `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, perbaikan, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kesembilan acceptance criteria terpetakan ke source; build akhir `PASS`; tabel diterapkan ke database development |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (pemakaian, `RWI-DEC-180`); `BillingManagement` (jembatan folio, milik Yasmina); `InPatientManagement` (pemanggil saat keluar ruangan) |
| Prefix registry | `Cli` — `ACTIVE / LEGACY`; `Bil` — `ACTIVE` (tidak ada tabel Billing baru) |
| Keberlakuan | `NEW CODE` (dua model, enum, service, worker, controller, DTO); `TOUCHED LEGACY` (lima berkas jembatan Billing) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`/`002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Menu Pemakaian Alat di Rawat Inap masih *placeholder* (`RWI-DEC-179` membatalkan penundaan `RWI-DEC-089`). Ventilator, syringe pump, dan alat besar lain tidak tercatat dan tidak pernah sampai ke invoice `RANAP`. Rumah sakit kehilangan pendapatan, dan tidak ada jejak siapa yang memasang alat atas tanggung jawab dokter siapa.

## 2. Proses bisnis

**Tujuan.** Perawat mencatat mulai dan selesai pemakaian alat; server menghitung unit dan menerbitkan tagihan ke invoice `RANAP` lewat jalur klinis yang sama dengan tindakan; pemakaian yang masih berjalan ditutup otomatis saat pasien keluar ruangan.

**Pelaku.**

| Pelaku | Peran |
| --- | --- |
| Perawat bangsal | Memulai, menyelesaikan, membatalkan, dan mengoreksi pemakaian; pelaksana diambil dari akun login |
| Dokter penanggung jawab | Wajib berpenugasan aktif pada episode saat pemakaian dimulai |
| Billing | Menentukan tarif dan membentuk baris invoice; Clinical tidak mengirim harga |

**Langkah utama.**

1. Perawat memilih alat aktif (dari `medical-equipments/options`), dokter penanggung jawab, waktu mulai, dan jumlah bila satuannya per pemakaian.
2. Server mengunci episode dan baris alat, memeriksa episode masih aktif dan pasien belum keluar, memeriksa penugasan dokter, lalu membekukan satuan dan pembulatan alat pada pemakaian itu.
3. Saat selesai, server menghitung unit dari waktu mulai dan selesai sesuai satuan dan pembulatan, lalu menyimpan status `Completed`.
4. Sesudah commit, server menerbitkan fakta klinis `EQUIPMENT_USAGE` (jumlah = unit, satuan, cuplikan aturan). Billing mencari `MstTariff` dengan `MedicalEquipmentId` dan kelas pasien, lalu membentuk baris invoice `RANAP`.
5. Bila tarif tidak ada, efek Billing berstatus `TARIFF_NOT_FOUND`, layar menampilkan "tarif belum ada", dan invoice tidak dapat difinalkan (`BIL-FIN-021`).

**Rumus unit.**

| Satuan | Pembulatan ke atas (bawaan) | Proporsional |
| --- | --- | --- |
| Per hari | `⌈durasi jam ÷ 24⌉` | `durasi ÷ 24`, dua desimal |
| Per jam | `⌈durasi jam⌉` | `durasi jam`, dua desimal |
| Per pemakaian | Jumlah yang diisi | Jumlah yang diisi |

**Contoh berangka (`UAT-RWF-06`).** Ventilator per hari, pembulatan ke atas, dipasang 1 Okt 08.00 dan dilepas 3 Okt 11.00. Durasi 51 jam = 2,125 hari, dibulatkan menjadi **3 unit**. Fakta terkirim `Quantity = 3`. Dengan tarif kelas 2 Rp 1.200.000 per hari (contoh), Billing membentuk satu baris Rp 3.600.000. Contoh lain: syringe pump per jam selama 5 jam = 5 unit; alat per jam proporsional selama 90 menit = 1,5 unit.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Dokter penanggung jawab tidak berpenugasan aktif | `403` |
| Waktu selesai mendahului waktu mulai, melewati sekarang, atau melewati waktu keluar pasien | `422` `CLI-EQP-001` |
| Batal atau koreksi saat invoice rawat inap bukan `OPEN` (atau Billing tidak terbaca) | `422` `CLI-EQP-002` |
| Episode bukan `Admitted`/`DischargePending`, atau pasien sudah keluar ruangan | `422` `CLI-EQP-003` |
| Versi berubah | `409` |
| Satuan tagih alat diubah selama ada pemakaian `Running` | `422` `MST-EQP-002` (master alat) |
| Respons tidak memuat rupiah | `ChargeState` saja: `PENDING`, `RECOGNIZED`, `TARIFF_NOT_FOUND`, `VOIDED` |

**Perubahan status pemakaian.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Mulai | `Running` | `EquipmentUsage : Create` | Alat aktif, dokter berpenugasan, episode aktif |
| `Running` | Selesai | `Completed` | `EquipmentUsage : Update` | Waktu sah |
| `Running` | Pasien keluar ruangan | `Completed` + "perlu diperiksa perawat" | Sistem | Dipicu `record-departure` |
| `Running`/`Completed` | Batal dengan alasan | `Cancelled` | `EquipmentUsage : Cancel` | Invoice `OPEN` |
| `Running`/`Completed` | Koreksi waktu | Tetap, revisi naik | `EquipmentUsage : Correct` | Invoice `OPEN`, alasan wajib |

**Jalur tidak normal.**

- **Koreksi waktu.** Nilai lama disimpan di `CliEquipmentUsageRevision`, unit dihitung ulang, dan fakta versi baru diterbitkan. Billing membandingkan dengan baris yang sudah ada lalu membuat penyesuaian selisih, bukan baris kedua. Contoh: waktu selesai dikoreksi menjadi 2 Okt 07.00 (47 jam → 2 unit), sehingga Billing mengkredit Rp 1.200.000.
- **Pembatalan.** Fakta pembatalan hanya untuk `EquipmentUsageId` itu; charge alkes Farmasi tidak tersentuh. Pembatalan sebelum ada tagihan tercatat sebagai pembatalan tanpa konsekuensi finansial.
- **Pasien keluar saat alat masih `Running`.** Setelah transaksi keluar ruangan commit, pemakaian ditutup pada waktu keluar dan ditandai `RequiresNurseReview`. Bila penutupan gagal, keluar ruangan tetap tersimpan, respons membawa peringatan, dan worker mencoba ulang tiap menit.
- **Billing gagal menerima fakta.** Pemakaian tetap tersimpan; worker menerbitkan ulang fakta yang belum terkirim.

**Hasil akhir.** Pemakaian alat tercatat lengkap dengan pelaksana, dokter, dan revisi, lalu tertagih di invoice `RANAP` tanpa rupiah di layar perawat.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.3, integrasi 9.2, `ClinicalMilestoneFactProducer` (versi, pembatalan tanpa tagihan sebelumnya, larangan terbit di dalam transaksi), `BillingClinicalChargeBridgeService` (rencana penyesuaian per versi), `BillingSourceTariffResolver`, `BillingFinalizationService` (`BIL-FIN-021`), `BillingChargeSourceAdapter`, `IInpBillingClearanceAdapter`, `InpatientClinicalContextService.IsDoctorAssignedAsync`, dan registrasi Billing di `BillingManagementServiceCollectionExtensions`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliEquipmentUsage.cs` | Baru. Pemakaian per episode dengan satuan/pembulatan beku, unit, status, tanda periksa perawat |
| `Areas/HealthServices/ClinicalManagement/Models/CliEquipmentUsageRevision.cs` | Baru. Nilai lama setiap koreksi |
| `Areas/HealthServices/ClinicalManagement/Enums/CliEquipmentUsageStatus.cs` | Baru. `Running`, `Completed`, `Cancelled` |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliEquipmentUsageConfigurations.cs` | Baru. FK `Restrict` ke episode, kunjungan, pasien, alat, dokter, pengguna; index pemakaian berjalan per episode |
| `Areas/HealthServices/ClinicalManagement/DTOs/EquipmentUsageDtos.cs` | Baru. Request dan response tanpa rupiah |
| `Areas/HealthServices/ClinicalManagement/Services/CliEquipmentUsageService.cs` | Baru. Mulai, selesai, batal, koreksi, tutup saat keluar ruangan, pemulihan; penerbitan fakta sesudah commit. Review Claude: query fakta yang tidak dipakai di `MapAsync` dihapus |
| `Areas/HealthServices/ClinicalManagement/Services/CliEquipmentUsageWorker.cs` | Baru. Tiap menit memulihkan penutupan saat keluar dan penyerahan fakta |
| `Areas/HealthServices/ClinicalManagement/Controllers/EquipmentUsageController.cs` | Baru. Lima endpoint |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | Konteks sumber `EQUIPMENT_USAGE` dan efek `EquipmentUsageCharge` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | Kebijakan: ditagih saat `COMPLETED`, batal saat `CANCELLED`/`VOIDED` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Domain `EQUIPMENT_USAGE` dikenali jembatan; `GetEquipmentChargeStateAsync` untuk `ChargeState` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | Tarif dicari lewat `MedicalEquipmentId`, kelas, dan klinik; tanpa tarif → `TARIFF_NOT_FOUND` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` | `EQUIPMENT_USAGE` masuk daftar domain milik jembatan (tidak bisa diketik manual lewat `from-source`) |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs` | Memanggil `CloseRunningForDepartureAsync` sesudah commit keluar ruangan (bersama `BE-RWI-153`) |
| `Areas/HealthServices/MasterData/Services/MedicalEquipmentService.cs` | Pemeriksaan `MST-EQP-002` (bersama `BE-RWI-167`) |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | DbSet jamak; registrasi service dan worker |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | Tabel `CliEquipmentUsage`, `CliEquipmentUsageRevision` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint API 8.3. Delta: `finish` dan `cancel` menerima `PATCH` sesuai kontrak dan alias `POST` sesuai standar endpoint transaksi |
| Database | Tabel baru `CliEquipmentUsage`, `CliEquipmentUsageRevision`; tidak ada tabel Billing baru. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Resource baru `EquipmentUsage` (`Read`, `Create`, `Update`, `Cancel`, `Correct`) di modul `HEALTH_SERVICE_CLINICAL`. Penugasan dokter dan hak tulis perawat pada episode diperiksa dari data |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Equipment Usage

Base URL: `api/v1/health-services/clinical-management/equipment-usages`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Riwayat alat kesehatan per episode | `EquipmentUsage : Read` | Query `episodeId`, `status` | `List<EquipmentUsageResponse>` |
| `POST` | `/` | Mulai pemakaian | `EquipmentUsage : Create` | `StartEquipmentUsageRequest` | `EquipmentUsageResponse` (`201`) |
| `PATCH` / `POST` | `/{id}/finish` | Selesai; server menghitung unit dan menerbitkan tagihan | `EquipmentUsage : Update` | `FinishEquipmentUsageRequest` | `EquipmentUsageResponse` |
| `PATCH` / `POST` | `/{id}/cancel` | Batal dengan alasan; hanya tagihan pemakaian ini yang batal | `EquipmentUsage : Cancel` | `CancelEquipmentUsageRequest` | `EquipmentUsageResponse` |
| `PUT` | `/{id}/time-correction` | Koreksi waktu berversi | `EquipmentUsage : Correct` | `CorrectEquipmentUsageRequest` | `EquipmentUsageResponse` |

Kode status: `400` isian atau alasan tidak sah, alat nonaktif; `403` dokter tidak berpenugasan atau perawat tidak berwenang; `404` tidak ditemukan; `409` versi berubah atau pemakaian sudah berakhir/dibatalkan; `422` `CLI-EQP-001` waktu tidak sah, `CLI-EQP-002` invoice bukan `OPEN`, `CLI-EQP-003` episode tidak aktif atau pasien sudah keluar.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir. Warning di `BillingInvoiceService.cs` (CS8073, baris 125–540) sudah ada di baris yang tidak disentuh |
| Build tahap penyelesaian | Error Codex `MstDoctor.DoctorName` (seharusnya `FullName`) sudah diperbaiki Codex sebelum terhenti | `NEW ERROR` (sudah diperbaiki) | Log build Codex |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | Dua tabel pemakaian di migration `20261005071042`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan statis atribut hak akses | Lima endpoint cocok `EquipmentUsage` ↔ action | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — 1 Okt 08.00 s.d. 3 Okt 11.00 per hari | 51 jam ÷ 24 = 2,125 → `decimal.Ceiling` = 3 | `PASS` | `CliEquipmentUsageService.CalculateBilledUnits` |
| Verifikasi proses bisnis — tarif tidak ada | `Build(null, …)` → `Rejected(TariffNotFound)`; finalisasi diblokir `BIL-FIN-021` | `PASS` | `BillingSourceTariffResolver.Build`; `BillingFinalizationService.cs:245-302` |
| Verifikasi proses bisnis — koreksi tanpa dobel | Versi fakta baru → `BuildAdjustmentPlanAsync` membuat penyesuaian selisih | `PASS` | `BillingClinicalChargeBridgeService.cs:243-322` |
| Verifikasi proses bisnis — keluar ruangan saat `Running` | Ditutup sesudah commit, `RequiresNurseReview = true`; kegagalan → peringatan + worker | `PASS` | `InpDischargeService.Departure.cs`; `CloseRunningForDepartureAsync`, `RepairDepartureAndDeliveryAsync` |
| Uji dengan Billing sungguhan dan UAT `UAT-RWF-06`/`27` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi dan worker tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ventilator per hari, pembulatan ke atas, 1 Okt 08.00 s.d. 3 Okt 11.00 → 3 unit dan satu baris invoice `RANAP` 3 × tarif kelas | Terpenuhi (source) | `CalculateBilledUnits`; fakta `Quantity = BilledUnits`; resolver `MedicalEquipmentId` + kelas; jembatan `RANAP` `BE-RWI-155` |
| 2. Tarif tidak ada → `TARIFF_NOT_FOUND` dan invoice tidak dapat difinalkan | Terpenuhi | Resolver `Build`; `BIL-FIN-021` |
| 3. Batal saat invoice `FINAL` → 422 `CLI-EQP-002` | Terpenuhi | `CancelAsync` (`InvoiceStatus != "OPEN"`) |
| 4. Pembatalan hanya membatalkan charge pemakaian itu; charge alkes Farmasi tidak tersentuh | Terpenuhi | Fakta pembatalan per `SourceAggregateId`; kebijakan `EQUIPMENT_USAGE` |
| 5. Koreksi waktu menyimpan revisi lama dan memperbarui tagihan tanpa dobel | Terpenuhi | `CorrectAsync` + `CliEquipmentUsageRevision`; penyesuaian selisih di jembatan |
| 6. Dokter penanggung jawab tidak berpenugasan → 403 | Terpenuhi | `StartAsync` (`IsDoctorAssignedAsync`) |
| 7. Pasien keluar ruangan saat `Running` → ditutup pada waktu keluar dan ditandai perlu diperiksa; kegagalan tidak membatalkan keluar ruangan | Terpenuhi | `CloseRunningForDepartureAsync` dipanggil di scope terpisah sesudah commit |
| 8. Respons tanpa rupiah | Terpenuhi | `EquipmentUsageResponse` tanpa field harga |
| 9. Mengubah satuan tagih alat yang sedang dipakai → 422 `MST-EQP-002` | Terpenuhi | `MedicalEquipmentService.SaveAsync` |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi proses bisnis dengan Billing sungguhan | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Fakta klinis hanya boleh terbit di luar transaksi; seluruh pemanggilan sudah sesudah `CommitAsync`/`DisposeAsync` |
| Masalah yang diketahui | **Gap integrasi 9.2:** resolver memakai `MstTariff.NormalPrice`; harga kontrak penjamin lewat `MstInsuranceTariff` belum diterapkan, sama seperti domain jembatan lain. Perlu task Billing (Yasmina) |
| Risiko tersisa | Belum diuji bersama Billing sungguhan; worker pemulihan belum terlihat berjalan di runtime |
| Perubahan sampingan | Review Claude: query fakta yang tidak dipakai di `MapAsync` dihapus; DbSet dinamai jamak |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-188` (dua tab Pemakaian Alat); UAT `UAT-RWF-06`/`27` bersama Billing; keputusan harga penjamin alat |
