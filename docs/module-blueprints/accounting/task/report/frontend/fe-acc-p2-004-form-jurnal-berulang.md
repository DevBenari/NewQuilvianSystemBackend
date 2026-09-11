# Laporan Perubahan Frontend — `FE-ACC-P2-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-004` |
| Judul | Form Jurnal Berulang |
| Slice | `P2-3` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-004` |
| Trace | `ACC-DEC-050`, `ACC-DEC-019`; `FR-P2-018` |
| Contract version | `ACC-API-0.9` — `approved`; amandemen `ACC-API-0.10` (usulan) tidak mengubah `POST /` maupun `PUT /{id}` |
| Wewenang UI | Layar anak `FE-ACC-P2-003`: tambah dan ubah template |
| Dependency | `BE-ACC-P2-007` — `DONE`; `FE-ACC-P2-003` — dikerjakan satu paket |
| Klasifikasi | `MEDIUM` — satu form dengan baris dinamis, memakai ulang baris Form Jurnal |
| Task mode | `CROSS-REPO` — source di frontend, laporan di backend |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, dan laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `460f717a0` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `968841e` (branch `rizkiG`) |
| Tanggal | 11 September 2026 |
| Status | Selesai di kode. `UAT-P2-13` diserahkan kepada Rizki |

---

## 1. Keadaan yang ditemukan di awal

Kartu ini punya satu larangan tegas: **komponen tabel baris dari Form Jurnal MVP wajib dipakai
ulang; membuat tandingan dilarang.** Risikonya ditulis kartu sendiri — menyalin baris membuat dua
tempat yang harus diperbaiki setiap kali aturan keseimbangan berubah.

Yang ditemukan saat memeriksa Form Jurnal:

| Temuan | Akibat bagi task ini |
| --- | --- |
| `JournalLineRow` **sudah diekspor** sejak `FE-ACC-010`, dan sudah dipakai ulang dialog koreksi | Dapat diimpor apa adanya — acceptance (4) terpenuhi tanpa menyentuh Form Jurnal |
| Logika `toCents` dan penghitung keseimbangan **sudah tersalin dua kali**: di `use-journal-editor.jsx` dan `use-journal-reversal.jsx` | Form template tidak boleh menjadi salinan ketiga |
| `accounting-journal-reversal.test.mjs` baris 116–117 **mengunci** letak hitungan itu di dalam hook pembalikan | Menyatukan ketiganya berarti mengubah verifikasi milik task lain |

Karena itu hitungannya dipindah ke **util bersama baru**
`src/utils/corporate/accounting/journal/journal-line-totals-utils.jsx`, dipakai form template.
Kedua hook MVP sengaja dibiarkan; menyatukannya layak menjadi pekerjaan tersendiri.

### Satu cacat yang ditemukan di Form Jurnal MVP

Saat meniru `use-journal-editor.jsx`, ditemukan bahwa penyimpanan ubahan draft jurnal
memanggil `updateJournal({ id, data: payload })`. Factory hanya membaca `{ id, payload }`,
sehingga `sanitizeMutationPayload(undefined)` jatuh ke nilai bawaan `{}` — **`PUT` dikirim dengan
badan kosong.** Dari seluruh hook di repository, hanya berkas itu yang memakai `data:`, dan
skenario uji `FE-ACC-006` tidak pernah mencakup menyimpan ubahan.

Cacat itu **tidak diperbaiki di sini** karena milik `FE-ACC-006`. Yang dilakukan: form template
memakai `{ id, payload }`, dan ada uji yang menjaga kesalahan yang sama tidak terulang.

---

## 2. Proses bisnis dari sisi pengguna

Form dibuka lewat tombol **+ Tambah Template** di daftar, atau **Ubah** di rincian.

1. **Kepala template.** Kode (maks. 50 karakter, unik per badan hukum), nama, jenis jurnal,
   frekuensi, tanggal terbit, dan masa berlaku.
2. **Frekuensi terkunci "Bulanan".** Ditampilkan, tetapi tidak dapat diubah — backend sengaja
   hanya mengenal nilai itu.
3. **Tanggal terbit hanya 1 sampai 28.** Tanggal 29–31 tidak ada di setiap bulan, sehingga
   template bertanggal itu akan terlewat pada Februari. Kalimat ini ada di bawah isiannya.
4. **Baris template** diisi persis seperti Form Jurnal — baris yang sama, bukan tiruannya.
5. **Total debit, kredit, dan selisih dihitung ulang setiap angka diketik.** Tombol **Simpan
   Template** mati selama belum seimbang.
6. Sesudah tersimpan, layar pindah ke rincian template. Template baru **selalu lahir tidak
   aktif**; spanduk di atas form sudah menyebutnya sebelum petugas menekan Simpan.

### Kenapa ada spanduk "template ini sedang aktif"

Backend **tidak menolak** penyuntingan template yang sedang aktif. Akibatnya perlu terbaca:
perubahan berlaku mulai penerbitan berikutnya, dan jurnal yang sudah terbit tidak ikut berubah.
Tanpa kalimat itu, petugas bisa mengira mengubah template ikut membetulkan jurnal bulan lalu.

### Jalur tidak normal

- **Satu baris berisi debit dan kredit sekaligus, atau keduanya kosong.** Nomor barisnya disebut:
  *"Baris 3: isi salah satu saja, debit atau kredit, dengan nilai lebih dari nol."* Simpan mati.
- **Akun beban tanpa unit biaya.** Kotak unit biaya berbunyi *"Wajib dipilih"*, dan menyimpan
  tanpa mengisinya ditolak form — perilaku bawaan `JournalLineRow`.
- **Isian kepala belum lengkap.** Toast menyebut isian mana saja, dan layar menggulir ke isian
  pertama yang salah.
- **Ditolak backend** — kode kembar, akun milik badan hukum lain, akun induk. Pesan aslinya
  ditampilkan apa adanya.
- **Tanpa hak `Create` atau `Update`.** Simpan mati beserta keterangan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `src/components/view/corporate/accounting/journal/form/journal-form-view.jsx` | `JournalLineRow` dan susunan form yang ditiru |
| `src/lib/hooks/corporate/accounting/journal/use-journal-editor.jsx` | Pola form, `useWatch`, dan pesan validasi |
| `src/components/view/corporate/accounting/journal/detail/journal-reversal-dialog.jsx` | Preseden pemakaian ulang `JournalLineRow` dan kelas tabelnya |
| `tests/unit/accounting-journal-reversal.test.mjs` | Uji yang mengunci letak `toCents` |
| `NewQuilvianSystemBackend/Areas/.../RecurringJournal/DTOs/RecurringJournalDtos.cs` | `CreateRecurringJournalRequest`, `UpdateRecurringJournalRequest` |
| `NewQuilvianSystemBackend/Areas/.../RecurringJournal/Services/AccRecurringJournalService.cs` | Aturan baris dan kepala beserta kalimat penolakannya |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/corporate/accounting/journal/journal-line-totals-utils.jsx` | **Baru.** `toCents`, `computeJournalLineTotals`, `findLinesWithInvalidSide` |
| `src/lib/hooks/corporate/accounting/recurring-journal/use-recurring-journal-editor.jsx` | **Baru.** Form, pemuatan ulang untuk ubah, hitungan, penyimpanan |
| `src/components/view/corporate/accounting/recurring-journal/form/recurring-journal-form-view.jsx` | **Baru.** Layar form |
| `src/app/corporate/accounting/recurring-journals/create/page.jsx`, `[slug]/update/page.jsx` | **Baru.** Entry point dan metadata saja |
| Constants dan berkas uji | Dibagi dengan `FE-ACC-P2-003` |

