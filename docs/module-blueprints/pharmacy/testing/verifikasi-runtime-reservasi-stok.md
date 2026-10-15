# Verifikasi Runtime — Penahanan Stok Resep (`PHA-DEC-066`, `PHA-BE-003`)

| Hal | Isi |
|---|---|
| Tanggal | 15 Oktober 2026 |
| Basis data | `localhost` / `QuilvianNewDevIkbalFr` — diverifikasi `inet_server_addr()` dan `current_database()` sebelum DDL |
| Keputusan | `PHA-DEC-066` Stock Reservation Authority V1 |
| Migration | `20261015000000_AddPrescriptionStockReservation` |

---

## 1. Snapshot EF disamakan dengan model

`ApplicationDbContextModelSnapshot.cs` sebelumnya **tidak** memuat
`PhmPrescriptionStockReservation`, sehingga siapa pun yang berikutnya menjalankan
`dotnet ef migrations add` akan menerima migration yang mengusulkan membuat tabel itu lagi.

Entity, sembilan indeks, dan lima relasinya ditambahkan. **Nol migration kedua dibuat.**

Verifikasi memakai EF sendiri, bukan pembacaan mata:

```
dotnet ef migrations has-pending-model-changes
→ No changes have been made to the model since the last migration.
```

### Dua temuan saat menyamakannya

**Nama indeks.** Konfigurasi semula tidak memberi nama eksplisit pada dua indeks unik tiga
kolom, sehingga nama bawaan EF terpotong menjadi
`IX_PhmPrescriptionStockReservation_PrescriptionId_Prescriptio~1` — tidak terbaca, dan kedua
indeks hanya dibedakan akhiran angka. `HasDatabaseName` ditambahkan supaya namanya tetap
`…_Item_Batch_Active` dan `…_Compound_Batch_Active`.

**Dua indeks kurang.** Migration semula membuat **7** indeks, sedangkan model menuntut **9**:
indeks penyerta foreign key `DrugBatchId` dan `PrescriptionItemId` belum ditulis. Tanpa
keduanya snapshot tidak akan pernah bersih, dan basis data yang dibangun dari migration akan
kehilangan dua indeks yang dipakai pembacaan per baris resep dan per batch.

### Peringatan: `ef migrations remove` merusak snapshot repo ini

Saat menyamakan, `dotnet ef migrations remove` dijalankan sekali untuk membuang migration
diagnostik. Perintah itu **mengembalikan snapshot ke keadaan yang salah**, dan `migrations add`
berikutnya menghasilkan migration yang **membuat ulang seluruh basis data** — ratusan
`CREATE TABLE` untuk tabel yang sudah ada.

Sebabnya: repo ini mengarsipkan 199 berkas Designer ke `Migrations/History/`, sehingga EF
kehilangan acuan keadaan model pada migration sebelumnya dan jatuh ke keadaan kosong.
Migration itu dibuang dan snapshot dipulihkan dari git sebelum apa pun diterapkan.

> **Jangan jalankan `dotnet ef migrations remove` di repo ini.** Hasilnya tidak dapat dipercaya,
> dan migration yang digenerate sesudahnya berbahaya bila dijalankan tanpa dibaca lebih dulu.

---

## 2. Audit SQL migration

Dibuat dengan `dotnet ef migrations script 20261014100000_FixClearanceReasonCodeReevaluatedByCashier
20261015000000_AddPrescriptionStockReservation`, yaitu **hanya** migration ini.

| Pemeriksaan | Hasil |
|---|---|
| `CREATE TABLE` | **1** — `public."PhmPrescriptionStockReservation"` |
| `CREATE INDEX` | **9** |
| `DROP` apa pun | **0** |
| `ALTER TABLE` | **0** |
| `DELETE` / `TRUNCATE` | **0** |
| Tabel modul lain yang **diubah** | **0** — lima tabel hanya dirujuk `REFERENCES`, tidak disentuh |
| Pembungkus | `START TRANSACTION` … `COMMIT` |

Kesembilan indeks:

```
IX_PhmPrescriptionStockReservation_PrescriptionId_State
IX_PhmPrescriptionStockReservation_ExpiresAt            (tersaring)
IX_PhmPrescriptionStockReservation_IdempotencyKey
IX_PhmPrescriptionStockReservation_Item_Batch_Active    (unik, tersaring)
IX_PhmPrescriptionStockReservation_Compound_Batch_Active(unik, tersaring)
IX_PhmPrescriptionStockReservation_DrugId
IX_PhmPrescriptionStockReservation_StorageLocationId
IX_PhmPrescriptionStockReservation_DrugBatchId
IX_PhmPrescriptionStockReservation_PrescriptionItemId
```

Lima foreign key, seluruhnya `ON DELETE RESTRICT`: `PhmPrescription`, `PhmPrescriptionItem`,
`PhmDrugBatch`, `MstDrug`, `MstDrugStorageLocation`.

---

## 3. Penerapan terbatas ke basis data pengembangan

**Rantai migration penuh tidak dijalankan.** Rantai itu sudah terbukti gagal pada migration
lama (`42P07`, nol kemajuan), dan basis data ini memang tertinggal jauh — puluhan tabel modul
lain belum ada.

Yang dijalankan hanya skrip satu migration di atas, lewat `psql` dengan target eksplisit.

| Langkah | Hasil |
|---|---|
| Verifikasi target sebelum DDL | `host=::1/128  db=QuilvianNewDevIkbalFr  user=postgres` |
| Tabel sebelum penerapan | **tidak ada** |
| Lima tabel acuan FK | **seluruhnya ada** |
| Penerapan | `COMMIT` tanpa galat |
| Kolom sesudahnya | **27** |
| Indeks sesudahnya | **10** (9 + primary key) |
| Foreign key sesudahnya | **5** |
| `__EFMigrationsHistory` | **1 baris** — dicatat karena seluruh `Up()` benar-benar diterapkan utuh |

Pencatatan history di sini sah dan berbeda dari patch 8, 13, dan 15 sebelumnya yang sengaja
**tidak** dicatat: ketiganya hanya potongan dari migration besar, sedangkan migration ini
diterapkan seluruhnya.

---

## 4. Uji runtime A–G

Status: **tertahan kekurangan data master.** Lihat bagian berikut.
