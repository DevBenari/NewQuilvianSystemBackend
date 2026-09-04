# Acceptance Test Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Compatibility impact | `0.3.0`: skenario amandemen diganti skenario **addendum** sesuai `RWI-DEC-091`, dan satu skenario baru menjaga rencana asuhan **tetap** berversi |
| Status | `draft` |
| Tanggal | 2 September 2026 |

Matriks memuat **jalur gagal**, bukan hanya jalur berhasil. Dari 24 skenario di bawah, **11**
adalah jalur gagal.

---

## 1. Konteks klinis rawat inap — `CAP-012`, `INT-KEP-01`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP012-01` | Perawat berwenang membuat pengkajian untuk episode `Admitted` **tanpa** nomor antrean dan tanpa kunjungan IGD | Integration | `201`; baris tersimpan dengan `QueueId` kosong dan `InpEpisodeId` terisi |
| `VAL-KEP-01` | **Gagal:** encounter tanpa episode rawat inap | Integration | `422`; pesan menyebut pasien tidak sedang dirawat inap |
| `VAL-KEP-02` | **Gagal:** episode masih `Draft` | Integration | `422`; pesan menyebut pasien belum masuk kamar |
| `VAL-KEP-03` | **Gagal:** episode `Closed` | Integration | `422`; tidak ada baris baru tersimpan |
| `VAL-KEP-04` | **Gagal:** encounter rawat jalan tanpa antrean dan tanpa episode | Integration | `400`; **membuktikan perilaku poliklinik tidak berubah** — penjaga `RWI-DEC-070` |
| `INT-KEP-01` | Pengkajian IGD lewat jalur lamanya tetap berhasil setelah cabang rawat inap ditambahkan | Regression | `201`; **penjaga regresi wajib** menurut `RWI-DEC-051` |

---

## 2. Pengkajian awal dan pengkajian ulang — `CAP-012`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| PRD 16.2 aturan 3 | Pengkajian ulang tersimpan sebagai baris **terpisah**; pengkajian awal tidak berubah | Integration | Dua baris; nilai baris pertama sama persis sebelum dan sesudah |
| `AC-CAP012-02` | Penilaian nyeri kedua tidak mengubah nilai pertama, dan lini masa menampilkan keduanya | Integration | Dua baris terurut waktu; nilai nyeri pertama utuh |
| `VAL-KEP-11` | **Gagal:** membuat pengkajian awal kedua pada episode yang sama | Integration | `409`; pesan mengarahkan ke pengkajian ulang |
| `VAL-KEP-08` | **Gagal:** menyelesaikan pengkajian dengan isian wajib kosong | Integration | `400`; pesan menyebut bagian yang kosong satu per satu |
| `AC-CAP012-03` | Pengkajian `Completed` tampil pada census/ruang kerja **tanpa** menambah status episode | Integration | Status episode tetap salah satu dari lima nilai `RWI-DEC-009` |
| `AC-CAP012-05` | Koreksi pengkajian final mempertahankan isi aslinya | Integration | Status **tetap** `Completed`; isi asli terbaca utuh dan koreksinya muncul sebagai addendum bernomor — lihat bagian 8 |
| `VAL-KEP-12` | **Gagal:** amandemen tanpa alasan | Integration | `400`; tidak ada versi baru terbentuk |

---

