# Matriks Izin dan Audit — CRM Membership

| Field | Nilai |
|---|---|
| Contract version | `v1`, status `approved` (Yoga Aji Pratama, 2026-10-13) |
| Owner | Yoga Aji Pratama |
| Masukan | `CRM-DEC-011`, `018`, `03-domain-architecture.md` H dan I |

Izin diberikan per departemen-posisi lewat Role Access (`SysAccessPolicies`). Penetapan
departemen-posisi untuk petugas CRM/Marketing adalah konfigurasi rumah sakit (`CRM-GAP-011`).

Base URL: `api/v1/health-services/patient-management/master-data/patient-memberships`

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger | Dicatat riwayat status |
|---|---|---|---|:---:|:---:|
| `POST /admin` | `PatientMembership` | `Create` | `[AccessPermission("PatientMembership", "Create")]` | Ya | Ya (`REGISTERED`) |
| `POST /admin/{id}/renewals` | `PatientMembership` | `Create` | `[AccessPermission("PatientMembership", "Create")]` | Ya | Ya (`REGISTERED`) |
| `POST /admin/{id}/upgrades` | `PatientMembership` | `Create` | `[AccessPermission("PatientMembership", "Create")]` | Ya | Ya (`REGISTERED`) |
| `POST /admin/{id}/cancel` | `PatientMembership` | `Cancel` | `[AccessPermission("PatientMembership", "Cancel")]` | Ya | Ya (`CANCELLED`) |
| `POST /admin/{id}/verify` | `PatientMembership` | `Verify` | `[AccessPermission("PatientMembership", "Verify")]` | Ya | Ya (`LEGACY_ACCEPTED`/`LEGACY_REJECTED`) |
| `PATCH /admin/{id}/notes` | `PatientMembership` | `Update` | `[AccessPermission("PatientMembership", "Update")]` | Ya | Ya (`NOTES_UPDATED`) |
| `GET /admin`, `/admin/{id}`, `/admin/summary`, `/admin/options`, `/admin/filters/metadata` | `PatientMembership` | `Read` | `[AccessPermission("PatientMembership", "Read")]` | Tidak | Tidak |
| `GET /admin/{id}/status-histories` | `PatientMembership` | `Read` | `[AccessPermission("PatientMembership", "Read")]` | Tidak | Tidak |
| `GET /admin/patient-lookup` | `PatientMembership` | `Read` | `[AccessPermission("PatientMembership", "Read")]` | Tidak | Tidak |
| `GET /admin/patients/{patientId}/active` | `PatientMembership` | `Read` | `[AccessPermission("PatientMembership", "Read")]` | Tidak | Tidak |
| `GET /`, `/kiosk`, `/options`, `/kiosk/options`, metadata | — | — | `[Authorize(Policy = "KioskRead")]` (tetap) | Tidak | Tidak |
| Proses terjadwal (tanpa endpoint) | — | — | — | Ya (ringkasan per putaran) | Ya (`EXPIRED`, `SCHEDULE_STARTED`, `UNPAID_CANCELLED`, sumber `System`) |
| `ConfirmFeePaidAsync` (tanpa endpoint, S2) | — | — | Dipanggil dari dalam billing | Ya | Ya (`FEE_PAID`, sumber `Billing`) |

`Cancel` dan `Verify` adalah aksi khusus, seperti `LabOrder : Confirm` dan `Journal : Approve`
yang sudah ada. Keduanya wajib dideklarasikan dengan `[AccessAction]` agar muncul di Role Access.

Pemegang izin per peran (usulan, isiannya konfigurasi rumah sakit):

| Peran | Read | Create | Update | Cancel | Verify |
|---|:---:|:---:|:---:|:---:|:---:|
| Petugas CRM/Marketing | Ya | Ya | Ya | Ya | Ya |
| Admisi RJ/IGD | Ya (lookup dan status aktif saja) | Tidak | Tidak | Tidak | Tidak |
| Kasir | Ya | Tidak | Tidak | Tidak | Tidak |
| Akun kiosk | Lewat policy `KioskRead` | Tidak | Tidak | Tidak | Tidak |

## Aturan logger

- Payload log hanya: `EntityId`, nama controller, action, status baru, dan aktor (`QBE-LOG-001`).
- **MUST NOT** memuat kolom sensitif: `CancellationReason`, `Notes`, `Reason`, `OldNotes`,
  `NewNotes`, `IdentityNumber`, `PhoneNumber`.
- Logger aplikasi tidak menyimpan objek data tambahan, sehingga isi yang wajib dibaca auditor
  disimpan di `PatMembershipStatusHistory`, bukan di log (`QBE-AUD-001`).
