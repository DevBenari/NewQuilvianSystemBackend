# Laporan Perubahan Backend — `BE-RWI-011`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> Yang **belum** berubah: acceptance criteria dan DoD task ini tetap belum terbukti penuh —
> build hijau bukan tanda selesai — sehingga tandanya pada roadmap tetap 🟡.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-011` |
| Judul | Pasien punya lokasi, dan tempat tidur ganda mustahil terjadi |
| Slice | S2 — Pasien punya lokasi, dan penempatan yang tidak layak ditolak |
| Trace | `RWI-DEC-021`, `RWI-DEC-039`, `RWI-DEC-072`; `RWI-RULE-015`, `RWI-RULE-027`, `RWI-RULE-029` aturan 8; `INV-INP-01`, `INV-INP-02`; api contract `POST /placements`; `RWI-AC-059`, `RWI-AC-062`, `RWI-AC-147` |
| Contract version | API `0.4.0` — bentuk tidak berubah |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Jenis perubahan | `InpBedOccupancyService.PlacePatientAsync`; pembalikan arah dependency antar service; penulisan salinan `MstBed.BedStatus` |
| Dependency | `BE-RWI-010` |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN, DAN SATU GERBANG MASIH TERBUKA.** Lihat bagian 5.3 dan 6 |

> **Task ini tidak boleh ditandai selesai** sebelum tiga hal: build dan test dijalankan, test
> tabrakan dua transaksi dijalankan terhadap PostgreSQL, dan gerbang "belum ada catatan klinis"
> pada roadmap bagian 5 ditutup.

---

## 1. Apa yang dibangun

`POST /bed-occupancies/placements`. Satu tindakan yang mengubah lima hal **di dalam satu
transaksi**:

1. baris `InpBedPlacement` baru dibuka;
2. pemesanan milik episode itu — bila ada — menjadi `Consumed`;
3. salinan `MstBed.BedStatus` menjadi `Occupied`;
4. `InpEpisode.EpisodeStatus` menjadi `Admitted` lewat `ApplyStatusChangeAsync`;
5. satu baris `InpStatusHistory` lahir.

Bila salah satu gagal — termasuk penulisan salinan status — **tidak ada satu pun yang
tersimpan**, dan episode tetap `Draft` dengan seluruh isian admisinya utuh.

---

## 2. Tiga lapis penjagaan `INV-INP-02`

Pemeriksaan "tempat tidur kosong" di dalam kode **tidak cukup**. Dua transaksi dapat sama-sama
lolos pemeriksaan sebelum salah satunya menyimpan.

| Lapis | Isinya | Diuji di mana |
| --- | --- | --- |
| 1. Penguncian baris | `SELECT 1 FROM public."MstBed" WHERE "Id" = {0} FOR UPDATE` di dalam transaksi | **Belum** — hanya PostgreSQL |
| 2. Pemeriksaan ulang di dalam transaksi | Setelah baris terkunci, keberadaan penempatan aktif milik episode lain diperiksa **ulang** | Test InMemory |
| 3. Unique index parsial | `IX_InpBedPlacement_BedId_Active` menolak baris kedua | **Belum** — hanya PostgreSQL |

> **Kejadian yang dicegah.** Pukul 09:00:01 Sdri. Wati menempatkan Tn. Budi ke
> `BD-RSMMC-00042`. Pada detik yang sama Sdri. Rina menempatkan Ny. Sari ke tempat tidur yang
> sama. Permintaan kedua menunggu di lapis 1, lalu membaca keadaan yang **sudah berubah** dan
> ditolak 409. Tidak ada satu baris penempatan ganda yang tersimpan.

**Lapis 1 dan 3 belum terbukti pada sesi ini.** Keduanya memerlukan PostgreSQL sungguhan; test
InMemory tidak menegakkan unique index dan tidak punya transaksi. Ini pembatasan pembuktian
yang harus dibaca sebelum mempercayai hasil test.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Diisi | `PlacePatientAsync`, `GetPlacementsByEpisodeAsync`, `GetPlacementAsync`, `ResolveBilledPatientClassId` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diubah | Konstruktor tidak lagi menerima `InpBedOccupancyService`; `ApplyStatusChangeAsync` menjadi `public`; `ReleaseBedHoldsAsync` kini mengembalikan salinan `MstBed.BedStatus`; `RestoreBedStatusCopyAsync` baru |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | Ditambah | Aksi `POST /placements` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientEpisodeTestWorld.cs` | Diubah | Menyesuaikan konstruktor service; pembantu kamar, tempat tidur, pasien, dokter, dan pegawai |

### 3.1 Arah dependency antar service **dibalik** — delta terhadap blueprint

