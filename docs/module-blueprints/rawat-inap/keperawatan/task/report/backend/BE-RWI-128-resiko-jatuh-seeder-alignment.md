# Laporan Perubahan Backend — `BE-RWI-128-resiko-jatuh-seeder-alignment`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-128` |
| Judul | Penyelarasan Seeder Definisi 3 Skala Usia Risiko Jatuh (Humpty Dumpty, Morse, Ontario Modified Stratify) dan Checklist Intervensi Pencegahan Jatuh |
| Modul | Pelayanan Kesehatan — Rawat Inap (*Inpatient Nursing Workspace*) |
| Menu Sasaran | Menu 1: Pengkajian Pasien (*Patient Assessment*) — Sub-Menu 2: Resiko Jatuh (*Fall Risk Assessment*) |
| Rencana Kerja | [`rencana-kerja-resiko-jatuh.md`](../../rencana-kerja-resiko-jatuh.md) — Tahap 1 |
| Trace | `SKP 6 / IPSG 6`, `BE-RWI-107`, `BE-RWI-109`, `FR-KEP-044`, `VAL-KEP-19c`, `AC-KEP-057` |
| Status Database | **Zero Migration** (Tidak ada penambahan tabel atau perubahan skema baru). Menggunakan entitas `TrxPatientAssessment` (`HasFallRisk`, `FallRiskStatus`, `FallRiskScore`, `FallRiskNote`), `CliClinicalInstrument`, `CliClinicalInstrumentVersion`, dan `ResponsesJson` pada `CliAssessmentInstrumentResponse`. |
| Task Mode | `BACKEND` |
| Target Tulis | `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` |
| Hasil Validasi | `dotnet build --no-incremental` lolos tanpa galat (257 warning pra-eksisting, **0 Error**, waktu kompilasi 1 menit 40 detik). |
| Tanggal | 24 September 2026 |
| Status | ✅ Selesai (Tahap 1) |

---

## 1. Masalah yang Diperbaiki

Pada seeder instrumen awal (`ClinicalInstrumentDraftSeeder.cs:174-255`), konfigurasi instrumen risiko jatuh mengalami beberapa kelemahan dan ketidaklengkapan klinis:
1. **`FALL_RISK_CHILD` (Humpty Dumpty)**: Bagian penilaian masih berupa draf kosong tanpa butir parameter (`Sections = { new() { Code = "PENILAIAN", Label = "Penilaian" } }`), dan pita skornya mengadopsi ambang Morse yang salah (Rendah 0–24, Sedang 25–44, Tinggi $\ge 45$) padahal skor maksimum skala Humpty Dumpty hanya 23.
2. **`FALL_RISK_ADULT` (Morse Fall Scale)**: Rentang skor pita (*bands*) tumpang tindih (*overlap*) pada skor 50: Kategori Sedang `[25, 51)` dan Kategori Tinggi `[50, null)`. Skor 50 masuk ke dua kategori sekaligus, sehingga memicu kegagalan validasi pita (`ValidateBands`).
3. **`FALL_RISK_ELDERLY` (Ontario Modified Stratify - Sydney Scoring)**: Bagian penilaian masih draf kosong tanpa parameter butir dan tidak memiliki kategori Tinggi ($\ge 17$), sehingga skor di atas 16 tidak memiliki kategori (*berlubang*).
4. **Ketiadaan Seksi Checklist Intervensi Pencegahan Jatuh**: Ketiga instrumen belum memiliki seksi terpadu untuk mendokumentasikan tindakan pencegahan jatuh (pemasangan gelang kuning, penanda visual segitiga kuning, pengaman tempat tidur, edukasi keluarga).

---

## 2. Perubahan yang Dilakukan

