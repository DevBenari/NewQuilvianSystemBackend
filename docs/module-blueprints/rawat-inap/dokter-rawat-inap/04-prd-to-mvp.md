# PRD ke MVP — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement` |
| Sub-modul | `dokter-rawat-inap`, bentuk `COMPOSITE` sejak `RWI-DEC-082` |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.1.0` |
| Revision artefak | `0.1` |
| Status | `draft` — **belum disetujui manusia** |
| Repository target | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Baseline requirement | `PRD-RWI-FINAL-001` v1.0.0 bagian 18, 19, 23.1, 30.3 |
| Ditulis paling akhir | Ya — menurunkan dari arsitektur dan kelima kontrak |

---

## 2. Ringkasan eksekutif

Sub-modul ini memberi dokter **satu tempat untuk mengerjakan seluruh dokumentasi pasien rawat
inap**: kajian medis awal, catatan perkembangan harian, catatan terpadu, visite, resep, tindakan,
dan pemeriksaan penunjang.

**Yang membuatnya murah:** enam dari tujuh kemampuan berdiri di atas tabel yang **sudah ada dan
sudah dipakai** poliklinik serta IGD. SOAP sudah ada di dalam konsultasi; resep punya mesin
lengkap di Farmasi; laboratorium sudah berjalan. Hanya **satu** tabel yang benar-benar baru:
catatan visite.

**Yang menahannya:** dua pelonggaran pada mesin klinis yang keputusannya **sudah turun sejak
2026-08-21** tetapi kodenya belum ada.

---

## 3. Masalah produk

| Masalah | Akibatnya hari ini |
| --- | --- |
| Konsultasi rawat inap tidak dapat dibuat tanpa nomor antrean | Dokter tidak dapat menulis SOAP, diagnosis, resep, maupun tindakan untuk pasien rawat inap |
| Batas satu konsultasi per kunjungan | Bahkan bila pintunya dibuka, dokter hanya dapat menulis **satu** catatan untuk seluruh masa perawatan |
| Batas satu resep aktif per konsultasi | Pasien yang dirawat sepuluh hari hanya dapat menerima satu resep |
| Tidak ada catatan visite di mana pun | Visite dokter tidak dapat dibuktikan, dan `RWI-DEC-025` yang sudah mendefinisikannya tidak punya tempat menyimpan |
| Verifikasi CPPT tidak dapat dicatat | Catatan terpadu ada, tetapi tidak ada cara menandai DPJP sudah memeriksanya |

---

## 4. Visi produk

DPJP membuka pasiennya dari census, menulis kajian medis awal tanpa nomor antrean, mencatat
visitenya setiap hari sebagai peristiwa tersendiri, menulis perkembangan, meresepkan obat berkali-
kali sepanjang perawatan, memesan pemeriksaan lab, dan memverifikasi catatan perawat — seluruhnya
di dalam konteks episode, **tanpa satu tabel tandingan pun**.

---

## 5. Batas MVP

| Batas | Isinya |
| --- | --- |
| **Titik mulai** | Pasien sudah dikonfirmasi tiba di kamar — episode `Admitted` |
| **Titik akhir** | Kajian medis, SOAP harian, CPPT beserta verifikasinya, visite, resep, tindakan, dan pemesanan lab tercatat; supervisor dapat melihat verifikasi yang tertunggak |
| **Di luar batas** | Radiologi, resume pulang, dan seluruh dokumentasi keperawatan |

### 5.1 Pelaku sasaran

| Pelaku | Yang dikerjakannya |
| --- | --- |
| DPJP | Semuanya, **termasuk verifikasi CPPT** |
| Dokter jaga ruangan | Semuanya kecuali verifikasi dan kajian medis awal |
| Dokter konsulen | Membaca, menulis CPPT, mencatat visite |
| Perawat | Membaca catatan dokter; menulis CPPT dari ruang kerjanya sendiri |

---

## 6. Kemampuan `MUST HAVE`

| Kemampuan | ID | Asal | Epic |
| --- | --- | --- | --- |
| Kajian medis awal | `CAP-022` | PRD bagian 18 | `EPIC DOK-02` |
| Dokumentasi SOAP | `CAP-020` | PRD bagian 18 | `EPIC DOK-03` |
| CPPT beserta verifikasinya | `CAP-021` | PRD bagian 18 | `EPIC DOK-04` |
| Resep rawat inap dan obat pulang | `CAP-023` | PRD bagian 19 | `EPIC DOK-06` |
| Tindakan dokter | `CAP-024` | PRD bagian 19 | `EPIC DOK-06` |
| Pencatatan visite | `CAP-025` | PRD bagian 19 | `EPIC DOK-05` |

Enam kemampuan `MUST HAVE`. Kepemilikan datanya **seluruhnya tegas** pada PRD 23.1 — tidak ada
`OPEN DECISION` kepemilikan pada sub-modul ini.

---

## 7. Prasyarat yang menahan seluruh MVP

| No | Prasyarat | Pemilik | Keadaan |
| ---: | --- | --- | --- |
| 1 | **`INT-DOK-01`** — cabang episode pada validasi **konsultasi** | `ClinicalManagement` | Disetujui `RWI-DEC-062`; **belum dikerjakan** |
| 2 | **`INT-DOK-02`** — pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif | `ClinicalManagement`, `PharmacyManagement` | `approved` sejak `RWI-DEC-038`, diperluas `RWI-DEC-070`; **belum dikerjakan** |
| 3 | **`INT-KEP-01`** — cabang episode pada validasi **pengkajian**, milik `keperawatan` | `ClinicalManagement` | Sama. **Wajib dikerjakan bersama butir 1** |

> **Butir 1 dan 3 adalah satu pekerjaan yang kebetulan berada di dua berkas.** Mengerjakan salah
> satunya saja menghasilkan setengah ruang kerja klinis: perawat dapat mencatat tetapi dokter
> tidak, atau sebaliknya. Rinciannya di
> [`contracts/integration-contract.md`](./contracts/integration-contract.md) bagian 1.1.

---

## 8. Kemampuan yang ditunda

| Kemampuan | ID | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Pemeriksaan **radiologi** | bagian `CAP-015` | **Modulnya tidak ada** di repository. Membuat tabelnya di sini berarti mengarang kepemilikan yang PRD 23.1 taruh pada modul Radiologi | Radiologi dipesan di luar sistem sebagaimana hari ini. **Laboratorium tetap masuk MVP penuh** |
| Nilai batas waktu verifikasi CPPT | bagian `CAP-021` | PRD aturan 4 menuntut angkanya berasal dari Clinical Governance, dan itu belum turun | **Mekanismenya tetap dibangun.** Kebijakan kosong berarti verifikasi `NotRequired`; pencatatan berjalan penuh |
| Nilai batas waktu kajian medis | bagian `CAP-022` | `RWI-RULE-021` menunggu pemilik klinis | Sama — mekanismenya siap, angkanya menyusul |
| *Administrative attestation* visite | bagian `CAP-025` | PRD aturan 4 membuka kemungkinannya, tetapi kebijakannya belum ada | **Bawaan yang aman:** hanya dokter yang dapat mencatat visite |
| Penandaan otomatis obat pulang sudah diserahkan | bagian `CAP-023` | `INV-DOK-04` melarang sub-modul ini menandai sendiri; pembacaan balik dari Farmasi adalah pekerjaan tersendiri | Butir daftar periksa administrasi tetap **ditandai manual** petugas admisi, seperti `RWI-DEC-033` |

---

## 9. Alur bisnis target

`FLOW-DOK-MVP-001`, diturunkan dari [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md):

```text
Pasien Admitted → DPJP ditetapkan → dokter buka ruang kerja
  → kajian medis awal Completed → catat visite → catatan perkembangan
  → pesan penunjang bila perlu → buat resep → catat tindakan
  → verifikasi catatan terpadu → (keputusan pulang milik episode-rawat-inap)
