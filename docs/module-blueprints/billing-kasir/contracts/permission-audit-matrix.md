# Billing dan Kasir — Permission & Audit Matrix

`contract_version: BIL-PERMISSION-0.4` · status **approved** · owner Security dan process owner · approved 20 Agustus 2026. String berikut adalah target exact string `[AccessPermission(...)]`.

| Endpoint/aksi | Resource/action dan string | Logger | Audit fact wajib |
| --- | --- | :---: | --- |
| `GET invoices` | `[AccessPermission("BillingInvoice", "Read")]` | Tidak | access log standar saja |
| `POST from-source` | `[AccessPermission("BillingInvoice", "Create")]` | Ya | source tuple, result ID, correlation |
| `POST catalog-charges` (**baru, approved**) | `[AccessPermission("BillingInvoice", "Create")]` | Ya | tariff ID, harga hasil lookup server, source tuple, correlation |
| `GET catalog-charges/coverage-preview` (**baru, approved**) | `[AccessPermission("BillingInvoice", "Read")]` | Tidak | access log standar saja — read-only, tanpa mutasi |
| recalculate/void | `[AccessPermission("BillingInvoice", "Update")]` | Ya | version, reason, before/after total |
| apply discount | `[AccessPermission("BillingDiscount", "Create")]` | Ya | policy, target, amount, actor |
| doctor approve | `[AccessPermission("BillingDoctorDiscount", "Approve")]` | Ya | doctor actor, own-share evidence |
| deposit read | `[AccessPermission("BillingDeposit", "Read")]` | Tidak | access standar |
| top-up | `[AccessPermission("BillingDeposit", "Create")]` | Ya | amount/method/shift/correlation |
| allocation | `[AccessPermission("BillingDeposit", "Allocate")]` | Ya | balance before/after, target |
| payment create/tender | `[AccessPermission("BillingPayment", "Create")]` | Ya | amount/method/status, no provider payload |
| adjustment create/approve | `[AccessPermission("BillingAdjustment", "Create")]` / `[AccessPermission("BillingAdjustment", "Approve")]` | Ya | maker/approver/reason/direction |
| refund create/approve | `[AccessPermission("BillingRefund", "Create")]` / `[AccessPermission("BillingRefund", "Approve")]` | Ya | original tender, proportional result |
| write-off create/approve | `[AccessPermission("BillingWriteOff", "Create")]` / `[AccessPermission("BillingWriteOff", "Approve")]` | Ya | outstanding before/after |
| reverse exception | `[AccessPermission("BillingFinancialException", "Reverse")]` | Ya | original/new entry correlation |
| shift open | `[AccessPermission("CashierShift", "Create")]` | Ya | register/opening cash |
| shift read | `[AccessPermission("CashierShift", "Read")]` | Tidak | access standar |
| handover/close | `[AccessPermission("CashierShift", "Handover")]` / `[AccessPermission("CashierShift", "Close")]` | Ya | both actors, system/physical/variance |
| variance/reopen | `[AccessPermission("CashierShift", "Review")]` / `[AccessPermission("CashierShift", "Reopen")]` | Ya | authority, reason, outcome |
| finalization read | `[AccessPermission("BillingFinalization", "Read")]` | Tidak | access standar |
| finalization create | `[AccessPermission("BillingFinalization", "Create")]` | Ya | calculation version, outcome, AR/AP keys |
| administration fee policy read/create/update | `[AccessPermission("AdministrationFeePolicy", "Read")]` / `[AccessPermission("AdministrationFeePolicy", "Create")]` / `[AccessPermission("AdministrationFeePolicy", "Update")]` | GET Tidak; command Ya | effective period, nominal, actor |
| discount policy read/create/update | `[AccessPermission("DiscountPolicy", "Read")]` / `[AccessPermission("DiscountPolicy", "Create")]` / `[AccessPermission("DiscountPolicy", "Update")]` | GET Tidak; command Ya | target, value/limit, approval rule |
| tax rule read/create/update | `[AccessPermission("TaxRule", "Read")]` / `[AccessPermission("TaxRule", "Create")]` / `[AccessPermission("TaxRule", "Update")]` | GET Tidak; command Ya | rate, rounding, allocation |
| room charge policy read/create/update | `[AccessPermission("RoomChargePolicy", "Read")]` / `[AccessPermission("RoomChargePolicy", "Create")]` / `[AccessPermission("RoomChargePolicy", "Update")]` | GET Tidak; command Ya | period, rounding, tariff moment |

