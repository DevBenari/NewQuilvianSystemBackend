# PLAN-REPAIR-002 — Perbaikan Tata Letak dan Komponen Modal Formulir Permintaan Hemodialisa Rawat Inap

```yaml
plan_id: PLAN-REPAIR-HMD-002
issue: ../issue/issue-002-tampilan-modal-permintaan-hemodialisa.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pengguna (Perintah Langsung: perbaiki tampilan ya pada modal ini)"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-HMD-002-01` | `ISS-HMD-002-01`, `ISS-HMD-002-02`, `ISS-HMD-002-T1` | Migrasi pembungkus dari `ConfirmModal` ke `react-bootstrap/Modal` standar berukuran `lg`, pemulihan header klinis rata kiri dengan ikon tematik, dan grid 2 kolom yang lapang | Frontend | 1 | `FE-HMD-07` | ✅ SELESAI | `hemodialysis-order-modal.jsx`, unit test 7/7 PASS |
| `FIX-HMD-002-02` | `ISS-HMD-002-03` | Penataan tombol pilihan cepat indikasi klinis HD sebagai chip/tag yang mengalir rapi dalam 2–3 baris horizontal tanpa mendominasi ruang vertikal | Frontend | 2 | `FE-HMD-07` | ✅ SELESAI | `hemodialysis-order-modal.jsx`, unit test 7/7 PASS |

**Ringkasan: 2 dari 2 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-HMD-002-01 & ISS-HMD-002-02 & ISS-HMD-002-T1 — Ukuran Modal Sempit (430px), Header Terpusat Peringatan, dan Form Terpotong

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Ganti `ConfirmModal` dengan `Modal` (`react-bootstrap`) ukuran `size="lg"` (~800px), gunakan `Modal.Header closeButton`, `Modal.Body`, dan `Modal.Footer` terpadu dengan `BaseButton`. | Menghilangkan batasan lebar 430px secara tuntas, header rata kiri profesional klinis, ruang lapang untuk grid isian 2 kolom, dan konsisten dengan modal penunjang lain (`SupportingResultDetailModal`). | Perlu merefaktor struktur JSX pembungkus di `hemodialysis-order-modal.jsx`. |
| B | Pertahankan `ConfirmModal` lalu tambahkan style override khusus CSS `!important` untuk menimpa lebar `.region-modal`. | Struktur JSX tidak banyak berubah. | Merusak semantik komponen; `ConfirmModal` tetap memunculkan ikon bulat `(i)` di tengah dan judul terpusat yang tidak ergonomis untuk formulir klinis. |

**Solusi terpilih: Opsi A.**
1. Mengembalikan semantik komponen yang tepat: `ConfirmModal` hanya untuk kotak dialog tanya/konfirmasi singkat, sedangkan `Modal` standar untuk formulir entri data klinis multi-bagian.
2. Memberikan ruang horizontal ~800px yang cukup untuk menampilkan ringkasan konteks pasien, sakelar urgensi, tanggal tindakan berdampingan dengan akses vaskular, dan textarea indikasi klinis tanpa terpotong di viewport standar (1366x768 / 1920x1080).

**Kenapa bukan opsi B:** Opsi B adalah hack CSS yang rentan patah pada pembaruan tema masa depan dan tetap menyisakan ikon bulat tengah serta judul terpusat.

---

### ISS-HMD-002-03 — Tombol Preset Indikasi Klinis Bertumpuk 6 Baris Vertikal

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | Gunakan wadah chip tag mengalir (`styles.icdTags` / `styles.icdTag` atau chip button compact) dengan wrap fleksibel dan batasan tinggi yang nyaman. | Tombol preset mengalir secara alami dalam 2–3 baris horizontal rapi, ringkas, mudah dibaca, dan cepat diklik perawat/dokter. | Tidak ada. |
| B | Ubah pilihan cepat menjadi dropdown select tunggal. | Menghemat ruang paling banyak. | Menghilangkan kemudahan klik langsung satu per satu dan memerlukan 2 kali klik (buka dropdown lalu pilih). |

