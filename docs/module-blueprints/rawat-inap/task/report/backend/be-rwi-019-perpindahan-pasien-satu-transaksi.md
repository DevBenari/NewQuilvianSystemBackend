# Laporan Perubahan Backend — `BE-RWI-019`

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
| Task ID | `BE-RWI-019` |
| Judul | Pasien dapat berpindah tanpa episode terputus |
| Slice | S4 — Penanggung jawab dan perpindahan |
| Trace | `RWI-DEC-012`, `RWI-DEC-013`, `RWI-DEC-014`, `RWI-DEC-023`; `RWI-RULE-006` s.d. `RWI-RULE-008`; `INV-INP-07`; `GUARD-INP-01`; api contract `POST /placements/transfer`; `FR-RI-120` s.d. `FR-RI-123`, `FR-RI-162`; `RWI-AC-133`; `UAT-08`, `UAT-09` |
| Contract version | API `0.4.0` — bentuk tidak berubah |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `bd97e5d` |
| Tanggal pengerjaan | 25 Agustus 2026 |
| Dependency | `BE-RWI-017`; `BE-RWI-013` dan `BE-RWI-015` — ketiganya dikerjakan pada sesi yang sama |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN** |

---

## 1. Apa yang dibangun

`POST /bed-occupancies/placements/transfer`. Satu tindakan utuh yang menutup penempatan lama
dan membuka penempatan baru **di dalam satu transaksi**:

1. baris penempatan lama ditutup dengan `EndReason = Transfer` dan alasan medisnya;
2. baris penempatan baru dibuka pada tempat tidur tujuan;
3. salinan `MstBed.BedStatus` tempat tidur tujuan menjadi `Occupied`;
4. salinan `MstBed.BedStatus` tempat tidur lama kembali `Available`.

Status episode **tidak berubah** — pasien tetap `Admitted`.

---

## 2. Satu daftar aturan, bukan dua

Roadmap menyebutnya sebagai kesalahan yang paling mahal di modul ini:

> *"Menulis daftar aturan kedua khusus perpindahan adalah kesalahan yang paling mahal di modul
> ini: dua daftar akan berselisih dalam hitungan minggu, dan jalur perpindahan justru yang
> paling sering dipakai petugas yang terburu-buru."*

`TransferAsync` memanggil `EvaluatePlacementEligibilityAsync` **yang sama persis** dengan
`PlacePatientAsync`. Tidak ada satu baris aturan pun yang ditulis ulang.

Ada satu test yang membuktikannya secara langsung: skenario penolakan jenis kelamin dijalankan
lewat **kedua jalur**, lalu status, kalimat, dan daftar kode aturannya dibandingkan — dan
ketiganya harus sama persis.

---

## 3. `INV-INP-07` — pasien tidak pernah tercatat tanpa tempat tidur

> **Kejadian yang dicegah.** Perpindahan Tn. Budi dari `MELATI-03-A` ke `ANGGREK-01-B` gagal di
> tengah jalan karena tempat tidur tujuan ternyata baru saja diambil pasien lain.
>
> Bila penutupan dan pembukaan adalah dua tindakan terpisah, penempatan lama sudah tertutup dan
> yang baru tidak jadi dibuka. Tn. Budi tercatat berada di ruangan **tanpa tempat tidur**,
> hilang dari census, dan tempat tidur lamanya muncul sebagai kosong — padahal ia masih
> berbaring di sana.

Karena keduanya berada di dalam satu transaksi, kegagalan membatalkan seluruhnya dan pasien
tetap berada di tempat semula. Ada test yang memaksa kegagalan di tengah transaksi dan
memeriksa bahwa baris penempatan lama **masih terbuka**.

---

## 4. Yang berubah pada source

| Berkas | Jenis | Isi perubahan |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Ditambah | `TransferAsync` |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs` | Ditambah | `TransferPatientRequest` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | Ditambah | Aksi `POST /placements/transfer` dengan butir `InpatientBedOccupancy : Transfer` |

### 4.1 `GUARD-INP-01` berlaku hanya untuk pemohon berperan dokter

Kepala ruangan, perawat pelaksana, dan supervisor tetap boleh memindahkan tanpa menjadi DPJP.
Itu `RWI-DEC-012` yang tidak dicabut, dan risikonya sudah diterima secara sadar sebagai
`RWI-RISK-001`.

Yang ditolak adalah **dokter yang bukan DPJP aktif** episode itu, dan tidak ada kolom
keterangan apa pun pada `TransferPatientRequest` yang dapat dipakai melewatinya. Ada test yang
memeriksa jumlah kolom permintaannya tepat tiga.

### 4.2 Kelas yang ditagihkan mengikuti kamar tujuan

`RWI-DEC-013`. Baris penempatan baru mengambil `PatientClassId` dari kamar tujuan, sehingga
riwayat penempatan dapat menunjukkan 2 hari kelas 2 lalu 2 hari kelas 1.

Kolom kelas pada `InpEpisode` **tidak** ditimpa — ia tetap merekam pilihan saat admisi dibuka.
Menimpanya akan menghapus jejak kelas awal, dan riwayat penagihan kehilangan titik awalnya.

---

## 5. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` |
| Pemilik/prefix pada registry | `InPatientManagement / Inpatient`, prefix `Inp` |
| Lifecycle registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Pengecualian QBE | Tidak ada |

**`QBE-TXN-001`** adalah aturan penentu pada task ini, dan ia diuji langsung.

---

## 6. Keputusan implementasi yang perlu ditinjau

