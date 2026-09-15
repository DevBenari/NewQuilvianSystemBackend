# Requirement Traceability — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 3
wave: "Dikoreksi 2026-09-15: MVP-1, MVP-2, R3.7 selesai; MVP-0, MVP-3, MVP-4, MVP-5 sebagian; MVP-6 terblokir BE-IGD-039"
status: ACTIVE
status_synced_at: "2026-09-15 — backend e89907c5, frontend 43adae648; IGD-DEC-110 sampai IGD-DEC-115"
planning_updated_at: "2026-09-15 (kedua) — BE-IGD-040..045, FE-IGD-019 (kartu susulan), FE-IGD-023..027; IGD-DEC-116 sampai IGD-DEC-121. Lihat bagian R3.4"
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

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

> **Penandaan 15 September 2026.** Tanda diturunkan dari pemetaan ulang acceptance criteria ke
> source, dengan rincian di
> [evidence/2026-09-15-pemeriksaan-status.md](../evidence/2026-09-15-pemeriksaan-status.md)
> bagian 8.2. Tanda di sini sama dengan tanda pada `backend-roadmap.md` dan
> `frontend-roadmap.md`.

---

## 1. Rantai penuh: requirement → keputusan → kontrak → task → uji

| Requirement | Isi | Keputusan asal | Kontrak (bagian, status) | Task | Uji | Status bukti 15 Sep 2026 |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-IGD-013` | Penilaian ulang tidak mengembalikan status kunjungan ke `Triaged` | `IGD-GAP-014` | State `0.3.0` §1.1 baris 2 — **approved** | `BE-IGD-019` ✅, `FE-IGD-012` 🟡 | `AT-IGD-086` | 🟡 backend terbukti; bukti layar `FE-IGD-012` belum ada |
| `FR-IGD-014` | Triase tidak dapat diselesaikan pada kunjungan yang sudah tertutup | `IGD-GAP-014` | Validation `0.3.0` §2 aturan 4 — **approved** | `BE-IGD-019` ✅, `BE-IGD-020` ✅, `FE-IGD-012` 🟡 | `AT-IGD-087`, `AT-IGD-088` | 🟡 backend terbukti; bukti layar `FE-IGD-012` belum ada |
| `FR-IGD-015` | Seluruh penulisan status kunjungan melewati pemeriksaan transisi | `IGD-CONF-05` | State `0.3.0` §1, §1.2 — **approved**; Validation `0.3.0` §2 aturan 5 — **approved** | `BE-IGD-018` ✅, `BE-IGD-019` ✅, `BE-IGD-021` ✅, `BE-IGD-022` ✅ | `AT-IGD-089` | ✅ seluruh task backend-nya ✅. Test `AT-IGD-089` terhapus 11 Sep 2026; daftar periksa manual penggantinya wajib dibuat (`IGD-DEC-110`) |

Tiga functional requirement, seluruhnya milik `EPIC IGD-03`. **Tidak ada** requirement
gelombang ini yang tidak punya task, dan tidak ada task gelombang ini yang tidak punya
requirement — kecuali `BE-IGD-017`, lihat bagian 3.

---

## 2. Penelusuran terbalik: task → requirement

| Task | Requirement | Berkas yang disentuh | Uji |
| --- | --- | --- | --- |
| `BE-IGD-017` 🟡 — laporan tracked tidak ada | **Tidak ada** — perbaikan build, lihat bagian 3 | `Program.cs` | `dotnet build` |
| `BE-IGD-018` ✅ [laporan](../task/report/backend/be-igd-018-penjaga-transisi-status-kunjungan.md) | `FR-IGD-015` (fondasi) | `Services/EmergencyVisitService.cs` +48 baris; test baru `HealthServices/EmergencyInstallationManagement/EmergencyVisitStatusTransitionTests.cs` | `AT-IGD-089` sebagian — **168 test lulus**, 81 sel matriks × 2 |
| `BE-IGD-019` ✅ [laporan](../task/report/backend/be-igd-019-jalur-triase-tidak-memundurkan-status.md) | `FR-IGD-013`, `FR-IGD-014`, `FR-IGD-015` | `EmergencyTriageController.cs` kedua titik tulis + injeksi penjaga + `409`; `EmergencyTriageService.cs` lubang `Completed`; test baru `EmergencyTriageVisitStatusTests.cs` | `AT-IGD-086`, `087`, `088` — **18 test lulus** |
| `BE-IGD-020` ✅ [laporan](../task/report/backend/be-igd-020-penilaian-ulang-kunjungan-tertutup.md) | `FR-IGD-014` | `Services/EmergencyTriageService.cs` 146–148 — `Completed` ditambahkan | `AT-IGD-088` — **34 test lulus** |
| `BE-IGD-021` ✅ [laporan gabungan](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) | `FR-IGD-015` | `EmergencyObservationController.cs` 277/279/283, `EmergencyResuscitationController.cs` 295, `EmergencyDispositionController.cs` 335 | `AT-IGD-089` |
| `BE-IGD-022` ✅ [laporan gabungan](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) | `FR-IGD-015` | `EmergencyVisitController.cs` 433 | `AT-IGD-089` |
| `FE-IGD-012` 🟡 [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) | `FR-IGD-013`, `FR-IGD-014` sisi tampilan | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx` | `AT-IGD-086` sisi tampilan |

