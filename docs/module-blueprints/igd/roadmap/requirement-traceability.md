# Requirement Traceability — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 1
status: DRAFT
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 4
  01-existing-capability-map.md: 2
contract_versions:
  - "API 0.2.0"
  - "State 0.2.0"
  - "Validation 0.2.0"
  - "Integration 0.2.0"
  - "Permission/Audit 0.2.0"
source_commits:
  backend: "e5331a015fa416a89454b435de0014455f0326d8"
  frontend: "08c84d371ed90640189ce1758019184b0a955e13"
```

Dokumen ini menjawab satu pertanyaan: **setiap keputusan yang sudah diambil, dikerjakan oleh
task mana dan dibuktikan oleh test mana.** Kalau ada baris yang kolom task atau kolom test-nya
kosong, itu bukan kelalaian penulisan, melainkan temuan yang memang harus terlihat.

---

## 1. Keputusan yang tertutup roadmap ini

| Requirement | Decision ID | Design/ERD | Contract | Backend task | Frontend task | Test/bukti | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Modul IGD dapat dipanggil saat aplikasi berjalan | `CAP-16` | `01-existing-capability-map.md#capability-register` | API `0.2.0` | `BE-IGD-001` | — | Build lulus + penelusuran constructor lengkap ([laporan](../../../hamzah/report/be-igd-001-registrasi-service-igd.md)); test aktivasi controller **belum ada** | In progress — kode terdaftar, bukti runtime tertunda |
| Target waktu yang belum diatur tidak dianggap nol menit | `IGD-DEC-027`, `IGD-DEC-035` | `contracts/validation-matrix.md#2-triage` | Validation `0.2.0` | `BE-IGD-002` | `FE-IGD-003` | Build lulus + migration terbentuk ([laporan](../../../hamzah/report/be-igd-002-target-waktu-boleh-kosong.md)); `AT-IGD-011` dan `AT-IGD-012` **belum ada** | In progress — kode dan migration selesai, uji berjalan tertunda; penyimpangan 1 migration menunggu pengesahan owner |
| Data master IGD tersedia sehingga modul dapat dipakai | `IGD-DEC-047`, `IGD-DEC-048` | `02-backend-architecture.md#7-rencana-data-master-awal` | — | `BE-IGD-003` | `FE-IGD-004`, `FE-IGD-008` | Build lulus ([laporan](../../../hamzah/report/be-igd-003-seeder-master-data-igd.md)); `AT-IGD-070` dan `AT-IGD-073` **belum ada** | Blocked sebagian — 50 baris siap di-seed, baris Hitam terhalang `CK_MstEmergencyTriageLevel_Level`; menunggu keputusan owner |
| Retriage append-only dan menunjuk penilaian sebelumnya | `IGD-DEC-004`, `IGD-DEC-048` | `erd/emergency-episode.md` | API `0.2.0`, State `0.2.0` | `BE-IGD-004` | `FE-IGD-004` | Build lulus ([laporan](../../../hamzah/report/be-igd-004-retriage-append-only.md)); `AT-IGD-013` sampai `AT-IGD-015` **belum ada** | In progress — endpoint selesai, uji berjalan tertunda; `CG-07` (`PUT` masih dapat menimpa) sengaja belum ditutup |
| Pelampauan target respons tercatat pada penilaian | `IGD-DEC-027`, `IGD-GAP-007` | `erd/data-dictionary.md#62` | — | `BE-IGD-005` | — | Kode, migration, dan snapshot selesai ([laporan](../../../hamzah/report/be-igd-005-penanda-pelampauan-sla.md)); **build belum dijalankan**, uji migration maju dan mundur **belum ada** | In progress — migration ditulis tangan dan belum dikompilasi; uji tertahan karena `DefaultConnection` menunjuk DB dev bersama tim, bukan lokal |
| Sistem menandai sendiri pasien yang terlambat ditangani | `IGD-DEC-027`, `IGD-GAP-007` | `contracts/integration-contract.md#proses-terjadwal-di-dalam-aplikasi` | Integration `0.2.0` | `BE-IGD-006` | — | Hosted service + pemindaian selesai; **build belum dijalankan**, `AT-IGD-020` sampai `AT-IGD-023` **belum ada** | In progress — kode selesai dan terdaftar, frekuensi dapat dikonfigurasi; belum dikompilasi maupun dijalankan |
| Daftar pasien yang melampaui batas dapat diambil | `IGD-DEC-027` | `contracts/api-contract.md` | API `0.2.0` | `BE-IGD-007` | `FE-IGD-003` | Endpoint `GET /sla-breaches` selesai; **build belum dijalankan**, `AT-IGD-024` **belum ada** | In progress — kode selesai, kolom sensitif dikecualikan dari balasan; belum dikompilasi |
| Penyelesaian klinis terpisah dari penetapan tindak lanjut | `IGD-DEC-049`, `IGD-GAP-001` | `contracts/state-transition-matrix.md#1` | State `0.2.0` | `BE-IGD-008` | `FE-IGD-005` | `AT-IGD-035` | Planned |
| Kunjungan hanya selesai bila closure gate terpenuhi | `IGD-DEC-049` | `contracts/validation-matrix.md#3` | API `0.2.0`, Validation `0.2.0` | `BE-IGD-009` | `FE-IGD-006` | `AT-IGD-030`, `AT-IGD-032`, `AT-IGD-033`, `AT-IGD-034` | Planned |
| Billing yang belum final tidak menahan penyelesaian klinis | `IGD-DEC-021` | `contracts/validation-matrix.md#3` | Validation `0.2.0` | `BE-IGD-009` | `FE-IGD-006` | `AT-IGD-031` | Planned |
| Pemeriksaan akses mengenal unit dan sumber daya | `IGD-DEC-026`, `IGD-GAP-006` | `contracts/permission-audit-matrix.md#3` | Permission `0.2.0` | `BE-IGD-010` | `FE-IGD-009` | `AT-IGD-041` | Planned |
| Akses darurat klinis tercatat dan berbatas waktu | `IGD-DEC-050`, `IGD-FACT-010` | `contracts/permission-audit-matrix.md#rincian-kebutuhan-3` | Permission `0.2.0` | `BE-IGD-011` | — | Belum ada `AT-IGD-*`; test dibuat pada task | Planned |
| Kewenangan SuperAdmin dipisahkan menurut jenis endpoint | `IGD-DEC-050`, `IGD-CONFLICT-003` | `contracts/permission-audit-matrix.md#rincian-kebutuhan-1` | Permission `0.2.0` | `BE-IGD-012` | — | `AT-IGD-052`, `AT-IGD-053` | Planned, aktivasi tertahan |
| Modul IGD tidak meniru penyimpangan struktur folder | `DEC-RSK-003` | `02-backend-architecture.md#4-arsitektur-folder` | — | `BE-IGD-013` | — | Regression seluruh test | Planned |
| Frontend memakai tipe encounter canonical | `IGD-DEC-041`, `CAP-17` | `01-existing-capability-map.md#as-is-contract` | API `0.2.0` | — | `FE-IGD-001` | Test pembentukan payload | Planned |
| Petugas dapat melihat pasien yang sedang di IGD | `IGD-DEC-046` | `03-frontend-architecture.md#2-layar-yang-dibutuhkan` | API `0.2.0` | — | `FE-IGD-002` | Test komponen tujuh keadaan layar | Planned |
| Perawat mencatat pemantauan berkala tanpa duplikasi data klinis | `IGD-DEC-003` | `02-backend-architecture.md#1-kepemilikan-data` | API `0.2.0` | — | `FE-IGD-007` | Test komponen rujukan tanda vital | Planned |
| Tindak lanjut terpisah dari eksekusi perpindahan | `IGD-DEC-005` | `contracts/state-transition-matrix.md#3` | State `0.2.0` | — | `FE-IGD-008`, `FE-IGD-009` | `AT-IGD-040`, `AT-IGD-042` | Planned |
| Seluruh perjalanan pasien terlihat dalam satu halaman | `IGD-DEC-046` | `03-frontend-architecture.md#2-layar-yang-dibutuhkan` | API `0.2.0` | — | `FE-IGD-010` | Test komponen kegagalan sebagian | Planned |
| Bukti penerimaan lintas-slice tersedia | seluruh `AT-IGD-*` | `testing/acceptance-test-matrix.md` | — | `BE-IGD-014` | — | Laporan per `AT-IGD-*` | Planned |

