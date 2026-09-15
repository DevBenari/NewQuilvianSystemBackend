# Laporan Perubahan Frontend — `FE-IGD-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-024` |
| Judul | Isian Kesimpulan saat menyelesaikan periode observasi |
| Slice | `IGD-S04` · layar observasi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.6, kartu `FE-IGD-024` |
| Trace | `IGD-DEC-115`, `IGD-DEC-119`, `IGD-DEC-121`; `IGD-OQ-083` tetap terbuka; **coverage gap:** tanpa `FR-IGD-*` (dijejak ke keputusan) |
| Contract version | API `0.5.0` §5 dan validation `0.5.0` §8 (berkas `draft`, aturannya `approved` lewat keputusan di atas). Implementasi kontrak di source backend `rizkiG` `0aa42668` — `EmergencyObservationController.UpdateObservationStatus` |
| Wewenang UI | `DEV_DISCRETION` untuk bentuk isian, mengikuti pola aksi beralasan yang sudah ada di tab ini. Tata letak tab Observasi tidak diubah |
| Dependency | `BE-IGD-040` — **source implementation complete** (commit `9f464cf3`); build **Not Verified**, runtime **belum** diverifikasi. `IGD-DEC-121` ✅ |
| Klasifikasi | `LIGHT` — satu tab dan satu berkas konstanta; tanpa route, slice, komponen bersama, atau CSS baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): `emergency-assessment-observation-tab.jsx`, `emergency-assessment-constant.jsx`. Backend: hanya laporan ini, roadmap, dan traceability |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `f37e949ea` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `0aa42668` (branch `rizkiG`) |
| Tanggal | 15 September 2026 |
| Status | **Implementation complete.** Lint dan unit test dijalankan (hasil di bagian 6). Build **Not Verified**. **Runtime not verified** — belum diuji di peramban. Bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

**Aksi Selesaikan sebelum perubahan.** Tab Observasi menampilkan tombol *Selesaikan* pada periode
`Active` dan `Escalated`. Menekannya membuka `ConfirmModal` berisi judul dan kalimat konfirmasi
saja, lalu mengirim `PATCH .../observation-status` dengan `{ "observationStatus": 2 }`. Tidak ada
isian kesimpulan, dan kalimat konfirmasinya sendiri menyatakan *"kesimpulan observasi belum punya
jalan simpan dari layar ini"* — benar pada saat ditulis, karena backend dulu membuang `notes` untuk
`Completed`.

**Tiga temuan yang menentukan bentuk perubahan:**

1. Isian bawaan `ConfirmModal` (`requireReason`) **selalu wajib** — label bertanda `*`,
   `aria-required`, dan tombol konfirmasi mati selama kosong. Tidak dapat dipakai untuk kesimpulan
   yang opsional tanpa mengubah komponen bersama.
2. Galat aksi status **tidak tampil di modal**. Thunk menulis ke `observations.saveError`, dan
   satu-satunya pembacanya adalah kartu *Buka Periode Observasi* di bagian atas tab. Pesan `400`
   atau `409` tidak terlihat di tempat petugas menekan tombol.
3. Kolom *Kesimpulan* pada daftar periode sudah ada sejak `FE-IGD-021` dan membaca
   `completionSummary` dari backend — cukup dipakai ulang.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD pada layar pengkajian pasien, tab **Asuhan Keperawatan →
Observasi**.

1. Pada daftar **Periode Observasi**, petugas menekan **Selesaikan** pada periode yang sedang
   berjalan atau sudah dieskalasi.
2. Modal *"Selesaikan periode observasi?"* terbuka dengan kalimat konfirmasi dan isian
   **Kesimpulan** (4 baris). Di bawah isian tertulis *"Opsional — boleh dikosongkan. 0/1000
   karakter."*, angkanya bergerak saat petugas mengetik.
3. Petugas menulis kesimpulan — atau membiarkannya kosong — lalu menekan **Selesaikan**.
4. Selama permintaan berjalan, tombol menampilkan pemutar, isian dan tombol nonaktif, dan modal
   tidak dapat ditutup.
5. Bila berhasil, modal tertutup dan daftar periode dimuat ulang dari backend. Periode tampil
   berstatus **Selesai**, kolom **Selesai** berisi waktu penutupan, dan kolom **Kesimpulan** berisi
   kesimpulan yang tersimpan.

**Jalur tidak normal**

