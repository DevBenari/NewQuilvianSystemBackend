# Laporan Perubahan Frontend — `FE-IGD-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-035` |
| Judul | Daftar Menunggu Triage membaca satu sumber (`triage-queue`) |
| Slice | `S2` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) bagian R3.12 |
| Trace | `FR-IGD-070` (sisi layar); `IGD-DEC-139` butir 2, `IGD-DEC-144`, `IGD-DEC-151`, `IGD-DEC-007`, `IGD-DEC-128`, `IGD-DEC-147` (label Terdaftar ≠ Tiba); layar `03-frontend-architecture.md` §13.3 B, §13.4, §13.5, §13.7 |
| Contract version | API **`0.11.0`** §8.3.1 — bagian encounter-first **`approved`** (`IGD-DEC-157`). Berkas kontrak kini `0.12.0` (amendment `IGD-DEC-170`, 23 September 2026), tetapi agent membandingkan isinya dengan versi yang dikunci hash (`c0bcea54…`, commit `48a78703`): selisihnya hanya baris versi dan bagian §9 baru. **§8.3.1 tidak berubah satu huruf pun**, jadi kontrak terkunci tetap berlaku |
| Wewenang UI | `DEV_DISCRETION` (03 §13.6): urutan kolom dan tombol, ikon, bentuk isian. Memakai tabel daftar yang sudah ada; nol elemen `NEW`; nol CSS baru; nol CSS global |
| Dependency | `BE-IGD-054` ✅ (24 September 2026, atas penilaian pemilik — [laporan](../backend/BE-IGD-054.md)); source di commit backend `5ed06bcd` |
| Klasifikasi | `MEDIUM` — skor 5: repository 0 (satu repository ditulis), berkas diperiksa 1 (± 20), berkas diubah 1 (7), logika bisnis 1 (label waktu dan tombol diturunkan dari data backend), kontrak API 1 (endpoint baru dikonsumsi), database 0, keamanan/auth 0, UI/workflow 1 (satu layar kerja) |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev` ditambah laporan ini beserta tautan buktinya pada roadmap dan traceability. Backend **hanya-baca**. Tanpa commit, push, pull, merge, rebase, pindah branch, atau stash. Pemilik memberi go-ahead 30 September 2026: *"mulai sesuai dengan urutan yang anda sarankan"* (urutan pertama: `FE-IGD-035`), ditambah dua arahan tampilan: *"buat tampilan yang user friendly dan mudah di mengerti oleh user, pastikan code tidak ada baris comment"* dan *"tampilan yang bagus juga"* |
| Target tulis | `QuilvianSystemFrontendDev`, branch `RizkiV2` (upstream `origin/RizkiV2`, working tree bersih sebelum mulai) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `2c2190858` (`RizkiV2`) + perubahan working tree task ini (belum di-commit) |
| Commit backend yang dijadikan rujukan | `327ccad3` (`rizkiG`); `GetTriageQueueAsync` tidak berubah sejak `5ed06bcd` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI 30 September 2026 — atas penilaian pemilik.** Implementation Complete; `eslint` tujuh berkas **0 error, 0 warning**; unit test berkas task **19/19**, test IGD terkait **70/70** (dijalankan agent). **`npm run build` dijalankan pemilik** — artefak `.next/BUILD_ID` bertanggal 11.30.28, sesudah perubahan source terakhir, dan memuat kode baru (diperiksa agent). **Uji layar U1–U10 dijalankan pemilik** lewat Playwright pukul 12.14–12.15: 7 `PASS`, U9 dan U10 `PASS` lewat simulasi `page.route`, dan **U3 `NOT RUN`** karena dev tidak punya pasien tanpa identitas (kriteria 3 dibuktikan unit test; tampilan layarnya diserahkan ke UAT). UAT belum dijalankan. *Sebelumnya: 🟡 30 September 2026 pagi — menunggu build dan uji layar* |

---

## 1. Keadaan yang ditemukan di awal

Daftar *Triage Pasien* membaca `GET /emergency-visits`, yaitu daftar **kunjungan** IGD. Akibatnya ada tiga masalah:

1. **Pasien yang baru terdaftar tetapi belum punya kunjungan tidak pernah tampil.** Pada rancangan encounter-first
   (`IGD-DEC-139`), loket hanya membuat encounter, dan kunjungan baru lahir saat perawat menekan Mulai Triage. Tanpa
   task ini, pasien seperti itu tidak terlihat oleh perawat triage.
2. **Kolom "Tanggal Daftar" sebenarnya menampilkan waktu tiba.** `getEncounterDate` membaca `arrivalDateTime` lebih
   dulu, sehingga dua waktu yang berbeda arti tampil di bawah satu judul.
3. **Tiga filter tidak bekerja.** Filter "Status Encounter" disimpan sebagai `encounterStatus`, padahal thunk
   mengirim `visitStatus`, jadi pilihannya tidak pernah sampai ke backend. Filter tanggal awal dan akhir tidak dikenal
   `triage-queue` (kontrak §8.3.1 hanya menerima `page`, `pageSize`, `search`, `queueStatus`).

Ada juga dua sisa kecil. Kolom "Unit Layanan" akan kosong selamanya karena `triage-queue` tidak mengirim ruas unit.
Kunci baris memakai `encounterId || id`, padahal baris baru tidak punya `id`.

Backend `BE-IGD-054` sudah menyediakan `GET /emergency-visits/triage-queue`. Agent membaca source-nya langsung
(`EmergencyVisitService.GetTriageQueueAsync`, `EmergencyVisitDtos.EmergencyTriageQueueRowResponse`) dan menemukan
tiga fakta yang menentukan bentuk layar:

| Fakta backend | Akibat bagi layar |
| --- | --- |
| `visitStatus` dikirim sebagai **angka** (tidak ada `JsonStringEnumConverter`), `queueStatus` sebagai **teks** | Label status dibaca dari `visitStatus` lewat peta label yang sudah ada; filter mengirim teks `queueStatus` |
| Kunjungan yang belum `Completed`/`Cancelled` **semuanya** ikut tampil, bukan hanya yang menunggu triage; `availableActions` kosong bagi yang sudah lewat triage | Baris kunjungan tanpa `FillTriage` tetap butuh tombol baca **Lihat Riwayat** (sama seperti hari ini, dan sejalan dengan tombol *[Buka]* pada skema 03 §13.3 B) |
| Halaman dibaca dari `page`, bukan `pageNumber` | Thunk menerjemahkan `pageNumber` state menjadi `page` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** perawat triage IGD.

1. Perawat membuka menu **Instalasi Gawat Darurat → Triage Pasien**.
2. Layar memuat satu daftar yang memuat **dua jenis pasien** sekaligus, satu baris per pasien:
   - **Pasien yang baru didaftarkan di loket dan belum punya kunjungan IGD.** Waktunya tampil sebagai
     **"Terdaftar 09.35"**, statusnya badge kuning **Menunggu triage**, dan kolom aksi bertuliskan *Belum ada
     kunjungan IGD*.
   - **Pasien yang sudah punya kunjungan IGD.** Waktunya tampil sebagai **"Tiba 09.20"**. Badge statusnya kuning
     bila masih perlu ditriage, hijau-toska bila sedang diproses, dan hijau bila tindak lanjutnya sudah ditetapkan.
3. Di bawah jam tercetak tanggalnya, misalnya *30 Sep 2026*, supaya pasien yang masih tertahan sejak kemarin tidak
   tertukar dengan pasien hari ini.
4. Keterangan filter menjelaskan beda kedua waktu dengan kalimat biasa: *Waktu Terdaftar adalah saat pasien
   didaftarkan di loket. Waktu Tiba adalah waktu kedatangan yang tercatat pada kunjungan IGD.*
5. Tombol pada baris kunjungan mengikuti `availableActions` dari backend:

   | Keadaan baris | Tombol |
   | --- | --- |
   | Kunjungan `Arrived` / `WaitingForTriage` | **Isi Triage** dan **Tangani Segera** — persis seperti `FE-IGD-029`/`030` |
   | Kunjungan yang sudah lewat triage | **Lihat Riwayat** |
   | Kunjungan lama tanpa encounter | **Isi Triage/Lihat Riwayat** tampil nonaktif, dan tooltip menjelaskan kunjungannya belum tertaut ke pendaftaran |
   | Baris tanpa kunjungan | Tidak ada tombol; tulisan *Belum ada kunjungan IGD*. Mulai Triage, Tangani Segera, dan Pergi sebelum ditriage dipasang oleh `FE-IGD-036` dan `FE-IGD-039` |

6. Perawat dapat **mencari** nama pasien, No. RM, nomor registrasi, atau nomor kunjungan, dan **menyaring** status
   lewat pilihan *Semua status*, *Menunggu triage*, *Pasien tiba*, *Sudah ditriage*, *Sedang ditangani*,
   *Dalam observasi*, *Menunggu keputusan*, dan *Tindak lanjut ditetapkan*. *Selesai* dan *Dibatalkan* sengaja
   tidak ada karena backend tidak pernah menampilkannya di daftar ini.

**Contoh.** Pukul 09.35 RAYYAN didaftarkan di loket tanpa kunjungan, sedangkan BUDI sudah punya kunjungan IGD-0011
dengan waktu tiba 09.20. Daftar memuat:

| Nama pasien | Waktu | Status | Aksi |
| --- | --- | --- | --- |
| Rayyan Dhafir · REG-20260930-0012 | **Terdaftar 09.35** · 30 Sep 2026 | 🟨 Menunggu triage | *Belum ada kunjungan IGD* |
| Budi Santoso · IGD-0011 | **Tiba 09.20** · 30 Sep 2026 | 🟨 Menunggu triage | [Isi Triage] [Tangani Segera] |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Filter/pencarian tidak menemukan pasien | *Tidak ada pasien yang cocok dengan pencarian ini.* + saran mencoba kata kunci/status lain |
| Tidak ada pasien sama sekali | *Tidak ada pasien yang menunggu triage.* |
| Gagal memuat | Spanduk merah berisi pesan backend apa adanya, atau *Daftar triage gagal dimuat.* bila backend tidak memberi pesan (misalnya jaringan putus), beserta tombol **Coba lagi** |
| Tanpa hak `EmergencyVisit : Read` | *Anda tidak memiliki hak akses untuk melihat data ini.* — tanpa tombol Coba lagi |
| Pasien tanpa identitas | Nama rekam pengganti tampil apa adanya, dan baris kedua menuliskan *Tanpa identitas · nama sementara …* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `QuilvianSystemFrontendDev/AGENTS.md`, `CLAUDE.md`; `rules/frontend/frontend-architecture.md`,
  `base-component-catalog.md`, `base-component-decision-gate.md`, `ui-consistency-checklist.md`, `test-policy.md`,
  `page-composition-patterns.md` §6, `REPORT_TEMPLATE.md`; `rules/rule-output/status-task-roadmap.md`.
- Blueprint: kartu `FE-IGD-035` dan R3.12.2 pada frontend roadmap; `contracts/api-contract.md` §8.1, §8.3, §8.3.1,
  §9.2; `03-frontend-architecture.md` §13.1–§13.7; backend roadmap R3.13.4–R3.13.6.
- Backend (baca saja): `EmergencyVisitController.cs` (`TriageQueue`, `[Tags]`, hak akses),
  `EmergencyVisitDtos.cs`, `EmergencyVisitService.cs` (`GetTriageQueueAsync`, `AksiBarisAntrean`,
  `ResolveNamaPasienAntrean`), `Responses/PagedResult.cs`, enum `EmergencyVisitStatus`.
- Frontend: route `emergency-triage/page.jsx` dan `triage-client.jsx`; view dan tabel daftar triage; slice, hook,
  constant, dan utils triage; `emergency-registration.constants.js` dan `.utils.js`; `status-badge.jsx`,
  `data-table.jsx`, `filter-select.jsx`; modul CSS triage; layar daftar pengkajian IGD (modul referensi visual).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-management-triage-constant.jsx` | `DEFAULT_TRIAGE_PATIENT_FILTERS` diringkas menjadi ruas kontrak (`search`, `queueStatus`, `pageNumber`, `pageSize`). Konstanta baru: `TRIAGE_QUEUE_MAX_PAGE_SIZE` (100), `TRIAGE_QUEUE_ACTION`, `TRIAGE_QUEUE_STATUS`, dan `TRIAGE_QUEUE_STATUS_OPTIONS` (labelnya dari `EMERGENCY_VISIT_STATUS_LABELS` yang sudah ada). Placeholder pencarian menyebut keempat ruas yang benar-benar dicari backend |
