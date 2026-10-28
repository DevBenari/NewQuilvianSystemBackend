# Medical Fee — Kontrak Integrasi

| Field | Nilai |
|---|---|
| Kontrak | `MDF-INTEGRATION-1.0` — `locked` 20 September 2026: §1 (kecuali baris 6–7), §2, §3, §6, §7.1–7.2, §8 |
| Amendment | `MDF-INTEGRATION-1.1` — **`locked` 27 Oktober 2026**: §1 baris 6–7, §4, §5, §7.3, §9 |
| `last_changed_in` | `MDF-INTEGRATION-1.1` (locked) |
| Owner | Yasmin — owner Medical Fee |
| `approved_by` / `approved_at` | 1.0: Yasmin, 20 September 2026. 1.1: owner Medical Fee, 27 Oktober 2026, pernyataan langsung dalam sesi *"saya setuju dan kunci"* atas draft 1.1 beserta rekomendasi `MF-OQ-012` pilihan A. Dicatat apa adanya, tidak ditetapkan skill |
| Masukan 1.1 | `evidence/04-balasan-owner-billing.md`; `MF-CQ-05` dan `MF-CQ-08` ditutup 26 Oktober 2026; `billing-kasir` `BKC-DEC-168`..`178`, `BKC-DES-093`..`103`, `BIL-INTEGRATION-1.9` `BIL-INT-023`..`024`; source port `IBillingDoctorShareResolver.cs` (`BE-BKC-090` ✅) |
| Dampak kompatibilitas 1.1 | Tidak memutus konsumen mana pun: endpoint `sharing-rules/resolve` pada 1.0 §4.3 tidak pernah dibuat dan tidak dipakai Billing. Perubahan perilaku: §5 tidak lagi mencatat seluruh entri `ADHOC` sebagai `PERFORMER_MISSING` |
| Blueprint ID | `MF-BP-001` |
| Kontrak tetangga | `BIL-INTEGRATION-1.9` (billing-kasir revisi 2.4; dokumen ini dulu menyebutnya `BIL-INTEGRATION-0.4`), `FIN-INTEGRATION-0.2`, `BIL-CASH-001`, `FIN-BP-001` revisi 2 |

---

## 1. Ringkasan seluruh integrasi

| # | Mitra | Arah | Jenis | Status |
|---:|---|---|---|---|
| 1 | Operating Room | Masuk | Baca langsung | Siap — datanya lengkap |
| 2 | Clinical | Masuk | Baca langsung | Sebagian — menunggu `MF-CQ-07` untuk tim |
| 3 | Laboratory | Masuk | Baca langsung | Sebagian — menunggu `MF-CQ-07` |
| 4 | Radiology | — | — | **Ditunda** `MF-DEC-015` |
| 5 | Billing — baca nilai layanan | Masuk | Baca langsung | Siap |
| 6 | Billing — persentase `DoctorShare` | **Keluar**, Medical Fee menjawab, Billing menulis | Port in-process | `MDF-INTEGRATION-1.1` §4 **locked** 27 Oktober 2026 |
| 7 | Billing — pelaksana entri kasir | Masuk | Baca langsung | `MDF-INTEGRATION-1.1` §5 **locked** 27 Oktober 2026; kolom Billing belum dijalankan ke database |
| 8 | HR — kontrak kerja | Masuk | Baca + FK | Siap |
| 9 | MasterData — tarif | Masuk | Baca + FK | Siap |
| 10 | Finance — penyerahan hasil jasa | Keluar | Tabel handoff | Siap; Finance sudah menyesuaikan |

Tujuh dari sepuluh siap. Tiga yang tertahan seluruhnya menyangkut Billing dan modul klinis.
*(1.1, locked 27 Oktober 2026: lima siap; dua sebagian menunggu `MF-CQ-07`; satu ditunda; dua integrasi Billing terkunci di
§4–5 dan tinggal menunggu pekerjaan teknis.)*

---

## 2. Masuk — sumber layanan

Seluruhnya dibaca lewat `MedicalFeeServiceSourceAdapter`, satu-satunya tempat yang mengetahui
perbedaan antar sumber.

### 2.1 Operating Room — `OPERATING_ROOM`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `OprCase`, `OprTeamMember` |
| Pelaksana | `OprTeamMember.WorkforceId` |
| Peran | `OprTeamMember.Role` (`OprTeamRole`), dipetakan lewat `MstMedicalFeeRole.OprTeamRoleMapping` |
| Pelaksana utama | `OprTeamMember.IsLead` |
| `SourceDetailId` | `"<OprCaseId>/<OprTeamMemberId>"` |
| Dukungan tim | **Penuh** — satu tindakan menghasilkan beberapa rincian, satu per anggota tim |

Inilah satu-satunya sumber yang sudah mendukung pembagian tim tanpa perubahan apa pun.

