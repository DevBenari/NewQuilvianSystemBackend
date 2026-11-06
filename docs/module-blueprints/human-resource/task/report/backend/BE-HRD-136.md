# Laporan Penyelesaian Task Backend: BE-HRD-136

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-136` |
| **Nama Task** | Engine penegakan kuota PP 35/2021 & deteksi eskalasi darurat klinis (`OvertimeQuotaEnforcementGuard`) |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-117`, `02-backend-architecture.md` 22.8 (`OT-B4`, `OT-W1`), `contracts/validation-matrix.md` (`VR-OT-04`, `VR-OT-12`) |

---

## 2. Deskripsi & Tujuan Bisnis

Berdasarkan Peraturan Pemerintah (PP) No. 35 Tahun 2021 Pasal 26 dan UU Kesehatan No. 17 Tahun 2023, waktu kerja lembur dibatasi maksimal 4 jam dalam 1 hari dan 18 jam dalam 1 minggu kerja. Pembatasan ini sangat vital di lingkungan rumah sakit untuk mencegah risiko kelelahan nakes (*fatigue risk*) yang dapat membahayakan keselamatan pasien (*patient safety*).

Namun demikian, rumah sakit menghadapi situasi khusus penyelamatan nyawa (*life-saving emergency*), seperti operasi darurat cito yang komplikasi atau bencana massal. Oleh karena itu, sistem menyediakan engine penjaga (*guard engine*) cerdas:
1. **Penolakan Keras Lembur Biasa (`VR-OT-04`, `OT-B4`)**: Pengajuan lembur terencana reguler yang melanggar batas 4 jam/hari atau 18 jam/minggu ditolak secara mutlak dengan HTTP 400 Bad Request.
2. **Pengecualian Darurat Penyelamatan Nyawa (`VR-OT-12`, `OT-W1`)**: Klaim darurat medis pasca-dinas (*Emergency Cito*) yang melebihi batas kuota tetap diizinkan masuk sistem, tetapi otomatis ditandai `IsMedicalDirectorEscalation = true` dan dialihkan ke alur otorisasi khusus Direktur Pelayanan Medik.
3. **Akumulasi Mingguan Kalender Kerja Indonesia**: Perhitungan akumulasi jam lembur mingguan didasarkan pada siklus baku Senin s.d. Minggu pada minggu berjalan.

---

## 3. Komponen yang Dikerjakan

1. **Interface & Model Hasil Evaluasi (`IOvertimeQuotaEnforcementGuard.cs`)**:
   - `OvertimeQuotaEvaluationResult`: Objek hasil evaluasi yang memuat flag `IsAllowed`, `IsMedicalDirectorEscalation`, `AccumulatedDailyHours`, `AccumulatedWeeklyHours`, dan pesan penolakan `DenialReason`.
   - `EvaluateQuotaAsync`: Metode evaluasi kuota yang menerima identitas staf, tanggal lembur, estimasi durasi, dan indikator darurat cito.

2. **Implementasi Guard (`OvertimeQuotaEnforcementGuard.cs`)**:
   - Mengambil parameter kuota dinamis via `IOvertimeSettingService` (`DailyMaxHoursRegular = 4`, `WeeklyMaxHoursRegular = 18`).
   - Menghitung rentang Senin hingga Minggu berbasis kalender kerja lokal.
   - Mengagregasi total jam dinas lembur aktif staf pada tanggal terkait dan sepanjang minggu berjalan.
   - Mengarahkan keputusan:
     - Jika kuota aman: `IsAllowed = true`, `IsMedicalDirectorEscalation = false`.
     - Jika kuota terlampaui & kategori `EmergencyCito`: `IsAllowed = true`, `IsMedicalDirectorEscalation = true`.
     - Jika kuota terlampaui & kategori `Planned`: `IsAllowed = false`, `DenialReason = "Pengajuan melebihi kuota maksimal lembur (4 jam/hari atau 18 jam/minggu) sesuai regulasi PP 35/2021."`.

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario 1: Penolakan Lembur Reguler Melebihi Batas
* **Kasus**: Staf rekam medis telah lembur 16 jam dari hari Senin hingga Kamis. Pada hari Jumat, ia mengajukan SPKL terencana selama 3 jam.
* **Hasil Evaluasi**: Total akumulasi mingguan menjadi 19 jam (> 18 jam).
* **Keputusan Sistem**: Sistem menolak submit dengan HTTP 400 Bad Request: *"Pengajuan melebihi kuota maksimal lembur (4 jam/hari atau 18 jam/minggu) sesuai regulasi PP 35/2021."*

### Skenario 2: Eskalasi Darurat Cito ke Direktur Medis
* **Kasus**: Dokter spesialis bedah syaraf telah bekerja intensif sepanjang minggu (akumulasi 17 jam). Terjadi kecelakaan lalu lintas parah yang memerlukan operasi cito trepanasi darurat selama 4 jam (total 21 jam).
* **Hasil Evaluasi**: Kategori klaim adalah `EmergencyCito`.
* **Keputusan Sistem**: Sistem meloloskan klaim ke database dengan penanda `IsMedicalDirectorEscalation = true`. Klaim masuk ke kotak masuk khusus Direktur Pelayanan Medik untuk asesmen dispensasi darurat medis.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| Pengajuan lembur biasa yang melebihi 4 jam/hari atau 18 jam/minggu ditolak keras | ✅ Lolos | Diimplementasikan pada blok pengecekan `exceedsQuota && !isEmergencyCito` dengan pesan baku `VR-OT-04`. |
| Klaim darurat cito medis melampaui kuota dialihkan ke Direktur Medis | ✅ Lolos | Diimplementasikan pada `exceedsQuota && isEmergencyCito` dengan flag `IsMedicalDirectorEscalation = true`. |
| Perhitungan akumulasi mingguan didasarkan pada rentang Senin s.d. Minggu | ✅ Lolos | Menggunakan formula kalender Senin-Minggu berbasis `DayOfWeek`. |
