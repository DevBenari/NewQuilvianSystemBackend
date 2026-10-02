# Integration Contract — Modul Operasi

Contract `opr-integration-v1`; status `approved`; approved by pemilik kebutuhan pada 2026-08-21; compatibility additive.

| ID | Producer → Consumer | Pemicu | Payload minimum | Idempotency/correlation | Gagal dan rekonsiliasi |
|---|---|---|---|---|---|
| `OPR-INT-001` | Operasi → Inventory/Farmasi | Pemakaian/retur/waste tercatat | case, encounter, item ID, quantity/unit, outcome, batch/serial, occurredAt | `case:usage:revision`; correlation case ID | Simpan `Pending/Failed`, retry; downstream tidak boleh mutasi ganda |
| `OPR-INT-002` | Operasi → Billing | Layanan aktual selesai/material berubah | case, encounter, procedure/tariff, komponen, quantity, tipe create/correct/reverse | `case:charge:component:revision` | Billing authoritative; simpan hasil dan rekonsiliasi per component |
| `OPR-INT-003` | Clinical → Operasi | Validasi procedure/consent | patient procedure, consent type/status/version | correlation case ID | `Ready` ditolak bila invalid, kecuali emergency bypass sah |
| `OPR-INT-004` | HR/Credentialing → Operasi | Penjadwalan/start | workforce, role, active/privilege validity, period | correlation schedule ID | Tolak assignment invalid; bila layanan belum ada tandai dependency blocked |
| `OPR-INT-005` | Operasi → Unit tujuan | Recovery release/handover | encounter, destination, condition summary, device/therapy/risk/instruction, sender | handover ID | Case belum `Completed` sampai diterima; retry tidak membuat handover kedua |
| `OPR-INT-006` | Operasi → Notification | Jadwal/status/handover berubah | event, recipient references, safe summary | event ID | Gagal notifikasi tidak rollback transaksi klinis; retry terpisah |

Transport sinkron/asinkron diputuskan saat capability downstream tersedia. Kontrak bisnis tetap: operasi lokal harus bertahan saat downstream sementara gagal, delivery dapat dipantau, dan retry idempotent.

## Amplop kejadian outbox (`BE-OPR-009`, sejak 29 September 2026)

Modul Operasi mengeluarkan kejadian ke outbox-nya sendiri dalam bentuk yang tetap, lalu berhenti
di situ. Penerjemahan ke Billing, Inventory, SATUSEHAT, atau consumer lain adalah pekerjaan
integration layer. Tidak ada nama, kode, atau struktur milik consumer tertentu di dalam amplop
ini, dan modul Operasi tidak bergantung pada ketersediaan mereka.

Amplopnya disimpan pada `OprIntegrationDelivery.PayloadJson` sebagai `jsonb`, dibekukan saat
kejadian terjadi. Ia tidak disusun ulang saat dibaca, sehingga pengiriman ulang membawa isi yang
persis sama dengan pengiriman pertama walaupun jadwal, tim, atau tindakan kasusnya sudah berubah.

| Bidang | Isi |
|---|---|
| `eventId` | Identitas kejadian. Diturunkan dari tujuan dan kunci idempotency-nya, bukan diacak, sehingga kejadian yang sama selalu memperoleh nilai yang sama. Berindeks unik di basis data. |
| `eventType` | Nama kejadian bisnis: `operating-room.material-usage.recorded` atau `operating-room.charge.registered` |
| `eventVersion` | Versi bentuk amplop. `1.0` sekarang; `0` menandai baris yang dibuat sebelum kontrak ini berlaku |
| `occurredAt` | Waktu kejadiannya, UTC. Bukan waktu pengirimannya |
| `caseId`, `caseNumber` | Kasus operasinya |
| `patientId`, `encounterId` | Pasien dan kunjungannya |
| `serviceRequestId` | `TrxPatientProcedure` milik tindakan utama. Modul Operasi tidak memiliki entitas permintaan tersendiri; ia memakai ulang tindakan pasien yang sudah ada |
| `case` | Status, outcome, tipe, prioritas, lateralitas, dan waktu permintaan saat kejadian terjadi |
| `procedures[]` | `patientProcedureId`, kode, nama, penanda utama, urutan |
| `performers[]` | `workforceId`, peran pada tim, penanda ketua — anggota yang sedang berlaku saat kejadian terjadi |
| `material` | Terisi untuk kejadian pemakaian: usage, item, jenis, jumlah, satuan, outcome, batch/serial, revisi, pencatat |
| `charge` | Terisi untuk kejadian tagihan: komponen dan revisinya |

Jaminan yang berlaku:

- **Satu kejadian bisnis menghasilkan satu pesan.** Dijaga dua lapis: pemeriksaan di service, dan
  indeks unik pada `EventId` serta pada `(Destination, IdempotencyKey)`.
- **Pesan ditulis dalam transaksi yang sama dengan perubahan bisnisnya**, sehingga tidak pernah ada
  pemakaian material tanpa pesan, dan tidak pernah ada pesan tentang pemakaian yang gagal tersimpan.
- **Pengiriman tidak terjadi di dalam transaksi itu.** Kegagalan jaringan atau kegagalan consumer
  hanya mengubah status barisnya menjadi `Failed`; tindakan klinis yang sudah dilakukan pada pasien
  tidak ikut dibatalkan.
- **Dapat diproses ulang.** `PATCH .../deliveries/{id}/retry` mengembalikan baris `Failed` ke
  `Pending` tanpa membuat pesan baru dan tanpa mengubah `EventId`. Pesan yang sudah diterima
  consumer tidak dapat dikirim ulang.
- **Dapat dibaca integration layer.** `GET .../deliveries/{id}/event` mengembalikan amplop yang
  dibekukan itu apa adanya.

Payload tidak membawa seluruh catatan klinis. Informasi sensitif hanya dikirim bila diperlukan oleh consumer dan pengguna berwenang.
