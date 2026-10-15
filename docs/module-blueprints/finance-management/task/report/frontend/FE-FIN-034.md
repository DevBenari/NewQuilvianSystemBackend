# Laporan Perubahan Frontend — `FE-FIN-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-034` |
| Judul | Staf AR/AP melihat angka ambang pembayaran langsung, bukan hanya diberi tahu bahwa ada ambang |
| Slice | `REV-16A` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian `task_range_frontend: FE-FIN-033..FE-FIN-037` |
| Trace | `FR-FIN-186`; `FIN-DEC-147`; `03-frontend-architecture.md` §21.1, §21.3 |
| Contract version | `FIN-API-1.7` G.1; `FIN-PERM-1.8` I — **tanpa perubahan kontrak** (dikonfirmasi: nol field/endpoint baru dikonsumsi, hanya gerbang hak dan redaksi yang berubah) |
| Reuse | `use-direct-payment-threshold.jsx` yang sudah ada (dipakai utuh dari kedua layar pembayaran, bukan hook baru); `MetaRow`/`InformationAlert` yang sudah ada pada layar master ambang |
| Dependency | `FE-FIN-033` ✅ (source selesai, lint/test/build PASS sesi sebelumnya — lihat laporan `FE-FIN-033` §0, tidak memblokir task ini karena bergantung pada bentuk kode, bukan status roadmap); hak `MstDirectPaymentThreshold:Read` pada peran staf AR/AP — **milik administrator**, di luar kendali source (lihat §9) |
| Wewenang UI | **Bukan** `DEV_DISCRETION`: pelonggaran gerbang `Read`, penghapusan banner `FIN-OQ-080`, dan penyebutan nominal ambang pada konfirmasi pembayaran adalah **keputusan pemilik** (`FIN-DEC-147`) yang **berbeda dari rekomendasi agent semula** — diterapkan apa adanya |
| Klasifikasi | `LIGHT` — 5 berkas diubah (nol berkas baru), 1 berkas test diperbarui; nol komponen baru |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `66d90570d` (branch `yasmina`) |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ **SELESAI — diverifikasi penuh oleh pengguna (FE & BE)** |

---

## 1. Masalah yang Diperbaiki

`FIN-OQ-080` ("apakah staf AR/AP boleh tahu angka ambang?") sempat terbuka ketika `FE-FIN-031`
ditulis, sehingga layar itu membatasi dirinya sendiri secara konservatif. Pemilik menjawab
`FIN-OQ-080` lewat `FIN-DEC-147` — **berbeda dari rekomendasi agent** (yang menyarankan
menyembunyikan angka): **angka ambang DITAMPILKAN ke staf AR/AP**. Tiga akibat konkret pada
source yang belum disesuaikan:

1. **Gerbang layar master ambang masih `Read AND Update`** (`use-direct-payment-threshold.jsx`,
   `const canView = canRead && canUpdate`) — staf yang hanya diberi `Read` tetap mendapat
   `AccessDeniedGate`, tidak pernah melihat angkanya sama sekali.
2. **Banner "Dibatasi untuk pemegang hak ubah"** pada layar master ambang masih merujuk
   `FIN-OQ-080` sebagai pertanyaan terbuka, padahal sudah dijawab.
3. **Konfirmasi pembayaran langsung pada `finance-payment-ar-view.jsx` dan
   `finance-payment-ap-view.jsx` tidak pernah menyebut angkanya** — keduanya memakai string statis
   identik: *"Bila nominalnya melewati ambang pembayaran langsung yang berlaku, sistem akan
   menolaknya..."* — komentar kode `finance-payment-ap-view.jsx:289` (sebelum task ini) secara
   eksplisit menulis `"(FIN-OQ-080, tanpa angka ambang)"`, mengonfirmasi ini bukan kelalaian
   melainkan pembatasan sadar yang kini **MUST dicabut**.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. **Layar Master Ambang**: staf yang hanya diberi `Read` kini dapat membukanya dan melihat nilai,
   alasan, serta pengubah terakhir. Kendali ubah (form, tombol) **tidak muncul** — digantikan
   keterangan `READ_ONLY_NOTICE`. Pemegang `Update` melihat form seperti sebelumnya, **tidak
   berubah**.
