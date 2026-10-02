# Laporan Perubahan Backend — `BE-LAB-23`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-23` |
| Judul | Jumlah pemeriksaan memperbanyak baris |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.6`; `LAB-DEC-038`, BR-33; `AC-52`, `AC-53`, `AC-54`; `VAL-60` |
| Contract version | `LAB-API-v1` `r8`; `LAB-VAL-v1` `r4` `VAL-60` |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-14 |
| Status | **⛔ `DIBATALKAN`.** Nol baris source diubah, dan tidak akan ada. `LAB-DEC-038` **dicabut** `LAB-DEC-050` pada hari yang sama — lihat addendum di bawah |

> ## Addendum — ditutup 2026-09-14
>
> Pemilik modul memilih **pilihan A**: `LAB-DEC-038` dicabut lewat `LAB-DEC-050`, dan **kolom
> Jumlah tidak dibuat sama sekali**. Petugas memilih dua butir katalog berbeda bila memang
> perlu dua pemeriksaan.
>
> **Task ini karena itu dibatalkan, bukan ditunda** — pekerjaannya sudah tidak ada.
>
> Yang ikut dicabut pada hari yang sama:
>
> | Artefak | Perubahan |
> |---|---|
> | `AC-52`, `AC-53`, `AC-54` | Dicabut |
> | `VAL-60` | Dicabut — tidak ada ruas yang perlu divalidasi |
> | `FR-11.6` | Dicabut dari `04-prd-to-mvp.md` |
> | `LAB-API-v1` | Naik ke `r9`; ruas `Quantity` dicabut sebelum sempat dibangun. `POST /lab-examinations` sama persis dengan `r6` |
> | `T-52a`, `T-53a`, `T-54a`, `T-60a`, `T-60b` | Dicabut |
> | `T-52b` | **Dipertahankan** dengan makna yang lebih kuat: Qty tidak pernah ada sama sekali |
> | `T-45a` | **Baru** — menjaga justru aturan keunikan yang membatalkan `LAB-DEC-038` |
> | `UAT-11.2` | Ditulis ulang memakai dua butir katalog, dengan langkah keempat yang membuktikan penolakan `VAL-07` |
>
> `T-45a` sengaja ditambahkan. Tanpa penjaga itu, index unik `(SpecimenId, ProcedureId)` dapat
> dilepas seseorang di kemudian hari tanpa menyadari bahwa sebuah keputusan bergantung padanya.

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | Tidak ada perubahan source |
| QBE ID yang berlaku | `QBE-MOD-001` — sisanya tidak berlaku karena tidak ada source yang disentuh |

---

## 1. Yang diminta task ini

`LAB-DEC-038` dan `BR-33`: kolom **Jumlah/Qty** adalah alat bantu layar, bukan kolom
penyimpanan. Satu permintaan ber-`Quantity` 3 memperbanyak diri menjadi **tiga baris**
`LabExamination`, masing-masing dengan salinan tarif, status, dan tempat hasilnya sendiri.
Nol migration, nol kolom baru.

`AC-52` menegakkannya: *"Mengisi Qty bernilai 3 pada satu pemeriksaan menghasilkan **tiga
baris** pemeriksaan tersimpan, masing-masing dengan salinan tarifnya sendiri."*

---

## 2. Penahan — keputusan ini tidak dapat dilaksanakan

`LabExamination` memiliki **index unik di tingkat database** atas pasangan
`(SpecimenId, ProcedureId)`:

`Repositories/Configurations/HealthServices/LaboratoryManagement/LabExaminationConfiguration.cs`:

```csharp
// BR-20 dan AC-35: satu wadah menopang banyak pemeriksaan, tetapi tidak boleh
// menopang jenis pemeriksaan yang sama dua kali. Keunikan ditegakkan database supaya
// dua permintaan bersamaan tidak dapat sama-sama berhasil...
builder.HasIndex(x => new { x.SpecimenId, x.ProcedureId })
    .IsUnique()
    .HasDatabaseName("IX_LabExamination_SpecimenId_ProcedureId")
    .HasFilter("\"IsDelete\" = false");
