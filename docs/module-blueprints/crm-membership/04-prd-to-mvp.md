# CRM Membership — PRD ke MVP

## 1. Identitas Dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian V2 |
| Modul | CRM Membership (kode `CRM`), di bawah Health Services / Patient Management |
| Status | `approved` oleh Yoga Aji Pratama, 2026-10-13 (`CRM-DEC-030`), untuk `MVP-0` s.d. `MVP-2`. `EPIC CRM-08`/`09` tetap `OPEN DECISION` |
| Owner | Yoga Aji Pratama |
| Repository target | `NewQuilvianSystemBackend` (`1aecf642`), `QuilvianSystemFrontendDev` (`f1e5e273a`) |
| Input | `02-backend-architecture.md` r1, `03-frontend-architecture.md` r1, `03-domain-architecture.md` r1 (`DOMAIN_ARCHITECTURE_PARTIAL`), `contracts/*` v1/v2, `erd/*` r1 |
| Cakupan | Pendaftaran, siklus hidup, dan status member pasien prioritas yang dibaca seluruh titik layanan |

## 2. Ringkasan Eksekutif

Rumah sakit menjual membership Silver, Gold, Platinum, dan Executive. Petugas CRM perlu
mendaftarkan member, melihat daftarnya, dan memastikan setiap titik layanan mengenali pasien
itu sebagai member **hanya selama** iurannya lunas dan masa berlakunya berjalan. Rilis pertama
memperbaiki data membership yang sudah ada agar aturannya benar, menambah pendaftaran yang
terkendali, dan membuat status member konsisten di seluruh aplikasi.

## 3. Masalah Produk

| Kondisi sekarang | Bukti |
|---|---|
| Data membership per pasien sudah ada, tetapi status dan tanggal diketik bebas | `CRM-CAP-002`, `CRM-CAP-003` |
| Membership tidak pernah kedaluwarsa, dan satu pasien bisa punya dua yang aktif | `CRM-CONF-004`, `CRM-CONF-005` |
| Akun kiosk bisa membuat membership. Petugas Marketing justru ditolak | `CRM-CONF-007` |
| Dua aturan "member aktif" yang berbeda (nurse station vs antrean RJ) | `CRM-CAP-016` |
| Kasir tidak bisa menagih iuran karena setiap tagihan wajib terikat kunjungan | `CRM-CAP-008` |
| Persen diskon per tingkat tersimpan tetapi tidak dipakai billing | `CRM-CAP-009` |
| Tidak ada menu CRM | `CRM-CAP-007` |

## 4. Visi Produk

1. Petugas CRM mencari pasien lewat No. RM, NIK, atau No. Telp, lalu memilih satu orang.
2. Petugas memilih tingkat. Sistem membuat nomor member dan pendaftaran Menunggu Pembayaran.
3. Pasien membayar iuran di kasir, dan membership Aktif selama 12 bulan.
4. Saat pasien berkunjung, pendaftaran mencatat tingkatnya. Antrean, nurse station, dan kasir membaca jawaban yang sama.
5. Tagihan kunjungan pribadi mendapat diskon sesuai tingkat.
6. Mendekati akhir, pasien memperpanjang dan periodenya tersambung tanpa jeda.
7. Hari setelah berakhir, pasien otomatis kembali Reguler.

## 5. Batas MVP

**Titik mulai:**

1. Pasien sudah terdaftar di master pasien (ber-No. RM).
2. Master tingkat berisi empat tingkat berbayar lengkap dengan nominal iuran dan persen diskon.
3. Petugas CRM punya izin `PatientMembership : *`.

**Titik akhir:**

1. Membership tercatat dengan status benar dan riwayat lengkap, dari pendaftaran sampai Kedaluwarsa, Di-upgrade, atau Dibatalkan.
2. Status member pasien terbaca sama di antrean RJ, nurse station, kiosk, dan layar CRM.
3. Membership lama sudah diverifikasi CRM.
4. *(Bergantung `DEC-CRM-001`)* Iuran dibayar di kasir dan membership aktif otomatis.
5. *(Bergantung persetujuan billing)* Diskon member diterapkan pada tagihan pribadi.

## 6. Pelaku Sasaran

