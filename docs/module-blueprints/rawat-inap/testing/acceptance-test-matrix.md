# Acceptance Test Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.3.0` |
| Status | `draft` |
| Masukan | `00-interview-decisions.md` revision `5` (139 acceptance criteria); `contracts/api-contract.md`, `contracts/validation-matrix.md`, dan `contracts/permission-audit-matrix.md` revision `0.3.0`; kontrak lain revision `0.2.0` |
| Backend SHA | `5afb54b` |
| Frontend SHA | `dec4fdeff` |

Matriks ini memuat **jalur berhasil dan jalur gagal**. Jalur gagal justru yang paling membuktikan
aturan bisnis benar-benar ditegakkan.

Keadaan hari ini yang harus disadari: backend hanya punya **satu** berkas test
(`QuilvianSystemBackend.Tests/BillingManagement/BillingModuleFoundationTests.cs`) dan frontend punya
**empat**. Tidak satu pun menyentuh tempat tidur, kunjungan, atau dokumentasi klinis. Karena itu
`RWI-DEC-051` mewajibkan test menjadi bagian pekerjaan, bukan pekerjaan terpisah.

---

## 1. Admisi dan pemesanan tempat tidur

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-001` | Tempat tidur berstatus `Reserved` tidak muncul pada pencarian tempat tidur kosong | Integrasi | `GET /available-beds` tidak memuat tempat tidur yang sedang dipesan |
| `RWI-AC-002` | Pemesanan pukul 09:15 masih mengunci pada pembacaan 11:14, dan sudah `Available` pada pembacaan 11:16, **tanpa proses latar belakang** | Integrasi | Dua pembacaan dengan waktu berbeda menghasilkan keadaan berbeda; tidak ada penjadwal yang dijalankan |
| `RWI-AC-003` | Batas 2 jam dapat diubah admin dan nilai barunya langsung dipakai | Integrasi | Ubah `BedReservationMinutes`, pemesanan berikutnya memakai nilai baru |
| **Gagal** | Memesan tempat tidur yang sudah dipesan episode lain | Integrasi | 409 dengan pesan "Tempat tidur ini sudah dipesan untuk pasien lain" |
| **Gagal** | Memesan tempat tidur berstatus `Maintenance` | Integrasi | 422 dengan pesan yang menyebut keadaan tempat tidur |
| **Gagal** | Membuka admisi pada kunjungan yang sudah punya episode | Integrasi | 409, dan `INV-INP-04` tidak dilanggar |
| **Gagal** | Membuka admisi tanpa DPJP | Integrasi | 400, dan `INV-INP-03` tidak dilanggar |
| `RWI-AC-116` | Menempatkan pasien yang sudah punya episode `Admitted` | Integrasi | 409 disertai nomor episode dan lokasi yang sedang ditempati. Membuktikan `INV-INP-10` |
| `RWI-AC-117` | Membuka admisi untuk pasien yang punya episode `Draft` lain | Integrasi | Berhasil, disertai peringatan. **Bukan** penolakan |
| **Gagal** | Menempatkan pasien yang episode lamanya `DischargePending` dan kepergiannya belum dicatat | Integrasi | 409 |
| — | Menempatkan pasien yang episode lamanya `DischargePending` tetapi kepergiannya **sudah** dicatat | Integrasi | **Berhasil.** Membuktikan batasnya kepergian fisik, bukan penutupan |

## 2. Penempatan pasien dan pencegahan tempat tidur ganda

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-059` | Setelah pasien ditempatkan, sistem menjawab siapa yang menempati dan sejak jam berapa | Integrasi | `GET /placements/by-episode/{id}` memuat baris dengan `StartDateTime` terisi |
| `RWI-AC-004` | Status episode yang tersedia hanya lima nilai. `InCare` ditolak | Unit | Enum hanya memuat lima nilai; nilai di luar itu ditolak |
| **Gagal — paling penting** | **Dua petugas merebut tempat tidur yang sama pada waktu hampir bersamaan** | Integrasi dengan dua transaksi bersamaan | Satu berhasil, satu ditolak 409. **Tepat satu** baris `InpBedPlacement` aktif untuk tempat tidur itu. Tidak ada penempatan ganda yang tersimpan |
| `RWI-AC-062` | Bila penulisan catatan penempatan gagal, `MstBed.BedStatus` juga tidak berubah | Integrasi | Paksa kegagalan di tengah transaksi; kedua tabel kembali ke keadaan semula |
| **Gagal** | Menempatkan pasien pada episode yang sudah `Admitted` | Integrasi | 409 |
| `RWI-AC-063` | Laporan selisih menampilkan tempat tidur yang statusnya tidak cocok dengan penghuninya | Integrasi | Buat selisih secara sengaja lewat perubahan langsung di database uji; laporan menampilkannya |

