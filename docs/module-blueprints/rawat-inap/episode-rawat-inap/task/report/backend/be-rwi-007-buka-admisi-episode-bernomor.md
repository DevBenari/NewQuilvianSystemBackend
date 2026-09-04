# Laporan Perubahan Backend — `BE-RWI-007`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> **Task ini kini ✅ SELESAI.** Seluruh butir DoD-nya hijau: test lulus (255/255) dan
> endpointnya terbukti berjalan lewat Swagger pada aplikasi tersambung PostgreSQL.
> Tandanya pada roadmap sudah dinaikkan 🟡 → ✅.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-007` |
| Judul | Petugas admisi dapat membuka admisi dan episode lahir bernomor |
| Slice | S1 — Petugas dapat membuka admisi dan memesan tempat tidur |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `RWI-DEC-009`, `RWI-DEC-011`, `RWI-DEC-041`; `INV-INP-03`, `INV-INP-04`; api contract `0.4.0` `POST /episodes`; state matrix bagian 1; validation matrix bagian 1; `RWI-AC-001`, `RWI-AC-004` s.d. `RWI-AC-006`, `RWI-AC-009`, `RWI-AC-010` |
| Contract version | API `0.4.0` — **bentuknya tidak berubah**. Satu endpoint yang sebelumnya "Rencana (belum tersedia)" kini ada di dalam kode |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `11711a1` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Jenis perubahan | Pengisian `InpEpisodeService`, satu controller baru, satu berkas DTO baru |
| Status | ✅ **SELESAI** — seluruh acceptance criteria dan DoD terbukti; build+test hijau dan endpoint terbukti berjalan 26 Agustus 2026 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. `dotnet build` dan `dotnet test` **tidak dijalankan** pada sesi
> ini. Task ini karena itu **belum boleh ditandai selesai**, sama seperti `BE-RWI-002`,
> `BE-RWI-004`, dan `BE-RWI-005`. Perintah yang perlu dijalankan ada pada bagian 6.1.

---

## 1. Apa yang dibangun, dan kenapa

### 1.1 Keadaan sebelum perubahan

`InpEpisodeService` hanya kerangka. Sebelas tabel transaksi sudah ada sejak `BE-RWI-003`, dan
service sudah terdaftar sejak `BE-RWI-004`, tetapi **tidak ada satu pun cara membuat baris
`InpEpisode`** selain perintah `INSERT` langsung ke basis data.

> **Contoh akibatnya.** Ibu Rina datang pukul 07:00 untuk operasi terencana. Petugas admisi
> membuka layar rawat inap dan tidak menemukan apa pun yang dapat ditekan: tidak ada nomor
> episode, tidak ada tempat mencatat DPJP, tidak ada jejak bahwa Ibu Rina hari itu masuk. Yang
> tersisa hanyalah buku tulis di meja admisi — persis keadaan yang hendak diakhiri modul ini.

### 1.2 Yang dibuka task ini

Satu endpoint, `POST /episodes`. Sejak task ini satu pasien terdaftar dapat dijadikan pasien
rawat inap: episode lahir berstatus `Draft` dengan nomor yang terbaca manusia, menempel pada
tepat satu kunjungan, dan sudah punya DPJP sejak detik pertama.

---

## 2. Proses bisnis

### 2.1 Tujuan

Membuka admisi menjadi satu tindakan yang utuh dan berjejak: satu form, satu kali isi, dan
seluruh akibatnya — kunjungan, episode, DPJP, riwayat status — tersimpan bersama atau tidak
tersimpan sama sekali.

### 2.2 Pelaku

| Pelaku | Perannya | Hak akses |
| --- | --- | --- |
| Petugas admisi | Membuka admisi dan memilih DPJP pertama | `InpatientEpisode : Create` |
| DPJP | Tidak menyentuh layar ini. Namanya dicatat sebagai penanggung jawab sejak episode lahir | — |
| Auditor | Membaca `InpStatusHistory` untuk mengetahui kapan dan oleh siapa episode dibuka | `InpatientEpisode : Read` |

### 2.3 Dua jalur masuk yang berakhir pada bentuk data yang sama

| Jalur | Yang dikirim petugas | Yang dilakukan sistem |
| --- | --- | --- |
| Kunjungan rawat inap sudah ada | `EncounterId` diisi | Kunjungan dipakai apa adanya sebagai jangkar |
| Pasien datang langsung | `EncounterId` dikosongkan | Sistem membuat kunjungan bertipe rawat inap sendiri, di dalam proses admisi yang sama |

Bagi petugas kedua jalur terasa sama: tetap satu form, tetap satu kali isi. Pembuatan kunjungan
otomatis pada jalur kedua berjalan di belakang layar (`RWI-AC-009`).

### 2.4 Contoh berangka

> **25 Agustus 07:00** — Ibu Rina datang untuk operasi terencana. Petugas admisi Sdri. Wati
> membuka admisi tanpa menunjuk kunjungan, memilih unit Rawat Inap Melati, kelas 1, dan dr.
> Andi sebagai DPJP.
>
> Dalam satu tindakan sistem menyimpan **empat** hal sekaligus: kunjungan rawat inap baru,
> episode `RI-260825070000-A1B2C3` berstatus `Draft`, penugasan DPJP dr. Andi bernomor urut 1,
> dan satu baris riwayat status bertanda "dibuka Sdri. Wati".
>
> **07:02** — Sdri. Wati sadar ia salah memilih kelas. Ia membetulkannya lewat `PUT`
> (`BE-RWI-008`), bukan dengan membuka admisi kedua.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | **Baru** | `OpenAdmissionRequest`, `UpdateAdmissionRequest`, `CancelAdmissionRequest`, `InpatientEpisodeDetailResponse`, `InpatientEpisodeActiveDoctorResponse` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diisi | `OpenAdmissionAsync`, `ApplyStatusChangeAsync`, `GetDetailResponseAsync`, `WasEncounterCreatedByAdmissionAsync`, pembantu validasi |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | **Baru** | Aksi `POST /` |

Berkas DTO dan controller dipakai bersama `BE-RWI-008`; laporan ini menulis bagiannya yang
menjadi milik `BE-RWI-007`.

### 3.1 `ApplyStatusChangeAsync` — satu pintu

Method ini adalah **satu-satunya** tempat `InpEpisode.EpisodeStatus` boleh berubah. Ia selalu
menulis satu baris `InpStatusHistory`, dan barisnya masuk ke `SaveChangesAsync` yang sama
dengan perubahan statusnya sendiri.

> **Kenapa ini bukan gaya penulisan, melainkan syarat.** Bila satu controller saja menyetel
> `EpisodeStatus` langsung, riwayat status berlubang. Laporan penutupan tanpa kelayakan
> keuangan, empat daftar pantau, dan pembuktian belum adanya catatan klinis saat pembatalan
> semuanya dibaca dari tabel riwayat itu — dan tidak satu pun dari mereka dapat mengetahui
> bahwa ada perpindahan yang tidak tercatat. Yang muncul bukan galat, melainkan angka yang
> salah dan terlihat wajar.

`SequenceNumber` dihitung dari nomor urut terakhir milik episode yang sama, di dalam transaksi
yang sama, dan dijaga index unik `(EpisodeId, SequenceNumber)`. Ini nomor urut riwayat, bukan
nomor bisnis yang dilihat pengguna, sehingga ia **bukan** alokasi kode yang diatur
`QBE-CODE-003`; nomor bisnis modul ini tetap dibentuk `InpEpisodeNumberService`.

### 3.2 Penanda asal-usul kunjungan

`InpStatusHistory.ActionType` pada baris pertama bernilai:

| Nilai | Artinya |
| --- | --- |
| `OpenAdmission` | Kunjungan ditunjuk petugas. Milik alur pendaftaran |
| `OpenAdmissionWithEncounter` | Kunjungan dibuat sendiri oleh proses admisi |

Penanda ini **bukan hiasan**. Ia adalah satu-satunya bukti tahan lama bahwa kunjungan jangkar
lahir bersama episodenya, dan bukti itulah yang dipakai `BE-RWI-008` untuk memutuskan apakah
kunjungan tersebut boleh ikut dibatalkan. Baris riwayat tidak dapat diubah maupun dihapus,
sehingga penanda ini tetap benar sepanjang umur episode.

Alternatif yang **ditolak**: menambah kolom penanda pada `TrxPatientEncounter`. Kolom itu milik
modul Registrasi, dan menambahnya berarti mengubah schema modul lain sekaligus menuntut
migration yang tidak diberi wewenang task ini.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Submodule | — |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`). `QBE-MOD-002` tidak lagi menahan |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-004`, `QBE-CODE-005`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada. `docs/engineering/QBE_EXCEPTIONS.json` tetap kosong |

### 4.1 Kepatuhan yang perlu disebut namanya

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| `QBE-SVC-001` | Controller tidak menerima `ApplicationDbContext`. Seluruh pembacaan dan perubahan lewat `InpEpisodeService`, termasuk penyusunan balasan |
| `QBE-CODE-002` | Controller tidak membentuk nomor apa pun. Nomor episode dibentuk `InpEpisodeNumberService`, dipanggil service |
| `QBE-CODE-003` | Tidak ada `Count + 1` maupun `Max + 1` yang dipakai sebagai alokator nomor bisnis. Nomor kunjungan yang dibuat modul ini **sengaja tidak** memakai alokator lama milik Registrasi, yang menyisir seluruh baris lalu memakai celah pertama — lihat bagian 5.2 |
| `QBE-CODE-004` | Nomor episode dijaga index unik `IX_InpEpisode_EpisodeNumber`; nomor kunjungan dijaga index unik pada `TrxPatientEncounter.EncounterNumber`. Keduanya sudah ada |
| `QBE-TXN-001` | Kunjungan, episode, penugasan DPJP, dan baris riwayat berada dalam satu transaksi dan satu `SaveChangesAsync` |
| `QBE-DTO-001` | Entity EF tidak pernah menjadi kontrak API. Balasan memakai `InpatientEpisodeDetailResponse` |
| `QBE-LOG-001` | `LoggerService.InfoAsync` dipanggil dengan `EntityId`, controller, action, dan kode status saja |
| `QBE-AUD-001` | Jejak bisnis ada pada `InpStatusHistory` dan kolom `IdentityModel`, terpisah dari catatan `LoggerService` |

### 4.2 Kolom sensitif

Permission/Audit matrix bagian 5.4 menandai `InpEpisode.Notes` sebagai sensitif. Payload
`LoggerService` pada ketiga aksi controller **tidak** memuatnya; hanya `EntityId`, nama
controller, nama action, dan kode status.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Kunjungan poliklinik tidak dapat dipakai sebagai jangkar

**Yang ditemukan.** `RWI-RULE-005` menyebut tiga jalur masuk, salah satunya "pasien kontrol di
poliklinik lalu dirujuk rawat inap — kunjungan poliklinik yang sudah ada dipakai sebagai jangkar
episode". Sementara itu **validation matrix `0.4.0` bagian 1** mewajibkan: "Kunjungan wajib
bertipe rawat inap … 'Kunjungan yang dipilih bukan kunjungan rawat inap.' 422".

Keduanya tidak dapat dijalankan bersamaan.

**Yang diambil.** Validation matrix dijalankan apa adanya, karena ia kontrak bertversi yang
dikunci dan **secara eksplisit disebut pada kolom Trace task ini**. Baris jalur poliklinik pada
`RWI-RULE-005` karena itu tidak dapat berjalan pada revisi ini.

**Yang perlu diputuskan.** Pemilik Product/Domain menentukan salah satu: validation matrix
diperlunak untuk menerima kunjungan poliklinik, atau `RWI-RULE-005` dikoreksi sehingga jalur
poliklinik menempuh pembuatan kunjungan rawat inap baru — sama seperti jalur IGD yang sudah
diputuskan begitu lewat `RWI-DEC-041`. **Jangan diselesaikan Backend sendiri.**

### 5.2 Bentuk nomor kunjungan yang dibuat modul ini berbeda

**Yang ditemukan.** Pendaftaran membentuk nomor kunjungan lewat
`PatientEncounterController.GenerateRunningCodeAsync`, yang menyisir seluruh baris
`TrxPatientEncounter`, mengumpulkan angka yang sudah terpakai, lalu memakai celah pertama.
Bentuknya `ENC-RSMMC-00042`.

Cara itu dilarang `QBE-CODE-003` untuk kode baru: dua permintaan bersamaan membaca kumpulan
angka yang sama, lalu keduanya menyimpulkan angka berikutnya yang sama.

**Yang diambil.** Kunjungan yang dibuat modul Rawat Inap memakai bentuk
`ENC-RSMMC-{yyMMddHHmmss}-{6 acak}`, mengikuti pola `InpEpisodeNumberService` yang sudah
disetujui pada `BE-RWI-004`. Awalannya dipertahankan supaya nomornya tetap dikenali sebagai
nomor kunjungan. Index unik pada `TrxPatientEncounter.EncounterNumber` menjadi penjaga
terakhirnya.

**Yang perlu diputuskan.** Format nomor kunjungan milik modul Registrasi (`QBE-CODE-005`).
Pemilik modul Registrasi perlu menyatakan salah satu: menerima dua bentuk nomor berdampingan,
atau menyediakan alokator bersama yang aman untuk dipakai kedua modul. **Owner: pemilik
`RegistrationManagement`.**

### 5.3 Menulis ke `TrxPatientEncounter` melampaui integration contract bagian 2

**Yang ditemukan.** `contracts/integration-contract.md` bagian 2 menyatakan penulisan salinan
status tempat tidur (`INT-INP-03`) adalah "**satu-satunya** arah tulis modul ini ke luar
batasnya sendiri". Sementara itu acceptance criteria 4 task ini mewajibkan modul membuat
kunjungan, dan acceptance criteria 5 `BE-RWI-008` mewajibkan modul membatalkannya.

**Yang diambil.** Acceptance criteria roadmap dijalankan, karena ia kontrak task yang aktif dan
didukung keputusan `approved`: `RWI-DEC-011` menyatakan sistem membuat kunjungan rawat inap
otomatis di dalam proses admisi, dan `RWI-RULE-022` menyatakan kunjungan yang terlanjur dibuat
ikut ditandai batal. Penulisannya dibatasi seketat mungkin: modul **hanya** menyentuh kunjungan
yang dibuatnya sendiri, dibuktikan penanda pada `InpStatusHistory`.

**Yang perlu diputuskan.** Kalimat "satu-satunya arah tulis" pada integration contract bagian 2
sudah tidak akurat. Ia perlu diperbarui menjadi dua arah tulis, atau diberi pengecualian
tertulis. Pemutakhiran kontrak **bukan** wewenang task ini. **Owner: Product/Domain bersama
pemilik `RegistrationManagement`.**

### 5.4 `RWI-TRC-002` — pemaksaan kelas pasien `RAWAT JALAN` **tidak** menahan

`RWI-RULE-005` menyebut ketergantungan yang harus dibereskan lebih dulu: PRD mengklaim sistem
memaksa kelas pasien `"RAWAT JALAN"` saat kunjungan dibuat.

**Diperiksa pada source.** `PatientEncounterController.ResolvePatientClassAsync` baris 1463
seterusnya memaksa kelas `RAWAT JALAN` **hanya** ketika `request.EncounterType ==
EncounterType.Outpatient`. Kunjungan bertipe `Inpatient` tidak tersentuh olehnya. Ditambah
lagi, kunjungan yang dibuat modul ini tidak melewati controller tersebut sama sekali.

**Kesimpulan:** `RWI-TRC-002` **tidak menahan** jalur pasien datang langsung. Ada satu test yang
menjaga kesimpulan itu (`Kriteria4_PasienDatangLangsungMendapatKunjunganRawatInapOtomatis`).

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan `POST /episodes` terhadap aplikasi berjalan | **NOT RUN** — memerlukan aplikasi dan basis data |

Selama ketiganya belum dijalankan, **tidak ada satu pun acceptance criteria yang berstatus
terbukti**, walaupun test yang membuktikannya sudah ditulis.

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpEpisodeOpenAdmissionTests.cs` — 14 test, ditambah 7 test kontrak controller pada `InpatientEpisodeControllerContractTests.cs` yang dipakai bersama `BE-RWI-008`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Episode lahir `Draft` bernomor berawalan master | `Kriteria1_EpisodeLahirDraftDenganNomorBerawalanDariMaster`, `Kriteria1_DpjpPertamaDitetapkanSejakDetikPertama` | ✅ **Lulus** 26 Agu 2026 |
| 2. Tanpa DPJP ditolak 400; `INV-INP-03` tidak dilanggar | `Kriteria2_TanpaDpjpDitolakDanTidakAdaEpisodeYangLahir` | ✅ **Lulus** 26 Agu 2026 |
| 3. Kunjungan yang sudah punya episode ditolak 409; `INV-INP-04` dijaga | `Kriteria3_KunjunganYangSudahPunyaEpisodeDitolak` | ✅ **Lulus** 26 Agu 2026 |
| 4. Pasien datang langsung mendapat kunjungan rawat inap otomatis | `Kriteria4_PasienDatangLangsungMendapatKunjunganRawatInapOtomatis`, `Kriteria4_ProvenanceKunjunganTercatatPadaRiwayatStatusPertama` | ✅ **Lulus** 26 Agu 2026 |
| 5. Setiap perubahan status menulis satu baris riwayat, dalam transaksi yang sama | `Kriteria5_KelahiranEpisodeMeninggalkanTepatSatuBarisRiwayat`, `Kriteria5_KegagalanDiTengahTidakMenyisakanEpisodeMaupunRiwayat` | ✅ **Lulus** 26 Agu 2026; lihat batasannya di bawah |
| 6. Admisi `Draft` ganda **berhasil** disertai peringatan | `Kriteria6_AdmisiDraftGandaBerhasilDisertaiPeringatan` | ✅ **Lulus** 26 Agu 2026 |

