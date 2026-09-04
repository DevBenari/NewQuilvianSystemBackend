# Integration Contract — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Compatibility impact | `0.3.0`: satu integrasi baru **`INT-KEP-06`** — keutuhan dan koreksi dokumen keperawatan kepada `MedicalRecordManagement`, sesuai `RWI-DEC-091`. Lima integrasi lama tidak berubah |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.3`; `PRD-RWI-FINAL-001` v1.0.0; decision log `13` |
| Keputusan yang mengikat | `RWI-DEC-091`, `RWI-FACT-016`, `RM-DEC-019` (milik `MedicalRecordManagement`) |
| Tanggal | 2 September 2026 |

---

## 0. Kenapa dokumen ini yang paling berisi di sub-modul ini

Sub-modul ini **tidak memiliki satu tabel pun**. Hampir seluruh wujudnya adalah integrasi:
membaca konteks dari episode, menulis ke tabel milik `ClinicalManagement`, memicu rujukan ke
Gizi, dan mengirim pemicu tagihan ke Billing.

---

## 1. `INT-KEP-01` — Pelonggaran konteks klinis rawat inap ★ **penghalang utama**

| Field | Isinya |
| --- | --- |
| Arah | Rawat Inap **meminta** perubahan pada `ClinicalManagement` |
| Bentuk | Sinkron, di dalam proses yang sama. Bukan pesan, bukan antrean |
| Pemilik perubahan | `ClinicalManagement` — Muhammad Hamzah, disetujui `RWI-DEC-062` |
| Yang diminta | `ValidateCreateWithoutQueueAsync` menerima encounter yang punya `InpEpisode` berstatus `Admitted`, setara dengan cara `EmgVisit` dipakai untuk IGD |
| Yang dibaca | `InpEpisode` — `EncounterId`, `EpisodeStatus` |
| Yang **tidak** berubah | Nol kolom. Nol tabel. Perilaku rawat jalan dan medical check-up tetap sama persis |
| Idempotency | Tidak berlaku — ini pemeriksaan baca |
| Timeout | Pembacaan lokal satu database; tidak ada panggilan jaringan |
| Bila gagal | Pengkajian ditolak `422` beserta `VAL-KEP-01`/`02`/`03`. Tidak ada keadaan setengah jadi |
| Traceability | `PRD-RWI-FINAL-001` bagian 30.3; `RWI-DEC-080`; `AC-CAP012-01` |

**Ini satu-satunya penghalang yang menahan seluruh sub-modul.** Selama `INT-KEP-01` belum ada,
tidak satu pun dari lima kemampuan dapat dipakai untuk pasien rawat inap.

---

## 2. `INT-KEP-02` — Konteks episode bagi ruang kerja

| Field | Isinya |
| --- | --- |
| Arah | **Baca** dari `episode-rawat-inap` |
| Yang dibaca | Pasien, lokasi terkini, DPJP, perawat penanggung jawab, status episode, lama dirawat |
| Endpoint | `GET /episodes/{id}`, `GET /census` |
| Frekuensi | Setiap kali ruang kerja dibuka dan sebelum setiap tulisan |
| Bila gagal | Ruang kerja menampilkan keadaan gagal beserta tombol coba lagi. **Tidak** menampilkan formulir kosong yang seolah siap diisi |
| Arah tulis | **Tidak ada.** Sub-modul ini tidak pernah mengubah episode |

---

## 3. `INT-KEP-03` — Catatan keperawatan ke CPPT

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** ke `ClinicalManagement` |
| Tabel | `TrxPatientIntegratedProgressNote`, `ProfessionType` = perawat |
| Perubahan yang dibutuhkan | **Nol.** Seluruh kolom penghubungnya sudah nullable |
| Pemilik kontrak CPPT | Sub-modul `dokter-rawat-inap` (`CAP-021`) |
| Kebijakan tampil | PRD `CAP-014` aturan 4: catatan keperawatan tampil pada lini masa klinis **sesuai kebijakan**. Kebijakannya belum ditetapkan; sampai itu terjadi, catatan tetap tersimpan dan tetap terbaca dari ruang kerja keperawatan |

---

## 4. `INT-KEP-04` — Rujukan gizi

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** pemicu ke modul Gizi; **baca** status dan ringkasannya |
| Keadaan modul tujuan | **`PLANNED`** — belum ada |
| Yang berlaku sementara | Hasil skrining gizi tersimpan pada pengkajian (`NutritionRiskStatus`, `NutritionRiskScore` — **kolom yang sudah ada**). `VAL-KEP-10` memunculkan saran, bukan penolakan |
| Kapan integrasi sungguhan dibuat | Setelah modul Gizi berdiri. `CAP-027` karena itu **ditunda** pada `04-prd-to-mvp.md` bagian 8 |
| Larangan | Sub-modul ini **MUST NOT** membuat tabel asuhan gizi sendiri — PRD 23.1 |

---

## 5. `INT-KEP-05` — Pemicu tagihan tindakan

