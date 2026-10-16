# PLAN-REPAIR-020 — Penyelarasan Input Tanggal Lahir Pencarian Pasien Lama Menggunakan Base Component FilterDatePicker

```yaml
plan_id: PLAN-REPAIR-EPS-020
issue: ../issue/issue-020-pencarian-pasien-lama-date-picker.md
status_rencana: DISETUJUI_DAN_DISELESAIKAN
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "User / Product Owner"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "3e43a14c (MHamzah)"
basis_source_frontend: "632351ead (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup Temuan | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | :---: | :---: | --- |
| `FIX-EPS-020-01` | `ISS-EPS-020-01`, `ISS-EPS-020-02`, `ISS-EPS-020-03` | Mengganti elemen native `<input type="date">` pada `InpatientAdmissionExistingPatientSplitStep` dengan base component `FilterDatePicker` ber-wrapper `.searchDatePickerWrapper`, lengkap dengan batasan `max`, placeholder yang seragam, dan auto-submit | FE | 1 | — | ✅ SELESAI | `tests/unit/inpatient-existing-patient-search.test.mjs` (5 test lolos) |

**Ringkasan: 1 dari 1 perbaikan selesai.**

---

## 2. Solusi Terpilih Per Temuan

### Temuan `ISS-EPS-020-01` s/d `ISS-EPS-020-03` — Penggantian Input Tanggal Lahir

| Opsi Solusi | Kelebihan | Kekurangan | Putusan |
| --- | --- | --- | :---: |
| **Opsi 1: Pertahankan `<input type="date">` dengan kustomisasi CSS tambahan** | Tidak mengubah struktur DOM JSX saat ini | Widget kalender tetap milik sistem operasi/browser (Chromium OS); navigasi tahun mundur puluhan tahun tetap lambat dan tidak ramah pengguna; menyimpang dari aturan konstitusi Quilvian | **DITOLAK** |
| **Opsi 2: Terapkan Base Component `FilterDatePicker` (`@/components/features/base-features/filter-date-picker`)** | 100% konsisten dengan desain sistem Quilvian; dropdown bulan Bahasa Indonesia dan dropdown tahun yang dapat dicari (*searchable* hingga 100 tahun ke belakang); mendukung batasan `max={toDateInputValue(new Date())}`; styling `.searchDatePickerWrapper` sudah tersedia rapi di CSS; memicu auto-search seketika tanggal dipilih | Memerlukan kondisional render di dalam form row split step (persis seperti yang sudah dilakukan di langkah non-split) | **DIPILIH** |

**Alasan Pemilihan Opsi 2:**
Komponen `FilterDatePicker` adalah standar tunggal pemilihan tanggal di frontend Quilvian yang sudah teruji pada berbagai modul (Rawat Jalan, IGD, Farmasi, Keuangan). Kode pendukung, utilitas konversi tanggal, dan styling CSS `.searchDatePickerWrapper` sudah siap pakai di berkas yang bersangkutan, sehingga perubahan bersifat bersih, aman, dan tanpa efek samping pada logika bisnis admisi.

---

## 3. Skema Tampilan: Sebelum → Sesudah

### A. Rangka Antarmuka Sebelum Perbaikan (Kondisi Saat Ini)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ PENCARIAN PASIEN TERDAFTAR                                             │
│ Pilih metode pencarian lalu masukkan nomor identitas atau tanggal...   │
│                                                                        │
│ [ RM Nomor RM ]  [ ID NIK ]  [ (TL) Tanggal Lahir *Aktif* ]            │
│                                                                        │
│ ┌──────────────────────────────────────────────────────────┐ ┌───────┐ │
│ │ 🔍  dd/mm/yyyy (Input Native Browser)                 📅 │ │ Cari  │ │
│ └──────────────────────────────────────────────────────────┘ └───────┘ │
│   ▲                                                                    │
│   │ [Muncul Popover Kalender Bawaan Chromium]                          │
│   │ ┌─────────────────────────────────┐                                │
│   │ │ October 2026   ▲ ▼              │                                │
│   │ │ Su Mo Tu We Th Fr Sa            │                                │
│   │ │ ... 14 ...                      │                                │
│   │ │ Clear                     Today │                                │
│   │ └─────────────────────────────────┘                                │
│                                                                        │
│ • Pilih tanggal lahir lalu klik Cari atau tekan Enter                  │
└────────────────────────────────────────────────────────────────────────┘
```

