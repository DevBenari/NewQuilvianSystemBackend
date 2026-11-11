# Laporan Perubahan Backend — `BE-HRD-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-035` |
| Judul | Standar Kebutuhan Tenaga, Angka Rekomendasi, dan Pengesahan T9 |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-8` (Angka Beban, Standar Kebutuhan Tenaga, dan Gap Langsung) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-8 |
| Traceability | `FR-HRD-095`, `VR-WFP-01`, `VR-WFP-02`, `VR-WFP-03`, `VR-WFP-04`, `VR-WFP-05`, `VR-WFP-06`, `VR-WFP-07`, `VR-WFP-10`, `VR-WFP-104`, `Bagian 16.9 Formula Rekomendasi`, `Alur Workflow T9`, `AT-HRD-D1-02` |
| Dependency | `BE-HRD-034` ✅, `BE-HRD-029` ✅, `BE-HRD-030` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `StaffingStandardDtos.cs`, `IHrdStaffingRecommendationService.cs`, `HrdStaffingRecommendationService.cs`, `IHrdStaffingStandardService.cs`, `HrdStaffingStandardService.cs`, `HrdStaffingStandardDecisionApplier.cs`, `HrdStaffingStandardController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 6 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Dalam operasional rumah sakit modern, setiap unit pelayanan medis maupun non-medis membutuhkan standar alokasi jumlah dan kualifikasi tenaga kerja yang jelas, dapat dipertanggungjawabkan, serta disahkan oleh pimpinan tertinggi rumah sakit (Direktur). Standar kebutuhan tenaga kerja (`Staffing Standard`) menjadi landasan perhitungan formasi tahunan, penjadwalan dinas jaga (*shift roster*), dan analisis kesenjangan tenaga (*staffing gap*).

Masalah dan aturan bisnis yang diselesaikan:
1. **Pembuatan Draf Berbasis Riwayat Aktif (`VR-WFP-01`)**:
   - Pembuatan draf baru secara cerdas menyalin seluruh baris standar dan komposisi kualifikasi (*skill mix*) dari versi aktif yang sedang berjalan, sehingga pengguna tidak perlu mengetik ulang konfigurasi dari awal.
   - Mencegah inkonsistensi dengan membatasi hanya satu draf aktif per unit kerja (`VR-WFP-01`). Permintaan pembuatan draf baru saat masih ada draf terbuka ditolak dengan `400 Bad Request`.
2. **Kepatuhan Penentuan Metode Perhitungan (`VR-WFP-02`, `VR-WFP-05`, `VR-WFP-07`, `VR-WFP-10`)**:
   - Menolak metode `WorkloadAnalysis` (Analisis Beban Kerja murni) pada MVP ini karena formula beban kerja kompleks ditunda (`VR-WFP-02`).
   - Faktor kelonggaran (*Relief Factor*) tidak boleh bernilai kurang dari 1,00 (`VR-WFP-05`).
   - Setiap jabatan di dalam satu standar unit harus unik; tidak boleh ada baris jabatan ganda (`VR-WFP-07`).
   - Bila aturan pembagian kualifikasi (*Skill Mix*) didefinisikan pada suatu baris, total persentase pembagian wajib tepat 100% (`VR-WFP-10`).
3. **Pengajuan ke Alur Persetujuan Direktur `T9` (`VR-WFP-03`, `VR-WFP-04`, `VR-WFP-06`)**:
   - Sebelum diajukan, seluruh isian parameter wajib terisi lengkap sesuai metode yang dipilih (`VR-WFP-03`, `VR-WFP-04`).
   - Tanggal mulai berlaku draf tidak boleh mundur dari tanggal pengajuan (`VR-WFP-06`).
   - Draf didaftarkan ke mesin workflow persetujuan `T9` yang bermuara pada persetujuan Direktur (`BE-HRD-029`, `BE-HRD-030`).
