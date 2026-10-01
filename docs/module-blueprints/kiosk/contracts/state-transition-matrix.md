# Kiosk — Matriks Perpindahan Keadaan

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `00-interview-decisions.md` r2 |
| Traceability | KSK-RM-005, KSK-OLD-001..010, SEC-KSK-007, `KSK-DEC-010/012/014`, `KSK-INV-004/008` |

> **Penting.** Revisi ini **tidak menambah status yang disimpan di database.** Status kunjungan (`EncounterStatus`) dan status sesi kiosk (`KioskScanSessionStatus`) tetap seperti hari ini. Yang diatur di sini adalah **keadaan layar Kiosk** — urutan yang wajib dipatuhi agar pasien tidak melompati langkah. Nama keadaan di bawah dipakai persis sama di `flowcharts/`.

## 1. Cek Nomor Rekam Medis

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `IDLE` | Tekan "Cek Nomor Rekam Medis" | `CHECKING` | Pasien | Isian lolos `KSK-VAL-001..006` | Tetap `IDLE` + pesan validasi |
| `CHECKING` | Backend menjawab `result = 1` | `FOUND` | Sistem | — | — |
| `CHECKING` | Backend menjawab `result = 2` | `NOT_FOUND` | Sistem | — | — |
| `CHECKING` | Backend menjawab `result = 3` | `MULTIPLE_MATCH` | Sistem | — | — |
| `CHECKING` | Backend menjawab `result = 4` | `CONTACT_STAFF` | Sistem | — | — |
| `CHECKING` | `429`, `5xx`, timeout, jaringan putus | `ERROR` | Sistem | — | — |
| `ERROR` | Tekan "Coba Lagi" | `CHECKING` | Pasien | Isian tidak berubah | — |
| `FOUND` | Tekan "Lanjut Pendaftaran Pasien Lama" | Pasien Lama `IDENTIFICATION` (terisi) | Pasien | `patientId` diteruskan lewat memori | — |
| `NOT_FOUND` | Tekan "Daftar Sebagai Pasien Baru" | Pendaftaran Pasien Baru | Pasien | Tanpa membawa KTP/HP (`KSK-GAP-005`) | — |
| `MULTIPLE_MATCH` (HP) | Tekan "Cari dengan No. KTP" | `IDLE` dengan metode KTP terpilih | Pasien | — | — |
| Keadaan apa pun | Tekan "Beranda" / timeout 120 detik | `SESSION_CLEARED` → Beranda | Pasien / Sistem | — | — |

**Perpindahan yang tidak sah:**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `ERROR` | `NOT_FOUND` / Pendaftaran Pasien Baru | Gagal teknis bukan bukti pasien belum terdaftar (`KSK-INV-002`) |
| `MULTIPLE_MATCH` / `CONTACT_STAFF` | Pendaftaran Pasien Baru | Akan membuat pasien ganda |
| `CHECKING` | `CHECKING` (request kedua) | Kirim ganda (`KSK-VAL-008`) |
| `IDLE` | `FOUND` | Keputusan wajib dari backend (`KSK-INV-001`) |

