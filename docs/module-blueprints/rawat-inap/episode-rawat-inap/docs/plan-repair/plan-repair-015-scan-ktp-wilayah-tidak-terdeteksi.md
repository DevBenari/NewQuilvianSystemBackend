# PLAN-REPAIR-015 — Perbaikan Deteksi dan Masuknya Wilayah Hasil Scan e-KTP ke Formulir Admisi Pasien Baru

```yaml
plan_id: PLAN-REPAIR-EPS-015
issue: ../issue/issue-015-scan-ktp-wilayah-tidak-terdeteksi.md
status_rencana: DISETUJUI_DAN_DISELESAIKAN
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "User / Product Owner"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
```

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-015-01` | `ISS-EPS-015-01` | Teruskan `searchText` asli ke penilai skor `findBestRegionOption` pada `emergency-region.service.js` agar indikator "Kota" / "Kabupaten" tidak hilang dan meloloskan uji unit `FE-RWI-204` | FE | 1 | — | SELESAI | Uji unit `inpatient-admission-registration-issue-003.test.mjs` lulus 9/9 |
| `FIX-EPS-015-02` | `ISS-EPS-015-02` | Setel `__scanResolvedRegion` ke form state pada `use-inpatient-admission-patient.jsx` dan `use-outpatient-registration.js` agar opsi dropdown langsung terisi | FE | 1 | — | SELESAI | `setValue("__scanResolvedRegion", ...)` terpasang di kedua hook admisi |
| `FIX-EPS-015-03` | `ISS-EPS-015-03` | Teruskan prop `selectedOption` pada `new-patient-form.jsx` ke masing-masing `EmergencySelectField` agar label nama wilayah langsung dirender di layar | FE | 1 | — | SELESAI | Prop `selectedOption` dipetakan ke 5 field wilayah di `new-patient-form.jsx` |
| `FIX-EPS-015-04` | `ISS-EPS-015-04` | Lengkapi alias OCR kelurahan (`"Desa"`, `"Desa/Kelurahan"`) dan awalan provinsi (`"PROV."`, `"PROV "`) pada `emergency-registration.utils.js` | FE | 2 | — | SELESAI | Uji unit `emergency-registration-scanned-region.test.mjs` lulus 5/5 |
| `FIX-EPS-015-05` | `ISS-EPS-015-05` | Tambahkan mekanisme fallback pencarian balik (*reverse lookup*) dari kelurahan/kode pos bila penentuan kota/kecamatan ambigu | FE | 2 | — | SELESAI | Fallback reverse lookup terintegrasi di `resolveEmergencyScannedRegion` |

**Ringkasan: 5 dari 5 perbaikan selesai.**

---

## 2. Solusi Terpilih Per Temuan

### ISS-EPS-015-01 — Indikator Jenis Wilayah Hilang Akibat `cleanSearch` Diteruskan ke Penilai Skor

| Opsi | Cara Kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Biarkan `cleanSearch` dan ubah regex `getCityTypeKey`)** | Tetap mengirimkan `cleanSearch`, namun mencoba menebak jenis kota dari parameter lain. | Perubahan minimal pada satu baris. | Sangat rapuh; kata kunci "KOTA" sudah hilang sebelum masuk ke fungsi scoring. |
| **Opsi 2 (Kirim `searchText` Asli ke `findBestRegionOption` - Terpilih)** | Pada `findRegionOption` di `emergency-region.service.js:450`, tetap gunakan `cleanSearch` untuk query pencarian ke backend (agar query SQL tidak gagal), tetapi kirimkan `searchText` asli (yang masih memiliki kata "KOTA" atau "KABUPATEN") ke `findBestRegionOption(items, searchText, type)`. | Langsung menyelesaikan kegagalan uji unit `inpatient-admission-registration-issue-003.test.mjs:80-87`. Kota Bekasi dan Kabupaten Bekasi terbedakan secara akurat (+300 bobot tipe), rantai pencarian kecamatan tidak terputus. | Tidak ada. |

**SOLUSI TERPILIH: Opsi 2.** Mengirimkan teks asli hasil OCR ke evaluator pembobotan wilayah sehingga konteks jenis wilayah tetap utuh.

---

### ISS-EPS-015-02 — Sinkronisasi State Hasil Scan Wilayah ke Hook Admisi

| Opsi | Cara Kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya mengandalkan pemanggilan API asinkron per event `onChange`)** | Biarkan hanya UUID yang diset, lalu trigger `useEffect` manual untuk memuat ulang opsi satu per satu. | Tidak perlu menyimpan objek ke state form. | Memerlukan waktu tunggu loading 5x request bertingkat; dropdown berkedip dan berisiko blank jika urutan respon tidak teratur. |
| **Opsi 2 (Setel `__scanResolvedRegion` seperti Pola IGD - Terpilih)** | Di `use-inpatient-admission-patient.jsx` dan `use-outpatient-registration.js`, panggil `setValue("__scanResolvedRegion", region.resolved)` saat hasil scan KTP berhasil dicocokkan. | Menggunakan pola baku yang sudah terbukti di IGD; hook `useEmergencyRegionOptions` otomatis menyuntikkan opsi wilayah ke dropdown seketika tanpa jeda request. | Memerlukan penambahan baris `setValue` di kedua hook. |

**SOLUSI TERPILIH: Opsi 2.** Memasang `__scanResolvedRegion` pada kedua hook admisi agar formulir langsung memiliki opsi wilayah aktif.

---

### ISS-EPS-015-03 — Penerusan Prop `selectedOption` pada `NewPatientForm`

| Opsi | Cara Kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya mengandalkan pencocokan `value` di dalam `options`)** | Biarkan `EmergencySelectField` mencari label semata-mata dari array `options`. | Tidak perlu mengubah properti form. | Jika array opsi masih kosong atau sedang fetching, label nama wilayah langsung hilang dan kembali menjadi placeholder. |
| **Opsi 2 (Teruskan `selectedOption` Terpilih - Terpilih)** | Pada `new-patient-form.jsx`, teruskan `selectedOption={region.selectedCountryOption}`, `selectedOption={region.selectedProvinceOption}`, `selectedOption={region.selectedCityOption}`, `selectedOption={region.selectedDistrictOption}`, dan `selectedOption={region.selectedPostalCodeOption}` ke masing-masing `EmergencySelectField`. | Menjamin teks label wilayah (misal: "Jawa Barat", "Kota Bandung", "Pameungpeuk") tampil seketika di layar meskipun data opsi lengkap di latar belakang sedang dimuat. Menghilangkan kesan "formulir kosong". | Memerlukan pemetaan prop pada 5 field wilayah di `NewPatientForm`. |

**SOLUSI TERPILIH: Opsi 2.** Memastikan kontrol tampilan menerima objek label terpilih secara eksplisit.

---

### ISS-EPS-015-04 — Penyempurnaan Kamus Alias OCR Kelurahan/Desa dan Awalan Provinsi

| Opsi | Cara Kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya menangani kelurahan di level view)** | Mengubah string alamat di tampilan tanpa memperbaiki utilitas pemetaan data. | Cepat selesai. | Data `__scanRegion` tetap kosong untuk KTP berformat Desa. |
| **Opsi 2 (Perluas Alias Kamus OCR dan Pembersih Awalan - Terpilih)** | 1. Pada `emergency-registration.utils.js:815`, tambahkan alias: `["KelDesa", "Kelurahan", "DesaKelurahan", "Desa", "Village"]`.<br>2. Pada `stripProvincePrefix`, tambahkan pemotongan regex untuk `/^PROV\.?\s+/i`. | Menjangkau variasi e-KTP nasional secara luas, baik warga kota ("Kelurahan") maupun warga kabupaten/desa ("Desa"). | Tidak ada. |

**SOLUSI TERPILIH: Opsi 2.** Memperluas kamus alias OCR agar toleran terhadap berbagai variasi format KTP Indonesia.

---

### ISS-EPS-015-05 — Fallback Pencarian Balik (*Reverse Lookup*)

| Opsi | Cara Kerja | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **Opsi 1 (Hanya hierarki satu arah top-down)** | Pertahankan pencarian `Negara -> Provinsi -> Kota -> Kecamatan -> Kelurahan`. | Kode sederhana. | Jika kota ambigu, kecamatan dan kelurahan di bawahnya pasti gugur. |
| **Opsi 2 (Pencarian Balik Berbasis Kelurahan/Kode Pos - Terpilih)** | Jika pencarian bertingkat menghasilkan kota yang ambigu atau kosong, gunakan teks `village` dan `district` untuk mencari langsung ke endpoint `postal-codes/options`. Bila ditemukan kelurahan yang cocok di bawah provinsi terkait, inferensi balik kota dan kecamatan yang sesuai. | Menyelamatkan data KTP yang kotanya tidak bertipe jelas tetapi kelurahan/kecamatannya sangat unik di provinsi tersebut. Tingkat keberhasilan deteksi otomatis meningkat drastis. | Memerlukan sedikit penambahan logika fallback di service wilayah. |

**SOLUSI TERPILIH: Opsi 2.** Mengadopsi pola cerdas seperti Kiosk untuk memastikan deteksi wilayah tidak terhenti di tengah jalan.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Alur Sebelum Perbaikan (Masalah Terjadi)
```text
PETUGAS MENEKAN TOMBOL "SCAN eKTP" DI ADMISI
  ┌──────────────────────────────────────────────────────────────┐
  │ Status Pemindai: Pemindaian selesai                          │
  │ Preview KTP: [ Gambar e-KTP Tampil ]                        │
  └──────────────────────────────────────────────────────────────┘
                                │
                                ▼
  FORMULIR DOMISILI PASIEN TAMPIL SEPERTI INI:
  ┌──────────────────────────────────────────────────────────────┐
  │ Alamat Lengkap: Jl. Sukasari No. 12, RT 01/RW 02, Kec. ...   │
  │ Negara            : [ Pilih negara                ▼ ]        │  <-- KOSONG!
  │ Provinsi          : [ Pilih negara dahulu         ▼ ]        │  <-- KOSONG & DISABLED!
  │ Kota / Kabupaten  : [ Pilih provinsi dahulu       ▼ ]        │  <-- KOSONG & DISABLED!
  │ Kecamatan         : [ Pilih kota dahulu           ▼ ]        │  <-- KOSONG & DISABLED!
  │ Kelurahan/Kode Pos: [ Pilih kecamatan dahulu      ▼ ]        │  <-- KOSONG & DISABLED!
  └──────────────────────────────────────────────────────────────┘
  AKIBAT: Petugas admisi menyangka scanner rusak / wilayah tidak terbaca,
          lalu terpaksa memilih manual 5 dropdown dari awal.