## 2A. Kelayakan penempatan — jenis kelamin dan isolasi

Bagian ini lahir pada `contract_version` `0.3.0`. Sebelumnya kelompok ini justru tercatat pada
bagian 14 sebagai **yang tidak diuji**, karena keputusannya belum turun.

Kedelapan aturan Kelayakan Penempatan dipanggil dari dua tindakan. Karena itu setiap skenario di
bawah wajib dijalankan **dua kali** — sekali lewat penempatan, sekali lewat perpindahan — kecuali
yang memang hanya masuk akal pada salah satunya. Test yang hanya menutup jalur penempatan akan
meloloskan pelanggaran lewat perpindahan, dan itu justru jalur yang paling sering dipakai petugas
yang sedang terburu-buru.

### 2A.1 Pemisahan jenis kelamin

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-128` | Pasien perempuan ditempatkan pada tempat tidur bertanda hanya laki-laki | Integrasi | 422 dengan pesan "Tempat tidur ini hanya untuk pasien laki-laki" |
| `RWI-AC-130` | Kamar sudah dihuni pasien perempuan, pasien laki-laki hendak masuk tempat tidur lain di kamar yang sama | Integrasi | 422, dan **pesannya menyebut nama kamarnya**. Pemeriksaannya membaca penghuni yang sedang ada, bukan penanda `MstRoom` |
| — | Kamar yang sama, pasien berikutnya berjenis kelamin sama | Integrasi | **Berhasil.** Membuktikan aturan menolak pencampuran, bukan menolak kamar berpenghuni |
| — | Kamar berisi satu tempat tidur | Integrasi | Aturan pencampuran tidak pernah menolak, apa pun jenis kelamin penghuni sebelumnya di kamar lain |
| `RWI-AC-129` | Jenis kelamin pasien belum tercatat, tempat tidur menerima keduanya, kamar belum berpenghuni | Integrasi | **Berhasil** |
| **Gagal** | Jenis kelamin belum tercatat, kamar sudah berpenghuni | Integrasi | 422. Membuktikan syaratnya dua-duanya, bukan salah satu |
| **Gagal** | Jenis kelamin belum tercatat, tempat tidur hanya menerima satu jenis kelamin | Integrasi | 422 |
| `RWI-AC-133` | Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda | Integrasi | 422 dengan kode dan pesan **sama persis** seperti penempatan. Membuktikan kedua tindakan memanggil pemeriksaan yang sama |

### 2A.2 Pengecualian boks bayi — dua arah

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-132` | Bayi laki-laki ditempatkan pada boks bayi di kamar ibunya | Integrasi | **Berhasil.** Aturan jenis kelamin dan pencampuran dilewati untuk penempatan **ke** boks bayi |
| `RWI-AC-131` | Kamar berisi ibu dan bayi laki-lakinya; pasien perempuan lain hendak masuk | Integrasi | **Berhasil.** Penghuni boks bayi tidak dihitung saat memeriksa pencampuran |
| — | Kamar berisi **hanya** bayi laki-laki di boks, tanpa penghuni dewasa; pasien perempuan hendak masuk | Integrasi | **Berhasil.** Membuktikan pengecualian berlaku juga saat bayi adalah satu-satunya penghuni |
| — | Pasien dewasa ditempatkan pada tempat tidur bertanda boks bayi | Integrasi | Perilakunya mengikuti penanda master; bila master menolak, 422. **Skenario ini menguji batas pengecualian**, bukan melebarkannya |