**Solusi terpilih: Opsi A.**
1. Mengikuti pola yang sudah ada pada `supporting-hemodialysis-form.jsx` (`styles.icdTags` dan `styles.icdTag`).
2. Di dalam modal berukuran `lg`, 6 preset indikasi klinis akan tertata rapi dalam 2 baris tanpa mendorong isian di bawahnya ke luar layar.

**Kenapa bukan opsi B:** Dokter dan perawat membutuhkan kecepatan klik (*quick-click*) saat mengisi formulir permintaan di situasi bangsal yang sibuk.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan Sebelum (Sesuai Laporan Pengguna)

```text
+-------------------------------------------------+
|                      ( i )                      |  <-- Ikon info bulat mengambang di tengah
|         Formulir Permintaan Hemodialisa         |  <-- Judul terpusat (center)
|                                                 |
| +---------------------------------------------+ |
| | Oemar Mobilindo (No. RM: 00-00-00-21)       | |
| | [ Konteks Rawat Inap Terkunci ]             | |  <-- Terjepit sempit (lebar 430px)
| | Kamar / Bed: Bangsal / -  DPJP: DPJP        | |
| +---------------------------------------------+ |
|                                                 |
| Status Urgensi Permintaan        [o] Rutin      |
| Permintaan Rutin / Terjadwal...                 |
|                                                 |
| Tgl Permintaan/Tindakan   Tipe Akses Vaskular   |  <-- Dua input dipaksa berhimpitan
| [ 06/10/2026 ]            [ AV Shunt / Cimino▾] |
|                                                 |
| Indikasi Klinis / Diagnosis HD *  Pilihan Cepat |
| [+ AKI Stadium 3 (Oliguria/Anuria)            ] |  <-- Preset bertumpuk 6 baris vertikal
| [+ CKD Stage 5 on HD Reguler                  ] |      mendorong seluruh form ke bawah
| [+ Hiperkalemia Refrakter (>6.5 mEq/L)        ] |
| [+ Edema Paru Akut / Overload Cairan          ] |
| [+ Asidosis Metabolik Berat (pH < 7.15)       ] |
| [+ Sindrom Uremikum / Ensefalopati            ] |
|                                                 |
| [ Tuliskan indikasi klinis spesifik...        ] |  <-- Textarea terpotong sebagian
| [ Batal ]          [ Kirim Permintaan HD ]      |  <-- Tombol terdorong ke bawah layar
+-------------------------------------------------+
```

### Tampilan Sesudah (Rencana Perbaikan)

```text
+-----------------------------------------------------------------------------------------+
| [🩸] Formulir Permintaan Hemodialisa                                                [×] |
| Pemesanan jadwal tindakan cuci darah ke Unit Hemodialisa dengan konteks rawat inap.     |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
| +-- Ringkasan Pasien Rawat Inap ------------------------------------------------------+ |
| | Oemar Mobilindo  (No. RM: 00-00-00-21)            [ Konteks Rawat Inap Terkunci ]   | |
| | Kamar / Bed: Bangsal Flamboyan / BD-RSMMC-00035  •  DPJP: dr. Rendy Pangalila       | |
| +-------------------------------------------------------------------------------------+ |
|                                                                                         |
| +-- Status Urgensi -------------------------------------------------------------------+ |
| | [!] Status Urgensi Permintaan                                      [  o] Rutin      | |
| |     Permintaan Rutin / Terjadwal sesuai jadwal reguler rawat inap.                    | |
| +-------------------------------------------------------------------------------------+ |
|                                                                                         |
| +-- Tanggal & Akses Vaskular ---------------------------------------------------------+ |
| | Tanggal Permintaan / Tindakan: *        Tipe Akses Vaskular Pasien: *               | |
| | [ 06/10/2026                 📅 ]       [ AV Shunt / Cimino (Akses Permanen)      ▾]| |
| +-------------------------------------------------------------------------------------+ |
|                                                                                         |
| +-- Indikasi Klinis & Diagnosis HD ---------------------------------------------------+ |
| | Indikasi Klinis / Diagnosis HD *                       Pilihan Cepat (Klik Tambah): | |
| | [+ AKI Stadium 3]  [+ CKD 5 on HD]  [+ Hiperkalemia >6.5]  [+ Edema Paru Akut]      | |
| | [+ Asidosis Berat pH <7.15]  [+ Sindrom Uremikum]                                   | |
| | [ Tuliskan indikasi klinis spesifik, komorbiditas, dan alasan HD...               ] | |
| | [ (minimal 5 karakter)                                                           ] | |
| +-------------------------------------------------------------------------------------+ |
|                                                                                         |
| +-- Instruksi Pengantar Bangsal ------------------------------------------------------+ |
| | Instruksi Khusus Pengantar Bangsal / Hasil Lab Terkini (Opsional):                  | |
| | [ Pasien tirah baring (bed-ridden), O2 nasal kanul 3 lpm, Hb 8.2 g/dL...          ] | |
| +-------------------------------------------------------------------------------------+ |
|                                                                                         |
+-----------------------------------------------------------------------------------------+
|                                                      [   Batal   ]  [ Kirim Permintaan ]|
+-----------------------------------------------------------------------------------------+
```

