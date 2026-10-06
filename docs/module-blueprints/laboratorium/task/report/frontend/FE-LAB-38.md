# Laporan Perubahan Frontend — `FE-LAB-38`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-38` |
| Judul | Dua layar data induk alasan |
| Slice | Gelombang `MVP-9c` — `EPIC-LAB-15` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-9`, bagian `FE-LAB-38` |
| Trace | `FR-15.12`; `LAB-DEC-082`, `LAB-DEC-138`, `LAB-DEC-003`, `LAB-DEC-019` |
| Contract version | `LAB-API-v1` `r34` bagian 29.6 (`approved` 2026-09-25); `LAB-VAL-v1` `r12` `VAL-140`..`VAL-142`; `LAB-PERM-v1` rev 11 bagian 13.2 |
| Wewenang UI | Diputuskan roadmap: daftar berhalaman, penyaring, ringkasan, tambah, ubah, aktif/nonaktif hanya lewat `PATCH /{id}/status`, kode baca-saja pada formulir ubah, sakelar *wajib catatan* hanya bagi pemegang `SystemFlag`, nol tombol hapus. `DEV_DISCRETION` yang dipakai: nama berkas; sakelar *wajib catatan* sebagai **aksi baris berkonfirmasi** (bukan kotak centang di formulir); keterangan terkunci *wajib catatan* di bawah formulir; lencana netral untuk *Tanpa catatan wajib* — lihat 3.3 |
| Dependency | `BE-LAB-71` ✅ (terbukti lewat HTTP sejak 2026-09-29) |
| Klasifikasi | `STANDARD` — dua layar baru (24 berkas baru: 11 per layar, satu berkas aturan bersama, satu berkas uji), 3 berkas bersama diubah (konstanta API, store, menu); nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`), di atas `FE-LAB-35`..`FE-LAB-37` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` + `BE-LAB-87` (belum ter-commit), `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-01 |
| Status | ✅ **`SELESAI`** (naik 2026-10-02) — pemisahan kepala instalasi/admin kini dibuktikan dengan **akun Kepala Instalasi asli** (dr. Bima) dan superadmin sebagai pengganti System Administrator yang belum punya akun: 7/7, nol tulis ke database. Lihat 9. *(Semula 2026-10-01: ⚠ — kedua layar hidup; nol hapus; kode tak dapat diubah; pesan `VAL-140`..`VAL-142` tampil pada isian yang tepat. Uji unit 13/13, lint nol peringatan, build hijau, layar 35/35 terhadap backend lokal. **Batas verifikasi:** pemisahan kepala instalasi/admin dibuktikan dengan **daftar izin tiruan**, bukan akun sungguhan — sandi akun Kepala Instalasi dev tidak tersedia pada sesi ini, dan akun System Administrator belum ada — lihat 6. Naik menjadi ✅ sesudah dijalankan dengan akun Kepala Instalasi dan System Administrator)* |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Kedua resource sudah hidup di backend, dengan sembilan endpoint berbentuk sama dan **satu** set aturan `LabResultReasonRules` | `LabResultCorrectionReasonController.cs`, `LabFourEyesExceptionReasonController.cs`, `LabResultReasonMasterDataService.cs` |
| Galat `409`/`422` hanya membawa **pesan**, tanpa nama ruas; setiap pesan diawali subjek ruasnya (*Kode alasan …*, *Nama alasan …*, *Keterangan alasan …*) | `LabResultReasonRules` |
| Backend **menolak** huruf kecil pada kode, bukan menormalkannya | `LabResultReasonRules.ValidateCode` |
| `UpdateRequest` tidak membawa `IsActive`; `CreateRequest` tidak membawa `RequiresNote` | `LabResultReasonMasterDataDtos.cs` |
| Data dev: alasan pengembalian `SAMPEL-TERTUKAR`, `SALAH-KETIK`; alasan pengecualian `SHIFT-TUNGGAL`; semuanya aktif, nol wajib catatan | `GET` kedua daftar dan ringkasan |
| Pola terdekat: Alasan Penolakan (sakelar penanda sistem) dan Organisme (formulir ubah lewat `GET /{id}`, `usePermission` untuk `Create`/`Update`) | `master-data/lab-rejection-reasons`, `master-data/lab-organisms` |
| `AGENTS.md` frontend melarang hook atau view master data generik dan factory baru | `AGENTS.md` bagian *Aturan Master Data* |

