# Laporan Task FE-HRD-108: Modal Unggah Surat Sakit Dokter Ber-SIP & Masking Privasi Medis K3RS

## 1. Informasi Task
* **Task ID**: `FE-HRD-108`
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Nama Fitur**: Modal Unggah Surat Sakit Ber-SIP (`MedicalCertificateUploadModal`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun modal dialog unggah surat keterangan sakit dokter ber-SIP untuk cuti sakit $ge 2$ hari (`HRD-DEC-112`, `VR-LEV-06` s.d. `VR-LEV-08`):
1. Input nama rumah sakit / klinik penerbit surat sakit.
2. Input nomor Surat Izin Praktik (SIP) dokter pemeriksa yang sah.
3. Input tanggal terbit surat sakit.
4. Dropzone berkas pindaian foto/PDF (maksimal 5 MB).
5. Banner jaminan perlindungan privasi medis staf K3RS: diagnosa klinis disamarkan dari atasan langsung.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/MedicalCertificateUploadModal.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Dropzone berkas dan penegakan validasi SIP dokter bekerja optimal.
