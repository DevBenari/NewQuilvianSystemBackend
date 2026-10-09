# Laporan Perubahan Frontend — `FE-LAB-52`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-52` |
| Judul | Susunan Beranda, kartu, pesanan terbaru, dan kesiapan operasional |
| Slice | Gelombang `MVP-13b` — `EPIC-LAB-19` Beranda Lab (BR-140) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Gelombang `MVP-13` — `EPIC-LAB-19`* |
| Trace | `FR-19.1`, `FR-19.2`, `FR-19.3` (tanpa *Jenis laboratorium*), `FR-19.6`, `FR-19.7`, `FR-19.8`; `LAB-DEC-204`, `LAB-DEC-205`, `LAB-DEC-208`..`LAB-DEC-211`, `LAB-FE-035`; `LAB-REQ-021` butir 6, 11 |
| Contract version | `LAB-API-v1` **`r44`** 39.3, 39.5; `LAB-PERM-v1` **revision 16** — `approved` 2026-10-08 (`LAB-REQ-021`) |
| Wewenang UI | Urutan bagian dan isinya: keputusan produk (`LAB-DEC-204`, `LAB-FE-035`). Teks deskripsi bagian, susunan panel kesiapan, nama berkas, dan pemisahan slice: `DEV_DISCRETION` dalam tema V2 |
| Dependency | `BE-LAB-92` ⚠ (working tree backend, belum di-commit). Biner lokal berisi `BE-LAB-92` dan `BE-LAB-93` |
| Klasifikasi | `MEDIUM` — 6 berkas source diubah, 3 berkas source baru, 1 uji baru. Nol route, menu, maupun izin baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap, traceability, dan manifest |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `9bd8b96fe` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `38c8a4f4` (branch `yoga`) + working tree `BE-LAB-92`/`BE-LAB-93` |
| Tanggal | 2026-10-08 (dimulai), 2026-10-09 (diselesaikan) |
| Status | ✅ **`SELESAI`** — 19/19 skenario peramban lolos dengan superadmin; **Uji ulang 2026-10-09 dengan akun analis asli** (Gilang; FE-LAB-53 juga Vina), tanpa mengganti jawaban sukses, nol tulis: 17/19, dua sisanya adalah pemeriksaan keadaan *sebelum* `FE-LAB-53` (kartu *Jenis laboratorium* belum ada, `yearly` belum dipanggil) yang sengaja berubah. **Catatan sisa, bukan penahan:** (1) dev 0 pesanan hari ini — angka bukan-nol `AC-295` (12 / 8 / 3 (25%) / 2) terbukti lewat uji unit; (2) `AC-301` lewat `403` tiruan karena tidak ada akun dev tanpa `LabOrder : Read` |

---

## 1. Keadaan yang ditemukan di awal

Sesi 2026-10-08 sudah membangun service, hook, aturan murni, kolom tabel, view, CSS, dan uji unit di working tree.
Sesi itu terhenti saat dev server port 3000 dinyalakan ulang tanpa `SESSION_SIGNING_SECRET`, sehingga login
pengguna terus diarahkan ke `/login`. Pada awal sesi ini tidak ada server node maupun dotnet yang hidup.

| Bukti | Isi |
| --- | --- |
| State Beranda | Ditaruh di `lab-order-slice.jsx` (ruas `dashboardToday*`, `dashboardRecentOrders*`) — **menyimpang** dari Cakupan (2) roadmap, yang meminta `lab-dashboard-slice.jsx` baru beserta tempat `yearly`/`selectedYear` |
| DTO backend | `LabDashboardTodayResponse` (9 ruas) dan `LabDashboardRecentOrderResponse` (8 ruas) — cocok dengan nama ruas yang dibaca layar |
| Line ending | View, hook, dan CSS ditulis ulang dengan LF (CSS bercampur), padahal checkout repo memakai CRLF |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas laboratorium pemegang `LabOrder : Read`.

