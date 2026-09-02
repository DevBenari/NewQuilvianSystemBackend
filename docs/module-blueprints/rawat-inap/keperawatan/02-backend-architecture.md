# Arsitektur Backend — Sub-modul `keperawatan` (Rawat Inap)

| Field | Value |
|---|---|
| Sub-modul | `keperawatan` — Keperawatan Rawat Inap, bagian modul `rawat-inap` |
| Revision | `0.0` |
| Status | `draft` — **belum dirancang** |
| Tanggal | 2026-09-02 |
| Manifest | [`blueprint-manifest.md`](./blueprint-manifest.md) |
| Peta modul | [`02-module-map.md`](./../02-module-map.md) |

## Alasan berkas ini belum berisi

Bounded context, class diagram, arsitektur folder, status model, dan rencana migration belum dapat ditulis karena sub-modul ini belum dirancang — dan `RWI-DEC-081` sudah memastikan isinya kelak **nol tabel baru**, sebab seluruh tabel dokumentasi klinis dimiliki modul lain.

Sub-modul ini berstatus `draft` dan **belum dirancang**, sehingga berkas ini belum berisi. Sebabnya bukan keputusan yang menggantung: kepemilikan tabelnya sudah diputuskan `RWI-DEC-081` (milik `ClinicalManagement`) dan persetujuan pemiliknya sudah diberikan `RWI-DEC-062` — yang tersisa adalah pekerjaan desain yang belum dikerjakan, ditambah satu penghalang teknis *shared inpatient clinical context resolver* pada `PRD-RWI-FINAL-001` bagian 30.3.

Berkas ini sengaja **dibuat kosong berisi alasan**, bukan dihapus, supaya pembaca berikutnya dapat
membedakan "memang belum perlu ditulis" dari "terlupa ditulis" —
`blueprint-output-contract.md` bagian "File yang tidak relevan bagi modul tertentu".