### 2A.3 Kebutuhan isolasi — penempatan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-134` | Pasien bertanda membutuhkan isolasi ditempatkan pada tempat tidur bukan isolasi | Integrasi | 422 dengan pesan "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi" |
| — | Pasien bertanda membutuhkan isolasi ditempatkan pada tempat tidur isolasi | Integrasi | **Berhasil** |
| `RWI-AC-135` | Pasien tidak membutuhkan isolasi ditempatkan pada tempat tidur isolasi | Integrasi | 422 dengan pesan "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi". Membuktikan penjagaannya **dua arah**, bukan satu arah |
| — | Hasil pencarian tempat tidur kosong untuk pasien yang membutuhkan isolasi | Integrasi | `GET /available-beds` hanya memuat tempat tidur isolasi. Penyaring dan penolak menghasilkan jawaban yang **sama**, bukan berbeda |

### 2A.4 Kebutuhan isolasi — siapa yang boleh menetapkan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-136` | Petugas admisi menyalakan kebutuhan isolasi selagi episode `Draft` | Integrasi | Berhasil; `IsolationSource = AdmissionRecord`, `IsolationSetByUserId` terisi, `IsolationSetByDoctorId` **kosong** |
| `RWI-AC-137` | DPJP aktif mengubah kebutuhan isolasi setelah episode `Admitted` | Integrasi | Berhasil; `IsolationSource = ClinicalDecision`, `IsolationSetByDoctorId` terisi |
| `RWI-AC-139` | Dokter yang **bukan** DPJP aktif mengubah kebutuhan isolasi | Integrasi | 403. Membuktikan `GUARD-INP-04`, penjaga keempat yang tidak dapat dikerjakan mesin hak akses |
| **Gagal** | Petugas admisi mengubah kebutuhan isolasi setelah episode `Admitted` | Integrasi | 403. Membuktikan wewenangnya berhenti begitu episode aktif, bukan berlaku selamanya |
| **Gagal** | Menyalakan kebutuhan isolasi tanpa mengisi keterangan | Integrasi | 400 dengan pesan "Tuliskan alasan atau keterangan kebutuhan isolasi" |
| **Gagal** | Peran di luar admisi dan dokter memanggil endpoint kebutuhan isolasi | Integrasi | 403 dari mesin hak akses, sebelum service dijalankan |

### 2A.5 Perubahan isolasi tidak pernah ditahan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-138` | DPJP menyalakan kebutuhan isolasi sementara pasien berada di tempat tidur biasa | Integrasi | **Berhasil, tidak ditolak.** Nilai tersimpan, dan episode muncul pada `GET /monitoring/isolation-mismatch` |
| — | Pasien pada daftar pantau dipindahkan ke tempat tidur isolasi | Integrasi | Perpindahan berhasil, dan episode **hilang** dari daftar pantau pada pembacaan berikutnya |
| — | Kebalikannya: DPJP mematikan kebutuhan isolasi sementara pasien berada di tempat tidur isolasi | Integrasi | Berhasil, dan episode muncul pada daftar pantau. Membuktikan daftar pantau bekerja dua arah |
| — | Daftar pantau dibaca ketika tidak ada satu pun ketidaksesuaian | Integrasi | Daftar kosong, bukan galat |

---

