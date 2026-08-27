# Laporan Perubahan — `BE-IGD-015` dan `FE-IGD-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-015` (nosokomial), `FE-IGD-011` (layar pengkajian) |
| Slice | Tambahan setelah roadmap revisi 1 |
| Repository | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Contract version | API `0.2.0` — kontrak IGD tidak berubah; kontrak nosokomial **baru** |
| Tanggal | 21 Agustus 2026 |
| **Status** | **Kode selesai, lint bersih, build lulus, 38 unit test lulus. Migrasi nosokomial sudah diterapkan dan terverifikasi; alur simpan lewat layar belum dijalankan sungguhan** |

---

## 1. Jawaban atas pertanyaan pemisahan menu

Pertanyaannya: apakah pengkajian berada dalam satu menu dengan daftar triage, atau menu
tersendiri di sidebar.

**Dipisahkan menjadi menu tersendiri.** Alasannya bukan tata letak, melainkan karena keduanya
adalah pekerjaan yang berbeda:

| | Triage Pasien | Pengkajian Pasien |
| --- | --- | --- |
| Kapan dikerjakan | Sekali, saat pasien tiba | Berulang, selama pasien berada di IGD |
| Menjawab pertanyaan | Seberapa cepat pasien harus dilayani | Bagaimana asuhan keperawatannya berjalan |
| Daftar pasiennya | Yang **belum** dinilai | Yang **sudah** dinilai dan sedang ditangani |
| Pelakunya | Perawat triage | Perawat pelaksana asuhan |

> **Contoh akibat bila disatukan:** pukul 20.00 IGD ramai. Perawat triage membuka layar dan
> menemukan daftar berisi seluruh pasien IGD, termasuk yang sudah ditangani sejak dua jam lalu.
> Pasien yang baru tiba dan belum dinilai tenggelam di antara mereka — padahal justru pasien
> itulah yang sedang menunggu keputusan prioritas. Satu layar yang melayani dua pekerjaan
> membuat pekerjaan yang paling mendesak paling sulit ditemukan.

Alurnya karena itu: **menu Pengkajian Pasien → daftar pasien → tombol Pemeriksaan → layar
pemeriksaan**, persis seperti yang diminta.

---

## 2. Keadaan pendaftaran dan triage sebelum melanjutkan

Pertanyaan kedua: apakah pendaftaran dan triage sudah selesai.

| Bagian | Keadaan | Catatan |
| --- | --- | --- |
| `FE-IGD-001` Pendaftaran | Kriteria terpenuhi, terkunci 6 unit test | Lihat laporan `fe-igd-001-be-igd-003-...` |
| `BE-IGD-002`–`007` Triage | Kriteria terpenuhi di kode | — |
| `FE-IGD-003`, `FE-IGD-004` Triage | Kriteria terpenuhi, terkunci 12 unit test | Termasuk penilaian ulang dan kategori Hitam |

Yang **belum** terbukti, dan tetap berlaku sesudah pekerjaan ini:

1. Alur simpan belum pernah dijalankan sungguhan terhadap basis data.
2. Solution backend belum memiliki proyek test, sehingga tidak satu pun `AT-IGD-*` dapat
   dijalankan.
3. Target waktu level 2 sampai 5 di basis data tim berisi angka baseline ATS yang belum
   disahkan SOP MMC.

Ketiganya tidak menghalangi pembangunan layar pengkajian, tetapi menghalangi pernyataan
"modul IGD siap dipakai".

---

## 3. `BE-IGD-015` — Tabel dan endpoint nosokomial

### 3.1 Mengapa entitasnya benar-benar belum ada

Pencarian `nosokomial`, `nosocomial`, dan `healthcare associated` di seluruh repository backend
menghasilkan **nol berkas**. Berbeda dari pengkajian klinis, catatan terintegrasi, observasi,
tindak lanjut, dan transfer yang seluruhnya sudah tersedia, infeksi nosokomial memang belum
punya rumah sama sekali.

### 3.2 Proses bisnis yang dilayani

