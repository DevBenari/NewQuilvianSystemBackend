# Laporan Perubahan Backend — `BE-RWI-008`

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
| Task ID | `BE-RWI-008` |
| Judul | Admisi dapat diperbaiki, dibatalkan, dan gugur sendiri bila ditinggalkan |
| Slice | S1 — Petugas dapat membuka admisi dan memesan tempat tidur |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `RWI-DEC-010`, `RWI-DEC-030`; `RWI-RULE-004`, `RWI-RULE-022`; api contract `0.4.0` `PUT /episodes/{id}` dan `PATCH /episodes/{id}/cancel`; state matrix bagian 1; `RWI-AC-007` s.d. `RWI-AC-010`, `RWI-AC-044` s.d. `RWI-AC-046`, `RWI-AC-090` s.d. `RWI-AC-092` |
| Contract version | API `0.4.0` — **bentuknya tidak berubah**. Dua endpoint yang sebelumnya "Rencana (belum tersedia)" kini ada di dalam kode |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `11711a1` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Jenis perubahan | Pengisian `InpEpisodeService`; dua aksi tambahan pada controller yang lahir bersama `BE-RWI-007` |
| Dependency | `BE-RWI-007` — dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Lihat bagian 6 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. `dotnet build` dan `dotnet test` **tidak dijalankan** pada sesi
> ini. Task ini karena itu **belum boleh ditandai selesai**.

---

## 1. Apa yang dibangun, dan kenapa

### 1.1 Tiga keadaan yang sebelumnya tidak punya jalan keluar

Sejak `BE-RWI-007`, admisi dapat dibuka. Tiga keadaan berikutnya belum punya jalan keluar sama
sekali:

| Keadaan | Tanpa task ini |
| --- | --- |
| Petugas salah mengisi kelas perawatan | Satu-satunya jalan adalah membuka admisi kedua, dan yang pertama menggantung selamanya |
| Pasien membatalkan rencana rawat inap | Episode tetap `Draft` dan tempat tidur yang dipesannya tetap terkunci |
| Pasien tidak pernah datang, dan tidak ada yang memberi tahu petugas | Episode `Draft` menumpuk tanpa batas, dan laporan admisi ikut salah |

### 1.2 Yang dibuka task ini

Dua endpoint — `PUT /episodes/{id}` dan `PATCH /episodes/{id}/cancel` — ditambah satu perilaku
yang **tidak** punya endpoint: kedaluwarsa episode `Draft` yang dihitung saat data dibaca.

---

## 2. Proses bisnis

### 2.1 Siapa boleh membatalkan, dan sampai kapan

Mengikuti `RWI-RULE-004` apa adanya:

| Keadaan episode | Siapa yang boleh | Yang dilakukan sistem |
| --- | --- | --- |
| `Draft` | Petugas admisi | Melepas pemesanan dan penempatan, membatalkan kunjungan yang lahir bersama episode, lalu memindahkan status |
| `Admitted` | **Hanya** supervisor atau kepala ruangan | Sama, ditambah pemeriksaan kewenangan |
| `DischargePending` | Tidak ada | Ditolak 422 |
| `Closed` | Tidak ada | Ditolak 409 |
| `Cancelled` | Tidak ada | Ditolak 409 |

Alasan wajib diisi orang, dan alasan yang hanya berisi tanda baca ditolak (`RWI-AC-008`).
Barisnya **tidak dihapus**, hanya ditandai batal, sehingga tetap dapat ditelusuri saat diaudit.

### 2.2 Kedaluwarsa dihitung saat dibaca

Tidak ada program penjadwal yang berjalan di latar belakang. Episode `Draft` yang tidak
disentuh melewati `DraftEpisodeExpiryHours` dibatalkan **pada saat seseorang membacanya lewat
service**.

> **Contoh berangka.**
>
> **12 Agustus 09:15** — Petugas membuka admisi Ibu Sari lewat jalur pasien datang langsung.
> Sistem membuat kunjungan rawat inap, dan episode berstatus `Draft`.
>
> **13 Agustus 09:20** — Seseorang membuka episode itu. Sistem menghitung bahwa umurnya sudah
> 24 jam 5 menit, melewati batas 24 jam. **Pada pembacaan itu juga**, episode menjadi
> `Cancelled` dengan alasan sistem, kunjungannya ikut ditandai batal, dan satu baris riwayat
> lahir bertanda **dilakukan sistem** — bukan dilakukan orang yang kebetulan membuka layar.

### 2.3 Konsekuensi yang disengaja

Karena kedaluwarsa dihitung saat dibaca, baris `Draft` basi **tetap ada di tabel** sampai
seseorang membacanya.

