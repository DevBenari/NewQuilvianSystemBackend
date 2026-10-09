# Laporan Perubahan Frontend — `FE-RWI-144`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-144` |
| Judul | Form & Cart Interaktif Laboratorium dan Radiologi (Auto-Sync SOAP & ICD-10) |
| Gelombang | Rencana Kerja Penunjang Medis Gelombang 2 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-144` |
| Dokumen Rencana | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/rencana-kerja/penunjang-medis/penunjang-medis.md` (Rev 2.0 DISETUJUI) |
| Wewenang UI | `rencana-kerja/penunjang-medis/penunjang-medis.md` §7.1B, §7.2; `frontend-roadmap-v2.md` kartu `FE-RWI-144` |
| Dependency | `FE-RWI-143` ✅ selesai |
| Klasifikasi | `HIGH` — Pembuatan komponen formulir split-view 2 kolom (katalog live search + cart interaktif) untuk Laboratorium dan Radiologi dengan auto-sync SOAP & ICD-10 search |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repositori backend untuk dokumen roadmap & laporan task |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ `SELESAI`. Terverifikasi lewat unit test `FE-RWI-144` (PASS), eslint 0 warning, pencegahan duplikasi item, dan akumulasi tarif real-time |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-144`:
1. Pemesanan Laboratorium dan Radiologi hanya menggunakan modal pop-up kecil sederhana dengan satu dropdown pilihan pemeriksaan tunggal.
2. Dokter tidak bisa memilih beberapa pemeriksaan sekaligus dalam satu pengajuan (tidak ada konsep keranjang pemesanan / cart).
3. Dokter harus mengetik ulang alasan klinis secara manual tanpa sinkronisasi diagnosa kerja dari catatan SOAP terakhir.
4. Tidak ada pencarian kode ICD-10 terstandarisasi untuk rujukan CPOE, dan tidak ada akumulasi estimasi tarif tindakan.

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka sub-tab `[Formulir Pemesanan]` pada layanan **Laboratorium** atau **Radiologi**.
2. **Auto-Sync SOAP & Diagnosa Terpilih**:
   - Sistem secara otomatis menarik diagnosa kerja aktif pasien dari modul SOAP (`workingDiagnosis`) dan menampilkannya sebagai tag diagnosa utama bertanda `(SOAP)`.
   - Dokter dapat mencari kode diagnosa ICD-10 tambahan dari master data rumah sakit (`MstDiagnosis`) melalui kotak pencarian live search yang responsif.
3. **Pemilihan Disiplin / Modalitas & Prioritas Urgensi**:
   - Untuk Lab: dokter memilih Disiplin (Patologi Klinik, Anatomi, Mikrobiologi, Parasitologi).
   - Untuk Rad: dokter memilih Modalitas (X-Ray, USG, CT-Scan, MRI).
   - Sakelar Prioritas: Dokter dapat memilih antara `⏳ Rutin` atau `⚡ CITO / Segera`. Bila CITO dipilih, banner peringatan keselamatan merah otomatis aktif.
4. **Split-View Katalog & Keranjang (Cart UI)**:
   - **Kolom Kiri (55%)**: Katalog pemeriksaan menampilkan daftar tes dengan kode, nama pemeriksaan, estimasi tarif, dan tombol `[+ Tambah]`. Terdapat kotak pencarian live filter nama/kode tes serta filter kategori. Item yang sudah dipilih berubah menjadi `[✓ Dipilih]` dan dinonaktifkan untuk mencegah duplikasi.
   - **Kolom Kanan (45%)**: Keranjang terpilih menampilkan daftar item yang dipilih, rincian biaya per item, tombol `[Hapus]`, akumulasi total estimasi biaya (Subtotal), textarea catatan klinis/indikasi, dan tombol aksi `[Simpan Permintaan CPOE]`.
5. Saat dokter menekan tombol Simpan, sistem memvalidasi kelengkapan data, mengirimkan tiket CPOE ke backend melalui `orderLab` / `orderRadiology`, menampilkan notifikasi sukses hijau, dan otomatis memindahkan layar ke sub-tab `[Riwayat & Hasil Pemeriksaan]`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-laboratory-form.jsx` | Baru | Komponen formulir Laboratorium split-view: auto-sync SOAP, pencarian ICD-10 live, katalog tes lab, cart dengan perhitungan harga real-time, dan pengiriman CPOE |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-radiology-form.jsx` | Baru | Komponen formulir Radiologi split-view: pemilihan modalitas, auto-sync SOAP & ICD-10, katalog tindakan radiologi, cart interaktif, dan submit CPOE |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | Ubah | Mengintegrasikan `SupportingLaboratoryForm` dan `SupportingRadiologyForm` ke dalam sub-view shell container |
| `src/style/health-services/inpatient-management/physician-supporting-service.module.css` | Ubah | Menambahkan token styling untuk layout split-view, kartu katalog, panel keranjang terpilih, tag ICD-10, dan sakelar urgensi Rutin/Cito |
| `tests/unit/inpatient-supporting-service-modernisasi.test.mjs` | Ubah | Menambahkan unit test spesifik untuk `FE-RWI-144` memvalidasi split-view, cart, dan integrasi diagnosa |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Input Pencarian & Teks** | `BaseInputField`, `BaseTextAreaField` | `REUSE` | Input standar teraksesibel dengan penanganan label dan error terpadu |
| **Dropdown Pilihan Disiplin / Modalitas** | `BaseNativeSelectField` | `REUSE` | Kontrol dropdown asli browser yang cepat dan konsisten di seluruh perangkat |
| **Banner Peringatan CITO & Validasi** | `ClinicalSafetyAlert` | `REUSE` | Menampilkan peringatan kegawatan klinis bertone `danger` |
| **Keranjang Pemesanan (Cart Panel)** | `BaseButton`, `ClinicalStatusBadge` | `COMPOSE` | Dirangkai menggunakan card surface dan button action standar Quilvian |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` yang melanggar governance.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Auto-sync diagnosa SOAP & ICD-10 | Diagnosa ditarik otomatis dari `workingDiagnosis` context; pencarian tambahan memanggil `/v1/health-services/master-data/diagnoses/options` |
| **AC-2** | Split-view katalog & cart berfungsi | `splitViewContainer` membagi layar 55% katalog dan 45% keranjang terpilih; item dapat ditambah dan dihapus |
| **AC-3** | Estimasi tarif terhitung | `subtotalPrice` menjumlahkan harga seluruh item di keranjang secara real-time |
| **AC-4** | Simpan order menerbitkan CPOE ke backend dan berpindah ke sub-tab Riwayat | Submit memanggil `orderLab` / `orderRadiology` lalu memicu `onSuccess()` yang memindahkan `activeSubTab` ke `"history"` |

---

## 5. Bukti verifikasi & eksekusi

- **Linting**:
  `npx eslint src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/`
  Hasil: **0 errors, 0 warnings (PASS)**.
- **Automated Test**:
  `node tests/unit/inpatient-supporting-service-modernisasi.test.mjs`
  Hasil: **Test FE-RWI-144 PASS**.
- **Manual Test**:
  Uji coba penambahan pemeriksaan dari katalog ke cart, verifikasi pencegahan duplikasi item, perhitungan subtotal tarif, dan sakelar CITO: **PASS**.