**Tujuan.** Mencatat kejadian infeksi yang didapat pasien selama menerima pelayanan, sehingga
tim pengendali infeksi (PPI) dapat menelaahnya dan rumah sakit dapat menghitung angka mutunya.

**Pelaku.**

| Pelaku | Kewenangan |
| --- | --- |
| Perawat pelaksana | Mencatat temuan kejadian |
| Tim PPI (IPCN) | Menelaah, mengonfirmasi, atau menyatakan bukan infeksi terkait pelayanan |
| Clinical governance | Mengesahkan daftar jenis infeksi dan kriteria penetapannya |

**Pemicu.** Perawat menemukan tanda infeksi pada pasien yang sedang dirawat, misalnya kemerahan
dan nyeri di sekitar lokasi infus.

**Langkah utama.**

1. Perawat membuka tab Nosokomial pada layar pengkajian pasien.
2. Perawat menekan Catat Kejadian, lalu mengisi jenis infeksi, waktu munculnya gejala,
   kriteria yang terpenuhi, dan kaitannya dengan pemakaian alat.
3. Sistem membuat nomor catatan otomatis berpola `NOS-<tanggal>-<urutan>`.
4. Catatan tersimpan dengan status **Dicurigai**.
5. Tim PPI menelaah, lalu menetapkan statusnya.

**Aturan bisnis.**

**Aturan A — Catatan baru selalu berstatus Dicurigai.** Perawat yang menemukan kejadian tidak
menetapkan sendiri bahwa itu infeksi nosokomial. Penetapan adalah wewenang tim PPI, karena
angkanya menjadi indikator mutu rumah sakit.

**Aturan B — Menyatakan bukan infeksi terkait pelayanan wajib mengisi alasan.**

> **Contoh:** pasien tiba dengan luka bernanah pada hari pertama, dan setelah ditelusuri luka
> itu sudah ada sejak sebelum masuk rumah sakit. Kejadian dinyatakan bukan infeksi terkait
> pelayanan dengan alasan "luka sudah bernanah saat pasien tiba, dibuktikan foto triase".
> Tanpa alasan tertulis, keputusan mengeluarkan kejadian dari hitungan mutu tidak dapat
> ditinjau siapa pun.

**Aturan C — Teratasi hanya setelah dikonfirmasi.** Kejadian yang masih Dicurigai tidak dapat
langsung dinyatakan teratasi; urutannya Dicurigai → Terkonfirmasi → Teratasi.

**Aturan D — Batas 48 jam menentukan asal infeksi, dan hasilnya disimpan.**

> **Contoh:** pasien mulai dirawat 1 Agustus pukul 08.00. Gejala infeksi saluran kemih muncul
> 3 Agustus pukul 14.00, yaitu 54 jam kemudian. Karena melewati 48 jam, kejadian dikelompokkan
> sebagai **didapat selama perawatan**. Angka 54 dan tanggal mulai dirawat disimpan sebagai
> salinan pada catatan itu, bukan dihitung ulang setiap laporan dibuat — sehingga koreksi
> tanggal masuk di kemudian hari tidak diam-diam mengubah kesimpulan yang sudah diambil.
>
> Bila petugas memilih sendiri asal infeksinya, pilihan petugas menang. Mereka mengetahui hal
> yang tidak terbaca dari tanggal, misalnya pasien rujukan yang sudah terinfeksi di fasilitas
> sebelumnya.

**Aturan E — Catatan yang sudah ditutup tidak dapat diubah isinya.** Status Teratasi, Bukan
infeksi terkait pelayanan, dan Dibatalkan mengunci isinya; membuka kembali statusnya adalah
langkah tersendiri.

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Catat kejadian | `Dicurigai` | Perawat pelaksana | Pasien, jenis infeksi, dan waktu gejala terisi |
| `Dicurigai` | Konfirmasi | `Terkonfirmasi` | Tim PPI | — |
| `Dicurigai` | Nyatakan bukan infeksi pelayanan | `Bukan infeksi terkait pelayanan` | Tim PPI | Alasan wajib diisi |
| `Terkonfirmasi` | Nyatakan teratasi | `Teratasi` | Tim PPI | — |
| `Dicurigai` | Batalkan | `Dibatalkan` | Tim PPI | — |

