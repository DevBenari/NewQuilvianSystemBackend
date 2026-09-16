# PRD ke MVP — Sub-modul `keperawatan` (Rawat Inap)

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement` |
| Sub-modul | `keperawatan`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | **`0.5.0`** untuk bagian 22; `0.1.0` untuk bagian 1 s.d. 20; `0.4.0` untuk bagian 21 |
| Revision artefak | **`0.4`** — bagian 22 penyelarasan `PRD-RWI-V2-001` |
| Status | **`draft`** untuk `0.4` — **belum disetujui manusia**. Status bagian 1 s.d. 21 mengikuti `blueprint-manifest.md` sub-modul |
| Repository target | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Baseline requirement | `PRD-RWI-FINAL-001` v1.0.0 bagian 16, 17, 20, 23.1, 30.3 |
| Ditulis paling akhir | Ya — menurunkan dari arsitektur dan kelima kontrak |

---

## 2. Ringkasan eksekutif

Sub-modul ini memberi perawat rawat inap **satu tempat untuk mencatat pekerjaannya**, dan memberi
kepala ruangan **cara melihat pengkajian mana yang belum dikerjakan**.

Hari ini keduanya tidak ada. Mesin pengkajian sudah berdiri dan sudah dipakai poliklinik dan IGD,
tetapi pintunya tertutup bagi pasien rawat inap — bukan karena keputusan bisnis, melainkan karena
satu pemeriksaan validasi yang belum mengenal episode rawat inap.

**Yang membuat sub-modul ini murah:** ia tidak membangun tabel baru milik Rawat Inap. Ia membuka
pintu yang sudah ada, meminta tiga tabel kepada modul yang memang pemiliknya, dan menyediakan ruang
kerjanya.

---

## 3. Masalah produk

| Masalah | Akibatnya hari ini |
| --- | --- |
| Pengkajian keperawatan tidak dapat dibuat untuk pasien rawat inap | Perawat menulis di kertas. Rekam medis elektronik tidak lengkap |
| Tidak ada rencana asuhan keperawatan di mana pun | Asuhan tidak dapat dievaluasi, dan tidak ada dasar tertulis bagi tindakan |
| Tindakan perawat tidak punya tempat | `TrxPatientProcedure` mewajibkan konsultasi dan dokter, sehingga tindakan perawat tidak muat |
| Kepala ruangan tidak dapat melihat kepatuhan pengkajian | Daftar pantau ketiga `RWI-RULE-023` tercatat sebagai gap sejak `BE-RWI-029` |

---

## 4. Visi produk

Perawat membuka pasiennya dari census, mengisi pengkajian tanpa nomor antrean, menetapkan masalah
keperawatan, mencatat tindakan yang benar-benar dilakukan, dan melihat perkembangan nyeri serta
risiko jatuh dari hari ke hari — semuanya di dalam konteks episode yang sudah dimiliki
`episode-rawat-inap`, tanpa satu tabel tandingan pun.

---

## 5. Batas MVP

| Batas | Isinya |
| --- | --- |
| **Titik mulai** | Pasien sudah dikonfirmasi tiba di kamar — episode berstatus `Admitted` |
| **Titik akhir** | Pengkajian awal dan ulang tercatat, rencana asuhan berjalan beserta evaluasinya, tindakan tercatat, dan kepala ruangan dapat melihat kepatuhan pengkajian |
| **Di luar batas** | Pemakaian alat, asuhan gizi ujung ke ujung, katalog SDKI, dan seluruh dokumentasi dokter |

### 5.1 Pelaku sasaran

| Pelaku | Yang dikerjakannya |
| --- | --- |
| Perawat pelaksana | Mengisi pengkajian, menyusun asuhan, mencatat tindakan |
| Kepala ruangan | Semua di atas, ditambah amandemen catatan final dan membaca daftar pantau |
| DPJP | **Membaca saja** |
| Ahli gizi | Membaca hasil skrining gizi |

---

## 6. Kemampuan `MUST HAVE`

| Kemampuan | ID | Asal | Epic |
| --- | --- | --- | --- |
| Pengkajian awal dan pengkajian ulang keperawatan | `CAP-012` | `PRD-RWI-FINAL-001` bagian 16 | `EPIC KEP-01`, `EPIC KEP-02` |
| Rencana asuhan keperawatan | `CAP-013` | Bagian 17 | `EPIC KEP-03` |
| Catatan dan tindakan keperawatan | `CAP-014` | Bagian 17 | `EPIC KEP-04` |

Tiga kemampuan `MUST HAVE`. Kepemilikan datanya sudah tegas — `RWI-DEC-081`, PRD 23.1.

---

## 7. Prasyarat yang menahan seluruh MVP

| Prasyarat | Pemilik | Keadaan |
| --- | --- | --- |
| **`INT-KEP-01`** — cabang episode pada validasi pengkajian | `ClinicalManagement`, Muhammad Hamzah | Disetujui `RWI-DEC-062`; **belum dikerjakan** |

Ini **satu-satunya** penghalang teknis, dan bentuknya sudah diketahui persis: satu cabang tambahan
pada `ValidateCreateWithoutQueueAsync`, nol kolom baru. Selama ia belum ada, tidak satu pun dari
tiga kemampuan `MUST HAVE` dapat dipakai pasien rawat inap.

---

## 8. Kemampuan yang ditunda

Setiap baris menyebut **alasan bersebab** dan **pengganti selama MVP berjalan**.

| Kemampuan | ID | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Asuhan gizi ujung ke ujung | `CAP-027` | Modul Gizi berstatus `PLANNED`; PRD 23.1 menaruh Nutrition Assessment/Care di sana, dan sub-modul ini dilarang membuat tabel tandingan | **Skrining gizi tetap berjalan penuh** — kolom `NutritionRiskStatus` dan `NutritionRiskScore` sudah ada pada pengkajian, dan `VAL-KEP-10` memunculkan saran rujukan. Yang belum ada hanya rujukan terkirimnya |
| Katalog terminologi SDKI/SLKI/SIKI | bagian `CAP-013` | PRD 17 aturan 3 mensyaratkannya **hanya bila** rumah sakit memakainya, dan itu belum dinyatakan | Masalah keperawatan ditulis sebagai teks pada `ProblemStatement`. Struktur rencana, tujuan, evaluasi, dan riwayat versinya **tetap lengkap** |
| Nilai batas waktu pengkajian | bagian `CAP-012` | `RWI-RULE-021` menunggu pemilik klinis | **Mekanismenya tetap dibangun.** Master kosong berarti tidak ada yang dinyatakan terlambat; pencatatan berjalan penuh — `VAL-KEP-17` |

---

## 9. Alur bisnis target

`FLOW-KEP-MVP-001`, diturunkan dari [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md):

```text
Pasien Admitted → perawat buka ruang kerja → pengkajian awal → Completed
  → tetapkan masalah keperawatan → susun tujuan dan rencana
  → lakukan tindakan lalu catat → pengkajian ulang harian
  → evaluasi → perbarui rencana → rencana pemulangan
```

---

## 10. Epic dan functional requirement

Setiap functional requirement dapat diuji dan punya disposisi.

### `EPIC KEP-01` — Pintu masuk pengkajian rawat inap dibuka

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-001` | Pengkajian dapat dibuat bagi encounter yang punya episode `Admitted`, **tanpa** nomor antrean dan tanpa kunjungan IGD | **EXTEND** |
| `FR-KEP-002` | Pengkajian ditolak bila episode tidak ada, masih `Draft`, atau sudah `Closed` | **MISSING / NEW** |
| `FR-KEP-003` | Perilaku pengkajian poliklinik dan medical check-up **tidak berubah sedikit pun** | **EXISTING / REUSE** — dijaga test regresi |
| `FR-KEP-004` | Pengkajian menyimpan `InpEpisodeId` sehingga terbaca per episode | **EXTEND** |

### `EPIC KEP-02` — Pengkajian awal, ulang, dan amandemen

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-005` | Pengkajian awal dan pengkajian ulang tersimpan sebagai **record terpisah** | **EXTEND** |
| `FR-KEP-006` | Pengkajian awal kedua pada satu episode ditolak dan diarahkan ke pengkajian ulang | **MISSING / NEW** |
| `FR-KEP-007` | Nilai nyeri, risiko jatuh, dan gizi terbaca sebagai perkembangan dari waktu ke waktu | **MISSING / NEW** |
| `FR-KEP-008` | Pengkajian final dapat diamandemen; versi sebelumnya tersimpan beserta aktor, waktu, dan alasannya | **MISSING / NEW** |
| `FR-KEP-009` | Pengkajian final **tidak dapat** dihapus maupun ditimpa diam-diam | **MISSING / NEW** |
| `FR-KEP-010` | Tenggat dan keterlambatan dihitung dari kebijakan yang **aktif saat pengkajian dibuat** | **MISSING / NEW** |
| `FR-KEP-011` | Master kebijakan kosong tidak menahan pencatatan; tidak ada yang dinyatakan terlambat | **MISSING / NEW** |

### `EPIC KEP-03` — Rencana asuhan keperawatan

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-012` | Satu episode tepat satu rencana asuhan; butir masalahnya banyak | **MISSING / NEW** |
| `FR-KEP-013` | Butir memuat masalah, tujuan, rencana tindakan, dan evaluasi | **MISSING / NEW** |
| `FR-KEP-014` | Memperbarui butir menyimpan versi sebelumnya **beserta penulis dan waktu aslinya** | **MISSING / NEW** |
| `FR-KEP-015` | Butir dinyatakan tercapai hanya bila sudah ada evaluasi | **MISSING / NEW** |
| `FR-KEP-016` | Menutup butir **tidak** menghapus tindakan dan evaluasi sebelumnya | **MISSING / NEW** |
| `FR-KEP-017` | Setelah episode ditutup, seluruh riwayat asuhan tetap terbaca hanya-baca | **MISSING / NEW** |

