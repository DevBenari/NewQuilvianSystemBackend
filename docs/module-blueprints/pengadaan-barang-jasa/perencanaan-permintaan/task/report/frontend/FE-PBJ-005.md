# Laporan Perubahan Frontend — `FE-PBJ-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-005` |
| Judul | Verifikasi dan persetujuan |
| Slice | Gelombang 4 (`MVP-1`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-11`, `-12`, `-13`, `-17`, route 11.2; API §Verification, `budgets/remaining`; kotak masuk `Wfl`; `BE-PBJ-008` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-004` ✅, `BE-PBJ-008` ✅ |
| Klasifikasi | `HEAVY` — ruang kerja verifikasi, 3 daftar, pintu kotak masuk `Wfl`, panel persetujuan |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Backend `BE-PBJ-008` menyediakan antrean verifikasi, cek stok per baris, selesaikan verifikasi yang membuat instance `Wfl`, kembalikan, tolak, batalkan PR disetujui, dan sinkronisasi status. Kotak masuk persetujuan `Wfl` sudah ada di modul HR.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Tim Pengadaan membuka **Verifikasi Kebutuhan** (atau **Validasi Pengajuan**, rute kedua layar yang sama). Antrean berisi PR "Menunggu Verifikasi" dengan umur antrean; PR mendesak tampil lebih dulu di halaman yang sama.
2. Membuka baris → **Ruang kerja verifikasi**: isi Stok Tersedia, Dipenuhi Stok, dan Sumber Stok per baris → **Simpan cek stok**. Kebutuhan beli dan nilai beli dihitung server.
3. **Selesaikan verifikasi** (konfirmasi) → status "Menunggu Persetujuan"; tanda "Melebihi pagu Rp …" tampil bila nilai beli melewati sisa pagu. Bila definisi persetujuan tidak aktif: "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator."
4. **Kembalikan** dan **Tolak** mewajibkan alasan. **Batalkan** tersedia untuk PR "Disetujui" (alasan wajib). **Sinkronkan status** tersedia saat menunggu persetujuan.
5. Pembuat PR yang membuka ruang kerja tidak melihat tombol verifikasi; muncul info "Anda pembuat pengajuan ini; verifikasi dilakukan anggota Tim Pengadaan lain."
6. **Pemeriksaan Stock** menampilkan hasil cek stok per baris (pemeriksa, waktu); membuka baris menuju ruang kerja.
7. Penyetuju membuka **Antrean Persetujuan**: tugas `Wfl` miliknya untuk dokumen Pengadaan (saring jenis dokumen; tampilan Perlu tindakan/Sudah diproses/Semua). **Setujui** (konfirmasi) dan **Minta revisi** (alasan wajib) langsung dari baris; **Lihat** membuka dokumen. Menolak dikerjakan di Kotak Masuk Terpadu karena memerlukan alasan baku `Wfl`.
8. Tab **Persetujuan** pada detail menampilkan langkah, status, penyetuju, dan keputusan dari instance `Wfl`; ringkasan **sisa pagu** per kategori tampil bagi pemegang `ProcurementBudget : Read`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/procurement/requests/purchase-request-detail-view.jsx` | `mode="verify"`: isian cek stok, aksi verifikator, info pembuat, ringkasan sisa pagu |
| `src/components/view/corporate/procurement/verification/stock-check-list-view.jsx` | Layar `-13` |
| `src/components/view/corporate/procurement/approvals/procurement-approval-inbox-view.jsx` | Layar `-17` |
| `src/components/view/corporate/procurement/shared/procurement-approval-panel.jsx` | Tab Persetujuan dari instance `Wfl` |
| `src/lib/services/corporate/procurement/procurement-approval-service.js`, `budget-service.js` | Kotak masuk, rincian instance, sisa pagu |
| `src/app/corporate/procurement/requests/verification/page.jsx`, `requests/verification/[slug]/page.jsx`, `requests/stock-checks/page.jsx`, `approvals/inbox/page.jsx`, `approvals/validation/page.jsx` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir Verifikasi Kebutuhan, Pemeriksaan Stock, Antrean Persetujuan, Validasi Pengajuan |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi mengikuti `frontend-architecture.md`: `src/app/corporate/procurement/**/page.jsx` hanya entry point dan `metadata` → view di `src/components/view/corporate/procurement/**` → hook di `src/lib/hooks/corporate/procurement/**` → service `InstanceAxios` di `src/lib/services/corporate/procurement/**`; konstanta di `src/lib/constants/corporate/procurement/procurement-constants.jsx`; fungsi murni di `src/utils/corporate/procurement/procurement-utils.jsx`; gaya di `src/style/corporate/procurement/procurement.module.css` (token saja, tanpa warna literal, tanpa `!important`, tanpa inline style). Tidak ada Redux slice baru: state layar bersifat lokal per layar dan dibaca ulang dari server (roadmap mengunci service per dokumen). Deviasi terhadap checklist yang dipertahankan: definisi kolom tabel ditulis di dalam view (bukan berkas `-table-columns.jsx` terpisah) dan layar form/detail memakai susunan `COMPOSE` (lihat keputusan di bawah). Aturan tipografi pada `procurement.module.css` hanya menyasar kelas fitur (judul kartu, teks redup, tabel baris), memakai token, dan tidak menimpa komponen shared.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman dan aksi utama | `REUSE` | `Hero` (`actions`) |
| Kartu ringkasan angka | `REUSE` | `SummaryGrid` |
| Saringan, pencarian, atur ulang | `REUSE` | `DataFilter` + `FilterSelect` |
| Tabel daftar dan paginasi | `REUSE` | `DataTable` + `Pagination` lewat `PaginationComponent` |
| Label status dokumen | `WRAP` | `StatusBadge` dibungkus `procurement-status-badge.jsx` (peta kode status → label Bahasa Indonesia + nada warna; teks selalu tampil, BRD B10) |
| Konfirmasi dan alasan wajib | `REUSE` | `ConfirmModal` (`requireReason`) |
| Notifikasi hasil aksi | `REUSE` | `ToastStack` |
| Pesan galat / info | `REUSE` | `InformationAlert`, `AccessDeniedGate` |
| Tombol | `REUSE` | `BaseButton` |
| Isian form | `REUSE` | `BaseTextField`, `BaseInputField`, `BaseSelectField`, `BaseDateField`, `BaseTextAreaField`, `BaseCheckboxField` |
| Tabel baris dokumen yang dapat diisi | `COMPOSE` | `<table>` + field base di dalam sel, mengikuti `journal-form-view.jsx` (Akuntansi); tabel baca-saja memakai `data-flat-table="true"` |
| Susunan halaman form/detail dokumen | `COMPOSE` | `Hero` + kartu bagian (`procurement.module.css`) + `ConfirmModal` + `ToastStack` |
| Tab Lampiran / Persetujuan / Riwayat | `COMPOSE` | `BaseButton` ber-`role="tab"` (belum ada base Tabs; pola sama dengan layar Akuntansi) |

Pilihan untuk elemen bukan `REUSE`:

1. **Susunan form/detail dokumen**
   1. `COMPOSE` Hero + kartu bagian, seperti yang dibangun **(rekomendasi)** — satu layar melayani mode draf (ubah) dan mode baca/verifikasi beserta tab alur; konsistensi visual terjaga karena semua bagian memakai token dan base field; risiko regresi nol pada komponen shared; biaya rendah.
   2. Migrasi ke `BaseDetailView` dengan isi editor sebagai `children` — lebih seragam dengan layar detail Akuntansi, tetapi mode ubah perlu dipisah ke `BaseEditorView` sehingga satu dokumen punya dua layar; biaya sedang.
2. **Tabel baris yang dapat diisi**
   1. `COMPOSE` tabel + field base **(rekomendasi)** — sama dengan baris jurnal; tidak menyentuh `DataTable`.
   2. `EXTEND` `DataTable` agar menerima sel isian — menyentuh komponen shared; risiko regresi di semua modul.
3. **Tab**
   1. `COMPOSE` `BaseButton role="tab"` **(rekomendasi)** — tanpa komponen baru.
   2. `NEW` komponen Tabs bersama — butuh keputusan pengguna; tidak dikerjakan.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` yang mengubah perilaku bawaan base component.


---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil antrean persetujuan..." / "Memuat rincian persetujuan..." |
| Kosong | "Tidak ada pengajuan yang menunggu verifikasi." / "Tidak ada tugas persetujuan pada saringan ini." / "Belum masuk persetujuan." |
| Gagal | Pesan + **Coba lagi** |
| Tanpa hak akses | Antrean persetujuan tanpa `ApprovalInbox : Read` → "Akses Ditolak"; tab Persetujuan tanpa `WorkflowInstance : Read` → "Anda belum berhak melihat rincian langkah persetujuan."; ringkasan pagu tersembunyi tanpa `ProcurementBudget : Read` (tidak memanggil API sebelum daftar izin termuat) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Request / Verification

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/purchase-request-verifications` | Antrean verifikasi | `ProcurementRequestVerification : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-request-verifications/stock-checks` | Hasil cek stok | `ProcurementRequestVerification : Read` |
| `PUT` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/stock-checks` | Simpan cek stok | `ProcurementRequestVerification : Verify` |
| `POST` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/complete` | Selesaikan (`Idempotency-Key`) | `ProcurementRequestVerification : Verify` |
| `POST` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/return` | Kembalikan | `ProcurementRequestVerification : Verify` |
| `POST` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/reject` | Tolak | `ProcurementRequestVerification : Verify` |
| `POST` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/cancel-approved` | Batalkan PR disetujui | `ProcurementRequestVerification : CancelApproved` |
| `POST` | `/v1/corporate/procurement-management/purchase-request-verifications/{id}/workflow/synchronize` | Sinkronkan status | `ProcurementRequestVerification : Verify` |
#### Corporate / Procurement Management / Planning / Budget

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/budgets/remaining` | Ringkasan sisa pagu di detail PR | `ProcurementBudget : Read` |
#### Corporate / Human Resource / Workflow Management / Approval Inbox

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/approval-inbox?referenceType=&view=&period=all` | Antrean persetujuan Pengadaan | `ApprovalInbox : Read` |
| `POST` | `/v1/corporate/human-resource/approval-inbox/{assignmentId}/approve` | Setujui | `ApprovalInbox : Approve` |
| `POST` | `/v1/corporate/human-resource/approval-inbox/{assignmentId}/request-revision` | Minta revisi | `ApprovalInbox : RequestRevision` |
#### Corporate / Human Resource / Workflow Management / Workflow Instance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workflow-instances/{id}` | Tab Persetujuan | `WorkflowInstance : Read` |


---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`; panggilan API `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (sumber `sukmagp` berisi BE-PBJ-002..011) dengan database `QuilvianNewDevSukma`. Aktor uji non-SuperAdmin `TEST-PBJ … 20261116110649` (pengusul IGD `reqA`, kepala IGD `headA`, Tim Pengadaan `proc`, Manajer `mgr`, Direktur `dirk`, Keuangan `keu`) memakai bundel izin `permission-audit-matrix.md` §3. Tangkapan layar tersimpan di folder `shots/` scratchpad sesi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada berkas Pengadaan | 0 error, 9 warning `react-hooks/set-state-in-effect` (pola muat-data di efek, sama dengan modul lain) | `PASS` | keluaran perintah, 16 November 2026 |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fe-lint3.log` scratchpad |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 4800 test, 4749 lulus, 51 gagal — seluruhnya modul lain; 8 berkas gagal yang membaca menu diuji A/B dengan `menu-items.jsx` versi `HEAD` dan hasilnya identik; tidak ada test yang mengimpor berkas Pengadaan | `EXISTING / ENVIRONMENT ISSUE` | baseline 28 Okt 2026 tercatat 41 gagal di modul lain |
| `npm run build` | `Compiled successfully`; 24 rute `/corporate/procurement/**` terbentuk | `PASS` | `fe-build4.log` scratchpad |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: test baru tidak wajib; logika murni berada di procurement-utils.jsx` | `NOT RUN` | — |
| -11 antrean dan route kedua -11.2 | Sesuai | `PASS` | `a-11-queue.png` |
| -13 cek stok (`status=SUBMITTED&isChecked=true`) | Sesuai | `PASS` | `ui-results-a.json` |
| -12 simpan cek stok + selesaikan → "Menunggu Persetujuan", tanda "Melebihi Pagu Rp 150.000" | Sesuai | `PASS` | `b-12-complete.png` |
| -12 pembuat PR membuka ruang kerja → tanpa tombol verifikasi | Sesuai | `PASS` | `ui-results-b.json` |
| -12 Kembalikan (alasan) → "Dikembalikan"; pengusul mengajukan ulang | Sesuai | `PASS` | `ui-results-g.json` |
| -12 `PBJ-VAL-091` saat definisi `Wfl` PBJ-PR dimatikan sementara | Pesan persis tampil; definisi diaktifkan lagi | `PASS` | `g-12-val091.png` |
| -12 Tolak (alasan) → "Ditolak" | Sesuai | `PASS` | `h-12-rejected.png` |
| -17 kepala unit, manajer (2 langkah), direktur menyetujui dari antrean | 4 × 200 | `PASS` | `ui-results-c.json`, `ui-results-e.json` |
| -10 status PR berubah setelah dimuat ulang → "Disetujui" | Sesuai | `PASS` | `e-10-approved.png` |
| -12 Batalkan PR disetujui (alasan) → "Dibatalkan" | Sesuai | `PASS` | `e-12-cancel-approved.png` |
| -17 Sudah diproses + Lihat → detail PR | Sesuai | `PASS` | `ui-results-c.json` |
| -17 pengguna tanpa `ApprovalInbox` → "Akses Ditolak" | Sesuai | `PASS` | `ui-results-a.json` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tombol verifikasi tersembunyi bagi pembuat PR | Terpenuhi | Uji fase B |
| (2) `PBJ-VAL-091` tampil "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator." | Terpenuhi | Uji fase G |
| (3) Tanda lewat pagu dan nilainya tampil setelah verifikasi | Terpenuhi | "Melebihi Pagu Rp 150.000" |
| (4) Penyetuju dapat menyetujui dari kotak masuk, dan status PR berubah setelah dimuat ulang | Terpenuhi | PR-000014 → "Disetujui" |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | (a) Backend tidak mengurutkan antrean "mendesak dulu"; FE mengurutkan dalam satu halaman. (b) Tolak tidak tersedia di `-17` karena butuh `RejectionReasonId` master `Wfl`; disediakan tombol ke Kotak Masuk Terpadu. (c) `budgets/remaining` memakai `ProcurementBudget : Read` (blueprint menulis `ProcurementRequest : Read`); pengusul tanpa izin pagu tidak melihat ringkasan. (d) Kotak masuk `view=open` juga memuat langkah yang belum giliran; label dibedakan "Perlu tindakan" vs "Menunggu giliran" dan tombol hanya pada tugas aktif |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Data uji | Definisi `Wfl` PBJ-PR dimatikan sementara untuk uji VAL-091 lalu diaktifkan lagi (diverifikasi `IsActive=true`, `Active`) |
| Langkah berikutnya | Usulan backend: urutan antrean `urgencyLevel desc`; izin `WorkflowInstance : Read` pada peran pemantau |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |

## Pembaruan 16 November 2026 (malam) — tab Persetujuan dan antrean (`PBJ-DEC-040`)

| Perubahan | Berkas | Bukti |
| --- | --- | --- |
| Tab Persetujuan memanggil `GET /{dokumen}/{id}/approval` (izin baca dokumen) menggantikan `GET workflow-instances/{id}`; pengusul kini melihat langkah dan keputusan | `shared/procurement-approval-panel.jsx`, `shared/procurement-document-tabs.jsx`, `procurement-approval-service.js` (`fetchDocumentApproval`), konstanta (endpoint/izin `WorkflowInstance` dihapus) | PR-000014 oleh pengusul: 4 langkah "Disetujui" beserta penyetuju; tidak ada panggilan `workflow-instances` |
| Antrean verifikasi diurutkan server (mendesak lebih dulu); pengurutan sisi klien dihapus | `requests/purchase-request-list-view.jsx` | `PR-000017` (mendesak) di baris pertama |

Endpoint baru yang dikonsumsi:

#### Corporate / Procurement Management / Request / Purchase Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/purchase-requests/{id}/approval` | Tab Persetujuan PR | `ProcurementRequest : Read` |

Endpoint `GET /{id}/approval` yang sama dipakai untuk rencana kebutuhan (`ProcurementNeedPlan : Read`), pagu
(`ProcurementBudget : Read`), dan rencana paket (`ProcurementPackagePlan : Read`).

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas Pengadaan | 0 error, warning `set-state-in-effect` pola muat-data | `PASS` |
| `npm run lint:errors` dan `npm run build` | exit 0; 24 rute `/corporate/procurement/**` | `PASS` |
| Uji layar `ui-results-j.json` (tab Persetujuan pengusul, antrean mendesak) | Sesuai | `PASS` |
