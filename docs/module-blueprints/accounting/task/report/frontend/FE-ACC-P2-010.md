# Laporan Perubahan Frontend — `FE-ACC-P2-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-010` |
| Judul | Layar dan Form Aturan Posting |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-010` (revisi 4) |
| Trace | `FR-P2-007`, `FR-P2-008`; `ACC-DEC-045`, `ACC-DEC-058`, `ACC-DEC-064`, `ACC-DEC-074`; `03-frontend-architecture.md` bagian 10 butir 15 dan 16 |
| Contract version | `ACC-API-0.10` grup Posting Rule + penyesuaian `JournalTypeId` 14 Sep 2026; `GET /event-types/options`; `GET /master-data/journal-types/options`; `GET /master-data/chart-of-accounts/options`; `ACC-PERMISSION-0.5` `PostingRule : Read/Create/Update`. Bentuk payload dari **source** `BE-ACC-P2-018` |
| Wewenang UI | Kartu roadmap: daftar tersaring, form berbaris, control account tidak dimatikan. Warna, jarak, urutan kolom `DEV_DISCRETION` |
| Dependency | `BE-ACC-P2-017` 🟡, `BE-ACC-P2-018` ✅ — source ada; data tampil sesudah migration `BE-ACC-P2-016` (Rizki) |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2 (13 berkas), logika 2 (form berbaris, tiga sumber pilihan, aturan sisi), kontrak API 1, database 0, keamanan 1, UI 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan dan tautan bukti di repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `f6b1498fe` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `b3ab542e` + working tree `BE-ACC-P2-015`/`017`/`018` (branch `rizkiG`) |
| Tanggal | 14 September 2026 |
| Status | **🟡 SEBAGIAN** — 8 dari 8 acceptance terpetakan ke source; ESLint `0 error, 0 warning`; tinggal `npm run build` oleh owner. `IMPLEMENTATION COMPLETE`, `READY FOR UAT` sesudah migration `016` |

---

## 1. Keadaan yang ditemukan di awal

Tidak ada layar, slice, maupun route Aturan Posting. Backend `BE-ACC-P2-018` sudah menyediakan lima
endpoint; `BE-ACC-P2-017` menyediakan pilihan jenis kejadian. Keduanya menjawab `500` sampai tabelnya
dibuat migration `BE-ACC-P2-016`.

**Jebakan yang ditunjuk kartu, dan terbukti ada di source.** Util pemilih akun Form Jurnal —
`buildJournalAccountOption` di `journal-account-option-utils.jsx` — mengembalikan
`disabled: isControlAccount`. Dipakai apa adanya di sini, akun Kas Kasir, Piutang, dan Utang tidak
dapat dipilih, sehingga aturan posting untuk kejadian kasir mustahil disusun, padahal backend
sengaja menerimanya (`ACC-DEC-064`).

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Alur normal — menyusun aturan pendapatan rawat jalan dengan jasa medis

| Langkah | Pelaku | Yang terjadi di layar |
| ---: | --- | --- |
| 1 | Staf akuntansi | Membuka **Akuntansi › Master Data › Aturan Posting**, memilih badan hukum PT Metropolitan Medical Centre |
| 2 | Layar | Daftar aturan badan hukum itu; dapat disaring per jenis kejadian dan status |
| 3 | Staf | Menekan **+ Tambah Aturan Posting** |
| 4 | Staf | Kepala: Jenis Kejadian `PENGAKUAN-PIUTANG`, Jenis Jurnal `JU`, Perlakuan **Buat Draft** (bawaan) |
| 5 | Staf | Menyusun empat baris (lihat tabel 2.2), menekan **Simpan Aturan** |
| 6 | Layar | Toast "Aturan posting tersimpan dan aktif.", kembali ke daftar |

### 2.2 Isi baris pada contoh

| # | Komponen | Akun | Unit Biaya | Sisi |
| ---: | --- | --- | --- | --- |
| 1 | `TOTAL` | `1-1201 Piutang Penjamin (control account)` — **dapat dipilih** | — | Debit |
| 2 | `TOTAL` | `4-1001 Pendapatan Rawat Jalan` | — | Kredit |
| 3 | `JASA_MEDIS` | `5-3001 Beban Jasa Medis` — akun beban | **Poli Umum** (wajib) | Debit |
| 4 | `JASA_MEDIS` | `2-1301 Utang Jasa Medis Dokter` | — | Kredit |

Payload yang dikirim:

```json
{
  "legalEntityId": "<badan hukum terpilih>",
  "eventTypeId": "<PENGAKUAN-PIUTANG>",
  "journalTypeId": "<JU>",
  "treatment": 2,
  "lines": [
    { "lineNumber": 1, "componentCode": "TOTAL", "accountId": "…", "costCenterId": null, "side": 1, "description": null },
    { "lineNumber": 2, "componentCode": "TOTAL", "accountId": "…", "costCenterId": null, "side": 2, "description": null },
    { "lineNumber": 3, "componentCode": "JASA_MEDIS", "accountId": "…", "costCenterId": "…", "side": 1, "description": null },
    { "lineNumber": 4, "componentCode": "JASA_MEDIS", "accountId": "…", "costCenterId": null, "side": 2, "description": null }
  ]
}
```

### 2.3 Jalur tidak normal

| Keadaan | Yang dilihat pengguna | Asal aturan |
| --- | --- | --- |
| Badan hukum belum dipilih | Tambah mati — "Pilih badan hukum lebih dahulu."; daftar menampilkan seluruh badan hukum | Layar |
| Semua baris Debit | Simpan mati — "Aturan wajib memiliki sekurang-kurangnya satu baris debit dan satu baris kredit …" | Rumusan `SusunBarisAsync`; backend tetap `400` |
| Menghapus baris saat tinggal dua | Tombol Hapus mati | Backend `400` untuk < 2 baris |
| Akun beban tanpa unit biaya | Pesan di bawah isian "Unit biaya wajib untuk akun beban."; kewajibannya dari `requiresCostCenter` **respons backend** | Acceptance (3) |
| Akun induk | Tidak muncul di pilihan (`/options` hanya akun penerima transaksi); bila tetap terkirim, toast `422` apa adanya | Backend |
| Akun badan hukum lain | Tidak muncul di pilihan (disaring `legalEntityId`); bila tetap terkirim, toast `409` "Baris ke-2: …" | Backend |
| Jenis sudah punya aturan aktif | Toast `409` "Jenis kejadian PENGAKUAN-PIUTANG sudah punya aturan posting aktif …" | Backend |
| Mengubah aturan | Badan hukum dan Jenis Kejadian tampil **terkunci**; Jenis Jurnal, Perlakuan, dan seluruh baris dapat diubah | `UpdatePostingRuleRequest` |
| Mengubah aturan nonaktif | Peringatan kuning "Aturan ini sudah nonaktif …" | Temuan owner `BE-ACC-P2-018` |
| Menonaktifkan | Konfirmasi merah; tidak ada tombol Aktifkan — kolom Keaktifan menampilkan "Riwayat" | Backend tanpa `activate` |
| Tanpa `PostingRule : Create`/`Update` | Tombol mati + keterangan biru | Acceptance (6) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; kartu `FE-ACC-P2-010`; `PostingRuleController.cs`, `AccPostingRuleService.cs`,
`PostingRuleDtos.cs`, `AccountingEventTreatment.cs`, `PostingSide.cs` (`rizkiG`);
`recurring-journal-form-view.jsx`, `use-recurring-journal-editor.jsx`, `recurring-journal-view.jsx`
(pola form berbaris dan daftar ber-badan hukum); `journal-form-view.jsx` (`JournalLineRow`);
`journal-account-option-utils.jsx`; `journal-form-view.module.css`; `use-accounting-legal-entity.jsx`;
`master-data-resource-slice-factory.jsx`; `accounting-chart-of-account-slice.jsx`,
`accounting-journal-type-slice.jsx` (thunk `/options`); `base-form-control.jsx`; `store.jsx`;
`menu-items.jsx`; `globals.css` (arti `data-flat-table`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/posting-rule/posting-rule-constants.jsx` | **Baru.** Config, endpoint, hak, enum `Treatment`/`Side` beserta pilihan, baris kosong, salinan teks |
| `src/utils/corporate/accounting/posting-rule/posting-rule-utils.jsx` | **Baru.** `buildPostingRuleAccountOption` (control account tidak dimatikan), `hasBothPostingSides` |
| `src/lib/state/slice/corporate/accounting/accounting-posting-rule-slice.jsx` | **Baru.** Factory; lima thunk sesuai lima endpoint |
| `src/lib/hooks/corporate/accounting/posting-rule/use-posting-rule.jsx` | **Baru.** Daftar tersaring badan hukum dan jenis kejadian, hak, nonaktifkan |
| `src/lib/hooks/corporate/accounting/posting-rule/use-posting-rule-editor.jsx` | **Baru.** `react-hook-form` + `useFieldArray`; tiga sumber pilihan; payload persis DTO |
| `src/components/view/corporate/accounting/posting-rule/posting-rule-view.jsx` | **Baru.** Layar daftar |
| `src/components/view/corporate/accounting/posting-rule/form/posting-rule-form-view.jsx` | **Baru.** Form; `PostingRuleLineRow` lokal |
| `src/style/corporate/accounting/posting-rule-form-view.module.css` | **Baru.** Dua lebar kolom saja — Komponen dan Sisi |
| `src/app/corporate/accounting/posting-rules/page.jsx`, `posting-rule-client.jsx`, `create/page.jsx`, `[slug]/update/page.jsx` | **Baru.** Route tipis |
| `src/lib/state/store.jsx` | Registrasi `accountingPostingRule` |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir "Aturan Posting" tingkat 3 di Akuntansi › Master Data |