```

---

## 10. Epic dan functional requirement

### `EPIC DOK-01` — Pintu masuk dokumentasi dokter dibuka

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-001` | Konsultasi dapat dibuat bagi encounter yang punya episode `Admitted`, tanpa antrean dan tanpa kunjungan IGD | **EXTEND** |
| `FR-DOK-002` | Konsultasi **kedua dan seterusnya** pada satu kunjungan rawat inap diterima | **EXTEND** |
| `FR-DOK-003` | Resep **kedua dan seterusnya** pada satu konsultasi rawat inap diterima | **EXTEND** |
| `FR-DOK-004` | Perilaku rawat jalan dan medical check-up **tidak berubah sedikit pun** | **EXISTING / REUSE** — dijaga test regresi |
| `FR-DOK-005` | Jalur konsultasi IGD terbukti tidak rusak | **EXISTING / REUSE** — dijaga test regresi |

### `EPIC DOK-02` — Kajian medis awal

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-006` | Dokter membuat kajian medis pada episode `Admitted` | **EXTEND** |
| `FR-DOK-007` | Kajian medis dan SOAP punya record serta lifecycle **berbeda** | **EXTEND** |
| `FR-DOK-008` | SOAP harian **tidak menimpa** kajian medis final | **EXTEND** |
| `FR-DOK-009` | Diagnosis dan daftar masalah tersimpan terstruktur, bukan teks di dalam SOAP | **EXISTING / REUSE** |
| `FR-DOK-010` | Amandemen kajian medis mempertahankan versi asli | **MISSING / NEW** |
| `FR-DOK-011` | Perawat **tidak dapat** membuat kajian medis | **MISSING / NEW** |

### `EPIC DOK-03` — Catatan perkembangan harian

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-012` | Beberapa SOAP sepanjang episode tersimpan sebagai lini masa | **EXTEND** |
| `FR-DOK-013` | Waktu klinis terpisah dari waktu penulisan, dan lini masa terurut waktu klinis | **MISSING / NEW** |
| `FR-DOK-014` | SOAP dapat dibuat walaupun pengkajian awal keperawatan **belum selesai** | **MISSING / NEW** |
| `FR-DOK-015` | Episode `Closed` menolak SOAP baru tetapi **menerima amandemen** catatan lama | **MISSING / NEW** |
| `FR-DOK-016` | Amandemen tidak mengaktifkan kembali episode, tidak membuka tempat tidur, tidak mengubah lama dirawat | **MISSING / NEW** |

