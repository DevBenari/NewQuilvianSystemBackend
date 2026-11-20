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

## 13. Amandemen 19 November 2026 — Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

Sumber requirement: Sukma Giri, 19 Nov 2026 — *"buatkan 1 page tanpa harus login untuk Pendaftaran pasien online flow nya sama persis seperti pendaftaran Rawat Jalan, dengan output code pendaftaran online"*, *"di checkin pasien online kiosk pasien masukan code tersebut untuk checkin verifikasi pendaftaran online tersebut"*, dan *"page pendaftaran online ini samakan flow nya seperti menu pendaftaran pasien di rawat jalan"*.

Mode: amendment pass (blueprint kiosk revisi 1 sudah `approved`). Capability map kiosk (`01-existing-capability-map.md` r1) dan impact scan Rawat Jalan terakhir disusun pada SHA lama (`551e1cbb`), sedangkan HEAD backend sekarang `5e777833` (`sukmagp`) dan frontend `af8a430ac` (`sukmagpV2`). Kemungkinan duplikasi belum diaudit ulang; tugas itu milik `trace-existing-capabilities` mode impact scan.

### 13.1 Fakta source (19 Nov 2026)

| ID | Fakta | Bukti |
| --- | --- | --- |
| `F-PO-1` | Backend belum punya wadah pendaftaran online/appointment Rawat Jalan. Pencarian kelas bernama Appointment, OnlineRegistration, Booking, atau Reservation hanya menemukan reservasi pra-kedatangan IGD, rawat inap, MCU, cuti HR, dan Reservasi Prioritas RJ | Backend `5e777833`, pencarian `--include=*.cs` |
| `F-PO-2` | Menu Pendaftaran Pasien RJ (petugas) punya langkah: Cari / Input Pasien → Jenis Pasien (Pribadi / Asuransi / Rujukan) → [Asuransi: Penjamin → Data Kunjungan] / [Pribadi: Data Kunjungan] / [Rujukan: Data Kunjungan (tanggal) → Data Rujukan → Penjamin] → General Consent (bila diwajibkan) → Verifikasi & Konfirmasi → Selesai | Frontend `src/lib/constants/health-services/registration-management/outpatient-registration/outpatient-registration.constants.js:57-155` |
| `F-PO-3` | Langkah "Cari / Input Pasien" di menu petugas menampilkan daftar pasien hasil pencarian. Langkah itu tidak dapat dipakai apa adanya di halaman tanpa login karena membuka data pasien lain | `RJ-DOC-DEC-034` (komponen `PatientSelectionStep` IGD dipakai ulang) |
| `F-PO-4` | Jadwal dokter sudah memisahkan izin dan kuota appointment dari walk-in: `IsAllowAppointment`, `IsAllowWalkIn`, `MaxPatientQuota`, `MaxAppointmentQuota`, `MaxWalkInQuota`. Pemakaian kuota dihitung dari kunjungan pada tanggal antrean (`IsAppointment`/`IsWalkIn`) | Backend `Areas/HealthServices/MasterData/Models/MstDoctorSchedule.cs:54-56`; `Areas/HealthServices/RegistrationManagement/Services/DoctorScheduleQuotaService.cs:32-80` |
| `F-PO-5` | General consent disimpan per pasien **dan per kunjungan** (`PatientId` dan `PatientEncounterId` wajib) beserta jenis penanda tangan dan gambar tanda tangan. Dikoreksi impact scan `COR-PO-01` | Backend `Areas/HealthServices/RegistrationManagement/Models/RegPatientGeneralConsent.cs:19-56` |
| `F-PO-6` | Preseden halaman publik tanpa login: konfirmasi kedatangan Reservasi Prioritas `/priority-arrival/{code}` memakai kode acak yang hanya disimpan hash-nya, tanpa data pribadi di halaman (`RJ-PRI-DEC-012`, `RJ-PRI-DEC-028`, `RJ-PRI-DEC-030`) | Blueprint `rawat-jalan/00-interview-decisions.md` Amendment RP |
| `F-PO-7` | Kartu "Check-in Pasien Online" sudah ada di beranda kiosk dan saat ini menampilkan pesan "Fitur akan dilanjutkan pada tahap berikutnya." | Frontend `src/components/view/kiosk/kiosk-home-view.jsx` (commit `5f18e9f04`, 19 Nov 2026; dikoreksi impact scan `COR-PO-02`) |
| `F-PO-8` | Backend belum punya pengirim email (tidak ada `SmtpClient`, `MailKit`, `IEmailSender`, atau sejenisnya), belum punya WA gateway, dan backend maupun frontend belum memakai captcha apa pun | Backend `5e777833` dan frontend `af8a430ac`, pencarian kata kunci pada `*.cs`, `appsettings.json`, `package.json`, dan `src/` |

### 13.2 Batas scope (dikonfirmasi Sukma Giri, 19 Nov 2026)

**Batas scope (satu kalimat):** pasien mendaftar Rawat Jalan sendiri lewat halaman publik tanpa login dan menerima kode pendaftaran online, lalu check-in dengan kode itu di kiosk saat datang.

**Di dalam scope:**

1. Halaman publik tanpa login "Pendaftaran Pasien Online" dengan alur langkah yang sama seperti menu Pendaftaran Pasien RJ (`F-PO-2`).
2. Kode pendaftaran online yang tampil di langkah Selesai.
3. Status pendaftaran online dan masa berlaku kodenya.
4. Menu Check-in Pasien Online di kiosk: pasien memasukkan kode, sistem memverifikasi, lalu pendaftaran diproses menjadi kunjungan RJ.
5. Perlindungan halaman publik: data pasien tidak bocor dan halaman tidak dapat disalahgunakan.
6. Pengiriman kode pendaftaran online ke WA atau email pasien, dengan satu kolom isian No. WA atau email di halaman online (ditambahkan `KSK-DEC-041`).
7. Jalur "Lupa Kode" di menu Check-in Pasien Online kiosk (ditambahkan `KSK-DEC-040`).
8. Layar petugas Pendaftaran Online: daftar, filter, pencarian, ubah tanggal/jadwal dokter/kontak, dan batal (ditambahkan `KSK-DEC-050` s.d. `052`).

**Di luar scope — untuk modul lain (titik sentuh saja):**

| Kebutuhan | Modul pemilik | Titik sentuh |
| --- | --- | --- |
| Tagihan dan pembayaran | `billing-kasir` | Sama seperti kunjungan RJ biasa |
| Cek kepesertaan BPJS online | Kartu "Kepesertaan BPJS" kiosk (masih ke halaman not found) | Tidak dicek saat daftar online |
| ~~Daftar pendaftaran online untuk petugas admisi~~ | Dipindah ke Di dalam scope butir 8 oleh `KSK-DEC-050` | — |
| Reservasi Prioritas dan member | `rawat-jalan` Amendment RP | Tidak berubah |
| Membuka domain dan API ke internet | IT/infrastruktur | Syarat go-live, bukan syarat desain |

### 13.3 Keputusan

