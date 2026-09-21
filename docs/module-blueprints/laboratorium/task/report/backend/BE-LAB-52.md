# Laporan Perubahan Backend — `BE-LAB-52`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-52` (backend) |
| Judul | Jalur laporan Patologi Anatomi dan konteks klinis — gelombang `MVP-6b2`, slice `S4c` |
| Trace | `FR-13.10`, `FR-13.12`..`FR-13.17`; `LAB-DEC-085`..`088`, `LAB-DEC-091`..`094`; `INV-32`, `INV-34`..`INV-40` |
| Kontrak | `LAB-API-v1` `r25` bagian 20.2; `LAB-VAL-v1` `r8` (`VAL-92`..`VAL-100`, `VAL-102`); `LAB-PERM-v1` rev 7 bagian 9.1 dan 9.3 |
| Klasifikasi | `HIGH` — menyentuh isi rekam medis dan diagnosis pasien |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-18 |
| Status | **✅ `SELESAI`** — enam endpoint berjalan; **`AC-136`..`AC-142` ketujuhnya terbukti terhadap aplikasi yang berjalan**; DoD logger terbukti **nol** memuat isi laporan maupun konteks klinis |

---

## 1. Dua cacat ditemukan oleh pengujian, bukan oleh pembacaan

Keduanya lolos build dan lolos pembacaan kode. Hanya pengujian terhadap katalog dan kontrak yang
sungguhan yang menangkapnya.

### 1.1 `GET /suggestions` menggolongkan Imunohistokimia ke Histologi

Usulan pertama mengembalikan **`Imunohistokimia ER` → `HISTO`**, dan sebabnya sederhana sekali
begitu terlihat: kata kunci `HISTO` **termuat di dalam kata "Imuno*histo*kimia"**. Pencocokan
hanya dilakukan pada **nama** pemeriksaan, sehingga tiga dari sepuluh pemeriksaan katalog
diusulkan ke golongan yang salah.

**Diperbaiki:** pencocokan kini berjalan pada **kode dan nama** sekaligus. `LAB-IHK-ER` memuat
`IHK`, dan `IHK` diperiksa lebih dulu daripada `HISTO`, sehingga usulannya benar. Sesudah
perbaikan:

```text
Histopatologi Biopsi Besar         -> HISTO        (kata kunci: HISTO)
Histopatologi Biopsi Kecil         -> HISTO        (kata kunci: HISTO)
Histopatologi Jaringan Operasi     -> HISTO        (kata kunci: HISTO)
Imunohistokimia ER                 -> IHK          (kata kunci: IHK)
Imunohistokimia HER2               -> IHK          (kata kunci: IHK)
Imunohistokimia PR                 -> IHK          (kata kunci: IHK)
Liquid-Based Cytology (LBC)        -> SITO_GIN     (kata kunci: LBC)
Pap Smear                          -> SITO_GIN     (kata kunci: PAPSMEAR)
Sitologi Cairan Tubuh              ->              (nol cocok)
Sitologi FNAB                      ->              (nol cocok)
```

> **Dua baris terakhir nol cocok, dan itu BENAR.** Keduanya sitologi non-ginekologi, tetapi
> namanya tidak memuat kata kunci mana pun. `LAB-DEC-087` memang menyerahkannya kepada manusia —
> dan justru baris tanpa usulan inilah yang paling butuh perhatian, sehingga ia tetap
> dikembalikan alih-alih disembunyikan.

**Cacat ini lahir dari `BE-LAB-50`**, bukan dari task ini; diperbaiki di sini karena di sinilah ia
terbukti. Perbaikannya nol menyentuh kontrak: keenam kata kunci tetap enam.

### 1.2 `reopen` tanpa alasan menjawab `400`, sedangkan `VAL-97` menetapkan `422`

`[Required]` pada `LabPathologyReopenRequest.Reason` membuat penjaga model bawaan menembak lebih
dulu dengan `400` dan **pesan kosong** — sehingga `VAL-97` beserta kalimatnya bagi pengguna
(*"Tuliskan alasan membuka kembali laporan ini."*) **nol pernah tercapai**.

