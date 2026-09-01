# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `4` |
| Module status | `PARTIAL` |
| Current phase | `BD-PH-005` |
| Last verified at | `belum pernah diverifikasi` |
| Backend source SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend source SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Terakhir diperbarui | `2026-09-02` |

Modul tetap `PARTIAL`. Arsitektur domain sudah disusun dan menghasilkan
`DOMAIN_ARCHITECTURE_PARTIAL`: sebagian besar bentuk domainnya siap diserahkan ke penyusunan
blueprint, sementara jalur pemberian darah, pengalihan kantong, penyerahan biaya, dan mekanik label
berhenti menunggu keputusan pemilik. Tidak ada blocker yang menghentikan seluruh modul.

## Fase modul

| Fase | Nama | Status | Keterangan |
| --- | --- | --- | --- |
| `BD-PH-001` | Discovery dan Requirement | `DONE` | Scope pass dan closure pass selesai: `SCOPE-BD-001` dan `DEC-BD-001` sampai `DEC-BD-024`. |
| `BD-PH-002` | Audit kemampuan existing | `DONE` | 24 baris kemampuan berbukti pada `02-existing-capability-map.md`. Tidak ada lagi baris berstatus `Conflict`. |
| `BD-PH-003` | Gerbang kelengkapan requirement | `DONE` | Penilaian per slice pada `02-requirement-completeness-assessment.md` revisi 2. Delapan slice `READY_FOR_DOMAIN_DESIGN`, dua `PARTIALLY_READY`. |
| `BD-PH-004` | Arsitektur domain rumah sakit (opsional) | `DONE` sebagian | Dijalankan dan menghasilkan `DOMAIN_ARCHITECTURE_PARTIAL` pada `03-domain-architecture.md`. Lima aggregate, dua puluh konsep domain, sembilan bounded context. Enam gap arsitektur baru muncul. |
| `BD-PH-005` | Penyusunan blueprint target | `READY` | Boleh berjalan untuk slice arsitektur yang dinyatakan siap pada handoff `03-domain-architecture.md`. |
| `BD-PH-006` | Perencanaan delivery | `NOT_STARTED` | Menunggu `BD-PH-005`. |
| `BD-PH-007` | Implementasi backend | `BLOCKED` | Terhalang `BD-DEP-008`, prefix modul belum terdaftar di registry. |
| `BD-PH-008` | Implementasi frontend | `NOT_STARTED` | Menunggu kontrak API dibekukan. |
| `BD-PH-009` | Verifikasi kesiapan | `NOT_STARTED` | — |

### Ringkasan fase

| Fase selesai | Fase siap | Fase terblokir |
| --- | --- | --- |
| `BD-PH-001`, `BD-PH-002`, `BD-PH-003`, `BD-PH-004` sebagian | `BD-PH-005` | `BD-PH-007` |

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## Blocker yang masih terbuka

