# Modul Operasi — Arsitektur Domain

| Field | Nilai |
|---|---|
| Blueprint ID | `operations` |
| Revision arsitektur | `1` |
| Status | `DOMAIN_ARCHITECTURE_READY` |
| Requirement gate | `READY_FOR_DOMAIN_DESIGN` |
| Decision input | `00-interview-decisions.md`, revision 5 |
| Capability input | `01-existing-capability-map.md`, revision 2 |
| Assessment input | `02-requirement-completeness-assessment.md`, revision 3 |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| Bounded context | `HealthServices/OperatingRoomManagement` |
| Prefix canonical | `Opr` (`PLANNED` pada registry repository) |
| Baseline Operating Theatre | `REFERENCE_ONLY: NOT_YET_AVAILABLE` |

## Scope Arsitektur

Arsitektur mencakup satu kasus operasi pasien sejak dokter mengirim permintaan sampai pasien selesai menjalani recovery dan diterima unit tujuan. Menu hanya menjadi cara pengguna menemukan pekerjaan; menu tidak menentukan entity domain.

### Termasuk

1. Permintaan dan daftar kasus operasi.
2. Penjadwalan ruang serta tim tanpa benturan.
3. Persiapan dan checklist keselamatan.
4. Pelaksanaan tindakan, anestesi, material, dan implant.
5. Recovery serta serah terima.
6. Histori, audit, notifikasi, laporan, dan handoff ke domain lain.

### Tidak dimiliki Modul Operasi

- identitas pasien dan encounter;
- master dokter, pegawai, ruang, procedure, obat, item, dan tarif;
- consent klinis;
- saldo persediaan;
- invoice dan pembayaran;
- perawatan setelah unit tujuan menerima pasien.

## Ubiquitous Language

| Istilah | Makna bisnis |
|---|---|
| Kasus Operasi | Satu episode perioperatif pasien yang mempunyai lifecycle sendiri |
| Tindakan Utama | `TrxPatientProcedure` utama yang menjadi alasan kasus operasi dibuat |
| Tindakan Tambahan | Tindakan lain yang dilakukan dalam kasus yang sama |
| Jadwal Operasi | Alokasi rentang waktu, ruang, dan tim untuk satu kasus |
| Kesiapan | Kondisi setelah dokter bedah, dokter anestesi, dan perawat menyelesaikan sign-off wajib |
| Pelaksanaan | Masa sejak dokter bedah memulai operasi sampai pekerjaan klinis kamar operasi selesai |
| Recovery | Pemantauan setelah tindakan sampai dokter anestesi menentukan tujuan berikutnya |
| Serah Terima | Pemindahan tanggung jawab pasien kepada unit tujuan dan penerima yang jelas |
| Addendum | Koreksi baru terhadap catatan final tanpa menghapus isi sebelumnya |
| Pemakaian Aktual | Jumlah obat, bahan, alat, atau implant yang benar-benar digunakan kepada pasien |
| Waste | Barang yang tidak dapat dikembalikan karena sudah dibuka, rusak, atau tidak layak |

## Peta Bounded Context

| Context | Peran | Hubungan dengan Operating Room Management |
|---|---|---|
| `OperatingRoomManagement` | Pemilik kasus, jadwal operasi, kesiapan, pelaksanaan, recovery, handover, dan histori | Context utama |
| `RegistrationManagement` | Pemilik encounter pasien | Upstream; Operasi hanya menyimpan referensi |
| `PatientManagement` | Pemilik identitas pasien | Upstream; digunakan sebagai referensi |
| `ClinicalManagement` | Pemilik `TrxPatientProcedure` dan `TrxPatientConsent` | Upstream/peer; Operasi menghubungkan order dan consent tanpa menduplikasi |
| `MasterData` | Pemilik ruang, procedure, unit layanan, dan tarif | Upstream reference data |
| `HumanResource` | Pemilik dokter/pegawai dan bukti kewenangan tenaga | Upstream; Operasi memvalidasi anggota tim melalui kontrak |
| `Pharmacy/Inventory` | Pemilik item dan saldo stok | Downstream; menerima pemakaian, retur, dan waste |
| `BillingManagement` | Pemilik charge, invoice, reversal, dan pembayaran | Downstream; menerima informasi chargeable secara idempotent |
| `InPatient/ICU/Unit Tujuan` | Pemilik perawatan setelah handover diterima | Downstream; menerima tanggung jawab pasien |

