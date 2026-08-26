# PRD → MVP — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5` |
| Status | `draft` — **belum disetujui**; memuat pertanyaan memblokir |
| Commit diaudit | backend `f69e9e48`, frontend `96a91201` |
| Turunan dari | `02-backend-architecture.md`, `03-frontend-architecture.md`, `erd/`, `contracts/`, `testing/` |

Seluruh entity, status, hak akses, dan endpoint yang disebut dokumen ini **sudah tercatat**
pada dokumen di atas. Tidak ada entity yang lahir dari epic, layar, atau nama task.

---

## 1. Batas MVP

**Titik mulai.** Pasien tiba di IGD dan petugas membuka layar pendaftaran.

**Titik akhir.** Kunjungan IGD berstatus `Completed`, dengan seluruh kewajiban klinisnya
tuntas: keputusan tindak lanjut ditetapkan, observasi selesai, kepergian pasien tercatat
sampai tiba di unit tujuan, serah terima diajukan, dan setiap pesanan yang belum selesai sudah
diberi sikap.

**Yang berada di luar titik akhir.** Pembentukan episode rawat inap, penempatan tempat tidur,
dan perawatan di bangsal. Ketiganya milik `RWI-BP-001`.

### 1.1 Ringkasan cakupan dalam satu kalimat

Satu pasien dapat dilayani di IGD dari pendaftaran sampai kepergiannya tercatat tuntas, dengan
riwayat klinis yang tidak pernah hilang dan pemilik klinis yang tidak pernah kosong — tanpa
episode rawat inap, tanpa hasil pemeriksaan penunjang, dan tanpa catatan pemberian obat.

---

## 2. Kemampuan `MUST HAVE`

| No | Kemampuan | ID capability map | Disposisi |
| ---: | --- | --- | --- |
| 1 | Pendaftaran pasien IGD termasuk pasien tanpa identitas | `IGD-CAP-01`, `02`, `07` | `EXISTING / REUSE` |
| 2 | Kunjungan IGD dapat dibedakan dari kunjungan poliklinik | `IGD-CAP-03` | `EXTEND` |
| 3 | Kelas pasien IGD ditetapkan sistem | `IGD-CAP-05` | `EXTEND` |
| 4 | Pencegahan dua episode IGD aktif untuk satu pasien | `IGD-CAP-08` | `MISSING / NEW` |
| 5 | Triase dan penilaian ulang beserta riwayatnya | `IGD-CAP-10`, `11`, `12`, `13` | `EXISTING / REUSE` |
| 6 | Status kunjungan tidak dapat mundur | `IGD-CAP-14` | `EXTEND` |
| 7 | Riwayat penugasan dokter pemeriksa | `IGD-CAP-15`, `16` | `MISSING / NEW` |
| 8 | Pengkajian keperawatan IGD dapat disimpan | `IGD-CAP-17` | `OPEN DECISION` |
| 9 | Diagnosis, tindakan, dan resep IGD dapat disimpan | `IGD-CAP-18`, `19`, `20`, `31` | `OPEN DECISION` |
| 10 | Tanda vital dan catatan perkembangan | `IGD-CAP-21`, `22` | `EXISTING / REUSE` |
| 11 | Riwayat versi catatan klinis | `IGD-CAP-24` | `OPEN DECISION` |
| 12 | Observasi dan resusitasi | `IGD-CAP-26`, `28` | `EXISTING / REUSE` |
| 13 | Pemesanan laboratorium | `IGD-CAP-29` | `EXISTING / REUSE` |
| 14 | Keputusan tindak lanjut beserta penanda penutup kunjungan | `IGD-CAP-33`, `34` | `EXTEND` |
| 15 | Catatan kepergian dua rangkaian status | `IGD-CAP-36` | `EXTEND` |
| 16 | Riwayat kejadian kepergian, koreksi, dan pembalikan | `IGD-CAP-36` | `MISSING / NEW` |
| 17 | Serah terima SBAR beserta daftar sikap pesanan | `IGD-CAP-36` | `MISSING / NEW` |
| 18 | Pemilik klinis pasien selalu terisi | `IGD-CAP-36` | `MISSING / NEW` |
| 19 | Kewenangan yang mengenal unit pelayanan | `IGD-CAP-41` | `MISSING / NEW` |
| 20 | Gerbang penutupan kunjungan | `IGD-CAP-35` | `EXTEND` |

