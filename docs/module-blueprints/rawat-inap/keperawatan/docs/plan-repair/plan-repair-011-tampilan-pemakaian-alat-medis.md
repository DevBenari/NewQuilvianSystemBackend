# PLAN-REPAIR-011 — Penyelarasan Navigasi Tab Tunggal, Base Components, dan Modernisasi Antarmuka Pemakaian Alat Medis

```yaml
plan_id: PLAN-REPAIR-KEP-011
issue: ../issue/issue-011-tampilan-pemakaian-alat-medis.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pengguna (Perintah Langsung: perbaiki tampilan pemakaian alat medis)"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-KEP-011-01` | `ISS-KEP-011-01` | Eliminasi bilah tab duplikat internal dan sinkronisasi reaktif prop `activeTab` dari bilah tab utama | Frontend | 1 | `FE-RWI-188` | ✅ SELESAI | `nursing-equipment-section.jsx`, unit test PASS |
| `FIX-KEP-011-02` | `ISS-KEP-011-02` | Perancangan ulang panel pemantauan alat berjalan: live badge, empty state klinis, dan kartu durasi pemakaian | Frontend | 1 | `FE-RWI-188` | ✅ SELESAI | `nursing-equipment-section.jsx`, styling domain terpadu |
| `FIX-KEP-011-03` | `ISS-KEP-011-03` | Penerapan Base Components (`BaseButton`, `ClinicalSafetyAlert`, `InformationAlert`) pada formulir mulai pemakaian | Frontend | 2 | `FE-RWI-188` | ✅ SELESAI | `nursing-equipment-section.jsx`, konsistensi UI checklist |
| `FIX-KEP-011-04` | `ISS-KEP-011-T1` | Migrasi dialog Selesai, Batal, dan Koreksi Waktu ke `react-bootstrap/Modal` resmi dengan penegakan `CLI-EQP-002` | Frontend | 2 | `FE-RWI-188` | ✅ SELESAI | `nursing-equipment-section.jsx`, dialog modal standar |
| `FIX-KEP-011-05` | `ISS-KEP-011-T2` | Perbaikan pemanggilan data awal guna mengeliminasi warning linter `react-hooks/set-state-in-effect` | Frontend | 3 | `FE-RWI-188` | ✅ SELESAI | ESLint 0 error 0 warning |

**Ringkasan: 5 dari 5 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-011-01 — Bilah Tab Ganda dan Desinkronisasi State

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Hapus `<div className="nav nav-tabs">` lokal di dalam seksi dan jadikan `activeTab` prop sebagai pengendali tunggal tampilan (`const currentTab = activeTab || "order"`). | Bilah tab bersih, satu pintu dari `NursingSecondaryTabBar`, bebas redundansi visual, dan sinkron 100% dengan URL query. | Menghilangkan tombol subtab lokal. |
| B | Pertahankan kedua bilah tab namun tambahkan `useEffect` dua arah untuk menyamakan state. | Mempertahankan tombol lama. | Layar tetap sesak dan janggal karena dua baris menu yang sama persis bertumpuk vertikal. |

**Solusi terpilih: Opsi A.**
Sesuai arsitektur kanonikal `NursingProcedureSection` dan `NursingAncillarySection` yang meniadakan duplikasi navigasi lokal.

---

### ISS-KEP-011-02 — Panel Pemantauan Alat Berjalan Kurang Informatif

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Hadirkan kartu modern dengan header berbadge jumlah alat aktif, *empty state* klinis lengkap dengan instruksi saat kosong, serta kartu alat aktif berdurasi berjalan dan tombol aksi terstruktur. | Informasi sangat jelas bagi perawat, penegasan durasi pemakaian terukur, dan aksi klinis mudah dijangkau. | Memerlukan sedikit penambahan markup UI kartu. |
| B | Biarkan tabel minimalis tanpa kartu status. | Kode lebih pendek. | Tidak informatif dan visual tidak selaras standar Quilvian. |

**Solusi terpilih: Opsi A.**

---

### ISS-KEP-011-03 — Formulir Belum Mengadopsi Base Components

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Terapkan `BaseButton`, `ClinicalSafetyAlert`, `InformationAlert`, serta penataan field dengan helper text dan preview satuan tagih alat (Per Jam / Per Pemakaian / Per Hari). | Tampilan elegan, aman secara klinis, dan patuh 100% pada katalog komponen Quilvian. | Memerlukan import komponen base. |
| B | Pertahankan tombol dan alert bootstrap murni. | Tidak perlu ubah import. | Melanggar aturan tata kelola frontend Quilvian. |

**Solusi terpilih: Opsi A.**

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan Sebelum (Bilah Tab Bertumpuk & Tampilan Kasar)

```text
+-----------------------------------------------------------------------------------------+
| [ Order Alat Kesehatan ] (Aktif)          [ History Alat Kesehatan ]   <- Tab Bar Atas  |
+-----------------------------------------------------------------------------------------+
| [🔧] Pemakaian Alat Medis                                                  [🔄 Segarkan] |
| Pencatatan pemakaian alat medis besar (Ventilator, Infus Pump...).                      |
+-----------------------------------------------------------------------------------------+
| ▶ Order Alat Kesehatan (Aktif)   ↶ History Alat Kesehatan (0)          <- Tab Bar Bawah |
+-----------------------------------------------------------------------------------------+
| [Mulai Pemakaian Alat Medis]           | [Alat Medis Sedang Berjalan]                   |
| Jenis Alat Medis *                     |                                                |
| [-- Pilih Jenis Alat Aktif -- ▾]       | Tidak ada alat medis yang sedang berjalan...   |
| DPJP *                                 |                                                |
| [dr. Bagus Purnama Sanjaya     ▾]       |                                                |
| Waktu Mulai *                          |                                                |
| [ 06/10/2026 10:27 ]                   |                                                |
| Catatan Tambahan                       |                                                |
| [ Indikasi klinis, nomor seri...     ] |                                                |
| [       Mulai Pemakaian       ]        |                                                |
+----------------------------------------+------------------------------------------------+
```

