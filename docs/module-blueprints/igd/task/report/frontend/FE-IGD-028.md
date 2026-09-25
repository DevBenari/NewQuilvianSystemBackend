# Laporan Perubahan Frontend — `FE-IGD-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-028` |
| Judul | Pemantauan observasi dengan tanda vital tertaut |
| Slice | `IGD-S04` · `EPIC IGD-09` (pemantauan observasi) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.7, kartu `FE-IGD-028` |
| Trace | `IGD-DEC-122`, `IGD-DEC-123`, `IGD-DEC-124`, `IGD-DEC-125`, `IGD-DEC-126`, `IGD-DEC-056`; api-contract bagian 7 (termasuk 7.2 dan 7.3); validation-matrix bagian 9; `03-frontend-architecture.md` bagian 12; bukti `IGD-EV-112`, `IGD-EV-117` |
| Contract version | API `0.6.0` dan validation `0.6.0` (16 September 2026, status berkas `draft`). Aturan yang dipakai `approved` lewat `IGD-DEC-122`…`126` (Rizki Gunawan, 16 September 2026) |
| Wewenang UI | Bentuk pemilihan jalur tanda vital, susunan kolom riwayat, dan bentuk tampilan tanda vital = `DEV_DISCRETION` (`03-frontend-architecture.md` bagian 12.5). Isi data, sumber datanya, dan sembilan aturan layar bagian 12.4 **bukan** `DEV_DISCRETION` |
| Dependency | `BE-IGD-046` — implementasi selesai, build bersih 16 September 2026, **runtime belum diverifikasi**; API/validation `0.6.0` tersedia |
| Klasifikasi | `MEDIUM` — satu tab existing diperluas, satu slice bertambah satu bagian daftar, satu util payload baru, satu berkas test baru. Nol route baru, nol komponen bersama diubah, nol CSS |
| Task mode | `FRONTEND` — backend `NewQuilvianSystemBackend` (`rizkiG`) hanya dibaca sebagai bukti kontrak |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): tab Observasi, slice pengkajian, hook detail, view detail, constants, util payload, satu test unit. Ditambah laporan ini beserta baris status roadmap dan traceability pada repository backend |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `2d95904ed` (branch `RizkiV2`), working tree bersih saat task dimulai |
| Commit backend yang dijadikan rujukan | `27351517` (branch `rizkiG`) **ditambah** perubahan `BE-IGD-046` yang masih di working tree pemilik dan belum di-commit |
| Tanggal | 16 September 2026 |
| Status | **Implementation complete.** **Build = Verified** — `npm run build` dijalankan pemilik 16 September 2026 dan berhasil tanpa error, termasuk `postbuild`. **Runtime = Verified (sebagian)** — diuji pemilik lewat layar 16 September 2026 pada periode `OBS-260916061355-B87DFC`, berhasil tanpa galat. Dua jalur belum dilalui: memilih tanda vital yang sudah tercatat (kriteria 3) dan ringkasan ABCDE yang terisi (kriteria 8 — yang tampil adalah keadaan kosongnya). Bukan UAT |