### `EPIC KEP-04` — Tindakan keperawatan

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-018` | Tindakan tercatat beserta apa, kapan, oleh siapa, dan hasilnya | **MISSING / NEW** |
| `FR-KEP-019` | Tindakan mendadak dapat dicatat **tanpa** rujukan rencana asuhan | **MISSING / NEW** |
| `FR-KEP-020` | Permintaan berulang dengan kunci idempotency sama menghasilkan **satu** baris | **MISSING / NEW** |
| `FR-KEP-021` | Kegagalan pengiriman tagihan **tidak** menghilangkan catatan klinis | **MISSING / NEW** |
| `FR-KEP-022` | Catatan final hanya dapat diamandemen penulisnya atau kepala ruangan, dan perubahannya tercatat | **MISSING / NEW** |
| `FR-KEP-023` | Catatan keperawatan dapat tampil pada catatan terpadu tanpa tabel baru | **EXISTING / REUSE** |

### `EPIC KEP-05` — Kepatuhan pengkajian terlihat

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-024` | Kepala ruangan melihat episode yang pengkajian awalnya belum ada atau terlambat | **MISSING / NEW** |
| `FR-KEP-025` | Daftar kosong berbunyi "sudah tepat waktu", bukan "tidak ada data" | **MISSING / NEW** |
| `FR-KEP-026` | Keterlambatan pengkajian **tidak menahan** tindakan apa pun | **MISSING / NEW** |

### `EPIC KEP-06` — Pemakaian alat — **`DEFERRED`**

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-027` | Pemakaian alat pada pasien tercatat beserta waktu mulai, selesai, dan penanggung jawabnya | **DEFERRED** |
| `FR-KEP-028` | Pemakaian alat yang masih berjalan ikut terbawa saat pasien pindah kamar | **DEFERRED** |

> **`EPIC KEP-06` dikeluarkan dari scope rilis pertama secara tertulis lewat `RWI-DEC-089`, dan tetap
> MUST NOT masuk gelombang pengiriman mana pun.** Kepemilikan tabelnya **sengaja tidak diputuskan**:
> `PRD-RWI-FINAL-001` bagian 23.1 memuat 28 baris *source of truth* dan **tidak satu pun** menyebut
> Equipment Usage, sedangkan `RWI-FACT-015` membuktikan modul persediaan/aset yang diandaikan PRD
> **belum berwujud** dan tidak ada master alat medis sama sekali. Memulai pekerjaannya sekarang berarti
> membangun di atas kepemilikan yang dikarang, lalu membongkarnya ketika keputusannya turun.
>
> **Penggantinya selama MVP berjalan:** pemakaian alat dicatat di luar sistem sebagaimana hari ini, dan
> penagihannya tetap manual. Konsekuensi ini disadari dan diterima pemilik saat `RWI-DEC-089` diambil.
>
> **Pemicu masuk kembali:** begitu modul persediaan/aset masuk roadmap Quilvian, `RWI-OQ-048` dibuka
> ulang untuk menetapkan pemiliknya, dan barulah task pemakaian alat boleh dibuat — `RWI-AC-171`.

---

## 11. Model status

Diturunkan dari [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md).

| Mesin | Nilai |
| --- | --- |
| Pengkajian | `Draft`, `InProgress`, `Completed`, `Cancelled` |
| Butir rencana asuhan | `Active`, `Resolved`, `Discontinued` |
| Catatan tindakan | `Recorded`, `Finalized` |
| Pengiriman tagihan | `NotApplicable`, `Pending`, `Dispatched`, `Failed` |

> **Sejak `RWI-DEC-091`, status `Amended` dicabut dari dua mesin pertama.** Koreksi tidak lagi
> memindahkan status dokumen; ia menambah **addendum bernomor urut** pada mesin keutuhan milik
> `MedicalRecordManagement`, dan dokumen tetap pada status finalnya.

**Nol status episode baru** — `RWI-DEC-009` dan `AC-CAP012-03`.

---

## 12. Sasaran arsitektur

| Sasaran | Isinya |
| --- | --- |
| Tabel baru milik Rawat Inap | **Nol** |
| Tabel baru milik `ClinicalManagement` | Empat transaksi + satu master |
| Kolom baru | Enam pada `TrxPatientAssessment`, seluruhnya nullable atau bernilai bawaan |
| Perubahan perilaku pada modul lain | **Satu** — `INT-KEP-01` |
| Endpoint baru | 13 rencana; nol yang sudah tersedia |

---

## 13. Matriks kewenangan

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md)
bagian 2. Resource baru: `NursingCarePlan`, `NursingIntervention`.

> **Peringatan yang dibawa dari `BE-RWI-034`.** Kedua Resource baru wajib memakai nama yang sama
> persis pada `[AccessAction]` dan `[AccessPermission]`, dan wajib diuji dengan peran
> non-SuperAdmin. Ketidakcocokan nama pernah membuat sembilan endpoint menjawab `403` bagi siapa
> pun dan menahan tujuh task frontend.

---

## 14. Batas integrasi dan billing

| Batas | Isinya |
| --- | --- |
| Billing | Menerima pemicu tagihan tindakan beserta kunci idempotency. Kegagalannya **tidak** menghilangkan catatan klinis |
| Gizi | Hanya skrining dan saran rujukan. Asuhan gizi milik modul Gizi |
| Persediaan | **Tidak ada integrasi pada MVP** — `CAP-016` `DEFERRED` lewat `RWI-DEC-089`; modulnya sendiri belum berwujud (`RWI-FACT-015`) |

---

## 15. Guardrail regulasi

| Kewajiban | Yang dipenuhi MVP | Yang belum |
| --- | --- | --- |
| Rekam medis elektronik | Pengkajian, asuhan, dan tindakan keperawatan tersimpan lengkap beserta pelaku dan waktunya | Dokumentasi dokter — milik sub-modul `dokter-rawat-inap` |
| Keterlacakan | Setiap amandemen menyimpan aktor, waktu, alasan, dan versi sebelumnya | — |
| Koreksi rekam medis | Amandemen beralasan; versi lama tidak pernah hilang | — |
| Masa simpan | — | `RWI-OQ-035` menunggu pemilik hukum. Tidak ada penghapusan otomatis yang dirancang |
| Batas waktu pengkajian klinis | Mekanisme pemantauannya siap dan berversi | Angkanya — `RWI-RULE-021` menunggu pemilik klinis |

---

## 16. Kebutuhan non-fungsional

| Kebutuhan | Sasaran |
| --- | --- |
| Ruang kerja terbuka | Konteks pasien tampil sebelum satu pun tombol tulis aktif |
| Idempotency | Wajib pada pencatatan tindakan |
| Concurrency | Unique index parsial `IdempotencyKey` diuji terhadap **PostgreSQL sungguhan**, bukan provider InMemory |
| Privasi | Kolom sensitif tidak masuk logger dan tidak tampil pada daftar ringkas |

---

## 17. Skenario UAT

Setiap epic `MUST HAVE` punya sekurang-kurangnya satu jalur berhasil dan satu jalur gagal.

### `EPIC KEP-01`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-KEP-01` | **Berhasil** | Ns. Sari membuka Tn. Budi yang sudah di kamar, lalu membuat pengkajian awal | Pengkajian tersimpan tanpa diminta nomor antrean |
| `UAT-KEP-02` | **Gagal** | Ns. Sari membuka Ny. Rina yang admisinya baru dibuat tetapi belum tiba | Ditolak: "Pasien belum dikonfirmasi tiba di kamar" |
| `UAT-KEP-03` | **Gagal** | Petugas poliklinik membuat pengkajian tanpa antrean | Ditolak seperti sebelumnya — **perilaku poliklinik tidak berubah** |

### `EPIC KEP-02`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-KEP-04` | **Berhasil** | Ns. Sari mengisi pengkajian ulang esok harinya | Dua baris; nilai nyeri kemarin tetap utuh dan keduanya tampil di lini masa |
| `UAT-KEP-05` | **Berhasil** | Kepala ruangan menambah koreksi pada pengkajian yang salah isi | Status **tetap** `Completed`; isi asli tetap terbaca apa adanya, dan koreksinya muncul sebagai addendum bernomor beserta alasan, penulis, dan waktunya |
| `UAT-KEP-06` | **Gagal** | Ns. Sari membuat pengkajian awal kedua | Ditolak dan diarahkan ke pengkajian ulang |
| `UAT-KEP-07` | **Gagal** | Menyelesaikan pengkajian dengan risiko jatuh belum terisi | Ditolak; bagian yang kosong disebut satu per satu |

