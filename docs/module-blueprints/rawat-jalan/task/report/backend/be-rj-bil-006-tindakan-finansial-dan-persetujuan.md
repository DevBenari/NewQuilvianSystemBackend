# Laporan Task — `RJ-BIL-BE-006` Tindakan Finansial, Persetujuan, dan Penutupan Folio

| Field | Nilai |
| --- | --- |
| `task_id` | `RJ-BIL-BE-006` |
| `blueprint` | `RJ-BIL-BP-001` revision `14` |
| `requirement` | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `RJ-BIL-CAP-015` |
| `keputusan pelaksana` | `RJ-BIL-DEC-011` (arsitektur maker-checker), `RJ-BIL-DEC-012` (wewenang eksekusi) |
| `kontrak` | `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` |
| `tanggal` | `2026-08-27` |
| **`status`** | **✅ `SELESAI`** |
| `build dijalankan` | **`YA`** — `0` error, `138` warning, seluruhnya di modul lain |
| `test dijalankan` | **`YA`** — `157` lulus, `0` gagal (`46` di antaranya milik task ini) |
| `migration dibangkitkan` | **`YA`** — `20260827075329_AddBillingFinancialAction` |
| `database disentuh` | **`YA`** — `QuilvianNewDevTim01` saja, atas `RJ-BIL-DEC-009` |
| `commit / push / deploy` | **`TIDAK`** |

> **Yang ✅ ini berarti, dan tidak berarti.** Implementasi selesai dan terverifikasi: ketiga
> acceptance criteria terbukti oleh test yang benar-benar dijalankan terhadap database sungguhan.
> Yang **tidak** ditutup: sign-off Finance dan Security/Privacy tetap `OPEN`, dan `RJ-BIL-OQ-004`
> belum ditetapkan. `IMPLEMENTATION COMPLETE` bukan `PRODUCTION GOVERNANCE APPROVED`, dan tidak
> ada satu pun bagian laporan ini yang memberi izin deploy.

---

## 1. Ringkasan untuk pembaca non-teknis

Task ini menjawab satu pertanyaan: **siapa boleh membatalkan, mengoreksi, menggratiskan, atau
mengembalikan uang sebuah tagihan — dan siapa yang harus menyetujuinya.**

Jawaban intinya satu kalimat, dan sudah Anda kunci sejak `RJ-BIL-GATE-DEC-006`: **orang yang
mengajukan tidak boleh menyetujui permintaannya sendiri.**

Yang dibangun adalah jalur lengkapnya: mengajukan, menunggu keputusan orang kedua, menjalankan
setelah disetujui, dan menutup folio hanya bila tidak ada lagi uang yang belum jelas nasibnya.

---

## 2. Keputusan terpenting: Billing memakai jalur persetujuannya sendiri

Sistem ini **sudah punya** mesin persetujuan yang lengkap dan matang —
`WorkflowService` sepanjang `3.384` baris di `Areas/Corporate/HumanResource/WorkflowManagement/`,
dengan definisi alur, langkah, matriks approval berambang nominal, delegasi, dan riwayat status.
Secara bentuk, mesin itu memang cukup untuk kebutuhan Billing.

Mesin itu tetap **tidak dipakai**, atas `RJ-BIL-DEC-011`. Tiga temuan yang menentukan:

| # | Temuan | Bukti | Mengapa penting untuk uang |
| --- | --- | --- | --- |
| 1 | Larangan self-approval dapat dinyalakan lewat konfigurasi | `MstWorkflowStep.AllowSelfApproval` adalah `bool` per step, diubah lewat `WorkflowStepController` | `RJ-BIL-GATE-DEC-006` melarangnya **tanpa syarat**. Menumpang mesin itu berarti invariant finansial dapat dimatikan dari layar konfigurasi modul lain, tanpa Billing tahu |
| 2 | Penyaringan maker hanya terjadi sekali, dan bukan di titik persetujuan | `AllowSelfApproval` dirujuk hanya di `WorkflowService.cs:543` (saat menyusun assignment) dan `:2954` (hanya dibaca untuk response). `ApproveAsync` tidak pernah membandingkan penyetuju dengan `RequestedByUserId`; `ApprovalDelegationService.cs` (`2.739` baris) tidak merujuk `RequestedByUserId` sama sekali | Delegasi dapat mengembalikan persetujuan kepada pengajunya — kasus yang justru dilarang eksplisit: *"Delegation tidak boleh membuat orang efektif yang sama menjadi keduanya"* |
| 3 | Ketiadaan approver menggagalkan permintaan | `WorkflowService.cs:556` menjawab `400` | `RJ-BIL-GATE-DEC-006` menuntut sebaliknya: permintaan **bertahan** sebagai `PendingApproval` atau `BlockedByPolicyConfiguration`. Permintaan yang hilang berarti tagihannya juga hilang |