| Blocker ID | Ringkasan | Pemilik | Terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| `DEC-BD-016` | Persetujuan pemilik Billing untuk menambah satu konteks sumber dan satu jenis efek biaya Bank Darah pada `BillingSourceContract`. Pemicunya sudah jelas: satu tindakan Bank Darah yang selesai. | Pemilik BillingManagement | Penyerahan biaya ke Billing pada slice tindakan | Pencatatan tindakan Bank Darah tetap dapat dirancang tanpa bagian penyerahan biayanya |
| `OQ-BD-011` | Isi label golongan darah, kapan boleh dicetak, identifier uniknya, dan perilaku cetak ulang. `DEC-BD-015` baru menutup sumber datanya. | Pemilik proses klinis | Slice label golongan darah | Pemeriksaan dan validasi golongan darah tetap dapat dirancang penuh |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama. | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen | Titik pemeriksaan kecocokan tetap dapat dirancang |
| `DEF-BD-004` | Peran pemakai jalur darurat dan peran validator hasil golongan darah. | Pemilik proses BDRS dan klinis | `IMPLEMENTATION` jalur darurat dan validasi | Bentuk kedua alur sudah pasti; tinggal peran yang mengisinya |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong yang sudah keluar. Fakta di luar sistem. | Pemilik proses BDRS | Kegunaan pilihan `RETURNED_TO_PROVIDER` | Rancangannya tetap dibuat |
| `BD-DEP-008` | Bank Darah belum terdaftar di `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Pembuatan entity operasional baru `BLOCKED` menurut `QBE-MOD-002` dan `QBE-MOD-003`. | Pemilik registry engineering | `BD-PH-007` | Seluruh fase perancangan tetap jalan |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository. | Pemilik kebutuhan | Penelusuran bukti ke kebutuhan | Perancangan tetap jalan |
| `ARCH-BD-GAP-01` | Bila PMI mengirim lebih banyak dari yang diminta, apa yang terjadi? Jumlah sisa berpotensi menjadi angka negatif. | Pemilik proses BDRS | Invariant pada aggregate permintaan | Bentuk dasar permintaan tetap dapat dirancang |
| `ARCH-BD-GAP-02` | Bila pasien punya lebih dari satu hasil golongan darah tervalidasi, mana yang berlaku? | Pemilik proses klinis | Aturan identitas hasil golongan darah | Pencatatan dan validasi hasil tetap dapat dirancang |
| `ARCH-BD-GAP-03` | Apakah bukti kecocokan punya masa berlaku? | Pemilik proses klinis | **Keselamatan.** Gerbang pemberian darah | Alokasi dan pencatatan bukti tetap dapat dirancang |
| `ARCH-BD-GAP-04` | Bila kantong dialihkan ke pasien lain, apakah bukti kecocokan sebelumnya gugur? | Pemilik proses klinis | **Keselamatan.** Jalur pengalihan kantong | Jalur pengembalian ke PMI dan penetapan tidak layak tetap jalan |
| `ARCH-BD-GAP-05` | Bolehkah alokasi yang keliru dibatalkan sebelum pemberian, dan oleh siapa? | Pemilik proses BDRS | Satu perpindahan pada aggregate kantong | Alokasi itu sendiri tetap dapat dirancang |
| `ARCH-BD-GAP-06` | Bagaimana mengoreksi pencatatan pemberian yang keliru? | Pemilik proses klinis dan BDRS | Jalur koreksi pemberian | Jalur pemberian normal tetap dirancang sesuai batasnya |

## Blocker yang sudah ditutup

| Blocker | Ditutup oleh |
| --- | --- |
| Sinyal penutupan kunjungan berbeda antar jenis kunjungan | `DEC-BD-014` |
| Bukti kecocokan sebelum pemberian darah belum diatur | `DEC-BD-013` dan `DEC-BD-017` |
| Sumber sah golongan darah belum ditetapkan | `DEC-BD-015` |
| Aturan pengembalian dan pemakaian ulang kantong (`DEF-BD-001`) | `DEC-BD-019` |
| Penutupan administratif permintaan PMI (`DEF-BD-002`) | `DEC-BD-020` |
| Requirement tindakan Bank Darah dan dasar biayanya | `DEC-BD-021` |
| Requirement sampling dan batas dengan Laboratorium | `DEC-BD-018` dan `DEC-BD-015` |
| Kedudukan HCLAB | `DEC-BD-022` |
| Requirement laporan | `DEC-BD-023` |
| Requirement setup | `DEC-BD-024` |

## Artefak arsitektur

`03-domain-architecture.md` revisi 1 memuat sembilan bounded context, dua puluh konsep domain, lima
aggregate, model relasi, empat model lifecycle, tanggung jawab hak akses, model audit, model
integrasi, dampak billing, dampak keselamatan klinis, dan enam gap arsitektur.

## Bukti yang sudah usang

| Artefak atau bukti | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `9522caa` | Terbatas pada konfigurasi Laboratorium yang berubah — `TrxLabSpecimenConfiguration.cs`, `TrxLabTransitionHistoryConfiguration.cs`, `MstLabRejectionReasonConfiguration.cs`. Dampaknya menyempit setelah `DEC-BD-018` menetapkan sampel Bank Darah terpisah dari sampel Laboratorium. |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `9522caa` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium digantikan `DEC-BD-018`. |

`02-existing-capability-map.md` terikat pada backend `9522caa` dan frontend `afbb8ab`. Bila salah
satu berubah, tandai peta itu `STALE` lalu jalankan pemindaian dampak terbatas sesuai daftar berkas
pemicu di dalamnya.

## Task berikutnya yang disarankan

Serahkan slice arsitektur yang dinyatakan siap pada handoff `03-domain-architecture.md` ke
`design-business-module`. Sejalan dengan itu, kembalikan enam gap arsitektur baru
(`ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-06`) ke `grill-me` sebagai closure pass lanjutan, karena
seluruhnya keputusan bisnis dan klinis yang bergantung pemilik.

## Kemajuan delivery

Belum dapat dihitung. Belum ada roadmap task yang disetujui, sehingga tidak ada pembagi yang sah.
Persentase tidak boleh diperkirakan secara manual.

## Kontrak status

`DRAFT` berarti identitas modul sudah ada tetapi pengumpulan kebutuhan belum lengkap. `DISCOVERY`
berarti sedang mengumpulkan keputusan dan bukti. `READY` berarti fase yang direncanakan boleh
dimulai. `PARTIAL` berarti minimal satu fase siap sementara fase lain terblokir atau belum
diketahui. `BLOCKED` berarti tidak ada satu pun fase berarti yang dapat berjalan dengan aman.
`IN_PROGRESS` berarti ada pekerjaan aktif yang sudah diberi wewenang. `VERIFYING` berarti menunggu
bukti kesiapan. `DONE` menuntut bukti verifikasi yang memadai. `SUPERSEDED` mencatat blueprint
penggantinya.

Status fase memakai `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, dan `SUPERSEDED`.
Sebuah fase menjadi `DONE` hanya bila bukti penerimaannya tercatat. Keberadaan file saja tidak
cukup.