### Tabel Wilayah

| Wilayah | Isi | Sumber Data | Komponen |
| :--- | :--- | :--- | :--- |
| Header Modal | Ikon tetes darah tematik, Judul "Formulir Permintaan Hemodialisa", Subjudul panduan, dan tombol tutup silang `[×]` | Statis / parameter | `Modal.Header closeButton`, `Modal.Title`, `RiDropLine` |
| Ringkasan Pasien | Nama pasien, No RM, Bed, DPJP, dan lencana konteks terkunci | `props.episode` | `Badge`, info bar berlatar netral |
| Sakelar Urgensi | Switch toggle Cito vs Rutin, deskripsi penjelasan, dan `ClinicalSafetyAlert` jika Cito | `state.isCito` | `Form.Check type="switch"`, `ClinicalSafetyAlert` |
| Parameter Jadwal & Akses | Input date picker dan dropdown akses vaskular (2 kolom sejajar) | `state.requestedDate`, `state.vascularAccess`, `HMD_VASCULAR_ACCESS_OPTIONS` | `Form.Group`, `Form.Control type="date"`, `Form.Select` |
| Indikasi Klinis | Pilihan cepat preset (2 baris horizontal rapi) dan textarea alasan klinis wajib | `state.clinicalReason`, `HMD_CLINICAL_INDICATION_PRESETS` | Chip preset buttons, `Form.Control as="textarea"` |
| Instruksi Pengantar | Textarea opsional catatan pengantar bangsal / lab terkini | `state.wardNotes` | `Form.Control as="textarea"` |
| Footer Modal | Tombol Batal dan Tombol Kirim Permintaan HD (merah jika Cito, teal jika Rutin) | `state.isFormValid`, `props.loading` | `Modal.Footer`, `BaseButton` (`secondary` & `primary`/`danger`) |

### Tabel Tombol

| Tombol | Label | Jenis | Kapan Aktif | Yang Terjadi |
| :--- | :--- | :--- | :--- | :--- |
| `[×]` Close | Silang | Header icon button | Selalu aktif (kecuali saat loading) | Menutup modal tanpa mengirim data (`onCancel`) |
| Preset Indikasi | `+ {preset.label}` | Chip button | Selalu aktif | Menambahkan teks preset ke textarea alasan klinis |
| Batal | `Batal` | `BaseButton variant="secondary"` | Tidak sedang loading | Menutup modal (`onCancel`) |
| Kirim Permintaan | `Kirim Permintaan HD` / `Kirim Permintaan CITO` | `BaseButton variant="primary"` / `variant="danger"` | Form valid (`isFormValid`) & tidak loading | Mengirim payload permintaan ke service (`onSubmit`) |

### Tabel Keadaan

