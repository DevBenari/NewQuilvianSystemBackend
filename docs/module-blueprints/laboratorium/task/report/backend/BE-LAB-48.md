# Laporan Perubahan Backend — `BE-LAB-48`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-48` (backend) |
| Judul | Jalur pengisian hasil Mikrobiologi — gelombang `MVP-6c`, slice `S4b` |
| Trace | `FR-13.1`..`FR-13.4`, `FR-13.7`, `FR-13.8`; `LAB-DEC-095`, `115`, `122`, `123`, `126`, `128` |
| Kontrak | `LAB-API-v1` `r24` bagian 19.2 diperluas `r27` bagian 22.2/22.3; `LAB-VAL-v1` `r7` (`VAL-83`..`VAL-87`, `VAL-89`) dan `r10` (`VAL-112`..`VAL-117`) |
| Klasifikasi | `HIGH` — isinya diagnosis pasien dan dasar pemilihan antibiotik |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-47` ✅, `BE-LAB-60` ✅, `BE-LAB-61` ✅ |
| Status | ✅ **`SELESAI`** — dua endpoint berjalan; **`AC-111`, `AC-112`, `AC-113`, `AC-114`, dan `AC-185` seluruhnya terbukti terhadap aplikasi yang berjalan** |

---

## 1. Yang dibangun

| Berkas | Isi |
|---|---|
| `DTOs/LabMicrobiologyResultDtos.cs` | **Enam** DTO |
| `Services/LabMicrobiologyResultService.cs` | `SetResultAsync`, `GetResultAsync` |
| `Controllers/LabExaminationController.cs` | `PUT` dan `GET /{id}/result/microbiology` |
| `Program.cs` | Registrasi DI |

**Nol migration** — seluruh kolomnya sudah berdiri pada `BE-LAB-47`.

### Satu `PUT` utuh di dalam satu transaksi

Isolat dan kepekaannya adalah **isi** sebuah hasil, bukan benda yang berdiri sendiri. Endpoint
terpisah per isolat akan membuat separuh hasil tersimpan tanpa separuh lainnya — persis batas
konsistensi yang dilindungi aggregate.

Seluruh penggantian berjalan di dalam `BeginTransactionAsync`: **satu baris yang ditolak
membatalkan seluruhnya.**

### Empat hal sengaja tidak diterima dari pemanggil

Nama organisme, nama antibiotik, kandungan cakram, dan rentang breakpoint. Keempatnya
**diturunkan server** lalu disimpan sebagai snapshot. Menerimanya berarti mengizinkan hasil
menyebut kuman yang tidak ada di daftar mana pun, atau breakpoint yang tidak pernah disahkan.

---

## 2. Verifikasi — terhadap aplikasi yang berjalan

Aplikasi pada `http://localhost:5107` terhadap `QuilvianNewDevYoga`.

### `AC-111` — kultur steril adalah hasil yang sah

`PUT` dengan `isolates: []` dan `microbiologyFinding: Negative` → **`200`**. ✅

> Ini kasus uji terpenting slice ini. Biakan yang tidak menumbuhkan apa pun **menyingkirkan
> dugaan infeksi bakteri**, dan itu temuan berguna. Mewajibkan organisme akan membuatnya
> mustahil disimpan.

### Jalur sah — interpretasi dihitung dan di-snapshot

Zona `20` terhadap rentang `13-17`:

```text
breakpointLowerMm=13  breakpointUpperMm=17
computedResult=3 (Sensitive)  result=3  isResultOverridden=false
```

✅ Rentangnya **di-snapshot**, interpretasinya **dihitung** — analis nol mengetik `S`.

### `AC-112` — penolakan tanpa meninggalkan baris separuh

`PUT` berisi satu isolat dengan **antibiotik yang sama dua kali** → **`422`**
*"Antibiotik ini sudah diuji pada kuman tersebut. Satu antibiotik cukup sekali."*

