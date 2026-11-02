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

---

## Amendment 15 September 2026 — Revisi Petty Cash: pencairan langsung dan anggaran per periode

`last_changed_in: BIL-PERMISSION-0.9` · status **approved** · owner Security dan Finance Operations · `approved_by`: Product/Domain Owner (`PC-DEC-026`) · `approved_at`: 2026-09-15 · input: **`PC-DEC-016`–`PC-DEC-025`**; keputusan arsitektur `PC-DES-015`–`PC-DES-025`.

Amendment ini **menghapus dua butir hak akses** dan **menambah lima**. Penghapusan butir hak akses belum pernah terjadi di modul ini sebelumnya, sehingga langkahnya ditulis lengkap di bawah.

### Butir hak akses yang dihapus

| Resource | Action | String yang dihapus | Dasar |
| --- | --- | --- | --- |
| `PettyCashVoucher` | `Approve` | `[AccessPermission("PettyCashVoucher", "Approve")]` | `PC-DEC-024` |
| `PettyCashVoucher` | `Reject` | `[AccessPermission("PettyCashVoucher", "Reject")]` | `PC-DEC-024` |

**Prasyarat implementasi yang MUST dikerjakan sebelum penghapusan** (`PC-DEC-024` menyebutnya eksplisit): periksa lebih dulu apakah ada Departemen × Posisi yang **hanya** memegang salah satu dari kedua butir ini dan tidak memegang butir Petty Cash lain. Bila ada, peran itu akan kehilangan seluruh kemampuannya atas Petty Cash begitu butirnya hilang — dan tidak ada pesan apa pun yang memberitahunya; layarnya hanya menjadi kosong. Pemeriksaan itu dikerjakan di `SysActionAccess`/`SysControllerAccess` sebelum migration dijalankan, dan hasilnya dilaporkan ke pemilik modul.

`AccessMenuSeeder` mendaftarkan butir hak akses dari atribut lewat pemindaian, sehingga butir yang atributnya dihapus **tidak** otomatis hilang dari baris yang sudah tersimpan. Pembersihan baris lama adalah langkah tersendiri, bukan efek samping penghapusan atribut.

### Butir hak akses baru

| Resource | Action | `AccessType` | String yang persis | Diberikan kepada |
| --- | --- | --- | --- | --- |
| `PettyCashVoucher` | `Return` | `Update` | `[AccessPermission("PettyCashVoucher", "Return")]` | Kasir (`PC-DEC-022`) |
| `PettyCashVoucher` | `Reverse` | `Update` | `[AccessPermission("PettyCashVoucher", "Reverse")]` | Kasir (`PC-DEC-022`) |
| `PettyCashBudget` | `Create` | `Create` | `[AccessPermission("PettyCashBudget", "Create")]` | Finance |
| `PettyCashBudget` | `Activate` | `Update` | `[AccessPermission("PettyCashBudget", "Activate")]` | Finance |
| `PettyCashBudget` | `Close` | `Update` | `[AccessPermission("PettyCashBudget", "Close")]` | Finance |

Ketiga ketentuan penamaan yang dicatat amendment 7 September tetap berlaku penuh dan tidak diulang di sini.

### Peta peran ke butir hak akses setelah revisi

| Peran | `PettyCashVoucher` | `PettyCashBudget` | `PettyCashCategory` |
| --- | --- | --- | --- |
| Kasir / petugas administrasi | `Read`, `Create`, `Cancel`, `Disburse`, `AttachProof`, **`Return`**, **`Reverse`** | `Read`, `TopUp` | `Read` |
| Finance | `Read` | `Read`, `TopUp`, `Adjust`, **`Create`**, **`Activate`**, **`Close`** | `Read`, `Create`, `Update`, `Delete` |
| Kepala Kasir / Finance Operations | `Read` | `Read` | `Read` |
| Auditor | `Read` | `Read` | `Read` |

> **Perubahan peran yang MUST disosialisasikan.** Kepala Kasir/Finance Operations kehilangan seluruh kemampuan operasionalnya atas Petty Cash pada revisi ini — bukan karena wewenangnya dicabut, melainkan karena satu-satunya kemampuan yang pernah dimilikinya (`Approve`/`Reject`) sudah tidak ada lagi. Perannya menjadi pemantau. Bila organisasi menghendaki ia tetap memegang kendali operasional, itu keputusan bisnis baru, bukan penyesuaian teknis.

### Kewenangan yang tidak dapat dijaga mesin hak akses

Bagian ini **bertambah satu baris penting** pada revisi ini.

