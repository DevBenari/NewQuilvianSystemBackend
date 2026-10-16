# Laporan Perubahan Frontend — `FE-FIN-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-046` |
| Judul | Registrasi: pilih pegawai dan keluarga, status terdaftar |
| Slice | Slice A — Enrollment & Tanggungan Benefit (`REV-19`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-238`..`241`, `FIN-DEC-198`, `199`, `201`, `204`, `227`, `228`, `229`, `230` |
| Kontrak | `FIN-INTEGRATION-1.8`; `FIN-INTEGRATION-1.9` (approved sisi Finance dan HR); `FIN-API-1.10` R.3 & R.4 |
| Wewenang UI | `03-frontend-architecture.md` §24.1–§24.4 (`FE-FIN-046-A`, wewenang terkunci `FIN-DEC-204` kode hubungan dari sistem dan `FIN-DEC-229` UX form kartu) |
| Dependency | `BE-FIN-105` ✅, `BE-FIN-106` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (services, constants, filter utils, editor utils, detail utils, editor hook, internal benefit component, form view, company guarantor editor config) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-046` mengimplementasikan sub-layar `FE-FIN-046-A` pada pendaftaran dan pengubahan kartu penjamin perusahaan pasien (`MstPatientCompanyGuarantor`), dengan cakupan:
1. **Deteksi Dinamis Penjamin Internal**: Saat pengguna memilih penjamin perusahaan yang memiliki tanda `IsEmployeeBenefitGuarantor = true`, form secara dinamis menampilkan bagian khusus kartu internal "RS Benefit" (`FIN-DEC-229`). Untuk penjamin biasa non-internal, form tetap bekerja dengan isian manual nomor karyawan standar tanpa gangguan (backward compatibility).
2. **Pencarian Pegawai HR Real-Time**: Antarmuka autocomplete pencarian pegawai aktif dengan query minimal 3 karakter ke `GET /employee-benefit-lookups/employees?search=`. Hasil pencarian hanya menampilkan data yang diizinkan (NIP, Nama Lengkap, NIK tersamar `NationalIdMasked`, dan Unit/Departemen) tanpa membocorkan data pribadi HR lainnya (`FIN-DEC-198`).
3. **Pilihan Subjek Yang Berobat**: Pilihan radio:
   - `Pegawai sendiri (SELF)`
   - `Anggota keluarga (FAMILY)`
4. **Dropdown Anggota Keluarga Terdaftar**: Jika memilih anggota keluarga, sistem memanggil `GET /employees/{employeeId}/family-members` untuk menyajikan daftar keluarga aktif beserta kode hubungan dan status verifikasi tanggungan (`FIN-DEC-199`).
5. **Pemeriksaan Kelayakan Real-Time**: Pengecekan otomatis ke `GET /employee-benefit-lookups/eligibility` setiap kali pegawai, target berobat, atau anggota keluarga berubah. Menampilkan lencana status kelayakan:
   - Hijau (`active`): **"Terdaftar dan terverifikasi"** lengkap dengan nama paket benefit dan hubungan keluarga yang diturunkan sistem.
   - Merah (`danger`): **"Belum terdaftar"** dengan pesan penolakan backend verbatim (`FIN-DEC-204`).
6. **Arahan Pendaftaran Pasien Umum**: Bila pasien belum terdaftar/tidak layak di HR, tombol **"Daftarkan sebagai Pasien Umum"** muncul secara jelas dan tombol simpan dinonaktifkan, mencegah penerbitan kartu internal ilegal (`FIN-DEC-204`, AC 3).
7. **Penguncian Kode Hubungan**: Klien tidak pernah mengirim `benefitRelationship` ke backend; field ini ditentukan sepenuhnya oleh server dari hasil verifikasi data HR (`FIN-DEC-204`, `FIN-VAL-260`).
8. **Dukungan Kartu Lama**: Kartu lama yang belum memiliki tautan HR (`employeeId` kosong) tetap dapat dibuka, dibaca, dan ditampilkan di detail view tanpa error (`CARD_NOT_LINKED`, AC 4).
9. **Kotak Centang Penjamin Internal pada Master Penjamin**: Menyediakan switch `isEmployeeBenefitGuarantor` ("Penjamin Internal (Benefit Karyawan)") pada master data penjamin perusahaan (`MstCompanyGuarantor`), menutup kebutuhan `FIN-DEC-229` butir (2).

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 9 elemen — REUSE 7, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Shell halaman form master data | `BaseGroupedEditorView` | `src/components/features/base-features/base-grouped-editor-view.jsx` | `REUSE` | Digunakan apa adanya sebagai struktur halaman form |
| Panel kelompok form editor | `BaseGroupedEditorForm` | `src/components/features/base-features/base-grouped-editor-form.jsx` | `REUSE` | Memakai prop `renderField` dan `renderActions` untuk kontrol kontekstual |
| Field pencarian & autocomplete pegawai | `BaseTextField` / input token | `src/components/features/base-features/base-text-field.jsx` | `COMPOSE` | Dirangkai dengan dropdown hasil pencarian pegawai ber-NIK tersamar |
| Pilihan radio subjek berobat | Radio input form | Scoped CSS Module berbasis design tokens | `COMPOSE` | Dirangkai pada panel kartu internal |
| Lencana status kelayakan HR | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Lencana status `active` (hijau), `danger` (merah), dan `pending` |
| Banner pesan penolakan backend | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Menampilkan pesan sistem backend secara verbatim |
| Tombol aksi Pasien Umum & navigasi | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Tombol `secondary` untuk Pasien Umum dan ganti pegawai |
| Tampilan rincian kartu penjamin | `HealthServicesMasterDataDetailView` | `src/components/view/health-services/master-data/base-view/` | `REUSE` | Ditampilkan dengan baris hubungan manfaat HR dan penjamin |
| Switch penjamin internal di Master Penjamin | `BaseGroupedEditorField` | `src/lib/constants/administrator/master-data/company-guarantor/` | `REUSE` | Tipe input `switch` dengan label "Penjamin Internal (Benefit Karyawan)" |

### Pilihan Bernomor untuk Elemen COMPOSE:

> **Keputusan: Komposisi Komponen Bagian Form Penjamin Internal (`FE-FIN-046-A`)**
> - **A. COMPOSE memakai Base Component yang ada (`BaseTextField`, `BaseButton`, `StatusBadge`, `InformationAlert`) di dalam `PatientCompanyGuarantorInternalBenefitSection` — Rekomendasi.** Menjaga konsistensi visual 100% dengan token sistem Quilvian, tidak memodifikasi base component universal, dan mengisolasi logika pencarian pegawai/keluarga HR pada view dan hook form kartu penjamin.
> - **B. Buat Base Component baru `BaseEmployeeLookup` di `src/components/features/base-features/`.** Mengabstraksi komponen ke level base, namun komponen ini sangat terikat erat pada kontrak domain registrasi dan HR internal benefit, sehingga tidak layak diangkat menjadi primitive universal.
>
> **Dipilih: Opsi A (Rekomendasi).**

---

## 3. Berkas yang Dibuat dan Diubah

### Berkas Baru
1. `src/lib/services/health-services/registration-management/employee-benefit-lookup.service.js` — Service Axios untuk pencarian pegawai (`searchEmployees`), daftar keluarga (`getFamilyMembers`), dan periksa kelayakan (`checkEligibility`).
2. `src/style/health-services/patient-management/patient-company-guarantor-internal-benefit.module.css` — Stylesheet CSS Module berbasis design tokens Quilvian untuk bagian penjamin internal.
3. `src/components/view/health-services/patient-management/master-data/patient-company-guarantors/add/patient-company-guarantor-internal-benefit-section.jsx` — Komponen tampilan interaktif untuk pencarian pegawai HR, pemilihan subjek berobat, pemilihan keluarga, status kelayakan, dan aksi Pasien Umum.

### Berkas yang Diubah
1. `src/lib/constants/health-services/patient-management/master-data/patient-company-guarantor-constants.jsx` — Menambahkan `employeeId`, `familyMemberId`, `benefitRelationship`, `isInternalBenefit`, `benefitTarget`, `BENEFIT_RELATIONSHIP_LABELS`, dan kolom rincian detail.
2. `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-table-filter-utils.jsx` — Mempertahankan flag boolean `isEmployeeBenefitGuarantor` pada daftar opsi penjamin perusahaan.
3. `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-editor-utils.jsx` — Pemetaan data internal benefit pada form, payload save (tanpa `benefitRelationship`), dan validasi kelayakan.
4. `src/utils/health-services/patient-management/master-data/patient-company-guarantor/patient-company-guarantor-detail-utils.jsx` — Pemformatan label hubungan manfaat HR (`BENEFIT_RELATIONSHIP_LABELS`) dan penanganan kartu lama tanpa tautan agar tetap tampil bersih.
5. `src/lib/hooks/health-services/patient-management/master-data/patient-company-guarantors/use-master-data-patient-company-guarantor-editor.jsx` — Pengelolaan state pencarian pegawai ter-debounce, daftar keluarga, pemeriksaan kelayakan real-time, dan pencegahan submit saat tidak layak.
6. `src/components/view/health-services/patient-management/master-data/patient-company-guarantors/add/patient-company-guarantors-form-view.jsx` — Integrasi `renderField` untuk menampilkan bagian penjamin internal secara dinamis dan menonaktifkan ketikan bebas nomor karyawan untuk penjamin internal.
7. `src/lib/constants/administrator/master-data/company-guarantor/company-guarantor-editor-config.jsx` — Menambahkan switch `isEmployeeBenefitGuarantor` ("Penjamin Internal (Benefit Karyawan)") pada konfigurasi form Master Penjamin Perusahaan (`FIN-DEC-229` butir 2).

---

## 4. Pemenuhan Acceptance Criteria

| # | Acceptance Criteria | Bukti Implementasi |
|:--:|---|---|
| 1 | Mengetik "198" menampilkan pegawai yang cocok, tanpa menampilkan data HR di luar NIP, NIK, Nama, unit | `EmployeeBenefitLookupService.searchEmployees` memanggil endpoint backend yang hanya mengembalikan `EmployeeLookupResponse` (NIP, Nama, NIK tersamar `NationalIdMasked`, unit); komponen `PatientCompanyGuarantorInternalBenefitSection` menampilkan data tersebut tanpa data HR sensitif lainnya. |
| 2 | Memilih Sari (terdaftar): kartu bisa disimpan | Saat memilih pegawai dan keluarga Sari yang berstatus terdaftar, `checkEligibility` mengembalikan `isEligible: true`, lencana "Terdaftar dan terverifikasi" aktif, dan form diizinkan untuk disimpan ke backend. |
| 3 | Memilih Pak Andi (tidak terdaftar): simpan ditolak dengan pesan backend dan tombol Pasien Umum muncul | Saat memilih Pak Andi yang belum terdaftar di HR, backend mengembalikan `isEligible: false` beserta pesan penolakan; UI menampilkan lencana merah "Belum terdaftar", menampilkan pesan sistem verbatim lewat `InformationAlert`, memunculkan tombol "Daftarkan sebagai Pasien Umum", dan memblokir aksi simpan. |
| 4 | Kartu lama tanpa tautan tetap tampil dan bisa dibaca | `patient-company-guarantor-detail-utils.jsx` menangani nilai `employeeId` dan `benefitRelationship` kosong secara aman dengan tanda hubung (`-`), memastikan seluruh kartu lama tanpa tautan HR tetap tampil utuh tanpa error. |
| 5 | Master Penjamin Perusahaan dilengkapi penanda penjamin internal (`FIN-DEC-229` butir 2) | Field `isEmployeeBenefitGuarantor` dengan label "Penjamin Internal (Benefit Karyawan)" telah terpasang pada konfigurasi form `MstCompanyGuarantor`. |

---

## 5. Hasil Validasi

- **ESLint**: `npx eslint` pada seluruh 9 berkas target: **PASS (exit code 0, 0 error)**.
- **Grep Anti-Regresi**:
  - Warna literal / `!important` pada CSS baru: **PASS (0 temuan)**.
  - Raw button / raw table pada view baru: **PASS (0 temuan)**.
- **Runtime / Dev Server**: **NOT RUN** (mematuhi instruksi eksplisit pengguna: *"Jangan jalankan npm run dev. Biarkan saya yang running dan test"*).
