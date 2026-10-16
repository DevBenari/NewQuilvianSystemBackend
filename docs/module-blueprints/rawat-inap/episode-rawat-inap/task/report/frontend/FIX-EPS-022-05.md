# Laporan Perubahan Frontend — FIX-EPS-022-05

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Reuse deposit admisi umum pada alur bayi |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE |

## Hasil

Implementasi khusus `NewbornDepositStep` dihapus. Alur bayi sekarang merender `InpatientAdmissionDepositStep` yang sama dengan admisi umum. Komponen domain tersebut hanya diperluas melalui props opsional `nextLabel`, `disabled`, dan `onCancelAdmission`; nilai default menjaga perilaku admisi umum tetap sama.

## Acceptance dan bukti

| Kriteria | Status |
| --- | --- |
| UI/policy/format rupiah berasal dari komponen umum | Terpenuhi |
| Tujuan bayi bertuliskan `Lanjut ke Penempatan` | Terpenuhi |
| Deposit resume terkunci dan menawarkan cancel server | Terpenuhi |
| Default admisi umum tetap `Lanjut ke Dokter` | Terpenuhi |

Unit test source memastikan `NewbornDepositStep` tidak lagi dipakai dan pemanggil bayi menggunakan `InpatientAdmissionDepositStep`.