**Gerbang base component** — `UI GATE: 8 elemen — REUSE 7, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Pemilih jalur tanda vital | `BaseSelectField` | `base-form-control.jsx`; pola `{ value, label }` sudah dipakai `OBSERVATION_STATUS_OPTIONS` pada tab ini | REUSE | Tiga pilihan pada satu dropdown |
| Formulir tanda vital baru | `VitalSignTab` | `features/health-services/nurse-station-management/VitalSignTab.jsx`, dipakai apa adanya oleh tab Assesmen Awal IGD dan antrean perawat | REUSE | Dipakai utuh, nol field disalin |
| Validasi tanda vital wajib | `useScreeningValidation` | `hooks/.../useScreeningValidation.js` beserta `validateVitalBeforeFinish` | REUSE | Enam field wajib yang sama dengan layar lain |
| Daftar tanda vital yang sudah tercatat | `BaseSelectField` | idem, memakai `options`, `loading`, dan `emptyText` bawaannya | REUSE | Daftar ber-scope dari backend |
| Pesan non-blocking dan galat | `InformationAlert` | `base-features/information-alert.jsx`, sudah dipakai tab ini pada modal Selesaikan | REUSE | `info`, `warning`, `danger` |
| Kartu dan bagian formulir | `EmergencyAssessmentFormCard`, `EmergencyAssessmentFormSection` | komponen tab yang sudah ada | REUSE | Bagian *Tanda Vital* memakai bentuk yang sama dengan bagian lain |
| Penanda kritis/abnormal | `Badge` (react-bootstrap) | sudah dipakai daftar periode pada tab yang sama | REUSE | `danger` untuk kritis, `warning` untuk abnormal |
| Tampilan angka tanda vital di riwayat dan pratinjau | `styles.recordGrid` (`dl`/`dt`/`dd`) | pola daftar riwayat yang sudah dipakai tab ini | COMPOSE | Opsi A |

Pilihan untuk elemen `COMPOSE`: **A** komponen lokal `VitalSignGrid` yang merangkai `dl.recordGrid`
yang sudah ada, dipakai ulang oleh riwayat dan pratinjau pilihan (**rekomendasi, dipilih** — nol
kelas baru, nol CSS, dan satu-satunya tempat pemetaan angka ke label); **B** menuliskan `dl` yang
sama dua kali langsung di JSX — duplikasi yang pasti berbeda isinya begitu satu sisi diubah;
**C** komponen bersama baru di `features/` — melanggar batas *nol komponen bersama diubah atau
ditambah* pada kartu task.

---

## 1. Keadaan yang ditemukan di awal

Tab **Observasi** sudah memiliki periode observasi, pencatatan pemantauan berkala, riwayat
pemantauan, dan penyelesaian periode beserta kesimpulannya (`FE-IGD-022`, `FE-IGD-024`). Yang
belum ada adalah hubungannya dengan tanda vital:

| Hal | Keadaan sebelum | Bukti |
| --- | --- | --- |
| Penautan tanda vital | **Nol**. Komentar pada komponen menyatakan sendiri: *"penautan itu belum punya jalur di layar — dicatat, bukan ditebak"* | `emergency-assessment-observation-tab.jsx` sebelum perubahan |
| Payload pemantauan | Tidak pernah mengirim `patientVitalSignId` | idem, fungsi `submitDetail` |
| Riwayat pemantauan | Hanya waktu, lima angka cairan, dan empat keterangan teks. Tidak ada angka tanda vital, tidak ada nama pencatat | idem |
| Konteks ABCDE | Tidak ada. Workspace pengkajian **tidak pernah memuat** data triase sama sekali | `use-emergency-assessment-detail.jsx` sebelum perubahan; `EmergencyVisitResponse` tidak memuat field ABCDE |
| Kemampuan yang sebenarnya sudah ada | Thunk `createVitalSign` dan `fetchVitalSigns` sudah ada di slice pengkajian, lengkap dengan penjaga lingkup pasien/encounter — tetapi `createVitalSign` **nol pemakai** | `emergency-assessment-slice.jsx` sebelum perubahan |

Akibatnya bagi perawat: satu putaran pemantauan hanya dapat berisi kalimat. Angka tekanan darah,
nadi, dan GCS harus dicatat di tab lain, lalu dibaca bolak-balik antar-tab setiap 15–30 menit, dan
riwayat pemantauan tidak pernah dapat menunjukkan perkembangan angkanya berurutan.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Perawat IGD yang memantau pasien di ruang observasi.

**Kapan dibuka.** Setiap putaran pemantauan, biasanya tiap 15–30 menit selama periode observasi
berjalan.

### 2.1 Jalur bawaan — mencatat tanda vital baru

1. Perawat membuka tab Observasi. Periode yang sedang berjalan terpilih otomatis.
2. Di bagian **Tanda Vital**, pilihan bawaannya adalah *Catat tanda vital baru*, dan formulir
   tanda vital yang sama persis dengan Assesmen Awal IGD terbuka di bawahnya — tanpa berpindah
   tab.
3. Perawat mengisi suhu, nadi, frekuensi napas, saturasi, dan tekanan darah (kelimanya beserta
   diastolik adalah isian wajib formulir bersama itu), lalu GCS, kesadaran, dan oksigen bila
   diperiksa.
4. Perawat mengisi keadaan klinis, tindakan, respons pasien, cairan, dan catatan seperti biasa.
5. Menekan **Simpan Pemantauan** menjalankan dua permintaan berurutan:
   `POST patient-vital-signs` lebih dulu, lalu `POST emergency-observation-details` yang membawa
   identitas hasil permintaan pertama.
6. Riwayat pemantauan bertambah satu baris lengkap dengan angka tanda vital dan nama pencatat.

### 2.2 Jalur alternatif — memilih tanda vital yang sudah tercatat

1. Perawat mengubah pilihan menjadi *Pilih tanda vital yang sudah tercatat*.
2. Layar meminta daftar tanda vital ke backend **beserta penyaring `patientId` dan `encounterId`
   sekaligus**, lalu menampilkannya sebagai daftar pilihan berisi waktu, tekanan darah, nadi, dan
   saturasi.
3. Setelah satu dipilih, angkanya ditampilkan sebagai pratinjau baca saja supaya perawat yakin
   memilih pengukuran yang benar.
4. Menyimpan hanya mengirim **satu** permintaan: pemantauan beserta `patientVitalSignId` pilihan.

### 2.3 Jalur ketiga — tanpa tanda vital

Pilihan *Tanpa tanda vital* tetap tersedia. Pemantauan naratif seperti sebelum task ini tetap sah
(validation `0.6.0` bagian 9 aturan 11), dan tidak ada isian tanda vital yang dipaksakan.

### 2.4 Kegagalan sebagian — tanda vital tersimpan, pemantauan gagal

Kedua permintaan pada 2.1 **tidak atomik**. Bila yang kedua gagal:

- baris tanda vitalnya **tetap tersimpan** sebagai pengukuran yang sah — dan memang seharusnya
  begitu, karena pengukurannya benar-benar dilakukan;
- identitasnya disimpan layar, formulir tanda vital diganti pesan *"Tanda vital sudah tersimpan
  pada percobaan sebelumnya dan akan dipakai untuk pemantauan ini. Menekan Simpan Pemantauan lagi
  tidak membuat tanda vital kedua."*;
- percobaan berikutnya **menautkan baris yang sama** dan tidak pernah mengirim `POST` tanda vital
  kedua;
- isian pemantauan tidak dihapus, sehingga perawat cukup memperbaiki sebabnya lalu menyimpan lagi.

**Gap UX yang diakui:** identitas itu hanya bertahan selama tab Observasi tidak ditinggalkan.
Bila perawat berpindah tab atau menutup halaman sesudah kegagalan sebagian, tanda vitalnya tetap
tersimpan tetapi tautannya hilang; pemulihannya adalah memilihnya kembali lewat jalur 2.2, karena
baris itu tetap muncul di daftar pilihan. Tidak ada penyimpanan pemulihan yang bertahan lintas
halaman, dan tidak ada transaksi lintas modul yang dibuat di sisi backend.

### 2.5 Periode yang sudah ditutup

Periode `Completed` dan `Cancelled` **tidak** menampilkan formulir *Catat Pemantauan* sama sekali;
yang tampil adalah pemberitahuan *"Periode observasi ini sudah ditutup."* beserta saran membuka
periode baru. Riwayat pemantauannya tetap terbaca. Periode `Active` dan `Escalated` tidak berubah
perilakunya.

Bila periode ditutup orang lain sementara formulir terbuka, backend menjawab `409`. Pesannya
ditampilkan apa adanya pada kartu, dan daftar periode dimuat ulang sehingga layar berhenti
menawarkan pencatatan yang pasti ditolak. Isian tidak dihapus.

### 2.6 Pasien tanpa identitas yang masih provisional

Bila `patientId` atau `encounterId` kunjungan belum ada, penautan tanda vital tidak mungkin
dilakukan: tanda vital milik Clinical Management mewajibkan `PatientId`. Layar **tidak** memaksa,
**tidak** mengirim GUID palsu, dan **tidak** memblokir pencatatan keadaan klinis, tindakan,
respons, maupun keseimbangan cairan. Yang tampil hanyalah pemberitahuan non-blocking bahwa tanda
vital tertaut belum tersedia untuk pasien ini, dan pilihan jalurnya dinonaktifkan.

Ini **tidak** menyelesaikan `RUNTIME / DOMAIN GAP — provisional patient vital sign`; gap itu tetap
milik pemilik domain (`IGD-EV-130`, temuan `J-5`).

### 2.7 ABCDE baca saja

Bagian **Primary Survey Terakhir** menampilkan ringkasan A/B/C/D/E beserta tanda bahaya dari
penilaian triase **terakhir** kunjungan itu, apa adanya sebagai teks. Layar tidak memecah kalimat
apa pun menjadi A/B/C/D/E, tidak menyediakan isian ABCDE, dan tidak membuat penilaian kedua
(`IGD-DEC-123`). Bila belum ada triase, yang tampil adalah kalimat kosong yang jelas; bila
permintaannya gagal, bagian itu disembunyikan dan pencatatan pemantauan tidak tertahan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen blueprint (repository backend):** `MODULE-STATUS.md`; `00-interview-decisions.md`
(`IGD-DEC-122`…`126`, `IGD-DEC-056`); `evidence/2026-09-15-audit-observasi-v1-v2.md`;
`02-backend-architecture.md` bagian 12; `03-frontend-architecture.md` bagian 12;
`contracts/api-contract.md` bagian 7; `contracts/validation-matrix.md` bagian 9;
`roadmap/frontend-roadmap.md` R3.7; `roadmap/backend-roadmap.md` R3.9;
`roadmap/requirement-traceability.md` R3.5; laporan `BE-IGD-046.md`, `FE-IGD-024.md`,
`fe-igd-022-...md`.

**Tata kelola:** `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`,
`base-component-catalog.md`, `base-component-decision-gate.md`, `page-composition-patterns.md`,
`design-tokens.md`, `ui-consistency-checklist.md`, `test-policy.md`, `REPORT_TEMPLATE.md`.

**Source frontend:** `emergency-assessment-observation-tab.jsx`;
`emergency-assessment-initial-tab.jsx`; `emergency-assessment-detail-view.jsx`;
`emergency-assessment-form-card.jsx`; `emergency-assessment-section.jsx`;
`emergency-assessment-slice.jsx`; `use-emergency-assessment-detail.jsx`;
`emergency-assessment-constant.jsx`; `VitalSignTab.jsx`; `useScreeningValidation.js`;
`screening-validation.utils.js`; `nurse-station-queue.constants.js`;
`nurse-screening-options.constants.js`; `patient-assessment-payload.utils.js`;
`use-nurse-station-queue.js` (pembentuk payload tanda vital yang tidak diekspor);
`base-form-control.jsx`; `information-alert.jsx`; `emergency-assessment.module.css`;
`tests/unit/patient-assessment-payload.test.mjs`.

**Source backend (baca saja):** `PatientVitalSignController.cs`; `PatientVitalSignDtos.cs`;
`TrxPatientVitalSign.cs`; `OxygenSupportType.cs`; `ConsciousnessStatus.cs`;
`PatientVitalSignSource.cs`; `PatientPosition.cs`; `EmergencyTriageController.cs`;
`EmergencyTriageDtos.cs`; `EmergencyObservationDetailDtos.cs` (proyeksi `0.6.0`);
`EmergencyObservationDetailController.cs`.

### 3.2 Berkas yang berubah

Seluruh source di repository frontend `QuilvianSystemFrontendDev`.

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx` | Bagian **Tanda Vital** pada *Catat Pemantauan* beserta tiga jalurnya; formulir `VitalSignTab` yang dipakai ulang; daftar pilihan tanda vital ber-scope beserta pratinjaunya; urutan simpan dua permintaan dengan penjaga klik ganda dan pemakaian ulang identitas tanda vital saat kegagalan sebagian; penanganan `409` periode tertutup; bagian **Primary Survey Terakhir** baca saja; riwayat pemantauan menampilkan angka tanda vital, penanda kritis/abnormal, dan nama pencatat; sembilan pembantu tampilan beserta komponen lokal `VitalSignGrid` |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | Thunk `fetchEmergencyTriages` memakai `buildListThunk` yang sudah ada beserta bagian state `triages`; kode status HTTP ikut dibawa pada penolakan `buildCreateThunk` supaya `409` dapat dibedakan dari `400` |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx` | Triase ikut dimuat saat tab Observasi dibuka; bagian `triages` diteruskan ke view |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx` | Dua prop baru untuk tab Observasi: `vitalSignSection` dan `triageSection` |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | `OBSERVATION_VITAL_MODE` dan pilihannya; `OBSERVATION_VITAL_SIGN_SOURCE`; `OBSERVATION_VITAL_SIGN_INVALID_STATUS` |
| `src/utils/health-services/clinical-management/patient-vital-sign-payload.utils.js` | **Baru.** `buildPatientVitalSignPayload` — pembentuk payload `POST patient-vital-signs` dari bentuk formulir skrining bersama, bersebelahan dengan pembentuk payload pengkajian milik `FE-IGD-019` |
| `tests/unit/patient-vital-sign-payload.test.mjs` | **Baru.** Lima test untuk pembentuk payload di atas, mengikuti bentuk `patient-assessment-payload.test.mjs` |

