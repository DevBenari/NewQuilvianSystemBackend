# Acceptance Test Matrix — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5`; **bagian encounter-first (`AT-IGD-166`…`185`) ditambahkan 22 September 2026** |
| Status | `draft` |
| Kontrak | API `0.3.0`, state `0.3.0`, validation `0.3.0`, permission/audit `0.3.0` |
| Prasarana uji backend | `QuilvianSystemBackend.Tests` — xunit 2.9.2, EFCore InMemory 9.0.18, pola `IsolatedBillingDbContextFactory` |
| Prasarana uji frontend | `npm run test:unit` → `node --test tests/unit/` |

Matriks ini **melanjutkan** penomoran revision `4` yang berhenti di `AT-IGD-073`. Empat puluh
skenario lama tetap berlaku dan tidak diulang di sini kecuali perilakunya berubah.

---

## 0. Perubahan pada skenario lama

| ID lama | Perubahan | Sebab |
| --- | --- | --- |
| Skenario transfer bagian 5 | **Digantikan** oleh bagian 12 di bawah | `IGD-DEC-069`, `070` |
| Skenario kunjungan bagian 1 | Ditambah pemeriksaan jenis kunjungan dan kelas pasien | `IGD-DEC-074`, `076` |
| Skenario penyelesaian bagian 4 | Ditambah gerbang sikap pesanan | `IGD-DEC-078` |

---

## 9. Jenis kunjungan dan kelas pasien

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-074` | Mendaftarkan pasien IGD baru | Berhasil | Kunjungan tersimpan dengan `EncounterType = Emergency` | `IGD-DEC-074` |
| `AT-IGD-075` | Mengirim `EncounterType = Outpatient` untuk kunjungan IGD | Gagal | `400` beserta pesan yang menyebut jenis kunjungan | `IGD-DEC-074` |
| `AT-IGD-076` | Penolakan pada `AT-IGD-075` berlaku lewat **dua** jalur masuk | Gagal | Baik service maupun controller menolak; menonaktifkan salah satu tidak meloloskan permintaan | `IGD-CONF-01` |
| `AT-IGD-077` | Mendaftar saat master kelas pasien IGD belum ada | Gagal | `400` menyebut master mana yang kurang, bukan gagal diam-diam dengan kelas kosong | `IGD-DEC-076` |
| `AT-IGD-078` | Mendaftar saat master kelas pasien IGD ada dua | Gagal | `400` meminta data master dirapikan | `IGD-DEC-076` |
| `AT-IGD-079` | Kunjungan IGD tersimpan | Berhasil | `PatientClassId` **tidak kosong** | `IGD-DEC-076` |
| `AT-IGD-080` | Frontend mengirim nilai kelas pasien | Berhasil | Nilai kiriman **diabaikan**; backend menetapkan sendiri | `IGD-DEC-076` |
| `AT-IGD-081` | Mendaftarkan pasien poliklinik setelah perubahan | Berhasil | Perilaku rawat jalan **tidak berubah sedikit pun** | `IGD-DEC-068` |

## 10. Episode ganda

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-082` | Mendaftarkan pasien yang kunjungan IGD-nya masih aktif | Gagal | `409` memuat **nomor kunjungan yang sudah ada** dan waktu kedatangannya | `IGD-DEC-084` |
| `AT-IGD-083` | Mendaftarkan pasien yang kunjungan IGD sebelumnya sudah `Completed` | Berhasil | Diterima tanpa hambatan | `IGD-DEC-084` |
| `AT-IGD-084` | Mendaftar dengan alasan jalan keluar terisi | Berhasil | Diterima; alasan, pelaku, dan waktu tersimpan; muncul di daftar pantau | `IGD-DEC-084` |
| `AT-IGD-085` | Pasien tanpa identitas yang belum tertaut data pasien | Berhasil | **Tidak** tertolak aturan episode ganda | `IGD-DEC-084` |

