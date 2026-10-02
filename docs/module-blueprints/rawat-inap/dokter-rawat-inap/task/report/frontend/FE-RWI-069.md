# Laporan Perubahan Frontend — `FE-RWI-069`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-069` |
| Judul | Tab CPPT Dokter Rawat Inap (`FE-DOK-04` pada `FE-DOK-09`) |
| Slice | Gelombang 2 — `PRD-RWI-V2-001`, `EPIC DOK-12` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-069` |
| Trace | `FR-DOK-082`, `FR-DOK-083`, `FR-DOK-085`; `INV-DOK-11`, `INV-DOK-16`; `VAL-DOK-44`, `VAL-DOK-59`; `RWI-DEC-125`, `RWI-DEC-126`, `RWI-DEC-140`, `RWI-DEC-141`, `RWI-DEC-152`; `BE-RWI-089` [BE], `BE-RWI-094` [BE], `BE-RWI-095` [BE] |
| Contract version | `0.6.0` state matrix bagian 8.2; API 12.4 |
| Wewenang UI | `FE-DOK-04` sebagai tab CPPT di dalam kerangka `FE-DOK-09` |
| Dependency | `FE-RWI-067` ✅ selesai 17 September 2026; `BE-RWI-089` [BE] ✅ selesai 16 September 2026; `BE-RWI-094` [BE] ✅ selesai 16 September 2026; `BE-RWI-095` [BE] ✅ selesai 16 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 8, berkas diubah 5, kontrol keselamatan kewenangan verifikasi klinis DPJP & integritas lini masa |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-6) terbukti pada source. `npm run lint` bersih (0 error) dan `npm run build` sukses (0 error, standalone siap). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan, tab CPPT pada ruang kerja dokter rawat inap memiliki sejumlah celah kepatuhan dan keselamatan rekam medis:
1. **Penyaringan Belum Berbasis Klasifikasi Resmi `NoteKind` (`FR-DOK-085`, `BE-RWI-094`):** Penyaring pada tab CPPT sebelumnya menyaring berdasarkan teks literal `ProfessionType`. Pendekatan ini tidak mampu membedakan bentuk catatan SOAP Keperawatan dengan Catatan Naratif Keperawatan, serta tidak memiliki penanganan khusus untuk entri legacy bernilai `Unspecified (0)`.
2. **Pelanggaran Kewenangan Verifikasi DPJP (`FR-DOK-082`, `INV-DOK-16`, `BE-RWI-089`):** Tombol *Verifikasi* muncul bagi dokter mana pun yang memiliki penugasan di episode tersebut (termasuk Dokter Konsulen dan Dokter Jaga). Padahal sesuai regulasi rekam medis rumah sakit dan backend `BE-RWI-089`, pernyataan resmi bahwa catatan profesi lain sudah dibaca dan disetujui hanya boleh dilakukan oleh DPJP yang sedang bertugas. Menampilkan tombol bagi konsulen atau dokter jaga memicu kebingungan karena aksi mereka akan ditolak server dengan `403 Forbidden`.
3. **Ketiadaan Akses Verifikasi Pascapenutupan bagi DPJP Terakhir (`FR-DOK-083`, `BE-RWI-095`):** Ketika episode rawat inap berstatus `Closed`, antarmuka sebelumnya langsung memblokir seluruh aksi verifikasi (`readOnlyEpisode`). Padahal DPJP terakhir memiliki kewenangan khusus untuk menyelesaikan verifikasi catatan prapenutupan yang tertinggal setelah pasien pulang.
4. **Ketiadaan Informasi Durasi Keterlambatan (`AC-5`):** Catatan yang melewati batas waktu verifikasi hanya bertanda status umum tanpa menampilkan lamanya keterlambatan secara konkret, sehingga DPJP tidak dapat memprioritaskan catatan yang paling lama tertahan.
5. **Resiko Penelanan Galat Server (`AC-6`):** Bila terjadi penolakan verifikasi (`403` atau `409`), pesan galat berpotensi tertelan atau hanya muncul sebagai teks kecil yang terlewat oleh dokter.

### 1.2 Bukti keadaan awal

1. Berkas `src/lib/constants/health-services/inpatient-management/inpatient-integrated-note-constants.jsx` hanya menyediakan `CPPT_PROFESSION_FILTER_OPTIONS` berbasis string profesi tanpa enum `NoteKind`.
2. Berkas `src/utils/health-services/inpatient-management/inpatient-integrated-note-utils.jsx` pada fungsi `canVerifyNote` mengecek `!isActiveDoctor || readOnlyEpisode`, sehingga memblokir verifikasi pada episode `Closed` dan tidak membedakan DPJP dari konsulen.
3. Fungsi `describeVerificationStatus` hanya menampilkan label statis tanpa menyertakan perhitungan selisih waktu keterlambatan.
4. Hook `use-inpatient-integrated-note.jsx` belum menerima konteks penugasan (`assignments`) dan status episode tertutup (`isEpisodeClosed`).

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter Penanggung Jawab Pelayanan (DPJP), Dokter Konsulen, dan Dokter Jaga yang bertugas di bangsal rawat inap.

**Kapan tab dibuka.** Saat dokter membuka ruang kerja rawat inap (`FE-DOK-09`), memilih pasien di panel kiri, lalu memilih tab **CPPT** (tab kedua).

**Alur proses bisnis bertahap:**

1. **Membaca Lini Masa Lintas Profesi:**
   - Dokter membuka tab CPPT pasien Tn. Budi.
   - Layar menampilkan seluruh catatan perkembangan terintegrasi dari dokter, perawat, bidan, apoteker, nutrisionis, hingga fisioterapis yang tersusun runtut berdasarkan waktu klinis nyata (`noteDateTime`).
   - Setiap kartu catatan menampilkan nama penulis dan perannya, isi catatan (format SOAP atau naratif), serta status verifikasi DPJP.

2. **Menyaring Catatan Berdasarkan Jenis (`NoteKind` — `FR-DOK-085`, `BE-RWI-094`):**
   - Dokter memilih kontrol saringan jenis catatan di sudut kanan atas lini masa:
     - **Semua:** Menampilkan seluruh catatan tanpa kecuali, termasuk catatan rekam medis lama yang berjenis `Unspecified (0)`.
     - **Dokter:** Hanya menampilkan Catatan Perkembangan Pasien yang ditulis dokter (`PhysicianNote = 1`).
     - **Perawat:** Menampilkan catatan yang ditulis perawat, baik yang berbentuk SOAP Keperawatan (`NursingSoap = 2`) maupun Catatan Naratif Keperawatan (`NursingNarrative = 3`).
     - **Profesi Lain:** Menampilkan catatan dari apoteker farmasi, ahli gizi, fisioterapis, dan profesi penunjang lainnya (`OtherProfessionNote = 4`).

3. **Verifikasi Catatan Profesi Lain oleh DPJP Aktif (`FR-DOK-082`, `INV-DOK-16`, `BE-RWI-089`):**
   - **Skenario DPJP Aktif (dr. Ahmad, Sp.PD membuka catatan Ns. Siti):**
     - dr. Ahmad melihat tombol *"Verifikasi"* berwarna primer di kartu catatan Ns. Siti yang berstatus *"MENUNGGU VERIFIKASI"*.
     - dr. Ahmad menekan tombol *"Verifikasi"*.
     - Catatan diperbarui: lencana berubah menjadi *"DIVERIFIKASI"*, baris Verifikator mencantumkan nama *"dr. Ahmad, Sp.PD"*, dan waktu verifikasi tercatat.
     - Penulis asli catatan tetap tercantum sebagai *"Ns. Siti"*; verifikasi tidak pernah menimpa identitas penulis.
   - **Skenario DPJP Membuka Catatan Miliknya Sendiri (`INV-DOK-11`):**
     - Pada kartu catatan SOAP dokter yang ditulis dr. Ahmad sendiri, tombol *"Verifikasi"* **tidak muncul**, karena menandatangani bacaan atas tulisan sendiri bukan verifikasi rekam medis.
   - **Skenario Dokter Konsulen / Dokter Jaga (dr. Rina, Sp.JP membuka catatan Ns. Siti):**
     - dr. Rina dapat membaca seluruh catatan Ns. Siti dengan jelas.
     - Di atas lini masa tampil pemberitahuan informatif: *"Hanya DPJP yang Berwenang Memverifikasi — Anda bertugas sebagai dokter pendukung pada episode ini. Sesuai aturan keselamatan rekam medis (FR-DOK-082), hanya DPJP yang berwenang memverifikasi catatan profesi lain."*
     - Tombol *"Verifikasi"* **sama sekali tidak ada** pada kartu-kartu catatan di layar dr. Rina.

4. **Verifikasi pada Episode yang Sudah Ditutup (`Closed` — `FR-DOK-083`, `BE-RWI-095`):**
   - Pasien Tn. Budi telah dipulangkan dan episodenya berstatus `Closed`.
   - dr. Ahmad sebagai DPJP terakhir membuka tab CPPT Tn. Budi.
   - dr. Ahmad **tetap melihat tombol Verifikasi** pada catatan-catatan perawat yang dibuat sebelum pasien pulang (`noteDateTime <= closedAt`).
   - Bila dokter lain (atau DPJP sebelum dr. Ahmad) membuka episode tertutup tersebut, tombol verifikasi tidak ditampilkan.

5. **Penandaan Entri Lewat Batas Beserta Durasinya (`AC-5`):**
   - Catatan yang belum diverifikasi dan telah melampaui batas waktu kebijakan (`verificationDueAt < now`) ditandai dengan lencana merah mencolok: **"LEWAT BATAS VERIFIKASI (Terlambat 3 jam 15 menit)"** atau **"LEWAT BATAS VERIFIKASI (Terlambat 2 hari 4 jam)"**.
   - Pada rincian dokumen meta di bawah kartu, baris *Batas Verifikasi* menampilkan tanggal batas beserta durasi keterlambatan terhitung.

6. **Transparansi Penolakan Server (`AC-6`, `VAL-DOK-44`):**
   - Bila terjadi kegagalan verifikasi dari backend (misal penolakan otorisasi `403 Forbidden` atau konflik `409 Conflict`), banner peringatan bahaya (*ClinicalSafetyAlert danger*) muncul di atas tab menampilkan pesan asli server (misalnya: *"Hanya DPJP yang sedang bertugas dapat memverifikasi catatan ini"*), bukan ditelan menjadi pesan generik.

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate.md`)