### B. Rangka Antarmuka Sesudah Perbaikan (Kondisi Target)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ PENCARIAN PASIEN TERDAFTAR                                             │
│ Pilih metode pencarian lalu masukkan nomor identitas atau tanggal...   │
│                                                                        │
│ [ RM Nomor RM ]  [ ID NIK ]  [ (TL) Tanggal Lahir *Aktif* ]            │
│                                                                        │
│ ┌──────────────────────────────────────────────────────────┐ ┌───────┐ │
│ │ 📅  Pilih tanggal lahir pasien... (FilterDatePicker)   ▾ │ │ Cari  │ │
│ └──────────────────────────────────────────────────────────┘ └───────┘ │
│   ▲                                                                    │
│   │ [Muncul Popover Quilvian Base Feature Component]                   │
│   │ ┌────────────────────────────────────────────────────────┐         │
│   │ │ ‹  [ Oktober       ▾ ]   [ 1995 (Cari tahun...) ▾ ]  › │         │
│   │ │ ────────────────────────────────────────────────────── │         │
│   │ │ Min  Sen  Sel  Rab  Kam  Jum  Sab                      │         │
│   │ │            1    2    3    4    5                       │         │
│   │ │  6    7    8    9   10   11   12                       │         │
│   │ │ ...                                                    │         │
│   │ │ ────────────────────────────────────────────────────── │         │
│   │ │ Hapus                                         Hari ini │         │
│   │ └────────────────────────────────────────────────────────┘         │
│                                                                        │
│ • Pilih tanggal lahir lalu klik Cari atau tekan Enter                  │
└────────────────────────────────────────────────────────────────────────┘
```

### C. Tabel Perbandingan Elemen Layar

| Bagian Layar | Tampilan Sebelum Perbaikan | Tampilan Sesudah Perbaikan |
| --- | --- | --- |
| **Komponen Input** | `<input type="date">` native | `<FilterDatePicker>` base component |
| **Wadah Pembungkus** | `<div className={admissionStyles.searchInputWrapper}>` | `<div className={admissionStyles.searchDatePickerWrapper}>` |
| **Ikon / Aksen Visual** | Kaca pembesar (kiri) + ikon kalender native (kanan) | Ikon kalender / teks tanggal + chevron dropdown `▾` standar Quilvian |
| **Navigasi Pemilihan Tahun** | Terbatas pada kontrol panah bawaan browser per bulan | Dropdown pilihan tahun dengan fitur pencarian teks (*searchable dropdown*) hingga 100 tahun ke belakang |
| **Batasan Tanggal Maksimal** | Tidak dibatasi (bisa memilih tanggal di masa depan) | Dibatasi otomatis (`max={toDateInputValue(new Date())}`) sehingga tanggal masa depan dinonaktifkan |
| **Alur Eksekusi Pencarian** | Wajib menekan tombol "Cari" atau tombol "Enter" secara manual | Otomatis mengeksekusi pencarian saat tanggal selesai diklik, tombol "Cari" tetap tersedia sebagai alternatif manual |

---

## 4. Rincian Perbaikan Teknis (`FIX-EPS-020-01`)

### Target Berkas
* [`src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx#L803-L875)

### Perubahan Kode (Diff Rencana)

Pada komponen `InpatientAdmissionExistingPatientSplitStep`:

```jsx
<<<< SEBELUM (Baris 803-875)
            <form
              className={admissionStyles.searchFormRow}
              onSubmit={(event) => {
                event.preventDefault();
                handleSubmitSearch();
              }}
            >
              <div className={admissionStyles.searchInputWrapper}>
                <span className={admissionStyles.searchIcon} aria-hidden="true">
                  <svg ...>...</svg>
                </span>

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
                />

                {searchKeyword ? (
                  <button
                    type="button"
                    className={admissionStyles.searchClearButton}
                    ...
                  >
                    &times;
                  </button>
                ) : null}
              </div>

              <BaseButton ...>Cari</BaseButton>
            </form>

==== SESUDAH
            <form
              className={admissionStyles.searchFormRow}
              onSubmit={(event) => {
                event.preventDefault();
                handleSubmitSearch();
              }}
            >
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
                  <span className={admissionStyles.searchIcon} aria-hidden="true">
                    <svg ...>...</svg>
                  </span>

                  <input
                    type="search"
                    className={admissionStyles.searchInputField}
                    value={searchKeyword}
                    placeholder={
                      searchMode ===
                      INPATIENT_EXISTING_PATIENT_SEARCH_MODE.IDENTITY_NUMBER
                        ? "Ketik 16 digit NIK pasien..."
                        : "Ketik nomor rekam medis (contoh: 00-00-00-08 atau 08)..."
                    }
                    aria-label={
                      searchMode ===
                      INPATIENT_EXISTING_PATIENT_SEARCH_MODE.IDENTITY_NUMBER
                        ? "Ketik 16 digit NIK pasien"
                        : "Ketik nomor rekam medis pasien"
                    }
                    maxLength={
                      searchMode ===
                      INPATIENT_EXISTING_PATIENT_SEARCH_MODE.IDENTITY_NUMBER
                        ? 16
                        : undefined
                    }
                    inputMode={
                      searchMode ===
                      INPATIENT_EXISTING_PATIENT_SEARCH_MODE.IDENTITY_NUMBER
                        ? "numeric"
                        : undefined
                    }
                    autoComplete="off"
                    onChange={(event) => handleSearchKeywordChange(event.target.value)}
                  />

                  {searchKeyword ? (
                    <button
                      type="button"
                      className={admissionStyles.searchClearButton}
                      aria-label="Hapus teks pencarian"
                      title="Hapus teks pencarian"
                      onClick={handleResetSearch}
                    >
                      &times;
                    </button>
                  ) : null}
                </div>
              )}

              <BaseButton ...>Cari</BaseButton>
            </form>
>>>>
```

---

## 5. Spesifikasi API Endpoint (Bergaya Swagger)

### [Tags("Health Services / Patient Management / Master Data / Patients")]

Pencarian pasien lama dengan parameter tanggal lahir mengonsumsi endpoint master data pasien backend yang telah ada:

| Properti | Nilai Spesifikasi |
| :--- | :--- |
| **Metode HTTP** | `GET` |
| **Path** | `/api/v1/health-services/patient-management/master-data/patients/admin` |
| **Deskripsi** | Mengambil daftar pasien terdaftar aktif berdasarkan kriteria pencarian (No. Rekam Medis, NIK, atau Tanggal Lahir). |
| **Otorisasi (Auth)** | Bearer JWT Token (`Patient:Read` via policy RBAC admin) |
| **Format Respons** | `application/json` |

#### Parameter Permintaan (Query Parameters):

| Nama Parameter | Tipe | Wajib | Keterangan | Contoh Nilai |
| --- | :--- | :---: | --- | --- |
| `birthDate` | `string` (ISO date) | Opsional | Tanggal lahir pasien yang difilter | `1990-01-01` |
| `medicalRecordNumber`| `string` | Opsional | Nomor rekam medis pasien | `00-00-00-08` |
| `identityNumber` | `string` | Opsional | 16 digit NIK pasien | `3273010101900001` |
| `search` | `string` | Opsional | Kata kunci bebas (nama/RM/NIK) | `Budi` |
| `isActive` | `boolean` | Opsional | Status aktif pasien | `true` |
| `pageNumber` | `integer` | Opsional | Nomor halaman paginasi | `1` |
| `pageSize` | `integer` | Opsional | Ukuran data per halaman | `10` |
| `sortBy` | `string` | Opsional | Kolom pengurutan | `createDateTime` |
| `sortDirection` | `string` | Opsional | Arah pengurutan | `desc` |

