# Laporan Perubahan Backend — `BE-RWI-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-052` |
| Judul | Pemeriksaan laboratorium dan radiologi dipesan dan hasilnya dibaca |
| Slice | `DOK-MVP-5` — resep, tindakan, penunjang |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-052` |
| Trace | `CAP-015`; `FR-DOK-035`, `FR-DOK-036`, `FR-DOK-042`, `FR-DOK-043`; `INV-DOK-12`; `AC-CAP015-01`, `AC-CAP015-02`; `RUL-DOK-02`; `contracts/api-contract.md` §7, §8; `VAL-DOK-22`, `VAL-DOK-30`, `VAL-DOK-31` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-042` 🟡 **sebagian** ([laporan](BE-RWI-042.md)) — kolom penanda perawatan pada kedua pesanan sudah ada. `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)) |
| Klasifikasi | `MEDIUM`, skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `LaboratoryManagement` dan `RadiologyManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9be5526d248d9813a4044f063e43066a2364dd7d` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Keenam acceptance criteria terbukti, termasuk dua uji arsitektur. Nol migration. Utang registry `Rad` **masih terbuka** — lihat bagian 7 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` dan `RadiologyManagement` |
| Pemilik / prefix registry | `LaboratoryManagement / Lab` — `ACTIVE`; `RadiologyManagement / Rad` — **`PLANNED`** |
| Applicability | `TOUCHED LEGACY`. Controller, DTO, dan service pesanan pada kedua modul adalah kode lama |
| QBE berlaku | `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-SVC-001`, `QBE-PERM-001` |
| Entity operasional baru | `NONE`. Kolom `InpEpisodeId` pada kedua pesanan sudah dibuat `BE-RWI-042`. **`QBE-MOD-002` tidak terpicu**: task ini tidak membuat entity operasional baru pada `RadiologyManagement` |
| Archetype | Transaksi. Satu endpoint baca baru per modul, ber-scope perawatan |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Pesanan penunjang rawat inap tidak dapat dibuktikan miliknya perawatan mana, dan hasilnya
tidak dapat dibaca per perawatan.**

Temuan pada `api-contract.md` §7 menyatakannya apa adanya: pesanan laboratorium terikat pada
kunjungan saja — tanpa antrean dan tanpa catatan dokter. Pemesanan lab rawat inap karena itu
**tidak tertahan gerbang mana pun**; yang kurang adalah penanda perawatan dan pembacaannya.

Akibatnya bagi pasien yang dirawat dua kali dalam sebulan:

> Perawatan Januari dan perawatan Februari berbagi satu nomor rekam medis. Tanpa penanda
> perawatan, layar dokter yang membuka perawatan Februari akan menampilkan pesanan dan hasil
> Januari bercampur di dalamnya. Dokter membaca hasil laboratorium bulan lalu sebagai hasil
> hari ini — dan `INV-DOK-12` menyebutnya risiko tertinggi pada scope ini.

---

## 2. Proses bisnis

### 2.1 Kenapa nol tabel salinan hasil

`RUL-DOK-02` melarang Rawat Inap menyimpan salinan hasil. Alasannya bukan kerapian data.

> Hasil laboratorium dapat **direvisi** pemiliknya. Salinan yang tidak ikut berubah menjadi
> angka basi di layar dokter, dan angka basi itu tetap terlihat sah. Dokter yang mengambil
> keputusan klinis dari angka basi adalah risiko keselamatan pasien, bukan masalah tampilan.

Karena itu yang dibaca adalah baris milik Laboratorium dan Radiologi **apa adanya**. Rawat Inap
tidak menyimpan satu baris hasil pun.

### 2.2 Kenapa hasil belum final wajib ditandai

`VAL-DOK-30`. Pesanan yang belum selesai bukan hasil, dan menampilkannya tanpa penanda membuat
dokter membaca angka sementara sebagai angka final.

| Modul | Hasil dinyatakan final ketika |
| --- | --- |
| Laboratorium | Pesanan berstatus selesai **dan** tidak dibatalkan |
| Radiologi | Pesanan berstatus selesai, tidak dibatalkan, **dan** ada sedikitnya satu study yang mutunya diterima |

Radiologi menuntut syarat ketiga karena pesanan yang selesai tetapi seluruh study-nya belum
lolos mutu bukan hasil sah: gambarnya justru akan diulang.

Setiap baris membawa kalimat siap tampil, sehingga layar tidak perlu menerjemahkan nama status:

| Keadaan | Kalimat |
| --- | --- |
| Final | "Hasil sudah final." |
| Belum final | "Hasil belum final. Jangan dipakai sebagai dasar keputusan klinis." |
| Dibatalkan | "Pesanan dibatalkan; tidak ada hasil." |

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Penanda perawatan tidak cocok dengan kunjungannya | "Pesanan ini tidak cocok dengan perawatan pasien." | `400` |
| Penanda perawatan tidak dikirim | **Diterima.** Pesanan poliklinik dan IGD memang tidak punya perawatan rawat inap | — |
| Percobaan menulis hasil dari sub-modul ini | Permukaannya **tidak ada** | — |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria dan DoD |
| `contracts/api-contract.md` §7, §8, §11 | Bentuk endpoint dan daftar endpoint yang sengaja tidak ada |
| `contracts/validation-matrix.md` §5, §6 | `VAL-DOK-22`, `VAL-DOK-23`, `VAL-DOK-30`, `VAL-DOK-31` |
| `contracts/state-transition-matrix.md` §7 | Status milik modul lain yang hanya dibaca |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`, `RadiologyManagement/Models/RadOrder.cs` | Kolom penanda perawatan dari `BE-RWI-042` |
| `Areas/HealthServices/RadiologyManagement/Models/RadStudy.cs` | Keadaan mutu study sebagai syarat hasil final |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | Permintaan pembuatan menerima penanda perawatan; baris daftar memuat penanda perawatan, penanda hasil final, dan kalimat ketersediaan hasil |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Penjaga kecocokan perawatan; penstempelan penanda; pembacaan per perawatan; proyeksi daftar bersama yang menurunkan penanda hasil final |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Endpoint `GET /episodes/{episodeId}` |
| `Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs` | Hal yang sama untuk radiologi |
| `Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs` | Hal yang sama, dengan syarat study lolos mutu |
| `Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs` | Endpoint `GET /episodes/{episodeId}` |
| `Tests/.../ClinicalManagement/InpatientSupportingOrderTests.cs` | **Baru.** Enam uji, dua di antaranya uji arsitektur |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu ruas **opsional** pada masing-masing `POST /` sesuai `api-contract.md` §7 dan §8. Satu endpoint baca baru per modul. Baris daftar bertambah tiga ruas baca; pemanggil lama tidak terpengaruh karena tidak ada ruas yang dihapus maupun diganti nama |
| Database | Nol perubahan schema, nol migration, nol eksekusi database |
| Keamanan/Auth | Nol butir hak akses baru; endpoint baru memakai `LabOrder : Read` dan `RadOrder : Read` yang sudah ada. **Nol jalur tulis hasil** dari sub-modul ini — dijaga uji arsitektur |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan laboratorium. **Perubahan:** menerima penanda perawatan | `LabOrder : Create` |
| `GET` | `/episodes/{episodeId}` | Pesanan dan ketersediaan hasil satu perawatan | `LabOrder : Read` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan radiologi. **Perubahan:** menerima penanda perawatan | `RadOrder : Create` |
| `GET` | `/episodes/{episodeId}` | Pesanan, study, dan ketersediaan hasil satu perawatan | `RadOrder : Read` |

**Yang sengaja tidak ada:** endpoint apa pun yang menulis hasil laboratorium maupun radiologi —
`RUL-DOK-02`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)`, `185 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet test` project uji SQLite | `Failed: 0, Passed: 320` | `PASS` | Keluaran perintah |
| Pesanan laboratorium perawatan lain ditolak | `400`, pesan memuat "tidak cocok dengan perawatan"; nol pesanan tersimpan | `PASS` | `PesananLaboratoriumPerawatanLain_Ditolak400` |
| Pesanan radiologi perawatan lain ditolak | `400`, pesan sama; nol pesanan tersimpan | `PASS` | `PesananRadiologiPerawatanLain_Ditolak400` |
| Hasil laboratorium terbaca per perawatan beserta penanda belum final | Dua baris milik perawatan yang dibuka; pesanan perawatan lain **tidak ikut tampil**; baris selesai bertanda final; baris berjalan bertanda belum final beserta kalimat larangan pemakaian | `PASS` | `HasilLaboratorium_TerbacaPerPerawatanBesertaPenandaBelumFinal` |
| Hasil radiologi final hanya ketika study lolos mutu | Pesanan selesai tanpa study belum final; setelah study lolos mutu ditambahkan, barulah final | `PASS` | `HasilRadiologi_FinalHanyaKetikaStudyLolosMutu` |
| **Architecture test** — nol tabel salinan hasil | Nol tipe bernama hasil penunjang di bawah `ClinicalManagement` maupun `InPatientManagement` | `PASS` | `RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang` |
| **Architecture test** — permukaan pemesanan tidak menerima isi hasil | Nol permintaan pembuatan pada kedua controller yang memuat ruas isi hasil | `PASS` | `PermintaanPemesananPenunjangTidakMenerimaIsiHasil` |
| `dotnet test` project uji InMemory | `Failed: 1, Passed: 908` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, berkas tidak disentuh task ini |
| `dotnet test` project uji PostgreSQL | `Failed: 54, Passed: 34` | `EXISTING / ENVIRONMENT ISSUE` | Satu sebab: `BLOCKED_BY_TEST_DB_CONFIGURATION` |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** migration dan perintah basis data apa pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pesanan laboratorium perawatan A tidak dapat diproses sebagai milik perawatan B — ditolak `400` | Terpenuhi | `PesananLaboratoriumPerawatanLain_Ditolak400` |
| 2. Hal yang sama berlaku untuk pesanan radiologi | Terpenuhi | `PesananRadiologiPerawatanLain_Ditolak400` |
| 3. Hasil final terbaca dari konteks pasien **tanpa tabel salinan** | Terpenuhi | `HasilLaboratorium_TerbacaPerPerawatanBesertaPenandaBelumFinal` dan `RawatInapTidakMemilikiSatuPunTabelSalinanHasilPenunjang` |
| 4. Hasil yang belum final ditampilkan dengan penanda dan tidak disajikan sebagai hasil sah | Terpenuhi | Uji yang sama; kalimatnya memuat larangan pemakaian sebagai dasar keputusan klinis |
| 5. Hasil milik kunjungan di luar perawatan yang dibuka **tidak ikut tampil** | Terpenuhi | Uji yang sama — pesanan perawatan lain tidak muncul |
| 6. Percobaan menulis hasil dari sub-modul ini ditolak `403` | Terpenuhi **dengan cara yang lebih kuat** | Permukaannya **tidak ada**; dibuktikan `PermintaanPemesananPenunjangTidakMenerimaIsiHasil` — lihat bagian 7 |

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
| **Selisih yang dilaporkan pada kriteria 6** | Kriteria menuntut percobaan menulis hasil ditolak `403`. Yang dicapai lebih kuat: permukaannya **tidak ada** pada permukaan pemesanan, sehingga tidak ada permintaan yang dapat mencapainya untuk ditolak. Menambahkan endpoint hanya agar ia dapat menjawab `403` justru menciptakan permukaan yang kelak dapat "dilengkapi" seseorang |
| **Utang registry yang masih terbuka** | Baris registry `RadiologyManagement / Rad` masih berstatus `PLANNED` padahal entity-nya sudah ada dan berjalan. Task ini **tidak terhalang** `QBE-MOD-002` karena tidak membuat entity operasional baru di sana — hanya menambah ruas DTO dan endpoint baca. Utangnya sama dengan yang tercatat pada [laporan `BE-RWI-042`](BE-RWI-042.md) dan tetap milik pemilik registry |
| **Batas pembacaan hasil yang perlu diketahui** | Modul Laboratorium dan Radiologi hari ini **tidak memiliki tabel nilai hasil** dengan penanda final tersendiri. Yang tersedia adalah status pesanan dan status mutu study. Penanda hasil final karena itu diturunkan dari keduanya, bukan dari kolom "hasil sudah diverifikasi". Bila kedua modul kelak menambahkan tabel hasil beserta verifikasinya, penurunan ini perlu diperbarui agar tetap menjawab pertanyaan yang sama |
| `VAL-DOK-31` | Terpenuhi lewat penyaringan penanda perawatan pada endpoint baca. Hasil milik kunjungan di luar perawatan yang dibuka tidak ikut tampil karena penanda perawatannya berbeda |
| Masalah yang diketahui | Pesanan yang dibuat **sebelum** task ini tidak memiliki penanda perawatan, sehingga tidak muncul pada pembacaan per perawatan. Pengisian penanda untuk baris lama adalah pekerjaan tersendiri milik kedua modul pemilik, dan memerlukan wewenang database yang terpisah |
| Risiko tersisa | Penjaga kecocokan hanya berlaku ketika penanda perawatan **dikirim**. Pemanggil yang tidak mengirimnya tetap dilayani, dan pesanannya tidak akan muncul pada layar per perawatan. Layar rawat inap dianjurkan selalu mengirimnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Bersih sebelum task; tidak ada stage, commit, maupun push |
| Langkah berikutnya | Pemilik registry menaikkan baris `RadiologyManagement / Rad` menjadi `ACTIVE`; pemilik kedua modul memutuskan pengisian penanda perawatan untuk pesanan lama |
