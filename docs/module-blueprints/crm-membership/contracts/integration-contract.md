# Kontrak Integrasi — CRM Membership

| Field | Nilai |
|---|---|
| Contract version | `v1`, status `approved` (Yoga Aji Pratama, 2026-10-13) |
| Owner | Yoga Aji Pratama. Titik sentuh: owner `billing-kasir`, owner Registration/rawat jalan |
| Masukan | `03-domain-architecture.md` J1 s.d. J5 |

Semua integrasi bersifat **internal** (satu aplikasi, satu basis data). Tidak ada sistem luar
pada MVP. Notifikasi WhatsApp ditunda (`CRM-OQ-003`).

| ID | Produsen → Konsumen | Tujuan | Bentuk | Sinkron? | Idempotency | Kegagalan | Status |
|---|---|---|---|---|---|---|---|
| `INT-01` | `PatientMembershipStatusResolver` → pendaftaran RJ/IGD/rawat inap | Snapshot tingkat saat kunjungan didaftarkan (`CRM-DEC-005`) | Panggilan service dalam proses | Ya | Baca saja | Gagal baca: kunjungan tetap terdaftar sebagai Reguler, dan kejadiannya dicatat log. Pelayanan tidak boleh tertahan | RJ: **Reuse with adapter** (antrean). IGD/rawat inap: **Rencana**, perlu persetujuan owner Registration (`AG-01`) |
| `INT-02` | Resolver → `OutpatientQueueClassificationService` | Satu aturan member aktif (`CRM-CAP-016`) | Klasifikasi antrean memanggil resolver, bukan query sendiri | Ya | Baca saja | Sama dengan `INT-01` | **Rencana**, amendment ke `rawat-jalan` (`AG-03`) |
| `INT-03` | Resolver → nurse station, kiosk, layar pasien | Tampilan status member | Membaca proyeksi `MstPatient` yang ditulis resolver | Ya | — | Proyeksi disegarkan setiap perpindahan status dan setiap pukul 00.05 WIB | **Rencana** |
| `INT-04` | Membership → Billing | Manfaat diskon (`CRM-DEC-016`, `026`, `027`, `028`) | Billing membaca snapshot kunjungan + persen tingkat, lalu menerapkan diskon saat tagihan difinalkan | Ya | Dihitung ulang sampai final | Snapshot kosong berarti tanpa diskon | **Menunggu persetujuan owner billing** (pertanyaan 5) |
| `INT-05` | Billing → Membership | Kabar iuran lunas | `ConfirmFeePaidAsync(membershipId, chargeReferenceId, paidAt)` | Ya, dalam transaksi pelunasan atau lewat outbox billing | Kunci `chargeReferenceId` unik (`VAL-21`) | Lewat batas 7 hari: ditolak (`VAL-20`), billing memproses pengembalian | **OPEN DECISION** `DEC-CRM-001` |
| `INT-06` | Membership → Billing | Buat / batalkan tagihan iuran, minta refund | Bentuk menunggu billing | — | Satu tagihan aktif dan satu refund per membership | Rekonsiliasi harian membandingkan status membership dengan status tagihan | **OPEN DECISION** `DEC-CRM-001` |
| `INT-07` | Billing → Membership | Apakah manfaat sudah terpakai (`AG-02`) | Kueri: adakah tagihan final berdiskon yang memakai snapshot membership ini | Ya | Baca saja | Bila tidak dapat dipastikan, refund salah input ditolak (fail-closed) | **OPEN DECISION** `DEC-CRM-001` |
| `INT-08` | Waktu → `PatientMembershipLifecycleHostedService` | Kedaluwarsa, mulai Terjadwal, batal tidak dibayar | Hosted service saat start + harian 00.05 WIB | Tidak | Memeriksa status terkini sebelum mengubah. Dijalankan ulang tidak menggandakan riwayat | Satu membership gagal tidak menahan yang lain. Kegagalan dicatat log, dan akan dicoba lagi pada putaran berikutnya | **Rencana** |

## Rekonsiliasi

Sampai `DEC-CRM-001` diputuskan, tidak ada rekonsiliasi dengan billing. Setelah itu, laporan
harian wajib menampilkan dua kelompok: (1) membership Menunggu Pembayaran yang tagihannya sudah
lunas, dan (2) membership Aktif atau Terjadwal tanpa tagihan lunas, kecuali `LegacyImport`.
