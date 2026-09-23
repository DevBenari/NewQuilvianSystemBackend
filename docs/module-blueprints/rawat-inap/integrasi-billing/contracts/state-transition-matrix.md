# Matriks Transisi Status (State Transition Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |

---

## 1. Siklus Hidup Clearance Kasir di Bangsal (`BillingClearanceStatus`)

| Status Asal | Aksi / Pemicu | Status Tujuan | Syarat Validasi (Preconditions) | Efek Samping Operasional di Bangsal |
|---|---|---|---|---|
| `None` | DPJP menerbitkan instruksi pulang medis (`DischargeRequested`) | `Pending` | Episode rawat inap aktif; resume medis telah diisi oleh dokter. | Lencana berubah menjadi kuning `Menunggu Kasir`; tombol pulang fisik terkunci. |
| `Pending` | Kasir menerbitkan persetujuan lunas / penjaminan | `Cleared` | Tagihan lunas / asuransi approve; webhook kasir diterima. | Lencana berubah menjadi hijau `Clearance Disetujui`; tombol pulang fisik aktif. |
| `Cleared` | Kasir mencabut persetujuan akibat tagihan susulan farmasi/lab | **`Revoked`** | Webhook pencabutan kasir diterima sebelum pasien keluar fisik. | **Auto-Reblock:** Tombol pulang fisik seketika terkunci; banner merah muncul. |
| `Revoked` | Kasir menerbitkan ulang persetujuan setelah tagihan susulan dilunasi | `Cleared` | Tagihan susulan telah lunas; webhook clearance diterima. | Tombol pulang fisik aktif kembali normal. |
| `Revoked` / `Pending` | Supervisor Bangsal menyetujui pemulangan darurat klinis | **`Overridden`** | Hak akses `InpatientSupervisor:Override`, alasan klinis wajib (>= 20 kar), PIN valid. | Tombol pulang fisik aktif darurat; stempel audit darurat tersimpan. |
| `Cleared` / `Overridden` | Perawat mengonfirmasi pasien keluar ruangan fisik | `Finalized` | Tombol `Konfirmasi Pasien Pulang Fisik` ditekan oleh perawat. | `PhysicallyLeftAt` terkunci; event `BED_RELEASED` terbit; bed dikosongkan. |

---

## 2. Siklus Hidup Hunian Tempat Tidur (`InpBedPlacement.OccupancyStatus`)

| Status Asal | Aksi / Pemicu | Status Tujuan | Syarat Validasi | Efek pada Billing Kasir |
|---|---|---|---|---|
| `Assigned` | Pasien tiba di kamar dan menempati tempat tidur | `Occupied` | Konfirmasi penempatan fisik oleh perawat. | Event `BED_OCCUPIED` dikirim; Room charge mulai dihitung aktif. |
| `Occupied` | Pasien pindah kamar / kelas saat billing `OPEN` | `Superseded` | Otorisasi Supervisor, alasan mutasi terisi, status kasir `OPEN`. | Event `OCCUPANCY_CORRECTED` dikirim; Kasir repricing tarif kamar. |
| `Occupied` | Pasien keluar ruangan secara fisik | `Released` | Status clearance `Cleared` atau `Overridden`; perawat konfirmasi pulang. | Event `BED_RELEASED` dikirim; Jam hunian ditutup; bed masuk status pembersihan. |

---

## 3. Siklus Hidup Pesan Outbox Integrasi (`OutboxStatus`)

| Status Asal | Aksi / Pemicu | Status Tujuan | Syarat Validasi | Efek Samping Sistem |
|---|---|---|---|---|
| `Pending` | Background worker mengambil batch pesan | `Processing` | Worker aktif; pesan belum diambil worker lain. | Pesan dikirim ke broker / HTTP listener Billing. |
| `Processing` | Billing mengembalikan respons ACK sukses (HTTP 200/201) | `Published` | Pengiriman berhasil terkonfirmasi. | `PublishedAtUtc` dicatat; pesan dipertahankan selama masa retensi 30 hari. |
| `Processing` | Kegagalan jaringan / HTTP error 5xx / timeout | `Failed` | Broker menolak atau tidak merespons dalam 5 detik. | `RetryCount += 1`; hitung `NextRetryAtUtc` via exponential backoff. |
| `Failed` | Waktu `NextRetryAtUtc` tercapai | `Processing` | Worker mengambil kembali pesan untuk percobaan ulang. | Upaya pengiriman ulang dijalankan. |
| `Failed` | Percobaan ulang gagal terus menerus (`RetryCount >= 10`) | `DeadLetter` | 10 kali retry gagal berturut-turut. | Pengiriman otomatis dihentikan; peringatan darurat dikirim ke tim IT. |

---

## 4. Transisi Ilegal yang Ditolak Sistem (Illegal State Transitions)

Sistem wajib menggagalkan upaya transisi ilegal berikut dan melemparkan exception validasi:

1. **`Pending` → `Finalized` Langsung:** Dilarang memulangkan pasien secara fisik tanpa melalui status `Cleared` kasir atau `Overridden` supervisor.
2. **`Revoked` → `Finalized` Tanpa Override:** Dilarang melepaskan pasien fisik saat clearance sedang dibatalkan kasir, kecuali supervisor mengeksekusi override beralasan.
3. **Mutasi Kamar saat Tagihan Kasir `CLOSED`:** Dilarang mengubah status kamar atau kelas perawatan bila folio kasir telah ditutup.
4. **Penerbitan Event Outbox Tanpa Idempotency Key:** Dilarang menyimpan entri outbox dengan kunci idempoten kosong atau format tidak standar.
5. **Hard-Delete Riwayat Hunian:** Dilarang menghapus baris `InpBedPlacement` yang lama saat koreksi kamar; wajib menggunakan transisi ke `IsSuperseded = true`.