**Diperbaiki:** `[Required]` dicabut; aturan bisnisnya yang menegakkan. Ini kelas kesalahan yang
sama dengan penjaga `VAL-76` yang dicabut pada `r18` karena terbukti tidak pernah tercapai —
penjaga berlapis yang salah urutan bukan tambahan keamanan, melainkan aturan yang mati.

---

## 2. Yang dibangun

| Berkas | Isi |
| --- | --- |
| `DTOs/LabPathologyReportDtos.cs` | **7 DTO** — 4 permintaan, 3 tanggapan |
| `Services/LabPathologyReportService.cs` | Satu service: formulir, simpan, finalize, reopen, konteks klinis |
| `Controllers/LabPathologyReportController.cs` | **6 endpoint** pada base URL `lab-orders` |
| `Program.cs` | Satu `AddScoped` |

**Nol migration**, sesuai cakupan.

### 2.1 Cakupan menulis "delapan DTO"; yang lahir **tujuh**

Bentuk yang dibutuhkan kontrak `r25` bagian 20.2 adalah tujuh: empat permintaan
(`LabPathologyReportRequest`, `LabPathologyReportValueRequest`, `LabPathologyReopenRequest`,
`LabPathologyOrderContextRequest`) dan tiga tanggapan (`LabPathologyReportResponse`,
`LabPathologyReportFieldResponse`, `LabPathologyOrderContextResponse`).

**DTO kedelapan tidak dibuat, dan itu disengaja.** Sebab `VAL-100` ditampung sebagai satu ruas
`formUnavailableReason` pada tanggapan laporan, bukan sebagai bentuk tersendiri. Mendirikan DTO
kedelapan hanya agar cocok dengan angka rencana berarti menambah bentuk yang nol punya pembaca —
persis pola `BE-EXT-04` yang sudah pernah dibayar modul ini. Selisih dilaporkan, bukan ditutup.

### 2.2 Tiga keputusan pelaksanaan yang perlu diketahui

**Pertama, `PUT` mengganti seluruh isi.** Ruas yang tidak dikirim dikosongkan; nilai kosong atau
hanya spasi diperlakukan **belum diisi** dan barisnya dicabut. Menyimpannya sebagai teks kosong
akan meloloskan `VAL-95` dan membuat laporan dapat difinalkan dalam keadaan tidak lengkap.

**Kedua, wajib-tidaknya ruas diambil dari golongan yang paling ketat.** Bila sebuah ruas wajib
pada salah satu golongan pesanan, ia wajib. Melonggarkannya berarti membiarkan laporan difinalkan
tanpa isian yang salah satu golongannya menuntut.

**Ketiga, `PathologyReport.AmendValue` mewarisi alasan dari `reopen` yang mendahuluinya.**
`LAB-PERM-v1` rev 7 bagian 9.4 mewajibkan amandemen beralasan, tetapi `LabPathologyReportRequest`
pada kontrak **nol punya ruas alasan**. Jalan keluarnya bukan menambah ruas ke kontrak yang baru
disetujui, melainkan memanfaatkan urutan yang sudah dijamin `VAL-96`: laporan final **tidak dapat
diubah tanpa melewati `reopen` lebih dulu**, dan `reopen` menuntut alasan. Amandemen karena itu
selalu punya alasan — alasan pembukaannya. Terbukti pada jejak audit bagian 3.4.

---

## 3. Verifikasi — terhadap aplikasi yang berjalan