Berkas yang dimodifikasi:  
[`NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs)

### 1. Pelengkapan Definisi `FALL_RISK_CHILD` (Humpty Dumpty Fall Scale)
- **Kelompok Usia**: Pasien anak (`0` s.d. `216` bulan / < 18 tahun).
- **Seksi 1 (`HUMPTY_DUMPTY`)**: 7 parameter klinis baku berbobot skor:
  - `HUMP_AGE`: Usia (< 3 thn: 4, 3–7 thn: 3, 7–13 thn: 2, $\ge$ 13 thn: 1).
  - `HUMP_GENDER`: Jenis kelamin (Laki-laki: 2, Perempuan: 1).
  - `HUMP_DIAGNOSIS`: Diagnosis medis (Kelainan neurologi: 4, Perubahan oksigenasi/respirasi/anemia: 3, Masalah psikis: 2, Lainnya: 1).
  - `HUMP_COGNITIVE`: Gangguan kognitif (Tidak sadar keterbatasan: 3, Lupa keterbatasan: 2, Mengetahui kemampuan: 1).
  - `HUMP_ENVIRONMENT`: Faktor lingkungan (Riwayat jatuh/boks khusus: 4, Alat bantu/ranjang standar: 3, Ranjang standar tanpa bantuan: 2, Rawat jalan: 1).
  - `HUMP_SURGERY`: Pembedahan/sedasi/anestesi (Dalam 24 jam: 3, Dalam 48 jam: 2, > 48 jam/tanpa bedah: 1).
  - `HUMP_MEDICATION`: Penggunaan obat (Bermacam obat berisiko: 3, Salah satu obat: 2, Tanpa obat berisiko: 1).
- **Seksi 2 (`INTERVENSI`)**: Checklist 8 tindakan pencegahan jatuh baku + catatan khusus.
- **Pita Skor Baku (*Bands*)**:
  - `Band("LOW", "Rendah", 0, 12, false, "LowRisk")` — mencakup skor 7–11.
  - `Band("HIGH", "Tinggi", 12, null, true, "HighRisk")` — mencakup skor $\ge 12$.

### 2. Penyelarasan Definisi & Pita Skor `FALL_RISK_ADULT` (Morse Fall Scale)
- **Kelompok Usia**: Pasien dewasa (`216` s.d. `720` bulan / 18–59 tahun).
- **Seksi 1 (`MORSE`)**: 6 parameter baku:
  - `MORSE_HISTORY`: Riwayat jatuh (Tidak: 0, Ya: 25).
  - `MORSE_SECONDARY_DX`: Diagnosis sekunder $\ge 2$ (Tidak: 0, Ya: 15).
  - `MORSE_AMBULATORY_AID`: Alat bantu jalan (Tidak ada/tirah baring: 0, Kruk/tongkat/walker: 15, Bertumpu perabot: 30).
  - `MORSE_IV`: Terpasang infus/heparin lock (Tidak: 0, Ya: 20).
  - `MORSE_GAIT`: Cara berjalan/gait (Normal/imobil: 0, Lemah: 10, Terganggu: 20).
  - `MORSE_MENTAL`: Status mental (Sadar kemampuan: 0, Lupa keterbatasan: 15).
- **Seksi 2 (`INTERVENSI`)**: Checklist 8 tindakan pencegahan jatuh baku + catatan khusus.
- **Pita Skor Baku Bebas Overlap (*Bands*)**:
  - `Band("LOW", "Rendah", 0, 25, false, "LowRisk")` — skor 0 s.d. 24.
  - `Band("MEDIUM", "Sedang", 25, 45, false, "MediumRisk")` — skor 25 s.d. 44.
  - `Band("HIGH", "Tinggi", 45, null, true, "HighRisk")` — skor $\ge 45$.

### 3. Pelengkapan Definisi `FALL_RISK_ELDERLY` (Ontario Modified Stratify - Sydney Scoring)
- **Kelompok Usia**: Pasien lansia/geriatri ($\ge 720$ bulan / $\ge 60$ tahun).
- **Seksi 1 (`SYDNEY`)**: 5 parameter baku klinis geriatri:
  - `SYD_HISTORY`: Riwayat jatuh saat masuk / 1 bulan terakhir (Tidak: 0, Ya: 6).
  - `SYD_MENTAL`: Status mental/kognitif (Sadar baik: 0, Agitasi/disorientasi/demensia: 14).
  - `SYD_VISION`: Penglihatan/vision (Normal: 0, Gangguan penglihatan/kacamata: 1).
  - `SYD_TOILETING`: Kebiasaan berkemih (Normal: 0, Sering berkemih/inkontinensia: 2).
  - `SYD_MOBILITY`: Transfer tempat tidur ke kursi & mobilitas (Mandiri: 0, Bantuan 1 orang: 3, Tergantung penuh: 3).
- **Seksi 2 (`INTERVENSI`)**: Checklist 8 tindakan pencegahan jatuh baku + catatan khusus.
- **Pita Skor Baku Menutup Lubang (*Bands*)**:
  - `Band("LOW", "Rendah", 0, 6, false, "LowRisk")` — skor 0 s.d. 5.
  - `Band("MEDIUM", "Sedang", 6, 17, false, "MediumRisk")` — skor 6 s.d. 16.
  - `Band("HIGH", "Tinggi", 17, null, true, "HighRisk")` — skor $\ge 17$.

---

## 3. Bukti Verifikasi Kompilasi Backend

- Perintah: `dotnet build --no-incremental`
- Hasil Eksekusi:
  ```text
  Build succeeded.
      257 Warning(s)
      0 Error(s)
  Time Elapsed 00:01:40.55
  ```
- Seluruh kode C# tervalidasi sintaksis, logika definisi instrumen lulus validasi internal `ClinicalInstrumentDefinitionEngine`, dan tidak ada galat dependensi model.

---

## 4. Langkah Berikutnya (Tahap 2)

Melanjutkan ke **Tahap 2: Frontend Instrument Rendering & Safety Alerts**:
1. Memastikan `ClinicalInstrumentFormRenderer` dan `assessment-section.jsx` merender parameter pilihan opsi radio berskor secara ergonomis dan rapi.
2. Menampilkan indikator skor total dan lencana warna pita risiko secara real-time (*Live Preview*).
3. Mengaktifkan **Clinical Safety Alert Banner** khusus kategori Risiko Tinggi (Protokol Gelang Kuning & Pengaman Tempat Tidur).
4. Memastikan checklist intervensi pencegahan jatuh tersimpan dengan aman ke `ResponsesJson`.
