# Perencanaan & Permintaan — Matriks Perpindahan Status

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-STATE` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-034`) |
| Pemilik | Pengguna sesi (Product Owner Pengadaan) · `approved_by: Pengguna sesi` · `approved_at: 2026-11-12` |
| Masukan | `PBJ-DEC-007`, `014`, `016`, `019`, `027`..`030`; `02-backend-architecture.md` revisi 2 |
| Dampak kompatibilitas | Kontrak baru; tidak mengubah status modul lain |

Status disimpan sebagai teks huruf besar. Label tampilan berbahasa Indonesia. Butir **[DIKONFIRMASI `PBJ-DEC-033`]** semula usulan dari
gap `PROPOSED` (`G-04`, `G-05`) dan wajib dikonfirmasi saat approval.

---

## 1. Permintaan Pengadaan (PR)

| Status | Label tampilan | Arti |
| --- | --- | --- |
| `DRAFT` | Draf | Disusun pengusul, belum bernomor |
| `SUBMITTED` | Menunggu Verifikasi | Sudah diajukan dan bernomor; menunggu Tim Pengadaan |
| `RETURNED` | Dikembalikan | Dikembalikan ke pengusul untuk diperbaiki |
| `PENDING_APPROVAL` | Menunggu Persetujuan | Verifikasi selesai; instance approval `Wfl` berjalan |
| `APPROVED` | Disetujui | Siap dijadikan paket oleh `pemilihan-penyedia` |
| `FULFILLED_FROM_STOCK` | Dipenuhi dari Stok | Seluruh kebutuhan dipenuhi stok; selesai tanpa pembelian |
| `REJECTED` | Ditolak | Final |
| `CANCELLED` | Dibatalkan | Final |
| `CONVERTED_TO_PACKAGE` | Masuk Paket | Final bagi sub-modul ini; ditulis `pemilihan-penyedia` lewat service pemilik |

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat | `DRAFT` | Pengusul (`ProcurementRequest : Create`) dengan penempatan aktif di unit PR | Unit ada dalam cakupan pengguna | 404 bila unit di luar cakupan |
| `DRAFT`, `RETURNED` | Ubah | sama | Pembuat PR | `ExpectedRowVersion` cocok | 409 `PBJ-VAL-090` |
| `DRAFT`, `RETURNED` | Ajukan | `SUBMITTED` | Pembuat PR (`ProcurementRequest : Submit`) | Lolos validasi pengajuan (`PBJ-VAL-001`..`012`); nomor dialokasikan bila belum ada; sisa rencana dibagi dalam/luar rencana | 422 sesuai aturan |
| `SUBMITTED` | Catat cek stok | sama | Tim Pengadaan (`ProcurementRequestVerification : Verify`) | Per baris; jumlah dipenuhi stok ≤ jumlah diminta | 422 `PBJ-VAL-020` |
| `SUBMITTED` | Selesaikan verifikasi, semua baris dipenuhi stok | `FULFILLED_FROM_STOCK` | Tim Pengadaan | Semua baris sudah dicek; kebutuhan beli = 0 | 422 `PBJ-VAL-021` bila ada baris belum dicek |
| `SUBMITTED` | Selesaikan verifikasi, ada kebutuhan beli | `PENDING_APPROVAL` | Tim Pengadaan | Semua baris dicek; instance `Wfl` berhasil dibuat dan diajukan dalam transaksi yang sama | 422 `PBJ-VAL-021`; 409 `PBJ-VAL-091` bila konfigurasi approval tidak ditemukan |
| `SUBMITTED` | Kembalikan | `RETURNED` | Tim Pengadaan | Alasan wajib | 422 `PBJ-VAL-030` |
| `SUBMITTED` | Tolak **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `REJECTED` | Tim Pengadaan | Alasan wajib (mis. duplikat) | 422 `PBJ-VAL-030` |
| `PENDING_APPROVAL` | Disetujui di `Wfl` (langkah terakhir) | `APPROVED` | Penyetuju sesuai matriks, **bukan** pengusul | Sinkron dari `Wfl` berstatus `Approved` | Penyetuju = pengusul ditolak `Wfl` |
| `PENDING_APPROVAL` | Ditolak di `Wfl` | `REJECTED` | Penyetuju | `Wfl` `Rejected`; alasan wajib di `Wfl` | — |
| `PENDING_APPROVAL` | Dikembalikan / diminta revisi di `Wfl` | `RETURNED` | Penyetuju | `Wfl` `Returned` atau `RevisionRequested` | — |
| `DRAFT`, `SUBMITTED`, `RETURNED` | Batalkan **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `CANCELLED` | Pembuat PR (`ProcurementRequest : Cancel`) | Alasan wajib bila sudah bernomor | 422 `PBJ-VAL-030` |
| `PENDING_APPROVAL` | Tarik **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `CANCELLED` | Pembuat PR | Instance `Wfl` ditarik (`Withdrawn`) dalam transaksi yang sama | 409 bila `Wfl` sudah final |
| `APPROVED` | Batalkan **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `CANCELLED` | Tim Pengadaan (`ProcurementRequestVerification : CancelApproved`) | Alasan wajib; belum masuk paket | 409 `PBJ-VAL-092` bila sudah masuk paket |
| `APPROVED` | Masukkan ke paket | `CONVERTED_TO_PACKAGE` | Service `pemilihan-penyedia` lewat `ProcurementPurchaseRequestService.MarkConvertedAsync` | Sub-modul itu sudah ada | — |

