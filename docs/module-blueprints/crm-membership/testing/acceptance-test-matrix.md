# Matriks Uji Penerimaan — CRM Membership

| Field | Nilai |
|---|---|
| Revision | `1`, status `draft` |
| Masukan | Acceptance criteria 1–22 di `00-interview-decisions.md`, `contracts/*` v1 |

Jenis uji: **Unit** = service dengan harness InMemory, **Integrasi** = endpoint + Npgsql lokal,
**E2E** = browser terhadap frontend, **Manual** = UAT petugas.

| ID | Requirement | Skenario | Jalur | Jenis | Bukti yang diharapkan |
|---|---|---|---|---|---|
| `AT-01` | AC-1, `VAL-01` | Daftar membership untuk pasien yang tidak ada | Gagal | Integrasi | 404, tidak ada baris baru |
| `AT-02` | AC-2 | Daftar Gold untuk pasien ber-RM | Berhasil | Integrasi | Status `PendingPayment`, nomor member terbit, `PaymentDueAt` = simpan + 7 hari, riwayat `REGISTERED` |
| `AT-03` | AC-2, J1 | Pasien dengan membership Menunggu Pembayaran dicek resolver | Berhasil | Unit | Resolver menjawab Reguler |
| `AT-04` | AC-3 | `ConfirmFeePaidAsync` pada membership Menunggu | Berhasil | Unit | Aktif, periode = tanggal lunas s.d. +12 bulan − 1 hari, proyeksi pasien terisi |
| `AT-05` | `VAL-21` | Kabar lunas yang sama dikirim dua kali | Gagal (diabaikan) | Unit | Satu baris riwayat `FEE_PAID` saja |
| `AT-06` | AC-4 | Waktu digeser melewati `PaymentDueAt`, proses terjadwal jalan | Berhasil | Unit | `Cancelled` (`Unpaid`), riwayat sumber `System` |
| `AT-07` | `VAL-20` | Kabar lunas datang setelah batal otomatis | Gagal | Unit | 409, status tetap `Cancelled` |
| `AT-08` | AC-5 | Hari setelah tanggal akhir, proses terjadwal jalan | Berhasil | Unit | `Expired`, resolver menjawab Reguler, proyeksi pasien kosong |
| `AT-09` | AC-5, `INV-11` | Pukul 23.30 WIB di hari terakhir | Berhasil | Unit | Resolver masih menjawab Aktif |
| `AT-10` | AC-5, `CRM-DEC-005` | Kunjungan terdaftar sebelum kedaluwarsa, dilayani sesudahnya | Berhasil | Integrasi | Snapshot kunjungan tetap tingkat lama |
| `AT-11` | AC-6, `VAL-04` | Daftar baru untuk pasien yang masih Aktif | Gagal | Integrasi | 409 dengan pesan menyarankan perpanjangan/upgrade |
| `AT-12` | `INV-03` | Dua permintaan daftar bersamaan untuk pasien yang sama | Gagal (satu) | Integrasi (Npgsql) | Satu sukses, satu 409. Tepat satu baris Menunggu |
| `AT-13` | AC-7 | Upgrade Silver ke Gold, lalu iuran lunas | Berhasil | Unit | Silver `Upgraded`, Gold Aktif mulai tanggal lunas |
| `AT-14` | `VAL-09` | Upgrade Gold ke Silver | Gagal | Integrasi | 409 |
| `AT-15` | AC-8, `VAL-10` | Batal tanpa alasan | Gagal | Integrasi | 400 |
| `AT-16` | AC-8 | Batal permintaan pasien | Berhasil | Integrasi | `Cancelled` (`PatientRequest`), tanpa permintaan refund |
| `AT-17` | `VAL-12` | Batal salah input setelah diskon terpakai | Gagal | Integrasi (setelah S2) | 409 |
| `AT-18` | AC-9 | Lookup No. Telp yang dipakai dua pasien | Berhasil | Integrasi | Dua baris dengan status masing-masing, tanpa pilihan otomatis |
| `AT-19` | AC-10 | Akun admisi tanpa `PatientMembership : Create` memanggil `POST /admin` | Gagal | Integrasi | 403 |
| `AT-20` | AC-11, `VAL-02` | Daftar dengan tingkat Corporate | Gagal | Integrasi | 400 |
| `AT-21` | AC-12, AC-21 | Kunjungan pribadi member Gold lalu asuransi ditambahkan sebelum final | Berhasil | Integrasi (billing, setelah persetujuan) | Diskon dicabut, riwayat diskon tercatat |
| `AT-22` | AC-13 | Akun kiosk memanggil `POST /kiosk` | Gagal | Integrasi | 404 |
| `AT-23` | AC-13 | Kiosk mencari nomor member Aktif | Berhasil | Integrasi | Pasien ditemukan |
| `AT-24` | AC-14 | Petugas CRM (bukan Administrator) dengan izin Create membuka daftar dan mendaftar | Berhasil | E2E | Halaman tampil dan pendaftaran sukses |
| `AT-25` | AC-15, `VAL-18` | Klien lama memanggil `PUT /admin/{id}` | Gagal | Integrasi | 410 |
| `AT-26` | AC-16 | Request daftar membawa `memberNumber` "ABC" | Berhasil (diabaikan) | Integrasi | Nomor member dari sistem, bukan "ABC" |
| `AT-27` | AC-17 | Migration data lama | Berhasil | Integrasi (lokal) | Semua baris lama `NeedsVerification`, proyeksi pasien kosong, riwayat `LEGACY_FLAGGED` |
| `AT-28` | `VAL-15` | Akui data lama untuk pasien yang sudah punya Aktif | Gagal | Integrasi | 409 |
| `AT-29` | AC-18 | Perpanjangan Gold setelah kedaluwarsa (daftar baru tingkat sama) | Berhasil | Integrasi | Diterima. Tidak ada penolakan "tier yang sama" |
| `AT-30` | AC-19, `VAL-07` | Perpanjangan 40 hari sebelum habis | Gagal | Integrasi | 409 dengan tanggal buka jendela |
| `AT-31` | AC-19 | Perpanjangan 11 hari sebelum habis lalu lunas | Berhasil | Unit | `Scheduled` dengan periode tersambung, lalu Aktif otomatis pada hari H |
| `AT-32` | AC-20 | Upgrade saat ada Terjadwal | Berhasil | Unit | Terjadwal `Cancelled` (`SupersededByUpgrade`) + permintaan refund |
| `AT-33` | AC-22 | Tagihan pribadi member Gold berisi konsultasi, obat, item ad-hoc | Berhasil | Integrasi (billing) | Diskon hanya pada konsultasi dan obat |
| `AT-34` | `CRM-DEC-028` | Rawat inap pribadi member Gold berisi obat dan kamar | Berhasil | Integrasi (billing) | Obat didiskon, kamar tidak |
| `AT-35` | `VAL-17` | Dua petugas membatalkan membership yang sama hampir bersamaan | Gagal (satu) | Integrasi | Satu sukses, satu 409 "baru saja diubah" |
| `AT-36` | `CRM-CAP-016` | Pasien kedaluwarsa dilihat di nurse station dan antrean RJ | Berhasil | Integrasi | Keduanya menampilkan Reguler |
| `AT-37` | `CRM-DEC-007` | Pasien Platinum triage Hijau dan pasien Reguler triage Merah di IGD | Berhasil | Manual | Urutan IGD tetap Merah lebih dulu |
