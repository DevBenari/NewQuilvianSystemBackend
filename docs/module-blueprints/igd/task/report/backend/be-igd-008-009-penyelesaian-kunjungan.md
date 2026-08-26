# Laporan Perubahan Backend — `BE-IGD-008` dan `BE-IGD-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-008`, `BE-IGD-009` |
| Judul | Status `Completed` dipisahkan dari `Disposed`, dan kunjungan dapat diselesaikan setelah closure gate terpenuhi |
| Slice | S3 — Kunjungan dapat diselesaikan secara klinis |
| Trace | `IGD-DEC-049`, `IGD-DEC-021`, `IGD-GAP-001`; state matrix bagian 1; validation matrix bagian 3; `AT-IGD-030` sampai `AT-IGD-035` |
| Commit backend | `21c609f2853574532f74dd2b1489b8d2e502abd1` |
| Tanggal | 18 Agustus 2026 |
| **Status** | **Belum selesai — belum dikompilasi; `BE-IGD-008` juga menunggu rilis `FE-IGD-005`** |

Kedua task ditulis dalam satu laporan karena `BE-IGD-009` tidak dapat berdiri tanpa nilai enum
yang dibuat `BE-IGD-008`, dan keduanya menyentuh berkas yang sama.

---

## 1. Masalah yang diperbaiki

Sebelum ini, `Disposed` merangkap dua arti: "dokter sudah memutuskan tindak lanjut" **dan**
"urusan pasien di IGD sudah tuntas". Akibatnya `VisitCompletedAt` terisi begitu keputusan
ditetapkan, padahal pasien mungkin masih diobservasi atau masih dalam proses perpindahan.

Laporan lama tinggal pasien di IGD karena itu salah: pasien yang masih ada di ruangan sudah
terhitung selesai.

## 2. Proses bisnis

### 2.1 Dua keadaan yang akhirnya terpisah

| Status | Arti | Yang mengisinya |
| --- | --- | --- |
| `Disposed` | Keputusan tindak lanjut sudah ditetapkan | `PATCH /{id}/visit-status` |
| `Completed` | Urusan pasien di IGD benar-benar tuntas | `PATCH /{id}/complete` |

`Completed` bernilai 9 supaya delapan nilai lama tidak bergeser dan data lama tetap terbaca
dengan arti yang sama.

### 2.2 Closure gate

| No | Syarat | Pesan bila dilanggar | Kode |
| --- | --- | --- | --- |
| 1 | Status harus `Disposed` | "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan." | 409 |
| 2 | Tidak ada observasi `Active` | "Masih ada observasi yang belum diselesaikan." | 409 |
| 3 | Tidak ada transfer selain `Completed` atau `Rejected` | "Masih ada proses perpindahan yang belum selesai." | 409 |
| 4 | Billing **bukan** syarat | — | — |

Aturan keempat adalah yang paling mudah dilanggar tanpa sadar. Sesuai `IGD-DEC-021`, tagihan
yang belum final **tidak** menahan penutupan klinis. Pasien yang pulang pukul 03.00 saat bagian
keuangan tidak bertugas tetap harus dapat dinyatakan selesai secara klinis, supaya ia tidak
terhitung masih aktif di IGD.

Karena itu `ValidateVisitClosureAsync` sama sekali tidak menyentuh entitas billing. Ketiadaan
pemeriksaan itu disengaja, bukan terlupa.

## 3. File yang diubah

| File | Task | Perubahan |
| --- | --- | --- |
| `Enums/EmergencyVisitStatus.cs` | 008 | `Completed = 9` |
| `Services/EmergencyVisitService.cs` | 008 | `Disposed` boleh ke `Completed`; `Completed` tidak boleh ke mana pun |
| `Controller/EmergencyVisitController.cs` | 008 | `VisitCompletedAt` tidak lagi diisi saat `Disposed` maupun `Cancelled`; target `Completed` ditolak lewat endpoint status umum |
| `Services/EmergencyDispositionService.cs` | 009 | `ValidateVisitClosureAsync` |
| `DTOs/EmergencyVisitDtos.cs` | 009 | `CompleteVisitRequest` |
| `Controller/EmergencyVisitController.cs` | 009 | `PATCH /{id}/complete` |

### 3.1 Dua lubang yang ditutup

**Jalan pintas lewat endpoint status umum.** State matrix mengesahkan transisi
`Disposed` ke `Completed`, sehingga `CanTransition` harus mengizinkannya. Tetapi bila
`PATCH /{id}/visit-status` dibiarkan memakai transisi itu, seluruh closure gate terlewati.
Endpoint status umum karena itu menolak target `Completed` secara terpisah dan mengarahkan
pemakai ke aksi selesaikan kunjungan.

