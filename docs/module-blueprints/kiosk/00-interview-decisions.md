# Kiosk — Keputusan Wawancara (Interview Decisions)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` |
| Revision | `2` |
| Pass | Scope pass (r1) dan closure pass (r2) — 30 September 2026 |
| Capability map | [01-existing-capability-map.md](01-existing-capability-map.md) r1 |
| Status | `approved` (seluruh keputusan pada pass ini) |
| Pemilik produk/domain & approver | Sukma Giri Pratama (`sukmagp`), atas nama Tim Bisnis — `KSK-DEC-005` |
| SHA backend | `419b910fca5188285946850a95976ebff83ae8ce` |
| SHA frontend | `4ec51b0bf5e724e899b95f118e273351437b71e7` |
| Bukti input | [evidence/2026-09-30-prd-revisi-kiosk.md](evidence/2026-09-30-prd-revisi-kiosk.md) |

> **Catatan audit.** Scope pada pass ini dikunci **sebelum** capability map resmi (`01-existing-capability-map.md`) disusun. Yang tersedia baru audit awal read-only yang dicatat di berkas bukti input. Kemungkinan duplikasi dengan kemampuan existing belum diperiksa secara menyeluruh dan menjadi tugas `trace-existing-capabilities`.

## 1. Scope dan hasil yang diharapkan

**Batas scope:** revisi alur self-service Kiosk — Cek No. RM, urutan Pasien Lama, pemilihan Penjamin Utama, dan dropdown Jadwal Dokter — tanpa mengubah master pasien, asuransi, atau perusahaan.

### Di dalam scope

1. **Cek Nomor Rekam Medis** — menu di halaman utama Kiosk, pencarian via No. KTP atau No. HP, Kartu Pasien, routing ke Pasien Lama atau Pasien Baru (KSK-RM-001..009).
2. **Urutan 8 step Pasien Lama** — Identifikasi → Review Data → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean (KSK-OLD-001..010).
3. **Penjamin Utama** saat Pembayaran untuk pasien yang punya Asuransi dan Perusahaan (KSK-GUA-001/002).
4. **Dropdown Poliklinik & Spesialis** pada Cek Jadwal Dokter (KSK-SCH-001).
5. **Loading, error, retry, dan pencegahan request ganda** pada lookup (KSK-UX-001..003).
6. **Keamanan lookup** — validasi ulang di backend, sanitasi, data minimal, log tanpa KTP/HP lengkap, rate limit (SEC-KSK-001..006).
7. **Session cleanup** — selesai, batal, timeout, kembali ke Home (SEC-KSK-007).
8. **Uji regresi** nama lebih dari tiga kata dan bulan pendek pada tanggal lahir (KSK-BASE-001/002).

### Di luar scope — untuk modul lain

| Hal | Pemilik | Alasan |
| --- | --- | --- |
| Master pasien, master asuransi, master perusahaan | PatientManagement / Master (`Pat`, `Mst`) | PRD §46 |
| Struktur Kartu Pasien, struktur No. RM, merger/penggabungan RM | Rekam Medis (`Mrc`) / PatientManagement | PRD §46; kasus data ganda hanya diarahkan ke petugas (`KSK-DEC-007`) |
| Dukcapil, OCR KTP, biometrik, OTP No. HP | — | PRD §46 |
| Revisi alur Pasien Baru | Kiosk Pasien Baru (existing) | Hanya dipakai sebagai tujuan CTA "Daftar Sebagai Pasien Baru" |
| Aturan internal Laboratorium (panel lab, surat dokter) | Laboratorium (`Lab`) | Titik sentuhnya hanya sesi kiosk (`KSK-DEC-002`) |
| Penggantian penjamin setelah kunjungan dibuat, billing, klaim | Pendaftaran petugas / Billing (`Reg`, `Bil`) | `KSK-DEC-009` |

## 2. Glosarium

| Istilah | Arti yang dapat diuji |
| --- | --- |
| **Lookup No. RM** | Satu permintaan ke backend dengan jenis pencarian (`KTP` atau `PHONE`) dan satu nilai. Hasilnya **hanya** salah satu dari: ditemukan tepat satu pasien, tidak ditemukan, cocok ke lebih dari satu pasien, atau gagal teknis. |
| **Ditemukan** | Backend menemukan **tepat satu** pasien aktif yang `IdentityNumber` (KTP) atau `PhoneNumber` ternormalisasinya sama persis dengan input. |
| **Tidak ditemukan** | Backend menjawab sukses dan tidak ada satu pun pasien yang cocok. Timeout, error jaringan, 5xx, dan 429 **bukan** "tidak ditemukan". |
| **Cocok ganda** | Backend menemukan dua pasien atau lebih untuk nilai yang sama. |
| **Kartu Pasien** | Kumpulan field Kartu Pasien existing (DEC-KSK-002 PRD). Field pastinya diambil dari fitur Cetak Kartu Pasien existing saat trace. |
| **Penjamin Utama kunjungan** | Satu-satunya penanggung yang melekat pada satu kunjungan. Tidak mengubah penanda "utama" di data pasien. |
| **Sesi Kiosk** | Seluruh data pasien yang tersimpan di memori layar Kiosk sejak lookup/identifikasi sampai selesai, batal, timeout, atau kembali ke Home. |

