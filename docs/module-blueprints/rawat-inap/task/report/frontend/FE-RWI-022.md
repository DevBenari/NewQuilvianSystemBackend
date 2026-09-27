# Laporan Perubahan Frontend — `FE-RWI-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-022` |
| Judul | Kerangka alur admisi dua jalur berdiri |
| Slice | Task revision 3–5; layar `FE-INP-03` — alur admisi berlangkah |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` revision `5`, kartu `FE-RWI-022` |
| Trace | `RWI-DEC-075`; `03-frontend-architecture.md` bagian 3A.1, 3A.2 langkah 1, 3A.3 langkah 1–3, dan 5.5; `05-skema-tampilan.md` bagian 3.0–3.2 serta 3.4 |
| Contract version | Tidak mengonsumsi API. Kontrak UI mengikuti urutan bernama `RWI-DEC-075`; pemilik menyetujui interpretasi `RWI-UI-GAP-001` pada 2026-08-29: seluruh langkah bernama ditampilkan tanpa mengunci tulisan “8 langkah” untuk pasien lama |
| Wewenang UI | `RWI-FE-003` untuk nama/label langkah dan `RWI-FE-004` untuk bentuk penanda; urutan dan isi langkah tetap mengikat |
| Dependency | `NONE` — kartu task tidak mempunyai dependency backend atau frontend |
| Klasifikasi | `MEDIUM` — enam berkas source berubah/ditambah, dua komponen pendaftaran IGD diperluas lewat props opsional yang backward-compatible, dan state langkah dipindahkan ke URL; tidak ada API, Redux, backend source, atau dependency package baru |
| Task mode | `FRONTEND` dengan wewenang lintas repository terbatas untuk laporan tracked ini |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` untuk source; `NewQuilvianSystemBackend` branch `MHamzah` hanya untuk laporan ini |
| Model | GPT-5 (Codex) |
| Commit frontend saat dikerjakan | `cf036c75753e04a8c7d192112f3be23f20e25954` |
| Commit backend yang dijadikan rujukan | `4db8909e5c77b06aadf2603bd1617ccdcca093db` |
| Tanggal | 2026-08-29 |
| Status | **Implementasi 5/5 acceptance criteria tersedia; `npm run lint` dan `npm run build` lulus. Verifikasi E2E/manual tidak dijalankan atas instruksi pemilik** |

---

## 1. Keadaan yang ditemukan di awal

Route `/health-services/inpatient-management/admissions` masih membuka satu formulir besar. Formulir
itu langsung membuat episode, mencatat kebutuhan isolasi, dan menempatkan pasien pada tempat tidur.
Ia belum memiliki pilihan pasien baru/pasien lama, urutan langkah target, maupun langkah aktif yang
tersimpan pada URL.

Pola yang diwajibkan task sudah tersedia pada pendaftaran IGD, tetapi sebelum perubahan masih
terikat pada salinan teks dan lima langkah khusus IGD:

- `patient-entry-choice-step.jsx` hanya membaca `PATIENT_ENTRY_OPTIONS` milik IGD;
- `emergency-registration-stepper.jsx` hanya membaca `EMERGENCY_REGISTRATION_STEPS` milik IGD;
- route Rawat Inap belum dapat memasok daftar langkah atau label aksesibilitasnya sendiri.

Task ini mengganti komposisi route admisi menjadi kerangka dua jalur. Hook, constants, utility, dan
test formulir admisi lama sengaja tidak dihapus pada task ini karena pembersihan jalur lama dimiliki
task lanjutan `FE-RWI-035` setelah seluruh isi langkah tersedia.

---

## 2. Proses bisnis dari sisi pengguna

### Tujuan dan pelaku

Petugas admisi atau supervisor membuka menu Admisi Rawat Inap untuk memulai alur pasien baru atau
pasien yang sudah memiliki nomor rekam medis.

### Langkah utama

