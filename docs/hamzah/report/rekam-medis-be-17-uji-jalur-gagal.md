# Rekam Medis — `BE-17` Uji jalur gagal lengkap

| | |
|---|---|
| Tanggal | 2026-08-27 |
| Task ID | `BE-17` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `yoga` (repository backend, tidak ada operasi Git write) |
| Trace | Acceptance test matrix bagian 3 |
| Verifikasi | Keluaran perintah uji |
| Migration | **Tidak ada** |
| Perubahan source aplikasi | **Tidak ada** — task ini murni pembuktian |
| Bukti | `dotnet test` → `Failed: 0, Passed: 129`. 6 uji baru, seluruhnya lulus |

---

## 1. Kenapa jalur gagal dijadikan task tersendiri

Roadmap menyatakan alasannya terus terang:

> Jalur gagal sering dianggap pelengkap lalu dilewati saat waktu menipis. Karena itu dijadikan
> task tersendiri, bukan diselipkan.

Pada modul ini alasan itu berlaku lebih keras daripada biasanya, karena **justru di jalur gagal
inilah aturan keselamatannya berada**:

| Aturan | Bila jalur gagalnya tidak diuji |
|---|---|
| Gagal mencatat jejak berarti gagal membaca | Bisa saja ada jalur membaca rekam medis tanpa jejak, dan tidak ada yang tahu |
| Dokumen wajib terdaftar pada daftar keutuhan | CPPT yang tersimpan tanpa baris keutuhan luput dari seluruh aturan penguncian **selamanya** |
| Kunjungan tidak boleh tertutup dengan dokumen terbuka | Catatan menggantung tidak akan pernah terkunci, karena pemicunya sudah lewat |
| Peran tertinggi tidak punya jalan pintas | Jalan pintas dapat masuk diam-diam pada perubahan berikutnya |

## 2. Peta lengkap empat belas jalur gagal

| No | Jalur gagal | ID | Uji | Ditutup task |
|---:|---|---|---|---|
| 1 | Mengubah dokumen terkunci | `AT-RM-01` | `MengubahCpptYangSudahDitandatangani_DitolakDanIsinyaTidakBerubah` | `BE-03` |
| 2 | Menandatangani catatan orang lain | `AT-RM-02` | `BukanPenulis_TidakDapatMenandatangani` | `BE-02` |
| 3 | Menandatangani dokumen yang sudah terkunci | `AT-RM-11` | `DokumenYangSudahTerkunci_TidakDapatDitandatanganiUlang` | `BE-02` |
| 4 | Addendum oleh yang tidak berwenang | `AT-RM-05` | `BukanPenulis_TanpaKewenanganPengganti_Ditolak` | `BE-06` |
| 5 | Addendum memakai penetapan kedaluwarsa | `AT-RM-27` | `PenetapanYangSudahLewatBatasWaktu_TidakLagiMembukaJalur` | `BE-06` |
| 6 | Penetapan tanpa batas waktu | `AT-RM-26` | `PenetapanTanpaBatasWaktu_DitolakDanTidakTersimpan` | **`BE-17`** |
| 7 | Akses tanpa alasan pada pasien tanpa kunjungan aktif | `AT-RM-07` | `PasienTanpaKunjunganAktif_TanpaKeperluan_DitolakDanIsinyaTidakDikembalikan` | `BE-11` |
| 8 | `SuperAdmin` tanpa alasan | `AT-RM-13` | `SuperAdmin_TetapDimintaAlasanDanTetapDitandaiPerluDitinjau` | **`BE-17`** |
| 9 | Penilaian kunjungan gagal | `AT-RM-25` | `PenilaianKunjunganGagal_DiperlakukanSebagaiAksesBeralasan` | `BE-11` |
| 10 | Pencatatan jejak gagal | `AT-RM-30` | `PencatatanJejakGagal_Dijawab503DanIsiTidakDikembalikan` | **`BE-17`** |
| 11 | Pendaftaran keutuhan gagal | `AT-RM-35` | `PendaftaranKeutuhanGagal_PembuatanCpptIkutDibatalkan` | **`BE-17`** |
| 12 | Penguncian saat penutupan gagal | `AT-RM-36` | `PenguncianGagalSaatKunjunganDitutup_KunjunganTetapTerbuka` | **`BE-17`** |
| 13 | Pasien hasil penggabungan | `AT-RM-22` | `PasienHasilPenggabungan_DitolakPadaSeluruhPintuMasukBerkas` | `BE-16` |
| 14 | Data lama tanpa penulis | `AT-RM-33` | `CatatanTanpaPenulis_TetapDibuatDenganPenandaPenulisTidakDiketahui` | `BE-08` |