Nomor baris pada kolom "Berkas yang disentuh" adalah keadaan saat task direncanakan. Letak
source sekarang (`e89907c5`) ada di kartu status masing-masing task pada `backend-roadmap.md`.

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

| Task | Requirement | Epic | Gelombang | Kontrak yang dibutuhkan | Penghalang tersisa (26 Agt) | Status 15 Sep 2026 | Laporan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-IGD-023` | `FR-IGD-001`…`004` | `IGD-01` | `MVP-1` | API §1.1, state — **`draft`** | Registration API owner belum ditunjuk; perilaku `Outpatient` lama belum diputuskan | ✅ — tindak lanjut `IGD-EV-121` | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-024` | `FR-IGD-065`…`068` | `IGD-10` | `MVP-1` | API, validation — **`draft`** | `IGD-UNK` jumlah baris tanpa `EncounterId` | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-025` | `FR-IGD-005`…`012` | `IGD-02` | `MVP-2` | Validation §1, §1.1 — **`draft`** | — | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-026` | `FR-IGD-060` | `IGD-09` | `MVP-3` | **Belum ditulis** | Pemilik `ClinicalManagement` belum ditunjuk; otorisasi migration | 🟡 uji langkah mundur migration belum | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-027` | `FR-IGD-060`, `061` | `IGD-09` | `MVP-3` | **Belum ditulis** | Sama | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-028` | `FR-IGD-062` | `IGD-09` | `MVP-3` | **Belum ditulis** | Sama | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-029` | `FR-IGD-063` | `IGD-09` | `MVP-3` | **Belum ditulis** | Pemilik `PharmacyManagement` belum ditunjuk | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-030` | `FR-IGD-064` | `IGD-09` | `MVP-3` | — | — | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-031` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | API §2, state §2–4 — **`draft`** | Pemilik integrasi belum ditunjuk | 🟡 uji `RENAME` balik belum | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-032` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | State §2–4 — **`draft`** | — | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-033` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | State, validation §4 — **`draft`** | — | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-034` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | Validation §4, §4.1 — **`draft`** | — | ✅ | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `BE-IGD-035` | `FR-IGD-044`…`052` | `IGD-07` | `MVP-5` | Validation §5 — **`draft`** | **Cakupan "pesanan" belum pasti** sebelum penunjang medis punya blueprint | 🟡 kriteria 2 (`IGD-EV-122`); kewenangan unit tujuan (`BE-IGD-039`) | [be-igd-021-035](../task/report/backend/be-igd-021-035-penyelesaian-perjalanan-pasien.md) |
| `FE-IGD-013` | `FR-IGD-060`…`064` | `IGD-09` | `MVP-3` | — | `BE-IGD-027`, `028`; kredensial petugas | 🟡 uji lewat layar belum | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-014` | `FR-IGD-001`…`012` | `IGD-01`, `02` | `MVP-1`, `MVP-2` | — | `BE-IGD-023`, `025` | 🟡 kriteria 2 belum ada (`IGD-EV-123`) | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-015` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | Rilis serentak `BE-IGD-031` | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-016` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | `BE-IGD-032` | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-017` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | — | `BE-IGD-033`, `034` | 🟡 pelaku tampil sebagai ID (`IGD-EV-123`) | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-018` | — (kebersihan) | — | kapan saja | — | — | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |

> **Catatan 15 September 2026.** Kolom "Kontrak yang dibutuhkan" dan "Penghalang tersisa" adalah
> keadaan 26 Agustus dan disimpan sebagai riwayat. Sejak 27 Agustus 2026, `IGD-DEC-108`
> menaikkan seluruh irisan kontrak itu menjadi `approved`, dan `IGD-DEC-107` membuat Product/Domain
> Owner IGD menggantikan sementara pemilik modul yang belum ditunjuk. Perilaku `Outpatient`
> diputuskan `IGD-DEC-109`.
>
> Task yang lahir sesudah tabel ini disusun:
>
> | Task | Requirement | Status | Laporan |
> | --- | --- | --- | --- |
> | `BE-IGD-036` | `FR-IGD-060` (jalur simpan terbukti) | ✅ | [be-igd-036-039](../task/report/backend/be-igd-036-039-penerapan-migration-pemindahan-master-dan-audit-kesiapan.md) |
> | `BE-IGD-037` | — (struktur modul) | ✅ | Sama |
> | `BE-IGD-038` | `FR-IGD-060`, `FR-IGD-064` (kolom respons) | ✅ | Sama |
> | `BE-IGD-039` | `FR-IGD-053`…`059` | ⛔ Security/Privacy owner | Sama |
> | `FE-IGD-019` | `FR-IGD-060` | ✅ — kartu susulan ditambahkan 15 September 2026 (lihat R3.4) | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
> | `FE-IGD-020` | — (route master) | ✅ | [fe-igd-020-021](../task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md) |
> | `FE-IGD-021` | — (kolom kesimpulan) | ✅ | Sama |
> | `FE-IGD-022` | `FR-IGD-060`…`064` | 🟡 uji layar; tab lab cacat; teks radiologi | [fe-igd-022](../task/report/frontend/fe-igd-022-asuhan-keperawatan-pengkajian-observasi-penunjang.md) |

## R3.2 Requirement yang **masih** tidak tertelusuri

`EPIC IGD-04` (`FR-IGD-016`…`022`, riwayat penugasan dokter) dan `EPIC IGD-08`
(`FR-IGD-053`…`059`, kewenangan unit) belum punya task pada revision `3`. Keduanya sudah punya
keputusan dan bentuk teknis, tetapi diletakkan pada `MVP-5` dan `MVP-6` yang belum diuraikan
menjadi task — menguraikannya sekarang menghasilkan task yang akan basi sebelum dikerjakan.

Diuraikan pada revisi roadmap berikutnya, setelah `MVP-3` selesai.

> **Diperbarui 15 September 2026.** `IGD-DEC-114`: `EPIC IGD-04` **tetap dalam lingkup `MVP-5`**
> dan dijadwalkan lewat `plan-module-delivery`. Selama belum punya task, `MVP-5` berstatus 🟡.
>
> **Koreksi rentang, 15 September 2026 (kedua).** `EPIC IGD-04` berisi **`FR-IGD-016` sampai
> `FR-IGD-021`** menurut `04-prd-to-mvp.md`. `FR-IGD-022` milik `EPIC IGD-05` dan **tidak**
> dipindahkan. Tulisan `FR-IGD-016`…`022` pada `IGD-DEC-114`, pada catatan di atas, dan pada
> tabel bagian 5 dikoreksi di sini tanpa mengubah makna requirement. `EPIC IGD-04` kini
> dipecah menjadi `BE-IGD-044`, `BE-IGD-045`, dan `FE-IGD-027` — lihat R3.4.
> `EPIC IGD-08` kini sebagian tertelusuri lewat `BE-IGD-039` (⛔).

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

---

## R3.4 Perencanaan 15 September 2026 — task baru

Ditambahkan `plan-module-delivery` pada backend `7b0c2ece` dan frontend `43adae648`. **Tidak
ada task di bagian ini yang sudah diimplementasikan**, kecuali `FE-IGD-019` yang kartunya
ditambahkan susulan.

Empat keadaan dibedakan dengan tegas:

| Kolom | Artinya | Contoh |
| --- | --- | --- |
| **Requirement approved** | Requirement atau keputusan yang menjadi dasar task sudah `approved` | `IGD-DEC-116` approved 15 Sep 2026 |
| **Delivery planned** | Task sudah punya kartu, acceptance criteria, dan dependency di roadmap | Kartu `BE-IGD-045` ada |
| **Implementation complete** | Source sudah ditulis **dan** acceptance criteria terpetakan, dengan laporan tracked | Hanya `FE-IGD-019` |
| **Runtime verified** | Perilaku dibuktikan pada aplikasi yang berjalan — uji API manual atau uji layar. **Bukan** UAT | Belum ada satu pun di bagian ini |

### R3.4.1 Task backend

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `BE-IGD-017` (tindak lanjut) | — (perbaikan build) | Laporan tracked historis `BE-IGD-017.md`; nol source | — | — | Ya | Source: ya (26 Agt). Laporan: **belum** | Historis (26 Agt), tidak dapat diulang |
| `BE-IGD-040` | `IGD-DEC-115`, `IGD-DEC-119`; **coverage gap: tanpa FR** | `EmergencyObservationController.UpdateObservationStatus`: `Completed` → `CompletionSummary`; batas 1000 karakter `400` | `IGD-DEC-115` ✅, `IGD-DEC-119` ✅ | Ya | Ya | **Ya** ✅ 15 September 2026 — kriteria 1–7 ke source; delta pesan catatan > 2000 karakter tercatat; build **Not Verified** (diserahkan ke Rizki) — [laporan](../task/report/backend/BE-IGD-040.md) | **Belum** — uji API manual contoh 1–6 belum dijalankan |
| `BE-IGD-041` | `FR-IGD-051`; `IGD-DEC-118` | `EmergencyDispositionService.ValidateVisitClosureAsync` memakai `ValidatePesananSebelumPenutupanAsync`; pesan menyebut pesanan | `IGD-DEC-118` ✅ | Ya | Ya | Belum | Belum |
| `BE-IGD-042` | `FR-IGD-001`, `FR-IGD-002`; `IGD-DEC-074`, `IGD-DEC-109`, `IGD-DEC-120` | `EmergencyVisitService.PeriksaJenisEncounter` menolak `Outpatient` | ⛔ **OWNER DATA CONFIRMATION** — jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`; `IGD-DEC-120` ✅ | Ya | Ya — **⛔ BLOCKED** | Belum | Belum |
| `BE-IGD-043` | `IGD-DEC-112` | Laporan tracked `BE-IGD-043.md` atas `f76ebaab`; nol source | — | Ya | Ya | Source: ya (28 Agt). Laporan: **belum** | Belum |
| `BE-IGD-044` | `FR-IGD-017`, `FR-IGD-019`; `IGD-DEC-082` (draft klinis), `IGD-DEC-116` | Tabel `EmgDoctorAssignment` + unique bersyarat + pengisian data lama; migration oleh Rizki | `MVP-1` ✅, `IGD-DEC-116` ✅ | Sebagian — `IGD-DEC-082` menunggu Clinical Governance | Ya | Belum | Belum |
| `BE-IGD-045` | `FR-IGD-016`…`021`; `IGD-DEC-116`, `IGD-DEC-117` | `EmergencyDoctorAssignmentController`/`Service`: `GET /`, `GET /active?at=`, `POST /`, `POST /{id}/handover` | `BE-IGD-044`, `IGD-DEC-116` ✅, `IGD-DEC-117` ✅ | Sebagian — sama | Ya | Belum | Belum |

