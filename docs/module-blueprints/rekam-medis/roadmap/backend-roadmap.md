# Rekam Medis — Backend Delivery Roadmap

```yaml
module_id: RM-BP-001
roadmap_revision: 1
status: DRAFT
owners:
  product_domain: Yoga Aji Pratama
  clinical_governance: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  api_authority: OPEN
approved_by: [Yoga Aji Pratama]
approved_at: 2026-08-26
input_revisions:
  interview_decisions: 4
  capability_map: 2
  backend_architecture: 1
artifact_hashes:
  interview_decisions: sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c
  capability_map: sha256:9cacecf803c0d552623a5f1ce5841af7bea7da5fc49aaf1b3142a076dd4416ae
  backend_architecture: sha256:32ab3711e9203bedf2838cdadbbeb1ab6400c20d49b1b1497eaed9efaa5243a1
contract_versions:
  api: 0.1.0 (draft)
  state_transition: 0.1.0 (draft)
  validation: 0.1.0 (draft)
  integration: 0.1.0 (draft)
  permission_audit: 0.1.0 (draft)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

> **GERBANG PERENCANAAN TERPENUHI.** Sejak 26 Agustus 2026 seluruh keputusan berstatus
> `approved` atas nama Yoga Aji Pratama (`RM-DEC-027`), sehingga roadmap ini **sudah menjadi
> izin mulai bekerja** untuk backend.
>
> Dua pengecualian yang tetap berlaku: `BE-08` menunggu bukti pada salinan data nyata, dan
> `BE-09` menunggu SOP rekam medis. Keduanya bukan soal pengesahan. Kontrak API juga masih
> menunggu pemilik API, tetapi itu hanya memblokir frontend.

---

## 1. Cara membaca roadmap ini

### Status keterlaksanaan

| Status | Arti |
|---|---|
| `SIAP` | Tidak bergantung pada keputusan atau kontrak berstatus draft. Dapat dimulai hari ini |
| `TERTAHAN APPROVAL` | Rancangannya lengkap, tetapi bergantung pada keputusan yang belum disahkan owner |
| `TERTAHAN BLOCKER` | Tertahan hal tertentu yang disebut namanya, di luar soal approval. **Tidak ada lagi task berstatus ini** |
| `SELESAI` | Sudah dikerjakan dan seluruh acceptance criteria terbukti |

Perbedaan dua status tertahan penting. `TERTAHAN APPROVAL` hilang begitu owner ditunjuk dan
menyetujui. `TERTAHAN BLOCKER` menuntut pekerjaan nyata lebih dulu, misalnya menetapkan angka
masa simpan atau menelusuri data. Per 24 Agustus 2026 kedua blocker itu sudah tertutup —
`RM-DEC-024` dijawab 25 tahun, dan penelusuran `RM-CAP-007` selesai.

### Aturan urutan yang mengikat

Tiga aturan berikut berasal dari keputusan, bukan dari selera penyusunan:

| Aturan | Sumber |
|---|---|
| Penutupan tiga celah CPPT mendahului layar penelusuran | `RM-DEC-019` |
| Pengisian data lama mendahului pemanggilan pendaftaran keutuhan | Integration contract bagian 3 |
| Angka masa simpan ditetapkan sebelum migration tabel jejak | `RM-DEC-023`. **Terpenuhi** — 25 tahun, `RM-DEC-024` |

### Satu ketergantungan yang tidak boleh dilanggar

**Penguncian tanpa addendum akan melumpuhkan pekerjaan klinis.** Bila `BE-04` (menandatangani
dan mengunci) dirilis tanpa `BE-06` (addendum), tenaga klinis tidak punya cara apa pun
membetulkan catatan yang keliru. Keduanya **wajib** dirilis bersamaan sebagai satu potongan
kerja, bukan berurutan lintas rilis.

Ini bukan penyempurnaan melainkan syarat keselamatan: catatan klinis yang salah dan tidak dapat
dikoreksi lebih berbahaya daripada catatan yang dapat diubah bebas.

---

## 2. Ringkasan status seluruh task

Status per 26 Agustus 2026, setelah pengesahan `RM-DEC-027`.

| Milestone | Task | Status | Keterangan |
|---|---|---|---|
| B0 | `BE-00` | **`SELESAI`** 24 Agustus 2026 | Project uji backend, 4 uji lulus |
| B0 | `BE-01` | **`SELESAI`** 26 Agustus 2026 | 3 tabel keutuhan, 6 enum, migration, 7 uji lulus |
| B0 | `BE-02` | **`SELESAI`** 26 Agustus 2026 | Service keutuhan, 4 metode, 17 uji lulus |
| B1 | `BE-03` | **`SELESAI`** 26 Agustus 2026 | Tiga celah CPPT ditutup, 8 uji lulus |
| B1 | `BE-04`, `BE-05`, `BE-06` | **`SELESAI`** 26 Agustus 2026 | Tanda tangan, penetapan berhalangan, addendum — 3 controller, 17 uji lulus. Dirilis bersamaan sesuai aturan urutan |
| B1 | `BE-07` | **`SELESAI`** 26 Agustus 2026 | Penguncian saat kunjungan selesai, dipasang di **tiga** jalur, 4 uji lulus |
| B1 | `BE-08` | **`SIAP DIJALANKAN`** | Alat dan panduannya selesai 26 Agustus 2026, 11 uji lulus. Yang tersisa: menjalankan penelaahan pada data nyata, lalu memberi tahu unit rekam medis. Lihat `BE-08-panduan-pengisian-data-lama.md` |
| B2 | `BE-09` | **`SELESAI` sebagian** 26 Agustus 2026 | Struktur tabel master selesai. **Isi awalnya masih menunggu SOP rekam medis** |
| B2 | `BE-10`, `BE-11` | **`SELESAI`** 26 Agustus 2026 | Tabel jejak akses, service kewenangan pasien, 12 uji lulus |
| B2 | `BE-12` | **`SELESAI`** 26 Agustus 2026 | Tinjauan akses, 4 endpoint, 6 uji lulus |
| B3 | `BE-13` | **`SELESAI`** 26 Agustus 2026 | Service penggabungan riwayat 13 sumber, 10 uji lulus |
| B3 | `BE-14`, `BE-15` | **`SIAP`** | Endpoint berkas rekam medis dan catatan pribadi |
| B3 | `BE-16` | **`SIAP`** — prioritas rendah | Pengaman pasien bernomor ganda (`RM-DEC-026`) |
| B4 | `BE-17`, `BE-18` | **`SIAP`** | Dikerjakan setelah task pendahulunya |

**Denominator: 19 task. Dua belas `SELESAI`, satu `SELESAI` sebagian, satu `SIAP DIJALANKAN`,
lima `SIAP`, nol `TERTAHAN APPROVAL`.**

Milestone B0, B1, dan B2 tuntas — kecuali dua hal yang bukan pekerjaan kode: isi awal master
keperluan akses (`BE-09`, menunggu SOP) dan penjalanan pengisian data lama (`BE-08`).

Sisa yang belum dikerjakan: `BE-14` sampai `BE-16` (endpoint berkas rekam medis, catatan
pribadi, dan pengaman pasien bernomor ganda), lalu `BE-17` dan `BE-18` (uji jalur gagal dan
catatan rilis).

**Bukti kumulatif:** `dotnet test` → `Failed: 0, Passed: 96`. `dotnet build` → 0 error.

### Hasil `BE-13`

| Berkas | Status |
|---|---|
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Baru — penggabungan 13 sumber |
| `Areas/.../MedicalRecordManagement/DTOs/MedicalRecordTimelineDtos.cs` | Baru — permintaan, baris riwayat, sumber gagal, hasil |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordTimelineTests.cs` | Baru — 10 uji |

**Tidak ada tabel baru, tidak ada migration, tidak ada endpoint.** Task ini murni lapisan
pembaca di atas tiga belas tabel klinis yang sudah ada, persis seperti status audit
`Reuse with adapter` pada `RM-CAP-004`. Endpoint-nya menyusul pada `BE-14`.

**Bisnis prosesnya, urut.** Petugas membuka berkas rekam medis seorang pasien. Sebelum service
ini ada, layar harus memanggil sampai tiga belas endpoint terpisah — CPPT, konsultasi, asesmen,
diagnosis, tindakan, tanda vital, alergi, riwayat penyakit, riwayat keluarga, dokumen klinis,
lampiran, surat keterangan, dan persetujuan — masing-masing dengan penomoran halaman sendiri,
lalu mengurutkan sendiri hasilnya. Sekarang alurnya menjadi:

1. Pemanggil menyebut pasien, jenis dokumen yang ingin dilihat, rentang tanggal, dan halaman.
2. Service menanyakan **hanya** jenis yang diminta ke tabel masing-masing, dengan batas baris.
3. Seluruh hasil digabung, diurutkan menurut waktu kejadian, lalu dipotong sesuai halaman.
4. Baris yang benar-benar ditampilkan dilengkapi status keutuhan dokumennya.
5. Bila ada sumber yang gagal dibaca, namanya ikut dikembalikan bersama hasilnya.

**Contoh.** Pasien dengan tiga kunjungan: CPPT 90 hari lalu, alergi 30 hari lalu, dan
persetujuan tindakan 2 hari lalu. Satu permintaan mengembalikan ketiganya sebagai satu daftar
berurut waktu, lengkap dengan nomor kunjungan masing-masing. Petugas tidak perlu membuka
kunjungan satu per satu.