> **Akibatnya bagi laporan.** Laporan yang menghitung baris langsung dari tabel `InpEpisode`
> tanpa melewati service akan menghitung episode basi itu sebagai admisi yang masih disiapkan.
> Angkanya salah, dan salahnya terlihat wajar. Setiap pembaca `InpEpisode` — laporan, daftar
> pantau, papan mana pun — **wajib** lewat `InpEpisodeService`. Ini bukan anjuran gaya
> penulisan; ia syarat kebenaran angka.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diisi | `UpdateAdmissionAsync`, `CancelAdmissionAsync`, `GetEpisodeAsync`, `ExpireDraftIfDueAsync`, `CancelEpisodeInternalAsync`, `ReleaseBedHoldsAsync`, `CancelAnchorEncounterAsync` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Ditambah | Aksi `PUT /{id}` dan `PATCH /{id}/cancel` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | Ditambah | `UpdateAdmissionRequest`, `CancelAdmissionRequest` |

### 3.1 Kedaluwarsa **disimpan**, bukan sekadar dihitung

`ExpireDraftIfDueAsync` membatalkan **dan menyimpan** dalam transaksinya sendiri.

> **Kenapa ini penting, dan kenapa versi pertama implementasi ini salah.** Versi pertama hanya
> menghitung kedaluwarsanya lalu menyerahkan penyimpanan kepada pemanggil. Akibatnya:
> `UpdateAdmissionAsync` dan `CancelAdmissionAsync` menolak permintaannya dan **kembali tanpa
> menyimpan apa pun**, sehingga episode yang "sudah gugur" itu masih `Draft` di basis data.
> Pembacaan berikutnya menghitungnya gugur lagi, dan seterusnya selamanya: pemesanan tidak
> pernah dilepas, kunjungan tidak pernah ditandai batal, dan tidak satu pun baris riwayat
> lahir. Cacat ini ditemukan saat review diri sebelum laporan ditulis, dan sudah diperbaiki.

### 3.2 Kunjungan mana yang ikut dibatalkan

Hanya kunjungan yang **dibuat sendiri oleh proses admisi**, dikenali dari
`InpStatusHistory.ActionType == "OpenAdmissionWithEncounter"` pada baris pertama episode —
penanda yang ditulis `BE-RWI-007`.

Kunjungan yang **ditunjuk petugas** adalah milik alur pendaftaran dan **tidak pernah**
dibatalkan modul ini. Ada satu test yang menjaga batas itu
(`Kriteria5_KunjunganYangDitunjukPetugasTidakIkutDibatalkan`).

### 3.3 Yang **sengaja tidak ada** pada `UpdateAdmissionRequest`

| Field | Kenapa tidak ada |
| --- | --- |
| `PatientId` | Pasien adalah jangkar episode. Menukarnya berarti episode yang lain, bukan koreksi isian. Salah pilih pasien dibetulkan dengan membatalkan lalu membuka admisi baru — persis contoh pada `RWI-RULE-004` |
| `EncounterId` | Sama. Ditambah lagi, menukar kunjungan akan menabrak `INV-INP-04` |
| `DoctorId` | Pengalihan DPJP punya endpoint bermakna sendiri (`POST /{id}/doctor-assignments`, `BE-RWI-017`) dan meninggalkan riwayat penugasan. Mengubahnya diam-diam lewat `PUT` akan menghapus jejak siapa DPJP sebelumnya |

Batas ini adalah keputusan implementasi; acceptance criteria hanya mewajibkan penolakan pada
episode yang bukan `Draft`. Bila Product/Domain menghendaki himpunan field yang berbeda,
sebutkan sebelum layar admisi dibangun.

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
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

### 4.1 `QBE-DEL-001` — lifecycle pembatalan dan jejak pelaku

Pembatalan **tidak menghapus** baris. Ia mengisi `CancelReason`, `IsCancel`, `CancelDateTime`,
`CancelBy`, dan `IsActive = false` pada episode, ditambah satu baris `InpStatusHistory` yang
memuat status asal, status tujuan, pelaku, waktu, alasan, nomor urut, dan penanda orang atau
sistem. Pemesanan dan penempatan diperlakukan sama: ditandai berakhir, tidak dihapus.

### 4.2 Kewenangan supervisor berada di service, bukan di mesin hak akses

Pembatalan `Draft` dan pembatalan `Admitted` memakai butir hak akses yang **sama**, yaitu
`InpatientEpisode : Update` (permission matrix bagian 2.1). Mesin hak akses karena itu tidak
dapat membedakan keduanya. Penjaganya berada di `InpEpisodeService.CancelAdmissionAsync`, dan
controller hanya menyampaikan apakah pelakunya berperan supervisor atau kepala ruangan.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Batas "belum ada catatan klinis" **belum diperiksa**

