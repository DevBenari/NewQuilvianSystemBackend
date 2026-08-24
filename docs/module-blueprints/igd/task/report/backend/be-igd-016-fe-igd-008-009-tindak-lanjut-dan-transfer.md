# Laporan Perubahan — `BE-IGD-016`, `FE-IGD-008`, dan `FE-IGD-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-016` (nama pada balasan + tiga cacat perilaku), `FE-IGD-008` (tindak lanjut), `FE-IGD-009` (transfer pasien) |
| Slice | `FE-IGD-008` dan `FE-IGD-009` berasal dari slice F4 revisi 1; `BE-IGD-016` tambahan di luar revisi 1 |
| Repository | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Contract version | API `0.2.0` — kontrak tidak berubah; **balasan bertambah field**, tidak ada field yang dihapus atau berubah arti |
| Tanggal | 21 Agustus 2026 |
| **Status** | **Kode selesai, lint bersih, build backend dan frontend lulus, 38 unit test lulus. Tidak ada migration. Alur simpan lewat layar belum dijalankan sungguhan** |

---

## 1. Titik berangkat

Sebelum pekerjaan ini, roadmap menyisakan empat task frontend tanpa blocker: `FE-IGD-007`
(observasi, sudah tertutup oleh formulir observasi pada layar pengkajian), `FE-IGD-008`,
`FE-IGD-009`, dan `FE-IGD-010`. Tab Tindak Lanjut dan bagian Transfer Pasien pada layar
pengkajian masih **baca-saja**.

Penelusuran sebelum menulis kode menemukan bahwa keduanya bukan sekadar belum bisa diisi —
yang ditampilkan pun tidak pernah terisi.

---

## 2. Temuan: lima kolom yang tidak pernah mungkin terisi

Layar pengkajian menampilkan kolom-kolom berikut:

| Tab | Field yang diminta layar | Ada di balasan backend? |
| --- | --- | --- |
| Tindak Lanjut | `dispositionTypeName` | **Tidak** |
| Tindak Lanjut | `destinationServiceUnitName` | **Tidak** |
| Tindak Lanjut | `referralFacilityName` | **Tidak** — nama field sebenarnya `destinationFacilityName` |
| Transfer | `destinationServiceUnitName` | **Tidak** — nama field sebenarnya `toServiceUnitId` |
| Transfer | `transferNumber` | Ada |

`EmergencyDispositionResponse` dan `EmergencyTransferResponse` hanya memuat identifier. Empat
dari lima kolom itu karena itu **selalu** menampilkan tanda hubung, berapa pun data yang ada di
basis data.

Ini bukan kesalahan ketik yang berdiri sendiri. Ia menandai keputusan yang belum pernah
diambil: siapa yang bertugas mengubah identifier menjadi nama. Modul Farmasi sudah
menjawabnya — `PrescriptionResponse` memuat `PatientName` dan `DoctorName`. Modul IGD belum.
`BE-IGD-016` mengikuti jawaban yang sudah ada, bukan membuat jawaban kedua.

---

## 3. `BE-IGD-016` — Backend

### 3.1 Nama ikut dikirim pada balasan

| Balasan | Field baru |
| --- | --- |
| `EmergencyDispositionResponse` | `DispositionTypeCode`, `DispositionTypeName`, `RequiresDestinationServiceUnit`, `RequiresReferralFacility`, `ClosesEmergencyVisit`, `DecidedByDoctorName`, `DestinationServiceUnitName` |
| `EmergencyTransferResponse` | `FromServiceUnitName`, `ToServiceUnitName` |

Nama diambil lewat navigation property yang **sudah ada** pada kedua entity. Aksi baca memakai
`.Include()`; aksi tulis memuat relasinya sesudah menyimpan, lewat satu helper per controller.

> **Mengapa aksi tulis ikut memuat nama.** Tanpa itu, balasan `POST` memuat identifier
> sedangkan balasan `GET` memuat nama. Layar yang baru saja menyimpan harus memuat ulang
> seluruh daftar hanya untuk memperoleh nama dari data yang barusan dikirimnya sendiri.