**`Completed` ke `Completed`.** Kriteria roadmap menuntut transisi dari `Completed` ke status
mana pun ditolak. Konvensi `CanTransition` yang sudah ada mengembalikan benar untuk status yang
sama, sehingga pemeriksaan `Completed` diletakkan **sebelum** jalan pintas itu.

### 3.2 Waktu selesai tidak diterima dari pemanggil

`CompleteVisitRequest` hanya memuat `Notes`. `VisitCompletedAt` diisi waktu server, supaya
penutupan kunjungan tidak dapat dimundurkan oleh pemanggil.

## 4. Data lama sengaja tidak disentuh

Baris lama yang terlanjur memiliki `VisitCompletedAt` terisi pada status `Disposed` **tidak**
diubah. Mengubahnya berarti memalsukan riwayat.

Konsekuensinya harus dicatat terbuka: **kolom `VisitCompletedAt` kini punya dua arti**
tergantung kapan barisnya dibuat. Laporan yang menghitung kunjungan selesai berdasarkan kolom
itu akan mencampur keduanya sampai batas waktu perubahan arti ditetapkan owner.

Ini risiko nomor 4 pada roadmap bagian 8 dan belum ada keputusannya.

## 5. Ketidakcocokan dokumen yang ditutup

Arsitektur bagian 3.4 menempatkan aksi penyelesaian pada `EmergencyDispositionController`,
sedangkan api contract dan permission matrix menempatkannya di bawah `emergency-visits` dengan
hak `EmergencyVisit : Update`.

Asumsi yang dipakai sesuai arahan roadmap: **dua dokumen kontrak menang atas satu dokumen
arsitektur.** Endpoint berada di `EmergencyVisitController`, sedangkan logika gate-nya tetap di
`EmergencyDispositionService` sesuai arsitektur bagian 3.3. Dengan begitu keduanya terpenuhi
sejauh mungkin.

Yang masih harus dirapikan: arsitektur bagian 3.4. Owner: Backend/API + Product/Domain.

## 6. Verifikasi

**Belum ada verifikasi berjalan.** Build tidak dijalankan; `AT-IGD-030` sampai `AT-IGD-035`
tidak punya tempat untuk ditulis karena solution tidak memiliki test project.

| Kriteria | Task | Status |
| --- | --- | --- |
| Nilai `Completed = 9` tersedia | 008 | Ada di kode — **belum terbukti** |
| `VisitCompletedAt` tidak terisi saat `Disposed` | 008 | Ada di kode — **belum terbukti** |
| `VisitCompletedAt` tidak terisi saat `Cancelled` | 008 | Ada di kode — **belum terbukti** |
| Data lama tidak diubah | 008 | Tidak ada migration data — **terjamin secara struktur** |
| Transisi dari `Completed` ditolak | 008 | Ada di kode — **belum terbukti** |
| Dari `Disposed` berhasil, waktu server terisi | 009 | Ada di kode — **belum terbukti** |
| Selain `Disposed` ditolak 409 | 009 | Ada di kode — **belum terbukti** |
| Observasi `Active` menolak | 009 | Ada di kode — **belum terbukti** |
| Transfer belum tuntas menolak | 009 | Ada di kode — **belum terbukti** |
| Billing tidak menghalangi | 009 | Ada di kode — **belum terbukti** |
| Tanpa hak akses ditolak 403 | 009 | Ada di kode — **belum terbukti** |

## 7. Gate yang belum terpenuhi

**`FE-IGD-005` belum rilis.** Roadmap mensyaratkan frontend menangani nilai enum baru lebih
dulu, karena `Completed = 9` berpotensi memutus tampilan yang memetakan status secara eksklusif.
Di sisi frontend belum ada satu pun task IGD yang dikerjakan.

Akibatnya DoD `BE-IGD-008` **tidak** terpenuhi walaupun kodenya selesai. Perubahan ini tidak
boleh dirilis ke produksi sebelum frontend siap.

## 8. Risiko tersisa

| No | Risiko | Penanganan |
| ---: | --- | --- |
| 1 | Arti `VisitCompletedAt` berubah di tengah jalan | Butuh keputusan owner soal batas waktu perubahan arti |
| 2 | `Completed = 9` memutus konsumen API lama | Gate `FE-IGD-005`; api contract sudah menandainya berpotensi memutus |
| 3 | Transfer `Cancelled` dihitung belum tuntas | Mengikuti validation matrix apa adanya; bila keliru, perlu penegasan owner |
| 4 | Arsitektur bagian 3.4 masih menyebut controller yang salah | Perlu dirapikan, bukan dibiarkan |