Audit disimpan append-only dengan actor, role, time, reason, correlation, entity/version, hasil, dan perubahan nominal. GET tidak memakai custom logger sesuai pola project. Custom log **dilarang** memuat nama/nomor identitas pasien, EncounterId mentah bila tidak perlu, debtor evidence, description klinis, provider reference lengkap, token, credential, nomor kartu, atau payload callback. Maker-checker diperiksa backend, bukan hanya permission. Tests `BIL-AT-012`,`014`,`016`,`022`,`024`.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-PERMISSION-0.5` · status **approved** · owner Security dan process owner · approved_by Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · approved_at 4 September 2026 · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009` (approved).

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET {id}/insurance-invoice-document` (**baru, draft**) | `BillingInvoice` | `Read` | `[AccessPermission("BillingInvoice", "Read")]` | Tidak |

Atribut lengkap yang **MUST** disalin implementer apa adanya:

```csharp
[HttpGet("{id:guid}/insurance-invoice-document")]
[AccessAction("Read", "Read Insurance Invoice Document", AccessType = AccessTypes.Read, SortOrder = 9)]
[AccessPermission("BillingInvoice", "Read")]
[ProducesResponseType(typeof(ApiResponse<InsuranceInvoiceDocumentResponse>), StatusCodes.Status200OK)]
```

**Tidak ada resource permission baru.** Konsekuensinya dinyatakan terbuka: siapa pun yang hari ini boleh membaca satu invoice otomatis boleh mencetak Invoice Asuransi invoice itu. Ini disengaja karena dokumen tidak memuat satu pun data pasien yang belum terlihat pengguna itu di Menu Pembayaran — tambahannya hanya nama, alamat, dan nomor kontrak perusahaan asuransi, yang merupakan data mitra kerja sama. Bila Security menghendaki kewenangan cetak dipisah dari kewenangan baca invoice, itu amendment tersendiri dengan pemilik Security, bukan keputusan desain.

**Audit.** `GET` tidak memakai custom logger, mengikuti konvensi project. Akibat yang **MUST** diketahui: tidak ada jejak siapa mencetak dokumen ini dan kapan. Bila jejak cetak kelak dibutuhkan (misalnya untuk sengketa klaim), itu kemampuan baru yang perlu keputusan Security/Compliance dan tabel penyimpannya sendiri — bukan sesuatu yang dapat ditambahkan diam-diam ke endpoint `GET`.

**Kolom sensitif yang dilewati endpoint ini** dan karena itu **MUST NOT** masuk payload log mana pun, termasuk log galat: `FullName`, `MedicalRecordNumber`, `PolicyNumberSnapshot`, `MemberNumberSnapshot`, `DescriptionSnapshot`. Field yang secara sengaja **tidak** dibaca sama sekali sehingga tidak mungkin bocor: `CardNumberSnapshot`, `MstInsuranceProvider.PicName`/`PicPhoneNumber`/`PicWhatsAppNumber`/`PicEmail`, `MstInsuranceCoverageRule.RuleCode`/`RuleName`/`ApprovalInstruction`/`BillingInstruction`.

Trace `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`. Tests `BIL-AT-033`, `BIL-AT-035`.

---

## Amendment 4 September 2026 — Anomali data penjamin dan gerbang PPN

`last_changed_in: BIL-PERMISSION-0.6` · status **approved** · owner Security dan process owner · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. **Catatan diperbarui 5 September 2026**: pemakaian ulang `BillingInvoice : Read` untuk lembar Invoice Asuransi (`BKC-GATE-03`) ~~tetap menunggu penilaian Security tersendiri~~ **DITUTUP** — `BKC-DEC-092` (Security Owner): dipakai ulang apa adanya, tidak ada permission baru.

### Hak akses — tidak ada butir baru

Amendment ini **tidak** menambah Resource, Action, maupun endpoint. Seluruh field baru terbawa oleh dua endpoint yang sudah terdaftar di layar Akses Role, dan pemetaannya sudah hidup di kolom `Hak akses` pada `api-contract.md`. Turunannya dihitung, tidak ditulis ulang di sini:

- string atributnya `[AccessPermission("BillingInvoice", "Read")]` untuk `calculation-preview`, dan `[AccessPermission("BillingInvoice", "Update")]` untuk `recalculate`;
- pencatatan logger mengikuti konvensi project — `GET` tidak dicatat, selain `GET` dicatat.

**Tidak ada pengecualian** terhadap kedua turunan itu pada amendment ini.

### Peta peran ke kemampuan baru

| Peran rumah sakit | Yang dapat dilihat/dilakukan | Pasangan Resource dan Action |
| --- | --- | --- |
| Kasir | Melihat peringatan anomali data dan nominal terdampak; tetap dapat menerima pembayaran | `BillingInvoice : Read` |
| Supervisor Billing | Sama seperti kasir, ditambah memicu hitung ulang | `BillingInvoice : Read`, `BillingInvoice : Update` |
| Petugas Pendaftaran | **Tidak** mendapat kemampuan baru di modul ini. Pembetulan data penjamin terjadi di modul Registrasi dengan hak aksesnya sendiri | — |
| Admin Master Data | **Tidak** mendapat kemampuan baru. Koreksi `MstTaxRule.AllocationRule` memakai hak akses Tax Rule yang sudah ada | `TaxRule : Update` |

### Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaga | Yang **tidak** dijaganya | Risiko |
| --- | --- | --- | --- |
| "Jangan menagih pasien untuk tagihan yang datanya anomali" | Peringatan di layar saja | Mesin hak akses tidak mencegah kasir menerima pembayaran atas tagihan beranomali, dan `BKC-DEC-073` tidak memintanya dicegah | Pasien membayar penuh untuk biaya yang seharusnya ditanggung asuransi, karena kolom `IsEligible` lupa dicentang. Koreksinya lewat refund. Lihat `BKC-OQ-086` |
| "Aturan pajak yang aktif harus `PROPORTIONAL`" | Tidak ada penjaga | Kode membaca `AllocationRule` apa adanya dari master (`BKC-DES-020`). Admin yang berwenang dapat menyetelnya ke `PATIENT` atau `GUARANTOR` tanpa peringatan apa pun | Seluruh PPN obat/alkes rawat jalan salah dialokasikan, dan salahnya tidak terlihat karena angkanya tetap menjumlah. Lihat `BKC-OQ-088` |
| "Care setting tagihan harus benar" | Tidak ada penjaga di Billing | `BilInvoice.ServiceType` adalah snapshot; Billing tidak dapat menilai apakah Registrasi mendaftarkan kunjungan dengan jenis yang benar | Tagihan rawat inap yang salah didaftarkan sebagai rawat jalan akan dikenai PPN yang seharusnya dibebaskan |

### Audit

| Kejadian | Lapisan | Jejak yang wajib tertinggal |
| --- | --- | --- |
| Perhitungan ulang yang dipersist dengan anomali data terdeteksi | `BilCalculationVersion.BreakdownSnapshot` | Kode anomali ikut tersimpan di dalam JSON snapshot, sehingga versi kalkulasi itu selamanya dapat menjelaskan kenapa nominalnya jatuh ke pasien |
| Perhitungan ulang yang dipersist | Custom logger (`POST recalculate`) | `InvoiceId`, versi sebelum/sesudah, alasan, aktor. **Ditambah** daftar `anomalyCodes` bila ada |
| Pratinjau perhitungan (`GET calculation-preview`) | Tidak dicatat | Konvensi project. Akibatnya: tidak ada jejak siapa **melihat** peringatan anomali, hanya jejak siapa **menyimpan** kalkulasi yang mengandungnya |
| Perubahan `MstTaxRule.AllocationRule` | Custom logger master data yang sudah ada | Nilai sebelum/sesudah, aktor. Ini satu-satunya jejak yang akan menjelaskan pergeseran alokasi PPN di kemudian hari |

### Kolom sensitif dan masa simpan

Payload log **MUST NOT** memuat `FullName`, `MedicalRecordNumber`, `PolicyNumberSnapshot`, `MemberNumberSnapshot`, maupun `DescriptionSnapshot`. Yang **boleh** masuk log adalah `InvoiceId` dan kode anomali (`PAYER_NOT_ELIGIBLE` dan sekerabatnya) — keduanya tidak mengidentifikasi pasien maupun mengungkap isi polis. Kalimat `anomalyMessages` juga **MUST NOT** masuk log, walaupun isinya tidak memuat data pasien, karena ia dibentuk untuk layar dan dapat berubah tanpa pemberitahuan.

Trace `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Tests `BIL-AT-036`–`048`.

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

