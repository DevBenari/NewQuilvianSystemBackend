# Requirement Traceability — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 3
wave: "MVP-0 berjalan; MVP-1..MVP-6 direncanakan"
status: ACTIVE
generated_at: "2026-08-24"
aligned_at: "2026-08-26 (correction pass revisi 6)"
input_revisions:
  blueprint-manifest.md: 6
  00-interview-decisions.md: "105 keputusan, sampai IGD-DEC-105"
  01-existing-capability-map.md: "3 + suplemen 3.1 (audit terarah EmergencyTransfer)"
  04-prd-to-mvp.md: 5
contract_versions:
  - "State 0.4.0 — bagian 1, 1.1, 1.2 APPROVED (IGD-DEC-093); bagian 6a baru"
  - "Validation 0.4.0 — bagian 2 aturan 4-5 APPROVED (IGD-DEC-093); bagian 2.1 dan 5 diperluas"
  - "API 0.4.0 — draft. BUKAN aditif: dua route revisi 5 diganti"
  - "Permission/Audit 0.4.0 — draft; bagian 3.1 baru"
  - "Integration 0.3.0 — draft, tidak dipakai"
source_commits:
  backend: "300922c — MVP-0 dikerjakan di atas commit ini"
  backend_at_authoring: "f69e9e483052845d11c91d8b7bbdce33c4acc8d8"
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
| `BE-IGD-018` **`SELESAI`** | `FR-IGD-015` (fondasi) | `Services/EmergencyVisitService.cs` +48 baris; test baru `HealthServices/EmergencyInstallationManagement/EmergencyVisitStatusTransitionTests.cs` | `AT-IGD-089` sebagian — **168 test lulus**, 81 sel matriks × 2 |
| `BE-IGD-019` **`SELESAI`** | `FR-IGD-013`, `FR-IGD-014`, `FR-IGD-015` | `EmergencyTriageController.cs` kedua titik tulis + injeksi penjaga + `409`; `EmergencyTriageService.cs` lubang `Completed`; test baru `EmergencyTriageVisitStatusTests.cs` | `AT-IGD-086`, `087`, `088` — **18 test lulus** |
| `BE-IGD-020` **`SELESAI`** | `FR-IGD-014` | `Services/EmergencyTriageService.cs` 146–148 — `Completed` ditambahkan | `AT-IGD-088` — **34 test lulus** |
| `BE-IGD-021` | `FR-IGD-015` | `EmergencyObservationController.cs` 277/279/283, `EmergencyResuscitationController.cs` 295, `EmergencyDispositionController.cs` 335 | `AT-IGD-089` |
| `BE-IGD-022` | `FR-IGD-015` | `EmergencyVisitController.cs` 433 | `AT-IGD-089` |
| `FE-IGD-012` | `FR-IGD-013`, `FR-IGD-014` sisi tampilan | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx` | `AT-IGD-086` sisi tampilan |

---

## 3. Satu task tanpa requirement, dan alasannya

`BE-IGD-017` tidak menelusuri ke functional requirement mana pun karena ia bukan pekerjaan
produk. Ia memulihkan solution yang rusak oleh merge `300922c`:

```
dotnet build ./QuilvianSystemBackend.sln --configuration Release
→ MSB5004: The solution file has two projects named "QuilvianSystemBackend.Tests".

dotnet test ./QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj
→ MSB4025: The project file could not be loaded. Name cannot begin with the
  '<' character  ← penanda konflik merge yang ter-commit
