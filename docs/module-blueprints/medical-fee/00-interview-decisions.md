# Medical Fee — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `MF-BP-001` |
| Revision | `1` |
| Status | `approved` untuk `MF-DEC-001`..`010` dan `MF-DEC-012`..`018`. `MF-DEC-011` berstatus `superseded` oleh `MF-DEC-012` |
| Pass | `Scope pass` — selesai 20 September 2026 · `Capability audit` — selesai 20 September 2026 · `Closure pass` — **selesai** 20 September 2026 |
| Revision | `2` — dinaikkan pada Closure pass; `MF-DEC-012`..`018` ditambahkan, `MF-DEC-011` digantikan |
| Product/domain owner | Yasmin (owner Finance, bertindak sebagai narasumber Medical Fee) |
| Backend SHA | `09101d05` (branch `Yasmina`) |
| Frontend SHA | `abed49b03` |
| Masukan | `Referensi_Meeting_FIN-OQ_dan_Medical_Fee.pdf` (transcript meeting RS MMC) · `FIN-BP-001` keputusan `FIN-DEC-003`, `FIN-DEC-019`, `FIN-DEC-022` · `FIN-CAP-020`, `FIN-CAP-021` |
| Tanggal mulai | 20 September 2026 |

## Catatan cara kerja

Wawancara ini menjawab **"aturan bisnisnya bagaimana"**, bukan "apa yang sudah ada di sistem".

**Scope dikunci tanpa capability map formal.** Belum ada `01-existing-capability-map.md` untuk
modul ini. Yang dipakai sebagai ganti: pemeriksaan terarah ke source pada `09101d05` yang
dijalankan saat pass desain Finance, hasilnya tercatat sebagai Fact di bawah. Audit penuh lewat
`/trace-existing-capabilities` tetap disarankan sebelum arsitektur dikunci.

Rekomendasi pada setiap pertanyaan **bukan keputusan dan bukan approval**.

---

## Scope dan Outcome

**Modul:** Medical Fee (`medical-fee`)

**Satu kalimat batas scope:** Medical Fee menentukan berapa jasa yang berhak diterima setiap
tenaga medis atas layanan yang sudah diberikan, memverifikasi dan menyetujuinya, lalu
menyerahkan hasil yang sudah final ke Finance — tanpa pernah menghitung ulang tagihan pasien
maupun membayar tenaga medis.

**Catatan nama:** cakupan penerima diperluas owner menjadi dokter **dan** tenaga kesehatan lain
(`MF-DEC-002`). Nama modul tetap "Medical Fee" untuk sementara; apakah nama itu masih tepat
dicatat sebagai `MF-OQ-001`.

### Di dalam scope

| ID | Kemampuan | Keterangan |
|---|---|---|
| `MF-SC-001` | Aturan perhitungan jasa | Aturan berversi; nilai lama tidak berubah saat aturan diperbarui |
| `MF-SC-002` | Perhitungan jasa per layanan per penerima | Termasuk pembagian tim ketika satu layanan melibatkan beberapa orang |
| `MF-SC-003` | Verifikasi dan persetujuan hasil | Jenjang dan wewenangnya |
| `MF-SC-004` | Koreksi hasil sebelum disetujui | Penyesuaian dengan alasan dan jejak |
| `MF-SC-005` | Periode jasa dan penutupannya | Pola bulanan |
| `MF-SC-006` | Penyerahan hasil yang sudah disetujui ke Finance | Kontrak keluar; berupa jasa **kotor** (`MF-DEC-005`) |
| `MF-SC-007` | Data PKS/SK sisi tarif sharing | Dimiliki modul ini untuk rilis pertama (`MF-DEC-011`); bukan kontrak kerja utuh |

### Di luar scope — untuk modul lain

| ID | Kemampuan | Pemilik | Titik sentuh yang tetap dibahas |
|---|---|---|---|
| `MF-OOS-001` | Perhitungan tagihan pasien, `BilInvoiceItem.DoctorShare`, diskon dokter | Billing | Hanya bentuk kontrak nilai jasa yang mengalir dua arah |
| `MF-OOS-002` | Utang dan pembayaran ke tenaga medis | Finance (`FIN-DEC-003`) | Hanya bentuk hasil yang diserahkan |
| `MF-OOS-003` | Aturan kelayakan dan penjadwalan layanan dokter (`MstDoctorServiceRule`) | Health Services / MasterData | Tidak ada; sekadar diperjelas bahwa namanya berbeda konsep |
| `MF-OOS-004` | Master tenaga medis, identitas, kontrak kerja | HR / MasterData | Rujukan identitas dan tipe kepegawaian |
| `MF-OOS-005` | Jurnal dan buku besar | Accounting | Tidak ada; Finance yang menerbitkan kejadian |
| `MF-OOS-006` | Pencatatan siapa yang mengerjakan layanan | Modul layanan masing-masing (`MF-DEC-003`) | Kontrak masuk: daftar pelaksana beserta perannya |