**Yang diminta.** `RWI-RULE-004` mewajibkan pembatalan episode `Admitted` ditolak bila sudah ada
satu saja dari enam jenis catatan klinis: pengkajian awal keperawatan, catatan dan tindakan
keperawatan, CPPT, resep rawat inap, tindakan dokter, dan tanda vital. `RWI-AC-007` dan
`RWI-AC-091` menuntut hal yang sama.

**Yang ada sekarang.** Pemeriksaan itu **belum diimplementasikan**. Keenam jenis catatan itu
milik `ClinicalManagement` dan `PharmacyManagement`, dan membacanya berarti menetapkan enam
jalur integrasi baca baru yang **tidak disebut** pada `contracts/integration-contract.md`
maupun pada scope task ini.

**Akibatnya hari ini: tidak ada.** Episode belum dapat mencapai status `Admitted` sebelum
`BE-RWI-011` membuka penempatan pasien, sehingga cabang `Admitted` pada `CancelAdmissionAsync`
belum punya jalur yang benar-benar terpakai.

**Akibatnya bila didiamkan: besar.** Sejak `BE-RWI-011` rilis, supervisor akan dapat
membatalkan episode yang sudah punya pengkajian dan tanda vital, dan catatan klinis itu
menempel pada episode yang berstatus `Cancelled`. **Ini wajib ditutup sebelum `BE-RWI-011`
dinyatakan selesai**, bukan sesudahnya. Owner: Backend/API bersama Product/Domain.

### 5.2 Salinan `MstBed.BedStatus` belum dikembalikan ke `Available`

Pembatalan sudah melepas `InpBedReservation` dan `InpBedPlacement`. Yang **belum** dilakukan
adalah menuliskan kembali salinan status pada `MstBed.BedStatus` — arah tulis `INT-INP-03`.

Itu milik `InpBedOccupancyService`, yang masih kerangka sampai `BE-RWI-010` dan `BE-RWI-011`.
Menuliskannya dari sini akan menduplikasi jalur tulis yang sama di dua tempat.

**Akibatnya hari ini: tidak ada.** Tidak ada satu pun baris `InpBedReservation` maupun
`InpBedPlacement` yang dapat lahir sebelum `BE-RWI-010` dan `BE-RWI-011`, sehingga tidak ada
salinan status yang perlu dikembalikan.

**Yang harus terjadi.** `BE-RWI-011` memindahkan `ReleaseBedHoldsAsync` ke belakang
`InpBedOccupancyService`, dan pengembalian salinan status ikut di dalamnya. `RWI-AC-006` —
"membatalkan episode `Draft` mengembalikan tempat tidurnya ke `Available` pada tindakan yang
sama" — baru terbukti penuh pada saat itu.

### 5.3 Daftar nama peran supervisor adalah asumsi

`InpatientEpisodeController.SupervisorOrWardHeadRoles` berisi `SuperAdmin`, `Supervisor`,
`KepalaRuangan`, dan `Kepala Ruangan`. Nama peran di repository ini adalah **data yang
disiapkan admin**, bukan daftar tetap di dalam kode, dan tidak ada satu pun kontrak modul ini
yang menyebutkan nama peran sesungguhnya.

