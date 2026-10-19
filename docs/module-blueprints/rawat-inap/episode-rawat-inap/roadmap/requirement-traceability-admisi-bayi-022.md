# Requirement Traceability — Perbaikan Admisi Bayi 022

| Requirement | Backend | Frontend | Bukti target | Status |
| --- | --- | --- | --- | --- |
| Calon ibu dari kasus obstetri `InProgress`/`Completed`, tidak bergantung ruang operasi | `BE-RWI-244` | `FE-RWI-257` | Candidate endpoint dan langkah 1 | ✅ Source selesai |
| Tujuh langkah tanpa payment/deposit | `BE-RWI-247` | `FE-RWI-257` | Kontrak episode dan step resolver | ✅ Source selesai |
| Nama `By. Ny.`, RM sendiri, urutan otomatis per kasus | `BE-RWI-245` | `FE-RWI-258` | Endpoint newborn, sequence, form identitas | ✅ Source selesai |
| Idempotency dan concurrency urutan | `BE-RWI-245` | `FE-RWI-258` | Hash request, lock DB, index unik, header stabil | ✅ Source selesai |
| Penjamin mengikuti ibu tanpa memalsukan kartu bayi | `BE-RWI-246` | `FE-RWI-259` | Snapshot provenance dan payload encounter | ✅ Source selesai |
| Invoice bayi terpisah; deposit ibu ranah Kasir/AR | `BE-RWI-247` | `FE-RWI-259` | Event existing dan copy review baca-saja | ✅ Scope Admisi selesai; Billing tetap dependency |
| Ringkasan besar hanya saat konfirmasi | — | `FE-RWI-259` | `NewbornReviewStep` | ✅ Source selesai |
| Bed bayi dan service unit memakai aturan yang sama | `BE-RWI-247` | `FE-RWI-259` | Shared predicate dan recovery UI | ✅ Source selesai |
| Tombol pembatalan existing tidak berubah | — | `FE-RWI-257` s.d. `259` | Review diff | ✅ Diverifikasi tidak diubah |
| Regresi dan sign-off | `BE-RWI-244` s.d. `247` | `FE-RWI-257` s.d. `259` | Build, lint, unit suite, live browser bila tersedia | ⚠️ Build/lint/targeted unit lulus; live pending migration |

Dependency eksternal `DEP-BIL-01` tetap di luar scope implementation ini: penggunaan saldo deposit
ibu pada invoice bayi memerlukan planning dan task Billing/Kasir/AR tersendiri.

