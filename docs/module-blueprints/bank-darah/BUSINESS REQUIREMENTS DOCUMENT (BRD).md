# BUSINESS REQUIREMENTS DOCUMENT (BRD)
# Modul Bank Darah — Quilvian V2

**Document ID:** BRD-BD-V2  
**Module Key:** `bank-darah`  
**Status:** DRAFT — BLUEPRINT AUTHORITY  
**Target:** Quilvian V2  
**Dokumentasi Canonical:** `docs/module-blueprints/bank-darah/`

## 1. Baseline

### Repository

- Backend:
  - Repository: `DevBenari/NewQuilvianSystemBackend`
  - Branch: `QuilvianIntegrationBackend`
  - Baseline SHA: `8b298bbd1b135527dc58fba3443b978da2473dd1`

- Frontend:
  - Repository: `DevBenari/QuilvianSystemFrontendDev`
  - Branch: `QuilvianDevV2`
  - Baseline SHA: `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254`

- Engineering Skills:
  - Repository: `DevBenari/QuilvianEngineeringSkills`
  - Branch: `main`
  - Baseline SHA: `48279dda40d9fe605d7bb5e97031763378f9c52e`

### Requirement Evidence

1. `Bank Darah(1).md`
2. `Artifact_Bank_Darah_Bagian_Kedua(1).md`
3. `Bank_Darah_Bagian_Ketiga(1).md`

## 2. Evidence Classification

Setiap requirement, entity, endpoint, flow, task, dan keputusan WAJIB memiliki salah satu klasifikasi:

- `[EVIDENCE]` — terlihat/dibuktikan oleh evidence Bank Darah.
- `[REPO-EXISTING]` — dibuktikan sudah tersedia pada source V2.
- `[PROPOSED]` — desain V2 yang diperlukan untuk memenuhi requirement.
- `[UNRESOLVED]` — membutuhkan keputusan manusia atau evidence tambahan.

Tidak boleh menaikkan `[PROPOSED]` atau `[UNRESOLVED]` menjadi fakta existing tanpa bukti repository.

---

# 3. Latar Belakang Bisnis

Bank Darah dibutuhkan sebagai bounded context operasional untuk mengelola pemenuhan kebutuhan darah pasien dari order sampai kantong diberikan atau dikembalikan.

Evidence memperlihatkan Bank Darah menerima konteks order pasien dari pelayanan rumah sakit, memperlihatkan permintaan komponen darah, jumlah kantong, golongan darah dan Rhesus, menyediakan stok kantong yang dapat dipilih, mencatat pemberian darah, restock/pembatalan, serta tindakan/biaya Bank Darah.

Bank Darah juga mempunyai keterkaitan terbatas dengan:

- Rawat Jalan.
- Rawat Inap.
- IGD.
- Pasien dan registrasi/encounter.
- Dokter perujuk.
- Petugas/Dokter BDRS.
- Billing/tarif.
- Laboratorium untuk Label Golongan Darah.
- Sysmex HCLAB melalui workstation `BANK DARAH`, code `BBW`, Lab Sec `GL`.

Hubungan teknis dan clinical authority dari dua integrasi terakhir belum lengkap dan tidak boleh diasumsikan.

---

# 4. Business Problem

Tanpa bounded context Bank Darah yang eksplisit, terdapat risiko:

1. order darah tidak memiliki lifecycle yang terkontrol;
2. jumlah kantong dipesan, disediakan, dan diberikan menjadi tidak sinkron;
3. satu kantong dapat dialokasikan lebih dari sekali apabila concurrency tidak dikendalikan;
4. perubahan stok tidak dapat ditelusuri;
5. pembatalan/restock dapat menghilangkan histori;
6. status klinis dan inventori tercampur dengan UI state;
7. tindakan Bank Darah dan billing dapat menghasilkan duplikasi transaksi;
8. integrasi Label Goldar/HCLAB berisiko dibuat berdasarkan asumsi;
9. role dan permission Bank Darah menjadi terlalu luas;
10. data master pasien, dokter, department, visit, dan tarif berpotensi diduplikasi.

---

# 5. Business Goals

## BG-BD-001 — Order Traceability

Setiap order darah harus dapat ditelusuri dari:

`patient/encounter -> order -> order item -> blood unit allocation -> issue/return/cancel`

## BG-BD-002 — Inventory Integrity

Sistem harus mencegah satu kantong/unit darah berada pada dua transaksi aktif yang bertentangan.

## BG-BD-003 — Fulfillment Visibility

Petugas harus dapat melihat minimal:

