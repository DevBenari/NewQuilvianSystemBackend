# Matriks Validasi — CRM Membership

| Field | Nilai |
|---|---|
| Contract version | `v1`, status `approved` (Yoga Aji Pratama, 2026-10-13) |
| Owner | Yoga Aji Pratama |
| Masukan | `CRM-DEC-003` s.d. `CRM-DEC-028`, `03-domain-architecture.md` `INV-01` s.d. `INV-11` |

| ID | Aturan | Berlaku pada | Kondisi gagal | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `VAL-01` | Pasien wajib ada, aktif, dan ber-No. RM (`CRM-DEC-003`) | Daftar baru | Pasien tidak ditemukan/nonaktif | "Pasien tidak ditemukan atau tidak aktif. Daftarkan pasien lebih dulu." | 404 |
| `VAL-02` | Tingkat wajib aktif dan bertipe Silver/Gold/Platinum/Executive (`CRM-DEC-006`) | Daftar baru, upgrade | Tingkat Regular/Corporate/Family/Other atau nonaktif | "Tingkat ini tidak bisa dipilih untuk pendaftaran membership." | 400 |
| `VAL-03` | Nominal iuran tingkat harus lebih dari 0 | Daftar baru, perpanjangan, upgrade | `AnnualFeeAmount = 0` | "Nominal iuran tingkat Gold belum diisi. Hubungi admin master data." | 409 |
| `VAL-04` | Pasien tidak boleh punya membership Aktif saat daftar baru (`INV-01`) | Daftar baru | Sudah ada Aktif | "Pasien sudah member Gold sampai 12 Okt 2027. Gunakan perpanjangan atau upgrade." | 409 |
| `VAL-05` | Paling banyak satu Menunggu Pembayaran (`INV-03`) | Daftar baru, perpanjangan, upgrade | Sudah ada Menunggu | "Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-2610-000123)." | 409 |
| `VAL-06` | Nomor member dari sistem (`CRM-DEC-023`) | Daftar baru, perpanjangan, upgrade | Request membawa `memberNumber` | Field diabaikan, tidak ada pesan | — |
| `VAL-07` | Perpanjangan hanya dalam jendela 30 hari (`CRM-DEC-024`) | Perpanjangan | Hari ini < tanggal akhir − 30 | "Perpanjangan baru bisa mulai 12 Sep 2027." | 409 |
| `VAL-08` | Belum ada Terjadwal (`INV-02`) | Perpanjangan | Sudah ada Terjadwal | "Perpanjangan untuk periode berikutnya sudah dibayar." | 409 |
| `VAL-09` | Upgrade hanya ke `PriorityLevel` lebih tinggi (`AG-04`) | Upgrade | Tingkat sama/lebih rendah | "Upgrade hanya ke tingkat yang lebih tinggi dari Gold." | 409 |
| `VAL-10` | Alasan pembatalan wajib 10–500 karakter (`CRM-DEC-013`) | Batal | Kosong/terlalu pendek | "Tulis alasan pembatalan minimal 10 karakter." | 400 |
| `VAL-11` | Kategori batal hanya `PatientRequest` atau `InputError` dari petugas | Batal | Kategori lain | "Pilih alasan: permintaan pasien atau salah input." | 400 |
| `VAL-12` | Refund salah input hanya bila manfaat belum terpakai (`CRM-DEC-013`) | Batal `InputError` | Diskon sudah dipakai | "Diskon membership ini sudah dipakai pada kunjungan 14 Okt 2026, sehingga pembatalan salah input dengan refund tidak tersedia." | 409 |
| `VAL-13` | Verifikasi hanya untuk status Perlu Verifikasi | Verifikasi | Status lain | "Membership ini tidak sedang menunggu verifikasi." | 409 |
| `VAL-14` | Diakui wajib dengan periode sah: mulai ≤ akhir, panjang ≤ `ValidityMonths` | Verifikasi `Accept` | Periode kosong/terbalik/terlalu panjang | "Periode yang diakui tidak sah. Tanggal mulai harus sebelum tanggal akhir dan maksimal 12 bulan." | 400 |
| `VAL-15` | Diakui tidak boleh bertabrakan dengan Aktif lain milik pasien (`INV-01`) | Verifikasi `Accept` | Sudah ada Aktif | "Pasien sudah punya membership aktif. Tolak data lama ini atau batalkan yang aktif lebih dulu." | 409 |
| `VAL-16` | Catatan maks 250 karakter, tidak untuk status akhir | Ubah catatan | Terlalu panjang/status akhir | "Catatan maksimal 250 karakter." / "Membership yang sudah berakhir tidak bisa diubah." | 400 / 409 |
| `VAL-17` | `expectedRowVersion` harus sama dengan data terkini | Semua perintah ubah | Berbeda | "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." | 409 |
| `VAL-18` | Pasien, tingkat, periode tidak bisa diubah (`CRM-DEC-022`) | Semua | Klien lama memanggil `PUT` | "Fitur ubah membership sudah tidak tersedia. Gunakan upgrade atau pembatalan." | 410 |
| `VAL-19` | Kiosk tidak boleh membuat (`CRM-DEC-018`) | `POST /`, `/kiosk` | Dipanggil | Rute tidak ada | 404 |
| `VAL-20` | Iuran lunas setelah `PaymentDueAt` ditolak (`CRM-DEC-014`) | `ConfirmFeePaidAsync` | Lewat batas | "Pendaftaran sudah batal karena tidak dibayar 7 hari. Proses sebagai pengembalian." (ke billing) | 409 |
| `VAL-21` | Kabar lunas ganda tidak mengaktifkan dua kali | `ConfirmFeePaidAsync` | `chargeReferenceId` sama sudah diproses | Dianggap berhasil tanpa perubahan | 200 |
| `VAL-22` | Pencarian minimal 3 karakter | `GET /admin/patient-lookup` | Kata kunci < 3 | "Ketik minimal 3 karakter No. RM, NIK, atau No. Telp." | 400 |

Semua pembandingan tanggal memakai tanggal kalender WIB (`INV-11`). Contoh `VAL-07`: tanggal
akhir 12 Okt 2027, jendela 30 hari, sehingga perpanjangan diterima mulai 12 Sep 2027 00.00 WIB.
