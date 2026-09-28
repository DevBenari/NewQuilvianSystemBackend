# Laporan Perubahan Frontend — `FE-HMD-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-13` |
| Judul | Dialog Penjadwalan Sesi HD dan Penugasan Staf Berperingatan Rasio/Kompetensi |
| Slice | `MVP-3` — Penjadwalan Sesi dan Daftar Kerja Harian Unit HD |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.4 |
| Trace | `FR-HMD-030`, `FR-HMD-031`, `FR-HMD-032`, `FR-HMD-034`, `FE-HMD-04`, `HMD-DEP-002`; `contracts/api-contract.md` grup Hemodialysis Schedule; `contracts/validation-matrix.md` Bagian 2 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak form dan pola interaksi konfirmasi. Peringatan rasio perawat wajib **tidak memblokir** simpan sesuai roadmap |
| Keputusan UI Gate | **12 elemen**: `REUSE 10, EXTEND 0, COMPOSE 1, WRAP 1, NEW 0` |
| Dependency | `FE-HMD-12` (selesai 23 September 2026), `BE-HMD-10` (selesai 22 September 2026) |
| Klasifikasi | `MEDIUM` — skor 9: repository 0, berkas diperiksa 12, berkas dibuat 4, berkas diubah 4, logika 3, kontrak API 3, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 24 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. `FE-HMD-12` sudah memasang tombol "Jadwalkan Sesi Baru", keadaan buka/tutup dialog, dan kerangka `schedule-session-modal.jsx` yang badannya masih berupa pemberitahuan "formulir penjadwalan belum tersedia".
2. Thunk `createScheduledSession` sudah ada sejak `FE-HMD-02` dan memanggil `POST /hemodialysis-sessions` dengan benar, tetapi **kode validasi domain dari backend tidak pernah sampai ke layar**. `normalizeHmdError` menyimpan body `errors` ke `error.details`, sedangkan `readErrorPayload` pada slice worklist hanya meneruskan `message` dan `statusCode`. Tanpa kode itu, layar tidak dapat mengetahui apakah `409` berasal dari benturan pasien, mesin, atau station.
3. Belum ada sumber opsi perawat yang memakai `WorkforceProfileId`. Registry `employees` pada `hr-select-resources.js` memakai `valueKey: "id"`, yaitu `Id` pegawai, sedangkan `HmdStaffAssignmentInput.WorkforceProfileId` menuntut identitas profil tenaga kerja.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Koordinator unit hemodialisa yang memegang hak `HemodialysisSchedule : Create`.

**Alur normal.**

1. Dari layar Jadwal & Daftar Kerja, koordinator menekan **+ Jadwalkan Sesi Baru**. Dialog terbuka dengan tanggal yang sedang aktif di layar worklist sudah terisi.
2. Koordinator memilih **pasien**. Daftarnya sengaja hanya memuat pasien yang programnya berstatus aktif **dan** sudah punya resep HD aktif — pasien tanpa resep aktif pasti ditolak server, jadi tidak ditawarkan sejak awal.
3. Bila pasien yang dipilih memerlukan isolasi, muncul pita kuning "Pasien ini memerlukan alokasi isolasi", dan daftar mesin serta station langsung dipersempit hanya ke sumber daya berdedikasi. Mesin reguler tidak ditawarkan sama sekali.
4. Koordinator mengisi **jam mulai**. Shift terisi sendiri dari jam itu — 07.00 menjadi Pagi, 13.00 menjadi Siang, 18.00 menjadi Sore — dan tetap boleh diubah manual.
5. Koordinator memilih **mesin** dan **station**. Yang muncul hanya mesin berstatus siap operasional, aktif, dan boleh dijadwalkan; serta station berstatus tersedia dan aktif.
6. Koordinator memilih **dokter penanggung jawab sesi** dan **perawat pendamping**.
7. Bila perawat yang dipilih sudah menangani pasien sebanyak batas rekomendasi unit pada shift itu, muncul kotak peringatan kuning: "Peringatan: Rasio perawat melampaui rekomendasi (3 pasien per perawat). Ns. Dewi sudah menangani 3 pasien pada shift ini. Penjadwalan tetap dapat disimpan." Batas tiga itu dibaca dari pengaturan unit, bukan angka tetap di kode.
8. Koordinator menekan **Jadwalkan Sesi**. Dialog tertutup dan daftar kerja di belakangnya langsung tersegarkan sehingga sesi baru terlihat.

