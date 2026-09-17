# Kontrak Perubahan Sumber Pembayaran Kunjungan — Serah Terima ke `RegistrationManagement`

## Metadata

| Field | Nilai |
| --- | --- |
| `contract_id` | `MPY-ENC-PAYER-001` |
| `contract_version` | `1.0.0` |
| Status | `APPROVED` |
| Disetujui oleh | Muhammad Hamzah — Product/Domain owner `RegistrationManagement` |
| Tanggal persetujuan | 11 September 2026 |
| Dasar persetujuan | `MPY-DEC-011` pada [`00-interview-decisions.md`](../../../billing-kasir/00-interview-decisions.md) (blueprint `billing-kasir`) |
| Modul peminta | `billing-kasir` (`BIL-CASH-001`, revisi `1.1`) |
| Modul pemilik pekerjaan | **`RegistrationManagement`** — seluruh perubahan source pada kontrak ini ditulis di modul itu, bukan di `billing-kasir` |
| Trace | `MPY-DEC-007`, `MPY-DEC-010`, `MPY-DEC-011`, `MPY-DES-001`, `MPY-DES-002`, `MPY-DES-004`; `CAP-33` (Missing) |
| Snapshot backend | `d295c4d59b68d223edc597c8b165b7ef4282b49f` (branch `Yasmina`) |
| Snapshot frontend | `0eafa76bf397a47ceb9d44a6f69006ee25f8ba51` (branch `yasmina`) |
| Dampak kompatibilitas | **Aditif.** Nol perubahan skema, nol endpoint existing berubah, nol perilaku pendaftaran kunjungan berubah |
| Hubungan dengan `RWI-ENC-PAYER-001` | **Tidak mencabut, tidak mengubah, tidak melonggarkan.** Kontrak ini justru bergantung pada invariant yang dikunci di sana |

> **Kenapa berkas ini disimpan di sini, bukan di `billing-kasir`.** Kontrak ini menyangkut tabel dan
> layanan yang sama dengan [`encounter-company-guarantor-contract.md`](./encounter-company-guarantor-contract.md)
> (`RWI-ENC-PAYER-001`) — addendum lintas modul milik `RegistrationManagement` yang sudah lebih
> dulu disimpan di `<blueprint-root>` ini. Mengikuti preseden itu, kontrak ini ditaruh berdampingan
> supaya Muhammad Hamzah, selaku pemilik `RegistrationManagement`, meninjau kedua addendum yang
> menyentuh tabel `RegPatientEncounterGuarantor` dari satu tempat yang sama. **Permintaannya
> sendiri berasal dari `billing-kasir`**, bukan dari `rawat-inap` — jejak keputusan bisnis dan
> desainnya tetap sepenuhnya berada di blueprint `billing-kasir` (`MPY-DEC-*`/`MPY-DES-*`); yang
> berpindah hanya lokasi fisik berkas serah terima ini.

> **Catatan provenance yang dicatat apa adanya.** Persetujuan `MPY-DEC-011` disampaikan kepada tim
> `billing-kasir` **melalui Product/Domain Owner dalam percakapan**, bukan sebagai pernyataan
> langsung pemilik `RegistrationManagement` pada sesi itu. Dokumen ini dibuat justru untuk
> menutup selisih itu: ia adalah bentuk tertulis yang dapat ditinjau dan ditandatangani pemilik
> modul sebelum satu baris source ditulis.

---

## 1. Kenapa dokumen ini ada

`billing-kasir` membutuhkan satu kemampuan yang **tidak boleh** ia bangun sendiri: mengubah
sumber pembayaran pada kunjungan yang sudah terbentuk. Tabel sumber pembayaran kunjungan dimiliki
`RegistrationManagement`, dan invariant "satu kunjungan tepat satu sumber pembayaran" dikunci
kontrak `RWI-ENC-PAYER-001` beserta index unik pada tingkat basis data.

Keputusan `MPY-DEC-007` menetapkan pembagian kerjanya: **`billing-kasir` mengorkestrasi,
`RegistrationManagement` menulis.** Dokumen ini adalah spesifikasi yang diserahkan kepada pemilik
`RegistrationManagement` berisi persis apa yang perlu ditambahkan di modulnya.

**Yang diminta hanya satu berkas layanan baru.** Tidak ada permintaan mengubah tabel, endpoint,
controller, maupun perilaku pendaftaran yang sudah berjalan.