| Kewenangan | Yang menjaganya | Yang **tidak** dijaganya | Risiko |
| --- | --- | --- | --- |
| Pencairan tanpa persetujuan siapa pun | Tidak ada penjaga sebelum fakta. Kontrolnya sepenuhnya audit setelah fakta: ledger append-only, pelaku, waktu, dan nomor voucher | Tidak ada satu pun mekanisme yang mencegah kasir berwenang mencairkan uang untuk penerima fiktif | **Naik dibanding revisi sebelumnya**, dan ini konsekuensi yang disengaja dari `PC-DEC-016`. Mitigasinya administratif: rekonsiliasi fisik kas kecil secara berkala dan pemeriksaan ledger oleh Finance |
| Pembalikan pencairan oleh kasir yang sama | Hak akses `Reverse` beserta alasan wajib dan baris ledger tersendiri | Tidak ada pemeriksaan dua orang. Kasir yang mencairkan boleh membalik pencairannya sendiri (`PC-DEC-022`) | Pencairan dan pembalikannya dapat saling menutupi sehingga saldo akhir terlihat wajar. Yang tetap terbaca: **kedua** barisnya ada permanen di ledger, lengkap dengan pelaku dan waktunya. Deteksinya lewat laporan, bukan lewat pencegahan |
| Penutupan periode oleh Finance | Hak akses `Close` beserta `BIL-VAL-104` | Tidak ada pemeriksaan bahwa periode penerus yang dipilih memang periode yang benar secara bisnis | Sisa saldo dapat berpindah ke periode yang salah. Kedua baris carry-forward tetap terbaca, sehingga koreksinya mungkin lewat penyesuaian beralasan |

### Audit

Tidak berubah bentuknya. Yang bertambah: perintah `RETURNED` dan `REVERSED` pada `BilPettyCashVoucherCommand`, serta empat jenis pergerakan baru pada ledger. Konvensi pencatatan tetap — `GET` tidak dicatat, selain `GET` dicatat.

### Kolom sensitif

Bertambah satu: `BilPettyCashVoucher.ReversalReason` bertanda **Sensitif** dan **MUST NOT** masuk payload custom logger, mengikuti perlakuan `Reason` pada ledger dan `Purpose` pada voucher.

---

## Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED`

`last_changed_in: BIL-PERMISSION-1.0` · status **approved** (`BKC-DEC-105`, 18 September 2026) · input: **`BKC-DEC-100`–`BKC-DEC-102`**; keputusan arsitektur `BKC-DES-028`–`BKC-DES-035`.

### Hak akses — nol butir baru, nol butir dihapus

Amendment ini tidak menambah satu pun butir `[AccessPermission(...)]`, tidak menghapus satu pun, dan tidak mengubah satu pun atribut hak akses yang sudah terpasang. Sebabnya lurus: perpindahan `FINAL`↔`CLOSED` dijalankan **Sistem** (`BKC-DEC-100`), bukan oleh perintah pengguna, sehingga tidak ada endpoint baru yang perlu dijaga.

Hak akses tetap diperiksa di pintu masuk peristiwa yang memicunya, dan seluruh pemeriksaan itu sudah ada hari ini:

| Peristiwa pemicu | Pemeriksaan hak akses yang sudah berlaku di pintu masuknya |
| --- | --- |
| Rekonsiliasi tender (pembayaran) | Butir hak akses pada endpoint rekonsiliasi settlement yang sudah tercatat di tabel dokumen ini |
| Alokasi deposit pasien ke invoice | Butir hak akses pada endpoint alokasi dana pasien yang sudah tercatat |
| Penyesuaian dan write-off beserta kedua jalur pembalikannya | Butir `BillingFinancialException`/write-off yang sudah tercatat |

Tidak ada peran yang perlu diberi butir baru, dan tidak ada peran yang kehilangan kemampuan. Implementer **MUST** memakai butir yang sudah ada apa adanya, **MUST NOT** menambahkan pemeriksaan hak akses baru di dalam service penyelarasan — service itu tidak pernah menjadi pintu masuk permintaan pengguna.

### Audit

Satu jenis catatan audit baru, mengikuti pola yang sudah dipakai modul ini:

| Peristiwa | Kategori | Isi payload | Dicatat logger |
| --- | --- | --- | :---: |
| Invoice berpindah `FINAL` → `CLOSED` atau `CLOSED` → `FINAL` | `HealthServices.BillingManagement.Billing` | `InvoiceId`, status sebelum, status sesudah, sisa tagihan yang terhitung, waktu peristiwa, `ActorUserId` | Ya |

Catatan itu ditulis **sesudah** transaksi peristiwanya berhasil di-commit, mengikuti pola `AuditTenderResultAsync` dan `AuditCorrectionAsync` yang sudah ada — sehingga tidak pernah ada baris audit untuk perpindahan yang ternyata dibatalkan.

`ActorUserId` diisi pengguna yang menjalankan peristiwa pemicunya (kasir yang menerima pembayaran, petugas yang memposting penyesuaian), **bukan** identitas sistem — karena memang tindakan merekalah yang menyebabkan perpindahan itu. Ini juga yang membuat catatan audit dapat menjawab "siapa yang menyebabkan tagihan ini tertutup", pertanyaan yang tidak terjawab bila pelakunya ditulis sebagai sistem.

### Kolom sensitif

Tidak bertambah. Payload audit di atas **MUST NOT** memuat identitas pasien, nomor polis, nomor kartu, maupun payload provider — konsisten dengan aturan yang sudah berlaku pada dokumen ini.

Trace **`BKC-DEC-100`–`102`**, `BKC-DES-028`–`035`.

---

