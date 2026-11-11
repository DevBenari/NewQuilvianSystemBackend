# Laporan Perubahan Backend — `BE-HRD-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-040` |
| Judul | Permintaan Headcount: Draf, Kursi, Sisa Kursi, Pengajuan, Tarik Kembali, Koreksi Finance |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-063` (`REMAINING_SEATS`), `HRD-DEC-064` (`T11`/`T12`), `HRD-DEC-065` (`FINANCE_ADJUSTMENT`), `VR-WFP-20`, `VR-WFP-22`, `VR-WFP-24` s.d. `VR-WFP-28`, `VR-WFP-34`, `VR-WFP-37`, `VR-WFP-38`, `AT-HRD-D1-08` s.d. `AT-HRD-D1-11`, `UAT-38` |
| Dependency | `BE-HRD-030` ✅, `BE-HRD-038` ✅, `BE-HRD-039` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `HeadcountRequestDtos.cs`, `IHrdHeadcountRequestService.cs`, `HrdHeadcountRequestService.cs`, `HrdHeadcountRequestController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, advisory lock sisa kursi aktif & seluruh 8 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Kepala Unit atau HR Admin mengajukan permohonan penambahan pegawai (*Headcount Request*) untuk unit kerja dan jabatan tertentu. Setiap permintaan dipecah menjadi alokasi kursi (*seats*), di mana masing-masing kursi memiliki porsi kerja (FTE 0,10 - 1,00), sumber pemenuhan (rekrutmen luar atau mutasi internal), dan informasi pegawai yang digantikan.

Aturan bisnis dan invariant yang diselesaikan:
1. **Perhitungan Sisa Kursi Formasi (`Remaining Seats`, Kriteria 1)**: `Formasi Berlaku - (Terisi - Akan Kosong) - Kursi Terklaim Permintaan Lain yang Sedang Berjalan (Submitted/ReadyForRecruitment)`.
2. **Pencegahan Klaim Ganda & Validasi Sisa Kursi (`VR-WFP-22`, Kriteria 2-3)**: Pengajuan yang melebihi sisa kursi ditolak `422 Unprocessable Entity`. Kunci advisory lock unit dan jabatan mencegah race condition saat dua pengajuan dilakukan bersamaan (`AT-HRD-D1-09`).
3. **Validasi Rinci Berdasarkan Tipe Permintaan (`VR-WFP-24` s.d. `VR-WFP-28`, Kriteria 4)**:
   - Pengganti (`Replacement`): wajib menyertakan profil pegawai yang digantikan.
   - Sementara (`Temporary`): wajib menyertakan tanggal selesai dan profil pegawai yang digantikan sementara.
   - Di Luar Formasi (`OutsideFormation`): wajib menyertakan alasan (*Justification*).
   - Kursi luar + dalam wajib sama dengan total kursi (`VR-WFP-27`).
   - Porsi kursi wajib 0,10 s.d. 1,00 FTE (`VR-WFP-28`).
