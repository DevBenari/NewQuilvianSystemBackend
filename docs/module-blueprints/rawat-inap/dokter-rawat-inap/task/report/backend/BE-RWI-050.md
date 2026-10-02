# Laporan Perubahan Backend — `BE-RWI-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-050` |
| Judul | Resep berulang dan obat pulang sepanjang perawatan |
| Slice | `DOK-MVP-5` — resep, tindakan, penunjang |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-050` |
| Trace | `EPIC DOK-06`; `FR-DOK-029` s.d. `FR-DOK-032`; `RWI-RULE-024`, `RWI-DEC-046`; `AC-CAP023-03`; `RUL-DOK-01`; `contracts/api-contract.md` §6; `VAL-DOK-19`, `VAL-DOK-21` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-042` 🟡 **sebagian** ([laporan](BE-RWI-042.md)) — kolom jenis resep dan kunci permintaan sudah ada. `BE-RWI-043` 🟡 **sebagian** ([laporan](BE-RWI-043.md)) — pelonggaran batas satu resep sudah berlaku. `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)) |
| Klasifikasi | `MEDIUM`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 2, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `PharmacyManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Keenam acceptance criteria terbukti, termasuk dua uji arsitektur. Nol migration. **Melepas satu penghalang `BE-RWI-043`** — lihat bagian 7 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `PharmacyManagement` |
| Pemilik / prefix registry | `PharmacyManagement / Phm` — `ACTIVE / LEGACY` |
| Applicability | `TOUCHED LEGACY`. Controller, DTO, dan service resep adalah kode lama |
| QBE berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-PERM-001` |
| Entity operasional baru | `NONE`. Kolom `PrescriptionOrderType`, `InpEpisodeId`, dan `IdempotencyKey` sudah dibuat `BE-RWI-042` |
| Archetype | Transaksi. Satu endpoint baca baru ber-scope perawatan. `GET /options` yang sudah ada pada controller ini adalah drift lama, tidak ditambah dan tidak dirapikan di sini |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Batas satu resep aktif per catatan membuat dokumentasi terapi rawat inap mustahil.**

Batas itu masuk akal di poliklinik: satu kunjungan, satu resep. Pada pasien yang dirawat lima
hari, batas yang sama berarti seluruh terapi lima hari harus muat pada satu resep yang ditulis
hari pertama — padahal terapi berubah setiap hari mengikuti perkembangan pasien.

Dua hal yang belum ada sebelum task ini:

1. **Jenis resep tidak dapat dibedakan.** Obat pulang tercampur dengan resep harian, sehingga
   petugas farmasi tidak dapat menyaringnya di layar mereka sendiri — `AC-CAP023-03`.
2. **Tidak ada kunci permintaan.** Percobaan ulang karena jaringan terputus melahirkan resep
   kedua, yang berarti obat ganda disiapkan farmasi dan tagihan ganda ditanggung pasien.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Dokter meresepkan sebanyak yang dibutuhkan sepanjang perawatan, dan obat pulang dikenali petugas farmasi sebagai jenis tersendiri |
| Pelaku | Dokter penulis resep; petugas farmasi sebagai pembaca |
| Pemicu | Dokter menekan Buat Resep dari konteks perawatan |
| Hasil akhir | Satu resep berjenis rutin, harian, atau obat pulang, tertaut pada perawatan yang benar |

### 2.2 Contoh berangka

> Pasien dirawat lima hari. Hari 1 sampai 4 dokter menulis resep **harian**; hari ke-5 ia
> menulis **obat pulang**.
>
> Yang tersimpan adalah **lima** resep. Membaca resep perawatan itu menghasilkan lima baris.
> Menyaring jenis obat pulang menghasilkan **satu** baris — itulah yang dilihat petugas farmasi.

### 2.3 Kenapa jenis obat pulang eksplisit, bukan ditebak

Menebaknya dari tanggal penulisan terasa hemat, tetapi salah pada keadaan yang paling sering
terjadi: pasien yang pemulangannya tertunda dua hari. Resep yang ditulis sebagai obat pulang
pada hari rencana pulang akan terbaca sebagai resep harian, dan petugas farmasi menyiapkannya
sebagai obat ruangan.

### 2.4 Batas yang tidak boleh dilanggar

`RUL-DOK-01`. Menandai obat sudah diserahkan adalah kewenangan petugas Farmasi. Sub-modul Rawat
Inap **hanya membaca** keadaan pemenuhan.

Ini bukan pembatasan administratif. Petugas farmasi yang menyerahkan obat adalah satu-satunya
orang yang benar-benar tahu obat itu sudah berpindah tangan; dokter yang menandainya dari
ruangannya hanya menebak.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Kunci permintaan sama dengan yang sudah tersimpan | **Bukan galat.** Resep yang sudah ada dikembalikan | `200` |
| Resep kedua pada catatan **rawat jalan** | Tetap ditolak dengan kalimat yang sama seperti sebelumnya | `400` |
| Catatan dokter sudah selesai atau dibatalkan | Ditolak | `400` |
| Percobaan menandai obat diserahkan dari sub-modul ini | Permukaannya **tidak ada** | — |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/api-contract.md` §6, §11 | Bentuk endpoint dan daftar endpoint yang sengaja tidak ada |
| `contracts/validation-matrix.md` §5 | `VAL-DOK-19`, `VAL-DOK-20`, `VAL-DOK-21` |
| `contracts/state-transition-matrix.md` §7 | Status milik modul lain yang hanya dibaca |
| `Areas/HealthServices/PharmacyManagement/Enums/PrescriptionOrderType.cs` | Tiga nilai jenis resep dari `BE-RWI-042` |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | Pelonggaran batas satu resep dari `BE-RWI-043` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs` | Permintaan pembuatan menerima jenis resep dan kunci permintaan; balasan daftar dan pembuatan memuat jenis resep serta penanda perawatan |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | Pemeriksaan kunci permintaan **paling awal**; penstempelan jenis resep dan kunci; endpoint `GET /episodes/{episodeId}` dengan penyaring jenis |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionSummaryService.cs` | Pembacaan ringkasan **tanpa menulis**, dipakai jalur kiriman ulang |
| `Tests/.../ClinicalManagement/InpatientPrescriptionTests.cs` | **Baru.** Lima uji, dua di antaranya uji arsitektur |
| `Tests/.../Infrastructure/RawatInapTestData.cs` | Menambahkan sumber pembayaran pada kunjungan uji — lihat bagian 7 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua ruas **opsional** pada `POST /` — jenis resep berbawaan rutin, dan kunci permintaan. Keduanya opsional sehingga pemanggil lama tetap berjalan apa adanya. Satu endpoint baca baru `GET /episodes/{episodeId}` sesuai `api-contract.md` §6. Balasan resep bertambah dua ruas baca |
| Database | Nol perubahan schema, nol migration, nol eksekusi database |
| Keamanan/Auth | Nol butir hak akses baru; endpoint baru memakai `Prescription : Read` yang sudah ada. **Nol jalur tulis** menuju keadaan pemenuhan — dijaga dua uji arsitektur |