---

## Aktor dan Tanggung Jawab

Belum digali secara kritis pada pass ini. Yang sudah diketahui dari bukti meeting:

| Peran | Yang dikerjakan hari ini | Sumber |
|---|---|---|
| Tim akuntansi (3 orang) | Menarik data, mengecek tarif sharing terhadap PKS/SK, membuat dan menyetujui rekap | Transcript 15 Juli 2026 |
| Dokter (DPJP) | Menyetujui diskon yang diberikan ke pasien — **di Billing, bukan di modul ini** | Transcript 24 April 2026 |

Siapa yang **seharusnya** memverifikasi dan menyetujui hasil jasa di sistem baru belum
ditentukan — lihat `MF-OQ-003`.

---

## Business Rules dan Invariants

Baris **Fact** berasal dari bukti yang sudah ada, bukan dari keputusan pass ini.

### Fakta dari source `09101d05`

| # | Fakta | Bukti |
|---|---|---|
| F1 | Bagian dokter per baris tagihan sudah ada, tetapi **diinput manual** tanpa aturan perhitungan | `BilInvoiceItem.DoctorShare`; `BillingInvoiceService` hanya memvalidasi "tidak melebihi gross item" |
| F2 | Nilai yang mengalir ke Finance hari ini = jumlah `DoctorShare` dikurangi diskon dokter yang disetujui | `BillingArApHandoffService` |
| F3 | Dokter yang diakui hanya **satu per kunjungan** | Diambil dari `RegPatientEncounter.DoctorId` |
| F4 | Untuk operasi, tim lengkap beserta perannya **sudah tercatat** | `OprTeamMember` dengan `Role`: `PrimarySurgeon`, `AssistantSurgeon`, `Anesthesiologist`, `ScrubNurse`, `CirculatingNurse`, `Other`, plus penanda `IsLead` |
| F5 | Tarif layanan tersedia sebagai angka acuan | `MstTariff.NormalPrice` |
| F6 | `MstDoctorServiceRule` **bukan** aturan perhitungan jasa — isinya aturan kelayakan layanan, nol field nominal | `FIN-CAP-020`, berstatus `Conflict` |
| F7 | Tidak ada satu pun entity perhitungan jasa medis di seluruh backend | `FIN-CAP-021` |

### Fakta dari praktik RS MMC (transcript meeting)

| # | Fakta | Sumber |
|---|---|---|
| F8 | Periode jasa **bulanan**, tanggal 1 sampai akhir bulan | 15 Juli 2026 |
| F9 | Data ditarik dari tagihan yang sudah **ditutup**, bukan berdasarkan status bayar pasien atau asuransi | 15 Juli 2026 |
| F10 | Tarif sharing berbeda menurut **tipe dokter**: Full Time (karyawan), Part Time/jaga, Dokter Tamu, serta dokter bertarif khusus | 15 Juli 2026 |
| F11 | Sumber kebenaran tarif sharing adalah **PKS/SK** masing-masing, dan pengecekannya manual — **proses paling memakan waktu** | 15 Juli 2026 |
| F12 | Pembagian tim untuk Lab, Radiologi, Anestesi, Digestive, Urologi **belum tertangani sistem**, masih Excel satelit | 15 Juli 2026 |
| F13 | Sebelum dibayar, jasa kotor disesuaikan dengan potongan dan tambahan: PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian | 15 Juli 2026 |
| F14 | Pembayaran dua tahap per bulan, tanggal 5 dan tanggal 10, dipisah berdasarkan apakah pasien/asuransi sudah membayar ke rumah sakit | 15 Juli 2026 |
| F15 | Diskon yang diberikan dokter ke pasien memerlukan persetujuan dokter itu sendiri — **dikerjakan di Billing** | 24 April 2026 |
| F16 | Skala: ±220 dokter per bulan, proses persetujuan satu-per-satu memakan ±4 hari kerja | 15 Juli 2026 |
| F17 | Bila ada kesalahan input, prosesnya harus dibatalkan dan diulang dari awal — tidak bisa diperbaiki langsung | 15 Juli 2026 |
| F18 | Tim rumah sakit secara eksplisit meminta proses yang lebih sederhana di sistem baru | 15 Juli 2026 |