| Decision ID | Jenis | Keputusan | Owner | Status | Disetujui oleh/pada | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| `KSK-DEC-026` | Decision | **Pendaftaran Pasien Online dan Check-in Pasien Online dicatat sebagai amendment blueprint kiosk, dengan tiga syarat.** (1) Batas scope kiosk diperluas dari "revisi alur self-service perangkat kiosk" menjadi **"layanan mandiri pasien: perangkat kiosk dan halaman publik"**, karena halaman pendaftaran online dibuka dari HP/laptop pasien, bukan perangkat kiosk. (2) Aturan pendaftaran — jadwal dokter, kuota, jenis pasien, penjamin, rujukan, dan general consent — **mengikuti keputusan Rawat Jalan** (antara lain `RJ-DOC-DEC-034` s.d. `039`) dan hanya dirujuk di sini, tidak ditulis ulang. Perubahan aturan itu tetap menjadi amendment blueprint `rawat-jalan`, supaya halaman online tidak menyimpang dari menu petugas. (3) Data pendaftaran online milik Registration (`entity_prefix: Reg`, sama dengan manifest kiosk), sehingga tidak ada prefix baru. Contoh: bila kelak RJ mengubah aturan "jadwal dokter wajib bila klinik `IsDoctorRequired`", halaman online ikut berubah lewat amendment RJ itu, bukan lewat keputusan kiosk | Sukma | `approved` | Sukma, 19 Nov 2026 ("untuk Kiosk dengan tiga syarat di atas: saya setuju") | Sesi ini |
| `KSK-DEC-027` | Decision | **Daftar Di dalam scope dan Di luar scope pada §13.2 disetujui. Hanya kebutuhan yang diminta yang dikerjakan.** Daftar pendaftaran online untuk petugas admisi, pengiriman kode otomatis, dan cek kepesertaan BPJS online tidak dibangun | Sukma | `approved` (diubah sebagian oleh `KSK-DEC-041`: pengiriman kode ke WA/email masuk scope, dan oleh `KSK-DEC-050`: layar petugas masuk scope) | Sukma, 19 Nov 2026 ("lanjut kerjakan sesuai kebutuhan yang saya minta, di luar itu tidak perlu dikerjakan") | Sesi ini |
| `KSK-DEC-028` | Decision | **`blueprint_shape` tetap `SINGLE`, `shape_decided_by` = `USER_CONFIRMED`.** Rumpun (a) pendaftaran online di halaman publik dan (b) check-in di kiosk memakai data, status, pemilik, dan hak akses yang sama. Check-in juga tidak bermakna tanpa pendaftaran online, sehingga tidak ada rumpun yang lolos 3 dari 5 syarat pemecahan. Kemampuan tanpa rumpun: tidak ada | Sukma | `approved` | Sukma, 19 Nov 2026 (syarat ketiga yang disetujui bersama `KSK-DEC-026`) | Sesi ini |
| `KSK-DEC-029` | Decision | **Alur halaman Pendaftaran Pasien Online = alur menu Pendaftaran Pasien RJ** (`F-PO-2`): Cari / Input Pasien, Jenis Pasien, Penjamin/Data Kunjungan/Data Rujukan sesuai jenis pasien, General Consent, Verifikasi & Konfirmasi, Selesai. Langkah Selesai menampilkan kode pendaftaran online. Penyesuaian yang wajib demi privasi halaman publik (`F-PO-3`) diputuskan lewat `KSK-PO-OQ-002` dan `003` | Sukma | `approved` | Sukma, 19 Nov 2026 ("page pendaftaran online ini samakan flow nya seperti menu pendaftaran pasien di rawat jalan") | Sesi ini |
| `KSK-DEC-030` | Decision | **Pendaftaran online belum menjadi kunjungan sampai pasien check-in di kiosk** (menutup `KSK-PO-OQ-001`). Saat pasien menyelesaikan halaman online, sistem menyimpan pendaftaran berstatus **Menunggu Check-in** dan langsung memakai **kuota appointment** jadwal dokter terpilih, sehingga pasien tidak kehabisan kuota saat datang. Kunjungan RJ, nomor antrean, dan tiket baru terbit saat check-in berhasil. Selama belum check-in, pendaftaran tidak muncul di Daftar Pasien RJ, antrean, nurse station, maupun billing. Pola ini sama dengan Reservasi Prioritas (`RJ-PRI-DEC-009`, `010`). Contoh: Ibu Rina mendaftar Senin malam untuk poli Anak dr. Sari hari Rabu. Kuota appointment dr. Sari Rabu berkurang satu sejak Senin. Rabu pukul 08.10 Ibu Rina check-in di kiosk, lalu kunjungan dan nomor antrean A-07 terbit | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-4` |
| `KSK-DEC-031` | Decision | **Pasien lama di halaman publik dikenali dengan NIK atau No. RM ditambah tanggal lahir** (menutup `KSK-PO-OQ-002`). Sistem hanya menjawab cocok atau tidak cocok, tanpa daftar hasil. Bila cocok, nama tampil tersamar. Langkah ini menggantikan pencarian bebas menu petugas pada langkah Cari / Input Pasien (`F-PO-3`). Contoh: pasien mengetik No. RM `00123456` dan tanggal lahir 12-09-1990. Bila cocok, layar menampilkan "Si** Ra****" dan pasien lanjut. Bila tidak cocok, layar menampilkan pesan umum tanpa menyebut bagian mana yang salah | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-3` |
| `KSK-DEC-032` | Decision | **Pasien baru dari halaman publik baru dibuatkan data master pasien dan No. RM saat check-in di kiosk** (menutup `KSK-PO-OQ-003`). Data diri yang diisi online hanya disimpan bersama pendaftaran online. Saat check-in, kiosk memindai KTP dan mencocokkannya dengan data online sebelum master pasien dibuat. Data palsu atau ganda dari internet tidak pernah masuk master pasien. Contoh: Pak Dodi (belum pernah berobat) mendaftar online hari Senin. Sampai Rabu pagi ia belum punya No. RM. Rabu 07.50 ia check-in, memindai KTP, lalu No. RM terbit bersama kunjungannya | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-3` |
| `KSK-DEC-033` | Decision | **Check-in di kiosk memakai kode pendaftaran online ditambah tanggal lahir pasien, dan hanya pada tanggal kunjungan (kalender WIB)** (menutup `KSK-PO-OQ-004`). Kode mati setelah dipakai. Kode yang terfoto atau bocor ke orang lain tidak dapat dipakai tanpa tanggal lahir. Contoh: pendaftaran untuk Rabu 25 Nov. Pasien yang mencoba check-in Selasa 24 Nov menerima pesan "Check-in hanya dapat dilakukan pada 25 Nov 2026". Pada Rabu, kode benar tetapi tanggal lahir salah ditolak dengan pesan umum | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-6` |
| `KSK-DEC-034` | Decision | **General Consent tetap ditandatangani di halaman online**, sebagai langkah yang sama dengan alur RJ (menutup `KSK-PO-OQ-005`). Kapan consent diwajibkan, siapa penanda tangan (pasien atau wali), dan isi persetujuan mengikuti aturan RJ (`KSK-DEC-026` syarat 2). Untuk pasien baru, consent disimpan bersama pendaftaran online lalu dilekatkan ke data pasien saat No. RM terbit ketika check-in (`KSK-DEC-032`, `F-PO-5`). Contoh: Ibu Rina (wali) menandatangani consent anaknya di HP pada Senin malam. Rabu saat check-in, consent itu sudah tercatat atas nama anaknya | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-5` |
| `KSK-DEC-035` | Decision | **Pasien Asuransi dan Pasien Rujukan di halaman online diproses sama seperti menu RJ** (menutup `KSK-PO-OQ-006`). Pasien Asuransi memilih kartu asuransi atau penjamin yang sudah terdaftar atas namanya, atau mengisi kartu baru. Pasien Rujukan mengisi data rujukan dan mengunggah foto atau PDF surat rujukan, paling besar 5 MB per berkas. Semua data itu tetap diperiksa petugas atau kasir menurut aturan RJ saat pasien datang. Halaman online tidak memeriksa kepesertaan BPJS (`KSK-DEC-027`). Contoh: Pak Dodi memilih Pasien Rujukan, mengisi faskes perujuk Puskesmas Tebet, dan mengunggah foto surat rujukan 2 MB. Berkas 8 MB ditolak dengan pesan "Ukuran berkas maksimal 5 MB" | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-2` |
| `KSK-DEC-036` | Decision | **Check-in yang berhasil menerbitkan kunjungan dan nomor antrean, lalu kiosk mencetak tiket** seperti alur Pasien Lama kiosk (nomor antrean, poli, dokter) (menutup `KSK-PO-OQ-007`). Contoh: Ibu Rina check-in pukul 08.10, kiosk mencetak tiket "A-07 — Poli Anak — dr. Sari", lalu Ibu Rina menuju poli | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `KSK-DEC-030` |
| `KSK-DEC-037` | Decision | **Pendaftaran online yang tidak check-in ditutup otomatis sebagai Tidak Datang setelah 23.59 WIB pada tanggal kunjungan** (menutup `KSK-PO-OQ-008`). Kode mati, dan kuota appointment yang dipakai (`KSK-DEC-030`) dilepas. Riwayat tidak dihapus. Proses ini idempoten: menjalankannya ulang tidak menambah riwayat, dan hari yang terlewat saat server mati ikut diproses saat server hidup lagi. Pola sama dengan `RJ-PRI-DEC-013`. Contoh: pendaftaran untuk Rabu 25 Nov tidak dipakai sampai malam. Kamis 00.00 statusnya Tidak Datang, dan kode yang dicoba di kiosk Kamis pagi ditolak | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `RJ-PRI-DEC-013` |
| `KSK-DEC-038` | Decision | **Perlindungan halaman publik: batas aturan, batas laju per IP, dan captcha** (menutup `KSK-PO-OQ-009`). (1) Paling banyak satu pendaftaran online terbuka per pasien per tanggal kunjungan. Pendaftaran juga ditolak bila pasien sudah punya kunjungan RJ aktif pada tanggal itu, sejalan dengan `RJ-PRI-DEC-021`. (2) Tanggal kunjungan paling jauh 30 hari ke depan. (3) Batas laju per alamat IP, misalnya 10 pendaftaran per jam dan 20 percobaan pencocokan data per jam. (4) Captcha di halaman online. Semua angka dapat diubah lewat konfigurasi. Penyedia captcha diputuskan lewat `KSK-PO-OQ-014`. Contoh: dari satu alamat IP, pendaftaran ke-11 dalam satu jam ditolak dengan pesan "Terlalu banyak percobaan, silakan coba lagi nanti". Pendaftaran untuk tanggal 31 hari ke depan ditolak | Sukma | `approved` | Sukma, 19 Nov 2026 (memilih "Rekomendasi + captcha") | `F-PO-8` |
| `KSK-DEC-039` | Decision | **NIK pasien baru yang ternyata sudah terdaftar diselesaikan saat check-in** (menutup `KSK-PO-OQ-011`). Saat daftar online, sistem tidak memberi tahu bahwa NIK sudah terdaftar, supaya halaman publik tidak dapat dipakai untuk mengecek siapa pasien RS. Saat check-in, kiosk memindai KTP dan menemukan pasien yang sudah ada, lalu kunjungan memakai No. RM lama tanpa membuat No. RM baru. Data diri dari halaman online yang berbeda tidak menimpa master pasien. Contoh: Pak Dodi lupa bahwa ia pernah berobat 2019 dan memilih pasien baru. Saat check-in, KTP-nya cocok dengan No. RM `00098765`, dan kunjungan memakai No. RM itu | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `KSK-DEC-032` |
| `KSK-DEC-040` | Decision | **Menu Check-in Pasien Online punya jalur "Lupa Kode"** (sebagian menutup `KSK-PO-OQ-012`). Pasien memasukkan NIK atau No. RM ditambah tanggal lahir, lalu kiosk menemukan pendaftaran online miliknya untuk hari ini. Pasien baru yang belum punya No. RM memakai NIK. Aturan hari kunjungan dan sekali pakai pada `KSK-DEC-033` tetap berlaku. Contoh: Ibu Rina kehilangan pesan berisi kode. Di kiosk ia menekan "Lupa Kode", memasukkan NIK dan tanggal lahir, lalu pendaftaran Poli Anak hari ini tampil dan ia check-in | Sukma | `approved` | Sukma, 19 Nov 2026 ("di menu checkin pasien online buatkan juga pilihan 'Lupa Kode' seperti rekomendasi") | Sesi ini |
| `KSK-DEC-041` | Decision | **Scope diperluas: kode pendaftaran online dikirim ke WA atau email pasien**, dengan satu kolom isian No. WA atau email di halaman online. Mengubah sebagian `KSK-DEC-027`. Cara pengirimannya diputuskan lewat `KSK-PO-OQ-013`, karena backend belum punya pengirim email maupun WA gateway (`F-PO-8`) | Sukma | `approved` | Sukma, 19 Nov 2026 ("kode pendaftaran online kirim ke WA/Email, jadi tambah 1 field untuk memasukan no WA atau Email untuk mengirim code tersebut") | `F-PO-8` |
| `KSK-DEC-042` | Decision | **Kode dikirim otomatis lewat email dan lewat tombol WhatsApp yang dikirim pasien sendiri** (menutup `KSK-PO-OQ-013`). Email: sistem mengirim otomatis lewat akun email SMTP rumah sakit sesudah pendaftaran tersimpan. WhatsApp: langkah Selesai menampilkan tombol "Kirim kode ke WhatsApp saya" yang membuka WhatsApp pasien dengan pesan siap kirim ke nomor yang ia isi, tanpa biaya dan tanpa akun pihak ketiga. WA otomatis lewat gateway menyusul bila `platform-notification` sudah punya gateway. Kegagalan kirim email tidak membatalkan pendaftaran: kode tetap tampil di layar, dan jalur Lupa Kode (`KSK-DEC-040`) tetap tersedia. Contoh: Ibu Rina mengisi `rina@contoh.id`, dan dalam 1 menit email berisi kode tiba. Pak Dodi mengisi `0812xxxxxxxx`, menekan tombol WhatsApp, lalu menekan Kirim di WhatsApp-nya sendiri | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-8` |
| `KSK-DEC-043` | Decision | **Kolom No. WA atau email wajib diisi salah satu.** Format divalidasi: No. WA lokal `08…` (aturan panjang sama dengan No. HP kiosk, `KSK-DEC-022`) atau alamat email yang sah. Contoh: kolom kosong ditolak dengan pesan "Isi No. WhatsApp atau email untuk menerima kode". Isian `0812-3456-7890` diterima | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `KSK-DEC-041` |
| `KSK-DEC-044` | Decision | **Penyedia captcha: Cloudflare Turnstile** (menutup `KSK-PO-OQ-014`). Gratis, umumnya tanpa teka-teki gambar sehingga ramah bagi pasien lansia, dan tidak memakai data pengunjung untuk iklan. Backend memverifikasi token Turnstile sebelum menyimpan pendaftaran. Site key dan secret disimpan di konfigurasi, bukan di source (`KSK-PO-OQ-016`) | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `KSK-DEC-038` |
| `KSK-DEC-045` | Decision | **Pendaftaran online ditutup otomatis bila pasien didaftarkan lewat jalur lain pada tanggal yang sama** (menutup `KSK-PO-OQ-017`). Kunjungan lewat kiosk Pasien Lama/Baru atau loket tetap dibuat seperti biasa. Pendaftaran online pasien itu untuk tanggal yang sama langsung berstatus **Didaftarkan lewat Jalur Lain**, ditautkan ke kunjungan tersebut, kodenya mati, dan kuota appointment yang dipakai dilepas, sehingga tidak ada kuota ganda maupun kunjungan ganda. Pola sama dengan `RJ-PRI-DEC-027`. Contoh: Ibu Rina punya pendaftaran online Poli Anak hari ini, tetapi memakai kiosk Pasien Lama pukul 08.00 dan mendapat nomor A-05. Pendaftaran onlinenya langsung berstatus Didaftarkan lewat Jalur Lain, dan kodenya ditolak bila dicoba di menu Check-in | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `RJ-PRI-DEC-027` |
| `KSK-DEC-046` | Decision | **Check-in ditolak bila jadwal dokter yang dipilih sudah tidak berlaku pada tanggal kunjungan** (menutup `KSK-PO-OQ-018`). Saat check-in, sistem memeriksa ulang jadwal dokter. Bila jadwal dibatalkan atau berubah sehingga tidak berlaku, kiosk menampilkan "Jadwal dokter berubah, silakan ke loket pendaftaran", dan petugas mendaftarkan pasien ke dokter atau jadwal lain lewat menu Pendaftaran Pasien RJ. Kode tidak dimatikan oleh penolakan ini. Pendaftaran online kemudian tertutup otomatis saat petugas membuat kunjungan (`KSK-DEC-045`), atau menjadi Tidak Datang pukul 23.59 (`KSK-DEC-037`). Contoh: dr. Sari cuti mendadak hari Rabu. Ibu Rina check-in pukul 08.10 dan diarahkan ke loket. Petugas mendaftarkannya ke dr. Budi, dan pendaftaran onlinenya berstatus Didaftarkan lewat Jalur Lain | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-PO-4` |
| `KSK-DEC-047` | Decision | **Pendaftaran online hanya untuk tanggal kunjungan besok (H+1) sampai 30 hari ke depan (H+30)** (menutup `KSK-PO-OQ-019`). Tanggal hari ini tidak dapat dipilih. Pasien yang ingin berobat hari ini datang ke rumah sakit dan memakai kiosk Pasien Lama/Baru atau loket (walk-in). Dengan begitu semua pendaftaran online memakai kuota appointment (`KSK-DEC-030`), kunjungan hasil check-in tercatat sebagai appointment, dan aturan RJ `RJ-DOC-DEC-037` (hari ini = walk-in) tetap utuh. Batas 30 hari tetap dapat diubah lewat konfigurasi (`KSK-DEC-038`). Contoh: Senin pukul 06.30 Pak Andi memilih tanggal Senin, dan halaman menolak dengan pesan "Pendaftaran online paling cepat untuk besok. Untuk berobat hari ini, silakan daftar di kiosk rumah sakit". Ia memilih Selasa, lalu kuota appointment dr. Hadi hari Selasa berkurang satu | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-GATE-PO-1` (`KSK-RCG-002`), `RJ-DOC-DEC-037` |
| `KSK-DEC-048` | Decision | **Check-in pasien baru: NIK pada kartu identitas yang dipindai wajib sama dengan NIK yang diisi online, dan data master pasien diambil dari hasil pindai** (menutup `KSK-PO-OQ-020`). Tanggal lahir sudah terkandung di NIK. Bila NIK berbeda atau pasien tidak membawa kartu identitas, check-in ditolak dengan pesan "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran". Penolakan ini tidak mengubah status dan tidak mematikan kode, sama seperti `KSK-DEC-046`. Pendaftaran online kemudian tertutup sebagai Didaftarkan lewat Jalur Lain saat petugas membuat kunjungan (`KSK-DEC-045`), atau menjadi Tidak Datang (`KSK-DEC-037`). Bila NIK cocok, master pasien dan No. RM dibuat dari hasil pindai, sedangkan No. WA/email diambil dari isian online. Data diri lain dari isian online tidak masuk master. Bila NIK ternyata sudah terdaftar, berlaku `KSK-DEC-039`. Cara pencocokan untuk SIM diputuskan lewat `KSK-PO-OQ-022`. Contoh: Ibu Wati mendaftar dengan NIK `3174…0011`, lalu memindai KTP suaminya dengan NIK `3174…0022`. Kiosk menolak dan mengarahkannya ke loket. Bila ia memindai KTP-nya sendiri, No. RM terbit dengan nama dan alamat sesuai KTP | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `KSK-DEC-032`, `F-GATE-PO-2` |
| `KSK-DEC-049` | Decision | **Kartu identitas yang diterima saat check-in pasien baru: KTP, KIA, dan SIM**, sama dengan kiosk Pasien Baru (`F-GATE-PO-2`). Keputusan ini memperjelas kata "memindai KTP" pada `KSK-DEC-032` (menutup `GATE-PO-11`). Pasien baru tanpa salah satu kartu itu, misalnya WNA dengan paspor, diarahkan ke loket. Contoh: Ibu Rina mendaftarkan anaknya sebagai pasien baru secara online. Saat check-in ia memindai KIA anaknya. NIK pada KIA sama dengan NIK online, lalu No. RM anak terbit | Sukma | `superseded` oleh `KSK-DEC-053` | Sukma, 19 Nov 2026 (pilihan rekomendasi) | `F-GATE-PO-2` |
| `KSK-DEC-050` | Decision | **Scope diperluas: petugas admisi mendapat layar Pendaftaran Online berupa daftar, filter, dan pencarian** (menutup sebagian `KSK-PO-OQ-021`; mengubah sebagian `KSK-DEC-027`). Daftar menampilkan pendaftaran online per tanggal kunjungan dan status. Petugas dapat mencari dengan kode, NIK/No. RM, atau nama. Dari layar ini petugas dapat mengubah (`KSK-DEC-051`) dan membatalkan (`KSK-DEC-052`). Tujuannya melayani pasien yang meminta perubahan atau pembatalan, dan membersihkan pendaftaran mencurigakan. Pasien tetap tidak dapat mengubah atau membatalkan sendiri dari halaman publik. Contoh: petugas memfilter tanggal 25 Nov dengan status Menunggu Check-in dan melihat 40 pendaftaran. Ia menemukan 6 pendaftaran pasien baru dengan email yang sama, lalu memeriksanya satu per satu | Sukma | `approved` | Sukma, 19 Nov 2026 (menjawab konfirmasi `KSK-PO-AS-001` dengan "pendaftaran online bisa di edit oleh petugas", lalu mengonfirmasi perluasan scope dengan memilih rekomendasi "Daftar + filter + pencarian", bukan opsi "Tidak jadi") | Sesi ini |
| `KSK-DEC-051` | Decision | **Petugas dapat mengubah tanggal kunjungan, jadwal dokter, dan No. WA/email, hanya selama status Menunggu Check-in** (menutup sebagian `KSK-PO-OQ-021`). Perubahan tunduk pada aturan yang sama dengan pendaftaran baru: tanggal H+1 s.d. H+30 dihitung dari hari perubahan (`KSK-DEC-047`), jadwal mengizinkan appointment dan kuota appointment masih ada (`KSK-DEC-030`), serta satu pendaftaran terbuka per pasien per tanggal (`KSK-DEC-038`). Kuota jadwal lama dilepas dan kuota jadwal baru dipakai dalam satu langkah. Jenis pasien, penjamin, rujukan, consent, dan identitas pasien tidak dapat diubah. Setiap perubahan tercatat di riwayat beserta nilai lama, nilai baru, petugas, dan waktu. Contoh: Ibu Rina menelepon pada Senin dan meminta pindah dari Rabu ke Kamis dengan dr. Sari. Petugas mengubah tanggalnya. Kuota dr. Sari hari Rabu bertambah satu dan kuota hari Kamis berkurang satu. Bila kuota Kamis sudah habis, perubahan ditolak dan pendaftaran Rabu tetap berlaku | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi "Tanggal, jadwal dokter, kontak") | `KSK-DEC-050` |
| `KSK-DEC-052` | Decision | **Petugas dapat membatalkan pendaftaran online dengan alasan wajib, hanya selama status Menunggu Check-in** (menutup `KSK-PO-OQ-021`). Status akhir baru **Dibatalkan**: kode mati, kuota appointment langsung dilepas, dan alasan serta petugas tercatat di riwayat. Pembatalan tidak dapat dibatalkan ulang. Pasien yang berubah pikiran mendaftar online lagi. Pola sama dengan pembatalan Reservasi Prioritas (`RJ-PRI-DEC-022`). Contoh: petugas menemukan 6 pendaftaran pasien baru dengan email yang sama untuk dr. Sari hari Rabu. Ia membatalkan keenamnya dengan alasan "Pendaftaran ganda/mencurigakan", dan 6 kuota appointment dr. Sari hari Rabu kembali tersedia | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi "Boleh, alasan wajib") | `KSK-DEC-050`, `RJ-PRI-DEC-022` |
| `KSK-DEC-053` | Decision | **Check-in pasien baru hanya menerima KTP dan KIA** (menutup `KSK-PO-OQ-022`; menggantikan `KSK-DEC-049`). SIM umumnya tidak mencantumkan NIK, sehingga syarat NIK hasil pindai pada `KSK-DEC-048` tidak dapat diterapkan untuk SIM. Pemegang SIM tanpa KTP diarahkan ke loket. Kiosk Pasien Baru biasa tetap menerima SIM seperti sekarang; keputusan ini hanya berlaku untuk check-in pendaftaran online. Contoh: Pak Budi memindai SIM saat check-in. Kiosk menampilkan "Untuk check-in pendaftaran online, gunakan KTP atau KIA. Bila tidak membawa, silakan ke loket pendaftaran" | Sukma | `approved` | Sukma, 19 Nov 2026 (pilihan rekomendasi "Check-in hanya KTP dan KIA") | `F-GATE-PO-2` |
| `KSK-DEC-054` | Approval | **Blueprint kiosk revisi 2 (Amendment PO) disetujui seluruhnya**: arsitektur, kontrak `KSK-CONTRACT-v2`, keputusan desain `KSK-DSN-PO-001` s.d. `017` (`02-backend-architecture.md` PO.15), dan letak menu "Pendaftaran Online" di grup yang sama dengan "Reservasi Prioritas" (`KSK-PO-FE-005`). Persetujuan ini **tidak** mencakup pembuatan atau eksekusi migration, yang tetap memerlukan persetujuan terpisah | Sukma | `approved` | Sukma, 19 Nov 2026 (memilih "Setujui semua" atas pertanyaan konfirmasi approval) | Sesi ini |
| `KSK-DEC-055` | Approval | **Roadmap Amendment PO disetujui**: `KSK-PO-RM-BE-001` r1 (`BE-KSK-PO-001` s.d. `010`), `KSK-PO-RM-FE-001` r1 (`FE-KSK-PO-001` s.d. `008`), dan `KSK-PO-TRACE-001` r1. Wewenang implementasi diberikan untuk seluruh task, dimulai dari gelombang 1 (`BE-KSK-PO-001`, `BE-KSK-PO-003`, `FE-KSK-PO-004`). Pembuatan dan penerapan migration `AddRegOnlineRegistration` tetap memerlukan konfirmasi terpisah | Sukma | `approved` | Sukma, 19 Nov 2026 (memilih "Setujui, mulai gelombang 1" atas pertanyaan konfirmasi approval roadmap) | Sesi ini |

### 13.4 Asumsi

| ID | Asumsi | Dasar |
| --- | --- | --- |
| `KSK-PO-AS-001` | Pasien tidak dapat membatalkan atau mengubah pendaftaran online dari halaman publik, karena fitur itu tidak diminta (`KSK-DEC-027`). Pendaftaran yang tidak dipakai diselesaikan lewat `KSK-PO-OQ-008` **Status: digantikan.** Saat diminta konfirmasi, Sukma (19 Nov 2026) menjawab "pendaftaran online bisa di edit oleh petugas". Rinciannya dibahas lewat `KSK-PO-OQ-021` | `KSK-DEC-027` |
| `KSK-PO-AS-002` | Hak akses layar petugas memakai Resource izin baru dengan aksi Lihat, Ubah, dan Batalkan, diberikan lewat pengaturan peran seperti Reservasi Prioritas (`RJ-PRI-DEC-020`). Nama Resource dan peran penerimanya ditetapkan saat desain | `KSK-DEC-050` s.d. `052` |
| `KSK-PO-AS-003` | Kode pendaftaran tidak berubah setelah petugas mengubah pendaftaran, dan sistem tidak mengirim ulang kode. Kode tidak dapat ditampilkan ulang karena hanya hash-nya yang disimpan (`CAP-PO-07`). Pasien yang tidak menerima kode memakai Lupa Kode (`KSK-DEC-040`) | `KSK-DEC-040`, `051` |

### 13.5 Open question dan blocker

| ID | Pertanyaan | Owner | Memblokir |
| --- | --- | --- | --- |
| `KSK-PO-OQ-001` | Kapan pendaftaran online menjadi kunjungan RJ dan mendapat nomor antrean, dan kapan kuota dokter terpakai | Sukma | Ditutup `KSK-DEC-030` |
| `KSK-PO-OQ-002` | Cara mengenali pasien lama di halaman publik sebagai pengganti pencarian bebas (`F-PO-3`) | Sukma | Ditutup `KSK-DEC-031` |
| `KSK-PO-OQ-003` | Pasien baru dari halaman publik: kapan data master pasien dan No. RM dibuat | Sukma | Ditutup `KSK-DEC-032` |
| `KSK-PO-OQ-004` | Syarat check-in di kiosk: hanya kode atau kode + data pembanding, dan jendela waktunya | Sukma | Ditutup `KSK-DEC-033` |
| `KSK-PO-OQ-005` | General Consent: ditandatangani di halaman online atau saat check-in di kiosk | Sukma | Ditutup `KSK-DEC-034` |
| `KSK-PO-OQ-006` | Jenis pasien Asuransi dan Rujukan di halaman publik: data penjamin dan surat rujukan diisi online atau dilengkapi di rumah sakit | Sukma | Ditutup `KSK-DEC-035` |
| `KSK-PO-OQ-007` | Hasil check-in di kiosk (misalnya cetak tiket antrean) | Sukma | Ditutup `KSK-DEC-036` |
| `KSK-PO-OQ-008` | Pendaftaran yang tidak check-in sampai hari kunjungan berakhir | Sukma | Ditutup `KSK-DEC-037` |
| `KSK-PO-OQ-009` | Pencegahan penyalahgunaan halaman publik: batas per pasien per tanggal, batas hari ke depan, dan batas laju per alamat IP | Sukma | Ditutup `KSK-DEC-038` |
| `KSK-PO-OQ-010` | Alamat publik halaman dan akses internet ke API, termasuk penerusan IP asli untuk batas laju (sama dengan `RJ-PRI-OQ-003`) | IT/infrastruktur | `IMPLEMENTATION` (go-live) |
| `KSK-PO-OQ-011` | Pasien memakai jalur pasien baru di halaman online, padahal NIK-nya sudah terdaftar di master pasien | Sukma | Ditutup `KSK-DEC-039` |
| `KSK-PO-OQ-012` | Pasien lupa atau kehilangan kode pendaftaran online saat tiba di rumah sakit (tidak ada pengiriman kode otomatis, `KSK-DEC-027`) | Sukma | Ditutup `KSK-DEC-040` dan `041` |
| `KSK-PO-OQ-013` | Cara mengirim kode ke WA/email: email otomatis lewat SMTP rumah sakit, WA otomatis lewat penyedia gateway, atau tombol WhatsApp yang dikirim pasien sendiri (`F-PO-8`) | Sukma, IT | Ditutup `KSK-DEC-042` |
| `KSK-PO-OQ-014` | Penyedia captcha (`KSK-DEC-038`) | Sukma, IT | Ditutup `KSK-DEC-044` |
| `KSK-PO-OQ-015` | Akun email SMTP rumah sakit untuk pengiriman kode (alamat pengirim, server, port, sandi) | IT/infrastruktur | `IMPLEMENTATION` (go-live) |
| `KSK-PO-OQ-016` | Akun Cloudflare Turnstile beserta site key dan secret untuk domain halaman online | IT/infrastruktur | `IMPLEMENTATION` (go-live) |
| `KSK-PO-OQ-017` | Pasien yang punya pendaftaran online terbuka lalu didaftarkan lewat jalur lain (kiosk Pasien Lama/Baru atau loket) pada tanggal yang sama | Sukma | Ditutup `KSK-DEC-045` |
| `KSK-PO-OQ-018` | Jadwal dokter yang dipilih berubah atau dibatalkan sesudah pasien mendaftar online | Sukma | Ditutup `KSK-DEC-046` |
| `KSK-PO-OQ-019` | Pendaftaran online untuk hari ini: boleh atau tidak, dan bila boleh memakai kuota appointment atau walk-in. `KSK-DEC-030` memakai kuota appointment, sedangkan `RJ-DOC-DEC-037` menetapkan hari ini = walk-in (ditemukan `KSK-RCG-002`) | Sukma | Ditutup `KSK-DEC-047` |
| `KSK-PO-OQ-020` | Check-in pasien baru: data apa yang wajib sama antara kartu identitas yang dipindai dan data online, apa yang terjadi bila tidak sama atau pasien tidak membawa kartu identitas, dan dari mana data master pasien diambil (ditemukan `KSK-RCG-002`) | Sukma | Ditutup `KSK-DEC-048` dan `049` (SIM menyisakan `KSK-PO-OQ-022`) |
| `KSK-PO-OQ-021` | Petugas dapat mengubah pendaftaran online (jawaban Sukma atas konfirmasi `KSK-PO-AS-001`). Belum jelas: isian apa yang boleh diubah, apakah petugas juga boleh membatalkan, dan cara petugas menemukan pendaftaran. Jawaban ini memperluas batas scope §13.2 dan mengubah `KSK-DEC-027` (daftar pendaftaran online untuk petugas tidak dibangun), sehingga perlu konfirmasi eksplisit | Sukma | Ditutup `KSK-DEC-050`, `051`, dan `052` |
| `KSK-PO-OQ-022` | SIM umumnya tidak mencantumkan NIK. Di kiosk Pasien Baru, NIK untuk SIM diisi atau dikoreksi sendiri oleh pasien (`requiredFields` SIM memuat `nik`). Bagaimana pencocokan `KSK-DEC-048` untuk pasien yang memindai SIM? | Sukma | Ditutup `KSK-DEC-053` |

**Blocker desain:** tidak ada. `KSK-PO-OQ-019` s.d. `022` sudah ditutup `KSK-DEC-047` s.d. `053`. `KSK-PO-OQ-010`, `015`, dan `016` milik IT/infrastruktur dan hanya memblokir go-live.

### 13.6 Status dan perpindahan — Pendaftaran Pasien Online

| Dari status | Tindakan | Ke status | Siapa | Syarat | Contoh |
| --- | --- | --- | --- | --- | --- |
| — | Menyelesaikan halaman online | Menunggu Check-in | Pasien (tanpa login) | Captcha lolos, batas laju dan batas aturan terpenuhi (`KSK-DEC-038`), kolom WA/email terisi (`KSK-DEC-043`), jadwal dokter berlaku dan kuota appointment tersisa (`KSK-DEC-030`, aturan RJ) | Ibu Rina, Senin malam, untuk Rabu |
| Menunggu Check-in | Check-in dengan kode + tanggal lahir, atau lewat Lupa Kode | Menjadi Kunjungan | Pasien di kiosk | Hari ini = tanggal kunjungan (WIB); jadwal dokter masih berlaku (`KSK-DEC-046`); pasien baru: KTP atau KIA dipindai dan NIK-nya sama dengan NIK online (`KSK-DEC-048`, `053`, `039`). Kunjungan, nomor antrean, dan tiket terbit (`KSK-DEC-036`) | Rabu 08.10, tiket A-07 |
| Menunggu Check-in | Pasien didaftarkan lewat kiosk Pasien Lama/Baru atau loket pada tanggal yang sama | Didaftarkan lewat Jalur Lain | Sistem | Ditautkan ke kunjungan itu; kode mati; kuota dilepas (`KSK-DEC-045`) | Ibu Rina memakai kiosk Pasien Lama |
| Menunggu Check-in | Tanggal kunjungan berakhir | Tidak Datang | Sistem | Setelah 23.59 WIB; kode mati; kuota dilepas; idempoten (`KSK-DEC-037`) | Tidak ada check-in sampai malam |
| Menunggu Check-in | Mengubah tanggal, jadwal dokter, atau No. WA/email | Menunggu Check-in | Petugas berhak akses | Aturan tanggal, jadwal, kuota, dan satu pendaftaran per tanggal tetap berlaku; kuota lama dilepas dan kuota baru dipakai (`KSK-DEC-051`) | Ibu Rina pindah dari Rabu ke Kamis |
| Menunggu Check-in | Membatalkan | Dibatalkan | Petugas berhak akses | Alasan wajib; kode mati; kuota dilepas (`KSK-DEC-052`) | Pendaftaran ganda dengan email yang sama |

Status akhir: Menjadi Kunjungan, Didaftarkan lewat Jalur Lain, Tidak Datang, Dibatalkan. Pasien tidak dapat mengubah atau membatalkan sendiri. Perubahan dan pembatalan hanya oleh petugas (`KSK-DEC-051`, `052`). Penolakan check-in (kode atau tanggal lahir salah, belum hari kunjungan, jadwal tidak berlaku, kartu identitas tidak sesuai) tidak mengubah status.

### 13.7 Skenario normal dan exception

| Skenario | Perilaku yang disepakati | Dasar |
| --- | --- | --- |
| Pasien lama memasukkan NIK/No. RM dan tanggal lahir yang tidak cocok | Pesan umum, tanpa menyebut bagian yang salah dan tanpa daftar pasien | `KSK-DEC-031` |
| Pasien baru ternyata sudah terdaftar | Tidak diberi tahu saat daftar online; saat check-in kunjungan memakai No. RM lama | `KSK-DEC-039` |
| Kuota appointment habis saat daftar online | Ditolak sesuai aturan kuota RJ; pasien memilih jadwal lain | `KSK-DEC-030`, `F-PO-4` |
| Pendaftaran kedua untuk pasien dan tanggal yang sama | Ditolak | `KSK-DEC-038` |
| Tanggal kunjungan lebih dari 30 hari ke depan | Ditolak | `KSK-DEC-038` |
| Batas laju per IP terlampaui atau captcha gagal | Ditolak, pasien diminta mencoba lagi nanti | `KSK-DEC-038`, `044` |
| Email kode gagal terkirim | Pendaftaran tetap tersimpan; kode tetap tampil di layar; Lupa Kode tersedia | `KSK-DEC-042` |
| Check-in sebelum tanggal kunjungan | Ditolak dengan pesan tanggal | `KSK-DEC-033` |
| Kode dipakai kedua kali | Ditolak | `KSK-DEC-033` |
| Pasien lupa kode | Jalur Lupa Kode dengan NIK/No. RM + tanggal lahir | `KSK-DEC-040` |
| Pasien mendaftar lewat jalur lain pada hari yang sama | Pendaftaran online ditutup otomatis | `KSK-DEC-045` |
| Jadwal dokter batal atau berubah | Check-in ditolak, pasien ke loket | `KSK-DEC-046` |
| Tidak datang sampai 23.59 WIB | Ditutup Tidak Datang, kuota dilepas; server mati ikut dikejar | `KSK-DEC-037` |
| Berkas surat rujukan lebih dari 5 MB | Ditolak | `KSK-DEC-035` |
| Pasien memilih tanggal hari ini di halaman online | Ditolak, diarahkan ke kiosk rumah sakit | `KSK-DEC-047` |
| Kartu identitas pasien baru saat check-in berbeda NIK, tidak dibawa, atau berupa SIM | Ditolak dengan arahan ke loket; status dan kode tetap | `KSK-DEC-048`, `053` |
| Pasien meminta ganti tanggal, dokter, atau kontak | Petugas mengubah lewat layar Pendaftaran Online | `KSK-DEC-051` |
| Pendaftaran mencurigakan, atau pasien meminta batal | Petugas membatalkan dengan alasan | `KSK-DEC-052` |

### 13.8 Wewenang keputusan frontend — Amendment PO

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
| --- | --- | --- | --- | --- | --- |
| `KSK-PO-FE-001` | Langkah halaman online | Sukma | `approved` | Sama dengan menu Pendaftaran Pasien RJ (`KSK-DEC-029`). Pencarian bebas diganti formulir NIK/No. RM + tanggal lahir (`KSK-DEC-031`). Tambahan hanya kolom No. WA/email dan captcha (`KSK-DEC-038`, `043`, `044`) | `KSK-DEC-029` |
| `KSK-PO-FE-002` | Data yang tampil di halaman publik | Sukma | `approved` | Pasien lama: hanya nama tersamar, serta kartu asuransi atau penjamin miliknya dengan nomor tersamar. Tidak pernah menampilkan data pasien lain, NIK, No. RM lengkap, maupun alamat | `KSK-DEC-031`, `035` |
| `KSK-PO-FE-003` | Isi pesan email dan WhatsApp | Sukma | `approved` | Kode, tanggal kunjungan, poli, dan dokter. Tanpa NIK, No. RM, maupun data penjamin. Kalimat persisnya `DEV_DISCRETION` | `KSK-DEC-042` |
| `KSK-PO-FE-004` | Route publik, tata letak halaman online, tata letak layar check-in dan Lupa Kode di kiosk | — | `DEV_DISCRETION` | Ikuti konvensi frontend Quilvian. Komponen menu RJ dipakai ulang bila tidak membuka data pasien. Layar kiosk mengikuti gaya beranda dan alur kiosk yang sudah ada | Belum ada brief UI |
| `KSK-PO-FE-005` | Layar petugas Pendaftaran Online | — | `DEV_DISCRETION` | Isi dan aksi sesuai `KSK-DEC-050` s.d. `052`. Data pasien hanya tampil bagi pemegang hak akses (`KSK-PO-AS-002`). Letak menu dan tata letak mengikuti konvensi frontend Quilvian dan pola layar Reservasi Prioritas; letak menu diusulkan saat desain dan dikunci saat approval blueprint | Belum ada brief UI |

### 13.9 Acceptance criteria — Amendment PO

1. Halaman Pendaftaran Pasien Online dapat dibuka tanpa login, dan urutan langkahnya untuk Pasien Pribadi, Asuransi, dan Rujukan sama dengan menu Pendaftaran Pasien RJ.
2. Pencocokan pasien lama tidak pernah mengembalikan daftar pasien. Data yang tidak cocok menghasilkan pesan umum. Data yang cocok hanya menampilkan nama tersamar.
3. Pendaftaran yang selesai berstatus Menunggu Check-in, memakai tepat satu kuota appointment jadwal dokter, dan tidak muncul di Daftar Pasien RJ, antrean, nurse station, maupun billing.
4. Pendaftaran pasien baru tidak membuat data master pasien atau No. RM sebelum check-in.
5. Langkah Selesai menampilkan kode. Bila email diisi, email berisi kode terkirim. Bila No. WA diisi, tombol WhatsApp membuka pesan berisi kode ke nomor itu.
6. Kolom No. WA/email kosong, captcha gagal, pendaftaran kedua untuk pasien dan tanggal yang sama, tanggal lebih dari 30 hari ke depan, dan batas laju per IP yang terlampaui masing-masing ditolak.
7. Check-in dengan kode dan tanggal lahir yang benar pada tanggal kunjungan menerbitkan kunjungan dan nomor antrean, lalu mencetak tiket. Kode yang sama ditolak bila dipakai lagi.
8. Check-in sebelum tanggal kunjungan ditolak dengan pesan tanggal. Tanggal lahir yang salah ditolak dengan pesan umum.
9. Check-in pasien baru membuat master pasien dan No. RM setelah KTP dipindai. Bila NIK sudah terdaftar, kunjungan memakai No. RM lama tanpa menimpa data master.
10. Lupa Kode dengan NIK/No. RM dan tanggal lahir menemukan pendaftaran online pasien untuk hari ini.
11. Setelah 23.59 WIB, pendaftaran yang belum check-in menjadi Tidak Datang, dan kuotanya dilepas. Menjalankan ulang penutupan tidak menambah riwayat.
12. Kunjungan lewat kiosk atau loket pada tanggal yang sama menutup pendaftaran online pasien itu sebagai Didaftarkan lewat Jalur Lain, mematikan kodenya, dan melepas kuotanya.
13. Check-in untuk jadwal dokter yang sudah tidak berlaku ditolak dengan arahan ke loket.
14. General consent yang ditandatangani online tersimpan. Untuk pasien baru, consent itu melekat ke data pasien setelah check-in.
15. Unggahan surat rujukan lebih dari 5 MB ditolak.
16. Tanggal kunjungan hari ini ditolak di halaman online. Tanggal besok sampai 30 hari ke depan diterima.
17. Check-in pasien baru dengan KTP atau KIA yang NIK-nya berbeda dari NIK online, atau tanpa kartu, ditolak dengan arahan ke loket, dan status tetap Menunggu Check-in. SIM ditolak untuk check-in. Bila NIK sama, master pasien dibuat dari hasil pindai.
18. Petugas berhak akses melihat daftar pendaftaran online per tanggal kunjungan dan status, serta dapat mencari dengan kode, NIK/No. RM, atau nama. Petugas tanpa hak akses tidak dapat membuka layar itu.
19. Petugas mengubah tanggal, jadwal dokter, atau No. WA/email hanya pada status Menunggu Check-in. Kuota lama dilepas dan kuota baru dipakai. Perubahan ke jadwal yang kuotanya habis ditolak tanpa mengubah pendaftaran. Riwayat mencatat nilai lama dan nilai baru.
20. Petugas membatalkan dengan alasan wajib. Status menjadi Dibatalkan, kode ditolak di kiosk, dan kuota langsung dilepas.

### 13.10 Langkah berikutnya

Seluruh pertanyaan bisnis yang memblokir desain sudah tertutup (`KSK-DEC-026` s.d. `046`). Capability map kiosk dan impact scan RJ tercatat pada SHA lama (`551e1cbb`), sedangkan fitur ini menyentuh jalur pembuatan kunjungan, pencocokan pasien kiosk, general consent, penjamin, berkas rujukan, kuota jadwal dokter, dan cetak tiket. Langkah berikutnya: `trace-existing-capabilities` mode impact scan pada HEAD backend `5e777833` dan frontend `af8a430ac`.

**Impact scan selesai (19 Nov 2026)** pada backend `5e777833` dan frontend `5f18e9f04`: [01-capability-impact-scan-pendaftaran-online.md](01-capability-impact-scan-pendaftaran-online.md). Dari 29 kemampuan: 5 Ready to reuse, 11 Reuse with adapter, 6 Extend, 6 Missing, 1 Unknown (`UNK-PO-01`, infrastruktur, sama dengan `KSK-PO-OQ-010`), tanpa Conflict. Tidak ada temuan yang membuka keputusan bisnis baru. Langkah berikutnya: `requirement-completeness-gate` untuk Amendment PO.

**Penilaian kelengkapan selesai (19 Nov 2026):** [02-requirement-completeness-assessment-pendaftaran-online.md](02-requirement-completeness-assessment-pendaftaran-online.md) (`KSK-RCG-002`), hasil `PARTIALLY_READY`. Lima dari tujuh slice siap desain. `S-PO-2` menunggu `KSK-PO-OQ-019` (pendaftaran untuk hari ini dan jenis kuotanya), dan `S-PO-6` menunggu `KSK-PO-OQ-020` (pencocokan kartu identitas pasien baru saat check-in). Usulan non-blocking dicatat sebagai `GATE-PO-01` s.d. `11`. Langkah berikutnya: `grill-me` singkat untuk kedua pertanyaan itu, lalu `design-business-module`.

**Closure pass singkat selesai (19 Nov 2026):** `KSK-PO-OQ-019` s.d. `022` ditutup oleh `KSK-DEC-047` s.d. `053`. `KSK-DEC-049` digantikan `KSK-DEC-053`. Scope bertambah satu butir: layar petugas Pendaftaran Online (`KSK-DEC-050`). Bentuk blueprint tetap `SINGLE` (`KSK-DEC-028`), karena layar petugas memakai data, status, dan pemilik yang sama. Layar petugas belum tercakup impact scan dan penilaian kelengkapan `KSK-RCG-002`. Langkah berikutnya: impact scan tambahan untuk layar petugas, lalu penilaian ulang `KSK-RCG-002`, kemudian `design-business-module`.

**Addendum impact scan selesai (19 Nov 2026):** [01-capability-impact-scan-pendaftaran-online.md](01-capability-impact-scan-pendaftaran-online.md) §8 menambah `CAP-PO-30` s.d. `37` untuk layar petugas dan `KSK-DEC-047` s.d. `053`: 2 Ready to reuse, 4 Reuse with adapter, 2 Extend, tanpa Missing, Unknown, atau Conflict baru. Dua temuan penting untuk desain. Pertama, pemeriksaan kuota belum dikunci per jadwal (`CAP-PO-34`). Kedua, langkah pindai kiosk membiarkan pasien mengubah NIK hasil OCR, sehingga layar check-in perlu varian dengan NIK terkunci (`CAP-PO-37`). Tidak ada keputusan bisnis baru yang terbuka. Langkah berikutnya: penilaian ulang `KSK-RCG-002`.

**Penilaian ulang kelengkapan selesai (19 Nov 2026):** `KSK-RCG-002` revisi 2 ([02-requirement-completeness-assessment-pendaftaran-online.md](02-requirement-completeness-assessment-pendaftaran-online.md) §9) menyatakan `READY_FOR_DOMAIN_DESIGN` untuk kedelapan slice, termasuk slice baru `S-PO-8` (layar petugas). Tidak ada butir `BLOCKING` yang terbuka. Usulan non-blocking baru `GATE-PO-12` s.d. `17`; yang terpenting `GATE-PO-13` (ganti jadwal hanya dalam poli yang sama) dan `GATE-PO-17` (kunci kuota per jadwal dan tanggal). `hospital-domain-architect` tidak dipakai. Langkah berikutnya: `design-business-module` untuk seluruh Amendment PO.

**Desain selesai sebagai `draft` (19 Nov 2026):** bagian *Amendment PO* pada `02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `data/data-dictionary.md`, kelima berkas `contracts/` (`KSK-CONTRACT-v2`), `testing/acceptance-test-matrix.md`, `flowcharts/00-alur-utama.md`, dan tiga flowchart baru. Keputusan desain yang menunggu approval: `KSK-DSN-PO-001` s.d. `017` (`02-backend-architecture.md` PO.15) dan letak menu `KSK-PO-FE-005`. Langkah berikutnya: approval blueprint revisi 2 oleh pemilik, lalu `plan-module-delivery`.

**Blueprint revisi 2 disetujui (19 Nov 2026, `KSK-DEC-054`).** Langkah berikutnya: `plan-module-delivery` untuk Amendment PO.

**Roadmap disusun (19 Nov 2026, `draft`):** `KSK-PO-RM-BE-001` r1 (10 task), `KSK-PO-RM-FE-001` r1 (8 task), `KSK-PO-TRACE-001` r1 di folder `roadmap/`. Langkah berikutnya: approval roadmap, lalu task gelombang 1 `BE-KSK-PO-001` (dengan konfirmasi terpisah untuk migration), `BE-KSK-PO-003`, dan `FE-KSK-PO-004`.
