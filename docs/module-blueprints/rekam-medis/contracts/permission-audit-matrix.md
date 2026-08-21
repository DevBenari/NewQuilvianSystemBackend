# Permission dan Audit Matrix — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-permission-v0.2-draft` |
| Status | `draft` |
| Approval | Belum tersedia — `RM-APR-002` |
| Snapshot | Backend `5103e68` |
| Compatibility | Permission existing tidak diubah; gap contextual authorization dan audit dicatat |

String di bawah disalin dari atribut controller existing. Source tidak memperlihatkan custom
activity logger pada sembilan controller ini. Karena itu kolom audit mutation diberi `Gap`, bukan
dianggap otomatis tercatat.

| Endpoint/pola | Resource | Action | String existing | Custom logger as-is | Keputusan RM |
| --- | --- | --- | --- | :---: | --- |
| Semua `GET patient-assessments...` | `PatientAssessment` | `Read` | `[AccessPermission("PatientAssessment", "Read")]` | Tidak | Tambah assignment check pada extension. |
| `POST patient-assessments` | `PatientAssessment` | `Create` | `[AccessPermission("PatientAssessment", "Create")]` | Gap | Audit metadata mutation wajib. |
| `PUT/PATCH patient-assessments...` | `PatientAssessment` | `Update` | `[AccessPermission("PatientAssessment", "Update")]` | Gap | Guard finality + audit. |
| Semua `GET doctor-consultations...` | `DoctorConsultation` | `Read` | `[AccessPermission("DoctorConsultation", "Read")]` | Tidak | Assignment check wajib. |
| `POST doctor-consultations` | `DoctorConsultation` | `Create` | `[AccessPermission("DoctorConsultation", "Create")]` | Gap | Audit create. |
| `PUT/PATCH doctor-consultations...` | `DoctorConsultation` | `Update` | `[AccessPermission("DoctorConsultation", "Update")]` | Gap | Audit SOAP/finalisasi/cancel. |
| Semua `GET patient-diagnoses...` | `PatientDiagnosis` | `Read` | `[AccessPermission("PatientDiagnosis", "Read")]` | Tidak | Assignment check wajib. |
| `POST patient-diagnoses` | `PatientDiagnosis` | `Create` | `[AccessPermission("PatientDiagnosis", "Create")]` | Gap | Audit create tanpa isi diagnosis. |
| `PUT/PATCH patient-diagnoses...` | `PatientDiagnosis` | `Update` | `[AccessPermission("PatientDiagnosis", "Update")]` | Gap | Audit set-primary/resolve/cancel. |
| Semua `GET patient-procedures...` | `PatientProcedure` | `Read` | `[AccessPermission("PatientProcedure", "Read")]` | Tidak | Assignment check wajib. |
| `POST patient-procedures...` | `PatientProcedure` | `Create` | `[AccessPermission("PatientProcedure", "Create")]` | Gap | Audit create/select. |
| `PUT/PATCH patient-procedures...` | `PatientProcedure` | `Update` | `[AccessPermission("PatientProcedure", "Update")]` | Gap | Audit approve/execute/cancel. |
| Semua `GET patient-allergies...` | `PatientAllergy` | `Read` | `[AccessPermission("PatientAllergy", "Read")]` | Tidak | Alergi safety view tetap contextual. |
| `POST patient-allergies` | `PatientAllergy` | `Create` | `[AccessPermission("PatientAllergy", "Create")]` | Gap | Audit create. |
| `PUT/PATCH patient-allergies...` | `PatientAllergy` | `Update` | `[AccessPermission("PatientAllergy", "Update")]` | Gap | Audit verify/resolve/cancel. |
| `DELETE patient-allergies/{id}` | `PatientAllergy` | `Delete` | `[AccessPermission("PatientAllergy", "Delete")]` | Gap | Larang untuk fakta resmi; audit percobaan. |
| Semua `GET patient-vital-signs...` | `PatientVitalSign` | `Read` | `[AccessPermission("PatientVitalSign", "Read")]` | Tidak | Assignment check wajib. |
| `POST patient-vital-signs` | `PatientVitalSign` | `Create` | `[AccessPermission("PatientVitalSign", "Create")]` | Gap | Audit create. |
| `PUT/PATCH patient-vital-signs...` | `PatientVitalSign` | `Update` | `[AccessPermission("PatientVitalSign", "Update")]` | Gap | Audit verify/notify/cancel. |
| `DELETE patient-vital-signs/{id}` | `PatientVitalSign` | `Delete` | `[AccessPermission("PatientVitalSign", "Delete")]` | Gap | Larang untuk fakta resmi. |
| Semua `GET patient-integrated-progress-notes...` | `PatientIntegratedProgressNote` | `Read` | `[AccessPermission("PatientIntegratedProgressNote", "Read")]` | Tidak | Assignment check wajib. |
| `POST patient-integrated-progress-notes...` | `PatientIntegratedProgressNote` | `Create` | `[AccessPermission("PatientIntegratedProgressNote", "Create")]` | Gap | Audit create/provenance. |
| `PUT/PATCH patient-integrated-progress-notes...` | `PatientIntegratedProgressNote` | `Update` | `[AccessPermission("PatientIntegratedProgressNote", "Update")]` | Gap | Correction guard wajib. |
| `DELETE patient-integrated-progress-notes/{id}` | `PatientIntegratedProgressNote` | `Delete` | `[AccessPermission("PatientIntegratedProgressNote", "Delete")]` | Gap | Tidak boleh untuk signed. |
| Semua `GET patient-clinical-documents...` | `PatientClinicalDocument` | `Read` | `[AccessPermission("PatientClinicalDocument", "Read")]` | Tidak | Assignment + sensitive category check. |
| `POST patient-clinical-documents` | `PatientClinicalDocument` | `Create` | `[AccessPermission("PatientClinicalDocument", "Create")]` | Gap | Audit file hash/ID, bukan isi/path. |
| `PUT/PATCH patient-clinical-documents...` | `PatientClinicalDocument` | `Update` | `[AccessPermission("PatientClinicalDocument", "Update")]` | Gap | Pisahkan authority review/verify/approve kelak. |
| `DELETE patient-clinical-documents/{id}` | `PatientClinicalDocument` | `Delete` | `[AccessPermission("PatientClinicalDocument", "Delete")]` | Gap | Tidak boleh untuk signed/approved. |
| Semua `GET patient-consents...` | `PatientConsent` | `Read` | `[AccessPermission("PatientConsent", "Read")]` | Tidak | Assignment + privacy check. |
| `POST patient-consents` | `PatientConsent` | `Create` | `[AccessPermission("PatientConsent", "Create")]` | Gap | Audit create. |
| `PUT/PATCH patient-consents...` | `PatientConsent` | `Update` | `[AccessPermission("PatientConsent", "Update")]` | Gap | Audit sign/verify/approve/reject/withdraw/cancel. |
| `DELETE patient-consents/{id}` | `PatientConsent` | `Delete` | `[AccessPermission("PatientConsent", "Delete")]` | Gap | Tidak boleh untuk signed. |

## Contextual authorization yang belum tersedia

Permission resource/action adalah syarat pertama. Target RM juga memerlukan penugasan formal aktif,
kecocokan patient–encounter, kewenangan profesi, dan pembatasan data sensitif. Selama extension itu
belum ada, permission existing tidak boleh dipakai sebagai bukti akses RM final.

## Isi audit minimum untuk mutation extension

Audit menyimpan actor/user ID, profesi/peran, patient dan encounter reference yang dipseudonimkan
sesuai policy, owner record ID, action, waktu, outcome, correlation ID, idempotency key, serta alasan
bila diwajibkan. Audit tidak menyimpan nama pasien, MRN, SOAP, diagnosis, nilai vital, isi dokumen,
path file, identitas signer, atau payload klinis.

**Contoh:** audit finalisasi mencatat `consultationId`, actor, outcome, dan correlation ID. Audit
tidak menyalin teks `Subjective` atau diagnosis pasien.
