# Laporan Perubahan Frontend — `FE-NV-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-016` |
| Judul | Monitor perawat (pengganti tampilan Konsol Panggilan) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-016` |
| Trace | `NV-DEC-058`; `NV-DEC-038`, `040`, `044`, `045`, `053`; `NV-AC-030`–`033`, `035`, `037`, `044` |
| Contract version | api §7 (`0.2.0`), §7a (`0.2.2`, `GET /patient-calls/floor-board`), §10 |
| Dependency | `BE-NV-017` ✅ (uji HTTP 5/5 `PASS`, verifier otorisasi `PASS`), `FE-NV-012` ✅, `FE-NV-013` 🟡 (rekaman bersuara) |
| Wewenang UI | Rancangan "Monitor perawat" dari Rizki, disahkan `NV-DEC-058` (identitas inisial, Tandai selesai = dialog alasan, gantikan Konsol Panggilan) |
| Task mode | `FRONTEND` — repository `QuilvianSystemFrontendDev` branch `RizkiV2` (HEAD `f7874c57c`) |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-11 |
| Status | ✅ **SELESAI 11 November 2026** — AC 7/7 terbukti: uji layar ujung-ke-ujung `uji-monitor.mjs` 13/13 `PASS` dengan data nyata (putaran bersih terakhir 15.00 WIB, sesudah penyempurnaan tombol Layar penuh). Satu temuan uji diperbaiki (join hub konsol tertahan saat React StrictMode). Berkas kolom tabel lama dihapus (`git rm`, rujukan 0). `npx eslint` 0 error 0 warning; grep anti-regresi bersih; `npm run build` `PASS` (574 halaman) |

---

## 1. Keadaan awal

Konsol Panggilan (`FE-NV-012`) berupa Hero + filter lantai + tabel panggilan. Rizki memberi rancangan
"Monitor perawat" yang menggabungkan peta kamar/bed satu lantai dan penanganan panggilan.

## 2. Proses dari sisi pengguna

1. Perawat membuka butir menu Konsol Panggilan Pasien (route dan izin tetap) dan memilih lantai lewat kartu lantai.
2. Ringkasan menampilkan panggilan baru, sedang ditangani, bed terisi, dan bed kosong.
3. Peta kamar menampilkan setiap bed: inisial · JK · umur, status, dan status panggilan dengan durasi.
4. Panel "Panggilan aktif": Darurat dan terlama di atas; "Respons & tangani" (klaim), "Ambil alih", "Lepas",
   dan "Tandai selesai" (membuka dialog alasan wajib, `NV-DEC-053`).
5. Bunyi `FE-NV-013` tetap; status suara, tunda pengingat, dan "Aktifkan suara" ada di kepala layar.
6. "Layar penuh" membuat monitor memenuhi layar (sidebar dan navbar tersembunyi).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/nurse-view-management/patient-call-console/nurse-view-patient-call-console-view.jsx` | Ditulis ulang: merangkai komponen Monitor perawat; dialog Tutup, toast, peringatan basi/galat, dan bunyi dipakai ulang |
| `.../patient-call-console/nurse-monitor/nurse-monitor-header.jsx` | **Baru** — kepala layar: ikon, judul, subjudul bangsal, status suara + tunda, jam server, tombol Layar penuh (teks "Layar penuh"; nama aksesibel dan tooltip "Layar penuh Monitor perawat" agar berbeda dari tombol navbar) |
| `.../nurse-monitor/nurse-monitor-floor-selector.jsx` | **Baru** — kartu lantai (`BaseButton`) + lencana jumlah panggilan |
| `.../nurse-monitor/nurse-monitor-summary.jsx` | **Baru** — empat petak ringkasan bernada warna |
| `.../nurse-monitor/nurse-monitor-room-card.jsx` | **Baru** — kartu kamar + status panggilan kamar (`StatusBadge`) |
| `.../nurse-monitor/nurse-monitor-bed-tile.jsx` | **Baru** — petak bed: label, status, inisial · JK · umur, baris panggilan |
| `.../nurse-monitor/nurse-monitor-call-card.jsx` | **Baru** — kartu panggilan: badge, timer, lokasi, jam tekan, pengklaim, tombol menurut `AvailableActions` |
| `.../nurse-monitor/nurse-monitor-call-panel.jsx` | **Baru** — panel panggilan aktif + slot (tim jaga) |
| `.../nurse-monitor/nurse-monitor-legend.jsx` | **Baru** — legenda status panggilan |
| `src/lib/hooks/.../patient-call-console/use-nurse-view-patient-call-console.jsx` | Memuat `floor-board` (ganti lantai, cadangan 15 detik, Coba lagi); join ulang grup konsol bila join gagal saat tetap tersambung (pola `FE-NV-007`/`011` §3); penanda join yang sedang berjalan terikat pada koneksi + lantai (perbaikan temuan uji §6.2) |
| `src/lib/hooks/.../patient-call-console/use-element-fullscreen.jsx` | **Baru** — layar penuh untuk wadah monitor (callback ref) |
| `src/lib/constants/.../patient-call-console-constants.jsx` | Endpoint `floorBoard`, `NURSE_MONITOR_ACTION_LABEL`, `NURSE_MONITOR_BED_STATUS`, `NURSE_MONITOR_TEXT` (termasuk `fullscreenLabel`, `exitFullscreenLabel`) |
| `.../patient-call-console/nurse-view-patient-call-console-table-columns.jsx` | **Dihapus** (`git rm` atas perintah Rizki "jalankan semua"; rujukan di `src` 0) |
| `src/lib/services/.../patient-call-console-service.jsx` | `getPatientCallConsoleFloorBoard` |
| `src/utils/.../patient-call-console/nurse-monitor-utils.jsx` | **Baru** — normalisasi respons, susun kamar/bed, gabung panggilan per bed, ringkasan, format jam/durasi |
| `src/style/health-services/nurse-view-management/nurse-monitor.module.css` | **Baru** — gaya bertoken; ukuran baca jauh lewat `clamp()` lokal (preseden layar TV/tablet) |

