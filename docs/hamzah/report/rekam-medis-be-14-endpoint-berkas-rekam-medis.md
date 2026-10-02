# Rekam Medis — `BE-14` Endpoint berkas rekam medis

| | |
|---|---|
| Tanggal | 2026-08-27 |
| Task ID | `BE-14` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `yoga` (repository backend, tidak ada operasi Git write) |
| Trace | `RM-DEC-002`; api-contract bagian 2 |
| Verifikasi | `AT-RM-09`, `AT-RM-32`, `AT-RM-37` |
| Migration | **Tidak ada** |
| Endpoint baru | **4** |
| Bukti | `dotnet test` → `Failed: 0, Passed: 107`. 11 uji baru, seluruhnya lulus |
| Breaking change | **Tidak** — seluruhnya endpoint baru; tidak ada pemanggil lama |

---

## 1. Masalah yang diselesaikan

`BE-13` sudah menyediakan lapisan penggabung tiga belas sumber dokumen klinis, tetapi belum ada
pintu masuknya. Frontend tidak dapat memanggil service secara langsung.

`BE-14` membuka pintu itu, dan sekaligus menegakkan aturan yang membuat berkas rekam medis boleh
dibuka sama sekali: **setiap pembukaan dinilai kewenangannya dan dicatat jejaknya lebih dulu**,
sebelum satu baris isi pun diambil.

## 2. Endpoint yang dibuat

### Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas: identitas, alergi aktif, diagnosis aktif, jumlah dokumen per jenis | `MedicalRecord : Read` |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan lintas kunjungan, urut waktu | `MedicalRecord : Read` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail satu dokumen beserta addendumnya | `MedicalRecord : Read` |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` |

Parameter permintaan:

| Endpoint | Query |
|---|---|
| `/summary` | `accessPurposeId`, `accessReason` |
| `/timeline` | `documentKinds`, `encounterId`, `startDate`, `endDate`, `includeCancelled`, `newestFirst`, `page`, `pageSize`, `accessPurposeId`, `accessReason` |
| `/documents/{documentKind}/{documentId}` | `accessPurposeId`, `accessReason` |
| `/filters/metadata` | — |

Kode status dan artinya bagi pengguna:

| Kode | Arti |
|---|---|
| `200` | Berkas terbuka. Satu baris jejak akses sudah tercatat |
| `400` | Permintaan tidak lengkap — misalnya alasan akses kosong padahal pasien tidak punya kunjungan berjalan |
| `401` | Belum masuk, atau sesi berakhir |
| `403` | Tidak punya hak akses ke menu rekam medis |
| `404` | Pasien tidak ditemukan, atau dokumen bukan milik pasien itu |
| `409` | Pasien hasil penggabungan nomor rekam medis; buka nomor penggantinya |
| `503` | Jejak akses gagal dicatat, sehingga isi tidak dikembalikan. Coba lagi |

**Endpoint `private-note` belum dibuat.** Itu scope `BE-15`, dengan izin terpisah
`MedicalRecord : ReadPrivateNote`.

## 3. Bisnis prosesnya, urut

Ini urutan yang berlaku pada ketiga endpoint yang membuka berkas pasien. Urutannya mengikat,
bukan anjuran.

1. **Pengguna meminta berkas seorang pasien.** Hak akses menu sudah diperiksa lapisan
   `[AccessPermission]` sebelum masuk ke controller.
2. **Kewenangan tingkat pasien dinilai.** Apakah pengguna ini boleh membuka rekam medis PASIEN
   INI — pertanyaan yang tidak dapat dijawab hak akses menu. Penilaiannya oleh
   `MedicalRecordAccessAuditService` (`BE-11`).
3. **Jejak akses ditulis dan disimpan.** Selesai **sebelum** isi diambil.
4. **Bila langkah 2 atau 3 gagal, permintaan berhenti di sini.** Isi rekam medis tidak
   disentuh sama sekali — bukan diambil lalu disembunyikan.
5. **Isi baru diambil** lewat `MedicalRecordTimelineService` (`BE-13`).
6. **Balasan dikembalikan** beserta keterangan pembukaannya.

### Contoh dua keadaan yang berbeda

