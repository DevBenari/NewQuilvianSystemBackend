# Laporan Perubahan Frontend — `FE-FIN-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-035` |
| Judul | Petugas cutover tahu siapa yang menyetujui batch, dan tahu apa yang harus dilakukan ketika berkasnya terlalu besar |
| Slice | `REV-16B` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian `task_range_frontend: FE-FIN-033..FE-FIN-037` |
| Trace | `FR-FIN-187`, `FR-FIN-188`; `FIN-DEC-149`, `150`, `151`, `155`; `03-frontend-architecture.md` §21.4 nomor 3, §21.5 |
| Contract version | `FIN-API-1.7` G.2; `FIN-VAL-1.9` `FIN-VAL-229` |
| Reuse | Kerangka keterangan galat `opening-item-batch-utils.jsx` yang sudah ada (`normalizeBatchError` generik, tanpa special-casing baru); pola kolom `approvedByName` dari `journal-constants.jsx`/`opening-balance-group-card.jsx` |
| Dependency | `BE-FIN-088` dan `BE-FIN-089` — **source selesai, `dotnet build` NOT RUN** (lihat §0: ini BUKAN "rilis" seperti `BE-FIN-086` pada `FE-FIN-033`) |
| Wewenang UI | **Bukan** `DEV_DISCRETION` untuk penyempitan `accept` ke CSV (`FIN-DEC-150`, keputusan pemilik eksplisit); `DEV_DISCRETION` untuk posisi kolom "Disetujui Oleh" pada tabel |
| Klasifikasi | `LIGHT` — 6 berkas diubah (nol berkas baru), 2 berkas test diperbarui; nol komponen baru |
| Task mode | `FRONTEND`. Target tulis `QuilvianSystemFrontendDev`; laporan ini ditulis di repository backend karena di sanalah `docs/module-blueprints/` tinggal |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `66d90570d` (branch `yasmina`) |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `npm run lint:errors`, `npm run test:unit`, `npm run build` seluruhnya PASS (dijalankan sesi ini). `npm run dev` **TIDAK DIJALANKAN**. Dependency backend **belum rilis** (lihat §0) — lihat §9 untuk mengapa task ini tetap dikerjakan source-only |

---

## 0. Gerbang Dependency Backend — Diperiksa, BERBEDA dari `FE-FIN-033`

Roadmap mensyaratkan `BE-FIN-088` **dan** `BE-FIN-089` **rilis**. Diperiksa ke laporan kedua task
itu di `01-backend-roadmap.md` baris `BE-FIN-088`/`BE-FIN-089`: **keduanya menyatakan eksplisit
`dotnet build` NOT RUN (instruksi pengguna); source selesai** — berbeda dari `BE-FIN-086` pada
`FE-FIN-033` yang sudah dikonfirmasi *build PASS, migration diterapkan*. Gerbang dependency task
ini **secara literal belum terpenuhi** — berbeda dari kesimpulan `FE-FIN-033` yang memverifikasi
gerbangnya memang sudah terbuka.

**Keputusan yang diterapkan**: melanjutkan source-only, mengikuti standing preference yang sudah
ditetapkan eksplisit pengguna pada rumpun `BE-FIN` sesi ini (dikonfirmasi lewat `AskUserQuestion`
pada giliran `BE-FIN-093`: *"Lanjutkan source-only, tetap tanpa build (Recommended)"* ketika
dependency-nya juga belum pernah di-build) — bukan keputusan baru yang saya ambil sendiri tanpa
dasar. Kontrak kedua task backend itu **sudah terbaca lengkap** dari laporan task masing-masing
(nama field persis, status code, pesan persis — lihat §4), cukup untuk menulis source frontend
yang benar secara struktural. **Dicatat tegas**: berbeda dari `FE-FIN-033`/`034`, perilaku yang
terlihat pengguna pada task ini **TIDAK AKAN berubah secara runtime** sampai `BE-FIN-088`/`089`
benar-benar di-build dan dijalankan — source hari ini hanya *siap* menampilkannya.