## 3. Aktor dan tanggung jawab

| Aktor | Tanggung jawab |
| --- | --- |
| Pasien (pengguna Kiosk) | Memasukkan KTP/HP, meninjau data, memilih tujuan layanan, jenis kunjungan, pembayaran, penjamin, layanan, dan dokter. |
| Akun perangkat Kiosk | Identitas yang dipakai backend untuk otorisasi (`KioskReadPolicy`) dan batas rate limit. |
| Petugas pendaftaran | Menangani kasus cocok ganda, kasus rate limit berulang, dan perubahan penjamin setelah Konfirmasi. |
| Sukma Giri Pratama | Approver blueprint dan pengambil keputusan bisnis atas nama Tim Bisnis. |

## 4. Aturan bisnis dan invariant

| ID | Aturan |
| --- | --- |
| `KSK-INV-001` | Keputusan pasien lama/baru **selalu** dari jawaban backend, tidak pernah dari cache atau data lokal (KSK-RM-009). |
| `KSK-INV-002` | Gagal teknis (timeout, jaringan, 5xx, 429) **tidak pernah** mengarahkan pasien ke Pendaftaran Pasien Baru (KSK-UX-002/003). |
| `KSK-INV-003` | Kiosk hanya menampilkan kartu pasien ketika hasilnya **tepat satu** pasien (`KSK-DEC-006`, `KSK-DEC-007`). |
| `KSK-INV-004` | Tujuan Layanan tidak pernah tampil sebelum Review Data selesai (KSK-OLD-003/004). |
| `KSK-INV-005` | Satu kunjungan tepat satu penanggung (selaras `RJ-BIL-DEC-015`). Pilihan Penjamin Utama di Kiosk tidak mengubah default pasien (`KSK-DEC-008`). |
| `KSK-INV-006` | KTP dan No. HP tidak pernah masuk ke URL, `localStorage`, atau `sessionStorage`, dan tidak ditulis lengkap ke log backend. |
| `KSK-INV-007` | Sesi kiosk Laboratorium dibentuk **sekali**, setelah Tujuan Layanan diketahui (`KSK-DEC-002`). |

## 5. Keadaan dan perpindahan

### Lookup No. RM

```text
IDLE ─(tekan Cek)─► VALIDATING ─(invalid)─► IDLE + pesan validasi
                         │
                         └─(valid)─► CHECKING ─┬─► FOUND ─────────► Lanjut Pasien Lama (Step 1 terisi)
                                               ├─► NOT_FOUND ─────► Daftar Sebagai Pasien Baru
                                               ├─► MULTIPLE ──────► Arahkan ke KTP / petugas
                                               └─► ERROR ─────────► Coba Lagi (kembali ke CHECKING)
```

Contoh: pasien memasukkan `0812-3456-7890`, dinormalisasi menjadi `6281234567890`. Backend menemukan dua pasien (ibu dan anak) dengan nomor itu, sehingga hasilnya `MULTIPLE`. Kiosk menampilkan "Nomor HP ini terdaftar untuk lebih dari satu pasien. Silakan cari memakai No. KTP atau hubungi petugas pendaftaran." Tidak ada kartu yang ditampilkan dan tidak ada tombol Pasien Baru.

### Pasien Lama (poliklinik)

`Identifikasi → Review Data → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean`

### Pasien Lama (Laboratorium) — akibat `KSK-DEC-002`

`Identifikasi → Review Data → Tujuan Layanan (Laboratorium + surat dokter) → Selesai (handoff lab)`

Sesi kiosk dibentuk ketika pasien menjawab pertanyaan surat dokter di step Tujuan Layanan, bukan ketika kartu dipindai di step Identifikasi.

## 6. Skenario normal dan exception