> Ketiganya adalah **pengamatan read-only** terhadap modul milik pihak lain, dibatasi pada tiga
> titik penegakan di atas. Ini bukan laporan cacat modul Kepegawaian — untuk cuti dan lembur
> perilaku itu mungkin memang dikehendaki. Yang dinyatakan sempit: perilaku itu tidak sepadan
> dengan invariant finansial yang dikunci `RJ-BIL-GATE-DEC-006`. **Tidak ada satu berkas pun milik
> modul itu yang disunting.**

Harganya dibayar sadar: kapabilitas persetujuan kini ada di dua tempat. `RJ-BIL-DEC-011` mencatat
harga itu, dan mencatat pula bahwa jalan ke mesin bersama tidak tertutup — bila kelak tersedia,
permintaan approval Billing dapat dicerminkan ke sana tanpa memindahkan kewenangan finansialnya.

---

## 3. Yang dibangun

### 3.1 Empat tabel

| Tabel | Isi | Catatan |
| --- | --- | --- |
| `BilFinancialActionRequest` | Pengajuan: jenis, sasaran, nominal, alasan, risiko, kebijakan, revisi, sidik isi, idempotensi, pelaksanaan | Keberadaannya **tidak** mengubah satu angka pun pada tagihan |
| `BilFinancialApproval` | Keputusan checker, hanya ditambahkan dan tidak pernah diubah | Membekukan sidik isi yang benar-benar dilihat checker |
| `MstBillingApprovalPolicy` | Kebijakan ambang berversi, berlaku-tanggal, dan harus disetujui | **Sengaja kosong tanpa seed** — lihat bagian `5` |
| `BilFolioClosureHistory` | Riwayat penutupan dan pembukaan kembali folio | Membuka kembali tidak menghapus fakta bahwa folio pernah ditutup |

### 3.2 Dua puluh enam endpoint

Basis path: `api/v1/health-services/billing-management/financial-actions`

| Kelompok | Jumlah | Kewenangan |
| --- | --- | --- |
| Pembacaan permintaan dan riwayat penutupan | `3` | `Read` |
| Pengajuan per jenis tindakan | `8` | `VoidCreate`, `AdjustmentCreate`, `ReversalCreate`, `RefundCreate`, `WaiverCreate`, `WriteOffCreate`, `ManualOverrideCreate`, `FolioReopenCreate` |
| Milik pengaju sendiri — submit, revise, cancel | `3` | `Submit`, `Revise`, `Cancel` |
| Keputusan checker per jenis tindakan | `8` | `VoidApprove`, `AdjustmentApprove`, `ReversalApprove`, `RefundApprove`, `WaiverApprove`, `WriteOffApprove`, `ManualOverrideApprove`, `FolioReopenApprove` |
| Pelaksanaan | `2` | `Execute`, `RefundExecute` |
| Penutupan dan pembukaan folio | `2` | `FolioClose`, `FolioReopenExecute` |

**Mengapa dipecah per jenis dan bukan satu endpoint serba bisa.** Kewenangan di sistem ini
diberikan per pasangan controller dan action, dan hanya nama yang benar-benar dideklarasikan
`[AccessAction]` yang dapat diberikan admin. Satu endpoint serba bisa berarti satu kewenangan
serba bisa — dan orang yang hanya boleh mengajukan adjustment diam-diam menjadi boleh mengajukan
refund. `RJ-BIL-GATE-DEC-006` butir `1` menuntut pemisahan itu secara harfiah.