Kolom tabel konsol lama (`nurse-view-patient-call-console-table-columns.jsx`) tidak terpakai sejak tampilan diganti dan sudah dihapus.

### 3.2 Kepatuhan arsitektur dan gerbang base component

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kepala layar | `Hero`, `BaseButton`, `StatusBadge` | `Hero` bergradien bertentangan dengan rancangan; layar TV/tablet Nurse View juga berkepala sendiri | `COMPOSE` | `BaseButton` + markup bertoken |
| Kartu pemilih lantai | `FilterSelect`, `BaseButton`, `BaseCheckboxCard` | `BaseCheckboxCard` = centang ganda; `FilterSelect` = dropdown | `COMPOSE` | `BaseButton variant="secondary"` + `StatusBadge` |
| Empat ringkasan | `SummaryGrid` | Tanpa ikon/nada warna; memperluasnya berisiko ke banyak modul | `WRAP` | Petak domain bertoken |
| Kartu kamar + petak bed | `StatusBadge` | Kartu bed TV berskala layar TV | `WRAP` | Komponen domain |
| Kartu panggilan aktif | `StatusBadge`, `BaseButton` | — | `WRAP` | Komponen domain |
| Legenda | `StatusBadge` | — | `COMPOSE` | Tiga badge + keterangan |
| Filter kamar | `FilterSelect` | — | `REUSE` | — |
| Tim jaga | `InformationAlert` | `title`, `icon`, `closable` | `REUSE` | — |
| Dialog Tutup | `NurseViewPatientCallCloseModal` | `FE-NV-012` | `REUSE` | — |
| Peringatan | `InformationAlert` | — | `REUSE` | — |
| Toast galat | `ToastStack` | — | `REUSE` | — |

`UI GATE: 11 elemen — REUSE 5, EXTEND 0, COMPOSE 3, WRAP 3, NEW 0`.

Pilihan untuk baris WRAP — **A. Komponen domain reusable di folder konsol (Rekomendasi, dijalankan)**: sesuai
permintaan Rizki, tanpa menyentuh base bersama. **B. Extend `SummaryGrid`/`StatusBadge`**: mengubah komponen
yang dipakai banyak modul, risiko regresi lebih besar.

Penyesuaian terhadap gambar: nama lengkap diganti inisial (`NV-DEC-044`); label "Simulasi tampilan" dan catatan
data fiktif tidak dipakai (data nyata); "Matikan bunyi" ditambahkan di kepala layar (`FE-NV-013` AC-2); info
"Ambang lama menunggu belum disahkan" tetap (`FE-NV-012` AC-8); timer kartu "Ditangani" dihitung dari tekan
pertama (`NV-DEC-040`), jam respons tampil di baris pengklaim; "Tandai selesai" juga tersedia untuk panggilan
`Menunggu` bila backend mengizinkan (`AvailableActions`). Kartu kamar memakai kolom otomatis selebar minimal
26rem: dua kolom di samping sidebar, tiga kolom saat Layar penuh.

## 4. State yang ditangani

