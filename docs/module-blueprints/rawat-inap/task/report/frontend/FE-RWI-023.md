# Laporan Perubahan Frontend — `FE-RWI-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-023` |
| Judul | Pasien dapat didaftarkan atau ditemukan dari dalam alur admisi |
| Slice | Layar `FE-INP-03` — langkah **Pendaftaran** jalur pasien baru, serta langkah **Pasien Lama** dan **Informasi Pasien Lama** jalur pasien lama |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` revision `5`, kartu `FE-RWI-023` |
| Trace | `FLOW-RI-MVP-001` langkah 1; `03-frontend-architecture.md` bagian 3A.2 langkah 2 dan 3A.3 langkah 1–2; `05-skema-tampilan.md` bagian 3.3–3.4 |
| Contract version | Kontrak diambil dari source backend commit `3c4c06fc41601c23c4173e033d3ffff68230d5a1` — `PatientController`, `PatientIdentityDocumentController`, dan `PatientEmergencyContactController`. Kontrak tidak ditebak; setiap route, DTO, dan policy diverifikasi langsung ke source |
| Wewenang UI | `DEV_DISCRETION` untuk susunan isian dan pemakaian pemindai, sesuai kartu task |
| Dependency | `FE-RWI-022` — **selesai**, kerangka alur dua jalur sudah berdiri ([FE-RWI-022](FE-RWI-022.md)). Tidak ada dependency task backend |
| Klasifikasi | `MEDIUM` — lima berkas baru dan empat berkas berubah; satu service HTTP baru, satu hook controller baru, satu utility baru; tanpa Redux slice baru, tanpa base component baru, tanpa dependency package baru |
| Task mode | `FRONTEND` dengan wewenang lintas repository terbatas untuk laporan tracked ini |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` untuk source; `NewQuilvianSystemBackend` branch `MHamzah` hanya untuk laporan ini |
| Model | Claude Opus 5 (Claude Code) |
| Commit frontend saat dikerjakan | `40f0e61067612c3f701631783e0843b17464f967` |
| Commit backend yang dijadikan rujukan | `3c4c06fc41601c23c4173e033d3ffff68230d5a1` |
| Tanggal | 2026-08-29 |
| Status | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terimplementasi; `npm run lint:errors` dan `npm run build` lulus. Butir DoD uji manual/E2E dikecualikan atas keputusan pengguna; catatan bahwa kriteria 1, 4, dan 5 belum terbukti dengan data nyata tetap berlaku dan tidak dihapus. Lihat "Penutupan status" di akhir laporan |

---

## 1. Keadaan yang ditemukan di awal

`FE-RWI-022` sudah mendirikan kerangka alur dua jalur pada
`/health-services/inpatient-management/admissions`. Langkah aktif tersimpan pada URL, penanda
langkah tampil, dan langkah **Tipe Pasien** sudah berfungsi. Seluruh langkah lain — termasuk
**Pendaftaran**, **Pasien Lama**, dan **Informasi Pasien Lama** — masih berupa pemberitahuan
bahwa isinya dimiliki task lanjutan. Petugas admisi belum dapat mendaftarkan pasien maupun
mencari pasien lama dari dalam alur.

Pola yang diwajibkan kartu task sudah tersedia pada pendaftaran IGD, tetapi tiga celah nyata
ditemukan saat diperiksa:

1. **`new-patient-form.jsx` tidak memiliki bagian Kontak Darurat.** Formulir itu memuat identitas,
   dokumen, kontak pasien, alamat, dan data tambahan, tetapi tidak memuat nama, hubungan, dan
   nomor kontak darurat yang diminta skema tampilan 3.3.
2. **`patient-selection-step.jsx` mencari dengan cara yang berbeda.** Komponen IGD itu menyediakan
   pencarian **No. Rekam Medis** dan **Nama + Tanggal Lahir**
   (`EXISTING_PATIENT_SEARCH_MODE`), sedangkan acceptance criteria 2 mewajibkan **No. Rekam Medis**
   dan **NIK**. Komponen itu juga terikat pada state Redux pendaftaran IGD dan salinan teks IGD
   sepanjang 1.635 baris.
