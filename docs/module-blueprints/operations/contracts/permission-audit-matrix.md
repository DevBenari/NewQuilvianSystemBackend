# Permission dan Audit Matrix — Modul Operasi

Contract `opr-permission-v1`; status `approved`; approved by pemilik kebutuhan pada 2026-08-21. String berikut adalah target persis.

| Kelompok endpoint | Resource | Action | Atribut | Custom logger |
|---|---|---|---|:---:|
| GET kasus/jadwal/report | `OperatingRoomCase` | `Read` | `[AccessPermission("OperatingRoomCase", "Read")]` | Tidak |
| POST case | `OperatingRoomCase` | `Create` | `[AccessPermission("OperatingRoomCase", "Create")]` | Ya |
| schedule/team/postpone | `OperatingRoomSchedule` | `Update` | `[AccessPermission("OperatingRoomSchedule", "Update")]` | Ya |
| checklist/readiness | `OperatingRoomPreparation` | `Update` | `[AccessPermission("OperatingRoomPreparation", "Update")]` | Ya |
| start/execution/addendum | `OperatingRoomExecution` | `Update` | `[AccessPermission("OperatingRoomExecution", "Update")]` | Ya |
| anesthesia/recovery | `OperatingRoomAnesthesia` | `Update` | `[AccessPermission("OperatingRoomAnesthesia", "Update")]` | Ya |
| material/implant | `OperatingRoomMaterial` | `Update` | `[AccessPermission("OperatingRoomMaterial", "Update")]` | Ya |
| handover | `OperatingRoomHandover` | `Update` | `[AccessPermission("OperatingRoomHandover", "Update")]` | Ya |
| cancel | `OperatingRoomCase` | `Cancel` | `[AccessPermission("OperatingRoomCase", "Cancel")]` | Ya |
| reconciliation | `OperatingRoomIntegration` | `Update` | `[AccessPermission("OperatingRoomIntegration", "Update")]` | Ya |

Permission adalah gerbang pertama; service tetap memeriksa aktor bisnis. Logger hanya mencatat entity ID, action, hasil, actor, timestamp, alasan/kode aman, dan correlation ID. Logger dilarang memuat diagnosis, catatan operasi/anestesi, komplikasi, recovery, atau isi handover.