| Field | Isinya |
| --- | --- |
| Arah | **Tulis** ke `BillingManagement` |
| Idempotency | Wajib. Kunci disimpan pada `TrxNursingIntervention.IdempotencyKey` |
| Bila gagal | `BillingDispatchStatus` menjadi `Failed`. **Catatan klinisnya tetap tersimpan** — `AC-CAP014-02` |
| Percobaan ulang | Dijalankan terpisah; tidak menyentuh catatan klinis |
| Rekonsiliasi | Daftar tindakan berstatus `Failed` dapat dibaca lewat `GET /{id}/billing-dispatch` |
| Keadaan modul tujuan | `BillingManagement` belum punya kemampuan transaksi. Sampai itu ada, `BillingDispatchStatus` tetap `Pending` dan tidak ada yang hilang |

---

## 6. Integrasi yang **belum dapat ditulis**

| Integrasi | Kenapa |
| --- | --- |
| Pemakaian alat ke persediaan dan Billing (`CAP-016`) | **Kemampuannya `DEFERRED`** lewat `RWI-DEC-089`. Selain kepemilikan tabelnya yang sengaja ditunda, lawan integrasinya pun belum berwujud: `RWI-FACT-015` membuktikan tidak ada modul persediaan/aset di `Areas/`. Kontraknya ditulis setelah modul itu ada |

---

## 7. `INT-KEP-06` — Keutuhan dan koreksi dokumen keperawatan ★ **baru pada `0.3.0`**

| Field | Isinya |
| --- | --- |
| Produsen dan pemilik | **`MedicalRecordManagement`** |
| Konsumen | `ClinicalManagement` dan ruang kerja keperawatan |
| Arah | Rawat Inap **memakai** mesin yang sudah ada, dan **meminta satu perluasan penegakan** |
| Bentuk | Sinkron, di dalam transaksi yang sama dengan finalisasi dokumen |
| Tujuan bisnis | Menandatangani, mengunci, dan **mengoreksi** pengkajian serta catatan tindakan keperawatan tanpa menimpa isi aslinya |
| Keadaan modul tujuan | **Sudah ada dan sudah dipakai.** Catatan terpadu sudah mendaftar ke mesin ini, dan mesinnya sudah mengenal profesi perawat — `RWI-FACT-016` |

### 7.1 Yang diminta

| Hal | Isinya |
| --- | --- |
| Perubahan model | **Nol.** Tabel keutuhan, addendum, dan pendelegasian penulis dipakai apa adanya |
| Nilai enum baru | **Nol.** `Assessment` dan `Procedure` sudah bernomor pada `ClinicalDocumentKind` |
| Pendaftaran | Pengkajian didaftarkan sebagai `Assessment` saat berpindah ke `Completed`; catatan tindakan sebagai `Procedure` saat berpindah ke `Finalized` — keduanya **dalam transaksi yang sama** dengan finalisasinya |
| Bila pendaftaran gagal | Finalisasi ikut batal. Tidak boleh ada dokumen final yang tidak dapat dikoreksi — celah itulah yang ditemukan `RWI-FACT-014` pada dokumen dokter |
| **Perubahan perilaku yang diminta** | Menambahkan `Assessment` dan `Procedure` ke daftar jenis yang **ditegakkan**. Hari ini daftar itu hanya berisi `ProgressNote` sesuai `RM-DEC-019` |

### 7.2 Kenapa perluasan penegakan itu wajib, bukan sekadar rapi

Pembacaan source menemukan dua hal yang bila digabung menghasilkan jebakan diam:

| Temuan | Akibatnya |
| --- | --- |
| `RegisterAsync` **tidak menyaring** jenis dokumen | Pendaftaran pengkajian dan tindakan **berhasil** hari ini juga |
| `EnsureMutableAsync` **membiarkan lewat** jenis yang belum ditegakkan | Penguncian **tidak berlaku**. Dokumen final tetap dapat disunting |

Gabungannya: bila dibangun apa adanya, pengkajian akan **terlihat terdaftar** pada mesin keutuhan sementara
kuncinya tidak pernah menutup. Seluruh mesin status pada
[`state-transition-matrix.md`](./state-transition-matrix.md) kehilangan penjaganya tanpa satu pun pesan
error muncul. Kegagalan yang tidak berbunyi adalah kegagalan yang paling mahal di rekam medis.

### 7.3 Keadaan

| Field | Nilai |
| --- | --- |
| Butir terbuka | **`RWI-OQ-051`** |
| Pemilik jawaban | Pemilik `MedicalRecordManagement`, **belum dinyatakan** |
| Memblokir desain | **Tidak.** Bentuk kontraknya sudah dapat dikunci sekarang |
| Memblokir implementasi | **Ya** — `BE-RWI-057` dan `BE-RWI-062` |
| Bila ditolak | Kembali ke `/qv-grill`. **Jangan** membangun penjaga penguncian sendiri di `ClinicalManagement`; itu mesin koreksi tandingan yang dilarang `RWI-DEC-087` |
