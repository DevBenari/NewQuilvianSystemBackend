# PRD → MVP — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5`; **bagian 8 (encounter-first, `EPIC IGD-11`/`12`) ditambahkan 22 September 2026** |
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

---

## 8. Encounter-first — 22 September 2026

Bagian ini **menurunkan** dari dokumen yang sudah ada — `02-backend-architecture.md` §13, `03-frontend-architecture.md`
§13, `erd/data-dictionary.md` §6, kontrak API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8, integration
`0.4.0` §5, permission/audit `0.5.0` §7 — dan **tidak** menciptakan entity, status, permission, atau endpoint baru.
Status: `draft`; approval tetap tindakan pemilik.

**Catatan penomoran gelombang.** Bagian 5 dokumen ini masih memakai penomoran revisi 5; roadmap revisi 3
memakai penomoran `MVP-0`…`MVP-6` yang berbeda. Slice ini memakai **`MVP-7`** pada penomoran roadmap.

### 8.1 Batas slice

| Titik | Isi |
| --- | --- |
| **Mulai** | Pasien tiba dan petugas loket memilih/mendaftarkan pasiennya (termasuk rekam pengganti untuk pasien tanpa identitas) |
| **Akhir** | Episode IGD pasien berakhir dengan encounter dan kunjungan **konsisten**: pergi sebelum ditriage (NoShow), batal karena salah daftar, atau kunjungan selesai/batal yang ikut menutup encounter; ditambah encounter lama yang dibereskan lewat rekonsiliasi |

*Dalam satu kalimat:* pasien IGD selalu punya tepat satu episode terbuka yang lahir di loket, menjadi kunjungan
saat proses klinis dimulai, dan berakhir bersih — tidak ada encounter yang tertinggal terbuka.

### 8.2 Kemampuan `MUST HAVE`

| Kemampuan | ID kemampuan asal (capability map suplemen 3.2) | Disposisi |
| --- | --- | --- |
| Penjaga satu pasien satu episode di pintu encounter, serentak aman | `IGD-CAP-51`, `IGD-CAP-52`, `IGD-CAP-08` | `EXTEND` |
| Override pendaftaran ganda tercatat | `IGD-CAP-60` | `MISSING / NEW` |
| Encounter Emergency tanpa antrean | `IGD-CAP-59`, `IGD-CAP-09` | `EXTEND` |
| Daftar *Menunggu Triage* terpadu | `IGD-CAP-57`, `IGD-CAP-47` | `MISSING / NEW` |
| Kelahiran kunjungan: Mulai Triage dan Tangani Segera | `IGD-CAP-58` | `EXTEND` |
| Waktu tiba oleh perawat + penanda + batas koreksi | `IGD-CAP-61` | `REPAIR` → `EXTEND` |
| Pasien pergi sebelum ditriage | `IGD-CAP-55`, `IGD-CAP-56` | `EXTEND` |
| Encounter ikut ditutup bersama kunjungan | `IGD-CAP-53`, `IGD-CAP-54` | `MISSING / NEW` (pemicu) + `EXISTING / REUSE` (penguncian catatan) |
| Jalur umum Registrasi dibatasi untuk Emergency | `IGD-CAP-51` | `EXTEND` |
| Identitas kunjungan terkunci | `IGD-CAP-63` | `EXTEND` |
| Rekonsiliasi encounter historis | `IGD-CAP-62` | `MISSING / NEW` |
| Pasien tanpa identitas lewat rekam pengganti | `IGD-CAP-07` | `EXISTING / REUSE` (alur pasien baru yang ada) |

### 8.3 Kemampuan yang ditunda

