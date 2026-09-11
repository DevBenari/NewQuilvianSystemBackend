# `FE-ACC-003` — Master jenis jurnal

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-003` |
| Blueprint | `ACC-BP-001` revisi 10, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task mode | `FRONTEND` |
| Kontrak | `ACC-API-0.3` grup Journal Type |
| Wewenang UI | `ACC-FE-001` pilihan B, `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev`, branch `RizkiV2` @ `1a86d9322` |
| Snapshot backend | `rizkiG` @ `822d48a` — **2 berkas berubah** sejak baseline, keduanya milik task ini |
| Status | **`IMPLEMENTED`** — menunggu verifikasi manual owner di peramban |
| Tanggal | 4 September 2026 |

## Ringkasan untuk pembaca umum

Administrator dapat mengatur **jenis jurnal dan awalan nomornya** lewat layar. Awalan itu bukan
hiasan: ia membentuk nomor jurnal `{awalan}/{tahun}/{bulan}/{urutan}`.

Empat jenis bawaan sudah terisi di database sejak 3 September 2026 — `JU` Jurnal Umum,
`JP` Jurnal Penyesuaian, `JB` Jurnal Pembalik, dan `SA` Saldo Awal. Dua di antaranya, `JB` dan
`SA`, **bertanda sistem**: kode dan awalan nomornya terkunci karena dipakai langsung oleh proses
pembalikan dan saldo awal.

## 1. Delta kontrak yang ditemukan impact scan — dan ditangani

Ini satu-satunya dari tiga task yang **terdampak pergeseran source**, dan justru inilah alasan
impact scan wajib dijalankan lebih dahulu.

| Hal | Keterangan |
|---|---|
| Yang berubah di backend | `b00e889` mencabut `RequiresApproval` dari `CreateJournalTypeRequest` dan `UpdateJournalTypeRequest` (`ACC-TD-019` `CLOSED`). `CreateAsync` kini memaksanya selalu `true`; `UpdateAsync` tidak menyentuhnya |
| Yang **belum** berubah | Daftar DTO pada `ACC-API-0.3` masih mencantumkan `RequiresApproval` pada kedua request itu, dan **tidak** mencantumkan `IsActive` yang justru ada di `UpdateJournalTypeRequest` |
| Yang saya ikuti | **Source**, sesuai aturan build: source backend adalah bukti otoritatif atas perilaku as-is |
| Akibatnya bagi layar | Form **tidak** memuat isian wajib-persetujuan. Kolomnya tetap **ditampilkan** di tabel sebagai informasi, karena `JournalTypeResponse` masih mengirimkannya |
| Sudah tercatat sebagai | **`ACC-GAP-004`** pada `testing/readiness-report.md` |

Bila kontrak dibaca apa adanya, form ini akan memuat isian yang backend tolak dan tidak memuat
isian yang backend butuhkan. Delta itu **tidak** saya tutup sendiri di kontrak — menaikkan
`ACC-API` adalah keputusan owner, sama seperti preseden `ACC-TD-013` dan `ACC-TD-014`.

## 2. Permukaan API jenis jurnal lebih sempit daripada master data lain

Diverifikasi langsung di controller `822d48a`:

| Ada | Tidak ada |
|---|---|
| `GET /`, `GET /options`, `POST /`, `PUT /{id}`, `POST /seed` | `GET /{id}`, `DELETE`, `activate`, `deactivate`, `summary`, `filters/metadata` |

Dua akibat langsung, keduanya disengaja dan bukan penyederhanaan sepihak:

1. **Tidak ada halaman rincian.** Roadmap memang hanya meminta "satu layar daftar dan form
   sederhana". Klik dua kali sebuah baris langsung membuka layar perbarui.
2. **Layar perbarui memuat datanya dari daftar**, bukan dari endpoint rincian, karena
   `GET /{id}` tidak ada. Satu permintaan daftar sudah memuat semuanya — jenisnya hanya empat
   bawaan ditambah yang dibuat admin.

Thunk yang tidak punya endpoint **tidak diekspor** dari slice, supaya tidak ada yang memanggilnya
lalu menerima `404` yang membingungkan.

## 3. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Jenis bertanda sistem tidak dapat diubah kode maupun awalan nomornya — tombolnya dinonaktifkan, dan bila tetap dikirim, pesan backend ditampilkan | **TERBUKTI di source, belum dilihat berjalan** | `use-journal-type-editor.jsx` menandai kedua isian `disabled` ketika `isSystemType`, beserta keterangan alasannya. Penolakan `409` backend tetap ditampilkan apa adanya |
| (2) | Slice terdaftar di `store.jsx` | **TERBUKTI** | `accountingJournalType: accountingJournalTypeSlice` |

Penonaktifan isian di layar **bukan pengamanan** — ia hanya mendahului backend supaya petugas
tidak mengetik lalu ditolak. Aturannya tetap ditegakkan `AccJournalTypeService.UpdateAsync`
dengan `409`.

## 4. Berkas

| Lapisan | Berkas | Baris |
|---|---|---:|
| Slice | `src/lib/state/slice/corporate/accounting/accounting-journal-type-slice.jsx` | 71 |
| Konstanta | `src/lib/constants/corporate/accounting/journal-type/journal-type-constants.jsx` | 186 |
| Hook daftar | `src/lib/hooks/corporate/accounting/journal-type/use-journal-type.jsx` | 162 |
| Hook form | `src/lib/hooks/corporate/accounting/journal-type/use-journal-type-editor.jsx` | 291 |
| View daftar | `src/components/view/corporate/accounting/journal-type/journal-type-view.jsx` | 193 |
| View form | `.../journal-type/form/journal-type-form-view.jsx` | 51 |
| Rute | `src/app/corporate/accounting/journal-types/` — 4 berkas | 50 |

Utils dipakai ulang dari `chart-of-account-utils.jsx`; tidak ada berkas utils baru, karena
kebutuhannya sama persis dan menyalinnya justru menghasilkan dua sumber kebenaran.

## 5. Validasi

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| `npm run build` | **PASS**, exit 0 |
| Unit test 434 | **PASS**, 0 gagal |
| Rute ter-build | `/corporate/accounting/journal-types`, `create`, `[slug]/update` — ketiganya ada di `app-paths-manifest.json` |

### Catatan kejujuran tentang proses

Lima berkas task ini — form view dan keempat rutenya — pertama kali tertulis **ke repository yang
salah**. Perintah pembuatannya berjalan dengan direktori kerja shell berada di
`NewQuilvianSystemBackend`, bukan `QuilvianSystemFrontendDev`, sehingga kelimanya mendarat di
`NewQuilvianSystemBackend/src/`. Pemeriksaan `find` pada perintah yang sama menampilkannya seolah
benar, karena jalurnya relatif terhadap direktori kerja yang keliru itu.

Ketahuan saat memeriksa `app-paths-manifest.json` sesudah build: rute `journal-types` tidak muncul
padahal slice, hook, konstanta, dan view daftar-nya utuh. Kelimanya ditulis ulang di repository
yang benar, dan build berikutnya menampilkan kesembilan rute Accounting dengan lengkap. Kelima
berkas nyasar sudah dihapus dari repository backend — seluruhnya untracked, nol berkas terlacak
tersentuh, dan isinya terbukti identik dengan yang di frontend sebelum dihapus.

**Pelajarannya:** `find` relatif tidak membuktikan berkas berada di repository yang dimaksud.
Yang membuktikannya adalah rute muncul di `app-paths-manifest.json` sesudah build, dan
`git status` pada kedua repository.

## 6. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | Buka **Akuntansi → Master Data → Jenis Jurnal** | Empat baris tampil: `JU`, `JP`, `JB`, `SA` |
| 2 | Perhatikan kolom **Bawaan Sistem** | `JB` dan `SA` bertanda **Sistem**; `JU` dan `JP` bertanda **Pengguna** |
| 3 | Perhatikan kolom **Wajib Persetujuan** | Keempatnya **Ya** — ditampilkan sebagai informasi, tanpa isian pengubahnya |
| 4 | **Inti acceptance (1).** Klik dua kali baris `JB` | Layar perbarui terbuka; **Kode** dan **Awalan Nomor** dinonaktifkan beserta keterangan alasannya; **Nama** dan **Aktif** tetap dapat diubah |
| 5 | Ubah nama `JB`, simpan | `200`, kembali ke daftar dengan nama baru |
| 6 | Klik dua kali `JU`, ubah awalan nomor, simpan | `200` — `JU` bukan jenis sistem, jadi boleh |
| 7 | Tambah jenis baru dengan kode `JU` | Ditolak, toast memuat pesan backend tentang kode yang sudah dipakai |
| 8 | Saring **Bawaan sistem** dan **Dibuat pengguna** bergantian | Daftarnya menyusut sesuai pilihan |

## 7. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Acceptance (1) belum dilihat berjalan | Sedang | Skrip bagian 6 langkah 4 menutupnya |
| Layar perbarui memuat 200 baris daftar | Rendah | Konsekuensi ketiadaan `GET /{id}`. Jenis jurnal berjumlah satuan, jadi biayanya kecil. Bila kelak tumbuh, endpoint rincian adalah jawabannya |
| `ACC-GAP-004` masih terbuka | **Sedang** | Frontend berikutnya yang menyusun klien dari kontrak akan tetap tersesat sampai daftar DTO `ACC-API` diperbaiki. Milik owner |

## 8. Langkah berikutnya

Owner meratifikasi `ACC-GAP-004` sehingga `ACC-API` menyusul source, atau lanjut ke `FE-ACC-005`.
