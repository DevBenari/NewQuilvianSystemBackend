# Farmasi — Penilaian Kelengkapan Requirement: Financial Clearance Handoff

| Field | Value |
| --- | --- |
| Blueprint | `PHA-BP-001` |
| Assessment ID | `PHA-RCG-002` |
| Menilai ulang | `PHA-RCG-001` (20 Agustus 2026), khusus bagian yang menahan "callback Billing" |
| Tanggal | 21 September 2026 |
| Backend snapshot | `6782ae652ca53299f7469c49b2edb64d23e77b60` (branch `Yasmina`, working tree memuat perubahan belum di-commit) |
| Frontend snapshot | `1b138b9aac7a50524fd751a47c9a76e0a55f8803` (branch `yasmina`) |
| Kesiapan slice ini | **`READY_FOR_DOMAIN_DESIGN`** per 21 September 2026 — satu-satunya blocker (`PHA-OQ-026`) ditutup pada hari yang sama oleh `PHA-DEC-068`, `PHA-DEC-068-A`, `PHA-DEC-069`, dan `PHA-DEC-070`. Lihat bagian 9 |
| Kesiapan saat penilaian pertama ditulis | `BUSINESS_DECISION_REQUIRED` — dipertahankan sebagai histori, jangan dibaca sebagai keadaan sekarang |

## 1. Scope penilaian

### Yang dinilai

Satu slice sempit: **bagaimana Farmasi mengetahui secara authoritative bahwa sebuah resep
sudah lunas secara finansial menurut Billing**, sehingga resep boleh berpindah dari keadaan
"menunggu pembayaran" ke "boleh masuk antrean telaah apoteker".

Slice ini berhenti tepat di titik resep masuk antrean. Tidak lebih jauh.

### Yang tidak dinilai

| Di luar scope | Pemilik slice |
| --- | --- |
| Routing Depo | Sudah `READY_FOR_DOMAIN_DESIGN` pada `PHA-RCG-001`, tidak dinilai ulang |
| Reservasi stok setelah resep diproses | Slice tersendiri, tetap seperti penilaian `PHA-RCG-001` |
| Penyiapan, peracikan, penyerahan obat | Slice tersendiri |
| Partial dispensing, retur, refund, obat tidak diambil | Slice tersendiri |
| Jalur pengecualian darurat klinis | Slice tersendiri — lihat catatan pada dimensi 17 |

### Mengapa dinilai ulang

`PHA-RCG-001` tidak pernah menilai slice ini sebagai slice tersendiri. Ia menempelkannya pada
slice "Reservasi setelah payment gate", lalu menahannya bersama-sama. Padahal keduanya
terpisah secara waktu: yang satu menentukan resep **boleh masuk** antrean, yang lain terjadi
**sesudah** Farmasi mulai bekerja.

Selain itu, `PHA-RCG-001` memotret keadaan yang sudah berubah secara material. Ia menandai
dimensi Integrasi sebagai `CONFLICT` dengan alasan "Billing authoritative `CONFLICT` dengan
generic `Prescription.Update`" — konflik yang sudah diselesaikan empat hari sesudahnya.

## 2. Bukti yang dipakai

| Bukti | Jenis | Wewenang |
| --- | --- | --- |
| `PHA-DEC-063` s.d. `PHA-DEC-067` (21 September 2026) | Keputusan pemilik | Requirement eksplisit terkini |
| `PHA-DEC-008`, `PHA-DEC-041`, `PHA-DEC-043`, `PHA-DEC-048` | Keputusan pemilik terdahulu | Requirement eksplisit |
| `RJ-BIL-GATE-DEC-007`, governance `CLOSED` 21 September 2026 | Keputusan lintas modul | Requirement eksplisit |
| `BKC-DEC-100`, `BKC-DEC-105` (termasuk `BKC-DES-031`) | Keputusan pemilik Billing | Requirement eksplisit |
| `RJ-BIL-BE-002` `COMPLETE` 24 Agustus 2026 | Bukti eksekusi terverifikasi | Bukti implementasi V2 |
| Pembacaan source langsung 21 September 2026 | Bukti implementasi | Bukti implementasi V2 terverifikasi |