## Amendment 21 September 2026 — Kewenangan atas penerbitan fakta ke modul konsumen

`last_changed_in: BIL-PERMISSION-1.1` · status **draft** · input `BKC-DEC-106`–`109`.

### Butir hak akses baru

Satu Resource baru, `BillingConsumerHandoff`, dengan dua Action. Pemetaan endpoint ke hak
aksesnya dipegang kolom `Hak akses` pada `api-contract.md` dan **tidak** didaftar ulang di sini.

| Peran rumah sakit | `Read` | `Acknowledge` | Alasan |
| --- | :---: | :---: | --- |
| Petugas Billing / Kepala Kasir | Ya | Tidak | Perlu melihat apakah ada surat menggantung; tidak perlu mengakui atas nama konsumen |
| Finance Operations | Ya | Ya | Konsumen sah surat penerimaan uang |
| Petugas Farmasi | Tidak | Tidak | Farmasi tidak memakai permukaan ini sama sekali; ia membaca lewat pemanggilan dalam proses |
| Administrator sistem | Ya | Ya | Pemulihan saat konsumen bermasalah |

### Kewenangan yang **tidak** dapat dijaga mesin hak akses

| Yang tidak dijaga | Risikonya | Penjaga penggantinya |
| --- | --- | --- |
| Penerbitan surat | Tidak ada permukaan yang dapat dipanggil manusia, sehingga tidak ada yang perlu dijaga hak akses. Ini justru penjaganya | Penerbitan hanya terjadi di dalam transaksi peristiwa finansial. Surat yang dapat diterbitkan manual adalah surat yang dapat dipalsukan |
| Kebenaran isi surat | Mesin hak akses tidak dapat menilai apakah nominal yang disalin benar | Nominal disalin apa adanya dari tender; Billing **MUST NOT** menghitung ulang, dan konsumen **MUST NOT** menghitung ulang |
| Pengakuan yang diberikan terlalu dini | Konsumen dapat mengakui surat sebelum benar-benar memprosesnya, lalu kehilangan isinya | Tidak dijaga mesin. Karena itu `BKC-DEC-108` sengaja **tidak** menggantungkan perilaku Billing pada pengakuan — pengakuan hanya keterangan, bukan gerbang |

Baris ketiga penting: pengakuan yang salah tidak merusak apa pun di Billing, justru karena
Billing tidak mempercayainya untuk hal yang penting.

### Audit

| Kejadian | Dicatat | Isi payload |
| --- | :---: | --- |
| Penerbitan surat | Ya | Identitas surat, jenis, identitas tagihan, korelasi. **MUST NOT** memuat nominal maupun identitas pasien |
| Pengakuan penerimaan | Ya | Identitas surat, pelaku, waktu |
| Pembacaan daftar surat menggantung | Tidak | Konvensi project: `GET` tidak dicatat |
| Pembacaan keadaan clearance oleh Farmasi | Tidak | Baca murni dalam proses, bukan endpoint |

### Kolom sensitif dan masa simpan

| Kolom | Tabel | Ketentuan |
| --- | --- | --- |
| `ProviderReference` | `BilCollectionHandoff` | **Sensitif.** **MUST NOT** masuk payload log |
| `ProviderEventId` | `BilCollectionHandoff` | **Sensitif.** **MUST NOT** masuk payload log |

Masa simpan kedua tabel: **selamanya** (`BKC-DEC-109`). Keduanya jejak audit lintas modul yang
membuktikan Billing pernah memberi tahu, dan kapan.

Surat clearance resep **tidak** memuat satu pun kolom klinis — tidak ada nama obat, dosis,
maupun aturan pakai. Batas ini ditegakkan pada bentuk tabelnya, bukan pada aturan logging saja.

Trace `BKC-DEC-106`–`109`, `BKC-DES-036`–`041`.

---

## Amendment 24 September 2026 — Kewenangan Integrasi Rawat Inap ↔ Billing Management

`last_changed_in: BIL-PERMISSION-1.2` · status **active** · input `BKC-DEC-112`–`122`, `BKC-DES-042`–`050`.

### Butir Hak Akses Baru

Satu Resource baru, `BillingInpatient` (`HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT`), untuk mengontrol integrasi data rawat inap dan otorisasi pemulangan finansial pasien.

| Peran Rumah Sakit | `Create` | `Read` | `Clearance` | `ValidateDeposit` | `Acknowledge` | `ReadLatest` | Keterangan & Batas Kewenangan |
| --- | :---: | :---: | :---: | :---: | :---: | :---: | --- |
| Kasir Utama / Petugas Billing | Ya | Ya | Ya | Ya | Tidak | Ya | Memproses beban sewa kamar, evaluasi clearance, validasi deposit tindakan; tidak mengakui surat atas nama bangsal |
| Perawat Bangsal / Nurse Station | Tidak | Ya* | Tidak | Ya | Ya | Ya | Melihat indikator kepulangan & kendala blocker (*field nominal finansial rupiah disembunyikan otomatis jika tanpa hak kasir); mengakui tanda terima surat clearance |
| Finance Operations | Tidak | Ya | Tidak | Ya | Tidak | Ya | Rekonsiliasi piutang & deposit rawat inap |
| Administrator Sistem | Ya | Ya | Ya | Ya | Ya | Ya | Pemulihan operasional dan audit kepatuhan |

