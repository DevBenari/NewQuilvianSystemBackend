# Laporan Perubahan Frontend — `FE-LAB-34`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-34` |
| Judul | Tiga layar data induk baru — breakpoint, profil Mikrobiologi katalog, pengaturan disiplin |
| Slice | `S4b` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Trace | `LAB-DEC-119`, `LAB-DEC-122`, `LAB-DEC-125`, `LAB-DEC-127`; `AC-182`, `AC-185`, `AC-189` |
| Kontrak | `LAB-API-v1` `r27` bagian 22.5/22.6/22.7 **dan `r30` bagian 25** — seluruhnya `approved` |
| Standar wajib | `rules/frontend/master-data-feature-standard.md` |
| Dependency | `BE-LAB-60` ✅, `BE-LAB-62` ✅, `BE-LAB-63` ✅, **`BE-LAB-64` ✅** |
| Klasifikasi | `HEAVY` — tiga fitur, 38 berkas source, 6 registrasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — ketiga layar diklik di peramban terhadap backend lokal dan PostgreSQL dev dengan **Kepala Instalasi asli** (dr. Bima), **analis asli** (Vina), dan superadmin sebagai pengganti **wewenang klinis** Mikrobiologi yang belum punya akun: 14/14 sesudah satu perbaikan + 5/5. `403` breakpoint tampil sebagai pesan terbaca. Lihat 7. *(Semula 2026-09-22: ⚠ — hak akses terbukti lewat HTTP; belum diklik di peramban.)* |

---

## 1. Dua temuan yang ditemukan sebelum satu baris pun ditulis

### 1.1 Layar rujukan yang disebut roadmap TIDAK ADA

Roadmap menulis *"Reuse: pola yang sama dengan layar `LabOrganism` dan `LabAntibiotic`"*.
Penelusuran menemukan **nol berkas** untuk keduanya: `LabOrganism` dan `LabAntibiotic` punya
controller backend, tetapi **nol pernah dibuatkan layar**. Baris reuse itu menunjuk ke sesuatu
yang belum pernah ada.

### 1.2 Layar Laboratorium terdekat yang ada TIDAK BOLEH ditiru

`lab-specimen-types` adalah fitur data induk Laboratorium paling dekat. Ia **nol punya halaman
detail**, **nol punya hook detail**, dan **memuat CSS Module** — sedangkan standar menyatakan
fitur master data konform nol menambah CSS Module, dan menggolongkan fitur tanpa halaman detail
sebagai *"belum lengkap — laporkan sebagai kekurangan, jangan jadikan contoh"*.

**Rujukan yang dipakai karena itu `hr/master-data/job-level`**, yakni modul rujukan otoritatif
yang disebut standar itu sendiri. Strukturnya disalin lalu diisi ulang — bukan diabstraksi
menjadi generator, sesuai larangan `AGENTS.md` frontend.

---

## 2. Yang dibangun

### 2.1 `lab-susceptibility-breakpoint` dan `lab-procedure-microbiology-profile` — bentuk penuh

Tujuh berkas source ditambah lima route, masing-masing:

| Berkas | Isi |
|---|---|
| `constants/.../<fitur>-constants.jsx` | Satu objek `<FITUR>_CONFIG` beku |
| `state/slice/.../master-data-<fitur>-slice.jsx` | **Sembilan thunk**, satu per endpoint baseline |
| `utils/.../<fitur>-utils.jsx` | Fungsi murni — nol hook, nol JSX, nol request |
| `hooks/.../use-master-data-<fitur>.jsx` | Controller daftar |
| `hooks/.../use-master-data-<fitur>-detail.jsx` | Controller detail |
| `hooks/.../use-master-data-<fitur>-editor.jsx` | Controller tambah dan ubah |
| `view/.../` | `master-data-<fitur>-view`, `detail/`, `add/` |

### 2.2 `lab-discipline-setting` — varian "pengaturan tunggal"

