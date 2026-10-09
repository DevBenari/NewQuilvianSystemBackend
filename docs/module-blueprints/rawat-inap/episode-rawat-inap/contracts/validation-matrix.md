# Validation Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | **`0.11.0`** — bagian 15 Workspace PPRI, `approved` 2026-10-08 (`RWI-DEC-265`). Sebelumnya `0.10.0` — bagian 14, `approved` (`RWI-DEC-221`); `0.9.0` — bagian 13 |
| `last_changed_in` | **`0.11.0`** — `VAL-RWA-01` s.d. `55`. Sebelumnya `0.10.0` — `VAL-RWF-70` s.d. `94`; `0.9.0` — `VAL-INP-01` s.d. `17`; `0.8.0` |
| Status | **`draft`** untuk `0.9.0`. `0.8.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | `00-interview-decisions.md` revision `16`; `02-backend-architecture.md` revision `0.7`; `04-prd-to-mvp.md` revision `0.6.0` |
| Dampak kompatibilitas | Seluruhnya baru, kecuali satu baris pada bagian 8 yang mengubah perilaku endpoint existing. **Sejak `0.8.0` satu aturan dicabut**, dan pencabutan itu **melonggarkan** penolakan, bukan menambahnya |

### Perubahan pada `contract_version` `0.8.0` — 11 September 2026

Menyerap `RWI-DEC-101`. Tiga baris pada bagian 3 berubah:

| Baris | Perubahan |
| --- | --- |
| Kamar sudah dihuni jenis kelamin berbeda | **Dicabut.** Kode `ROOM_GENDER_MIXED` dihapus, penghuni kamar tidak lagi diperiksa |
| Jenis kelamin belum tercatat | **Dipersempit.** Syarat "kamar belum ada penghuninya" dicabut; yang tersisa hanya syarat tempat tidur menerima keduanya |
| Pengecualian boks bayi | Jumlah aturan yang dikecualikan turun dari tiga menjadi dua |

Dua aturan isolasi pada bagian yang sama **tidak tersentuh**, dan `BED_GENDER_MISMATCH` juga
tidak. Baris lama dipertahankan dengan coretan supaya pembaca berikutnya tahu aturan itu pernah
ada dan kenapa dicabut.

Pesan pada kolom "Pesan bagi pengguna" ditulis sebagaimana akan dibaca petugas di layar. Bukan
istilah teknis, bukan nama kolom.

---

## 1. Membuka admisi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Pasien wajib ada | `POST /episodes` | `PatientId` kosong atau tidak ditemukan | "Pasien belum dipilih." | 400 |
| Kunjungan wajib bertipe rawat inap | `POST /episodes` | Kunjungan yang dipilih bukan bertipe rawat inap | "Kunjungan yang dipilih bukan kunjungan rawat inap." | 422 |
| Satu kunjungan satu episode | `POST /episodes` | Kunjungan sudah punya episode | "Kunjungan ini sudah punya episode rawat inap." | 409 |
| DPJP wajib ditentukan | `POST /episodes` | `DoctorId` kosong | "Dokter penanggung jawab belum dipilih." | 400 |
| Unit layanan wajib bertipe rawat inap | `POST /episodes` | `ServiceUnitType` bukan `Inpatient` | "Unit layanan yang dipilih bukan unit rawat inap." | 422 |
| Kelas pasien wajib berlaku untuk rawat inap | `POST /episodes` | `IsForInpatient` bernilai salah | "Kelas perawatan yang dipilih tidak berlaku untuk rawat inap." | 422 |

## 2. Memesan tempat tidur

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Draft` | `POST /bed-occupancies/reservations` | Episode bukan `Draft` | "Pemesanan tempat tidur hanya dapat dilakukan sebelum pasien ditempatkan." | 422 |
| Tempat tidur wajib aktif | idem | `MstBed.IsActive` salah | "Tempat tidur ini sedang tidak aktif." | 422 |
| Tempat tidur wajib dapat dipesan | idem | `IsReservable` salah | "Tempat tidur ini tidak dapat dipesan." | 422 |
| Tempat tidur tidak sedang ditutup | idem | `BedStatus` bernilai `Cleaning`, `Maintenance`, `Blocked`, atau `Inactive` | "Tempat tidur sedang tidak dapat dipakai. Keadaan saat ini: Perbaikan." | 422 |
| Tempat tidur belum dipesan orang lain | idem | Ada pemesanan aktif milik episode lain | "Tempat tidur ini sudah dipesan untuk pasien lain." | 409 |
| Tempat tidur belum ditempati | idem | Ada penempatan aktif | "Tempat tidur ini sedang ditempati pasien lain." | 409 |
| Satu episode satu pemesanan aktif | idem | Episode sudah punya pemesanan aktif | "Episode ini sudah memesan tempat tidur lain. Batalkan dulu pemesanan sebelumnya." | 409 |

## 3. Menempatkan pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Draft` | `POST /bed-occupancies/placements` | Episode bukan `Draft` | "Pasien sudah ditempatkan sebelumnya." | 409 |
| Seluruh aturan Kelayakan Penempatan | idem | Salah satu aturan pada bagian 2 tidak terpenuhi | Sama seperti pesan di bagian 2 | 409 atau 422 |
| Pemesanan yang gugur tidak menghalangi | idem | Pemesanan sudah `Expired` tetapi tempat tidur masih kosong | **Tidak ada penolakan.** Penempatan diteruskan tanpa peringatan | — |
| Isian admisi tetap utuh saat ditolak | idem | Penempatan ditolak karena tempat tidur diambil pasien lain | "Tempat tidur BD-RSMMC-00042 sudah ditempati pasien lain. Silakan pilih tempat tidur lain; isian admisi Anda tetap tersimpan." | 409 |
| **Satu pasien satu episode yang hadir** | idem | Pasien sudah punya episode `Admitted`, atau `DischargePending` yang kepergiannya belum dicatat | "Tn. Budi sudah dirawat pada episode RI-2026-09-000123 di Melati 3B. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru." | 409 |
| Peringatan admisi `Draft` ganda | `POST /episodes` | Pasien sudah punya episode `Draft` lain | **Bukan penolakan.** "Pasien ini punya admisi lain yang sedang disiapkan sejak kemarin." Petugas boleh lanjut atau membatalkan yang lama | 200 |
| **Jenis kelamin tidak diterima tempat tidur** | idem | Penanda tempat tidur tidak menerima jenis kelamin pasien | "Tempat tidur ini hanya untuk pasien laki-laki." | 422 |
| **Jenis kelamin belum tercatat** | idem | `MstPatient.Gender` kosong **dan** tempat tidur tidak menerima laki-laki dan perempuan sekaligus. Penghuni kamar **tidak diperiksa** sejak `RWI-DEC-101` | "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang menerima laki-laki dan perempuan." | 422 |
| ~~**Kamar sudah dihuni jenis kelamin berbeda**~~ **DICABUT 11 September 2026** oleh `RWI-DEC-101` | — | ~~Ada penempatan aktif di kamar yang sama dengan jenis kelamin berbeda, di luar boks bayi~~ Penghuni kamar **tidak lagi diperiksa sama sekali** | ~~"Kamar Melati 3 sedang dihuni pasien perempuan, sehingga tidak dapat menerima pasien laki-laki."~~ Tidak ada pesan; kode `ROOM_GENDER_MIXED` dihapus | — |
| **Butuh isolasi, tempat tidur bukan isolasi** | idem | `RequiresIsolation` benar dan `MstBed.IsIsolationBed` salah | "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi." | 422 |
| **Tidak butuh isolasi, tempat tidur isolasi** | idem | `RequiresIsolation` salah dan `MstBed.IsIsolationBed` benar | "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi." | 422 |
| Pengecualian boks bayi | idem | Tempat tidur bertanda `IsForNewborn` | **Tidak ada penolakan** dari **dua** aturan jenis kelamin yang tersisa. Sebelum `RWI-DEC-101` aturannya tiga | — |
| **Pasien asal IGD belum tercatat tiba** | idem | Episode lahir dari serah terima IGD dan catatan kepergian IGD belum bertanda `Tiba` | "Pasien belum tercatat tiba di bangsal. Perawat penerima perlu mencatat kedatangannya lebih dulu." | 422 |