```

Tanpa task ini, **tidak satu pun** requirement di bagian 1 dapat dibuktikan, dan CI tidak
dapat hijau untuk perubahan apa pun dari siapa pun.

Ia dicantumkan dalam roadmap, bukan dikerjakan diam-diam, supaya jelas bahwa gelombang ini
bergantung pada dua berkas di luar `EmergencyInstallationManagement` yang **bukan milik IGD**,
dan butir 10 Definition of Done berlaku untuknya.

> **Catatan ketertelusuran.** Metadata `source_commits` pada ketiga berkas roadmap mencatat
> `f69e9e48` karena itulah commit saat roadmap disusun. Bukti cacat di atas diambil pada
> `300922c`. Bukti source `EPIC IGD-03` di bagian 2 — sembilan titik tulis `VisitStatus` —
> dikumpulkan pada `f69e9e48` dan **perlu diperiksa ulang** terhadap `300922c` sebelum
> `BE-IGD-018` dimulai, karena merge menyentuh `InPatientManagement` yang berbagi tabel
> kunjungan.

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

---

# Revision 3 — penelusuran perjalanan pasien penuh

Ditambahkan 26 Agustus 2026. Bagian di atas menelusuri `MVP-0` dan tetap berlaku.

## R3.1 Task baru dan requirement-nya

| Task | Requirement | Epic | Gelombang | Kontrak yang dibutuhkan | Penghalang tersisa |
| --- | --- | --- | --- | --- | --- |
| `BE-IGD-023` | `FR-IGD-001`…`004` | `IGD-01` | `MVP-1` | API §1.1, state — **`draft`** | Registration API owner belum ditunjuk; perilaku `Outpatient` lama belum diputuskan |
| `BE-IGD-024` | `FR-IGD-065`…`068` | `IGD-10` | `MVP-1` | API, validation — **`draft`** | `IGD-UNK` jumlah baris tanpa `EncounterId` |
| `BE-IGD-025` | `FR-IGD-005`…`012` | `IGD-02` | `MVP-2` | Validation §1, §1.1 — **`draft`** | — |
| `BE-IGD-026` | `FR-IGD-060` | `IGD-09` | `MVP-3` | **Belum ditulis** | Pemilik `ClinicalManagement` belum ditunjuk; otorisasi migration |
| `BE-IGD-027` | `FR-IGD-060`, `061` | `IGD-09` | `MVP-3` | **Belum ditulis** | Sama |
| `BE-IGD-028` | `FR-IGD-062` | `IGD-09` | `MVP-3` | **Belum ditulis** | Sama |
| `BE-IGD-029` | `FR-IGD-063` | `IGD-09` | `MVP-3` | **Belum ditulis** | Pemilik `PharmacyManagement` belum ditunjuk |
| `BE-IGD-030` | `FR-IGD-064` | `IGD-09` | `MVP-3` | — | — |
| `BE-IGD-031` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | API §2, state §2–4 — **`draft`** | Pemilik integrasi belum ditunjuk |
| `BE-IGD-032` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | State §2–4 — **`draft`** | — |
| `BE-IGD-033` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | State, validation §4 — **`draft`** | — |
| `BE-IGD-034` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | Validation §4, §4.1 — **`draft`** | — |
| `BE-IGD-035` | `FR-IGD-044`…`052` | `IGD-07` | `MVP-5` | Validation §5 — **`draft`** | **Cakupan "pesanan" belum pasti** sebelum penunjang medis punya blueprint |
| `FE-IGD-013` | `FR-IGD-060`…`064` | `IGD-09` | `MVP-3` | — | `BE-IGD-027`, `028`; kredensial petugas |
| `FE-IGD-014` | `FR-IGD-001`…`012` | `IGD-01`, `02` | `MVP-1`, `MVP-2` | — | `BE-IGD-023`, `025` |
| `FE-IGD-015` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | Rilis serentak `BE-IGD-031` |
| `FE-IGD-016` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | `BE-IGD-032` |
| `FE-IGD-017` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | — | `BE-IGD-033`, `034` |
| `FE-IGD-018` | — (kebersihan) | — | kapan saja | — | — |

## R3.2 Requirement yang **masih** tidak tertelusuri

`EPIC IGD-04` (`FR-IGD-016`…`022`, riwayat penugasan dokter) dan `EPIC IGD-08`
(`FR-IGD-053`…`059`, kewenangan unit) belum punya task pada revision `3`. Keduanya sudah punya
keputusan dan bentuk teknis, tetapi diletakkan pada `MVP-5` dan `MVP-6` yang belum diuraikan
menjadi task — menguraikannya sekarang menghasilkan task yang akan basi sebelum dikerjakan.

Diuraikan pada revisi roadmap berikutnya, setelah `MVP-3` selesai.

## R3.3 Tiga area tanpa requirement sama sekali

Penunjang medis, pemakaian alat, dan billing IGD **tidak punya satu pun `FR-IGD-*`**. Karena
itu tidak muncul di tabel mana pun di atas — bukan karena terlewat, melainkan karena belum ada
yang dapat ditelusuri.

| Area | Bukti source | Yang hilang |
| --- | --- | --- |
| Penunjang medis | `LabOrder`: `EncounterId`, `ProcedureId`. Empat endpoint. **Nol status, nol hasil, nol spesimen.** Radiologi nol berkas | Seluruh requirement |
| Pemakaian alat | **Nol berkas.** Folder `DeviceManagement` tidak ada | Seluruh requirement |
| Billing IGD | Seam `POST /folios/internal/milestones/recognize` matang dan idempoten. Nol pemanggil dari luar billing | Kejadian IGD mana yang layak tagih |

Menutup lubang ini adalah pekerjaan `/qv-grill`, bukan `/qv-plan`.