### `EPIC DOK-04` — Catatan terpadu dan verifikasi

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-017` | Catatan dokter dan perawat tampil sebagai entry terpisah dengan penulis dan profesi masing-masing | **EXISTING / REUSE** |
| `FR-DOK-018` | DPJP dapat memverifikasi catatan; **penulis aslinya tidak berubah** | **MISSING / NEW** |
| `FR-DOK-019` | Verifikasi hanya oleh DPJP episode itu | **MISSING / NEW** |
| `FR-DOK-020` | Keterlambatan verifikasi terpantau menurut kebijakan aktif dan **tidak menahan** pekerjaan | **MISSING / NEW** |
| `FR-DOK-021` | Kebijakan verifikasi kosong berarti tidak ada yang menunggu verifikasi | **MISSING / NEW** |
| `FR-DOK-022` | Amandemen catatan terverifikasi mengembalikannya ke menunggu verifikasi | **MISSING / NEW** |

### `EPIC DOK-05` — Visite sebagai peristiwa

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-023` | Visite tercatat beserta waktu, dokter, peran, dan pencatatnya | **MISSING / NEW** |
| `FR-DOK-024` | **SOAP tanpa visite eksplisit tidak menambah hitungan visite** | **MISSING / NEW** |
| `FR-DOK-025` | Visite muncul di riwayat walaupun catatannya ditulis kemudian | **MISSING / NEW** |
| `FR-DOK-026` | Pengiriman berulang tidak melahirkan visite ganda | **MISSING / NEW** |
| `FR-DOK-027` | Visite kedua pada jam berdekatan **diperingatkan, bukan ditolak** | **MISSING / NEW** |
| `FR-DOK-028` | Hanya pengguna berkewenangan dokter yang dapat mencatat visite | **MISSING / NEW** |

