# ISSUE-015 — Wilayah Hasil Scan e-KTP Tidak Terdeteksi dan Tidak Masuk ke Formulir Pendaftaran Pasien Baru

```yaml
issue_id: ISSUE-EPS-015
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap (FE-INP-03 / Skema Tampilan 3.3) & Pendaftaran Rawat Jalan — Formulir Pasien Baru"
sumber_laporan: "Laporan pengguna: pada saat scan ktp di admisi pasien baru, ada masalah wilayah tidak terdeteksi dan tidak masuk form input yang membuat admisi harus menginput lagi"
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-015-scan-ktp-wilayah-tidak-terdeteksi.md
```

## 1. Ringkasan

Ketika petugas admisi melakukan pemindaian e-KTP pada alur pendaftaran pasien baru (baik di Admisi Rawat Inap maupun Pendaftaran Rawat Jalan), scanner Plustek dan OCR berhasil memindai fisik KTP serta membaca identitas utama (NIK, Nama, Tempat/Tanggal Lahir, Jenis Kelamin, dan Alamat Bebas). Namun, **seluruh pilihan wilayah (Negara, Provinsi, Kota/Kabupaten, Kecamatan, dan Kelurahan/Kode Pos) tidak terdeteksi atau tidak terpilih pada kontrol dropdown formulir**. 

Kolom-kolom wilayah di layar tetap menampilkan teks placeholder kosong (*"Pilih provinsi"*, *"Pilih kota / kabupaten"*, dst.), sehingga petugas admisi terpaksa mencari dan menginput ulang wilayah satu per satu dari awal secara manual.

Hasil penelusuran membuktikan bahwa masalah ini berakar pada **5 (lima) titik kegagalan teknis**:
1. **Penerusan Teks Bersih (*Clean Search*) Menghilangkan Indikator Kota/Kabupaten (`emergency-region.service.js:450`):** Fungsi `findRegionOption` membersihkan teks OCR dengan membuang kata "KOTA" / "KABUPATEN" sebelum memanggil pencarian backend. Namun teks bersih itu juga yang diteruskan ke `findBestRegionOption(items, cleanSearch || searchText, type)`. Karena `cleanSearch` bernilai "BEKASI", indikator "KOTA" hilang, skor kecocokan antara Kota Bekasi dan Kabupaten Bekasi seri (imbang 1000), algoritma menolak menebak dan mengembalikan `null`. Hal ini dibuktikan langsung oleh kegagalan uji unit `inpatient-admission-registration-issue-003.test.mjs:80-87`.
2. **Ketiadaan Sinkronisasi State Hasil Scan Wilayah (`use-inpatient-admission-patient.jsx:148-157` & `use-outpatient-registration.js:406-414`):** Hook pendaftaran rawat inap dan rawat jalan tidak menyetel `setValue("__scanResolvedRegion", region.resolved)`. Akibatnya hook dropdown `useEmergencyRegionOptions` tidak pernah menerima opsi yang sudah dicocokkan, sehingga daftar pilihan di memori tetap kosong `[]` dan input dropdown di layar tidak dapat merender label nama wilayah.
3. **Komponen Form Tidak Mengirimkan `selectedOption` (`new-patient-form.jsx:291-355`):** Komponen `NewPatientForm` tidak meneruskan prop `selectedOption` yang telah dihitung oleh `useEmergencyRegionOptions` ke `EmergencySelectField`, sehingga kontrol `FilterSelect` jatuh ke placeholder saat opsi asinkron belum selesai dimuat.
4. **Keterbatasan Kamus Kata Kunci OCR (`emergency-registration.utils.js:815-833`):** Kata `"Desa"` (yang lazim pada KTP non-perkotaan) belum masuk dalam daftar alias pembacaan kelurahan (`["KelDesa", "Kelurahan", "DesaKelurahan", "Village"]`).
5. **Efek Domino Hierarki Ketat:** Satu kegagalan pada level atas (misal Kota `null`) otomatis membatalkan pencarian Kecamatan dan Kelurahan di bawahnya, tanpa adanya mekanisme pencarian balik (*reverse lookup*) dari kelurahan/kode pos.

---

## 2. Laporan Asli

| No. Laporan | Kalimat Pelapor | Konteks Layar |
| :---: | --- | --- |
| 1 | "pada saat scan ktp di admisi pasien baru, ada masalah wilayah tidak terdekteksi dan tidak masuk form input yang membuat harus admisi menginput lagi, coba anda analisis, dan lalu jelaskan masalah ya kepada saya" | Admisi Rawat Inap — Langkah 2 Pendaftaran Pasien Baru (`inpatient-admission-registration-step.jsx`) & Pendaftaran Rawat Jalan (`outpatient-registration-patient-form.jsx`) |

