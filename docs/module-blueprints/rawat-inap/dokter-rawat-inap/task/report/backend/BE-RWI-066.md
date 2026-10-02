# Laporan Perubahan Backend — `BE-RWI-066`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-066` |
| Judul | Balasan catatan terpadu menyebutkan siapa yang memverifikasi dan kapan |
| Slice | `DOK-MVP-7` — menutup tiga celah kontrak yang ditemukan layar |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 kartu `BE-RWI-066`; bagian 0.1 dan 5 |
| Trace | `CAP-021`; `AC-CAP021-03`; `INV-DOK-11`; `contracts/api-contract.md` bagian 3; `03-frontend-architecture.md` bagian 3.4; `RWI-DEC-062` untuk kewenangan menyentuh modul `ClinicalManagement`. **Ditemukan** saat `FE-RWI-046` dikerjakan |
| Contract version | `0.3.0` — **tidak berubah**. Kontrak mengikat pada tingkat endpoint, hak akses, dan jenis balasan (`ApiResponse<ProgressNoteResponse>`), bukan pada daftar kolomnya. Melengkapi kolom balasan **memenuhi** kontrak yang sudah disetujui |
| Dependency | `BE-RWI-053` ✅ selesai 4 September 2026. Tidak menunggu `BE-RWI-067` |
| Klasifikasi | `MEDIUM` — skor 6: repository 0 (satu repository), berkas diperiksa 1 (9–20), berkas diubah 1 (4–8), logika bisnis 1 (sedang), kontrak API 1 (memakai kontrak yang sudah ada), database 1 (hanya perilaku query yang sudah ada), keamanan/auth 1 (berkaitan tetapi bukan intinya), UI/workflow 0 |
| Task mode | `BACKEND` — backend target tulis, frontend rujukan hanya-baca |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, laporan ini, roadmap, dan `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | Dikerjakan di atas `21b8575db3ea941862b7c5894a662d5ab3719934`, branch `MHamzah`. Agent tidak menjalankan satu pun tindakan Git |
| Tanggal | 8 September 2026 |
| Status | ✅ **SELESAI.** Kelima dari keenam acceptance criteria terpenuhi penuh dan terpetakan ke source. **Kriteria 2 terpenuhi pada bagian yang dituntutnya** — balasan menyebut nama verifikator — sementara **butir Verification "nama terbaca dari snapshot" tidak dapat dijalankan** karena kolom snapshot nama verifikator tidak ada pada tabel, dan menambahkannya berarti kolom tabel baru beserta migration yang dilarang kartu task yang sama. Selisih itu dilaporkan pada bagian 7, bukan ditambal. Validasi nyata: `dotnet build` **0 error**; `dotnet test` **`Failed: 0, Passed: 470, Total: 470`**, 9 di antaranya uji baru task ini. **Nol migration dibuat, nol kolom tabel bertambah** |

---

## 1. Masalah yang diperbaiki

Sejak `BE-RWI-040`, tabel catatan terpadu sudah punya empat kolom verifikasi, dan sejak
`BE-RWI-053` keempatnya sudah **benar-benar terisi** setiap kali DPJP memverifikasi sebuah
catatan. Tetapi tidak satu pun dari keempatnya ikut terkirim ke layar.

Akibatnya bagi pengguna, seperti yang ditemukan saat `FE-RWI-046` dikerjakan:

> Seorang DPJP membuka lini masa catatan terpadu pasiennya. Ia baru saja menekan **Verifikasi**
> pada catatan perawat, dan servernya menjawab berhasil. Namun layar tetap menuliskan
> **"Status verifikasi belum dapat dipastikan"** dan **"Nama verifikator belum tersedia dari
> layanan ini"** — karena balasan yang baru saja diterimanya tidak menyebutkan siapa pun.

Layar sudah mengambil keputusan yang benar: alih-alih menebak "sudah diverifikasi", ia berkata
terus terang bahwa statusnya tidak dapat dipastikan. Itu pilihan yang tepat pada rekam medis,
tetapi bukan yang dijanjikan `AC-CAP021-03`, dan itulah yang menahan `FE-RWI-046` pada status
🟡 `SEBAGIAN`.

**Contoh berangka.** Sebuah perawatan memuat 10 catatan. Dua di antaranya menunggu verifikasi
dan muncul pada daftar pantau, sehingga statusnya jelas. Delapan sisanya tidak muncul di mana
pun; yang tersedia hanya rekapitulasi "5 sudah diverifikasi, 3 tidak diwajibkan". Layar tidak
punya cara menentukan catatan mana yang termasuk yang lima dan mana yang termasuk yang tiga,
sehingga kedelapan-delapannya ditandai belum dapat dipastikan. Sesudah task ini, kesepuluhnya
menyebut status, waktu, dan nama verifikatornya sendiri-sendiri.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Dokter dan supervisor dapat melihat langsung bahwa sebuah catatan sudah diverifikasi, oleh siapa, dan pukul berapa — sementara nama penulis aslinya tetap berdiri di tempatnya |
| Pelaku | DPJP yang memverifikasi; siapa pun yang berhak membaca catatan terpadu |
| Pemicu | Membuka lini masa catatan terpadu, membuka satu catatan, atau menekan tombol Verifikasi |

### 2.2 Alur normal

1. Perawat menulis catatan pada lembar terpadu. Catatan lahir berstatus **tidak diwajibkan**
   diverifikasi, karena kebijakan `RWI-RULE-021` belum disahkan.
2. Bila kebijakan verifikasi kelak aktif, catatan berstatus **menunggu verifikasi** beserta
   batas waktunya.
3. DPJP membuka lini masa perawatan. Setiap catatan kini menyebutkan **empat hal** sekaligus:
   keadaan verifikasinya, waktu verifikasinya, siapa verifikatornya, dan batas waktunya.
4. DPJP menekan **Verifikasi** pada catatan profesi lain. Balasannya langsung menyebut dirinya
   sebagai verifikator beserta waktunya — tidak perlu memuat ulang halaman untuk mengetahuinya.
5. Penulis catatan **tidak bergeser satu huruf pun**. Perawat yang menulisnya tetap tercatat
   sebagai penulis; DPJP tercatat sebagai verifikator. Dua orang, dua kolom, dua tanggung jawab.

### 2.3 Jalur tidak normal

| Keadaan | Yang dikembalikan balasan |
| --- | --- |
| Catatan belum pernah diverifikasi | Waktu verifikasi dan verifikator **kosong** — bukan `0001-01-01`, dan bukan teks kosong yang terbaca sebagai nama orang |
| Kebijakan verifikasi tidak aktif | Keadaan `NotRequired`, sehingga layar dapat membedakan "tidak diwajibkan" dari "sudah diverifikasi" tanpa menebak |
| Baris verifikator sudah tidak dapat dikenali | Nomor verifikator tetap dikembalikan apa adanya, namanya **kosong**. Nama penulis **tidak pernah** dipakai sebagai penggantinya |
| Catatan dikoreksi setelah diverifikasi | `BE-RWI-053` mengembalikannya ke keadaan menunggu, dan balasan ini ikut menyebutkan keadaan barunya. Tidak ada perubahan perilaku dari task ini |
| Catatan poliklinik, medical check-up, dan IGD | Seluruh kolom lamanya tetap sama persis. Kolom verifikasinya ikut terkirim dan berbunyi tidak-diwajibkan, karena memang begitu nilainya tersimpan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap dan kontrak: kartu `BE-RWI-066` beserta bagian 0.1 dan 5 `backend-roadmap.md`;
  `contracts/api-contract.md` bagian 3; laporan [`FE-RWI-046`](../frontend/FE-RWI-046.md) yang
  menemukan celahnya.
- Source: `PatientIntegratedProgressNoteController.cs`, `PatientIntegratedProgressNoteDtos.cs`,
  `TrxPatientIntegratedProgressNote.cs`, `TrxPatientIntegratedProgressNoteConfiguration.cs`,
  `CpptVerificationService.cs`, `CpptVerificationStatus.cs`, `ApplicationUser.cs`,
  `Migrations/ApplicationDbContextModelSnapshot.cs`.
- Uji: `CpptVerificationTests.cs` beserta `Infrastructure/` sebagai pola.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` | Menambah navigation `VerifiedByUser`. Relasinya sendiri **sudah ada** sejak `BE-RWI-040`, tetapi tanpa navigation sehingga namanya tidak pernah dapat ikut dibaca |
| `Repositories/Configurations/HealthServices/TrxPatientIntegratedProgressNoteConfiguration.cs` | `HasOne<ApplicationUser>()` untuk `VerifiedByUserId` dipasangkan ke navigation itu. Kolom kunci asing, principal key, index, dan perilaku hapusnya **tidak berubah sedikit pun** |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs` | Lima kolom verifikasi pada `PatientIntegratedProgressNoteResponse` — yang otomatis ikut terbawa `PatientIntegratedProgressNoteDetailResponse` karena ia mewarisinya — dan lima kolom yang sama pada `PatientIntegratedProgressNoteTimelineResponse` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Tiga `Include` relasi verifikator pada query yang menghasilkan balasan itu; pemetaan lima kolom pada `ToResponse`, `ToDetailResponse`, dan `ToTimelineResponse`; helper `NamaVerifikator` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/CpptVerificationResponseTests.cs` | **Baru.** Sembilan uji yang menutup keenam acceptance criteria beserta risiko jumlah query |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/TestDatabase.cs` | Parameter opsional `catatEksekusi` pada `CreateContext`, supaya uji dapat **menghitung** berapa perintah SQL yang benar-benar dijalankan sebuah pembacaan. Kosong pada pemakaian biasa, sehingga nol biaya bagi uji lain |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif dan kompatibel mundur.** Lima kolom bertambah pada tiga bentuk balasan; nol kolom lama diganti nama, dihapus, atau berubah arti. Nol endpoint baru, nol hak akses baru. Kontrak `0.3.0` tetap berlaku apa adanya |
| Database | **Nol migration dibuat, nol kolom tabel bertambah, nol perintah database dijalankan.** Buktinya ada pada `Migrations/ApplicationDbContextModelSnapshot.cs` baris 96230–96233: relasi `VerifiedByUserId` → `ApplicationUser` beserta `HasIndex("VerifiedByUserId")` **sudah tercatat** di sana sejak `BE-RWI-040`, hanya tanpa nama navigation. Perubahan task ini mengubah `null` menjadi nama navigation, dan navigation adalah konsep model CLR — bukan schema. Rinciannya pada bagian 7 |
| Keamanan/Auth | **Nol perubahan.** Ketiga endpoint yang terdampak memakai `[AccessAction]` dan `[AccessPermission]` yang sama persis seperti sebelumnya, dan tidak satu pun ditambah, diganti nama, atau dilonggarkan. Nol `IsInRole`, nol nama peran, nol `UserType` dipakai sebagai penentu kewenangan. Nama verifikator adalah keterangan yang memang boleh dibaca siapa pun yang sudah berhak membaca catatannya |

---

## 4. Dokumentasi endpoint

Nol endpoint dibuat. Ketiga endpoint berikut **sudah ada** dan hanya balasannya yang bertambah
kolom.

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/timeline` | Lini masa catatan pasien atau kunjungan; kini menyebut keadaan verifikasi setiap catatan beserta nama verifikatornya | `PatientIntegratedProgressNote : Read` |
| `GET` | `/episodes/{episodeId}` | Lini masa lintas profesi satu perawatan rawat inap; kini menyebut keadaan verifikasi setiap catatan beserta nama verifikatornya | `PatientIntegratedProgressNote : Read` |
| `PATCH` | `/{id}/verify` | DPJP memverifikasi catatan profesi lain; balasannya kini langsung menyebut verifikator beserta waktunya | `PatientIntegratedProgressNote : Verify` |
| `GET` | `/` dan `/{id}` | Daftar dan detail CPPT; ikut membawa kolom yang sama karena memakai pemetaan yang sama | `PatientIntegratedProgressNote : Read` |