**Nol berkas CSS baru untuk form ini.** Kelas tata letaknya diambil dari
`journal-form-view.module.css` yang sama dengan Form Jurnal — `JournalLineRow` memang membawa
kelas-kelas itu, jadi tabel pembungkusnya harus sama supaya barisnya tampil identik.

### 3.3 Kepatuhan arsitektur frontend

`UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Header halaman | `Hero` | dipakai `journal-form-view.jsx` | `REUSE` |
| Gerbang tanpa hak akses | `AccessDeniedGate` | dipakai `journal-form-view.jsx` | `REUSE` |
| Spanduk template baru dan template aktif | `InformationAlert` | `base-features/information-alert.jsx` | `REUSE` |
| Isian kode, nama, tanggal terbit | `BaseTextField` | `base-form-control.jsx` | `REUSE` |
| Pilihan jenis jurnal dan frekuensi | `BaseSelectField` | dipakai `journal-form-view.jsx` | `REUSE` |
| Isian masa berlaku | `BaseDateField` | dipakai `journal-form-view.jsx` | `REUSE` |
| **Baris template** | **`JournalLineRow`** milik `FE-ACC-006` | diekspor `journal-form-view.jsx:60`; sudah dipakai ulang `journal-reversal-dialog.jsx` | `REUSE` |
| Total debit, kredit, selisih | `SummaryGrid` | dipakai `journal-form-view.jsx` | `REUSE` |
| Tombol Tambah Baris, Batal, Simpan | `BaseButton` | `base-features/base-button.jsx` | `REUSE` |
| Notifikasi hasil | `ToastStack` | `base-features/toast-stack.jsx` | `REUSE` |

**Satu `<table>` mentah dipertahankan, dengan alasan.** `JournalLineRow` merender `<tr>`, sehingga
ia harus berada di dalam `<table>` yang sama bentuknya dengan Form Jurnal — preseden yang sama
dipakai `FE-ACC-006` dan `FE-ACC-010`. Atribut `data-flat-table` **sengaja tidak dipasang**: ia
menimpa typography `th`/`td` lewat `globals.css`, sedangkan tabel Form Jurnal tidak memakainya.
Memasangnya justru membuat baris yang sama tampil berbeda di dua layar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (ubah) | Isian dan tombol mati sampai rincian template selesai dimuat |
| Menyimpan | Tombol Simpan menampilkan keadaan memuat bawaan `BaseButton` |
| Belum seimbang | Kalimat oranye beserta selisihnya; Simpan mati |
| Sisi baris tidak sah | Nomor barisnya disebut; Simpan mati |
| Seimbang | "Debit dan kredit sudah seimbang." |
| Gagal menyimpan | Toast merah beserta pesan asli backend, ditambah kotak galat halaman |
| Tautan ubah tidak valid | "Tautan template tidak valid. Buka ulang dari daftar Jurnal Berulang." |
| Tanpa hak akses | Simpan mati beserta keterangan |
| Kosong | `NOT APPLICABLE` — form selalu dimulai dengan dua baris kosong |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Recurring Journal

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/accounting/recurring-journals` | Menambah template | `RecurringJournal : Create` |
| `GET` | `/v1/corporate/accounting/recurring-journals/{id}` | Memuat template yang akan diubah | `RecurringJournal : Read` |
| `PUT` | `/v1/corporate/accounting/recurring-journals/{id}` | Mengubah template, dikirim sebagai `{ id, payload }` | `RecurringJournal : Update` |

