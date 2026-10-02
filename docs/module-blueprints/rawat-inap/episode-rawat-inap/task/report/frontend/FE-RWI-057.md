# Laporan Perubahan Frontend — `FE-RWI-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-057` |
| Judul | Kartu tempat tidur menyebut aturan yang menolaknya |
| Slice | Slice alasan penolakan — revision `8` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), kartu `FE-RWI-057` |
| Trace | Bukti runtime pemilik 9 September 2026; `RWI-RULE-012`; `FE-INP-02` langkah Pilih Bed; skema tampilan bagian 3.8 |
| Contract version | `contracts/api-contract.md` `0.7.0` — query `includeIneligible` dan field `ineligible` pada `GET /bed-occupancies/available-beds`. Permission tidak berubah, tetap `InpatientBedOccupancy : Read` |
| Wewenang UI | Susunan kartu dan letak lencana aturan mengikuti bentuk `PlacementFailureList` yang sudah disetujui pada `FE-RWI-026`; gaya visual `DEV_DISCRETION`. Wewenang itu dipakai sebatas dua kelas tata letak baru pada stylesheet yang sudah ada |
| Dependency | `BE-RWI-069` ✅ selesai 10 September 2026; `FE-RWI-026` ✅ sebagai pemilik layar |
| Klasifikasi | `LIGHT` — repository 1, berkas diubah 5 (4 source, 1 test), logika bisnis 0, kontrak API 1, database 0, keamanan/auth 0 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `7f6b9356f6349d516570d6d603ca026f2c7f4ec2` pada branch `HamzahV2` — perubahan task ini **masih lokal, belum di-commit** |
| Commit backend yang dijadikan rujukan | `d6858a9dba706f96a77e5e1855a9e4ae7cd78566` pada branch `MHamzah` — dibaca saja |
| Tanggal | 12 September 2026 |
| Status | ✅ **Selesai 12 September 2026.** Keenam acceptance criteria terpetakan ke source dan dikunci lima test baru. `npm run test:unit` 754 lulus 0 gagal; `npm run lint:errors` 0 error; `npm run build` beserta `postbuild` berhasil. Satu butir verifikasi **tidak dijalankan dan ditulis apa adanya**: bukti peramban atas tiga keadaan bed `NOT RUN` — repository tidak memiliki `playwright.config.*` dan `RWI-UI-GAP-007` masih terbuka |

---

## 1. Keadaan yang ditemukan di awal

Papan pemilihan tempat tidur meredupkan kartu yang tidak dapat dipilih, lalu menjelaskan
sebabnya hanya dari kolom yang kebetulan ada pada `bed-board`: terisi, dipesan, atau nama
status master. Ketika tidak satu pun kolom itu menjelaskan, layar jatuh ke kalimat
`NOT_ELIGIBLE`:

> "Server tidak memasukkan tempat tidur ini ke hasil pencarian untuk pasien tersebut.
> Alasan lengkapnya muncul bila penempatan tetap dicoba."

Kalimat itu jujur, tetapi tidak berguna. Petugas harus mencoba menempatkan pasien dulu
untuk tahu alasannya.

**Cacat yang dilaporkan pemilik 9 September 2026 lebih tajam dari itu.** Tempat tidur
berstatus master `Dipesan` yang sebenarnya ditolak aturan lain — misalnya jenis kelamin —
berbunyi "Status tempat tidur sekarang Dipesan". Petugas lalu mencari episode pemegang
yang tidak pernah ada, dan alasan sebenarnya tidak pernah muncul di layar.

`BE-RWI-069` sudah menutup sisi server pada 10 September 2026: `available-beds` menerima
`includeIneligible` dan mengembalikan `ineligible[]` berisi **seluruh** aturan yang menolak
tiap tempat tidur, dengan kalimat yang sama persis seperti yang dijawab `POST /placements`.
Frontend belum memakainya sama sekali.

---

## 2. Proses bisnis dari sisi pengguna

Petugas admisi berada di langkah Pilih Bed dengan satu episode di tangan. Papan
menampilkan seluruh tempat tidur pada unit yang dipilih. Yang redup tidak dapat dipilih.

