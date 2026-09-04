# Rekam Medis — `BE-15` Endpoint catatan pribadi klinisi

| | |
|---|---|
| Tanggal | 2026-08-27 |
| Task ID | `BE-15` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `yoga` (repository backend, tidak ada operasi Git write) |
| Trace | `RM-DEC-022`; api-contract bagian 2; validation matrix bagian 4 |
| Verifikasi | `AT-RM-16`, `AT-RM-37` |
| Migration | **Tidak ada** |
| Endpoint baru | **1** |
| Bukti | `dotnet test` → `Failed: 0, Passed: 116`. 9 uji baru, seluruhnya lulus |
| Breaking change | **Tidak** — endpoint baru; tidak ada perilaku lama yang berubah |

---

## 1. Masalah yang diselesaikan

Kolom `PrivateNote` pada CPPT sudah ada sejak lama, tetapi tidak pernah ditampilkan di layar
mana pun. Akibatnya muncul dua masalah yang saling bertolak belakang:

1. **Penulisnya menganggap kolom itu sepenuhnya pribadi**, padahal isinya tersimpan di basis
   data rumah sakit dan dapat dibaca siapa pun yang punya akses basis data — tanpa jejak apa pun.
2. **Tidak ada cara sah membukanya.** Bila suatu saat isinya benar-benar perlu diperiksa —
   penelaahan mutu, sengketa, atau permintaan resmi — tidak ada jalur yang dapat
   dipertanggungjawabkan.

`RM-DEC-022` menjawab keduanya sekaligus: kolom itu tidak dibuat rahasia, tetapi juga tidak
dibiarkan terbuka. Ia dapat dibuka lewat **satu jalur** yang berizin terpisah, selalu beralasan,
dan selalu tercatat.

## 2. Endpoint yang dibuat

| Method | Path | Hak akses |
|---|---|---|
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | `MedicalRecord : ReadPrivateNote` |

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Query | Wajib | Keterangan |
|---|:---:|---|
| `accessPurposeId` | **Ya** | Selalu wajib, apa pun keadaan kunjungan |
| `accessReason` | Bersyarat | Wajib bila keperluan yang dipilih menuntut penjelasan bebas |

Kode status:

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Catatan pribadi terbuka. Pembukaan tercatat dan akan ditelaah |
| `400` | Keperluan akses belum dipilih, atau penjelasannya belum diisi |
| `404` | Dokumen tidak ditemukan pada berkas pasien ini, **atau** jenis dokumennya memang tidak punya catatan pribadi |
| `409` | Pasien hasil penggabungan nomor rekam medis |
| `503` | Jejak akses gagal dicatat, sehingga isi tidak dikembalikan |

## 3. Tiga hal yang membuat endpoint ini berbeda

Endpoint ini duduk di controller yang sama dengan empat endpoint berkas rekam medis lainnya,
tetapi aturannya sengaja tidak sama.

| Perbedaan | Alasan |
|---|---|
| **Izin terpisah** `MedicalRecord : ReadPrivateNote` | Seseorang dapat diberi hak membaca seluruh berkas rekam medis tanpa pernah dapat membuka catatan pribadi |
| **Keperluan akses selalu wajib** | Bahkan dokter yang sedang merawat pasien itu tetap harus memilih keperluan. Untuk isi rekam medis lain, ia tidak diminta alasan |
| **Jejak bercakupan `PrivateNote`** | Agar pembukaannya dihitung terpisah pada rekap tinjauan unit rekam medis |

Alasan di balik ketiganya sama: kolom itu ditulis rekan sejawat dengan harapan bersifat pribadi,
sehingga membukanya selalu merupakan tindakan yang perlu dipertanggungjawabkan — bukan bagian
dari pekerjaan sehari-hari.

## 4. Bisnis prosesnya, urut

1. **Pengguna meminta catatan pribadi sebuah dokumen.** Hak `ReadPrivateNote` diperiksa lapisan
   izin sebelum masuk controller.
2. **Bentuk permintaan diperiksa.** Jenis dokumen yang memang tidak punya kolom catatan pribadi
   dijawab `404` di sini, **sebelum** jejak dicatat.
3. **Kewenangan dinilai dengan cakupan `PrivateNote`.** Karena cakupannya itu, keperluan akses
   menjadi wajib tanpa kecuali.