#### Contoh Respons Berhasil (`200 OK`):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data pasien berhasil diambil.",
  "data": {
    "items": [
      {
        "id": "4110016b-7535-4b31-826d-56436f297965",
        "medicalRecordNumber": "00-00-00-08",
        "patientCode": "P-0008",
        "fullName": "Budi Santoso",
        "identityNumber": "3273010101900001",
        "birthDate": "1990-01-01T00:00:00Z",
        "gender": 1,
        "genderName": "Laki-laki",
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalData": 1,
    "totalPage": 1
  },
  "errors": null,
  "timestamp": "2026-10-14T20:50:00.000Z"
}
```

---

## 6. Urutan Pengerjaan & Dependency

```text
Persetujuan Pemilik (K-01)
  │
  ▼
FIX-EPS-020-01: Update JSX pada InpatientAdmissionExistingPatientSplitStep
  │
  ▼
Eksekusi Uji Unit: tests/unit/inpatient-existing-patient-search.test.mjs
  │
  ▼
Verifikasi Antarmuka (Browser/Lint/Build Check)
  │
  ▼
Penyelesaian Register & Laporan Selesai
```

---

## 7. Keputusan Pemilik yang Dibutuhkan

* **K-01**: Konfirmasi persetujuan untuk mengubah JSX `InpatientAdmissionExistingPatientSplitStep` agar mengintegrasikan `FilterDatePicker` menggantikan tag `<input type="date">`.

---

## 8. Dokumen Hulu yang Terkait

* [03-frontend-architecture.md](../03-frontend-architecture.md) — Bab komponen admisi pasien lama.
* [05-skema-tampilan.md](../05-skema-tampilan.md) — Bagian 3.1 & 3.4 Langkah 1 Pencarian & Verifikasi Pasien.

---

## 9. Verifikasi Menyeluruh (Acceptance Criteria)

1. **AC-01 (Komponen & Desain)**: Ketika tab "TL Tanggal Lahir" dipilih, ruas isian menampilkan trigger `FilterDatePicker` ber-placeholder *"Pilih tanggal lahir pasien..."* dengan tinggi dan styling yang harmonis berdampingan dengan tombol "Cari".
2. **AC-02 (Popover Quilvian)**: Mengklik ruas tanggal membuka popover kalender Quilvian lengkap dengan navigasi nama bulan (Indonesia) dan dropdown pemilihan tahun yang mendukung pencarian tahun lahir mundur puluhan tahun.
3. **AC-03 (Batasan Tanggal)**: Tanggal di masa depan (melebihi hari ini) dalam keadaan nonaktif (*disabled*) dan tidak dapat dipilih.
4. **AC-04 (Auto Search & Manual Search)**:
   * Mengklik salah satu tanggal valid di popover langsung mengisi keyword dan memicu eksekusi pencarian pasien ke server.
   * Menekan tombol "Cari" atau tombol "Enter" tetap dapat menjalankan pencarian ulang menggunakan tanggal yang tertera.
5. **AC-05 (Tombol Bersihkan/Reset)**: Mengklik tombol silang "×" atau "Hapus" pada kalender mereset tanggal dan mengosongkan hasil pencarian.
6. **AC-06 (Uji Regresi)**: Mode pencarian Nomor RM dan NIK tetap berfungsi normal tanpa perubahan perilaku. Unit test `node --test tests/unit/inpatient-existing-patient-search.test.mjs` lulus 100%.

---

## 10. Riwayat

| Tanggal | Versi | Penulis | Perubahan |
| :---: | :---: | :---: | --- |
| 2026-10-14 | 1.0.0 | AI Antigravity / Tim Rekayasa Quilvian | Penyusunan rencana perbaikan awal untuk penyeragaman date picker pencarian pasien lama. |
