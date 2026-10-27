# Laporan Perubahan Frontend — `FE-RWI-229`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-229` |
| Judul | Integrasi Pre-fill Stepper Admisi Pasien Transfer IGD (Auto-Populate DPJP, Diagnosa & Identitas) |
| Slice | Slice Transfer IGD / RJ; Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-transfer-igd.md`](../../../roadmap/frontend-roadmap-admisi-transfer-igd.md) — kartu `FE-RWI-229` |
| Trace | `FR-RI-190`, `FR-RI-191`; `RWI-DEC-275`; `RWI-AC-401`, `RWI-AC-403`, `RWI-AC-404`; API Contract `0.12.0` (11.7.2) |
| Contract version | `0.12.0` **`approved`** (10 Oktober 2026) |
| Dependency | `BE-RWI-206` ✅, `FE-RWI-228` ✅ |
| Klasifikasi | `HIGH` — Integrasi stepper pendaftaran terpadu, auto-populate data demografi, DPJP, dan diagnosa IGD |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-transfer-review-step.jsx`, `inpatient-admission-view.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-401`, `RWI-AC-403`, dan `RWI-AC-404` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Sesuai kebutuhan Analisis Bisnis (Tahap C alur admisi transfer IGD):
1. **Peniadaan Entri Data Ganda (*Auto-Populate*):** Saat pasien transfer IGD dipilih dari antrean, petugas admisi tidak perlu mengetik ulang data pasien dari lembar pengantar kertas. Nama, NIK, No. Rekam Medis, tanggal lahir, dan jenis kelamin pasien langsung terisi otomatis.
2. **Keterikatan Medis IGD:** Dokter penanggung jawab (DPJP) yang dituju dari IGD dan diagnosa awal/indikasi opname terisi otomatis pada form admisi.
3. **Penyelarasan Alur Bersama Wali Pasien:** Petugas dan wali pasien langsung fokus pada penentuan penjamin biaya (BPJS/Asuransi/Umum), pemilihan kamar yang tersedia (*Bed Selection*), pemesanan kamar (*Bed Booking*), dan penandatanganan formulir persetujuan rawat inap secara digital.

---

## 2. Rincian Perubahan Source Code

### 2.1 `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-review-step.jsx`
- Membuat komponen langkah verifikasi pasien transfer IGD (`transfer-patient-review`):
  - Kartu Identitas Pasien terverifikasi (Nama, No RM, Jenis Kelamin).
  - Kartu Instruksi Klinis IGD (Dokter pemeriksa/DPJP, Diagnosa kerja IGD, Rekomendasi bangsal/unit).
  - Banner panduan operasional langkah selanjutnya bersama keluarga pasien.
  - Tombol navigasi `[Ganti Pasien Transfer]` untuk kembali ke antrean dan `[Lanjut ke Pembayaran]` untuk maju ke pemilihan penjamin biaya.

### 2.2 `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- Menambahkan state `transferReferral` dan fungsi handler `handleSelectTransferReferral`.
- Memetakan langkah `transfer-referral` ke komponen `InpatientAdmissionTransferList`.
- Memetakan langkah `transfer-patient-review` ke `InpatientAdmissionTransferReviewStep`.
- Meneruskan data rujukan transfer ke controller `doctor`, `payment`, dan `patient` sehingga stepper langsung melompat melewati pencarian pasien manual menuju pemilihan kamar & penjamin.

---

## 3. Bukti Verifikasi
- Pengujian unit `inpatient-admission-transfer.test.mjs` membuktikan bahwa `transfer-referral` dan `transfer-patient-review` terdaftar dalam urutan langkah transfer.
- Simulasi klik "Proses Admisi" memuat data pasien dan DPJP tanpa input manual.


## Koreksi bukti 12 Oktober 2026 - EPS-006

Status saat ini: 🟡 SEBAGIAN. Laporan historis tidak menjadi bukti runtime slice transfer current. Source diperbaiki pada MHamzah/HamzahV2 dengan [kontrak EPS-006](../../../contracts/admission-transfer-repair-006.md). Bukti build, unit, browser dan batas migration/database ada di [FIX-EPS-006-08](../frontend/FIX-EPS-006-08.md). Tidak mengklaim transaksi PostgreSQL PROVEN.


---

## Pembaruan 26 Oktober 2026 — Harmonisasi Pola State Switch 1-Step (Standar Pasien Lama)

### 1. Latar Belakang & Persetujuan Pengguna
Mengikuti arahan pengguna tertanggal 26 Oktober 2026, antrean transfer IGD & Poliklinik diharmonisasi dengan pola pendaftaran Pasien Lama (`InpatientAdmissionExistingPatientSplitStep`). Pemilihan antrean transfer dan verifikasi data rujukan disatukan ke dalam **1 Langkah Tunggal dengan fitur Switch**:
1. **Pola State Switch Terpadu:**
   - **State A (`!hasSelectedTransfer`):** Tabel antrean transfer IGD & Poliklinik full-width (`InpatientAdmissionTransferList`).
   - **State B (`hasSelectedTransfer`):** Verifikasi data pasien dan rujukan klinis 2-kolom seimbang (`InpatientAdmissionTransferReviewStep`) dengan tombol `[Pilih Pasien Lain]` di kanan atas header untuk kembali ke State A secara instan.
2. **Penyederhanaan Stepper:**
   - Stepper alur transfer disederhanakan dari 10 tahapan menjadi **9 tahapan terpadu**, mengurangi fragmentasi dan beban kognitif petugas admisi.

### 2. Berkas yang Diubah / Dibuat
- `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-split-step.jsx` (Komponen wrapper terpadu baru).
- `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-review-step.jsx` & `.module.css` (Penambahan tombol header switcher dan penyelarasan label navigasi).
- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` (Integrasi komponen split step dan penyelarasan handler URL).
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` (Pembaruan konstanta `INPATIENT_ADMISSION_TRANSFER_STEPS` menjadi 9 langkah).
- `tests/unit/inpatient-admission-transfer.test.mjs` (Validasi stepper 9 langkah lulus 100%).
