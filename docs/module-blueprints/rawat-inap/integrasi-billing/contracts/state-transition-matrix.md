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

---

## 5. Perubahan pada `contract_version` `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `1.1.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Owner | Muhammad Hamzah; sisi Billing Yasmina (`RWI-DEC-192`) |
| `input_revision` | Decision log revision `30`; PRD Finishing v`0.4`; gate `1.9` |
| Traceability | `BP-RWF-01`, `BP-RWF-02`; `RWI-DEC-166`, `167`, `169`, `186`, `187` |

**Bagian 1 di atas tidak berlaku lagi.** Status `BillingClearanceStatus` di sisi Rawat Inap tidak lagi berpindah; status izin kasir hanya bergerak di Billing. Yang dicatat Rawat Inap hanya pengamatan (5.2).

### 5.1 Pesan outbox Rawat Inap

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Kejadian bisnis tersimpan | `Pending` | Sistem, dalam transaksi bisnis | Payload lolos daftar putih (`INV-RWF-05`) | Transaksi bisnis gagal; tidak ada event hantu |
| `Pending` atau `Failed` (jatuh tempo coba ulang) | Worker mengambil | `Processing` | Worker | `ProcessingStartedAtUtc` diisi | — |
| `Processing` | Billing mengembalikan tanda terima `Accepted = true` | `Published` | Worker | `AcknowledgedReceiptId` diisi | **Dilarang** tanpa tanda terima (`INV-RWF-04`) |
| `Processing` | Billing menolak atau gagal dipanggil | `Failed` | Worker | `RetryCount + 1`; `NextRetryAtUtc` menurut backoff | — |
| `Processing` | Masa sewa terlewati (aplikasi mati di tengah pengiriman) | `Pending` | Worker berikutnya | `ProcessingStartedAtUtc` lebih tua dari masa sewa | Pesan tersangkut selamanya (`FIN-FACT-06`) |
| `Failed` | Batas coba ulang tercapai | `DeadLetter` | Worker | `RetryCount ≥ MaxRetry` | Tampil di `GET integration-outbox` |
| `Published` atau `DeadLetter` | Putar ulang | `Pending` | Pemegang `InpatientIntegrationOutbox : Replay` | Episode masih aktif; `ReplayBatchId` diisi | — |
| `Published` | Tindakan lain apa pun | — | — | — | Ditolak; pesan terkirim bersifat final kecuali lewat putar ulang |

### 5.2 Jejak pengamatan status kasir pada episode

Ini bukan status episode. `InpEpisodeStatus` tetap lima nilai (`RWI-DEC-009`); keluar ruangan tetap bukan perubahan status (`RWI-RULE-036`).

| Kejadian | Kolom yang diisi | Nilai | Siapa | Dapat diubah |
|---|---|---|---|---|
| Keluar ruangan dicatat | `DepartureClearanceObserved`, `DepartureClearanceObservedAt`, `DepartureClearanceWarningAcknowledged` | Status kasir dari bacaan langsung, atau `Unreadable` | Pencatat keluar ruangan | Tidak. Keluar ruangan tidak dapat dibatalkan (`RWI-RULE-036`) |
| Penutupan dengan override | `ClosureClearanceObserved` | Status kasir dari bacaan langsung, atau `Unreadable` | Pemegang `InpatientDischarge : CloseOverride` | Tidak; pembukaan kembali mengikuti `RWI-RULE-020` |
| Penutupan normal | `ClosureClearanceObserved` | Selalu `Cleared` | Petugas admisi, supervisor | Tidak |