| State | Yang dilihat |
| --- | --- |
| Belum pilih lantai | Kartu lantai + info "Pilih lantai…" |
| Memuat peta | "Mengambil panggilan aktif..." di area peta |
| Peta gagal | Peringatan "Peta lantai gagal dimuat…", panel panggilan tetap jalan |
| Daftar gagal | Galat + "Coba lagi" |
| Data basi | Pita "Data terakhir … — koneksi terputus" |
| Tanpa panggilan | Panel: "Tidak ada panggilan aktif…"; kamar: "Tanpa panggilan" |
| Suara ditahan browser | Tombol "Aktifkan suara" + peringatan |
| Tanpa izin | `AccessDeniedGate` |

## 5. Endpoint yang dikonsumsi

`GET /patient-calls/floors`, `GET /patient-calls/active`, **`GET /patient-calls/floor-board`** (baru), `POST /patient-calls/{id}/claim|take-over|release|close`; hub `JoinNurseViewCallConsole`.

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` seluruh berkas konsol (view, komponen, hook, util, konstanta, service) | 0 error, 0 warning (3 warning `react-hooks/refs` sempat muncul, diperbaiki dengan callback ref) | `PASS` | Keluaran perintah |
| Grep anti-regresi checklist §G | Warna literal 0; tombol mentah 0; `<table>` 0; utility Bootstrap 0; `!important` 0; inline style 0; dark mode 0. Typography 63 deklarasi, semuanya pada elemen milik view; class yang dipasang ke `BaseButton` hanya tata letak | `PASS` | Keluaran grep |
| Akhiran baris | Semua berkas baru/berubah CRLF, LF 0, tanpa BOM | `PASS` | Hitungan byte Node |
| Pratinjau layar (`uji-fe-nv-016-20261111/pratinjau-monitor.mjs`): data papan asli lantai 4 lewat akun TV, respons `floor-board`/`active` dicegat, 2 panggilan tiruan (Darurat, biasa), akun perawat | 27 kamar, 43 bed, 2 kartu panggilan pada 1920×1080, 1366×900, 390×844; tanpa galat halaman; tanpa klik aksi | `PASS` (simulasi) | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-016-20261111/pratinjau-2026-11-11T04-28-40-050Z/` |
| `npx eslint` hook sesudah perbaikan join | 0 error, 0 warning; CRLF 464, LF 0 | `PASS` | Keluaran perintah, hitungan byte Node |
| Uji layar ujung-ke-ujung sesudah penyempurnaan tombol Layar penuh dan penghapusan berkas lama (11 Nov 15.00 WIB) | **13/13 `PASS`** (putaran 6; M1 kini juga memeriksa jumlah kolom kamar dan nama tombol) | `PASS` | `.../uji-fe-nv-016-20261111/layar-2026-11-11T08-00-45-350Z/` |
| Uji layar ujung-ke-ujung `uji-monitor.mjs` M1–M13 (11 Nov 14.29 WIB) | **13/13 `PASS`** (putaran ketiga; dua putaran sebelumnya lihat §6.2) | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-016-20261111/layar-2026-11-11T07-29-41-793Z/` (14 tangkapan, 3 video, `hasil.json`, `percobaan.json`) |
| Log backend selama uji | 166 baris; 0 `Error`; 0 respons 5xx; 0 token | `PASS` | `Logs/quilvian-backend-20261111.json` 07.29.41–07.30.40 UTC |
| Bersih-bersih | 4 tablet uji dinonaktifkan, 3 panggilan sisa ditutup (`200`); policy Dewi dipulihkan identik (36 = 36); Sari tidak diubah (izin sudah tetap); cek-sisa: tablet uji 0, policy 22/36 | `PASS` | `hasil.json` → `bersihBersih`, `pemulihanPolicy` |
| `npm run build` ulang sesudah penyempurnaan (11 Nov 15.01–15.06 WIB) | Compiled successfully in 2,9 menit; 574/574 halaman; route Monitor terbangun; exit 0 | `PASS` | `.next/BUILD_ID` 15.06.00 WIB |
| `npm run build` (oleh agent atas perintah Rizki, 11 Nov 14.38–14.44 WIB; `next dev` Rizki tetap berjalan) | Compiled successfully in 5,8 menit; 574/574 halaman statis; route `/health-services/nurse-view-management/patient-call-console` terbangun; standalone siap; exit 0 | `PASS` | Log build, `.next/BUILD_ID` 14.43.54 WIB |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest.`
`MANUAL TEST: PASS — uji layar Playwright ujung-ke-ujung dengan dua perawat, akun tanpa izin, dan empat tablet uji (§6.1).`

### 6.1 Rincian uji layar

