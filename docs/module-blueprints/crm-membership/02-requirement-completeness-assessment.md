# CRM Membership — Penilaian Kelengkapan Requirement

| Field | Nilai |
|---|---|
| Blueprint ID | `crm-membership` |
| Revision | `1` |
| Status | `draft` |
| Tanggal | 2026-10-13 |
| Masukan | `00-interview-decisions.md` (`CRM-DEC-001` s.d. `CRM-DEC-023`), `01-existing-capability-map.md` revision 1, `approval-requests/bkc-amendment-tagihan-iuran-membership.md` (`draft`), PDF "Pendaftaran membership pasien prioritas" dan PDF "Pendaftaran pasien prioritas IGD" (12 Okt 2026) |
| Backend SHA | `36422a4d` |
| Frontend SHA | `9aba9929d` |
| Baseline rujukan | Tidak dipakai. `indonesia-hospital-domain-reference` tidak dipanggil karena keputusan owner sudah menjawab pertanyaan inti. Ketiadaan baseline bukan bukti bahwa requirement tidak ada |
| Kesimpulan | **`PARTIALLY_READY`**. Revisi 1a (amendment pass 3): S1a, S1b, S3, S4 (aturan manfaat), dan S5 siap. Hanya S2 yang menunggu `DEC-CRM-001` |

## 1. Scope Penilaian

Modul yang dinilai: `crm-membership` (Health Services / Patient Management, `CRM-DEC-017`).
Batas modul mengikuti `CRM-DEC-001`. Alur reservasi RJ, antrean kelipatan 5, IGD, kamar
rawat inap, dan wallet berada di luar penilaian ini.

| Slice | Isi | Kesiapan |
|---|---|---|
| `S1a` | Siklus hidup membership: daftar, Menunggu Pembayaran, Aktif, Kedaluwarsa, upgrade, batal (atas permintaan pasien dan salah input), Perlu Verifikasi untuk data lama, nomor member otomatis, koreksi terbatas | `READY_FOR_DOMAIN_DESIGN`, dengan dependency ke `S2` untuk pemicu aktivasi |
| `S1b` | Perpanjangan **sebelum** masa berlaku habis | `BUSINESS_DECISION_REQUIRED` (`DEC-CRM-004`) |
| `S2` | Tagihan iuran di kasir, kabar lunas, pembatalan tagihan, refund salah input | `BUSINESS_DECISION_REQUIRED` (`DEC-CRM-001`) |
| `S3` | Pembacaan status member oleh modul lain dan pencarian pasien (No. RM/NIK/No. Telp) | `READY_FOR_DOMAIN_DESIGN` |
| `S4` | Penerapan diskon per tingkat pada kunjungan pribadi | `BUSINESS_DECISION_REQUIRED` (`DEC-CRM-002`, `DEC-CRM-003`) |
| `S5` | Hak akses membership (CRM, admisi, kiosk) | `READY_FOR_DOMAIN_DESIGN` |

## 2. Bukti yang Dipakai

| Sumber | Wewenang | Menjawab |
|---|---|---|
| Keputusan owner `CRM-DEC-001` s.d. `CRM-DEC-023`, Yoga Aji Pratama, 2026-10-13 | 1, requirement eksplisit terkini | Apa yang harus dibangun |
| Dua PDF sumber | 1, requirement eksplisit (lebih tua dan diperhalus oleh keputusan) | Latar kebutuhan |
| `01-existing-capability-map.md` @ `36422a4d`/`9aba9929d` | 6, implementasi V2 terverifikasi | Apa yang ada sekarang |
| `rawat-jalan/00-interview-decisions.md:201,216` | Keputusan approved modul lain | Batas diskon sebagai potongan harga |
| Permintaan amendment billing (`draft`) | Belum berwenang, menunggu owner billing | Sisi pembayaran iuran |

Pertentangan antara PDF dan keputusan sudah diselesaikan owner: diskon bukan cara bayar
(`CRM-DEC-008`), dan daftar tingkat mengikuti PDF (`CRM-DEC-006`). Pertentangan antara
keputusan dan source (`CRM-CONF-003` s.d. `CRM-CONF-008`) adalah gap implementasi, bukan
pertentangan requirement. Requirement target mengikuti keputusan owner.

## 3. Temuan per Dimensi