## Aggregate Utama

### `OprCase`

`OprCase` adalah aggregate root dan sumber kebenaran lifecycle satu kasus operasi. Nama ini adalah konsep target; arsitektur ini belum menetapkan bentuk tabel fisik.

#### Identitas dan referensi wajib

- identitas kasus operasi;
- `PatientId` sebagai referensi pasien existing;
- `EncounterId` sebagai referensi encounter existing;
- satu atau lebih referensi `TrxPatientProcedure`;
- tepat satu tindakan ditandai sebagai tindakan utama;
- dokter pemohon dan dokter bedah utama;
- jenis elektif/darurat dan prioritas;
- status kasus saat ini;
- versi untuk mencegah dua petugas mengubah kasus yang sama pada waktu hampir bersamaan.

#### Batas konsistensi

`OprCase` menjaga invariant berikut:

1. Satu tindakan pasien tidak boleh terkait dengan dua kasus operasi aktif.
2. Kasus tidak dapat menjadi `Scheduled` tanpa ruang, rentang waktu, dokter bedah, dokter anestesi, perawat instrumen, dan perawat sirkuler yang sah.
3. Ruang atau anggota tim tidak boleh mempunyai jadwal aktif yang bertabrakan.
4. Kasus hanya menjadi `Ready` setelah sign-off tiga pihak dan checklist wajib terpenuhi, kecuali bypass darurat tercatat.
5. Kasus hanya menjadi `In Progress` dari `Ready` dan hanya oleh dokter bedah utama.
6. `Cancelled` hanya dapat dicapai sebelum `In Progress`.
7. Kasus hanya menjadi `Completed` setelah catatan operasi selesai, keputusan keluar recovery tersedia, dan handover diterima.
8. Catatan final hanya dikoreksi melalui addendum.
9. Histori status, jadwal, pemakaian material, dan integrasi tidak boleh dihapus.

**Contoh:** Kasus A menggunakan Ruang Operasi 1 pukul 09.00–11.00. Kasus B tidak dapat dijadwalkan di ruang tersebut pukul 10.00, walaupun dokter untuk Kasus B tersedia.

## Katalog Konsep Domain

| ID | Konsep | Klasifikasi | Ownership | Peran |
|---|---|---|---|---|
| `OPS-CON-001` | `OprCase` | `AGGREGATE_ROOT` | `New` — OperatingRoomManagement | Menjaga lifecycle dan invariant kasus |
| `OPS-CON-002` | Procedure Reference | `VALUE_OBJECT` | `Adapter/View` ke `TrxPatientProcedure` | Menghubungkan satu atau lebih tindakan; satu tindakan utama |
| `OPS-CON-003` | Consent Reference | `VALUE_OBJECT` | `Adapter/View` ke `TrxPatientConsent` | Menunjukkan consent operasi/anestesi yang berlaku tanpa menyalin isi consent |
| `OPS-CON-004` | `OprSchedule` | `ENTITY` | `New` | Menyimpan revisi jadwal ruang, waktu, alasan, dan status aktif |
| `OPS-CON-005` | `OprTeamMember` | `ENTITY` | `New` | Menyimpan penugasan peran dan referensi tenaga klinis |
| `OPS-CON-006` | Preparation Record | `VALUE_OBJECT` | `New` | Menyimpan hasil persiapan kasus tanpa harus menjadi tabel terpisah |
| `OPS-CON-007` | `OprSafetyChecklist` | `ENTITY` | `New` | Checklist berversi, item, sign-off, dan bypass darurat |
| `OPS-CON-008` | `OprExecutionRecord` | `ENTITY` | `New` | Catatan operasi yang disahkan dokter bedah utama |
| `OPS-CON-009` | Execution Addendum | `ENTITY` | `New` | Koreksi append-only terhadap catatan operasi final |
| `OPS-CON-010` | `OprAnesthesiaRecord` | `ENTITY` | `New` | Catatan anestesi dengan kewenangan dokter anestesi |
| `OPS-CON-011` | `OprMaterialUsage` | `ENTITY` | `New` | Pemakaian aktual, retur, waste, batch/serial, dan status handoff stok |
| `OPS-CON-012` | `OprRecovery` | `ENTITY` | `New` | Pemantauan recovery dan keputusan dokter anestesi |
| `OPS-CON-013` | `OprHandover` | `ENTITY` | `New` | Bukti pemberi, penerima, unit tujuan, kondisi, instruksi, dan waktu penerimaan |
| `OPS-CON-014` | `OprStatusHistory` | `ENTITY` | `New` | Histori append-only setiap transition |
| `OPS-CON-015` | Integration Delivery | `VALUE_OBJECT` | `New` | Correlation ID, idempotency key, status pengiriman, kegagalan, dan retry |
| `OPS-CON-016` | Patient/Encounter | `REFERENCE_DATA` | `Existing` | Referensi ke pemilik pasien/episode |
| `OPS-CON-017` | Doctor/Employee | `REFERENCE_DATA` | `Existing` | Referensi tenaga; bukan salinan master SDM |
| `OPS-CON-018` | Room/Procedure/Tariff/Item | `REFERENCE_DATA` | `Existing` atau kontrak upstream | Referensi master sesuai pemilik otoritatif |