### `EPIC DOK-06` — Resep, tindakan, dan penunjang

| No | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-DOK-029` | Resep dibuat dari konteks rawat inap | **EXTEND** |
| `FR-DOK-030` | **Obat pulang menjadi jenis order eksplisit** yang dapat dibedakan dari resep harian | **EXTEND** |
| `FR-DOK-031` | Status pemenuhan resep dapat **dibaca**; sub-modul ini tidak pernah menulisnya | **EXISTING / REUSE** |
| `FR-DOK-032` | Pengiriman resep berulang tidak melahirkan resep ganda | **EXTEND** |
| `FR-DOK-033` | Tindakan membedakan direncanakan dari dilakukan | **EXTEND** |
| `FR-DOK-034` | Kegagalan tagihan **tidak menghilangkan** catatan tindakan | **EXTEND** |
| `FR-DOK-035` | Pemesanan lab membawa konteks episode, dan pesanan episode A tidak dapat diproses sebagai milik episode B | **EXTEND** |
| `FR-DOK-036` | Hasil lab terverifikasi terbaca **tanpa baris salinan** yang menjadi kebenaran baru | **EXISTING / REUSE** |

**Tidak ada epic berstatus `OPEN DECISION` pada sub-modul ini.**

---

## 11. Model status

| Mesin | Nilai |
| --- | --- |
| Konsultasi dan SOAP | `Draft`, `InProgress`, `Completed`, `Cancelled`, `Amended` |
| Kajian medis | Sama — tabel yang sama, pembeda `AssessmentType` |
| Verifikasi CPPT | `NotRequired`, `Pending`, `Verified`, `Overdue` |
| Catatan tindakan | `Ordered`, `Performed`, `Cancelled`, `Amended` |
| Pengiriman tagihan | `NotApplicable`, `Pending`, `Dispatched`, `Failed` |
| Jenis order resep | `Routine`, `Daily`, `Discharge` |

**Nol status episode baru** — `RWI-DEC-009`.

---

## 12. Sasaran arsitektur

| Sasaran | Isinya |
| --- | --- |
| Tabel baru milik Rawat Inap | **Nol** |
| Tabel baru milik modul lain | **Satu** — `TrxPhysicianVisit`, milik `ClinicalManagement` |
| Kolom baru | 3 pada konsultasi, 8 pada CPPT, 5 pada tindakan, 3 pada resep, 1 pada pesanan lab |
| Perubahan perilaku pada modul lain | **Dua** — `INT-DOK-01` dan `INT-DOK-02` |
| Endpoint baru | 17 rencana |

---

## 13. Matriks kewenangan

Diturunkan dari [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md).
Resource baru: `PhysicianVisit`. Action baru: `Amend`, `Verify`.

> **Peringatan `BE-RWI-034`.** Resource dan Action baru wajib memakai nama yang sama persis pada
> `[AccessAction]` dan `[AccessPermission]`, dan wajib diuji dengan peran non-SuperAdmin.

---

## 14. Batas integrasi dan billing

| Batas | Isinya |
| --- | --- |
| Farmasi | Resep dikirim; **status pemenuhan hanya dibaca** — `INV-DOK-04` |
| Laboratorium | Pesanan dikirim; **hasil hanya dibaca** — `INV-DOK-05` |
| Radiologi | **Tidak ada integrasi pada MVP** — modulnya belum ada |
| Billing | Pemicu tagihan tindakan beserta kunci idempotency; kegagalannya tidak menghilangkan catatan klinis |

---

## 15. Guardrail regulasi

| Kewajiban | Yang dipenuhi MVP | Yang belum |
| --- | --- | --- |
| Rekam medis elektronik | Kajian medis, SOAP, CPPT, visite, resep, dan tindakan tersimpan lengkap beserta pelaku dan waktunya | Radiologi |
| Keterlacakan | Setiap amandemen dan setiap verifikasi menyimpan aktor, waktu, dan alasan | — |
| Pemisahan penulis dan penyetuju | `VerifiedByUserId` terpisah dari `ProviderUserId` | — |
| Koreksi rekam medis | Amandemen beralasan; versi lama tidak pernah hilang; episode tertutup tetap dapat dikoreksi tanpa dibuka kembali | — |
| Masa simpan | — | `RWI-OQ-035` menunggu pemilik hukum |
| Batas waktu klinis | Mekanisme pemantauannya siap dan berversi | Angkanya — `RWI-RULE-021` dan kebijakan verifikasi CPPT |

---

## 16. Kebutuhan non-fungsional

| Kebutuhan | Sasaran |
| --- | --- |
| Ruang kerja terbuka | Konteks pasien dan penanda alergi tampil sebelum satu pun tombol tulis aktif |
| Idempotency | Wajib pada visite, resep, dan tindakan |
| Concurrency | Ketiga unique index parsial diuji terhadap **PostgreSQL sungguhan**, bukan InMemory |
| Privasi | Kolom sensitif tidak masuk logger dan tidak tampil pada daftar ringkas |
| Regresi | Setiap task yang menyentuh mesin klinis membawa test regresi poliklinik dan IGD — `RWI-DEC-051` |

---

## 17. Skenario UAT

### `EPIC DOK-01`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-01` | **Berhasil** | dr. Andi membuka Tn. Budi yang sudah di kamar, lalu menulis catatan | Tersimpan tanpa diminta nomor antrean |
| `UAT-DOK-02` | **Berhasil** | dr. Andi menulis catatan kedua keesokan harinya | Dua catatan pada satu kunjungan; keduanya tersimpan |
| `UAT-DOK-03` | **Gagal** | Dokter poliklinik menulis konsultasi tanpa antrean | Ditolak seperti sebelumnya — **perilaku poliklinik tidak berubah** |