4. **Jejak ditulis dan disimpan.** Selesai sebelum isi diambil.
5. **Bila langkah 3 atau 4 gagal, permintaan berhenti** tanpa menyentuh isi catatan.
6. **Isi catatan diambil** dan dikembalikan beserta nama penulisnya.

### Contoh dua keadaan

**Dokter membuka catatan pribadi pasien yang sedang dirawatnya, tanpa memilih keperluan.**
Ditolak `400` dengan pesan *"Membuka catatan pribadi selalu memerlukan keperluan akses."* —
padahal pada menit yang sama, dokter itu dapat membuka detail dokumen yang sama tanpa diminta
alasan apa pun. Perbedaan inilah inti `RM-DEC-022`, dan diuji sebagai pembanding langsung.

**Petugas rekam medis membuka dengan keperluan "Penelaahan mutu rekam medis".** Dilayani. Jejak
tercatat sebagai akses beralasan bercakupan `PrivateNote`, ditandai untuk ditelaah, dan muncul
terpisah pada rekap tinjauan `BE-12`.

## 5. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `Areas/.../MedicalRecordManagement/Controllers/MedicalRecordController.cs` | Diperbarui | 1 endpoint |
| `Areas/.../MedicalRecordManagement/DTOs/MedicalRecordDtos.cs` | Diperbarui | `MedicalRecordPrivateNoteResponse` |
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Diperbarui | `GetPrivateNoteAsync`, `MendukungCatatanPribadi` |
| `tests/.../MedicalRecordPrivateNoteTests.cs` | Baru | 9 uji |
| `docs/.../roadmap/BE-15-pemberitahuan-penulis-cppt.md` | Baru | Bahan komunikasi untuk DoD |
| `docs/.../contracts/api-contract.md` | Diperbarui | Status endpoint dan catatan `BE-15` |

**Tidak ada tabel baru, tidak ada migration, tidak ada `AddScoped` baru.**

### Kenapa task ini sangat tipis

Sebagian besar aturannya sudah ada sejak `BE-11`. Service jejak akses sudah menegakkan "catatan
pribadi selalu menuntut keperluan" **beserta pesan khususnya**, dan sudah punya ujinya sendiri.
`BE-15` tinggal memanggilnya dengan cakupan `PrivateNote` lalu memasang izin terpisah.

Ini hasil langsung dari keputusan `BE-11` meletakkan aturan kewenangan di satu service alih-alih
di controller. Bila dahulu ditulis di controller, `BE-15` akan berarti menulis ulang aturan yang
sama untuk kelima kalinya — dan setiap penulisan ulang adalah kesempatan baru untuk keliru.

## 6. Privasi

| Hal | Perlakuan |
|---|---|
| Isi catatan pribadi | **Tidak** masuk ke logger. Yang dicatat hanya bahwa pembukaan terjadi |
| `accessReason` | **Tidak** masuk ke logger — dapat mengungkap keadaan pasien |
| Empat endpoint berkas lain | **Tetap tidak pernah** membawa isi `PrivateNote`. Hanya penanda `hasPrivateNote` |
| Dokumen milik pasien lain | `404` — dicocokkan `PatientId`, bukan hanya id dokumen |
| Penulis catatan | Namanya dikembalikan. Pembaca berhak tahu catatan pribadi siapa yang sedang ia buka |

**"Memang kosong" dibedakan dari "disembunyikan".** Dokumen yang catatan pribadinya kosong tetap
dijawab `200` dengan `hasPrivateNote = false` dan pesan *"Dokumen ini tidak memuat catatan
pribadi."* Tanpa pembedaan itu, pembaca tidak punya cara tahu mana yang benar — dan itu justru
mendorongnya mencari lewat jalur lain.

## 7. Verifikasi

```powershell
dotnet test tests\QuilvianSystemBackend.Tests\QuilvianSystemBackend.Tests.csproj
```

| Hasil | Angka |
|---|---|
| Kompilasi | **0 error**, tidak ada warning dari berkas modul rekam medis |
| Uji seluruh suite | **Failed: 0, Passed: 116, Skipped: 0** — naik dari 107 |
| Uji `BE-15` | 9 uji, seluruhnya lulus |
| Durasi | 1 menit 40 detik |