**Lima kolom yang bertambah pada balasan:**

| Kolom | Jenis | Arti |
| --- | --- | --- |
| `verificationStatus` | angka enum `CpptVerificationStatus` | `0` tidak diwajibkan, `1` menunggu, `2` sudah diverifikasi, `3` lewat batas. Dikembalikan **apa adanya seperti yang tersimpan**; `Overdue` diturunkan dari batas waktu pada daftar pantau dan tidak dihitung ulang di sini |
| `verifiedAt` | waktu, boleh kosong | Saat verifikasi. Kosong berarti belum diverifikasi |
| `verifiedByUserId` | nomor pengguna, boleh kosong | Verifikator. **Bukan** penulis pada `providerUserId` |
| `verifiedByUserName` | teks, boleh kosong | Nama verifikator. Kosong berarti belum diverifikasi atau verifikatornya tidak dapat dikenali |
| `verificationDueAt` | waktu, boleh kosong | Batas waktu verifikasi. Kosong berarti catatan ini tidak dipantau |

---

## 5. Verifikasi

Seluruh angka berasal dari eksekusi nyata pada 8 September 2026.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Kompilasi berhasil, **0 error `CS`** | `PASS` untuk kompilasi; `EXISTING / ENVIRONMENT ISSUE` untuk penyalinan | Keluaran build memuat `0` baris `error CS`. Dua error yang muncul adalah `MSB3027`/`MSB3021`: `bin\Debug\net9.0\QuilvianSystemBackend.exe` dan `.dll` **terkunci proses aplikasi backend yang sedang dijalankan pengguna** (`QuilvianSystemBackend` PID 8476). Bukan kesalahan kode, dan proses pengguna tidak dimatikan |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` — keluaran dialihkan ke luar repository | `Failed: 8, Passed: 462, Total: 470` | `EXISTING / ENVIRONMENT ISSUE` | Kedelapan kegagalannya `NursingAssessmentAccessContractTests.SourceBaru_TidakMemakaiHardcodeRole` dengan pesan `Akar repository tidak ditemukan dari lokasi uji` — uji itu menaiki folder dari `AppContext.BaseDirectory` mencari `QuilvianSystemBackend.sln`, dan pengalihan keluaran membuatnya tidak akan pernah ketemu. Kedelapan berkas yang dipindainya milik slice `keperawatan`; nol di antaranya disentuh task ini |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` — keluaran di dalam repository | **`Failed: 0, Passed: 470, Skipped: 0, Total: 470`**, durasi 5 menit 34 detik | `PASS` | Baris ringkasan `Passed!` dari runner. **9 di antaranya uji baru `BE-RWI-066`** |
| Uji `AC 1, 2, 3` — verifikasi membalas dua nama pada dua kolom | Verifikator bernama, penulis tetap penulis, keduanya berbeda | `PASS` | `Verify_MembalasNamaVerifikatorDanNamaPenulisPadaDuaKolomBerbeda` |
| Uji `AC 3` — nol kolom penulis bergeser sebelum dan sesudah | Enam kolom penulis dibandingkan sebelum dan sesudah; seluruhnya sama | `PASS` | `Verify_TidakMengubahSatuPunKolomPenulisSebelumDanSesudah` |
| Uji `AC 1` — nilai sama persis dengan barisnya | Keempat nilai tersimpan dibandingkan dengan yang dibalas | `PASS` | `LiniMasaPerawatan_MembawaNilaiVerifikasiSamaPersisDenganBarisnya` |
| Uji `AC 1` — `GET /timeline` ikut membawanya | Lini masa pasien menyebut keadaan verifikasi beserta namanya | `PASS` | `Timeline_IkutMembawaKeadaanVerifikasiBesertaNamanya` |
| Uji `AC 4` — kosong tetap kosong | Waktu dan verifikator kembali `null`, bukan `0001-01-01` dan bukan teks kosong | `PASS` | `BelumDiverifikasi_MengembalikanVerifikatorDanWaktunyaKosong` |
| Uji `AC 5` — tidak-diwajibkan terbaca sebagai tidak-diwajibkan | Keadaan `NotRequired` dikembalikan, dan terbukti **bukan** `Verified` | `PASS` | `KebijakanTidakAktif_MengembalikanNotRequiredBukanDiverifikasi` |
| Uji `AC 2` — verifikator tak dikenali tidak menjadi nama penulis | Nama verifikator `null`, dan terbukti berbeda dari nama penulis | `PASS` | `VerifikatorTidakDikenali_NamanyaKosongDanBukanNamaPenulis` |
| Uji `AC 6` — jalur non-rawat-inap tidak berubah | Sebelas kolom lama dibandingkan satu per satu dan seluruhnya sama | `PASS` | `CatatanDiLuarRawatInap_KolomLamanyaTidakBerubahSatuPun` |
| Uji risiko jumlah query | Lima catatan dengan lima verifikator berbeda dibaca sekaligus; **tepat 2 perintah SQL** dijalankan — satu menghitung total, satu mengambil halaman. Tanpa `Include` jumlahnya menjadi tujuh | `PASS` | `LiniMasaBanyakCatatan_SatuPembacaanTanpaQueryTambahanPerBaris` |
| Pemeriksaan rahasia pada berkas yang berubah | Nol password, token, connection string, key, maupun nilai konfigurasi sensitif | `PASS` | Tinjauan diff |

