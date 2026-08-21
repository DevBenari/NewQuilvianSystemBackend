# Acceptance Test Matrix — Rawat Jalan Billing

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| Exactly-once | Request milestone dikirim dua kali dengan key/fingerprint sama | Integration | Satu charge line dan replay canonical |
| Idempotency conflict | Key sama dikirim dengan amount/snapshot berbeda | Integration | HTTP 409 `BIL_IDEMPOTENCY_CONFLICT` |
| Stale version | Version 1 masuk setelah version 2 applied | Integration | HTTP 409 `BIL_VERSION_CONFLICT`; histori version 2 tetap |
| Outcome unknown | Commit terjadi lalu response timeout | Integration/recovery | Status query menemukan outcome; retry tidak menggandakan charge |
| Partial component | Satu component berhasil, component lain gagal | Domain/integration | Component applied tetap ada; failed component visible |
| Lab milestone | Requested/Collected/Received tidak membentuk final charge; Accepted membentuk eligibility | Domain | Transition dan charge eligibility sesuai rule |
| Radiology safety | Acquisition dimulai tanpa safety clearance | Domain/security | Request ditolak dan audit reason tersedia |
| Multi-payer | Net Rp1.000.000 dialokasikan A Rp600.000, B Rp250.000, patient Rp150.000 | Domain | Total tepat Rp1.000.000; tidak over-allocate |
| Payer replacement | Payer diganti setelah partial approval | Integration/domain | Allocation version baru; keputusan lama tetap terlihat |
| Financial correction | Void/reversal/refund diajukan tanpa approval | API/security | Ditolak; canonical charge tidak berubah |
| Maker-checker | Maker mencoba approve request sendiri | Authorization | Ditolak `BIL_SELF_APPROVAL` |
| Folio close | Mandatory reconciliation masih pending | Domain/API | Close ditolak `BIL_FOLIO_NOT_READY_TO_CLOSE` |
| Clinical boundary | Pharmacy mencoba menandai Paid | Authorization/contract | Tidak ada clinical endpoint authoritative untuk Paid |
| Urgent dispensing | Billing unavailable tetapi clinical exception disahkan | Workflow/integration | Dispensing tercatat; financial obligation tetap outstanding |
| External adapter | Adapter belum punya UAT/contract/credential | Release gate | Adapter tetap disabled; manual flow tetap berjalan |
| Privacy | Audit action terjadi pada source klinis sensitif | Security | Logger hanya menyimpan reference/hash, bukan raw payload |