*Catatan Perlindungan Privasi Finansial:*
Pada endpoint `GET /invoices/encounter/{encounterId}/inpatient-summary`, jika pemanggil tidak memiliki hak finansial kasir (`BillingInvoice:Read` / `BillingSettlement:Read` / `Financial`), sistem secara otomatis mengosongkan (`null`) rincian nominal `depositRequired`, `depositBalance`, `depositShortfall`, `totalCharges`, dan `outstanding`. Perawat bangsal hanya melihat `financialClearanceStatus`, `canDischarge`, dan `blockerReasons`.

### Audit Log

| Peristiwa | Kategori | Isi Payload | Dicatat Logger |
| --- | --- | --- | :---: |
| Evaluasi / Re-evaluasi Clearance Ranap | `HealthServices.BillingManagement.Billing.InpatientClearance` | `EncounterId`, `InvoiceId`, `ClearanceStatus`, `FinancialVersion`, `ReasonCode`, `ActorUserId` | Ya |
| Pengakuan Surat Handoff Ranap | `HealthServices.BillingManagement.Billing.InpatientClearance` | `HandoffId`, `EncounterId`, `ClearanceStatus`, `ActorUserId`, `AcknowledgedAt` | Ya |
| Validasi Deposit Tindakan Besar | `HealthServices.BillingManagement.Billing.InpatientClearance` | `EncounterId`, `EstimatedCost`, `PatientExcess`, `IsSufficient` | Ya (Info) |
| Inquiry Summary Ranap | — | Konvensi: pembacaan GET tidak dicatat | Tidak |

Kolom Sensitif: Payload audit log **MUST NOT** memuat nama lengkap pasien, NIK, nomor rekam medis, maupun rincian obat/tindakan klinis.

Trace `BKC-DEC-112`–`122`, `BKC-DES-042`–`050`, `BIL-API-1.4`, `BIL-PERMISSION-1.2`.



# Amendment 24 September 2026 — Revisi UI Billing (Revisi 1.6, `BIL-PERMISSION-1.3`)

Status: `draft`. **Nol butir `[AccessPermission]` baru.** Seluruh endpoint yang dipakai
(`BUI-DEC-001`–`015`) sudah ada beserta hak aksesnya masing-masing — lihat
`contracts/api-contract.md` amendment revisi 1.6, kolom "Hak akses". Amendment ini tidak
menambah resource baru maupun action baru.

Satu klarifikasi yang MUST ditegaskan saat implementasi: tombol Refund/Adjustment/Write-Off
yang berpindah lokasi tampilan (`BUI-DEC-013`, dari Menu Pembayaran ke Riwayat Pembayaran)
**tetap memakai butir akses yang sama persis** yang sudah melekat pada aksi itu di lokasi lama
— perpindahan lokasi tampilan **bukan** perpindahan wewenang. Peran yang sebelumnya bisa
mengajukan refund di Menu Pembayaran MUST tetap bisa (dan hanya bisa, tidak lebih) mengajukan
refund di Riwayat Pembayaran.

Trace `BUI-DEC-013`.

---

## Amendment 13 Oktober 2026 — Pelunasan Gabungan Lintas Kunjungan (Revisi 1.9)