### 2.2 Clinical — `PROCEDURE`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `TrxPatientProcedure` |
| Pelaksana | `TrxPatientProcedure.DoctorId` (wajib), `PerformedByUserId` (opsional) |
| Peran | **Tidak ada** — dianggap peran yang `IsPrimaryRole = true` |
| `SourceDetailId` | `"<TrxPatientProcedureId>"` |
| Dukungan tim | **Tidak** — satu pelaksana saja sampai `MF-CQ-07` turun |

Bila `MF-CQ-07` disetujui, `SourceDetailId` berubah menjadi `"<ProcedureId>/<TeamMemberId>"`
dan peran diambil dari pencatatan tim. Bentuk rincian tidak berubah.

### 2.3 Laboratory — `LABORATORY`

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `LabOrder` |
| Pelaksana | `LabOrder.ExaminerDoctorId` — **boleh kosong** |
| Peran | Tidak ada — dianggap peran utama |
| `SourceDetailId` | `"<LabOrderId>"` |
| Bila pemeriksa kosong | `MdfUnresolvedService` `PERFORMER_MISSING` |

### 2.4 Radiology — tidak dibaca

`MF-DEC-015`. Modul Radiology tidak menyimpan pelaksana sama sekali. Selama ditunda, layanan
radiologi **tidak** masuk perhitungan dan **tidak** dicatat sebagai belum terhitung — ia
diabaikan sepenuhnya, karena mencatat ribuan baris `SOURCE_UNSUPPORTED` setiap bulan hanya akan
menenggelamkan daftar yang benar-benar perlu ditindaklanjuti.

`SOURCE_UNSUPPORTED` disediakan pada kamus data untuk kelak, bukan untuk revisi ini.

---

## 3. Masuk — Billing, nilai layanan

| Aspek | Nilai |
|---|---|
| Tabel dibaca | `BilInvoiceItem`, `BilDiscountApplication` |
| Penaut ke layanan | `BilInvoiceItem.SourceDomain` + `SourceDetailId` — pola yang sudah ada |
| Nilai yang dipakai | `Quantity × UnitPrice` — **kotor, sebelum diskon** (`MDF-DES-012`) |
| Diskon | **Tidak dikurangkan** oleh Medical Fee |
| Tagihan yang dibaca | Hanya yang sudah difinalisasi pada rentang periode |

Perlakuan diskon sengaja mengikuti perilaku Billing apa adanya (`MF-DEC-013`): audit menemukan
diskon promo tidak menyentuh `DoctorShare`, dan hanya diskon bertipe dokter yang menguranginya.
Medical Fee **tidak mengubah** perilaku itu, dan tidak mengurangkan diskon apa pun dari basis
perhitungannya.

Konsekuensi yang diterima dan sudah disampaikan ke owner Billing: nilai jasa yang dihitung
Medical Fee dapat berbeda dari `DoctorShare` setelah diskon dokter. Selisihnya bukan kekeliruan
— keduanya menjawab pertanyaan berbeda.

---

## 4. Keluar — Billing, persentase `DoctorShare` lewat port

> `last_changed_in`: **`MDF-INTEGRATION-1.1` — `locked` 27 Oktober 2026** oleh owner Medical Fee. Menggantikan seluruh §4 `MDF-INTEGRATION-1.0`, yang
> berstatus `OPEN DECISION` dan dikecualikan dari penguncian 20 September 2026. Dasar: `MF-CQ-08` ditutup owner Billing
> 26 Oktober 2026 (`evidence/04`); `BKC-DEC-168`..`170`, `174`, `175`, `177`; `BKC-DES-093`..`096`; `BIL-INTEGRATION-1.9`
> `BIL-INT-023`. Keputusan desain sisi Medical Fee: `MDF-DES-019`, `MDF-DES-020` (approved 27 Oktober 2026,
> `02-backend-architecture.md` §2.7).

### 4.1 Ringkasan untuk pembaca umum

Saat sebuah baris tagihan berjasa dicatat, Billing bertanya kepada Medical Fee: *"Untuk dr. Rina, tarif Konsultasi Dokter
Spesialis, tanggal 14 Oktober 2026, peran utama — berapa persen bagiannya?"* Medical Fee menjawab dari kesepakatan tarif
yang **sama** dengan yang nanti dipakai menghitung jasa bulanan, misalnya 40%. Billing lalu menulis sendiri bagian dokter
Rp 500.000 × 40% = Rp 200.000 pada baris tagihannya.

Pertanyaan itu tidak lewat jaringan. Billing dan Medical Fee berjalan dalam satu aplikasi, jadi Billing menyediakan sebuah
"colokan" (port) berupa interface, dan Medical Fee memasang isinya. Selama Medical Fee belum memasangnya, colokan itu
menjawab "tidak tersedia", dan bagian dokter tetap nol persis seperti hari ini.

Akibatnya batas modul **menjadi lebih rapat** daripada rancangan 1.0: Medical Fee tidak menulis satu kolom pun milik Billing.
Ia hanya menjawab pertanyaan.

### 4.2 Pembagian peran

