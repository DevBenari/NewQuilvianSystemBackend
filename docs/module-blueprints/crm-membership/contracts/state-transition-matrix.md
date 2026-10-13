# Matriks Perpindahan Status — CRM Membership

| Field | Nilai |
|---|---|
| Contract version | `v1`, status `approved` (Yoga Aji Pratama, 2026-10-13) |
| Owner | Yoga Aji Pratama |
| Masukan | `03-domain-architecture.md` G, `CRM-DEC-004`, `005`, `012`, `013`, `014`, `021`, `024`, `025` |

Nilai enum `MembershipStatus`: `Active = 1`, `Expired = 2`, `Cancelled = 4`, `Upgraded = 5`,
`PendingPayment = 7`, `Scheduled = 8`, `NeedsVerification = 9`. Nilai `Suspended = 3` dan
`Downgraded = 6` tidak dipakai di MVP (`CRM-OQ-002`). Semua perintah yang menuju status itu
ditolak.

## Perpindahan yang sah

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Daftar baru | `PendingPayment` | Petugas CRM (`PatientMembership : Create`) | Pasien aktif, tidak punya Aktif/Menunggu, tingkat berbayar dengan iuran > 0 | 400/409 sesuai `validation-matrix.md` |
| — | Daftar perpanjangan | `PendingPayment` (`Renewal`) | Petugas CRM | Ada Aktif, tidak ada Terjadwal/Menunggu, hari ini ≥ tanggal akhir − 30 hari | 409 "Perpanjangan baru bisa mulai 12 Sep 2027" |
| — | Daftar upgrade | `PendingPayment` (`Upgrade`) | Petugas CRM | Ada Aktif, tidak ada Menunggu, `PriorityLevel` tingkat baru > tingkat Aktif | 409 |
| `PendingPayment` (`New`/`Upgrade`) | Iuran lunas | `Active` | Billing (S2) lewat `ConfirmFeePaidAsync` | Belum lewat `PaymentDueAt`. Periode = tanggal lunas WIB s.d. + `ValidityMonths` − 1 hari | Ditolak dan dikembalikan ke billing sebagai pengembalian |
| `Active` (lama, saat upgrade lunas) | Upgrade lunas | `Upgraded` | Sistem, dalam transaksi yang sama | — | — |
| `Scheduled` (saat upgrade lunas) | Upgrade lunas | `Cancelled` (`SupersededByUpgrade`), minta refund | Sistem, dalam transaksi yang sama | — | — |
| `PendingPayment` (`Renewal`) | Iuran lunas | `Scheduled` | Billing (S2) | Periode = tanggal akhir Aktif + 1 s.d. + `ValidityMonths` − 1 hari | — |
| `PendingPayment` | Lewat `PaymentDueAt` | `Cancelled` (`Unpaid`) | Sistem terjadwal | — | — |
| `Active` | Tanggal akhir lewat (hari WIB berikutnya) | `Expired` | Sistem terjadwal | — | — |
| `Scheduled` | Tanggal mulai tiba | `Active` | Sistem terjadwal | Diproses setelah yang lama `Expired`, dalam satu transaksi | — |
| `Active` | Batal permintaan pasien | `Cancelled` (`PatientRequest`) | Petugas CRM (`PatientMembership : Cancel`) | Alasan wajib. Tanpa refund. Terjadwal milik pasien ikut dibatalkan dengan refund (`CRM-DEC-025`) | 400 |
| `Active` | Batal salah input | `Cancelled` (`InputError`) | Petugas CRM | Alasan wajib. Refund hanya bila manfaat belum terpakai (`AG-02`, menunggu S2) | 409 bila manfaat sudah terpakai: "Diskon membership ini sudah dipakai, refund tidak tersedia" |
| `Scheduled` | Batal (pasien/salah input) | `Cancelled`, refund penuh | Petugas CRM | Alasan wajib | 400 |
| `PendingPayment` | Batal oleh petugas (`CRM-DEC-029`) | `Cancelled` (kategori yang dipilih) | Petugas CRM | Alasan wajib. Tagihan iuran dibatalkan | 400 |
| `NeedsVerification` | Diakui | `Active` | Petugas CRM (`PatientMembership : Verify`) | Periode sah, tanggal akhir ≥ hari ini, tidak bertabrakan dengan Aktif lain | 409 |
| `NeedsVerification` | Diakui dengan tanggal akhir sudah lewat | `Expired` | Petugas CRM | Periode sah | — |
| `NeedsVerification` | Ditolak | `Cancelled` | Petugas CRM | Alasan wajib | 400 |
| (status apa pun) | Ubah catatan | (tetap) | Petugas CRM (`PatientMembership : Update`) | Bukan status akhir | 409 untuk status akhir |

## Perpindahan yang tidak sah (ditolak dengan 409)

| Dari | Upaya | Alasan penolakan |
|---|---|---|
| `Expired`, `Upgraded`, `Cancelled` | Perintah apa pun selain baca | Status akhir. Gunakan pendaftaran baru |
| `Active`, `Scheduled` | Mengubah pasien, tingkat, atau periode | `CRM-DEC-022` |
| (siapa pun selain billing) | `PendingPayment` → `Active`/`Scheduled` | Aktivasi hanya dari pelunasan (`CRM-DEC-004`, `019`) |
| `Active` | Upgrade ke tingkat sama atau lebih rendah | `AG-04`. Turun tingkat tidak tersedia di MVP |
| `Active` | Perpanjangan kedua saat sudah ada Terjadwal | `INV-02` |
| (apa pun) | Menuju `Suspended` atau `Downgraded` | Tidak dipakai di MVP |
| `PendingPayment` | Iuran lunas setelah lewat `PaymentDueAt` | `CRM-DEC-014`: pasien daftar ulang |

## Contoh berurutan

| Waktu (WIB) | Kejadian | Gold #1 | Gold #2 |
|---|---|---|---|
| 13 Okt 2026 09.00 | CRM mendaftarkan | `PendingPayment` | — |
| 13 Okt 2026 10.15 | Iuran lunas | `Active` 13 Okt 2026 s.d. 12 Okt 2027 | — |
| 1 Okt 2027 08.00 | CRM mendaftarkan perpanjangan | `Active` | `PendingPayment` (`Renewal`) |
| 1 Okt 2027 08.30 | Iuran lunas | `Active` | `Scheduled` 13 Okt 2027 s.d. 12 Okt 2028 |
| 13 Okt 2027 00.05 | Proses terjadwal | `Expired` | `Active` |
