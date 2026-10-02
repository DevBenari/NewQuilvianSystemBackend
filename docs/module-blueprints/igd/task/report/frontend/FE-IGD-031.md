# Laporan Perubahan Frontend — `FE-IGD-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-031` |
| Judul | Segmen Formulir dan Riwayat pada tab pemeriksaan |
| Slice | `IGD-S04`, `IGD-S05` · lanjutan `FE-IGD-022` (ruang kerja pemeriksaan IGD) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.9 |
| Trace | `IGD-DEC-133`; evidence [`2026-09-16-tata-letak-riwayat-pemeriksaan.md`](../../../evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md). **Coverage gap:** tanpa `FR-IGD-*` |
| Contract version | **Nol perubahan kontrak, nol endpoint baru, nol ruas data baru, nol perubahan backend** |
| Wewenang UI | `03-frontend-architecture.md:247`, `:328`, `:329` — `DEV_DISCRETION`. Batas `:331` (isi dan sumber data dikunci) dipatuhi; `:249` dan `:250` (larangan palet dan pustaka baru) dipatuhi |
| Dependency | `IGD-DEC-133` ✅ |
| Klasifikasi | `MEDIUM` — satu komponen pembungkus baru, satu util baru, lima tab memakainya; nol layar baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source) + laporan ini pada blueprint IGD |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `36f122af9` (branch `RizkiV2`) + working tree |
| Commit backend yang dijadikan rujukan | `8544af1c` (branch `rizkiG`), strict read-only |
| Tanggal | 16 September 2026 |
| Status | 🟡 **SEBAGIAN — 16 September 2026.** Ketiga belas acceptance criteria **terpetakan ke source**. `npm run lint:errors` **PASS**; `node --import ./tests/helpers/register.mjs --test tests/unit` **866/866 lulus** (859 lama + 7 baru). **`npm run build` LULUS 17 September 2026** (exit 0, 0 error, 0 warning). **Uji lewat layar belum dijalankan** — itu satu-satunya sisa penahan ✅. Bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Ruang kerja Pemeriksaan IGD memasang satu `role="tablist"` berisi tujuh tab
(`emergency-assessment-detail-view.jsx:235-260`). Setiap tab menumpuk isinya pada satu kolom:
kartu formulir lebih dulu, lalu daftar riwayat di bawahnya.

Dua akibatnya:

1. **Riwayat praktis tidak terbaca.** Pada tab Assesmen Awal IGD, daftar riwayat baru muncul
   sesudah tanda vital, pengkajian, pengkajian lanjutan, catatan psikososial, catatan edukasi,
   dan blok penyelesaian dilewati.
2. **Penyimpanan yang berhasil tidak terlihat.** Pada
   `emergency-assessment-initial-tab.jsx:116-131`, penyimpanan yang berhasil me-reset formulir
   lalu memanggil `reload()`. Posisi gulir tidak berpindah, sehingga yang dilihat perawat
   hanyalah formulir yang tiba-tiba kosong — bentuk umpan balik yang sama persis dengan
   kegagalan simpan.

Quilvian V1 menyelesaikan keduanya dengan memisahkan formulir dan riwayat menjadi dua tab
`react-bootstrap` yang berpindah sendiri sesudah submit (`assesment-awal-tabs.jsx:13-20`).

---

## 2. Proses bisnis dari sisi pengguna

**Sebelum.** Perawat membuka tab Assesmen Awal IGD, menggulir melewati seluruh formulir untuk
memastikan pasien ini belum dikaji, menggulir kembali ke atas untuk mengisi, menekan Simpan,
lalu melihat formulir kosong tanpa tahu apakah datanya masuk.

**Sesudah.** Perawat membuka tab yang sama dan melihat dua segmen: **Formulir** dan
**Riwayat (3)**. Angka pada segmen Riwayat sudah memberi tahu bahwa pasien ini pernah dikaji.
Di atas formulir ada satu baris tetap: *"Terakhir dikaji — 16 Sep 2026 14.05 · Ns. Dwi · Nyeri
dada · skala 6"*. Perawat mengisi, menekan Simpan, dan layar **berpindah sendiri** ke segmen
Riwayat dengan baris barunya di daftar. Untuk mengkaji lagi, ia menekan **Pengkajian baru**.