## 3. Tenggat dan keterlambatan — `CAP-012` aturan 11

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP012-04` | Keterlambatan dinilai memakai kebijakan yang **aktif saat pengkajian dibuat** | Integration | Mengubah kebijakan tidak mengubah penilaian pengkajian yang lalu |
| `VAL-KEP-17` | Master kebijakan **kosong**: pengkajian tetap dapat dibuat dan diselesaikan | Integration | `201` lalu `Completed`; `DueAt` kosong; nol baris terlambat |
| `VAL-KEP-18` | Pengkajian lewat tenggat muncul di daftar pantau **tanpa** menahan tindakan apa pun | Integration | Baris muncul; pembuatan tindakan tetap `201` |

---

## 4. Rencana asuhan keperawatan — `CAP-013`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP013-01` | Masalah dari pengkajian dapat dikaitkan ke butir rencana asuhan | Integration | Butir tersimpan merujuk pengkajian asalnya |
| `AC-CAP013-02` | Memperbarui butir menyimpan versi sebelumnya **beserta penulis dan waktu aslinya** | Integration | Baris revisi memuat `OriginalAuthorEmployeeId` dan `OriginalAuthoredAt` yang **tidak** berubah |
| `VAL-KEP-16` | **Gagal:** menyatakan butir tercapai tanpa satu pun evaluasi | Integration | `400`; status butir tetap `Active` |
| `CAP-013` aturan 6 | Menutup butir **tidak** menghapus tindakan dan evaluasi sebelumnya | Integration | Tindakan yang merujuk butir itu tetap ada; rujukannya menjadi kosong, barisnya tidak hilang |
| `AC-CAP013-03` | Setelah episode ditutup, seluruh riwayat asuhan tetap terbaca hanya-baca | Integration | `GET` berhasil; setiap `POST`/`PUT` dijawab `422` |

---

## 5. Tindakan keperawatan — `CAP-014`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP014-01` | Permintaan diulang dengan kunci idempotency sama menghasilkan **satu** baris | Integration | Panggilan kedua `200` beserta Id yang sama; jumlah baris tetap satu |
| `AC-CAP014-01` | Dua permintaan bersamaan dengan kunci sama | Integration terhadap **PostgreSQL sungguhan** | Satu berhasil, satu memakai baris yang sama. **Provider InMemory tidak dapat membuktikan unique index parsial** |
| `AC-CAP014-02` | Pengiriman tagihan gagal: catatan klinis **tetap tersimpan** | Integration | Catatan `Recorded`; status pengiriman `Failed`; keduanya terbaca |
| `AC-CAP014-03` | **Gagal:** pengguna yang bukan penulis dan bukan kepala ruangan mengubah catatan final | Integration | `403`; isi catatan tidak berubah |
| `VAL-KEP-13` | **Gagal:** waktu tindakan di masa depan | Unit | `400` |
| `VAL-KEP-14` | **Gagal:** waktu tindakan sebelum pasien masuk kamar | Integration | `400` |
| `CAP-014` aturan 3 | Tindakan mendadak tanpa rujukan rencana tetap dapat dicatat | Integration | `201` dengan rujukan rencana kosong |

---

## 6. Gizi — `CAP-027`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `VAL-KEP-10` | Skrining berisiko tinggi memunculkan **saran**, bukan penolakan | Integration | Pengkajian tetap `Completed`; saran muncul pada jawaban |
| `AC-CAP027-01` | Hasil skrining tersimpan pada pengkajian tanpa membuat konteks pasien kedua | Integration | Nilai risiko gizi terbaca dari pengkajian; nol tabel gizi baru |

> `AC-CAP027-02` **belum dapat diuji**: ia menuntut ruang kerja profesional gizi, dan modul Gizi
> berstatus `PLANNED`.

---

## 7. Yang **belum dapat diuji**, beserta sebabnya

| Butir | Kenapa belum | Kapan dapat diuji |
| --- | --- | --- |
| Seluruh `CAP-016` pemakaian alat | **`DEFERRED`** lewat `RWI-DEC-089` — dikeluarkan dari scope rilis pertama secara tertulis | Setelah modul persediaan/aset ada dan `RWI-OQ-048` dibuka ulang — `RWI-AC-171` |
| `AC-CAP027-02` kewenangan ahli gizi | Modul Gizi `PLANNED` | Setelah modul Gizi berdiri |
| Nilai batas waktu klinis | `RWI-RULE-021` menunggu pemilik klinis | Yang **dapat** diuji sekarang adalah mekanismenya, dan itu tercakup bagian 3 |
| Katalog SDKI/SLKI/SIKI | `OPEN DECISION` pada `02-backend-architecture.md` bagian 4.2 | Setelah pemakaian SDKI dinyatakan |

