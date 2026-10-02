# Laporan Perubahan Frontend — `FE-ACC-P2-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-003` |
| Judul | Layar daftar Jurnal Berulang |
| Slice | `P2-3` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-003` |
| Trace | `ACC-DEC-050`; `FR-P2-022`; `03-frontend-architecture.md` bagian 9 dan 10 |
| Contract version | `ACC-API-0.9` — `approved`. Grup Recurring Journal diamandemen menjadi **`ACC-API-0.10` (usulan)**, menunggu ratifikasi Rizki |
| Wewenang UI | Satu butir menu tingkat 2, layar daftar, dan layar anak **Rincian Jurnal Berulang** beserta riwayat penerbitannya |
| Dependency | `BE-ACC-P2-007` — `DONE`; diperiksa ulang 11 September 2026 pada `968841e`: 46/46 uji backend lulus |
| Klasifikasi | `MEDIUM` — dua layar, satu slice, tanpa komponen base baru |
| Task mode | `CROSS-REPO` — source di frontend, laporan dan amandemen kontrak di backend |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`, laporan ini, dan kedua kontrak yang diamandemen atas persetujuan Rizki |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `460f717a0` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `968841e` (branch `rizkiG`) |
| Tanggal | 11 September 2026 |
| Status | **✅ `SELESAI` sisi development — penilaian ulang 11 September 2026.** `IMPLEMENTATION COMPLETE` · `READY FOR UAT`; UAT belum dijalankan, diserahkan ke tim UAT terpisah atas keputusan owner. Lihat baris Status kartu task pada roadmap. *Riwayat: Selesai di kode. Uji peramban diserahkan kepada Rizki.* |

---

## 1. Keadaan yang ditemukan di awal

Sebelum satu baris ditulis, backend-nya diperiksa ulang langsung — bukan dibaca dari status di
roadmap:

| Yang diperiksa | Hasil |
| --- | --- |
| Kode backend | Sudah di-commit (`968841e`), tree bersih |
| Endpoint | Delapan, di `api/v1/corporate/accounting/recurring-journals` |
| Migration | Sudah diterapkan; ketiga tabel ada dan berisi 0 baris |
| Izin di database | `Read`, `Create`, `Update`, **`Activate`**, `Generate` terdaftar di `SysActionAccess` |
| Uji backend | Dijalankan ulang pada HEAD: `Passed: 46, Failed: 0` |

Pemeriksaan database memakai sesi **READ ONLY**, hanya `SELECT`.

### Selisih kontrak yang ditemukan — dan sudah diamandemen

Kasusnya sama dengan `Period` vs `AccountingPeriod` pada 10 September:

| | Aktifkan / Nonaktifkan |
| --- | --- |
| `api-contract.md` `ACC-API-0.9` | `RecurringJournal : Update` |
| `permission-audit-matrix.md` Phase 2 | `Activate` tidak disebut sama sekali |
| `RecurringJournalController` + database | **`RecurringJournal : Activate`** |

Laporan `BE-ACC-P2-007` sudah mencatatnya sebagai delta sejak 10 September, tetapi catatan itu
tidak pernah menyeberang ke kontrak. **Layar ini mengikuti kode.** Kedua kontrak diamandemen atas
persetujuan Rizki dan berlabel *menunggu ratifikasi* — status `approved`-nya tidak diubah.

Saat mengamandemen matriks izin, ditemukan pula bahwa **tiga baris penutupan periode di sana masih
menulis `Period : ...`** — sisa kesalahan yang sudah diperbaiki di `api-contract.md` tetapi
terlewat di matriks. Ikut diperbaiki dalam amandemen yang sama.

### Keputusan cakupan

Kartu menulis cakupannya *"Butir menu tingkat 2 `/accounting/recurring-journals`;
`accounting-recurring-journal-slice.jsx`"*. Tetapi acceptance (2) menuntut *"riwayat penerbitan
per periode dapat dibuka beserta tautan ke jurnal yang dihasilkan"*, dan acceptance (3) menuntut
tombol Aktifkan. Keduanya butuh tempat.

Tempatnya adalah **Rincian Jurnal Berulang**, layar anak yang memang sudah tercantum di
`03-frontend-architecture.md` bagian 9 — *"Riwayat Penerbitan Template | Rincian Jurnal
Berulang"*. Layar itu dibangun dalam task ini.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya Staf Akuntansi dan Manajer Akuntansi.** Layar dibuka lewat **Akuntansi › Jurnal
Berulang**.

1. Petugas memilih **badan hukum**. Daftar baru diminta setelah itu — lihat catatan pada
   bagian 3.3.
