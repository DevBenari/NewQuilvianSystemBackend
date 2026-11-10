# Laporan Penyelesaian Task Backend: BE-HRD-133

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-133` |
| **Nama Task** | DTO & validasi FluentValidation SPKL pra-dinas & klaim cito pasca-dinas |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-116`, `HRD-DEC-118`, `02-backend-architecture.md` 22.8 (`OT-B1`, `OT-B2`, `OT-B3`, `OT-B6`), `contracts/validation-matrix.md` (`VR-OT-01`, `VR-OT-02`, `VR-OT-05`, `VR-OT-06`, `VR-OT-20`, `VR-OT-22`) |

---

## 2. Deskripsi & Tujuan Bisnis

Dalam operasional rumah sakit, validasi input data lembur merupakan garis pertahanan pertama untuk mencegah sengketa hukum ketenagakerjaan, memastikan keselamatan pasien, dan menjaga kepatuhan Good Corporate Governance.

Task ini menyediakan DTO command lengkap beserta kelas validator ketat (*FluentValidation*) yang menegakkan aturan bisnis rumah sakit sejak tingkat permohonan, mencakup:
1. **Validasi Pra-Dinas Lembur Terencana (`VR-OT-01`, `OT-B1`)**: Memastikan SPKL terencana minimal diajukan sebelum jam dinas dimulai (`PlanStartTime > DateTime.UtcNow`).
2. **Kewajiban Bukti Medis Klaim Cito (`VR-OT-02`, `OT-B3`)**: Memastikan klaim darurat pasca-dinas wajib mencantumkan Nomor Rekam Medis pasien / Nomor Insiden Darurat dan uraian tindakan klinis.
3. **Pengecualian Pejabat Manajerial (`VR-OT-05`, `OT-B6`)**: Menolak pejabat struktural (level Manajer ke atas) memilih kompensasi finansial tunai (*Overtime Exempt*).
4. **Tenggat Pelaporan Cito H+1 (`VR-OT-06`, `OT-B2`)**: Menolak pelaporan darurat medis yang diserahkan melampaui batas toleransi H+1 hari kerja.
5. **Integritas SPKL Tim Medis Ruangan (`VR-OT-20`, `HRD-ASM-26`)**: Menolak pengajuan lembur kelompok ruangan yang tidak memiliki satupun staf nakes terdaftar.
6. **Batas Kalender Maksimal 24 Jam (`VR-OT-22`)**: Menolak permohonan lembur dengan rentang durasi di luar akal sehat (> 24 jam dalam satu hari).

---

## 3. Komponen yang Dikerjakan

1. **DTOs (`OvertimeEssCommandsAndValidators.cs`)**:
   - `CreateOvertimePlanCommand`: Command pembuatan SPKL pra-dinas perorangan atau tim kelompok.
   - `OvertimePlanStaffItemDto`: DTO rincian staf nakes pelaksana lembur dalam satu tim.
   - `CancelOvertimePlanCommand`: Command pembatalan rencana SPKL pra-dinas dengan alasan operasional.
   - `SubmitOvertimeRealizationCommand`: Command pelaporan jam lembur aktual atau klaim cito pasca-dinas.
   - `OvertimePlanResultDto`, `OvertimePlanDetailDto`, `OvertimePlanSummaryDto`: DTO respons hasil pemrosesan.

2. **Validator FluentValidation**:
   - `CreateOvertimePlanCommandValidator`: Menguji aturan pra-dinas `VR-OT-01`, batas 24 jam `VR-OT-22`, keabsahan anggota tim `VR-OT-20`, dan pembatasan kompensasi finansial manajer `VR-OT-05`.
   - `SubmitOvertimeRealizationCommandValidator`: Menguji keabsahan klaim cito `VR-OT-02`, batas waktu H+1 kerja `VR-OT-06`, dan pembatasan manajerial `VR-OT-05`.
   - `CancelOvertimePlanCommandValidator`: Memvalidasi alasan pembatalan tidak boleh kosong.

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario: Pengajuan SPKL Tim Bedah Ruang OK
* **Kondisi**: Kepala Ruangan Instalasi Bedah Sentral merencanakan operasi elektif tambahan untuk 3 pasien katarak pada hari Sabtu mendatang pukul 08:00–12:00.
* **Payload**: `CreateOvertimePlanCommand` dikirim dengan `IsGroupOvertime = true` dan `StaffList` berisi 4 orang (1 perawat instrumen, 1 perawat sirkuler, 1 penata anestesi, 1 petugas CSSD).
* **Validasi**:
  - `PlanStartTime` (Sabtu 08:00) > `DateTime.UtcNow` -> Lolos `VR-OT-01`.
  - `StaffList` tidak kosong -> Lolos `VR-OT-20`.
  - Seluruh staf bertaraf pelaksana (`IsOvertimeExempt = false`) -> Lolos `VR-OT-05`.
  - SPKL berhasil diterbitkan dan masuk ke status `WaitingApproval`.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| Pengajuan kategori Planned dengan jam mulai di masa lampau ditolak | ✅ Lolos | Diuji oleh rule `RuleFor(x => x.PlanStartTime).GreaterThan(DateTime.UtcNow)` dengan pesan baku `VR-OT-01`. |
| Klaim EmergencyCito tanpa Nomor RM / Insiden ditolak | ✅ Lolos | Diuji oleh blok `When(x => x.OvertimeCategory == "EmergencyCito")` pada `EmergencyClinicalReferenceNo` & `ClinicalActionSummary`. |
| Pegawai Manajer ke atas ditolak memilih kompensasi finansial | ✅ Lolos | Diuji oleh rule `VR-OT-05` yang menolak `CompensationType == "FinancialPayout"` untuk `IsOvertimeExempt = true`. |
| SPKL kelompok tanpa nakes pelaksana ditolak | ✅ Lolos | Diuji oleh rule `VR-OT-20` pada `StaffList.NotEmpty()`. |