---

## 1. Masalah yang Diperbaiki

Tiga ketidaksesuaian konkret pada layar Batch Migrasi Tagihan Lama (`FE-FIN-032`) terhadap
kontrak yang diperbarui amandemen 4 Oktober 2026:

1. **`approvedByName` belum pernah dikonsumsi.** `normalizeOpeningItemBatch` tidak membaca ruas
   itu sama sekali; kolom tabel dan panel rincian hanya menampilkan `approvedAt` (waktu), tidak
   pernah nama penyetujunya (`FIN-DEC-151`).
2. **Pemilih unggah (dan unggah ulang) masih menawarkan CSV *dan* XLSX** — `accept` literal
   `.csv,.xlsx,...` dan label "CSV atau XLSX" di dua tempat (`opening-item-batch-upload-card.jsx`,
   `opening-item-batch-detail-panel.jsx`). `FIN-DEC-150` (amandemen 4 Oktober 2026, **sesudah**
   `FE-FIN-032` ditulis) secara eksplisit menunda bagian kontrak itu: *"Pemilih unggah hanya CSV
   selama XLSX ditunda... Menunda bagian kontrak `FIN-API-1.6` F.2 'satu pemilih menerima CSV dan
   XLSX'."* Komentar kode `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` (sebelum task ini) secara
   eksplisit masih menulis "pemilih unggah tetap menerima CSV dan XLSX" — bukti ini belum
   disesuaikan sejak amandemen.
3. **Batas 10.000 baris** (`FIN-VAL-229`, `BE-FIN-088`) — **diinvestigasi, ternyata SUDAH
   terpenuhi tanpa perubahan kode** (lihat §2 nomor 3). Dicatat di sini supaya tidak disangka
   terlewat.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. **Nama penyetuju** kini tampil pada kolom "Disetujui Oleh" di tabel daftar batch, dan baris
   "Disetujui oleh" pada panel rincian (bersebelahan dengan "Disetujui pada", kondisi render yang
   sama — hanya tampil bila `approvedAt` terisi). `null`/belum disetujui/pengguna tidak ditemukan
   seluruhnya menampilkan `"-"`, **tidak pernah** `ApprovedBy` (Guid) mentah.