Yang terbukti lewat uji:

| Acceptance criteria | Uji |
|---|---|
| 1) Dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu (`AT-RM-09`) | `RiwayatTigaKunjungan_TampilSebagaiSatuDaftarBerurutWaktu` |
| 2) Jumlah baris dibatasi dan penyaringan tanggal berfungsi (`AT-RM-31`) | `PasienDenganBanyakDokumen_JumlahBarisDibatasiDanTanggalTersaring` |
| 3) Hanya jenis dokumen yang diminta yang diambil | `HanyaJenisYangDiminta_YangDiambil` |
| 4) Satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai | `SatuSumberGagal_SumberLainTetapTampilDanYangGagalDitandai` |
| 5) Memakai `AsNoTracking` | `SeluruhPembacaan_TidakMeninggalkanEntityTerlacak` |
| Penyaring kunjungan mempersempit ke satu kunjungan | `PenyaringKunjungan_HanyaMengambilDokumenKunjunganItu` |
| Status keutuhan menempel; jenis yang belum tunduk ditandai terbuka | `StatusKeutuhan_DitempelkanUntukJenisYangSudahDitegakkan` |
| Dokumen dibatalkan tidak tampil kecuali diminta | `DokumenDibatalkan_TidakTampilKecualiDiminta` |
| Riwayat pasien lain tidak ikut terbawa | `RiwayatPasienLain_TidakIkutTerbawa` |
| Permintaan tanpa id pasien ditolak, bukan dijawab daftar kosong | `TanpaIdPasien_PermintaanDitolak` |

**Cara membuktikan sumber gagal, bukan sekadar disimulasikan.** Uji nomor 4 menghapus satu
tabel dari basis data uji, sehingga pembacaannya benar-benar gagal. Hasilnya: dua sumber lain
tetap terbaca, dan jenis `Persetujuan Tindakan` muncul pada daftar sumber gagal. Ini penting
karena penandanya bukan hiasan — layar wajib menyatakan bahwa daftar yang tampil belum lengkap.

**Dua query per sumber, bukan satu.** Satu untuk menghitung jumlah dokumen, satu untuk mengambil
barisnya. Penghitungan dipilih dengan sadar: tanpa itu, jumlah total pada layar hanya sebesar
isi halaman dan tombol "halaman berikutnya" kehilangan artinya. Query penghitungan tidak
mengambil baris apa pun dan dilayani index `(PatientId, tanggal, IsDelete)` yang sudah ada di
tiga belas tabel klinis. Konsekuensinya harus dipahami pemakai kontrak: meminta seluruh jenis
berarti dua puluh enam query, sedangkan meminta satu jenis berarti dua. **Menyebut jenis
dokumen adalah cara termurah menekan biaya permintaan.**

**Batas yang berlaku.** Ukuran halaman bawaan 25 dan paling besar 100; permintaan yang lebih
besar dipotong ke batas itu, bukan ditolak. Setiap sumber paling banyak menyerahkan 500 baris
dalam satu permintaan. Bila batas itu tersentuh, hasilnya ditandai terpotong — jumlah totalnya
tetap benar, tetapi isi halaman yang jauh belum tentu lengkap.

**Daftar riwayat sengaja tidak membawa isi catatan klinis.** Setiap baris hanya memuat nomor
dokumen, judul pendek, dan keterangan singkat dari kolom yang panjangnya sudah terbatas.
Isi lengkap dokumen — termasuk `PrivateNote` — tidak pernah ikut, dan itu memang harus dibuka
lewat endpoint detail yang jalur aksesnya tercatat tersendiri (`BE-14`, `BE-15`).

**Delta kontrak yang perlu diputuskan pada `BE-14`.** Api-contract bagian 2 menyebut balasan
`/timeline` berbentuk `ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>`. Bentuk itu
tidak punya tempat untuk daftar sumber gagal, padahal acceptance criteria nomor 4 mewajibkan
kekurangan dinyatakan. Karena `BE-13` hanya lapisan service, keputusannya belum diambil di
sini: service mengembalikan `MedicalRecordTimelineResult` yang memuat halaman **beserta**
daftar sumber gagal dan penanda terpotong. `BE-14` yang menentukan bagaimana keterangan itu
tampil pada balasan endpoint.

### Hasil `BE-09`, `BE-10`, dan `BE-11`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs` | Baru |
| `Areas/.../MedicalRecordManagement/Models/TrxMedicalRecordAccessLog.cs` | Baru |
| `Repositories/Configurations/HealthService/MedicalRecordManagement/` | 2 configuration baru |
| `Services/MedicalRecordAccessAuditService.cs` | Baru — penilaian kewenangan dan pencatatan jejak |
| `Migrations/20260826081755_AddMedicalRecordAccessAuditTables.cs` | Baru |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordAccessAuditTests.cs` | Baru — 12 uji |

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Pasien berkunjungan aktif dibuka tanpa alasan, tetap tercatat | `PasienDenganKunjunganAktif_DibukaTanpaAlasanDanTetapTercatat` |
| Tanpa kunjungan aktif dan tanpa keperluan ditolak, **isi tidak dikembalikan** | `PasienTanpaKunjunganAktif_TanpaKeperluan_DitolakDanIsinyaTidakDikembalikan` |
| Dengan keperluan sah, diizinkan dan ditandai untuk ditinjau | `PasienTanpaKunjunganAktif_DenganKeperluan_DiizinkanDanDitandaiUntukDitinjau` |
| Catatan pribadi **selalu** menuntut keperluan | `CatatanPribadi_SelaluMenuntutKeperluanWalauPasienSedangDirawat` |
| Sepuluh pembukaan menghasilkan tepat sepuluh jejak | `SepuluhPembukaan_MenghasilkanTepatSepuluhBarisJejak` |
| Pasien hasil penggabungan ditolak `409` disertai nomor pengganti | `PasienHasilPenggabungan_DitolakDisertaiNomorPengganti` |
| Kegagalan penilaian **tidak** melonggarkan kewenangan | `PenilaianKunjunganGagal_DiperlakukanSebagaiAksesBeralasan` |

**Kunci utama gabungan.** `TrxMedicalRecordAccessLog` memakai kunci utama `(Id, AccessedAt)`,
bukan `Id` saja. Ini disiapkan untuk pembagian tabel per tahun: PostgreSQL mensyaratkan kolom
pembagi ikut menjadi bagian kunci utama, dan **mengubah kunci utama pada tabel berisi jutaan
baris adalah bagian yang menuntut waktu henti layanan**. Menyiapkannya sejak awal menghilangkan
biaya itu.

**Yang belum dikerjakan pada pembagian tabel.** Perintah `PARTITION BY RANGE` belum diterapkan;
tabelnya masih tabel biasa. Alasannya: pada volume sekarang, pembagian adalah pekerjaan
sia-sia. Yang mahal untuk ditunda — bentuk kunci utama — sudah dikerjakan, sehingga penerapan
pembagian kelak hanya memerlukan pemindahan data, bukan perubahan skema kunci.

### Yang tersisa pada `BE-09`

Struktur tabelnya selesai dan siap dipakai. **Isi awalnya belum**, karena membutuhkan SOP rekam
medis rumah sakit yang belum tersedia. Isi minimum yang direncanakan ada pada arsitektur
bagian 9: lima keperluan akses beserta penanda mana yang menuntut penjelasan tambahan.

Sampai master ini terisi, seluruh pembukaan rekam medis pasien di luar rawatan akan ditolak,
karena tidak ada keperluan yang dapat dipilih. Itu perilaku yang menutup rapat, tetapi berarti
**master ini wajib terisi sebelum modul dipakai**.

### Hasil gabungan `BE-04`, `BE-05`, dan `BE-06`

Ketiganya dirilis bersamaan karena mengunci tanpa menyediakan addendum akan melumpuhkan
koreksi klinis.

| Berkas | Status |
|---|---|
| `DTOs/ClinicalDocumentIntegrityDtos.cs` | Baru — permintaan tanda tangan, balasan keutuhan, addendum |
| `DTOs/ClinicalNoteAuthorDelegationDtos.cs` | Baru — penetapan dan jawaban kewenangan |
| `Services/ClinicalNoteAddendumService.cs` | Baru — kewenangan bertingkat dan pembuatan addendum |
| `Services/ClinicalNoteAuthorDelegationService.cs` | Baru — penetapan beserta 6 aturan validasi |
| `Controllers/ClinicalDocumentIntegrityController.cs` | Baru — 4 endpoint |
| `Controllers/ClinicalNoteAddendumController.cs` | Baru — 4 endpoint |
| `Controllers/ClinicalNoteAuthorDelegationController.cs` | Baru — 3 endpoint |
| `Program.cs` | Diperbarui — 2 `AddScoped` tambahan |
| `tests/.../AuthorDelegationAndAddendumTests.cs` | Baru — 17 uji |

**Perubahan kontrak.** Grup addendum bertambah satu endpoint, dari 3 menjadi 4. Rancangan
semula menyatukan addendum biasa dan addendum pengganti dalam satu endpoint, dengan kewenangan
diperiksa di dalamnya. Itu tidak dapat diterapkan: atribut `[AccessAction]` hanya boleh satu
per endpoint, sehingga hak akses `CreateAsSubstitute` tidak akan pernah terdaftar dan tidak
dapat diberikan kepada siapa pun. Rinciannya pada `contracts/api-contract.md` bagian 4.

**Keputusan desain.** Kewenangan pengganti diterima service sebagai masukan, bukan dibaca
sendiri. `AccessPermissionService` memerlukan `UserManager`, yang tidak dapat dihidupkan pada
uji tanpa menyalakan seluruh Identity. Dengan memisahkannya, aturan bisnisnya tetap dapat
diuji dan sumber kewenangannya tetap satu tempat yang jelas di controller.

---

## 3. Milestone B0 — Fondasi

### `BE-00` — Membuat project test backend

| Field | Isi |
|---|---|
| **Task ID** | `BE-00` |
| **Status** | **`SELESAI`** — dikerjakan 24 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Tim punya cara membuktikan bahwa perubahan pada catatan klinis tidak merusak alur IGD dan antrean dokter. Sebelum ini, satu-satunya cara adalah mencoba manual dan berharap tidak ada yang terlewat |
| **Trace** | `RM-CAP-032`; open question nomor 11 pada decision log |
| **Reuse** | Tidak ada. Backend belum memiliki project test apa pun |
| **Scope** | Project test baru pada solution; penyiapan basis data uji; contoh uji yang menyentuh basis data |
| **Dependency** | **Tidak ada.** Tidak bergantung pada satu pun keputusan modul rekam medis |
| **Acceptance criteria** | 1) Perintah uji dapat dijalankan dan melaporkan hasil. 2) Sekurang-kurangnya satu uji integrasi menyentuh basis data dan lulus. 3) Uji dapat dijalankan berulang tanpa saling mengganggu |
| **Verification** | `dotnet test` dijalankan tiga kali berturut-turut: `Failed: 0, Passed: 4` pada ketiganya. `dotnet build QuilvianSystemBackend.sln` lolos dengan 0 error |
| **Risk/blocker** | **Tertutup.** Kerangka uji ditetapkan xUnit; basis data uji memakai SQLite di dalam memori. Batasan yang diterima tercatat pada `tests/README.md` bagian "Batasan yang diketahui" |
| **DoD** | **Terpenuhi.** Project test ada di solution, cara menjalankannya terdokumentasi di `tests/README.md`, dan empat uji lulus |

### Hasil `BE-00`

| Berkas | Status | Isi |
|---|---|---|
| `tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | Baru | Project test xUnit, menunjuk project utama |
| `tests/QuilvianSystemBackend.Tests/Infrastructure/TestDatabase.cs` | Baru | Penyedia basis data uji SQLite di dalam memori |
| `tests/QuilvianSystemBackend.Tests/Infrastructure/TestDatabaseTests.cs` | Baru | Empat uji yang membuktikan fondasi bekerja |
| `tests/README.md` | Baru | Cara menjalankan, cara menulis uji baru, dan batasan yang diketahui |
| `QuilvianSystemBackend.csproj` | Diperbarui | Pengecualian `tests\**` agar project test tidak ikut terkompilasi ke aplikasi |
| `QuilvianSystemBackend.sln` | Diperbarui | Project test ditambahkan ke solution |