**Empat thunk, bukan sembilan, dan itu mengikuti backend-nya.** Endpoint yang ada memang hanya
empat: nol `POST`, nol `DELETE`, nol `summary`, nol `filters/metadata`. Menyediakan thunk bagi
endpoint yang nol ada berarti menaruh jalan yang berujung `404`.

Layarnya karena itu **nol punya tombol tambah dan nol punya tombol hapus** — butir DoD — dan
ketiadaannya **dinyatakan di layar** lewat `InformationAlert`, bukan didiamkan. Tombol yang
sekadar hilang terbaca sebagai hak akses yang kurang, dan pertanyaannya berakhir sebagai tiket.

### 2.3 Registrasi

| Tempat | Isi |
|---|---|
| `state/store.jsx` | Tiga reducer, memakai `stateKey` dari masing-masing config |
| `menu-sidebar/menu-items.jsx` | Tiga entri di bawah data induk Health Services |
| `hooks/select/health-service/health-service-select-resources.js` | **Dua sumber pilihan baru**: `labOrganisms` dan `labAntibiotics` |

**Nol CSS Module baru.** Hanya `base-data-components.module.css` yang dipakai.

---

## 3. Empat keputusan yang layak dibaca

### 3.1 Penunjuk data induk dibuat `createOnly`, bukan sekadar dihilangkan dari form

`UpdateLabSusceptibilityBreakpointRequest` dan `UpdateLabProcedureMicrobiologyProfileRequest`
**nol memuat** `labOrganismId`, `labAntibioticId`, maupun `procedureId`. Pasangannya terkunci
sesudah baris dibuat.

Utils rujukan hanya mengenal `updateOnly`. Ia diberi `createOnly` sebagai kebalikannya, dan
ketiga tempat yang menyaring field — `buildInitialForm`, `validateForm`, `buildPayload` —
dialihkan ke satu penjaga `isFieldInMode`. Tanpa itu, form ubah akan memvalidasi ruas yang nol
dirender dan mengirim ruas yang backend nol punya tempat menerimanya.

### 3.2 `optionMap` benar-benar diisi — cacat yang standar larang ditiru

Standar bagian 8 memperingatkan: sebagian fitur mendeklarasikan `optionResource` tetapi hook
editor-nya mengembalikan `optionMap: config.optionMap || {}` yang **selalu kosong**, sehingga
select relasinya nol pernah terisi. Rujukan `job-level` memang berbentuk begitu.

Ketiga select relasi di sini disambungkan sungguhan:

| Select | Sumber |
|---|---|
| Kuman, Antibiotik | `useSelectResource("labOrganisms"/"labAntibiotics")` → `/options` |
| Pemeriksaan katalog | `useSelectResource("procedures")` → `/options` |

### 3.3 Dropdown dibaca dari `/options`, dan katalognya didaftarkan di registry bersama

`lab-microbiology-result-slice` sudah punya `fetchLabOrganismOptions`, tetapi **ia memanggil
`GET /` — daftar utama, bukan `/options`.** Standar melarang daftar utama dipakai mengisi
dropdown, dan memperbaikinya di tempat akan mengubah bentuk data yang dibaca layar hasil
Mikrobiologi (`FE-LAB-31`), yang membaca `organismName` dari baris penuh.

Keduanya karena itu **didaftarkan di `health-service-select-resources.js`** — satu-satunya
tempat repo menyimpan alamat `/options`. Selisih ini dicatat, bukan ditambal diam-diam:
**`fetchLabOrganismOptions` masih memanggil daftar utama**, dan memperbaikinya milik task yang
menyentuh `FE-LAB-31`.

### 3.4 Pemisah nomor cetak yang SENGAJA kosong dijaga di tiga tempat

`reportNumberSeparator` Patologi Klinik adalah **teks kosong**, dan itu pilihan sadar
(`r29` bagian 24.2): `null` berarti "belum pernah disetel" dan jatuh ke `-`.

