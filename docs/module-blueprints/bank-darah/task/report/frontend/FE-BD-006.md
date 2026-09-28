# Laporan Perubahan Frontend — `FE-BD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-006` |
| Judul | Seluruh layar Bank Darah terjangkau dari menu |
| Slice | Roadmap frontend Bank Darah — Slice 1, registrasi menu modul |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §3 kartu `FE-BD-006` |
| Trace | `03-frontend-architecture.md` §2 (peta butir menu dan butir hak akses) · `BD-UI-GAP-002` · `GET /api/v1/Auth/permissions` → `EffectivePermissionSet` |
| Contract version | `v4` — ✅ **`approved`** (`Sukmagp`, 2026-09-03) |
| Wewenang UI | Rupa layar `DEV_DISCRETION`. Yang dikunci: susunan menu, induk tiap butir, pemetaannya ke layar |
| Keputusan pemilik | **18 September 2026 — `Sukmagp` menyetujui paket A pada keputusan #2–#6**, perluasan scope di luar `menu-items.jsx`, integrasi sidebar bersama, perluasan slice `authPermission`, dan kebijakan gagal-tertutup untuk butir menu bergerbang |
| Dependency | `G1` ✅ — nol dependency backend. Sumber hak akses backend sudah ada di `77f60c88` |
| Klasifikasi | `MEDIUM` — 4 berkas source diubah, 1 berkas source baru, 2 berkas test baru; menyentuh sidebar bersama dan slice kewenangan lintas modul |
| Task mode | `FRONTEND` — backend strict read-only (kecuali laporan dan bukti dokumentasi Bank Darah) |
| Target tulis | `V2QuilvianSystemFrontendDev` (source dan test) + `docs/module-blueprints/bank-darah/` pada backend (dokumentasi saja) |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | Titik awal `e24c9e4c53f64e8c8972d8fd317355099c065695` cabang `sukmagpV2` |
| Commit implementasi frontend | **`fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e`** (`feat(bank-darah): enforce permission-aware setup menu`) cabang `sukmagpV2`, ter-push ke `origin/sukmagpV2` — lihat §9. **Riwayat:** perubahan belum di-commit di atas `e24c9e4c5` sampai uji runtime selesai |
| Commit backend yang dijadikan rujukan | `f577a530` cabang `sukmagp` (source otorisasi tidak berubah sejak `77f60c88`) |
| Tanggal | `2026-09-18` (pengerjaan ulang); laporan pertama `2026-09-10` — lihat Lampiran A |
| Status | ✅ **SELESAI 18 September 2026 — 2 dari 2 acceptance terbukti.** Validasi otomatis lulus dan uji runtime pemilik `Sukmagp` R1–R8 seluruhnya `PASS` (§6.3). Nol perubahan source sesudah uji runtime. Source ter-commit sebagai `fbe29f6d1` dan ter-push ke `origin/sukmagpV2` (§9). **Riwayat:** source belum di-commit saat penutupan; 🟡 SELESAI SEBAGIAN 18 September 2026 — kedua acceptance terimplementasi dan terbukti otomatis, bukti runtime belum ada; 🟡 SELESAI SEBAGIAN 10 September 2026 — 1 dari 2 (Lampiran A) |

---

## 0. Ringkasan untuk pembaca umum

**Sebelum:** ketiga butir menu Bank Darah → Setup tampil bagi siapa pun yang login. Petugas yang
tidak berhak baru tahu ketika membuka layarnya dan ditolak.

**Sesudah:** setiap butir hanya tampil bila daftar kewenangan dari backend untuk **pengguna yang
sedang login** memuat pasangan yang tepat. Bila daftarnya belum datang, gagal diambil, atau
bentuknya rusak, butirnya **disembunyikan**. Bila ketiga butir tersembunyi, grup Setup dan induk
Bank Darah ikut hilang. Menu lain tidak berubah sama sekali.

Ini **tampilan saja**. Membuka URL layar secara langsung tetap dijaga backend (`403`) dan
`AccessDeniedGate`, persis seperti sebelumnya.

---

## 1. Keadaan yang ditemukan di awal (18 September 2026)

### 1.1 Koreksi temuan lama — pembaca kewenangan frontend **sudah ada**

Laporan 10 September 2026 (Lampiran A §1.2) dan dokumen sinkronisasi roadmap 18 September 2026
menyatakan frontend **tidak punya** pembaca kewenangan pengguna berjalan. Untuk source saat ini
pernyataan itu **tidak benar**:

| Fakta | Bukti |
| --- | --- |
| Pembacanya dibuat di commit `622a46f41` | 8 September 2026, penulis `Rivenjxv`, pesan "updates FE modul lab": `permission-slice.jsx` (166 baris), `use-permission.jsx` (63 baris), registrasi `authPermission` di `store.jsx`, dan `tests/unit/auth-permission-slice.test.mjs` |
| Ia memanggil endpoint backend yang benar | `GET /v1/auth/permissions` lewat `InstanceAxios` |
| Pemakai pertamanya layar Lab | `use-master-data-lab-rejection-reason.jsx` — `usePermission("LabRejectionReason", "SystemFlag")` |
| Kapan ia masuk cabang Bank Darah | `622a46f41` **bukan** leluhur `6640a5e7` maupun `f79af1684`/`b98f5bdc9`. Ia masuk `sukmagpV2` lewat merge `d7059b563` (18 September 2026, merge `QuilvianDevV2` ke `sukmagpV2`) |
| Mengapa tidak ditemukan | Sinkronisasi roadmap memeriksa `6640a5e7` — sebelum merge itu — dan catatan SHA sesudahnya tidak memeriksa ulang berkas kewenangan yang ikut masuk lewat merge |

