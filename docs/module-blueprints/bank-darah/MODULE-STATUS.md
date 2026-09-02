# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `7` |
| Module status | `PARTIAL` |
| Current phase | `BD-PH-005` |
| Last verified at | `belum pernah diverifikasi` |
| Backend source SHA | `9dc7637adbafb321ad8078d5c52ebe5e4398fe86` cabang `sukmagp` |
| Frontend source SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Terakhir diperbarui | `2026-09-02` |

Modul tetap `PARTIAL`, tetapi arsitektur domainnya kini `DOMAIN_ARCHITECTURE_READY` untuk seluruh
scope yang dinilai. Setelah architecture gap final closure pass pada 2 September 2026, sembilan gap
arsitektur `ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-09` sudah tertutup oleh `DEC-BD-025` sampai
`DEC-BD-034`, sehingga jalur pemberian darah, pengalihan kantong, pembatalan alokasi, koreksi
pemberian, aturan hasil golongan darah, penyelesaian konflik lewat pemeriksaan ulang, masa berlaku
bukti kecocokan per komponen, dan batas koreksi terhadap biaya semuanya sudah punya aturan bisnisnya.

Yang masih menahan tinggal dua slice yang memang **di luar** scope yang dinilai: penyerahan biaya ke
Billing (`DEC-BD-016`) dan mekanik label golongan darah (`OQ-BD-011`). Tidak ada blocker yang
menghentikan slice yang sudah siap, dan tidak ada gap arsitektur yang tersisa.

`03-domain-architecture.md` sudah naik ke revisi 3 dan **sudah** menyerap kesepuluh keputusan closure.
Statusnya kini `DOMAIN_ARCHITECTURE_READY`: dua potongan yang dulu menahan revisi 2 — satu perpindahan
pada `BD-AGG-04` dan satu kumpulan atribut pada `BD-DOM-13` — sudah tertutup oleh `DEC-BD-031` dan
`DEC-BD-032`. Seluruh slice yang dinilai boleh diserahkan ke penyusunan blueprint.

## Fase modul

| Fase | Nama | Status | Keterangan |
| --- | --- | --- | --- |
| `BD-PH-001` | Discovery dan Requirement | `DONE` | Scope pass dan closure pass selesai: `SCOPE-BD-001` dan `DEC-BD-001` sampai `DEC-BD-024`. |
| `BD-PH-002` | Audit kemampuan existing | `DONE` | 24 baris kemampuan berbukti pada `02-existing-capability-map.md`. Tidak ada lagi baris berstatus `Conflict`. |
| `BD-PH-003` | Gerbang kelengkapan requirement | `DONE` | Penilaian per slice pada `02-requirement-completeness-assessment.md` revisi 2. Delapan slice `READY_FOR_DOMAIN_DESIGN`, dua `PARTIALLY_READY`. |
| `BD-PH-004` | Arsitektur domain rumah sakit (opsional) | `DONE` | Dijalankan sampai revisi 3 dan menghasilkan `DOMAIN_ARCHITECTURE_READY` pada `03-domain-architecture.md`. Lima aggregate, dua puluh tiga konsep domain, sembilan bounded context. Seluruh sembilan gap arsitektur sudah tertutup. |
| `BD-PH-005` | Penyusunan blueprint target | `READY` | Boleh berjalan untuk seluruh scope yang dinyatakan `READY` pada handoff `03-domain-architecture.md` revisi 3. |
| `BD-PH-006` | Perencanaan delivery | `NOT_STARTED` | Menunggu `BD-PH-005`. |
| `BD-PH-007` | Implementasi backend | `BLOCKED` | Terhalang `BD-DEP-008`, prefix modul belum terdaftar di registry. |
| `BD-PH-008` | Implementasi frontend | `NOT_STARTED` | Menunggu kontrak API dibekukan. |
| `BD-PH-009` | Verifikasi kesiapan | `NOT_STARTED` | — |

### Ringkasan fase