| `src/utils/health-services/emergency-installation-management/emergency-management-triage-utils.jsx` | Fungsi murni baru: `buildTriageQueueParams`, `getTriageQueueRowKey`, `getTriageQueueVisitId`, `getTriageQueueEncounterId`, `hasTriageQueueVisit`, `hasTriageQueueAction`, `getTriageQueuePatientName`, `getTriageQueueReferenceNumber`, `getTriageQueueIdentityNote`, `resolveTriageQueueTime`, `formatTriageQueueMoment`, `resolveTriageQueueStatus`. Fungsi lama tidak diubah |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-management-triage-slice.jsx` | `fetchEmergencyTriagePatients` pindah ke `GET …/emergency-visits/triage-queue`, meneruskan `signal`, dan membentuk parameter lewat `buildTriageQueueParams`. Permintaan yang dibatalkan tidak menimpa layar. Pesan gagal memakai pesan backend, atau *Daftar triage gagal dimuat.* |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-management-triage/use-emergency-management-list.jsx` | Permintaan lama dibatalkan saat filter/halaman berubah, supaya jawaban yang terlambat tidak menimpa halaman yang sedang dibuka. Pemuatan metadata encounter dihapus karena filter "Status Encounter" sudah tidak ada. Hook kini menyediakan `queueStatusOptions` |
| `src/components/view/.../components/emergency-triage-patient-table.jsx` | Kolom baru **WAKTU** (label Tiba/Terdaftar + tanggal) menggantikan "Tanggal Daftar". Kolom "Unit Layanan" dihapus. Status memakai `StatusBadge` berwarna. Tombol diturunkan dari `availableActions`; baris tanpa kunjungan bertuliskan *Belum ada kunjungan IGD*. `rowKey` memakai `rowKey` backend. Klik dua kali hanya membuka triage untuk baris yang memang bisa dibuka. Teks kosong dibedakan antara "belum ada pasien" dan "tidak cocok dengan filter" |
| `src/components/view/.../emergency-triage-patient-list-view.jsx` | Filter tanggal dan "Status Encounter" diganti satu filter **Status** (`queueStatus`). Keterangan filter menjelaskan beda Terdaftar dan Tiba. Pembaca id memakai util baru (`encounterId`, `emergencyVisitId`), bukan `id` |
| `tests/unit/emergency-visit-status.test.mjs` | 13 kasus baru untuk baris tanpa kunjungan dan baris kunjungan (03 §13.7) |

