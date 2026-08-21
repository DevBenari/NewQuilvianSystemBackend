# Rawat Inap — Peta Antar Bounded Context

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.1` |
| Status | `draft` |
| Backend SHA | `5afb54b` |

Dokumen ini menunjukkan **arah ketergantungan** antar konteks, bukan kolom per tabel. Kolom ada
pada ERD per konteks dan pada kamus data.

---

## 1. Peta ketergantungan

```mermaid
erDiagram
    CTX_INP_CARE ||--o{ CTX_INP_CONFIG : "membaca pengaturan"
    CTX_REG ||--o{ CTX_INP_CARE : "menyediakan kunjungan"
    CTX_PAT ||--o{ CTX_REG : "menyediakan pasien"
    CTX_MST ||--o{ CTX_INP_CARE : "menyediakan tempat tidur dan kelas"
    CTX_INP_CARE ||--o{ CTX_MST : "menulis salinan status tempat tidur"
    CTX_WFP ||--o{ CTX_INP_CARE : "menyediakan dokter dan pegawai"
```

| Kode pada diagram | Nama lengkap | Modul pemilik |
| --- | --- | --- |
| `CTX_INP_CARE` | Episode Perawatan Rawat Inap | `InPatientManagement` |
| `CTX_INP_CONFIG` | Konfigurasi Rawat Inap | `InPatientManagement` |
| `CTX_REG` | Registrasi dan Kunjungan | `RegistrationManagement` |
| `CTX_PAT` | Identitas Pasien | `PatientManagement` |
| `CTX_MST` | Master Fasilitas dan Layanan | `MasterData` HealthServices |
| `CTX_WFP` | Tenaga Kerja dan Praktisi | `Corporate/HumanResource` |

---

## 2. Sifat setiap ketergantungan

| Dari | Ke | Arah | Sifat | Yang dilewatkan |
| --- | --- | --- | --- | --- |
| `CTX_INP_CARE` | `CTX_REG` | Baca | Mengikuti bentuk yang sudah ada | `TrxPatientEncounter.Id` sebagai jangkar episode |
| `CTX_INP_CARE` | `CTX_MST` | Baca | Mengikuti | `MstBed`, `MstRoom`, `MstServiceUnit`, `MstPatientClass` |
| `CTX_INP_CARE` | `CTX_MST` | **Tulis** | **Bermitra** | Hanya kolom `MstBed.BedStatus`, dan hanya nilai `Available`, `Reserved`, `Occupied` |
| `CTX_INP_CARE` | `CTX_WFP` | Baca | Mengikuti | `MstDoctor.Id`, `MstEmployee.Id` |
| `CTX_INP_CARE` | `CTX_INP_CONFIG` | Baca | Milik sendiri | Batas waktu dan daftar butir administrasi |

**Satu-satunya arah tulis keluar** adalah baris ketiga. Rinciannya, termasuk laporan selisih yang
mengawasinya, ada pada [`../contracts/integration-contract.md`](../contracts/integration-contract.md).

---

## 3. Konteks yang sengaja tidak terhubung pada revisi ini

| Konteks | Modul | Kenapa belum terhubung |
| --- | --- | --- |
| Dokumentasi Klinis | `ClinicalManagement` | Menunggu `DEC-INP-001` |
| Farmasi | `PharmacyManagement` | Menunggu `DEC-INP-001` |
| Instalasi Gawat Darurat | `EmergencyInstallationManagement` | Menunggu `DEC-INP-002` |
| Interoperabilitas SATUSEHAT | Belum ada pemiliknya | Menunggu `DEC-INP-005` |
| Billing | `BillingManagement` | Tidak dipakai pada MVP; digantikan penandaan manual sesuai `RWI-RULE-028` |

Ketiadaan garis ke lima konteks itu adalah **keadaan yang disengaja**, bukan gambar yang belum
selesai.

---

## 4. Daftar ERD per konteks

| Konteks | Berkas |
| --- | --- |
| `CTX_INP_CARE` | [`01-inpatient-episode.md`](./01-inpatient-episode.md) |
| `CTX_INP_CONFIG` | [`02-inpatient-configuration.md`](./02-inpatient-configuration.md) |
| Kamus data seluruh tabel | [`data-dictionary.md`](./data-dictionary.md) |