2. **Pemilih unggah kini hanya menawarkan CSV** (`accept=".csv,text/csv"`, label "Berkas Migrasi
   (CSV)"). Hint di bawahnya berbunyi *"Pemilih ini hanya menerima CSV untuk saat ini — dukungan
   XLSX ditunda sampai pembacanya tersedia di sistem."* — **tidak lagi** menjanjikan menerima XLSX.
   Keduanya (toggle `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` dan hint-nya) otomatis kembali ke
   bentuk dual-format begitu `BE-FIN-083` selesai — nol perubahan kode diperlukan saat itu, hanya
   mengubah satu konstanta boolean (pola yang sudah ada sejak `FE-FIN-032` untuk pemilih **format
   templat**, kini diterapkan konsisten ke pemilih **unggah**).
3. **Batas 10.000 baris**: diinvestigasi penuh dengan membaca `FinanceOpeningItemBatchService.cs`
   (`BE-FIN-088`) — pesan backend *sudah* persis sesuai kontrak: *"Berkas memuat lebih dari 10.000
   baris. Pecah menjadi beberapa berkas, lalu unggah masing-masing sebagai batch tersendiri."*,
   dijawab `400` (`OpeningItemBatchBadRequestException`). Jalur render-nya ditelusuri dari
   `handleUpload` (hook) → `uploadOpeningItemBatch.rejected` (slice, `markFailure`) →
   `state.actionError` → `<InformationAlert variant="danger" title="Tindakan gagal"
   message={actionError.message} />` pada level halaman (`opening-item-batches-view.jsx`,
   **tidak disentuh task ini**). `normalizeBatchError` (status `400`) sudah jatuh ke cabang
   `return message || fallback` — meneruskan teks backend **apa adanya**, tanpa special-casing
   yang bisa memotong atau mengubahnya. **Simpulan: nol kode berubah untuk kriteria ini** — sudah
   benar sejak `FE-FIN-032`, hanya belum pernah terbukti karena `BE-FIN-088` belum dibangun. Tidak
   ada percobaan ulang otomatis di manapun pada alur unggah — `handleUpload` memanggil thunk
   tepat sekali per klik, dijaga `inFlightRef`.

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

**UI GATE**: nol komponen baru. Kolom tabel "Disetujui Oleh" memakai pola `render(item) =>
item.field || "-"` yang **sudah dipakai** pada enam kolom lain di berkas kolom yang sama — `REUSE`
murni. Baris "Disetujui oleh" pada panel rincian memakai `MetaRow` lokal yang sudah ada,
bersebelahan dengan baris "Disetujui pada" yang sudah ada — `REUSE` murni, nol komponen/pola baru.

Penyempitan `accept`/label pemilih unggah bukan perubahan komponen — satu-satunya perubahan
adalah NILAI prop `accept` dan teks label pada elemen `<input type="file">` HTML native yang
sudah ada, kini bersumber dari konstanta terpusat (`OPENING_ITEM_BATCH_UPLOAD_ACCEPT`/
`_FORMAT_LABEL`) alih-alih literal berulang di dua berkas.

---

## 4. Endpoint yang Dikonsumsi dan Pemetaan Kontrak

**Nol endpoint baru, nol perubahan route/verb.** Field yang baru dikonsumsi:

| Field | Sumber | Dibawa oleh |
| --- | --- | --- |
| `ApprovedByName` (`string?`) | `OpeningItemBatchResponse`/`OpeningItemBatchDetailResponse` (`BE-FIN-089`, diverifikasi langsung ke `OpeningItemBatchDtos.cs:61` — `DetailResponse` mewarisi `Response`, sehingga satu field menjangkau daftar **dan** rincian) | `GET /opening-item-batches` (daftar), `GET /opening-item-batches/{id}` (rincian) |
| Batas 10.000 baris (`400`) | `FinanceOpeningItemBatchService.cs` (`BE-FIN-088`, konstanta `MaxUploadRowCount = 10_000`), diverifikasi pesan persis | `POST /opening-item-batches` (unggah), berlaku juga pada unggah ulang (pemeriksaan sama, dipanggil ulang) |

---

## 5. Berkas yang Diubah

Nol berkas baru. Seluruhnya **diubah**:

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/subledger-setup/opening-item-batch-constants.jsx` | Tambah `OPENING_ITEM_BATCH_UPLOAD_ACCEPT`, `OPENING_ITEM_BATCH_UPLOAD_FORMAT_LABEL` (keduanya mengikuti toggle `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` yang sudah ada); `XLSX_UPLOAD_HINT` ditulis ulang (CSV-saja, bukan lagi "menerima CSV dan XLSX"); komentar `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` diperbarui |
| `src/utils/finance/subledger-setup/opening-item-batch-utils.jsx` | `normalizeOpeningItemBatch` menambah `approvedByName` |
| `src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-upload-card.jsx` | `accept` dan label field memakai konstanta baru, bukan literal CSV+XLSX |
| `src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-detail-panel.jsx` | `accept`/`aria-label` pada input unggah ulang memakai konstanta yang sama; baris `MetaRow` "Disetujui oleh" ditambah |
| `src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-table-columns.jsx` | Kolom baru "Disetujui Oleh" ditambah setelah kolom "Status" |
| `tests/unit/opening-item-batch-utils.test.mjs` | Tambah test `approvedByName` (termasuk null untuk belum-disetujui dan pengguna-tidak-ditemukan, dan memastikan `approvedBy` mentah tidak ikut terbawa) dan test konstanta `OPENING_ITEM_BATCH_UPLOAD_ACCEPT`/`_FORMAT_LABEL`/`XLSX_UPLOAD_HINT` (CSV-saja) |

`finance-opening-item-batch-slice.jsx`, `use-opening-item-batches.jsx`, dan
`opening-item-batches-view.jsx` **sengaja tidak disentuh** — jalur render galat `400` yang sudah
ada (§2 nomor 3) dan thunk/selector yang sudah ada sepenuhnya cukup, tidak ada state baru yang
dibutuhkan.

---

## 6. Dampak Database dan Backend

`NOT APPLICABLE`. `FRONTEND MODE` murni. Backend adalah referensi read-only — dibaca untuk
memverifikasi nama field dan teks pesan persis (`OpeningItemBatchDtos.cs`,
`FinanceOpeningItemBatchService.cs`), **tidak diubah**.

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Hasil | Keterangan |
| --- | :--: | --- |
| `npm run lint:errors` | `PASS` | Nol error |
| `npm run test:unit` | `PASS` | 2 test baru `FE-FIN-035` pada `opening-item-batch-utils.test.mjs` **PASS**. 49 kegagalan pra-ada pada berkas lain (modul Rawat Inap, tidak terkait) — jumlah **persis sama** dengan baseline yang sudah diverifikasi `git stash` pada `FE-FIN-033` |
| `npm run build` | `PASS` | Exit code `0`; nol "Failed to compile"; route `/finance/subledger-setup/opening-item-batches` muncul pada daftar output build |
| `npm run dev` | **TIDAK DIJALANKAN** | Instruksi eksplisit pengguna |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

**Alasan konkret, lebih kuat dari task sebelumnya**: selain server dev tidak dijalankan (instruksi
pengguna), backend `BE-FIN-088`/`089` **belum pernah dikompilasi** — endpoint sungguhan TIDAK
dapat dipanggil sama sekali untuk memverifikasi `approvedByName` benar-benar terkirim atau batas
10.000 baris benar-benar ditegakkan, terlepas dari ada-tidaknya server dev frontend. Uji `K.3.2`,
`K.3.4` **tidak dapat dijalankan dalam bentuk apa pun** sampai backend dibangun.

`AUTOMATED TEST: node --test "tests/unit/*.test.mjs" — PASS (berkas opening-item-batch-utils.test.mjs, termasuk 2 kasus baru FE-FIN-035)`.

---

## 8. Pemenuhan Acceptance Criteria

| Kriteria (roadmap `02-frontend-roadmap.md` baris `FE-FIN-035`) | Status | Bukti |
| --- | :--: | --- |
| Nama penyetuju tampil; `null` ditulis keterangannya, **bukan** ID | **Terpenuhi struktural** (unit test; **NOT FEASIBLE** data sungguhan — §7) | Kolom tabel + `MetaRow` keduanya `item.approvedByName \|\| "-"` |
| `400` batas baris memunculkan keterangan yang dapat ditindaklanjuti dan berkas **tidak** dikirim ulang otomatis | **Sudah terpenuhi sebelum task ini** — nol kode berubah (lihat §2 nomor 3) | `normalizeBatchError` sudah meneruskan pesan backend apa adanya; `handleUpload` memanggil thunk tepat sekali |
| Pemilih unggah **hanya** menawarkan CSV | **Terpenuhi struktural** (unit test; **NOT FEASIBLE** verifikasi klik sungguhan — §7) | `OPENING_ITEM_BATCH_UPLOAD_ACCEPT === ".csv,text/csv"` pada kedua lokasi input berkas |
| Pilihan XLSX pada unduh templat **tetap nonaktif** beserta alasannya | **Tidak disentuh, tetap benar** | `isTemplateFormatSelectable`/`XLSX_TEMPLATE_DISABLED` tidak diubah |
| Isi baris berkas **MUST NOT** masuk keterangan galat — hanya nomor baris dan sebabnya | **Tidak disentuh, tetap benar** | `buildErrorListText`/`buildErrorListCsv` (FE-FIN-032) tidak diubah, sudah sesuai sejak awal |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **PASS, dijalankan sesi ini** | §7 |
| Uji `K.3.2`, `K.3.4` | **NOT FEASIBLE** — backend belum dikompilasi, bukan hanya server dev tidak dijalankan | §7 |

**DoD roadmap**: **belum terpenuhi** — acceptance criteria source-level terpenuhi; validasi
statis (`lint`/`test:unit`/`build`) PASS dijalankan sesi ini; namun verifikasi fungsional sungguhan
(`K.3.2`, `K.3.4`) tidak hanya menunggu server dev, melainkan menunggu `BE-FIN-088`/`089`
benar-benar dibangun dan dijalankan terlebih dahulu.

---

## 9. Risiko dan Hal yang MUST Disampaikan

1. **Risiko terbesar task ini: dependency backend belum rilis, berbeda dari `FE-FIN-033`.**
   Jangan menyamakan status 🟡 task ini dengan 🟡 `FE-FIN-033`/`034` — pada kedua task itu,
   backend-nya sudah terkonfirmasi di-build dan (untuk `BE-FIN-086`) migration-nya sudah
   diterapkan; hanya uji manual yang tersisa. Di sini, **source backend itu sendiri belum pernah
   dikompilasi** — risiko bahwa `BE-FIN-088`/`089` ternyata tidak kompilasi bersih, atau nama
   field berubah saat benar-benar dibangun, nyata dan belum tertutup.
2. **Pola toggle terpusat dipertahankan, bukan dihardcode.** `OPENING_ITEM_BATCH_UPLOAD_ACCEPT`/
   `_FORMAT_LABEL` sengaja dibuat mengikuti `OPENING_ITEM_BATCH_XLSX_READER_AVAILABLE` (bukan
   string CSV-only yang di-hardcode permanen) — begitu `BE-FIN-083` selesai dan flag itu diubah
   `true`, pemilih unggah otomatis kembali menawarkan XLSX **tanpa** perubahan kode lain. Mengubah
   ini menjadi hardcode permanen akan menghapus jalur kembali yang sudah direncanakan `FIN-DEC-149`.
3. **Temuan positif, bukan gap**: investigasi mendalam terhadap kriteria batas 10.000 baris
   menyimpulkan infrastruktur frontend (`FE-FIN-032`) SUDAH benar sejak awal — **tidak ada
   perbaikan yang diperlukan**. Dicatat eksplisit supaya pembaca laporan ini tidak menyangka
   kriteria itu terlewat begitu saja.
4. **Field `ApprovedBy` (Guid mentah) tetap TIDAK dibawa ke state frontend** — konsisten dengan
   larangan `FIN-DEC-151`/`FIN-DES-095` menampilkan ID mentah; diuji eksplisit (`"approvedBy" in
   approved === false`).

---

## 10. Status Git (frontend)

```text
 M src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-detail-panel.jsx
 M src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-table-columns.jsx
 M src/components/view/finance/subledger-setup/opening-item-batches/opening-item-batch-upload-card.jsx
 M src/lib/constants/finance/subledger-setup/opening-item-batch-constants.jsx
 M src/utils/finance/subledger-setup/opening-item-batch-utils.jsx
 M tests/unit/opening-item-batch-utils.test.mjs
```

(Berkas `FE-FIN-033`/`034` yang masih tercatat berubah pada `git status` keseluruhan bukan bagian
task ini — lihat laporan masing-masing.) Tidak ada stage, commit, push, pull, merge, atau rebase.

---

## 11. Langkah Berikutnya

1. Pemilik menjalankan `dotnet build` untuk `BE-FIN-088` dan `BE-FIN-089` (keduanya source-only
   saat ini) — prasyarat **sesungguhnya** sebelum field/perilaku apa pun pada task ini dapat
   diverifikasi hidup.
2. Sesudah langkah 1, jalankan uji manual `K.3.2`, `K.3.4` dengan server dev dan backend
   berjalan bersamaan (berkas >10.000 baris, dan batch yang sudah disetujui seseorang).
3. Sesudah langkah 1-2 dilaporkan, `FE-FIN-035` dapat ditandai sepenuhnya selesai pada roadmap.
