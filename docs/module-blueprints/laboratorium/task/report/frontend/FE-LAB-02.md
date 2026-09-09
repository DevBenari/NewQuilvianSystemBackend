# Laporan Perubahan Frontend — `FE-LAB-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-02` |
| Judul | Layar batas nilai dan pengajuan batas kritis |
| Slice | `S3` — batas nilai dan batas kritis (`roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 3 |
| Trace | `FR-07.4`, `FR-03.1` .. `FR-03.5`; `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-023`; `LAB-FE-011`, `LAB-FE-013`, `LAB-FE-014`; `VAL-21` .. `VAL-35`; `03-frontend-architecture.md` bagian 3.4, 4, dan 5 |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02. Grup Lab Value Bound dan Lab Critical Bound Approval |
| Wewenang UI | `LAB-FE-011` **invariant keselamatan** — batas kritis tampil sebagai pengajuan, tanpa jalur simpan langsung. `LAB-FE-013` konvensi project — bentuk isian mengikuti bentuk hasil. `LAB-FE-014` konvensi project — layar data induk berada di `health-services/master-data/`. `LAB-FE-002` `DEV_DISCRETION` untuk tata letak dan pilihan komponen |
| Dependency | `FE-LAB-01` — **selesai** 2026-09-04. Endpoint dari `BE-LAB-04` dan `BE-LAB-05` — keduanya **selesai**, dan keberadaannya diverifikasi langsung pada source backend `3029af9` |
| Klasifikasi | `HEAVY` — skor 12: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan 1, UI/workflow 2. Angka logika bisnis dan UI/workflow-lah yang mengangkatnya: satu invariant keselamatan, sebelas aturan validasi, dan enam layar |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — `health-services/master-data/lab-value-bounds` pada tujuh lapisnya, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan satu berkas uji; serta `NewQuilvianSystemBackend` — **hanya** `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-02.md` beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `443270f3f` — *Merge remote-tracking branch origin/QuilvianIntegrationFrontend into YogaV2*, branch `YogaV2`, upstream `origin/YogaV2`. Pekerjaan `FE-LAB-01` sudah masuk lewat commit `427ff526d` |
| Commit backend yang dijadikan rujukan | `2dfc4f2`, branch `yoga`. Pemeriksaan controller dan DTO dilakukan pada `3029af9` lalu diperiksa ulang terhadap `2dfc4f2`: selisihnya hanya penambahan berkas Lab Catalog, dan tidak menyentuh grup Lab Value Bound maupun Lab Critical Bound Approval |
| Tanggal | 2026-09-04 |
| Status | **Selesai.** Enam layar berdiri, invariant `LAB-FE-011` ditegakkan dan dibuktikan uji, dan seluruh butir DoD terpenuhi. Tiga temuan yang berada **di luar** kendali task ini dicatat pada bagian 8 |

---

## 1. Keadaan yang ditemukan di awal

**Tidak ada satu pun layar batas nilai di frontend.** Pencarian `lab-value-bound` dan
`labValueBound` pada `src` tidak menghasilkan apa pun. Folder
`src/lib/constants/health-services/master-data/` berisi 28 menu data induk, dan batas nilai
laboratorium bukan salah satunya.

**Fondasinya sudah ada.** `FE-LAB-01` menyelesaikan kerangka modul Laboratorium beserta
kontrak penanganan state-nya. Task ini memakainya sebagai acuan bentuk, tetapi berkasnya
sendiri berada di `health-services/master-data/` — bukan di `laboratory-management` — karena
`LAB-FE-014` mengikat seluruh menu data induk mengikuti konvensi frontend yang sudah berjalan.

**Selisih penting antara dokumen kontrak dan source backend.**
`contracts/api-contract.md` r3 masih menandai enam endpoint Lab Value Bound dan lima endpoint
Lab Critical Bound Approval sebagai **`Rencana (belum tersedia)`**. Pemeriksaan langsung pada
backend `2dfc4f2` menunjukkan **seluruhnya sudah ada**:

| Yang tertulis di kontrak | Yang benar-benar ada di source |
| --- | --- |
| `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `PUT /{id}/deactivate`, `GET /{id}/history` — `Rencana` | Keenamnya ada pada `LabValueBoundController.cs` |
| `GET /`, `POST /`, `POST /{requestId}/approve`, `POST /{requestId}/reject`, `POST /{requestId}/withdraw` — `Rencana` | Kelimanya ada pada `LabCriticalBoundApprovalController.cs` |

Method dan path-nya **sama persis** dengan kontrak terkunci, sehingga tidak ada kontrak yang
dilanggar. Yang usang adalah kolom statusnya, dan itu dicatat sebagai temuan pembukuan pada
bagian 8. Laporan `BE-LAB-04.md` dan `BE-LAB-05.md` sendiri sudah menyatakan keduanya selesai.

**Satu perilaku backend yang menentukan bentuk layar.** `PUT /lab-value-bounds/{id}` menolak
`422` bila ruas batas kritis yang dikirim **berbeda** dari yang berlaku (`VAL-28`), bukan bila
ruas itu ada. Artinya menghilangkan ruas batas kritis dari permintaan justru membuat setiap
perubahan gagal — nilai kosong dibaca sebagai perubahan. Konsekuensinya bagi layar dijelaskan
pada bagian 3.3.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Kepala instalasi laboratorium mengelola batas nilai dan mengajukan
perubahan batas kritis. Pemegang kewenangan persetujuan batas kritis memutuskan pengajuan itu.
Petugas laboratorium melihat saja.

**Kenapa layar ini ada.** Batas nilai adalah yang membuat angka hasil laboratorium punya arti.
Tanpa batas normal, "Hemoglobin 9,1 g/dL" hanyalah angka. Dengan batas normal 13,2–17,3, angka
itu terbaca rendah. Dengan batas kritis 7, angka itu belum masuk kategori bahaya — dan justru
batas kritis inilah yang menentukan kapan pasien dinyatakan dalam bahaya, sehingga perubahannya
tidak boleh dilakukan sendirian.

### 2.1 Mengelola batas nilai

1. Pengguna membuka **Pelayanan Kesehatan → Master Data → Batas Nilai Pemeriksaan**.
2. Layar daftar menampilkan lima kartu rekap — total, aktif, bentuk angka, bentuk pilihan, dan
   berapa batas kritis yang sedang menunggu persetujuan — lalu tabelnya.
3. Penyaringnya: tanggal mulai, tanggal akhir, periode, jenis pemeriksaan, status, dan jumlah
   baris. Pencarian bebas mencari kode dan nama pemeriksaan.
4. Tombol **+ Tambah Batas Nilai** membuka formulir. Pengguna memilih jenis pemeriksaan dari
   master data Jenis Pemeriksaan yang sudah ada, lalu memilih bentuk hasilnya.
5. **Bentuk isian berubah mengikuti bentuk hasil.** Bila dipilih **Angka**, yang muncul adalah
   satuan, batas normal bawah dan atas, serta batas kritis bawah dan atas. Bila dipilih
   **Pilihan Terbatas**, keenam isian itu **hilang** dan digantikan daftar pilihan hasil yang
   dapat ditambah baris demi baris — misalnya Negatif, +1, +2, +3, +4 untuk protein urin.
6. Setelah tersimpan, pengguna diarahkan ke halaman detail.

### 2.2 Mengubah batas nilai — dan batas mana yang tidak dapat diubah di sana

1. Dari daftar, klik dua kali sebuah baris untuk membuka detail; atau tekan **Perbarui** pada
   kolom aksi.
2. Formulir ubah menampilkan satuan, batas normal, batas waktu cito, daftar pilihan, dan
   alasan perubahan.
3. **Batas kritis tidak punya isian sama sekali di sini.** Yang tampil adalah nilai yang
   berlaku di dalam bidang bergaris putus-putus berlabel *Terkunci — hanya berubah lewat
   pengajuan*, ditambah keterangan bahwa perubahannya hanya dapat ditempuh lewat pengajuan
   yang disetujui pihak klinis.
4. Hal yang sama berlaku pada daftar pilihan: penanda **kritis** setiap pilihan tampil sebagai
   penanda terkunci, bukan kotak centang. Pilihan baru yang ditambahkan pada mode ubah selalu
   berstatus bukan kritis.
5. Setelah disimpan, setiap kolom yang berubah menghasilkan satu baris riwayat tersendiri.

### 2.3 Mengajukan perubahan batas kritis

1. Dari halaman detail, tombol **Ajukan Perubahan Batas Kritis** membuka layar tersendiri.
2. Layar itu menampilkan batas kritis yang berlaku sebagai bidang terkunci, lalu formulir
   pengajuan: usulan batas kritis bawah dan atas untuk pemeriksaan berhasil angka, atau usulan
   kode pilihan kritis untuk pemeriksaan berhasil pilihan — ditambah **alasan pengajuan yang
   wajib diisi**.
3. Tombolnya berbunyi **Kirim Pengajuan**, bukan Simpan. Di bawahnya tertulis apa adanya:
   batas yang berlaku tidak berubah sampai pengajuan disetujui.
4. Daftar pengajuan di bawah formulir menampilkan status, batas yang berlaku, usulannya,
   alasan, dan catatan keputusan.
5. Pemegang kewenangan persetujuan menekan **Setujui** atau **Tolak**, dan wajib menuliskan
   catatan keputusan pada kotak konfirmasi. Pengaju sendiri melihat tombol **Tarik**, bukan
   tombol keputusan.

### 2.4 Menelusuri riwayat

Tombol **Riwayat Perubahan** pada halaman detail membuka layar baca saja berisi setiap
perubahan beserta kolom yang berubah, nilai lama, nilai baru, waktu, alasan, dan penanda
apakah perubahan itu melewati persetujuan. Penanda terakhir inilah yang membedakan perubahan
batas kritis dari perubahan biasa.

### 2.5 Jalur yang tidak normal

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Mencoba menyimpan batas nilai berbentuk angka tanpa satuan | Isian satuan ditandai merah sebelum permintaan dikirim; bila lolos, backend menjawab `VAL-22` dan pesannya ditampilkan apa adanya |
| Batas normal bawah lebih besar daripada batas atas | Ditolak di layar lebih dulu dengan kalimat yang sama seperti `VAL-25` |
| Bentuk pilihan tanpa satu pun pilihan | Kotak merah pada panel daftar pilihan: pemeriksaan berhasil pilihan wajib punya sekurang-kurangnya satu pilihan (`VAL-23`) |
| Membuat baris keempat dengan kombinasi pemeriksaan, jenis kelamin, dan umur yang sama | Backend menjawab `409` beserta pesan `VAL-21`; pesannya muncul sebagai notifikasi merah |
| Menonaktifkan batas aktif terakhir milik sebuah pemeriksaan | Backend menjawab `422` beserta pesan `VAL-30` yang menjelaskan akibatnya: hasil tidak dapat dinilai |
| Mengajukan perubahan kedua saat yang pertama belum diputuskan | Formulir pengajuan **tidak dirender sama sekali**; yang muncul peringatan kuning bahwa masih ada pengajuan berjalan (`VAL-32`) |
| Pengaju membuka daftar pengajuannya sendiri | Tombol Setujui dan Tolak **tidak ditampilkan**; yang ada hanya Tarik (`VAL-33`, `VAL-35`) |
| Tautan detail dibuka pada sesi baru | Token route sudah tidak ada, sehingga muncul ajakan membuka ulang dari daftar data — bukan pesan teknis |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan aturan.** `AGENTS.md` frontend; `CLAUDE.md` frontend;
`rules/frontend/frontend-architecture.md`; `rules/frontend/master-data-feature-standard.md`;
`rules/frontend/base-component-catalog.md`;
`rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`;
`rules/frontend/page-composition-patterns.md`; `rules/frontend/ui-consistency-checklist.md`;
`rules/frontend/test-policy.md`; `rules/frontend/REPORT_TEMPLATE.md`;
`rules/backend/TASK_CLASSIFICATION.md`.

**Blueprint dan kontrak.** `roadmap/frontend-roadmap.md` bagian 3;
`03-frontend-architecture.md` bagian 3.4, 4, 5; `contracts/api-contract.md` bagian 2;
`contracts/validation-matrix.md` `VAL-21` .. `VAL-35`;
`testing/acceptance-test-matrix.md` `AC-24`, `AC-28`, `AC-33`, `AC-34`;
`task/report/backend/BE-LAB-04.md`; `task/report/backend/BE-LAB-05.md`.

**Backend sebagai sumber kebenaran kontrak — strict read-only.**
`Areas/HealthServices/LaboratoryManagement/Controllers/LabValueBoundController.cs`;
`.../Controllers/LabCriticalBoundApprovalController.cs`;
`.../DTOs/LabValueBoundDtos.cs`; `.../DTOs/LabCriticalBoundApprovalDtos.cs`;
`.../DTOs/LabFilterAndSummaryDtos.cs`; `.../Services/LabValueBoundService.cs`
(khususnya `UpdateAsync` dan `EnsureNoCriticalBoundChange`);
`.../Services/LabFilterMetadataFactory.cs`; `.../Enums/LaboratoryEnums.cs`;
`Responses/ApiResponse.cs`.

**Frontend sebagai acuan pola.** Modul rujukan otoritatif `hr/master-data/job-level` —
constants, slice, utils, ketiga hook, ketiga view, dan kelima route-nya. Ditambah
`components/features/base-features/` (data-table, data-filter, base-editor-view,
base-detail-view, base-form-control, confirm-modal, resource-filter-select, status-badge,
information-alert, toast-stack), `lib/hooks/select/select-resource-registry.js`,
`lib/hooks/select/health-service/health-service-select-resources.js`,
`lib/state/slice/health-services/master-data/master-data-age-category-slice.jsx`,
`components/view/health-services/billing-management/master-data/register/register-view.jsx`
sebagai preseden aksi status pada halaman daftar, `utils/security/private-route-token-utils`,
dan `src/app/globals.css` untuk token desain.

### 3.2 Berkas yang berubah

**Tujuh berkas inti standar master data:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/lab-value-bounds/lab-value-bound-constants.jsx` | `LAB_VALUE_BOUND_CONFIG` — endpoint, route, salinan teks, penyaring, kolom, field form, baris detail, dan pesan gagal. Ditambah `LAB_CRITICAL_BOUND` yang mengumpulkan seluruh kalimat tentang batas kritis di satu tempat |
| `src/lib/state/slice/health-services/master-data/master-data-lab-value-bound-slice.jsx` | Tiga belas thunk: delapan untuk batas nilai, lima untuk pengajuan perubahan batas kritis. Berikut helper sanitasi, normalisasi, dan pengenalan pembatalan permintaan |
| `src/utils/health-services/master-data/lab-value-bounds/lab-value-bound-utils.jsx` | Fungsi murni: pembacaan payload, pembentukan form, validasi, pembentukan payload buat dan ubah, baris detail, serta baris pilihan hasil |
| `src/lib/hooks/.../use-master-data-lab-value-bound.jsx` | Controller halaman daftar, termasuk aksi menonaktifkan beserta konfirmasinya |
| `src/lib/hooks/.../use-master-data-lab-value-bound-detail.jsx` | Controller halaman detail dan jalan menuju layar riwayat serta layar pengajuan |
| `src/lib/hooks/.../use-master-data-lab-value-bound-editor.jsx` | Controller formulir buat dan ubah, termasuk baris pilihan hasil dan penguncian batas kritis |
| `src/components/view/health-services/master-data/lab-value-bounds/` | `master-data-lab-value-bound-view.jsx`, `detail/lab-value-bound-detail-view.jsx`, `add/lab-value-bound-form-view.jsx` |

**Lima berkas tambahan yang dituntut bentuk fitur ini:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../use-lab-value-bound-history.jsx` | Controller layar riwayat perubahan |
| `src/lib/hooks/.../use-lab-critical-bound-request.jsx` | Controller layar pengajuan batas kritis, termasuk penegakan tampilan `VAL-33` dan `VAL-35` |
| `src/components/view/.../history/lab-value-bound-history-view.jsx` | Layar riwayat, baca saja |
| `src/components/view/.../critical-bound/lab-critical-bound-request-view.jsx` | Layar pengajuan batas kritis — satu-satunya jalur perubahan batas kritis |
| `src/components/view/.../add/lab-value-option-rows-editor.jsx` | Penyunting baris pilihan hasil, dirangkai dari base component yang sudah ada |

**Delapan berkas route:**

`src/app/health-services/master-data/lab-value-bounds/` — `page.jsx`,
`lab-value-bounds-client.jsx`, `create/page.jsx`, `[slug]/route-token.js`, `[slug]/page.jsx`,
`[slug]/update/page.jsx`, `[slug]/history/page.jsx`, dan `[slug]/critical-bound/page.jsx`.
`route-token.js` memuat penjaga token route yang dipakai bersama keempat route bersegmen
`[slug]`, supaya penjaganya tidak disalin empat kali.

**Satu berkas style dan satu berkas uji:**

| Berkas | Perubahan |
| --- | --- |
| `src/style/health-services/master-data/lab-value-bounds/lab-value-bound.module.css` | Kolom aksi, panel, penanda terkunci, dan baris pilihan. Seluruh nilai memakai token; tidak ada satu pun warna literal |
| `tests/unit/lab-value-bound-utils.test.mjs` | Delapan uji terhadap fungsi murni, termasuk dua uji yang menjaga invariant `LAB-FE-011` |

**Dua berkas yang disunting:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Satu baris import dan satu baris pendaftaran reducer dengan kunci `masterDataLabValueBound` |
| `src/utils/menu-sidebar/menu-items.jsx` | Satu butir menu **Batas Nilai Pemeriksaan** di dalam grup Master Data Pelayanan Kesehatan, tepat setelah Prosedur. Ikon `RiFlaskLine` yang sudah diimpor dipakai ulang |

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensi tidak dibalik.** `src/app` hanya entry point dan metadata; view merangkai;
hook mengendalikan; slice memanggil service HTTP lewat `InstanceAxios`; utils murni. Tidak ada
view yang memanggil Axios, dan tidak ada instance Axios baru.

**Bentuknya mengikuti standar fitur master data, dengan tiga selisih yang disengaja.**

| Selisih | Alasan |
| --- | --- |
| **Delapan thunk, bukan sembilan** | Batas nilai tidak punya `GET /options` maupun `DELETE /{id}`. Backend menyatakannya terbuka lewat `IsDeletable: false`. Yang ada sebagai gantinya `PUT /{id}/deactivate` dan `GET /{id}/history` |
| **Aksi menonaktifkan berada di halaman daftar** | `AGENTS.md` frontend melarang aksi Aktifkan/Nonaktifkan pada halaman detail master data, dan mengizinkannya pada halaman daftar ketika polanya sudah ada. Polanya memang sudah ada pada master data Register Kasir. Halaman detail karena itu hanya memuat Kembali, Riwayat Perubahan, dan Perbarui — tanpa Hapus, karena memang tidak ada jalur hapus |
| **Satu berkas CSS Module ditambahkan** | Standar menyatakan fitur master data yang konform tidak menambah CSS Module. Fitur ini menambahkannya karena bentuknya memang berbeda: ada kolom aksi, ada baris pilihan yang disunting, dan ada penanda terkunci yang wajib terlihat. Alternatifnya inline style, yang dilarang checklist konsistensi UI |

**Dua master data lain dipakai ulang, tidak disalin.** Jenis pemeriksaan diambil dari registry
select bersama (`procedures`, yang menunjuk `master-data/procedures/options`), dan kelompok
umur diambil lewat thunk `getAgeCategoryOptions` milik master data Kategori Umur. Modul
Laboratorium tidak membuat daftar pemeriksaan maupun daftar kelompok umurnya sendiri.

**Bagaimana `LAB-FE-011` ditegakkan — tiga lapis, bukan satu.**

1. **Ruas isian tidak ada.** `getVisibleFields("update", form)` membuang `criticalLow` dan
   `criticalHigh` dari formulir ubah. Bukan disembunyikan lewat CSS, melainkan tidak dirender.
2. **Payload tidak pernah membaca isian.** `buildUpdatePayload` menyalin batas kritis dari
   data yang berlaku. Ini juga yang membuat perubahan ruas lain tetap lolos: `VAL-28` menolak
   `422` bila nilainya **berbeda**, sehingga mengosongkan ruas itu justru menggagalkan setiap
   penyimpanan.
3. **Penanda kritis pilihan ikut dijaga.** Pada mode ubah, penanda kritis setiap pilihan
   ditampilkan sebagai penanda terkunci, dan payload menyalinnya dari data yang berlaku.
   Pilihan baru selalu bukan kritis, karena menandai pilihan baru sebagai kritis juga
   merupakan perubahan batas kritis menurut `EnsureNoCriticalBoundChange`.

Ketiganya dijaga uji unit `S3`, `S4`, dan `S5`.

**Batas kritis tetap dapat diisi saat membuat batas nilai baru.** Ini bukan kelonggaran:
`AC-28` justru menuntut pembuatan batas berbentuk pilihan dengan `P3` dan `P4` **bertanda
kritis**, dan backend memang menerimanya pada `POST`. `LAB-FE-011` berbicara tentang
**perubahan** batas kritis, dan di sanalah jalur simpan langsung ditiadakan.

**Gerbang keputusan base component.**

```text
UI GATE: 11 elemen — REUSE 8, EXTEND 0, COMPOSE 2, WRAP 1, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `.../hero.jsx` | `REUSE` | Dipakai pada keempat layar |
| Kartu rekap | `SummaryGrid` | `.../summary-grid.jsx` | `REUSE` | Lima kartu dari `GET /summary` |
| Penyaring dan pencarian | `DataFilter`, `FilterDatePicker`, `FilterSelect` | `.../data-filter.jsx` | `REUSE` | Urutan baku: tanggal, periode, domain, status, jumlah baris |
| Penyaring jenis pemeriksaan | `ResourceFilterSelect` | `.../resource-filter-select.jsx` | `REUSE` | Membaca registry `procedures` |
| Tabel daftar, pilihan, riwayat, pengajuan | `DataTable` | `.../data-table.jsx` | `REUSE` | Empat pemakaian dengan kolom berbeda |
| Halaman detail | `BaseDetailView` | `.../base-detail-view.jsx` | `REUSE` | `renderDetailActions` dan `afterDetail` |
| Halaman buat dan ubah | `BaseEditorView` | `.../base-editor-view.jsx` | `REUSE` | `afterForm` menampung panel batas kritis dan daftar pilihan |
| Konfirmasi keputusan | `ConfirmModal` | `.../confirm-modal.jsx` | `REUSE` | `requireReason` untuk catatan keputusan |
| Panel batas kritis terkunci | `InformationAlert`, `StatusBadge`, `BaseButton` | `.../information-alert.jsx` | `COMPOSE` | Dirangkai di view detail dan view formulir |
| Panel pengajuan | `BaseTextField`, `BaseTextAreaField`, `BaseButton` | `.../base-form-control.jsx` | `COMPOSE` | Formulir pengajuan dirangkai dari field siap pakai |
| Penyunting baris pilihan hasil | `BaseTextField`, `BaseCheckboxField`, `BaseButton` | `.../base-form-control.jsx` | `WRAP` | Pembungkus tipis khusus domain di lapis view |

**Keputusan untuk ketiga baris yang bukan `REUSE`:**

> **Keputusan: penyunting daftar pilihan hasil**
>
> - **A. Rangkai dari `BaseTextField`, `BaseCheckboxField`, dan `BaseButton` menjadi satu
>   komponen khusus domain di lapis view — Rekomendasi.** Tidak ada base component yang
>   berubah, sehingga modul lain tidak terdampak. Kontrak visual field tetap milik base
>   component, dan yang ditambahkan hanya penataan barisnya. Preseden bentuk seperti ini sudah
>   ada pada penyunting resep Farmasi.
> - **B. Pakai `BaseGroupedEditorView` dan perlakukan setiap pilihan sebagai satu grup.**
>   Tanpa komponen baru sama sekali, tetapi jumlah pilihan berubah-ubah sementara `groups`
>   bersifat statis, sehingga menambah dan menghapus pilihan tidak terwakili.
> - **C. Tambah komponen base baru untuk baris berulang.** Paling berguna bagi modul lain,
>   tetapi berstatus `NEW` dan menuntut persetujuan tersendiri, sementara kebutuhannya baru
>   muncul di satu fitur.
>
> Opsi **A** yang dijalankan. Kedua baris `COMPOSE` lainnya mengikuti urutan preferensi dan
> tidak mengubah satu pun base component.

**Token desain.** Berkas style barunya tidak memuat satu pun nilai warna, radius, bayangan,
atau jarak sebagai literal; seluruhnya `var(...)`. Tidak ada `!important`, tidak ada inline
style untuk nilai statis, dan tidak ada selector yang menyasar typography komponen bersama.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu rekap menjadi kerangka abu-abu; tabel menampilkan "Mengambil data batas nilai..."; tombol tambah, perbarui, nonaktifkan, dan atur ulang terkunci selama proses |
| Kosong | "Data batas nilai tidak ditemukan." disertai ajakan mengganti penyaring atau menambah data. Pada layar pengajuan: "Belum ada pengajuan perubahan batas kritis." Pada layar riwayat: "Belum ada perubahan yang tercatat." |
| Gagal | Pesan dari server ditampilkan apa adanya — sebagai kotak merah pada halaman daftar, dan sebagai notifikasi merah untuk kegagalan aksi. Tidak ada kalimat buatan layar yang menggantikan pesan server |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" beserta arahan menghubungi IT Helpdesk, lewat `AccessDeniedGate` |
| Kirim ganda | Setiap tombol yang memanggil API terkunci sejak ditekan sampai jawaban datang, dan permintaan lama dibatalkan ketika penyaring berubah |
| Tanpa kewenangan pada baris tertentu | Tombol Setujui dan Tolak **tidak dirender** bagi pengaju; tombol Tarik tidak dirender bagi selain pengaju |
| Pengajuan sedang berjalan | Formulir pengajuan tidak dirender sama sekali; digantikan peringatan kuning |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Value Bound

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds/filters/metadata` | Pilihan bentuk hasil, jenis kelamin, dan ukuran halaman pada layar daftar dan formulir | `LabValueBound : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds/summary` | Lima kartu rekap pada layar daftar | `LabValueBound : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds` | Tabel daftar batas nilai beserta penyaring dan paginasinya | `LabValueBound : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}` | Halaman detail, formulir ubah, layar riwayat, dan layar pengajuan | `LabValueBound : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-value-bounds` | Menyimpan batas nilai baru beserta daftar pilihannya | `LabValueBound : Create` |
| `PUT` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}` | Mengubah satuan, batas normal, batas waktu cito, dan daftar pilihan — **tanpa** menyentuh batas kritis | `LabValueBound : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/deactivate` | Tombol Nonaktifkan pada kolom aksi halaman daftar | `LabValueBound : Update` |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/history` | Layar riwayat perubahan | `LabValueBound : Read` |

