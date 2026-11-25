# Gerbang Kelengkapan Requirement — Finishing ACC-FIN-005 + ACC-FIN-008

| Field | Nilai |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Skill | `requirement-completeness-gate` |
| Tanggal | 24 November 2026 |
| Slice yang dinilai | Buku besar (ACC-FIN-005), laba rugi dan neraca (ACC-FIN-008), serta unduhan CSV empat laporan — persis batas `ACC-DEC-141` dan `153` |
| Di luar penilaian | Rasio keuangan, target pendapatan, laporan pajak, laporan pusat laba — dinyatakan di luar scope oleh `ACC-DEC-141` |
| Bukti | Decision log revisi 19 (`ACC-DEC-141`..`154`); [peta kemampuan finishing](../finishing/01-capability-map-finishing.md); MASTER PRD `ACC-FIN-PRD-001` bagian 6 (ACC-FIN-005, 008), 7.4, 9, 12; source `rizkiG` `1f353c45`, `RizkiV2` `22894248` |
| Hasil | **`READY_FOR_DOMAIN_DESIGN`** |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasannya di bagian 4 |

## 1. Sifat slice ini

Slice ini **hanya membaca**. Ia menghitung ulang angka dari baris jurnal yang sudah disahkan,
menyusunnya menjadi laporan, dan menyediakan berkas unduhan. Ia tidak membuat jurnal, tidak mengubah
status apa pun, tidak menyimpan hasil hitungan, dan tidak menambah tabel.

**Contoh:** Rina, petugas akuntansi, membuka Neraca November 2026. Sistem menjumlahkan seluruh baris
jurnal `Posted` sampai 30 November per akun, mengelompokkannya menjadi Aset, Liabilitas, dan Ekuitas,
lalu menambahkan baris laba yang belum ditutup. Bila Rina membuka laporan yang sama sejam kemudian dan
tidak ada jurnal baru yang disahkan, angkanya persis sama.

## 2. Penilaian 18 dimensi

