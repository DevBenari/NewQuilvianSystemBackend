# Template Class Diagram, ERD, dan Kamus Data

Semua diagram memakai Mermaid sebagai blok kode di dalam markdown. Alasannya: tampil sebagai
gambar di GitHub dan VS Code, tetap terbaca sebagai teks, dan perubahannya dapat ditelusuri
git baris per baris.

Aturan yang berlaku untuk seluruh diagram: **satu diagram MUST muat dibaca dalam satu layar.**
Pecah per bounded context atau per kelompok proses. Modul dengan 15 model **MUST NOT**
digambar dalam satu diagram.

---

## 1. Class diagram

Ditempatkan di `02-backend-architecture.md`.

### 1.1 Bentuk diagram

````markdown
```mermaid
classDiagram
    class TrxEmergencyVisit {
        +Guid Id
        +Guid EncounterId
        +Guid PatientId
        +EmergencyVisitStatus VisitStatus
        +DateTime ArrivalDateTime
    }
    class TrxEmergencyTriage {
        +Guid Id
        +Guid EmergencyVisitId
        +Guid TriageLevelId
        +bool IsRetriage
        +EmergencyTriageStatus TriageStatus
    }
    class MstEmergencyTriageLevel {
        +Guid Id
        +int Level
        +int MaxWaitingMinutes
    }
    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyTriage : memiliki
    MstEmergencyTriageLevel "1" --> "0..*" TrxEmergencyTriage : menentukan level
```
````

Tampilkan hanya field yang penting bagi pembaca diagram: kunci, status, dan field yang dipakai
aturan bisnis. Field lengkap ada di kamus data, bukan di diagram.

### 1.2 Tabel penjelasan setiap class

Setiap class yang muncul di diagram **MUST** punya tabel berikut. Dua baris pertama adalah
yang paling sering terlupa dan paling dibutuhkan implementer.

| Aspek | Penjelasan |
| --- | --- |
| **Status** | `Baru` / `Diperbarui` / `Sudah ada` |
| **Lokasi file** | Path lengkap, mengikuti [backend-structure-rules.md](backend-structure-rules.md) |
| Kategori | Master / Transaksi / Service / Controller / Configuration |
| Tanggung jawab utama | Satu paragraf, bahasa yang dipahami orang umum |
| Field penting | Daftar field beserta tipe |
| Navigation property dan relasi | Hubungan ke class lain |
| Pemakaian dalam alur bisnis | Kapan class ini aktif, dari sudut pandang petugas |
| Catatan desain | Larangan dan jebakan yang perlu dihindari |
| Ekuivalen model lama | Diisi bila menggantikan class lama; tulis `—` bila tidak ada |

Contoh terisi:

| Aspek | Penjelasan |
| --- | --- |
| **Status** | `Diperbarui` |
| **Lokasi file** | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs` |
| Kategori | Transaksi IGD |
| Tanggung jawab utama | Menyimpan satu episode penilaian triage. Penilaian ulang membuat baris baru, tidak menimpa baris lama, agar perubahan kondisi pasien dapat ditelusuri |
| Field penting | `EmergencyVisitId`, `TriageLevelId`, `IsRetriage`, `PreviousTriageId`, `TriageStatus`, `ResponseDueAt` |
| Navigation property dan relasi | Milik `TrxEmergencyVisit`; menunjuk `MstEmergencyTriageLevel`; punya banyak `TrxEmergencyTriageDetail` |
| Pemakaian dalam alur bisnis | Dibuat perawat saat pasien dinilai pertama kali, dan setiap kali dinilai ulang |
| Catatan desain | Jangan menimpa baris lama saat retriage. Target waktu respons diambil dari master, jangan di-hardcode |
| Ekuivalen model lama | `IGDTriage` |

### 1.3 Service dan controller juga dijelaskan

Class diagram dan tabel penjelasan **MUST** mencakup service dan controller, bukan hanya
model. Untuk keduanya, tambahkan:

| Aspek tambahan | Untuk |
| --- | --- |
| Service yang dipakai | Controller |
| Dipanggil oleh | Service |
| Membuka transaksi database | Service |
| Endpoint yang diurus | Controller |

Controller yang tidak memakai service **MUST** dinyatakan alasannya, misalnya *"CRUD sederhana,
memakai `ApplicationDbContext` langsung"*.

---

## 2. ERD

Ditempatkan di `erd/`.

| File | Isi |
| --- | --- |
| `00-context-erd.md` | Peta antar bounded context beserta arah ketergantungannya |
| `<bounded-context>.md` | ERD detail satu konteks, misalnya `emergency-episode.md` |
| `data-dictionary.md` | Kamus data per kolom |

### 2.1 Bentuk diagram

Status entity ditulis pada label relasi agar terbaca tanpa legenda terpisah.

````markdown
```mermaid
erDiagram
    TrxPatientEncounter ||--|| TrxEmergencyVisit : "1:1 — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyTriage : "1:N — Sudah ada"
    TrxEmergencyTriage ||--o{ TrxEmergencyTriageDetail : "1:N — Sudah ada"
    TrxEmergencyTriage |o--o| TrxEmergencyTriage : "0:1 — Baru, penilaian pengganti"
    MstEmergencyTriageLevel ||--o{ TrxEmergencyTriage : "1:N — Sudah ada"
```
````

Notasi kardinalitas Mermaid yang dipakai:

| Notasi | Arti |
| --- | --- |
| `||--||` | Satu ke satu, keduanya wajib |
| `||--o{` | Satu ke banyak, sisi banyak boleh kosong |
| `|o--o|` | Nol atau satu ke nol atau satu |

### 2.2 Tabel status entity

Mendampingi setiap ERD:

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `TrxPatientEncounter` | Sudah ada | Registration Management | Direferensikan, **MUST NOT** disalin |
| `TrxEmergencyVisit` | Sudah ada | Emergency Installation | — |
| `TrxEmergencyTriage` | Diperbarui | Emergency Installation | Tambah kolom penilaian pengganti |
| `TrxEmergencyWaitingAlert` | Baru | Emergency Installation | Tabel baru |

---

## 3. Kamus data

Ditempatkan di `erd/data-dictionary.md`.

### 3.1 Kedalaman mengikuti status tabel

Ini menghindari menyalin ratusan kolom yang tidak berubah.

| Status tabel | Yang wajib didokumentasikan |
| --- | --- |
| **Baru** | Seluruh kolom |
| **Diperbarui** | Seluruh kolom |
| **Sudah ada** | Kolom kunci saja: PK, FK, kolom status, dan kolom yang dipakai aturan bisnis modul ini. Ditambah rujukan ke file model sebagai sumber lengkap |

Bila modul menambah kolom pada tabel yang tadinya `Sudah ada`, statusnya berubah menjadi
`Diperbarui` dan seluruh kolomnya wajib ditulis.

Sepuluh kolom warisan `IdentityModel` **MUST NOT** diulang. Tulis satu kali di kepala dokumen.

### 3.2 Bentuk tabel

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EmergencyVisitId` | `Guid` | Ya | — | Index | FK ke `TrxEmergencyVisit` | `Restrict` | Tidak | Induk kunjungan IGD |
| `TriageLevelId` | `Guid` | Ya | — | Index | FK ke `MstEmergencyTriageLevel` | `Restrict` | Tidak | Level triage yang ditetapkan |
| `IsRetriage` | `bool` | Ya | `false` | — | — | — | Tidak | Menandai penilaian ulang |
| `ResponseDueAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Batas waktu respons, dihitung dari master |
| `ChiefComplaint` | `string(1000)` | Tidak | — | — | — | — | **Ya** | Keluhan utama pasien |

### 3.3 Kolom Sensitif

Kolom bertanda **Ya** pada kolom Sensitif:

- **MUST NOT** masuk ke custom logger;
- **MUST NOT** dipakai sebagai contoh berisi data asli di dokumentasi;
- **SHOULD** ditinjau kebutuhan maskingnya pada response DTO.

Penandaan ini bukan hiasan. Ia menjadi masukan langsung bagi aturan logging dan bagi
`permission-audit-matrix.md`.

### 3.4 Contoh kepala dokumen

```markdown
# Kamus Data — Modul IGD

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.
```