Empat kemampuan berdisposisi `OPEN DECISION`. Sesuai kontrak, keempatnya **tidak masuk
gelombang pengiriman mana pun** sampai keputusannya turun.

---

## 3. Kemampuan yang ditunda

| Kemampuan | ID | Alasan | Pengganti selama MVP |
| --- | --- | --- | --- |
| Status dan hasil pemeriksaan laboratorium | `IGD-CAP-29` | `LabOrder` tidak punya kolomnya; pemiliknya belum ditunjuk — `IGD-DEC-088` | Perawat menuliskan pemeriksaan yang hasilnya belum keluar pada bagian "yang harus dilanjutkan" dalam SBAR |
| Pemesanan radiologi | `IGD-CAP-30` | Modulnya tidak ada | Pemesanan di luar sistem |
| Catatan pemberian obat | `IGD-CAP-32` | Menyentuh Pharmacy Management; pemiliknya belum ditunjuk | Pemberian dicatat pada catatan perkembangan |
| Episode rawat inap dan penempatan tempat tidur | `IGD-CAP-39` | Milik `RWI-BP-001`, belum diimplementasikan | Petugas admisi membuka admisi rawat inap secara manual, sesuai `RWI-CAP-038` |
| Pembaruan realtime daftar pantau | `IGD-CAP-45` | `IGD-TRQ-07`, `LATER SLICE` | Muat ulang berkala oleh petugas |
| Bentuk terstruktur primary survey ABCDE | `IGD-GAP-027` | Enam kolom ringkasan teks sudah memenuhi `IGD-DEC-057` | Ringkasan teks pada penilaian triase |
| Jejak audit perubahan data master | — | `IGD-DEC-080` sengaja tidak memperluas ke tabel non-klinis | Tidak ada. Keterbatasan disadari |

---

## 4. Epic dan functional requirement

### `EPIC IGD-01` — Kunjungan IGD dapat dibedakan · `EXTEND`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-001` | Kunjungan IGD tersimpan dengan `EncounterType = Emergency` | `AT-IGD-074` |
| `FR-IGD-002` | Kunjungan IGD bertipe lain ditolak pada **dua** jalur validasi | `AT-IGD-075`, `AT-IGD-076` |
| `FR-IGD-003` | Kunjungan IGD lama diperbaiki tipenya; jumlahnya sama dengan jumlah kunjungan IGD yang punya encounter | `AT-IGD-074` |
| `FR-IGD-004` | Kunjungan poliklinik tidak ikut berubah | `AT-IGD-081` |
| `FR-IGD-005` | Kelas pasien IGD diambil dari master bertanda `IsForEmergency` dan `IsDefault` | `AT-IGD-079`, `AT-IGD-080` |
| `FR-IGD-006` | Master kelas pasien IGD yang kosong atau ganda menolak pendaftaran dengan pesan yang menyebut sebabnya | `AT-IGD-077`, `AT-IGD-078` |

**UAT berhasil.** Petugas mendaftarkan Ny. Sari di IGD. Kunjungan tersimpan bertipe Gawat
Darurat dengan kelas pasien terisi. Laporan kunjungan rawat jalan hari itu tidak memuat
Ny. Sari.

**UAT gagal.** Master kelas pasien IGD belum diisi. Pendaftaran ditolak dengan pesan yang
menyebut master mana yang kurang, bukan tersimpan dengan kelas kosong.

