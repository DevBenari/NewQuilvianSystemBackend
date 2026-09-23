# Acceptance Test Matrix — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` |
| Contract version | **`0.5.0`** — bagian 9, `draft` |
| `last_changed_in` | **`0.5.0`** — bagian 9: `AC-KEP-051` s.d. `135`. Sebelumnya `0.4.0` — bagian 5A |
| Compatibility impact | `0.3.0`: skenario amandemen diganti skenario **addendum** sesuai `RWI-DEC-091`, dan satu skenario baru menjaga rencana asuhan **tetap** berversi |
| Status | **`draft`** untuk `0.5.0`. `0.4.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
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


## 5A. Gelombang 1A — jalur hapus dan kewenangan perawat — `0.4.0` ★ baru

Menyerap `RWI-DEC-098` dan `RWI-DEC-100`. Seluruh baris di bawah ini belum pernah ada.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-040` | `DELETE /patient-vital-signs/{id}` dipanggil | Integrasi | **`404`**, bukan `403`. Route tidak ada |
| `AC-KEP-041` | Tanda vital yang sudah tercatat tetap terbaca pada lini masa setelah perubahan | Integrasi | Deret waktu utuh; nol baris hilang |
| `AC-KEP-042` | Penanda pemberitahuan dokter tidak dapat dimatikan lewat penghapusan | Integrasi | Tidak ada jalur yang mematikan `NeedDoctorNotification` tanpa alasan tersimpan |
| `AC-KEP-043` | Regresi Rawat Jalan dan IGD: pencatatan tanda vital di luar rawat inap tidak ikut berubah | Integrasi | Perilaku sama persis seperti sebelum perubahan. **Wajib**, karena controller dipakai bersama |
| `AC-KEP-044` | Perawat menulis pengkajian untuk pasien di **unitnya**, bukan pasien penanggung jawabnya | Integrasi | **`200`**. Ini jalur normal dinas malam, dan wajib dibuktikan berhasil |
| `AC-KEP-045` | Perawat menulis untuk pasien di unit lain | Integrasi | `403` |
| `AC-KEP-046` | Pengguna tanpa `ApplicationUser.EmployeeId` menulis | Integrasi | `403` |
| `AC-KEP-047` | Perawat mengirim `nurseId` milik perawat lain | Integrasi | `403`, dan nol baris tersimpan atas nama pihak lain |
| `AC-KEP-048` | Perawat penanggung jawab berganti, perawat lama tetap dapat menulis selama masih di unit yang sama | Integrasi | `200`. Membuktikan `InpNurseAssignment` memang bukan gerbang |
| `AC-KEP-049` | Pasien dipindahkan ke unit lain, perawat unit lama menulis | Integrasi | `403`. Membuktikan kewenangan mengikuti unit **episode**, bukan salinan unit pada baris penugasan |
| `AC-KEP-050` | Seluruh skenario negatif dijalankan memakai peran nyata, bukan SuperAdmin | Integrasi | Peran yang dipakai tercatat pada laporan task |

### 5A.1 Satu butir yang **belum dapat** diuji

| Yang belum | Sebabnya |
| --- | --- |
| Pembatalan tanda vital final ditolak dan diarahkan ke addendum | `ClinicalDocumentKind.VitalSign` belum termasuk jenis yang ditegakkan mesin keutuhan dokumen. Selama itu belum berubah, tidak ada keadaan "final" yang dapat diuji. Dilacak `V2-UNK-01` |

Butir itu **tidak** memblokir `Gelombang 1A`, tetapi ia memblokir pernyataan bahwa jalur pengganti
sudah lengkap. Menuliskannya lulus sekarang berarti mengaku menguji sesuatu yang tidak ada.

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

---

## 9. Penyelarasan `PRD-RWI-V2-001` — `0.5.0` ★ 15 September 2026

**Status `draft`.** Seluruh skenario negatif dijalankan memakai peran nyata, bukan SuperAdmin (`AC-KEP-050` berlaku).
Nama pasien dan petugas di bawah adalah contoh, bukan data asli.