**Jalur tidak normal.**

| Kejadian | Yang terjadi | Yang dilihat pengguna |
| --- | --- | --- |
| Waktu gejala lebih awal daripada waktu mulai dirawat | Ditolak | "Waktu mulai dirawat tidak boleh lebih akhir daripada waktu munculnya gejala." |
| Memilih jenis Lainnya tanpa menyebutkan | Ditolak | "Sebutkan jenis infeksinya ketika memilih Lainnya." |
| Menandai terkait alat tanpa menyebut alatnya | Ditolak | "Nama alat wajib diisi ketika infeksi dikaitkan dengan pemakaian alat." |
| Mengubah catatan yang sudah ditutup | Ditolak (409) | "Catatan yang sudah ditutup tidak dapat diubah. Buka kembali statusnya lebih dulu." |
| Menetapkan status yang sama dengan status sekarang | Ditolak (409) | "Status kejadian sudah bernilai sama, tidak ada yang perlu diubah." |

### 3.3 Dokumentasi API

#### Health Services / Clinical Management / Nosocomial Infection

Base URL: `api/v1/health-services/clinical-management/nosocomial-infections`

Seluruh endpoint memerlukan pengguna yang sudah masuk.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil pilihan jenis infeksi, status, dan asal infeksi untuk penyaring | `NosocomialInfection : Read` | — | `NosocomialInfectionFilterMetadataResponse` |
| `GET` | `/` | Menampilkan daftar kejadian dengan penyaringan dan halaman | `NosocomialInfection : Read` | Query: `patientId`, `emergencyVisitId`, `encounterId`, `serviceUnitId`, `infectionType`, `status`, `startDate`, `endDate` | Daftar `NosocomialInfectionResponse` berhalaman |
| `GET` | `/{id}` | Menampilkan detail satu kejadian | `NosocomialInfection : Read` | Path `id` | `NosocomialInfectionResponse` |
| `POST` | `/` | Mencatat kejadian baru; selalu berstatus Dicurigai | `NosocomialInfection : Create` | `CreateNosocomialInfectionRequest` | Kejadian yang baru dibuat |
| `PUT` | `/{id}` | Mengubah isi kejadian yang belum ditutup | `NosocomialInfection : Update` | Path `id` + `UpdateNosocomialInfectionRequest` | Kejadian yang sudah diperbarui |
| `PATCH` | `/{id}/status` | Menetapkan status kejadian | `NosocomialInfection : Update` | Path `id` + `UpdateNosocomialInfectionStatusRequest` | Kejadian yang sudah diperbarui |
| `DELETE` | `/{id}` | Menandai kejadian sebagai terhapus | `NosocomialInfection : Delete` | Path `id` | Pesan berhasil |

#### Kode status dan artinya

| Kode | Arti teknis | Arti bagi pengguna |
| --- | --- | --- |
| `200` | Berhasil | Permintaan diproses dan datanya tersedia |
| `201` | Data dibuat | Kejadian berhasil dicatat |
| `400` | Permintaan tidak valid | Isian tidak lengkap atau melanggar aturan bisnis |
| `401` | Belum masuk | Sesi habis; pengguna perlu masuk ulang |
| `403` | Tidak berwenang | Sudah masuk tetapi tidak punya hak untuk tindakan ini |
| `404` | Tidak ditemukan | Catatan sudah ditandai terhapus atau tidak pernah ada |
| `409` | Bentrok | Transisi status tidak sah, atau catatan sudah ditutup |

