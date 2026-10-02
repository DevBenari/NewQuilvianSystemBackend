# Validation Matrix — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-VAL-001@1.0.0` |
| Status | `draft` |
| Source | Decision revision `10`, domain architecture revision `1` |

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Identity wajib | Semua milestone | `MilestoneFactId`, version, encounter, source, effect, idempotency tersedia | Data milestone belum lengkap | `BIL_SOURCE_INVALID` |
| Quantity dan unit berpasangan | Component | Quantity ada atau Unit ada, tidak boleh salah satu | Quantity dan unit harus diisi bersama | `BIL_SOURCE_INVALID` |
| Snapshot JSON valid | Tariff/rule/rounding | Nilai object JSON dan ukuran <= 20.000 karakter | Snapshot tarif atau aturan tidak valid | `BIL_SOURCE_INVALID` |
| Duplicate operation | Processing | Consumer+operation+idempotency key sudah ada | Permintaan sebelumnya sudah diproses; hasil canonical dikembalikan | `BIL_REPLAY` |
| Fingerprint konflik | Processing | Key sama tetapi input material berbeda | Kunci idempotency sudah dipakai untuk data berbeda | `BIL_IDEMPOTENCY_CONFLICT` |
| Versi stale | Clinical fact | Versi incoming lebih kecil dari applied | Versi fakta sudah lebih lama dan ditolak | `BIL_VERSION_CONFLICT` |
| Over-allocation | Allocation | Total allocation melebihi net eligible charge | Alokasi payer melebihi nilai yang boleh ditanggung | `BIL_OVER_ALLOCATION` |
| Rule tidak tersedia | Partial charge | Tidak ada rule approved/effective | Komponen menunggu tinjauan finansial | `BIL_CALCULATION_REVIEW_REQUIRED` |
| Self approval | Approval | Effective maker sama dengan checker | Pengaju tidak boleh menyetujui permintaannya sendiri | `BIL_SELF_APPROVAL` |
| Close prerequisite | Folio close | Ada outcome unknown, review, allocation, atau reconciliation belum selesai | Folio belum dapat ditutup karena masih ada pekerjaan wajib | `BIL_FOLIO_NOT_READY_TO_CLOSE` |
| Clinical financial mutation | Clinical/Pharmacy endpoint | Endpoint mencoba Paid/waiver/void canonical | Status finansial hanya dapat diubah oleh pemilik finansial | `BIL_FINANCIAL_OWNER_REQUIRED` |



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.0` |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Owner | Billing + API authority |
| Traceability | `RJ-E2E-DEC-005`, `006`, `008`, `009`, `010`, `012`, `014` |

Kode `RJE-VAL-*` tampil ke pengguna lewat API atau layar. Kode `*_NOT_FOUND`, `NOT_OUTPATIENT`, dan
sejenisnya adalah kode sebab di antrean rekonsiliasi, dibaca petugas Billing.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Domain klinis Rawat Jalan tidak boleh dicatat manual | `POST /billing/invoices/from-source` | `SourceDomain` ∈ {`PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSULTATION`} dan kunjungan `Outpatient` | "Pelayanan Rawat Jalan ditagihkan otomatis dari pelayanan klinis dan tidak dapat dicatat lewat jalur ini." | `RJE-VAL-010` (`422`) |
| Tarif wajib ada | Jembatan | Tidak ada `MstTariff` berlaku pada tanggal pelayanan menurut `V2.7.4` | "Tarif untuk pelayanan ini belum tersedia pada tanggal pelayanan. Lengkapi tarif, lalu kirim ulang." | `TARIFF_NOT_FOUND` |
| Tagihan tidak pernah Rp0 karena tarif hilang | Jembatan | `UnitPrice` hasil resolver = 0 **dan** tarif tidak ber-`NormalPrice` 0 yang sah | Sama dengan di atas | `TARIFF_NOT_FOUND` |
| Kunjungan bukan Rawat Jalan | Jembatan | `EncounterType ≠ Outpatient` | (tidak tampil; baris `NotApplicable`) | `NOT_OUTPATIENT` |
| Konteks di luar scope | Jembatan | `SourceContext` di luar lima konteks | (tidak tampil) | `SOURCE_OUT_OF_SCOPE` |
| Pengulangan Radiologi karena kesalahan rumah sakit | Jembatan | `RuleSnapshot.repeatCause = InternalHospitalError` | (tidak tampil) | `REPEAT_INTERNAL_ERROR` |
| Tindakan gratis atau tidak ditagihkan | Jembatan | `IsFreeOfCharge = true` atau `IsBillable = false` | (tidak tampil) | `NOT_BILLABLE` |
| Adjustment ditolak Billing | Jembatan | `CreateAdjustmentAsync` menolak | "Penyesuaian tagihan untuk pelayanan ini tidak dapat dibuat otomatis. Tangani dari antrean rekonsiliasi." | `ADJUSTMENT_REJECTED` |
| Batas percobaan tercapai | Pekerja | Percobaan = `MaxAttemptCount` | "Pengiriman otomatis sudah dicoba berulang kali dan belum berhasil." | `RETRY_EXHAUSTED` |
| Kebijakan kirim ulang tidak aktif | Pekerja | Tidak ada `MstBillingSyncPolicy` aktif untuk jalurnya | "Pengiriman ulang otomatis sedang dimatikan. Item menunggu penanganan manual." | `SYNC_POLICY_INACTIVE` |
| Baris lama sebelum jembatan | Migration | `RJ-E2E-DEC-014` | "Pelayanan ini tercatat sebelum penagihan otomatis aktif. Periksa apakah sudah ditagih manual." | `LEGACY_PRE_BRIDGE` |
| Item yang sudah masuk tidak dikirim ulang | `POST /charge-reconciliations/{itemType}/{id}/retry` | Status `Synced` atau `Resolved` | "Item ini sudah tercatat di tagihan dan tidak perlu dikirim ulang." | `RJE-VAL-020` (`422`) |
| Keputusan manual bersifat final | `retry` / `resolve` | Status `Resolved` | "Item ini sudah diselesaikan secara manual." | `RJE-VAL-021` (`409`) |
| Catatan penyelesaian wajib | `POST .../resolve` | `Note` kosong, < 10 atau > 500 karakter | "Tuliskan alasan penyelesaian, minimal 10 karakter." | `RJE-VAL-022` (`422`) |
| Jenis penyelesaian sah | `POST .../resolve` | `Resolution` di luar `BILLED_MANUALLY`, `NOT_BILLABLE`, `DUPLICATE` | "Pilih jenis penyelesaian yang tersedia." | `RJE-VAL-023` (`422`) |
| Jenis item sah | `retry` / `resolve` | `itemType` bukan `CLINICAL_FACT`/`CHARGE_LINE` | "Jenis item tidak dikenali." | `RJE-VAL-024` (`422`) |
| Item sedang diproses | `retry` / `resolve` | Pekerja sedang memegang item yang sama | "Item sedang diproses sistem. Muat ulang beberapa saat lagi." | `RJE-VAL-025` (`409`) |
| Batas kebijakan | `PUT /billing-sync-policies/{id}` | `MaxAttemptCount` di luar 0–20; `BaseDelaySeconds` di luar 10–3600; `MaxDelaySeconds` < `BaseDelaySeconds` atau > 86400 | "Angka kebijakan di luar batas yang diizinkan." | `RJE-VAL-030` (`422`) |
| Kebijakan diubah bersamaan | `PUT /billing-sync-policies/{id}` | `RowVersion` tidak cocok | "Kebijakan sudah diubah pengguna lain. Muat ulang, lalu ulangi perubahan." | `RJE-VAL-031` (`409`) |
| Kunjungan tidak ditemukan | `GET /encounter-billing-summaries/{encounterId}` | `encounterId` kosong atau tidak ada | "Kunjungan tidak ditemukan." | `RJE-VAL-040` (`404`) |

**Contoh batas kebijakan:** `MaxAttemptCount = 25` ditolak (`RJE-VAL-030`). `BaseDelaySeconds = 60`
dengan `MaxDelaySeconds = 30` juga ditolak karena jeda maksimum lebih kecil dari jeda awal.
`MaxAttemptCount = 0` **sah** dan berarti tidak ada kirim ulang otomatis — semua kegagalan langsung
masuk antrean.


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`, `draft`)