**Kode baru tanpa satu pun baris komentar**, sesuai arahan pemilik. Diperiksa dengan grep pada baris tambahan diff:
0 baris `//`, `/*`, atau `*`. Komentar lama pada bagian berkas yang tidak disentuh dibiarkan (di luar lingkup).
Komentar lama yang menempel pada kode yang diganti ikut terhapus karena isinya sudah tidak benar (misalnya
*"Daftar bersumber dari kunjungan IGD, sehingga id adalah id kunjungan"*). Alasan setiap keputusan ditulis di laporan
ini, bukan di source.

### 3.3 Kepatuhan arsitektur frontend

- **Alur dependensi utuh:** view → hook → slice (thunk + `InstanceAxios`) → backend. Normalisasi baris ada di `utils`
  sebagai fungsi murni, konstanta ada di `constants`, dan view tidak memanggil Axios. Tidak ada Axios instance,
  slice, atau hook baru.
- **Abort signal** diteruskan dari `createAsyncThunk` ke Axios, sesuai checklist arsitektur.
- **Modul referensi visual:** layar daftar pengkajian IGD (`emergency-assessment-list-view.jsx`) dan layar triage itu
  sendiri, yang sama-sama memakai `emergency-triage.module.css`. Kerangka halaman (judul, tombol Muat Ulang, spanduk)
  **tidak diubah**, supaya kedua layar IGD tetap terbaca sebagai satu aplikasi.

