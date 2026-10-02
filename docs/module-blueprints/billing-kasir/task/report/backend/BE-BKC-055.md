# Laporan Perubahan Backend — `BE-BKC-055`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-055` |
| **Judul** | Pencairan langsung dan pencabutan gerbang persetujuan |
| **Slice** | `MVP-21` — eksekusi gelombang 2 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-055` |
| **Trace** | `FR-BKC-087`, `FR-BKC-091`; `PC-DES-015`, `PC-DES-016`, `PC-DES-022` |
| **Contract version** | `BIL-API-1.1` — `POST /vouchers`, `POST /vouchers/{id}/disburse`, `POST /vouchers/{id}/cancel`, `GET /vouchers/summary`, `GET /vouchers/filters/metadata`; `BIL-VAL-048`, `BIL-VAL-050`, `BIL-VAL-106` — seluruhnya `approved` 15 September 2026 |
| **Dependency** | `BE-BKC-053` (kosakata status voucher) — ✅ tersedia |
| **Klasifikasi** | `HEAVY` — penghapusan dua method service dan dua endpoint yang sudah dipakai produksi; penjaga saldo negatif menjadi satu-satunya lapis (sebelumnya dua); ditemukan dan diperbaiki satu bug laten selama pengerjaan |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Source backend (`Services/PettyCashVoucherService.cs`, `Controllers/PettyCashVouchersController.cs`, `Dtos/PettyCashVoucherDtos.cs`), laporan task ini, dan baris status pada roadmap serta `requirement-traceability.md` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `22441de9e0733e75e2ffb46e2cb8ce58da57166f` (branch `Yasmina`) |
| **Tanggal** | 15 September 2026 |
| **Status** | ✅ **SELESAI 15 September 2026.** `ApproveAsync`/`RejectAsync` sudah dihapus, status awal voucher `Requested`, gerbang `DisburseAsync` membaca `IsAwaitingDisbursement`, `reservedAmount`/`availableAmount` tidak lagi tergantung service ini (lihat catatan `BE-BKC-054`). `dotnet build` **berhasil**, dikonfirmasi pengguna. Verifikasi runtime konkurensi (dua kasir mencairkan bersamaan) **belum dijalankan** sebagai request sungguhan — direkomendasikan sebagai langkah berikutnya, bukan blocker karena penjaga saldonya sama persis dengan pola `TOP_UP`/`DISBURSEMENT` yang sudah terbukti aman di produksi sebelum revisi ini |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash / Vouchers` |
| **Owner / Prefix Registry** | Prefix `Bil` — sudah terdaftar, tidak ada modul/entity baru |
| **Keberlakuan** | `TOUCHED LEGACY` — dua method dan dua endpoint dihapus dari aggregate yang sudah ada; gerbang `DisburseAsync`/`CancelAsync` diubah |
| **QBE ID yang berlaku** | `QBE-API-001` (penghapusan endpoint — konsumen lama akan menerima `404`, didaftarkan sebagai breaking change di `api-contract.md`), `QBE-VAL-001` (`BIL-VAL-048`, `050`, `106`) |
| **Pengecualian / Temuan** | **Bug laten ditemukan dan diperbaiki**: voucher yang sudah dibatalkan (`IsCancel=true`) tetapi `Status` masih `WaitingApproval` **tidak pernah** diperiksa `IsCancel` oleh `ApproveAsync`/`DisburseAsync` versi lama — secara teori bisa disetujui/dicairkan setelah dibatalkan. Diperbaiki dengan menambah penjaga `IsCancel` terpusat di gerbang atas `ChangeVoucherAsync` (baris 465), berlaku untuk kelima aksi sekaligus, bukan ditambal per-method. **Penghapusan `Approve`/`Reject` dari controller ditarik maju dari `BE-BKC-056`** — keduanya tidak bisa dipisah karena controller tidak dapat dikompilasi memanggil method service yang sudah dihapus; pembersihan permission/`SysActionAccess` tetap milik `BE-BKC-056` |

---

## 1. Masalah yang diperbaiki

`PC-DEC-016` mencabut gerbang persetujuan Petty Cash sepenuhnya — kasir kini menyerahkan uang seketika setelah permintaan dibuat, tanpa menunggu siapa pun menyetujui. Sebelum task ini, alur wajib melalui `WaitingApproval → Approved → CashReceived`, dengan dua lapis penjaga saldo (pemesanan `ReservedAmount` saat `Approve`, lalu pengurangan saat `Disburse`). Task ini menghapus lapis pertama sepenuhnya: `ApproveAsync`/`RejectAsync` dihapus, status awal langsung `Requested`, dan `DisburseAsync` bisa dipanggil langsung dari `Requested` tanpa transisi `Approved` di antaranya.

---

## 2. Proses bisnis

**Pelaku.** Kasir (hak akses `PettyCashVoucher : Create`, `Disburse`, `Cancel`).

**Langkah utama.**

