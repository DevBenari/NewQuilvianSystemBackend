# Laporan Susulan Frontend — `FE-IGD-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-025` |
| Judul | Laporan susulan perombakan layar pengkajian (`bd1d94a8a`) dan temuan privasi |
| Slice | `IGD-S04` · lanjutan `FE-IGD-022` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.7 |
| Keputusan | `IGD-DEC-107` (IGD pemegang wewenang sementara `ClinicalManagement`); bukti `IGD-EV-117` |
| Kontrak | **Nol perubahan** |
| Klasifikasi | `REPORT` — **nol perubahan source di kedua repository** |
| Source yang dibaca | frontend `RizkiV2` `c4bcc39ac`; backend `rizkiG` `ed5cb292` |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI — acceptance 1–5 terpenuhi; `git diff` source kosong** |

---

## 1. Acceptance 1 — perombakan 19 berkas

Commit `bd1d94a8a`, **31 Agustus 2026, Rizki Gunawan**, *"memperbaiki tampilan pengkajian pasien
IGD"* — **19 berkas, 734 sisipan, 822 penghapusan**.

| Kelompok | Berkas | Catatan |
| --- | --- | --- |
| **Dihapus** | `emergency-assessment-vital-sign-tab.jsx` (**−403**) | Tab Tanda Vital IGD tersendiri; lihat bagian 1.1 |
| Ditambah | `emergency-assessment-date-time-field.jsx` (+78) | Field tanggal-jam dipakai bersama tab observasi dan transfer |
| Dirombak besar | `emergency-assessment-observation-tab.jsx` (215), `emergency-assessment-nosocomial-tab.jsx` (188), `emergency-assessment-disposition-tab.jsx` (184), `emergency-assessment-initial-tab.jsx` (145) | Empat tab utama ruang kerja perawat |
| Dirombak kecil | `emergency-assessment-transfer-tab.jsx` (37), `emergency-assessment-form-card.jsx` (22), `emergency-assessment-detail-view.jsx` (20), `emergency-assessment-diagnostic-support-tab.jsx` (−18), `emergency-assessment-record-tab.jsx` (−16), `emergency-assessment-section.jsx` (−11), `emergency-assessment-soap-tab.jsx` (−8), `emergency-triage-form-view.jsx` (8) | — |
| State dan hook | `emergency-assessment-slice.jsx` (32), `use-emergency-assessment-detail.jsx` (18), `emergency-assessment-constant.jsx` (−5) | Memuat penjaga lingkup bagian 2 |
| Gaya | `emergency-assessment.module.css` (96), `emergency-triage.module.css` (52) | Nol `globals.css` |

### 1.1 Kenapa tab Tanda Vital IGD tersendiri dihapus, dan ke mana isiannya pindah

Tab itu **403 baris yang menggandakan** formulir tanda vital yang sudah ada milik Nurse Station.
Penggandaan semacam itu berarti setiap perubahan aturan tanda vital harus dikerjakan dua kali, dan
yang terlewat menjadi selisih diam-diam antara layar IGD dan layar rawat jalan.

Isiannya **tidak hilang** — ia dipindahkan ke komponen bersama, dan hari ini masih di sana:

```
emergency-assessment-initial-tab.jsx:18   import VitalSignTab from "@/components/features/health-services/nurse-station-management/VitalSignTab";
emergency-assessment-initial-tab.jsx:17   import AssessmentTab from "@/components/features/health-services/nurse-station-management/AssessmentTab";
emergency-assessment-initial-tab.jsx:919  <VitalSignTab …
emergency-assessment-initial-tab.jsx:927  <AssessmentTab …
```

Jadi tanda vital kini dirender **di dalam** tab *Assesmen Awal IGD* memakai komponen Nurse Station,
berurutan sebelum formulir pengkajian. Pola ini yang kemudian dipetakan `FE-IGD-019` sebagai
*"Assesmen Awal IGD memakai formulir bersama"*.

## 2. Acceptance 2 — penjaga lingkup data, sebelum dan sesudah

### Bentuk penjaganya

`emergency-assessment-slice.jsx`, `buildListThunk` (`:294`) menerima argumen keempat
`requiredScope`. Penjaganya di `:312`–`:314`:

```js
if (requiredScope.length > 0 && !requiredScope.some((key) => context?.[key])) {
  return [];
}
```

Komentar panjang di atasnya (`:297`–`:311`) menjelaskan bahayanya: konteks kunjungan dimuat
belakangan, sehingga pada render pertama `patientId` dan `encounterId` masih string kosong. Nilai
kosong diubah menjadi `undefined`, **Axios membuang parameter `undefined`**, dan permintaannya
berangkat **tanpa penyaring apa pun** — lalu endpoint daftar klinis menjawabnya dengan seluruh isi
tabel.

### Sebelum dan sesudah

| | Perilaku |
| --- | --- |
| **Sebelum** | `fetchPatientAssessments({ patientId: "", encounterId: "" })` → `GET patient-assessments?pageNumber=1&pageSize=50` **tanpa penyaring pasien** → 50 pengkajian terbaru **milik pasien mana pun** masuk ke state layar pasien ini |
| **Sesudah** | Thunk mengembalikan `[]` tanpa bertanya sama sekali. Begitu konteksnya tiba, thunk dijalankan ulang dengan penyaring yang benar |

Dasarnya dinyatakan komentar itu sendiri: *"Daftar kosong sesaat jauh lebih benar daripada daftar
milik orang lain."*

### Cakupan penjaga hari ini — tujuh dari tiga belas