#### Corporate - Accounting - Master Data - Chart of Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts/options` | Pilihan akun, beserta `RequiresCostCenter` | `ChartOfAccount : Read` |

#### Corporate - Accounting - Master Data - Journal Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/journal-types/options` | Pilihan jenis jurnal | `JournalType : Read` |

Pilihan unit biaya diambil `JournalLineRow` sendiri lewat resource `costCenters` yang sudah
terdaftar — nol jalur pengambilan data baru.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada seluruh berkas baru | Tanpa keluaran, exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | 0 error, 673 warning | `PASS` | Jumlah warning tidak bertambah |
| `npm run build` | `✓ Compiled successfully in 44s` | `PASS` | `○ /create` dan `ƒ /[slug]/update` terdaftar |
| `node --test tests/unit/` | 640 lulus, 0 gagal | `PASS` | 13 uji baru |
| Grep anti-regresi | Satu `<table>` mentah, dipertahankan | `PASS` | Alasan pada bagian 3.3 |
| `UAT-P2-13` | Tidak dijalankan | `NOT RUN` | **Diserahkan kepada Rizki atas permintaannya** |

Uji manual: `NOT RUN` — pemilik meminta pengujian layar dilakukan sendiri.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Total debit dan kredit tampil berjalan saat mengetik, dan tombol Simpan mati selama tidak seimbang | Terpenuhi | `useWatch` + `computeJournalLineTotals`; `canSave` bergantung pada `totals.isBalanced`. Uji "uang dihitung dalam sen", "tidak seimbang terbaca", "Simpan mati selama template tidak seimbang" |
| (2) Kolom cost center wajib muncul saat akun berjenis beban dipilih | Terpenuhi | `costCenterRequiredByIndex` dibaca dari `RequiresCostCenter` milik backend, lalu diteruskan ke `JournalLineRow` yang sudah menegakkannya — perilaku yang sama dengan Form Jurnal |
| (3) Satu baris hanya menerima debit atau kredit | Terpenuhi | `findLinesWithInvalidSide` menyebut nomor baris dan mematikan Simpan. Uji "satu baris hanya menerima debit ATAU kredit" |
| (4) Komponen baris yang dipakai terbukti komponen yang sama dengan Form Jurnal, bukan salinan | Terpenuhi | `import { JournalLineRow } from ".../journal-form-view"`. Uji "form template memakai JournalLineRow milik Form Jurnal, bukan salinan" sekaligus memeriksa tidak ada definisi baris maupun pemilih unit biaya sendiri |

**Definition of Done:** lint dan build hijau, laporan menyebut komponen yang dipakai ulang —
terpenuhi (`JournalLineRow`, kelas `journal-form-view.module.css`). Yang belum: `UAT-P2-13`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari task ini |
| Masalah yang diketahui | Jenis jurnal template boleh apa saja asal aktif, termasuk `JT` atau `JB` — backend tidak membatasi, dan `ACC-DEC-050` memang tidak menyebut pembatasan. Perlu keputusan owner |
| Temuan di luar lingkup | Cacat `{ id, data }` pada Form Jurnal MVP — lihat bagian 1. **Tidak diubah** |
| Utang yang disengaja | Logika keseimbangan kini hidup di tiga tempat: util bersama baru, `use-journal-editor.jsx`, dan `use-journal-reversal.jsx`. Menyatukan kedua hook MVP ke util menuntut perubahan uji `FE-ACC-010` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`, ditambah berkas baru di `src/app/corporate/accounting/recurring-journals/`, `src/components/view/corporate/accounting/recurring-journal/`, `src/lib/constants/corporate/accounting/recurring-journal/`, `src/lib/hooks/corporate/accounting/recurring-journal/`, `src/lib/state/slice/corporate/accounting/accounting-recurring-journal-slice.jsx`, dua CSS Module, `src/utils/corporate/accounting/journal/`, dan `tests/unit/accounting-recurring-journal.test.mjs`. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | `UAT-P2-13` di peramban |
