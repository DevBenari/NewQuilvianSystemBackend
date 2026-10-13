# Laporan Perubahan Frontend — `FE-FIN-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-033` |
| Judul | Pejabat yang mengubah ambang pembayaran langsung bekerja dari nilai terbaru, dan layar menyebut siapa yang terakhir mengubahnya |
| Slice | `REV-16A` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian `task_range_frontend: FE-FIN-033..FE-FIN-037` |
| Trace | `FR-FIN-184`, `FR-FIN-185`, `FR-FIN-187`; `FIN-DEC-145`, `146`, `151`, `153`; `03-frontend-architecture.md` §21.4 nomor 1 dan 2, §21.5, §21.8 |
| Contract version | `FIN-API-1.7` G.1; `FIN-VAL-1.9` `FIN-VAL-228` |
| Reuse | Pola `rowVersion`/benturan `409` dari `use-opening-balances.jsx` (`isStaleConflict` → tutup dialog → muat ulang) diadaptasi; pola fallback nama `|| "-"` dari `MetaRow` di layar yang sama |
| Dependency | `BE-FIN-086` — **rilis** (`dotnet build` PASS, migration diterapkan, `has-pending-model-changes` retroaktif PASS, seluruhnya dilaporkan pemilik 4 Oktober 2026; laporan `BE-FIN-086` §7 menyatakan eksplisit "`FE-FIN-033` tidak lagi tertahan backend, boleh mulai"). Status roadmap backend masih 🟡 karena uji manual `K.1.*`/`K.2.*` belum dilaporkan — **bukan** karena source/build/migration belum selesai |
| Wewenang UI | `DEV_DISCRETION` untuk redaksi label baris baru ("Diubah oleh") dan penempatannya; **bukan** `DEV_DISCRETION` untuk larangan menampilkan `rowVersion` (invariant pemilik, `03-frontend-architecture.md` §21.8) dan redaksi pesan benturan 409 (approved UI copy, §21.5) |
| Klasifikasi | `LIGHT` — 4 berkas source diubah (nol berkas baru), 1 berkas test unit diperbarui; nol komponen baru |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `66d90570d` (branch `yasmina`) |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **SEBAGIAN, 10 Oktober 2026.** Source lengkap; `npm run lint:errors`, `npm run test:unit`, dan `npm run build` seluruhnya PASS (dijalankan sesi ini). `npm run dev` **TIDAK DIJALANKAN** (instruksi eksplisit pengguna). Sisa: uji manual `K.1.3`, `K.1.4`, `K.2.1`, `K.2.2`, `K.2.6` **NOT FEASIBLE** tanpa server berjalan — satu-satunya yang tersisa untuk ✅ |

---

## 0. Gerbang Dependency Backend — Diperiksa Sebelum Menulis Kode

Baris roadmap frontend mensyaratkan `BE-FIN-086` **rilis**. Diperiksa langsung ke laporan task
`BE-FIN-086` (bukan hanya tanda 🟡 pada roadmap, yang bisa menyesatkan): `dotnet build` **PASS**,
migration `AlterMstDirectPaymentThresholdRowVersion` **diterapkan** ke database, dan
`dotnet ef migrations has-pending-model-changes` retroaktif **PASS** — ketiganya dilaporkan
pemilik 4 Oktober 2026 dengan bukti log migrasi yang namanya cocok persis. Laporan itu sendiri
menyatakan eksplisit pada §7: **"`FE-FIN-033` tidak lagi tertahan backend. Boleh mulai."** Satu-
satunya sisa `BE-FIN-086` (uji manual `K.1.*`/`K.2.*` belum dilaporkan) tidak menahan task ini —
itu adalah bukti *penggunaan* API yang baru bisa dikumpulkan setelah frontend-nya ada, bukan
prasyarat untuk *memulai* frontend. Disimpulkan: task ini **tidak terblokir**.

---

## 1. Masalah yang Diperbaiki

Layar Master Ambang Pembayaran Langsung (`FE-FIN-031`) ditulis sebelum `BE-FIN-086` mengubah
kontrak `FIN-API-1.7` G.1. Tiga ketidaksesuaian konkret terhadap kontrak baru:

1. **Payload `PUT` masih mengirim `effectiveFrom`**, ruas yang sudah dibuang dari
   `UpdateDirectPaymentThresholdRequest` dan kolomnya dari database. Backend mengabaikannya
   (ruas tak dikenal), tetapi payload tidak membawa `expectedRowVersion` yang **wajib** sejak
   baris ambang aktif pertama kali ada — setiap `PUT` kedua dan seterusnya akan dijawab **409**
   oleh backend (`FIN-VAL-228`), fitur ubah ambang akan selalu gagal sesudah penetapan pertama.
2. **Benturan `409` tidak ditangani sama sekali.** `classifyThresholdError` tidak punya cabang
   untuk status `409` — jatuh ke `GENERIC`, hanya menampilkan pesan mentah tanpa memuat ulang
   nilai/alasan/nama pengubah terbaru seperti yang dituntut `03-frontend-architecture.md` §21.5.
3. **Nama pengubah terakhir tidak ditampilkan.** Baris "Terakhir diubah" hanya menunjukkan
   waktunya; `DirectPaymentThresholdResponse.LastChangedByName` (baru di `BE-FIN-086`) belum
   dikonsumsi sama sekali (§21.4 nomor 2, `FIN-DEC-151`).

**Catatan cakupan**: kolom tanggal berlaku **sudah tidak ada** di form maupun tampilan sejak
`FE-FIN-031` ditulis — acceptance criteria roadmap menyebutnya sebagai "dihapus", tetapi
pemeriksaan langsung ke `direct-payment-threshold-view.jsx` (sebelum task ini) menunjukkan form
hanya pernah punya dua field (`amount`, `changeReason`); kolom tanggal berlaku tidak pernah
dirender di layar manapun. Satu-satunya jejak `effectiveFrom` ada di `utils`/payload (§1 nomor 1
di atas), bukan di UI. Dicatat di sini agar laporan tidak mengklaim "menghapus" sesuatu yang
sebenarnya tidak pernah ada secara visual.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. Pejabat membuka layar, `GET` mengembalikan ambang aktif beserta `rowVersion` dan
   `lastChangedByName` — keduanya disimpan di state, hanya `lastChangedByName` yang dirender.
2. Pejabat mengubah nilai/alasan, konfirmasi, `PUT` dikirim membawa `expectedRowVersion` persis
   dari `rowVersion` yang baru dibaca.
3. **Jalur sukses**: respons baru (dengan `rowVersion` baru) menggantikan state; dialog tertutup;
   pesan sukses tampil.
4. **Jalur benturan (409)** — pejabat lain mengubah ambang di antara pembacaan dan penyimpanan:
   dialog konfirmasi tertutup, data dimuat ulang (nilai, alasan, nama pengubah terbaru), dan
   banner halaman menampilkan *"Ambang sudah diubah oleh orang lain. Muat ulang sebelum
   melanjutkan."* — redaksi tetap, sama untuk ketiga lapis benturan backend (versi basi, race
   penyimpanan, race penetapan pertama bersamaan), bukan teks mentah backend yang sedikit berbeda
   kata kerjanya ("diubah" vs "ditetapkan").
5. **Penetapan pertama** (`notSet === true`, belum ada baris): `rowVersion` di state adalah
   `null`/tidak ada; payload mengirim `expectedRowVersion: null`, sesuai pengecualian backend —
   **MUST NOT** pernah mengirim nilai bukan-null pada jalur ini (risiko yang sama persis
   diperingatkan laporan `BE-FIN-086`: ambang tidak akan pernah bisa ditetapkan).