Dua penanda kewajiban — `RequiresDestinationServiceUnit` dan `RequiresReferralFacility` —
sengaja ikut dikirim. Keduanya menentukan isian mana yang wajib muncul di formulir. Dikirim
dari master berarti rumah sakit dapat menambah jenis tindak lanjut baru tanpa frontend diubah;
disalin ke frontend berarti aturan yang sama hidup di dua tempat dan cepat atau lambat berbeda.

### 3.2 Cacat pertama — `BE-IGD-008` bocor lewat jalur kedua

`BE-IGD-008` mensyaratkan `VisitCompletedAt` **tidak lagi terisi** saat kunjungan menjadi
`Disposed`. Task itu memperbaiki `EmergencyVisitController`, dan laporannya mencatat kriteria
tersebut "ada di kode — belum terbukti".

Kriteria itu ternyata tidak terpenuhi. `VisitStatus` menjadi `Disposed` dari **dua** jalur:

| Jalur | Keadaan sebelum perbaikan |
| --- | --- |
| `PATCH /emergency-visits/{id}/status` | Sudah benar sejak `BE-IGD-008` |
| `PATCH /emergency-dispositions/{id}/disposition-status` → `Executed` | **Masih mengisi `VisitCompletedAt`** |

Jalur kedua justru jalur yang dipakai sehari-hari: dokter menjalankan tindak lanjut, dan
kunjungan menjadi `Disposed` sebagai akibatnya. Petugas hampir tidak pernah mengubah status
kunjungan secara langsung.

> **Akibat bila dibiarkan:** pukul 14.00 dokter menjalankan keputusan rawat inap. Kunjungan
> memperoleh `VisitCompletedAt` pukul 14.00, padahal pasien masih menunggu proses perpindahan
> ke bangsal dan secara fisik masih berada di IGD. Laporan lama tinggal pasien menghitungnya
> sebagai sudah pergi. Inilah persis keadaan yang `BE-IGD-008` dibuat untuk menghapusnya.

Baris pengisian dihapus dan diganti komentar yang menyebutkan alasannya, supaya tidak
dikembalikan orang berikutnya yang mengira itu kelalaian.

### 3.3 Cacat kedua — pembatalan dan penolakan tanpa alasan

| Aksi | Sebelum | Sesudah |
| --- | --- | --- |
| Membatalkan tindak lanjut | Alasan tidak diminta sama sekali | `400` bila `Notes` kosong |
| Menolak perpindahan | Alasan menumpang di `Notes`, tidak pernah diwajibkan | `400` bila `RejectionReason` dan `Notes` sama-sama kosong |

`UpdateEmergencyTransferTransferStatusRequest` memperoleh field `RejectionReason` tersendiri.
Kolomnya sudah ada di tabel sejak awal — yang tidak ada hanyalah jalan mengisinya. `Notes`
tetap diterima sebagai cadangan supaya pemanggil lama tidak mendadak kehilangan alasannya.

Tindak lanjut **tidak** memperoleh kolom alasan tersendiri, dan alasannya disimpan pada
`Notes`. Kolom baru berarti migration pada basis data yang dipakai bersama satu tim, sedangkan
`Cancelled` adalah status terminal — tidak ada transisi keluar darinya, sehingga catatan yang
ditulis saat pembatalan tidak akan tertimpa perubahan status berikutnya. Bila kelak tim
memutuskan kolom tersendiri lebih tepat, itu menjadi task dengan migration-nya sendiri.

### 3.4 Cacat ketiga — penerima terisi oleh pengaju

`Create` perpindahan mengisi `AcceptedByUserId = request.AcceptedByUserId ?? actorUserId`.
Perpindahan yang baru **diajukan** karena itu langsung tercatat "diterima oleh" pengajunya
sendiri, sementara `AcceptedAt` tetap kosong.

Dua hal yang salah sekaligus: kolom penerima berisi orang yang justru satu-satunya orang yang
**tidak boleh** menerimanya (`AT-IGD-041`), dan dua kolom yang menerangkan satu kejadian
menjadi tidak sinkron. Nilainya kini dibiarkan apa adanya dari permintaan, dan hanya diisi
ketika perpindahan benar-benar diterima.

### 3.5 Rapian kecil

Dua tempat memakai refleksi untuk menulis properti `Notes`:

```csharp
if (!string.IsNullOrWhiteSpace(request.Notes) && entity.GetType().GetProperty("Notes") != null)
{
    entity.GetType().GetProperty("Notes")?.SetValue(entity, NormalizeText(request.Notes));
}
```