---

## 2. Hasil bisnis yang dikunci

Kasir dapat memperbaiki penjamin kunjungan yang keliru **sebelum pasien membayar**, tanpa
membatalkan kunjungan dan mendaftar ulang.

Contoh dengan data samaran: pasien **Ny. S** didaftarkan tunai karena lupa membawa kartu asuransi.
Setelah pelayanan selesai, di depan kasir ia menunjukkan kartu **Prudential** miliknya yang masih
berlaku. Kasir mengganti penjamin kunjungan itu menjadi Prudential. Kunjungan tetap memiliki
**tepat satu** sumber pembayaran; yang berubah adalah isinya, bukan jumlahnya.

Hari ini perbaikan semacam itu tidak mungkin: sumber pembayaran hanya dapat ditetapkan saat
kunjungan dibuat, dan tidak ada satu pun jalur yang mengubahnya sesudah itu (`CAP-33`,
`01-existing-capability-map.md` § 19 milik `billing-kasir`).

---

## 3. Yang diminta dibangun di `RegistrationManagement`

### 3.1 Satu layanan baru

| Unsur | Ketentuan |
| --- | --- |
| Nama | `EncounterPaymentSourceService` |
| Lokasi | `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs` |
| Pendaftaran | `AddScoped<EncounterPaymentSourceService>()` mengikuti pola service Registrasi yang sudah ada |
| Pemanggil | Orkestrator edit tagihan milik `billing-kasir` |
| Endpoint publik | **Tidak ada.** Layanan ini tidak diekspos sebagai endpoint sendiri; endpoint publiknya milik `billing-kasir` |

### 3.2 Bentuk pemanggilan

Satu method yang menerima jenis sumber pembayaran apa pun (`MPY-DES-001`), bukan tiga method
terpisah per jenis:

| Masukan | Ketentuan |
| --- | --- |
| Id kunjungan | Wajib |
| Jenis sumber pembayaran | Wajib — Tunai, Asuransi, atau Penjamin Perusahaan |
| Id metode pembayaran | Wajib **dan hanya boleh diisi** bila jenisnya Tunai |
| Id kartu asuransi pasien | Wajib **dan hanya boleh diisi** bila jenisnya Asuransi |
| Id kartu penjamin perusahaan pasien | Wajib **dan hanya boleh diisi** bila jenisnya Penjamin Perusahaan |
| Tanggal pelayanan | Wajib — dipakai memeriksa masa berlaku kartu |
| Alasan perubahan | Wajib — diteruskan pemanggil, disimpan pemanggil pada jejaknya sendiri |

| Keluaran | Ketentuan |
| --- | --- |
| Jenis dan nama sumber pembayaran **sebelum** perubahan | Dibutuhkan pemanggil untuk jejak auditnya |
| Jenis dan nama sumber pembayaran **sesudah** perubahan | — |
| Hasil penolakan | Alasan bisnis yang dapat dibaca pengguna, bukan galat teknis |

### 3.3 Yang dijamin layanan ini

1. Kunjungan tetap memiliki **tepat satu** baris sumber pembayaran, sebelum maupun sesudah.
2. Baris yang ada **diperbarui di tempat** — bukan dinonaktifkan lalu disisipkan baris baru.
3. Seluruh kolom salinan dibangun ulang dari kartu penjamin yang baru: nomor polis, nomor kartu,
   nomor anggota, nama paket manfaat, nama kelas, kode paket manfaat, kode dan nama perusahaan,
   nomor dan nama karyawan, masa berlaku, kelayakan, dan keaktifan polis.
4. Kolom yang tidak relevan bagi jenis baru **dikosongkan**, bukan ditinggalkan berisi nilai lama.
5. Kartu yang dipilih terbukti milik pasien pada kunjungan itu, aktif, layak, dan berlaku pada
   tanggal pelayanan.
6. Ringkasan jenis pembayaran pada kunjungan ikut diselaraskan sehingga tidak pernah bertentangan
   dengan baris sumber pembayarannya.

### 3.4 Yang **MUST NOT** dilakukan layanan ini

