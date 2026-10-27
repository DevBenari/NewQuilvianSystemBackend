# Laporan Perubahan Frontend — `FE-IGD-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-052` |
| Judul | Tab Penunjang di layar dokter IGD: laboratorium, radiologi, bank darah, hemodialisa |
| Slice | R3.14 slice D2 · `SCR-IGD-D01` tab Penunjang |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14, kartu `FE-IGD-052` |
| Trace | `FR-IGD-101` (empat jenis); `AT-IGD-208`; `IGD-DEC-220`, `224`, `228`; [arsitektur frontend](../../../03-frontend-architecture.md) §15.8 |
| Contract version | API §10.7, §10.8 dan state §10.2 — bagian yang dipakai berstatus "sudah ada / dipakai apa adanya"; nol kontrak baru |
| Wewenang UI | `DEV_DISCRETION` §15.8. Yang mengikat: satu jenis gagal dimuat tidak menyembunyikan jenis lain; penolakan modul tampil apa adanya |
| Dependency | `FE-IGD-045` ✅ (21 Oktober 2026). Nol backend baru |
| Klasifikasi | `MEDIUM` — 3 berkas baru, 3 berkas disunting, nol komponen bersama diubah |
| Task mode | `FRONTEND` — backend dibaca sebagai source of truth; dokumen modul ditulis atas perintah pemilik |
| Target tulis | `QuilvianSystemFrontendDev/src/**` (enam berkas di bawah); laporan dan register di `docs/module-blueprints/igd/` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `RizkiV2` `376a6b512` + working tree enam berkas, **belum di-commit**; build terakhir `bERnTlIXGmLZ3y3pW2Cfg` |
| Commit backend yang dijadikan rujukan | `rizkiG` `c396bd7d` |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI — 26 Oktober 2026 (petang), 5 dari 5 acceptance terbukti.** Sebelumnya pada hari yang sama 🟡 (acceptance 1–2 tertahan izin akun dokter, `IGD-FACT-239`); izin diberikan agen atas perintah pemilik (`IGD-DEC-352`), `npm run build` dijalankan dua kali dan lulus, uji layar diulang pada hasil build — bagian 9 |

---

## 1. Keadaan yang ditemukan di awal

1. **Tab Penunjang sudah terdaftar tetapi tidak pernah tampil.** `EMERGENCY_PHYSICIAN_WORKSPACE_TABS`
   memuat `{ key: "supporting", label: "Penunjang" }`, tetapi `doctor-emergency-view.jsx` tidak
   punya entri `supporting` pada `TAB_COMPONENTS`, sehingga tab itu tersaring keluar.
2. **Hook rawat inap tidak dapat dipakai langsung.** `use-inpatient-supporting-service.jsx`
   menghentikan seluruh pembacaannya bila `episodeId` rawat inap tidak sah (`:165`, `:201`, `:243`,
   `:311`), membaca lab dan radiologi per episode, dan mengirim pesanan darah lewat endpoint per
   episode rawat inap. Kunjungan IGD tidak punya episode.
3. **Panel perawat IGD (`FE-IGD-056`) tidak cocok untuk dokter.** Panelnya terikat
   `emergency-assessment-slice` dan kartu formulir pengkajian, mewajibkan dokter pemberi
   instruksi, dan panel radiologinya memakai modalitas cadangan ber-UUID karangan
   (`emergency-assessment-radiology-panel.jsx:110`–`:115`).
4. **Preseden adapter sudah ada.** Tab Penunjang dokter rawat jalan
   (`doctor-supporting-exam-tab.jsx` + `use-doctor-supporting-service.js`) memakai ulang seksi dan
   formulir rawat inap lewat hook per encounter dan provider context-nya sendiri.
5. **Hemodialisa dapat dibaca per kunjungan** — `HmdOrderPagedQuery.EncounterId`
   (`HmdOrderDtos.cs:10`) diterapkan `HmdOrderService.cs:99`–`:100`. Catatan 19 Oktober 2026 bahwa
   hemodialisa belum punya pembacaan per `encounterId` keliru (`IGD-FACT-240`).