| Hal | Billing | Medical Fee |
|---|:---:|:---:|
| Mendefinisikan port `IBillingDoctorShareResolver` serta record masukan dan jawabannya | **Ya** (`BE-BKC-090` ✅) | Tidak — MUST NOT mengubah bentuknya |
| Implementasi bawaan `NullBillingDoctorShareResolver` (selalu `Unavailable`) | **Ya** | — |
| Implementasi sungguhan `MedicalFeeDoctorShareResolver` | — | **Ya** (`BE-MDF-016`) |
| Menentukan kapan bertanya, batas waktu 2000 ms, dan penanganan gagal lapis kedua | **Ya** (`BKC-DES-095`) | — |
| Menghitung `DoctorShare` dari persentase dan membatasinya pada nilai kotor | **Ya** (`BKC-DES-096`) | — |
| Menulis `DoctorShare`, `DoctorShareResolution`, `DoctorShareSharingRuleId` | **Ya** | **Tidak pernah** |
| Memilih baris tarif yang berlaku | Tidak — dilarang membaca `MdfSharingRule`/`MdfSharingAgreement` | **Ya, satu-satunya** |

### 4.3 Masukan dan jawaban

Bentuk persisnya adalah source port milik Billing,
`Areas/HealthServices/BillingManagement/Billing/Services/IBillingDoctorShareResolver.cs`. Bila source dan tabel ini berbeda,
source port yang berlaku dan kontrak ini MUST diperbarui.

```csharp
Task<BillingDoctorShareResolution> ResolveAsync(BillingDoctorShareQuery query, CancellationToken cancellationToken = default);
```

**Masukan — `BillingDoctorShareQuery`**

| Field | Tipe | Wajib | Arti di Medical Fee |
|---|---|:---:|---|
| `PerformerType` | `string` | Ya | `DOCTOR` → `MdfPayeeTypes.Doctor`; `WORKFORCE` → `MdfPayeeTypes.Workforce`. Nilai lain → `Unavailable` |
| `PerformerReferenceId` | `Guid` | Ya | Dicocokkan ke `MdfSharingAgreement.PayeeReferenceId` |
| `TariffId` | `Guid` | Ya | Tarif baris. Kategorinya dibaca dari `MstTariff.TariffCategoryId` untuk baris tarif berlingkup kategori |
| `ServiceDate` | `DateOnly` | Ya | Tanggal layanan zona Asia/Jakarta. Billing menurunkannya dari `BilInvoiceItem.SourceOccurredAt` |
| `RoleCode` | `string?` | Tidak | Kosong → peran aktif dengan `IsPrimaryRole = true`. Terisi → `MstMedicalFeeRole.RoleCode` yang aktif |

**Jawaban — `BillingDoctorShareResolution`**

| `Outcome` | `SharingPercentage` | `SharingRuleId` | Arti | Yang dilakukan Billing |
|---|---|---|---|---|
| `Resolved` | 0–100 | Id baris tarif | Satu baris tarif sah berlaku | `DoctorShare` = nilai kotor × persentase; jejak `MEDICAL_FEE` |
| `NoRule` | `null` | `null` | Tidak ada aturan sah. Ini **keadaan bisnis biasa**, mis. kesepakatan dokter tamu belum disusun | `DoctorShare = 0`; jejak `NO_RULE` |
| `Unavailable` | `null` | `null` | Medical Fee **tidak dapat menjawab dengan benar** saat ini: gangguan, habis waktu, atau data tarif rusak | `DoctorShare = 0`; jejak `UNAVAILABLE` dan peristiwa audit `BillingInvoice.DoctorShareUnavailable` |

Pemisahan `NoRule` dan `Unavailable` disengaja. Yang pertama dapat dibiarkan sampai kesepakatan tarif disusun. Yang kedua
menandakan sesuatu yang rusak dan MUST terlihat, karena baris itu baru pulih saat sumbernya diperbarui (`BKC-DEC-177`).

**Contoh**

| Masukan | Keadaan data | Jawaban |
|---|---|---|
| `DOCTOR`, dr. Rina, tarif Konsultasi Spesialis, 14 Oktober 2026, peran kosong | Kesepakatan aktif; baris tarif peran utama untuk tarif itu 40% | `Resolved`, 40.00, id baris tarif |
| `DOCTOR`, dr. Tamu, tarif yang sama | Belum ada kesepakatan untuk dr. Tamu | `NoRule` |
| `DOCTOR`, dr. Rina, tarif yang sama | Dua baris tarif sama khusus berlaku bersamaan | `Unavailable` |
| `NURSE`, siapa pun | Jenis pelaksana di luar kontrak | `Unavailable` |

### 4.4 Cara Medical Fee menjawab

Urutan dan aturannya **sama persis** dengan langkah pemilihan tarif pada perhitungan periode, karena keduanya memakai satu
komponen bersama, `MedicalFeeSharingRuleMatcher` (`MDF-DES-020`). Tanpa komponen bersama, jawaban kepada Billing dan hasil
jasa bulanan dapat berbeda untuk layanan yang sama. Itulah alasan bentuk C ditolak pada 1.0.