`getByKeys` memperlakukan string kosong sebagai "tidak ada" lalu jatuh ke fallback — benar bagi
ruas lain, **salah di sini**. `buildInitialForm` karena itu membacanya langsung, dan
`buildPayload` mengirimnya sebagai teks kosong, bukan `null`. Keduanya diuji.

Tanpa ini, membuka lalu menyimpan pengaturan Patologi Klinik akan diam-diam memasang tanda
hubung pada nomor cetaknya — dan `26000001` berubah menjadi `26-000001` tanpa satu pun galat
yang terlihat.

---

## 4. Verifikasi

| Yang diuji | Hasil | Klasifikasi |
|---|---|---|
| `npx eslint` atas seluruh berkas baru dan berubah | **0 error, 4 warning** | `PASS` — keempat warning `set-state-in-effect`, pola yang sama persis dengan rujukan `job-level` (yang sendirinya memuat 3) |
| `npm run build` | **`✓ Compiled successfully`** | `PASS` |
| Sepuluh route baru muncul di keluaran build | Kesepuluhnya | `PASS` |
| Uji unit baru | **20/20 lulus** | `PASS` |
| Seluruh suite | 1556/1562 | `PASS` — **6 kegagalan sama persis** dengan sebelum perubahan; nol bertambah |
| Delapan route dipanggil pada dev server | `200` | `PASS` — delapan dari sepuluh; dua route `[slug]` detail nol diuji sebab menuntut token sesi peramban |
| Slug disiplin asing (`radiologi`) | Menyentuh batas `notFound()` | `PASS` — penanda `404` muncul HANYA pada slug asing |

### 4.1 Hak akses — diuji pada akun sungguhan, bukan superadmin

Dijalankan sebagai **dr. Bima Prasetya (`Kepala Instalasi Laboratorium`)**:

| Yang diuji | Hasil | Artinya |
|---|---|---|
| Keenam endpoint baca baru | **`200`** | Kedelapan endpoint `r30` **mewarisi kebijakan akses yang sudah ada** — hasil langsung dari keputusan `BE-LAB-64` untuk nol membuat nama aksi baru |
| `GET /lab-discipline-settings` | **`200`** | — |
| `PATCH` status **breakpoint** | **`403`** | **Pemisahan wewenang tegak.** Breakpoint dipegang wewenang klinis Mikrobiologi (`DR-LAB-002`), **bukan** kepala instalasi |
| `PATCH` status **profil** | **`200`** | Profil katalog memang dipegang kepala instalasi |
| `PUT` pengaturan disiplin | **`200`** | Sama |
| Bentuk nomor ketiga disiplin sesudahnya | `26000001`, `26.0001`, `26-0001` | Nol satu pun bergeser |

> **`403` pada breakpoint itu hasil yang BENAR, bukan kegagalan.** Roadmap menandainya sebagai
> risiko utama task ini: *"Layar breakpoint dipegang wewenang klinis, bukan kepala instalasi —
> pemisahan itu harus terlihat pada menu, bukan hanya ditegakkan server."* Sisi servernya kini
> **terbukti**; sisi menunya belum.

**`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`**
(1556 lulus; 6 kegagalan lama, nol bertambah).

**`MANUAL TEST: PARTIAL`.** Hak akses dan route terbukti lewat HTTP terhadap backend sungguhan.
Yang **belum** diklik satu pun: pemilih kuman dan antibiotik, kotak centang set bakteri, sakelar
status per baris, tombol simpan pada ketiga form, dan tabel pada ketiga layar. Sesi ini nol
punya alat kendali peramban.

---

## 5. Acceptance criteria — dibaca apa adanya

