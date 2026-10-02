# Laporan Perubahan Backend — `BE-RWI-127-kajian-umum-8-seksi-seeder`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-127` |
| Judul | Penyelarasan Seeder Definisi Instrumen Kajian Umum 8 Seksi Lengkap dari V1 |
| Modul | Rawat Inap — Pelayanan Keperawatan (*Inpatient Nursing Workspace*) |
| Menu Sasaran | Menu 1: Pengkajian Pasien (*Patient Assessment*) — Sub-Menu: Kajian Umum |
| Rencana Kerja | [`rencana-kerja-kajian-umum.md`](../../rencana-kerja-kajian-umum.md) — Tahap 1 |
| Trace | `RWI-DEC-141`, `RLN3-CAP-05`, `BE-RWI-107`, `FR-KEP-039`, `FR-KEP-045` |
| Status Database | **Zero Migration** (Tidak ada penambahan tabel atau skema baru). Menggunakan kolom `ResponsesJson` pada `CliAssessmentInstrumentResponse` dan kolom entitas `TrxPatientAssessment`. |
| Task Mode | `BACKEND` |
| Target Tulis | `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` |
| Hasil Validasi | `dotnet build` lolos tanpa galat (224 warning pra-eksisting, 0 Error, waktu kompilasi 90 detik). |
| Tanggal | 24 September 2026 |
| Status | ✅ Selesai (Tahap 1) |

---

## 1. Masalah yang Diperbaiki

Pada seeder instrumen awal (`ClinicalInstrumentDraftSeeder.cs:290`), pengembang sebelumnya menyederhanakan formulir Kajian Umum menjadi catatan teks kosong (*textarea*) dengan pesan peninjauan:
> *"Susunan delapan bagian mengikuti RWI-DEC-141; isian bagian Pernapasan, Integritas Kulit, Eliminasi, dan Ketergantungan belum dipetakan dari label V1 (RLN3-CAP-05) dan hanya berupa catatan."*

Akibatnya:
1. Perawat bangsal di `QuilvianFinal` kehilangan formulir operasional terstruktur yang sebelumnya sudah disetujui klien di `QuilvianV1` (Pernapasan, Braden Scale, MST Nutrisi, Eliminasi kateter/stoma, Ketergantungan ADL, dan Barthel Index).
2. Data klinis tidak dapat terkuantifikasi dan tidak memicu perlindungan keselamatan pasien (*safety alerts*).

---

## 2. Perubahan yang Dilakukan

1. **Pembaruan Definisi `GENERAL_NURSING_ASSESSMENT`**:
   - Memetakan 8 seksi secara penuh dan komprehensif ke dalam objek `ClinicalInstrumentDefinition`:
     - **Seksi 1 (`SUMBER_DATA`)**: Sumber data (Pasien/Orang Lain), nama pemberi informasi, hubungan keluarga, nilai kepercayaan, skrining psikologis (tenang, cemas, takut, depresi/bunuh diri), hubungan keluarga, tempat tinggal, gangguan fungsional sensorik/motorik, dan catatan relevan (`PsychosocialNote`).
     - **Seksi 2 (`KONDISI_UMUM`)**: Keluhan utama (`ChiefComplaint`), riwayat penyakit sekarang (`CurrentIllnessHistory`), riwayat obat (`MedicationHistory`), tingkat kesadaran (`ConsciousnessStatus`), riwayat alergi (`HasAllergy`), dan catatan alergi (`AllergyNote`).
     - **Seksi 3 (`PERNAPASAN`)**: Kesulitan bernapas, pemakaian terapi O2 (`IsUsingOxygen`), aliran oksigen L/menit (`OxygenFlowRate`), jenis alat bantu O2 (`OxygenSupportType`), batuk produktif, pola napas (regular, takipnea, kussmaul, dll.), dan gejala penyerta (dyspnea, orthopnea, sianosis, wheezing, stridor).
     - **Seksi 4 (`INTEGRITAS_KULIT`)**: Integritas terganggu, Braden Scale score (6–23), deskripsi kulit (ruam, parut, memar, sianotik, berkeringat, dekubitus), stadium luka tekan (Stage 1–4, unstageable), dan lokasi/catatan.
     - **Seksi 5 (`SKRINING_NUTRISI`)**: Nafsu makan (`AppetiteStatus`), mual (`HasNausea`), muntah (`HasVomiting`), skor penurunan BB MST (0–4), penurunan asupan makan MST (0–1), kondisi penyakit berat/kritis, gangguan metabolisme/komorbid (DM, HT, Ginjal, dll.), status risiko gizi (`NutritionRiskStatus`), dan total skor MST (`NutritionRiskScore`).
     - **Seksi 6 (`ELIMINASI`)**: Masalah BAK (striktur, retensi, inkontinensia, dialisis, disuria), warna urin, pemasangan kateter urin (jenis: foley, silikon, kondom, suprapubik; ukuran Fr; tanggal pasang), masalah BAB (stoma, atresia ani, konstipasi, inkontinensia alvi, diare, melena), dan catatan eliminasi.
     - **Seksi 7 (`KETERGANTUNGAN`)**: 5 indikator ADL (mobilisasi, kebersihan diri, toileting, berpakaian, makan/minum dengan opsi: mandiri, dibantu sebagian, tergantung penuh), alat bantu aktivitas (kursi roda, tongkat, walker, penyangga, gigi palsu, kacamata, alat dengar, tirah baring), dan indikator notifikasi DPJP.
     - **Seksi 8 (`STATUS_FUNGSIONAL`)**: 10 parameter baku Barthel Index berskor (Defekasi, Miksi, Cuci Muka/Grooming, Toilet, Makan, Transfer Bed-Chair, Mobilitas Datar, Berpakaian, Tangga, Mandi), status fungsional umum (`FunctionalStatus`), dan catatan status fungsional (`FunctionalNote`).

2. **Dukungan Sinkronisasi Seeder Otomatis (*Draft Auto-Update*)**:
   - Memodifikasi mekanisme loop seeder agar memeriksa apakah versi draft baseline (`c1a1f000-0107-4a01-9b02-000000000004`) sudah ada di database.
   - Bila sudah ada dan statusnya masih `Draft`, seeder secara otomatis memperbarui `DefinitionJson` dan `DefinitionHash` terbaru tanpa perlu menghapus database (*zero manual maintenance*).

---

## 3. Bukti Verifikasi Kompilasi Backend

- Perintah: `dotnet build --no-incremental`
- Hasil:
  ```text
  Build succeeded.
      224 Warning(s)
      0 Error(s)
  Time Elapsed 00:01:30.63
  ```
- Seluruh kode C# tervalidasi sintaksis, tipe enum, dan dependensi model.

---

## 4. Langkah Berikutnya (Tahap 2)

Melanjutkan ke **Tahap 2**:
- Memodifikasi `clinical-instrument-form-renderer.jsx` di repository frontend agar mendukung **Accordion / Expandable Sections** per seksi (buka/tutup seksi), tata letak grid dua kolom, serta visualisasi indikator klinis yang rapi dan ergonomis bagi perawat bangsal.