**Lingkup aturan pasien asal IGD.** Aturan itu hanya diperiksa bila episode punya kunjungan asal,
yaitu bila `TrxPatientEncounter.OriginEncounterId` terisi. Pasien datang langsung dan pasien
poliklinik melewatinya tanpa pemeriksaan apa pun. Karena jalur serah terima IGD adalah `INP-S09`
yang di luar scope revisi ini, pada MVP aturan itu tidak pernah menyala. Dasarnya `RWI-DEC-072`
dan `RWI-RULE-029` aturan 8.

**Contoh berangka aturan satu pasien satu episode.** Tn. Budi sedang dirawat di Melati 3B. Pukul
14:00 petugas lain mencoba menempatkannya di Anggrek 1A karena mengira ia pasien baru. Ditolak 409
disertai nomor episode dan lokasi yang sedang ditempati, sehingga petugas langsung tahu bahwa yang
dibutuhkan adalah perpindahan, bukan admisi baru.

Sebaliknya, bila Tn. Budi sudah pulang pukul 10:15 — kepergiannya dicatat — lalu kembali pukul
12:00 dengan keluhan baru, admisi barunya **diterima** walaupun episode lama belum ditutup.

**Contoh berangka aturan baris ketiga.** Sdri. Wati memesan `BD-RSMMC-00042` pukul 09:15 untuk
Ny. Sari. Batasnya 2 jam, jadi gugur pukul 11:15. Ny. Sari baru sampai kamar pukul 11:40. Karena
tempat tidur itu masih kosong, penempatan **tetap berhasil** dan tidak ada peringatan apa pun.
Ini sesuai `RWI-RULE-015`.

## 4. Memindahkan pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `Admitted` | `POST /bed-occupancies/placements/transfer` | Episode bukan `Admitted` | "Perpindahan hanya dapat dilakukan selama pasien masih dirawat." | 422 |
| Alasan medis wajib | idem | `TransferReason` kosong | "Alasan perpindahan wajib diisi." | 400 |
| Tempat tidur tujuan berbeda | idem | Tempat tidur tujuan sama dengan yang sekarang | "Tempat tidur tujuan sama dengan tempat tidur saat ini." | 400 |
| Seluruh aturan Kelayakan Penempatan | idem | Tidak terpenuhi | Sama seperti bagian 2 | 409 atau 422 |
| **Kewenangan per pasien** | idem | Pemohon adalah dokter, tetapi bukan DPJP aktif episode itu | "Hanya DPJP episode ini yang dapat memindahkan pasien. Alihkan tanggung jawab DPJP lebih dulu bila diperlukan." | 403 |
| Pasien yang sudah pergi tidak dapat dipindahkan | idem | Kepergian fisik pasien sudah dicatat | "Pasien sudah tercatat meninggalkan ruangan, sehingga tidak dapat dipindahkan." | 422 |
| Aturan jenis kelamin dan isolasi | idem | Salah satu dari **empat** aturan pada bagian 3 tidak terpenuhi. Sebelum `RWI-DEC-101` aturannya lima | Sama seperti pesan di bagian 3 | 422 |
| Perpindahan utuh | idem | Salah satu langkah gagal di tengah jalan | "Perpindahan gagal. Pasien tetap berada di tempat tidur semula." | 500 |

**Catatan tentang baris kewenangan.** Aturan ini berlaku **hanya untuk pemohon berperan dokter**.
Kepala ruangan, perawat pelaksana, dan supervisor tetap boleh memindahkan tanpa menjadi DPJP,
sesuai `RWI-DEC-012` yang tidak dicabut.

## 5. Membatalkan admisi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Alasan wajib | `PATCH /episodes/{id}/cancel` | Alasan kosong | "Alasan pembatalan wajib diisi." | 400 |
| Wewenang saat `Draft` | idem | Bukan petugas admisi | "Anda tidak punya hak akses untuk tindakan ini." | 403 |
| Wewenang saat `Admitted` | idem | Bukan supervisor atau kepala ruangan | "Pembatalan setelah pasien dirawat hanya dapat dilakukan supervisor atau kepala ruangan." | 403 |
| Belum ada catatan klinis | idem | Sudah ada catatan klinis pada episode | "Episode ini sudah memiliki catatan klinis, sehingga tidak dapat dibatalkan." | 422 |
| Pelepasan tempat tidur menyatu | idem | Pelepasan tempat tidur gagal | "Pembatalan gagal. Tidak ada data yang berubah." | 500 |

**Catatan tentang baris keempat.** Selama slice dokumentasi klinis masih menunggu `DEC-INP-001`,
pemeriksaan "sudah ada catatan klinis" **belum dapat dijalankan sepenuhnya**. Pada MVP, pemeriksaan
memakai penanda pengganti yang tercatat pada `04-prd-to-mvp.md`, dan ini adalah pengurangan
kemampuan yang disadari, bukan kelalaian.

## 4A. Menetapkan kebutuhan isolasi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Wewenang saat episode `Draft` | `PATCH /episodes/{id}/isolation-requirement` | Bukan petugas admisi dan bukan DPJP | "Anda tidak punya hak akses untuk tindakan ini." | 403 |
| Wewenang setelah episode aktif | idem | Bukan **DPJP aktif** episode tersebut | "Setelah pasien dirawat, hanya DPJP episode ini yang dapat mengubah kebutuhan isolasi." | 403 |
| Sumber ditetapkan sistem | idem | — | Petugas admisi menghasilkan `AdmissionRecord`; DPJP menghasilkan `ClinicalDecision`. **Pemanggil tidak boleh menentukannya sendiri** | — |
| Keterangan wajib bila menyalakan | idem | `RequiresIsolation` diubah menjadi benar tanpa `IsolationNote` | "Tuliskan alasan atau keterangan kebutuhan isolasi." | 400 |
| Episode belum ditutup | idem | Episode `Closed` tanpa sesi koreksi | "Episode sudah ditutup." | 409 |
| Perubahan tidak ditahan penempatan | idem | Pasien sedang menempati tempat tidur yang tidak sesuai | **Tidak ada penolakan.** Perubahan diterima, dan episode muncul pada daftar pantau penempatan tidak sesuai | 200 |

**Kenapa baris terakhir tidak menahan.** Menahan pencatatan klinis demi menjaga aturan penempatan
adalah urutan yang terbalik. Yang benar: fakta klinis dicatat lebih dulu, lalu sistem menunjukkan
bahwa penempatannya perlu dibetulkan. Dasarnya `RWI-RULE-012` bagian A aturan 7.