3. **Route yang dipakai pendaftaran IGD tidak dapat diakses petugas admisi.** Inilah gerbang
   `RWI-UI-GAP-006`, dan pemeriksaan source backend menutupnya dengan bukti — dijelaskan pada
   bagian 5.

---

## 2. Proses bisnis dari sisi pengguna

### Tujuan dan pelaku

Petugas admisi rawat inap membuka menu **Admisi Rawat Inap** untuk memulai admisi. Ia tidak lagi
perlu keluar ke modul Pendaftaran atau modul Pasien untuk mendaftarkan pasien baru maupun mencari
pasien yang sudah punya nomor rekam medis.

### Jalur pasien baru — langkah Pendaftaran

1. Petugas memilih kartu **Pendaftaran Pasien Baru**, lalu memilih jenis pasien pada langkah
   **Tipe Pasien**, lalu menekan **Lanjut ke Pendaftaran**.
2. Layar **Pendaftaran Pasien Baru** terbuka pada `?entry=new&step=registration`.
3. Bagian paling atas adalah panel **Scan eKTP**. Bila pemindai Plustek aktif, petugas menekan
   **Scan eKTP** dan hasil pembacaan langsung mengisi kolom identitas beserta wilayah alamatnya.
   Bila pemindai tidak aktif, panelnya **tetap tampil** dan menjelaskan bahwa formulir di bawah
   dapat diisi manual — panelnya tidak hilang tanpa penjelasan.
4. Petugas melengkapi **Identitas Pasien**, **Dokumen dan Kontak**, dan **Alamat Pasien**.
5. Petugas melengkapi bagian baru **Kontak Darurat**: nama, hubungan, dan nomor HP. Ketiganya
   wajib.
6. Tombol **Simpan & Lanjut ke Pembayaran** baru aktif setelah seluruh kolom wajib terisi, dan
   **mati selama permintaan berjalan** dengan tulisan `Menyimpan...`.
7. Saat ditekan, sistem mengirim tiga permintaan berurutan: membuat pasien, lalu dokumen
   identitasnya, lalu kontak daruratnya. Setelah ketiganya berhasil, layar berpindah ke langkah
   **Pembayaran** dan nomor pasien yang baru terbentuk ikut tersimpan pada URL.

### Jalur pasien lama — langkah Pasien Lama dan Informasi Pasien Lama

1. Petugas memilih kartu **Pendaftaran Pasien Lama**. Layar membuka
   `?entry=existing&step=existing-patient`.
2. Layar **Cari Data Pasien** menampilkan dua kartu cara mencari: **Nomor RM** dan **NIK**.
   Memilih salah satunya mengubah contoh isian dan mengosongkan hasil pencarian sebelumnya.
3. Petugas mengetik nomornya, lalu menekan **Cari**.
4. Bila hasilnya lebih dari satu, daftar hasil tampil berisi **nama, nomor rekam medis, dan
   tanggal lahir**. NIK penuh **tidak** ditampilkan pada daftar ini.
5. Petugas menekan satu baris hasil. Layar berpindah ke **Informasi Pasien Lama**
   (`?entry=existing&step=existing-patient-information&patientId=...`) dan menampilkan kartu
   tinjauan berisi nama, nomor rekam medis, NIK, tanggal lahir beserta umurnya, jenis kelamin,
   nomor HP, dan alamat.
6. Petugas menekan **Lanjut ke Tipe Pasien** bila datanya benar, atau **Ganti Pasien** untuk
   kembali ke pencarian dengan pilihan pasien dikosongkan.

### Jalur tidak normal

- **Pasien tidak ditemukan.** Layar menampilkan "Pasien dengan nomor itu tidak ditemukan." beserta
  tombol **Daftarkan sebagai pasien baru** yang memindahkan petugas ke jalur pasien baru.
- **Server menolak penyimpanan pasien.** Pesan penolakan dari server ditampilkan **apa adanya** di
  atas formulir, dan **seluruh isian tetap utuh**. Petugas memperbaiki bagian yang ditolak lalu
  menekan simpan lagi.