> `last_changed_in`: `BIL-PERMISSION-1.3` atas `BIL-PERMISSION-1.2` — status **approved** (14 Oktober 2026).
> Owner: Security Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi; Security Owner belum menyatakan terpisah · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-118`, `BKC-DES-057`–`BKC-DES-062`. Backend SHA: `e049a65b`.

Pemetaan endpoint ke hak akses **tidak** didaftar ulang di sini; ia dipegang kolom `Hak akses`
pada `contracts/api-contract.md` amendment revisi 1.9. Yang dicatat di bawah hanya hal yang tidak
dapat diturunkan dari daftar endpoint.

### Resource hak akses baru

| Resource | Action | Dipakai untuk | Diturunkan menjadi |
|---|---|---|---|
| `BillingConsolidatedSettlement` | `Read` | Melihat daftar tagihan kandidat dan membaca hasil transaksi gabungan | `[AccessPermission("BillingConsolidatedSettlement", "Read")]` |
| `BillingConsolidatedSettlement` | `Create` | Membentuk dan memproses satu transaksi pembayaran gabungan | `[AccessPermission("BillingConsolidatedSettlement", "Create")]` |

Resource ini **MUST** ditambahkan ke seeder hak akses sebelum layar baru diaktifkan; tanpa itu
seluruh endpoint baru akan menolak dengan kode `403` meski kodenya sudah terpasang.

### Peta peran rumah sakit ke butir hak akses

| Peran | `BillingConsolidatedSettlement : Read` | `BillingConsolidatedSettlement : Create` | Alasan |
|---|:---:|:---:|---|
| Kasir | Ya | Ya | Pembentukan pembayaran gabungan adalah pekerjaan kasir di loket |
| Supervisor Kasir | Ya | Ya | Perlu dapat menyelesaikan transaksi yang menggantung di loket bawahannya |
| Petugas Billing (tanpa loket) | Ya | **Tidak** | Perlu memeriksa dan menjelaskan rincian, tetapi tidak menerima uang |
| Finance / AR | Ya | **Tidak** | Hanya membaca untuk rekonsiliasi penerimaan kas |
| Perawat bangsal | **Tidak** | **Tidak** | Tidak pernah menangani uang; nominal rupiah juga tidak ditampilkan kepada perawat |

### Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaganya | Yang **tidak** dijaganya | Risikonya |
|---|---|---|---|
| Keluarga pasien yang memutuskan digabung atau dipisah | **Tidak ada penjaga teknis.** Ini keputusan lisan di loket | Sistem tidak dapat membuktikan keluarga pasien benar-benar setuju digabung | Kasir berpotensi menggabungkan tanpa menanyakan. Mitigasi: catatan kasir pada transaksi, dan kwitansi yang merinci per unit sehingga keluarga dapat memeriksanya di tempat |
| Satu tagihan hanya ikut satu transaksi berjalan | Aturan bisnis `BIL-VAL-138` beserta indeks `IX_BilSettlementInvoice_Invoice_Status` | Mesin hak akses tidak mengetahui konteks ini sama sekali | Dua loket mengalokasikan uang ke tagihan yang sama. Mitigasi ada di lapisan aturan bisnis dan `RowVersion`, bukan di hak akses |
| Urutan pelunasan tidak dapat diatur petugas | Ditetapkan sistem (`BKC-DES-060`), tidak ada endpoint pengubahnya | — | Bila di kemudian hari endpoint pengubah urutan dibuat, ia **MUST** memperoleh Action tersendiri dan jejak audit alasannya |

### Audit

| Kejadian | Lapisan pencatatan | Jejak yang wajib tahan lama |
|---|---|---|
| Pembentukan pembayaran gabungan | Log audit finansial, mengikuti konvensi non-`GET` dicatat | Pengenal transaksi, daftar tagihan peserta, urutan alokasi, identitas kasir, dan shift kasir yang aktif |
| Alokasi per tagihan | Baris `BilPaymentAllocation` yang sudah ada | Nominal per tagihan beserta versi kalkulasi — sudah tersimpan apa adanya |
| Penolakan karena tagihan terikat loket lain | Log audit finansial | Pengenal tagihan dan pengenal transaksi pemegangnya, untuk menelusuri sengketa antar loket |
| Pencetakan ulang kwitansi gabungan | Log audit finansial | Pengenal transaksi dan identitas pencetak; pencetakan ulang **MUST NOT** membuat transaksi baru |

Pembacaan daftar tagihan kandidat memakai `GET` sehingga **tidak** dicatat, mengikuti konvensi
project. Ini pengecualian yang disengaja dan konsisten, bukan kelalaian.

### Kolom sensitif dan masa simpan

Nol kolom bertanda **Sensitif** pada `BilSettlementInvoice` (lihat `data/data-dictionary.md`
amendment revisi 1.9). Payload log untuk tabel ini **MUST** memuat `EntityId`, controller, action,
dan status saja, dan **MUST NOT** memuat diagnosis, keluhan, nomor polis, maupun rincian layanan
klinis — meski rincian itu tersedia pada invoice yang ditunjuk.

Satu catatan privasi khusus: layar kasir menampilkan nominal rupiah lintas kunjungan. Nominal ini
**MUST NOT** ikut terbawa ke layar perawat bangsal, yang hanya berhak melihat status kelayakan
pulang tanpa angka rupiah — aturan yang sudah berlaku sejak `BKC-DEC-117` dan tidak dilonggarkan
oleh amendment ini.

Trace `BKC-DEC-118`, `BKC-DES-057`–`BKC-DES-062`.


---

## Amendment 14 Oktober 2026 — Kelayakan Pulang Lintas Kunjungan (Revisi 2.0)

> `last_changed_in`: `BIL-PERMISSION-1.4` atas `BIL-PERMISSION-1.3` — status **approved** (14 Oktober 2026).
> Owner: Security Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi; Security Owner belum menyatakan terpisah · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-144`, `BKC-DEC-117`, `BKC-DES-063`–`BKC-DES-069`. Backend SHA: `d166cf4f`.

Pemetaan endpoint ke hak akses **tidak** didaftar ulang di sini; ia dipegang kolom `Hak akses` pada
`contracts/api-contract.md` amendment revisi `2.0`. Yang dicatat di bawah hanya hal yang tidak dapat
diturunkan dari daftar endpoint.

### Action baru pada Resource yang sudah ada

| Resource | Action | Status | Dipakai untuk | Diturunkan menjadi |
|---|---|---|---|---|
| `BillingInpatient` | `Reconcile` | **Baru** | Menyelesaikan rekonsiliasi rantai kunjungan: mengikutkan atau mengecualikan satu tagihan beserta alasannya | `[AccessPermission("BillingInpatient", "Reconcile")]` |
| `BillingInpatient` | `Read` | Sudah ada | Membaca rincian kewajiban rantai | — |

