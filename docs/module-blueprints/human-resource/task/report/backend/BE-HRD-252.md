# Laporan Task Tracked: BE-HRD-252 — Portofolio 50 SKP Kemenkes Plataran Sehat Gateway & Feed OPPE Sub-15

**ID Task**: `BE-HRD-252`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-50` (Ikatan Dinas, Penalti Wanprestasi & Portofolio SKP Kemenkes Plataran Sehat)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan pencatatan portofolio perolehan Satuan Kredit Profesi (SKP) tenaga kesehatan berbasis regulasi Kementerian Kesehatan RI, gateway integrasi ke Plataran Sehat / SatuSehat SDMK (`INT-HR-80`), serta feed data kepatuhan ke modul evaluasi praktik profesional berkelanjutan (*Ongoing Professional Practice Evaluation* / OPPE Sub-15, `INT-HR-78`):
- **Target 50 SKP 5 Tahunan**: Memantau akumulasi pencapaian target 50 SKP per siklus 5 tahun untuk syarat perpanjangan SIP/STR (`VR-TRN-22`, `AT-HRD-TRN-19`).
- **Klasifikasi 3 Ranah SKP Kemenkes**: Mendukung kategori Pembelajaran (Diklat/Seminar), Pelayanan (Tindakan Medis/Visite), dan Pengabdian Masyarakat (Baksos/Bencana) (`VR-TRN-23`).
- **Verifikasi Plataran Sehat**: Memvalidasi ID sertifikat Plataran Sehat Kemenkes dan mencatat stempel verifikasi SatuSehat (`INT-HR-80`, `AT-HRD-TRN-20`).
- **Sinkronisasi Feed OPPE Sub-15**: Mengirimkan rekaman kegiatan dan total capaian SKP secara otomatis ke modul OPPE sebagai indikator kinerja profesional tenaga medis (`INT-HR-78`, `AT-HRD-TRN-21`).

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/PlataranSehatSkpDtos.cs`
     - `RecordSkpActivityRequestDto`, `PlataranSehatSkpRecordDto`, `SkpPortfolioSummaryDto`, `SyncOppeResponseDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/IPlataranSehatSkpService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/PlataranSehatSkpService.cs`
     - Pencatatan aktivitas SKP per kategori Kemenkes.
     - Penghitungan persentase progress menuju target 50 SKP.
     - Verifikasi ID sertifikat Plataran Sehat Kemenkes.
     - Sinkronisasi data ke feed OPPE Sub-15 (`INT-HR-78`).
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/PlataranSehatSkpsController.cs`
     - `POST /api/v1/corporate/hrd/training/plataran-sehat-skp`
     - `GET /api/v1/corporate/hrd/training/plataran-sehat-skp/portfolio/{workforceProfileId}`
     - `POST /api/v1/corporate/hrd/training/plataran-sehat-skp/sync-oppe/{workforceProfileId}`
     - `POST /api/v1/corporate/hrd/training/plataran-sehat-skp/verify/{skpRecordId}`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<IPlataranSehatSkpService, PlataranSehatSkpService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-22`**: Target akumulasi 50 SKP nakes terpantau secara transparan.
- **`VR-TRN-23`**: 3 Ranah Kemenkes (Pembelajaran, Pelayanan, Pengabdian) terakomodasi dalam penghitungan.
- **`INT-HR-80`**: Gateway verifikasi Plataran Sehat tervalidasi.
- **`INT-HR-78`**: Feed integrasi ke modul evaluasi OPPE Sub-15 berfungsi otomatis.
- **`AT-HRD-TRN-19` s.d. `AT-HRD-TRN-21`**: Seluruh kriteria penerimaan teruji dan lulus.