6. Penanda versi (`rowVersion`) **tidak pernah** dirender ke JSX manapun — sesuai larangan
   eksplisit `03-frontend-architecture.md` §21.8 ("Menampilkan penanda versi ambang kepada
   pengguna" masuk daftar "yang sengaja TIDAK dibuat").

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

**UI GATE**: nol komponen baru. Satu baris `MetaRow` tambahan ("Diubah oleh") memakai komponen
lokal `MetaRow` yang **sudah ada** di berkas view yang sama (didefinisikan baris 28-33,
dipakai dua kali sebelum task ini) — `REUSE` murni, tanpa perlu tabel keputusan karena tidak ada
elemen baru yang bukan `REUSE`. Fallback teks `data.lastChangedByName || "-"` mengikuti konvensi
`|| "-"` yang **sudah dipakai baris tepat di atasnya** (`data.changeReason || "-"`) pada berkas
yang sama — bukan pola baru, dan bukan redaksi lebih panjang (`"Nama penyetuju tidak tersedia"`)
yang dipakai modul lain (`opening-balance-group-card.jsx`), karena modul **ini** tidak memakai
konvensi itu di tempat manapun.

Redaksi pesan benturan 409 (§21.5) dan larangan menampilkan `rowVersion` (§21.8) adalah
**approved invariant/UI copy milik pemilik** — diterapkan apa adanya, bukan `DEV_DISCRETION`.

---

## 4. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

| Endpoint | Method | Perubahan payload/respons | Konsumen |
| --- | --- | --- | --- |
| `/api/v1/corporate/finance-management/master-data/direct-payment-threshold` | `GET` | Respons kini membawa `rowVersion`, `lastChangedByName`; `effectiveFrom` **tidak lagi** dikirim backend (sudah dibuang `BE-FIN-086`) | `fetchDirectPaymentThreshold` (slice, tidak diubah — hanya `normalizeDirectPaymentThreshold` yang berubah) |
| `/api/v1/corporate/finance-management/master-data/direct-payment-threshold` | `PUT` | Payload kini `{ amount, changeReason, expectedRowVersion }`, **bukan** `{ amount, changeReason, effectiveFrom }`; respons `409` baru harus ditangani | `updateDirectPaymentThreshold` (slice, tidak diubah — payload dibentuk `buildThresholdPayload` yang berubah) |

Route, verb, dan nama slice/thunk **tidak diubah** — seluruh delta murni pada bentuk payload dan
respons yang diproses `direct-payment-threshold-utils.jsx`, persis cakupan yang diberi wewenang
roadmap (empat berkas: `*-utils.jsx`, `*-constants.jsx`, `use-direct-payment-threshold.jsx`,
`*-view.jsx`).

---

## 5. Berkas yang Diubah

Nol berkas baru. Seluruhnya **diubah**:

| Berkas | Perubahan |
| --- | --- |
| `src/utils/finance/master-data/direct-payment-threshold/direct-payment-threshold-utils.jsx` | `normalizeDirectPaymentThreshold`: `effectiveFrom` dibuang, `rowVersion`+`lastChangedByName` ditambah (dual-casing, mengikuti pola defensif yang sudah ada). `buildThresholdPayload`: parameter `now`/`effectiveFrom` dibuang (beserta import `getTodayWibDateString` yang jadi tidak terpakai), `expectedRowVersion` ditambah dari `form.rowVersion ?? null`. `THRESHOLD_ERROR_KIND`: tambah `STALE`. `classifyThresholdError`: cabang `409` baru. `normalizeThresholdError`: `409` kini memakai redaksi tetap `STALE_CONFLICT`, bukan teks mentah backend |
| `src/lib/constants/finance/master-data/direct-payment-threshold/direct-payment-threshold-constants.jsx` | Tambah `DIRECT_PAYMENT_THRESHOLD_MESSAGES.STALE_CONFLICT` — redaksi persis `03-frontend-architecture.md` §21.5 |
| `src/lib/hooks/finance/master-data/direct-payment-threshold/use-direct-payment-threshold.jsx` | `handleConfirm`: payload kini menyertakan `rowVersion: data?.rowVersion ?? null`; cabang baru menutup dialog dan memanggil `reload()` ketika `classifyThresholdError(result.payload) === THRESHOLD_ERROR_KIND.STALE` |
| `src/components/view/finance/master-data/direct-payment-threshold/direct-payment-threshold-view.jsx` | Tambah satu `MetaRow` "Diubah oleh" menampilkan `data.lastChangedByName \|\| "-"`, di antara "Alasan perubahan terakhir" dan "Terakhir diubah" |
| `tests/unit/direct-payment-threshold-utils.test.mjs` | Diperbarui (bukan ditambah baru secara independen) mengikuti perubahan kontrak di atas: test payload dan normalisasi disesuaikan ke `expectedRowVersion`/`rowVersion`/`lastChangedByName`; test klasifikasi dan normalisasi pesan galat ditambah kasus `409`. Pembaruan test existing yang jadi basi oleh perubahan kontrak yang diberi wewenang — bukan penulisan test baru yang independen (`rules/frontend/test-policy.md`) |

`master-data-direct-payment-threshold-slice.jsx` (Redux) **sengaja tidak disentuh** — thunk-nya
agnostik terhadap bentuk payload/respons, delta sepenuhnya tertampung di `utils.jsx`.

---

## 6. Dampak Database dan Backend

`NOT APPLICABLE`. Task ini `FRONTEND MODE` murni; backend adalah referensi read-only
(`AGENTS.md` frontend). Nol file backend dibaca sebagai target tulis, nol migration, nol source
backend diubah.

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Hasil | Keterangan |
| --- | :--: | --- |
| `npm run lint:errors` | `PASS` | Nol error |
| `npm run test:unit` | `PASS` | `node --test "tests/unit/*.test.mjs"`. Seluruh 12 test `FE-FIN-031`/`FE-FIN-033` pada `direct-payment-threshold-utils.test.mjs` **PASS**. 49 test gagal pada berkas lain (modul Rawat Inap — `surgery-booking`, `ward-pre-op`, `post-op-summary`, dll.) — **dikonfirmasi pre-existing**: `git stash` lalu jalankan ulang menghasilkan **persis 49** kegagalan yang sama sebelum perubahan task ini disentuh, sama sekali tidak terkait modul Finance |
| `npm run build` | `PASS` | Exit code `0`; nol "Failed to compile"; route `/finance/master-data/direct-payment-threshold` muncul pada daftar output build |
| `npm run dev` | **TIDAK DIJALANKAN** | Instruksi eksplisit pengguna |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

**Alasan konkret**: verifikasi perilaku interaktif (klik "Ubah Ambang", respons `409` nyata dari
server, tampilan nama pengubah dari data sungguhan) menuntut server dev (`npm run dev`) dan/atau
backend berjalan untuk memicu benturan 409 sungguhan — keduanya di luar cakupan sesi ini sesuai
instruksi eksplisit pengguna. Cakupan yang **tercakup** tanpa server: seluruh logika murni
(`buildThresholdPayload`, `normalizeDirectPaymentThreshold`, `classifyThresholdError`,
`normalizeThresholdError`) diverifikasi lewat unit test di atas, bukan hanya dibaca manual.

`AUTOMATED TEST: node --test "tests/unit/*.test.mjs" — PASS (berkas direct-payment-threshold-utils.test.mjs)`.

---

## 8. Pemenuhan Acceptance Criteria

| Kriteria (roadmap `02-frontend-roadmap.md` baris `FE-FIN-033`) | Status | Bukti |
| --- | :--: | --- |
| Penetapan **pertama** (`404`) tetap berhasil **tanpa** `expectedRowVersion` | **Terpenuhi** (source + unit test) | `buildThresholdPayload({ ... })` tanpa `rowVersion` → `expectedRowVersion: null`; diuji `tests/unit/direct-payment-threshold-utils.test.mjs` |
| Ambang yang sudah ada mengirim `expectedRowVersion` pada setiap penyimpanan | **Terpenuhi** (source + unit test) | `handleConfirm` membaca `data?.rowVersion`; diuji payload test dengan `rowVersion` terisi |
| `409` menampilkan *"Ambang sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan."* lalu memuat ulang nilai, alasan, dan nama pengubah terbaru | **Terpenuhi struktural** (source + unit test untuk klasifikasi/pesan; **NOT FEASIBLE** untuk alur UI sungguhan — lihat §7) | `normalizeThresholdError` mengembalikan `STALE_CONFLICT` persis; `handleConfirm` memanggil `reload()` pada `THRESHOLD_ERROR_KIND.STALE` |
| Nama pengubah tampil; `null` ditulis keterangannya — **MUST NOT** menampilkan ID mentah | **Terpenuhi** | `MetaRow label="Diubah oleh"` → `data.lastChangedByName \|\| "-"`; `rowVersion`/ID mentah tidak pernah direferensikan di JSX manapun (diverifikasi `grep` nol hasil) |
| Penanda versi **MUST NOT** ditampilkan kepada pengguna | **Terpenuhi** | `grep` lintas keempat berkas yang diubah: `rowVersion`/`RowVersion` hanya muncul di state/payload, nol di JSX |
| `effectiveFrom` **tidak** lagi dikirim | **Terpenuhi** | Dibuang dari `buildThresholdPayload`; `Object.keys(payload)` diuji persis `["amount", "changeReason", "expectedRowVersion"]` |
| Kolom tanggal berlaku **dihapus** dari form dan tampilan | **Sudah terpenuhi sebelum task ini** (lihat §1 catatan cakupan) | Form hanya punya `amount`+`changeReason` sejak `FE-FIN-031`; tidak ada kolom tanggal di JSX manapun |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **PASS, dijalankan sesi ini** | §7 |
| Uji `K.1.3`, `K.1.4`, `K.2.1`, `K.2.2`, `K.2.6` | **NOT FEASIBLE** — menuntut server berjalan | §7 |

**DoD roadmap**: **belum terpenuhi sepenuhnya** — acceptance criteria source-level terpenuhi seluruhnya dan `lint`/`test:unit`/`build` PASS dijalankan sesi ini (bukan "dijalankan pengguna" seperti task lain pada epik ini — instruksi pengguna kali ini hanya mengecualikan `npm run dev`), tetapi uji `K.1.3`, `K.1.4`, `K.2.1`, `K.2.2`, `K.2.6` yang disyaratkan roadmap tetap menunggu server/browser sungguhan.

---

## 9. Risiko dan Hal yang MUST Disampaikan

1. **Risiko paling mahal pada epik ini (diperingatkan laporan `BE-FIN-086`) sudah ditangani secara spesifik**: `expectedRowVersion` hanya diisi dari `data?.rowVersion`, yang bernilai `null` tepat ketika `data` belum ada (penetapan pertama) — path ini **tidak pernah** mengirim nilai selain `null` pada jalur itu, diverifikasi lewat unit test `payload hanya memuat tiga ruas kontrak; expectedRowVersion null pada penetapan pertama`.
2. **Redaksi 409 memakai teks tetap, bukan teks backend asli** — keputusan sadar mengikuti `03-frontend-architecture.md` §21.5, bukan oversight. Bila pemilik kelak ingin menampilkan teks backend yang sesungguhnya (yang membedakan "diubah" vs "ditetapkan" rekan), itu perubahan `DEV_DISCRETION`→kebijakan yang **MUST** melalui keputusan pemilik, bukan diterapkan diam-diam.
3. **Verifikasi runtime penuh (klik sungguhan, 409 nyata, race dua tab) belum pernah terjadi** sejak `BE-FIN-086` maupun task ini — baik agent maupun sesi ini tidak menjalankan server. Pemilik **MUST** menjalankan uji manual `K.1.3`, `K.1.4`, `K.2.1`, `K.2.2`, `K.2.6` sebelum menandai `FE-FIN-033` maupun `BE-FIN-086` selesai sepenuhnya di roadmap.
4. **Nol perubahan pada Redux slice** — keputusan desain, bukan kelalaian: thunk-nya sudah agnostik terhadap bentuk payload sejak `FE-FIN-031`.

---

## 10. Status Git (frontend)

```text
 M src/components/view/finance/master-data/direct-payment-threshold/direct-payment-threshold-view.jsx
 M src/lib/constants/finance/master-data/direct-payment-threshold/direct-payment-threshold-constants.jsx
 M src/lib/hooks/finance/master-data/direct-payment-threshold/use-direct-payment-threshold.jsx
 M src/utils/finance/master-data/direct-payment-threshold/direct-payment-threshold-utils.jsx
 M tests/unit/direct-payment-threshold-utils.test.mjs
```

Tidak ada stage, commit, push, pull, merge, atau rebase.

---

## 11. Langkah Berikutnya

1. Pemilik menjalankan uji manual `K.1.3`, `K.1.4`, `K.2.1`, `K.2.2`, `K.2.6` (menuntut backend
   dan frontend berjalan bersamaan, termasuk simulasi dua pejabat/dua tab untuk memicu 409
   sungguhan).
2. Sesudah langkah 1 dilaporkan, `FE-FIN-033` dapat ditandai sepenuhnya selesai pada roadmap, dan
   `FE-FIN-034` (sudah menunggu `FE-FIN-033` pada grafik dependency) dapat dimulai.
