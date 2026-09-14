# Laporan Perubahan Frontend — `FE-BD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-006` |
| Judul | Seluruh layar Bank Darah terjangkau dari menu |
| Slice | Roadmap frontend Bank Darah — registrasi menu modul |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §4 |
| Trace | `03-frontend-architecture.md` §2 — peta butir menu, tingkat, induk, dan butir hak akses |
| Contract version | `v4` — ✅ **`approved`** (`Sukmagp`, 2026-09-03) |
| Wewenang UI | Rupa layar `DEV_DISCRETION`. Yang dikunci: susunan menu, induk tiap butir, dan pemetaannya ke layar |
| Dependency | `G1` ✅ — **nol dependency backend** |
| Klasifikasi | `LIGHT` — satu berkas, 18 baris dihapus, nol berkas baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `V2QuilvianSystemFrontendDev` (source) + `docs/module-blueprints/bank-darah/` pada backend (laporan & bukti roadmap saja) |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `f79af1684` cabang `sukmagpV2` |
| Commit backend yang dijadikan rujukan | `95e4b8d` cabang `sukmagp` |
| Tanggal | `2026-09-10` |
| Status | 🟡 **SELESAI SEBAGIAN.** Separuh outcome tercapai dan terbukti; separuh lagi **tidak dapat dikerjakan dalam scope task ini** karena mekanismenya belum ada di frontend |

---

## 1. Keadaan yang ditemukan di awal

Kartu task menyebut scope `menu-items.jsx` dan outcome dua bagian: **(a)** setiap layar Bank Darah
dapat dicapai dari menu, dan **(b)** butir menu hanya tampil bagi peran yang berhak.

### 1.1 Bagian (a) ternyata sudah hampir tuntas — tetapi menyimpang dari kontrak

Tiga layar Bank Darah sudah berdiri: `FE-BD-08` Katalog Komponen Darah, `FE-BD-09` Daftar Alasan
Terkendali, dan `FE-BD-10` Lokasi Penyimpanan Darah. Ketiganya **sudah** terdaftar di menu —
dua oleh `FE-BD-001`, satu oleh `FE-BD-011`.

**Masalahnya: ketiganya terdaftar dua kali.** Sekali di grup generik **Pelayanan Kesehatan →
Master Data**, sekali lagi di **Bank Darah → Setup**.

Bahwa itu penyimpangan, bukan pola rumah, dibuktikan dengan menghitung seluruh berkas:

| Pemeriksaan | Hasil |
| --- | ---: |
| Total `pathname` pada `menu-items.jsx` | 153 |
| Path unik | 149 |
| **Path yang muncul lebih dari sekali** | **4** |

Tiga dari empat duplikat itu adalah layar Bank Darah. Satu sisanya milik modul lain
(`/health-services/registration-management/doctor-queues`) dan berada di luar scope task ini.

Kontrak `03-frontend-architecture.md` §2 juga tidak mengenal penempatan ganda itu. Ia menempatkan
**seluruh** butir Bank Darah di bawah induk `Bank Darah`, dengan `Setup` sebagai grup tingkat 1 —
dan nol butir di bawah grup Master Data generik. Label yang dipakai entri duplikat pun berbeda dari
kontrak: "Komponen Darah" dan "Alasan Bank Darah", bukan "Katalog Komponen Darah" dan "Daftar Alasan
Terkendali".

### 1.2 Bagian (b) tidak punya mekanisme sama sekali

Penelusuran source memulangkan temuan yang menentukan status task ini:

| Yang diperiksa | Keadaan |
| --- | --- |
| `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx` | **Stub.** Admin dan Manajer dikembalikan menu utuh; untuk peran lain seluruh logika filternya **dikomentari**, sehingga fungsinya memulangkan daftar yang sama persis |
| Pemanggilnya | `left-sidebar-items-virtualized.jsx:236` — dipanggil, tetapi hasilnya identik dengan masukannya |
| `access-denied-gate.jsx` | **Reaktif saja** — ia menampilkan pesan setelah backend memulangkan `403`, bukan sumber hak akses yang dapat dibaca sebelum request |
| Slice hak akses pengguna berjalan | **Nihil.** `roleSlice.jsx` mengurus administrasi role dan posisi, bukan permission milik pengguna yang sedang login |
| Endpoint permission pengguna berjalan | **Nihil** — nol pemanggilan semacam `my-access` atau `menuAccess` di seluruh source |