- jumlah kantong dipesan;
- jumlah kantong sudah diberikan;
- jumlah kantong belum diberikan.

Nilai tersebut harus berasal dari transaksi, bukan counter yang dapat diedit manual.

## BG-BD-004 — Auditability

Semua perubahan lifecycle penting harus menyimpan:

- actor;
- timestamp;
- business reference;
- before/after state bila relevan;
- alasan untuk pembatalan/return/restock/void.

## BG-BD-005 — Safe Integration

Patient, encounter, dokter, department/location, kelas, tarif, billing, workforce, dan master bersama lain tetap dimiliki bounded context asal.

Bank Darah hanya menyimpan identifier/reference yang diperlukan.

## BG-BD-006 — Backend-First Contract

API contract, lifecycle, ownership data, error contract, RBAC, dan dependency contract wajib dibekukan sebelum implementasi frontend dimulai.

## BG-BD-007 — Security

Setiap operasi harus:

- authenticated;
- authorized;
- divalidasi server-side;
- memiliki object-level authorization bila diperlukan;
- aman terhadap mass assignment;
- aman terhadap duplicate request;
- tidak membocorkan PHI atau internal exception.

---

# 6. Actors

## ACT-BD-001 — Petugas BDRS / Bank Darah

[EVIDENCE]

Melakukan aktivitas operasional Bank Darah termasuk melihat order, stok, memilih kantong, tindakan Bank Darah, pemberian darah dan aktivitas administratif yang terlihat pada evidence.

## ACT-BD-002 — Dokter BDRS

[EVIDENCE]

Terlihat sebagai salah satu field pada proses tindakan/order Bank Darah.

Authority klinis detail masih `[UNRESOLVED]`.

## ACT-BD-003 — Petugas Pengambil Contoh Darah

[EVIDENCE]

Terlihat sebagai informasi pada data order.

## ACT-BD-004 — Dokter Perujuk

[EVIDENCE]

Terikat dengan order darah dari pelayanan.

## ACT-BD-005 — Pengguna Pelayanan

[PROPOSED]

Pengguna Rawat Jalan, Rawat Inap atau IGD dapat menjadi originating actor dari permintaan apabila source V2 membuktikan ownership tersebut.

Permission aktual harus diambil dari architecture authorization V2.

---

# 7. Business Scope MVP

## 7.1 P0 — Mandatory Core Scope

### BR-BD-001 — Daftar Order Bank Darah

Sistem menyediakan daftar order dengan minimal kemampuan pencarian/filter yang evidence-backed:

- periode;
- No. RM;
- No. RM lama apabila masih tersedia pada patient contract V2;
- nama pasien;
- tipe registrasi:
  - Semua;
  - Rawat Jalan;
  - Rawat Inap;
  - IGD.

### BR-BD-002 — Monitoring Order

Untuk setiap order tampil minimal:

- identitas order;
- pasien;
- tanggal order;
- ruangan/poli;
- jenis komponen;
- jumlah order kantong;
- jumlah telah diberikan;
- jumlah belum diberikan;
- status.

### BR-BD-003 — Detail Order

Detail harus menghubungkan order dengan:

- patient reference;
- encounter/registration reference;
- requesting location;
- referring doctor;
- requested component;
- quantity;
- ABO/Rh bila authoritative source tersedia;
- tanggal/waktu;
- catatan/keterangan.

### BR-BD-004 — Tindakan Bank Darah

Sistem mendukung pencatatan tindakan yang evidence-backed, dengan konteks:

- department;
- Dokter BDRS;
- Petugas BDRS;
- tanggal/waktu;
- kelas;
- tindakan;
- tariff/billing reference.

Bank Darah TIDAK boleh membuat tariff engine baru apabila Billing/Finance sudah menjadi owner tarif.

### BR-BD-005 — Inventory Darah

Sistem menyediakan pencarian/monitoring blood unit berdasarkan data yang telah terbukti:

- komponen;
- golongan darah;
- Rhesus;
- identifier kantong/unit;
- availability/status.

Field inventori tambahan harus berasal dari repository atau keputusan bisnis baru.

### BR-BD-006 — Allocation Kantong

Petugas dapat memilih blood unit yang tersedia untuk memenuhi order.

Sistem harus menjamin secara transactional:

- blood unit valid;
- blood unit tersedia;
- blood unit belum dialokasikan secara conflicting;
- order masih dapat diproses;
- jumlah yang dialokasikan tidak merusak invariant order.

### BR-BD-007 — Pemberian/Issue Darah

Sistem dapat menandai blood unit sebagai diberikan/issued terhadap order tertentu.

Operation harus:

- atomic;
- auditable;
- concurrency-safe;
- idempotent terhadap duplicate request.

### BR-BD-008 — Partial Fulfillment

Order lebih dari satu kantong dapat berada dalam kondisi pemenuhan sebagian.

Contoh:

`3 ordered -> 1 issued -> 2 outstanding`

Jumlah harus dihitung dari transaksi aktual.

### BR-BD-009 — Restock / Return

Blood unit yang memenuhi rule return dapat dikembalikan ke status yang ditentukan contract.

Wajib merekam:

- alasan;
- actor;
- timestamp;
- source transaction.

Detail kelayakan medis blood unit untuk kembali ke stok adalah `[UNRESOLVED]` dan tidak boleh ditebak.

### BR-BD-010 — Cancel / Void Order

Evidence memperlihatkan aksi pembatalan dan `Hapus Order Bank Darah`.

Di V2:

- transaksi klinis yang sudah mempunyai histori TIDAK boleh di-hard-delete;
- aksi tersebut harus dimodelkan sebagai cancel/void dengan audit trail;
- hard-delete hanya boleh dipertimbangkan untuk draft yang belum mempunyai dependent transaction dan setelah dibuktikan aman.

### BR-BD-011 — Label Golongan Darah

Sistem harus memetakan requirement `Label Goldar / Label Golongan Darah`.

Namun sebelum implementasi, kontrak harus menjawab:

- sumber authoritative ABO/Rh;
- siapa yang memvalidasi;
- kapan label boleh dicetak;
- isi label;
- identifier unik;
- apakah merupakan capability Laboratorium atau Bank Darah.

Sampai terjawab, statusnya `[UNRESOLVED]`.

### BR-BD-012 — Sampling Golongan Darah

Aksi sampling terlihat pada evidence Bank Darah.

Data source, specimen lifecycle, validator, dan integrasi Laboratorium masih harus diverifikasi sebelum contract dikunci.

---

# 8. P1 — Integration Discovery

## BR-BD-013 — Laboratory Integration

Verifikasi hubungan Bank Darah dengan:

`Hasil/Riwayat Lab-Patologi Klinik -> Label Goldar`

Jangan membuat coupling langsung sebelum ownership data diketahui.

## BR-BD-014 — HCLAB Integration

Evidence hanya membuktikan:

- workstation: `BANK DARAH`;
- code: `BBW`;
- Lab Sec: `GL`;
- tersedia di `Workstation Results`.

Claude harus membuat integration discovery document, bukan mengarang protokol.

## BR-BD-015 — Reporting

Menu `Laporan` terlihat pada source evidence tetapi report detail tidak terbukti.

Semua report harus `[UNRESOLVED]` sampai field/output diketahui.

## BR-BD-016 — Setup

Menu `Setup` terlihat tetapi konfigurasi internal tidak terbukti.

Tidak boleh membuat master/configuration hanya berdasarkan nama menu.

---

# 9. Explicitly Out of Scope Until Approved

Capability berikut TIDAK menjadi requirement MVP hanya karena umum pada Bank Darah:

- donor management;
- donor registration;
- blood collection;
- donor eligibility;
- component production;
- infectious disease screening;
- quarantine;
- blood unit clinical release;
- crossmatch engine;
- clinical compatibility matrix;
- antibody screening;
- transfusion reaction management;
- post-transfusion surveillance;
- expiry disposition;
- destruction/disposal;
- PMI integration;
- automatic clinical decision making.

Masing-masing harus tetap `[UNRESOLVED]` atau future scope sampai ada requirement/evidence/decision authority.

---

# 10. Core Business Invariants

## INV-BD-001

Satu blood unit identifier harus unik.

## INV-BD-002

Satu blood unit tidak boleh mempunyai lebih dari satu active conflicting allocation.

## INV-BD-003

Issued quantity tidak boleh dihitung dari field counter manual.

## INV-BD-004

`issued + outstanding` harus konsisten terhadap requested quantity sesuai lifecycle contract.

## INV-BD-005

Cancelled/void transaction tidak boleh menghapus audit history.

## INV-BD-006

Setiap perubahan inventory harus mempunyai business reference.

## INV-BD-007

Server adalah authority atas status.

Frontend tidak boleh menentukan transition hanya berdasarkan UI.

## INV-BD-008

State-changing request harus concurrency-safe.

Optimistic concurrency, row version, locking atau strategi yang sesuai harus dipilih berdasarkan pattern repository.

## INV-BD-009

Duplicate HTTP request tidak boleh menyebabkan satu blood unit ter-issue dua kali.

## INV-BD-010