| Keadaan | Penanganan pada Tampilan |
| :--- | :--- |
| Form Baru Dibuka | Form di-reset: status Rutin, tanggal hari ini, akses Cimino, indikasi kosong, tombol submit mati sampai indikasi >= 5 karakter. |
| Sakelar Cito Aktif | Latar kotak urgensi berubah kemerahan lembut, teks label merah tebal, muncul kotak peringatan kritis `ClinicalSafetyAlert`, tombol kirim menjadi merah "Kirim Permintaan CITO". |
| Validasi Gagal | Muncul teks umpan balik merah di bawah textarea indikasi klinis. |
| Sedang Mengirim (*Loading*) | Tombol Kirim menampilkan status loading "Memproses...", tombol Batal dinonaktifkan. |

---

## 4. Rincian Perbaikan

### FIX-HMD-002-01 — Migrasi Pembungkus Modal ke React-Bootstrap Modal Ukuran `lg` dan Header-Footer Standar

| | |
| :--- | :--- |
| **Menutup** | `ISS-HMD-002-01`, `ISS-HMD-002-02`, `ISS-HMD-002-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`hemodialysis-order-modal.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx` |
| **Radius dampak** | Digunakan di 2 tempat: `supporting-service-tab.jsx` (dokter) dan `nursing-ancillary-section.jsx` (perawat). Keduanya akan otomatis menerima tampilan modal yang lega dan rapi. |
| **Bergantung pada** | Tidak ada |

**Langkah rinci:**
1. Hapus impor `ConfirmModal` dari `hemodialysis-order-modal.jsx`.
2. Impor `Modal` dari `react-bootstrap`.
3. Ganti elemen `<ConfirmModal ...>` dengan:
   ```jsx
   <Modal
     show={show}
     onHide={onCancel}
     centered
     size="lg"
     backdrop="static"
     data-testid="hemodialysis-order-modal"
   >
     <Modal.Header closeButton>
       <div className="d-flex align-items-center gap-2">
         <div
           className="d-flex align-items-center justify-content-center rounded-circle"
           style={{
             width: "36px",
             height: "36px",
             backgroundColor: isCito ? "#fee2e2" : "#e0f2fe",
             color: isCito ? "#dc2626" : "#0284c7",
             fontSize: "1.25rem",
             flexShrink: 0,
           }}
         >
           <RiDropLine />
         </div>
         <div>
           <Modal.Title className={styles.modalHeaderTitle}>
             Formulir Permintaan Hemodialisa
           </Modal.Title>
           <p className="text-muted small mb-0">
             Pemesanan jadwal tindakan cuci darah ke Unit Hemodialisa dengan konteks rawat inap terkunci.
           </p>
         </div>
       </div>
     </Modal.Header>
     <Modal.Body className={styles.modalBodyScrollable}>
       ...form isian...
     </Modal.Body>
     <Modal.Footer className="d-flex justify-content-end gap-2">
       <BaseButton
         type="button"
         variant="secondary"
         size="sm"
         disabled={loading}
         onClick={onCancel}
         data-testid="hmd-order-cancel-btn"
       >
         Batal
       </BaseButton>
       <BaseButton
         type="button"
         variant={isCito ? "danger" : "primary"}
         size="sm"
         disabled={!isFormValid || loading}
         loading={loading}
         onClick={handleConfirm}
         data-testid="hmd-order-confirm-btn"
       >
         {isCito ? "Kirim Permintaan CITO" : "Kirim Permintaan HD"}
       </BaseButton>
     </Modal.Footer>
   </Modal>
   ```
4. Pastikan atribut `data-testid="hmd-order-confirm-btn"` dan `data-testid="hmd-order-cancel-btn"` dipertahankan secara tepat agar tidak merusak uji otomatis.

**Acceptance criteria:**
- Modal tampil dengan lebar ukuran `size="lg"` (~800px) tanpa batas paksa 430px.
- Header modal memiliki ikon hemodialisa, judul rata kiri yang elegan, subjudul deskriptif, dan tombol `[×]`.
- Isian form tersaji lapang dalam `Modal.Body` dengan scrollbar internal yang mulus bila diperlukan.
- Footer modal memuat tombol Batal (`variant="secondary"`) dan Kirim Permintaan (`variant="primary"` atau `"danger"`).

---

### FIX-HMD-002-02 — Penataan Tombol Pilihan Cepat Indikasi Klinis HD Menjadi Chip Tag Horizontal

| | |
| :--- | :--- |
| **Menutup** | `ISS-HMD-002-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`hemodialysis-order-modal.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx` |
| **Radius dampak** | Modal formulir permintaan hemodialisa |
| **Bergantung pada** | `FIX-HMD-002-01` |

