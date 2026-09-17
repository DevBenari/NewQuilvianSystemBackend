# Requirement Traceability Matrix — Integrasi Rawat Inap ↔ Kasir / Billing

> ## Berkas Baru — Sub-modul `integrasi-billing` (Slice `INP-S22`)
>
> Berkas ini adalah register matriks keterlacakan (*requirement traceability matrix*) untuk sub-modul **Integrasi Rawat Inap ↔ Kasir / Billing**. Berkas ini membuktikan bahwa setiap kebutuhan bisnis (*PRD*), keputusan wawancara (*interview decisions*), arsitektur target, dan kriteria penerimaan (*acceptance criteria*) memiliki pemetaan 1-ke-1 yang solid ke task backend (`BE-RWI-127` s.d. `134`), task frontend (`FE-RWI-095` s.d. `100`), dan skenario pengujian end-to-end (`UAT-INT-001` s.d. `013`).

---

## Metadata

```yaml
module_id: rawat-inap
submodule: integrasi-billing
slice_id: INP-S22
blueprint_id: RWI-BP-001-INT-BIL
blueprint_revision: 1.0.0
traceability_file: docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/requirement-traceability.md
traceability_revision: 1
status: APPROVED
approval_gate: APPROVED
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-17"
approval_decision: RWI-DEC-162
upstream_input: "docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md"
input_revision_hash: sha256:6e03ec84a7e93c153e7f45bf948529283f518e1d2ac8d228f44101e4a3c10b7a
backend_source_sha: fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344
frontend_source_sha: 2c00758832f834cff0288bef4f0d2fcf1161fb52
backend_roadmap: docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md
frontend_roadmap: docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/frontend-roadmap.md
contract_version: 1.0.0
decision_source: "docs/module-blueprints/rawat-inap/00-interview-decisions.md revision 26 (RWI-DEC-156 s.d. 162)"
capability_map_source: "docs/module-blueprints/rawat-inap/evidence/01-existing-capability-map.md Bagian 18"
completeness_gate_source: "docs/module-blueprints/rawat-inap/evidence/02-requirement-completeness-gate.md Bagian 16"
fr_range: FR-INT-001..FR-INT-018
ac_range: RWI-AC-236..RWI-AC-241
uat_range: UAT-INT-001..UAT-INT-013
backend_tasks: BE-RWI-127..BE-RWI-134
frontend_tasks: FE-RWI-095..FE-RWI-100
```

---

## 1. Matriks Keterlacakan Utama (Requirement → Desain → Kontrak → Task → Bukti)

Tabel berikut menghubungkan setiap Kebutuhan Fungsional (*Functional Requirement*), Epic, Keputusan Bisnis Terkait, Rujukan Desain Arsitektur, Kontrak Terkunci Versi `1.0.0`, Task Eksekusi Backend/Frontend, dan Kriteria Bukti Penerimaan (*Acceptance Proof*).

