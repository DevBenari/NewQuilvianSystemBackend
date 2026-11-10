# Optik — Matriks Validasi

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-VAL-v1.3` — status `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Dasarnya: `OPT-VAL-v1.2` `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Hanya keterangan bagian 8 yang berubah: `OPT-APR-013` disetujui (`OPT-DEC-084`) |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-06 untuk `v1.3`, `v1.2`, dan `v1.1`; `v1` 2026-11-03 |
| `input_revision` | `00-interview-decisions.md` r12; `02-backend-architecture.md` r4 |

Kode memakai pola `OPT-VAL-nnn` dan dikirim sebagai kode `ApiResponse.Fail`. Pesan ditampilkan
apa adanya ke pengguna.

## 1. Izin dan kewenangan

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- | --- |
| `OPT-VAL-001` | Hanya RO/Sp.M membuat/merevisi pemeriksaan dan resep | Pemeriksaan, resep | Akun tidak tertaut pegawai berprofesi RO/Sp.M | "Anda tidak berwenang membuat resep atau pemeriksaan refraksi." | `403` |
| `OPT-VAL-002` | Izin praktik berlaku pada tanggal WIB; SIP **dan** STR wajib (`OPT-DEC-056`) | Pemeriksaan, resep, pemeriksaan ulang | Tidak ada STR/SIP aktif, terverifikasi, tidak dicabut dengan `ExpiredDate.Date ≥` tanggal WIB hari ini. **Contoh:** SIP berakhir 31-10-2026, ditolak pada 03-11-2026 | "SIP Anda tidak berlaku sejak 31-10-2026." / "SIP Anda belum tercatat berlaku. Hubungi bagian SDM." / "STR Anda tidak berlaku sejak dd-MM-yyyy." / "STR Anda belum tercatat berlaku. Hubungi bagian SDM." | `403` |
| `OPT-VAL-003` | Data HR tidak terbaca | Sama | Pembaca izin gagal | "Data izin praktik sedang tidak dapat diperiksa. Coba lagi beberapa saat." | `503` |
| `OPT-VAL-004` | Lewat rentang hanya Sp.M | Resep | Ada nilai di luar rentang dan pelaku bukan Sp.M | "SPH di luar rentang −20,00..+20,00; hanya Sp.M yang dapat mengonfirmasi." | `403` |
| `OPT-VAL-005` | Alasan lewat rentang | Resep | Sp.M melewati rentang tanpa `RangeOverrideReason` | "Tuliskan alasan konfirmasi ukuran di luar rentang." | `400` |

## 2. Isian resep (`OPT-DEC-045`, `046`)

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| `OPT-VAL-010` | SPH | Di luar −20,00..+20,00 atau bukan kelipatan 0,25 (contoh −1,30) | "SPH harus kelipatan 0,25 antara −20,00 dan +20,00." | `400` |
| `OPT-VAL-011` | CYL | > 0, < −6,00, atau bukan kelipatan 0,25 | "CYL harus notasi minus, kelipatan 0,25, antara −6,00 dan 0,00." | `400` |
| `OPT-VAL-012` | AXIS wajib | CYL ≠ 0 dan AXIS kosong | "AXIS wajib diisi bila ada silinder." | `400` |
| `OPT-VAL-013` | AXIS rentang | AXIS < 0 atau > 180, atau CYL = 0 tetapi AXIS terisi | "AXIS harus 0–180 dan hanya diisi bila ada silinder." | `400` |
| `OPT-VAL-014` | ADD | Di luar +0,75..+3,50 atau bukan kelipatan 0,25 | "ADD harus kelipatan 0,25 antara +0,75 dan +3,50." | `400` |
| `OPT-VAL-015` | PD | Di luar 50..80 mm | "PD harus antara 50 dan 80 mm." | `400` |
| `OPT-VAL-016` | Resep plus disalin | Resep luar bernotasi plus | Tidak ditolak: sistem mentransposisi. **Contoh:** +1,00 / +0,50 × 90 → +1,50 / −0,50 × 180; teks asli disimpan | — |
| `OPT-VAL-017` | Lensa kontak lengkap | Resep lensa kontak tanpa power, BC, DIA, atau merk/jenis per mata yang dipakai | "Resep lensa kontak wajib berisi power, BC, DIA, dan merk/jenis." | `400` |
| `OPT-VAL-018` | BC/DIA | Di luar rentang `OptSetting` (bawaan BC 7,50–9,50, DIA 13,00–15,00) | "BC harus antara 7,50 dan 9,50." | `400` |
| `OPT-VAL-019` | Resep luar | `ExternalAuthorName`, `ExternalFacilityName`, atau `IssuedDate` kosong | "Lengkapi penulis, asal fasilitas, dan tanggal resep luar." | `400` |
| `OPT-VAL-020` | Tanggal resep | `IssuedDate` di masa depan | "Tanggal resep tidak boleh melewati hari ini." | `400` |
| `OPT-VAL-021` | Alasan revisi | Versi ≥ 2 tanpa `RevisionReason` | "Tuliskan alasan revisi." | `400` |
| `OPT-VAL-022` | Resep terkunci | Revisi saat `LockedAt` terisi | "Resep terkunci karena lensa sudah dipesan." | `409` |
| `OPT-VAL-023` | Resep kedaluwarsa | Dipakai pesanan baru saat `ValidUntil` < hari ini (WIB) | "Resep berusia lebih dari 12 bulan, lakukan pemeriksaan ulang." | `409` |

## 3. Pemeriksaan (`OPT-DEC-042`, `047`)

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| `OPT-VAL-030` | Kunjungan terbuka | Kunjungan sudah ditutup/batal | "Kunjungan sudah ditutup. Daftarkan kunjungan baru." | `409` |
| `OPT-VAL-031` | Isian minimum final | Tidak ada visus jauh untuk kedua mata | "Isi visus jauh mata kanan dan kiri sebelum menyimpan final." | `400` |
| `OPT-VAL-032` | Final sekali | Final ulang | "Pemeriksaan sudah final. Gunakan revisi bila ada koreksi." | `409` |
| `OPT-VAL-033` | Batal final | Batalkan pemeriksaan `Final` | "Pemeriksaan final tidak dapat dibatalkan; gunakan revisi." | `409` |
| `OPT-VAL-034` | Format visus (`OPT-DEC-059`) | Bukan notasi sah: `n/n` (boleh berdesimal), `1/~`, atau `0`. **Contoh:** `6/12`, `6/7.5`, `1/~`, dan `0` diterima; `enam` ditolak | "Visus ditulis seperti 6/12." | `400` |

## 4. Pesanan

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| `OPT-VAL-040` | Barang Optik | `DrugId` tanpa `OptItemProfile` aktif | "Barang ini belum terdaftar sebagai barang Optik." | `400` |
| `OPT-VAL-041` | Resep untuk berukuran | Pesanan berpengerjaan tanpa versi resep | "Pilih resep untuk pesanan kacamata." | `400` |
| `OPT-VAL-042` | Resep lensa kontak | Item kategori lensa kontak tanpa resep lensa kontak berlaku (`OPT-DEC-046`) | "Lensa kontak wajib memakai resep lensa kontak yang masih berlaku." | `409` |
| `OPT-VAL-043` | Jenis lensa | Lensa kacamata tanpa `LensSupply` | "Tentukan lensa stok atau custom." | `400` |
| `OPT-VAL-044` | Jumlah | `Quantity` ≤ 0 | "Jumlah harus lebih dari 0." | `400` |
| `OPT-VAL-045` | Tarif ada | Billing menolak fakta karena tarif tidak ditemukan | "Tarif barang FRM-A-HTM-52 belum ada. Hubungi bagian tarif." | `400` |
| `OPT-VAL-046` | Minimum uang muka | Terbayar < `DownPaymentMinPercentSnapshot` × bagian pasien. **Contoh:** bagian pasien Rp 1.050.000, minimum 50% = Rp 525.000, terbayar Rp 500.000 | "Uang muka belum memenuhi minimum Rp 525.000." | `409` |
| `OPT-VAL-047` | Aturan sementara satu pesanan | Ada pesanan berpengerjaan lain di kunjungan yang sama yang belum lunas (`OPT-DEC-035`) | "Lunasi pesanan OPT-ORD-… dulu atau daftarkan kunjungan baru." | `409` |
| `OPT-VAL-048` | Tujuan pengerjaan | `WorkshopType` kosong, atau `ExternalLab` tanpa supplier, atau `EstimatedReadyDate` < hari ini | "Lengkapi tujuan pengerjaan dan perkiraan jadi." | `400` |
| `OPT-VAL-049` | Stok cukup | Stok tersedia depo Optik < kebutuhan saat kirim/serah | "Stok frame FRM-A-HTM-52 di depo Optik tidak cukup." | `409` |
| `OPT-VAL-050` | Lunas | Serahkan saat bagian pasien belum lunas | "Tagihan belum lunas. Sisa Rp 525.000." | `409` |
| `OPT-VAL-051` | Identitas | `HandoverIdentityVerified = false` | "Cocokkan identitas pasien sebelum menyerahkan." | `400` |
| `OPT-VAL-052` | Wakil | Pengambil bukan pasien tetapi nama/hubungan kosong | "Isi nama dan hubungan pengambil." | `400` |
| `OPT-VAL-053` | Alasan batal | Alasan kosong | "Tuliskan alasan pembatalan." | `400` |
| `OPT-VAL-054` | Tanda potong | Pengajuan batal sesudah pengerjaan tanpa `IsLensCut` untuk lensa stok | "Tandai apakah lensa stok sudah dipotong." | `400` |
| `OPT-VAL-055` | Satu pengajuan | Pengajuan batal kedua saat yang pertama `Requested` | "Masih ada pengajuan batal yang menunggu keputusan." | `409` |
| `OPT-VAL-056` | Batas tidak diambil | Tutup sebelum `ReadyAt` + `NotCollectedCloseDays` | "Pesanan baru bisa ditutup sesudah 90 hari sejak siap diambil." | `409` |
| `OPT-VAL-057` | Status tidak sah | Transisi di luar `state-transition-matrix.md` | "Tindakan ini tidak berlaku untuk pesanan berstatus …" | `409` |
| `OPT-VAL-058` | Concurrency | `ExpectedVersion` ≠ `Version` | "Pesanan sudah diubah petugas lain. Muat ulang halaman." | `409` |

## 5. Keluhan garansi

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| `OPT-VAL-060` | Pesanan diserahkan | Pesanan asal belum `HandedOver` | "Keluhan hanya untuk kacamata yang sudah diserahkan." | `409` |
| `OPT-VAL-061` | Kunjungan baru | `EncounterId` = kunjungan pesanan asal, atau milik pasien lain | "Daftarkan kunjungan keluhan baru untuk pasien ini." | `400` |
| `OPT-VAL-062` | Dalam garansi | Setujui remake saat `IsWithinWarranty = false`. **Contoh:** diserahkan 05-11-2026, keluhan ukuran 06-12-2026 (31 hari) | "Keluhan di luar masa garansi. Proses sebagai pesanan berbayar." | `409` |
| `OPT-VAL-063` | Wajib diperiksa | Putuskan sebelum `Examined` | "Keluhan harus diperiksa ulang oleh RO/Sp.M lebih dulu." | `409` |
| `OPT-VAL-064` | Satu remake | Remake kedua untuk keluhan yang sama | "Keluhan ini sudah menghasilkan remake." | `409` |

## 6. Pengaturan

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| `OPT-VAL-070` | Persentase uang muka | < 0 atau > 100 | "Persentase uang muka harus 0–100." | `400` |
| `OPT-VAL-071` | Hari pengingat dan tutup | Pengingat < 1, atau batas tutup ≤ pengingat | "Batas tutup harus lebih lama dari batas pengingat." | `400` |
| `OPT-VAL-072` | Usia resep | < 1 atau > 60 bulan | "Usia resep harus 1–60 bulan." | `400` |
| `OPT-VAL-073` | Rentang BC/DIA | Min ≥ Max | "Nilai minimum harus lebih kecil dari maksimum." | `400` |

## 7. Kiriman ganda (`OPT-DEC-077`)

Kode ini mengikuti kode umum yang sudah dipakai `POST visits`, bukan kode `OPT-VAL` baru.

| Kode | Aturan | Berlaku pada | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- | --- |
| `OPT-INVALID-REQUEST` | Kunci wajib | `POST visits` (sudah ada) | Header `Idempotency-Key` kosong | "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat kunjungan ganda." | `400` |
| `OPT-INVALID-REQUEST` | Kunci wajib | `POST prescriptions`, `POST prescriptions/external` | Header kosong atau Guid nol | "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat resep ganda." | `400` |
| `OPT-INVALID-REQUEST` | Kunci wajib | `POST orders` | Sama | "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat pesanan ganda." | `400` |
| `OPT-INVALID-REQUEST` | Kunci wajib | `POST warranty-claims` | Sama | "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat keluhan ganda." | `400` |
| — | Kunci sudah dipakai | Empat `POST` yang sama | Kunci sudah menghasilkan data. **Contoh:** keluhan KLM-0001 dikirim ulang | Tidak ditolak: jawaban `200` berisi data pertama, `IsReplay = true` | `200` |

## 8. Penjamin Perusahaan pada kunjungan (`OPT-DEC-078`)

Penegaknya `EncounterIntakeService` milik Registrasi. `OptVisitService` meneruskan pesannya apa adanya dengan kode `OPT-REG-REJECTED`.
Perubahan ini disetujui `OPT-APR-013` dan tersedia sejak `BE-OPT-061`.

| Kode | Aturan | Kondisi | Pesan | HTTP |
| --- | --- | --- | --- | --- |
| — | Unit Optik menerima penjamin | Kunjungan unit Optik dengan kartu yang lolos seluruh baris di bawah | Tidak ditolak; kunjungan tersimpan | `200` |
| `OPT-REG-REJECTED` | Unit lain tetap ditolak | Kunjungan penunjang selain Optik dan MCU, misalnya Laboratorium | "Penjamin perusahaan hanya dapat dipilih lewat pendaftaran loket." | `400` |
| `OPT-REG-REJECTED` | Kartu wajib dipilih | `PatientCompanyGuarantorId` kosong | "Kartu penjamin perusahaan pasien wajib dipilih untuk pembayaran Penjamin Perusahaan." | `400` |
| `OPT-REG-REJECTED` | Kartu aktif | Kartu terhapus atau tidak aktif | "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi." | `400` |
| `OPT-REG-REJECTED` | Kartu milik pasien | Kartu milik pasien lain | "Kartu penjamin yang dipilih bukan milik pasien ini." | `400` |
| `OPT-REG-REJECTED` | Masa berlaku kartu | Tanggal pelayanan di luar masa kartu. **Contoh:** kartu berakhir 31-10-2026, kunjungan 05-11-2026 | "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi." | `400` |
| `OPT-REG-REJECTED` | Kelayakan kartu | `IsEligible = false` | "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai." | `400` |
| `OPT-REG-REJECTED` | Kerja sama berlaku | Kontrak perusahaan berakhir atau tidak aktif | "Kerja sama dengan perusahaan penjamin ini sudah berakhir pada tanggal pelayanan." | `400` |
| `OPT-REG-REJECTED` | Benefit karyawan internal tetap lewat loket | `IsEmployeeBenefitGuarantor = true`, karena verifikasi HR hanya ada di loket | "Penjamin benefit karyawan internal hanya dapat dipilih lewat pendaftaran loket." | `400` |

## 9. Kode tambahan di luar pola `OPT-VAL` (laporan `BE-OPT-033`)

| Kode | Arti bagi pengguna | HTTP |
| --- | --- | --- |
| `OPT-DEPOT-MISSING` | Depo Optik belum terdaftar, sehingga stok tidak bisa dipotong | `409` |
| `OPT-STOCK-REJECTED` | Farmasi menolak mutasi stok | `409` |
| `OPT-STOCK-FORBIDDEN` | Akun tidak berizin melakukan mutasi stok | `403` |
| `OPT-STOCK-UNAVAILABLE` | Layanan stok sedang tidak dapat diakses; tindakan ditolak tanpa mengubah data | `503` |

## 10. Jejak keputusan

`OPT-DEC-005`, `007`, `009`, `012`, `015`, `017`, `022`, `023`, `035`, `038`, `040`, `041`, `042`, `044`..`049`, `056`, `059`, `077`, `078`.
