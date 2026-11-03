# Laporan Perubahan Frontend — `FE-NV-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-005` |
| Judul | Papan lantai tampil di TV |
| Slice | `MVP-2` — papan lantai |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-005` |
| Trace | `FR-NV-010`–`016`, `021`, `030`, `031`; `NV-DEC-007`, `013`, `014`, `017`; `NV-FE-002`–`004`; `03-frontend-architecture.md` skema `FE-NVW-04` |
| Contract version | `0.1.1` — `approved`; bagian `/board` sama dengan `0.1.0` |
| Wewenang UI | Warna, ikon, ritme kedip, dan durasi per halaman `DEV_DISCRETION` (`NV-FE-003`, `004`). Keterbacaan ±3 m (`NV-FE-002`) dinilai Rizki di TV |
| Dependency | `BE-NV-005` 🟡 (bagian yang dipakai layar sudah terbukti), `FE-NV-003` ✅ |
| Klasifikasi | `STANDARD` — layar baru di route milik Nurse View, tanpa menyentuh berkas bersama |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend | Versi awal di-commit Rizki sebagai `61b5e3f35`; perbaikan tata letak, penyambungan view ke route, dan penghapusan kerangka `FE-NV-003` di `2e510a555` |
| Commit backend yang dijadikan rujukan | `6489c32c` |
| Tanggal | 2026-11-02 sampai 2026-11-03 |
| Status | 🟡 **SEBAGIAN 3 November 2026** — uji layar 8/8 `PASS` (44 cek; L5, L6, L8 memakai jawaban `/board` tiruan), L9 perubahan data `PASS`, uji TV N1–N9 `PASS` dengan papan sungguhan, `npx eslint` 0 error 0 warning. Belum: keterbacaan ±3 m di TV sungguhan (Rizki); ikon J dan I baru terlihat dengan jawaban tiruan, karena datanya belum bisa dibuat (`BE-NV-005` §6). Verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Sesudah `FE-NV-003`, layar `/nurse-view-display` hanya kerangka: nama lantai, nama dan kode perangkat, serta "Papan
lantai sedang disiapkan." Layar itu memanggil `/current` setiap 60 detik, sedangkan `GET /board` belum ada sampai
`BE-NV-005` dibuild.

---

## 2. Proses bisnis dari sisi pengguna

1. TV login, lalu masuk ke papan lantai. Kepala layar menampilkan nama lantai, jam berjalan, dan "Data hh.mm", yaitu
   jam papan terakhir dari server.
2. Satu halaman memuat 5 kolom × 3 baris kartu. Nama bangsal tampil di atas kelompoknya.
3. Kartu kritis tampil di urutan pertama bangsalnya, berlatar merah dan berkedip. Kartu lain diurutkan menurut nama
   bed, dengan angka dibandingkan sebagai angka.
4. Bila kartu lebih dari satu halaman, halaman berganti setiap 15 detik dan pojok kanan bawah menampilkan
   "Halaman n / m".
5. Papan dimuat ulang setiap 60 detik (nilai dari `/current`), dan juga segera sesudah jaringan tersambung kembali.
6. Isi kartu berpasien:
   - nama bed dan ikon J (risiko jatuh), A (alergi), I (isolasi);
   - "inisial · L/P · umur";
   - "PJ nama" atau "Belum ada PJ";
   - "EWS skor · tingkat" beserta jam ukur;
   - TD · N · RR dan S · SpO₂;
   - "Lewat jadwal …" atau "Diukur … lalu";
   - "Bed tidak sesuai isolasi" bila berlaku.

   Bed kosong, dibersihkan, dan dipesan tampil sebagai kartu abu-abu berisi nama bed dan labelnya.
7. Tidak ada suara. Papan hanya disimpan di memori halaman, tidak di storage.

**Jalur tidak normal:**

| Kejadian | Yang dilihat di TV |
| --- | --- |
| Jaringan putus sebelum papan pertama tiba | "Tidak dapat menghubungi server. Sistem mencoba lagi otomatis." dan tombol Coba lagi |
| Jaringan putus saat papan tampil | Papan lama tetap tampil. Begitu data berumur lebih dari 120 detik, muncul pita abu-abu "Data terakhir hh.mm — koneksi terputus". Pita hilang sesudah tersambung |
| Lantai tanpa kamar | "Belum ada kamar terdaftar untuk lantai ini. Hubungi IT." |
| Perangkat dinonaktifkan atau sesi basi | "Mengalihkan ke layar login..." lalu layar login |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Berkas `nurse-view-display` dari `FE-NV-003`, pola `queue-display-screen-view`, `loadingScreen.jsx`,
`base-button.jsx`, `base-patient-card.jsx`, `status-badge.jsx`, `information-alert.jsx`, `pagination.jsx`, token
`globals.css`, `InstanceAxios.jsx`, `login-slice.jsx`; api §3, skema `FE-NVW-04`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/nurse-view-display/nurse-view-display-client.jsx` | Memuat view papan (`dynamic`, tanpa SSR); teks memuat "Memuat papan lantai..." |
| `src/components/view/nurse-view-display/nurse-view-floor-board-view.jsx` | **Baru.** Kepala, satu grid untuk seluruh halaman dengan baris per bangsal (`gridTemplateRows` dihitung dari jumlah baris tiap bangsal), pesan koneksi, pesan lantai kosong, nomor halaman, pita data basi |
| `src/components/view/nurse-view-display/nurse-view-bed-card.jsx` | **Baru.** Kartu berpasien dan kartu tanpa pasien, dibungkus `memo` |
| `src/lib/hooks/nurse-view-display/use-nurse-view-floor-board.jsx` | **Baru**, menggantikan hook `FE-NV-003`. Isinya: `/current` sekali untuk interval; `/board` berkala dan saat `online`; `401`/`403` → hapus cookie, logout, ke login; jam per detik; status data basi; rotasi halaman 15 detik |
| `src/lib/services/nurse-view-display/nurse-view-display-service.jsx` | Tambah `getNurseViewFloorBoard` |
| `src/lib/constants/nurse-view-display/nurse-view-display-constants.jsx` | Batas data basi bawaan, durasi halaman, 5 kolom × 3 baris, zona waktu, label EWS, endpoint `/board` |
| `src/utils/nurse-view-display/nurse-view-board-utils.jsx` | **Baru.** Format jam, durasi, angka, tekanan darah, samaran; label bed; urutan kartu; pembagian halaman per bangsal |
| `src/style/nurse-view-display/nurse-view-floor-board.module.css` | **Baru.** Hanya token. Ukuran huruf mengikuti layar lewat `--board-unit: min(1vw, 1.78vh)` |
| Dihapus: `nurse-view-display-screen-view.jsx`, `use-nurse-view-display-runtime.jsx`, `nurse-view-display-screen.module.css` | Kerangka sementara `FE-NV-003` yang sudah digantikan |