| Pelaku | Tanggung jawab di MVP |
|---|---|
| Petugas CRM/Marketing | Mendaftarkan, memperpanjang, upgrade, membatalkan, memverifikasi data lama |
| Kasir | Menerima iuran (setelah `DEC-CRM-001`) |
| Admisi RJ/IGD | Melihat status member saat mendaftarkan kunjungan |
| Perawat nurse station | Melihat status member yang benar |
| Sistem terjadwal | Kedaluwarsa, mulai Terjadwal, batal tidak dibayar |

## 7. Pemilihan Kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Data membership dengan siklus hidup benar | `CRM-CAP-002`, `CRM-CAP-003` | Wajib. Tanpa ini status member salah |
| Satu aturan member aktif + proyeksi | `CRM-CAP-004`, `CRM-CAP-016` | Wajib. Tanpa ini layanan berbeda jawaban |
| Pencarian pasien dengan status member | `CRM-CAP-005` | Wajib. Langkah pertama pendaftaran |
| Otorisasi CRM dan penutupan pembuatan via kiosk | `CRM-CAP-006` | Wajib. Celah diskon |
| Halaman CRM | `CRM-CAP-007` | Wajib. Tanpa layar tidak ada yang mendaftar |
| Master tingkat dengan iuran | `CRM-CAP-001` | Wajib. Pendaftaran ditolak tanpa iuran |
| Tagihan iuran dan aktivasi | `CRM-CAP-008` | Wajib, tetapi **`OPEN DECISION`** (`DEC-CRM-001`). Tanpa ini membership baru tidak pernah Aktif, dan tidak ada jalan sementara yang diizinkan (`CRM-DEC-019` melarang bukti bayar manual) |
| Diskon di billing | `CRM-CAP-009` | Wajib, tetapi **`OPEN DECISION`** (persetujuan owner billing) |

## 8. Kemampuan yang Ditunda

| Kemampuan | ID asal | Alasan bersebab | Pengganti selama MVP |
|---|---|---|---|
| Wallet / prepaid | — | Uang titipan pasien butuh aturan akuntansi Finance (`CRM-DEC-009`) | Cara bayar biasa di kasir |
| Pengingat WA | — | Pemilik layanan WhatsApp belum diketahui (`CRM-OQ-003`) | CRM melihat filter "berakhir 30 hari lagi" di Daftar Membership |
| Kartu member | — | Bentuk cetak/digital belum diputuskan (`CRM-OQ-004`) | Nomor member di detail dan lookup |
| Suspended / turun tingkat | — | Belum ada keputusan (`CRM-OQ-002`) | Batal lalu daftar baru pada tingkat lain |
| Member keluarga / perusahaan | — | Di luar `CRM-DEC-006` | Daftar per pasien |
| Antrean kelipatan 5, reservasi tanpa poli, link konfirmasi | — | Milik rawat jalan/kiosk (`CRM-DEC-001`) | Antrean prioritas yang sudah ada (`CRM-CAP-011`) |

## 9. Alur Bisnis Target — `FLOW-CRM-MVP-001`

1. Petugas CRM membuka Pendaftaran Membership dan mengetik No. Telp `0812-xxxx`.
2. Sistem menampilkan Ibu Sari (Reguler) dan Dika (Reguler). Petugas memilih Ibu Sari.
3. Petugas memilih Gold. Sistem menampilkan iuran Rp 2.400.000 (angka samaran).
4. Petugas menyimpan. Status Menunggu Pembayaran, nomor `MBR-2610-000123`, batas bayar 20 Okt 2026 09.00.
5. *(S2)* Kasir menerima iuran. Membership Aktif 13 Okt 2026 s.d. 12 Okt 2027.
6. Ibu Sari berobat RJ pada 14 Okt 2026. Pendaftaran mencatat snapshot Gold, dan antrean menandainya prioritas.
7. *(Persetujuan billing)* Tagihan pribadi mendapat diskon konsultasi dan farmasi Gold.
8. Pada 1 Okt 2027 Ibu Sari memperpanjang. Setelah lunas, statusnya Terjadwal 13 Okt 2027 s.d. 12 Okt 2028.
9. Pada 13 Okt 2027 pukul 00.05 WIB, Gold lama Kedaluwarsa dan perpanjangan menjadi Aktif.

## 10. Epic dan Functional Requirement

### EPIC CRM-01 — Fondasi data — `EXTEND`