Ukuran diff: `689 / 109` baris pada tab (`595 / 15` bila perubahan indentasi diabaikan — kartu
*Catat Pemantauan* turun satu tingkat karena kini berada di dalam percabangan periode tertutup),
`28 / 0` slice, `44 / 0` constants, `5 / 1` hook, `2 / 0` view.

### 3.3 Kepatuhan arsitektur frontend

- **Nol Axios baru.** Ketiga permintaan memakai thunk slice yang sudah ada: `createVitalSign` dan
  `fetchVitalSigns` yang sebelumnya menganggur, serta `createObservationDetail`. Thunk triase
  memakai `buildListThunk` yang sudah ada, bukan pola baru.
- **Nol endpoint backend baru.** Ketiga endpoint yang dipakai sudah ada sejak sebelum task ini.
- **Nol komponen bersama diubah** dan **nol CSS** — tidak ada berkas `.css` yang disentuh, dan
  seluruh kelas yang dipakai sudah ada pada `emergency-assessment.module.css`.
- **Nol nilai visual literal**: tanpa warna, ukuran font, atau `style={{ }}` pada baris yang
  ditambahkan (bukti pada bagian 6).
- Batas panjang, kode status, dan nilai enum disimpan di constants domain, bukan angka lepas di
  view.
- Pembentuk payload ditaruh di `src/utils/health-services/clinical-management/`, sebelah pembentuk
  payload pengkajian, karena tabel tujuannya memang milik Clinical Management.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat daftar pilihan tanda vital | Pemutar pada dropdown lewat prop `loading` bawaan `BaseSelectField` |