## 5A. Mencatat kepergian fisik pasien

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `DischargePending` | `POST /discharges/{id}/record-departure` | Status lain | "Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang." | 422 |
| Hanya sekali per episode | idem | Kepergian sudah pernah dicatat | "Kepergian pasien sudah dicatat pada pukul 10:15." | 409 |
| Wewenang | idem | Bukan petugas admisi, perawat, kepala ruangan, atau supervisor | "Anda tidak punya hak akses untuk tindakan ini." | 403 |
| Waktu kepergian tidak boleh di masa depan | idem | Waktu yang dikirim melewati waktu sekarang | "Waktu kepergian tidak boleh melewati waktu sekarang." | 400 |
| Waktu kepergian tidak boleh sebelum keputusan pulang | idem | Waktu yang dikirim mendahului `DischargeDecidedAt` | "Waktu kepergian tidak boleh mendahului keputusan pulang." | 400 |
| Pelepasan tempat tidur menyatu | idem | Pelepasan tempat tidur gagal | "Pencatatan kepergian gagal. Tidak ada data yang berubah." | 500 |

**Yang tidak divalidasi, dan itu disengaja.** Sistem **tidak** memeriksa apakah butir administrasi
atau kelayakan keuangan sudah selesai. Kepergian fisik adalah fakta, bukan izin — pasien yang sudah
pulang tetap harus dicatat pulang walaupun administrasinya belum beres. Episode tetap `DischargePending`
dan tetap muncul pada daftar pantau penutupan tertunda.

## 5B. Hubungan episode bayi dan ibu

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Boleh kosong | `POST /episodes`, `PUT /episodes/{id}` | `MotherEpisodeId` tidak diisi | **Bukan penolakan.** Sebagian besar episode memang bukan bayi rawat gabung | 200 |
| Tidak boleh menunjuk diri sendiri | idem | `MotherEpisodeId` sama dengan episode itu sendiri | "Episode tidak dapat menunjuk dirinya sendiri sebagai episode ibu." | 400 |
| Tidak boleh pasien yang sama | idem | Episode ibu milik pasien yang sama | "Episode ibu harus milik pasien yang berbeda." | 422 |
| Episode ibu harus ada dan belum ditutup | idem | Episode ibu tidak ditemukan atau sudah `Closed`/`Cancelled` | "Episode ibu tidak ditemukan atau sudah selesai." | 422 |

## 6. Keputusan pulang dan resume

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Hanya DPJP aktif | `POST /discharges/{id}/decide` | Pemohon bukan DPJP aktif | "Hanya DPJP episode ini yang dapat menyatakan pasien boleh pulang." | 403 |
| Cara pulang wajib dipilih | idem | `DischargeType` kosong atau `Unknown` | "Cara pulang wajib dipilih." | 400 |
| Cara pulang wajib termasuk yang berlaku | idem | Nilai di luar tiga yang berlaku | "Cara pulang yang dipilih belum tersedia pada versi ini." | 422 |
| Diagnosis utama wajib | `PATCH /discharges/{id}/summary/sign` | `PrimaryDiagnosisText` kosong | "Diagnosis utama wajib diisi sebelum resume ditandatangani." | 400 |
| Tujuan rujukan wajib bila dirujuk | idem | Cara pulang `Referred` dan `ReferralDestination` kosong | "Tujuan rujukan wajib diisi untuk pasien yang dirujuk." | 400 |
| Hanya DPJP aktif yang menandatangani | idem | Penandatangan bukan DPJP aktif | "Hanya DPJP episode ini yang dapat menandatangani resume." | 403 |
| Satu resume per episode | `PUT /discharges/{id}/summary` | Sudah ada resume | Resume yang ada diperbarui, bukan ditolak | — |
| Perubahan sebelum tanda tangan tidak membuat versi | idem | Resume belum ditandatangani | Isi ditimpa biasa, tanpa salinan versi | — |
| Perubahan setelah tanda tangan menyimpan versi lama | idem, lewat sesi koreksi | Resume sudah ditandatangani | Salinan versi lama tersimpan otomatis; pengguna tidak perlu melakukan apa pun | — |

## 7. Penutupan episode

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Episode wajib `DischargePending` | `POST /discharges/{id}/close` | Status lain | "Episode hanya dapat ditutup setelah DPJP menyatakan pasien boleh pulang." | 422 |
| Resume wajib tertandatangani | idem | `SignedAt` kosong | "Resume pulang belum ditandatangani DPJP." | 422 |
| Butir wajib administrasi | idem | Ada butir `IsMandatory` yang belum ditandai | "Masih ada butir administrasi yang belum ditandai: Berkas administrasi pasien lengkap." | 422 |
| Kelayakan keuangan `Cleared` | idem | Nilai terakhir `Pending` atau `Blocked` | "Kelayakan keuangan belum dinyatakan lunas oleh kasir." | 422 |
| Wewenang menembus gerbang | `POST /discharges/{id}/close-with-override` | Bukan supervisor | "Hanya supervisor yang dapat menutup episode tanpa kelayakan keuangan." | 403 |
| Alasan wajib saat menembus | idem | Alasan kosong | "Alasan penutupan tanpa kelayakan keuangan wajib diisi." | 400 |
| Empat syarat lain tetap berlaku saat menembus | idem | Salah satu dari empat syarat lain belum terpenuhi | Sama seperti pesan masing-masing | 422 |

**Yang penting dipahami:** jalan keluar supervisor **hanya** menembus syarat kelayakan keuangan.
Keempat syarat lainnya tetap wajib, dan tidak ada satu pun peran yang dapat melewatinya.

## 8. Kelayakan keuangan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Wewenang | `POST /discharges/{id}/financial-clearance` | Bukan kasir atau billing | "Hanya petugas kasir atau billing yang dapat menandai kelayakan keuangan." | 403 |
| Catatan wajib | idem | `Note` kosong | "Catatan wajib diisi saat menandai kelayakan keuangan." | 400 |
| Episode belum ditutup | idem | Episode `Closed` tanpa sesi koreksi | "Episode sudah ditutup." | 409 |

## 8A. Deposit rawat inap

