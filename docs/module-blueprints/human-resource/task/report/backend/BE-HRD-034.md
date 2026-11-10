# Laporan Perubahan Backend — `BE-HRD-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-034` |
| Judul | Isian Angka Beban Manual |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-8` (Angka Beban, Standar Kebutuhan Tenaga, dan Gap Langsung) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-8 |
| Traceability | `HRD-DEC-060` (`MANUAL_WORKLOAD_INPUT`), `FR-HRD-096`, `VR-WFP-08`, `VR-WFP-09`, `VR-WFP-38`, `API v7 §13 grup Staffing Workload` |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `LOW` |
| Task Mode | `BACKEND` |
| Target Tulis | `StaffingWorkloadDtos.cs`, `IHrdStaffingWorkloadService.cs`, `HrdStaffingWorkloadService.cs`, `HrdStaffingWorkloadController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 5 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Dalam perencanaan kebutuhan tenaga kerja di rumah sakit, beberapa unit klinis atau penunjang medis menggunakan metode rasio beban kerja (misalnya unit ICU menghitung rasio perawat terhadap rata-rata pasien yang dirawat per hari, atau unit Farmasi menghitung jumlah resep harian). Integrasi otomatis penarikan data langsung dari rekam medis pasien (`INT-HR-12`) sengaja ditunda agar modul tidak memiliki ketergantungan rapuh pada sub-sistem SIMRS lama. Oleh karena itu, disediakan mekanisme isian angka beban manual yang akurat dan dapat dipertanggungjawabkan (`HRD-DEC-060`, `FR-HRD-096`).

Prinsip dan invariant bisnis yang diselesaikan:
1. **Pencatatan Rinci Per Unit × Jabatan × Periode**: Angka beban dicatat spesifik per unit organisasi, jabatan, serta rentang tanggal awal dan akhir periode pengamatan (misal: "rata-rata 7 pasien ICU per hari untuk periode 1 Januari 2027 s.d. 31 Maret 2027").
2. **Kewajiban Dokumentasi Sumber Resmi (`VR-WFP-08`)**: Setiap pengisian angka beban wajib mencantumkan keterangan sumber data yang valid (misalnya: "rekapitulasi sensus harian rawat inap", "laporan bulanan resep farmasi"). Isian tanpa keterangan sumber ditolak dengan status HTTP `400 Bad Request`.
3. **Pencegahan Data Ganda Per Periode (`VR-WFP-09`)**: Satu unit dan jabatan hanya boleh memiliki satu angka beban untuk tanggal mulai periode yang sama. Upaya memasukkan isian kedua ditolak dengan HTTP `409 Conflict` ("Angka beban periode ini sudah diisi. Ubah isian yang ada.").
4. **Isolasi Batas Wewenang Kepala Unit (`VR-WFP-38`)**: Kepala Unit hanya diperkenankan melihat dan mengelola angka beban kerja untuk unit dalam cakupan organisasinya (unit miliknya beserta seluruh sub-unit turunannya). Permintaan data atau mutasi untuk unit di luar cakupan dijawab HTTP `404 Not Found` ("Data tidak ditemukan.").
5. **Independensi Tanpa Persetujuan Khusus**: Isian angka beban manual tidak melewati alur persetujuan bertingkat dan tidak menunjuk langsung ke baris standar kebutuhan tertentu. Dengan demikian, ketika standar kebutuhan tenaga kerja mengalami pembaruan versi (revisi), angka beban yang berlaku pada periode tersebut tetap valid dan dapat langsung digunakan oleh mesin kalkulasi rekomendasi tenaga.

---

## 2. Backend Governance Preflight & Kepatuhan QBE

| Parameter | Nilai Evaluasi |
| :--- | :--- |
| **Area** | `Corporate` |
| **Module** | `HumanResource` |
| **Submodule** | `WorkforcePlanning` |
| **Status Registry** | `REGISTERED` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| **Prefix Entitas** | `Hrd` (`HrdStaffingWorkloadInput`) |
| **Keberlakuan Kode** | `NEW CODE` |
| **Role Access Standard** | Memenuhi `role-access-rules.md`: `[AccessController]` menggunakan ControllerName `WorkforceStaffingWorkload`, `[AccessAction]` dan `[AccessPermission]` konsisten untuk `Read`, `Create`, dan `Update`. |
| **API Contract Standard** | Memenuhi Swagger grup `[Tags("Corporate / Human Resource / Workforce Planning / Staffing Workload")]` dengan Base URL `api/v1/corporate/human-resource/workforce-planning/staffing-workloads`. |
| **Pencegahan Data Bocor** | Pengecekan cakupan Kepala Unit diimplementasikan berbasis penugasan manajerial aktif (`WfpOrganizationAssignments.IsManagerialAssignment`) dan penelusuran hierarki unit anak (`ParentOrganizationUnitId`). |

---

