# Laporan Perubahan Frontend — `FE-IGD-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-045` |
| Judul | Ruang Kerja Dokter IGD: daftar pasien, saringan *Pasien saya*, kartu pasien, kerangka tab |
| Slice | R3.14 slice D1 · `SCR-IGD-D01` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14 |
| Trace | `FR-IGD-096`; `AT-IGD-200`; DoD PRD §10.4 butir 1; `IGD-DEC-220`, `221`, `222`, `223`; `IGD-DEC-230` pilihan desain 8; `03-frontend-architecture.md` §15.2–§15.7 |
| Contract version | API **`0.15.0`** §10.2 (`doctorId`, `ongoing`, `activeDoctorId`, `activeDoctorName`); validation **`0.14.0`** §12.1; permission **`0.8.0`** §9.1 baris 1 — `approved` (`IGD-DEC-230`) |
| Wewenang UI | `DEV_DISCRETION` `03` §15.8 (nama route dan folder, label ringkasan, susunan detail); letak menu ditetapkan pemilik `IGD-DEC-232` (*Dokter → IGD*). Mengikat: susunan layar dokter rawat inap; kalimat kosong `03` §15.4 tabel A |
| Dependency | `BE-IGD-065` 🟡 — source selesai 7 Oktober 2026; build pemilik dan uji API belum. Tanpa build itu, saringan `doctorId`/`ongoing` diabaikan server lama dan ruas DPJP kosong |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2 (> 20), berkas diubah 1 (12 berkas), logika bisnis 1, kontrak API 1 (memakai kontrak `BE-IGD-065`), database 0, keamanan/auth 0, UI/workflow 1 (satu halaman baru) |
| Task mode | `FRONTEND` — izin pemilik 7 Oktober 2026 (*"iyaa lanjut kerjakan"*, pasangan `BE-IGD-065`). Backend baca-saja; laporan dan tautan bukti di repository backend (wewenang sempit `AGENTS.md`) |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): berkas pada bagian 3.2 |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `6c66327aa` (`RizkiV2`, sejajar origin) + perubahan pemilik pada `emergency-assessment-observation-tab.jsx` (bukan task ini, tidak disentuh) |
| Commit backend yang dijadikan rujukan | `30ea0a3a` (`rizkiG`) + working tree `BE-IGD-065` (3 berkas) |
| Tanggal | 7 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 7 Oktober 2026: implementasi selesai; 3 dari 10 acceptance terbukti** (8 lewat diff, 9, 10). Sebelas berkas baru + `menu-items.jsx` (+7); `eslint` 0 error 0 warning; `npm run build` lulus 09.39 WIB (472/472 halaman, 0 warning). **Belum:** uji layar acceptance 1–7 pada putaran 1, sesudah build backend `BE-IGD-065`. Delta: butir menu ditambahkan di kode (`IGD-UNK-14` terjawab source). Tanpa UAT |

---

## 1. Keadaan yang ditemukan di awal

- Tidak ada layar dokter IGD. Layar pemeriksaan IGD (`emergency-assessment`) adalah layar perawat (`IGD-DEC-220`).
- Layar dokter rawat inap (`doctor-inpatient-view.jsx`) tersusun dari primitive bersama `@/components/ui/doctor-clinical-base`
  (`ClinicalPageHeader`, `ClinicalSummaryBar`, `ClinicalTabNav`, `ClinicalPatientCard`, `ClinicalContextBar`,
  `ClinicalEmptyState`) — pustaka yang juga dipakai Hemodialisa dan layar perawat IGD. Kartu pasien dan header konteksnya
  sangat terikat episode rawat inap: kamar, bed, lama rawat, peran penugasan, penjamin bawaan *BPJS Kesehatan*.
- Data layar dokter rawat inap lewat hook + service, bukan Redux (`use-inpatient-physician-patients.jsx`,
  `use-inpatient-physician-workspace.jsx`).
- `GET /emergency-visits` di frontend hanya dipakai lewat Redux slice layar perawat dan service triage; belum ada pemakai
  saringan `doctorId`/`ongoing` (keduanya baru ada di source `BE-IGD-065`).