Patient/encounter/doctor/workforce/department/tariff master tidak boleh diduplikasi ke schema Bank Darah.

---

# 11. Proposed Lifecycle

Lifecycle berikut adalah target desain dan masih harus divalidasi terhadap source:

### Order

`REQUESTED -> PROCESSING -> PARTIALLY_FULFILLED -> FULFILLED`

Alternative:

`REQUESTED/PROCESSING/PARTIALLY_FULFILLED -> CANCELLED`

Jika V2 mempunyai canonical status convention lain, gunakan convention repository tersebut.

### Blood Unit

Minimum technical lifecycle:

`AVAILABLE -> RESERVED -> ISSUED`

Return/restock transition ditentukan setelah business validation.

Status seperti:

- QUARANTINED;
- EXPIRED;
- DISCARDED;
- CROSSMATCHED;

TIDAK boleh ditambahkan sebagai fakta requirement tanpa authority.

---

# 12. Ownership Boundary

Bank Darah boleh memiliki:

- blood bank order;
- blood bank order item;
- blood unit inventory representation bila repository membuktikan Bank Darah sebagai owner;
- unit allocation;
- unit movement;
- blood bank procedure association;
- module-specific audit/state.

Bank Darah tidak boleh menjadi owner baru dari:

- Patient;
- Encounter/Registration;
- Doctor;
- Employee/Workforce;
- Department;
- Ward/Polyclinic;
- Patient Class;
- Tariff;
- Invoice;
- Payment;
- general Laboratory Result.

Data tersebut harus direferensikan melalui contract.

---

# 13. Billing Requirement

Tindakan Bank Darah mempunyai indikasi tarif pada evidence.

Target architecture:

`Bank Darah -> Billing Contract -> Billing-owned transaction`

Bank Darah tidak boleh:

- menghitung ulang authoritative tariff secara independen;
- membuat invoice sendiri jika Billing menjadi owner;
- menulis langsung ke tabel Billing tanpa service/contract yang sesuai architecture existing.

Duplicate billing harus dicegah dengan business reference/idempotency.

---

# 14. RBAC Business Requirements

Claude harus menelusuri authorization implementation saat ini dan membangun exact role/action matrix.

Minimum capability class yang perlu dipetakan:

- Read Order
- Create Order, jika Pesan Baru menjadi bagian V2
- Process Order
- Read Inventory
- Allocate Unit
- Issue Unit
- Return/Restock Unit
- Cancel Order
- Record Procedure
- Print Label
- Record Sampling
- Read Report
- Manage Setup

Jangan invent role code.

Gunakan role/permission model existing.

---

# 15. Non-Functional Business Requirements

## Security

- deny-by-default;
- authentication seluruh protected endpoint;
- action-level permission;
- object-level authorization;
- strict request validation;
- server-side state validation;
- no mass assignment;
- safe output;
- safe logging;
- no credentials/hardcoded secrets;
- no detailed exception leakage.

## Auditability

State penting harus dapat direkonstruksi dari histori.

## Availability

Kegagalan dependency seperti Billing/Lab tidak boleh membuat inventory transition menjadi setengah selesai tanpa strategi transaction/compensation yang jelas.

## Performance

List order dan inventory wajib:

- server-side pagination;
- bounded page size;
- filterable;
- queryable dengan index yang sesuai;
- tidak melakukan unbounded data load.

## Accessibility

Frontend harus:

- keyboard usable;
- memiliki loading/error/empty state;
- semantic labels;
- visible disabled state;
- tidak hanya mengandalkan warna untuk status.

---

# 16. Success Criteria

Modul dianggap implementation-ready hanya apabila:

1. evidence matrix lengkap;
2. source V2 backend dan frontend sudah ditelusuri;
3. ownership entity dibekukan;
4. ERD conceptual dan physical mapping tersedia;
5. status-transition contract tersedia;
6. endpoint catalog tersedia;
7. error catalog tersedia;
8. dependency contract tersedia;
9. RBAC matrix tersedia;
10. frontend consumption contract tersedia;
11. seluruh P0 requirement mempunyai task;
12. seluruh P0 requirement mempunyai test;
13. evidence -> requirement -> contract -> task -> test dapat ditelusuri;
14. unresolved clinical decision tidak disamarkan sebagai requirement;
15. implementation builder belum dijalankan untuk item yang masih blocked.

---

# 17. Blueprint Decision Gate

Status awal:

`BLUEPRINT_DRAFT`

Bukan:

`IMPLEMENTATION_AUTHORITY_GRANTED`

Implementation authority hanya boleh naik setelah requirement completeness dan contract readiness berhasil diverifikasi.