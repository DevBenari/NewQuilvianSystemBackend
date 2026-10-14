# CRM Membership — Arsitektur Domain

## A. Identitas Arsitektur

| Field | Nilai |
|---|---|
| Blueprint ID | `crm-membership` |
| Revision arsitektur | `1` |
| Status | `draft` (belum disetujui owner) |
| Tanggal | 2026-10-13 |
| Scope | Slice `S1a`, `S1b`, `S3`, `S4`, `S5` dari `02-requirement-completeness-assessment.md` revisi 1a. Slice `S2` hanya sebagai kontrak masuk/keluar |
| Kesiapan requirement | `PARTIALLY_READY` (S2 `BUSINESS_DECISION_REQUIRED`) |
| Keputusan yang dipakai | `CRM-DEC-001` s.d. `CRM-DEC-028` (approved, Yoga Aji Pratama, 2026-10-13) |
| Keputusan terbuka | `DEC-CRM-001` (tagihan iuran di billing) |
| Bukti keadaan saat ini | `01-existing-capability-map.md`: audit BE `36422a4d` / FE `9aba9929d`, impact scan BE `1aecf642` / FE `f1e5e273a` |
| Baseline rujukan | Tidak dipakai |
| **Kesiapan arsitektur** | **`DOMAIN_ARCHITECTURE_PARTIAL`** (lihat bagian N) |

> Dokumen ini memodelkan **makna bisnis dan kepemilikan**, bukan tabel atau endpoint. Nama
> seperti `MstPatientMembership` disebut hanya untuk menunjuk bukti keadaan saat ini. Bentuk
> fisiknya ditetapkan di `/design-business-module`.

## B. Bahasa Bersama (Ubiquitous Language)

| Istilah | Arti tunggal di modul ini | Bukan |
|---|---|---|
| Membership | Satu periode keanggotaan berbayar milik satu pasien pada satu tingkat | Bukan status pasien. Satu pasien bisa punya banyak membership sepanjang waktu |
| Tingkat (tier) | Silver, Gold, Platinum, atau Executive, beserta persen diskon dan nominal iurannya | "Reguler" bukan tingkat, melainkan ketiadaan membership aktif (`CRM-DEC-006`) |
| Periode | Rentang tanggal kalender WIB, dari tanggal mulai sampai tanggal akhir (inklusif) | Bukan jam UTC |
| Member aktif | Pasien yang pada suatu tanggal WIB punya membership berstatus Aktif dan tanggal itu masuk periodenya | Bukan penanda `IsMember` yang diketik manual |
| Iuran | Uang yang dibayar untuk satu periode membership | Bukan cara bayar tagihan layanan (`CRM-DEC-008`) |
| Manfaat | Diskon per kelompok item dan perlakuan prioritas non-klinis | Tidak pernah mengubah urutan medis (`CRM-DEC-007`) |
| Snapshot tingkat | Tingkat member yang dicatat saat kunjungan didaftarkan dan dipakai sepanjang kunjungan itu | Bukan status member hari ini (`CRM-DEC-005`) |
| Perpanjangan | Membership baru pada pasien yang sama, disambung setelah periode lama (`CRM-DEC-024`) | Bukan pengubahan tanggal akhir membership lama |
| Upgrade | Membership baru pada tingkat lebih tinggi yang langsung mulai saat lunas (`CRM-DEC-012`) | Bukan pengubahan tingkat membership lama |
| Salah input | Pembatalan karena kesalahan petugas, dengan refund bila manfaat belum terpakai (`CRM-DEC-013`) | Bukan berhenti atas permintaan pasien |

## C. Peta Context