Baseline rumah sakit Indonesia **tidak dipakai**. Tidak ada observasi `REFERENCE_ONLY` yang
diperlakukan sebagai kebijakan rumah sakit dalam penilaian ini.

### Bukti implementasi yang diverifikasi langsung

| Fakta | Bukti |
| --- | --- |
| Satu kunjungan tepat satu tagihan | `BilInvoiceConfiguration.cs:26`, unique index pada `EncounterId` |
| Tagihan hanya melacak sisa di tingkat tagihan, bukan per baris | `BilInvoiceItem.cs` tidak memiliki kolom nominal terbayar |
| Keadaan tagihan mengenal lunas dan lunas-lewat-penghapusan sebagai dua hal berbeda | `BilInvoice.cs:26-29` |
| Cara bayar sudah membedakan asuransi dan penjamin perusahaan | `MstPaymentMethod.cs:21-37` |
| Keadaan finansial resep tidak pernah lagi ditulis dari modul klinis | Seluruh source hanya menulis `NotBilled` di `PrescriptionWorkflowService.cs:39` |
| Resep tidak punya jalan masuk ke antrean Farmasi | Tidak ada penulisan `ReadyForPharmacy` di mana pun; `PrescriptionReviewService.cs:51-56` mensyaratkannya |
| Gate penyerahan sudah menanti tiga keadaan lunas yang sama | `PrescriptionDispensingService.cs:58-63` |
| **Status lunas dapat dicabut kembali** | `BillingInvoiceClosureService.cs:121-126` mengembalikan tagihan ke belum-lunas saat sisa naik lagi |

## 3. Temuan kelengkapan pada 18 dimensi

| ID | Dimensi | Status | Dasar |
| --- | --- | --- | --- |
| 01 | Tujuan | `CONFIRMED` | Resep tidak boleh dikerjakan Farmasi sebelum Billing menyatakannya lunas (`PHA-DEC-008`, `PHA-DEC-063`) |
| 02 | Aktor | `CONFIRMED` | Billing menetapkan, sistem menyalin, Farmasi hanya membaca (`PHA-DEC-043`, `PHA-DEC-063`) |
| 03 | Pemicu / prasyarat | `CONFIRMED` | Resep difinalkan dokter lalu menunggu; tagihan kunjungan terbentuk |
| 04 | Alur utama | `CONFIRMED` | Tagihan lunas → salinan keadaan finansial di Farmasi diperbarui → resep masuk antrean |
| 05 | Alur alternatif / exception | **`MISSING`** | Pencabutan status lunas belum diatur — lihat `PHA-OQ-026` |
| 06 | Data minimum | `CONFIRMED` | Rujukan finansial, keadaan, nomor versi, asal, waktu perubahan sumber, waktu salin (`RJ-BIL-GATE-DEC-007`) |
| 07 | Aturan bisnis / validation | `CONFIRMED` | Lunas berarti seluruh tagihan kunjungan lunas, bukan per baris (`PHA-DEC-064`) |
| 08 | Status / perubahan status | **`MISSING` sebagian** | Keadaan maju sudah pasti; keadaan mundur belum — lihat `PHA-OQ-026` |
| 09 | Peran / authorization | `PROPOSED` | Farmasi hanya membaca; tidak ada aksi finansial yang perlu wewenang baru |
| 10 | Dependency antarmodul | `CONFIRMED` | Billing sebagai pemilik keadaan finansial, Farmasi sebagai pembaca |
| 11 | Integrasi internal | `CONFIRMED` | Salinan searah dari Billing, bernomor versi dan idempotent (`PHA-DEC-063`) |
| 12 | Hasil akhir | `CONFIRMED` | Resep tampil di antrean telaah apoteker dan keadaan finansialnya tercatat |
| 13 | Pembatalan / koreksi | **`MISSING`** | Lihat `PHA-OQ-026` |
| 14 | Audit / histori | `CONFIRMED` | Setiap penyalinan tercatat dan dapat direkonsiliasi (`PHA-DEC-063`) |
| 15 | Notifikasi | `PROPOSED` | Antrean kerja sudah menjadi pemberitahuan alami; pemberitahuan aktif belum diputuskan |
| 16 | Dampak billing / charge | `CONFIRMED` | Slice ini tidak pernah mengubah tagihan; hanya membaca |
| 17 | Dampak keselamatan klinis | `CONFIRMED` dengan batas | Jalur darurat klinis sengaja di luar slice ini — lihat catatan di bawah |
| 18 | Pelaporan / traceability | `CONFIRMED` | Dari resep sampai tagihan sudah dapat ditelusuri lewat rantai penautan yang terbukti |