Pembaca itu **tidak** dipakai `FE-BD-006` sampai pengerjaan ulang ini. Yang ada padanya dan tidak
cocok untuk visibilitas menu:

| Sifat pembaca lama | Akibatnya bila dipakai untuk menu |
| --- | --- |
| `selectHasPermission` menjawab **`true`** selama daftar belum termuat atau gagal | Menu akan **terbuka** saat permintaan gagal — kebalikan dari gagal-tertutup |
| `isSuperAdmin: true` langsung menjawab `true` | Tanda boolean menggantikan daftar pasangan dari backend |
| Hanya `auth/clearAuth` yang mengosongkan daftar | Logout lewat `LogoutUser` dan login baru **tidak** mengosongkannya |
| Tidak ada penolak permintaan ganda | Setiap konsumen dapat meminta ulang; tidak ada cap pemilik sesi |

Sifat longgar itu **disengaja** untuk tombol Lab dan dijaga test `auth-permission-slice.test.mjs`
S1–S7, sehingga sesuai keputusan pemilik #3 **tidak diubah**. Yang ditambahkan adalah keputusan
ketat terpisah.

### 1.2 Yang lain tetap seperti laporan pertama

`filterMenuItemsByRole` masih stub berisi nama peran (`Admin`, `Manajer`). Sesuai keputusan #6 ia
**bukan** sumber otorisasi dan tidak disentuh. `AccessDeniedGate` tetap reaktif dan tidak diubah.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Alur normal

1. Petugas login (kata sandi atau sidik jari).
2. Sidebar meminta daftar kewenangan efektif **sekali** untuk sesi itu.
3. Selama menunggu, butir Bank Darah → Setup **belum** tampil; menu lain langsung tampil.
4. Daftar tiba. Untuk setiap butir Setup, sidebar memeriksa pasangan yang dipetakan:

| Butir menu | Pasangan yang harus ada |
| --- | --- |
| Katalog Komponen Darah | `BloodComponent : Read` |
| Daftar Alasan Terkendali | `BloodBankReason : Read` |
| Lokasi Penyimpanan Darah | `BloodStorageLocation : Read` |

5. Butir yang pasangannya ada ditampilkan. Contoh: petugas yang hanya memegang
   `BloodStorageLocation : Read` hanya melihat **Lokasi Penyimpanan Darah** di bawah Setup.

### 2.2 Jalur tidak normal

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Tidak memegang satu pun dari tiga pasangan | Grup **Bank Darah** tidak ada di sidebar |
| Daftar gagal diambil (jaringan, `401`, `500`) | Bank Darah tersembunyi; menu lain normal; **tidak ada** pesan error mentah di sidebar. Dicoba lagi pada pemasangan sidebar berikutnya (muat ulang halaman) atau sesi baru |
| Jawaban backend rusak bentuknya | Bank Darah tersembunyi |
| Akun SuperAdmin | Tampil **karena** backend memasukkan pasangannya ke daftar — bukan karena tanda `isSuperAdmin` |
| Logout lalu pengguna lain login di tab yang sama | Daftar pengguna pertama dibuang; pengguna kedua menunggu daftarnya sendiri |
| Membuka URL layar langsung tanpa hak | Backend menolak `403`, `AccessDeniedGate` menampilkan pesan — tidak berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` frontend · `rules/frontend/frontend-architecture.md` · `rules/frontend/test-policy.md` · `rules/frontend/REPORT_TEMPLATE.md` (suite `1.19.1`) |
| Pembaca kewenangan yang sudah ada | `src/lib/state/slice/auth/permission-slice.jsx` · `src/lib/hooks/auth/use-permission.jsx` · `tests/unit/auth-permission-slice.test.mjs` · pemakai Lab `use-master-data-lab-rejection-reason.jsx` · `store.jsx` |
| Daur hidup sesi | `src/lib/state/slice/auth/login-slice.jsx` (thunk `auth/login`, `auth/refresh`, `auth/checkStatus`, `auth/logout`; reducer `setAuthSession`, `clearAuth`) beserta seluruh pemanggilnya |
| Sidebar | `left-sidebar-items-virtualized.jsx` · `left-sidebar-menu-handle.jsx` · `filter-menu-items-by-role.jsx` · `menu-items.jsx` · `scroll-layout.jsx` (Redux `Provider` membungkus sidebar) |
| Backend (read-only) | `Controllers/AuthController.cs` `Permissions()` · `DTOs/Auth/EffectivePermissionResponse.cs` · `Services/Security/AccessPermissionService.cs` `GetEffectivePermissionsAsync` · `[AccessPermission("BloodComponent"/"BloodBankReason"/"BloodStorageLocation", "Read")]` — masing-masing 5 pemakaian |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/auth/permission-slice.jsx` | **Diperluas secara aditif.** `PERMISSION_DECISION`; `resolvePermissionDecision` / `selectPermissionDecision` (keputusan ketat); `selectPermissionOwnerKey`, `selectEffectivePermissionState`; state `ownerKey`, `requestId`, `malformed`; `condition` + `getPendingMeta` pada `fetchEffectivePermissions` (penolak permintaan ganda, cap pemilik); jawaban usang dibuang lewat `requestId`; pengosongan pada lima aksi sesi dan pada pergantian identitas `/auth/me`. `selectHasPermission` **tidak diubah** |
| `src/lib/hooks/auth/use-permission.jsx` | Menambah `useEffectivePermissions()` — keputusan ketat untuk banyak pasangan sekaligus. `usePermission` **tidak diubah** |
| `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` | **Baru.** Penyaring murni `filterMenuItemsByPermission(menuItems, isPermissionAllowed)` |
| `src/utils/menu-sidebar/menu-items.jsx` | `requiredPermission` pada tiga butir Setup Bank Darah (+7 baris termasuk komentar) |
| `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx` | Memanggil `useEffectivePermissions()` dan menerapkan penyaring **sesudah** `filterMenuItemsByRole` |
| `tests/unit/auth-permission-strict-decision.test.mjs` | **Baru.** 17 kasus |
| `tests/unit/menu-permission-filter.test.mjs` | **Baru.** 9 kasus |