## 3. Perpindahan pasien

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Perpindahan berhasil menutup penempatan lama dan membuka yang baru | Integrasi | Dua baris `InpBedPlacement`; yang lama punya `EndDateTime` dan `EndReason = Transfer` |
| — | Kelas yang ditagihkan mengikuti kamar yang ditempati | Integrasi | Baris kedua membawa `PatientClassId` kamar tujuan, bukan kamar asal |
| **Gagal — invariant** | Perpindahan gagal di tengah jalan | Integrasi | Pasien tetap pada tempat tidur semula. **Tidak pernah** ada keadaan pasien tanpa tempat tidur. Membuktikan `INV-INP-07` |
| `RWI-AC-079` | Dokter yang bukan DPJP aktif meminta perpindahan | Integrasi | 403 dengan pesan yang menyebut alasannya. Membuktikan `GUARD-INP-01` |
| `RWI-AC-080` | DPJP aktif meminta perpindahan disertai alasan | Integrasi | Berhasil |
| `RWI-AC-083` | Dokter yang tanggung jawabnya sudah berakhir meminta perpindahan | Integrasi | 403 |
| **Gagal** | Perpindahan tanpa alasan | Integrasi | 400 |
| **Gagal** | Perpindahan ke tempat tidur yang sedang ditempati | Integrasi | 409 |
| **Gagal** | Memindahkan pasien yang kepergiannya sudah dicatat | Integrasi | 422 |

## 4. Penanggung jawab

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-078` | Sistem menjawab siapa DPJP pada tanggal tertentu | Integrasi | Dua baris penugasan berperiode; query per tanggal mengembalikan yang benar |
| `RWI-AC-081` | Pengalihan DPJP tanpa alasan ditolak | Integrasi | 400 |
| `RWI-AC-082` | Setelah dialihkan, baris DPJP sebelumnya tetap terbaca dan tidak tertimpa | Integrasi | Baris lama masih ada dengan `EndDateTime` terisi |
| `RWI-AC-084` | Episode aktif tidak pernah tanpa DPJP aktif maupun dengan dua DPJP aktif | Integrasi | Unique index parsial menolak baris kedua yang `EndDateTime` kosong |
| `RWI-AC-101` | Kepala ruangan menugaskan perawat, dan census menampilkan namanya | Integrasi | Census memuat nama perawat |
| `RWI-AC-102` | Peran selain kepala ruangan menugaskan perawat | Integrasi | 403 |
| `RWI-AC-104` | Episode tanpa perawat tetap dapat menerima perpindahan | Integrasi | Perpindahan berhasil walaupun perawat belum ditugaskan |
| `RWI-AC-105` | Episode tanpa perawat muncul pada daftar pantau | Integrasi | `GET /monitoring/unassigned-nurse-episodes` memuat episode itu |

## 4A. Kepergian fisik pasien

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-118` | Mencatat kepergian melepas tempat tidur seketika | Integrasi | Baris penempatan tertutup dengan `EndReason = PatientDeparted`; `MstBed.BedStatus` menjadi `Available`; tempat tidur muncul pada pencarian berikutnya |
| `RWI-AC-119` | Setelah kepergian dicatat, episode tetap `DischargePending` | Integrasi | Status tidak berubah, dan episode tetap muncul pada daftar pantau penutupan tertunda |
| `RWI-AC-120` | Pasien yang sudah pergi tidak muncul di census dan tidak dapat dipindahkan | Integrasi | Census tidak memuatnya; perpindahan ditolak 422 |
| `RWI-AC-121` | Menutup episode tanpa mencatat kepergian tetap berhasil | Integrasi | Tempat tidur dilepas saat penutupan, `EndReason = EpisodeClosed` |
| — | Kepergian **tidak** menulis baris riwayat status | Integrasi | Jumlah baris `InpStatusHistory` tidak bertambah |
| **Gagal** | Mencatat kepergian pada episode `Admitted` | Integrasi | 422 |
| **Gagal** | Mencatat kepergian dua kali | Integrasi | 409 |
| **Gagal** | Waktu kepergian mendahului keputusan pulang | Integrasi | 400 |
| **Gagal** | Kepergian dicatat peran yang tidak berwenang | Integrasi | 403 |
| **Gagal** | Pelepasan tempat tidur gagal di tengah jalan | Integrasi | Kolom kepergian pada episode juga tidak terisi |