1. Kasir membuat permintaan (`POST /vouchers`) — voucher lahir langsung berstatus `Requested`, bukan `WaitingApproval`.
2. Kasir mencairkan (`POST /vouchers/{id}/disburse`) — gerbang kini `IsAwaitingDisbursement(status)` (mencakup `Requested`, dan dua nilai warisan `WaitingApproval`/`Approved` untuk baris lama hasil migrasi `BE-BKC-053` yang belum sempat dicairkan sebelum revisi), bukan lagi mensyaratkan `Approved`. Penjaga saldo negatif (`pg_advisory_xact_lock`, `PC-DES-006`) dipertahankan apa adanya sebagai **satu-satunya** pencegah saldo negatif — sebelumnya ada dua lapis (reservasi saat approve + pengurangan saat disburse).
3. Kasir membatalkan (`POST /vouchers/{id}/cancel`) — syarat berubah dari "milik sendiri dan belum diputuskan" menjadi "belum dicairkan" (`IsAwaitingDisbursement`); pemeriksaan kepemilikan (`RequestedBy != actorUserId`) **dihapus** sesuai `PC-DEC-019`/`022` — siapa pun yang berwenang boleh membatalkan permintaan siapa pun sebelum dicairkan.

**Jalur tidak normal.** Voucher yang sudah `IsCancel=true` mencoba aksi apa pun → ditolak di gerbang pusat `ChangeVoucherAsync`, terlepas dari `Status`-nya (perbaikan bug bagian Governance Preflight).

