# Laporan Perubahan Backend — `BE-LAB-49`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-49` (backend) |
| Judul | Pembuktian index parsial: hapus lalu pilih ulang — gelombang `MVP-6c`, slice `S4b` |
| Trace | `INV-27`; `LAB-CONFLICT-005` sebagai pelajaran |
| Kontrak | `LAB-VAL-v1` `r7` (`VAL-87`) |
| Klasifikasi | `VERIFIKASI` — **nol source baru, nol migration, nol berkas kode disentuh** |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | `BE-LAB-47` ✅, `BE-LAB-48` ✅ |
| Status | ⚠ **`SELESAI DENGAN TEMUAN`** — `AC-115` **terbukti separuh**. Separuh lainnya **terbantah**, dan itu temuan nyata, bukan kegagalan pengujian |

---

## 1. Kenapa task ini baru dikerjakan sekarang

Ia ditemukan **oleh audit kesiapan**, bukan oleh perencanaan. Perhitungan gelombang `MVP-7`
dan `MVP-7b` menyebut *"tiga belas dari tiga belas selesai"* dan **nol memuat `BE-LAB-49`**,
yang hidup di gelombang `MVP-6c` dengan dependency `BE-LAB-47` serta `BE-LAB-48` — keduanya
baru selesai 2026-09-21.

Roadmap sendiri sudah memperingatkannya: task ini dipisahkan **justru supaya pembuktiannya
tidak ikut tertelan anggapan _"migration sudah jalan, berarti beres"_**. Ia kemudian tertelan
anggapan yang berbeda — perhitungan gelombang yang tidak memuatnya.

---

## 2. Yang diuji, dan hasilnya

Seluruhnya panggilan sungguhan terhadap `QuilvianNewDevYoga`, pemeriksaan
`6fa8a2b6-…` berisi satu isolat *Branhamella catarrhalis*.

| Langkah | Tindakan | Hasil |
|---|---|---|
| 1 | Keadaan awal | 1 baris kepekaan, `Ampicillin`, zona `10`, `Resistant`, id `0b41ac43-…` |
| 2 | `PUT` ulang **tanpa** baris Ampicillin | `200` |
| 3 | Periksa baris lama di database | **HILANG SECARA FISIK** — `0` baris tersisa, `0` bertanda terhapus |
| 4 | `PUT` ulang **dengan antibiotik yang SAMA** | **`200`** ✅ id baru `72abe762-…`, zona `14` → `Intermediate` |
| 5 | Bonus `VAL-87`: antibiotik sama **dua kali dalam satu kiriman** | **`422`** ✅ *"Antibiotik ini sudah diuji pada kuman tersebut."* |

### `AC-115` punya dua bagian, dan keduanya berbeda nasib

> *"urutan **simpan → hapus baris → simpan lagi dengan antibiotik yang sama** berhasil, dan
> baris lama tetap ada bertanda `IsDelete = true`"*

| Bagian | Hasil |
|---|---|
| Pilih ulang antibiotik yang sama **berhasil** | ✅ **TERBUKTI** — langkah 4 |
| Baris lama **tetap ada** bertanda `IsDelete = true` | ⛔ **TERBANTAH** — langkah 3 |

Outcome yang benar-benar diminta task ini — *"analis dapat menghapus satu baris kepekaan lalu
memilih antibiotik yang sama lagi tanpa ditolak basis data"* — **terbukti**. Yang terbantah
adalah **mekanisme yang diasumsikan** menghasilkannya.

---

## 3. `LAB-CONFLICT-011` — premis index bertentangan dengan kode

`LabIsolateSusceptibilityConfiguration` menulis alasan filternya apa adanya:

```csharp
// Index ini WAJIB parsial: penghapusan berupa penandaan (IsDelete), sehingga index unik
// penuh akan menolak baris baru yang antibiotiknya sama dengan baris yang sudah dihapus.
builder.HasIndex(x => new { x.LabMicrobiologyIsolateId, x.LabAntibioticId })
    .IsUnique()
    .HasFilter("\"IsDelete\" = false");
```