| Skenario | Perilaku |
| --- | --- |
| KTP valid, satu pasien | Kartu Pasien + "Lanjut Pendaftaran Pasien Lama". |
| KTP/HP tidak terdaftar | "Pasien Belum Terdaftar" + "Daftar Sebagai Pasien Baru". |
| HP dipakai beberapa pasien | Tanpa kartu, arahkan ke KTP/petugas (`KSK-DEC-006`). |
| KTP cocok ke beberapa RM | Tanpa kartu, arahkan ke petugas (`KSK-DEC-007`). |
| Timeout / jaringan / 5xx | "Data pasien belum dapat diperiksa. Silakan coba kembali." + "Coba Lagi". |
| Rate limit terlampaui (429) | "Terlalu banyak percobaan, silakan coba lagi sebentar." Bukan "belum terdaftar" (`KSK-DEC-011`). |
| Tekan Cek dua kali | Satu request saja; tombol terkunci selama request. |
| Tidak ada sentuhan 120 detik | Peringatan hitung mundur 15 detik, lalu data dibersihkan dan kembali ke Home (`KSK-DEC-010`). |
| Pasien ingin ganti penjamin setelah Konfirmasi | Tidak bisa dari Kiosk; ke petugas (`KSK-DEC-009`). |

## 7. Wewenang keputusan frontend

Urutan hierarki: keamanan/privasi/invariant → brief produk/UI yang disetujui (PRD) → konvensi project → diskresi developer.

| Decision ID | Area | Owner | Status | Rentang yang diizinkan | Bukti |
| --- | --- | --- | --- | --- | --- |
| `KSK-UI-001` | Teks pesan dan CTA lookup | PRD | `approved` | Persis teks PRD §15, §33 + teks `KSK-DEC-006/007/011` | PRD |
| `KSK-UI-002` | Letak route halaman Cek No. RM | `DEV_DISCRETION` | `approved` | Di bawah `/kiosk/registration/**`, mengikuti pola route kiosk existing; tanpa KTP/HP di query | Konvensi project |
| `KSK-UI-003` | Tampilan pemilihan KTP/HP | PRD | `approved` | Dua pilihan + satu input yang relevan (PRD §9) | PRD |
| `KSK-UI-004` | Tampilan Penjamin Utama | PRD + `DEV_DISCRETION` | `approved` | Pilihan radio Asuransi/Perusahaan (PRD §27); gaya visual mengikuti step Pembayaran existing | PRD |
| `KSK-UI-005` | Perbaikan dropdown jadwal dokter | `DEV_DISCRETION` | `approved` | Teknik bebas (portal, z-index, dll.) selama AC-SCH-001/002 terpenuhi dan komponen dasar tidak rusak di layar lain | PRD §31 |
| `KSK-UI-006` | Tampilan peringatan timeout | `DEV_DISCRETION` | `approved` | Modal/banner hitung mundur 15 detik dengan tombol "Lanjutkan" | `KSK-DEC-010` |

## 8. Log keputusan