- **Menu sidebar didefinisikan di kode** (`src/utils/menu-sidebar/menu-items.jsx`), termasuk grup *Instalasi Gawat Darurat*
  dan *Dokter → Rawat Inap*. Ini menjawab `IGD-UNK-14` dari sisi source: butir menu tidak dapat didaftarkan admin tanpa
  perubahan kode.
- Pencarian backend (`search`) tidak mencakup nama pasien dan No. RM — hanya nomor kunjungan, keluhan, lokasi, alias, dan
  catatan (`EmergencyVisitController.GetAll` `:93`–`:104`).

---

## 2. Proses bisnis dari sisi pengguna

| Butir | Isi |
| --- | --- |
| Pengguna | Dokter IGD (akun dengan `doctorId` pada `auth/me`) |
| Kapan dibuka | Saat dokter jaga mulai memeriksa pasien IGD — dari menu *Dokter → IGD* (`IGD-DEC-232`), atau tautan berisi `?visitId=` |
| Prasyarat | Hak `EmergencyVisit : Read`; DPJP ditetapkan di layar triage (`BE-IGD-045`) |

**Langkah.**

1. Dokter membuka layar. Daftar kiri memuat kunjungan IGD berjalan dengan saringan bawaan **Pasien saya**
   (`GET /emergency-visits?ongoing=true&doctorId=<doctorId pengguna>`). Ringkasan di kanan atas: *Pasien IGD berjalan N* dan
   *Pasien saya M*.
2. Dokter mengetik nama, No. RM, atau nomor kunjungan di kotak cari; daftar tersaring di layar.
3. Dokter mengganti saringan ke **Semua pasien IGD**; pasien lain dan pasien tanpa DPJP ikut tampil, masing-masing dengan
   *DPJP: dr. …* atau *DPJP: Belum ada DPJP*.
4. Dokter memilih satu pasien. URL berubah menjadi `?visitId=<id>`; panel kanan memuat kartu konteks: DPJP, nomor kunjungan,
   unit, status, waktu tiba, No. RM, nama, cara datang, jenis kasus, alergi, dan diagnosis kerja atau *Belum ada diagnosis*.
5. Tab kerja (Pengkajian Medis, Catatan Dokter, Resep, Tindakan, Penunjang, Tindak Lanjut) dipasang kartu `FE-IGD-046`…`054`;
   sampai kartu itu dibangun, panel menampilkan keterangan bahwa tab dipasang bertahap.

**Jalur tidak normal.**

| Keadaan | Yang dilihat dokter |
| --- | --- |
| *Pasien saya* kosong | *"Belum ada pasien IGD dengan Anda sebagai DPJP."* — *"Ganti saringan ke Semua pasien IGD untuk melihat pasien lain."* |
| Akun tanpa identitas dokter | Segmen *Pasien saya* tidak ditawarkan; keterangan *"Saringan Pasien saya nonaktif karena akun ini belum terhubung ke data dokter. Daftar menampilkan semua pasien IGD."*; daftar *Semua* tetap tampil |
| Daftar gagal dimuat | Pesan server apa adanya dan tombol *Coba Lagi* |
| Tanpa hak `EmergencyVisit : Read` (`403`) | *"Akses daftar pasien IGD tidak tersedia"* |
| Kunjungan dari `?visitId=` gagal dimuat | Pesan server dan tombol *Coba Lagi*; `403` → *"Akses kunjungan IGD tidak tersedia"* |
| Kunjungan dari `?visitId=` sudah berakhir | Pita *"Kunjungan IGD ini sudah berakhir — data hanya untuk dibaca."* |
| Alergi atau diagnosis gagal dimuat | *"Alergi gagal dimuat"* / *"Diagnosis gagal dimuat"* — tidak pernah dibaca sebagai bebas alergi atau tanpa diagnosis |
| Angka ringkasan gagal | Tampil *"—"*, daftar tetap jalan |

**Hasil akhir.** Dokter melihat pasiennya sendiri lebih dulu tanpa kehilangan akses ke pasien IGD lain; tidak ada data yang
ditulis oleh layar ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `base-component-decision-gate.md`,
  `base-component-catalog.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`