### `EPIC KEP-03`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-KEP-08` | **Berhasil** | Ns. Sari menetapkan masalah, lalu memperbaruinya sesudah evaluasi | Versi lama tersimpan beserta penulis dan waktu aslinya |
| `UAT-KEP-09` | **Gagal** | Menyatakan masalah tercapai tanpa evaluasi | Ditolak; butir tetap `Active` |

### `EPIC KEP-04`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-KEP-10` | **Berhasil** | Ns. Sari mencatat tindakan; koneksi lambat lalu tombol tertekan dua kali | **Satu** tindakan tercatat |
| `UAT-KEP-11` | **Berhasil** | Tindakan dicatat saat sistem tagihan sedang mati | Catatan klinis tersimpan; penanda pengiriman `Failed` |
| `UAT-KEP-12` | **Gagal** | Ns. Dewi mencoba mengubah catatan final milik Ns. Sari | Ditolak; isi catatan tidak berubah |

### `EPIC KEP-05`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-KEP-13` | **Berhasil** | Kepala ruangan membuka daftar pantau saat ada dua episode terlambat | Dua baris; masing-masing membuka ruang kerja pasiennya |
| `UAT-KEP-14` | **Gagal** | Daftar pantau dibuka sebelum kebijakan batas waktu diisi | Berbunyi "batas waktu belum ditetapkan", **bukan** daftar kosong yang menyesatkan |

---

## 18. Definition of Done

Setiap butir dapat dijawab "ya" atau "belum" beserta buktinya.

| No | Butir | Bukti |
| ---: | --- | --- |
| 1 | `INT-KEP-01` terpasang dan pengkajian rawat inap dapat dibuat tanpa antrean | Test integrasi `AC-CAP012-01` hijau |
| 2 | Perilaku poliklinik dan medical check-up terbukti tidak berubah | Test regresi `FR-KEP-003` hijau |
| 3 | Jalur pengkajian IGD terbukti tidak rusak | Test regresi hijau — `RWI-DEC-051` |
| 4 | Pengkajian awal dan ulang terpisah, nilai lama tidak tertimpa | `AC-CAP012-02` hijau |
| 5 | Amandemen menyimpan aktor, waktu, alasan, dan versi lama | `AC-CAP012-05` hijau |
| 6 | Rencana asuhan menyimpan versi beserta penulis aslinya | `AC-CAP013-02` hijau |
| 7 | Menutup butir tidak menghapus tindakan sebelumnya | `CAP-013` aturan 6 hijau |
| 8 | Idempotency terbukti pada **PostgreSQL sungguhan**, bukan InMemory | Test dua permintaan bersamaan hijau |
| 9 | Kegagalan tagihan tidak menghilangkan catatan klinis | `AC-CAP014-02` hijau |
| 10 | Catatan final tidak dapat disunting pihak ketiga | `AC-CAP014-03` hijau |
| 11 | Master kebijakan kosong tidak menahan pencatatan | `VAL-KEP-17` hijau |
| 12 | Kedua Resource hak akses baru berfungsi bagi peran non-SuperAdmin | Test hak akses per peran hijau |
| 13 | **Nol tabel `Inp*` untuk dokumentasi klinis** | Architecture test bagian 8 matriks acceptance hijau |
| 14 | Enam layar terjangkau sesuai `IA-INP-01` dan `IA-INP-05` | Bukti navigasi |
| 15 | Kolom sensitif tidak muncul di logger | Pemeriksaan payload log |

---

## 19. Urutan pengiriman

| Gelombang | Isinya | Prasyarat |
| --- | --- | --- |
| **`KEP-MVP-0`** | `INT-KEP-01`; enam kolom pada pengkajian; enum baru; master kebijakan | `episode-rawat-inap` `M1` selesai |
| **`KEP-MVP-1`** | `EPIC KEP-01`, `EPIC KEP-02` — pengkajian awal, ulang, amandemen, lini masa | `KEP-MVP-0` |
| **`KEP-MVP-2`** | `EPIC KEP-03` — rencana asuhan beserta riwayat versinya | `KEP-MVP-1` |
| **`KEP-MVP-3`** | `EPIC KEP-04` — tindakan, idempotency, pemisahan kegagalan tagihan | `KEP-MVP-1` |
| **`KEP-MVP-4`** | `EPIC KEP-05` — daftar pantau kepatuhan | `KEP-MVP-1` |
| **`POST-MVP`** | `CAP-027` asuhan gizi; katalog SDKI; nilai batas waktu klinis | Modul Gizi berdiri; keputusan SDKI; pemilik klinis |
| **Tidak masuk gelombang mana pun** | **`EPIC KEP-06` pemakaian alat** | `DEFERRED` lewat `RWI-DEC-089` — dikeluarkan dari scope rilis pertama secara tertulis. Masuk kembali setelah modul persediaan/aset ada |

---

## 20. Pertanyaan terbuka sebelum development lock

| No | Pertanyaan | Pemilik | Memblokir? |
| ---: | --- | --- | :---: |
| 1 | ~~**Siapa pemilik tabel catatan pemakaian alat?**~~ **TERTUTUP 2026-09-02** oleh `RWI-DEC-089`: pertanyaannya dijawab dengan **menunda kemampuannya**, bukan dengan memilih pemilik. `EPIC KEP-06` dikeluarkan dari scope rilis pertama secara tertulis; `RWI-OQ-048` dibuka ulang saat modul persediaan/aset ada | Product/Domain bersama pemilik persediaan | **Tidak lagi** — `CAP-016` kini `DEFERRED` |
| 2 | Apakah rumah sakit memakai terminologi SDKI/SLKI/SIKI? Menentukan perlu-tidaknya katalog terminologi berversi | Clinical governance | Tidak — struktur rencana asuhan tetap dapat dibangun |
| 3 | Berapa batas waktu pengkajian awal dan pengkajian ulang? `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | Tidak — mekanismenya dibangun, angkanya menyusul |
| 4 | Apakah catatan keperawatan tampil pada catatan terpadu bagi seluruh profesi? PRD `CAP-014` aturan 4 menyebut "sesuai kebijakan" tanpa menyebut kebijakannya | Clinical governance | Tidak — catatan tetap tersimpan dan terbaca dari ruang kerja |

> **Tidak ada lagi pertanyaan yang memblokir.** Pertanyaan 1 ditutup `RWI-DEC-089` pada 2026-09-02 dengan
> mengeluarkan `EPIC KEP-06` dari scope rilis pertama secara tertulis — tepat jalan keluar yang disyaratkan
> paragraf ini sebelumnya. Pertanyaan 2, 3, dan 4 tidak memblokir dan tidak pernah memblokir.
>
> **Gerbang yang tersisa bukan pertanyaan wawancara, melainkan dua hal lain:** penghalang teknis
> `INT-KEP-01` milik `ClinicalManagement`, dan satu butir konsistensi baru yang ditemukan 2026-09-02 pada
> bagian 20.1. Keduanya dicatat supaya tidak terlewat, dan keduanya di luar wewenang dokumen ini.

### 20.1 Butir konsistensi — **ditutup 2026-09-02** oleh `RWI-DEC-091`

`RWI-DEC-086` dan `RWI-DEC-087` terbit **setelah** desain sub-modul ini ditulis, dan keduanya mengubah cara
dokumen klinis rawat inap dinyatakan final serta dikoreksi. Butir ini sempat terbuka, dan **sudah ditutup**
pemilik pada 2026-09-02 lewat `RWI-DEC-091`.

**Jawabannya: koreksi dibedakan dari perkembangan.**

| Dokumen | Cara membetulkannya | Alasannya |
| --- | --- | --- |
| Pengkajian keperawatan | **Addendum** pada mesin keutuhan `MedicalRecordManagement`, jenis `Assessment` | Pembetulan kesalahan. Sama seperti dokumen dokter |
| Catatan tindakan keperawatan | **Addendum**, jenis `Procedure` | Pembetulan kesalahan |
| Butir rencana asuhan | **Tetap berversi**, tidak beraddendum | Perubahannya **bukan** pembetulan melainkan perkembangan klinis — PRD `CAP-013` aturan 5. `AC-CAP013-02` menuntut versi lama tetap menyimpan penulis dan waktu aslinya |

Akibatnya pada dokumen ini: status `Amended` **dicabut** dari mesin pengkajian dan mesin catatan tindakan,
dan dua kolom amandemen tidak jadi diminta. Rinciannya ada di
[`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) `0.3.0` dan
[`contracts/integration-contract.md`](./contracts/integration-contract.md) `INT-KEP-06`.

**Satu syarat teknis menyusul, dan ia memblokir pembangunan.** `RWI-FACT-016` menemukan mesin keutuhan
hari ini **hanya menegakkan** jenis `ProgressNote` sesuai `RM-DEC-019`, sedangkan pendaftaran jenis lain
tetap diterima tanpa dikunci. Bila dibangun apa adanya, pengkajian akan terlihat terdaftar tetapi tidak
pernah terkunci. `RWI-OQ-051` meminta `Assessment` dan `Procedure` ikut ditegakkan — nol nilai enum
baru, tetapi perubahannya milik `MedicalRecordManagement`.

Tabel di bawah dipertahankan sebagai jejak pertanyaan aslinya.

