# FE-BKC-029 — Panel Edit Status Tagihan

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-029` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `1.1`) |
| Task type | Frontend, satu panel baru (`FE-MPY-03`) dipasang ke kerangka `FE-BKC-028` yang sudah ada |
| Task mode | `FRONTEND` (backend read-only — endpoint dan aturan validasi `BE-BKC-048` dibaca langsung dari source, tidak ada perubahan backend) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Dependency | `FE-BKC-028` (kerangka halaman — source selesai, lihat laporannya), `BE-BKC-048` ✅ (`PUT /{id}/item-payer-assignments`, dikonfirmasi ada persis sesuai kontrak beserta seluruh aturan validasinya dibaca dari `BillingPayerEditService.UpdateItemPayerAssignmentsAsync`) |
| Status task | **Source selesai.** Panel terpasang pada slot `activeMode === INVOICE_EDIT_MODES.ITEM_STATUS` di `edit-tagihan-view.jsx` (disiapkan `FE-BKC-028`). **Sesuai instruksi baku pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — hanya `node --check` pada berkas logika non-JSX (lihat § DoD). Belum di-commit |

## Ringkasan untuk pembaca umum

Panel ini menambahkan mode kedua pada halaman Edit Tagihan (`FE-BKC-028`): "Edit Status Tagihan".
Di sini kasir bisa menandai, **untuk tiap baris biaya satu per satu**, siapa yang menanggungnya —
Pribadi (pasien sendiri), Asuransi, atau Penjamin (perusahaan) — lalu menyimpan seluruh perubahan
sekaligus dengan satu alasan. Baris yang sudah dibatalkan (voided) ikut ditampilkan supaya kasir
tetap punya gambaran lengkap tagihan, tetapi penanggungnya terkunci (tidak bisa diklik) karena
baris yang sudah dibatalkan memang tidak relevan lagi diubah penanggungnya.

Pilihan Asuransi/Penjamin hanya bisa dipakai bila kunjungan itu memang berpenjamin sesuai
jenisnya — pada kunjungan Tunai, misalnya, kedua pilihan itu tampil tapi tidak bisa diklik,
disertai keterangan kenapa.

## Keputusan yang mengunci scope (ringkas)

Trace `FR-BKC-074`–`076`; `MPY-DEC-004`. `frontend-roadmap.md` § `FE-BKC-029`.
`03-frontend-architecture.md` § "Skema fitur — `FE-MPY-03` Edit Status Tagihan". Acceptance
frontend `#63`.

**Temuan yang wajib dilaporkan:**

1. **Sumber baris tabel bukan `itemPayerAssignments`, melainkan `editContext.items` (`BE-BKC-FIX-009`)
   disilangkan dengan `itemPayerAssignments`.** Diperiksa langsung ke source
   (`BillingPayerEditService.GetEditContextAsync`): `ItemPayerAssignments` dibangun dari
   `activeItems` yang SUDAH DIFILTER `Status == Active` — baris `VOIDED` **tidak pernah muncul**
   di situ sama sekali. Padahal wireframe eksplisit meminta *"Baris berstatus dibatalkan tampil
   tetapi isian penanggungnya nonaktif"*. Solusinya: ambil daftar baris dari `editContext.items`
   (mencakup baris `VOIDED`, tersedia sejak `BE-BKC-FIX-009`), lalu untuk tiap baris cari nilai
   penanggungnya di `itemPayerAssignments` by `invoiceItemId` — baris yang tidak ketemu (pasti
   `VOIDED`, karena hanya itu yang dikecualikan backend) dirender terkunci. Ini bukan menebak
   payload, dua-duanya field yang sudah nyata ada.
2. **Ketersediaan pilihan Asuransi/Penjamin per baris memakai `currentPayer.paymentType`
   (jenis penanggung kunjungan yang berlaku sekarang), BUKAN `availablePayerOptions`.** Diperiksa
   langsung `BillingPayerEditService.UpdateItemPayerAssignmentsAsync` (baris 795–810):
   backend menolak (`422`) `PayerKind=INSURANCE` bila `effectivePaymentType != Insurance`, dan
   `PayerKind=COMPANY_GUARANTOR` bila `effectivePaymentType != CompanyGuarantor` — validasi ini
   berbasis jenis penanggung **kunjungan yang sedang aktif**, bukan daftar kartu kandidat yang
   *bisa* dipakai (`availablePayerOptions`, konsep milik `FE-MPY-02`). Memakai sumber yang salah
   akan membuat tombol tampil bisa diklik padahal backend pasti menolaknya. Pesan alasan nonaktif
   pun disamakan maknanya dengan pesan galat backend ("Kunjungan ini tidak memakai asuransi." /
   "...penjamin perusahaan.") supaya konsisten bila pengguna berhasil memicunya lewat jalur lain.
