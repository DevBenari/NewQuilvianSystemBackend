# Integration Contract — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0 |
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
| Pemakaian alat ke persediaan dan Billing (`CAP-016`) | **Kepemilikan tabelnya belum diputuskan** — `02-backend-architecture.md` bagian 2.3. Menulis kontrak integrasi tanpa tahu siapa pemilik datanya berarti mengarang kepemilikan |
