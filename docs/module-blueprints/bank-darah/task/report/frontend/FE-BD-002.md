# Laporan Perubahan Frontend — `FE-BD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-002` |
| Judul | Petugas mengelola order darah, pemenuhan, dan pembatalan |
| Layar | `FE-BD-01` (daftar kerja), `FE-BD-02` (detail), halaman `/create` terpisah |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) kartu `FE-BD-002` |
| Trace | `DEC-BD-055`, `DEC-BD-056`, `DEC-BD-057`, `DEC-BD-058`; `INV-BD-011`, `INV-BD-035`; `BD-CAP-021` |
| Contract version | `v5` — `approved` oleh `Sukmagp` 19 September 2026 |
| Dependency | `BE-BD-003` ✅, `BE-BD-017` ✅, `BE-BD-018` ✅ — nol penahan |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` — `src/**` saja; nol perubahan backend |
| Model | Claude Opus 5 |
| Branch frontend | `sukmagpV2` |
| Commit frontend saat sesi dimulai | `c296f7096` |
| Tanggal | 23 September 2026 |
| Status | 🟡 **SEBAGIAN — 23 September 2026.** Seluruh gap kontrak `v5` (G1–G7) dan pass review gap (G8–G10) tertutup di source, terbukti lewat lint `0 error` dan build `exit 0`. Satu gap ditahan sebagai keputusan pemilik: [`BD-UI-GAP-004`](../../../BD-UI-GAP-004-filter-tanggal-order-darah.md) filter tanggal. Satu gap backend dicatat tanpa workaround: `BloodOrderListDto` tidak membawa pembuat order (bagian 8.1). **Validasi runtime BELUM dijalankan**, sehingga Definition of Done butir "empat keadaan layar digambar" dan seluruh acceptance runtime belum terbukti. Runbook R1–R17 pada bagian 6 menunggu eksekusi pemilik terhadap `QuilvianNewDevSukma` |

---

## 1. Keadaan yang ditemukan di awal

Berbeda dari kartu roadmapnya, **source `FE-BD-002` sudah ada dan sudah ter-commit** sebelum sesi ini:
tiga commit di `sukmagpV2` (`db0a24e27`, `5f1af3669`, `c296f7096`) membawa daftar kerja, halaman buat,
halaman detail, tiga hook, satu service, satu utilitas, dan satu modul CSS. Kartu roadmap masih menulis
"Belum dikerjakan", dan laporan tracked-nya belum pernah dibuat. Berkas ini menutup kekosongan itu.

Audit kesesuaian terhadap kontrak `v5` dijalankan lebih dulu, tanpa menyentuh satu baris source pun.
Hasilnya: **keempat kewajiban kontrak `v5` sudah benar secara pokok** — `NotDisclosed` tidak ditawarkan,
penahanan ganda dibaca dari `errors.code`, kategori pembatalan tidak dihitung layar, dan angka pemenuhan
tidak dihitung ulang. Yang tersisa adalah cacat pinggiran, dan sesi ini memperbaiki yang berada di dalam
scope §1–§5 yang diberi wewenang.

---

## 2. Gap yang diperbaiki

| # | Gap | Keadaan sebelum | Keadaan sesudah |
| :---: | --- | --- | --- |
| G1 | Kalimat "tidak tercatat pada order lama" tampil saat memuat | `requestedBloodGroupLabel` jatuh ke kalimat itu setiap kali labelnya kosong, **termasuk sebelum detail termuat**. Layar menyatakan fakta tentang data padahal datanya belum ada | Kalimat hanya muncul sesudah `state.detail` benar-benar termuat. Sebelum itu barisnya kosong dan dirender `-` |
| G2 | Ringkasan pemenuhan hilang bila payload detail tidak membawanya | `fulfillment` diambil hanya dari `GET /{id}`. Bila slot itu kosong, bagian Pemenuhan Order kosong tanpa jalan pulih | Jalur cadangan ke `GET /{id}/fulfillment` dipasang. **Tetap nol perhitungan di layar** — angkanya tetap sepenuhnya dari backend |
| G3 | Angka pemenuhan dikosongkan sesudah pembatalan | Jawaban `POST /{id}/cancel` yang tidak membawa `fulfillment` menimpa angka lama dengan `null` | Angka terakhir dari backend dipertahankan apa adanya |
| G4 | Komponen bentrok bisa hilang walau ID-nya benar | Nama diambil dari daftar opsi lalu `.filter(Boolean)`. Opsi dimuat berhalaman dan tersaring pencarian, sehingga butir tanpa label **dibuang diam-diam**. Petugas melihat daftar bentrok lebih pendek dari kenyataan, atau kosong | Setiap ID yang cocok tetap tampil; yang labelnya belum termuat ditulis sebagai nomor barisnya |
| G5 | Kategori alasan pembatalan tidak pernah terlihat petugas | `cancellationReasonCategory` hanya dipakai diam-diam sebagai `?category=`. Petugas tidak pernah tahu kategori mana yang sedang berlaku baginya | Kategori ditampilkan di modal pembatalan lewat `InformationAlert`. Penentuannya tetap milik backend |
| G6 | `403` pada katalog komponen terbaca sebagai "katalog kosong" | `GET /blood-components/options` dijaga `BloodComponent : Read`. Penolakan kewenangan tampil sebagai "Komponen darah aktif tidak ditemukan" — pesan yang salah dan menyesatkan | Kesalahan pemuatan katalog ditampilkan jujur di `afterHero`, sejajar perlakuan unit pelayanan |
| G7 | Dua baris golongan darah dapat berdesakan pada satu baris grid | "Golongan Darah Diminta" tidak `fullWidth`, sehingga dapat berbagi baris dengan kolom lain | Keduanya `fullWidth`, tampil sebagai dua pernyataan terpisah yang sejajar dan tidak pernah terbaca sebagai satu nilai |

### 2.1 Pass review gap — 23 September 2026

Tiga perbaikan kecil sesudah implementasi kontrak `v5`, dikerjakan atas permintaan pemilik.

| # | Gap | Keadaan sebelum | Keadaan sesudah |
| :---: | --- | --- | --- |
| G8 | **Tombol Reset penyaring tidak mereset apa pun** | `onReset` tersambung ke `reload`, yang hanya mengambil ulang data dengan penyaring yang sama. `resetFilters` ada tetapi **tidak pernah dipakai**. Label tombolnya pun sudah ditulis ulang menjadi "Muat ulang data", sehingga penyimpangannya tersamar. Penyaring tidak punya jalan dibersihkan | `onReset` tersambung ke `resetFilters`. Seluruh penyaring kembali ke bawaan — `startDate`, `endDate`, periode, pencarian, komponen, dan status kosong; halaman kembali `1` — lalu data diambil ulang. Label kembali ke bawaan `data-filter`: "Atur ulang filter" |
| G9 | **Teks pencarian tidak ikut bersih saat reset** | Kotak pencarian `DataFilter` memegang nilainya sendiri dan **tidak pernah disinkronkan turun** dari prop. Mereset state penyaring saja meninggalkan kata kunci lama tetap terlihat, sementara hasilnya sudah tidak tersaring — dua pernyataan yang bertentangan di satu layar | `DataFilter` dipasang ulang lewat React `key` yang berubah saat reset, sehingga seluruh kontrol terbaca kembali dari nilai bawaan. Perbaikan **setempat**: `data-filter` yang dipakai belasan layar lain tidak disentuh |
| G10 | **Kolom "Dibuat Oleh" permanen berisi `-`** | Kolom mencoba sepuluh nama kunci; **nol** di antaranya ada pada `BloodOrderListDto` | Kolom **dihapus**, bukan dialihkan ke isian lain. Alasannya pada bagian 8.1 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Alasan diperiksa |
| --- | --- |
| `contracts/api-contract.md` Amendment `v5` D1–D4 | Bentuk kanonik seluruh isian baru |
| `contracts/permission-audit-matrix.md` bagian 2 | Ketergantungan antarbutir hak akses |
| `contracts/validation-matrix.md` | Kalimat dan status `VAL-BD-001`, `VAL-BD-013`, `VAL-BD-083`, `VAL-BD-085` |
| `task/report/backend/BE-BD-017.md`, `BE-BD-018.md` | Keadaan backend yang sebenarnya tersedia |
| `Enums/BloodType.cs` | Kesamaan kata per kata daftar golongan darah |
| `Enums/BbkBloodOrderStatus.cs` | Nilai status dan labelnya |
| `DTOs/BloodOrderDtos.cs` | Isian yang benar-benar ada pada daftar dan detail |
| `Controllers/BbkBloodOrderController.cs` | Parameter query yang benar-benar diterima; pemetaan slot `errors` |
| `Controllers/BbkBloodGroupExamController.cs` | Route dan butir hak akses golongan darah sah |
| `Controllers/BloodBankReasonController.cs` | Butir hak akses `GET /options` |
| `base-features/` (11 komponen) | Gerbang keputusan reuse |

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Gap |
| --- | --- | --- |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-order-detail.jsx` | `readFulfillmentFallback` baru; `fulfillment` dipertahankan sesudah pembatalan; `requestedBloodGroupLabel` hanya bicara sesudah detail termuat; kategori pembatalan beserta labelnya diekspor | G1, G2, G3, G5 |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-order-form.jsx` | `conflictingComponentNames` tidak lagi membuang butir tanpa label | G4 |
| `src/lib/constants/health-services/blood-bank-management/blood-order-constants.jsx` | `CANCELLATION_REASON_CATEGORY_LABEL` baru — **penerjemah tampilan, bukan penghitung kategori** | G5 |
| `src/components/view/health-services/blood-bank-management/blood-orders/detail/blood-order-detail-view.jsx` | Dua baris golongan darah `fullWidth` dan disanitasi; kategori pembatalan tampil di modal | G5, G7 |
| `src/components/view/health-services/blood-bank-management/blood-orders/form/blood-order-form-view.jsx` | Kesalahan pemuatan katalog komponen ditampilkan | G6 |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-order-list.jsx` | `resetFilters` mengembalikan seluruh penyaring ke bawaan **lalu** mengambil ulang data; `filterResetKey` diekspor; `reload` yang tidak lagi dipakai dibuang | G8, G9 |
| `src/components/view/health-services/blood-bank-management/blood-orders/blood-order-list-view.jsx` | `onReset` → `resetFilters`; `key` pada `DataFilter`; label tombol kembali ke bawaan | G8, G9 |
| `src/components/view/health-services/blood-bank-management/blood-orders/blood-order-table-columns.jsx` | Kolom "Dibuat Oleh" dan `CREATED_BY_KEYS` dihapus; alasannya ditulis sebagai catatan di berkas | G10 |

Total **8 berkas**. **Nol berkas source backend disentuh. Nol migration. Nol database.**

### 3.3 Gerbang keputusan base component

| Kebutuhan UI | Kandidat base | Status | Keputusan |
| --- | --- | --- | --- |
| Peringatan kategori pembatalan | `information-alert` | `REUSE` | Dipakai apa adanya, varian `info` |
| Peringatan katalog komponen gagal dimuat | `information-alert` | `REUSE` | Dipakai apa adanya, varian `warning` |
| Baris golongan darah pada detail | `base-detail-view` / `base-detail-card` | `REUSE` | `forceShow` + `fullWidth` yang sudah didukung |

**Nol komponen `base-features/` baru dibuat. Nol komponen dasar tandingan.** Ketujuh base component yang
diwajibkan audit terpakai seluruhnya: `hero`, `data-filter`, `data-table`, `base-detail-view`,
`base-grouped-editor-view`, `status-badge`, `information-alert`.

---

## 4. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/blood-orders` | Daftar kerja: `components[]`, `totalRequestedQuantity`, `totalIssuedQuantity`, saringan `bloodComponentId` | `BloodOrder : Read` |
| `GET` | `/blood-orders/summary` | Kartu ringkasan | `BloodOrder : Read` |
| `GET` | `/blood-orders/filters/metadata` | Opsi status dan ukuran halaman | `BloodOrder : Read` |
| `GET` | `/blood-orders/{id}` | Detail: `requestedBloodGroupLabel`, `cancellationReasonCategory`, `fulfillment`, `transitions` | `BloodOrder : Read` |
| `GET` | `/blood-orders/{id}/fulfillment` | **Jalur cadangan** ringkasan pemenuhan | `BloodOrder : Read` |
| `POST` | `/blood-orders` · `/manual` · `/confirm-duplicate` | Pembuatan order, termasuk `requestedBloodGroup` wajib | `BloodOrder : Create` |
| `POST` | `/blood-orders/{id}/cancel` | Pembatalan dengan `reasonCode` + `version` | `BloodOrder : Cancel` |
| `GET` | `/blood-group-exams/patient/{patientId}/valid` | Golongan darah **hasil pemeriksaan**, terpisah penuh dari yang diminta | `BloodGroupExam : Read` |
| `GET` | `/master-data/blood-bank-reasons/options?category=` | Daftar alasan sesuai kategori dari backend | `BloodBankReason : Read` |
| `GET` | `/master-data/blood-components/options` | Katalog komponen | `BloodComponent : Read` |
| `GET` | `/master-data/service-units/options` | Unit pelayanan berwenang | mengikuti butir master unit |

**`GET /blood-orders/{id}/status-history` tidak dipanggil** — riwayat sudah dibawa payload detail.

---

## 5. Verifikasi

| Perintah / pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` atas seluruh scope Bank Darah order darah | **Nol keluaran — `0 error`** | `PASS` |
| `npm run build` | Lihat bagian 5.1 | `PASS` |
| Nol parsing kalimat pesan untuk deteksi ganda | Pencarian `message.includes` / `indexOf` pada seluruh scope: **nol hasil** | `PASS` |
| Nol perhitungan ulang angka pemenuhan | Pencarian `reduce` atas quantity pada seluruh scope: **nol hasil**. Daftar memakai `totalRequestedQuantity`/`totalIssuedQuantity`, detail memakai `fulfillment` dari backend | `PASS` |
| Nol perhitungan kategori pembatalan di layar | Pencarian pembandingan `requestingDoctorId` terhadap pengguna yang login: **nol hasil** | `PASS` |
| Nol hardcode peran | Pencarian `SuperAdmin` / `role` / `isAdmin` pada seluruh scope: nol hasil di luar atribut ARIA `role="alert"` | `PASS` |
| `NotDisclosed` (`99`) tidak pernah ditawarkan | `BLOOD_TYPE_OPTIONS` memuat tepat sembilan nilai `0`–`8`, identik kata per kata dengan `Display(Name=...)` pada `BloodType.cs` | `PASS` |
| Tombol Batalkan tidak tampil tanpa kewenangan | Dijaga tiga syarat sekaligus: `BloodOrder : Cancel`, `BloodBankReason : Read`, dan kategori tidak kosong dari backend | `PASS` |
| Keputusan kewenangan bersifat ketat | Memakai `decide()` yang memulangkan `allowed`/`denied`/`unknown`; hanya `allowed` menampilkan. Bukan `usePermission` yang punya jaring "belum diketahui berarti boleh" | `PASS` |

### 5.1 Hasil build

`npm run build` dijalankan 23 September 2026 dari `V2QuilvianSystemFrontendDev`, log penuh disimpan.
Next.js `16.2.12` (Turbopack).

Build terakhir dijalankan **sesudah pass review gap** (G8–G10).

```
✓ Compiled successfully in 37.4s
  Running TypeScript ...
  Finished TypeScript in 375ms ...
✓ Generating static pages using 15 workers (367/367) in 8.5s
[exited with code 0]
```

| Pemeriksaan | Hasil |
| --- | --- |
| Exit code | **`0`** |
| Kompilasi | **`✓ Compiled successfully in 37.4s`** |
| TypeScript | Selesai `375ms`, nol keluhan |
| Halaman statis | **367 / 367** dibangkitkan |
| Peringatan atau kesalahan pada log | **Nol.** Pencarian `warning`/`error` pada log penuh hanya memulangkan route `/error-page` — sebuah nama halaman, bukan diagnostik |
| Ketiga route order darah terbangun | `○ /health-services/blood-bank-management/blood-orders` · `ƒ .../blood-orders/[slug]` · `○ .../blood-orders/create` |
| `postbuild` | `prepare-standalone` berhasil menyalin static dan public assets |

Build dijalankan **tiga kali** sepanjang pekerjaan ini: dua kali pada pass implementasi kontrak `v5`
(yang pertama lognya terpotong karena dipipa ke `tail`, sehingga diulang dengan log penuh — keduanya
exit `0`, kompilasi `37.8s`), dan sekali lagi pada pass review gap, yang angkanya tercatat di atas.
Ketiganya exit code `0`.

`npx eslint --quiet` atas seluruh scope Bank Darah order darah dijalankan ulang sesudah G8–G10:
**nol keluaran, `0 error`.**

---

## 6. Runbook runtime — BELUM DIEKSEKUSI

> **Status: belum dijalankan.** Tidak ada satu pun skenario di bawah yang sudah dibuktikan pada aplikasi
> berjalan. Agent tidak dapat menjalankannya: dibutuhkan aplikasi hidup, sesi login, dan dua aktor
> non-SuperAdmin. Sampai bagian ini terisi, `FE-BD-002` **tidak boleh** dinyatakan selesai.

Seluruh skenario dijalankan terhadap **`QuilvianNewDevSukma`** — satu-satunya database yang migrasi
`BE-BD-017` sudah terterapkan.

**Aktor yang dibutuhkan:** aktor **A** yang tertaut ke dokter peminta order; aktor **B** pemegang
`BloodOrder : Cancel` yang bukan dokter peminta. Keduanya non-SuperAdmin.

| # | Skenario | Yang diharapkan |
| :---: | --- | --- |
| R1 | Buka daftar order darah | Kolom Komponen Darah terisi dari `components[]`; kolom "Diminta / Diberikan" terisi dari `totalRequestedQuantity` / `totalIssuedQuantity` |
| R2 | Saring dengan satu komponen | Hanya order yang punya baris komponen itu yang muncul; paging tetap berlaku |
| R3 | Buat order elektronik, golongan darah diminta **A Positif** | `200`; layar berpindah ke detail |
| R4 | Periksa daftar pilihan golongan darah | Tepat sembilan pilihan; **"Tidak diinformasikan" tidak ada**; "Tidak diketahui" ada |
| R5 | Kosongkan golongan darah diminta lalu simpan | Ditahan layar; bila dipaksa lewat, backend menolak `400 VAL-BD-085` dan pesannya tampil |
| R6 | Buka detail order R3 | Dua baris terpisah: "Golongan Darah Diminta: A Positif" dan "Golongan Darah Hasil Pemeriksaan (Sah)". **Tidak pernah satu kolom gabungan** |
| R7 | Pakai pasien yang hasil pemeriksaannya **B Positif**, order meminta A Positif | Baris pertama A Positif, baris kedua B Positif. Keduanya berbeda dan berlabel berbeda |
| R8 | Buka order yang dibuat **sebelum** 21 September 2026 | "Golongan darah diminta tidak tercatat pada order lama" — **bukan** "Tidak diketahui", dan **tidak** muncul saat layar masih memuat |
| R9 | Buat order PRC + trombosit untuk kunjungan yang sudah punya order PRC aktif | Modal "Order serupa masih aktif"; **komponen bentrok hanya PRC**; alasan wajib diisi; Lanjutkan tersimpan |
| R10 | Ulangi R9 dengan kotak pencarian komponen disaring sampai PRC hilang dari opsi | Komponen bentrok **tetap tampil** (sebagai nama atau nomor baris) — tidak hilang |
| R11 | Aktor **A** membuka detail lalu Batalkan | Modal menampilkan "Alasan klinis — berlaku bagi dokter peminta order ini"; daftar alasan berisi kategori klinis |
| R12 | Aktor **B** membuka detail yang sama lalu Batalkan | Modal menampilkan "Alasan operasional — berlaku bagi petugas Bank Darah"; daftar alasan berisi kategori operasional |
| R13 | Order yang sudah dibatalkan dibuka lagi | Tombol Batalkan **tidak tampil** |
| R14 | Aktor tanpa `BloodOrder : Create` membuka `/create` | Form nonaktif dan peringatan kewenangan tampil; tombol Tambah tidak tampil di daftar |
| R15 | Empat keadaan layar daftar dan detail | Kosong, memuat, berisi, dan gagal — keempatnya tergambar (DoD kartu) |
| R16 | Isi pencarian, pilih komponen, pilih status, pindah ke halaman 2, lalu tekan tombol Atur Ulang | **Seluruh** penyaring kembali kosong — termasuk **teks pencarian yang terlihat** —, halaman kembali `1`, dan daftar diambil ulang **satu kali** |
| R17 | Perhatikan kolom daftar kerja | **Tidak ada kolom "Dibuat Oleh".** Kolom yang ada: No, Tanggal Dibuat, Kode Order, Nama Pasien, Unit Pelayanan, Komponen Darah, Diminta / Diberikan, Status |

Kirim balik hasil per baris beserta status HTTP dan nomor order yang terbit. Laporan ini diperbarui apa
adanya dari hasil itu.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria kartu | Status | Bukti |
| --- | --- | --- |
| Order ganda tertahan beserta alasannya | 🟡 **Terbukti struktur, runtime belum** | `errors.code === "VAL-BD-001"` dibaca dari slot terstruktur; nol parsing kalimat; `duplicateComponentIds` kini tidak pernah dibuang diam-diam; alasan override wajib. Runtime: R9, R10 |
| Kategori alasan pembatalan **sesuai peran** | 🟡 **Terbukti struktur, runtime belum** | Kategori dibaca utuh dari `cancellationReasonCategory`; nol aturan kepemilikan disalin ke layar; kategori kini juga ditampilkan. Runtime: R11, R12, R13 |
| **Kewajiban layar `FE-BD-001`** — golongan darah **diminta** terlihat jelas berbeda dari **hasil pemeriksaan** | 🟡 **Terbukti struktur, runtime belum** | Dua baris terpisah, dua sumber terpisah, dua label berbeda, keduanya `forceShow` dan `fullWidth`. `INV-BD-011` dihormati: nilai yang diminta tidak pernah dibaca sebagai fakta klinis. Runtime: R6, R7, R8 |

### Definition of Done

| Butir | Status |
| --- | --- |
| Empat keadaan layar digambar: kosong, memuat, berisi, gagal | 🟡 **Belum terbukti** — tersedia di source (`emptyTitle` / `loadingText` / `InformationAlert` / `AccessDeniedGate`), belum pernah dilihat berjalan. Runtime: R15 |
| Sumber data terkunci pada endpoint kontrak | ✅ Seluruh permintaan menembak endpoint kontrak `v5`; nol endpoint karangan |
| Dilarang membuat komponen dasar tandingan | ✅ Nol komponen `base-features/` baru |
| Laporan tracked `task/report/frontend/FE-BD-002.md` | ✅ Berkas ini |
| Lint bersih | ✅ `0 error` |
| Build berhasil | ✅ Lihat bagian 5.1 |

**Definition of Done BELUM terpenuhi.** Penahan tunggalnya adalah bukti runtime.

---

## 8. Yang sengaja TIDAK dikerjakan

| Temuan | Akibat bagi pengguna | Mengapa ditunda |
| --- | --- | --- |
| **Filter tanggal dan periode tidak pernah sampai ke backend** | Petugas memilih "Hari Ini", daftar tidak berubah, nol pesan. **Kontrol yang berbohong** | **Diputuskan Opsi B** 23 September 2026 — backend menambahkan penyaringnya lewat [`BE-BD-019`](../backend/BE-BD-019.md). Lihat bagian 8.2 |
| **Berkas CSS kembar yang mati** di `src/utils/.../blood-orders/` | Nol bagi pengguna; kebersihan repo | Penghapusan berkas di luar scope perbaikan gap |

Selain itu, `duplicateOverrideReason`, `duplicateOverrideAt`, dan `expiredAt` tersedia pada
`BloodOrderDetailDto` tetapi belum ditampilkan di detail. Order yang dilanjutkan walau ganda masih
terlihat identik dengan order biasa.

### 8.1 GAP BACKEND — `BloodOrderListDto` tidak membawa pembuat order

**Status: gap backend terbuka. Frontend sudah menyesuaikan diri; nol workaround dipasang.**

Audit isian pembuat pada jalur daftar kerja:

| Yang diperiksa | Hasil |
| --- | --- |
| `BloodOrderListDto` | **Nol isian pembuat.** Tidak `createByName`, tidak `createBy`, tidak `createdByName` |
| Entity `BbkBloodOrder` | Mewarisi `CreateBy` dari `IdentityModel` — **datanya ada di database**, tetapi berupa `Guid` tanpa nama |
| Proyeksi daftar pada `BbkBloodOrderService` | `CreateBy` **tidak pernah** diproyeksikan ke DTO daftar |
| `BloodOrderDetailDto` | Punya `InputByUserId`, juga `Guid` tanpa nama, dan hanya terisi pada order manual |
| Preseden di repository | Pola baku sudah ada dan dipakai luas: `CreateByName = GetActorName(actorNames, entity.CreateBy)` — misalnya `BankController`, `CompanyGuarantorController`, `IdentityScannerProfileController` |

**Keputusan frontend: kolom dihapus, bukan dipetakan ke isian lain.**

Pemetaan ke `requestingDoctorName` sempat menjadi kandidat dan **ditolak**. Dokter peminta dan pembuat
order adalah **dua orang yang berbeda** pada order manual: ordernya datang di kertas dari dokter
peminta, lalu diinput petugas Bank Darah — justru itu sebabnya `InputByUserId` ada dan `VAL-BD-010`
mewajibkannya. Menaruh nama dokter di bawah judul "Dibuat Oleh" akan menyatakan sesuatu yang tidak benar
tentang siapa yang memasukkan data, tepat pada layar yang dipakai menelusuri jejak. Kolom kosong
permanen buruk; kolom yang **salah** lebih buruk.

**Yang dibutuhkan dari backend bila kolom ini memang diinginkan:** satu isian `createByName` pada
`BloodOrderListDto`, diisi dengan pola `GetActorName` yang sudah baku. Aditif, nol klien lama rusak.
Belum dibuka sebagai task; menunggu pemilik menyatakan kolomnya memang dibutuhkan.

### 8.2 Filter tanggal — DIPUTUSKAN Opsi B, 23 September 2026

`BD-UI-GAP-004` **ditutup pemilik dengan Opsi B**: penyaring tanggal **dipertahankan**, dan backend yang
menyesuaikan diri. Ditulis sebagai `DEC-BD-059` dan `DEC-BD-060` pada
[`00-interview-decisions.md`](../../../00-interview-decisions.md) bagian 8.34.

| Butir | Isi |
| --- | --- |
| Kolom yang disaring | `BbkBloodOrder.CreateDateTime` |
| Parameter | `startDate`, `endDate` |
| Tempat penyaringan | Server-side; frontend tidak pernah menyaring tanggal secara lokal |
| `endDate` | Inklusif sampai akhir hari |
| Zona waktu | `Asia/Jakarta`, mengikuti `AppDateTimeHelper` |

Pelaksananya task **[`BE-BD-019`](../backend/BE-BD-019.md)** — Blood Order Date Range Filter,
✅ **SELESAI dan terbukti runtime 23 September 2026**. `GET /blood-orders` kini menerima `startDate` dan
`endDate`, menyaring `CreateDateTime` dengan batas yang dikonversi dari waktu Jakarta ke UTC, dan menolak
rentang terbalik `400 VAL-BD-086`.

**Yang masih harus dikerjakan `FE-BD-002` sesudah `BE-BD-019` terbukti runtime:** kedua parameter itu
sudah dikirim layar apa adanya, jadi penyaringnya akan langsung berfungsi — tetapi dua hal belum
ditangani. **(1)** Kegagalan `400 VAL-BD-086` belum punya penanganan khusus; ia akan tampil lewat jalur
error daftar yang umum. **(2)** Dropdown periode dihitung di browser memakai zona waktu perangkat,
sedangkan backend menafsirkannya sebagai tanggal `Asia/Jakarta` — pada perangkat non-WIB rentangnya
bergeser satu hari. Keduanya dicatat sebagai risiko R3 pada laporan `BE-BD-019` dan belum diputuskan.

**Akibatnya bagi `FE-BD-002`:** kartu ini memperoleh dependency **parsial** pada `BE-BD-019`. Yang
tertahan **hanya** penyaring tanggal pada `FE-BD-01`. Seluruh bagian lain — pembuatan order, golongan
darah diminta, penahanan ganda, kategori pembatalan, pemenuhan, daftar kerja, dan pembatalan — **tidak
tertahan**, dan runbook bagian 6 tetap dapat dijalankan penuh sekarang kecuali skenario penyaring
tanggal.

Sampai `BE-BD-019` selesai, **kontrol tanggal tetap tayang dan tetap tidak berfungsi.** Nol perubahan
frontend dikerjakan untuk ini; penyambungannya menunggu backend siap.

---

## 9. Risiko tersisa

| # | Risiko | Keterangan |
| :---: | --- | --- |
| 1 | **Nol bukti runtime** | Seluruh acceptance bersandar pada pembacaan source. Preseden `FE-BD-001` menunjukkan cacat simpan yang lolos lint, uji unit, dan build, lalu baru tertangkap ketika layarnya benar-benar dijalankan |
| 2 | **Rilis ke lingkungan yang belum dimigrasi = layar order darah mati total** | Kolom `RequestedBloodGroup` baru ada di `QuilvianNewDevSukma`. Di `QuilvianNewDevTim01`, staging, dan production, **setiap** pembuatan order ditolak `400 VAL-BD-085`. Risiko tertinggi dalam daftar ini, dan sudah tercatat `BE-BD-017` bagian 7 butir (3) |
| 3 | **Nol unit test** | Empat kewajiban `v5` yang paling mudah rusak diam-diam tidak satu pun terkunci uji: pembacaan `errors.code`, penolakan `NotDisclosed`, kategori dari backend, nol perhitungan ulang pemenuhan |
| 4 | **`BLOOD_TYPE_OPTIONS` adalah salinan enum backend** | Tidak ada endpoint yang memulangkan pilihan `BloodType`, jadi menyalin adalah satu-satunya jalan. Hari ini identik kata per kata; nol penjaga yang menjaganya tetap begitu |
| 5 | **Kegagalan aksi dapat menelan seluruh halaman detail** | `AccessDeniedGate` menerima `actionError` dan mencocokkan teks. Kata "hak akses" atau "akses ditolak" pada pesan aksi apa pun akan mengganti seluruh detail dengan layar Akses Ditolak. Pola ini dipakai layar lain, jadi bukan cacat khas task ini |
| 6 | **Ketergantungan hak akses belum tertulis di matriks** | `BloodOrder : Create` menuntut `BloodComponent : Read`, setara dengan `Cancel` → `BloodBankReason : Read` yang sudah ditetapkan `DEC-BD-057`. Sesudah G6 akibatnya terbaca jujur di layar, tetapi kontraknya belum mencatatnya |
| 7 | **`AC-BD-112` bersifat potret sesaat** | Kebijakan akses baru yang memberi `BloodOrder : Cancel` tanpa `BloodBankReason : Read` akan memunculkan lagi wewenang yang tidak dapat dipakai. Nol penjaga otomatis |
| 8 | **Jalur cadangan pemenuhan menambah satu request** ketika dipakai | Hanya terpicu bila payload detail tidak membawa `fulfillment` — jalur pinggir. Pada jalur normal jumlah request tidak bertambah |
| 9 | **Reset penyaring memasang ulang `DataFilter`** | Konsekuensinya `ResourceFilterSelect` komponen darah ikut terpasang ulang dan dapat mengambil ulang opsinya. Hanya terjadi pada penekanan tombol Atur Ulang yang disengaja pengguna, dan pengambilan ulang memang yang diharapkan di situ. Akar masalahnya ada di `data-filter` — kotak pencarian tidak pernah sinkron turun dari prop — dan **sengaja tidak diperbaiki di sana**, karena komponen itu dipakai belasan layar lain di luar Bank Darah |
| 10 | **Kolom pembuat order hilang sampai backend menyediakannya** | Petugas kehilangan cara menelusuri siapa yang memasukkan order dari daftar kerja; jejaknya masih ada di database (`CreateBy`) dan di riwayat status, tetapi tidak terbaca dari layar. Dipilih sadar, karena kolom yang salah lebih berbahaya daripada kolom yang tidak ada. Lihat bagian 8.1 |

---

## 10. Status Git

| Hal | Isi |
| --- | --- |
| Branch | `sukmagpV2` |
| Commit saat sesi dimulai | `c296f7096` |
| Berkas berubah | **8 berkas, +202 / −47 baris**, seluruhnya di `V2QuilvianSystemFrontendDev/src/**` |
| Stage / commit / push | **Nol.** Perubahan dibiarkan di working tree untuk ditinjau pemilik |
| Berkas backend | **Nol source disentuh.** Dua berkas dokumen baru dan belum ter-track: laporan ini dan [`BD-UI-GAP-004`](../../../BD-UI-GAP-004-filter-tanggal-order-darah.md) |

---

## 11. Langkah berikutnya

1. Pemilik menjalankan runbook bagian 6 terhadap `QuilvianNewDevSukma`, lalu mengirim balik hasilnya.
2. Laporan ini diperbarui apa adanya; status naik ke ✅ hanya bila R1–R17 seluruhnya `PASS`.
3. Pemilik memutuskan [`BD-UI-GAP-004`](../../../BD-UI-GAP-004-filter-tanggal-order-darah.md):
   **Opsi A** hapus kontrol tanggal, atau **Opsi B** buka task backend penyaring tanggal.
4. Pemilik menyatakan apakah kolom pembuat order memang dibutuhkan di daftar kerja. Bila ya, satu isian
   `createByName` ditambahkan ke `BloodOrderListDto` dengan pola `GetActorName` yang sudah baku
   (bagian 8.1).
5. Kartu `FE-BD-002` pada `roadmap/frontend-roadmap.md` diperbarui, beserta `frontend_source_sha`;
   `BD-UI-GAP-004` didaftarkan ke `open_ui_gaps` sesudah diputuskan.
6. Ketergantungan `BloodOrder : Create` → `BloodComponent : Read` dicatat di
   `contracts/permission-audit-matrix.md` bagian 2.
