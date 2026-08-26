# Laporan Perubahan Backend — `BE-RWI-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-010` |
| Judul | Tempat tidur dapat dicari dan dipesan, dan pemesanan gugur sendiri |
| Slice | S1 — Petugas dapat membuka admisi dan memesan tempat tidur |
| Trace | `RWI-DEC-008`; `RWI-RULE-001`, `RWI-RULE-002`; `INV-INP-02` sebagian; api contract `/available-beds`, `/bed-board`, `POST /reservations`, `PATCH /reservations/{id}/cancel`; `RWI-AC-001` s.d. `RWI-AC-003` |
| Contract version | API `0.4.0` — bentuk tidak berubah; empat endpoint "Rencana" kini ada |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Jenis perubahan | Pengisian `InpBedOccupancyService`; controller `InpatientBedOccupancyController` baru |
| Dependency | `BE-RWI-004` 🟡; data master kamar dan tempat tidur **belum terbukti terisi** |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN, DAN GERBANG DATA MASTER MASIH TERBUKA.** Lihat bagian 5.1 dan 6 |

> **Peringatan yang tidak boleh dilewat.** Pengerjaan dilakukan **tanpa menjalankan build**
> atas permintaan pemilik pekerjaan. Selain itu, gerbang kesiapan data master pada roadmap
> bagian 5 **masih terbuka**, sehingga keenam acceptance criteria task ini belum dapat diuji
> terhadap data sungguhan.

---

## 1. Apa yang dibangun

Empat endpoint pada grup Bed Occupancy:

| Endpoint | Kegunaannya |
| --- | --- |
| `GET /bed-occupancies/available-beds` | Mencari tempat tidur yang benar-benar dapat ditempati |
| `GET /bed-occupancies/bed-board` | Papan ketersediaan per unit layanan dan kamar |
| `POST /bed-occupancies/reservations` | Memesan tempat tidur untuk satu episode `Draft` |
| `PATCH /bed-occupancies/reservations/{id}/cancel` | Membatalkan pemesanan sebelum dipakai |

Ditambah tiga aturan pertama **Kelayakan Penempatan**, yang dipakai bersama oleh pemesanan,
penempatan, perpindahan, dan penyaringan pencarian.

---

## 2. Proses bisnis

### 2.1 Pemesanan mengunci, lalu gugur sendiri

> **Contoh berangka.** Sdri. Wati memesan `BD-RSMMC-00042` pukul 09:15 untuk Ny. Sari. Batas
> pemesanan 2 jam, jadi gugur pukul 11:15.
>
> **Pembacaan pukul 11:14** — tempat tidur itu **tidak muncul** pada pencarian petugas lain.
> **Pembacaan pukul 11:16** — tempat tidur itu **muncul kembali**, dan baris pemesanannya
> sudah berstatus `Expired`.
>
> Di antara kedua pembacaan itu **tidak ada satu pun proses yang berjalan**. Yang
> menggugurkannya adalah pembacaan kedua itu sendiri.

Tidak ada program penjadwal. Ini konsekuensi `RWI-DEC-007` yang disengaja, dan akibatnya sama
seperti pada kedaluwarsa episode `Draft`: siapa pun yang membaca `InpBedReservation` langsung
dari tabel tanpa lewat service akan menghitung pemesanan basi sebagai pemesanan yang masih
berlaku.

### 2.2 Batas waktunya milik admin, bukan milik kode

`MstInpatientSetting.BedReservationMinutes` dibaca ulang **setiap kali** pemesanan dibuat.
Angka yang diubah admin pukul 10:00 berlaku pada pemesanan pukul 10:01, tanpa aplikasi
dinyalakan ulang.

Pemesanan yang sudah terlanjur dibuat tetap memakai batas yang berlaku saat ia dibuat, karena
`ExpiresAt` disimpan pada barisnya sendiri. Ini penting: mengubah batas dari 2 jam menjadi
30 menit tidak boleh menggugurkan pemesanan yang sedang berjalan secara surut.

### 2.3 Salinan status tempat tidur