`last_changed_in`: `RJ-DOC-ENCLIST-001@1.0.0` · Owner: Sukma Giri

| Kode | Kondisi | Endpoint | Aturan | Pesan untuk petugas | HTTP |
|---|---|---|---|---|---|
| `RJDP-VAL-001` | Pengguna tanpa cakupan | Semua `GET`, `PATCH cancel` | Tidak terhubung ke dokter, tidak di cluster perawat, tanpa `ReadAll` | "Akun Anda belum terhubung ke data dokter atau cluster perawat. Hubungi admin untuk pengaturan akses." | `403` |
| `RJDP-VAL-002` | Alasan kosong / terlalu panjang | `PATCH cancel` | Setelah dipangkas: 1-250 karakter | "Alasan pembatalan wajib diisi, maksimal 250 karakter." | `400` |
| `RJDP-VAL-003` | Kunjungan tidak ada atau di luar cakupan | `PATCH cancel` | Bukan RJ berklinik, terhapus, atau tidak terlihat oleh pengguna | "Kunjungan tidak ditemukan." | `404` |
| `RJDP-VAL-004` | Sudah dibatalkan | `PATCH cancel` | `IsCancel = true` | "Kunjungan sudah dibatalkan." | `400` |
| `RJDP-VAL-005` | Konsultasi masih aktif | `PATCH cancel` | Status 6 dan ada konsultasi tidak batal | "Konsultasi masih aktif. Selesaikan atau batalkan konsultasi lewat workspace dokter." | `400` |
| `RJDP-VAL-006` | Status tidak boleh dibatalkan | `PATCH cancel` | Status 7-11 atau `CompletedAt` terisi | "Kunjungan dengan status {nama status} tidak dapat dibatalkan." | `400` |
| `RJDP-VAL-007` | Rentang tanggal salah | `GET /`, `/summary` | `mode=range` tanpa tanggal, `dateFrom > dateTo`, atau lebih dari 31 hari | "Rentang tanggal tidak valid. Maksimal 31 hari." | `400` |
| `RJDP-VAL-008` | Pencarian terlalu panjang / `pageSize` > 100 | `GET /` | — | "Saringan tidak valid." | `400` |

**Contoh `RJDP-VAL-006`:** kunjungan status 7 → "Kunjungan dengan status Konsultasi Selesai tidak
dapat dibatalkan."

**Contoh `RJDP-VAL-003`:** dr. A mencoba membatalkan kunjungan pasien dr. C lewat API → `404`,
bukan `403`, supaya keberadaan kunjungan di luar cakupan tidak terbaca.