### Batas yang sudah dikunci modul tetangga

| # | Aturan | Sumber |
|---|---|---|
| F19 | Finance mengakui utang jasa **hanya** dari hasil yang sudah disetujui, bukan dari `BilApHandoff` | `FIN-DEC-003` Opsi B |
| F20 | Kejadian akuntansi pengakuan piutang **tidak** membawa komponen jasa medis; jasa medis terbit terpisah | `FIN-DEC-003` |
| F21 | Satu pembayaran Finance boleh merekap banyak hasil jasa, dan tiap rupiahnya tetap tertelusur | `FIN-DEC-019` |

---

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `MF-DEC-001` | Decision | Medical Fee menjadi **sumber angka jasa**. Aturan perhitungan hidup di modul ini, dan `BilInvoiceItem.DoctorShare` diisi dari hasil perhitungannya — **bukan lagi diketik petugas**. Menutup sumber kesalahan terbesar hari ini (F1). | Yasmin — **titik sentuh, butuh konfirmasi owner Billing** | `approved` (sisi Medical Fee) | Yasmin, 20 September 2026 | Jawaban langsung owner; F1, F18 |
| `MF-DEC-002` | Decision | Cakupan penerima jasa **diperluas**: bukan hanya dokter, tetapi juga tenaga kesehatan lain. **Memperluas batas scope** yang semula diusulkan hanya dokter. | Yasmin | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; F4 menunjukkan perawat sudah tercatat dalam tim operasi |
| `MF-DEC-003` | Decision | Daftar pelaksana beserta perannya dicatat **di modul layanan masing-masing** saat layanan diberikan, mengikuti pola `OprTeamMember` yang sudah berjalan untuk operasi. Medical Fee membacanya, tidak menentukan sendiri. | Yasmin — **butuh koordinasi lintas modul** (poli, rawat inap, penunjang) | `approved` (sisi Medical Fee) | Yasmin, 20 September 2026 | Jawaban langsung owner; F3, F4, F12 |
| `MF-DEC-004` | Decision | Dasar perhitungan jasa = **persentase dari tarif layanan**, berbeda per penerima dan per layanan. Aturan disimpan **berversi** sehingga perubahan tidak mengubah jasa yang sudah dihitung pada periode lalu. | Yasmin | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; F5, F10, F11 |
| `MF-DEC-005` | Decision | Medical Fee menyerahkan jasa **KOTOR** ke Finance. Seluruh potongan dan tambahan ("Atless") — PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian — diterapkan **Finance** saat menyusun rekap pembayaran. Alasan: potongan itu bersifat per-orang-per-periode, bukan per-layanan; memaksakannya ke level layanan berarti membagi kasbon ke puluhan baris jasa. Menutup `MF-OQ-002`. | Yasmin — **berdampak pada `FIN-BP-001`**, lihat `MF-CQ-01` | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; F13, F14 |
| `MF-DEC-006` | Decision | Tarif sharing disimpan **per kontrak/PKS-SK dengan masa berlaku**, bukan per tipe dokter atau per dokter langsung. Satu PKS/SK menjadi satu kesatuan aturan bermasa berlaku yang memuat tarif sharing penerima itu. Menutup `MF-OQ-004`. | Yasmin — **butuh kejelasan siapa yang memasukkan PKS**, lihat `MF-OQ-007` | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; F10, F11 — langsung menghapus bottleneck pengecekan manual terhadap PKS |
| `MF-DEC-007` | Decision | Selama periode jasa **belum ditutup**, perubahan tagihan memicu **perhitungan ulang otomatis** dan nilai jasa menyesuaikan sendiri. Setelah periode ditutup atau hasil disetujui, perubahan hanya lewat **penyesuaian bernilai selisih** — angka lama tidak pernah diubah. Menutup `MF-OQ-005`. | Yasmin | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; menjawab F17 langsung ("salah input harus ulang dari awal") |
| `MF-DEC-008` | Decision | Utang jasa di Finance **diperluas menjadi satu entity utang jasa tenaga medis** yang membedakan jenis penerima lewat kolom, bukan lewat tabel terpisah per jenis — mengikuti pola `FinPayment` yang sudah melayani supplier dan dokter sekaligus. `FinDoctorPayable` hasil `FIN-BP-001` karena itu **digantikan**. Menutup `MF-OQ-001`. | Yasmin — **menuntut amendment pada `FIN-BP-001`**, lihat `MF-CQ-02` | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; `MF-DEC-002` |
| `MF-DEC-009` | Decision | Hasil jasa memakai pola **maker-checker**: penyusun tidak boleh menyetujui hasil yang disusunnya sendiri, dan wewenangnya ditentukan lewat konfigurasi permission — sama seperti `FIN-DEC-012` untuk write-off piutang. Menutup `MF-OQ-003`. | Yasmin | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; menutup gap yang diakui tim RS MMC sendiri (F16, bukti `FIN-DEC-022`) |
| `MF-DEC-010` | Decision | Pola pembayaran **dua tahap dipertahankan** di V2. Medical Fee tetap menghitung jasa dari **seluruh tagihan yang sudah ditutup** tanpa memandang status bayar; **Finance** yang menentukan jasa mana masuk tahap pertama atau kedua berdasarkan status bayar tagihan sumbernya. Menutup `MF-OQ-006`. | Yasmin — **menuntut jalur data status bayar di Finance** (`FIN-OQ-012`) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner; F9, F14 |
| `MF-DEC-011` | Decision | ~~Data PKS/SK dimiliki Medical Fee untuk rilis pertama, memuat sisi tarif sharing saja. Tidak menunggu HR membangun manajemen kontrak yang belum terjadwal.~~ | Yasmin | **`superseded`** oleh `MF-DEC-012` | Yasmin, 20 September 2026 | **Digantikan pada hari yang sama.** Alasan yang mendasarinya — "HR belum punya manajemen kontrak dan belum terjadwal" — terbukti **tidak benar** lewat `MF-CAP-010`. Keputusan ini tidak dihapus agar terlihat bahwa ia pernah diambil dan atas dasar apa |
| `MF-DEC-012` | Decision | **Menggantikan `MF-DEC-011`.** Tarif sharing disimpan sebagai entity Medical Fee yang **menunjuk `WfpContractHistory` milik HR**. Nomor kontrak, masa berlaku, tanggal tanda tangan, status, dan dokumennya dipakai ulang; Medical Fee hanya menambah sisi tarifnya. Satu sumber kebenaran untuk "kontrak mana", dan tarif ikut kedaluwarsa dengan sendirinya saat kontraknya berakhir. Menutup `MF-CQ-01`. | Yasmin — **titik sentuh, perlu sepengetahuan owner HR** | `approved` | Yasmin, 20 September 2026 | `MF-CAP-010`; jawaban langsung owner |
| `MF-DEC-013` | Decision | Persentase jasa dihitung dari **nilai kotor baris layanan**, sebelum diskon apa pun. Diskon promo yang diberikan rumah sakit ke pasien **tidak** mengurangi hak tenaga medis; hanya diskon yang diberikan dan disetujui tenaga medis itu sendiri yang menguranginya. Menutup `MF-OQ-009`. | Yasmin | `approved` | Yasmin, 20 September 2026 | `MF-CAP-003` — konsisten dengan perilaku Billing yang sudah berjalan |
| `MF-DEC-014` | Decision | Porsi antar penerima pada satu layanan ditentukan **persentase per peran yang ditetapkan di aturan tarif sharing**, bukan per kasus. Sistem membagi otomatis begitu tim tercatat. Menutup `MF-OQ-010`. | Yasmin | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner. **Konsekuensi yang MUST ditangani:** menuntut daftar peran yang seragam, sementara peran baru tercatat di operasi (`OprTeamRole`) dan belum ada di tindakan klinis maupun laboratorium — dicatat sebagai `MF-OQ-011` |
| `MF-DEC-016` | Decision | Medical Fee menetapkan **daftar perannya sendiri** untuk perhitungan jasa, lalu memetakan `OprTeamRole` yang sudah ada ke dalamnya. Modul layanan lain mengikuti daftar yang sama saat kelak menambahkan pencatatan tim. Menutup `MF-OQ-011`. | Yasmin | `approved` | Yasmin, 20 September 2026 | `MF-CAP-005`; tanpa daftar tunggal, porsi "operator" di operasi dan di tindakan klinis bisa berbeda arti |
| `MF-DEC-017` | Decision | Entri bebas kasir (`ADHOC`, `ADHOC_CATALOG`) **BOLEH** menghasilkan jasa tenaga medis, **dengan syarat kasir menunjuk pelaksananya saat input**. Menutup `MF-CQ-03`. | Yasmin — **menuntut perubahan pada Billing**, lihat `MF-CQ-05` | `approved` (sisi Medical Fee) | Yasmin, 20 September 2026 | Jawaban langsung owner, menyimpang dari rekomendasi. **Konsekuensi:** `BilInvoiceItem` dan kontrak charge `BIL-INTEGRATION-0.4` saat ini **tidak punya** field pelaksana sama sekali (`MF-CAP-012`), sehingga Billing MUST menambahkannya untuk kedua sumber itu. **Risiko yang diterima:** data yang berujung pada penghasilan orang bergantung pada ketelitian kasir |
| `MF-DEC-018` | Decision | Bila pelaksana tidak tercatat saat jasa dihitung — misalnya `LabOrder.ExaminerDoctorId` kosong — baris jasanya **tetap dibuat berstatus tertahan** beserta alasannya, tidak terbit dan tidak hilang. Petugas melengkapi pelaksananya, lalu jasa terbit. Menutup `MF-CQ-04`. | Yasmin | `approved` | Yasmin, 20 September 2026 | `MF-CAP-007`. **Konsekuensi yang MUST ditangani:** perlu layar yang menampilkan jasa tertahan, dan **periode jasa tidak boleh ditutup selama masih ada baris tertahan** |
| `MF-DEC-015` | Decision | **Radiologi ditunda** dari rilis pertama. Rilis pertama hanya mencakup sumber layanan yang data pelaksananya sudah siap: operasi dan tindakan klinis. Radiologi menyusul setelah modul radiologi menambahkan pencatatan pelaksana. Menutup `MF-CQ-02`. | Yasmin — **pekerjaan lintas modul, perlu owner Radiologi** | `approved` | Yasmin, 20 September 2026 | `MF-CAP-008` — nol field pelaksana pada seluruh model radiologi. **Pengganti selama ditunda:** jasa radiologi tetap dihitung manual seperti sekarang |

