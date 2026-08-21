# Rekam Medis — Arsitektur Domain Lengkap untuk Development Draft

| Field | Nilai |
| --- | --- |
| Blueprint ID | `QV-RM-001` |
| Revision arsitektur | `1` |
| Tanggal | 21 Agustus 2026 |
| Status | `DOMAIN_ARCHITECTURE_READY` |
| Batas status | Siap untuk desain draft; bukan approval atau izin produksi |
| Requirement gate | Revision `4`, SHA-256 `4C828F9FEE69BE3FF1D7F74BF69300B83A546223E75076B0A26957DDCD641B94` |
| Decision baseline | Revision `1`, SHA-256 `CE28BB1126799FFC54C4515ED313AC470A22DD9D0F1126086864599A08B0F210` |
| Capability baseline | Revision `1`, SHA-256 `E16740282974D0820742E62C862B1A3F7CEA6BCE3449268667E17586925694C6` |
| Source snapshot | Backend `5103e68eec5529540d369673c8a4e2651be0344b`; frontend `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Production gate | `RM-APR-002`; approver belum ditunjuk menurut `RM-APR-005` |

## Scope dan Guardrail

Arsitektur mencakup episode Rekam Medis, record signed, correction/addendum, completeness policy,
akses normal/darurat, data sensitif, release, audit, pelaporan, retensi, downtime, integration
reliability, dan reference antarmodul untuk rawat jalan, IGD, serta rawat inap.

Menurut `RM-APR-006`, arsitektur dan blueprint draft boleh dilanjutkan. Akses darurat, release,
retensi/penghapusan, policy klinis, deployment produksi, dan sign-off tetap fail-closed sampai
approval formal tersedia.

## Ubiquitous Language

| Istilah | Definisi domain |
| --- | --- |
| Episode pelayanan | Encounter milik Registration yang menjadi anchor pelayanan. |
| Episode Rekam Medis | Lifecycle dokumentasi yang terkait satu encounter tetapi terpisah dari status layanan. |
| Record klinis | Fakta/dokumen milik context klinis yang direferensikan oleh Rekam Medis. |
| Record envelope | Metadata lifecycle Rekam Medis untuk record owner: signature, hash, versi, correction, dan status kesalahan. |
| Signature | Pengesahan dengan reauth, identitas, profesi, waktu, makna, dan hash isi. |
| Addendum/koreksi | Record baru yang mengubah atau menambahkan penjelasan tanpa menimpa versi lama. |
| Entered in Error | Penandaan record salah yang mempertahankan histori tetapi mengeluarkannya dari ringkasan/perhitungan. |
| Checklist snapshot | Salinan versi policy yang dibekukan saat episode dimulai. |
| Item conditional | Kewajiban yang muncul setelah peristiwa bisnis nyata. |
| Penugasan pelayanan | Hubungan formal bertanggal yang menjadi prasyarat akses normal. |
| Break-glass | Akses darurat sementara 15 menit dengan reauth, alasan, audit, dan review. |
| Release request | Permintaan formal pelepasan informasi dengan pemohon, dasar, tujuan, penerima, dan ruang data. |
| Legal hold | Larangan penghapusan data terkait sampai dicabut pejabat berwenang. |
| Provenance | Bukti owner, ID, versi/hash, waktu, dan korelasi asal data. |

## Peta Bounded Context

| Context ID | Context | Tanggung jawab | Upstream/downstream utama |
| --- | --- | --- | --- |
| `CTX-RM-EPI` | Medical Record Episode | Episode RM, checklist snapshot, applicability, completeness, closure/reopening. | Downstream Registration dan Clinical; upstream reporting/billing status. |
| `CTX-RM-REC` | Clinical Record Governance | Record envelope, signature, correction/addendum, entered-in-error, wrong-patient linkage. | Membungkus reference fakta milik Clinical/Lab/Radiology. |
| `CTX-RM-POL` | Medical Record Policy | Checklist, SLA/escalation, sensitive classification, retention, effective dating, approval state. | Supplier policy bagi seluruh context RM. |
| `CTX-RM-ACC` | Medical Record Access | Evaluasi role + assignment, break-glass session, sensitive expansion, access review. | Downstream Identity/Workforce/Registration; upstream audit. |
| `CTX-RM-RLS` | Information Release | Request, evidence, verification, approval, delivery, cancel/revoke/expire. | Downstream Patient/Identity; upstream privacy audit. |
| `CTX-RM-AUD` | Medical Record Audit & Reporting | Audit append-only, worklist, laporan, export audit, legal-hold visibility. | Mengonsumsi event seluruh context RM. |
| `CTX-RM-INT` | Medical Record Integration Reliability | Idempotency, durable event, retry, downtime import, reconciliation. | Boundary Clinical, Lab/Radiology, Pharmacy, Billing. |
| `CTX-PAT/REG/WRK/CLN/DIA/PHM/FIN` | Context owner existing | Pasien, encounter, workforce, fakta klinis, hasil, resep, finansial. | Tetap menjadi source of truth; RM hanya reference/adapter. |

## Katalog Konsep Domain

| Concept ID | Konsep | Klasifikasi | Owner | Identitas/lifecycle utama |
| --- | --- | --- | --- | --- |
| `RM-EPI-001` | Medical Record Episode | `AGGREGATE_ROOT` / `New` | `CTX-RM-EPI` | Satu identitas RM per encounter; `Aktif` → `Belum Lengkap` → `Ditutup Final`; dapat reopened/duplicate/cancelled sesuai aturan. |
| `RM-EPI-002` | Checklist Snapshot | `ENTITY` / `New` | `CTX-RM-EPI` | Versi policy dibekukan saat episode dimulai. |
| `RM-EPI-003` | Checklist Item | `ENTITY` / `New` | `CTX-RM-EPI` | Required/pending/completed/removed-with-review; histori tidak dihapus. |
| `RM-REC-001` | Record Envelope | `AGGREGATE_ROOT` / `New` | `CTX-RM-REC` | Mereferensikan owner record; draft/signed/entered-in-error; append-only setelah signed. |
| `RM-REC-002` | Signature Evidence | `VALUE_OBJECT` | `CTX-RM-REC` | Signer, profesi, waktu, meaning, reauth evidence, content hash. |
| `RM-REC-003` | Record Amendment | `ENTITY` / `New` | `CTX-RM-REC` | Correction/addendum menunjuk record/versi sebelumnya. |
| `RM-POL-001` | Completeness Policy | `AGGREGATE_ROOT` / `New` | `CTX-RM-POL` | Draft/effective/retired; versioned dan effective-dated. |
| `RM-POL-002` | Privacy & Release Policy | `AGGREGATE_ROOT` / `New` | `CTX-RM-POL` | Fail-closed sampai approval; kategori, evidence, review, retention. |
| `RM-ACC-001` | Emergency Access Session | `AGGREGATE_ROOT` / `New` | `CTX-RM-ACC` | Requested/active/expired/ended; maksimal 15 menit. |
| `RM-ACC-002` | Access Review | `ENTITY` / `New` | `CTX-RM-ACC` | Pending/outcome/escalated; SLA 1 hari atau 4 jam 24/7. |
| `RM-RLS-001` | Release Request | `AGGREGATE_ROOT` / `New` | `CTX-RM-RLS` | Request, verification, approval, delivery, revoke/cancel/expire. |
| `RM-RLS-002` | Release Delivery | `ENTITY` / `New` | `CTX-RM-RLS` | Disiapkan/Diserahkan/Sebagian/Gagal/Dibatalkan/Kedaluwarsa/Dicabut. |
| `RM-AUD-001` | Audit Entry | `ENTITY` append-only / `New` | `CTX-RM-AUD` | Pelaku, waktu, purpose, object, action, outcome, correlation. |
| `RM-INT-001A` | Integration Message | `ENTITY` / `New` | `CTX-RM-INT` | Idempotency key, payload hash, status delivery/retry. |
| `RM-INT-002A` | Reconciliation Case | `AGGREGATE_ROOT` / `New` | `CTX-RM-INT` | Open/resolved/escalated; tidak menimpa clinical history. |
| `RM-REF-001`–`008` | Reference/provenance owner existing | `REFERENCE_DATA`/`Adapter/View` | Context owner | Mengikuti arsitektur ownership/reference revision sebelumnya. |

Konsep domain di atas bukan satu-per-satu tabel. Batas persistence ditetapkan pada
`design-business-module`, bukan dari nama form atau layar.

## Aggregate dan Invariant

### Medical Record Episode

- Tepat satu episode kanonik per encounter; duplikat tetap tersimpan dan tertaut.
- Versi checklist tidak berubah setelah episode dimulai.
- Applicability item conditional dapat bertambah hanya setelah event nyata.
- Item tidak boleh dikeluarkan tanpa alasan, review Unit RM, pengesahan klinis, dan histori.
- `Ditutup Final` hanya bila semua item wajib terbukti lengkap dan signed.
- Billing tidak boleh menahan signature atau closure RM.

### Record Envelope

- Owner record ID dan provenance wajib.
- Signature memerlukan reauth dan content hash.
- Isi signed tidak dapat dimutasi; amendment selalu record baru.
- Entered-in-error tidak menghapus record dan membutuhkan maker-checker sesuai episode.
- Wrong-patient membuat record baru pada pasien benar dan linkage audit dua arah.

### Emergency Access Session

- Tidak dapat aktif bila duration, sensitive category, reviewer/SLA/outcome/escalation policy belum
  approved.
- Durasi maksimal 15 menit; renewal memerlukan reauth dan alasan baru.
- Normal assignment yang terbentuk mengakhiri session lebih cepat.
- Semua read/write yang diizinkan masuk audit dan review.

### Release Request

- Pemohon, kewenangan, tujuan, penerima, evidence, dan data scope wajib.
- Approval tidak dapat melampaui data scope atau masa berlaku.
- Retry dengan key sama dan isi sama idempotent; isi berbeda conflict.
- Data yang sudah diserahkan tidak dianggap dapat ditarik kembali.

## Relasi Logis

| Sumber | Tujuan | Kardinalitas | Makna |
| --- | --- | --- | --- |
| Patient | Medical Record Episode | `1 : 0..*` | Pasien dapat memiliki banyak episode RM. |
| Encounter | Medical Record Episode | `1 : 1` kanonik | Encounter menjadi anchor; duplicate episode tertaut terpisah. |
| Medical Record Episode | Checklist Snapshot | `1 : 1` | Snapshot policy saat mulai. |
| Checklist Snapshot | Checklist Item | `1 : 1..*` | Kewajiban episode. |
| Medical Record Episode | Record Envelope | `1 : 0..*` | Record yang diperhitungkan dalam episode. |
| Record Envelope | Record Amendment | `1 : 0..*` | Correction/addendum append-only. |
| User/Workforce Assignment | Access Decision | `1 : 0..*` | Assignment formal menjadi prasyarat akses normal. |
| Emergency Access Session | Access Review | `1 : 1..*` | Seluruh session masuk review. |
| Release Request | Release Delivery | `1 : 0..*` | Delivery parsial/retry berada dalam approval scope. |

## Lifecycle Utama

### Episode Rekam Medis

| Dari | Tindakan | Ke | Wewenang/syarat |
| --- | --- | --- | --- |
| Tidak ada | Mulai dari encounter | `Aktif` | Encounter valid; snapshot policy tersimpan. |
| `Aktif` | Layanan berakhir dengan item kurang | `Belum Lengkap` | Sistem completeness; tidak auto-close. |
| `Aktif`/`Belum Lengkap` | Semua item signed | `Ditutup Final` | Bukti item lengkap. |
| `Ditutup Final` | Late data memengaruhi safety/kewajiban | `Belum Lengkap` | Unit RM + pejabat klinis, alasan, audit. |
| `Aktif` | Registrasi salah tanpa signed/service nyata | `Dibatalkan` | Aturan cancellation. |
| Status nonterminal | Ditetapkan duplicate | `Duplikat` | Unit RM memilih kanonik; record tidak dipindah. |

### Release

Lifecycle delivery memakai `Disiapkan`, `Diserahkan`, `Diserahkan Sebagian`, `Gagal`, `Dibatalkan`,
`Kedaluwarsa`, dan `Dicabut`. Setelah kedaluwarsa/dicabut, retry dilarang dan request baru wajib.

## Authorization

- Akses normal memerlukan role sesuai dan assignment aktif bertanggal.
- Mantan tim tidak memiliki akses normal setelah assignment/episode berakhir.
- Correction proxy aktif: DPJP/pengganti resmi; pasca-closure: Kepala Pelayanan/pejabat Komite.
- Pengesah wrong-patient harus berbeda dari pengaju.
- Break-glass membatasi scope baca/tulis; mass download dan release dilarang.
- Data sensitif default-hidden; pembukaan memerlukan reauth, alasan, dan priority review.
- Release diverifikasi Unit RM; kasus sensitif/pengecualian ditinjau privacy/legal.

## Audit dan Histori

Audit append-only mencakup signature, correction, entered-in-error, access/read, sensitive open,
break-glass, review outcome, release, export, policy change, checklist change, downtime, retry,
reconciliation, dan reopening. Payload log operasional tidak memuat isi klinis sensitif.

Retensi draft policy: record/versi/signature 25 tahun sejak interaksi terakhir; audit akses/release
10 tahun; bukti downtime/integrasi 10 tahun setelah rekonsiliasi; snapshot laporan 5 tahun. Legal
hold selalu menang. Penghapusan tetap disabled sampai policy approved.

## Integrasi dan Reliability

| Producer | Consumer | Kontrak domain | Kegagalan |
| --- | --- | --- | --- |
| Registration | `CTX-RM-EPI` | Encounter started/ended/cancelled reference | Jangan menyamakan akhir layanan dengan closure RM. |
| Clinical owners | `CTX-RM-REC/EPI` | Owner record + signature/provenance/event | Retry idempotent; signed record tetap sah. |
| Lab/Radiology | `CTX-RM-INT/REC` | Released result, corrected/withdrawn version | Simpan semua versi; notify DPJP/tim aktif. |
| Pharmacy | RM | Prescription reference/status | Tidak mengambil alih dispensing. |
| RM | Billing/Casemix | Completeness status | Billing menentukan readiness sendiri. |
| Downtime import | `CTX-RM-REC` | Nomor form unik, event time, input time | Rekonsiliasi anti-duplikat; signature pembuat tetap wajib. |

Duplicate submit memakai idempotency key dan payload hash. Partial failure berstatus
`Sinkronisasi Tertunda`, memakai durable retry dan rekonsiliasi manual. Pilihan transport teknis
ditentukan di blueprint, bukan domain architecture.

## Pelaporan

Worklist real-time memuat layanan, dokumen, penanggung jawab, dan umur keterlambatan. Laporan
signature/correction/entered-in-error, break-glass, release, downtime, dan sinkronisasi dibatasi per
peran serta diaudit. Jadwal: harian 07.00 WIB, mingguan Senin 08.00 WIB, bulanan hari kerja kelima;
insiden tetap event-driven.

## Dampak Billing

RM menerbitkan completeness status. Coding/claim/billing menentukan readiness sendiri. Tidak ada
charge yang dibuat oleh aggregate RM dan proses finansial tidak boleh menahan signature/closure.

## Dampak Keselamatan Klinis

Status `RELEVAN_TERHADAP_KESELAMATAN`. Guardrail utama: patient–encounter consistency, immutable
signed record, maker-checker wrong-patient, data keselamatan tampil lebih dulu, sensitive masking,
all-version result, assignment authorization, fail-closed policy, dan audited emergency access.

## Gap yang Ditunda

| ID | Gap | Dampak sekarang | Gerbang akhir |
| --- | --- | --- | --- |
| `RM-APR-002` | Individu approver dan memo belum ada. | Tidak memblokir desain draft menurut `RM-APR-006`. | Memblokir approved/activation/production/sign-off. |
| `ARCH-RM-SRC-001` | CPPT/dokumen existing mutable. | Menjadi repair constraint. | Harus diperbaiki sebelum invariant dianggap terpenuhi. |
| `ARCH-RM-SRC-002` | Security existing tidak contextual. | Menjadi extension constraint. | Harus ada enforcement patient/encounter assignment. |
| `ARCH-RM-SRC-003` | Finalisasi frontend multi-request. | Menjadi reliability constraint. | Harus menjadi idempotent/atomic atau dapat direkonsiliasi. |

## Traceability Ringkas

| Domain slice | Decision utama |
| --- | --- |
| Record lifecycle | `RM-SIG-*`, `RM-COR-*`, `RM-EIE-*`, `RM-LFC-*` |
| Completeness | `RM-CLS-001`–`RM-CLS-020` |
| Access/privacy | `RM-REL-*`, `RM-PRV-*` |
| Release | `RM-RLS-*` |
| Integration/reliability | `RM-INT-*`, `RM-EXC-*`, `RM-BIL-001` |
| Reporting/retention | `RM-RPT-*` |
| Development/approval gate | `RM-APR-002`–`RM-APR-006` |

## Kesiapan dan Handoff

Status arsitektur domain lengkap: `DOMAIN_ARCHITECTURE_READY` untuk `$design-business-module`
berstatus `draft`.

Arsitektur ini tidak boleh digunakan untuk menandai blueprint `approved`. Sebelum aktivasi fitur
berisiko, deployment produksi, atau sign-off, `RM-APR-002` harus ditutup dan gate kesiapan dijalankan
ulang.