| # | Langkah | Bila gagal |
|---:|---|---|
| 1 | Petakan `PerformerType` ke jenis penerima Medical Fee | `Unavailable` |
| 2 | Tentukan peran: `RoleCode` kosong → peran utama aktif; terisi → peran aktif dengan kode itu | `NoRule` |
| 3 | Cari kesepakatan `Active` milik penerima yang masa berlakunya mencakup `ServiceDate` | `NoRule` |
| 4 | Periksa kontrak HR yang ditunjuk kesepakatan belum berakhir pada `ServiceDate` (`MDF-DES-008`) | `NoRule` |
| 5 | Ambil baris tarif peran itu yang berlaku pada `ServiceDate`, lalu pilih yang paling khusus: tarif persis, kemudian kategori tarif, kemudian bawaan peran | `NoRule` |
| 6 | Bila dua baris sama khusus berlaku bersamaan | `Unavailable`, dicatat log peringatan. Data tarif bertabrakan mestinya sudah ditolak saat disusun (`MDF-VAL-1.0` §3); bila tetap lolos, itu kerusakan, bukan "tidak ada aturan" |
| 7 | Satu baris terpilih | `Resolved` dengan persentase snapshot dan id baris itu |

Langkah 3–5 memakai alasan yang sama dengan `MdfUnresolvedService` (`AGREEMENT_MISSING`, `CONTRACT_EXPIRED`, `RULE_MISSING`).
Jadi layanan yang dijawab `NoRule` kepada Billing akan muncul pula di daftar belum dapat dihitung saat periode dihitung.
Dengan begitu janji jawaban 3 owner Billing terpenuhi: layanan itu tidak hilang tanpa jejak.

### 4.5 Kewajiban teknis implementasi

| # | Kewajiban | Alasan |
|---:|---|---|
| 1 | Tidak pernah melempar exception. Kesalahan apa pun, termasuk `OperationCanceledException`, dijawab `Unavailable` | Bunyi port; pencatatan tagihan tidak boleh gagal karena Medical Fee (`INV-MF-1`) |
| 2 | Meneruskan `cancellationToken` ke **setiap** query EF | Billing memasang batas waktu 2000 ms dan memakai `DbContext` scoped yang sama sesudahnya. Query yang tetap berjalan setelah Billing melanjutkan memicu kesalahan "a second operation was started on this context" di jalur Billing |
| 3 | Baca saja: `AsNoTracking`, tanpa `SaveChanges`, tanpa membuka transaksi | Port dipanggil sebelum transaksi Billing dengan `DbContext` yang sama (`BKC-DES-095`). Entity yang terlacak akan ikut tersimpan oleh `SaveChanges` milik Billing |
| 4 | Paling banyak tiga query terindeks: peran; kesepakatan beserta baris tarif penerima; kategori tarif | Batas waktu 2000 ms. Index `(PayeeType, PayeeReferenceId, Status)` pada `MdfSharingAgreement` sudah ada |
| 5 | Log hanya `Outcome`, `SharingRuleId`, dan kode alasan. Persentase, nilai, dan nama penerima tidak dicatat | Data penghasilan bersifat sensitif (`MDF-PERM-1.0`) |
| 6 | Didaftarkan dengan `services.Replace(ServiceDescriptor.Scoped<IBillingDoctorShareResolver, MedicalFeeDoctorShareResolver>())` di `Program.cs`, **setelah** pemanggilan terakhir `AddBillingManagement()` | Billing mendaftarkan bawaan dengan `TryAddScoped`, dan `Program.cs` memanggil `AddBillingManagement()` dua kali (baris 1157 dan 1205 pada saat penulisan). `TryAddScoped` milik Medical Fee akan kalah diam-diam. `Replace` membuang bawaan sehingga hanya satu implementasi yang terdaftar |
| 7 | Tidak ada endpoint HTTP untuk pertanyaan ini | `MDF-DES-019` |

### 4.6 Akibat yang perlu diketahui

| Hal | Akibat |
|---|---|
| Tim pelaksana | `DoctorShare` per baris memakai persentase **peran utama** saja (`BKC-DEC-175`). Jasa anggota tim lain tetap dihitung Medical Fee saat periode. Jumlah `DoctorShare` pada tagihan karena itu dapat lebih kecil dari jasa periode, dan itu bukan kekeliruan (sejalan dengan §3) |
| Diskon dokter | Billing boleh mengurangi `DoctorShare` dengan diskon bertipe dokter (`BKC-DEC-174`). Medical Fee tetap menghitung dari nilai kotor (`MDF-DES-012`) |
| Baris tarif baru setelah tagihan dicatat | `DoctorShare` pada baris lama tidak bergeser sampai sumbernya diperbarui (`BKC-DEC-177`). Perhitungan periode memakai baris tarif yang berlaku pada tanggal layanan (`MDF-DES-009`). Keduanya sama, kecuali baris tarif baru diberi masa berlaku mundur |
| Sebelum `BE-MDF-016` terpasang | Seluruh jawaban `Unavailable` dari implementasi bawaan, `DoctorShare` nol seperti hari ini |
| Entri bebas `ADHOC` tanpa tarif | Billing tidak bertanya (`TariffId` wajib) dan mencatat `NOT_APPLICABLE` (`BKC-OQ-150`). Perlakuannya di Medical Fee diatur §5.4 |