Pemesanan menulis `MstBed.BedStatus = Reserved`; pembatalan dan kedaluwarsa mengembalikannya
menjadi `Available` — **tetapi hanya bila tempat tidur itu tidak sedang dipegang episode lain**,
dan **tidak pernah** bila keadaannya `Cleaning`, `Maintenance`, `Blocked`, atau `Inactive`.

> **Kenapa keempat keadaan itu tidak boleh ditimpa.** Admin menutup tempat tidur `MELATI-03-B`
> karena rangkanya patah. Bila modul ini menimpanya menjadi `Available` saat sebuah pemesanan
> dibatalkan, tempat tidur patah itu kembali muncul pada pencarian — dan pasien berikutnya
> ditempatkan di sana. Batas ini adalah isi `INT-INP-03`, disetujui `RWI-DEC-062`.

---

## 3. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Diisi | `SearchAvailableBedsAsync`, `GetBedBoardAsync`, `ReserveBedAsync`, `CancelReservationAsync`, `ExpireDueReservationsAsync`, `GetReservationAsync`, `EvaluatePlacementEligibilityAsync` aturan 1–3, `LockBedRowAsync`, `WriteBedStatusCopyAsync`, `ReleaseBedStatusCopyAsync` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | **Baru** | Empat aksi task ini, ditambah tiga aksi milik `BE-RWI-011` dan `BE-RWI-019` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs` | **Baru** | `AvailableBedQuery`, `AvailableBedResponse`, `AvailableBedPagedResult`, `BedBoardResponse` beserta tiga bentuk turunannya, `ReserveBedRequest`, `CancelReservationRequest`, `BedReservationResponse` |

---

## 4. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-TXN-001`** — pemesanan, pembatalan, dan kedaluwarsa masing-masing berada di dalam satu
transaksi bersama penulisan salinan `MstBed.BedStatus`. Bila salah satu gagal, tidak ada yang
tersimpan.

**`QBE-LOG-001`** — pemesanan dan pembatalannya dicatat `LoggerService` dengan payload yang
hanya memuat identitas baris, controller, action, dan kode status. Endpoint `GET` tidak
dicatat, mengikuti konvensi project.

---

## 5. Keputusan implementasi yang perlu ditinjau

### 5.1 Gerbang data master masih terbuka

Roadmap bagian 5 menyatakan: *"Kesiapan data master — penanggung jawab ditetapkan
`RWI-DEC-063`, target 22 Agustus 2026. Sejak revision 3 penandanya harus **benar**, bukan
sekadar terisi. Menahan: `BE-RWI-010` ke atas tidak dapat diuji."*

**Yang dikerjakan task ini adalah kodenya, bukan pengujiannya terhadap data sungguhan.** Test
yang ditulis memakai kamar dan tempat tidur buatan sendiri di dalam database di memori,
sehingga ia membuktikan **kode** berperilaku benar — bukan bahwa data master rumah sakit sudah
benar. Keduanya tidak saling menggantikan.

> **Risiko yang nyata bila ini dilupakan.** Bila penanda `MstBed.IsForMale`, `IsForFemale`, dan
> `IsIsolationBed` salah pada data sungguhan, seluruh aturan kelayakan akan bekerja dengan
> benar terhadap data yang salah — dan hasilnya adalah penolakan yang tampak seperti cacat
> program.

### 5.2 Penguncian baris tidak dapat diuji provider InMemory

`LockBedRowAsync` menjalankan `SELECT 1 FROM public."MstBed" WHERE "Id" = {0} FOR UPDATE`, dan
**hanya** pada penyedia relasional. Provider InMemory yang dipakai test tidak mengenal
`FOR UPDATE` maupun transaksi sungguhan, sehingga pemanggilan itu dilewati di sana.

Konsekuensinya: lapis pertama penjagaan `INV-INP-02` **tidak terbukti** oleh test mana pun
pada sesi ini. Pembuktiannya harus dijalankan terhadap PostgreSQL sungguhan.

### 5.3 `IsReservable` tidak diperiksa pada jalur pencarian

Penanda `MstBed.IsReservable` diperiksa hanya ketika jalur yang memanggil adalah **pemesanan**.
Pencarian tempat tidur dan penempatan langsung tidak memeriksanya, karena tempat tidur yang
tidak dapat dipesan tetap dapat ditempati langsung.

