# Laporan Perubahan Backend — `BE-LAB-61`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-61` (backend) |
| Judul | Penghitung interpretasi dan penimpaannya — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-123`, `LAB-DEC-115`, `LAB-DEC-128`; menegakkan `LAB-DEC-039` |
| Kontrak | `LAB-API-v1` `r27` bagian 22.2 dan 22.4; `LAB-VAL-v1` `r10` (`VAL-113`, `VAL-114`, `VAL-116`) |
| Klasifikasi | `HIGH` — hasilnya adalah interpretasi yang dilaporkan kepada dokter |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-60` ✅ selesai |
| Status | ✅ **`SELESAI`** — **16 dari 16 skenario lulus**, termasuk keempat batas tepat |

---

## 1. Yang dibangun

Satu berkas: `Services/LabSusceptibilityInterpreter.cs`, ditambah registrasi DI.

| Bagian | Sifat |
|---|---|
| `Compute(zona, breakpoint)` | **Fungsi murni** — nol menyentuh database |
| `Resolve(zona, breakpoint, nilaiDikirim, alasan, namaAntibiotik)` | Menggabungkan hitungan dengan nilai analis |
| `LoadBreakpointsAsync(organismId)` | Sekali kueri untuk seluruh baris satu isolat |
| `HasAnyBreakpointAsync(organismId)` | Sumber ruas turunan `breakpointAvailable` |
| `LabBreakpointSnapshot`, `LabSusceptibilityVerdict` | Dua `readonly record struct` |

**Tujuh kolom pada `LabIsolateSusceptibility` tidak dikerjakan di sini** — seluruhnya sudah
berdiri pada `BE-LAB-47`, sebab tabel itu dibangun mengikuti ERD `r27` sejak awal.

### Kenapa `Compute` dibuat murni dan `static`

Nol proyek uji ada di repository (`LAB-RDY-C04`). Fungsi murni dapat diperiksa langsung
terhadap DLL hasil build **tanpa database, tanpa HTTP, dan tanpa data uji** — dan itu
satu-satunya cara membuktikan **batas tepatnya** secara menyeluruh.

### Angka cutoff nol dihardcode

`LAB-DEC-039` mewajibkannya. Kelas ini hanya mengetahui **bentuk** aturannya; rentangnya
datang dari `LabSusceptibilityBreakpoint` yang diisi `DR-LAB-002`.

---

## 2. Verifikasi — 16 skenario, seluruhnya lulus

Dijalankan lewat harness di **direktori scratchpad** yang mereferensikan
`QuilvianSystemBackend.dll` hasil build. **Nol berkas tambahan masuk ke repository.**

### `AC-186` — tiga contoh dari `LAB-EVD-006`

| Skenario | Hasil |
|---|---|
| Fosfomycin zona `11` pada `12-16` | ✅ `Resistant` |
| Netilmicin zona `13` pada `12-15` | ✅ `Intermediate` |
| Imipenem zona `32` pada `13-16` | ✅ `Sensitive` |

### Batas tepat — jebakan `<` versus `<=`

**Ini yang paling mudah salah, dan kesalahannya nol terlihat pada contoh biasa.**

| Skenario | Hasil |
|---|---|
| Zona **persis** batas bawah, `12` pada `12-15` | ✅ `Intermediate` — **bukan** `Resistant` |
| Zona **persis** batas atas, `15` pada `12-15` | ✅ `Intermediate` — **bukan** `Sensitive` |
| Satu di bawah batas bawah, `11` | ✅ `Resistant` |
| Satu di atas batas atas, `16` | ✅ `Sensitive` |

> Kedua batas **termasuk** ke dalam `Intermediate`. Bukti lapangannya Netilmicin: zona `13`
> pada rentang `12-15` dilaporkan `I`. Bila tandanya keliru, sebagian hasil bergeser dari
> `I` menjadi `R` — dan nol galat muncul.

### `AC-191` — zona nol adalah data, kosong bukan

| Skenario | Hasil |
|---|---|
| Zona `0` pada `12-15` | ✅ `Resistant` — **dihitung, bukan diabaikan** |
| Zona **kosong** pada `12-15` | ✅ `null` — belum diukur |
| Zona `20` **tanpa rentang** | ✅ `null` |

### `AC-187` — penimpaan

| Skenario | Hasil |
|---|---|
| Analis nol mengirim nilai | ✅ hitungan dipakai, `ditimpa=False` |
| Nilai dikirim **sama** dengan hitungan | ✅ `ditimpa=False` |
| Ditimpa **tanpa** alasan (`VAL-113`) | ✅ **DITOLAK** |
| Ditimpa **dengan** alasan | ✅ `computed=Resistant`, `result=Sensitive`, `ditimpa=True` |
| `VAL-114` rentang kosong, nol dikirim | ✅ **DITOLAK** |
| `VAL-114` rentang kosong, nilai dikirim | ✅ `computed=null`, nilai analis dipakai |

> **`computed` tetap tersimpan meski ditimpa** — itu yang membuat pertanyaan *"analis menimpa
> dari apa"* tetap terjawab sesudah breakpoint diperbarui.

---

## 3. Tiga jebakan yang roadmap peringatkan — seluruhnya diuji

| Jebakan | Bukti |
|---|---|
| Salah tanda pada batas (`<` versus `<=`) | Empat uji batas tepat |
| Zona `0` diperlakukan sebagai kosong | Dua uji berdampingan: `0` → `Resistant`, kosong → `null` |
| Lupa menyimpan `ComputedResult` | `LabSusceptibilityVerdict` membawanya, dan uji penimpaan memeriksanya |

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Pemanggilan penghitung dari jalur pengisian | **`BE-LAB-48`** — penghitung ini belum punya pemanggil, dan itu memang urutannya |
| Ruas turunan `breakpointAvailable` pada response | `BE-LAB-48`; sumbernya sudah disediakan di sini |
| Migration | **Nol dibutuhkan** — kolomnya sudah berdiri pada `BE-LAB-47` |
| `git add`, `commit`, `push` | Nol diminta |