Bila nama peran di rumah sakit berbeda, penjaga ini akan menolak supervisor yang sah. Perlu
dikonfirmasi pemilik modul sebelum `BE-RWI-011` rilis. Owner: Product/Domain.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| Pemanggilan `PUT /episodes/{id}` dan `PATCH /episodes/{id}/cancel` terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpEpisodeDraftLifecycleTests.cs` — 16 test, ditambah 7 test kontrak controller pada `InpatientEpisodeControllerContractTests.cs` yang dipakai bersama `BE-RWI-007`.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Mengubah isian episode yang bukan `Draft` ditolak | `Kriteria1_MengubahIsianEpisodeYangBukanDraftDitolak`, `MengubahIsianEpisodeDraftBerhasil` | ✅ **Lulus** 26 Agu 2026 |
| 2. Pembatalan melepas pemesanan dan penempatan dalam satu tindakan utuh | `Kriteria2_PembatalanDraftBerhasilDanMelepasPemesananSertaPenempatan` | ✅ **Lulus** 26 Agu 2026. Cakupannya belum penuh — lihat bagian 5.2 |
| 3. Pembatalan setelah `Admitted` hanya supervisor atau kepala ruangan; peran lain 403 | `Kriteria3_PembatalanEpisodeAdmittedOlehPeranLainDitolak403`, `Kriteria3_PembatalanEpisodeAdmittedOlehSupervisorBerhasil` | ✅ **Lulus** 26 Agu 2026. Batas catatan klinis **belum** diuji karena belum ada — lihat bagian 5.1 |
| 4. `Draft` telantar terbaca `Cancelled` pada pembacaan berikutnya, tanpa penjadwal | `Kriteria4_DuaPembacaanPadaWaktuBerbedaMembuktikanTidakAdaPenjadwal`, `Kriteria4_KedaluwarsaDitulisSebagaiTindakanSistem`, `Kriteria4_EpisodeYangSudahGugurTidakDapatDiubahLagi` | ✅ **Lulus** 26 Agu 2026 |
| 5. Kunjungan yang ikut lahir bersama episode ikut dibatalkan | `Kriteria5_KunjunganYangLahirBersamaEpisodeIkutDibatalkan`, `Kriteria5_KunjunganYangDitunjukPetugasTidakIkutDibatalkan`, `Kriteria5_KunjunganIkutDibatalkanSaatEpisodeGugurSendiri` | ✅ **Lulus** 26 Agu 2026 |
| 6. Batas jam dapat diubah admin dan berlaku pada pembacaan berikutnya | `Kriteria6_BatasJamYangDiubahAdminBerlakuPadaPembacaanBerikutnya` | ✅ **Lulus** 26 Agu 2026 |

Empat test tambahan menjaga: pembatalan tanpa alasan, alasan yang hanya tanda baca
(`RWI-AC-008`), episode yang sudah dibatalkan tidak dapat dibatalkan lagi, dan episode
`DischargePending` tidak dapat dibatalkan.

Verifikasi yang diminta roadmap — "test dua pembacaan pada waktu berbeda yang membuktikan tidak
ada penjadwal yang dijalankan" — dijawab
`Kriteria4_DuaPembacaanPadaWaktuBerbedaMembuktikanTidakAdaPenjadwal`: pembacaan pertama
mengembalikan `Draft`, jejak sentuhan dimundurkan, pembacaan kedua mengembalikan `Cancelled`,
dan **tidak ada satu pun proses** yang dijalankan di antara keduanya.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Dua endpoint baru. Tidak ada endpoint existing yang berubah |
| Database | **Tidak ada perubahan schema.** Tidak ada migration dibuat maupun dijalankan |
| Modul tetangga | Baris `TrxPatientEncounter` yang dibuat modul ini dapat ditandai batal. Kunjungan milik alur pendaftaran tidak tersentuh. Lihat laporan `BE-RWI-007` bagian 5.3 |
| Perilaku pembacaan | **Berubah.** Sejak task ini, membaca episode `Draft` lewat service dapat mengubah statusnya. Pembaca yang melewati service akan melihat angka yang salah — lihat bagian 2.3 |
| Keamanan | Penjaga kewenangan supervisor ditambahkan di service. Belum diverifikasi terhadap nama peran sesungguhnya — lihat bagian 5.3 |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Tidak ada satu pun kriteria yang benar-benar terbukti | Menjalankan perintah pada bagian 6.1 |
| Batas catatan klinis belum diperiksa | Sejak `BE-RWI-011` rilis, episode yang sudah punya pengkajian dan tanda vital dapat dibatalkan | **Wajib ditutup sebelum `BE-RWI-011` selesai** — bagian 5.1 |
| Salinan `MstBed.BedStatus` belum dikembalikan | Tempat tidur dapat terlihat terisi padahal episodenya sudah batal | `BE-RWI-011` — bagian 5.2 |
| Nama peran supervisor keliru | Supervisor yang sah ditolak 403 | Konfirmasi Product/Domain — bagian 5.3 |
| Laporan membaca `InpEpisode` tanpa lewat service | Episode `Draft` basi terhitung sebagai admisi yang masih disiapkan | Ditegakkan lewat review; disebut pada bagian 2.3 dan pada komentar service |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Tiga kemampuan selesai | ✅ Ubah isian, batalkan, dan gugur sendiri — ketiganya ada di dalam kode |
| Keenam kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan. Kriteria 2 dan 3 cakupannya belum penuh — bagian 5.1 dan 5.2 |
| Api contract diperbarui | ✅ **Sudah** — status dinaikkan `Rencana` → `Tersedia` pada 26 Agustus 2026, setelah endpointnya terbukti berjalan (Swagger HTTP 200, 49 operasi, 401 tanpa token) |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`, lalu perbarui bagian 6 dengan hasil sebenarnya.
2. Bawa bagian 5.1 ke Product/Domain **sebelum** `BE-RWI-011` dimulai. Ia gerbang, bukan
   catatan tambahan.
3. Konfirmasi nama peran supervisor dan kepala ruangan (bagian 5.3).
4. `BE-RWI-009` membuka endpoint baca; ia wajib memanggil jalur kedaluwarsa yang sama supaya
   daftar episode tidak menampilkan `Draft` basi.