| ID | Context | Tanggung jawab | Konsep yang dimiliki | Hubungan |
|---|---|---|---|---|
| `BC-MBR` | **Patient Membership** (Health Services / Patient Management) | Siklus hidup membership, aturan "member aktif", nomor member, riwayat status | `DC-01` s.d. `DC-05`, `DC-08` | Hulu bagi `BC-REG`, `BC-BIL`, `BC-KSK`. Hilir dari `BC-PAT`, `BC-TIER`. Mitra `BC-BIL` untuk iuran (S2) |
| `BC-PAT` | Patient Identity (Patient Management) | Identitas pasien dan No. RM | `DC-06` | Hulu bagi `BC-MBR` |
| `BC-TIER` | Master Tingkat (Administrator / Master Data) | Definisi tingkat, persen diskon, nominal iuran, pengaturan antrean per tingkat | `DC-07` | Hulu bagi `BC-MBR`, `BC-BIL`, `BC-REG` |
| `BC-REG` | Registration & Queue | Pendaftaran kunjungan, antrean, dan snapshot tingkat saat kunjungan didaftarkan | `DC-09` | Hilir dari `BC-MBR` (membaca status) |
| `BC-BIL` | Billing & Cashier | Penerapan diskon pada tagihan. Tagihan, pelunasan, dan refund iuran (menunggu `DEC-CRM-001`) | Diskon terapan, tagihan iuran | Hilir dari `BC-MBR` untuk diskon, hulu bagi `BC-MBR` untuk kabar lunas |
| `BC-KSK` | Kiosk | Pencarian pasien lewat nomor member (baca saja) | — | Hilir dari `BC-MBR` |
| `BC-AUTH` | Platform Authorization | Izin per departemen-posisi | — | Pendukung semua context |

Keputusan ownership: `BC-MBR` tetap berada di Patient Management dengan prefix yang sudah
`ACTIVE` (`CRM-DEC-017`). "CRM" hanya pengelompokan menu di frontend, bukan context tersendiri.

## D. Katalog Konsep Domain

| ID | Nama | Klasifikasi | Pemilik | Ownership | Identitas | Peran / invariant utama | Bukti |
|---|---|---|---|---|---|---|---|
| `DC-01` | Membership | `AGGREGATE_ROOT` | `BC-MBR` | **Extend** (`MstPatientMembership`) | Id internal + nomor member unik buatan sistem | Seluruh siklus hidup. `INV-01` s.d. `INV-11` | `CRM-DEC-003`, `004`, `012`, `017`, `022`, `023`; `CRM-CAP-002` |
| `DC-02` | Periode | `VALUE_OBJECT` | `BC-MBR` | Extend (kini `JoinDate`/`ExpiredDate` bebas isi) | — | Tanggal mulai ≤ tanggal akhir, keduanya tanggal kalender WIB. Ditetapkan sistem, tidak diketik petugas | `CRM-DEC-005`, `024`; `CRM-GAP-008` |
| `DC-03` | Status membership | `REFERENCE_DATA` | `BC-MBR` | **Extend** (`MembershipStatus`) | — | Tambah Menunggu Pembayaran, Terjadwal, Perlu Verifikasi. Suspended/Downgraded tidak dipakai di MVP (`CRM-OQ-002`) | `CRM-DEC-004`, `021`, `024`; `CRM-FACT-004` |
| `DC-04` | Riwayat status | `ENTITY` (di dalam `DC-01`) | `BC-MBR` | **New** | Urutan per membership | Setiap perpindahan status tercatat: dari, ke, pelaku, waktu, alasan, sumber (petugas/sistem/kasir) | `CRM-DEC-013`, `021`, `022`; `CRM-CONF-008` |
| `DC-05` | Alasan pembatalan | `VALUE_OBJECT` | `BC-MBR` | New | — | Kategori: permintaan pasien, salah input, tidak dibayar, digantikan upgrade (untuk Terjadwal), ditambah teks alasan wajib untuk aksi petugas | `CRM-DEC-013`, `014`, `025` |
| `DC-06` | Pasien | `REFERENCE_DATA` (lintas modul) | `BC-PAT` | **Existing** (`MstPatient`) | No. RM | Membership hanya untuk pasien aktif ber-No. RM | `CRM-DEC-003` |
| `DC-07` | Tingkat | `REFERENCE_DATA` | `BC-TIER` | **Extend** (`MstMembershipTier`): tambah nominal iuran | Kode tingkat | Hanya Silver/Gold/Platinum/Executive yang boleh dipilih saat pendaftaran | `CRM-DEC-006`, `015`, `027`; `CRM-CAP-001` |
| `DC-08` | Penentu member aktif | Aturan domain (layanan baca) | `BC-MBR` | **New** sebagai satu-satunya aturan | — | Satu aturan dipakai semua konsumen (lihat bagian J) | `CRM-CAP-004`, `CRM-CAP-016`; `CRM-DEC-005` |
| `DC-09` | Snapshot tingkat kunjungan | `VALUE_OBJECT` milik kunjungan | `BC-REG` | **Existing** untuk antrean RJ (`RegQueue.*Snapshot`). Lokasi untuk IGD/rawat inap ditetapkan saat desain (`AG-01`) | — | Ditulis sekali saat kunjungan didaftarkan, tidak ikut berubah bila status member berubah | `CRM-DEC-005`, `028` |
| `DC-10` | Proyeksi status di pasien | `Adapter/View` | `BC-MBR` (menulis), `BC-PAT` (menyimpan) | **Adapter/View** (`MstPatient.IsMember`, `DefaultMembershipTierId`, `ActivePatientMembershipId`) | — | Bukan sumber kebenaran. Hanya boleh diisi dari `DC-08` dan tidak boleh diketik lewat formulir pasien | `CRM-CAP-004` |
| `DC-11` | Tagihan iuran | `EXTERNAL_CONTRACT` | `BC-BIL` | Menunggu `DEC-CRM-001` | — | Satu tagihan iuran per membership yang Menunggu Pembayaran | `CRM-DEC-019` |
| `DC-12` | Kejadian domain membership | `DOMAIN_EVENT` | `BC-MBR` | New | — | Lihat bagian E | — |

