# Rekam Medis — `BE-13` Service penggabungan riwayat tiga belas sumber

| | |
|---|---|
| Tanggal | 2026-08-26 |
| Task ID | `BE-13` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `master` (repository backend, tidak ada operasi Git write) |
| Trace | `RM-DEC-002`; `RM-CAP-004`; arsitektur backend bagian 5.8 |
| Verifikasi | `AT-RM-09`, `AT-RM-31` |
| Migration | **Tidak ada** |
| Endpoint baru | **Tidak ada** — endpoint menyusul pada `BE-14` |
| Breaking change | **Tidak** — hanya penambahan; tidak ada perilaku lama yang berubah |

---

## 1. Masalah yang diselesaikan

Untuk menampilkan satu halaman rekam medis pasien, frontend saat ini harus memanggil sampai
**tiga belas endpoint terpisah**, masing-masing dengan penomoran halaman sendiri, lalu
mengurutkan hasilnya sendiri di layar. Tidak ada satu pun endpoint yang menggabungkan
ketiga belas sumber itu menjadi satu riwayat berurut waktu.

Ini gap `RM-CAP-004`, berstatus audit `Reuse with adapter`: yang kurang bukan tabelnya,
melainkan **lapisan penggabung** di atas tabel-tabel yang sudah ada.

Akibat nyata di lapangan: riwayat pasien lintas kunjungan praktis tidak dapat dibaca utuh.
Petugas harus membuka kunjungan satu per satu dan menyusun urutannya di kepala sendiri.

## 2. Yang dikerjakan

Satu service pembaca, `MedicalRecordTimelineService`, yang menggabungkan tiga belas sumber
dokumen klinis menjadi satu daftar berurut waktu.

**Tidak ada tabel baru. Tidak ada migration. Tidak ada perubahan pada tabel klinis mana pun.**
Ini sesuai `RM-DEC-013`, yang menolak menempelkan kolom status ke tiga belas tabel yang sedang
dipakai IGD, antrean dokter, dan farmasi.

### Tiga belas sumber yang digabungkan

| Jenis dokumen | Tabel asal | Kolom waktu yang dipakai |
|---|---|---|
| CPPT | `TrxPatientIntegratedProgressNote` | `NoteDateTime` |
| Konsultasi Dokter | `TrxDoctorConsultation` | `ConsultationDateTime` |
| Asesmen Pasien | `TrxPatientAssessment` | `AssessmentDateTime` |
| Diagnosis | `TrxPatientDiagnosis` | `DiagnosisDateTime` |
| Tindakan | `TrxPatientProcedure` | `ProcedureDateTime` |
| Tanda Vital | `TrxPatientVitalSign` | `ObservationDateTime` |
| Alergi | `TrxPatientAllergy` | `ReportedDateTime` |
| Riwayat Penyakit | `TrxPatientMedicalHistory` | `RecordedDateTime` |
| Riwayat Keluarga | `TrxPatientFamilyHistory` | `RecordedDateTime` |
| Dokumen Klinis | `TrxPatientClinicalDocument` | `DocumentDateTime` |
| Lampiran Catatan | `TrxClinicalNoteAttachment` | `UploadedAt` |
| Surat Keterangan | `TrxMedicalCertificate` | `CertificateDateTime` |
| Persetujuan Tindakan | `TrxPatientConsent` | `ConsentDateTime` |

Ketiga belas tabel memakai nama kolom waktu yang berbeda-beda. Itulah sebabnya penyaringan
tanggal dan pengurutan dibangun secara umum, bukan ditulis tiga belas kali.

## 3. Bisnis prosesnya, urut

1. **Pemanggil menyebut apa yang ingin dilihat.** Pasien mana, jenis dokumen apa saja, rentang
   tanggal berapa, kunjungan tertentu atau seluruhnya, halaman keberapa, dan apakah dokumen
   yang dibatalkan ikut ditampilkan.
