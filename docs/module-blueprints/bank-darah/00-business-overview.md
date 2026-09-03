# Bank Darah — Business Overview

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Revision | `1` |
| Status | `DRAFT` |
| Sumber keputusan | `00-interview-decisions.md` revisi 1 |
| Backend SHA | `9522caacf29371b1fddd1584e9a71ad94fe48d19` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |

Dokumen ini merangkum maksud bisnis modul Bank Darah yang sudah terverifikasi lewat wawancara.
Fakta dipisahkan dari keputusan, asumsi, konflik, dan pertanyaan terbuka. Tidak ada kebijakan,
pemilik, invariant, atau persetujuan yang disimpulkan dari source code.

---

## 1. Untuk apa modul ini ada

Rumah sakit MMC membutuhkan satu tempat yang jelas untuk mengurus kebutuhan darah pasien. Tanpa itu,
beberapa masalah nyata muncul: jumlah kantong yang dipesan, disediakan, dan diberikan bisa tidak
cocok satu sama lain; satu kantong bisa terpakai dua kali bila dua petugas bekerja bersamaan;
perubahan stok sulit ditelusuri; dan pembatalan bisa menghapus riwayat yang seharusnya disimpan.

Bank Darah menjawab itu dengan menjadi satu-satunya tempat yang mencatat perjalanan darah dari
permintaan sampai pemberian.

**Batas modulnya dalam satu kalimat:** Bank Darah mengurus pemenuhan kebutuhan darah pasien, dari
order masuk sampai kantong darah diberikan, dikembalikan, atau ordernya dibatalkan.

## 2. Bagaimana darah sampai ke pasien

```text
Unit pelayanan membuat order darah
  -> Bank Darah memproses order
  -> Bank Darah meminta darah ke PMI atas nama pasien itu
  -> Permintaan diteruskan ke PMI secara manual, di luar sistem
  -> Darah diterima secara fisik oleh petugas Bank Darah
  -> Kantong dicatat, dan stok operasional bertambah di sini
  -> Kantong dialokasikan untuk order pasien
  -> Kantong diberikan kepada pasien
```

Jalur sampingnya: kantong yang sudah dialokasikan atau diberikan bisa dikembalikan, dan order bisa
dibatalkan. Aturan rincinya belum ditetapkan — lihat `DEF-BD-001`.

## 3. Siapa yang berperan

| Pelaku | Tanggung jawab | Catatan |
| --- | --- | --- |
| Dokter atau unit pelayanan asal | Menentukan kebutuhan klinis darah dan membuat permintaan | Pemilik alasan medis. Pada MVP: Rawat Inap, IGD, Rawat Jalan |
| Petugas Bank Darah / BDRS | Memproses permintaan, meminta ke PMI, menerima darah, mengalokasikan, memberikan | Pemilik pemenuhan operasional |
| Dokter BDRS | Penanggung jawab tindakan Bank Darah bila diperlukan | Bukan pihak yang menahan alur |
| PMI | Menyediakan darah | Di luar sistem Quilvian |
| MMC | Rumah sakit pemilik dan pengguna sistem | Bukan pemasok darah |

## 4. Hasil bisnis yang ingin dicapai

| ID | Sasaran | Bunyinya dalam praktik |
| --- | --- | --- |
| BG-BD-001 | Order dapat ditelusuri | Dari pasien, ke order, ke permintaan PMI, ke kantong, sampai pemberian atau pengembalian |
| BG-BD-002 | Stok tetap benar | Satu kantong tidak boleh berada di dua transaksi yang bertentangan |
| BG-BD-003 | Pemenuhan terlihat | Petugas melihat jumlah dipesan, sudah diberikan, dan belum diberikan — dihitung dari transaksi, bukan diketik manual |
| BG-BD-004 | Semua tercatat | Setiap perubahan penting menyimpan pelaku, waktu, rujukan bisnis, dan alasan |
| BG-BD-005 | Data bersama tetap milik pemiliknya | Pasien, kunjungan, dokter, ruangan, kelas, tarif, dan billing tetap dimiliki modul asalnya |
| BG-BD-006 | Backend lebih dulu | Kontrak API, lifecycle, kepemilikan data, dan hak akses dibekukan sebelum frontend dikerjakan |
| BG-BD-007 | Aman sejak awal | Setiap operasi wajib terautentikasi, terotorisasi, divalidasi di server, dan menolak dulu secara bawaan |