---

## 2. Proses bisnis dari sisi pengguna

**Kepala instalasi** (izin `Read`/`Create`/`Update`):

1. Membuka **Laboratorium → Master Data → Alasan Pengembalian Hasil**. Ringkasan menunjukkan total, aktif,
   nonaktif, dan jumlah yang wajib catatan; tabel diurutkan menurut *Urutan Tampil*.
2. Menekan **+ Tambah Alasan Pengembalian Hasil**, mengetik kode — huruf langsung tampil besar — nama, keterangan,
   dan urutan, lalu menyimpan. Kode yang sudah dipakai ditolak dengan *"Kode alasan ini sudah dipakai."* tepat di
   bawah isian **Kode Alasan**.
3. Membuka **Perbarui** (atau klik ganda baris). Kode tampil **abu-abu baca-saja** dengan keterangan *"Kode tidak
   dapat diubah. Buat alasan baru bila perlu."*
4. Menonaktifkan alasan yang tidak lagi dipakai lewat aksi baris **Nonaktifkan** dan konfirmasi. Tidak ada tombol
   hapus di mana pun.

**Admin sistem** (izin `Read`/`SystemFlag`): hanya melihat aksi baris **Wajibkan Catatan** / **Lepas Wajib
Catatan**, berkonfirmasi. Tombol tambah, perbarui, dan aktif/nonaktif tidak dirender baginya.

Layar **Alasan Pengecualian Empat Mata** identik bentuknya; salinan teksnya menyebut validasi atau rilis yang
merangkap peran.

**Jalur tidak normal:** `403` ditampilkan sebagai kalimat izin, bukan sebagai galat isian; `404` detail
ditampilkan apa adanya dari backend (*"Alasan … tidak ditemukan."*); tautan ubah tanpa token sah menampilkan
pesan dan tidak menyimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-38`; `contracts/api-contract.md` 29.6 | Cakupan, kriteria, kontrak |
| Backend `LabResultCorrectionReasonController.cs`, `LabFourEyesExceptionReasonController.cs`, `LabResultReasonMasterDataDtos.cs`, `LabResultReasonMasterDataService.cs`, `LabFilterMetadataFactory.LabResultReason` | Perilaku runtime: izin per aksi, pesan galat, ukuran halaman `10/25/50/100`, urutan tetap per `SortOrder` |
| `master-data-resource-slice-factory.jsx` dan pemakainya (`company-guarantor`) | Factory yang sudah ada — dipakai ulang |
| Seluruh fitur `lab-rejection-reasons` dan `lab-organisms` | Pola layar, hook, formulir, token route |
| `use-permission.js`, `permission-slice.jsx`, `row-action-menu.jsx`, `base-editor-field.jsx`, `status-badge.jsx` | Perilaku izin "belum diketahui", ruas `readOnly`, varian lencana |
| `AGENTS.md` | Larangan hook/view generik dan factory baru |

### 3.2 Berkas yang berubah

**Bersama kedua layar**

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/laboratory-management/master-data/lab-result-reason-rules.js` (baru) | Aturan murni `LabResultReasonResponse`: pembaca baris, `validateReasonForm` dengan **kalimat yang sama persis** dengan backend, `buildReasonCreatePayload` / `buildReasonUpdatePayload` (tanpa `reasonCode`, `isActive`, `requiresNote`), `buildReasonListParams` (empat ruas `PagedQuery` saja), **`mapReasonErrorToField`** (`409`/`422`/`400` → isian menurut awalan pesan; `403` → kalimat izin; selainnya milik halaman), `normalizeReasonCodeInput` |
| `src/lib/constants/.../laboratory-constants.jsx` | `LABORATORY_API.labResultCorrectionReasons`, `labFourEyesExceptionReasons` |
| `src/lib/state/store.jsx` | Reducer `masterDataLabResultCorrectionReason`, `masterDataLabFourEyesExceptionReason` |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua butir di bawah *Laboratorium → Master Data*, sesudah *Alasan Penolakan Sampel*, ber-`requiredPermission` `Read` resource masing-masing |
| `tests/unit/lab-result-reason-rules.test.mjs` (baru) | 13 uji — lihat 6 |