## 5. Census dan lama dirawat

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Census menampilkan pasien `Admitted` dan `DischargePending`, tidak menampilkan `Draft`, `Closed`, `Cancelled` | Integrasi | Lima episode berstatus berbeda; census memuat tepat dua |
| — | Lama dirawat dihitung dari selisih tanggal dengan hasil paling sedikit 1 hari | Unit | Masuk 21 Sept 22:30, pulang 22 Sept 06:00 menghasilkan **1 hari**, bukan 0 |
| — | Lama dirawat bertambah pada pergantian tanggal, bukan setiap genap 24 jam | Unit | Masuk 21 Sept 22:30; pada 22 Sept 00:30 sudah bernilai 1 |
| `RWI-AC-064` | Setelah episode ditutup, tempat tidur terbaca `Available` pada pencarian berikutnya | Integrasi | `GET /available-beds` memuat tempat tidur itu |

## 6. Pembatalan admisi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| — | Pembatalan `Draft` oleh petugas admisi disertai alasan | Integrasi | Episode `Cancelled`, pemesanan `Cancelled`, tempat tidur `Available` |
| — | Pembatalan `Admitted` oleh supervisor | Integrasi | Episode `Cancelled`, penempatan ditutup dengan `EndReason = AdmissionCancelled` |
| **Gagal** | Pembatalan `Admitted` oleh petugas admisi | Integrasi | 403 |
| **Gagal** | Pembatalan tanpa alasan | Integrasi | 400 |
| **Gagal** | Pelepasan tempat tidur gagal saat pembatalan | Integrasi | Seluruh pembatalan dibatalkan; episode tetap `Admitted` |

## 7. Keputusan pulang dan resume

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-093` | Resume menampilkan DPJP beserta periodenya secara otomatis | Integrasi | Resume memuat dua DPJP bila episode berganti DPJP |
| `RWI-AC-092` | Membuat resume kedua untuk episode yang sama | Integrasi | 409. Membuktikan `INV-INP-05` |
| `RWI-AC-096` | Cara pulang `Referred` tetapi tujuan rujukan kosong | Integrasi | 400 |
| `RWI-AC-097` | Mengubah resume setelah episode ditutup tanpa sesi koreksi | Integrasi | 409 dengan pesan yang mengarahkan pada sesi koreksi |
| `RWI-AC-124` | Menyunting resume yang belum ditandatangani | Integrasi | Isi berubah, **tidak** ada baris versi yang dibuat |
| `RWI-AC-125` | Mengubah resume yang sudah ditandatangani lewat sesi koreksi | Integrasi | Satu baris `InpDischargeSummaryRevision` berisi salinan isi lama, penandatangan lama, dan sesi koreksi yang menyebabkannya |
| `RWI-AC-126` | Mengubah atau menghapus salinan versi resume | Integrasi | Tidak ada endpoint; percobaan menghasilkan 404 |
| **Gagal** | Dokter bukan DPJP aktif menyatakan pasien boleh pulang | Integrasi | 403. Membuktikan `GUARD-INP-02` |
| **Gagal** | Dokter bukan DPJP aktif menandatangani resume | Integrasi | 403. Membuktikan `GUARD-INP-03` |
| **Gagal** | Menandatangani resume tanpa diagnosis utama | Integrasi | 400 |

## 8. Kelayakan keuangan dan penutupan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-065` | Episode baru berstatus kelayakan `Pending`, dan penutupan ditolak | Integrasi | 422 menyebut kelayakan keuangan |
| `RWI-AC-066` | Setelah kasir menandai `Cleared`, penutupan berhasil tanpa supervisor | Integrasi | Episode `Closed` |
| `RWI-AC-067` | Petugas admisi, perawat, atau dokter menandai kelayakan keuangan | Integrasi | 403 |
| `RWI-AC-068` | Penandaan tanpa catatan ditolak; yang berhasil menyimpan nama dan waktu | Integrasi | 400 pada kasus pertama; kolom terisi pada kasus kedua |
| `RWI-AC-069` | Layar dan laporan menampilkan bahwa kelayakan berasal dari penandaan manual | e2e | `IsManualMarking` terbaca pada jawaban dan tampil di layar |
| `RWI-AC-070` | Episode `Blocked` tidak dapat ditutup petugas admisi | Integrasi | 422 |
| — | Supervisor menutup menembus gerbang keuangan disertai alasan | Integrasi | Episode `Closed`, `IsClosedWithoutFinancialClearance = true`, muncul pada laporan pengecualian |
| **Gagal** | Supervisor menembus gerbang sementara resume belum ditandatangani | Integrasi | 422. Membuktikan jalan keluar **hanya** menembus syarat keuangan |
| **Gagal** | Menutup episode berstatus `Admitted` | Integrasi | 422 dengan pesan yang menyebut keputusan pulang |
| — | Jawaban `closure-readiness` memuat kelima syarat beserta tanda sudah atau belum | Integrasi | Lima baris, bukan satu nilai boleh atau tidak |