**Yang berubah.** Sampai `BE-RWI-008`, `InpEpisodeService` menerima `InpBedOccupancyService`
lewat konstruktor tanpa pernah memakainya. Sejak task ini arahnya menjadi sebaliknya:
`InpBedOccupancyService` menerima `InpEpisodeService`.

**Kenapa harus dibalik.** Roadmap `BE-RWI-011` bagian Scope mewajibkan
`InpBedOccupancyService.PlacePatientAsync` memanggil `InpEpisodeService.ApplyStatusChangeAsync`
— satu-satunya pintu perubahan status. Mempertahankan kedua arah sekaligus menghasilkan
dependency melingkar yang **ditolak container saat aplikasi dinyalakan**, bukan saat
dikompilasi. Aplikasinya tidak akan menyala sama sekali.

**Delta yang perlu diputuskan.** Class diagram pada `02-backend-architecture.md` bagian 3.4
menggambar panah `InpEpisodeService --> InpBedOccupancyService`. Panah itu kini terbalik.
Roadmap task lebih spesifik daripada diagram, dan roadmap yang diikuti — tetapi diagramnya
perlu dikoreksi supaya kedua dokumen tidak berselisih. **Owner: pemilik arsitektur backend.**

Pelepasan pemesanan dan penempatan saat pembatalan tetap berada di `InpEpisodeService`, tidak
dipindahkan ke `InpBedOccupancyService` seperti yang disarankan laporan `BE-RWI-008` bagian
5.2 — memindahkannya akan mengembalikan dependency melingkar itu.

### 3.2 Celah salinan `MstBed.BedStatus` ditutup

Laporan `BE-RWI-008` bagian 5.2 mencatat: pembatalan admisi melepas baris pemesanan dan
penempatan, tetapi **tidak** mengembalikan `MstBed.BedStatus`. Tempat tidur milik pasien yang
admisinya batal karena itu tetap terlihat terisi pada layar master.

Celah itu ditutup di sini, di dalam transaksi yang sama dengan pembatalannya.

> **Satu jebakan yang ditemukan saat menulisnya.** Baris pemesanan dan penempatan milik episode
> yang sedang dibatalkan sudah ditutup **di memori** tetapi belum disimpan, sehingga query ke
> database masih membacanya sebagai aktif. Bila ia ikut dihitung, tidak ada satu pun tempat
> tidur yang pernah kembali `Available`. Karena itu pemeriksaan "masih dipegang siapa pun"
> mengecualikan episode yang sedang melepas.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-TXN-001`** adalah aturan paling menentukan pada task ini. Kelima perubahan berada di
dalam satu transaksi, dan ada test yang membuktikan kegagalan penyimpanan tidak menyisakan
baris apa pun.

**`QBE-AUD-001`** — jejak database (`InpBedPlacement`, `InpStatusHistory`) terpisah dari
catatan aplikasi (`LoggerService`). Yang pertama adalah bukti tindakan bisnis; yang kedua
hanya penelusuran teknis.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Aturan 9 tidak dapat diperiksa — kolomnya belum ada

`RWI-DEC-072` menambahkan aturan 9 pada Kelayakan Penempatan: penempatan pasien asal IGD
menunggu event `Tiba` milik IGD. Aturan itu hanya berlaku bila
`TrxPatientEncounter.OriginEncounterId` terisi.

**Kolom itu belum ada pada source hari ini.** Ia dibuat modul IGD lewat `IGD-DEC-075`, sesuai
`RWI-DEC-073`. Karena itu aturan 9 **tidak diimplementasikan**, dan memang tidak perlu: jalur
serah terima IGD adalah `INP-S09` yang di luar scope revisi ini.

Yang dikerjakan dari `RWI-DEC-072` hanyalah kriteria 7 sebagai penjaga:
`InpBedPlacement.StartDateTime` untuk jalur datang langsung dan poliklinik adalah **waktu
penempatan dibuat**, tidak menunggu apa pun. Ada test yang memeriksanya.

### 5.2 Kelas yang ditagihkan diambil dari kamar

`InpBedPlacement.PatientClassId` diisi dari `MstRoom.PatientClassId` bila kamarnya punya kelas,
dan jatuh kembali ke `InpEpisode.PatientClassId` bila tidak. Dasarnya `RWI-DEC-013`.

Kolom kelas pada episode **tidak** ditimpa. Ia tetap merekam pilihan saat admisi dibuka,
sehingga jejak kelas awal tidak hilang ketika pasien pindah kelas di tengah perawatan.

### 5.3 Gerbang "belum ada catatan klinis" **masih terbuka**

Roadmap bagian 5 menyatakan `BE-RWI-011` **tidak boleh ditandai selesai** sebelum batas "belum
ada catatan klinis" pada pembatalan ditutup.

**Keadaannya hari ini: masih terbuka.** `RWI-RULE-004` mewajibkan pembatalan episode `Admitted`
ditolak bila sudah ada satu saja dari enam jenis catatan klinis. Keenamnya milik
`ClinicalManagement` dan `PharmacyManagement`, dan jalur bacanya belum ada pada
`contracts/integration-contract.md` maupun pada scope task ini.

**Akibatnya sejak task ini rilis: nyata.** Sebelum `BE-RWI-011`, episode tidak pernah dapat
mencapai `Admitted`, sehingga cabang itu tidak pernah terpakai. Sejak sekarang ia terpakai —
dan supervisor dapat membatalkan episode yang sudah punya pengkajian dan tanda vital.
**Owner: Backend/API bersama Product/Domain.**

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| **Test dua transaksi bersamaan terhadap PostgreSQL** | **NOT RUN** — inilah verifikasi terpenting task ini, dan ia tidak dapat dijalankan tanpa database sungguhan |
| Pemanggilan `POST /placements` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpBedPlacementTests.cs` — 12 test (sebagian
milik `BE-RWI-012`).

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Sistem menjawab siapa menempati dan sejak jam berapa | `Kriteria1_SetelahPenempatanSistemMenjawabSiapaMenempatiDanSejakJamBerapa` | ✅ **Lulus** 26 Agu 2026 |
| 2. Dua transaksi bersamaan: satu berhasil, satu 409, tepat satu baris aktif | `Kriteria2_PenempatanKeduaPadaTempatTidurYangSamaDitolak409DanHanyaSatuBarisAktifTersimpan` — **hanya lapis 2** | Ditulis; **lapis 1 dan 3 belum terbukti** |
| 3. Kegagalan penulisan salinan status tidak menyisakan penempatan | `Kriteria3_KegagalanPenyimpananTidakMenyisakanPenempatanDanEpisodeTetapDraft` | ✅ **Lulus** 26 Agu 2026 |
| 4. Keadaan tempat tidur diperiksa ulang saat penempatan | `Kriteria4_KeadaanTempatTidurDiperiksaUlangSaatPenempatanBukanHanyaSaatPemesanan` | ✅ **Lulus** 26 Agu 2026 |
| 5. Penolakan tidak menghapus isian admisi | `Kriteria5_PenolakanTidakMenghapusIsianAdmisiDanPesannyaMengatakannya` | ✅ **Lulus** 26 Agu 2026 |
| 6. Pemesanan milik episode ini dipakai, bukan ditolak | `Kriteria6_PemesananMilikEpisodeIniYangMasihBerlakuDipakaiBukanDitolak` | ✅ **Lulus** 26 Agu 2026 |
| 7. `RWI-AC-147` — waktu mulai adalah waktu penempatan dibuat | Bagian dari `Kriteria1_...` | ✅ **Lulus** 26 Agu 2026 |