`last_changed_in: BIL-PERMISSION-0.7` · status **draft** · owner Security dan Kepala Kasir/Finance Operations · `approved_by`: — · `approved_at`: — · input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014`.

Berbeda dari empat amendment sebelumnya yang seluruhnya **tidak** menambah butir hak akses, amendment ini menambah **tiga Resource baru** beserta dua belas Action.

### Cara kerja hak akses di repository ini

Diringkas di sini karena rumpun ini memperkenalkan Resource baru, dan salah menuliskannya menghasilkan `403` permanen yang **tidak dapat diperbaiki dari layar Akses Role**.

| Lapis | Berkas source | Perannya |
| --- | --- | --- |
| Deklarasi controller | `Attributes/AccessControllerAttribute.cs` | Mendaftarkan controller sebagai menu pada layar Akses Role |
| Deklarasi aksi | `Attributes/AccessActionAttribute.cs` | Mendaftarkan kemampuan sebagai baris yang dapat dicentang admin |
| Penegakan | `Attributes/AccessPermissionAttribute.cs`, `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs` | Memeriksa izin pada setiap permintaan masuk |
| Pendaftaran | `Seeders/AccessMenuSeeder.cs` | Menyimpan `SysControllerAccess` dan `SysActionAccess` saat aplikasi mulai |
| Pemberian izin | `Areas/Administrator/Setting/Controllers/RoleAccessController.cs` | Admin mencentang Departemen × Posisi |

**Kontrak penamaan yang mengikat, dan yang paling mudah salah:**

1. Argumen pertama `[AccessPermission]` **MUST** sama persis dengan `ControllerName` pada `[AccessController]`.
2. Argumen kedua `[AccessPermission]` **MUST** sama persis dengan argumen pertama `[AccessAction]` **pada method yang sama**.
3. `AccessType` **MUST** salah satu dari `AccessTypes.Read`, `Create`, `Update`, atau `Delete`, agar kemampuannya muncul di layar Akses Role dan benar-benar dapat diberikan admin.

Aksi bernama di luar keempat kata itu **boleh** — modul ini sudah memakainya pada `CashierShiftsController` (`Handover`, `Close`, `Review`, `Reopen`) — asalkan ketiga ketentuan di atas dipenuhi bersamaan. Petty Cash mengikuti pola itu apa adanya.

### Butir hak akses baru

Pemetaan endpoint ke hak akses hidup di kolom `Hak akses` pada [`api-contract.md`](./api-contract.md) dan **tidak** didaftar ulang di sini. Yang didaftar di sini adalah **string atribut yang persis** untuk ketiga Resource baru, karena ketiganya belum pernah ada di repository dan implementer tidak punya tempat lain untuk menyalinnya.

#### Resource `PettyCashVoucher`

Controller: `PettyCashVouchersController`, `ControllerName = "PettyCashVoucher"`.

| Action | `AccessType` | String atribut yang **MUST** disalin apa adanya | Dicatat logger |
| --- | --- | --- | :---: |
| `Read` | `Read` | `[AccessPermission("PettyCashVoucher", "Read")]` | Tidak |
| `Create` | `Create` | `[AccessPermission("PettyCashVoucher", "Create")]` | Ya |
| `Approve` | `Update` | `[AccessPermission("PettyCashVoucher", "Approve")]` | Ya |
| `Reject` | `Update` | `[AccessPermission("PettyCashVoucher", "Reject")]` | Ya |
| `Cancel` | `Update` | `[AccessPermission("PettyCashVoucher", "Cancel")]` | Ya |
| `Disburse` | `Update` | `[AccessPermission("PettyCashVoucher", "Disburse")]` | Ya |
| `AttachProof` | `Update` | `[AccessPermission("PettyCashVoucher", "AttachProof")]` | Ya |

Bentuk lengkap satu action, sebagai contoh yang **MUST** ditiru bentuknya:

```csharp
[HttpPost("{id:guid}/disburse")]
[AccessAction("Disburse", "Disburse Petty Cash Voucher", AccessType = AccessTypes.Update, SortOrder = 5)]
[AccessPermission("PettyCashVoucher", "Disburse")]
[ProducesResponseType(typeof(ApiResponse<PettyCashVoucherResponse>), StatusCodes.Status200OK)]
```

Perhatikan bahwa `"Disburse"` muncul **dua kali** dan nilainya identik — pada argumen pertama `[AccessAction]` dan argumen kedua `[AccessPermission]`. Bila keduanya berbeda, baris untuk dicentang tidak pernah terbentuk, dan endpoint itu menolak **semua orang** selamanya.

#### Resource `PettyCashBudget`

Controller: `PettyCashBudgetController`, `ControllerName = "PettyCashBudget"`.

| Action | `AccessType` | String atribut yang **MUST** disalin apa adanya | Dicatat logger |
| --- | --- | --- | :---: |
| `Read` | `Read` | `[AccessPermission("PettyCashBudget", "Read")]` | Tidak |
| `TopUp` | `Update` | `[AccessPermission("PettyCashBudget", "TopUp")]` | Ya |
| `Adjust` | `Update` | `[AccessPermission("PettyCashBudget", "Adjust")]` | Ya |

#### Resource `PettyCashCategory`

Controller: `PettyCashCategoriesController`, `ControllerName = "PettyCashCategory"`.

| Action | `AccessType` | String atribut yang **MUST** disalin apa adanya | Dicatat logger |
| --- | --- | --- | :---: |
| `Read` | `Read` | `[AccessPermission("PettyCashCategory", "Read")]` | Tidak |
| `Create` | `Create` | `[AccessPermission("PettyCashCategory", "Create")]` | Ya |
| `Update` | `Update` | `[AccessPermission("PettyCashCategory", "Update")]` | Ya |
| `Delete` | `Delete` | `[AccessPermission("PettyCashCategory", "Delete")]` | Ya |

`PATCH /{id}/status` memakai `[AccessPermission("PettyCashCategory", "Update")]`, mengikuti pola `TaxRulesController` yang juga memakai izin `Update` untuk `deactivate` dan `activate`.

### Turunan yang dihitung, bukan ditulis ulang

| Yang tidak ditulis ulang | Cara menurunkannya |
| --- | --- |
| Pemetaan endpoint ke hak akses | Kolom `Hak akses` pada `api-contract.md` |
| Status pencatatan logger | Konvensi project: `GET` tidak dicatat, selain `GET` dicatat |

**Tidak ada pengecualian** terhadap kedua turunan itu pada amendment ini. Seluruh `GET` Petty Cash tidak dicatat custom logger, dan seluruh `POST`, `PUT`, `PATCH`, serta `DELETE` dicatat.

### Peta peran ke butir hak akses

| Peran rumah sakit | Yang dapat dilakukan | Pasangan Resource dan Action |
| --- | --- | --- |
| Kasir / petugas administrasi | Membuat voucher atas nama siapa saja, menyerahkan uang untuk voucher yang sudah disetujui, memasukkan bukti nota, membatalkan voucher **yang diajukannya sendiri**, dan melihat saldo | `PettyCashVoucher : Read`, `Create`, `Cancel`, `Disburse`, `AttachProof`; `PettyCashBudget : Read` |
| Kepala Kasir / Finance Operations | Seluruh yang di atas, **ditambah** menyetujui dan menolak voucher | ditambah `PettyCashVoucher : Approve`, `Reject` |
| Finance (pengelola anggaran) | Menambah dan mengoreksi anggaran, mengelola kategori, melihat seluruh voucher dan riwayat pergerakan | `PettyCashBudget : Read`, `TopUp`, `Adjust`; `PettyCashCategory : Read`, `Create`, `Update`, `Delete`; `PettyCashVoucher : Read` |
| Penerima uang (pegawai, kurir, vendor) | **Tidak mendapat kemampuan apa pun.** Ia tidak login ke sistem sama sekali | — |
| Kasir dari unit lain | Melihat daftar voucher bila diberi `Read` | `PettyCashVoucher : Read` |

Perlu ditekankan bahwa peran-peran di atas adalah **saran pemetaan**, bukan penegakan. Siapa yang benar-benar mendapat butir mana **ditentukan admin lewat layar Akses Role**, per Departemen dan Posisi. Tidak ada satu pun nama peran, nama departemen, atau nama posisi yang boleh ditulis di dalam kode sebagai penentu kewenangan.

**`PC-DEC-004` diterjemahkan sebagai butir hak akses, bukan sebagai daftar peran di kode.** Keputusan itu menyatakan wewenang persetujuan ada pada Kepala Kasir/Finance Operations; yang dibuat kode adalah butir `PettyCashVoucher : Approve`, dan admin yang memberikannya kepada Posisi yang sesuai.

### Kewenangan yang tidak dapat dijaga mesin hak akses

Bagian ini yang paling penting pada amendment ini, karena rumpun ini menyentuh uang tunai.

| Kewenangan | Penjaga | Yang **tidak** dijaganya | Risiko |
| --- | --- | --- | --- |
| "Pengaju voucher tidak boleh menyetujui voucher yang diajukannya sendiri" | **Tidak ada penjaga sama sekali** | `PC-DEC-004` menetapkan satu jenjang tanpa menyebut pemeriksaan dua orang, berbeda dari write-off yang punya `BIL-VAL-017`. Seseorang yang memegang `Create` **dan** `Approve` sekaligus dapat mengajukan lalu menyetujui pengeluaran uang tunai sendirian | Uang tunai keluar atas keputusan satu orang, tanpa mata kedua. **Mitigasi yang tersedia sekarang murni administratif**: admin **SHOULD NOT** memberikan `Create` dan `Approve` kepada Posisi yang sama. Ini mitigasi konfigurasi, bukan penjaga sistem, dan dapat dilanggar tanpa peringatan apa pun. Diangkat sebagai `PC-OQ-004` |
| "Nama penerima yang ditulis adalah orang yang benar-benar menerima uang" | Tidak ada penjaga | `RecipientName` adalah teks bebas (`PC-DEC-011`). Sistem tidak dapat memeriksa apakah "Budi Santoso" benar-benar ada, apalagi benar-benar menerima uangnya | Voucher fiktif beratas nama orang yang tidak ada. Yang tersisa adalah jejak audit: siapa membuat, siapa menyetujui, siapa menyerahkan. **Bukti nota (`PC-DEC-006`) adalah satu-satunya pengendali sesudahnya, dan ia tidak wajib** |
| "Uang benar-benar sampai ke penerima" | Tidak ada penjaga | `PC-DEC-005` menetapkan "Uang Diberikan" sebagai aksi **satu langkah** oleh kasir, tanpa konfirmasi penerima. Sistem hanya tahu kasir menekan tombol | Kasir menandai uang diserahkan padahal belum. Ini konsekuensi yang disengaja dari `PC-DEC-005`, dan ditandai sebagai kandidat penyempurnaan rilis berikutnya |
| "Bukti nota akhirnya masuk" | Tidak ada penjaga | `PC-DEC-006` menyatakan eksplisit tidak ada mekanisme pemaksaan, eskalasi, maupun pemblokiran | Voucher menggantung di `Uang Diterima` selamanya. Finance memantau manual di luar sistem selama MVP |
| "Saldo yang dimasukkan Finance sesuai uang fisik di laci" | Tidak ada penjaga | Anggaran Petty Cash **tidak** terhubung ke kas fisik shift kasir (`PC-DEC-001`), dan tidak ada penghitungan uang fisik seperti pada penutupan shift | Saldo di sistem dan uang di laci dapat menyimpang tanpa terdeteksi sistem. Koreksinya lewat `POST /budget/adjustments` yang beralasan dan tercatat |

Kelima baris di atas **bukan** cacat desain yang perlu ditutup sebelum rilis. Kelimanya adalah konsekuensi langsung dari keputusan `PC-DEC-004`, `PC-DEC-005`, `PC-DEC-006`, `PC-DEC-011`, dan `PC-DEC-001` yang diambil pemilik secara sadar untuk MVP. Yang **MUST** terjadi adalah pemilik membacanya berdampingan seperti ini, bukan menemukannya setelah uang hilang.

### Audit

| Kejadian | Lapisan | Jejak yang wajib tertinggal |
| --- | --- | --- |
| Voucher dibuat | `BilPettyCashVoucherCommand` (`SUBMIT`) **dan** custom logger | `VoucherId`, `VoucherNumber`, `CategoryId`, nominal, `ActorUserId`, waktu |
| Voucher disetujui atau ditolak | `BilPettyCashVoucherCommand` (`APPROVE`/`REJECT`) **dan** custom logger | Status sebelum dan sesudah, `ActorUserId`, `ActorRole`, alasan penolakan, sisa anggaran sebelum keputusan |
| Voucher dibatalkan pemohon | `BilPettyCashVoucherCommand` (`CANCEL`), `IsCancel`/`CancelBy`/`CancelDateTime`, **dan** custom logger | Pelaku pembatalan beserta alasannya |
| Uang diserahkan | `BilPettyCashVoucherCommand` (`DISBURSE`), **satu baris** `BilPettyCashBudgetMovement`, **dan** custom logger | `BalanceBefore`, `BalanceAfter`, nominal, `ActorUserId`. Inilah jejak paling penting di seluruh rumpun ini |
| Bukti nota dimasukkan atau dikoreksi | `BilPettyCashVoucherCommand` (`ATTACH_PROOF`/`PROOF_CORRECTED`) **dan** custom logger | Nomor nota sebelum dan sesudah, pelaku, waktu |
| Anggaran ditambah atau dikoreksi | `BilPettyCashBudgetMovement` **dan** custom logger | Jenis pergerakan, nominal, saldo sebelum dan sesudah, alasan, pelaku |
| Kategori dibuat, diubah, dinonaktifkan, atau dihapus | Custom logger master data | Nilai sebelum dan sesudah, pelaku |
| Melihat daftar voucher atau saldo | **Tidak dicatat** | Konvensi project. Akibatnya: tidak ada jejak siapa **melihat** saldo kas kecil, hanya jejak siapa **mengubahnya** |

**Kenapa dua lapis jejak, bukan satu.** Custom logger adalah log aplikasi yang dirotasi; `BilPettyCashVoucherCommand` dan `BilPettyCashBudgetMovement` adalah tabel bisnis yang tidak pernah dihapus. Pertanyaan "siapa menyetujui pengeluaran Rp 300.000 tujuh bulan lalu" harus tetap terjawab setelah log aplikasi hilang, dan itulah gunanya kedua tabel tersebut (`PC-DES-009`).

### Kolom sensitif dan masa simpan

Payload custom logger **MUST NOT** memuat `RecipientName`, `Purpose`, `RejectionReason`, `Reason`, maupun `ResponseJson`. Yang **boleh** masuk log: `VoucherId`, `VoucherNumber`, `CategoryId`, `Amount`, `Status` sebelum dan sesudah, `BalanceBefore`, `BalanceAfter`, dan `ActorUserId`.

Rumpun ini **tidak menyentuh satu pun data pasien** — tidak ada `EncounterId`, nomor rekam medis, nomor polis, maupun diagnosis. Yang sensitif di sini adalah identitas penerima uang dan keperluan internal rumah sakit.

**Masa simpan.** Baris `BilPettyCashVoucherCommand` dan `BilPettyCashBudgetMovement` **MUST NOT** dihapus maupun ditandai hapus dalam keadaan apa pun. Keduanya adalah catatan keuangan, dan penghapusannya menghapus bukti pengeluaran uang.

Trace **`PC-DEC-001`–`013`**, `PC-DES-001`–`014`. Tests `BIL-AT-064`–`080`, khususnya `BIL-AT-078` (hak akses) dan `BIL-AT-079` (privasi log).

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

> `last_changed_in`: `BIL-PERMISSION-0.8` / revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010`, `MPY-DES-001`–`017`.
>
> Pemetaan endpoint ke hak akses **tidak diulang di sini** — ia hidup pada kolom `Hak akses` di [`api-contract.md`](./api-contract.md). Bagian di bawah hanya memuat hal yang tidak dapat diturunkan dari daftar endpoint.

