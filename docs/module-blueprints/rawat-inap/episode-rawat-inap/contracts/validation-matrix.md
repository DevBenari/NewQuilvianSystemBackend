# Validation Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.6.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | `00-interview-decisions.md` revision `15`; `02-backend-architecture.md` revision `0.4`; `04-prd-to-mvp.md` revision `0.6.0` |
| Dampak kompatibilitas | Seluruhnya baru, kecuali satu baris pada bagian 8 yang mengubah perilaku endpoint existing |

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
| **Jenis kelamin belum tercatat** | idem | `MstPatient.Gender` kosong dan tempat tidur tidak menerima keduanya, atau kamarnya sudah berpenghuni | "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang menerima laki-laki dan perempuan, di kamar yang belum ada penghuninya." | 422 |
| **Kamar sudah dihuni jenis kelamin berbeda** | idem | Ada penempatan aktif di kamar yang sama dengan jenis kelamin berbeda, di luar boks bayi | "Kamar Melati 3 sedang dihuni pasien perempuan, sehingga tidak dapat menerima pasien laki-laki." | 422 |
| **Butuh isolasi, tempat tidur bukan isolasi** | idem | `RequiresIsolation` benar dan `MstBed.IsIsolationBed` salah | "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi." | 422 |
| **Tidak butuh isolasi, tempat tidur isolasi** | idem | `RequiresIsolation` salah dan `MstBed.IsIsolationBed` benar | "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi." | 422 |
| Pengecualian boks bayi | idem | Tempat tidur bertanda `IsForNewborn` | **Tidak ada penolakan** dari tiga aturan jenis kelamin di atas | — |
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
| Aturan jenis kelamin dan isolasi | idem | Salah satu dari lima aturan pada bagian 3 tidak terpenuhi | Sama seperti pesan di bagian 3 | 422 |
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
| Deposit tidak boleh dikirim tanpa episode | `POST /patient-funds/deposits/{encounterId}/top-ups` | `episodeId` kosong pada penerimaan yang berasal dari admisi rawat inap | "Deposit rawat inap wajib terikat pada episode." | 422 |
| Satu deposit satu episode | idem | `episodeId` menunjuk episode milik pasien lain atau kunjungan lain | "Episode yang dipilih bukan milik kunjungan ini." | 409 |
| Retry tidak membuat kwitansi ganda | idem | `idempotencyKey` sama dengan penerimaan yang sudah tersimpan | Tidak ada pesan; transaksi pertama dikembalikan apa adanya | 200 |
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
