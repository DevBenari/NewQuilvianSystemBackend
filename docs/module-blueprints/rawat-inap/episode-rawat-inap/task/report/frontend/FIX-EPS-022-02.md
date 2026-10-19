# Laporan Perubahan Frontend — FIX-EPS-022-02

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Ringkasan bayi–ibu hanya pada konfirmasi |
| Plan | PLAN-REPAIR-EPS-022 |
| Target | `QuilvianSystemFrontendDev`, branch `HamzahV2` |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE |

## Hasil

`NewbornAdmissionHeader` dan render globalnya dihapus, sehingga kartu besar bayi–ibu tidak lagi memenuhi setiap langkah. `NewbornReviewStep` sekarang memiliki dua ringkasan ringkas: identitas bayi/ibu dan data admisi. Langkah dokumen akhir tidak menerima kembali header global.

## Keputusan komponen

| Elemen | Keputusan | Implementasi |
| --- | --- | --- |
| Ringkasan akhir | REUSE | `BaseDetailCard` |
| Layout langkah | REUSE | panel dan stepper existing |
| Base component baru | Tidak ada | — |

## Acceptance dan bukti

| Kriteria | Status |
| --- | --- |
| Tidak ada kartu `Bayi dan ibu` global | Terpenuhi |
| Identitas dan RM bayi/ibu tampil pada Tinjau dan Aktifkan | Terpenuhi |
| Ringkasan admisi tetap memuat DPJP, perawatan, penjamin, penagihan, bed, dan reservasi | Terpenuhi |

Validasi source diperkuat oleh unit test yang memastikan simbol `NewbornAdmissionHeader` tidak lagi dipakai dan kartu identitas hanya dirender pada review.

