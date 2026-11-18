# Laporan Perubahan Frontend — `FE-HRD-R34`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R34` |
| Judul | Perjalanan Dinas bagian 2: uang muka, pertanggungjawaban, overdue, klaim non-perjalanan |
| Slice | `S-D5` (Perjalanan Dinas & Biaya), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R34`; task asal `FE-HRD-298`–`304` |
| Trace | Batas uang muka 80%, ambang tanpa nota 150.000/10%, alasan minimal 15 karakter |
| Contract version | Source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Migrasi base component dan penyambungan API nyata |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 4 layar, 5 modal, 1 komponen baru, pencabutan slice mock |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — uang muka, pertanggungjawaban, tanpa nota, setor balik/kurang bayar, radar overdue, dan klaim non-perjalanan nyata; unggah struk tertahan backend |

---

## 1. Keadaan yang ditemukan di awal

- Layar uang muka, pertanggungjawaban, overdue, dan klaim membaca `businessTravelSlice` berisi data contoh.
- Modal unggah struk, setor balik, dan kurang bayar memanggil stub yang selalu "tidak tersedia".
- Kartu roadmap menyebut radar overdue menunggu `FE-HRD-R53`, padahal backend sudah punya `GET travel/overdue-reports`.

## 2. Proses bisnis dari sisi pengguna

1. Pegawai mengajukan **uang muka** untuk SPPD (maksimal 80% estimasi). Atasan menyetujui, Keuangan menyetujui, lalu kasir mencairkan.
2. Sepulang dinas, pegawai mengirim **pertanggungjawaban** dengan rincian biaya. Biaya tanpa nota diisi lewat **deklarasi tanpa nota** (maksimal 150.000 per item atau 10% total, alasan minimal 15 karakter) yang disetujui atasan.
3. Verifikator memeriksa (`Submitted → Verified`), lalu menutup: selisih lebih wajib nomor bukti setor balik, selisih kurang wajib nomor bukti pembayaran.
4. Admin memantau **radar overdue** pertanggungjawaban yang lewat batas.
5. Pegawai mengajukan **klaim biaya non-perjalanan** dengan kategori biaya; klaim disetujui, diverifikasi, lalu dibayar.
6. Jalur tidak normal: daftar kosong, galat server, dan tanpa hak akses ditampilkan dengan pesan; tombol aksi dibatasi per status karena server tidak memeriksa status saat verifikasi/penutupan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Controller uang muka, pertanggungjawaban, kebijakan tarif, klaim langsung, SPPD, dan `ExpenseCategoryController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/travel/travel-constants.js` | Permission, rute, status uang muka/settlement/tanpa nota/klaim, batas, metode bayar, resource kategori biaya |
| `src/lib/services/hr/business-travel-service.js` | Rincian/verify/close settlement, tanpa nota, klaim langsung; stub "tidak tersedia" dihapus |
| `components/TravelExpenseItemsEditor.jsx` | BARU: editor rincian biaya (mode nota / tanpa nota) |
| `modal/SettlementSubmitModal.jsx`, `modal/SettlementCloseModal.jsx`, `modal/ExpenseClaimFormModal.jsx` | BARU |
| `unified-travel-advances-view`, `unified-travel-settlements-view`, `unified-travel-overdue-view`, `unified-travel-expenses-view`, `AdvanceDisbursementModal`, `MissingReceiptDeclarationModal` | Ditulis ulang |
| `ExpenseReceiptUploadModal`, `RefundSettlementModal`, `UnderpaidPaymentModal`, `businessTravelSlice.js`, `business-travel.module.css`, `travel-display-utils.js` | Dihapus |
| `menu-items.jsx` | `requiredPermission` empat menu |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; slice mock dicabut dari `store.jsx`. Komponen `TravelExpenseItemsEditor` adalah `COMPOSE` dari `BaseFormControl` + `DataTable` + `BaseButton`.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `DataTable` | `REUSE` | — |
| Editor rincian biaya | `TravelExpenseItemsEditor` | `COMPOSE` | Baris biaya dapat ditambah/hapus |
| Dialog | `ConfirmModal` + `BaseFormControl` | `COMPOSE` | — |
| Status overdue | `HrStatusBadge` dengan peta gabungan | `WRAP` | Memperbaiki temuan `PROP-TIDAK-SAH` |

`UI GATE: PASS — 4 elemen REUSE, 2 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <10 berkas JSX R34>
OK — 10 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas R34>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel |
| Kosong | "Belum ada uang muka." / "Belum ada pertanggungjawaban." / "Tidak ada yang overdue." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | Peringatan hak; tombol aksi disembunyikan |

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Business Travel Advances

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/travel/advances` | Daftar | `BusinessTravelAdvance : Read` |
| `POST` | `/v1/hrd/travel/advances` | Ajukan | `BusinessTravelAdvance : Request` |
| `POST` | `/v1/hrd/travel/advances/{id}/approve` | Setuju atasan | `BusinessTravelAdvance : Approve` |
| `POST` | `/v1/hrd/travel/advances/{id}/finance-approve` | Setuju Keuangan | `BusinessTravelAdvance : ApproveFinance` |
| `POST` | `/v1/hrd/travel/advances/{id}/disburse` | Cairkan | `BusinessTravelAdvance : Disburse` |

#### Corporate - HRD Business Travel Settlements

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/travel/settlements` | Daftar | `BusinessTravelSettlement : Read` |
| `GET` | `/v1/hrd/travel/settlements/{id}` | Rincian | `BusinessTravelSettlement : Read` |
| `POST` | `/v1/hrd/travel/settlements` | Kirim | `BusinessTravelSettlement : Submit` |
| `POST` | `/v1/hrd/travel/settlements/{id}/verify` | Verifikasi | `BusinessTravelSettlement : Verify` |
| `POST` | `/v1/hrd/travel/settlements/{id}/close` | Tutup (setor balik/kurang bayar) | `BusinessTravelSettlement : Close` |
| `GET` | `/v1/hrd/travel/settlements/{id}/missing-receipts` | Daftar tanpa nota | `BusinessTravelSettlement : Read` |
| `POST` | `/v1/hrd/travel/settlements/{id}/missing-receipts` | Deklarasi tanpa nota | `BusinessTravelSettlement : DeclareMissingReceipt` |
| `POST` | `/v1/hrd/travel/settlements/missing-receipts/{itemId}/approve` | Setujui tanpa nota | `BusinessTravelSettlement : ApproveMissingReceipt` |

