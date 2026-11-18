# Usulan desain: verifikasi administratif susulan untuk permintaan CITO

| Field | Nilai |
| --- | --- |
| Blueprint | `LGS-BP-001` r4 (diperbarui r5) |
| Terkait | `LGS-DEC-002` butir 5 (jalur CITO), `LGS-OQ-022` |
| Status | **Disetujui sebagai `LGS-DEC-005` (2026-11-17) dan diterapkan dengan pilihan penyimpanan A** (baris riwayat, tanpa migration). Proses tindak lanjut temuan belum memiliki penyimpanan (blocker persistence). Bagian di bawah dipertahankan sebagai catatan desain asal |
| Disusun | 2026-11-17 |

## 1. Masalah yang ingin dijawab

Permintaan CITO bisa langsung disetujui oleh petugas yang memegang izin otorisasi CITO, tanpa
melewati atasan unit peminta. Ini memang tujuan jalur darurat. Akibatnya, atasan unit bisa saja
tidak pernah tahu ada barang yang keluar atas nama unitnya.

Contoh: perawat IGD mengajukan 20 ampul obat sebagai CITO pukul 02.00. Apoteker jaga
mengotorisasinya, lalu gudang menyerahkan barangnya. Kepala Ruang IGD baru masuk pukul 08.00 dan
tidak pernah diminta memeriksa permintaan itu.

Usulan awal `LGS-OQ-004` menyebut "validasi menyusul". Keputusan final `LGS-DEC-002` tidak
menyebutnya. Karena itu verifikasi susulan **tidak diterapkan** sampai ada keputusan bisnis.

## 2. Usulan alur

```text
Submitted (CITO) ──authorize-emergency──▶ Approved ──fulfill──▶ Completed
                                              │
                                              └─ tugas verifikasi susulan untuk atasan unit
                                                 (tidak mengubah status, tidak menahan gudang)
                                                    ├─ Dikonfirmasi
                                                    └─ Dipersoalkan + alasan ─▶ tindak lanjut audit
```

Prinsip usulan:

1. **Tidak menahan barang.** Verifikasi berjalan sesudah otorisasi dan tidak menghalangi
   penyerahan. Kalau menahan penyerahan, jalur CITO kehilangan fungsinya.
2. **Tidak membalik transaksi.** Hasil "Dipersoalkan" tidak membatalkan permintaan, karena
   barangnya mungkin sudah dipakai. Hasilnya adalah catatan audit dan bahan tindak lanjut.
3. **Pemisahan tugas.** Verifikator adalah atasan unit peminta (aturan kewenangan yang sama
   dengan persetujuan biasa). Pemohon dan petugas yang mengotorisasi CITO tidak boleh menjadi
   verifikator untuk permintaan yang sama.
4. **Bukan status baru.** "Menunggu verifikasi susulan" dihitung dari data, sehingga nilai
   `StockRequestStatus` tidak bertambah dan aturan transisi yang sudah ada tidak berubah.

## 3. Pilihan penyimpanan

| Pilihan | Cara | Migration | Kelebihan | Kekurangan |
| --- | --- | --- | --- | --- |
| A | Baris riwayat baru dengan aksi `VerifyEmergencyStockRequest`; `FromStatus` = `ToStatus` = status saat itu; hasil dan alasan di `Reason` | Tidak perlu | Cepat; memakai idempotensi dan jejak riwayat yang sudah ada | Hasil (Dikonfirmasi/Dipersoalkan) disimpan sebagai teks; batas waktu tidak tersimpan, jadi dihitung dari waktu otorisasi |
| B | Tabel baru `PhmStockRequestEmergencyReview` (permintaan, verifikator, hasil, alasan, tenggat, waktu) | Perlu | Hasil terstruktur, tenggat tersimpan, mudah dilaporkan | Butuh migration, prefix tabel, dan persetujuan skema |

Rekomendasi teknis: mulai dengan **A** bila verifikasi disetujui. Pindah ke **B** bila rumah
sakit membutuhkan laporan tenggat yang terlewat atau rekap hasil per unit.

## 4. Usulan kontrak API (belum dibuat)

### `POST /api/v1/health-services/pharmacy-management/stock-requests/{id}/verify-emergency`

- Izin: `StockRequest:Approve`, ditambah kewenangan atasan unit peminta (pemeriksaan domain yang
  sama dengan `approve`)
- Body:

```json
{
  "outcome": "Confirmed",
  "reason": "Sesuai kondisi pasien IGD malam itu",
  "idempotencyKey": "phm-verify-..."
}
```

- `outcome`: `Confirmed` atau `Disputed`. `reason` wajib untuk `Disputed`
- `200`: detail permintaan, dengan penanda `emergencyReviewOutcome` dan `emergencyReviewedAt`
- `403`: bukan atasan unit peminta, atau petugas adalah pemohon/pengotorisasi CITO
- `409`: sudah diverifikasi, atau permintaan bukan CITO yang diotorisasi lewat jalur darurat
- `GET /stock-requests?pendingEmergencyReview=true`: daftar CITO yang belum diverifikasi

## 5. Keputusan yang dibutuhkan (`LGS-OQ-022`)

1. Apakah verifikasi susulan wajib?
2. Siapa verifikatornya: atasan unit peminta, atau pejabat lain?
3. Berapa batas waktunya (misalnya 1×24 jam), dan apa yang terjadi bila terlewat (pengingat,
   laporan ke manajemen)?
4. Apa tindak lanjut hasil "Dipersoalkan"?
5. Apakah verifikasi tetap diperlukan bila CITO diotorisasi oleh atasan unit peminta sendiri?

Sampai keputusan ini ada, perilaku yang berlaku tetap: otorisasi CITO adalah keputusan final.

## 6. Penerapan (r5)

| Butir usulan | Penerapan |
| --- | --- |
| Pilihan penyimpanan | A: baris riwayat dengan aksi `VerifyEmergencyStockRequestConfirmed` / `VerifyEmergencyStockRequestDeviation` |
| Endpoint | `POST /stock-requests/{id}/verify-emergency`, body `{ outcome: 1 | 2, note, expectedVersion, idempotencyKey }` (angka enum, bukan teks) |
| Daftar menunggu | `GET /stock-requests?emergencyReviewState=1` |
| Verifikator | Izin `StockRequest:Approve` eksplisit + atasan unit peminta; pemohon dan pengotorisasi CITO ditolak |
| Verifikasi ganda | `409 PHM019`; balapan dijaga token versi (`409 PHM012`) |
| Bukan jalur CITO | `409 PHM010` |
| Temuan tanpa catatan | `422 PHM008` |
| Stok dan status | Tidak berubah |
| Tenggat dan tindak lanjut | **Belum diterapkan.** Tenggat dan penelusuran tindak lanjut butuh tabel (pilihan B) |