Tidak dimodelkan sebagai konsep terpisah: "kartu member" (`CRM-OQ-004`, ditunda), wallet
(`CRM-DEC-009`), poin dan total belanja (tidak ada keputusan, sehingga kolom yang sudah ada
dibiarkan dan tidak dipakai aturan).

## E. Model Aggregate — `DC-01` Membership

**Batas aggregate:** satu membership beserta periode, status, alasan, dan riwayat statusnya.
Aturan yang melibatkan **beberapa membership milik satu pasien** (`INV-01` s.d. `INV-03`)
dijaga pada tingkat pasien. Konsekuensinya, desain harus menyediakan penguncian per pasien
atau batasan unik bersyarat, agar dua petugas yang menyimpan bersamaan tidak lolos keduanya.

### Invariant

| ID | Invariant | Contoh pelanggaran yang harus ditolak | Dasar |
|---|---|---|---|
| `INV-01` | Paling banyak **satu** membership Aktif per pasien | Silver dan Gold aktif bersamaan | `CRM-DEC-012` |
| `INV-02` | Paling banyak **satu** membership Terjadwal per pasien. Hanya boleh ada bila ada membership Aktif, dan tanggal mulainya = tanggal akhir yang Aktif + 1 hari | Terjadwal mulai 20 Okt padahal Aktif berakhir 12 Okt | `CRM-DEC-024` |
| `INV-03` | Paling banyak **satu** membership Menunggu Pembayaran per pasien (baru, perpanjangan, atau upgrade) | Dua pendaftaran Gold menunggu bayar untuk pasien yang sama | `CRM-DEC-012`; `CRM-GAP-009` |
| `INV-04` | Setelah Aktif atau Terjadwal: pasien, tingkat, dan periode tidak berubah. Hanya catatan yang boleh diubah | Memindahkan Gold dari Dika ke Ibu Sari | `CRM-DEC-022` |
| `INV-05` | Tingkat harus tingkat berbayar yang aktif dan boleh dipilih | Mendaftarkan "Corporate" atau "Regular" | `CRM-DEC-006` |
| `INV-06` | Pasien harus aktif dan ber-No. RM | Pasien yang sudah dihapus | `CRM-DEC-003` |
| `INV-07` | Nomor member dibuat sistem, unik, dan tidak dipakai ulang | Petugas mengetik nomor sendiri | `CRM-DEC-023` |
| `INV-08` | Status hanya berpindah sesuai tabel G | Kedaluwarsa kembali menjadi Aktif | Semua keputusan status |
| `INV-09` | Pembatalan oleh petugas wajib beralasan. Refund hanya untuk salah input tanpa manfaat terpakai, atau untuk Terjadwal | Refund atas permintaan pasien saat Aktif | `CRM-DEC-013`, `025` |
| `INV-10` | Perpanjangan hanya dalam jendela N hari sebelum tanggal akhir (N bawaan 30, dapat diatur) | Perpanjangan 40 hari sebelum habis | `CRM-DEC-024` |
| `INV-11` | Penentuan "aktif pada tanggal X" memakai tanggal kalender WIB | Pukul 23.30 WIB di hari terakhir dianggap sudah habis | `CRM-DEC-005`; `CRM-GAP-008` |