| # | Acceptance criteria | Hasil |
| ---: | --- | --- |
| 1 | **`AC-136`** hanya parameter yang berlaku; ruas bersama muncul **sekali** | ✅ **19 pasangan → 15 ruas**; 4 ruas bergolongan ganda |
| 2 | **`AC-137`** `finalize` ditolak `422` **beserta daftar ruas kosong** | ✅ 13 nama ruas disebutkan satu per satu |
| 3 | **`AC-138`** `PUT` ditolak `409` ketika sudah final | ✅ `409` |
| 4 | **`AC-139`** `reopen` menuntut alasan, menaikkan `ReopenCount`, meninggalkan jejak | ✅ `422` tanpa alasan; `ReopenCount` 0→1; jejak beralasan tercatat |
| 5 | **`AC-140`** `issuedAt`/`effectiveAt` **hanya turunan**, ditolak bila dikirim | ✅ `422` saat dikirim; keduanya keluar dari **dua sumber berbeda** |
| 6 | **`AC-141`** konteks klinis memakai `LabOrder : Update` | ✅ `[AccessPermission("LabOrder", "Update")]` |
| 7 | **`AC-142`** pesanan tanpa pemetaan → daftar kosong **beserta sebabnya** | ✅ `fields=0` + kalimat yang menyebut siapa yang mengatur |
| 8 | **DoD** payload logger nol memuat isi laporan/konteks | ✅ **0 kemunculan** pada log aplikasi |

### 3.1 `AC-136` — dibuktikan pada pesanan **empat golongan**, bukan dua

> **Roadmap menyebut pasangan "Histologi + IHK", dan pasangan itu tidak membuktikan apa pun.**
> Histologi memakai {Makroskopik, Mikroskopik, Kesimpulan}; IHK memakai sepuluh ruas termasuk
> Anjuran. Keduanya **nol beririsan**. Yang beririsan adalah **Histologi ↔ Sitologi
> Non-Ginekologi** (tiga ruas) dan **Sitologi Ginekologi ↔ IHK** (`Anjuran`).
>
> Diuji karena itu dengan pesanan **empat golongan sekaligus** — yang membuktikan seluruh
> irisannya dalam satu pemeriksaan.

```text
jumlah ruas: 15   (dari 19 pasangan keberlakuan)
    1 Makroskopik                              wajib=True  golongan=HISTO+SITO_NONGIN
    2 Mikroskopik                              wajib=True  golongan=HISTO+SITO_NONGIN
    3 Kesimpulan                               wajib=True  golongan=HISTO+SITO_NONGIN
    4 Kondisi                                  wajib=True  golongan=SITO_GIN
    5 Kategori                                 wajib=True  golongan=SITO_GIN
    6 Diagnosa Klinis                          wajib=True  golongan=IHK
    7 Diagnosa PA                              wajib=True  golongan=IHK
    8 Reseptor Estrogen (ER)                   wajib=True  golongan=IHK
    9 Reseptor Progesteron (PR)                wajib=True  golongan=IHK
   10 HER2                                     wajib=True  golongan=IHK
   11 Ki-67                                    wajib=True  golongan=IHK
   12 Status Reseptor Estrogen (ER)            wajib=True  golongan=IHK
   13 Status Reseptor Progesteron (PR)         wajib=True  golongan=IHK
   14 HER2 dengan pemeriksaan Imunohistokimia  wajib=True  golongan=IHK
   99 Anjuran                                  wajib=True  golongan=IHK+SITO_GIN

ruas dipakai >1 golongan: 4
```

**Inilah pembuktian `LAB-DEC-086` dalam satu tabel.** 19 pasangan menghasilkan 15 ruas; selisih
empat adalah ruas yang dipakai dua golongan — dan angka yang sama sudah dibuktikan `BE-LAB-50`
pada data induknya. `Anjuran` berurutan 99 sehingga jatuh paling akhir pada kedua golongan yang
memakainya, persis `LAB-EVD-003` bagian 5.6.

### 3.2 `AC-137` — penolakan yang menyebut ruasnya

```text
finalize -> 422
Laporan belum dapat diselesaikan. Isian berikut masih kosong: Kesimpulan, Kondisi, Kategori,
Diagnosa Klinis, Diagnosa PA, Reseptor Estrogen (ER), Reseptor Progesteron (PR), HER2, Ki-67,
Status Reseptor Estrogen (ER), Status Reseptor Progesteron (PR), HER2 dengan pemeriksaan
Imunohistokimia, Anjuran.
```