### `EPIC DOK-02`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-04` | **Berhasil** | dr. Andi menulis kajian medis awal, lalu tiga hari menulis SOAP harian | Isi kajian medis awal **sama persis** seperti hari pertama |
| `UAT-DOK-05` | **Gagal** | Menyelesaikan kajian medis tanpa diagnosis | Ditolak; bagian yang kosong disebut |
| `UAT-DOK-06` | **Gagal** | Ns. Sari mencoba menulis kajian medis | Ditolak: hanya dokter |

### `EPIC DOK-03`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-07` | **Berhasil** | dr. Andi visite pukul 07.00, menulis catatannya pukul 11.00 | Lini masa menempatkannya pada pukul 07.00 |
| `UAT-DOK-08` | **Berhasil** | dr. Andi menulis SOAP walaupun pengkajian awal perawat belum selesai | Tersimpan; tidak ada penolakan |
| `UAT-DOK-09` | **Berhasil** | Catatan pada episode yang sudah ditutup dibetulkan lewat amandemen | Tersimpan; episode **tetap** `Closed`, tempat tidur tidak berubah |
| `UAT-DOK-10` | **Gagal** | Menulis SOAP **baru** pada episode yang sudah ditutup | Ditolak, dan pesan menyebutkan koreksi tetap bisa |

### `EPIC DOK-04`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-11` | **Berhasil** | dr. Andi memverifikasi catatan Ns. Sari | Terverifikasi; **nama Ns. Sari tetap sebagai penulis**, dr. Andi tercatat sebagai verifikator |
| `UAT-DOK-12` | **Berhasil** | Supervisor membuka daftar pantau saat kebijakan verifikasi belum diisi | Berbunyi "verifikasi tidak diwajibkan", **bukan** daftar kosong yang menyesatkan |
| `UAT-DOK-13` | **Gagal** | dr. Rina yang bukan DPJP mencoba memverifikasi | Ditolak |