**Premis itu tidak berlaku bagi tabel ini.** `LabMicrobiologyResultService` baris 165–166
memakai `RemoveRange` dan `Remove`, dan **nol tempat di codebase mengubah `EntityState.Deleted`
menjadi penandaan**:

| Yang dicari | Hasil |
|---|---|
| `EntityState.Deleted` di seluruh source | **nol** |
| `HasQueryFilter` global | **nol** |
| Interseptor `SaveChanges` | **nol** |

Sehingga `Remove` benar-benar menghasilkan `DELETE` SQL. Akibatnya **filter parsial pada index
itu tidak pernah teruji** — index unik penuh akan berperilaku persis sama pada jalur ini.

### Modul ini memang memakai dua gaya hapus, dan keduanya disengaja

| Gaya | Dipakai oleh | Untuk apa |
|---|---|---|
| Penandaan `IsDelete = true` | `LabPathologyCategoryService`, `LabPathologyReportService` | Data induk dan dokumen |
| `Remove` / `RemoveRange` fisik | `LabMicrobiologyResultService`, `LabSpecimenCorrectionService`, `LabValueBoundService` | **Koleksi anak yang diganti utuh** |

Jadi hard delete di jalur hasil **konsisten** dengan dua tempat lain yang polanya sama. Yang
tidak konsisten adalah **komentar index dan `AC-115`**, yang keduanya menganggap seluruh
penghapusan berupa penandaan.

### Dua jalan keluar, dan keduanya bukan wewenang task ini

| Jalan | Akibatnya |
|---|---|
| **A. Kode diubah menjadi penandaan** | `AC-115` lolos utuh dan filter parsial menjadi bermakna. Tetapi setiap penyimpanan ulang antibiogram akan meninggalkan satu generasi baris mati — antibiogram 22 baris yang disunting lima kali meninggalkan 110 baris, dan nol di antaranya pernah dibaca |
| **B. `AC-115` dan komentar index diperbaiki** | Mengakui bahwa koleksi anak memang diganti utuh, dan `LAB-DEC-112` sudah menempatkan jejak perubahan pada `LabFieldChangeLog` — sumbu yang berbeda. Filter parsial tetap dipertahankan sebagai jaring pengaman bila kelak gaya hapusnya berubah |

> **Saya tidak memilih salah satunya.** Cakupan task ini adalah *"nol source baru, seluruhnya
> verifikasi berbukti"*, dan keduanya menyentuh keputusan rancangan. Yang dapat saya nyatakan:
> **satu dari dua dokumen pasti salah**, dan dibiarkan seperti sekarang, komentar index itu
> akan menyesatkan orang berikutnya yang membacanya.

**Pemilik:** pemilik modul. **Nol memblokir `S4b`** — perilaku yang dialami analis sudah benar.

---

## 4. Satu hal yang justru menguatkan, bukan melemahkan

Interpretasi `S`/`I`/`R` terbukti konsisten sepanjang pengujian ini **tanpa direncanakan**:

| Zona | Breakpoint | Hasil |
|---|---|---|
| `10` | 13–17 | `Resistant` — di bawah batas bawah |
| `14` | 13–17 | `Intermediate` — di dalam rentang |

`LAB-DEC-123` berjalan pada baris yang baru saja dibuat ulang, bukan hanya pada baris lama.

---

## 5. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Mengubah `RemoveRange` menjadi penandaan | Keputusan rancangan, bukan verifikasi. `LAB-CONFLICT-011` |
| Memperbaiki komentar index | Sama — ia bagian dari keputusan yang sama |
| Mencabut filter parsial | **Tegas ditolak.** Ia jaring pengaman yang murah; mencabutnya sebelum gaya hapusnya diputuskan adalah urutan yang terbalik |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** pemeriksaan `6fa8a2b6-…` kini berisi satu baris kepekaan
> `Ampicillin` zona `14` (`Intermediate`), menggantikan zona `10` sebelumnya. Baris lama
> **tidak dapat dikembalikan** — ia terhapus fisik, dan itulah temuan bagian 3.