Klasifikasi konsep tidak otomatis berarti satu konsep sama dengan satu tabel. Keputusan fisik dibuat pada blueprint backend setelah arsitektur disetujui.

## Model Relasi Logis

| Sumber | Tujuan | Kardinalitas | Makna |
|---|---|---|---|
| `OprCase` | Patient | banyak kasus : satu pasien | Pasien dapat menjalani beberapa kasus pada waktu berbeda |
| `OprCase` | Encounter | banyak kasus : satu encounter | Satu encounter dapat mempunyai lebih dari satu kasus bila memang diperlukan |
| `OprCase` | `TrxPatientProcedure` | satu : satu atau lebih | Satu kasus mempunyai tindakan utama dan dapat mempunyai tindakan tambahan |
| `TrxPatientProcedure` | kasus aktif | satu : paling banyak satu | Mencegah order yang sama diproses dua kali |
| `OprCase` | `TrxPatientConsent` | satu : nol atau lebih | Dapat mereferensikan consent operasi dan anestesi beserta versi/status yang berlaku |
| `OprCase` | `OprSchedule` | satu : satu atau lebih setelah dijadwalkan | Reschedule membuat revisi baru; jadwal lama tetap tersimpan |
| `OprCase` | `OprTeamMember` | satu : banyak | Tim minimum dan anggota tambahan |
| `OprCase` | checklist/execution/anesthesia/recovery | satu : masing-masing nol atau satu record aktif | Dibuat sesuai tahap lifecycle |
| `OprExecutionRecord` | addendum | satu : nol atau banyak | Catatan final tidak berubah; koreksi ditambahkan |
| `OprCase` | material usage/status history/handover | satu : banyak | Semua kejadian penting tetap dapat ditelusuri |

## Lifecycle dan Transition

| Dari | Tindakan | Ke | Wewenang | Prasyarat utama |
|---|---|---|---|---|
| - | Kirim permintaan | `Requested` | Dokter penanggung jawab/dokter bedah | Data minimum dan minimal satu tindakan tersedia |
| `Requested` | Tetapkan jadwal | `Scheduled` | Koordinator kamar operasi | Ruang, waktu, dan tim lengkap tanpa benturan |
| `Requested`/`Scheduled` | Tunda | `Postponed` | Koordinator dengan konfirmasi dokter terkait | Alasan dan jadwal sebelumnya dicatat |
| `Postponed` | Jadwalkan kembali | `Scheduled` | Koordinator | Jadwal baru lolos pemeriksaan benturan |
| `Scheduled` | Lengkapi kesiapan | `Ready` | Sistem setelah tiga sign-off | Consent/checklist valid atau bypass darurat sah |
| `Ready` | Mulai operasi | `In Progress` | Dokter bedah utama | Identitas pasien dan tindakan dikonfirmasi kembali |
| `In Progress` | Selesaikan kasus | `Completed` | Sistem | Catatan operasi selesai, recovery disetujui, handover diterima |
| `Requested`/`Scheduled`/`Ready` | Batalkan | `Cancelled` | Dokter bedah/dokter anestesi | Alasan klinis dicatat |