**Dokter membuka pasien yang sedang dirawatnya.** Pasien punya kunjungan yang belum ditutup,
jadi tidak diminta alasan. Jejak tetap tercatat sebagai akses rawatan, dan tidak ditandai untuk
ditelaah. Balasannya memuat `access.accessTypeName = "Akses Rawatan"`.

**Petugas klaim membuka pasien yang kunjungannya sudah selesai.** Tidak ada kunjungan berjalan,
jadi keperluan akses wajib dipilih. Bila kosong → `400`, dan isi tidak dikembalikan. Bila diisi
keperluan yang sah → dilayani, jejak tercatat sebagai akses beralasan, dan **balasannya
menyatakan `access.isFlaggedForReview = true`** sehingga petugas tahu pembukaannya akan
ditelaah unit rekam medis.

## 4. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordController.cs` | Baru | 4 endpoint |
| `Areas/HealthServices/MedicalRecordManagement/DTOs/MedicalRecordDtos.cs` | Baru | Bentuk balasan seluruh endpoint |
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Diperbarui | Tambah `GetSummaryAsync`, `GetDocumentCountsAsync`, `GetDocumentDetailAsync` |
| `tests/QuilvianSystemBackend.Tests/MedicalRecordManagement/MedicalRecordFileEndpointTests.cs` | Baru | 10 uji |
| `docs/module-blueprints/rekam-medis/contracts/api-contract.md` | Diperbarui | Status endpoint dan catatan delta kontrak |

**Tidak ada tabel baru, tidak ada migration, tidak ada `AddScoped` baru.** Controller ASP.NET
tidak perlu didaftarkan, dan kedua service yang dipakainya sudah terdaftar sejak `BE-11` dan
`BE-13`.

### Kenapa tiga metode baca ditaruh di service, bukan di controller

Arsitektur backend bagian 5.9 menyatakannya tegas: controller rekam medis **wajib** memakai
service dan tidak boleh menyentuh `ApplicationDbContext` langsung, karena setiap pembacaan harus
melewati pencatatan jejak lebih dulu.

Karena itu ringkasan berkas dan detail dokumen ikut diletakkan di `MedicalRecordTimelineService`,
bukan ditulis di controller. Alasan kedua sama pentingnya: pengetahuan tentang tiga belas tabel
klinis hanya boleh tinggal di satu tempat. Menyebarnya ke controller adalah pengulangan yang
persis menjadi temuan `RM-CAP-010`.

Satu-satunya query yang dijalankan controller langsung adalah daftar keperluan akses pada
`/filters/metadata` — dan itu tidak memuat data pasien mana pun.

## 5. Empat acceptance criteria dan cara pemenuhannya

### 1) Setiap permintaan melewati pencatatan jejak lebih dulu

Ditegakkan satu tempat: metode `NilaiAksesAsync` pada controller. Ketiga endpoint yang membuka
berkas memanggilnya sebagai langkah pertama, dan bila ia mengembalikan balasan penolakan,
pemanggilnya wajib mengembalikan balasan itu apa adanya.

Cakupan jejak dibedakan per endpoint — `Summary`, `Timeline`, `DocumentDetail` — supaya
pembukaan catatan pribadi kelak dapat dihitung terpisah saat ditinjau (`RM-DEC-022`).

`/filters/metadata` **tidak** mencatat jejak, dan itu disengaja: ia tidak menyentuh data pasien
mana pun. Mencatatnya akan mengotori angka tinjauan dengan pembukaan yang tidak pernah terjadi.

### 2) Status keutuhan ikut dikembalikan untuk jenis yang sudah tunduk aturan

Baris riwayat dan detail dokumen membawa `integrityStatus` beserta namanya dalam Bahasa
Indonesia, diambil dari `TrxClinicalDocumentIntegrity`. Detail dokumen juga membawa penanda
tangan, penulis, dan daftar addendumnya.

### 3) Jenis yang belum tunduk ditandai jelas

Setiap baris riwayat, setiap detail dokumen, setiap angka pada ringkasan, dan setiap pilihan
pada daftar penyaring membawa `isIntegrityEnforced`. Rilis pertama hanya menegakkan CPPT
(`RM-DEC-019`), jadi dua belas dari tiga belas bernilai `false`.