## 11. Status kunjungan tidak boleh mundur

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-086` | Menilai ulang pasien yang sudah `InTreatment` | Berhasil | Status kunjungan **tetap** `InTreatment`, tidak kembali ke `Triaged` | `IGD-GAP-014` |
| `AT-IGD-087` | Menyelesaikan triase `Draft` pada kunjungan `Disposed` | Gagal | `409`; kunjungan **tidak** terbuka kembali | `IGD-GAP-014` |
| `AT-IGD-088` | Menyelesaikan triase pada kunjungan `Completed` | Gagal | `409` | `IGD-GAP-014` |
| `AT-IGD-089` | Seluruh jalur yang menulis `VisitStatus` | Berhasil | Setiap jalur memanggil `CanTransition`; tidak ada penulisan langsung | `IGD-CONF-05` |

## 12. Kepergian pasien — dua rangkaian

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-090` | Mencatat kedatangan saat dokumen masih `Pending` | Berhasil | Diterima; fisik `Arrived`, dokumen **tetap** `Pending`; bukan galat | `IGD-DEC-070` |
| `AT-IGD-091` | Kedatangan dicatat | Berhasil | Pemilik klinis berpindah ke unit penerima | `IGD-DEC-064`, `072` |
| `AT-IGD-092` | Kedatangan dicatat | Berhasil | Dokumen **tidak** otomatis menjadi `Diterima` | `IGD-DEC-064` |
| `AT-IGD-093` | Menerima dokumen saat pasien belum berangkat | Gagal | `409` | `IGD-DEC-070` |
| `AT-IGD-094` | Pasien berstatus `Berangkat` | Berhasil | Pemilik klinis **tetap IGD** | `IGD-DEC-072` |
| `AT-IGD-095` | Setiap tahap kepergian | Berhasil | Sistem selalu menjawab pemilik unit dengan **tepat satu** nama; tidak ada keadaan tanpa pemilik | `IGD-GAP-015` |
| `AT-IGD-096` | Menolak dokumen tanpa alasan | Gagal | `400` | `IGD-DEC-079` |
| `AT-IGD-097` | Waktu berangkat dan waktu tiba | Berhasil | Keduanya **benar-benar terisi**, bukan tetap kosong seperti sebelumnya | `IGD-DEC-070` |
| `AT-IGD-098` | Membuat kepergian dengan field tempat tidur | Gagal | Field tidak dikenal; tidak ada kolom tempat tidur pada tabel | `IGD-DEC-069` |
| `AT-IGD-099` | Jalur IGD apa pun | Berhasil | Tidak satu pun mengubah `MstBed.BedStatus` | `IGD-DEC-069` |
| `AT-IGD-100` | Memanggil route lama `emergency-transfers` | Gagal | `410` beserta pesan yang menyebut route penggantinya | API `0.3.0` |

## 13. Kedatangan susulan dan koreksi

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-101` | Mencatat kedatangan pukul 14.40 untuk kejadian pukul 14.05 | Berhasil | Keduanya tersimpan terpisah: waktu sebenarnya 14.05, waktu server 14.40 | `IGD-DEC-065` |
| `AT-IGD-102` | Pencatatan susulan tanpa rujukan catatan manual | Gagal | `400` | `IGD-DEC-065` |
| `AT-IGD-103` | Mencatat kejadian dengan waktu di masa depan | Gagal | `400` | `IGD-DEC-065` |
| `AT-IGD-104` | Mengoreksi waktu kedatangan | Berhasil | Kejadian lama **tetap tersimpan**, ditandai tidak berlaku, tertaut ke penggantinya | `IGD-DEC-066` |
| `AT-IGD-105` | Membalik kejadian salah pasien tanpa persetujuan orang kedua | Gagal | `403` | `IGD-DEC-066` |
| `AT-IGD-106` | Membalik dengan penyetuju yang sama dengan pengaju | Gagal | `400` | `IGD-DEC-066` |
| `AT-IGD-107` | Kejadian dibalik | Berhasil | Unit terdampak dan penulis catatan turunan **diberi tahu** | `IGD-DEC-085` |
| `AT-IGD-108` | Kejadian dibalik | Berhasil | Catatan klinis turunan **tidak berubah isinya**, hanya diberi penanda | `IGD-DEC-085` |
| `AT-IGD-109` | Pemberitahuan gagal terkirim | Berhasil | Koreksi **tetap berlaku**; kegagalan tercatat sebagai pekerjaan belum tuntas | `IGD-DEC-085` |

## 14. Serah terima SBAR

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-110` | Mengajukan dokumen dengan satu bagian SBAR kosong | Gagal | `400` menyebut bagian mana | `IGD-DEC-079` |
| `AT-IGD-111` | Menandai bagian tidak dapat diisi tanpa alasan | Gagal | `400` | `IGD-DEC-056`, `079` |
| `AT-IGD-112` | Mengajukan dokumen | Berhasil | Alergi, tanda vital terakhir, dan tingkat kegawatan terisi **oleh sistem** | `IGD-DEC-079` |
| `AT-IGD-113` | Tingkat kegawatan pada dokumen | Berhasil | Diambil dari penilaian `Completed` terakhir, **bukan** dari yang `Superseded` atau `Cancelled` | `IGD-DEC-079` |
| `AT-IGD-114` | Dokumen belum lengkap | Berhasil | Tindakan berangkat dan tiba **tetap dapat dijalankan** | `IGD-DEC-070`, `078` |
| `AT-IGD-115` | Dokumen belum lengkap | Berhasil | Muncul pada daftar pantau sebagai pekerjaan belum tuntas | `IGD-DEC-062` |

