# Roadmap Frontend — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `2` |
| `roadmap_revision` | `1` — dibuat 8 Oktober 2026. Isinya tidak berubah pada `blueprint_revision` `2`; keputusan review `PAT-OQ-001`–`007` tidak menambah pekerjaan frontend |
| Status roadmap | **`approved`** — tidak ada task frontend |
| Dasar | `PAT-DEC-015`: rekonsiliasi MRN Pilot adalah endpoint admin sementara tanpa layar |

Berkas ini sengaja dibuat walau kosong. Tanpanya, pembaca tidak dapat membedakan antara "memang
tidak ada pekerjaan frontend" dan "roadmap frontend terlupa ditulis".

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
(tidak ada task frontend pada roadmap_revision 1)
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | — | Tidak ada |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| — | Tidak ada task frontend | `PAT-DEC-015` | — | — | — | — | — | — | — | — |

Bila kelak pemilik memutuskan rekonsiliasi atau kemampuan pasien lain butuh layar, task
frontendnya ditambahkan di sini lewat skill perencanaan, dengan keputusan baru yang tercatat.
`BE-PAT-MIG-001` tidak memberi wewenang membuat layar apa pun.