### Tampilan Sesudah (Navigasi Bersih Satu Pintu & Ergonomi Premium)

```text
+-----------------------------------------------------------------------------------------+
| [ Order Alat Kesehatan ] (Aktif)          [ History Alat Kesehatan ]   <- Tab Bar Tunggal|
+-----------------------------------------------------------------------------------------+
| [🔧] Pemakaian Alat Medis                                 [🟢 1 Alat Aktif] [🔄 Segarkan]|
| Pencatatan pemakaian alat medis besar (Ventilator, Infus Pump, Syringe Pump).            |
| Otomatis tertagih ke invoice Rawat Inap tanpa menampilkan nominal rupiah (RWI-DEC-219). |
+-----------------------------------------------------------------------------------------+
| [ Mulai Pemakaian Alat Medis ]          | [ Alat Medis Sedang Berjalan (1) ]            |
|                                         |                                                |
| Jenis Alat Medis *                      | +--------------------------------------------+ |
| [ Syringe Pump B.Braun #04 (Per Jam) ▾] | | [⚡] Syringe Pump B.Braun #04   [ Running ] | |
|                                         | | DPJP: dr. Bagus Purnama Sanjaya, Sp.JP     | |
| Dokter Penanggung Jawab *               | | Pelaksana: Ns. Siti Aminah                 | |
| [ dr. Bagus Purnama Sanjaya (Dokter) ▾] | | Mulai: 06 Okt 2026, 08:30 (2 jam lalu)    | |
| Hanya dokter penugasan aktif yg dipilih | | Catatan: Terapi titrasi nicardipine        | |
|                                         | |                                            | |
| Waktu Mulai Pemasangan *                | | [✏️ Koreksi]  [❌ Batal]  [⏹️ Selesai]     | |
| [ 06/10/2026 10:27 ]                    | +--------------------------------------------+ |
|                                         |                                                |
| Catatan / Nomor Seri / Bed              | Saat kosong:                                   |
| [ Indikasi klinis alat medis...       ] | [🛡️] Tidak Ada Alat Medis Aktif               |
|                                         | Saat ini tidak ada alat medis besar terpasang. |
| [ℹ️ Tarif dihitung otomatis oleh server]|                                                |
| [ ▶  Mulai Pemakaian Alat Medis       ] |                                                |
+-----------------------------------------+------------------------------------------------+
```

---

## 4. Rincian Perbaikan

### FIX-KEP-011-01 — Penghapusan Bilah Tab Duplikat
- Menghapus elemen `<div className="nav nav-tabs mb-4">` lokal dari `nursing-equipment-section.jsx`.
- Menggunakan `currentTab = activeTab || "order"` secara langsung dari prop navigasi induk `NursingSecondaryTabBar`.

### FIX-KEP-011-02 — Modernisasi Panel Pemantauan Alat Berjalan
- Menambahkan counter badge pada kartu kanan (`Alat Medis Sedang Berjalan`).
- Menambahkan kartu *empty state* dengan ikon `FaCheckCircle` dan teks panduan ketika tidak ada alat yang berjalan.
- Menata ulang kartu pemakaian aktif dengan badge status, metadata dokter dan perawat, catatan, serta tombol aksi `BaseButton` ukuran kompak.

### FIX-KEP-011-03 — Penyelarasan Form dengan Base Components
- Mengganti tombol submit dengan `BaseButton variant="primary"` berukuran penuh.
- Mengganti notifikasi error dan sukses dengan `ClinicalSafetyAlert`.
- Menambahkan `InformationAlert` terkait transparansi penghitungan durasi dan kepatuhan privasi tarif non-rupiah (`RWI-DEC-219`).

### FIX-KEP-011-04 — Migrasi Dialog Modal ke React Bootstrap
- Modal Selesai Pemakaian, Pembatalan (`CLI-EQP-002`), dan Koreksi Waktu dimigrasikan ke komponen `Modal` dari `react-bootstrap`.
- Ditambahkan ringkasan konteks alat yang sedang diproses pada badan modal dan tombol aksi `BaseButton`.

### FIX-KEP-011-05 — Pembersihan Siklus Hidup dan Peringatan Linter
- Mengatur pemanggilan data awal dengan pola asinkron yang aman guna mencegah peringatan `react-hooks/set-state-in-effect`.

---

## 5. Verifikasi dan Pengujian

1. **Unit Test Regresi Finishing Roadmap (`inpatient-nursing-finishing-roadmap.test.mjs`):**
   - Tes `FE-RWI-188: Pemakaian Alat: Order & History tabs tanpa rupiah` wajib lolos (memastikan keberadaan `CLI-EQP-002`, `tarif belum ada`, dan `perlu diperiksa perawat`).
2. **Pemeriksaan Linter:**
   - 0 error, 0 warning pada `nursing-equipment-section.jsx`.
3. **Integritas Navigasi:**
   - Tab "Order Alat Kesehatan" dan "History Alat Kesehatan" berpindah mulus ketika diklik dari bilah menu horizontal atas tanpa sisa bilah tab lokal ganda.

---

## 6. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Rencana perbaikan disusun dan dieksekusi tuntas; bilah tab duplikat dihilangkan dan panel pemantauan alat medis dimodernisasi. Status: SELESAI. | Antigravity Builder |