| Decision ID | Jenis | Keputusan / pertanyaan | Owner | Status | Disetujui oleh/pada | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-DEC-001` | Decision | Revisi Kiosk dijalankan lewat jalur blueprint penuh. | Sukma | `approved` | Sukma, 30 Sep 2026 | Sesi ini |
| `KSK-DEC-002` | Decision | Pembuatan sesi kiosk (`TrxKioskScanSession`) ditunda di frontend sampai Tujuan Layanan dipilih setelah Review Data. Kontrak backend `scan-result` tidak berubah. Catatan `FE-LAB-13` / `AC-93` pada blueprint Laboratorium harus direvisi melalui amendment pada blueprint itu. | Sukma | `approved` | Sukma, 30 Sep 2026 | FE `kiosk-service-target-rules.js` |
| `KSK-DEC-003` | Decision | Batas scope pada §1 disetujui, sama dengan PRD §3 dan §46. Alur Pasien Baru hanya dipakai sebagai tujuan CTA. | Sukma | `approved` | Sukma, 30 Sep 2026 | Sesi ini |
| `KSK-DEC-004` | Decision | `blueprint_shape: SINGLE`, `shape_decided_by: USER_CONFIRMED`. Rumpun: (1) Lookup No. RM, (2) Flow Pasien Lama, (3) Penjamin Utama kunjungan, (4) Jadwal Dokter. Hasil uji: keempatnya ada di satu bounded context `RegistrationManagement`/Kiosk, memakai satu policy `KioskReadPolicy`, punya satu pemilik, dan tidak punya kosakata status sendiri. Tidak ada rumpun yang lolos ≥3/5 syarat. Tidak ada kemampuan tanpa rumpun. | Sukma | `approved` | Sukma, 30 Sep 2026 | Sesi ini |
| `KSK-DEC-005` | Decision | Sukma Giri Pratama adalah approver blueprint dan pengambil keputusan bisnis PRD ini atas nama Tim Bisnis. | Sukma | `approved` | Sukma, 30 Sep 2026 | Sesi ini |
| `KSK-DEC-006` | Decision | **Menutup DEC-KSK-001 PRD.** Jika No. HP cocok ke lebih dari satu pasien: tanpa kartu, tampilkan arahan cari dengan No. KTP atau ke petugas, dan jangan arahkan ke Pasien Baru. Backend tidak mengirim data pasien apa pun untuk hasil ini. | Sukma | `approved` | Sukma, 30 Sep 2026 | PRD §47 |
| `KSK-DEC-007` | Decision | Jika No. KTP cocok ke lebih dari satu pasien: tanpa kartu, arahkan ke petugas pendaftaran untuk verifikasi, dan jangan arahkan ke Pasien Baru. Backend tidak mengirim data pasien apa pun. | Sukma | `approved` | Sukma, 30 Sep 2026 | Sesi ini |
| `KSK-DEC-008` | Decision | **Menutup DEC-KSK-003 PRD.** Penjamin Utama yang dipilih di Kiosk hanya berlaku untuk kunjungan ini, dan default pasien tidak berubah. Tombol "Jadikan Utama" existing (mengubah penanda utama level pasien) tidak dipakai lagi di Kiosk. Endpoint `PATCH .../kiosk/{id}/primary` tidak dihapus oleh blueprint ini; statusnya diputuskan saat trace/desain. | Sukma | `approved` | Sukma, 30 Sep 2026 | PRD KSK-GUA-002; `RJ-BIL-DEC-015` |
| `KSK-DEC-009` | Decision | **Menutup DEC-KSK-004 PRD.** Setelah Konfirmasi, penjamin tidak bisa diubah dari Kiosk. Perubahan dilakukan petugas pendaftaran/kasir mengikuti aturan modul pemiliknya (di luar scope). | Sukma | `approved` | Sukma, 30 Sep 2026 | PRD §47 |
| `KSK-DEC-010` | Decision | **Menutup DEC-KSK-005 PRD.** Inactivity timeout 120 detik tanpa sentuhan, didahului peringatan hitung mundur 15 detik yang bisa dibatalkan. Setelah habis, data sesi dibersihkan dan layar kembali ke Home. Berlaku di halaman Cek No. RM dan seluruh step Pasien Lama. | Sukma | `approved` | Sukma, 30 Sep 2026 | SEC-KSK-007 |
| `KSK-DEC-011` | Decision | Rate limit lookup: maksimal 10 permintaan per menit per akun perangkat Kiosk. Bila terlampaui, backend menjawab `429 Too Many Requests` dan Kiosk menampilkan pesan coba lagi, bukan "belum terdaftar". | Sukma | `approved` | Sukma, 30 Sep 2026 | SEC-KSK-006 |
| `KSK-DEC-012` | Decision | Handoff Cek No. RM → Pasien Lama: Step 1 Identifikasi tampil sudah terisi dengan pasien hasil lookup, lalu pasien menekan Lanjut ke Review Data. Pasien dibawa lewat state memori, bukan URL, dan detail pasien diambil ulang dari backend memakai `patientId`. | Sukma | `approved` | Sukma, 30 Sep 2026 | AC-OLD-001; PRIV-3 |
| `KSK-FACT-001` | Fact | Tile Cek No. RM sudah ada di home tanpa route. | — | — | — | Bukti input §4 |
| `KSK-FACT-002` | Fact | Tidak ada endpoint lookup KTP/HP khusus; backend tanpa rate limiter. | — | — | — | Bukti input §4 |
| `KSK-FACT-003` | Fact | Urutan step sekarang `serviceTarget → find → review → …`, disengaja karena sesi kiosk lab ditulis sekali saat scan. | — | — | — | Bukti input §4 |
| `KSK-ASM-001` | Assumption | Kunjungan (`PatientEncounter`) sudah dapat menyimpan satu penanggung per kunjungan sehingga `KSK-DEC-008` tidak butuh kolom baru. **Wajib diverifikasi** oleh `trace-existing-capabilities`. | — | `draft` | — | `RJ-BIL-DEC-015` |
| `KSK-ASM-002` | Assumption | Field Kartu Pasien diambil dari fitur Cetak Kartu Pasien existing. **Wajib diverifikasi** saat trace. | — | `draft` | — | PRD DEC-KSK-002 |
| `KSK-ASM-003` | Assumption | Aturan normalisasi No. HP di backend sudah ada atau bisa diturunkan dari cara `MstPatient.PhoneNumber` disimpan. **Wajib diverifikasi** saat trace. | — | `draft` | — | KSK-RM-004 |

## 9. Acceptance criteria yang sudah dapat diuji

Selain AC-RM-001..005, AC-OLD-001..008, AC-GUA-001/002, dan AC-SCH-001/002 dari PRD:

| ID | Given / When / Then |
| --- | --- |
| `KSK-AC-001` | **Given** No. HP terdaftar untuk dua pasien **When** lookup HP **Then** tidak ada kartu, tampil arahan KTP/petugas, dan tidak ada CTA Pasien Baru. |
| `KSK-AC-002` | **Given** No. KTP cocok ke dua RM **When** lookup KTP **Then** tidak ada kartu, tampil arahan petugas, dan tidak ada CTA Pasien Baru. |
| `KSK-AC-003` | **Given** pasien punya Asuransi A (utama di data pasien) dan Perusahaan B **When** memilih B sebagai penjamin utama di Kiosk dan kunjungan dibuat **Then** kunjungan dijamin B **And** penanda utama di data pasien tetap A. |
| `KSK-AC-004` | **Given** 11 lookup dalam satu menit dari satu perangkat **When** lookup ke-11 **Then** backend `429` **And** Kiosk tidak menampilkan "Pasien Belum Terdaftar". |
| `KSK-AC-005` | **Given** layar Review Data tanpa sentuhan 120 detik **Then** peringatan 15 detik muncul **And** bila tidak dibatalkan, data pasien hilang dari state dan layar kembali ke Home. |
| `KSK-AC-006` | **Given** pasien memilih Laboratorium di step Tujuan Layanan (setelah Review) **When** menjawab pertanyaan surat dokter **Then** tepat satu sesi kiosk terbentuk dengan `targetService` dan `hasPhysicianRequest` terisi. |
| `KSK-AC-007` | **Given** lookup ditemukan **When** tekan "Lanjut Pendaftaran Pasien Lama" **Then** Step 1 menampilkan pasien itu **And** URL tidak memuat KTP, HP, maupun `patientId`. |
| `KSK-AC-008` | **Given** log backend setelah lookup **Then** nilai KTP/HP tidak tercatat utuh (paling banyak 4 digit terakhir). |

## 10. Open question dan blocker

| ID | Pertanyaan | Owner | Memblokir | Status |
| --- | --- | --- | --- | --- |
| `KSK-OQ-001` | Verifikasi `KSK-ASM-001` (penanggung per kunjungan). | `trace-existing-capabilities` | — | **Ditutup** oleh `KSK-FACT-004` |
| `KSK-OQ-002` | Verifikasi `KSK-ASM-002` (field Kartu Pasien). | `trace-existing-capabilities` | — | **Ditutup** oleh `KSK-FACT-005` |
| `KSK-OQ-003` | Verifikasi `KSK-ASM-003` (normalisasi HP di backend). | `trace-existing-capabilities` | — | **Ditutup** oleh `KSK-FACT-006` |
| `KSK-OQ-004` | Amendment blueprint Laboratorium untuk `FE-LAB-13` / `AC-93` akibat `KSK-DEC-002` dan `KSK-DEC-014`. | Sukma | IMPLEMENTATION (FE urutan step) | **Ditutup** 30 Sep 2026 — amendment tercatat di `laboratorium/00-interview-decisions.md` Amendment Pass putaran 20 (revisi 82) dan `laboratorium/03-frontend-architecture.md` §Amandemen 2026-09-30, atas persetujuan Sukma |
| `KSK-OQ-005` | Catatan amendment `RWI-ENC-PAYER-001` v1.0.0 → v1.1.0 pada blueprint rawat-inap (`encounter-company-guarantor-contract.md` §7), sesuai `KSK-DEC-013`. | Sukma / Muhammad Hamzah | IMPLEMENTATION (BE route kiosk menerima Penjamin Perusahaan) | **Ditutup** 30 Sep 2026 — `RWI-ENC-PAYER-001` `1.1.0` tercatat di `rawat-inap/episode-rawat-inap/contracts/encounter-company-guarantor-contract.md` §7 dan §11 atas permintaan Sukma |

Tidak ada keputusan bisnis PRD yang masih OPEN. Seluruh `DEC-KSK-00x` sudah ditutup oleh `KSK-DEC-006..010`. Seluruh conflict dan unknown pada capability map r1 sudah ditutup oleh `KSK-DEC-013..019` (§11).

## 11. Closure pass (revision 2) — 30 September 2026

Masukan: capability map r1 (`KSK-CONF-001..004`, `KSK-UNK-001..004`), dan query **baca-saja** ke `QuilvianNewDevSukma` atas izin Sukma. Query hanya mengembalikan jumlah per pola; tidak ada nilai HP, KTP, atau nama yang dicetak.

### 11.1 Fakta baru

| ID | Jenis | Fakta | Bukti |
| --- | --- | --- | --- |
| `KSK-FACT-004` | Fact | Satu kunjungan sudah menyimpan tepat satu penanggung (`RegPatientEncounterGuarantor`, berisi `PatientInsuranceId` atau `PatientCompanyGuarantorId`). `KSK-ASM-001` terbukti. | Capability map `KSK-CAP-030` |
| `KSK-FACT-005` | Fact | Field Kartu Pasien: Nama, No. RM, Kode Pasien, Tipe Pasien, Jenis Kelamin, Golongan Darah, QR (nama + No. RM). `KSK-ASM-002` terbukti. | `KSK-CAP-008` |
| `KSK-FACT-006` | Fact | Backend **tidak** menormalisasi HP pasien. Di dev (16 pasien tidak terhapus): 1 nomor `08…`, 8 nomor `+628…`, 7 nomor `+62` diikuti angka selain 8 (diduga telepon rumah/kantor); tidak ada spasi/strip. `KSK-ASM-003` gugur. | Query baca-saja 30 Sep 2026; `KSK-CAP-004` |
| `KSK-FACT-007` | Fact | 3 nomor HP (setelah normalisasi) dipakai 2 pasien → 6 pasien terdampak `KSK-DEC-006`. | Query baca-saja 30 Sep 2026 |
| `KSK-FACT-008` | Fact | 2 dari 16 nomor identitas bukan 16 digit. Seluruh pasien dev berstatus Active, tidak ada yang meninggal/digabung. | Query baca-saja 30 Sep 2026 |
| `KSK-FACT-009` | Fact | KTP unik di antara pasien tidak terhapus, sehingga `KSK-DEC-007` hanya berperan sebagai pengaman. | `KSK-CAP-002` |

Catatan: database dev kecil dan tidak mewakili variasi data produksi. Desain normalisasi wajib tahan terhadap spasi, strip, titik, dan kurung walaupun tidak ditemukan di dev.

### 11.2 Keputusan

| Decision ID | Jenis | Keputusan | Owner | Status | Disetujui oleh/pada | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-DEC-013` | Decision | **Menutup `KSK-CONF-001`.** Route kiosk `POST .../patient-encounters/kiosk` **boleh** menerima `paymentType = 3` (Penjamin Perusahaan) dengan `patientCompanyGuarantorId`, memakai validasi yang sama dengan route admin. Larangan `RWI-ENC-PAYER-001` §7 dicabut untuk route kiosk. Menurut pemilik kontrak, perubahan admisi rawat inap soal penjamin justru mengikuti Kiosk, dan ia tidak melarang aturan tersebut. Defect `KSK-CAP-032` (tab Perusahaan berakhir error) diperbaiki dalam revisi ini. | Muhammad Hamzah (pemilik `RWI-ENC-PAYER-001`) | `approved` | Muhammad Hamzah, disampaikan langsung melalui Sukma Giri Pratama, 30 Sep 2026 | Sesi ini; pencatatan formal di blueprint rawat-inap = `KSK-OQ-005` |
| `KSK-DEC-014` | Decision | **Menutup `KSK-CONF-002`, memperinci `KSK-DEC-002`.** Sesi kiosk (`TrxKioskScanSession`) dibentuk **tepat sekali**, saat Tujuan Layanan dipilih di Step 3, untuk **semua** tujuan (poliklinik dan laboratorium). Step 1 mengenali kartu lewat pencarian tanpa membentuk sesi, dan hasil pindaian disimpan di memori untuk dikirim saat sesi dibentuk. Kontrak `scan-result` tidak berubah. Kartu yang hanya dapat dikenali lewat nomor asuransi/member (via `FindPatientAsync`) wajib punya jalur pengenalan setara di Step 1; caranya ditentukan saat desain. | Sukma | `approved` | Sukma, 30 Sep 2026 | `KSK-CAP-021`, `KSK-CAP-022` |
| `KSK-DEC-015` | Decision | **Menutup `KSK-CONF-003`.** Tanggal lahir di Review Data dan Konfirmasi Pasien Lama memakai bulan pendek, contoh `12 Sep 1990`. KSK-BASE-002 berlaku sebagai target, bukan klaim existing. | Sukma | `approved` | Sukma, 30 Sep 2026 | `KSK-CAP-027` |
| `KSK-DEC-016` | Decision | **Menutup `KSK-CONF-004`.** Diperbaiki dalam revisi ini: (1) `console.log` yang mencetak URL/diagnostik/body respons pasien di service Kiosk dihapus; (2) input Step 1 berbentuk KTP (16 digit) atau No. HP dialihkan ke endpoint lookup POST yang sama dengan Cek No. RM, sedangkan pencarian nama/No. RM tetap memakai jalur existing. | Sukma | `approved` | Sukma, 30 Sep 2026 | `KSK-CAP-024`; `KSK-INV-006` |
| `KSK-DEC-017` | Decision | **Menutup `KSK-UNK-001`.** Lookup menganggap "ditemukan" hanya pasien `PatientStatus = Active`, `IsActive = true`, dan belum dihapus. Pasien `Merged` diikuti ke `MergedToPatientId`; bila pasien tujuan Aktif, kartu pasien tujuan yang ditampilkan. Status lain (Tidak Aktif, Meninggal, Diblokir, atau Merged dengan tujuan tidak aktif) menghasilkan pesan netral "Silakan hubungi petugas pendaftaran" **tanpa alasan** dan tanpa CTA Pasien Baru. Backend tidak mengirim data pasien untuk hasil ini. | Sukma | `approved` | Sukma, 30 Sep 2026 | `KSK-FACT-008`; PRIV-4 |
| `KSK-DEC-018` | Decision | **Menutup `KSK-UNK-002`.** Jalur HP menerima seluruh nomor Indonesia, baik seluler maupun telepon rumah/kantor, dalam bentuk `0…`, `62…`, atau `+62…`. Normalisasi: buang semua karakter selain digit, ganti awalan `0` menjadi `62`, lalu wajib diawali `62` dengan panjang 9–15 digit. Pencocokan memakai bentuk ternormalisasi di **kedua sisi** (input dan data tersimpan), karena data tersimpan tidak seragam (`KSK-FACT-006`). Teknik penyimpanan/index ditentukan saat desain. | Sukma | `approved` | Sukma, 30 Sep 2026 | KSK-RM-004 |
| `KSK-DEC-019` | Decision | Validasi KTP tetap tepat 16 digit (KSK-RM-003). Pasien dengan nomor identitas non-KTP (paspor/KITAS) memakai jalur No. HP atau menemui petugas. Metode pencarian ketiga **tidak** ditambahkan. | Sukma | `approved` | Sukma, 30 Sep 2026 | `KSK-FACT-008`; KSK-RM-002 |
| `KSK-DEC-020` | Decision | Kartu Jenis Kunjungan dan Pembayaran tanpa baris ceklis. Kartu poli menampilkan nama lengkap saja; baris unit layanan/jenis klinik ("Rawat Jalan \| Spesialis") dan estimasi menit dihapus; lokasi pindah ke kiri chip singkatan poli. Berlaku untuk Pasien Lama dan Pasien Baru. | Sukma | `approved` | Sukma, 1 Okt 2026 (catatan pemilik 16.07) | `FE-KSK-009` |
| `KSK-DEC-021` | Decision | Masa berlaku kartu penjamin berupa tanggal "Berlaku s/d" yang disimpan ke `effectiveEndDate` asuransi/penjamin perusahaan pasien saat disimpan. Penjamin tersimpan membaca tanggal itu (read-only) dan tidak dipilih ulang; kartu kedaluwarsa ditolak. Pilihan "kurang/lebih dari 1 tahun" dihapus dari Kiosk. | Sukma | `approved` | Sukma, 1 Okt 2026 (opsi rekomendasi dipilih di sesi agent) | `FE-KSK-010` |
| `KSK-ASM-001` | Assumption | — | — | `superseded` oleh `KSK-FACT-004` | — | — |
| `KSK-ASM-002` | Assumption | — | — | `superseded` oleh `KSK-FACT-005` | — | — |
| `KSK-ASM-003` | Assumption | — | — | `rejected` oleh `KSK-FACT-006` | — | — |