**Akibatnya:** hari ini setiap butir menu tampil bagi **setiap** pengguna yang punya peran apa pun.
Frontend tidak memiliki katalog permission yang dapat dibaca, sehingga menyembunyikan butir menu
menurut hak akses **tidak dapat dikerjakan hanya dengan menyunting `menu-items.jsx`**.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya** setiap petugas yang membuka aplikasi dan mencari layar Bank Darah dari sidebar.

### 2.1 Alur normal

1. Petugas membuka sidebar dan menemukan grup **Bank Darah**.
2. Di dalamnya ada satu sub-grup **Setup**.
3. Sub-grup itu memuat tiga butir yang mengarah ke tiga layar yang benar-benar ada.
4. Mengkliknya membuka layar yang dimaksud.

### 2.2 Yang berubah bagi pengguna

**Sebelum:** ketiga layar muncul dua kali di sidebar — di Master Data generik dan di Bank Darah →
Setup — dengan nama yang berbeda-beda untuk layar yang sama. Petugas yang mencari "Alasan Bank
Darah" dan petugas yang mencari "Daftar Alasan Terkendali" berakhir di layar yang sama tanpa tahu
keduanya identik.

**Sesudah:** ketiganya muncul **tepat sekali**, di bawah Bank Darah → Setup, dengan nama sesuai
kontrak.

### 2.3 Jalur tidak normal — layar yang belum ada

Tujuh layar sisanya — `FE-BD-01` Order Darah, `FE-BD-03` Permintaan PMI, `FE-BD-04` Kantong Darah,
`FE-BD-06` Pemeriksaan Golongan Darah, `FE-BD-07` Tindakan Bank Darah, ditambah layar anak
`FE-BD-02` dan `FE-BD-05` — **belum dibangun**. Kartu task memerintahkan:

> Butir menu yang menunjuk layar belum ada **wajib disembunyikan**, bukan menampilkan halaman kosong.

Ketujuhnya karena itu **tidak** didaftarkan. Petugas tidak akan menemukan butir menu yang mengarah
ke halaman kosong.

### 2.4 Jalur tidak normal — tanpa hak akses

**Belum tertangani.** Butir menu tetap tampil walau pengguna tidak berhak; penolakan baru terjadi
ketika layarnya dibuka dan backend memulangkan `403`, yang lalu ditampilkan `AccessDeniedGate`.
Pengguna tetap terlindungi — data tidak bocor — tetapi ia melihat pintu yang tidak bisa dibukanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` frontend · `rules/GLOBAL_RULES.md` · `rules/rule-output/status-task-roadmap.md` · `rules/rule-output/grafik-dependency-roadmap.md` (**baru pada suite 1.18.0**) |
| Kontrak menu | `03-frontend-architecture.md` §2 — tabel butir menu, tingkat, induk, layar, butir hak akses |
| Source menu | `src/utils/menu-sidebar/menu-items.jsx` |
| Mekanisme akses | `filter-menu-items-by-role.jsx` · `left-sidebar-items-virtualized.jsx` · `access-denied-gate.jsx` · `roleSlice.jsx` |
| Roadmap | `roadmap/frontend-roadmap.md` §4 kartu `FE-BD-006` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | **18 baris dihapus.** Tiga entri Bank Darah yang menduplikasi layar Setup dibuang dari grup `healthServicesMasterData`. Nol baris ditambahkan |

**Nol berkas baru.** **Nol berkas lain disentuh.**

Ketiga ikon yang dipakai entri terhapus — `RiFlaskLine`, `RiFileList3Line`, `RiMapPinLine` — tetap
terpakai di tempat lain (1, 35, dan 1 pemakaian tersisa), sehingga tidak ada import yang menjadi
yatim.

### 3.3 Kepatuhan arsitektur frontend

Task ini tidak membuat route, view, hook, slice, maupun style. Ia hanya membuang data menu yang
menyimpang dari kontrak, sehingga alur dependensi frontend tidak tersentuh sama sekali.

Susunan menu sesudah perubahan **cocok persis** dengan `03-frontend-architecture.md` §2 untuk
seluruh butir yang layarnya sudah ada:

```text
Bank Darah                                  <- tingkat 0
└── Setup                                   <- tingkat 1
    ├── Katalog Komponen Darah    -> /health-services/master-data/blood-components
    ├── Daftar Alasan Terkendali  -> /health-services/master-data/blood-bank-reasons
    └── Lokasi Penyimpanan Darah  -> /health-services/master-data/blood-storage-locations