---

## Acceptance Criteria

Yang sudah dapat diuji dari sebelas keputusan di atas:

1. Nilai `BilInvoiceItem.DoctorShare` berasal dari perhitungan Medical Fee, dan **tidak ada**
   jalur yang membiarkan petugas mengetiknya manual (`MF-DEC-001`).
2. Satu layanan yang dikerjakan tiga orang menghasilkan **tiga** baris jasa terpisah, masing-
   masing dapat ditelusuri ke pelaksananya (`MF-DEC-002`, `MF-DEC-003`).
3. Aturan perhitungan yang diperbarui hari ini **tidak mengubah** nilai jasa periode yang sudah
   lewat (`MF-DEC-004`).
4. Nilai yang diserahkan ke Finance adalah jasa **kotor** — tanpa PPh 21, kasbon, maupun
   potongan lain (`MF-DEC-005`).
5. Tarif sharing yang dipakai dapat ditelusuri ke PKS/SK beserta masa berlakunya, tanpa
   membuka berkas di luar sistem (`MF-DEC-006`).
6. Tagihan yang direvisi sebelum periode ditutup membuat nilai jasa menyesuaikan sendiri; bila
   periode sudah ditutup, yang muncul adalah baris penyesuaian bernilai selisih dan angka lama
   tetap terbaca (`MF-DEC-007`).