**Per layar** (sepasang berkas untuk `lab-result-correction-reasons` dan `lab-four-eyes-exception-reasons`)

| Berkas | Isi |
| --- | --- |
| `src/lib/state/slice/health-services/master-data/master-data-lab-<entitas>-slice.jsx` | `createMasterDataResourceSlice` yang sudah ada: metadata, ringkasan, daftar, detail, tambah, ubah, status `PATCH /{id}/status`. Thunk hapus bawaan factory **tidak diekspor**. Thunk `system-flags` berdiri sendiri (`PUT /{id}/system-flags`) |
| `src/lib/constants/.../master-data/lab-<entitas>s/lab-<entitas>-constants.jsx` | Ruas formulir (tambah: kode dapat diisi; ubah: kode `readOnly`), salinan teks, konfigurasi ringkasan, salinan *wajib catatan* |
| `src/lib/hooks/.../master-data/lab-<entitas>s/use-master-data-lab-<entitas>.jsx` | Penyaring, halaman, status, sakelar *wajib catatan*; izin `Create`/`Update` (longgar) dan `SystemFlag` (**ketat**) |
| `src/lib/hooks/.../master-data/lab-<entitas>s/use-master-data-lab-<entitas>-editor.jsx` | Formulir tambah/ubah; mode ubah memuat `GET /{id}`; token route lewat `useSyncExternalStore` (nol `setState` di efek); galat simpan dipetakan ke isian |
| `src/components/view/.../master-data/lab-<entitas>s/master-data-lab-<entitas>-view.jsx` | Halaman daftar dan dua `ConfirmModal` |
| `src/components/view/.../master-data/lab-<entitas>s/add/lab-<entitas>-form-view.jsx` | `BaseEditorView` dan keterangan terkunci *wajib catatan* |
| `src/app/.../master-data/lab-<entitas>s/{page.jsx, lab-<entitas>s-client.jsx, create/page.jsx, [slug]/update/page.jsx, [slug]/route-token.js}` | Route |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| Hook dan view per entitas, bukan satu yang generik | `AGENTS.md` melarang hook/view master data generik. Layar kedua diturunkan dari yang pertama dengan substitusi nama, lalu salinan teksnya ditulis ulang |
| Satu berkas aturan bersama | Backend menulis aturan kedua data induk **satu kali** (`LabResultReasonRules`, `LAB-VAL-v1` 14.4). Dua salinan validasi dan pemetaan galat pasti bercabang; berkasnya fungsi murni atas DTO yang sama, bukan hook atau view |
| Slice lewat factory yang sudah ada | `createMasterDataResourceSlice` dipakai ulang tanpa diubah; `system-flags` tidak didukung factory, sehingga thunk-nya berdiri sendiri dan status sibuknya dipegang hook |
| *"Dua fungsi service data induk"* pada roadmap | Ditafsirkan sebagai dua slice. Pola terdekat (Alasan Penolakan, Organisme) memanggil `InstanceAxios` dari slice, tanpa berkas `*.service.js` |
| Sakelar *wajib catatan* | Aksi baris **Wajibkan Catatan** / **Lepas Wajib Catatan** dengan konfirmasi — satu nilai boolean, tanpa kotak centang. Tampil hanya bila daftar izin **sudah termuat dan** memuat `SystemFlag`, supaya kepala instalasi tidak melihatnya sekejap sebelum daftar tiba |
| Tombol tambah, perbarui, aktif/nonaktif | `usePermission` biasa (tampil selama daftar izin belum diketahui; backend menjawab `403`) — sama dengan Organisme |
| Kode huruf besar | Diubah saat diketik, sehingga yang terlihat adalah yang dikirim; spasi dan karakter lain **dibiarkan** supaya ditolak dengan pesan `VAL-141`, bukan hilang diam-diam |
| Penyaring tanggal/periode | Tampil mengikuti kontrak baku master data, hidup di state UI saja, **tidak** dikirim ke API (uji layar S2) |
| Lencana *Tanpa catatan wajib* | Varian `info` (netral), bukan `inactive` (merah) — "tidak wajib" bukan keadaan bermasalah |

