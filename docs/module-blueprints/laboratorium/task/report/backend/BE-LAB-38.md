# Laporan Perubahan Backend — `BE-LAB-38`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-38` |
| Judul | `r17`: daftar penerimaan lintas pesanan |
| Slice | `MVP-5g`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6h |
| Trace | `FR-11.5`, `LAB-DEC-042`, `AC-67` |
| Contract version | `LAB-API-v1` **`r17`** bagian 12 — **`approved` 2026-09-17** |
| Dependency | `BE-LAB-37` ✅ — ruas `orderNumber` berasal dari sana |
| Klasifikasi | `MEDIUM` — satu endpoint, dua DTO, satu proyeksi berpaging. Nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `13665452` (belum di-stage maupun di-commit) |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** — 6 pemeriksaan runtime, seluruhnya `PASS`, termasuk **pembuktian dua arah** atas butir yang menentukan |

---

## 1. Masalah yang diperbaiki

`FE-LAB-12` menuntut daftar penerimaan **lintas pesanan** berpenyaring rentang tanggal.
Penelusuran source membuktikan **nol endpoint mengembalikannya**:

| Jalur yang ada sebelum task ini | Yang dikembalikan |
| --- | --- |
| `GET /lab-specimens/summary` | **Angka rekap saja** — nol baris |
| `GET /lab-specimens/by-order/{labOrderId}` | Baris untuk **satu** pesanan |
| `GET /lab-specimens/by-order/{labOrderId}/history` | Riwayat satu pesanan |

Penelusuran diteruskan ke luar controller wadah: `LabMonitoringService` menyentuh `LabSpecimens`
hanya untuk **mencacah** dan sebagai sub-query penyaring — ia memproyeksikan **pesanan**, bukan
wadah. `LabWorklistService` nol menyentuhnya.

**Setengahnya justru sudah siap sejak `BE-LAB-22`.** `GetSummaryAsync` menyaring tepat pada
`PhysicallyReceivedAt ?? CreateDateTime`, dan `AC-67` terpenuhi di sana. **Yang hilang barisnya,
bukan aturannya** — dan task ini menyalin aturan itu, bukan mengarangnya ulang.

---

## 2. Proses bisnis

Kepala instalasi membuka daftar penerimaan, memilih rentang tanggal, lalu melihat setiap wadah
yang **tiba** pada rentang itu — beserta nomor pesanannya, nama pasien, No. RM, jenis specimen,
volume, status, **waktu kedatangan sebenarnya**, dan **waktu pencatatannya**.

Kedua waktu itu ditampilkan berdampingan justru supaya **selisihnya** terbaca. Selisih sebelas jam
pada sampel yang datang pukul 21.00 adalah jam operasional; selisih yang sama pada sampel pukul
10.00 adalah pertanyaan yang pantas diajukan.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabSpecimenDtos.cs` | **+2 DTO**: `LabSpecimenPagedQuery` dan `LabSpecimenListResponse` |
| `Services/LabSpecimenService.cs` | **+1 method** `GetListAsync` beserta proyeksinya |
| `Controllers/LabSpecimenController.cs` | **+1 endpoint** `GET /lab-specimens` |

### 3.1 Tiga keputusan implementasi

**1. Penyaring rentangnya disalin dari `GetSummaryAsync`, bukan ditulis ulang.** Keduanya kini
menjawab pertanyaan yang sama dengan cara yang sama; bila salah satu kelak berubah, selisihnya
akan terlihat sebagai dua angka yang tidak cocok — bukan tersembunyi.

**2. Pengurutannya memakai ekspresi yang sama dengan penyaringnya** —
`PhysicallyReceivedAt ?? CreateDateTime`, terbaru lebih dulu. Mengurutkan menurut satu waktu
sementara menyaring menurut waktu lain menghasilkan daftar yang **benar isinya tetapi ganjil
urutannya**, dan keganjilan itu sulit dilacak.

**3. Nama pasien diterjemahkan di dalam proyeksi yang sama**, bukan lewat pencarian per baris
sesudahnya — pola yang sama dengan `LabMonitoringService`. Daftar 25 baris tidak boleh berubah
menjadi 51 perjalanan ke database hanya untuk menerjemahkan satu penunjuk.

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Dampak |
| --- | --- |
| Endpoint yang sudah ada | **Nol berubah** |
| Permission | **Nol** resource baru. Memakai ulang `LabSpecimen : Read` |
| Migration | **Nol.** Seluruh kolomnya berdiri sejak `BE-LAB-21` dan `BE-LAB-22` |

---

## 4. Endpoint yang ditambahkan

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-specimens` | Daftar penerimaan lintas pesanan | `LabSpecimen : Read` |

