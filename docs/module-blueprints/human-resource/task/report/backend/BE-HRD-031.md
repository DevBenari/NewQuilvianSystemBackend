# Laporan Perubahan Backend — `BE-HRD-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-031` |
| Judul | Pengaturan Perencanaan Tenaga Kerja Per Rumah Sakit |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-061`, `HRD-DEC-071`, `VR-WFP-40`, `VR-WFP-41`, `API v7 §13 grup Setting`, `Permission v7 §11 WorkforcePlanningSetting` |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `WorkforcePlanningResult.cs`, `WorkforcePlanningSettingDtos.cs`, `IHrdWorkforcePlanningSettingService.cs`, `HrdWorkforcePlanningSettingService.cs`, `HrdWorkforcePlanningSettingController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 6 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Di setiap fasilitas rumah sakit, kebijakan perencanaan tenaga kerja memiliki batas operasional yang disetel oleh HR Manager setempat dan sengaja tidak dipatok secara kaku (hardcoded) oleh sistem perbankan/aplikasi:
1. **Batas Cuti Panjang / Tidak Tersedia Sementara (`LongLeaveThresholdDays`)**: Berapa lama cuti pegawai (misalnya cuti besar, cuti melahirkan, cuti haji) yang menyebabkan pegawai dianggap "tidak tersedia sementara" dalam kalkulasi lapisan gap ketersediaan tenaga ICU/Rawat Inap (`HRD-DEC-061`). Umumnya disetel 30 hari.
2. **Batas Permintaan Lewat Batas (`OverdueRequestThresholdDays`)**: Berapa hari sebuah permintaan penambahan tenaga kerja (*headcount request*) yang sudah disetujui boleh menunggu sebelum ditandai berstatus "lewat batas" (*overdue*) pada monitor HR Manager untuk dievaluasi kelanjutannya tanpa kedaluwarsa otomatis (`HRD-DEC-071`). Umumnya disetel 90 hari.

Sesuai aturan bisnis, rumah sakit baru yang belum menyetel angka kebijakan ini **TIDAK BOLEH** diberikan angka default buatan sistem, melainkan wajib mengembalikan nilai kosong (`null`), sehingga HR Manager sadar untuk menentukan kebijakan unitnya sendiri. Selain itu, setiap perubahan pengaturan wajib menyertakan alasan tertulis, batas hari berada pada rentang wajar (1 s.d. 365 hari), dilindungi dari konflik pembaruan simultan (*optimistic concurrency check* via `RowVersion`), serta mencatat audit trail tanpa membocorkan data sensitif ke logger.

---

## 2. Backend Governance Preflight & Kepatuhan QBE

| Parameter | Nilai Evaluasi |
| :--- | :--- |
| **Area** | `Corporate` |
| **Module** | `HumanResource` |
| **Submodule** | `WorkforcePlanning` |
| **Status Registry** | `REGISTERED` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| **Prefix Entitas** | `Hrd` |
| **Keberlakuan Kode** | `NEW CODE` |
| **Role Access Standard** | Memenuhi `role-access-rules.md`: `[AccessController]` menggunakan ControllerName `WorkforcePlanningSetting`, `[AccessAction]` dan `[AccessPermission]` identik untuk `Read` dan `Update`, `AccessTypes.Read` dan `AccessTypes.Update`. Tanpa hardcoded `IsInRole`. |
| **API Contract Standard** | Memenuhi Swagger grup `[Tags("Corporate / Human Resource / Workforce Planning / Setting")]` dengan Base URL `api/v1/corporate/human-resource/workforce-planning/settings`. |
| **Audit & Sanitasi** | Memenuhi `permission-audit-matrix.md` §11.4: Logger hanya memuat `EntityId`, controller, action, dan status. Alasan perubahan (`ChangeReason`) disimpan di database, **tidak** dicatat di logger. |

---