| FR ID | Epic & Kemampuan | Keputusan Terkait | Rujukan Desain Arsitektur | Kontrak `1.0.0` | Task Backend | Task Frontend | Bukti Acceptance / Kasus Uji | Status Implementasi |
|---|---|---|---|---|---|---|---|:---:|
| **`FR-INT-001`** | `EPIC-INT-01`<br>(`INT-CAP-01`) | `RWI-DEC-156` | `02-backend` §2.1 & §3 | `contracts/integration` §1.2 | `BE-RWI-130` | — | `RWI-AC-236`<br>`UAT-INT-001` | Siap Dikerjakan (Approved) |
| **`FR-INT-002`** | `EPIC-INT-01`<br>(`INT-CAP-06`) | `RWI-DEC-161` | `02-backend` §3 & §7 | `contracts/integration` §1.1 | `BE-RWI-128` | — | `RWI-AC-241`<br>`TC-OUTBOX-01` | Siap Dikerjakan (Approved) |
| **`FR-INT-003`** | `EPIC-INT-01`<br>(`INT-CAP-01`) | `RWI-DEC-156` | `02-backend` §2.1 | `contracts/state` §1 | `BE-RWI-130` | — | `RWI-AC-236`<br>`UAT-INT-001` | Siap Dikerjakan (Approved) |
| **`FR-INT-004`** | `EPIC-INT-02`<br>(`INT-CAP-02`) | `RWI-DEC-156`, `159` | `02-backend` §2.2 | `contracts/integration` §1.2 | `BE-RWI-130` | — | `RWI-AC-236`<br>`UAT-INT-002` | Siap Dikerjakan (Approved) |
| **`FR-INT-005`** | `EPIC-INT-02`<br>(`INT-CAP-02`) | `RWI-DEC-159` | `02-backend` §2.2 & §5 | `contracts/state` §2 | `BE-RWI-134` | `FE-RWI-100` | `RWI-AC-239`<br>`UAT-INT-013` | Siap Dikerjakan (Approved) |
| **`FR-INT-006`** | `EPIC-INT-02`<br>(`INT-CAP-02`) | `RWI-DEC-159` | `02-backend` §3 & §5 | `contracts/integration` §1.2 | `BE-RWI-134` | `FE-RWI-100` | `RWI-AC-239`<br>`UAT-INT-013` | Siap Dikerjakan (Approved) |
| **`FR-INT-007`** | `EPIC-INT-03`<br>(`INT-CAP-03`) | `RWI-DEC-157` | `02-backend` §2.3 | `contracts/validation` `VAL-INT-002` | `BE-RWI-131` | — | `RWI-AC-237`<br>`UAT-INT-004` | Siap Dikerjakan (Approved) |
| **`FR-INT-008`** | `EPIC-INT-03`<br>(`INT-CAP-03`) | `RWI-DEC-157` | `02-backend` §2.3 | `contracts/validation` `VAL-INT-003` | `BE-RWI-131` | — | `RWI-AC-237`<br>`UAT-INT-005` | Siap Dikerjakan (Approved) |
| **`FR-INT-009`** | `EPIC-INT-03`<br>(`INT-CAP-03`) | `RWI-DEC-157` | `02-backend` §2.3 & §3 | `data/data-dict` §2 | `BE-RWI-131` | — | `RWI-AC-237`<br>`UAT-INT-005` | Siap Dikerjakan (Approved) |
| **`FR-INT-010`** | `EPIC-INT-03`<br>(`INT-CAP-03`) | `RWI-DEC-157` | `02-backend` §3 | `contracts/integration` §1.2 | `BE-RWI-131` | — | `RWI-AC-237`<br>`UAT-INT-005` | Siap Dikerjakan (Approved) |
| **`FR-INT-011`** | `EPIC-INT-04`<br>(`INT-CAP-04`) | `RWI-DEC-160` | `02-backend` §4, `03-fe` §4 | `contracts/api` §1 | `BE-RWI-132` | `FE-RWI-095`, `096` | `RWI-AC-240`<br>`TC-PRIVACY-01` | Siap Dikerjakan (Approved) |
| **`FR-INT-012`** | `EPIC-INT-04`<br>(`INT-CAP-04`) | `RWI-DEC-160` | `03-frontend` §4, `05-skema` §1, 2 | `contracts/api` §1 | `BE-RWI-132` | `FE-RWI-095`, `096` | `RWI-AC-240`<br>`UAT-INT-008` | Siap Dikerjakan (Approved) |
| **`FR-INT-013`** | `EPIC-INT-04`<br>(`INT-CAP-04`) | `RWI-DEC-160` | `02-backend` §4, `05-skema` §5 | `contracts/permission` §1, 2 | `BE-RWI-132` | `FE-RWI-097` | `RWI-AC-240`<br>`TC-PRIVACY-02` | Siap Dikerjakan (Approved) |
| **`FR-INT-014`** | `EPIC-INT-05`<br>(`INT-CAP-05`) | `RWI-DEC-158` | `02-backend` §5, `03-fe` §4.2 | `contracts/validation` `VAL-INT-001` | `BE-RWI-134` | `FE-RWI-098` | `RWI-AC-238`<br>`UAT-INT-012` | Siap Dikerjakan (Approved) |
| **`FR-INT-015`** | `EPIC-INT-05`<br>(`INT-CAP-05`) | `RWI-DEC-158` | `02-backend` §5, `03-fe` §4.2 | `contracts/api` §2 (Webhook) | `BE-RWI-133` | `FE-RWI-098` | `RWI-AC-238`<br>`TC-GATE-02` | Siap Dikerjakan (Approved) |
| **`FR-INT-016`** | `EPIC-INT-05`<br>(`INT-CAP-05`) | `RWI-DEC-158` | `02-backend` §5, `05-skema` §4 | `contracts/validation` `VAL-INT-004`, `005` | `BE-RWI-134` | `FE-RWI-099` | `RWI-AC-238`<br>`TC-GATE-03`, `04` | Siap Dikerjakan (Approved) |
| **`FR-INT-017`** | `EPIC-INT-06`<br>(`INT-CAP-06`) | `RWI-DEC-161` | `02-backend` §3 & §7 | `data/data-dict` §3 | `BE-RWI-127`, `128` | — | `RWI-AC-241`<br>`TC-OUTBOX-01`, `02` | Siap Dikerjakan (Approved) |
| **`FR-INT-018`** | `EPIC-INT-06`<br>(`INT-CAP-06`) | `RWI-DEC-161` | `02-backend` §7.2 | `contracts/integration` §1.1 | `BE-RWI-129` | — | `RWI-AC-241`<br>`TC-OUTBOX-03`, `UAT-INT-006` | Siap Dikerjakan (Approved) |