6. **Status verifikasi pesanan dokter** (acceptance 2) ditentukan server, bukan layar: lab tanpa
   `inpEpisodeId` selalu `NotRequired` (`LabOrderService.cs:727`–`:728`); bank darah `NotRequired`
   selama `InstructionVerificationStatus` tidak dikirim (`BbkBloodOrderService.cs:748`–`:758`);
   radiologi mengikuti pola lab.
7. **Unit pemesan darah wajib dikirim** (`CreateBloodOrderRequest.ServiceUnitId`, `VAL-BD-013`),
   dan unit tak berizin dijawab **`403` berpesan** (`BbkBloodOrderController.cs:379`–`:381`).
   Normalizer kunjungan dokter membuang `serviceUnitId` walau `EmergencyVisitResponse` memuatnya.
8. **Cacat komponen bersama rawat inap** — `SupportingHistorySection` mengirim `state=` yang tidak
   dikenal `ClinicalStateBoundary` dan `data=` padahal `ClinicalDataTable` membaca `rows=`; kolom
   riwayatnya ditulis `render(row)` sedangkan tabel memanggil `render(value, row, index)`
   (`IGD-FACT-242`). Komponen itu tidak dipakai IGD dan **tidak diubah**.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter IGD (DPJP atau dokter lain yang berwenang atas pasien). Perawat dan peran lain
dapat membaca tab ini tanpa tombol pesan.

1. Dokter membuka **Dokter → IGD**, memilih pasien, lalu menekan tab **Penunjang**.
2. Tab menampilkan empat segmen: **Laboratorium**, **Radiologi**, **Bank Darah**, **Hemodialisa**.
   Angka di segmen adalah jumlah pesanan kunjungan itu. **Gizi tidak ada** — menyusul `FE-IGD-053`.
3. Segmen terbuka pada sub-tab **Riwayat & Hasil Pemeriksaan**: daftar pesanan kunjungan itu
   beserta status dan, untuk lab dan radiologi, kefinalan hasil serta tombol **Lihat Detail Hasil**.
4. Untuk memesan, dokter menekan **+ Pesan Lab** / **+ Pesan Radiologi** / **+ Permintaan Darah** /
   **+ Pesan Hemodialisa**, atau sub-tab **Formulir Pemesanan**. Formulirnya formulir rawat inap
   apa adanya; dokter peminta diisi dari akun yang sedang masuk, unit pemesan darah dari unit
   kunjungan IGD.
5. Sesudah terkirim, notifikasi **Pesanan Berhasil Dicatat** tampil dan layar kembali ke riwayat
   yang dimuat ulang.

**Contoh.** dr. Jenah Ali memesan satu kantong PRC untuk pasien IGD. Layar mengirim
`POST /blood-orders` dengan `encounterId` kunjungan itu, `serviceUnitId` Instalasi Gawat Darurat,
dan `requestingDoctorId` dr. Jenah Ali. Bila unit IGD belum diizinkan memesan darah, kotak merah
**Pesanan Penunjang Gagal** menampilkan kalimat Bank Darah persis seperti dijawab server.

**Jalur tidak normal**

