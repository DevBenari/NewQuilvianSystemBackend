# Permintaan Persetujuan — Data ringkasan Beranda Lab mengikuti susunan v1

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-021` |
| `tanggal` | 2026-10-08 |
| `pengaju` | `design-business-module`, menurunkan Amendment Pass putaran 25 (BR-140) |
| `rujukan` | `LAB-DEC-204`..`LAB-DEC-211`, `LAB-FE-035`; decision log revision 92; `02-backend-architecture.md` bagian 28; `03-frontend-architecture.md` amandemen 2026-10-08; `04-prd-to-mvp.md` bagian 29; `AC-294`..`AC-301` |
| `status` | ✅ **`disetujui`** 2026-10-08 — Yoga Aji Pratama, pemilik modul: *"Setuju 11 butir"*; **kesebelas butir bagian 3 sesuai usulan** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen tiga kontrak. Keputusan bisnisnya **sudah** diambil 2026-10-07 (putaran 25); yang diminta adalah persetujuan **bentuk kontrak dan rincian hitungannya**. **Kode belum diubah** |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Beranda Lab akan menampilkan kartu hari ini, antrean validasi, grafik setahun, tren bulanan, dan 10 pesanan terbaru —
seperti Beranda v1, dengan tampilan V2. Datanya dari **tiga endpoint baca baru**; endpoint rekap lama tidak diubah.
Tidak ada tabel, kolom, index, migration, maupun izin baru. Ada sebelas rincian hitungan yang tidak dijawab putaran 25;
masing-masing diusulkan di bawah beserta akibatnya bila ditolak. Saat menelusuri kode ditemukan satu cacat yang
**tidak** diperbaiki di sini (bagian 5).

## 2. Usulan amandemen kontrak

| Kontrak | Revisi usulan | Bagian | Isi |
|---|---|---|---|
| `LAB-API-v1` | `r44` | [39](../contracts/api-contract.md) | `GET /lab-orders/dashboard/today`, `/dashboard/yearly`, `/dashboard/recent-orders` |
| `LAB-VAL-v1` | `r20` | [22](../contracts/validation-matrix.md) | `VAL-154` — tahun 2000 sampai tahun berjalan |
| `LAB-PERM-v1` | revision 16 | [18](../contracts/permission-audit-matrix.md) | Ketiga endpoint dijaga `LabOrder : Read`; nol aksi baru |

`LAB-STATE-v1` dan `LAB-INT-v1` **tidak berubah**: Beranda tidak memindahkan status dan tidak memanggil modul lain.

## 3. Sebelas butir yang diminta persetujuannya

| No | Butir | Usulan | Bila tidak disetujui |
|---|---|---|---|
| 1 | Bentuk endpoint | **Tiga** endpoint baca baru; `GET /lab-orders/summary` dan Rekap Status Pesanan **tidak berubah** | Satu endpoint gabungan: galat satu bagian mengosongkan semua (`AC-300` gugur); atau `summary` diubah dan arti rekap lama ikut berubah |
| 2 | Waktu acuan | *Waktu diminta* = `RequestedAt`; bila kosong (pesanan lama) memakai waktu baris dibuat | Pesanan lama tanpa `RequestedAt` hilang dari semua hitungan |
| 3 | Arti CITO | Pesanan CITO bila **permintaan** pemeriksaannya (saat memesan) **atau** pemeriksaannya (sesudah *Tandai Cito*) bertanda cito. Kartu CITO **ikut menghitung** pesanan yang sudah *Dibatalkan*, sejalan dengan *Pesanan hari ini* | Hanya membaca pemeriksaan seperti Daftar Pasien Lab: kartu CITO pagi hari hampir selalu 0 karena sampel belum datang |
| 4 | Arti *Hasil menunggu validasi* | **Sama persis** dengan isi Antrean Validasi tahap *Menunggu Validasi*, tanpa penyaring dan **tanpa batas tanggal**. Hanya Patologi Klinik dan Mikrobiologi — PA belum punya tahap validasi (`S4e`). Angkanya tampil juga bagi pemegang `LabOrder : Read` yang tidak bisa membuka Antrean Validasi — **angka saja** | Dibatasi hari ini: hasil final kemarin yang belum divalidasi tidak terlihat, padahal itu yang paling perlu dikejar |
| 5 | *Total pesanan tercatat* | Seluruh pesanan **sepanjang waktu**, termasuk *Dibatalkan* | Dibatasi tahun terpilih (angkanya ikut berubah saat memilih tahun) |
| 6 | Persentase | Dibulatkan ke bilangan bulat (setengah ke atas); 0% bila belum ada pesanan. *Tingkat penyelesaian* = angka yang sama dengan persentase kartu *Selesai* | Satu desimal; atau penyebut tanpa pesanan batal (3 dari 11 = 27%, bukan 25% seperti contoh `AC-295`) |
| 7 | Arti *pemeriksaan* pada grafik | Pemeriksaan yang tidak gugur/batal **ditambah** permintaan yang belum masuk wadah, pada pesanan yang **tidak** dibatalkan, di tahun terpilih. Disiplin dari pesanan, jatuh ke katalog. Yang tak tergolong dihitung terpisah dan baru tampil bila > 0 | Hanya pemeriksaan bertabung: pesanan yang sampelnya belum datang tidak terhitung |
| 8 | Batas tahun | 2000 sampai tahun berjalan WIB; di luar itu `422` (`VAL-154`). Layar menawarkan tahun berjalan dan empat tahun sebelumnya | Tanpa batas: tahun depan selalu nol |
| 9 | Pesanan terbaru | Seluruh status, **termasuk** *Dibatalkan*; kolom Pemeriksaan berisi nama-nama pemeriksaan yang diminta | Tanpa pesanan batal: tabel tidak lagi menunjukkan "10 terakhir" yang sebenarnya |
| 10 | Index | **Nol** index baru sekarang; ditambah sebagai task tersendiri bila kueri tahunan terukur > 1 detik | Index `RequestedAt` ditambah sekarang — satu migration, padahal rumus waktu acuan tidak dapat memakainya langsung |
| 11 | Kartu rekap lama | Lima kartu rentang (Total, tiga disiplin, Tanpa Disiplin) **dipindah ke dalam bagian Rekap Status Pesanan** di bawah, bukan dibuang | Tetap di atas: dua deret kartu disiplin berbeda arti tampil berdampingan |

## 4. Contoh hasil bila seluruh butir disetujui

Rabu 2026-10-08 pukul 09.15 WIB: 12 pesanan sejak tengah malam, 3 *Selesai*, 1 *Dibatalkan*, 2 CITO (satu belum ada
sampel). Antrean Validasi berisi 7 hasil. Kartu berbunyi *Pesanan hari ini 12 · Menunggu 8 · Selesai 3 (25%) · CITO 2*
dan *Total tercatat 4.812 · Menunggu validasi 7 · Jenis laboratorium 3 · Tingkat penyelesaian 25%*.

## 5. Temuan di luar permintaan ini — CITO dari pemesanan hilang (usulan `LAB-CONFLICT-019`)

Tanda CITO yang dipilih dokter atau kiosk saat memesan **tidak** disalin ke pemeriksaan ketika wadah direncanakan
(`LabSpecimenService.cs:249`). Akibatnya pesanan itu tampil **tanpa** lencana CITO di Daftar Pasien Lab, tercetak
*Routine* pada label, tidak didahulukan di Daftar Kerja, dan tidak dipantau keterlambatannya. Rincian di
`02-backend-architecture.md` 28.8.

Ini menyangkut keselamatan klinis dan mengubah perilaku lima layar, sehingga **tidak** dimasukkan ke permintaan ini.
Beranda tidak tertahan karenanya (butir 3 membaca dua sumber). Usulan langkahnya: `/grill-me` Amendment Pass tersendiri.

## 6. Cara menjawab

Cukup *"setuju kesebelas butir"*, atau sebutkan nomor butir yang ingin diubah.