| Ditunda | Alasan bersebab | Pengganti selama MVP |
| --- | --- | --- |
| Kelayakan dokter jaga dan override (`IGD-CAP-64`) | `IGD-OQ-102` (sumber roster) dan `IGD-OQ-103` (penyimpanan penanda) belum dijawab; angka E1–E3 belum ada | Penetapan dokter berjalan seperti hari ini (`BE-IGD-045`, `FE-IGD-027` ✅) — pilihan dari seluruh master dokter aktif |
| Layar rekonsiliasi admin | `IGD-DEC-162` (menjawab `IGD-OQ-107`) — tanpa layar | Admin menjalankan lewat API dengan hak akses khusus. **Koreksi B4, 22 September 2026:** Swagger hanya aktif di Development (`Program.cs:1330`); lingkungan lain lewat HTTP client bertoken |
| Penggabungan rekam pasien pengganti dengan rekam asli | Milik Master Patient (`IGD-OQ-098`, pemilik belum dipetakan) | Rekam pengganti tetap terpisah; kunjungan bertanda `IsUnknownPatient` untuk ditelusuri |
| Layar laporan override dan "pergi sebelum ditriage" | Slice kemudian (gate §5.2) | Datanya tersimpan; kueri baca-saja oleh pemilik |
| Tindak lanjut pasien berisiko yang pergi | Menunggu Clinical Governance (`IGD-DEC-150`) | Prosedur manual di luar sistem |

### 8.4 Epic dan functional requirement

#### `EPIC IGD-11` — Encounter-first dan episode yang selalu tertutup rapi · `EXTEND` + `MISSING / NEW`

| ID | Functional requirement | Disposisi | Bukti uji |
| --- | --- | --- | --- |
| `FR-IGD-069` | Pendaftaran IGD pasien beridentitas membuat encounter Emergency **tanpa** kunjungan IGD | `EXTEND` | `AT-IGD-166` |
| `FR-IGD-070` | Encounter tanpa kunjungan tampil *Menunggu Triage* pada satu daftar terpadu, berlabel waktu terdaftar | `MISSING / NEW` | `AT-IGD-167` |
| `FR-IGD-071` | Pendaftaran Emergency kedua ditolak di pintu encounter selama episode terbuka (klausa A atau B) | `EXTEND` | `AT-IGD-168`, `AT-IGD-185` |
| `FR-IGD-072` | Dua pendaftaran serentak menghasilkan tepat satu episode terbuka | `MISSING / NEW` | `AT-IGD-169` |
| `FR-IGD-073` | Pendaftaran ganda beralasan tercatat di catatan IGD; tabel encounter tanpa ruas baru | `MISSING / NEW` | `AT-IGD-170` |
| `FR-IGD-074` | Encounter Emergency tidak pernah membuat antrean | `EXTEND` | `AT-IGD-171` |
| `FR-IGD-075` | Mulai Triage melahirkan kunjungan menunggu triage dengan waktu tiba wajib dan dikonfirmasi | `EXTEND` | `AT-IGD-172` |
| `FR-IGD-076` | Tangani Segera melahirkan kunjungan sedang ditangani tanpa isian, dengan waktu tiba sementara | `EXTEND` | `AT-IGD-173` |
| `FR-IGD-077` | Mulai Triage/Tangani Segera idempoten dan aman serentak; Tangani Segera menang; status tidak mundur | `EXTEND` | `AT-IGD-174` |
| `FR-IGD-078` | Koreksi waktu tiba ditolak bila di masa depan atau sesudah peristiwa klinis pertama | `MISSING / NEW` | `AT-IGD-175` |
| `FR-IGD-079` | Pasien pergi sebelum ditriage ditandai perawat dengan alasan; final; tidak ditagih; pasien yang kembali didaftarkan ulang | `EXTEND` | `AT-IGD-176`, `AT-IGD-177` |
| `FR-IGD-080` | Kunjungan selesai/batal menutup encounter pada penyimpanan yang sama; catatan klinis terbuka terkunci | `MISSING / NEW` | `AT-IGD-178` |
| `FR-IGD-081` | Hapus lunak kunjungan tidak menutup encounter; encounter `Outpatient` tertaut ikut ditutup | `MISSING / NEW` | `AT-IGD-179` |
| `FR-IGD-082` | Jalur umum Registrasi menolak perubahan status encounter Emergency; batal hanya sebelum kunjungan lahir | `EXTEND` | `AT-IGD-180` |
| `FR-IGD-083` | Pasien dan encounter sebuah kunjungan IGD tidak dapat diganti lewat ubah kunjungan | `EXTEND` | `AT-IGD-181` |
| `FR-IGD-084` | Rekonsiliasi: pratinjau K1–K4 tanpa menulis; eksekusi hanya K1 dengan penjaga data basi; dapat dibalik | `MISSING / NEW` | `AT-IGD-182`, `AT-IGD-183` |
| `FR-IGD-085` | Pasien tanpa identitas dapat ditangani penuh lewat rekam pengganti | `EXISTING / REUSE` | `AT-IGD-184` |