| FR | Requirement yang dapat diuji |
|---|---|
| `FR-CRM-001` | Folder `PatientManagement/Membership` terdaftar di registry sebelum model `PatMembershipStatusHistory` dibuat |
| `FR-CRM-002` | Migration menambah 10 kolom `MstPatientMembership`, `AnnualFeeAmount` tingkat, dan tabel riwayat. Contoh: setelah migration, membership baru tanpa status eksplisit tersimpan `PendingPayment` (7) |
| `FR-CRM-003` | Semua membership lama menjadi Perlu Verifikasi dengan satu riwayat `LEGACY_FLAGGED`, dan proyeksi pasiennya kosong |
| `FR-CRM-004` | Tiga index unik bersyarat menolak baris kedua Aktif/Terjadwal/Menunggu untuk pasien yang sama |

### EPIC CRM-02 — Pendaftaran — `EXTEND`

| FR | Requirement |
|---|---|
| `FR-CRM-010` | `POST /admin` membuat membership Menunggu Pembayaran dengan nomor dari `NumberSeriesAllocator`, `FeeAmountSnapshot` dari tingkat, dan `PaymentDueAt` = simpan + 7 hari |
| `FR-CRM-011` | Pendaftaran ditolak untuk pasien tanpa RM (404), tingkat non-berbayar (400), iuran 0 (409), pasien yang masih Aktif (409), atau yang punya Menunggu (409) |
| `FR-CRM-012` | Perpanjangan diterima hanya bila hari ini ≥ tanggal akhir − 30 hari. Contoh: berakhir 12 Okt 2027, sehingga 11 Sep ditolak dan 12 Sep diterima |
| `FR-CRM-013` | Upgrade hanya ke `PriorityLevel` lebih tinggi |
| `FR-CRM-014` | `GET /admin/patient-lookup` mengembalikan semua pasien yang cocok dengan No. RM/NIK/No. Telp beserta status member, minimal 3 karakter |

### EPIC CRM-03 — Siklus otomatis — `MISSING / NEW`

| FR | Requirement |
|---|---|
| `FR-CRM-020` | Pada 00.05 WIB dan saat aplikasi mulai, Aktif yang tanggal akhirnya < hari ini menjadi Kedaluwarsa |
| `FR-CRM-021` | Terjadwal yang tanggal mulainya ≤ hari ini menjadi Aktif dalam transaksi yang sama dengan kedaluwarsanya periode lama |
| `FR-CRM-022` | Menunggu Pembayaran yang lewat `PaymentDueAt` menjadi Cancelled (`Unpaid`) |
| `FR-CRM-023` | Menjalankan ulang proses pada hari yang sama tidak menambah riwayat. Server mati 2 hari: putaran berikutnya memproses semua yang tertinggal |

### EPIC CRM-04 — Batal, verifikasi, catatan, riwayat — `EXTEND`

| FR | Requirement |
|---|---|
| `FR-CRM-030` | Batal wajib kategori dan alasan 10–500 karakter. Permintaan pasien tanpa refund |
| `FR-CRM-031` | Membatalkan Aktif yang punya Terjadwal ikut membatalkan Terjadwal dengan permintaan refund |
| `FR-CRM-032` | Verifikasi data lama: diakui dengan periode sah (Aktif, atau Kedaluwarsa bila sudah lewat) atau ditolak dengan alasan |
| `FR-CRM-033` | Hanya catatan yang bisa diubah, dan perubahan tercatat nilai lama-baru |
| `FR-CRM-034` | Setiap perubahan status menulis satu riwayat berisi pelaku, sumber, waktu, alasan |
| `FR-CRM-035` | Perintah dengan `expectedRowVersion` usang ditolak 409 |

### EPIC CRM-05 — Satu aturan member aktif — `EXTEND` (memperbaiki `CRM-CAP-004`)

| FR | Requirement |
|---|---|
| `FR-CRM-040` | `PatientMembershipStatusResolver` menjawab Aktif hanya bila status Aktif dan tanggal WIB masuk periode. Contoh: pukul 23.30 WIB di hari terakhir masih Aktif |
| `FR-CRM-041` | Proyeksi `MstPatient` hanya ditulis resolver. Formulir pasien tidak lagi mengubah `IsMember` |
| `FR-CRM-042` | Antrean RJ memakai resolver (`AG-03`, amendment rawat-jalan) |
| `FR-CRM-043` | Kiosk `/options` dan pencarian nomor member hanya mengembalikan membership Aktif |