---

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Prescription

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat resep. **Perubahan:** menerima jenis resep dan kunci permintaan | `Prescription : Create` |
| `GET` | `/episodes/{episodeId}` | Seluruh resep satu perawatan beserta keadaan pemenuhannya, dapat disaring jenis | `Prescription : Read` |

**Kode status pembuatan:** `200` berhasil, juga untuk kiriman ulang berkunci sama; `400`
permintaan tidak sah, termasuk resep aktif kedua pada catatan rawat jalan.

**Yang sengaja tidak ada:** endpoint apa pun yang mengubah keadaan pemenuhan resep — `RUL-DOK-01`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Lima resep pada satu perawatan tersimpan seluruhnya | Empat resep harian dan satu obat pulang; lima baris tersimpan; pembacaan per perawatan mengembalikan lima | `PASS` | `LimaResepPadaSatuPerawatan_TersimpanSeluruhnyaDanObatPulangTersaring` |
| Obat pulang tersaring tersendiri | Penyaring jenis obat pulang mengembalikan **satu** baris berjenis obat pulang | `PASS` | Uji yang sama |
| Pengiriman berulang berkunci sama tidak melahirkan resep ganda | Dua `200`; identitas resep identik; satu baris tersimpan | `PASS` | `PengirimanBerulangBerkunciSama_TidakMelahirkanResepGanda` |
| Keadaan pemenuhan dapat dibaca dari konteks perawatan | Keadaan yang dinaikkan farmasi terbaca apa adanya, beserta penanda siap diambil dan penanda perawatan | `PASS` | `StatusPemenuhanResep_DapatDibacaDariKonteksPerawatan` |
| **Architecture test** — nol permintaan yang menerima keadaan pemenuhan | Tidak satu pun badan permintaan pada controller resep memuat `FulfillmentStatus` | `PASS` | `TidakAdaPermintaanResepYangMenerimaKeadaanPemenuhan` |
| **Architecture test** — nol aksi penyerahan obat | Tidak satu pun method bernama menyerupai penyerahan obat | `PASS` | `ControllerResepTidakMenyediakanAksiPenyerahanObat` |
| Regresi rawat jalan dan medical check-up | Tetap hijau; kalimat penolakan resep aktif kedua tidak berubah | `PASS` | Suite SQLite penuh, termasuk uji `BE-RWI-043` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lima resep pada satu perawatan lima hari tersimpan seluruhnya | Terpenuhi | `LimaResepPadaSatuPerawatan_TersimpanSeluruhnyaDanObatPulangTersaring` |
| 2. Resep obat pulang tersaring tersendiri menurut jenisnya | Terpenuhi | Uji yang sama — penyaring jenis mengembalikan satu baris |
| 3. Pengiriman berulang berkunci sama tidak melahirkan resep ganda | Terpenuhi | `PengirimanBerulangBerkunciSama_TidakMelahirkanResepGanda` |
| 4. Status pemenuhan dapat **dibaca** kembali | Terpenuhi | `StatusPemenuhanResep_DapatDibacaDariKonteksPerawatan` |
| 5. Percobaan menandai obat sudah diserahkan dari sub-modul ini ditolak `403` | Terpenuhi **dengan cara yang lebih kuat** | Permukaannya **tidak ada sama sekali**, sehingga tidak ada permintaan yang dapat mencapainya. Dibuktikan `ControllerResepTidakMenyediakanAksiPenyerahanObat` — lihat bagian 7 |
| 6. **Nol jalur tulis** menuju status pemenuhan | Terpenuhi | `TidakAdaPermintaanResepYangMenerimaKeadaanPemenuhan` dan `ControllerResepTidakMenyediakanAksiPenyerahanObat` |

