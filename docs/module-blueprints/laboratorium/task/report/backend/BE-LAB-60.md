# Laporan Perubahan Backend — `BE-LAB-60`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-60` (backend) |
| Judul | Data induk breakpoint dan kandungan cakram — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-122`; menegakkan `LAB-DEC-039` (angka cutoff nol dihardcode) |
| Kontrak | `LAB-API-v1` `r27` bagian 22.5; `LAB-VAL-v1` `r10` (`VAL-115`, `VAL-119`); `LAB-PERM-v1` rev 9 bagian 11.1 |
| Klasifikasi | `HIGH` — isinya menentukan interpretasi yang dilaporkan kepada dokter |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-53` ✅ selesai |
| Status | ✅ **`SELESAI`** — tabel dan kolom berdiri; **kelima endpoint terbukti berjalan**; `VAL-115` dan `VAL-119` terbukti |

---

## 1. Urutan gelombang dikoreksi sebelum task ini dipilih

Task berikutnya menurut roadmap adalah `BE-LAB-48` (jalur pengisian hasil Mikrobiologi).
**Gate menolaknya.**

`BE-LAB-48` berkontrak `r24` bagian 19.2 dengan dependency hanya `BE-LAB-47`. Di bawah `r27`
yang disetujui 2026-09-21, endpoint pengisian hasil wajib **menyalin snapshot kandungan cakram
dan rentang breakpoint** lalu **menghitung interpretasi** — dan ketiganya milik `BE-LAB-60`
dan `BE-LAB-61`.

**Membangun `BE-LAB-48` lebih dulu berarti menulisnya dua kali.** Urutan yang benar:
`BE-LAB-60` → `BE-LAB-61` → `BE-LAB-48`.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabSusceptibilityBreakpoint.cs` | Tabel rentang per pasangan organisme dan antibiotik |
| `Models/LabAntibiotic.cs` | Kolom `DiscContentUg` |
| `Repositories/Configurations/.../LabSusceptibilityBreakpointConfiguration.cs` | FK, index unik parsial |
| `DTOs/LabSusceptibilityBreakpointDtos.cs` | 4 DTO |
| `Services/LabSusceptibilityBreakpointService.cs` | CRUD + validasi |
| `Controllers/LabSusceptibilityBreakpointController.cs` | 5 endpoint |
| `Repositories/ApplicationDbContext.cs`, `Program.cs` | `DbSet` + registrasi DI |
| `Migrations/20260921042352_AddLabSusceptibilityBreakpoint.cs` | Dibangkitkan |

### Kenapa rentang disimpan per ORGANISME, bukan per antibiotik saja

CLSI menetapkannya begitu: rentang Ampicillin terhadap *Branhamella catarrhalis* tidak sama
dengan terhadap *Escherichia coli*. Menyederhanakannya menjadi per antibiotik berarti
**melaporkan kepekaan yang salah untuk sebagian kuman, tanpa satu pun galat terlihat**.

### Kenapa hak tulisnya berbeda dari `LabOrganism` dan `LabAntibiotic`

Kedua data induk itu dipegang kepala instalasi; tabel ini dipegang **wewenang klinis
Mikrobiologi** (`LAB-PERM-v1` rev 9 bagian 11.2).

> Menentukan kuman apa yang dipanel adalah penataan laboratorium. Menentukan angka yang
> memisahkan `Resistant` dari `Sensitive` adalah penilaian klinis — menggeser batas bawah dari
> `13` menjadi `12` mengubah sebagian hasil **tanpa satu pun hasil disunting**.

### `DELETE` menonaktifkan, bukan menghapus

Baris hasil menyimpan **snapshot** rentangnya, dan riwayat perubahan rentang adalah riwayat
keselamatan. Perubahan rentang juga mencatat **nilai sebelum dan sesudah** ke jejak audit,
sebab pertanyaan *"kenapa hasil bulan lalu berbeda"* hanya dapat dijawab dari sana.

---

## 3. Verifikasi

### 3.1 Terhadap database

Migration diterapkan ke `QuilvianNewDevYoga`.

| Pemeriksaan | Hasil |
|---|---|
| Tabel `LabSusceptibilityBreakpoint` berdiri | ✅ |
| Kolom `LabAntibiotic.DiscContentUg` berdiri | ✅ |
| Baris `LabAntibiotic` yang sudah ada | **tidak berubah** |
| Index unik | ✅ `... WHERE ("IsDelete" = false)` — **parsial** |
| `FK_..._LabOrganism_LabOrganismId` | **RESTRICT** |
| `FK_..._LabAntibiotic_LabAntibioticId` | **RESTRICT** |

### 3.2 Terhadap aplikasi yang berjalan

Data induk yang ada seluruhnya **nonaktif** (baris uji `BE-LAB-44`), sehingga organisme
*Branhamella catarrhalis* dan antibiotik *Ampicillin* dibuat lebih dulu lewat endpoint
masing-masing — bukan lewat penyisipan langsung ke database.

| # | Skenario | Diharapkan | Hasil |
|---|---|---|---|
| 1 | Rentang **terbalik** `17-13` (`VAL-115`) | `400` | ✅ *"Batas bawah tidak boleh lebih besar daripada batas atas."* |
| 2 | Buat rentang sah `13-17` | `200` | ✅ tersimpan beserta nama organisme dan antibiotiknya |
| 3 | Pasangan **duplikat** (`VAL-119`) | `409` | ✅ *"Breakpoint untuk pasangan … sudah ada."* |
| 4 | `GET` daftar | `200` | ✅ |
| 5 | `GET` satu baris | `200` | ✅ |
| 6 | `PUT` ubah menjadi `12-16` | `200` | ✅ `lowerMm=12`, `upperMm=16` |
| 7 | `DELETE` | `200` | ✅ *"…berhasil dinonaktifkan."* |

**Kelima endpoint berjalan.**

### 3.3 `DELETE` terbukti menonaktifkan, bukan menghapus

Keadaan baris sesudah `DELETE`:

```text
LowerMm=12 | UpperMm=16 | GuidelineVersion=CLSI M100 ED35 | IsActive=False | IsDelete=False
```

✅ Barisnya **bertahan**. Perubahan `PUT` juga terbukti tersimpan.

### 3.4 ⛔ Yang BELUM terbukti

| Butir | Penahan |
|---|---|
| `AC-185` — mengubah breakpoint **tidak mengubah** baris hasil lama yang ber-snapshot | Kolom snapshot sudah berdiri (`BE-LAB-47`), tetapi **nol baris kepekaan ada** sebab endpoint pengisiannya adalah `BE-LAB-48`. Tidak ada hasil lama yang bisa dibandingkan |

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Pengisian baris breakpoint | `LAB-DEC-122` menyerahkannya kepada `DR-LAB-002`. Baris uji di atas **wajib dibersihkan** sebelum dipakai sungguhan |
| Penghitung interpretasi | `BE-LAB-61` |
| Jalur pengisian hasil | `BE-LAB-48` |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji yang tertinggal di dev:** organisme `BRANH-CAT`, antibiotik `AMPI`, dan satu
> rentang breakpoint bertanda nonaktif. Ketiganya dibuat untuk pengujian ini dan **bukan**
> data resmi.