7. Penyusun hasil jasa **tidak dapat** menyetujui hasil yang disusunnya sendiri (`MF-DEC-009`).
8. Jasa dihitung untuk seluruh tagihan yang sudah ditutup, termasuk yang pasien atau
   asuransinya belum membayar (`MF-DEC-010`).

Ditambah dari Closure pass:

9. Tarif sharing yang dipakai dapat ditelusuri ke satu baris kontrak HR beserta masa
   berlakunya; ketika kontraknya berakhir, tarif itu berhenti berlaku dengan sendirinya
   (`MF-DEC-012`).
10. Tarif layanan Rp 1.000.000 dengan sharing 40% menghasilkan jasa Rp 400.000, dan nilai itu
    **tidak berubah** walaupun pasien mendapat diskon promo Rp 100.000 (`MF-DEC-013`).
11. Satu tindakan yang dikerjakan operator, asisten, dan anestesi terbagi menurut persentase
    peran pada aturan — bukan menurut angka yang diketik petugas per kasus (`MF-DEC-014`).
12. Layanan laboratorium yang pemeriksanya belum diisi menghasilkan baris jasa **tertahan**
    yang terlihat di layar, bukan jasa yang hilang tanpa jejak (`MF-DEC-018`).
13. Periode jasa **tidak dapat ditutup** selama masih ada baris tertahan (`MF-DEC-018`).
14. Layanan radiologi tidak menghasilkan jasa pada rilis pertama, dan ketiadaannya terlihat
    sebagai cakupan yang memang ditunda — bukan sebagai kegagalan perhitungan (`MF-DEC-015`).