> **Analisis Kelengkapan FR:** Tepat 18 Functional Requirements terpetakan penuh ke 8 task backend dan 6 task frontend. **Nol FR tanpa task penanggung (Zero Gap)**.

---

## 2. Kebutuhan Non-Fungsional (NFR) & Ketahanan Sistem

| NFR ID | Aspek Kebutuhan | Bunyi Persyaratan | Task Backend | Task Frontend | Metode Verifikasi Pembuktian |
|---|---|---|---|---|---|
| **`NFR-INT-01`** | **Integritas Transaksional (Atomicity)** | Penyimpanan status rawat inap lokal dan antrean event outbox wajib berada dalam transaksi DbContext yang sama (Commit bersama atau Rollback bersama). | `BE-RWI-128` | — | Unit test `InpatientIntegrationOutboxResilienceTests` (`TC-OUTBOX-01`) dengan memicu exception sengaja dan memastikan rollback menyeluruh. |
| **`NFR-INT-02`** | **Idempotensi & Anti-Duplikasi** | Pengiriman ulang event outbox akibat kegagalan jaringan tidak boleh menyebabkan duplikasi tagihan kamar di kasir. | `BE-RWI-127`, `BE-RWI-129` | — | Database unique constraint `UQ_InpIntegrationOutbox_IdempotencyKey` (`TC-OUTBOX-02`) dan uji idempotensi dispatcher (`UAT-INT-006`). |
| **`NFR-INT-03`** | **Privasi Finansial di Bangsal** | DTO operasional bangsal dilarang memuat angka rupiah secara mutlak untuk pengguna tanpa hak akses `InpatientBilling:View`. | `BE-RWI-132` | `FE-RWI-095`, `FE-RWI-096` | Contract test DTO serialization (`TC-PRIVACY-01`) dan verifikasi DOM inspector di browser tanpa teks nominal rupiah. |
| **`NFR-INT-04`** | **Ketahanan Layanan (Resilience)** | Downtime atau kelambatan respon modul kasir tidak boleh menghentikan proses admisi, mutasi kamar, atau pencatatan medis di bangsal rawat inap. | `BE-RWI-128`, `BE-RWI-129` | — | Simulasi HTTP mock 503 Service Unavailable pada endpoint Billing; sistem rawat inap tetap merespons 200 OK ke staf medis, sementara outbox menjadwalkan retry berkala. |
| **`NFR-INT-05`** | **Kinerja & Latensi Query** | Kueri pembacaan status kasir bangsal harus selesai dalam waktu < 200ms pada beban 100 request simultan. | `BE-RWI-132` | `FE-RWI-096` | Indexing komposit pada foreign key `AdmissionId` dan integrasi smart client polling (30s background) tanpa flicker UI. |
| **`NFR-INT-06`** | **Audit Trail Hukum Kedaruratan** | Setiap aksi pelepasan darurat (*Supervisor Override*) wajib mencatat jejak audit permanen yang tidak dapat diubah (immutable). | `BE-RWI-134` | `FE-RWI-099` | Verifikasi entri tabel audit log memuat `ActorId`, `SupervisorPIN`, `TimestampUtc`, dan `EmergencyReason` (minimal 20 karakter). |

---

## 3. Matriks Skenario UAT End-to-End (`UAT-INT-001` s.d. `013`)

Merujuk pada **PRD Bersama Bagian 47**, berikut adalah pemetaan 13 skenario UAT lintas modul beserta pembagian tanggung jawab antara Rawat Inap dan Billing:

| Kode UAT | Judul Skenario Pengujian | Hasil yang Diharapkan (PRD §47) | Tanggung Jawab Modul Rawat Inap | Tanggung Jawab Modul Billing | Task Penanggung di Rawat Inap |
|---|---|---|---|---|---|
| **`UAT-INT-001`** | Pasien Baru Masuk Rawat Inap | Encounter aktif; Billing dapat mengenali episode dan membuat folio OPEN. | Menerbitkan event outbox `ADMISSION_CONFIRMED`. | Menerima event dan membentuk `BillingFolio` status `OPEN`. | `BE-RWI-130` |
| **`UAT-INT-002`** | Pasien Mendapatkan Tempat Tidur | Occupancy tersedia; Room charge terbentuk tepat satu kali. | Menetapkan bed fisik dan menerbitkan event outbox `BED_OCCUPIED`. | Menginisiasi kalkulasi tarif sewa kamar harian. | `BE-RWI-130` |
| **`UAT-INT-003`** | Pasien Masuk Malam Hari | Kebijakan tarif jam masuk diterapkan dengan benar (misal cut-off 18.00 / 22.00). | Mengirimkan stempel waktu `OccupancyStartAt` yang akurat ke billing. | Menerapkan formula tarif malam sesuai aturan billing RS. | `BE-RWI-130` |
| **`UAT-INT-004`** | Pasien Pindah Kamar di Hari yang Sama | Charge kamar terbagi proporsional sesuai kebijakan RS (misal 50:50). | Melakukan transfer bed dan menerbitkan event `OCCUPANCY_CORRECTED`. | Menghitung prorata tagihan kamar lama dan kamar baru. | `BE-RWI-131` |
| **`UAT-INT-005`** | Kelas Perawatan Salah Dikoreksi | Histori occupancy tetap; Billing mereverse charge lama dan menerbitkan charge baru. | Koreksi kamar dengan alasan; simpan record baru berversi tanpa hapus lama. | Melakukan koreksi posting jurnal/folio dan penyesuaian nominal. | `BE-RWI-131` |
| **`UAT-INT-006`** | Gangguan Jaringan & Network Retry | Outbox mengirim ulang pesan; tidak timbul duplikasi tagihan kamar. | Outbox worker melakukan exponential retry dengan IdempotencyKey tetap. | Memeriksa tabel idempotensi dan menolak eksekusi ganda jika key sudah pernah diproses. | `BE-RWI-129` |
| **`UAT-INT-007`** | Cetak Tagihan Sementara (Provisional) | Tagihan kasir tidak terkunci; charge pelayanan bangsal masih dapat bertambah. | Tidak membatasi pelayanan klinis bangsal selama tagihan sementara dicetak. | Memfasilitasi cetak billing estimasi tanpa mengubah status folio menjadi CLOSED. | — (Scope Kasir) |
| **`UAT-INT-008`** | Pasien Mempunyai Kurang Deposit | Rawat inap membaca kekurangan deposit/status blocker dari Billing. | Menampilkan lencana status operasional kasir dan kendala blocker di layar bangsal. | Menyediakan endpoint status operasional kasir untuk dikonsumsi bangsal. | `BE-RWI-132`<br>`FE-RWI-096` |
| **`UAT-INT-009`** | Pasien Menjamin Menggunakan Asuransi | Tagihan terbagi menjadi porsi asuransi (covered) dan selisih bayar (excess). | Mencatat penjamin pasien pada admisi rawat inap. | Memproses verifikasi penjaminan dan alokasi tanggungan kasir. | `BE-RWI-130` |
| **`UAT-INT-010`** | Pasien Rawat Inap Berasal dari IGD | Tagihan akhir terkonsolidasi; riwayat pelayanan IGD tetap terlacak. | Menghubungkan nomor admisi ranap dengan episode IGD rujukan. | Menggabungkan charge IGD dan charge Ranap dalam satu settlement folio. | `BE-RWI-130` |
| **`UAT-INT-011`** | Finalisasi Tagihan (Close Bill Kasir) | Posting charge normal berikutnya dari pelayanan ditolak oleh sistem. | Menerima status penolakan mutasi bila mencoba pindah kamar saat kasir CLOSED. | Mengunci posting transaksi baru pada folio pasien. | `BE-RWI-131` |
| **`UAT-INT-012`** | Masih Terdapat Tagihan Belum Lunas | Status Financial Clearance = BLOCKED / PENDING; tombol pulang bangsal terkunci. | Mengunci tombol `Konfirmasi Pasien Pulang Fisik` di modal discharge. | Mengirimkan status clearance belum lunas ke rawat inap. | `BE-RWI-134`<br>`FE-RWI-098` |
| **`UAT-INT-013`** | Pelunasan Selesai (Settlement Final) | Financial Clearance = CLEARED; tombol pulang aktif; kepulangan melepas bed. | Membuka tombol pulang; mencatat jam fisik pulang; menerbitkan `BED_RELEASED`. | Menerbitkan webhook `ClearanceApproved` dan mengunci billing permanen. | `BE-RWI-134`<br>`FE-RWI-100` |