### Catatan dimensi 17

`RJ-BIL-GATE-DEC-007` menyediakan jalur pengecualian darurat klinis yang dapat melanjutkan
pelayanan walau syarat finansial belum terpenuhi, dengan syarat ada kebijakan yang disahkan,
alasan, pemberi wewenang, dan waktu. Kebijakan itu sendiri belum tersedia.

Ini **tidak memblokir** slice ini, karena jalur normal dapat dirancang utuh tanpa jalur
darurat. Namun batas itu harus dinyatakan terbuka: sampai kebijakan darurat disahkan, satu-
satunya jalan resep masuk antrean adalah lewat pernyataan lunas dari Billing. Keadaan ini
sama dengan yang sudah berjalan hari ini.

## 4. Butir CONFIRMED, PROPOSED, MISSING, dan CONFLICT

### `CONFIRMED` yang menutup temuan lama

| Temuan `PHA-RCG-001` | Keadaan sekarang | Bukti |
| --- | --- | --- |
| Integrasi `CONFLICT`: Billing authoritative berbenturan dengan `Prescription.Update` | **Selesai.** Empat endpoint finansial Farmasi dihapus; tidak ada lagi jalur klinis yang menetapkan keadaan finansial | `RJ-BIL-BE-002` `COMPLETE`; seluruh source hanya menulis `NotBilled` |
| Aktor: owner callback Billing `MISSING` | **Selesai.** Billing adalah pemilik tunggal, Farmasi pembaca | `PHA-DEC-063`, `PHA-DEC-043` |
| `PHA-OQ-015`: sumber authoritative dan callback ganda | **Selesai.** Salinan bernomor versi, idempotent, versi basi ditolak | `PHA-DEC-063` |

### `MISSING` yang tersisa

| Butir | Dampak | Decision ID |
| --- | --- | --- |
| Nasib resep yang sudah masuk antrean ketika status lunas dicabut Billing | `BLOCKING` | `PHA-OQ-026` |
| Kebijakan darurat klinis yang disahkan | Tidak memblokir slice ini; membatasi cakupannya | `PHA-OQ-027` |
| Pemberitahuan aktif saat resep masuk antrean | `NON_BLOCKING_STANDARD` | — |

### `CONFLICT`

Tidak ditemukan pertentangan bukti yang material untuk slice ini.

Perlu dicatat satu hal yang **bukan** pertentangan tetapi mudah disangka begitu: `PHA-DEC-044`
menetapkan daftar keadaan resep yang tidak memuat keadaan pembayaran, sementara keadaan
terpasang memuatnya. `PHA-DEC-063` sudah menyelesaikan pertanyaan bisnisnya dengan menyatakan
penjagaan pembayaran tetap berlaku. Sisa pekerjaannya adalah pemetaan teknis, dan itu wewenang
tahap desain, bukan gerbang ini.

## 5. Klasifikasi dampak gap

### `BLOCKING`

Satu butir, yaitu `PHA-OQ-026`. Ia memenuhi kriteria pemblokir karena mengubah **state machine
dan lifecycle** resep, dan menyentuh **alur kerja yang sulit dibatalkan**: obat yang sudah
diracik tidak dapat dikembalikan menjadi bahan.

### Bukan pemblokir bagi slice ini

| Butir | Alasan |
| --- | --- |
| `PHA-OQ-027` (kebijakan darurat klinis) | Jalur normal lengkap tanpanya; ketiadaannya hanya mempersempit cakupan, tidak membuat desain tidak aman |
| Pemberitahuan aktif | Pilihan penyajian, tidak mengubah makna domain |
| Rincian hak akses baca | Farmasi tidak melakukan mutasi finansial; mengikuti hak akses resep yang sudah ada |