#### Health Services / Laboratory Management / Lab Critical Bound Approval

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests` | Daftar pengajuan pada layar pengajuan | `LabCriticalBound : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests` | Tombol Kirim Pengajuan | `LabValueBound : Update` |
| `POST` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests/{requestId}/approve` | Tombol Setujui | `LabCriticalBound : Approve` |
| `POST` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests/{requestId}/reject` | Tombol Tolak | `LabCriticalBound : Approve` |
| `POST` | `/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests/{requestId}/withdraw` | Tombol Tarik | `LabValueBound : Update` |

Dua endpoint yang **tidak** dikonsumsi task ini: `GET /critical-change-requests/filters/metadata`
dan `GET /critical-change-requests/summary`. Keduanya tersedia, tetapi keterangan yang
dibawanya — larangan menyetujui pengajuan sendiri dan batas satu pengajuan berjalan — sudah
ditegakkan layar memakai data yang ada, sehingga memanggilnya hanya menambah permintaan tanpa
menambah perilaku.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Kode keluar `0` |
| `npx eslint` pada seluruh berkas baru, termasuk peringatan | 0 error, 5 peringatan `react-hooks/set-state-in-effect` | `EXISTING WARNING` | Peringatan yang sama muncul pada modul rujukan `hr/master-data/job-level` (2 peringatan sejenis). Penyebabnya pola resolusi token route di dalam efek, yang memang bentuk baku fitur master data. Aturan ini sengaja diturunkan menjadi peringatan pada `eslint.config.mjs` selama migrasi React 18 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 448 uji, 448 lulus, 0 gagal | `PASS` | Seluruh suite dijalankan, termasuk delapan uji baru |
| Uji `S2` — bentuk isian mengikuti bentuk hasil | Lulus | `PASS` | `getVisibleFields` menampilkan satuan dan batas pada bentuk angka, dan **tidak** menampilkannya pada bentuk pilihan (`LAB-FE-013`) |
| Uji `S3` — tidak ada isian batas kritis pada formulir ubah | Lulus | `PASS` | `getVisibleFields("update")` tidak pernah memuat `criticalLow` maupun `criticalHigh` (`LAB-FE-011`) |
| Uji `S4` — payload ubah menyalin batas kritis yang berlaku | Lulus | `PASS` | Isian `criticalLow: "99"` diabaikan; payload berisi `2.8` dari data yang berlaku (`VAL-28`) |
| Uji `S5` — penanda kritis pilihan disalin, pilihan baru tidak kritis | Lulus | `PASS` | `P3` tetap kritis, `NEG` tetap bukan, dan pilihan baru `P4` dipaksa bukan kritis |
| Uji `S6` — bentuk angka tidak mengirim daftar pilihan | Lulus | `PASS` | `VAL-24` dijaga sejak di layar |
| Uji `S7` — urutan batas diperiksa di layar | Lulus | `PASS` | `VAL-25` sampai `VAL-27` ditolak sebelum permintaan dikirim |
| Uji `S8` — pengajuan tanpa alasan ditolak | Lulus | `PASS` | `VAL-31` dijaga sejak di layar |
| `npm run build` | Selesai, termasuk `postbuild` standalone | `PASS` | Kode keluar `0`. Keenam route terbit: `/lab-value-bounds`, `/create`, `/[slug]`, `/[slug]/update`, `/[slug]/history`, `/[slug]/critical-bound` |
| Grep anti-regresi warna literal pada style baru | Tidak ada temuan | `PASS` | Seluruh nilai memakai token `var(...)` |
| Grep anti-regresi tombol non-base dan tabel mentah | Tidak ada temuan | `PASS` | Seluruh tombol `BaseButton`; seluruh tabel `DataTable` |
| Grep anti-regresi `!important` dan inline style statis | Tidak ada temuan | `PASS` | Kolom aksi memakai class module, bukan inline style seperti preseden Register Kasir |

**Uji manual: `NOT FEASIBLE`.**

Alasannya sama seperti pada `FE-LAB-01`, dan tetap konkret:

1. Seluruh perilaku yang perlu dilihat — penolakan `VAL-21` sampai `VAL-35`, alur persetujuan,
   dan penguncian pengajuan kedua — **hanya dapat dimunculkan oleh jawaban server yang
   sebenarnya**. Pada lingkungan sesi ini tidak ada backend Laboratorium yang berjalan dan
   tidak ada sesi login yang sah.
2. Alur persetujuan menuntut **dua akun berbeda** — pengaju dan penyetuju — karena `VAL-33`
   melarang pengaju memutuskan pengajuannya sendiri. Satu sesi tidak cukup.
3. Peran pemegang `LabCriticalBound : Approve` sendiri **belum ditetapkan** manajemen rumah
   sakit; `BE-LAB-05` mencatatnya sebagai hal terbuka. Tanpa pemegang peran itu, jalur
   persetujuan belum dapat dijalankan ujung-ke-ujung oleh siapa pun.

Penggantinya bukan asumsi. Delapan uji unit di atas membuktikan aturan yang paling menentukan
secara deterministik — termasuk kedua uji yang menjaga invariant keselamatan — dan keluaran
build membuktikan keenam layarnya benar-benar terbit sebagai route yang dapat dibuka.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | Tidak diminta task, dan lingkungan sesi ini tidak menyediakan backend maupun dua sesi login yang dibutuhkan skenario persetujuan |
| `npm run test:uat` | Hanya dijalankan bila diminta secara eksplisit; tidak diminta |
| `npm run dev` | `AGENTS.md` melarang menjalankan development server tanpa kebutuhan konkret; keluaran build sudah membuktikan keenam route terbit |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-24` — tiga baris batas untuk satu pemeriksaan dengan kelompok pasien berbeda; baris keempat yang sama ditolak `409` | **Terpenuhi di sisi layar** | Formulir mengirim `procedureId`, `genderScope`, dan `ageCategoryId` sebagai satu kesatuan, dan penolakan `409` beserta pesan `VAL-21` ditampilkan apa adanya sebagai notifikasi. Pembuktian penyimpanan ketiganya adalah cakupan `BE-LAB-04` |
| `AC-28` — batas berbentuk pilihan dengan lima pilihan, `P3` dan `P4` bertanda kritis; jalur gagal `VAL-22`, `VAL-23`, `VAL-24` | **Terpenuhi** | Penyunting baris pilihan mengirim `isCritical` pada mode buat (uji `S6`); satuan wajib pada bentuk angka dan daftar pilihan wajib pada bentuk pilihan diperiksa sejak di layar; bentuk angka tidak pernah mengirim daftar pilihan |
| `AC-33` — perubahan batas normal langsung berlaku; perubahan batas kritis hanya lewat pengajuan; jalur gagal `VAL-28`, `VAL-32`, `VAL-33` | **Terpenuhi** | Uji `S3` dan `S4` membuktikan formulir ubah tidak punya jalur batas kritis sama sekali; layar pengajuan menyembunyikan formulir saat ada pengajuan berjalan (`VAL-32`) dan menyembunyikan tombol keputusan bagi pengaju (`VAL-33`) |
| `AC-34` — riwayat memuat kolom, nilai lama, nilai baru, pelaku, waktu, dan alasan | **Terpenuhi sebagian** | Layar riwayat menampilkan kolom, nilai lama, nilai baru, waktu, alasan, dan penanda apakah perubahan melewati persetujuan. **Kolom pelaku belum dapat ditampilkan** karena backend hanya mengirim penunjuk penggunanya tanpa nama — lihat bagian 8 |

