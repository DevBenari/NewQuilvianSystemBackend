# Farmasi — Integration Contract Routing Depo

Contract version: `PHA-INT-ROUTING-v1`; status `approved`; owner Pharmacy Backend; disetujui product/domain owner 21 Agustus 2026.

| Producer | Consumer | Contract | Mode | Idempotency/kegagalan |
| --- | --- | --- | --- | --- |
| Registration | Resolver Farmasi | Encounter snapshot berdasarkan ID | Sinkron internal | Tidak ditemukan menghasilkan rejection |
| Master Data | Resolver Farmasi | Query lokasi eligible | Sinkron internal | Nol/ganda menghasilkan rejection |
| Resolver | Workflow resep | `PharmacyDepotRoutingResult` | In-process | Result tidak mengubah state; caller berhenti saat gagal |

Timeout mengikuti cancellation token request. Tidak ada retry otomatis di dalam resolver; retry berada pada caller dan selalu membaca konfigurasi terbaru. Tidak ada dead-letter karena tidak ada event eksternal.

Traceability: `PHA-DEC-040`, `PHA-DEC-041`, `PHA-DEP-004`, `PHA-DA-001`.

---

# Slice Financial Clearance — `PHA-INT-CLEARANCE-v1`

Status **draft** · input `PHA-DEC-063`–`070` · sisi penerbit `BIL-INT-014` · 21 September 2026.

## Arah dan bentuk

| Producer | Consumer | Contract | Mode | Idempotency dan kegagalan |
| --- | --- | --- | --- | --- |
| Billing | Farmasi | Surat clearance per resep: identitas resep dan tagihan, keadaan clearance, hasil finansial, kode sebab, nomor versi, waktu berlaku | Pembacaan baris di dalam proses yang sama | Dikunci nomor versi per resep. Surat bernomor lebih rendah diabaikan diam-diam. Kegagalan memproses dicatat beserta jumlah percobaan; surat tetap ada di sisi Billing |
| Farmasi | Billing | Pengakuan bahwa surat sudah diproses | Pemanggilan dalam proses | Pengakuan kedua ditolak tanpa mengubah apa pun |
| Farmasi | Billing | Pertanyaan keadaan clearance terkini sebuah resep | Pemanggilan dalam proses, baca murni | Aman dipanggil berulang. Resep tak dikenal dijawab belum diketahui, **bukan** galat |

Tidak ada HTTP, tidak ada antrian pesan, tidak ada outbox. Modul ini satu assembly dengan
Billing — konsisten dengan seluruh pola integrasi yang sudah berjalan di kedua modul.

## Yang Farmasi **MUST NOT** lakukan

| Larangan | Sebabnya |
| --- | --- |
| Membaca tabel tagihan, tender, atau alokasi pembayaran | Akan memberi Farmasi jalan menyimpulkan sendiri keadaan finansial — persis yang `RJ-BIL-BE-002` tutup |
| Menulis apa pun ke tabel Billing | Arah kebenaran satu arah |
| Menghitung sendiri hasil finansial dari cara bayar | Penentuan `Paid` versus `InsuranceApproved` milik Billing (`PHA-DEC-065`) |
| Menganggap ketiadaan surat sebagai izin | `PHA-DEC-067` fail-closed |

## Ketergantungan yang memblokir

Sisi penerbit (`BKC-DES-036`–`041`) **MUST** berdiri lebih dulu. Tanpa surat yang terbit, slice
ini tidak punya masukan apa pun — dan resep akan tetap macet seperti sekarang.

Ini ketergantungan satu arah: Billing dapat dibangun dan berguna tanpa Farmasi, karena Finance
juga menunggu penerbit yang sama.

Trace `PHA-DEC-063`–`070`, `BKC-DEC-106`–`109`. Tests `PHA-AT-CLR-01`–`PHA-AT-CLR-12`.