| Hal | Keadaannya |
| --- | --- |
| Yang ditetapkan `RWI-DEC-086` | Catatan menjadi final saat penulisnya menekan **Selesai**; sejak itu isinya tidak dapat disunting, dan satu-satunya jalan membetulkan adalah **addendum beralasan** |
| Yang ditetapkan `RWI-DEC-087` | `ClinicalManagement` mendaftarkan dokumen ke **mesin keutuhan dokumen** saat finalisasi, dan pekerjaan itu **tidak boleh** membuat mesin koreksi tandingan |
| Cakupan tertulis keduanya | **Catatan dokter** — SOAP, kajian medis, tindakan dokter, dan catatan terpadu. Dokumen keperawatan **tidak disebut** |
| Kenapa tetap menyentuh sub-modul ini | `INT-KEP-03` mengalirkan catatan keperawatan ke **catatan terpadu**, dan catatan terpadu justru **disebut** `RWI-DEC-086` |
| Yang dirancang sub-modul ini | Mesin amandemennya sendiri: `Completed` — `Amended`, dan `Recorded` — `Finalized` — `Amended`, dengan "versi lama tersalin" serta "setiap amandemen menambah satu versi" — `contracts/state-transition-matrix.md` bagian 1 dan 3 |
| **Pertanyaannya** | ~~Apakah dokumen keperawatan memakai mesin keutuhan dokumen seperti dokumen dokter, atau tetap memakai mesin versi milik sub-modul ini?~~ **Terjawab `RWI-DEC-091`.** Catatan: mesinnya ternyata milik **`MedicalRecordManagement`**, bukan `ClinicalManagement` — `RWI-FACT-016` |
| Kenapa tidak dijawab di sini | Ini keputusan **kepemilikan mesin koreksi**, sejenis `RWI-DEC-081`. Menjawabnya sendiri berarti blueprint memutuskan hal yang sudah dinyatakan milik pemilik modul |
| Memblokir? | **Sudah tidak.** Ditutup `RWI-DEC-091` sebelum satu task pun dibangun — tepat seperti yang diharapkan baris ini. Yang tersisa adalah syarat teknis `RWI-OQ-051` |
| Pemilik jawaban | Muhammad Hamzah, selaku Product/Domain sekaligus pemilik `ClinicalManagement` |
| Langkah yang benar | ~~`/qv-grill` Amendment Pass~~ **Sudah dijalankan 2026-09-02.** Hasilnya `RWI-DEC-091` |

## 21. Gelombang 1A — Rawat Inap Safety Corrections

**Ditambahkan 11 September 2026**, menyerap `RWI-DEC-098` dan `RWI-DEC-100`. Menurunkan dari
`contracts/api-contract.md` `0.4.0` beserta kedua matriks `0.4.0`.

### 21.1 Dua penyimpangan yang dibereskan

| Penyimpangan | Keadaan hari ini di source | Kenapa berbahaya |
| --- | --- | --- |
| Tanda vital dapat dihapus | `DELETE /patient-vital-signs/{id}` melakukan soft delete tanpa pemeriksaan status, tanpa alasan, **dan ikut mematikan penanda pemberitahuan dokter** | Tanda vital adalah deret waktu. Menghapus satu baris tidak menyisakan lubang yang terlihat; grafik tetap tersambung dan tetap tampak wajar |
| Dokumentasi keperawatan tidak punya penjaga kewenangan | Resolver konteks klinis **nol menyebut perawat** | Pengguna mana pun yang memegang butir hak aksesnya dapat menulis pengkajian untuk pasien mana pun di rumah sakit |

### 21.2 Batas gelombang ini

**Titik mulai.** Kontrak `0.4.0` `draft`; keputusan `RWI-DEC-098` dan `RWI-DEC-100` `approved`.

**Titik akhir.**

1. `DELETE` pada tanda vital tidak tersedia; jawabannya `404`.
2. Penulis keperawatan diambil dari pengguna terautentikasi.
3. Kewenangan menulis dinilai dari unit tempat episode berada.
4. Acceptance criteria `AC-KEP-040` s.d. `AC-KEP-050` lulus.

### 21.3 Epic dan functional requirement

| ID | Epic | Prioritas | Disposisi |
| --- | --- | --- | --- |
| `EPIC KEP-07` | Tanda vital tidak dapat disembunyikan | `P0` | `EXTEND` |
| `EPIC KEP-08` | Penulis keperawatan berasal dari pengguna terautentikasi | `P0` | `MISSING / NEW` |

| FR | Bunyi requirement | Epic | Disposisi |
| --- | --- | --- | --- |
| `FR-KEP-029` | Jalur `DELETE` pada tanda vital tidak tersedia | `KEP-07` | `EXTEND` |
| `FR-KEP-030` | Penanda pemberitahuan dokter tidak dapat dimatikan tanpa alasan tersimpan | `KEP-07` | `EXTEND` |
| `FR-KEP-031` | Penulis diambil dari `ApplicationUser.EmployeeId`; `nurseId` pada payload tidak menentukan penulis | `KEP-08` | `MISSING / NEW` |
| `FR-KEP-032` | Perawat boleh menulis untuk pasien yang episodenya berada di unit tempat ia bertugas | `KEP-08` | `MISSING / NEW` |
| `FR-KEP-033` | `InpNurseAssignment` tetap menjadi penunjukan penanggung jawab, **bukan** gerbang hak tulis | `KEP-08` | `EXISTING / REUSE` |
| `FR-KEP-034` | Sumber data unit tempat perawat bertugas ditetapkan tanpa menyalin unit ke baris penugasan | `KEP-08` | `OPEN DECISION` — bentuk teknisnya diselesaikan pada task implementasi |

### 21.4 Skenario UAT

**Jalur berhasil.** Perawat dinas malam mendokumentasikan pengkajian untuk pasien di bangsalnya
yang bukan tanggung jawabnya, dan berhasil. Perawat penanggung jawab berganti, tetapi perawat lama
yang masih bertugas di unit itu tetap dapat menulis. Tanda vital yang sudah tercatat tetap terbaca
utuh pada lini masa.

**Jalur gagal.** Perawat menulis untuk pasien di unit lain dan ditolak. Pasien dipindahkan ke unit
lain, lalu perawat unit lama menulis dan ditolak. Pengguna tanpa pemetaan pegawai menulis dan
ditolak. Pemanggilan `DELETE` pada tanda vital dijawab sebagai route yang tidak ada.

### 21.5 Definition of Done gelombang ini

| Butir | Cara menjawabnya |
| --- | --- |
| `DELETE` hilang dari controller tanda vital | Pencarian source mengembalikan nol `HttpDelete` pada berkas itu |
| Deret waktu tanda vital terbukti utuh | `AC-KEP-041` lulus |
| Kewenangan berbasis unit terbukti bekerja dua arah | `AC-KEP-044` berhasil dan `AC-KEP-045` ditolak |
| Kewenangan mengikuti unit episode, bukan salinan | `AC-KEP-049` lulus |
| Regresi Rawat Jalan dan IGD lulus | `AC-KEP-043` lulus. **Wajib**, karena controller dipakai bersama |
| Skenario negatif memakai peran nyata | `AC-KEP-050` lulus |
| Nol butir hak akses baru | Daftar permission sebelum dan sesudah sama persis |

**Satu butir yang sengaja dinyatakan belum terpenuhi.** Pembatalan tanda vital final belum dapat
diuji, karena `ClinicalDocumentKind.VitalSign` belum termasuk jenis yang ditegakkan mesin keutuhan
dokumen. Butir ini ditulis **`NOT RUN` apa adanya** pada laporan task, bukan dihilangkan dari
daftar. Ia tidak memblokir gelombang ini, tetapi memblokir pernyataan bahwa jalur pengganti sudah
lengkap. Dilacak `V2-UNK-01`.

### 21.6 Yang sengaja di luar gelombang ini

| Yang ditunda | Alasan |
| --- | --- |
| Medication Administration Record, transfusi, sliding scale, catatan cairan, handover shift, dan reaksi obat | Seluruhnya `Missing` pada capability map bagian 16. Di luar `Gelombang 1A`, dan sebagiannya menunggu sepuluh butir `OPEN-MVP` yang `RWI-DEC-097` tidak buka |
| Peran pada penugasan perawat | `RWI-DEC-100` tidak membedakan peran perawat. Menambahkannya sekarang berarti merancang kebijakan yang belum diputuskan |

---

---

## 22. Penyelarasan `PRD-RWI-V2-001` — revision `0.4` ★ 15 September 2026

### 22.1 Identitas dokumen

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| `contract_version` | `0.5.0` |
| Baseline requirement | `PRD-RWI-V2-001` v`2.0` bagian 24–49; `PRD-to-MVP-Rawat-Inap-V2` `FR-MVP-KEP-001` s.d. `018`, `AC-MVP-025` s.d. `034` |
| Keputusan | `RWI-DEC-100`, `108`, `113` s.d. `120`, `124`, `131` s.d. `137`, `140`, `141`, `145` s.d. `149` |
| Gate | `02-requirement-completeness-gate.md` revision `1.6` — `INP-S17`, `INP-S18`, `INP-S19` sliding scale `READY_FOR_DOMAIN_DESIGN`; handover shift dan transfusi `DEFERRED` |
| Menurunkan dari | `02-backend-architecture.md` `0.4` bagian 11; `data/data-dictionary.md` `0.4` bagian 11; `03-frontend-architecture.md` `0.3` bagian 10; kontrak `0.5.0` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — kepemilikan ditetapkan `RWI-DEC-117`, `118`, `132`, `147` s.d. `149` |