Sepuluh sudah terbukti pada task pendahulunya. Lima ditutup pada task ini, satu di antaranya
adalah temuan — lihat bagian 5.

## 3. Cara menirukan kegagalan

Tabel yang bersangkutan **dihapus** dari basis data uji, sehingga query ke tabel itu benar-benar
gagal. Bukan disimulasikan lewat penanda maupun tiruan objek: yang diuji adalah perilaku sistem
ketika basis datanya sungguh-sungguh bermasalah.

| Uji | Tabel yang dihapus | Yang dibuktikan |
|---|---|---|
| `AT-RM-30` | `TrxMedicalRecordAccessLog` | Ketiga pintu masuk berkas menjawab `503`, dan tidak ada isi yang dikembalikan |
| `AT-RM-35` | `TrxClinicalDocumentIntegrity` | Pembuatan CPPT gagal, dan **CPPT-nya tidak tersimpan** |
| `AT-RM-36` | `TrxClinicalDocumentIntegrity` | Penutupan kunjungan gagal, dan **kunjungan tetap terbuka** |

Nama tabel diambil dari model EF, bukan dituliskan sebagai teks, sehingga uji ikut rusak bila
nama tabelnya kelak berubah — dan itu memang yang diinginkan.

## 4. Yang dibuktikan tiap uji baru

### `AT-RM-13` — `SuperAdmin` tanpa alasan

Ini keputusan yang paling mudah dilanggar tanpa sengaja. Pola umum pada banyak sistem adalah
memberi peran tertinggi jalan pintas melewati pemeriksaan. Di modul ini jalan pintas itu tidak
ada, dan **ketiadaannya perlu dibuktikan** — bukan sekadar dipercaya karena tidak ada kode yang
menuliskannya.

Uji membuat peran `SuperAdmin` sungguhan dan melekatkannya pada pengguna, lalu memeriksa dua hal:
tanpa keperluan akses ditolak `400` seperti pengguna mana pun, dan dengan keperluan akses
dilayani tetapi tetap ditandai perlu ditinjau.

### `AT-RM-30` — pencatatan jejak gagal

Diperiksa pada seluruh pintu masuk berkas, karena satu pintu yang lolos berarti ada jalur membaca
tanpa jejak. Uji juga memastikan isi catatan tidak bocor lewat pesan galat.

### `AT-RM-35` — pendaftaran keutuhan gagal

Yang paling penting bukan bahwa permintaannya gagal, melainkan bahwa **CPPT-nya benar-benar tidak
tersimpan**. CPPT yang tersimpan tanpa baris keutuhan akan luput dari seluruh pemeriksaan
`EnsureMutableAsync` selamanya — bukan karena aturannya salah, melainkan karena dokumen itu tidak
pernah terdaftar untuk diperiksa.

### `AT-RM-36` — penguncian gagal saat kunjungan ditutup

Arah sebaliknya, dan sama tegasnya: kunjungan yang tertutup sementara dokumennya masih terbuka
adalah keadaan yang dilarang `RM-DEC-003`. Lebih baik kunjungan gagal ditutup dan petugas mencoba
lagi, daripada tertutup dengan catatan menggantung.

Ditambah satu uji pembanding, `PenguncianSehat_PenutupanKunjunganBerhasil`, supaya uji di atas
benar-benar membuktikan penguncian yang gagal membatalkan penutupan — bukan sekadar membuktikan
bahwa penutupan memang tidak pernah bekerja.

