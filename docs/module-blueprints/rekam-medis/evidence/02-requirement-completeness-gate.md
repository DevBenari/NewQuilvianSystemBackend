# Rekam Medis — Requirement Completeness Gate

| Field | Nilai |
| --- | --- |
| Blueprint ID | `QV-RM-001` |
| Revision penilaian | `4` |
| Tanggal penilaian | 21 Agustus 2026 |
| Hasil keseluruhan | `READY_FOR_DOMAIN_DESIGN` |
| Decision baseline | `00-interview-decisions.md`, revision `1`, SHA-256 `CE28BB1126799FFC54C4515ED313AC470A22DD9D0F1126086864599A08B0F210` |
| Capability baseline | `01-existing-capability-map.md`, SHA-256 `E16740282974D0820742E62C862B1A3F7CEA6BCE3449268667E17586925694C6` |
| Source snapshot | Backend `5103e68eec5529540d369673c8a4e2651be0344b`; frontend `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Batas penilaian | Inti Rekam Medis rawat jalan, IGD, dan rawat inap, termasuk signature, koreksi, kelengkapan, akses, privasi, release, integrasi, reliability, pelaporan, dan retensi. |

`READY_FOR_DOMAIN_DESIGN` berarti seluruh slice requirement boleh diterjemahkan menjadi arsitektur
domain dan desain blueprint berstatus `draft`. Gate ini bukan approval manusia dan bukan izin
mengaktifkan policy, fitur berisiko, deployment produksi, atau source implementation.

## Kesimpulan Gate

Closure pass berhasil menutup seluruh pertanyaan bisnis yang sebelumnya terbuka tentang signature,
wrong-patient, `Entered in Error`, pembatalan dan duplikasi episode, checklist, SLA, hubungan
pelayanan, break-glass, data sensitif, release, downtime, partial failure, hasil penunjang, billing,
pelaporan, serta retensi. Decision log sekarang juga menetapkan melalui `RM-APR-006` bahwa approval
formal boleh ditunda selama fase analisis dan desain draft.

`RM-APR-002` tetap terbuka dan `RM-APR-005` mengonfirmasi approver belum ditunjuk. Gap tersebut tidak
lagi memblokir domain design atau blueprint draft. Ia tetap memblokir status `approved`, aktivasi
policy/fitur berisiko, deployment produksi, dan sign-off kesiapan. Fitur yang mensyaratkan approval
harus tetap fail-closed.

Konflik source existing tetap ada. CPPT dan dokumen klinis masih dapat diubah/dihapus melalui jalur
yang tidak memenuhi invariant append-only; permission existing belum memeriksa hubungan pelayanan;
dan finalisasi frontend memakai beberapa request terpisah. Konflik ini bukan lagi kekurangan
keputusan bisnis. Ia menjadi constraint repair yang wajib dibawa ke arsitektur setelah approval.

## Bukti yang Dipakai

| ID bukti | Klasifikasi | Isi yang didukung | Referensi |
| --- | --- | --- | --- |
| `EVD-RM-REQ-002` | Requirement eksplisit pengguna | Scope, lifecycle, kewenangan, privasi, integrasi, exception, SLA, laporan, retensi, dan development gate | `00-interview-decisions.md`, revision `1`, hash `CE28...F210` |
| `EVD-RM-OPEN-002` | Bukti gap | `RM-APR-002`/`RM-APR-005` menjadi production-approval gap; `RM-APR-006` mengizinkan desain draft | Decision log |
| `EVD-RM-CAP-001` | Bukti implementasi V2 | 30 capability existing dengan status reuse, extension, repair, missing, conflict, atau unknown | `01-existing-capability-map.md`, hash `E167...94C6` |
| `EVD-RM-BE-001` | Source backend | Patient, encounter, workforce, fakta klinis, mutation path, permission, dan workflow existing | Branch `yoga`, commit `5103e68eec5529540d369673c8a4e2651be0344b` |
| `EVD-RM-FE-001` | Source frontend | Workspace dokter, autosave, finalisasi multi-request, serta gap consumer RM | Branch `YogaV2`, commit `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |

Baseline umum rumah sakit Indonesia tidak dipakai untuk menetapkan policy. Semua nilai kebijakan
dalam gate ini berasal dari jawaban pengguna. SOP yang disahkan dan memo approval formal belum
tersedia sebagai bukti.

### Aturan klasifikasi bukti

Isi keputusan pengguna diklasifikasikan `CONFIRMED` sebagai requirement eksplisit yang benar-benar
diberikan dalam wawancara. Status ini tidak sama dengan approval formal. `RM-APR-002` tetap
`MISSING`, tetapi menurut `RM-APR-006` dampaknya `NON_BLOCKING_STANDARD` untuk domain design dan
`BLOCKING` untuk aktivasi/produksi. Seluruh keputusan tetap berstatus `draft`.

## Proses Bisnis yang Sudah Terdefinisi

### Tujuan dan pelaku

Unit Rekam Medis memiliki proses operasional. Komite Medis atau Direktur Pelayanan Medis memiliki
approval klinis. Pejabat privasi/hukum memiliki approval privacy dan release. Tenaga kesehatan
membuat serta menandatangani catatan sesuai profesi dan penugasan formalnya.

### Alur utama

1. Modul pelayanan memulai episode dan sistem membekukan versi checklist yang berlaku.
2. Tenaga kesehatan membuat catatan sesuai penugasan dan kewenangan profesinya.
3. Setiap signature memerlukan autentikasi ulang serta menyimpan identitas, profesi, waktu, makna,
   dan sidik isi catatan.
4. Catatan signed tidak boleh ditimpa. Perubahan memakai koreksi atau addendum yang mempertahankan
   versi lama.
5. Sistem mengevaluasi dokumen wajib tetap dan conditional berdasarkan peristiwa nyata.
6. Akhir layanan dapat menghasilkan status `Belum Lengkap`.
7. Episode menjadi `Ditutup Final` hanya setelah seluruh item wajib lengkap dan ditandatangani.
8. Sistem menjalankan pengingat dan eskalasi tanpa menutup episode otomatis.

**Contoh:** Pasien rawat inap pulang pukul 14.00 ketika ringkasan pulang belum signed. Layanan boleh
berakhir, tetapi episode tetap `Belum Lengkap`. Setelah ringkasan ditandatangani dan semua item lain
terpenuhi, episode dapat menjadi `Ditutup Final`.

### Jalur exception

- Catatan salah pasien ditandai `Entered in Error`, tetap tersimpan, dikeluarkan dari ringkasan, dan
  dihubungkan dengan catatan baru pada pasien yang benar.
- Episode dapat `Dibatalkan` tanpa review hanya jika belum ada catatan signed. Pelayanan nyata
  mencegah pembatalan.
- Episode duplikat tetap tersimpan dan ditautkan ke episode kanonik tanpa memindahkan catatan.
- Downtime memakai formulir resmi bernomor dan direkonsiliasi sebagai `Entri Downtime`.
- Duplicate submit memakai kunci idempotency; isi berbeda dengan kunci sama ditolak.
- Data terlambat tidak membuka episode otomatis. Reopening memerlukan Unit Rekam Medis dan pejabat
  klinis dengan alasan serta audit.
- Kegagalan integrasi memberi status `Sinkronisasi Tertunda`; catatan signed tetap sah.

**Contoh:** Catatan operasi yang salah dikaitkan ke pasien A tidak dipindahkan atau dihapus. Catatan
itu ditandai `Entered in Error`, lalu tenaga berwenang membuat catatan baru untuk pasien B. Pengesah
yang berbeda memverifikasi keterkaitan kedua catatan.

## Penilaian 18 Dimensi

