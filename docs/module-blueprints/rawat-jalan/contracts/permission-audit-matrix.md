# Permission and Audit Matrix — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-PERM-001@1.0.0` |
| Status | `draft` |
| Catatan | String permission AS-IS dipertahankan; permission target baru perlu registry/security approval |

| Endpoint | Resource | Action | String yang dipakai | Audit logger |
|---|---|---|---|:---:|
| `GET /by-encounter/{encounterId}` | `BillingFolio` | `Read` | `[AccessPermission("BillingFolio", "Read")]` | Tidak |
| `GET /{folioId}` | `BillingFolio` | `Read` | `[AccessPermission("BillingFolio", "Read")]` | Tidak |
| `POST /internal/milestones/recognize` | `BillingMilestone` | `RecognizeInternal` | `[AccessPermission("BillingMilestone", "RecognizeInternal")]` | Ya |
| `POST /{folioId}/allocations` | `BillingAllocation` | `Create` | `[AccessPermission("BillingAllocation", "Create")]` | Ya; rencana |
| `POST /{folioId}/financial-actions` | `BillingFinancialAction` | `Create` | `[AccessPermission("BillingFinancialAction", "Create")]` | Ya; rencana |
| `POST /{folioId}/close` | `BillingFolio` | `Close` | `[AccessPermission("BillingFolio", "Close")]` | Ya; rencana |
| `POST /{folioId}/reopen` | `BillingFolio` | `Reopen` | `[AccessPermission("BillingFolio", "Reopen")]` | Ya; rencana |
| `POST /{id}/execute` | `BillingFinancialAction` | `Execute` | `[AccessPermission("BillingFinancialAction", "Execute")]` | Ya; rencana |
| `POST /{id}/resolve` | `BillingReconciliation` | `Resolve` | `[AccessPermission("BillingReconciliation", "Resolve")]` | Ya; rencana |