### R3.4.2 Task frontend

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `FE-IGD-019` | `FR-IGD-060` | Tab Assesmen Awal memakai `VitalSignTab`/`AssessmentTab` dan builder payload bersama | `BE-IGD-026`, `BE-IGD-027` | Ya | Ya (kartu susulan) | **Ya** ✅ — [laporan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) | Belum — uji simpan lewat layar tercatat pada `FE-IGD-013` |
| `FE-IGD-023` | `FR-IGD-046`; `IGD-DEC-105`, `IGD-DEC-111` | `fetchLabOrders` dengan `encounterId` + paging; teks radiologi | `IGD-DEC-111` ✅ | Ya | Ya | **Ya** ✅ 15 September 2026 — 8/8 kriteria ke source; lint exit 0; unit test 686/686 — [laporan](../task/report/frontend/FE-IGD-023.md) | **Belum** — uji layar `NOT FEASIBLE`, `npm run build` diserahkan ke Rizki |
| `FE-IGD-024` | `IGD-DEC-115`, `IGD-DEC-119`, `IGD-DEC-121`; **coverage gap: tanpa FR** | Isian Kesimpulan opsional saat Selesaikan | `BE-IGD-040` (source selesai; build dan runtime belum diverifikasi), `IGD-DEC-121` ✅ | Ya | Ya | **Ya** ✅ 15 September 2026 — 6/6 kriteria ke source; lint exit 0; unit test 852/852; build **Not Verified** — [laporan](../task/report/frontend/FE-IGD-024.md) | **Belum** — uji peramban `NOT FEASIBLE`; runtime `BE-IGD-040` juga belum |
| `FE-IGD-025` | `IGD-EV-117`; `IGD-DEC-107` | Laporan tracked atas `bd1d94a8a` + daftar endpoint klinis tanpa filter; nol source | — | Ya | Ya | Source: ya (31 Agt). Laporan: **belum** | Belum |
| `FE-IGD-026` | `FR-IGD-001`…`012` (dibaca); `IGD-DEC-084` | Laporan tracked atas `c8613d88c` (+ `40f0e6106`/`5bc96f09b`); nol source | — | Ya | Ya | Source: ya (29–30 Agt). Laporan: **belum** | Belum |
| `FE-IGD-027` | `FR-IGD-016`…`021`; `IGD-DEC-116`, `IGD-DEC-117` | Layar triase memakai `Emergency Doctor Assignment`, bukan endpoint Registrasi | `BE-IGD-045` | Sebagian — `IGD-DEC-082` menunggu Clinical Governance | Ya | Belum | Belum |

