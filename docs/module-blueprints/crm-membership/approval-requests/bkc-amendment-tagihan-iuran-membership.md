# Permintaan Amendment Billing-Kasir — Tagihan Iuran Membership

| Field | Nilai |
|---|---|
| Diajukan oleh | Yoga Aji Pratama (owner `crm-membership`) |
| Ditujukan kepada | Product/Domain Owner `billing-kasir` (nama belum tercatat di blueprint billing) |
| Tanggal | 2026-10-13 |
| Status | `draft` — menunggu keputusan owner billing |
| Dasar | `crm-membership/00-interview-decisions.md` `CRM-DEC-004`, `CRM-DEC-013`, `CRM-DEC-014`, `CRM-DEC-019`, `CRM-DEC-020`, `CRM-DEC-024` s.d. `CRM-DEC-027`; `01-existing-capability-map.md` `CRM-CAP-008` |
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
5. **Refund penuh untuk periode Terjadwal.** Bila member yang sudah membayar perpanjangan
   (periode belum mulai) melakukan upgrade atau berhenti, iuran perpanjangan dikembalikan penuh
   (`CRM-DEC-025`). Contoh: iuran perpanjangan Gold Rp 2.400.000 (angka samaran) dibayar 1 Okt
   2027 untuk periode mulai 13 Okt 2027. Pasien upgrade pada 5 Okt 2027, sehingga Rp 2.400.000
   dikembalikan.
6. **Iuran dibayar di muka.** Perpanjangan boleh dibayar paling cepat 30 hari sebelum periode
   berjalan (`CRM-DEC-024`). Ini relevan untuk pertanyaan perlakuan akuntansi di bawah.

## Yang Diminta dari Billing-Kasir — Penerapan Diskon Member

Aturan manfaat sudah diputuskan owner CRM. Billing diminta menerapkannya:

1. Diskon member hanya untuk kunjungan yang dibayar pribadi sepenuhnya (`CRM-DEC-016`).
2. Diskon dihitung ulang otomatis mengikuti penjamin pada saat tagihan difinalkan, dan setiap
   perubahan tercatat (`CRM-DEC-026`).
3. Kelompok diskon mengikuti asal item: `CONSULTATION`, `PROCEDURE`, `LABORATORY`, `RADIOLOGY`,
   `PHARMACY`, dan biaya pendaftaran/administrasi. Item lain tidak didiskon (`CRM-DEC-027`).
4. Tingkat yang dipakai adalah tingkat member pada saat kunjungan didaftarkan (`CRM-DEC-005`).

Contoh: member Gold (konsultasi 10%, farmasi 5%) berobat pribadi. Konsultasi Rp 300.000
didiskon Rp 30.000, obat Rp 200.000 didiskon Rp 10.000, dan item ad-hoc Rp 50.000 tidak
didiskon, sehingga tagihan final Rp 510.000.

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
| 4 | Siapa yang boleh menyetujui refund iuran (salah input dan periode Terjadwal)? | |
| 5 | Apakah billing setuju menerapkan diskon member sesuai bagian "Penerapan Diskon Member"? | |

Disetujui oleh: ______________________ Tanggal: ____________
