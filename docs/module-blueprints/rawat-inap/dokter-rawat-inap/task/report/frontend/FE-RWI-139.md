# Laporan Perubahan Frontend — `FE-RWI-139`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-139` |
| Judul | ICD-10 dan rekomendasi dari master; penyimpanan diagnosa yang benar |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 1 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.2; didaftarkan di [`frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) |
| Trace | Permintaan pemilik 1 dan 4; keputusan K1, K2; cacat C1–C3, C5–C8 (`soap.md` bagian 2.3); `soap.md` bagian 3.1 dan 3.4 |
| Contract version | `0.6.1` + delta `BE-RWI-142` (`soap.md` bagian 4.2), disetujui pemilik 30-09-2026 |
| Wewenang UI | Rencana kerja `soap.md` Rev 2.1 yang disetujui pemilik 30-09-2026 ("saya ingin anda kerjakan semua sampai tuntas"). Batasnya: tab SOAP ruang kerja dokter rawat inap saja |
| Dependency | Endpoint yang sudah ada. `BE-RWI-142` (urutan relevansi, nama obat/tindakan, gerbang K1) — ✅ source 30-09-2026 |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2, berkas diubah 1 (7 berkas), logika 2, kontrak 1 (memakai kontrak yang ada), database 0, keamanan 0, UI 1 |
| Task mode | `FRONTEND` (laporan di repository backend) |
| Target tulis | `QuilvianSystemFrontendDev/src/**` bagian SOAP rawat inap, `tests/unit/inpatient-soap-modernization.test.mjs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `82487a491` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b342ae46` + perubahan `BE-RWI-141`/`BE-RWI-142` di working tree |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. ESLint dan test PASS. `npm run build` dan runtime **NOT RUN** |

---

## 1. Keadaan yang ditemukan di awal

1. **ICD hardcode.** Form SOAP rawat inap memakai `src/utils/icdData.jsx`: 10 diagnosa dan planning fiktif salinan V1, lengkap dengan GUID milik database V1. Importer-nya hanya `soap-icd-section.jsx` dan `use-inpatient-progress-note.jsx`.
2. **Diagnosa tidak pernah tersimpan (C1–C3).** Diagnosa dari dropdown cepat membawa GUID V1, backend menolaknya, dan error ditelan `console.warn`. SOAP tetap dikunci **tanpa diagnosa terkode**.
3. **Hapus diagnosa tidak tersimpan.** Tabel hanya membuang baris di layar. Setelah dimuat ulang, diagnosa kembali lagi.
4. **Assessment menimpa ketikan dokter** setiap kali diagnosa berubah.
5. **Planning dari data fiktif**, bukan master rekomendasi terapi yang sudah punya alur tinjauan (`ActiveForSoap`).

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** dokter rawat inap saat visite, di Tab SOAP → Form SOAP.

1. Dokter mengetik minimal dua huruf di **Cari ICD-10**, misalnya "J18" atau "pneumonia". Hasil datang dari master `MstDiagnosis` dengan kode persis di atas.
2. Pilihan lain:
   - klik chip **Diagnosa aktif episode** (diagnosa yang sudah tercatat dari IGD, admisi, atau SOAP kemarin);
   - buka **Katalog ICD-10** untuk menjelajah.
3. Diagnosa pertama otomatis menjadi **Utama**, kecuali master melarangnya (`IsPrimaryDiagnosisAllowed = false`). Dalam kasus itu layar memberi tahu bahwa diagnosa ditambahkan sebagai Tambahan.
4. Tabel diagnosa (`DoctorDiagnosisTable`, sama dengan SOAP poliklinik) menyediakan **Jadikan Utama** dan **Hapus**. Utama selalu tepat satu.
5. Penyimpanan:
   - **catatan yang sudah tersimpan:** setiap aksi langsung ke API, yaitu tambah `POST`, hapus `PATCH /{id}/cancel` dengan alasan "Dihapus dari SOAP oleh dokter penulis.", dan ganti Utama `PATCH /{id}/set-primary`. Daftar lalu dibaca ulang dari server;
   - **catatan baru:** diagnosa ditampung di layar, dan saat **Simpan Draf** atau **Selesaikan** pertama disimpan berurutan (Utama lebih dulu) **sebelum** layar berpindah ke catatan barunya.
6. **Assessment** terisi blok otomatis di atas:

   ```text
   1. J18.0 - Bronchopneumonia, unspecified (Diagnosa Utama)
   2. E86 - Volume depletion (Diagnosa Tambahan)
   ```

   Narasi dokter di bawahnya tidak pernah ditimpa. Bila dokter menyunting blok itu sendiri, pembaruan otomatis berhenti dan muncul petunjuk **Susun ulang blok diagnosa**.
7. **Planning per Diagnosa** menampilkan rekomendasi aktif dari master, dikelompokkan Obat · Tindakan · Penunjang · Rujukan · Kontrol · Edukasi. Mencentang menambah satu baris ke Plan, misalnya `• Terapi: Ceftriaxone 1 g — 1 g · IV · tiap 12 jam`. Menghapus centang mencabut baris yang sama. Baris tercentang juga disimpan ke kolom Plan terstruktur backend (`PrescriptionPlan`, `ProcedurePlan`, `SupportingExamPlan`, `ReferralPlan`, `FollowUpNote`, `EducationPlan`).

**Jalur tidak normal.**
- Penyimpanan diagnosa gagal: layar menulis *"Diagnosa I10 belum tersimpan: <pesan server>"*, penyelesaian dibatalkan, dan catatan tetap Draf. Ini skenario 3 `soap.md`.
- Pencarian gagal: pesan *"Pencarian ICD-10 gagal. Coba lagi."* Tidak ada fallback hardcode.
- Diagnosa sudah ada di daftar: pesan *"ICD-10 J18.0 sudah ada dalam daftar."*
- Belum ada rekomendasi aktif: *"Belum ada rekomendasi aktif untuk diagnosa terpilih. Tulis rencana langsung di kolom Plan."*
- Selesaikan tanpa diagnosa atau tanpa Utama: tombol **Selesaikan & Kunci** nonaktif dan banner kuning menyebut kekurangannya. Bila backend tetap menolak, kalimat penolakannya ditampilkan apa adanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/` (`frontend-architecture.md`, `base-component-catalog.md`, `base-component-decision-gate.md`, `design-tokens.md`, `page-composition-patterns.md`, `ui-consistency-checklist.md`, `test-policy.md`); tab SOAP rawat inap lama beserta hook dan utils-nya; SOAP poliklinik (`use-doctor-soap.js`, `doctor-soap-utils.js`); base klinis `DoctorDiagnosisTable`, `DoctorDiagnosisSearchModal`, `ClinicalSectionPanel`; base `FilterSelect`, `BaseCheckboxCard`, `InformationAlert`, `BaseButton`; service `patient-diagnosis.service.js`, `diagnosis-recommendation.service.js`; source V1 `soap-componen/form-soap.jsx`; backend `PatientDiagnosisController`, `DiagnosisRecommendationResolverController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/icdData.jsx` | **Dihapus.** Grep memastikan tidak ada importer |
| `src/lib/hooks/health-services/inpatient-management/use-progress-note-diagnoses.jsx` (baru) | Diagnosa satu catatan: muat, cari (penjaga urutan request), katalog, chip episode, tambah/hapus/Utama, `persistPending`, `copyDiagnoses` (`FE-RWI-142`), rekomendasi resolver |
| `src/components/view/.../tabs/progress-note/soap-icd-section.jsx` | Ditulis ulang: `FilterSelect` server-side, tombol Katalog, chip episode, tabel, pesan gagal/pemberitahuan, modal katalog |
| `src/components/view/.../tabs/progress-note/soap-planning-accordion.jsx` | Ditulis ulang: akordeon per diagnosa dari resolver, `BaseCheckboxCard` compact |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-progress-note.jsx` | Blok Assessment terlindungi, centang rekomendasi → Plan, diagnosa disimpan sebelum Selesaikan, pesan penolakan backend dibaca per butir |
| `src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx` | `buildAssessmentBlock`, `replaceGeneratedPrefix`, `normalizeRecommendations`, `togglePlanLine`, `isPlanLineChecked`, `derivePlanStructuredFields`, `findProgressNoteCompletionIssues`, `extractFinalizationIssueMessages` |
| `src/lib/constants/health-services/inpatient-management/inpatient-progress-note-constants.jsx` | `PLAN_RECOMMENDATION_GROUP`, `PROCEDURE_RECOMMENDATION_GROUP_BY_TYPE`, `PLAN_STRUCTURED_FIELDS`, kalimat K1 yang sama dengan backend |
| `tests/unit/inpatient-soap-modernization.test.mjs` | Ditulis ulang untuk Rev 2 (12 test fungsi murni, termasuk blok Assessment, penggantian blok, dan centang Plan) |

### 3.3 Kepatuhan arsitektur frontend

Alurnya `view → hook → service → InstanceAxios`, dengan logika murni di `utils` dan teks di `constants`. Tidak ada Redux slice baru, dan hook baru ditempatkan bersebelahan dengan hook tab. Service yang ada dipakai apa adanya; tidak ada service baru.

**Tabel keputusan base component**

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kolom cari ICD async | `REUSE` `FilterSelect` (`serverSide`, `searchable`, `onSearchChange`) | Pola pencarian server-side modul lain |
| Tombol Katalog, chip episode, Salin | `REUSE` `BaseButton` (`secondary`, `subtle`, `ghost`) | Katalog base |
| Tabel diagnosa | `REUSE` `DoctorDiagnosisTable` | SOAP poliklinik |
| Katalog ICD | `REUSE` `DoctorDiagnosisSearchModal` | SOAP poliklinik |
| Kartu rekomendasi | `COMPOSE` `BaseCheckboxCard` compact di dalam `<details>` native | Katalog base; tanpa komponen baru |
| Pesan gagal/info | `REUSE` `InformationAlert` | Katalog base |
| Panel seksi | `REUSE` `ClinicalSectionPanel layout="document"` | Base klinis |

`UI GATE: PASS — 0 NEW, 0 EXTEND yang mengubah perilaku default; seluruh elemen REUSE/COMPOSE.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel diagnosa dalam keadaan memuat; "Mencari di master ICD-10..."; "Memuat rekomendasi planning..." |
| Kosong | "Ketik minimal 2 huruf untuk mencari"; "Diagnosa tidak ditemukan di master ICD-10"; "Pilih diagnosa ICD-10 lebih dulu untuk melihat rekomendasi planning." |
| Gagal | "Diagnosa {kode} belum tersimpan: {pesan server}" (penyelesaian dibatalkan); "Pencarian ICD-10 gagal. Coba lagi."; "Rekomendasi planning tidak dapat dimuat." |
| Tanpa hak akses | Seluruh editor dibungkus `ClinicalActionGuard mode="disable"` beserta alasannya. Pesan 403 dari API tampil lewat pesan gagal di atas |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-diagnoses/master-options?search=&take=` | Cari ICD-10 (20 cepat, 50 katalog) | `PatientDiagnosis : Read` |
| `GET` | `/v1/health-services/clinical-management/patient-diagnoses?consultationId=&pageSize=100` | Diagnosa satu catatan | `PatientDiagnosis : Read` |
| `GET` | `/v1/health-services/clinical-management/patient-diagnoses?inpEpisodeId=&diagnosisStatus=Active` | Chip diagnosa aktif episode | `PatientDiagnosis : Read` |
| `POST` | `/v1/health-services/clinical-management/patient-diagnoses` | Tambah diagnosa (`diagnosisId` master) | `PatientDiagnosis : Create` |
| `PATCH` | `/v1/health-services/clinical-management/patient-diagnoses/{id}/set-primary` | Ganti Utama | `PatientDiagnosis : SetPrimary` |
| `PATCH` | `/v1/health-services/clinical-management/patient-diagnoses/{id}/cancel` | Hapus dari SOAP (jejak audit tetap) | `PatientDiagnosis : Cancel` |

#### Health Services / Clinical Management / Diagnosis Recommendation Resolver

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/clinical-management/diagnosis-recommendations/resolve` | Rekomendasi obat/tindakan/edukasi per diagnosa | `DiagnosisRecommendationResolver : Read` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/clinical-management/doctor-consultations/{id}/complete` | Selesaikan; penolakan K1 dibaca dari `data.sections[].issues[].message` | `DoctorConsultation : Complete` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `grep -r "utils/icdData" src` | Tidak ada hasil | `PASS` | 30-09-2026 |
| `npx eslint <15 berkas SOAP> --quiet` lalu tanpa `--quiet` | Exit 0; 0 error, 0 peringatan (satu peringatan `react-hooks/purity` di Riwayat SOAP sudah diperbaiki) | `PASS` | Keluaran ESLint |
| `npx eslint src --quiet` | 1 error di `tabs/procedure/procedure-form-panel.jsx` (`react-hooks/rules-of-hooks`) — berkas Tab Tindakan dari commit `82487a491`, tidak disentuh task ini | `EXISTING / ENVIRONMENT ISSUE` | Keluaran ESLint; `git status` bersih untuk berkas itu |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-soap-modernization.test.mjs` | 12/12 lulus | `PASS` | Blok Assessment, penggantian blok, centang Plan, K1, pesan backend |
| Suite unit penuh `tests/unit/*.test.mjs` | 2106 test, 2101 lulus, 5 gagal. Garis dasar sebelum task: 2101/2096/5 | `EXISTING / ENVIRONMENT ISSUE` untuk 5 kegagalan | Kelimanya di `petty-cash-finance-separation`, `hemodialysis-sidebar-navigation` (2), `menu-permission-filter`, `accounting-reconciliation`; tidak terkait SOAP |
| Kompilasi Turbopack dev (`next dev` pemilik di port 3000) atas route `/health-services/inpatient-management/doctor-inpatient` | HTTP 200; chunk `progress-note` 291 KB tanpa satu pun stub galat kompilasi; hook diagnosa ter-bundel | `PASS` | Pengambilan chunk 30-09-2026 |
| Grep konsistensi UI (tombol/tabel/select mentah, `style={{`, hex/rgb, `!important`, dark mode, `window.confirm`) | Nihil | `PASS` | `ui-consistency-checklist.md` |
| Cek salah-prop (DoD 5) | `ConfirmModal show=`, `ClinicalActionGuard allowed=`, `ClinicalStatusBadge label=`, `ClinicalStateBoundary loading/error/denied`, `BaseTextField field={{ label }}` sesuai source | `PASS` | Dibaca dari source komponen |
| `npm run build` | — | `NOT RUN` | `next dev` pemilik sedang berjalan (PID 16584); `next build` menimpa `.next` miliknya. Build dijalankan pemilik |
| Skenario runtime kriteria 2–4 | — | `NOT RUN` | Butuh backend berjalan dan akun dokter |

MANUAL TEST: NOT FEASIBLE — tidak ada sesi peramban berakun dokter pada sesi ini; repo tanpa `playwright.config`.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-soap-modernization.test.mjs` — PASS (12/12). Test ini hanya bukti fungsi murni, bukan bukti pemenuhan DoD (`soap.md` DoD butir 6).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.2) | Status | Bukti |
| --- | --- | --- |
| 1. `grep -r "utils/icdData" src` tidak menemukan apa pun | Terpenuhi | Bagian 6 |
| 2. Mencari "J18" menampilkan kode dari `MstDiagnosis`; memilihnya menyimpan `TrxPatientDiagnosis` dengan `diagnosisId` master | Terpenuhi di tingkat source; runtime `NOT RUN` | `onSearchChange` → `searchMasterDiagnosisOptions`; `addDiagnosis` menolak ID non-UUID dan mengirim `diagnosisId` master (`buildCreatePayload`) |
| 3. Menghapus diagnosa tersimpan memanggil `cancel`, dan diagnosa itu tidak muncul lagi saat dimuat ulang | Terpenuhi di tingkat source; runtime `NOT RUN` | `removeDiagnosis` → `cancelPatientDiagnosis`; `normalizeProgressNoteDiagnoses` membuang status `Cancelled` (4); timeline backend juga menyaringnya |
| 4. Bila penyimpanan diagnosa gagal, catatan tidak diselesaikan dan pesan backend tampil | Terpenuhi di tingkat source; runtime `NOT RUN` | `persistPending` berhenti pada kegagalan pertama dan mengembalikan `{ ok: false, message }`; `completeNote` keluar sebelum `completeDoctorConsultation` |
| 5. Narasi Assessment tetap ada setelah diagnosa kedua ditambahkan | Terpenuhi | `replaceGeneratedPrefix`; test "blok buatan sistem diganti tanpa menimpa tulisan dokter" |
| 6. Menghapus centang rekomendasi mencabut barisnya dari Plan dan dari kolom terstruktur | Terpenuhi | `togglePlanLine` + `derivePlanStructuredFields`; test "rekomendasi master menjadi baris Plan…" |
| DoD 2 — ESLint berkas yang diubah bersih | Terpenuhi | Bagian 6 |
| DoD 4 — build/runtime pemilik; bila tidak dijalankan ditulis `NOT RUN` | Terpenuhi | Bagian 6 |
| DoD 5 — cek salah-prop | Terpenuhi | Bagian 6 |
| DoD 7 — laporan, roadmap, status `soap.md` | Terpenuhi | Laporan ini; `frontend-roadmap-v2.md`; `requirement-traceability-v2.md` bagian 17; `soap.md` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan ESLint `react-hooks/purity` (`Date.now()` saat render di Riwayat SOAP) diperbaiki dengan menghitung batas waktu di handler penyaring |
| Masalah yang diketahui | Pencarian cepat tidak menandai diagnosa yang dilarang menjadi Utama sebelum dipilih; layar memberi tahu sesudah dipilih |
| Dependency backend | `BE-RWI-142` ✅ source. Tanpa backend baru, pencarian tetap jalan tetapi urutannya per kode dan nama obat tidak tampil |
| Perubahan sampingan | `src/utils/icdData.jsx` dihapus, sesuai isi task |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi |
| Status Git | Lihat laporan `FE-RWI-141` bagian 8 (berkas SOAP dipakai bersama keempat task frontend) |
| Langkah berikutnya | Pemilik menjalankan `npm run build`, lalu menguji skenario 1 dan 3 `soap.md` di peramban |