| ID | Dimensi | Status bukti | Temuan | Dampak gap |
| --- | --- | --- | --- | --- |
| `DIM-01` | Tujuan | `CONFIRMED` | Hasil klinis dan scope tiga layanan jelas. | - |
| `DIM-02` | Aktor | `CONFIRMED` | Kelompok pelaku, owner, reviewer, dan authority jelas; individu approver belum ditunjuk. | Tidak memblokir domain design; memblokir produksi. |
| `DIM-03` | Pemicu/prasyarat | `CONFIRMED` | Episode, penugasan formal, peristiwa checklist, signature, release, dan break-glass memiliki pemicu. | - |
| `DIM-04` | Alur utama | `CONFIRMED` | Create, sign, completeness, closure, correction, access, dan release sudah berurutan. | - |
| `DIM-05` | Exception | `CONFIRMED` | Wrong-patient, duplicate, cancellation, downtime, retry, late data, dan partial failure diputuskan. | - |
| `DIM-06` | Data minimum | `CONFIRMED` | Evidence signature, alasan, assignment, klasifikasi sensitif, bukti pemohon, dan audit ditetapkan. | - |
| `DIM-07` | Aturan/validation | `CONFIRMED` | Immutability, checklist, SLA, akses, release, dan retensi memiliki aturan teruji. | Policy tetap fail-closed sebelum approval. |
| `DIM-08` | Status/transisi | `CONFIRMED` | Lifecycle episode, record, result version, release, dan sinkronisasi terdefinisi. | - |
| `DIM-09` | Authorization | `CONFIRMED` | Role, penugasan, maker-checker, emergency access, reviewer, dan release authority jelas. | Aktivasi menunggu approval formal. |
| `DIM-10` | Dependency | `CONFIRMED` | Patient, encounter, workforce, lab/radiologi, pharmacy, billing, security, dan notification dibatasi ownership-nya. | - |
| `DIM-11` | Integrasi | `CONFIRMED` | Owner hasil penunjang, version evidence, idempotency, durable retry, dan reconciliation diputuskan. | Kontrak teknis adalah pekerjaan arsitektur hilir. |
| `DIM-12` | Hasil akhir | `CONFIRMED` | Catatan immutable, status kelengkapan, audit akses/release, dan closure dapat diamati. | - |
| `DIM-13` | Pembatalan/koreksi | `CONFIRMED` | Correction, addendum, entered-in-error, cancellation, duplicate, dan reopening terdefinisi. | - |
| `DIM-14` | Audit/histori | `CONFIRMED` | Identitas, waktu, alasan, hash, versi, review, release, dan retensi ditetapkan. | - |
| `DIM-15` | Notifikasi | `CONFIRMED` | Reminder 75%, eskalasi 24/72 jam, review break-glass, on-call, dan hasil terkoreksi diputuskan. | Channel teknis boleh ditentukan kemudian tanpa mengubah makna. |
| `DIM-16` | Billing | `CONFIRMED` | Billing menentukan readiness sendiri dan tidak menahan signature atau closure RM. | - |
| `DIM-17` | Keselamatan klinis | `CONFIRMED` | Data inti keselamatan, wrong-patient, sensitive access, hasil terkoreksi, dan late data memiliki guardrail. | - |
| `DIM-18` | Pelaporan | `CONFIRMED` | Worklist real-time, distribusi harian/mingguan/bulanan, audit, ekspor, retensi, dan legal hold diputuskan. | - |

## Register Requirement dan Gap Material