| Memuat konteks ABCDE | Kerangka bagian (`sectionSkeleton`), bukan layar kosong |
| Kosong — belum ada tanda vital | *"Belum ada tanda vital pada kunjungan ini. Catat tanda vital baru."* |
| Kosong — belum ada triase | *"Belum ada penilaian triase pada kunjungan ini."* beserta penjelasan kapan ringkasannya muncul |
| Kosong — belum ada pemantauan | Kalimat lama tidak berubah |
| Kosong — nilai tanda vital tidak diukur | Tanda hubung, bukan nol dan bukan `undefined` |
| Menyimpan | Tombol *Simpan Pemantauan* nonaktif selama salah satu dari dua permintaan berjalan, ditambah penjaga `useRef` untuk klik yang sangat rapat |
| Gagal menyimpan tanda vital | Kotak merah berisi pesan backend di dalam bagian Tanda Vital; pemantauan **tidak** dikirim |
| Gagal menyimpan pemantauan | Pesan backend pada kartu, mis. *"Tanda vital yang dipilih bukan milik pasien pada kunjungan ini."*; isian tetap utuh sehingga pilihan dapat diperbaiki lalu dikirim ulang |
| Gagal `409` periode tertutup | Pesan backend apa adanya, lalu daftar periode dimuat ulang; formulir berganti menjadi pemberitahuan periode sudah ditutup |
| Kegagalan sebagian | Pesan kuning bahwa tanda vital sudah tersimpan dan akan dipakai ulang |
| Tanpa hak akses | `403` triase menyembunyikan bagian ABCDE; `403` lain tetap ditangani `EmergencyAssessmentSection` seperti sebelumnya |
| Identitas pasien belum lengkap | Pemberitahuan biru non-blocking; pilihan jalur tanda vital dinonaktifkan, sisa formulir tetap dapat diisi |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/clinical-management/patient-vital-signs` | Mencatat tanda vital baru dari layar pemantauan | `PatientVitalSign : Create` |
| `GET` | `/v1/health-services/clinical-management/patient-vital-signs?patientId=&encounterId=` | Daftar pilihan tanda vital kunjungan ini | `PatientVitalSign : Read` |

#### Health Services / Emergency Installation Management / Emergency Observation Detail

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/.../emergency-observation-details` | Menyimpan satu putaran pemantauan beserta tautan tanda vitalnya | `EmergencyObservationDetail : Create` |
| `GET` | `/v1/.../emergency-observation-details?emergencyObservationId=` | Riwayat pemantauan beserta proyeksi `vitalSign` dan `recordedByName` | `EmergencyObservationDetail : Read` |