**Gerbang keputusan base component:**

`UI GATE: 6 elemen — REUSE 4, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Judul halaman, Muat Ulang, spanduk galat/berhasil | Gaya layar IGD yang ada | `emergency-triage.module.css` `.pageHeading`, `.errorBanner`, `.successBanner` | REUSE | Tidak diubah |
| Pencarian + filter Status | `DataFilter`, `FilterSelect` | `base-features/data-filter.jsx`, `filter-select.jsx`; sudah dipakai layar ini | REUSE | Opsi di `constants` domain, entri pertama bernilai kosong |
| Tabel + paginasi | `DataTable` | `base-features/data-table.jsx`; `rowKey(item, index)` diperiksa di baris 251 | REUSE | `rowKey` dari backend, `sortLatestFirst={false}` karena backend sudah mengurutkan |
| Badge status berwarna | `StatusBadge` | `base-features/status-badge.jsx`, dipakai 270 view | REUSE | `status` = `warning` / `info` / `active`, `showIcon={false}` |
| Waktu dua baris "Tiba 09.20 / 30 Sep 2026" | Pola dua baris `.patientCell` | Sudah dipakai sel nama pasien pada tabel yang sama | COMPOSE | Pilihan A di bawah |
| Keterangan *Belum ada kunjungan IGD* | Teks kecil `.patientCell span` | Sama | COMPOSE | Pilihan A di bawah |

**Keputusan untuk dua baris COMPOSE** (disampaikan ke pemilik sebelum kode ditulis):

- **A. Rangkai dari kelas yang sudah ada — Rekomendasi, dijalankan.** Nol CSS baru (kriteria 7), dan tampilannya
  identik dengan sel nama pasien, jadi mata perawat membaca kedua kolom dengan cara yang sama.
- **B. Tambah kelas CSS baru di modul triage.** Bentuknya lebih bebas, tetapi melanggar syarat "nol CSS baru" dan
  menambah gaya yang harus dirawat.

**Satu pergeseran visual yang disengaja:** badge status kini `StatusBadge` berwarna (kuning = perlu ditriage,
hijau-toska = sedang diproses, hijau = tindak lanjut ditetapkan). Sebelumnya semua status memakai satu badge
hijau-toska `.statusBadge`. Alasannya adalah arahan pemilik agar layar mudah dipahami: pasien yang perlu disentuh
langsung menonjol tanpa membaca teks. `StatusBadge` adalah komponen status baku katalog. Layar daftar pengkajian IGD
masih memakai badge lama; menyamakannya **bukan** lingkup task ini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel dari `DataTable` dengan teks *Mengambil data pasien triage...*; tombol Muat Ulang nonaktif |
| Kosong | *Tidak ada pasien yang menunggu triage.* — *Pasien yang didaftarkan di loket IGD akan muncul di sini.* Bila ada filter/pencarian: *Tidak ada pasien yang cocok dengan pencarian ini.* — *Coba kata kunci atau status lain, atau tekan tombol reset filter.* |
| Gagal | Spanduk merah dengan pesan backend apa adanya, atau *Daftar triage gagal dimuat.*, beserta tombol **Coba lagi** |
| Tanpa hak akses | *Anda tidak memiliki hak akses untuk melihat data ini.* — tanpa tombol Coba lagi, karena mencoba lagi hanya menghasilkan penolakan yang sama |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/triage-queue?page=&pageSize=&search=&queueStatus=` | Satu-satunya sumber daftar *Triage Pasien* | `EmergencyVisit : Read` |
| `PATCH` | `/{id}/visit-status` | Tangani Segera pada baris kunjungan — **tidak berubah** dari `FE-IGD-030` | `EmergencyVisit : Update` |