### EPIC CRM-06 — Otorisasi — `EXTEND`

| FR | Requirement |
|---|---|
| `FR-CRM-050` | `POST /` dan `POST /kiosk` tidak ada lagi (404) |
| `FR-CRM-051` | Aksi baru memakai `Create`, `Update`, `Cancel`, `Verify`, `Read` pada resource `PatientMembership` |
| `FR-CRM-052` | `PUT`, `PATCH status`, dan `DELETE` lama mengembalikan 410 dengan petunjuk pengganti |

### EPIC CRM-07 — Halaman CRM — `EXTEND`

| FR | Requirement |
|---|---|
| `FR-CRM-060` | Menu CRM tampil bagi pemegang `PatientMembership : Read` |
| `FR-CRM-061` | Formulir hanya memuat cari pasien, tingkat, dan catatan |
| `FR-CRM-062` | Detail menampilkan riwayat dan tombol aksi sesuai status dan izin |
| `FR-CRM-063` | Semua pemanggilan memakai rute `/admin` |

### EPIC CRM-08 — Iuran dan aktivasi — `OPEN DECISION` (`DEC-CRM-001`)

| FR | Requirement |
|---|---|
| `FR-CRM-070` | Iuran lunas mengaktifkan (baru/upgrade) atau menjadwalkan (perpanjangan) lewat `ConfirmFeePaidAsync` |
| `FR-CRM-071` | Batal otomatis membatalkan tagihan iuran. Batal salah input atau Terjadwal meminta refund |

### EPIC CRM-09 — Diskon di billing dan snapshot IGD/rawat inap — `OPEN DECISION`

| FR | Requirement |
|---|---|
| `FR-CRM-080` | Diskon per kelompok asal item untuk kunjungan pribadi semua jenis, dihitung ulang mengikuti penjamin saat final |
| `FR-CRM-081` | Snapshot tingkat dicatat pada kunjungan IGD dan rawat inap (`AG-01`) |

## 11. Model Status yang Diusulkan

Menunggu Pembayaran → Aktif / Terjadwal → Kedaluwarsa. Status akhir lainnya: Di-upgrade,
Dibatalkan. Perlu Verifikasi khusus data lama. Invariant utama: per pasien paling banyak satu
Aktif, satu Terjadwal, dan satu Menunggu. Detail lengkap di
`contracts/state-transition-matrix.md`.

## 12. Sasaran Arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| `MstPatient`, `NumberSeriesAllocator`, `RegQueue` snapshot, halaman patient-memberships | `MstPatientMembership`, `MstMembershipTier`, `MembershipStatus`, `PatientMembershipController`, `PatientController`, `OutpatientQueueClassificationService` | `PatMembershipStatusHistory`, `PatientMembershipService`, `PatientMembershipStatusResolver`, `PatientMembershipLifecycleHostedService`, tiga enum |

## 13. Sasaran Kemampuan API

### Health Services / Patient Management / Master Data / Patient Membership

Base URL: `api/v1/health-services/patient-management/master-data/patient-memberships`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/admin` | Daftar baru | `PatientMembership : Create` | `RegisterPatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `EPIC CRM-02` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/renewals` | Perpanjangan | `PatientMembership : Create` | `RenewPatientMembershipRequest` | sama | `EPIC CRM-02` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/upgrades` | Upgrade | `PatientMembership : Create` | `UpgradePatientMembershipRequest` | sama | `EPIC CRM-02` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/cancel` | Batal | `PatientMembership : Cancel` | `CancelPatientMembershipRequest` | sama | `EPIC CRM-04` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/verify` | Verifikasi data lama | `PatientMembership : Verify` | `VerifyLegacyPatientMembershipRequest` | sama | `EPIC CRM-04` | **Rencana (belum tersedia)** |
| `PATCH` | `/admin/{id}/notes` | Ubah catatan | `PatientMembership : Update` | `UpdatePatientMembershipNotesRequest` | sama | `EPIC CRM-04` | **Rencana (belum tersedia)** |
| `GET` | `/admin/{id}/status-histories` | Riwayat | `PatientMembership : Read` | - | `ApiResponse<List<PatientMembershipStatusHistoryResponse>>` | `EPIC CRM-04` | **Rencana (belum tersedia)** |
| `GET` | `/admin/patient-lookup` | Cari pasien + status | `PatientMembership : Read` | `PatientMembershipLookupQuery` | `ApiResponse<PagedResult<PatientMembershipLookupResponse>>` | `EPIC CRM-02` | **Rencana (belum tersedia)** |
| `GET` | `/admin/patients/{patientId}/active` | Status aktif pasien | `PatientMembership : Read` | `date?` | `ApiResponse<PatientActiveMembershipResponse>` | `EPIC CRM-05` | **Rencana (belum tersedia)** |