Nol berkas backend source, nol migration, nol database. `AccessDeniedGate` dan
`filterMenuItemsByRole` tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya mengikuti `frontend-architecture.md`:

```text
left-sidebar-items-virtualized  (components/features)
  -> useEffectivePermissions      (lib/hooks/auth/use-permission.jsx — sudah ada, diperluas)
       -> permission-slice        (lib/state — sudah ada, diperluas; terdaftar sebagai authPermission)
            -> InstanceAxios      (tidak diubah)
  -> filterMenuItemsByPermission  (utils/menu-sidebar/permission — fungsi murni, tanpa Redux)
```

Nol Axios instance baru, nol provider/context baru, nol slice baru. Utility tidak mengimpor Redux:
keputusan dimasukkan sebagai fungsi `isPermissionAllowed` oleh sidebar.

#### Keputusan #2 — invalidasi sesi

Pengosongan terjadi di reducer `authPermission`, dicocokkan lewat nama aksi `login-slice` — tidak
ada `dispatch(clearEffectivePermissions())` yang disebar di komponen.

| Aksi | Alasan | Diminta pemilik |
| --- | --- | :---: |
| `auth/login/pending` | Login kata sandi dimulai | ✅ |
| `auth/setAuthSession` | Sesi dibentuk login sidik jari — satu-satunya pemanggil `use-base-login.jsx:992` | ✅ |
| `auth/logout/pending` | Logout dimulai | ✅ |
| `auth/clearAuth` | Sesi dihapus paksa (`session-activity-manager.jsx:177`, `use-base-login.jsx:1174`) | ✅ |
| `auth/logout/fulfilled` | **Tambahan, dengan bukti.** `isLoggedIn` baru turun di `LogoutUser.fulfilled` (`login-slice.jsx`). Di antara `pending` dan `fulfilled` sesi yang sedang ditutup masih dianggap login, sehingga konsumen dapat meminta daftar lagi; daftar itu dibuang di sini | tambahan |
| `auth/checkStatus/fulfilled`, `auth/refresh/fulfilled` — **hanya bila `userId` berbeda dari pemilik daftar** | **Tambahan, dengan bukti.** Keduanya membangun `userInfo` dari `GET /auth/me` tanpa melewati login (`auth-wrapper.jsx:61`, `session-activity-manager.jsx:264/322`); cookie sesi dapat sudah milik pengguna lain (login di tab lain). Pengguna yang sama **tidak** memicu pengosongan | tambahan |

Tidak ditambahkan: `auth/login/rejected`, `auth/checkStatus/rejected`, `auth/refresh/rejected`.
Ketiganya tidak dapat menghasilkan pemakaian lintas pengguna karena login berikutnya selalu melewati
`auth/login/pending` atau `auth/setAuthSession`, dan cap pemilik menolak daftar milik orang lain.

Pengaman lapis kedua: setiap daftar dicap `ownerKey` (`userId` saat permintaan dimulai). Keputusan
ketat menjawab `unknown` bila cap itu tidak sama dengan `userId` sesi saat ini — daftar pengguna
lama tidak pernah memuaskan sesi baru, bahkan bila tidak ada aksi reset sama sekali.

#### Keputusan #3 — keputusan ketat

| Keadaan | `selectPermissionDecision` | `selectHasPermission` (Lab, tidak berubah) |
| --- | --- | --- |
| Termuat, milik sesi ini, pasangan ada | `allowed` | `true` |
| Termuat, milik sesi ini, pasangan tidak ada | `denied` | `false` |
| Termuat, `isSuperAdmin: true`, pasangan tidak ada | **`denied`** | `true` |
| Memuat / belum termuat / gagal | `unknown` | `true` |
| Jawaban cacat | `unknown` | seperti sebelumnya |
| Milik pengguna lain | `unknown` | — |
| `Resource`/`Action` kosong | `denied` | — |

Pencocokan memakai `buildPermissionKey` yang sudah ada: pasangan persis setelah `trim`, tanpa
wildcard, tanpa pencocokan sebagian, dan tanpa beda huruf besar-kecil — sama dengan kunci yang
dipakai layar Lab. Nama peran tidak pernah dibaca.

Mengapa `isSuperAdmin` boleh diabaikan tanpa merugikan SuperAdmin: `GetEffectivePermissionsAsync`
memulangkan **seluruh** pasangan terdaftar untuk SuperAdmin tanpa penegakan kebijakan klinis, dan
hanya pasangan yang memang dipegangnya bila penegakan itu aktif.

Jawaban dianggap **cacat** bila bukan objek, `permissions` bukan array, atau ada satu butir tanpa
`resource`/`action` teks yang tidak kosong. Satu butir cacat menandai seluruh daftar.