### 22.2 Ringkasan eksekutif

Ruang kerja keperawatan V2 sudah berdiri, tetapi isinya baru empat bagian. Perawat V1 terbiasa dengan delapan menu:
pengkajian bertujuh isi, obat per dosis, cairan masuk-keluar, gula darah, dan catatan asuhan. Penyelarasan ini
**mempertahankan layout V2** dan mengisinya dengan kemampuan V1 — tanpa satu pun tabel milik Rawat Inap, dan tanpa
satu pun angka batas klinis tertanam di source code.

Tiga hal paling berat: **MAR per dosis** (obat tidak lagi dicatat di catatan bebas), **pelaksanaan sliding scale** yang
menghitung dosis insulin dari GDS bangsal, dan **konfigurasi klinis berversi** yang membuat skor risiko jatuh dapat
disahkan pemilik klinis alih-alih ditentukan developer.

### 22.3 Masalah produk

| Masalah | Bukti hari ini | Akibatnya |
| --- | --- | --- |
| Batas kategori risiko jatuh tertanam di controller | `RWI-FACT-036` | Mengubah skala berarti rilis kode; tidak ada jejak versi yang dipakai |
| Pengkajian datar tanpa Resiko Jatuh, Monitoring Nyeri, Edukasi terpisah; tanpa progres | `RLN3-CAP-05` s.d. `08`, `18`, `22` | Perawat tidak tahu bagian mana yang belum dikerjakan; "belum dikaji" terkirim sebagai normal |
| Tidak ada MAR | Capability map bagian 16 `Missing` | Pemberian obat, penahanan, dan cek ganda high-alert tidak tercatat terstruktur |
| Tidak ada catatan cairan dan gula darah rawat inap | `RLN3-CAP-09`, `12` | Balance dihitung di kertas; sliding scale tidak punya sumber GDS |
| Evaluasi Awal MPP tidak punya tempat | `RWI-DEC-118` | Dokumen MPP ditulis di luar sistem |
| Menu V1 hilang tanpa penjelasan | `RLN-04`, `RLN-06` | Perawat mengira fitur dihapus |

### 22.4 Visi produk

Ns. Siti membuka Budi dari census. Di kiri ada delapan menu yang ia kenal. Pengkajian Pasien menunjukkan 40% — Kajian
Umum dan Resiko Jatuh selesai, Monitoring Nyeri masih konsep. Ia mencatat Ceftriaxone 08.00 di MAR, mengetik GDS 280
mg/dL di layar sliding scale dan melihat "3 unit" sebelum memberi, lalu meminta Ns. Rina mengonfirmasi insulin itu.
Di Pengawasan Harian, balance 24 jam +550 ml dihitung sendiri dari entri yang ia catat. Menu Bank Darah menampilkan
"Integrasi belum tersedia" — jujur, tanpa data contoh.

### 22.5 Batas MVP penyelarasan ini

| Batas | Isinya |
| --- | --- |
| **Titik mulai** | Episode `Admitted`; resep rawat inap aktif dari `dokter-rawat-inap`; instrumen klinis minimal berstatus draft di lingkungan uji |
| **Titik akhir** | (1) Delapan menu tampil dengan isi nyata atau "Integrasi belum tersedia"; (2) lima dokumen Pengkajian Pasien berinstrumen berversi dengan progres; (3) Evaluasi Awal ditulis MPP di unitnya; (4) Pengawasan Harian menghitung balance per shift dan 24 jam; (5) setiap dosis obat rawat inap tercatat di MAR dengan cek ganda high-alert; (6) dosis sliding scale dihitung dari GDS bangsal dan tercatat sekali; (7) SOAP dan Catatan Keperawatan tersimpan sebagai CPPT berjenis |
| **Di luar batas** | Handover shift, transfusi, pemakaian alat, pemesanan kamar operasi, Gizi/Hemodialisa/Bank Darah/Rehab Medik, serah terima klinis antarunit, rekonsiliasi saat transfer dan pulang |

### 22.6 Pelaku sasaran

| Pelaku | Yang dikerjakan |
| --- | --- |
| Perawat pelaksana | Seluruh dokumentasi harian, MAR, sliding scale, cek ganda perawat lain, mencatat obat bawaan, memesan atas instruksi |
| Kepala ruangan | Seperti perawat, ditambah koreksi MAR |
| MPP | Evaluasi Awal di unit penempatannya |
| Dokter | Membaca MAR, Pengawasan Harian, Evaluasi Awal; memutuskan obat bawaan dari ruang kerja dokter |
| Apoteker / admin Farmasi | Jadwal pemberian obat; membaca MAR |
| Admin konfigurasi klinis dan komite keperawatan | Menyusun dan mengesahkan instrumen — terpisah |
| Petugas yang ditunjuk | Membaca ringkasan tagihan |

### 22.7 Pemilihan kemampuan MVP

| Kemampuan | ID asal | Prioritas | Alasan |
| --- | --- | --- | --- |
| Pengkajian Pasien tujuh isi dan progres | `CAP-012`; `RLN3-CAP-05` s.d. `08`, `17`, `18`, `21`, `22`, `29` | `MUST HAVE` | Inti ruang kerja; `FR-MVP-KEP-001` s.d. `005` |
| Konfigurasi klinis berversi | `CAP-012`; `RLN3-CON-01` | `MUST HAVE` | Syarat keselamatan `RWI-DEC-124`, `136` |
| Pengawasan Harian | `CAP-012`; `RLN3-CAP-09`, `12` | `MUST HAVE` | `FR-MVP-KEP-017`; sumber GDS sliding scale |
| Evaluasi Awal MPP | `CAP-012` | `MUST HAVE` | `RWI-DEC-118` |
| MAR | `CAP-023-MAR` | `MUST HAVE` | `RWI-DEC-116` |
| Pelaksanaan sliding scale | `CAP-023-MAR` | `MUST HAVE` pada rilis `KEP-V2-2` | `RWI-DEC-145` |
| SOAP dan Catatan Keperawatan berjenis | `CAP-014`, `CAP-021` | `MUST HAVE` | `RWI-DEC-115`, `140` |
| Tindakan dan Penunjang Medis permukaan | `CAP-024`, `CAP-015-LAB`, `CAP-015-RAD` | `SHOULD HAVE` | `RWI-DEC-113`, `114`; Lab/Rad bergantung pemiliknya |
| Transfer Pasien permukaan | `CAP-017` | `SHOULD HAVE` | `RWI-DEC-113` |
| Tagihan Pasien baca-saja | `RWI-DEC-137` | `SHOULD HAVE` | Bergantung kontrak Billing |

