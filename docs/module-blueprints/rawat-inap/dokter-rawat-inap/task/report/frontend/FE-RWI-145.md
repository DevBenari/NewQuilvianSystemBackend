# Laporan Perubahan Frontend — `FE-RWI-145`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-145` |
| Judul | Form Permintaan Darah Lengkap (Bank Darah) & Konsultasi Asuhan Gizi |
| Gelombang | Rencana Kerja Penunjang Medis Gelombang 3 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-145` |
| Dokumen Rencana | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/rencana-kerja/penunjang-medis/penunjang-medis.md` (Rev 2.0 DISETUJUI) |
| Wewenang UI | `rencana-kerja/penunjang-medis/penunjang-medis.md` §7.1, §7.2; `frontend-roadmap-v2.md` kartu `FE-RWI-145` |
| Dependency | `FE-RWI-143` ✅ selesai |
| Klasifikasi | `HIGH` — Integrasi formulir pemesanan Bank Darah (`createBloodOrder`) dan Konsultasi Gizi (`createNutritionOrder`) beserta tabel riwayat pemesanan aktif |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repositori backend untuk dokumen roadmap & laporan task |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ `SELESAI`. Terverifikasi lewat unit test `FE-RWI-145` (PASS), integrasi controller backend resmi, dan tampilan tabel riwayat dengan `data-flat-table="true"` |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-145`:
1. Layanan Bank Darah dan Gizi / Konsultasi Gizi pada ruang kerja dokter menampilkan panel statis "Integrasi belum tersedia" (`RWI-DEC-108`) karena sebelumnya belum dihubungkan.
2. Dokter tidak memiliki sarana digital untuk memesan kantong darah (PRC, WB, TC, FFP, Cryo) maupun meminta uji cocok serasi (crossmatch) dari ruang kerja rawat inap.
3. Permintaan konsultasi asuhan gizi dan evaluasi diet pasien rawat inap masih harus dilakukan manual tanpa integrasi ke Instalasi Gizi rumah sakit.

---

## 2. Proses bisnis dari sisi pengguna

1. **Formulir Bank Darah RS (BDRS)**:
   - Dokter membuka kartu **Bank Darah** → sub-tab `[Formulir Pemesanan]`.
   - Dokter memilih Golongan Darah & Rhesus pasien (A, B, AB, O / Rh+ atau Rh-, atau opsi konfirmasi BDRS).
   - Dokter memilih Komponen Darah yang dibutuhkan (PRC, WB, TC, FFP, Cryoprecipitate).
   - Dokter menentukan Jumlah Kantong (Bag) dan Waktu Diperlukan Transfusi (jadwal tanggal dan jam).
   - Dokter menentukan Prioritas Permintaan: `⏳ Terencana / Rutin` vs `⚡ CITO / Darurat`.
   - Sistem secara otomatis mengunci konteks Ruang Rawat / Bangsal Pasien dan Diagnosa Kerja.
   - Dokter mengisi Indikasi Klinis & Kadar Hb Terakhir sebelum menekan tombol `[Kirim Permintaan Darah ke BDRS]`.
   - Tiket CPOE darah terkirim ke modul `BloodBankManagement` melalui `createBloodOrder`.
2. **Formulir Konsultasi Asuhan Gizi**:
   - Dokter membuka kartu **Gizi** → sub-tab `[Formulir Pemesanan]`.
   - Dokter memilih Jenis Permintaan Asuhan Gizi: Konsultasi Gizi Klinis, Penentuan/Evaluasi Diet Pasien, Terapi Nutrisi Enteral/Parenteral, atau Edukasi Diet Khusus.
   - Dokter menentukan Prioritas: `⏳ Rutin` vs `⚡ Urgent / Kritis`.
   - Sistem otomatis menarik Diagnosa Kerja aktif dan daftar Riwayat Alergi Makanan/Obat pasien dari rekam medis.
   - Dokter mengisi Alasan Rujukan & Rencana Asuhan Nutrisi lalu menekan `[Kirim Permintaan Konsultasi Gizi]`.
   - Tiket permintaan terkirim ke modul `NutritionManagement` melalui `createNutritionOrder`.
3. **Sub-Tab Riwayat & Hasil Pemeriksaan**:
   - Menampilkan riwayat seluruh permintaan darah dan asuhan gizi yang sudah dibuat selama episode rawat inap berjalan, lengkap dengan status pengerjaan, prioritas, dan tombol pembuatan pesanan baru.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-blood-bank-form.jsx` | Baru | Komponen formulir permintaan darah lengkap: golongan darah, rhesus, komponen darah, jumlah kantong, waktu kebutuhan, dan pengiriman ke `createBloodOrder` |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-nutrition-form.jsx` | Baru | Komponen formulir konsultasi gizi: jenis asuhan, prioritas, auto-sync diagnosa & riwayat alergi, dan pengiriman ke `createNutritionOrder` |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-history-section.jsx` | Baru | Komponen tabel riwayat dinamis untuk Gizi, Bank Darah, dan Rehab Medik menggunakan `ClinicalDataTable` dan badge status terpadu |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx` | Ubah | Mengintegrasikan pemanggilan `getNutritionOrders`, `getBloodOrders`, `orderNutrition`, dan `orderBloodBank` |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | Ubah | Mengintegrasikan form dan riwayat Bank Darah serta Gizi ke dalam container sub-view |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Formulir Bank Darah & Gizi** | `BaseInputField`, `BaseNativeSelectField`, `BaseTextAreaField` | `REUSE` | Kontrol form standar dengan validasi kelayakan klinis |
| **Tabel Riwayat Asuhan & Darah** | `ClinicalDataTable` | `REUSE` | Menampilkan baris data responsif dengan atribut `data-flat-table="true"` |
| **Status Pemrosesan BDRS / Gizi** | `ClinicalStatusBadge` | `REUSE` | Menampilkan status pesanan (`Diproses`, `Terpenuhi`, `Aktif`, `Selesai`) |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` yang melanggar governance.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Form Bank Darah tersimpan ke backend | `SupportingBloodBankForm` memvalidasi input dan memanggil `orderBloodBank`, payload tersimpan ke `BbkBloodOrderController` |
| **AC-2** | Form Gizi tersimpan ke backend | `SupportingNutritionForm` memanggil `orderNutrition`, payload tersimpan ke `NutritionOrderController` |
| **AC-3** | Riwayat pesanan tampil di sub-tab Riwayat | `SupportingHistorySection` menampilkan kolom spesifik untuk Bank Darah (kantong, komponen, crossmatch) dan Gizi (jenis asuhan, diet, status) |

---

## 5. Bukti verifikasi & eksekusi

- **Linting**:
  `npx eslint src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/`
  Hasil: **0 errors, 0 warnings (PASS)**.
- **Automated Test**:
  `node tests/unit/inpatient-supporting-service-modernisasi.test.mjs`
  Hasil: **Test FE-RWI-145 PASS**.
- **Manual Test**:
  Uji coba pengiriman formulir permintaan darah PRC 2 kantong CITO dan konsultasi gizi rawat inap: **PASS**.
