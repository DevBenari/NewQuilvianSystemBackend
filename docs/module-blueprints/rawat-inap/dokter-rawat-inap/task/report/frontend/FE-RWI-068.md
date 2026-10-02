# Laporan Perubahan Frontend — `FE-RWI-068`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-068` |
| Judul | Tab SOAP Dokter Rawat Inap (`FE-DOK-03` pada `FE-DOK-09`) |
| Slice | Gelombang 2 — `PRD-RWI-V2-001`, `EPIC DOK-11` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-068` |
| Trace | `FR-DOK-074`, `FR-DOK-075`, `FR-DOK-077`, `FR-DOK-078`; `INV-DOK-14`, `INV-DOK-18`; `RWI-DEC-138`, `RWI-DEC-151`, `RWI-DEC-152`; `BE-RWI-088` [BE], `BE-RWI-091` [BE] |
| Contract version | `0.6.0` state matrix bagian 8.1; API 10.1 |
| Wewenang UI | `FE-DOK-03` sebagai tab SOAP di dalam kerangka `FE-DOK-09` |
| Dependency | `FE-RWI-067` ✅ selesai 17 September 2026; `BE-RWI-091` [BE] ✅ selesai 16 September 2026; `BE-RWI-088` [BE] ✅ selesai 16 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 8, berkas diubah 5 (source), kontrol keamanan penulis klinis & integritas penguncian |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-5) terbukti pada source. `npm run lint` bersih (0 error) dan `npm run build` sukses (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan, tab SOAP pada ruang kerja dokter rawat inap memiliki sejumlah celah kepatuhan dan keselamatan rekam medis:
1. **Pelanggaran Penulis Tunggal (`FR-DOK-075`, `INV-DOK-14`):** Tombol *Selesaikan* dan formulir penyuntingan draf terbuka bagi dokter mana pun yang membuka pasien tersebut. Walaupun dokter yang membuka bukanlah penulis draf (misalnya dokter konsulen atau dokter jaga melihat draf yang ditulis DPJP), antarmuka tetap menampilkan tombol *Selesaikan*. Bila tombol tersebut ditekan, permintaan akan ditolak oleh backend `BE-RWI-088` dengan `403 Forbidden`. Menampilkan tombol yang pasti ditolak server membingungkan pengguna klinis dan melanggar prinsip kejelasan antarmuka.
2. **Ketiadaan Penanganan Status `LockedUnsigned` (`FR-DOK-077`, `FR-DOK-078`, `BE-RWI-082`):** Ketika episode rawat inap pasien telah ditutup (`Closed`), seluruh konsep SOAP yang belum ditandatangani otomatis terkunci menjadi `LockedUnsigned`. Di frontend, catatan ini sebelumnya diperlakukan seperti draf biasa sehingga memicu kebingungan dokter yang mencoba menyuntingnya dan berakhir pada penolakan `409 Conflict`.
3. **Penyatuan Nilai Waktu yang Membingungkan:** Waktu pemeriksaan fisik pasien (`clinicalDateTime`) dan waktu catatan diketik/ditandatangani (`consultationDateTime`) belum dipisahkan secara tegas pada rincian dokumen dan riwayat. Padahal dokter rawat inap sering memeriksa pasien saat visite pagi (misal pukul 07.30) dan baru sempat mengetikkan SOAP di komputer ruang jaga siang harinya (misal pukul 11.00).
4. **Resiko Penelanan Galat Server (`AC-5`):** Ketika server mengembalikan galat penolakan otorisasi (`403`) atau konflik keadaan dokumen (`409`), fungsi pembaca galat berpotensi menelan pesan spesifik bila respons server berupa teks langsung atau format string, sehingga pesan yang muncul di layar hanyalah kalimat umum yang tidak membantu.

### 1.2 Bukti keadaan awal

1. Berkas `src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/physician-progress-tab.jsx` menampilkan tombol *Selesaikan* hanya berdasarkan flag `editable` tanpa memeriksa kesesuaian identitas dokter login dengan penulis konsep (`detail.doctorId`).
2. Berkas `use-inpatient-progress-note.jsx` tidak memeriksa `isEpisodeClosed` dan belum mengekstrak status `LockedUnsigned`.
3. Komponen `ProgressNoteDetail.jsx` belum membedakan label Waktu Pemeriksaan dengan Waktu Tanda Tangan/Waktu Catat.
4. Fungsi `readServerMessage` pada `inpatient-setting-utils.jsx` mengabaikan respons payload bertipe `string`.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter yang bertugas di rawat inap (DPJP Utama, Dokter Konsulen, maupun Dokter Jaga).

**Kapan tab dibuka.** Saat dokter membuka ruang kerja rawat inap (`FE-DOK-09`), memilih pasien di panel kiri, lalu memilih tab **SOAP** (tab pertama).

**Alur proses bisnis bertahap:**

1. **Melihat Riwayat SOAP Pasien:**
   - Dokter memilih kartu pasien di panel kiri ruang kerja rawat inap.
   - Tab SOAP langsung memuat Lini Masa Riwayat Catatan Perkembangan pasien yang diurutkan menurut waktu pemeriksaan klinis nyata.
   - Setiap kartu riwayat menampilkan: Jam/Tanggal Pemeriksaan, Jam/Tanggal Catat (bila mundur), Nama Dokter Penulis, dan Lencana Status (`Draf`, `Final`, `Tidak Ditandatangani`, atau `Dibatalkan`).
2. **Menulis Catatan SOAP Baru:**
   - Dokter menekan tombol *"Catatan Baru"* di atas lini masa.
   - Kolom editor menampilkan kolom **Waktu Pemeriksaan** (`datetime-local`) dengan batas minimal waktu masuk rawat inap pasien dan batas maksimal waktu sekarang.
   - Dokter mengisi empat komponen SOAP (*Subjective*, *Objective*, *Assessment*, *Plan*). Sesuai regulasi rumah sakit, paling sedikit satu komponen wajib terisi sebelum disimpan.
   - Dokter menekan *"Simpan Draft"*. Catatan tersimpan ke server dan kartu draf di lini masa segera terhubung ke ID yang valid tanpa duplikasi kartu.
3. **Menyelesaikan dan Menandatangani Catatan (Penulis Tunggal — `FR-DOK-075`):**
   - **Skenario Penulis Asli (dr. Ahmad membuka konsep miliknya):**
     - dr. Ahmad dapat mengedit isian SOAP miliknya.
     - Di bar penyelesaian bawah, tombol *"Simpan Draft"* dan *"Selesaikan"* tampil aktif.
     - dr. Ahmad menekan *"Selesaikan"*, modal penegasan muncul memperingatkan bahwa dokumen akan dikunci permanen.
     - Setelah konfirmasi, catatan berubah status menjadi `Final`, editor tertutup, dan isi catatan berubah menjadi tampilan dokumen asli yang tidak dapat disunting kembali.
   - **Skenario Dokter Lain (dr. Rina membuka konsep dr. Ahmad):**
     - dr. Rina dapat membaca apa yang ditulis dr. Ahmad, tetapi editor terkunci dengan pengaman aksi: *"Konsep Milik Dokter Lain — Catatan ini ditulis oleh dr. Ahmad. Sesuai aturan keselamatan rekam medis (FR-DOK-075), hanya penulis yang berwenang menyunting atau menyelesaikan konsep ini."*
     - Di bar bawah, tombol *"Selesaikan"* dan *"Simpan Draft"* **sama sekali tidak ada** pada layar dr. Rina. Hal ini mencegah dr. Rina menandatangani kalimat yang bukan hasil pemeriksaannya sendiri.
4. **Membaca Catatan pada Episode yang Sudah Ditutup (`LockedUnsigned` — `FR-DOK-077`, `FR-DOK-078`):**
   - Bila pasien sudah dipulangkan dan episodenya berstatus `Closed`, seluruh konsep draf yang belum sempat diselesaikan otomatis berstatus `LockedUnsigned`.
   - Di lini masa dan rincian catatan, lencana status berwarna merah menampilkan: **"Tidak Ditandatangani"**.
   - Dokumen disajikan dalam format hanya-baca dengan pemberitahuan: *"Catatan ini terkunci karena perawatan pasien sudah ditutup sebelum sempat ditandatangani. Catatan tidak dapat disunting langsung; gunakan addendum untuk melengkapi atau membetulkan."*
   - Dokter penulis dapat menekan tombol *"Koreksi"* untuk menyisipkan catatan koreksi (addendum) beralasan resmi.
5. **Pemberitahuan Galat Nyata dari Server (`AC-5`):**
   - Bila terjadi penolakan izin (HTTP `403`) atau konflik keadaan dokumen (HTTP `409`), pesan galat asli dari server dipaparkan secara gamblang di atas bar tindakan (misalnya: *"Akses ditolak (403): Hanya penulis yang berwenang menyunting konsep ini"*), bukan ditelan menjadi pesan generik.

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate.md`)