1. Petugas membuka `/health-services/inpatient-management/admissions`.
2. Sistem menampilkan dua kartu terpisah: **Pendaftaran Pasien Baru** dan **Pendaftaran Pasien Lama**.
3. Bila petugas memilih pasien baru, URL diarahkan ke `?entry=new&step=patient-type` dan penanda
   menampilkan sembilan langkah pasien baru.
4. Bila petugas memilih pasien lama, URL diarahkan ke `?entry=existing&step=existing-patient` dan
   penanda menampilkan seluruh langkah bernama pasien lama. Layar tidak menampilkan klaim jumlah
   resmi pasien lama selama `RWI-UI-GAP-001` belum ditutup.
5. Pada langkah **Tipe Pasien**, petugas memilih salah satu dari Umum, Ibu, Bayi Baru Lahir, Anak,
   Pegawai, atau Korporat.
6. Bila **Bayi Baru Lahir** dipilih, wilayah **Episode Ibu** muncul. Bila jenis lain dipilih, wilayah
   itu tidak dirender.
7. Untuk pasien baru berjenis selain Bayi Baru Lahir, tombol **Lanjut ke Pendaftaran** memindahkan
   langkah aktif ke `?entry=new&step=registration`.
8. Memuat ulang URL tersebut mempertahankan langkah Pendaftaran karena langkah dibaca dari query
   URL, bukan dari state React sementara.

### Jalur tidak normal

- Nilai `entry` atau `step` yang tidak dikenal dinormalisasi dengan `router.replace`: nilai jalur
  tidak valid kembali ke layar pilihan, sedangkan langkah tidak valid kembali ke langkah pertama
  jalur yang sah.
- Bayi Baru Lahir belum dapat melanjutkan tanpa episode ibu. Kontrol episode ibu sudah terlihat,
  tetapi pengambilan data episode berada di luar scope `FE-RWI-022`, sehingga layar menjelaskan
  keterbatasan itu dan tidak mengarang endpoint.
- Langkah selain **Tipe Pasien** menampilkan pemberitahuan bahwa kerangkanya tersedia tetapi isi
  operasional dimiliki task lanjutan. Layar tidak mengirim request palsu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/app/health-services/inpatient-management/admissions/page.jsx`
- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx`
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-constants.jsx`
- `src/components/view/health-services/registration-management/emergency-registration/patient-entry-choice-step.jsx`
- `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-stepper.jsx`
- `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-page.jsx`
- `src/components/features/base-features/base-checkbox-card.jsx`
- `src/components/features/base-features/base-button.jsx`
- `src/components/features/base-features/resource-filter-select.jsx`
- `src/components/features/base-features/hero.jsx`
- `src/components/features/base-features/information-alert.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Formulir tunggal pada route diganti komposisi Hero, pilihan dua jalur, stepper, langkah Tipe Pasien, kontrol Episode Ibu kondisional, dan placeholder jujur untuk langkah lanjutan |
| `src/components/view/health-services/registration-management/emergency-registration/patient-entry-choice-step.jsx` | Menambah props opsional `eyebrow`, `title`, `description`, dan `options`; nilai default mempertahankan tampilan IGD |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-stepper.jsx` | Menambah props opsional `steps`, `ariaLabel`, `className`, dan `style`; status selesai dihitung dari posisi array dan default IGD tetap sama |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | **Baru.** Mendefinisikan dua jalur, seluruh urutan langkah bernama, enam jenis pasien, query key, dan placeholder select Episode Ibu |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` | **Baru.** Membaca/menulis `entry` dan `step` pada URL, menormalisasi query tidak valid, mengelola mundur/lanjut, serta aturan kondisional Bayi Baru Lahir |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | **Baru.** Layout panel, stepper horizontal, grid jenis pasien, Episode Ibu, dan aksi menggunakan design token global |

Route `page.jsx` tetap tipis dan tidak perlu berubah karena sudah merender
`InpatientAdmissionView` dengan metadata yang benar.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi setelah perubahan:

```text
src/app/.../admissions/page.jsx
  -> inpatient-admission-view.jsx
  -> use-inpatient-admission-flow.jsx
  -> next/navigation untuk state URL
```

Tidak ada request API, Axios instance, Redux slice, HTTP service, package, atau komponen base baru.
State langkah yang harus bertahan saat reload tidak disimpan pada local React state. Local state
hanya dipakai untuk pilihan jenis pasien dan Episode Ibu yang belum mempunyai titik tulis pada task
shell ini.

---

## 4. UI gate dan state layar

`UI GATE: 6 elemen — REUSE 3, EXTEND 2, COMPOSE 1, WRAP 0, NEW 0`.

| Kebutuhan UI | Kandidat dan bukti | Status | Keputusan |
| --- | --- | --- | --- |
| Kepala halaman | `base-features/hero.jsx` | `REUSE` | `Hero` dipakai apa adanya |
| Pilihan dua jalur | `emergency-registration/patient-entry-choice-step.jsx` | `EXTEND` | Props opsional memungkinkan copy Rawat Inap; default IGD tidak berubah |
| Penanda langkah | `emergency-registration/emergency-registration-stepper.jsx` | `EXTEND` | Props opsional menerima langkah Rawat Inap; default IGD tidak berubah |
| Enam jenis pasien | `base-features/base-checkbox-card.jsx` | `COMPOSE` | Enam kartu dirangkai sebagai satu pilihan tunggal tanpa varian base baru |
| Episode ibu | `base-features/resource-filter-select.jsx` | `REUSE` | Kontrol hanya dirender ketika Bayi Baru Lahir dipilih |
| Aksi dan pemberitahuan | `base-button.jsx`, `information-alert.jsx` | `REUSE` | Tidak ada raw button baru pada view Rawat Inap |