### 4.7 Yang dilarang

| Larangan | Alasan |
|---|---|
| Billing membaca `MdfSharingRule` atau `MdfSharingAgreement` langsung | Menduplikasi pemilihan baris tarif (bentuk C yang ditolak) |
| Medical Fee menulis `BilInvoiceItem` | Billing tetap satu-satunya penulis baris tagihan |
| Medical Fee mengubah bentuk port | Port milik Billing; perubahan bentuk adalah amendment Billing |
| Membuat endpoint HTTP `sharing-rules/resolve` sebagai jalur kedua | Dua jalur untuk satu pertanyaan akan menyimpang; endpoint juga membuka data penghasilan ke permukaan baru (`MDF-DES-019`) |

### 4.8 Yang diganti dari `MDF-INTEGRATION-1.0`

| Isi 1.0 | Nasibnya di 1.1 |
|---|---|
| 4.1 "`DoctorShare` diketik petugas" | Dikoreksi Billing: seluruh jalur internal sudah menulis nol; satu-satunya jalur bernilai bukan nol adalah isian pemanggil `POST /invoices/from-source` (`evidence/04` §3) |
| 4.2 tiga bentuk A/B/C | Diputuskan: **A**, diwujudkan sebagai port in-process (`BKC-DES-093`) |
| 4.3 `GET .../sharing-rules/resolve` | Diganti port. Semantik masukan sama; `agreementNumber` tidak lagi dikembalikan; `resolved: false` dipecah menjadi `NoRule` dan `Unavailable` |
| Pertanyaan perlakuan tanpa aturan | Dijawab: `DoctorShare = 0`, tagihan tetap dapat difinalisasi, tanpa jalur ketik (`BKC-DEC-170`) |

## 5. Masuk — Billing, pelaksana entri kasir

> `last_changed_in`: **`MDF-INTEGRATION-1.1` — `locked` 27 Oktober 2026** oleh owner Medical Fee. Menggantikan seluruh §5 `MDF-INTEGRATION-1.0`.
> Dasar: `MF-CQ-05` ditutup owner Billing 26 Oktober 2026; `BKC-DEC-171`, `172`, `176`, `178`; `BKC-DES-099`, `102`, `103`;
> `BIL-INTEGRATION-1.9` `BIL-INT-024`. Keputusan desain sisi Medical Fee: `MDF-DES-021` (approved 27 Oktober 2026).
> `MF-OQ-012` diputuskan 27 Oktober 2026: pilihan A (§5.4).

### 5.1 Ringkasan untuk pembaca umum

Kasir kadang mencatat layanan langsung di layar pembayaran, misalnya "Konsultasi Dokter Spesialis" dari katalog tarif, atau
entri bebas tanpa katalog. Sejak Billing revisi 2.4, kasir dapat menandai entri itu **berjasa** dan memilih dokter atau tenaga
medis pelaksananya. Medical Fee membaca tanda dan pelaksana itu saat menghitung jasa bulanan. Entri yang tidak ditandai,
seperti materai atau biaya administrasi, bukan layanan berjasa dan tidak disentuh sama sekali.

### 5.2 Kolom yang dibaca — milik Billing, baca saja

| Kolom pada `BilInvoiceItem` | Tipe | Arti |
|---|---|---|
| `IsFeeBearing` | `bool`, bawaan `false` | Kasir menandai layanan berjasa |
| `PerformerType` | `string(20)?` | `DOCTOR` atau `WORKFORCE`; wajib bersama `PerformerReferenceId` (`BKC-DEC-178`) |
| `PerformerReferenceId` | `Guid?` | Pelaksana; wajib bila `IsFeeBearing = true` (`CK_BilInvoiceItem_PerformerRequired`) |
| `PerformerRoleCode` | `string(30)?` | Kode peran Medical Fee; kosong berarti peran utama. Billing hanya memeriksa bentuknya (`BKC-DES-103`) |
| `SourceDomain`, `SourceDetailId` | — | Sudah dibaca sejak 1.0 (§3) |
| `SourceOccurredAt` | `timestamptz` | Waktu layanan; dasar tanggal layanan |
| `TariffId`, `CategoryId` | `Guid?` | Lingkup baris tarif; kosong pada entri bebas `ADHOC` |
| `Quantity`, `UnitPrice`, `Status` | — | Nilai kotor dan status baris, sudah dibaca sejak 1.0 |