**Transisi yang tidak sah** (ditolak 409 `PBJ-VAL-093` "Status pengajuan tidak memungkinkan tindakan ini"):
- `REJECTED`, `CANCELLED`, `FULFILLED_FROM_STOCK`, atau `CONVERTED_TO_PACKAGE` → status apa pun. Dokumen final tidak dihidupkan lagi; pengusul membuat PR baru.
- `DRAFT` → `PENDING_APPROVAL` atau `APPROVED` tanpa melalui `SUBMITTED`.
- `RETURNED` → `PENDING_APPROVAL` tanpa diajukan ulang dan diverifikasi ulang.
- Perubahan isi baris PR saat `SUBMITTED`, `PENDING_APPROVAL`, atau `APPROVED`. Yang boleh hanya kolom cek stok oleh Tim Pengadaan saat `SUBMITTED`.
- Status `APPROVED` yang ditulis langsung oleh endpoint Pengadaan. Status ini hanya lahir dari sinkron `Wfl`.

**Efek samping status pada sisa rencana (`PBJ-DEC-029`):**

| Status | Memakan sisa rencana |
| --- | --- |
| `SUBMITTED`, `PENDING_APPROVAL`, `APPROVED`, `FULFILLED_FROM_STOCK`, `CONVERTED_TO_PACKAGE` | Ya |
| `DRAFT`, `RETURNED`, `REJECTED`, `CANCELLED` | Tidak; sisa kembali |

**Contoh:** rencana IGD 10 monitor. PR-1 (6 unit) `APPROVED`, sisa 4. PR-1 dibatalkan → `CANCELLED`, sisa kembali
10. PR yang `RETURNED` lalu diajukan ulang dihitung ulang pembagian dalam/luar rencananya saat diajukan ulang.

## 2. Rencana Kebutuhan Tahunan **[DIKONFIRMASI `PBJ-DEC-033`, `G-05`]**