**Jalur tidak normal.**

- **Field wajib kosong** — dialog tidak mengirim apa pun; pesan merah muncul di bawah field yang bersangkutan.
- **Jam selesai tidak setelah jam mulai** — "Jam selesai wajib setelah jam mulai."
- **Benturan jadwal** — server menjawab `409` dan field sumbernya disorot. Mesin `M-01` yang sudah terisi jam 08.00–12.00 memunculkan pesan pada field mesin: "Mesin ini sudah terpakai pada rentang jam tersebut. Pilih mesin lain yang masih kosong." Benturan pasien menyorot field pasien, benturan station menyorot field station.
- **Mesin tidak memenuhi kebutuhan isolasi** — server menjawab `422 HMD-VAL-035` dan field mesin disorot beserta sarannya.
- **Mengganti pasien setelah mesin dipilih** — bila kebutuhan isolasinya berubah, pilihan mesin dan station yang menjadi tidak sah otomatis gugur dan tidak ikut terkirim.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend sebagai sumber kontrak (read-only).** `Controllers/HmdScheduleController.cs`; `Services/HmdScheduleService.cs`; `Services/HmdOperationResult.cs` (kode dan pesan `HMD-VAL-031` s/d `HMD-VAL-035`); `Controllers/HmdHttp.cs` (bentuk body kegagalan `errors: { code, details }`); `DTOs/HmdSessionDtos.cs`; `DTOs/HmdResourceDtos.cs`; `DTOs/HmdEpisodeDtos.cs`; `Enums/HemodialysisEnums.cs`; `Models/HmdSession.cs`; `Models/HmdSessionStaffAssignment.cs`; `Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs`; `DTOs/EmployeeDtos.cs`.

**Frontend sebagai rujukan pola.** `base-features/confirm-modal.jsx`, `filter-select.jsx`, `filter-time-picker.jsx`, `resource-filter-select.jsx`, `information-alert.jsx`; `lib/hooks/select/use-select-resource.jsx`; `lib/hooks/select/hr/hr-select-resources.js`; modal modul `master-data/machines/modals/create-edit-machine-modal.jsx`; slice `hemodialysisMasterSlice.js` dan `hemodialysisEpisodeSlice.js`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/hemodialysis-management/hemodialysis-schedule-display-utils.js` | **Baru.** Fungsi murni: `inferShiftFromTime`, `buildEligibleEpisodeOptions`, `buildEligibleMachineOptions`, `buildEligibleStationOptions`, `evaluateNurseRatio`, `resolveScheduleConflictField`, `combineDateAndTime`, `validateScheduleForm`, `buildCreateSessionPayload` |
| `src/lib/hooks/health-services/hemodialysis-management/use-hemodialysis-schedule-dialog.jsx` | **Baru.** Controller dialog: pembacaan sumber daya saat dibuka, penurunan shift, penggugatan pilihan yang tidak sah, penghitungan beban perawat, pengiriman, dan pemetaan benturan ke field |
| `src/components/view/health-services/hemodialysis-management/worklist/modals/schedule-session-modal.jsx` | Kerangka diganti dialog penjadwalan lengkap sepuluh field |
| `src/components/view/health-services/hemodialysis-management/worklist/modals/schedule-form-field.jsx` | **Baru.** Pembungkus tipis label, penanda wajib, teks bantuan, dan teks kesalahan per field |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisWorklistSlice.js` | `readErrorPayload` meneruskan `errorCode` dari `errors.code` supaya layar dapat menyorot field sumber benturan |
| `src/components/view/health-services/hemodialysis-management/worklist/hemodialysis-worklist-view.jsx` | Dialog dirender hanya saat terbuka, menerima `defaultDate`, dan memicu penyegaran daftar setelah sesi terjadwal |
| `src/style/health-services/hemodialysis-management/hemodialysis-worklist.module.css` | Kelas tata letak form dialog; seluruhnya memakai `var(--token)` |
| `tests/unit/hemodialysis-schedule-dialog.test.mjs` | **Baru.** 24 unit test fungsi murni dialog |

