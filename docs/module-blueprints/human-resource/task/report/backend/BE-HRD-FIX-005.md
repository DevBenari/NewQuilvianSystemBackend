# Laporan Perubahan Backend — `BE-HRD-FIX-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-FIX-005` |
| Judul | Template batch penempatan awal gagal `500` karena pegawai tercatat di lebih dari satu baris batch |
| Slice | Perbaikan cacat di luar roadmap pada fitur `S-FIN-M07` · Penempatan Awal |
| Roadmap | Tidak ada kartu. Dicatat pada `roadmap/backend-roadmap.md` di baris Status kartu `BE-HRD-412` |
| Trace | `HRD-DEC-054` (sekali per pegawai), `AUD-HRD07-15`; endpoint `GET api/v1/corporate/human-resource/workforce-core/initial-batches/template` |
| Contract version | Tidak berubah. Bentuk request/response, kode diagnostik, dan pesan tetap sama |
| Dependency | `BE-HRD-412` ✅ (fitur induk) |
| Klasifikasi | `LIGHT` — skor 2: satu repository (0), sekitar 8 berkas diperiksa (1), 1 berkas source diubah (0), logika validasi yang dipakai bersama oleh template, validasi, pembuatan draf, validasi ulang, dan eksekusi batch (1), tanpa perubahan API (0), tanpa schema (0), tanpa keamanan (0) |
| Task mode | `BACKEND` (perbaikan bug atas instruksi pengguna 24 November 2026: "tolong atasi masalah eror ini", disertai respons `500` endpoint template) |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs`; laporan ini; baris Status kartu `BE-HRD-412` di roadmap; `roadmap/requirement-traceability.md` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7281c68d` (branch `MHamzah`), working tree bersih sebelum task ini |
| Tanggal | 24 November 2026 |
| Status | 🟡 Sebagian — `dotnet build` 0 error, QBE Strict `PASS`; uji ulang endpoint di backend yang berjalan `NOT RUN` (backend perlu di-restart pengguna untuk memuat perubahan) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore` (batch penempatan awal) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` — perbaikan minimal tanpa refactor (legacy ratchet) |
| QBE yang berlaku | `QBE-SVC-001` (aturan tetap di service), `QBE-VAL-001` (invarian sekali per pegawai tetap ditegakkan) |
| Tidak berlaku | `QBE-API-001`/`QBE-DTO-001` (kontrak tidak berubah), `QBE-PERM-001` (hak akses tidak disentuh), `QBE-DB-*` (tanpa schema), `QBE-NAM-*`, `QBE-CODE-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Saat petugas SDM membuka layar Batch Penempatan Awal, frontend memanggil `GET .../initial-batches/template` untuk menampilkan
daftar pegawai baseline beserta status kelayakannya. Pada 24 November 2026 pukul 10.42 dan 10.48 WIB, panggilan itu gagal
dengan `500 "Terjadi kesalahan pada server."`, sehingga daftar pegawai tidak tampil sama sekali.

Log backend (`logs/quilvian-backend-20261124.json`) menunjukkan penyebabnya:

```text
System.ArgumentException: An item with the same key has already been added. Key: 49333d61-93e1-46d2-b6b7-3b1329db26df
   at ...ToDictionaryAsync(...)
   at HrdInitialPlacementBatchService.GetTemplateRowsAsync(CancellationToken ct) ... line 81