```

### Alur Sesudah Perbaikan (Solusi Berjalan)
```text
PETUGAS MENEKAN TOMBOL "SCAN eKTP" DI ADMISI
  ┌──────────────────────────────────────────────────────────────┐
  │ Status Pemindai: Scan eKTP berhasil dan data OCR dimasukkan  │
  │ Preview KTP: [ Gambar e-KTP Tampil ]                        │
  └──────────────────────────────────────────────────────────────┘
                                │
                                ▼
  FORMULIR DOMISILI PASIEN LANGSUNG TERISI LENGKAP:
  ┌──────────────────────────────────────────────────────────────┐
  │ Alamat Lengkap: Jl. Sukasari No. 12, RT 01/RW 02, Kec. ...   │
  │ Negara            : [ INDONESIA                   ▼ ]        │  <-- OTOMATIS TERPILIH!
  │ Provinsi          : [ JAWA BARAT                  ▼ ]        │  <-- OTOMATIS TERPILIH!
  │ Kota / Kabupaten  : [ KOTA BEKASI                 ▼ ]        │  <-- OTOMATIS TERPILIH!
  │ Kecamatan         : [ PAMEUNGPEUK                 ▼ ]        │  <-- OTOMATIS TERPILIH!
  │ Kelurahan/Kode Pos: [ SUKASARI - 40376            ▼ ]        │  <-- OTOMATIS TERPILIH!
  └──────────────────────────────────────────────────────────────┘
  HASIL: Petugas admisi TIDAK PERLU menginput ulang; langsung dapat
         melanjutkan ke langkah berikutnya secara cepat dan mulus.