### 3.3 Kepatuhan Arsitektur Frontend

```text
view/.../worklist/modals/schedule-session-modal.jsx  -> komposisi dialog
  -> lib/hooks/.../use-hemodialysis-schedule-dialog.jsx -> controller
  -> lib/state/slice/.../hemodialysisWorklistSlice.js   -> createScheduledSession
  -> lib/services/.../hmdScheduleService.js             -> InstanceAxios -> Backend API
```

Modal tidak memanggil Axios langsung. Pembacaan daftar kerja untuk menghitung beban perawat memakai service `getHmdWorklist` langsung dari hook — dibenarkan `rules/frontend/frontend-architecture.md` untuk operasi satu kali yang tidak perlu masuk Redux global, dan sengaja tidak memakai thunk worklist supaya tidak menimpa daftar yang sedang ditampilkan layar di belakang dialog.

Sumber opsi perawat memakai kemampuan yang sudah ada: `useSelectResource` menerima objek konfigurasi lepas, sehingga `valueKey: "workforceProfileId"` cukup dinyatakan di tempat pemakaian tanpa menambah registry baru.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Setiap select membawa teks memuatnya sendiri: "Memuat daftar dokter...", "Memuat daftar petugas...", dan keadaan `loading` pada select pasien, mesin, serta station |
| Kosong | "Tidak ada pasien dengan episode dan resep aktif", "Tidak ada mesin siap yang cocok untuk pasien ini", "Tidak ada station tersedia yang cocok untuk pasien ini" |
| Gagal | `InformationAlert` merah memuat pesan server. Untuk benturan yang dikenali, field sumbernya ikut disorot beserta saran tindakan |
| Tanpa hak akses | Tombol pembuka dialog tidak dirender bagi pengguna tanpa `HemodialysisSchedule : Create`, sehingga dialog ini tidak dapat dicapai |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions` | Menjadwalkan sesi baru | `HemodialysisSchedule : Create` |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions/worklist` | Menghitung beban perawat pada tanggal dan shift terpilih | `HemodialysisSchedule : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-episodes` | Daftar pasien yang sah dijadwalkan | `HemodialysisEpisode : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Machine, Station, Setting

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../master-data/hemodialysis-machines` | Opsi mesin beserta status dan dedikasi isolasinya | `HemodialysisMachine : Read` |
| `GET` | `.../master-data/hemodialysis-stations` | Opsi station beserta status dan penanda isolasinya | `HemodialysisStation : Read` |
| `GET` | `.../master-data/hemodialysis-settings/{serviceUnitId}` | Batas pasien per perawat untuk peringatan rasio | `HemodialysisSetting : Read` |

#### Corporate / Human Resource / Master Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/doctors/options` | Opsi dokter penanggung jawab sesi | `Doctor : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/employees/options` | Opsi perawat pendamping, dibaca lewat `workforceProfileId` | `Employee : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <8 berkas task ini>` | 0 error, 0 warning | `PASS` | Keluaran perintah kosong, exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-schedule-dialog.test.mjs` | 24 test, 24 lolos, 0 gagal | `PASS` | Berkas test baru milik task ini |
| Grep anti-regresi konsistensi UI, 8 pemeriksaan | Seluruhnya kosong | `PASS` | Tidak ada warna literal, penimpaan typography, `<button>` mentah, `<table>` mentah, utility typography Bootstrap, `!important`, inline style, maupun blok dark mode |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Sesi ini tidak memiliki server pengembangan frontend maupun backend yang berjalan beserta sesi login berotorisasi |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

