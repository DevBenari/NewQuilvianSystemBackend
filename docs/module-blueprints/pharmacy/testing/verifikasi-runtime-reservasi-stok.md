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

## 4. Uji runtime A–G — **PASS, 11/11**

Dijalankan terhadap **PostgreSQL pengembangan yang sesungguhnya**, memakai service produksi
(`PrescriptionStockReservationService`, `PharmacyDepotRoutingService`) di atas resep, butir
resep, obat, dan kunjungan nyata yang sudah ada di basis data ini.

| | Skenario | Hasil |
|---|---|---|
| **A** | Finalisasi resep → penahanan dibuat | **PASS** — 1 baris `Reserved`, batas waktu **tepat 4 jam**, `QuantityReserved` naik 9 |
| **B** | Pembayaran valid → penahanan tetap aktif | **PASS** — `ExpiresAt` menjadi `null`; penyapu pada +3 hari melepas **0** |
| **C** | Penyiapan Farmasi → tidak menahan ulang | **PASS** — `QuantityReserved` tetap 9 sebelum dan sesudah; rencana alokasi dibaca dari penahanan resep |
| **D** | Penyerahan → penahanan dikonsumsi | **PASS** — `Consumed` + `ConsumedAt`; pemanggilan kedua ditolak `PHA_RSV_UNAVAILABLE` |
| **E** | Pembatalan → penahanan dilepas | **PASS** — `Released`, sebab `PHA_RSV_PRESCRIPTION_CANCELLED`, `QuantityReserved` kembali 0 |
| **F** | Stok kurang → finalisasi ditolak | **PASS** — `PHA_RSV_INSUFFICIENT_STOCK`, *"Kurang 5"*, **nol** penahanan parsial, saldo tidak tersentuh |
| **G** | Retry finalisasi → tidak menahan dua kali | **PASS** — `WasAlreadyReserved`, tetap 1 baris, `QuantityReserved` tetap 9 |
| + | Batas waktu rawat jalan → dilepas penyapu | **PASS** — pada +4 jam 1 menit menjadi `Expired`, saldo kembali 0 |
| + | Rawat inap dan IGD | **PASS** — `ExpiresAt` `null`; penyapu pada +30 hari melepas **0** |
| + | Keadaan untuk layar `PHA-FE-001` | **PASS** — `NOT_RESERVED` → `RESERVED`, `CanProceed` benar, nama depo dan label baris terbaca |

### Mengapa lewat service, bukan HTTP

Boot aplikasi menjalankan **seluruh** seeder dan tidak memiliki sakelar untuk melewatinya.
Basis data ini tertinggal puluhan tabel modul lain — `AccAccountingEvent`,
`MstNursingDiagnosisGroup`, `CliEquipmentUsage`, dan lainnya — sehingga seeder gagal satu per
satu. Dengan `Seeders__ContinueOnFailure=true` proses tetap maju, tetapi **lebih dari tiga
puluh menit tanpa pernah mendengarkan port**.

Yang diuji di sini karena itu perilaku service terhadap PostgreSQL sungguhan, bukan lapisan
HTTP-nya. **Yang belum terbukti runtime: authorization filter dan wiring controller** pada
`GET …/prescriptions/{id}/stock-reservation`. Controller-nya tipis — satu pemanggilan
`DescribeAsync` yang sudah terbukti di atas — tetapi itu tetap belum dibuktikan dan tidak
diklaim terbukti.

### Nilai tambah dibanding uji SQLite

Tiga hal ini **tidak dapat** dibuktikan basis data uji, karena di sana indeks unik tersaring
diturunkan menjadi tidak unik dan foreign key dimatikan:

| Hal | Bukti |
|---|---|
| Indeks unik tersaring menolak penahanan ganda | Penahanan kedua atas resep/butir/batch yang sama → `duplicate key value violates unique constraint "IX_PhmPrescriptionStockReservation_Item_Batch_Active"` |
| Saringan indeks bekerja sesuai maksud | Penahanan ulang setelah yang lama `Released` → **diterima** |
| Foreign key ditegakkan | Seluruh baris uji memakai resep, butir, obat, dan depo nyata; tidak ada FK yang dimatikan |

### Depo pelayanan: kekurangan data master yang ditemukan

Satu-satunya depo yang memenuhi syarat routing di basis data ini — `DSL-RSMMC-00001` — memiliki
`ServiceUnitId` **dan** `ClinicId` kosong, sedangkan `PHA-DEC-040` /
`PHA-DEPOT-ROUTING-v1` menuntut salah satunya cocok dengan kunjungan. Tanpa perbaikan, setiap
finalisasi resep rawat jalan akan ditolak `PHA_RSV_DEPOT_UNRESOLVED`.

Uji ini membuat depo **baru** di dalam transaksi, bukan mengubah depo yang sudah ada, supaya
bukti runtime modul lain tidak tersentuh. Ke-24 kunjungan rawat jalan di basis data ini
**sudah** memiliki `ServiceUnitId`, jadi perbaikannya adalah mengisi konfigurasi routing depo —
pekerjaan data master, bukan kode.

> **Tindakan yang dibutuhkan sebelum pemakaian sungguhan:** isi `ServiceUnitId` atau `ClinicId`
> pada depo farmasi, atau tambahkan depo per unit layanan. Selama itu belum dilakukan,
> finalisasi resep akan ditolak dengan sebab yang jelas — bukan gagal diam-diam.

### Kebersihan basis data

Setiap skenario berjalan di dalam transaksi yang **di-rollback**. Diperiksa sesudahnya:

```
baris reservasi tertinggal : 0
depo uji tertinggal        : 0
batch uji tertinggal       : 0
status resep acuan         : 2  (kembali seperti semula)
```

---

## 5. Kesimpulan

`PHA-BE-003` dan `PHA-FE-001` **DONE**. Farmasi **19/19 = 100%**, runtime reservasi **PASS**,
dengan satu catatan terbuka yang bukan pekerjaan kode: konfigurasi routing depo pada data
master, dan authorization/controller `PHA-FE-001` yang belum dibuktikan lewat HTTP.