Empat uji yang dihasilkan:

| Uji | Yang dibuktikan |
|---|---|
| `BasisDataUji_DapatDibentukLengkapDenganTabelnya` | Seluruh pemetaan EF Core aplikasi dapat dibentuk menjadi tabel sungguhan |
| `DataYangDisimpan_DapatDibacaKembaliLewatKonteksBaru` | Uji integrasi benar-benar menyentuh basis data, bukan hanya memori konteks |
| `IndexUnik_MenolakKodeYangKembar` | Aturan keunikan ditegakkan basis data — **prasyarat agar `BE-01` dapat dibuktikan** |
| `SetiapBasisDataUji_BerdiriSendiriDanTidakSalingMelihat` | Uji tidak saling mengganggu, sehingga hasilnya tidak bergantung urutan jalannya |

Uji ketiga yang paling menentukan. `BE-01` menuntut pembuktian bahwa index unik
`(DocumentKind, DocumentId)` menolak baris kembar. Bila basis data uji tidak menegakkan
keunikan, uji itu akan lulus padahal seharusnya gagal — dan itu lebih berbahaya daripada tidak
punya uji sama sekali.

### Yang sengaja tidak dikerjakan pada `BE-00`

| Yang tidak dikerjakan | Alasan |
|---|---|
| Menambahkan langkah `dotnet test` ke CI | Mengubah perilaku CI, perlu persetujuan pemilik CI. Cara menambahkannya sudah dituliskan di `tests/README.md` |
| Uji lewat HTTP | Belum ada endpoint modul ini. Ditambahkan saat endpoint pertama dibuat |
| Uji migration | `EnsureCreated` membentuk tabel langsung dari model, tidak menjalankan migration. Diperlukan saat `BE-08` yang memindahkan data |
| Uji untuk controller yang sudah ada | Di luar acceptance criteria `BE-00`. Uji regresi alur antrean dokter menjadi bagian `BE-03` |

Catatan mengapa task ini didahulukan meski bukan bagian modul rekam medis. Tiga perbaikan pada
`BE-03` menyentuh `PatientIntegratedProgressNoteController`, berkas sepanjang 1.407 baris yang
dipakai alur antrean dokter dan IGD. Mengubahnya tanpa jaring pengaman otomatis adalah risiko
yang dapat dihindari dengan pekerjaan yang tidak menunggu approval siapa pun.

### `BE-01` — Model, enum, dan configuration keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-01` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Sistem punya tempat menyimpan keterangan keutuhan dokumen, terpisah dari isi klinisnya |
| **Trace** | `RM-DEC-013`; ERD `keutuhan-dokumen.md`; kamus data bagian 1 sampai 3 dan 6 |
| **Reuse** | `IdentityModel`; pola configuration `Repositories/Configurations/HealthService/`; `ApplyConfigurationsFromAssembly` pada `ApplicationDbContext.cs:612` |
| **Scope** | `Areas/HealthServices/MedicalRecordManagement/Models/` tiga model; `Enums/` enam enum; `Repositories/Configurations/HealthService/MedicalRecordManagement/` tiga configuration; migration `AddMedicalRecordIntegrityTables` |
| **Dependency** | `BE-00` disarankan lebih dulu |
| **Acceptance criteria** | 1) Migration berjalan dan mundur tanpa galat. 2) Index unik `(DocumentKind, DocumentId)` menolak baris kembar. 3) Index unik `(IntegrityId, Sequence)` menolak addendum berurutan kembar. 4) Seluruh relasi memakai `DeleteBehavior.Restrict` |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 11`. Tujuh uji baru pada `ClinicalDocumentIntegritySchemaTests`. `dotnet build` lolos 0 error |
| **Risk/blocker** | **Tertutup untuk lingkup task ini.** Risiko rujukan polimorfik diterima sadar dan ditutup di service pada `BE-02` |
| **DoD** | **Terpenuhi.** Migration `20260826034557_AddMedicalRecordIntegrityTables` terbentuk dengan 3 tabel, 2 index unik, dan `Down()` yang menghapus ketiganya |

### Hasil `BE-01`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Enums/` — 6 enum | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalDocumentIntegrity.cs` | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAddendum.cs` | Baru |
| `Areas/HealthServices/MedicalRecordManagement/Models/TrxClinicalNoteAuthorDelegation.cs` | Baru |
| `Repositories/Configurations/HealthService/MedicalRecordManagement/` — 3 configuration | Baru |
| `Repositories/ApplicationDbContext.cs` | Diperbarui — 3 `DbSet` dan satu `using` |
| `Migrations/20260826034557_AddMedicalRecordIntegrityTables.cs` | Baru |
| `tests/.../Infrastructure/RekamMedisTestData.cs` | Baru — penyiapan pengguna, unit, pasien, kunjungan |
| `tests/.../MedicalRecordManagement/ClinicalDocumentIntegritySchemaTests.cs` | Baru — 7 uji |

Aturan yang terbukti ditegakkan basis data:

| Aturan | Uji |
|---|---|
| Satu dokumen tepat satu baris keutuhan | `DokumenYangSama_TidakDapatPunyaDuaBarisKeutuhan` |
| Keunikan berlaku pada pasangan jenis + Id, bukan Id saja | `JenisDokumenBerbeda_BolehMemakaiIdYangSama` |
| Urutan addendum tidak dapat kembar pada satu dokumen | `Addendum_TidakDapatPunyaUrutanKembarPadaDokumenYangSama` |
| Urutan boleh sama antar dokumen berbeda | `Addendum_BerurutanDiterimaDanUrutanBolehSamaAntarDokumen` |
| Dokumen yang punya addendum tidak dapat dihapus | `KeutuhanYangMasihPunyaAddendum_TidakDapatDihapus` |
| Enum tersimpan utuh sebagai angka | `StatusKeutuhanDanJenisDokumen_TersimpanUtuh` |

**Catatan temuan.** Penolakan penghapusan terjadi di lapisan EF Core saat baris ditandai hapus,
bukan di basis data saat disimpan. Hasilnya sama — addendum tidak pernah ikut terhapus diam-diam
— tetapi jenis galatnya `InvalidOperationException`, bukan `DbUpdateException`. Ini dicatat pada
komentar uji agar implementer berikutnya tidak salah menduga.

**Tidak dijalankan.** Migration **belum diterapkan** ke basis data mana pun. Berkasnya sudah
terbentuk dan terbukti maju-mundur secara bentuk, tetapi penerapannya ke basis data berjalan
memerlukan otorisasi terpisah.

