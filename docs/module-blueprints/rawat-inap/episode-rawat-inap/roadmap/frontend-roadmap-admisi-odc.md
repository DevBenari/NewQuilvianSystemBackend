# Roadmap Frontend Pendaftaran Admisi Pasien One Day Care (ODC)

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` (COMPOSITE; peta menu pada [module map 10.2](../../02-module-map.md)) |
| planning_id / revision / tanggal | `ODC-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | Desain disetujui pengguna 17 Oktober 2026 lewat `/plan-module-delivery` menyerap `RWI-DEC-327` s.d. `332`. Kontrak target `0.15.0`. Setiap task dieksekusi tepat 1 task per pemanggilan via `build-module-frontend` dengan task ID-nya |
| blueprint / kontrak | [Frontend 19](../03-frontend-architecture.md), [PRD 29](../04-prd-to-mvp.md), [API Contract 16](../contracts/api-contract.md), [Flowchart 20](../flowcharts/20-alur-admisi-odc.md), [Validation Matrix](../contracts/validation-matrix.md) |
| input revisions | Decision log 53; manifest anak 14; module map 7; kontrak target `0.15.0` |
| source SHA frontend / backend | `fc9aea6d62418460493899cb669832f032e48328` / `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` |
| hash lock | [admisi-odc-delivery-lock.json](admisi-odc-delivery-lock.json) |
| scope / ID | 3 task `FE-RWI-260` s.d. `262`. Ruang ID dipindai global 2026-10-17: terpakai sampai `FE-RWI-259`; berikutnya `FE-RWI-263` |
| roadmap pasangan | [Backend](backend-roadmap-admisi-odc.md) · [Traceability](requirement-traceability-admisi-odc.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
FE-RWI-260 ─────────┬─> FE-RWI-262
                    │
                    └─┬─> FE-RWI-261
                      │
BE-RWI-248 [BE] ──────┘
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-admisi-odc.md](backend-roadmap-admisi-odc.md). Garis selalu prasyarat → task. `FE-RWI-260` menyediakan opsi tipe pasien dan state ODC yang menjadi prasyarat untuk filter bed di `FE-RWI-261` dan formulir persetujuan di `FE-RWI-262`. `FE-RWI-261` juga membutuhkan kueri bed ODC dari backend (`BE-RWI-248 [BE]`).

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 | — | `FE-RWI-260` ✅ | Penambahan tipe pasien ODC pada konstanta admisi dan Langkah 2 Wizard dengan validasi DPJP wajib |
| 2 | `FE-RWI-260`, `BE-RWI-248` [BE] | `FE-RWI-261` ✅ | Filter otomatis dan seleksi bed ODC (`isOdcBed=true`) pada Langkah 6 Wizard Penempatan Bed |
| 2 | `FE-RWI-260` | `FE-RWI-262` ✅ | Penyesuaian formulir & lembar cetak General Consent dengan judul dinamis ODC dan klausul edukasi 24 jam |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-260` ✅ Selesai | Penambahan opsi tipe pasien ke-4 ODC pada konstanta admisi dan Langkah 2 Wizard dengan DPJP penanggung jawab wajib | FR-ODC-001, FR-ODC-002; RWI-DEC-327, 328 | 0.15.0 draft; lock ODC-DEL-20261017-01 | `INPATIENT_PATIENT_TYPE`, `Step2PatientTypeView` | Tambah tipe ODC (value 4) pada konstanta, render kartu ODC ber-badge warning, wajibkan input DPJP Utama pada wizard state | — | RWI-AC-463, RWI-AC-464; VAL-ADM-13 | V-FE + UAT-ODC-01 | Registrasi & Admisi; kelengkapan DPJP klinis | D-FE |
| `FE-RWI-261` ✅ Selesai | Filter otomatis dan seleksi bed khusus ODC (`isOdcBed=true`) pada Langkah 6 Wizard Penempatan Bed | FR-ODC-003, FR-ODC-004; RWI-DEC-329, 330 | 0.15.0 draft; lock ODC-DEL-20261017-01 | `Step6BedPlacementView`, `inpatientAdmissionService` | Injeksi parameter `isOdcBed=true` saat query bed ketersediaan tipe ODC, render badge "Khusus ODC", blokir pemilihan bed reguler | `FE-RWI-260`, `BE-RWI-248` [BE] | RWI-AC-465, RWI-AC-466; VAL-ADM-10, 11 | V-FE + UAT-ODC-01, 02 | Admisi; kepatuhan alokasi bed fisik ODC | D-FE |
| `FE-RWI-262` ✅ Selesai | Penyesuaian formulir digital & lembar cetak General Consent dengan judul dinamis ODC dan klausul edukasi batasan 24 jam | FR-ODC-006; RWI-DEC-332 | 0.15.0 draft; lock ODC-DEL-20261017-01 | `Step7GeneralConsentView`, `GeneralConsentPrintTemplate`, `Step8ConfirmationView` | Judul dinamis persetujuan ODC, klausul batasan 24 jam, klausul transfer bed reguler saat komplikasi, ringkasan tipe ODC di Langkah 8 | `FE-RWI-260` | RWI-AC-468; VAL-ADM-14 | V-FE + UAT-ODC-01 | Legal RS & Rekam Medis; kepatuhan consent ODC | D-FE |

---

## Batas Handoff dan Definition of Done

### D-FE — Bukti Wajib Setiap Task Frontend

Sesuai aturan engineering frontend Quilvian:
1. Tidak mengubah skema API atau backend secara sepihak.
2. Menggunakan komponen dasar canonical (`@/components/features/base-features/base-form-control`, `BaseButton`, `StatusBadge`, `InformationAlert`).
3. Mengikuti token warna dan gaya tampilan yang konsisten tanpa inline/hardcoded CSS sembarangan.
4. Lolos pengecekan linter dan build Next.js (`npm run build` / `npm run lint`).
5. Menyajikan tangkapan layar/verifikasi visual pada laporan tracked `task/report/frontend/<TASK-ID>.md`.

---

## Kartu Task

### FE-RWI-260 — Penambahan Opsi Tipe Pasien ODC pada Konstanta Admisi dan Langkah 2 Wizard dengan DPJP Wajib

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai (Source Code Updated; Build Mandiri User) |
| Outcome | Langkah 2 Wizard Admisi Rawat Inap menampilkan 4 kartu pilihan tipe pasien: Umum, BPJS, Asuransi, dan One Day Care (ODC), serta mewajibkan pengisian DPJP Utama rawat inap saat opsi ODC dipilih |
| Ukuran / Batas | Modifikasi konstanta tipe pasien dan komponen Step 2 Wizard Admisi |
| Jejak | FR-ODC-001, FR-ODC-002; RWI-DEC-327, RWI-DEC-328; RWI-AC-463, RWI-AC-464; VAL-ADM-13 |
| Desain / Kontrak | Frontend 19.1; PRD Bagian 29.3 |
| Reuse / Disposisi | `inpatient-admission-constants.js` (Extend), `step-2-patient-type.jsx` (Extend), `BaseRadioGroup` / `SelectionCard` (Reuse) |
| Cakupan | (1) Perbarui objek `INPATIENT_PATIENT_TYPE` dengan menambahkan properti `ODC: { value: 4, id: 'odc', label: 'One Day Care (ODC)', description: 'Pelayanan observasi dan perawatan medis terencana paling lama 24 jam', badgeVariant: 'warning' }`. (2) Pada `step-2-patient-type.jsx`, verifikasi perenderan kartu ODC otomatis dari `INPATIENT_PATIENT_TYPE_OPTIONS`. (3) Saat tipe ODC aktif, kunci durasi estimasi perawatan ke `< 24 jam` dan tambahkan validasi form: field DPJP Utama wajib dipilih sebelum tombol `[Lanjut]` aktif |
| Batas Source | `src/features/rawat-inap/constants/inpatient-admission-constants.js`; `src/features/rawat-inap/components/admission-wizard/steps/step-2-patient-type.jsx` |
| Dependency | — |
| Verifikasi yang Wajib | V-FE (linter/build) + pengujian interaktif memilih kartu ODC, memastikan DPJP ditandai wajib dan tombol Lanjut terblokir jika DPJP belum dipilih |
| Definition of Done | D-FE; kriteria penerimaan terverifikasi; laporan `task/report/frontend/FE-RWI-260.md` |

---

### FE-RWI-261 — Filter Otomatis dan Seleksi Bed Khusus ODC (`isOdcBed=true`) pada Langkah 6 Wizard Penempatan Bed

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai (Source Code Updated; Build Mandiri User) |
| Outcome | Langkah 6 Wizard Admisi secara otomatis memfilter ketersediaan tempat tidur khusus ODC saat tipe pasien adalah ODC, menandai kartu bed dengan label badge "Khusus ODC", dan mencegah penempatan pada bed rawat inap reguler |
| Ukuran / Batas | Modifikasi integrasi pemanggilan service bed pada Step 6 Wizard Admisi |
| Jejak | FR-ODC-003, FR-ODC-004; RWI-DEC-329, RWI-DEC-330; RWI-AC-465, RWI-AC-466; VAL-ADM-10, VAL-ADM-11 |
| Desain / Kontrak | Frontend 19.2; API Contract Bagian 16 (`Inpatient Bed Occupancy`) |
| Reuse / Disposisi | `step-6-bed-placement.jsx` (Extend), `inpatient-admission-service.js` (Extend), `StatusBadge` (Reuse), `InformationAlert` (Reuse) |
| Cakupan | (1) Pada `step-6-bed-placement.jsx`, periksa tipe pasien dari context wizard. Jika `patientType === 4` (ODC), tambahkan parameter `isOdcBed: true` pada pemanggilan `getAvailableBeds`. (2) Tampilkan banner informasi biru (`InformationAlert`): *"Menampilkan tempat tidur khusus perawatan One Day Care (ODC)"*. (3) Render badge kuning "Khusus ODC" pada setiap kartu bed hasil kueri. (4) Jika pengguna mencoba memilih bed yang tidak bertanda ODC, tampilkan error validasi modal/alert dan tolak pemilihan |
| Batas Source | `src/features/rawat-inap/components/admission-wizard/steps/step-6-bed-placement.jsx`; `src/features/rawat-inap/services/inpatient-admission-service.js` |
| Dependency | `FE-RWI-260`, `BE-RWI-248` [BE] |
| Verifikasi yang Wajib | V-FE (linter/build) + pengujian integrasi browser memastikan hanya bed ODC yang tampil dan terpilih untuk tipe pasien ODC |
| Definition of Done | D-FE; kriteria penerimaan terverifikasi; laporan `task/report/frontend/FE-RWI-261.md` |

---

### FE-RWI-262 — Penyesuaian Formulir & Lembar Cetak General Consent dengan Judul Dinamis ODC dan Klausul Edukasi 24 Jam

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai (Source Code Updated; Build Mandiri User) |
| Outcome | Langkah 7 Wizard Admisi dan lembar cetak General Consent menyajikan judul dinamis khusus ODC serta memuat klausul edukasi batasan perawatan 24 jam dan klausul eskalasi alih rawat reguler |
| Ukuran / Batas | Modifikasi komponen formulir persetujuan umum Langkah 7 dan cetak PDF/kertas General Consent |
| Jejak | FR-ODC-006; RWI-DEC-332; RWI-AC-468; VAL-ADM-14 |
| Desain / Kontrak | Frontend 19.3; PRD Bagian 29.3 |
| Reuse / Disposisi | `step-7-general-consent.jsx` (Extend), `GeneralConsentPrintTemplate.jsx` (Extend), `step-8-confirmation.jsx` (Extend) |
| Cakupan | (1) Pada `step-7-general-consent.jsx`, jika `patientType === 4` (ODC), sesuaikan header menjadi *"Formulir Persetujuan Umum Pasien Rawat Inap (One Day Care - ODC)"*. (2) Tambahkan paragraf klausul edukasi: *"Pasien dan keluarga memahami bahwa layanan One Day Care (ODC) berlangsung maksimal 24 jam. Apabila kondisi medis pasien membutuhkan perawatan lanjutan, pasien akan dialihkan ke ruang rawat inap reguler dengan ketentuan biaya kelas perawatan terkait."* (3) Terapkan penyesuaian teks yang sama pada cetakan `GeneralConsentPrintTemplate.jsx`. (4) Pada Langkah 8 (Konfirmasi), pastikan ringkasan admisi menampilkan badge tipe pasien "One Day Care (ODC)" dan nama DPJP penanggung jawab |
| Batas Source | `src/features/rawat-inap/components/admission-wizard/steps/step-7-general-consent.jsx`; `src/features/rawat-inap/components/admission-documents/general-consent-print-view.jsx`; `src/features/rawat-inap/components/admission-wizard/steps/step-8-confirmation.jsx` |
| Dependency | `FE-RWI-260` |
| Verifikasi yang Wajib | V-FE (linter/build) + verifikasi visual tampilan Langkah 7 dan pratinjau cetak PDF formulir General Consent ODC |
| Definition of Done | D-FE; kriteria penerimaan terverifikasi; laporan `task/report/frontend/FE-RWI-262.md` |