### 3.3 Kepatuhan arsitektur frontend

View → hook → slice factory → `InstanceAxios`. Pola daftar dan form disalin dari Jurnal Berulang
(Phase 2 terbaru), bukan dari standar master data HR — alasan yang sama dengan `FE-ACC-P2-009`
bagian 3.3. Nol komponen base diubah, nol perubahan `globals.css`, nol `style={{ }}`.

#### Gerbang keputusan base component

`UI GATE: 12 elemen — REUSE 11, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header | `Hero` | `recurring-journal-view.jsx` | REUSE | — |
| Pemilih badan hukum | `AccountingLegalEntitySelect` + `.contextCard` | `recurring-journal-view.jsx` | REUSE | Kelas dari `recurring-journal-view.module.css` |
| Filter jenis kejadian, status, jumlah baris, cari | `DataFilter`, `FilterSelect searchable` | `recurring-journal-view.jsx` | REUSE | — |
| Tabel berhalaman + status | `DataTable`, `StatusBadge`, `RegionPagination` | idem | REUSE | — |
| Tombol per baris dan Tambah (mati + alasan) | `BaseButton` | idem | REUSE | Kolom Ubah dan Keaktifan terpisah |
| Keterangan hak / control account / aturan nonaktif | `InformationAlert` | `recurring-journal-form-view.jsx` | REUSE | — |
| Konfirmasi dan notifikasi | `ConfirmModal`, `ToastStack` | `chart-of-account-view.jsx` | REUSE | — |
| Isian kepala (select) dan identitas terkunci | `BaseSelectField`, `BaseTextField disabled` | `recurring-journal-form-view.jsx` | REUSE | — |
| Kartu kepala, kartu baris, tombol aksi form | Kelas `journal-form-view.module.css` | `recurring-journal-form-view.jsx` baris 36 | REUSE | — |
| Pemilih unit biaya per baris | `ResourceFilterSelect` + `useSelectResource("costCenters")` | `JournalLineRow` | REUSE | — |
| Pemilih akun per baris | `BaseSelectField` + `buildPostingRuleAccountOption` | `JournalLineRow` | REUSE | Util baru membalik `disabled` saja |
| **Baris aturan posting** | `JournalLineRow` | Memuat Debit/Kredit bernominal, tidak memuat Komponen/Sisi | **COMPOSE** | Lihat keputusan di bawah |

**Keputusan: baris aturan posting**

- **A. Rangkai `PostingRuleLineRow` lokal dari field dasar yang sama dengan `JournalLineRow` — Rekomendasi, dijalankan.**
  Tampilan sel, pemilih akun, dan pemilih unit biaya identik dengan Form Jurnal karena kelas dan
  komponennya sama. Nol perubahan komponen bersama, jadi nol risiko regresi pada Form Jurnal,
  dialog penyesuaian, dan Form Jurnal Berulang. Biaya: ±150 baris JSX di satu berkas.
- **B. Tambah mode `postingRule` pada `JournalLineRow`.** Satu komponen baris untuk empat layar,
  tetapi mengubah komponen yang dipakai tiga layar yang sudah `READY FOR UAT` — setiap cabang
  kondisi baru berisiko mematikan kolom nominal di sana tanpa galat.
- **C. Tabel baris baru dengan gaya sendiri.** Paling bebas, tetapi menyimpang dari tampilan Form
  Jurnal dan menambah CSS tandingan.

**Temuan grep yang dipertahankan:** satu `<table>` mentah tanpa `data-flat-table="true"` di
`posting-rule-form-view.jsx`. Sama persis dengan Form Jurnal dan Form Jurnal Berulang yang memakai
kelas `lineTable` yang sama; atribut itu memberlakukan kontrak typography `globals.css` baris 245–256
pada seluruh `span`/`div` di dalam sel, yang akan menimpa typography `BaseSelectField` dan
`ResourceFilterSelect` di dalamnya.

**CSS Module baru** `posting-rule-form-view.module.css` hanya berisi `width` dua kolom — nol warna,
nol typography, nol spacing literal.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil aturan posting..."; form ubah: isian mati selama rincian dimuat |
| Kosong | "Aturan posting belum ada." — "Belum ada jenis kejadian yang dipetakan ke akun pada pilihan ini." |
| Gagal | Pesan backend di atas tabel/form; penolakan simpan sebagai toast merah apa adanya |
| Tanpa hak akses | Baca: `AccessDeniedGate`; Tambah/Ubah/Nonaktifkan/Simpan: tombol mati + keterangan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Master Data / Posting Rule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/posting-rules` | Daftar; query `legalEntityId`, `eventTypeId`, `isActive`, `search`, `pageNumber`, `pageSize` | `PostingRule : Read` |
| `GET` | `/v1/corporate/accounting/posting-rules/{id}` | Mengisi form ubah beserta baris | `PostingRule : Read` |
| `POST` | `/v1/corporate/accounting/posting-rules` | Tambah — bentuk bagian 2.2 | `PostingRule : Create` |
| `PUT` | `/v1/corporate/accounting/posting-rules/{id}` | Ubah — `{ journalTypeId, treatment, lines }` | `PostingRule : Update` |
| `PATCH` | `/v1/corporate/accounting/posting-rules/{id}/deactivate` | Nonaktifkan, tanpa badan | `PostingRule : Update` |