1. Petugas membuka **Laboratorium → Beranda**.
2. Dari atas ke bawah ia melihat:
   - hero berisi *Perbarui Data*;
   - waktu muat dalam WIB;
   - empat kartu **Hari Ini** (*Pesanan Hari Ini*, *Menunggu*, *Selesai n (p%)*, *CITO*);
   - **Fokus Operasional** (*Total Pesanan Tercatat*, *Hasil Menunggu Validasi*, *Tingkat Penyelesaian*);
   - **Pesanan Terbaru**, maksimal 10 baris;
   - **Kesiapan Operasional** (*"3 dari 12 pesanan selesai · 8 menunggu · 2 CITO"*);
   - **Rekap Status Pesanan** lama di paling bawah.
3. Petugas mengklik baris pesanan, atau memfokuskannya lalu menekan Enter. *Detail Pesanan Laboratorium*
   terbuka.
4. *Perbarui Data* memuat ulang ketiga sumber. Mengganti tanggal rekap hanya memuat ulang rekap.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Pesanan terbaru gagal dimuat | Hanya bagian *Pesanan Terbaru* berisi *"Pesanan terbaru gagal dimuat"* + *Coba lagi*. Kartu dan rekap tetap tampil |
| Ringkasan hari ini gagal | Kartu *Hari Ini* dan *Fokus Operasional* bergalat. Tabel dan rekap tetap tampil |
| Salah satu sumber `401`/`403` | Seluruh halaman diganti *Ups! Akses Ditolak* |
| 0 pesanan hari ini | *0*, *Selesai 0 (0%)*, *"0 dari 0 pesanan selesai · 0 menunggu · 0 CITO"*. Tidak muncul `NaN` |
| Belum ada pesanan sama sekali | Tabel kosong: *"Belum ada pesanan laboratorium"* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap `FE-LAB-52` dan `r44` 39.3, 39.5.
- `LabDashboardDtos.cs`.
- `lab-order-slice.jsx`, `lab-monitoring-slice.jsx` (pola helper per slice), `store.jsx`.
- `access-denied-gate.jsx`, `access-denied-utils.js`, `summary-grid.jsx` (`percent`/`subtitle`), dan
  `data-table.jsx` (`onRowClick` + papan ketik).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/.../lab-order.service.js` | `getLabDashboardToday`, `getLabDashboardRecentOrders` (base URL `lab-orders`) |
| `src/lib/state/slice/.../lab-dashboard-slice.jsx` | **Baru.** Bagian `today`, `recentOrders`, dan `yearly` masing-masing berisi `data`, `loading`, `error`, `errorStatus`, `loadedAt`. Ada juga `selectedYear` untuk `FE-LAB-53`. Permintaan yang di-`abort` tidak dianggap galat. Jawaban permintaan lama yang tiba belakangan diabaikan lewat `requestId` |
| `src/lib/state/store.jsx` | `labDashboard: labDashboardReducer` |
| `src/lib/hooks/.../laboratory-dashboard-rules.js` | **Baru.** Fungsi murni: kartu hari ini dan fokus, ringkasan progres, baris tabel (`procedureNames` digabung koma, disiplin kosong → *Belum tergolong*, nilai kosong → `-`), label status (fallback *Dikonfirmasi*), waktu muat WIB, dan pendeteksi `401`/`403` lintas sumber. Persentase tidak dihitung ulang di layar |
| `src/lib/hooks/.../use-laboratory-overview.jsx` | Memuat `today`, `recentOrders`, dan `summary`. *Perbarui Data* (`reloadAll`) memuat ketiganya. Data basi 60 detik berlaku untuk ketiganya. Coba lagi per bagian. `openOrder` lewat token rute privat |
| `src/lib/constants/.../laboratory-constants.jsx` | `LAB_DASHBOARD_COPY`, `LAB_DASHBOARD_TODAY_CARDS`, `LAB_DASHBOARD_STATUS_LABEL_FALLBACK` |
| `src/components/view/.../laboratory-overview-view.jsx` | Bagian 1–3 dan 7–9 berurutan. Tempat bagian 4–6 disisakan untuk `FE-LAB-53`. Kartu rentang lama dipindah ke bagian Rekap. Setiap bagian punya `LaboratoryStatePanel` sendiri |
| `src/components/view/.../laboratory-overview-recent-columns.jsx` | **Baru.** Kolom: No, No. RM, Nama Pasien, Pemeriksaan, Disiplin, Status, Waktu Diminta (WIB) |
| `src/style/.../laboratory-overview.module.css` | Tata letak panel kesiapan, memakai token tema |
| `tests/unit/laboratory-dashboard-rules.test.mjs` | **Baru.** 11 uji: kartu `AC-295`, nol tanpa `NaN`, fokus `AC-296`, ringkasan progres, baris tabel, label tak dikenal, waktu muat WIB, `AC-301`, isolasi galat slice, `abort` bukan galat, dan jawaban lama tidak menimpa |

### 3.3 Kepatuhan arsitektur frontend

- Berkas berada di tujuh lapis wajib.
- State Beranda dipindah dari `lab-order-slice.jsx` ke `lab-dashboard-slice.jsx` sesuai Cakupan (2). Hasilnya,
  diff `lab-order-slice.jsx` terhadap `HEAD` kini nol. Diff lamanya disimpan di scratchpad sebelum dikembalikan.
- Helper `readServerFailure`/`toArray` disalin per slice, mengikuti pola sembilan slice Lab yang ada.
- Rekap status tetap di `labOrder` karena terikat penyaringnya.
- Kartu *Hasil Menunggu Validasi* tidak bertautan (`LAB-DEC-208`).
- Nama pasien tidak masuk judul tab maupun log konsol.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu dan tabel per bagian; *Perbarui Data* berputar selama satu sumber masih dimuat |
| Kosong | Tabel: *"Belum ada pesanan laboratorium"*. Kartu menampilkan 0 |
| Gagal | Per bagian, dengan *Coba lagi* milik bagian itu |
| Bentrok `409` | Rekap: perilaku lama dipertahankan |
| Basi | Kembali ke tab sesudah ≥ 60 detik → ketiga sumber dimuat ulang |
| Tanpa hak akses | *Ups! Akses Ditolak* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/dashboard/today` | Kartu hari ini, fokus operasional, ringkasan progres | `LabOrder : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/dashboard/recent-orders` | Tabel pesanan terbaru | `LabOrder : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/summary` | Rekap status — tidak berubah | `LabOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas yang disentuh | 0 error, 0 warning | `PASS` | Exit 0 |
| Uji unit baru | 11/11 | `PASS` | |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2533 uji: 2525 lulus, 8 gagal | `PASS` (nol kegagalan baru) | Kedelapan nama identik dengan baseline. Dua yang menyebut "beranda" berasal dari `hemodialysis-navigation-and-privacy-audit.test.mjs` |
| `npm run build` | Exit 0; standalone siap | `PASS` | Nol server hidup saat build |
| `AC-294` — urutan bagian | Beranda → Hari Ini → Fokus Operasional → Pesanan Terbaru → Kesiapan Operasional → Rekap Status Pesanan | `PASS` | Posisi teks berurutan naik; tangkapan layar |
| `AC-295` — kartu hari ini | 0 / 0 / 0 (0%) / 0 = jawaban `dashboard/today`; tanpa `NaN` | `PASS` (nilai nol) | Angka bukan-nol hanya lewat uji unit — dev 0 pesanan pada 2026-10-09 |
| `AC-296` — menunggu validasi | 0 = `totalData` `validation-queue?stage=AwaitingValidation`; nol tautan; tanpa kartu *Petugas Laboratorium*/*Jenis laboratorium* | `PASS` | Angka acuan diambil lewat jalur terpisah |
| `AC-299` — tabel terbaru | 10 baris = 10 baris backend. Waktu `2026-10-08T02:59:12Z` tampil *08 Okt 2026, 09.59* (WIB). Pemeriksaan digabung koma | `PASS` | |
| `AC-299` — klik baris | Membuka rincian `LAB-RSMMC-000023` (token URL memuat nomornya; Hemoglobin, *Diterima*, 8 Okt 09.59) | `PASS` | Tangkapan layar rincian |
| `AC-299` — papan ketik | Fokus baris + Enter membuka pesanan | `PASS` | |
| `AC-300` — waktu muat | *"Terakhir dimuat 9 Okt 2026, 08.48 WIB"* | `PASS` | |
| `AC-300` — *Perbarui Data* | `today`, `recent-orders`, `summary` masing-masing +1 permintaan | `PASS` | Pencatat permintaan |
| `AC-300` — ganti tanggal rekap | Hanya `summary` +1; `today`/`recent-orders` tetap | `PASS` | |
| `AC-300` — galat terisolasi | `recent-orders` dicegat `500` (predikat memuat `/v1/`) → hanya tabel bergalat; kartu dan rekap tetap | `PASS` | Satu endpoint dicegat; tangkapan layar |
| `AC-301` — tanpa izin | `dashboard/today` dicegat `403` → *Ups! Akses Ditolak*, tanpa angka | `PASS` (tiruan) | Akun tanpa `LabOrder : Read` tidak dipakai |
| `LAB-REQ-021` butir 11 | Lima kartu rentang lama tampil di bawah judul Rekap | `PASS` | |
| Privasi | Judul tab *Beranda*; nama pasien tidak ada di log konsol | `PASS` | |
| Penjaga tulis | Nol permintaan non-GET | `PASS` | Log skrip |
| Bagian tahunan | Nol panggilan `dashboard/yearly` | `PASS` | Milik `FE-LAB-53` |
| Ulang dengan akun analis asli Gilang (2026-10-09) | 17/19: seluruh AC lolos; dua FAIL = pemeriksaan pra-`FE-LAB-53` (*"tanpa kartu Jenis laboratorium"*, *"`yearly` belum dipanggil"*) yang kini sengaja berubah | `PASS` | Login lewat formulir, sandi dari berkas sementara yang sudah dihapus; nol jawaban sukses diganti |

**Lingkungan.**

- Backend lokal Development `https://localhost:7184` dari biner yang memuat `BE-LAB-92`/`93`. Tidak ada `.cs`
  yang lebih baru dari biner.