## 6. Empat pertanyaan lama: apakah memblokir slice ini?

Inilah pertanyaan pokok yang diminta dijawab. Jawabannya: **tidak, keempatnya tidak memblokir
slice ini.** Alasannya satu dan sama — keempatnya terjadi **sesudah** titik henti slice ini.

Urutan waktunya dikunci `PHA-DEC-041`: reservasi stok dilakukan setelah pembayaran sah **dan**
Farmasi mulai memproses resep. Artinya reservasi berada di hilir titik "resep masuk antrean".

| Decision ID | Pertanyaan | Letak terhadap slice ini | Memblokir? |
| --- | --- | --- | --- |
| `PHA-OQ-014` | Pembayaran sah tetapi reservasi gagal karena stok habis | **Hilir.** Reservasi baru terjadi setelah resep masuk antrean | Tidak |
| `PHA-OQ-009` | Obat tidak diambil setelah 24 jam | **Jauh di hilir.** Setelah obat siap diambil | Tidak |
| `PHA-OQ-016` | Partial dispensing | **Jauh di hilir.** Saat penyerahan | Tidak |
| `PHA-OQ-017` | Obat yang wajib pemeriksa kedua | **Jauh di hilir.** Saat penyerahan | Tidak |

### Arah dependency-nya satu arah

`PHA-OQ-014` **membutuhkan** slice ini — untuk menjawab "pembayaran sah tetapi reservasi
gagal", lebih dulu harus ada definisi "pembayaran sah", dan definisi itulah yang baru dikunci
`PHA-DEC-064`. Sebaliknya, slice ini **tidak membutuhkan** jawaban `PHA-OQ-014`: kegagalan
reservasi tidak mengubah kenyataan bahwa tagihannya lunas, dan tidak mengubah cara salinan
keadaan finansial bekerja.

Menahan slice ini demi `PHA-OQ-014` berarti menahan hulu demi hilir. Itu membalik urutan yang
benar.

### Contoh yang memperjelas pemisahan

Pasien membayar lunas di kasir pukul 09.00. Salinan keadaan finansial di Farmasi diperbarui,
resep masuk antrean. **Slice ini selesai di sini.** Pukul 09.05 petugas hendak mereservasi 10
tablet, ternyata stok tinggal 6 karena resep lain mendahului. Apa yang resmi terjadi sesudahnya
adalah `PHA-OQ-014` — dan itu tetap terbuka, tetapi tidak membuat langkah pukul 09.00 menjadi
tidak dapat dirancang.

## 7. Decision Log

### `PHA-OQ-026` — Pencabutan status lunas atas resep yang sudah diproses

| Field | Isi |
| --- | --- |
| Decision ID | `PHA-OQ-026` |
| Pertanyaan | Apa yang terjadi pada resep yang sudah masuk antrean Farmasi — bahkan mungkin sudah ditelaah, disiapkan, atau diracik — ketika Billing mencabut status lunas tagihan kunjungan itu? |
| Kemampuan terdampak | Financial clearance handoff Billing → Farmasi |
| Bukti saat ini | `BillingInvoiceClosureService.cs:121-126` membuktikan pencabutan itu **nyata terjadi di source**: tagihan yang sudah lunas kembali menjadi belum lunas ketika sisa tagihannya naik lagi, dan penanda waktu pelunasannya dikosongkan. Perilaku ini disetujui pemilik Billing lewat `BKC-DES-031` dalam `BKC-DEC-105`. Tidak satu pun dari `PHA-DEC-063` sampai `PHA-DEC-067` mengatur akibatnya pada resep |
| Usulan baseline | Resep yang belum disentuh ditarik kembali ke keadaan menunggu pembayaran; resep yang sudah diracik tidak ditarik tetapi ditahan sebelum penyerahan dan dimunculkan sebagai perkara yang harus diselesaikan. Ini **usulan**, bukan kebijakan rumah sakit |
| Dampak | Lifecycle resep, keadaan yang diizinkan, pemberitahuan kepada petugas, keselamatan pekerjaan yang sudah terlanjur dikerjakan, dan keterlusuran |
| Pemilik | Product/Domain Owner, Billing/Payer owner, Clinical Governance |
| Status | `OPEN` |
| Dampak implementasi | Slice financial clearance handoff terblokir sampai ini ditutup. Rancangan salinan keadaan finansial tidak dapat dinyatakan lengkap tanpa arah mundurnya |