## 5. Fakta, keputusan, asumsi, konflik, dan pertanyaan terbuka

| ID | Type | Pernyataan | Owner | Status | Bukti / persetujuan |
| --- | --- | --- | --- | --- | --- |
| `FAK-BD-001` | `Fact` | Tidak ditemukan kapabilitas Bank Darah di backend. Area `Areas/HealthServices/` tidak memiliki folder Blood Bank. | — | terverifikasi sekilas | Pemeriksaan direktori @`9522caa` |
| `FAK-BD-002` | `Fact` | Tidak ditemukan halaman Bank Darah di frontend. Folder `bank` yang ada adalah master data bank finansial. | — | terverifikasi sekilas | Pemeriksaan direktori @`afbb8ab` |
| `FAK-BD-003` | `Fact` | Kontrak response bersama tersedia: `Responses/ApiResponse.cs` dan `Responses/PagedResult.cs`. | — | terverifikasi | Berkas ada @`9522caa` |
| `FAK-BD-004` | `Fact` | Pola otorisasi tersedia: `Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`. | — | terverifikasi | Berkas ada @`9522caa` |
| `FAK-BD-005` | `Fact` | Bank Darah **sudah terdaftar** di registry kepemilikan modul dengan prefix `Bbk`, Lifecycle `PLANNED`. Semula belum terdaftar; ditutup 3 September 2026. | — | terverifikasi | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` commit `ed7fba8`. `PLANNED` memberi wewenang penamaan saja, bukan implementasi |
| `SCOPE-BD-001` | `Decision` | Batas scope MVP dikunci: BR-BD-001..016 ditambah BR-BD-017, BR-BD-018, BR-BD-019 | Pemilik kebutuhan | `confirmed` | Sesi wawancara 2026-09-02 |
| `DEC-BD-001` .. `DEC-BD-012` | `Decision` | Dua belas keputusan bisnis inti | Pemilik proses BDRS | `draft` | `00-interview-decisions.md` §8 |
| `ASM-BD-001` .. `ASM-BD-004` | `Assumption` | Empat asumsi menunggu koreksi | Pemilik proses BDRS | `draft` | `00-interview-decisions.md` §8 |
| `CONF-BD-001` .. `CONF-BD-003` | `Conflict` | Tiga konflik yang sudah diselesaikan | Pemilik proses BDRS | `resolved` | `00-interview-decisions.md` §8 |
| `DEF-BD-001` | `Open Question` | Aturan pengembalian dan pemakaian ulang kantong | Pemilik proses BDRS | terbuka | Ditunda secara sadar |
| `DEF-BD-002` | `Open Question` | Penutupan administratif permintaan PMI yang masih kurang | Pemilik proses BDRS | terbuka | Ditunda secara sadar |
| `BR-BD-011` | `Open Question` | Sumber sah golongan darah dan Rhesus, penjaminnya, syarat pencetakan label | Pemilik proses klinis | terbuka | BRD menandainya `[UNRESOLVED]` |
| `OQ-BD-009` | `Open Question` | Billing tindakan, sampling, Laboratorium, HCLAB, laporan, setup, dan matriks hak akses rinci | Pemilik proses BDRS | terbuka | Belum digali pada scope pass |

`FAK-BD-001` dan `FAK-BD-002` ditandai "terverifikasi sekilas" karena berasal dari pemeriksaan
direktori, bukan dari audit kemampuan yang resmi. Statusnya baru menjadi bukti penuh setelah
`02-existing-capability-map.md` tersedia.

## 6. Yang dikerjakan modul ini, dan yang tidak

### Dikerjakan

Daftar dan detail order darah · tindakan Bank Darah dengan tarif tetap milik Billing · pemantauan
kantong yang sudah masuk proses pelayanan · alokasi kantong ke order · pemberian kantong ·
pemenuhan sebagian · pengembalian kantong · pembatalan order · pencatatan permintaan darah ke PMI ·
pencatatan penerimaan fisik kantong · penerimaan order dari unit pelayanan yang berwenang · hak
akses, jejak audit, dan pengaman terhadap perebutan kantong yang sama.

### Tidak dikerjakan — milik modul lain

| Data atau kemampuan | Modul pemilik |
| --- | --- |
| Pasien dan registrasi | PatientManagement, RegistrationManagement |
| Dokter dan pegawai | Human Resource — Master Data Workforce |
| Ruangan, poli, department, kelas pasien | Master Data |
| Tarif, invoice, pembayaran | BillingManagement |
| Hasil laboratorium umum dan specimen | LaboratoryManagement |

Bank Darah hanya menyimpan rujukan ke data tersebut, tidak menyalinnya.

### Tidak dikerjakan — belum menjadi kebutuhan

Integrasi API PMI · manajemen donor · produksi darah · mesin crossmatch · mesin kesesuaian klinis ·
keputusan klinis otomatis. Ditambah sisa daftar BRD §9: registrasi donor, pengambilan darah donor,
kelayakan donor, skrining penyakit infeksi, karantina, pelepasan klinis kantong, skrining antibodi,
penanganan reaksi transfusi, pemantauan pasca transfusi, penanganan kedaluwarsa, dan pemusnahan.

## 7. Kebutuhan yang tertaut

| ID kebutuhan | Ringkasan | Sumber |
| --- | --- | --- |
| BR-BD-001 sampai BR-BD-016 | Kebutuhan bisnis dasar Bank Darah | `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` |
| FR-BD-001 sampai FR-BD-013 | Kebutuhan fungsional dan kriteria penerimaannya | `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` |
| `BR-BD-017`, `BR-BD-018`, `BR-BD-019` | Tambahan dari hasil wawancara | `00-interview-decisions.md` |

BRD dan PRD mencantumkan baseline SHA `8b298bb`, sementara blueprint ini berdiri di `9522caa`.
Selisihnya hanya menyentuh konfigurasi Entity Framework untuk Radiologi dan Laboratorium beserta
snapshot migration. Tinjauan dampaknya terbatas pada BR-BD-012 dan BR-BD-013, dan dicatat pada
`MODULE-STATUS.md`.

## 8. Risiko bisnis yang sudah dikenali

| Risiko | Kenapa penting | Yang sudah menahannya |
| --- | --- | --- |
| Kantong menganggur terlupakan di lemari pendingin | Darah adalah barang terbatas dan mudah rusak | `DEC-BD-007` mewajibkan daftar pemantauan kantong yang menunggu keputusan |
| Golongan darah yang diminta dikira hasil pemeriksaan | Berpotensi keliru memilih kantong untuk pasien | `INV-BD-011` melarang pemakaiannya untuk keputusan kesesuaian; `ASM-BD-004` menuntut pembedaan tampilan |
| Order ganda dari dua jalur pemesanan | Permintaan ke PMI bisa dobel, kantong datang berlebih | `DEC-BD-005` mendeteksi dari empat penanda sekaligus |
| Permintaan ke PMI hilang saat pasien pulang | Kantong tetap datang tetapi tidak ada yang bertanggung jawab | `DEC-BD-008` melarangnya hilang; mekanismenya menunggu `DEF-BD-002` |
| Kantong pasien lain dipakai karena dianggap stok bebas | Melanggar keterlacakan dan berpotensi salah pasien | `DEC-BD-003` dan `DEC-BD-007` menutup kemungkinan itu |
