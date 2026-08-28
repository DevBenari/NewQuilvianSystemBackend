# Laporan Perubahan Backend — `RJ-BIL-BE-007`

## Metadata

| Field | Nilai |
| --- | --- |
| TASK ID | `RJ-BIL-BE-007` — reconciliation case dan recovery status |
| TASK TYPE | Implementasi task backend approved dari roadmap canonical |
| COMPLEXITY | `HEAVY` |
| MODEL | `claude-opus-5` |
| TASK MODE | `MODULE BLUEPRINT` |
| WRITE TARGET | `NewQuilvianSystemBackend/` — `Areas/HealthServices/BillingManagement/Operational/`, `Repositories/`, `Program.cs`, `Migrations/`, `Tests/`, `docs/module-blueprints/rawat-jalan/` |
| Trace | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017`; `RJ-BIL-DEP-008`; keputusan `RJ-BIL-DEC-008`, `RJ-BIL-DEC-009`, `RJ-BIL-DEC-010` |
| Contract version | Integration `RJ-BIL-INT-001@1.0.0` — **tidak berubah**. Endpoint yang ditambahkan bersifat baru dan tidak menyentuh kontrak yang sudah disetujui |
| Branch / HEAD | `sukmagp` / `6b25e60` |
| Migration | `20260827040349_AddBillingReconciliationCase` — **sudah diterapkan** ke `QuilvianNewDevTim01` atas `RJ-BIL-DEC-009` |
| Tanggal verifikasi | 27 Agustus 2026 |
| Status | **SELESAI** — ketiga acceptance criteria terbukti; governance formal Billing/Finance tetap `OPEN` |

---

## 1. Yang dibangun, dan mengapa bentuknya begini

`RJ-BIL-GATE-DEC-008` menuntut satu hal yang mudah diucapkan dan sulit dijamin: **kehilangan
jawaban tidak boleh berubah menjadi tagihan ganda**.

Sebelum task ini, Billing sudah punya idempotensi, deteksi duplikat, dan status `OutcomeUnknown`.
Yang belum ada adalah apa yang terjadi *setelah* `OutcomeUnknown` muncul. Nilainya tercatat, lalu
diam di sana. Tidak ada yang membukanya menjadi pekerjaan, tidak ada yang memiliki
penyelesaiannya, dan tidak ada yang menahan folio agar tidak ditutup sementara uangnya belum
jelas.

Empat kemampuan yang ditambahkan menutup celah itu.

### 1.1 Menemukan

`ScanAsync` membandingkan efek pemrosesan dengan keadaan Billing, lalu membuka case untuk setiap
hasil yang bermasalah. Pemindaian ini **idempoten**, dan keidempotenannya dijaga database, bukan
oleh pemeriksaan di memori:

```
IX_BilReconciliationCase_CaseType_SourceContext_MilestoneFactI~  (unique, filter IsDelete = false)
```

Bila dua pemindaian berlomba, yang kalah menangkap `DbUpdateException`, mencari case pemenang,
lalu memakainya. Bila ternyata tidak ada pemenang, exception aslinya dilempar kembali apa adanya
— menelannya akan menyembunyikan kesalahan yang sesungguhnya.

### 1.2 Menampilkan

Case memiliki pemilik, prioritas, umur, SLA, tindakan berikutnya, dan alasan kegagalan. Laporan
pemulihan menjumlahkannya per hasil dan per jenis, beserta encounter dan folio yang terdampak.

Case **lahir tanpa pemilik**. Itu disengaja: `RJ-BIL-GATE-DEC-008` mewajibkan case punya owner,
tetapi tidak menetapkan aturan penugasan otomatis. Mengarang aturan penugasan berarti mengarang
SOP, jadi penugasan dibuat menjadi tindakan sadar yang tercatat siapa dan kapan.

### 1.3 Menahan

`EvaluateClosureReadinessAsync` menjawab boleh atau tidaknya sebuah folio ditutup, **beserta
daftar alasannya satu per satu**. Petugas yang ditolak berhak tahu persis apa yang menahannya.

Tiga hal menahan penutupan:

| Penahan | Alasan |
| --- | --- |
| Reconciliation case terbuka yang material | Ada uang yang nasibnya belum diketahui |
| Efek dengan hasil `OutcomeUnknown` atau `PendingReconciliation` | Menutup folio sambil ada pengiriman yang hasilnya tidak diketahui adalah cara paling langsung kehilangan tagihan tanpa jejak |
| Baris tagihan berstatus `PendingFinancialReview` | Telaah finansialnya belum selesai |

### 1.4 Menjawab

`GetProcessingStatusAsync` menjawab hasil pemrosesan sebuah fakta berdasarkan identitas sumbernya
yang stabil. Inilah jalan keluar dari kehilangan jawaban: modul klinis yang tidak menerima balasan
tidak menyimpulkan apa pun, melainkan bertanya.

Jawabannya memuat `SafeToRetryWithSameKey` secara eksplisit, sehingga pemanggilnya tidak perlu
menebak:

| Hasil | Aman diulang? | Sebabnya |
| --- | --- | --- |
| `Succeeded`, `Reconciled` | Tidak | Sudah diterapkan |
| `TransientFailure` | **Ya** | Gangguan sementara |
| `RejectedValidation` | Tidak | Final untuk versi itu; perbaikannya versi baru |
| `PermanentFailure` | Tidak | Tunggu penyelesaian case |
| `PartialOutcome` | Tidak | Mengulang seluruh fakta akan menggandakan komponen yang sudah berhasil |
| `OutcomeUnknown` | Tidak | Hasilnya belum terverifikasi |
| Tidak ditemukan | **Ya** | Pengiriman belum pernah sampai |

---

## 2. Batas kewenangan yang dijaga

Tidak ada satu pun method maupun endpoint pada `RJ-BIL-BE-007` yang memindahkan uang.

Rekonsiliasi berhenti pada temuan. Ketika penelusuran menyimpulkan bahwa memang dibutuhkan
tindakan finansial, jenis penyelesaiannya adalah `ManualFinancialAction` — sebuah pernyataan
bahwa masalahnya diketahui dan uangnya diputuskan pihak lain, melalui jalur persetujuan
`RJ-BIL-GATE-DEC-006` yang menjadi isi `RJ-BIL-BE-006`.

Tanpa jenis penyelesaian itu, petugas rekonsiliasi tidak punya cara menyatakan hal tersebut dan
akan tergoda menutup case seolah tidak berdampak.

Kewenangan endpoint sengaja dipecah menjadi empat:

| Permission | Untuk |
| --- | --- |
| `BillingReconciliation : Read` | Melihat case, kesiapan penutupan, laporan, status kanonik |
| `BillingReconciliation : Scan` | Menjalankan pemindaian |
| `BillingReconciliation : Assign` | Menugaskan pemilik |
| `BillingReconciliation : Resolve` | Menyatakan case selesai |

Menyatukan keempatnya akan membuat hak lihat diam-diam berubah menjadi hak menutup masalah.

---

## 3. Kosakata hasil pemrosesan

`RJ-BIL-GATE-DEC-008` menuntut sepuluh nilai. Lima sudah ada sejak `RJ-BIL-BE-002` dengan nama
setara; lima ditambahkan di sini.

| Nilai | Asal | Arti tindakan |
| --- | --- | --- |
| `Received` = 1 | `BE-002` | — |
| `InProgress` = 2 | `BE-002` | — |
| `Succeeded` = 3 | `BE-002` | Selesai diterapkan |
| `FailedBeforeEffect` = 4 | `BE-002` | **Peninggalan**, tidak dipakai lagi untuk baris baru |
| `PartialOutcome` = 5 | `BE-002` | Sebagian komponen diterapkan |
| `OutcomeUnknown` = 6 | `BE-002` | Hasil tidak diketahui |
| `RejectedValidation` = 7 | **`BE-007`** | Final; perbaikannya versi fakta baru |
| `TransientFailure` = 8 | **`BE-007`** | Boleh diulang otomatis dengan kunci sama |
| `PermanentFailure` = 9 | **`BE-007`** | Berhenti dari percobaan ulang; masuk dead-letter |
| `PendingReconciliation` = 10 | **`BE-007`** | Menunggu rekonsiliasi |
| `Reconciled` = 11 | **`BE-007`** | Sudah direkonsiliasi |

### Kenapa `FailedBeforeEffect` tidak dihapus

Nilainya sudah tersimpan pada baris nyata di database. Menghapus anggota enum membuat baris
tersebut tidak dapat dibaca kembali sebagai status mana pun — persis cacat yang ditemukan pada
migration `RJ-BIL-BE-003` ketika kolom status diberi nilai bawaan `0` sementara enum-nya mulai
dari `1`.

Nilai itu diperlakukan sebagai **kegagalan menetap**, bukan sementara. Tidak ada dasar untuk
menyatakannya aman diulang, dan menganggapnya sementara berarti mengulang sesuatu yang mungkin
sudah terlanjur diterapkan.

---

## 4. Angka kebijakan — `RJ-BIL-DEC-010`

`RJ-BIL-GATE-DEC-008` menyebut *"financially material permanent failure"* tanpa satu pun angka.
Ketiga angka berikut karena itu menjadi master data yang dapat diubah admin tanpa rilis, bukan
konstanta di kode:

| Kolom | Nilai awal | Alasan |
| --- | --- | --- |
| `MaterialityThresholdAmount` | `0` | Setiap kegagalan menahan penutupan folio. Perilaku paling aman, dan bukan angka karangan |
| `SlaMinutes` | `0` | SLA belum diatur, sehingga tidak ada tenggat dan tidak ada peringatan palsu |
| `DefaultPriority` | `2` (Normal) | Netral |
| `AllowAutoResolveDeterministicDuplicate` | `false` | Tidak ada case yang tertutup sendiri |

Angka sebenarnya tetap `OWNER_DECISION_REQUIRED`.

Ketika baris kebijakan untuk sebuah jenis case belum ada sama sekali, jawabannya **menahan**.
Ketiadaan kebijakan bukan izin melewatkan uang yang nasibnya belum jelas.

Pelampauan SLA hanya menandai, menaikkan prioritas, dan mengeskalasi. Tidak ada case yang
diselesaikan, tidak ada tagihan yang dihapus, dan tidak ada persetujuan yang diberikan — sesuai
kalimat `RJ-BIL-GATE-DEC-008` bahwa pelampauan SLA adalah soal perhatian, bukan soal keputusan.

---

## 5. Bukti verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `Build succeeded`, `0 Error(s)` | `PASS` |
| Seluruh suite Billing | `Failed: 0, Passed: 111, Total: 111` | `PASS` |
| Test `RJ-BIL-BE-007` di dalamnya | `37` — 21 murni, 16 berbasis database | `PASS` |
| Migration diterapkan | `20260827040349` ke `QuilvianNewDevTim01` | `APPLIED` |
| Tinjauan statis migration | Tidak ada kolom enum ber-`defaultValue: 0` | `PASS` |

### Acceptance criteria roadmap

| Kriteria | Bukti |
| --- | --- |
| *Timeout tidak menggandakan charge* | `JawabanHilang_PencarianStatusMenyatakanTidakAmanDiulang`, `PemindaianDiulang_TidakMenggandakanCase` |
| *Failed component visible* | `KegagalanSebagianKomponen_TerlihatSebagaiCaseTersendiri`, `LaporanPemulihan_MenampilkanCaseBelumSelesaiBesertaPemilikDanTindakan` |
| *Folio close blocked sampai case resolved* | `FolioTidakBolehDitutupSelamaMasihAdaCaseTerbuka`, `SetelahCaseDiselesaikan_FolioTidakLagiTertahanOlehCaseItu` |

Batas kewenangan dibuktikan `MenyelesaikanCase_TidakMengubahNilaiMaupunStatusTagihan`, yang
membandingkan nilai dan status setiap baris tagihan sebelum dan sesudah case ditutup.

---

## 6. Migration: apa yang dibuang dari hasil bangkitan EF, dan kenapa

EF semula menerbitkan **lima** `CreateTable`, bukan dua, plus 45 pasang
`DropForeignKey`/`AddForeignKey`. Migration itu **gagal** ketika dijalankan:

```
Npgsql.PostgresException : 42P07: relation "MstRegister" already exists
```

Penyebabnya bukan pada `RJ-BIL-BE-007`. `ApplicationDbContextModelSnapshot.cs` kehilangan tiga
entity master milik modul lain, sehingga EF menyangka tabelnya belum ada:

| Entity | Ada di snapshot | Punya migration | Ada di database |
| --- | --- | --- | --- |
| `MstRoomChargePolicy` | Tidak | Ya — `20260820084721` | Ya |
| `MstTaxRule` | Tidak | Ya — `20260820084721` | Ya |
| `MstRegister` | Tidak | **Tidak ada** | Ya |

Ketiga `CreateTable` dibuang dari migration, tetapi ketiga entity **tetap ada pada snapshot** —
dan itulah perbaikannya: snapshot kembali merekam bahwa tabelnya memang ada, sehingga migration
siapa pun berikutnya tidak lagi mencoba membuatnya ulang.

Ke-45 pasang foreign key juga dibuang, setelah diverifikasi tidak mengubah perilaku apa pun:
tidak ada yang hanya di-drop, tidak ada yang hanya ditambahkan, dan seluruhnya `Restrict` — sama
persis dengan definisi aslinya pada `20260824080052`.

Hasil akhirnya migration turun dari sekitar `1.160` baris menjadi `339`, dan hanya menyentuh dua
tabel milik Billing.

Rinciannya beserta yang masih terbuka ada pada
[`RJ-BIL-NOTICE-001`](../../../approval-requests/2026-08-27-pemberitahuan-tabel-modul-lain-terbawa-migration.md).

---

## 7. Cacat yang ditemukan di luar cakupan, dan penanganannya

Menjalankan test terhadap database sungguhan untuk pertama kalinya sejak `RJ-BIL-BE-002`
memunculkan empat hal yang sebelumnya tidak terlihat.

### 7.1 `Database.Migrate()` mati untuk seluruh tim

EF Core 9 menolak menerapkan migration selama ada entity tanpa migration. Ketiga entity pada
bagian `6` membuat keadaan itu terjadi, sehingga `dotnet ef database update` gagal untuk semua
orang. Penjagaannya ditekan **hanya pada fixture test**, tidak pada konfigurasi aplikasi, dan
penyebabnya dilaporkan alih-alih disembunyikan.

### 7.2 Test saling menyerobot

Tiga kelas test berjalan paralel terhadap satu database dengan isolasi `Serializable`,
menghasilkan `9` kegagalan yang tampak seperti cacat domain. Buktinya bahwa penyebabnya
paralelisme: kelas yang gagal tiga kali lulus `11` dari `11` ketika dijalankan sendirian.

Menaikkan jumlah percobaan ulang pada service adalah jawaban yang salah — itu menutupi gejala
pada kode produksi demi kenyamanan test. Yang dilakukan adalah menonaktifkan paralelisme
antarkelas pada assembly test.

### 7.3 Celah kehilangan pendapatan pada `RJ-BIL-BE-003`

`GetCurrentUserId()` pada `LabSpecimenService` dan `LabOrderService` mengembalikan `Guid.Empty`
secara diam-diam ketika klaim pengguna hilang atau rusak. Akibatnya sampel **tetap** berhasil
dinyatakan layak, sementara penyerahan fakta ke Billing ditolak sebagai
`CLIN_FACT_ACTOR_INVALID`.

Hasil akhirnya: pemeriksaan dikerjakan, tagihannya tidak pernah terbentuk, dan tidak ada satu pun
pesan galat yang sampai ke petugas. Kedua service sekarang menolak bertindak ketika identitas
pelakunya tidak dapat ditentukan.

### 7.4 Harness test tidak pernah punya petugas

Test Lab membuat `new HttpContextAccessor()` kosong, sehingga yang diuji selama ini adalah
ketiadaan pengguna login, bukan perilaku domain. Fixture kini menyediakan
`CreateHttpContextAccessor(actorUserId)` yang memuat identitas petugas sungguhan dari encounter
yang dibuat test.

### 7.5 Satu test konkurensi yang tidak pernah menguji konkurensi

`DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak` mengandaikan context kedua memegang versi
lama, padahal service memuat ulang sampel di dalam pemanggilannya — sehingga yang menolak adalah
penjaga status, bukan penjaga versi. Test ditulis ulang agar context kedua benar-benar membuka
baris lebih dulu, dan kini juga membuktikan bahwa keputusan ganda tidak menghasilkan tagihan
ganda.

---

## 8. Ringkasan field laporan

- **API CONTRACT IMPACT**: `ADDITIVE`. Delapan endpoint baru pada
  `api/v1/health-services/billing-management/reconciliation`. Tidak ada endpoint, request,
  response, atau permission yang sudah ada yang berubah.
- **DATABASE IMPACT**: dua tabel baru milik Billing beserta tujuh index dan sepuluh baris master
  data. Migration sudah diterapkan ke database pengembangan bersama atas `RJ-BIL-DEC-009`.
  Staging, UAT, dan production tidak tersentuh.
- **SECURITY IMPACT**: empat permission baru yang sengaja dipisah. Endpoint mutasi menolak
  bekerja tanpa identitas actor. Tidak ada kewenangan finansial yang ditambahkan.
- **VISUAL REFERENCE**: `NOT REQUIRED`
- **MANUAL TEST**: `NOT APPLICABLE` — seluruh acceptance criteria terbukti otomatis
- **INCIDENTAL CHANGES**: perbaikan pada `RJ-BIL-BE-003` (bagian `7.3`), harness test (`7.4`),
  test konkurensi (`7.5`), dan paralelisme test (`7.2`). Seluruhnya diperlukan agar test
  `RJ-BIL-BE-007` dapat dijalankan sama sekali.
- **KNOWN ISSUES**: `MstRegister` tidak punya migration di mana pun, sehingga database baru tidak
  akan memilikinya. Milik modul lain; dilaporkan melalui `RJ-BIL-NOTICE-001`.
- **STALE EVIDENCE / BLOCKED PHASES**: governance formal Billing/Finance dan Integration owner
  atas `RJ-BIL-GATE-DEC-008` tetap `OPEN`. `IMPLEMENTATION_COMPLETE` bukan
  `PRODUCTION GOVERNANCE APPROVED`.
- **GIT STATUS**: tidak ada `commit`, `push`, `merge`, maupun `deploy`.
- **NEXT RECOMMENDED STEP**: `RJ-BIL-BE-006` — kini dependency-ready karena gerbang penutupan
  folio yang dibutuhkannya sudah tersedia.