Aktor: Sari (`PERAWAT_RUANGAN`, izin tetap, 1920×1080), Dewi (`ADMISI`, izin sementara copy–tambah–pulihkan,
1280×800), DOKTER (tanpa izin), tablet uji di 326-C, 325-A, 325-B (lantai 3) dan 420-A (lantai 4). Data pembanding
peta diambil dari `GET floor-board` sesi API Sari. Bunyi dipantau dengan menyadap `HTMLMediaElement.play` dan
`AudioContext.createOscillator` tanpa mengubah perilakunya. Panggilan 420-B milik Rizki (aktif sejak 10.51 WIB)
tidak disentuh.

| No | Skenario | Hasil |
| --- | --- | --- |
| M1 | Tata letak + keadaan kosong (K1) | Menu tampil; judul "Monitor perawat"; kartu lantai 2, 3, 4, 5, 8, 9 = `/floors`, Lantai 3 aktif (diingat); ringkasan new/handled/occupied/empty; 29 kamar, 66 bed = papan API; Bed terisi 4/66, Bed kosong 61 = API; peta "Lantai 3 — Rawat Inap"; panel kosong; legenda; tim jaga; Matikan bunyi; Layar penuh; Pembaruan terakhir; info ambang; tanpa geser datar di 1920 dan 1280 |
| M2 | Saring kamar + layar penuh | "Kamar 325" → 1 kamar, 3 bed (= API), "Satu kamar ditampilkan"; "Semua kamar" kembali 29; Layar penuh masuk dan keluar (`document.fullscreenElement`) |
| M3 | Panggilan baru ≤ 5 detik, urutan (K2) | 325-A tampil di panel dan petaknya bernada `new` dalam 0,17 detik (Sari), 0,19 detik (Dewi); urutan 326-C (Darurat), 325-A, 325-B di kedua layar; petak 326-C "Darurat · Tunggu 00:01"; Panggilan baru 03 |
| M4 | Bunyi tetap (AC-6) | Panggilan biasa → `Audio.play` `queue-dingdong.mp3`; Darurat → 4 oscillator (nada Darurat); browser tidak menahan suara |
| M5 | Identitas (K3) | Kartu "P.P. · L • 34 th", "P.B.T. · L • 31 th", "P.U.P. · L • 41 th"; 12 nilai nama/No. RM pasien lantai 3–4 tidak muncul di layar Sari dan Dewi |
| M6 | Respons & tangani, Ambil alih (K4) | Kartu Sari "Sedang ditangani" + "Perawat Ruangan (Uji 85) • Respons …"; petak "Ditangani · 00:08"; Sedang ditangani 01; tablet 325-A menerima nama yang sama; Dewi hanya ditawari "Ambil alih"; sesudahnya Dewi memegang (Lepas), Sari ditawari Ambil alih, tablet berganti ke nama Dewi |
| M7 | Lepas (K5) | Kartu kembali "Panggilan baru" + "Respons & tangani", petak `new`; timer 10 detik vs 11 detik sejak tekan pertama |
| M8 | Klaim bersamaan (K6) | Tepat satu pengklaim (status 2); hanya layar yang kalah menampilkan pesan "sudah diambil/sudah berubah"; kedua layar menampilkan pengklaim |
| M9 | Tandai selesai (K7, AC-5) | Dialog "Tutup panggilan dari 326-C?"; tanpa alasan → "Pilih alasan…"; Lainnya tanpa keterangan → pesan keterangan, panggilan tetap aktif; dengan keterangan → tertutup, alasan 4 + catatan di DB, kartu hilang, petak `none` |
| M10 | DOKTER (K8) | Menu tidak tampil; route langsung → Akses Ditolak; 0 petak, 0 kartu |
| M11 | Pindah lantai (K9) | Lencana Lantai 3 = 2 (= DB); Lantai 4 → 43 bed, 420-A tampil 0,16 detik, bisa ditangani; lencana Lantai 4 = 2 kartu (420-B Rizki + 420-A); lantai "4" diingat; identitas lantai 4 tidak tampil |
| M12 | K10 | `nurseCallContext.jsx` tetap tidak ada |
| M13 | K11 | 0 galat halaman |

### 6.2 Temuan uji dan perbaikannya