## 2. Pendaftaran Pasien Lama

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| (masuk halaman) | Buka dari Beranda | `IDENTIFICATION` | Pasien | — | — |
| (masuk halaman) | Dari Cek No. RM `FOUND` | `IDENTIFICATION` terisi | Sistem | Detail pasien berhasil dimuat ulang dengan `patientId` | Gagal memuat → `IDENTIFICATION` kosong + pesan |
| `IDENTIFICATION` | Pasien terpilih, tekan Lanjut | `DATA_REVIEW` | Pasien | Tepat satu pasien terpilih | `KSK-VAL-010` |
| `DATA_REVIEW` | Tekan "Cari Pasien Lain" | `IDENTIFICATION` | Pasien | Pasien dan hasil pindai dibuang | — |
| `DATA_REVIEW` | Tekan Lanjut | `SERVICE_DESTINATION` | Pasien | — | — |
| `SERVICE_DESTINATION` | Pilih Poliklinik | `VISIT_TYPE` | Pasien | Sesi kiosk poliklinik terbentuk (tanpa target) | `KSK-VAL-012`, tetap di `SERVICE_DESTINATION` |
| `SERVICE_DESTINATION` | Pilih Laboratorium, jawab surat dokter | `LAB_HANDOFF` | Pasien | Sesi kiosk Laboratorium terbentuk (`targetService = 2`) | `KSK-VAL-012`, tetap di `SERVICE_DESTINATION` |
| `VISIT_TYPE` | Pilih jenis kunjungan | `PAYMENT` | Pasien | — | — |
| `PAYMENT` | Pilih metode + penjamin, Lanjut | `SERVICE_AND_DOCTOR` | Pasien | Kondisi C: satu penjamin utama terpilih | `KSK-VAL-013/014` |
| `SERVICE_AND_DOCTOR` | Pilih layanan, dokter, jadwal, Lanjut | `CONFIRMATION` | Pasien | Ketiganya terisi | `KSK-VAL-016` |
| `CONFIRMATION` | Tekan Kembali | `SERVICE_AND_DOCTOR` | Pasien | — | — |
| `CONFIRMATION` | Tekan Daftar | `QUEUE_PRINT` | Pasien | Kunjungan berhasil dibuat backend | `KSK-VAL-015` / gagal → tetap `CONFIRMATION` + pesan |
| `QUEUE_PRINT` | Tekan Selesai / timeout | `SESSION_CLEARED` → Beranda | Pasien / Sistem | — | — |
| `LAB_HANDOFF` | Tekan Selesai / timeout | `SESSION_CLEARED` → Beranda | Pasien / Sistem | — | — |
| Keadaan apa pun sebelum `QUEUE_PRINT` | Beranda / timeout 120 detik | `SESSION_CLEARED` → Beranda | Pasien / Sistem | Tidak sedang menunggu jawaban backend (`KSK-GAP-008`) | Timeout ditunda sampai request selesai |

Kembali ke langkah sebelumnya selain yang tercantum (misalnya dari `PAYMENT` ke `VISIT_TYPE`) diizinkan dan tidak menghapus pilihan yang sudah dibuat, **kecuali** kembali ke `IDENTIFICATION` yang membuang seluruh draf.

**Perpindahan yang tidak sah:**

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `IDENTIFICATION` | `SERVICE_DESTINATION` | Tujuan Layanan tidak boleh sebelum Review (`KSK-INV-004`) |
| `DATA_REVIEW` | `VISIT_TYPE` / `PAYMENT` | Melompati Tujuan Layanan (KSK-OLD-010) |
| `SERVICE_DESTINATION` | `SERVICE_DESTINATION` dengan sesi kedua | Satu perjalanan = satu sesi (`KSK-INV-008`); memilih ulang setelah sesi terbentuk memakai sesi yang sama bila tujuannya sama, dan **dilarang** mengganti tujuan setelah sesi terbentuk — pasien harus mulai ulang dari Beranda |
| `CONFIRMATION` | `PAYMENT` untuk mengganti penjamin **setelah** kunjungan dibuat | `KSK-DEC-009` |
| `QUEUE_PRINT` | langkah mana pun sebelumnya | Kunjungan sudah terbentuk |

## 3. Status database yang disentuh (tidak berubah)

| Entity | Status awal saat dibuat dari Kiosk | Catatan |
| --- | --- | --- |
| `TrxKioskScanSession` | Sesuai `scan-result` existing (`Success` bila cocok, `ManualInput` bila `isManualInput`) | Kini dibentuk di `SERVICE_DESTINATION`, bukan `IDENTIFICATION` |
| `RegPatientEncounter` | `EncounterStatus.Registered` (existing) | Tidak berubah |