#### Health Services / Emergency Installation Management / Emergency Triage

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/.../emergency-triages?emergencyVisitId=&sortBy=startedAt` | Ringkasan ABCDE penilaian triase terakhir, baca saja | `EmergencyTriage : Read` |

### 5.1 Badan permintaan pemantauan — bukti hanya identitas yang dikirim

Ditangkap dari thunk yang sebenarnya lewat pemeriksaan sementara (bagian 6), dengan
`InstanceAxios.post` di-stub:

```json
{
  "emergencyObservationId": "obs-1",
  "patientVitalSignId": "vital-1",
  "recordedAt": "2026-09-16T10:30:00.000Z",
  "clinicalConditionSummary": "Nyeri dada berkurang.",
  "urineOutputMl": 150,
  "isActive": true
}
```

Tiga belas kunci diperiksa **tidak ada** pada badan itu: `bloodPressureSystolic`,
`bloodPressureDiastolic`, `pulseRate`, `respiratoryRate`, `temperature`, `oxygenSaturation`,
`gcsEye`, `gcsVerbal`, `gcsMotor`, `gcsTotal`, `consciousnessStatus`, `oxygenSupportType`, dan
`recordedByUserId`.

`recordedByUserId` memang **tidak pernah** dikirim layar ini, bahkan sebelum task — jadi tidak ada
yang perlu dihapus. Backend `BE-IGD-046` mengabaikannya dan memakai pengguna terautentikasi.

### 5.2 Badan permintaan tanda vital

`gcsTotal` **tidak pernah** ikut: backend yang menjumlahkannya dari E/V/M
(`PatientVitalSignController` memanggil `CalculateGcsTotal`). Layar juga tidak pernah menghitung
totalnya sendiri untuk ditampilkan — yang ditampilkan adalah `gcsTotal` dari proyeksi backend.
`vitalSignSource` dikirim bernilai `4` (`PatientVitalSignSource.EmergencyObservation`) supaya asal
pengukurannya terbaca; nilainya diverifikasi langsung terhadap enum backend.

**Contract mismatch:** `NONE` terhadap kontrak `0.6.0`. Dua temuan konstanta frontend yang **sudah
ada sebelumnya** dicatat pada bagian 8 tanpa diperbaiki.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet` pada 6 berkas source yang berubah | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run lint:errors` (`eslint . --quiet`) | exit 0, nol error pada seluruh repository | `PASS` | Keluaran perintah |
| `npm run test:unit` | Gagal sebelum test berjalan: *"Could not find '...\tests\unit\**\*.test.mjs'"* — Node tidak mengekspansi glob pada `--test` | `EXISTING / ENVIRONMENT ISSUE` | Sama persis seperti `FE-IGD-023` dan `FE-IGD-024`; masalah skrip `package.json`, tidak berkaitan dengan perubahan ini |
| `node --import ./tests/helpers/register.mjs --test tests/unit` (perintah DoD roadmap) | **857 test, 857 lulus, 0 gagal** — 852 test lama tetap lulus, 5 test baru milik pembentuk payload tanda vital | `PASS` | Keluaran perintah |
| Pemeriksaan **sementara** di scratchpad sesi — **bukan** test repository | 5/5 lulus: badan pemantauan hanya membawa `patientVitalSignId` (13 kunci terlarang absen); penolakan membawa `status` 409 beserta pesan backend; tanda vital dikirim ke endpoint Clinical Management; urutan simpan tanda-vital-dahulu, pemakaian ulang identitas saat gagal sebagian, penjaga klik ganda, riwayat membaca proyeksi backend, nol isian obat/EKG/DC Shock, nol total GCS yang dihitung layar; daftar pilihan hanya diminta ketika pasien dan encounter ada | `PASS` | Keluaran skrip; `InstanceAxios.post` di-stub |
| Grep anti-regresi pada 774 baris yang ditambahkan | Nol warna literal, nol `<button`/`btn-*`, nol `<table`, nol `fw-*`/`fs-*`, nol `style={{`, nol `!important` | `PASS` | Keluaran perintah |
| `npm run build` | **Berhasil tanpa error.** Tahap `postbuild` (`scripts/prepare-standalone.mjs`) ikut selesai: static assets tersalin, public assets tersalin, standalone runtime siap dijalankan | `PASS` | Dijalankan **pemilik, 16 September 2026**; keluaran terminal owner |
| Uji di peramban — catat tanda vital baru lalu simpan satu putaran pemantauan | **Berhasil tanpa galat.** Periode `OBS-260916061355-B87DFC`, putaran 16 September 2026 13.00 | `PASS` | Dijalankan **pemilik, 16 September 2026**; tangkapan layar tab Observasi |
| Riwayat menampilkan angka dari proyeksi backend | **Berhasil.** TD `100/65 mmHg`, nadi `250 x/menit`, RR `80 x/menit`, suhu `40 °C`, SpO₂ `80 %`, GCS `E3 V3 M4 (total 10)`, kesadaran `Sadar penuh`, cairan masuk `500 ml`, urine `200 ml` | `PASS` | Tangkapan layar yang sama. Membuktikan runtime `BE-IGD-046` sekaligus — proyeksi `vitalSign` terbaca dari balasan asli |
| Nama pencatat, bukan GUID | **Berhasil.** *"Pencatat: SuperAdmin"* | `PASS` | Tangkapan layar yang sama |
| Nilai kosong tampil sebagai tanda hubung | **Berhasil.** Keluaran lain, perdarahan, dan muntah tampil `-`, bukan `0` | `PASS` | Tangkapan layar yang sama |
| Periode tertutup menolak pemantauan baru | **Berhasil.** Kartu *Catat Pemantauan* diganti pemberitahuan *"Periode observasi ini sudah ditutup. Pemantauan baru tidak dapat ditambahkan."* | `PASS` | Tangkapan layar yang sama |
| Isian Kesimpulan `FE-IGD-024` tetap bekerja | **Berhasil.** Kartu periode menampilkan `KESIMPULAN: testing` | `PASS` | Tangkapan layar yang sama |
| Ringkasan ABCDE — keadaan kosong | **Berhasil.** *"Belum ada penilaian triase pada kunjungan ini."* beserta petunjuknya | `PASS` | Tangkapan layar yang sama. Keadaan **terisi** belum dilihat |
| Memilih tanda vital yang sudah tercatat (kriteria 3) | Tidak dijalankan | `NOT RUN` | Putaran uji memakai jalur *catat baru* |
| Pasien provisional tanpa `PatientId` (`IGD-EV-130`) | Tidak dijalankan | `NOT RUN` | — |

Uji manual: **`PASS`**. `NOT FEASIBLE` bagi agent — tidak menjalankan peramban dengan sesi
petugas — sehingga **dijalankan pemilik** pada 16 September 2026. Putaran itu sekaligus menjadi
bukti runtime pertama bagi `BE-IGD-046`: angka tanda vital yang tampil di riwayat datang dari
proyeksi backend, bukan dari isian formulir.

**Bukan UAT.** Ini verifikasi pengembangan oleh pemilik modul; UAT milik tim terpisah.

**Tidak dijalankan:** `test:e2e` dan `test:uat` (tidak diminta task), jalur pilih-existing, dan
pasien provisional.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bagian **Tanda Vital** tersedia di dalam *Catat Pemantauan* | Terpenuhi | `EmergencyAssessmentFormSection title="Tanda Vital"` pada tab |
| 2. Dapat mencatat tanda vital baru memakai formulir yang sudah ada, tanpa berpindah tab | Terpenuhi | `VitalSignTab` dipakai apa adanya di dalam kartu pemantauan |
| 3. Dapat memilih tanda vital yang sudah tercatat; daftarnya hanya kunjungan itu, disaring backend | Terpenuhi | `fetchVitalSigns(context)` mengirim `patientId` **dan** `encounterId`; dispatch dijaga `tautanVitalTersedia` |
| 4. Yang dikirim hanya `patientVitalSignId`; nol angka tanda vital | Terpenuhi | Bagian 5.1 — 13 kunci terlarang absen dari badan permintaan |
| 5. Riwayat menampilkan angka dari proyeksi backend, bukan isian formulir | Terpenuhi | `const vital = item?.vitalSign || null;` lalu `VitalSignGrid` |
| 6. Riwayat menampilkan GCS E/V/M beserta total, kesadaran, dan oksigen bila backend mengirimnya; layar tidak menghitung total | Terpenuhi | `formatGcs` hanya membaca `gcsTotal`; nol penjumlahan pada source |
| 7. `recordedByName` tampil; kosong menjadi tanda hubung, bukan GUID | Terpenuhi | `Pencatat: {item?.recordedByName \|\| "-"}`; GUID tidak pernah dirender |
| 8. Ringkasan ABCDE triase terakhir tampil baca saja, tanpa endpoint baru | Terpenuhi | Bagian **Primary Survey Terakhir**; memakai endpoint triase yang sudah ada lewat `fetchEmergencyTriages`. **Delta tercatat:** kartu mengandaikan datanya sudah dimuat workspace, nyatanya belum — satu thunk daftar ditambahkan atas keputusan pemilik 16 September 2026 |
| 9. Periode `Completed`/`Cancelled` tanpa jalan masuk pencatatan; `409` backend tampil apa adanya | Terpenuhi | `periodeTertutup` mengganti kartu dengan pemberitahuan; `status === 409` memuat ulang daftar periode |
| 10. Isian Kesimpulan dan alur Selesaikan milik `FE-IGD-024` tetap bekerja | Terpenuhi | `runAction`, `collectsConclusion`, badan `ConfirmModal`, dan `OBSERVATION_CONCLUSION_MAX_LENGTH` tidak disentuh; 857 test lulus |
| 11. Nol isian obat, gambaran EKG, atau DC Shock | Terpenuhi | Pemeriksaan sementara: nol `obatDosis`, `dcShock`, `defibril`, `medicationName`; kata "EKG" hanya pada dua placeholder teks yang sudah ada sebelum task |
| 12. Layar Resusitasi tidak disentuh | Terpenuhi | Nol berkas resusitasi pada `git status` |
| 13. Nilai kosong tampil sebagai tanda hubung; kosong berarti tidak diukur, bukan nol | Terpenuhi | `isEmptyValue` dan `formatNumber` mengembalikan `"-"`; pembentuk payload mengirim `null`, bukan `0` |

**Definition of Done gelombang R3.7**

| Butir | Status |
| --- | --- |
| Acceptance criteria terpetakan ke source | Terpenuhi — tabel di atas |
| `npm run lint:errors` dijalankan dan dicatat apa adanya | Terpenuhi — `PASS` |
| `node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan dan dicatat apa adanya | Terpenuhi — 857/857 lulus |
| Perintah `npm run build` diberikan kepada Rizki | Terpenuhi — perintahnya diberikan, lalu dijalankan pemilik 16 September 2026 dengan hasil berhasil tanpa error |
| Catatan uji layar ditulis apa adanya | Terpenuhi — `NOT FEASIBLE` beserta alasannya |
| Laporan tracked `task/report/frontend/FE-IGD-028.md` | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi — `frontend-roadmap.md` R3.7 dan `requirement-traceability.md` R3.5 |
| Nol komponen bersama dan CSS global diubah | Terpenuhi — nol berkas `.css`, nol berkas `features/` yang diubah |
| Tanpa UAT PASS | Terpenuhi — status UAT tidak ditulis lulus di mana pun |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tab Observasi kini ±1.400 baris setelah bertambah tiga wilayah. Pemecahan menjadi beberapa komponen layak dipertimbangkan sebagai task rapi-rapi tersendiri, dan sengaja **tidak** dikerjakan di sini supaya diff-nya tetap terbaca sebagai satu fitur |
| Masalah yang diketahui | **(1)** Kegagalan sebagian pada 2.4 kehilangan tautannya bila tab ditinggalkan — pemulihannya lewat jalur pilih yang sudah tercatat. **(2)** Dua konstanta frontend yang **sudah ada sebelum task ini** tidak cocok dengan enum backend: `CONSCIOUSNESS_OPTIONS` pada `emergency-assessment-constant.jsx` memetakan `5` sebagai *Soporocoma* padahal backend `5 = Coma`, dan `PATIENT_VITAL_SIGN_SOURCE.ManualEntry = 1` padahal backend `1 = Assessment` dan `ManualEntry = 8`. Keduanya **nol pemakai** saat ini sehingga belum pernah salah tampil; keduanya **tidak** diperbaiki karena di luar lingkup task, dan task ini sengaja memakai konstanta yang benar. **(3)** Daftar pilihan menyembunyikan tanda vital yang dibatalkan lewat penyaring tampilan di browser — lingkupnya sendiri tetap dari backend, dan penolakan sebenarnya tetap milik backend. **(4)** *Ditemukan pada uji layar 16 September 2026, di luar lingkup task ini:* formulir tanda vital bersama menerima kombinasi yang saling bertentangan — riwayat menampilkan *"Oksigen: Tidak menggunakan · 50 L/menit"*, yang berarti baris tersimpan dengan `isUsingOxygen = true` sementara jenisnya bernilai "tidak menggunakan" dan laju alirnya terisi. Tampilannya benar; yang longgar adalah isiannya. Milik formulir tanda vital `ClinicalManagement`, bukan tab Observasi. **(5)** *Ditemukan pada uji yang sama:* nilai tanda vital di luar batas fisiologis diterima tanpa peringatan — putaran uji tersimpan dengan nadi `250 x/menit` dan frekuensi napas `80 x/menit`. Penanda *"Tanda Vital Kritis"* memang menyala, tetapi tidak ada batas kewajaran yang menahan salah ketik. Keduanya layak dipertimbangkan sebagai kartu tersendiri oleh pemilik `ClinicalManagement` |
| Dependency backend | `BE-IGD-046` sudah selesai implementasi dan build-nya bersih, tetapi **runtime-nya belum diverifikasi**. Proyeksi `vitalSign` dan `recordedByName` karena itu belum pernah dilihat dari balasan asli; bila keduanya ternyata tidak terkirim, riwayat akan tampil tanpa angka tanda vital dan tanpa nama pencatat, tanpa merusak bagian lain layar |
| Perubahan sampingan | `NONE` — working tree frontend bersih ketika task dimulai, dan tujuh berkas pada bagian 3.2 seluruhnya milik task ini |
| Interupsi | `NONE` |
| Status Git | Bagian 8.1 |
| Langkah berikutnya | 1) ~~Build~~ — **selesai 16 September 2026, tanpa error**. 2) Uji lewat peramban: catat baru, pilih yang sudah tercatat, periode `Completed`, dan satu kunjungan tanpa `patientId`. 3) Verifikasi runtime `BE-IGD-046` (uji API manual contoh 1–10 pada laporannya) supaya proyeksi `vitalSign` terbukti — keduanya dapat dijalankan pada satu sesi uji yang sama. 4) `IGD-OQ-089` (alat jalan napas terstruktur) dan `IGD-OQ-090` (entri susulan) tetap terbuka dan tidak menahan apa pun di layar ini |