| Keadaan | Yang terjadi |
| --- | --- |
| Satu jenis gagal dimuat | Hanya seksi jenis itu menampilkan kegagalan beserta **Coba Lagi**; tiga jenis lain tetap terbaca (uji `U3`) |
| Akun tidak berizin membaca satu modul | Seksi jenis itu menampilkan **Akses … ditolak**; jenis lain tidak terpengaruh (uji `U1`) |
| Modul menolak pesanan | Kalimat penolakan server tampil apa adanya di kotak **Pesanan Penunjang Gagal** (uji `U4`) |
| Akun tanpa identitas dokter atau kunjungan sudah berakhir | Tombol pesan disembunyikan; penjaga **Pemesanan penunjang ditahan** menyebut alasannya; daftar tetap terbaca (uji `U2`) |
| Pindah pasien | Segmen kembali ke Laboratorium/riwayat; pesan sukses/gagal pasien sebelumnya dibuang; jawaban permintaan pasien lama diabaikan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Frontend: `doctor-emergency-view.jsx`, `emergency-physician-workspace-context.jsx`,
  `use-emergency-physician-workspace.jsx`, `emergency-physician-utils.js`,
  `emergency-physician-constant.jsx`; seluruh `inpatient-management/physician-workspace/tabs/supporting-service/*`;
  `use-inpatient-supporting-service.jsx`; `doctor-supporting-exam-tab.jsx`,
  `use-doctor-supporting-service.js`; `emergency-assessment-*-panel.jsx`,
  `emergency-assessment-diagnostic-support-tab.jsx`, `emergency-assessment-slice.jsx`;
  service lab, lab-catalog, radiologi, bank darah, hemodialisa; `ClinicalStateBoundary`,
  `ClinicalDataTable`, `ClinicalTabNav`, `ClinicalSegmentedNav`, `ClinicalActionGuard`.