| Acceptance criteria | Uji |
|---|---|
| 1) Alasan diminta walaupun pasien punya kunjungan aktif (`AT-RM-16`) | `CatatanPribadi_TetapMenuntutAlasanWalaupunPasienSedangDirawat` |
| 1) Dengan keperluan sah, catatan benar-benar terbuka | `CatatanPribadi_TerbukaBilaKeperluanDiisi` |
| 2) Jejak bercakupan `PrivateNote` | `CatatanPribadi_JejakTercatatDenganCakupanPrivateNote` |
| 2) Terhitung terpisah pada rekap tinjauan | `PembukaanCatatanPribadi_TerhitungTerpisahPadaRekapTinjauan` |
| 3) Memakai izin terpisah | `EndpointCatatanPribadi_MemakaiIzinTerpisah` |
| Jenis tanpa catatan pribadi ditolak jelas, tanpa jejak | `JenisDokumenTanpaCatatanPribadi_Dijawab404TanpaJejak` |
| "Memang kosong" dibedakan dari "disembunyikan" | `DokumenTanpaIsiCatatanPribadi_DitandaiKosongBukanDisembunyikan` |
| Catatan pribadi pasien lain ditolak `404` | `CatatanPribadiPasienLain_Dijawab404` |
| Pasien tanpa kunjungan berjalan tetap ditolak tanpa keperluan | `PasienTanpaKunjunganBerjalan_TetapDitolakTanpaKeperluan` |

**Uji pembanding pada acceptance criteria nomor 1** patut disorot. Pada pasien yang sama dan
pengguna yang sama, detail dokumen biasa dijawab `200` tanpa keperluan akses sedangkan catatan
pribadinya dijawab `400`. Ini membuktikan aturan `RM-DEC-022` benar-benar berbeda dari aturan
isi rekam medis lain, bukan sekadar dinyatakan pada dokumen.

**Satu uji sempat gagal dan diperbaiki.** `CatatanPribadiPasienLain_Dijawab404` memakai
`Assert.IsType<ObjectResult>`, padahal `NotFound(...)` mengembalikan `NotFoundObjectResult`.
Kekeliruan ada pada ujinya, bukan pada kodenya; assertion diperbaiki menjadi tipe yang benar.
Dua warning analyzer xUnit pada berkas uji `BE-14` juga ikut dibereskan pada kesempatan yang sama.

## 8. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Lapisan hak akses sungguhan | Uji memanggil controller langsung. Izin diperiksa lewat atributnya, bukan lewat filter yang berjalan |
| Pendaftaran hak `MedicalRecord : ReadPrivateNote` | Dihasilkan atribut saat aplikasi berjalan; belum dipastikan muncul pada daftar hak akses |
| Tampilan Swagger | Atribut sudah lengkap, halamannya belum dibuka |

## 9. Yang BELUM selesai — dan bukan pekerjaan kode

**Definition of Done `BE-15` menuntut dua hal.** Endpoint berjalan — selesai. Penulis CPPT sudah
diberi tahu bahwa kolom itu ternyata dapat dibuka lewat jalur sah — **belum**.

Butir kedua bukan pekerjaan pengembang. Bahan siap pakainya sudah disiapkan pada
`docs/module-blueprints/rekam-medis/roadmap/BE-15-pemberitahuan-penulis-cppt.md`, memuat isi
pemberitahuan, langkah menjalankannya, dan tabel bukti penyampaian.

Karena itu `BE-15` dicatat **`SELESAI` sebagian** pada roadmap, bukan `SELESAI`. Menandainya
selesai penuh berarti menyatakan sesuatu yang belum terjadi.

Satu keputusan yang juga masih terbuka dan disebut pada dokumen itu: **siapa saja yang berhak
diberi izin `ReadPrivateNote`.** Bila diberikan seluas hak baca rekam medis, seluruh pengaman
pada task ini kehilangan artinya.

## 10. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 11. Task berikutnya

`BE-16` — penanganan pasien hasil penggabungan nomor rekam medis. Prioritasnya rendah dan
perilaku `409`-nya sudah berjalan sejak `BE-11`; yang tersisa adalah memastikan penelusuran
lapangan apakah pasien seperti itu benar-benar ada.

Setelah itu `BE-17` (uji jalur gagal lengkap) dan `BE-18` (Swagger dan catatan rilis) menutup
milestone B4.
