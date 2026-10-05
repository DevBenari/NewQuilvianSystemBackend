# Laporan Perubahan Backend — `BE-FIN-088`

> **Catatan penamaan berkas.** Mengikuti konvensi yang sudah berlaku pada 16 laporan backend
> modul ini (huruf besar, tanpa judul ringkas) — lihat catatan serupa pada `BE-FIN-086.md`.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-088` |
| Judul | Berkas migrasi yang terlalu besar ditolak sebelum menyentuh penyimpanan, beserta arahan memecahnya |
| Slice | `REV-16B` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `REV-16B` |
| Trace | `FR-FIN-188`; `FIN-DEC-155`; `FIN-DES-096` |
| Contract version | `FIN-API-1.7` G.2; `FIN-VAL-1.9` `FIN-VAL-229` |
| Dependency | `BE-FIN-085` — **terpenuhi**: `ReadAndCheckUploadAsync` sudah ada di source dan dipakai `UploadAsync` **dan** `ReuploadAsync` sebagai aturan unggah tunggal |
| Klasifikasi | `LIGHT` — 1 berkas, 1 konstanta, 1 pemeriksaan di dalam metode yang sudah ada. Nol migration, nol controller, nol DTO |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 4 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` |
| Tanggal | 4 Oktober 2026 |
| Status | 🟡 **Source selesai.** `dotnet build` **NOT RUN** (instruksi pengguna: tanpa build otomatis). Nol migration, nol database. Uji manual `K.3.1`..`K.3.6` **belum dilaporkan** |

---

## 1. Perubahan

