# Laporan Perubahan Frontend — `FE-RWI-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-043` |
| Judul | Ruang kerja dokter berdiri di atas konteks pasien yang pasti |
| Slice | `DOK-MVP-FE` urutan 2 — ruang kerja dan konteks pasien |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian 3 task `FE-RWI-043`; gerbang §4.1 butir 1, 2, 3, 4, 5, 18, 19, 20 |
| Trace | `FE-DOK-01`; `03-frontend-architecture.md` §3.1; `INV-DOK-01`, `INV-DOK-02`; rules §1.1 butir 3–8 roadmap revision 2 |
| Contract version | `0.3.0`. Seluruh endpoint dibaca apa adanya dari source backend as-is; tidak ada endpoint baru yang dikarang, dan tidak satu pun permintaan tulis dikirim task ini |
| Wewenang UI | [`skema-tampilan-dokter-rawat-inap.md`](../../../skema-tampilan-dokter-rawat-inap.md) §2–6, §14–20, §22 — urutan komposisi, isi konteks keselamatan, enam tab, state handling, dan responsive. Jarak serta dekorasi yang tidak dikunci mengikuti design token existing |
| Dependency | `FE-RWI-042` ✅ selesai 7 September 2026; `BE-RWI-044` ✅ selesai 4 September 2026 (`dotnet test` SQLite `Failed: 0, Passed: 262`) |
| Klasifikasi | `HEAVY` — skor 11: repository 2, berkas diperiksa >20 (2), berkas diubah >8 (2), logika bisnis sedang (1), memakai kontrak API existing (1), database 0, keamanan/auth berkaitan tetapi bukan intinya (1), UI/workflow banyak halaman (2) |
| Task mode | `CROSS-REPO` sempit — source hanya frontend; pada repository backend hanya laporan ini, roadmap, dan `requirement-traceability.md` sub-modul yang sama |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/` untuk laporan, roadmap, dan traceability |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | Dikerjakan di atas `30db3734a5d1e1ed0de35197ffabc30ae9c8d4e3` pada branch `HamzahV2` — commit `FE-RWI-042` milik pemilik pekerjaan. Perubahan task ini **belum di-commit** |
| Commit backend yang dijadikan rujukan | `350361c7c489e8d3e861908fcc8410ea46bbdabf` pada branch `MHamzah` — sama dengan `focused_review_source_sha.backend` roadmap |
| Tanggal | 7 September 2026 |
| Status | ✅ **SELESAI 7 September 2026.** Keenam acceptance criteria fungsional dan keenam acceptance visual terpetakan ke source yang benar-benar ada. `npm run lint:errors` 0 error; `npm run test:unit` **458/458** lulus (16 di antaranya milik task ini); `npm run build` berhasil; **22 skenario peramban lulus** di Edge — 13 skenario `FE-RWI-043` ditambah 9 skenario regresi `FE-RWI-042`. Isi dokumentasi keenam tab **memang belum ada** dan itu bukan scope task ini; masing-masing dikerjakan `FE-RWI-044` s.d. `FE-RWI-049` |

---

## 1. Keadaan yang ditemukan di awal

`FE-RWI-042` sudah memindahkan pintu masuk dokter ke daftar pasien berbasis census dan membuka route `…/episodes/{id}/physician`. Yang berdiri di route itu baru sebuah **entry sementara**: ia menampilkan tujuh baris identitas episode beserta dua tautan, dan tidak memiliki satu pun perangkat keselamatan yang dituntut kontrak:

1. Tidak ada Patient Safety Context yang lengkap — hari rawat, lokasi/kamar/bed, diagnosis kerja, dan riwayat alergi sama sekali tidak dibaca.
2. Tidak ada penanda kewenangan. Layar tidak pernah menyatakan apakah pengguna yang membuka episode itu memang DPJP yang berlaku.
3. Tidak ada mekanisme yang menutup jalur tulis ketika konteks gagal. Konsekuensinya baru terasa pada task berikutnya, dan justru karena itu harus dipasang lebih dulu.
4. Tidak ada navigasi tab. Enam tab dokumentasi belum punya tempat, sehingga `FE-RWI-044` dan seterusnya tidak punya titik pasang.
5. Base klinis yang dijanjikan §1.2 baru satu: `ClinicalStateBoundary`. `ClinicalSafetyAlert` dan `ClinicalActionGuard` belum ada.
6. `ClinicalTabNav` yang sudah ada belum menghubungkan tab dengan panelnya dan belum mendukung navigasi papan ketik. Komponen ini **belum dipakai layar mana pun**, sehingga memperluasnya tidak berisiko regresi.

Yang sudah tersedia dan karena itu dipakai ulang: endpoint episode, penempatan tempat tidur, penugasan DPJP, alergi aktif, dan diagnosis pasien pada backend; utilitas `normalizeEpisodeDetail`, `normalizePlacements`, `resolveCurrentLocation`, `normalizeDoctorAssignments`; katalog `doctor-clinical-base`; serta pola hook dan service inpatient yang sudah baku.

---

## 2. Proses bisnis dari sisi pengguna

Penggunanya adalah **dokter** yang akan mendokumentasikan perawatan satu pasien rawat inap.

### 2.1 Alur normal

1. Dokter membuka **Dokter → Rawat Inap**, memilih satu pasien, lalu menekan **Buka Workspace**. Ruang kerja terbuka pada alamat `…/episodes/<id-episode>/physician`. Jalur lewat Census dan Detail Episode juga bermuara ke alamat yang sama.
2. Selagi konteks dibaca, layar menampilkan "Membuka konteks pasien..." beserta kerangka baris. Belum ada satu pun tab yang muncul, dan belum ada satu pun area tulis yang aktif.
3. Setelah konteks siap, **Patient Safety Context** tampil di bagian paling atas dan memuat sepuluh hal: nama pasien, No. RM, nomor episode, lokasi/kamar/bed, hari rawat, DPJP, diagnosis kerja, status episode, penanda alergi, dan penanda kewenangan pengguna.
4. Contoh yang terbaca di layar: **Ny. Sari Melati**, No. RM RM-000003 · Episode RWI-2026-000003, "Rawat Inap Melati / Kamar 302 / Bed 3", "Hari rawat ke-3", DPJP dr. Andi Pratama, "Pneumonia (J18.9)", status "Sedang dirawat", peringatan merah **"Alergi tercatat: Amoxicillin"** beserta bobot "Mengancam jiwa", dan kalimat hijau "Anda berwenang melakukan dokumentasi pada episode ini".
5. Di bawah konteks berdiri enam tab persis: **Kajian Medis, Catatan Perkembangan, Catatan Terpadu, Visite, Resep & Tindakan, Penunjang**. Tab dipilih dengan klik maupun tombol panah kiri/kanan, `Home`, dan `End`.
6. Konteks keselamatan **tidak ikut menggulir hilang**. Pada layar lebar ia menempel di atas; pada layar kecil ringkasan menetap berisi nama pasien, No. RM, nomor episode, dan keadaan alergi tetap terlihat.
7. Isi dokumentasi masing-masing tab dibuka pada task klinis berikutnya. Yang sudah berlaku sekarang adalah penjagaannya: setiap area tulis berada di dalam penjaga kewenangan yang sama.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| ID episode pada alamat tidak sah | "Data pasien tidak dapat dimuat" — tanpa tab, tanpa area tulis |
| Episode gagal dibaca (500) | Sama, beserta tombol **Coba Lagi** yang memuat ulang seluruh sumber |
| Episode yang dijawab server berbeda dari yang diminta | Diperlakukan sebagai kegagalan konteks, bukan diisi diam-diam |
| Tanpa izin membaca episode | "Akses ruang kerja ditolak" — tidak ada data pasien yang ditampilkan |
| Episode tidak ditemukan | "Episode tidak ditemukan" beserta arahan kembali ke daftar pasien |
| Alergi gagal dibaca | Peringatan merah **"Riwayat alergi tidak dapat dimuat"** beserta tombol **Coba Lagi** — tidak pernah berubah menjadi "tidak ada alergi" |
| Alergi berhasil dibaca dan kosong | Kalimat biru "Tidak ada alergi aktif yang tercatat" — jelas berbeda dari kalimat di atas |
| Lokasi rawat gagal dibaca | Kolom lokasi berbunyi "Lokasi rawat tidak dapat dimuat" beserta tombol **Muat Ulang Lokasi**, bukan tanda hubung |
| Diagnosis gagal dibaca | Kolom diagnosis berbunyi "Diagnosis kerja tidak dapat dimuat" beserta tombol **Muat Ulang Diagnosis** |
| Penugasan DPJP gagal atau ditolak | "Anda belum berwenang melakukan dokumentasi pada episode ini" beserta alasannya; seluruh area tulis tertutup |
| Pengguna bukan DPJP yang berlaku | Sama, beserta kalimat "DPJP aktif: dr. Sinta Rahayu." |
| Akun tanpa identitas dokter | Area tulis tertutup dengan kalimat bahwa akun tidak terhubung ke data dokter |
| Episode sudah ditutup atau dibatalkan | Banner **"Episode telah ditutup"**, dokumentasi baru ditahan, dan disebutkan bahwa dokumen final yang berwenang tetap dapat dikoreksi lewat addendum |
| Dokter berpindah ke episode lain | Permintaan episode lama dibatalkan; ruang kerja baru tidak pernah terisi pasien sebelumnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap dan kontrak: `roadmap/frontend-roadmap.md` (revision 2) bagian 1.1, 1.2, 3, 4.1, 6; `skema-tampilan-dokter-rawat-inap.md` §2–6, §14–20, §22; `contracts/api-contract.md` bagian 1, 10, 11.
- Governance: `AGENTS.md` frontend, `rules/frontend/frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `design-tokens.md`, `page-composition-patterns.md`, `ui-consistency-checklist.md`, `test-policy.md`.
- Backend sebagai bukti perilaku as-is: `InpatientEpisodeController.cs` (detail episode, `doctor-assignments`), `PatientAllergyController.cs` (`active-alerts`), `PatientDiagnosisController.cs` (daftar diagnosis), `PatientAllergyDtos.cs`, `PatientDiagnosisDtos.cs`, `PatientAllergySeverity.cs`, `PatientAllergyCategory.cs`, `PatientDiagnosisType.cs`, `PatientDiagnosisStatus.cs`.
- Frontend: seluruh isi `src/components/ui/doctor-clinical-base/`, `inpatient-episode-utils.jsx`, `use-inpatient-episode-detail.jsx`, `bed-occupancy.service.js`, `inpatient-episode.service.js`, `patient-diagnosis.service.js`, `login-slice.jsx`, `base-button`, `globals.css`.