### Tindakan bisnis dan kejadian yang diterbitkan

| Tindakan | Pelaku | Hasil | Kejadian |
|---|---|---|---|
| Daftarkan membership baru | Petugas CRM | Menunggu Pembayaran, nomor member dibuat | `MembershipRegistered` → meminta tagihan iuran (S2) |
| Daftarkan perpanjangan | Petugas CRM | Menunggu Pembayaran (jenis: perpanjangan) | `MembershipRenewalRegistered` |
| Daftarkan upgrade | Petugas CRM | Menunggu Pembayaran (jenis: upgrade) | `MembershipUpgradeRegistered` |
| Terima kabar iuran lunas | Billing (S2) | Baru/upgrade: Aktif. Perpanjangan: Terjadwal | `MembershipActivated` / `MembershipScheduled` (+ `MembershipUpgraded` untuk yang lama, dan pembatalan Terjadwal bila ada) |
| Batal otomatis tidak dibayar | Sistem | Cancelled (tidak dibayar) | `MembershipPaymentExpired` → meminta pembatalan tagihan (S2) |
| Mulai periode Terjadwal | Sistem | Aktif | `MembershipActivated` |
| Kedaluwarsa | Sistem | Kedaluwarsa | `MembershipExpired` |
| Batalkan (permintaan pasien) | Petugas CRM | Cancelled tanpa refund | `MembershipCancelled` |
| Batalkan (salah input) | Petugas CRM | Cancelled, refund bila manfaat belum terpakai | `MembershipCancelled` → meminta refund (S2) |
| Tandai data lama | Sistem (sekali, saat aturan dipasang) | Perlu Verifikasi | `MembershipVerificationRequired` |
| Akui / batalkan data lama | Petugas CRM | Aktif dengan periode sah, atau Cancelled | `MembershipVerified` / `MembershipCancelled` |

Setiap kejadian di atas juga menulis satu baris riwayat status (`DC-04`).

## F. Model Relasi

| Sumber | Tujuan | Makna | Kardinalitas | Wajib? | Arah ownership | Hapus/arsip |
|---|---|---|---|---|---|---|
| Membership | Pasien | Membership dimiliki satu pasien | N : 1 | Wajib | `BC-MBR` merujuk `BC-PAT` | Membership tidak pernah dihapus fisik. Pasien yang dinonaktifkan tidak menghapus riwayat |
| Membership | Tingkat | Membership berada pada satu tingkat | N : 1 | Wajib | `BC-MBR` merujuk `BC-TIER` | Tingkat yang dinonaktifkan tidak mengubah membership yang sudah ada |
| Membership | Membership (yang digantikan) | Upgrade/perpanjangan menunjuk membership sebelumnya | 0..1 : 0..1 | Opsional | Di dalam `BC-MBR` | Rujukan bersifat historis |
| Membership | Riwayat status | Jejak perpindahan status | 1 : N | Minimal satu baris | Bagian dari aggregate | Ikut tersimpan selamanya |
| Membership | Tagihan iuran | Iuran untuk membership ini | 1 : 0..1 aktif | Wajib sebelum Aktif/Terjadwal (kecuali hasil verifikasi data lama) | `BC-BIL` merujuk `BC-MBR` | Mengikuti aturan billing (S2) |
| Kunjungan | Membership | Snapshot tingkat saat kunjungan didaftarkan | N : 0..1 | Opsional (Reguler = kosong) | `BC-REG` merujuk `BC-MBR` | Snapshot tidak berubah |
| Pasien (proyeksi) | Membership aktif | Penunjuk cepat ke membership aktif | 1 : 0..1 | Opsional | Ditulis `BC-MBR` | Diperbarui setiap perpindahan status |

