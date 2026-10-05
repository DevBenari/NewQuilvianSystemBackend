# Laporan Perubahan Backend — `BE-FIN-085`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-085` |
| Judul | Berkas batch migrasi yang masih Draf dapat diunggah ulang, dan pesan penolakan rekonsiliasi memuat dua desimal |
| Slice | `REV-14E` (`EPIC FIN-24`) — amandemen pasca-`FE-FIN-032` |
| Roadmap | **Belum terdaftar.** Baris task pada `roadmap/01-backend-roadmap.md` belum ditulis karena wewenang tulis artefak blueprint tidak diberikan pada giliran ini (§7) |
| Trace | `FR-FIN-168`, `FR-FIN-169`; `FIN-DEC-136`, `FIN-DEC-140`; `FIN-DES-093`; `FIN-STATE-1.6` F.2 (baris "Mengunggah ulang berkas"); `FIN-VAL-192` |
| Contract version | `FIN-STATE-1.6` F.2 (sudah memuat tindakan unggah ulang); `FIN-API-1.6` F.2 **belum** memuat endpoint-nya (§7); `FIN-VAL-1.7` `FIN-VAL-192` |
| Dependency | `BE-FIN-081` 🟡, `BE-FIN-082` 🟡 (source lengkap; pengguna melaporkan `dotnet build` sukses setelah task itu ditulis) |
| Klasifikasi | `MEDIUM` — 3 berkas diubah, 1 endpoint baru, **refactor** metode unggah yang sudah ada; database tidak disentuh |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 3 Oktober 2026 (hanya backend source; artefak kontrak/blueprint **tidak** diberikan) |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/` |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Source selesai.** `dotnet build` **NOT RUN** (instruksi pengguna: tidak membangun otomatis). Tidak ada migration dan tidak ada perubahan skema. **Belum pernah dikompilasi** |

---

## 1. Masalah yang Diperbaiki

Saat `FE-FIN-032` dibangun ditemukan dua selisih pada alur batch migrasi:

1. **Unggah ulang tidak punya endpoint.** `state-transition-matrix.md` F.2 menetapkan tindakan *Mengunggah ulang berkas* pada batch `DRAFT` (`DRAFT` → `DRAFT`, hak `Update`, hasil validasi sebelumnya dibuang), tetapi `POST /` hanya membuat batch baru. Memperbaiki berkas berarti membuat batch baru dan menolak yang lama. Pemilik memutuskan (3 Oktober 2026): **tambah endpoint**.
2. **Pesan penolakan rekonsiliasi membulatkan.** `ApproveAsync` memformat kedua angka dengan `N0` mengikuti budaya server, sehingga selisih di bawah Rp 1 tidak terlihat pada teks, dan redaksinya tidak memuat kalimat penutup `FIN-VAL-192` (*"Batch tidak dapat disetujui."*).

---

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Services/FinanceOpeningItemBatchService.cs` | (a) **Refactor:** pemeriksaan berkas unggahan (terlampir, jenis item sah, bukan XLSX, terbaca, memuat baris, kolom templat lengkap) diekstrak dari `UploadAsync` menjadi `ReadAndCheckUploadAsync`, dipakai bersama oleh unggah dan unggah ulang — aturannya satu, bukan dua salinan. Urutan pemeriksaan dan pesan galat tidak berubah. (b) **Baru:** `ReuploadAsync`. (c) Pesan `422` rekonsiliasi memakai `N2` dengan budaya `id-ID` dan kalimat penutup kontrak |
| `DTOs/OpeningItemBatchDtos.cs` | `ReuploadOpeningItemBatchRequest` (`File`, `ExpectedRowVersion`) |
| `Controllers/FinanceOpeningItemBatchesController.cs` | `POST /{id}/reupload` — hak `FinanceOpeningItemBatch : Update`, aksi `Update` yang sama dengan validasi dan deklarasi |

### 2.1 Perilaku `ReuploadAsync`

| Langkah | Keterangan |
| --- | --- |
| Batch tidak ditemukan | `404` |
| Status bukan `DRAFT` | `409` — *"Berkas hanya dapat diunggah ulang selama batch berstatus DRAFT."* (batch `VALIDATED` termasuk: sesuai matriks, unggah ulang hanya dari `DRAFT`) |
| `ExpectedRowVersion` basi | `409` — *"Data telah berubah. Muat ulang sebelum melanjutkan."* |
| Berkas | Pemeriksaan yang sama dengan unggah memakai `ItemKind` **milik batch** (jenis item tidak dapat diganti): `400` berkas kosong/tidak terbaca/kolom kurang; `503` XLSX |
| Saldo awal cutover belum ditetapkan | `422`, pesan yang sama dengan unggah |
| Hasil | `UploadedFileName`, `SourceFormat`, `CutoverDate` (diambil ulang seperti saat unggah) diperbarui; `TotalItemCount` dan `TotalOutstandingAmount` kembali `0`; `ValidationSummaryJson` dikosongkan; `RowVersion` diputar. **Dipertahankan:** `DeclaredAccountingOpeningAmount` dan `AccountingReferenceDocument` (pernyataan petugas, bukan hasil berkas) |
| Penyimpanan berkas | Berkas baru ditulis ke jalur sementara `*.tmp`; **berkas lama tidak disentuh** sampai `SaveChanges` berhasil. Sesudahnya berkas sementara menggantikan berkas final (`overwrite`) dan berkas lama berformat berbeda dibuang. Bila `SaveChanges` gagal, berkas sementara dihapus |