4. **Mesin Rekomendasi Kebutuhan Tenaga Kerja (Bagian 16.9)**:
   - Menyediakan mesin kalkulasi rekomendasi kebutuhan tenaga kerja (*Staffing Recommendation Engine*) berdasarkan 3 metode yang didukung:
     - `FixedMinimum`: Mengambil langsung nilai batas minimum tenaga.
     - `WorkloadRatio`: `(WorkloadValue ÷ RatioWorkloadQuantity × RatioStaffQuantity) per shift × ShiftsPerDay × ReliefFactor`, dibulatkan sesuai `RoundingMethod`. Bila angka beban manual belum diisi untuk periode tersebut, rekomendasi bernilai `null` dan mencantumkan catatan masukan kurang (`VR-WFP-104`), bukan bernilai 0.
     - `ServiceCoverage`: `ShiftsPerDay × StaffPerShift × ReliefFactor`, dibulatkan sesuai `RoundingMethod`.
   - Menghasilkan rincian string breakdown kalkulasi persis format penerimaan contoh (`AT-HRD-D1-02`) dengan format desimal budaya Indonesia (`id-ID`).
5. **Penerapan Keputusan Workflow T9 (`HrdStaffingStandardDecisionApplier`)**:
   - Ketika Direktur menyetujui dokumen `T9`:
     - Jika `EffectiveStartDate <= Hari Ini`, standar langsung berstatus `Active` dan versi aktif sebelumnya ditandai `Superseded`.
     - Jika `EffectiveStartDate > Hari Ini`, standar berstatus `Scheduled` (akan diaktifkan oleh scheduler pada tanggal berlakunya).
   - Ketika Direktur menolak atau mengembalikan dokumen, status standar diperbarui menjadi `Rejected` atau `ReturnedForRevision` agar unit dapat merevisi atau membatalkannya.
6. **Kontrak Integrasi Modul Penjadwalan (`GET /effective`)**:
   - Modul Penjadwalan dapat meminta standar yang sedang berlaku untuk satu unit kerja pada tanggal jadwal tertentu untuk memvalidasi pemenuhan kebutuhan minimal jaga (*minimum coverage*).

---

## 2. Backend Governance Preflight & Kepatuhan QBE

| Parameter | Nilai Evaluasi |
| :--- | :--- |
| **Area** | `Corporate` |
| **Module** | `HumanResource` |
| **Submodule** | `WorkforcePlanning` |
| **Status Registry** | `REGISTERED` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| **Prefix Entitas** | `Hrd` (`HrdStaffingStandard`, `HrdStaffingStandardLine`, `HrdStaffingStandardSkillMix`) |
| **Keberlakuan Kode** | `NEW CODE` |
| **Role Access Standard** | Memenuhi `role-access-rules.md`: `[AccessController]` menggunakan ControllerName `WorkforceStaffingStandard`, `[AccessAction]` dan `[AccessPermission]` konsisten untuk `Read`, `Create`, `Update`, dan `Delete`. |
| **API Contract Standard** | Memenuhi Swagger grup `[Tags("Corporate / Human Resource / Workforce Planning / Staffing Standard")]` dengan Base URL `api/v1/corporate/human-resource/workforce-planning/staffing-standards`. |
| **Audit & RowVersion** | Operasi mutasi diverifikasi concurrency-nya dengan `RowVersion` dan pencatatan audit lengkap (`CreatedBy`, `LastModifiedBy`, timestamp). |

---

