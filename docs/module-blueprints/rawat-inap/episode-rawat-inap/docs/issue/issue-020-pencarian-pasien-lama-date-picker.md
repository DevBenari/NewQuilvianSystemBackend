# ISSUE-020 — Input Tanggal Lahir pada Pencarian Pasien Lama Menggunakan Elemen Native HTML5 dan Tidak Mengikuti Base Component Date Picker

```yaml
issue_id: ISSUE-EPS-020
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 1 Pencarian & Verifikasi Pasien Lama (InpatientAdmissionExistingPatientSplitStep)"
sumber_laporan: "Laporan pengguna: tolong perbaiki pencarian pasien lama ketika tanggal lahir, component tanggal lahir ikuti base component ya date picker yang ada disini ya (dilengkapi tangkapan layar popup kalender native browser)"
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "3e43a14c (MHamzah)"
source_sha_frontend: "632351ead (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-020-pencarian-pasien-lama-date-picker.md
```

---

## 1. Ringkasan Masalah

Pada modul **Pelayanan Rawat Inap (Inpatient Management)**, layar admisi pasien baru dan lama menyajikan formulir pencarian terintegrasi (*Split Screen Step 1*). Ketika petugas admisi memilih metode pencarian **Tanggal Lahir (TL)** untuk menemukan rekam medis pasien terdaftar, ruas isian tanggal saat ini dirender menggunakan elemen native browser `<input type="date">` yang dibungkus di dalam wadah teks pencarian umum (`searchInputWrapper`).

Hal ini menyebabkan browser (khususnya Chromium / Google Chrome / Microsoft Edge) memunculkan *widget popover* kalender bawaan sistem operasi / browser. Popover bawaan tersebut membuka bulan dan tahun berjalan (Oktober 2026) tanpa fasilitas pemilihan tahun mundur yang cepat (misalnya untuk pasien kelahiran 1960–2000 petugas harus berpindah bulan berulang kali), tampilannya tidak seragam dengan *design tokens* antarmuka Quilvian, serta menyimpang dari standar pustaka komponen dasar (*base component*) Quilvian di mana seluruh komponen pemilihan tanggal wajib memanfaatkan `FilterDatePicker` (`@/components/features/base-features/filter-date-picker`).

Komponen `FilterDatePicker` sendiri sebenarnya telah terpasang dan digunakan pada varian langkah pencarian lama (`InpatientAdmissionExistingPatientSearchStep`), tetapi terlewat saat penyusunan langkah belah layar terpadu (`InpatientAdmissionExistingPatientSplitStep`).

---

## 2. Laporan Asli Pengguna

| No. | Kalimat Pelapor | Konteks Layar & Bukti Lampiran |
| :---: | --- | --- |
| 1 | *"coba anda analisis dan berikan planing"* | Permintaan analisis akar masalah dan rencana kerja komprehensif |
| 2 | *"tolong perbaiki pencarian pasien lama ketika tanggal lahir"* | Formulir *Pencarian Pasien Terdaftar* tab `TL Tanggal Lahir` |
| 3 | *"component tanggal lahir ikuti base component ya date picker yang ada disini ya"* | Tangkapan layar menunjukkan popover kalender native browser Chromium bertuliskan *October 2026*, *Clear*, dan *Today* di atas input `dd/mm/yyyy` |

---

## 3. Ringkasan Temuan Masalah

