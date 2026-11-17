# Laporan Perubahan Frontend — `FE-AST-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-003` |
| Judul | Pencatatan draft dan persetujuan nomor |
| Slice | `REG-FE-S1` — Fondasi dan pencatatan |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-003` |
| Trace | `CAP-03.1..03.3`, `AST-DEC-043`; `SCR-AST-REG-10`, `12`; `03-frontend-architecture.md` §3.2, §3.3, §6 |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-007` (`55610f4a`) ditambah tambalan daftar 2026-11-17 (bagian 1) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-002` ✅, `BE-AST-007` ✅ |
| Klasifikasi | `HEAVY` — 3 layar (daftar draft, formulir buat/ubah, antrean persetujuan), 14 berkas baru, 2 berkas diubah |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-17 ("commit sekarang lalu mulai FE-AST-003"); tambalan backend sempit atas pilihan Rizki pada hari yang sama |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); backend hanya tambalan daftar yang disetujui (bagian 1); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `7e3a5968c` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `55610f4a` (branch `rizkiG`) + tambalan belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — kelima kriteria terpetakan ke source, uji layar 24 langkah lulus dengan tiga aktor, ESLint `0 errors, 0 warnings`, dan build pemilik (frontend dan backend) berhasil |

---

## 1. Keadaan yang ditemukan di awal

- Backend `BE-AST-007` menyediakan 13 endpoint pencatatan dan persetujuan, tetapi daftar aset tidak membawa
  kolom yang dibutuhkan skema layar:
  - §3.2 meminta "draft saya" lewat `createdByMe=true`, padahal penyaring itu tidak ada;
  - §3.2 meminta draft yang ditolak ditandai beserta alasannya, padahal daftar tidak membawa `RejectedAt` dan `DecisionReason`;
  - kriteria 4 ("baris milik pengaju tidak dapat dicentang") butuh pengaju per baris, padahal daftar tidak membawa `SubmittedBy`.
- `FRONTEND MODE` melarang mengubah backend. Pilihan diajukan ke Rizki; ia memilih **tambal backend dulu**.
  Tambalan: `CreatedByMe` pada `AssetPagedQuery`, serta `CreateBy`, `SubmittedBy`, `RejectedAt`, `DecisionReason`
  pada `AssetListItemResponse` (+15 baris, QBE Strict `PASS`, nol komentar). Dicatat sebagai delta di laporan
  `BE-AST-007`.
- `GET /master-data/groups/options` tidak membawa `isMedical`. Daftar kelompok (`GET /master-data/groups`)
  membawanya, jadi pilihan kelompok memakai daftar itu. Backend tidak perlu diubah.
- Grup menu "Registrasi & Inventaris" belum ada.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Petugas Aset | Pencatatan Aset Baru | Menekan **+ Catat Aset Baru**, mengisi kelompok, kepemilikan, jumlah 1–200, data perolehan, harga per unit |
| 2 | Petugas Aset | Formulir | Bila kelompok medis, bagian **Atribut Alat Medis** muncul. Contoh: memilih Alat Kedokteran Umum menampilkan merek, tipe, nomor seri, produsen, izin edar; mengganti ke Mebel menyembunyikannya |
| 3 | Petugas Aset | Formulir | **Simpan Draft** (boleh belum lengkap) atau **Simpan & Ajukan** (diperiksa lengkap dulu di layar, lalu backend) |
| 4 | Petugas Aset | Daftar draft | Ubah, Ajukan, atau Batalkan (alasan wajib). Draft yang dikembalikan bertanda **Dikembalikan** dengan alasannya di kolom Catatan |
| 5 | Kepala Bagian Aset | Nomor Inventaris | Mencentang aset, **Setujui Terpilih (n)** → modal "x disetujui, y gagal" beserta nomor yang terbit dan sebab kegagalan; atau Setujui / Kembalikan ke draft per baris |
| 6 | Siapa pun | Nomor Inventaris | Baris yang ia ajukan sendiri bertanda **Anda**; kotak centang dan menu Setujui/Kembalikan nonaktif |

Aturan yang sengaja dijaga di layar:

- Alat medis lebih dari satu unit tidak dapat langsung diajukan dari formulir (§3.2): tombol **Simpan & Ajukan**
  disembunyikan dan muncul penjelasan, karena tiap unit perlu nomor seri sendiri.
- Ubah draft mengisi harga dari `GET /{id}/amounts`, karena rupiah tidak ada di rincian (`AST-DES-016`).
- `rowVersion` rincian dikirim pada `PUT`; bila backend menjawab 409 `015`, layar memuat ulang draft.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; referensi skill (katalog base component,
gerbang keputusan, checklist UI); layar `FE-AST-002` (list/form view, hook, kolom, route token); base
`data-table`, `data-filter`, `row-action-menu`, `confirm-modal`, `base-grouped-editor-{view,form,field}`,
`filter-date-picker`, `filter-select`; `use-select-resource`, registry pilihan Finance/Aset;
`input-normalizer-utils` (rupiah); `login-slice` (`selectUserInfo`); kontrak 2.8, `03-frontend-architecture.md`
§3.2–3.3, `02-module-map.md` §3.1; DTO dan service `BE-AST-007`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/register/asset-record-constants.jsx` | Baru — endpoint, route, tab, kartu ringkasan, isian dan grup formulir, pemetaan kode galat → isian |
| `src/utils/corporate/asset-management/register/asset-record-utils.js` | Baru — baca respons, galat service, parameter daftar, isian tampil/sembunyi, form awal, payload, validasi layar, galat per isian, ringkasan hasil massal |
| `src/lib/services/corporate/asset-management/asset-record-service.js` | Baru — 10 fungsi API lewat `InstanceAxios` |
| `src/lib/hooks/corporate/asset-management/register/use-asset-draft-list.jsx` | Baru — daftar "buatan saya", tab status, ringkasan, ajukan, batal |
| `src/lib/hooks/corporate/asset-management/register/use-asset-draft-editor.jsx` | Baru — buat/ubah, atribut medis bersyarat, simpan vs simpan-dan-ajukan |
| `src/lib/hooks/corporate/asset-management/register/use-asset-approval-list.jsx` | Baru — antrean `Diajukan`, centang, setujui satuan/massal, tolak |
| `src/components/view/corporate/asset-management/register/asset-record-table-columns.jsx` | Baru — kolom kedua tabel, aksi baris, kolom centang |
| `src/components/view/corporate/asset-management/register/asset-draft-list-view.jsx` | Baru — `SCR-AST-REG-10` daftar |
| `src/components/view/corporate/asset-management/register/asset-draft-form-view.jsx` | Baru — `SCR-AST-REG-10` formulir |
| `src/components/view/corporate/asset-management/register/asset-approval-list-view.jsx` | Baru — `SCR-AST-REG-12` |
| `src/app/corporate/asset-management/register/drafts/page.jsx`, `drafts/create/page.jsx`, `drafts/[slug]/update/page.jsx`, `approvals/page.jsx` | Baru — route tipis |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | Diubah — pilihan `assetOwnershipTypes` dan `assetGroupsDetailed` (daftar kelompok yang membawa `isMedical`) |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — grup "Registrasi & Inventaris": Pencatatan Aset Baru, Nomor Inventaris (`AssetRegister : Read`) |