Aturan di bawah dijalankan `BillingManagement`. Modul Rawat Inap hanya membacanya. Baris bertanda
**peringatan** sengaja **tidak** menghasilkan kode kesalahan: admisi tetap berlanjut.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Nominal wajib berupa angka positif bila diisi | Langkah Deposit admisi | Nominal negatif atau bukan angka | "Jumlah deposit tidak boleh negatif." | 400 |
| ~~Deposit tidak boleh dikirim tanpa episode~~ | — | **Dicabut `0.6.1`.** Penerimaan dikirim per kunjungan, dan satu kunjungan rawat inap hanya boleh punya satu episode — `InpEpisode.EncounterId` unique — sehingga episodenya tidak perlu dikirim ulang | — | — |
| ~~Satu deposit satu episode~~ | — | **Dicabut `0.6.1`.** Sudah dijaga database: `BilDepositAccount.EncounterId` unique | — | — |
| Retry tidak membuat kwitansi ganda | `POST /patient-funds/deposits/{encounterId}/top-ups` | Header `Idempotency-Key` sama dengan penerimaan yang sudah tersimpan | Tidak ada pesan; transaksi pertama dikembalikan dengan penanda `IsReplay` | 200 |
| Kunci sama tetapi isinya berbeda | idem | `Idempotency-Key` diulang dengan nominal atau metode bayar yang berbeda | "Permintaan dengan kunci yang sama sudah dipakai untuk data yang berbeda." | 409 |
| **Peringatan** deposit di bawah minimum kebijakan | Langkah Deposit admisi | Nominal lebih kecil dari minimum kebijakan penjamin/kelas | "Deposit kurang Rp… dari minimum yang disarankan. Admisi tetap dapat dilanjutkan." | — |
| **Peringatan** deposit belum diisi padahal kebijakan mensyaratkan | idem | Nominal kosong atau nol sementara kebijakan mensyaratkan deposit | "Deposit belum diisi. Admisi tetap dapat dilanjutkan dan kekurangannya akan ditagih." | — |
| Kebijakan tidak mensyaratkan deposit | idem | Penjamin/kelas tidak mensyaratkan deposit | Langkah dilewati; tidak ada transaksi Rp0 yang dibuat | — |
| `Cleared` ditolak selama settlement belum selesai | `POST /discharges/{id}/financial-clearance` | Masih ada kekurangan terhadap tagihan final atau refund yang belum diselesaikan | "Masih ada kekurangan pembayaran atau refund yang belum diselesaikan." | 422 |
| Pembatalan admisi tidak menghapus uang | `POST /episodes/{id}/cancel` | Episode punya penerimaan deposit yang belum di-refund/reversal | "Deposit yang sudah diterima harus diselesaikan lebih dulu, atau pakai penutupan override supervisor." | 422 |

## 9. Sesi koreksi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Hanya supervisor | `POST /episodes/{id}/correction-sessions` | Bukan supervisor | "Hanya supervisor yang dapat membuka kembali episode." | 403 |
| Episode wajib `Closed` | idem | Status lain | "Sesi koreksi hanya untuk episode yang sudah ditutup." | 422 |
| Alasan wajib | idem | Alasan kosong | "Alasan membuka kembali episode wajib diisi." | 400 |
| Satu sesi terbuka | idem | Sudah ada sesi terbuka | "Episode ini sedang dalam sesi koreksi yang belum ditutup." | 409 |
| Daftar perubahan wajib | `PATCH .../correction-sessions/{sessionId}/close` | `ChangedFieldSummary` kosong | "Tuliskan apa saja yang diubah sebelum menutup sesi koreksi." | 400 |

## 10. Perubahan aturan pada endpoint yang sudah ada

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Status penghunian bukan wewenang admin | `PATCH /health-services/master-data/beds/{id}/availability` | Nilai yang dikirim `Reserved` atau `Occupied` | "Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. Untuk menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir." | 422 |

Ini satu-satunya baris pada dokumen ini yang **mengubah perilaku endpoint yang sudah dipakai**.
Dasarnya `RWI-RULE-027` aturan 4 dan 5. Persetujuan pemilik `MasterData` tercatat sebagai
`RWI-OQ-033` dan **sudah diberikan** 21 Agustus 2026 lewat `RWI-DEC-062`. Diterapkan
`BE-RWI-006` pada 1 September 2026.

Dua aturan turunan ikut ditegakkan pada aksi yang sama, keduanya dari `RWI-RULE-027` aturan 2
yang menempatkan `MstBed.BedStatus` sebagai **salinan** catatan penempatan:

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| Tempat tidur sedang ditempati | `PATCH /health-services/master-data/beds/{id}/availability` | Ada penempatan aktif pada tempat tidur itu | "Tempat tidur ini sedang ditempati pasien rawat inap. Statusnya baru dapat diubah setelah pasien dipindahkan atau kepergiannya dicatat." | 422 |
| Nilai tidak dikenali | idem | Nilai yang dikirim `Unknown` | "Status ketersediaan tempat tidur tidak dikenali." | 422 |

Nilai `Available`, `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive` **tetap diterima**.
`Available` sengaja tetap diizinkan sebagai jalan kembali: tanpa itu, tempat tidur yang
ditutup admin untuk dibersihkan tidak akan pernah dapat dibuka lagi dari layar master.

## 11. Aturan penanganan waktu

| Aturan | Penjelasan |
| --- | --- |
| Seluruh waktu disimpan sebagai UTC | Mengikuti konvensi project; seluruh model existing memakai `DateTime.UtcNow` |
| Waktu mulai penempatan **bukan selalu** waktu penempatan dibuat | Untuk episode yang lahir dari serah terima IGD, `InpBedPlacement.StartDateTime` dibaca dari event `Tiba` pada catatan kepergian IGD dan **tidak pernah dikoreksi** setelah tersimpan. Untuk jalur lain tetap waktu penempatan dibuat. `RWI-DEC-072` |
| Kedaluwarsa pemesanan dihitung saat dibaca | `RWI-DEC-007`. Tidak ada program penjadwal |
| Episode `Draft` telantar dihitung saat dibaca | `RWI-DEC-030`. Tidak ada program penjadwal |
| Lama dirawat dihitung dari **selisih tanggal**, bukan selisih jam | `RWI-RULE-019`. Hasil paling sedikit 1 hari |
| Lama dirawat bertambah pada pergantian tanggal | Bukan setiap genap 24 jam |

**Contoh berangka lama dirawat.** Tn. Budi masuk 21 September pukul 22:30 dan pulang 22 September
pukul 06:00. Selisih jamnya hanya 7,5 jam, tetapi tanggalnya berbeda, sehingga lama dirawat
tercatat **1 hari**, bukan 0 hari.

---

## 12. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 1 | `RWI-RULE-005`, `RWI-DEC-011` |
| 2, 3 | `RWI-RULE-001`, `RWI-RULE-002`, `RWI-RULE-015`, `RWI-RULE-027` |
| 4 | `RWI-RULE-006`, `RWI-RULE-008`, `RWI-RULE-016`, `RWI-RULE-030` |
| 5 | `RWI-RULE-004`, `RWI-DEC-010` |
| 6 | `RWI-RULE-011`, `RWI-RULE-032`, `RWI-DEC-045` |
| 7 | `RWI-RULE-009`, `RWI-RULE-010`, `RWI-RULE-018` |
| 8 | `RWI-RULE-028`, `RWI-DEC-040` |
| 3, 11 | `RWI-RULE-029` aturan 8, `RWI-DEC-072` — ditambahkan pada `0.4.0` |
| 4A | `RWI-RULE-012` bagian A, `RWI-DEC-065` |
| 5A | `RWI-RULE-036`, `RWI-DEC-055` |
| 5B | `RWI-DEC-056`, `RWI-RULE-014` |
| 9 | `RWI-RULE-020`, `RWI-DEC-028`, `RWI-DEC-057` |
| 10 | `RWI-RULE-027`, `RWI-DEC-039` |
| 11 | `RWI-RULE-002`, `RWI-RULE-019`, `RWI-RULE-022` |

---

## 13. Perubahan pada `contract_version` `0.9.0` — amandemen terbatas ★ 15 September 2026

**Status `draft`.** Bagian ini memperkenalkan **nomor aturan** `VAL-INP-##` untuk aturan baru, karena sub-modul lain
merujuknya — `dokter-rawat-inap` `0.6.0` merujuk `VAL-INP-01`. Aturan pada bagian 1 s.d. 12 tetap tanpa nomor.