---

## 8. Penjaga batas sub-modul

Satu skenario yang **tidak** diturunkan dari requirement mana pun, melainkan dari `RWI-DEC-081`.
Ia menutup coverage gap yang ditemukan saat resync roadmap `episode-rawat-inap`.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-081` | Tidak ada satu pun tabel berawalan `Inp` yang menyimpan pengkajian, asuhan, tindakan keperawatan, CPPT, SOAP, resep, atau tindakan dokter | Architecture test | Pemindaian `ApplicationDbContext` menemukan **nol** entity `Inp*` bernama demikian |

> Tanpa test ini, larangan `RWI-DEC-081` hanya dijaga dokumen. Test ini membuatnya dijaga mesin.

---

## 8. Skenario koreksi dokumen — **baru pada `0.3.0`**

Diturunkan dari `RWI-DEC-091` beserta acceptance criteria `RWI-AC-175` s.d. `RWI-AC-177` pada decision log.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `FR-KEP-008`, `RWI-AC-175` | Ns. Sari menyelesaikan pengkajian awal, lalu menyadari skor nyerinya salah dan menambah koreksi beralasan | Integration | Isi pengkajian asli **tidak berubah sedikit pun**; koreksi muncul sebagai addendum bernomor 1 beserta alasan, penulis, dan waktu; status pengkajian **tetap** `Completed` |
| `FR-KEP-008`, `RWI-AC-175` | Pengkajian yang sudah `Completed` terdaftar pada mesin keutuhan dengan jenis `Assessment` | Integration | Baris keutuhan ada, dan **tidak ada nilai enum baru** ditambahkan ke `ClinicalDocumentKind` |
| `FR-KEP-009` | Percobaan menyunting langsung isi pengkajian yang sudah `Completed` | Integration — **jalur gagal** | Ditolak. Bila `RWI-OQ-051` belum dikerjakan, skenario ini **akan lolos padahal seharusnya gagal** — lihat catatan di bawah |
| `FR-KEP-008` | Percobaan menambah addendum pada pengkajian yang masih `Draft` | Integration — **jalur gagal** | Ditolak, dengan arahan membetulkan langsung pada isinya — `RWI-FACT-013` |
| `FR-KEP-022`, `RWI-AC-176` | Catatan tindakan yang sudah `Finalized` dikoreksi lewat addendum | Integration | Isi asli utuh; status tetap `Finalized`; jenis dokumen `Procedure` |
| `FR-KEP-014`, `RWI-AC-177` | Butir rencana asuhan diperbarui karena keadaan pasien membaik | Integration | Menghasilkan **versi baru**, **bukan** addendum; versi sebelumnya tetap menyimpan **penulis dan waktu aslinya**, bukan penulis yang mengubah |
| `RWI-DEC-091` | Satu episode memuat koreksi perawat dan koreksi dokter pada catatan terpadu yang sama | Integration | Keduanya tampil dalam **satu bentuk yang sama**, yaitu addendum bernomor — bukan satu versi dan satu addendum |

> **Peringatan yang menentukan urutan pengujian.** Skenario `FR-KEP-009` di atas **tidak dapat membuktikan
> apa pun** sebelum `RWI-OQ-051` dikerjakan. `EnsureMutableAsync` membiarkan lewat jenis dokumen yang belum
> ditegakkan, sehingga penyuntingan dokumen final **akan berhasil** dan test akan lulus dengan alasan yang
> salah. Selama `Assessment` dan `Procedure` belum masuk daftar jenis yang ditegakkan, skenario ini wajib
> ditandai **belum dapat diuji**, bukan ditandai lulus.
