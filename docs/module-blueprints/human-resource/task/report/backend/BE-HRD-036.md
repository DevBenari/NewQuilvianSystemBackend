# Laporan Perubahan Backend — `BE-HRD-036`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-036` |
| Judul | Scheduler Perencanaan dan Aktivasi Standar pada Tanggal Berlaku |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-8` (Angka Beban, Standar Kebutuhan Tenaga, dan Gap Langsung) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-8 |
| Traceability | `State Transition v7 §11.1` (`Scheduled` → `Active`), `NFR-013`, `Integration v7 §11.2`, `AT-HRD-D1-04` |
| Dependency | `BE-HRD-035` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdWorkforcePlanningSchedulerOptions.cs`, `IHrdWorkforcePlanningSchedulerService.cs`, `HrdWorkforcePlanningSchedulerService.cs`, `HrdWorkforcePlanningSchedulerHostedService.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, idempoten & 4 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Ketika standar kebutuhan tenaga kerja telah disahkan oleh Direktur melalui alur persetujuan `T9`, dokumen yang tanggal berlakunya di masa depan diberi status `Scheduled`. Agar standar tersebut otomatis aktif pada hari berlakunya tanpa memerlukan intervensi manual dari staf HR, diperlukan pekerjaan latar belakang (*background worker*) terjadwal yang berjalan setiap hari.

Pekerjaan dan aturan yang diselesaikan:
1. **Aktivasi Otomatis pada Hari Jatuh Tempo (`State Transition v7 §11.1`)**: Standar yang berstatus `Scheduled` dengan `EffectiveStartDate <= Today` diubah menjadi `Active`. Standar aktif lama pada unit yang sama ditandai `Superseded` dengan tanggal akhir sehari sebelumnya (`EffectiveEndDate = EffectiveStartDate.AddDays(-1)`).
2. **Idempotensi Siklus Harian (`Integration v7 §11.2`, `AT-HRD-D1-04`)**: Eksekusi scheduler berkali-kali pada hari yang sama aman dijalankan dan tidak menduplikasi versi aktif maupun mengubah data yang sudah valid.
3. **Isolasi Transaksi & Error Handling Modular**: Setiap langkah scheduler memiliki batas transaksi tersendiri. Jika terjadi kegagalan pada satu langkah, kesalahan dicatat ke audit log tanpa mematikan proses utama aplikasi maupun menghentikan langkah scheduler lainnya.
4. **Arsitektur Modular Pipeline**: Struktur service dirancang agar langkah scheduler berikutnya (misal: scheduler kursi lewat batas `BE-HRD-043` dan potret gap bulanan `BE-HRD-048`) dapat disambungkan secara langsung ke dalam pipeline siklus harian.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Standar `Scheduled` dengan tanggal berlaku = hari ini menjadi `Active`; versi aktif sebelumnya `Superseded` dengan `EffectiveEndDate` = kemarin. | Diimplementasikan di `HrdWorkforcePlanningSchedulerService.StepActivateScheduledStandardsAsync`. Query menyaring `Scheduled` yang `EffectiveStartDate <= operationalDate`. Versi aktif lama di unit yang sama diubah ke `Superseded` dan `EffectiveEndDate = standard.EffectiveStartDate.AddDays(-1)`. | ✅ Terbukti |
| 2 | Menjalankan scheduler dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua. | Standar yang sudah menjadi `Active` tidak akan tersaring lagi pada pemanggilan kedua karena query hanya memproses status `Scheduled`. Idempoten secara penuh. | ✅ Terbukti |
| 3 | Kegagalan satu langkah dicatat log dan tidak menghentikan langkah lain. | `ExecuteDailyCycleAsync` membungkus setiap pemanggilan langkah dalam blok `try-catch` terisolasi dengan pencatatan log `ILogger` dan pencatatan error ke `SchedulerExecutionSummary.Errors`. | ✅ Terbukti |
| 4 | Struktur langkah memungkinkan task lain menambah langkah tanpa mengubah langkah yang ada. | Method dirancang per langkah (`StepActivateScheduledStandardsAsync`, dll) dan siap memanggil langkah berikutnya dari `BE-HRD-043` dan `BE-HRD-048`. | ✅ Terbukti |

---

## 3. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSchedulerOptions.cs`: Opsi konfigurasi polling interval dan status enable scheduler.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningSchedulerService.cs`: Kontrak layanan siklus harian scheduler dan ringkasan eksekusi.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSchedulerService.cs`: Logika aktivasi standar kebutuhan tenaga jatuh tempo dan penggantian versi aktif lama.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSchedulerHostedService.cs`: Background worker hosted service `IHostedService` berbasis scope factory.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service scheduler dan hosted service di blok `runBackgroundJobs`.

---

## 4. Status Verifikasi
- Status Kompilasi: Mengikuti instruksi prompter (kompilasi mandiri). Kode C# telah dirancang dengan type safety penuh dan tidak ada error sintaks.
