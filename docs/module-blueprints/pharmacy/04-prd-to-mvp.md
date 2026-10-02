# Farmasi — PRD ke MVP: Slice Financial Clearance

| Field | Nilai |
| --- | --- |
| Blueprint | `PHA-BP-001` |
| Slice | Financial Clearance Handoff |
| Status | **draft** |
| Tanggal | 21 September 2026 |
| Masukan | `PHA-DEC-063`–`070`, gerbang `PHA-RCG-002` (`READY_FOR_DOMAIN_DESIGN`) |
| Ketergantungan | Sisi penerbit `billing-kasir` (`BKC-DES-036`–`041`) **MUST** berdiri lebih dulu |

Berkas ini lahir bersama slice ini. Ia **tidak** mencakup seluruh modul Farmasi — slice Routing
Depo, persediaan, penyerahan bertahap, dan retur punya kesiapan requirement masing-masing yang
sebagian masih terbuka.

## 1. Ringkasan eksekutif

Sejak 24 Agustus 2026, **seluruh resep rawat jalan macet permanen**. Jalur lama yang
memungkinkan modul klinis menyatakan resep lunas sudah dihapus — dan itu benar, karena siapa pun
yang boleh mengubah resep dapat menyatakannya lunas tanpa melewati kasir. Tetapi penggantinya
belum pernah dibangun.

Akibatnya apoteker tidak dapat memulai telaah untuk resep mana pun. Slice ini membangun
penggantinya: Billing menyatakan, Farmasi mengikuti.

## 2. Masalah produk

| Yang terjadi | Siapa yang menanggung |
| --- | --- |
| Resep berhenti di keadaan menunggu pembayaran dan tidak pernah bergerak | Pasien yang sudah membayar, menunggu di loket obat |
| Tidak ada cara sah memindahkannya | Petugas Farmasi, yang tidak punya tombol apa pun untuk menolong |
| Satu-satunya jalur lama sengaja dihapus karena berbahaya | Rumah sakit, bila jalur itu dikembalikan |

## 3. Batas rilis

| Batas | Isi |
| --- | --- |
| Titik mulai | Billing menyatakan keadaan finansial sebuah resep berubah |
| Titik akhir | Resep berada pada keadaan yang benar — masuk antrean, tertahan, atau tetap menunggu — dan petugas mengetahui sebabnya |
| **Di luar batas** | Penentuan keadaan finansial itu sendiri (milik Billing); reservasi stok; penyerahan bertahap; retur; obat tidak diambil; jalur darurat klinis |

Pelaku sasaran: apoteker dan petugas Farmasi pada alur rawat jalan.

## 4. Kemampuan MUST HAVE

| Kemampuan | Asal keputusan |
| --- | --- |
| Menerima dan menyimpan pernyataan keadaan finansial per resep | `PHA-DEC-063`, `PHA-DEC-070` |
| Menolak pernyataan yang lebih tua dari yang tersimpan | `PHA-DEC-063` |
| Memindahkan resep dari menunggu pembayaran ke antrean apoteker | `PHA-DEC-064`, `PHA-DEC-065` |
| Menahan pekerjaan di tempat saat izin dicabut | `PHA-DEC-069` |
| Melanjutkan dari titik terakhir saat izin dipulihkan | `PHA-DEC-069` |
| Menolak melanjutkan saat keadaan tidak dapat dipastikan | `PHA-DEC-067` |
| Menampilkan alasan penahanan pada layar kerja | `PHA-DEC-069` |

## 5. Kemampuan yang ditunda

| Ditunda | Alasan bersebab | Pengganti selama MVP |
| --- | --- | --- |
| Jalur pengecualian darurat klinis | Kebijakannya belum disahkan (`PHA-OQ-027`). Membangunnya tanpa kebijakan berarti membuat pintu yang tidak ada aturan siapa boleh melewatinya | Tidak ada. Keadaan darurat tertahan seperti resep lain — batas yang disadari dan diterima |
| Pemetaan penuh lifecycle enam-keadaan ke sebelas-keadaan | `PHA-OQ-018` baru tertutup sebagian; yang dipastikan hanya bahwa gerbang pembayaran tetap ada | Keadaan yang terpasang sekarang dipakai apa adanya |
| Pemulihan resep yang sudah terlanjur macet | Menuntut pernyataan pertama terbit dari Billing untuk tagihan lama | Pemeriksaan ulang per resep lewat permukaan milik Billing |

## 6. Epic dan functional requirement

### `EPIC PHA-CLR-01` — Salinan keadaan finansial

| FR | Kemampuan | Disposisi |
| --- | --- | --- |
| `FR-PHA-CLR-01` | Menyimpan satu baris keadaan finansial per resep | `MISSING / NEW` |
| `FR-PHA-CLR-02` | Memperbarui salinan dari pernyataan Billing, menolak yang lebih tua | `MISSING / NEW` |
| `FR-PHA-CLR-03` | Menandai salinan bermasalah saat penerimaan gagal berulang | `MISSING / NEW` |
| `FR-PHA-CLR-04` | Memeriksa ulang keadaan terkini ke Billing tanpa menunggu pernyataan berikutnya | `MISSING / NEW` |

### `EPIC PHA-CLR-02` — Gerbang pekerjaan Farmasi