Keadaan kolom pada 27 Oktober 2026: model, configuration, dan berkas migration ada di source (`BE-BKC-089` ✅), tetapi
migration **belum dijalankan** ke database (`{OTORISASI-MIG-3}`). Pengisian dari layar kasir menunggu `BE-BKC-091`.

### 5.3 Aturan baca

| Keadaan baris | Perlakuan Medical Fee |
|---|---|
| `SourceDomain` bukan `ADHOC`/`ADHOC_CATALOG` | Di luar cakupan §5 |
| `IsFeeBearing = false`, termasuk **seluruh** baris yang tercatat sebelum migration | Bukan layanan berjasa: tidak dihitung, **tidak** dicatat `PERFORMER_MISSING`. Menggantikan bunyi 1.0 "seluruh entri jatuh ke `PERFORMER_MISSING`" |
| `IsFeeBearing = true`, `PerformerReferenceId` kosong | Mestinya mustahil karena constraint Billing. Bila terjadi: `PERFORMER_MISSING` |
| `PerformerType` = `DOCTOR` / `WORKFORCE` | Dipetakan ke `Doctor` / `Workforce` **sebelum** disimpan atau dicocokkan, sama dengan §4.3 |
| `PerformerType` kosong atau nilai lain | `PERFORMER_MISSING` dengan keterangan jenis pelaksana tidak dikenal. **Tidak** menebak `Doctor` |
| `PerformerRoleCode` kosong | Peran utama aktif; bila tidak ada peran utama aktif: `ROLE_UNMAPPED` |
| `PerformerRoleCode` terisi dan ditemukan aktif | Peran itu |
| `PerformerRoleCode` terisi tetapi tidak dikenal atau nonaktif | `ROLE_UNMAPPED` |
| Tanggal layanan | `SourceOccurredAt` dikonversi ke tanggal Asia/Jakarta, **sama dengan** `ServiceDate` yang Billing kirim di §4 |
| Nilai dasar | `Quantity × UnitPrice`, kotor sebelum diskon (`MDF-DES-012`) |
| `TariffId` terisi (umumnya `ADHOC_CATALOG`) | Pemilihan baris tarif lewat `MedicalFeeSharingRuleMatcher`. Hasilnya sama dengan jawaban yang diterima Billing untuk baris itu |
| `TariffId` kosong (entri bebas `ADHOC`) | Dihitung memakai baris tarif **bawaan peran** penerima, yaitu baris tanpa lingkup tarif maupun kategori. Tanpa baris bawaan peran: `RULE_MISSING` (`MF-OQ-012` pilihan A, §5.4) |

### 5.4 `MF-OQ-012` — entri bebas berjasa tanpa tarif

Billing mencatat bagian dokter entri seperti ini selalu nol (`NOT_APPLICABLE`), karena tidak ada tarif untuk ditanyakan
(`BKC-OQ-150`). Medical Fee harus memutuskan perlakuannya saat periode dihitung.

| Pilihan | Cara kerja | Untung | Rugi |
|---|---|---|---|
| **A — baris tarif bawaan peran** *(rekomendasi)* | Dihitung memakai baris tarif peran yang tidak berlingkup tarif maupun kategori. Ini perilaku pencocokan yang sudah ada | Tanda berjasa adalah tindakan sengaja kasir; jasanya tidak hilang. Tanpa pekerjaan manual tambahan | Bagian dokter pada tagihan Rp 0 tetapi jasa periodenya terisi. Petugas perlu tahu bahwa ini disengaja |
| B — selalu masuk daftar belum dapat dihitung | Dicatat `RULE_MISSING`; petugas memutuskan lewat pengesampingan bercatatan | Setiap rupiah dari entri bebas ditinjau manusia | Setiap entri bebas berjasa menjadi pekerjaan manual dan menahan penutupan periode (`MDF-DES-016`) |

Rekomendasi **A**, karena baris tarif bawaan peran memang disepakati sebagai cadangan bila tidak ada baris yang lebih khusus.