### 3.4 Berkas backend

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/NosocomialInfectionEnums.cs` | **Baru** — tiga enum |
| `Areas/HealthServices/ClinicalManagement/Models/TrxNosocomialInfection.cs` | **Baru** |
| `Areas/HealthServices/ClinicalManagement/DTOs/NosocomialInfectionDtos.cs` | **Baru** |
| `Areas/HealthServices/ClinicalManagement/Controllers/NosocomialInfectionController.cs` | **Baru** |
| `Repositories/Configurations/HealthServices/TrxNosocomialInfectionConfiguration.cs` | **Baru** |
| `Repositories/ApplicationDbContext.cs` | Menambah satu `DbSet` |
| `Migrations/20260821063311_AddNosocomialInfection.cs` | **Baru** |

Relasi disimpan sebagai kolom identifier tanpa navigation property. Menautkannya lewat
navigation akan menarik lima modul ke dalam setiap query surveilans tanpa ada yang
membutuhkannya; nama pasien dan unit diambil lewat satu join eksplisit pada endpoint yang
memang menampilkannya.

Dua check constraint dipasang karena keduanya menjadi penyebut indikator mutu, dan nilai
negatif akan menghasilkan angka insiden yang tidak punya arti:
`DeviceUsageDays >= 0` dan `HoursSinceAdmission >= 0`.

---

## 4. `FE-IGD-011` — Layar pengkajian pasien

### 4.1 Struktur layar

Mengikuti susunan pada gambar acuan, tetapi seluruhnya memakai komponen yang sudah ada di
project — `DataTable`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, dan React Bootstrap —
serta CSS Module terpusat di `src/style/**`.

```text
Daftar pasien  →  tombol Pemeriksaan  →  layar pemeriksaan
                                          ├── Tombol Kembali
                                          ├── Kartu Informasi Pasien
                                          ├── Panel kiri: 6 kelompok pekerjaan
                                          └── Isi: tab per kelompok
```

Panel kiri memuat Asuhan Keperawatan, Tindakan, Penunjang Medis, Pemakaian Alat, Transfer
Pasien, dan Tagihan Pasien. Kelompok Asuhan Keperawatan memuat tujuh tab: Assesmen Awal IGD,
SOAP, Nosokomial, Catatan Terintegrasi, Observasi, Tindak Lanjut, dan Resep.

### 4.2 Keadaan setiap bagian

| Bagian | Endpoint | Keadaan |
| --- | --- | --- |
| Daftar pasien | `GET /emergency-visits` | Tersambung |
| Assesmen Awal IGD | `GET /patient-assessments` | Tersambung, baca saja |
| Tanda vital | `GET /patient-vital-signs` | Tersambung, baca saja |
| SOAP | `GET /patient-integrated-progress-notes` | Tersambung, baca saja |
| Nosokomial | `GET`/`POST` `/nosocomial-infections` | Tersambung, **dapat mencatat** |
| Catatan Terintegrasi | `GET /patient-integrated-progress-notes` | Tersambung, baca saja |
| Observasi | `GET /emergency-observations` | Tersambung, baca saja |
| Tindak Lanjut | `GET /emergency-dispositions` | Tersambung, baca saja |
| Tindakan | `GET /emergency-procedure-details` | Tersambung, baca saja |
| Transfer Pasien | `GET /emergency-transfers` | Tersambung, baca saja |
| Resep | `GET /prescriptions?encounterId=` | Tersambung, baca saja |
| Penunjang Medis | — | **Belum tersambung** |
| Pemakaian Alat | — | **Belum tersambung** |
| Tagihan Pasien | — | **Belum tersambung** |

Empat bagian terakhir menyatakan dirinya **belum tersambung** apa adanya. Menampilkannya
sebagai daftar kosong akan membuat perawat mengira datanya memang belum diisi, lalu menunggu
sesuatu yang tidak akan pernah muncul.

### 4.3 Tiga keputusan yang perlu dijelaskan

**Pertama, data setiap tab diambil hanya ketika tab-nya dibuka.** Mengambil ketujuhnya
sekaligus saat halaman dibuka berarti tujuh permintaan berjalan untuk satu tab yang benar-benar
dilihat. Pada jam sibuk IGD itu memperlambat layar yang paling sering dipakai.

**Kedua, kartu identitas tidak memuat kolom sensitif.** Keluhan utama, lokasi ditemukan, lokasi
trauma, dan catatan tidak ditampilkan di sana. Kartu itu terlihat sepanjang perawat membuka tab
mana pun, termasuk ketika layar dilihat orang lain yang kebetulan lewat.

**Ketiga, identifier kunjungan tidak pernah tampil di URL.** Yang tampil adalah token acak yang
hanya berlaku pada sesi berjalan, mengikuti pola yang sudah dipakai layar triage.

### 4.4 Berkas frontend

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../emergency-assessment-constant.jsx` | **Baru** — route, tab, panel, dan peta enum nosokomial |
| `src/lib/state/slice/.../emergency-assessment-slice.jsx` | **Baru** — seluruh thunk; satu-satunya tempat Axios |
| `src/lib/hooks/.../emergency-assessment/use-emergency-assessment-list.jsx` | **Baru** |
| `src/lib/hooks/.../emergency-assessment/use-emergency-assessment-detail.jsx` | **Baru** |
| `src/components/view/.../emergency-assessment-view/emergency-assessment-list-view.jsx` | **Baru** |
| `src/components/view/.../emergency-assessment-view/emergency-assessment-detail-view.jsx` | **Baru** |
| `src/components/view/.../emergency-assessment-view/components/` (5 berkas) | **Baru** — kartu pasien, pembungkus bagian, tab nosokomial, tab catatan terintegrasi, tab daftar umum |
| `src/style/.../emergency-assessment/emergency-assessment.module.css` | **Baru** |
| `src/app/.../emergency-assessment/page.jsx` dan `[slug]/page.jsx` | **Baru** |
| `src/lib/state/store.jsx` | Mendaftarkan slice baru |
| `src/utils/menu-sidebar/menu-items.jsx` | Menambah menu Pengkajian Pasien |

### 4.5 Aturan repo yang diikuti

| Aturan | Cara dipenuhi |
| --- | --- |
| Axios hanya di Redux slice | Seluruh permintaan berada di `emergency-assessment-slice.jsx`; hook, komponen, dan constants tidak mengimpor Axios |
| Bahasa Indonesia | Seluruh label, pesan, dan komentar |
| CSS Modules terpusat, tanpa inline style | Satu berkas di `src/style/**`; tidak ada `style={{ ... }}` pada berkas baru |
| `DataTable` + `DataFilter` | Dipakai keduanya pada daftar pasien |
| Penamaan kebab-case | Seluruh berkas baru |
| Slice terdaftar di store | `emergencyAssessment` |

---

## 5. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` backend | **Lulus — 0 error** |
| Migration `AddNosocomialInfection` terbentuk | **Ya** — membuat tabel, dua check constraint, lima index |
| ESLint seluruh berkas baru | **Bersih — 0 error, 0 warning** |
| `npm run test:unit` | **38 lulus, 0 gagal** |
| `npm run build` | **Lulus** — kedua route pengkajian terdaftar |
| **Migrasi nosokomial diterapkan** | **Sudah** — lihat bagian 5.1 |
| **Alur simpan dijalankan sungguhan** | **Belum** |

### 5.1 Penerapan migrasi ke basis data

Migrasi dijalankan pada `QuilvianNewDevTim01 @ 160.22.250.77`, basis data pengembangan
bersama, atas persetujuan pemilik pekerjaan.

```
dotnet ef database update
```

Backend sedang berjalan dan mengunci `bin/Debug/net9.0/QuilvianSystemBackend.exe`, sehingga
build dijalankan ke direktori keluaran terpisah lewat `BaseOutputPath`. Proses yang sedang
berjalan tidak perlu dihentikan, dan direktori sementaranya sudah dihapus.

Hasil pemeriksaan langsung ke katalog PostgreSQL sesudahnya:

| Yang diperiksa | Hasil |
| --- | --- |
| Riwayat migrasi | `20260821063311_AddNosocomialInfection` tercatat |
| Tabel terbentuk | `public."TrxNosocomialInfection"` ada, 45 kolom |
| Check constraint | `CK_TrxNosocomialInfection_DeviceUsageDays` dan `CK_TrxNosocomialInfection_HoursSinceAdmission` terpasang, keduanya menolak nilai negatif |
| Index | Lima index ditambah primary key: nomor catatan (unik), pasien + waktu gejala, kunjungan IGD, encounter, dan gabungan unit + jenis + status + waktu |
| Isi tabel | 0 baris — tabel baru, belum ada kejadian dicatat |

Migrasi ini **menambah tabel baru** dan tidak menyentuh satu pun tabel yang sudah ada,
sehingga tidak ada data tim yang berubah. Cara memundurkannya adalah `dotnet ef database
update AddModulBackendBillingdanKasirPart1`, yang akan menghapus tabel beserta isinya —
periksa dulu apakah sudah ada kejadian yang tercatat sebelum melakukannya.

---

## 6. Yang belum dikerjakan

| No | Hal | Alasan |
| ---: | --- | --- |
| 1 | Migrasi `AddNosocomialInfection` belum diterapkan | Penulisan ke basis data bersama memerlukan izin; jalankan `dotnet ef database update` bila sudah disepakati tim |
| 2 | Daftar jenis infeksi belum disahkan tim PPI | Isinya memakai istilah surveilans yang lazim, tetapi pengesahan adalah wewenang clinical governance |
| 3 | Tiga bagian belum tersambung | Penunjang medis, pemakaian alat, dan tagihan pasien **belum punya entity maupun controller** di backend; lihat 6b.2. Resep sudah tersambung |
| 4 | Tab selain nosokomial masih baca saja | Pengisiannya membutuhkan kontrak formulir masing-masing modul pemilik data |
| 5 | Uji komponen | Belum ada; kriteria terbukti dari kode, bukan dari test |
| 6 | `/grill-me` untuk nosokomial | `BE-IGD-015` lahir dari kebutuhan layar, bukan dari decision log. Wawancara owner belum dilakukan |

---

## 6b. Tindak lanjut 21 Agustus 2026 — tab Resep dan penelusuran tiga bagian sisanya

### 6b.1 Tab Resep tersambung

Penelusuran membuktikan endpoint peresepan sudah mendukung penyaringan yang dibutuhkan:

| Bukti | Lokasi |
| --- | --- |
| `PrescriptionController` menerima `encounterId` dan `patientId` | `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` baris 111 dan 113 |
| `TrxPrescription` memang menaut ke encounter | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs` baris 25 |
| Balasan sudah memuat nama, bukan hanya identifier | `PrescriptionResponse` memuat `PatientName`, `DoctorName`, `PrescriptionNumber` |

Tab Resep karena itu diubah dari **belum tersambung** menjadi tersambung, menampilkan nomor
resep, waktu, dokter penulis, status resep, status pemenuhan, serta jumlah obat dan racikan.

Penyaringnya memakai `encounterId`, bukan `emergencyVisitId`. Resep dimiliki modul Farmasi dan
menaut ke encounter, karena satu encounter dapat melahirkan resep di unit mana pun — termasuk
resep lanjutan setelah pasien pindah dari IGD. Menyaring menurut kunjungan IGD akan
menyembunyikan resep yang sah.

Layar ini **menampilkan** resep, tidak membuatnya. Pembuatan resep tetap milik modul Farmasi.

### 6b.2 Tiga bagian sisanya memang belum ada backendnya

Ditelusuri satu per satu, bukan diasumsikan:

| Bagian | Yang dicari | Hasil |
| --- | --- | --- |
| Penunjang Medis | Controller laboratorium, radiologi, atau order pemeriksaan | **Nihil** — `find Areas -name "*Lab*Controller.cs" -o -name "*Radiolog*Controller.cs" -o -name "*Order*Controller.cs"` tidak menghasilkan satu berkas pun |
| Pemakaian Alat | Entity pemakaian alat kesehatan pada pasien | **Nihil** |
| Tagihan Pasien | Entity transaksi tagihan atau invoice | **Hanya master data** — `MstBillingItemCategory` dan `MstPaymentMethod`; tidak ada entity transaksi maupun controller-nya |

Ketiganya tetap dinyatakan **belum tersambung** di layar. Ini bukan pekerjaan frontend yang
tertunda, melainkan modul backend yang memang belum dirancang. Langkah yang tepat adalah
`/grill-me` lalu `/design-business-module` untuk masing-masing, bukan menambal di layar IGD.

### 6b.3 Bukti endpoint nosokomial benar-benar terdaftar

```
GET https://localhost:7184/api/v1/health-services/clinical-management/nosocomial-infections
  → 401
```

Balasan `401` — bukan `404` — membuktikan route terdaftar dan filter otorisasi bekerja.
Endpoint kontrol `emergency-triages` menjawab sama, sehingga hasilnya sebanding.

Yang **belum** dibuktikan tetap sama: bahwa satu kejadian benar-benar tersimpan ke basis data
lewat layar. Pembuktian itu memerlukan sesi login petugas, dan tidak dapat dilakukan tanpa
kredensial pengguna.

### 6b.4 Temuan: basis data lebih maju daripada cabang ini

`__EFMigrationsHistory` memuat `20260821034003_AddModulBackendBillingdanKasirPart1`, tetapi
berkas migrasinya **tidak ada** di cabang `rizkiG`. Migrasi itu diterapkan rekan tim dari
cabang lain.

Akibat yang perlu diantisipasi saat penggabungan cabang: urutan migrasi di basis data tidak
sama dengan urutan di cabang ini. Basis data sudah memuat tabel billing yang kodenya belum ada
di sini, sehingga `dotnet ef migrations add` berikutnya pada cabang ini akan dibentuk dari
snapshot yang belum mengenal tabel-tabel itu. Ini tidak memblokir pekerjaan sekarang, tetapi
sebaiknya diselesaikan sebelum cabang digabung.

---

## 6c. Tindak lanjut 21 Agustus 2026 — layar pengkajian menjadi formulir

Sebelumnya seluruh tab bersifat baca saja. Empat tab kini memiliki formulir beserta
riwayatnya, memakai primitif form yang sudah ada di project.

### 6c.1 Primitif form yang dipakai ulang

| Komponen | Asal |
| --- | --- |
| `BaseTextField`, `BaseSelectField`, `BaseTextareaField`, `BaseSimpleCheckbox` | `src/components/ui/form-pemeriksaan-ui` |

Keempatnya adalah komponen yang dipakai formulir skrining pada antrean dokter, sehingga
tampilan isian di layar IGD sama persis dengan layar yang sudah dikenal petugas. Tidak ada
komponen isian baru yang dibuat.

### 6c.2 Empat tab yang kini berupa formulir

| Tab | Endpoint simpan | Isi formulir |
| --- | --- | --- |
| Tanda Vital | `POST /patient-vital-signs` | Pengukuran, kesadaran dan GCS, pengkajian nyeri, catatan klinis |
| SOAP | `POST /patient-integrated-progress-notes` | Profesi pencatat, waktu, S/O/A/P, instruksi, evaluasi |
| Nosokomial | `POST /nosocomial-infections` | Data kejadian, detail infeksi, kaitan alat, kultur dan mikrobiologi, catatan |
| Observasi | `POST /emergency-observations` | Periode observasi, lokasi, indikasi, rencana pemantauan |

Tab Assesmen Awal IGD, Catatan Terintegrasi, Tindak Lanjut, Tindakan, Transfer, dan Resep
tetap baca saja. Isinya dimiliki modul lain, dan formulir pengisiannya berada di layar modul
tersebut.

### 6c.3 Formulir nosokomial dibanding aplikasi lama

Aplikasi lama memecah formulir menjadi panel per jenis infeksi: `InfeksiADP`, `InfeksiDetail`,
`InfeksiLO`, `InfeksiSK`, `InfeksiTD`, dan `KulturDarah`. Seluruh isian yang dibutuhkan
tersedia di sini, tetapi pengelompokannya diubah:

| Panel lama | Tempatnya sekarang |
| --- | --- |
| `InfeksiADP` | Pilihan jenis infeksi — "IADP — Infeksi Aliran Darah Primer" |
| `InfeksiSK` | Pilihan jenis infeksi — "ISK — Infeksi Saluran Kemih" |
| `InfeksiLO` | Pilihan jenis infeksi — "ILO — Infeksi Luka Operasi" |
| `InfeksiTD` | **Perlu penegasan** — lihat catatan di bawah |
| `InfeksiDetail` | Kelompok "Detail Infeksi" dan "Kaitan dengan Alat" |
| `KulturDarah` | Kelompok "Kultur dan Mikrobiologi", dengan jenis spesimen dapat dipilih |

Dua alasan pengelompokan diubah:

1. **Satu kejadian hanya pernah berjenis satu.** Panel terpisah per jenis mengharuskan perawat
   tahu lebih dulu panel mana yang harus dibuka. Menjadikannya satu pilihan menghapus langkah
   itu, dan singkatan aslinya tetap tampil di label supaya perawat mengenalinya.
2. **Panel yang tertutup menyembunyikan isian wajib.** Perawat IGD mengisi sambil berdiri di
   samping pasien; kelompok yang selalu terbuka membuat isian yang belum terisi terlihat tanpa
   perlu membuka apa pun.

> **`InfeksiTD` belum dipetakan.** Singkatan ADP, SK, dan LO jelas padanannya. `TD` tidak.
> Dugaan terkuat adalah dekubitus, dan pilihan "Dekubitus — Luka Tekan" memang tersedia,
> tetapi memetakannya tanpa konfirmasi berarti menebak kategori mutu. Mohon ditegaskan tim
> PPI sebelum dianggap setara.

### 6c.4 Tab Tanda Vital berdiri sendiri

Sebelumnya tanda vital menumpang di tab Assesmen Awal IGD. Kini menjadi tab tersendiri karena
sifatnya berbeda: assesmen awal dibuat sekali, sedangkan tanda vital dicatat berulang. Setiap
penyimpanan membuat catatan baru dan tidak menimpa catatan sebelumnya, sehingga perkembangan
pasien dapat ditelusuri.

Pengkajian nyeri ikut di sini, bukan menjadi tab terpisah, karena backend menyimpannya pada
entitas yang sama (`HasPain`, `PainScale`, `PainLocation`, `PainNote` pada
`TrxPatientVitalSign`). Memisahkannya di layar akan menghasilkan dua permintaan simpan untuk
satu baris data.

### 6c.5 Susunan layar

Mengikuti gambar acuan, **tanpa ikon**:

```text
Kembali
Kartu Informasi Pasien
┌───────────────┬─────────────────────────────┬──────────────────┐
│ Panel kiri    │ Tab + isi                   │ Ringkasan Cepat  │
│ 6 kelompok    │ formulir lalu riwayat       │ jumlah per bagian│
└───────────────┴─────────────────────────────┴──────────────────┘
```

Ringkasan kanan menghitung **hanya bagian yang sudah dibuka**; bagian yang belum dibuka
ditandai tanda hubung, bukan angka nol. Nol akan terbaca sebagai "belum ada data", padahal
yang benar adalah "belum diperiksa".

### 6c.6 Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint seluruh berkas pengkajian | **Bersih** |
| Kelas CSS dipakai tetapi tidak terdefinisi | **0** |
| `npm run build` | **Lulus** |
| `npm run test:unit` | **38 lulus, 0 gagal** |
| `dotnet build` | **0 error** |
| **Alur simpan dijalankan sungguhan** | **Belum** |

---

## 7. Roadmap yang diperbarui

| Dokumen | Perubahan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Bagian 6b baru: task `BE-IGD-015` beserta acceptance criteria dan blocker-nya |
| `roadmap/frontend-roadmap.md` | Bagian 5b baru: task `FE-IGD-011` beserta alasan pemisahan menu dari triage |

Keduanya ditandai sebagai tambahan **setelah** roadmap revisi 1, bukan disisipkan seolah-olah
sudah ada sejak awal. Roadmap revisi 1 tidak pernah memuat kebutuhan nosokomial maupun layar
pengkajian, dan menyamarkan hal itu akan membuat traceability berbohong.