```

`LabExaminationService.AddAsync` menegakkannya lebih dulu dengan pesan yang berarti:
*"Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah."*

**Akibatnya `Quantity` 3 untuk satu jenis pemeriksaan pada satu wadah mustahil.** Baris kedua
dan ketiga ditolak — oleh service, dan bila lolos, oleh database.

Komentar pada configuration itu bahkan sudah menjawab pertanyaan ini sebelum ditanyakan:

> *"Pengerjaan ganda tidak melanggar keunikan ini karena duplo adalah penanda pada satu baris —
> `IsDuplo` — bukan dua baris pemeriksaan yang sama."*

### 2.1 Contoh pada `BR-33` sendiri yang keliru

`BR-33` memakai contoh ini untuk menjelaskan kenapa Qty harus memecah diri:

> *"Pasien diminta Glukosa puasa **dan** Glukosa 2 jam setelah makan. Petugas mengetik Qty 2
> pada Glukosa. Yang tersimpan adalah dua baris pemeriksaan."*

Contoh itu tidak berlaku. Pada katalog yang tergolong benar, **Glukosa Puasa dan Glukosa 2 Jam
PP adalah dua `MstProcedure` yang berbeda** — keduanya punya kode, tarif, dan batas nilai
sendiri. Petugas memilih dua butir katalog yang berbeda, dan Qty tidak diperlukan sama sekali.

Yang tersisa adalah `Quantity` > 1 untuk **jenis pemeriksaan yang sama pada wadah yang sama** —
dan justru itulah yang `BR-20` larang dengan sengaja, karena dua baris identik pada satu tabung
tidak punya makna klinis yang dapat dibedakan saat hasilnya diisi.

### 2.2 Kenapa ini tidak diakali

Tiga jalan keluar teknis dipertimbangkan lalu ditolak:

| Jalan keluar | Kenapa ditolak |
| --- | --- |
| Melonggarkan index unik | Mencabut `BR-20` dan `AC-35` yang disetujui 2026-09-01. Keputusan bisnis, bukan wewenang task ini |
| Menambah kolom pembeda, misalnya nomor urut | Menambah kolom pada `LabExamination` — padahal `LAB-DEC-038` justru menjanjikan nol kolom baru |
| Memperbanyak baris ke wadah berbeda | Mengubah arti Qty menjadi "beberapa tabung", yang tidak pernah diputuskan siapa pun |

---

## 3. Pertanyaan yang perlu dijawab pemilik modul

Dicatat sebagai `LAB-CONFLICT-005`. Tiga kemungkinan:

| Pilihan | Akibat |
| --- | --- |
| **A. `LAB-DEC-038` dicabut; kolom Qty tidak dibuat** | Layar penerimaan tidak punya kolom Jumlah. Petugas memilih dua butir katalog berbeda bila memang perlu dua pemeriksaan. **Tidak menyentuh satu pun keputusan lain**, dan sesuai dengan cara katalog laboratorium memang disusun |
| **B. Qty berarti jumlah tabung, bukan jumlah pemeriksaan** | Qty memperbanyak **wadah**, bukan baris pemeriksaan. `BR-20` utuh. Tetapi ini kemampuan yang berbeda dari yang diminta `LAB-EVD-001`, dan perlu diperiksa ulang apakah itu memang yang dimaksud petugas |
| **C. `BR-20` dan `AC-35` dicabut** | Index unik dilepas, dua baris identik diizinkan. Menyentuh keputusan yang disetujui 2026-09-01 beserta migration yang sudah berjalan, dan membuka pertanyaan baru: saat hasil diisi, baris mana yang mana |

Rekomendasi pelapor: **A**. Ia satu-satunya yang tidak mencabut keputusan lama maupun mengarang
kemampuan baru — dan dugaan terkuatnya, kolom Jumlah pada `LAB-EVD-001` adalah bawaan tampilan
layar transaksi umum, bukan kebutuhan laboratorium yang sebenarnya.

**Yang perlu ditanyakan langsung kepada petugas lab** sebelum memilih: ketika mereka mengetik
Jumlah 2 pada satu pemeriksaan di sistem lama, apa yang sebenarnya mereka maksudkan — dua
tabung, dua pemeriksaan berbeda yang namanya mirip, atau satu pemeriksaan yang ditagih dua kali?

---

## 4. Yang terbukti pada pemeriksaan ini

| AC | Hasil |
| --- | --- |
| `AC-52` | **Tidak dapat dipenuhi.** Tiga baris untuk satu jenis pemeriksaan pada satu wadah ditolak index unik |
| `AC-53` | **Tidak dapat diuji** — bergantung pada `AC-52` |
| `AC-54` | **Tidak dapat diuji** — bergantung pada `AC-52` |

Satu butir DoD tetap terbukti tanpa perubahan apa pun:

| Butir DoD | Status |
| --- | --- |
| Tidak ada properti `Qty` atau `Quantity` pada model Laboratorium mana pun | **Terpenuhi** — dan memang tidak pernah ada |
| `IsDuplo` tetap ada dan tidak digantikan | **Terpenuhi** |
| Konsumen lama tidak berubah perilakunya | **Terpenuhi secara hampa** — tidak ada yang diubah |

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Jenis | Hasil |
| --- | --- |
| Backend Governance Preflight | **Lulus** |
| Review diff/scope | **Lulus — nol baris source diubah** |
| `dotnet build` | **Tidak dijalankan.** Tidak ada source yang disentuh |
| Verifikasi kontrak API | **Tidak dilaksanakan.** `LAB-API-v1` `r8` mencantumkan ruas `Quantity` yang kini terbukti tidak dapat dilaksanakan; kontraknya perlu ditinjau ulang bersama keputusan `LAB-CONFLICT-005` |
| Verifikasi proses bisnis | **Terlaksana sebagai pemeriksaan**, dan hasilnya menemukan penahan |

---

## 6. Catatan penutup

Ini penahan **kedua** pada hari yang sama, dan keduanya berakar pada satu hal yang sama.

`BE-LAB-24` tertahan karena `AC-55`/`AC-57` bertentangan dengan `LAB-INH-006` dan `LAB-INH-010`.
`BE-LAB-23` tertahan karena `AC-52` bertentangan dengan `BR-20` dan `AC-35`.

Keduanya adalah acceptance criteria yang ditulis amandemen 2026-09-14 dari `LAB-EVD-001`, dan
keduanya lolos sampai tahap implementasi tanpa pernah diadu dengan model data yang sudah
dikunci `LAB-DEC-024` pada 2026-09-01. Artifact lapangan menggambarkan satu layar sebagaimana
dipahami penggunanya; ia tidak tahu bahwa wadah dan pemeriksaan sudah dipisahkan, dan bahwa
pemisahan itu membawa aturan keunikan.

**Yang perlu diperbaiki bukan hanya dua task ini, melainkan caranya:** amandemen berikutnya yang
berangkat dari artifact lapangan perlu satu langkah tambahan — mengadu setiap acceptance
criteria baru dengan ERD dan index yang sudah berjalan, sebelum ia disetujui, bukan sesudah
implementasinya dicoba.

**Yang perlu dikerjakan berikutnya:** jawaban atas `LAB-CONFLICT-005`, dan sebaiknya bersamaan
dengan `LAB-CONFLICT-004` karena keduanya menyentuh `BR-33`, `BR-34`, dan bentuk kolom pada
layar penerimaan yang sama.