### 6.1 Pasien `DischargePending` ditolak dengan pesan yang berbeda

State matrix membedakan dua penolakan:

| Keadaan | Kalimatnya |
| --- | --- |
| `DischargePending` | "Pasien sudah diputuskan boleh pulang, sehingga tidak dapat dipindahkan lagi." |
| Status lain selain `Admitted` | "Perpindahan hanya dapat dilakukan selama pasien masih dirawat." |

Keduanya 422, dan pembedaannya disengaja: yang pertama memberi tahu petugas bahwa yang perlu
dihubungi adalah DPJP, bukan bahwa ia salah memilih menu.

### 6.2 Unit layanan pada episode tidak ikut berubah

Perpindahan antar unit layanan menghasilkan baris penempatan baru dengan `ServiceUnitId` kamar
tujuan, tetapi `InpEpisode.ServiceUnitId` **tidak** ditimpa — dengan alasan yang sama seperti
kelas.

Konsekuensinya: penyaringan daftar episode menurut unit layanan menyaring terhadap unit
**admisi**, sementara census menyaring terhadap unit **penempatan saat ini**. Perbedaan itu
benar secara semantik tetapi perlu diketahui perancang layar. Bila Product/Domain menghendaki
keduanya sama, sebutkan.

---

## 7. Validasi

### 7.1 Yang **belum** dijalankan

| Perintah | Keadaannya |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | ✅ **PASS** — Build succeeded, 0 Error(s); dijalankan 26 Agustus 2026, dan diulang tiga kali berturut-turut dengan hasil sama |
| `dotnet test QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` | ✅ **PASS** — Passed! Failed 0, Passed 255, Skipped 0, Total 255 |
| `UAT-08`, `UAT-09`, dan `UAT-29` lewat jalur perpindahan terhadap aplikasi berjalan | **NOT RUN** |

### 7.2 Test yang ditulis

`QuilvianSystemBackend.Tests/InPatientManagement/InpBedTransferTests.cs` — 8 test.

| Acceptance criteria | Test yang membuktikan | Status |
| --- | --- | --- |
| 1. Dua baris penempatan; yang lama punya waktu berakhir dan alasan `Transfer` | `Kriteria1_PerpindahanMenghasilkanDuaBarisPenempatanDanYangLamaDitutupDenganAlasanTransfer` | ✅ **Lulus** 26 Agu 2026 |
| 2. Bila pembukaan penempatan baru gagal, penempatan lama tidak jadi ditutup | `Kriteria2_BilaPembukaanPenempatanBaruGagalPasienTetapDiTempatSemula` — memaksa kegagalan di tengah transaksi | ✅ **Lulus** 26 Agu 2026 |
| 3. Kelas yang ditagihkan mengikuti kamar tujuan | `Kriteria3_KelasYangDitagihkanMengikutiKamarTujuan` | ✅ **Lulus** 26 Agu 2026 |
| 4. Dokter yang bukan DPJP aktif ditolak 403, tanpa kolom keterangan yang dapat melewatinya | `Kriteria4_DokterYangBukanDpjpAktifDitolak403SementaraKepalaRuanganTetapBoleh` | ✅ **Lulus** 26 Agu 2026 |
| 5. Perpindahan tanpa alasan medis ditolak 400 | `Kriteria5_PerpindahanTanpaAlasanMedisDitolak400` | ✅ **Lulus** 26 Agu 2026 |
| 6. Penolakan jenis kelamin lewat perpindahan **sama persis** dengan lewat penempatan | `Kriteria6_PenolakanJenisKelaminLewatPerpindahanSamaPersisDenganLewatPenempatan` | ✅ **Lulus** 26 Agu 2026 |

Dua test tambahan menjaga: perpindahan ke tempat tidur yang sama ditolak 400, dan pasien yang
sudah diputuskan pulang tidak dapat dipindahkan.

---

## 8. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Aditif.** Satu endpoint baru dengan butir hak akses `InpatientBedOccupancy : Transfer` |
| Database | Tidak ada perubahan schema |
| Modul tetangga | Salinan `MstBed.BedStatus` ditulis pada dua tempat tidur sekaligus di dalam satu transaksi |

---

## 9. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| **Build dan test belum dijalankan** | Keenam kriteria belum terbukti | Bagian 7.1 |
| Penguncian baris tempat tidur tujuan belum diuji | Dua perpindahan bersamaan ke tempat tidur yang sama belum terbukti tertangani | Test terhadap PostgreSQL |
| Selisih arti unit layanan antara daftar episode dan census | Layar menampilkan angka yang berbeda untuk pertanyaan yang tampak sama | Konfirmasi Product/Domain — bagian 6.2 |

---

## 10. Definition of Done

| Butir DoD | Keadaannya |
| --- | --- |
| Endpoint sesuai kontrak | ✅ Ada di dalam kode |
| Keenam kriteria lulus | ✅ **Lulus** — seluruh test-nya dijalankan 26 Agustus 2026 dan hijau (255/255) |
| Terbukti hanya ada **satu** daftar aturan di seluruh source | ✅ `TransferAsync` dan `PlacePatientAsync` memanggil method yang sama; ada test yang membandingkan hasil keduanya |
| Api contract diperbarui | ❌ **Belum, dan memang belum boleh** |

---

## 11. Langkah berikutnya

1. Jalankan `dotnet build` dan `dotnet test`.
2. Jalankan `UAT-29` lewat jalur perpindahan terhadap aplikasi berjalan.
3. Konfirmasi arti unit layanan pada daftar episode (bagian 6.2).
