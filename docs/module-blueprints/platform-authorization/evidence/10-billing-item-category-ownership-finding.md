# Temuan Terpisah — Kepemilikan `BillingItemCategory` Sesudah Konsolidasi ke `TariffCategory`

> **Status:** temuan **non-blocking**, **di luar `BE-SEC-003`**.
> **Tidak boleh diselesaikan di dalam `BE-SEC-003`.** Dicatat terpisah supaya tidak hilang.

| Field | Nilai |
|---|---|
| Jenis dokumen | **Hand-off finding** — bukan laporan task, bukan bagian scope `BE-SEC-003` |
| Ditemukan oleh | Global registry drift audit — `evidence/07` bagian D.1 |
| Dikonfirmasi ulang | `evidence/09` bagian F, sesudah merge Integration |
| Pemilik | **Tim Billing / master data tarif**, bersama pemilik layar Akses Role |
| Menahan `BE-SEC-003B`? | **Tidak** |
| Tanggal | 11 September 2026 |

---

## A. Temuannya

Empat kunci registry berikut hidup di database, memegang hak efektif, tetapi **tidak lagi
dideklarasikan source**:

| Kunci runtime | Policy efektif | Pengguna aktif | Pemegang |
|---|---:|---:|---|
| `BillingItemCategory.Create` | 3 | 14 | Finance × Manajer Finance; Human Resource × Manajer HR; Medis × Dokter Umum |
| `BillingItemCategory.Delete` | 3 | 14 | sama |
| `BillingItemCategory.Read` | 3 | 14 | sama |
| `BillingItemCategory.Update` | 3 | 14 | sama |

Total **12 baris `SysAccessPolicy`** pada **3 pasangan Departemen × Posisi**.

## B. Sebabnya — terbaca langsung dari source

`Areas/HealthServices/MasterData/Models/MstTariffCategory.cs:34`:

> *"`MstBillingItemCategory` saat kategori billing digabung ke kategori tarif."*

Kategori billing **digabungkan ke `TariffCategory`**. Controller-nya hilang dari source, dan
tabelnya dihapus migration `20260902072756_DropTableMstBillingCategory`.

## C. Kenapa ini BUKAN kehilangan hak akibat rekonsiliasi

Tidak ada satu pun endpoint di HEAD yang mendeklarasikan `BillingItemCategory.*`.
`HasAccessAsync` hanya dipanggil oleh filter pada endpoint; tanpa endpoint, izin itu **tidak pernah
ditanyakan**. Kedua belas baris policy tersebut karena itu **sudah mati secara praktis hari ini**,
terlepas dari flag registry-nya.

Ketika `AccessMenuSeeder` menutup keempat baris registry itu, yang berubah hanyalah kematian itu
menjadi terlihat. **Nol kemampuan terjangkau yang hilang.** Itulah sebabnya temuan ini tidak
menahan jendela `BE-SEC-003`.

## D. Yang sebenarnya perlu diputuskan — dan ini bukan akibat seeder

Penerus kemampuannya adalah `TariffCategory`, yang di database **hanya dipegang Medis × Dokter Umum
(1 policy, 2 pengguna)**.

| Pasangan | Dulu `BillingItemCategory` | Sekarang `TariffCategory` |
|---|:---:|:---:|
| Medis × Dokter Umum | ✅ | ✅ |
| **Finance × Manajer Finance** | ✅ | ❌ |
| **Human Resource × Manajer HR** | ✅ | ❌ |

Konsolidasi billing → tarif meninggalkan **celah kepemilikan**: dua jabatan kehilangan kemampuan
mengelola kategori, dan celah itu **sudah terjadi saat source digabungkan**, bukan saat seeder
berjalan.

## E. Pertanyaan untuk pemilik

> Apakah **Finance × Manajer Finance** dan **Human Resource × Manajer HR** seharusnya memegang
> `TariffCategory`?

Jawabannya keputusan instalasi keuangan dan pemilik master data tarif — **tidak boleh disimpulkan
dari kode**, dan tidak boleh diputuskan oleh `BE-SEC-003`.

## F. Kenapa mendesak dicatat, walau tidak mendesak diperbaiki

Begitu `AccessMenuSeeder` berjalan pada HEAD, keempat baris registry itu ditutup dan **hilang dari
layar Akses Role**. Sesudah itu, bukti siapa yang dulu memegangnya tidak lagi dapat dibaca admin
dari layar mana pun — hanya tersisa di dokumen ini dan di baris `SysAccessPolicy` yang menggantung.

Dokumen ini ditulis **sebelum** jendela penerapan justru karena alasan itu.

## G. Yang TIDAK boleh dilakukan

- Jangan menyelesaikannya di dalam `BE-SEC-003` — scope-nya pilot Dokter Rawat Jalan.
- Jangan menambahkan `TariffCategory` ke pasangan mana pun tanpa keputusan pemilik. Itu
  **privilege broadening**, dan `D-ARCH-2` melarangnya tanpa keputusan sadar.
- Jangan menghidupkan kembali `BillingItemCategory` di source hanya agar registry-nya tidak
  tertutup. Kemampuannya memang sudah tidak ada.