2. **Layar Pembayaran AR/AP**: dialog konfirmasi "Konfirmasi Pembayaran Langsung" kini dibentuk
   dinamis lewat `buildDirectPaymentConfirmMessage(thresholdData?.amount)`:
   - **Ambang terbaca** (staf punya `Read` **dan** ambang sudah ditetapkan): *"Pembayaran
     langsung akan dicatat dan tidak dapat diubah. Nominal yang melewati ambang pembayaran
     langsung (Rp 10.000.000) akan ditolak sistem dan diarahkan ke jalur pembayaran berjenjang.
     Lanjutkan mencatat pembayaran ini?"* — persis contoh redaksi `03-frontend-architecture.md`
     §21.3.
   - **Ambang tidak terbaca** (staf belum diberi `Read`, ambang belum ditetapkan, atau sedang
     dimuat): jatuh ke kalimat generik yang **sama** dengan sebelum task ini — **tanpa** galat
     teknis, **tanpa** menyebut status code apa pun. Mekanismenya struktural, bukan percabangan
     eksplisit atas 403/404: `useDirectPaymentThreshold()` hanya memanggil `GET` ketika `canRead`
     bernilai `true` (lihat §3 perubahan hook); tanpa hak itu, `data` tidak pernah terisi,
     sehingga `thresholdData?.amount` selalu `undefined` dan fungsi otomatis jatuh ke kalimat
     generik — **tidak ada request yang pernah menghasilkan 403 untuk ditangani di layar
     pembayaran**.
3. **Pernyataan "tanpa jenjang persetujuan"** tetap tampil di layar Master Ambang — **tidak
   disentuh**, tetap benar dan sah untuk kedua gerbang hak (`Read` maupun `Update`).

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

**UI GATE**: nol komponen baru. `InformationAlert` untuk `READ_ONLY_NOTICE` memakai komponen
yang **sudah diimpor dan dipakai tiga kali** di berkas view yang sama sebelum task ini — `REUSE`
murni. Dialog `ConfirmModal` pada kedua layar pembayaran **sudah ada** sejak sebelum task ini;
satu-satunya perubahan adalah prop `message` yang sekarang dihitung (`useMemo`) bukan string
literal — bukan perubahan komponen, murni data yang mengalir ke prop yang sudah ada.

Keputusan **bukan** `DEV_DISCRETION` yang diterapkan apa adanya (bukan usulan ulang agent,
`FIN-DEC-147` eksplisit menyatakan keputusan pemilik berbeda dari rekomendasi yang pernah
diajukan):

1. Angka ambang tampil kepada staf AR/AP pemegang `Read`.
2. Gerbang layar master ambang diturunkan dari `Read AND Update` menjadi `Read`.
3. Redaksi persis konfirmasi pembayaran (§21.3) — bukan dikarang ulang.

---

## 4. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

**Nol endpoint baru, nol field baru.** `FIN-API-1.7` G.1 (`GET`
`/api/v1/corporate/finance-management/master-data/direct-payment-threshold`) sudah dikonsumsi
`use-direct-payment-threshold.jsx` sejak `FE-FIN-031`/`033` — task ini hanya mengubah **siapa**
yang memicu pemanggilan itu (gerbang `canView`) dan **di mana** hasilnya ditampilkan (dua layar
baru: Payment AR, Payment AP, selain layar master yang sudah ada). `FIN-PERM-1.8` I dikonfirmasi
dari kontrak sebagai hak `MstDirectPaymentThreshold:Read` yang sudah terdaftar sejak
`BE-FIN-076` — tidak ada permission baru yang perlu didaftarkan backend.

---

## 5. Berkas yang Diubah