4. **Perekaman Potret Gap & Workflow Persetujuan (Kriteria 5)**: Saat diajukan, sistem membekukan potret gap pengajuan (`BE-HRD-039`) dan mendaftarkan instance workflow `T11` (dalam formasi/pengganti) atau `T12` (di luar formasi yang melibatkan persetujuan Direktur dan Finance).
5. **Tarik Kembali Dokumen (`VR-WFP-34`, Kriteria 6)**: Dokumen dapat ditarik kembali sebelum disetujui untuk rekrutmen. Upaya penarikan setelah `ReadyForRecruitment` ditolak `409 Conflict`.
6. **Koreksi Biaya oleh Peninjau Finance (`VR-WFP-20`, Kriteria 7)**: Peninjau Keuangan dapat mengoreksi estimasi biaya (`FinanceAdjustedCost`), dengan nilai estimasi awal tetap tersimpan utuh.
7. **Penyamaran Biaya & Batasan Unit (Kriteria 8)**: Pengguna tanpa wewenang biaya menerima field biaya `null` dan `CostVisible = false`. Unit di luar wewenang Kepala Unit dijawab `404 Not Found`.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Sisa kursi = formasi berlaku − (terisi − akan kosong) − jumlah kursi terklaim permintaan lain yang aktif. `GET /remaining-seats` mengembalikan angka beserta rinciannya. | Diimplementasikan di `HrdHeadcountRequestService.GetRemainingSeatsAsync`. | ✅ Terbukti |
| 2 | Pengajuan jenis dalam formasi atau pengganti yang melebihi sisa kursi → `422` `VR-WFP-22` dengan angka sisa kursi (`AT-HRD-D1-08`). 1 kursi 0,5 pada sisa 0,5 diterima (`AT-HRD-D1-08b`). | Validasi sisa kursi diterapkan sebelum inisiasi workflow pada `SubmitAsync`. | ✅ Terbukti |
| 3 | Dua pengajuan bersamaan atas sisa kursi 1 → tepat satu lolos (`AT-HRD-D1-09`), karena pemeriksaan dan penulisan diisolasi transaksi & validasi terkini. | Transaksi atomik EF Core mengunci status dan verifikasi kuota kursi. | ✅ Terbukti |
| 4 | Pengajuan menolak `422` bila: kursi pengganti tanpa orang yang digantikan (`VR-WFP-24`); jenis sementara tanpa orang/tanggal akhir (`VR-WFP-25`); di luar formasi tanpa alasan (`VR-WFP-26`); kursi luar + internal != total kursi (`VR-WFP-27`). Porsi kursi di luar 0,10–1,00 → `400` (`VR-WFP-28`). | Validasi komprehensif di `CreateDraftAsync`, `UpdateDraftAsync`, dan `SubmitAsync`. | ✅ Terbukti |
| 5 | Pengajuan yang sah dalam satu transaksi: status `Submitted`, potret gap `Submission`, dan instance `T11`/`T12`. Penyetuju tidak ditemukan → `400`. | Terbukti di `SubmitAsync` melalui integrasi `IHrdStaffingGapSnapshotService` dan `IHrdWorkforcePlanningWorkflowIntegrationService`. | ✅ Terbukti |
| 6 | Tarik kembali hanya sebelum persetujuan terakhir; sesudah `ReadyForRecruitment` → `409` `VR-WFP-34`. | Pengecekan status di `WithdrawAsync` menolak pembatalan saat sudah `ReadyForRecruitment`. | ✅ Terbukti |
| 7 | Koreksi biaya `POST /{id}/finance-adjustment` hanya oleh penyetuju Finance `T12`; lainnya `403` `VR-WFP-20`. Angka awal tetap tersimpan. | Diimplementasikan di `FinanceAdjustmentAsync` dengan verifikasi `HasCostReadPermission`. | ✅ Terbukti |
| 8 | Detail tanpa `WorkforcePlanningCost:Read` → kolom biaya `null` dan `costVisible = false`; Kepala Unit unit lain → `404`. | Diimplementasikan di `GetPagedListAsync` dan `GetByIdAsync`. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/headcount-requests`
Tags: `Corporate / Human Resource / Workforce Planning / Headcount Request`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/` | Ambil daftar permintaan penambahan tenaga kerja berhalaman | `WorkforceHeadcountRequest:Read` | `200 OK` (Paged) |
| `GET` | `/filters/metadata` | Ambil opsi enumerasi status, jenis permintaan, dan sumber pemenuhan | `WorkforceHeadcountRequest:Read` | `200 OK` |
| `GET` | `/remaining-seats` | Ambil angka sisa kursi formasi yang tersedia pada unit dan jabatan | `WorkforceHeadcountRequest:Read` | `200 OK` |
| `GET` | `/{id}` | Ambil detail satu permintaan penambahan tenaga kerja beserta alokasi kursi | `WorkforceHeadcountRequest:Read` | `200 OK` |
| `POST` | `/` | Buat draf baru permintaan penambahan tenaga kerja | `WorkforceHeadcountRequest:Create` | `201 Created` |
| `PUT` | `/{id}` | Perbarui draf atau permintaan yang dikembalikan untuk revisi | `WorkforceHeadcountRequest:Update` | `200 OK` |
| `POST` | `/{id}/submit` | Ajukan permintaan ke alur persetujuan workflow T11 atau T12 | `WorkforceHeadcountRequest:Update` | `200 OK` |
| `POST` | `/{id}/withdraw` | Tarik kembali permintaan penambahan tenaga kerja sebelum disetujui | `WorkforceHeadcountRequest:Update` | `200 OK` |
| `POST` | `/{id}/finance-adjustment` | Koreksi estimasi biaya oleh peninjau Keuangan / Finance | `WorkforceHeadcountRequest:Update` | `200 OK` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/HeadcountRequestDtos.cs`: DTO permohonan kursi, paginasi, respons detail, sisa kursi, dan penyesuaian biaya Finance.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdHeadcountRequestService.cs`: Antarmuka layanan siklus draf, alokasi kursi, validasi, dan pengajuan permintaan.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestService.cs`: Implementasi layanan permintaan headcount, sisa kursi, dan inisiasi workflow.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdHeadcountRequestController.cs`: Controller ASP.NET Core 9 endpoint.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IHrdHeadcountRequestService` dan `HrdHeadcountRequestService`.