| Keadaan | Yang terjadi |
| --- | --- |
| Kesimpulan dikosongkan | Penyelesaian tetap dikirim; kesimpulan lama (bila ada) tidak terhapus |
| Backend menolak `400` / `409` | Modal **tetap terbuka**, isian **tetap utuh**, pesan backend tampil di dalam modal dengan kotak merah. Petugas memperbaiki lalu menekan Selesaikan lagi |
| Petugas menekan tombol dua kali cepat | Hanya satu `PATCH` terkirim |
| Petugas menutup modal | Isian dikosongkan; saat modal dibuka lagi mulai dari kosong |

*Contoh:* perawat Ani menulis *"Nyeri dada hilang setelah 2 jam, EKG ulang normal, siap
disposisi"* lalu menekan Selesaikan. Beberapa detik kemudian kalimat itu tampil pada kolom
Kesimpulan periode `OBS-…`, dan tetap tampil setelah halaman dimuat ulang karena dibaca dari
backend.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Frontend: `emergency-assessment-observation-tab.jsx`, `emergency-assessment-constant.jsx`
  (`OBSERVATION_STATUS_ACTIONS`), `emergency-assessment-slice.jsx` (`buildStatusThunk`,
  `updateObservationStatus`, `attachCreate`, `normalizeError`),
  `use-emergency-assessment-detail.jsx`, `base-features/confirm-modal.jsx`,
  `base-features/base-form-control.jsx` (`BaseTextAreaField`),
  `base-features/information-alert.jsx`,
  `view/health-services/medical-record-management/my-unsigned-notes/my-unsigned-notes-page.jsx`
  (preseden `ConfirmModal` dengan `children`), `emergency-assessment-transfer-tab.jsx`.
- Backend (read-only): `EmergencyObservationController.cs`, `EmergencyObservationDtos.cs`,
  `EmgObservationConfiguration.cs`, `EmergencyObservationStatus.cs`.
- Dokumen: `MODULE-STATUS.md`, `00-interview-decisions.md`, `roadmap/frontend-roadmap.md`,
  `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`,
  `task/report/backend/BE-IGD-040.md`.

### 3.2 Berkas yang berubah

Seluruhnya di repository frontend `QuilvianSystemFrontendDev`.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | Konstanta `OBSERVATION_CONCLUSION_MAX_LENGTH = 1000`. Penanda `collectsConclusion: true` pada kedua aksi *Selesaikan* (dari `Active` dan dari `Escalated`). Kalimat usang *"kesimpulan observasi belum punya jalan simpan"* dihapus dari konfirmasi. Komentar `OBSERVATION_STATUS_ACTIONS` diselaraskan dengan perilaku backend sesudah `BE-IGD-040` |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx` | State `conclusion` dan `actionError`; penjaga klik ganda `useRef`; `openAction`/`closeAction`; `runAction` mengirim `notes` untuk aksi `collectsConclusion`, menampilkan pesan backend saat gagal, dan memuat ulang daftar periode saat berhasil; badan `ConfirmModal` khusus aksi Selesaikan berisi kalimat konfirmasi, `BaseTextAreaField` *Kesimpulan*, dan `InformationAlert` galat |

Berkas lain yang tampil berubah di working tree — `emergency-assessment-detail-view.jsx`,
`emergency-assessment-patient-card.jsx`, dan `emergency-assessment-workspace.module.css` — **bukan**
perubahan task ini; sudah ada sebelum task dimulai dan tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

- Thunk `updateObservationStatus` yang sudah ada dipakai apa adanya. Slice, endpoint, dan service
  **tidak** diubah; tidak ada panggilan Axios baru.
- Muat ulang sesudah berhasil memakai `fetchObservations(context)` yang sudah dipakai tab ini —
  hanya bagian observasi, bukan seluruh workspace.
- Batas panjang disimpan di constants domain, bukan angka lepas di view.
- Nol komponen bersama, nol CSS global, dan nol CSS module yang diubah. Kelas yang dipakai di badan
  modal adalah milik `ConfirmModal` (`region-confirm-message`) dan utilitas Bootstrap
  (`mt-3`, `text-start`) yang juga dipakai `ConfirmModal` untuk isian bawaannya.

**Gerbang base component** — `UI GATE: 4 elemen — REUSE 3, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Modal konfirmasi Selesaikan | `ConfirmModal` | `base-features/confirm-modal.jsx`, sudah dipakai tab ini | REUSE | Satu modal untuk seluruh aksi |
| Isian Kesimpulan | `BaseTextAreaField` | `base-form-control.jsx`: `maxLength` lewat `field`, `description` dipakai sebagai penghitung | REUSE | Tanpa `required` |
| Pesan galat backend | `InformationAlert` | `base-features/information-alert.jsx` | REUSE | `variant="danger"`, kosong berarti tidak dirender |
| Susunan pesan + isian + galat | `ConfirmModal` `children` | preseden `my-unsigned-notes-page.jsx` | COMPOSE | Opsi A |

