# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `6` |
| Module status | `PARTIAL` |
| Current phase | `BD-PH-005` |
| Last verified at | `belum pernah diverifikasi` |
| Backend source SHA | `9dc7637adbafb321ad8078d5c52ebe5e4398fe86` cabang `sukmagp` |
| Frontend source SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Terakhir diperbarui | `2026-09-02` |

Modul tetap `PARTIAL`. Arsitektur domain sudah disusun dan menghasilkan
`DOMAIN_ARCHITECTURE_PARTIAL`. Setelah architecture gap closure pass pada 2 September 2026, enam gap
arsitektur `ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-06` sudah tertutup oleh `DEC-BD-025` sampai
`DEC-BD-030`, sehingga jalur pemberian darah, pengalihan kantong, pembatalan alokasi, koreksi
pemberian, dan aturan hasil golongan darah yang berlaku sudah punya aturan bisnisnya.

Yang masih menahan tinggal penyerahan biaya ke Billing (`DEC-BD-016`) dan mekanik label golongan
darah (`OQ-BD-011`). Tidak ada blocker yang menghentikan seluruh modul.

`03-domain-architecture.md` sudah naik ke revisi 2 dan **sudah** menyerap keenam keputusan itu.
Statusnya tetap `DOMAIN_ARCHITECTURE_PARTIAL`, tetapi isinya berubah besar: yang berhenti bukan lagi
dua slice utuh yang menyangkut keselamatan pasien, melainkan satu perpindahan pada `BD-AGG-04` dan
satu kumpulan atribut pada `BD-DOM-13`. Jalur pemberian, pengalihan, pembatalan alokasi, dan koreksi
pemberian kini boleh diserahkan ke penyusunan blueprint.

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
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan, dan apakah nilainya sama untuk semua komponen darah. Bentuk aturannya sudah dikunci `DEC-BD-027`; yang belum ada hanya angkanya. | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian | Seluruh perancangan gerbang pemberian tetap jalan; nilainya datang dari konfigurasi |
| `OQ-BD-013` | Di mana perbedaan hasil golongan darah diselesaikan. `DEC-BD-026` menuntut ada tempatnya, sedangkan `DEC-BD-023` sudah mengunci MVP pada tepat tiga daftar kerja. | Pemilik proses BDRS | `DESIGN` satu layar saja | Usulan yang tidak memperluas scope: diselesaikan di dalam layar pemeriksaan golongan darah, bukan daftar kerja keempat |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru sebagai diberikan, setelah pencatatannya dikoreksi. | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi | Konsep catatan koreksi `DEC-BD-030` tetap dapat dirancang penuh |
| `ARCH-BD-GAP-07` | Apa artinya "menyelesaikan perbedaan" hasil golongan darah — validator menyatakan salah satu hasil tidak sah, atau wajib ada pemeriksaan ketiga sebagai penengah. | Pemilik proses klinis | Satu perpindahan pada `BD-AGG-04` | Deteksi dan penahanan hasil bertentangan sudah lengkap dan tetap dapat dirancang |
| `ARCH-BD-GAP-08` | Di mana nilai masa berlaku bukti kecocokan disimpan. Bila per komponen, ia menumpang pada katalog komponen darah dan Setup tidak melebar; bila satu angka global, Setup melebar melampaui `DEC-BD-024`. | Pemilik proses klinis bersama BDRS | Pembekuan kumpulan atribut `BD-DOM-13` | Katalog komponen darah tetap dapat dirancang sebagai konsep. Sebaiknya dijawab bersama `OQ-BD-012` |
| `ARCH-BD-GAP-09` | Bila koreksi menyatakan satu-satunya pemberian di bawah sebuah tindakan tidak pernah terjadi, apakah fakta biaya yang terlanjur terkirim perlu ditinjau ulang. | Pemilik BillingManagement | Menempel pada kontrak yang memang sudah tertahan `DEC-BD-016` | Tidak menahan apa pun yang baru |

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

## Artefak arsitektur

`03-domain-architecture.md` revisi 2 memuat sembilan bounded context, **dua puluh tiga** konsep
domain, lima aggregate, **empat** invariant lintas aggregate, **tiga** posisi arsitektur, model
relasi, empat model lifecycle, tanggung jawab hak akses, model audit, model integrasi, dampak billing,
dampak keselamatan klinis, peninjauan ulang batas aggregate dan batas ownership, serta tiga gap
arsitektur baru.

**Yang berubah pada revisi 2.** Enam gap revisi 1 diserap dan ditutup. Lahir tiga konsep domain baru
— `BD-DOM-21` golongan darah sah pasien, `BD-DOM-22` penyelesaian perbedaan hasil, `BD-DOM-23`
catatan koreksi pemberian — ditambah dua invariant lintas aggregate `BD-XINV-03` dan `BD-XINV-04`.
Tidak ada satu pun batas aggregate maupun batas ownership yang berpindah.

## Bukti yang sudah usang

| Artefak atau bukti | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `9522caa` | Terbatas pada konfigurasi Laboratorium yang berubah — `TrxLabSpecimenConfiguration.cs`, `TrxLabTransitionHistoryConfiguration.cs`, `MstLabRejectionReasonConfiguration.cs`. Dampaknya menyempit setelah `DEC-BD-018` menetapkan sampel Bank Darah terpisah dari sampel Laboratorium. |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `9522caa` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium digantikan `DEC-BD-018`. |

`02-existing-capability-map.md` terikat pada backend `9522caa` dan frontend `afbb8ab`. Backend kini
berada di `9dc7637`. Pemeriksaan dampak sudah dijalankan pada 2 September 2026: seluruh perbedaan
antara `9522caa` dan `9dc7637` hanya berisi sepuluh dokumen blueprint Bank Darah itu sendiri, nol
berkas source aplikasi. Peta kemampuan **tidak ditandai** `STALE`, dan `BD-CAP-001` sampai
`BD-CAP-024` tetap sahih. Frontend `afbb8ab` tidak berubah.

## Task berikutnya yang disarankan

Pass ulang `hospital-domain-architect` sudah dijalankan pada 2 September 2026 dan menghasilkan
revisi 2. Dua langkah berikutnya berjalan berdampingan:

1. **`design-business-module`** untuk sembilan slice yang dinyatakan siap pada handoff
   `03-domain-architecture.md` revisi 2, termasuk `BD-AGG-03` yang pada revisi 1 berhenti separuh.
2. **`grill-me`** untuk tiga gap arsitektur baru `ARCH-BD-GAP-07`, `ARCH-BD-GAP-08`, dan
   `ARCH-BD-GAP-09`, sebaiknya digabung dengan `OQ-BD-012` dan `OQ-BD-013`. Khususnya
   `ARCH-BD-GAP-08` dan `OQ-BD-012` sebaiknya dijawab dalam satu tarikan, karena keduanya menanyakan
   hal yang sama dari dua sisi: berapa nilainya, dan di mana ia disimpan.

`trace-existing-capabilities` **tidak** perlu diulang. Peta kemampuan masih sahih dan revisi 2 tidak
memunculkan kebutuhan bukti implementasi baru.

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