Backend (tambalan yang disetujui; rincian di laporan `BE-AST-007`): `Register/DTOs/AssetRecordDtos.cs`,
`Register/Services/AssetRecordService.Query.cs`.

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. View tidak memanggil Axios.
- **Service, bukan Redux.** Data layar tidak dibagi lintas halaman, jadi dipakai `lib/services`, sesuai aturan
  "service untuk API yang tidak memerlukan Redux global". Tidak ada slice atau perubahan store.
- Pola "respons berkunci" pada hook: status memuat dihitung dari kunci permintaan, dan `setState` hanya
  dipanggil di callback (aturan `react-hooks/set-state-in-effect`).
- Nol baris komentar, nol inline style, tidak ada CSS baru, tidak ada komponen base baru.

`UI GATE: 13 elemen — REUSE 11, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kepala halaman | `Hero` | `base-features/hero.jsx`, dipakai `FE-AST-002` | REUSE | — |
| Kartu ringkasan | `SummaryGrid` | `summary-grid.jsx` | REUSE | Tiga kartu "saya" |
| Penyaring, cari, tab status | `DataFilter` (`tabs`) | `data-filter.jsx` `tabs`/`activeTab` | REUSE | — |
| Tabel + paginasi | `DataTable`, `Pagination` | `data-table.jsx` | REUSE | — |
| Status | `StatusBadge` | `status-badge.jsx`, nada dari `asset-status-labels.js` | REUSE | — |
| Aksi baris | `RowActionMenu` | `row-action-menu.jsx` (`disabled`, `title`) | REUSE | — |
| Alasan batal/tolak | `ConfirmModal` `requireReason` | `confirm-modal.jsx` | REUSE | — |
| Formulir bergrup, atribut medis bersyarat | `BaseGroupedEditorView` | `hidden`, `groups`, `onFieldChange` | REUSE | — |
| Harga rupiah | isian teks + `formatThousandsInput`/`normalizeWholeRupiahInput` | `input-normalizer-utils.jsx` | REUSE | — |
| Dua tombol simpan | `formProps.renderActions` + `BaseButton` | `base-grouped-editor-form.jsx` | REUSE | — |
| Pilihan kelompok/kepemilikan/GR | pilihan remote `optionResource` | registry pilihan; `goodsReceipts` sudah ada | REUSE | — |
| Kolom centang | `DataTable` + kolom `header`/`render` | `DataTable` tanpa prop pilih | COMPOSE | A |
| Hasil massal per aset | `ConfirmModal` `hideCancel` + `children` | `confirm-modal.jsx` | COMPOSE | A |

Pilihan yang disajikan sebelum menulis kode:

- **Kolom centang.** **A. Rangkai lewat definisi kolom — dipakai.** Base tidak berubah, jadi tidak ada risiko
  bagi layar lain. B. Tambah prop `selectable` ke `DataTable`: lebih rapi untuk dipakai ulang, tetapi menyentuh
  komponen yang dipakai puluhan layar.
- **Hasil massal.** **A. Modal berisi ringkasan, nomor terbit, dan daftar gagal — dipakai.** B. Toast saja:
  hasil per aset hilang setelah beberapa detik, jadi kriteria 3 lemah.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable` memuat; kartu ringkasan berkerangka; formulir ubah menunggu rincian + `/amounts` |