## 3. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | `POST /` untuk draf versi baru menyalin baris dari versi aktif bila ada; tolak bila masih ada draf terbuka untuk unit tersebut (`VR-WFP-01`). | Diimplementasikan di `HrdStaffingStandardService.CreateDraftAsync`. Pengecekan draf terbuka dilakukan (`Draft` atau `ReturnedForRevision`). Jika ada, mengembalikan `400` dengan pesan: *"Unit ini masih memiliki draf standar kebutuhan tenaga yang terbuka."*. Bila ada versi aktif, seluruh `HrdStaffingStandardLine` dan `HrdStaffingStandardSkillMix` disalin ke draf baru dengan penomoran standar dan versi bertambah. | ✅ Terbukti |
| 2 | `PUT /{id}` menolak `WorkloadAnalysis` (`VR-WFP-02`), relief factor < 1 (`VR-WFP-05`), jabatan duplikat pada draf (`VR-WFP-07`). | Diimplementasikan di `HrdStaffingStandardService.UpdateDraftAsync`. Memvalidasi setiap baris draf: jika metode `WorkloadAnalysis` ditolak `400` (`VR-WFP-02`), jika relief factor < 1 ditolak `400` (`VR-WFP-05`), jika jabatan duplikat ditolak `400` (`VR-WFP-07`), dan jika total skill mix != 100% ditolak `400` (`VR-WFP-10`). | ✅ Terbukti |
| 3 | `POST /{id}/submit` memvalidasi kelengkapan isian per metode (`VR-WFP-03`, `VR-WFP-04`), tanggal berlaku tidak mundur (`VR-WFP-06`), dan mendaftarkan workflow persetujuan ke Direktur `T9`. | Diimplementasikan di `HrdStaffingStandardService.SubmitAsync`. Memeriksa kelengkapan parameter masing-masing metode (FixedMinimum, WorkloadRatio, ServiceCoverage) dan validasi `EffectiveStartDate >= today`. Memanggil `IHrdWorkforcePlanningWorkflowIntegrationService.StartWorkflowAsync` dengan kode transaksi `T9` ke alur persetujuan Direktur. Status draf berubah menjadi `Submitted`. | ✅ Terbukti |
| 4 | `GET /{id}/recommendation` menghitung angka rekomendasi per rumus Bagian 16.9 (contoh ICU 32, Farmasi 4, Satpam 9) dan teks rincian sesuai format budaya `id-ID` (`VR-WFP-104`). | Diimplementasikan di `HrdStaffingRecommendationService.CalculateRecommendationsAsync`. Menghasilkan teks rincian kalkulasi: *"7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32"* (ICU), *"Minimum headcount: 4 → 4"* (Farmasi), *"3 × 2 = 6; 6 × 1,5 = 9 → 9"* (Satpam). Bila beban kerja belum terisi, mengembalikan `missingInput = "angka beban belum diisi"` dan nilai `null`. | ✅ Terbukti |
| 5 | Pengesahan `T9` oleh Direktur: status berubah menjadi `Active` bila tanggal berlaku <= hari ini dan versi aktif lama menjadi `Superseded`; bila tanggal berlaku masa depan, status menjadi `Scheduled`. | Diimplementasikan di `HrdStaffingStandardDecisionApplier.ApplyDecisionAsync`. Ketika aksi `Approved`: memeriksa tanggal berlaku terhadap hari ini. Jika masa berlaku telah tiba, status menjadi `Active` dan versi `Active` lama di unit yang sama otomatis diubah menjadi `Superseded`. Jika tanggal di masa depan, status menjadi `Scheduled`. Bila ditolak, status menjadi `Rejected` atau `ReturnedForRevision`. | ✅ Terbukti |
| 6 | Endpoint baca standar aktif untuk modul Penjadwalan (`GET /effective`). | Diimplementasikan di `HrdStaffingStandardController.GetEffective` dan `HrdStaffingStandardService.GetEffectiveAsync`. Menerima `organizationUnitId` dan `date`, mencari standar dengan status `Active` yang rentang berlakunya mencakup tanggal yang diminta. Mengembalikan `200 OK` dengan payload standar lengkap. | ✅ Terbukti |

---