## 15. Sikap atas pesanan

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-116` | Menyusun daftar pesanan belum selesai | Berhasil | Memuat obat dan tindakan yang statusnya belum tuntas | `IGD-DEC-078` |
| `AT-IGD-117` | Daftar pesanan ditampilkan | Berhasil | Menyertakan keterangan bahwa **penunjang belum dapat dihitung sistem** | `IGD-DEC-087` |
| `AT-IGD-118` | Mengajukan dokumen dengan pesanan tanpa sikap | Gagal | `400` menyebut jumlahnya | `IGD-DEC-078` |
| `AT-IGD-119` | Membatalkan pesanan tanpa alasan | Gagal | `400` | `IGD-DEC-078` |
| `AT-IGD-120` | Meneruskan pesanan ke unit penerima | Berhasil | Muncul sebagai tugas di unit penerima, bukan hanya teks pada ringkasan | `IGD-DEC-078` |
| `AT-IGD-121` | Mengubah sikap yang sudah tersimpan | Berhasil | Tercatat sebagai koreksi; sikap lama tetap tersimpan | `IGD-DEC-078` |
| `AT-IGD-122` | Menyelesaikan kunjungan dengan pesanan tanpa sikap | Gagal | `409` | `IGD-DEC-078` |
| `AT-IGD-123` | Menyelesaikan kunjungan dengan tagihan belum final | Berhasil | Diterima; tagihan **tidak diperiksa** | `IGD-DEC-021` |

## 16. Riwayat penugasan dokter

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-124` | Menetapkan dokter kedua lewat endpoint penetapan | Gagal | `409`; diarahkan memakai aksi pengalihan | `IGD-DEC-082` |
| `AT-IGD-125` | Mengalihkan dokter | Berhasil | Baris lama memperoleh waktu berakhir; baris baru dibuat; **keduanya tersimpan** | `IGD-DEC-082` |
| `AT-IGD-126` | Menanyakan dokter penanggung jawab pada waktu tertentu | Berhasil | Menjawab dokter yang berlaku **saat itu**, bukan dokter sekarang | `IGD-DEC-073`, `082` |
| `AT-IGD-127` | Dua permintaan penetapan bersamaan | Gagal | Salah satu ditolak; index unik bersyarat mencegah dua dokter aktif | `IGD-DEC-082` |
| `AT-IGD-128` | Setelah penetapan atau pengalihan | Berhasil | `TrxPatientEncounter.DoctorId` sama dengan dokter yang sedang aktif | `IGD-DEC-082` |
| `AT-IGD-129` | Mengalihkan tanpa alasan | Gagal | `400` | `IGD-DEC-082` |