### 5.3 Episode — gerbang keluar ruangan dan penutupan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `DischargePending`, bed terisi | Catat keluar ruangan, status kasir `CLEARED` | `DischargePending`, bed `Available` | `InpatientDischarge : RecordDeparture` | — | — |
| `DischargePending`, bed terisi | Catat keluar ruangan, status kasir bukan `CLEARED` atau tidak terbaca | `DischargePending`, bed `Available` | Sama | `ClearanceWarningAcknowledged = true` | 409 `INP-DEP-001`, tidak ada perubahan |
| `Admitted` | Catat keluar ruangan | — | — | — | 422; DPJP belum memutuskan pulang |
| `DischargePending` | Tutup normal | `Closed` | `InpatientDischarge : Close` | Bacaan langsung `CLEARED` **dan** syarat `RWI-RULE-010` lain | 422 `INP-CLS-010` atau `INP-CLS-011` |
| `DischargePending` | Tutup dengan override | `Closed`, `IsClosedWithoutFinancialClearance = true` | `InpatientDischarge : CloseOverride` | Alasan tidak kosong dan tidak hanya tanda baca | 400 `INP-CLS-012`; 403 tanpa permission. **Nama peran tidak diperiksa** |
| `Closed` | Tutup lagi | — | — | — | 409 |

**Penguncian ulang otomatis** (`RWI-DEC-158`) tidak punya transisi di Rawat Inap: begitu Billing mencabut izin, bacaan berikutnya mengembalikan `REVOKED`, dan penutupan normal ditolak.

### 5.4 Penempatan bed — koreksi salah catat

| Dari keadaan | Tindakan | Ke keadaan | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| Penempatan berlaku | Koreksi | Baris lama: `SupersededByCorrectionId` terisi. Baris baru: `CorrectsPlacementId` terisi, `Version` lama + 1 | `InpatientBedOccupancy : Correct` | Invoice `RANAP` berstatus `OPEN` dari bacaan langsung; alasan wajib; versi cocok | 422 `INP-COR-001`/`002`; 409 `INP-COR-003` |
| Penempatan yang sudah dikoreksi (`SupersededByCorrectionId` terisi) | Koreksi lagi | — | — | — | 409; koreksi selalu dilakukan pada baris yang berlaku |
| Penempatan berlaku | Hapus | — | — | — | Tidak ada endpoint hapus; penghapusan sungguhan dilarang |

### 5.5 Invoice — tanda "perlu diperiksa" (Billing)

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `RequiresReview = false` | Billing menemukan biaya kamar manual dan tarif kamar otomatis sekaligus | `RequiresReview = true`, `ReviewReasonCode = MANUAL_AND_AUTOMATIC_ROOM_CHARGE` | Sistem Billing | — | — |
| `RequiresReview = true` | Kasir menyelesaikan pemeriksaan | `RequiresReview = false`, `ReviewResolvedAt` terisi | `BillingInvoice : Update` | Tidak ada lagi biaya kamar manual yang aktif bersamaan dengan tarif kamar otomatis | 422 `BIL-REV-001` |
| Invoice `OPEN`, `RequiresReview = true` | Finalisasi | — | — | — | 422 `BIL-FIN-020` |
| Invoice `OPEN` dengan baris `TARIFF_NOT_FOUND` | Finalisasi | — | — | — | 422 `BIL-FIN-021` |

### 5.6 Tanda terima event (Billing)

| Kejadian | Hasil | Akibat |
|---|---|---|
| `ADMISSION_CONFIRMED`, invoice belum ada | `INVOICE_OPENED` | Invoice `RANAP` `OPEN` terbuka |
| `ADMISSION_CONFIRMED`, invoice sudah ada | `INVOICE_ALREADY_OPEN` | Tidak ada invoice kedua |
| `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED` | `RECALCULATED` | Versi hitungan baru; invoice dibuka lebih dulu bila belum ada |
| Kunci idempotensi sudah pernah diterima | `DUPLICATE`, `Accepted = true` | Tidak ada efek kedua |
| Encounter tidak dikenal | `REJECTED_UNKNOWN_ENCOUNTER`, `Accepted = false` | Pesan Rawat Inap `Failed`, lalu `DeadLetter` |
