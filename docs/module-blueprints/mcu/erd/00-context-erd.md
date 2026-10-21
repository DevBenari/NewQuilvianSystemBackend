# MCU — Context ERD

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21 |
| Sumber | `02-backend-architecture.md` r1; `03-domain-architecture.md` r1 (`MCU-HDA-001`) |

Peta ini menunjukkan **arah ketergantungan** antar bounded context. Panah dibaca "menunjuk ke".
Rincian kolom ada di ERD per context.

```mermaid
erDiagram
    KATALOG_MCU {
        string Pemilik "MCU - Baru"
        string Isi "Paket, komponen, persyaratan, harga perusahaan, jenis pemeriksaan, kategori kelayakan"
    }
    JADWAL_MCU {
        string Pemilik "MCU - Baru"
        string Isi "Jadwal harian, kuota, pelampauan"
    }
    LAYANAN_MCU {
        string Pemilik "MCU - Baru"
        string Isi "Order, komponen, hasil, resume, persetujuan, penyerahan"
    }
    MASTER_DATA {
        string Pemilik "Master Data - Sudah ada"
        string Isi "MstTariff, MstProcedure, MstCompanyGuarantor, MstDoctor, MstClinic"
    }
    PATIENT_MANAGEMENT {
        string Pemilik "Sudah ada"
        string Isi "MstPatient"
    }
    REGISTRASI {
        string Pemilik "Sudah ada"
        string Isi "RegPatientEncounter, RegQueue"
    }
    LABORATORIUM {
        string Pemilik "Diperbarui"
        string Isi "LabOrder + CoveringMcuOrderId"
    }
    RADIOLOGI {
        string Pemilik "Diperbarui"
        string Isi "RadOrder + CoveringMcuOrderId"
    }
    BILLING {
        string Pemilik "Sudah ada"
        string Isi "BilInvoiceItem, BilRefundCase"
    }
    KATALOG_MCU }o--|| MASTER_DATA : "tarif, prosedur, perusahaan"
    LAYANAN_MCU }o--|| KATALOG_MCU : "order menjalani paket"
    LAYANAN_MCU }o--|| JADWAL_MCU : "order memakai kuota"
    LAYANAN_MCU }o--o| PATIENT_MANAGEMENT : "peserta tertaut"
    LAYANAN_MCU |o--o| REGISTRASI : "kunjungan MCU"
    LAYANAN_MCU }o--o| BILLING : "baris paket, refund"
    LABORATORIUM }o--o| LAYANAN_MCU : "dicakup paket"
    RADIOLOGI }o--o| LAYANAN_MCU : "dicakup paket"
```

| Context | Status | Pemilik | ERD rinci |
| --- | --- | --- | --- |
| Katalog MCU | Baru | MCU | [katalog-mcu.md](katalog-mcu.md) |
| Jadwal MCU | Baru | MCU | [jadwal-mcu.md](jadwal-mcu.md) |
| Layanan MCU | Baru | MCU | [layanan-mcu.md](layanan-mcu.md) |
| Master Data, Patient Management, Registrasi, Billing | Sudah ada | Modul masing-masing | Dirujuk, tidak disalin |
| Laboratorium, Radiologi | Diperbarui (satu kolom) | Modul masing-masing | [layanan-mcu.md](layanan-mcu.md) bagian Lab/Rad |

**Arah ketergantungan yang sengaja dibalik:** Lab dan Rad menunjuk ke MCU (`CoveringMcuOrderId`),
bukan sebaliknya saja, karena keputusan *jangan menagih* harus diambil Lab/Rad tanpa bertanya ke MCU
(`MCU-HDA-001` `REL-MCU-12`).