| Status | Label |
| --- | --- |
| `DRAFT` | Draf |
| `PENDING_APPROVAL` | Menunggu Persetujuan |
| `RETURNED` | Dikembalikan |
| `APPROVED` | Disetujui (berlaku bila `IsCurrentVersion = true`) |
| `SUPERSEDED` | Digantikan revisi |
| `REJECTED` | Ditolak |
| `CANCELLED` | Dibatalkan |

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat versi 1 | `DRAFT` | Penyusun unit (`ProcurementNeedPlan : Create`) | Belum ada rencana aktif unit-tahun yang sama | 409 `PBJ-VAL-040` |
| `DRAFT`, `RETURNED` | Ubah | sama | Penyusun | `ExpectedRowVersion` | 409 `PBJ-VAL-090` |
| `DRAFT`, `RETURNED` | Ajukan | `PENDING_APPROVAL` | Penyusun (`ProcurementNeedPlan : Submit`) | Minimal 1 baris valid; nomor `RKB` dialokasikan | 422 |
| `PENDING_APPROVAL` | Disetujui `Wfl` | `APPROVED` | Penyetuju | Versi sebelumnya (bila ada) menjadi `SUPERSEDED` dan `IsCurrentVersion` berpindah, dalam satu transaksi | — |
| `PENDING_APPROVAL` | Ditolak / dikembalikan `Wfl` | `REJECTED` / `RETURNED` | Penyetuju | — | — |
| `APPROVED` (berlaku) | Buat revisi | versi baru `DRAFT` | Penyusun atau Tim Pengadaan (`ProcurementNeedPlan : Revise`) | Belum ada revisi lain yang belum final; alasan revisi wajib | 409 `PBJ-VAL-041` |
| `DRAFT` | Batalkan | `CANCELLED` | Penyusun | — | — |

**Tidak sah:**
- Mengubah baris versi `APPROVED` atau `SUPERSEDED`. Perubahan harus lewat revisi.
- Menurunkan kuantitas baris dalam revisi di bawah kuantitas yang sudah dipakai PR aktif (`PBJ-VAL-042`).

**Contoh:** rencana IGD 2027 v1 disetujui (10 monitor), dan PR aktif sudah memakai 6. Revisi v2 menurunkan ke
5 → ditolak, karena sudah terpakai 6.

## 3. Rencana Paket **[DIKONFIRMASI `PBJ-DEC-033`, `G-05`]**

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| — | Buat | `DRAFT` | Tim Pengadaan (`ProcurementPackagePlan : Create`) | Baris dari rencana `APPROVED` berlaku |
| `DRAFT`, `RETURNED` | Ajukan | `PENDING_APPROVAL` | Tim Pengadaan (`ProcurementPackagePlan : Submit`) | Alokasi valid (`PBJ-VAL-050`) |
| `PENDING_APPROVAL` | Disetujui / ditolak / dikembalikan `Wfl` | `APPROVED` / `REJECTED` / `RETURNED` | Penyetuju | — |
| `DRAFT`, `RETURNED`, `APPROVED` | Batalkan | `CANCELLED` | Tim Pengadaan (`ProcurementPackagePlan : Cancel`) | Alasan wajib bila `APPROVED`; belum dipakai paket eksekusi |

**Tidak sah:** mengubah rencana paket `APPROVED`. Dokumen final tidak dihidupkan lagi.

## 4. Pagu **[DIKONFIRMASI `PBJ-DEC-033`, `G-05`]**

Sama dengan rencana kebutuhan: `DRAFT` → `PENDING_APPROVAL` → `APPROVED` / `RETURNED` / `REJECTED`. Revisi
membuat versi baru, dan versi lama menjadi `SUPERSEDED` saat revisi disetujui. Revisi tidak boleh menurunkan
pagu di bawah komitmen aktif (`PBJ-VAL-062`).

**Contoh:** pagu Radiologi Rp 500 juta dengan komitmen Rp 120 juta. Revisi ke Rp 100 juta → ditolak, karena
lebih kecil dari komitmen.

## 5. Pemetaan status `Wfl` → status dokumen

| Status instance `Wfl` | PR | Rencana / Rencana Paket / Pagu |
| --- | --- | --- |
| `Submitted`, `InProgress` | `PENDING_APPROVAL` | `PENDING_APPROVAL` |
| `Approved`, `Completed` | `APPROVED` | `APPROVED` |
| `Rejected` | `REJECTED` | `REJECTED` |
| `Returned`, `RevisionRequested` | `RETURNED` | `RETURNED` |
| `Withdrawn`, `Cancelled` | `CANCELLED` | `RETURNED` (penyusun dapat mengajukan lagi) |

Sinkron bersifat idempoten: penangan menurunkan status dari status `Wfl` saat itu. Pemanggilan kedua tidak
mengubah apa pun dan tidak menambah baris riwayat ganda.