### 3.2 Berkas yang berubah

**Base component baru**

| Berkas | Perubahan |
| --- | --- |
| `src/components/ui/doctor-clinical-base/ClinicalSafetyAlert.jsx` + `clinical-safety-alert.module.css` | Peringatan keselamatan generik dengan tiga bobot (`critical`, `warning`, `info`), daftar rincian, dan slot aksi pemulihan. Base ini tidak menghitung risiko klinis apa pun |
| `src/components/ui/doctor-clinical-base/ClinicalActionGuard.jsx` + `clinical-action-guard.module.css` | Penjaga area/aksi klinis. `mode="hide"` menghilangkan aksi yang memang tidak dimiliki; `mode="disable"` membungkus isinya dengan `<fieldset disabled>` sehingga tombol dan field di dalamnya benar-benar mati, disertai alasan yang dibaca pengguna |

**Base component yang diperluas**

| Berkas | Perubahan |
| --- | --- |
| `src/components/ui/doctor-clinical-base/ClinicalTabNav.jsx` | Menambah relasi tab/panel (`id`, `aria-controls`), roving `tabIndex`, dan navigasi papan ketik `ArrowLeft`/`ArrowRight`/`Home`/`End`, beserta pembangun id `buildClinicalTabId`/`buildClinicalTabPanelId`. Tampilan, props lama, dan perilaku klik tidak berubah; komponen ini belum dipakai layar mana pun sebelum task ini |
| `src/components/ui/doctor-clinical-base/index.js` | Mengekspor dua base baru beserta kedua pembangun id |