- Layar dokter rawat inap: `src/app/health-services/inpatient-management/doctor-inpatient/page.jsx`,
  `doctor-inpatient-client.jsx`, `doctor-inpatient-view.jsx`, `inpatient-physician-patient-card.jsx`,
  `inpatient-physician-context-header.jsx`, `inpatient-physician-patient-list.jsx`, `inpatient-physician-patient-filters.jsx`,
  `physician-workspace-context.jsx`, `use-inpatient-physician-patients.jsx`, `use-inpatient-physician-workspace.jsx`,
  `inpatient-physician-workspace-utils.js`, `doctor-inpatient-workspace.module.css`
- Primitive: `src/components/ui/doctor-clinical-base/` (`ClinicalSegmentedNav`, `ClinicalPatientCard`, `ClinicalContextBar`,
  `ClinicalSummaryBar`, `ClinicalEmptyState`, `ClinicalTabNav`), `base-button.jsx`, `data-filter.jsx`
- Data IGD: `emergency-assessment-slice.jsx`, `emergency-management-triage.service.js`,
  `emergency-registration.constants.js` (`EMERGENCY_VISIT_STATUS`, `_LABELS`), `emergency-management-triage-utils.jsx`
  (`formatDateTimeId`), `patient-allergy.service.js`, `patient-diagnosis.service.js`, `login-slice.jsx` (`doctorId`)