Kedua entity memang punya properti `Notes` yang diketahui saat kompilasi. Refleksi di sini
tidak memberi keleluasaan apa pun, hanya memindahkan kesalahan ketik dari waktu build ke waktu
jalan. Diganti pengisian langsung.

### 3.6 Berkas backend

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/EmergencyDispositionDtos.cs` | Tujuh field nama dan penanda pada response; dokumentasi kewajiban alasan pembatalan |
| `.../DTOs/EmergencyTransferDtos.cs` | Dua field nama unit; field `RejectionReason` pada request status |
| `.../Controllers/EmergencyDispositionController.cs` | `.Include()` pada dua aksi baca; helper pemuat nama; pemetaan nama; kewajiban alasan pembatalan; **penghapusan pengisian `VisitCompletedAt`**; refleksi diganti |
| `.../Controllers/EmergencyTransferController.cs` | `.Include()` pada dua aksi baca; helper pemuat nama; pemetaan nama; kewajiban alasan penolakan; perbaikan `AcceptedByUserId`; refleksi diganti |

**Tidak ada migration.** Tidak ada kolom, tabel, maupun index yang berubah — seluruh nama
diambil dari relasi yang sudah ada. Basis data bersama tidak disentuh sama sekali.

---

## 4. `FE-IGD-008` dan `FE-IGD-009` — Frontend

### 4.1 Tempatnya berubah dari rencana revisi 1

Roadmap revisi 1 merencanakan keduanya sebagai route tersendiri. Keduanya dibangun sebagai
bagian **layar pengkajian pasien**, yang sudah memiliki tab Tindak Lanjut dan bagian Transfer
Pasien dalam keadaan baca-saja.

Alasannya satu: perawat sudah membuka layar pengkajian untuk pasien yang sama. Route tersendiri
berarti ia menutup layar, mencari pasien yang sama di daftar lain, lalu membukanya kembali —
untuk melanjutkan pekerjaan pada pasien yang sedang ada di hadapannya. Tab baca-saja yang sudah
ada juga akan menjadi tandingan layar baru, dan dua tempat yang menampilkan fakta sama adalah
cara tercepat membuat keduanya berbeda isi.

Penyimpangan ini dicatat pada `frontend-roadmap.md` bagian 5c, bukan disamarkan seolah-olah
rencananya memang begitu sejak awal.

### 4.2 Tab Tindak Lanjut

Formulir menetapkan keputusan akhir pasien, tersimpan sebagai **draf** lebih dulu.

| Kelompok isian | Isi |
| --- | --- |
| Keputusan | Jenis tindak lanjut, waktu keputusan, unit tujuan, fasilitas rujukan, nomor rujukan |
| Alasan dan Instruksi | Alasan, kondisi pasien saat keputusan, instruksi tindak lanjut, alasan penolakan pasien |
| Bila Pasien Meninggal | Waktu, lokasi, dugaan penyebab, permintaan visum |
| Catatan | Catatan tambahan |

**Isian unit tujuan dan fasilitas rujukan hanya muncul ketika jenis yang dipilih memang
mensyaratkannya**, mengikuti penanda dari master. Perawat yang memilih "Pulang" tidak melihat
isian unit tujuan sama sekali; yang memilih "Rujuk" melihatnya sebagai isian wajib.

Riwayat di bawahnya menampilkan setiap keputusan beserta status dan tombol aksinya:

| Status sekarang | Aksi yang muncul |
| --- | --- |
| Draf | Konfirmasi, Batalkan |
| Dikonfirmasi | Jalankan, Batalkan |
| Dijalankan | — |
| Dibatalkan | — |

Membatalkan membuka dialog yang **mewajibkan alasan**, memakai `ConfirmModal` yang sudah ada
beserta kemampuan `requireReason`-nya.

**Butir 5 acceptance criteria** — layar tidak boleh menyiratkan tindak lanjut sama dengan
kunjungan selesai — dipenuhi di dua tempat: satu blok penegas yang selalu tampil di formulir,
dan kalimat pada dialog Jalankan yang menyebutkan kunjungan akan berstatus `Disposed` dan itu
belum berarti selesai.

### 4.3 Bagian Transfer Pasien

Formulir mengajukan perpindahan; riwayatnya menampilkan rangkaian tahapnya.

Rangkaian **Diajukan → Diterima → Berangkat → Tiba** selalu tampil utuh, termasuk tahap yang
belum tercapai — ditandai "Belum", bukan disembunyikan. Perawat pengaju perlu melihat pasiennya
sedang menunggu tahap yang mana; menampilkan hanya tahap yang sudah lewat justru menyembunyikan
tahap yang sedang ditunggu.

| Status sekarang | Aksi yang muncul |
| --- | --- |
| Diajukan | Terima, Tolak, Batalkan |
| Diterima | Berangkatkan, Tolak, Batalkan |
| Dalam perjalanan | Tandai Tiba, Batalkan |
| Tiba / Ditolak / Dibatalkan | — |

Tombol **Terima tidak ditampilkan kepada pengaju perpindahan itu sendiri**, dan barisnya
menyatakan alasannya. Penyembunyian ini hanya kenyamanan layar — pemisahan tugas ditegakkan
backend, yang menolak permintaan semacam itu dengan `403` walaupun dikirim di luar layar ini.
Unit asal pasien juga dikeluarkan dari pilihan tujuan, karena backend menolak perpindahan ke
unit yang sama dengan unit asal.

### 4.4 Komponen yang dipakai ulang

Tidak ada komponen isian, tabel, maupun dialog baru yang dibuat.

| Kebutuhan | Yang dipakai |
| --- | --- |
| Isian formulir | `BaseSelectField`, `BaseTextField`, `BaseTextareaField`, `BaseSimpleCheckbox` dari `components/ui/form-pemeriksaan-ui` |
| Pembungkus formulir | `EmergencyAssessmentFormCard` + `EmergencyAssessmentFormSection` |
| Ketujuh keadaan layar | `EmergencyAssessmentSection` |
| Dialog konfirmasi dan alasan | `ConfirmModal` dari `base-features`, termasuk `requireReason` |
| Identitas pengguna | `selectUserInfo` dari `login-slice`, sesuai aturan repo nomor 5 |

### 4.5 Berkas frontend

| Berkas | Perubahan |
| --- | --- |
| `.../emergency-assessment-constant.jsx` | Peta status, varian warna, dan daftar aksi untuk tindak lanjut dan transfer; tahapan rangkaian perpindahan |
| `.../emergency-assessment-slice.jsx` | Thunk `createDisposition`, `createTransfer`, `updateDispositionStatus`, `updateTransferStatus`, `fetchDispositionTypeOptions`, `fetchServiceUnitOptions`; state `masterOptions`; `saving`/`saveError` pada dua bagian |
| `.../use-emergency-assessment-detail.jsx` | Memuat master hanya pada tab yang memakainya, dan hanya sekali |
| `.../components/emergency-assessment-disposition-tab.jsx` | **Baru** |
| `.../components/emergency-assessment-transfer-tab.jsx` | **Baru** |
| `.../emergency-assessment-detail-view.jsx` | Dua blok baca-saja diganti kedua tab; meneruskan `masterOptions` |
| `.../emergency-assessment.module.css` | Kelas aksi per baris, catatan penegas formulir, dan rangkaian tahap perpindahan |

### 4.6 Aturan repo yang diikuti

| Aturan | Cara dipenuhi |
| --- | --- |
| Axios hanya di Redux slice | Seluruh permintaan di `emergency-assessment-slice.jsx`; kedua komponen baru tidak mengimpor Axios |
| Bahasa Indonesia | Seluruh label, pesan, dan komentar |
| CSS Modules terpusat, tanpa inline style | Kelas baru ditambahkan pada modul yang sudah ada; nol `style={{ ... }}` |
| `createBy` dari `state.auth.userInfo` | `selectUserInfo` dipakai membandingkan pengaju perpindahan; `RequestedByUserId` diisi backend dari token, tidak dikirim layar |
| Penamaan kebab-case | Kedua berkas baru |
| Slice terdaftar di store | `emergencyAssessment`, sudah terdaftar sejak `FE-IGD-011` |

---

## 5. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` backend | **Lulus — 0 error**, 127 warning (sama persis dengan sebelum perubahan; tidak ada warning baru) |
| ESLint seluruh berkas yang disentuh | **Bersih — 0 error, 0 warning** |
| Kelas CSS dipakai tetapi tidak terdefinisi | **0** dari 70 kelas pada 13 berkas |
| `npm run test:unit` | **38 lulus, 0 gagal** |
| `npm run build` | **Lulus** — kedua route pengkajian tetap terdaftar |
| Migration baru | **Tidak ada** — basis data bersama tidak disentuh |
| **Alur simpan dijalankan sungguhan** | **Belum** |