## G. Model Lifecycle / Status

Status awal: **Menunggu Pembayaran**, atau **Perlu Verifikasi** khusus data lama.
Status akhir: **Kedaluwarsa**, **Upgraded**, **Cancelled**.

| Dari | Tindakan | Ke | Wewenang | Prasyarat / invariant | Audit |
|---|---|---|---|---|---|
| — | Daftar baru | Menunggu Pembayaran | Petugas CRM | `INV-03`, `INV-05`, `INV-06`. Pasien tidak punya Aktif (baru) | Riwayat + pelaku |
| — | Daftar perpanjangan | Menunggu Pembayaran | Petugas CRM | Ada Aktif, dalam jendela `INV-10`, belum ada Terjadwal | Riwayat |
| — | Daftar upgrade | Menunggu Pembayaran | Petugas CRM | Ada Aktif, tingkat baru lebih tinggi (urutan tingkat: lihat `AG-04`) | Riwayat |
| Menunggu Pembayaran | Iuran lunas (baru/upgrade) | Aktif | Kabar lunas dari billing | Periode = tanggal lunas s.d. +12 bulan − 1 hari | Riwayat + rujukan tagihan |
| Menunggu Pembayaran | Iuran lunas (perpanjangan) | Terjadwal | Kabar lunas dari billing | `INV-02` | Riwayat + rujukan tagihan |
| Menunggu Pembayaran | 7 hari tanpa lunas | Cancelled (tidak dibayar) | Sistem | — | Riwayat, sumber = sistem |
| Aktif | Tanggal akhir lewat | Kedaluwarsa | Sistem | — | Riwayat |
| Aktif | Upgrade lunas | Upgraded | Kabar lunas dari billing | Bersamaan dengan Aktif yang baru | Riwayat + rujukan pengganti |
| Aktif | Berhenti (pasien) | Cancelled | Petugas CRM | Alasan wajib, tanpa refund | Riwayat + alasan |
| Aktif | Salah input | Cancelled | Petugas CRM | Alasan wajib. Refund hanya bila manfaat belum terpakai (cek ke billing) | Riwayat + alasan + rujukan refund |
| Terjadwal | Periode lama berakhir | Aktif | Sistem | — | Riwayat |
| Terjadwal | Upgrade lunas / berhenti / salah input | Cancelled, refund penuh | Sistem (upgrade) / Petugas CRM | Alasan wajib untuk aksi petugas | Riwayat + rujukan refund |
| Perlu Verifikasi | Diakui | Aktif | Petugas CRM | Periode sah diisi, tidak melanggar `INV-01` | Riwayat + alasan |
| Perlu Verifikasi | Ditolak | Cancelled | Petugas CRM | Alasan wajib | Riwayat + alasan |

Perpindahan yang tidak ada di tabel ditolak, misalnya Kedaluwarsa ke Aktif atau Cancelled ke
Aktif. Pembukaan ulang (reopen) tidak ada. Yang ada adalah pendaftaran baru.

