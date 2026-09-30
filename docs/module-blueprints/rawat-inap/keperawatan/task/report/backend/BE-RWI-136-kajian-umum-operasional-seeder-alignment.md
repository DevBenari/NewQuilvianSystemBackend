# Laporan Perubahan Backend — `BE-RWI-136-kajian-umum-operasional-seeder-alignment`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-136` |
| Judul | Penyelarasan Seeder Definisi Instrumen Kajian Umum Selaras Screenshot Operasional Rumah Sakit |
| Modul | Rawat Inap — Pelayanan Keperawatan (*Inpatient Nursing Workspace*) |
| Menu Sasaran | Menu 1: Pengkajian Pasien (*Patient Assessment*) — Sub-Menu: Kajian Umum |
| Rencana Kerja | [`rencana-kerja-kajian-umum.md`](../../roadmap/rencana-kerja/pengkajian-umum/rencana-kerja-kajian-umum.md) — Tahap 1 |
| Trace | `RWI-DEC-141`, `RLN3-CAP-05`, `FR-KEP-039`, `FR-KEP-045`, `BE-RWI-127` |
| Status Database | **Zero Migration** (Tidak ada perubahan skema database baru). Menggunakan kolom `ResponsesJson` pada `CliAssessmentInstrumentResponse` dan entitas `TrxPatientAssessment`. |
| Task Mode | `BACKEND` |
| Target Tulis | `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` |
| Hasil Validasi | `dotnet build --no-restore` lolos tanpa galat (**0 Error, 0 Warning**). |
| Tanggal | 29 September 2026 |
| Status | ✅ Selesai (Tahap 1 Terpenuhi) |

---

## 1. Masalah yang Diperbaiki

Berdasarkan audit 9 tangkapan layar (*screenshot*) operasional rumah sakit pada folder `referensi/kajian-umum`:
1. Metadata seeder instrumen awal memiliki susunan seksi yang belum presisi dengan formulir operasional harian perawat, di mana status fungsional, skrining nutrisi, dan tingkat ketergantungan ADL membutuhkan parameter-parameter spesifik yang sesuai dengan formulir fisik/operasional rumah sakit.
2. Skrining nutrisi di operasional rumah sakit terbagi menjadi 3 profil pasien yang berbeda:
   - **Skrining Gizi Dewasa** (4 parameter: TB/BB, Penurunan BB, Penurunan asupan, Kondisi sakit berat).
   - **Skrining Gizi Anak** (6 parameter: Tampak kurus, Penurunan BB 1 bulan, Diare/muntah, Asupan makan, Penyakit berisiko malnutrisi, Kategori risiko gizi).
   - **Skrining Gizi Obesitas** (4 parameter: Berat badan, Tinggi badan, Indeks Massa Tubuh / IMT, Kategori Obesitas).
3. Status Fungsional Barthel Index di operasional dinilai menggunakan skala matriks 0–2 untuk 10 butir aktivitas (0 = Tergantung/Tidak Mampu, 1 = Perlu Bantuan, 2 = Mandiri) dengan rentang total skor 0–20.
4. Ketergantungan ADL di operasional memiliki 10 baris indikator (Mobilisasi, Personal, Toileting, Berpakaian, Makan/Minum, Kesadaran, Observasi TTV, Respirasi, Pengobatan, Lapor DPJP) serta 9 alat bantu multi-pilih (Penyangga, Gigi Palsu, Tongkat, Kacamata, Kursi Roda, Mata Palsu, Pacemaker, Alat Bantu Dengar, Walker).

---

## 2. Perubahan yang Dilakukan

Pembaruan dilakukan pada berkas `ClinicalInstrumentDraftSeeder.cs` method `BuildGeneralNursingAssessment()`:

1. **Seksi 1 (`SUMBER_DATA`)**:
   - Memasukkan parameter `DATA_SOURCE_TYPE` (`Pasien Sendiri`, `Keluarga`, `Orang Lain`).
   - Menyertakan data keluarga/pemberi informasi (`DATA_SOURCE_FAMILY_NAME`, `DATA_SOURCE_FAMILY_RELATION`, `DATA_SOURCE_FAMILY_PHONE`).
   - Menyertakan status psikososial dan nilai kepercayaan yang dianut.