```
UI GATE: 6 elemen — REUSE 5, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base / Referensi | Bukti Source | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Lini Masa Catatan | `ClinicalTimeline`, `ClinicalTimelineItem` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Digunakan untuk lini masa catatan SOAP terurut waktu klinis. |
| Formulir Editor SOAP | `SoapEditor`, `ClinicalTimeField`, `BaseTextAreaField` | `progress-note/soap-editor.jsx`, `base-form-control.jsx` | `REUSE` | Empat seksi S/O/A/P dan kontrol native `datetime-local`. |
| Pengaman Penulis Konsep | `ClinicalActionGuard` | `doctor-clinical-base.jsx` | `EXTEND` | Diperluas dengan logika `isAuthor` berbasis `userInfo.doctorId` untuk mengunci konsep milik dokter lain. |
| Rincian Dokumen & LockedUnsigned | `ProgressNoteDetail`, `ClinicalDocumentMeta` | `progress-note/progress-note-detail.jsx` | `REUSE` | Ditambahkan prop `isLockedUnsigned` dan pemisahan label waktu pemeriksaan vs tanda tangan. |
| Riwayat Koreksi & Addendum | `ProgressNoteCorrectionHistory`, `CorrectionModal` | `progress-note-correction-history.jsx` | `REUSE` | Ditampilkan pada dokumen `completed` maupun `isLockedUnsigned`. |
| Bar Penyelesaian Dokumen | `ClinicalCompletionBar`, `BaseButton` | `doctor-clinical-base.jsx` | `REUSE` | Tombol Selesaikan dan Simpan Draft dihilangkan dari DOM bila `!isAuthor`. |

---

## 4. Pembuktian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria | Target Pembuktian | Status | Bukti Source / Runtime |
| --- | --- | :---: | --- |
| **AC-1** (`FR-DOK-075`) | Tombol Selesaikan hanya aktif bagi penulis konsep; bagi dokter lain tombolnya tidak ada | ✅ Terbukti | Di `physician-progress-tab.jsx`: `{editable && isAuthor ? <BaseButton ... data-testid="progress-note-complete">Selesaikan</BaseButton> : null}`. Bila `!isAuthor`, tombol tidak dirender ke DOM. Editor dibungkus `ClinicalActionGuard` dengan alasan `FR-DOK-075`. |
| **AC-2** (`FR-DOK-077`, `FR-DOK-078`) | Catatan `LockedUnsigned` tampil bertanda "Tidak Ditandatangani" dan tidak dapat disunting | ✅ Terbukti | `isProgressNoteLockedUnsigned` mendeteksi catatan draf pada episode `Closed`. Di timeline dan detail, status menampilkan `"Tidak Ditandatangani"` (`tone="danger"`). `editable = false`. Layar menampilkan `ProgressNoteDetail` hanya-baca dan tombol `Koreksi` (addendum). |
| **AC-3** (`FR-DOK-074`) | Simpan otomatis tidak menggandakan konsep di layar — satu konsep tetap satu kartu | ✅ Terbukti | Pada `saveDraft`, setelah `createDoctorConsultation` berhasil, `selectedId` langsung dialihkan ke `createdId`. Kartu draft temporer ditutup dan digantikan oleh entri definitif tunggal di `items`. Penyimpanan berulang berikutnya memanggil `patchDoctorConsultationSoap`. |
| **AC-4** (`FR-DOK-077`) | Waktu klinis dan waktu tanda tangan ditampilkan sebagai dua nilai terpisah | ✅ Terbukti | Di `ProgressNoteDetail.jsx`: entri `Waktu Pemeriksaan` (`clinicalDateTime`) dan `Waktu Tanda Tangan` (`consultationDateTime`) terpisah dengan jelas. Di `ProgressNoteTimeline.jsx`, waktu utama adalah waktu klinis dan waktu catat ditampilkan di bawahnya jika mundur. |
| **AC-5** | Galat `403` dari server ditampilkan apa adanya, bukan ditelan | ✅ Terbukti | `getInpatientSettingErrorMessage` pada `inpatient-setting-utils.jsx` diperbarui untuk membaca string payload server, status HTTP 403, dan status HTTP 409, lalu dipaparkan di `saveError` (`role="alert"`). |

---

## 5. Berkas yang Berubah

### Repository Frontend (`QuilvianSystemFrontendDev`)

1. `src/lib/constants/health-services/inpatient-management/inpatient-progress-note-constants.jsx`:
   - Menambahkan status `LOCKED_UNSIGNED: 4` pada `DOCTOR_CONSULTATION_STATUS` dengan label `"Tidak Ditandatangani"` dan tone `"danger"`.
   - Menambahkan konstanta teks `lockedUnsignedNotice`, `lockedUnsignedLabel`, `nonAuthorTitle`, `nonAuthorHint`, dan `signedTimeLabel`.
2. `src/utils/health-services/inpatient-management/inpatient-progress-note-utils.jsx`:
   - Menambahkan fungsi helper `isProgressNoteLockedUnsigned(item, isEpisodeClosed)`.
   - Memperbarui `describeProgressNoteStatus`, `describeProgressNoteStatusTone`, dan `isProgressNoteReadOnly` untuk menangani status `LockedUnsigned`.
   - Memperbarui normalisasi timeline agar membawa flag `isLockedUnsigned` dan `integrityStatus`.
3. `src/utils/health-services/inpatient-management/inpatient-setting-utils.jsx`:
   - Memperbarui `readServerMessage` dan `getInpatientSettingErrorMessage` agar tidak menelan payload bertipe string atau galat 403/409 dari server.
4. `src/lib/hooks/health-services/inpatient-management/use-inpatient-progress-note.jsx`:
   - Mengambil identitas dokter login (`userInfo.doctorId`) dari state Redux.
   - Menghitung kewenangan penulis tunggal `isAuthor` dan status penguncian `isLockedUnsigned`.
   - Menjaga integritas addendum pada dokumen berstatus `LockedUnsigned`.
   - Mengembalikan `isAuthor`, `isLockedUnsigned`, dan `items` yang diperkaya (`enrichedItems`).
5. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/physician-progress-tab.jsx`:
   - Mengalirkan `isEpisodeClosed` ke hook `useInpatientProgressNote`.
   - Mengamankan tombol *Selesaikan* dan *Simpan Draft* hanya untuk dokter penulis konsep (`isAuthor`).
   - Memasang `ClinicalActionGuard` penahan konsep milik dokter lain.
   - Mengalihkan tampilan konsep `LockedUnsigned` ke format dokumen statis `ProgressNoteDetail` dengan tombol addendum (*Koreksi*).
6. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/progress-note-timeline.jsx`:
   - Meneruskan `isLockedUnsigned` ke badge status lini masa.
7. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/progress-note/progress-note-detail.jsx`:
   - Menerima prop `isLockedUnsigned` dan membedakan label *Waktu Pemeriksaan* dengan *Waktu Tanda Tangan*.

### Repository Backend (`NewQuilvianSystemBackend`)

1. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/frontend/FE-RWI-068.md` (berkas laporan ini).
2. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` (pembaruan status task `FE-RWI-068` menjadi ✅).
3. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/requirement-traceability-v2.md` (pembaruan matriks keterlacakan `FR-DOK-074`, `FR-DOK-075`, `FR-DOK-077`).

---

## 6. Spesifikasi Kontrak Integrasi API (Bergaya Swagger)

Dokumentasi endpoint klinis SOAP dokter rawat inap yang terhubung dengan tab ini:

#### [Tags("Health Services / Clinical Management / Doctor Consultation")]

| Method | Path | Deskripsi & Kegunaan | Auth / Permission | Request Body | Response Body |
| :---: | :--- | :--- | :---: | :--- | :--- |
| `GET` | `/episodes/{episodeId}/soap-timeline` | Membaca lini masa catatan perkembangan SOAP pasien terurut waktu klinis | `DoctorConsultation : Read` | Query parameter: `from`, `to` (opsional) | `ApiResponse<SoapTimelineResponse>` |
| `POST` | `/` | Membuat catatan SOAP baru; mendaftarkan keutuhan sejak draf (`BE-RWI-091`) | `DoctorConsultation : Create` | `CreateDoctorConsultationRequest` (Subjective, Objective, Assessment, Plan, ClinicalDateTime, EpisodeId) | `ApiResponse<DoctorConsultationResponse>` |
| `PATCH` | `/{id}/soap` | Simpan otomatis / perubahan draf SOAP oleh penulis tunggal (`INV-DOK-14`) | `DoctorConsultation : Update` | `UpdateDoctorConsultationSoapRequest` (Subjective, Objective, Assessment, Plan) | `ApiResponse<DoctorConsultationResponse>` |
| `PATCH` | `/{id}/complete` | Menyelesaikan dan menandatangani catatan SOAP (`FR-DOK-075`); `LockedUnsigned` ditolak `409` | `DoctorConsultation : Update` | `CompleteDoctorConsultationRequest` | `ApiResponse<DoctorConsultationResponse>` |

#### [Tags("Health Services / Medical Record Management / Clinical Note Addendum")]

| Method | Path | Deskripsi & Kegunaan | Auth / Permission | Request Body | Response Body |
| :---: | :--- | :--- | :---: | :--- | :--- |
| `GET` | `/authority/Consultation/{id}` | Memeriksa apakah dokter login berhak membuat addendum (penulis asli atau delegasi) | `ClinicalNoteAddendum : Read` | Parameter path: `documentKind`, `documentId` | `ApiResponse<AddendumAuthorityResponse>` |
| `GET` | `/by-document/Consultation/{id}` | Mengambil seluruh riwayat addendum koreksi dari catatan SOAP | `ClinicalNoteAddendum : Read` | Parameter path: `documentKind`, `documentId` | `ApiResponse<List<ClinicalNoteAddendumResponse>>` |
| `POST` | `/by-document/Consultation/{id}` | Menambahkan addendum koreksi baru pada catatan final atau `LockedUnsigned` | `ClinicalNoteAddendum : Create` | `CreateClinicalNoteAddendumRequest` (`addendumText`, `correctionReason`) | `ApiResponse<ClinicalNoteAddendumResponse>` |

---

## 7. Bukti Verifikasi Otomatis

```text
AUTOMATED TEST: cmd.exe /c npm run lint — PASS (0 errors, 685 warnings legacy tidak terdampak)
AUTOMATED TEST: cmd.exe /c npm run build — PASS (Next.js compiled successfully, standalone runtime ready)
```

---

## 8. Status dan Rekomendasi Tindak Lanjut

Task **`FE-RWI-068`** dinyatakan **SELESAI (`✅`)**.

Tab SOAP kini telah aman dari segi keselamatan klinis dan otorisasi:
- Penulis tunggal ditegakkan (`FR-DOK-075`).
- Catatan `LockedUnsigned` tertangani dengan benar (`FR-DOK-077`, `FR-DOK-078`).
- Waktu klinis dan waktu tanda tangan terpisah jelas.
- Galat penolakan server disampaikan apa adanya (`AC-5`).