---

## Open Questions dan Blocker

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| `MF-OQ-001` | Setelah cakupan diperluas ke tenaga kesehatan non-dokter (`MF-DEC-002`), apakah nama modul "Medical Fee" masih tepat? Dan yang lebih penting: Finance hanya punya `FinDoctorPayable`, belum ada bentuk utang untuk penerima non-dokter. | Yasmin (Finance + Medical Fee) | `DESIGN` untuk Medical Fee **dan** perubahan pada `FIN-BP-001` |
Seluruh pertanyaan pembuka Scope pass sudah ditutup: ~~`MF-OQ-001`~~ (`MF-DEC-008`),
~~`MF-OQ-002`~~ (`MF-DEC-005`), ~~`MF-OQ-003`~~ (`MF-DEC-009`), ~~`MF-OQ-004`~~ (`MF-DEC-006`),
~~`MF-OQ-005`~~ (`MF-DEC-007`), ~~`MF-OQ-006`~~ (`MF-DEC-010`), ~~`MF-OQ-007`~~ (`MF-DEC-011`).

**Closure pass 20 September 2026 menutup seluruhnya.** ~~`MF-OQ-008`~~ (tidak relevan lagi —
`MF-DEC-012` justru memilih menunjuk kontrak HR, sehingga tidak ada dua sumber kebenaran),
~~`MF-OQ-009`~~ (`MF-DEC-013`), ~~`MF-OQ-010`~~ (`MF-DEC-014`), ~~`MF-OQ-011`~~ (`MF-DEC-016`),
~~`MF-CQ-01`~~ (`MF-DEC-012`), ~~`MF-CQ-02`~~ (`MF-DEC-015`), ~~`MF-CQ-03`~~ (`MF-DEC-017`),
~~`MF-CQ-04`~~ (`MF-DEC-018`).

Tidak ada pertanyaan bisnis yang tersisa. Yang tersisa adalah **konfirmasi dari modul tetangga**
atas keputusan yang sudah diambil:

| ID | Yang perlu dikonfirmasi | Owner | Memblokir |
|---|---|---|---|
| `MF-CQ-05` | `MF-DEC-017` menuntut Billing menambahkan **field pelaksana** pada entri `ADHOC` dan `ADHOC_CATALOG`. Saat ini `BilInvoiceItem` dan kontrak charge `BIL-INTEGRATION-0.4` tidak punya field itu sama sekali. | Billing Owner | `DESIGN` untuk rumpun entri kasir saja — **tidak menahan** rumpun operasi dan tindakan klinis |
| `MF-CQ-06` | `MF-DEC-012` membuat tarif sharing menunjuk `WfpContractHistory` milik HR. HR perlu mengetahui bahwa datanya kini menjadi rujukan perhitungan penghasilan, sehingga perubahan status kontrak berdampak langsung pada jasa. | HR Owner | `DESIGN` untuk `MF-SC-007` — perlu sepengetahuan, bukan persetujuan atas bentuk internal Medical Fee |
| `MF-CQ-07` | `MF-DEC-003` menuntut tindakan klinis dan laboratorium menambahkan pencatatan **tim beserta peran**, mengikuti daftar `MF-DEC-016`. Saat ini keduanya hanya menyimpan satu orang tanpa peran. | Owner Clinical + Laboratory | `DESIGN` untuk pembagian tim di luar operasi — rumpun operasi tetap dapat berjalan |
| `MF-CQ-08` | **Dibuka 20 September 2026 saat gerbang `/design-business-module` diperiksa; sebelumnya terlewat.** `MF-DEC-001` memindahkan sumber kebenaran `BilInvoiceItem.DoctorShare` dari ketikan petugas Billing menjadi hasil perhitungan Medical Fee. Keputusan itu bertanda "butuh konfirmasi owner Billing" sejak Scope pass, tetapi **tidak pernah diberi closure question**, sehingga tidak muncul di daftar yang perlu dikonfirmasi. Ini perubahan **terbesar** yang dituntut modul ini pada Billing — lebih luas dari `MF-CQ-05`, karena menyentuh seluruh layanan, bukan hanya entri bebas kasir. | Billing Owner | **`DESIGN` untuk seluruh jalur nilai jasa.** Perhitungan internal Medical Fee tetap dapat dirancang; yang tertahan adalah arah alir nilainya kembali ke Billing |