| ID | Dimensi | Status | Bukti / catatan |
| --- | --- | --- | --- |
| 01 | Tujuan | `CONFIRMED` | Laporan keuangan resmi dari jurnal sah, dapat ditelusuri sampai jurnal (`ACC-DEC-141`; PRD ACC-FIN-008) |
| 02 | Aktor | `CONFIRMED` | Pemegang hak `GeneralLedger : Read` (`ACC-DEC-150`) |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Periode akuntansi dan bagan akun ada; laporan boleh dibuka untuk periode berstatus apa pun (`ACC-DEC-144` butir 5) |
| 04 | Alur utama | `CONFIRMED` | Pilih periode → laporan → telusuri ke buku besar → rincian jurnal (`ACC-DEC-144`, `148`) |
| 05 | Exception | `CONFIRMED` | Pembanding tidak ada → `—` (`144`); tahun lalu belum ditutup → baris + peringatan (`143`); unit biaya badan hukum lain ditolak (`147`); tanpa hak → `403` (`150`) |
| 05b | Laporan tidak seimbang | `PROPOSED` — `NON_BLOCKING_STANDARD` | `ACC-DEC-154` berstatus `draft`, mengikuti preseden `IsBalanced` neraca saldo. Tidak mengubah struktur apa pun |
| 06 | Data minimum | `CONFIRMED` | Seluruhnya sudah ada: baris jurnal, jenis akun, induk akun, unit biaya, jenis jurnal, rujukan kejadian dan jurnal berulang (peta kemampuan bagian 4) |
| 07 | Aturan bisnis | `CONFIRMED` | Hanya `Posted`; laba rugi tanpa `JT` (`142`); tanda per kelompok (`145`); akun nol disembunyikan (`146`); saringan unit biaya (`147`) |
| 07b | Akun anak berjenis lain dari induk | `CONFLICT` terbuka (`C-05`) — `NON_BLOCKING_STANDARD` | Laporan memakai aturan cadangan `ACC-DEC-146` butir 5, sehingga bagan akun apa pun tetap dapat disajikan |
| 08 | Status | Tidak berlaku | Slice tidak punya lifecycle; laporan tidak disimpan |
| 09 | Authorization | `CONFIRMED` | `GeneralLedger : Read` untuk empat laporan dan empat unduhan; ditegakkan backend (`150`). Catatan operasional: pemberian hak ke tujuh peran belum dilakukan di lingkungan mana pun (MODULE-STATUS, T-5) — bukan gap requirement |
| 10 | Dependency antarmodul | `CONFIRMED` | Hanya master pusat biaya dan badan hukum (HR), dibaca apa adanya. Kotak masuk kejadian dibaca sebagai rujukan asal |
| 11 | Integrasi | Tidak berlaku | Tidak ada sistem luar. Unduhan CSV dibaca Excel, bukan integrasi |
| 12 | Hasil akhir | `CONFIRMED` | Laporan dan berkas CSV yang angkanya sama dengan layar (`149`) |
| 13 | Pembatalan / koreksi | Tidak berlaku | Tidak ada yang dibuat. Koreksi angka tetap lewat jurnal pembalik, yang otomatis tercermin di laporan |
| 14 | Audit | `CONFIRMED` | Unduhan dicatat tanpa nominal; pembukaan layar tidak (`151`) |
| 14b | Bentuk berkas CSV untuk Excel berbahasa Indonesia | `PROPOSED` — `NON_BLOCKING_STANDARD` | `ACC-DEC-149` butir 3 menyerahkan pengodean, pemisah kolom, dan pemisah desimal ke desain, dibuktikan saat uji |
| 15 | Notifikasi | Tidak berlaku | Tidak ada kejadian yang perlu diberitahukan |
| 16 | Billing | Tidak berlaku | Tidak membuat atau mengubah konsekuensi keuangan; hanya menyajikan |
| 17 | Keselamatan klinis | Tidak berlaku | Tidak ada data pasien di laporan. Kotak masuk tidak menyimpan pengenal pasien (`ACC-DEC-056`), dan kolom Asal hanya menyebut modul serta nomor transaksi sumber |
| 18 | Pelaporan / traceability | `CONFIRMED` | Inti slice ini: laporan → buku besar → jurnal → asal (`148`) |
| — | Isi ClickUp | `MISSING` — `NON_BLOCKING_STANDARD` | Deskripsi task `z8m132yv7x` dan `z8m132yv84` belum dibaca (`U-01`). PRD dan keputusan cukup untuk desain; bila isi ClickUp berbeda, slice diamandemen |

## 3. Kesiapan

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| Buku besar: drill-down, saldo awal, unit biaya, asal, CSV | `READY_FOR_DOMAIN_DESIGN` | Nol keputusan pemblokir |
| Laba rugi | `READY_FOR_DOMAIN_DESIGN` | Nol keputusan pemblokir; `C-05` tertangani aturan cadangan |
| Neraca | `READY_FOR_DOMAIN_DESIGN` | Nol keputusan pemblokir; `154` hanya menyangkut tampilan data rusak |
| CSV neraca saldo | `READY_FOR_DOMAIN_DESIGN` | Nol keputusan pemblokir |

Nol keputusan `BLOCKING`. Tidak ada decision ID baru yang perlu dibuat oleh gerbang ini.

## 4. Mengapa arsitektur domain tidak dijalankan

`hospital-domain-architect` bersifat opsional dan dipakai bila kemampuan melintasi lebih dari satu
bounded context, menyentuh master data bersama untuk ditulis, atau berdampak pada billing dan
keselamatan klinis. Slice ini tidak memenuhi satu pun: seluruhnya di dalam bounded context buku besar
Accounting, master pusat biaya dan badan hukum hanya dibaca, dan tidak ada konsekuensi billing maupun
klinis. Arsitektur domain Accounting yang lebih luas sudah ditetapkan di `evidence/09`.

## 5. Handoff

Lanjut ke **`design-business-module`** (Amendment) untuk keempat slice di atas, dengan membawa:
`ACC-DEC-141`..`154`, `C-05` (cadangan `146` butir 5), `U-01`, butir `PROPOSED` 05b dan 14b.
