# Laporan Perubahan Frontend — `FE-RWI-254`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-254` |
| Judul | Banner serah terima klinis aktif dan alert non-blocking berkas PPRI pada Detail Episode Rawat Inap |
| Slice | Integrasi notifikasi serah terima pasien masuk dan pemantauan dokumen admisi PPRI |
| Roadmap | [`frontend-roadmap-transfer-pasien-masuk.md`](../../../roadmap/frontend-roadmap-transfer-pasien-masuk.md) — kartu `FE-RWI-254` |
| Trace | `FR-TRF-002`, `FR-TRF-007`; `TRF-AC-008`, `TRF-AC-009`; Kontrak 0.14.0 Transfer Handover Bagian 5.A; Kamus Data 23.3 |
| Contract version | `0.14.0` |
| Dependency | `BE-RWI-241` ✅, `BE-RWI-243` ✅ |
| Klasifikasi | `MEDIUM` — Integrasi komponen peringatan amber dan alert status non-blocking pada layar sentral rawat inap |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository untuk dokumen laporan dan roadmap |
| Target tulis | `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx`, `src/components/features/health-services/clinical-management/transfer-handover/transfer-handover-banner.jsx`, `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ Selesai — Seluruh acceptance criteria terbukti, `npm run lint:errors` exit 0, `node --test tests/unit/transfer-handover.test.mjs` lulus 3/3, `npm run build` lulus (exit 0) |

---

## 1. Keadaan yang Ditemukan di Awal

1. Pada layar Detail Episode Rawat Inap (`InpatientEpisodeDetailView.jsx`), komponen `TransferHandoverBanner` sudah diimpor tetapi kondisinya belum secara eksplisit mencocokkan status `Sent` transfer masuk antar-unit.
2. DTO respons detail episode dari backend yang membawa agregat `admissionDocumentsSummary` (`BE-RWI-243`) belum dipetakan ke dalam objek `episode` pada fungsi `normalizeEpisodeDetail` di frontend.
3. Belum ada tampilan `InformationAlert` non-blocking yang menginformasikan status pemrosesan dokumen di Workspace PPRI kepada perawat bangsal ketika berkas admisi belum selesai.

> **Skenario Sebelum:** Pasien yang dipindahkan dari IGD/Rajal ke bangsal tidak menampilkan banner transfer aktif secara konsisten, dan perawat tidak mengetahui bahwa dokumen admisi PPRI (seperti deposit atau surat selisih biaya) masih sedang diproses di admisi.

> **Skenario Sesudah:** Begitu transfer masuk disetujui, Detail Episode menampilkan banner amber dengan tombol `[Lihat Detail Serah Terima]` untuk membuka laci transfer. Selain itu, alert kuning non-blocking menginformasikan progres kelengkapan berkas PPRI (`{completed}/4` selesai) dengan tautan navigasi langsung ke Workspace PPRI tanpa mengganggu pelayanan medis pasien.

---

## 2. Alur Proses Bisnis & Perilaku Pengguna

| Unsur | Penjelasan |
| --- | --- |
| **Tujuan** | Memberikan visibilitas instan atas serah terima klinis transfer yang menunggu penerimaan dan progres berkas admisi PPRI secara transparan |
| **Pelaku** | Perawat Ruangan / Bangsal Rawat Inap, Dokter DPJP, Petugas Admisi |
| **Pemicu** | Pembukaan layar Detail Episode Rawat Inap (`/health-services/inpatient-management/episodes/[id]`) |

### Langkah dan Aturan Interaksi:
1. **Banner Serah Terima Transfer (`TransferHandoverBanner`):**
   - Sistem memeriksa daftar serah terima transfer aktif pada episode. Jika terdapat dokumen berstatus `Sent` (atau tertunda / belum diterima), banner amber muncul di bagian atas detail episode.
   - Banner menampilkan teks: *"Serah terima tertunda — Menunggu Penerimaan"* dan rincian unit asal.
   - Mengklik tombol `[Lihat Detail Serah Terima]` membuka `TransferHandoverDrawer`.
   - **Kondisi Negatif:** Jika tidak ada dokumen berstatus `Sent`, banner tidak ditampilkan di layar.
2. **Alert Kelengkapan Berkas PPRI Non-Blocking (`InformationAlert`):**
   - Sistem membaca metrik `admissionDocumentsSummary` dari data episode.
   - Jika `isAllCompleted === false`, alert kuning ramah pengguna tampil dengan pesan:
     *"Dokumen admisi di Workspace PPRI sedang diproses ({completedDocumentsCount}/{totalRequiredDocuments} selesai). Pelayanan dan perawatan medis di bangsal tetap berjalan penuh."*
   - Dilengkapi daftar nama dokumen yang masih pending dan tautan langsung `Buka Dokumen PPRI ->`.
   - **Kondisi Negatif:** Begitu seluruh berkas (4/4) selesai, alert otomatis hilang dari layar.

---

## 3. Rincian Perubahan Kode

### 3.1 Berkas yang Diperiksa & Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Menambahkan pemetaan properti `admissionDocumentsSummary` pada fungsi `normalizeEpisodeDetail` |
| `src/components/features/health-services/clinical-management/transfer-handover/transfer-handover-banner.jsx` | Memperbarui filter pencarian `pendingHandover` agar mencakup status `Sent` (angka 2 maupun string "Sent"), menyelaraskan pesan teks unit asal, dan memperbarui label tombol aksi menjadi *"Lihat Detail Serah Terima"* |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Menyisipkan `InformationAlert` varian `warning` untuk ringkasan berkas PPRI non-blocking lengkap dengan tautan ke Workspace PPRI |

### 3.2 Tabel Keputusan Base Component (Base Component Decision Gate)

| Elemen UI | Status Keputusan | Dasar Bukti & Alasan Rekomendasi |
| :--- | :---: | :--- |
| `InformationAlert` | `REUSE` | Menggunakan komponen dasar `@/components/features/base-features/information-alert` dengan varian `warning` dan ikon standar tanpa hardcoded styling |
| `BaseButton` | `REUSE` | Menggunakan tombol sekunder `@/components/features/base-features/base-button` untuk aksi pembukaan laci serah terima |
| `TransferHandoverBanner` | `EXTEND` | Memperluas komponen fitur yang sudah ada agar mendukung kontrak status `Sent` dan label tombol terkini |

---

## 4. Verifikasi Kriteria Penerimaan (Acceptance Criteria)

| Kode AC | Kriteria Penerimaan | Bukti Implementasi pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `TRF-AC-008` | Jika ada transfer handover berstatus `Sent`, banner amber tampil di bagian atas Detail Episode dengan tombol `[Lihat Detail Serah Terima]` | `transfer-handover-banner.jsx`: evaluasi `h.status === TRANSFER_HANDOVER_STATUS.SENT \|\| h.status === "Sent" \|\| h.status === 2` dan render tombol `Lihat Detail Serah Terima` | ✅ Terbukti |
| `TRF-AC-008` | Mengklik tombol banner membuka Laci Serah Terima Klinis (`TransferHandoverDrawer`) | `transfer-handover-banner.jsx`: `onClick={() => { setSelectedHandoverId(pendingHandover.id); setIsDrawerOpen(true); }}` | ✅ Terbukti |
| `TRF-AC-008` | Jika tidak ada handover `Sent`, banner serah terima tidak tampil | `transfer-handover-banner.jsx`: `if (!pendingHandover) return ...` | ✅ Terbukti |
| `TRF-AC-009` | Jika dokumen admisi PPRI belum lengkap, alert kuning menampilkan pesan proses non-blocking | `inpatient-episode-detail-view.jsx`: `InformationAlert` dengan pesan kelengkapan `{completed}/{total}` dan tautan Workspace PPRI | ✅ Terbukti |
| `TRF-AC-009` | Jika dokumen PPRI sudah 100% lengkap, alert kuning dokumen PPRI otomatis hilang | `inpatient-episode-detail-view.jsx`: kondisi `episode.admissionDocumentsSummary && !episode.admissionDocumentsSummary.isAllCompleted` | ✅ Terbukti |

---

## 5. Bukti Validasi Sistem (Build, Lint, Unit Test)

- **Unit Test Spesifik:**
  ```text
  node --test tests/unit/transfer-handover.test.mjs
  ✔ FE-RWI-200 AC-1: Service transfer-handover memuat seluruh kontrak API 11.8
  ✔ FE-RWI-200 AC-2: TransferHandoverDrawer memuat 9 bagian dokumen klinis dan kontrol wewenang
  ✔ FE-RWI-200 AC-3: TransferHandoverBanner terintegrasi di Detail Episode dan Keperawatan secara non-blocking
  ℹ pass 3, fail 0
  ```
- **Linter:**
  ```text
  npm run lint:errors
  Exit code: 0 (Bersih tanpa error ESLint)
  ```
- **Produksi Build:**
  ```text
  npm run build
  Exit code: 0 (Kompilasi Standalone Next.js Turbopack Berhasil)
  ```