2. **Service menanyakan hanya jenis yang diminta.** Satu jenis dokumen berarti satu tabel yang
   dibaca. Bila jenisnya tidak disebut, seluruh tiga belas dibaca.
3. **Setiap sumber dibatasi.** Rentang tanggal, penyaring kunjungan, dokumen terhapus, dan
   dokumen dibatalkan diterapkan di sisi basis data, bukan setelah data terlanjur diambil.
4. **Hasilnya digabung dan diurutkan menurut waktu kejadian**, lalu dipotong sesuai halaman.
5. **Baris yang benar-benar ditampilkan dilengkapi status keutuhan** dokumennya, lewat satu
   query tambahan yang kecil.
6. **Sumber yang gagal dibaca disebut namanya** pada hasil, bukan didiamkan.

### Contoh

Pasien dengan tiga kunjungan berbeda:

| Dokumen | Kunjungan | Waktu |
|---|---|---|
| CPPT | Kunjungan 1 | 90 hari lalu |
| Tanda Vital | Kunjungan 1 | 91 hari lalu |
| Alergi | Kunjungan 2 | 30 hari lalu |
| CPPT | Kunjungan 2 | 29 hari lalu |
| Persetujuan Tindakan | Kunjungan 3 | 2 hari lalu |
| CPPT | Kunjungan 3 | 1 hari lalu |

Satu permintaan mengembalikan keenamnya sebagai **satu daftar berurut waktu**, lengkap dengan
nomor kunjungan masing-masing. Petugas tidak perlu membuka kunjungan satu per satu.

## 4. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | Baru | Penggabungan tiga belas sumber, pembatasan, isolasi kegagalan |
| `Areas/HealthServices/MedicalRecordManagement/DTOs/MedicalRecordTimelineDtos.cs` | Baru | Bentuk permintaan, baris riwayat, sumber gagal, dan hasil |
| `Program.cs` | Diperbarui | Satu baris `AddScoped<MedicalRecordTimelineService>()` |
| `tests/QuilvianSystemBackend.Tests/MedicalRecordManagement/MedicalRecordTimelineTests.cs` | Baru | 10 uji |

**Selisih terhadap scope roadmap.** Roadmap menyebut scope `Services/MedicalRecordTimelineService.cs`
saja. Berkas DTO ditambahkan karena service memerlukan bentuk permintaan dan balasannya sendiri.
Diletakkan pada berkas terpisah — bukan pada `DTOs/MedicalRecordDtos.cs` — supaya berkas itu
tetap menjadi milik `BE-14` dan tidak ada dua task yang menyunting berkas yang sama.

## 5. Pembatas yang berlaku

Arsitektur bagian 5.8 menyebut risikonya terang-terangan: menggabungkan tiga belas sumber
berpotensi menghasilkan banyak query. Pembatasnya wajib, bukan pilihan.

| Pembatas | Nilai | Alasan |
|---|---|---|
| Ukuran halaman bawaan | 25 baris | Cukup untuk satu layar |
| Ukuran halaman maksimal | 100 baris | Permintaan lebih besar **dipotong**, bukan ditolak |
| Batas baris per sumber | 500 baris | Pengaman terakhir; bila tersentuh, hasilnya ditandai terpotong |
| Penyaring jenis dokumen | Opsional | Cara termurah menekan biaya permintaan |
| Penyaring rentang tanggal | Opsional | Diterapkan pada kolom waktu masing-masing sumber |

### Dua query per sumber, bukan satu

Satu query menghitung jumlah dokumen yang cocok, satu lagi mengambil barisnya.

Penghitungan dipilih dengan sadar. Tanpa itu, jumlah total pada layar hanya sebesar isi halaman,
dan tombol "halaman berikutnya" kehilangan artinya. Query penghitungan tidak mengambil baris apa
pun dan dilayani index `(PatientId, tanggal, IsDelete)` yang **sudah ada** di tiga belas tabel
klinis, jadi biayanya kecil.

Konsekuensinya harus dipahami pemakai kontrak:

| Permintaan | Jumlah query |
|---|---|
| Seluruh jenis dokumen | 26 query + 1 query status keutuhan |
| Satu jenis dokumen | 2 query + 1 query status keutuhan |

**Menyebut jenis dokumen adalah cara termurah menekan biaya permintaan.** Layar rekam medis
sebaiknya tidak meminta seluruh tiga belas jenis sekaligus kecuali memang diperlukan.

### Penanda terpotong

Bila sebuah sumber menyentuh batas 500 baris, hasilnya ditandai terpotong. Jumlah totalnya tetap
benar karena dihitung terpisah; yang belum tentu lengkap adalah **isi halaman yang jauh**.
Penandanya ada supaya layar dapat menyarankan mempersempit rentang tanggal, bukan menampilkan
daftar yang kurang tanpa memberi tahu.

## 6. Satu sumber gagal bukan berarti seluruhnya gagal

Setiap sumber dibaca terpisah dan dibungkus penanganan kesalahannya sendiri. Bila satu tabel
bermasalah, sumber lain tetap dikembalikan dan yang gagal dicatat lengkap dengan nama jenis
dokumen dan pesan kesalahannya.

Pilihan ini disengaja: **riwayat yang hilang seluruhnya lebih berbahaya bagi pelayanan daripada
riwayat yang kurang satu jenis — asalkan kekurangannya dinyatakan, bukan disembunyikan.**

Pembatalan permintaan oleh pemanggil dikecualikan; itu bukan kegagalan sumber dan diteruskan
apa adanya.

## 7. Privasi dan keamanan

| Hal | Perlakuan |
|---|---|
| Isi catatan klinis | **Tidak** ikut pada daftar riwayat. Hanya nomor dokumen, judul pendek, dan keterangan singkat dari kolom yang panjangnya sudah terbatas |
| `PrivateNote` | **Tidak pernah** ikut pada daftar riwayat mana pun dari service ini |
| Riwayat pasien lain | Diuji tersendiri, tidak pernah ikut terbawa |
| Kewenangan dan jejak akses | **Bukan tanggung jawab service ini.** Ditegakkan `MedicalRecordAccessAuditService` (`BE-11`), dipanggil controller **sebelum** service ini dipakai — lihat `BE-14` |
| Penulisan data | Tidak ada. Seluruh query `AsNoTracking`, tidak ada transaksi, tidak ada `SaveChanges` |

Perlu ditegaskan untuk `BE-14`: service ini **tidak** memeriksa apakah pengguna berhak membuka
rekam medis pasien tersebut. Pemeriksaan itu wajib dilakukan lebih dulu di controller, sesuai
acceptance criteria `BE-14` nomor 1.

## 8. Status keutuhan dokumen

Baris yang ditampilkan dilengkapi status keutuhannya bila dokumen itu terdaftar pada
`TrxClinicalDocumentIntegrity`. Pencocokannya memakai pasangan **jenis dan id**, bukan id saja,
karena id dokumen hanya unik di dalam tabel asalnya.

Setiap baris juga membawa penanda apakah jenis dokumennya **sudah tunduk** aturan keutuhan.
Rilis pertama hanya menegakkan CPPT (`RM-DEC-019`); dua belas jenis lain belum. Keadaan itu
wajib dinyatakan terbuka di layar sesuai `RM-FE-009` — jangan menampilkan alergi seolah-olah
sudah terlindungi aturan keutuhan.

## 9. Verifikasi yang dijalankan

| Perintah | Hasil |
|---|---|
| `dotnet build QuilvianSystemBackend.csproj` | **0 error**, 125 warning (seluruhnya warning lama yang sudah ada sebelum task ini) |
| `dotnet test tests\QuilvianSystemBackend.Tests` — hanya `MedicalRecordTimelineTests` | **Failed: 0, Passed: 10** |
| `dotnet test tests\QuilvianSystemBackend.Tests` — seluruh suite | **Failed: 0, Passed: 96** (sebelumnya 86) |

