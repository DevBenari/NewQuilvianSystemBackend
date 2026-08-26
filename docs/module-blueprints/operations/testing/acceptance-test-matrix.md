# Acceptance Test Matrix — Modul Operasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `OPS-REQ-001` | Dokter membuat kasus dengan satu tindakan utama | API/domain integration | `Requested`, procedure tertaut, history tercipta |
| `OPS-REQ-001` | Tindakan utama kosong/duplikat aktif | Negative | `400/409`, tidak ada kasus parsial |
| `OPS-REQ-002` | Transition normal sampai `Completed` | State integration | Urutan status dan history benar |
| `OPS-REQ-002` | Start dari `Scheduled` | Negative | `409 InvalidStateTransition` |
| `OPS-REQ-003` | Dua kasus bentrok ruang atau anggota tim | Concurrency | Hanya satu jadwal berhasil; lainnya `409` |
| `OPS-REQ-004` | Tim minimum/credential tidak lengkap | Negative/integration | Penjadwalan ditolak dengan pesan jelas |
| `OPS-REQ-005` | Sign-off lengkap membuat `Ready` | Domain | Sistem transition tepat sekali |
| `OPS-REQ-005` | Emergency bypass tanpa alasan | Negative | `422`; audit tidak tercipta |
| `OPS-REQ-006` | Finalisasi record lalu edit langsung | Negative | Ditolak; addendum tetap bisa dibuat |
| `OPS-REQ-007` | Implant tanpa serial/batch | Negative | `422 OPR009` |
| `OPS-REQ-007` | Retry pemakaian dengan key sama | Idempotency | Satu usage dan satu mutasi downstream |
| `OPS-REQ-008` | Handover belum diterima | State negative | Case tidak `Completed` |
| `OPS-REQ-009` | Cancel setelah `In Progress` | Negative | Ditolak; gunakan outcome `StoppedEarly` |
| `OPS-REQ-010` | Retry charge dan reversal | Contract test | Billing tidak menggandakan component |
| `OPS-REQ-011` | Notifikasi gagal | Resilience | Transaksi klinis tetap sukses; delivery `Failed` dapat retry |
| Security | Aktor tanpa permission/business authority | Authorization | `403`, tanpa perubahan data |
| Privacy | Custom logger memproses command klinis | Logging test | Tidak ada diagnosis/catatan/anestesi di log |
| Concurrency | Dua pengguna memakai expectedVersion sama | Integration | Satu sukses; satu `409` dan diminta reload |
| Frontend | loading/empty/error/stale/duplicate submit | Component/E2E | State terbaca dan tidak ada submit ganda |

Build/test source belum dijalankan karena blueprint tidak mengimplementasikan kode.
