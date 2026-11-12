# Laporan Perubahan Frontend — `FE-HRD-058`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-058` |
| Judul | Fondasi Layanan API (`lifecycle-service.js`), Redux State, & Konstanta Label Status `S-C4-ONB` |
| Slice | `S-C4-ONB` (Onboarding dan Kontrak Kerja Pegawai RS) · `MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 1 S-C4-ONB |
| Trace | `HRD-DEC-081`, `HRD-DEC-096`, `03-frontend-architecture.md` 14.6 & 14.7, API Contract v9 bagian 15 |
| Contract version | API Contract v9 bagian 15.1 s.d. 15.9, Permission v9 bagian 13 |
| Dependency | — (Mandiri / Fondasi Layanan Pertama) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/lifecycle-service.js`, `src/utils/hr/lifecycle/lifecycle-display-utils.js`, `src/lib/state/slice/hr/lifecycle/lifecycle-slice.js`, `src/lib/state/store.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Sub-modul Onboarding dan Kontrak Kerja mengelola perjalanan staf baru sejak hari pertama masuk kerja, masa orientasi berjenjang (3-lapis), evaluasi masa percobaan 3 bulan, bimbingan klinis perawat/nakes (preseptorship), hingga tata kelola kontrak PKWT dan PKWTT.

Task ini membangun fondasi klien API terintegrasi dan state management:
1. **Axios Client Terpadu (`LifecycleService`)**:
   - Menghubungkan seluruh agregat lifecycle ke backend ASP.NET Core: `onboarding-templates`, `onboarding-cases`, `employment-contracts`, `clinical-preceptorships`, `probations`, `initial-contract-batches`, `onboarding-access-requests`, `lifecycle-settings`, dan `worklist`.
   - Standarisasi penanganan error 409 (konflik versi data usang) dengan pesan informatif: *"Data ini sudah diubah orang lain. Muat ulang lalu ulangi."*
2. **Kamus & Utilitas Tampilan Terpusat (`lifecycle-display-utils.js`)**:
   - Pemetaan label Indonesia untuk status kasus orientasi, lapis checklist (HRD, Fasilitas IT, Unit), jenis kontrak (PKWT, PKWTT, Mitra Dokter), status probation, dan format mata uang/tanggal.
3. **Penyimpanan State Redux (`lifecycle-slice.js`)**:
   - Terdaftar di `store.jsx` dengan async thunk untuk pembacaan dan pembaruan data secara reaktif.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Seluruh endpoint bagian 15.1 s.d. 15.9 memiliki fungsi service dengan header multipart saat unggah berkas | ✅ PASS | Terbukti pada `LifecycleService` di `lifecycle-service.js` dengan fungsi lengkap dan header `multipart/form-data`. |
| 2 | Label status berada di satu berkas konstanta dan tidak diulang di komponen | ✅ PASS | Terbukti pada `lifecycle-display-utils.js` yang mengekspor status enum dan fungsi badge mapper. |
| 3 | Jawaban 409 memunculkan pesan ramah pembaruan bersama | ✅ PASS | Ditangani oleh `handleError` pada `lifecycle-service.js`. |
| 4 | Kepatuhan Aturan CSS Bebas Tailwind | ✅ PASS | Modul styling `lifecycle.module.css` 100% menggunakan CSS variables token standar Quilvian. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/lib/services/hr/lifecycle-service.js` (Klien service Axios)
- `src/utils/hr/lifecycle/lifecycle-display-utils.js` (Utilitas format dan konstanta status)
- `src/lib/state/slice/hr/lifecycle/lifecycle-slice.js` (Redux Toolkit slice)
- `src/lib/state/store.jsx` (Registrasi reducer `lifecycle`)
- `src/style/hr/lifecycle/lifecycle.module.css` (CSS Modules tanpa Tailwind)
