# Permintaan Persetujuan — Lingkup kunci Lunas: rawat inap dan IGD *Ditagih Kemudian*

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-020` |
| `tanggal` | 2026-10-08 |
| `pengaju` | `design-business-module`, menurunkan Amendment Pass putaran 27 |
| `rujukan` | `LAB-DEC-222`..`LAB-DEC-224`; decision log revision 92; capability map revision 8 (`LAB-CONFLICT-018`); `02-backend-architecture.md` bagian 27; `AC-314`..`AC-317` |
| `status` | ✅ **`disetujui`** 2026-10-08 — Yoga Aji Pratama, pemilik modul: *"setuju keenam butir"*; **keenam butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen tiga kontrak. Keputusan bisnisnya **sudah** diambil 2026-10-08; yang diminta adalah persetujuan **bentuk kontraknya**. **Kode belum diubah** — implementasi menunggu persetujuan ini |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Pasien rawat inap dan IGD yang membayar Tunai saat ini tidak bisa diproses pemeriksaan Lab-nya: rawat inap tertahan
sampai pulang, IGD tertahan selamanya karena tagihan Lab IGD tidak pernah terbit. Usulan ini menambah satu nilai
status pembayaran, **Ditagih Kemudian**, yang meloloskan kedua jenis kunjungan itu tanpa mengubah aturan bagi pasien
rawat jalan, kiosk, MCU, dan Telemedicine. Tidak ada tabel, kolom, endpoint, maupun izin baru.

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r43` | [38](../contracts/api-contract.md) | Nilai `paymentStatus` `Deferred`; `start-process` lolos bagi rawat inap/IGD |
| `LAB-STATE-v1` | `r10` | [12](../contracts/state-transition-matrix.md) | Syarat Proses dilonggarkan bagi rawat inap/IGD |
| `LAB-VAL-v1` | `r19` | [21](../contracts/validation-matrix.md) | `VAL-152`, `VAL-153` dipersempit |

`LAB-PERM-v1` dan `LAB-INT-v1` **tidak berubah**: nol aksi baru, dan `EncounterType` milik Registrasi sudah dibaca
daftar pantau.

## 3. Enam butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Nilai baru | **`Deferred`** berlabel *Ditagih Kemudian*, `isPaymentCleared` = `true` | — |
| 2 | Nominal | `outstandingAmount` diisi sisa tagihan untuk `Deferred` (0 bila belum terbit) — mengubah bunyi `r42` "0 selain `Unpaid`" | Nominal disembunyikan; analis tidak tahu besar tagihan pasien bangsal |
| 3 | **Tafsir `LAB-DEC-224`** | Pasien rawat inap/IGD Tunai yang **sudah lunas** tetap *Lunas* (`Paid`), bukan *Ditagih Kemudian* — label mengikuti keadaan sebenarnya | Semua pasien Tunai rawat inap/IGD selalu *Ditagih Kemudian*, termasuk yang sudah lunas |
| 4 | Jenis kunjungan tidak terbaca | Diperlakukan **dikunci** seperti rawat jalan (fail-closed, sejalan `LAB-DEC-223`) | Kunjungan berdata rusak lolos tanpa bayar |
| 5 | Penolakan Proses | `VAL-152`/`VAL-153` tidak berlaku bagi `Inpatient`/`Emergency`; bunyi pesan tidak berubah | — |
| 6 | Urutan rilis | Backend dan frontend dirilis bersamaan (frontend lama akan menampilkan *Lunas* untuk `Deferred`) | Rilis terpisah dengan label keliru sementara |

## 4. Cara menjawab

Cukup *"setuju keenam butir"*, atau sebutkan nomor butir yang ingin diubah.