### `EPIC DOK-05`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-14` | **Berhasil** | dr. Andi mencatat visite, menulis SOAP-nya sepuluh menit kemudian | Visite muncul di riwayat sejak dicatat |
| `UAT-DOK-15` | **Berhasil** | dr. Andi menulis tiga SOAP tanpa mencatat visite | Riwayat visite **tetap kosong** — dan itu benar |
| `UAT-DOK-16` | **Berhasil** | Koneksi lambat, tombol catat visite tertekan dua kali | **Satu** visite tercatat |
| `UAT-DOK-17` | **Gagal** | Ns. Sari mencoba mencatat visite atas nama dr. Andi | Ditolak |

### `EPIC DOK-06`

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-DOK-18` | **Berhasil** | dr. Andi meresepkan setiap hari selama lima hari | Lima resep tersimpan pada satu perawatan |
| `UAT-DOK-19` | **Berhasil** | dr. Andi membuat resep obat pulang | Tersaring tersendiri sebagai obat pulang, terbedakan dari resep harian |
| `UAT-DOK-20` | **Berhasil** | Tindakan dicatat saat sistem tagihan sedang mati | Catatan klinis tersimpan; penanda pengiriman gagal |
| `UAT-DOK-21` | **Gagal** | dr. Andi mencoba menandai obat sudah diserahkan | Ditolak: hanya petugas Farmasi |
| `UAT-DOK-22` | **Gagal** | Pesanan lab episode A dibuka dari episode B | Ditolak |

---

## 18. Definition of Done

| No | Butir | Bukti |
| ---: | --- | --- |
| 1 | `INT-DOK-01` terpasang; konsultasi rawat inap dapat dibuat tanpa antrean | `AC-CAP022-01`, `AC-CAP023-01` hijau |
| 2 | `INT-DOK-02` terpasang; konsultasi dan resep kedua diterima | Test `RWI-RULE-026` aturan 4 dan 5 hijau |
| 3 | Perilaku poliklinik dan medical check-up terbukti tidak berubah | Test regresi `FR-DOK-004` hijau |
| 4 | Jalur konsultasi IGD terbukti tidak rusak | Test regresi `FR-DOK-005` hijau |
| 5 | Kajian medis dan SOAP terbukti record serta lifecycle berbeda | `AC-CAP022-02` hijau |
| 6 | SOAP harian terbukti tidak menimpa kajian medis | Test `FR-DOK-008` hijau |
| 7 | Waktu klinis terpisah dari waktu penulisan, lini masa terurut benar | `FR-DOK-013` hijau |
| 8 | Episode tertutup menolak catatan baru **dan** menerima amandemen tanpa membuka tempat tidur | `AC-CAP020-03` hijau |
| 9 | Verifikasi tidak mengubah penulis asli | `AC-CAP021-03` hijau |
| 10 | Verifikasi hanya oleh DPJP | `VAL-DOK-07` hijau |
| 11 | SOAP tanpa visite eksplisit tidak menambah hitungan visite | `AC-CAP025-02` hijau |
| 12 | Idempotency visite, resep, dan tindakan terbukti pada **PostgreSQL sungguhan** | Test dua permintaan bersamaan hijau |
| 13 | Kegagalan tagihan tidak menghilangkan catatan tindakan | `AC-CAP024-02` dan aturan 5 hijau |
| 14 | Obat pulang terbedakan dari resep harian | `AC-CAP023-03` hijau |
| 15 | Pesanan lab tidak dapat dipakai lintas episode | `AC-CAP015-01` hijau |
| 16 | Hasil lab terbaca tanpa baris salinan | `AC-CAP015-02` hijau |
| 17 | **Nol jalur tulis** menuju status pemenuhan resep dan hasil lab | Architecture test bagian 7 matriks acceptance hijau |
| 18 | **Nol tabel `Inp*`** untuk dokumentasi dokter | Architecture test hijau |
| 19 | Resource dan Action baru berfungsi bagi peran non-SuperAdmin | Test hak akses per peran hijau |
| 20 | Delapan layar terjangkau sesuai `IA-INP-01` dan `IA-INP-05` | Bukti navigasi |
| 21 | Kolom sensitif tidak muncul di logger | Pemeriksaan payload log |

---

## 19. Urutan pengiriman

| Gelombang | Isinya | Prasyarat |
| --- | --- | --- |
| **`DOK-MVP-0`** | `INT-DOK-01` dan `INT-DOK-02`; kolom baru pada lima tabel; enum baru; `TrxPhysicianVisit` | `episode-rawat-inap` `M1` selesai; **dikerjakan bersama `KEP-MVP-0`** |
| **`DOK-MVP-1`** | `EPIC DOK-01`, `EPIC DOK-02` — pintu masuk dan kajian medis | `DOK-MVP-0` |
| **`DOK-MVP-2`** | `EPIC DOK-03` — SOAP, waktu klinis, amandemen pada episode tertutup | `DOK-MVP-1` |
| **`DOK-MVP-3`** | `EPIC DOK-05` — visite | `DOK-MVP-1` |
| **`DOK-MVP-4`** | `EPIC DOK-06` — resep, tindakan, laboratorium | `DOK-MVP-1` |
| **`DOK-MVP-5`** | `EPIC DOK-04` — CPPT dan verifikasi | `DOK-MVP-2`; **paling akhir** karena bergantung pada catatan yang sudah ada untuk diverifikasi |
| **`POST-MVP`** | Radiologi; nilai batas waktu verifikasi dan kajian; attestation visite; pembacaan balik penyerahan obat pulang | Modul Radiologi; Clinical Governance; pemilik klinis |

**Nol epic `OPEN DECISION`, sehingga nol epic yang tertahan di luar gelombang.**

---

## 20. Pertanyaan terbuka sebelum development lock

| No | Pertanyaan | Pemilik | Memblokir? |
| ---: | --- | --- | :---: |
| 1 | **Kajian medis memakai ulang `TrxPatientAssessment` dengan pembeda jenis, atau tabel tersendiri?** Blueprint memilih pakai ulang; alasan dan konsekuensinya di `02-backend-architecture.md` bagian 4.2 | Product/Domain bersama pemilik `ClinicalManagement` | Tidak — **keputusan struktur, bukan penghalang.** Bila dipilih tabel tersendiri, yang berubah hanya arsitektur dan kamus data; kontrak API, kewenangan, dan alur tetap |
| 2 | Apakah verifikasi DPJP atas CPPT **diwajibkan** di rumah sakit ini, dan berapa batas waktunya? | Clinical Governance | Tidak — mekanismenya dibangun, bawaan `NotRequired` |
| 3 | Berapa batas waktu kajian medis awal? `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | Tidak |
| 4 | Apakah *administrative attestation* visite diizinkan, yaitu petugas non-dokter mencatat visite atas nama dokter? | Clinical Governance | Tidak — bawaan aman: hanya dokter |
| 5 | Apakah catatan keperawatan wajib tampil pada CPPT bagi seluruh profesi? PRD `CAP-014` aturan 4 menyebut "sesuai kebijakan" tanpa menyebut kebijakannya | Clinical Governance | Tidak — catatan tetap tersimpan dan terbaca |

> **Tidak satu pun pertanyaan memblokir.** Berbeda dari `keperawatan` yang menyisakan `RWI-OQ-048`,
> sub-modul ini dapat diteruskan ke `/qv-plan` begitu owner menyetujui dokumen ini — dengan catatan
> bahwa pertanyaan 1 sebaiknya dijawab lebih dulu supaya arsitekturnya tidak berubah di tengah
> pengerjaan.