- **Penyimpanan gagal di tengah urutan.** Bila pasien sudah terbentuk tetapi dokumen identitas
  atau kontak darurat gagal, nomor pasien itu diingat. Menekan simpan lagi **melanjutkan dari
  operasi yang gagal** dan tidak membuat pasien kedua. Layar menjelaskan hal ini lewat
  pemberitahuan tersendiri.
- **Tombol simpan ditekan dua kali.** Penekanan kedua tertahan seketika oleh penjaga permintaan,
  sehingga hanya satu pasien terbentuk.
- **Halaman dimuat ulang di langkah Informasi Pasien Lama.** Nomor pasien dibaca dari URL, detail
  pasien diambil ulang, dan kartu tinjauan pulih. Layar tidak kembali ke langkah pertama.
- **Pemindai tidak aktif.** Panel Scan tetap tampil dengan penanda `Scanner offline` dan tombol
  **Input Manual**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Frontend:

- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx`
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
- `src/components/view/health-services/registration-management/emergency-registration/` — seluruh
  berkas, terutama `new-patient-form.jsx`, `patient-selection-step.jsx`, `plustek-scan-panel.jsx`,
  `emergency-registration-fields.jsx`, dan `emergency-registration-page.jsx`
- `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js`
  dan `use-plustek-ktp-scanner.js`
- `src/lib/services/health-services/registration-management/emergency-registration.service.js`
  dan `emergency-region.service.js`
- `src/lib/state/slice/health-services/registration-management/emergency-registration-slice.jsx`
- `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js`
- `src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js`
- `src/lib/services/kiosk/registration/kiosk-new-patient-registration.service.js` dan
  `src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx`
- `src/lib/constants/kiosk/registration/kiosk-new-patient-registration.constants.js`
- `src/components/features/base-features/` — `base-button.jsx`, `base-checkbox-card.jsx`,
  `base-detail-card.jsx`, `base-form-control.jsx`, `base-text-field.jsx`, `data-filter.jsx`,
  `data-table.jsx`, `information-alert.jsx`, `base-patient-card.jsx`
- `src/components/view/health-services/inpatient-management/inpatient-census-view.jsx` sebagai
  modul referensi visual terdekat dalam modul yang sama
- `src/lib/services/health-services/inpatient-management/inpatient-api.service.js`

Backend, dibaca sebagai referensi kontrak dan **tidak diubah**:

- `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs`
- `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientIdentityDocumentController.cs`
- `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientEmergencyContactController.cs`
- `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientDtos.cs`,
  `PatientIdentityDocumentDtos.cs`, `PatientEmergencyContactDtos.cs`
- `Program.cs` bagian `AddAuthorization`, untuk membaca isi policy `KioskRead`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js` | **Baru.** Empat operasi HTTP pasien untuk alur admisi memakai route operasional `/admin`, lengkap dengan penerusan `signal` dan pemetaan pesan error server apa adanya |
| `src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx` | **Baru.** Penyusun payload dokumen identitas dan kontak darurat, pemeriksa kelengkapan kontak darurat, penyusun baris kartu tinjauan, dan penyusun baris hasil pencarian |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | **Baru.** Controller kedua jalur: formulir pasien baru, pemindai eKTP, urutan tiga permintaan beserta penjaga klik ganda, serta pencarian, pemilihan, dan pemuatan detail pasien lama |
| `src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` | **Baru.** Langkah Pendaftaran: panel scan, formulir pasien IGD yang dipakai ulang, bagian Kontak Darurat, dan aksinya |
| `src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx` | **Baru.** Dua langkah jalur pasien lama: layar pencarian dan layar tinjauan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Pemetaan langkah dipindahkan ke `AdmissionStep`, tiga langkah baru dipasang, dan controller pasien disambungkan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` | Menambah query `patientId` pada URL beserta `goNextWithPatient` dan `goBackWithPatient`, supaya perpindahan langkah dan pilihan pasien terjadi dalam satu penulisan URL |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Menambah cara mencari pasien lama, nilai awal kontak darurat, nilai awal formulir pasien baru, kolom hasil pencarian, dan ukuran halaman |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Menambah tata letak panel kontak darurat, kartu cara mencari, dan panel pasien tidak ditemukan; seluruhnya memakai design token |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi setelah perubahan:

```text
src/app/.../admissions/page.jsx
  -> inpatient-admission-view.jsx
       -> use-inpatient-admission-flow.jsx        (langkah + pasien pada URL)
       -> use-inpatient-admission-patient.jsx     (controller data pasien)
            -> inpatient-admission-patient.service.js
                 -> InstanceAxios
            -> inpatient-admission-patient-utils.jsx  (fungsi murni)
       -> inpatient-admission-registration-step.jsx
       -> inpatient-admission-existing-patient-step.jsx