### 9.1 Pengkajian Pasien dan konfigurasi klinis — `CAP-012`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-051`, `RWI-AC-196` | Andi menyimpan draft "Morse Dewasa v2", lalu Andi sendiri menekan Sahkan | Integrasi — **jalur gagal** | `403`; versi tetap `Draft` |
| `AC-KEP-052`, `RWI-AC-196` | Ns. Wati mengesahkan versi yang dibuat Andi | Integrasi | `Approved`; versi sah lama `Retired` dalam transaksi yang sama; hanya satu `Approved` |
| `AC-KEP-053` | Menyimpan pita Rendah `[0,25)`, Sedang `[30,45)`, Tinggi `[45,null)` | Unit — **jalur gagal** | `400` celah 25–30 |
| `AC-KEP-054`, `RWI-AC-197` | Pemindaian source backend dan frontend untuk angka batas atau skor butir risiko jatuh pada perhitungan rawat inap | Architecture test | Nol temuan |
| `AC-KEP-055`, `RWI-AC-196` | Produksi tanpa versi sah: perawat menyelesaikan Resiko Jatuh | Integrasi — **jalur gagal** | `422`; dokumen tetap konsep dan tersimpan |
| `AC-KEP-056` | Lingkungan uji dengan `AllowDraftVersionsForTesting = true` | Integrasi | Dokumen dapat diselesaikan; hasil menyimpan versi draft dan `resolve` menandai lingkungan uji |
| `AC-KEP-057` | Budi 67 tahun, skor Morse 50 pada versi v2 | Integrasi | `TotalScore = 50`, `BandCode = HIGH`, `IsAlertBand = true`, `FallRiskStatus = HighRisk`; versi dan hash tersimpan |
| `AC-KEP-058` | Setelah dokumen selesai, versi v3 disahkan | Integrasi | Hasil Budi tetap v2 skor 50; tidak dihitung ulang |
| `AC-KEP-059`, `RWI-AC-207` | Membuka formulir Kajian Umum | E2E / komponen | Tepat delapan bagian urutan `RWI-DEC-141`; tidak ada bagian Psikososial, Alat Bantu, Catatan Relevan tersendiri |
| `AC-KEP-060`, `RWI-AC-208` | Mencari isian kateter dan kursi roda pada Kajian Umum | Komponen | Kateter hanya di Eliminasi; kursi roda hanya di Ketergantungan |
| `AC-KEP-061`, `RWI-AC-209` | Pemindaian source untuk daftar isian wajib Kajian Umum yang tetap | Architecture test | Nol temuan; isian wajib hanya dari `requiredItemCodes` |
| `AC-KEP-062`, `INV-KEP-04` | Kajian Umum memilih tanda vital 08.00 | Integrasi | `VitalSignId` terisi; kolom tanda vital `TrxPatientAssessment` kosong; layar menampilkan angka dari baris tanda vital |
| `AC-KEP-063` | Kajian Umum memilih tanda vital milik pasien lain | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-064`, `INV-KEP-04` | Mencatat tanda vital rawat inap berisi skala nyeri | Integrasi — **jalur gagal** | `400`; nol baris |
| `AC-KEP-065` | Regresi: tanda vital poliklinik dan IGD berisi skala nyeri | Integrasi | Diterima seperti sebelum perubahan |
| `AC-KEP-066` | Monitoring Nyeri diselesaikan dengan keadaan nyeri belum dipilih | Integrasi — **jalur gagal** | `422` |
| `AC-KEP-067`, `BR-RWI-007` | Monitoring Nyeri "tidak dapat dinilai" | Integrasi | Tersimpan `UnableToAssess`; **tidak** terbaca "tidak nyeri" di ringkasan |
| `AC-KEP-068` | Regresi: pengkajian risiko jatuh poliklinik mengirim dua centang lama | Integrasi | Perhitungan poliklinik tidak berubah |

### 9.2 Progres Pengkajian Pasien — `RWI-DEC-119`, `RWI-DEC-120`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-069` | Budi: Kajian Umum selesai, Resiko Jatuh selesai kategori Tinggi, Monitoring Nyeri draft, Edukasi dan Perencanaan Pulang kosong | Integrasi | `Completed`, `Completed`, `NeedsAttention`, `NotFilled`, `NotFilled`; `CompletedCount = 2`; `ProgressPercent = 40`; Tinggi **tidak** mengubah `State` |
| `AC-KEP-070` | Bagian yang punya dokumen selesai **dan** draft baru | Integrasi | `Completed` |
| `AC-KEP-071` | Pengawasan Harian dicatat 06.00, Evaluasi Awal belum ada | Integrasi | Keduanya tidak menambah `CompletedCount`; `DailyMonitoringLastRecordedAt = 06.00`; status Evaluasi Awal "milik MPP" |
| `AC-KEP-072`, `UI-AC-KEP-008` | Endpoint progres gagal | E2E | Layar "Gagal memuat progres pengkajian" dan Coba Lagi; **nol** ikon ○ |
| `AC-KEP-073` | Dokumen yang dibatalkan satu-satunya pada bagian itu | Integrasi | `NotFilled` |

