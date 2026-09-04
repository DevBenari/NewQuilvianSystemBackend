# Laporan Perubahan Frontend — `FE-RWI-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-041` |
| Judul | Pengaturan Rawat Inap mempunyai shell dan form yang operasional |
| Slice | `F12 — Repair layar existing` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), task `FE-RWI-041` |
| Trace | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-12`; skema §17 dan §24.1; `FR-RI-142` s.d. `FR-RI-144`; `RWI-DEC-063` |
| Contract version | API Master Data `0.4.0`; dua endpoint inpatient settings (`GET /`, `PUT /{id}`) berstatus tersedia. Source backend pada commit `44fd6ccba` menjadi bukti runtime as-is. Hak akses `InpatientSetting : Read/Update` |
| Wewenang UI | Urutan field, satuan, dan audit mengikuti skema §17 yang disetujui; bentuk form dan penataan `DEV_DISCRETION` |
| Dependency | `BE-RWI-002` (pengisian baris master `DEFAULT`); `BE-RWI-005` (endpoint controller pengaturan) ✅ tersedia; `FE-RWI-033` (pemindahan menu ke Master Data) ✅ selesai; `RWI-UI-GAP-007` (keterisian master environment) |
| Klasifikasi | `MEDIUM` — empat berkas frontend dimodifikasi dan satu suite unit test diperbarui; tanpa perubahan backend, skema database, atau penanaman data tiruan |
| Task mode | `FRONTEND` — source backend strict read-only; wewenang lintas repository hanya berkas laporan ini, roadmap, dan traceability modul Rawat Inap |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; folder laporan, roadmap, dan traceability Rawat Inap pada `NewQuilvianSystemBackend` |
| Model | Claude Opus 4.6 (Thinking) / Gemini 3.7 Flash |
| Commit frontend saat dikerjakan | `e313437e1a33fa8580a6b83d7c16f359f92acdab` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `44fd6ccbaa9b403df1bff2f16729eedf7a0ea32a` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Ketujuh acceptance criteria terpenuhi. `npm.cmd run lint` lulus dengan `0 errors` serta 573 warning baseline; `npm.cmd run build` berhasil (`✓ Compiled successfully`). Unit test `tests/unit/inpatient-setting.test.mjs` lulus 12/12 tes (`0 fails`). Tidak ada data awal yang ditanam di frontend. Bukti runtime baris master `DEFAULT` tetap menunggu penutupan `RWI-UI-GAP-007` pada database environment target |

---

## 1. Keadaan yang ditemukan di awal

Pada penelusuran awal dan bukti runtime pemilik tertanggal 28 Agustus 2026, halaman **Pengaturan Rawat Inap** (`FE-INP-12`) berada dalam status **`REPAIR`**:
1. Screenshot runtime menampilkan pesan peringatan data belum tersedia dengan tombol **Muat ulang** di dalam kontainer sederhana yang terputus dari kerangka shell halaman standar (`Health Services / Master Data`).
2. Saat endpoint `GET /inpatient-settings` mengembalikan respon `404 Not Found` (karena baris master `DEFAULT` belum di-seed di environment target), halaman kehilangan struktur Hero, navigasi breadcrumb, dan tombol kembali ke Master Data, sehingga tampak sebagai area kosong yang tidak rapi.
3. Tombol **Simpan Pengaturan** belum dilengkapi pengecekan keadaan form yang telah diubah (*dirty check*) dan validitas klien (*client validity*), sehingga berisiko mengirim permintaan pembaruan tanpa adanya perubahan data riil.
4. Informasi jejak audit (*audit trail*) — siapa yang terakhir mengubah pengaturan dan kapan perubahannya terjadi — belum ditampilkan secara eksplisit di form sesuai spesifikasi skema §17.2.
5. Tombol pembatalan/kembali pada form belum mengarahkan pengguna secara tepat ke induk navigasi Master Data (`/health-services/master-data`).

Meskipun penyediaan baris master `DEFAULT` bergantung pada penutupan `RWI-UI-GAP-007` oleh tim database/backend melalui seeder `BE-RWI-002`, frontend **wajib menyajikan shell halaman yang utuh, informatif, dan actionable saat 404, serta form yang operasional, aman, dan mematuhi kontrak saat data master tersedia**, tanpa mengarang endpoint `POST` atau menanam nilai bawaan tiruan di sisi klien.

---

## 2. Proses bisnis dari sisi pengguna

Halaman **Pengaturan Rawat Inap** digunakan oleh Admin Master Data dan Manajemen Rumah Sakit untuk mengonfigurasi parameter operasional global modul Rawat Inap, seperti batas waktu penguncian pemesanan tempat tidur, ambang kadaluarsa admisi draft, target waktu pelayanan klinis, ambang daftar pantau, dan awalan penomoran episode.

### 2.1 Alur Penggunaan Normal (Data Master Tersedia)

1. **Membuka Halaman Pengaturan**: Admin membuka menu `Pelayanan Kesehatan → Master Data → Pengaturan Rawat Inap` pada URL `/health-services/inpatient-management/settings`.
2. **Pembacaan Data**: Sistem secara otomatis memanggil `GET /inpatient-settings`. Seluruh 9 parameter kontrak dibaca dan ditampilkan ke dalam formulir terstruktur beserta satuan dan deskripsi jelasnya:
   - **Nama Pengaturan**: teks identifikasi baris pengaturan (maks. 150 karakter).
   - **Lama Kunci Pemesanan Tempat Tidur**: angka waktu penguncian tempat tidur (1 s.d. 1440 menit).
   - **Umur Episode Draft Sebelum Gugur**: batas waktu draft admisi (1 s.d. 720 jam).
   - **Target Pengkajian Awal**: target penyelesaian asesmen awal pasien (1 s.d. 720 jam).
   - **Target Verifikasi Catatan Perkembangan**: target verifikasi CPPT pasien (1 s.d. 720 jam).
   - **Ambang Episode Tertahan Menunggu Penutupan**: batas waktu episode masuk daftar pantau (1 s.d. 720 jam).
   - **Awalan Nomor Episode**: awalan kode registrasi episode rawat inap (contoh: `RI`, maks. 20 karakter).
   - **Pengaturan Aktif**: sakelar status keaktifan pengaturan.
   - **Catatan**: catatan administratif opsional (maks. 1000 karakter).
3. **Penyajian Jejak Audit**: Bagian bawah formulir menampilkan ringkasan riwayat audit:
   - Jika sudah pernah diubah: *"Diubah terakhir oleh [Nama Admin] pada [Tanggal & Waktu]"*.
   - Jika belum pernah diubah: *"Nilai bawaan sistem (dibuat pada [Tanggal & Waktu])"*.
4. **Mengubah Nilai**:
   - Admin mengubah salah satu atau beberapa nilai pada formulir (misalnya mengubah lama kunci pemesanan dari 120 menit menjadi 90 menit).
   - Tombol **Simpan Pengaturan** yang semula nonaktif (*disabled*) otomatis menjadi aktif begitu ada perubahan data yang valid.
5. **Menyimpan Perubahan**:
   - Admin menekan **Simpan Pengaturan**.
   - Sistem mengirim `PUT /inpatient-settings/{id}` dengan muatan data yang diperbarui.
   - Respon berhasil diterima, toast notifikasi *"Pengaturan tersimpan — Nilai baru berlaku pada pembacaan berikutnya"* muncul, formulir diperbarui dengan respon terkini, dan tombol simpan kembali nonaktif sampai ada perubahan berikutnya.

### 2.2 Skenario Master Belum Terisi (Respon 404 / Dependency GAP 007)

1. Ketika backend menjawab `404 Not Found`, halaman tidak rusak dan tidak berubah menjadi area kosong.
2. Shell halaman tetap utuh dengan:
   - Hero header bertuliskan *"Health Services / Master Data — Pengaturan Rawat Inap"*.
   - Tombol aksi Hero: **Muat ulang** (untuk mencoba menarik data kembali) dan **Kembali ke Master Data** (untuk kembali ke daftar master data).
   - Kotak peringatan kuning (*InformationAlert warning*) yang secara transparan menjelaskan blocker: *"Pengaturan Rawat Inap belum terisi di lingkungan ini. Baris master berkode DEFAULT diperlukan oleh modul Rawat Inap untuk membaca parameter batas waktu dan penomoran. Silakan hubungi Administrator Master Data untuk menjalankan seeder atau menyiapkan data awal."*
3. Layar tidak menawarkan tombol "Tambah / Create" dan tidak menyediakan jalur pengiriman `POST` karena kontrak API menetapkan konfigurasi ini sebagai baris tunggal berkode `DEFAULT`.

### 2.3 Skenario Gagal Simpan & Retensi Isian

- **Penolakan Validasi Server (400 / 422)**: Jika server menolak perubahan (contoh: menonaktifkan baris aktif satu-satunya), alert merah menampilkan pesan asli server apa adanya. Isian yang sudah diketik petugas **tidak dihapus/tidak di-reset**, sehingga pengguna dapat langsung menyesuaikan data tanpa mengetik ulang dari awal.
- **Validasi Klien**: Jika pengguna mengosongkan field wajib atau memasukkan angka di luar rentang (misal: 0 menit atau 1500 menit), pesan error spesifik muncul di bawah field dan tombol simpan tetap nonaktif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: `05-skema-tampilan.md` §17 dan §24.1; `roadmap/frontend-roadmap.md`; `roadmap/requirement-traceability.md`.
- Backend read-only: `InpatientSettingController.cs`, `InpatientSettingDtos.cs`, `InpatientSettingService.cs` pada commit `44fd6ccba`.
- Frontend existing: `inpatient-setting-view.jsx`, `use-inpatient-setting.jsx`, `inpatient-setting-constants.jsx`, `inpatient-setting-utils.jsx`, `inpatient-setting.service.js`, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-setting-view.jsx` | Memperbarui tata letak halaman dengan shell modern (`administrator-region-page`), mempertahankan Hero dan tombol aksi (**Muat ulang** dan **Kembali ke Master Data**) pada keadaan 404, menyajikan jejak audit pada footer form, serta menghubungkan gating tombol simpan (`canSubmit`) |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-setting.jsx` | Menambahkan perhitungan state `isDirty` (deteksi perubahan form terhadap server data), `isValid` (validitas klien), `canSubmit` (gating tombol submit), `auditText` (teks audit terformat), serta memperbarui navigasi pembatalan ke rute Master Data |
| `src/lib/constants/health-services/inpatient-management/inpatient-setting-constants.jsx` | Menambahkan konstanta rute Master Data `HEALTH_SERVICES_MASTER_DATA_ROUTE` dan pesan bawaan not found `INPATIENT_SETTING_NOT_FOUND_MESSAGE` |
| `src/utils/health-services/inpatient-management/inpatient-setting-utils.jsx` | Menambahkan fungsi pembantu murni `isSettingFormChanged`, `isSettingFormValid`, `formatDateTime`, dan `formatAuditDisplay` |
| `tests/unit/inpatient-setting.test.mjs` | Memperluas suite unit test menjadi 12 pengujian yang mencakup deteksi perubahan form, validasi, pemformatan audit dan datetime, ketiadaan hardcoded default values, ketiadaan POST/Create, dan integritas shell pada 404 |

### 3.3 Kepatuhan Arsitektur Frontend & Base Component Decision Gate

Pola komunikasi mengikuti arsitektur baku: `View → Hook → Service → InpatientApiService → Redux Slice`.

| Kebutuhan UI | Kandidat Base Component | Keputusan Gate | Rationale & Implementasi |
| --- | --- | --- | --- |
| Shell halaman master | `BaseEditorView` + shell token Administrator | `REUSE` | Memakai CSS module token standar tanpa styling literal |
| Header Halaman | `Hero` | `REUSE` | Eyebrow, judul halaman, deskripsi, dan tombol aksi (Muat Ulang / Kembali) |
| Alert informasi & error | `InformationAlert` | `REUSE` | Peringatan master belum siap (warning pada 404) dan pesan error simpan (danger) |
| Formulir Editor | `BaseEditorForm` (di dalam `BaseEditorView`) | `REUSE` | Menampilkan 9 input/switch/textarea sesuai kontrak backend |
| Panel Samping / Preview | `BaseEditorPreview` | `REUSE` | Panel panduan master data dengan glow aksen standar |
| Jejak Audit | Footer terstruktur di bawah form | `REUSE` / `COMPOSE` | Teks terformat id-ID dengan batas pemisah token standar |
| Tombol & Aksi | `BaseButton` | `REUSE` | Tombol Simpan Pengaturan (primary dengan state loading dan disabled gating) dan Kembali (secondary) |
| Notifikasi Toast | `ToastStack` | `REUSE` | Notifikasi feedback penyimpanan berhasil |
| Gerbang Hak Akses | `AccessDeniedGate` | `REUSE` | Melindungi halaman dari error 401/403 |

`UI GATE: 9 elemen — REUSE 9, COMPOSE 0, EXTEND 0, WRAP 0, NEW 0.`

Seluruh komponen visual memakai base component resmi. Tidak ada komponen baru yang dibuat di luar katalog, tidak ada `!important`, dan tidak ada CSS hardcoded.

---

## 4. State yang ditangani di layar

| State Layar | Tampilan & Perilaku Pengguna |
| --- | --- |
| **Loading Awal** | Indikator loading aktif pada form/tombol selama proses `GET /inpatient-settings` berlangsung |
| **Data Tersedia (Bersih / Unchanged)** | Seluruh 9 parameter dan teks jejak audit tampil lengkap. Tombol **Simpan Pengaturan** dalam keadaan nonaktif (*disabled*) karena belum ada perubahan data |
| **Data Diedit & Valid (Dirty & Valid)** | Pengguna mengubah isian dan seluruh field memenuhi rentang nilai valid. Tombol **Simpan Pengaturan** otomatis menjadi aktif (*enabled*) |
| **Data Diedit tapi Tidak Valid (Invalid)** | Field yang melanggar batasan menampilkan pesan error spesifik (misal: di luar rentang 1–1440 menit), dan tombol **Simpan Pengaturan** dinonaktifkan |
| **Menyimpan Perubahan (Submitting)** | Tombol **Simpan Pengaturan** menampilkan spinner loading dengan label *"Menyimpan..."* dan input dinonaktifkan sementara |
| **Simpan Gagal (Server Rejection / 400)** | Alert bahaya menampilkan pesan penolakan asli server. Isian pengguna **tidak di-reset**, modal tidak ditutup, dan pengguna dapat langsung memperbaiki input |
| **Simpan Berhasil (200 OK)** | Toast hijau *"Pengaturan tersimpan"* muncul, nilai terbaru disinkronkan ke state, dan tombol simpan kembali nonaktif |
| **Master Belum Terisi (404 Not Found)** | Shell halaman tetap utuh dengan Hero header, alert warning menjelaskan baris `DEFAULT` belum di-seed (`RWI-UI-GAP-007`), serta tombol **Muat ulang** dan **Kembali ke Master Data** tersedia |
| **Akses Ditolak (401/403)** | `AccessDeniedGate` menampilkan antarmuka penolakan hak akses secara aman |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Deskripsi | Hak Akses | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/` | Mengambil pengaturan Rawat Inap yang berlaku | `InpatientSetting : Read` | – | `ApiResponse<InpatientSettingResponse>` |
| `PUT` | `/{id}` | Mengubah nilai pengaturan Rawat Inap | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` (`name`, `bedReservationMinutes`, `draftEpisodeExpiryHours`, `initialAssessmentTargetHours`, `progressNoteVerificationTargetHours`, `pendingClosureThresholdHours`, `episodeNumberPrefix`, `isActive`, `notes`) | `ApiResponse<InpatientSettingResponse>` |

> **Catatan Kontrak**: Endpoint `POST` sengaja tidak ada karena pengaturan Rawat Inap didesain sebagai baris tunggal berkode `DEFAULT`. Layar frontend sepenuhnya mematuhi prinsip ini dan tidak pernah memicu permintaan `POST`.

---

## 6. Verifikasi

| Skenario / Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm.cmd run lint` | Exit `0`; `0 errors`, 573 warning baseline | `PASS` | Log ESLint repository frontend |
| `npm.cmd run build` | Next.js build sukses; 245 route terkompilasi | `PASS` | `✓ Compiled successfully`; standalone runtime siap |
| `node --test tests/unit/inpatient-setting.test.mjs` | 12 test pass, 0 fail (157 ms) | `PASS` | Seluruh 12 kasus uji logika, deteksi perubahan, audit, dan batasan DTO lulus |
| Uji Anti-Regresi UI (Checklist & Grep) | Tidak ada warna literal, inline styles, font mentah, atau `!important` | `PASS` | Pemeriksaan statis source berkas task |
| Uji Gating Tombol Simpan | Tombol simpan hanya aktif saat `isDirty && isValid && !loading` | `PASS` | Diverifikasi pada logic hook dan unit test |
| Uji Retensi Form pada Penolakan Server | Form tidak di-reset saat server mengembalikan penolakan HTTP 400 | `PASS` | `use-inpatient-setting.jsx` catch block dan test unit |
| Uji Integritas Shell pada 404 | Shell Hero, tombol Muat Ulang, dan alert peringatan tetap utuh | `PASS` | Diverifikasi pada logic view dan test unit |