`Completed` dan `Cancelled` adalah status terminal. Operasi yang dihentikan setelah mulai tetap berakhir `Completed` dengan outcome `StoppedEarly`, bukan `Cancelled`.

## Tanggung Jawab Authorization

| Tindakan | Wewenang bisnis |
|---|---|
| Membuat permintaan | Dokter penanggung jawab atau dokter bedah |
| Menetapkan/mengubah jadwal dan tim | Koordinator kamar operasi |
| Sign-off bedah | Dokter bedah utama |
| Sign-off anestesi dan keputusan recovery | Dokter anestesi |
| Sign-off persiapan/checklist keperawatan | Perawat kamar operasi sesuai kewenangan |
| Memulai operasi dan mengesahkan catatan operasi | Dokter bedah utama |
| Mengisi catatan anestesi | Dokter anestesi atau petugas yang diberi kewenangan, dengan pengesahan dokter anestesi |
| Membatalkan sebelum mulai | Dokter bedah atau dokter anestesi |
| Membuat addendum catatan operasi | Dokter bedah yang berwenang; catatan asli tetap utuh |
| Menerima handover | Petugas berwenang dari unit tujuan |

Backend harus memeriksa kewenangan pada setiap tindakan. Menyembunyikan tombol di frontend tidak menggantikan pemeriksaan backend.

## Audit dan Histori

Setiap kejadian material menyimpan identitas kasus, pelaku, waktu, nilai/status sebelum dan sesudah, alasan, sumber tindakan, serta correlation ID bila berasal dari integrasi.

Wajib append-only:

- transition status;
- revisi jadwal dan tim;
- sign-off dan bypass darurat;
- pengesahan catatan serta addendum;
- keputusan recovery dan handover;
- pemakaian, retur, waste, batch, dan serial implant;
- permintaan charge/reversal;
- pengiriman, kegagalan, retry, dan rekonsiliasi integrasi.

## Model Integrasi

| Kontrak | Produsen → Konsumen | Informasi minimum | Aturan kegagalan |
|---|---|---|---|
| Validasi order/consent | OperatingRoomManagement ↔ ClinicalManagement | Case ID, procedure ID, consent ID/type/status | Kasus tidak `Ready` bila prasyarat tidak valid, kecuali bypass darurat tercatat |
| Validasi tenaga | OperatingRoomManagement ↔ HR/Credentialing | tenaga, peran, waktu, status aktif/kewenangan | Penjadwalan ditolak bila tenaga tidak sah; availability runtime belum tersedia dicatat sebagai dependency |
| Pemakaian persediaan | OperatingRoomManagement → Pharmacy/Inventory | Case ID, item eksternal, quantity, unit, batch/serial, outcome penggunaan, waktu | Gunakan idempotency key; simpan `Pending/Accepted/Failed`; retry tidak boleh menggandakan mutasi |
| Charge capture | OperatingRoomManagement → Billing | Case ID, procedure/tariff reference, komponen anestesi, material aktual, tipe create/correct/reverse | Billing tetap authoritative; retry idempotent dan status harus dapat direkonsiliasi |
| Handover pasien | OperatingRoomManagement → unit tujuan | Encounter, kondisi, alat/terapi, risiko, instruksi, pemberi/penerima, waktu | `Completed` menunggu penerimaan; kegagalan tidak boleh menghilangkan tanggung jawab klinis |
| Notifikasi | OperatingRoomManagement → Notification | event, penerima terdampak, ringkasan aman | Kegagalan notifikasi tidak membatalkan catatan klinis; tetap diaudit dan dapat dicoba ulang |

Identitas master item/implant mengikuti pemilik inventory yang tersedia kemudian. Modul Operasi tidak membuat master item tandingan.

