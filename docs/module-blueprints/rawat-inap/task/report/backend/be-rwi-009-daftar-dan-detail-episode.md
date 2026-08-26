# Laporan Perubahan Backend — `BE-RWI-009`

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
| Task ID | `BE-RWI-009` |
| Judul | Daftar dan detail episode dapat dibaca dan disaring |
| Slice | S1 — Petugas dapat membuka admisi dan memesan tempat tidur |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | Api contract `0.4.0` `GET /episodes`, `/{id}`, `/summary`, `/filters/metadata`; permission matrix `InpatientEpisode : Read`; privasi pada `03-frontend-architecture.md` bagian 6 |
| Contract version | API `0.4.0` — **bentuknya tidak berubah**. Empat endpoint yang sebelumnya "Rencana (belum tersedia)" kini ada di dalam kode |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Jenis perubahan | Pengisian bagian baca `InpEpisodeService`; empat aksi baca pada `InpatientEpisodeController`; DTO daftar, ringkasan, dan metadata penyaring |
| Dependency | `BE-RWI-007` ✅ kode siap |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Lihat bagian 6 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. `dotnet build` dan `dotnet test` **tidak dijalankan** pada sesi
> ini. Task ini karena itu **belum boleh ditandai selesai**.

---

## 1. Apa yang dibangun, dan kenapa

Sampai `BE-RWI-008`, episode dapat dibuka, diubah, dan dibatalkan — tetapi **tidak dapat
dicari**. Petugas yang ingin melanjutkan admisi yang dibuatnya kemarin harus mengingat sendiri
nomor episodenya, karena tidak ada satu pun endpoint yang mengembalikan daftar.

Task ini membuka empat endpoint baca:

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /episodes/filters/metadata` | Pilihan penyaring beserta nilai bawaannya, supaya layar tidak menebak sendiri unit layanan mana yang berlaku untuk rawat inap |
| `GET /episodes/summary` | Jumlah episode per status, memakai penyaring yang sama dengan daftar |
| `GET /episodes` | Daftar bertingkat, dapat disaring unit layanan, kelas perawatan, status, rentang tanggal, kebutuhan isolasi, dan nama pasien |
| `GET /episodes/{id}` | Detail satu episode beserta DPJP aktif, perawat aktif, dan lokasi terkini |

---

## 2. Tiga batas yang mengikat, dan alasannya

### 2.1 Lokasi selalu dibaca dari catatan penempatan

Roadmap menyebutnya sebagai risiko utama task ini: *"Godaan menyimpan lokasi terakhir sebagai
kolom pada episode akan muncul di sini karena query-nya lebih murah."*

`InpatientEpisodeDetailResponse.CurrentLocation` dan
`InpatientEpisodeListItemResponse.CurrentBedName` keduanya dibaca dari `InpBedPlacement` yang
`EndDateTime`-nya masih kosong. **Tidak ada satu pun kolom lokasi pada `InpEpisode`**, dan
penambahannya dilarang arsitektur.

> **Kenapa larangan ini penting.** Kolom "lokasi terakhir" hanya benar sampai pertama kali ada
> perpindahan yang gagal di tengah jalan. Sejak saat itu, layar menampilkan kamar A sementara
> catatan penempatan menyimpan kamar B — dan tidak ada cara mengetahui mana yang benar.

Ada satu test yang menjaga batas ini: menutup baris penempatan membuat `CurrentLocation`
menjadi kosong, **tanpa satu pun kolom pada episode disentuh**.

### 2.2 Kolom sensitif hanya pada detail

Permission matrix bagian 5.4 menandai `InpEpisode.Notes` dan `InpEpisode.IsolationNote` sebagai
sensitif. Keduanya **tidak ada** pada `InpatientEpisodeListItemResponse`, dan ketiadaan itu
dijaga test yang membaca daftar properti bentuknya.

Yang boleh tampil pada daftar hanyalah nilai benar/salah `RequiresIsolation`. Perbedaannya
nyata: mengetahui seorang pasien membutuhkan isolasi adalah kebutuhan operasional ruangan;
mengetahui **alasan klinisnya** bukan.

### 2.3 Pembacaan menjalankan perhitungan kedaluwarsa lebih dulu

`GetEpisodeListAsync` dan `GetEpisodeSummaryAsync` memanggil `ExpireDueDraftEpisodesAsync`
sebelum membaca. Modul ini sengaja tidak memakai program penjadwal (`RWI-DEC-030`), sehingga
baris `Draft` basi tetap ada di tabel sampai seseorang membacanya.

> **Bila langkah ini dilewat.** Layar menampilkan admisi yang sesungguhnya sudah gugur. Petugas
> menekan "lanjutkan", lalu menerima penolakan yang tidak dapat dijelaskan oleh apa pun yang
> terlihat di layarnya.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs` | **Baru** | `GetEpisodeListAsync`, `GetEpisodeSummaryAsync`, `GetFilterMetadataAsync`, `GetEpisodeDetailAsync`, `ExpireDueDraftEpisodesAsync`, `BuildFilteredEpisodeQuery`, `NormalizePaging` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diubah | Class menjadi `partial`; `GetDetailResponseAsync` menambah perawat aktif, lokasi terkini, dan enam kolom isolasi |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeReadDtos.cs` | **Baru** | `InpatientEpisodeListQuery`, `InpatientEpisodeListItemResponse`, `InpatientEpisodePagedResult`, `InpatientEpisodeSummaryResponse`, `InpatientEpisodeFilterMetadataResponse`, `InpatientEpisodeCurrentLocationResponse`, `InpatientEpisodeActiveNurseResponse` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientSharedDtos.cs` | **Baru** | `InpatientOptionResponse`, `InpatientSortOptionResponse`, `PlacementEligibilityFailureResponse` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | Diubah | `InpatientEpisodeDetailResponse` menambah `IsolationNote`, `IsolationSource`, `IsolationSetBy*`, `DischargeType`, `ActiveNurse`, `CurrentLocation` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Diubah | Empat aksi baca |