**Domain — ruang kerja dokter**

| Berkas | Perubahan |
| --- | --- |
| `…/physician-workspace/physician-workspace-client.jsx` | Pembungkus client komponen ruang kerja |
| `…/physician-workspace/physician-workspace-view.jsx` | Shell: `ClinicalPageHeader → InpatientEpisodeHeader → ClinicalTabNav → TabContent`, dibungkus `ClinicalStateBoundary` untuk memuat/gagal/ditolak/kosong/baca-saja |
| `…/physician-workspace/physician-workspace-context.jsx` | Konteks React satu pasien satu episode; tab tidak menghitung ulang kewenangan maupun membaca ulang episode |
| `…/components/inpatient-episode-header.jsx` | Adapter `ClinicalContextBar` berisi sepuluh elemen konteks keselamatan, ditambah ringkasan menetap untuk layar kecil |
| `…/components/inpatient-allergy-alert.jsx` | Tiga keadaan alergi yang berbeda tegas: gagal, kosong, dan ada alergi |
| `…/components/physician-authority-indicator.jsx` | Penanda kewenangan beserta alasan dan tombol muat ulang kewenangan |
| `…/components/physician-workspace-tabs.jsx` | Enam tab; hanya panel aktif yang dirender, lengkap dengan `role="tabpanel"` dan `aria-labelledby` |
| `…/components/clinical-tab-placeholder.jsx` | Kerangka isi tab yang seluruhnya dibungkus `ClinicalActionGuard` |
| `…/tabs/**` (6 berkas) | Titik pasang `FE-RWI-044` s.d. `FE-RWI-049`, sesuai struktur folder yang dikunci skema §5 |

