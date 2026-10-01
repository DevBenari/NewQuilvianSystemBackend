# Laporan Perubahan Frontend — `FE-LAB-44`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-44` |
| Judul | Layar Laporan Operasional |
| Slice | Gelombang `MVP-11b` — `EPIC-LAB-17`, `S16a` tiga laporan operasional |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-11`, bagian `FE-LAB-44` |
| Trace | `FR-17.10`; bagian tampilan `FR-17.1`..`FR-17.3`, `FR-17.5`, `FR-17.6`, `FR-17.9`; `LAB-DEC-159`, `LAB-DEC-160`; `ARCH-GAP-LAB-11`, `ARCH-GAP-LAB-13`; A7.12; `02-backend-architecture.md` 23.10 butir 4, 6, 8; `03-frontend-architecture.md` amandemen 2026-09-25 (keempat) |
| Contract version | `LAB-API-v1` **`r37`** bagian 32, `LAB-VAL-v1` **`r15`** (`VAL-147`..`VAL-149`), `LAB-PERM-v1` **revision 12** — ketiganya **`approved` 2026-09-28** |
| Wewenang UI | Disetujui: teks *belum dapat dihitung* (`ARCH-GAP-LAB-11`), nol daftar pasien (A7.12), butir menu tersendiri di menu Laboratorium (23.10 butir 8). `DEV_DISCRETION` yang dipakai: tiga bagian bertumpuk (bukan tab), tabel (bukan grafik), menit satu desimal (*48,6 menit*), periode bawaan tanggal 1 bulan berjalan sampai hari ini |
| Dependency | `BE-LAB-83` ✅, `BE-LAB-84` ✅, `BE-LAB-85` ✅ (verifikasi); `BE-LAB-86` ✅ milik `FE-LAB-45`. Nol task frontend |
| Klasifikasi | `MEDIUM` — satu layar baru baca saja: 9 berkas baru, 2 berkas diubah, 4 endpoint baca, satu butir menu berizin; nol Redux slice, nol dependency baru, nol CSS baru |
| Task mode | `FRONTEND` — backend baca saja |
| Target tulis | `QuilvianSystemFrontendDev` (source); `NewQuilvianSystemBackend` hanya laporan ini serta tautan buktinya pada roadmap frontend dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `68195b2be` (branch `YogaV2`, upstream `origin/YogaV2`). **Impact scan** dari `696a906a6` (SHA roadmap): 6 commit, seluruhnya Gizi; nol berkas Laboratorium. `menu-items.jsx` ikut berubah oleh butir Gizi — tidak bersinggungan dengan butir baru |
| Commit backend yang dijadikan rujukan | `7ff35b8c` (branch `yoga`) beserta perubahan `BE-LAB-84`..`86` yang belum ter-commit — sumber bentuk ruas `LabOperationalReportDtos.cs` dan judul kolom `LabReportCsvWriter.cs` |
| Tanggal | 2026-09-30 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — layar, butir menu, service, hook, dan berkas aturan terbangun. Uji unit **17/17**, e2e layar **8/8**, lint dan build hijau. Layar hasil build diberi **respons sungguhan** backend lokal terhadap PostgreSQL dev (September 2026, `422`, `400`) dan menampilkannya benar. **Batas:** layar belum dijalankan tersambung langsung ke backend dengan akun asli — build frontend mengarah ke API dev bersama yang belum menerima `MVP-11a`, dan belum ada jabatan pemegang `LabOperationalReport : Read` (langkah rilis `MVP-11c` 0-1) |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Belum ada layar, route, service, atau butir menu laporan operasional | `git ls-files` — nol berkas `lab-operational*` |
| Backend ketiga laporan sudah berdiri dan terbukti lewat HTTP | `BE-LAB-83`..`86` ✅ |
| Pola layar laporan yang dapat dijadikan acuan | *Laporan Penerimaan* (`FE-LAB-12`): route tipis, client wrapper, view, hook, dan berkas aturan murni teruji |
| Menu Laboratorium belum disaring izin sama sekali | Komentar `menu-items.jsx` pada blok Laboratorium: *"Penyaringan menu per izin belum ditegakkan"* |
| Properti `permission:` pada butir Hemodialisa **tidak dibaca** fungsi mana pun | `filterMenuItemsByRole` menerima `userPermissions` tetapi tidak memakainya; nol pembaca `.permission` di `src/utils/menu-sidebar` |
| `FilterDatePicker` sudah menerima `max` | `filter-date-picker.jsx:419` — **katalog base component basi**: prop ini tidak tercantum |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa:** kepala instalasi laboratorium dan — setelah jabatannya ditetapkan — manajemen. Analis tidak
memakainya.

**Alur normal:**

1. Kepala instalasi membuka menu **Laboratorium → Laporan Operasional**.
2. Layar langsung terbuka dengan periode **1 bulan berjalan sampai hari ini** dan *Semua Disiplin*;
   ketiga laporan dimuat sendiri-sendiri.
3. **Jumlah Pemeriksaan** — berapa hasil yang dirilis per disiplin, total terhitung, lalu rincian per jenis
   pemeriksaan. Contoh: *Patologi Klinik 5*; rincian *Kalium 3*, *Natrium 2*.
4. **Penolakan Wadah** — wadah diputuskan, wadah ditolak, dan angka penolakannya, lalu rincian alasan.
   Contoh: 400 diputuskan, 12 ditolak → **3,0%**; rincian *Hemolisis 8*, *Volume kurang 4*.
5. **Waktu Penyelesaian** — per disiplin, cito dan rutin terpisah: jumlah dirilis, rata-rata menit, terlambat,
   dan cito tanpa batas. Contoh: cito *412 hasil, 48,6 menit, 19 terlambat*, dengan keterangan
   *"3 pemeriksaan cito belum punya batas waktu"*.
6. Kepala instalasi dapat mengganti periode (tanggal masa depan tidak dapat dipilih) atau memilih satu disiplin;
   ketiga laporan dibentuk ulang. Tombol ↻ mengembalikan periode bawaan; **Muat ulang** meminta ulang.

**Setiap bagian menyebut periode yang benar-benar dipakai backend** (*"Periode 1 September 2026 - 30 September
2026"*) dan kapan laporannya dibentuk, supaya angka tidak terbaca lepas dari periodenya.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Disiplin belum dapat dihitung (Patologi Anatomi) | Angka ditulis **"—"**, bukan 0; kolom Keterangan: *"Rilis hasil Patologi Anatomi belum tersedia."* |
| Nol wadah diputuskan | Angka penolakan **"—"** dengan sebab *"Nol wadah diputuskan"* — bukan *0,0%* |
| Nol hasil dirilis | Rata-rata **"—"** dengan sebab *"Nol hasil dirilis"*; bagian diberi kalimat *"Tidak ada hasil yang dirilis pada periode ini."* |
| Baris rutin | Kolom *Terlambat* dan *Cito tanpa batas* **"—"** — rutin tidak pernah dinilai terlambat |
| Periode dikosongkan, terbalik, atau lebih dari 366 hari | Pesan backend **apa adanya** tepat di bawah penyaring (*"Periode laporan wajib diisi."* / *"Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya."*); setiap bagian menulis *"Laporan belum dapat dibentuk — Perbaiki periode pada penyaring di atas."*, nol angka |
| Satu laporan lambat | Dua laporan lain tampil lebih dulu; yang lambat menulis *"Membentuk laporan..."* |
| Satu laporan gagal (galat server) | Pesannya hanya pada bagian itu; dua lainnya tetap tampil |
| Tanpa izin laporan | Butir menu tidak tampil; membuka alamatnya langsung → *"Ups! Akses Ditolak"* dengan *"Anda tidak memiliki izin membuka Laporan Operasional Laboratorium."*, nol angka |
| Backend menjawab `403` | Halaman diganti pesan tidak berwenang yang sama, nol angka |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `AGENTS.md` frontend; `rules/frontend/` (arsitektur, katalog dan gerbang base component, komposisi halaman, checklist UI, kebijakan test, template laporan) | Governance |
| `frontend-roadmap.md` `FE-LAB-44`; `03-frontend-architecture.md` amandemen keempat; `api-contract.md` bagian 32; matriks uji *Matriks — layar* | Cakupan, wewenang tampilan, kontrak, dan skenario uji |
| Backend `LabOperationalReportDtos.cs`, `LabOperationalReportController.cs`, `LabOperationalReportService.ResolvePeriod`, `LabFilterMetadataFactory.LabOperationalReport()`, `LabReportCsvWriter.cs` | Nama ruas sebenarnya, kode status, bunyi pesan, format desimal CSV (`"0.0"` berkoma) |
| `lab-reception-reports/*`, `use-lab-reception-report.jsx`, `lab-reception-report-rules.js`, e2e dan uji unitnya | Pola referensi |
| `laboratory-overview-view.jsx` dan kolomnya | Referensi visual — shell, Hero, DataFilter, DataTable tanpa paginasi |
| `lab-monitoring-rules.js` (`todayDateValue`), `date-picker-utils.js` (`toDateInputValue`) | Tanggal lokal, bukan `toISOString()` |
| `use-permission.jsx`, `permission-slice.jsx`, `access-denied-gate.jsx`, `access-denied-utils.jsx`, `InstanceAxios.jsx` | Lapis izin dan bentuk galat |
| `menu-items.jsx`, `filter-menu-items-by-permission.jsx`, `filter-menu-items-by-role.jsx`, `left-sidebar-items-virtualized.jsx` | Penyaringan butir menu |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx` | Diubah — `LABORATORY_API.labOperationalReports` |
| `src/lib/constants/health-services/laboratory-management/lab-operational-report-constants.jsx` | Baru — route, pasangan izin, kunci ketiga laporan, salinan teks, judul kolom **sama dengan judul kolom CSV backend** |
| `src/lib/services/health-services/laboratory-management/lab-operational-report.service.js` | Baru — `getLabOperationalReportMetadata`, `getLabOperationalReport(kunci, params, config)` lewat `InstanceAxios`; unduhan milik `FE-LAB-45` |
| `src/lib/hooks/health-services/laboratory-management/lab-operational-report-rules.js` | Baru — berkas aturan murni: tanggal polos, periode bawaan, parameter, pilihan disiplin, format jumlah/persen/menit, sel *belum dapat dihitung*, kolom cito, sebab angka kosong, nama disiplin alasan, rincian jenis, periode, pemilah galat, pemeriksa *kosong bersebab* |
| `src/lib/hooks/health-services/laboratory-management/use-lab-operational-report.jsx` | Baru — hook: izin, penyaring, metadata, tiga laporan **terpisah** dengan `AbortController`; keadaan memuat diturunkan dari kunci permintaan |
| `src/components/view/health-services/laboratory-management/lab-operational-reports/lab-operational-report-view.jsx` | Baru — komposisi layar |
| `src/components/view/health-services/laboratory-management/lab-operational-reports/lab-operational-report-table-columns.jsx` | Baru — kolom kelima tabel |
| `src/app/health-services/laboratory-management/lab-operational-reports/page.jsx`, `lab-operational-reports-client.jsx` | Baru — route tipis dan client wrapper, pola `LAB-FE-001` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — butir *Laporan Operasional* dengan **`requiredPermission: { resource: "LabOperationalReport", action: "Read" }`**, sesudah *Laporan Penerimaan* |
| `tests/unit/lab-operational-report-rules.test.mjs` | Baru — 17 uji, berjalan pada `TZ=Asia/Jakarta` |
| `tests/e2e/lab-operational-report-screen.spec.mjs` | Baru — 8 skenario layar, pola `lab-reception-report-screen.spec.mjs` |

**Keempat jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| (a) Properti `permission:` pola Hemodialisa | `requiredPermission` — satu-satunya yang dibaca `filterMenuItemsByPermission` | Tangkapan layar: kepala instalasi 1 tautan menu, analis 0 |
| (b) Tanggal lewat `toISOString()` | Nilai `FilterDatePicker` dikirim apa adanya; objek `Date` disusun dari komponen lokal | Uji unit pada WIB: 1 September 00.00 → `2026-09-01` (jalur `toISOString()` → `2026-08-31`); e2e zona `Asia/Jakarta` |
| (c) Angka 0 bagi disiplin yang belum dapat dihitung | `countCell` mengembalikan "—" walau backend mengirim 0 | Uji unit; e2e baris PA sel jumlah = "—" |
| (d) Menghitung ulang persen atau rata-rata | Hanya diformat, satu desimal berkoma seperti `Desimal()` CSV | Uji unit `3.0` → *3,0%*, `48.6` → *48,6 menit* |

**Satu perbaikan saat verifikasi:** tangkapan layar pertama memperlihatkan kolom Keterangan tabel Waktu
Penyelesaian terpotong di kanan pada layar 1440 px (jumlah lebar minimum ±1282 px, ruang ±1083 px) — padahal
sebab *belum dapat dihitung* ada di kolom itu. Lebar kolom dirampingkan menjadi ±1032 px; tangkapan kedua
memperlihatkan kolom utuh.

### 3.3 Kepatuhan arsitektur frontend

- **Alur dependensi:** `page.jsx` → client wrapper → view → hook → service → `InstanceAxios`. View nol
  panggilan Axios; route nol logika.
- **Tanpa Redux:** data laporan hanya dipakai satu halaman — service plus keadaan lokal hook, sesuai
  `frontend-architecture.md` (*service ketika operasi tidak memerlukan Redux global*).
- **Nol komponen baru, nol CSS baru.** Gerbang keputusan base component:

`UI GATE: 9 elemen — REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx`; dipakai Ringkasan Laboratorium | REUSE | Tombol *Muat ulang* pada `actions` |
| Shell halaman | `administrator-region-page` + `base-data-components` + tema region | Sama dengan Ringkasan Laboratorium | REUSE | — |
| Penyaring periode, disiplin, reset | `DataFilter`, `FilterDatePicker` (`max`), `FilterSelect` | `data-filter.jsx:306` — kotak cari hanya bila `onSearchChange` ada | REUSE | Tanpa kotak cari |
| Pesan periode | `InformationAlert` | Pola Laporan Penerimaan | REUSE | Di bawah penyaring |
| Tiga bagian berjudul beserta rinciannya | `BaseDetailSection` + `InformationAlert` + `DataTable` | `base-detail-section.jsx` — `<section aria-labelledby>`, `showCount`, `soft` | COMPOSE | Dirangkai di view |
| Lima tabel | `DataTable` (`pagination={false}`, `sortLatestFirst={false}`) | Pola Ringkasan Laboratorium | REUSE | Kolom di `*-table-columns.jsx` |
| Penanda kesegeraan | `StatusBadge` | Pola daftar kerja | REUSE | Cito `warning`, rutin `info` |
| Akses ditolak | `AccessDeniedGate` | Mengenali `403` dan *"tidak memiliki izin"* | REUSE | — |
| Butir menu | `requiredPermission` + `filterMenuItemsByPermission` | Penyaring rekursif, dipanggil sidebar | REUSE | Bukan `permission:` |

Keputusan `COMPOSE` disajikan dengan tiga pilihan — (A) `BaseDetailSection` + `DataTable`, rekomendasi;
(B) `DataFilter` bertab; (C) kartu Bootstrap mentah seperti Laporan AR — dan dijalankan A: ketiga laporan
terlihat bersamaan sehingga *memuat per laporan* dapat diamati, tanpa komponen baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Per bagian: tabel menulis *"Membentuk laporan..."*; tombol *Muat ulang* berlabel *"Memuat ulang..."* |
| Kosong | *"Tidak ada hasil yang dirilis pada periode ini."* / *"Tidak ada wadah yang diputuskan pada periode ini."*; angka kosong "—" beserta sebabnya |
| Gagal | Galat periode: pesan backend di bawah penyaring, bagian menulis *"Laporan belum dapat dibentuk"*. Galat lain: pesan pada bagiannya saja; pemulihan lewat *Muat ulang* |
| Tanpa hak akses | *"Ups! Akses Ditolak"* dengan *"Anda tidak memiliki izin membuka Laporan Operasional Laboratorium."* atau pesan `403` backend |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Operational Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/filters/metadata` | Pilihan disiplin (`name`, `label`) | `LabOperationalReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/examination-count` | Bagian Jumlah Pemeriksaan dan rinciannya | `LabOperationalReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/specimen-rejection` | Bagian Penolakan Wadah dan rincian alasan | `LabOperationalReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/turnaround-time` | Bagian Waktu Penyelesaian | `LabOperationalReport : Read` |

Parameter ketiga laporan: `startDate`, `endDate` (`YYYY-MM-DD`, tanggal WIB), `discipline` (nama enum; kosong
tidak dikirim). Tiga endpoint `…/export` milik `FE-LAB-45`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` pada seluruh berkas yang disentuh | Nol error, nol warning — sesudah satu warning `react-hooks/set-state-in-effect` diperbaiki dengan menurunkan keadaan memuat dari kunci permintaan | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-operational-report-rules.test.mjs` | **17/17** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2074 uji: 2067 lolos, **7 gagal — seluruhnya kegagalan lama** | `EXISTING / ENVIRONMENT ISSUE` | Rincian di bawah |
| `npm run build` | Exit 0 (94 dan 76 detik); route `/health-services/laboratory-management/lab-operational-reports` terbentuk | `PASS` | Keluaran build |
| `npx playwright test tests/e2e/lab-operational-report-screen.spec.mjs` terhadap build standalone | **8/8** (dua kali: sebelum dan sesudah perbaikan lebar kolom) | `PASS` | Keluaran perintah |
| Layar hasil build diberi **respons sungguhan** backend lokal (PostgreSQL dev) | Ketiga skenario tampil benar — rincian di bawah | `PASS` | Tangkapan layar |
| Grep anti-regresi (checklist G) | Nol `<button`/`.btn`, nol `<table`, nol tipografi Bootstrap, nol inline style, nol stylesheet baru; `toISOString` hanya di komentar peringatan | `PASS` | Keluaran perintah |

**Tujuh uji unit lama yang gagal** — tidak satu pun karena perubahan ini:

| Uji | Sebab |
| --- | --- |
| `route, menu, dan store terdaftar` (akuntansi), `CASE 1 & CASE 2` (petty cash) | Pola yang dicari (`/corporate/accounting/reconciliation`, `label: "Keuangan"`) **juga tidak ada pada `menu-items.jsx` versi HEAD** — dibuktikan dengan menguji pola yang sama pada kedua versi |
| `M0: tiga butir Setup Bank Darah` | Struktur menu Bank Darah; blok itu tidak disentuh |
| Dua uji privasi daftar kerja Hemodialisa; dua uji `FE-HMD-01 AC-3` | Berkas Hemodialisa dan berkas ujinya tidak disentuh; `permission:` memang tidak dibaca penyaring mana pun |

Diff `menu-items.jsx` hanya **satu blok tambahan 11 baris** di menu Laboratorium.

**Verifikasi manual** — Chromium (Playwright) terhadap `npm run build` standalone, zona `Asia/Jakarta`:

| Skenario | Hasil sebenarnya |
| --- | --- |
| Periode bawaan | Permintaan pertama `startDate` = tanggal 1 bulan berjalan, `endDate` = hari ini menurut WIB; `discipline` tidak dikirim |
| Angka dari backend | *3,0%*, *48,6 menit*, *131,2 menit*, *3.905*, *Total terhitung: 5 pemeriksaan dirilis.*, periode *1 September 2026 - 30 September 2026* |
| `ARCH-GAP-LAB-11` | Baris PA: sel jumlah "—", Keterangan *"Rilis hasil Patologi Anatomi belum tersedia."*; baris waktu PA tanpa angka 0 |
| Angka kosong | *Nol wadah diputuskan*; *Nol hasil dirilis*; kolom cito "—" pada baris rutin; *"3 pemeriksaan cito belum punya batas waktu"* |
| Memuat per laporan | Waktu Penyelesaian ditahan 8 detik: Jumlah dan Penolakan tampil lebih dulu, Waktu menulis *"Membentuk laporan..."*, lalu tampil |
| Satu laporan gagal `500` | Pesan hanya pada Penolakan Wadah; dua lainnya tampil |
| `422` | Pesan tepat satu kali di bawah penyaring; nol angka |
| `403` backend | Pesan tidak berwenang; nol bagian laporan |
| Tanpa izin (pemegang `LabExamination : Read` dan `LabWorklist : Read`) | Butir menu **tidak tampil** (0 tautan); alamat langsung → *"Ups! Akses Ditolak"*, nol angka |
| Pemegang `LabOperationalReport : Read` | Butir menu **tampil** (1 tautan) |
| Penyaring disiplin | Memilih *Mikrobiologi* → tepat tiga permintaan `discipline=Microbiology` |
| Reset (↻) | Tiga permintaan berikutnya tanpa `discipline`, periode kembali ke bawaan |
| **Respons sungguhan** — `GET` ketiga laporan September 2026 dan metadata dari backend lokal terhadap PostgreSQL dev | Setiap ruas yang dibaca layar ada dengan nama yang sama. Tampil: *"Tidak ada hasil yang dirilis pada periode ini."* (dev belum punya rilis), PK *20,0%* (5 diputuskan, 1 ditolak), *Nol wadah diputuskan* pada PA dan Mikrobiologi, alasan *Jumlah sampel tidak mencukupi*, sebab PA pada Waktu Penyelesaian |
| **Respons sungguhan** — 1 Januari 2026 - 2 Januari 2027 | `422` *"Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya."* tampil di bawah penyaring |
| **Respons sungguhan** — tanpa `startDate` | `400` *"Periode laporan wajib diisi."* tampil di bawah penyaring |

Uji manual: `PASS` — dengan batas di bawah.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-operational-report-rules.test.mjs — PASS`

`MANUAL TEST: PASS pada build produksi dengan respons backend sungguhan yang dimasukkan lewat Playwright; sambungan langsung layar ke backend dengan akun asli NOT FEASIBLE — lihat di bawah`

**Tidak dijalankan:**

- **Layar tersambung langsung ke backend dengan akun kepala instalasi, pemegang `Read` saja, dan analis.**
  `NEXT_PUBLIC_API_QUILVIAN` build frontend mengarah ke API dev bersama, yang belum menerima `MVP-11a`
  (langkah rilis `MVP-11c` langkah 0). Belum ada jabatan pemegang `LabOperationalReport : Read` (langkah 1).
  Pengganti yang dijalankan: respons sungguhan backend lokal dimasukkan ke layar hasil build, dan pemisahan
  peran dibuktikan lewat daftar kewenangan yang dimock dengan bentuk `permission-slice`.
- `npm run test:e2e` penuh dan `npm run test:uat` — hanya spec task ini yang dijalankan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Matriks layar — `ARCH-GAP-LAB-11` (teks tampil, 0 tidak) | Terpenuhi | Uji unit; e2e; tangkapan respons sungguhan |
| Matriks layar — angka kosong "—" beserta sebabnya | Terpenuhi | Uji unit; e2e |
| Matriks layar — pemformatan `3.0` → *3,0%*, `48.6` → menit | Terpenuhi | Uji unit (*48,6 menit*) |
| Matriks layar — menu tidak tampil bagi pengguna tanpa `Read`; route langsung → tidak berwenang | Terpenuhi | Tangkapan: 0 dan 1 tautan; e2e |
| Matriks layar — memuat per laporan | Terpenuhi | e2e dengan penundaan 8 detik |
| Matriks layar — galat periode 367 hari → pesan `422` backend pada penyaring | Terpenuhi pada respons sungguhan; lewat sambungan langsung **belum** | Tangkapan `422` |
| `AC-250`..`AC-253` bagian antarmuka | Terpenuhi pada build dengan respons sungguhan; tiga akun asli **belum** | Bagian 6 |
| Verifikasi roadmap — uji unit berkas aturan: periode WIB, *3,0%*, `isCountable = false` bukan 0, kolom cito kosong pada rutin | Terpenuhi | 17/17 |
| Verifikasi roadmap — tiga akun samaran terhadap backend | **Belum terpenuhi** | Menunggu `MVP-11c` langkah 0-1 |
| DoD — layar dan butir menu berjalan; berkas aturan teruji; nol daftar pasien; lint dan build hijau; laporan | Terpenuhi | Bagian 3 dan 6 |
| Nol daftar pasien, nol tautan dari angka | Terpenuhi | Kolom tabel hanya disiplin, jenis, alasan, dan angka; nol `onRowClick` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning lint `react-hooks/set-state-in-effect` muncul pada versi pertama hook — diperbaiki, bukan ditekan. Build dan lint repository tanpa error |
| Masalah yang diketahui | **1.** Bagi pengguna tanpa izin, tiga permintaan laporan tetap terkirim **sebelum** daftar kewenangan sesi termuat — perilaku bawaan `usePermission` (*belum diketahui berarti boleh*). Backend menjawab `403`; layar menampilkan nol angka. **2.** Katalog base component basi: `FilterDatePicker` menerima `max`. **3.** Tujuh uji unit lama gagal (bagian 6) |
| Dependency backend | Nol task backend yang belum selesai. Pemakaian sungguhan menunggu langkah rilis `MVP-11c`: deploy `MVP-11a` (langkah 0) dan pemberian `LabOperationalReport : Read` kepada kepala instalasi (langkah 1). Angka jumlah dan waktu penyelesaian tetap nol sampai rilis hasil dipakai sungguhan (`MVP-9d` langkah 4) |
| Perubahan sampingan | Playwright membersihkan folder `test-results/`: `test-results/.last-run.json` berubah dan satu `error-context.md` Rawat Inap terhapus — keduanya berkas ter-track. **Dipulihkan** ke isi HEAD (CRLF seperti hasil checkout); `git status` bersih dari keduanya |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx`, ` M src/utils/menu-sidebar/menu-items.jsx`, `??` `src/app/health-services/laboratory-management/lab-operational-reports/`, `src/components/view/health-services/laboratory-management/lab-operational-reports/`, `lab-operational-report-constants.jsx`, `lab-operational-report-rules.js`, `use-lab-operational-report.jsx`, `lab-operational-report.service.js`, `tests/e2e/lab-operational-report-screen.spec.mjs`, `tests/unit/lab-operational-report-rules.test.mjs`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-45` — tombol unduh ketiga laporan (`BE-LAB-86` ✅), kini `SIAP DIKERJAKAN` |