**Tidak lagi dipanggil dari layar daftar:** `GET /emergency-visits` dan metadata filter encounter milik Registrasi.
Endpoint `GET /emergency-visits` tetap ada dan tetap dipakai layar lain.

**Contoh permintaan.** Perawat membuka halaman 2 dan memilih status *Menunggu triage*:

```http
GET /api/v1/health-services/emergency-installation-management/emergency-visits/triage-queue?page=2&pageSize=10&queueStatus=WaitingForTriage
```

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` tujuh berkas task | 0 error, 0 warning (exit 0) | `PASS` | Dijalankan agent 30 September 2026, ulang sesudah perubahan terakhir |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-visit-status.test.mjs` | 19/19 lulus (6 lama + 13 baru) | `PASS` | Dijalankan agent |
| Enam berkas test IGD (`emergency-visit-status`, `emergency-triage-utils`, `emergency-registration-existing-visit`, `emergency-registration-payload`, `emergency-assessment-summary`, `emergency-departure-event-actor-name`) | 70/70 lulus | `PASS` | Dijalankan agent |
| `npm run test:unit` (suite penuh) | 1812 test: 1805 lulus, **7 gagal** | `UNRELATED EXISTING ISSUE` | Ketujuhnya di `inpatient-physician-entry`, `inpatient-physician-workspace`, `menu-permission-filter` (Bank Darah), dan `petty-cash-finance-separation`. Tidak satu pun berkas itu menyebut IGD/triage |
| Grep baris komentar baru pada diff | 0 baris | `PASS` | `git diff -U0` baris `+` difilter `//`, `/*`, `*` |
| Grep anti-regresi UI pada tabel dan view | Nol warna literal, nol `font-size`/`font-weight` baru, nol `<table>`, nol `fw-*`/`fs-*`, nol `!important`, nol inline style. Dua `<button>` di tabel **dipertahankan** | `PASS` dengan catatan | `<button className={styles.primaryMiniButton}>` adalah tombol yang sama dengan `FE-IGD-029`/`030` dan layar pengkajian IGD. Menggantinya dengan `BaseButton` akan membuat tombol daftar triage berbeda dari layar IGD lainnya |
| Akhir baris | Tujuh berkas seluruhnya CRLF, seragam dengan `HEAD` | `PASS` | Dihitung byte CR/LF per berkas |
| `npm run build` | Lulus menurut pemilik; artefak cocok | `PASS` (pernyataan owner + artefak) | Pemilik menyatakan build sudah dijalankan (30 September 2026). Diperiksa agent: `.next/BUILD_ID` `6d4a9YHDdB2pxFYhn6uU9` bertanggal **11.30.28**, sesudah perubahan source terakhir (slice, 11.23.42); teks baru *Belum ada kunjungan IGD* ada di chunk build `.next/static/chunks/2gyrepk9gsqws.js` dan chunk SSR daftar triage. Keluaran perintah dan jumlah warning tidak dilampirkan |
| Uji layar U1–U10 | 7 `PASS`, 2 `PASS` lewat simulasi (U9, U10), 1 `NOT RUN` (U3) — rincian 6.1 | lihat 6.1 | Dijalankan pemilik lewat Playwright (Antigravity) 30 September 2026 12.14–12.15 WIB. Bukti mentah diperiksa agent: `test-with-agy/igd/u1_u10_test_results.json` (cap waktu per skenario), skrip `test-u1-u10-fe-igd-035.mjs`, dan tangkapan layar `u7_filter_waiting_for_triage.png`. Ringkasan agen `testing/2026-09-30-laporan-uji-layar-u1-u10-fe-igd-035.md` **bukan** bukti — ia menandai U3 `PASS` padahal JSON mencatat `u3Found: false`, menyebut akun `sysadmin_test` padahal skrip masuk sebagai `superadmin@admin.com`, dan menyebut route `/emergency-triages` yang tidak ada |

