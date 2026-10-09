# Laporan Perubahan Frontend — `FE-RWI-146`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-146` |
| Judul | Form Permintaan Hemodialisa Terpadu & Sesi Fisioterapi / Rehabilitasi Medik |
| Gelombang | Rencana Kerja Penunjang Medis Gelombang 3 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-146` |
| Dokumen Rencana | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/rencana-kerja/penunjang-medis/penunjang-medis.md` (Rev 2.0 DISETUJUI) |
| Wewenang UI | `rencana-kerja/penunjang-medis/penunjang-medis.md` §7.1, §7.2; `frontend-roadmap-v2.md` kartu `FE-RWI-146` |
| Dependency | `FE-RWI-143` ✅ selesai |
| Klasifikasi | `HIGH` — Integrasi formulir permintaan Hemodialisa inline terpadu (`createHmdOrder`) dan formulir sesi Fisioterapi / Rehab Medik (`createInpatientProcedureOrder`) menyelesaikan seluruh 6 layanan penunjang aktif |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repositori backend untuk dokumen roadmap & laporan task |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ `SELESAI`. Terverifikasi lewat unit test `FE-RWI-146` (PASS), eslint 0 warning, seluruh 6 layanan penunjang terhubung penuh 100% |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-146`:
1. Layanan Rehabilitasi Medik sebelumnya berstatus "Integrasi belum tersedia" tanpa formulir dan tanpa akses pemesanan sesi fisioterapi rawat inap.
2. Layanan Hemodialisa hanya memiliki modal pop-up pemesanan tanpa formulir inline terpadu di dalam sub-view, sehingga tidak konsisten dengan pola 2-subtab layanan lainnya.
3. Keterhubungan 6 layanan penunjang medis pada tab ruang kerja dokter rawat inap belum tuntas sepenuhnya.

---

## 2. Proses bisnis dari sisi pengguna

1. **Formulir Permintaan Hemodialisa Terpadu**:
   - Dokter membuka kartu **Hemodialisa** → sub-tab `[Formulir Pemesanan]`.
   - Dokter menentukan Prioritas Urgensi: `⏳ Rutin / Terjadwal` vs `⚡ CITO / Darurat`.
   - Dokter memilih Akses Vaskular pasien (AV Shunt, CDL / Double Lumen, Femoral).
   - Dokter menentukan Tanggal Rencana Sesi HD.
   - Tersedia tombol cepat **Preset Indikasi Klinis HD** (Uremia berat, Hiperkalemia K > 6.5, Asidosis metabolik refrakter, Overload cairan / Edema paru) yang dapat diklik untuk mengisi teks otomatis tanpa mengetik ulang.
   - Dokter menambahkan Instruksi Khusus Pengantar Bangsal (kebutuhan transfer brankar, isolasi, O2 portable).
   - Dokter menekan tombol `[Kirim Permintaan Hemodialisa]`, tiket terkirim ke `HmdOrderController` melalui `orderHemodialysis`.
2. **Formulir Sesi Fisioterapi / Rehabilitasi Medik**:
   - Dokter membuka kartu **Rehabilitasi Medik** → sub-tab `[Formulir Pemesanan]`.
   - Dokter memilih Tindakan Terapi / Fisioterapi: Fisioterapi Dada & Latihan Batuk Efektif (Chest Physio), Mobilisasi Bertahap Pasca Stroke, Latihan ROM Aktif/Pasif, Terapi Wicara & Evaluasi Menelan, Terapi Okupasi Kemandirian ADL, atau Terapi Modalitas Fisik (TENS / US).
   - Dokter menentukan Rencana Jumlah Sesi Terapi (1 s.d. 20 sesi).
   - Dokter menentukan Sifat Permintaan: `⏳ Rutin` vs `⚡ CITO / Segera`.
   - Sistem menarik Diagnosa Kerja aktif secara otomatis dari catatan SOAP pasien.
   - Dokter mengisi Catatan Disposisi & Instruksi Khusus ke Terapis (tujuan terapi, toleransi hemodinamik, kontraindikasi gerak).
   - Dokter menekan tombol `[Kirim Permintaan Fisioterapi]`, tiket terkirim ke `PatientProcedureController` melalui `orderRehab`.
3. **Penyelesaian Penuh**:
   - Seluruh 6 layanan penunjang medis kini memiliki form pemesanan aktif dan tabel riwayat pemesanan live. Nol layanan tertinggal dan nol blocker integrasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-hemodialysis-form.jsx` | Baru | Komponen formulir inline permintaan Hemodialisa: prioritas Cito/Rutin, akses vaskular, preset indikasi klinis, instruksi bangsal, dan submit ke `createHmdOrder` |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-rehab-form.jsx` | Baru | Komponen formulir permintaan fisioterapi & rehabilitasi medik: pilihan tindakan fisioterapi, jumlah sesi, urgensi Cito, auto-sync diagnosa, dan submit ke `createInpatientProcedureOrder` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx` | Ubah | Mengintegrasikan pemanggilan `getPatientProceduresByEpisode`, mutasi `orderRehab`, dan sinkronisasi status pengerjaan |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | Ubah | Mengintegrasikan form Hemodialisa dan Rehab Medik ke dalam sub-view shell container; menyempurnakan alur 6 layanan |
| `tests/unit/inpatient-supporting-service-modernisasi.test.mjs` | Ubah | Menambahkan unit test spesifik untuk `FE-RWI-146` memvalidasi keterhubungan HD dan Rehab |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Formulir Hemodialisa & Rehab** | `BaseInputField`, `BaseNativeSelectField`, `BaseTextAreaField` | `REUSE` | Kontrol form terstandarisasi dengan penanganan validasi dan batas input |
| **Preset Indikasi Cepat** | Tag tombol interaktif (`styles.icdTag`) | `COMPOSE` | Merangkai preset teks cepat untuk efisiensi waktu penginputan dokter |
| **Aksi Tombol Simpan CPOE** | `BaseButton` (`variant="primary"`, `size="sm"`) | `REUSE` | Tombol primer dengan proteksi guard kewenangan dokter |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` yang melanggar governance.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Form Hemodialisa tersimpan | `SupportingHemodialysisForm` memvalidasi alasan klinis dan memanggil `orderHemodialysis`, tersimpan ke Unit HD |
| **AC-2** | Form Rehab Medik tersimpan | `SupportingRehabForm` memvalidasi catatan disposisi dan memanggil `orderRehab`, tersimpan ke `PatientProcedureController` |
| **AC-3** | Seluruh 6 layanan berstatus terhubung penuh | Seluruh 6 layanan (`laboratory`, `radiology`, `nutrition`, `rehab`, `hemodialysis`, `blood-bank`) memiliki formulir pemesanan aktif dan riwayat |

---

## 5. Bukti verifikasi & eksekusi

- **Linting**:
  `npx eslint src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/`
  Hasil: **0 errors, 0 warnings (PASS)**.
- **Automated Test**:
  `node tests/unit/inpatient-supporting-service-modernisasi.test.mjs`
  Hasil: **Test FE-RWI-146 PASS**.
- **Manual Test**:
  Uji coba pengiriman permintaan cuci darah CITO dan permohonan 5 sesi fisioterapi dada (chest physio): **PASS**.