| ID | Pernyataan | Status bukti | Dampak | Slice |
| --- | --- | --- | --- | --- |
| `REQ-RM-101` | Signature menyimpan reauth, identitas, profesi, waktu, makna, dan hash; record signed append-only. | `CONFIRMED` | - | Signature/correction |
| `REQ-RM-102` | Wrong-patient, entered-in-error, cancellation, duplicate, dan reopening mempertahankan histori. | `CONFIRMED` | - | Lifecycle |
| `REQ-RM-103` | Checklist tiga layanan, versi frozen, conditional event, SLA, dan eskalasi terdefinisi. | `CONFIRMED` | - | Completeness |
| `REQ-RM-104` | Akses normal berasal dari penugasan formal bertanggal dan berakhir saat dicabut/episode ditutup. | `CONFIRMED` | - | Authorization |
| `REQ-RM-105` | Break-glass 15 menit, fail-closed bila policy belum disahkan, sensitive masking, dan review SLA terdefinisi. | `CONFIRMED` | - | Privacy |
| `REQ-RM-106` | Release memiliki bukti pemohon, maker-checker, status delivery, retry boundary, revocation, dan audit. | `CONFIRMED` | - | Release |
| `REQ-RM-107` | Lab/radiologi tetap owner hasil; RM menyimpan reference/copy released beserta seluruh versi. | `CONFIRMED` | - | Integration |
| `REQ-RM-108` | Idempotency, downtime, durable event, retry, reconciliation, dan late-data review terdefinisi. | `CONFIRMED` | - | Reliability |
| `REQ-RM-109` | Worklist, laporan, distribusi, retensi, dan legal hold terdefinisi. | `CONFIRMED` | - | Reporting |
| `REQ-RM-110` | Individu approver belum ditunjuk dan memo yang benar-benar ditandatangani belum tersedia. | `MISSING` | `NON_BLOCKING_STANDARD` untuk desain; `BLOCKING` untuk aktivasi/produksi | Governance approval |
| `REQ-RM-111` | CPPT dan clinical document existing dapat diubah/dihapus setelah keadaan yang seharusnya final. | `CONFLICT` | `BLOCKING` untuk reuse langsung, bukan untuk keputusan bisnis | Alignment source |
| `REQ-RM-112` | Security existing tidak memeriksa patient/encounter assignment. | `CONFLICT` | `BLOCKING` untuk reuse langsung | Alignment source |
| `REQ-RM-113` | Finalisasi frontend multi-request bertentangan dengan kebutuhan atomic/idempotent recovery. | `CONFLICT` | `BLOCKING` untuk reuse langsung | Alignment source |

## Decision Log Pemblokir

| Decision ID | Pertanyaan/bukti yang dibutuhkan | Bukti saat ini | Dampak | Owner | Status |
| --- | --- | --- | --- | --- | --- |
| `RM-APR-002` | Siapa yang ditunjuk sebagai approver operasional, klinis, dan privacy/release; kapan memo ditandatangani dan mulai berlaku? | `RM-APR-005` mengonfirmasi approver belum ditunjuk; `RM-APR-006` menundanya sampai gerbang aktivasi/produksi. | Tidak memblokir desain draft; memblokir status approved, aktivasi, produksi, dan sign-off. | Manajemen rumah sakit serta tiga authority terkait | `DEFERRED_PRODUCTION_GATE` |

Tidak ada keputusan klinis, lifecycle, authorization, integrasi, billing, atau keselamatan lain yang
masih terbuka pada snapshot ini.

## Kesiapan per Slice

| Slice | Kesiapan | Boleh berjalan | Harus berhenti | Dependency |
| --- | --- | --- | --- | --- |
| Referensi patient, encounter, workforce, location, dan shared clinical fact | `READY_FOR_DOMAIN_DESIGN` | Pemetaan ownership serta batas anti-duplikasi. | Membuat master atau fakta klinis tandingan. | Tidak ada blocker bisnis. |
| Signature, correction, entered-in-error, dan episode lifecycle | `READY_FOR_DOMAIN_DESIGN` | Arsitektur dan desain draft lengkap. | Aktivasi/produksi sebelum approval. | `RM-APR-002` sebagai production gate |
| Checklist, SLA, reminder, escalation, dan reporting | `READY_FOR_DOMAIN_DESIGN` | Arsitektur dan desain draft lengkap. | Mengaktifkan policy sebelum approval. | `RM-APR-002` sebagai production gate |
| Normal access, break-glass, sensitive access, dan release | `READY_FOR_DOMAIN_DESIGN` | Arsitektur dan desain draft lengkap dengan fail-closed. | Mengaktifkan fitur berisiko sebelum approval. | `RM-APR-002` sebagai production gate |
| Reliability, downtime, order-result, dan billing touchpoint | `READY_FOR_DOMAIN_DESIGN` | Arsitektur dan kontrak draft. | Deployment produksi sebelum approval. | `RM-APR-002` sebagai production gate |
| Frontend Rekam Medis | `READY_FOR_DOMAIN_DESIGN` | Desain fungsional draft sesuai hierarchy authority. | Mengklaim UI approved tanpa product/UI brief. | Product/UI brief tetap diperlukan sebelum sign-off |