#### Keputusan #4 — pengambilan, penolak ganda, percobaan ulang

| Kebutuhan | Cara | Bukti test |
| --- | --- | --- |
| Konsumen serentak tidak menggandakan `GET` | `condition` menolak bila permintaan untuk pemilik yang sama sedang berjalan | F1: 3 dispatch serentak → 1 panggilan |
| Daftar yang sudah termuat tidak diminta ulang | `condition` menolak bila termuat untuk pemilik yang sama | F1: dispatch keempat → tetap 1 |
| Permintaan gagal tetap dapat dicoba ulang | Gagal meninggalkan `loading` dan `loaded` salah; `condition` mengizinkan | F2 |
| Sesi baru mengambil daftar baru | Pemilik berbeda → `condition` mengizinkan, `pending` membuang daftar lama | F3, S4 |
| Jawaban pengguna lama tidak mendarat di sesi baru | `requestId` dikosongkan saat reset; jawaban dengan `requestId` lain diabaikan | S4 |
| Tidak ada perulangan tanpa batas | Efek `useEffectivePermissions` hanya berjalan ulang ketika `isAuthenticated`, `ownerKey`, atau status "termuat untuk pemilik ini" berubah. Gagal tidak mengubah ketiganya, sehingga tidak ada permintaan berikutnya sampai pemasangan ulang atau pergantian sesi | tinjauan kode |

Satu perbaikan kecil ikut masuk: sebelumnya `rejected` yang **dibatalkan** (`aborted`) keluar tanpa
menurunkan `loading`, sehingga dengan penolak ganda baru pengambilan akan terkunci. Kini `loading`
turun lebih dulu. Tidak ada pemanggil saat ini yang membatalkan permintaan ini.

#### Keputusan #5 dan #6 — metadata dan penyaring menu

`requiredPermission: { resource, action }` hanya pada tiga butir Setup; induk Bank Darah dan grup
Setup tidak dijaga sendiri. Penyaringnya:

- butir bergerbang tampil hanya bila `isPermissionAllowed(...) === true`;
- butir tanpa gerbang dipulangkan **dengan objek yang sama** (referensi tidak berubah);
- anak dikenali dari setiap properti berisi daftar butir ber-`key` (`subMenu`, `subItems`, dst.);
- grup yang seluruh anaknya hilang **karena penyaringan ini** ikut dibuang, merambat ke atas,
  kecuali grup itu sendiri punya `pathname`/`href`;
- masukan tidak dimutasi (dibuktikan dengan pohon beku).

### 3.4 Gerbang keputusan base component