## Closure Questions lintas modul

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| `MF-CQ-01` | **`MF-DEC-005` membuka gap nyata pada blueprint Finance yang baru saja disetujui.** `FinPayment` hasil rancangan `FIN-BP-001` hanya memiliki `TotalAmount` dan alokasi ke utang — **tidak ada tempat sama sekali** untuk potongan dan tambahan per penerima per periode (PPh 21, kasbon, iuran, KSO, sitting fee). Karena Medical Fee kini menyerahkan jasa kotor, Finance MUST diperluas agar dapat menerapkan potongan itu, dan `FinPayment.TotalAmount` tidak lagi otomatis sama dengan jumlah alokasinya. | Yasmin (owner kedua modul) | **`DESIGN` untuk `EPIC FIN-09`** — sudah `POST-MVP` sehingga tidak menahan MVP Finance, tetapi MUST diselesaikan sebelum rumpun pembayaran dibangun |
| `MF-CQ-02` | **`MF-DEC-008` menggantikan `FinDoctorPayable`** dengan satu entity utang jasa tenaga medis. Berkas `FIN-BP-001` yang terdampak dan MUST diperbarui lewat amendment pass: `02-backend-architecture.md` (model, service, controller, tabel kepemilikan data), `erd/payable.md`, `erd/data-dictionary.md`, `contracts/api-contract.md` (grup Doctor Payable), `contracts/permission-audit-matrix.md` (Resource `FinanceDoctorPayable`), `contracts/state-transition-matrix.md`, dan `04-prd-to-mvp.md` (`EPIC FIN-08`). | Yasmin (owner Finance) | **`DESIGN` untuk `EPIC FIN-08`** — sudah `POST-MVP` dan belum ada satu baris kode pun, sehingga tidak ada yang perlu dibongkar. MUST ditutup sebelum rumpun utang dokter dibangun |

## Langkah berikutnya

**Tiga pass selesai 20 September 2026:** Scope pass, capability audit, dan Closure pass.
Delapan belas keputusan diambil; tujuh belas berlaku, satu (`MF-DEC-011`) digantikan.

**Tidak ada pertanyaan bisnis yang tersisa.** Seluruh open question dan closure question dari
pass sebelumnya sudah ditutup keputusan.

**Yang tersisa adalah konfirmasi dari tiga modul tetangga** (`MF-CQ-05`, `MF-CQ-06`,
`MF-CQ-07`). Ketiganya menyentuh rumpun tertentu saja dan **tidak menahan desain secara
keseluruhan**:

| Rumpun | Dapat didesain sekarang | Menunggu |
|---|:---:|---|
| Aturan tarif sharing dan perhitungan | Ya | `MF-CQ-06` hanya perlu sepengetahuan HR, bukan persetujuan bentuk |
| Jasa dari layanan operasi | Ya | — data pelaksananya sudah lengkap |
| Jasa dari tindakan klinis dan laboratorium | Sebagian | `MF-CQ-07` untuk pencatatan tim; perhitungan satu pelaksana sudah dapat dirancang |
| Jasa dari entri bebas kasir | Tidak | `MF-CQ-05` — Billing belum punya field pelaksana |
| Jasa dari radiologi | Tidak | Sengaja ditunda (`MF-DEC-015`) |
| Verifikasi, persetujuan, periode, penyerahan ke Finance | Ya | — |

**Dua amendment yang dituntut modul ini pada `FIN-BP-001`** (`MF-CQ-01` dan `MF-CQ-02` versi
lama, kini tercatat sebagai `FIN-OQ-013` dan `FIN-OQ-014`) **sudah ditutup** oleh amendment
revisi 2 pada blueprint Finance, 20 September 2026.