Pilihan yang disajikan: **A** `children` pada `ConfirmModal`, hanya untuk aksi Selesaikan
(rekomendasi, dipilih); **B** prop baru `reasonOptional` pada `ConfirmModal` — mengubah komponen
bersama yang dipakai puluhan layar; **C** modal baru khusus observasi — duplikasi pola modal.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (mengirim) | Pemutar pada tombol Selesaikan; isian, tombol, dan penutup modal nonaktif |
| Kosong | Isian kosong diterima; penghitung *"0/1000 karakter"* |
| Gagal | Kotak merah berisi pesan backend, mis. *"Catatan paling banyak 1000 karakter."* atau *"Status kunjungan tidak dapat berubah dari Disposed ke AwaitingDisposition."*; modal tetap terbuka |
| Tanpa hak akses | Pesan backend `403` tampil pada kotak yang sama |
| Berhasil | Modal tertutup; daftar periode dimuat ulang dengan status, waktu selesai, dan kesimpulan dari backend |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Observation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-observations`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/observation-status` | Menyelesaikan periode beserta kesimpulan opsional | `EmergencyObservation : Update` |
| `GET` | `/` | Memuat ulang daftar periode sesudah berhasil (`emergencyVisitId`) | `EmergencyObservation : Read` |

**Payload aktual** — ditangkap dari thunk existing lewat pemeriksaan sementara (bagian 6):

| Aksi | Isian | Badan JSON yang dikirim |
| --- | --- | --- |
| Selesaikan | `"  Nyeri dada hilang, EKG normal  "` | `{"observationStatus":2,"notes":"Nyeri dada hilang, EKG normal"}` |
| Selesaikan | kosong / spasi saja | `{"observationStatus":2}` — `notes` tidak dikirim |
| Eskalasi | alasan *"Saturasi turun"* | `{"observationStatus":3,"rejectionReason":"Saturasi turun","notes":"Saturasi turun"}` — **sama dengan sebelum task** |
| Batalkan | — | `{"observationStatus":4}` — **sama dengan sebelum task** |

Kesimpulan kosong **tidak dikirim** (bukan `null` atau string kosong). Thunk existing sudah membuang
nilai kosong lewat `notes || reason || undefined`; backend memperlakukan `notes` yang tidak ada
sebagai "pertahankan kesimpulan lama" (`catatan ?? entity.CompletionSummary`).

**Riwayat kesimpulan.** Kolom *Kesimpulan* pada daftar periode membaca `item.completionSummary`
dari `GET /` (`EmergencyObservationResponse.CompletionSummary`), bukan dari isian formulir. Nilai
isian dikosongkan begitu permintaan berhasil, sehingga yang tampil hanyalah nilai yang tersimpan.