### 3.1 Kenapa dipecah menjadi partial class

`InpEpisodeService.cs` sudah 1.100 baris sebelum task ini. Perilaku tulis dan perilaku baca
punya alasan berubah yang berbeda, dan menggabungkannya menghasilkan satu berkas yang tidak
lagi dapat dibaca utuh. Polanya mengikuti `WorkflowService.ActionsV2.cs` yang sudah ada di
repository ini — bukan pola baru.

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Submodule | — |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-ENT-003` |
| Pengecualian QBE | Tidak ada |

**`QBE-PAGE-001`** — daftar memakai bentuk `PagedResult<T>` yang sudah dipakai seluruh endpoint
daftar repository ini, beserta `pageNumber`, `pageSize`, `sortBy`, `sortDirection`, dan
`search`. `InpatientEpisodePagedResult` adalah turunannya, bukan bentuk baru.

**`QBE-ENT-003`** — tidak ada kolom persisten baru yang ditambahkan untuk keperluan tampilan.
Seluruh kolom baru pada DTO dihitung dari baris yang sudah ada.

**`QBE-SVC-001`** — controller tidak menyentuh `ApplicationDbContext`; seluruh pembacaan lewat
service pemiliknya. Dijaga test `ControllerTidakMenerimaApplicationDbContext`.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Rentang tanggal disaring terhadap waktu admisi dibuka

Roadmap menyebut "rentang tanggal" tanpa menetapkan kolomnya. Yang dipakai adalah
`InpEpisode.CreateDateTime`, yaitu waktu admisi dibuka — bukan `AdmittedAt`, karena episode
`Draft` belum punya `AdmittedAt` dan akan hilang seluruhnya dari penyaringan.

Bila Product/Domain menghendaki penyaringan terhadap waktu pasien ditempatkan, sebutkan
sebelum layar daftar dibangun.

### 5.2 `GET /{id}/status-history` sengaja tidak dibuat

Api contract memuatnya, tetapi roadmap menempatkannya pada `BE-RWI-028`. Ia tidak dibuat di
sini, dan ada test yang menahannya lahir lebih awal — bentuk riwayat status yang dikunci
sebelum ada yang memutuskannya akan sulit diubah kemudian.

### 5.3 Kriteria 5 belum terbukti

Acceptance criteria 5 — "tanpa hak akses, ditolak 403" — memerlukan `AccessPermissionFilter`
yang baru berjalan pada permintaan HTTP sungguhan beserta basis datanya. Yang dapat dijaga
tanpa aplikasi berjalan adalah bahwa setiap endpoint memang diberi `[AccessPermission]`, dan
itu dijaga `InpatientEpisodeControllerContractTests`.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan keempat endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpEpisodeReadTests.cs` — 7 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Daftar dapat disaring unit layanan, status, rentang tanggal, dan nama pasien | `Kriteria1_DaftarDapatDisaringUnitLayananStatusRentangTanggalDanNamaPasien` | ✅ **Lulus** 26 Agu 2026 |
| 2. Detail menampilkan DPJP aktif, perawat aktif, dan lokasi dari `InpBedPlacement` | `Kriteria2_DetailMenampilkanDpjpAktifPerawatAktifDanLokasiDariCatatanPenempatan` | ✅ **Lulus** 26 Agu 2026 |
| 3. Ringkasan menghitung jumlah per status | `Kriteria3_RingkasanMenghitungJumlahPerStatus` | ✅ **Lulus** 26 Agu 2026 |
| 4. Kolom sensitif tidak ikut pada daftar, hanya pada detail | `Kriteria4_KolomSensitifTidakIkutPadaDaftarTetapiAdaPadaDetail` | ✅ **Lulus** 26 Agu 2026 |
| 5. Tanpa hak akses ditolak 403 | **Tidak dapat diuji tanpa aplikasi berjalan** — lihat 5.3 | Tertunda |

Tiga test tambahan menjaga: perhitungan kedaluwarsa berjalan sebelum daftar dibaca, metadata
penyaring hanya menawarkan unit dan kelas yang berlaku untuk rawat inap, dan bentuk pagination
sama dengan endpoint daftar lain.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Empat endpoint baru. Tidak ada endpoint existing yang berubah |
| Database | **Tidak ada perubahan schema.** Tidak ada migration dibuat maupun dijalankan |
| Perilaku pembacaan | Membaca daftar episode kini dapat mengubah status episode `Draft` yang telantar. Ini perilaku yang disengaja, bukan efek samping |
| Keamanan | Kolom sensitif dijaga tidak bocor ke daftar. Penegakan hak akses per peran belum diverifikasi runtime |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Tidak ada satu pun kriteria yang benar-benar terbukti | Menjalankan perintah pada bagian 6.1 |
| Kriteria 403 belum terbukti | Peran yang tidak berhak mungkin dapat membaca daftar episode | Verifikasi runtime terhadap aplikasi berjalan |
| Kolom penyaring rentang tanggal belum dikonfirmasi | Layar daftar menyaring terhadap kolom yang tidak dimaksud Product/Domain | Konfirmasi Product/Domain — bagian 5.1 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Empat endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Kelima kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan; kriteria 5 belum dapat diuji |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** sebelum endpointnya terbukti berjalan |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`, lalu perbarui bagian 6 dengan hasil sebenarnya.
2. Konfirmasi kolom penyaring rentang tanggal ke Product/Domain (bagian 5.1).
3. Verifikasi 403 lewat pemanggilan endpoint dengan peran yang tidak berhak.