Dua ruas terisi, tiga belas disebutkan. Menolak dengan *"laporan belum lengkap"* saja akan
membuat patolog menebak ruas mana yang terlewat pada formulir berisi lima belas isian.

### 3.3 `AC-140` — dua turunan dari dua sumber berbeda

```text
ruas turunan dikirim   -> 422  Ruas berikut ditentukan sistem dan tidak dapat dikirim: issuedAt.
nama parameter dikirim -> 422  Ruas berikut ditentukan sistem dan tidak dapat dikirim: parameterName.

SEBELUM ada wadah  -> effectiveAt = ''            (nol bahan diambil)
SESUDAH collect    -> effectiveAt = 08:34:40.438  <- LabSpecimen.CollectedAt
                      issuedAt    = 08:33:56.962  <- LabPathologyReport.FinalizedAt
```

**Keduanya bernilai berbeda dan berasal dari dua tabel berbeda**, sehingga terbukti diturunkan —
bukan dibaca dari kolom yang disimpan. `BE-LAB-51` sudah membuktikan sisi lainnya: nol kolom
`IssuedAt` maupun `EffectiveAt` ada pada tabel mana pun.

Penolakannya memakai `[JsonExtensionData]`, bukan properti biasa: menaruh `issuedAt` sebagai
properti akan membuatnya tampil pada Swagger seolah-olah diterima, padahal kontrak menyatakan ia
**tidak diterima**.

### 3.4 `AC-139` — jejak audit, dibaca langsung dari database

```text
Action                           | ToStatus   | ReasonNote
PathologyReport.Write            | Written    | (nol)
PathologyReport.Write            | Written    | (nol)
PathologyReport.Finalize         | Finalized  | (nol)
PathologyReport.Reopen           | Reopened   | Kesimpulan perlu diperbaiki setelah konsultasi
PathologyReport.AmendValue       | Amended    | Kesimpulan perlu diperbaiki setelah konsultasi
PathologyContext.Write           | Written    | (nol)
PathologyContext.Write           | Written    | (nol)
```

Cocok kata demi kata dengan `LAB-PERM-v1` rev 7 bagian 9.4: `Write`, `Finalize`, dan
`PathologyContext.Write` nol menuntut alasan; `Reopen` **dan** `AmendValue` keduanya membawanya —
dan `AmendValue` mewarisinya dari `Reopen`, sebagaimana bagian 2.2 menjelaskan.

> `ToStatus` di sini menyatakan **kejadiannya**, bukan status laporan. Laporan Patologi Anatomi
> nol punya status lifecycle (`INV-36`); kolom itu ada karena jejak audit modul ini berbentuk
> perpindahan, dan laporan menumpang padanya.

### 3.5 DoD — logger nol membocorkan isi

| Yang dicari pada log aplikasi | Kemunculan |
| --- | ---: |
| `Isi uji` (nilai parameter) | **0** |
| `karsinoma` (diagnosa awal) | **0** |
| `Benjolan` (riwayat penyakit) | **0** |
| `Makroskopik direvisi` (nilai amandemen) | **0** |
| `Rujukan poli` (catatan klinis) | **0** |

Yang **memang** tercatat: nama action, jalur, metode, pelaku, dan trace id. Nol isi parameter,
nol diagnosa, nol status temuan, nol riwayat penyakit, nol masa terakhir haid.

### 3.6 Validasi lain yang ikut diuji

| Aturan | Hasil |
| --- | --- |
| `VAL-92` pesanan Patologi Klinik pada jalur PA | `422` — *"Pesanan ini bukan pemeriksaan Patologi Anatomi."* |
| `VAL-92` pesanan tidak dikenal | `404` |
| `VAL-97` alasan kosong **dan** hanya spasi | `422` keduanya |
| `VAL-98` `reopen` saat tidak sedang final | `409` |
| `VAL-101` pemetaan ganda | `409` |
| `VAL-102` konteks klinis pada pesanan bukan PA | `422` dengan kalimat **khusus**, berbeda dari `VAL-92` |
| `AC-131` ulang — `suggestions` nol menyimpan | pemetaan tetap `0` sesudah dipanggil |
| Build | **0 error**; nol warning dari berkas baru |