- Backend: `LabOrderDtos.cs`, `LabOrderService.cs`, `RadiologyDtos.cs`, `RadOrderService.cs`,
  `BloodOrderDtos.cs`, `BbkBloodOrderService.cs`, `BbkBloodOrderController.cs`, `HmdOrderDtos.cs`,
  `HmdOrderService.cs`, `HemodialysisPermissions.cs`, `HemodialysisEnums.cs`,
  `EmergencyVisitDtos.cs`, `EmergencyVisitController.cs`, controller lab catalog, rad study,
  procedure, blood component.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-supporting-tab.jsx` | **Baru.** Tab Penunjang dokter IGD: header, empat segmen, penjaga tulis, sub-tab formulir/riwayat, notifikasi, modal detail hasil. Merangkai seksi dan formulir rawat inap tanpa mengubahnya; provider bertingkat mengisi `placement` dari unit IGD untuk formulir hemodialisa |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-physician/use-emergency-physician-supporting-service.jsx` | **Baru.** Adapter dengan bentuk keluaran sama seperti `useInpatientSupportingService`: empat daftar per `encounterId` yang dimuat mandiri, katalog lab/radiologi hanya bila boleh menulis, empat aksi pesan. Pesanan darah membawa `serviceUnitId`, `requestingDoctorId`, `idempotencyKey`, dan jalur konfirmasi duplikat `VAL-BD-001` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-physician-supporting-constant.jsx` | **Baru.** Empat jenis (diambil dari `SUPPORTING_SERVICES` rawat inap), ikon, ukuran halaman, dan seluruh teks IGD |
| `src/components/view/health-services/emergency-installation-management/doctor-emergency/doctor-emergency-view.jsx` | +2 baris: impor tab dan entri `supporting` pada `TAB_COMPONENTS` |
| `src/lib/services/health-services/emergency-management/emergency-physician.service.js` | +18 baris: dua pembaca katalog radiologi (`master-data/procedures?isRadiology=true`, `rad-studies/modalities`) supaya hook tidak memanggil `InstanceAxios` dengan URL mentah |
| `src/utils/health-services/emergency-installation-management/emergency-physician-utils.js` | +12 baris: `serviceUnitId` diteruskan dari `EmergencyVisitResponse`; `normalizeEmergencyHemodialysisOrders` memetakan `requestedAt` → `orderDate` dan `orderStatus`/`orderStatusName` → `status`/`statusText`, karena daftar HD tidak memakai nama ruas yang dibaca `buildHmdOrderColumns` (bagian 9, `IGD-FACT-244`) |

`git diff --stat`: 3 berkas, +32 baris, nol penghapusan; ditambah 3 berkas baru. **Nol berkas
`inpatient-management` berubah.**

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya `view → hook → service → InstanceAxios`; teks di `lib/constants`, normalisasi
di `utils`. Pola mengikuti preseden rawat jalan (`use-doctor-supporting-service.js`) dengan dua
perbaikan: katalog lewat service, dan pembacaan dibatalkan dengan `AbortController`.

**Tabel keputusan base component**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header tab | `DoctorSupportingHeader`, `ClinicalStatusBadge` | `components/ui/doctor-clinical-base`, dipakai tab rawat inap dan rawat jalan | REUSE | Apa adanya |
| Navigasi empat jenis | `ClinicalSegmentedNav` | idem | REUSE | Apa adanya |
| Penjaga tulis | `ClinicalActionGuard` | idem | REUSE | `writeAccess` IGD |
| Notifikasi sukses/gagal | `ClinicalSafetyAlert` | idem | REUSE | Apa adanya |
| Daftar lab, radiologi | `SupportingOrderSection` + `buildLabOrderColumns`/`buildRadOrderColumns` | `supporting-order-section.jsx` | REUSE | Teks kosong IGD lewat props |
| Daftar hemodialisa | `SupportingOrderSection` + `buildHmdOrderColumns` | `supporting-hemodialysis-section.jsx` mengekspor kolomnya | COMPOSE | Seksi bawaan HD terkunci teks "rawat inap" |
| Daftar bank darah | `SupportingOrderSection` + `buildSupportingHistoryColumns("blood-bank")` | Kolom diadaptasi `render(value,row) → column.render(row)` | COMPOSE | `SupportingHistorySection` cacat (`IGD-FACT-242`) |
| Formulir lab, radiologi, darah | `SupportingLaboratoryForm`, `SupportingRadiologyForm`, `SupportingBloodBankForm` | Dipakai rawat inap dan rawat jalan | REUSE | Apa adanya |
| Formulir hemodialisa | `SupportingHemodialysisForm` | Satu-satunya UI pemesan HD di repository | WRAP | Provider bertingkat mengisi `placement` |
| Sub-tab formulir/riwayat | Kelas `subTabBar` | `physician-supporting-service.module.css`, dipakai rawat jalan | REUSE | Nol CSS baru |
| Modal detail hasil | `SupportingResultDetailModal` | Dipakai rawat inap dan rawat jalan | REUSE | Apa adanya |

```text
UI GATE: 11 elemen — REUSE 8, EXTEND 0, COMPOSE 2, WRAP 1, NEW 0
```

**Keputusan: formulir hemodialisa** (diambil sebagai `DEV_DISCRETION`, karena tidak ada `NEW`)

- **A. Pakai `SupportingHemodialysisForm` apa adanya, `placement` diisi unit IGD lewat provider bertingkat — dijalankan.**
  Nol berkas rawat inap berubah. Kolom tempat bernilai "Instalasi Gawat Darurat (Bed -)", bukan
  "Rawat Inap". Sisa utang teks: label "Ruang Bangsal & Bed" dan "Catatan Pengantar Bangsal /
  Instruksi Khusus" tetap berbunyi rawat inap, dan bila kolom catatan itu diisi, alasan klinis
  yang tersimpan diberi akhiran `[Instruksi Bangsal: …]`.
- **B. Formulir hemodialisa khusus IGD dari base form field.** Teks benar, tetapi ±150 baris
  menyalin logika yang sama dan akan menyimpang dari formulir rawat inap. Perlu persetujuan karena
  `NEW`.
- **C. Tambah props label pada formulir rawat inap.** Perilaku bawaan tetap, tetapi menyentuh
  komponen bersama yang dilarang diubah task ini (acceptance 4).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu kerangka per seksi: *"Membaca pesanan laboratorium..."*, *"...radiologi..."*, *"Membaca permintaan darah..."*, *"Membaca pesanan hemodialisa..."* |
| Kosong | *"Belum ada pesanan laboratorium pada kunjungan IGD ini."* (dan padanannya per jenis) + *"Pesanan yang dibuat pada kunjungan lain tidak ikut ditampilkan di sini."* |
| Gagal | Judul gagal per jenis + pesan server apa adanya + tombol **Coba Lagi** — `U3-laboratory-seksi.png` |
| Tanpa hak akses | **Akses … ditolak** — *"Akun ini tidak memiliki izin membaca pesanan … pada perawatan ini."* per jenis — `U1-*-seksi.png` |
| Penolakan saat memesan | Kotak **Pesanan Penunjang Gagal** berisi kalimat server apa adanya — `U4-hemodialisa-ditolak-pesan.png` |

---

## 5. Endpoint yang dikonsumsi

Base path `/api/v1/health-services`.

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/laboratory-management/lab-orders?encounterId=&pageNumber=1&pageSize=50` | Daftar lab kunjungan | `LabOrder : Read` |
| `POST` | `/laboratory-management/lab-orders` | Pesan lab — `{ encounterId, procedureId, inpEpisodeId: null }` | `LabOrder : Create` |

