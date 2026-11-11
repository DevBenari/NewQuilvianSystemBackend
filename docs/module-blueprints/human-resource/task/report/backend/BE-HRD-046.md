# Laporan Perubahan Backend — `BE-HRD-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-046` |
| Judul | Rencana Tahunan dan Addendum: Draf, Hitung Ulang, Pengajuan T10, Koreksi Finance |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-10` (Rencana Tahunan, Addendum, dan Potret Bulanan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-10 |
| Traceability | `HRD-DEC-062`, `HRD-DEC-065`, `HRD-DEC-068` (`APPROVED_PLAN_LOCKED`), `FR-HRD-106` s.d. `FR-HRD-110`, `VR-WFP-15` s.d. `VR-WFP-20`, `AT-HRD-D1-15`, `AT-HRD-D1-21`, `UAT-35`, `UAT-36`, `API v7 §13 grup Manpower Plan` |
| Dependency | `BE-HRD-038` ✅, `BE-HRD-039` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `ManpowerPlanDtos.cs`, `IHrdManpowerPlanService.cs`, `HrdManpowerPlanService.cs`, `HrdManpowerPlanController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, 10 endpoint aktif & seluruh 8 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Penyusunan Rencana Kebutuhan Tenaga Kerja Tahunan (*Annual Manpower Plan*) mengompilasi usulan formasi dari seluruh unit kerja rumah sakit untuk satu tahun anggaran ke depan. Di pertengahan tahun berjalan, perubahan target formasi hanya dapat dilakukan melalui mekanisme Addendum dari rencana yang telah disetujui (`HRD-DEC-068`).

