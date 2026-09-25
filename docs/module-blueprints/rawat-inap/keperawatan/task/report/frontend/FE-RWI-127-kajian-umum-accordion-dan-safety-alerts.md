# Laporan Perubahan Frontend — `FE-RWI-127-kajian-umum-accordion-dan-safety-alerts`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-127` |
| Judul | Penyempurnaan Renderer Pengkajian Pasien: Accordion Sections, Toolbar Navigasi, dan Banner Keselamatan Klinis (*Clinical Safety Alerts*) |
| Modul | Rawat Inap — Pelayanan Keperawatan (*Inpatient Nursing Workspace*) |
| Menu Sasaran | Menu 1: Pengkajian Pasien (*Patient Assessment*) — Sub-Menu: Kajian Umum |
| Rencana Kerja | [`rencana-kerja-kajian-umum.md`](../../rencana-kerja-kajian-umum.md) — Tahap 2 & Tahap 3 |
| Trace | `FE-RWI-083`, `FR-KEP-039`, `FR-KEP-045`, `FR-KEP-046`, `RWI-DEC-141` |
| Task Mode | `FRONTEND` |
| Target Tulis | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/renderer/clinical-instrument-form-renderer.jsx`, `src/style/health-services/inpatient-management/nursing-workspace.module.css`, `tests/unit/inpatient-general-assessment-accordion-and-alerts.test.mjs` |
| Hasil Validasi | Unit test `inpatient-general-assessment-accordion-and-alerts.test.mjs` lolos 2/2; regression test suite `inpatient-*.test.mjs` lolos 665/665 PASS (0 FAIL). |
| Tanggal | 24 September 2026 |
| Status | ✅ Selesai (Tahap 2 & Tahap 3) |

---

## 1. Masalah yang Diperbaiki

Sebelumnya:
1. Formulir pengkajian dirender sebagai daftar vertikal yang sangat panjang tanpa pengelompokan yang dapat dilipat/dibuka (*collapsible*). Saat 8 seksi lengkap diaktifkan, perawat harus melakukan *scroll* layar yang melelahkan untuk berpindah antar topik klinis.
2. Tidak ada indikator ringkas pada header seksi mengenai berapa banyak butir pertanyaan yang sudah terisi.
3. Nilai skor kritis yang dimasukkan perawat (seperti Braden Scale $\le 12$, Skor Malnutrisi MST $\ge 2$, atau Ketergantungan Total ADL 5 indikator) tidak memicu peringatan visual aktif (*Clinical Safety Alerts*), sehingga berisiko terlewat dalam alur komunikasi dokter dan perawat.

---

## 2. Perubahan yang Dilakukan

1. **Fitur Accordion / Expandable Section**:
   - Header setiap seksi kini interaktif (`instrumentSectionHeaderInteractive`) dengan efek hover yang lembut.
   - Status lipatan (`collapsedSections`) dikelola per seksi secara dinamis.
   - Ikon panah penunjuk status (`FaChevronDown` saat tertutup, `FaChevronUp` saat terbuka).
   - Lencana jumlah isian terisi (`answered/total terisi`) dengan warna hijau saat sudah terisi atau abu-abu saat masih kosong.
   - Dukungan aksesibilitas keyboard: tombol Enter dan Spasi dapat membuka/menutup seksi.

2. **Toolbar Navigasi Cepat Pengkajian**:
   - Menyediakan tombol cepat **"Buka Semua"** dan **"Tutup Semua"** di atas daftar seksi untuk navigasi instan saat perawat ingin melihat formulir secara keseluruhan atau fokus per seksi.

3. **Banner Peringatan Keselamatan Klinis Real-Time (*Clinical Safety Alerts*)**:
   - Dihitung secara reaktif berbasis memoize (`useMemo`) tanpa membebani re-render formulir.
   - **Alert 1 (Risiko Dekubitus)**: Terpicu saat `SKIN_BRADEN_SCORE` terisi angka $\le 12$. Menampilkan banner merah: *"Pasien berisiko tinggi dekubitus. Pasang kasur anti-dekubitus, berikan pelembab/barrier cream, dan jadwalkan alih baring setiap 2 jam."*
   - **Alert 2 (Risiko Malnutrisi)**: Terpicu saat `NUT_RISK_SCORE` $\ge 2$ atau status gizi bernilai `HighRisk`/`MediumRisk`. Menampilkan banner amber: *"Skrining nutrisi menunjukkan pasien berisiko malnutrisi. Segera jadwalkan konsultasi asuhan gizi bersama dietisien."*
   - **Alert 3 (Ketergantungan Total ADL)**: Terpicu saat kelima aktivitas ADL bernilai `TERGANTUNG_PENUH`. Menampilkan banner merah: *"Pasien mengalami ketergantungan total pada seluruh aktivitas harian. Wajib lapor dokter DPJP untuk penetapan protokol perawatan intensif."*

---

## 3. Bukti Verifikasi Pengujian

1. **Unit Test Spesifik Accordion & Alerts**:
   ```bash
   node --test tests/unit/inpatient-general-assessment-accordion-and-alerts.test.mjs
   ```
   Hasil:
   ```text
   ✔ TAHAP-2 & 3: Accordion / Expandable Sections dan Toolbar Buka/Tutup Seluruh Seksi (8.3ms)
   ✔ TAHAP-3: Clinical Safety Alerts Banner untuk Kajian Umum (Dekubitus, MST Gizi, ADL Lapor DPJP) (3.4ms)
   ℹ tests 2, pass 2, fail 0
   ```

2. **Pengujian Regresi Komprehensif Seluruh Workspace Rawat Inap**:
   ```bash
   node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-*.test.mjs
   ```
   Hasil:
   ```text
   ℹ tests 665, suites 0, pass 665, fail 0, duration_ms 3166ms
   ```
   Tidak ada cacat regresi pada fitur rujukan TTV, evaluasi nyeri, resep dokter, atau prosedur tindakan.
