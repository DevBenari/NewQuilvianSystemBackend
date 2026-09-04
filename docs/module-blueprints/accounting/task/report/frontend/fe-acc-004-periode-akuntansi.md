# `FE-ACC-004` — Periode akuntansi

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-004` |
| Blueprint | `ACC-BP-001` revisi 10, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task mode | `FRONTEND` |
| Kontrak | `ACC-API-0.3` grup Accounting Period; `ACC-STATE-0.1` bagian 2 |
| Wewenang UI | `ACC-FE-001` pilihan B, `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev`, branch `RizkiV2` @ `1a86d9322` |
| Snapshot backend | `rizkiG` @ `822d48a` — impact scan: **0 berkas** Accounting Period berubah sejak baseline |
| Status | **`IMPLEMENTED` dengan satu acceptance TIDAK DAPAT DIPENUHI** — lihat bagian 3 |
| Tanggal | 4 September 2026 |

## Ringkasan untuk pembaca umum

Manajer akuntansi kini dapat mengelola **kalender pembukuan** lewat layar:

1. **Bangkitkan setahun** — dua belas periode bulanan dibuat sekaligus untuk satu tahun buku.
2. **Tutup sementara** — jurnal umum ditolak, tetapi jurnal penyesuaian dan pembalikan masih
   diterima. Ini masa tenggang tutup buku.
3. **Tutup permanen** — menolak semuanya.
4. **Buka kembali** dengan alasan tertulis yang wajib diisi.

Satu hal yang penting dan mudah salah: **periode yang sudah tutup permanen, bila dibuka kembali,
menjadi Tutup Sementara — bukan Terbuka.** Dengan begitu jurnal operasional baru tidak dapat masuk
ke bulan yang laporannya sudah terbit (`ACC-DEC-028`).

## 1. Kenapa slice-nya ditulis manual

Roadmap menetapkannya, dan alasannya terlihat begitu endpoint-nya dibaca. Periode **tidak
mengikuti bentuk CRUD**: tidak ada `POST /` biasa maupun `PUT /{id}`, melainkan tiga aksi domain —
`POST /generate`, `POST /{id}/close`, `POST /{id}/reopen`, ditambah dua pembacaan `GET /` dan
`GET /current`. Tidak ada `DELETE` sama sekali: periode adalah kerangka pembukuan, bukan data yang
boleh hilang.

Memaksakannya ke `createMasterDataResourceSlice` akan menyembunyikan bentuk sebenarnya di balik
nama-nama CRUD yang tidak cocok.

## 2. Acceptance yang terpenuhi

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Tiga status tampil dengan penanda berbeda dan berlabel Bahasa Indonesia | **TERBUKTI di source** | `PERIOD_STATUS_BADGE`: `Terbuka` hijau, `Tutup Sementara` kuning, `Tutup Permanen` merah. Nilai enumnya disalin apa adanya dari `AccountingPeriodStatus.cs` |
| (2) | Tombol Buka Kembali menampilkan isian alasan, dan tidak dapat dikirim bila kosong | **TERBUKTI di source, belum dilihat berjalan** | `canSubmitDialog` mensyaratkan alasan tidak kosong pada mode `reopen`; tombol Konfirmasi dinonaktifkan selama itu |
| (3) | Setelah membuka kembali periode tutup permanen, layar menampilkan **Tutup Sementara** — dimuat ulang dari backend, bukan ditebak | **TERBUKTI di source, belum dilihat berjalan** | `refreshFromBackend()` dipanggil sesudah **setiap** aksi. Layar tidak pernah menghitung status berikutnya sendiri |

Butir (3) adalah yang paling penting secara rancangan, dan roadmap sendiri menandainya:
*"menguji bahwa frontend tidak menyimpulkan status sendiri"*. Bila layar menebak, ia akan
menampilkan `Terbuka` sesudah membuka kembali periode `Tutup Permanen` — dan itu salah.

## 3. Acceptance (4) — `TIDAK DAPAT DIPENUHI` dengan kemampuan platform saat ini

> **(4) Tombol tutup dan buka kembali hanya muncul bagi pemegang haknya.**

Ini **tidak dikerjakan**, dan saya tidak memalsukannya.

| Yang diperiksa | Hasil |
|---|---|
| Mekanisme hak akses sisi klien di repository frontend | **Tidak ada.** Nol `usePermission`, `hasPermission`, atau `canManage` |
| Data hak akses pada state autentikasi | **Tidak ada.** `login-slice.jsx` hanya menyimpan `role`, bukan daftar `Resource : Action` |
| Endpoint "hak akses milik saya" di backend | **Tidak ada** |
| Yang ada sebagai gantinya | `AccessDeniedGate`, yang bereaksi **sesudah** backend menolak `403` |

Menebak pemetaan nama peran ke `AccountingPeriod : Close`/`Reopen` di layar berarti mengarang
aturan otorisasi — dilarang, dan rapuh: pemetaannya milik data peran di backend, bukan konstanta
frontend.

**Preseden yang tepat sudah ada di modul ini.** `JournalDetailResponse` memuat `AvailableActions`
yang **dihitung backend** dari status jurnal, hak akses, dan aturan `ACC-DEC-016`; frontend
menampilkan tombol berdasarkan daftar itu, bukan menghitung sendiri. `AccountingPeriodResponse`
belum punya padanannya.

| Hal | Keterangan |
|---|---|
| Cara menutup | Tambahkan `AvailableActions` pada `AccountingPeriodResponse`, mengikuti pola `JournalDetailResponse`. Pekerjaan backend kecil, tetapi **di luar scope task frontend ini** dan menuntut persetujuan owner |
| Alternatif | Platform menyediakan endpoint hak akses pengguna berjalan — ini menyelesaikan masalah yang sama untuk seluruh modul, bukan hanya Accounting |
| Sementara ini | Tombolnya **tampil untuk semua**, dan pengguna tanpa hak menerima penolakan `403` dari backend beserta pesannya. **Keamanannya tidak berkurang** — backend tetap satu-satunya penegak; yang berkurang hanya kenyamanan |
| Diusulkan dicatat sebagai | `ACC-GAP-010` pada `UTANG-TEKNIS.md` |

## 4. Berkas

| Lapisan | Berkas | Baris |
|---|---|---:|
| Slice (manual) | `src/lib/state/slice/corporate/accounting/accounting-period-slice.jsx` | 231 |
| Konstanta | `src/lib/constants/corporate/accounting/accounting-period/accounting-period-constants.jsx` | 114 |
| Hook | `src/lib/hooks/corporate/accounting/accounting-period/use-accounting-period.jsx` | 238 |
| View | `src/components/view/corporate/accounting/accounting-period/accounting-period-view.jsx` | 312 |
| Gaya | `src/style/corporate/accounting/accounting-period-view.module.css` | 46 |
| Rute | `src/app/corporate/accounting/periods/` — 2 berkas | 14 |

## 5. Validasi

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| `npm run build` | **PASS**, exit 0 |
| Unit test 434 | **PASS**, 0 gagal |
| Rute ter-build | `/corporate/accounting/periods` ada di `app-paths-manifest.json` |

## 6. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | Buka **Akuntansi → Master Data → Periode Akuntansi**, pilih badan hukum | Tabel kosong bila tahun buku itu belum dibangkitkan |
| 2 | Tekan **Bangkitkan Setahun**, isi `2026`, konfirmasi | Dua belas baris muncul, seluruhnya berstatus **Terbuka** |
| 3 | Tekan **Bangkitkan Setahun** lagi untuk `2026` | Ditolak, toast memuat pesan backend tentang tahun buku yang sudah ada |
| 4 | Pada satu periode, tekan **Tutup Sementara** | Statusnya menjadi **Tutup Sementara** berpenanda kuning |
| 5 | Pada periode itu, tekan **Tutup Permanen** | Statusnya menjadi **Tutup Permanen** berpenanda merah |
| 6 | **Inti acceptance (2).** Tekan **Buka Kembali**, biarkan alasan kosong | Tombol Konfirmasi **tidak dapat ditekan** |
| 7 | **Inti acceptance (3).** Isi alasan, konfirmasi | Statusnya menjadi **Tutup Sementara** — **bukan Terbuka**. Inilah bukti layar memuat ulang dari backend, bukan menebak |
| 8 | Saring status **Terbuka** saja | Hanya periode terbuka yang tampil |
| 9 | Ubah tahun buku ke `2027` | Tabel kosong, karena `2027` belum dibangkitkan |

Langkah 7 adalah yang paling bermakna: bila suatu saat layar diubah menjadi menebak status, hasil
langkah itu berubah menjadi **Terbuka**, dan itu langsung terlihat.

## 7. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| **Acceptance (4) tidak dipenuhi** | **Sedang** | Bagian 3. Bukan lubang keamanan — backend tetap menolak `403` — tetapi pengguna tanpa hak melihat tombol yang akan gagal |
| Acceptance (2) dan (3) belum dilihat berjalan | Sedang | Skrip bagian 6 langkah 6 dan 7 menutupnya |
| Daftar dibatasi 100 baris tanpa paginasi | Rendah | Satu tahun buku berisi dua belas periode; seratus baris menampung delapan tahun sekaligus. Paginasi ditambahkan bila kelak dibutuhkan |
| Nol test otomatis | Sedang | Sama seperti dua task lain di gelombang ini |

## 8. Langkah berikutnya

Putuskan `ACC-GAP-010`: menambahkan `AvailableActions` pada `AccountingPeriodResponse` seperti
`JournalDetailResponse`, atau menerima bahwa tombolnya tampil untuk semua dan mencatatnya sebagai
utang. Keduanya keputusan owner.