**Contoh perjalanan lengkap:** Ibu Sari didaftarkan Gold pada 13 Okt 2026 dan berstatus
Menunggu Pembayaran. Iuran lunas di hari yang sama, sehingga Aktif 13 Okt 2026 s.d. 12 Okt
2027. Pada 1 Okt 2027 ia memperpanjang dan lunas, sehingga perpanjangan berstatus Terjadwal
untuk 13 Okt 2027 s.d. 12 Okt 2028. Pada 13 Okt 2027 pukul 00.00 WIB, Gold lama menjadi
Kedaluwarsa dan yang Terjadwal menjadi Aktif.

## H. Tanggung Jawab Authorization

| Tindakan | Siapa | Dasar |
|---|---|---|
| Mendaftarkan (baru, perpanjangan, upgrade), membatalkan, memverifikasi data lama, mengubah catatan | Petugas CRM/Marketing dengan izin membership yang sesuai, diberikan lewat Role Access per departemen-posisi | `CRM-DEC-011`, `018`; `CRM-GAP-011` |
| Melihat daftar dan detail membership | Petugas CRM | `CRM-DEC-011` |
| Melihat status member pasien (baca saja) | Admisi RJ/IGD, nurse station, dokter, kasir, sesuai izin modul masing-masing | `CRM-DEC-011` |
| Mencari pasien lewat nomor member | Akun kiosk (baca saja) | `CRM-DEC-018` |
| Membuat membership dari kiosk | **Tidak ada yang boleh** | `CRM-DEC-018` |
| Mengaktifkan setelah lunas | Hanya lewat kabar lunas billing, bukan tombol petugas | `CRM-DEC-004`, `019` |
| Kedaluwarsa, batal otomatis, mulai Terjadwal | Sistem terjadwal | `CRM-DEC-005`, `014`, `024` |
| Menyetujui refund iuran | **Belum diputuskan** (pertanyaan 4 di permintaan billing) | `DEC-CRM-001` |

String izin persisnya ditetapkan di desain. Arsitektur ini tidak menetapkan departemen atau
posisi tertentu.

## I. Model Audit / Histori

| Kejadian | Yang harus tercatat | Dasar |
|---|---|---|
| Setiap perpindahan status | Status lama, status baru, pelaku (pengguna atau "sistem"), waktu, alasan, sumber (petugas/kasir/sistem), rujukan tagihan atau refund bila ada | `CRM-DEC-013`, `022` |
| Perubahan catatan | Nilai lama, nilai baru, pelaku, waktu | `CRM-DEC-022` |
| Verifikasi data lama | Keputusan (diakui/ditolak), periode yang diakui, alasan, pelaku | `CRM-DEC-021` |
| Diskon dicabut atau diterapkan karena penjamin berubah | Dicatat oleh billing: penjamin lama dan baru, nilai diskon, waktu | `CRM-DEC-026` (milik `BC-BIL`) |

Catatan keadaan saat ini: logger aplikasi tidak menyimpan objek data tambahan. Jejak audit
yang wajib harus berupa data riwayat (`DC-04`), bukan log.

## J. Model Integrasi

### J1. Satu aturan "member aktif" (menutup `CRM-CAP-016`)

| Aspek | Ketentuan |
|---|---|
| Pemilik aturan | `BC-MBR` (`DC-08`) |
| Isi aturan | Pada tanggal WIB `X`, pasien adalah member aktif bila ada membership berstatus Aktif dengan `mulai ≤ X ≤ akhir` dan tingkatnya aktif. Selain itu, pasien adalah Reguler |
| Konsumen | Pendaftaran RJ/IGD/rawat inap (snapshot), klasifikasi antrean RJ (`OutpatientQueueClassificationService`), nurse station, kiosk, billing |
| Keadaan saat ini | Dua aturan berbeda: proyeksi pasien (`IsActive && IsPrimary`) dan klasifikasi antrean (status + tanggal). Keduanya diselaraskan ke aturan tunggal ini. Klasifikasi antrean menjadi konsumen, bukan pemilik aturan |
| Proyeksi di pasien (`DC-10`) | Boleh dipertahankan untuk tampilan cepat, tetapi hanya ditulis oleh `BC-MBR` saat status berubah dan oleh proses terjadwal harian. Tidak boleh diketik lewat formulir pasien |
| Contoh | Gold berakhir 12 Okt 2027. Pada 13 Okt 2027, semua konsumen menjawab "Reguler". Tidak ada lagi nurse station yang masih menampilkan Gold |