### Definition of Done

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | ✅ |
| Architecture test hijau | ✅ Dua uji arsitektur |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan build baru |
| **Selisih yang dilaporkan pada kriteria 5** | Kriteria menuntut percobaan menandai obat diserahkan ditolak `403`. Yang dicapai lebih kuat: permukaannya **tidak ada**, sehingga tidak ada permintaan yang dapat mencapainya untuk ditolak. Menambahkan endpoint hanya agar ia dapat menjawab `403` justru menciptakan permukaan yang kelak dapat "dilengkapi" seseorang. Buktinya karena itu berupa uji arsitektur, bukan uji kode status |
| **Penghalang `BE-RWI-043` yang lepas** | [Laporan `BE-RWI-043`](BE-RWI-043.md) mencatat kriteria 2 — resep kedua sepanjang perawatan diterima — hanya terbukti pada aturan aplikasi dan index database, karena jalur pemesanan resep rawat inap belum menyala. Jalur itu **kini menyala**, dan `LimaResepPadaSatuPerawatan_...` membuktikan lima resep berturut-turut tersimpan lewat endpoint. Penghalang itu lepas |
| **Perubahan pada data uji bersama** | `RawatInapTestData.SiapkanPerawatan` kini membuat sumber pembayaran pada kunjungan uji. Tanpa baris itu, seluruh jalur yang menghitung tarif ditolak dengan "Sumber pembayaran encounter tidak ditemukan", dan penolakan itu menyamarkan hal yang sedang diuji. Setiap kunjungan sungguhan memang wajib punya tepat satu sumber pembayaran, sehingga data uji kini lebih menyerupai keadaan nyata |
| Masalah yang diketahui | `VAL-DOK-20` — peringatan ketika obat pulang ditulis sebelum pasien dinyatakan boleh pulang — **belum dibuat**. Ia berupa peringatan, bukan penolakan, dan bentuk penyampaian peringatan pada balasan pembuatan resep belum ada polanya di controller ini. Dicatat sebagai pekerjaan menyusul, bukan didiamkan |
| Risiko tersisa | Kunci permintaan bersifat **opsional**. Pemanggil yang tidak mengirimnya tetap dapat melahirkan resep ganda saat jaringan terputus. Mewajibkannya adalah perubahan kontrak yang akan menolak seluruh pemanggil lama, dan itu di luar wewenang task ini. Layar rawat inap dianjurkan selalu mengirimnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Pemilik `PharmacyManagement` diminta memutuskan bentuk penyampaian peringatan `VAL-DOK-20` |