#### Corporate / Accounting / Master Data / Event Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/event-types/options` | Pilihan Jenis Kejadian (form) dan penyaring (daftar) | `EventType : Read` |

#### Corporate / Accounting / Master Data / Journal Type dan Chart of Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/master-data/journal-types/options?onlyActive=true` | Pilihan Jenis Jurnal | `JournalType : Read` |
| `GET` | `/v1/corporate/accounting/master-data/chart-of-accounts/options?legalEntityId=…` | Pilihan akun per badan hukum aturan, termasuk `requiresCostCenter` dan `isControlAccount` | `ChartOfAccount : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada seluruh berkas baru + `store.jsx` + `menu-items.jsx` | Exit `0`, keluaran kosong — **0 error, 0 warning** | `PASS` | Terminal agent, 14 Sep 2026 |
| Grep anti-regresi JSX | Satu `<table>` mentah — dipertahankan dengan alasan bagian 3.3; nol `<button`, `style={{`, `fw-`/`fs-` | `PASS` dengan catatan | `Select-String` |
| Grep anti-regresi CSS baru | Nol hex, `rgb`, typography, `!important` | `PASS` | idem |
| **Tiga arah (1) kolom layar ↔ respons** | `eventTypeCode`, `eventTypeName`, `legalEntityName`, `journalTypeCode`, `treatment`, `lineCount`, `isActive` ada di `PostingRuleListResponse`; baris form ↔ `PostingRuleLineResponse` (`componentCode`, `accountId`, `costCenterId`, `side`, `description`) | `PASS` | `PostingRuleDtos.cs` |
| **Tiga arah (2) payload ↔ isian** | Create: 5 kunci kepala + 6 kunci baris ↔ `CreatePostingRuleRequest`/`PostingRuleLineRequest`; Update: 3 kunci ↔ `UpdatePostingRuleRequest`. Nol kunci payload tanpa isian; `legalEntityId` dari pemilih badan hukum | `PASS` | idem |
| **Tiga arah (3) angka pilihan ↔ enum** | `Treatment` "1" Langsung Disahkan / "2" Buat Draft ↔ `LangsungSahkan = 1`, `BuatDraft = 2`; `Side` "1" Debit / "2" Kredit ↔ `Debit = 1`, `Kredit = 2`; dikirim `Number(...)` | `PASS` | `AccountingEventTreatment.cs`, `PostingSide.cs` |
| Control account tidak dimatikan | `buildPostingRuleAccountOption` mengembalikan `disabled: false`; `buildJournalAccountOption` milik Form Jurnal **tidak diubah** | `PASS` | `posting-rule-utils.jsx` |
| Pemanggilan endpoint sungguhan | Tidak dapat — tabel belum ada sampai `016` | `NOT RUN` | — |
| `npm run build` | Dijalankan owner — belum | `NOT RUN` | DoD kartu |

`AUTOMATED TEST: SKIPPED (opsional) — ACC-DEC-081.`

Uji manual: `NOT FEASIBLE` — agent tanpa peramban, dan kelima endpoint menjawab `500` sampai
migration `BE-ACC-P2-016`. Skenario bagian 2 diserahkan ke tim UAT sesudah migration.

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Daftar memakai `GET /posting-rules` dan dapat disaring per badan hukum dan jenis kejadian | **Terpenuhi** | `requestParams` membawa `legalEntityId` dari pemilih badan hukum dan `eventTypeId` dari `FilterSelect` |
| 2 | Form memuat jenis kejadian dari `/event-types/options`, jenis jurnal dari `/journal-types/options`, dan perlakuan yang angkanya cocok dengan enum backend | **Terpenuhi** | `getEventTypeOptions`, `getJournalTypeOptions`; verifikasi tiga arah (3) |
| 3 | Baris memuat komponen, akun, sisi Debit/Kredit, dan cost center; kewajiban cost center mengikuti respons backend | **Terpenuhi** | `PostingRuleLineRow`; `costCenterRequiredByIndex` dari `requiresCostCenter` |
| 4 | **Akun control account tidak dimatikan** pada pemilih akun layar ini | **Terpenuhi** | `buildPostingRuleAccountOption` + keterangan "Akun control account boleh dipilih di sini." |
| 5 | Penolakan `400`, `409`, dan `422` tampil apa adanya | **Terpenuhi** | Toast berisi `error.message` backend pada simpan dan nonaktifkan |
| 6 | Tombol dimatikan bagi yang tidak berhak | **Terpenuhi** | `createBlockedReason`, `updateBlockedReason`, `saveBlockedReason` |
| 7 | Keadaan memuat, gagal, dan kosong ditangani | **Terpenuhi** | Bagian 4 |
| 8 | `globals.css` tidak disentuh, tanpa `style={{ }}` | **Terpenuhi** | `git status`, grep |

| Butir DoD | Hasil |
| --- | --- |
| Lint hijau | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |
| Build dijalankan owner | **Belum** |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | (1) Penyaring `journalTypeId` dan `treatment` diterima backend tetapi tidak dirender — tidak diminta kartu. (2) Unit biaya baris tidak disaring per badan hukum; backend menolak `409` bila tidak cocok — perilaku sama dengan Form Jurnal. (3) Aturan nonaktif masih dapat diubah karena backend menerimanya; layar hanya memperingatkan — menunggu keputusan owner (temuan 1 `BE-ACC-P2-018`) |
| Dependency backend | `BE-ACC-P2-016` (migration, Rizki); `BE-ACC-P2-017` 🟡 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M store.jsx`, ` M menu-items.jsx` (bersama `009`), `?? src/app/corporate/accounting/posting-rules/`, `?? src/components/view/corporate/accounting/posting-rule/`, `?? src/lib/constants/corporate/accounting/posting-rule/`, `?? src/lib/hooks/corporate/accounting/posting-rule/`, `?? src/lib/state/slice/corporate/accounting/accounting-posting-rule-slice.jsx`, `?? src/utils/corporate/accounting/posting-rule/`, `?? src/style/corporate/accounting/posting-rule-form-view.module.css`. Nol commit |
| Langkah berikutnya | Owner: `npm run build`; migration `016`; beri hak `PostingRule : Read/Create/Update` dan `EventType : Read`. Tim UAT: skenario bagian 2 sesudah migration |