Resource `BillingInpatient` sudah ada sejak revisi `1.5`, sehingga amendment ini **tidak** menambah
Resource baru — hanya satu Action. Action itu **MUST** ditambahkan ke seeder hak akses sebelum layar
rekonsiliasi diaktifkan; tanpa itu endpoint rekonsiliasi akan menolak dengan kode `403` meski
kodenya sudah terpasang.

### Peta peran rumah sakit ke butir hak akses

| Peran | `BillingInpatient : Read` | `BillingInpatient : Reconcile` | Alasan |
|---|:---:|:---:|---|
| Kasir | Ya | **Ya** (`BKC-DEC-150`) | Kasir berhadapan langsung dengan keluarga pasien; rekonsiliasi di sini mengonfirmasi fakta hubungan kunjungan. Jejak audit menjadi penjaga utamanya. **Perhatian:** keputusan yang ditetapkan tidak dapat dicabut (amendment revisi `2.1`) |
| Supervisor Kasir | Ya | Ya | Pemegang wewenang pengecualian pada alur-alur lain yang sebanding |
| Petugas Billing (tanpa loket) | Ya | **Tidak** (`BKC-DEC-150`) | Memeriksa dan menjelaskan rincian, tetapi tidak memutuskan |
| Finance / AR | Ya | Tidak | Hanya membaca untuk rekonsiliasi pendapatan |
| Perawat bangsal | **Proyeksi terbatas** | Tidak | Melihat status kelayakan dan penghambat per unit **tanpa nominal rupiah**; tidak pernah melihat rincian rupiah lintas kunjungan |

> `BKC-OQ-119` **sudah dijawab** 14 Oktober 2026 lewat `BKC-DEC-150`: Action `Reconcile` diberikan ke
> Kasir dan Supervisor Kasir. Seeder hak akses ditulis mengikuti baris tabel di atas.

### Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaganya | Yang **tidak** dijaganya | Risikonya |
|---|---|---|---|
| Kebenaran alasan rekonsiliasi | Kolom alasan wajib terisi (`BIL-VAL-144`) dan jejak audit | Mesin tidak dapat menilai apakah alasannya benar atau sekadar diisi formalitas | Petugas dapat mengecualikan tagihan dengan alasan asal-asalan. Mitigasi: alasan tersimpan permanen beserta identitas petugas, dan rincian surat dapat direkonstruksi per versi |
| Batas kedalaman penelusuran | Konstanta di source | Mesin hak akses tidak mengenal konsep ini | Tidak ada endpoint yang dapat mengubahnya, sehingga tidak ada permukaan serangan |
| Satu admisi aktif per episode | Deteksi anomali `MULTIPLE_ACTIVE_ADMISSION` yang menetapkan `NEEDS_RECONCILIATION` | Mesin hak akses tidak menjaga invariant data | Bila terjadi, sistem gagal tertutup dan menyerahkannya ke petugas, bukan memilih sendiri |
| Privasi rupiah terhadap perawat | Proyeksi response yang berbeda per peran (`BKC-DES-069`) | Mesin hak akses hanya menjaga endpoint, bukan isi field | Bila proyeksi perawat keliru memuat rupiah, hak akses **tidak** akan memergokinya. Karena itu `BIL-AT-175` mengujinya secara eksplisit |

### Audit

| Kejadian | Lapisan pencatatan | Jejak yang wajib tahan lama |
|---|---|---|
| Penerbitan surat kelayakan hasil penilaian rantai | Log audit finansial, mengikuti konvensi non-`GET` dicatat | Pengenal kunjungan, status, nomor versi finansial, jumlah tagihan yang dinilai, dan kedalaman rantai |
| Penyelesaian rekonsiliasi | Log audit finansial | Keputusan mengikutkan atau mengecualikan, pengenal tagihan sasaran, alasan, identitas petugas, dan nomor versi surat |
| Deteksi anomali rantai | Log audit finansial | Kode anomali beserta pengenal kunjungan yang memicunya, supaya pola kesalahan data rujukan dapat ditelusuri |
| Pembacaan rincian kewajiban rantai | **Tidak dicatat** | Memakai `GET`, mengikuti konvensi project. Pengecualian yang disengaja dan konsisten, bukan kelalaian |

### Kolom sensitif dan masa simpan

Nol kolom bertanda **Sensitif** pada `BilClearanceObligation` (lihat `data/data-dictionary.md`
amendment revisi `2.0`). Payload log **MUST** memuat `EntityId`, controller, action, dan status saja.

Satu aturan privasi yang mengikat dan tidak dilonggarkan amendment ini: `PatientOutstanding` dan
seluruh nominal rupiah lintas kunjungan **MUST NOT** terbawa ke proyeksi yang dibaca perawat
bangsal, mengikuti `BKC-DEC-117`. Perawat menerima unit layanan dan penanda ada atau tidaknya
penghambat saja.

Trace `BKC-DEC-144`, `BKC-DEC-117`, `BKC-DES-069`.


---