Uji layar berjalan pada `next dev`: tangkapan layar memuat lencana "N" di kiri bawah. Source yang dilayani sama
dengan source yang di-build (tidak ada perubahan sesudah 11.23.42), jadi hasilnya berlaku untuk kode task ini.

### 6.1 Skenario uji layar — hasil dari bukti mentah

| # | Langkah | Yang diharapkan | Kriteria | Hasil | Bukti dari JSON/PNG |
| ---: | --- | --- | ---: | --- | --- |
| U1 | Buka *Triage Pasien* dengan tab Network terbuka | Hanya `GET …/emergency-visits/triage-queue?page=1&pageSize=10`; **tidak ada** `GET …/emergency-visits?` maupun permintaan metadata encounter | 1 | `PASS` | Listener `page.on('request')`: `triageQueueCallsCount` 1, `oldEmergencyVisitsCallsCount` 0, `encounterMetadataCallsCount` 0 |
| U2 | Encounter IGD tanpa kunjungan | *Menunggu Triage* (kuning), **"Terdaftar hh.mm"**, *Belum ada kunjungan IGD*; klik dua kali tidak membuka apa pun | 2 | `PASS` | RAYYAN DHAFIR PRASETYA MAULANA `ENC-RSMMC-00181`: "Terdaftar 13.51 21 Sep 2026", "Menunggu Triage", "Belum ada kunjungan IGD"; URL tidak berubah sesudah `dblclick` |
| U3 | Pasien tanpa identitas yang kunjungannya ada | Nama rekam pengganti apa adanya + *Tanpa identitas · nama sementara …* | 3 | `NOT RUN` | `u3Found: false` — dev tidak punya pasien tanpa identitas yang kunjungannya masih terbuka. Kriteria 3 dibuktikan unit test (*rekam pengganti tampil apa adanya*); tampilan layarnya diserahkan ke UAT |
| U4 | Kunjungan menunggu triage | **"Tiba hh.mm"** dengan **Isi Triage** dan **Tangani Segera**; Tangani Segera → konfirmasi | 4 | `PASS` | ANDRE PRATAMA `IGD-260918025407-7C8014`: "Tiba 09.53 18 Sep 2026", kedua tombol ada; dialog *Tangani pasien sekarang?* terbuka lalu **dibatalkan** (tidak ada data yang diubah). Lompatan ke Assesmen IGD sesudah konfirmasi tidak dijalankan — perilaku itu tidak diubah task ini (`FE-IGD-030`) |
| U5 | Pasien sudah ditriage/sedang ditangani | Badge hijau-toska dan **Lihat Riwayat** | 4 | `PASS` | AGNES YULIANI RAJA GUK GUK: "Sudah Ditriage" + Lihat Riwayat; klik membuka `…/emergency-triage/agnes-yuliani-raja-guk-guk-bc33ad4d297b` |
| U6 | Halaman 1 → 2 → 1 | Tidak ada pasien berulang/terlewat | 5 | `PASS` | "Menampilkan 1 sampai 10 dari 14 data" → "11 sampai 14 dari 14 data" → kembali "1 sampai 10"; baris pertama tiap halaman berbeda |
| U7 | Filter *Menunggu triage*, lalu *Sedang ditangani*, lalu reset | Kedua jenis baris pada filter pertama; hanya baris kunjungan pada filter kedua; reset mengembalikan semua | 1, 5 | `PASS` | Permintaan `…&queueStatus=WaitingForTriage` → 7 baris (1 kunjungan ANDRE + 6 baris tanpa kunjungan — dicocokkan dengan PNG); `InTreatment` → 1 baris kunjungan; reset → 10 baris halaman 1 |
| U8 | Cari nama, No. RM, nomor registrasi, nomor kunjungan | Baris cocok tampil; kata acak → teks kosong | 6 | `PASS` (tiga dari empat ruas) | "ANDRE", "00-00-00-11", "ENC-RSMMC-00181" masing-masing menemukan barisnya; "XYZ9999NONEXISTENT" → *Tidak ada pasien yang cocok dengan pencarian ini.* Pencarian **nomor kunjungan** tidak dijalankan |
| U9 | Galat server, lalu Coba lagi | Spanduk galat + **Coba lagi**; pulih sesudah diklik | 6 | `PASS` (simulasi) | Respons 500 dipalsukan `page.route` dengan pesan sendiri; spanduk menampilkan pesan backend itu apa adanya + Coba lagi; sesudah `unroute`, Coba lagi memuat 10 baris. Jalur tanpa pesan backend (jaringan putus → *Daftar triage gagal dimuat.*) tidak teramati |
| U10 | Pengguna tanpa `EmergencyVisit : Read` | Pesan hak akses tanpa Coba lagi | 6 | `PASS` (simulasi) | Respons 403 dipalsukan `page.route`; spanduk *Anda tidak memiliki hak akses untuk melihat data ini.*, `hasCobaLagiOnForbidden: false`. Bukan pengguna nyata tanpa hak |