| AC | Status | Bukti |
|---|---|---|
| `AC-182` mengubah nama konsultan mengubah footer dan **tidak** memindahkan wewenang klinis | ✅ **Terbukti pada layar untuk sisi layar** (B8) — sisi *footer* milik cetakan (`LAB-OPEN-039`) | Payload `PUT` diuji memuat **tepat tujuh ruas** tampilan dan bentuk nomor — nol penunjuk dokter, nol jabatan, nol hak akses |
| `AC-185` mengubah breakpoint **tidak** mengubah hasil lama | ✅ **Sisi layar terbukti** (W4; `FE-LAB-31` T5): pasangan terkunci, layar hasil menampilkan **snapshot baris** `13-17`, bukan data induk. Ketetapan hasil lama tetap milik backend (`BE-LAB-61`/`BE-LAB-63` ✅) | Pasangan kuman/antibiotik terkunci sesudah dibuat dan nol terkirim saat perbarui (diuji). **Bahwa hasil lama nol berubah adalah perilaku backend** dan nol dapat dibuktikan dari layar |
| `AC-189` profil katalog menentukan bentuk layar hasil | ✅ **Terbukti di layar** (`FE-LAB-31` T14) | `usesSusceptibilitySet` tersimpan dan terbaca; akibatnya pada layar hasil belum diklik |
| DoD: pengaturan disiplin **nol punya tombol tambah maupun hapus** | **Terbukti secara struktural** | Nol `POST`, nol `DELETE` di slice; nol tombolnya di view |

> Perbedaan **"terbukti pada aturannya"** dan **"terbukti"** ditulis dengan sengaja. Aturan
> murninya diuji dan lulus; yang belum diverifikasi adalah bahwa komponennya memanggil aturan
> itu pada keadaan yang benar, dan hasilnya terlihat sebagaimana mestinya di layar.

---

## 6. Yang perlu dikerjakan sebelum task ini boleh ditandai `SELESAI` penuh

1. ~~**Verifikasi klik menyeluruh** pada peramban, memakai akun kepala instalasi dan akun
   wewenang klinis — keduanya, sebab pemisahannya justru intinya.~~ **Selesai 2026-10-02 dan
   2026-10-06** (bagian 7). Wewenang klinis diwakili superadmin — akunnya belum ada di dev.
2. ~~**Layar breakpoint wajib diperiksa dari akun yang NOL memegang wewenang klinis**.~~
   **Selesai 2026-10-02** (B3): `403` asli tampil sebagai pesan terbaca.
3. **Data induknya masih hampir kosong** — satu organisme, satu antibiotik, satu profil. Bukan
   penahan status: pekerjaan data kepala instalasi (`B3`), bukan kode.

**Nol operasi git dijalankan.**

Satu backend (`https://localhost:7184` dan `http://localhost:5107`) dan satu dev server
frontend (`http://localhost:3000`) **dibiarkan hidup** supaya verifikasi klik dapat langsung
dikerjakan.

---

## 7. Verifikasi susulan di peramban — 2026-10-02 dan 2026-10-06

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir**. **Seluruh
tulis dicegat** dan dijawab tiruan, **kecuali satu**: `PUT` breakpoint oleh dr. Bima diteruskan dengan
sengaja, sebab ditolak `403` oleh filter izin **sebelum** menulis — itulah yang diuji B3.

### 7.1 Hasil

