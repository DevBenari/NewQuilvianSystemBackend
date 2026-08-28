# Template Laporan Task Claude

Pakai struktur ini untuk laporan task tracked di `docs/module-blueprints/<module-slug>/task/report/backend/<TASK-ID>.md`. Catat bukti saja; jangan menuliskan nilai rahasia.

> **Jangan tertukar dengan laporan task modul.** Template ini mengatur handoff sesi yang
> **tidak** terlacak Git. Laporan task yang terlacak — yang ditunjuk roadmap dan
> `requirement-traceability.md` — punya jalur dan bentuknya sendiri di
> [rules/rule-output/lokasi-laporan-task.md](../rule-output/lokasi-laporan-task.md):
> `docs/module-blueprints/<module-slug>/task/report/backend/` atau `.../frontend/`. Keduanya
> berlaku bersamaan dan tidak saling menggantikan.

Nama field dan nilai statusnya sengaja dipertahankan dalam bahasa Inggris karena keduanya adalah kunci kontrak laporan yang dibaca lintas repository. Keterangan setiap field ada pada tabel di bawah.

```md
# [TASK ID] — [short title]

- TASK ID:
- TASK TYPE:
- COMPLEXITY:
- CLASSIFICATION SCORE:
- MODEL:
- TASK MODE:
- WRITE TARGET:
- FILES INSPECTED:
- FILES CHANGED:
- IMPLEMENTATION:
- BLUEPRINT STATUS/EVIDENCE: required for MODULE BLUEPRINT MODE; otherwise NOT APPLICABLE
- API CONTRACT IMPACT:
- DATABASE IMPACT:
- SECURITY IMPACT:
- VISUAL REFERENCE: NOT REQUIRED | REQUIRED | PROVIDED
- VALIDATION: command/check | result | classification | evidence/note
- WARNINGS:
- KNOWN ISSUES:
- STALE EVIDENCE / BLOCKED PHASES: required for MODULE BLUEPRINT MODE; otherwise NOT APPLICABLE
- MANUAL TEST: REQUIRED | PASS | FAIL | NOT FEASIBLE | NOT APPLICABLE
- INCIDENTAL CHANGES: NONE | restored/removed item and reason
- INTERRUPTIONS: NONE | interruption type and recovery performed
- GIT STATUS:
- NEXT RECOMMENDED STEP:
```

## Keterangan field

| Field | Yang harus diisi |
| --- | --- |
| `TASK ID` | Identifier task beserta judul pendeknya |
| `TASK TYPE` | Jenis pekerjaan yang dikerjakan |
| `COMPLEXITY` | Hasil klasifikasi: `LIGHT`, `MEDIUM`, `HEAVY`, atau `EPIC` |
| `CLASSIFICATION SCORE` | Total skor dari model penilaian di `TASK_CLASSIFICATION.md` |
| `MODEL` | Model Claude yang dipakai mengerjakan task, misalnya `Claude Sonnet 5` atau `Claude Opus 5` |
| `TASK MODE` | Mode task yang berlaku, misalnya `AUDIT` atau `MODULE BLUEPRINT` |
| `WRITE TARGET` | Repository dan jalur yang boleh ditulis |
| `FILES INSPECTED` | Berkas yang diperiksa |
| `FILES CHANGED` | Berkas yang benar-benar berubah |
| `IMPLEMENTATION` | Ringkasan perubahan yang dikerjakan |
| `BLUEPRINT STATUS/EVIDENCE` | Wajib pada `MODULE BLUEPRINT MODE`; selain itu isi `NOT APPLICABLE` |
| `API CONTRACT IMPACT` | Dampak terhadap kontrak API |
| `DATABASE IMPACT` | Dampak terhadap schema, entity, atau migration |
| `SECURITY IMPACT` | Dampak terhadap authorization, authentication, atau privasi |
| `VISUAL REFERENCE` | `NOT REQUIRED`, `REQUIRED`, atau `PROVIDED` |
| `VALIDATION` | Perintah/pemeriksaan, hasilnya, klasifikasinya, dan buktinya |
| `WARNINGS` | Peringatan yang muncul selama pengerjaan |
| `KNOWN ISSUES` | Masalah yang diketahui dan sengaja ditinggalkan |
| `STALE EVIDENCE / BLOCKED PHASES` | Wajib pada `MODULE BLUEPRINT MODE`; selain itu isi `NOT APPLICABLE` |
| `MANUAL TEST` | `REQUIRED`, `PASS`, `FAIL`, `NOT FEASIBLE`, atau `NOT APPLICABLE` |
| `INCIDENTAL CHANGES` | `NONE`, atau butir yang dipulihkan/dihapus beserta alasannya |
| `INTERRUPTIONS` | `NONE`, atau jenis interupsi beserta pemulihan yang dilakukan |
| `GIT STATUS` | Keluaran status Git di akhir pekerjaan |
| `NEXT RECOMMENDED STEP` | Langkah berikutnya yang disarankan |
