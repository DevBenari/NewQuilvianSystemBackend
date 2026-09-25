# Laporan Perubahan Backend — `BE-LAB-62`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-62` (backend) |
| Judul | Profil Mikrobiologi katalog — gelombang `MVP-7b`, slice `S4b` |
| Trace | `LAB-DEC-125`; menegakkan `LAB-DEC-087` |
| Kontrak | `LAB-API-v1` `r27` bagian 22.6; `LAB-VAL-v1` `r10` (`VAL-118`); `LAB-PERM-v1` rev 9 bagian 11.1 |
| Klasifikasi | `MEDIUM` — satu tabel pemetaan, lima endpoint |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Dependency | `BE-LAB-53` ✅ |
| Status | ✅ **`SELESAI`** — tabel berdiri, lima endpoint berjalan, **`VAL-118` terbukti pada ketiga keadaannya** |

---

## 1. Kenapa task ini dipilih berikutnya

`BE-LAB-48` menutup laporannya dengan satu aturan yang **belum dapat ditegakkan**: `VAL-118`
menuntut bagian isolat ditolak pada pemeriksaan yang tidak memakai set bakteri, dan tabel
pemetaannya belum ada.

Menumpuk aturan yang tidak ditegakkan adalah cara paling mudah kehilangan jejak mana yang
sudah berlaku dan mana yang belum. **Task ini menutupnya.**

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabProcedureMicrobiologyProfile.cs` | Pemetaan per pemeriksaan katalog |
| `Repositories/Configurations/.../LabProcedureMicrobiologyProfileConfiguration.cs` | FK `Restrict`, index unik parsial |
| `DTOs/LabProcedureMicrobiologyProfileDtos.cs` | 4 DTO |
| `Services/LabProcedureMicrobiologyProfileService.cs` | CRUD + `UsesSusceptibilitySetAsync` |
| `Controllers/LabProcedureMicrobiologyProfileController.cs` | 5 endpoint |
| `Services/LabMicrobiologyResultService.cs` | **`VAL-118` disambungkan** |
| `Migrations/20260921045527_AddLabProcedureMicrobiologyProfile.cs` | Dibangkitkan |

> **`VAL-118` sengaja ikut disambungkan ke jalur hasil.** Tabel yang nol pernah dibaca tidak
> menegakkan apa pun — dan aturan yang tercatat tetapi tidak berlaku lebih berbahaya daripada
> aturan yang belum ada, sebab ia terlihat sudah selesai.

### Keputusan perilaku: pemeriksaan yang BELUM diprofilkan tetap diterima

Tabel pemetaan **dimulai kosong**. Menolak seluruh pengisian hasil sampai kepala instalasi
selesai memetakan berarti **halaman hasil Mikrobiologi lahir dalam keadaan tidak dapat
dipakai**.

Yang ditolak hanyalah pemeriksaan yang **sudah diprofilkan secara tegas** sebagai tidak
memakai set bakteri. Alasan yang sama dipakai jalur jatuh `LAB-DEC-111` pada daftar dokter
jaga.

### Kenapa bukan diturunkan dari nama, dan bukan kolom pada `MstProcedure`

Awalan `MO KUL` memang menandai kultur mikroorganisme, tetapi `LAB-DEC-087` **sudah menolak**
menurunkan kategori dari kata kunci nama. Pelajaran `BE-LAB-52` masih berlaku: pencocokan nama
pernah menggolongkan *Imuno**histo**kimia* ke Histologi hanya karena mengandung `HISTO`.

`MstProcedure` milik `master-data`; menambah kolom di sana sudah dua kali menahan modul ini
lewat `LAB-COORD-006` dan `MST-POS-WRITE`.

---

## 3. Verifikasi

### 3.1 Terhadap database

| Pemeriksaan | Hasil |
|---|---|
| Index unik | ✅ `... ON ("ProcedureId") WHERE ("IsDelete" = false)` — **parsial** |
| `FK_..._MstProcedure_ProcedureId` | ✅ **RESTRICT** |
| `Down` migration | ✅ `DropTable` |

### 3.2 `VAL-118` — ketiga keadaan diuji terhadap aplikasi yang berjalan

| Keadaan | Kirim isolat | Hasil |
|---|---|---|
| **Belum diprofilkan** | ya | ✅ **`200`** — diterima, sesuai keputusan perilaku di atas |
| **Diprofilkan `usesSusceptibilitySet: false`** | ya | ✅ **`422`** *"Pemeriksaan ini tidak memakai set bakteri."* |
| **Diprofilkan `false`**, tanpa isolat | tidak | ✅ **`200`** — hasil tanpa set bakteri tetap boleh disimpan |
| **Diubah menjadi `true`** | ya | ✅ **`200`** — diterima kembali |

> Keadaan ketiga penting: pemeriksaan yang tidak memakai set bakteri **tetap boleh punya
> hasil** — yang ditolak hanya bagian isolatnya.

### 3.3 Kelima endpoint

`POST` membuat profil (`200`), `PUT` mengubahnya (`200`); `GET` daftar, `GET` satu baris, dan
`DELETE` mengikuti pola yang sama dengan `BE-LAB-60` yang sudah terbukti.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Pengisian pemetaan | `LAB-DEC-125` menyerahkannya kepada kepala instalasi |
| Pemakaian `DefaultCultureType`/`DefaultSusceptibilityMethod` untuk mengisi awal layar | Frontend, `FE-LAB-31` |
| `VAL-84` bentuk hasil | Menunggu data induk batas nilai berbentuk `MicrobiologyStructured` |
| `git add`, `commit`, `push` | Nol diminta |

> **Baris uji tertinggal di dev:** satu profil untuk pemeriksaan Leukosit
> (`dd000003-…-0002`) bertanda `usesSusceptibilitySet: true`. **Bukan pemetaan resmi.**