| Putaran | Hasil | Penyebab | Tindakan |
| --- | --- | --- | --- |
| 1 (14.21 WIB) | Berhenti di M2 | Ada dua tombol "Layar penuh" (navbar aplikasi dan kepala Monitor) | Skrip: tombol dicari di dalam kepala Monitor. Lihat catatan UX §8 |
| 2 (14.22 WIB) | 9/13; M3 gagal 8,33 detik | **Cacat source:** `reactStrictMode: true` membuat effect realtime berjalan–dibersihkan–berjalan lagi di `next dev`. Penanda join (`consoleJoiningRef`) hanya menyimpan lantai, sehingga join koneksi kedua dibatalkan oleh join koneksi pertama yang sudah dihentikan. Grup konsol baru di-join pada putaran 15 detik. Bukti log: `/hubs/queues` selesai 7 ms sesudah negotiate, negotiate berikutnya tepat 15 detik kemudian | Penanda join diikat pada `{ connection, floorCode }`. Penanda ini masuk dalam perubahan task ini; HEAD tidak terdampak |
| 2 | M6, M7, M11 gagal | **Cacat skrip:** `innerText` mengikuti `text-transform` lencana ("Sedang Ditangani"); regex lencana lantai kehilangan backslash lewat heredoc. Tangkapan layar menunjukkan tampilan benar | Pencocokan tidak peka huruf; lencana dibaca dari `.region-status-label` |
| 3 (14.29 WIB) | **13/13** | — | — |
| 4 (14.55 WIB) | 13/13 dengan grid kamar 24,5rem | Percobaan dua kolom di 1280 (catatan UX §8). Tangkapan 1280 menunjukkan petak bed menyempit menjadi ±170 px: "Belum ada pasien" terpotong, "Tidak ada panggilan" dan "Darurat · Tunggu 00:02" terlipat | **Dibatalkan**, kembali ke 26rem (satu kolom di 1280, petak 379 px, teks utuh). Ukuran: `ukur-grid.mjs` (1280: wadah 811 px; 1366: 637 px; 1920: 1191 px; Layar penuh: 1433 px) |
| 5 (14.58 WIB) | Berhenti sesudah M7 (7/7) | DB dev bersama timeout (psql `Connection timed out`), sehingga bersih-bersih di `finally` gagal: 3 panggilan dan 4 tablet uji tertinggal; policy Dewi tetap pulih lewat API | Dibersihkan lewat API saja (`bersihkan-sisa-api.mjs`): 3 panggilan ditutup Sari (alasan Lainnya, `200`), 4 tablet dinonaktifkan (`200`); cek-sisa bersih |
| 6 (15.00 WIB) | **13/13** | Grid 26rem + nama aksesibel tombol Layar penuh | Kamar 2 kolom di 1920, 1 kolom di 1280, 3 kolom saat Layar penuh; tombol bernama "Layar penuh" di halaman tinggal satu (navbar); bersih-bersih semua `200`; policy Dewi identik |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 tata letak sesuai rancangan | ✅ | M1, M2; tangkapan `M1-sari-1920-kosong.png`, `M1-dewi-1280-kosong.png` |
| AC-2 identitas hanya inisial · JK · umur | ✅ | M5, M11 |
| AC-3 panggilan baru ≤ 5 detik di panel dan petak bed, Darurat dulu | ✅ | M3 (0,17 / 0,19 detik), M11 (0,16 detik) |
| AC-4 Respons & tangani mengklaim; petak dan panel "Ditangani"; tablet "… menuju" | ✅ | M6 |
| AC-5 Tandai selesai → dialog alasan wajib | ✅ | M9 |
| AC-6 bunyi `FE-NV-013` tetap | ✅ (pemicu) | M4. Bunyi yang benar-benar terdengar tetap dibuktikan rekaman `FE-NV-013` |
| AC-7 regresi uji konsol K1–K11 (disesuaikan) | ✅ | M1, M3, M5–M13 |
| DoD — AC-1–7 terbukti; laporan | Terpenuhi | Berkas ini |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Halaman panjang pada lantai dengan banyak kamar satu bed (lantai 4: 27 kamar); filter kamar membantu |
| Catatan UX (diselesaikan 11 Nov atas perintah "jalankan semua") | (1) Dua tombol "Layar penuh" (navbar aplikasi dan kepala Monitor): teks tombol Monitor tetap sesuai rancangan, nama aksesibel dan tooltip menjadi "Layar penuh Monitor perawat". (2) Grid dua kolom di 1280 dicoba lalu dibatalkan karena petak bed terlalu sempit (§6.2 putaran 4). Satu kolom di 1280 dipertahankan, dengan filter kamar sebagai alat ringkas |
| Status Git | Di-commit dan di-push 11 Nov 2026 atas perintah Rizki: frontend `RizkiV2` (`c3ecd1185` + commit penyempurnaan), laporan di backend `rizkiG` |
| Langkah berikutnya | Merge ke branch integrasi oleh Rizki; rekaman bersuara `FE-NV-013` |