## 9. Daftar periksa administrasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-053` | Butir "obat pulang sudah diserahkan" dinonaktifkan admin, lalu penutupan tidak lagi tertahan olehnya | Integrasi | Butir nonaktif tidak muncul pada syarat penutupan |
| — | Butir wajib yang belum ditandai menahan penutupan | Integrasi | 422 menyebut nama butirnya |
| **Gagal** | Menandai butir pada episode yang sudah `Closed` | Integrasi | 409 |

## 10. Riwayat status dan sesi koreksi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-085` | Seluruh perpindahan status terbaca urut lengkap dengan pelaku, waktu, dan alasan | Integrasi | Episode dari `Draft` sampai `Closed` meninggalkan empat baris berurutan |
| `RWI-AC-086` | Bila penulisan riwayat gagal, status episode juga tidak berubah | Integrasi | Paksa kegagalan; kedua tabel kembali semula |
| `RWI-AC-087` | Baris riwayat tidak dapat diubah maupun dihapus lewat endpoint mana pun | Integrasi | Tidak ada endpoint update atau delete; percobaan menghasilkan 404 |
| `RWI-AC-088` | Pemesanan yang gugur meninggalkan baris riwayat bertanda dilakukan sistem | Integrasi | Baris dengan `ActorType = System` dan `ChangedByUserId` kosong |
| `RWI-AC-089` | Episode `Draft` yang batal sendiri meninggalkan baris bertanda sistem | Integrasi | Sama seperti di atas |
| `RWI-AC-090` | Laporan penutupan tanpa kelayakan keuangan disusun dari tabel riwayat tanpa membaca berkas log | Integrasi | Laporan benar walaupun berkas log dikosongkan |
| — | Sesi koreksi dibuka, cara pulang diubah, sesi ditutup | Integrasi | Status episode **tetap** `Closed` sepanjang sesi; tempat tidur tidak berubah; lama dirawat tidak bertambah |
| **Gagal** | Membuka sesi koreksi kedua sementara yang pertama belum ditutup | Integrasi | 409 |
| **Gagal** | Menutup sesi koreksi tanpa daftar perubahan | Integrasi | 400 |
| **Gagal** | Peran selain supervisor membuka sesi koreksi | Integrasi | 403 |