### `EPIC IGD-02` — Satu pasien satu episode IGD · `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-007` | Pendaftaran kedua ditolak selama kunjungan IGD sebelumnya belum `Completed` atau `Cancelled` | `AT-IGD-082` |
| `FR-IGD-008` | Pesan penolakan memuat nomor kunjungan dan waktu kedatangannya | `AT-IGD-082` |
| `FR-IGD-009` | Pasien yang kunjungan sebelumnya sudah selesai dapat didaftarkan kembali | `AT-IGD-083` |
| `FR-IGD-010` | Jalan keluar beralasan tersedia dan pemakaiannya tercatat | `AT-IGD-084` |
| `FR-IGD-011` | Pasien tanpa identitas tidak tertolak aturan ini | `AT-IGD-085` |
| `FR-IGD-012` | Aturan ini tidak menahan penanganan klinis | `AT-IGD-085` |

**UAT berhasil.** Petugas kedua mencoba mendaftarkan Budi yang sudah terdaftar 5 menit lalu.
Sistem menolak, menampilkan nomor kunjungan yang ada, dan petugas membukanya langsung.

**UAT gagal.** Budi benar-benar datang kedua kali untuk keluhan berbeda, sementara kunjungan
pertamanya lupa ditutup. Petugas memakai jalan keluar beralasan; alasannya tersimpan dan
muncul pada daftar pantau.

### `EPIC IGD-03` — Status kunjungan tidak dapat mundur · `EXTEND`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-013` | Penilaian ulang tidak mengembalikan status kunjungan ke `Triaged` | `AT-IGD-086` |
| `FR-IGD-014` | Triase tidak dapat diselesaikan pada kunjungan yang sudah tertutup | `AT-IGD-087`, `AT-IGD-088` |
| `FR-IGD-015` | Seluruh penulisan status kunjungan melewati pemeriksaan transisi | `AT-IGD-089` |

**UAT berhasil.** Ny. Sari sedang ditangani. Perawat menilainya ulang karena kondisinya
memburuk. Status kunjungan tetap sedang ditangani.

**UAT gagal.** Petugas mencoba menyelesaikan penilaian lama pada kunjungan yang sudah ditutup.
Ditolak; kunjungan tidak terbuka kembali.

### `EPIC IGD-04` — Riwayat penugasan dokter · `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-016` | Penetapan dokter kedua lewat endpoint penetapan ditolak | `AT-IGD-124` |
| `FR-IGD-017` | Pengalihan menutup baris lama dan membuka baris baru | `AT-IGD-125` |
| `FR-IGD-018` | Dokter penanggung jawab pada waktu tertentu dapat dijawab | `AT-IGD-126` |
| `FR-IGD-019` | Tepat satu dokter aktif per kunjungan, dijaga basis data | `AT-IGD-127` |
| `FR-IGD-020` | Nilai efektif pada kunjungan selalu sama dengan dokter aktif | `AT-IGD-128` |
| `FR-IGD-021` | Pengalihan menuntut alasan | `AT-IGD-129` |

**UAT berhasil.** dr. Budi menyerahkan Ny. Sari kepada dr. Sita saat pergantian shift dengan
alasan tertulis. Keduanya terbaca pada riwayat.

**UAT gagal.** Dua petugas menetapkan dokter bersamaan. Satu ditolak; tidak pernah ada dua
dokter aktif.

### `EPIC IGD-05` — Kepergian pasien dua rangkaian · `EXTEND` + `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-022` | Rangkaian fisik dan rangkaian dokumen berjalan sendiri-sendiri | `AT-IGD-090` |
| `FR-IGD-023` | Kedatangan memindahkan pemilik klinis ke unit penerima | `AT-IGD-091` |
| `FR-IGD-024` | Kedatangan tidak otomatis menerima dokumen | `AT-IGD-092` |
| `FR-IGD-025` | Dokumen tidak dapat diterima sebelum pasien berangkat | `AT-IGD-093` |
| `FR-IGD-026` | Pemilik klinis tetap IGD selama pasien di perjalanan | `AT-IGD-094` |
| `FR-IGD-027` | Sistem selalu menjawab pemilik unit dengan tepat satu nama | `AT-IGD-095` |
| `FR-IGD-028` | Waktu berangkat dan waktu tiba benar-benar terisi | `AT-IGD-097` |
| `FR-IGD-029` | Tidak ada kolom tempat tidur pada catatan kepergian | `AT-IGD-098` |
| `FR-IGD-030` | Tidak ada jalur IGD yang mengubah keadaan tempat tidur | `AT-IGD-099` |
| `FR-IGD-031` | Route lama menjawab dengan pesan yang menyebut penggantinya | `AT-IGD-100` |