`UI GATE: 14 elemen — REUSE 14` — `Hero`, `SummaryCards`, `DataFilter`, `FilterDatePicker`, `FilterSelect`,
`DataTable`, `RegionPagination`, `RowActionMenu`, `StatusBadge`, `ConfirmModal`, `ToastStack`,
`AccessDeniedGate`, `BaseEditorView`, `InformationAlert`. Nol komponen baru, nol CSS Module baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` *loading* dan kartu ringkasan *loading*; formulir ubah *loading* sampai `GET /{id}` kembali |
| Kosong | *"Data alasan … tidak ditemukan."* (dibuktikan dengan penyaring Nonaktif) |
| Gagal muat | Pesan backend di atas tabel; `AccessDeniedGate` untuk `403` |
| Galat isian | Di bawah isiannya: validasi layar dan `409`/`422` backend, berkalimat sama |
| Tanpa hak akses | Aksi yang tidak berhak tidak dirender; `403` saat menyimpan → kalimat izin di atas formulir |
| Sibuk | Aksi baris dan tombol konfirmasi dinonaktifkan; penjaga ref mencegah kirim ganda sakelar |
| Data tidak dapat dihapus | Nol tombol hapus; deskripsi daftar menyatakannya |

---

## 5. Endpoint yang dikonsumsi

Kedua base URL: `/api/v1/health-services/laboratory-management/lab-result-correction-reasons` dan
`/lab-four-eyes-exception-reasons`.

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `GET` | `/`, `/filters/metadata`, `/summary`, `/{id}` | Daftar, ukuran halaman, kartu ringkasan, formulir ubah |
| `POST` | `/` | Tambah — `{ reasonCode, reasonName, description, sortOrder }` |
| `PUT` | `/{id}` | Ubah — `{ reasonName, description, sortOrder }` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif — `{ isActive }` |
| `PUT` | `/{id}/system-flags` | Wajib catatan — `{ requiresNote }` |

`GET /options` tidak dipakai di sini — milik `FE-LAB-39`. **Delta dokumen:** kolom *Status* tabel 29.6 di
`api-contract.md` masih bertulis *"Rencana (belum tersedia)"*, padahal `BE-LAB-71` ✅ sejak 2026-09-29. Tidak
diubah dari task frontend ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-result-reason-rules.test.mjs` | **13/13** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2230 uji: 2224 lolos, 6 gagal — kegagalan lama di luar Laboratorium (Hemodialisa ×4, Bank Darah M0, petty cash); `menu-permission-filter` lolos | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` berkas yang disentuh | Nol masalah (dua peringatan `set-state-in-effect` pada versi awal hook formulir diperbaiki) | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0; enam route baru terdaftar | `PASS` | Keluaran build |

Isi uji unit: kalimat validasi layar sama persis dengan backend; `VAL-140` `409` → *Kode Alasan*; enam pesan
`VAL-141` `422` → isian masing-masing; `VAL-142` → *Kode Alasan*; bentuk galat thunk/Axios/teks; `403`, `404`,
`500` berawalan *Kode alasan*, dan galat jaringan tetap milik halaman; batas 32/200/256; urutan negatif dan
pecahan; mode ubah tidak memeriksa kode; payload ubah hanya tiga ruas; payload tambah tanpa `requiresNote`;
huruf besar saat diketik; parameter daftar hanya empat ruas.

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal, akun superadmin (memegang keempat
aksi kedua resource). `PATCH`/`PUT` **dicegat dan dijawab tiruan**; satu-satunya non-GET yang diteruskan adalah
`POST` dengan kode yang sudah ada, yang ditolak `409` **sebelum** menyimpan. Data dev sesudahnya sama persis
dengan sebelumnya (2/2/0/0 dan 1/1/0/0).

| Skenario | Hasil sebenarnya |
| --- | --- |
| S1 Daftar Alasan Pengembalian | `SAMPEL-TERTUKAR`, `SALAH-KETIK` urut *Urutan Tampil*; ringkasan 2/2/0/0; kolom *Wajib Catatan* berteks; butir menu kedua layar ada |
| S2 Penyaring | Nonaktif → `isActive=false`, keadaan kosong; atur ulang mencabut `isActive`; cari *ketik* → `search=ketik`, sisa `SALAH-KETIK`; 10 baris → `pageSize=10&pageNumber=1`; nol `startDate`/`endDate`/`customPeriod` ke API |
| S3 Aksi baris superadmin | *Perbarui*, *Nonaktifkan*, *Wajibkan Catatan*; nol *Hapus* di seluruh halaman |
| S4 Nonaktifkan | Konfirmasi berkalimat riwayat; `PATCH …/{id}/status` `{"isActive":false}`; toast sukses |
| S5 Wajibkan Catatan | Konfirmasi; `PUT …/{id}/system-flags` `{"requiresNote":true}`; toast sukses |
| S6 `VAL-140` nyata | `POST` `{"reasonCode":"SAMPEL-TERTUKAR",…}` → `409`; *"Kode alasan ini sudah dipakai."* di bawah *Kode Alasan*; tetap di halaman tambah |
| S7 Validasi layar | *salah ketik* tampil *SALAH KETIK*; pesan format kode dan *Nama alasan wajib diisi.* pada isiannya; nol permintaan |
| S8 Ubah | Klik ganda → alamat bertoken (bukan UUID); kode baca-saja `SALAH-KETIK`; `PUT` hanya `reasonName`, `description`, `sortOrder`; `422` `VAL-142` tiruan tampil di bawah *Kode Alasan*; sukses kembali ke daftar |
| S9 Formulir | *"Wajib Catatan: Tanpa catatan wajib (Terkunci — hanya admin sistem)"* |
| S10 Layar Empat Mata | `SHIFT-TUNGGAL`, total 1; aksi baris sama; `VAL-140` nyata pada isian kode |
| S11 Izin tiruan kepala instalasi (`Read`/`Create`/`Update`) | Aksi baris *Perbarui*, *Nonaktifkan* — **sakelar wajib catatan tidak ada**; tombol tambah ada |
| S12 Izin tiruan admin sistem (`Read`/`SystemFlag`) | Aksi baris hanya *Wajibkan Catatan*; tombol tambah tidak ada; klik ganda tidak membuka formulir |

Layar: **35/35** `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-result-reason-rules.test.mjs — PASS`

`MANUAL TEST: PASS` — dengan batas di bawah.

**Batas verifikasi:**

- Akun Kepala Instalasi dev (dr. Bima, dipakai `BE-LAB-71`) ada, tetapi sandinya tidak tersedia pada sesi ini;
  akun System Administrator belum ada sama sekali (`BE-LAB-71` 8.2). Pemisahan S11/S12 dibuktikan dengan
  **daftar izin tiruan** pada `GET /v1/auth/permissions`; penegakan sesungguhnya tetap `AccessPermission`
  backend (`BE-LAB-71`).
- Tambah, ubah, status, dan sakelar yang **berhasil** tidak dijalankan terhadap database — tanpa izin tulis ke
  data induk dev. Bentuk permintaannya dibuktikan lewat pencegatan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pesan `VAL-140`..`VAL-142` tampil terbaca pada isian yang tepat | Terpenuhi | Uji unit; S6, S8, S10 |
| Sakelar sistem tersembunyi bagi kepala instalasi dan tampil bagi admin | ✅ Terpenuhi — **akun Kepala Instalasi asli** (2026-10-02) dan admin (superadmin pengganti + izin tiruan `Read`/`SystemFlag`) | S11, S12; K1, K2, K5–K7 di 9 |
| Verifikasi — unit test aturan pemetaan galat ke ruas | Terpenuhi | 13/13 |
| Verifikasi — kedua layar terhadap backend `BE-LAB-71` dengan akun kepala instalasi dan admin | ✅ Terpenuhi 2026-10-02 — kepala instalasi **asli**; admin diwakili superadmin, sebab akun System Administrator belum ada (`BE-LAB-71` 8.2) | 6; 9 |
| DoD — kedua layar hidup; nol hapus; kode tak dapat diubah | Terpenuhi | S1, S3, S8, S10 |
| DoD — lint dan build hijau; laporan | Terpenuhi | 6; berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint; build tanpa error |
| Risiko | (1) Pemetaan galat ke isian bergantung pada **awalan pesan** backend, karena backend tidak mengirim nama ruas; uji unit pertama menjaga kalimatnya. (2) Bila daftar izin gagal dimuat, sakelar *wajib catatan* tersembunyi juga bagi admin — sengaja, agar kepala instalasi tidak pernah melihatnya; admin cukup memuat ulang |
| Masalah yang diketahui | Kolom *Status* kontrak 29.6 masih *"Rencana (belum tersedia)"* (lihat 5) |
| Dependency backend | Nol perubahan; `BE-LAB-71` dipakai apa adanya |
| Perubahan sampingan | `NONE` di repository. **Lingkungan:** sebelum build, `TaskStop` kembali meninggalkan dua proses `next dev`; keduanya dihentikan dan port 3000/7184/5107 dipastikan bebas **sebelum** build dijalankan |
| Interupsi | `NONE` |
| Status Git | Frontend (`e613321c5`): ` M` `laboratory-constants.jsx`, `store.jsx`, `menu-items.jsx` (sebagian milik task ini; `menu-items.jsx` juga memuat perbaikan impor `RiSafeLine` dari `FE-LAB-35`); `??` seluruh berkas `lab-result-correction-reasons`, `lab-four-eyes-exception-reasons`, `lab-result-reason-rules.js`, `lab-result-reason-rules.test.mjs`; sisanya milik `FE-LAB-35`..`FE-LAB-37`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-39` (`SIAP DIKERJAKAN`) — memakai `GET /options` kedua daftar ini untuk *Kembalikan ke analis* dan pertanyaan alasan pengecualian |