**Mengapa ini penting, dengan contoh.** Pasien lunas pukul 09.00, resep masuk antrean, apoteker
meracik puyer pukul 09.20. Pukul 09.30 kasir mencatat tindakan tambahan yang terlewat, sehingga
tagihan kunjungan itu kembali belum lunas. Puyer sudah jadi dan tidak dapat dikembalikan
menjadi bahan. Sistem hari ini tidak punya jawaban resmi: apakah resep ditarik mundur, apakah
obat ditahan, siapa yang diberi tahu, dan bagaimana biayanya diselesaikan.

### `PHA-OQ-027` — Kebijakan pengecualian darurat klinis

| Field | Isi |
| --- | --- |
| Decision ID | `PHA-OQ-027` |
| Pertanyaan | Kebijakan apa yang mengizinkan obat diserahkan mendahului penyelesaian finansial dalam keadaan darurat klinis, siapa yang berwenang memberi izin, dan bagaimana kewajiban finansialnya diselesaikan kemudian? |
| Kemampuan terdampak | Jalur pengecualian di luar jalur normal slice ini |
| Bukti saat ini | `RJ-BIL-GATE-DEC-007` mensyaratkan adanya kebijakan yang disahkan beserta alasan, pemberi wewenang, dan waktu, serta menegaskan pengecualian ini bukan penghapusan kewajiban. Kebijakannya sendiri belum ada. `PHA-DEC-067` sudah menegaskan bahwa gangguan teknis **bukan** alasan yang sah |
| Usulan baseline | Tidak diusulkan. Ini kebijakan klinis dan finansial rumah sakit |
| Dampak | Cakupan slice, bukan keamanan rancangannya |
| Pemilik | Clinical Governance, Billing/Finance |
| Status | `OPEN`, tidak memblokir slice ini |
| Dampak implementasi | Selama terbuka, satu-satunya jalan resep masuk antrean adalah pernyataan lunas dari Billing |

## 8. Temuan tambahan: manifest blueprint sudah usang

Ini bukan keputusan bisnis, tetapi material bagi tahap berikutnya.