**UAT berhasil.** Ny. Sari berangkat 23.45, tiba 23.52. Perawat bangsal mencatat kedatangan;
tanggung jawab berpindah. Dokumen serah terima masih ditinjau, dan layar menampilkannya
sebagai keadaan normal.

**UAT gagal.** Perawat bangsal lain mencoba mencatat kedatangan untuk unit yang bukan
tempatnya bertugas. Ditolak dengan penjelasan.

### `EPIC IGD-06` — Kedatangan susulan dan koreksi · `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-032` | Waktu sebenarnya dan waktu server tersimpan terpisah | `AT-IGD-101` |
| `FR-IGD-033` | Pencatatan susulan menuntut rujukan catatan manual | `AT-IGD-102` |
| `FR-IGD-034` | Waktu kejadian tidak boleh di masa depan | `AT-IGD-103` |
| `FR-IGD-035` | Koreksi tidak menimpa kejadian lama | `AT-IGD-104` |
| `FR-IGD-036` | Pembalikan menuntut persetujuan orang kedua yang berbeda | `AT-IGD-105`, `AT-IGD-106` |
| `FR-IGD-037` | Koreksi memberi tahu unit dan penulis catatan turunan | `AT-IGD-107` |
| `FR-IGD-038` | Catatan turunan diberi penanda, isinya tidak diubah | `AT-IGD-108` |
| `FR-IGD-039` | Kegagalan pemberitahuan tidak membatalkan koreksi | `AT-IGD-109` |

**UAT berhasil.** Sistem mati saat Ny. Sari tiba 14.05. Pukul 14.40 petugas mencatat susulan
dengan rujukan formulir manual. Keduanya tersimpan; lama tinggal dihitung dari 14.05.

**UAT gagal.** Petugas mencatat kedatangan untuk pasien yang salah, lalu mengajukan
pembalikan sendirian. Ditolak sampai petugas kedua menyetujui.

### `EPIC IGD-07` — Serah terima SBAR dan sikap pesanan · `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-040` | Empat bagian SBAR wajib terisi atau ditandai tidak dapat diisi | `AT-IGD-110`, `AT-IGD-111` |
| `FR-IGD-041` | Tiga bagian otomatis diisi sistem | `AT-IGD-112` |
| `FR-IGD-042` | Tingkat kegawatan diambil dari penilaian selesai terakhir | `AT-IGD-113` |
| `FR-IGD-043` | Dokumen belum lengkap tidak menahan tindakan fisik | `AT-IGD-114` |
| `FR-IGD-044` | Dokumen belum lengkap muncul pada daftar pantau | `AT-IGD-115` |
| `FR-IGD-045` | Daftar sikap memuat obat dan tindakan | `AT-IGD-116` |
| `FR-IGD-046` | Keterbatasan penunjang dinyatakan **di layar** | `AT-IGD-117` |
| `FR-IGD-047` | Dokumen tidak dapat diajukan selama ada pesanan tanpa sikap | `AT-IGD-118` |
| `FR-IGD-048` | Pembatalan pesanan menuntut alasan | `AT-IGD-119` |
| `FR-IGD-049` | Pesanan yang diteruskan muncul sebagai tugas di unit penerima | `AT-IGD-120` |
| `FR-IGD-050` | Perubahan sikap tercatat sebagai koreksi | `AT-IGD-121` |
| `FR-IGD-051` | Kunjungan tidak dapat diselesaikan bila ada pesanan tanpa sikap | `AT-IGD-122` |
| `FR-IGD-052` | Tagihan tidak diperiksa saat penutupan klinis | `AT-IGD-123` |