**UAT berhasil.** Pak Rayyan didaftarkan 09.35 dan muncul *Menunggu Triage — Terdaftar 09.35*. Perawat menekan
Mulai Triage, mengoreksi waktu tiba menjadi 09.20, mengisi triage, dan pasien ditangani. Pukul 13.10 dokter
menyelesaikan kunjungan; encounter-nya ikut selesai, dan catatan SOAP yang lupa ditandatangani terkunci. Pukul
15.00 Pak Rayyan kembali karena keluhan baru dan dapat didaftarkan tanpa penolakan.

**UAT gagal.** (a) Petugas kedua mencoba mendaftarkan Pak Rayyan pukul 09.40 tanpa alasan → ditolak dengan nomor
encounter dan keterangan *Menunggu Triage*. (b) Bu Sari dipanggil tiga kali dan tidak ada → perawat menandai
*pergi sebelum ditriage* tanpa alasan → ditolak; dengan alasan → berhasil, dan Bu Sari tidak muncul di daftar
tagihan. (c) Perawat mencoba mengoreksi waktu tiba Pak Rayyan menjadi 10.00, padahal triage dimulai 09.42 → ditolak
dengan menyebut "mulai triage 09.42".

#### `EPIC IGD-12` — Kelayakan dokter jaga IGD · `OPEN DECISION`

Tidak masuk gelombang mana pun sampai `IGD-OQ-102` dan `IGD-OQ-103` dijawab. Prinsipnya sudah `approved`
(`IGD-DEC-141`): kandidat layak saja, validasi ulang di backend, override beralasan, tidak pernah buntu.

### 8.5 Urutan pengiriman

| Gelombang | Isi | Prasyarat |
| --- | --- | --- |
| `MVP-7` | `EPIC IGD-11` | `MVP-2` (satu pasien satu episode) dan `EPIC IGD-04` selesai — keduanya ✅ |

Urutan **di dalam** `MVP-7` yang mengikat (rinciannya milik `plan-module-delivery`):

1. Encounter ikut ditutup bersama kunjungan (`FR-IGD-080`, `081`) — supaya tidak ada encounter tertinggal baru.
2. Rekonsiliasi encounter lama (`FR-IGD-084`) — sesudah butir 1; **eksekusi** tiap lingkungan menunggu angka kueri D.
3. Penjaga di pintu encounter, serentak, override, tanpa antrean, pembatasan jalur Registrasi (`FR-IGD-071`…`074`, `082`) — sesudah butir 2.
4. Kelahiran kunjungan, waktu tiba, NoShow, kunci identitas (`FR-IGD-075`…`079`, `083`) — boleh paralel dengan butir 2–3.
5. Layar: daftar terpadu, lalu loket berhenti membuat kunjungan (`FR-IGD-069`, `070`) — **hanya sesudah** butir 3 aktif.

`POST-MVP`: `EPIC IGD-12` (setelah keputusannya), layar laporan, layar rekonsiliasi (bila `IGD-OQ-107` meminta).

### 8.6 Definition of Done — `EPIC IGD-11`

