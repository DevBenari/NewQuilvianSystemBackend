# Permintaan Amendment Billing-Kasir — Tagihan Iuran Membership

| Field | Nilai |
|---|---|
| Diajukan oleh | Yoga Aji Pratama (owner `crm-membership`) |
| Ditujukan kepada | Product/Domain Owner `billing-kasir` (nama belum tercatat di blueprint billing) |
| Tanggal | 2026-10-13 |
| Status | `draft` — menunggu keputusan owner billing |
| Dasar | `crm-membership/00-interview-decisions.md` `CRM-DEC-004`, `CRM-DEC-013`, `CRM-DEC-014`, `CRM-DEC-019`, `CRM-DEC-020`; `01-existing-capability-map.md` `CRM-CAP-008` |
| Backend SHA | `36422a4d` |

## Latar Belakang

Rumah sakit menjual membership pasien prioritas (Silver, Gold, Platinum, Executive). Owner CRM
sudah memutuskan bahwa membership baru **aktif setelah iuran lunas di kasir**.

Saat ini kasir belum bisa menerima pembayaran iuran. Setiap tagihan di billing wajib terikat
pada satu kunjungan pasien (`BilInvoice.EncounterId` wajib diisi, `BilInvoice.cs:11`). Iuran
membership tidak berasal dari kunjungan. Piutang non-pasien yang sudah ada
(`FinNonPatientReceivable`) khusus untuk sewa parkir dan tenant, dan sengaja dipisah dari billing.

## Yang Diminta dari Billing-Kasir

1. **Tagihan iuran tanpa kunjungan.** Kasir dapat menerima tagihan iuran membership yang
   terhubung ke tepat satu pendaftaran membership dan satu pasien, tanpa memilih kunjungan.
2. **Kabar lunas.** Ketika tagihan iuran lunas, sistem membership diberi tahu agar status
   berpindah dari Menunggu Pembayaran ke Aktif.
3. **Pembatalan tagihan.** Bila pendaftaran tidak dibayar dalam 7 hari kalender, sistem
   membership membatalkan pendaftaran, dan tagihan iurannya ikut dibatalkan.
4. **Refund penuh untuk salah input.** Bila CRM membatalkan membership karena salah input
   petugas dan belum ada diskon yang terpakai, iuran dikembalikan penuh lewat proses refund kasir.

## Contoh Alur

1. Petugas CRM mendaftarkan Ibu Sari (nama samaran) sebagai member Gold pada 13 Okt 2026 pukul
   09.00. Iuran Gold Rp 2.400.000 (angka samaran).
2. Kasir membuka tagihan iuran milik Ibu Sari tanpa memilih kunjungan, lalu menerima
   pembayaran tunai Rp 2.400.000 pukul 10.15.
3. Tagihan lunas, dan membership Gold Ibu Sari aktif sampai 12 Okt 2027.
4. Jalur tidak normal: bila sampai 20 Okt 2026 pukul 09.00 belum dibayar, pendaftaran dan
   tagihannya batal otomatis.

## Batasan yang Sudah Diputuskan CRM

- Tidak boleh memakai kunjungan semu untuk menampung iuran.
- Tidak boleh berupa bukti bayar manual yang diaktifkan sendiri oleh CRM.
- Membership **bukan** cara bayar. Kasir sudah menolak metode bayar `IsMembership` sebagai
  tender (`BillingSettlementService.cs:1178-1187`), dan perilaku itu dipertahankan.

## Yang Diputuskan Owner Billing

| No | Pertanyaan | Keputusan |
|---|---|---|
| 1 | Apakah billing setuju menyediakan tagihan non-kunjungan untuk iuran membership? | |
| 2 | Bentuk penyimpanannya (perluasan `BilInvoice` atau entity tagihan terpisah)? | |
| 3 | Bagaimana pencatatan akuntansi iuran (pendapatan di muka atau pendapatan langsung)? | |
| 4 | Siapa yang boleh menyetujui refund iuran salah input? | |

Disetujui oleh: ______________________ Tanggal: ____________
