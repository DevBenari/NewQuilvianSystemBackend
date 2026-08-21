# State Transition Matrix — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-state-v0.2-draft` |
| Status | `draft` |
| Owner | Clinical Management untuk state existing; Unit RM hanya consumer |
| Approval | Belum tersedia — `RM-APR-002` |
| Input | Domain architecture revision `1`; source backend `5103e68` |
| Compatibility | Tidak mengubah enum atau transisi source |

Matriks memisahkan state as-is dari arti Rekam Medis. `Completed`, `Verified`, `Approved`, dan
`Signed` existing tidak boleh diterjemahkan otomatis menjadi record final RM.

## Assessment dan konsultasi

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat existing | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Mulai/isi pelayanan | `InProgress` | Permission create/update owner | Patient–encounter valid | Tolak `400/409`; jangan buat context lokal. |
| `Draft`/`InProgress` | Complete | `Completed` | `PatientAssessment/DoctorConsultation : Update` | Validasi owner lulus | Tetap pada status lama; tampilkan kekurangan. |
| `Draft`/`InProgress` | Cancel | `Cancelled` | Permission update owner | Alasan sesuai request | Tolak bila status final atau konteks salah. |
| `Completed` | Update/ubah SOAP | Tidak sah untuk target RM | — | Harus melalui correction/addendum setelah signature | Endpoint existing harus diberi guard; jangan overwrite. |
| `Completed`/`Cancelled` | Complete ulang | Tidak sah | — | Tidak ada | Tolak `400/409`; idempotency tidak boleh membuat completion kedua. |

## Diagnosis

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | Create | `Active` | `PatientDiagnosis : Create` | Encounter, konsultasi, patient, doctor valid | Tolak `400/404/409`. |
| `Active` | Set primary | `Active` + `IsPrimary=true` | `PatientDiagnosis : Update` | Hanya satu diagnosis utama aktif dalam konteks owner | Tolak konflik; jangan memilih otomatis. |
| `Active` | Resolve | `Resolved` | `PatientDiagnosis : Update` | Alasan/waktu sesuai request | Tolak bila sudah cancelled. |
| `Active` | Cancel | `Cancelled` | `PatientDiagnosis : Update` | Belum terkunci finality RM | Jika signed, wajib correction workflow. |
| `Resolved`/`Cancelled` | Update isi lama | Tidak sah untuk target RM | — | Correction/addendum baru | Tolak overwrite. |

## Tindakan

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Tidak ada | Create/select | `Planned` | `PatientProcedure : Create` | Master/konteks valid | Tolak `400/409`. |
| `Planned`/`Ordered` | Execute | `InProgress` atau `Completed` sesuai owner | `PatientProcedure : Update` | Approval owner bila dibutuhkan | Tolak; jangan membuat checklist completed palsu. |
| Status aktif | Cancel | `Cancelled` | `PatientProcedure : Update` | Alasan tersedia | Jika event sudah memicu item RM, item tidak dihapus otomatis. |
| `Completed` | Update/delete | Tidak sah untuk record final | — | Correction workflow | Tolak overwrite/soft-delete. |

## Alergi dan tanda vital

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Active`/`Recorded` | Verify | `Active`/`Verified` | Permission update owner | Verifier dan konteks valid | Tolak `404/409`. |
| `Active` | Resolve alergi | `Resolved` | `PatientAllergy : Update` | Alasan klinis | Alergi tetap aktif bila gagal. |
| `Recorded`/`Verified` | Koreksi vital | `Corrected` atau record koreksi target | Permission update owner | Histori lama wajib dipertahankan | Overwrite nilai final ditolak. |
| Status aktif | Cancel | `Cancelled` | Permission update owner | Alasan; review dampak keselamatan | Jangan hilangkan alert tanpa jejak. |
| Status resmi | Delete | Tidak sah untuk target RM | — | Hanya correction/entered-in-error | Tolak delete dan audit percobaan. |

## Dokumen dan consent

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Upload/create | `Uploaded` atau `Draft` | Permission create owner | Patient/konteks/file valid | Tolak. |
| `Uploaded` | Verify | `Verified` | Permission update owner | Verifier existing | Tolak bila record tidak ditemukan/status salah. |
| `Verified` | Approve | `Approved` | Permission update owner | Approver existing | Jangan artikan sebagai approval policy RM. |
| `Draft`/`PendingSignature` | Sign consent | `Signed` | Permission update owner | Signer dan isian consent valid | Tolak bila evidence tidak lengkap. |
| State yang diizinkan owner | Reject/withdraw/cancel/archive | State tujuan owner | Permission update owner | Alasan sesuai request | Histori dan audit tetap ada. |
| `Signed`/`Approved` | PUT/DELETE isi lama | Tidak sah untuk target RM | — | Correction/entered-in-error | Tolak overwrite/soft-delete. |

## CPPT

CPPT existing tidak mempunyai enum lifecycle draft/signed. `IsReadOnlyGenerated` hanya melindungi
catatan hasil generator tertentu. Sampai extension finality tersedia, operasi `PUT`, `cancel`, dan
`DELETE` tidak boleh dianggap aman untuk record resmi bertanda tangan.

**Contoh:** tindakan `Completed` kemudian dinyatakan salah. Owner boleh menerbitkan koreksi, tetapi
item checklist yang pernah muncul tidak boleh hilang otomatis; Unit RM dan pejabat klinis harus
mereview pengeluarannya dengan alasan dan histori.