### 13.1 Penugasan dokter pendukung

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| **`VAL-INP-01`** | `POST /episodes/{id}/doctor-assignments/supporting` dengan `LateDocumentation` | Peran bukan `OnCallDoctor`; `EndDateTime` kosong atau tidak lebih besar dari `StartDateTime`; alasan kosong — `RWI-DEC-130`, `RWI-AC-189` | "Penugasan singkat penulisan catatan terlambat wajib berperan dokter jaga, punya waktu selesai, dan beralasan." | 400 |
| `VAL-INP-02` | Sama, tujuan apa pun | Peran `Dpjp` | "DPJP dialihkan lewat Pengalihan DPJP, bukan penugasan pendukung." | 422 |
| `VAL-INP-03` | Sama | Pengguna bukan kepala ruangan atau supervisor | "Hanya kepala ruangan atau supervisor yang dapat menugaskan dokter pendukung." | 403 |
| `VAL-INP-04` | Sama | Alasan kosong untuk `Regular` | "Isi alasan pelibatan dokter." | 400 |
| `VAL-INP-05` | Sama | Episode bukan `Admitted` atau `DischargePending` | "Penugasan dokter hanya untuk pasien yang masih dirawat." | 422 |
| `VAL-INP-06` | Sama | Dokter tidak aktif pada master dokter | "Dokter yang dipilih tidak aktif." | 422 |
| `VAL-INP-07` | Sama | Dokter yang sama sudah punya penugasan dengan peran yang sama yang periodenya bertumpuk | "Dokter ini sudah ditugaskan sebagai {peran} pada periode itu." | 409 |
| `VAL-INP-08` | Sama, tujuan `LateDocumentation` | `StartDateTime` lebih dari 5 menit sebelum saat simpan. Untuk tujuan `Regular`, waktu mulai lampau diterima selama tidak sebelum waktu pasien masuk — konsulen yang dipanggil 09.00 boleh dicatat 09.30 | "Waktu mulai penugasan singkat tidak boleh di masa lalu. Catatan terlambat dinilai terhadap penugasan lama dokter." | 400 |
| `VAL-INP-09` | `PATCH …/{assignmentId}/end` | Penugasan `Dpjp`; sudah berakhir; `EndDateTime` sebelum `StartDateTime` | "Penugasan ini tidak dapat diakhiri dengan cara ini." | 409 |

> **Kenapa `VAL-INP-08` menolak waktu mulai lampau.** `RWI-DEC-130` butir (7): penugasan singkat tidak menjangkau waktu
> lampau. Mengizinkan waktu mulai Selasa akan membuat penugasan singkat Kamis diam-diam menjadi penugasan Selasa.

### 13.2 Census dokter

| Aturan | Berlaku pada | Kondisi | Perilaku |
| --- | --- | --- | --- |
| `VAL-INP-10` | `GET census?assignedToMe=true` | Query juga mengirim `DoctorId` dokter lain | `DoctorId` **diabaikan**; bukan penolakan |
| `VAL-INP-11` | Sama | Akun tanpa `DoctorId` | Daftar kosong beserta pesan "Akun Anda tidak terhubung dengan data dokter" |

### 13.3 Resume pulang

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | ---: |
| `VAL-INP-12` | `PUT /discharges/{episodeId}/summary` | `ImportantFindingsSummary` > 4.000 karakter; `DischargeConditionNote` atau `EducationSummary` > 2.000 karakter | "Isian {nama} terlalu panjang, paling banyak {n} karakter." | 400 |

Tiga isian baru **tidak wajib**. Menambahkannya ke syarat tanda tangan menunggu pemilik klinis — `RWI-RULE-032`.

### 13.4 Penutupan episode — peringatan, bukan penolakan

| Aturan | Kondisi | Perilaku |
| --- | --- | --- |
| `VAL-INP-13` | Ada konsep catatan dokter yang belum ditandatangani | Peringatan `UNSIGNED_DOCTOR_DRAFTS`; penutupan **tidak** ditahan — `RWI-DEC-138` (5) |
| `VAL-INP-14` | Ada pesanan tindakan tertunda | Peringatan `PENDING_PROCEDURE_ORDERS`; dibatalkan saat penutupan — `RWI-DEC-143` (3) |
| `VAL-INP-15` | Ada pesanan tindakan tertunda yang sudah ditagih | Peringatan `BILLED_PENDING_PROCEDURE_ORDERS`; tidak dibatalkan; masuk daftar pantau |
| `VAL-INP-16` | Ada dosis obat lewat jadwal belum dicatat | Peringatan `UNRECORDED_PAST_DOSES`; tidak dibatalkan |
| `VAL-INP-17` | Salah satu langkah 4–6 gagal | Seluruh penutupan batal; `500` "Penutupan gagal disimpan, coba lagi" |

---

## 14. Perubahan pada `contract_version` `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.10.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Traceability | `FR-RWF-040` s.d. `049`, `071`, `080` s.d. `082`, `086` s.d. `090` |

