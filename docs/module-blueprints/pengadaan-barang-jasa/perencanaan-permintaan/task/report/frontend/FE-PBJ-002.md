# Laporan Perubahan Frontend — `FE-PBJ-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-002` |
| Judul | Komponen lampiran |
| Slice | Gelombang 1 (`MVP-0`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | API §Attachment; `PBJ-VAL-080`..`083`; `BE-PBJ-004` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant |
| Dependency | `BE-PBJ-004` ✅ |
| Klasifikasi | `LIGHT` — 1 komponen fitur, 1 hook, 1 service |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — kriteria terbukti di layar PR (`FE-PBJ-003`) |

---

## 1. Keadaan yang ditemukan di awal

Belum ada komponen lampiran Pengadaan. Backend (`BE-PBJ-004`) menyediakan daftar lampiran per dokumen, unggah multipart dengan versi pengganti, unduh, dan tarik lampiran, serta validasi ekstensi, tanda tangan isi, ukuran, dan dokumen final.

---

## 2. Proses bisnis dari sisi pengguna

1. Pada tab **Lampiran** dokumen (PR, rencana kebutuhan, pagu, rencana paket), pengguna mengisi kategori (mis. "Penawaran") lalu menekan **+ Unggah Lampiran** dan memilih berkas.
2. Berkas berekstensi di luar PDF/JPG/PNG/DOCX/XLSX atau di atas 10 MB dicegah di peramban dengan pesan "Jenis berkas tidak diizinkan. Gunakan PDF, JPG, PNG, DOCX, atau XLSX." — server tidak dipanggil.
3. Berkas yang lolos dikirim; penolakan server tampil terbaca, misalnya `palsu.pdf` berisi HTML → "Isi berkas tidak sesuai dengan jenisnya." (`PBJ-VAL-081`).
4. **Ganti versi** pada baris lampiran mengunggah versi baru; daftar menampilkan versi baru, dan **Tampilkan semua versi** memperlihatkan versi lama.
5. **Unduh** mengambil berkas lewat endpoint `download`; **Tarik** menonaktifkan lampiran.
6. Dokumen yang tidak dapat diubah (`readOnly`) atau pengguna tanpa `ProcurementAttachment : Upload` hanya melihat daftar dan Unduh.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`api-contract.md` §Attachment, `validation-matrix.md` (`PBJ-VAL-080`..`083`), komponen base unggah (tidak ada base uploader; dipakai `BaseButton` + input file tersembunyi).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/corporate/procurement/procurement-attachment-service.js` | Daftar, unggah `FormData`, unduh blob + nama dari `Content-Disposition`, tarik |
| `src/lib/hooks/corporate/procurement/use-procurement-attachments.jsx` | Validasi awal ekstensi/ukuran, unggah/ganti versi, unduh, tarik, tampil semua versi |
| `src/components/view/corporate/procurement/shared/procurement-attachment-panel.jsx` | Komponen `documentType`, `documentId`, `readOnly`; tabel baca-saja ber-`data-flat-table` |
| `src/components/view/corporate/procurement/shared/procurement-document-tabs.jsx` | Tab Lampiran/Persetujuan/Riwayat yang memasang panel lampiran pada keempat jenis dokumen |

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
| Memuat | "Memuat lampiran..." dan tombol unggah ber-label "Mengunggah..." |
| Kosong | "Belum ada lampiran." |
| Gagal | `InformationAlert` berisi pesan backend |
| Tanpa hak akses | Tombol unggah, ganti versi, dan tarik tersembunyi |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Common / Attachment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/attachments?documentType=&documentId=&includeHistory=` | Daftar versi berlaku / semua versi | `ProcurementAttachment : Read` |
| `POST` | `/v1/corporate/procurement-management/attachments` | Unggah dan ganti versi (`replacesAttachmentId`) | `ProcurementAttachment : Upload` |
| `GET` | `/v1/corporate/procurement-management/attachments/{id}/download` | Unduh | `ProcurementAttachment : Read` |
| `POST` | `/v1/corporate/procurement-management/attachments/{id}/deactivate` | Tarik lampiran | `ProcurementAttachment : Upload` |


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
| Unggah `penawaran-v1.pdf` pada PR draf | 200, baris tampil | `PASS` | `ui-results-d.json` |
| Ganti versi → `penawaran-v2.pdf`; semua versi memperlihatkan v1 | Sesuai | `PASS` | `d-02-versions.png` |
| `palsu.pdf` (isi HTML) → 422 "Isi berkas tidak sesuai dengan jenisnya." | Sesuai | `PASS` | `ui-results-d.json` |
| `program.exe` dicegah di klien; tidak ada POST | Sesuai | `PASS` | `ui-results-d.json` |
| Dokumen tidak dapat diubah (PR-000014) → tanpa tombol unggah | Sesuai | `PASS` | `b-12-complete.png` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Penolakan server `PBJ-VAL-080`..`083` tampil terbaca | Terpenuhi | `PBJ-VAL-081` diuji di layar; semua kode memakai jalur yang sama (`readApiError` → pesan backend). 080 dicegah lebih dulu oleh validasi klien; 082/083 tidak dipicu terpisah |
| (2) Unggah pengganti menampilkan versi baru | Terpenuhi | `penawaran-v2.pdf` |
| (3) `readOnly` menyembunyikan unggah | Terpenuhi | PR-000014 |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | — |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