Akibatnya, `GET /available-beds` dapat menampilkan tempat tidur yang kemudian ditolak
`POST /reservations`. Ini konsisten dengan arti endpoint-nya — "tempat tidur yang dapat
**ditempati**" — tetapi perlu diketahui perancang layar. Bila layar pemesanan menghendaki
penyaringan yang berbeda, sebutkan.

---

## 6. Validasi

### 6.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **NOT RUN** — diminta pemilik pekerjaan |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | **NOT RUN** — diminta pemilik pekerjaan |
| Test unique index parsial pemesanan aktif terhadap PostgreSQL | **NOT RUN** — memerlukan database sungguhan |
| Pemanggilan keempat endpoint terhadap aplikasi berjalan | **NOT RUN** |

### 6.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpBedReservationTests.cs` — 10 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Tempat tidur `Reserved` tidak muncul pada pencarian | `Kriteria1_TempatTidurYangSudahDipesanTidakMunculPadaPencarian` | Ditulis, **belum dijalankan** |
| 2. Pukul 11:14 masih mengunci, pukul 11:16 sudah bebas, tanpa penjadwal | `Kriteria2_DuaPembacaanPadaWaktuBerbedaMembuktikanTidakAdaPenjadwal` | Ditulis, **belum dijalankan** |
| 3. Batas 2 jam dapat diubah admin dan berlaku pada pemesanan berikutnya | `Kriteria3_BatasWaktuYangDiubahAdminDipakaiPemesananBerikutnya` | Ditulis, **belum dijalankan** |
| 4. Memesan tempat tidur yang sudah dipesan episode lain ditolak 409 | `Kriteria4_MemesanTempatTidurYangSudahDipesanEpisodeLainDitolak409` | Ditulis, **belum dijalankan** |
| 5. Memesan tempat tidur `Maintenance` ditolak 422 dengan pesan yang menyebut keadaannya | `Kriteria5_MemesanTempatTidurBerstatusPerbaikanDitolak422DenganPesanYangMenyebutKeadaannya` | Ditulis, **belum dijalankan** |
| 6. Papan ketersediaan mengelompokkan per unit layanan dan kamar | `Kriteria6_PapanKetersediaanMengelompokkanPerUnitLayananDanKamar` | Ditulis, **belum dijalankan** |
| Unique index parsial pemesanan aktif | **Tidak dapat diuji InMemory** — lihat 5.2 | Tertunda |

Empat test tambahan menjaga: satu episode hanya boleh punya satu pemesanan aktif, pemesanan
hanya untuk episode `Draft`, pembatalan mengembalikan salinan status, dan salinan status tidak
pernah menimpa keadaan yang merupakan wewenang admin.

---

## 7. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Empat endpoint baru |
| Database | **Tidak ada perubahan schema.** Tidak ada migration dibuat maupun dijalankan |
| Modul tetangga | Modul ini mulai **menulis** `MstBed.BedStatus`. Arah tulis `INT-INP-03`, disetujui `RWI-DEC-062`. Hanya tiga nilai yang ditulis: `Available`, `Reserved`, `Occupied` |
| Keamanan | Butir hak akses `InpatientBedOccupancy : Read/Create/Update` terdaftar otomatis oleh `AccessMenuSeeder` |

---

## 8. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Tidak ada satu pun kriteria yang benar-benar terbukti | Menjalankan perintah pada bagian 6.1 |
| **Data master belum terbukti benar** | Seluruh test task ini gagal karena alasan yang salah — bukan karena kodenya salah | Menuntaskan `RWI-DEC-063` |
| Penguncian baris belum diuji | Lapis pertama `INV-INP-02` belum terbukti | Test terhadap PostgreSQL sungguhan |
| Selisih penyaringan `IsReservable` | Layar pemesanan menawarkan tempat tidur yang kemudian ditolak | Konfirmasi perancang layar — bagian 5.3 |

---

## 9. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Empat endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keenam kriteria lulus | ❌ **Belum.** Test ditulis, belum dijalankan |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 10. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Pastikan `RWI-DEC-063` tuntas sebelum menguji terhadap data sungguhan.
3. Jalankan test tabrakan dan test unique index parsial terhadap PostgreSQL.