## Butir hak akses yang dipakai rumpun ini

| Resource | Action | Dipakai untuk | Status |
| --- | --- | --- | --- |
| `BillingInvoice` | `Read` | Membuka layar Edit Tagihan, pratinjau perbandingan payer, dan mencetak lembar tagihan perusahaan penjamin | **Sudah ada, dipakai ulang** |
| `BillingInvoice` | `Update` | Ketiga perintah edit: ganti payer, penanggung per baris, disposisi penebusan | **Sudah ada, dipakai ulang** |
| `CompanyGuarantorReimbursementRoute` | `Read`, `Create`, `Update`, `Delete` | CRUD master rute reimbursement | **Baru** |
| `CompanyGuarantorCoverageRule` | `Read`, `Create`, `Update`, `Delete` | CRUD master aturan tanggungan perusahaan | **Baru** |

**Nol permission baru pada `BillingInvoice`.** Lembar tagihan perusahaan penjamin memakai ulang `BillingInvoice : Read` apa adanya (`MPY-DEC-006`), mengikuti preseden `BKC-DEC-092` untuk lembar Invoice Asuransi. Konsekuensinya dicatat terbuka: **siapa pun yang sudah berwenang membaca tagihan biasa otomatis berwenang membaca lembar tagihan perusahaan**, termasuk identitas karyawan di dalamnya. Ini keputusan sadar pemilik, bukan kelalaian.