Kalimat pesan di bawah adalah bunyi yang dilihat pengguna. Kode `400` untuk bentuk isian; `422` untuk aturan bisnis; `403` untuk hak akses.

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWF-70` | Order tindakan wajib ada, aktif, dan milik kunjungan episode | `POST episodes/{id}/surgery-bookings` | `422` `INP-SRG-001` | "Tindakan operasi belum dipesan dokter" |
| `VAL-RWF-71` | Episode harus `Admitted` | Sama | `422` `INP-SRG-002` | "Pemesanan ruang bedah hanya untuk pasien yang sedang dirawat" |
| `VAL-RWF-72` | Tab Obgyn selalu `Obstetric`; tab Bedah Operasi selalu `General` — isian dari klien diabaikan | Sama | — | — |
| `VAL-RWF-73` | `PreferredAt` tidak lebih dari 15 menit di masa lalu | Sama | `400` | "Tanggal dan jam operasi yang diinginkan sudah lewat" |
| `VAL-RWF-74` | Sisi penandaan = `OprCase.Laterality` bila sisi kasus bukan `NotApplicable` | `PUT ward-pre-op/draft`, `PATCH send`, `PATCH confirm` | `422` `OPR-WPO-001` | "Sisi penandaan berbeda dengan sisi pada pesanan operasi" |
| `VAL-RWF-75` | Penerima pra-operasi ≠ pengirim | `PATCH ward-pre-op/confirm` | `422` `OPR-WPO-002` | "Pengirim dan penerima harus akun yang berbeda" |
| `VAL-RWF-76` | Semua butir wajib dikonfirmasi pengirim sebelum dikirim | `PATCH ward-pre-op/send` | `422` `OPR-WPO-003` | "Butir wajib belum lengkap: {daftar butir}" |
| `VAL-RWF-77` | Pra-operasi tidak dapat diubah pada kasus `InProgress`, `Completed`, `Rejected`, `Cancelled` | Semua tulis pra-operasi | `422` `OPR-WPO-004` | "Catatan pra-operasi tidak dapat diubah pada kasus ini" |
| `VAL-RWF-78` | Kirim butuh tanda vital tercatat untuk episode | `PATCH ward-pre-op/send` | `422` `OPR-WPO-005` | "Catat tanda vital pasien lebih dulu" |
| `VAL-RWF-79` | Titik penandaan: `X`, `Y` 0–100; minimal satu titik bila sisi bukan `NotApplicable` | `PUT ward-pre-op/draft` | `400` | "Tandai area operasi pada gambar tubuh" |
| `VAL-RWF-80` | Gerbang "Siap": versi terbaru `Confirmed`, bukan `NeedsUpdate` | Kesiapan OK | `422` (kendala) | "Catatan pra-operasi belum dikonfirmasi" / "Catatan pra-operasi perlu diperbarui setelah penundaan" |
| `VAL-RWF-81` | Tolak order hanya dari `Requested` | `PATCH cases/{id}/reject` | `422` `OPR-CASE-REJ-001` | "Hanya pesanan berstatus Diminta yang dapat ditolak" |
| `VAL-RWF-82` | Alasan tolak 10–500 karakter | Sama | `400` | "Isi alasan penolakan" |
| `VAL-RWF-83` | Kasus `Rejected` tidak dapat diubah | `PUT`, `schedule`, `postpone`, `cancel`, `start` | `422` `OPR-CASE-REJ-002` | "Kasus yang ditolak tidak dapat diubah; pesan ulang sebagai kasus baru" |
| `VAL-RWF-84` | Penerima serah terima pasca operasi ≠ pengirim | `PATCH handovers/{id}/accept` | `422` `OPR-HO-002` | "Pengirim tidak dapat menerima serah terima sendiri" |
| `VAL-RWF-85` | Pasien menempati bed aktif di unit tujuan sebelum diterima (tolak tidak butuh syarat ini) | Sama, `Accept = true` | `422` `OPR-HO-001` | "Pindahkan pasien ke tempat tidur di unit ini lewat Transfer Pasien sebelum menerima serah terima" |
| `VAL-RWF-86` | Tolak serah terima wajib beralasan | Sama, `Accept = false` | `422` (sudah ada) | Tetap |
| `VAL-RWF-87` | Admisi pasien yang punya permintaan `Pending` wajib merujuknya | `POST episodes` | `409` `INP-ADM-REF-001` | "Pasien punya permintaan admisi dari kamar pulih; buka admisi dari permintaan itu" |
| `VAL-RWF-88` | Permintaan yang dirujuk harus `Pending` dan milik pasien yang sama | Sama | `422` `INP-ADM-REF-002` | "Permintaan admisi ini sudah selesai atau dibatalkan" |
| `VAL-RWF-89` | Pembatalan permintaan oleh OK beralasan | Panggilan OK | — (kesalahan program bila kosong) | — |
| `VAL-RWF-90` | Periode laporan wajib dan ≤ 31 hari | `GET reports/room-transfers`, `/export` | `400` | "Pilih periode paling lama 31 hari" |
| `VAL-RWF-91` | Penerima serah terima transfer ≠ pengirim; pasien di unit tujuan | `PATCH transfer-handovers/{id}/accept` | `422` `CLI-TRH-001`, `CLI-TRH-002` | "Pengirim tidak dapat menerima serah terima sendiri" / "Penerima harus bertugas di unit tujuan pasien" |
| `VAL-RWF-92` | Tolak serah terima transfer beralasan | Sama | `400` | "Isi alasan penolakan" |
| `VAL-RWF-93` | Ambang daftar pantau 1–1440 menit | `PUT master-data/inpatient-settings` | `400` | "Batas waktu harus 1 sampai 1440 menit" |
| `VAL-RWF-94` | Kode butir persiapan unik | `POST/PUT master-data/surgical-preparation-items` | `409` `MST-SPI-001` | "Kode butir sudah dipakai" |

**Aturan waktu.** Seluruh waktu disimpan UTC dan ditampilkan Asia/Jakarta, sama dengan bagian 11. "Lamanya menunggu" pada daftar pantau dihitung server dari `SentAt` atau `RequestedAt` terhadap waktu server.

**Penyelarasan decision log revision `31` ★ 2 Oktober 2026.** `VAL-RWF-71`, `VAL-RWF-87`, dan `VAL-RWF-90` beserta aturan ambang `VAL-RWF-93` disahkan pemilik lewat `RWI-DEC-220`; bunyi dan kodenya tidak berubah.

---

## 15. Perubahan pada `contract_version` `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.11.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`) |
| Traceability | `FR-RWA-005` s.d. `008`, `030` s.d. `034`, `050` s.d. `053`, `060`, `061`, `072`, `080` s.d. `083`, `092`, `100` s.d. `102`, `110`, `121`, `122`, `125`, `128`; `RWI-DEC-231`, `234`, `239` s.d. `243`, `248`, `252`, `255`, `258`, `261`, `263` |

Kalimat pesan di bawah adalah bunyi yang dilihat pengguna. Kode `400` untuk bentuk isian; `403` untuk hak akses; `404` bila data tidak ditemukan; `409` untuk tabrakan dengan keadaan data; `422` untuk aturan bisnis. Kalimat bertanda *{…}* diisi server.

### 15.1 Episode, konkurensi, dan siklus

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-01` | Penulisan hanya pada episode `Admitted`/`DischargePending` | Seluruh tulis grup Workspace PPRI | `409` `INP-ADM-DOC-001` | "Episode sudah ditutup atau dibatalkan. Dokumen admisi hanya dapat dibaca dan dicetak ulang." |
| `VAL-RWA-02` | Episode `Draft` belum membuka Workspace PPRI (G-30) | Seluruh endpoint kecuali `/summary` dan `/letterhead` | `409` `INP-ADM-DOC-002` | "Admisi belum dikonfirmasi. Selesaikan alur Admisi Rawat Inap lebih dulu." |
| `VAL-RWA-03` | Satu dokumen aktif per jenis per episode (`INV-RWA-01`) | `POST documents` | `409` `INP-ADM-DOC-003` | "Sudah ada *{nama dokumen}* yang aktif untuk episode ini. Buka dokumen itu atau buat versi koreksi." |
| `VAL-RWA-04` | `RowVersion` harus sama dengan yang tersimpan | Seluruh `PUT`/`PATCH` dan tanda tangan | `409` `INP-ADM-DOC-004` | "Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda." |
| `VAL-RWA-05` | Aksi harus sah untuk status dokumen (`state-transition-matrix.md` 10.1) | Seluruh tulis dokumen | `409` `INP-ADM-DOC-005` | "Dokumen berstatus *{status}* tidak dapat *{tindakan}*." Contoh: "Dokumen berstatus Lengkap tidak dapat diubah. Buat versi koreksi." |
| `VAL-RWA-06` | Buka kunci hanya bila belum ada tanda tangan | `PATCH unlock` | `409` `INP-ADM-DOC-006` | "Kunci tidak dapat dibuka karena sudah ada tanda tangan. Minta supervisor admisi membatalkan dokumen bila isinya salah." |
| `VAL-RWA-07` | Panjang alasan: buang konsep 1–500; batal 10–500; koreksi 10–500 | `PATCH discard`, `PATCH cancel`, `POST revisions` | `400` | "Isi alasan membuang konsep." / "Alasan pembatalan minimal 10 karakter." / "Alasan koreksi minimal 10 karakter." |
| `VAL-RWA-08` | Konsep hanya dibuang pembuatnya | `PATCH discard` | `422` `INP-ADM-DOC-008` | "Hanya pembuat konsep yang dapat membuangnya. Minta supervisor admisi membatalkan dokumen ini." |
| `VAL-RWA-09` | `Idempotency-Key` maksimal 80 karakter | `POST` bertanda kunci | `400` | "Kunci permintaan tidak sah." |