Pembacaan sesudahnya: **keadaan sebelumnya utuh** — satu isolat *Branhamella catarrhalis*,
satu baris *Ampicillin*, `result=3`. ✅

> Transaksi terbukti bekerja. Tanpa itu, isolat pertama akan tersimpan sedangkan baris
> keduanya gagal — dan hasil pasien tinggal separuh tanpa ada yang menyadarinya.

### `AC-114` — `PUT` mengganti, bukan menambah

`PUT` kedua dengan satu isolat dan zona `10` → hasilnya **tetap satu isolat**, zona `10`,
`result=1` (Resistant). ✅ Nol penumpukan.

### `AC-113` — data induk nonaktif, dua arah

Antibiotik *Ampicillin* dinonaktifkan, lalu:

| Arah | Hasil |
|---|---|
| Hasil **lama** dibaca | ✅ **Tetap utuh** — `antibioticName: Ampicillin`, zona `10`, `result=1` |
| Baris **baru** memakainya | ✅ **`422`** *"Antibiotik ini sudah tidak ada pada panel uji."* |

> Pembacaan **nol menyaring `IsActive`**, dan namanya sudah tersimpan sebagai snapshot —
> itulah yang membuat `INV-31` bekerja.

### `AC-185` — snapshot breakpoint terbukti menjaga arti hasil lama

**Uji paling menentukan pada gelombang ini.** Breakpoint diubah dari `13-17` menjadi `5-8`,
lalu hasil lama dibaca ulang:

```text
breakpointLowerMm=13  breakpointUpperMm=17
zoneDiameterMm=10  computedResult=1 (Resistant)  result=1
```

✅ **Tidak berubah.**

> Bila sistem menghitung ulang dengan rentang baru, zona `10` akan berada **di atas** batas
> atas `8` dan hasilnya menjadi **`Sensitive`** — kebalikan total dari yang dilaporkan.
> Snapshot mencegahnya. Inilah yang membuat cetak ulang tahun depan menghasilkan lembar yang
> sama persis.
>
> `AC-185` sebelumnya menggantung pada `BE-LAB-60` karena nol baris hasil ada untuk
> dibandingkan. Task ini menutupnya.

---

## 3. Keadaan database sesudah pengujian

```text
isolat aktif = 1 | baris kepekaan aktif = 1 | isolat total termasuk terhapus = 1
```

Penggantian berulang **nol meninggalkan baris tertinggal**.

---

## 4. ⛔ Satu aturan yang BELUM dapat ditegakkan

| Aturan | Penahan |
|---|---|
| `VAL-118` — bagian isolat ditolak pada pemeriksaan yang profilnya menyatakan **tidak** memakai set bakteri | `LabProcedureMicrobiologyProfile` adalah **`BE-LAB-62`** dan belum dibangun. Tanpa tabel itu, nol cara mengetahui pemeriksaan mana memakai set bakteri |

Konsekuensinya hari ini: jalur ini menerima hasil Mikrobiologi untuk pemeriksaan apa pun.
**Penegakannya menyusul bersama `BE-LAB-62`.**

`VAL-84` (bentuk hasil tidak cocok dengan jalur) juga belum ditegakkan — ia bergantung
`LabValueBound.ResultForm` bernilai `MicrobiologyStructured`, dan nol baris batas nilai yang
memakainya sampai data induknya disetel.

---

## 5. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Penandaan nilai kritis | `S5`, dan aturannya `BE-LAB-56` |
| Validasi profil katalog | `BE-LAB-62` |
| Ruas cetak — nomor, konsultan, tanggal | `BE-LAB-63` |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** organisme `BRANH-CAT`, antibiotik `AMPI` (**kini
> nonaktif**), satu breakpoint `5-8`, dan satu hasil Mikrobiologi pada pemeriksaan
> `6fa8a2b6…` (Leukosit). Seluruhnya dibuat untuk pengujian dan **bukan data resmi**.