Balasan detail dokumen untuk jenis yang belum tunduk juga menyatakannya pada pesannya:
*"Jenis dokumen ini belum tunduk aturan keutuhan rekam medis."*

Ini bukan hal kecil. Menampilkan alergi seolah-olah sudah terlindungi aturan keutuhan membuat
pembacanya mempercayai dokumen yang sebenarnya masih dapat diubah bebas. Layar wajib
menyatakannya sesuai `RM-FE-009`.

### 4) `PrivateNote` tidak ada pada respons mana pun

Tidak satu pun endpoint pada controller ini mengembalikan isi `PrivateNote`. Yang dikembalikan
hanya penanda `hasPrivateNote` pada detail dokumen.

Penanda itu perlu: tanpanya, tidak ada yang tahu bahwa ada sesuatu yang dapat dibuka lewat jalur
sah dengan izin terpisah (`BE-15`). Menyembunyikan keberadaannya sekaligus isinya justru membuat
jalur sah itu tidak pernah dipakai.

Daftar riwayat juga sengaja tidak membawa isi catatan klinis sama sekali — hanya nomor dokumen,
judul pendek, dan keterangan dari kolom yang panjangnya sudah terbatas.

## 6. Privasi dan batas kepemilikan

| Hal | Perlakuan |
|---|---|
| Dokumen milik pasien lain | Dijawab `404`. Diperiksa di service dengan mencocokkan `PatientId`, bukan hanya id dokumen |
| Pasien hasil penggabungan | Dijawab `409` disertai nomor rekam medis pengganti. Riwayat sebagian **tidak** ditampilkan |
| `accessReason` | **Tidak pernah** masuk ke logger — kolom itu dapat mengungkap keadaan pasien |
| Isi catatan klinis pada daftar riwayat | Tidak ikut. Hanya pada detail dokumen |
| Penulisan data | Tidak ada, kecuali baris jejak akses |

Pemeriksaan kepemilikan dokumen bukan kemewahan: tanpa itu, siapa pun yang berhak membuka rekam
medis satu pasien dapat membaca dokumen pasien lain hanya dengan menebak id-nya.

## 7. Perubahan kontrak

**Bentuk balasan `/timeline` berubah** dari `ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>`
menjadi `ApiResponse<MedicalRecordTimelineResponse>`.

Alasannya tidak dapat dihindari. Riwayat digabung dari tiga belas sumber, dan acceptance criteria
`BE-13` nomor 4 mewajibkan sumber yang gagal dibaca **ditandai**. Bentuk `PagedResult` tidak punya
tempat untuk menyatakan itu. Memaksakannya berarti daftar yang kurang satu jenis dokumen akan
terbaca sebagai daftar lengkap — kekeliruan yang paling berbahaya pada berkas rekam medis.

Selubung barunya memuat halaman yang sama pada field `page`, ditambah `access`, `requestedKinds`,
`failedSources`, `isTruncated`, dan `isComplete`.

**Dampak frontend:** pembacaan berubah dari `data.items` menjadi `data.page.items`. Belum ada
kode frontend yang memanggil endpoint ini, jadi tidak ada pemanggil lama yang rusak.

**Field `access` ditambahkan pada seluruh balasan** endpoint ini. Rinciannya beserta alasannya
tercatat pada `contracts/api-contract.md` bagian 2.

**Perlu pengesahan pemilik API.** Sampai `api_authority` ditunjuk, perubahan ini berstatus
diterapkan pada backend dan tercatat, belum disahkan.

## 8. Verifikasi

Perintah yang dijalankan:

```powershell
dotnet test tests\QuilvianSystemBackend.Tests\QuilvianSystemBackend.Tests.csproj
```

| Hasil | Angka |
|---|---|
| Kompilasi | **0 error**. Tidak ada satu pun warning yang berasal dari berkas `BE-14` |
| Uji seluruh suite | **Failed: 0, Passed: 107, Skipped: 0** — naik dari 96 sebelum task ini |
| Uji `BE-14` | 11 uji, seluruhnya lulus |
| Durasi | 1 menit 42 detik |

Uji berjalan di atas basis data SQLite dalam memori yang dibentuk dari konfigurasi EF Core yang
sama dengan aplikasi. **Tidak ada basis data bersama yang disentuh.**