**Contoh `VAL-RWA-07`.** Hendra membatalkan Serah Terima `Completed` dengan alasan "salah" (5 karakter) → ditolak. Alasan "salah pasien, dokumen dibuat untuk RM 00-12-34-57" diterima.

### 15.2 Syarat membuat dokumen

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-10` | Selisih Biaya hanya untuk penjamin utama asuransi atau perusahaan, termasuk yang tidak mengizinkan selisih dibebankan ke pasien (`RWI-DEC-256`) | `POST documents`, `PATCH lock` | `422` `INP-ADM-DOC-010` | "Selisih Biaya hanya untuk pasien dengan penjamin asuransi atau perusahaan." |
| `VAL-RWA-11` | Pelunasan Deposit hanya bila Billing mencatat kekurangan > 0 (`RWI-DEC-231`) | `POST documents`, `PATCH lock` | `422` `INP-ADM-DOC-011` | "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan." |
| `VAL-RWA-12` | Angka deposit harus terbaca dari Billing; tidak pernah diketik (G-45) | `POST documents`, `PATCH lock` jenis Pelunasan Deposit | `422` `INP-ADM-DOC-012` | "Data deposit tidak dapat dibaca dari kasir. Coba lagi beberapa saat." |

### 15.3 Isian dokumen — saat simpan

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-14` | Telepon: tanda hubung, spasi, dan titik dibuang; sisanya angka saja, maksimal 13 digit (`FR-RWA-082`) | `POST`/`PUT documents` | `400` | "Nomor telepon maksimal 13 digit." |
| `VAL-RWA-15` | Kerabat dan permintaan khusus masing-masing paling banyak 3 baris, 1–200 karakter per baris (G-40) | Sama, jenis Privasi | `400` | "Paling banyak 3 kerabat." / "Paling banyak 3 permintaan khusus." |
| `VAL-RWA-16` | Hal yang bertentangan paling banyak 5 butir (`RWI-DEC-242`) | Sama, jenis Nilai Kepercayaan | `400` | "Maksimal 5 butir." |
| `VAL-RWA-17` | Butir serah terima hanya butir yang dibekukan di dokumen itu | `PUT documents`, jenis Serah Terima | `400` | "Butir serah terima tidak dikenal pada dokumen ini." |
| `VAL-RWA-18` | Pihak bersumber relasi atau kontak darurat harus milik pasien yang sama dan masih aktif | `POST`/`PUT documents` | `422` `INP-ADM-DOC-018` | "Data wali atau kontak darurat yang dipilih tidak ditemukan pada data pasien." |
| `VAL-RWA-19` | Jatuh tempo tidak sebelum tanggal surat dan tidak melewati tanggal surat + interval kebijakan (`RWI-DEC-231`, `248`) | `POST`/`PUT documents`, `PATCH lock`, jenis Pelunasan Deposit | `422` `INP-ADM-DOC-019` | "Jatuh tempo paling lambat *{tanggal}* menurut kebijakan deposit." / "Jatuh tempo tidak boleh sebelum tanggal surat." |

**Contoh `VAL-RWA-14`.** Petugas mengetik "0812-3456-7890"; tersimpan "081234567890" (12 digit). "08123456789012" (14 digit) ditolak.

**Contoh `VAL-RWA-19`.** Surat Jumat 9 Oktober 2026, interval 3 hari: 13 Oktober ditolak "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit."

### 15.4 Isian dokumen — saat kunci

Penolakan kunci memuat **seluruh** isian yang kurang dalam satu respons, supaya petugas tidak mengunci berulang kali.

| ID | Aturan | Jenis | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-20` | Setiap butir dipilih Sudah atau Belum (`RWI-DEC-241`) | Serah Terima | `422` `INP-ADM-DOC-020` | "Butir *{nomor}* belum dipilih Sudah atau Belum." Sub-butir disebut namanya, misalnya "Sub-butir Radiologi pada butir 2 belum dipilih." |
| `VAL-RWA-21` | Butir Belum wajib berketerangan | Serah Terima | `422` `INP-ADM-DOC-021` | "Butir *{nomor}* berstatus Belum wajib diberi keterangan." |
| `VAL-RWA-22` | Nama penanda tangan, kota, dan tanggal wajib | Privasi | `422` `INP-ADM-DOC-022` | "Nama penanda tangan wajib diisi." / "Kota dan tanggal wajib diisi." |
| `VAL-RWA-23` | Minimal satu hal yang bertentangan; nama, jenis kelamin, hubungan, alamat penanda tangan wajib | Nilai Kepercayaan | `422` `INP-ADM-DOC-023` | "Minimal satu hal yang bertentangan wajib diisi." / "*{isian}* penanda tangan wajib diisi." |
| `VAL-RWA-24` | Subjek pernyataan wajib (keterangan wajib bila "saudara kandung lainnya"); nama, alamat, tipe ID, No. ID deklarer wajib (G-44); kota dan tanggal wajib | Selisih Biaya | `422` `INP-ADM-DOC-024` | "*{isian}* deklarer wajib diisi." |
| `VAL-RWA-25` | Nama, alamat, telepon yang menyatakan wajib; tanggal surat dan jatuh tempo wajib; kota wajib | Pelunasan Deposit | `422` `INP-ADM-DOC-025` | "*{isian}* wajib diisi." |
| `VAL-RWA-26` | Tidak ada baris "Tarif belum tersedia"; harga manual wajib beralasan; jenis tindakan wajib; lama rawat 1–365 hari (`FR-RWA-092`) | Estimasi Biaya (di luar gelombang) | `422` `INP-ADM-DOC-026` | "Baris *{uraian}*: tarif belum tersedia. Isi harga manual beserta alasannya atau hapus baris itu." |
| `VAL-RWA-27` | Identitas pasien dan profil rumah sakit harus terbaca untuk salinan beku | Semua jenis | `422` `INP-ADM-DOC-027` | "Data pasien atau profil rumah sakit tidak dapat dimuat. Dokumen belum dapat dikunci; coba lagi." |

**Contoh `VAL-RWA-20` dan `21` sekaligus.** Serah Terima dengan butir 5 belum dipilih dan butir 11 Belum tanpa keterangan ditolak satu kali dengan dua pesan: "Butir 5 belum dipilih Sudah atau Belum." dan "Butir 11 berstatus Belum wajib diberi keterangan."

### 15.5 Tanda tangan

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-30` | Tanda tangan hanya pada dokumen `AwaitingSignature` | Seluruh `signatures/*` | `409` `INP-ADM-DOC-030` | "Dokumen belum dikunci oleh petugas admisi, sehingga belum dapat ditandatangani." |
| `VAL-RWA-31` | Slot harus slot wajib jenis dokumen itu (`02-backend-architecture.md` 13.9) | Sama | `422` `INP-ADM-DOC-031` | "Kolom tanda tangan ini tidak ada pada *{nama dokumen}*." |
| `VAL-RWA-32` | Satu slot satu tanda tangan | Sama | `409` `INP-ADM-DOC-032` | "Kolom ini sudah ditandatangani *{nama}* pada *{waktu}*." |
| `VAL-RWA-33` | Satu akun satu slot petugas (`RWI-DEC-239`) | Slot petugas | `422` `INP-ADM-DOC-033` | "Satu petugas tidak boleh menandatangani dua kolom pada serah terima yang sama." Untuk jenis lain: "…pada dokumen yang sama." |
| `VAL-RWA-34` | Slot Perawat penerima hanya bila pasien menempati bed aktif pada episode (`RWI-DEC-255`) | `signatures/receiving-nurse` | `422` `INP-ADM-DOC-034` | "Pasien belum menempati tempat tidur." |
| `VAL-RWA-35` | Catatan kertas: nama 1–200 karakter, hubungan wajib, waktu tanda tangan tidak sebelum dokumen dikunci dan tidak lebih dari 5 menit di depan waktu server | `signatures/patient-or-family` | `400` | "Isi nama dan hubungan penanda tangan." / "Waktu tanda tangan tidak boleh sebelum dokumen dikunci atau di masa depan." |

