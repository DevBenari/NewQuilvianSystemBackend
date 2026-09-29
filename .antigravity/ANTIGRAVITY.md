# Quilvian Antigravity Local Governance — Modernisasi Menu Keperawatan

Dokumen ini adalah instruksi operasional lokal untuk agen Google Antigravity yang bekerja pada modernisasi modul dan antarmuka **Quilvian Rawat Inap & Keperawatan**.

---

## 1. Misi & Tujuan Utama

Modernisasi per menu dirancang untuk:
1. **Mempercantik & Memperbaiki Tampilan (UI/UX)**:
   - Mengubah antarmuka form yang kaku menjadi desain modern, elegan, responsif, dan intuitif.
   - Menggunakan design token resmi Quilvian (warna status risiko, kartu parameter interaktif, radio chip, progress tracker, banner keselamatan klinis).
2. **Menyelaraskan dengan Logika Bisnis Operasional V1**:
   - `QuilvianV1` adalah cerminan operasional rumah sakit yang sudah teruji di lapangan.
   - Semua parameter medis, kelompok usia (Anak, Dewasa, Lansia), formula skoring, dan checklist intervensi dari V1 wajib diakomodasi 100% di `QuilvianFinal`.
3. **Menciptakan Inovasi Bisnis & Sistem Pelengkap**:
   - Melampaui V1 dengan inovasi cerdas: deteksi otomatis kelompok usia pasien, auto-scoring real-time, rekomendasi intervensi keselamatan otomatis (SKP 6), riwayat tren pengkajian per shift, dan integrasi rekam medis elektronik tanpa hambatan.

---

## 2. Empat Langkah Wajib Per Menu

Setiap menu atau sub-menu yang dibahas harus melalui 4 langkah baku:

```
[Tahap 1: Analisis Mendalam]
  └─ Audit form V1 vs Final → Klasifikasi: Sudah / Belum / Sebagian Diterapkan
         │
[Tahap 2: Laporan Teknis & Bisnis]
  └─ Endpoint Swagger, Flowchart Mermaid, Dampak Bisnis, Desain UI Baru, Inovasi
         │
[Tahap 3: Persistensi Dokumen Rencana Kerja]
  └─ Simpan di docs/module-blueprints/.../roadmap/rencana-kerja/[kategori]/[menu]/[name].md
         │  (Menunggu Persetujuan User)
         ▼
[Tahap 4: Implementasi Tuntas End-to-End]
  └─ Eksekusi penuh Backend + Frontend hingga tuntas dan terverifikasi 0 error
```

### Tahap 1: Analisis 1 Menu Secara Mendalam
- Mengkaji form operasional lapangan (dari capture screenshot atau source code `QuilvianV1`).
- Memeriksa source code `QuilvianFinal` (Backend: Entity, DTO, Seeder, Controller; Frontend: Component, State, API Hooks).
- Menentukan status komponen secara tegas dan berbasis bukti:
  - **`SUDAH DITERAPKAN`**: Sudah ada di Final dan bekerja penuh sesuai standar.
  - **`SEBAGIAN DITERAPKAN`**: Kerangka sudah ada tetapi butir parameter, skoring, atau validasi belum lengkap.
  - **`BELUM DITERAPKAN`**: Sama sekali belum ada di Final.

### Tahap 2: Pelaporan Endpoint Swagger, Bisnis Proses & Inovasi
- Menyajikan spesifikasi endpoint Swagger lengkap: Tag, HTTP Method, Route URL, Auth/Roles, Request Body, Response Schema, dan HTTP Status Code.
- Memaparkan alur proses bisnis rumah sakit secara terperinci dilengkapi skenario nyata pasien rawat inap.
- Menganalisis dampak perubahan terhadap efisiensi perawat, keselamatan pasien, dan kepatuhan akreditasi (KARS / SNARS / SKP).
- Menyusun diagram alur visual menggunakan **Mermaid Flowchart**.
- **Skema Tampilan UI/UX Modern (Wajib Ada)**: Menyertakan wireframe visual rancangan antarmuka, tata letak kartu/tabel, badge live score, status risiko adaptif, dan panel intervensi modern yang lebih unggul dari V1.
- Merancang inovasi sistem dan peningkatan fungsionalitas pendukung keselamatan pasien.

### Tahap 3: Penyimpanan Laporan Rencana Kerja
Seluruh hasil analisis wajib didokumentasikan di folder canonical:
`C:\Users\Admin\Documents\Quilvian\Source Code\QuilvianFinal\NewQuilvianSystemBackend\docs\module-blueprints\rawat-inap\keperawatan\roadmap\rencana-kerja\[kategori]/[menu]/[name].md`
Contoh:
- `asuhan-keperawatan/soap/soap.md`
- `asuhan-keperawatan/resiko-jatuh/resiko-jatuh.md`

### Tahap 4: Implementasi Tuntas Tanpa Bertahap
- Setelah laporan disajikan dan disetujui oleh pengguna, implementasi dijalankan secara tuntas (Full-Stack).
- Tidak dipecah menjadi task-task mikro yang bertele-tele; kerjakan backend dan frontend hingga selesai, lalu verifikasi dengan build test (`dotnet build --no-incremental`, `npm run build`).

---

## 3. Aturan Keselamatan & Repository

- `QuilvianV1` berstatus **READ-ONLY** (hanya dibaca sebagai bukti acuan operasional).
- Seluruh kode baru dan perbaikan hanya ditulis di **`QuilvianFinal`**.
- Git di backend hanya boleh dibaca (`status`, `log`, `diff`). Perintah yang mengubah git (`commit`, `push`, `merge`) diserahkan kepada user.
- Bahasa laporan dan komunikasi: **Bahasa Indonesia** baku, terstruktur, dan mudah dipahami tenaga medis maupun tim teknis.