### 9.3 Evaluasi Awal MPP

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-074`, `RWI-AC-191` | Ns. Dewi MPP Melati menulis Evaluasi Awal pasien Melati | Integrasi | `200`; konsep tersimpan |
| `AC-KEP-075`, `RWI-AC-191` | Dewi menulis untuk pasien Anggrek | Integrasi — **jalur gagal** | `403` |
| `AC-KEP-076` | Perawat pelaksana tanpa hak MPP di unit yang sama menulis | Integrasi — **jalur gagal** | `403` |
| `AC-KEP-077` | Membuat Evaluasi Awal kedua pada episode yang sama | Integrasi — **jalur gagal** | `409` |
| `AC-KEP-078` | Pasien dipindah unit saat Dewi menyimpan konsep | Integrasi — **jalur gagal** | `403` pada simpan berikutnya |
| `AC-KEP-079` | Perawat membuka Evaluasi Awal | E2E | Hanya baca; tidak ada tombol ubah |

### 9.4 Pengawasan Harian — cairan, GDS, observasi, shift

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-080`, `RWI-AC-231` | Hari Budi: infus 1.500, oral 600, obat 200, darah 200; urin 1.800, drain 150 | Integrasi | Intake 2.500, output 1.950, balance +550 ml; per shift sesuai jam shift bawaan |
| `AC-KEP-081`, `RWI-AC-229` | Entri sumber Obat tanpa dosis MAR | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-082`, `RWI-AC-229` | Entri kedua untuk dosis Ceftriaxone 08.00 yang sama | Integrasi — **jalur gagal** | `409`; total tidak berubah |
| `AC-KEP-083`, `RWI-AC-230` | Menautkan dosis `Held` | Integrasi — **jalur gagal** | `409` |
| `AC-KEP-084` | Dua permintaan simpan paralel untuk dosis yang sama | Integrasi — konkurensi | Tepat satu baris aktif; unique parsial menolak yang kedua |
| `AC-KEP-085`, `RWI-AC-231` | Koreksi infus 500 → 450 ml | Integrasi | Revisi menyimpan 500; total turun 50; `RevisionNumber = 1` |
| `AC-KEP-086` | Koreksi dengan `ExpectedRevisionNumber` basi | Integrasi — **jalur gagal** | `409` |
| `AC-KEP-087`, `RWI-AC-206` | Catatan naratif perawat berisi "intake 300 ml" | Integrasi | Total cairan tidak berubah |
| `AC-KEP-088` | Dosis tertaut dikoreksi menjadi `Held` | Integrasi | Entri intake tetap aktif dan `DoseCorrectionFlaggedAt` terisi — usulan `G-26` |
| `AC-KEP-089` | GDS tanpa satuan | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-090` | GDS 280 dengan satuan mmol/L | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-091` | Unit tanpa baris shift dan bawaan kosong | Integrasi | Total per shift tidak ada, `ShiftConfigurationMissing = true`; total 24 jam tampil |
| `AC-KEP-092` | Shift Pagi 07–14, Siang 14–20, Malam 21–07 | Integrasi — **jalur gagal** | `400` celah |
| `AC-KEP-093`, `RWI-DEC-100` | Konfigurasi shift tidak mempengaruhi siapa yang boleh menulis | Integrasi | Perawat unit yang tidak terdaftar pada jadwal shift mana pun menulis pukul 03.00 → `200`; jam shift hanya dipakai menghitung total |

### 9.5 MAR — `CAP-023-MAR`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-094`, `AC-MVP-025` | Ceftriaxone `q12h` berjadwal 08.00/20.00; MAR dibuka | Integrasi | Dua dosis `Due` per hari; membuka ulang tidak menambah baris |
| `AC-KEP-095` | Hosted service dan pembukaan MAR berjalan bersamaan | Integrasi — konkurensi | Tidak ada dosis ganda |
| `AC-KEP-096` | Frekuensi `q6h` tanpa jadwal | Integrasi | Nol dosis; pesan jadwal belum dikonfigurasi; pemberian tanpa jadwal beralasan diterima |
| `AC-KEP-097`, `AC-MVP-026` | Mencatat `Administered` 1 g IV 08.05 | Integrasi | `Administered`; pencatat dari akun login, bukan dari request |
| `AC-KEP-098`, `AC-MVP-026` | Tombol simpan ditekan dua kali dengan `Idempotency-Key` sama | Integrasi | Satu pemberian |
| `AC-KEP-099`, `AC-MVP-027` | `Held` tanpa alasan | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-100` | Dosis 2 g padahal resep 1 g tanpa catatan penyimpangan | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-101`, `AC-MVP-028` | Insulin high-alert dicatat Ns. Siti | Integrasi | `Due` + `Pending` |
| `AC-KEP-102`, `AC-MVP-028` | Ns. Siti mengonfirmasi cek ganda catatannya sendiri | Integrasi — **jalur gagal** | `403` |
| `AC-KEP-103`, `AC-MVP-028` | Ns. Rina mengonfirmasi | Integrasi | `Administered` + `Confirmed`; pemeriksa Rina |
| `AC-KEP-104` | Ns. Rina menolak tanpa catatan | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-105`, `RWI-DEC-121` | Dokter menghentikan butir 14.00 | Integrasi | Dosis 20.00 `Cancelled` "resep dihentikan"; 08.00 `Administered` tetap |
| `AC-KEP-106` | Mencatat dosis butir yang sudah dihentikan | Integrasi — **jalur gagal** | `409` |
| `AC-KEP-107`, `RWI-DEC-116` (c) | Kepala ruangan mengoreksi `Administered` → `Refused` beralasan | Integrasi | Revisi menyimpan `Administered`; baris berlaku `Refused` |
| `AC-KEP-108` | Perawat pelaksana tanpa `Update` mengoreksi | Integrasi — **jalur gagal** | `403` |
| `AC-KEP-109`, `FR-MVP-KEP-013` | PRN parasetamol tanpa indikasi | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-110` | PRN pada butir bukan `IsAsNeeded` | Integrasi — **jalur gagal** | `409` |
| `AC-KEP-111`, `AC-MVP-029` | Dugaan reaksi obat dari dosis 08.00 | Integrasi | Satu alergi `Suspected` tertaut dosis; baris MAR tidak berubah; tampil pada peringatan alergi aktif |
| `AC-KEP-112` | Dosis `Due` lewat `MissedAfterMinutes` | Integrasi | Penanda lewat waktu; status tetap `Due` |
| `AC-KEP-113`, `INT-KEP-15` | Episode ditutup 13.00 dengan dosis `Due` 08.00 belum dicatat dan 20.00 | Integrasi | 20.00 `Cancelled` "perawatan ditutup"; 08.00 tetap `Due` hanya-baca; penutupan tidak tertahan |
| `AC-KEP-114` | Arsitektur: tabel MAR hanya di `PharmacyManagement` | Architecture test | Nol entity MAR di `ClinicalManagement` atau `InPatientManagement` |