| ID Temuan | No. Lap. | Masalah | Jenis | Area | Keparahan | Status Bukti | Solusi Rencana |
| --- | :---: | --- | :---: | :---: | :---: | :---: | --- |
| `ISS-EPS-020-01` | 2, 3 | Komponen `InpatientAdmissionExistingPatientSplitStep` merender `<input type="date">` native browser saat mode `BIRTH_DATE`, mengabaikan base component `FilterDatePicker` | `UI_INCONSISTENCY` | Frontend View | Medium | TERBUKTI di `inpatient-admission-existing-patient-step.jsx:821-826` | `FIX-EPS-020-01` |
| `ISS-EPS-020-02` | 2 | Pembungkus CSS input tanggal pada split step memakai `.searchInputWrapper` (untuk text/search), bukan `.searchDatePickerWrapper` yang sudah dirancang khusus untuk `FilterDatePicker` | `STYLING_DEFECT` | Frontend CSS/JSX | Low | TERBUKTI di `inpatient-admission-existing-patient-step.jsx:803` dan `inpatient-admission.module.css:331-346` | `FIX-EPS-020-02` |
| `ISS-EPS-020-03` | 2, 3 | Otomatisasi pencarian saat tanggal dipilih (`handleSubmitSearch(cleanDate)`) tidak aktif pada input native, memaksa pengguna menekan tombol "Cari" atau "Enter" secara manual | `UX_FRICTION` | Frontend Logic | Low | TERBUKTI di `inpatient-admission-existing-patient-step.jsx:860` | `FIX-EPS-020-03` |

---

## 4. Rincian dan Analisis Akar Masalah

### Temuan 1: Penggunaan `<input type="date">` Alih-Alih `FilterDatePicker` (`ISS-EPS-020-01`)

* **No. Laporan**: 2, 3
* **Jenis**: `UI_INCONSISTENCY`
* **Area**: Frontend View (`src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx`)
* **Keparahan**: Medium
* **Status Bukti**: SUDAH-VERIFIKASI pada source frontend `632351ead`
* **Perbaikan**: `FIX-EPS-020-01`

#### Apa yang terjadi
Ketika petugas pendaftaran memilih tombol pill **TL (Tanggal Lahir)** pada kartu pencarian pasien lama, ruas input berubah menjadi kolom input bertipe tanggal HTML5 (`type="date"`). Saat diklik, browser Chromium memunculkan popover kalender bawaan browser dengan navigasi bulan panah atas-bawah, bukan kalender bergaya Quilvian yang memiliki dropdown pilihan bulan dan tahun yang dapat dicari (*searchable year dropdown*).