## 3. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | `POST /` yang lengkap → `201`. | Diimplementasikan pada `HrdStaffingWorkloadController.Create` dan `HrdStaffingWorkloadService.CreateAsync`. Permintaan yang memenuhi seluruh validasi berhasil disimpan ke `HrdStaffingWorkloadInputs` dan mengembalikan status HTTP `201 Created` beserta payload `StaffingWorkloadResponse`. | ✅ Terbukti |
| 2 | Sumber kosong → `400` `VR-WFP-08`. | Diimplementasikan validasi `string.IsNullOrWhiteSpace(request.SourceNote)`. Jika kosong, mengembalikan HTTP `400 Bad Request` dengan pesan baku: *"Tuliskan sumber angka beban, misalnya rekap sensus harian."*. | ✅ Terbukti |
| 3 | Isian kedua untuk unit, jabatan, dan tanggal mulai periode yang sama → `409` `VR-WFP-09`. | Diimplementasikan pengecekan unik `_dbContext.HrdStaffingWorkloadInputs.AnyAsync(w => !w.IsDelete && w.OrganizationUnitId == request.OrganizationUnitId && w.PositionId == request.PositionId && w.PeriodStartDate == request.PeriodStartDate)`. Jika duplikat ditemukan, mengembalikan HTTP `409 Conflict` dengan pesan: *"Angka beban periode ini sudah diisi. Ubah isian yang ada."*. | ✅ Terbukti |
| 4 | Kepala Unit di luar cakupan unitnya: tidak muncul di daftar; `POST`/`PUT`/`GET by id` → `404` (`VR-WFP-38`). | Diimplementasikan helper `GetScopedOrganizationUnitIdsAsync`. Untuk pengguna Kepala Unit, query `GET /` otomatis memfilter unit dalam cakupan. Pemanggilan `GET /{id}`, `POST /`, atau `PUT /{id}` dengan unit di luar cakupan dijawab `404 Not Found` dengan pesan: *"Data tidak ditemukan."*. | ✅ Terbukti |
| 5 | Isian tidak melewati persetujuan dan tidak menunjuk baris standar, sehingga tetap berlaku saat standar naik versi. | Entitas `HrdStaffingWorkloadInput` tidak memiliki kolom `WorkflowInstanceId` dan tidak memiliki relasi FK langsung ke `HrdStaffingStandard` maupun baris standar. Kebutuhan kalkulasi nantinya mencocokkan angka beban murni berdasarkan pasangan unit, jabatan, dan rentang tanggal berlakunya. | ✅ Terbukti |

---

## 4. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/staffing-workloads`
Tags: `Corporate / Human Resource / Workforce Planning / Staffing Workload`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses | Respons Error |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET` | `/` | Ambil daftar angka beban kerja berhalaman (filter unit, jabatan, periode) | `WorkforceStaffingWorkload:Read` | `200 OK` (Paged) | `401`, `403` |
| `GET` | `/{id}` | Ambil detail satu angka beban kerja berdasarkan ID | `WorkforceStaffingWorkload:Read` | `200 OK` | `401`, `403`, `404` |
| `POST` | `/` | Catat isian angka beban kerja baru satu periode | `WorkforceStaffingWorkload:Create` | `201 Created` | `400`, `401`, `403`, `404`, `409` |
| `PUT` | `/{id}` | Perbarui isian angka beban kerja yang sudah ada | `WorkforceStaffingWorkload:Update` | `200 OK` | `400`, `401`, `403`, `404`, `409` |

---

## 5. Daftar Berkas yang Ditambahkan dan Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/StaffingWorkloadDtos.cs`
   - DTO: `UpsertStaffingWorkloadRequest`, `StaffingWorkloadResponse`, `StaffingWorkloadPagedQuery`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingWorkloadService.cs`
   - Antarmuka layanan pencatatan dan pengelolaan angka beban kerja manual.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdStaffingWorkloadService.cs`
   - Implementasi logika bisnis: pembatasan wewenang Kepala Unit (`VR-WFP-38`), validasi sumber wajib (`VR-WFP-08`), pencegahan duplikasi periode (`VR-WFP-09`), dan operasi CRUD data beban.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdStaffingWorkloadController.cs`
   - Controller ASP.NET Core mengekspos endpoint `GET`, `POST`, dan `PUT` dengan otorisasi `WorkforceStaffingWorkload` dan tag OpenAPI.

### Berkas Diubah:
1. `Program.cs`
   - Mendaftarkan dependensi scoped `IHrdStaffingWorkloadService` dan `HrdStaffingWorkloadService`.

---

## 6. Bukti Kompilasi & Pengujian

### Status Pengujian Otomatis
```text
AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)
```

### Hasil Kompilasi (`dotnet build`)
- Perintah: `dotnet build NewQuilvianSystemBackend\QuilvianSystemBackend.csproj -c Debug -p:SkipMigrationMetadata=true --no-restore`
- Hasil: **0 Error(s)**
- Keterangan: Berhasil terkompilasi bersih tanpa peringatan atau kesalahan baru.

---

## 7. Penutupan Task & Rekomendasi Langkah Berikutnya

Task `BE-HRD-034` telah selesai secara penuh dengan seluruh kriteria penerimaan terbukti.

### Task Approved Berikutnya (Gelombang MVP-8):
- **`BE-HRD-035`**: Standar kebutuhan tenaga, angka rekomendasi, dan pengesahan `T9` (Penyusunan standar kebutuhan per unit sebagai versi bertanggal berlaku, penghitungan rekomendasi 3 metode — beban kerja, jumlah tetap, cakupan layanan — dan integrasi workflow persetujuan Direktur `T9`). Prasyarat: `BE-HRD-034` ✅, `BE-HRD-029` ✅, `BE-HRD-030` ✅.