## Cara Resource baru terdaftar

Tidak ada berkas seed maupun berkas konstanta yang perlu disunting. Pendaftaran berlangsung **otomatis lewat pemindaian atribut saat aplikasi dijalankan**: seluruh action controller dipindai, atribut di tingkat kelas dan di tingkat method dibaca, lalu baris modul, controller, dan action di-upsert ke tabel hak akses (`CAP-39`, `Seeders/AccessMenuSeeder.cs`).

Konsekuensi bagi implementer kedua controller master baru:

1. Pasang atribut di tingkat kelas beserta nama controller yang persis sama dengan nama Resource pada tabel di atas.
2. Pasang atribut action dan atribut permission pada setiap endpoint.
3. Jalankan aplikasi sekali. Baris hak akses baru akan muncul dengan sendirinya pada layar pengaturan peran.

Melewatkan langkah 1 atau 2 membuat endpoint **tidak terlindungi sekaligus tidak terdaftar** — kegagalan yang senyap, karena aplikasinya tetap berjalan normal.

## Peta peran ke butir hak akses

| Peran rumah sakit | Butir hak akses | Yang dapat dilakukan pada rumpun ini |
| --- | --- | --- |
| Kasir | `BillingInvoice : Read`, `BillingInvoice : Update` | Membuka Edit Tagihan, membandingkan payer, mengganti payer, mengubah penanggung baris biaya, mengatur penebusan obat, mencetak lembar tagihan perusahaan |
| Kepala Kasir / Finance Operations | Sama dengan Kasir | **Tidak ada kewenangan tambahan pada rumpun ini** — `MPY-DEC-005` menetapkan satu kasir berwenang cukup, tanpa persetujuan tahap kedua |
| Admin Master Data | `CompanyGuarantorReimbursementRoute : *`, `CompanyGuarantorCoverageRule : *` | Mengelola rute reimbursement dan aturan tanggungan perusahaan |
| Finance / Akuntansi | `BillingInvoice : Read` | Membaca dan mencetak lembar tagihan perusahaan untuk keperluan penagihan |
| Petugas Registrasi | — | **Tidak memakai rumpun ini.** Perubahan kartu penjamin pasien tetap lewat layar Data Pasien/Registrasi dengan hak aksesnya sendiri |

## Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaga yang ada | Yang **tidak** dijaganya | Risiko yang tersisa |
| --- | --- | --- | --- |
| Mengganti payer kunjungan | Hak akses `BillingInvoice : Update`, gerbang status tagihan, alasan wajib, jejak perintah yang tidak dapat dihapus | **Tidak ada pemeriksaan orang kedua** (`MPY-DEC-005`). Seorang kasir dapat mengganti payer berulang kali selama tagihan belum dibayar | Perubahan payer yang keliru atau disengaja tidak tercegah di depan; ia hanya **terbaca setelahnya** lewat jejak perintah. Mitigasinya bersifat administratif: tinjauan berkala atas jejak perubahan payer |
| Menandai baris biaya ditanggung asuransi padahal tidak tertanggung | Tidak ada — ini **disengaja** (`MPY-DEC-004`) | Mesin tidak menolak penandaan semacam itu | Rendah secara uang: hasil perhitungannya tetap nol tertanggung sehingga pasien tetap membayar penuh. Yang berpindah hanya keterangan, bukan nominal |
| Menandai obat tidak ditebus padahal pasien menerimanya | Alasan wajib, jejak baris nonaktif yang tidak dihapus | Mesin tidak dapat mengetahui apa yang benar-benar diterima pasien di loket obat | Pendapatan hilang bila disalahgunakan. Penjaga sebenarnya ada di luar sistem: pencocokan dengan catatan penyerahan milik Farmasi, yang memang tetap utuh karena rumpun ini tidak pernah menyentuhnya |
| Mengubah aturan tanggungan perusahaan | Hak akses master data, penolakan hapus aturan yang sudah dipakai | Mesin tidak memeriksa apakah aturan baru sesuai kontrak kerja sama yang sebenarnya | Aturan yang salah ketik menggeser tanggungan seluruh karyawan perusahaan itu. Mitigasi: aturan bermasa berlaku, dan versi perhitungan lama tetap menyimpan angka yang sudah terjadi |