### `BE-02` — Service keutuhan

| Field | Isi |
|---|---|
| **Task ID** | `BE-02` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Ada satu tempat yang memutuskan apakah sebuah dokumen masih boleh diubah. Tidak tersebar di banyak controller |
| **Trace** | `RM-DEC-003`; arsitektur bagian 5.6; state transition matrix bagian 2 dan 3 |
| **Reuse** | Pola service tanpa interface, didaftarkan `AddScoped`, mencontoh `DoctorConsultationLifecycleService` |
| **Scope** | `Services/ClinicalDocumentIntegrityService.cs`; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-01` |
| **Acceptance criteria** | 1) `RegisterAsync` menolak pendaftaran kedua untuk dokumen yang sama. 2) `SignAsync` menolak bila pemanggil bukan `AuthorUserId`. 3) `SignAsync` menolak bila status bukan `Draft`. 4) `EnsureMutableAsync` menolak dokumen `Signed`, `LockedUnsigned`, dan `Cancelled`. 5) `AuthorUserId` tidak dapat diubah lewat jalur mana pun |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 28`. Tujuh belas uji baru pada `ClinicalDocumentIntegrityServiceTests`, mencakup `AT-RM-02`, `AT-RM-10`, `AT-RM-11` |
| **Risk/blocker** | **Ditutup dengan cakupan sempit.** Aturan hanya ditegakkan untuk `ProgressNote`, dinyatakan lewat `ClinicalDocumentIntegrityService.DitegakkanUntuk` yang dapat dibaca dan diuji |
| **DoD** | **Terpenuhi.** Service terdaftar `AddScoped` pada `Program.cs`; seluruh acceptance criteria terbukti uji |

### Hasil `BE-02`

| Berkas | Status |
|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Baru |
| `Program.cs` | Diperbarui — satu `AddScoped` dan satu `using` |
| `tests/.../ClinicalDocumentIntegrityServiceTests.cs` | Baru — 17 uji |

Empat metode yang tersedia:

| Metode | Kegunaan | Menyimpan sendiri? |
|---|---|:---:|
| `RegisterAsync` | Mendaftarkan dokumen baru berstatus draf. Aman dipanggil berulang | **Tidak** — pemanggil wajib satu transaksi dengan pembuatan dokumen |
| `EnsureMutableAsync` | Memeriksa apakah dokumen masih boleh diubah | Tidak menulis |
| `SignAsync` | Menandatangani sekaligus mengunci | **Ya** — hanya menyentuh satu baris keutuhan |
| `LockOpenDocumentsForEncounterAsync` | Mengunci seluruh dokumen draf saat kunjungan ditutup, bertahap per potongan | **Tidak** — pemanggil wajib satu transaksi dengan penutupan kunjungan |

Perbedaan "menyimpan sendiri" itu mengikat. Keliru di sini berakibat data setengah tersimpan —
misalnya dokumen terbuat tetapi baris keutuhannya tidak, sehingga dokumen itu luput dari
seluruh aturan penguncian.

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Penulis dapat menandatangani, tanpa kata sandi maupun sidik jari | `Penulis_DapatMenandatanganiCatatannyaSendiri` |
| Bukan penulis ditolak `403`, bukan `400` | `BukanPenulis_TidakDapatMenandatangani` |
| Dokumen terkunci tidak dapat ditandatangani ulang | `DokumenYangSudahTerkunci_TidakDapatDitandatanganiUlang` |
| Dokumen terkunci menolak perubahan dan mengarahkan ke addendum | `DokumenYangDitandatangani_MenolakPerubahanDanMengarahkanKeAddendum` |
| Jenis dokumen yang belum ditegakkan dibiarkan lewat | `JenisDokumenYangBelumDitegakkan_DibiarkanLewat` |
| Penutupan kunjungan mengunci seluruh dokumen draf | `PenutupanKunjungan_MenguncSeluruhDokumenDrafPadaKunjunganItu` |
| Dokumen yang sudah ditandatangani tidak ikut diubah | `PenutupanKunjungan_TidakMengubahDokumenYangSudahDitandatangani` |
| Aman dipanggil berulang | `PenguncianKunjungan_AmanDipanggilBerulang` |
| Kunjungan lain tidak ikut terkunci | `PenutupanKunjungan_TidakMenyentuhDokumenKunjunganLain` |
| Penguncian bertahap menyelesaikan seluruhnya | `PenguncianBertahap_MenguncSeluruhnyaWalauPotongannyaKecil` |
| Penulis tidak berubah setelah ditandatangani | `PenulisDokumen_TidakBerubahSetelahDitandatangani` |

**Catatan desain.** Penolakan dikembalikan sebagai `IntegrityGuardResult` berisi kode status
dan pesan, bukan dilempar sebagai exception. Alasannya: setiap penolakan sudah punya pesan dan
kode yang ditetapkan validation matrix, sehingga controller cukup meneruskannya tanpa
menerjemahkan ulang. Pola ini mengikuti `ValidateMergedPatientReferenceAsync` yang sudah ada
pada `PatientController`.

**Catatan temuan.** Kolom `UserCode` pada tabel pengguna memiliki keunikan yang ditegakkan
basis data. Penyiapan data uji wajib mengisinya dengan nilai berbeda tiap pengguna; bila
dilewatkan, uji kedua yang membuat pengguna akan gagal.

---

## 4. Milestone B1 — Slice minimum: CPPT terkunci dan dapat dikoreksi

Ini vertical slice pertama yang menghasilkan sesuatu yang dapat diverifikasi pemilik proses:
**catatan CPPT yang sudah ditandatangani tidak dapat diubah diam-diam, dan koreksinya
meninggalkan jejak.**

### `BE-03` — Menutup tiga celah pada CPPT

| Field | Isi |
|---|---|
| **Task ID** | `BE-03` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti |
| **Outcome** | Catatan CPPT tidak lagi dapat diubah setelah ditandatangani, penulisnya tidak dapat dipindahkan ke orang lain, dan penanda read-only tidak dapat dilepas dari luar |
| **Trace** | `RM-DEC-019`; `RM-CAP-011`, `RM-CAP-012`, `RM-CAP-013`; api-contract bagian 8 |
| **Reuse** | `PatientIntegratedProgressNoteController` yang sudah ada; hanya perilakunya berubah |
| **Scope** | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`. Tiga perubahan: memanggil `EnsureMutableAsync` sebelum mengubah; **menghapus** penetapan `entity.ProviderUserId` dari permintaan pada baris 533; **menghapus** penetapan `entity.IsReadOnlyGenerated` dari permintaan pada baris 550. Tambahan: memanggil `RegisterAsync` saat CPPT dibuat |
| **Dependency** | `BE-00`, `BE-02` |
| **Acceptance criteria** | 1) Mengubah CPPT terkunci ditolak `400` dengan pesan mengarahkan ke addendum. 2) Mengirim `ProviderUserId` orang lain tidak mengubah apa pun. 3) Mengirim `IsReadOnlyGenerated` tidak mengubah apa pun. 4) Membuat CPPT menghasilkan baris keutuhan `Draft`. 5) Bila pendaftaran keutuhan gagal, pembuatan CPPT ikut dibatalkan |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 36`. Delapan uji baru pada `ProgressNoteIntegrityRepairTests`, mencakup `AT-RM-01`, `AT-RM-19`, `AT-RM-20`, `AT-RM-24`. `dotnet build` lolos 0 error |
| **Risk/blocker** | **Risiko tertinggi di seluruh roadmap, dan terbukti tertutup.** `BE-00` menjadi jaring pengamannya: 36 uji lulus setelah perubahan, termasuk uji yang membuktikan CPPT draf tetap dapat diubah seperti biasa |
| **DoD** | **Terpenuhi.** Ketiga celah tertutup dan terbukti uji. Pencatatan perubahan perilaku pada Swagger dan catatan rilis menjadi bagian `BE-18` |

### Hasil `BE-03`

Seluruh perubahan berada pada **perilaku**, bukan skema. Tidak ada satu kolom pun berubah.

| Berkas | Perubahan |
|---|---|
| `PatientIntegratedProgressNoteController.cs` | Diperbarui — 4 perubahan, lihat di bawah |
| `tests/.../ProgressNoteIntegrityRepairTests.cs` | Baru — 8 uji |
| `tests/.../Infrastructure/ControllerTestHarness.cs` | Baru — alat bantu memanggil controller dari uji |

Empat perubahan pada controller:

| No | Perubahan | Menutup |
|---:|---|---|
| 1 | `UpdateProgressNote` memanggil `EnsureMutableAsync` sebelum mengubah | `RM-CAP-011` |
| 2 | Penetapan `entity.ProviderUserId` dari isi permintaan **dihapus** | `RM-CAP-012` |
| 3 | Penetapan `entity.IsReadOnlyGenerated` dari isi permintaan **dihapus** | `RM-CAP-013` |
| 4 | `CreateProgressNote` dan `CreateFromConsultation` memanggil `RegisterIntegrityAsync` | `RM-DEC-013` |

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| CPPT yang ditandatangani menolak perubahan, isinya tidak tersentuh | `MengubahCpptYangSudahDitandatangani_DitolakDanIsinyaTidakBerubah` |
| CPPT yang terkunci karena kunjungan ditutup juga menolak | `MengubahCpptYangTerkunciKarenaKunjunganDitutup_Ditolak` |
| **CPPT draf tetap dapat diubah seperti biasa** | `MengubahCpptYangMasihDraf_TetapBerhasil` |
| Mengirim penulis orang lain tidak memindahkan penulis | `MengirimPenulisOrangLain_TidakMengubahPenulisCatatan` |
| Mengirim penanda hanya-baca tidak mengubah penanda | `MengirimPenandaHanyaBaca_TidakMengubahPenandaPadaCatatan` |
| Membuat CPPT menghasilkan baris keutuhan draf | `MembuatCppt_MenghasilkanBarisKeutuhanBerstatusDraf` |
| CPPT baru langsung tunduk aturan setelah ditandatangani | `CpptBaru_LangsungTundukAturanKeutuhanSetelahDitandatangani` |
| CPPT tanpa kunjungan tidak membuat baris keutuhan menggantung | `CpptTanpaKunjungan_TetapDibuatTetapiTidakTerdaftarKeutuhan` |

Uji ketiga yang paling menenangkan: **CPPT draf tetap dapat diubah seperti biasa.** Perbaikan
ini menutup celah tanpa memblokir alur yang wajar.

### Keterbatasan yang dinyatakan terbuka

| Keterbatasan | Penjelasan |
|---|---|
| CPPT tanpa kunjungan tidak terdaftar keutuhan | Baris keutuhan mensyaratkan kunjungan sebagai pengelompokannya. CPPT semacam itu tetap dapat dibuat, tetapi tidak tunduk aturan penguncian. Dilewati tanpa menggagalkan pembuatan, karena menolak akan memblokir alur yang berjalan |
| CPPT lama belum terdaftar | Baru terdaftar setelah `BE-08` dijalankan. Sampai saat itu, `EnsureMutableAsync` memperlakukannya sebagai masih boleh diubah |
| Hak akses dan bentuk balasan HTTP belum diuji | Uji memanggil controller langsung, melewati lapisan pemeriksaan hak akses. Perlu uji lewat HTTP bila kelak diperlukan |

### Perubahan perilaku yang wajib diberitahukan

Dicatat di sini dan akan dituangkan ke Swagger serta catatan rilis pada `BE-18`.

| Endpoint | Perubahan | Dampak bagi klien |
|---|---|---|
| `PUT .../patient-integrated-progress-notes/{id}` | Menolak bila dokumen terkunci | Menerima `400`. Sebelumnya berhasil |
| Endpoint yang sama | `ProviderUserId` diabaikan | **Tidak** menerima galat, tetapi nilainya tidak berpengaruh |
| Endpoint yang sama | `IsReadOnlyGenerated` diabaikan | Sama seperti di atas |

### `BE-04` — Menandatangani dan mengunci dokumen

| Field | Isi |
|---|---|
| **Task ID** | `BE-04` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Dokter dan perawat dapat menyatakan catatannya final, dan setelah itu isinya terjamin tidak berubah |
| **Trace** | `RM-DEC-003`, `RM-DEC-021`; api-contract bagian 3 |
| **Reuse** | `BE-02` |
| **Scope** | `Controllers/ClinicalDocumentIntegrityController.cs`; `DTOs/ClinicalDocumentIntegrityDtos.cs`. Empat endpoint: status per dokumen, menandatangani, catatan saya yang belum ditandatangani, keutuhan per kunjungan |
| **Dependency** | `BE-02` |
| **Acceptance criteria** | 1) Menandatangani mengisi `SignedAt`, `SignedByUserId`, `SignatureDeviceInfo`, dan `SignatureIpAddress`. 2) Perangkat dan IP diambil server dari permintaan, **tidak** dari kiriman klien. 3) Tidak ada permintaan kata sandi maupun sidik jari. 4) `/my-unsigned` hanya memuat dokumen milik pengguna |
| **Verification** | `AT-RM-02`, `AT-RM-18` |
| **Risk/blocker** | Risiko: bila layar `/my-unsigned` tidak ada, catatan yang lupa ditandatangani tidak dapat ditemukan. Karena itu endpoint ini bagian dari task yang sama, bukan tambahan |
| **DoD** | Empat endpoint berjalan; acceptance criteria terbukti uji; terdaftar di Swagger |

### `BE-05` — Penetapan penulis berhalangan

| Field | Isi |
|---|---|
| **Task ID** | `BE-05` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Kepala unit dapat membuka jalur koreksi ketika penulis catatan berhalangan, dan penetapan itu tercatat beserta alasan dan batas waktunya |
| **Trace** | `RM-DEC-020`; api-contract bagian 5; validation matrix bagian 3 |
| **Reuse** | `BE-01` |
| **Scope** | `Services/ClinicalNoteAddendumService.cs` bagian penentu kewenangan; `Controllers/ClinicalNoteAuthorDelegationController.cs`; `DTOs/` |
| **Dependency** | `BE-01` |
| **Acceptance criteria** | 1) Penetapan tanpa `ValidUntil` ditolak. 2) Penetapan dengan batas waktu yang sudah lewat ditolak. 3) Menetapkan diri sendiri ditolak. 4) Penetapan ganda untuk penulis yang sama ditolak. 5) Penetapan untuk akun yang sudah nonaktif ditolak disertai penjelasan bahwa jalurnya sudah terbuka otomatis |
| **Verification** | `AT-RM-26`, `AT-RM-27` |
| **Risk/blocker** | Risiko: penetapan manual dapat disalahgunakan. Ditutup dengan kewajiban batas waktu dan pencatatan alasan |
| **DoD** | Seluruh aturan validasi terbukti uji; penetapan tanpa batas waktu tidak dapat tersimpan lewat jalur mana pun |

### `BE-06` — Addendum

| Field | Isi |
|---|---|
| **Task ID** | `BE-06` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Kesalahan pada catatan yang sudah terkunci dapat dibetulkan tanpa menghapus isi aslinya. Pembaca melihat keduanya dan tahu urutan kejadiannya |
| **Trace** | `RM-DEC-004`, `RM-DEC-020`; api-contract bagian 4; state transition matrix bagian 4 |
| **Reuse** | `BE-02`, `BE-05` |
| **Scope** | `Services/ClinicalNoteAddendumService.cs`; `Controllers/ClinicalNoteAddendumController.cs`; `DTOs/` |
| **Dependency** | `BE-02`, `BE-05`. **Wajib dirilis bersamaan `BE-04`** — lihat bagian 1 |
| **Acceptance criteria** | 1) Addendum pada dokumen `Draft` ditolak. 2) Addendum oleh bukan penulis tanpa penetapan ditolak `403`. 3) Kepala unit dapat menambah addendum bila akun penulis nonaktif, dan `AuthorUserId` berisi kepala unit. 4) Isi dokumen induk tidak berubah. 5) Status dokumen tetap sama setelah addendum. 6) `AuthorUserId`, `IsSubstituteAuthor`, dan `DelegationId` ditentukan server, **tidak** diterima dari klien. 7) Tidak ada endpoint mengubah atau menghapus addendum |
| **Verification** | `AT-RM-04`, `AT-RM-05`, `AT-RM-14`, `AT-RM-17`, `AT-RM-28` |
| **Risk/blocker** | Risiko: addendum tidak dapat dihapus, sehingga kiriman ganda menempel selamanya. Pencegahannya di sisi frontend, `FE-04` |
| **DoD** | Seluruh acceptance criteria terbukti uji; endpoint pemeriksa kewenangan tersedia untuk dipakai frontend |

### `BE-07` — Penguncian saat kunjungan ditutup

| Field | Isi |
|---|---|
| **Task ID** | `BE-07` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026 |
| **Outcome** | Tidak ada catatan yang tertinggal terbuka setelah kunjungan pasien selesai |
| **Trace** | `RM-DEC-003`; integration contract bagian 2.2 |
| **Reuse** | `PatientEncounterController` yang sudah ada |
| **Scope** | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`, endpoint `PATCH /{id}/status`; penambahan `LockOpenDocumentsForEncounterAsync` pada `BE-02` |
| **Dependency** | `BE-02`, `BE-03` |
| **Acceptance criteria** | 1) Perpindahan menuju `Completed` mengunci seluruh dokumen `Draft` pada kunjungan itu. 2) Penguncian dan perubahan status berada dalam satu transaksi. 3) Bila penguncian gagal, penutupan kunjungan ikut dibatalkan. 4) Perpindahan menuju `Cancelled` **tidak** mengunci apa pun. 5) Aman dipanggil berulang |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 57`. Empat uji baru pada `EncounterClosureLockTests`, mencakup `AT-RM-03`. `dotnet build` lolos 0 error |
| **Risk/blocker** | Risiko transaksi panjang ditutup dengan penguncian per potongan. Catatan: endpoint status kunjungan tidak memvalidasi perpindahan (`RM-CAP-019`), dan itu **tidak** diperbaiki task ini |
| **DoD** | **Terpenuhi.** Pemicu terpasang pada seluruh jalur penyelesaian kunjungan; acceptance criteria terbukti uji |

### Hasil `BE-07` — dan satu temuan yang mengubah scope

**Desain menyebut satu jalur. Kenyataannya ada tiga.**

Penelusuran menemukan kunjungan dapat berpindah ke `Completed` lewat tiga tempat berbeda,
bukan hanya lewat `PatientEncounterController` seperti tertulis pada arsitektur bagian 5.11:

| Jalur | Berkas | Kapan dipakai |
|---|---|---|
| Perubahan status umum | `PatientEncounterController.UpdateEncounterStatus` | Penyesuaian manual oleh petugas |
| **Konsultasi dokter selesai** | `DoctorQueueController` baris ~461 | **Jalur paling sering dipakai** pada rawat jalan |
| Screening perawat selesai | `NurseStationQueueController` baris ~330 | Pasien yang tidak memerlukan dokter |

Bila pemicu hanya dipasang di jalur pertama seperti tertulis di desain, **kunjungan yang
diselesaikan lewat antrean dokter tidak akan pernah mengunci catatannya** — dan itu justru
jalur yang paling sering dipakai. Aturan `RM-DEC-003` lapis kedua akan tampak berjalan padahal
hampir tidak pernah aktif.

Pemicu karena itu dipasang pada **ketiganya**.

| Berkas | Perubahan |
|---|---|
| `PatientEncounterController.cs` | Diperbarui — pemicu pada perpindahan menuju `Completed` |
| `DoctorQueueController.cs` | Diperbarui — pemicu saat konsultasi dokter selesai |
| `NurseStationQueueController.cs` | Diperbarui — pemicu saat screening selesai tanpa dokter |
| `tests/.../EncounterClosureLockTests.cs` | Baru — 4 uji |

Pada ketiganya, penguncian **tidak menyimpan sendiri** — ia ikut `SaveChanges` yang sudah ada,
sehingga bila penguncian gagal, penyelesaian kunjungan ikut dibatalkan.

Yang terbukti lewat uji:

| Aturan | Uji |
|---|---|
| Kunjungan selesai mengunci seluruh catatan draf | `KunjunganSelesai_MenguncSeluruhCatatanDrafDanTidakMenyentuhYangDitandatangani` |
| Catatan yang sudah ditandatangani tidak tersentuh | uji yang sama |
| Setelah selesai, seluruh catatan menolak perubahan | `SetelahKunjunganSelesai_SeluruhCatatannyaMenolakPerubahan` |
| Pembatalan kunjungan **tidak** mengunci | `KunjunganDibatalkan_TidakMenguncCatatan` |
| Catatan yang dibuat setelah kunjungan selesai tetap draf | `CatatanYangDibuatSetelahKunjunganSelesai_TetapBerstatusDraf` |

### Keterbatasan yang dinyatakan terbuka

| Keterbatasan | Penjelasan |
|---|---|
| Catatan susulan tetap berstatus draf | Penguncian hanya berlaku pada saat kunjungan berpindah ke selesai. Catatan yang dibuat setelahnya perlu ditandatangani penulisnya sendiri, atau akan menggantung terbuka |
| Perpindahan status tidak divalidasi | `RM-CAP-019` tetap terbuka. Penguncian dipicu tujuan perpindahan, bukan urutannya, sehingga tetap bekerja walaupun status melompat |

### `BE-08` — Pengisian data lama

| Field | Isi |
|---|---|
| **Task ID** | `BE-08` |
| **Status** | **`SIAP DIJALANKAN`** — alat, uji, dan panduan selesai 26 Agustus 2026. Belum dijalankan pada data nyata |
| **Outcome** | Catatan CPPT yang sudah tersimpan sebelum modul ini ada ikut memiliki status keutuhan, sehingga tidak ada bagian rekam medis yang luput dari aturan |
| **Trace** | `RM-DEC-014`; arsitektur bagian 8.2 migration ketiga |
| **Reuse** | — |
| **Scope** | Migration `BackfillProgressNoteIntegrity` |
| **Dependency** | `BE-01`. **Wajib selesai sebelum `BE-03` diaktifkan di produksi** — lihat integration contract bagian 3 |
| **Acceptance criteria** | 1) CPPT pada kunjungan `Completed` atau `Cancelled` bernilai `LockedUnsigned` dengan `LockTrigger = BackfillEncounterClosed`. 2) CPPT pada kunjungan berjalan bernilai `Draft`. 3) CPPT yang sudah dibatalkan bernilai `Cancelled`. 4) CPPT tanpa `ProviderUserId` tetap dibuat barisnya dengan `IsAuthorKnown = false`, **tidak dilewati diam-diam**. 5) Dijalankan bertahap per potongan. 6) Dapat dimundurkan dengan menghapus baris yang dibuatnya |
| **Verification** | `dotnet test`: `Failed: 0, Passed: 68`. Sebelas uji baru pada `MedicalRecordBackfillTests`, mencakup `AT-RM-21` dan `AT-RM-33` |
| **Risk/blocker** | **Satu-satunya task yang menyentuh data klinis nyata.** Risikonya diturunkan lewat tiga hal: penelaahan yang hanya membaca, mode percobaan yang tidak menyimpan, dan penjalanan bertahap yang dapat dilanjutkan. Jumlah baris pada data nyata **masih belum diketahui** |
| **DoD** | Terpenuhi **sebagian**. Alat, uji, dan panduan selesai. Yang tersisa: penelaahan pada data nyata, pemberitahuan ke unit rekam medis, lalu penjalanan |

### Hasil `BE-08` — dan satu perubahan cara

**Rancangan semula memakai migration. Diganti menjadi service yang dijalankan terkendali.**

| Alasan | Penjelasan |
|---|---|
| Jumlah barisnya tidak diketahui | Migration langsung berjalan tanpa dapat ditelaah lebih dulu |
| Waktunya tidak dapat dipilih | Migration berjalan otomatis saat aplikasi naik. Pengisian ini sebaiknya dijalankan ketika unit rekam medis sudah siap |
| Sulit dilanjutkan bila terhenti | Service dapat dijalankan bertahap dan melanjutkan dari sisa |

Aturan penentuan statusnya **tidak berubah** — persis `RM-DEC-014`.

| Berkas | Status |
|---|---|
| `Services/MedicalRecordBackfillService.cs` | Baru — penelaahan dan penjalanan bertahap |
| `DTOs/MedicalRecordBackfillDtos.cs` | Baru |
| `Controllers/MedicalRecordBackfillController.cs` | Baru — 2 endpoint |
| `Program.cs` | Diperbarui — satu `AddScoped` |
| `tests/.../MedicalRecordBackfillTests.cs` | Baru — 11 uji |
| `roadmap/BE-08-panduan-pengisian-data-lama.md` | Baru — panduan menjalankan beserta bahan pemberitahuan |

Tiga sifat yang menurunkan risikonya:

| Sifat | Penjelasan |
|---|---|
| **Penelaahan hanya membaca** | Menjawab berapa banyak dan akan menjadi apa, tanpa mengubah apa pun. Aman dijalankan kapan saja |
| **Percobaan tidak menyimpan** | Menghitung seluruhnya lalu melaporkan hasilnya, tanpa menyentuh data. Terbukti melaporkan angka yang sama dengan penjalanan sungguhan |
| **Bertahap dan dapat dilanjutkan** | Catatan yang sudah terdaftar tidak diproses ulang, sehingga aman bila terhenti di tengah |

**Cara mundur.** Pengisian ini **hanya menambah baris** pada tabel keutuhan dan tidak mengubah
satu pun catatan klinis. Pembatalannya berupa penghapusan baris yang dibuatnya, tanpa menyentuh
data pasien. Rinciannya pada panduan bagian 4.

### Hasil penelaahan — dijalankan 26 Agustus 2026

| Yang ditelaah | Hasil | Dampaknya |
|---|---:|---|
| Total CPPT | **10** | Selesai satu potongan, penjalanan bertahap tidak diperlukan |
| Penulis tidak tercatat | **0** | Kekhawatiran penulis tidak diketahui tidak berlaku |
| Kunjungan masih berjalan | **0** | Nol risiko mengganggu pasien yang sedang dirawat |
| Tanpa kunjungan | **0** | Seluruh catatan dapat didaftarkan |
| Akan terkunci tanpa tanda tangan | **10** | Angka yang muncul di laporan kelengkapan |

Profil risikonya turun jauh dari perkiraan semula. Rinciannya pada
`BE-08-panduan-pengisian-data-lama.md` bagian 0.

**Peringatan yang tetap berlaku.** Sepuluh CPPT adalah jumlah lingkungan pengembangan, bukan
jumlah pelayanan nyata. Koneksi `Development` project ini menunjuk server bersama, bukan
produksi. **Sebelum modul dipasang di produksi, penelaahan wajib dijalankan ulang di sana**,
dan seluruh pertimbangan pada panduan kembali berlaku penuh.

### Yang tersisa sebelum `BE-08` dapat dinyatakan selesai

| No | Yang tersisa | Siapa | Keadaan |
|---:|---|---|---|
| 1 | ~~Penelaahan pada basis data~~ | — | **Selesai** 26 Agustus 2026 |
| 2 | Menjalankan percobaan, memeriksa pembagian antara terkunci dan dibatalkan | Tim pengembang | Belum |
| 3 | Memberi tahu unit rekam medis — ringan, hanya 10 catatan | Penanggung jawab modul | Belum |
| 4 | Menjalankan sungguhan, satu potongan cukup | Tim pengembang | Belum |
| 5 | Mengulang penelaahan di produksi sebelum pemasangan di sana | Tim pengembang | Belum, dan **wajib** |

---

## 5. Milestone B2 — Jejak dan kewenangan akses