#### Corporate - HRD Travel Rates & Policies

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/travel/overdue-reports` | Radar overdue | `TravelRatePolicy : MonitorOverdue` |

#### Corporate - HRD Direct Non-Trip Expenses

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/expenses/claims` | Daftar klaim | `DirectExpenseClaim : Read` |
| `GET` | `/v1/hrd/expenses/claims/{id}` | Rincian | `DirectExpenseClaim : Read` |
| `POST` | `/v1/hrd/expenses/claims` | Ajukan | `DirectExpenseClaim : Create` |
| `POST` | `/v1/hrd/expenses/claims/{id}/approve` | Setujui | `DirectExpenseClaim : Approve` |
| `POST` | `/v1/hrd/expenses/claims/{id}/verify` | Verifikasi | `DirectExpenseClaim : Verify` |
| `POST` | `/v1/hrd/expenses/claims/{id}/pay` | Bayar | `DirectExpenseClaim : Pay` |

#### Corporate - HRD Business Travel Requests

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/travel/requests` | Pilihan SPPD | `BusinessTravelRequest : Read` |

Pendukung: `GET /v1/corporate/human-resource/master-data/expense-categories/options` (pilihan kategori biaya).

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 14 berkas R34 | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 10 JSX | 0 temuan (setelah 1 perbaikan `PROP-TIDAK-SAH` di overdue) | `PASS` | 3.4 |
| `npm run build` BUILD-1 / BUILD-2 | 821/821 dan 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| `FE-HRD-298`–`303` alur nyata | Terpenuhi | Bagian 2 dan 5 |
| Radar overdue | Terpenuhi | Endpoint sudah ada (bukan `R53`) |
| `FE-HRD-304` unggah struk + pratinjau | Belum terpenuhi | Tidak ada endpoint unggah/unduh berkas perjalanan (`ReceiptFilePath` hanya teks) |
| Paginasi daftar | Belum terpenuhi | Endpoint daftar mengembalikan array tanpa total; dimuat maksimal 100 baris |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Backend `verify`/`close` settlement tidak memeriksa status; frontend menggerbangi tombol per status |
| Masalah yang diketahui | `PaymentOrRefundRef` tidak diwajibkan server; frontend mewajibkan untuk selisih lebih/kurang. Kategori biaya perjalanan tidak punya endpoint opsi. Rekening uang muka (`BankAccountId`) tidak dibuka |
| Dependency backend | Unggah berkas perjalanan, total halaman daftar, pemeriksaan status, opsi rekening |
| Perubahan sampingan | Slice mock, CSS, util data contoh, dan tiga modal stub dihapus |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Endpoint unggah berkas perjalanan; uji browser |