## 14. Matriks Kewenangan

| Peran | String izin |
|---|---|
| Petugas CRM/Marketing | `[AccessPermission("PatientMembership", "Read")]`, `[AccessPermission("PatientMembership", "Create")]`, `[AccessPermission("PatientMembership", "Update")]`, `[AccessPermission("PatientMembership", "Cancel")]`, `[AccessPermission("PatientMembership", "Verify")]` |
| Admisi, kasir | `[AccessPermission("PatientMembership", "Read")]` |
| Kiosk | `[Authorize(Policy = "KioskRead")]` untuk baca saja |

## 15. Batas Integrasi dan Billing

Modul ini **MUST NOT**: membuat tagihan atau tender sendiri, mencatat bukti bayar manual,
menerapkan diskon di luar billing, membuat tabel tagihan iuran sendiri, atau menyimpan saldo
wallet. Semua itu milik billing-kasir (`INT-04` s.d. `INT-07`).

## 16. Guardrail Regulasi

- NIK dan No. Telp adalah data pribadi pasien: tersamar di tampilan dan tidak masuk log.
- Status member tidak boleh memengaruhi urutan penanganan medis (`CRM-DEC-007`). Ini syarat keselamatan pasien dan keadilan layanan.
- Riwayat membership tidak dihapus (penandaan saja).

## 17. Kebutuhan Non-Fungsional

| ID | Kebutuhan |
|---|---|
| `NFR-001` | Setiap perintah atomik: status, riwayat, dan proyeksi tersimpan bersama atau tidak sama sekali |
| `NFR-002` | Kunci per pasien + index unik bersyarat mencegah dua Aktif/Terjadwal/Menunggu |
| `NFR-003` | `RowVersion` mencegah dua petugas menimpa perubahan satu sama lain |
| `NFR-004` | Semua perpindahan status tercatat di riwayat, bukan hanya di log |
| `NFR-005` | Semua aturan tanggal memakai kalender WIB |
| `NFR-006` | Proses terjadwal idempoten dan mengejar ketertinggalan |
| `NFR-007` | Gagal membaca status member tidak menahan pendaftaran kunjungan |

## 18. Skenario UAT