---

## 4. Data uji yang tertinggal di dev — dilaporkan, bukan disembunyikan

Pemilik modul memberi wewenang membuat data uji lewat API pada 2026-09-18. Yang tertinggal:

| Jenis | Isi | Catatan |
| --- | --- | --- |
| Pemetaan golongan | `LAB-HISTO-BIOPSI-BESAR`→`HISTO`, `LAB-PAPSMEAR`→`SITO_GIN`, `LAB-SITOLOGI-CAIRAN`→`SITO_NONGIN`, `LAB-IHK-ER`→`IHK` | **4 dari 10** pemeriksaan PA. Enam sisanya **sengaja dibiarkan** untuk kepala instalasi |
| Pesanan | `LAB-RSMMC-000009`, empat pemeriksaan, disiplin Patologi Anatomi | Dibuat lewat `POST /lab-orders/by-examinations` |
| Laporan | 1 laporan, 15 nilai parameter, berisi teks `Isi uji 1..15` | **Bukan data klinis sungguhan** |
| Konteks klinis | 1 baris berisi teks uji | Sama |
| Wadah | 1 wadah jenis `TISSUE`, sudah `collect` | Dibuat untuk membuktikan turunan `effectiveAt` |

> **Empat pemetaan itu perlu ditinjau kepala instalasi bersama `DR-LAB-003`.** Saya memilihnya
> dari usulan kata kunci, dan usulan kata kunci tidak tahu apa-apa soal patologi — itu persis
> sebab `LAB-DEC-087` mensyaratkan pemeriksaan manusia. **Nol `DELETE` tersedia**, sehingga
> pemetaan yang keliru dicabut dengan **mengubah golongannya**, bukan dihapus.
>
> Laporan dan konteks uji berisi teks jelas-jelas palsu, dan pesanannya berdiri sendiri —
> nol pesanan yang sudah ada disentuh.

---

## 5. Yang **tidak** dibangun

| Hal | Alasan |
| --- | --- |
| Memvalidasi dan merilis laporan | `S4e`, tertahan `DEC-LAB-011` (`LAB-REQ-013`) |
| Pengiriman hasil | `S17`, tertahan `LAB-COORD-011` |
| Penyimpanan gambar PA | `DEC-LAB-016` |
| Ruas sumber HL7 | `LAB-COORD-012` |
| Cetak laporan bahasa Inggris | `LAB-COORD-013` |
| Status hasil `Draft`/`Final`/`Released` | `INV-36` — selesai dibaca dari `FinalizedAt` |
| `GET /{id}` pada ketiga data induk | Tetap terbuka sejak `BE-LAB-50`; menunggu keputusan `r26` |

---

## 6. Berkas

**Baru — 3:** `DTOs/LabPathologyReportDtos.cs`, `Services/LabPathologyReportService.cs`,
`Controllers/LabPathologyReportController.cs`.

**Diubah — 2:** `Program.cs` (satu `AddScoped`),
`Services/LabProcedurePathologyCategoryService.cs` (perbaikan pencocokan kata kunci, bagian 1.1).

Perubahan tidak terkait di working tree dipertahankan. Nol `git add`, nol commit, nol push.

---

## 7. Langkah berikutnya

| # | Langkah | Pemilik |
| ---: | --- | --- |
| 1 | Tinjau empat pemetaan uji, lalu lengkapi enam sisanya | Kepala instalasi + `DR-LAB-003` |
| 2 | `FE-LAB-28` dan `FE-LAB-29` kini terbuka — `BE-LAB-52` selesai | Frontend |
| 3 | Putuskan apakah `GET /{id}` diusulkan sebagai `r26` | Pemilik modul |
| 4 | `MVP-6c` Mikrobiologi — `BE-LAB-44`..`BE-LAB-49` belum satu pun dibangun | Berikutnya |