2. Daftar menampilkan setiap template: kode, nama, jenis jurnal, **jadwal terbit** ("Bulanan,
   tanggal 5"), masa berlaku, nilai, **berapa kali sudah terbit**, dan status aktifnya.
3. Petugas dapat menyaring menurut status aktif dan jenis jurnal, atau mencari kode dan nama.
4. **Klik dua kali** sebuah baris membuka rinciannya: kepala template, baris debit-kreditnya, dan
   **riwayat penerbitan** — satu baris untuk tiap periode tempat template itu pernah terbit.
5. Tombol **Buka Jurnal** pada riwayat membuka rincian jurnal yang dihasilkan, lewat token rute
   yang sama dengan layar Jurnal.
6. Tombol **Aktifkan** atau **Nonaktifkan** meminta konfirmasi lebih dahulu. Sesudah berhasil,
   rincian dan riwayatnya dimuat ulang dari backend.

### Kenapa kolom "Terbit" ada

`RunCount` membedakan template yang **belum pernah terbit** dari yang rutin terbit. Status aktif
saja tidak cukup: template aktif yang tanggal mulainya belum tiba sama-sama "aktif", tetapi belum
pernah menghasilkan apa pun. Nol ditampilkan sebagai *"Belum pernah"*, bukan angka `0`.

### Jalur tidak normal

- **Badan hukum belum dipilih.** Tabel kosong beserta penjelasannya; tombol Tambah mati.
- **Tidak ada template yang cocok penyaring.** Kalimat kosongnya berbeda dari daftar yang memang
  kosong.
- **Tanpa hak `Activate`.** Tombol Aktifkan **dimatikan, bukan disembunyikan**, beserta keterangan
  *"Anda tidak memiliki hak mengaktifkan template jurnal berulang."*
- **Aktivasi ditolak backend.** Aktivasi memeriksa ulang seluruh baris — akun yang sudah
  dinonaktifkan, unit biaya yang dipindah. Pesannya ditampilkan apa adanya karena ia menyebut
  baris mana yang bermasalah.
- **Riwayat gagal dimuat.** Hanya bagian riwayat yang berganti menjadi kotak galat; rincian
  templatenya tetap utuh.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/.../RecurringJournal/Controllers/RecurringJournalController.cs` | Route, `[AccessAction]`, dan `[AccessPermission]` sebenarnya |
| `NewQuilvianSystemBackend/Areas/.../RecurringJournal/DTOs/*.cs`, `Enums/RecurringFrequency.cs` | Bentuk daftar, rincian, riwayat, dan permintaan |
| `NewQuilvianSystemBackend/Areas/.../RecurringJournal/Services/AccRecurringJournalService.cs` | Seluruh penolakan beserta kalimatnya |
| `NewQuilvianSystemBackend/Seeders/AccessMenuSeeder.cs` | Aksi di layar Akses Role berasal dari `[AccessAction]` lewat refleksi |
| `src/lib/state/slice/master-data-resource-slice-factory.jsx` | Factory menangani `activate`/`deactivate` beserta `actionLoading`-nya |
| `src/components/view/corporate/accounting/journal/journal-view.jsx`, `use-journal.jsx` | Modul referensi visual layar daftar |
| `src/components/view/corporate/accounting/journal/detail/journal-detail-view.jsx`, `use-journal-detail.jsx` | Modul referensi visual layar rincian dan pola token rute |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/corporate/accounting/accounting-recurring-journal-slice.jsx` | **Baru.** Factory untuk daftar, rincian, tambah, ubah, aktifkan, nonaktifkan; thunk tersendiri untuk riwayat dengan state terpisah |
| `src/lib/constants/corporate/accounting/recurring-journal/recurring-journal-constants.jsx` | **Baru**, dibagi dengan `FE-ACC-P2-004`. Enum, kolom, salinan teks |
| `src/lib/hooks/corporate/accounting/recurring-journal/use-recurring-journal.jsx` | **Baru.** Daftar, penyaring, token rute |
| `src/lib/hooks/corporate/accounting/recurring-journal/use-recurring-journal-detail.jsx` | **Baru.** Rincian, riwayat, aktifkan/nonaktifkan, tautan ke jurnal |
| `src/components/view/corporate/accounting/recurring-journal/recurring-journal-view.jsx` | **Baru.** Layar daftar |
| `src/components/view/corporate/accounting/recurring-journal/detail/recurring-journal-detail-view.jsx` | **Baru.** Layar rincian |
| `src/style/corporate/accounting/recurring-journal-view.module.css`, `recurring-journal-detail-view.module.css` | **Baru.** Hanya tata letak |
| `src/app/corporate/accounting/recurring-journals/page.jsx`, `recurring-journal-client.jsx`, `[slug]/page.jsx` | **Baru.** Entry point dan metadata saja |
| `tests/unit/accounting-recurring-journal.test.mjs` | **Baru**, dibagi dengan `FE-ACC-P2-004`. 13 uji |
| `src/lib/state/store.jsx` | Mendaftarkan slice |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir menu tingkat 2 sesudah Jurnal, ikon `RiHistoryLine` yang sudah diimpor |

### 3.3 Kepatuhan arsitektur frontend

`UI GATE: 12 elemen — REUSE 12, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Header halaman daftar | `Hero` | dipakai `journal-view.jsx` | `REUSE` |
| Gerbang tanpa hak akses | `AccessDeniedGate` | dipakai `journal-view.jsx` | `REUSE` |
| Pemilih badan hukum | `AccountingLegalEntitySelect` | `view/corporate/accounting/shared/` | `REUSE` |
| Panel penyaring dan pencarian | `DataFilter` | dipakai `journal-view.jsx` | `REUSE` |
| Penyaring status, jenis, ukuran halaman | `FilterSelect` | dipakai `journal-view.jsx`, termasuk `searchable` | `REUSE` |
| Tabel berhalaman | `DataTable` + `RegionPagination` | dipakai `journal-view.jsx` | `REUSE` |
| Lencana aktif/tidak aktif | `StatusBadge` | nada `active`/`inactive` yang sudah ada | `REUSE` |
| Tombol Tambah, Ubah, Aktifkan, Buka Jurnal | `BaseButton` | `base-features/base-button.jsx` | `REUSE` |
| Kerangka rincian | `BaseDetailView` | dipakai `journal-detail-view.jsx` | `REUSE` |
| Konfirmasi Aktifkan/Nonaktifkan | slot `deleteConfirm` milik `BaseDetailView` | pola yang sama di `journal-detail-view.jsx` | `REUSE` |
| Tabel baris dan tabel riwayat | `DataTable` | menggantikan `<table>` mentah yang dipakai rincian jurnal MVP | `REUSE` |
| Keterangan status dan galat riwayat | `InformationAlert` | `base-features/information-alert.jsx` | `REUSE` |

Tiga catatan pelaksanaan:

- **Daftar hanya diminta sesudah badan hukum dipilih.** Berbeda dari daftar jurnal MVP yang
  memintanya tanpa badan hukum. `LegalEntityId` pada `RecurringJournalPagedQuery` opsional,
  sehingga tanpa itu backend mengembalikan template **seluruh** badan hukum — padahal pembukuan
  dipisah per badan hukum (`ACC-DEC-037`).
- **Aktifkan/nonaktifkan memakai thunk factory, bukan thunk tambahan.** Factory sudah menangani
  `PATCH /{id}/activate` dan `/deactivate` lewat `activateConfig`/`deactivateConfig`, dan
  reducernya sendiri yang menyalakan `actionLoading`. Berbeda dari slice jurnal MVP, tidak ada
  celah penjaga tombol yang perlu ditambal.
- **Riwayat punya state sendiri.** `runs`, `runsLoading`, `runsError` terpisah dari rincian,
  supaya riwayat yang gagal dimuat tidak menghilangkan rincian template.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil daftar template...", "Mengambil rincian template...", "Mengambil riwayat penerbitan..." |
| Kosong — belum pilih badan hukum | "Badan hukum belum dipilih." beserta penjelasan pemisahan per badan hukum |
| Kosong — tersaring | "Tidak ada template yang cocok dengan penyaring." |
| Kosong — belum ada template | "Belum ada template jurnal berulang untuk badan hukum ini." beserta ajakan membuat template pertama |
| Kosong — belum pernah terbit | "Template ini belum pernah terbit." |
| Gagal — daftar atau rincian | Kotak galat halaman beserta pesan asli backend |
| Gagal — riwayat | Kotak galat setempat; rincian template tetap tampil |
| Gagal — aktivasi | Toast merah "Tindakan Ditolak" beserta pesan asli backend |
| Tanpa hak akses | Tombol mati beserta keterangan; `403` halaman ditangani `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Recurring Journal

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/recurring-journals` | Daftar template berhalaman | `RecurringJournal : Read` |
| `GET` | `/v1/corporate/accounting/recurring-journals/{id}` | Rincian template beserta barisnya | `RecurringJournal : Read` |
| `GET` | `/v1/corporate/accounting/recurring-journals/{id}/runs` | Riwayat penerbitan | `RecurringJournal : Read` |
| `PATCH` | `/v1/corporate/accounting/recurring-journals/{id}/activate` | Mengaktifkan template | **`RecurringJournal : Activate`** |
| `PATCH` | `/v1/corporate/accounting/recurring-journals/{id}/deactivate` | Menonaktifkan template | **`RecurringJournal : Activate`** |

#### Corporate - Accounting - Master Data - Journal Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/journal-types/options` | Pilihan penyaring jenis jurnal | `JournalType : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada seluruh berkas baru | Tanpa keluaran, exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | 0 error, 673 warning | `PASS` | Jumlah warning **tidak bertambah**; nol temuan pada berkas task ini |
| `npm run build` | `✓ Compiled successfully in 44s` | `PASS` | `○ /corporate/accounting/recurring-journals` dan `ƒ /[slug]` terdaftar |
| `node --test tests/unit/` | 640 lulus, 0 gagal | `PASS` | Naik dari 627; 13 uji baru dibagi dengan `FE-ACC-P2-004` |
| Uji backend `FullyQualifiedName~AccRecurringJournal` pada `968841e` | `Passed: 46, Failed: 0` | `PASS` | Dijalankan ulang 11 September 2026 |
| Aksi izin di database | Lima aksi terdaftar | `PASS` | `SELECT` pada `SysActionAccess` |
| Grep anti-regresi 1, 3, 4, 5, 6, 7, 8 | Kosong | `PASS` | Tanpa warna literal, tombol non-base, tabel mentah, `!important`, inline style; `globals.css` tidak disentuh |
| Grep anti-regresi 2 — typography | Empat temuan, dipertahankan | `PASS` | Seluruhnya menyasar `<h2>`, `<p>`, `<span>` milik layar ini |
| Uji peramban | Tidak dijalankan | `NOT RUN` | **Diserahkan kepada Rizki atas permintaannya** |

Uji manual: `NOT RUN` — pemilik meminta pengujian layar dilakukan sendiri.

**Tidak dijalankan:** pemanggilan API sungguhan. Backend lokal sedang tidak berjalan; bentuk
respons diambil dari DTO pada source, yang otoritatif.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Template aktif dan tidak aktif terbedakan jelas | Terpenuhi | `resolveActiveBadge` memberi dua nada berbeda, di daftar maupun rincian; uji "aktif dan tidak aktif memakai nada berbeda" |
| (2) Riwayat penerbitan per periode dapat dibuka beserta tautan ke jurnal yang dihasilkan | Terpenuhi di kode | Tabel riwayat pada rincian, tombol Buka Jurnal memakai token rute layar Jurnal. **Belum dapat diperlihatkan dengan data nyata** — lihat bagian 8 |
| (3) Tombol Aktifkan hanya menyala bagi yang berhak | Terpenuhi | `usePermission("RecurringJournal", "Activate")` mematikan tombol beserta keterangan; uji "tombol Aktifkan dijaga hak Activate, bukan Update" |
| (4) Keadaan kosong berbunyi wajar | Terpenuhi | Tiga sebab daftar kosong dan satu keadaan riwayat kosong, masing-masing dengan kalimatnya |

**Definition of Done:** lint dan build hijau, laporan tracked ini ada — terpenuhi.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Belum ada peran yang diberi izin `RecurringJournal`** — `SysAccessPolicy` berisi 0 baris untuknya, sama dengan `AccountingConfiguration` dan `YearEndClosing`. Akun non-SuperAdmin akan melihat akses ditolak. Beri izinnya lewat layar Akses Role, dan **centang `Activate` tersendiri** |
| Masalah yang diketahui | Acceptance (2) belum dapat diperlihatkan dengan data nyata. Penjadwal bawaannya mati (`Enabled = false`) dan `appsettings` tidak menyalakannya, sementara tidak ada task FE yang menyediakan tombol terbit manual. Riwayat baru berisi bila `POST /{id}/generate` dipanggil lewat Swagger atau penjadwal dinyalakan |
| Dependency backend | `NONE` — `BE-ACC-P2-007` dan `008` sudah `DONE` |
| Kontrak | `ACC-API-0.10` dan `ACC-PERMISSION-0.5` diusulkan, **menunggu ratifikasi Rizki**. Pembagian peran untuk `Activate` — Manager dan Administrator — adalah usulan |
| Temuan di luar lingkup | **Form Jurnal MVP (`FE-ACC-006`) mengirim badan kosong saat menyimpan ubahan draft.** `use-journal-editor.jsx:341` memanggil `updateJournal({ id, data: payload })`, sedangkan factory hanya membaca `{ id, payload }`. Dari seluruh hook di repository, hanya berkas itu yang memakai `data:`. Skenario uji `FE-ACC-006` tidak pernah mencakup menyimpan ubahan. **Tidak diubah** karena di luar lingkup task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `FE-ACC-P2-004` — keduanya dikerjakan dalam satu paket. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Ratifikasi kedua amandemen kontrak; beri izin `RecurringJournal` pada peran uji; uji peramban |
