# Optik — Matriks Perubahan Status

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-STATE-v1` — status `approved` (sisi Optik) |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-03 |
| `input_revision` | `03-domain-architecture.md` r1 bagian G; `00-interview-decisions.md` r4 |

Penolakan memakai kode `409` bila status saat ini tidak mengizinkan, `403` bila pelaku tidak
berwenang, dan `400` bila isian kurang. Setiap transisi sah menulis satu baris
`OptOrderStatusHistory` (pesanan) atau jejak versi (pemeriksaan/resep).

## 1. Pemeriksaan Refraksi (`OptRefractionExam`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Mulai | `Draft` | RO/Sp.M (`OptRefractionExam : Create`) | Kunjungan Optik belum ditutup; izin praktik berlaku hari ini (WIB) | `409` "Kunjungan sudah ditutup"; `403` "SIP Anda tidak berlaku sejak …" |
| `Draft` | Isi hasil ukur | `Draft` | RO/Sp.M | — | — |
| `Draft` | Simpan final | `Final` | RO/Sp.M (`Finalize`) | Minimal satu visus per mata terisi; izin berlaku | `400` isian kurang; `403` izin |
| `Final` | Revisi | `Final` (revisi +1) | RO/Sp.M (`Revise`) | `RevisionReason` terisi; **tidak** menagih ulang | `400` alasan kosong |
| `Draft` | Batalkan | `Cancelled` | RO/Sp.M (`Cancel`) | Alasan | `400` |
| `Final` | Batalkan | — | — | **Tidak sah**: pemeriksaan final sudah ditagih; koreksi lewat revisi | `409` "Pemeriksaan final tidak dapat dibatalkan; gunakan revisi" |
| `Cancelled` | apa pun | — | — | **Tidak sah** | `409` |

## 2. Resep (`OptPrescription`) — sifat turunan

| Keadaan | Tindakan | Hasil | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat internal / salin luar | Versi 1 | RO/Sp.M (`Create`) | Izin berlaku; isian dalam rentang atau Sp.M dengan `OverrideRange` + alasan | `403` petugas penjualan; `400` rentang |
| Belum terkunci | Revisi | Versi +1 | RO/Sp.M (`Revise`) | `RevisionReason` | `400` |
| Terkunci | Revisi | — | — | **Tidak sah** | `409` "Resep terkunci karena lensa sudah dipesan" |
| Belum terkunci | Pesanan yang memakainya masuk `InFabrication` | Terkunci | Sistem | — | — |
| Kedaluwarsa (`ValidUntil` < hari ini WIB) | Dipakai pesanan baru | — | — | **Tidak sah** | `409` "Resep berusia lebih dari 12 bulan, lakukan pemeriksaan ulang" |

## 3. Pesanan (`OptOrder`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat | `Draft` | Petugas penjualan (`OptOrder : Create`) | Kunjungan terbuka; berukuran → versi resep berlaku; lensa kontak → resep lensa kontak berlaku; barang punya profil Optik aktif | `409`/`400` |
| `Draft` | Ubah item | `Draft` | Petugas penjualan (`Update`) | — | — |
| `Draft` (berpengerjaan) | Simpan & tagihkan | `AwaitingDownPayment` | Petugas penjualan (`Bill`) | Semua barang bertarif berlaku; fakta tagihan diterima Billing | `400` "Tarif barang … belum ada"; `503` Billing |
| `Draft` (penjualan langsung) | Simpan & tagihkan | `Draft` (tertagih, `BilledAt` terisi) | Petugas penjualan (`Bill`) | Sama | Sama |
| `Draft` (penjualan langsung, tertagih) | Serahkan | `HandedOver` | Petugas penjualan (`HandOver`) | Lunas; identitas dicocokkan; stok cukup → stok dipotong | `409` "Tagihan belum lunas"; `400` identitas; `409` stok |
| `AwaitingDownPayment` | Kirim ke pengerjaan | `InFabrication` | Petugas penjualan (`SendToWork`) | Terbayar ≥ `DownPaymentMinPercentSnapshot` × bagian pasien; tujuan + perkiraan jadi; tidak ada pesanan berpengerjaan lain yang belum lunas di kunjungan ini (aturan sementara); stok cukup. **Resep dikunci, stok frame & lensa stok dipotong** | `409` "Uang muka belum memenuhi minimum Rp …"; `409` "Lunasi pesanan … dulu atau daftarkan kunjungan baru"; `409` stok |
| `InFabrication` | Tandai siap | `ReadyForPickup` | Petugas penjualan (`MarkReady`) | — | — |
| `ReadyForPickup` | Serahkan | `HandedOver` | Petugas penjualan (`HandOver`) | Lunas; identitas dicocokkan; masa garansi dibekukan | `409` lunas; `400` identitas |
| `NotCollected` | Serahkan susulan | `HandedOver` | Petugas penjualan (`HandOver`) | Lunas; identitas dicocokkan (`OPT-DEC-049`) | `409` lunas |
| `Draft` / `AwaitingDownPayment` | Batal | `Cancelled` | Petugas penjualan (`Cancel`) | Alasan; seluruh baris tagihan dibatalkan | `400` |
| `InFabrication` / `ReadyForPickup` | Ajukan batal | (tetap) + pengajuan `Requested` | Petugas penjualan (`RequestCancel`) | Alasan; tanda `IsLensCut` per lensa stok; belum ada pengajuan `Requested` lain | `409` "Masih ada pengajuan batal yang menunggu" |
| `InFabrication` / `ReadyForPickup` | Setujui batal | `Cancelled` | Kepala Optik (`DecideCancel`) | Pengajuan `Requested`. Frame + lensa stok belum dipotong: baris dibatalkan, stok kembali. Lensa custom + lensa sudah dipotong: tetap ditagih | `409` |
| `InFabrication` / `ReadyForPickup` | Tolak batal | (tetap) | Kepala Optik (`DecideCancel`) | `DecisionNote` | `400` |
| `InFabrication` / `ReadyForPickup` | Batal langsung (tanpa pengajuan) | — | — | **Tidak sah** | `409` "Pembatalan sesudah pengerjaan perlu persetujuan Kepala Optik" |
| `ReadyForPickup` | Tutup tidak diambil | `NotCollected` | Kepala Optik (`CloseNotCollected`) | `ReadyAt` + `NotCollectedCloseDays` ≤ hari ini; alasan; semua item tetap ditagih | `409` "Belum melewati batas 90 hari" |
| `HandedOver` / `Cancelled` | apa pun selain keluhan garansi | — | — | **Tidak sah** — status terminal | `409` |
| `InFabrication` sesudahnya | Ubah item / ganti versi resep | — | — | **Tidak sah** | `409` |
| Pesanan remake | Simpan & tagihkan | — | — | **Tidak sah**: remake tidak menerbitkan tagihan; langsung `AwaitingDownPayment` → dikirim tanpa syarat uang muka | `409` |

**Pesanan remake** dibuat sistem saat keluhan disetujui. Statusnya `AwaitingDownPayment` dengan
minimum uang muka Rp 0, sehingga langsung bisa dikirim ke pengerjaan. Ia mengikuti alur yang sama
sampai `HandedOver`, tanpa fakta tagihan.

## 4. Keluhan Garansi (`OptWarrantyClaim`)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Catat | `Submitted` | Petugas penjualan / RO / Sp.M (`OptWarrantyClaim : Create`) | Pesanan asal `HandedOver`; kunjungan keluhan baru milik pasien yang sama; `IsWithinWarranty` dihitung | `409` "Pesanan belum diserahkan" |
| `Submitted` | Periksa ulang | `Examined` | RO/Sp.M (`Examine`) | Temuan terisi | `400` |
| `Examined` | Setujui remake | `Approved` | Kepala Optik (`Decide`) | `IsWithinWarranty = true`; belum ada remake; pesanan remake dibuat | `409` "Keluhan di luar masa garansi" |
| `Examined` | Tolak | `Rejected` | Kepala Optik (`Decide`) | `DecisionReason` | `400` |
| `Submitted` | Setujui / tolak | — | — | **Tidak sah**: wajib diperiksa ulang dulu | `409` |
| `Approved` / `Rejected` | apa pun | — | — | **Tidak sah** — terminal | `409` |

## 5. Jejak keputusan

`OPT-DEC-007`, `010`, `011`, `015`, `016`, `018`, `019`, `020`, `023`, `028`, `035`, `038`, `041`,
`042`, `043`, `044`, `046`, `047`, `048`, `049`.
