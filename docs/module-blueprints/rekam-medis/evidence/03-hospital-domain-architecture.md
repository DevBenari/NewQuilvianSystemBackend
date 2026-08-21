# Rekam Medis — Arsitektur Domain Slice Ownership dan Reference

| Field | Nilai |
| --- | --- |
| Blueprint ID | `QV-RM-001` |
| Revision arsitektur | `2` |
| Tanggal | 21 Agustus 2026 |
| Status slice | `DOMAIN_ARCHITECTURE_READY` |
| Status modul keseluruhan | `DOMAIN_ARCHITECTURE_PARTIAL` |
| Requirement gate | `evidence/02-requirement-completeness-gate.md`, revision `3`, SHA-256 `126D6BD13AABC9232A2FB909C7A5B6619B52CF1A86B695994AA45B84BD28AD17` |
| Decision baseline | `00-interview-decisions.md`, revision `1`, SHA-256 `985389AC736725F74F2933A74CFC239C7BAFEBCB827A3C103142EF60B627A2E8` |
| Capability baseline | `01-existing-capability-map.md`, SHA-256 `E16740282974D0820742E62C862B1A3F7CEA6BCE3449268667E17586925694C6` |
| Source snapshot | Backend `5103e68eec5529540d369673c8a4e2651be0344b`; frontend `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |

## Scope Arsitektur

### Di dalam scope

- Ownership identitas pasien dan nomor Rekam Medis.
- Encounter sebagai anchor episode pelayanan.
- Referensi tenaga kesehatan, profesi, dan identitas pengguna.
- Referensi unit pelayanan, klinik, ruang, dan tempat tidur.
- Referensi fakta klinis yang dimiliki Clinical Management.
- Batas ownership hasil laboratorium/radiologi dan resep.
- Aturan anti-duplikasi sumber kebenaran pada konteks Rekam Medis.

### Di luar scope dan tetap terblokir

- Aggregate dan lifecycle episode Rekam Medis.
- Signature, correction, addendum, dan `Entered in Error`.
- Checklist, SLA, reminder, escalation, dan laporan kelengkapan.
- Contextual authorization, break-glass, data sensitif, dan release.
- Kontrak integrasi final, API target, database, dan UI.

Bagian di luar scope bergantung pada `RM-APR-002`. `RM-APR-005` mengonfirmasi bahwa individu
approver belum ditunjuk. Dokumen ini tidak mengisi atau mengakali blocker tersebut.

## Kesimpulan Arsitektur

Rekam Medis harus menjadi konsumen data lintas konteks, bukan pemilik baru atas pasien, encounter,
tenaga kesehatan, lokasi, fakta klinis, hasil penunjang, resep, atau proses finansial. Slice ini tidak
membutuhkan aggregate root baru di dalam Rekam Medis. Ia menetapkan reference dan
adapter/view—yaitu representasi baca atau penerjemah data milik konteks lain—dengan identitas owner
yang tetap dipertahankan.

**Contoh:** Nomor Rekam Medis pasien tetap berasal dari Patient Management. Rekam Medis boleh
menampilkan nomor tersebut bersama sebuah episode, tetapi tidak boleh membuat nomor pasien kedua
atau mengubah nama pasien melalui domain Rekam Medis.

## Ubiquitous Language

| Istilah | Makna tunggal dalam slice ini |
| --- | --- |
| Pasien | Subjek pelayanan yang identitas otoritatifnya dimiliki Patient Management. |
| Nomor Rekam Medis | Pengenal pasien yang diterbitkan dan dikelola Patient Management; bukan ID episode. |
| Encounter | Kejadian pelayanan yang dimiliki Registration Management dan menjadi anchor episode pelayanan. |
| Episode Rekam Medis | Lifecycle kelengkapan dokumentasi yang kelak mereferensikan encounter; desainnya belum termasuk slice ini. |
| Tenaga kesehatan | Workforce/professional yang identitas, profesi, dan credential-nya berasal dari owner HR/Identity. |
| Penugasan pelayanan | Hubungan formal bertanggal antara tenaga kesehatan dan pelayanan; lifecycle authorization-nya belum dirancang pada slice ini. |
| Lokasi pelayanan | Unit, klinik, ruang, atau tempat tidur yang dimiliki master data/registration. |
| Fakta klinis | Diagnosis, tindakan, alergi, tanda vital, assessment, consent, CPPT, dokumen, atau fakta lain yang owner-nya tetap Clinical Management. |
| Hasil penunjang | Hasil yang owner-nya tetap laboratorium/radiologi; Rekam Medis hanya menerima reference dan salinan released. |
| Reference | Identitas logis menuju data owner tanpa mengambil alih lifecycle-nya. |
| Adapter/View | Penerjemah atau tampilan baca data owner untuk kebutuhan Rekam Medis tanpa menjadi sumber kebenaran baru. |
| Provenance | Bukti asal data: owner, ID owner, versi, waktu, dan korelasi sumber. |

## Peta Bounded Context

| Context ID | Bounded context | Tanggung jawab otoritatif | Hubungan dengan Rekam Medis |
| --- | --- | --- | --- |
| `CTX-PAT` | Patient Management | Identitas pasien, nomor Rekam Medis, dan status master pasien. | Upstream; Rekam Medis memakai patient reference. |
| `CTX-REG` | Registration Management | Encounter dan konteks pelayanan yang mendaftarkan pasien. | Upstream; encounter menjadi anchor episode pelayanan. |
| `CTX-WRK` | Workforce/Identity | Pengguna, tenaga kesehatan, profesi, credential, dan identitas organisasi. | Upstream; Rekam Medis memakai workforce reference. |
| `CTX-LOC` | Health Services Master Data/Registration | Unit pelayanan, klinik, ruang, dan tempat tidur. | Upstream; Rekam Medis memakai location reference. |
| `CTX-CLN` | Clinical Management | Fakta dan dokumen klinis yang dibuat selama pelayanan. | Upstream; Rekam Medis mengindeks atau menampilkan reference, bukan menyalin ownership. |
| `CTX-DIA` | Laboratory/Radiology | Order, hasil, validasi, hasil kritis, acknowledgment, dan versi hasil. | Upstream; hanya hasil released/signed yang direferensikan atau disalin dengan provenance. |
| `CTX-PHM` | Pharmacy Management | Resep dan lifecycle farmasi/dispensing. | Upstream; Rekam Medis membaca summary/status tanpa mengambil alih transaksi. |
| `CTX-FIN` | Billing/Casemix/Keuangan | Readiness coding, claim, charge, dan proses finansial. | Downstream/peer; tidak menjadi owner catatan atau closure Rekam Medis. |
| `CTX-RM` | Rekam Medis | Konsumen reference dan kelak owner lifecycle dokumentasi Rekam Medis. | Downstream terhadap seluruh owner reference. |

Hubungan antarcontext memakai pola customer/supplier dengan owner upstream sebagai sumber
kebenaran. Bila bentuk data owner tidak cocok, `CTX-RM` memakai anti-corruption adapter agar istilah
atau status internal owner tidak bocor menjadi kebijakan Rekam Medis.

## Katalog Konsep Domain

| Concept ID | Konsep | Klasifikasi | Ownership | Identitas logis | Invariant utama | Bukti |
| --- | --- | --- | --- | --- | --- | --- |
| `RM-REF-001` | Patient Reference | `REFERENCE_DATA` | `Existing` — `CTX-PAT` | ID pasien milik Patient Management | Nomor dan identitas pasien tidak diterbitkan ulang oleh RM. | `RM-CAP-01`, `RM-SCP-001` |
| `RM-REF-002` | Encounter Reference | `REFERENCE_DATA` | `Existing` — `CTX-REG` | ID encounter milik Registration | Setiap reference episode pelayanan menunjuk encounter existing; status layanan bukan status kelengkapan RM. | `RM-CAP-03`, `RM-CLS-001` |
| `RM-REF-003` | Workforce Reference | `REFERENCE_DATA` | `Existing` — `CTX-WRK` | ID user/workforce owner | RM tidak membuat master tenaga kesehatan atau profesi tandingan. | `RM-CAP-05`, `RM-REL-001` |
| `RM-REF-004` | Care Location Reference | `REFERENCE_DATA` | `Existing` — `CTX-LOC` | ID unit/klinik/ruang/bed owner | Perubahan master lokasi tetap dilakukan owner. | `RM-CAP-06` |
| `RM-REF-005` | Clinical Fact Reference | `REFERENCE_DATA` | `Existing` — `CTX-CLN` | ID fakta klinis dan jenis owner | RM tidak membuat salinan editable yang bersaing dengan fakta klinis owner. | `RM-CAP-07`–`RM-CAP-15` |
| `RM-REF-006` | Diagnostic Result Reference | `EXTERNAL_CONTRACT` | `Adapter/View` dari `CTX-DIA` | ID hasil dan versi owner | Hanya versi released/signed; semua versi lama tetap dapat ditelusuri. | `RM-INT-001`, `RM-INT-002` |
| `RM-REF-007` | Prescription Reference | `REFERENCE_DATA` | `Adapter/View` dari `CTX-PHM` | ID prescription owner | RM tidak mengubah dispensing atau status farmasi. | `RM-CAP-17` |
| `RM-REF-008` | Source Provenance | `VALUE_OBJECT` | `CTX-RM` untuk reference yang dikonsumsi | Owner context, owner ID, versi/hash, waktu sumber | Reference tanpa asal-usul yang dapat diverifikasi tidak boleh dianggap bukti klinis final. | `RM-SIG-002`, `RM-INT-001`, `RM-INT-002` |

`RM-REF-008` adalah konsep logis, bukan pernyataan bahwa harus ada satu tabel bernama provenance.

## Model Aggregate dan Ownership

Tidak ada aggregate root baru dalam scope siap ini. Aggregate upstream tetap berada di context
masing-masing:

| Aggregate upstream | Owner | Yang boleh disimpan RM | Yang dilarang |
| --- | --- | --- | --- |
| Patient | `CTX-PAT` | Reference ID dan data baca yang diperlukan sesuai izin. | Master pasien atau nomor Rekam Medis tandingan. |
| Patient Encounter | `CTX-REG` | Reference ID dan konteks episode pelayanan. | Mengubah status layanan dari aggregate RM. |
| Workforce/Professional | `CTX-WRK` | Reference user/workforce/profesi pada waktu kejadian. | Mengubah profesi, credential, atau penugasan HR. |
| Location master | `CTX-LOC` | Reference unit/klinik/ruang/bed. | Master lokasi tandingan. |
| Clinical fact/document | `CTX-CLN` | Reference dan, bila policy kelak mengizinkan, projection baca dengan provenance. | Salinan editable yang mengubah sumber kebenaran. |
| Diagnostic result | `CTX-DIA` | Reference dan salinan released/signed beserta versi. | Validasi, koreksi, hasil kritis, atau acknowledgment owner. |
| Prescription | `CTX-PHM` | Reference dan summary baca. | Order/dispense/status farmasi tandingan. |

Aggregate episode Rekam Medis kelak mungkin membutuhkan reference ini, tetapi batas aggregate-nya
tidak dirancang sebelum `RM-APR-002` selesai.

## Model Relasi Logis

| Sumber | Tujuan | Kardinalitas logis | Makna | Arah ownership |
| --- | --- | --- | --- | --- |
| Patient | Encounter | `1` ke `0..*` | Seorang pasien dapat memiliki banyak encounter. | Patient dan Encounter tetap pada owner masing-masing. |
| Encounter | Patient | `*` ke `1` | Setiap encounter harus menunjuk satu pasien owner. | `CTX-REG` mereferensikan `CTX-PAT`. |
| Encounter | Care Location | `*` ke `0..*` sepanjang layanan | Encounter dapat berpindah konteks lokasi. | Lokasi tetap dimiliki `CTX-LOC`; histori pelayanan dimiliki owner encounter. |
| Encounter | Clinical Fact | `1` ke `0..*` | Fakta episode mereferensikan encounter sesuai kontrak owner klinis. | Fakta tetap dimiliki `CTX-CLN`. |
| Clinical Fact | Workforce | `*` ke `1..*` sesuai jenis fakta | Pembuat, pemeriksa, atau signer direferensikan dari workforce. | Identitas workforce tetap `CTX-WRK`. |
| Encounter | Diagnostic Result | `1` ke `0..*` | Hasil released dapat dikaitkan dengan pelayanan. | Hasil tetap `CTX-DIA`. |
| Encounter | Prescription | `1` ke `0..*` | Resep pelayanan dapat ditampilkan sebagai reference. | Resep tetap `CTX-PHM`. |

Relasi di atas bersifat logis. Dokumen ini tidak menetapkan foreign key, tabel, atau mekanisme
sinkronisasi fisik.

## Lifecycle Reference

Reference tidak memiliki lifecycle klinis tandingan. Status upstream selalu menang.

| Kejadian pada owner | Perilaku boundary RM | Larangan |
| --- | --- | --- |
| Patient/encounter tersedia | Resolve ID owner dan verifikasi pasangan patient–encounter. | Membuat record patient/encounter baru secara diam-diam. |
| Owner sementara tidak tersedia | Pertahankan ID dan provenance terakhir; tampilkan bahwa sumber belum dapat diverifikasi bila digunakan. | Menganggap data kosong sebagai data tidak pernah ada. |
| Hasil penunjang dikoreksi | Tambahkan versi baru dan tandai versi lama `Digantikan`/`Ditarik` sesuai event owner. | Menimpa isi versi lama. |
| Master lokasi/profesi berubah | Gunakan owner sebagai data terkini; histori kejadian mempertahankan reference/provenance saat kejadian bila diwajibkan. | Menulis ulang histori klinis tanpa audit. |
| Patient merge/identity reconciliation terjadi | Tahan reassociation klinis sampai event/keputusan owner yang berwenang dapat diverifikasi. | Memindahkan catatan hanya dari perubahan tampilan atau kemiripan identitas. |

**Contoh:** Owner hasil penunjang merilis versi 1 pukul 09.00 lalu menariknya dan merilis versi 2
pukul 11.00. Rekam Medis tidak mengganti isi versi 1. Reference versi 1 tetap ada dengan status
`Ditarik`, lalu ditautkan ke versi 2.

## Tanggung Jawab Authorization

- Kemampuan resolve sebuah reference tidak memberi hak melihat isi klinisnya.
- `CTX-PAT`, `CTX-REG`, `CTX-WRK`, dan `CTX-CLN` tetap menerapkan permission owner pada provider
  existing.
- Rekam Medis kelak wajib menambahkan contextual authorization berdasarkan role dan penugasan aktif;
  modelnya berada di luar slice ini dan terblokir `RM-APR-002`.
- Adapter tidak boleh memakai bypass teknis seperti `SuperAdmin` sebagai kebijakan klinis.
- Pengguna yang dapat melihat identitas pasien belum tentu boleh melihat seluruh fakta klinis.

**Contoh:** Petugas dapat mencari encounter untuk pekerjaan registrasi. Hak tersebut tidak otomatis
memberinya hak membuka CPPT pasien melalui Rekam Medis.

## Audit dan Histori

Untuk setiap reference yang digunakan sebagai bukti klinis, boundary harus dapat menelusuri:

- owner context dan owner record ID;
- jenis reference;
- versi atau hash bila owner menyediakannya;
- waktu data diterbitkan dan waktu diterima/dibaca;
- correlation/idempotency key bila reference datang melalui integrasi;
- perubahan hubungan ke versi pengganti;
- hasil verifikasi pasangan patient–encounter.

Audit reference tidak menggantikan audit klinis milik owner dan tidak boleh dipakai untuk mengubah
isi owner.

## Model Integrasi

| Producer | Consumer | Tujuan | Sumber kebenaran | Saat gagal |
| --- | --- | --- | --- | --- |
| Patient Management | Rekam Medis | Resolve identitas dan nomor Rekam Medis. | Patient Management | Jangan membuat pasien lokal; pertahankan reference dan tandai verifikasi tertunda. |
| Registration | Rekam Medis | Resolve encounter dan konteks pelayanan. | Registration | Jangan menyamakan kegagalan lookup dengan encounter batal. |
| Workforce/Identity | Rekam Medis | Resolve pelaku/profesi. | Workforce/Identity | Jangan menghapus identitas historis signer karena profil terkini tidak tersedia. |
| Clinical Management | Rekam Medis | Menyajikan fakta klinis. | Clinical Management | Jangan membuat salinan editable untuk menutup kegagalan baca. |
| Lab/Radiologi | Rekam Medis | Mengirim hasil released dan koreksinya. | Owner penunjang | Simpan event tahan gagal, retry idempotent, dan rekonsiliasi tanpa menimpa versi. |
| Pharmacy | Rekam Medis | Menyajikan reference resep. | Pharmacy | Status tidak boleh ditebak dari salinan lama. |

Pilihan sinkron, asinkron, cache, atau projection fisik belum ditetapkan. Module blueprint hilir hanya
boleh memilihnya setelah menunjukkan bahwa ownership, provenance, retry, dan rekonsiliasi tetap
terjaga.

## Dampak Billing

Slice reference tidak membuat, mengubah, atau membatalkan charge. Billing/Casemix/Keuangan tetap
owner readiness finansial. Rekam Medis tidak boleh mengubah master atau transaksi billing melalui
reference ini.

## Dampak Keselamatan Klinis

Status: `RELEVAN_TERHADAP_KESELAMATAN`.

Risiko terbesar adalah salah pasien, encounter yang tidak cocok, fakta klinis tanpa provenance, dan
versi hasil yang ditimpa. Guardrail slice ini:

1. Patient ID dan encounter ID selalu berasal dari owner.
2. Pasangan patient–encounter diverifikasi sebelum reference klinis dipakai.
3. Reassociation tidak dilakukan hanya karena nama atau nomor tampak serupa.
4. Hasil penunjang mempertahankan seluruh versi.
5. Data owner yang tidak tersedia tidak diterjemahkan menjadi “tidak ada masalah klinis”.

## Endpoint Existing sebagai Bukti

Endpoint berikut membuktikan provider existing. Ia bukan kontrak API target Rekam Medis.

### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/admin/{id}` | Membaca identitas pasien existing. | `Patient : Read` | Path `id` | `ApiResponse<PatientDetailResponse>` |
| `GET` | `/admin/options` | Memilih patient reference dari data owner. | `Patient : Read` | Query filter | `ApiResponse<PatientOptionPagedResponse>` |

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}` | Membaca encounter yang menjadi anchor episode pelayanan. | `PatientEncounter : Read` | Path `id` | `ApiResponse<PatientEncounterDetailResponse>` |
| `GET` | `/admin/options` | Memilih encounter reference, termasuk filter pasien. | `PatientEncounter : Read` | Query patient/filter | `ApiResponse<PatientEncounterOptionPagedResponse>` |

### Health Services / Master Data / Service Unit

Base URL: `api/v1/health-services/master-data/service-units`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}` | Membaca unit pelayanan owner. | `ServiceUnit : Read` | Path `id` | `ApiResponse<ServiceUnitDetailResponse>` |
| `GET` | `/options` | Memilih service-unit reference. | `ServiceUnit : Read` | Query filter | `ApiResponse<ServiceUnitOptionPagedResponse>` |

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}` | Membaca detail CPPT existing sebagai fakta Clinical Management. | `PatientIntegratedProgressNote : Read` | Path `id` | `ApiResponse<PatientIntegratedProgressNoteDetailResponse>` |
| `GET` | `/timeline` | Membaca timeline CPPT berdasarkan patient/encounter. | `PatientIntegratedProgressNote : Read` | Query patient/encounter | `ApiResponse<List<PatientIntegratedProgressNoteTimelineResponse>>` |

Respons `200` berarti data berhasil dibaca; `401` berarti pengguna belum terautentikasi; `403`
berarti permission teknis ditolak; dan `404` berarti reference owner tidak ditemukan. Kode `200`
belum membuktikan bahwa contextual authorization Rekam Medis sudah terpenuhi.

## Gap dan Constraint yang Dipertahankan

| ID | Jenis | Isi | Dampak |
| --- | --- | --- | --- |
| `ARCH-RM-REF-001` | Blocker modul | `RM-APR-002` belum selesai dan `RM-APR-005` mengonfirmasi approver belum ditunjuk. | Aggregate/lifecycle RM, delivery planning, dan implementasi tetap tidak boleh dilanjutkan. |
| `ARCH-RM-REF-002` | Repair source | Patient merge existing belum membuktikan maker-checker, reversal, dan histori immutable. | Adapter tidak boleh melakukan reassociation otomatis. |
| `ARCH-RM-REF-003` | Conflict source | CPPT/dokumen existing memiliki mutation path yang bertentangan dengan finality RM. | Provider boleh menjadi sumber reference, tetapi bukan kontrak finality target. |
| `ARCH-RM-REF-004` | Conflict source | Permission existing tidak membawa patient/encounter context. | Provider read tidak boleh dianggap cukup untuk akses RM. |
| `ARCH-RM-REF-005` | Batas desain | Mekanisme sync/cache/event belum dipilih. | Module blueprint wajib mempertahankan ownership dan provenance saat memilih mekanisme. |

## Traceability Requirement ke Domain

| Requirement/capability | Keputusan domain |
| --- | --- |
| `RM-CAP-01` | Patient tetap `Existing` pada `CTX-PAT`; RM memakai `RM-REF-001`. |
| `RM-CAP-03` | Encounter tetap `Existing` pada `CTX-REG`; RM memakai `RM-REF-002`. |
| `RM-CAP-05` | Workforce tetap `Existing`; RM memakai `RM-REF-003`. |
| `RM-CAP-06` | Lokasi tetap `Existing`; RM memakai `RM-REF-004`. |
| `RM-CAP-07`–`RM-CAP-15` | Fakta klinis tetap pada `CTX-CLN`; RM memakai `RM-REF-005`. |
| `RM-INT-001`, `RM-INT-002` | Hasil penunjang memakai `RM-REF-006` dan mempertahankan versi owner. |
| `RM-BIL-001` | Reference slice tidak menciptakan dependency charge atau menahan closure. |

## Kesiapan dan Handoff

Status slice ownership/reference: `DOMAIN_ARCHITECTURE_READY`.

Status arsitektur modul Rekam Medis keseluruhan: `DOMAIN_ARCHITECTURE_PARTIAL` karena
`RM-APR-002` masih memblokir seluruh policy final.

Slice siap ini boleh menjadi input `$design-business-module` hanya untuk mengunci ownership,
reference, anti-duplication, provenance, dan boundary adapter. Ia tidak memberi izin merancang
aggregate episode, state machine klinis, kontrak privacy/release, API target, database, atau UI.

Setelah memo approval tersedia, requirement gate harus dijalankan ulang sebelum arsitektur domain
modul lengkap dilanjutkan.