Audit wajib menyimpan actor, action, entity/reference ID, prior/new status, reason, policy/rule
version, correlation, dan timestamp. Idempotency key hanya dicatat sebagai hash/reference;
payload klinis sensitif dan raw payer payload tidak masuk custom logger.



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.0` |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Owner | Security/Privacy + Billing |
| Traceability | `RJ-E2E-DEC-008`, `009`, `SEC-RJ-001`..`006` |

Pemetaan endpoint ke hak akses **hanya** ada di kolom `Hak akses` pada
[api-contract.md](api-contract.md) bagian V2, dan tidak didaftar ulang di sini. String atribut
diturunkan langsung: `[AccessPermission("<Resource>", "<Action>")]`. Pencatatan logger mengikuti
konvensi project: `GET` tidak dicatat, selain `GET` dicatat. **Tidak ada pengecualian** pada
amendment ini.

## V2-1. Cara kerja hak akses di repository ini

| Lapisan | Rujukan source | Peran |
|---|---|---|
| Registrasi resource dan action | `[AccessController]` di kelas, `[AccessAction]` di action — contoh `PatientBillingSummaryController.cs:32-40`, `:62` | Butir hak akses terdaftar otomatis dari atribut |
| Penegakan | `[Authorize]` + `[AccessPermission]` (filter) | Menolak `401` tanpa sesi, `403` tanpa butir |
| Frontend | `usePermission(resource, action)` (`src/lib/hooks/auth/use-permission.jsx`) dan `AccessDeniedGate` | Menyembunyikan tab dan tombol. **Bukan** batas keamanan (`SEC-RJ-001`) |

## V2-2. Butir hak akses baru

| Resource | Action | Makna |
|---|---|---|
| `EncounterBillingSummary` | `Read` | Membaca ringkasan tagihan **satu** kunjungan tanpa harga per item |
| `BillingChargeReconciliation` | `Read` | Membaca antrean rekonsiliasi |
| `BillingChargeReconciliation` | `Update` | Mengirim ulang dan menyelesaikan item antrean |
| `BillingSyncPolicy` | `Read` | Membaca kebijakan kirim ulang |
| `BillingSyncPolicy` | `Update` | Mengubah kebijakan kirim ulang |

`EncounterBillingSummary : Read` **berdiri sendiri** dan sengaja tidak menumpang
`BillingInvoice : Read`. Butir `BillingInvoice : Read` membuka daftar seluruh invoice, riwayat
pembayaran, dan `cashier-overview`; memberikannya ke dokter berarti dokter dapat membaca tagihan
semua pasien lewat API (`RJ-E2E-DEC-008`).

## V2-3. Peta peran ke butir hak akses

| Peran | Butir yang diberikan | Tidak diberikan |
|---|---|---|
| Dokter Rawat Jalan | `EncounterBillingSummary : Read` | Semua butir `BillingInvoice`, `BillingDiscount`, `BillingChargeReconciliation`, `BillingSyncPolicy` |
| Perawat poli | `EncounterBillingSummary : Read` (opsional, keputusan admin) | Sama dengan dokter |
| Petugas Billing | `EncounterBillingSummary : Read`, `BillingInvoice : Read`, `BillingChargeReconciliation : Read/Update` | `BillingSyncPolicy : Update` |
| Supervisor / Admin Billing | Seluruh butir di atas + `BillingSyncPolicy : Read/Update` | — |
| Kasir | `BillingInvoice : *` sesuai peran kasir yang ada | `BillingChargeReconciliation : Update` |

**Contoh:** dr. B memegang `EncounterBillingSummary : Read` saja. Tab Ringkasan Billing tampil;
tombol *Buka Detail Billing* tidak tampil. Bila dr. B memanggil `GET /billing/invoices/{id}` lewat
Postman, backend menjawab `403` (`AC-RJ-015`).

## V2-4. Kewenangan yang tidak dapat dijaga mesin hak akses

| Penjaga aturan bisnis | Yang dijaga | Yang **tidak** dijaga | Risiko |
|---|---|---|---|
| Jembatan hanya menerima baris dari folio, bukan dari HTTP | Harga dan jumlah tidak dapat disusupkan pengguna | Kebenaran data tarif di `MstTariff` | Tarif salah input menghasilkan tagihan salah — tanggung jawab admin tarif |
| `from-source` menolak domain klinis Rawat Jalan | Kasir tidak dapat mencatat ulang pelayanan Rawat Jalan dengan harga bebas | Domain `ADHOC` (biaya lain-lain kasir) tetap berharga bebas, sesuai keputusan Billing `BKC-DEC-047` | Di luar scope amendment |
| Adjustment dari jembatan berstatus `SUBMITTED` | Tidak ada pengurangan/penambahan tagihan pasca-final tanpa persetujuan | Siapa penyetujunya — mengikuti aturan Billing yang ada | — |
| Penyelesaian manual wajib catatan | Setiap keputusan manual tertelusur | Kebenaran isi catatan | Diperiksa audit berkala |

## V2-5. Audit

| Kejadian | Lapisan | Wajib tercatat |
|---|---|---|
| Jembatan sinkron, void, adjustment | `LoggerService.AuditAsync` | `ActorUserId`, aksi, `EncounterId`, `SourceDomain`, `SourceDetailId`, `SourceVersion`, `InvoiceId`, waktu, `CorrelationId`, hasil |
| Kirim ulang otomatis | Log aplikasi + kolom percobaan pada baris | Jumlah percobaan dan kode kegagalan |
| Kirim ulang dan penyelesaian manual | `LoggerService.AuditAsync` + kolom `Reconciliation*` | Petugas, waktu, jenis penyelesaian, catatan |
| Perubahan kebijakan kirim ulang | `LoggerService.AuditAsync` | Nilai lama dan baru |

## V2-6. Kolom sensitif dan masa simpan

Tidak ada kolom baru pada amendment ini yang bertanda sensitif (lihat
[data/data-dictionary.md](../data/data-dictionary.md)). `ReconciliationResolutionNote` diisi petugas
Billing dan **tidak boleh** memuat diagnosis atau isi klinis; layar penyelesaian menampilkan
peringatan itu. Payload log **tidak boleh** memuat nama pasien, diagnosis, atau isi `RuleSnapshot`.
Masa simpan mengikuti tabel induknya (tidak ada penghapusan fisik; `IsDelete`).


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`, `draft`)

`last_changed_in`: `RJ-DOC-ENCLIST-001@1.0.0` · Owner: Sukma Giri (Security/Privacy authority) ·
Menutup `RJ-DOC-OQ-010` (menunggu approval).

## DP-1. Butir hak akses baru

Resource `OutpatientEncounter`, modul `HEALTH_SERVICE_REGISTRATION_MANAGEMENT`.

| Action | Didaftarkan lewat | String | Makna |
|---|---|---|---|
| `Read` | `[AccessAction("Read", …)]` pada tiga `GET` | `[AccessPermission("OutpatientEncounter", "Read")]` | Membuka layar dan membaca kunjungan **dalam cakupan** |
| `Cancel` | `[AccessAction("Cancel", "Cancel Outpatient Encounter", AccessType = AccessTypes.Update)]` | `[AccessPermission("OutpatientEncounter", "Cancel")]` | Membatalkan kunjungan dalam cakupan |
| `ReadAll` | `[assembly: AccessExplicitPermission(moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT", resourceName: "OutpatientEncounter", actionName: "ReadAll", displayName: "Read All Outpatient Encounter", accessType: AccessTypes.Read)]` | Diperiksa `AccessPermissionService.HasAccessAsync(user, "OutpatientEncounter", "ReadAll")` | Penanda: melihat kunjungan **semua** klinik. Tidak menjaga endpoint mana pun sendirian |

Butir ini **terpisah** dari `PatientEncounter : Read/Update`: pemegang `PatientEncounter : Read`
saat ini membaca semua kunjungan tanpa cakupan; memakai ulang butir itu berarti memberi
"lihat semua" kepada setiap dokter.