## 5. Temuan: `AT-RM-26` semula belum benar-benar tertutup

Saat mengaudit kedua puluh jalur, satu jalur ternyata hanya **terlihat** tertutup.

`BE-05` sudah punya `PenetapanDenganBatasWaktuYangSudahLewat_Ditolak`, yang menguji batas waktu
**diisi tetapi sudah lewat**. Yang diminta `AT-RM-26` berbeda: kepala unit membuat penetapan
**tanpa mengisi batas waktu sama sekali**.

Bedanya bukan main-main:

> Atribut `[Required]` pada kolom tanggal yang tidak boleh kosong **tidak** menangkap keadaan
> itu. Nilai bawaan tanggal bukan nilai kosong, sehingga lolos pemeriksaan atribut.

Yang benar-benar menahannya adalah aturan "batas waktu harus setelah hari ini" di dalam service,
yang kebetulan juga menolak nilai bawaan karena nilai itu berada di masa lampau.

**Perilakunya sudah benar sejak `BE-05`, tetapi belum pernah dibuktikan.** Sekarang sudah, dan
uji itu menjaga agar aturannya tidak dilonggarkan seseorang yang mengira `[Required]` sudah cukup.

Tidak ada perubahan source yang diperlukan untuk temuan ini.

## 6. Alat bantu baru: konteks SignalR tiruan

`AT-RM-36` menuntut pengujian lewat `PatientEncounterController`, yang konstruktornya menerima
layanan antrean realtime walaupun endpoint yang diuji tidak memakainya. Tanpa tiruan, controller
itu tidak dapat dibentuk sama sekali dari uji.

`HubContextKosong<THub>` sengaja **tidak mencatat apa pun**. Bila kelak ada uji yang perlu
membuktikan pesan realtime terkirim, tiruan itu perlu diganti yang mencatat panggilannya — bukan
dipakai apa adanya lalu dianggap membuktikan sesuatu. Catatan itu ditulis pada berkasnya sendiri
supaya tidak salah dipakai.

## 7. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `tests/.../MedicalRecordFailurePathTests.cs` | Baru | 6 uji |
| `tests/.../Infrastructure/HubContextKosong.cs` | Baru | Konteks SignalR tiruan |

**Tidak ada perubahan pada source aplikasi.** Seluruh perilaku yang diuji sudah benar sebelum
task ini; yang kurang hanyalah buktinya.

## 8. Verifikasi

```powershell
dotnet test tests\QuilvianSystemBackend.Tests\QuilvianSystemBackend.Tests.csproj
```

| Hasil | Angka |
|---|---|
| Kompilasi | **0 error**, tanpa warning dari berkas modul rekam medis |
| Uji seluruh suite | **Failed: 0, Passed: 129, Skipped: 0** — naik dari 123 |
| Uji `BE-17` | 6 uji, seluruhnya lulus |
| Durasi | 1 menit 59 detik |

**Tidak ada uji yang ditandai dilewati.** `Skipped: 0` adalah bagian dari Definition of Done task
ini, bukan sekadar angka pelengkap.

## 9. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Lapisan HTTP dan hak akses | Uji memanggil controller langsung, sesuai keterangan pada `ControllerTestHarness` |
| Perilaku pada PostgreSQL | Uji berjalan di SQLite dalam memori. Kegagalan ditirukan dengan menghapus tabel, yang perilakunya setara pada kedua basis data, tetapi belum dijalankan terhadap PostgreSQL |
| Jalur gagal di luar daftar empat belas | Acceptance test matrix bagian 4 mendaftar hal yang memang tidak dapat diuji otomatis |

## 10. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 11. Task berikutnya

`BE-18` — Swagger dan catatan rilis. Ini task terakhir modul.

Di luar kode, tiga butir masih menahan penyelesaian penuh modul: isi awal master keperluan akses
(`BE-09`), penjalanan pengisian data lama (`BE-08`), dan pemberitahuan penulis CPPT (`BE-15`).