Sesudah perubahan ini, kartu yang redup karena aturan kelayakan menyebut **nomor aturan dan
kalimat servernya** — bukan tebakan layar. Bila satu tempat tidur ditolak lebih dari satu
aturan, semuanya disebut, sebab petugas yang hanya membaca aturan pertama akan mengira
memindahkan pasien ke kamar lain sudah cukup.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx` | Satu nilai baru `BED_UNAVAILABLE_REASONS.RULE_REJECTED`, dibedakan tegas dari `NOT_ELIGIBLE` yang justru berarti layar **tidak tahu** aturan mana yang gagal |
| `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | `buildAvailableBedQuery` mengirim `includeIneligible` **hanya bersama** `episodeId`; dua fungsi baru `normalizeIneligibleBeds` dan `buildIneligibleFailureIndex`; `describeBedUnavailability` menerima argumen ketiga opsional |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx` | Menyimpan `ineligibleBeds`, menyusun peta `bedId` → `failures`, dan meneruskannya ke `describeBedUnavailability` |
| `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` | Kartu merender daftar aturan beserta nomornya ketika server mengirimkannya; kalimat lama tetap dirender ketika tidak |
| `src/style/health-services/inpatient-management/inpatient-bed-board.module.css` | Dua kelas tata letak `bedRules` dan `bedRuleItem`, seluruhnya memakai token yang sudah dipakai stylesheet ini |

### 3.2 Urutan yang mengikat pada `describeBedUnavailability`

Urutannya bukan selera, dan itu sebabnya ditulis sebagai komentar pada source:

| Urut | Keadaan | Alasan |
| --- | --- | --- |
| 1 | `isSelectable` | Tempat tidur yang lolos tidak menampilkan penolakan apa pun — kriteria 5 |
| 2 | `isOccupied` | Pemegangnya didahulukan; itulah yang dicari petugas — kriteria 6 |
| 3 | `isReserved` | Sama, pemegangnya didahulukan — kriteria 6 |
| 4 | **alasan server** | **Mendahului kalimat status master** — kriteria 2, cacat yang dilaporkan pemilik |
| 5 | `bedStatusName` | Kalimat lama, dipakai ketika server tidak mengirim alasan — kriteria 3 |
| 6 | `NOT_ELIGIBLE` | Layar mengaku tidak tahu, bukan menebak — kriteria 3 dan 4 |

Menaikkan langkah 4 ke atas langkah 2 akan menghilangkan nama pemegang tempat tidur, yang
justru dituntut kriteria 6. Menurunkannya ke bawah langkah 5 mengembalikan cacat pemilik.

### 3.3 Kenapa `includeIneligible` tidak pernah dikirim sendirian

Server menjawab `ineligible` kosong bila `episodeId` tidak ikut, sebab empat dari sembilan
aturan kelayakan tidak dapat dinilai tanpa episode dan alasan sebagian akan menyesatkan.
Mengirim penanda itu sendirian hanya menambah kolom yang selalu kosong, jadi `buildAvailableBedQuery`
menyalakannya **di dalam** cabang `if (episodeId)`. Papan tempat tidur berdiri sendiri —
yang memang tidak punya episode — karena itu tidak berubah satu perilaku pun.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| `LOADING` | Tidak berubah |
| Bed lolos | Nol kalimat penolakan, sekalipun server sempat mengirim alasan untuk bed lain |
| Bed ditolak aturan | Seluruh aturan disebut beserta nomornya, memakai kalimat server |
| Bed ditolak tanpa alasan server | Kalimat lama dipakai; layar tidak menampilkan kartu kosong |
| `ERROR` | `ineligibleBeds` ikut dikosongkan bersama papan, supaya alasan basi tidak tertinggal di layar |

---

## 5. Endpoint yang dikonsumsi

| Endpoint | Perubahan |
| --- | --- |
| `GET /bed-occupancies/available-beds` | Query `includeIneligible=true` ditambahkan ketika `episodeId` ada; kolom balasan `ineligible[]` mulai dibaca |

Nol endpoint baru. Nol permission baru. Nol route baru.

---

## 6. Verifikasi

| Butir | Hasil |
| --- | --- |
| `npm run test:unit` | **754 lulus, 0 gagal** — naik dari 737, lima di antaranya test baru task ini |
| `npm run lint:errors` | **0 error** |
| `npm run build` | **✓ Compiled successfully**, `postbuild` berhasil |
| Regresi enam test `inpatient-bed-board.test.mjs` yang sudah ada | Lulus tanpa disentuh — argumen ketiga `describeBedUnavailability` opsional, sehingga pemanggil lama tidak berubah perilakunya |
| Bukti peramban tiga keadaan bed | **`NOT RUN`** — repository tidak punya `playwright.config.*`, dan `RWI-UI-GAP-007` membuat lingkungan target belum punya data master yang layak |

---

## 7. Acceptance criteria

| # | Kriteria | Hasil | Bukti |
| --- | --- | --- | --- |
| 1 | Bed ditolak menampilkan kalimat server beserta nomor aturannya | ✅ | `inpatient-bed-board.jsx` merender `failures.map` dengan lencana `Aturan {ruleNumber}` |
| 2 | Bed berstatus master Dipesan menampilkan alasan sebenarnya | ✅ | Test `FE-RWI-057 kriteria 2`; alasan server mendahului cabang `bedStatusName` |
| 3 | Tanpa alasan server, kalimat lama tetap dipakai | ✅ | Test `FE-RWI-057 kriteria 3`, termasuk pemanggil dua argumen |
| 4 | Layar tidak menghitung ulang satu pun aturan | ✅ | Test lama "frontend tidak punya aturan kelayakan penempatan versinya sendiri" tetap lulus |
| 5 | Bed yang lolos tidak menampilkan penolakan | ✅ | Test `FE-RWI-057 kriteria 5 dan 6` |
| 6 | Bed terisi dan dipesan tetap menampilkan pemegangnya | ✅ | Test yang sama; urutan langkah 2 dan 3 menjaganya |

---

## 8. Catatan penutup

**Satu godaan yang sengaja tidak diambil.** Kartu tempat tidur kini punya bentuk yang sama
dengan `PlacementFailureList`, dan menggantinya dengan komponen itu akan terlihat lebih
rapi. Itu tidak dilakukan: `PlacementFailureList` merender `InformationAlert` seukuran
modal, dan menaruhnya di dalam kartu pada grid tempat tidur akan membuat papan tidak
terbaca. Kartu memakai `StatusBadge` yang memang sudah dipakai di sana. **Nol komponen
baru dibuat**, sesuai baris Reuse pada kartu roadmap.

**Yang belum tertutup.** Bukti peramban atas tiga keadaan bed — isolasi, sekamar dengan
pasien berjenis kelamin berbeda, dan bed yang dapat dipilih — tetap `NOT RUN`. Ia menunggu
`RWI-UI-GAP-007`, dan itu pekerjaan menyiapkan data, bukan menulis kode.