## 11. Pengaturan admin

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-110` | Batas pemesanan diubah dari 2 jam menjadi 3 jam, dan nilai baru berlaku pada pembacaan berikutnya tanpa aplikasi dinyalakan ulang | Integrasi | Pemesanan baru memakai 3 jam |
| `RWI-AC-111` | Kelima angka dapat diubah dari satu layar yang sama | e2e | Satu layar memuat kelimanya |
| `RWI-AC-112` | Setiap perubahan menyimpan nama pengubah dan waktunya | Integrasi | Kolom audit terisi |
| `RWI-AC-113` | Butir daftar periksa dikelola dari layar tersendiri | e2e | Layar terpisah |
| — | Pemesanan yang sudah berjalan **tidak** ikut berubah ketika pengaturan diubah | Integrasi | `ExpiresAt` pemesanan lama tetap memakai nilai lama |

## 12. Perbaikan dan regresi modul lain

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-108` | Tombol nonaktifkan tempat tidur berhasil, dan tempat tidur hilang dari pencarian | e2e | Tidak ada 404 |
| `RWI-AC-109` | Tombol aktifkan berhasil mengaktifkan kembali | e2e | Tidak ada 404 |
| `RWI-AC-060` | Menyetel `BedStatus` menjadi `Occupied` lewat menu master data ditolak | Integrasi | 422 dengan pesan yang mengarahkan ke modul Rawat Inap |
| `RWI-AC-061` | Menyetel `BedStatus` menjadi `Maintenance` lewat menu master data tetap berhasil | Integrasi | 200. Membuktikan wewenang admin tidak berkurang |
| `RWI-AC-114` | Setiap task yang menyentuh Master Data membawa test regresi jalur lama | Integrasi | Endpoint bed lain tetap berperilaku sama |
| `RWI-AC-115` | Penjaga kewenangan DPJP punya test yang membuktikan dokter bukan DPJP ditolak | Integrasi | Sudah tercakup pada bagian 3 dan 7 |

## 12A. Bayi baru lahir dan hubungan dengan ibu

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-122` | Episode bayi menyimpan rujukan ke episode ibunya | Integrasi | Detail episode bayi memuat episode ibu; pencarian bayi yang dirawat gabung di kamar tertentu berhasil |
| `RWI-AC-123` | Menutup episode ibu tidak menutup episode bayi | Integrasi | Episode bayi tetap `Admitted`, boks bayi tetap terisi |
| `RWI-AC-127` | Riwayat lokasi satu episode terbaca lengkap dari catatan penempatan milik Rawat Inap | Integrasi | Tidak ada query ke tabel milik modul Registrasi |
| **Gagal** | Episode bayi menunjuk episode milik pasien yang sama | Integrasi | 422 |
| **Gagal** | Episode menunjuk dirinya sendiri sebagai episode ibu | Integrasi | 400 |
| **Gagal** | Episode ibu yang ditunjuk sudah `Closed` | Integrasi | 422 |

## 13. Data master awal

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-AC-106` | Seeder menolak berjalan pada lingkungan produksi | Unit | Seeder mengembalikan penolakan bila lingkungan produksi |
| `RWI-AC-107` | Admin menambah kamar dan tempat tidur lewat layar, tanpa perintah database | e2e | Kamar dan tempat tidur baru muncul pada pencarian |
| — | Modul tetap berjalan bila `MstInpatientSetting` belum terisi | Integrasi | Nilai bawaan dipakai, dan peringatan tercatat |

---

## 14. Yang **tidak** diuji pada revisi ini

| Yang tidak diuji | Alasan | Decision ID |
| --- | --- | --- |
| Pengkajian, catatan dokter, tindakan, resep untuk pasien rawat inap | Slice di luar scope | `DEC-INP-001` |
| Serah terima IGD ke rawat inap | Slice di luar scope | `DEC-INP-002` |
| Persetujuan umum rawat inap | Slice di luar scope | `DEC-INP-003` |
| Pengiriman SATUSEHAT | Slice di luar scope | `DEC-INP-005` |
| Cara pulang meninggal dan kabur | Slice di luar scope | `DEC-INP-007` |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice yang di luar scope | `DEC-INP-001` |

Ketiadaan test untuk **keenam** butir itu adalah **keadaan yang disengaja**, bukan cakupan yang
terlupa.

**Satu baris keluar dari daftar ini pada `0.3.0`.** "Penolakan penempatan karena isolasi atau jenis
kelamin" kini justru menjadi bagian 2A dengan 26 skenario, setelah `DEC-INP-004` turun lewat
`RWI-DEC-064` sampai `RWI-DEC-066`.

---

## 15. Ringkasan cakupan