## Apa yang Boleh Berjalan

1. Seluruh slice dapat dikirim ke `hospital-domain-architect` untuk arsitektur domain draft lengkap.
2. Blueprint backend/frontend, kontrak, ERD, dan backlog draft boleh disusun.
3. Tim dapat menyiapkan memo approval dengan hash decision log yang tepat.
4. Conflict source existing dapat disusun sebagai daftar repair/adapter.

## Apa yang Harus Berhenti

- Mengubah status blueprint atau keputusan menjadi `approved` tanpa memo aktual.
- Mengaktifkan policy klinis/privasi, break-glass, release, atau penghapusan retensi tanpa approval.
- Deployment produksi atau sign-off kesiapan tanpa approval.
- Menganggap endpoint existing sebagai kontrak target yang aman untuk record final.

## Endpoint Existing sebagai Bukti, Bukan Desain Target

Gate ini tidak merancang endpoint baru. Daftar berikut hanya menunjukkan konflik source yang harus
diperbaiki setelah approval.

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat CPPT existing. | `PatientIntegratedProgressNote : Create` | Body CPPT | `ApiResponse<PatientIntegratedProgressNoteCreateResponse>` |
| `PUT` | `/{id}` | Mengubah CPPT existing; konflik bila catatan sudah final. | `PatientIntegratedProgressNote : Update` | Body CPPT | `ApiResponse<PatientIntegratedProgressNoteUpdateResponse>` |
| `DELETE` | `/{id}` | Menandai CPPT terhapus tanpa correction contract RM. | `PatientIntegratedProgressNote : Delete` | Path `id` | `ApiResponse<object>` |

### Health Services / Clinical Management / Patient Clinical Document

Base URL: `api/v1/health-services/clinical-management/patient-clinical-documents`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat dokumen existing. | `PatientClinicalDocument : Create` | Body dokumen | `ApiResponse<PatientClinicalDocumentCreateResponse>` |
| `PUT` | `/{id}` | Mengubah dokumen existing, termasuk keadaan verified/approved tertentu. | `PatientClinicalDocument : Update` | Body dokumen | `ApiResponse<PatientClinicalDocumentUpdateResponse>` |
| `PATCH` | `/{id}/verify` | Memverifikasi dokumen dengan permission generic update. | `PatientClinicalDocument : Update` | Body verifikasi | `ApiResponse<object>` |
| `PATCH` | `/{id}/approve` | Menyetujui dokumen dengan permission generic update. | `PatientClinicalDocument : Update` | Body persetujuan | `ApiResponse<object>` |
| `DELETE` | `/{id}` | Menandai dokumen terhapus tanpa finality contract RM. | `PatientClinicalDocument : Delete` | Path `id` | `ApiResponse<object>` |

Respons `200` berarti request diterima; `400` berarti status atau isian tidak valid; `401` berarti
pengguna belum terautentikasi; `403` berarti permission teknis ditolak; `404` berarti record tidak
ditemukan; dan `409` dapat menandakan conflict. Respons tersebut belum membuktikan contextual
authorization atau invariant Rekam Medis target.

## Handoff yang Tepat

1. Kirim seluruh slice ke `$hospital-domain-architect` untuk melengkapi arsitektur domain draft.
2. Lanjutkan `$design-business-module` setelah arsitektur domain lengkap berstatus
   `DOMAIN_ARCHITECTURE_READY`.
3. Manajemen tetap harus menunjuk approver dan memperoleh memo sebelum aktivasi/produksi.
4. Gunakan `$grill-me` untuk mencatat bukti approval aktual, lalu jalankan ulang gate kesiapan.