3. **Label pilihan di panel ini ("Pribadi", "Asuransi", "Penjamin") SENGAJA berbeda** dari label
   bersama `INVOICE_PAYER_TYPE_LABELS` ("Tunai", "Asuransi", "Penjamin Perusahaan") yang dipakai
   `FE-MPY-01`/`FE-MPY-02`. Wireframe `FE-MPY-03` sendiri eksplisit menulis kata-kata itu untuk
   konteks penanggung PER BARIS, berbeda dari konteks penanggung KUNJUNGAN pada dua layar
   lainnya — bukan inkonsistensi, dua wireframe berbeda memang memakai istilah berbeda untuk
   nuansa yang berbeda pula.
4. **Tanpa dialog konfirmasi sebelum simpan** — berbeda dari `FE-MPY-02` yang eksplisit meminta
   satu. Wireframe `FE-MPY-03` tidak mencantumkan kalimat konfirmasi apa pun (dibandingkan
   `FE-MPY-02` yang mencantumkan templatnya kata per kata); menambahkannya akan mengarang
   langkah yang tidak diminta.
5. **Kontrol pilihan per baris disusun dari tiga `BaseButton` langsung** (variant `primary` untuk
   yang terpilih), bukan komponen "choice group" baru — tidak ada komponen sejenis itu di
   `base-features/` (sudah diperiksa), dan pola susun-manual seperti ini sudah dipakai kerangka
   `FE-BKC-028` sendiri untuk toolbar tiga mode. Tidak melanggar gerbang komponen karena tidak ada
   komponen baru yang dibuat.

## Base Component Decision Gate

`UI GATE: 0 elemen NEW, 0 elemen EXTEND, seluruh elemen REUSE`