`AUTOMATED TEST: node --test tests/unit/inpatient-setting.test.mjs — PASS (12/12 tests)`

`MANUAL TEST: NOT FEASIBLE` — Pembuktian end-to-end terhadap database sesungguhnya memerlukan baris master `DEFAULT` yang aktif pada environment target (`RWI-UI-GAP-007`). Namun seluruh interaksi komponen, transisi state (dirty/valid/error/404), dan penanganan HTTP telah diverifikasi secara menyeluruh melalui pengujian statis dan unit test.

---

## 7. Acceptance Criteria dan Definition of Done

| Acceptance Criteria | Status | Bukti Implementasi |
| :--- | :--- | :--- |
| 1. Dengan baris `DEFAULT`, seluruh field kontrak dan audit tampil | ✅ Terpenuhi | 9 field formulir dirender lengkap dengan satuan/deskripsi, dan teks jejak audit ditampilkan pada footer formulir |
| 2. **Simpan Pengaturan** hanya aktif ketika form valid dan berubah | ✅ Terpenuhi | Hook menghitung `isDirty = isSettingFormChanged(form, item)` dan `isValid = isSettingFormValid(form)`. Tombol simpan memiliki `disabled={!canSubmit}` |
| 3. Error simpan mempertahankan isian | ✅ Terpenuhi | Blok `catch` pada `handleSubmit` hanya memanggil `setActionError` tanpa memanggil `setForm`, mempertahankan seluruh isian pengguna |
| 4. Pada 404, shell tetap utuh dan menyebut master environment belum diisi, menyediakan **Muat Ulang** serta navigasi kembali ke Master Data | ✅ Terpenuhi | View merender `Hero` dengan tombol **Muat ulang** dan **Kembali ke Master Data**, serta `InformationAlert` warning yang menjelaskan dependensi seed `DEFAULT` |
| 5. Layar tidak menawarkan Create dan tidak mengirim POST | ✅ Terpenuhi | Mode editor dikunci `update`, tidak ada tombol tambah/create, dan service hanya mengekspos `get()` dan `put()` |
| 6. Layar muncul tepat sekali di `Pelayanan Kesehatan → Master Data` | ✅ Terpenuhi | `menu-items.jsx` mendaftarkan *"Pengaturan Rawat Inap"* tepat satu kali di bawah menu Master Data pada path `/health-services/inpatient-management/settings` |
| 7. Tidak ada nilai bawaan yang ditanam di frontend | ✅ Terpenuhi | `INPATIENT_SETTING_FORM_DEFAULTS` berisi nilai string kosong; seluruh data angka murni bersumber dari respon `GET /inpatient-settings` backend |

Definition of Done terpenuhi: source code bersih, lint 0 error, build lulus, seluruh tes unit lulus, laporan tracked dibuat, serta roadmap & requirement traceability diperbarui.

---

## 8. Catatan Penutup

| Topik | Keterangan |
| --- | --- |
| Peringatan | ESLint menghasilkan 0 error dan mempertahankan baseline warning repository. |
| Dependensi Backend | Controller `InpatientSettingController` telah siap dan teruji. Ketersediaan baris master `DEFAULT` di lingkungan database pengujian tetap berada pada `RWI-UI-GAP-007`. |
| Status Git | Frontend: 4 berkas dimodifikasi (`inpatient-setting-view.jsx`, `use-inpatient-setting.jsx`, `inpatient-setting-constants.jsx`, `inpatient-setting-utils.jsx`) dan 1 berkas unit test diperbarui. Backend: berkas laporan `FE-RWI-041.md` ditambahkan, serta berkas roadmap/traceability diperbarui. Tidak ada commit/push otomatis. |
| Langkah Selanjutnya | Melanjutkan ke task berikutnya pada roadmap Rawat Inap (`FE-RWI-035`: Pembuktian alur bisnis utama ujung ke ujung). |