| Fase selesai | Fase siap | Fase terblokir |
| --- | --- | --- |
| `BD-PH-001`, `BD-PH-002`, `BD-PH-003`, `BD-PH-004` | `BD-PH-005` | `BD-PH-007` |

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
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan per komponen. Struktur penyimpanannya sudah ditutup `DEC-BD-032` (atribut per komponen di katalog); yang belum ada hanya angkanya. | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian | Seluruh perancangan gerbang pemberian tetap jalan; nilainya datang dari konfigurasi katalog |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru sebagai diberikan, setelah pencatatannya dikoreksi. | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi | Konsep catatan koreksi `DEC-BD-030` tetap dapat dirancang penuh |

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
| Kelebihan kiriman PMI (`ARCH-BD-GAP-01`) | `DEC-BD-025` |
| Hasil golongan darah mana yang berlaku (`ARCH-BD-GAP-02`) | `DEC-BD-026` |
| Masa berlaku bukti kecocokan (`ARCH-BD-GAP-03`) | `DEC-BD-027` |
| Gugurnya bukti kecocokan saat pengalihan (`ARCH-BD-GAP-04`) | `DEC-BD-028` |
| Pembatalan alokasi sebelum pemberian (`ARCH-BD-GAP-05`) | `DEC-BD-029` |
| Koreksi pencatatan pemberian (`ARCH-BD-GAP-06`) | `DEC-BD-030` |
| Arti "menyelesaikan perbedaan" hasil golongan darah (`ARCH-BD-GAP-07`) | `DEC-BD-031` |
| Tempat menyimpan masa berlaku bukti kecocokan (`ARCH-BD-GAP-08`) | `DEC-BD-032` |
| Perlakuan fakta biaya saat koreksi menghapus satu-satunya pemberian (`ARCH-BD-GAP-09`) | `DEC-BD-034` |
| Tempat penyelesaian perbedaan hasil golongan darah (`OQ-BD-013`) | `DEC-BD-033` |

## Artefak arsitektur

`03-domain-architecture.md` revisi 3 memuat sembilan bounded context, **dua puluh tiga** konsep
domain, lima aggregate, **empat** invariant lintas aggregate, **tiga** posisi arsitektur, model
relasi, empat model lifecycle, tanggung jawab hak akses, model audit, model integrasi, dampak billing,
dampak keselamatan klinis, peninjauan ulang batas aggregate dan batas ownership, serta catatan
penutupan seluruh gap arsitektur.

**Yang berubah pada revisi 3.** Empat keputusan final `DEC-BD-031` sampai `DEC-BD-034` diserap, dan
ketiga gap yang dibuka revisi 2 (`ARCH-BD-GAP-07`, `08`, `09`) ditutup. Tidak ada konsep domain baru;
empat keputusan itu hanya mempertajam `BD-DOM-22` (wajib pemeriksaan ulang), `BD-DOM-13` (atribut masa
berlaku per komponen), lifecycle `BD-AGG-04`, dan batas terhadap Billing. Tidak ada satu pun batas
aggregate maupun batas ownership yang berpindah, dan status naik ke `DOMAIN_ARCHITECTURE_READY`.

## Bukti yang sudah usang

| Artefak atau bukti | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `9522caa` | Terbatas pada konfigurasi Laboratorium yang berubah — `TrxLabSpecimenConfiguration.cs`, `TrxLabTransitionHistoryConfiguration.cs`, `MstLabRejectionReasonConfiguration.cs`. Dampaknya menyempit setelah `DEC-BD-018` menetapkan sampel Bank Darah terpisah dari sampel Laboratorium. |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `9522caa` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium digantikan `DEC-BD-018`. |

`02-existing-capability-map.md` terikat pada backend `9522caa` dan frontend `afbb8ab`. Backend kini
berada di `db08c14` (lewat `9dc7637`). Pemeriksaan dampak sudah dijalankan pada 2 September 2026:
seluruh perbedaan antara `9522caa` dan `db08c14` hanya berisi dokumen blueprint Bank Darah itu
sendiri, nol berkas source aplikasi. Peta kemampuan **tidak ditandai** `STALE`, dan `BD-CAP-001`
sampai `BD-CAP-024` tetap sahih. Frontend `afbb8ab` tidak berubah.

## Task berikutnya yang disarankan

Architecture gap final closure pass (`grill-me`) dan pass ulang `hospital-domain-architect` revisi 3
sudah dijalankan pada 2 September 2026. Seluruh gap arsitektur tertutup dan status arsitektur
`DOMAIN_ARCHITECTURE_READY`. Langkah berikutnya:

1. **`design-business-module`** untuk seluruh scope yang dinyatakan `READY` pada handoff
   `03-domain-architecture.md` revisi 3 — membekukan arsitektur BE/FE, kamus data, kontrak API,
   state-transition, dan PRD ke MVP.
2. **`grill-me`** hanya bila hendak membuka dua slice yang masih di luar scope: penyerahan biaya ke
   Billing (`DEC-BD-016`, pemilik BillingManagement) dan mekanik label golongan darah (`OQ-BD-011`,
   pemilik proses klinis). Keduanya tidak menahan slice yang sudah siap.

`trace-existing-capabilities` **tidak** perlu diulang. Peta kemampuan masih sahih dan kesepuluh
keputusan closure tidak memunculkan kebutuhan bukti implementasi baru.

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