| ID | Epic | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
|---|---|---|---|---|---|
| `UAT-01` | CRM-01 | Berhasil | Ada 3 membership lama | Jalankan migration | Ketiganya Perlu Verifikasi. Pasiennya terbaca Reguler |
| `UAT-02` | CRM-01 | Gagal | Membership Menunggu untuk pasien X | Coba sisipkan Menunggu kedua lewat basis data | Ditolak index unik |
| `UAT-03` | CRM-02 | Berhasil | Pasien ber-RM, Gold beriuran | Daftar Gold | Menunggu Pembayaran, nomor terbit, batas bayar 7 hari |
| `UAT-04` | CRM-02 | Gagal | Pasien masih Gold Aktif | Daftar baru Silver | Pesan menyarankan perpanjangan/upgrade (409) |
| `UAT-05` | CRM-02 | Gagal | Gold berakhir 12 Okt 2027, hari ini 1 Sep 2027 | Perpanjang | "Perpanjangan baru bisa mulai 12 Sep 2027" |
| `UAT-06` | CRM-03 | Berhasil | Aktif berakhir kemarin | Tunggu 00.05 WIB | Kedaluwarsa, nurse station menampilkan Reguler |
| `UAT-07` | CRM-03 | Gagal | Menunggu dengan batas bayar lewat | Kasir mencoba melunasi (setelah S2) | Ditolak, pendaftaran sudah batal |
| `UAT-08` | CRM-04 | Berhasil | Data lama Perlu Verifikasi | CRM mengakui dengan periode sah | Aktif, riwayat `LEGACY_ACCEPTED` |
| `UAT-09` | CRM-04 | Gagal | Membership Aktif | Batal tanpa alasan | Ditolak, minta alasan |
| `UAT-10` | CRM-05 | Berhasil | Pasien kedaluwarsa | Buka nurse station dan daftar antrean RJ | Keduanya Reguler |
| `UAT-11` | CRM-05 | Gagal | Admisi membuka formulir pasien | Mencentang "Member" | Tidak berpengaruh, status tetap dari membership |
| `UAT-12` | CRM-06 | Berhasil | Akun kiosk | Cari nomor member aktif | Pasien ditemukan |
| `UAT-13` | CRM-06 | Gagal | Akun kiosk | Mencoba membuat membership | Ditolak (404) |
| `UAT-14` | CRM-07 | Berhasil | Petugas CRM bukan Administrator | Buka menu CRM dan daftar | Berhasil |
| `UAT-15` | CRM-07 | Gagal | Dua petugas membuka detail yang sama | Keduanya membatalkan | Satu berhasil, satu mendapat "baru saja diubah petugas lain" |

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Registry folder `PatientManagement/Membership` disetujui sebelum model dibuat | Baris registry + `FR-CRM-001` |
| Migration diterapkan di lokal tanpa galat, dan data lama menjadi Perlu Verifikasi | `UAT-01`, `AT-27` |
| Tidak mungkin ada dua Aktif/Terjadwal/Menunggu untuk satu pasien | `UAT-02`, `AT-12` |
| Pendaftaran baru, perpanjangan, upgrade mengikuti aturan | `UAT-03` s.d. `UAT-05`, `AT-11`, `AT-13`, `AT-14`, `AT-30`, `AT-31` |
| Kedaluwarsa, mulai Terjadwal, dan batal tidak dibayar berjalan otomatis | `UAT-06`, `AT-06`, `AT-08`, `AT-31` |
| Status member sama di semua layanan | `UAT-10`, `AT-36` |
| Kiosk tidak bisa membuat, petugas CRM bisa | `UAT-13`, `UAT-14` |
| Master tingkat berisi empat tingkat beriuran | Rencana data master awal (`02-backend-architecture.md` §8) |
| Iuran mengaktifkan membership | **Belum** dapat dijawab. Menunggu `DEC-CRM-001` (`FR-CRM-070`) |
| Diskon diterapkan billing | **Belum** dapat dijawab. Menunggu persetujuan billing (`FR-CRM-080`) |

## 20. Urutan Pengiriman dan Pertanyaan Terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | `EPIC CRM-01` | Blueprint disetujui, registry folder disetujui |
| `MVP-1` | `EPIC CRM-02`, `EPIC CRM-04`, `EPIC CRM-06` | `MVP-0` selesai |
| `MVP-2` | `EPIC CRM-03`, `EPIC CRM-05`, `EPIC CRM-07` | `MVP-1` selesai. `FR-CRM-042` menunggu persetujuan owner rawat-jalan |
| — | `EPIC CRM-08`, `EPIC CRM-09` | **Tidak masuk gelombang mana pun** sampai keputusannya turun |
| `POST-MVP` | Kemampuan di bagian 8 | Di luar rilis pertama |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| `DEC-CRM-001`: tagihan iuran non-kunjungan, akuntansi, penyetuju refund, penerapan diskon | Owner billing-kasir + Finance | Membership baru tidak bisa Aktif. `EPIC CRM-08`/`09` tidak dapat dimulai | **Ya** |
| `AG-01`: snapshot pada kunjungan IGD/rawat inap | Owner Registration | Diskon IGD/rawat inap tidak dapat diterapkan | Ya (untuk `EPIC CRM-09`) |
| `AG-03`: antrean RJ memakai resolver | Owner rawat-jalan | `FR-CRM-042` tertunda. Antrean RJ tetap memakai aturannya sendiri yang sudah mendekati benar | Tidak |
| `CRM-OQ-001`: nominal iuran dan persen diskon | Yoga Aji Pratama / Marketing | Pendaftaran ditolak (`VAL-03`) | Tidak untuk development, ya untuk go-live |

**Development lock:** terbatas. Owner mengizinkan perencanaan `MVP-0` s.d. `MVP-2` dan menerima
bahwa rilis ini belum bisa mengaktifkan member baru (`CRM-DEC-030`). `AG-04` dan dua usulan
perilaku sudah disetujui (`CRM-DEC-029`). `EPIC CRM-08`/`09` tetap tidak masuk gelombang mana
pun sampai `DEC-CRM-001` diputuskan.