| No | Butir | Bukti yang diterima |
| ---: | --- | --- |
| 1 | `AT-IGD-166`…`185` dijalankan pemilik dan hasilnya tercatat per skenario | Laporan task beserta angka/badan respons — pernyataan tanpa lampiran dicatat apa adanya |
| 2 | Uji paralel `AT-IGD-169` dan `AT-IGD-174` menunjukkan tepat satu episode / satu kunjungan | Hitungan baris dicatat |
| 3 | Tiga migration punya `Down()` berpenjaga yang diuji di basis data terpisah | Catatan uji (pola `BE-IGD-048`) |
| 4 | Snapshot EF hanya bertambah blok tabel slice ini | `git diff` snapshot |
| 5 | Nol kolom baru pada `RegPatientEncounter`; nol baris baru `Program.cs` | `git diff --stat` |
| 6 | Kueri invariant "kunjungan berakhir, encounter terbuka" sesudah rilis = 0 | Kueri baca-saja pemilik |
| 7 | Kontrak `0.11.0`/`0.8.0`/`0.5.0`/`0.4.0`/`0.5.0` dinyatakan `approved` oleh pemilik dan hash dihitung ulang | `blueprint-manifest.md` — **terpenuhi 22 September 2026** (`IGD-DEC-157`; manifest bagian 2) |
| 8 | Perubahan pada berkas Registrasi disetujui pemilik Registrasi secara tertulis | **Belum dapat dijawab "ya"** — pemilik belum dipetakan; berjalan di bawah `IGD-DEC-135` |
| 9 | Butir wajib tinjau klinis (`IGD-DEC-150`) tercatat belum ditinjau | Manifest dan MODULE-STATUS |
| 10 | UAT oleh tim UAT terpisah | Status UAT dipisah; agent tidak pernah menulis `UAT PASS` |

### 8.7 Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir |
| --- | --- | :-: |
| `IGD-OQ-102`, `IGD-OQ-103` | Sumber roster dan penanda override dokter | **Ya** — hanya `EPIC IGD-12` (tidak di gelombang mana pun) |
| `IGD-OQ-104` | Waktu tiba hanya lewat `PATCH …/arrival-time` | **Dijawab `IGD-DEC-159`** (22 September 2026) — pilihan desain disahkan |
| `IGD-OQ-105` | Backend tidak menahan triage karena waktu tiba sementara | **Dijawab `IGD-DEC-160`** — disahkan; wajib tinjau Nursing authority |
| `IGD-OQ-106` | Letak ruas kunjungan non-waktu (Mulai Triage) | **Dijawab `IGD-DEC-161`** — ruas pindah ke Mulai Triage; keluhan utama tetap di loket |
| `IGD-OQ-107` | Rekonsiliasi tanpa layar | **Dijawab `IGD-DEC-162`** — tanpa layar |
| `IGD-OQ-108` | Encounter kedua hasil override belum dapat Mulai Triage selama kunjungan lama berjalan | Tidak — kontrak berlaku sampai pemilik mengubahnya; sebaiknya dijawab sebelum `FE-IGD-036` dirilis |
| Kueri D | Angka K1–K4 per lingkungan | Tidak untuk development; **Ya** untuk eksekusi rekonsiliasi |
| `IGD-OQ-098`, `IGD-OQ-099` | Pemilik Master Patient; penunjukan Clinical Governance / Nursing | Tidak |
| Pemilik Registrasi | Persetujuan tertulis atas titik sentuh | Tidak untuk development (`IGD-DEC-135`); **Ya** untuk DoD butir 8 |

**Kesimpulan.** `EPIC IGD-11` tidak memuat pertanyaan pemblokir untuk development. Dokumen ini tetap `draft`
sampai pemilik menyetujui bagian 8 beserta kontrak `0.11.0` dan kawan-kawannya; sesudah itu slice ini boleh
diteruskan ke `plan-module-delivery` final.

## 9. `EPIC IGD-13` — penutupan kunjungan lewat disposisi yang dilaksanakan

Gelombang **`MVP-8`**, sesudah `MVP-7` (encounter-first). Sumber: `IGD-DEC-163`…`169`; kemampuan asal
`IGD-CAP-66`, `IGD-CAP-67`, `IGD-CAP-68` (capability map suplemen 3.3).

**Status: `draft`** — menunggu approval pemilik.

### 9.1 Batas slice

| Butir | Isi |
| --- | --- |
| Titik mulai | Petugas menandai disposisi pasien sebagai dilaksanakan |
| Titik akhir | Kunjungan IGD tertutup beserta encounter-nya, atau tercatat menunggu penutupan dengan alasan yang terbaca |
| Di luar slice | Data lama (`IGD-DEC-167`); perubahan pada modul Bank Darah dan Laboratorium (`IGD-DEC-169`); rekonsiliasi `BE-IGD-052` |

### 9.2 Functional requirement