### 22.8 Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti selama MVP |
| --- | --- | --- |
| Handover shift | `RWI-DEC-145` | Catatan Keperawatan naratif; tidak ada menu |
| Transfusi dan intake darah dari transfusi | `RWI-DEC-145`; `G-20` `CONFLICT` | Intake darah diketik perawat sebagai sumber Darah |
| Pemakaian alat | `RWI-DEC-089` | Menu "Integrasi belum tersedia" |
| Pemesanan kamar operasi | `RWI-DEC-108` | Sama |
| Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-113` | Sama |
| Serah terima klinis antarunit | `RWI-DEC-113` | Perpindahan tempat tidur `CAP-017` |
| Rekonsiliasi saat transfer dan pulang | Gate `G-17` | Rekonsiliasi saat admisi |
| Tagihan strip glukometer | Gate `G-28` | Tidak ditagih dari Pengawasan Harian |
| Nasib catatan sliding scale V1 | Gate `G-29` | Masuk cakupan migrasi `OPEN-MVP-010` |

### 22.9 Alur bisnis target

Diagram ada di [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) bagian 4, dan tiga berkas per proses:
[`02-pengkajian-pasien-dan-instrumen.md`](./flowcharts/02-pengkajian-pasien-dan-instrumen.md),
[`03-obat-mar-dan-sliding-scale.md`](./flowcharts/03-obat-mar-dan-sliding-scale.md),
[`04-pengawasan-harian-dan-cairan.md`](./flowcharts/04-pengawasan-harian-dan-cairan.md).

| Langkah | Pelaku | Hasil |
| ---: | --- | --- |
| 1 | Perawat | Kajian Umum menunjuk tanda vital masuk; Resiko Jatuh dan Monitoring Nyeri diselesaikan dengan versi sah |
| 2 | MPP | Evaluasi Awal diselesaikan |
| 3 | Dokter | Resep aktif; dosis `Due` terbentuk dari jadwal frekuensi |
| 4 | Perawat | Dosis dicatat; high-alert menunggu perawat kedua |
| 5 | Perawat | GDS dicatat dari layar sliding scale; dosis insulin dihitung dan dicatat sekali |
| 6 | Perawat | Intake dan output dicatat; intake obat menunjuk dosis MAR |
| 7 | Perawat | SOAP dan Catatan Keperawatan tersimpan sebagai CPPT berjenis |
| 8 | Perawat | Perencanaan Pulang diselesaikan; progres 100% |
| 9 | `episode-rawat-inap` | Penutupan membatalkan dosis `Due` masa depan |

### 22.10 Epic dan functional requirement

| Epic | Nama | Prioritas | Disposisi |
| --- | --- | --- | --- |
| `EPIC KEP-09` | Ruang kerja V2 dengan delapan menu | `P0` | `EXTEND` |
| `EPIC KEP-10` | Konfigurasi klinis berversi | `P0` | `MISSING / NEW` |
| `EPIC KEP-11` | Pengkajian Pasien tujuh isi dan progres | `P0` | `EXTEND` |
| `EPIC KEP-12` | Evaluasi Awal MPP | `P0` | `MISSING / NEW` |
| `EPIC KEP-13` | Pengawasan Harian | `P0` | `MISSING / NEW` |
| `EPIC KEP-14` | MAR | `P0` | `MISSING / NEW` |
| `EPIC KEP-15` | Pelaksanaan sliding scale | `P1` | `MISSING / NEW` |
| `EPIC KEP-16` | Asuhan, Tindakan, Penunjang, dan Transfer di ruang kerja perawat | `P1` | `EXTEND` |
| `EPIC KEP-17` | Tagihan Pasien baca-saja | `P2` | `MISSING / NEW` — bergantung Billing |

| FR | Bunyi requirement | Epic | Disposisi | Bukti di dokumen |
| --- | --- | --- | --- | --- |
| `FR-KEP-035` | Navigasi internal kiri memuat delapan menu dan tab sekunder PRD bagian 25 dengan urutan tetap | `KEP-09` | `EXTEND` | `03` 10.3 |
| `FR-KEP-036` | Menu yang backend-nya belum ada menampilkan konteks pasien dan "Integrasi belum tersedia" tanpa data tiruan dan tanpa permintaan jaringan | `KEP-09` | `MISSING / NEW` | `INT-KEP-17` |
| `FR-KEP-037` | Kegagalan konteks pasien menonaktifkan seluruh tombol tulis delapan menu | `KEP-09` | `EXISTING / REUSE` | `03` 3.1 |
| `FR-KEP-038` | Enum jenis pengkajian frontend sama dengan backend; daftar per episode dibaca berpaginasi; detail dibaca sebelum disunting; isian belum dikaji tidak terkirim sebagai normal | `KEP-09` | `EXTEND` — perbaikan | `RLN3-CAP-17`, `18`, `21`, `22`, `29` |
| `FR-KEP-039` | Instrumen dan formulir disimpan sebagai versi `Draft`/`Approved`/`Retired` dengan definisi JSON dan hash | `KEP-10` | `MISSING / NEW` | Data 11.4–11.5 |
| `FR-KEP-040` | Pengesah versi bukan pengubah terakhir | `KEP-10` | `MISSING / NEW` | `VAL-KEP-20a` |
| `FR-KEP-041` | Pita skor divalidasi tidak bertumpuk dan tidak berlubang; instrumen sejenis tidak bertumpuk rentang usia | `KEP-10` | `MISSING / NEW` | `VAL-KEP-19` |
| `FR-KEP-042` | Versi draft hanya dapat dipakai menyelesaikan dokumen di lingkungan dengan pengaturan uji aktif | `KEP-10` | `MISSING / NEW` | `INT-KEP-16` |
| `FR-KEP-043` | Source tidak memuat angka batas atau skor butir risiko jatuh rawat inap maupun daftar isian wajib tetap | `KEP-10` | `EXTEND` — mencabut `RWI-FACT-036` | `AC-KEP-054`, `061` |
| `FR-KEP-044` | Skor dan pita dihitung server dan disimpan bersama versi; tidak dihitung ulang setelah dokumen selesai | `KEP-11` | `MISSING / NEW` | Data 11.6 |
| `FR-KEP-045` | Kajian Umum delapan bagian; kateter di Eliminasi, alat bantu mobilitas di Ketergantungan, psikososial di Kondisi Umum, catatan relevan di Sumber Data Pasien | `KEP-11` | `EXTEND` | `03` 10.4.3 |
| `FR-KEP-046` | Kajian Umum menunjuk satu baris tanda vital, tidak menyalin angkanya | `KEP-11` | `EXTEND` | `VitalSignId` |
| `FR-KEP-047` | Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi menjadi jenis dokumen tersendiri | `KEP-11` | `EXTEND` | Enum `6`–`8` |
| `FR-KEP-048` | Monitoring Nyeri mewajibkan keadaan nyeri sebelum selesai dan menyimpan waktu kajian ulang | `KEP-11` | `MISSING / NEW` | `VAL-KEP-22` |
| `FR-KEP-049` | Input tanda vital rawat inap tidak menerima isian nyeri | `KEP-11` | `EXTEND` | `VAL-KEP-22c` |
| `FR-KEP-050` | Progres menghitung lima bagian ✓/!/○ menurut keadaan dokumen, persen kelipatan 20; Pengawasan Harian dan Evaluasi Awal tampil tanpa dihitung | `KEP-11` | `MISSING / NEW` | API 7.1 |
| `FR-KEP-051` | Temuan berisiko tampil sebagai alert pada kepala konteks, bukan mengubah progres | `KEP-11` | `MISSING / NEW` | `RWI-DEC-119` (3) |
| `FR-KEP-052` | Progres gagal dimuat menampilkan galat, bukan ○ | `KEP-11` | `MISSING / NEW` | `AC-KEP-072` |
| `FR-KEP-053` | Evaluasi Awal adalah dokumen tersendiri milik `ClinicalManagement` dengan delapan bagian checklist berversi | `KEP-12` | `MISSING / NEW` | Data 11.7 |
| `FR-KEP-054` | Hanya pemegang hak MPP yang ditempatkan di unit episode menulis; perawat lain membaca | `KEP-12` | `MISSING / NEW` | `VAL-KEP-23` |
| `FR-KEP-055` | Satu Evaluasi Awal hidup per episode; koreksi lewat addendum setelah jenis dokumen `14` tersedia | `KEP-12` | `MISSING / NEW` — addendum bergantung `INT-KEP-12` | State 5.3 |
| `FR-KEP-056` | Tanda vital menyimpan episode dan tampil sebagai deret dan grafik per episode | `KEP-13` | `EXTEND` | Data 11.2 |
| `FR-KEP-057` | Entri cairan masuk/keluar bersumber, bervolume ml, berwaktu, berpelaksana; batal dan koreksi beralasan menyimpan nilai lama | `KEP-13` | `MISSING / NEW` | Data 11.8 |
| `FR-KEP-058` | Intake obat wajib menunjuk tepat satu dosis MAR `Administered` dan volume diketik termasuk pelarut | `KEP-13` | `MISSING / NEW` | `VAL-KEP-24d`–`g` |
| `FR-KEP-059` | Balance per shift dan 24 jam dihitung dari entri aktif; tanpa shift hanya 24 jam | `KEP-13` | `MISSING / NEW` | API 7.5 |
| `FR-KEP-060` | Jam shift dikonfigurasi per unit atau bawaan dan tidak mempengaruhi kewenangan | `KEP-13` | `MISSING / NEW` | `AC-KEP-093` |
| `FR-KEP-061` | GDS bangsal tersimpan satu tempat dengan satuan wajib tanpa bawaan | `KEP-13` | `MISSING / NEW` | Data 11.9 |
| `FR-KEP-062` | Diet, mobilisasi, lingkar perut, dan agitasi dicatat terstruktur | `KEP-13` | `MISSING / NEW` | Data 11.10 |
| `FR-KEP-063` | Pengingat dosis tanpa entri intake dan penanda entri yang dosisnya dikoreksi — tanpa mewajibkan | `KEP-13` | `MISSING / NEW` — usulan `G-26`, `G-27` | `VAL-KEP-36c`, `f` |
| `FR-KEP-064` | Dosis `Due` terbentuk dari butir resep aktif berjadwal secara idempoten, saat MAR dibuka dan terjadwal | `KEP-14` | `MISSING / NEW` | `INT-KEP-08` |
| `FR-KEP-065` | Pencatatan dosis `Administered`/`Held`/`Refused`/`Missed` dengan syarat isian, alasan, dan catatan penyimpangan; pelaksana dari akun login; kiriman ulang tidak menggandakan | `KEP-14` | `MISSING / NEW` | `VAL-KEP-30` |
| `FR-KEP-066` | Obat high-alert menunggu konfirmasi perawat lain sebelum `Administered` | `KEP-14` | `MISSING / NEW` | `VAL-KEP-31` |
| `FR-KEP-067` | PRN mencatat indikasi dan evaluasi; butir tanpa jadwal terkonfigurasi dicatat beralasan | `KEP-14` | `MISSING / NEW` | API 7.11 |
| `FR-KEP-068` | Koreksi dosis beralasan menyimpan revisi; dosis tidak pernah dihapus | `KEP-14` | `MISSING / NEW` | Data 11.13 |
| `FR-KEP-069` | Penghentian butir membatalkan dosis `Due` sesudahnya; penutupan episode membatalkan dosis `Due` masa depan | `KEP-14` | `MISSING / NEW` | `INT-KEP-09`, `15` |
| `FR-KEP-070` | Dugaan reaksi obat dicatat dari dosis sebagai alergi `Suspected` tertaut tanpa mengubah MAR | `KEP-14` | `EXTEND` | `INT-KEP-13` |
| `FR-KEP-071` | Jadwal jam per frekuensi dan pengaturan MAR dikonfigurasi Farmasi; frekuensi tanpa jadwal terlihat | `KEP-14` | `MISSING / NEW` | API 7.13 |
| `FR-KEP-072` | Pelaksanaan sliding scale ditolak tanpa order aktif | `KEP-15` | `MISSING / NEW` | `VAL-KEP-27` |
| `FR-KEP-073` | Dosis hanya dihitung dari GDS bangsal dengan satuan sama dengan protokol | `KEP-15` | `MISSING / NEW` | `VAL-KEP-28`, `29a` |
| `FR-KEP-074` | GDS, dosis MAR, dan pelaksanaan tersimpan dalam satu transaksi idempoten; dosis tampil sekali di MAR | `KEP-15` | `MISSING / NEW` | `INT-KEP-11` |
| `FR-KEP-075` | Pratinjau rentang dan dosis tampil sebelum menyimpan; pengecualian dosis beralasan | `KEP-15` | `MISSING / NEW` | API 7.12 |
| `FR-KEP-076` | Rentang 0 unit mencatat dosis `Held` beralasan | `KEP-15` | `OPEN DECISION` — usulan `G-22`, dikonfirmasi saat approval; **tidak menahan epic** karena alternatifnya hanya mengubah status dosis | Integrasi 8.5 |
| `FR-KEP-077` | SOAP dan Catatan Keperawatan disimpan sebagai CPPT berjenis `NursingSoap`/`NursingNarrative`; menu masing-masing menyaring jenisnya | `KEP-16` | `EXTEND` — kolom dari `dokter-rawat-inap` | `RWI-AC-204`, `205` |
| `FR-KEP-078` | Perawat mencatat obat bawaan; keputusan per obat dibaca | `KEP-16` | `EXTEND` — kontrak `dokter-rawat-inap` | API 7.15 |
| `FR-KEP-079` | Perawat memesan tindakan dengan dokter pemberi instruksi | `KEP-16` | `EXTEND` — kontrak `dokter-rawat-inap` | `INT-DOK-19` |
| `FR-KEP-080` | Penunjang Laboratorium dan Radiologi menampilkan pesanan dan hasil final; pesanan perawat setelah persetujuan pemilik | `KEP-16` | `EXTEND` | `03` 10.4.8 |
| `FR-KEP-081` | Transfer Pasien memakai perpindahan tempat tidur `CAP-017`; serah terima klinis "belum tersedia" | `KEP-16` | `EXISTING / REUSE` | `RWI-DEC-113` |
| `FR-KEP-082` | Ringkasan tagihan baca-saja tanpa harga per item hanya bagi pemegang `PatientBillingSummary : Read` | `KEP-17` | `MISSING / NEW` | API 7.14 |

`FR-KEP-076` berstatus `OPEN DECISION` pada tingkat requirement, bukan epic; `EPIC KEP-15` tetap berdisposisi
`MISSING / NEW` karena kedua jawaban yang mungkin memakai tabel, endpoint, dan layar yang sama.

### 22.11 Model status yang diusulkan

| Mesin | Nilai | Rujukan |
| --- | --- | --- |
| Versi konfigurasi klinis | `Draft`, `Approved`, `Retired` | State 5.1 |
| Dokumen Pengkajian Pasien | Tetap `Draft`, `InProgress`, `Completed`, `Cancelled` + addendum | State 5.2 |
| Evaluasi Awal | `Draft`, `Completed`, `Cancelled` + addendum | State 5.3 |
| Entri terukur | `Active`, `Cancelled` + nomor revisi | State 5.4 |
| Dosis MAR | `Due`, `Administered`, `Held`, `Refused`, `Missed`, `Cancelled`; cek ganda `NotRequired`, `Pending`, `Confirmed`, `Rejected` | State 5.5 |
| Pelaksanaan sliding scale | `Recorded`, `Cancelled` | State 5.6 |

### 22.12 Sasaran arsitektur

| Sasaran | Bentuknya |
| --- | --- |
| Nol tabel milik Rawat Inap | Sebelas tabel `ClinicalManagement`, lima `PharmacyManagement` |
| Satu fakta satu tempat | `INV-KEP-04` |
| Nol angka batas klinis di source | `CliClinicalInstrumentVersion` |
| Logika baru di service, bukan controller | `NursingAssessmentDocumentService` walau `PatientAssessmentController` hari ini berlogika di controller |
| Satu transaksi untuk tulis berantai | `INT-KEP-09`, `11`, `15` |

### 22.13 Sasaran kemampuan API

Sepuluh grup baru dan tiga grup diperluas — `contracts/api-contract.md` bagian 7. Seluruh endpoint baru berlabel
**Rencana (belum tersedia)**.

### 22.14 Matriks kewenangan

`contracts/permission-audit-matrix.md` bagian 6 dan `03-frontend-architecture.md` bagian 10.5. Sepuluh Resource baru,
sembilan di antaranya milik modul yang endpoint-nya dirancang di sini.

### 22.15 Batas integrasi dan billing

| Hal | Keputusan |
| --- | --- |
| MAR, pelaksanaan sliding scale, Pengawasan Harian | **Bukan** kejadian tagihan — `BR-RWI-013` |
| Tagihan pemeriksaan GDS bangsal | Tidak dirancang — `G-28` |
| Ringkasan tagihan | Dibaca dari Billing, tidak disalin, tidak dihitung ulang — `RWI-DEC-137` (4) |
| Integrasi luar | Tidak ada |

### 22.16 Guardrail regulasi

| Kewajiban | Bentuknya pada MVP |
| --- | --- |
| Rekam pemberian obat tidak dihapus dan tidak ditimpa | Revisi; tanpa `DELETE` |
| Cek ganda obat high-alert | `INV-KEP-05`; daftar high-alert gerbang produksi `RWI-DEC-116` |
| Instrumen klinis disahkan pemilik klinis | Gerbang produksi; pemilik belum ditunjuk (`RWI-OQ-056`, `057`) |
| Protokol sliding scale disahkan pemilik klinis | Gerbang produksi `RWI-DEC-146` — dirancang `dokter-rawat-inap` |
| Evaluasi Awal oleh MPP | `RWI-DEC-118`, `131` |
| Kolom sensitif tidak masuk log | Permission 6.5 |

### 22.17 Kebutuhan non-fungsional

| ID | Kebutuhan | Cara membuktikan |
| --- | --- | --- |
| `NFR-018` | Pembentukan dosis, pelaksanaan sliding scale, penghentian butir, dan penutupan episode tidak meninggalkan perubahan parsial | Galat buatan di tengah transaksi → nol perubahan |
| `NFR-019` | Kiriman ulang dengan kunci sama tidak menggandakan dosis, GDS, entri cairan, maupun pelaksanaan; dua permintaan paralel diuji pada PostgreSQL sungguhan | Dua permintaan → satu baris |
| `NFR-020` | MAR satu hari untuk 20 butir dan 3 hari rawat terbuka paling lama 2 detik pada p95 di lingkungan uji termasuk pembentukan dosis — **angka usulan desain**, dikonfirmasi saat approval | Pengukuran pada laporan task |
| `NFR-021` | Hosted service pembentukan dosis tidak berhenti karena galat satu episode | Episode rusak buatan → episode lain tetap terbentuk |
| `NFR-022` | Waktu klinis disimpan UTC dan ditampilkan `Asia/Jakarta`; hari Pengawasan Harian mengikuti jam shift pertama unit | Entri 06.30 masuk hari sebelumnya bila shift pertama 07.00 |
| `NFR-023` | Seluruh penjaga unit, MPP, cek ganda, dan pengesah diuji dengan peran nyata non-SuperAdmin | Laporan task mencatat peran uji |
| `NFR-024` | Kolom sensitif baru tidak masuk custom logger | Pemeriksaan payload log |

### 22.18 Skenario UAT

| ID | Epic | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-KEP-15` | `KEP-09` | Berhasil | Siti membuka Budi; delapan menu tampil; Bank Darah "Integrasi belum tersedia" | Tanpa data contoh |
| `UAT-KEP-16` | `KEP-09` | Gagal | Server episode mati saat membuka | Tombol tulis nonaktif pada seluruh menu |
| `UAT-KEP-17` | `KEP-10` | Berhasil | Andi menyusun Morse Dewasa v2; Wati mengesahkan | Versi sah; versi lama pensiun |
| `UAT-KEP-18` | `KEP-10` | Gagal | Andi mengesahkan versinya sendiri | Ditolak |
| `UAT-KEP-19` | `KEP-11` | Berhasil | Siti menyelesaikan Kajian Umum dan Resiko Jatuh skor 50 | Progres 40%; alert Risiko Jatuh Tinggi di kepala konteks |
| `UAT-KEP-20` | `KEP-11` | Gagal | Produksi tanpa instrumen sah; Siti menekan Selesaikan | Ditolak; konsep tetap tersimpan |
| `UAT-KEP-21` | `KEP-11` | Gagal | Siti mengetik skala nyeri pada input tanda vital | Isian tidak tersedia; permintaan langsung ditolak |
| `UAT-KEP-22` | `KEP-12` | Berhasil | Dewi MPP Melati menyelesaikan Evaluasi Awal Budi | Siti membaca tanpa tombol ubah |
| `UAT-KEP-23` | `KEP-12` | Gagal | Dewi menulis untuk pasien Anggrek | Ditolak |
| `UAT-KEP-24` | `KEP-13` | Berhasil | Siti mencatat intake dan output sehari | Balance +550 ml dan per shift sesuai |
| `UAT-KEP-25` | `KEP-13` | Gagal | Siti menautkan dosis Ceftriaxone yang sudah punya entri | Ditolak; total tetap |
| `UAT-KEP-26` | `KEP-13` | Gagal | Siti menyimpan GDS tanpa satuan | Ditolak |
| `UAT-KEP-27` | `KEP-14` | Berhasil | Siti mencatat Ceftriaxone 08.05; Rina mengonfirmasi insulin | Dosis tercatat; cek ganda oleh Rina |
| `UAT-KEP-28` | `KEP-14` | Gagal | Siti mengonfirmasi insulin catatannya sendiri | Ditolak |
| `UAT-KEP-29` | `KEP-14` | Gagal | Siti menahan dosis tanpa alasan; menekan simpan dua kali | Ditolak tanpa alasan; dengan alasan hanya satu baris |
| `UAT-KEP-30` | `KEP-14` | Berhasil | dr. Rina menghentikan Amlodipin 14.00 | Dosis 20.00 dibatalkan; 08.00 tetap |
| `UAT-KEP-31` | `KEP-15` | Berhasil | GDS 280 mg/dL, rentang `[250,300)` | 3 unit tercatat sekali di MAR, GDS sekali di Pengawasan Harian |
| `UAT-KEP-32` | `KEP-15` | Gagal | Order dihentikan; Siti mencatat pelaksanaan | Ditolak; nol GDS tersimpan dari percobaan itu |
| `UAT-KEP-33` | `KEP-15` | Gagal | Siti memilih GDS laboratorium | Tidak tersedia sebagai pilihan; permintaan langsung ditolak |
| `UAT-KEP-34` | `KEP-16` | Berhasil | Siti menulis SOAP dan Catatan Keperawatan | Masing-masing tampil di menunya; keduanya di Catatan Terintegrasi |
| `UAT-KEP-35` | `KEP-16` | Gagal | Siti memindahkan Budi ke ICU lalu mencari serah terima klinis | Perpindahan tercatat; serah terima "Integrasi belum tersedia" |
| `UAT-KEP-36` | `KEP-17` | Gagal | Rudi tanpa hak membuka Tagihan Pasien | "Anda tidak punya akses" |