Jenis tindakan diambil dari **rute**, bukan dari isi permintaan. Membiarkan body menentukan jenis
akan membuat gerbang kewenangan dapat dilewati dengan menukar satu field.

Submit, revise, dan cancel tidak ikut dipecah karena ketiganya hanya boleh dilakukan pengaju
permintaan itu sendiri, yang sudah harus memegang kewenangan pengajuan jenis tersebut sejak awal.

**Tidak ada satu pun endpoint yang memindahkan uang.** Refund yang dijalankan mencatat kewenangan
pengembalian dana; pencairannya bukan cakupan task ini dan tetap tertutup selama `RJ-BIL-DEP-009`
`INACTIVE`.

---

## 4. Bagaimana ketiga acceptance criteria ditegakkan

### 4.1 Self-approval ditolak

Pemeriksaannya tidak membaca konfigurasi apa pun, tidak punya parameter, dan tidak punya jalan
pintas: ia membandingkan `MakerUserId` dengan `checkerUserId` lalu berhenti. Berlaku untuk
**ketiga** jenis keputusan — menyetujui, menolak, dan mengembalikan untuk revisi — karena pengaju
yang boleh "menolak" permintaannya sendiri tetap dapat menutupnya sebelum orang lain sempat
melihat.

Ada pula index unik pada `(RequestId, Decision = Approve)`, sehingga dua checker yang menekan
tombol bersamaan tidak menghasilkan dua persetujuan atas satu permintaan.

### 4.2 Menunggu persetujuan tidak mengubah state

Tidak ada satu baris pun pada `BillingFinancialActionService` yang menyentuh `BilChargeLine` atau
`BilFolio` sebelum `ExecuteAsync`, dan `ExecuteAsync` menolak berjalan kecuali statusnya
`Approved`.

Dua penjagaan tambahan pada pelaksanaan:

- **Idempoten.** Permintaan yang sudah `Executed` mengembalikan hasil yang sama tanpa menggandakan
  efeknya.
- **Revalidasi.** Versi baris tagihan saat pengajuan disimpan. Bila keadaan sasaran berubah setelah
  persetujuan, pelaksanaan berhenti pada `RevalidationRequired` — bukan menjalankan keputusan atas
  keadaan yang sudah tidak berlaku.

Waiver, write-off, adjustment, dan manual override **tidak menghapus charge asli**. Hanya void dan
reversal yang menyentuh status baris tagihan, dan keduanya pun hanya menandainya.

### 4.3 Penutupan folio ditolak saat rekonsiliasi tertunda

Penutupan bertanya pada dua gerbang:

1. `EvaluateClosureReadinessAsync` milik `RJ-BIL-BE-007` — reconciliation case terbuka, hasil
   pemrosesan yang belum pasti, dan baris tagihan yang masih menunggu telaah finansial.
2. Gerbang milik task ini — permintaan tindakan finansial yang belum selesai.

Status `Approved` **termasuk** yang menahan. Permintaan yang sudah disetujui tetapi belum
dijalankan berarti angka folio masih akan berubah; menutupnya sekarang sama saja menyatakan selesai
atas sesuatu yang jelas-jelas belum.

---

## 5. Dua tempat yang sengaja dibiarkan kosong

### 5.1 `MstBillingApprovalPolicy` tidak di-seed sama sekali

Ini **berbeda** dari `RJ-BIL-BE-007`, yang atas `RJ-BIL-DEC-010` memakai nilai awal nol.
Perbedaannya berasal dari keputusannya masing-masing: `RJ-BIL-GATE-DEC-006` menyatakan
*"Invalid/missing approval policy tidak memakai default approver/threshold"*. Mengisi tabel itu
dengan angka karangan justru melanggar keputusan yang sedang dilaksanakan.