Tiga test tambahan menjaga: pemesanan yang gugur tidak menghalangi penempatan, episode yang
sudah ditempatkan tidak dapat ditempatkan lagi, dan pembatalan mengembalikan salinan status
tempat tidur.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru |
| Database | **Tidak ada perubahan schema.** Tidak ada migration dibuat maupun dijalankan |
| Arsitektur | Arah dependency `InpEpisodeService` ↔ `InpBedOccupancyService` **dibalik** — bagian 3.1 |
| Modul tetangga | `MstBed.BedStatus` mulai ditulis menjadi `Occupied` dan dikembalikan menjadi `Available` |
| Perilaku existing | `CancelAdmissionAsync` kini juga mengembalikan salinan status tempat tidur — perbaikan celah `BE-RWI-008` bagian 5.2 |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Tidak ada kriteria yang benar-benar terbukti | Bagian 6.1 |
| **Test tabrakan belum dijalankan** | Pertahanan sesungguhnya terhadap tempat tidur ganda belum terbukti sama sekali | Test terhadap PostgreSQL |
| **Batas catatan klinis masih terbuka** | Supervisor dapat membatalkan episode yang sudah punya pengkajian dan tanda vital | Bagian 5.3 — **gerbang, bukan catatan tambahan** |
| Class diagram blueprint berselisih dengan source | Pembaca berikutnya menulis kode mengikuti diagram lalu aplikasinya tidak menyala | Koreksi `02-backend-architecture.md` bagian 3.4 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Ketujuh kriteria lulus | ❌ **Belum.** Kriteria 2 baru terbukti sebagian |
| Test tabrakan lulus | ❌ **Belum dijalankan** |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |
| Gerbang catatan klinis ditutup | ❌ **Masih terbuka** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan test tabrakan dua transaksi terhadap PostgreSQL — **ini yang paling penting**.
3. Bawa bagian 5.3 ke Product/Domain sebelum task ini ditandai selesai.
4. Koreksi class diagram `02-backend-architecture.md` bagian 3.4.