#### Health Services / Laboratory Management / Lab Catalog

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/laboratory-management/lab-catalog/examinations?pageNumber=1&pageSize=100` | Katalog formulir lab | `LabCatalog : Read` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/radiology-management/rad-orders?encounterId=` | Daftar radiologi kunjungan | `RadOrder : Read` |
| `POST` | `/radiology-management/rad-orders` | Pesan radiologi — `{ encounterId, procedureId, modalityId, clinicalIndication, isUrgent, inpEpisodeId: null }` | `RadOrder : Create` |

#### Health Services / Radiology Management / Rad Study

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/radiology-management/rad-studies/modalities` | Pilihan modalitas | `RadStudy : Read` |

#### Health Services / Master Data / Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/master-data/procedures?isRadiology=true&isActive=true` | Katalog radiologi | `Procedure : Read` |

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/blood-bank-management/blood-orders?encounterId=&pageNumber=1&pageSize=50` | Daftar permintaan darah kunjungan | `BloodOrder : Read` |
| `POST` | `/blood-bank-management/blood-orders` | Permintaan darah — `{ patientId, encounterId, serviceUnitId, requestingDoctorId, requestedBloodGroup, clinicalNote, lines, idempotencyKey }`; tanpa `instructionVerificationStatus` | `BloodOrder : Create` |
| `POST` | `/blood-bank-management/blood-orders/confirm-duplicate` | Lanjutkan pesanan yang ditahan `VAL-BD-001`, beralasan | `BloodOrder : Create` |

#### Health Services / Master Data / Blood Component

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/master-data/blood-components/options?onlyActive=true` | Pilihan komponen (dibaca formulir rawat inap) | `BloodComponent : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/hemodialysis-management/hemodialysis-orders?encounterId=&pageNumber=1&pageSize=50` | Daftar hemodialisa kunjungan | `HemodialysisOrder : Read` |
| `POST` | `/hemodialysis-management/hemodialysis-orders` | Pesan hemodialisa — `{ patientId, encounterId, inpEpisodeId: null, requestingDoctorId, priority, clinicalReason, requestedDate }` | `HemodialysisOrder : Create` |

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/emergency-installation-management/emergency-visits/{id}` | Sumber `serviceUnitId` unit pemesan darah (sudah dipanggil ruang kerja) | `EmergencyVisit : Read` |

---

## 6. Verifikasi

Bukti di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fe-igd-052-20261026/` (di-gitignore).
Layar dilayani `npm run dev` milik pemilik di port 3000; backend `https://localhost:7184`; Playwright
`msedge` headless. **Setiap verdict dihitung dari nilai DOM atau jaringan yang diukur dan menjadi
konjungsi wajib verdict** (`skrip/u-layar.mjs`, fungsi `check`); `hasil.json` dirakit
`skrip/rakit-hasil.mjs` dari berkas hasil, bukan ditulis tangan. PNG diperiksa langsung.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src` (HEAD bersih, sebelum perubahan) | 0 error, 1096 warning, 5100 berkas | baseline | keluaran perintah |
| `npx eslint src` (sesudah perubahan) | 0 error, 1096 warning, 5103 berkas; **nol berkas berubah jumlah temuannya** | `PASS` | perbandingan per berkas, `hasil.json` acceptance 5 |
| Komentar pada diff dan berkas baru | 0 baris | `PASS` | `grep` diff dan berkas baru |
| Akhiran baris | CRLF pada keenam berkas, sama dengan blob `HEAD` | `PASS` | hitungan CR per berkas |
| `globals.css` | Tidak tersentuh | `PASS` | `git status` |
| `U1` — dokter `ranger.biru@admin.com`, kunjungan `IGD-261009030511-26F549` | 9/9 pemeriksaan: tab tampil, empat segmen tanpa gizi, setiap pembacaan disaring `encounterId`, keempat jenis `403` di jaringan **dan** `data-clinical-state="denied"` di layar | `PASS` | `hasil-U1.json`, `jaringan-U1.json`, `U1-*-seksi.png` |
| `U2` — akun perawat (baca saja), kunjungan yang sama | 13/13: keempat jenis `200`; baris tabel = item respons (lab 3, radiologi 1, darah 1, hemodialisa 0 → keadaan kosong); tombol pesan tersembunyi | `PASS` | `hasil-U2.json`, `U2-laboratory-seksi.png`, `U2-blood-bank-seksi.png` |
| `U3` — `U2` dengan daftar lab dipaksa `500` lewat `page.route` | 9/9: lab `error` memuat pesan simulasi apa adanya; radiologi, darah, hemodialisa tetap tampil sesuai responsnya | `PASS` | `hasil-U3.json`, `U3-laboratory-seksi.png` |
| `U4` — dokter mengirim formulir hemodialisa | 9/9: `POST` `403`; kotak gagal memuat *"Anda tidak memiliki akses ke menu atau fitur ini."* persis; muatan `encounterId` kunjungan, `inpEpisodeId: null`, `requestingDoctorId` = dokter yang masuk. **Nol data tercipta** | `PASS` | `hasil-U4.json`, `U4-hemodialisa-ditolak.png` |
| Sondean izin akun dokter (`POST {}` dan `GET` katalog) | `POST` lab/radiologi/darah/hemodialisa `403`; katalog lab, modalitas, komponen darah `403`; master prosedur `200` | `EXISTING / ENVIRONMENT ISSUE` | `02-izin-tulis-kosong.json` |
| `npm run build` ke-1 (15.15–15.17, atas perintah pemilik; `next dev` pemilik dihentikan atas izinnya) | `✓ Compiled successfully in 70s`, 517/517 halaman, standalone siap, BUILD_ID `BN80fEoLJYzg30R7f1Kkh` | `PASS` | `build-1-npm-run-build.log` |
| `npm run build` ke-2 (15.28–15.30, sesudah perbaikan HD) | `✓ Compiled successfully in 62s`, 517/517 halaman, standalone siap, BUILD_ID `bERnTlIXGmLZ3y3pW2Cfg`, nol baris error | `PASS` | `build-2-npm-run-build.log` |
| `npx eslint src` sesudah perbaikan HD | 0 error, 1096 warning — identik baseline, nol berkas berubah jumlah temuannya; nol komentar baru; CRLF di keenam berkas | `PASS` | `hasil.json` acceptance 5 |
| `U5` — dokter memesan empat jenis dari layar (build ke-1) | 20/22: keempat `POST` sukses (lab `201`, radiologi `201`, darah `200`, HD `201`), keempatnya ada di daftar kunjungan, baris layar +1 per jenis, muatan tanpa `inpEpisodeId`. **Dua pemeriksaan gagal karena rancangan pemeriksaannya**: item daftar `lab-orders`/`rad-orders` tidak memuat `instructionVerificationStatus`, sehingga diukur ulang lewat `U5b` | `FAIL` (dicatat apa adanya) | `hasil-U5.json`, `jaringan-U5.json`, `U5-*-sesudah-seksi.png` |
| `U5b` — detail tiga pesanan dokter dari `createdId` `U5` | 3/3: `LAB-RSMMC-000004` `NotRequired`, `RAD-RSMMC-000002` `NotRequired`, `ORD-00000002` `0` (= `NotRequired`); `instructingDoctorId` kosong | `PASS` | `hasil-U5b.json` |
| `U6` — dokter berizin membaca empat jenis (build ke-2) | 15/15: keempat jenis `200`, baris = item (lab 4, radiologi 2, darah 2, HD 1), tombol pesan tampil; sel status HD *Diminta* = label `orderStatus` 1, waktu *26 Okt 2026, 15.26 WIB* | `PASS` | `hasil-U6.json`, `U6-hemodialisa-status-seksi.png` |
| `U2`, `U3` diulang pada build ke-2 | 13/13 dan 9/9 | `PASS` | `hasil-U2.json`, `hasil-U3.json` |

`U1` dan `U4` dijalankan **sebelum** izin diberikan dan pada `next dev`; keduanya tidak dapat diulang
dalam bentuk yang sama karena dokter kini berizin. Pemilik menyatakan layar `npm run dev` sah selama
modul masih dikembangkan (19 Oktober 2026).

Uji manual: `PASS`.

**Tidak dijalankan:** unit test (aturan pemilik); uji layar tab Penunjang dokter rawat inap
(acceptance 4 dibuktikan lewat penalaran diff); rilis hasil radiologi oleh radiolog (`IGD-UNK-15`) —
bukan bagian acceptance, kolom hasil tampil *Hasil belum final*.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Satu pesanan per jenis (lab, radiologi, darah, hemodialisa) tercatat di modulnya dan statusnya terbaca di tab | **Terpenuhi** | `U5` (empat `POST` sukses, ada di daftar, baris +1), `U6` (status keempat jenis terbaca, termasuk HD *Diminta*) |
| 2 | Pesanan dokter tidak berstatus menunggu verifikasi instruksi | **Terpenuhi** | `U5b` — lab, radiologi, darah `NotRequired`; modul HD tidak mengenal verifikasi instruksi |
| 3 | Penolakan modul tampil apa adanya; jenis lain tetap tampil bila satu jenis gagal dimuat | **Terpenuhi** | `U1`, `U3`, `U4` |
| 4 | Regresi: tab Penunjang layar dokter rawat inap tidak berubah | **Terpenuhi (penalaran diff)** | Nol berkas `inpatient-management` di `git status`; `use-inpatient-supporting-service.jsx`, `supporting-service-tab.jsx`, dan seluruh komponen seksinya identik `HEAD` |
| 5 | Diff, komentar, akhiran baris, `globals.css`; `eslint` 0 error; `npm run build` lulus | **Terpenuhi** | Bagian 6 — dua build lulus, ESLint identik baseline |

DoD: acceptance 1–5 ✅; laporan ✅; register, kartu, grafik, dan traceability diperbarui ✅.
*Riwayat:* pada pass pertama hari yang sama acceptance 1–2 dinyatakan tidak dapat diuji dan acceptance
5 sebagian; catatan itu dipertahankan di roadmap dan decision log (`IGD-FACT-239`).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Akun perawat uji `dimas.kurniawan@rsmmc.local` berperan **SuperAdmin** (`IGD-FACT-241`); `U2`/`U3` sah untuk ketahanan dan pembacaan, bukan untuk izin peran perawat |
| Masalah yang diketahui | (1) Utang teks formulir hemodialisa (bagian 3.3 pilihan A). (2) `SupportingHistorySection` rawat inap cacat — daftar Bank Darah dan Gizi dokter rawat inap diperkirakan selalu kosong dan tidak pernah menampilkan penolakan, **dari pembacaan source, belum diuji layar** (`IGD-FACT-242`); milik Rawat Inap. (3) Kolom HD rawat inap membaca ruas yang tidak dikirim daftar HD — di layar rawat inap status HD akan tampil *Tidak Diketahui* (`IGD-FACT-244`); IGD menormalkan sendiri. (4) Kolom *Indikasi Klinis* HD tampil `-` karena daftar HD tidak memuat `clinicalReason` (batas DTO backend). (5) Tombol pesan tampil bagi dokter yang tidak berizin `Create`; server menolaknya dengan pesan apa adanya (`U4`) |
| Dependency backend | Nol. Konfigurasi izin dokter diberikan (`IGD-DEC-352`); `IGD-UNK-13` terjawab — unit IGD `4dd4827b…` diterima Bank Darah (`IGD-FACT-245`); `IGD-UNK-15` belum diuji (bukan acceptance) |
| Perubahan sampingan | **Data dev** (basis data milik pemilik): 11 policy role-access ditambahkan ke departemen `676f2aa7…` / posisi `cd1cd442…` (222 → 233, nol hilang; cadangan `10-policy-dokter-sebelum.json`); empat pesanan uji pada kunjungan `IGD-261009030511-26F549` — `LAB-RSMMC-000004`, `RAD-RSMMC-000002`, `ORD-00000002`, `HD-ORD-00000001` |
| Interupsi | `next dev` pemilik (PID 10340/26592) dihentikan atas izin pemilik untuk `npm run build`; server standalone hasil build dipakai untuk uji lalu dihentikan; `npm run dev` **belum dinyalakan ulang** |
| Status Git | ` M .../doctor-emergency/doctor-emergency-view.jsx`, ` M .../emergency-management/emergency-physician.service.js`, ` M .../emergency-installation-management/emergency-physician-utils.js`, `?? .../tabs/emergency-doctor-supporting-tab.jsx`, `?? .../emergency-physician-supporting-constant.jsx`, `?? .../use-emergency-physician-supporting-service.jsx` — belum di-commit |
| Langkah berikutnya | Pemilik meninjau dan meng-commit enam berkas frontend dan dokumen backend; nyalakan kembali `npm run dev` |

---

## 9. Pengerjaan lanjutan — 26 Oktober 2026 (petang), atas perintah pemilik

Pemilik memerintahkan agen: *login memakai SuperAdmin lalu berikan akses kepada dokter IGD, jalankan
`npm run build`, baru ulangi uji layar*, dan mengizinkan `next dev` dihentikan saat build.

1. **Izin dokter (`IGD-DEC-352`).** `POST /administrator/setting/role-access/policies` memakai
   `overwriteTarget: true` — seluruh policy lama departemen+posisi dihapus lalu hanya yang dikirim
   dipasang kembali (`RoleAccessController.ApplyPoliciesAsync`). Karena itu skrip
   `10-beri-izin-dokter.mjs` membaca 222 policy lama, menyimpan cadangannya, mengirim gabungan
   222 + 11, lalu memverifikasi: 233 sesudahnya, nol policy lama hilang (`10-beri-izin-terapkan.json`).
   Login admin memakai akun `dimas.kurniawan@rsmmc.local` yang berperan `SuperAdmin` — satu-satunya
   kredensial SuperAdmin yang tersedia lewat environment; jejak audit perubahan atas nama akun itu.
   Sondean ulang: keempat `POST {}` kini `400`/`404` validasi (bukan `403`), ketiga katalog `200`.
2. **Build.** `next dev` pemilik dihentikan; `npm run build` lulus (bagian 6); hasil build dilayani
   `node .next/standalone/server.js` di port 3000 (`HOSTNAME=localhost` dari PowerShell).
3. **Cacat yang ditemukan uji pada hasil build.** `U5` memperlihatkan baris HD berstatus
   *Tidak Diketahui* dengan waktu `-`: kolom `buildHmdOrderColumns` membaca `status`/`orderDate`,
   sedangkan daftar HD mengirim `orderStatus`/`orderStatusName`/`requestedAt` (`IGD-FACT-244`).
   Diperbaiki di utils IGD, build ulang, dibuktikan `U6`.
4. **Satu pemeriksaan yang salah rancang dicatat, tidak dihapus.** `U5` menuntut status verifikasi
   ada di item daftar lab/radiologi; DTO daftar keduanya memang tidak memuat ruas itu. Verdict `U5`
   tetap `FAIL` di berkasnya; acceptance 2 dinilai dari `U5b` (endpoint detail).