### 22.19 Definition of Done penyelarasan

| Butir | Bukti |
| --- | --- |
| Delapan menu tampil dengan isi nyata atau "belum tersedia" jujur | `AC-KEP-130`; tangkapan layar bertopeng |
| Nol angka batas risiko jatuh dan nol isian wajib tetap di source | `AC-KEP-054`, `AC-KEP-061` |
| Pengesah ≠ pengubah; hasil menyimpan versi | `AC-KEP-051`, `057`, `058` |
| Progres ✓/!/○ dan galat sesuai `RWI-DEC-119` | `AC-KEP-069` s.d. `073` |
| Evaluasi Awal hanya MPP unit | `AC-KEP-074` s.d. `078` |
| Balance terhitung dan tanpa hitung ganda obat | `AC-KEP-080` s.d. `085` |
| MAR idempoten, cek ganda perawat lain, koreksi beriwayat | `AC-KEP-094` s.d. `108` |
| Penghentian butir dan penutupan episode membatalkan dosis yang benar | `AC-KEP-105`, `AC-KEP-113` |
| Sliding scale satu transaksi, satu dosis, hanya GDS bangsal | `AC-KEP-115` s.d. `128` |
| Nol tabel MAR/sliding scale/dokumentasi di `ClinicalManagement` yang salah atau `InPatientManagement` | `AC-KEP-114`, `128`; architecture test bagian 8 |
| Regresi poliklinik dan IGD | `AC-KEP-065`, `068`; pemberitahuan kepada pemilik `rawat-jalan` sebelum K2 tercatat |
| Konfigurasi awal terisi di lingkungan uji | `02-backend-architecture.md` 11.10 |
| Butir yang tertahan pemilik lain ditulis `NOT RUN` apa adanya | Laporan task |