#### Kenapa terjadi
Pada berkas [`inpatient-admission-existing-patient-step.jsx`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx#L820-L826), komponen `InpatientAdmissionExistingPatientSplitStep` mengimplementasikan ruas pencarian tunggal dengan percabangan tipe inline:

```jsx
// inpatient-admission-existing-patient-step.jsx baris 820-826
<input
  type={
    searchMode ===
    INPATIENT_EXISTING_PATIENT_SEARCH_MODE.BIRTH_DATE
      ? "date"
      : "search"
  }
  className={admissionStyles.searchInputField}
  value={searchKeyword}
...
```

Padahal, pada komponen varian non-split di berkas yang sama ([`InpatientAdmissionExistingPatientSearchStep`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx#L133-L149)), pengembang sebelumnya sudah mengintegrasikan `FilterDatePicker` dengan benar:

```jsx
// inpatient-admission-existing-patient-step.jsx baris 133-149
{searchMode === INPATIENT_EXISTING_PATIENT_SEARCH_MODE.BIRTH_DATE ? (
  <div className={admissionStyles.searchDatePickerWrapper}>
    <FilterDatePicker
      value={searchKeyword}
      placeholder="Pilih tanggal lahir pasien..."
      max={toDateInputValue(new Date())}
      disabled={searchLoading}
      className={admissionStyles.searchDatePicker}
      onChange={(nextDate) => {
        const cleanDate = nextDate || "";
        handleSearchKeywordChange(cleanDate);
        if (cleanDate) {
          handleSubmitSearch(cleanDate);
        }
      }}
    />
  </div>
) : (
  <div className={admissionStyles.searchInputWrapper}>
...
```

Saat transisi layar ke layout belah (*split screen*, FE-RWI-224), percabangan ini terlewat dan disederhanakan secara keliru menjadi tag `<input type="date">`.

#### Apakah ini menyimpang dari desain?
Ya, ini adalah `BUG` / `UI_INCONSISTENCY`. Panduan komponen dasar Quilvian (`rules/frontend/base-component-catalog.md` dan `base-component-decision-gate.md`) menetapkan bahwa seluruh pemilihan tanggal harus memakai komponen resmi `FilterDatePicker` (`@/components/features/base-features/filter-date-picker`) agar tata letak, warna tema, perilaku popover/portal, serta aksesibilitas tetap konsisten.

#### Dampak nyata di lapangan
1. **Efisiensi Petugas Terganggu**: Pasien dewasa/lansia yang lahir pada tahun 1960–1980 sulit dicari karena petugas harus memundurkan kalender native browser yang sangat melelahkan jika navigasi tahunnya tidak intuitif.
2. **Estetika Pecah**: Tampilan popover abu-abu/biru native Windows/Chromium kontras dan tidak menyatu dengan visual modern toska/putih Quilvian.
3. **Format Tanggal Ambigu**: Format visual bergantung pada locale browser pengguna (misal `mm/dd/yyyy` vs `dd/mm/yyyy`), padahal API backend menuntut format baku `YYYY-MM-DD`.

#### Rekomendasi
Ganti elemen `<input type="date">` pada `InpatientAdmissionExistingPatientSplitStep` dengan kondisional render yang memuat `FilterDatePicker`, persis seperti yang telah diterapkan pada `InpatientAdmissionExistingPatientSearchStep`.

---

### Temuan 2: Penggunaan Wrapper CSS yang Tidak Sesuai (`ISS-EPS-020-02`)

* **No. Laporan**: 2
* **Jenis**: `STYLING_DEFECT`
* **Area**: Frontend View & CSS
* **Keparahan**: Low
* **Status Bukti**: SUDAH-VERIFIKASI pada `inpatient-admission.module.css:331-346`
* **Perbaikan**: `FIX-EPS-020-02`

#### Apa yang terjadi
Pada mode `BIRTH_DATE`, kontainer pembungkus tetap menggunakan class `.searchInputWrapper` yang diperuntukkan bagi text input (memuat ikon kaca pembesar dan tombol reset silang di dalam bingkai). Hal ini menyebabkan konflik penataan flexbox dan padding jika `FilterDatePicker` diletakkan di dalamnya.

#### Kenapa terjadi
CSS module `inpatient-admission.module.css` sebenarnya telah memiliki aturan khusus:
```css
/* inpatient-admission.module.css:331-346 */
.searchDatePickerWrapper {
  flex: 1;
  min-width: 0;
  --filter-date-height: 48px;
  --filter-date-radius: var(--radius-lg, 12px);
  --filter-date-border: var(--color-border);
  --filter-date-bg: var(--color-surface);
}

.searchDatePickerWrapper button {
  border-width: 1.5px;
}

.searchDatePicker {
  width: 100%;
}
```
Aturan ini sudah mengatur tinggi 48px, border radius yang selaras dengan tombol "Cari", serta penyesuaian tombol trigger `FilterDatePicker`. Namun karena `InpatientAdmissionExistingPatientSplitStep` tidak menggunakan wrapper ini, styling tersebut menganggur.

#### Rekomendasi
Terapkan kondisional pembungkus: gunakan `.searchDatePickerWrapper` saat mode `BIRTH_DATE`, dan pertahankan `.searchInputWrapper` saat mode `MEDICAL_RECORD` atau `IDENTITY_NUMBER`.

---

### Temuan 3: UX Interaksi Pemilihan Tanggal Tanpa Auto-Submit (`ISS-EPS-020-03`)

* **No. Laporan**: 2, 3
* **Jenis**: `UX_FRICTION`
* **Area**: Frontend Logic
* **Keparahan**: Low
* **Status Bukti**: SUDAH-VERIFIKASI pada `use-inpatient-admission-patient.jsx`
* **Perbaikan**: `FIX-EPS-020-03`

#### Apa yang terjadi
Pada input tanggal saat ini, setelah pengguna memilih tanggal di kalender native, pengguna masih harus mengklik tombol "Cari" secara manual. Padahal pada `FilterDatePicker` di `InpatientAdmissionExistingPatientSearchStep`, pemilihan tanggal dari popover langsung memicu `handleSubmitSearch(cleanDate)` sehingga hasil pencarian pasien langsung muncul seketika tanpa perlu klik tambahan.

#### Rekomendasi
Pada properti `onChange` dari `FilterDatePicker`, panggil `handleSearchKeywordChange(cleanDate)` dan jika `cleanDate` terisi valid, langsung jalankan `handleSubmitSearch(cleanDate)`.

---

## 5. Tanya-Jawab Pelapor

| Pertanyaan Pelapor | Jawaban Berbasis Bukti Teknis |
| --- | --- |
| *"Kenapa tampilan kalendernya seperti kalender bawaan komputer/browser?"* | Karena pada komponen belah layar admisi (`InpatientAdmissionExistingPatientSplitStep`), elemen input di-render dengan atribut `<input type="date">`. Browser mendeteksi tipe tersebut dan menampilkan kontrol kalender internal browser bawaan (Chromium OS datepicker), bukan komponen kustom aplikasi. |
| *"Apakah di Quilvian ada komponen date picker bawaan yang sudah standar?"* | Ya, Quilvian memiliki komponen dasar `FilterDatePicker` yang terletak di `@/components/features/base-features/filter-date-picker`. Komponen ini dilengkapi pemilihan bulan (Bahasa Indonesia) dan tahun yang dapat dicari/diketik, batasan tanggal maksimal (`max`), serta popover yang ramah pengguna. |
| *"Apakah format pengiriman data ke server terpengaruh?"* | Tidak terpengaruh. Hook `useInpatientAdmissionPatient` dan backend `PatientController` sama-sama menuntut format ISO date `YYYY-MM-DD`. `FilterDatePicker` secara bawaan menghasilkan format `YYYY-MM-DD` yang presisi. |

---

## 6. Temuan Tambahan (Audit Penunjang)

1. **Atribut Batasan Tanggal Maksimal (`max`)**:
   Pencarian pasien lama berdasarkan tanggal lahir tidak boleh menerima tanggal lahir di masa depan. `FilterDatePicker` mendukung prop `max={toDateInputValue(new Date())}` untuk mencegah pemilihan tanggal yang melampaui hari ini. Input native sebelumnya tidak memasang atribut `max`.
2. **Konsistensi Dua Komponen Langkah**:
   Terdapat dua komponen pencarian pasien lama pada berkas yang sama: `InpatientAdmissionExistingPatientSearchStep` (langkah wizard tunggal lama) dan `InpatientAdmissionExistingPatientSplitStep` (langkah aktif saat ini). Pembaruan pada `InpatientAdmissionExistingPatientSplitStep` akan menyelaraskan kedua implementasi secara total tanpa meninggalkan disparitas kode.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

* **Keputusan K-01 (Pemilik / User)**: Menyetujui penggantian ruas input native `<input type="date">` pada `InpatientAdmissionExistingPatientSplitStep` dengan base component `FilterDatePicker` beserta aktivasi auto-search saat tanggal dipilih.

---

## 8. Catatan Pola dan Pencegahan

* **Pola Pencegahan**: Setiap kali menyusun atau merombak komponen antarmuka (*refactoring to split-view / new layout*), hindari penggunaan elemen form native HTML5 (`<input type="date">`, `<select>`, dll.) secara langsung. Selalu gunakan pustaka *Base Feature Component* Quilvian (`FilterDatePicker`, `FilterSelect`, `BaseFormControl`, `BaseButton`) untuk menjaga konsistensi desain sistem.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Penulis | Perubahan |
| :---: | :---: | :---: | --- |
| 2026-10-14 | 1.0.0 | AI Antigravity / Tim Rekayasa Quilvian | Penerbitan dokumen analisis issue awal berdasarkan laporan pengguna dan tangkapan layar kalender native browser. |
