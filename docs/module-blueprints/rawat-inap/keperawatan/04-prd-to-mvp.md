# PRD ke MVP — Sub-modul `keperawatan` (Rawat Inap)

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement` |
| Sub-modul | `keperawatan`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.1.0` |
| Revision artefak | `0.1` |
| Status | `draft` — **belum disetujui manusia** |
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

### `EPIC KEP-06` — Pemakaian alat — **`OPEN DECISION`**

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-KEP-027` | Pemakaian alat pada pasien tercatat beserta waktu mulai, selesai, dan penanggung jawabnya | **OPEN DECISION** |
| `FR-KEP-028` | Pemakaian alat yang masih berjalan ikut terbawa saat pasien pindah kamar | **OPEN DECISION** |

> **`EPIC KEP-06` MUST NOT masuk gelombang pengiriman mana pun.** Kepemilikan tabelnya belum
> diputuskan: `PRD-RWI-FINAL-001` bagian 23.1 memuat 28 baris *source of truth* dan **tidak satu
> pun** menyebut Equipment Usage. Memulai pekerjaannya sekarang berarti membangun di atas
> kepemilikan yang dikarang, lalu membongkarnya ketika keputusannya turun.

---

## 11. Model status

Diturunkan dari [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md).

| Mesin | Nilai |
| --- | --- |
| Pengkajian | `Draft`, `InProgress`, `Completed`, `Cancelled`, `Amended` |
| Butir rencana asuhan | `Active`, `Resolved`, `Discontinued` |
| Catatan tindakan | `Recorded`, `Finalized`, `Amended` |
| Pengiriman tagihan | `NotApplicable`, `Pending`, `Dispatched`, `Failed` |

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
| Persediaan | **Tidak ada integrasi pada MVP** — `CAP-016` `OPEN DECISION` |

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
| `UAT-KEP-05` | **Berhasil** | Kepala ruangan mengamandemen pengkajian yang salah isi | Status `Amended`; versi lama tetap terbaca beserta alasannya |
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
| **Tidak masuk gelombang mana pun** | **`EPIC KEP-06` pemakaian alat** | `OPEN DECISION` — kepemilikan tabelnya belum diputuskan |

---

## 20. Pertanyaan terbuka sebelum development lock

| No | Pertanyaan | Pemilik | Memblokir? |
| ---: | --- | --- | :---: |
| 1 | **Siapa pemilik tabel catatan pemakaian alat?** Persediaan, `ClinicalManagement`, atau `InPatientManagement`. PRD 23.1 tidak memuat barisnya. Usulan ID `RWI-OQ-048` | Product/Domain bersama pemilik persediaan | **Ya** — untuk `CAP-016` saja |
| 2 | Apakah rumah sakit memakai terminologi SDKI/SLKI/SIKI? Menentukan perlu-tidaknya katalog terminologi berversi | Clinical governance | Tidak — struktur rencana asuhan tetap dapat dibangun |
| 3 | Berapa batas waktu pengkajian awal dan pengkajian ulang? `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | Tidak — mekanismenya dibangun, angkanya menyusul |
| 4 | Apakah catatan keperawatan tampil pada catatan terpadu bagi seluruh profesi? PRD `CAP-014` aturan 4 menyebut "sesuai kebijakan" tanpa menyebut kebijakannya | Clinical governance | Tidak — catatan tetap tersimpan dan terbaca dari ruang kerja |

> **Hanya pertanyaan 1 yang memblokir, dan hanya untuk `CAP-016`.** Ketiga kemampuan `MUST HAVE`
> tidak tertahan olehnya.
>
> Dokumen ini boleh berstatus `draft` dengan pertanyaan memblokir terbuka, tetapi **MUST NOT**
> diteruskan ke `/qv-plan` sebelum pertanyaan 1 dijawab **atau** `EPIC KEP-06` dikeluarkan dari
> scope secara tertulis.
