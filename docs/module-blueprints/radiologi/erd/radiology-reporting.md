# ERD — Radiology Reporting

| Field | Value |
|---|---|
| Contract version | `RAD-ERD-REP-001` |
| Revision | `1` |
| Status | `draft` |
| Konteks | `BC-RAD-03` Reporting |
| Backend SHA | `64da911` |
| Decision | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003`, `RAD-DEC-006` |

Kedua tabel di sini berstatus **`Baru`**. Belum ada satu pun di source.

---

## Diagram

```mermaid
erDiagram
    RadStudy {
        uuid Id PK
        boolean IsUsable "hanya yang true melahirkan bacaan"
        int StudyStatus
    }
    RadReport {
        uuid Id PK
        uuid RadStudyId FK UK
        uuid RadOrderId FK "disalin, untuk pencarian"
        uuid EncounterId FK "disalin, untuk pencarian"
        varchar ReportNumber UK
        int ReportStatus "enum RadReportStatus"
        int CurrentVersionNumber "versi yang sedang berlaku"
        timestamp FirstReleasedAt
        timestamp LastReleasedAt
        int Version "token konkurensi"
    }
    RadReportVersion {
        uuid Id PK
        uuid RadReportId FK
        int VersionNumber "unik bersama RadReportId"
        uuid PreviousVersionId FK "versi yang digantikan"
        int VersionStatus "enum RadReportVersionStatus"
        boolean IsAmendment
        text Findings "SENSITIF"
        text Impression "SENSITIF"
        text Recommendation "SENSITIF"
        uuid AuthorUserId
        int AuthorRoleSnapshot "enum, peran penulis dibekukan"
        timestamp DraftedAt
        uuid ValidatorUserId
        timestamp ValidatedAt
        timestamp ReleasedAt
        varchar AmendmentReason "wajib bila IsAmendment"
        int Version
    }
    RadStudy |o--o| RadReport : "1:0..1 — Baru"
    RadReport ||--o{ RadReportVersion : "1:N — Baru"
    RadReportVersion |o--o| RadReportVersion : "0:1 — Baru, menggantikan"
```

---

## Tabel Status Entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `RadStudy` | `Sudah ada` | Radiology Management | Direferensikan, tidak berubah |
| `RadReport` | **`Baru`** | Radiology Management | Wadah identitas dan status |
| `RadReportVersion` | **`Baru`** | Radiology Management | Isi bacaan per versi |

---

## Index dan Batasan yang Diusulkan

| Tabel | Index | Sifat | Alasan |
|---|---|---|---|
| `RadReport` | `RadStudyId` | **Unik**, difilter `"IsDelete" = false` | Satu study paling banyak satu bacaan |
| `RadReport` | `ReportNumber` | **Unik**, difilter `"IsDelete" = false` | Nomor bacaan tidak boleh kembar |
| `RadReport` | `EncounterId` | Biasa | Mencari seluruh bacaan satu kunjungan |
| `RadReport` | `ReportStatus` | Biasa | Menyaring bacaan yang belum disahkan |
| `RadReportVersion` | `RadReportId` + `VersionNumber` | **Unik** | Nomor versi tidak boleh kembar dalam satu bacaan |
| `RadReportVersion` | `PreviousVersionId` | Biasa | Menelusuri rantai koreksi |

Seluruh relasi memakai `DeleteBehavior.Restrict`.

---

## Tiga Bentuk yang Menentukan

### 1. Mengapa isi bacaan tidak disimpan di `RadReport`

Kalau isi bacaan disimpan di induk, koreksi berarti menimpanya — dan itu dilarang
`RJ-BIL-GATE-DEC-004`. Dengan isi tinggal di versi, koreksi cukup menambah baris baru dan
menaikkan `CurrentVersionNumber`. Versi lama tetap utuh tanpa usaha khusus.

> **Contoh.** Bacaan CT-Scan Tn. B dirilis pukul 08.00 sebagai versi 1. Pukul 09.00 dr. Sinta
> menemukan hal yang terlewat dan merilis amandemen. Yang terjadi: baris versi 2 dibuat dengan
> `PreviousVersionId` menunjuk versi 1, versi 1 berubah statusnya menjadi `Superseded`, dan
> `CurrentVersionNumber` pada induk menjadi 2. **Isi versi 1 tidak berubah satu huruf pun.**

### 2. Mengapa tidak ada foreign key dari induk ke versi berlaku

Menyimpan `CurrentVersionId` sebagai kunci asing akan membuat induk menunjuk anaknya sementara
anak menunjuk induknya — relasi melingkar yang menyulitkan penyimpanan dan penghapusan. Cukup
`CurrentVersionNumber` berupa angka, dan versinya dicari lewat pasangan
`RadReportId` + `VersionNumber` yang sudah unik.

### 3. Mengapa `AuthorRoleSnapshot` disimpan, bukan dibaca dari peran pengguna

Ini inti aturan pengesahan `RAD-DEC-003`.

> **Contoh bahayanya.** dr. Rian menulis draf pada Januari sebagai **residen**. Menurut aturan,
> draf itu wajib disahkan radiolog lain. Pada Juli dr. Rian lulus menjadi Sp.Rad. Kalau peran
> dibaca dari data pengguna saat itu juga, draf Januari mendadak terbaca ditulis oleh
> radiolog — dan aturan "wajib disahkan orang lain" ikut menguap. Membekukan perannya pada saat
> draf dibuat mencegah itu.

---

## Yang Sengaja Belum Ada

| Yang belum ada | Alasan |
|---|---|
| Penanda temuan kritis | `DEC-RAD-002` belum ditutup. Lihat `GAP-RAD-01` pada `02-backend-architecture.md` bagian 10 |
| Bacaan sementara (*preliminary*) sebelum bacaan final | Tidak terdapat dalam bukti mana pun. `GAP-RAD-02` |
| Lampiran berkas citra | PACS dan DICOM di luar scope (`RAD-DEC-001`) |