Keterangan: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`,
**—** = tidak berlaku secara material (alasan di catatan).

| No | Dimensi | S1a | S1b | S2 | S3 | S4 | S5 | Catatan dan bukti |
|---|---|---|---|---|---|---|---|---|
| 01 | Tujuan | C | C | C | C | C | C | `CRM-DEC-001`, PDF poin 1 dan 2 |
| 02 | Aktor | C | C | P | C | C | C | Aktor S2 di sisi billing (siapa penyetuju refund) belum diputuskan, lihat `DEC-CRM-001` |
| 03 | Pemicu / Prasyarat | C | C | C | C | C | C | No. RM wajib (`CRM-DEC-003`), iuran lunas (`CRM-DEC-004`) |
| 04 | Alur utama | C | M | P | C | P | C | S1b: tanggal mulai periode baru belum ditetapkan. S2: bentuk tagihan menunggu billing. S4: lihat `DEC-CRM-002` |
| 05 | Alur alternatif / exception | C | M | P | C | M | C | S4: penjamin berubah setelah diskon diterapkan belum diputuskan |
| 06 | Data minimum | P | C | M | C | M | C | S1a: nominal iuran per tingkat belum diisi (`CRM-OQ-001`). S2: perlakuan akuntansi iuran. S4: pemetaan kelompok diskon ke kategori tagihan |
| 07 | Aturan bisnis / validation | C | M | C | C | C | C | `CRM-DEC-004`, `005`, `010`, `012` s.d. `016`, `022`, `023` |
| 08 | Status / perubahan status | C | M | C | — | — | — | Tabel status di decision log. S3/S4/S5 tidak punya status sendiri |
| 09 | Peran / authorization | C | C | M | C | C | C | `CRM-DEC-011`, `018`. S2: penyetuju refund |
| 10 | Dependency antarmodul | C | C | C | C | C | C | Tabel amendment di decision log |
| 11 | Integrasi internal / eksternal | C | — | P | C | P | — | Internal saja. Tidak ada integrasi eksternal di MVP (WA ditunda, `CRM-OQ-003`) |
| 12 | Hasil akhir | C | C | C | C | C | C | Status member terbaca benar di semua titik layanan |
| 13 | Pembatalan / koreksi | C | M | P | — | M | C | `CRM-DEC-013`, `022`. S4: membatalkan diskon saat penjamin berubah |
| 14 | Audit / histori | C | C | C | — | C | C | Alasan wajib dan riwayat tidak ditimpa (`CRM-DEC-013`, `022`) |
| 15 | Notifikasi | — | — | — | — | — | — | Pengingat WA ditunda (`CRM-OQ-003`). Tidak ada notifikasi yang wajib di MVP |
| 16 | Dampak billing / charge | C | M | M | — | M | — | Iuran dan diskon berdampak uang. S1b: nilai iuran untuk sisa masa yang bertumpuk |
| 17 | Dampak keselamatan klinis | C | — | — | C | — | — | `CRM-DEC-007`: membership tidak pernah mengubah urutan medis. Berlaku sebagai batasan bagi konsumen status (S3) |
| 18 | Pelaporan / traceability | C | C | M | C | C | C | Daftar Membership dan ringkasan sudah ada (`CRM-CAP-007`). Laporan pendapatan iuran milik billing |

## 4. Butir Requirement Material

### CONFIRMED

Seluruh `CRM-DEC-001` s.d. `CRM-DEC-023` berstatus `CONFIRMED` dengan bukti persetujuan owner
pada 2026-10-13. Yang paling menentukan desain:

| Butir | Bukti |
|---|---|
| Satu membership aktif per pasien, menempel pada No. RM | `CRM-DEC-003`, `CRM-DEC-012` |
| Aktif setelah iuran lunas, batal otomatis 7 hari, kedaluwarsa otomatis tanpa tenggang | `CRM-DEC-004`, `CRM-DEC-005`, `CRM-DEC-014` |
| Upgrade: periode baru, iuran penuh, lama `Upgraded` saat iuran baru lunas | `CRM-DEC-012` |
| Data tetap di `MstPatientMembership` (PatientManagement) | `CRM-DEC-017` |
| Kiosk tidak boleh membuat, CRM via izin `PatientMembership : Create` | `CRM-DEC-011`, `CRM-DEC-018` |
| Tidak boleh mengubah pasien/tingkat/tanggal setelah Aktif | `CRM-DEC-022` |
| Diskon = potongan harga, hanya kunjungan pribadi sepenuhnya | `CRM-DEC-008`, `CRM-DEC-016` |
| Membership tidak mengubah triage | `CRM-DEC-007` |

### PROPOSED, MISSING, dan CONFLICT

| ID | Butir | Status | Dampak | Slice | Alasan dampak |
|---|---|---|---|---|---|
| `CRM-GAP-001` | Tagihan iuran non-kunjungan, kabar lunas, dan pembatalan tagihan di billing | `PROPOSED` (kebutuhan CRM `CONFIRMED`, sisi billing belum disetujui) | `BLOCKING` | S2 | Mengubah model data billing dan konsekuensi uang. Lihat `DEC-CRM-001` |
| `CRM-GAP-002` | Perlakuan akuntansi iuran (pendapatan di muka yang diakui selama 12 bulan, atau pendapatan langsung) | `MISSING` | `BLOCKING` | S2 | Konsekuensi finansial dan jurnal. Bagian `DEC-CRM-001` |
| `CRM-GAP-003` | Penyetuju refund iuran salah input | `MISSING` | `BLOCKING` | S2 (refund saja) | Wewenang persetujuan uang keluar. Bagian `DEC-CRM-001` |
| `CRM-GAP-004` | Kunjungan pribadi yang sudah didiskon lalu ditambah asuransi/penjamin, atau sebaliknya | `MISSING` | `BLOCKING` | S4 | Mengubah tagihan dan lifecycle diskon. Lihat `DEC-CRM-002` |
| `CRM-GAP-005` | Pemetaan enam kelompok diskon di master tier (pendaftaran, konsultasi, tindakan, lab, radiologi, farmasi) ke kategori item tagihan billing | `MISSING` | `BLOCKING` | S4 | Menentukan nominal diskon setiap tagihan. Lihat `DEC-CRM-003` |
| `CRM-GAP-006` | Perpanjangan sebelum masa berlaku habis: periode baru mulai saat lunas (sisa hari hangus) atau mulai sehari setelah periode lama berakhir (perlu status "terjadwal") | `MISSING` | `BLOCKING` | S1b | Mengubah state machine. Lihat `DEC-CRM-004` |
| `CRM-GAP-007` | Nominal iuran dan persen diskon per tingkat | `MISSING` | `CONFIGURABLE_DEFAULT` | S1a, S4 | Nilai master yang wajar berbeda antar rumah sakit dan dapat diubah tanpa rilis (`CRM-OQ-001`) |
| `CRM-GAP-008` | Batas hari berakhir mengikuti tanggal kalender WIB. Contoh: berakhir 12 Okt 2027, sehingga pukul 23.59 WIB masih Aktif dan 13 Okt 00.00 WIB sudah Kedaluwarsa | `PROPOSED` | `NON_BLOCKING_STANDARD` | S1a | Standar tanggal kalender lokal. Mencegah jebakan selisih 7 jam UTC |
| `CRM-GAP-009` | Dua petugas CRM mendaftarkan pasien yang sama hampir bersamaan: pendaftaran kedua ditolak karena sudah ada yang Aktif atau Menunggu Pembayaran | `PROPOSED` | `NON_BLOCKING_STANDARD` | S1a | Turunan langsung `CRM-DEC-012` |
| `CRM-GAP-010` | Pasien meninggal atau digabung (merge) saat masih member: CRM membatalkan manual dengan alasan. Sistem tidak memindahkan membership otomatis | `PROPOSED` | `NON_BLOCKING_STANDARD` | S1a | Penggabungan pasien di V2 hanya penandaan (`RM-CAP-007`). Kasus jarang dan bisa ditangani lewat pembatalan |
| `CRM-GAP-011` | Departemen-posisi mana yang mendapat izin `PatientMembership : Create/Update` | `MISSING` | `CONFIGURABLE_DEFAULT` | S5 | Diatur lewat Role Access (`SysAccessPolicies`) per rumah sakit |
| `CRM-GAP-012` | Format nomor member | `MISSING` | `CONFIGURABLE_DEFAULT` | S1a | `CRM-DEC-023` mengunci "otomatis dan unik". Formatnya boleh dikonfigurasi |
| `CRM-GAP-013` | Jumlah dan kondisi membership lama di database | `MISSING` | `NON_BLOCKING_STANDARD` | S1a | Aturannya sudah ada (`CRM-DEC-021`). Jumlah hanya memengaruhi beban verifikasi, bukan desain (`CRM-CAP-015`) |

Tidak ada `CONFLICT` requirement yang tersisa. Urutan diskon di billing (diterapkan pada
invoice) dibanding `CRM-DEC-008` (sebelum pembagian penjamin) tidak lagi material. Sebab
`CRM-DEC-016` membatasi diskon hanya untuk kunjungan pribadi sepenuhnya, sehingga porsi pasien
sama dengan seluruh tagihan.

## 5. Decision Log Pemblokir

### `DEC-CRM-001` — Tagihan iuran di billing-kasir

| Field | Isi |
|---|---|
| Pertanyaan | Apakah billing-kasir menyediakan tagihan non-kunjungan untuk iuran membership? Bila ya, bagaimana bentuk penyimpanan, perlakuan akuntansi, dan siapa penyetuju refund salah input? |
| Kemampuan terdampak | S2 (aktivasi setelah lunas, pembatalan tagihan, refund) |
| Bukti saat ini | Kebutuhan CRM `CONFIRMED` (`CRM-DEC-019`). `BilInvoice.EncounterId` wajib (`CRM-CAP-008`). Permintaan amendment masih `draft` |
| Usulan baseline | Tagihan iuran yang berdiri sendiri. Iuran diakui sebagai pendapatan di muka yang diamortisasi selama masa berlaku. Refund disetujui kepala kasir atau supervisor billing |
| Dampak | Model data billing, jurnal akuntansi, wewenang uang keluar, pemicu aktivasi membership |
| Pemilik | Owner `billing-kasir` (nama belum tercatat) dan Finance/Accounting untuk perlakuan akuntansi |
| Status | `OPEN` (`CRM-OQ-005`) |
| Dampak domain | S2 berhenti. S1a boleh dirancang dengan pemicu aktivasi berupa kontrak "iuran lunas" yang sumbernya menunggu keputusan ini |

### `DEC-CRM-002` — Penjamin berubah setelah diskon

| Field | Isi |
|---|---|
| Pertanyaan | Kunjungan pribadi member Gold sudah mendapat diskon 10%, lalu di tengah kunjungan ditambah asuransi. Apakah diskon dicabut otomatis, dicabut manual oleh kasir, atau dipertahankan? Sebaliknya, bila asuransi dilepas dan kunjungan menjadi pribadi, apakah diskon diterapkan? |
| Kemampuan terdampak | S4 |
| Bukti saat ini | `CRM-DEC-016` hanya mengatur keadaan akhir (pribadi = diskon, berpenjamin = tanpa diskon), belum mengatur perubahan di tengah kunjungan |
| Usulan baseline | Diskon dihitung ulang mengikuti penjamin pada saat tagihan difinalkan, dengan jejak perubahan |
| Contoh | Konsultasi Rp 500.000. Pribadi: diskon Rp 50.000, tagihan Rp 450.000. Asuransi ditambahkan sebelum final: diskon dicabut, tagihan ke asuransi Rp 500.000 |
| Dampak | Konsekuensi billing dan lifecycle diskon |
| Pemilik | Yoga Aji Pratama (aturan manfaat), dengan titik sentuh owner `billing-kasir` |
| Status | `OPEN` |
| Dampak domain | S4 berhenti. S1a, S3, dan S5 tidak terdampak |

### `DEC-CRM-003` — Pemetaan kelompok diskon ke tagihan

| Field | Isi |
|---|---|
| Pertanyaan | Master tier punya enam persen diskon: pendaftaran, konsultasi, tindakan, laboratorium, radiologi, farmasi. Item tagihan mana masuk kelompok mana? Bagaimana dengan item di luar enam kelompok (kamar, bahan habis pakai, biaya administrasi)? |
| Kemampuan terdampak | S4 |
| Bukti saat ini | Kolom diskon ada (`CRM-CAP-001`), tetapi tidak ada pemetaan ke kategori item billing (`CRM-CAP-009`) |
| Usulan baseline | Pemetaan per kategori item billing disimpan sebagai master, dan item tanpa pemetaan tidak didiskon |
| Contoh | Tagihan: konsultasi Rp 300.000 (diskon konsultasi 10% = Rp 30.000), obat Rp 200.000 (diskon farmasi 5% = Rp 10.000), biaya administrasi Rp 25.000 (tanpa diskon). Total diskon Rp 40.000 |
| Dampak | Nominal diskon setiap tagihan |
| Pemilik | Yoga Aji Pratama, dengan titik sentuh owner `billing-kasir` |
| Status | `OPEN` |
| Dampak domain | S4 berhenti |

### `DEC-CRM-004` — Perpanjangan sebelum habis

| Field | Isi |
|---|---|
| Pertanyaan | Member Gold yang berlaku s.d. 12 Okt 2027 memperpanjang dan membayar pada 1 Okt 2027. Periode baru mulai kapan? |
| Kemampuan terdampak | S1b |
| Bukti saat ini | `CRM-DEC-005` hanya mengatur perpanjangan **sesudah** kedaluwarsa. `CRM-DEC-012` mengatur upgrade (mulai saat lunas, sisa hangus) |
| Opsi | (a) Mulai saat lunas seperti upgrade: periode baru 1 Okt 2027 s.d. 30 Sep 2028, dan 12 hari sisa hangus. Paling sederhana, tanpa status baru. (b) Mulai 13 Okt 2027: periode baru s.d. 12 Okt 2028, sehingga butuh status "Terjadwal" dan dua baris membership sekaligus. (c) Perpanjangan hanya boleh setelah kedaluwarsa |
| Dampak | State machine dan aturan "satu membership aktif" |
| Pemilik | Yoga Aji Pratama |
| Status | `OPEN` |
| Dampak domain | S1b berhenti. S1a tetap jalan, dan perpanjangan setelah kedaluwarsa sudah tercakup |

## 6. Apa yang Boleh Berjalan

Diteruskan ke `hospital-domain-architect`:

| Slice | Yang diteruskan | Dependency yang harus tetap terlihat |
|---|---|---|
| `S1a` | Siklus hidup membership di `MstPatientMembership`, termasuk Perlu Verifikasi, nomor otomatis, koreksi terbatas, kedaluwarsa dan batal otomatis | Pemicu Menunggu Pembayaran ke Aktif dimodelkan sebagai kontrak masuk "iuran lunas", sedangkan sumbernya menunggu `DEC-CRM-001`. Gap `CRM-GAP-007` s.d. `010`, `012`, `013` dibawa sebagai usulan atau konfigurasi |
| `S3` | Penentuan "member aktif" yang dibaca pendaftaran, kiosk, nurse station, dan antrean, serta pencarian pasien dengan status member | Batasan keselamatan `CRM-DEC-007` bagi semua konsumen |
| `S5` | Otorisasi: rute berizin untuk CRM, kiosk hanya membaca, admisi hanya melihat | `CRM-GAP-011` sebagai konfigurasi Role Access |

## 7. Apa yang Harus Berhenti

| Slice | Berhenti sampai | Pemilik |
|---|---|---|
| `S1b` | `DEC-CRM-004` diputuskan | Yoga Aji Pratama |
| `S2` | `DEC-CRM-001` disetujui | Owner `billing-kasir`, Finance/Accounting |
| `S4` | `DEC-CRM-002` dan `DEC-CRM-003` diputuskan | Yoga Aji Pratama, titik sentuh `billing-kasir` |

## 8. Langkah Berikutnya

1. `/grill-me` Amendment pass untuk `DEC-CRM-002`, `DEC-CRM-003`, dan `DEC-CRM-004`. Ketiganya
   wewenang owner CRM dan bisa dijawab di sesi ini.
2. Owner billing menjawab `DEC-CRM-001` lewat permintaan amendment yang sudah diajukan.
3. `/hospital-domain-architect` untuk `S1a`, `S3`, dan `S5`, bisa berjalan sekarang. Slice
   lain menyusul setelah keputusannya tertutup.

## Revisi 1a — Setelah Amendment Pass 3 (2026-10-13)

| Decision gate | Ditutup oleh | Akibat |
|---|---|---|
| `DEC-CRM-004` perpanjangan sebelum habis | `CRM-DEC-024` (disambung, status Terjadwal, jendela 30 hari), `CRM-DEC-025` (Terjadwal dibatalkan dengan refund penuh saat upgrade/berhenti) | **S1b → `READY_FOR_DOMAIN_DESIGN`**. Refund periode Terjadwal ikut bergantung pada S2 |
| `DEC-CRM-002` penjamin berubah | `CRM-DEC-026` (dihitung ulang saat final, tercatat) | Gap `CRM-GAP-004` → `CONFIRMED` |
| `DEC-CRM-003` pemetaan kelompok diskon | `CRM-DEC-027` (ikut asal item, sisanya tanpa diskon) | Gap `CRM-GAP-005` → `CONFIRMED`. **S4 → `READY_FOR_DOMAIN_DESIGN`** untuk aturan manfaat. Penerapan di billing menunggu persetujuan owner billing (pertanyaan 5 permintaan amendment) |
| `DEC-CRM-001` tagihan iuran di billing | Belum | **S2 tetap `BUSINESS_DECISION_REQUIRED`** |

Temuan impact scan yang dibawa ke arsitektur domain: `CRM-CAP-016`. Ada dua definisi "member
aktif" (referensi pasien vs klasifikasi antrean RJ). Slice S3 harus menetapkan satu sumber.

Diteruskan ke `hospital-domain-architect`: **S1a, S1b, S3, S4 (aturan manfaat), S5**. S2
dimodelkan sebagai kontrak masuk "iuran lunas" dan kontrak keluar "batalkan tagihan" atau
"minta refund", yang sumbernya menunggu `DEC-CRM-001`.