---

## 2. Test penerimaan yang belum punya task

Ketiga baris berikut ada pada acceptance test matrix tetapi tidak dapat dijalankan oleh satu
pun task pada roadmap revisi ini.

| Test | Skenario | Mengapa belum dapat dijalankan | Coverage gap |
| --- | --- | --- | --- |
| `AT-IGD-002` | Membuat kunjungan untuk pasien tidak dikenal tanpa `PatientId` | `TrxEmergencyVisit` memang mengizinkan pasien kosong, tetapi encounter yang menaunginya mewajibkan pasien. Jalur utuhnya belum dirancang | `CG-01` |
| `AT-IGD-003` | Pasien gawat mulai ditangani sebelum registrasi selesai | Sama seperti di atas; jalur provisional belum dirancang | `CG-01` |
| `AT-IGD-017` | Aplikasi mencoba menetapkan kategori Hitam secara otomatis | Baris Hitam baru ada setelah `BE-IGD-003`, tetapi larangan penetapan otomatisnya belum punya tempat penegakan di kode | `CG-07` |

Ketiganya **tidak** boleh dianggap lulus hanya karena belum diuji, sesuai penutup acceptance
test matrix.

---

## 3. Requirement tanpa task sama sekali

Ringkasan dari bagian 7 `backend-roadmap.md`, ditulis ulang di sini supaya traceability dapat
dibaca berdiri sendiri.

