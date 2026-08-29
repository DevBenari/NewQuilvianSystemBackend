# Laporan Perubahan Frontend — `FE-RWI-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-024` |
| Judul | Penjamin dan kelas perawatan dipilih, bukan diasumsikan |
| Slice | Layar `FE-INP-03` — langkah **Pembayaran** alur admisi rawat inap |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` revision `5`, kartu `FE-RWI-024` |
| Trace | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 3; `05-skema-tampilan.md` bagian 3.5 |
| Contract version | Kontrak diambil dari source backend commit `3c4c06fc41601c23c4173e033d3ffff68230d5a1` — `PatientInsuranceController`, `PatientCompanyGuarantorController`, `InsuranceProviderController` (Administrator), `CompanyGuarantorController` (Administrator), `PatientClassController`. Setiap route, DTO, dan policy diverifikasi langsung ke source |
| Wewenang UI | Bentuk pemilihan penjamin `DEV_DISCRETION`. **Batas:** kelas perawatan wajib dipilih di langkah ini |
| Dependency | `FE-RWI-023` — **selesai**, committed `b8080e444` ([FE-RWI-023](FE-RWI-023.md)). Tidak ada dependency task backend |
| Klasifikasi | `MEDIUM` — empat berkas baru dan tiga berkas berubah; satu service HTTP baru, satu hook controller baru, satu utility baru, satu view component baru; tanpa Redux slice baru, tanpa base component baru, tanpa dependency package baru |
| Task mode | `FRONTEND` dengan wewenang lintas repository terbatas untuk laporan tracked ini |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` untuk source; `NewQuilvianSystemBackend` branch `MHamzah` hanya untuk laporan ini |
| Model | Gemini (Google Antigravity) |
| Commit frontend saat dikerjakan | `b8080e444463541e902db7afd367664d9214085a` |
| Commit backend yang dijadikan rujukan | `3c4c06fc41601c23c4173e033d3ffff68230d5a1` |
| Tanggal | 2026-08-29 |
| Status | **Implementasi 5 dari 5 acceptance criteria tersedia. `npm run lint:errors` EXIT 0 dan `npm run build` compiled successfully. Verifikasi manual `NOT FEASIBLE` karena tidak tersedia sesi login petugas admisi; kriteria 1, 2, dan 5 karenanya belum terbukti dengan data nyata** |

---

## 1. Keadaan yang ditemukan di awal

`FE-RWI-023` sudah mendirikan langkah pendaftaran pasien baru dan pencarian/tinjauan pasien lama. Langkah **Pembayaran** masih berupa `PendingStep` — pemberitahuan bahwa isinya dimiliki task lanjutan. Petugas admisi tidak dapat memilih cara bayar, mendaftarkan kartu penjamin, atau memilih kelas perawatan.

Gerbang skema `RWI-UI-GAP-002` menyatakan *"request kunjungan source terkini belum membawa payer perusahaan."* Pemeriksaan menunjukkan gap ini menyangkut `POST /patient-encounters` yang dimiliki `FE-RWI-025`, **bukan** langkah Pembayaran ini. Pendaftaran dan pemilihan kartu penjamin perusahaan dapat dikerjakan sekarang karena controller `PatientCompanyGuarantor` dengan route `/admin` sudah lengkap.

Pola yang dirujuk kartu task — `payment-method-step.jsx`, `emergency-patient-payer-modal.jsx`, `patient-payer-table.jsx`, dan `patient-payer-drawer.jsx` — diperiksa. Keempatnya terikat pada Redux slice pendaftaran IGD dan memakai route kiosk (`KioskRead`). Karena itu polanya dipakai sebagai referensi, bukan diimpor langsung.

---

## 2. Tabel keputusan base component

