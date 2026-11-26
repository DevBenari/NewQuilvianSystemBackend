# Laporan Perubahan Frontend — `FE-HRD-313`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-313` |
| Judul | Layar master hari libur |
| Slice | `A52` / `MVP-67` / `EPIC HRD-112` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-313` |
| Trace | `FR-PK-015`; `HRD-DEC-210`; `VR-PK-25`; `03-frontend-architecture.md` baris Hari libur (Admin HRD: tambah, ubah, nonaktif) |
| Contract version | `v26` — `api-contract.md` bagian 32 "Master Data / Holiday", `approved` 12 November 2026. Tanpa delta: ketujuh endpoint tercantum di kontrak |
| Wewenang UI | Rute `/hr/master-data/holiday` sesuai usulan kartu, mengikuti pola `/hr/master-data/*` (daftar, `create`, `[slug]`, `[slug]/update`); butir menu **Hari Libur** di grup Jadwal Kerja & Shift. Tata letak `DEV_DISCRETION`; base component wajib |
| Dependency | `FE-HRD-306` ✅ (slice `master-data-holiday-slice`), `BE-HRD-347` ✅ |
| Klasifikasi | `MEDIUM` — empat rute, tiga view, tiga hook, konstanta, dua utilitas, menu, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 24 November 2026: "lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/master-data/holiday`, `src/components/view/hr/master-data/holiday`, `src/lib/hooks/hr/master-data/holiday`, `src/lib/constants/hr/master-data/holiday`, `src/utils/hr/master-data/holiday`, `src/utils/menu-sidebar/menu-items.jsx`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `004524f4c` (branch `YogaV2`), working tree bersih sebelum task |
| Commit backend yang dijadikan rujukan | `4d2c0162` (branch `yoga`) |
| Tanggal | 24 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-11` dengan data nyata belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar hari libur | Tidak ada rute atau pemakaian `master-data-holiday-slice` di `src/` selain registrasi store (`FE-HRD-306`) | Admin HRD tidak dapat mencatat hari libur; pegawai jadwal tetap terbaca "Tidak Ada Scan" pada hari libur |
| Kontrak tanpa `/summary` dan `/options` | `api-contract.md` §32 Holiday; slice `FE-HRD-306` sengaja tidak mengekspornya | Layar tanpa kartu ringkasan, berbeda dari master data HR lain |
| Ubah tidak membawa status aktif | `UpdateHolidayRequest` tanpa `IsActive`; status lewat `PATCH /{id}/status` | Aktif/nonaktif menjadi aksi tersendiri di detail, tidak seperti sakelar status di formulir Kalender Kerja |
| Select relasi dapat mengambil data sendiri | `BaseEditorField` memakai registry `SELECT_RESOURCES` berdasarkan nama ruas | Ruas kalender dan jenis diberi `remote: false` agar pilihan tetap dari `filters/metadata` (acceptance criteria kartu) |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Admin HRD (`Holiday : Read/Create/Update/Delete`).

**Contoh UAT-PK-11 (bagian data libur).** Admin HRD mencatat libur nasional 17 Agustus pada kalender kerja rumah sakit.

1. Admin membuka **Sumber Daya Manusia → Master Data → Jadwal Kerja & Shift → Hari Libur**. Daftar menampilkan tanggal (mis. "3 November 2026 – 4 November 2026 (2 hari)"), kode, nama, jenis (label server), tanda libur nasional, kalender kerja, dan status.
2. Ia menekan **+ Tambah Hari Libur**. Kalender default terpilih otomatis, misalnya "Kalender Kerja 2026 (WCL-RSMMC-00002) · 1 Jan 2026 – 1 Jan 2027"; di bawahnya tertulis "Rentang Kalender Kerja 2026: 1 Januari 2026 – 1 Januari 2027. Tanggal libur wajib berada di dalam rentang ini."
3. Ia mengisi kode `HUT-RI-2026`, nama "Hari Kemerdekaan RI", tanggal mulai dan selesai 17 Agustus 2026, jenis **Libur Nasional**, dan menandai **Libur Nasional**. Pemilih tanggal menonaktifkan tanggal di luar rentang kalender, dan tanggal selesai tidak dapat sebelum tanggal mulai.
4. Sesudah **Simpan Hari Libur**, layar pindah ke detail hari libur itu. Rekap kehadiran tanggal tersebut berubah ketika diproses ulang (job harian, sapuan, atau proses manual — lihat risiko `BE-HRD-347`).

Daftar dapat disaring dengan kalender kerja, tahun, jenis libur, status, dan pencarian kode/nama. Di detail tersedia **Nonaktifkan/Aktifkan**, **Perbarui**, dan **Hapus**, masing-masing hanya bagi yang berhak.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Tanggal di luar rentang kalender (mis. kalender diganti sesudah tanggal dipilih) | "Tanggal libur harus berada di dalam kalender kerja yang dipilih." di ruas tanggal; tidak ada permintaan simpan |
| Tanggal selesai sebelum mulai | "Tanggal selesai tidak boleh lebih awal dari tanggal mulai."; tidak ada permintaan simpan |
| Kode sudah dipakai pada kalender yang sama (409) | Pesan server di pemberitahuan; tetap di formulir |
| Ruas wajib kosong / terlalu panjang | Pesan per ruas ("… wajib diisi.", "… maksimal 50 karakter.") |
| Kalender hari libur sudah nonaktif saat diubah | Pilihan tampil "… · tidak aktif"; rentang diperiksa server |
| Gagal memuat metadata | Pesan server di daftar |
| Gagal nonaktifkan/hapus | Pesan server di dalam dialog konfirmasi |
| Hanya hak baca | Tanpa tombol Tambah/Nonaktifkan/Perbarui/Hapus; membuka `/create` langsung: "Anda tidak berwenang menambah hari libur." tanpa tombol simpan |
| Tanpa hak baca | "Anda tidak berwenang membuka daftar hari libur."; tidak ada permintaan data |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-313`; `api-contract.md` §32 Holiday; `00-interview-decisions.md` `HRD-DEC-210`; `04-prd-to-mvp.md` `UAT-PK-11`; laporan `BE-HRD-347`; `HolidayController.cs`, `HolidayDtos.cs`; `master-data-holiday-slice.jsx`, `master-data-resource-slice-factory.jsx`; pola Kalender Kerja (`work-calendar-constants.jsx`, `work-calendar-utils.jsx`, tiga hook, tiga view, empat rute); `base-editor-field.jsx`, `base-form-control.jsx` (`BaseDateField` min/max), `select-resource-registry.js`, `confirm-modal.jsx`, `data-filter.jsx`; `menu-items.jsx`; laporan dan spec `FE-HRD-312`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/master-data/holiday/{page.jsx,holiday-client.jsx,create/page.jsx,[slug]/page.jsx,[slug]/update/page.jsx}` (baru) | Empat rute dengan pola token rute privat yang sama dengan Kalender Kerja |
| `src/lib/constants/hr/master-data/holiday/holiday-constants.jsx` (baru) | `HOLIDAY_CONFIG`: filter, kolom, ruas formulir (batas panjang = DTO server), ruas detail |
| `src/utils/hr/master-data/holiday/holiday-form-rules.js` (baru) | Fungsi murni: pilihan kalender/jenis/tahun dari metadata, kalender default, `VR-PK-25`, payload `POST`/`PUT` tanpa `isActive`, format rentang tanggal, query daftar |
| `src/utils/hr/master-data/holiday/holiday-utils.jsx` (baru) | Utilitas tampilan dan formulir berbentuk `work-calendar-utils` |
| `src/lib/hooks/hr/master-data/holiday/use-master-data-holiday{,-editor,-detail}.jsx` (baru) | Daftar + filter + hak; formulir dengan kalender default, batas tanggal dinamis, galat server; detail dengan aktif/nonaktif (`PATCH`) dan hapus |
| `src/components/view/hr/master-data/holiday/{master-data-holiday-view.jsx,add/holiday-form-view.jsx,detail/holiday-detail-view.jsx}` (baru) | Tiga view memakai `DataFilter`, `DataTable`, `BaseEditorView`, `BaseDetailView`, `ConfirmModal` |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Hari Libur** (`Holiday : Read`) sesudah Kalender Kerja |
| `tests/unit/hr-holiday-form-rules.test.mjs` (baru) | 7 uji |
| `tests/e2e/hr-master-data-holiday.spec.mjs` (baru) | 7 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Mengikuti pola master data HR berkas-per-sumber daya (konstanta, utils, tiga hook, tiga view, empat rute) seperti arahan kartu. State tetap di slice `FE-HRD-306` (factory master data, `unwrap()` untuk galat `{ message, statusCode }`); tidak ada service atau slice baru. Hak memakai `useStrictPermissions`. Seluruh kontrol memakai base component; aturan bisnis berada di fungsi murni yang diuji. Perbedaan dari pola Kalender Kerja: tanpa kartu ringkasan (kontrak tanpa `/summary`) dan status aktif sebagai aksi di detail (kontrak memisahkan `PATCH /status`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil data hari libur..."; formulir terkunci dan "Menyiapkan pilihan data..."; tombol konfirmasi berputar dan dialog terkunci selama permintaan |
| Kosong | "Data hari libur tidak ditemukan." dengan saran sesuai hak |
| Gagal | Pesan server di daftar, formulir (pemberitahuan), atau dialog konfirmasi |
| Tanpa hak akses | Pesan tidak berwenang; tombol aksi disembunyikan sesuai hak, server tetap memeriksa |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Holiday

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/holidays/filters/metadata` | Pilihan kalender kerja (beserta rentang) dan jenis libur | `Holiday : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/holidays` | Daftar (`workCalendarId`, `year`, `holidayType`, `isActive`, `search`, paginasi) | `Holiday : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Detail dan isi formulir ubah | `Holiday : Read` |
| `POST` | `/v1/corporate/human-resource/master-data/holidays` | Menambah | `Holiday : Create` |
| `PUT` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Mengubah (tanpa `isActive`) | `Holiday : Update` |
| `PATCH` | `/v1/corporate/human-resource/master-data/holidays/{id}/status` | Aktif/nonaktif | `Holiday : Update` |
| `DELETE` | `/v1/corporate/human-resource/master-data/holidays/{id}` | Hapus lunak | `Holiday : Delete` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-master-data-holiday.spec.mjs` (build produksi, port 3710, API tiruan berkeadaan, nol tulis) | 7/7 lulus | `PASS` | Keluaran Playwright |
| — `UAT-PK-11` (data libur) | Kalender default terpilih dengan keterangan rentang; kode, nama, tanggal 17, jenis Libur Nasional, tanda nasional → badan `POST` persis `{workCalendarId, holidayCode, holidayName, startDate, endDate, holidayType: "National", isNationalHoliday: true, description: null}`; pindah ke detail | `PASS` | Skenario 1 |
| — Filter | Pilihan kalender = 2 kalender metadata + "Semua Kalender"; kalender, tahun, jenis, status terkirim gabungan dengan `pageNumber=1`; ganti jenis → daftar kosong; Atur ulang filter → hanya `pageNumber`/`pageSize` | `PASS` | Skenario 2 |
| — `VR-PK-25` + 409 | Kalender diganti ke tahun lalu → dua pesan "Tanggal libur harus berada di dalam kalender kerja yang dipilih." dan nol `POST`; kembali ke kalender berjalan → pesan hilang, `POST` terkirim, pesan 409 server tampil, tetap di formulir | `PASS` | Skenario 3 |
| — Detail | Nonaktifkan → `PATCH …/status` `{ isActive: false }`, tombol berganti Aktifkan; Perbarui → badan `PUT` tanpa `isActive`; Hapus → `DELETE`, kembali ke daftar | `PASS` | Skenario 4 |
| — Hak baca saja / tanpa hak / metadata gagal | Tanpa tombol aksi; `/create` menolak; tanpa hak tidak ada permintaan ke `holidays`; pesan metadata server tampil | `PASS` | Skenario 5–7 |
| Regresi `hr-attendance-devices.spec.mjs` (`FE-HRD-312`, menu ikut berubah) | 8/8 lulus bersama 7/7 spec ini dalam satu putaran (15/15) | `PASS` | Keluaran Playwright |
| Pemeriksaan visual (tangkapan layar formulir, pemilih tanggal, detail) | Tata letak rapi; tanggal di luar rentang tidak dapat dipilih; tidak ada UUID tampil | `PASS` | Tangkapan di scratchpad sesi |
| `tests/unit/hr-holiday-form-rules.test.mjs` | 7/7 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (rentang kalender tidak diperiksa; selesai < mulai lolos; `isActive` ikut payload; keterangan kosong menjadi `""`; kalender default diabaikan; `isActive=false` tidak dikirim) | 6/6 tertangkap; source dipulihkan identik byte | `PASS` | `mut313.cjs` di scratchpad sesi |
| Seluruh uji unit (`node --import ./tests/helpers/register.mjs --test tests/unit/`) | 5.268 lulus, 100 gagal dari 5.368 | `EXISTING / ENVIRONMENT ISSUE` | Jumlah gagal sama dengan putaran `FE-HRD-312` (100); seluruhnya di modul lain (billing, payroll, base component) |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas berkas task | Tanpa error; 2 peringatan `react-hooks/set-state-in-effect` pada efek token rute, sama persis dengan hook Kalender Kerja yang dijadikan pola | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; empat rute `/hr/master-data/holiday*` terdaftar | `PASS` | Keluaran perintah |

AUTOMATED TEST: unit — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol baru (Tambah, pemilih kalender/jenis/tanggal, sakelar Libur Nasional, Simpan, filter kalender/tahun/jenis/status, Atur ulang filter, Nonaktifkan, Perbarui, Hapus, Batal/konfirmasi) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-11` dengan data nyata devYoga. Menambah hari libur di devYoga memengaruhi rekap kehadiran pegawai jadwal tetap saat diproses ulang. Perilaku backend sudah dibuktikan `AT-HRD-PK-28` (laporan `BE-HRD-347`, termasuk 403 akun non-superadmin).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pilihan kalender kerja dari `filters/metadata` | Terpenuhi | Skenario 1–3; uji unit 1; mutasi M5; ruas `remote: false` |
| Outcome: Admin HRD mengelola hari libur per kalender kerja | Terpenuhi | Skenario 1–4 |
| DoD umum: base component, memuat/kosong/galat, tombol nonaktif saat mengirim, tidak berwenang, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-11` (data libur) | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `IsPaidHoliday` dan `IsRecurringAnnually` hanya ditampilkan di detail; kontrak tidak mengizinkan mengubahnya (`BE-HRD-347` §3.3) |
| Masalah yang diketahui | (1) Resource `Holiday` belum diberikan ke jabatan Admin HRD di Role Access (`BE-HRD-347` §7) — sampai itu dilakukan, menu dan layar hanya terbuka bagi superadmin. (2) Menambah/menghapus hari libur tidak memicu hitung ulang rekap yang sudah ada; layar menyebutkannya di panel formulir dan dialog konfirmasi |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` — keluaran Playwright diarahkan ke scratchpad; spec sementara tangkapan layar sudah dihapus |
| Interupsi | `NONE` |
| Status Git | ` M src/utils/menu-sidebar/menu-items.jsx`, `?? src/app/hr/master-data/holiday/`, `?? src/components/view/hr/master-data/holiday/`, `?? src/lib/constants/hr/master-data/holiday/`, `?? src/lib/hooks/hr/master-data/holiday/`, `?? src/utils/hr/master-data/holiday/`, `?? tests/e2e/hr-master-data-holiday.spec.mjs`, `?? tests/unit/hr-holiday-form-rules.test.mjs`. Repository backend: laporan ini (baru), ` M roadmap/frontend-roadmap.md`, ` M roadmap/requirement-traceability.md`. Tidak ada commit |
| Langkah berikutnya | `FE-HRD-314` (Jadwal Saya tersambung ke server; butuh `BE-HRD-348`) |