| Coverage gap | Requirement | Decision ID | Status capability | Langkah berikutnya |
| --- | --- | --- | --- | --- |
| `CG-01` | Encounter provisional dan pasien tidak dikenal | `IGD-DEC-002`, `IGD-DEC-016` | `CAP-02` `Conflict` | `/design-business-module` |
| `CG-02` | Mode korban massal atau bencana | `IGD-DEC-009` | `CAP-19` `Missing` | `/grill-me` lalu `/design-business-module` |
| `CG-03` | Lifecycle dan penggabungan identitas pasien sementara | `IGD-DEC-017`–`019`, `IGD-DEC-031` | `CAP-03` `Repair` | `/design-business-module` |
| `CG-04` | Serah terima billing saat kunjungan selesai | `IGD-DEC-021` | `CAP-09` `Missing` | `/design-business-module` |
| `CG-05` | Tindak lanjut hasil penunjang terlambat | `IGD-DEC-024`, `IGD-DEC-032` | `CAP-11` `Missing` | `/grill-me` |
| `CG-06` | Outbox, inbox, dan rekonsiliasi lintas modul | `IGD-DEC-025`, `IGD-DEC-033` | `CAP-14` `Missing` | Tinjau saat integrasi eksternal pertama muncul |
| `CG-07` | Perlindungan sifat append-only riwayat klinis | `IGD-DEC-029` | `CAP-04`, `CAP-07`, `CAP-08` `Repair` | `/design-business-module` |

Perbandingan yang jujur: decision log memuat sekitar 50 keputusan bernomor. Roadmap revisi ini
menutup keputusan yang dirancang pada blueprint revision 4, yaitu seputar triase, penyelesaian
kunjungan, dan otorisasi. Tujuh kelompok besar di atas belum tertutup, dan sebagian besarnya
menunggu desain, bukan menunggu pengetikan kode.

---

## 4. Gate go-live yang belum terpenuhi

| Gate | Menunggu | Task yang terdampak | Sifat |
| --- | --- | --- | --- |
| SOP triase MMC untuk target level 2 sampai 5 | Pemilik SOP MMC | `BE-IGD-003` isi master, `FE-IGD-003` tampilan | Menahan pengisian data, tidak menahan kode |
| Security/privacy owner | Sponsor governance MMC | `BE-IGD-011`, `BE-IGD-012` | Menahan aktivasi produksi, tidak menahan pembangunan |
| Clinical governance owner | Sponsor governance MMC | `BE-IGD-003`, `FE-IGD-003`, `FE-IGD-004` | Menahan pengesahan klinis; baseline regulasi tetap berlaku sementara |
| `GovernanceAssignment` bernama | Sponsor governance MMC | Seluruh klaim approval formal | Menahan bukti approval, tidak menahan delivery |

---

## 5. Cara memakai dokumen ini

1. Setiap kali sebuah task selesai, ubah kolom **Status** baris terkait dari `Planned` menjadi
   `Done` dan cantumkan bukti test yang benar-benar dijalankan.
2. Jangan menandai `Done` tanpa bukti. Task yang selesai secara kode tetapi belum punya bukti
   test berstatus `In Progress`, bukan `Done`.
3. Bila muncul requirement baru, tambahkan barisnya lebih dulu di sini. Baris tanpa task adalah
   informasi yang berguna, bukan cacat dokumen.
4. Bila salah satu SHA sumber berubah, jalankan impact scan sebelum memakai roadmap ini untuk
   implementasi, sesuai aturan pada `01-existing-capability-map.md` bagian Impact Scan Trigger.