## 17. Kewenangan unit

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-130` | Petugas unit lain mencatat kedatangan | Gagal | `403` beserta pesan yang menyebut sebabnya | `IGD-DEC-086` |
| `AT-IGD-131` | Petugas unit tujuan mencatat kedatangan | Berhasil | Diterima | `IGD-DEC-086` |
| `AT-IGD-132` | Penugasan yang masa berlakunya sudah lewat | Gagal | `403` | `IGD-DEC-086` |
| `AT-IGD-133` | Punya penugasan unit tetapi tidak punya kemampuan klinis | Gagal | `403` | `IGD-DEC-058` |
| `AT-IGD-134` | Punya kemampuan tetapi tidak punya penugasan unit | Gagal | `403` | `IGD-DEC-058` |
| `AT-IGD-135` | Mesin hak akses `SysAccessPolicy` | Berhasil | Perilakunya **tidak berubah** untuk modul mana pun | `IGD-DEC-086` |
| `AT-IGD-136` | Pelayanan klinis darurat saat penugasan tidak ada | Berhasil | **Tidak diblokir** | `IGD-DEC-086` |

## 18. Riwayat versi catatan klinis

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-137` | Mengoreksi tanda vital | Berhasil | Jumlah baris **bertambah**, tidak tetap | `IGD-DEC-080` |
| `AT-IGD-138` | Setelah tiga kali koreksi | Berhasil | Ketiga nilai terbaca lengkap beserta pelaku dan waktunya | `IGD-DEC-080` |
| `AT-IGD-139` | Membaca tanda vital biasa | Berhasil | Hanya nilai berlaku yang dikembalikan | `IGD-DEC-080` |
| `AT-IGD-140` | Koreksi tanpa alasan | Gagal | `400` | `IGD-DEC-080` |
| `AT-IGD-141` | Penyimpanan koreksi gagal di tengah | Gagal | Baris lama **tidak** terlanjur ditandai tidak berlaku | `IGD-DEC-080` |
| `AT-IGD-142` | Endpoint hapus permanen catatan klinis | Gagal | Tidak ada endpoint semacam itu | `IGD-DEC-080` |

## 19. Penghubung kunjungan

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-143` | Kunjungan menunjuk dirinya sendiri sebagai asal | Gagal | `400` | `IGD-DEC-075` |
| `AT-IGD-144` | Rangkaian kunjungan membentuk lingkaran | Gagal | `400` | `IGD-DEC-075` |
| `AT-IGD-145` | Kunjungan tanpa asal | Berhasil | Nilai kosong diterima sebagai sah | `IGD-DEC-075` |
| `AT-IGD-146` | Seluruh kunjungan lama | Berhasil | Tetap terbaca tanpa diubah | `IGD-DEC-075` |

## 20. Pengkajian keperawatan IGD

| ID | Skenario | Jalur | Hasil yang diharapkan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-147` | Menyimpan pengkajian untuk pasien IGD | Berhasil | Tersimpan **tanpa** satu pun baris antrean dibuat | `IGD-DEC-068` |
| `AT-IGD-148` | Daftar antrean poliklinik | Berhasil | **Tidak memuat** satu pun pasien IGD | `IGD-DEC-068` |
| `AT-IGD-149` | Menyimpan pengkajian rawat jalan tanpa antrean | Gagal | Tetap ditolak seperti sebelumnya | `IGD-DEC-068` |
| `AT-IGD-150` | Membuat diagnosis, tindakan, dan resep IGD | Berhasil | Ketiganya tersimpan | `IGD-DEC-068` |
| `AT-IGD-151` | Memesan pemeriksaan laboratorium IGD | Berhasil | Tersimpan menempel pada kunjungan IGD | `IGD-DEC-087` |

---

## 21. Cakupan yang belum dapat diuji