**Bug yang ditemukan unit test dan diperbaiki sebelum task ditutup.** `inferShiftFromTime("")` semula mengembalikan shift Pagi, bukan `null`, karena `Number("")` bernilai `0` dan lolos pemeriksaan rentang jam. Akibatnya field jam mulai yang masih kosong akan diam-diam menetapkan shift Pagi pada payload. Penjagaan jam kosong ditambahkan, dan pemeriksaannya dipertahankan di berkas test.

Kontrol interaktif yang **belum** dibuktikan di peramban: pembukaan dan penutupan dialog, pengisian sepuluh field, penurunan shift otomatis dari jam mulai, penyempitan daftar mesin/station saat pasien isolasi dipilih, munculnya peringatan rasio perawat, penyorotan field saat `409`, dan penyegaran daftar kerja setelah sesi tersimpan.

Yang **sudah** dibuktikan di tingkat logika oleh 24 unit test: penurunan shift untuk keenam batas jam beserta empat masukan tidak sah, penyaringan pasien atas kombinasi status episode dan keberadaan resep aktif, penyaringan mesin atas status-dedikasi-keaktifan-kelayakan jadwal untuk pasien umum maupun isolasi, penyaringan station, penghitungan rasio perawat termasuk batas dari pengaturan unit dan pengabaian baris perawat lain, pemetaan kelima kode benturan ke field beserta sarannya, bentuk penanda waktu lokal tanpa akhiran `Z`, validasi seluruh field wajib dan urutan jam, serta bentuk payload `CreateHmdSessionRequest` termasuk pembuangan field opsional yang kosong.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, dan `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Koordinator memilih mesin `M-01` untuk jam 08.00–12.00; bila mesin sudah terisi, sistem memunculkan pesan validasi benturan | Terpenuhi | `resolveScheduleConflictField` memetakan `HMD-VAL-032` ke field `machineId` beserta pesan "Mesin ini sudah terpakai pada rentang jam tersebut. Pilih mesin lain yang masih kosong." Pencegahan di hulu: daftar mesin hanya memuat mesin berstatus siap, dan mesin yang tidak cocok isolasi tidak pernah ditawarkan |
| **AC-2** Perawat Dewi sudah ditugaskan pada 3 pasien di shift pagi; saat dipilih untuk pasien ke-4 muncul kotak peringatan kuning | Terpenuhi | `evaluateNurseRatio` menghasilkan pesan "Peringatan: Rasio perawat melampaui rekomendasi (3 pasien per perawat)..." dan view merender `InformationAlert variant="warning"`. Batas dibaca dari `maxPatientsPerNurse` pengaturan unit. Peringatan tidak mengunci tombol simpan |

**Definition of Done** — "Dialog penjadwalan terpasang pada layar worklist, pengujian alokasi sumber daya dan umpan balik benturan tervalidasi":

| Butir | Status |
| --- | --- |
| Dialog terpasang pada layar worklist | Terpenuhi |
| Pemilihan pasien dibatasi pada episode dan resep aktif | Terpenuhi |
| Tanggal dan rentang jam pelayanan | Terpenuhi |
| Pilihan mesin hanya berstatus `Ready` dan menyaring kompatibilitas isolasi | Terpenuhi |
| Pilihan station yang tersedia | Terpenuhi, dengan catatan di bagian 8 |
| Pilihan DPJP dan perawat pendamping | Terpenuhi |
| Peringatan rasio perawat yang tidak memblokir simpan | Terpenuhi |
| Penanganan `409` dengan penyorotan sumber benturan | Terpenuhi |
| Component test `ScheduleDialogTests` | Tidak dipenuhi dalam bentuk yang disebut roadmap. Repository tidak memakai Jest maupun `@testing-library`; digantikan 24 unit test `node:test`. Butir DoD berbentuk test dilepas atas keputusan pemilik 1 September 2026 |

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Empat peringatan `react-hooks/set-state-in-effect` dan `exhaustive-deps` sempat muncul dan **sudah dihilangkan**: kesahihan mesin/station diturunkan saat render alih-alih disimpan ulang lewat effect, dialog dirender hanya saat terbuka sehingga tidak perlu effect pengosong form, dan fallback daftar kosong memakai satu rujukan tetap. Hasil akhir 0 error 0 warning |
| Masalah yang diketahui | **Satu keterbatasan kontrak.** Penyaringan station belum memeriksa ketersediaan **pada jam tertentu**; `HmdStationResponse` hanya membawa status master `Available`/`Blocked`/`Maintenance` dan backend tidak menyediakan endpoint ketersediaan station per rentang waktu. Benturan jam station tetap tertangkap server lewat `409 HMD-VAL-033` dan disorot di layar, tetapi station yang sudah terisi masih ikut muncul di daftar. Menutup celah ini memerlukan endpoint backend baru dan berada di luar cakupan task. **Satu keterbatasan lain**: peringatan rasio perawat mencocokkan berdasarkan nama tampilan karena `HmdWorklistItemResponse` membawa `PrimaryNurseName` tanpa identitas perawat; dua orang bernama persis sama akan terhitung satu. Ditambah tiga cacat modul di luar cakupan yang sudah dilaporkan pada `FE-HMD-12.md` bagian 8 dan masih belum diperbaiki |
| Dependency backend | Tidak ada yang tertahan. `BE-HMD-10` dan `BE-HMD-11` selesai 22 September 2026 |
| Perubahan sampingan | `readErrorPayload` pada slice worklist ditambah field `errorCode`. Penambahan ini dibutuhkan task untuk menyorot field sumber benturan dan tidak mengubah perilaku pemakai lama |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch. Berkas baru task ini: empat berkas source dan satu berkas test; berkas yang diubah: empat |
| Langkah berikutnya | Kerjakan `FE-HMD-14` — ruang kerja sesi tahap Pra-HD, menggantikan halaman sementara `/sessions/{sessionId}` yang dipasang `FE-HMD-12` |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 12 elemen — REUSE 10, EXTEND 0, COMPOSE 1, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Wadah dialog | `ConfirmModal` (`hideFooter`) | `REUSE` | Sudah dipakai sebagai kerangka sejak `FE-HMD-12` |
| Pemilihan pasien, shift, mesin, station | `FilterSelect` | `REUSE` | Empat select ber-search dengan opsi domain |
| Tanggal pelayanan | `FilterDatePicker` | `REUSE` | — |
| Jam mulai dan jam selesai | `FilterTimePicker` | `REUSE` | `format={24}`, `minuteStep={5}` |
| Dokter penanggung jawab | `ResourceFilterSelect` + `useSelectResource("doctors")` | `REUSE` | — |
| Perawat pendamping | `ResourceFilterSelect` + `useSelectResource(<config lepas>)` | `REUSE` | `valueKey: "workforceProfileId"` lewat konfigurasi lepas yang memang didukung hook |
| Peringatan rasio perawat | `InformationAlert variant="warning"` | `REUSE` | — |
| Pita kebutuhan isolasi | `InformationAlert variant="warning"` | `REUSE` | — |
| Pesan kegagalan pengiriman | `InformationAlert variant="danger"` | `REUSE` | — |
| Tombol Batal dan Jadwalkan Sesi | `BaseButton` | `REUSE` | `loading` plus `loadingLabel` pada aksi kirim |
| Penyorotan field sumber benturan | `InformationAlert` + teks kesalahan per field | `COMPOSE` | Dirangkai di view tanpa perubahan base |
| Label, penanda wajib, teks bantuan, teks kesalahan per field | `ScheduleFormField` | `WRAP` | Pembungkus tipis khusus domain di `components/view`, memakai token yang sama dengan `BaseFormControl`. Dipilih karena kontrol yang dipakai adalah select ber-search yang tidak dibungkus `BaseFormControl`, dan alternatifnya — `<select>` polos atau `Form.Group` Bootstrap — masing-masing kehilangan pencarian atau mencampur dua bahasa visual |