## Audit

| Kejadian | Lapisan pencatatan | Tahan lama |
| --- | --- | --- |
| Ganti payer kunjungan | Baris jejak perintah tersendiri, memuat jenis dan nama payer sebelum/sesudah, versi perhitungan sebelum/sesudah, jumlah penanggung baris yang ikut direset, alasan, dan kunci idempotensi | **Ya — append-only.** Baris ini **MUST NOT** diperbarui maupun ditandai hapus dalam keadaan apa pun |
| Ubah penanggung baris biaya | Baris lama dinonaktifkan dan baris baru disisipkan; riwayatnya terbaca dari deretan baris nonaktif | Ya |
| Ubah disposisi penebusan | Pola yang sama | Ya |
| Perubahan master rute dan aturan tanggungan | Pencatatan logger sesuai konvensi project | Ya |
| Membuka layar Edit Tagihan, pratinjau perbandingan, mencetak lembar tagihan | **Tidak dicatat** | Sesuai konvensi: `GET` tidak dicatat |

Satu pengecualian bernama terhadap konvensi "selain `GET` dicatat": **pratinjau perbandingan payer memakai `POST` tetapi tidak mengubah apa pun**, sehingga ia tidak menghasilkan jejak perubahan. Ia memakai `POST` semata karena parameternya berbentuk badan permintaan, bukan karena ia perintah. Pencatatan logger tetap mengikuti konvensi untuk metode non-`GET`; yang tidak ada adalah jejak perubahan data, karena memang tidak ada data yang berubah.