**Definition of Done.**

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Layar ada di `master-data/lab-value-bounds/` | **Terpenuhi** | Keenam route terbit pada keluaran build, seluruhnya di bawah `health-services/master-data/lab-value-bounds` sesuai `LAB-FE-014` |
| Bentuk isian mengikuti bentuk hasil | **Terpenuhi** | Uji `S2`; isian satuan dan keempat batas hanya muncul pada bentuk angka, daftar pilihan hanya muncul pada bentuk pilihan |
| Jalur pengajuan terpisah dan terlihat | **Terpenuhi** | Route tersendiri `[slug]/critical-bound`, dengan tombol masuknya pada halaman detail dan keterangannya pada formulir ubah |
| Riwayat dapat dibuka | **Terpenuhi** | Route `[slug]/history` beserta tombol Riwayat Perubahan pada halaman detail |
| Tidak ada jalur simpan langsung untuk batas kritis | **Terpenuhi** | Tiga lapis penjagaan pada bagian 3.3, dua di antaranya dijaga uji unit `S3` dan `S4` |

Satu butir acceptance criteria — kolom pelaku pada `AC-34` — belum terpenuhi penuh, dan
penyebabnya berada di luar cakupan task frontend. Disebut apa adanya di bawah.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Lima peringatan `react-hooks/set-state-in-effect` pada kelima hook, seluruhnya dari pola resolusi token route yang memang bentuk baku fitur master data. Modul rujukan `job-level` memunculkan peringatan yang sama. Diklasifikasikan `EXISTING WARNING` dan tidak diperbaiki karena memperbaikinya berarti menyimpang dari pola rujukan |
| Masalah yang diketahui — 1 | **Riwayat tidak dapat menampilkan pelaku.** `LabValueBoundHistoryResponse` hanya membawa `actorUserId` dan `approvedByUserId` berbentuk penunjuk, tanpa nama. Penunjuk seperti itu tidak boleh tampil di layar, sehingga kolom pelaku tidak dirender. Agar `AC-34` terpenuhi penuh, backend perlu menambahkan nama pelaku dan nama penyetuju pada respons riwayat — sejajar dengan `createByName` yang sudah lazim di modul lain. **Perbaikannya milik backend, bukan task ini** |
| Masalah yang diketahui — 2 | **Status kontrak usang.** `contracts/api-contract.md` r3 masih menandai sebelas endpoint sebagai `Rencana (belum tersedia)` padahal seluruhnya sudah ada sejak `BE-LAB-04` dan `BE-LAB-05`. Method dan path-nya cocok, jadi tidak ada kontrak yang dilanggar — yang perlu diperbarui hanya kolom statusnya. Pembaruan dokumen kontrak berada di luar wewenang tulis task ini |
| Masalah yang diketahui — 3 | **Kewenangan menyetujui belum berpemilik.** Peran pemegang `LabCriticalBound : Approve` belum ditetapkan manajemen rumah sakit, sebagaimana dicatat `BE-LAB-05` dan `roadmap/traceability.md` bagian 4.3. Selama belum ada pemegangnya, layar pengajuan dapat dipakai mengirim pengajuan tetapi tidak ada akun yang dapat menyetujuinya, sehingga batas kritis tetap tidak dapat berubah lewat aplikasi |
| Dependency backend | `NONE` yang menahan. Seluruh endpoint yang dikonsumsi sudah ada dan diverifikasi langsung pada source backend `2dfc4f2` |
| Perubahan sampingan | `NONE`. Tidak ada berkas di luar cakupan yang disentuh. Repository backend memiliki perubahan belum di-commit milik pekerjaan lain sejak sebelum task ini; tidak satu pun berasal dari task ini |
| Interupsi | `NONE` untuk pekerjaan itu sendiri. Selama task berjalan, `FE-LAB-01` di-commit pemilik sebagai `427ff526d` lalu di-merge menjadi `443270f3f`; berkas task ini diperiksa ulang setelahnya dan seluruhnya utuh |
| Status Git | Lihat blok di bawah tabel ini |
| Langkah berikutnya | Kerjakan `FE-LAB-03` — layar alasan penolakan sampel di `master-data/lab-rejection-reasons/`. Bentuknya mengikuti fitur ini, dan invariant yang dijaga di sana `LAB-FE-012`: kolom kesalahan internal dan kolom wajib catatan harus **terlihat terkunci** bagi kepala instalasi, bukan sekadar gagal saat disimpan. Endpoint pasangannya `BE-LAB-06`, yang sudah selesai |

```text
 M src/lib/state/store.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/master-data/lab-value-bounds/
?? src/components/view/health-services/master-data/lab-value-bounds/
?? src/lib/constants/health-services/master-data/lab-value-bounds/
?? src/lib/hooks/health-services/master-data/lab-value-bounds/
?? src/lib/state/slice/health-services/master-data/master-data-lab-value-bound-slice.jsx
?? src/style/health-services/master-data/lab-value-bounds/
?? src/utils/health-services/master-data/lab-value-bounds/
?? tests/unit/lab-value-bound-utils.test.mjs
```

Tidak ada `git add`, commit, push, merge, rebase, maupun perpindahan branch yang dilakukan pada
kedua repository.
