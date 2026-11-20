# Laporan Perubahan Backend — `BE-HRD-380`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-380` |
| Judul | Verifikasi amandemen `S-D5` v28: kontrak, registri hak, build, QBE, dan penutupan celah `BE-HRD-333` |
| Slice | `S-D5` amandemen / Verifikasi (`MVP-70`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-380` |
| Trace | `FR-TRV-26` s.d. `FR-TRV-33`; kontrak `v28` seluruh bagian; AT `AT-HRD-TRV-38` |
| Contract version | `v28` — approved 19 November 2026 |
| Dependency | `BE-HRD-374` s.d. `BE-HRD-379` ✅ |
| Klasifikasi | `LOW` — verifikasi; satu berkas uji diperluas, tanpa perubahan source aplikasi |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjut") |
| Target tulis | `Tests/QuilvianSystemBackend.HumanResourceTests/TravelContractV25Tests.cs`; laporan ini, laporan `BE-HRD-333`, roadmap, traceability, indeks uji |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `43f66daf` (branch `yoga`); `BE-HRD-376` s.d. `379` belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti; `BE-HRD-333` dinaikkan ke ✅ |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `BusinessTravelManagement` |
| Keberlakuan | Verifikasi atas `NEW CODE` dan `TOUCHED LEGACY` task `374`–`379`; tanpa source aplikasi baru |
| QBE yang diperiksa | Seluruh QBE checker atas 17 berkas source amandemen (`Strict`) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Perubahan

| Berkas | Isi |
| --- | --- |
| `TravelContractV25Tests.cs` | Endpoint ke-25 `POST /api/v1/hrd/travel/requests/{id}/submit` (§34.1) masuk tabel kontrak (tag, login, izin = `ControllerName` + `[AccessAction]`); batas jumlah endpoint source ≥ 25; **baru** `Kode_status_v28_terdokumentasi` (6 kasus: Ajukan 200/400/403/404/409, ubah 404/409, batal 404/409, buat 201, uang muka 409, settlement 409); **baru** `Ajukan_memakai_hak_Create_dan_tidak_ada_endpoint_setujui_modul` (§34.1, `HRD-DEC-224`) |

---

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `TravelContractV25Tests` | 34/34 lulus (25 endpoint + kode status v28 + hak Ajukan + validator registri) | `PASS` (AT-38) | Keluaran `dotnet test` |
| Kontrol negatif | K1 rute Ajukan diganti `{id}/ajukan` → 3 uji gagal; K2 `409` dihapus dari batal → 1 uji gagal; berkas dipulihkan identik | `PASS` | idem |
| QBE `Strict` atas 17 berkas source amandemen (konstanta, seeder, 7 service, 3 controller, DTO, worker, `Program.cs`) | 0 violation, 0 review | `PASS` | Keluaran checker |
| Proyek uji HR penuh (`dotnet test -p:RunAnalyzers=False`, termasuk build aplikasi) | **379/379** lulus | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2563 identitas kanonik dari 2563 kemampuan terdaftar." (Ajukan berbagi identitas `BusinessTravelRequest : Create`); seeder workflow 0/0; `TravelWorkflowReconciliationWorker dimulai.`; tanpa galat DI | `PASS` | `be380.log` |
| Bukti devYoga per task | `375`: Draf → Ajukan → tugas atasan, `403`/`409`/`404`; `376`: tarik, kembalikan, ajukan ulang, setujui (presensi + gembok), tolak final; `377`: batal menunggu/revisi/sesudah disetujui, `409` bukan pemohon; `378`: gerbang uang muka/settlement dan plafon 80%; `379`: log audit empat SPPD | `PASS` | Laporan task masing-masing |

---

## 3. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Uji kontrak reflektif lulus untuk 25 endpoint dengan kontrol negatif | Terpenuhi | 34/34 + K1/K2 |
| 2. Validator registri bersih; startup "Permission registry valid" | Terpenuhi | Uji validator + log startup |
| 3. `dotnet build -p:RunAnalyzers=False` 0 galat; QBE tanpa pelanggaran pada berkas yang diubah | Terpenuhi | Build dalam `dotnet test`; QBE Strict 17 berkas |
| 4. Laporan menyatakan tanpa migration; status `BE-HRD-333` diperbarui | Terpenuhi | **Tidak ada migration** di seluruh amandemen `374`–`380`; `BE-HRD-333` → ✅ |

---

## 4. Ringkasan amandemen `S-D5` v28 (`MVP-70`)

| Task | Hasil |
| --- | --- |
| `BE-HRD-374` ✅ | Definisi workflow `BUSINESS_TRAVEL_REQUEST` ditanam aktif, idempoten |
| `BE-HRD-375` ✅ | SPPD dibuat Draf; isi terkunci; endpoint Ajukan |
| `BE-HRD-376` ✅ | Penangan Workflow V2 + presensi Dinas Luar/gembok + penyelarasan ulang berkala (keputusan (a)) |
| `BE-HRD-377` ✅ | Batal menarik/membatalkan workflow, melepas gembok |
| `BE-HRD-378` ✅ | Gerbang uang muka dan settlement |
| `BE-HRD-379` ✅ | 12 event audit bernama matriks |
| `BE-HRD-380` ✅ | Verifikasi; `BE-HRD-333` ditutup |

Penyegaran blueprint yang tercatat (tidak disunting selama delivery): integration-contract 25.1 (penangan sesudah commit + penyelarasan berkala), state-transition 32.3 (instance `Draft` tidak memindahkan SPPD), state 32.2 (batal saat `NeedRevision` menutup workflow; `Rejected` tidak dapat dibatalkan).

Temuan yang diusulkan menjadi task/konfigurasi: master jenis perjalanan dinas tanpa API/seeder; alasan penolakan `HRD_TRV_REQUEST` ber-`CloseRequest` wajib dikonfigurasi; atasan langsung pegawai dan hak `BusinessTravelRequest` per jabatan belum diisi di devYoga; penomoran SPPD/uang muka/settlement `Count + 1`.

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / basis data | Tidak ada migration. Data devYoga hanya lewat API/startup dan satu INSERT `MstTravelType` yang diizinkan pengguna (`BE-HRD-375`) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | Frontend `MVP-71`: `FE-HRD-337` (layar SPPD dari API) lalu `FE-HRD-338`/`339`/`340` |