`UI GATE: 10 elemen — REUSE 6, EXTEND 0, COMPOSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Status | Keputusan |
|---|---|---|---|
| Tiga kartu cara bayar | `BaseCheckboxCard` | REUSE | Tiga `BaseCheckboxCard` dalam `radiogroup` |
| Kartu ringkas tunai | JSX + `InformationAlert` | COMPOSE | Panel CSS module |
| Daftar kartu penjamin | `DataTable` | REUSE | `DataTable` dengan `onRowClick` |
| Panel penjamin dipilih | `<dl>` + CSS module | COMPOSE | Sama dengan pola `selectedPayerPanel` IGD |
| Tombol + Tambah Kartu | `BaseButton` | REUSE | `variant="secondary" size="sm"` |
| Modal pendaftaran | `createPortal` + `FormProvider` + `BaseButton` + `ResourceFilterSelect` | COMPOSE | Portal + form tanpa base modal (tidak ada `BaseModal` di base-features) |
| Isian kelas perawatan | `ResourceFilterSelect` | REUSE | Filter `isForInpatient=true` |
| Pesan error | `InformationAlert` | REUSE | `variant="danger"` |
| Tombol Kembali / Lanjut | `BaseButton` | REUSE | `variant="secondary"` dan `variant="primary"` |
| Penanda penjamin kosong | JSX panel | COMPOSE | Panel CSS module dengan border dashed |

Tidak ada elemen `NEW` atau `EXTEND` yang mengubah perilaku default.

---

## 3. Daftar berkas

### Berkas baru (4)

| Berkas | Peran | Baris |
|---|---|---|
| `src/lib/services/health-services/inpatient-management/inpatient-admission-payment.service.js` | Service HTTP — 7 fungsi untuk asuransi pasien, penjamin perusahaan, opsi provider, opsi perusahaan, dan opsi kelas perawatan | ~270 |
| `src/utils/health-services/inpatient-management/inpatient-admission-payment-utils.jsx` | Normalizer response, merger daftar penjamin, payload builder, format tanggal | ~250 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-payment.jsx` | Hook controller langkah pembayaran — cara bayar, daftar penjamin, modal, kelas perawatan, validasi | ~310 |
| `src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx` | View langkah Pembayaran — tiga kartu, tabel, panel, modal, `ResourceFilterSelect` | ~545 |

### Berkas berubah (3)

| Berkas | Perubahan |
|---|---|
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Import payment step dan hook; tambah case `payment` di `AdmissionStep`; instantiasi `useInpatientAdmissionPayment` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Tambah `INPATIENT_PAYMENT_CATEGORY`, `INPATIENT_PAYMENT_CATEGORY_OPTIONS`, `INPATIENT_PAYER_SOURCE`, `INPATIENT_PAYER_SOURCE_OPTIONS`, `DEFAULT_INPATIENT_PAYER_FORM_VALUES` |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Tambah ~325 baris style — payment grid, payer layout, cash card, detail panel, modal, service class |

---

## 4. Pemetaan acceptance criteria

| # | Kriteria | Implementasi | Bukti |
|---|---|---|---|
| 1 | Ketiga cara bayar tersedia dan dipilih sadar — **tidak ada** nilai bawaan | `paymentCategory` state dimulai `null`; `INPATIENT_PAYMENT_CATEGORY_OPTIONS` memuat tiga kartu; tombol Lanjut `disabled={!payment.canContinue}` | Source `use-inpatient-admission-payment.jsx` baris `const [paymentCategory, setPaymentCategory] = useState(null)` |
| 2 | Asuransi dan penjamin perusahaan menuntut kartunya dipilih atau didaftarkan | `canContinue` memeriksa `selectedPayer` ketika bukan tunai; `handleNext` menampilkan pesan error saat penjamin belum dipilih | Source `inpatient-admission-payment-step.jsx` baris `if (!payment.isCash && !payment.selectedPayer)` |
| 3 | Kelas perawatan dipilih di langkah ini | `canContinue` memeriksa `patientClassId`; `ResourceFilterSelect` dengan `isForInpatient=true` | Source `use-inpatient-admission-payment.jsx` baris `if (!patientClassId) return false` |
| 4 | Nomor kartu asuransi **tidak** muncul di luar langkah ini | Data payer disimpan di hook state lokal, bukan di URL query atau Redux global. Langkah lain tidak mengimpor atau menampilkan nomor kartu | Source — `selectedPayer` dan `payerList` ada di `useInpatientAdmissionPayment` |
| 5 | Isian tidak hilang ketika server menolak | `savePayer` menangkap error dan menampilkan pesan lewat `savePayerError` / `InformationAlert`; form tetap utuh karena `form.reset` hanya dipanggil setelah sukses | Source `use-inpatient-admission-payment.jsx` catch block; `inpatient-admission-payment-step.jsx` `PayerRegistrationModal` |

---

## 5. Endpoint yang dikonsumsi