Bila penyimpanan **gagal**, layar tetap pada segmen Formulir beserta seluruh isian, dan pesan
galat tampil apa adanya di tempatnya sekarang.

**Kenapa baris "terakhir dikaji" ada.** Menyembunyikan riwayat memindahkan risiko, bukan
menghapusnya. Di IGD satu bed dapat disentuh dua perawat dalam satu giliran; tanpa pengingat
yang tetap terlihat, pengkajian ganda menghasilkan dua dokumen sah yang saling bertentangan.
V1 membiarkan lubang ini terbuka.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `emergency-assessment-detail-view.jsx` | Memastikan jumlah `role="tablist"` tetap satu |
| `ui/doctor-clinical-base/ClinicalSegmentedNav.jsx` + modul CSS-nya | Komponen segmen yang dipakai ulang; komentarnya menjelaskan kenapa tablist kedua ditolak |
| `physician-workspace/tabs/medication-procedure/prescription-procedure-tab.jsx:88` | Preseden pemakaian segmen di dalam satu tab klinis |
| `app/globals.css` | Memastikan token `--color-primary`, `--radius-pill`, `--focus-ring`, `--app-control-min-height`, `--color-surface-soft` memang ada — **baca saja, tidak disentuh** |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` (backend) | Memastikan `AssessmentByUserName` memang sudah ada pada proyeksi `PatientAssessmentResponse` — **baca saja** |
| Kelima tab yang diubah | Bentuk `return`, penanganan sukses simpan, dan komponen yang dipakai |

### 3.2 Berkas yang berubah

Angka diambil dari `git diff -w --stat`, yaitu **mengabaikan pergeseran indentasi**. Sebagian
besar baris pada diff mentah hanyalah blok JSX yang bergeser dua spasi karena berpindah dari
`<>…</>` ke dalam `const formContent = (…)`.

| Berkas | Sifat | Perubahan (tanpa indentasi) |
| --- | --- | --- |
| `components/…/emergency-assessment-work-panel.jsx` | **Baru** | 125 baris |
| `utils/…/emergency-assessment-summary.utils.js` | **Baru** | 85 baris |
| `tests/unit/emergency-assessment-summary.test.mjs` | **Baru** | 7 test |
| `…/emergency-assessment-initial-tab.jsx` | Ubah | 83 |
| `…/emergency-assessment-transfer-tab.jsx` | Ubah | 47 |
| `…/emergency-assessment-nosocomial-tab.jsx` | Ubah | 24 |
| `…/emergency-assessment-disposition-tab.jsx` | Ubah | 24 |
| `…/emergency-assessment-diagnostic-support-tab.jsx` | Ubah | 24 |
| `style/…/emergency-assessment.module.css` | Ubah | +94 (kelas panel, aksi, dan baris ringkas) |

### 3.3 Kepatuhan arsitektur frontend

| Butir | Keadaan |
| --- | --- |
| Komponen bersama diubah | **Nol.** `ClinicalSegmentedNav` dipakai apa adanya |
| CSS global | **Nol.** Kelas baru hanya pada modul CSS layar IGD |
| Palet warna baru | **Nol.** Warna diambil dari nilai yang sudah dipakai berkas modul itu: `#0b8290`, `#13a6b6`, `#dce8ed`, `#f3fbfc`, `#12314d`, `#758a9d` |
| Pustaka baru | **Nol.** `react-bootstrap` `Tabs` **tidak** dipakai |
| Endpoint baru | **Nol** |
| Perubahan backend | **Nol** |

### 3.4 Gerbang keputusan base component — reuse atau baru