```

Artinya: untuk menandai pegawai yang **sudah pernah** masuk batch berstatus `Approved` atau `Executed` (aturan `HRD-DEC-054`),
service memuat baris batch itu ke kamus yang satu kuncinya satu pegawai. Pegawai `49333d61-…` ternyata tercatat di **lebih
dari satu** baris batch tersebut, sehingga kamus menolak kunci kedua dan seluruh permintaan gagal.

**Contoh:** pegawai A tercatat di batch `BATCH-INIT-…-0001` (`Executed`) dan juga di baris batch lain yang `Approved`. Sebelum
perbaikan, satu pegawai seperti ini membuat **seluruh** template gagal. Sesudah perbaikan, pegawai A tampil sebagai
`DILEWATI_SUDAH_DITEMPATKAN` dengan pesan "Pegawai sudah tercatat dalam batch 'BATCH-INIT-…-0001' …", dan pegawai lain tampil
normal.

Index unik pada tabel baris batch hanya mencegah pegawai yang sama muncul dua kali **di dalam satu batch yang sama**
(`BatchId` + `WorkforceProfileId`, baris tidak terhapus). Pegawai yang sama di dua batch berbeda, atau baris yang terhapus
lunak, tetap mungkin terjadi, dan query lama tidak menyaring baris terhapus lunak. Mana dari kedua kemungkinan itu yang
terjadi pada data pegawai `49333d61-…` belum diperiksa karena query database belum diizinkan (lihat bagian 7).

Cacat yang sama ada di `ValidateRowsAsync`. Method itu dipakai oleh `POST /validate`, `POST /` (buat draf),
`POST /{id}/validate`, dan `POST /{id}/execute`, jadi semua aksi itu juga akan `500` bila memuat pegawai yang sama.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Petugas SDM | Membuka layar Batch Penempatan Awal; frontend memanggil `GET /template` |
| 2 | Backend | Mengambil penugasan organisasi primer aktif (pegawai baseline), jabatan primer aktif, dan **nomor batch `Approved`/`Executed` per pegawai** |
| 3 | Backend | Memberi tiap pegawai satu status: `DILEWATI_SUDAH_DITEMPATKAN` (sudah di batch disetujui/dieksekusi, atau sudah punya jabatan aktif), `PERLU_PERBAIKAN` (non-aktif, jabatan kosong/tidak aktif, departemen tidak cocok), atau `SIAP_DIPROSES` |
| 4 | Petugas SDM | Memilih pegawai `SIAP_DIPROSES`, memvalidasi (`POST /validate`), lalu membuat draf (`POST /`) |
| 5 | Pengaju / penyetuju | Mengajukan, menyetujui (bukan pembuat/pengaju), lalu mengeksekusi batch; validasi dijalankan ulang sebelum eksekusi |

**Jalur tidak normal yang diperbaiki:** pada langkah 2, bila satu pegawai punya lebih dari satu baris batch `Approved`/`Executed`,
backend kini memilih **batch paling awal** (urut tanggal buat batch, lalu nomor batch) sebagai nomor yang ditampilkan,
alih-alih gagal. Aturan "sekali per pegawai" tidak melemah: pegawai itu tetap ditolak/dilewati persis seperti sebelumnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdInitialPlacementBatchController.cs` (route, hak akses, tidak ada `try/catch` di `GetTemplate`)
- `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs`
- `Areas/Corporate/HumanResource/WorkforceCore/Models/HrdInitialPlacementBatch.cs` (navigasi `Batch` nullable)
- `Repositories/Configurations/Corporate/HumanResource/WorkforceCore/HrdInitialPlacementBatchConfiguration.cs` (index unik baris per batch saja)
- `Repositories/Configurations/Corporate/HumanResource/WorkforceCore/WfpPositionAssignmentConfiguration.cs` (index unik tersaring jabatan primer aktif per pegawai)
- `logs/quilvian-backend-20261124.json` (stack trace)
- `AGENTS.md`, `docs/engineering/*`, laporan `BE-HRD-412` dan `BE-HRD-FIX-004` (pola laporan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs` | Method privat baru `GetApprovedBatchNumberByProfileAsync`: memproyeksikan `WorkforceProfileId`, `BatchNumber`, dan tanggal buat batch, lalu mengelompokkan per pegawai dan mengambil batch paling awal. Dua query `ToDictionaryAsync` yang rapuh di `GetTemplateRowsAsync` dan `ValidateRowsAsync` diganti panggilan ke method itu. Filter (status `Approved`/`Executed`, batch tidak terhapus) sama persis. +24/−14, CRLF dipertahankan |

**Sengaja tidak diubah:** `activePositions` (template) dan `existingPositions` (validasi) juga berupa kamus per pegawai, tetapi
tabel `WfpPositionAssignment` punya index unik tersaring `WorkforceProfileId` (`IsPrimary` ∧ `IsActive` ∧ `¬IsDelete`), dan
filter query-nya lebih sempit dari filter index itu, sehingga kunci ganda tidak mungkin terjadi. Pemeriksaan sekali per
pegawai di `CreateBatchDraftAsync`, `ApproveBatchAsync`, dan `ExecuteBatchAsync` memakai `FirstOrDefaultAsync`, jadi aman.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — route, DTO, kode diagnostik, dan pesan tidak berubah |
| Database | `NOT APPLICABLE` — tanpa perubahan entity, schema, atau migration. Query baca saja, dengan kolom yang lebih sedikit (proyeksi alih-alih `Include`) |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessAction]`/`[AccessPermission]` tidak disentuh |

---

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Initial Placement Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/template` | Daftar pegawai baseline beserta status kelayakan penempatan awal. **Sumber `500` yang diperbaiki** | `HrdInitialPlacementBatch : Read` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/validate` | Validasi baris sebelum dibuat draf (ikut diamankan) | `HrdInitialPlacementBatch : Read` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches` | Buat draf batch; memanggil validasi yang sama (ikut diamankan) | `HrdInitialPlacementBatch : Create` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/validate` | Validasi ulang batch tersimpan (ikut diamankan) | `HrdInitialPlacementBatch : Update` |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute` | Eksekusi batch; validasi ulang pra-eksekusi (ikut diamankan) | `HrdInitialPlacementBatch : Approve` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Debug -p:OutDir=<scratch>` (bin proyek terkunci oleh backend yang sedang berjalan) | 0 error, 616 warning. Warning CS8602 tersisa di berkas ini (baris 81, 566, 568, 834, 835, 914) semuanya kode lama; kode baru tidak menambah warning, dan dua query lama yang memicu CS8602 kini hilang | `PASS` | Keluaran perintah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <service>` | 1 berkas: 0 VIOLATION, 0 REVIEW, 0 INFO | `PASS` | Keluaran checker |
| Review logika: kamus dibangun dari `GroupBy` di memori, satu entri per pegawai | Kunci ganda tidak mungkin lagi; status dan pesan per pegawai sama dengan sebelumnya, nomor batch yang dipakai deterministik (batch paling awal) | `PASS` | Diff |
| Uji ulang `GET /template` pada backend lokal `https://localhost:7184` | Belum dijalankan; backend yang berjalan masih memuat build lama dan perlu di-restart oleh pengguna | `NOT RUN` | — |
| Query baca database untuk memastikan pola duplikat pegawai `49333d61-…` | Belum dijalankan; eksekusi database memerlukan izin terpisah (`CLAUDE.md` backend) | `NOT RUN` | — |

Uji manual: `REQUIRED` — restart backend, buka ulang layar Batch Penempatan Awal, pastikan template tampil dan pegawai
`49333d61-…` berstatus `DILEWATI_SUDAH_DITEMPATKAN`.

**Tidak dijalankan:** automated test. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `GET /template` tidak lagi `500` ketika satu pegawai tercatat di lebih dari satu baris batch `Approved`/`Executed` | Terpenuhi di source; uji runtime belum | Bagian 3.2; review logika `PASS`; uji ulang endpoint `NOT RUN` |
| Validasi, pembuatan draf, validasi ulang, dan eksekusi batch aman dari kunci ganda yang sama | Terpenuhi di source | Bagian 3.2 |
| Perilaku diagnostik (`DILEWATI_SUDAH_DITEMPATKAN`, `ALREADY_IN_APPROVED_INITIAL_BATCH`, pesan) tetap sama | Terpenuhi | Diff: hanya sumber kamus yang berubah; blok penilaian tidak disentuh |
| Build lulus | Terpenuhi | `dotnet build` 0 error |

Belum terpenuhi: uji ulang endpoint di backend yang berjalan. Begitu pengguna me-restart backend dan template tampil,
status task ini dapat dinaikkan ke ✅.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Percobaan build pertama dengan `BaseIntermediateOutputPath` ke scratch gagal (atribut assembly ganda karena `obj/` proyek ikut terbaca sebagai source). Itu kesalahan cara build, bukan kode; build ulang dengan `OutDir` saja lulus. Tidak ada berkas proyek yang tertinggal |
| Masalah yang diketahui | (1) Query sekali per pegawai (di sini, `CreateBatchDraftAsync`, `ApproveBatchAsync`, `ExecuteBatchAsync`) tidak menyaring baris batch yang terhapus lunak (`r.IsDelete`). Tidak diubah karena mengubah cakupan aturan `HRD-DEC-054`; perlu keputusan pemilik. (2) Asal data duplikat pegawai `49333d61-…` belum diketahui: dua batch berbeda (mis. disetujui sebelum penjaga `BE-HRD-412` ada pada 20 November 2026), atau baris terhapus lunak di batch yang sama |
| Risiko tersisa | Rendah. Bila memang ada pegawai di dua batch `Executed`, kemungkinan ia juga punya dua penugasan jabatan dari eksekusi lama. Itu ditangkap index unik jabatan primer aktif, tetapi perlu dicek di data |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs`, laporan ini (baru), `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`. Tidak ada commit |
| Langkah berikutnya | Restart backend lalu uji ulang layar. Bila diizinkan, jalankan query baca untuk melihat baris batch pegawai `49333d61-…` dan memutuskan apakah datanya perlu dirapikan |