**Penutup butir terbuka `BE-IGD-054`.** Uji `BE-IGD-054` belum menyatakan terpisah saringan `queueStatus` dua arah.
U7 kini membuktikannya lewat layar: `WaitingForTriage` memuat baris tanpa kunjungan **dan** kunjungan menunggu
triage, sedangkan `InTreatment` hanya memuat baris kunjungan.

**Temuan data (bukan cacat layar).** Filter *Menunggu triage* memperlihatkan **enam encounter IGD tanpa kunjungan**:
empat milik MIRA SETIAWAN (`ENC-RSMMC-00160`, `00161`, `00169`, `00171`), satu milik BAGUS SETIAWAN (`00162`), dan
satu milik RAYYAN (`00181`). RAYYAN sekaligus masih punya kunjungan `IGD-260917023643-4A7A93` berstatus *Sudah
Ditriage*. Ini daftar kerja K3 dari rekonsiliasi `BE-IGD-052` (K3 tidak pernah ditutup otomatis, `IGD-DEC-148`).
Encounter yang memang duplikat perlu dibatalkan petugas pendaftaran lewat `PATCH /patient-encounters/{id}/cancel`
sebelum `FE-IGD-036` dirilis. Tanpa itu, begitu tombol Mulai Triage tersedia, MIRA akan tampil empat kali dengan
tombol aktif.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria (persis roadmap) | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar triage memanggil **satu** endpoint (`triage-queue`); nol `GET /emergency-visits` dari layar daftar (panel network) | Terpenuhi | Source (thunk `EMERGENCY_TRIAGE_QUEUE_URL`, hook tanpa metadata) + U1 (1 panggilan `triage-queue`, 0 `GET /emergency-visits?`, 0 metadata) + U7 (`queueStatus` terkirim) |
| 2 | Pasien tanpa kunjungan tampil *Menunggu Triage* dengan label "Terdaftar hh.mm" | Terpenuhi | Source + unit test + U2. Label tampil **"Menunggu Triage"** persis 03 §13.5, karena aturan global `.badge { text-transform: capitalize }` (`src/style/style.css`) mengapitalkan teks peta label |
| 3 | Pasien rekam pengganti tampil dengan nama rekamnya apa adanya; alias sementara tampil bila kunjungannya ada | Terpenuhi di source dan unit test; **tampilan layar tidak teramati** | `getTriageQueuePatientName`, `getTriageQueueIdentityNote`; test *rekam pengganti tampil apa adanya*. U3 `NOT RUN` — dev tidak punya datanya. Diserahkan ke UAT, pola yang sama dengan `FE-IGD-027` (baris legacy "Data historis") |
| 4 | Tombol baris kunjungan tetap **Isi Triage** dan **Tangani Segera** sesuai `FE-IGD-029`/`030` | Terpenuhi | Source + unit test + U4 (kedua tombol, dialog konfirmasi) + U5 (Lihat Riwayat membuka Detail triage) |
| 5 | Halaman berikutnya tidak mengulang atau melompati pasien | Terpenuhi | U6 (14 data: 10 + 4, baris pertama tiap halaman berbeda, kembali ke halaman 1 identik) |
| 6 | Memuat / kosong / gagal sesuai skema 03 §13.3 B | Terpenuhi (gagal dan tanpa hak lewat simulasi) | U8 (teks kosong), U9 (500 disimulasikan + Coba lagi + pulih), U10 (403 disimulasikan, tanpa Coba lagi) |
| 7 | `eslint` berkas task 0 error; unit test lulus (`emergency-visit-status.test.mjs` + kasus baris tanpa kunjungan); nol CSS baru; `npm run build` dan uji layar — milik pemilik | Terpenuhi | `eslint` 0 error; 19/19 dan 70/70; nol CSS baru; `npm run build` (pernyataan owner + artefak `BUILD_ID` 11.30.28); uji layar 12.14–12.15 (bagian 6.1) |