| Kosong | "Belum ada draft." / "Belum ada aset yang menunggu persetujuan." |
| Gagal | `InformationAlert` + `AccessDeniedGate` (403 → layar Akses Ditolak) |
| Galat per isian | `AST-VAL-REG-001..006`, `008`, `009` → isian terkait; `007` → isian medis yang kosong |
| Data basi (`015`) | Toast "Data Sudah Berubah" + rincian dimuat ulang |
| Kirim ganda | Tombol `loading`/nonaktif selama permintaan; persetujuan massal nonaktif saat memuat |
| Tanpa hak | Tombol Catat/Ubah/Ajukan/Setujui disembunyikan menurut `usePermission`; kolom centang hilang tanpa `Approve` |
| Sebagian gagal diajukan sesudah disimpan | Peringatan tetap di atas formulir; untuk draft baru, tombol simpan dikunci supaya draft tidak dibuat dua kali |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Register / Asset (`api/v1/corporate/asset-management/register/assets`)

| Layar | Endpoint |
|---|---|
| Daftar draft | `GET /?createdByMe=true&recordStatus=…` (tabel), tiga `GET /?createdByMe=true&recordStatus=n&pageSize=1` (kartu) |
| Formulir | `POST /drafts`, `GET /{id}`, `GET /{id}/amounts`, `PUT /{id}`, `POST /{id}/submit` |
| Daftar draft (aksi) | `POST /{id}/submit`, `POST /{id}/cancel` |
| Nomor Inventaris | `GET /?recordStatus=2`, `POST /approve-batch`, `POST /{id}/approve`, `POST /{id}/reject` |
| Pilihan | `GET /master-data/groups?isActive=true`, `GET /master-data/ownership-types/options`, `GET /finance-management/purchasing/goods-receipts?status=RECEIVED` |

