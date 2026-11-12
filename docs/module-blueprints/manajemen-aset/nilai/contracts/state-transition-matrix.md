# Nilai Aset — Matriks Perubahan Status

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

## 1. Buku nilai (`AssetBookStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat |
|---|---|---|---|---|
| — | Aset disetujui | `Aktif` | Sistem (`register`) | Parameter nilai berlaku ada |
| — | Batch impor diterapkan | `Aktif` (saldo awal) | Sistem (`register`) | `AST-VAL-NLI-040` |
| `Aktif` | Penghapusan final | `Dihentikan` | Sistem (`penghapusan`) | Penyusutan bulan sebelumnya sudah diproses (`AST-DES-013`) |
| `Dihentikan` | Pemulihan disetujui | `Aktif` | Sistem (`penghapusan`) | Bulan yang terlewat dikejar (`AST-DES-010`) |

**Tidak sah:** mengubah buku `Dihentikan` lewat proses penyusutan atau perubahan nilai; menghapus entri.

## 2. Proses penyusutan

Proses hanya punya satu keadaan tersimpan: **ada** (diproses). Yang diatur adalah kapan boleh dibuat.

| Kondisi | Boleh diproses? | Bila dilanggar |
|---|---|---|
| Periode ≤ bulan berjalan | Ya | `AST-VAL-NLI-010` |
| Belum ada proses untuk periode itu | Ya | `AST-VAL-NLI-011` |
| Periode sebelumnya sudah diproses, atau periode ini adalah periode pertama (bulan sesudah tanggal awal / bulan mulai penyusutan aset paling awal) | Ya | `AST-VAL-NLI-012` |
| Pratinjau tanpa galat | Ya | `AST-VAL-NLI-013` |

**Tidak sah:** membatalkan atau mengulang proses; memproses dua periode sekaligus.

## 3. Perubahan nilai (`ValueChangeStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Akibat |
|---|---|---|---|---|---|
| — | Ajukan | `Diajukan` | Petugas Aset | Buku `Aktif`, dikapitalisasi; bukti terlampir | — |
| `Diajukan` | Setujui | `Disetujui` | Kepala Akuntansi, bukan pengaju | Aturan per jenis (bagian 4) | Entri dibuat; buku diperbarui; kejadian bila jenis penurunan nilai |
| `Diajukan` | Tolak | `Ditolak` | Kepala Akuntansi | Alasan | — |
| `Diajukan` | Batalkan | `Dibatalkan` | Pengaju | Alasan | — |

**Tidak sah:** mengubah pengajuan `Disetujui`; menyetujui sendiri; dua pengajuan `Diajukan` untuk satu aset.

## 4. Akibat persetujuan per jenis

| Jenis | Perubahan buku | Kejadian Akuntansi | Contoh |
|---|---|---|---|
| `PerbaikanBesar` | `CapitalizedCost` + nilai; opsional tambahan umur | **Tidak** (`AST-DEC-041`) | Genset biaya tercatat Rp 400 jt + Rp 40 jt = Rp 440 jt; umur +24 bulan |
| `KoreksiHargaPerolehan` | `CapitalizedCost` diganti; selisih akumulasi dihitung sebagai penyesuaian pada proses berikutnya | Tidak untuk harga; penyesuaian penyusutan ikut kejadian `AST-PENYUSUTAN` | Rp 58 jt → Rp 85 jt sesudah 6 bulan: penyesuaian Rp 2.700.000 |
| `PenurunanNilai` | `AccumulatedImpairment` + nilai (≤ nilai buku − residu) | **Ya**, `AST-PENURUNAN-NILAI` | Nilai buku Rp 30 jt, penurunan Rp 12 jt → nilai buku Rp 18 jt |
| `PerubahanEstimasi` | Umur dan/atau residu aset diganti; `HasEstimateOverride = true` | Tidak | Lihat `AST-RCG-G17` |

## 5. Kotak keluar (`AccountingOutboxStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat |
|---|---|---|---|---|
| — | Ditulis | `Pending` atau `Held` | Service pemicu | `Held` bila kodenya belum dinyalakan (`AST-DES-034`) |
| `Held` | Kode dinyalakan | `Pending` | Konfigurasi + pekerja | — |
| `Pending` | Terkirim, balasan 200/201 | `Acknowledged` | Pekerja | — |
| `Pending` | Gagal | `Pending` (`AttemptCount`+1) | Pekerja | Di bawah batas percobaan |
| `Pending` | Gagal ke-N | `Failed` | Pekerja | `AttemptCount` = `MaxAttempts` |
| `Failed` | Coba ulang | `Pending` | Kepala Akuntansi | `AttemptCount` direset |

Baris `Held` tidak menambah `AttemptCount` dan tidak pernah menjadi `Failed` (pola Finance Lapis 2).