| Yang belum dapat diuji | Sebab | Kapan dapat diuji |
| --- | --- | --- |
| `AT-IGD-147` sampai `AT-IGD-150` | Menunggu persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` | Setelah pelonggaran dikerjakan |
| Pembuatan kunjungan rawat inap dari disposisi `RANAP` | Modul Inpatient Management belum ada | Setelah `RWI-BP-001` diimplementasikan |
| Status dan hasil pemeriksaan penunjang | `LabOrder` tidak punya kolomnya | Setelah slice pelengkapan `LabOrder` |
| Pembedaan obat "diserahkan" dan "diberikan" | Catatan pemberian obat belum ada | `IGD-GAP-025` |
| Perilaku unit tanpa simpul organisasi | Belum diputuskan — `IGD-OQ-071` | Setelah Security/Privacy owner memutuskan |
| Nilai batas waktu eskalasi dan interval pengkajian ulang | Menunggu SOP MMC | Setelah SOP disahkan |

## 22. Prasarana uji yang dipakai

| Lapis | Prasarana | Keadaan |
| --- | --- | --- |
| Backend | `QuilvianSystemBackend.Tests`, xunit + EFCore InMemory | **Ada dan berjalan**; cakupan baru Billing |
| Backend, pola isolasi DbContext | `IsolatedBillingDbContextFactory` | Dapat dipakai ulang untuk IGD |
| Frontend | `node --test tests/unit/` | **Ada**; tiga berkas khusus IGD |
| Frontend, e2e | `tests/e2e/` | Ada |

Kewajiban `RWI-DEC-051` berlaku: setiap task yang menyentuh modul milik pihak lain **wajib**
membawa test regresi untuk jalur lama yang disentuhnya, dan test itu menjadi syarat selesainya
task. Ini menyangkut `AT-IGD-081`, `AT-IGD-135`, dan `AT-IGD-149`.

---

## Revisi 6 — sikap dan penerimaan pesanan

Empat belas skenario baru, `AT-IGD-152` sampai `AT-IGD-165`. Seluruhnya lahir dari
`IGD-DEC-100`, `101`, `102`, dan `103`.

| ID | Skenario | Hasil | Yang dibuktikan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-152` | Pesanan yang spesimennya sudah diambil ditetapkan `Continue`, lalu kunjungan ditutup | Berhasil | Kunjungan **boleh** ditutup; pesanan tetap berjalan sampai hasil final | `IGD-DEC-100` (a) |
| `AT-IGD-153` | Pesanan yang belum dimulai dibatalkan tanpa alasan | Gagal `400` | "Alasan pembatalan pesanan wajib diisi." | `IGD-DEC-100` (c) |
| `AT-IGD-154` | Kunjungan IGD ditutup sementara ada pesanan tanpa sikap | Gagal `400` | Menyebut **jumlah** pesanan yang belum ditentukan | `IGD-DEC-078` |
| `AT-IGD-155` | Kunjungan ditutup, ada pesanan berstatus `Continue` saja | Berhasil | `Continue` **bukan** pesanan tanpa sikap dan tidak menahan penutupan | `IGD-DEC-100` (a) |
| `AT-IGD-156` | Kunjungan IGD selesai; sistem memeriksa apakah ada pesanan yang dibatalkan otomatis | Berhasil | **Nol** pembatalan otomatis | `IGD-DEC-100` (d) |
| `AT-IGD-157` | Pasien dipindahkan; unit penerima menerima pasien tetapi **menolak** satu pesanan | Berhasil | Perpindahan pasien **tetap sah**; `physicalStatus` dan `handoverStatus` tidak bergeser | `IGD-DEC-102` (a), (d) |
| `AT-IGD-158` | Pesanan ditolak tanpa alasan | Gagal `400` | "Alasan penolakan pesanan wajib diisi." | `IGD-DEC-102` |
| `AT-IGD-159` | Kunjungan ditutup sementara ada pesanan `Rejected` yang belum diberi sikap pengganti | Gagal `409` | Menyebut pesanan mana | `IGD-DEC-102` (c) |
| `AT-IGD-160` | Pesanan yang ditolak diberi sikap pengganti `Handover` ke unit lain | Berhasil | Baris **baru** menunjuk baris lama; baris lama **tetap terbaca**, ditandai tidak berlaku | `IGD-DEC-102` (c) |
| `AT-IGD-161` | Pesanan radiologi yang dibuat di luar sistem didaftarkan tanpa `externalReference` | Gagal `400` | "Pesanan di luar sistem wajib menyertakan nomor rujukan dan uraiannya." | `IGD-DEC-103` |
| `AT-IGD-162` | Pesanan `External` didaftarkan lengkap, lalu ditetapkan sikapnya | Berhasil | `orderReferenceId` **kosong**, `externalReference` dan `orderDescription` terisi dan terbaca | `IGD-DEC-103` |
| `AT-IGD-163` | Sikap pesanan laboratorium ditetapkan petugas | Berhasil | Response dan layar **wajib** menyatakan sikap berasal dari petugas, **bukan** dari `LabOrder` | `IGD-DEC-101` |
| `AT-IGD-164` | Sikap ditetapkan tanpa pelaku atau waktu | Gagal `400` | Pelaku, waktu, dan alasan wajib tersimpan pada setiap penetapan | `IGD-DEC-101` |
| `AT-IGD-165` | `accept` dipanggil dua kali pada pesanan yang sama | Gagal `409` | `Accepted` bersifat final pada barisnya | State §6a.2 |