### 11.3 Invariant tambahan

| ID | Aturan |
| --- | --- |
| `KSK-INV-008` | Setiap kunjungan dari Kiosk Pasien Lama terhubung ke **tepat satu** sesi kiosk yang membawa `TargetService` benar (`KSK-DEC-014`). |
| `KSK-INV-009` | Lookup tidak pernah membocorkan status non-aktif pasien (meninggal/diblokir) kepada pengguna Kiosk (`KSK-DEC-017`). |

### 11.4 Acceptance criteria tambahan

| ID | Given / When / Then |
| --- | --- |
| `KSK-AC-009` | **Given** pasien punya Asuransi dan Perusahaan **When** memilih Perusahaan sebagai penjamin utama lalu Konfirmasi **Then** kunjungan terbentuk dengan `paymentType = 3` dan `patientCompanyGuarantorId` terisi **And** tidak ada error "Asuransi pasien belum dipilih". |
| `KSK-AC-010` | **Given** pasien memindai kartu di Step 1 lalu memilih Poliklinik di Step 3 **When** kunjungan dibuat **Then** tercatat tepat satu sesi kiosk untuk pasien itu, dan sesi itu terhubung ke kunjungan. |
| `KSK-AC-011` | **Given** pasien memindai kartu di Step 1 lalu memilih Laboratorium di Step 3 **Then** tepat satu sesi kiosk dengan `TargetService = Laboratory` terbentuk **And** tidak ada sesi tanpa target untuk pasien itu dari perjalanan yang sama. |
| `KSK-AC-012` | **Given** pasien lahir 12 September 1990 **Then** Review Data dan Konfirmasi menampilkan `12 Sep 1990`. |
| `KSK-AC-013` | **Given** pasien mengetik KTP 16 digit di Step 1 **Then** tidak ada request GET yang memuat KTP di URL **And** console browser tidak memuat data pasien. |
| `KSK-AC-014` | **Given** KTP milik pasien berstatus Meninggal **When** lookup **Then** tampil "Silakan hubungi petugas pendaftaran" tanpa kata "meninggal", tanpa kartu, dan tanpa CTA Pasien Baru. |
| `KSK-AC-015` | **Given** pasien A digabung ke pasien B yang Aktif **When** lookup KTP milik A **Then** kartu pasien B yang tampil. |
| `KSK-AC-016` | **Given** HP tersimpan `+6281234567890` **When** input `0812-3456-7890` **Then** ditemukan. **Given** HP tersimpan `+62215551234` **When** input `021 555 1234` **Then** ditemukan. |
| `KSK-AC-017` | **Given** input KTP `A1234567890123456` atau 15 digit **Then** pesan "Nomor KTP harus terdiri dari 16 digit." dan tidak ada request ke backend. |