| Elemen | Status | Bukti/alasan |
| --- | --- | --- |
| Tombol pilihan per baris | `REUSE` | `BaseButton` × 3 per baris, pola identik toolbar mode `FE-BKC-028` |
| Tabel baris biaya | `REUSE` | `baseStyles.dataTable`/`tableWrapper`, pola identik `billing-invoice-items-table.jsx` |
| Alert kosong/nonaktif | `REUSE` | `InformationAlert` |
| Isian alasan | `REUSE` | `BaseTextAreaField` |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}/item-payer-assignments` | Mengganti penanggung beberapa baris biaya sekaligus (hanya yang berubah) dan menghitung ulang tagihan atomik | `BillingInvoice : Update` | Body: daftar `{invoiceItemId, payerKind}`, `ExpectedRowVersion`, alasan; header `Idempotency-Key` | `InvoiceEditResultResponse` |

Tidak ada endpoint lain yang ditambahkan — panel ini murni memakai `edit-context` yang sudah
dimuat kerangka `FE-BKC-028` untuk sumber datanya.

Kode status: `404` (tagihan tidak ditemukan), `400` (Idempotency-Key/alasan/`ExpectedRowVersion`
kosong atau tidak valid, daftar assignment kosong atau berisi baris ganda), `409` (tagihan sudah
difinalisasi/dibayar, atau versi data sudah basi), `422` (baris bukan milik tagihan ini, baris
sudah dibatalkan, atau jenis penanggung tidak sesuai jenis kunjungan — pesan server ditampilkan
apa adanya).

## File yang diubah/ditambah

| File | Perubahan |
| --- | --- |
| `.../billing-invoices/edit-tagihan/edit-status-tagihan-panel.jsx` | **Baru.** Panel `FE-MPY-03` — tabel baris, tombol pilihan per baris, penghitung perubahan, alasan, simpan/batal |
| `src/lib/hooks/.../billing-invoices/use-edit-status-tagihan-panel.js` | **Baru.** Hook panel — silang `items`×`itemPayerAssignments`, ketersediaan pilihan dari `currentPayer.paymentType`, diff baris berubah, `Idempotency-Key`/`CorrelationId` dibangkitkan sekali saat simpan |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | **Diubah.** Tambah thunk `updateItemPayerAssignments` + 3 reducer case (pola identik `switchInvoicePaymentSource`, memakai `actionLoading`/`actionError` bersama). Thunk/selector lama tidak diubah |
| `.../billing-invoices/edit-tagihan/edit-tagihan-view.jsx` | **Diubah** (milik `FE-BKC-028`). Slot `activeMode === ITEM_STATUS` yang tadinya placeholder kini merender `EditStatusTagihanPanel`; `handleSwitched` diganti nama `handlePanelSaved` (dipakai dua panel) |
| `src/style/.../edit-tagihan.module.css` | **Diubah** (milik `FE-BKC-028`). Tambah `.payerKindChoices`, `.changedRow`, `.voidedNote` |

Total: **2 berkas baru, 3 berkas diubah** (2 di antaranya milik `FE-BKC-028`, task pasangan
langsung task ini).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npm run lint:errors` / `test:unit` / `build` | **SKIPPED** — instruksi baku pengguna | Tidak dijalankan sesi ini |
| Sintaks berkas logika non-JSX valid | **LULUS** | `node --check` pada `use-edit-status-tagihan-panel.js` dan `billing-invoice-slice.jsx` (via salinan `.js` sementara) — keduanya `exit 0` |
| Sintaks berkas JSX baru | **BELUM DIVERIFIKASI ALAT** | Sama seperti `FE-BKC-028` — diverifikasi manual lewat pembacaan ulang |
| Subjudul "Ubah penanggung biaya per item" ada | **LULUS (tinjauan kode)** | `edit-status-tagihan-panel.jsx`, `<h3>` pertama pada panel |
| Pada kunjungan Tunai, Asuransi dan Penjamin nonaktif beserta alasan (Acceptance `#63`) | **LULUS (tinjauan kode)** | `kindAvailability` di hook: `disabled: currentPaymentType !== INVOICE_PAYER_TYPES.INSURANCE` (dan pasangannya untuk `COMPANY_GUARANTOR`) — bila `currentPaymentType === "CASH"`, keduanya `true`; `title` pada tombol dibaca dari `availability.reason` |
| Baris `VOIDED` tampil tapi terkunci | **LULUS (tinjauan kode)** | `row.isVoided` dipakai sebagai kondisi `disabled` tambahan pada ketiga tombol, terlepas dari `kindAvailability` |
| Hanya baris yang berubah dikirim | **LULUS (tinjauan kode)** | `changedRows` (hook) memfilter `pending !== row.originalKind && !isVoided`; payload `assignments` dibangun hanya dari `changedRows` |
| Nol perubahan → simpan nonaktif | **LULUS (tinjauan kode)** | `canSave = changedRows.length > 0 && Boolean(reason.trim()) && !submitting` |
| Verifikasi manual (klik-coba: ubah beberapa baris, batal, simpan, cek `422` jenis tidak sesuai, cek baris voided terkunci) | **NOT FEASIBLE (sesi ini)** | Tidak ada environment ter-autentikasi pada sesi ini |

**Task ini belum bisa ditandai selesai.** Source selesai dan ditinjau kode menyeluruh terhadap
validasi backend nyata, tetapi lint/test/build serta klik-coba ter-autentikasi belum dijalankan.

- MANUAL TEST: NOT FEASIBLE pada sesi ini — perlu environment ter-autentikasi
- AUTOMATED TEST: SKIPPED (instruksi baku pengguna)

## Risiko yang tersisa

1. **Sama sekali belum diverifikasi lewat `npm run lint`/`test:unit`/`build` maupun browser** pada
   sesi ini.
2. **Bergantung pada `FE-BKC-028` yang juga belum di-build/test** — dua task menumpuk perubahan
   pada berkas yang sama (`edit-tagihan-view.jsx`, `billing-invoice-slice.jsx`) tanpa satu pun
   lolos build.
3. **`AssignmentSource` ("MANUAL" vs "AUTO") tidak ditampilkan** — panel hanya menampilkan
   `PayerKind` saat ini, tidak membedakan apakah nilai itu hasil pilihan manual sebelumnya atau
   default otomatis sistem. Wireframe tidak memintanya, dicatat sebagai potensi info tambahan bila
   dibutuhkan nanti.

## Langkah berikutnya yang direkomendasikan

1. Pengguna menjalankan `npm run lint:errors`/`test:unit`/`build` bersama `FE-BKC-028`, lalu
   klik-coba manual (ubah penanggung beberapa baris pada kunjungan asuransi/penjamin/tunai, cek
   baris voided terkunci, cek penolakan `422`/`409`).
2. Setelah terverifikasi, perbarui `frontend-roadmap.md` (kartu `FE-BKC-029`) dan
   `requirement-traceability.md`, tandai `✅`, tautkan laporan ini.
3. Lanjutkan `FE-BKC-030` (Edit Billing) — slot placeholder terakhir pada `edit-tagihan-view.jsx`.