Pilihan yang dipilih untuk kedua extension adalah props opsional karena paling kecil risiko
regresinya dan memenuhi kewajiban reuse. Alternatif membuat kartu atau stepper khusus Rawat Inap
ditolak karena menduplikasi pola. Untuk jenis pasien, komposisi `BaseCheckboxCard` dipilih daripada
menambah varian radio global yang belum dibutuhkan pemakai lain.

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — task tidak membaca API atau data server |
| Kosong | Layar pembuka selalu menampilkan dua jalur; tidak ada permukaan kosong |
| Gagal | Query URL tidak valid dinormalisasi; integrasi Episode Ibu yang belum tersedia dijelaskan lewat `InformationAlert` |
| Tanpa hak akses | Tidak menambah pemeriksaan baru; route tetap berada di shell autentikasi dan akses aplikasi existing. Task ini tidak mengubah permission |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — `FE-RWI-022` adalah task kerangka UI dan tidak memanggil endpoint. Data Episode
Ibu tidak ditebak atau diambil dari endpoint yang belum dikontrak untuk task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` melalui PowerShell | Tidak masuk ke npm karena `npm.ps1` diblokir execution policy Windows | `EXISTING / ENVIRONMENT ISSUE` | `PSSecurityException`; tidak ada perubahan policy sistem |
| `npm.cmd run lint` | Selesai dengan `0 error` dan `566 warning` existing di repository | `PASS` | Exit code `0`; warning tidak berasal dari error task |
| `npm.cmd run build` | Kompilasi Next.js berhasil, TypeScript check selesai, 245 halaman statis dibuat, dan route admisi tercantum | `PASS` | Exit code `0`; lifecycle `postbuild` menyiapkan standalone runtime |
| `git diff --check` | Tidak ada whitespace error | `PASS` | Hanya peringatan normalisasi LF/CRLF working copy Windows |
| Grep anti-regresi stylesheet baru | Tidak ada warna literal, `rgb/rgba/hsl`, override typography, `!important`, atau dark-mode block | `PASS` | Hasil pencarian kosong |
| Grep anti-regresi view Rawat Inap | Tidak ada raw `<button>`, raw `<table>`, class Bootstrap button, atau utility typography Bootstrap | `PASS` | Hasil pencarian kosong |
| Reload langkah dari browser | Tidak dijalankan karena pengguna membatasi validasi pada lint dan build | `NOT RUN` | State URL dibuktikan pada source dan build, bukan diklaim sebagai uji runtime |

`AUTOMATED TEST: SKIPPED — unit test dan E2E berbasis .mjs tidak dijalankan sesuai instruksi pengguna.`

`MANUAL TEST: NOT FEASIBLE — tidak menjalankan development server atau sesi browser karena command yang diizinkan hanya npm run lint dan npm run build.`

Catatan: `npm run build` menjalankan lifecycle `postbuild` milik repository,
`scripts/prepare-standalone.mjs`. Itu bagian otomatis dari command build yang diizinkan, bukan
perintah test `.mjs` terpisah.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Dua jalur masuk tersedia dan terpisah | Terpenuhi pada source/build | `INPATIENT_ADMISSION_ENTRY_OPTIONS` dan `PatientEntryChoiceStep`; setiap kartu menulis nilai `entry` berbeda ke URL |
| Sembilan langkah pasien baru dan seluruh langkah bernama pasien lama tampil berurutan tanpa mengunci jumlah resmi pasien lama | Terpenuhi pada source/build | `INPATIENT_ADMISSION_NEW_PATIENT_STEPS` dan `INPATIENT_ADMISSION_EXISTING_PATIENT_STEPS`; tidak ada tulisan “8 langkah” pada source task |
| Langkah aktif dan yang sudah lewat terbeda | Terpenuhi pada source/build | `EmergencyRegistrationStepper` memasang `stepItemActive` pada langkah saat ini dan `stepItemCompleted` pada indeks sebelumnya |
| Reload memulihkan langkah dari URL | Terpenuhi pada implementasi; runtime belum diuji | `useSearchParams` membaca `entry`/`step`; `router.push`/`router.replace` menulis query; tidak ada local state untuk langkah aktif |
| Bayi Baru Lahir menampilkan Episode Ibu dan jenis lain tidak | Terpenuhi pada source/build | `isNewborn` mengendalikan render `ResourceFilterSelect`; tombol lanjut menuntut `motherEpisodeId` khusus Bayi Baru Lahir |

Kelima kriteria tersedia pada implementasi dan build lulus. Definition of Done belum diberi klaim
verifikasi runtime penuh karena skenario E2E yang disebut roadmap sengaja tidak dijalankan atas
instruksi pengguna.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` perlu dipanggil melalui `npm.cmd` pada PowerShell karena execution policy memblokir wrapper `npm.ps1` |
| Masalah yang diketahui | Integrasi daftar Episode Ibu belum ada pada task shell; kontrol sudah kondisional dan tombol lanjut tetap aman. Test E2E admisi lama belum diarahkan ke flow baru karena test `.mjs` tidak disentuh pada pekerjaan ini |
| Dependency backend | `NONE`; task tidak memanggil backend |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu interupsi pengguna terjadi sebelum penulisan source; pekerjaan dilanjutkan setelah status Git diverifikasi dan batas validasi dikunci ulang |
| Status Git frontend | Enam berkas source task berubah/baru pada branch `HamzahV2`; tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Status Git backend | Perubahan roadmap/traceability dan laporan `FE-RWI-021.md` sudah ada sebelum task ini. Task ini hanya menambah `task/report/frontend/FE-RWI-022.md` dan tidak mengubah roadmap maupun traceability sesuai instruksi pengguna |
| Langkah berikutnya | Isi langkah Pasien Lama, Informasi Pasien Lama, dan Pendaftaran melalui `FE-RWI-023`; jalankan skenario E2E reload bila pemilik membuka izin test `.mjs` |