## 12. Amandemen 8 Oktober 2026 — Revisi Pendaftaran Pasien manual

Sumber: Sukma Giri, 8 Okt 2026, PDF "Pendaftaran pasien manual". Pemilik memilih cakupan "Petugas RJ +
Kiosk" dan "Kiosk ikut semua poin" untuk jalur A (frontend saja). Keputusan sisi petugas RJ dan
jalur B tercatat di blueprint Rawat Jalan (`RJ-DOC-DEC-062`..`067`). Cari pasien Kiosk sudah menjadi
acuan (`KSK-DEC-016`/`018`) dan tidak berubah.

| Decision ID | Jenis | Keputusan | Owner | Status | Disetujui oleh/pada | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-DEC-022` | Decision | Pada step Pilih Layanan Tujuan (Pasien Lama dan Baru): pesan "Silakan pilih poliklinik terlebih dahulu" berwarna merah; daftar dokter dapat dicari dan menampilkan nama serta info praktik. No. HP/WhatsApp pada form Pasien Baru maksimal 13 angka lokal (`08…`), yaitu 14 digit dalam bentuk `+62…` yang disimpan Kiosk; normalizer lokal Kiosk diganti normalizer bersama `normalizeIndonesianPhoneNumber`. Kontak darurat tetap 15 digit | Sukma | `approved` | Sukma, 8 Okt 2026 | `FE-KSK-012` |
| `KSK-DEC-023` | Decision | *(superseded oleh `KSK-DEC-025`)* Jenis Kunjungan (Pasien Umum / Pasien Rujukan) pindah ke step Pilih Layanan Tujuan, ringkas di samping jadwal dokter, Umum di atas Rujukan. Step Pilih Jenis Pasien dihapus dari Pasien Lama dan Pasien Baru, sehingga jumlah step berkurang satu. Alur Laboratorium tidak berubah | Sukma | `superseded` | Sukma, 8 Okt 2026 (pilihan "Kiosk ikut semua poin") | `FE-KSK-013` |
| `KSK-DEC-024` | Approval | `IMPLEMENTATION_AUTHORITY` `GRANTED`, `TASK MODE: FRONTEND`, untuk `FE-KSK-012` dan `FE-KSK-013`. Wajib memakai base component dan util normalizer yang sudah ada. Tanpa backend, migration, commit, push, merge, deploy | Sukma | `approved` | Sukma, 8 Okt 2026 | — |
| `KSK-DEC-025` | Decision | **Menggantikan `KSK-DEC-023`.** Jenis Kunjungan Kiosk tetap step tersendiri sebelum memilih layanan, karena pilihan Umum/Rujukan menentukan apakah step Rujukan muncul (`RJ-DOC-DEC-072`). `FE-KSK-013` dibatalkan. Step Rujukan Kiosk mengikuti `RJ-DOC-DEC-076` | Sukma | `approved` | Sukma, 8 Okt 2026 (pilihan "Tetap step sendiri sebelum layanan") | — |