### `BE-09` — Master keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-09` |
| **Status** | `TERTAHAN APPROVAL` — memerlukan SOP rekam medis untuk isi awalnya |
| **Outcome** | Petugas punya daftar keperluan akses yang dapat dipilih, bukan kotak teks kosong yang jawabannya tidak dapat dibandingkan |
| **Trace** | Arsitektur bagian 9; api-contract bagian 7 |
| **Reuse** | Pola master data yang sudah ada, misalnya `MstBillingItemCategory` |
| **Scope** | `Areas/HealthServices/MasterData/Models/MstMedicalRecordAccessPurpose.cs`; DTO; controller; configuration; migration `AddMedicalRecordAccessAuditTables` bagian master; data awal |
| **Dependency** | Tidak ada task pendahulu |
| **Acceptance criteria** | 1) Lima keperluan minimum terisi. 2) Baris `Lainnya` memiliki `IsFreeTextRequired` bernilai benar. 3) `PurposeCode` dijamin unik. 4) Endpoint `/options` mengembalikan hanya yang aktif |
| **Verification** | Uji integrasi endpoint master; pemeriksaan data awal setelah migration |
| **Risk/blocker** | **Blocker:** isi awal harus berasal dari SOP rekam medis rumah sakit yang belum tersedia. Owner: product/domain, **`OPEN`** |
| **DoD** | Master terisi; daftar keperluan disetujui unit rekam medis; endpoint berjalan |

### `BE-10` — Tabel jejak akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-10` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-024` **sudah terjawab**: masa simpan 25 tahun, terbagi per tahun |
| **Outcome** | Sistem punya tempat menyimpan catatan siapa membuka rekam medis siapa |
| **Trace** | `RM-DEC-015`, `RM-DEC-023`; ERD `jejak-akses.md`; kamus data bagian 4 |
| **Reuse** | `IdentityModel`, dengan pengecualian: penandaan hapus tidak dipakai |
| **Scope** | `Models/TrxMedicalRecordAccessLog.cs`; configuration; migration `AddMedicalRecordAccessAuditTables` bagian jejak, **termasuk rancangan pembagian tabel per periode** |
| **Dependency** | `BE-09` |
| **Acceptance criteria** | 1) Tabel terbentuk dengan empat index gabungan sesuai kamus data. 2) Pembagian tabel per periode terpasang sejak migration pertama. 3) Tidak ada endpoint yang dapat mengubah atau menghapus baris |
| **Verification** | Uji integrasi index; pemeriksaan rancangan pembagian tabel |
| **Risk/blocker** | **Blocker keras.** Memasang pembagian tabel setelah berisi puluhan juta baris menuntut waktu henti layanan. Menunda keputusan ini berarti memilih pekerjaan yang jauh lebih mahal di kemudian hari. Owner: security/privacy, **`OPEN`** |
| **DoD** | Migration teruji; tabel terbagi per tahun terpasang; penjadwalan pembuatan bagian tahun berikutnya sudah otomatis dan terpantau |

### `BE-11` — Service jejak dan kewenangan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-11` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-005`, `RM-DEC-016`, `RM-DEC-017` |
| **Outcome** | Setiap pembukaan rekam medis tercatat, dan pembukaan di luar pasien rawatan menuntut alasan lebih dulu |
| **Trace** | `RM-DEC-005`, `RM-DEC-015`, `RM-DEC-016`, `RM-DEC-017`; permission-audit-matrix |
| **Reuse** | `TrxPatientEncounter` untuk menilai kunjungan aktif |
| **Scope** | `Services/MedicalRecordAccessAuditService.cs`; pendaftaran pada `Program.cs` |
| **Dependency** | `BE-09`, `BE-10` |
| **Acceptance criteria** | 1) Pasien dengan kunjungan aktif diperlakukan `RoutineCare` tanpa diminta alasan. 2) Pasien tanpa kunjungan aktif menuntut keperluan; bila kosong, isi **tidak dikembalikan sama sekali**. 3) Jejak ditulis dan transaksinya selesai **sebelum** isi dikembalikan. 4) Bila penulisan jejak gagal, permintaan dijawab `503` dan isi tidak dikembalikan. 5) Bila penilaian kunjungan gagal, akses diperlakukan sebagai beralasan, bukan sebagai rawatan. 6) `SuperAdmin` tunduk aturan yang sama. 7) `AccessPermissionService.cs` **tidak disentuh** |
| **Verification** | `AT-RM-06`, `AT-RM-07`, `AT-RM-12`, `AT-RM-13`, `AT-RM-25`, `AT-RM-30` |
| **Risk/blocker** | Risiko: gangguan tabel jejak akan menghambat pembacaan rekam medis. Diterima sadar. `RM-DEC-017` menyentuh wilayah di luar modul dan paling mungkin ditolak owner |
| **DoD** | Tujuh acceptance criteria terbukti uji, terutama nomor 2, 4, dan 5 yang merupakan jalur gagal |

### `BE-12` — Tinjauan akses

| Field | Isi |
|---|---|
| **Task ID** | `BE-12` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-005` |
| **Outcome** | Unit rekam medis dapat memeriksa akses yang ditandai perlu ditinjau, sehingga jejak akses berguna alih-alih hanya menumpuk |
| **Trace** | `RM-DEC-005`; api-contract bagian 6 |
| **Reuse** | `BE-10` |
| **Scope** | `Services/MedicalRecordAccessReviewService.cs`; `Controllers/MedicalRecordAccessLogController.cs`; DTO |
| **Dependency** | `BE-10`, `BE-11` |
| **Acceptance criteria** | 1) Antrean tinjauan hanya memuat baris bertanda perlu ditinjau. 2) Menandai baris yang tidak perlu ditinjau ditolak. 3) Menandai ulang baris yang sudah ditinjau ditolak. 4) Tidak ada endpoint mengubah atau menghapus jejak |
| **Verification** | `AT-RM-08`, `AT-RM-29` |
| **Risk/blocker** | Risiko privasi: layar ini memuat `AccessReason` yang bertanda sensitif. Hak aksesnya harus lebih sempit daripada hak baca rekam medis |
| **DoD** | Endpoint berjalan; acceptance criteria terbukti uji; batasan hak akses tercatat pada permission matrix |

---

## 6. Milestone B3 — Penelusuran berkas

### `BE-13` — Service penggabungan riwayat

| Field | Isi |
|---|---|
| **Task ID** | `BE-13` |
| **Status** | **`SELESAI`** — dikerjakan 26 Agustus 2026, seluruh acceptance criteria terbukti uji. Lihat bagian 2 "Hasil `BE-13`" |
| **Outcome** | Riwayat klinis seorang pasien dapat diambil dari tiga belas sumber sekaligus dalam satu daftar berurut waktu |
| **Trace** | `RM-DEC-002`; `RM-CAP-004`; arsitektur bagian 5.8 |
| **Reuse** | Index `(PatientId, <tanggal>, IsDelete)` yang sudah ada pada seluruh tabel klinis; pola penggabungan `PrescriptionWorkspaceService` |
| **Scope** | `Services/MedicalRecordTimelineService.cs`. **Bertambah saat pengerjaan:** `DTOs/MedicalRecordTimelineDtos.cs`, karena service memerlukan bentuk permintaan dan balasannya sendiri. Diletakkan pada berkas terpisah supaya `DTOs/MedicalRecordDtos.cs` tetap menjadi milik `BE-14` |
| **Dependency** | `BE-02` untuk status keutuhan |
| **Acceptance criteria** | 1) Dokumen dari beberapa kunjungan tampil dalam satu daftar berurut waktu. 2) Jumlah baris dibatasi dan penyaringan tanggal berfungsi. 3) Hanya jenis dokumen yang diminta yang diambil. 4) Bila satu sumber gagal, sumber lain tetap tampil dan yang gagal ditandai. 5) Memakai `AsNoTracking` |
| **Verification** | `AT-RM-09`, `AT-RM-31` |
| **Risk/blocker** | Risiko: penggabungan tiga belas sumber dapat menghasilkan banyak query. Ditutup pembatasan wajib pada acceptance criteria nomor 2 dan 3 |
| **DoD** | Acceptance criteria terbukti uji; waktu tanggap diukur pada data yang cukup banyak |

### `BE-14` — Endpoint berkas rekam medis

| Field | Isi |
|---|---|
| **Task ID** | `BE-14` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-002` |
| **Outcome** | Frontend dapat menampilkan berkas rekam medis pasien lengkap dengan ringkasan dan riwayatnya |
| **Trace** | `RM-DEC-002`; api-contract bagian 2 |
| **Reuse** | `BE-11`, `BE-13` |
| **Scope** | `Controllers/MedicalRecordController.cs`; `DTOs/MedicalRecordDtos.cs`. Endpoint ringkasan, riwayat, detail dokumen, dan metadata penyaring |
| **Dependency** | `BE-11`, `BE-13`. **Tidak boleh dimulai sebelum `BE-03` selesai** sesuai `RM-DEC-019` |
| **Acceptance criteria** | 1) Setiap permintaan melewati pencatatan jejak lebih dulu. 2) Status keutuhan ikut dikembalikan untuk jenis dokumen yang sudah tunduk aturan. 3) Jenis dokumen yang belum tunduk ditandai jelas. 4) `PrivateNote` **tidak ada** pada respons mana pun di endpoint ini |
| **Verification** | `AT-RM-09`, `AT-RM-32`, `AT-RM-37` |
| **Risk/blocker** | Risiko: menampilkan catatan sebagai berkas resmi padahal baru CPPT yang terlindungi. Ditutup acceptance criteria nomor 3 dan `RM-FE-009` |
| **DoD** | Endpoint berjalan; jejak akses tercatat pada setiap permintaan; Swagger terisi |