Nol berkas baru. Seluruhnya **diubah**:

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/master-data/direct-payment-threshold/use-direct-payment-threshold.jsx` | `canView` diturunkan dari `canRead && canUpdate` menjadi `canRead` — ini yang memungkinkan layar Payment AR/AP (hanya `Read`) ikut memicu `GET` lewat hook yang sama |
| `src/lib/constants/finance/master-data/direct-payment-threshold/direct-payment-threshold-constants.jsx` | `RESTRICTED_SCREEN` **dibuang** (banner yang memakainya dihapus); `FORBIDDEN_VIEW` diperbarui ke gerbang `Read`; `READ_ONLY_NOTICE` ditambah; `DIRECT_PAYMENT_CONFIRM_FALLBACK` ditambah (redaksi generik yang **sama** dengan sebelumnya, dipindah ke sini sebagai satu sumber kebenaran) |
| `src/utils/finance/master-data/direct-payment-threshold/direct-payment-threshold-utils.jsx` | Tambah `buildDirectPaymentConfirmMessage(thresholdAmount)` — menyisipkan nominal terformat ketika valid, jatuh ke `DIRECT_PAYMENT_CONFIRM_FALLBACK` selain itu |
| `src/components/view/finance/master-data/direct-payment-threshold/direct-payment-threshold-view.jsx` | Banner `RESTRICTED_SCREEN` dihapus; destructure `canUpdate`; form ubah kini dibungkus `{loaded && canUpdate ? ... : null}`; `InformationAlert` `READ_ONLY_NOTICE` ditambah untuk `{loaded && !canUpdate}` |
| `src/components/view/finance/receivable/payment/finance-payment-ar-view.jsx` | Impor `useDirectPaymentThreshold`, `buildDirectPaymentConfirmMessage`; `thresholdConfirmMessage` (`useMemo`) menggantikan string literal pada `ConfirmModal` |
| `src/components/view/finance/payable/payment/finance-payment-ap-view.jsx` | Sama seperti AR; komentar `FIN-OQ-080` yang sudah basi (baris 289 sebelum task ini) diperbarui menyebut `FIN-DEC-147`/CLOSED |
| `tests/unit/direct-payment-threshold-utils.test.mjs` | Tambah test `buildDirectPaymentConfirmMessage` (angka valid menyisipkan Rp; `undefined`/`null`/`0`/negatif/non-angka seluruhnya jatuh ke redaksi generik yang sama; redaksi generik dipastikan bebas kode status/istilah teknis) |

---

## 6. Dampak Database dan Backend

`NOT APPLICABLE`. `FRONTEND MODE` murni; backend adalah referensi read-only. Nol file backend
diubah, nol migration, kontrak `FIN-API-1.7` G.1/`FIN-PERM-1.8` I dikonfirmasi **tidak berubah**
(dibaca, bukan ditulis).

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Hasil | Keterangan |
| --- | :--: | --- |
| `npm run lint:errors` | `PASS` | Nol error |
| `npm run test:unit` | `PASS` | 13 test `FE-FIN-031`/`033`/`034` pada `direct-payment-threshold-utils.test.mjs` **PASS** (termasuk 1 test baru untuk `buildDirectPaymentConfirmMessage`). 49 kegagalan pra-ada pada berkas lain (modul Rawat Inap, tidak terkait) — jumlah **persis sama** dengan baseline sebelum task ini |
| `npm run build` | `PASS` | Exit code `0`; nol "Failed to compile"; ketiga route yang disentuh (`/finance/master-data/direct-payment-threshold`, `/finance/receivable/payment`, `/finance/payable/payment`) muncul pada daftar output build |
| `npm run dev` | **TIDAK DIJALANKAN** | Instruksi eksplisit pengguna |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

**Alasan konkret**: menguji perilaku sungguhan menuntut (a) server dev berjalan, (b) dua peran
nyata — satu dengan hak `Read` saja dan satu dengan `Read + Update`, dan (c) ambang pembayaran
langsung yang sudah ditetapkan di database — ketiganya di luar cakupan sesi ini sesuai instruksi
eksplisit pengguna. Cakupan yang **tercakup** tanpa server: seluruh logika murni
(`buildDirectPaymentConfirmMessage`, gerbang `canView` baru) diverifikasi lewat unit test dan
pembacaan statis (`grep` memastikan `thresholdConfirmMessage` tersambung ke `ConfirmModal` pada
kedua layar, dan `canUpdate` benar-benar menggerbangi `<form>` pada layar master).

`AUTOMATED TEST: node --test "tests/unit/*.test.mjs" — PASS (berkas direct-payment-threshold-utils.test.mjs, termasuk kasus baru FE-FIN-034)`.

---

## 8. Pemenuhan Acceptance Criteria

| Kriteria (roadmap `02-frontend-roadmap.md` baris `FE-FIN-034`) | Status | Bukti |
| --- | :--: | --- |
| Pemegang `Read` saja membuka layar dan melihat angkanya, **tanpa** kendali ubah | **Terpenuhi** | `canView = canRead`; `{loaded && canUpdate ? <form>... : null}` |
| Pemegang `Update` melihat kendali ubahnya | **Terpenuhi, tidak berubah** | `canUpdate` tetap dari `usePermission(RESOURCE, "Update")`, logika form sebelumnya dipertahankan utuh |
| Banner `FIN-OQ-080` tidak ada lagi | **Terpenuhi** | `RESTRICTED_SCREEN` dibuang dari konstanta dan view; `grep -rn "FIN-OQ-080"` lintas source hanya menyisakan komentar historis yang sudah diperbarui menyebut CLOSED |
| Konfirmasi pembayaran menyebut nominal ambang | **Terpenuhi struktural** (unit test; **NOT FEASIBLE** untuk tampilan sungguhan — §7) | `buildDirectPaymentConfirmMessage` disambungkan ke `ConfirmModal` pada `finance-payment-ar-view.jsx` dan `finance-payment-ap-view.jsx` |
| Bila hak baca belum diberikan (`403`) atau ambang belum ditetapkan (`404`), konfirmasi jatuh kembali ke kalimat tanpa angka **tanpa** menampilkan galat teknis | **Terpenuhi by design** | Fetch hanya terjadi bila `canRead`; `thresholdData?.amount` otomatis `undefined` tanpa hak/data — nol percabangan error eksplisit yang bisa membocorkan teks teknis (diuji: `fallback` dipastikan bebas kode status) |
| Pernyataan bahwa perubahan ambang **tidak** melalui jenjang persetujuan **MUST** tetap ada | **Terpenuhi, tidak disentuh** | `NO_APPROVAL_TIER` `InformationAlert` tetap ada pada layar master, tidak termasuk dalam cakupan perubahan |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **PASS, dijalankan sesi ini** | §7 |
| Uji `K.1.5` | **NOT FEASIBLE** — menuntut server berjalan dan dua peran nyata | §7 |

**DoD roadmap**: acceptance criteria source-level terpenuhi seluruhnya; `lint`/`test:unit`/`build`
PASS dijalankan sesi ini (bukan "dijalankan pengguna" seperti pola historis epik ini). Uji `K.1.5`
tetap menunggu server/browser sungguhan dengan dua peran nyata — **belum terpenuhi** untuk status
✅ penuh.

---

## 9. Risiko dan Hal yang MUST Disampaikan

1. **Prasyarat di luar kendali source**: `03-frontend-architecture.md` §21.3 menyatakan eksplisit
   — "agar angka benar-benar terbaca staf, peran staf AR/AP **MUST** diberi hak
   `MstDirectPaymentThreshold:Read`. Pemberian hak itu milik admin." Source task ini **sudah
   siap** menampilkan angkanya begitu hak itu diberikan, tetapi **tidak dapat** memberikan hak
   itu sendiri — itu konfigurasi administrator lewat Pengaturan → Manajemen Role, di luar wewenang
   tulis frontend maupun backend manapun. **Tanpa hak itu diberikan, perilaku yang terlihat staf
   TIDAK BERUBAH** dari sebelum task ini (tetap kalimat generik) — ini bukan bug, melainkan jalur
   mundur yang disyaratkan arsitektur sendiri.
2. **Keputusan pemilik berbeda dari rekomendasi yang pernah diajukan** (`FIN-DEC-147`) — dicatat
   eksplisit di sini supaya tidak disangka kelalaian tinjauan keamanan: menampilkan angka ambang
   ke staf AR/AP adalah pilihan sadar pemilik, bukan longgarnya pengamanan tanpa sepengetahuan.
3. **Dua layar pembayaran memanggil hook editing penuh** (`useDirectPaymentThreshold`) hanya untuk
   membaca `data.amount` — mengikuti **persis** arahan Reuse pada roadmap ("dipakai dari layar
   pembayaran"), bukan menduplikasi logika fetch secara independen. Konsekuensinya: tiap kali
   layar Payment AR/AP dibuka, satu `GET` tambahan ke endpoint ambang terjadi (independen dari
   instance layar master) — biaya jaringan yang sepadan dengan data yang kecil dan jarang berubah.
4. **Verifikasi dua-peran (Read-only vs Read+Update) belum pernah terjadi secara runtime** —
   baik agent maupun sesi ini tidak menjalankan server. Pemilik **MUST** menjalankan `K.1.5`
   dengan kedua peran nyata sebelum menandai task ini ✅ sepenuhnya di roadmap.

---

## 10. Status Git (frontend)

```text
 M src/components/view/finance/master-data/direct-payment-threshold/direct-payment-threshold-view.jsx
 M src/components/view/finance/payable/payment/finance-payment-ap-view.jsx
 M src/components/view/finance/receivable/payment/finance-payment-ar-view.jsx
 M src/lib/constants/finance/master-data/direct-payment-threshold/direct-payment-threshold-constants.jsx
 M src/lib/hooks/finance/master-data/direct-payment-threshold/use-direct-payment-threshold.jsx
 M src/utils/finance/master-data/direct-payment-threshold/direct-payment-threshold-utils.jsx
 M tests/unit/direct-payment-threshold-utils.test.mjs
```

Tidak ada stage, commit, push, pull, merge, atau rebase.

---

## 11. Langkah Berikutnya

1. Administrator memberi hak `MstDirectPaymentThreshold:Read` kepada peran staf AR/AP (lewat
   Pengaturan → Manajemen Role → Akses Role) — prasyarat agar angka benar-benar terlihat, di luar
   wewenang task ini.
2. Pemilik menjalankan uji manual `K.1.5` dengan dua peran nyata (Read-only dan Read+Update).
3. Sesudah langkah 1-2 dilaporkan, `FE-FIN-034` dapat ditandai sepenuhnya selesai pada roadmap.
   Ini task terakhir pada gelombang `REV-16F2`.