| Elemen | Putusan | Bukti |
| --- | --- | --- |
| Segmen Formulir/Riwayat | **REUSE** `ClinicalSegmentedNav` | Sudah `radiogroup`, sudah mendukung badge angka, sudah memakai token yang sama |
| Kartu riwayat | **REUSE** `EmergencyAssessmentRecordTab` dan `EmergencyAssessmentSection` | Dipindah tempat, nol perubahan isi |
| Kartu formulir | **REUSE** `EmergencyAssessmentFormCard` | Nol perubahan |
| Tombol kembali ke formulir | **REUSE** `styles.ghostButton` | Sudah dipakai aksi sekunder pada layar ini |
| Pembungkus dua segmen | **BARU**, lokal modul IGD | Tidak ada komponen di repo yang memasangkan formulir dan riwayat beserta perpindahan otomatis sesudah simpan. Ditaruh di folder komponen IGD, **bukan** di `ui/`, karena perilakunya khas layar ini |
| Baris "terakhir dikaji" | **BARU**, lokal berkas tab | Tidak ada strip ringkas sejenis. Logikanya dipisah ke util supaya dapat diuji |

---

## 4. State yang ditangani di layar

| State | Tempat | Keterangan |
| --- | --- | --- |
| Segmen aktif | `emergency-assessment-work-panel.jsx` | Bawaan `Formulir` |
| Perpindahan sesudah simpan | `savedSignal` naik pada tiap tab | Disesuaikan **saat render**, bukan di dalam `useEffect` — pola resmi React untuk "menyesuaikan state ketika prop berubah", dan tidak memicu peringatan `react-hooks/set-state-in-effect` |
| Ringkasan terakhir dikaji | `useMemo` pada tab Assesmen Awal | Dihitung dari `section.items` yang sudah dimuat |
| Formulir koreksi transfer | `amendment` pada tab Transfer | Kini ditampilkan di dalam baris kejadiannya |

---

## 5. Endpoint yang dikonsumsi

**Nol perubahan.** Seluruh thunk, URL, parameter, dan bentuk payload sama persis dengan
sebelum task ini. Tidak ada pemanggilan baru yang ditambahkan — termasuk untuk baris "terakhir
dikaji", yang seluruhnya dibentuk dari daftar riwayat yang memang sudah dimuat.

---

## 6. Verifikasi

### 6.1 Perintah yang dijalankan

| Perintah | Hasil |
| --- | --- |
| `npx eslint` pada berkas yang disentuh | **PASS**, nol error, nol warning baru |
| `npm run lint:errors` (seluruh repo) | **PASS**, nol keluaran |
| `node --import ./tests/helpers/register.mjs --test tests/unit` | **866/866 lulus** — 859 lama, 7 baru |

### 6.2 Test baru

`tests/unit/emergency-assessment-summary.test.mjs`, tujuh test, mengunci perilaku yang paling
berisiko pada baris "terakhir dikaji": daftar kosong, pemilihan terbaru dari **waktu** dan bukan
dari urutan baris, baris tanpa waktu, skala nyeri `0` yang tidak boleh ikut hilang, nama
pencatat kosong yang **tidak** boleh berubah menjadi GUID, dan kunci ruas yang dikunci agar
penambahan ruas baru ketahuan.

### 6.3 Yang **belum** dijalankan

| Butir | Keadaan |
| --- | --- |
| `npm run build` | ✅ **LULUS 17 September 2026** — `npm run build` exit 0, **0 error, 0 warning**, postbuild standalone siap; keempat route IGD terkompilasi |
| Uji lewat layar | **Belum.** `NOT FEASIBLE` bagi agent — menuntut kredensial petugas dan backend berjalan |
| UAT | **Belum**, dan bukan milik pekerjaan ini |

---

## 7. Acceptance criteria dan Definition of Done

