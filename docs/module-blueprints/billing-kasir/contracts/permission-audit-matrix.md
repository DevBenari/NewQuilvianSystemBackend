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

`contract_version: BIL-PERMISSION-0.5` · status **draft** · owner Security dan process owner · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`.

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

`last_changed_in: BIL-PERMISSION-0.6` · status **draft** · owner Security dan process owner · `approved_by`/`approved_at`: belum ada. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`.

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
