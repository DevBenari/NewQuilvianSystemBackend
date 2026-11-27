# Manajemen Vendor — Matriks Perpindahan Status

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-STATE` · `last_changed_in: 1.1` (`PBJ-DEC-064`, 2026-11-19) · status `approved` (`PBJ-DEC-062`) |
| Masukan | `PBJ-DEC-021`, `045`..`049`, `054`, `059`..`061`; `PBJ-ASM-003` |

Setiap perpindahan menulis satu baris riwayat (`PrcDocumentHistory`) pada dokumennya, dan perubahan yang
memengaruhi vendor juga ditulis ke riwayat vendor (`PBJ_VENDOR`).

## 1. Status vendor (dihitung, tidak disimpan)

| Status tampil | Kondisi | Bisa dipilih di RFQ/penetapan baru |
| --- | --- | --- |
| Nonaktif di Master Data | `MstSupplier.IsActive = false` | Tidak |
| Diblokir | `MstSupplier.IsBlacklisted = true` | Tidak |
| Dibatasi | Profil `IsRestricted` dan tanggal berakhir kosong atau belum lewat | Ya, dengan alasan pemilih (`PBJ-DEC-047`) |
| Aktif | Selain di atas | Ya, bila dokumen wajib kategori terpenuhi |

| Dari | Ke | Pemicu | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| Aktif / Nonaktif di Master Data | Dibatasi | Batasi | Pemegang `ProcurementVendor : Restrict` | Alasan wajib; tanggal berakhir opsional ≥ hari ini. Vendor nonaktif tetap tampil Nonaktif (1.1, `VD-006 D-4`) |
| Dibatasi | Aktif | Cabut | Pemegang `ProcurementVendor : Restrict` | Alasan pencabutan minimal 10 karakter; alasan, tanggal berakhir, waktu, dan pelaku pembatasan dikosongkan di profil, jejaknya tetap di riwayat (1.1, `VD-006 D-3`) |
| Dibatasi | Aktif | Tanggal berakhir lewat | Sistem (dihitung saat dibaca) | — |
| Aktif / Dibatasi | Diblokir | Usulan `BLOCK` disetujui | Penyetuju `Wfl` | Lihat bagian 3. **(1.1)** Supplier sekaligus bukan lagi supplier pilihan (`VD-007 D-4`) |
| Diblokir | Aktif atau Dibatasi (bila profil masih dibatasi) | Usulan `UNBLOCK` disetujui | Penyetuju `Wfl` | Lihat bagian 3. **(1.1)** Status supplier pilihan **tidak** dikembalikan (`VD-007 D-4`) |

**Tidak sah:** Batasi vendor yang Diblokir (409 `PBJ-VAL-093`); mengubah blokir dari Master Data (`PBJ-DEC-060`).
**(1.1, `VD-006 D-1`, aturan bisnis `PBJ-DEC-064`)** Batasi vendor yang **sedang** Dibatasi juga tidak sah (409
`PBJ-VAL-093`): untuk mengubah alasan atau tanggal berakhir, cabut dulu lalu batasi ulang, supaya kedua langkah tercatat.
Cabut vendor yang tidak sedang Dibatasi → 409 `PBJ-VAL-093`.

## 2. Dokumen legalitas

| Dari | Ke | Aksi | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| — | `PENDING` | Catat dokumen | `ProcurementVendorDocument : Create` | Jenis aktif; tanggal berlaku bila jenis mewajibkan |
| `PENDING` | `VERIFIED` | Verifikasi | `ProcurementVendorDocument : Verify` | Berkas ada; pemeriksa ≠ pengunggah (`PBJ-DEC-046`) |
| `PENDING` | `REJECTED` | Tolak | `ProcurementVendorDocument : Verify` | Alasan wajib; pemeriksa ≠ pengunggah |
| `PENDING` | `CANCELLED` | Batalkan | Pengunggah (pemegang `Create`; orang lain → 403, 1.1 `VD-004 D-3`) | Alasan wajib |

`VERIFIED`, `REJECTED`, dan `CANCELLED` adalah status akhir. Dokumen pengganti = dokumen baru. Mencatat dokumen untuk
vendor yang belum punya profil Pengadaan membuat profil sekaligus (1.1, `VD-004 D-4`).

**Keadaan berlaku (dihitung)** untuk dokumen `VERIFIED`: Berlaku; Segera kedaluwarsa bila sisa ≤ 30 hari
**[NILAI AWAL `PBJ-ASM-002`]**; Kedaluwarsa bila `ExpiryDate` < hari ini. Jenis tanpa tanggal berlaku selalu Berlaku.
**(1.1, `VD-004 D-1`)** Digantikan (`REPLACED`) bila sudah ada dokumen terverifikasi lebih baru untuk vendor dan jenis
yang sama; hanya dokumen terverifikasi terakhir per jenis yang menentukan kelayakan dan keadaan dokumen vendor.
**Contoh:** Izin PBF 2026 terverifikasi, lalu Izin PBF 2027 terverifikasi → izin 2026 tampil Digantikan.

## 3. Usulan blokir / buka blokir

| Dari | Ke | Aksi | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| — | `DRAFT` | Buat usulan | `ProcurementVendorStatusRequest : Create` | Tidak ada usulan terbuka lain untuk vendor itu; `BLOCK` hanya untuk vendor belum diblokir, `UNBLOCK` hanya untuk vendor diblokir |
| `DRAFT` / `RETURNED` | `PENDING_APPROVAL` | Ajukan | Pengusul (orang lain → 403, 1.1 `VD-007 D-2`) | Alasan wajib; definisi `PBJ-VND-*` aktif (`PBJ-VAL-091`). Nomor `UBV` dialokasikan sebelum gerbang approval; bila ditolak `PBJ-VAL-091`, nomor itu hangus (1.1, `VD-007 D-3`) |
| `PENDING_APPROVAL` | `APPROVED` | Disetujui `Wfl` | Penyetuju (≠ pengusul) | Menulis `MstSupplier` dalam transaksi yang sama |
| `PENDING_APPROVAL` | `REJECTED` | Ditolak `Wfl` | Penyetuju | — |
| `PENDING_APPROVAL` | `RETURNED` | Dikembalikan `Wfl` | Penyetuju | — |
| `DRAFT` / `RETURNED` | `CANCELLED` | Batalkan | `ProcurementVendorStatusRequest : Cancel` | Alasan |
| `PENDING_APPROVAL` | `CANCELLED` | Tarik | Pengusul, `Cancel` | Instance `Wfl` → `Withdrawn` |
| `DRAFT` / `RETURNED` | `DRAFT` / `RETURNED` | Ubah alasan/bukti | Pengusul (orang lain → 403, 1.1 `VD-007 D-2`) | `expectedRowVersion` |
| `PENDING_APPROVAL` | (sesuai keputusan `Wfl`) | Sinkron ulang | Pemegang `Create` | Dipakai bila penerapan keputusan sempat gagal (1.1, `VD-007 D-1`) |

**Pemetaan status `Wfl`:** `Approved`/`FinalApproved`/`Completed` → `APPROVED`; `Rejected` → `REJECTED`;
`Returned`/`RevisionRequested` → `RETURNED`; `Withdrawn`/`Cancelled` → `CANCELLED`.

## 4. Penilaian berkala

| Dari | Ke | Aksi | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| — | `DRAFT` | Buat | `ProcurementVendorEvaluation : Create` | Kategori milik vendor; total bobot kriteria aktif = 100; belum ada penilaian `DRAFT`/`FINAL` untuk vendor × kategori × periode yang sama, **termasuk revisi** (`PBJ-DEC-059`; 1.1 `VD-008 D-1`). Nomor periode sesuai jenisnya: `SEMESTER` 1–2, `QUARTER` 1–4, `YEAR` 1 (1.1, `VD-008 D-8`) |
| `DRAFT` | `DRAFT` | Simpan nilai | `ProcurementVendorEvaluation : Update` (siapa pun, bukan hanya pembuat; 1.1 `VD-008 D-4`, aturan bisnis `PBJ-DEC-064`) | Nilai akhir dihitung ulang bila semua nilai terisi (1.1, `VD-008 D-5`) |
| `DRAFT` | `FINAL` | Finalkan | `ProcurementVendorEvaluation : Finalize` | Semua nilai 1–5 terisi; rekomendasi terisi; pemfinal ≠ penilai (`PBJ-DEC-049`) |
| `DRAFT` | `CANCELLED` | Batalkan | `ProcurementVendorEvaluation : Update` | Alasan |
| `FINAL` | — (revisi `DRAFT` lahir) | Buat revisi | `ProcurementVendorEvaluation : Revise` | Alasan revisi; belum ada revisi `DRAFT` lain. **(1.1, `VD-008 D-3`)** Revisi menyalin nilai, rekomendasi, catatan, dan snapshot kriteria penilaian asal; pembuat revisi menjadi penilainya |
| `FINAL` (lama) | `SUPERSEDED` | Revisi difinalkan | Sistem | Terjadi dalam transaksi finalisasi revisi |

Rekomendasi pada penilaian `FINAL` tidak mengubah status vendor (`PBJ-DEC-048`).