### Yang **tidak** dapat diuji otomatis

| Yang tidak diuji | Sebab |
| --- | --- |
| Apakah spesimen benar-benar sudah diambil | `LabOrder` tidak punya kolom status. `AT-IGD-152` menguji **pencatatan sikapnya**, bukan kebenaran keadaan di laboratorium — itu di luar jangkauan sistem sampai pemilik `LaboratoryManagement` melengkapinya |
| Apakah unit penerima benar-benar melanjutkan pesanan | Berada di luar batas modul IGD |

Dua baris ini dicatat supaya `AT-IGD-152` **tidak** dibaca sebagai bukti bahwa sistem tahu
keadaan laboratorium. Ia hanya membuktikan sikapnya tercatat beserta pelakunya.

---

## Encounter-first — 22 September 2026 (**Rencana (belum tersedia)**)

Dua puluh skenario baru, `AT-IGD-166` sampai `AT-IGD-185`, untuk `EPIC IGD-11`. Diturunkan dari
`IGD-DEC-139`, `142`…`148`, `150`…`154`; pesan mengikat ada di validation `0.8.0` §10. Proyek test backend
sudah dihapus (capability map `IGD-CAP-43`), sehingga skenario backend dibuktikan lewat **uji API dan
kueri baca-saja oleh pemilik**, bukan test otomatis. Tidak satu pun ditulis `UAT PASS` oleh agent.

