# Roadmap Frontend Admisi Pasien Operasi & Kasus Bedah Obgyn

| Metadata | Nilai |
| :--- | :--- |
| **blueprint_id / owner** | `RWI-BP-001` / `episode-rawat-inap` |
| **planning_id / revision / tanggal** | `ADM-OPR-FE-20261023-01` / 1 / 2026-10-23 Asia/Jakarta |
| **status / approval** | `APPROVED_DESIGN_PLANNED`; disetujui lewat `RWI-DEC-355` s.d. `RWI-DEC-358` dan `OPS-DEC-038` s.d. `OPS-DEC-042`. Eksekusi memakai `build-module-frontend`, tepat satu task per pemanggilan |
| **kontrak terkunci** | `0.19.0`; [Dokumen Desain Terpadu](../../../operations/05-desain-amandemen-operasi-dan-admisi.md), [Dokumen Rujukan Admisi](../06-desain-amandemen-admisi-operasi-obgyn.md) |
| **scope / ID** | 3 task frontend: `FE-RWI-280`, `FE-RWI-281`, dan `FE-RWI-282` |
| **roadmap pasangan** | [Backend](backend-roadmap-admisi-operasi.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | :--- |
| ✅ | Selesai dan seluruh acceptance/DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` |

---

## Grafik Urutan Dependency

```text
FE-RWI-282 ✅ (Mandiri — Selesai 24 Okt 2026)

[BE-RWI] BE-RWI-182 ─> FE-RWI-280 ✅ ─┬─> FE-RWI-281
                                   │
[BE-RWI] BE-RWI-183 ───────────────┘
```

> **Legenda:**  
> `[BE-RWI]` = Cermin baca-saja dari task backend Modul Rawat Inap (`backend-roadmap-admisi-operasi.md`).

---

## Tabel Gelombang Eksekusi

| Gelombang | Boleh mulai setelah | Task ID | Hasil Gelombang |
| ---: | :--- | :--- | :--- |
| 1 | Mandiri (Tanpa dependency backend) | `FE-RWI-282` ✅ | Pencabutan & penghapusan penuh kartu serta jalur masuk Admisi Kamar Pulih ([Laporan Task](../task/report/frontend/FE-RWI-282.md)) |
| 1 | Kontrak backend `BE-RWI-182` siap | `FE-RWI-280` | Widget kartu status jadwal operasi IBS read-only pada Langkah 1 Rujukan Admisi |
| 2 | `FE-RWI-280` & Kontrak `BE-RWI-183` siap | `FE-RWI-281` | Formasi tri-dokter di Langkah 5, filter bangsal maternitas & toggle boks bayi di Langkah 6 & 7 |

---

## Register Task Frontend

| Task ID | Outcome | Trace | Kontrak | Cakupan & Reuse | Dependency | Acceptance Criteria & Verifikasi | DoD |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FE-RWI-280` | Widget kartu status jadwal operasi IBS read-only pada Langkah 1 Rujukan Admisi | `RWI-DEC-356` | `0.19.0` | `inpatient-admission-existing-patient-step.jsx`, `useInpatientAdmissionReferralSelection.jsx` | `[BE-RWI] BE-RWI-182` | 1. Memanggil API lookup saat No. RM / SPRI dimasukkan.<br/>2. Menampilkan kartu toska informatif: Ruang OK, Jam, Operator, Anestesi, dan Anak.<br/>3. Petugas admisi tidak memiliki kontrol untuk mengedit ruangan/jam.<br/>4. Unit test lulus. | Komponen banner + state handling + unit test |
| `FE-RWI-281` | Formasi tri-dokter, filter bangsal maternitas, dan toggle fasilitas boks bayi | `RWI-DEC-357`, `RWI-DEC-358` | `0.19.0` | `inpatient-admission-doctor-step.jsx`, `inpatient-admission-bed-step.jsx` | `FE-RWI-280`, `[BE-RWI] BE-RWI-183` | 1. Langkah 5 Dokter memuat otomatis Sp.OG, Sp.An, dan Sp.A.<br/>2. Langkah Bed Selection otomatis memfilter ke Bangsal Maternitas/Nifas.<br/>3. Toggle `[✓] Fasilitas Rawat Gabung Boks Bayi` aktif dan nilai tersimpan di form state.<br/>4. Unit test lulus. | Form fields update + selector hook + unit test |
| `FE-RWI-282` ✅ | Penghapusan penuh kartu dan jalur masuk Admisi Kamar Pulih dari antarmuka Admisi | `RWI-DEC-359`, `OPS-DEC-040` | `0.19.0` | `inpatient-admission-view.jsx`, `inpatient-admission-flow-constants.jsx` | Mandiri | 1. Kartu "Admisi dari Kamar Pulih" dihapus dari `INPATIENT_ADMISSION_ENTRY_OPTIONS` pada beranda `/admissions`.<br/>2. Beranda admisi ranap menyajikan tepat 4 pintu masuk resmi.<br/>3. Route `/admissions/recovery` dinonaktifkan/dialihkan aman.<br/>4. Unit test diperbarui dan lulus 4/4 PASS. | Pembersihan kartu & konstanta opsi + unit test ([Laporan Task](../task/report/frontend/FE-RWI-282.md)) |

---

## Aturan Desain & Pengalaman Pengguna (UX Rules)
1. **Peniadaan Jalur Kamar Pulih**: Sesuai standar operasional rumah sakit (`RWI-DEC-359`), seluruh pasien operasi harus masuk melalui rawat inap terlebih dahulu di bangsal sebelum tindakan bedah. Kartu masuk "Admisi dari Kamar Pulih" dihapus total dari opsi loket pendaftaran rawat inap depan.
2. **Design Tokens**: Menggunakan komponen baku Quilvian: `InformationAlert`, `StatusBadge`, `BaseCheckboxCard`, dan token CSS modul admisi yang sudah ada.