## Kolom sensitif dan masa simpan

| Kolom | Tabel | Ketentuan |
| --- | --- | --- |
| `EmployeeGrade` | Aturan tanggungan perusahaan | Golongan karyawan. **MUST NOT** masuk payload logger |
| Nomor polis, nomor kartu, nomor karyawan, nama karyawan | Baris sumber pembayaran kunjungan (milik Registrasi) dan lembar tagihan perusahaan | **MUST NOT** masuk payload logger, **MUST NOT** muncul pada pesan galat (`BIL-VAL` batas isi pesan), dan **MUST NOT** dipakai sebagai nama berkas dokumen |
| Nama berkas lembar tagihan perusahaan | — | **MUST** memakai nomor tagihan, bukan nama pasien maupun nama perusahaan — mitigasi yang sama seperti lembar Invoice Asuransi |

**Masa simpan.** Baris jejak perubahan payer adalah catatan keuangan: ia menerangkan mengapa satu tagihan berpindah penanggung, dan menghapusnya menghapus satu-satunya bukti nilai payer sebelumnya. Baris itu **MUST NOT** dihapus maupun ditandai hapus dalam keadaan apa pun.

Trace **`MPY-DEC-001`–`010`**, `MPY-DES-001`–`017`. Tests `BIL-AT-081`–`100`, khususnya `BIL-AT-097` (hak akses) dan `BIL-AT-098` (privasi log dan nama berkas).