### J2. Snapshot tingkat saat kunjungan didaftarkan

| Aspek | Ketentuan |
|---|---|
| Produsen | `BC-MBR` (aturan J1) |
| Konsumen dan pemilik snapshot | `BC-REG`, saat kunjungan RJ, IGD, atau rawat inap didaftarkan |
| Arah | Baca sinkron saat pendaftaran |
| Kegagalan | Bila penentuan status gagal, kunjungan tetap didaftarkan sebagai Reguler dan kejadian itu tercatat. Pelayanan pasien tidak boleh tertahan oleh membership |
| Celah desain | `AG-01`: snapshot RJ sudah ada di antrean. Lokasi snapshot untuk IGD dan rawat inap belum ada |

### J3. Manfaat diskon ke billing (S4)

| Aspek | Ketentuan |
|---|---|
| Produsen aturan manfaat | `BC-TIER` (persen per kelompok) + `BC-MBR` (snapshot dan aturan) |
| Pelaksana | `BC-BIL` |
| Aturan yang wajib diterapkan billing | Hanya kunjungan pribadi sepenuhnya (`CRM-DEC-016`), semua jenis kunjungan (`CRM-DEC-028`), dihitung ulang mengikuti penjamin saat final (`CRM-DEC-026`), kelompok mengikuti asal item (`CRM-DEC-027`), tingkat dari snapshot kunjungan (`CRM-DEC-005`) |
| Kebutuhan balik | `BC-MBR` perlu tahu apakah manfaat sebuah membership **sudah terpakai** (ada diskon pada tagihan final) untuk aturan refund salah input (`INV-09`) |
| Ketergantungan | Persetujuan owner billing (pertanyaan 5 di permintaan amendment) |

### J4. Iuran (S2, kontrak saja, menunggu `DEC-CRM-001`)

| Arah | Kontrak logis | Idempotency / kegagalan |
|---|---|---|
| `BC-MBR` → `BC-BIL` | "Buat tagihan iuran" untuk satu membership Menunggu Pembayaran | Satu membership paling banyak satu tagihan aktif. Permintaan ulang tidak membuat tagihan kedua |
| `BC-BIL` → `BC-MBR` | "Iuran lunas" (rujukan membership, rujukan tagihan, waktu lunas) | Kabar yang sama diterima dua kali tidak mengaktifkan dua kali. Kabar untuk membership yang sudah batal (lewat 7 hari) ditolak dan dikembalikan ke billing untuk diproses sebagai pengembalian |
| `BC-MBR` → `BC-BIL` | "Batalkan tagihan iuran" saat batal otomatis | Batal yang sudah terjadi tidak diulang |
| `BC-MBR` → `BC-BIL` | "Minta refund iuran" (salah input tanpa manfaat terpakai, atau Terjadwal dibatalkan) | Satu refund per membership |
| Rekonsiliasi | Membership Menunggu Pembayaran yang tagihannya sudah lunas, atau sebaliknya, harus dapat dideteksi | Bentuknya ditetapkan setelah `DEC-CRM-001` |

### J5. Waktu (proses terjadwal)

Tiga pekerjaan berbasis tanggal WIB: kedaluwarsa, mulai periode Terjadwal, dan batal otomatis
7 hari. Ketiganya harus aman bila dijalankan ulang (tidak menggandakan riwayat), dan harus
dapat mengejar ketertinggalan bila server mati semalam.

## K. Dampak Billing

Klasifikasi: **berdampak pada charge**, dengan satu **dependency billing yang belum
terselesaikan**.