## Domain Event

- `OperationCaseRequested`
- `OperationCaseScheduled`
- `OperationCasePostponed`
- `OperationCaseReady`
- `OperationCaseStarted`
- `OperationExecutionFinished`
- `OperationRecoveryReleased`
- `OperationHandoverAccepted`
- `OperationCaseCompleted`
- `OperationCaseCancelled`
- `OperationMaterialUsageRecorded`
- `OperationChargeCaptureRequested`
- `OperationIntegrationFailed`

Nama event adalah bahasa domain target, bukan kontrak message broker atau nama class implementasi yang sudah disetujui.

## Dampak Billing

Status: **berdampak pada charge**.

1. Tindakan operasi dan anestesi menjadi chargeable setelah layanan aktual selesai.
2. Material dan implant mengikuti pemakaian aktual, bukan jumlah yang hanya disiapkan.
3. Pembatalan sebelum mulai mengirim informasi pembatalan; Billing menentukan apakah ada charge existing yang perlu direversal.
4. Koreksi tidak menghapus histori charge; Operasi meminta correction/reversal kepada Billing.
5. Billing menjadi sumber kebenaran invoice, status bayar, payer, dan reversal.

## Dampak Keselamatan Klinis

Status: **relevan terhadap keselamatan**.

Kontrol wajib:

- identitas pasien, tindakan, dan lokasi/sisi dikonfirmasi;
- consent yang tepat diverifikasi;
- jadwal ruang/tim tidak bentrok;
- kewenangan klinis anggota tim divalidasi;
- checklist dan sign-off tidak dapat dilewati tanpa jalur darurat;
- catatan anestesi berbeda dari consent anestesi;
- implant dapat ditelusuri melalui batch/serial;
- catatan final hanya diamandemen;
- pasien tidak keluar recovery tanpa keputusan dokter anestesi;
- tanggung jawab tidak berpindah sebelum handover diterima.

## Alur Bisnis Utama

1. Dokter membuat kasus dari encounter dan satu atau lebih tindakan pasien.
2. Koordinator memilih ruang, rentang waktu, dan tim.
3. Sistem menolak benturan jadwal atau tenaga yang tidak sah.
4. Tim menyelesaikan persiapan, consent, checklist, dan sign-off.
5. Sistem menetapkan kasus `Ready`.
6. Dokter bedah memulai operasi sehingga kasus `In Progress`.
7. Tim mencatat tindakan, anestesi, komplikasi, material, dan implant aktual.
8. Dokter bedah mengesahkan catatan operasi; koreksi berikutnya memakai addendum.
9. Dokter anestesi memutuskan pasien keluar recovery atau dipindahkan ke unit yang sesuai.
10. Unit tujuan menerima handover.
11. Sistem menetapkan kasus `Completed` dan meneruskan handoff persediaan, billing, laporan, serta notifikasi yang diperlukan.

## Gap dan Dependency Implementasi

Tidak ada Decision ID bisnis yang memblokir arsitektur domain. Dependency teknis berikut tetap harus diselesaikan pada blueprint final:

1. Owner transaksi Billing belum tersedia lengkap pada source audit; kontrak logical dapat dirancang, implementasi menunggu capability terkait.
2. Master item/implant umum belum terbukti tersedia; kontrak memakai referensi eksternal tanpa membuat master baru.
3. Enforcement credential/clinical privilege existing belum terbukti; adapter harus dirancang dan dapat memblokir penjadwalan jika dependency tersedia.
4. Bounded context canonical adalah `OperatingRoomManagement`, bukan `OperatingTheatreManagement`.
5. Prefix `Opr` sudah tercatat `PLANNED`; penggunaan persisted entity tetap harus mengikuti governance backend dan task implementation yang disetujui.

## Kesiapan Arsitektur

Status: `DOMAIN_ARCHITECTURE_READY`.

Arsitektur siap diserahkan ke `design-business-module` untuk menyusun blueprint backend/frontend, kontrak API/integrasi, ERD konseptual, validasi, permission, audit, dan test strategy. Status ini tidak mengizinkan implementasi source, migration, atau database.