### 5.1 Yang belum dibuktikan, dan mengapa

Sama seperti pada `BE-IGD-015` dan `FE-IGD-011`: **belum ada satu pun bukti bahwa tindak lanjut
atau perpindahan benar-benar tersimpan lewat layar.** Pembuktiannya memerlukan sesi login
petugas, dan tidak dapat dilakukan tanpa kredensial pengguna.

Yang dapat dinyatakan sekarang hanyalah bahwa kode dibangun sesuai kontrak, lulus build, dan
lulus lint. Itu bukan hal yang sama dengan berjalan.

Solution backend masih **tanpa proyek test**, sehingga tidak satu pun `AT-IGD-*` dapat
dijalankan — termasuk `AT-IGD-041` yang justru menjadi dasar butir 3 `FE-IGD-009`.

---

## 6. Yang belum dikerjakan

| No | Hal | Alasan |
| ---: | --- | --- |
| 1 | Nama pengaju dan penerima perpindahan | `EmergencyTransferResponse` masih memuat keduanya sebagai identifier. `BE-IGD-016` menambah nama unit, bukan nama pengguna. Kolom pelaku sengaja belum ditampilkan daripada menampilkan identifier sebagai pengganti nama |
| 2 | `FE-IGD-010` halaman detail kunjungan | Dependency-nya kini terpenuhi seluruhnya. Menjadi task frontend berikutnya |
| 3 | `BE-IGD-011`, `BE-IGD-012` | Menunggu penunjukan security/privacy owner |
| 4 | `BE-IGD-014` bukti penerimaan | Menunggu proyek test ada |
| 5 | Uji komponen kedua tab | Belum ada; kriteria terbukti dari kode, bukan dari test |
| 6 | Penunjang medis, pemakaian alat, tagihan pasien | Tetap **belum tersambung** — belum punya entity maupun controller di backend |