**Uji manual:** `NOT APPLICABLE` — task ini menambah kolom balasan pada endpoint yang sudah ada,
dan seluruh perilakunya dapat dibuktikan uji otomatis pada tingkat controller. Menjalankan
aplikasi backend memerlukan wewenang runtime terpisah dan tidak diminta task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `dotnet ef migrations add` | Task ini dilarang membuat migration, dan memang tidak membutuhkannya — lihat bagian 7 |
| Perintah database apa pun | Perubahan model tidak memberi wewenang eksekusi database. Nol perintah dijalankan terhadap basis data mana pun |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres` | Tidak diminta task ini, dan celah yang ditutup task ini tidak menyentuh perilaku PostgreSQL yang berbeda dari SQLite |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Balasan `GET /timeline`, `GET /episodes/{episodeId}`, dan `PATCH /{id}/verify` memuat `VerificationStatus`, `VerifiedAt`, `VerifiedByUserId`, dan `VerificationDueAt` dengan nilai yang sama persis dengan yang tersimpan pada barisnya | **Terpenuhi** | Ketiganya dipetakan pada `ToTimelineResponse` dan `ToResponse`. Uji `LiniMasaPerawatan_MembawaNilaiVerifikasiSamaPersisDenganBarisnya` membandingkan nilai tersimpan dengan nilai yang dibalas, termasuk waktu yang ditulis eksplisit `2026-09-08 07:30 UTC` dan batas `2026-09-08 12:00 UTC`. `Timeline_IkutMembawaKeadaanVerifikasiBesertaNamanya` menutup `GET /timeline` |
| 2. Balasan memuat **nama** verifikator, bukan hanya nomor pengguna; penulis dan verifikator pada dua kolom berbeda | **Terpenuhi** | `verifiedByUserName` diisi dari relasi pengguna verifikator. Uji `Verify_MembalasNamaVerifikatorDanNamaPenulisPadaDuaKolomBerbeda` memeriksa nama penulis **dan** nama verifikator terbaca berbeda pada dua kolom berbeda. **Batas yang dinyatakan:** butir Verification kartu task meminta uji "nama tetap terbaca dari snapshot"; kolom snapshot nama verifikator **tidak ada** pada tabel — lihat bagian 7 |
| 3. Memverifikasi sebuah catatan **tidak mengubah satu pun** kolom penulis, baik pada balasan maupun pada barisnya | **Terpenuhi** | Uji `Verify_TidakMengubahSatuPunKolomPenulisSebelumDanSesudah` membandingkan enam kolom penulis sebelum dan sesudah verifikasi, lalu memeriksa balasannya juga membawa penulis yang sama. Source verifikasi `CpptVerificationService.VerifyAsync` memang hanya menyentuh kolom verifikasi |
| 4. Catatan yang belum diverifikasi mengembalikan waktu verifikasi dan verifikator **kosong** — bukan `0001-01-01`, bukan teks kosong yang terbaca sebagai nama orang | **Terpenuhi** | `VerifiedAt` dan `VerifiedByUserId` bertipe boleh-kosong dan dipetakan apa adanya; `NamaVerifikator` mengembalikan `null` untuk teks kosong maupun spasi. Uji `BelumDiverifikasi_MengembalikanVerifikatorDanWaktunyaKosong` |
| 5. Catatan yang kebijakan verifikasinya tidak aktif mengembalikan `VerificationStatus` bernilai `NotRequired` | **Terpenuhi** | Nilai dikembalikan apa adanya dari barisnya, dan bawaan kolomnya memang `NotRequired`. Uji `KebijakanTidakAktif_MengembalikanNotRequiredBukanDiverifikasi` memeriksa nilainya `NotRequired` **dan** bukan `Verified` |
| 6. Catatan poliklinik, medical check-up, dan IGD tetap mengembalikan seluruh kolom lamanya persis seperti sebelumnya — `RWI-AC-143` | **Terpenuhi** | Uji `CatatanDiLuarRawatInap_KolomLamanyaTidakBerubahSatuPun` memeriksa sebelas kolom lama satu per satu pada catatan tanpa penanda perawatan. Seluruh perubahan bersifat aditif; nol kolom lama disentuh |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terpetakan ke source yang benar-benar ada | Terpenuhi |
| `dotnet build` dijalankan dan hasilnya dicatat apa adanya | Terpenuhi — 0 error `CS`; dua error penyalinan berkas dicatat apa adanya sebagai `EXISTING / ENVIRONMENT ISSUE` beserta penyebabnya |
| `dotnet test` dijalankan dan hasilnya dicatat apa adanya | Terpenuhi — `Failed: 0, Passed: 470, Total: 470`. Run pertama yang gagal 8 **tidak dihapus** dari laporan ini; penyebabnya dijelaskan pada bagian 5 |
| Test unit baru menutup keenam kriteria | Terpenuhi — 9 uji baru |
| Test "nama verifikator terbaca dari snapshot saat relasi dikosongkan" | **Tidak dapat dijalankan** — kolom snapshot itu tidak ada. Diganti uji yang membuktikan namanya **kosong** dan tidak pernah jatuh ke nama penulis. Lihat bagian 7 |
| Test membandingkan seluruh kolom penulis sebelum dan sesudah verifikasi | Terpenuhi |
| Test regresi membaca catatan di luar rawat inap | Terpenuhi |
| Laporan tracked ada di `../task/report/backend/BE-RWI-066.md` | Terpenuhi — berkas ini |
| Register bagian 4.1 dan `requirement-traceability.md` diperbarui | Terpenuhi |
| Nol kolom sensitif masuk logger | Terpenuhi — pemanggilan logger pada `VerifyProgressNote` **tidak disentuh** task ini dan tetap hanya membawa id, waktu, dan nomor pengguna; nol isi catatan |
| Nol migration dibuat | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan peringatan `CS1573`/`CS1574`/`CS1587` pada berkas yang **tidak** disentuh task ini. Peringatan itu sudah ada sebelumnya dan tidak diperbaiki tanpa wewenang |
| **Masalah yang diketahui — butir Verification yang tidak dapat dijalankan** | Kartu task meminta "satu test yang mengosongkan relasi pengguna verifikator lalu membuktikan namanya tetap terbaca **dari snapshot**". Kolom snapshot nama verifikator **tidak ada** pada `TrxPatientIntegratedProgressNote`; yang ada hanya `ProviderDisplayNameSnapshot` milik **penulis**. Membuat kolom snapshot verifikator berarti kolom tabel baru beserta migration-nya, dan baris Scope serta DoD kartu yang sama menuliskan **nol kolom tabel baru, nol migration**. Kedua ketentuan itu tidak dapat dipenuhi bersamaan, jadi yang dijalankan adalah ketentuan yang lebih membatasi, dan selisihnya dilaporkan di sini alih-alih ditambal diam-diam. Uji penggantinya membuktikan hal yang justru paling berbahaya bila salah: nama verifikator yang tidak dapat dikenali **kosong**, dan **tidak pernah** jatuh ke nama penulis |
| **Risiko tersisa — nama verifikator ikut berubah bila akunnya berganti nama** | Karena tidak ada snapshot, nama verifikator dibaca dari akun **saat ini**. Bila dr. Andi berganti nama menjadi dr. Andi Pratama, verifikasi yang ia lakukan bulan lalu akan tampil atas nama barunya. Untuk **penulis** catatan, hal ini sudah dijaga snapshot sejak awal; untuk **verifikator**, belum. Perbaikannya adalah kolom snapshot baru beserta migration, dan itu task tersendiri yang belum ada. Dampaknya terbatas: nomor pengguna verifikator tetap benar dan tidak pernah berubah |
| Risiko tersisa — snapshot model EF | `Migrations/ApplicationDbContextModelSnapshot.cs` hanya ditulis ulang ketika migration baru dibuat. Karena task ini **tidak** membuat migration, berkas itu masih mencatat relasi verifikator tanpa nama navigation. Selisihnya murni model CLR dan **nol pengaruh terhadap schema**; migration berikutnya yang dibuat siapa pun akan menyerapnya tanpa menghasilkan perintah `ALTER` apa pun |
| **Delta kontrak yang dicatat** | `contracts/api-contract.md` baris 111–112 masih menandai `PATCH /{id}/verify` dan `GET /episodes/{episodeId}/verification-status` sebagai **"Rencana (belum tersedia)"**, padahal keduanya sudah tersedia di source sejak `BE-RWI-053`. Ini selisih dokumen, bukan cacat source, dan **tidak disunting dari sini** karena kontrak bukan target tulis task ini |
| Perubahan sampingan | Penulisan berkas sempat menambahkan penanda urutan byte (BOM) pada empat berkas source dan satu berkas uji yang aslinya tidak memilikinya. **Dipulihkan seluruhnya** dan diperiksa ulang dengan membandingkan tiga byte pertama setiap berkas terhadap `HEAD`. Direktori keluaran build sementara `artifacts/tb/` **sudah dihapus**; `artifacts/` memang sudah tercantum pada `.gitignore` |
| Interupsi | Satu kejadian. Build pertama berjalan lebih dari 10 menit lalu dipindahkan ke latar, dan sesi berikutnya tidak menemukan catatan penyelesaiannya. Keluaran parsialnya diperiksa, ternyata kompilasinya **sudah berhasil** dan yang gagal hanya penyalinan berkas terkunci. Pekerjaan dilanjutkan dari keadaan itu tanpa mengulang penyuntingan |
| Status Git | `M` pada 4 berkas source dan 1 berkas infrastruktur uji, `??` pada 1 berkas uji baru. **Nol tindakan Git dijalankan agent** — tanpa `git add`, commit, push, merge, maupun rebase |
| Langkah berikutnya | `BE-RWI-067` untuk nama penulis pada daftar pantau, lalu menjalankan ulang builder frontend pada `FE-RWI-046` supaya kriteria 2 dan gerbang §4.1 butir 10 dapat ditutup penuh |