## DP-2. Peta peran (saran awal, diatur lewat layar Akses Role)

| Peran | `Read` | `ReadAll` | `Cancel` |
|---|:---:|:---:|:---:|
| Petugas pendaftaran | ✓ | ✓ | ✓ |
| Perawat poli / kepala ruangan | ✓ | | ✓ |
| Dokter | ✓ | | sesuai kebijakan RS |
| Super Admin | ✓ | ✓ | ✓ |

## DP-3. Kewenangan yang tidak dijaga mesin hak akses

| Kewenangan | Penjaganya |
|---|---|
| Cakupan dokter/perawat | `ClinicalActorScopeService` di server — bukan butir hak akses |
| Boleh batal menurut status | `OutpatientEncounterRules` di server |

## DP-4. Audit dan privasi

| Aksi | Dicatat di | Isi |
|---|---|---|
| Batalkan | Baris kunjungan | `CancelledByUserId`, `CancelledAt`, `CancelReason`, `CancelBy`, `CancelDateTime` |
| Batalkan | Custom logger | `EntityId`, controller, action, status hasil — **tanpa** alasan, nama pasien, no. RM |
| `GET` | — | Tidak dicatat (pola backend) |

Kolom sensitif pada respons: `patientName`, `medicalRecordNumber`. Hanya untuk kunjungan dalam
cakupan; tidak boleh muncul di log maupun penyimpanan peramban.

---

# Amendment KT — Konsultasi Tertunda (`RJ-DOC-PENDCONS-001@1.0.0`, `draft`)

`last_changed_in`: `RJ-DOC-PENDCONS-001@1.0.0` · Owner: Sukma Giri (Security/Privacy authority)

**Tidak ada butir hak akses baru.**

| Aksi | Butir | Penjaga tambahan (lama) |
|---|---|---|
| Lihat Konsultasi tertunda | `[AccessPermission("DoctorQueue", "Read")]` | Cakupan dokter `ResolveAllowedDoctorIdAsync` |
| Simpan | `[AccessPermission("DoctorQueue", "FinishConsultation")]` | Cakupan antrean, penjaga penulis, keutuhan dokumen |
| Batalkan konsultasi | `[AccessPermission("DoctorConsultation", "Cancel")]` | Penjaga penulis tunggal (`EnsureSoleAuthorAsync`) |
| Penanda `canCancelConsultation` | `AccessPermissionService.HasAccessAsync(user, "DoctorConsultation", "Cancel")` | Hanya tampilan |

| Audit | Pencatat |
|---|---|
| Simpan | Endpoint finalisasi lama (logger + registrasi keutuhan dokumen) |
| Batalkan konsultasi | Endpoint batal lama: `CancelledByUserId`, `CancelledAt`, `CancelReason`, registrasi keutuhan `Cancelled` |
| Membaca daftar | Tidak dicatat, sama dengan daftar antrean hari ini |

Catatan risiko: jalur `IsCurrentUserSuperAdminAsync` (nama role) adalah utang teknis existing di
`DoctorQueueController`; fitur ini tidak menambah pemeriksaan role baru (`02` KT.3.2).


# Amendment PM-B — `RJ-DOC-REFERRAL-001@1.0.0`

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-DOC-REFERRAL-001@1.0.0` — `approved` |
| Owner | Sukma Giri |
| `approved_by` / `approved_at` | Sukma Giri / 2026-10-08 |
| `input_revision` | Decision log *Amendment PM-B* (`RJ-DOC-DEC-068`..`082`) |

| Aksi | Petugas pendaftaran | Kiosk | Petugas master data | Audit |
|---|---|---|---|---|
| Buat kunjungan rujukan (poli) | `PatientEncounter : Create` | Policy `KioskRead` | — | Kolom audit kunjungan |
| Buat kunjungan rujukan (Lab) | `LabPatientRegistration : Create` + `PatientEncounter : Update` | — | — | Kolom audit + revisi |
| Lihat rincian & unduh surat | `PatientEncounter : Read` | Tidak | — | — |
| Lengkapi / koreksi rincian, kelola berkas | `PatientEncounter : Update` | Unggah saja (batas `RJ-VAL-PM-14`) | — | `RegEncounterReferralRevision` |
| Pilihan Institusi/Dokter Perujuk | `ReferralInstitution : Read`, `ReferralDoctor : Read` | `kiosk/options` (`KioskRead`) | — | — |
| Kelola master Institusi/Dokter Perujuk, tanda mitra | — | — | `ReferralInstitution` / `ReferralDoctor` : Create/Update/Delete | Kolom audit master |
| Tambah penjamin asuransi dengan scan | `PatientInsurance : Create` | Policy `KioskRead` | — | Kolom audit penjamin |

Privasi: `DiagnosisId`, `DiagnosisNote`, `ReferralReason`, dan berkas surat termasuk data
kesehatan. Data ini tidak masuk log, tidak masuk URL, dan berkasnya tidak dapat dibuka lewat
`/uploads`.