- `src/utils/menu-sidebar/menu-items.jsx`
- Backend: `EmergencyVisitController.GetAll`/`GetById`, `EmergencyVisitDtos.cs` (`BE-IGD-065`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/emergency-installation-management/doctor-emergency/page.jsx` (baru) | Route tipis, metadata *Dokter - IGD* — pola `doctor-inpatient/page.jsx` |
| `src/components/view/health-services/emergency-installation-management/doctor-emergency/doctor-emergency-client.jsx` (baru) | Pembungkus client |
| `…/doctor-emergency/doctor-emergency-view.jsx` (baru) | Komposisi layar: header + ringkasan, panel kiri (saringan, cari, daftar, keadaan), panel kanan (konteks, pita kunjungan berakhir, kerangka tab). `PhysicianWorkspaceProvider` diisi konteks kunjungan IGD — titik sambung tab `FE-IGD-046`…`054`. Peta tab kosong (`TAB_COMPONENTS`); tab yang belum dibangun tidak ditampilkan |
| `…/doctor-emergency/emergency-physician-patient-card.jsx` (baru) | Pembungkus tipis `ClinicalPatientCard`: waktu tiba, nama, No. RM, status kunjungan, nomor kunjungan, DPJP |
| `…/doctor-emergency/emergency-physician-context-header.jsx` (baru) | Pembungkus tipis `ClinicalContextBar` + pita alergi/diagnosis (kelas yang sama dengan rawat inap) |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-physician-patients.jsx` (baru) | Saringan (`mine`/`all`), dua permintaan paralel — daftar dengan saringan aktif (`pageSize` 100) dan hitungan saringan lainnya (`pageSize` 1) — pencarian di layar, keadaan muat/gagal/`403`, `AbortController` |
| `…/emergency-physician/use-emergency-physician-workspace.jsx` (baru) | Konteks satu kunjungan: `GET /emergency-visits/{id}`, alergi aktif pasien, diagnosis aktif encounter; DPJP aktif, `isActiveDoctor`, `isVisitEnded`; tiga sumber dibaca terpisah (pola rawat inap) |
| `src/lib/services/health-services/emergency-management/emergency-physician.service.js` (baru) | `getEmergencyPhysicianVisits`, `getEmergencyPhysicianVisitDetail` lewat `InstanceAxios` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-physician-constant.jsx` (baru) | Route, ukuran halaman, saringan, enam tab, nada status, seluruh teks layar |
| `src/utils/health-services/emergency-installation-management/emergency-physician-utils.js` (baru) | Fungsi murni: normalisasi baris/halaman/detail, parameter daftar, pencarian, nada status, kunjungan berakhir, perbandingan dokter, pesan galat |
| `src/style/health-services/emergency-installation-management/doctor-emergency-workspace.module.css` (baru) | Empat kelas kecil (keterangan saringan, teks konteks, jarak tombol di keadaan kosong) — seluruhnya token |
| `src/utils/menu-sidebar/menu-items.jsx` (+7) | Butir *IGD* di grup *Dokter*, sejajar *Rawat Jalan* dan *Rawat Inap* — arahan pemilik 7 Oktober 2026 (`IGD-DEC-232`), menggantikan letak `03` §15.3. *Sebelumnya pada hari yang sama:* butir *Ruang Kerja Dokter* di grup *Instalasi Gawat Darurat*, dipindah atas arahan itu |

Seluruh berkas CRLF tanpa BOM (konvensi berkas sekitar), diperiksa per byte. Nol baris komentar. Nol `globals.css`, nol
Redux slice, nol perubahan `store.jsx`, nol berkas rawat inap disunting, nol backend.

### 3.3 Kepatuhan arsitektur frontend

- Alur `app → view → hooks → services → InstanceAxios`; normalisasi di `utils`; teks dan konfigurasi di `constants`.
- Pola data mengikuti layar dokter rawat inap: hook + service, bukan Redux (data hanya dipakai satu layar).
- **Dipakai ulang tanpa perubahan:** primitive `doctor-clinical-base`, `BaseButton`, `PhysicianWorkspaceProvider`, util murni
  rawat inap untuk alergi/diagnosis (`normalizeAllergyAlerts`, `normalizeDiagnoses`, `selectWorkingDiagnosis`,
  `formatDiagnosisText`, `DIAGNOSIS_STATUS_ACTIVE`), `formatDateTimeId`, `EMERGENCY_VISIT_STATUS_LABELS`, dan CSS module
  `doctor-inpatient-workspace.module.css` (tata letak dua panel).

#### Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 6, EXTEND 0, COMPOSE 1, WRAP 2, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman + ringkasan | `ClinicalPageHeader`, `ClinicalSummaryBar` | `doctor-clinical-base`; dipakai `doctor-inpatient-view.jsx` | REUSE | Judul *Dokter - IGD*; ringkasan *Pasien IGD berjalan* + *Pasien saya* |
| Saringan *Pasien saya* / *Semua* | `ClinicalSegmentedNav` | `doctor-clinical-base/ClinicalSegmentedNav.jsx` (`segments`, `activeSegment`, `onSegmentChange`) | REUSE | Dua segmen; tanpa identitas dokter hanya *Semua* + keterangan |
| Kotak cari panel kiri | `DataFilter` (catatan katalog: *"jangan membuat input search sendiri"*) | `doctor-inpatient-view.jsx` `:235`–`:244` memakai input di kelas `filterStrip` | COMPOSE | Opsi A (lihat bawah) |
| Kartu pasien di daftar | `ClinicalPatientCard` | `doctor-clinical-base`; pembungkus rawat inap terikat episode | WRAP | `emergency-physician-patient-card.jsx` |
| Header konteks pasien | `ClinicalContextBar` | idem; header rawat inap terikat kamar/bed/penjamin | WRAP | `emergency-physician-context-header.jsx` |
| Navigasi tab | `ClinicalTabNav` | dipakai rawat inap | REUSE | Enam tab `IGD-DEC-221`, `223`; tampil hanya tab yang sudah dibangun |
| Keadaan muat/kosong/gagal | `ClinicalEmptyState`, `BaseButton` | dipakai rawat inap | REUSE | Teks di constants |
| Pita kunjungan berakhir | kelas `closedEpisodeBanner` | `doctor-inpatient-workspace.module.css` | REUSE | Teks IGD |
| Tata letak dua panel | `doctor-inpatient-workspace.module.css` | dipakai `doctor-inpatient-view.jsx` | REUSE | Opsi A (lihat bawah) |

**Keputusan: kotak cari panel kiri**

- **A. Pola panel kiri layar dokter rawat inap — input di kelas `filterStrip` — Rekomendasi (dijalankan).** Brief pemilik
  (*"mengikuti Ruang Kerja Dokter Rawat Inap"*) berada di atas konvensi umum katalog pada hierarki wewenang UI. Tampilan
  identik dengan layar dokter rawat inap; pencarian di layar sehingga tidak butuh debounce. Konsekuensi: satu input mentah
  seperti rawat inap.
- **B. `DataFilter` di panel kiri.** Patuh katalog dan membawa debounce, tetapi dirancang untuk area lebar; tampilannya
  berbeda dari layar dokter rawat inap.

**Keputusan: kartu pasien dan header konteks**

- **A. Pembungkus tipis IGD di atas primitive yang sama — Rekomendasi (dijalankan).** Komponen rawat inap tidak disentuh
  (nol risiko regresi rawat inap); visual sama karena primitive dan kelasnya sama.
- **B. Adapter di komponen rawat inap** (prop sumber data). Mencampur kunjungan IGD ke berkas rawat inap dan menambah
  risiko regresi pada layar yang sedang dipakai.

**Keputusan: gaya tata letak**

- **A. Pakai ulang `doctor-inpatient-workspace.module.css` — Rekomendasi (dijalankan).** Satu sumber tata letak untuk kedua
  layar dokter; perubahan tampilan rawat inap ikut terbawa, sesuai brief.
- **B. Salin kelas ke CSS module IGD.** Lepas dari rawat inap, tetapi menduplikasi ±500 baris CSS lama yang berisi nilai
  literal.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Memuat Pasien IGD..."* pada daftar; *"Memuat daftar pasien IGD..."* pada hitungan; *"Memeriksa alergi..."* |
| Kosong | *Pasien saya*: *"Belum ada pasien IGD dengan Anda sebagai DPJP."* + *"Ganti saringan ke Semua pasien IGD untuk melihat pasien lain."*; *Semua*: *"Belum ada pasien IGD yang kunjungannya berjalan."*; pencarian tanpa hasil + *Atur Ulang Pencarian*; belum memilih pasien |
| Gagal | Pesan server apa adanya + *Coba Lagi* (daftar dan kunjungan); angka ringkasan *"—"*; alergi/diagnosis gagal ditandai |
| Tanpa hak akses | *"Akses daftar pasien IGD tidak tersedia"*; *"Akses kunjungan IGD tidak tersedia"* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits?ongoing=true[&doctorId=]&pageNumber=1&pageSize=100&sortBy=arrivalDateTime&sortDirection=desc` | Daftar kiri dan angka ringkasan (saringan lainnya dengan `pageSize=1`) | `EmergencyVisit : Read` |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Konteks pasien terpilih | `EmergencyVisit : Read` |

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-diagnoses?encounterId=&diagnosisStatus=1` | Diagnosis kerja pada kartu konteks | `PatientDiagnosis : Read` |

Alergi aktif dibaca lewat `patientAllergyService.getActivePatientAllergyAlerts(patientId)` — service yang sama dengan layar
dokter rawat inap.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <11 berkas task .js/.jsx>` | Pertama: 0 error, 1 warning (`aria-selected` pada `role="button"`); prop dibuang karena `ClinicalPatientCard` sudah mengisinya dari `active`. Kedua: **0 error, 0 warning** | `PASS` | Keluaran perintah |
| Grep anti-regresi checklist UI (G.1–G.6, inline style) | Nol warna literal, nol `!important`, nol tombol non-base, nol `<table>`, nol utility typography Bootstrap, nol inline style; typography hanya pada kelas baru sendiri memakai token | `PASS` | Keluaran perintah |
| Akhiran baris (hitungan byte lewat Node) | 11 berkas baru CRLF, 0 LF; `menu-items.jsx` 2195 CRLF, 0 LF | `PASS` | Keluaran perintah |
| Pemindahan menu ke *Dokter → IGD* (`IGD-DEC-232`) sesudah build | `npx eslint src/utils/menu-sidebar/menu-items.jsx` 0 error 0 warning; `menu-items.jsx` 2195 CRLF, 0 LF. Build 09.39 dibuat **sebelum** pemindahan; build ulang dijalankan bersama `FE-IGD-046` | `PASS` (lint); build ulang 10.15 WIB bersama `FE-IGD-046` lulus (472/472) | Keluaran perintah; [laporan FE-IGD-046](FE-IGD-046.md) bagian 6 |
| `npm run build` | `npm run build` mulai 09.37.45, selesai 09.39.32 WIB, exit 0: *Compiled successfully in 61s*, *Generating static pages … (472/472)*, nol baris error/warning di log; route `○ /health-services/emergency-installation-management/doctor-emergency` terbentuk; `postbuild` *Standalone runtime siap dijalankan* | `PASS` | Log build |
| Uji layar `AT-IGD-200` dan acceptance 1–8 | Belum — putaran uji 1 sesudah build backend `BE-IGD-065` | `NOT RUN` | — |

Uji manual: `REQUIRED` — lewat agen Antigravity pada hasil build, 1440 × 900, akun dokter `ranger.biru@admin.com`.

AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik.

**Tidak dijalankan:** `npm run test:unit` (atas perintah pemilik); uji layar (menunggu build backend dan putaran uji 1).

### 6.1 Skenario uji untuk panduan putaran 1

Layar dilayani **hasil build** (`node .next/standalone/server.js`), backend berjalan dari build yang memuat `BE-IGD-065`.
Data lewat layar dengan akun peran nyata; tanpa SuperAdmin; sandi dari variabel lingkungan.

| No | Langkah | Harapan | Acceptance |
| ---: | --- | --- | --- |
| `045-U1` | Dokter membuka menu *Dokter → IGD* | Daftar memuat kunjungan berjalan dengan *Pasien saya* aktif; rekam jaringan memuat `ongoing=true&doctorId=<doctorId>`; ganti ke *Semua* menampilkan pasien lain dan *Belum ada DPJP* | 1 |
| `045-U2` | Ketik sebagian nama, lalu No. RM, lalu nomor kunjungan | Daftar tersaring sesuai | 2 |
| `045-U3` | Dokter tanpa pasien ber-DPJP dirinya | Kalimat kosong persis | 3 |
| `045-U4` | Akun tanpa `doctorId` (atau simulasi) | Hanya segmen *Semua* + keterangan; daftar tampil | 4 |
| `045-U5` | Pilih pasien; buka ulang dengan `?visitId=` | Kartu konteks lengkap; `?visitId=` langsung memilih pasien | 5 |
| `045-U6` | Bandingkan angka ringkasan dengan `totalData` | Sama | 6 |
| `045-U7` | Periksa seluruh teks layar | Nol GUID, nol tanggal `0001` | 7 |
| `045-R1` | Dokter rawat inap membuka layar dokter rawat inap | Perilaku tidak berubah | 8 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar kunjungan berjalan, *Pasien saya* bawaan, nama DPJP atau *belum ada DPJP*; ganti ke *Semua* menampilkan pasien lain | **Terpetakan, belum diuji** | `use-emergency-physician-patients.jsx` (saringan bawaan `mine`, `ongoing: true`, `doctorId`); kartu `DPJP: … / Belum ada DPJP` |
| 2 | Pencarian nama, No. RM, nomor kunjungan menyaring daftar | **Terpetakan, belum diuji** | `filterEmergencyPhysicianVisits` (di layar — `search` backend tidak mencakup nama/No. RM) |
| 3 | *Pasien saya* kosong menampilkan kalimat `03` §15.4 tabel A persis | **Terpetakan, belum diuji** | Kalimat dipecah menjadi judul + deskripsi `ClinicalEmptyState`; isi kata persis |
| 4 | Akun tanpa `doctorId`: *Pasien saya* nonaktif dengan keterangan; *Semua* tetap tampil | **Terpetakan, belum diuji — dengan catatan** | Segmen *Pasien saya* tidak ditawarkan (bukan tombol abu-abu: `ClinicalSegmentedNav` tidak punya status nonaktif per segmen) + keterangan |
| 5 | Kartu: identitas, unit, status, waktu tiba, DPJP, alergi, diagnosis kerja / *Belum ada diagnosis*; `?visitId=` memilih pasien | **Terpetakan, belum diuji** | `emergency-physician-context-header.jsx`; `doctor-emergency-view.jsx` (`visitId` dari URL, detail dibaca langsung walau tidak ada di daftar aktif) |
| 6 | Angka ringkasan sama dengan `totalCount` API | **Terpetakan, belum diuji** | `totals` dari `totalData` kedua permintaan |
| 7 | Nol GUID dan tanggal `0001`; `403` tampil sebagai pesan | **Terpetakan, belum diuji** | Hanya nama, nomor, label; `formatDateTimeId`; keadaan `denied` |
| 8 | Regresi: layar dokter rawat inap tidak berubah | **Terpenuhi lewat diff** | Nol berkas rawat inap disunting; hanya diimpor (`PhysicianWorkspaceProvider`, CSS module, util murni) |
| 9 | Diff: nol backend, `globals.css`; nol komentar; akhiran baris dipertahankan | **Terpenuhi** | Bagian 3.2 dan 6 |
| 10 | `eslint` 0 error; `npm run build` lulus | **Terpenuhi** — `eslint` 0/0; build lulus 09.39 WIB | Bagian 6 |

**Definition of Done:** laporan tracked ✅; register, node grafik R3.14.2, dan traceability ditandai; tanpa UAT PASS ✅.
**Belum:** uji layar acceptance 1–7 pada putaran 1 sesudah build backend `BE-IGD-065`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning ESLint pertama sudah dibereskan (bagian 6) |
| Masalah yang diketahui | (1) **Delta menu:** kartu menulis *"butir menu didaftarkan admin, bukan oleh kode"*, tetapi source membuktikan menu ada di `menu-items.jsx`; butir ditambahkan di kode supaya DoD butir 1 dapat dicapai — menjawab `IGD-UNK-14`. Letaknya *Dokter → IGD* atas arahan pemilik (`IGD-DEC-232`). (2) Daftar memuat paling banyak 100 kunjungan berjalan per saringan (batas `NormalizePaging` backend); pencarian berjalan atas 100 baris itu — teks *"x dari N pasien"* memperlihatkan bila N lebih besar. (3) Acceptance 4 diwujudkan dengan menyembunyikan segmen, bukan menonaktifkannya. (4) Teks keadaan *"Tab kerja dokter belum tersedia"* hanya terlihat sampai `FE-IGD-046`…`054` dibangun |
| Dependency backend | `BE-IGD-065` 🟡 — tanpa build-nya, server lama mengabaikan `doctorId`/`ongoing` (daftar memuat semua kunjungan, termasuk yang selesai) dan ruas DPJP kosong. Uji dan rilis bersama |
| Perubahan sampingan | `NONE`. Perubahan pemilik pada `emergency-assessment-observation-tab.jsx` sudah ada sebelum task ini dan tidak disentuh |
| Interupsi | Server Next.js pemilik di port 3000 (PID 7656) dihentikan atas izin pemilik 7 Oktober 2026 untuk `npm run build`; **tidak** dinyalakan ulang |
| Langkah berikutnya | (1) Build backend `BE-IGD-065` oleh Rizki. (2) `build-module-backend` `BE-IGD-066` → `build-module-frontend` `FE-IGD-046` (langkah 2 R3.16.4), lalu langkah 3–4. (3) Panduan uji putaran 1 sesudah langkah 1–4 |

`git status --short` di akhir pekerjaan (frontend):

```text
 M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/emergency-installation-management/doctor-emergency/
?? src/components/view/health-services/emergency-installation-management/doctor-emergency/
?? src/lib/constants/health-services/emergency-installation-management/emergency-physician-constant.jsx
?? src/lib/hooks/health-services/emergency-installation-management/emergency-physician/
?? src/lib/services/health-services/emergency-management/emergency-physician.service.js
?? src/style/health-services/emergency-installation-management/doctor-emergency-workspace.module.css
?? src/utils/health-services/emergency-installation-management/emergency-physician-utils.js
```