```

### 3.4 Gerbang keputusan base component

Task ini **tidak menyusun satu layar pun**. Yang disentuh adalah data menu; satu-satunya JSX di
sana adalah komponen ikon yang sudah ada, dan task ini **menghapus** pemakaiannya, bukan menambah.

| Elemen | Status | Keterangan |
| --- | --- | --- |
| Butir menu | `NOT APPLICABLE` | Data konfigurasi, bukan komponen tampilan |
| Ikon menu | `REUSE` | `react-icons/ri` yang sudah di-import; nol ikon baru, nol import baru |
| Komponen tampilan | `NOT APPLICABLE` | Nol layar disusun |

`UI GATE: PASSED — 0 elemen NEW, 0 EXTEND, 0 menunggu keputusan pengguna.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — menu dirender dari konstanta statis, tanpa permintaan jaringan |
| Kosong | `NOT APPLICABLE` — grup Bank Darah selalu memuat sekurang-kurangnya tiga butir Setup |
| Gagal | `NOT APPLICABLE` — nol pemanggilan API pada jalur ini |
| Tanpa hak akses | **BELUM tertangani** — butir menu tetap tampil bagi pengguna yang tidak berhak. Penolakan baru terjadi di layarnya lewat `403` dan `AccessDeniedGate`. Lihat §1.2 dan §7 |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini nol memanggil API. Butir menu hanya menunjuk route frontend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/utils/menu-sidebar/menu-items.jsx` | **Keluaran kosong** — `0 error, 0 warning` | `PASS` | Dijalankan terpisah untuk memisahkan kontribusi task ini |
| `npm run lint` seluruh repository | **`608 problems (0 errors, 608 warnings)`** | `PASS` | Sama persis garis dasar; **nol** dari berkas task ini |
| `npm run build` | **`✓ Compiled successfully in 33.9s`** | `PASS` | `postbuild` standalone juga berhasil |
| Ketiga path menu punya route nyata | **3 dari 3 terbukti** | `PASS` | Keluaran build memuat `/blood-components`, `/blood-bank-reasons`, `/blood-storage-locations`, masing-masing beserta `create`, `[slug]`, dan `[slug]/update` |
| Duplikat path menu sesudah perubahan | **150 path, 149 unik, 1 duplikat tersisa** | `PASS` | Ketiga duplikat Bank Darah hilang; sisa satu milik modul lain (`doctor-queues`) di luar scope |
| Import ikon menjadi yatim | **Nol** | `PASS` | `RiFlaskLine` 1, `RiFileList3Line` 35, `RiMapPinLine` 1 pemakaian tersisa |
| `node --test tests/unit` | **`pass 434, fail 0`** | `PASS` | Nol regresi |
| Grep anti-regresi UI — warna, typography, button, tabel, `!important` | Nihil seluruhnya | `PASS` | Lima grep bersih |
| Grep anti-regresi UI — utility `fs-` | 163 baris | `PASS` | Konvensi ikon menu yang sudah berlaku untuk seluruh ~150 entri; task ini **menghapus 3 pemakaian dan menambah nol** |

**Uji manual: `NOT FEASIBLE`.** Menyaksikan sidebar menuntut aplikasi berjalan beserta sesi login
yang sah, dan backend menunjuk database yang tabelnya lengkap. Wewenang menjalankan aplikasi tidak
diberikan task ini. Yang **dapat** dibuktikan tanpa menjalankan aplikasi sudah dibuktikan: susunan
menu cocok kontrak, ketiga path punya route nyata, nol duplikat tersisa, dan nol regresi pada 434
kasus uji.

**Tidak dijalankan:** `npm run test:e2e` — menuntut aplikasi berjalan, dan menurut
`rules/frontend/test-policy.md` test otomatis baru bersifat opsional dan bukan gerbang selesai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Butir menu mengarah ke layar yang **hak aksesnya benar** — bagian *mengarah ke layar* | ✅ **Terpenuhi** | Ketiga butir Setup menunjuk route yang terbukti ada pada keluaran build, dengan label dan induk sesuai `03-frontend-architecture.md` §2. Tujuh layar yang belum dibangun **tidak** didaftarkan, sesuai catatan urutan |
| Butir menu **hanya tampil bagi peran yang berhak** | ⛔ **BELUM terpenuhi** | Mekanismenya tidak ada. `filterMenuItemsByRole` adalah stub yang seluruh logikanya dikomentari; frontend tidak memiliki katalog permission pengguna berjalan. **Tidak dapat dikerjakan dengan menyunting `menu-items.jsx`** — lihat §1.2 dan §8 |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Registrasi menu menjadi acceptance salah satu task layar, bukan pekerjaan berdiri sendiri | ✅ **Terpenuhi, dan memang begitu yang terjadi** — pendaftaran ketiga butir dikerjakan `FE-BD-001` dan `FE-BD-011` bersama layarnya masing-masing. Yang tersisa bagi task ini adalah merapikan penyimpangan, bukan mendaftarkan dari nol |
| Butir menu yang menunjuk layar belum ada wajib disembunyikan | ✅ **Terpenuhi** — tujuh layar tanpa source tidak punya butir menu |
| Susunan menu cocok kontrak `03-frontend-architecture.md` §2 | ✅ **Terpenuhi** untuk seluruh butir yang layarnya ada |

**Satu dari dua acceptance criteria terpenuhi.** Karena itu task ini 🟡 **SEBAGIAN**, bukan ✅.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` memulangkan 608 warning pada seluruh repository, **nol** dari berkas task ini |
| Masalah yang diketahui | Visibilitas menu menurut hak akses belum ada. Pengguna tanpa hak tetap melihat butir menunya, dan baru ditolak ketika layarnya dibuka. **Data tidak bocor** — penolakannya ditegakkan backend — tetapi pengalamannya membingungkan |
| **Temuan 1 — `filterMenuItemsByRole` adalah stub** | Seluruh logika filter di dalamnya dikomentari, sehingga fungsi itu memulangkan menu yang sama persis untuk setiap peran. Yang tersisa aktif hanyalah jalan pintas `Admin`/`Manajer`. Menutup ini bukan pekerjaan `menu-items.jsx`: ia menuntut sumber permission pengguna berjalan, pemetaan tiap butir menu ke `Resource : Action`, lalu penyaringan. Itu kemampuan lintas modul, bukan milik Bank Darah |
| **Temuan 2 — nama peran ditanam di kode** | `filterMenuItemsByRole` memakai literal `"Admin"`, `"Manajer"`, dan pada blok yang dikomentari `"Perawat"`, `"Dokter"`. Backend melarang keras penentuan kewenangan lewat nama peran; prinsip yang sama sebaiknya berlaku di sini, sehingga perbaikannya membaca **hak akses yang diberikan**, bukan nama peran |
| **Temuan 3 — satu duplikat menu milik modul lain** | `/health-services/registration-management/doctor-queues` masih muncul dua kali. Di luar scope Bank Darah, dilaporkan tanpa diubah |
| Dependency backend | `NOT APPLICABLE` — nol dependency backend, sesuai kartu task |
| Perubahan sampingan | `NONE` |
| Interupsi | **Ada, dan dipulihkan.** Percobaan pertama task ini berhenti karena branch frontend yang di-checkout adalah `QuilvianDevV2`, sedangkan roadmap menetapkan `sukmagpV2`. Sesuai `AGENTS.md`, branch **tidak** diganti otomatis; ketidaksesuaian dilaporkan, pengguna memindahkannya sendiri, lalu pekerjaan dilanjutkan di `sukmagpV2` |
| Status Git | Lihat §9 |
| Langkah berikutnya | **(1)** Jadwalkan kemampuan visibilitas menu berbasis hak akses sebagai task tersendiri lintas modul — itu yang menutup acceptance kedua `FE-BD-006`. **(2)** Daftarkan lima butir menu operasional Bank Darah bersama task layarnya masing-masing, sesuai DoD, bukan sebagai task menu terpisah. **(3)** Bereskan duplikat `doctor-queues` lewat pemilik modul Registration |

---

## 9. Status Git

```text
 M src/utils/menu-sidebar/menu-items.jsx
```

Frontend `HEAD` tetap `f79af1684` cabang `sukmagpV2`, upstream `origin/sukmagpV2`. Backend
**tidak disentuh** di luar berkas laporan ini beserta bukti roadmap dan traceability.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. Nol perintah database dijalankan.