Enum dikirim sebagai angka (`acquisitionType`, `recordStatus`), karena backend tidak memasang
`JsonStringEnumConverter`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 16 berkas | `0 errors, 0 warnings` | `PASS` | Putaran pertama 0 galat / 5 peringatan React Compiler (`set-state-in-effect`, `preserve-manual-memoization`); diperbaiki dengan pola respons berkunci |
| Grep anti-regresi checklist UI (tombol/tabel mentah, `fw-`/`fs-`, inline style, CSS baru, komentar) | Semua kosong | `PASS` | — |
| `npm run build` | Dijalankan Rizki dan berhasil. `BUILD_ID` `v4-uoNfSTi4jLS1nOoz7d` (2026-11-17 08:58:57); manifest memuat 4 route `register` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| `dotnet build` (tambalan backend) | Dijalankan Rizki dan berhasil, 601 warning; DLL 08:54:22, sesudah suntingan 08:27:17. Respons daftar memuat `createBy`, `submittedBy`, `rejectedAt`, `decisionReason` | `PASS` | Panggilan API langsung |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-003-20261117/uji-pencatatan.mjs`, tag `FE003-2LV8`) | **24 langkah, semua sesuai harapan**; 0 galat halaman, 0 respons 5xx | `PASS` | `hasil-uji.jsonl`, 9 tangkapan layar |

Rincian uji layar (pengaju = superadmin uji, penyetuju = akun "Uji Penyetuju Aset", tanpa hak = LOKET):

| Langkah | Hasil |
|---|---|
| Daftar draft | 200; permintaan memuat `createdByMe=true`; 4 tab; 3 kartu; tombol Catat ada; grup menu tampil; tanpa luapan horizontal halaman |
| Formulir awal | Atribut medis tersembunyi; Simpan Draft dan Simpan & Ajukan ada |
| **AC2** | Pilih Alat Kedokteran Umum → atribut medis muncul; Mebel → hilang; kembali → muncul. Harga diketik `45000000` tampil `45.000.000` |
| Validasi layar | Simpan & Ajukan tanpa atribut medis → 5 pesan "Wajib diisi untuk alat medis", **0 POST** |
| Simpan draft | POST 200, kembali ke daftar, baris tampil |
| Ubah draft | Nama dan harga (`45.000.000`, dari `/amounts`) terisi; atribut medis tampil; Jumlah Unit tersembunyi; isi atribut → PUT 200 + submit 200 → pindah ke tab Menunggu Persetujuan |
| Alat medis 2 unit | Simpan & Ajukan tidak tampil; penjelasan tampil |
| Kursi Mebel 3 unit | Simpan & Ajukan → POST 200, 3 baris di Menunggu Persetujuan |
| Batal draft | Tombol konfirmasi nonaktif tanpa alasan; dengan alasan → 200, baris di tab Dibatalkan |
| **AC4** (pengaju di antrean) | Centang baris sendiri nonaktif, badge "Anda", menu Setujui nonaktif, centang semua nonaktif |
| Antrean penyetuju | 5 baris uji; centang aktif; tombol "Setujui Terpilih (0)" |
| Tolak | Kembalikan ke draft beralasan → 200, baris keluar antrean |
| **AC3** | Centang 4 → "Setujui Terpilih (4)" → 200: `AST-ALKMED-000004`, `AST-MBL-000005..000007`; modal "4 disetujui, 0 gagal." dengan daftar nomor; antrean kosong |
| Penyetuju di Pencatatan | Tombol Catat tidak tampil (tanpa `Create`) |
| Draft dikembalikan (pengaju) | Badge "Dikembalikan" + alasan di kolom Catatan |
| Tab Terdaftar (pengaju) | Monitor bernomor `AST-ALKMED-…`, 3 kursi |
| LOKET | Layar Akses Ditolak; grup menu tidak tampil; tombol Catat tidak tampil |

Satu galat skrip terjadi di putaran awal: skrip mengklik opsi sebelum pilihan remote selesai dimuat. Skrip
diperiksa terpisah (`periksa-kepemilikan.mjs`: 4 opsi termuat dan dapat dipilih), lalu diubah supaya menunggu
opsi tampil. Layar tidak diubah.

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`** (lewat layar, izin Rizki untuk uji aset): 1 monitor dan 3 kursi
`Terdaftar` (nomor di atas), 1 meja `Draft` (dikembalikan), 1 lemari `Dibatalkan`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Skema §3.2 dan §3.3 | Terpenuhi | Daftar "draft saya" + formulir + Simpan & Ajukan; antrean dengan centang, Setujui Terpilih, Tolak beralasan; uji layar. Delta rupa: draft yang ditolak ditandai per baris, bukan kartu "n ditolak" (hitungan itu tidak dapat disaring backend) |
| 2. Atribut medis tampil bila kelompok medis | Terpenuhi | `getDraftFields` + `extendOption.isMedical`; uji AC2 |
| 3. Persetujuan massal menampilkan hasil per aset | Terpenuhi | Modal ringkasan + nomor + daftar gagal; uji AC3 (4/4). Tampilan daftar gagal tidak terpicu di uji karena 0 gagal; jalur gagal per aset di backend teruji pada `BE-AST-007` |
| 4. Baris milik pengaju tidak dapat dicentang | Terpenuhi | `isSubmittedByUser` dari `submittedBy`; uji AC4 |
| 5. Butir menu "Pencatatan Aset Baru" dan "Nomor Inventaris" | Terpenuhi | `menu-items.jsx`; tampil untuk pemegang `AssetRegister : Read`, tidak untuk LOKET |
| `DoD-FE` | Terpenuhi | Lint, build pemilik, uji layar, laporan, roadmap, traceability |

Rekap: **5 dari 5 kriteria terpenuhi** → `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) Pada lebar 1600 px, tabel daftar draft (9 kolom) perlu digeser agar kolom Aksi terlihat. Fungsinya tidak terganggu (pergeseran di dalam `DataTable`), tetapi bisa dirapikan dengan mengecilkan `minWidth` kolom pada perbaikan berikutnya (perlu build ulang). (2) Harga dibulatkan ke rupiah utuh pada formulir; backend menerima dua desimal. (3) Kolom Pengaju di antrean hanya membedakan "Anda" / "Petugas lain", karena daftar tidak membawa nama pengaju |
| Risiko | Data uji di DB dev bertambah (bagian 6) |
| Perubahan sampingan | Tidak ada. Dua berkas lain yang berubah di working tree frontend (layar dokter IGD dan rawat inap) sudah ada sebelum task ini dan tidak disentuh |
| Status Git | Frontend `RizkiV2`: 2 berkas diubah, 14 berkas baru, belum di-commit. Backend `rizkiG`: 2 berkas tambalan + dokumen, belum di-commit |
| Langkah berikutnya | `FE-AST-004` (daftar inventaris, data perolehan, detail aset) |