| Thunk | `requiredScope` |
| --- | :---: |
| `fetchIntegratedNotes` (`:337`) | ✅ |
| `fetchPatientAssessments` (`:348`) | ✅ |
| `fetchVitalSigns` (`:358`) | ✅ |
| `fetchProcedures` (`:432`) | ✅ |
| `fetchPrescriptions` | ✅ |
| `fetchRadOrders` (`:790`) | ✅ |
| `fetchBloodOrders` (`:807`) | ✅ |
| `fetchEmergencyTriages`, `fetchObservations`, `fetchObservationDetails`, `fetchDispositions`, `fetchTransfers`, `fetchLegacyProcedures` | — |

**Keenam yang tanpa `requiredScope` bukan celah**, dan ini perlu dinyatakan supaya tidak dibaca
sebagai temuan: keenamnya memanggil endpoint **milik IGD sendiri** dan disaring
`emergencyVisitId`, sedangkan efek pemuatnya di `use-emergency-assessment-detail.jsx` dibuka
dengan `if (!emergencyVisitId || !visitContextLoaded) return;`. Jadi keenamnya tidak pernah
berangkat sebelum `emergencyVisitId` ada.

`requiredScope` dibutuhkan justru pada tujuh thunk di atas karena kuncinya `patientId` dan
`encounterId` — keduanya **tiba belakangan** dari `visitContext`, sesudah `emergencyVisitId`
sudah ada dan efeknya sudah lolos penjaga hook. Perbedaan waktu tiba itulah celahnya.

`fetchNosocomialInfections` yang disebut kartu memakai jalur yang sama; pemuatnya hanya
dijalankan pada tab Nosokomial, dan penyaringnya mengikuti pola yang sama.

## 3. Acceptance 3 — endpoint daftar klinis yang menjawab tanpa filter

Keempatnya ada di `NewQuilvianSystemBackend`. Pada seluruhnya, `patientId` dan `encounterId` adalah
parameter **nullable tanpa `[Required]`**, penyaringnya diterapkan **hanya bila nilainya ada**, dan
**nol `BadRequest`** diterbitkan saat lingkup pasien tidak disebut.

| # | Method + Path | Baris source | Bukti |
| ---: | --- | --- | --- |
| 1 | `GET api/v1/health-services/clinical-management/patient-assessments` | `PatientAssessmentController.cs:316` | Penyaring bersyarat `:357` (`x.EncounterId == encounterId.Value`) dan `:363` (`x.PatientId == patientId.Value`) |
| 2 | `GET api/v1/health-services/clinical-management/patient-vital-signs` | `PatientVitalSignController.cs:191` | `[FromQuery] Guid? patientId` `:197`, `[FromQuery] Guid? encounterId` `:198` |
| 3 | `GET api/v1/health-services/clinical-management/patient-integrated-progress-notes` | `PatientIntegratedProgressNoteController.cs:273` | `[FromQuery] Guid? patientId` `:279`, `encounterId` `:280` |
| 4 | `GET api/v1/health-services/clinical-management/patient-procedures` | `PatientProcedureController.cs:313` | `[FromQuery] Guid? encounterId` `:319`, `patientId` `:321` |

Hak akses yang dijaga adalah **jenis data**, bukan **pasiennya**: `[AccessPermission("PatientVitalSign", "Read")]`
mengizinkan membaca tanda vital, tanpa menyatakan tanda vital **siapa**. Pembatasan per pasien
sepenuhnya bergantung pada pemanggil yang mengirim penyaring.

Akibatnya pembatasan data pasien hari ini **ditegakkan di layar, bukan di server**. Penjaga
`requiredScope` pada bagian 2 adalah satu-satunya lapisan yang mencegahnya pada layar IGD — dan
pemanggil lain, termasuk yang belum ditulis, tidak terikat olehnya.

## 4. Acceptance 4 — diserahkan sebagai rekomendasi, tidak diperbaiki

Temuan bagian 3 **tidak diperbaiki** pada task ini. `IGD-DEC-107` menempatkan IGD sebagai pemegang
wewenang **sementara** `ClinicalManagement`, dan wewenang sementara tidak cukup untuk mengubah
kontrak baca empat endpoint yang dipakai lintas modul.

**Rekomendasi untuk pemilik `ClinicalManagement`**, diurutkan dari yang paling murah:

| # | Usulan | Dampak |
| ---: | --- | --- |
| 1 | Tolak `400` bila keempat endpoint dipanggil **tanpa** `patientId` maupun `encounterId`, kecuali pemanggil memegang izin pelaporan lintas pasien | Menutup celahnya di server; menyentuh pemanggil yang hari ini mengandalkan daftar tanpa penyaring |
| 2 | Pisahkan izin *baca lintas pasien* dari izin baca biasa | Lebih benar secara otorisasi; menuntut pemetaan ulang hak akses |
| 3 | Biarkan apa adanya dan nyatakan pembatasan per pasien sebagai tanggung jawab pemanggil | Termurah, tetapi menjadikan setiap layar baru sebagai titik bocor berikutnya |

Pilihan ada pada pemilik `ClinicalManagement`. Yang diminta laporan ini hanya agar pilihannya
**dinyatakan**, bukan diwariskan diam-diam.

## 5. Acceptance 5 — nol perubahan source

| Pemeriksaan | Hasil |
| --- | --- |
| `git diff` frontend | **kosong** |
| `git diff` backend | **kosong** |
| Berkas yang ditulis | Laporan ini, baris status roadmap, traceability |