| Area | Status |
|---|---|
| Diskon member pada tagihan layanan | Aturan lengkap dan dikonfirmasi (`CRM-DEC-008`, `016`, `026`, `027`, `028`). Pelaksanaannya milik `BC-BIL` dan menunggu persetujuan owner billing |
| Membership bukan cara bayar | Sudah sesuai di source (`CRM-CAP-010`) |
| Tagihan, pelunasan, dan refund iuran | `DEC-CRM-001` terbuka: bentuk tagihan, perlakuan akuntansi (termasuk iuran dibayar di muka hingga 30 hari), dan penyetuju refund |

Arsitektur ini tidak menetapkan tarif, jurnal, atau perlakuan pendapatan.

## L. Dampak Keselamatan Klinis

Klasifikasi: **non-klinis**, dengan satu batasan keselamatan yang mengikat konsumen.

`CRM-DEC-007`: status member tidak boleh dipakai untuk mengurutkan penanganan medis (triage
IGD, urutan tindakan). Antrean RJ boleh memakai status member hanya untuk antrean
administratif, tidak untuk menggeser prioritas klinis. Penentuan "member aktif" (J1) tidak
membawa informasi klinis apa pun.

## M. Celah Arsitektur dan Keputusan Terbuka

| ID | Isu | Jenis | Memblokir |
|---|---|---|---|
| `DEC-CRM-001` | Tagihan iuran, akuntansi, penyetuju refund, dan persetujuan penerapan diskon di billing | Keputusan bisnis (owner billing + Finance) | Slice S2. S4 hanya untuk pelaksanaan di billing |
| `AG-01` | Lokasi snapshot tingkat untuk kunjungan IGD dan rawat inap (`DC-09`) | Desain, wewenang `BC-REG` | Tidak memblokir arsitektur. Diputuskan di `/design-business-module` bersama owner pendaftaran |
| `AG-02` | Cara `BC-MBR` mengetahui "manfaat sudah terpakai" (J3) | Desain kontrak dengan billing | Hanya refund salah input, ikut menunggu S2 |
| `AG-03` | Penyelarasan `OutpatientQueueClassificationService` (milik rawat jalan) ke aturan tunggal J1 | Amendment lintas modul (`rawat-jalan`) | Tidak memblokir. Perilakunya saat ini sudah dekat dengan aturan target |
| `AG-04` | Urutan "lebih tinggi" antar tingkat untuk upgrade. Usulan (`PROPOSED`): Silver < Gold < Platinum < Executive mengikuti `PriorityLevel` di master. Turun tingkat di tengah periode tidak tersedia (`CRM-OQ-002`) | **Ditutup** oleh `CRM-DEC-029` | — |
| `CRM-OQ-010` | Pengenalan item biaya pendaftaran untuk kelompok diskon pendaftaran | Fakta source | Tidak memblokir |

## N. Kesiapan Arsitektur

**`DOMAIN_ARCHITECTURE_PARTIAL`**

| Slice | Kesiapan | Boleh ke `/design-business-module`? |
|---|---|---|
| `S1a` siklus hidup | Siap | Ya. Aktivasi dirancang sebagai penerima kontrak "iuran lunas" (J4) |
| `S1b` perpanjangan Terjadwal | Siap | Ya. Refund Terjadwal ikut S2 |
| `S3` aturan member aktif dan pencarian | Siap | Ya |
| `S4` aturan manfaat diskon | Siap di sisi membership (J3) | Ya, untuk kontrak manfaat dan snapshot. Pelaksanaan di billing menunggu owner billing |
| `S5` otorisasi | Siap | Ya |
| `S2` tagihan iuran | **Terblokir** (`DEC-CRM-001`) | Tidak. Hanya kontrak logis J4 yang dibawa |

Slice siap tidak bergantung pada bentuk teknis S2: membership tetap Menunggu Pembayaran sampai
kontrak "iuran lunas" diterima, apa pun bentuk tagihannya nanti. Karena itu kelima slice
boleh lanjut tanpa menunggu S2.