**Hook, service, util, constant, route, style**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/…/use-inpatient-physician-workspace.jsx` | Membaca lima sumber **terpisah** — episode, penempatan, penugasan DPJP, alergi, diagnosis — masing-masing dengan keadaan memuat/gagal/ditolak sendiri, `AbortController` per sumber, dan pembacaan ulang kewenangan saat layar difokuskan kembali |
| `src/lib/services/…/clinical-management/patient-allergy.service.js` | Service baru untuk alert alergi aktif. Sengaja tidak punya nilai bawaan saat gagal, supaya kegagalan tidak dapat menyamar sebagai daftar kosong |
| `src/lib/services/…/clinical-management/patient-diagnosis.service.js` | Menambah `getPatientDiagnoses` untuk membaca diagnosis pada kunjungan jangkar episode |
| `src/utils/…/inpatient-physician-workspace-utils.jsx` | Normalisasi alergi dan diagnosis, pemilihan diagnosis kerja, perhitungan hari rawat, dan `resolveWorkspaceWriteAccess` — satu tempat yang memutuskan boleh-tidaknya menulis |
| `src/lib/constants/…/inpatient-physician-constants.jsx` | Enam tab, prefix id tab, tujuh sebab penutupan tulis, serta kalimat baku konteks gagal dan alergi |
| `src/app/…/episodes/[id]/physician/page.jsx` | Route memuat shell ruang kerja; isinya tetap hanya entry point dan metadata |
| `src/style/…/physician-workspace.module.css` | Style ruang kerja, penanda kewenangan, area tab, dan ringkasan menetap layar kecil; seluruhnya memakai design token |

**Dihapus dan dirapikan**

| Berkas | Alasan |
| --- | --- |
| `…/doctor-inpatient/inpatient-physician-workspace-entry-view.jsx` | Entry sementara `FE-RWI-042` digantikan shell ruang kerja penuh; tidak ada lagi yang mengimpornya |
| `src/lib/hooks/…/use-inpatient-physician-workspace-entry.jsx` | Hook milik entry sementara di atas |
| `src/style/…/doctor-inpatient-entry.module.css` | Empat kelas yatim milik entry sementara (`contextGrid`, `contextItem`, `workspaceActions` beserta override media query-nya) dihapus; kelas milik daftar pasien tidak disentuh |
| `tests/unit/inpatient-physician-entry.test.mjs`, `tests/e2e/inpatient-physician-entry.spec.mjs` | Empat assertion `FE-RWI-042` disesuaikan dengan judul dan kalimat baru ruang kerja; cakupan ujinya tidak dikurangi |

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi tetap: route `src/app` hanya entry point dan metadata → view domain → hook → service Axios existing → utilitas. Tidak ada arsitektur state atau HTTP paralel yang ditambahkan; ruang kerja memakai state lokal seperti `use-inpatient-episode-detail`, bukan slice Redux baru.
- Struktur folder `physician-workspace/` beserta `components/` dan `tabs/**` mengikuti skema §5 yang sudah dikunci.
- Base component tetap generik: aturan DPJP, status episode, dan kebijakan verifikasi seluruhnya tinggal di adapter domain, tidak satu pun masuk ke `ClinicalSafetyAlert` maupun `ClinicalActionGuard`.
- Tidak ada nilai visual literal; warna lembut peringatan memakai `color-mix` di atas token, pola yang sudah dipakai stylesheet inpatient lain.

### 3.4 Gerbang keputusan base component

`UI GATE: 11 elemen — REUSE 6, EXTEND 1, COMPOSE 0, WRAP 2, NEW 2`

| Elemen | Keputusan | Alasan dan konsekuensi |
| --- | --- | --- |
| Header halaman | `REUSE` | `ClinicalPageHeader` apa adanya |
| Batas keadaan memuat/gagal/ditolak/kosong/baca-saja | `REUSE` | `ClinicalStateBoundary` milik `FE-RWI-042`, tanpa perubahan API |
| Bar konteks pasien | `REUSE` | `ClinicalContextBar` dipakai lewat props `infoItems`; responsivenya sudah 4 → 2 → 1 kolom sesuai §1.4 |
| Badge status episode | `REUSE` | `ClinicalStatusBadge` dengan tone dari adapter domain |
| Panel dan keadaan kosong isi tab | `REUSE` | `ClinicalSectionPanel` dan `ClinicalEmptyState` |
| Tombol aksi | `REUSE` | `BaseButton`; tidak ada tombol mentah pada view |
| Navigasi enam tab | `EXTEND` | Pilihan 1 **(rekomendasi, dipakai)** menambah relasi tab/panel dan navigasi papan ketik pada `ClinicalTabNav` — perilaku lama utuh, dan komponennya belum dipakai layar mana pun sehingga risiko regresi nol. Pilihan 2 membuat navigasi tab domain sendiri — tanpa menyentuh base, tetapi melahirkan navigasi kedua yang harus dirawat terpisah |
| Kepala episode beserta konteks keselamatan | `WRAP` | Pilihan 1 **(rekomendasi, dipakai)** adapter domain `InpatientEpisodeHeader` di atas base — aturan episode tidak masuk base. Pilihan 2 menambah props domain ke `ClinicalContextBar` — lebih ringkas, tetapi memasukkan aturan rawat inap ke komponen bersama |
| Penanda kewenangan | `WRAP` | Pilihan 1 **(rekomendasi, dipakai)** `PhysicianAuthorityIndicator` domain yang menampilkan hasil keputusan adapter. Pilihan 2 menaruh logika kewenangan di base — melanggar batas §1.2 dan menyebar aturan DPJP ke seluruh modul |
| Peringatan alergi dan kegagalan data | `NEW` | Pilihan 1 **(rekomendasi, dipakai)** `ClinicalSafetyAlert` generik — sudah dikunci kontrak task dan dipakai bersama `FE-RWI-044`–`FE-RWI-049`. Pilihan 2 menyusun peringatan per layar — tanpa base baru, tetapi peringatan alergi berisiko tampil berbeda-beda antar tab |
| Penjaga area tulis | `NEW` | Pilihan 1 **(rekomendasi, dipakai)** `ClinicalActionGuard` dengan `<fieldset disabled>` — satu mekanisme yang benar-benar mematikan kontrol, bukan sekadar meredupkan. Pilihan 2 menonaktifkan tombol satu per satu di setiap tab — tanpa base baru, tetapi satu tombol yang terlewat berarti tulisan terkirim tanpa konteks sah |

**Catatan atas status `NEW` dan `EXTEND`.** Keduanya tercantum eksplisit sebagai deliverable pada kartu task `FE-RWI-043` di roadmap yang sudah `APPROVED` (`New Base Components: Membuat ClinicalSafetyAlert, ClinicalActionGuard`; `Base Components To Extend: ClinicalTabNav dilengkapi relasi tab/panel dan navigasi keyboard generik bila belum tersedia`). Tabel pilihan di atas tetap disajikan; pekerjaan dilanjutkan atas dasar kontrak roadmap tersebut, dan tidak ada perilaku bawaan base yang berubah bagi konsumen lama — `ClinicalTabNav` memang belum punya konsumen sebelum task ini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membuka konteks pasien..." beserta kerangka baris; tab belum muncul dan seluruh jalur tulis ditahan |
| Kosong | "Episode tidak ditemukan" beserta arahan kembali ke daftar pasien |
| Gagal | "Data pasien tidak dapat dimuat" beserta penjelasan bahwa identitas belum terverifikasi dan tombol **Coba Lagi** |
| Tanpa hak akses | "Akses ruang kerja ditolak" — tanpa satu pun data pasien |
| Baca saja | "Episode telah ditutup" beserta pengecualian koreksi lewat addendum |
| Gagal per sumber | Alergi, lokasi, dan diagnosis punya kalimat kegagalan sendiri beserta tombol muat ulang masing-masing; kegagalan satu sumber tidak menghapus sumber lain yang berhasil |
| Kewenangan tertahan | Tujuh sebab dibedakan mesin lewat `data-authority-block`: `DENIED`, `CONTEXT_FAILURE`, `CONTEXT_PENDING`, `EPISODE_CLOSED`, `NO_DOCTOR_IDENTITY`, `AUTHORITY_FAILURE`, `NOT_ATTENDING` |
| Permintaan berganti | Berpindah episode membatalkan permintaan lama; respons yang tidak cocok ditolak dan tidak pernah mengisi ruang kerja baru |

---

## 5. Endpoint yang dikonsumsi

Seluruhnya **hanya membaca**. Task ini tidak mengirim satu pun permintaan tulis.

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Identitas pasien, nomor episode, status, kunjungan jangkar, waktu masuk, DPJP aktif | `InpatientEpisode : Read` |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments` | Menentukan DPJP yang berlaku — dasar kewenangan menulis | `InpatientEpisode : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-occupancies/placements/by-episode/{episodeId}` | Lokasi rawat terkini: unit layanan, kamar, tempat tidur | `InpatientBedOccupancy : Read` |

#### Health Services / Clinical Management / Patient Allergy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-allergies/active-alerts?patientId=` | Alergi aktif pasien pada kepala ruang kerja | `PatientAllergy : Read` |

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-diagnoses?encounterId=&diagnosisStatus=1` | Diagnosis kerja pada kunjungan jangkar episode | `PatientDiagnosis : Read` |

Pemilihan diagnosis kerja mengikuti urutan `WorkingDiagnosis` → diagnosis utama → baris pertama yang masih berlaku; layar tidak pernah menyusun diagnosis yang tidak dikirim server. Bobot alergi dan kategori alergen diterjemahkan dari angka enum backend (`PatientAllergySeverity`, `PatientAllergyCategory`) menjadi kalimat Indonesia; nilai di luar daftar disebut "belum diketahui", bukan diturunkan menjadi "ringan".

---

## 6. Verifikasi

Seluruh angka berasal dari eksekusi nyata pada 7 September 2026.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran perintah kosong |
| `npm run test:unit` | `tests 458`, `pass 458`, `fail 0` — termasuk 16 test baru `FE-RWI-043` | `PASS` | Ringkasan runner Node |
| `npm run build` beserta `postbuild` | Kompilasi produksi berhasil, standalone runtime siap | `PASS` | Keluaran build |
| `tests/e2e/inpatient-physician-workspace.spec.mjs` (Edge, server standalone port 3710, API tiruan) | **13 skenario lulus** | `PASS` | Reporter `list` Playwright |
| `tests/e2e/inpatient-physician-entry.spec.mjs` (regresi `FE-RWI-042`) | **9 skenario lulus** | `PASS` | Dijalankan dalam rangkaian yang sama: `22 passed` |
| Grep anti-regresi UI pada berkas yang diubah | Warna literal 0; typography literal 0; `<table>` mentah 0; utility Bootstrap 0; `!important` baru 0; blok `prefers-color-scheme` 0; inline style statis 0 | `PASS` | Perintah §G `ui-consistency-checklist.md` |

Rincian ketiga belas skenario `FE-RWI-043`: konteks keselamatan lengkap sebelum enam tab; navigasi tab lewat klik dan panah papan ketik; kegagalan alergi berbeda dari alergi kosong; konteks gagal menutup jalur tulis dan menyembunyikan tab; kewenangan gagal dibaca tidak dianggap berwenang; dokter selain DPJP ditutup beserta nama DPJP; DPJP aktif memperoleh kewenangan; episode tertutup menjadi baca saja; berpindah episode tidak membawa pasien lama; lokasi gagal tidak terbaca sebagai tanpa tempat tidur; serta tiga viewport desktop, tablet, dan mobile.

**Uji manual:** `PASS` — dikerjakan lewat peramban Edge yang dikemudikan skrip terhadap hasil build produksi. Kontrol interaktif yang diverifikasi: perpindahan tab dengan klik, panah kiri/kanan, dan `Home`; tombol **Coba Lagi** konteks; tombol **Coba Lagi** alergi; tombol **Muat Ulang Lokasi**; tombol **Muat Ulang Kewenangan**; serta tautan **Kembali ke Daftar Pasien** dan **Buka Detail Episode**.

**Temuan yang ditemukan uji dan sudah diperbaiki.** Skenario "kewenangan gagal dibaca" mula-mula **gagal**: penugasan DPJP yang dijawab `403` membuat layar tetap menyatakan pengguna berwenang, karena penilai kewenangan hanya memeriksa kegagalan jaringan dan diam-diam jatuh kembali ke identitas DPJP pada payload episode. Ini persis bahaya yang dilarang rules §1.1 butir 7. Perbaikannya: penolakan izin diperlakukan sama dengan kegagalan baca — keduanya menutup jalur tulis dengan kalimatnya sendiri. Skenario diulang setelah build ulang dan lulus.

**Catatan cara menjalankan e2e.** Repository tidak memiliki `playwright.config.*` dan binary browser bawaan tidak cocok versi, jadi dipakai config sementara di dalam repository dengan `channel: "msedge"`, lalu **dihapus setelah dijalankan**. Server yang diuji adalah `.next/standalone/server.js` dengan API dipalsukan lewat `page.route`, sehingga tidak menyentuh backend maupun database tim.

**Temuan grep yang dipertahankan:** `ClinicalTabNav.jsx` memakai elemen `<button>` mentah. Itu markup milik base component navigasi tab yang memang harus menghasilkan elemen ber-`role="tab"`; aturan "tanpa tombol mentah" berlaku untuk view fitur, dan tidak ada satu pun `<button>` mentah pada view maupun komponen domain task ini.

**Tidak dijalankan:** `npm run lint` versi penuh beserta warning-nya tidak dijalankan terpisah karena `lint:errors` sudah menutup gerbang error; `npm run test:e2e` polos tidak dipakai karena akan menyapu `tests/unit/*.test.mjs`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kesepuluh elemen safety context selalu tampil sebelum dokumentasi; data gagal tidak diganti nilai normal palsu | Terpenuhi | `inpatient-episode-header.jsx` menyusun kesepuluhnya; setiap sumber yang gagal menampilkan kalimat kegagalannya sendiri, bukan tanda hubung. Skenario peramban memeriksa kesepuluh elemen sekaligus, ditambah skenario lokasi gagal |
| 2. Context loading/gagal/mismatch menonaktifkan seluruh write termasuk autosave, modal, dan addendum, dengan retry | Terpenuhi | `resolveWorkspaceWriteAccess` menutup tulisan pada keadaan memuat, gagal, tidak cocok, dan ditolak; seluruh area tab berada di dalam satu `ClinicalActionGuard` ber-`<fieldset disabled>`, sehingga kontrol tulis apa pun yang dipasang task berikutnya ikut mati. Tombol **Coba Lagi** memuat ulang seluruh sumber |
| 3. Allergy error menonjol dan berbeda dari alergi kosong | Terpenuhi | Tiga keadaan alergi terpisah beserta `data-testid` masing-masing; skenario peramban membandingkan keduanya berurutan pada satu sesi |
| 4. Authority tidak sah menonaktifkan area tulis dan menjelaskan yang berwenang; aksi tanpa permission tetap hidden | Terpenuhi | Tujuh sebab penutupan beserta kalimatnya; kalimat "DPJP aktif: …" tampil pada penanda kewenangan dan pada penjaga area tulis. `ClinicalActionGuard` menyediakan `mode="hide"` untuk aksi yang tidak dimiliki, dipakai task aksi berikutnya |
| 5. Nol aksi/layanan antrean | Terpenuhi | Test source-level memindai 16 berkas jalur ruang kerja: nol `useDoctorQueue`, `useDoctorQueueBoard`, `useInfiniteQueueScroll`, `queueCode`, `doctor-queue`, dan nol aksi Panggil/Lewati/Tidak Hadir. Skenario peramban memeriksa seluruh URL yang diminta halaman |
| 6. Tepat enam tab pada satu pasien/episode; Closed read-only dengan addendum sah, tanpa global finalize | Terpenuhi | Enam tab dikunci konstanta dan diperiksa test; hanya satu panel dirender; skenario episode Closed menampilkan banner baca saja beserta pengecualian koreksi; test source-level membuktikan nol "Simpan Konsultasi"/"Finalize Consultation" |
| Visual: urutan composition sama dengan skema §6/§20 | Terpenuhi | Test urutan komposisi memeriksa posisi `ClinicalPageHeader` → `InpatientEpisodeHeader` → `ClinicalTabNav`, dan di dalam kepala episode `ClinicalContextBar` → `ClinicalSafetyAlert` → `PhysicianAuthorityIndicator` |
| Visual: safety context terlihat pada tiap tab dan saat scroll | Terpenuhi | Konteks berada di luar panel tab dan menempel di atas pada layar lebar; skenario peramban membandingkan posisi konteks terhadap daftar tab |
| Visual: alergi gagal/tercatat/kosong berbeda | Terpenuhi | Tiga bentuk peringatan yang berbeda tone dan kalimat |
| Visual: authority dan Closed terbaca | Terpenuhi | Penanda kewenangan dan banner "Episode telah ditutup" |
| Visual: tab aktif dan kontennya terhubung | Terpenuhi | `aria-controls` pada tab dan `aria-labelledby` pada panel; skenario peramban mencocokkan id keduanya |
| Visual: tiga viewport memenuhi §1.4 | Terpenuhi | Tiga skenario viewport lulus; pada mobile ringkasan menetap membawa nama, No. RM, episode, dan keadaan alergi; tidak ada gulir horizontal pada ketiganya |
| Gerbang §4.1 butir 1, 2, 3, 4, 5, 18, 19, 20 | Terpenuhi untuk lingkup task ini | Butir 1 dan 2 lewat pemindaian dan skenario satu-episode; butir 3, 4, 5 lewat konteks menetap, tiga keadaan alergi, dan penjaga tulis; butir 18 lewat pembedaan kosong/gagal/ditolak; butir 19 lewat ketiadaan finalisasi global; butir 20 lewat tiga viewport |
| DoD: adapter/base/domain terpisah; laporan memuat bukti context failure, allergy error, authority, Closed, responsive, dan nol queue | Terpenuhi | Bagian 3, 4, dan 6 laporan ini |

**Yang belum dan sengaja tidak diklaim:** isi dokumentasi keenam tab. `FE-RWI-043` menyediakan shell, konteks, dan penjagaannya; kajian medis, catatan perkembangan, catatan terpadu, visite, resep & tindakan, serta penunjang dikerjakan `FE-RWI-044` s.d. `FE-RWI-049` beserta dependency backend masing-masing.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sepanjang pengujian ditemukan satu cacat keselamatan pada penilai kewenangan (penolakan `403` sempat terbaca sebagai berwenang) — sudah diperbaiki dan diuji ulang; rinciannya pada bagian 6 |
| Masalah yang diketahui | Hari rawat dihitung di layar dari `admittedAt` memakai aturan yang sama dengan census backend (selisih tanggal, minimal satu hari) karena endpoint episode tidak mengirimkan angkanya. Bila kelak backend menyediakan `lengthOfStayDays` pada detail episode, angka server yang dipakai. `PatientAllergy : Read` dan `PatientDiagnosis : Read` adalah dua hak akses tambahan yang harus dimiliki peran dokter; keduanya sudah ada di backend dan tidak dibuat baru oleh task ini |
| Dependency backend | `BE-RWI-044` ✅ selesai. Task ini tidak menunggu endpoint baru; seluruh sumber konteks sudah tersedia pada SHA rujukan |
| Perubahan sampingan | Entry ruang kerja sementara milik `FE-RWI-042` beserta hook-nya dihapus karena digantikan shell penuh, dan empat kelas CSS yatim yang ditinggalkannya ikut dibersihkan. Empat assertion pada test `FE-RWI-042` disesuaikan dengan kalimat baru — cakupan ujinya tidak dikurangi. Artefak sementara e2e (config Playwright dan folder `test-results/`) sudah dihapus |
| Interupsi | `NONE` |
| Status Git | Frontend `HamzahV2` di atas commit `30db3734a`: 24 berkas baru, 8 berkas diubah, 2 berkas dihapus — **belum di-commit**; commit dan push dilakukan pemilik pekerjaan sendiri. Backend `MHamzah`: hanya laporan ini, roadmap, dan traceability sub-modul ini |
| Langkah berikutnya | `FE-RWI-044` (kajian medis awal, memerlukan `BE-RWI-045`) dan `FE-RWI-045` (catatan perkembangan, memerlukan `BE-RWI-046` dan `BE-RWI-047`); keduanya dapat berjalan paralel di atas shell ini |