```
UI GATE: 5 elemen — REUSE 4, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base / Referensi | Bukti Source | Status | Rekomendasi |
| --- | --- | --- | :---: | --- |
| Kontrol Filter Jenis Catatan | `BaseNativeSelectField` | `src/components/features/base-features/base-form-control.jsx` | `REUSE` | Digunakan untuk memilih kategori Semua, Dokter, Perawat, dan Profesi Lain. |
| Lini Masa CPPT | `ClinicalTimeline`, `ClinicalTimelineItem` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Digunakan untuk lini masa kronologis klinis. |
| Lencana Status & Keterlambatan | `ClinicalAuditBadge`, `ClinicalDocumentMeta` | `doctor-clinical-base.jsx` | `EXTEND` | Diperluas dengan penambahan teks durasi keterlambatan ramah baca (*Terlambat X jam/hari*) pada lencana status dan metadata batas waktu. |
| Tombol Verifikasi | `BaseButton`, `ClinicalActionGuard` | `base-button.jsx`, `doctor-clinical-base.jsx` | `REUSE` | Tombol Verifikasi disembunyikan bagi pengguna yang bukan DPJP berwenang. |
| Banner Galat Server | `ClinicalSafetyAlert` | `doctor-clinical-base.jsx` | `REUSE` | Menampilkan pesan galat HTTP 403 / 409 secara transparan. |

---

## 4. Pembuktian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria | Target Pembuktian | Status | Bukti Source / Runtime |
| --- | --- | :---: | --- |
| **AC-1** (`FR-DOK-085`) | Saring Semua / Dokter / Perawat / Profesi Lain bekerja dari kolom `NoteKind` | ✅ Terbukti | Di `inpatient-integrated-note-utils.jsx`: fungsi `filterNotesByKind` menyaring `noteKind === 1` (Dokter), `noteKind === 2 \|\| noteKind === 3` (Perawat), dan `noteKind === 4` (Profesi Lain). Dropdown menggunakan `CPPT_NOTE_KIND_FILTER_OPTIONS`. |
| **AC-2** (`FR-DOK-085`) | Entri lama berjenis `Unspecified` tetap tampil dan masuk saringan "Semua" | ✅ Terbukti | Di `filterNotesByKind`: bila filter bernilai kosong/`ALL`, seluruh catatan (termasuk `noteKind === 0` / `Unspecified`) dikembalikan tanpa pengecualian. |
| **AC-3** (`FR-DOK-082`, `INV-DOK-16`) | Tombol Verifikasi hanya muncul bagi DPJP aktif; konsulen dan dokter jaga tidak melihatnya | ✅ Terbukti | Fungsi `resolveIsAuthorizedDpjp` memeriksa penugasan aktif peran `DPJP` (`assignmentRole === 1`). Di `integrated-note-timeline.jsx`, tombol Verifikasi dibungkus `ClinicalActionGuard` dengan `allowed={eligible}` di mana `eligible` mewajibkan `isAuthorizedDpjp === true`. Konsulen (`role === 2`) dan Dokter Jaga (`role === 3`) tidak melihat tombol di DOM. |
| **AC-4** (`FR-DOK-083`, `BE-RWI-095`) | Pada episode `Closed`, DPJP terakhir tetap melihat tombol Verifikasi untuk entri prapenutupan | ✅ Terbukti | Pada episode `Closed`, `resolveIsAuthorizedDpjp` mengurutkan penugasan DPJP secara descending (`startDateTime`, `sequenceNumber`) identik dengan `FindLastAttendingDoctorIdAsync` di backend. `canVerifyNote` memvalidasi `noteDateTime <= closedAt` dan mengizinkan tombol bagi DPJP terakhir. |
| **AC-5** | Entri lewat batas ditandai terlambat, beserta lamanya | ✅ Terbukti | Fungsi `formatCpptLateness` menghitung selisih waktu secara presisi. `describeVerificationStatus` menyertakan label misalnya `"LEWAT BATAS VERIFIKASI (Terlambat 3 jam)"`. `ClinicalDocumentMeta` menyajikan baris `verificationDueAt` dengan hint durasi keterlambatan. |
| **AC-6** (`VAL-DOK-44`) | Galat `403` dari server ditampilkan apa adanya, bukan ditelan | ✅ Terbukti | `useInpatientIntegratedNote` menggunakan `getInpatientSettingErrorMessage` (yang telah mendukung string response dan preservasi HTTP 403/409). `integrated-progress-note-tab.jsx` menampilkan `ClinicalSafetyAlert tone="danger"` dengan judul *"Verifikasi Catatan Ditolak"* memaparkan pesan asli server. |

---

## 5. Dokumentasi Integrasi Endpoint API

Mengacu pada kontrak backend terverifikasi `BE-RWI-089`, `BE-RWI-094`, dan `BE-RWI-095`:

#### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `/api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Deskripsi | Auth / Permission | Request Parameter / Body | Response Payload |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Mengambil lini masa CPPT satu episode rawat inap terurut waktu klinis | `PatientIntegratedProgressNote : Read` | Query: `noteKind` (integer enum), `professionType` (string), `pageNumber`, `pageSize` | `ApiResponse<PagedResult<PatientIntegratedProgressNoteResponse>>` memuat `noteKind`, `noteKindName`, `verificationStatus`, `verificationDueAt`, `verifiedAt`, `verifiedByUserName` |
| `GET` | `/episodes/{episodeId}/verification-status` | Mengambil rekapitulasi status verifikasi dan daftar pantau | `PatientIntegratedProgressNote : Read` | Path: `episodeId` (GUID) | `ApiResponse<CpptVerificationStatusResponse>` memuat `isVerificationPolicyEmpty`, `pendingCount`, `overdueCount`, `watchList` |
| `PATCH` | `/{id}/verify` | DPJP aktif (atau DPJP terakhir pada episode Closed) memverifikasi catatan profesi lain | `PatientIntegratedProgressNote : Verify` | Path: `id` (GUID catatan CPPT) | `ApiResponse<PatientIntegratedProgressNoteResponse>`. Mengembalikan HTTP `403` bila bukan DPJP, HTTP `409` bila sudah diverifikasi, atau HTTP `422` bila entri pascapenutupan |