| Larangan | Sebabnya |
| --- | --- |
| Membuka atau menutup transaksi basis data sendiri | Perubahan penjamin dan perhitungan ulang tagihan **MUST** batal bersama. Layanan ini ikut transaksi pemanggil |
| Menyisipkan baris sumber pembayaran baru | Index unik pada kolom kunjungan **tidak difilter**, sehingga baris lama yang ditandai terhapus pun tetap menabrak (`MPY-DES-002`) |
| Mengubah skema tabel mana pun | Kontrak ini aditif; nol migration diminta dari `RegistrationManagement` |
| Membuat atau mengubah kartu penjamin pasien | Kartu dimiliki `PatientManagement`. Layanan ini hanya **memilih** dari yang sudah terdaftar (`MPY-DEC-003`) |
| Menyentuh tagihan, perhitungan, atau pembayaran | Seluruhnya milik `billing-kasir` |
| Memeriksa status tagihan atau keberadaan pembayaran | Gerbang itu sudah dijalankan pemanggil sebelum layanan ini dipanggil — lihat bagian 5 |

---

## 4. Yang **tidak** berubah di `RegistrationManagement`

| Hal | Keadaan |
| --- | --- |
| Skema tabel sumber pembayaran kunjungan | **Nol kolom berubah**, nol index berubah, nol migration |
| Invariant satu sumber pembayaran per kunjungan | **Tetap berlaku penuh** — kontrak ini bergantung padanya, bukan melonggarkannya |
| `RWI-ENC-PAYER-001` | Tetap berlaku apa adanya dan tidak dinaikkan versinya |
| Endpoint pendaftaran kunjungan yang sudah ada | Tidak disentuh sama sekali |
| Perilaku pendaftaran kunjungan baru | Tidak berubah sedikit pun |
| Jalur pendaftaran dari modul lain | Tidak berubah, termasuk penolakan penjamin perusahaan pada jalur non-loket |

Dengan kata lain: sesudah kontrak ini dikerjakan, **seluruh alur `RegistrationManagement` yang
berjalan hari ini berperilaku persis sama.** Yang bertambah hanyalah satu pintu baru yang hanya
dipakai `billing-kasir`.

---

## 5. Pembagian tanggung jawab pemeriksaan

Ini bagian yang paling menentukan agar tidak ada pemeriksaan ganda maupun celah yang tidak
diperiksa siapa pun.

| Pemeriksaan | Dijalankan oleh |
| --- | --- |
| Tagihan masih berstatus terbuka | `billing-kasir`, sebelum memanggil |
| Belum ada pembayaran berhasil | `billing-kasir`, sebelum memanggil |
| Versi baris tagihan masih sesuai | `billing-kasir`, sebelum memanggil |
| Alasan perubahan terisi | `billing-kasir`, sebelum memanggil |
| Kunci idempotensi | `billing-kasir` |
| **Kartu milik pasien yang sama** | **`RegistrationManagement`** |
| **Kartu aktif dan tidak terhapus** | **`RegistrationManagement`** |
| **Kartu berlaku pada tanggal pelayanan** | **`RegistrationManagement`** |
| **Kartu dinyatakan layak** | **`RegistrationManagement`** |
| **Pasangan jenis pembayaran dan id target** | **`RegistrationManagement`** |
| **Invariant satu sumber pembayaran** | **`RegistrationManagement`** |
| Perhitungan ulang tagihan | `billing-kasir`, sesudah layanan ini berhasil |
| Jejak audit perubahan | `billing-kasir`, pada tabelnya sendiri |

Kalimat penolakan untuk baris bertanda `RegistrationManagement` sudah dirumuskan pada
[`validation-matrix.md`](../../../billing-kasir/contracts/validation-matrix.md) `BIL-VAL-064`
sampai `BIL-VAL-074`, dan boleh dipakai apa adanya supaya pengguna membaca kalimat yang sama dari
kedua sisi.

---

## 6. Urutan langkah di dalam satu transaksi

Seluruh langkah di bawah berada dalam **satu** transaksi milik pemanggil. Gagal pada langkah mana
pun membatalkan seluruhnya, dan tidak ada perubahan yang tersimpan sebagian.

1. `billing-kasir` mengunci baris tagihan dan menjalankan gerbang kelayakan edit.
2. `billing-kasir` memanggil `EncounterPaymentSourceService`.
3. Layanan memeriksa keabsahan kartu dan pasangan jenis pembayaran.
4. Layanan memperbarui baris sumber pembayaran di tempat beserta seluruh kolom salinannya.
5. Layanan mengembalikan nilai sebelum dan sesudah kepada pemanggil.
6. `billing-kasir` mengembalikan penanggung baris biaya yang jenisnya tidak lagi tersedia.
7. `billing-kasir` menghitung ulang tagihan dan menghasilkan versi perhitungan baru.
8. `billing-kasir` mencatat jejak perintah beserta nilai sebelum dan sesudah.
9. Transaksi disimpan.