| No | Kriteria | Keadaan | Bukti pada source |
| ---: | --- | :-: | --- |
| 1 | Lima tab punya segmen Formulir/Riwayat berbadge | ✅ source | `emergency-assessment-work-panel.jsx:89-101`; dipakai kelima tab |
| 2 | Grup radio, bukan tablist kedua; `Tabs` react-bootstrap tidak dipakai | ✅ source | `ClinicalSegmentedNav.jsx` memakai `role="radiogroup"`; grep `Tabs` pada berkas yang disentuh **nihil** |
| 3 | Simpan berhasil memindahkan ke Riwayat dan memuat ulang daftarnya | ✅ source | `work-panel.jsx:64-70`; `setSavedSignal` pada kelima tab, selalu **sesudah** `reload()` |
| 4 | Ada aksi kembali ke Formulir dari sisi Riwayat | ✅ source | `work-panel.jsx:110-118` |
| 5 | Simpan gagal tidak memindahkan segmen | ✅ source | `setSavedSignal` hanya di dalam cabang `fulfilled.match` |
| 6 | Baris "terakhir dikaji" tetap terlihat saat Formulir aktif; sembunyi bila riwayat kosong | ✅ source | `initial-tab.jsx:53-81`; `summary` hanya dirender pada cabang `isForm` |
| 7 | Baris itu dibentuk dari daftar yang sudah dimuat — nol endpoint, ruas, dan sumber baru | ✅ source | `emergency-assessment-summary.utils.js`; `AssessmentByUserName` sudah ada pada `PatientAssessmentResponse` backend |
| 8 | Koreksi kejadian transfer menjadi aksi pada baris riwayat | ✅ source | `transfer-tab.jsx` — formulir koreksi kini di dalam `<li>` kejadian, tombolnya menyembunyikan diri saat formulirnya terbuka |
| 9 | SOAP, Catatan Terintegrasi, dan Resep tidak disentuh | ✅ source | `git status` — ketiganya tidak ada dalam daftar berkas berubah |
| 10 | Setiap ruas yang tampil hari ini tetap tampil dari sumber yang sama | ✅ source | Diff `-w` pada kelima tab tidak memuat penghapusan ruas |
| 11 | Nilai kosong tetap tanda hubung | ✅ source | `initial-tab.jsx:67-77` |
| 12 | Nol palet warna baru | ✅ source | Bagian 3.3 |
| 13 | Nol pustaka baru, nol komponen bersama diubah, nol CSS global | ✅ source | Bagian 3.3 |

| Butir DoD | Keadaan |
| --- | --- |
| Acceptance criteria terpetakan ke source | ✅ |
| `npm run lint:errors` dijalankan | ✅ PASS |
| Unit test dijalankan | ✅ 866/866 |
| `npm run build` | ✅ **lulus 17 September 2026** — 0 error, 0 warning |
| Uji lewat layar | 🟡 **belum** |
| Laporan tracked | ✅ berkas ini |
| Roadmap dan traceability diperbarui | ✅ |
| Nol komponen bersama dan CSS global diubah | ✅ |
| UAT PASS | ❌ **tidak diklaim** — milik tim terpisah |

---

## 8. Catatan penutup

### 8.1 Temuan lama yang **tidak** diperbaiki, dan alasannya

Pada tab Transfer, kartu formulir "Koreksi kejadian" memakai prop `disabled={…}` — tetapi
`EmergencyAssessmentFormCard` **tidak punya prop bernama `disabled`**; yang dibacanya adalah
`canSubmit`. Artinya tombol **Simpan Koreksi** tidak pernah benar-benar dinonaktifkan meskipun
alasan koreksinya kosong atau waktunya di masa depan. Penjaga sesungguhnya ada di backend.

Cacat ini **sudah ada sebelum task ini** dan dipindahkan apa adanya, karena memperbaikinya
mengubah perilaku yang tidak diminta pemilik. Dicatat di sini supaya tidak hilang. Perbaikannya
satu kata (`disabled` → `canSubmit`) dan pantas menjadi butir backlog tersendiri.

### 8.2 Peringatan lint lama yang dibiarkan

`emergency-assessment-observation-tab.jsx:499` membawa peringatan
`react-hooks/set-state-in-effect` sejak `FE-IGD-028`. Ia **bukan** error, tidak menahan
`lint:errors`, dan berada di luar lingkup task ini.

### 8.3 Yang dinilai pemilik sesudah build

Letak baris "terakhir dikaji" berada **di atas** kartu formulir. Bila pemilik menghendakinya
menempel pada kepala kartu formulir, itu perubahan satu baris pada `work-panel.jsx`.