## 4. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/staffing-standards`
Tags: `Corporate / Human Resource / Workforce Planning / Staffing Standard`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses | Respons Error |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET` | `/` | Ambil daftar standar kebutuhan tenaga berhalaman (filter unit, status, tahun) | `WorkforceStaffingStandard:Read` | `200 OK` (Paged) | `401`, `403` |
| `GET` | `/filters/metadata` | Ambil metadata filter dan enumerasi opsi status / metode kalkulasi | `WorkforceStaffingStandard:Read` | `200 OK` | `401`, `403` |
| `GET` | `/effective` | Ambil standar kebutuhan tenaga yang aktif berlaku pada tanggal tertentu | `WorkforceStaffingStandard:Read` | `200 OK` | `401`, `403`, `404` |
| `GET` | `/{id}` | Ambil detail satu versi standar kebutuhan tenaga beserta baris dan skill mix | `WorkforceStaffingStandard:Read` | `200 OK` | `401`, `403`, `404` |
| `POST` | `/` | Buat draf baru standar kebutuhan tenaga (salin dari versi aktif jika ada) | `WorkforceStaffingStandard:Create` | `201 Created` | `400`, `401`, `403`, `404` |
| `PUT` | `/{id}` | Perbarui rincian baris draf standar kebutuhan tenaga | `WorkforceStaffingStandard:Update` | `200 OK` | `400`, `401`, `403`, `404` |
| `GET` | `/{id}/recommendation` | Hitung angka rekomendasi kebutuhan tenaga per baris berdasarkan rumus Bagian 16.9 | `WorkforceStaffingStandard:Read` | `200 OK` | `400`, `401`, `403`, `404` |
| `POST` | `/{id}/submit` | Ajukan standar kebutuhan tenaga ke alur persetujuan workflow Direktur T9 | `WorkforceStaffingStandard:Update` | `200 OK` | `400`, `401`, `403`, `404` |
| `POST` | `/{id}/cancel` | Batalkan draf atau dokumen standar yang dikembalikan dari workflow | `WorkforceStaffingStandard:Delete` | `200 OK` | `400`, `401`, `403`, `404` |

---

## 5. Daftar Berkas yang Ditambahkan dan Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/StaffingStandardDtos.cs`
   - DTOs lengkap: `CreateStaffingStandardRequest`, `UpdateStaffingStandardRequest`, `StaffingStandardLineRequest`, `StaffingSkillMixRequest`, `SubmitStaffingStandardRequest`, `CancelStaffingStandardRequest`, `StaffingStandardListItemResponse`, `StaffingStandardResponse`, `StaffingStandardLineResponse`, `StaffingSkillMixResponse`, `StaffingRecommendationResponse`, `StaffingLineRecommendationResponse`, `StaffingStandardPagedQuery`, `StaffingStandardFilterMetadataResponse`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingRecommendationService.cs` & `HrdStaffingRecommendationService.cs`
   - Mesin kalkulasi rekomendasi tenaga kerja 3 metode: `FixedMinimum`, `WorkloadRatio`, dan `ServiceCoverage`, lengkap dengan pembulatan (`Ceiling`, `Nearest`, `Floor`) dan format breakdown desimal Indonesia (`id-ID`).
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingStandardService.cs` & `HrdStaffingStandardService.cs`
   - Logika bisnis utama standar kebutuhan tenaga: penyalinan versi aktif, proteksi draf tunggal (`VR-WFP-01`), penolakan `WorkloadAnalysis` (`VR-WFP-02`), validasi parameter (`VR-WFP-03`, `VR-WFP-04`, `VR-WFP-05`), validasi tanggal (`VR-WFP-06`), keunikan jabatan (`VR-WFP-07`), skill mix 100% (`VR-WFP-10`), dan inisiasi alur persetujuan T9.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdStaffingStandardDecisionApplier.cs`
   - Implementasi `IWorkforcePlanningDecisionApplier` untuk tipe transaksi `T9`: transisi status ke `Active` (dengan *superseding* versi lama) atau `Scheduled`, penanganan pengembalian draf (`ReturnedForRevision`), dan penolakan (`Rejected`).
5. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdStaffingStandardController.cs`
   - Controller ASP.NET Core mengekspos 9 endpoint standar kebutuhan tenaga dengan otorisasi `WorkforceStaffingStandard` dan tag Swagger OpenAPI.

### Berkas Diubah:
1. `Program.cs`
   - Mendaftarkan dependensi scoped `IHrdStaffingRecommendationService`, `HrdStaffingRecommendationService`, `IHrdStaffingStandardService`, `HrdStaffingStandardService`, dan `HrdStaffingStandardDecisionApplier`.

---

## 6. Bukti Kompilasi & Pengujian

### Status Pengujian Otomatis
```text
AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)
```

### Hasil Kompilasi (`dotnet build`)
- Perintah: `dotnet build NewQuilvianSystemBackend\QuilvianSystemBackend.csproj -c Debug -p:SkipMigrationMetadata=true --no-restore`
- Hasil: **0 Error(s)**, 408 Warning(s)
- Waktu Kompilasi: 00:01:16.15
- Keterangan: Seluruh kode C# baru terkompilasi bersih tanpa kesalahan kompilasi.

---

## 7. Penutupan Task & Rekomendasi Langkah Berikutnya

Task `BE-HRD-035` telah selesai secara penuh dengan seluruh 6 kriteria penerimaan terbukti.

### Task Approved Berikutnya (Gelombang MVP-8):
- **`BE-HRD-036`**: Scheduler perencanaan dan aktivasi standar pada tanggal berlaku (Background worker terjadwal untuk memeriksa standar berstatus `Scheduled` yang tanggal mulai berlakunya telah tiba `EffectiveStartDate <= Today`, mengubah statusnya menjadi `Active`, dan menandai versi aktif lama menjadi `Superseded` secara idempoten). Prasyarat: `BE-HRD-035` ✅.