**Langkah rinci:**
1. Perbarui styling tombol pilihan cepat preset indikasi klinis agar tampil sebagai chip badge bergaris tepi lembut yang fleksibel:
   ```jsx
   <div className="d-flex flex-wrap gap-2 mb-2">
     {HMD_CLINICAL_INDICATION_PRESETS.map((preset) => (
       <button
         key={preset.id}
         type="button"
         className="btn btn-sm btn-outline-secondary py-1 px-2"
         style={{
           fontSize: "0.75rem",
           borderRadius: "6px",
           backgroundColor: "#f8fafc",
           borderColor: "#cbd5e1",
           color: "#334155",
           lineHeight: "1.2",
         }}
         onClick={() => handleSelectPreset(preset.text)}
       >
         + {preset.label}
       </button>
     ))}
   </div>
   ```
2. Pastikan klik pada preset tetap menambahkan teks ke textarea dan menghapus galat validasi field indikasi klinis.

**Acceptance criteria:**
- Preset indikasi klinis tidak lagi bertumpuk 6 baris penuh ke bawah, melainkan mengalir anggun dalam 2 baris rapi.
- Textarea alasan klinis dan instruksi bangsal tetap terlihat dengan nyaman tanpa terdorong ke luar layar.

---

## 5. Urutan Pengerjaan

```text
FIX-HMD-002-01 (Migrasi pembungkus ke Modal lg + Header & Footer)
  └── FIX-HMD-002-02 (Penataan chip preset indikasi klinis)
```

Kedua perbaikan dapat diselesaikan secara terpadu dalam satu refaktor bersih pada `hemodialysis-order-modal.jsx`.

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Status | Dampak bila ditolak |
| :---: | :--- | :---: | :--- |
| K-01 | Pengesahan migrasi modal permintaan hemodialisa dari `ConfirmModal` ke `react-bootstrap/Modal` berukuran `lg` | **Disetujui untuk perbaikan** | Form klinis tetap terjepit di 430px dengan tampilan konfirmasi hapus data. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada kontrak API backend atau entitas database yang berubah. Perubahan murni merupakan peningkatan tata letak visual UI/UX frontend (`FE-HMD-07` / `FE-HMD-12`).

---

## 8. Verifikasi Menyeluruh

1. **Pengujian Unit Frontend:**
   Jalankan `node --test tests/unit/hemodialysis-inpatient-order.test.mjs` di direktori `QuilvianSystemFrontendDev`.
   - 7 skenario pengujian wajib lulus 100%.
2. **Pengujian Linting:**
   Jalankan `npx eslint src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/hemodialysis-order-modal.jsx`.
   - 0 error, 0 warning.
3. **Pemeriksaan Visual / Interaktif:**
   - Buka modal melalui menu Rawat Inap -> Penunjang Medis -> Hemodialisa -> "+ Pesan Hemodialisa".
   - Verifikasi modal memiliki lebar besar (`lg` ~800px).
   - Verifikasi header rata kiri dengan ikon tetes darah `RiDropLine`.
   - Verifikasi preset indikasi mengalir dalam 2 baris rapi.
   - Verifikasi tombol Cito mengubah warna tombol konfirmasi menjadi merah darurat.
   - Verifikasi tombol Batal dan tombol Close silang berfungsi menutup modal.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Rencana perbaikan dibuat berdasarkan temuan `ISSUE-HMD-002`; mencakup solusi penggantian `ConfirmModal` ke `Modal` `lg` dan penataan chip preset. | `diagnose-module-issue` (Antigravity) |
| 2026-10-06 | Implementasi perbaikan diselesaikan pada `hemodialysis-order-modal.jsx`: migrasi ke Modal lg, chip preset fleksibel, validasi dan pengujian unit lolos 7/7, lint 0 error. Status rencana: SELESAI. | Antigravity Builder |