Akibatnya nyata dan memang dikehendaki: selama Finance belum menjawab `RJ-BIL-OQ-004`, tindakan
yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration`. Permintaannya **tetap hidup**
di sana — tidak digagalkan, tidak pula diloloskan. Tagihan deterministik yang normal tidak
terpengaruh sama sekali, persis seperti yang dituntut kalimat *"fail-closed ... tidak memblokir
normal deterministic charge yang sah"*.

Tindakan yang **selalu** high-risk tidak ikut tertahan sebagai masalah konfigurasi: kewajiban
persetujuannya tidak berasal dari kebijakan mana pun, sehingga ia langsung `PendingApproval`.
Kebijakan boleh **menambah** kewajiban persetujuan; ia tidak pernah dapat mencabutnya.

### 5.2 Dua dari empat aturan high-risk belum dapat dinilai apa adanya

`RJ-BIL-GATE-DEC-006` menyebut empat hal yang selalu high-risk tanpa memandang nominal:

| Aturan | Dapat dinilai sekarang? | Yang dilakukan |
| --- | --- | --- |
| Reopen folio yang tertutup | **Ya** | Dinilai apa adanya |
| Koreksi lintas encounter | **Ya** | Dinilai apa adanya |
| Void/reversal terhadap `Paid`, `Posted`, `Claimed`, `Settled` | **Tidak** — keempat keadaan itu belum ada di model mana pun; lahir bersama `RJ-BIL-BE-005` dan `RJ-BIL-BE-008` | **Fail-closed**: void/reversal atas baris `Recognized` diperlakukan high-risk |
| Refund atas pembayaran yang sudah settled | **Tidak** — keadaan settled belum ada | **Fail-closed**: seluruh refund diperlakukan high-risk |

Menebak padanan keempat keadaan itu berarti mengarang keputusan yang bukan milik task ini. Yang
dipilih sebaliknya: bila keadaan tidak dapat dipastikan, tindakan diperlakukan high-risk. Salah
menganggap high-risk hanya menambah satu persetujuan; salah menganggap aman berarti uang berpindah
tanpa pengawasan. Keduanya akan dipersempit begitu keadaan pembayaran yang sesungguhnya tersedia.

---

## 6. Berkas yang ditulis

| Berkas | Isi |
| --- | --- |
| `Enums/BillingFinancialActionEnums.cs` | `BillingFinancialActionType`, `BillingFinancialActionStatus`, `BillingApprovalDecision`, `BillingFinancialRiskLevel`, `BillingFolioClosureAction` |
| `Models/BilFinancialActionRequest.cs` | Pengajuan |
| `Models/BilFinancialApproval.cs` | Keputusan checker |
| `Models/MstBillingApprovalPolicy.cs` | Kebijakan ambang |
| `Models/BilFolioClosureHistory.cs` | Riwayat penutupan folio |
| `Configurations/BillingFinancialActionConfigurations.cs` | Empat konfigurasi, `15` index, `4` di antaranya unik berfilter |
| `Constants/BillingFinancialCapabilities.cs` | Peta kewenangan per jenis tindakan |
| `DTOs/BillingFinancialActionDtos.cs` | Permintaan masuk dan jawaban keluar |
| `Services/BillingFinancialActionService.cs` | Pengajuan, keputusan, pelaksanaan, revisi, kedaluwarsa |
| `Services/BillingFolioClosureService.cs` | Penutupan, pembukaan kembali, riwayat |
| `Controllers/BillingFinancialActionController.cs` | `26` endpoint |
| `Repositories/ApplicationDbContext.cs` | `+4` `DbSet` |
| `Program.cs` | `+2` registrasi service |
| `Tests/.../BillingFinancialActionAuthorityTests.cs` | `25` test murni tanpa database |
| `Tests/.../BillingFinancialActionServiceTests.cs` | `21` test berbasis database |
| `Tests/.../BillingTestDatabaseFixture.cs` | Pembersihan tabel baru saat teardown |

---


| `Migrations/20260827075329_AddBillingFinancialAction.cs` | `4` `CreateTable`, `15` `CreateIndex`. Nol tabel milik modul lain |

---

## 7. Tiga kegagalan dalam perjalanan, dan semuanya ada pada test

Nol kegagalan berasal dari produk. Ketiganya dicatat di sini karena cara memperbaikinya lebih
penting daripada keberadaannya.

### 7.1 `51` test gagal — tabel belum ada

`42P01: relation "public.BilFinancialActionRequest" does not exist`. Migration memang belum
dibangkitkan pada saat itu. Jumlahnya besar bukan karena kerusakan meluas, melainkan karena
pembersihan teardown pada `BillingTestDatabaseFixture` kini menyentuh tabel baru pada **setiap**
kelas test berbasis database — jadi seluruhnya ikut gagal di teardown, bukan hanya `21` test task
ini. Diperbaiki dengan membangkitkan lalu menerapkan migration.

### 7.2 `2` test menuntut folio berstatus `Open`

Salah tebak saya: folio hasil seed berstatus `ReviewRequired`, bukan `Open`. Yang perlu dicatat,
**ketiga assertion inti kedua test itu sudah lulus** — penutupan folio memang ditolak, kode
galatnya `BIL_FOLIO_CLOSE_BLOCKED`, dan alasannya memang menyebut penghalangnya. Acceptance
criteria ketiga terbukti; hanya tebakan tentang status awal yang meleset.

Perbaikannya **tidak** melonggarkan assertion menjadi sekadar *"bukan `Closed`"*. Yang dipakai
justru lebih ketat: rekam status folio sebelum percobaan penutupan, bandingkan sesudahnya, dan
pastikan tidak berubah sama sekali — penutupan yang ditolak tidak boleh meninggalkan jejak apa pun
pada folio, apa pun status semulanya.

### 7.3 `1` test idempotensi meleset `2` tick

`Expected: …3336352Z, Actual: …3336350Z` — selisih `200` nanodetik. Ini beda presisi, bukan
pelaksanaan yang terjadi dua kali: `DateTime` .NET berpresisi `100` nanodetik, sedangkan kolom
`timestamp with time zone` PostgreSQL berpresisi mikrodetik. Nilai kembalian pemanggilan pertama
masih di memori dengan presisi penuh; pemanggilan kedua membacanya kembali dari baris tersimpan.

**Toleransi waktu sengaja tidak dipakai.** Test idempotensi adalah test yang paling tidak boleh
dilonggarkan — melonggarkannya berarti membuka celah refund ganda tanpa ada yang menyadari.
Perbaikannya menghilangkan artefaknya: bandingkan nilai tersimpan dengan nilai tersimpan, sehingga
kedua sisi berpresisi sama. Assertion kenaikan versi baris tagihan (`+1`, bukan `+2`) tetap utuh
dan justru baru benar-benar dijalankan setelah perbaikan ini, sebab sebelumnya test berhenti
sebelum sampai ke sana.

---

## 8. Satu hal yang ditemukan tetapi sengaja tidak diperbaiki

`BilFolio.Status` dapat basi terhadap baris tagihannya: folio tetap `ReviewRequired` walaupun baris
yang memicunya sudah `Recognized`. Perilakunya aman — gerbang penutupan menilai faktanya langsung,
bukan status ringkasan itu — sehingga tidak ada uang yang berisiko.

Tidak diperbaiki di sini karena `BilFolio.Status` milik `RJ-BIL-BE-001`, dan memperbaikinya dari
task ini berarti mengubah perilaku task orang lain tanpa sepengetahuannya. Dicatat agar pemiliknya
dapat memutuskan.

---

## 9. Yang masih terbuka

| Terbuka | Pemiliknya |
| --- | --- |
| Sign-off Finance dan Security/Privacy | Finance dan Security owner, keduanya belum bernama |
| `RJ-BIL-OQ-004` — matriks nominal approval | Finance/Billing. Selama belum ada, tindakan yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration`, dan itu memang perilaku yang dikunci `RJ-BIL-GATE-DEC-006` |
| Working tree belum di-commit | Pemilik blueprint |

---

## 10. Langkah berikutnya

`RJ-BIL-BE-004`, `RJ-BIL-BE-005`, `RJ-BIL-BE-008`, dan `RJ-BIL-BE-009` — seluruhnya ⛔ dan
menunggu keputusan di luar kendali task ini: penunjukan owner `RadiologyManagement`, dan jawaban
`RJ-BIL-OQ-001`, `OQ-002`, serta `OQ-005`.