---

## 7. Kewajiban pengujian di sisi `RegistrationManagement`

| Yang diuji | Hasil yang diharapkan |
| --- | --- |
| Rangkaian tunai → asuransi → penjamin perusahaan → tunai pada satu kunjungan | Keempat perubahan berhasil; kunjungan tetap punya **tepat satu** baris sumber pembayaran sepanjang rangkaian |
| Kartu milik pasien lain | Ditolak beserta alasan yang dapat dibaca; baris sumber pembayaran tidak berubah |
| Kartu di luar masa berlaku pada tanggal pelayanan | Ditolak; baris tidak berubah |
| Kartu belum dinyatakan layak | Ditolak; baris tidak berubah |
| Pasangan jenis pembayaran dan id target tidak cocok | Ditolak sebagai isian tidak sah |
| Kolom salinan sesudah penggantian | Seluruhnya berasal dari kartu **baru**; nol kolom menyisakan nilai kartu lama |
| Pendaftaran kunjungan baru | **Regresi** — perilakunya identik dengan sebelum kontrak ini dikerjakan |

---

## 8. Yang perlu diputuskan pemilik `RegistrationManagement`

Ketiganya sudah punya usulan yang dapat langsung disetujui atau dikoreksi.

| No | Hal | Usulan `billing-kasir` | Konsekuensi bila dikoreksi |
| ---: | --- | --- | --- |
| 1 | Satu layanan generik untuk ketiga jenis pembayaran, atau tiga layanan terpisah | **Satu layanan generik** (`MPY-DES-001`) — modelnya memang satu baris dengan satu jenis pembayaran dan tiga himpunan rujukan yang saling eksklusif | Tiga layanan terpisah menggandakan tiga kali pemeriksaan dan penyusunan kolom salinan yang isinya sama |
| 2 | Nama layanan dan nama method | `EncounterPaymentSourceService` | Bebas diganti mengikuti kebiasaan penamaan modul Registrasi; `billing-kasir` menyesuaikan pemanggilannya |
| 3 | Apakah perubahan penjamin perlu dicatat juga di sisi Registrasi | **Tidak** — jejaknya dicatat `billing-kasir` pada tabelnya sendiri (`MPY-DES-003`), supaya `RegistrationManagement` tidak perlu tabel baru | Bila Registrasi menghendaki jejaknya sendiri, itu menambah satu tabel dan satu migration di sisi Registrasi — di luar cakupan kontrak ini dan perlu kesepakatan tersendiri |

---

## 9. Apa yang terjadi bila kontrak ini tidak dikerjakan

Gelombang `MVP-17` pada [`04-prd-to-mvp.md`](../../../billing-kasir/04-prd-to-mvp.md) Bagian E
tidak dapat berjalan, dan kemampuan "mengganti penjamin kunjungan dari layar kasir" tidak
terwujud.

**Yang tetap berjalan tanpa kontrak ini**, karena tidak bergantung padanya: perbaikan perhitungan
kunjungan berpenjamin perusahaan (`MVP-16`, termasuk penutupan cacat peringatan palsu yang sudah
aktif hari ini), penanggung per baris biaya dan penebusan obat (`MVP-18`), serta lembar tagihan
perusahaan (`MVP-19`). Ketiganya sudah dirancang agar tidak menunggu kontrak ini.

---

## 10. Berkas yang disentuh

| Repository | Berkas | Jenis perubahan |
| --- | --- | --- |
| `NewQuilvianSystemBackend` | `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs` | **Baru** |
| `NewQuilvianSystemBackend` | Registrasi dependency pada komposisi layanan Registrasi | Satu baris tambahan |
| — | Tabel, migration, endpoint, controller, DTO Registrasi | **Nol perubahan** |

Task yang melacak pekerjaan ini pada sisi `billing-kasir` adalah `BE-BKC-045` pada
[`roadmap/backend-roadmap.md`](../../../billing-kasir/roadmap/backend-roadmap.md), dan status
task itu **mengikuti** penyelesaian kontrak ini di modul pemiliknya.