---

## 5. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 error, 0 warning** |

### 5.1 Enam pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga`

| # | Pemeriksaan | Hasil |
| ---: | --- | --- |
| `S1` | Endpoint berdiri; kelima ruas tambahan terbaca | `totalData 5`; `orderNumber`, `patientName`, `physicallyReceivedAt`, `createDateTime` seluruhnya terisi — **`PASS`** |
| `S2` | **Wadah tiba 15 Sep, dicatat 17 Sep → rentang 15 Sep** | `total 1`, wadah uji **muncul** — **`PASS`** |
| `S3` | **Wadah yang sama → rentang 17 Sep** | `total 0`, wadah uji **tidak muncul** — **`PASS`** |
| `S4` | Jalur cadangan — 5 wadah lama ber-`PhysicallyReceivedAt` `null`, dicatat 9 Sep | `total 5`, seluruhnya `null` — **`PASS`** |
| `S5` | Paging | Halaman 1, ukuran 2, total 6, totalPage 3, 2 baris — **`PASS`** |
| `S6` | Penyaring status | `total 1`, seluruhnya `Planned` — **`PASS`** |

### 5.2 `S2` dan `S3` adalah seluruh isi task ini

Keduanya menguji **satu wadah yang sama** dari dua arah:

| Rentang | Yang diharapkan | Kenapa |
| --- | --- | --- |
| 15 Sep | **Muncul** | Ia **tiba** 15 Sep |
| 17 Sep | **Tidak muncul** | Ia hanya **dicatat** 17 Sep |

**Implementasi yang keliru memakai `CreateDateTime` akan memberi hasil terbalik persis** — lulus
`S3` dan gagal `S2`. Tanpa `S3`, implementasi yang menyaring pada kedua kolom sekaligus juga akan
lolos, dan kekeliruannya tidak akan terlihat sampai ada yang membandingkan daftarnya dengan
kertas.

Inilah yang dimaksud butir DoD yang ditebalkan pada roadmap: kegagalannya **tidak menimbulkan
galat**. Endpointnya tetap berjalan, tetap mengembalikan baris, dan tetap terlihat benar — hanya
tanggalnya yang salah.

### 5.3 Kebersihan database

Satu pesanan uji beserta wadah dan jejaknya dihapus. Keadaan akhir diperiksa dari koneksi baru:
**8 pesanan, 5 wadah, nomor tertinggi `LAB-RSMMC-000008`** — persis seperti sebelum task ini.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Endpoint berdiri sesuai `r17` | **Terpenuhi** | `S1` |
| **Rentang disaring pada `PhysicallyReceivedAt ?? CreateDateTime`, dibuktikan dengan wadah yang kedua waktunya berbeda hari** | **Terpenuhi, dua arah** | `S2`, `S3` |
| Kelima ruas tambahan terbaca | **Terpenuhi** | `S1` |
| Nol endpoint lama berubah | **Terpenuhi** | Nol baris endpoint lain disentuh |

| AC | Status |
| --- | --- |
| `AC-67` | **Terpenuhi pada sisi daftar**, melengkapi sisi rekap yang sudah terpenuhi `BE-LAB-22` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru |
| Masalah yang diketahui | Pencarian bebas menjangkau barcode wadah dan nomor order; **nama pasien dan No. RM belum ikut dicari** — keduanya diterjemahkan lewat sub-query, dan memasukkannya ke penyaring menuntut `join` yang belum diukur pada daftar besar. Ruasnya tetap **ditampilkan**; yang belum hanya dicari |
| Dependency | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Nol `git add`, `commit`, maupun `push` |
| Langkah berikutnya | **`FE-LAB-12` kini nol penahan.** Menyelesaikannya menuntaskan `MVP-5a` |