```

Yang dipatuhi:

- View tidak memanggil `InstanceAxios` langsung; seluruh permintaan lewat service.
- Tidak ada Axios instance baru; service memakai `InstanceAxios` yang sudah ada.
- Endpoint tidak tersebar sebagai teks di view; seluruhnya terkumpul pada
  `INPATIENT_ADMISSION_PATIENT_API_URLS`.
- Utility tetap murni: tidak memakai hook React, tidak merender JSX, tidak memanggil API.
- Pencarian pasien dan pemuatan detail meneruskan `AbortController.signal`, dan permintaan
  dibatalkan saat komponen dilepas maupun saat pencarian berikutnya dimulai.
- Route `page.jsx` tidak berubah dan tetap hanya menjadi entry point.
- Tidak ada Redux slice baru. Data pasien pada task ini hanya dipakai satu layar, sehingga
  ditangani service dan hook, sesuai aturan `src/lib/services`.

Yang dipakai ulang, bukan ditulis ulang:

- `new-patient-form.jsx`, `plustek-scan-panel.jsx`, `emergency-registration-fields.jsx`, dan
  `use-plustek-ktp-scanner.js` dari pendaftaran IGD — **tanpa mengubah satu baris pun** di
  berkas-berkas itu.
- `buildPatientPayload`, `normalizePatient`, `calculateAge`, `formatDateDisplay`,
  `getCollectionItems`, `getCollectionPagination`, `extractEntityId`, dan
  `mapScanResultToPatientValues` dari utility pendaftaran IGD.
- `resolveEmergencyScannedRegion` dan `buildEmergencyScannedAddress` untuk mencocokkan wilayah
  hasil scan.
- `REQUIRED_NEW_PATIENT_FIELDS`, `DEFAULT_PATIENT_FORM_VALUES`, dan `EMERGENCY_RELATION_OPTIONS`
  dari constants yang sudah ada.

**Satu penyimpangan terhadap checklist, disebut apa adanya.** Checklist UI meminta definisi kolom
tabel diletakkan pada berkas `<feature>-table-columns.jsx` terpisah. Kolom hasil pencarian di sini
diletakkan pada berkas constants modul sebagai `INPATIENT_PATIENT_SEARCH_COLUMNS`, karena kolomnya
tidak memuat JSX sama sekali dan modul Rawat Inap yang menjadi referensi terdekat
(`inpatient-census-view.jsx`) mendefinisikan kolomnya inline. Meletakkannya di constants lebih
mendekati checklist daripada pola inline modul ini, tanpa membuat berkas berpola baru.

---

## 4. UI gate dan state layar

`UI GATE: 12 elemen — REUSE 6, EXTEND 0, COMPOSE 6, WRAP 0, NEW 0`.

| Kebutuhan UI | Kandidat dan bukti | Status | Keputusan |
| --- | --- | --- | --- |
| Panel Scan KTP | `emergency-registration/plustek-scan-panel.jsx` | `REUSE` | Dipakai apa adanya; salinan teksnya sudah netral untuk pasien baru |
| Formulir identitas, dokumen, alamat | `emergency-registration/new-patient-form.jsx` | `REUSE` | Dipakai apa adanya di dalam `FormProvider` |
| Kontak Darurat | `emergency-registration-fields.jsx` — `EmergencyTextField`, `EmergencySelectField`; `kiosk-new-patient-registration.constants.js` — `EMERGENCY_RELATION_OPTIONS` | `COMPOSE` | Dirangkai di layer view; `new-patient-form.jsx` tidak diubah |
| Pesan penolakan server | `base-features/information-alert.jsx` | `REUSE` | `variant="danger"` di atas formulir |
| Aksi langkah | `base-features/base-button.jsx` | `REUSE` | Tidak ada tombol mentah baru |
| Kartu cara mencari RM/NIK | `base-features/base-checkbox-card.jsx` | `COMPOSE` | Dua kartu dirangkai sebagai satu pilihan tunggal, sama seperti kartu jenis pasien pada `FE-RWI-022` |
| Isian pencarian dan tombol Cari | `base-features/data-filter.jsx` | `COMPOSE` | Search milik `DataFilter`; tombol **Cari** dipasang lewat prop `actions` |
| Daftar hasil pencarian | `base-features/data-table.jsx` | `REUSE` | Loading, empty, paginasi, dan kontrak `data-flat-table` ikut terpakai |
| Paginasi hasil | `features/pagination/pagination.jsx` | `REUSE` | Diteruskan lewat `PaginationComponent`, sama seperti `inpatient-census-view.jsx` |
| Panel tinjauan pasien lama | `base-features/base-detail-card.jsx` | `REUSE` | `item`, `title`, `code`, `rows`, `showAudit={false}`; menghasilkan kepala berisi inisial, nama, nomor RM, dan grid **Validasi Data** |
| Panel pasien tidak ditemukan | `information-alert.jsx` + `base-button.jsx` | `COMPOSE` | Dirangkai dengan tautan aksi ke jalur pasien baru |
| Pemberitahuan batas kontrak | `information-alert.jsx` | `COMPOSE` | Menjelaskan baris riwayat kunjungan yang belum ada kontraknya |

Tidak ada elemen berstatus `NEW`, dan tidak ada `EXTEND` yang mengubah perilaku bawaan base
component, sehingga gerbang keputusan tidak menghasilkan butir yang menunggu persetujuan.
`base-patient-card.jsx` sempat dipertimbangkan untuk panel tinjauan, lalu ditolak karena berkas itu
adalah kartu pasien **cetak** milik kiosk — pemakaiannya ada di
`kiosk-patient-card-step-preview.jsx` — dan menjadi milik `FE-RWI-029`, bukan layar tinjauan.

### State layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pencarian: `DataTable` menampilkan "Mengambil data pasien...". Tinjauan: `BaseDetailCard` menampilkan "Mengambil detail pasien...". Simpan: tombol utama mati dan bertulisan `Menyimpan...` |
| Kosong | Sebelum mencari, tabel hasil menampilkan "Data pasien tidak ditemukan." beserta "Ubah cara mencari atau periksa kembali nomor yang diketik." Setelah pencarian tanpa hasil, panel khusus menambahkan "Pasien dengan nomor itu tidak ditemukan." dan tombol **Daftarkan sebagai pasien baru** |
| Gagal | Pesan server ditampilkan apa adanya lewat `InformationAlert` merah — di atas formulir untuk penyimpanan, dan di atas tabel untuk pencarian. Isian formulir tidak direset |
| Tanpa hak akses | Task ini **tidak** menambah pemeriksaan hak akses baru. Route tetap berada di shell autentikasi aplikasi. Penolakan `403` dari route `/admin` muncul sebagai pesan server pada `InformationAlert`. Layar admisi belum dibungkus `AccessDeniedGate`; pembungkusan itu tidak dilakukan di sini karena mengubah perilaku seluruh alur admisi, bukan hanya dua langkah milik task ini |

---

## 5. Endpoint yang dikonsumsi

### Gerbang `RWI-UI-GAP-006` — route dan permission petugas admisi

Kartu task menahan penulisan integrasi sampai route dan permission petugas admisi dikunci.
Pemeriksaan source backend menutup gerbang ini dengan bukti, bukan dengan asumsi:

| Operasi | Route tanpa `/admin` | Route `/admin` |
| --- | --- | --- |
| Daftar pasien | `[AccessPermission("Patient", "Read")]` | sama |
| Pilihan pasien (`options`) | `[Authorize(Policy = "KioskRead")]` | `[AccessPermission("Patient", "Read")]` |
| Detail pasien | `[Authorize(Policy = "KioskRead")]` | `[AccessPermission("Patient", "Read")]` |
| Buat pasien | `[Authorize(Policy = "KioskRead")]` | `[AccessPermission("Patient", "Create")]` |
| Buat dokumen identitas | `[Authorize(Policy = "KioskRead")]` | `[AccessPermission("PatientIdentityDocument", "Create")]` |
| Buat kontak darurat | `[Authorize(Policy = "KioskRead")]` | `[AccessPermission("PatientEmergencyContact", "Create")]` |

Isi policy `KioskRead` pada `Program.cs` hanya menerima role `SuperAdmin`, `Administrator`, atau
`Kiosk`, serta akun yang membawa penanda kiosk. Artinya **petugas admisi biasa akan ditolak** pada
route tanpa `/admin`, walaupun rolenya sudah memiliki permission `Patient : Create`. Karena itu
alur admisi rawat inap memakai varian `/admin` yang dijaga RBAC petugas — permission yang sama
dengan modul master data lain.

**Temuan sampingan yang dilaporkan, tidak diperbaiki.** Pendaftaran IGD
(`emergency-registration.service.js`) saat ini memanggil route pasien **tanpa** `/admin`, yang
berarti petugas IGD non-administrator akan ditolak pada langkah simpan pasien. Perbaikannya berada
di luar cakupan `FE-RWI-023` dan diserahkan kepada pemilik modul IGD.

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin` | Mencari pasien lama dengan nomor rekam medis atau NIK; parameter `search` backend sudah mencakup kedua kolom itu | `Patient : Read` |
| `GET` | `/v1/health-services/patient-management/master-data/patients/admin/{id}` | Mengambil detail pasien untuk kartu tinjauan, karena alamat lengkap hanya ada pada `PatientDetailResponse` | `Patient : Read` |
| `POST` | `/v1/health-services/patient-management/master-data/patients/admin` | Menyimpan pasien baru dari langkah Pendaftaran | `Patient : Create` |

