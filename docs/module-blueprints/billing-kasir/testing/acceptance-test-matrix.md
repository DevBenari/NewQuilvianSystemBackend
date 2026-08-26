# Billing dan Kasir — Acceptance Test Matrix

`contract_version: BIL-TEST-0.4` · status **approved** · owner QA + Product/Billing/Finance/Security · approved 20 Agustus 2026. Test data wajib fiktif dan tidak memakai data pasien produksi.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-001` | Satu invoice/encounter | Dua charge source masuk untuk encounter sama | Integration | Satu invoice, dua item |
| `BIL-AT-002` | Source idempotent | Event yang sama dikirim 3 kali | Integration/concurrency | Satu item aktif; response replay konsisten |
| `BIL-AT-003` | Void rule | Void sebelum complete berhasil; sesudah complete ditolak | Domain/API | Histori void dan pesan `BIL-VAL-003` |
| `BIL-AT-004` | Pharmacy actual qty | Order 10, diserahkan 7 | Contract | Charge qty 7, bukan 10 |
| `BIL-AT-005` | Split tender | Tunai 300 ribu sukses, QRIS 700 ribu gagal | E2E | Tunai tetap tercatat; outstanding 700 ribu |
| `BIL-AT-006` | Provider timeout | Callback terlambat | Integration | Tender PENDING; retry tidak menggandakan charge |
| `BIL-AT-007` | Ranap progress | Deposit 8 juta, allocation 5 juta, charge bertambah | E2E | Invoice tetap OPEN; ledger immutable; saldo recalculated |
| `BIL-AT-008` | Deposit release | Final bill di bawah saldo deposit | Domain | Refundable credit terbentuk; bukan auto cash out |
| `BIL-AT-009` | Admin fee harian | Dua encounter rajal pasien sama pada tanggal Jakarta sama | Integration | Fee hanya invoice pertama |
| `BIL-AT-010` | Transfer rajal→ranap | Fee rajal telah dihitung lalu transfer | Domain | Rajal diganti ranap melalui version/adjustment, tidak dobel |
| `BIL-AT-011` | Admin fee rules | Coba diskon admin; insurer cover flag true | Domain | Diskon ditolak; coverage mengikuti policy |
| `BIL-AT-012` | Discount | Promo master otomatis; doctor discount perlu doctor approval | API/security | Promo efektif; doctor share pending lalu approved oleh dokter benar |
| `BIL-AT-013` | Insurance waterfall | Primary sebagian, excess residual, sisanya pasien | Domain | Total coverage ≤ eligible; AR per debtor benar |
| `BIL-AT-014` | Write-off | Partial dan full; maker mencoba self-approve | API/security | Self-approve 403/422; full outcome SETTLED_BY_WRITE_OFF |
| `BIL-AT-015` | Reversal | Reverse write-off posted | Domain/integration | Entry baru membuka AR; histori lama tetap |
| `BIL-AT-016` | Shift variance | Physical cash berbeda lalu close/review/reopen | E2E | Variance persisted; audit authority lengkap |
| `BIL-AT-017` | Late noncash | QRIS settle setelah shift closed | Integration | Tender asal berubah; physical shift tidak berubah |
| `BIL-AT-018` | Departure exception | Death partial family payment dan DAMA unpaid | Domain | Departure boleh; AR ke family/lawful debtor; bukan PAID |
| `BIL-AT-019` | Final AR/AP | Finalisasi insured invoice dengan doctor share | Integration | AR dan AP handoff idempotent; AP not-ready lalu ready by policy |
| `BIL-AT-020` | Optimistic concurrency | Dua user recalculate/allocate versi sama | Concurrency | Satu sukses; satu `409`; tak ada lost update |
| `BIL-AT-021` | Post-final correction | Harga berkurang setelah final | Integration | Credit adjustment/refundable credit dan AR/AP correction |
| `BIL-AT-022` | Authorization | Kasir coba approve write-off/reopen shift | Security | `403`, tidak ada mutation, denied access evidence |
| `BIL-AT-023` | Failure recovery | AR consumer down saat final | Resilience | Invoice FINAL, outbox retry, satu AR saat pulih |
| `BIL-AT-024` | Privacy/a11y | Scan logs/UI keyboard/status | Security/UI | Tak ada field sensitif; status tidak hanya warna; fokus/label valid |

## Exit evidence

Setiap slice harus menyertakan test command dan hasil, request/response tersanitasi, database assertion untuk unique/idempotency, audit assertion, serta screenshot hanya untuk behavior UI yang relevan. Uji nominal memakai decimal boundary/rounding, waktu melewati tengah malam Asia/Jakarta, effective-date boundary, retry, out-of-order event, dan unauthorized paths. Approval blueprint bukan bukti test; build tetap belum dimulai.