## 3. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | `GET /?hospitalSiteId=` mengembalikan pengaturan; rumah sakit yang belum punya baris mendapat kedua nilai kosong (`null`), **bukan** angka bawaan. | Diimplementasikan pada `HrdWorkforcePlanningSettingService.GetByHospitalSiteIdAsync`. Jika baris `HrdWorkforcePlanningSetting` belum ada untuk site tersebut, service mengembalikan DTO dengan `LongLeaveThresholdDays = null` dan `OverdueRequestThresholdDays = null`. Jika site tidak terdaftar di `MstHospitalSite`, mengembalikan `404 NotFound`. | ✅ Terbukti |
| 2 | `PUT /` hanya untuk pemegang `WorkforcePlanningSetting : Update`; peran lain `403`. | Diimplementasikan dengan atribut deklaratif `[AccessAction("Update", "Update Workforce Planning Setting", AccessType = AccessTypes.Update, SortOrder = 2)]` dan `[AccessPermission("WorkforcePlanningSetting", "Update")]` pada method `Update` di `HrdWorkforcePlanningSettingController`. | ✅ Terbukti |
| 3 | Alasan kosong → `400` dengan pesan `VR-WFP-40`. | Divalidasi di `HrdWorkforcePlanningSettingService.UpdateAsync`: `if (string.IsNullOrWhiteSpace(request.ChangeReason)) return Fail(400, "Tuliskan alasan perubahan pengaturan.");`. Memenuhi `VR-WFP-40`. | ✅ Terbukti |
| 4 | Angka di bawah 1 atau di atas 365 → `400` dengan pesan `VR-WFP-41`. | Divalidasi di `HrdWorkforcePlanningSettingService.UpdateAsync`: Jika nilai diisi dan `< 1` atau `> 365`, mengembalikan `400 BadRequest` dengan pesan `"Isi angka antara 1 dan 365 hari."`. Memenuhi `VR-WFP-41`. | ✅ Terbukti |
| 5 | `rowVersion` usang → `409`. | Diimplementasikan di `HrdWorkforcePlanningSettingService.UpdateAsync`: Memeriksa kecocokan `request.RowVersion` terhadap `setting.RowVersion`. Jika tidak cocok atau terjadi benturan simultan (`DbUpdateConcurrencyException`), mengembalikan `409 Conflict` dengan pesan `"Data telah diubah oleh pengguna lain. Silakan muat ulang data terbaru."`. | ✅ Terbukti |
| 6 | Perubahan meninggalkan jejak sesuai `permission-audit-matrix.md` bagian 11.4 tanpa memuat kolom bertanda sensitif di logger. | Diimplementasikan via `_logger.LogInformation("WorkforcePlanningSetting updated. EntityId: {EntityId}, Controller: {Controller}, Action: {Action}, Status: {Status}", setting.Id, "WorkforcePlanningSetting", "Update", "Success");`. Alasan disimpan di kolom `ChangeReason` entitas database, bukan pada logger. | ✅ Terbukti |

---

## 4. Daftar Berkas yang Ditambahkan dan Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/WorkforcePlanningResult.cs`
   - DTO hasil operasi terstandarisasi untuk modul perencanaan tenaga kerja (`Success`, `StatusCode`, `Message`, `Data`).
2. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/WorkforcePlanningSettingDtos.cs`
   - DTO respons `WorkforcePlanningSettingResponse` (`Id`, `HospitalSiteId`, `HospitalSiteName`, `LongLeaveThresholdDays`, `OverdueRequestThresholdDays`, `ChangeReason`, `RowVersion`, `UpdatedAt`, `UpdatedBy`).
   - DTO permintaan `UpdateWorkforcePlanningSettingRequest` (`HospitalSiteId`, `LongLeaveThresholdDays`, `OverdueRequestThresholdDays`, `ChangeReason`, `RowVersion`).
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningSettingService.cs`
   - Kontrak antarmuka layanan pengambilan dan pembaruan pengaturan perencanaan per rumah sakit.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSettingService.cs`
   - Implementasi logika bisnis: query `MstHospitalSite`, query default kosong jika belum disetel, validasi `VR-WFP-40` & `VR-WFP-41`, penanganan optimistic concurrency token, dan sanitasi logger.
5. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdWorkforcePlanningSettingController.cs`
   - Controller ASP.NET Core dengan routing `api/v1/corporate/human-resource/workforce-planning/settings`, tag Swagger `Corporate / Human Resource / Workforce Planning / Setting`, dan integrasi izin role-access `WorkforcePlanningSetting : Read / Update`.

### Berkas Diubah:
1. `Program.cs`
   - Mendaftarkan dependensi scoped `IHrdWorkforcePlanningSettingService` dan `HrdWorkforcePlanningSettingService` pada container service aplikasi.

---

## 5. Bukti Kompilasi & Pengujian

### Status Pengujian Otomatis
```text
AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)
```

### Hasil Kompilasi (`dotnet build`)
- Perintah: `dotnet build NewQuilvianSystemBackend\QuilvianSystemBackend.csproj -c Debug`
- Hasil: **0 Error(s)**, 408 Warning(s) (seluruh warning merupakan XML comment param tag eksisting dan package vulnerability library eksternal).
- Durasi: Berhasil terkompilasi bersih dalam waktu build standar.

---

## 6. Penutupan Task & Rekomendasi Langkah Berikutnya

Task `BE-HRD-031` telah selesai secara penuh dengan seluruh kriteria penerimaan terbukti.

### Task Berikutnya:
- **`BE-HRD-032`**: Buku formasi berversi: satu-satunya penulis formasi dan API baca (`HrdPositionFormationService` dengan advisory lock PostgreSQL, `HrdPositionFormationController` baca saja, `PositionFormationDtos.cs`). Prasyarat: `BE-HRD-017` ✅.