#### Health Services / Patient Management / Master Data / Patient Identity Document

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/patient-management/master-data/patient-identity-documents/admin` | Menyimpan dokumen identitas pasien baru, dijalankan setelah pasien terbentuk | `PatientIdentityDocument : Create` |

#### Health Services / Patient Management / Master Data / Patient Emergency Contact

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/patient-management/master-data/patient-emergency-contacts/admin` | Menyimpan kontak darurat pasien baru, dijalankan paling akhir | `PatientEmergencyContact : Create` |

### Selisih terhadap skema tampilan

| Butir skema 3.4 | Keadaan kontrak backend | Yang dilakukan |
| --- | --- | --- |
| Alamat pasien | Tidak ada pada `PatientResponse` maupun `PatientOptionResponse`; hanya ada pada `PatientDetailResponse` | Detail pasien diambil sekali setelah pasien dipilih |
| Kunjungan terakhir | **Tidak ada** pada satu pun DTO pasien | Barisnya tidak dibuat. Layar menjelaskan alasannya lewat pemberitahuan, dan nilainya tidak dikarang |
| `GET /patients/options` yang disebut kartu task | Tersedia, tetapi `PatientOptionResponse` tidak membawa alamat, wilayah, maupun foto | Pencarian memakai operasi daftar `/patients/admin` yang membawa data lebih lengkap dan sudah dijaga permission petugas |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <8 berkas yang diubah>` | 0 error, 0 warning | `PASS` | Keluaran perintah kosong, exit code 0 |
| `npm run lint:errors` | Berhasil tanpa error di seluruh repository | `PASS` | Keluaran perintah kosong, exit code 0 |
| `npm run build` | `✓ Compiled successfully in 2.0min`; route `/health-services/inpatient-management/admissions` ikut terbangun; `postbuild` selesai | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 264 test, 262 lulus, 2 gagal | `EXISTING / ENVIRONMENT ISSUE` | Kedua kegagalan sudah ada sebelum perubahan ini — lihat di bawah |
| `npm run test:unit` | Gagal dijalankan: `ERR_UNSUPPORTED_DIR_IMPORT` pada `tests/unit` | `EXISTING / ENVIRONMENT ISSUE` | Node.js `v24.19.0` menolak bentuk import direktori yang dipakai script; tidak berkaitan dengan perubahan ini |
| Grep anti-regresi checklist UI, butir 1–6 | Seluruhnya kosong | `PASS` | Tidak ada warna literal, typography penimpa, tombol mentah, tabel mentah, utility Bootstrap, maupun `!important` baru |
| Uji manual kontrol interaktif di layar | Tidak dapat dijalankan | `NOT FEASIBLE` | Lihat di bawah |

**Kedua kegagalan unit test bukan berasal dari task ini:**

- `tests/unit/auth-security.test.mjs` — di luar cakupan modul Rawat Inap.
- `tests/unit/inpatient-admission.test.mjs:168` — menegaskan
  `viewSource.includes("refreshToken: admission.bedRefreshToken")`. Versi
  `inpatient-admission-view.jsx` **pada commit `40f0e61`** juga tidak memuat teks itu, karena
  formulir admisi lama sudah diganti kerangka berlangkah pada `FE-RWI-022`. Pembersihan test jalur
  lama dimiliki `FE-RWI-035`. Diverifikasi dengan
  `git show HEAD:src/.../inpatient-admission-view.jsx | grep -c "refreshToken: admission.bedRefreshToken"` yang mengembalikan `0`.

**Uji manual: `NOT FEASIBLE`.** Alasannya konkret: seluruh route yang dipakai task ini dijaga
`[Authorize]` ditambah `AccessPermission`, dan tidak tersedia sesi login petugas admisi maupun akun
uji pada lingkungan pengerjaan. Host API yang dikonfigurasi memang menjawab, tetapi tanpa sesi
login layar admisi tidak dapat dibuka. Kontrol yang **belum** terbukti secara langsung:

- pemindaian eKTP mengisi formulir;
- pencarian dengan nomor rekam medis dan dengan NIK mengembalikan pasien yang benar;
- penolakan server tampil apa adanya dan isian tetap utuh;
- penekanan simpan dua kali hanya menghasilkan satu pasien;
- paginasi hasil pencarian;
- pemulihan kartu tinjauan setelah halaman dimuat ulang.

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — keduanya membutuhkan lingkungan
beserta akun uji yang sama, dan pengguna meminta agar tidak ada penulisan test `.mjs` baru pada
task ini. Penulisan test baru bersifat opsional menurut `rules/frontend/test-policy.md`, sehingga
`AUTOMATED TEST: SKIPPED (opsional) — perubahan berupa komposisi view, hook, dan service; suite
unit yang sudah ada tetap dijalankan dan hasilnya dilaporkan apa adanya`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien baru tersimpan beserta dokumen identitas dan kontak darurat | **Terimplementasi, belum terbukti runtime** | `handleSaveNewPatient` pada `use-inpatient-admission-patient.jsx` menjalankan `POST /patients/admin` → `POST /patient-identity-documents/admin` → `POST /patient-emergency-contacts/admin` berurutan; payloadnya disusun `inpatient-admission-patient-utils.jsx` sesuai DTO backend. Belum diuji dengan data nyata karena uji manual `NOT FEASIBLE` |
| 2. Pencarian pasien lama menerima nomor rekam medis dan NIK | **Terpenuhi** | `INPATIENT_EXISTING_PATIENT_SEARCH_MODE` menyediakan kedua cara; `ApplyStandardFilter` pada `PatientController.cs` baris 1956–1962 membuktikan parameter `search` mencakup `MedicalRecordNumber` dan `IdentityNumber` |
| 3. Data pasien lama ditinjau sebelum alur dilanjutkan | **Terpenuhi** | Langkah `existing-patient-information` berdiri sendiri; tombol **Lanjut ke Tipe Pasien** mati sampai ada pasien terpilih; kartu tinjauan memakai `BaseDetailCard` |
| 4. Penolakan server ditampilkan apa adanya dan isian tidak hilang | **Terimplementasi, belum terbukti runtime** | `getRequestErrorMessage` pada service mengambil `message`/`title`/`detail`/`errors` dari respons tanpa menggantinya; `handleSaveNewPatient` hanya memanggil `setSaveError` dan tidak pernah mereset formulir. Belum diuji dengan penolakan server sungguhan |
| 5. Menekan simpan dua kali hanya menghasilkan satu pasien | **Terimplementasi, belum terbukti runtime** | Penjaga `savePatientInFlight` berbentuk `useRef` menahan panggilan kedua seketika; `savedPatientId` mencegah pembuatan pasien kedua pada percobaan ulang setelah kegagalan parsial. Belum diuji dengan pemeriksaan jaringan |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kelima kriteria lulus | **Belum** — tiga kriteria terimplementasi tetapi belum terbukti runtime |
| E2E kedua jalur ada dan lulus | **Belum** — tidak ditulis dan tidak dijalankan; lingkungan tidak menyediakan sesi login petugas admisi, dan pengguna meminta tidak ada test `.mjs` baru pada task ini |

Kartu task juga meminta pemeriksaan jaringan bahwa tidak ada pasien kembar saat tombol ditekan dua
kali. Pemeriksaan itu **belum dijalankan** karena alasan yang sama.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dua peringatan lint sempat muncul pada berkas baru — `react-hooks/incompatible-library` akibat `watch()` milik React Hook Form, dan `react-hooks/set-state-in-effect`. Keduanya sudah dibereskan: `watch()` diganti `useWatch`, dan pemuatan detail dipindahkan ke fungsi async tersendiri. Akhirnya berkas baru bersih dari error maupun warning |
| Masalah yang diketahui | 1) Baris **Kunjungan terakhir** pada skema 3.4 tidak dibuat karena tidak ada kontrak backend yang mengembalikannya. 2) Pencarian memakai parameter `search` backend yang bersifat luas, sehingga pencarian NIK juga dapat mengembalikan pasien yang cocok pada kolom lain; hasil tidak disaring ulang di layar supaya tidak ada pasien sah yang justru disembunyikan. 3) Layar admisi belum dibungkus `AccessDeniedGate`. 4) `plustek-scan-panel.jsx` yang dipakai ulang masih memakai tombol `<button>` mentah bawaan pendaftaran IGD; berkas itu sengaja tidak diubah agar pendaftaran IGD tidak ikut terdampak |
| Dependency backend | `NONE` untuk task ini — seluruh endpoint yang dibutuhkan sudah tersedia pada commit `3c4c06f`. `RWI-UI-GAP-006` ditutup dengan bukti source pada bagian 5 dan menunggu pengesahan pemilik modul |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend `HamzahV2`: `M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`, `M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`, `M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`, `M src/style/health-services/inpatient-management/inpatient-admission.module.css`, `?? src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx`, `?? src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx`, `?? src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx`, `?? src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js`, `?? src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx`. Tidak ada `git add`, commit, maupun push |
| Langkah berikutnya | Sediakan satu akun petugas admisi pada lingkungan uji, lalu jalankan verifikasi manual kriteria 1, 4, dan 5 — termasuk pemeriksaan jaringan penekanan simpan dua kali. Setelah itu `FE-RWI-024` dapat dimulai karena pasien terpilih sudah tersedia pada URL |


---

## Penutupan status — 1 September 2026

| Field | Isi |
| --- | --- |
| Status akhir | ✅ **SELESAI** |
| Dasar | Keputusan pemilik pekerjaan 1 September 2026: butir Definition of Done yang mensyaratkan test `.mjs`, E2E, atau uji manual **tidak lagi menahan status selesai** untuk task frontend yang seluruh acceptance criterianya sudah terpetakan ke source yang benar-benar ada. Dicatat pada [`frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian "Keputusan penutupan verifikasi" |
| Yang dikecualikan | Butir DoD e2e/`.mjs`/uji manual pada task ini |
| Yang tidak dihapus | Seluruh catatan verifikasi di atas tetap berlaku apa adanya. Alasan teknisnya — repository tanpa `playwright.config.*`, `npm run test:unit` gagal oleh `ERR_UNSUPPORTED_DIR_IMPORT` pada Node `v24.13.0`, dan data master rawat inap yang belum layak (`RWI-UI-GAP-007`) — tidak dianggap gugur |
| Pembuktian runtime ujung-ke-ujung | Tetap menjadi milik `FE-RWI-035` dan tidak dihapus dari roadmap |
| Register yang ikut diperbarui | [`frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), [`requirement-traceability.md`](../../../roadmap/requirement-traceability.md) |
