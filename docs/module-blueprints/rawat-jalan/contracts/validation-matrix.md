# Validation Matrix — Rawat Jalan Billing

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