**Perintah build** (dari folder `QuilvianSystemFrontendDev`): `npm run build` — sudah dijalankan pemilik pada 16 September 2026, berhasil tanpa error.

### 8.1 `git status --short` di akhir pekerjaan

```text
 M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx
 M src/components/view/health-services/emergency-installation-management/emergency-assessment-view/emergency-assessment-detail-view.jsx
 M src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx
 M src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-assessment-detail.jsx
 M src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx
?? src/utils/health-services/clinical-management/patient-vital-sign-payload.utils.js
?? tests/unit/patient-vital-sign-payload.test.mjs
```

Branch tetap `RizkiV2`. Tidak ada stage, commit, push, merge, maupun perpindahan branch.

### 8.2 Yang sengaja tidak dikerjakan

| Hal | Alasan |
| --- | --- |
| Bentuk terstruktur alat bantu jalan napas (OPA, NPA, ETT, LMA, stoma) | `IGD-DEC-124`, `IGD-OQ-089` masih terbuka. Alat dan tindakan jalan napas tetap ditulis pada *Tindakan yang Dilakukan* |
| Isian obat, dosis, gambaran EKG, dan DC Shock | Milik Farmasi, Tindakan, dan Resusitasi. Dilarang kontrak bagian 7.4 |
| Layar Resusitasi | Di luar lingkup; `IGD-EV-111` sengaja tanpa ID task |
| Perbaikan `PUT` detail menjadi tambah-saja | `IGD-DEC-080`, temuan `J-4` — di luar lingkup, dan layar ini memang tidak punya jalur ubah pemantauan |
| Penyelesaian gap pasien provisional | Milik pemilik domain; layar hanya menjelaskan keadaannya tanpa memblokir |
| Penambahan nilai enum oksigen (Head Box, JR/T-Piece, Ambu Bag, Ventilator) | `IGD-DEC-125` — milik `ClinicalManagement`, dirutekan sebagai permintaan tersendiri |
| Perbaikan dua konstanta frontend yang tidak cocok enum backend | Nol pemakai, di luar lingkup; dicatat pada bagian 8 |