### 22.20 Urutan pengiriman dan pertanyaan terbuka

Awalan `KEP-V2-` supaya tidak bertabrakan dengan `KEP-MVP-*` dan `Gelombang 1A`.

| Gelombang | Isinya | Syarat mulai |
| --- | --- | --- |
| `KEP-V2-0` | `FR-KEP-038` perbaikan tanpa bentuk data (migration K0) | Blueprint revision `7` disetujui |
| `KEP-V2-1` | `EPIC KEP-10`, `EPIC KEP-11`, `EPIC KEP-12`, dan `EPIC KEP-09` — migration K1–K3 | `KEP-V2-0`; pemberitahuan kepada pemilik `rawat-jalan` sebelum K2 |
| `KEP-V2-2` | `EPIC KEP-13`, `EPIC KEP-14`, `EPIC KEP-15` — migration K4–K7 | `KEP-V2-0`; **dirilis bersama `DOK-V2-2`**; `episode-rawat-inap` `0.9.0` langkah penutupan `INT-KEP-15` |
| `KEP-V2-3` | `EPIC KEP-16` | `DOK-V2-1` (jenis catatan, pesanan); bagian Lab/Rad setelah persetujuan pemiliknya |
| `KEP-V2-4` | `EPIC KEP-17` | Kontrak Billing disetujui |
| `POST-MVP` | Seluruh baris 22.8 | Keputusan pemilik masing-masing |

**Nol epic `OPEN DECISION` di dalam gelombang.** Pertanyaan bernomor melanjutkan bagian 20.

| No | Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| ---: | --- | --- | --- | :---: |
| 5 | Frekuensi Evaluasi Awal — usulan satu dokumen hidup per episode dilengkapi addendum (`G-09`) | Muhammad Hamzah | Unique parsial per episode; bila berulang, hanya index yang berubah | Tidak — dikonfirmasi saat approval |
| 6 | Rentang 0 unit → dosis `Held` beralasan (`G-22`) — sama dengan `dokter-rawat-inap` 22.20 nomor 7 | Muhammad Hamzah | `FR-KEP-076` | Tidak — dikonfirmasi saat approval |
| 7 | Satuan GDS rumah sakit (`G-25`) — usulan disimpan eksplisit, tanpa konversi | Muhammad Hamzah / pemilik klinis | Satuan dipilih setiap pencatatan | Tidak |
| 8 | Entri intake yang dosisnya dikoreksi ditandai, tidak diubah (`G-26`); pengingat dosis tanpa entri tidak mewajibkan (`G-27`) | Muhammad Hamzah | `FR-KEP-063` | Tidak |
| 9 | Dosis `Due` yang terlewat saat perawatan ditutup tetap `Due` hanya-baca "Tidak dicatat sebelum perawatan ditutup" | Muhammad Hamzah | `INT-KEP-15`; alternatifnya dibatalkan sistem dengan penanda — tabel sama | Tidak |
| 10 | Jam shift bawaan Pagi 07–14, Siang 14–21, Malam 21–07 (`G-06`) | Kepala keperawatan | Seeder usulan, dapat diubah admin | Tidak |
| 11 | Jam standar frekuensi, jendela lewat waktu, interval PRN (`G-12`, `G-14`) | Farmasi / pemilik klinis | Konfigurasi kosong: dosis berfrekuensi tanpa jadwal tidak terbentuk | Tidak untuk desain; ya untuk pemakaian pasien sungguhan |
| 12 | Penerima notifikasi aktif dugaan reaksi obat (`G-15`) | Pemilik klinis | Tampil pada alert dan daftar pantau saja | Tidak |
| 13 | Pengesah isi instrumen risiko jatuh, nyeri, formulir, checklist MPP, dan isian wajib (`RWI-OQ-056`, `057`, `G-02`, `G-04`, `G-07`) | Manajemen rumah sakit menunjuk pemilik klinis | Dokumen tidak dapat diselesaikan di produksi | **Gerbang produksi** |
| 14 | Persetujuan Yoga Aji Pratama atas jenis dokumen `CaseManagementEvaluation = 14` (`INT-KEP-12`) | Yoga Aji Pratama | Addendum dan penguncian Evaluasi Awal tertahan | Tidak untuk desain |
| 15 | Kontrak ringkasan tagihan dari pemilik Billing (`INT-KEP-14`) | Pemilik `BillingManagement` | `KEP-V2-4` tertahan | Tidak untuk desain; ya untuk `KEP-V2-4` |
| 16 | Integrasi Gizi, Bank Darah, Kamar Operasi dibuka sekarang karena modulnya ada di source — sama dengan `dokter-rawat-inap` nomor 12 | Muhammad Hamzah lewat `grill-me` | Tetap "Integrasi belum tersedia" | Tidak |
| 17 | Daftar obat high-alert termasuk insulin disahkan (`G-13`) | Pemilik klinis / Farmasi | Cek ganda tidak diminta untuk obat yang belum ditandai | **Gerbang produksi** |