| ID | Requirement | Disposisi |
| --- | --- | --- |
| `FR-IGD-086` | Disposisi yang berpindah ke dilaksanakan memicu penutupan kunjungan, untuk semua jenis disposisi | `MISSING / NEW` |
| `FR-IGD-087` | Bila masih ada penahan, disposisi tetap tercatat dilaksanakan dan kunjungan ditandai menunggu penutupan beserta alasannya | `MISSING / NEW` |
| `FR-IGD-088` | Kunjungan yang menunggu penutupan tertutup otomatis pada aksi yang membereskan penahan terakhir, atas nama petugas yang melakukannya | `MISSING / NEW` |
| `FR-IGD-089` | Asal penutupan terbaca pada kunjungan — dari disposisi yang mana, atau manual | `MISSING / NEW` |
| `FR-IGD-090` | Pembatalan disposisi atas kunjungan yang sudah selesai ditolak | `EXTEND` — menumpang endpoint status disposisi yang sudah ada |
| `FR-IGD-091` | Petugas dapat menyaring daftar kunjungan untuk melihat yang menunggu penutupan beserta jumlahnya | `EXTEND` — menumpang daftar kunjungan yang sudah ada |

### 9.3 Skenario UAT

| # | Jalur | Langkah | Hasil yang diharapkan |
| ---: | --- | --- | --- |
| `AT-IGD-186` | Berhasil | Pasien tanpa penahan; tandai disposisi dilaksanakan | Kunjungan langsung selesai, encounter ikut tertutup, asal penutupan menunjuk disposisi itu |
| `AT-IGD-187` | Berhasil (menyusul) | Ada satu observasi aktif; tandai disposisi dilaksanakan, lalu tutup observasinya | Langkah pertama: disposisi berhasil, kunjungan menunggu penutupan dengan alasan observasi. Langkah kedua: kunjungan tertutup atas nama petugas yang menutup observasi |
| `AT-IGD-188` | Berhasil (kepergian) | Ada serah terima menggantung; terima serah terima itu | Kunjungan tertutup pada aksi penerimaan |
| `AT-IGD-189` | Gagal | Batalkan disposisi pada kunjungan yang sudah selesai | Ditolak dengan pesan yang menyuruh mendaftarkan episode baru |
| `AT-IGD-190` | Gagal (hilir) | Sesudah kunjungan tertutup, coba pesan pemeriksaan laboratorium baru pada encounter itu | Ditolak modul Laboratorium dengan pesannya sendiri — perilaku yang diterima `IGD-DEC-169` |
| `AT-IGD-191` | Daftar | Saring daftar kunjungan dengan "menunggu penutupan" | Hanya kunjungan berdisposisi dilaksanakan yang belum selesai yang tampil, masing-masing dengan alasan penahannya |

### 9.4 Definition of Done

| # | Butir | Cara menjawab |
| ---: | --- | --- |
| 1 | Keempat titik pemicu terpasang dan diuji satu per satu | Uji `AT-IGD-186`…`188` ditambah uji sikap pesanan |
| 2 | Kunjungan tidak pernah tertutup saat masih ada penahan | Uji `AT-IGD-187` langkah pertama |
| 3 | Pelaku penutupan susulan adalah petugas yang membereskan penahan, bukan sistem | Baca baris kunjungan sesudah `AT-IGD-187` |
| 4 | Asal penutupan terbaca dan dapat dibedakan dari penutupan manual | Bandingkan dua kunjungan: satu ditutup manual, satu lewat disposisi |
| 5 | Pembatalan disposisi atas kunjungan selesai ditolak | Uji `AT-IGD-189` |
| 6 | Saringan menunggu penutupan menampilkan jumlah dan alasan | Uji `AT-IGD-191` |
| 7 | Migration satu kolom diterapkan; `Down()` berpenjaga diuji di basis data terpisah | Catatan uji migration |
| 8 | Nol perubahan `Program.cs`; nol perubahan modul Bank Darah dan Laboratorium | `git diff --stat` |
| 9 | Build 0 error | Keluaran build milik pemilik |

### 9.5 Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir? |
| --- | --- | :-: |
| `IGD-OQ-110` | Jumlah encounter `IsActive = false` tanpa tanda berakhir pada data lama | Tidak — memengaruhi angka, bukan bentuk aturan |

Nol pertanyaan memblokir. Slice ini boleh diteruskan ke `plan-module-delivery` begitu pemilik menyetujui kontraknya.