Uji berjalan di atas basis data SQLite dalam memori yang dibentuk dari konfigurasi EF Core yang
sama dengan aplikasi. **Tidak ada basis data bersama yang disentuh.**

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
| Permintaan tanpa id pasien ditolak | `TanpaIdPasien_PermintaanDitolak` |

**Cara membuktikan sumber gagal.** Uji nomor 4 menghapus satu tabel dari basis data uji,
sehingga pembacaannya benar-benar gagal — bukan disimulasikan lewat penanda. Hasilnya: dua
sumber lain tetap terbaca, dan `Persetujuan Tindakan` muncul pada daftar sumber gagal.

## 10. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Waktu tanggap pada data sungguhan | Bagian DoD `BE-13` menyebut "waktu tanggap diukur pada data yang cukup banyak". Uji berjalan pada SQLite dalam memori, bukan SQL Server berisi data nyata. **Pengukuran ini masih tersisa** |
| Perilaku pada SQL Server | Query dibangun EF Core dan tidak memakai SQL mentah, jadi risikonya kecil — tetapi belum dijalankan terhadap SQL Server |
| Jalur endpoint | Belum ada endpoint. Menyusul `BE-14` |

## 11. Delta kontrak untuk `BE-14`

`contracts/api-contract.md` bagian 2 menyebut balasan `GET /{patientId}/timeline` berbentuk
`ApiResponse<PagedResult<MedicalRecordTimelineItemResponse>>`.

Bentuk itu **tidak punya tempat** untuk daftar sumber gagal, padahal acceptance criteria `BE-13`
nomor 4 mewajibkan kekurangan dinyatakan. Karena `BE-13` hanya lapisan service, keputusannya
tidak diambil sepihak di sini: service mengembalikan `MedicalRecordTimelineResult` yang memuat
halaman **beserta** daftar sumber gagal, daftar jenis yang diminta, dan penanda terpotong.

`BE-14` yang menentukan bagaimana keterangan itu tampil pada balasan endpoint. Dua pilihan yang
masuk akal: menambah selubung balasan tersendiri, atau menyampaikannya lewat pesan pada
`ApiResponse`. Keduanya perlu persetujuan pemilik API, yang sampai sekarang masih `OPEN`.

## 12. Risiko yang tersisa

| Risiko | Penilaian |
|---|---|
| Permintaan seluruh jenis menghasilkan 27 query | **Diketahui dan dibatasi.** Pembatas jenis, tanggal, dan jumlah baris sudah wajib. Layar sebaiknya menyebut jenis dokumen |
| Halaman yang sangat jauh belum tentu utuh | **Dinyatakan** lewat penanda terpotong. Bukan kegagalan diam-diam |
| Dua belas jenis dokumen belum tunduk aturan keutuhan | **Dinyatakan** per baris. Wajib ditampilkan layar sesuai `RM-FE-009` |
| Pasien hasil penggabungan nomor rekam medis | **Belum ditangani di sini.** Itu scope `BE-16`, diperiksa di controller sebelum riwayat diambil |

## 13. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan berada di worktree sebagai:

| Berkas | Keadaan |
|---|---|
| `Areas/.../Services/MedicalRecordTimelineService.cs` | Baru, belum di-stage |
| `Areas/.../DTOs/MedicalRecordTimelineDtos.cs` | Baru, belum di-stage |
| `tests/.../MedicalRecordTimelineTests.cs` | Baru, belum di-stage |
| `Program.cs` | Diubah, belum di-stage |
| `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` | Diubah, belum di-stage |
| `docs/module-blueprints/rekam-medis/roadmap/requirement-traceability.md` | Diubah, belum di-stage |

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 14. Task berikutnya

`BE-14` — endpoint berkas rekam medis. Dependency-nya sudah terpenuhi: `BE-03` selesai
(syarat `RM-DEC-019`), `BE-11` selesai, dan `BE-13` selesai lewat laporan ini.