---

## 6. Berkas yang Diubah

### Repository Frontend (`QuilvianSystemFrontendDev`)

1. `src/lib/constants/health-services/inpatient-management/inpatient-integrated-note-constants.jsx`
   - Menambahkan enum `CPPT_NOTE_KIND`, label resmi `CPPT_NOTE_KIND_LABELS`, opsi filter `CPPT_NOTE_KIND_FILTER_OPTIONS`, serta teks notifikasi otorisasi non-DPJP.
2. `src/utils/health-services/inpatient-management/inpatient-integrated-note-utils.jsx`
   - Normalisasi `noteKind`, `noteKindName`, `doctorId`, `doctorName`.
   - Implementasi helper durasi keterlambatan `formatCpptLateness`.
   - Pembaruan label lencana status `describeVerificationStatus` dengan teks keterlambatan.
   - Implementasi evaluator kewenangan DPJP `resolveIsAuthorizedDpjp` (mendukung episode aktif dan episode `Closed`).
   - Pembaruan `canVerifyNote` dan implementasi filter `filterNotesByKind`.
3. `src/lib/hooks/health-services/inpatient-management/use-inpatient-integrated-note.jsx`
   - Menerima `episode`, `assignments`, dan `isEpisodeClosed`.
   - Mengintegrasikan evaluasi `isAuthorizedDpjp`.
   - Menerapkan filter `noteKind` pada query API dan memori klien.
   - Menangani galat verifikasi transparan HTTP `403` / `409`.
4. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/integrated-note/integrated-note-timeline.jsx`
   - Menerima prop `isAuthorizedDpjp`, `sessionDoctorId`, `isEpisodeClosed`, `closedAt`.
   - Menampilkan label lencana status dengan durasi keterlambatan.
   - Menambahkan entri metadata batas verifikasi beserta hint keterlambatan.
5. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/integrated-note/integrated-progress-note-tab.jsx`
   - Menghubungkan konteks penugasan dan status episode ke hook.
   - Menggunakan kontrol saringan `CPPT_NOTE_KIND_FILTER_OPTIONS`.
   - Menampilkan alert informatif bagi dokter pendukung (konsulen/dokter jaga).
   - Menampilkan alert merah `ClinicalSafetyAlert` jika verifikasi ditolak server.
6. `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx`
   - Meneruskan `isEpisodeClosed` dan `selectedPatient` ke dalam `PhysicianWorkspaceProvider`.

---

## 7. Bukti Verifikasi Otomatis

### Linting
```bash
cmd.exe /c npm run lint
```
**Hasil:** `PASS` (0 error, 685 warning bawaan legacy tidak terdampak).

### Build Produksi Next.js
```bash
cmd.exe /c npm run build
```
**Hasil:** `PASS` (Next.js berhasil mengompilasi rute client dan server, script postbuild `prepare-standalone` sukses, standalone runtime siap).