Aturan bisnis:
1. **Pencegahan Rencana Ganda (`VR-WFP-15`, Kriteria 1)**: Rumah sakit hanya boleh memiliki satu rencana tahunan aktif untuk tahun anggaran yang sama. Upaya membuat rencana tahunan kedua ditolak dengan `409 Conflict`.
2. **Keterikatan Addendum pada Rencana Disetujui (`VR-WFP-16`, Kriteria 2)**: Addendum hanya dapat diterbitkan dari rencana induk yang telah berstatus `Approved`, dan wajib memuat sekurangnya satu baris perubahan.
3. **Penguncian Rencana Disetujui (`VR-WFP-17`, Kriteria 3)**: Rencana yang sudah `Approved` terkunci mutlak dan tidak dapat diedit secara langsung (ditolak `409 Conflict` dengan instruksi membuat addendum).
4. **Hitung Ulang Otomatis Formasi, Rekomendasi, dan Biaya (`POST /{id}/recalculate`, Kriteria 4)**: Sistem secara otomatis menarik angka formasi saat ini (`CurrentFormationQuantity`), angka rekomendasi dari mesin kalkulasi standar tenaga (`BE-HRD-035`), teks rincian formula, serta estimasi biaya dari gaji tengah kelas jabatan (`BE-HRD-038`).
5. **Validasi Pengajuan T10 (`VR-WFP-18`, `VR-WFP-19`, Kriteria 5)**: Pengajuan menolak jika terdapat baris tanpa kelas gaji (`VR-WFP-18`) atau jika usulan berbeda dari rekomendasi tanpa menyertakan alasan penyimpangan (`VR-WFP-19`). Pengajuan membekukan potret gap pengajuan (`BE-HRD-039`) dan mendaftarkan dokumen ke alur workflow Direktur Utama `T10`.
6. **Koreksi Anggaran oleh Finance (`VR-WFP-20`, Kriteria 6)**: Peninjau Keuangan dapat mengoreksi total anggaran rencana pada langkah persetujuannya (`FinanceAdjustedTotal`), dengan angka estimasi awal tetap tersimpan.
7. **Penelusuran Potret Gap Dokumen (Kriteria 7)**: Endpoint `GET /{id}/gap-snapshots` menampilkan potret kesenjangan yang menempel pada dokumen.
8. **Penyamaran Biaya bagi Non-Finance (Kriteria 8)**: Kolom biaya disamarkan menjadi `null` dan `CostVisible = false` bagi pengguna yang tidak memiliki wewenang akses biaya.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Rencana tahunan kedua untuk rumah sakit dan tahun yang sama yang belum ditolak/dibatalkan → `409` `VR-WFP-15`. | Diimplementasikan di `HrdManpowerPlanService.CreateAnnualPlanAsync`. | ✅ Terbukti |
| 2 | Addendum hanya dari rencana `Approved` → selain itu `422` `VR-WFP-16`; addendum memuat sekurangnya satu baris. | Diimplementasikan di `CreatePlanAddendumAsync`. | ✅ Terbukti |
| 3 | Mengubah rencana `Approved` → `409` `VR-WFP-17` dengan arahan membuat addendum (`UAT-36`). | Diimplementasikan di `UpdateDraftAsync`. | ✅ Terbukti |
| 4 | `POST /{id}/recalculate` mengisi formasi saat ini, rekomendasi beserta rincian teksnya, dan biaya. | Diimplementasikan di `RecalculateAsync` dan `InternalRecalculateAsync`. | ✅ Terbukti |
| 5 | Pengajuan menolak `422` bila baris tanpa grade (`VR-WFP-18`) atau usulan berbeda dari rekomendasi tanpa alasan (`VR-WFP-19`); pengajuan menyimpan potret gap `Submission` dan memulai `T10`. | Diimplementasikan di `SubmitAsync`. | ✅ Terbukti |
| 6 | Koreksi Finance hanya oleh penyetuju Finance `T10` → `VR-WFP-20`; angka awal dan koreksi tersimpan berdampingan. | Diimplementasikan di `FinanceAdjustmentAsync`. | ✅ Terbukti |
| 7 | `GET /{id}/gap-snapshots` mengembalikan potret yang menempel pada dokumen. | Diimplementasikan di `GetGapSnapshotsAsync`. | ✅ Terbukti |
| 8 | Kolom biaya mengikuti aturan penyamaran `BE-HRD-038`. | Logika penyamaran `HasCostReadPermission` terintegrasi penuh. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/manpower-plans`
Tags: `Corporate / Human Resource / Workforce Planning / Manpower Plan`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/` | Ambil daftar rencana kebutuhan tenaga tahunan dan addendum berhalaman | `WorkforceManpowerPlan:Read` | `200 OK` (Paged) |
| `GET` | `/filters/metadata` | Ambil opsi enumerasi status dan jenis rencana | `WorkforceManpowerPlan:Read` | `200 OK` |
| `GET` | `/{id}` | Ambil detail rencana kebutuhan tenaga beserta baris per unit dan jabatan | `WorkforceManpowerPlan:Read` | `200 OK` |
| `POST` | `/` | Buat draf baru rencana formasi tahunan (Annual Plan) | `WorkforceManpowerPlan:Create` | `201 Created` |
| `POST` | `/addendums` | Buat draf addendum perubahan formasi dari rencana Approved | `WorkforceManpowerPlan:Create` | `201 Created` |
| `PUT` | `/{id}` | Perbarui draf rencana atau addendum | `WorkforceManpowerPlan:Update` | `200 OK` |
| `POST` | `/{id}/recalculate` | Hitung ulang formasi, rekomendasi, rincian formula, dan estimasi biaya | `WorkforceManpowerPlan:Update` | `200 OK` |
| `POST` | `/{id}/submit` | Ajukan rencana kebutuhan tenaga ke alur persetujuan workflow Direktur Utama T10 | `WorkforceManpowerPlan:Update` | `200 OK` |
| `POST` | `/{id}/finance-adjustment` | Koreksi total estimasi biaya oleh peninjau Keuangan / Finance pada langkah T10 | `WorkforceManpowerPlan:Update` | `200 OK` |
| `GET` | `/{id}/gap-snapshots` | Ambil daftar potret kesenjangan yang menempel pada dokumen rencana ini | `WorkforceManpowerPlan:Read` | `200 OK` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/ManpowerPlanDtos.cs`: DTO rencana tahunan, addendum, paginasi, rincian baris, dan koreksi biaya Finance.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdManpowerPlanService.cs`: Antarmuka layanan rencana formasi tahunan dan addendum.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdManpowerPlanService.cs`: Implementasi layanan kompilasi rencana, validasi deviasi, hitung ulang, dan inisiasi workflow T10.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdManpowerPlanController.cs`: Controller ASP.NET Core 10 endpoint.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IHrdManpowerPlanService` dan `HrdManpowerPlanService`.
