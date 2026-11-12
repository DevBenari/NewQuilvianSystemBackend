# Remunerasi dan Jasa — Blueprint Manifest (sub-modul)

```yaml
blueprint_id: PRL-BP-001
submodule_slug: remunerasi-jasa
submodule_prefix: REM
status: BLOCKED
blocked_by:
  - "DEC-PRL-011: amandemen Medical Fee (MF-BP-001) agar jasa membawa unit atau cost center, kontrak Medical Fee ke Remunerasi (MF-CQ-06), dan apakah unit = Cost Center"
  - "DEC-PRL-012: rumus alokasi pool ke unit, penilaian tahunan mana untuk periode distribusi, definisi indeks dan bobot jabatan, kasus tepi"
  - "DEC-PRL-013: pejabat penyetuju Persetujuan Remunerasi ditetapkan Direksi"
  - "DEC-PRL-014: registry dan prefix untuk entity skema remunerasi, pool, dan distribusi"
contract_versions: {}
artifact_hashes: {}
approved_by: null
approved_at: null
decisions_already_made: [PRL-DEC-22, PRL-DEC-23, PRL-DEC-24, PRL-DEC-26, PRL-DEC-29, PRL-DEC-30, PRL-DEC-37, PRL-DEC-40, PRL-DEC-43, PRL-DEC-45, PRL-DEC-47]
capabilities: [R-01, R-02, R-03, R-04, R-05, R-06, R-07, R-08, P-24]
next_step: "grill-me Closure Pass untuk DEC-PRL-011 dan DEC-PRL-012 setelah Medical Fee diamandemen"
```

Sub-modul ini lahir berstatus `BLOCKED`, keadaan yang sah menurut aturan bentuk blueprint
bagian 6: batas kepemilikan data dan rumus intinya belum diputuskan. Keputusan yang sudah
diambil pemilik tetap berlaku dan tercatat di `../00-interview-decisions.md`.