Berkas bersama `FE-NV-003` (`proxy.js`, `use-base-login.jsx`, `root-layout-shell.jsx`, `auth-wrapper.jsx`,
`session-activity-manager.jsx`) tidak disentuh.

**Delta terhadap backend.** Backend mengurutkan kartu menurut `BedCode`, yang di DB dev berupa kode internal
(`BD-RSMMC-00029`). Layar mengurutkan ulang menurut `BedName` ("325-A"), sehingga urutan di TV sesuai nama bed.

### 3.3 Kepatuhan arsitektur frontend

Alur `app` → `components/view` → `hooks` → `services` → `InstanceAxios`, tanpa Axios instance baru. Papan disimpan di
state hook, tidak di Redux maupun storage (syarat kartu). Redux hanya dipakai untuk `LogoutUser`. `globals.css` tidak
diubah.

**Tabel keputusan base component:**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Layar memuat dan mengalihkan | `LoadingScreen` | `features/loading/loadingScreen.jsx` | `REUSE` | Prop `text` |
| Tombol Coba lagi | `BaseButton` | `base-features/base-button.jsx` | `REUSE` | `variant="primary"` |
| Kartu bed | `BasePatientCard` | Kartu cetak Kiosk dengan identitas lengkap dan QR | `COMPOSE` | Kartu TV harus samaran dan berhuruf besar; CSS module dengan token |
| Ikon J/A/I | `StatusBadge` | Badge `react-bootstrap` dengan set status admin dan ukuran tabel | `COMPOSE` | Tanda huruf berukuran layar TV; CSS module dengan token |
| Pesan koneksi dan pita data basi | `InformationAlert` | Alert dengan tombol tutup | `COMPOSE` | Pita tidak boleh bisa ditutup dan harus hilang sendiri saat tersambung |
| Nomor halaman | `Pagination` | Navigasi tabel yang diklik pengguna | `COMPOSE` | TV berganti halaman sendiri; cukup teks |