| Field manifest | Tercatat | Kenyataan |
| --- | --- | --- |
| `decision_revision` | `2` | Decision log sudah sampai `PHA-DEC-067` (21 September 2026) |
| `backend_source_sha` | `767470f7…` | `6782ae65…` |
| `frontend_source_sha` | `400104f2…` | `1b138b9a…` |
| `artifact_hashes` `00-interview-decisions.md` | `18c87934…` | Sudah berubah oleh amandemen 3 September, 7 September, dan 21 September |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_READY_FOR_ROUTING_DEPOT` | Hanya berlaku untuk slice Routing Depo, bukan slice ini |

Konsekuensinya bagi slice ini: arsitektur domain **belum pernah dijalankan** untuknya. Karena
arsitektur domain bersifat opsional, slice ini boleh langsung ke desain begitu `PHA-OQ-026`
ditutup — dengan catatan readiness-nya dicatat sebagai `DOMAIN_ARCHITECTURE_NOT_RUN` beserta
alasannya, bukan diam-diam meminjam kesiapan milik slice Routing Depo.

## 9. Kesiapan, yang boleh berjalan, dan yang harus berhenti

### Kesiapan

**`READY_FOR_DOMAIN_DESIGN`** untuk slice financial clearance handoff, per 21 September 2026.

Perjalanannya dalam satu hari: `PHA-RCG-001` menahan slice ini di balik lima pertanyaan. Empat
terbukti berada di hilir dan tidak relevan, satu sudah ditutup sebelumnya, dan sebagai gantinya
penilaian ini menemukan satu pemblokir baru yang lebih tepat sasaran — `PHA-OQ-026`. Pemblokir
itu kemudian ditutup pada hari yang sama.

### Bagaimana `PHA-OQ-026` ditutup

| Decision | Isi ringkas |
| --- | --- |
| `PHA-DEC-068` | Clearance tidak dicabut hanya karena invoice kembali `FINAL`; ditentukan pada tingkat resep. Biaya tindakan/laboratorium/kamar yang ditambahkan tidak mencabut clearance obat yang sudah dibayar |
| `PHA-DEC-068-A` | Untuk sebab berbasis penarikan uang — reversal pembayaran, reversal write-off, pencabutan penjaminan — clearance selalu dicabut (fail-closed), karena `BilPaymentAllocation` hanya mengenal sasaran `INVOICE` sehingga Billing tidak dapat membuktikan uang mana milik resep |
| `PHA-DEC-069` | Bila pencabutan sah terjadi saat Farmasi sudah bekerja, resep ditahan di tempat dan kemajuannya dikunci — tidak ditarik mundur, racikan tidak direstock, dan pemulihan melanjutkan dari titik terakhir |
| `PHA-DEC-070` | Kontrak clearance berbentuk pernyataan per resep, memuat identitas resep dan invoice, keadaan clearance, hasil finansial, kode sebab, nomor versi, dan waktu berlaku |

Jawaban pemilik mengoreksi premis pertanyaan, bukan sekadar memilih di antara pilihan yang
disodorkan. Itu tercatat apa adanya pada decision log.

### Dampaknya pada dimensi yang tadinya `MISSING`

| Dimensi | Sebelum | Sesudah |
| --- | --- | --- |
| 05 Alur alternatif / exception | `MISSING` | `CONFIRMED` — sebab yang mencabut dan yang tidak sudah terdaftar |
| 08 Status / perubahan status | `MISSING` sebagian | `CONFIRMED` — arah mundur ditetapkan sebagai penahanan, bukan penarikan status |
| 13 Pembatalan / koreksi | `MISSING` | `CONFIRMED` — perlakuan pencabutan dan pemulihan sudah dapat diuji |

### Batas yang diketahui dan sengaja diterima

`PHA-DEC-064` mengikat pemberian clearance pada invoice `CLOSED`, sementara `PHA-DEC-068`
melepaskan pencabutan dari status invoice. Asimetri ini membuat urutan pencatatan menentukan
hasil: biaya tindakan yang dicatat **sesudah** pasien melunasi tidak menahan obat, sedangkan
biaya yang sama bila dicatat **sebelum** pelunasan akan menahannya. Dua pasien dengan tagihan
identik dapat mengalami hasil berbeda.

Ini bukan cacat tersembunyi melainkan konsekuensi langsung dari tidak adanya alokasi uang per
baris di Billing, dan sudah dicatat pada decision log. Menghilangkannya menuntut keputusan
Billing tersendiri.

### Yang boleh berjalan sekarang

- Desain arsitektur target untuk slice financial clearance handoff.
- Slice Routing Depo, tidak terpengaruh penilaian ini.

### Yang harus berhenti

- Slice reservasi, penyerahan, partial dispensing, dan obat tidak diambil, sesuai `PHA-RCG-001`
  yang tidak dinilai ulang di sini.

### Keputusan pemilik yang masih dibutuhkan

Tidak ada yang memblokir slice ini. `PHA-OQ-027` tetap terbuka tetapi hanya membatasi cakupan,
bukan menghalangi desain.

### Skill berikutnya

| Kondisi | Skill |
| --- | --- |
| Sekarang | `design-business-module`, dengan `domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN` dicatat beserta alasannya, dan manifest `PHA-BP-001` yang usang disinkronkan |
| Bila batas domain ternyata tidak dapat diselesaikan dari bukti saat desain | `hospital-domain-architect`, opsional |

`PHA-OQ-027` tidak perlu ditutup sebelum desain, tetapi harus tetap tercatat terbuka agar
cakupan slice tidak disalahpahami sebagai mencakup jalur darurat.