---

## 7. Yang perlu diputuskan pemilik

| No | Hal | Yang diminta |
| ---: | --- | --- |
| 1 | Arti `VisitCompletedAt` berubah untuk jalur tindak lanjut | Sejak perubahan ini, kunjungan yang tindak lanjutnya dijalankan **tidak lagi** memperoleh waktu selesai otomatis. Penyelesaiannya harus lewat `PATCH /emergency-visits/{id}/complete`. Itu memang kehendak `BE-IGD-008`, tetapi baris lama memakai arti yang berbeda — batas waktunya perlu dicatat, sama seperti yang sudah diminta pada laporan `BE-IGD-008` |
| 2 | Alasan pembatalan tindak lanjut disimpan pada `Notes` | Bila tim menghendaki kolom tersendiri, itu menjadi task dengan migration-nya sendiri pada basis data bersama |
| 3 | Nama pengaju dan penerima perpindahan | Perlu ditegaskan apakah `EmergencyTransferResponse` menyertakan nama pengguna, dan dari sumber mana namanya diambil |

Tiga keputusan yang sudah tercatat sebelumnya **tetap terbuka**: target waktu triage level 2–5,
pengesahan daftar jenis infeksi nosokomial oleh tim PPI, dan padanan singkatan `InfeksiTD`.

---

## 8. Roadmap yang diperbarui

| Dokumen | Perubahan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Bagian 6c baru: `BE-IGD-016` beserta acceptance criteria, risiko, dan penjelasan mengapa `BE-IGD-008` bocor |
| `roadmap/frontend-roadmap.md` | Bagian 5c baru: penyesuaian tempat `FE-IGD-008` dan `FE-IGD-009`, beserta keadaan setiap acceptance criteria apa adanya |

Keduanya ditandai sebagai perubahan **setelah** roadmap revisi 1. `FE-IGD-008` dan `FE-IGD-009`
isinya berasal dari revisi 1 dan tidak berubah; yang berubah hanya tempat layarnya, dan itu
dicatat sebagai penyimpangan, bukan disamarkan.