### R3.4.3 Requirement `EPIC IGD-04` — rantai penuh

| Requirement | Isi | Uji | Task backend | Task frontend |
| --- | --- | --- | --- | --- |
| `FR-IGD-016` | Penetapan dokter kedua lewat endpoint penetapan ditolak | `AT-IGD-124` | `BE-IGD-045` | `FE-IGD-027` |
| `FR-IGD-017` | Pengalihan menutup baris lama dan membuka baris baru | `AT-IGD-125` | `BE-IGD-044`, `BE-IGD-045` | `FE-IGD-027` |
| `FR-IGD-018` | Dokter penanggung jawab pada waktu tertentu dapat dijawab | `AT-IGD-126` | `BE-IGD-045` (`GET /active?at=`, `IGD-DEC-117`) | `FE-IGD-027` |
| `FR-IGD-019` | Tepat satu dokter aktif per kunjungan, dijaga basis data | `AT-IGD-127` | `BE-IGD-044`, `BE-IGD-045` | — |
| `FR-IGD-020` | Nilai efektif pada kunjungan selalu sama dengan dokter aktif | `AT-IGD-128` | `BE-IGD-045` | — |
| `FR-IGD-021` | Pengalihan menuntut alasan | `AT-IGD-129` | `BE-IGD-045` | `FE-IGD-027` |

Enam dari enam requirement `EPIC IGD-04` kini punya task. `FR-IGD-022` **bukan** bagian epic ini.

### R3.4.4 Coverage gap yang tetap terbuka

| Gap | Keterangan |
| --- | --- |
| `BE-IGD-040` dan `FE-IGD-024` tanpa `FR-IGD-*` | Penutupan observasi adalah kapabilitas reuse (`IGD-CAP-26`) tanpa requirement tertulis; dijejak ke `IGD-DEC-115`/`119`/`121` |
| Uji `AT-IGD-124`…`129` tanpa automated test | Proyek test dihapus; bukti pengganti berupa uji API manual (`IGD-DEC-110`) |
| Layar resusitasi IGD | Tanpa ID task atas instruksi owner (`IGD-EV-111`) |
| Layar baca/aksi `order-items` | Tanpa ID task atas instruksi owner (`IGD-EV-109`); aksi tulis terhalang `BE-IGD-039` |
| Teks berkas kontrak belum diselaraskan | API §3 nama tabel dan query `at`, validation §1 aturan 2, §6 aturan 4, dan batas 1000 karakter masih mengikuti teks lama; `IGD-DEC-116`…`120` yang berlaku |