**Diputuskan 27 Oktober 2026: pilihan A**, oleh owner Medical Fee bersama penguncian `MDF-INTEGRATION-1.1` (*"saya setuju dan
kunci"*). Akibat yang diterima: bagian dokter pada tagihan untuk entri seperti ini Rp 0, sedangkan jasa periodenya terisi.
Petugas Medical Fee perlu tahu bahwa selisih itu disengaja.

### 5.5 Contoh

| Entri kasir | Yang terjadi di Billing | Yang terjadi di Medical Fee saat periode Oktober |
|---|---|---|
| "Konsultasi Dokter Spesialis" Rp 500.000 dari katalog, berjasa, dr. Rina, peran kosong | Bertanya lewat §4 → 40% → `DoctorShare` Rp 200.000 | Membaca baris yang sama, peran utama, tanggal yang sama, baris tarif yang sama → rincian jasa dr. Rina Rp 200.000 |
| Materai Rp 10.000, tidak berjasa | Tidak bertanya | Tidak dibaca sama sekali |
| Entri bebas "Tindakan tambahan" Rp 300.000, berjasa, dr. Budi | Tidak bertanya; `DoctorShare` 0 | Dihitung dengan baris tarif bawaan peran dr. Budi, mis. 30% → rincian Rp 90.000. Tanpa baris bawaan peran → daftar belum dapat dihitung `RULE_MISSING` |

### 5.6 Celah as-is pada source

Pembacaan entri berjasa **sudah ditulis** di `MedicalFeeServiceSourceAdapter.ExtractAdhocBillingServices` (laporan
`BE-MDF-009`), mendahului kontrak ini. Ada empat penyimpangan dari to-be di atas. Skill desain tidak memperbaikinya; perbaikannya
masuk cakupan `BE-MDF-015` lewat `build-module-backend`.

| # | As-is | To-be | Akibat bila dibiarkan |
|---:|---|---|---|
| G1 | `PayeeType` diisi `PerformerType` mentah (`DOCTOR`); kosong dianggap `Doctor` | Dipetakan; kosong atau lain → `PERFORMER_MISSING` | Hasil jasa dokter dapat tersimpan dengan `PayeeType` `DOCTOR` dan bernomor berawalan `WFP` (`MedicalFeeCalculationService`, pembentukan nomor hasil jasa) |
| G2 | `PerformerRoleCode` diabaikan; selalu peran utama | Dipakai sesuai §5.3 | Pelaksana berperan selain utama dibayar dengan persentase peran utama |
| G3 | Tanggal layanan dari `Invoice.InvoiceDate` dalam UTC | `SourceOccurredAt` tanggal Asia/Jakarta | Layanan pukul 00.00–06.59 WIB tercatat sehari lebih awal; pada pergantian masa berlaku tarif, Billing dan periode memilih baris berbeda |
| G4 | Komentar source menyebut "sesuai penguncian permukaan `MDF-INTEGRATION-1.0` §5" | §5 belum pernah dikunci | Pembaca source mengira kontraknya sudah final |

---

## 6. Masuk — HR dan MasterData

| Sumber | Yang dibaca | FK | Aturan |
|---|---|:---:|---|
| `WfpContractHistory` | `WorkforceProfileId`, `StartDate`, `EndDate`, `ContractStatus`, `IsCurrent` | Ya, `Restrict` | Layanan setelah `EndDate` → `CONTRACT_EXPIRED` |
| `MstDoctor` | Identitas penerima bertipe `Doctor` | Tidak (polimorfik) | Divalidasi service |
| `MstTariff`, `MstTariffCategory` | Lingkup baris tarif | Ya, `Restrict` | — |

Medical Fee **tidak pernah menulis** ke satu pun tabel HR atau MasterData.

Satu pertanyaan terbuka yang perlu diketahui saat implementasi: apakah dokter tamu dan dokter
paruh waktu punya baris `WfpContractHistory`. Bila tidak, kelompok itu tidak akan punya
kesepakatan tarif yang sah, padahal justru merekalah yang tarifnya paling sering berbeda-beda.
Pertanyaan itu ada di `evidence/02-pemberitahuan-untuk-owner-hr.md` bagian 5 nomor 2.

---

## 7. Keluar — Finance

| Aspek | Nilai |
|---|---|
| Mekanisme | Tabel handoff `MdfFinanceHandoff` (`MDF-DES-018`) |
| Kapan ditulis | Di dalam transaksi yang sama dengan persetujuan hasil jasa |
| Kunci idempoten | `HandoffKey` (`Guid`) |
| Nilai | `MdfServiceFee.FinalAmount` — **kotor, sebelum potongan** (`MF-DEC-005`) |
| Yang mengonsumsi | Finance, membuat `FinMedicalServicePayable` |
| Konfirmasi | Finance memanggil `POST /finance-handoffs/{id}/acknowledgement` |
| Arah tulis | Medical Fee **tidak pernah** menulis ke tabel Finance |

### 7.1 Bentuk baris handoff

```json
{
  "handoffKey": "8f14e45f-...",
  "serviceFeeId": "...",
  "payeeType": "Doctor",
  "payeeReferenceId": "...",
  "periodCode": "2026-09",
  "grossAmount": 42750000.00,
  "status": "Created",
  "correlationId": "...",
  "createdAt": "2026-10-03T16:02:11+07:00"
}
```

`periodCode` dan `payeeType` disalin dengan sengaja: Finance dapat membaca satu baris tanpa
menyentuh tabel Medical Fee mana pun.

### 7.2 Pembagian tanggung jawab dengan Finance

| Tanggung jawab | Medical Fee | Finance |
|---|:---:|:---:|
| Menghitung jasa dari layanan | **Ya** | Tidak |
| Menentukan persentase sharing | **Ya** | Tidak |
| Menyimpan nilai kotor per penerima per periode | **Ya** | Tidak |
| Menghitung PPh 21 | Tidak | **Ya** (`FIN-DES-026`) |
| Kasbon, iuran, KSO, potongan lain | Tidak | **Ya** |
| Membuat utang jasa medis | Tidak | **Ya** (`FinMedicalServicePayable`, `FIN-DES-025`) |
| Membayar | Tidak | **Ya** |
| Jurnal | Tidak | **Ya** |

Finance sudah menyesuaikan diri lebih dulu: `FIN-DES-025`..`028` pada `FIN-BP-001` revisi 2
mengganti `FinDoctorPayable` menjadi `FinMedicalServicePayable` dan menambahkan
`FinPaymentDeduction`. Tidak ada perubahan yang diminta ke Finance oleh revisi ini.

### 7.3 `BilApHandoff` bukan sumber utang jasa — `MDF-INTEGRATION-1.1` locked

Billing meminta Medical Fee dan Finance menegaskan hal ini (`BKC-DEC-173`; `BIL-INTEGRATION-1.9`, "Titik sentuh yang tidak
berubah"). Penegasannya **tidak membuat keputusan baru**; ia disusun dari keputusan yang sudah approved:

| Pernyataan | Dasar |
|---|---|
| Satu-satunya sumber utang jasa medis adalah `MdfFinanceHandoff` | `MDF-DES-018`; `MF-DEC-005` |
| Finance mengakui utang jasa hanya dari hasil jasa yang sudah disetujui, **bukan** dari `BilApHandoff` | Fakta F19; `FIN-DEC-003` Opsi B |
| `BilApHandoff` tetap terbit tanpa perubahan kode Billing, hanya sebagai rujukan kesiapan | `BKC-DEC-173`; `FIN-INTEGRATION-0.2` §3 |

Kenapa ini perlu ditulis sekarang: hari ini `BilApHandoff.Amount` nol karena seluruh jalur menulis `DoctorShare = 0`. Begitu
§4 mengisi `DoctorShare`, nilai itu mulai terisi. Bila suatu saat ada yang membacanya sebagai utang, dokter dibayar dua kali:
sekali dari `BilApHandoff`, sekali dari `MdfFinanceHandoff`.

Status: **ditegaskan 27 Oktober 2026** lewat penguncian `MDF-INTEGRATION-1.1` oleh owner Medical Fee, yang juga owner Finance.
Permintaan penegasan Billing (`BKC-DEC-173`) terjawab.

---

## 8. Yang tidak ada dan memang tidak seharusnya ada

| Integrasi | Mengapa tidak |
|---|---|
| Medical Fee → Accounting | Jurnal dibuat dari sisi Finance saat utang terbit. Dua sumber jurnal untuk satu peristiwa akan menggandakan pencatatan |
| Medical Fee → Registration | Modul ini tidak menyentuh data pasien sama sekali |
| Medical Fee menulis ke tabel HR | Kontrak milik HR; hubungan satu arah |
| Medical Fee menulis ke tabel Finance | Finance mengonsumsi handoff, bukan ditulisi |
| Billing → Medical Fee untuk diskon | Diskon tidak mengurangi basis perhitungan (`MF-DEC-013`) |

---

## 9. Pemicu peninjauan kontrak ini

| Pemicu | Yang ditinjau |
|---|---|
| ~~Owner Billing menjawab `MF-CQ-08`~~ | **Terjadi 26 Oktober 2026** → bagian 4 ditulis ulang sebagai `MDF-INTEGRATION-1.1` draft. *(Teks 1.0 menyebut target `MDF-INTEGRATION-0.2`; penomoran yang benar setelah 1.0 adalah 1.1)* |
| ~~Owner Billing menjawab `MF-CQ-05`~~ | **Terjadi 26 Oktober 2026** → bagian 5 ditulis ulang sebagai `MDF-INTEGRATION-1.1` draft |
| ~~Owner Medical Fee memutuskan `MF-OQ-012`~~ | **Terjadi 27 Oktober 2026** — pilihan A; §5.3–5.5 diperbarui dan dikunci |
| Billing mengubah bentuk port `IBillingDoctorShareResolver` | Bagian 4.3–4.5 |
| Billing menutup `BKC-OQ-148` (pelaksana dari fakta modul sumber klinis) | Bagian 2 dan 5; sumber klinis mungkin mulai membawa pelaksana lewat Billing |
| Billing mencabut pemanggilan ganda `AddBillingManagement()` di `Program.cs` | Bagian 4.5 butir 6 — aturan `Replace` tetap berlaku, hanya keterangan barisnya yang berubah |
| Clinical atau Laboratory menambahkan pencatatan tim | Bagian 2.2 dan 2.3 |
| Radiology menambahkan pencatatan pelaksana | Bagian 2.4; `MF-DEC-015` dapat ditinjau ulang |
| Billing mengubah perlakuan diskon dokter | Bagian 3 |
| HR mengubah `WfpContractHistory` | Bagian 6 |
| `FIN-BP-001` menyentuh `FinMedicalServicePayable` | Bagian 7 |