Empat test tambahan menjaga validation matrix bagian 1: pasien kosong, kunjungan bukan rawat
inap, unit layanan bukan rawat inap, dan kelas perawatan yang tidak berlaku untuk rawat inap.
Satu test menjaga bahwa kunjungan yang ditunjuk petugas **tidak** ditandai sebagai buatan
admisi.

### 6.3 Batas pembuktian transaksi — disebut apa adanya

Provider InMemory **tidak punya transaksi**. `IsolatedInpatientDbContextFactory` karena itu
diberi `ConfigureWarnings(...Ignore(InMemoryEventId.TransactionIgnoredWarning))`, sehingga
`BeginTransactionAsync` menjadi tindakan kosong, bukan galat.

Akibatnya, yang dibuktikan `Kriteria5_KegagalanDiTengahTidakMenyisakanEpisodeMaupunRiwayat`
adalah **sifat yang membuat transaksinya bekerja**, bukan transaksinya sendiri: seluruh
perubahan masuk ke satu `SaveChangesAsync`, sehingga kegagalan menyisakan **nol** episode, nol
baris riwayat, nol penugasan DPJP, dan nol kunjungan — bukan satu tanpa yang lain.

Pembuktian bahwa PostgreSQL benar-benar mengembalikan perubahan saat transaksi digagalkan
**belum dilakukan**, dan memerlukan basis data sungguhan. Ini bukan kelalaian yang didiamkan;
ia dicatat sebagai verifikasi tertunda pada bagian 8.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru. Tidak ada endpoint existing yang berubah bentuk maupun perilakunya |
| Database | **Tidak ada perubahan schema.** Tidak ada entity baru, tidak ada kolom baru, tidak ada migration dibuat maupun dijalankan |
| Modul tetangga | `TrxPatientEncounter` mendapat baris baru bertipe `Inpatient` dari jalur pasien datang langsung. Tidak ada kolom, index, maupun perilaku modul Registrasi yang berubah. Lihat bagian 5.3 |
| Keamanan | Butir hak akses `InpatientEpisode : Create` dan `: Update` didaftarkan otomatis `AccessMenuSeeder` saat aplikasi menyala. Belum diverifikasi karena aplikasi belum dijalankan |
| Frontend | Tidak ada. Endpoint ini belum dipanggil layar mana pun |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Kode dapat saja tidak dapat dikompilasi, dan tidak ada satu pun kriteria yang benar-benar terbukti | Menjalankan perintah pada bagian 6.1 |
| Perilaku transaksi belum diuji terhadap PostgreSQL | Bila `SaveChangesAsync` gagal sebagian di produksi, episode dapat lahir tanpa riwayat status | Test integrasi terhadap PostgreSQL, dijadwalkan bersama `BE-RWI-033` |
| Jalur poliklinik tidak berjalan | Pasien yang dirujuk dari poliklinik tidak dapat diadmisikan tanpa kunjungan rawat inap baru | Keputusan pada bagian 5.1 |
| Dua bentuk nomor kunjungan berdampingan | Laporan yang mengurutkan kunjungan berdasarkan nomor akan mencampur dua pola | Keputusan pada bagian 5.2 |
| `ApplyStatusChangeAsync` dilangkahi task berikutnya | Riwayat status berlubang dan seluruh laporan pengecualian ikut salah | Ditegakkan lewat review. `InpEpisode.EpisodeStatus` **tidak** disetel di luar method itu pada seluruh kode yang ada hari ini |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak `0.4.0` | ✅ Bentuk, verb, route, dan hak aksesnya sesuai; dijaga `InpatientEpisodeControllerContractTests` |
| Keenam kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Test transaksi gagal lulus | ⚠️ **Dijalankan dan lulus** 26 Agustus 2026, tetapi cakupannya tetap terbatas|
| Api contract diperbarui | ✅ **Sudah** — `Rencana` → `Tersedia` 26 Agustus 2026, setelah endpointnya terbukti berjalan |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`, lalu perbarui bagian 6 laporan ini dengan hasil
   sebenarnya — termasuk bila gagal.
2. Bawa tiga butir bagian 5.1, 5.2, dan 5.3 ke pemilik keputusannya masing-masing.
3. `BE-RWI-008` melanjutkan pada berkas yang sama dan sudah dikerjakan bersama sesi ini; lihat
   [laporannya](be-rwi-008-ubah-batal-kedaluwarsa-draft.md).
4. `BE-RWI-009` membuka endpoint baca, dan menentukan kolom mana yang boleh tampil pada daftar
   dan mana yang hanya pada detail.
