# Requirement Traceability — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 2
wave: MVP-0
status: DRAFT
generated_at: "2026-08-24"
input_revisions:
  blueprint-manifest.md: 5
  00-interview-decisions.md: "91 keputusan, sampai IGD-DEC-093"
  01-existing-capability-map.md: 3
  04-prd-to-mvp.md: 5
contract_versions:
  - "State 0.3.0 — bagian 1, 1.1, 1.2 APPROVED"
  - "Validation 0.3.0 — bagian 2 aturan 4-5 APPROVED"
  - "API 0.3.0 — draft, tidak dipakai"
  - "Integration 0.3.0 — draft, tidak dipakai"
  - "Permission/Audit 0.3.0 — draft, tidak dipakai"
source_commits:
  backend: "f69e9e483052845d11c91d8b7bbdce33c4acc8d8"
  frontend: "96a9120111f6acc6b7c0f37973ea0c717ba41f17"
supersedes: "roadmap/archive/revision-1/requirement-traceability.md"
```

Dokumen ini menelusuri **hanya** gelombang `MVP-0`. Penelusuran task `BE-IGD-001`…`016` dan
`FE-IGD-001`…`011` ada di `roadmap/archive/revision-1/requirement-traceability.md`.

---

## 1. Rantai penuh: requirement → keputusan → kontrak → task → uji

| Requirement | Isi | Keputusan asal | Kontrak (bagian, status) | Task | Uji |
| --- | --- | --- | --- | --- | --- |
| `FR-IGD-013` | Penilaian ulang tidak mengembalikan status kunjungan ke `Triaged` | `IGD-GAP-014` | State `0.3.0` §1.1 baris 2 — **approved** | `BE-IGD-019`, `FE-IGD-012` | `AT-IGD-086` |
| `FR-IGD-014` | Triase tidak dapat diselesaikan pada kunjungan yang sudah tertutup | `IGD-GAP-014` | Validation `0.3.0` §2 aturan 4 — **approved** | `BE-IGD-019`, `BE-IGD-020`, `FE-IGD-012` | `AT-IGD-087`, `AT-IGD-088` |
| `FR-IGD-015` | Seluruh penulisan status kunjungan melewati pemeriksaan transisi | `IGD-CONF-05` | State `0.3.0` §1, §1.2 — **approved**; Validation `0.3.0` §2 aturan 5 — **approved** | `BE-IGD-018`, `BE-IGD-019`, `BE-IGD-021`, `BE-IGD-022` | `AT-IGD-089` |

Tiga functional requirement, seluruhnya milik `EPIC IGD-03`. **Tidak ada** requirement
gelombang ini yang tidak punya task, dan tidak ada task gelombang ini yang tidak punya
requirement — kecuali `BE-IGD-017`, lihat bagian 3.

---

## 2. Penelusuran terbalik: task → requirement

| Task | Requirement | Berkas yang disentuh | Uji |
| --- | --- | --- | --- |
| `BE-IGD-017` | **Tidak ada** — perbaikan build, lihat bagian 3 | `Program.cs` | `dotnet build` |
| `BE-IGD-018` | `FR-IGD-015` (fondasi) | `Services/EmergencyVisitService.cs` | `AT-IGD-089` sebagian |
| `BE-IGD-019` | `FR-IGD-013`, `FR-IGD-014`, `FR-IGD-015` | `Controllers/EmergencyTriageController.cs` 250, 356 | `AT-IGD-086`, `087`, `088` |
| `BE-IGD-020` | `FR-IGD-014` | `Services/EmergencyTriageService.cs` 141–143 | `AT-IGD-088` |
| `BE-IGD-021` | `FR-IGD-015` | `EmergencyObservationController.cs` 277/279/283, `EmergencyResuscitationController.cs` 295, `EmergencyDispositionController.cs` 335 | `AT-IGD-089` |
| `BE-IGD-022` | `FR-IGD-015` | `EmergencyVisitController.cs` 433 | `AT-IGD-089` |
| `FE-IGD-012` | `FR-IGD-013`, `FR-IGD-014` sisi tampilan | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx` | `AT-IGD-086` sisi tampilan |

---

## 3. Satu task tanpa requirement, dan alasannya

`BE-IGD-017` tidak menelusuri ke functional requirement mana pun karena ia bukan pekerjaan
produk. Ia memulihkan kompilasi yang rusak pada commit `f69e9e48`:

```
Program.cs(273,32): error CS0246: The type or namespace name 'LabOrderService'
could not be found
```

Tanpa task ini, **tidak satu pun** requirement di bagian 1 dapat dibuktikan — `dotnet build`
dan `dotnet test` sama-sama gagal, sehingga seluruh `AT-IGD-*` tidak dapat dijalankan.

Ia dicantumkan dalam roadmap, bukan dikerjakan diam-diam, supaya jelas bahwa gelombang ini
menyentuh satu berkas di luar `EmergencyInstallationManagement` dan butir 10 Definition of
Done berlaku untuknya.

---

## 4. Keputusan yang dipakai gelombang ini

| Keputusan | Status | Dipakai oleh |
| --- | --- | --- |
| `IGD-GAP-014` | Gap tercatat | `FR-IGD-013`, `FR-IGD-014` |
| `IGD-CONF-05` | Konflik tercatat | `FR-IGD-015` |
| `IGD-DEC-089` | **`approved`** — penetapan Product/Domain Owner | Kewenangan approval `IGD-DEC-093` |
| `IGD-DEC-093` | **`approved`** — approval kontrak sempit | Seluruh gelombang |

Keputusan `IGD-DEC-090`, `091`, dan `092` yang lahir 24 Agustus 2026 **tidak** dipakai
gelombang ini. Ketiganya menyangkut `EPIC IGD-05`, `06`, dan `08` pada gelombang berikutnya.

---

## 5. Requirement yang **tidak** ditelusuri gelombang ini

Enam puluh lima dari enam puluh delapan functional requirement `04-prd-to-mvp.md` berada di
luar `MVP-0`.

| Epic | Requirement | Gelombang | Penghalang tersisa |
| --- | --- | --- | --- |
| `EPIC IGD-01` | `FR-IGD-001`…`004` | `MVP-1` | Kontrak `draft`; master kelas pasien belum terisi |
| `EPIC IGD-02` | `FR-IGD-005`…`012` | `MVP-1` | Kontrak `draft`; Registration API owner belum ditunjuk |
| `EPIC IGD-04` | `FR-IGD-016`…`022` | `MVP-2` | Kontrak `draft` |
| `EPIC IGD-05` | `FR-IGD-023`…`035` | `MVP-3` | Kontrak `draft`; `IGD-UNK-03` |
| `EPIC IGD-06` | `FR-IGD-036`…`043` | `MVP-3` | Kontrak `draft` |
| `EPIC IGD-07` | `FR-IGD-044`…`052` | `MVP-4` | Kontrak `draft` |
| `EPIC IGD-08` | `FR-IGD-053`…`059` | `MVP-5` | Kontrak `draft`; data pemetaan belum terisi; pengesahan Security/Privacy owner |
| `EPIC IGD-09` | `FR-IGD-060`…`064` | `POST-MVP` | Pemilik `ClinicalManagement` dan `PharmacyManagement` belum ditunjuk |
| `EPIC IGD-10` | `FR-IGD-065`…`068` | `MVP-2` | Kontrak `draft` |

Penghalang berupa "kontrak `draft`" seluruhnya dapat dicabut Rizki Gunawan sendiri untuk
bagian yang tabelnya milik IGD, dengan cara yang sama seperti `IGD-DEC-093`. Penghalang berupa
penunjukan pemilik **tidak** dapat.

---

## 6. Lubang yang diketahui

| Lubang | Akibat |
| --- | --- |
| `AT-IGD-089` menuntut pembuktian *"tidak ada penulisan langsung"* pada seluruh jalur | Test biasa tidak dapat membuktikan ketiadaan. Pembuktiannya berupa penelusuran kode yang dilampirkan pada laporan `BE-IGD-021`, ditambah test perilaku per jalur |
| Alur simpan lewat layar belum pernah dijalankan sungguhan | Butuh kredensial petugas. Berlaku sejak revision `1` dan belum terselesaikan |
| Gerbang kemampuan rumah sakit belum terpenuhi | `evidence/02-requirement-completeness-gate.md` dan `evidence/03-hospital-domain-architecture.md` tidak ada. Tidak memblokir gelombang ini, tetapi berarti `MVP-0` tidak punya klasifikasi kesiapan requirement |