| Elemen | Status | Keterangan |
| --- | --- | --- |
| Pembaca kewenangan (`permission-slice`, `usePermission`) | `EXTEND` | Aditif; perilaku default `selectHasPermission`/`usePermission` tidak berubah. **Disetujui pemilik** (#3, #4) |
| `useEffectivePermissions` | `EXTEND` | Ditambahkan di berkas hook yang sama. **Disetujui pemilik** (#4) |
| Penyaring menu | `NEW` (utility, bukan komponen tampilan) | **Disetujui pemilik** (#6) |
| Sidebar `VirtualizedSideBarItems` | `REUSE` | Rupa tidak berubah; hanya data menu yang tersaring |
| `AccessDeniedGate` | `REUSE` | Tidak diubah |

`UI GATE: PASSED — 1 NEW dan 2 EXTEND, ketiganya disetujui pemilik 18 September 2026; 0 menunggu keputusan.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Butir bergerbang belum tampil; menu tanpa gerbang tampil seperti biasa |
| Kosong | Tanpa satu pun pasangan Bank Darah → grup Bank Darah tidak ada |
| Gagal | Butir bergerbang tersembunyi, tanpa pesan error di sidebar. Pemulihan: muat ulang halaman atau login ulang |
| Tanpa hak akses | Butir tersembunyi. URL langsung → `403` backend dan `AccessDeniedGate`, tidak berubah |

---

## 5. Endpoint yang dikonsumsi

#### 01-Authentication

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/Auth/permissions` (frontend: `/v1/auth/permissions` lewat `InstanceAxios`) | Daftar kewenangan efektif sesi — `ApiResponse<EffectivePermissionSet>` berisi `isSuperAdmin`, `permissions[] { resource, action }`, `totalPermission` | `[Authorize]` — pengguna yang login |

Endpoint sama yang sudah dipakai layar Lab sejak `622a46f41`; tidak ada endpoint baru.

---

## 6. Verifikasi

### 6.1 Validasi otomatis

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Baseline sebelum perubahan: `node --import ./tests/helpers/register.mjs --test tests/unit/` | `tests 759, pass 759, fail 0` | `PASS` | Dijalankan sebelum edit |
| Test terarah: `auth-permission-strict-decision` + `menu-permission-filter` + `auth-permission-slice` | `tests 33, pass 33, fail 0` | `PASS` | 26 kasus baru + 7 kasus Lab lama tetap lulus |
| Suite unit penuh (bentuk Windows) | `tests 785, pass 785, fail 0` | `PASS` | 759 + 26 baru |
| `npm run test:unit` | Keluar `0`, **tetapi menjalankan nol test**: `Could not find 'tests\unit\**\*.test.mjs'` | `EXISTING / ENVIRONMENT ISSUE` | Node 20 di Windows tidak mengembangkan glob; karena itu dipakai bentuk direktori di atas. **Bukan** bukti lulus |
| `npm run lint:errors` | Keluar `0`, nol error | `PASS` | |
| `npx eslint` atas 7 berkas yang disentuh | Keluar `0`, nol error, nol warning | `PASS` | |
| `npm run lint` | `0 errors, 679 warnings` — keluar `0` | `PASS` | Nol warning dari berkas task ini |
| `npm run build` | `✓ Compiled successfully in 63s`; `postbuild` standalone berhasil; keluar `0` | `PASS` | Route `/health-services/master-data/blood-components`, `/blood-bank-reasons`, `/blood-storage-locations` ada di keluaran |
| `git diff --check` | Keluar `0` | `PASS` | |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS (785/785)`

### 6.2 Test terarah yang ditambahkan

| Kebutuhan pemilik | Test |
| --- | --- |
| Keputusan ketat: memuat / belum termuat / allowed / denied / gagal | D2 / D1 / D3 / D4 / D5 |
| Jawaban cacat (9 bentuk) | D6 |
| `isSuperAdmin` tanpa pasangan tidak memberi akses; semantik Lab tetap | D7 |
| `Resource`/`Action` kosong atau tidak dikenal | D8 |
| Daftar pengguna lain tidak memuaskan sesi ini | D9 |
| Logout mengosongkan (`pending`, `fulfilled`, `clearAuth`) | S1 |
| Login/sesi baru mengosongkan (`login/pending`, `setAuthSession`) | S2 |
| Identitas `/auth/me` berbeda mengosongkan; sama tidak | S3 |
| User A → logout → User B, jawaban A terlambat dibuang | S4 |
| Penolak permintaan ganda serentak | F1 |
| Percobaan ulang sesudah gagal | F2 |
| Sesi baru mengambil daftar baru | F3 |
| Pemetaan kanonis tiga butir dibaca dari `menu-items.jsx` sungguhan | M0 |
| Akses penuh → 3 butir | M1 |
| Akses sebagian → hanya butir berhak | M2 |
| Tanpa akses → Setup dan induk Bank Darah hilang; menu lain tetap | M3 |
| Memuat / gagal / belum termuat / pemutus bukan `true` → tersembunyi | M4 |
| Butir tanpa gerbang tidak berubah (referensi sama) | M5 |
| Pohon masukan tidak dimutasi | M6 |
| `requiredPermission` tidak utuh → tersembunyi | M7 |
| Grup berhalaman tidak dibuang | M8 |

### 6.3 Uji manual — `PASS` (uji runtime pemilik, 18 September 2026)

**Hasil uji runtime pemilik `Sukmagp`, 18 September 2026 — 8 dari 8 `PASS`.** Dijalankan pemilik
pada aplikasi berjalan dengan akun uji berbeda kewenangan; dicatat di sini persis seperti
dilaporkan pemilik. Tidak ada hasil yang bertentangan dengan implementasi, sehingga source frontend
**tidak** diubah sesudahnya.

| No | Skenario | Hasil | Yang teramati pemilik |
| ---: | --- | :---: | --- |
| R1 | Akses penuh | `PASS` | Ketiga butir Setup Bank Darah tampil; `GET /Auth/permissions` hanya **satu** untuk sesi |
| R2 | Akses sebagian | `PASS` | Hanya butir yang kewenangan `Read`-nya dimiliki yang tampil |
| R3 | Tanpa akses | `PASS` | Grup Bank Darah tidak tampil; menu lain tetap normal |
| R4 | Ganti pengguna di tab yang sama | `PASS` | Kewenangan User A tidak terbawa ke User C; `GET /Auth/permissions` baru terjadi sesudah login User C |
| R5 | Endpoint kewenangan gagal | `PASS` | Bank Darah tersembunyi (gagal-tertutup); menu lain normal; tidak ada error atau exception mentah di sidebar |
| R6 | URL langsung tanpa hak | `PASS` | Backend tetap menolak; `AccessDeniedGate` tetap bekerja |
| R7 | SuperAdmin | `PASS` | Visibilitas mengikuti `Resource`/`Action` yang dipulangkan endpoint; boolean `isSuperAdmin` sendiri tidak dipakai sebagai jalan pintas |
| R8 | Regresi layar Lab | `PASS` | Perilaku `SystemFlag` yang sudah ada tidak berubah |

`MANUAL TEST: PASS — 8/8, dijalankan pemilik Sukmagp 18 September 2026.`

**Riwayat — sebelum uji pemilik:** uji manual `NOT FEASIBLE` pada sesi implementasi, karena
menyaksikan sidebar menuntut frontend dan backend berjalan, database dengan kebijakan akses, dan
beberapa akun dengan kombinasi kewenangan berbeda; kredensial akun uji tidak tersedia dan tidak
boleh dikarang. Skenario yang waktu itu diserahkan kepada pemilik (DevTools → Network terbuka,
filter `permissions`):

| No | Persiapan | Langkah | Hasil yang diharapkan |
| ---: | --- | --- | --- |
| R1 | Akun A memegang ketiga `Blood* : Read` | Login, buka sidebar | Bank Darah → Setup memuat 3 butir. Tepat **satu** `GET /api/v1/Auth/permissions` sesudah login |
| R2 | Akun B hanya memegang `BloodStorageLocation : Read` | Login | Setup hanya memuat **Lokasi Penyimpanan Darah** |
| R3 | Akun C tanpa ketiga pasangan | Login | Tidak ada grup **Bank Darah**; menu lain sama seperti sebelum task ini |
| R4 | Akun A lalu akun C, tab yang sama, tanpa muat ulang manual | Login A → lihat 3 butir → Logout → Login C | Bank Darah **tidak** tampil untuk C; ada `GET /Auth/permissions` baru sesudah login C |
| R5 | Akun A | DevTools → *Block request URL* `*/Auth/permissions` → muat ulang | Bank Darah tersembunyi, menu lain normal, **tidak ada** teks error mentah. Lepas blokir → muat ulang → 3 butir kembali |
| R6 | Akun C | Buka `/health-services/master-data/blood-components` langsung | Backend `403`, `AccessDeniedGate` tampil — tidak berubah |
| R7 | Akun SuperAdmin | Login, periksa jawaban `permissions` | Butir tampil hanya bila pasangannya tercantum di jawaban |
| R8 | Akun yang memakai layar Alasan Penolakan Lab | Buka layar itu | Tombol `SystemFlag` berperilaku sama seperti sebelumnya |

**Tidak dijalankan:** `npm run test:e2e` (butuh aplikasi berjalan), `npm run test:uat` (tidak diminta).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Butir menu mengarah ke layar — bagian *mengarah ke layar* | ✅ **Terpenuhi** (sejak 10 September 2026) | Ketiga route ada di keluaran build 18 September 2026; susunan cocok `03-frontend-architecture.md` §2 |
| Butir menu **hanya tampil bagi yang berhak** | ✅ **Terpenuhi** (18 September 2026) | Otomatis: M1–M4 dengan pemetaan dari `menu-items.jsx` sungguhan, D1–D9, S1–S4, F1–F3; build dan lint `PASS`. Runtime: R1–R8 `PASS` oleh pemilik `Sukmagp` (§6.3). **Riwayat:** 🟡 terimplementasi dan terbukti otomatis, bukti runtime belum |

| Butir DoD | Status |
| --- | --- |
| Registrasi menu menjadi acceptance task layar | ✅ Tidak berubah |
| Butir menu yang menunjuk layar belum ada disembunyikan | ✅ Tidak berubah — tujuh layar belum dibangun tetap tanpa butir |
| Susunan menu cocok kontrak §2 | ✅ Tidak berubah |
| Validasi otomatis | ✅ unit 785/785, `lint:errors` 0, build `PASS`, `git diff --check` bersih |
| Bukti runtime | ✅ R1–R8 `PASS`, pemilik `Sukmagp` 18 September 2026 — §6.3. **Riwayat:** ⛔ belum |

**Kedua acceptance terpenuhi, validasi otomatis lulus, dan bukti runtime lengkap. Karena itu task ini ✅ SELESAI.**

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan keamanan | Lihat §8.1 |
| Masalah yang diketahui | (1) Sesudah permintaan gagal, butir tetap tersembunyi sampai sidebar dipasang ulang (muat ulang) atau sesi berganti — disengaja agar tidak ada perulangan permintaan. (2) `npm run test:unit` di Windows menjalankan nol test karena glob tidak dikembangkan — `EXISTING / ENVIRONMENT ISSUE`, di luar scope. (3) `filterMenuItemsByRole` tetap stub bernama peran — di luar scope sesuai keputusan #6. (4) Duplikat `doctor-queues` milik modul Registration tetap ada |
| `BD-UI-GAP-002` | ✅ **DITUTUP PENUH 18 September 2026.** Pembacanya sudah ada sejak `622a46f41`; task ini menambah keputusan ketat, invalidasi sesi, dan penolak ganda, lalu seluruhnya terbukti runtime (R1–R8). `FE-BD-009` (`FE-BD-019`) dan `FE-BD-007` (`FE-BD-020`) dapat memakai ulang `useEffectivePermissions` / `selectPermissionDecision`; memakainya tetap pekerjaan kedua task itu. **Riwayat:** tertutup pada tingkat implementasi, bukti runtime menunggu §6.3 |
| Dependency backend | `NOT APPLICABLE` — endpoint sudah ada; backend tidak diubah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git frontend | **Ter-commit dan ter-push:** `fbe29f6d1` di `sukmagpV2` = `origin/sukmagpV2`, working tree bersih (§9). **Riwayat:** `M` `left-sidebar-items-virtualized.jsx`, `use-permission.jsx`, `permission-slice.jsx`, `menu-items.jsx`; `??` `src/utils/menu-sidebar/permission/`, dua berkas test baru — nol stage, commit, push oleh agent |
| Langkah berikutnya | Task frontend kanonis berikutnya: **`FE-BD-002`** — order darah, pemenuhan, dan pembatalan. **Riwayat:** commit source frontend dan dokumen ini atas perintah pemilik — source sudah ter-commit `fbe29f6d1`; sebelumnya: pemilik menjalankan R1–R8 — sudah dilakukan 18 September 2026, seluruhnya `PASS` |

### 8.1 Bukti keamanan

| No | Tuntutan pemilik | Bukti |
| ---: | --- | --- |
| 1 | Gagal ambil → butir bergerbang tertutup | D5 (`unknown`), M4 (menu kosong untuk memuat/gagal/belum termuat); runtime R5 `PASS` |
| 2 | Kewenangan User A tidak bertahan untuk User B | S1, S2, S4 (jawaban A yang terlambat dibuang), D9 (cap pemilik); runtime R4 `PASS` |
| 3 | `isSuperAdmin` saja tidak membuka menu | D7 — `denied` untuk ketat, sementara semantik Lab tetap; runtime R7 dan R8 `PASS` |
| 4 | Jawaban cacat tidak memberi akses | D6 — 9 bentuk cacat semuanya `unknown` |
| 5 | Resource/Action tidak dikenal tidak memberi akses | D8, M2 (action lain pada resource sama), M7 |
| 6 | Tidak ada pencatatan token/kredensial | `grep` `console.`/`token`/`password`/`localStorage` pada tiga berkas source yang disentuh dan tambahan sidebar: nihil. Tidak ada log baru |
| 7 | Tidak ada exception backend mentah yang dirender | Sidebar tidak merender `error` slice sama sekali; `readServerFailure` lama tidak berubah dan tidak dipakai jalur menu; runtime R5 `PASS` |
| 8 | Cache kewenangan dikosongkan pada transisi identitas | S1–S3 + tabel invalidasi §3.3; runtime R4 `PASS` |

---

## 9. Jangkar bukti Git (18 September 2026)

| Field | Nilai |
| --- | --- |
| Repository | `DevBenari/QuilvianSystemFrontendDev` |
| Cabang | `sukmagpV2` |
| Titik awal `FE-BD-006` | `e24c9e4c53f64e8c8972d8fd317355099c065695` (`feat(bank-darah): close FE-BD-011 storage deactivation flow`) |
| Commit implementasi final | **`fbe29f6d1b7408b13f4377b1fe4b77fc4dabf82e`** — `feat(bank-darah): enforce permission-aware setup menu`, `Sukma Giri Pratama`, 2026-09-18 21:34:22 +0700 |
| Remote | `origin/sukmagpV2` — ref lokal `origin/sukmagpV2` sama dengan `HEAD` `fbe29f6d1`; push dinyatakan pemilik |
| Isi commit | Satu-satunya commit di atas `e24c9e4c5`; 7 berkas, +1030 / −8 — persis berkas §3.2: `left-sidebar-items-virtualized.jsx`, `use-permission.jsx`, `permission-slice.jsx`, `menu-items.jsx`, `filter-menu-items-by-permission.jsx`, `auth-permission-strict-decision.test.mjs`, `menu-permission-filter.test.mjs` |
| Working tree frontend | Bersih sesudah commit |

Commit ini memuat source yang sama yang divalidasi di §6.1 (33/33 terarah, 785/785 penuh, `lint:errors`
0, `npm run lint` `0 errors, 679 warnings`, build `PASS`) dan diuji pemilik di §6.3 (R1–R8 `PASS`).
Pass penjangkaran ini dokumentasi saja: test dan build **tidak** dijalankan ulang.

---

## Lampiran A — Laporan 10 September 2026 (riwayat, isi asli tidak diubah)

> **Koreksi 18 September 2026.** Pernyataan pada A §1.2 bahwa frontend "tidak memiliki katalog
> permission" dan "slice hak akses pengguna berjalan: Nihil" **tidak berlaku** untuk source saat ini.
> Pembaca `permission-slice.jsx` / `use-permission.jsx` dibuat di `622a46f41` (8 September 2026) pada
> garis `QuilvianDevV2`, dan masuk `sukmagpV2` lewat merge `d7059b563` (18 September 2026). Pada SHA
> yang diperiksa laporan ini (`f79af1684`) berkas itu memang belum ada di cabang ini. Pembaca itu tidak
> ditemukan saat sinkronisasi roadmap 18 September 2026, dan baru dipakai `FE-BD-006` pada pengerjaan
> ulang di atas. Isi di bawah dipertahankan apa adanya sebagai riwayat.

### A Metadata

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

### A 1. Keadaan yang ditemukan di awal

Kartu task menyebut scope `menu-items.jsx` dan outcome dua bagian: **(a)** setiap layar Bank Darah
dapat dicapai dari menu, dan **(b)** butir menu hanya tampil bagi peran yang berhak.

#### A 1.1 Bagian (a) ternyata sudah hampir tuntas — tetapi menyimpang dari kontrak

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

#### A 1.2 Bagian (b) tidak punya mekanisme sama sekali

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

### A 2. Proses bisnis dari sisi pengguna

**Penggunanya** setiap petugas yang membuka aplikasi dan mencari layar Bank Darah dari sidebar.

#### A 2.1 Alur normal

1. Petugas membuka sidebar dan menemukan grup **Bank Darah**.
2. Di dalamnya ada satu sub-grup **Setup**.
3. Sub-grup itu memuat tiga butir yang mengarah ke tiga layar yang benar-benar ada.
4. Mengkliknya membuka layar yang dimaksud.

#### A 2.2 Yang berubah bagi pengguna

**Sebelum:** ketiga layar muncul dua kali di sidebar — di Master Data generik dan di Bank Darah →
Setup — dengan nama yang berbeda-beda untuk layar yang sama. Petugas yang mencari "Alasan Bank
Darah" dan petugas yang mencari "Daftar Alasan Terkendali" berakhir di layar yang sama tanpa tahu
keduanya identik.

**Sesudah:** ketiganya muncul **tepat sekali**, di bawah Bank Darah → Setup, dengan nama sesuai
kontrak.

#### A 2.3 Jalur tidak normal — layar yang belum ada

Tujuh layar sisanya — `FE-BD-01` Order Darah, `FE-BD-03` Permintaan PMI, `FE-BD-04` Kantong Darah,
`FE-BD-06` Pemeriksaan Golongan Darah, `FE-BD-07` Tindakan Bank Darah, ditambah layar anak
`FE-BD-02` dan `FE-BD-05` — **belum dibangun**. Kartu task memerintahkan:

> Butir menu yang menunjuk layar belum ada **wajib disembunyikan**, bukan menampilkan halaman kosong.

Ketujuhnya karena itu **tidak** didaftarkan. Petugas tidak akan menemukan butir menu yang mengarah
ke halaman kosong.

#### A 2.4 Jalur tidak normal — tanpa hak akses

**Belum tertangani.** Butir menu tetap tampil walau pengguna tidak berhak; penolakan baru terjadi
ketika layarnya dibuka dan backend memulangkan `403`, yang lalu ditampilkan `AccessDeniedGate`.
Pengguna tetap terlindungi — data tidak bocor — tetapi ia melihat pintu yang tidak bisa dibukanya.

---

### A 3. Perubahan yang dikerjakan

#### A 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` frontend · `rules/GLOBAL_RULES.md` · `rules/rule-output/status-task-roadmap.md` · `rules/rule-output/grafik-dependency-roadmap.md` (**baru pada suite 1.18.0**) |
| Kontrak menu | `03-frontend-architecture.md` §2 — tabel butir menu, tingkat, induk, layar, butir hak akses |
| Source menu | `src/utils/menu-sidebar/menu-items.jsx` |
| Mekanisme akses | `filter-menu-items-by-role.jsx` · `left-sidebar-items-virtualized.jsx` · `access-denied-gate.jsx` · `roleSlice.jsx` |
| Roadmap | `roadmap/frontend-roadmap.md` §4 kartu `FE-BD-006` |

#### A 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | **18 baris dihapus.** Tiga entri Bank Darah yang menduplikasi layar Setup dibuang dari grup `healthServicesMasterData`. Nol baris ditambahkan |

**Nol berkas baru.** **Nol berkas lain disentuh.**

Ketiga ikon yang dipakai entri terhapus — `RiFlaskLine`, `RiFileList3Line`, `RiMapPinLine` — tetap
terpakai di tempat lain (1, 35, dan 1 pemakaian tersisa), sehingga tidak ada import yang menjadi
yatim.

#### A 3.3 Kepatuhan arsitektur frontend

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

#### A 3.4 Gerbang keputusan base component

Task ini **tidak menyusun satu layar pun**. Yang disentuh adalah data menu; satu-satunya JSX di
sana adalah komponen ikon yang sudah ada, dan task ini **menghapus** pemakaiannya, bukan menambah.

| Elemen | Status | Keterangan |
| --- | --- | --- |
| Butir menu | `NOT APPLICABLE` | Data konfigurasi, bukan komponen tampilan |
| Ikon menu | `REUSE` | `react-icons/ri` yang sudah di-import; nol ikon baru, nol import baru |
| Komponen tampilan | `NOT APPLICABLE` | Nol layar disusun |

`UI GATE: PASSED — 0 elemen NEW, 0 EXTEND, 0 menunggu keputusan pengguna.`

---

### A 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — menu dirender dari konstanta statis, tanpa permintaan jaringan |
| Kosong | `NOT APPLICABLE` — grup Bank Darah selalu memuat sekurang-kurangnya tiga butir Setup |
| Gagal | `NOT APPLICABLE` — nol pemanggilan API pada jalur ini |
| Tanpa hak akses | **BELUM tertangani** — butir menu tetap tampil bagi pengguna yang tidak berhak. Penolakan baru terjadi di layarnya lewat `403` dan `AccessDeniedGate`. Lihat §1.2 dan §7 |

---

### A 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini nol memanggil API. Butir menu hanya menunjuk route frontend.

---

### A 6. Verifikasi

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

### A 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Butir menu mengarah ke layar yang **hak aksesnya benar** — bagian *mengarah ke layar* | ✅ **Terpenuhi** | Ketiga butir Setup menunjuk route yang terbukti ada pada keluaran build, dengan label dan induk sesuai `03-frontend-architecture.md` §2. Tujuh layar yang belum dibangun **tidak** didaftarkan, sesuai catatan urutan |
| Butir menu **hanya tampil bagi peran yang berhak** | ⛔ **BELUM terpenuhi** | Mekanismenya tidak ada. `filterMenuItemsByRole` adalah stub yang seluruh logikanya dikomentari; frontend tidak memiliki katalog permission pengguna berjalan. **Tidak dapat dikerjakan dengan menyunting `menu-items.jsx`** — lihat §1.2 dan §8 |

#### A Definition of Done

| Butir DoD | Status |
| --- | --- |
| Registrasi menu menjadi acceptance salah satu task layar, bukan pekerjaan berdiri sendiri | ✅ **Terpenuhi, dan memang begitu yang terjadi** — pendaftaran ketiga butir dikerjakan `FE-BD-001` dan `FE-BD-011` bersama layarnya masing-masing. Yang tersisa bagi task ini adalah merapikan penyimpangan, bukan mendaftarkan dari nol |
| Butir menu yang menunjuk layar belum ada wajib disembunyikan | ✅ **Terpenuhi** — tujuh layar tanpa source tidak punya butir menu |
| Susunan menu cocok kontrak `03-frontend-architecture.md` §2 | ✅ **Terpenuhi** untuk seluruh butir yang layarnya ada |

**Satu dari dua acceptance criteria terpenuhi.** Karena itu task ini 🟡 **SEBAGIAN**, bukan ✅.

---

### A 8. Catatan penutup

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

### A 9. Status Git

```text
 M src/utils/menu-sidebar/menu-items.jsx
```

Frontend `HEAD` tetap `f79af1684` cabang `sukmagpV2`, upstream `origin/sukmagpV2`. Backend
**tidak disentuh** di luar berkas laporan ini beserta bukti roadmap dan traceability.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. Nol perintah database dijalankan.