---

## 3. Ringkasan Temuan

| ID | No. Laporan | Judul | Jenis | Area | Keparahan | Status Bukti | Perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-015-01` | 1 | Indikator Jenis Wilayah Hilang Akibat `cleanSearch` Diteruskan ke Penilai Skor Wilayah | `BUG` | Frontend | High | SUDAH-VERIFIKASI (`emergency-region.service.js:450`, gagal di `inpatient-admission-registration-issue-003.test.mjs`) | `FIX-EPS-015-01` |
| `ISS-EPS-015-02` | 1 | Hook Admisi Rawat Inap & Rawat Jalan Tidak Menyetel `__scanResolvedRegion` ke Form State | `BUG` | Frontend | High | SUDAH-VERIFIKASI (`use-inpatient-admission-patient.jsx:148-157` & `use-outpatient-registration.js:406-414`) | `FIX-EPS-015-02` |
| `ISS-EPS-015-03` | 1 | `NewPatientForm` Tidak Meneruskan Prop `selectedOption` ke `EmergencySelectField` | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI (`new-patient-form.jsx:291-355`) | `FIX-EPS-015-03` |
| `ISS-EPS-015-04` | 1 | Kamus Alias OCR Kelurahan/Desa dan Pembersih Awalan Provinsi Kurang Lengkap | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI (`emergency-registration.utils.js:815-833` & `emergency-region.service.js:115-119`) | `FIX-EPS-015-04` |
| `ISS-EPS-015-05` | 1 | Ketiadaan Mekanisme Pencarian Balik (*Reverse Lookup*) saat Rantai Hierarki Atas Terputus | `DESIGN_GAP` | Frontend | Medium | SUDAH-VERIFIKASI (`emergency-region.service.js:498-558`) | `FIX-EPS-015-05` |

---

## 4. Rincian Per Temuan

### ISS-EPS-015-01 — Indikator Jenis Wilayah Hilang Akibat `cleanSearch` Diteruskan ke Penilai Skor Wilayah

* **Jenis**: `BUG` (Logika Pencocokan Wilayah)
* **Area**: Frontend — `src/lib/services/health-services/registration-management/emergency-region.service.js`
* **Keparahan**: High
* **Status Bukti**: Terbukti langsung melalui pengujian unit `inpatient-admission-registration-issue-003.test.mjs:80-87`:
  * `FE-RWI-204: hasil scan KOTA BEKASI terpasang ke Kota Bekasi` -> GAGAL (menghasilkan string kosong `""` bukan `"kota-bekasi"`).
  * `FE-RWI-204: hasil scan KABUPATEN / KAB. BEKASI terpasang ke Kabupaten Bekasi` -> GAGAL (menghasilkan string kosong `""` bukan `"kab-bekasi"`).
* **Akar Masalah**:
  Pada baris 438-450:
  ```javascript
  const cleanSearch = removeRegionPrefix(searchText); // "KOTA BEKASI" menjadi "BEKASI"
  let items = await loader({ ...params, search: cleanSearch });
  let match = findBestRegionOption(items, cleanSearch || searchText, type);
  ```
  Nilai `cleanSearch || searchText` menghasilkan `"BEKASI"`. Ketika masuk ke `scoreRegionOption`:
  ```javascript
  const wantedType = getCityTypeKey(searchValue); // getCityTypeKey("BEKASI") -> ""
  ```
  Karena kata "KOTA" sudah terbuang, sistem menganggap tidak ada jenis yang diminta. Kota Bekasi dan Kabupaten Bekasi memperoleh nilai sama (1000). Sesuai aturan keselamatan baris 421:
  ```javascript
  if (type === "city" && ranked[1]?.score === ranked[0].score) return null;
  ```
  Pencocokan kota menghasilkan `null`, dan membatalkan pencarian tingkat di bawahnya.
* **Perbaikan**: Panggil `findBestRegionOption(items, searchText, type)` agar teks asli yang memuat kata "KOTA" atau "KABUPATEN" dapat dibaca oleh fungsi pemberi bobot jenis kota.

---

### ISS-EPS-015-02 — Hook Admisi Rawat Inap & Rawat Jalan Tidak Menyetel `__scanResolvedRegion` ke Form State

* **Jenis**: `BUG` (Desinkronisasi State Form)
* **Area**: Frontend — `use-inpatient-admission-patient.jsx` dan `use-outpatient-registration.js`
* **Keparahan**: High
* **Akar Masalah**:
  Hook [useEmergencyRegionOptions](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-region-option.js#L79-L147) yang mengatur daftar pilihan dropdown di `NewPatientForm` mendengarkan perubahan field `__scanResolvedRegion`.
  Pada alur IGD, nilai ini diset. Namun pada alur Admisi Rawat Inap (`use-inpatient-admission-patient.jsx`) dan Rawat Jalan (`use-outpatient-registration.js`), `__scanResolvedRegion` tidak pernah diset ke formulir. Akibatnya, `countryOptions`, `provinceOptions`, `cityOptions`, `districtOptions`, dan `postalCodeOptions` tetap bernilai array kosong `[]`.
  Meskipun UUID tersimpan di form state, kontrol UI dropdown tidak menemukan opsi dengan ID tersebut dan menampilkan placeholder kosong.

---

### ISS-EPS-015-03 — `NewPatientForm` Tidak Meneruskan Prop `selectedOption` ke `EmergencySelectField`

* **Jenis**: `BUG` (Komponen UI)
* **Area**: Frontend — `src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx`
* **Keparahan**: Medium
* **Akar Masalah**:
  Hook `useEmergencyRegionOptions` sudah menyiapkan objek pilihan terpilih (`selectedCountryOption`, `selectedProvinceOption`, `selectedCityOption`, `selectedDistrictOption`, `selectedPostalCodeOption`). Komponen `EmergencySelectField` dan `FilterSelect` juga sudah memiliki prop `selectedOption`.
  Namun, pada baris 291-355 di `new-patient-form.jsx`, prop tersebut tidak diteruskan. Tanpa `selectedOption`, `FilterSelect` hanya mengandalkan pencarian pada array `options`. Jika array `options` masih kosong atau sedang dimuat, teks label wilayah langsung hilang.

---

### ISS-EPS-015-04 — Kamus Alias OCR Kelurahan/Desa dan Pembersih Awalan Provinsi Kurang Lengkap

* **Jenis**: `BUG` (Normalisasi Input Scanner)
* **Area**: Frontend — `emergency-registration.utils.js` dan `emergency-region.service.js`
* **Keparahan**: Medium
* **Akar Masalah**:
  Pada `emergency-registration.utils.js:815`, pencarian kelurahan tidak menyertakan kata `"Desa"`. Pada KTP warga pedesaan di mana tercetak kata "Desa" (bukan "Kel/Desa"), kelurahan terbaca string kosong.
  Pada `emergency-region.service.js:115`, fungsi `stripProvincePrefix` hanya memotong `"PROVINSI "` dan `"PROPINSI "`, belum memotong singkatan `"PROV. "` atau `"PROV "`.

---

### ISS-EPS-015-05 — Ketiadaan Mekanisme Pencarian Balik (*Reverse Lookup*)

* **Jenis**: `DESIGN_GAP`
* **Area**: Frontend — `src/lib/services/health-services/registration-management/emergency-region.service.js`
* **Keparahan**: Medium
* **Akar Masalah**:
  Berbeda dengan Kiosk yang memiliki fallback pencarian kelurahan langsung ke tabel kode pos untuk menginferensi kecamatan/kota/provinsi, service admisi petugas hanya mencari dari atas ke bawah. Jika salah satu tingkat di atas ambigu atau gagal, seluruh hierarki di bawahnya terputus.

---

## 5. Tanya-Jawab Pelapor

> **T:** *Kenapa pada saat scan KTP di admisi pasien baru, wilayah tidak terdeteksi dan tidak masuk ke form input sehingga admisi harus menginput lagi?*
>
> **J:** Ada dua penyebab utama:
> 1. **Data wilayah sebenarnya berhasil dicocokkan di memori, tetapi layarnya lupa diberi tahu:** Modul admisi rawat inap dan rawat jalan tidak mengirimkan paket data pilihan (`__scanResolvedRegion`) ke form, dan komponen input form tidak dihubungkan dengan properti label terpilih (`selectedOption`). Akibatnya, kotak dropdown di layar tetap kosong dan kembali menampilkan *"Pilih provinsi"* atau *"Pilih kota"*. Petugas menyangka sistem gagal membaca dan terpaksa memilih manual.
> 2. **Pencarian kota kembar (seperti Bekasi, Bogor, Bandung) sempat macet:** Ketika scanner membaca "KOTA BEKASI", sistem tidak sengaja membuang kata "KOTA" sebelum menilai kecocokan data. Karena kata "KOTA" hilang, sistem bingung membedakan antara "Kota Bekasi" dan "Kabupaten Bekasi". Karena bingung, sistem memilih tidak mengisi kota. Dan karena kota kosong, pencarian kecamatan dan kelurahan di bawahnya langsung otomatis dibatalkan (efek domino).