---

## 9. Verifikasi susulan 2026-10-02 — akun Kepala Instalasi asli

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir**. **Seluruh
tulis dicegat** dan dijawab tiruan — nol data induk berubah.

**Akun.** **dr. Bima (Kepala Instalasi, asli** — pemegang `Read`/`Create`/`Update`, tanpa `SystemFlag`);
**superadmin** sebagai pengganti System Administrator, sebab akun itu belum ada di dev (`BE-LAB-71` 8.2).
Daftar izin persis milik admin (`Read`/`SystemFlag` saja) sudah dibuktikan S12 dengan izin tiruan.

| ID | Akun | Skenario | Hasil | Bukti |
| --- | --- | --- | --- | --- |
| K1 | dr. Bima | Alasan Pengembalian: Tambah, Perbarui, Aktif/Nonaktif ada; **sakelar wajib catatan TIDAK ada**; nol Hapus | `PASS` | Menu `["Perbarui","Nonaktifkan"]` |
| K2 | dr. Bima | Alasan Empat Mata: sama | `PASS` | Menu `["Perbarui","Nonaktifkan"]` |
| K3 | dr. Bima | `VAL-142` formulir ubah: *Kode Alasan* baca-saja | `PASS` | Alamat bertoken `…/sampel-tertukar-…/update` |
| K4 | dr. Bima | Permintaan ubah **tidak** membawa kode, `isActive`, maupun `requiresNote` | `PASS` | Badan `{reasonName, description, sortOrder}` (dicegat) |
| K5 | superadmin | Alasan Pengembalian: sakelar wajib catatan **tampil** | `PASS` | Menu `[… "Wajibkan Catatan"]` |
| K6 | superadmin | Alasan Empat Mata: sama | `PASS` | — |
| K7 | superadmin | Sakelar berkonfirmasi lalu `PUT …/{id}/system-flags {requiresNote}` | `PASS` | Badan `{"requiresNote":true}` (dicegat) |

Nol perubahan kode diperlukan untuk task ini. Validasi akhir sesi (2026-10-06): uji unit 2308/2314
(6 gagal = baseline, nol Laboratorium), `lint:errors` 0 error, `npm run build` hijau. **Nol operasi Git
dijalankan.**
