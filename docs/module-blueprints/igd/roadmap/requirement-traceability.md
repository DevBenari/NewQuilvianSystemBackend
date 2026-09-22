# Requirement Traceability — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 3
wave: "Dikoreksi 2026-09-15: MVP-1, MVP-2, R3.7 selesai; MVP-0, MVP-3, MVP-4, MVP-5 sebagian; MVP-6 terblokir BE-IGD-039"
status: ACTIVE
status_synced_at: "2026-09-15 — backend e89907c5, frontend 43adae648; IGD-DEC-110 sampai IGD-DEC-115"
planning_updated_at: "2026-09-16 (keempat) — plan-module-delivery: FE-IGD-031 dan FE-IGD-032 (tata letak riwayat pada ruang kerja pemeriksaan); IGD-DEC-133 dan IGD-DEC-134; evidence 2026-09-16-tata-letak-riwayat-pemeriksaan.md. Nol perubahan kontrak, nol perubahan backend, nol requirement didesain ulang. Lihat bagian R3.8. Sebelumnya 2026-09-16 (ketiga) — penyelarasan kesiapan EPIC IGD-04 sebelum coding: IGD-DEC-129 sampai IGD-DEC-132; API naik 0.7.0 (aditif, proyeksi nama §3.2 + nama canonical RegPatientEncounter); IsActive dihapus dari rancangan EmgDoctorAssignment; acceptance BE-IGD-044/045 dan FE-IGD-027 diperbarui. Nol requirement didesain ulang. Lihat bagian R3.7. Sebelumnya 2026-09-16 (kedua) — FE-IGD-029 dan FE-IGD-030 (kunjungan keluar dari Arrived); IGD-DEC-127 dan IGD-DEC-128; nol perubahan kontrak, bagian R3.6. Sebelumnya 2026-09-16: BE-IGD-046 dan FE-IGD-028, IGD-DEC-122 sampai IGD-DEC-126, kontrak API dan validation 0.6.0 (bagian R3.5); 2026-09-15 (kedua): BE-IGD-040..045, FE-IGD-019, FE-IGD-023..027"
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
| `FE-IGD-014` | `FR-IGD-001`…`012` | `IGD-01`, `02` | `MVP-1`, `MVP-2` | — | `BE-IGD-023`, `025` | ✅ **Dinilai ulang 21 September 2026 (malam) — atas penilaian pemilik**, sesudah `FE-IGD-034` ✅ (kriteria 2 kini bersumber pra-cek `BE-IGD-050`; "membukanya" termasuk pernyataan pemilik bahwa uji layar lulus) — **celah `IGD-OQ-093` `open` dinyatakan**: tidak berarti tidak ada encounter yatim dalam segala keadaan (`IGD-DEC-138`, `IGD-EV-123` butir 1). *Riwayat:* 🟡 kriteria 2 di source, hasil klik belum dikonfirmasi | [FE-IGD-014](../task/report/frontend/FE-IGD-014.md) |
| `FE-IGD-015` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | Rilis serentak `BE-IGD-031` | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-016` | `FR-IGD-023`…`035` | `IGD-05` | `MVP-4` | — | `BE-IGD-032` | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-017` | `FR-IGD-036`…`043` | `IGD-06` | `MVP-4` | — | `BE-IGD-033`, `034` | ✅ **22 September 2026 — atas penilaian pemilik**: kriteria 1 di source — pelaku dan penyetuju dibaca sebagai nama (`IGD-DEC-137`, `BE-IGD-049` ✅); eslint 0 error, 5 test baru lulus; **uji layar pemilik LULUS** (pelaku `SuperAdmin`, bukan GUID, 21 September); **`npm run build` dinyatakan lulus pemilik** (`IGD-EV-123` butir 2 tertutup) | [FE-IGD-017](../task/report/frontend/FE-IGD-017.md) |
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
| `BE-IGD-041` 🟡 | `FR-IGD-051`; `IGD-DEC-118` | `EmergencyDispositionService.ValidateVisitClosureAsync` memakai `EmergencyDepartureService.AmbilPesananPenahanPenutupanAsync`; pesan menyebut pesanan. Menutup kriteria 2 `BE-IGD-035` | `IGD-DEC-118` ✅ | Ya | Ya | **Sebagian** — 16 September 2026, [laporan](../task/report/backend/BE-IGD-041.md); ketujuh kriteria terpetakan ke source, `dotnet build` dan uji API belum dijalankan | Belum |
| `BE-IGD-042` | `FR-IGD-001`, `FR-IGD-002`; `IGD-DEC-074`, `IGD-DEC-109`, `IGD-DEC-120` | `EmergencyVisitService.PeriksaJenisEncounter` menolak `Outpatient` | ⛔ **OWNER DATA CONFIRMATION** — jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`; `IGD-DEC-120` ✅ | Ya | Ya — **⛔ BLOCKED** | Belum | Belum |
| `BE-IGD-043` | `IGD-DEC-112` | Laporan tracked `BE-IGD-043.md` atas `f76ebaab`; nol source | — | Ya | Ya | Source: ya (28 Agt). Laporan: **belum** | Belum |
| `BE-IGD-044` ✅ | `FR-IGD-017`, `FR-IGD-019`; `IGD-DEC-082` (**`approved` 17 Sep 2026**), `IGD-DEC-116`, `IGD-DEC-130` | Tabel `EmgDoctorAssignment` tanpa `IsActive` + unique bersyarat `EffectiveTo IS NULL` + pengisian data lama; migration oleh Rizki | `MVP-1` ✅, `IGD-DEC-116` ✅ | **Ya** — `IGD-DEC-082` approved 17 Sep 2026 | Ya | **Selesai** — 21 September 2026: seluruh enam acceptance terpenuhi. Acceptance 4 dikerjakan `BE-IGD-048` ✅; acceptance 5 (`Down` asli di basis data terpisah) terbukti ([bukti](../evidence/2026-09-21-uji-down-migration-be-igd-044.md), [laporan](../task/report/backend/BE-IGD-044.md)) | Belum |
| `BE-IGD-045` ✅ | `FR-IGD-016`…`021`; `IGD-DEC-116`, `IGD-DEC-117`, `IGD-DEC-129`, `IGD-DEC-130`, `IGD-DEC-131` | `EmergencyDoctorAssignmentController`/`Service`: `GET /`, `GET /active?at=`, `POST /`, `POST /{id}/handover`, beserta proyeksi `doctorName`/`assignedByName` | `BE-IGD-044` — tabel sudah ada dan migration diterapkan 17 Sep 2026 | **Ya** — `IGD-DEC-082` approved, `IGD-DEC-135` | Ya | **Ya** — 17 September 2026, ketiga belas acceptance terpetakan; source diterima pemilik ([laporan](../task/report/backend/BE-IGD-045.md)) | **Ya** — 17 September 2026: build nol error dan **12 dari 12 skenario uji API `PASS`** termasuk concurrency S12, dijalankan pemilik ([evidence](../evidence/2026-09-17-verifikasi-runtime-be-igd-045.md)). UAT belum |
| `BE-IGD-048` ✅ | `FR-IGD-017`, `FR-IGD-019` (melanjutkan acceptance 4 `BE-IGD-044`) | Migration `BackfillEmergencyDoctorAssignment` — **schema + data** (bukan lagi data-only): `AssignedByUserId` nullable, lalu sisip baris legacy | `BE-IGD-044` ✅; `IGD-OQ-092` ditutup `IGD-DEC-136`; rekonsiliasi schema disetujui 21 September 2026 | **Ya** — 21 September 2026: source, kamus data, kontrak `0.8.0`; build nol error; migration diterapkan ke dev; kriteria A–E, `Up→Down→Up`, guard `Down()`, dan idempotensi terbukti di salinan basis data terpisah; probe service asli. Uji HTTP/layar belum ([laporan](../task/report/backend/BE-IGD-048.md)) | Ya | Belum | Belum |

### R3.4.2 Task frontend

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `FE-IGD-019` | `FR-IGD-060` | Tab Assesmen Awal memakai `VitalSignTab`/`AssessmentTab` dan builder payload bersama | `BE-IGD-026`, `BE-IGD-027` | Ya | Ya (kartu susulan) | **Ya** ✅ — [laporan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) | Belum — uji simpan lewat layar tercatat pada `FE-IGD-013` |
| `FE-IGD-023` | `FR-IGD-046`; `IGD-DEC-105`, `IGD-DEC-111` | `fetchLabOrders` dengan `encounterId` + paging; teks radiologi | `IGD-DEC-111` ✅ | Ya | Ya | **Ya** ✅ 15 September 2026 — 8/8 kriteria ke source; lint exit 0; unit test 686/686 — [laporan](../task/report/frontend/FE-IGD-023.md) | **Belum** — uji layar `NOT FEASIBLE`, `npm run build` diserahkan ke Rizki |
| `FE-IGD-024` | `IGD-DEC-115`, `IGD-DEC-119`, `IGD-DEC-121`; **coverage gap: tanpa FR** | Isian Kesimpulan opsional saat Selesaikan | `BE-IGD-040` (source selesai; build dan runtime belum diverifikasi), `IGD-DEC-121` ✅ | Ya | Ya | **Ya** ✅ 15 September 2026 — 6/6 kriteria ke source; lint exit 0; unit test 852/852; build **Not Verified** — [laporan](../task/report/frontend/FE-IGD-024.md) | **Belum** — uji peramban `NOT FEASIBLE`; runtime `BE-IGD-040` juga belum |
| `FE-IGD-025` | `IGD-EV-117`; `IGD-DEC-107` | Laporan tracked atas `bd1d94a8a` + daftar endpoint klinis tanpa filter; nol source | — | Ya | Ya | Source: ya (31 Agt). Laporan: **belum** | Belum |
| `FE-IGD-026` | `FR-IGD-001`…`012` (dibaca); `IGD-DEC-084` | Laporan tracked atas `c8613d88c` (+ `40f0e6106`/`5bc96f09b`); nol source | — | Ya | Ya | Source: ya (29–30 Agt). Laporan: **belum** | Belum |
| `FE-IGD-027` ✅ | `FR-IGD-016`…`021`; `IGD-DEC-116`, `IGD-DEC-117`, `IGD-DEC-129`, `IGD-DEC-130` | Layar triase memakai `Emergency Doctor Assignment`, bukan endpoint Registrasi | `BE-IGD-045` ✅ Build dan Runtime terverifikasi 17 Sep 2026 | **Ya** — `IGD-DEC-082` `approved` 17 Sep 2026 | Ya | **Ya** — 17 September 2026, ketujuh acceptance terpetakan; lint `PASS`, unit test 866/866 ([laporan](../task/report/frontend/FE-IGD-027.md)) | **Selesai** — 18 September 2026 **pada revisi `3213419a7`**: `npm run build` lulus dan **13 pemeriksaan lewat layar `PASS`** oleh pemilik ([evidence](../evidence/2026-09-18-verifikasi-runtime-fe-igd-027.md)). **Revisi terbaru** (terminologi + "Data historis", commit `16c767916`): eslint berkas task dan 38 test IGD `PASS`, **`npm run build` lulus — dijalankan pemilik 21 September 2026 (362/362 halaman)**. Tampilan "Data historis" belum terlihat di layar karena dev 0 baris legacy. UAT belum |

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
| ~~Teks berkas kontrak belum diselaraskan~~ | **Ditutup 15 September 2026** — API dan validation naik ke `0.5.0`, lalu `0.6.0` pada 16 September 2026; lihat manifest bagian 0c dan 0d |

---

## R3.5 Perencanaan 16 September 2026 — pemantauan observasi bertanda vital

Sumber: audit Observasi V1 lawan V2
([evidence](../evidence/2026-09-15-audit-observasi-v1-v2.md)) dan keputusan `IGD-DEC-122`
sampai `IGD-DEC-126`.

### R3.5.1 Task

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `BE-IGD-046` ✅ | `IGD-DEC-122`…`126`, `IGD-DEC-056`, `IGD-DEC-057`; **coverage gap: tanpa FR** | `EmergencyObservationService` + `EmergencyObservationDetailController` + DTO: validasi lingkup tanda vital, pelaku dari token, tolak `409` pada periode tertutup, proyeksi `vitalSign` dan `recordedByName` | `IGD-DEC-122` ✅, `IGD-DEC-126` ✅, kontrak `0.6.0` | Ya | Ya | Ya — 16 September 2026, [laporan](../task/report/backend/BE-IGD-046.md); build bersih (nol error) | **Sebagian** — terbukti lewat uji layar `FE-IGD-028` 16 September 2026: proyeksi `vitalSign` dan `recordedByName` terbaca, periode tertutup menolak pemantauan baru |
| `FE-IGD-028` ✅ | `IGD-DEC-122`, `IGD-DEC-123`, `IGD-DEC-125`, `IGD-DEC-126`; **coverage gap: tanpa FR** | Tab Observasi: bagian Tanda Vital pada *Catat Pemantauan*, riwayat bertanda vital dan nama pencatat, konteks ABCDE baca saja | `BE-IGD-046` | Ya | Ya | Ya — 16 September 2026, [laporan](../task/report/frontend/FE-IGD-028.md); lint, 857 unit test, dan build lulus | **Sebagian** — uji layar pemilik 16 September 2026 lulus tanpa galat; jalur pilih-existing (kriteria 3) dan ABCDE terisi (kriteria 8) belum dilalui |

### R3.5.2 Keputusan yang dipakai gelombang ini

| Keputusan | Isi singkat | Task |
| --- | --- | --- |
| `IGD-DEC-122` | Tanda vital ditautkan lewat `PatientVitalSignId`, tidak disalin; pilihan terbatas pada pasien dan encounter yang sama | `BE-IGD-046`, `FE-IGD-028` |
| `IGD-DEC-123` | ABCDE terakhir dibaca saja; evaluasi ditulis pada `ClinicalConditionSummary` | `FE-IGD-028` |
| `IGD-DEC-124` | Alat bantu jalan napas belum terstruktur; ditulis pada `InterventionSummary` | — (batas lingkup) |
| `IGD-DEC-125` | Jenis oksigen memakai enum `ClinicalManagement` apa adanya | `FE-IGD-028` |
| `IGD-DEC-126` | Periode `Completed`/`Cancelled` menolak pemantauan baru dengan `409` | `BE-IGD-046`, `FE-IGD-028` |

### R3.5.3 Coverage gap yang tetap terbuka

| Gap | Keterangan |
| --- | --- |
| `BE-IGD-046` dan `FE-IGD-028` tanpa `FR-IGD-*` | Pemantauan observasi adalah kapabilitas reuse (`IGD-CAP-26`, `IGD-CAP-21`) tanpa requirement tertulis; dijejak ke `IGD-DEC-122`…`126` |
| Bentuk terstruktur alat jalan napas | `IGD-OQ-089` terbuka; tidak menahan kedua task |
| Entri susulan setelah periode ditutup | `IGD-OQ-090` terbuka; **dilarang** menumpang `BE-IGD-046` |
| Tanda vital untuk pasien tanpa identitas provisional | `IGD-EV-130` — `PatientId` wajib pada tanda vital sementara `EmgVisit.PatientId` boleh kosong; perlu uji runtime, bukan keputusan |

---

## R3.6 Perencanaan 16 September 2026 (kedua) — kunjungan keluar dari `Arrived`

Sumber: temuan pemakaian layar oleh Product/Domain Owner
([evidence](../evidence/2026-09-16-kunjungan-terjebak-arrived.md), `IGD-EV-131`…`IGD-EV-136`)
dan keputusan `IGD-DEC-127` serta `IGD-DEC-128`.

Gelombang ini **memulihkan kemampuan yang sudah dibangun**, bukan menambah yang baru. Karena
tidak ada jalan keluar dari `Arrived` di layar, seluruh rantai requirement sesudah triage —
`FR-IGD-013` sampai `FR-IGD-015` dan setiap requirement yang bergantung padanya — tidak dapat
dibuktikan lewat layar untuk pasien baru, walaupun source-nya sudah ada dan sudah lulus build.

### R3.6.1 Task

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `FE-IGD-029` 🟡 | `FR-IGD-013`, `FR-IGD-015`; `IGD-DEC-127`, `IGD-DEC-093` | `emergency-registration.utils.js`: default `visitStatus` payload pendaftaran menjadi `WaitingForTriage` | `IGD-DEC-127` ✅ | Ya | Ya | Ya — 16 September 2026, [laporan](../task/report/frontend/FE-IGD-029.md); lint, 859 unit test, dan build lulus | **Belum** — menunggu satu pendaftaran IGD baru dijalankan lewat layar (kriteria 1 dan 2) |
| `FE-IGD-030` ✅ | `IGD-DEC-128`, `IGD-DEC-104` (b), `IGD-DEC-093`; **coverage gap: tanpa FR** | Daftar triage: aksi Tangani Segera memanggil `PATCH /emergency-visits/{id}/visit-status` dengan `InTreatment`, lalu mengalihkan perawat ke layar Assesmen IGD | `BE-IGD-018` ✅, `IGD-DEC-128` ✅ | Ya | Ya | Ya — 16 September 2026, [laporan](../task/report/frontend/FE-IGD-030.md); lint, 859 unit test, dan build lulus | **Ya** — ketiga skenario lulus lewat layar 16 September 2026: status berpindah, pengalihan bekerja, triage susulan tidak memundurkan status. Tanpa UAT |

### R3.6.2 Keputusan yang dipakai gelombang ini

| Keputusan | Isi singkat | Task |
| --- | --- | --- |
| `IGD-DEC-127` | Pendaftaran IGD yang tuntas menutup kunjungan dengan `WaitingForTriage`, bukan `Arrived` | `FE-IGD-029` |
| `IGD-DEC-128` | Penanganan cepat lewat aksi status kunjungan ke `InTreatment`; triage disusulkan; tiga jalan pintas ditolak | `FE-IGD-030` |
| `IGD-DEC-104` (b) | Penilaian ulang pada pasien yang sudah melewati triage tersimpan tanpa memundurkan status | `FE-IGD-030` |
| `IGD-DEC-093` | Kontrak state bagian 1 `approved` — sumber kesahan kedua transisi yang dipakai | `FE-IGD-029`, `FE-IGD-030` |

### R3.6.3 Kontrak

**Nol perubahan, nol kenaikan versi.** Kedua transisi yang dipakai sudah ada pada tabel state
`0.4.0` bagian 1 dan sudah `approved` sejak 24 Agustus 2026. Endpoint yang dipanggil `FE-IGD-030`
sudah berjalan sebelum gelombang ini. `CanTransition` **tidak** disentuh.

### R3.6.4 Coverage gap yang tetap terbuka

| Gap | Keterangan |
| --- | --- |
| `FE-IGD-030` tanpa `FR-IGD-*` | Jalur penanganan cepat tidak pernah dituliskan sebagai requirement; dijejak ke `IGD-DEC-128` |
| Kedatangan sebelum pendaftaran | `IGD-OQ-091` terbuka — apakah `Arrived` masih butuh penghasil sendiri sesudah `IGD-DEC-127`. Tidak menahan kedua task |
| Baris lama yang terlanjur `Arrived` | Dua kunjungan pada basis data dev (`IGD-EV-131`). `FE-IGD-029` **tidak** memindahkannya; jalan keluarnya lewat `FE-IGD-030` atau tindakan data terpisah |
| Izin `EmergencyVisit` + `Update` pada peran perawat triage | Belum terverifikasi; menahan pembuktian runtime `FE-IGD-030`, bukan implementasinya |
| Master level triage di basis data dev | `IGD-EV-135` — 4 baris manual lawan 6 baris seeder; `AllowsTreatmentBeforeRegistration` belum terbaca. Perlu pemeriksaan pemilik, bukan keputusan |

---

## R3.7 Penyelarasan kesiapan `EPIC IGD-04` — 16 September 2026 (ketiga)

Pass ini **tidak menambah task dan tidak mengubah requirement**. Ia menutup empat celah
pelaksanaan yang ditemukan pada tinjauan kesiapan sebelum coding, seluruhnya diputuskan owner
pada hari yang sama.

### R3.7.1 Ketujuh pertanyaan audit owner terhadap rantai yang sudah ada

| Pertanyaan owner | Requirement | Dijawab oleh |
| --- | --- | --- |
| Siapa dokter penanggung jawab sekarang | `FR-IGD-019` | `GET /active` tanpa `at`; `EffectiveTo IS NULL` dijaga unique bersyarat |
| Sejak kapan | `FR-IGD-017` | Kolom `EffectiveFrom` |
| Siapa dokter sebelumnya | `FR-IGD-017` | `GET /` riwayat urut waktu; baris lama tidak pernah ditimpa |
| Kapan pengalihan terjadi | `FR-IGD-017` | `EffectiveTo` lama ditutup bersamaan `EffectiveFrom` baru, satu transaksi |
| Siapa yang mengalihkan | `FR-IGD-021` | `AssignedByUserId` pada baris baru |
| Alasan pengalihan | `FR-IGD-021` | `AssignmentReason`, wajib saat pengalihan |
| Siapa dokter pada waktu tertentu | `FR-IGD-018` | `GET /active?at={datetime}` (`IGD-DEC-117`) |

**Nol requirement baru, nol requirement diubah.** Ketujuhnya sudah tertelusur pada bagian R3.4.3.

### R3.7.2 Keputusan yang dipakai pass ini

| Keputusan | Isi singkat | Task terdampak |
| --- | --- | --- |
| `IGD-DEC-129` | Response §3 menyertakan `doctorName` dan `assignedByName`; aditif, nol kolom baru, nol `N+1` | `BE-IGD-045`, `FE-IGD-027` |
| `IGD-DEC-130` | `EffectiveFrom`/`EffectiveTo` satu-satunya penanda; `IsActive` dihapus dari rancangan sebelum migration | `BE-IGD-044`, `BE-IGD-045`, `FE-IGD-027` |
| `IGD-DEC-131` | `Program.cs` boleh disentuh terbatas untuk pendaftaran DI service baru | `BE-IGD-045` |
| `IGD-DEC-132` | Kontrak menyebut `RegPatientEncounter`, bukan `TrxPatientEncounter` | `BE-IGD-045` (kontrak), integration contract |

### R3.7.3 Kontrak

API naik **`0.6.0` → `0.7.0`**, **aditif**: bagian 3.2 baru untuk proyeksi nama, dan nama tabel
Registrasi diselaraskan. Nol route baru, nol bentuk request berubah, nol penolakan baru. Validation,
state, dan permission/audit **tidak berubah**. Integration contract disentuh hanya pada nama tabel.

### R3.7.4 Coverage gap yang tetap terbuka

| Gap | Keterangan |
| --- | --- |
| ~~`IGD-DEC-082` masih `draft`~~ | **Ditutup 17 September 2026** — `approved` oleh Product/Domain Owner; butir 10 DoD `BE-IGD-045` tidak lagi tertahan. Peran Clinical Governance tetap `OPEN` dan wajib meninjau ulang bila kelak ditunjuk |
| Nama `TrxPatientEncounter` di luar kontrak | `erd/00-context-erd.md` dan kamus data §5.3 masih memakai nama lama, termasuk jalur berkas model. Di luar lingkup `IGD-DEC-132` yang menyebut *kontrak*; perlu keputusan terpisah bila hendak diselaraskan |
| ~~Uji langkah mundur migration `BE-IGD-044`~~ | **Ditutup 21 September 2026** — `Down` asli diuji di basis data terpisah dan lulus ([bukti](../evidence/2026-09-21-uji-down-migration-be-igd-044.md)); `BE-IGD-044` ✅ |

---

## R3.8 Perencanaan 16 September 2026 (keempat) — tata letak riwayat pada ruang kerja pemeriksaan

Sumber: tinjauan tampilan layar Assesmen IGD oleh Product/Domain Owner
([evidence](../evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md)) dan keputusan
`IGD-DEC-133` serta `IGD-DEC-134`.

Gelombang ini **memperbaiki cara data yang sudah ada dibaca**. Tidak ada requirement baru, tidak
ada requirement yang didesain ulang, dan tidak ada data baru yang diminta. Karena itu seluruh
rantainya berhenti di keputusan, bukan di `FR-IGD-*` — susunan tampilan memang tidak pernah
dituliskan sebagai requirement pada modul ini.

### R3.8.1 Task

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `FE-IGD-031` 🟡 | `IGD-DEC-133`; wewenang UI `03-frontend-architecture.md` bagian 11 dan 12.5; **coverage gap: tanpa FR** | Satu pembungkus segmen "Formulir \| Riwayat" memakai `ClinicalSegmentedNav`, dipakai lima tab pemeriksaan; perpindahan otomatis ke Riwayat sesudah simpan berhasil; baris "terakhir dikaji" pada tab Assesmen Awal IGD | `IGD-DEC-133` ✅ | Ya | Ya | **Ya — 16 September 2026**, [laporan](../task/report/frontend/FE-IGD-031.md); lint dan 866 unit test lulus; `npm run build` belum dijalankan | **Belum** — uji lewat layar belum dijalankan |
| `FE-IGD-032` 🟡 | `IGD-DEC-134`, `IGD-DEC-133`; `IGD-DEC-122`…`126` tetap berlaku; **coverage gap: tanpa FR** | Tab Observasi: pemilih periode di atas, primary survey diringkas jadi satu baris, lembar pemantauan berbentuk tabel berjajar, segmen "Lembar Pemantauan \| Catat Pemantauan" | `IGD-DEC-134` ✅, `FE-IGD-031` 🟡, `FE-IGD-028` ✅ | Ya | Ya | **Ya — 16 September 2026**, [laporan](../task/report/frontend/FE-IGD-032.md); lint dan 866 unit test lulus; **dua delta** pada kriteria 3 dan 4; `npm run build` belum dijalankan | **Belum** — kriteria 8 dan 9 menahan perilaku `FE-IGD-024` dan `FE-IGD-028`, dan keduanya menuntut uji lewat layar |

### R3.8.2 Keputusan yang dipakai gelombang ini

| Keputusan | Isi singkat | Task |
| --- | --- | --- |
| `IGD-DEC-133` | Segmen Formulir/Riwayat memakai `ClinicalSegmentedNav`; berpindah sendiri sesudah simpan; baris "terakhir dikaji" pada Assesmen Awal; SOAP, Catatan Terintegrasi, dan Resep dikecualikan | `FE-IGD-031`, `FE-IGD-032` |
| `IGD-DEC-134` | Tata letak Observasi: pemilih periode, primary survey diringkas, lembar pemantauan berbentuk tabel, segmen Lembar/Catat | `FE-IGD-032` |
| `IGD-DEC-123` | ABCDE terakhir dibaca saja — **tetap berlaku**, hanya berubah bentuk penyajiannya | `FE-IGD-032` |
| `IGD-DEC-122`, `IGD-DEC-126` | Penautan tanda vital dan penolakan `409` pada periode tertutup — **tetap berlaku tanpa perubahan perilaku** | `FE-IGD-032` |

### R3.8.3 Kontrak

**Nol perubahan, nol kenaikan versi, nol endpoint baru, nol perubahan backend.** API `0.7.0`,
validation `0.6.0`, state `0.4.0`, permission/audit, dan integration contract **tidak disentuh**.
Aturan bagian 12.4 `03-frontend-architecture.md` tidak berubah. Yang dipakai gelombang ini adalah
wewenang `DEV_DISCRETION` yang sudah tertulis pada bagian 11 dan 12.5 dokumen yang sama.

### R3.8.4 Coverage gap yang tetap terbuka

| Gap | Keterangan |
| --- | --- |
| `FE-IGD-031` dan `FE-IGD-032` tanpa `FR-IGD-*` | Susunan tampilan tidak pernah dituliskan sebagai requirement pada modul ini; dijejak ke `IGD-DEC-133` dan `IGD-DEC-134`. Sama polanya dengan `FE-IGD-028` dan `FE-IGD-030` |
| Pengkajian ganda oleh dua perawat | Baris "terakhir dikaji" (`FE-IGD-031` kriteria 6 dan 7) adalah penambal tampilan, **bukan** penjaga aturan. Penjaga sesungguhnya harus di backend, dan belum ada requirement maupun keputusan yang memintanya. Dicatat sebagai lubang terbuka, bukan sebagai bagian gelombang ini |
| Tab SOAP tanpa riwayat di tempatnya sendiri | Disengaja — riwayatnya milik tab Catatan Terintegrasi (`IGD-DEC-133`). Bila kelak dipandang perlu, itu keputusan baru, bukan kekurangan kedua task ini |
| Uji lewat layar | Belum dijalankan untuk kedua task; keduanya `NOT FEASIBLE` bagi agent dan menunggu pemilik. Bukan UAT |
| `ClinicalDataTable` tidak dapat dipakai tanpa kepalanya | Ditemukan saat mengerjakan `FE-IGD-032`: komponen bersama itu selalu merender judul, badge jumlah, dan empty state sendiri. Tabel lembar pemantauan karena itu ditulis sebagai `<table>` pada modul CSS layar IGD. Menambah prop untuk mematikan kepalanya berarti mengubah komponen bersama — perlu keputusan terpisah |
| Prop `disabled` pada `EmergencyAssessmentFormCard` | Ditemukan saat mengerjakan `FE-IGD-031`: kartu koreksi tab Transfer mengirim `disabled`, sedangkan komponennya membaca `canSubmit`. Tombol Simpan Koreksi karena itu tidak pernah dinonaktifkan di layar. Cacat lama, dipindahkan apa adanya, belum diperbaiki |

---

## R3.12 Gelombang 21 September 2026 (sore) — nama pelaku pada event kepergian dan pra-cek episode ganda

Dua keputusan pemilik atas temuan gerbang backlog frontend. **`BE-IGD-049` dan `BE-IGD-050` sudah diimplementasikan
(21 September 2026, malam); `FE-IGD-034` 🟡 sudah diimplementasikan (21 September 2026, malam; build dan uji layar milik pemilik).**

### R3.12.1 Task

| Task | Requirement / keputusan | Target implementasi | Dependency | Requirement approved | Delivery planned | Implementation complete | Runtime verified |
| --- | --- | --- | --- | :-: | :-: | :-: | :-: |
| `BE-IGD-049` | `FR-IGD-036`…`043`; **`IGD-DEC-137`** (`approved` 21 Sep 2026); pola `IGD-DEC-129`, `BE-IGD-046` | `recordedByName` dan `approvedByName` pada `EmergencyDepartureEventResponse`; satu kueri batch pengguna per respons; nol schema | `BE-IGD-033` ✅, `BE-IGD-034` ✅ | **Ya** | Ya | **Ya** — 21 Sep 2026 (malam): 3 berkas source, kontrak `0.9.0`, **`dotnet build` `0 Error(s)`, 207 warning (baseline)**; di-commit `3ebc4110` ([laporan](../task/report/backend/BE-IGD-049.md)) | **Ya — atas pernyataan pemilik (22 Sep 2026):** S1 lewat layar 21 Sep; **S2–S5 dilaporkan lulus**, badan respons dan log SQL tidak dilampirkan |
| `BE-IGD-050` ✅ | `FR-IGD-005`…`012`; **`IGD-DEC-138`** (`approved`, perilaku target), `IGD-DEC-084` | `GET emergency-visits/active-episode?patientId=` baca-saja memakai `CariEpisodeAktifAsync`; `POST /` tidak berubah | `BE-IGD-023` ✅, `BE-IGD-025` ✅ | **Ya** untuk perilaku; mekanisme lapis A diputuskan pemilik 21 Sep 2026 (malam) | Ya | **Ya** — 21 Sep 2026 (malam): 3 berkas source, kontrak API `0.10.0` dan validation `0.7.0`; build dijalankan pemilik ([laporan](../task/report/backend/BE-IGD-050.md)) | **Ya — atas pernyataan pemilik (21 Sep 2026 malam):** uji API S1–S7 `PASS` semuanya; angka tidak dilampirkan. **Dikecualikan:** kueri audit A/B (acceptance 8) belum dilaporkan. `IGD-OQ-093` `open` = backend gap eksplisit |
| `FE-IGD-017` ✅ | `FR-IGD-036`…`043`; `IGD-DEC-137` | Dua ruas nama pada tab kepergian, sesudah backend | `BE-IGD-033` ✅, `BE-IGD-034` ✅, **`BE-IGD-049` ✅** | Ya | Ya | Kriteria 1–3 ✅ (21 Sep 2026 larut malam; lint 0 error, 5 test baru; di-commit `c941012ac`; [laporan](../task/report/frontend/FE-IGD-017.md)) | **Ya** — uji layar LULUS (21 Sep 2026 larut malam, [laporan](../task/report/frontend/FE-IGD-017.md) 6.1); **`npm run build` dinyatakan lulus pemilik 22 Sep 2026** (keluaran tidak dilampirkan) |
| `FE-IGD-034` ✅ | `FR-IGD-005`…`012` sisi tampilan; **`IGD-DEC-138`** | Pra-cek sebelum `POST patient-encounters`; menggantikan heuristik `FE-IGD-014` | **`BE-IGD-050`** ✅, `FE-IGD-014` ✅ | Ya | Ya | **Ya** — 21 Sep 2026 (malam): 5 berkas frontend (commit `c941012ac`); eslint 0 error; unit test berkas task 14/14; build dijalankan pemilik ([laporan](../task/report/frontend/FE-IGD-034.md)) | **Ya — atas pernyataan pemilik** (21 Sep 2026 malam): build dan uji layar lulus; keluaran/rincian tidak dilampirkan. `IGD-OQ-093` `open` tidak tertutup |

### R3.12.2 Keputusan dan pertanyaan terbuka

| ID | Isi ringkas | Dipakai oleh |
| --- | --- | --- |
| `IGD-DEC-137` | Respons event kepergian menyertakan nama pelaku selain ID; aditif; tanpa `N+1`; tanpa schema; **tidak** menyentuh authorization/ownership/ruas aktor lain | `BE-IGD-049`, `FE-IGD-017` |
| `IGD-DEC-138` | Pendaftaran IGD ganda tidak boleh meninggalkan encounter yatim; deteksi sebelum encounter dibuat; dilarang `hard-delete` tanpa audit referensi | `BE-IGD-050`, `FE-IGD-034`, `FE-IGD-014` |
| `IGD-OQ-093` | `open` — jaminan sisi server untuk dua celah pra-cek (serentak; klien tanpa pra-cek); opsi B1/B2/B3 | Backend gap eksplisit; tidak menahan `BE-IGD-050`/`FE-IGD-034` |

### R3.12.3 Coverage gap yang dicatat

| Gap | Keterangan |
| --- | --- |
| Encounter yatim yang sudah ada | Kandidat belum diketahui jumlahnya; **hasil kueri audit belum dilaporkan** (`BE-IGD-050` ✅ dengan acceptance 8 dikecualikan). Kueri baca-saja ada pada kartu `BE-IGD-050` acceptance 8, dijalankan pemilik. **Tidak ada pembersihan** tanpa audit referensi (`IGD-DEC-138`) |
| Jaminan sisi server | `IGD-OQ-093` — pra-cek dijaga pemanggil, bukan server; dua celah tetap terbuka |
| Ruas aktor lain pada kontrak kepergian | `requestedByUserId`, `sendingNurseUserId`, `receivingNurseUserId`, `actionByUserId`, `acceptedByUserId` tetap ID. `IGD-DEC-137` sengaja sempit; layar kepergian yang ada tidak menampilkannya |
| Perilaku bila pra-cek itu sendiri gagal | `FE-IGD-034` butir 5 — **`fail-open` diputuskan pemilik** (21 September 2026 malam) dan diimplementasikan; runtime belum terbukti (uji layar U5) |
| Hak akses pra-cek | `EmergencyVisit : Create` diusulkan; pilihan `Read` belum tentu dipegang petugas pendaftaran. Hak akses basis data tidak diperiksa agent |
| `FE-IGD-014` | Pencarian daftar kunjungan pada implementasi sekarang memakai `Read` dan **dapat gagal senyap** untuk peran tanpa `Read`; digantikan `FE-IGD-034` |
