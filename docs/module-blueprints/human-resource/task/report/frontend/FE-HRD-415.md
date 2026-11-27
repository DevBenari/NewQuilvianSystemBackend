# Laporan Perubahan Frontend — `FE-HRD-415`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-415` |
| Judul | Layar Penugasan Tambahan (penempatan sementara) |
| Slice | `A52-B` / `MVP-78` · `EPIC HRD-134` |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-415` (amandemen `A52-B`) |
| Trace | `FR-PK-037`; `HRD-DEC-251`; `VR-PK-27`; `api-contract.md` 37.6; desain `03` 36.3 (`F-TA`) |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Rute baru `/hr/workforce-core/temporary-assignments` sesuai desain 36.2/36.3; susunan kolom `DEV_DISCRETION`. Komponen dasar: `Hero`, `DataFilter`, `ResourceFilterSelect`, `FilterDatePicker`, `FilterSelect`, `DataTable`, `StatusBadge`, `InformationAlert` |
| Dependency | `FE-HRD-410` ✅ (fungsi service); `BE-HRD-359` ✅ (endpoint dan `VR-PK-27`) |
| Klasifikasi | `SMALL` — rute, view, dan utilitas baru; satu kelas CSS ditambahkan; uji unit dan spec E2E baru; tanpa service, slice, atau entri sidebar |
| Task mode | `FRONTEND` (instruksi pengguna 27 November 2026: "lanjutkan", sesudah `FE-HRD-414`) |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/workforce-core/temporary-assignments`, `src/components/view/hr/workforce-core`, `src/utils/hr/workforce-core`, `src/style/hr/workforce-core`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan `FE-HRD-410` s.d. `414` yang belum di-commit + perubahan modul lain milik sesi lain (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan yang belum di-commit; DLL lokal 26 November 2026 |
| Tanggal | 27 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar lintas pegawai untuk penempatan sementara | Service `getTemporaryAssignmentFilterMetadata`/`getTemporaryAssignments` sudah ada (`FE-HRD-410`); belum ada rute atau view | Kepala Unit tidak bisa melihat siapa yang ditempatkan sementara ke atau dari unitnya |
| Perilaku backend | `TemporaryAssignmentQueryService`: daftar dibatasi ke penempatan yang unit asal **atau** unit tujuannya dalam cakupan baca; `organizationUnitId` di luar cakupan → `403` "Anda tidak berwenang melihat data unit ini"; status waktu tidak sah atau rentang terbalik → `400`; `pageSize` dibatasi 1–100; tanggal = penempatan yang bersinggungan dengan rentang; cari = nama, kode pegawai, nomor surat | Pesan dan perilaku layar mengikuti server |
| Status waktu | Metadata mengirim `timeStatusOptions` `{ value, label }` (devYoga: Akan Datang, Berlangsung, Selesai); baris membawa `timeStatusLabel` | Label dari server, tidak dipetakan sendiri |
| Opsi unit | `organizationUnits` (`/master-data/organization-units/options`) memuat **semua** unit master data, tidak dibatasi cakupan. Tidak ada endpoint unit per cakupan | Pemilih unit untuk semua pengguna akan menawarkan unit yang pasti ditolak `403` |
| Preseden | `FE-HRD-315` (laporan kehadiran, keputusan pengguna 24 November 2026): pemilih unit hanya untuk pemegang `AttendanceReport : ReadAllUnits`; pengguna lain melihat seluruh unit dalam cakupannya tanpa pemilih | Pola yang sama dipakai di sini |
| Cakupan baca semua unit | `HrdUnitScopeService.GetReadScopeAsync`: `AttendanceReport : ReadAllUnits` **atau** cakupan kelola semua unit `RosterPeriod : ManageAllUnits` | Keduanya membuka pemilih unit |
| Data devYoga | Baca-saja: 2 penempatan (Raka Firmansyah → Instalasi Gawat Darurat, 1–14 dan 15–17 Maret 2027, Akan Datang, data uji `UJI-A52`) | Layar dapat diuji dengan data asli |
| Startup backend lokal | Seeder gagal `42P01 relation "public.MstProcurementMethod" does not exist` (migration Pengadaan dari merge lain belum diterapkan di devYoga) | Untuk pemeriksaan baca-saja backend dijalankan dengan `Seeders__ContinueOnFailure=true`. Migration tidak diterapkan (di luar wewenang) |

---

## 2. Proses bisnis dari sisi pengguna

Contoh dari data devYoga: Raka Firmansyah ditempatkan sementara di Instalasi Gawat Darurat pada 1–14 Maret dan 15–17 Maret 2027 (surat `UJI-A52-IGD-Tono-Temp`).

1. **Kepala Unit** membuka **Penugasan Tambahan** (`/hr/workforce-core/temporary-assignments`). Tabel menampilkan pegawai yang unit asal atau unit tujuannya ada dalam cakupannya: Pegawai (nama sebagai tautan ke **Profil Pegawai 360** dan kodenya), Unit Asal, Unit Tujuan, Tanggal ("01 Mar 2027 – 14 Mar 2027", atau "tanpa tanggal akhir"), Status Waktu (label server), dan No. Surat beserta keterangannya.
2. Kepala Unit menyaring dengan cari (nama, kode pegawai, nomor surat), rentang tanggal, dan status waktu. Pemilih unit tidak ditampilkan; keterangan menyebut "Menampilkan seluruh unit dalam cakupan Anda (unit asal atau unit tujuan)".
3. **Admin HRD** (pemegang cakupan baca semua unit) juga mendapat pemilih unit. Memilih "Instalasi Gawat Darurat" mengirim `organizationUnitId` dan menampilkan penempatan dengan unit asal atau tujuan IGD.
4. Untuk mengubah atau menambah penempatan, pengguna membuka Profil Pegawai 360. Layar ini tidak punya tombol tambah, ubah, atau hapus.

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Unit di luar cakupan (`403`) | "Anda tidak berwenang melihat data unit ini" (pesan server) + **Coba Lagi**; tabel tidak ditampilkan |
| Tidak ada data | "Belum ada penempatan sementara" |
| Rentang terbalik | "Tanggal akhir tidak boleh sebelum tanggal awal."; permintaan tidak dikirim |
| Tanpa `TemporaryAssignment : Read` | "Anda tidak berwenang membuka daftar penempatan sementara."; tanpa permintaan daftar |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-415`; kontrak 37.6, `VR-PK-27`; desain 36.3; backend `TemporaryAssignmentController`, `TemporaryAssignmentQueryService`, `TemporaryAssignmentDtos`, `HrdUnitScopeService`; frontend `workforce-core-cross-employee-service.js`, `attendance-report-view.jsx` (preseden pemilih unit), `hr-select-resources.js` (`organizationUnits`), rute `employee-360`, spec `hr-attendance-reports.spec.mjs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/workforce-core/temporary-assignments/page.jsx` (baru) | Rute dan metadata halaman |
| `src/components/view/hr/workforce-core/temporary-assignments-view.jsx` (baru) | Layar baca-saja: filter cari/unit (bersyarat)/tanggal/status waktu, tabel berpaginasi, tautan Profil Pegawai 360, pesan `403`/kosong/galat + Coba Lagi |
| `src/utils/hr/workforce-core/temporary-assignment-utils.js` (baru) | `canPickAnyUnit` (`AttendanceReport : ReadAllUnits` atau `RosterPeriod : ManageAllUnits`), pilihan status waktu dari metadata, query (unit hanya dikirim bila pemilih tersedia), validasi rentang, badge status waktu (label server), `employee360Href` |
| `src/style/hr/workforce-core/workforce-core.module.css` | `.tableLink`: tautan sel tabel berwarna utama dan bergaris bawah, dengan cincin fokus (token) |
| `tests/unit/hr-temporary-assignment-utils.test.mjs` (baru) | 4 uji |
| `tests/e2e/hr-temporary-assignments.spec.mjs` (baru) | 5 uji Playwright API tiruan |

### 3.3 Kepatuhan arsitektur frontend

- Data lewat `useHrRemoteData` dan service `FE-HRD-410`; opsi unit lewat `useSelectResource("organizationUnits")` + `ResourceFilterSelect`, sama dengan `FE-HRD-315`. Tidak ada service, slice, atau hook baru.
- Label status waktu dan pesan galat dari server.
- Tautan memakai `next/link` ke rute Profil Pegawai 360 yang ada (`/hr/workforce-core/employee-360/{id}`). Garis bawah membuat tautan terbaca tanpa bergantung pada warna.

### 3.4 Delta terhadap kartu dan kontrak

| Hal | Kartu/desain | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Filter unit "hanya dalam cakupan; pilihan dari opsi unit berizin" | Pemilih unit untuk semua pengguna | Pemilih unit hanya untuk pemegang cakupan baca semua unit; pengguna lain melihat seluruh unit dalam cakupannya tanpa pemilih | Belum ada daftar unit per cakupan; opsi unit master data memuat semua unit. Mengikuti keputusan pengguna 24 November 2026 (`FE-HRD-315`). Bila nanti ada endpoint unit per cakupan, pemilih dapat dibuka untuk semua pengguna |
| Kolom tambahan | Pegawai, unit asal, unit tujuan, tanggal, status waktu | Ditambah No. Surat + keterangan | Ruas ada di kontrak (`assignmentNumber`, `description`) dan dapat dicari |
| Entri sidebar | — | Tidak ditambahkan | Lingkup `FE-HRD-324` |
| Status kontrak 37.6 | "Rencana (belum tersedia)" | Tidak disunting | Artefak terkunci hash; dicatat untuk pass blueprint |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Memuat penempatan sementara..."; pilihan status waktu nonaktif selama metadata dimuat |
| Kosong | "Belum ada penempatan sementara" + keterangan cakupan |
| Gagal / `403` | Pesan server + **Coba Lagi**; tabel tidak ditampilkan |
| Tanpa hak akses | Pesan tidak berwenang; tanpa permintaan |
| Validasi | Rentang terbalik → peringatan, tanpa permintaan |
| Baca-saja | Tidak ada tombol tambah, ubah, atau hapus |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Core / Temporary Assignment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-core/temporary-assignments/filters/metadata` | Pilihan status waktu | `TemporaryAssignment : Read` |
| `GET` | `/v1/corporate/human-resource/workforce-core/temporary-assignments` | Daftar: `organizationUnitId` (hanya pemilih unit), `fromDate`, `toDate`, `timeStatus`, `search`, `pageNumber`, `pageSize=25` | `TemporaryAssignment : Read` |

#### Corporate / Human Resource / Master Data / Organization Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/options` | Pilihan unit (hanya untuk pemegang cakupan baca semua unit) | Sesuai endpoint opsi yang ada (dipakai juga `FE-HRD-315`) |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-temporary-assignment-utils.test.mjs` | 4/4 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif unit: 12 mutasi (unit selalu/tidak pernah dikirim; query tanpa halaman/status waktu; `pageSize` di atas batas; pemilih unit untuk semua; `ManageAllUnits` diabaikan; pilihan tanpa dedupe; label status buatan sendiri; rentang tidak diperiksa; label badge diabaikan; tautan 360 tanpa encode) | 12/12 tertangkap; source pulih (hash `2d96cd047d30` sama) | `PASS` | `mut415.cjs`, `mut415-hasil.log` |
| E2E API tiruan `hr-temporary-assignments.spec.mjs` (`next dev` 3711) | 5/5 lulus | `PASS` | Keluaran Playwright |
| Kontrol negatif E2E: 9 mutasi view (pemilih unit untuk semua; unit tidak dikirim walau dipilih; tanpa tautan 360; unit tujuan tidak tampil; galat tetap menampilkan tabel; filter tidak kembali ke halaman 1; tanpa hak Read tetap memuat; ada tombol tambah; teks kosong berbeda) | 9/9 tertangkap. Pada putaran pertama "unit tujuan tidak tampil" lolos, karena teks "HCU" juga ada di keterangan surat. Uji diperketat dengan memeriksa sel per kolom, lalu mutasi itu tertangkap. View pulih | `PASS` | `mut415e2e.cjs`, `mut415e2e-ulang.cjs`, `mut415e2e-hasil.log` |
| Regresi E2E: spec baru + `hr-attendance-reports` (pemilih unit `FE-HRD-315`) | 12/12 lulus | `PASS` | `e2e415-akhir.log` |
| Data asli devYoga (backend lokal 7184, GET diteruskan, izin disuapkan, tulis diblokir) | **Kepala Unit**: 2 baris Raka → IGD dengan label "Akan Datang", tautan 360 ke profil Raka, tanpa pemilih unit; filter status waktu mengirim `timeStatus=Upcoming`. **Admin HRD** (`ReadAllUnits`): 5 pilihan unit; memilih IGD mengirim `organizationUnitId` dan menampilkan 2 baris. Tidak ada tulis | `PASS` | `uat415.mjs`, `uat415-hasil.log`, `uat415-kepala-unit.png`, `uat415-admin-hrd.png` |
| Pemeriksaan visual | Temuan: nama pegawai bertautan tampil seperti teks biasa. Ditambah `.tableLink` (warna utama + garis bawah), lalu tangkapan diulang | `PASS` | `uat415-kepala-unit.png` |
| `npx eslint` atas lima berkas baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Tanpa galat | `PASS` | `lint415.log` |
| `npm run build` | "Compiled successfully in 99s"; rute `○ /hr/workforce-core/temporary-assignments` | `PASS` | `build415.log` |
| Suite unit penuh | 5.590 lulus, 161 gagal dari 5.751. Daftar nama yang gagal identik dengan run `FE-HRD-414` (tidak ada kegagalan baru); semuanya modul lain | `EXISTING / ENVIRONMENT ISSUE` | `unit415.log`, `f414.txt`/`f415.txt` |

**Uji manual:** lewat Playwright, dengan API tiruan dan data asli: daftar pilihan status waktu (3, label server), label terpilih, efek pada query dan hasil, filter gabungan (status + tanggal; unit + status), rentang terbalik, cari, reset, paginasi dan kembali ke halaman 1, pemilih unit hanya untuk cakupan semua unit, tautan Profil Pegawai 360. Tangkapan layar diperiksa visual.

**Tidak dijalankan:** `403` dengan data asli. Akun yang dipakai untuk meneruskan permintaan adalah superadmin (cakupan semua unit), sehingga penolakan unit di luar cakupan dibuktikan lewat API tiruan dan backend (`BE-HRD-359`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Filter mengubah query dan hasil; reset; paginasi | Terpenuhi | Uji unit query/pilihan/rentang; E2E AC1 dan Admin HRD; data asli: `timeStatus=Upcoming` dan `organizationUnitId` IGD |
| 2. `403` → "Anda tidak berwenang melihat data unit ini"; kosong → "Belum ada penempatan sementara" | Terpenuhi | E2E AC2 (pesan server, tabel disembunyikan, Coba Lagi → kosong) |
| 3. Tidak ada aksi ubah | Terpenuhi | E2E: tidak ada tombol Tambah/Ubah/Edit/Hapus/Simpan; mutasi "ada tombol tambah" tertangkap |
| DoD: AC terbukti, laporan `FE-HRD-415.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Backend lokal ke devYoga gagal start tanpa `Seeders__ContinueOnFailure=true`, karena migration Pengadaan (`MstProcurementMethod`) belum diterapkan. Perlu penerapan migration oleh yang berwenang. (2) Suite unit penuh tetap 161 gagal di modul lain |
| Masalah yang diketahui | Pemilih unit belum tersedia bagi Kepala Unit (menunggu daftar unit per cakupan, lihat delta); belum ada entri sidebar sampai `FE-HRD-324`; unit asal kosong ("-") bila pegawai tidak punya penempatan utama pada tanggal mulai (data uji Raka) |
| Dependency backend | Tidak ada yang tertunda untuk AC. Usulan untuk pass blueprint: opsi unit per cakupan baca agar pemilih unit dapat dibuka untuk semua pengguna (berlaku juga untuk laporan kehadiran `FE-HRD-315`) |
| Perubahan sampingan | `NONE`. `test-results/` tidak berubah |
| Server | `next dev` 3711 dan backend 7184 dinyalakan task ini dan dihentikan sebelum build. Proses `VBCSCompiler` milik sesi lain dibiarkan |
| Status Git | Berkas task ini: `M src/style/hr/workforce-core/workforce-core.module.css`, `?? src/app/hr/workforce-core/temporary-assignments/page.jsx`, `?? src/components/view/hr/workforce-core/temporary-assignments-view.jsx`, `?? src/utils/hr/workforce-core/temporary-assignment-utils.js`, `?? tests/unit/hr-temporary-assignment-utils.test.mjs`, `?? tests/e2e/hr-temporary-assignments.spec.mjs`. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-HRD-100` (portal ESS izin pulang cepat dan dinas luar), lalu `FE-HRD-416` dan `FE-HRD-324` |