**UAT berhasil.** Sebelum Ny. Sari berangkat, perawat memberi sikap pada antibiotik dan
kateter: keduanya diteruskan. Bangsal menerima kedua tugas itu.

**UAT gagal.** Perawat mengajukan dokumen dengan bagian Recommendation kosong. Ditolak dengan
menyebut bagian mana.

### `EPIC IGD-08` — Kewenangan unit · `MISSING / NEW`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-053` | Petugas unit lain ditolak mencatat kedatangan | `AT-IGD-130` |
| `FR-IGD-054` | Petugas unit tujuan diterima | `AT-IGD-131` |
| `FR-IGD-055` | Penugasan yang sudah berakhir tidak memberi kewenangan | `AT-IGD-132` |
| `FR-IGD-056` | Penugasan unit tidak dengan sendirinya memberi kemampuan klinis | `AT-IGD-133` |
| `FR-IGD-057` | Kemampuan tidak melewati batas penugasan unit | `AT-IGD-134` |
| `FR-IGD-058` | Mesin hak akses tidak berubah perilakunya | `AT-IGD-135` |
| `FR-IGD-059` | Pelayanan klinis darurat tidak pernah diblokir | `AT-IGD-136` |

**UAT berhasil.** Perawat Melati mencatat kedatangan Ny. Sari. Diterima.

**UAT gagal.** Perawat Anggrek mencoba hal yang sama. Ditolak dengan penjelasan bahwa ia tidak
bertugas di unit tujuan.

### `EPIC IGD-09` — Pengkajian klinis IGD · `OPEN DECISION`