Slot Kepala Ruangan tanpa `SignAsHeadNurse` ditolak filter hak akses `403` (`RWI-AC-352`), bukan oleh aturan di atas.

### 15.6 Cetak

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-40` | Cetak ulang wajib beralasan; pada episode `Closed`/`Cancelled` setiap cetak wajib beralasan (`RWI-DEC-240`, G-33) | `POST print-logs` | `422` `INP-ADM-PRT-001` | "Pilih alasan cetak ulang." |
| `VAL-RWA-41` | Alasan "lainnya" wajib berketerangan 1–200 karakter | Sama | `400` | "Isi keterangan alasan cetak ulang." |
| `VAL-RWA-42` | Dokumen berupiah hanya lewat `/amount-print`; dokumen tanpa rupiah hanya lewat `/print` | `GET print`, `GET amount-print` | `422` `INP-ADM-PRT-002` | "Dokumen ini dicetak lewat jalur cetak yang lain." (pesan teknis; layar tidak pernah memanggil jalur yang salah) |
| `VAL-RWA-43` | Cetak berupiah juga butuh `Print` (`RWI-DEC-258` butir 3) | `GET amount-print` | `403` `INP-ADM-PRT-003` | "Anda tidak punya hak mencetak dokumen ini." |
| `VAL-RWA-44` | Cetak IPD ditahan bila identitas pasien atau data episode gagal terbaca (`FR-RWA-072`) | `POST print-logs` jenis IPD; `GET base-data` (`CanPrint = false`) | `422` `INP-ADM-PRT-004` | "Cetak ditahan sampai data wajib terbaca lengkap." |
| `VAL-RWA-45` | Jenis gelang yang dicatat harus sama dengan jenis yang dihitung dari data pasien (`RWI-DEC-243`) | `POST print-logs` | `422` `INP-ADM-PRT-005` | "Jenis gelang tidak sesuai data pasien. Muat ulang data gelang." |
| `VAL-RWA-46` | Jumlah salinan 1–10; `DocumentId` wajib untuk cetak dokumen dan harus milik episode | `POST print-logs` | `400` / `404` | "Jumlah salinan 1 sampai 10." |

**Contoh `VAL-RWA-40`.** Gelang Tn. Budi sudah dicetak 09.50. Andi mencetak lagi 15.10 tanpa memilih alasan → ditolak "Pilih alasan cetak ulang." Dengan alasan "rusak" → log "Cetakan ke-2, rusak".

### 15.7 Master butir dan pengaturan

| ID | Aturan | Endpoint | Kode | Pesan |
|---|---|---|---|---|
| `VAL-RWA-50` | Induk sub-butir harus ada, aktif, jenis sama, butir utama (bukan sub-butir), dan bukan dirinya sendiri | `POST`/`PUT inpatient-clearance-items` | `422` `MST-ICI-001` | "Induk butir harus butir utama dengan jenis yang sama." |
| `VAL-RWA-51` | Butir penutupan tidak punya induk maupun sumber saran | Sama | `422` `MST-ICI-002` | "Butir penutupan tidak boleh punya induk atau sumber saran." |
| `VAL-RWA-52` | Jenis butir tidak dapat diubah setelah dipakai | `PUT` | `422` `MST-ICI-003` | "Jenis butir tidak dapat diubah karena sudah dipakai dokumen atau penandaan." |
| `VAL-RWA-53` | Satu sumber saran hanya untuk satu butir serah terima aktif | `POST`/`PUT`, `PATCH status` | `409` `MST-ICI-004` | "Sumber saran ini sudah dipakai butir *{kode}*." |
| `VAL-RWA-54` | Batas umur gelang bayi 0–16 tahun | `PUT inpatient-settings` | `400` `MST-IST-001` | "Batas umur gelang bayi 0 sampai 16 tahun." |
| `VAL-RWA-55` | Kode formulir maksimal 50 karakter; kota 100; kode label 30 | Sama | `400` `MST-IST-002` | "*{isian}* terlalu panjang." |

### 15.8 Peringatan yang sengaja tanpa kode kesalahan

| Peringatan | Tempat | Bunyi |
|---|---|---|
| Dokumen admisi belum lengkap (`RWI-DEC-234`) | Detail Episode, header Workspace PPRI | "Dokumen admisi belum lengkap: *n* (*nama dokumen*)" |
| Jatuh tempo pelunasan terlewati (`RWI-DEC-260`) | Detail Episode (tanpa rupiah); header Workspace PPRI bagi pemegang `ViewAmount` (dengan rupiah) | "Pelunasan deposit jatuh tempo *12 Okt 2026 11.00* terlewati — lihat kasir" / "… — kurang Rp 3.000.000" |
| Sumber aturan gagal | Detail Episode, header | "Kelengkapan dokumen admisi tidak dapat dihitung" |
| Data pasien berubah sesudah dokumen dikunci (`FR-RWA-124`) | Layar dokumen | "Data pasien telah diperbarui sejak dokumen ini dikunci." |
| Relasi tidak ditemukan untuk hubungan yang dipilih (`FR-RWA-021`) | Formulir GC V1, Data Wali | "Tidak ditemukan di data wali/kontak darurat. Isi manual." |

### 15.9 Aturan penanganan waktu dan hari kerja

| Aturan | Isi | Contoh |
|---|---|---|
| Penyimpanan | Seluruh waktu UTC; tampilan dan cetak menurut `MstHospitalSite.TimeZoneId`, bawaan `Asia/Jakarta` (`NFR-RWA-08`) | 11.00 WIB disimpan `04:00Z` |
| Hari kerja | Senin–Jumat, tanpa kalender libur (`RWI-DEC-261`) | Surat Kamis 8 Oktober 2026, interval 3 → bawaan Jumat 9 Oktober 11.00 WIB |
| Pemotongan | Bawaan tidak pernah melewati tanggal surat + interval; boleh jatuh Sabtu atau hari libur (`RWI-DEC-248`) | Surat Jumat 9 Oktober, interval 1 → Sabtu 10 Oktober 11.00 WIB; interval 0 → tanggal surat |
| Terlewati | Saat waktu server > `DueAt` dan Billing masih mencatat kekurangan > 0 (`RWI-DEC-260`) | Sabtu 10 Oktober 11.01 dengan kekurangan Rp 1.000.000 → peringatan tampil |
| Umur gelang | Dihitung pada tanggal cetak, zona waktu rumah sakit | Lahir 12 Maret 1981, dicetak 7 Oktober 2026 → "45 th" |