| FR | Kemampuan | Disposisi |
| --- | --- | --- |
| `FR-PHA-CLR-05` | Memindahkan resep ke antrean apoteker saat izin diberikan | `MISSING / NEW` |
| `FR-PHA-CLR-06` | Menolak telaah, penyiapan, dan pemeriksaan akhir saat ditahan | `EXTEND` — keempat service sudah ada |
| `FR-PHA-CLR-07` | Menolak penyerahan saat keadaan finansial belum beres | `EXISTING / REUSE` — gerbang penyerahan sudah ada, hanya belum pernah ada yang mengisinya |
| `FR-PHA-CLR-08` | Melanjutkan dari titik terakhir tanpa mengulang pekerjaan | `MISSING / NEW` |

### `EPIC PHA-CLR-03` — Keterbacaan bagi petugas

| FR | Kemampuan | Disposisi |
| --- | --- | --- |
| `FR-PHA-CLR-09` | Menampilkan keadaan finansial dan alasan penahanan pada layar kerja | `EXTEND` — response layar kerja yang sudah ada bertambah field |
| `FR-PHA-CLR-10` | Menonaktifkan tombol lanjut saat resep belum boleh dikerjakan | `EXTEND` |

## 7. Skenario UAT

### Jalur berhasil

| ID | Skenario | Hasil yang diharapkan |
| --- | --- | --- |
| `UAT-PHA-CLR-01` | Pasien melunasi tagihan kunjungan | Resep muncul di antrean apoteker dalam waktu wajar, tanpa petugas melakukan apa pun |
| `UAT-PHA-CLR-02` | Tagihan lunas lewat penjaminan | Resep masuk antrean; keadaan tercatat disetujui penjamin, bukan lunas tunai |
| `UAT-PHA-CLR-03` | Pasien melunasi kekurangan setelah resep sempat ditahan | Pekerjaan dilanjutkan dari titik terakhir; apoteker tidak meracik ulang |

### Jalur gagal

| ID | Skenario | Hasil yang diharapkan |
| --- | --- | --- |
| `UAT-PHA-CLR-04` | Tagihan berubah saat obat sedang diracik | Racikan tidak dibuang, resep berhenti di tempat, dan alasannya terbaca petugas tanpa mengklik apa pun |
| `UAT-PHA-CLR-05` | Sambungan ke Billing bermasalah | Layar tetap dapat dimuat; resep tidak dapat dilanjutkan; tidak ada tombol paksa bagi peran mana pun |
| `UAT-PHA-CLR-06` | Petugas mencoba menelaah resep yang belum dibayar | Ditolak dengan kalimat yang menyebut kasir, bukan istilah teknis |
| `UAT-PHA-CLR-07` | Tagihan dibalik setelah obat diserahkan | Tidak ada yang berubah di Farmasi; riwayat penyerahan utuh |

## 8. Definition of Done

| Butir | Dapat dijawab | Bukti |
| --- | --- | --- |
| Tabel salinan berdiri beserta index uniknya | Ya / Belum | Migration diterapkan pada basis data pengembang |
| Resep berpindah ke antrean saat izin diberikan | Ya / Belum | `PHA-AT-CLR-01` |
| Pernyataan lebih tua tidak menimpa yang lebih baru | Ya / Belum | `PHA-AT-CLR-02`, `PHA-AT-CLR-11` |
| Penahanan tidak menarik mundur dan tidak membuang racikan | Ya / Belum | `PHA-AT-CLR-04`, `PHA-AT-CLR-04-F` |
| Pemulihan tidak mengulang pekerjaan | Ya / Belum | `PHA-AT-CLR-06` |
| Tidak ada override bagi peran mana pun | Ya / Belum | `PHA-AT-CLR-07-F` |
| Keadaan tidak diketahui tidak menjadi izin | Ya / Belum | `PHA-AT-CLR-08` |
| Alasan penahanan terbaca tanpa mengklik | Ya / Belum | `PHA-AT-CLR-10` |
| Farmasi tidak menulis satu baris pun ke tabel Billing | Ya / Belum | Tinjauan source dan uji integrasi |

## 9. Urutan pengiriman

| Gelombang | Isi | Prasyarat |
| --- | --- | --- |
| `MVP-0` | `EPIC PHA-CLR-01` — salinan dan konsumsi pernyataan | **Sisi penerbit Billing sudah berjalan**; otorisasi migration terpisah |
| `MVP-1` | `EPIC PHA-CLR-02` — gerbang pada empat titik | `MVP-0` selesai |
| `MVP-2` | `EPIC PHA-CLR-03` — keterbacaan bagi petugas | `MVP-1` selesai |
| `POST-MVP` | Pemulihan resep yang terlanjur macet; jalur darurat klinis | `PHA-OQ-027` terjawab; otorisasi pemutakhiran data |

Gelombang `MVP-1` dapat dikerjakan tanpa `MVP-2`, tetapi **tidak sebaiknya dirilis tanpanya**:
gerbang yang menolak tanpa menjelaskan sebabnya akan membuat petugas menghubungi administrator
untuk masalah yang ada di kasir.

## 10. Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir? | Penjawab |
| --- | --- | --- | --- |
| `PHA-OQ-027` | Kebijakan pengecualian darurat klinis | **Tidak** — jalur normal lengkap tanpanya; hanya mempersempit cakupan | Clinical Governance + Billing/Finance |
| `PHA-OQ-018` | Pemetaan penuh lifecycle | **Tidak** — keadaan terpasang dipakai apa adanya | Product/Domain Owner |
| Pemulihan resep macet | Gelombang yang sama atau menyusul | **Tidak memblokir `MVP-0`** | Product/Domain Owner |

Ketiganya **bukan** `OPEN DECISION` pada tingkat epic. Seluruh epic dapat dirancang dan
dikerjakan penuh tanpa menunggu jawabannya, sehingga slice ini siap diteruskan ke
`plan-module-delivery` begitu desain disetujui **dan** sisi penerbit Billing berdiri.