Epic ini **tidak masuk gelombang pengiriman mana pun**. Ia menunggu penunjukan pemilik
`ClinicalManagement` dan `PharmacyManagement`.

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-060` | Pengkajian keperawatan IGD tersimpan tanpa antrean | `AT-IGD-147` |
| `FR-IGD-061` | Daftar antrean poliklinik tidak tercemar | `AT-IGD-148` |
| `FR-IGD-062` | Perilaku rawat jalan tidak berubah | `AT-IGD-149` |
| `FR-IGD-063` | Diagnosis, tindakan, dan resep IGD tersimpan | `AT-IGD-150` |
| `FR-IGD-064` | Koreksi catatan klinis menambah baris, tidak menimpa | `AT-IGD-137` sampai `AT-IGD-142` |

### `EPIC IGD-10` — Penghubung kunjungan · `EXTEND`

| ID | Functional requirement | Bukti uji |
| --- | --- | --- |
| `FR-IGD-065` | Kunjungan tidak dapat menunjuk dirinya sendiri | `AT-IGD-143` |
| `FR-IGD-066` | Rangkaian tidak boleh membentuk lingkaran | `AT-IGD-144` |
| `FR-IGD-067` | Kunjungan tanpa asal tetap sah | `AT-IGD-145` |
| `FR-IGD-068` | Kunjungan lama tidak berubah | `AT-IGD-146` |

**UAT berhasil.** Kunjungan rawat inap Ny. Sari menunjuk kunjungan IGD-nya. Riwayat terbaca
utuh.

**UAT gagal.** Percobaan membuat rangkaian melingkar ditolak.

---

## 5. Urutan pengiriman

Gelombang, bukan tanggal. Epic `OPEN DECISION` tidak muncul di sini.

| Gelombang | Isi | Prasyarat |
| --- | --- | --- |
| `MVP-0` | Master kelas pasien IGD; pemetaan unit ke simpul organisasi; `EPIC IGD-03` | Tidak ada. Perbaikan status kunjungan tidak butuh keputusan siapa pun |
| `MVP-1` | `EPIC IGD-01`, `EPIC IGD-02` | `MVP-0` selesai — master kelas pasien **wajib** ada lebih dulu |
| `MVP-2` | `EPIC IGD-04`, `EPIC IGD-10` | `MVP-1` |
| `MVP-3` | `EPIC IGD-05`, `EPIC IGD-06` | `MVP-1`; angka `IGD-UNK-03` diketahui |
| `MVP-4` | `EPIC IGD-07` | `MVP-3` |
| `MVP-5` | `EPIC IGD-08` | `IGD-OQ-071` terjawab; data penugasan terisi |
| `POST-MVP` | `EPIC IGD-09`; pelengkapan `LabOrder`; catatan pemberian obat; realtime | Pemilik modul ditunjuk |

`MVP-0` sengaja didahulukan karena satu-satunya isinya yang menyangkut kode — perbaikan status
kunjungan — **tidak membutuhkan keputusan siapa pun** dan menutup cacat data yang sedang
berjalan.

---

## 6. Definition of Done

Setiap butir dijawab "ya" atau "belum" beserta buktinya.

| No | Butir | Bukti yang diterima |
| ---: | --- | --- |
| 1 | Seluruh functional requirement gelombangnya punya test yang lulus | Keluaran `dotnet test` dan `npm run test:unit` |
| 2 | Test regresi jalur rawat jalan tersedia untuk setiap perubahan lintas modul | Berkas test beserta keluarannya |
| 3 | Migration punya langkah mundur yang tertulis dan sudah diuji di basis data terpisah | Catatan hasil uji |
| 4 | Tidak ada endpoint yang menghapus permanen catatan klinis | Hasil penelusuran kode |
| 5 | Tidak ada isi klinis yang masuk berkas log | Contoh keluaran log |
| 6 | Setiap tahap kepergian punya pemilik klinis tepat satu | `AT-IGD-095` lulus |
| 7 | Layar menyatakan keterbatasan penunjang | Tangkapan layar |
| 8 | Data master gelombangnya sudah terisi | Kueri jumlah baris |
| 9 | Kontrak yang berubah sudah dinaikkan versinya dan hash-nya dihitung ulang | `blueprint-manifest.md` |
| 10 | Perubahan pada modul milik pihak lain sudah disetujui pemiliknya secara tertulis | Catatan persetujuan beserta nama |

Butir 10 **belum dapat dijawab "ya"** untuk gelombang mana pun yang menyentuh
`ClinicalManagement`, `PharmacyManagement`, `Registration Management`, atau `Master Data`.

---

## 7. Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir |
| --- | --- | :-: |
| `IGD-OQ-068` | Apakah penafsiran dua kolom status ditambah tabel kejadian dapat diterima? | **Ya** — `EPIC IGD-05`, `06` |
| `IGD-OQ-069` | Apakah dua kolom pengaturan mati benar dicabut? | Tidak |
| `IGD-OQ-070` | Apakah penggantian nama `TrxEmergencyTransfer` diterima? | **Ya** — `EPIC IGD-05` |
| `IGD-OQ-071` | Perilaku unit yang belum dipetakan ke simpul organisasi | **Ya** — `EPIC IGD-08` |
| `IGD-OQ-067` | Kewenangan sementara perawat bantuan | Tidak — dapat menyusul |
| `IGD-OQ-037` | Break-glass | Tidak untuk MVP |
| `IGD-OQ-038` | Approver bernama roadmap | **Ya** untuk approval, bukan untuk desain |
| `IGD-UNK-01` … `IGD-UNK-07` | Tujuh hal yang hanya dapat dijawab kueri basis data | **Ya** untuk `MVP-1`, `MVP-3`, `MVP-5` |
| `DEC-INP-001` setara | Penunjukan pemilik `ClinicalManagement` dan `PharmacyManagement` | **Ya** — `EPIC IGD-09` |

**Dokumen ini memuat pertanyaan memblokir yang belum terjawab.** Sesuai kontrak, ia tetap
boleh berstatus `draft`, tetapi **tidak boleh** diteruskan ke `/qv-plan` sebelum
`IGD-OQ-068`, `IGD-OQ-070`, dan `IGD-OQ-071` dijawab.

Pengecualiannya: **`MVP-0` tidak bergantung pada satu pun pertanyaan di atas** dan dapat
direncanakan lebih dulu.