### 1.1 Source

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` | 1 konstanta `MaxUploadRowCount = 10_000`; 1 pemeriksaan di dalam `ReadAndCheckUploadAsync`, sesudah pemeriksaan kelengkapan kolom dan sebelum `return` |

### 1.2 Kenapa satu pemeriksaan menutup unggah **dan** unggah ulang

`ReadAndCheckUploadAsync` adalah metode privat bersama yang sudah diekstrak `BE-FIN-085` dan
dipakai **kedua** pemanggil publik (`UploadAsync`, `ReuploadAsync`). Menaruh pemeriksaan batas di
sana — bukan menduplikasinya di masing-masing pemanggil — berarti `FIN-DES-096` ("aturannya
tunggal") terpenuhi secara struktural: tidak ada cara memanggil unggah atau unggah ulang tanpa
lewat pemeriksaan ini.

### 1.3 Posisi pemeriksaan dan akibatnya pada K.3.3/K.3.4

Pemeriksaan ditaruh **sesudah** `reader.Read(buffer)` mengurai baris (jumlah baris sebenarnya
hanya diketahui setelah itu — menghitung dari ukuran berkas adalah terkaan yang dilarang
`FIN-DEC-140`) dan **sebelum** `return` ke pemanggil. Akibatnya struktural, bukan perilaku yang
ditambahkan terpisah:

- Pada `UploadAsync`: `physicalPath`, `Directory.CreateDirectory`, dan `_dbContext.Add` seluruhnya
  terjadi **sesudah** `ReadAndCheckUploadAsync` kembali. Exception yang dilempar di dalamnya
  membuat baris-baris itu **tidak pernah tercapai** — nol berkas fisik, nol baris batch (`K.3.3`).
- Pada `ReuploadAsync`: `finalPath`/`tempPath`, mutasi `entity.*`, dan `SaveChangesAsync` juga
  seluruhnya sesudah pemanggilan itu. Batch tetap memakai berkas lamanya dan hasil validasi
  lamanya **tidak terhapus** (`K.3.4`) — bukan karena ditangani khusus, melainkan karena baris
  mutasi itu tidak pernah dieksekusi.
- Blok `catch { buffer.Dispose(); throw; }` yang membungkus seluruh isi `ReadAndCheckUploadAsync`
  tetap menangkap exception baru ini seperti exception lain di metode itu — buffer di memori
  dibuang, tidak ada kebocoran.

### 1.4 Baris judul tidak dihitung (`K.3.5`) — diverifikasi pada pembaca, bukan ditambahkan di sini

`rows.Count` yang diperiksa adalah nilai balik `reader.Read(buffer)`. Dibaca
`CsvOpeningItemFileReader.Read` (baris 39: `for (var i = 1; i < records.Count; i++)`) — baris
indeks `0` (judul) **sudah** dikecualikan oleh pembaca sebelum method ini menerimanya. Pemeriksaan
batas tidak perlu — dan tidak boleh — mengurangi `1` lagi; melakukannya akan membuat berkas
10.001 baris data (10.002 baris fisik) salah diterima sebagai batas.

### 1.5 Redaksi pesan — disalin persis dari kontrak

Pesan `400` memakai teks statis yang sama persis dengan `contracts/validation-matrix.md` baris
`FIN-VAL-229`: *"Berkas memuat lebih dari 10.000 baris. Pecah menjadi beberapa berkas, lalu
unggah masing-masing sebagai batch tersendiri."* Tidak disisipi nilai dinamis — kontrak sendiri
tidak menuntutnya, dan menambah nilai dinamis akan menyimpang dari redaksi yang sudah disetujui.

---

## 2. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `NOT RUN` | Instruksi eksplisit pengguna pada giliran ini. **Belum terkompilasi** |
| Pembacaan statis — kurung kurawal seimbang | `PASS` | `FinanceOpeningItemBatchService.cs`: 133 `{` = 133 `}` |
| Pembacaan statis — exception sudah dipetakan controller | `PASS` | `OpeningItemBatchBadRequestException` → `400` sudah ada di `FinanceOpeningItemBatchesController.MapError`/`IsHandled` sejak sebelumnya; **nol** perubahan controller diperlukan |
| Pembacaan statis — pemeriksaan tercapai dari kedua pemanggil | `PASS` | `UploadAsync` baris 75 dan `ReuploadAsync` baris 249 keduanya memanggil `ReadAndCheckUploadAsync` sebelum operasi disk/DB apa pun |
| Pembacaan statis — baris judul dikecualikan pembaca | `PASS` | `CsvOpeningItemFileReader.Read`, loop mulai `i = 1` |
| Uji manual `K.3.1`..`K.3.6` | `BELUM DILAPORKAN` | Butuh build dan berkas uji 10.000/10.001 baris |

---

## 3. Acceptance Criteria

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | Berkas tepat 10.000 baris data diterima (batas inklusif) | Terpenuhi (source); belum dikompilasi | `rows.Count > MaxUploadRowCount` (`>`, bukan `>=`) |
| 2 | Berkas 10.001 baris ditolak `400` beserta pesan yang menyebut batas dan menyarankan memecah | Terpenuhi (source) | Pesan §1.5 |
| 3 | Baris judul tidak dihitung | Terpenuhi (source, pra-ada) | §1.4 |
| 4 | Berkas yang ditolak meninggalkan nol berkas fisik dan nol baris batch | Terpenuhi (source, struktural) | §1.3 |
| 5 | Batas berlaku pada unggah **dan** unggah ulang | Terpenuhi (source, struktural) | §1.2 |

---

## 4. Risiko dan Hal yang Harus Diketahui

1. **Belum dikompilasi.** Sintaks konstanta `10_000` (pemisah digit C#) dan operator `>` pada
   `int` vs `List<T>.Count` adalah hal paling sederhana yang mungkin salah ketik — risiko kecil,
   tetap perlu `dotnet build` sebelum dipercaya.
2. **`K.3.6` (migrasi besar dipecah 23.000 → tiga batch) adalah uji workflow lintas-task**, bukan
   perilaku baru task ini. Task ini hanya memastikan setiap unggahan tunggal dibatasi; memecah
   berkas dan menyatakan saldo awal masing-masing batch adalah alur yang **sudah ada** sejak
   `BE-FIN-081`/`082`.
3. **Batas bersifat global, bukan per jenis item.** `RECEIVABLE` dan `SUPPLIER_PAYABLE` memakai
   batas yang sama — sesuai `FIN-DES-096`, yang tidak membedakan jenis item.
4. **Tidak ada uji otomatis ditambahkan.** Sesuai `rules/backend/TEST_POLICY.md` — bukan
   coverage gap.

---

## 5. Status Git (backend)

```text
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs
```

(Perubahan lain yang belum di-commit berasal dari task sebelumnya.) Tidak ada stage, commit,
push, pull, merge, rebase, atau deploy.

---

## 6. Dampak ke Frontend

`FE-FIN-035` (REV-16 frontend, belum dikerjakan) menunggu rilis ini: layar batch migrasi **MUST**
menangani `400` dengan keterangan yang menyebut batasnya dan menyarankan memecah berkas, **tanpa**
mengirim ulang berkas secara otomatis.

---

## 7. Yang Belum Dikerjakan

| Hal | Keadaan |
| --- | --- |
| `dotnet build` | **NOT RUN** — instruksi pengguna |
| Uji manual `K.3.1`..`K.3.6` | **BELUM DILAPORKAN** |
| `contracts/api-contract.md` G.2 | Tidak perlu diubah — endpoint sudah ada, bentuk request/response tidak berubah |
| `roadmap/01-backend-roadmap.md` | Baris `BE-FIN-088` **MUST** diperbarui: ⬜ → 🟡 |

### Langkah yang perlu pengguna jalankan

1. `dotnet build` — pastikan perubahan terkompilasi.
2. Uji manual: unggah berkas tepat 10.000 baris data (harus `200`); unggah 10.001 baris (harus
   `400` dengan pesan yang menyebut batas); ulangi keduanya lewat endpoint unggah ulang pada
   batch `DRAFT`.
3. Setelah itu task dapat ditandai ✅ pada roadmap.