### `BE-15` — Endpoint `PrivateNote`

| Field | Isi |
|---|---|
| **Task ID** | `BE-15` |
| **Status** | `TERTAHAN APPROVAL` — `RM-DEC-022` |
| **Outcome** | Catatan pribadi klinisi tidak terlihat pada pemakaian sehari-hari, tetapi tetap dapat dibuka secara sah bila benar diperlukan |
| **Trace** | `RM-DEC-022`; api-contract bagian 2; validation matrix bagian 4 |
| **Reuse** | `BE-11` |
| **Scope** | Satu endpoint pada `MedicalRecordController`; izin terpisah `MedicalRecord : ReadPrivateNote` |
| **Dependency** | `BE-11`, `BE-14` |
| **Acceptance criteria** | 1) Alasan diminta **walaupun** pasien punya kunjungan aktif. 2) Jejak tercatat dengan `AccessScope = PrivateNote`. 3) Memakai izin terpisah, bukan izin baca biasa |
| **Verification** | `AT-RM-16`, `AT-RM-37` |
| **Risk/blocker** | Risiko: penulis CPPT selama ini menganggap kolom itu sepenuhnya pribadi. `RM-DEC-022` mewajibkan mereka diberi tahu bahwa tidak demikian. Ini pekerjaan komunikasi, bukan kode |
| **DoD** | Endpoint berjalan; penulis CPPT sudah diberi tahu perubahan sifat kolom ini |

### `BE-16` — Penanganan pasien hasil penggabungan

| Field | Isi |
|---|---|
| **Task ID** | `BE-16` |
| **Status** | `TERTAHAN APPROVAL` — **prioritas rendah**. Ditetapkan sebagai pengaman pada `RM-DEC-026`; perilakunya sudah pasti (`409`), dikerjakan paling akhir di milestone B3 |
| **Outcome** | Pasien yang punya dua nomor rekam medis tidak ditampilkan riwayatnya secara terpotong tanpa peringatan |
| **Trace** | `RM-CAP-007`; validation matrix bagian 4; api-contract bagian 2 kode `409` |
| **Reuse** | `MstPatient.MergedToPatientId` yang sudah ada |
| **Scope** | Pemeriksaan pada `MedicalRecordController` sebelum riwayat diambil |
| **Dependency** | `BE-14`; keputusan closure question nomor 8 pada capability map revision 2 |
| **Acceptance criteria** | 1) Pasien dengan `MergedToPatientId` terisi dijawab `409` disertai nomor rekam medis pengganti. 2) Riwayat sebagian **tidak** ditampilkan |
| **Verification** | `AT-RM-22` |
| **Risk/blocker** | **Blocker:** kolom `MergedToPatientId` ada tetapi alur penggabungannya tidak ditemukan di controller mana pun. Perlu dipastikan lebih dulu apakah di lapangan benar ada pasien seperti ini. Bila ternyata tidak ada, task ini tetap dikerjakan sebagai pengaman, tetapi prioritasnya turun |
| **DoD** | Hasil penelusuran tercatat; perilaku `409` terbukti uji |

---

## 7. Milestone B4 — Pengerasan dan kesiapan

### `BE-17` — Uji jalur gagal lengkap

| Field | Isi |
|---|---|
| **Task ID** | `BE-17` |
| **Status** | `TERTAHAN APPROVAL` — bergantung seluruh task pendahulu |
| **Outcome** | Empat belas jalur gagal terbukti berperilaku sebagaimana dirancang, bukan hanya jalur berhasil |
| **Trace** | Acceptance test matrix bagian 3 |
| **Reuse** | `BE-00` |
| **Scope** | Project test |
| **Dependency** | `BE-03` sampai `BE-16` |
| **Acceptance criteria** | Empat belas jalur gagal pada acceptance test matrix bagian 3 seluruhnya punya uji dan lulus |
| **Verification** | Keluaran perintah uji |
| **Risk/blocker** | Risiko: jalur gagal sering dianggap pelengkap lalu dilewati saat waktu menipis. Karena itu dijadikan task tersendiri, bukan diselipkan |
| **DoD** | Empat belas uji ada dan lulus; tidak ada yang ditandai dilewati |

### `BE-18` — Swagger dan catatan rilis

| Field | Isi |
|---|---|
| **Task ID** | `BE-18` |
| **Status** | `TERTAHAN APPROVAL` |
| **Outcome** | Pemakai API mengetahui perubahan perilaku yang tidak terlihat dari bentuk permintaan maupun responsnya |
| **Trace** | api-contract bagian 8; manifest bagian dampak kompatibilitas |
| **Reuse** | Pengaturan Swagger yang sudah ada |
| **Scope** | Keterangan pada endpoint CPPT; catatan rilis |
| **Dependency** | `BE-03` |
| **Acceptance criteria** | 1) Swagger menyebut bahwa `ProviderUserId` dan `IsReadOnlyGenerated` diabaikan pada permintaan ubah. 2) Catatan rilis memuat empat perubahan perilaku pada manifest. 3) Keterangan bahwa baru CPPT yang tunduk aturan keutuhan |
| **Verification** | Pemeriksaan manual halaman Swagger dan catatan rilis |
| **Risk/blocker** | Risiko: mengabaikan kiriman klien tanpa pemberitahuan adalah praktik buruk. Task ini yang menutupnya |
| **DoD** | Swagger terbaca jelas; catatan rilis disetujui pemilik API |

---

## 8. Urutan pelaksanaan yang disarankan

```text
BE-00  (SIAP — dapat dimulai hari ini, tidak menunggu siapa pun)
   |
   +-- setelah tiga owner ditunjuk --------------------------------+
                                                                   |
BE-01 -> BE-02 -> BE-03 -> BE-08 (data lama)                       |
                     |                                             |
                     +-> BE-04 ------+                             |
                     |               +-- WAJIB dirilis bersama     |
                     +-> BE-05 -> BE-06 ------+                    |
                     |                                             |
                     +-> BE-07 (penguncian saat kunjungan ditutup) |
                                                                   |
BE-09 -> BE-10 -> BE-11 -> BE-12                                    |
                                                                   |
BE-13 -> BE-14 -> BE-15                                            |
            |                                                      |
            +-> BE-16 (butuh keputusan closure question no. 8)      |
                                                                   |
BE-17, BE-18 -------------------------------------------------------+
```

Tiga hal yang mengikat urutan di atas:

1. `BE-08` **wajib** selesai sebelum `BE-03` diaktifkan di produksi. Bila tidak, CPPT lama tidak
   punya baris keutuhan sementara CPPT baru punya, dan layar penelusuran akan menampilkan
   sebagian dokumen tanpa status tanpa penjelasan.
2. `BE-04` dan `BE-06` **wajib** dirilis bersamaan. Mengunci tanpa menyediakan addendum berarti
   tenaga klinis tidak dapat membetulkan catatan yang keliru sama sekali.
3. `BE-14` **tidak boleh** dimulai sebelum `BE-03` selesai, sesuai `RM-DEC-019`.

---

## 9. Risiko yang berdiri di atas seluruh roadmap

| Risiko | Dampak | Cara menutup | Owner |
|---|---|---|---|
| Tiga owner belum ditunjuk | 16 dari 19 task tertahan | Penunjukan owner | Manajemen rumah sakit |
| Tidak ada project test backend | Tiga perbaikan menyentuh kode berjalan tanpa jaring pengaman | `BE-00`, sudah `SIAP` | Arsitektur backend, `OPEN` |
| Jumlah data lama tidak diketahui | Lama dan dampak `BE-08` tidak dapat diperkirakan | Percobaan pada salinan data nyata | Clinical governance, `OPEN` |
| Bagian tahun baru lupa dibuat pada tabel jejak | Pembacaan rekam medis berhenti pada 1 Januari, karena gagal mencatat jejak berarti gagal membaca | Penjadwalan otomatis dan pemantauan, bukan pengingat manusia | Security/privacy, `OPEN` |
| `RM-DEC-017` menyentuh luar modul | Bila ditolak, `BE-11` berubah | Penerapan sudah dibatasi di dalam service modul | Security/privacy, `OPEN` |
| SOP rekam medis belum ada | Isi awal master keperluan akses tidak dapat ditetapkan | Permintaan SOP ke unit rekam medis | Product/domain, `OPEN` |

---

## 10. Yang sengaja tidak masuk roadmap ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Kelengkapan berkas, verifikasi koding, resume medis, peminjaman, retensi | Cakupan 4 sampai 8, rilis berikutnya menurut `RM-DEC-002` |
| Perbaikan validasi perpindahan status kunjungan | `RM-CAP-019`, di luar tiga celah yang ditetapkan `RM-DEC-019` |
| Penegakan tingkat kerahasiaan dokumen | Ditolak `RM-DEC-018` untuk rilis pertama |
| Perubahan pada `AccessPermissionService` | Ditolak arsitektur bagian 11 |
| Perapian nama domain `HealthService` menjadi `HealthServices` | Utang teknis yang harus jadi task tersendiri dengan approval pemilik arsitektur |
| Keutuhan untuk dua belas jenis dokumen selain CPPT | Rilis berikutnya, sesuai arsitektur bagian 7 |
