# PLAN-REPAIR-009 — Perbaikan Tombol "+ Buat Template Baru" dan Penyinkronan Prop Template Resep Dokter IGD

```yaml
plan_id: PLAN-REPAIR-IGD-009
issue: ../issue/issue-009-tombol-buat-template-resep-tidak-berfungsi.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-18"
diputuskan_oleh: "Pengguna / Pemilik proses (instruksi perbaikan langsung pada 18-11-2026)"
tanggal_keputusan: "2026-11-18"
basis_source_backend: "ae054ee8 (rizkiG)"
basis_source_frontend: "a2ee4c22c (RizkiV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-009-01` | `ISS-IGD-009-01`, `ISS-IGD-009-T1` | Sinkronkan prop pemanggilan `PrescriptionTemplatesPanel` pada tab resep dokter IGD (`onCreateNew`, `state`, `doctorName`, `onRetry`) | FE | 1 | — | ✅ SELESAI | `src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-prescription-tab.jsx` & `prescription-templates-panel.jsx`; lolos ESLint 0 galat |
| `FIX-IGD-009-02` | `ISS-IGD-009-T2` | Sediakan prop `noticeText` fleksibel dan sesuaikan teks keterangan agar tidak menyebut "rawat inap" saat dibuka di IGD | FE | 2 | — | ✅ SELESAI | `prescription-templates-panel.jsx` & kedua tab resep IGD (`doctor-emergency` dan `emergency-assessment`); lolos ESLint 0 galat |

**Ringkasan: 2 dari 2 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-IGD-009-01 — Tombol "+ Buat Template Baru" tidak mengeksekusi aksi

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi A (Terpilih)** | Hubungkan `onCreateNew` pada `EmergencyDoctorPrescriptionTab` untuk mengalihkan ke sub-tab Buat Resep (`setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER)`), serta sinkronkan prop `state={templatesState}`, `doctorName={loggedInDoctorName}`, dan `onRetry={refreshTemplates}`. | Sesuai desain resep Quilvian di seluruh modul; dokter dapat langsung meracik obat lalu menyimpannya sebagai template; memperbaiki pula status loading dan retry. | Memerlukan peralihan tab (bukan modal langsung dari tab template). |
| Opsi B | Buka langsung modal `DoctorPrescriptionTemplateCreateModal` dari sub-tab Template Resep. | Dokter tidak perlu berpindah tab. | Melanggar arsitektur resep Quilvian: modal template membutuhkan draf obat/racikan aktif yang sudah dipilih agar isi template tidak kosong. |

**Solusi terpilih: Opsi A**. Pada arsitektur resep Quilvian, template resep dibentuk dari draf obat/racikan aktif. Mengarahkan dokter ke tab "Buat Resep" adalah alur baku yang identik dengan modul Rawat Inap (`inpatient-prescription-tab.jsx`) dan Asesmen IGD (`emergency-assessment-prescription-tab.jsx`).

---

## 3. Skema tampilan sebelum → sesudah

**SEBELUM:**
```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Buat Resep ]       [ Template Resep (Aktif) ]       [ Riwayat Resep ]│
├────────────────────────────────────────────────────────────────────────┤
│ Daftar Template Resep                               [+ Buat Template]  │ ← Dikelik: TIDAK BEREAKSI
│ Ruang kerja rawat inap hanya menampilkan template pribadi...           │ ← Teks ganjil di IGD
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │                         Belum Ada Template                         │ │
│ │                  [+ Buat Template Baru]                            │ │ ← Dikelik: TIDAK BEREAKSI
│ └────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

**SESUDAH:**
```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Buat Resep ]       [ Template Resep (Aktif) ]       [ Riwayat Resep ]│
├────────────────────────────────────────────────────────────────────────┤
│ Daftar Template Resep (dr. Jenah Ali)               [+ Buat Template]  │ ← Dikelik: Pindah ke "Buat Resep"
│ Halaman ini hanya menampilkan template resep pribadi milik Anda...    │ ← Teks netral
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │                         Belum Ada Template                         │ │
│ │                  [+ Buat Template Baru]                            │ │ ← Dikelik: Pindah ke "Buat Resep"
│ └────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Rincian perbaikan

### FIX-IGD-009-01 — Sinkronisasi Prop `PrescriptionTemplatesPanel` pada Dokter IGD

**Berkas:**
1. `QuilvianSystemFrontendDev/src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-prescription-tab.jsx`
2. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-templates-panel.jsx`

**Perubahan kode:**
- Pada `emergency-doctor-prescription-tab.jsx`:
  - Menambahkan `onCreateNew={() => setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER)}`.
  - Mengubah `templatesState` menjadi `state={templatesState}`.
  - Mengubah `loggedInDoctorName` menjadi `doctorName={loggedInDoctorName}`.
  - Mengubah `onRefresh` menjadi `onRetry={refreshTemplates}`.
- Pada `prescription-templates-panel.jsx`:
  - Mendukung prop alias cadangan: `panelState = state || templatesState`, `panelDoctorName = doctorName || loggedInDoctorName`, dan `panelRetry = onRetry || onRefresh`.

**Hasil Acceptance Criteria:**
- [x] Klik tombol `+ Buat Template Baru` di sudut kanan atas panel memindahkan tab aktif ke sub-tab **"Buat Resep"**.
- [x] Klik tombol `+ Buat Template Baru` di kotak status kosong (*empty state*) memindahkan tab aktif ke sub-tab **"Buat Resep"**.
- [x] Status memuat (*loading spinner*) dan batas galat (*error boundary*) bekerja karena `panelState` terhubung.
- [x] Tombol "Coba Lagi" saat galat memicu pemanggilan ulang `panelRetry`.
- [x] Header panel menampilkan nama dokter login ketika template masih kosong.

---

### FIX-IGD-009-02 — Penyesuaian Teks Pemberitahuan Template Resep

**Berkas:**
- `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-templates-panel.jsx`
- `QuilvianSystemFrontendDev/src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-prescription-tab.jsx`
- `QuilvianSystemFrontendDev/src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-prescription-tab.jsx`

**Perubahan kode:**
- Menambahkan *prop* `noticeText` pada `PrescriptionTemplatesPanel`: `{noticeText || INPATIENT_PRESCRIPTION_TEXT.templateMineOnlyNotice}`.
- Meneruskan `noticeText="Halaman ini hanya menampilkan template resep pribadi milik Anda sendiri."` pada tab resep dokter IGD.

**Hasil Acceptance Criteria:**
- [x] Sub-tab Template Resep pada Ruang Kerja Dokter IGD menampilkan keterangan yang relevan dengan konteks IGD dan tidak menyebut "rawat inap".
- [x] Sub-tab Template Resep pada Ruang Kerja Rawat Inap tetap mempertahankan teks bawaan rawat inap tanpa regresi.

---

## 5. Urutan pengerjaan

```text
FIX-IGD-009-01 (Prioritas 1: Perbaikan fungsi klik & prop komponen) — SELESAI
  └── FIX-IGD-009-02 (Prioritas 2: Perapihan teks pemberitahuan konteks IGD) — SELESAI
```

---

## 6. Riwayat

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-11-18 | Antigravity AI | Pembuatan dokumen rencana perbaikan awal untuk ISSUE-IGD-009. |
| 2026-11-18 | Antigravity AI | Penerapan perbaikan FIX-IGD-009-01 dan FIX-IGD-009-02; verifikasi ESLint 0 error; status rencana diubah menjadi SELESAI. |