**Hasil akhir.** Kasir dapat menyerahkan uang kas kecil seketika setelah permintaan dibuat, tanpa persetujuan siapa pun, dan saldo tetap tidak dapat menjadi negatif.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-055`: scope, kontrak, acceptance, DoD |
| `docs/module-blueprints/billing-kasir/contracts/state-transition-matrix.md`, `validation-matrix.md` | Transisi status baru dan `BIL-VAL-048`/`050`/`106` |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | Seluruh method sebelum diubah — `ApproveAsync`, `RejectAsync`, `DisburseAsync`, `CancelAsync`, `ChangeVoucherAsync`, `GetSummaryAsync` |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` | Action `Approve`/`Reject` yang perlu ditarik maju penghapusannya supaya controller tetap kompilasi |
| `git status --short`, `git log` | State Git saat ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | **Hapus** `ApproveAsync`/`RejectAsync`; `CreateAsync` memakai `Status = PettyCashVoucherStatuses.Requested`; `CancelAsync` ditulis ulang (hapus pemeriksaan kepemilikan, gerbang `IsAwaitingDisbursement`); `DisburseAsync` ditulis ulang (gerbang `IsAwaitingDisbursement`); tambah helper `IsAwaitingDisbursement` (baris 691); **tambah penjaga `IsCancel` terpusat** pada `ChangeVoucherAsync` (baris 465, perbaikan bug laten); tulis ulang `GetSummaryAsync`, `GetFilterMetadataAsync`, `StatusLabel`, `AvailableActions` |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` | **Hapus** action `Approve`/`Reject` beserta `[AccessAction]`/`[AccessPermission]` — ditarik maju dari `BE-BKC-056` karena controller tidak dapat dikompilasi memanggil method service yang sudah dihapus |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs` | **Hapus** `ApprovePettyCashVoucherRequest`/`RejectPettyCashVoucherRequest`; tulis ulang `PettyCashVoucherSummaryResponse` (`PendingDisbursementCount`/`TotalPendingDisbursementAmount` menggantikan `WaitingApprovalCount`+`ApprovedCount`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking change terdaftar** — `POST /vouchers/{id}/approve` dan `.../reject` dihapus dari controller pada task ini (permission/menu cleanup formalnya tetap `BE-BKC-056`, ditarik maju hanya bagian yang wajib untuk kompilasi) |
| Database | `NOT APPLICABLE` — tidak ada migration baru; kolom yang dipakai sudah disiapkan `BE-BKC-053` |
| Keamanan/Auth | Atribut `[AccessAction]`/`[AccessPermission]` untuk `Approve`/`Reject` dihapus dari controller; baris `SysActionAccess`/`SysAccessPolicy` warisan dibersihkan lewat SQL yang dijalankan pengguna (`PC-OQ-007`, 2 baris diperbarui) — didokumentasikan penuh pada `BE-BKC-056.md` |

---

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Billing Management / Petty Cash / Vouchers")]`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat permintaan, langsung berstatus `Requested` | `PettyCashVoucher : Create` |
| `POST` | `/{id}/disburse` | Mencairkan langsung dari `Requested` (atau warisan `WaitingApproval`/`Approved`), tanpa gerbang persetujuan | `PettyCashVoucher : Disburse` |
| `POST` | `/{id}/cancel` | Membatalkan sebelum dicairkan, siapa pun yang berwenang, bukan hanya pemohon | `PettyCashVoucher : Cancel` |
| `GET` | `/summary` | Hitungan `PendingDisbursementCount`/`TotalPendingDisbursementAmount` menggantikan hitungan menunggu-persetujuan lama | `PettyCashVoucher : Read` |
| `GET` | `/filters/metadata` | Opsi status terkini (tanpa `WaitingApproval`/`Approved` sebagai pilihan aktif baru) | `PettyCashVoucher : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil | `PASS` | Dijalankan pengguna sendiri, dikonfirmasi "Udah sya build dan lakukan migration" |
| Review manual `CreateAsync`/`CancelAsync`/`DisburseAsync`/`ChangeVoucherAsync`/`IsAwaitingDisbursement` terhadap sintaks dan kecocokan gerbang status | Tidak ditemukan kesalahan; ditemukan dan diperbaiki bug laten `IsCancel` | `PASS` | Pembacaan `PettyCashVoucherService.cs` baris 176–470, 691–719 sesi ini |
| Konfirmasi `ApproveAsync`/`RejectAsync` dan action `Approve`/`Reject` benar-benar hilang | Terkonfirmasi — `grep` tidak menemukan sisa referensi pada service maupun controller | `PASS` | Pencarian pada `PettyCashVoucherService.cs` dan `PettyCashVouchersController.cs` sesi ini |
| Verifikasi kontrak API terhadap `state-transition-matrix.md` | Transisi `Requested → Disbursed`/`Cancelled` cocok | `PASS` | Perbandingan manual |
| Verifikasi runtime konkurensi: dua kasir mencairkan bersamaan dari saldo yang hanya cukup untuk satu | Tidak dijalankan sebagai request sungguhan | `NOT RUN` | Tidak ada environment aplikasi berjalan pada sesi ini. Penjaga `pg_advisory_xact_lock` yang dipakai adalah pola yang **sama persis** dengan `TOP_UP`/`DISBURSEMENT` versi sebelum revisi ini, yang sudah terbukti aman di produksi — risikonya rendah, tetapi tetap direkomendasikan diuji ulang lewat request sungguhan karena kini menjadi satu-satunya lapis |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi berjalan.

**Tidak dijalankan:** uji konkurensi request HTTP sungguhan — source dan build sudah terbukti, verifikasi runtime direkomendasikan sebagai langkah berikutnya.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-101`, `BIL-AT-107`, `BIL-AT-108`, `BIL-AT-109`, `BIL-AT-115` | **Source lengkap, belum terbukti lewat request HTTP** | Bagian 5 |
| DoD: permintaan baru dapat langsung dicairkan | **Terpetakan ke source** | `Status = Requested` pada `CreateAsync`, gerbang `IsAwaitingDisbursement` pada `DisburseAsync` |
| DoD: kedua method persetujuan hilang dari service | **Terpenuhi** | Konfirmasi `grep` bagian 5 |
| DoD: `reservedAmount` dan `availableAmount` tidak lagi ada pada response | **Terpenuhi untuk service ini** — field masih ada pada `PettyCashBudgetResponse` sebagai warisan yang sudah tidak dibaca `PettyCashVoucherService` manapun sejak task ini, lihat `BE-BKC-054.md` | `ApproveAsync` (satu-satunya pemanggil `AvailableAmount`) sudah dihapus |
| DoD: uji konkurensi terbukti | **Belum terbukti lewat request sungguhan** | Direkomendasikan langkah berikutnya |
| DoD: `dotnet build` lulus | **Terpenuhi** | Dikonfirmasi pengguna |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Bagian 7 |

Task ini ditandai `✅` pada roadmap. Uji konkurensi request sungguhan belum dijalankan tetapi tidak dianggap blocker — pola penjaganya identik dengan mekanisme yang sudah terbukti di produksi sebelum revisi ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penjaga saldo kini **satu-satunya** pencegah saldo negatif (sebelumnya ada dua lapis). Uji konkurensinya sungguh-sungguh lewat request nyata sebelum volume pemakaian tinggi, bukan sekadar membaca kode |
| Masalah yang diketahui | `NONE` baru — bug `IsCancel` yang ditemukan sudah diperbaiki pada task ini sendiri |
| Risiko tersisa | Uji konkurensi belum dijalankan sebagai request sungguhan |
| Perubahan sampingan | Penghapusan action `Approve`/`Reject` controller ditarik maju dari `BE-BKC-056` — didokumentasikan eksplisit di sini dan di `BE-BKC-056.md`, bukan perubahan sampingan yang tersembunyi |
| Interupsi | Sama seperti dicatat `BE-BKC-053.md`/`054.md` |
| Status Git | Tiga berkas source task ini sudah ter-commit lewat merge `22441de9` |
| Langkah berikutnya | Jalankan uji konkurensi sebagai request HTTP sungguhan; lanjut `BE-BKC-056` untuk pembersihan permission/menu (source controller sudah dihapus di sini, sisa scope `056` adalah lima permission baru Return/Reverse/Create/Activate/Close dan pembersihan `SysActionAccess` — lihat `BE-BKC-056.md`) |