### 9.6 Pelaksanaan sliding scale

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-115`, `RWI-AC-219`, `RWI-AC-227` | Order Budi v2 aktif; perawat mengetik GDS 280 mg/dL 11.00, rentang `[250,300)` 3 unit | Integrasi | Satu `CliBloodGlucoseReading`, satu dosis MAR 3 unit, satu pelaksanaan merujuk GDS itu |
| `AC-KEP-116`, `RWI-AC-220` | Kiriman ulang dengan kunci sama | Integrasi | Pelaksanaan, GDS, dan dosis tetap satu; riwayat MAR menampilkan insulin 11.00 sekali |
| `AC-KEP-117`, `RWI-AC-219` | Tanpa order aktif | Integrasi — **jalur gagal** | `409`, `VAL-KEP-27`; nol GDS dan nol dosis |
| `AC-KEP-118`, `RWI-AC-226` | Order dihentikan 10.00, pelaksanaan 11.00 | Integrasi — **jalur gagal** | `409`, `VAL-KEP-27` |
| `AC-KEP-119`, `RWI-AC-228` | Memilih id hasil GDS laboratorium 190 | Integrasi — **jalur gagal** | `422`, `VAL-KEP-28` |
| `AC-KEP-120` | GDS 15,6 mmol/L terhadap protokol mg/dL | Integrasi — **jalur gagal** | `409`; tidak ada konversi |
| `AC-KEP-121` | Dosis aktual 2 unit, hitungan 3, tanpa alasan | Integrasi — **jalur gagal** | `400` |
| `AC-KEP-122` | Dokter menyesuaikan order saat perawat di layar | Integrasi — **jalur gagal** | `409` versi basi; perawat melihat dosis baru |
| `AC-KEP-123` | Galat pada langkah penulisan dosis | Integrasi — kegagalan | Seluruh transaksi batal; GDS pun tidak tersimpan |
| `AC-KEP-124` | GDS 820 dikoreksi 280 setelah pelaksanaan | Integrasi | Pelaksanaan tetap menyimpan salinan 820 dan `ReadingCorrectedAfterExecution = true` |
| `AC-KEP-125`, `RWI-AC-224` | Template v3 disahkan setelah pelaksanaan | Integrasi | Pelaksanaan lama tetap merujuk versi order lama |
| `AC-KEP-126` | Insulin high-alert | Integrasi | Dosis `Pending`; pelaksanaan `Recorded` |
| `AC-KEP-127` | Mencatat insulin sliding scale lewat `/record` | Integrasi — **jalur gagal** | `409`, `VAL-KEP-29f` |
| `AC-KEP-128`, `RWI-AC-225` | Arsitektur: tabel sliding scale | Architecture test | Nol entity sliding scale di `ClinicalManagement` atau `InPatientManagement` |

### 9.7 Menu, batas rilis, dan hak lihat

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-KEP-129`, `RWI-AC-221` | Pemindaian menu, entity, dan route | Architecture test | Nol menu, tabel, atau endpoint handover shift dan transfusi |
| `AC-KEP-130`, `RWI-DEC-108` | Menu Gizi, Bank Darah, Hemodialisa, Rehab Medik, Pemesanan Ruangan Bedah, Pemakaian Alat | E2E | "Integrasi belum tersedia" tanpa data tiruan dan tanpa permintaan jaringan ke modul itu |
| `AC-KEP-131`, `RWI-AC-198` | Ns. Rudi tanpa `PatientBillingSummary : Read` membuka Tagihan Pasien | E2E + integrasi | Layar "Anda tidak punya akses"; permintaan langsung `403` |
| `AC-KEP-132`, `RWI-AC-198` | Kontrak Billing belum ada | E2E | "Integrasi belum tersedia"; bukan angka nol |
| `AC-KEP-133`, `RWI-AC-205` | Menu SOAP dan Catatan Keperawatan | E2E | Masing-masing hanya menampilkan jenisnya; Catatan Terintegrasi menampilkan keduanya |
| `AC-KEP-134`, `RWI-AC-193` | Rilis `KEP-V2-2` | Pemeriksaan rilis | Rekonsiliasi dan MAR hadir bersama; tidak ada menu rekonsiliasi "belum tersedia" |
| `AC-KEP-135`, `RWI-DEC-100` | Dokter membuka MAR dan Pengawasan Harian | E2E | Hanya baca |

### 9.8 Yang belum dapat diuji pada `0.5.0`

| Skenario | Sebab | Diuji setelah |
| --- | --- | --- |
| Addendum Evaluasi Awal; penguncian konsep Evaluasi Awal saat penutupan | `INT-KEP-12` menunggu persetujuan Yoga Aji Pratama | Jenis dokumen `14` tersedia |
| Isi ringkasan Tagihan Pasien | `INT-KEP-14` menunggu kontrak Billing | Kontrak Billing disetujui |
| Pengesahan instrumen di produksi dengan isi klinis | Pemilik klinis belum ditunjuk — `RWI-OQ-056`, `RWI-OQ-057` | Pengesahan pemilik klinis |
| Cek ganda pada daftar high-alert yang sebenarnya | Daftar high-alert gerbang produksi `RWI-DEC-116` | Daftar disahkan |
| Jam jadwal dan jendela lewat waktu sebenarnya | Gate `G-12` | Farmasi/klinis mengisi |
