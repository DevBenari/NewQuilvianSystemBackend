# Gap Billing yang ditemukan dari pengujian Farmasi

Dicatat 1 Oktober 2026 saat verifikasi runtime `BE-BKC-067`. **Keduanya milik tim Billing/Kasir**
dan sengaja tidak dikerjakan dari scope Farmasi.

## `BIL-GAP-TENDER-RECOVERY`

| Hal | Isi |
|---|---|
| Prioritas usulan | **P0** — dapat memacetkan tagihan pasien secara permanen |
| Pemilik | Billing/Kasir |
| Kasus bukti | invoice `BIL-20261001-00000001` · `f944d9ad-7957-42f6-ab90-8d1c5fc71364` |

### Masalah

Tender yang sudah tercipta lalu gagal sebelum sukses tetap berstatus `CREATED`, tetap memegang
nominal settlement, dan tidak memiliki endpoint `cancel`/`reconcile`/`retry` yang dapat dipakai
lewat HTTP. Settlement dan invoicenya macet.

### Bagaimana ia muncul

1. `POST settlements/{id}/tenders` memakai metode bayar yang bukan `IsCash`.
2. Baris tender **tercipta lebih dulu**, lalu validasi menolak:
   *"ProviderReference wajib tersedia untuk tender yang berhasil."*
   (`BillingSettlementService.cs:453`)
3. Baris `CREATED` itu kini memegang seluruh nominal: `pendingAmount` 25.000,
   `collectibleAmount` **0**.
4. Tender berikutnya ditolak *"Total metode pembayaran melebihi saldo yang harus dibayar."*
5. `POST settlements` kedua ditolak 409 *"Target masih memiliki settlement aktif; lanjutkan
   settlement yang sudah ada."*

Jalan keluarnya tertutup seluruhnya:

- `BillingSettlementsController` hanya memaparkan 4 rute — `POST settlements`,
  `POST settlements/{id}/tenders`, dan dua `GET`;
- `ReconcileTenderAsync` ada di service tetapi **tidak dipaparkan** sebagai endpoint;
- tidak ada sweeper maupun background service yang memulihkan tender `CREATED` yang menggantung.

### Akibat pada kasus bukti

Invoice `BIL-20261001-00000001` untuk resep `RX-20261001-00003`
(`4e4c6351-73aa-4cdf-84ad-f4f1c2936b92`) macet permanen lewat HTTP: settlement
`534a8c70-13f9-4c3a-b808-0157c9b9eec3` berstatus `IN_PROGRESS` dengan satu tender `CREATED`
senilai Rp25.000 yang tidak dapat disukseskan maupun dibatalkan. Resepnya tetap tertahan tahap 2.

Verifikasi happy path karena itu dijalankan pada invoice kedua dari nol.

### Arah perbaikan yang disarankan

Bukan keputusan Farmasi, hanya pengamatan: baris tender sebaiknya tidak tercipta sebelum validasi
yang dapat menolaknya selesai, **atau** disediakan rute pembatalan/rekonsiliasi tender menggantung
bagi kasir. Yang pertama mencegah, yang kedua memulihkan; keduanya mungkin dibutuhkan.

## Master data: `TUNAI` bertentangan dengan dirinya sendiri

| Hal | Isi |
|---|---|
| Prioritas usulan | **P1** |
| Pemilik | Billing/Kasir |
| Record | `MstPaymentMethod` kode `TUNAI` · `744e25ce-0a3b-4118-bab3-594ab4ad1def` |

`paymentMethodType = "Cash"` tetapi `isCash = false`. Karena
`BillingSettlementService.cs:453` membaca `IsCash` dan bukan `PaymentMethodType`, pembayaran tunai
diperlakukan sebagai pembayaran berperantara dan menuntut `ProviderReference` — nilai yang tidak
dapat dikirim klien karena `CreateTenderRequest` tidak punya field itu. Akibatnya **tidak ada satu
pun metode bayar pada lingkungan ini yang dapat menyelesaikan tender lewat HTTP**: ketujuhnya
ber-`isCash = false`, termasuk `JAMINAN` dan `PENJAMIN` yang juga tidak bertanda `IsInsurance`
maupun `IsCompanyGuarantor`.

Record existing **tidak disentuh**. Untuk pengujian dibuat metode baru lewat endpoint existing:
`UJI-PHM-TUNAI` "Tunai Uji Farmasi" · `a3f9e17a-3615-41cc-bd87-c3c1b1a7a131` ·
`IsCash = true`, `IsInsurance = false`, `IsCompanyGuarantor = false`.

Konsekuensi yang belum tertutup: `INSURANCE_APPROVED` (`BIL-AT-140`) tidak akan pernah terjadi
selama tidak ada metode bayar bertanda asuransi atau penjamin, walaupun kasir memilih Jaminan
Asuransi. Pembayaran apa pun akan diturunkan menjadi `PAID`.

## Perubahan environment untuk pengujian

Bukan gap, dicatat agar terlacak. Satu kolom ditambahkan pada basis data dev
`localhost / QuilvianNewDevIkbalFr` agar jalur charge dapat dijalankan, persis sesuai migration
`20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory` yang belum terpasang:

```sql
ALTER TABLE public."BilDiscountApplication"
  ADD COLUMN "DoctorDiscountMemoFile" character varying(500) NULL;
```

Diverifikasi sebelum (kolom tidak ada, tabel ada) dan sesudah (`character varying`, panjang 500,
nullable). Tanpa `dotnet ef database update`, tanpa kolom atau tabel lain. Hanya lingkungan dev;
lingkungan lain tetap membutuhkan migration itu dijalankan sebagaimana mestinya.