| Kelompok | Skenario berhasil | Skenario gagal |
| --- | ---: | ---: |
| Admisi dan pemesanan | 5 | 6 |
| Penempatan dan tempat tidur ganda | 4 | 2 |
| **Kelayakan penempatan — jenis kelamin dan isolasi** | 15 | 11 |
| Perpindahan | 3 | 6 |
| Penanggung jawab | 6 | 2 |
| **Kepergian fisik pasien** | 5 | 5 |
| Census dan lama dirawat | 4 | 0 |
| Pembatalan | 2 | 3 |
| Pulang dan resume | 6 | 4 |
| Kelayakan dan penutupan | 6 | 4 |
| Daftar periksa | 2 | 1 |
| Riwayat dan koreksi | 7 | 3 |
| Pengaturan | 5 | 0 |
| Perbaikan dan regresi | 6 | 0 |
| Bayi dan hubungan dengan ibu | 3 | 3 |
| Data master | 3 | 0 |
| **Total** | **82** | **49** |

Delapan skenario gagal yang paling penting, dan tidak boleh dilewati:

1. **Dua petugas merebut tempat tidur yang sama** — membuktikan `INV-INP-02`.
2. **Perpindahan gagal di tengah jalan** — membuktikan `INV-INP-07`.
3. **Dokter bukan DPJP meminta perpindahan** — membuktikan `GUARD-INP-01`, satu-satunya kewenangan
   yang tidak dijaga mesin hak akses.
4. **Supervisor menembus gerbang keuangan sementara resume belum ditandatangani** — membuktikan
   jalan keluar itu benar-benar sempit.
5. **Menempatkan pasien yang sudah punya episode aktif** — membuktikan `INV-INP-10`.
6. **Menempatkan pasien yang episode lamanya sudah ditinggalkan secara fisik** — membuktikan
   batasnya kepergian, bukan penutupan. Ini kebalikan nomor 5 dan sama pentingnya: yang pertama
   mencegah data ganda, yang kedua mencegah pasien tertahan oleh urusan administrasi.
7. **Perpindahan ke kamar yang sudah dihuni jenis kelamin berbeda** — membuktikan kedua tindakan
   memanggil pemeriksaan yang sama. Test yang hanya menutup jalur penempatan meloloskan pelanggaran
   lewat jalur yang justru paling sering dipakai.
8. **Pasien biasa menempati tempat tidur isolasi** — membuktikan penjagaannya dua arah. Arah ini
   yang paling sering terlupa, karena tidak terasa berbahaya bagi pasien yang bersangkutan; yang
   dirugikan adalah pasien berikutnya yang benar-benar membutuhkan isolasi.

---

## 16. Perubahan pada `contract_version` `0.3.0`

| Yang ditambahkan | Dasar |
| --- | --- |
| Bagian 2A kelayakan penempatan, 26 skenario dalam lima kelompok | `RWI-DEC-064`, `RWI-DEC-065`, `RWI-DEC-066` |
| Dua skenario gagal wajib baru pada ringkasan cakupan | `RWI-RULE-012` A.6 dan B.7 |

| Yang dihapus | Alasan |
| --- | --- |
| Baris "penolakan penempatan karena isolasi atau jenis kelamin" pada bagian 14 | Bukan lagi di luar scope; keputusannya turun 2026-08-21 |

Dua belas acceptance criteria baru `RWI-AC-128` sampai `RWI-AC-139` seluruhnya tercakup.

---

## 17. Perubahan pada `contract_version` `0.2.0`

| Yang ditambahkan | Dasar |
| --- | --- |
| Bagian 4A kepergian fisik pasien, 10 skenario | `RWI-DEC-055` |
| Empat skenario satu pasien satu episode pada bagian 1 | `RWI-DEC-054` |
| Tiga skenario versi resume pada bagian 7 | `RWI-DEC-057` |
| Bagian 12A hubungan bayi dan ibu, 6 skenario | `RWI-DEC-056` |
| Satu skenario kepemilikan riwayat lokasi pada bagian 12A | `RWI-DEC-053` |

Tidak ada skenario yang dihapus pada revision ini.