---

## 4. Matriks Ketergantungan Lintas Sub-Modul & Modul Eksternal

### 4.1 Ketergantungan Keluar (*Outbound Dependencies*) ke Modul Kasir / Billing

| Task Rawat Inap | Komponen yang Dihubungi | Modul Target | Tujuan & Dampak Integrasi | Status Kesiapan Target |
|---|---|---|---|:---:|
| `BE-RWI-129` | HTTP Endpoint Receiver Event | `BillingManagement` | Background worker mengirimkan event payload JSON (`ADMISSION_CONFIRMED`, `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED`). | Menunggu Kontrak Bersama Approved |
| `BE-RWI-131` | API Query Folio Status | `BillingManagement` | Validasi sinkron status billing (`OPEN` vs `CLOSED`) sebelum mengizinkan mutasi kamar. | Menunggu Endpoint Kasir |
| `BE-RWI-132` | API Query Operational Billing | `BillingManagement` | Mengambil status operasional kasir (`statusText`, `blockers`) untuk disajikan di bangsal. | Menunggu Endpoint Kasir |

### 4.2 Ketergantungan Masuk (*Inbound Dependencies*) dari Modul Kasir / Billing

| Pemicu Eksternal | Komponen Penerima di Rawat Inap | Dampak pada Modul Rawat Inap | Penanggung Jawab di Rawat Inap |
|---|---|---|:---:|
| Webhook `ClearanceApproved` | `POST /api/inpatient/episodes/{id}/discharge-clearance/webhook` | Mengubah status clearance lokal menjadi `Cleared`, membuka tombol pemulangan fisik di modal perawat. | `BE-RWI-133`<br>`FE-RWI-098` |
| Webhook `ClearanceRevoked` | `POST /api/inpatient/episodes/{id}/discharge-clearance/webhook` | Memicu **Auto-Reblock** seketika: mengubah status menjadi `Revoked`, mengunci tombol pulang, memunculkan banner peringatan merah. | `BE-RWI-133`<br>`FE-RWI-098` |

---

## 5. Gerbang Kesiapan Implementasi (*Implementation Readiness Gates*)

Sebelum salah satu task `BE-RWI-127` s.d. `134` atau `FE-RWI-095` s.d. `100` dieksekusi oleh builder, gerbang-gerbang tata kelola berikut wajib dipenuhi:

| ID Gerbang | Deskripsi Gerbang Tata Kelola | Kriteria Pemenuhan | Pemilik Wewenang | Status Gerbang |
|:---:|---|---|:---:|:---:|
| **`GATE-DES-01`** | **Persetujuan Blueprint & Roadmap** | Dokumen blueprint `02-backend`, `03-frontend`, `04-prd-to-mvp`, dan seluruh matriks kontrak disetujui resmi oleh Product Owner. | Muhammad Hamzah | 🟡 Menunggu Review |
| **`GATE-DES-02`** | **Persetujuan Kontrak Integrasi Bersama** | Spesifikasi payload outbox dan webhook disetujui bersama oleh penanggung jawab kedua modul. | Muhammad Hamzah & Yasmina | 🟡 Menunggu Review |
| **`GATE-MIG-01`** | **Wewenang Eksekusi Migrasi Basis Data** | Otorisasi eksplisit sebelum membuat dan menjalankan migration EF Core untuk tabel outbox dan field occupancy. | Muhammad Hamzah / Tech Lead | ⛔ Terkunci per Kebijakan |
| **`GATE-IMP-01`** | **Wewenang Tulis Implementasi Task Tunggal** | Builder hanya diizinkan mengambil tepat 1 task approved per pemanggilan (`build-module-backend` / `build-module-frontend`). | Aturan Workspace Quilvian | ⛔ Terkunci per Task |

---

## 6. Riwayat Perubahan (*Revision History*)

| Versi | Tanggal | Penulis | Ringkasan Perubahan |
|:---:|:---:|:---:|---|
| **`1`** | 17 September 2026 | Antigravity AI (atas nama Muhammad Hamzah) | Pembuatan dokumen matriks keterlacakan awal (*initial requirement traceability matrix*) untuk sub-modul `integrasi-billing` (Slice `INP-S22`). Memetakan 18 FR, 6 AC, 6 NFR, 13 skenario UAT, 8 task backend (`BE-RWI-127` s.d. `134`), dan 6 task frontend (`FE-RWI-095` s.d. `100`). |