2. **Seksi 2 (`PERNAPASAN`)**:
   - Menyelaraskan opsi pola napas (`Normal`, `Dispnea`, `Sianosis`, `Ortopnea`, `Batuk`, `Trakeostomi`).
   - Menyelaraskan opsi alat bantu pernapasan (`Nasal Kanul`, `Sungkup`, `Ventilator`, `Tidak Ada`).
   - Menyelaraskan suara napas (`Vesikuler`, `Wheezing`, `Ronkhi`, `Stridor`).

3. **Seksi 3 (`INTEGRITAS_KULIT`)**:
   - Menyelaraskan opsi warna kulit (`Normal`, `Pucat`, `Kuning/Ikterik`, `Sianosis`).
   - Menyelaraskan turgor kulit (`Baik`, `Sedang`, `Buruk`).
   - Menyelaraskan risiko dekubitus / Braden Scale (`Tidak Berisiko`, `Risiko Rendah`, `Risiko Sedang`, `Risiko Tinggi`) beserta stadium luka tekan.

4. **Seksi 4 (`SKRINING_NUTRISI`)**:
   - Memasukkan metadata 3 sub-kartu:
     - `Dewasa`: `NUT_ADULT_TB`, `NUT_ADULT_BB`, `NUT_ADULT_WEIGHT_LOSS`, `NUT_ADULT_INTAKE_DECREASE`, `NUT_ADULT_SEVERE_ILLNESS`.
     - `Anak`: `NUT_PEDIATRIC_THIN`, `NUT_PEDIATRIC_WEIGHT_LOSS`, `NUT_PEDIATRIC_GI_SYMPTOMS`, `NUT_PEDIATRIC_INTAKE`, `NUT_PEDIATRIC_RISK_DISEASE`, `NUT_PEDIATRIC_CATEGORY`.
     - `Obesitas`: `NUT_OBESITY_BB`, `NUT_OBESITY_TB`, `NUT_OBESITY_BMI`, `NUT_OBESITY_CATEGORY`.

5. **Seksi 5 (`ELIMINASI`)**:
   - Menyelaraskan eliminasi urin (frekuensi, warna urin, penggunaan kateter, tanggal pasang, ukuran kateter Fr).
   - Menyelaraskan eliminasi alvi/BAB (frekuensi, konsistensi feses, colostomy/stoma).

6. **Seksi 6 (`KETERGANTUNGAN_ADL`)**:
   - 10 butir indikator ketergantungan ADL lengkap.
   - Pilihan multi-centang 9 alat bantu aktivitas harian.
   - Penanda eskalasi lapor dokter DPJP (`ADL_REPORT_DPJP`).

7. **Seksi 7 (`STATUS_FUNGSIONAL`)**:
   - 10 butir aktivitas baku Barthel Index dengan skala 0, 1, dan 2.
   - Skema akumulasi total skor (0–20) dan kategori ketergantungan (*Total*, *Berat*, *Sedang*, *Ringan*, *Mandiri*).

8. **Sinkronisasi Otomatis Seeder**:
   - Seeder secara otomatis menghitung `DefinitionHash` dan memperbarui instrumen draft baseline jika terdapat pembaruan definisi.

---

## 3. Bukti Verifikasi Kompilasi Backend

- **Perintah**: `dotnet build --no-restore`
- **Direktori**: `NewQuilvianSystemBackend`
- **Hasil**:
  ```text
  QuilvianSystemBackend -> C:\Users\Admin\Documents\Quilvian\Source Code\QuilvianFinal\NewQuilvianSystemBackend\bin\Debug\net9.0\QuilvianSystemBackend.dll

  Build succeeded.
      0 Warning(s)
      0 Error(s)

  Time Elapsed 00:00:02.65
  ```
- **Kesimpulan**: Kode backend tervalidasi bersih tanpa eror dan tanpa warning.