`UI GATE: PASS — dua REUSE, sisanya COMPOSE dengan token, tanpa base component baru.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna | Bukti |
| --- | --- | --- |
| Memuat | "Memuat papan lantai..." | Source |
| Kosong | "Belum ada kamar terdaftar untuk lantai ini. Hubungi IT." | L6 (simulasi) |
| Gagal | Pesan koneksi beserta Coba lagi, mencoba lagi otomatis | N4 |
| Data basi | Pita abu-abu dengan jam data terakhir | L4 |
| Tanpa hak akses | "Mengalihkan ke layar login..." lalu layar login | N5, T1–T2 |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/nurse-view-management/display-runtime/current` | Sekali saat layar dibuka: interval muat ulang dan batas data basi | policy `NurseViewDisplayRead` |
| `GET` | `/v1/health-services/nurse-view-management/display-runtime/board` | Isi papan, setiap 60 detik dan saat tersambung kembali | policy `NurseViewDisplayRead` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 7 berkas JSX | 0 error, 0 warning | `PASS` | Keluaran perintah, 3 Nov |
| Grep anti-regresi pada 7 JSX dan CSS module | Warna literal 0, `!important` 0, `<button>` mentah 0, komentar 0; `globals.css` tanpa diff; semua berkas CRLF | `PASS` | Keluaran perintah |
| L1 papan lantai 3, 1920×1080 | Kepala "Lantai 3 09.28 · Data 09.28". Kartu pertama 325-A kritis berlatar merah `rgb(180, 35, 24)`, animasi 1,2 detik berulang. Tanda A dan PJ tampil. Tidak ada kartu terpotong. 11 cek | `PASS` | `layar-2026-11-03T02-28-23-995Z/L1.json`, `L1-halaman-1.png` |
| L2 rotasi dan isi kartu | 5 halaman bergantian setiap 14–15 detik; 66 dari 66 bed terlihat. C "Belum ada PJ" dan "Belum ada TTV"; B "EWS 6 · Tinggi" dan "Diukur … lalu"; E "Dipesan — pasien akan datang" tanpa identitas; 326-A "Dibersihkan"; nama lengkap dan No. RM tidak ada di layar. 10 cek | `PASS` | `L2.json`, `L2-sesudah-putar.png` |
| L3 memori dan suara | Storage tanpa data papan dan identitas; 0 elemen audio/video; 0 galat halaman | `PASS` | `L3.json` |
| L4 putus jaringan | Papan lama tetap tampil (15 kartu). Pita "Data terakhir 09.29 — koneksi terputus" muncul 91 detik sesudah `/board` diputus, lalu hilang sesudah tersambung. Tetap di papan, tidak ke login | `PASS` | `L4.json`, `L4-pita-data-basi.png` |
| L5 ikon padat (simulasi) | Jawaban `/board` tiruan: A dengan J, A, I dan "Bed tidak sesuai isolasi"; B dengan J dan "Lewat jadwal 30 menit"; C dengan I. Kartu terpadat tidak terpotong di 1920×1080 | `PASS` simulasi | `L5.json`, `L5-simulasi-*.png` |
| L6 lantai kosong (simulasi) | Pesan lantai kosong tampil, tanpa kartu | `PASS` simulasi | `L6.json` |
| L7 dua bangsal, data nyata | Lantai 2: ICU dan Ruang Bayi di halaman yang sama, tinggi kartu seragam 269 px, layar tidak meluap | `PASS` | `L7.json`, `L7-lantai-2-dua-bangsal.png` |
| L8 layar 1366×768 (simulasi) | Kartu terpadat tidak terpotong, layar tidak meluap | `PASS` simulasi | `L8.json` |
| L9 perubahan data tanpa muat ulang halaman | `/board` dipanggil setiap 60 detik. TTV baru pasien B tiba di TV 57 detik sesudah disimpan dan kartu 325-B berubah menjadi "EWS 0 · Rendah", tanpa memuat ulang halaman. Pembatalan tiba dalam 60 detik. 9 cek | `PASS` | `segar-2026-11-03T02-38-35-982Z`, `L9-kartu-berubah.png` |
| T1–T2 akun manusia membuka papan | SuperAdmin dan LOKET dialihkan ke `/login` | `PASS` | `tolak-2026-11-03T02-43-22-993Z` |
| Uji TV `FE-NV-003` N1–N9 diulang dengan hook papan | 9/9 `PASS` (67 cek): `/board` berkala, putus jaringan, nonaktif ke login 59 detik, Atur Ulang Login, akun manusia | `PASS` | `uji-nv-003-20261102/tv-2026-11-03T02-01-15-524Z` |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-005-20261103/`, kecuali yang disebut lain.
Hook berubah sesudah uji N1–N9 hanya pada pembagian halaman; logika sesi, muat ulang, dan logout sama.

**Catatan L4.** Pita dihitung dari umur data, bukan dari lama putus: pita muncul saat tidak ada papan baru lebih dari
`StaleAfterSeconds` = 120 detik (`03-frontend-architecture.md` §5). Panggilan terakhir yang berhasil terjadi sekitar
30 detik sebelum jaringan diputus, sehingga pita muncul 91 detik sesudah putus. Dengan muat ulang 60 detik, pita
muncul antara 60 dan 120 detik sesudah putus. Artinya pita pasti sudah tampil saat putus lebih dari 2 menit
(`NV-AC-006`). Nama cek di `L4.json` ("sesudah lebih dari 120 detik") kurang tepat; syarat yang sebenarnya diperiksa
adalah pita tampil paling cepat 60 detik sesudah putus.

**Riwayat perbaikan produk.**

| Putaran | Hasil | Tindakan |
| --- | --- | --- |
| `layar-2026-11-03T02-17-05-886Z` | L1, L5 `FAIL`: isi kartu 325-A dan 325-B terpotong 76 px (grid 5×4) | Grid diubah menjadi 5×3; jam ukur dipindah ke baris EWS; baris status digabung |
| `layar-2026-11-03T02-23-15-569Z` | L5 terpotong 2 px; L8 terpotong 25 px di 1366×768; L7 tinggi kartu dua bangsal 273 dan 256 px. L1 `FAIL` karena skrip membaca warna di tengah animasi (`oklab`) | Satu grid untuk seluruh halaman dengan baris per bangsal, jarak antarbaris kartu nol, huruf sedikit dikecilkan; skrip menghentikan animasi saat membaca warna |
| `layar-2026-11-03T02-28-23-995Z` | 8/8 `PASS` | — |

**Ukuran huruf di 1920×1080** (CSS px): nama lantai 50, jam 46, nama bed 31, samaran 26, EWS 27, nama bangsal 27,
TTV dan PJ 21, pita 29. Ukuran fisiknya bergantung pada diagonal TV, jadi keterbacaan ±3 m harus dinilai di TV
sungguhan.

**Penyimpangan.** Data uji disiapkan lewat endpoint yang dipanggil layar Rawat Inap dan Klinis, bukan dari layar
(`BE-NV-005` §3.4). L5, L6, dan L8 memakai jawaban `/board` tiruan karena keadaannya belum bisa dibuat di DB dev.
Semua uji berjalan di Edge headless, bukan di TV.

`MANUAL TEST: DONE — Playwright (Edge headless, ignoreHTTPSErrors) 2026-11-03; 8/8 layar, L9, T1–T2, dan N1–N9 PASS.`

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`

