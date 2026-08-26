# State Transition Contract — Modul Operasi

Contract `opr-state-v1`; status `approved`; owner/approved by pemilik kebutuhan pada 2026-08-21; input decision revision 5.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| - | Request | `Requested` | Dokter pemohon/bedah | Data minimum dan tindakan utama valid | `400`, jelaskan data yang kurang |
| `Requested` | Schedule | `Scheduled` | Koordinator | Ruang/waktu/tim lengkap tanpa benturan | `409`, tampilkan konflik |
| `Requested`/`Scheduled` | Postpone | `Postponed` | Koordinator + konfirmasi dokter | Alasan wajib | `422`, alasan wajib |
| `Postponed` | Reschedule | `Scheduled` | Koordinator | Jadwal baru valid | `409` bila bentrok |
| `Scheduled` | Complete readiness | `Ready` | Sistem | Tiga sign-off dan checklist/consent valid atau bypass sah | `422`, daftar prasyarat |
| `Ready` | Start | `In Progress` | Dokter bedah utama | Identitas/tindakan dikonfirmasi | `403/422` |
| `In Progress` | Complete case | `Completed` | Sistem | Execution final, recovery released, handover accepted | `422`, daftar yang belum selesai |
| `Requested`/`Scheduled`/`Ready` | Cancel | `Cancelled` | Dokter bedah/anestesi | Alasan klinis wajib | `422` |

Transition lain ilegal dan menghasilkan `409 InvalidStateTransition`. `Completed` serta `Cancelled` terminal. Penghentian setelah mulai disimpan sebagai outcome `StoppedEarly`, lalu kasus tetap menuju `Completed` setelah syarat keselamatan terpenuhi.
