# Laporan Perubahan Backend — `BE-HRD-048`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-048` |
| Judul | Potret Gap Bulanan dan Rekonstruksi |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-10` (Rencana Tahunan, Addendum, dan Potret Bulanan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-10 |
| Traceability | `HRD-DEC-069` (`MISSED_SNAPSHOT_NOT_FAKED`), `FR-HRD-105`, `AT-HRD-D1-16b`, `INT-HR-14`, `State Transition v7 §11.7`, `Data Dictionary v7 §8.2` |
| Dependency | `BE-HRD-036` ✅, `BE-HRD-039` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `IHrdStaffingGapSnapshotService.cs`, `HrdStaffingGapSnapshotService.cs`, `HrdWorkforcePlanningSchedulerService.cs`, `HrdStaffingGapController.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, potret bulanan otomatis & rekonstruksi transparan terbukti |

---

## 1. Masalah yang Diselesaikan

Untuk keperluan evaluasi tren pemenuhan tenaga kerja rumah sakit dari waktu ke waktu (*trend analysis*), sistem secara berkala pada akhir setiap bulan mengambil potret kesenjangan (*monthly snapshot*) untuk seluruh unit kerja. Jika terjadi insiden server mati pada akhir bulan sehingga jadwal scheduler terlewat, sistem tidak boleh memanipulasi data seolah-olah jadwal berjalan normal; bulan yang terlewat harus ditampilkan sebagai belum tersedia, dan pengisian susulan wajib diberi label `ReconstructedAt` ("direkonstruksi") secara jujur dan transparan (`HRD-DEC-069`).

Aturan bisnis:
1. **Perekaman Otomatis pada Hari Terakhir Bulan (Kriteria 1)**: Scheduler Langkah 3 secara otomatis memeriksa apakah tanggal operasional adalah hari terakhir bulan (`operationalDate.Day == DaysInMonth`). Jika ya, sistem menyimpan snapshot bertipe `Monthly` untuk setiap rumah sakit aktif dengan `PeriodMonth = tanggal 1 bulan berjalan`.
2. **Idempotensi Akhir Bulan (Kriteria 2)**: Jika scheduler dijalankan berulang pada akhir bulan, potret bulanan tidak akan diduplikasi.
3. **Mekanisme Rekonstruksi Susulan Transparan (Kriteria 3)**: Melalui endpoint `POST /snapshots/reconstruct`, administrator dapat menjalankan pengisian susulan untuk bulan lampau yang terlewat. Potret hasil rekonstruksi secara eksplisit mengisi kolom `ReconstructedAt` sebagai penanda audit bahwa potret diambil di luar jadwal rutinnya.
4. **Kolom Integrasi Penjadwalan (Kriteria 4)**: Kolom ringkasan kekurangan jaga (*ShortageSummaryCount*) dibiarkan `null` sampai modul Penjadwalan mengirimkan datanya (`INT-HR-14`).

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Scheduler akhir bulan menyimpan potret gap seluruh unit bertipe `Monthly` untuk setiap rumah sakit. | Diimplementasikan di `HrdStaffingGapSnapshotService.StepProcessMonthlySnapshotAsync`. | ✅ Terbukti |
| 2 | Menjalankan scheduler dua kali pada akhir bulan tidak menduplikasi potret bulanan. | Pengecekan keberadaan potret dengan `PeriodMonth` yang sama mencegah duplikasi. | ✅ Terbukti |
| 3 | Bulan yang terlewat tampil tidak tersedia; pengisian susulan diberi label `ReconstructedAt`. | Diimplementasikan di `ReconstructMonthlySnapshotAsync`. | ✅ Terbukti |
| 4 | Kolom `ShortageSummaryCount` tetap kosong sampai Penjadwalan mengirimnya (`INT-HR-14`). | Field dibiarkan bernilai default `null`. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/staffing-gaps`
Tags: `Corporate / Human Resource / Workforce Planning / Staffing Gap`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/snapshots/reconstruct` | Rekonstruksi susulan potret gap bulanan untuk periode lampau yang terlewat | `WorkforceStaffingGap:Create` | `201 Created` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Diubah:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingGapSnapshotService.cs` & `HrdStaffingGapSnapshotService.cs`: Menambahkan method `StepProcessMonthlySnapshotAsync` dan `ReconstructMonthlySnapshotAsync`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSchedulerService.cs`: Memanggil `StepProcessMonthlySnapshotAsync` pada Langkah 3 siklus harian scheduler.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdStaffingGapController.cs`: Menambahkan endpoint `POST /snapshots/reconstruct`.