**Tidak dijalankan:** `npm run build` (`NOT RUN`: dev server Rizki aktif di port 3000 dan memakai folder `.next` yang
sama; DoD kartu tidak memintanya).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-001` per bangsal | Terpenuhi di layar; bagian kamar tanpa `FloorCode` lihat `BE-NV-005` | L7, L2 |
| `NV-AC-002` samaran di kartu | Terpenuhi | L1, L2 |
| `NV-AC-003` merah berkedip, teratas, tanpa bunyi | Terpenuhi | L1, L3 |
| `NV-AC-004` tanpa interval hanya "Diukur … lalu" | Terpenuhi | L2 |
| `NV-AC-005` "Lewat jadwal 30 menit" | Terpenuhi di layar dengan jawaban tiruan; data aslinya terbukti di `BE-NV-005` B4 | L5 |
| `NV-AC-006` pita data basi | Terpenuhi | L4 |
| `NV-AC-007` TTV dibatalkan | Terpenuhi | L9 |
| `NV-AC-008` "Belum ada PJ" | Terpenuhi | L2 |
| `NV-AC-010` "Dibersihkan", "Dipesan — pasien akan datang" | Terpenuhi | L2 |
| `NV-AC-011` ikon J | Tampilan terbukti dengan jawaban tiruan; data nyata belum ada (`BE-NV-005` §6) | L5 |
| `NV-AC-012` ikon A tanpa nama zat | Terpenuhi | L1, L3 |
| `NV-AC-013` ikon I dan "Bed tidak sesuai isolasi" | Tampilan terbukti dengan jawaban tiruan; data nyata belum ada (`BE-NV-005` §6) | L5 |
| `NV-AC-014` pindah lantai ≤ 60 detik | Terpenuhi: perpindahan terbukti lewat HTTP (`BE-NV-005` B6), dan perubahan data tampil di TV dalam 57 detik (L9). Uji dua TV sekaligus belum dilakukan | B6, L9 |
| `NV-AC-020` kartu berubah ≤ 60 detik | Terpenuhi dengan TTV disimpan dan dibatalkan; pemulangan pasien tidak dicoba | L9 |
| `NV-AC-021` kamar tanpa `FloorPlanRoomKey` | Tidak bisa diuji, tidak ada datanya (`BE-NV-005` §6) | — |
| `UAT-04` | Terverifikasi developer sebagian: samaran dan A terbukti, J hanya dengan jawaban tiruan. UAT belum dijalankan | L1, L5 |
| `UAT-09` | Terverifikasi developer dengan perubahan TTV, bukan pemulangan. UAT belum dijalankan | L9 |
| `UAT-10` | Terverifikasi developer dengan memutus `/board`, bukan mencabut jaringan TV. UAT belum dijalankan | L4 |
| Keterbacaan ±3 m (`NV-FE-002`) | **Belum** — menunggu penilaian Rizki di TV | — |
| DoD — lint lulus | Terpenuhi | §6 |
| DoD — rekaman di layar besar termasuk putus jaringan | Sebagian: tangkapan layar 1920×1080 dan 1366×768 dari browser headless, termasuk putus jaringan; belum di TV sungguhan | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` `NOT RUN` |
| Masalah yang diketahui | Indikator dev Next.js ("N" di pojok kiri bawah) ikut tampil di tangkapan layar karena dev server; tidak ada di build produksi |
| Kondisi uji `G-06` | `FloorCode` terisi untuk semua kamar aktif. Versi instrumen risiko jatuh belum `Approved`, sehingga ikon J belum bisa muncul dengan data asli. `OrganizationUnitId` bangsal untuk roster baru diperiksa di `BE-NV-006` |
| Dependency backend | `BE-NV-005` 🟡 |
| Perubahan sampingan | `NONE` |
| Interupsi | Sekitar pukul 09.37 dev server menjawab `500` pada `/login` dan `/nurse-view-display`, karena `use-emergency-registration.js` (IGD, bukan berkas task ini) sedang disunting pihak lain. Berkas itu pulih sendiri pukul 09.38, sebelum L9 dijalankan, dan tidak disentuh |
| Status Git | Versi awal di `61b5e3f35`; sisanya (7 berkas diubah, 3 dihapus) di `2e510a555`, satu commit dengan perubahan IGD dan `FE-NV-006` |
| Langkah berikutnya | Penilaian keterbacaan oleh Rizki; lalu `FE-NV-006` dan `FE-NV-007` |