```

---

## 4. Acceptance Criteria

1. **Uji Unit Pencocokan Kota Kembar Lolos**:
   * Menjalankan `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-registration-issue-003.test.mjs` menghasilkan **9 dari 9 lulus (100% pass)** tanpa ada assertion failure pada `FE-RWI-204`.
2. **Keterisian Form Otomatis di Admisi Rawat Inap**:
   * Saat memindai e-KTP di Admisi Rawat Inap Langkah 2, kolom Negara, Provinsi, Kota/Kabupaten, Kecamatan, dan Kelurahan langsung terisi dan menampilkan label teks yang sesuai.
3. **Keterisian Form Otomatis di Pendaftaran Rawat Jalan**:
   * Saat memindai e-KTP pada formulir pendaftaran rawat jalan pasien baru, seluruh dropdown wilayah terisi otomatis sesuai hasil pemindaian.
4. **Toleransi KTP Format Pedesaan**:
   * e-KTP dengan teks baris `"Desa CIPAGANTI"` atau `"PROV. JAWA TENGAH"` berhasil dicocokkan ke master wilayah tanpa terputus.
5. **Penanganan Kasus Tanpa Jenis Tetap Aman**:
   * KTP yang hanya memuat nama ambigu tanpa jenis (misal: "BEKASI" tanpa kata Kota/Kabupaten) tidak ditebak, dan sistem menampilkan pesan notifikasi ramah yang meminta petugas memilih manual, sementara tingkat yang lain tetap tersimpan di memori.

---

## 5. Radius Dampak Perbaikan

* **Berkas yang Diperbaiki**:
  1. `src/lib/services/health-services/registration-management/emergency-region.service.js` (Logika pencocokan dan evaluasi skor wilayah).
  2. `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` (Sinkronisasi `__scanResolvedRegion` di admisi rawat inap).
  3. `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js` (Sinkronisasi `__scanResolvedRegion` di pendaftaran rawat jalan).
  4. `src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx` (Penerusan `selectedOption` ke kontrol dropdown).
  5. `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` (Penambahan alias OCR desa/kelurahan).
* **Radius Pengujian**:
  * Uji unit: `tests/unit/emergency-registration-scanned-region.test.mjs` dan `tests/unit/inpatient-admission-registration-issue-003.test.mjs`.
  * Layar: Admisi Pasien Baru Rawat Inap (`/health-services/inpatient-management/admissions`), Pendaftaran Pasien Baru Rawat Jalan, dan Pendaftaran IGD.