| ID | Akun | Skenario | Hasil | Bukti |
| --- | --- | --- | --- | --- |
| B1 | dr. Bima (breakpoint `Read` saja) | Daftar breakpoint termuat; **Tambah tidak ditawarkan** | **`FAIL` → `PASS`** | Lihat 7.2 |
| B2 | dr. Bima | Detail breakpoint: nol `Perbarui`/`Hapus` | **`FAIL` → `PASS`** | Semula `["Kembali","Perbarui","Hapus"]`, kini `["Kembali"]` |
| B3 | dr. Bima | Halaman ubah dibuka langsung lalu disimpan → `403` asli tampil sebagai **pesan terbaca**, bukan galat mentah | `PASS` | "Anda tidak memiliki akses ke menu atau fitur ini." |
| B4 | dr. Bima (profil CRUD) | Daftar profil Mikrobiologi termuat; Tambah ditawarkan | `PASS` | — |
| B5 | dr. Bima | Tambah profil: kosong ditolak di layar tanpa permintaan; pemilih pemeriksaan dan kuman/antibiotik tersedia | `PASS` | 3 pemilih |
| B6 | dr. Bima | Keputusan 3.1 — ubah profil: badan `PUT` **tanpa** `procedureId` | `PASS` | Dicegat |
| B7 | dr. Bima | Pengaturan disiplin: nol Tambah/Hapus; ketiadaannya dinyatakan | `PASS` | 3 baris |
| B8 | dr. Bima | Keputusan 3.4 — pemisah nomor cetak yang sengaja kosong terkirim `""`, bukan `null`; badan nol memuat ruas wewenang klinis (`AC-182`) | `PASS` | Dicegat |
| B9 ×3 | dr. Bima | 390 px: ketiga layar tanpa gulir horizontal halaman | `PASS` | `375 ≤ 390` |
| B10 ×3 | Vina (`Read` saja) | Ketiga daftar terbaca; nol Tambah | **`FAIL` → `PASS`** (breakpoint, profil) | Lihat 7.2 |
| W1 | superadmin (pengganti wewenang klinis) | Daftar breakpoint; Tambah ditawarkan | `PASS` | 2026-10-06 |
| W2 | superadmin | Tambah breakpoint: pemilih kuman dan antibiotik (nol ketikan bebas) | `PASS` | — |
| W3 | superadmin | Detail: `Perbarui` dan `Hapus` ditawarkan | `PASS` | — |
| W4 | superadmin | `AC-185` ubah: pasangan kuman/antibiotik **tidak dirender dan tidak terkirim** | `PASS` | Badan `{lowerMm, upperMm, guidelineVersion, isActive}` (dicegat) |
| W5 | superadmin | Hapus berkonfirmasi lalu `DELETE` | `PASS` | Dicegat — nol data berubah |
| W6 | — | Nol tulis diteruskan pada sesi 2026-10-06 | `PASS` | — |

### 7.2 Satu cacat yang ditemukan uji, dan perbaikannya

**Gejala.** Layar breakpoint dan profil menawarkan `Tambah`, `Perbarui`, dan `Hapus` kepada **setiap**
pengguna, termasuk pemegang `Read` saja — padahal risiko utama task ini menurut roadmap adalah
*"pemisahan itu harus terlihat pada menu, bukan hanya ditegakkan server"*. Server menolak (`403`), tetapi
layar mengundang tindakan yang pasti gagal.

| Berkas frontend | Perubahan |
| --- | --- |
| `use-master-data-lab-susceptibility-breakpoint.jsx`, `use-master-data-lab-procedure-microbiology-profile.jsx` | `canCreate` dari `usePermission(resource, "Create")` |
| `…-breakpoint-detail.jsx`, `…-microbiology-profile-detail.jsx` (hook) | `canUpdate`, `canDelete` dari `usePermission` |
| `master-data-lab-susceptibility-breakpoint-view.jsx`, `master-data-lab-procedure-microbiology-profile-view.jsx` | `+ Tambah` hanya bila `canCreate` |
| `lab-susceptibility-breakpoint-detail-view.jsx`, `lab-procedure-microbiology-profile-detail-view.jsx` | `Perbarui` hanya bila `canUpdate`; `Hapus` hanya bila `canDelete` |

Pola yang sama dengan layar data induk Laboratorium lain (Organisme). Penegakan tetap di backend.
Perbaikan ini ter-commit pada `d05fb95e0`.

**Validasi akhir (2026-10-06):** uji unit 2308/2314 (6 gagal = baseline, nol Laboratorium),
`lint:errors` 0 error, `npm run build` hijau. **Nol operasi Git dijalankan.**