- `next dev` port 3000 diarahkan ke backend lokal lewat `.env.local`. `SESSION_SIGNING_SECRET` diberikan lewat
  env proses dan nilainya tidak dicetak.
- DB dev bersama, superadmin.
- Kedua server dibiarkan hidup sesudah uji supaya pengguna dapat langsung masuk ke port 3000.

Uji manual: `PASS` — superadmin dan akun analis asli.

AUTOMATED TEST: PASS (11 uji baru; nol kegagalan baru).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-294` bagian 1–3 dan 7–9 berurutan | Terpenuhi (4–6 menyusul `FE-LAB-53`) | Bagian 6 |
| `AC-295` sisi layar | Terpenuhi pada data nol. Angka 12 / 8 / 3 (25%) / 2 lewat uji unit | Bagian 6 |
| `AC-296` sisi layar | Terpenuhi | Bagian 6 |
| `AC-299` | Terpenuhi | Bagian 6 |
| `AC-300` (bagian `FE-LAB-52`) | Terpenuhi | Bagian 6 |
| `AC-301` | Terpenuhi lewat `403` tiruan | Bagian 6 |
| Tambahan: 0 pesanan → *0 (0%)* | Terpenuhi | Peramban + uji unit |
| DoD — akun analis asli | Terpenuhi 2026-10-09 | Gilang, bagian 6 |
| DoD — uji unit, lint, build, laporan | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | `LAB_ORDER_STATUS_LABEL` belum memuat *Confirmed* (sejak `FE-LAB-48`). Beranda memakai fallback *Dikonfirmasi*; layar lain tidak diubah. Rincian pesanan tidak menampilkan nomor pesanan — di luar cakupan |
| Dependency backend | `BE-LAB-92` ⚠ wajib dideploy bersama (langkah rilis `MVP-13c`). Tanpanya kartu dan tabel menerima `404`, sedangkan rekap tetap jalan |
| Perubahan sampingan | `NONE`. Line ending view, hook, dan CSS dikembalikan ke CRLF; statistik diff tidak berubah |
| Interupsi | Sesi 2026-10-08 menyalakan ulang dev server tanpa `SESSION_SIGNING_SECRET`, sehingga login di port 3000 gagal. Sesi ini menyalakannya dengan rahasia yang sama dengan spec e2e. Sesi peramban lama pengguna perlu login ulang |
| Status Git | Frontend: 6 berkas `M`, 4 berkas `??` — belum di-commit |
| Langkah berikutnya | `FE-LAB-53` sudah selesai (laporan [`FE-LAB-53.md`](FE-LAB-53.md)). Sisa `EPIC-LAB-19`: langkah rilis `MVP-13c` serempak dengan `BE-LAB-92`/`93` |