| Acceptance criteria | Uji |
|---|---|
| 1) Setiap permintaan melewati pencatatan jejak lebih dulu | `SetiapPembukaan_MencatatJejakLebihDulu` |
| 1) Akses ditolak, isi tidak dikembalikan | `AksesDitolak_IsiRekamMedisTidakDikembalikan` |
| 1) Akses beralasan dilayani dan dinyatakan akan ditelaah | `AksesBeralasan_DilayaniDanPenggunaDiberiTahuAkanDitelaah` |
| 2) Status keutuhan ikut dikembalikan | `StatusKeutuhan_IkutDikembalikanUntukJenisYangSudahTunduk` |
| 3) Jenis yang belum tunduk ditandai jelas (`AT-RM-32`) | `JenisYangBelumTundukAturanKeutuhan_DitandaiJelas` |
| 4) `PrivateNote` tidak ada di respons mana pun (`AT-RM-37`) | `CatatanPribadi_TidakAdaPadaResponsManaPun` |
| Dokumen pasien lain ditolak | `DokumenMilikPasienLain_Dijawab404` |
| Pasien hasil penggabungan ditolak `409` | `PasienHasilPenggabungan_Dijawab409TanpaRiwayatSebagian` |
| Ringkasan berkas (`AT-RM-09`) | `RingkasanBerkas_MemuatIdentitasAlergiAktifDanJumlahDokumen` |
| Peringatan master keperluan kosong | `DaftarPilihan_MemperingatkanBilaMasterKeperluanKosong` |
| Daftar pilihan tidak menghasilkan jejak | `DaftarPilihan_TidakMenghasilkanJejakAkses` |

**Cara membuktikan `PrivateNote` tidak bocor.** Seluruh balasan diubah menjadi teks JSON, lalu
dicari isinya. Bila kolom itu bocor lewat jalur mana pun — bagian isi dokumen, judul, keterangan
pendek, atau kolom yang tidak sengaja ikut — uji gagal. Ini cara yang paling sulit dielakkan.

## 9. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Hak akses dan penyaringan permintaan HTTP | Uji memanggil controller langsung, bukan lewat HTTP. Lapisan `[AccessPermission]`, `[Authorize]`, dan model binding dilewati — sesuai keterangan pada `ControllerTestHarness` |
| Swagger terisi | Atribut `[ProducesResponseType]`, `[Tags]`, dan komentar XML sudah dipasang, tetapi tampilan Swagger-nya belum dibuka |
| Waktu tanggap pada data sungguhan | Warisan dari `BE-13`; masih tersisa |
| Pendaftaran hak akses `MedicalRecord : Read` | Dihasilkan atribut `[AccessAction]`/`[AccessPermission]` saat aplikasi berjalan, belum dipastikan muncul pada daftar hak akses |

## 10. Risiko yang tersisa

| Risiko | Penilaian |
|---|---|
| Master keperluan akses masih kosong (`BE-09`) | **Nyata sekarang.** Selama kosong, pembukaan rekam medis pasien di luar rawatan **selalu** ditolak. Ditutup sebagian: `/filters/metadata` mengembalikan peringatan tegas agar pengguna tidak mengira itu kesalahannya |
| Bentuk balasan `/timeline` belum disahkan pemilik API | `api_authority` masih `OPEN` |
| Permintaan riwayat tanpa penyaring jenis = 27 query | Diketahui dan dibatasi. Layar sebaiknya menyebut jenis dokumen |
| Pasien hasil penggabungan | Sudah dijawab `409` lewat `BE-11`. `BE-16` tinggal memastikan penelusuran lapangannya |

## 11. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 12. Task berikutnya

`BE-15` — endpoint `PrivateNote` dengan izin terpisah `MedicalRecord : ReadPrivateNote`.
Dependency-nya sudah terpenuhi: `BE-11` dan `BE-14` selesai.

Satu hal yang bukan pekerjaan kode dan **harus** berjalan lebih dulu menurut `RM-DEC-022`:
penulis CPPT perlu diberi tahu bahwa kolom catatan pribadi ternyata dapat dibuka lewat jalur
sah. Selama ini mereka menulisnya dengan anggapan kolom itu sepenuhnya pribadi.