**Contract mismatch:** `NONE`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet` pada 2 berkas yang berubah | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run lint:errors` (`eslint . --quiet`) | exit 0, nol error (seluruh repository, termasuk perubahan owner yang sudah ada di working tree) | `PASS` | Keluaran perintah |
| `npm run test:unit` | Gagal sebelum test berjalan: *"Could not find '...\tests\unit\**\*.test.mjs'"* — Node `v20.20.2` tidak mengekspansi glob pada `--test` | `EXISTING / ENVIRONMENT ISSUE` | Sama seperti `FE-IGD-023`; masalah skrip, tidak berkaitan dengan perubahan |
| `node --import ./tests/helpers/register.mjs --test tests/unit` (perintah DoD roadmap) | **852 test, 852 lulus, 0 gagal** | `PASS` | Keluaran perintah. Tidak ada test existing yang menyentuh tab Observasi |
| Pemeriksaan **sementara** di scratchpad sesi — **bukan test resmi**, tidak masuk repository | 6/6 lulus: payload keempat baris tabel bagian 5 persis; `collectsConclusion` hanya pada kedua aksi Selesaikan; Batalkan tanpa isian; Eskalasi tetap `requiresReason`; batas konstanta 1000; pesan `400` dan `409` backend diteruskan thunk apa adanya | `PASS` | Keluaran skrip; `InstanceAxios.patch` di-stub |
| Grep anti-regresi pada baris yang ditambahkan | Nol `<button`/`.btn`, nol `<table`, nol `fw-*`/`fs-*`, nol `style={{`, nol warna literal | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan agent — diserahkan kepada owner | `NOT RUN` — **Build = Not Verified** | — |
| Uji di peramban: selesaikan periode dengan/tanpa kesimpulan, galat `400`/`409` | Tidak dijalankan | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` — agent tidak menjalankan peramban dengan sesi petugas, dan runtime
`BE-IGD-040` sendiri belum diverifikasi.

**Perintah untuk owner** (dari folder `QuilvianSystemFrontendDev`): `npm run build`.

**Tidak dijalankan:** build, uji peramban, `test:e2e`, `test:uat`. Tidak ada test baru yang
ditambahkan ke repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Memilih aksi **Selesaikan** menampilkan isian **Kesimpulan**, paling banyak 1000 karakter | Terpenuhi | `collectsConclusion: true` pada kedua aksi Selesaikan; badan modal merender `BaseTextAreaField` dengan `CONCLUSION_FIELD.maxLength = OBSERVATION_CONCLUSION_MAX_LENGTH` (1000) dan penghitung karakter |
| 2. Isian **opsional**: kosong pun penyelesaian tetap dapat dikirim | Terpenuhi | Tanpa `required`; `requireReason` hanya aktif untuk aksi `requiresReason`; payload kosong `{"observationStatus":2}` |
| 3. Setelah berhasil, riwayat observasi menampilkan kesimpulan tanpa memuat ulang halaman | Terpenuhi pada source | `runAction` memanggil `reload()` → `fetchObservations(context)`; kolom *Kesimpulan* membaca `item.completionSummary`. Belum dibuktikan di peramban |
| 4. Aksi **Batalkan** tidak berubah — tanpa isian alasan (`IGD-OQ-083`) | Terpenuhi | Aksi Batalkan tanpa `collectsConclusion`/`requiresReason`; modal memakai jalur `message` bawaan; payload `{"observationStatus":4}` |
| 5. Aksi eskalasi tidak ikut diubah | Terpenuhi | Konstanta eskalasi tidak disentuh; modal memakai jalur `requireReason` bawaan; payload identik dengan sebelum task |
| 6. Pesan `400` *"Catatan paling banyak 1000 karakter."* dan `409` dari backend tampil apa adanya | Terpenuhi pada source | `setActionError(result.payload?.message …)` → `InformationAlert` di dalam modal; `normalizeError` membaca `response.data.message` (`ApiResponse.Fail`). Belum dibuktikan terhadap backend yang berjalan |

**Definition of Done R3.6**

| Butir | Status |
| --- | --- |
| Acceptance criteria terpetakan ke source | Ya — 6/6 |
| `npm run lint:errors` dijalankan, hasil dicatat | Ya |
| `node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan, hasil dicatat | Ya — 852/852 |
| Perintah `npm run build` diberikan kepada Rizki | Ya |
| Catatan uji layar ditulis apa adanya | Ya — `NOT FEASIBLE` beserta alasannya |
| Laporan tracked | Ya — berkas ini |
| Roadmap dan traceability diperbarui | Ya |
| Nol komponen bersama dan CSS global diubah | Ya |
| Tanpa UAT PASS | Ya |

**Pembedaan status:** Requirement approved = Ya (keputusan) · Delivery planned = Ya ·
**Implementation complete = Ya** · Build = **Not Verified** · **Runtime verified = Belum**.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Git mencatat *"LF will be replaced by CRLF"* — perilaku `core.autocrlf`, bukan perubahan isi |
| Masalah yang diketahui | (1) Galat aksi **Eskalasi** dan **Batalkan** masih tidak tampil di dalam modal — hanya pada kartu *Buka Periode Observasi* lewat `saveError`, perilaku lama; sengaja tidak diubah karena kedua aksi di luar lingkup. (2) Galat aksi Selesaikan juga ikut tampil pada kartu *Buka Periode Observasi*, karena thunk tetap menulis `observations.saveError` — duplikasi tampilan, perilaku reducer lama. (3) Catatan > 2000 karakter ditolak backend oleh validasi model dengan pesan bawaan framework (delta `BE-IGD-040`); tidak dapat dicapai dari layar ini karena isian dibatasi 1000. (4) `npm run test:unit` gagal oleh glob di Node 20 |
| Dependency backend | `BE-IGD-040` source selesai, build dan runtime belum diverifikasi. Bila backend yang berjalan belum memuat `9f464cf3`, kesimpulan akan dibuang backend tanpa galat — perilaku lama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git frontend | ` M .../emergency-assessment-observation-tab.jsx`, ` M .../emergency-assessment-constant.jsx` (task ini); ` M .../emergency-assessment-patient-card.jsx`, ` M .../emergency-assessment-detail-view.jsx`, `?? .../emergency-assessment-workspace.module.css` (sudah ada sebelum task, bukan milik task ini) |
| Langkah berikutnya | Owner menjalankan build backend dan `npm run build`, lalu di peramban: selesaikan satu periode dengan kesimpulan, satu tanpa kesimpulan, dan satu pada kunjungan `Disposed` untuk melihat pesan `409` |