**DoD.** Acceptance 1–7 terpenuhi, dengan satu butir tampilan (kriteria 3, U3) yang tidak dapat diamati di dev dan
diserahkan ke UAT. Laporan tracked ✅; roadmap dan traceability diperbarui ✅. Status: **✅ SELESAI 30 September 2026
atas penilaian pemilik.** UAT belum dijalankan — tim UAT terpisah; agent tidak menulis `UAT PASS`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Jangan rilis layar ini sendirian.** Usulan rilis R3.12.2 menggabungkan `FE-IGD-035`, `FE-IGD-036`, dan `FE-IGD-039` supaya baris *Menunggu triage* tanpa kunjungan tidak pernah tampil tanpa tombol. Selama loket masih membuat kunjungan (sebelum `FE-IGD-036`), baris tanpa kunjungan hanya muncul untuk encounter yatim/K3, dan baris itu bertuliskan *Belum ada kunjungan IGD* tanpa aksi |
| Masalah yang diketahui | (1) ~~Label *Menunggu triage* berhuruf kecil~~ — **gugur.** Aturan global `.badge { text-transform: capitalize }` di `src/style/style.css` membuat badge tampil **"Menunggu Triage"**, persis 03 §13.5 (teramati pada U2 dan U7). (2) Thunk `fetchPatientEncounterFilterMetadata` dan util `buildEncounterStatusOptions` kini tidak dipanggil siapa pun. Keduanya dibiarkan supaya diff tetap sempit; kandidat bersih-bersih. (3) Impor `EmergencyTriageSlaBreachPanel` yang tidak terpakai sudah ada sebelum task ini (panelnya dikomentari di `HEAD`); tidak disentuh. (4) Panel pelanggaran SLA tidak diubah; SLA dihitung sejak mulai triage (`ResponseDueAt`), jadi baris tanpa kunjungan memang belum ikut. (5) Enam encounter K3 di dev (bagian 6.1, *Temuan data*) perlu dibereskan petugas pendaftaran sebelum `FE-IGD-036` dirilis. (6) Ringkasan agen uji `testing/2026-09-30-laporan-uji-layar-u1-u10-fe-igd-035.md` memuat baris akun **beserta kata sandinya** (`sysadmin_test`, akun yang bahkan tidak dipakai skrip). Baris itu sebaiknya dihapus sebelum di-commit |
| Dependency backend | `BE-IGD-054` ✅. Tidak ada dependency backend lain untuk task ini |
| Perubahan sampingan | Dua berkas sempat tertulis dengan akhir baris LF (`emergency-triage-patient-table.jsx` utuh, `emergency-management-triage-utils.jsx` pada bagian tambahan), lalu diseragamkan kembali ke CRLF seperti `HEAD`. Tidak ada perubahan lain di luar tujuh berkas |
| Interupsi | Dua arahan pemilik masuk di tengah task (tampilan mudah dipahami dan bagus; kode tanpa baris komentar). Keduanya diterapkan: badge berwarna, label waktu yang jelas, keterangan filter, teks kosong yang dibedakan, dan nol baris komentar baru |
| Status Git | Frontend `RizkiV2`: tujuh berkas `M` (bagian 3.2), belum di-commit; folder `test-with-agy/` di-ignore `.gitignore`. Backend: laporan ini, pembaruan roadmap/traceability, dan dua ringkasan agen uji di `testing/` (`??`), belum di-commit |
| Langkah berikutnya | Task berikutnya menurut urutan yang disepakati: `BE-IGD-060` (R3.14). Layar ini **jangan dirilis sendirian** — tunggu `FE-IGD-036` dan `FE-IGD-039` (usulan rilis R3.12.2) |