| Operasi | Method | Path | Permission | Dipakai di |
|---|---|---|---|---|
| Daftar asuransi pasien | `GET` | `/patient-insurances/admin` | `PatientInsurance : Read` | `fetchInpatientPatientInsurances` |
| Daftar penjamin perusahaan | `GET` | `/patient-company-guarantors/admin` | `PatientCompanyGuarantor : Read` | `fetchInpatientPatientCompanyGuarantors` |
| Buat asuransi pasien | `POST` | `/patient-insurances/admin` | `PatientInsurance : Create` | `createInpatientPatientInsurance` |
| Buat penjamin perusahaan | `POST` | `/patient-company-guarantors/admin` | `PatientCompanyGuarantor : Create` | `createInpatientPatientCompanyGuarantor` |
| Opsi provider asuransi | `GET` | `/administrator/master-data/insurance-providers/admin/options` | `InsuranceProvider : Read` | `fetchInpatientInsuranceProviderOptions` |
| Opsi perusahaan penjamin | `GET` | `/administrator/master-data/company-guarantors/admin/options` | `CompanyGuarantor : Read` | `fetchInpatientCompanyGuarantorOptions` |
| Opsi kelas perawatan | `GET` | `/health-services/master-data/patient-classes/options` | `PatientClass : Read` | `fetchInpatientPatientClassOptions` |

Semua endpoint pasien memakai varian `/admin` — konsisten dengan keputusan `FE-RWI-023`. Route kiosk (`KioskRead`) tidak dipakai karena petugas admisi biasa tidak memiliki policy kiosk.

---

## 6. Gerbang skema

### `RWI-UI-GAP-002` — penjamin perusahaan

**Status untuk FE-RWI-024: TIDAK MEMBLOKIR.**

Gap ini menyangkut `POST /patient-encounters` yang belum membawa payer perusahaan — milik `FE-RWI-025`. Langkah Pembayaran hanya melakukan pendaftaran dan pemilihan kartu ke `patient-insurances` dan `patient-company-guarantors`, yang controllernya lengkap. Ketiga kartu cara bayar (Tunai, Asuransi, Penjamin Perusahaan) sudah tersedia.

Gap tetap terbuka untuk `FE-RWI-025` dan `FE-RWI-035`.

---

## 7. Validasi

| Langkah | Command | Hasil |
|---|---|---|
| Lint | `npm run lint:errors` | EXIT 0 — tanpa error |
| Build | `npm run build` | `✓ Compiled successfully in 252.5s` — tanpa error |
| Test otomatis | `SKIPPED (opsional)` — per instruksi user; repository tidak memakai Jest |
| Verifikasi manual | `NOT FEASIBLE` — tidak tersedia sesi login petugas admisi pada environment development |

### Git status

```
 M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/style/health-services/inpatient-management/inpatient-admission.module.css
?? src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-payment.jsx
?? src/lib/services/health-services/inpatient-management/inpatient-admission-payment.service.js
?? src/utils/health-services/inpatient-management/inpatient-admission-payment-utils.jsx
```

---

## 8. Risiko

| # | Risiko | Dampak | Mitigasi |
|---|---|---|---|
| 1 | Daftar penjamin diambil klien-side (`GET /admin` + filter `patientId`), bukan dari endpoint opsi `/options/{patientId}` yang punya varian admin | Performa bisa lebih lambat bila pasien punya banyak kartu | `pageSize=50` sudah mencukupi sebagian besar kasus. Bila endpoint opsi admin per pasien tersedia kelak, migrasi hanya ganti satu baris service |
| 2 | Kelas perawatan memakai `PatientClass` (`patient-classes/options`) dengan filter `isForInpatient=true`, bukan `ServiceClass` | `ServiceClass` mungkin maksud yang lain di domain rawat inap | `PatientClass` sudah menyediakan filter rawat inap dan dipakai oleh `05-skema-tampilan.md` |
| 3 | Data `selectedPayer` hilang bila user refresh halaman | Flow sudah menyimpan step dan `patientId` di URL, tapi payer belum | Payer perlu dipersistenkan di URL atau session storage pada iterasi berikut |

---

## 9. Langkah berikutnya

`FE-RWI-025` — Kunjungan dan episode `Draft` terbentuk beserta penjaminnya. Task ini akan mengambil data penjamin yang dipilih pada langkah Pembayaran dan mengirimnya lewat `POST /patient-encounters`.
