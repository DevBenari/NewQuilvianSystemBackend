# Laporan Perubahan Frontend — `FE-RWI-053`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-053` |
| **Judul** | Lini Masa Pengkajian Keperawatan Rawat Inap (Monitoring Kronologis Menurun, Tren Parameter Nyeri/Jatuh/Gizi, dan Integritas Historis) |
| **Slice** | Gelombang `KEP-MVP-1` — Lini Masa Pemantauan Pengkajian |
| **Roadmap** | [`../../../roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian 3 task `FE-RWI-053` |
| **Trace** | `FE-KEP-03`; `FR-KEP-007`, `FR-KEP-010`, `FR-KEP-011`; `AC-CAP012-02`, `RWI-DEC-091`; `03-frontend-architecture.md` bagian 3.3; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 13, 14, 26, 27 |
| **Contract version** | API `0.3.0` (`GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}/timeline` dan `GET .../episodes/{episodeId}/due-status`), validation `0.3.0` (`VAL-KEP-17`) |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../../../skema-tampilan-keperawatan-rawat-inap.md) Bagian 13, 14, 26 |
| **Dependency** | `FE-RWI-051` ✅, `FE-RWI-052` ✅, `BE-RWI-058` ✅ |
| **Klasifikasi** | `MEDIUM` — Skor 7: 2 repository, 8 berkas dibuat/diperbarui, kalkulasi tren dinamis, boundary legalitas rekam medis historis |
| **Task mode** | `CROSS-REPO` sempit — source aplikasi dikerjakan di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Target tulis** | Frontend: `src/components/ui/clinical-workspace/**`, `src/components/view/health-services/inpatient-management/nursing-workspace/sections/timeline/**`, `src/lib/hooks/**`, `src/utils/**`, `tests/unit/inpatient-nursing-timeline.test.mjs`; Backend: `docs/module-blueprints/rawat-inap/keperawatan/task/report/frontend/FE-RWI-053.md`, `frontend-roadmap.md`, dan `requirement-traceability.md` |
| **Tanggal** | 8 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh Acceptance Criteria terbukti (4/4 fungsional). Seluruh unit test lulus (8/8 pass timeline + 14/14 pass pendukung). Tidak ada error pada build dan linting. |

---

## 1. Masalah yang Diselesaikan

Sebelum implementasi task ini:
1. **Ketiadaan Garis Waktu Kronologis Menurun:** Riwayat pengkajian pasien rawat inap tersebar atau hanya berupa tabel daftar tanpa kesinambungan garis waktu (*timeline track*). Klinisi (perawat primer, perawat pelaksana, maupun DPJP) kesulitan melihat secara cepat bagaimana kondisi pasien berkembang dari waktu ke waktu sejak hari pertama masuk rawat hingga saat ini (`AC-CAP012-02`).
2. **Bahaya Kehilangan Nilai Historis Rekam Medis:** Sistem lama berisiko menimpa (*overwrite*) nilai pengukuran lama saat perawat melakukan pengkajian ulang, melanggar prinsip integritas data rekam medis di mana riwayat pengukuran masa lalu tidak boleh hilang atau dihapus.
3. **Ketiadaan Kalkulasi Tren Perkembangan Parameter Klinis:** Perawat harus membaca angka satu per satu secara manual untuk menyimpulkan apakah nyeri pasien membaik atau memburuk, atau apakah risiko jatuh meningkat setelah pergantian shift.
4. **Pencampuradukan Tiga Kondisi Ekstrem (Empty, Error, No Policy):** Banyak antarmuka menyamakan kondisi *"belum ada pengkajian"* dengan kondisi *"gagal memuat data"*, atau menampilkan pesan error/countdown palsu ketika master kebijakan rumah sakit (`MstClinicalAssessmentPolicy`) belum dikonfigurasi (`VAL-KEP-17`).
5. **Kurangnya Transparansi Addendum Koreksi:** Nilai yang telah dikoreksi melalui addendum tidak ditandai secara visual pada garis waktu, sehingga perawat lain dapat salah menafsirkan keabsahan data masa lalu (`RWI-DEC-091`).

Perubahan pada `FE-RWI-053` menyelesaikan permasalahan tersebut secara menyeluruh dengan menghadirkan:
- Komponen dasar terstandar: `ClinicalTimeline`, `ClinicalTimelineItem`, dan `ClinicalDocumentMeta` pada pustaka `clinical-workspace`.
- Hook terisolasi `usePatientAssessmentTimeline` yang memuat data lini masa dan status tenggat waktu pengkajian awal secara serentak, menyortir urutan kronologis menurun (waktu terbaru di atas), serta menghitung tren komparatif terhadap pengukuran sebelumnya.
- Penyaringan terfokus parameter klinis: Semua Parameter, Skala Nyeri, Risiko Jatuh, dan Status Gizi.
- Penegakan pemisahan mutlak tiga keadaan sistem: Empty State dengan tombol pembuatan asesmen, Error State dengan tombol coba lagi, dan No Policy State yang menampilkan status abu-abu netral *"Batas Waktu Belum Ditetapkan"* tanpa menghambat alur kerja keperawatan.
- Tampilan kartu pengukuran `TimelineMeasurementCard` yang secara eksplisit menyematkan penanda `[Koreksi #X]` bila terdapat addendum resmi tanpa menghapus nilai rekaman asli.

---

## 2. Proses Bisnis & Alur Pengguna

### 2.1 Alur Pemantauan Normal (Happy Path)
1. **Pemicu:** Perawat membuka Ruang Kerja Keperawatan untuk pasien rawat inap dan mengklik tab **Lini Masa**.
2. **Penyajian Garis Waktu Terurut Waktu Menurun:**
   - Halaman menampilkan garis waktu vertikal dengan titik penanda waktu (*bullet indicator*).
   - Pengkajian paling mutakhir (misal: shif pagi hari ini) tampil paling atas, diikuti pengkajian shif malam kemarin, hingga pengkajian awal masuk rawat di posisi paling bawah.
3. **Indikator Tren Parameter Klinis:**
   - Setiap kartu menampilkan 3 parameter utama:
     - **Skala Nyeri (NRS/Wong-Baker):** Menampilkan skor (misal: `3 / 10 (Nyeri Ringan)`) disertai badge tren:
       - `↓ Membaik dari 7` (hijau) bila skala turun.
       - `↑ Memburuk dari 2` (merah) bila skala naik.
       - `→ Stabil pada 4` (abu-abu) bila skala tetap sama.
     - **Risiko Jatuh (Morse / Humpty Dumpty):** Menampilkan skor dan kategori risiko disertai badge tren perbaikan/perburukan kategori risiko.
     - **Status Gizi (MST / Skrining Gizi):** Menampilkan kategori risiko gizi disertai badge tren.
4. **Atribusi Rekam Medis:**
   - Bagian bawah kartu menyajikan metadata perawat penilai, waktu pencatatan resmi, dan status dokumen (`FINAL`).

### 2.2 Alur Penyaringan Parameter Klinis (Filter)
1. Perawat ingin berfokus hanya pada perkembangan nyeri pasca-pemberian analgetik.
2. Perawat menekan chip filter **Skala Nyeri** pada `TimelineFilterBar`.
3. Lini masa secara dinamis hanya menampilkan simpul-simpul pengkajian yang memiliki pengukuran nyeri, menyembunyikan pengkajian yang tidak memuat parameter tersebut.
4. Perawat dapat berpindah ke filter **Risiko Jatuh**, **Status Gizi**, atau kembali ke **Semua Parameter**.

### 2.3 Alur Keadaan Khusus (Edge Cases & Safety Boundary)
1. **Kondisi Pasien Baru Belum Memiliki Pengkajian (Empty State):**
   - Layar menampilkan `ClinicalStateBoundary` dengan ikon dan teks ramah: *"Belum ada pengkajian untuk pasien ini. Mulai dengan membuat pengkajian awal untuk menetapkan baseline klinis."*
   - Disediakan tombol primer `[Buat Pengkajian]` yang langsung mengalihkan tab aktif ke section **Pengkajian** (`activeSection = "assessment"`).
2. **Kondisi Gangguan Jaringan / Server (Error State):**
   - Layar menampilkan pesan kegagalan: *"Lini masa pengkajian keperawatan tidak dapat dimuat."*
   - Disediakan tombol primer `[Coba Muat Ulang Lini Masa]` untuk melakukan refetch tanpa me-reload browser.
3. **Kondisi Master Kebijakan Belum Ditetapkan (`isPolicyMasterEmpty === true`):**
   - Penanda `ClinicalDeadlineBadge` di sebelah kanan filter bar menampilkan status netral: *"○ Batas Waktu Belum Ditetapkan — Pengkajian tetap dapat dilakukan"*.
   - Garis waktu tetap tampil normal, tidak terkunci, dan perawat tetap leluasa melakukan pendokumentasian (`VAL-KEP-17`).
4. **Kondisi Dokumen Memiliki Addendum Koreksi (`RWI-DEC-091`):**
   - Simpul lini masa ditandai dengan titik berstatus kuning (`warning`).
   - Kartu pengukuran menampilkan banner amber: `[KOREKSI #X]` beserta isi teks koreksi.
   - Terdapat penegasan tertulis: *"Nilai asli tetap tersimpan pada riwayat rekam medis"*.

---

## 3. Rincian Perubahan Kode

### 3.1 Komponen Pustaka `src/components/ui/clinical-workspace/`
- **`ClinicalTimeline.jsx`**: Kontainer garis waktu vertikal yang menjaga konsistensi alur visual lini masa klinis.
- **`ClinicalTimelineItem.jsx`**: Simpul individual lini masa dengan bulatan penanda status (`default`, `active`, `warning`, `danger`, `success`), garis vertikal penghubung (`isLast` awareness), serta penataan waktu dan judul.
- **`ClinicalDocumentMeta.jsx`**: Komponen ringkas penampil metadata atribusi rekam medis (nama penilai, peran/shift, tanggal/jam pencatatan, versi dokumen, dan catatan).
- **`clinical-workspace.module.css`**: Penambahan CSS module bagian 14 untuk kelas styling timeline (`.timelineRoot`, `.timelineItem`, `.timelineDot`, `.timelineTrackLine`, `.docMetaRoot`, dll.).
- **`index.js`**: Ekspor publik untuk ketiga komponen base baru.

### 3.2 Utilitas & Hook Domain
- **`inpatient-nursing-workspace-utils.js`**:
  - `computeMeasurementTrend(type, currentValue, previousValue)`: Fungsi penentu tren klinis komparatif (pertama, membaik, memburuk, stabil) untuk skor nyeri, risiko jatuh, dan status gizi.
  - `filterTimelineEntries(entries, activeFilter)`: Utilitas filter berbasis kategori parameter (`ALL`, `PAIN`, `FALL_RISK`, `NUTRITION`).
  - `formatTimelineDate(dateInput)`: Pemformatan tanggal & jam standar Bahasa Indonesia.
- **`use-patient-assessment-timeline.jsx`**:
  - Hook reaktif pengelola data lini masa via `patientAssessmentService.getPatientAssessmentTimeline` dan `getPatientAssessmentDueStatus`.
  - Mengurutkan entri secara kronologis menurun (DESC).
  - Menyematkan objek tren pada setiap entri berdasarkan perbandingan dengan entri sebelumnya dalam urutan waktu.
  - Mengisolasi penanganan status error, denied, loading, dan no-policy.

### 3.3 Komponen Section Domain Rawat Inap
- **`timeline-filter-bar.jsx`**: Bar navigasi filter interaktif terpadu dengan chip seleksi dan `ClinicalDeadlineBadge`.
- **`timeline-measurement-card.jsx`**: Kartu 3 metrik pengukuran klinis (Nyeri, Jatuh, Gizi) dengan visualisasi tren, banner addendum koreksi `[Koreksi #X]`, dan `ClinicalDocumentMeta`.
- **`assessment-timeline-section.jsx`**: Section utama lini masa yang memanfaatkan `ClinicalStateBoundary` untuk memisahkan 3 keadaan mutlak, merender filter bar, dan menyusun urutan kartu pada `ClinicalTimeline`.
- **`nursing-workspace-sections.jsx`**: Menggantikan import stub `TimelineSectionStub` dengan `AssessmentTimelineSection`.
- **`timeline-section-stub.jsx`**: Berkas stub dihapus sepenuhnya dari repositori.
- **`nursing-workspace.module.css`**: Penambahan kelas styling kartu pengukuran, badge tren (positif/negatif/netral), chip filter, dan banner addendum.

---

## 4. Bukti Verifikasi & Acceptance Criteria

### 4.1 Pemenuhan Acceptance Criteria Fungsional

| Kriteria | Deskripsi Kebutuhan | Status | Bukti Implementasi & Verifikasi |
| :--- | :--- | :---: | :--- |
| **AC-01** | Kronologis Menurun & Integritas Nilai Lama | ✅ **TERBUKTI** | Data disortir `new Date(b.recordedAt) - new Date(a.recordedAt)`. Nilai historis masa lalu tidak pernah ditimpa atau dihapus. Lulus uji `FE-RWI-053 AC-01`. |
| **AC-02** | Tren Parameter Nyeri, Jatuh, dan Gizi | ✅ **TERBUKTI** | Fungsi `computeMeasurementTrend` menghitung arah panah dan teks perbandingan terhadap titik ukur sebelumnya (turun = `↓ Membaik`, naik = `↑ Memburuk`, sama = `→ Stabil`). Lulus uji `FE-RWI-053 AC-01 & AC-02`. |
| **AC-03** | Diferensiasi Mutlak 3 Keadaan (Empty, Error, No Policy) | ✅ **TERBUKTI** | `ClinicalStateBoundary` memisahkan pesan ramah kosong (*Belum Ada Pengkajian*) dengan tombol buat asesmen, pesan error dengan tombol coba lagi, dan `isPolicyMasterEmpty === true` yang menampilkan penanda netral abu-abu tanpa memblokir sistem (`VAL-KEP-17`). Lulus uji `FE-RWI-053 AC-03`. |
| **AC-04** | Tanda Koreksi Resmi [Koreksi #X] | ✅ **TERBUKTI** | Entri yang memiliki addendum membawa badge amber `KOREKSI #X` dan kartu tetap mempertahankan rekaman asli sesuai `RWI-DEC-091`. Lulus uji `FE-RWI-053 AC-04`. |

### 4.2 Hasil Eksekusi Unit Test

Perintah eksekusi native runner Node.js:
```powershell
node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-nursing-*.test.mjs"
```

Hasil eksekusi:
```text
✔ FE-RWI-052: Seluruh berkas komponen assessment dan base workspace terpasang (10.7554ms)
✔ FE-RWI-052 AC-01 & Visual AC-01: Satu layar terpadu dengan tepat 7 kelompok isian (FE-KEP-02) (0.7677ms)
✔ FE-RWI-052: Kalkulasi kelengkapan 7 kelompok pengkajian (0.2429ms)
✔ FE-RWI-052 VAL-KEP-08: Deteksi bagian wajib yang belum lengkap sebelum finalisasi (0.1921ms)
✔ FE-RWI-052 VAL-KEP-11: Deteksi pencegahan pengkajian awal kedua (0.2451ms)
✔ FE-RWI-052 VAL-KEP-17 & AC-06: Penanda tenggat waktu kosong menampilkan 'Batas waktu belum ditetapkan' (2.7422ms)
✔ FE-RWI-052 VAL-KEP-12 & AC-02: Dialog koreksi addendum mewajibkan alasan min 5 karakter sebelum kirim (0.7333ms)
✔ FE-RWI-052 RWI-DEC-091 & AC-03 & AC-04: Dokumen selesai terkunci tanpa tombol sunting dan koreksi via addendum (1.0029ms)
✔ FE-RWI-053: Seluruh berkas komponen timeline, hook, dan base workspace terpasang (7.6339ms)
✔ FE-RWI-053: Stub timeline telah digantikan dan diekspor dengan benar (0.9736ms)
✔ FE-RWI-053 AC-01 & AC-02: Kalkulasi tren perkembangan parameter klinis (computeMeasurementTrend) (0.4706ms)
✔ FE-RWI-053 AC-01: Integritas rekam medis & urutan kronologis menurun (waktu terbaru di atas) (0.2045ms)
✔ FE-RWI-053: Filter entri lini masa per kategori (filterTimelineEntries) (0.7618ms)
✔ FE-RWI-053 AC-03 & VAL-KEP-17: Pemisahan tiga keadaan mutlak pada section timeline (1.9453ms)
✔ FE-RWI-053 AC-04 & RWI-DEC-091: Tanda koreksi resmi [Koreksi #X] pada entri addendum (0.6771ms)
✔ FE-RWI-053: Format tanggal Bahasa Indonesia (formatTimelineDate) (17.9715ms)
✔ FE-RWI-051: Seluruh file domain dan base clinical workspace terpasang (10.9733ms)
✔ FE-RWI-051 AC-06: Nol butir menu baru pada sidebar (IA-INP-05) (1.1705ms)
✔ FE-RWI-051 AC-01: Akses dalam <= 3 klik lewat Census dan Detail Episode (IA-INP-01) (1.2707ms)
✔ FE-RWI-051 AC-07: Navigasi internal 4 section persis (0.7691ms)
✔ FE-RWI-051 AC-03: Context failure mematikan seluruh izin tulis secara mutlak (0.1823ms)
✔ FE-RWI-051 AC-08: Quick Summary metrics terhitung dengan benar (0.217ms)

ℹ tests 22
ℹ suites 0
ℹ pass 22
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 164.6812
```

---

## 5. Dampak Traceability & Registri

1. **`frontend-roadmap.md`**:
   - Task `FE-RWI-053` diubah statusnya menjadi `[x] ✅ Selesai`.
2. **`requirement-traceability.md`**:
   - Pemenuhan `FR-KEP-007` (Lini masa pemantauan pengkajian kronologis menurun) diperbarui dari parsial menjadi selesai penuh (`FE-RWI-053` ✅, `BE-RWI-058` ✅).
   - Pemenuhan `FR-KEP-010` dan `FR-KEP-011` (Riwayat pengkajian dan pemantauan klinis terintegrasi) diperbarui dengan bukti frontend `FE-RWI-053` ✅.
3. **Integritas Konstitusi Quilvian:**
   - Tidak ada menu sidebar baru (`IA-INP-05`).
   - Tidak ada perubahan skema database atau penulisan kode backend.
   - Menggunakan token desain Quilvian murni tanpa hardcoded CSS variables maupun dark mode.
