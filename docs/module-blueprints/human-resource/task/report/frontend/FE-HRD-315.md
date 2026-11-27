# Laporan Perubahan Frontend — `FE-HRD-315`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-315` |
| Judul | Tujuh layar laporan dengan ekspor Excel |
| Slice | `A52` / `MVP-68` / `EPIC HRD-114` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-315` |
| Trace | `FR-PK-017`, `FR-PK-018`; `HRD-DEC-206`; `VR-PK-27`, `VR-PK-28`; `03-frontend-architecture.md` 31.1 (M12) dan 31.2 baris "Laporan (7)" |
| Contract version | `v26` — `api-contract.md` bagian 32 "Attendance Management / Attendance Report", `approved` 12 November 2026. Respons memakai perluasan `BE-HRD-349` (`columns`, `reportName`, rentang) yang sudah tercatat di laporan backend |
| Wewenang UI | Rute `/hr/attendance/reports/{reportCode}` sesuai usulan kartu (satu rute dinamis untuk tujuh kode kontrak); grup menu **Laporan Kehadiran** berisi tujuh butir. Tata letak `DEV_DISCRETION`; base component wajib. **Keputusan pengguna 24 November 2026** (pertanyaan saat delivery): pemilih unit hanya untuk pemegang `AttendanceReport : ReadAllUnits` |
| Dependency | `FE-HRD-306` ✅ (service, slice laporan), `BE-HRD-349` ✅, `BE-HRD-350` ✅ |
| Klasifikasi | `MEDIUM` — satu rute dinamis, satu view, satu utilitas, perbaikan slice dan service, menu, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 24 November 2026: "lanjutkan FE-HRD-315") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/attendance/reports`, `src/components/view/hr/attendance`, `src/utils/hr/attendance`, `src/lib/state/slice/hr/attendance/attendance-report-slice.js`, `src/lib/services/hr/attendance-management-service.js`, `src/utils/menu-sidebar/menu-items.jsx`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `004524f4c` (branch `YogaV2`) + working tree `FE-HRD-313`, `FE-HRD-314` |
| Commit backend yang dijadikan rujukan | `4d2c0162` (branch `yoga`) |
| Tanggal | 24 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-14`/`-15` dengan data nyata devYoga belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar laporan kehadiran | Tidak ada rute `/hr/attendance/reports`; service `getAttendanceReport`/`exportAttendanceReport` dan slice `attendance-report-slice` dari `FE-HRD-306` belum dipakai | Laporan M12 dan ekspor hanya dapat diakses lewat API |
| **Kontrak tanpa daftar unit per cakupan** | Kontrak §32 hanya memuat `GET /{reportCode}` dan `/export`; satu-satunya pilihan unit yang ada (`organization-units/options`) memuat semua unit | Syarat kartu "pilihan unit dibatasi cakupan" tidak dapat dipenuhi dengan daftar unit. Ditanyakan ke pengguna; dipilih: pemilih unit hanya untuk `ReadAllUnits`, selain itu laporan memuat seluruh unit dalam cakupan |
| Status shift, cuti, dan lembur dikirim mentah | `AttendanceReportService`: `StatusLabel = AssignmentStatus` / `Status` / `OvertimeRequestStatus` (`BE-HRD-349` §3.4) | Layar menampilkan teks server apa adanya agar sama dengan berkas ekspor; saring status hanya untuk laporan yang punya sumber label |
| **Nama berkas ekspor tidak terbaca peramban** | API berada di origin lain (`NEXT_PUBLIC_API_QUILVIAN`); CORS backend (`Program.cs` `FrontendCorsPolicy`) tanpa `WithExposedHeaders("Content-Disposition")` | Nama berkas jatuh ke cadangan service "laporan-kehadiran-<kode>.xlsx". Diperbaiki di sisi layar (bagian 3.2); celah CORS dicatat untuk backend |
| Slice laporan menerapkan respons basi | `fetchAttendanceReport.rejected` (pembatalan) dari permintaan lama mematikan `loading` permintaan baru | Saat filter diubah cepat, tabel dapat menampilkan data lama tanpa tanda memuat. Diperbaiki (bagian 3.2) |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Kepala Unit, Kepala Bidang, Admin HRD (`AttendanceReport : Read`; ekspor `AttendanceReport : Export`).

**Contoh UAT-PK-14.** Sari, Kepala Unit ICU, membuka **Laporan Kehadiran → Laporan Keterlambatan**.

1. Rentang bawaan bulan berjalan (WIB). Di bawah filter tertulis "Laporan memuat seluruh unit dalam cakupan Anda. Rentang paling lama 92 hari." — Sari tidak diberi pemilih unit; server membatasi data ke ICU.
2. Label periode: "Periode sudah ditutup — data final." (hijau) atau "Periode masih terbuka — data dapat berubah." (kuning), kalimat yang sama dengan kepala berkas ekspor.
3. Tabel memakai kolom yang dikirim server: Kode Pegawai, Nama, Unit, Tanggal, Shift, Jadwal Masuk, Masuk, Terlambat (menit). Jam tampil WIB, misalnya "03 Nov 2026 07.12".
4. **Ekspor** mengunduh `laporan-lateness_2026-11-01_2026-11-30.xlsx`. Query ekspor = query tabel tanpa paginasi (unit, rentang, status sama), sehingga isi berkas sama dengan layar. Selama mengunduh tombol bertuliskan "Mengunduh..." dan tidak dapat ditekan lagi.

**Admin HRD** (`ReadAllUnits`) mendapat pemilih **Semua unit** dan dapat memilih satu unit; pilihan itu ikut ke tabel dan ekspor.

**Saring status:** Laporan Kehadiran dan Keterlambatan memakai label status harian dari server (hanya bila pengguna dapat membaca rekap harian); Laporan Ketidakhadiran memakai lima kategori (Alpa, Tidak Ada Scan, Belum Lengkap, Sakit, Cuti). Laporan Shift, Cuti, Lembur, dan Rekap Per-Unit tanpa saring status. Pindah ke laporan lain mengosongkan saring status.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Tanggal akhir sebelum tanggal awal | "Tanggal akhir tidak boleh sebelum tanggal awal."; tidak ada permintaan; **Ekspor** nonaktif |
| Rentang lebih dari 92 hari (`VR-PK-28`) | "Rentang laporan paling lama 92 hari"; tidak ada permintaan; **Ekspor** nonaktif |
| Ekspor ditolak server (403 `VR-PK-27`, 400) | Pesan server di pemberitahuan; tidak ada berkas |
| Gagal memuat laporan | Pesan server dan **Coba Lagi** |
| Tidak ada baris | "Tidak ada data untuk filter ini." |
| Tanpa hak `Export` | Tombol **Ekspor** tidak ada |
| Tanpa hak `Read` | "Anda tidak berwenang membuka laporan kehadiran."; tidak ada permintaan |
| Kode laporan di luar tujuh kode | Halaman tidak ditemukan bawaan aplikasi; tidak ada permintaan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-315`; `api-contract.md` §32 Attendance Report; `00-interview-decisions.md` `HRD-DEC-206`; `validation-matrix.md` `VR-PK-27`, `-28`; `04-prd-to-mvp.md` `UAT-PK-14`, `-15`; laporan `BE-HRD-349`, `BE-HRD-350`; `AttendanceReportController.cs`, `AttendanceReportDtos.cs`, `AttendanceReportService.cs` (kolom, label, status), `Program.cs` (CORS); `attendance-management-service.js`, `attendance-report-slice.js`, `hr-a52-api-client.test.mjs`; `attendance-monitoring-view.jsx` (pola filter), `attendance-daily-status-utils.js`, `use-select-resource.jsx`, `resource-filter-select.jsx`, `pagination.jsx`, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/attendance/reports/[reportCode]/page.jsx` (baru) | Rute dinamis; kode di luar tujuh kode → `notFound()`; judul halaman per laporan |
| `src/components/view/hr/attendance/attendance-report-view.jsx` (baru) | Filter tanggal, unit (hanya `ReadAllUnits`), status; label periode; tabel berkolom server dengan paginasi; **Ekspor**; galat + Coba Lagi; hak Read/Export. State laporan dan filter di slice `FE-HRD-306` |
| `src/utils/hr/attendance/attendance-report-utils.js` (baru) | Metadata tujuh laporan, kategori ketidakhadiran, rentang bawaan WIB, `VR-PK-28`, `buildReportQuery` (dipakai tabel dan ekspor), format sel, label periode |
| `src/lib/state/slice/hr/attendance/attendance-report-slice.js` | `currentRequestId`: respons/pembatalan permintaan lama tidak lagi menimpa permintaan terbaru |
| `src/lib/services/hr/attendance-management-service.js` | Nama berkas cadangan ekspor mengikuti format server `laporan-<kode>_<mulai>_<selesai>.xlsx` bila `Content-Disposition` tidak terbaca |
| `src/utils/menu-sidebar/menu-items.jsx` | Grup **Laporan Kehadiran** dengan tujuh butir (`AttendanceReport : Read`) |
| `tests/unit/hr-attendance-report-utils.test.mjs` (baru) | 7 uji |
| `tests/e2e/hr-attendance-reports.spec.mjs` (baru) | 7 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Data laporan memakai slice `attendance-report-slice` dan service `AttendanceManagementService` dari `FE-HRD-306`; tidak ada service atau slice baru. Ekspor dipanggil langsung lewat service sesuai catatan slice ("unduhan sekali jalan"). Metadata status harian memakai `useHrRemoteData` seperti layar `A52` lain. Hak memakai `useStrictPermissions`. Pilihan unit memakai `useSelectResource("organizationUnits")` + `ResourceFilterSelect` seperti layar monitoring. Seluruh kontrol memakai base component; aturan di fungsi murni yang diuji.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Memuat laporan..."; Atur ulang nonaktif; Ekspor "Mengunduh..." |
| Kosong | "Tidak ada data untuk filter ini." |
| Gagal | Pesan server + **Coba Lagi**; ekspor gagal sebagai pemberitahuan |
| Validasi | Pesan rentang; tanpa permintaan; Ekspor nonaktif |
| Tanpa hak akses | Pesan tidak berwenang; tombol Ekspor disembunyikan tanpa `Export` |
| Privasi | Hanya kolom server; sakit tetap "Sakit" tanpa jenis atau alasan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Attendance Management / Attendance Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/reports/{reportCode}` | Tabel: `fromDate`, `toDate`, `organizationUnitId` (hanya `ReadAllUnits`), `status`, `pageNumber`, `pageSize` | `AttendanceReport : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/reports/{reportCode}/export` | Berkas `.xlsx`; query sama dengan tabel tanpa paginasi | `AttendanceReport : Export` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/filters/metadata` | Label status harian untuk saring status Kehadiran/Keterlambatan | `AttendanceDaily : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/options` | Pemilih unit, hanya untuk `ReadAllUnits` | sesuai endpoint options |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-reports.spec.mjs` (build produksi, port 3710, API tiruan, nol tulis) | 7/7 lulus | `PASS` | Keluaran Playwright |
| — `UAT-PK-14` (Kepala Unit) | Tanpa pemilih unit dan tanpa permintaan daftar unit; "Periode sudah ditutup — data final."; 8 kolom server; jam WIB; unduhan `laporan-lateness_<mulai>_<selesai>.xlsx`; query ekspor = query tabel tanpa `pageNumber`/`pageSize` | `PASS` | Skenario 1 |
| — Admin HRD | Pilih ICU dan "Sakit" → tabel dan ekspor sama-sama `organizationUnitId` + `status=Sick`; 6 pilihan status; HCU kosong; Atur ulang filter → bulan berjalan tanpa unit/status | `PASS` | Skenario 2 |
| — Rentang terbalik | Pesan; tanpa permintaan baru; Ekspor nonaktif | `PASS` | Skenario 3 |
| — `UAT-PK-15` (bagian layar) | Ekspor 403 → pesan server; nol unduhan; tombol aktif kembali | `PASS` | Skenario 4 |
| — Paginasi dan Rekap Per-Unit | Halaman 2 → `pageNumber=2`; pindah ke Rekap Per-Unit → kolom server, tanpa saring status | `PASS` | Skenario 5 |
| — Hak dan kode tak dikenal | Tanpa Export tanpa tombol; tanpa Read pesan dan nol permintaan; `/reports/payroll` → halaman tidak ditemukan, nol permintaan | `PASS` | Skenario 6 |
| — Saring status harian | Dengan `AttendanceDaily : Read`: label server, `status=Late` terkirim; tanpa hak: tanpa saring status | `PASS` | Skenario 7 |
| Regresi spec yang berbagi menu/service: `hr-attendance-devices` 8/8, `hr-master-data-holiday` 7/7, `hr-my-schedule` 4/4 | Lulus bersama 7/7 spec ini (26/26) sesudah build terakhir | `PASS` | Keluaran Playwright |
| Pemeriksaan visual (tangkapan layar Admin dan Kepala Unit) | Satu perbaikan dari pemeriksaan ini: saring status harian tidak lagi tampil berisi satu pilihan cadangan bila metadata tidak termuat | `PASS` | Tangkapan di scratchpad sesi |
| `tests/unit/hr-attendance-report-utils.test.mjs` + regresi `hr-a52-api-client.test.mjs` | 7/7 + 12/12 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 7 mutasi (ekspor ikut paginasi; ekspor tanpa unit; batas 93 hari; status terkirim untuk laporan tanpa saring; jam tanpa geser WIB; respons basi diterapkan slice; nama cadangan lama) | 7/7 tertangkap; source dipulihkan identik byte | `PASS` | `mut315.cjs` di scratchpad sesi; build ulang sesudah mutasi sebelum uji layar akhir |
| Seluruh uji unit | 5.281 lulus, 100 gagal dari 5.381 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-313`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) dan `npx eslint` atas berkas task | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; rute `/hr/attendance/reports/[reportCode]` terdaftar | `PASS` | Keluaran perintah |

AUTOMATED TEST: unit — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol baru (tanggal awal/akhir, pemilih unit, saring status, Atur ulang filter, paginasi, Ekspor, Coba Lagi, butir menu) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-14`/`-15` dengan data nyata devYoga. `AttendanceReport : Read/Export/ReadAllUnits` belum diberikan ke jabatan mana pun (`BE-HRD-349`, `BE-HRD-350`). Perilaku server (cakupan unit, `VR-PK-27`/`-28`, log ekspor, tanpa berkas saat ditolak) dibuktikan `AT-HRD-PK-37`–`39`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Ekspor memakai filter layar yang sama | Terpenuhi | Skenario 1–2; uji unit 2; mutasi M1, M2, M4 |
| Pilihan unit dibatasi cakupan | Terpenuhi sesuai keputusan pengguna: pemilih semua unit hanya untuk `ReadAllUnits` (cakupannya memang semua unit); pengguna lain tanpa pemilih, laporan otomatis dibatasi cakupan oleh server. Kepala Bidang belum dapat menyaring satu unit di bawahnya | Skenario 1–2 |
| Outcome: laporan per cakupan, label periode terbuka/ditutup, tombol **Ekspor** | Terpenuhi | Skenario 1–2, 5 |
| Risiko kartu: berkas besar — status mengunduh | Terpenuhi ("Mengunduh...", tombol terkunci) | Bagian 4; skenario 4 |
| DoD umum | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-14`, `UAT-PK-15` | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1 dan 4 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Status pada Laporan Shift, Cuti, dan Lembur tampil sebagai nilai server berbahasa Inggris (`Published`, `Approved`, `Realized`), sama dengan berkas ekspor |
| Masalah yang diketahui / tindak lanjut backend | (1) **CORS**: `FrontendCorsPolicy` tidak membuka `Content-Disposition`, sehingga nama berkas dari server tidak terbaca di peramban untuk seluruh ekspor aplikasi; layar ini memakai nama cadangan berformat sama. Usul: `.WithExposedHeaders("Content-Disposition")`. (2) **Daftar unit per cakupan** (mis. `GET attendance/reports/filters/metadata`) agar Kepala Bidang dapat menyaring satu unit; perlu penyegaran kontrak. (3) **Label status** shift/cuti/lembur berbahasa Indonesia di server agar layar dan ekspor sama-sama berlabel, sekaligus sumber pilihan saring status ketiganya. (4) Izin `AttendanceReport : Read/Export/ReadAllUnits` belum diberikan ke jabatan mana pun |
| Dependency backend | `NONE` untuk task ini; butir (1)–(3) di atas usulan task lanjutan |
| Perubahan sampingan | `NONE` — keluaran Playwright diarahkan ke scratchpad; spec sementara tangkapan layar dihapus. Akhir baris `attendance-management-service.js` diseragamkan CRLF sesudah suntingan (salinan kerja repo memakai CRLF) |
| Interupsi | `NONE` |
| Status Git | ` M src/lib/services/hr/attendance-management-service.js`, ` M src/lib/state/slice/hr/attendance/attendance-report-slice.js`, ` M src/utils/menu-sidebar/menu-items.jsx`, `?? src/app/hr/attendance/reports/`, `?? src/components/view/hr/attendance/attendance-report-view.jsx`, `?? src/utils/hr/attendance/attendance-report-utils.js`, `?? tests/unit/hr-attendance-report-utils.test.mjs`, `?? tests/e2e/hr-attendance-reports.spec.mjs`; ditambah berkas `FE-HRD-313`, `FE-HRD-314` yang belum di-commit. Repository backend: laporan ini (baru), ` M roadmap/frontend-roadmap.md`, ` M roadmap/requirement-traceability.md`. Tidak ada commit |
| Langkah berikutnya | `FE-HRD-316` — verifikasi browser MVP `A52` dengan data asli devYoga (`UAT-PK-01` s.d. `-15`); butuh pemberian izin dan data uji |