| ID | Skenario | Hasil | Yang dibuktikan | Keputusan |
| --- | --- | --- | --- | --- |
| `AT-IGD-166` | Pasien beridentitas didaftarkan di loket IGD | Berhasil | Satu encounter Emergency, **nol** kunjungan IGD, nol baris antrean | `IGD-DEC-139`, `144` |
| `AT-IGD-167` | Daftar triage dibuka sesudah `AT-IGD-166` | Berhasil | Pasien tampil *Menunggu Triage* dengan label **"Terdaftar"**, bukan "Tiba"; satu baris per episode | `IGD-DEC-139` |
| `AT-IGD-168` | Pasien yang sedang *Menunggu Triage* didaftarkan lagi tanpa alasan | Gagal `409` | Pesan menyebut nomor encounter dan "Menunggu Triage"; jumlah encounter tidak bertambah | `IGD-DEC-139`, validation §10.1 aturan 3 |
| `AT-IGD-169` | Dua pendaftaran Emergency paralel untuk pasien yang sama | Satu `200`, satu `409` | Tepat satu episode terbuka di basis data; hitungan dicatat | `IGD-DEC-146` |
| `AT-IGD-170` | Pendaftaran kedua dengan alasan | Berhasil | Encounter kedua lahir; catatan override berisi alasan, pelaku token, waktu server, episode yang dilangkahi; tabel encounter tanpa ruas baru | `IGD-DEC-145` |
| `AT-IGD-171` | Unit/klinik IGD diset `IsQueueRequired = true`, lalu pasien didaftarkan | Berhasil | Nol baris `TrxQueue` untuk encounter itu | `IGD-DEC-144` |
| `AT-IGD-172` | Mulai Triage dengan waktu tiba 10 menit sebelum waktu terdaftar | Berhasil `201` | Kunjungan `WaitingForTriage`; waktu tiba tersimpan; sumber **Confirmed** | `IGD-DEC-139`, `147` |
| `AT-IGD-173` | Tangani Segera pada baris tanpa kunjungan | Berhasil `201` | Kunjungan `InTreatment` dalam **satu** permintaan tanpa isian; waktu tiba = waktu terdaftar, sumber **Fallback** | `IGD-DEC-143`, `147` |
| `AT-IGD-174` | Mulai Triage dan Tangani Segera dikirim paralel untuk encounter yang sama | `201` + `200` | Tepat satu kunjungan; status akhir `InTreatment` | `IGD-DEC-143` |
| `AT-IGD-175` | Waktu tiba dikoreksi menjadi sesudah mulai triage | Gagal `409` | Pesan menyebut "mulai triage" dan jamnya; waktu tiba tidak berubah | `IGD-DEC-152` |
| `AT-IGD-176` | Perawat menandai pasien pergi tanpa alasan | Gagal `400` | Alasan wajib | `IGD-DEC-142` |
| `AT-IGD-177` | Perawat menandai pasien pergi dengan alasan; lalu pasien kembali | Berhasil | Encounter `NoShow` + pelaku/waktu/alasan; hilang dari daftar triage; tidak di daftar tagihan; pendaftaran ulang **berhasil** membuat encounter baru | `IGD-DEC-142` |
| `AT-IGD-178` | Kunjungan diselesaikan | Berhasil | Kunjungan dan encounter `Completed` pada satu penyimpanan; catatan klinis belum bertanda tangan terkunci | `IGD-DEC-139` |
| `AT-IGD-179` | Kunjungan dihapus lunak; di data lain, kunjungan ber-encounter `Outpatient` diselesaikan | Berhasil | Encounter kunjungan terhapus **tetap terbuka**; encounter `Outpatient` tertaut **ikut** `Completed` | `IGD-DEC-148` TK-1, TK-2 |
| `AT-IGD-180` | Petugas memakai `PATCH …/status` pada encounter Emergency; lalu `PATCH …/cancel` pada encounter Emergency yang sudah punya kunjungan | Gagal `409` keduanya | Pesan validation §10.1 aturan 8 dan 9 | `IGD-DEC-153` |
| `AT-IGD-181` | `PUT /emergency-visits/{id}` mengubah `patientId` | Gagal `409` | Identitas kunjungan terkunci; ruas lain tetap dapat diubah | `IGD-DEC-154` |
| `AT-IGD-182` | Pratinjau rekonsiliasi di basis data berkandidat | Berhasil | Jumlah K1–K4 sama dengan kueri D; **nol** baris berubah | `IGD-DEC-148` |
| `AT-IGD-183` | Eksekusi dengan `expectedCount` basi; lalu eksekusi benar; lalu dibalik | `409`, `201`, `200` | Hanya K1 berubah; `CompletedAt` hanya dari bukti; pembalikan mengembalikan nilai sebelum dan melewati baris yang sudah berubah | `IGD-DEC-148` |
| `AT-IGD-184` | Pasien tanpa identitas didaftarkan dengan rekam pengganti lalu ditangani | Berhasil | Tanda vital, SOAP, pesanan lab dapat dibuat; kunjungan bertanda `IsUnknownPatient` dengan alias | `IGD-DEC-151` |
| `AT-IGD-185` | Klien memanggil `POST /patient-encounters` Emergency **tanpa** pra-cek untuk pasien berepisode terbuka | Gagal `409` | Penjaga ada di server, bukan di layar — celah (b) `IGD-OQ-093` tertutup | `IGD-DEC-139`, `146` |

### Yang tidak dapat diuji otomatis pada slice ini

| Yang tidak diuji | Sebab |
| --- | --- |
| Kebenaran waktu tiba yang diketik perawat | Sistem hanya dapat membuktikan batas dan penandanya, bukan bahwa pasien memang tiba jam itu |
| Kelayakan dokter jaga | `S7` ditahan `IGD-OQ-102`/`103` |