### 2.2 Endpoint

`POST /api/v1/corporate/finance-management/opening-item-batches/{id}/reupload` — `multipart/form-data`: `file`, `expectedRowVersion`. Respons `200` `ApiResponse<OpeningItemBatchResponse>`; galat `400`, `404`, `409`, `422`, `503`. Atribut `[AccessController]` tidak berubah; resource/action baru: **nol**.

---

## 3. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `NOT RUN` | Instruksi pengguna. **Belum terkompilasi**; yang paling perlu dicermati kompilator: metode privat berkelas tuple, deklarasi `IndonesianCulture`, interpolasi `ToString("N2", …)` di dalam string interpolasi |
| Pembacaan kode — urutan galat `UploadAsync` tidak berubah | `PASS` | Blok diekstrak apa adanya; hanya buffer kini dibuang pada galat (`try/catch`) dan dikembalikan ke pemanggil |
| Pembacaan kode — `ReuploadAsync` tidak menyentuh berkas lama sebelum basis data tersimpan | `PASS` | Urutan: tulis `*.tmp` → `SaveChanges` → `Move` |
| Uji manual | `NOT FEASIBLE` | Butuh build dan basis data |

---

## 4. Risiko

1. **Refactor `UploadAsync` menyentuh jalur yang sudah ada** dan belum dikompilasi. Regresinya paling mungkin berupa kesalahan kompilasi, bukan perubahan perilaku; yang harus diamati setelah build: unggah CSV sah, berkas kosong, jenis item kosong, XLSX (`503`).
2. **Jendela kegagalan setelah `SaveChanges`.** Bila `File.Move` gagal (mis. berkas terkunci) sesudah basis data menunjuk berkas baru, metadata baru dan berkas fisik lama tidak sinkron. Jendelanya sempit, tetapi tidak nol; tidak ada transaksi lintas basis data dan sistem berkas.
3. **`CutoverDate` diambil ulang saat unggah ulang.** Bila saldo awal cutover diubah sejak unggah pertama, batch ikut nilai terbaru — sejalan dengan `FIN-DEC-129` tetapi perlu diketahui.
4. **Berkas lama berformat sama ditimpa** setelah basis data tersimpan; tidak ada riwayat berkas sebelumnya (sesuai matriks: hasil validasi lama dibuang).
5. **SourceFormat masih ditetapkan "CSV" apa adanya** (KNOWN LIMITATION `BE-FIN-081`, ditinjau `BE-FIN-083`); unggah ulang mewarisi batas itu.

---

## 5. Status Git (backend)

```text
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs
 M Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/OpeningItemBatchDtos.cs
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs
```

(Perubahan lain yang belum di-commit berasal dari task sebelumnya.) Tidak ada stage, commit, push, pull, merge, rebase, atau deploy.

---

## 6. Dampak ke Frontend

`FE-FIN-032` memakai endpoint ini (thunk `reuploadOpeningItemBatch`, bagian *Unggah Ulang Berkas* pada panel rincian untuk batch `DRAFT`). Tanpa build dan penerapan backend, tombolnya akan gagal `404`.

---

## 7. Yang Belum Dikerjakan — Menunggu Wewenang Artefak Kontrak

Pengguna memberi wewenang **backend source saja**. Artefak kontrak/blueprint di bawah ini **tidak** diubah dan tertinggal dari source:

| Berkas | Perubahan yang diperlukan |
| --- | --- |
| `contracts/api-contract.md` F.2 | Tambah baris `POST /{id:guid}/reupload` (`multipart/form-data`; `FinanceOpeningItemBatch : Update`; `ReuploadOpeningItemBatchRequest`; `OpeningItemBatchResponse`; kode `400`, `404`, `409`, `422`, `503`). Beri label **Tersedia** pada kedelapan endpoint F.2 yang sudah ada (`BE-FIN-080`..`082`) |
| `contracts/validation-matrix.md` `FIN-VAL-192` | Redaksi source kini: *"Total sisa tagihan (Rp A) tidak sama dengan saldo awal Accounting yang dinyatakan (Rp B). Batch tidak dapat disetujui."* dengan dua desimal; samakan atau pertahankan kontrak lama |
| `roadmap/01-backend-roadmap.md` | Daftarkan `BE-FIN-085` pada REV-14E beserta bukti laporan ini |
| `contracts/state-transition-matrix.md` F.2 | **Tidak perlu diubah:** baris "Mengunggah ulang berkas" kini punya endpoint |