## Amendment 14 Oktober 2026 — Keputusan Rantai Permanen (Revisi 2.1)

> `last_changed_in`: `BIL-PERMISSION-1.5` atas `BIL-PERMISSION-1.4` — status **approved** (14 Oktober 2026).
> Owner: Security Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi; Security Owner belum menyatakan terpisah · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-150`, `BKC-DEC-151`, `BKC-DES-072`. Backend SHA: `d166cf4f`.

### Nol Resource dan nol Action baru

Keputusan permanen ditulis lewat `[AccessPermission("BillingInpatient", "Reconcile")]` yang sudah ada.
Pembacaan riwayat keputusan memakai `[AccessPermission("BillingInpatient", "Read")]`. **Tidak ada**
Action pencabutan, karena pencabutan memang tidak ada. Baris peran pada amendment revisi `2.0` sudah
diperbarui mengikuti `BKC-DEC-150`: Kasir dan Supervisor Kasir memegang `Reconcile`; Petugas Billing
dan Finance/AR hanya `Read`.

### Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaganya | Yang **tidak** dijaganya | Risikonya dan mitigasinya |
|---|---|---|---|
| Kebenaran keputusan mengecualikan | Konfirmasi wajib di layar, alasan wajib, dan jejak audit | Mesin tidak dapat menilai apakah kasir memeriksa dengan benar | **Paling tinggi pada amendment ini.** Satu kekeliruan kasir berlaku permanen untuk rawat inap itu, karena pemilik memilih pengecualian tidak dapat dicabut. Mitigasinya: tagihan yang dikecualikan tetap piutang terbuka di invoicenya sendiri, sehingga uangnya tidak hilang; yang hilang hanya fungsi menahan kepulangan |
| Keputusan tidak diubah lewat jalur di luar aplikasi | Indeks unik tanpa filter `IsDelete` | Akses langsung ke basis data oleh administrator | Di luar jangkauan aplikasi. Log audit finansial menyimpan kejadian penetapan, sehingga penghapusan langsung tetap dapat dibandingkan dengan jejak audit |

### Audit

| Kejadian | Lapisan pencatatan | Jejak yang wajib tahan lama |
|---|---|---|
| Penetapan keputusan rekonsiliasi permanen | Log audit finansial **dan** baris `BilClearanceChainDecision` itu sendiri | Arah keputusan, kunjungan sasaran, kode kejanggalan, identitas petugas, waktu, nomor versi surat. Isi `Reason` tersimpan di baris keputusan, **MUST NOT** masuk payload log |
| Penolakan karena keputusan sudah ada atau arahnya berlawanan | Log audit finansial | Pengenal kunjungan dan kode penolakan, untuk menelusuri sengketa antar petugas |
| Pembacaan riwayat keputusan | **Tidak dicatat** | Memakai `GET`, mengikuti konvensi project |

Trace `BKC-DEC-150`, `BKC-DEC-151`, `BKC-DES-072`.

---

## Amendment 16 Oktober 2026 — Tinjauan Void Keliru dan Kredit Admin Rawat Jalan (Revisi 2.3)

> `last_changed_in`: `BIL-PERMISSION-1.6` atas `BIL-PERMISSION-1.5` — status **draft**.
> Masukan: `BKC-DEC-162`, `BKC-DES-086`, `BKC-DES-089`, arahan A3. Backend SHA: `738e03a0`.
> Satu Resource baru, dua Action. Butir hak akses yang sudah ada tidak berubah.

### Resource dan Action baru

| Resource | Action | Endpoint | `[AccessPermission]` persis | `AccessType` |
|---|---|---|---|---|
| `BillingSupersededItemReview` | `Read` | `GET /superseded-item-reviews`, `GET /superseded-item-reviews/{invoiceItemId}` | `[AccessPermission("BillingSupersededItemReview", "Read")]` | `Read` |
| `BillingSupersededItemReview` | `Decide` | `POST /superseded-item-reviews/{invoiceItemId}/decision` | `[AccessPermission("BillingSupersededItemReview", "Decide")]` | `Update` |

### Pemberian per peran

| Peran | `Read` | `Decide` | Alasan |
|---|:---:|:---:|---|
| Supervisor Kasir | Ya | Ya | `BKC-DEC-162`; dijalankan sendiri tanpa persetujuan berjenjang (A3) |
| Kasir | Tidak | Tidak | `BKC-DEC-162` menetapkan Supervisor Kasir; kasir biasa tidak diberi wewenang |
| Petugas Billing | Tidak | Tidak | Tidak disebut keputusan mana pun |
| Finance | Tidak | Tidak | Antrean `NEEDS_POST_CLOSE_CORRECTION` baru relevan setelah instrumen `BKC-OQ-139` ada; pemberian `Read` ditinjau saat itu |

> **Risiko yang diterima (A3).** Satu orang dapat memulihkan item ke tagihan yang masih terbuka tanpa pemeriksa kedua.
> Kendalinya adalah keputusan final yang tidak dapat diubah, alasan wajib, jejak audit, dan fakta bahwa tagihan masih
> terbuka sehingga kasir dan pasien melihat rinciannya sebelum membayar. Preseden: `PC-DEC-016`/`PC-DEC-022`
> (satu lapis kontrol, audit setelah fakta).

Seeder hak akses ditulis mengikuti baris tabel di atas.

### Peristiwa audit baru

Seluruhnya lewat `_loggerService.AuditAsync`, kategori Billing. Payload **MUST NOT** memuat nama pasien, NIK, nomor rekam medis, maupun diagnosis.

| Peristiwa | Kapan | Payload |
|---|---|---|
| `BillingSupersededItemReview.Decide` | Keputusan tinjauan tersimpan atau di-replay | `ReviewId`, `InvoiceItemId`, `InvoiceId`, `Decision`, `ReinstatedInvoiceItemId`, `ActorUserId`, `IsReplay` |
| `BillingRefundableCredit.RecognizeReferredAdmin` | Kredit refund admin Rawat Jalan lahir saat finalisasi | `CreditId`, `InpatientInvoiceId`, `Amount`, `CalculationVersionNo`, `ActorUserId` |
| `BillingRefundableCredit.ReferredAdminDrift` | Finalisasi ulang menghasilkan sisa kredit berbeda dari kredit yang sudah ada | `CreditId`, `InpatientInvoiceId`, `ExistingAmount`, `NewUnappliedAmount`, `ActorUserId` |
| Kalkulasi invoice | **Tidak** ada peristiwa baru | Penggantian dan kredit tercatat permanen di versi kalkulasi (*append-only*) |
| Pembacaan daftar dan detail tinjauan | **Tidak dicatat** | Memakai `GET`, mengikuti konvensi project |

Trace `BKC-DEC-158`, `BKC-DEC-162`, `BKC-DES-086`, `BKC-DES-089`, A3. Tests: `BIL-AT-221`, `BIL-AT-224`.

## Amendment 26 Oktober 2026 — Pelaksana Entri Kasir dan `DoctorShare` dari Medical Fee (Revisi 2.4)

> `last_changed_in`: `BIL-PERMISSION-1.7` atas `BIL-PERMISSION-1.6` — status **draft**.
> Masukan: `BKC-DEC-169`, `172`, `173`, `178`; `BKC-DES-093`..`095`. Backend SHA: `ecddd8f8`.
> **Nol Resource dan nol Action baru.** Yang bergerak hanya peristiwa audit dan catatan kewenangan.

### Hak akses

| Hal | Keadaan |
|---|---|
| Menandai berjasa dan memilih pelaksana | Bagian dari `BillingInvoice : Create` yang sudah ada pada `POST catalog-charges` dan `POST other-charges`. Tidak ada Action terpisah |
| Membaca pelaksana dan asal `DoctorShare` | Bagian dari `BillingInvoice : Read` |
| Membaca daftar dokter | Milik HR (`GET .../master-data/doctors/options`, `[Authorize(Policy = KioskReadPolicy)]`). Pemberian untuk peran Kasir **perlu diverifikasi**, bukan dirancang di sini |

### Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga | Yang dijaganya | Yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| `BIL-VAL-161`..`165` | Pelaksana ada, aktif, dan berjenis sah | Bahwa pelaksana yang dipilih **benar-benar** yang mengerjakan layanan | Jasa jatuh ke orang yang salah. Risiko ini **diterima** Medical Fee (`MF-DEC-017`, "bergantung pada ketelitian kasir") dan Billing (`BKC-DEC-172`). Kendalinya: pelaksana tampil pada rincian tagihan dan pada verifikasi periode Medical Fee |
| `INV-MF-4` (`BKC-DES-098`) | Dua pelaksana tidak tergabung dalam satu baris | — | — |
| Port tanpa Action | Medical Fee tidak dapat dipanggil dari luar lewat port ini | — | Port in-process; tidak ada permukaan HTTP baru |

### Peristiwa audit

Seluruhnya lewat `_loggerService.AuditAsync`, kategori Billing. Payload **MUST NOT** memuat nama pasien, NIK, nomor rekam medis, diagnosis,
maupun nama pelaksana. Persentase sharing tidak dicatat.

| Peristiwa | Kapan | Payload |
|---|---|---|
| `BillingInvoice.DoctorShareUnavailable` | **Baru.** Medical Fee menjawab `Unavailable`, melempar kesalahan, atau melewati batas waktu | `InvoiceId`, `InvoiceItemId`, `SourceDomain`, `PerformerType`, `Cause` (`NOT_INSTALLED`/`TIMEOUT`/`ERROR`/`UNAVAILABLE`), `CorrelationId`, `ActorUserId` |
| `BillingInvoice.CreateCharge`, `BillingInvoice.UpsertCharge` | Sudah ada | Ditambah `IsFeeBearing` dan `DoctorShareResolution`. Tidak ditambah `PerformerReferenceId` |
| `NoRule` dan `NOT_APPLICABLE` | **Tidak** ada peristiwa | Tersimpan permanen di `DoctorShareResolution` |

Trace `BKC-DEC-169`, `172`, `178`; `BKC-DES-097`. Tests: `BIL-AT-232`..`234`.
