# Laporan Perubahan Backend — `BE-HRD-447`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-447` |
| Judul | `ContractRenewalBuckets`: satu definisi tahapan perpanjangan kontrak |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-73` · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-447` |
| Trace | `FR-PKB-02`; `HRD-DEC-237`; state-transition 33.1 (`CONTRACT_EXPIRY`); `INT-HR-111`; `AT-HRD-PKB-05` |
| Contract version | `v29` — **approved** 23 November 2026 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — refactor perhitungan yang dipakai layar Kontrak; risiko regresi angka |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `Areas/Corporate/HumanResource/LifecycleManagement/Services/**`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria: respons `summary`/`renewal-summary` build sebelum refactor (`7e4c3774`) dan sesudah (`9062bd61`) identik pada DB dev yang sama (23 November 2026); kesetaraan predikat dibuktikan baris per baris; build 0 error, QBE Strict `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` (service) + `NEW CODE` (kelas statis tanpa persistence) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-CODE-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Aturan tahapan perpanjangan kontrak (H-60, H-14, H-7, menunggu keputusan, lewat jatuh tempo) tertanam langsung di dua metode layar Kontrak. Dashboard SDM wajib menampilkan angka yang **sama persis** dengan layar Kontrak (`HRD-DEC-237`). Bila aturan disalin ke dashboard, kedua angka bisa berbeda begitu salah satunya diubah.

## 2. Proses bisnis

Aturan tahapan (tanggal acuan = tanggal UTC hari ini, sama dengan sebelumnya):

| Tahapan | Aturan | Contoh pada 20 Desember 2026 |
| --- | --- | --- |
| Segera berakhir (`ALL`, `ExpiringSoonCount`) | Kontrak berjalan, berakhir hari ini s.d. 60 hari ke depan | Berakhir 18 Februari 2027 termasuk; 19 Februari tidak |
| `H60` | Berakhir 15–60 hari lagi | Berakhir 10 Februari 2027 |
| `H14` | Berakhir 8–14 hari lagi | Berakhir 1 Januari 2027 |
| `H7` | Berakhir 0–7 hari lagi | Berakhir 25 Desember 2026 |
| `PENDING_DECISION` | Usulan perpanjangan berstatus diajukan | Usulan perpanjangan Sari menunggu Rina |
| `OVERDUE` | Kontrak aktif/ditandatangani yang tanggal akhirnya sudah lewat | Kontrak Budi berakhir 15 Desember, belum diputuskan |

Layar Kontrak dan Dashboard SDM kini membaca aturan di atas dari satu kelas.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LifecycleManagement/Services/HrdEmploymentContractService.cs` (`GetContractSummaryAsync`, `GetRenewalSummaryAsync`), `Constants/EmploymentLifecycleValueConstants.cs`, `Enums/ContractDecisionType.cs`, `Enums/ContractDecisionStatus.cs`, `Models/HrdContractDecision.cs`, `WorkforceCore/Models/WfpContractHistory.cs`; pencarian aturan 60/14/7 hari di seluruh `Areas/Corporate/HumanResource`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/ContractRenewalBuckets.cs` (baru) | Kode tahapan, `ReferenceDate()`, predikat `ExpiringWithinWindow`, `Stage(H60/H14/H7)`, `OverduePredicate`, `PendingDecisionPredicate`, dan `Classify` untuk kontrak yang sudah dimuat |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs` | `GetContractSummaryAsync` (`ExpiringSoonCount`) dan `GetRenewalSummaryAsync` memakai `ContractRenewalBuckets`; variabel tanggal lokal dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah — `GET employment-contracts/summary` dan `renewal-summary` mengembalikan bentuk dan arti yang sama |
| Database | Tidak ada |
| Keamanan/Auth | `NOT APPLICABLE` |

**Pembuktian kesetaraan (sebelum → sesudah):**

| Angka | Sebelum | Sesudah |
| --- | --- | --- |
| `ExpiringSoonCount` | `query(!IsDelete).Count(IsCurrent && EndDate ≥ today && EndDate ≤ today+60)` | `query(!IsDelete).Count(ExpiringWithinWindow(today))` = `!IsDelete && IsCurrent && EndDate ≥ today && EndDate ≤ today+60` |
| `ReminderH60Count` | basis `!IsDelete && IsCurrent && EndDate ≥ today`, lalu `EndDate ≤ today+60 && EndDate > today+14` | basis sama, lalu `Stage(H60)` = syarat basis + rentang yang sama |
| `EscalationH14Count` | `EndDate ≤ today+14 && > today+7` | `Stage(H14)` dengan rentang yang sama |
| `NoticeDueH7Count` | `EndDate ≤ today+7` | `Stage(H7)` dengan rentang yang sama |
| `PendingDecisionCount` | `!IsDelete && Extend && Submitted` | `PendingDecisionPredicate()` identik |
| `OverdueCount` | `!IsDelete && EndDate < today && status Active/Signed` | `OverduePredicate(today)` identik |
| Tanggal acuan | `DateTime.UtcNow.Date` | `ContractRenewalBuckets.ReferenceDate()` = `DateTime.UtcNow.Date` |

Filter lokasi (`hospitalSiteId`) tetap diterapkan pada basis kueri seperti sebelumnya.

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah; dua endpoint layar Kontrak tetap.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` penuh Paket B | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Pencarian salinan aturan tahapan | Tidak ada lagi di `HrdEmploymentContractService`. Ditemukan aturan 60 hari tersendiri di `HrdLifecycleWorklistService.BuildWorklistItemsAsync` (hanya status `Signed`, tanpa `IsCurrent`) — arti berbeda, milik worklist siklus hidup, tidak disentuh | `PASS` (dengan temuan) | grep 23 November 2026 |
| Perbandingan respons `summary`/`renewal-summary` sebelum dan sesudah di DB dev (`AT-HRD-PKB-05`; izin pengguna 23 November 2026) | Build `7e4c3774` (worktree sementara, port 5198) dan build `9062bd61` (port 5199) dijalankan bergantian terhadap DB dev yang sama: kedua respons **identik**. Batas bukti: DB dev belum punya kontrak (0 baris), sehingga seluruh angka 0; kesetaraan untuk data berisi bertumpu pada pembuktian predikat di tabel 3.3 | `PASS` | `contract-before.json` vs `contract-after.json` (scratchpad sesi) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `summary` dan `renewal-summary` mengembalikan angka yang sama sebelum dan sesudah refactor; bukti respons sebelum/sesudah di DB dev (`AT-HRD-PKB-05`) | Terpenuhi | Respons identik pada DB dev (batas: 0 kontrak); kesetaraan predikat di tabel 3.3 |
| 2. Tidak ada salinan aturan tahapan di luar `ContractRenewalBuckets` | Terpenuhi untuk layar Kontrak | Temuan worklist siklus hidup dicatat (arti berbeda, butuh keputusan bila ingin disatukan) |
| 3. Kontrak API kontrak kerja tidak berubah | Terpenuhi | DTO dan route tidak disentuh |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Worklist siklus hidup (`HrdLifecycleWorklistService`) menghitung kontrak "segera berakhir" dengan aturan lain (status `Signed`, tanpa `IsCurrent`). Angkanya bisa berbeda dari layar Kontrak dan dashboard; menyatukannya mengubah perilaku worklist sehingga butuh keputusan terpisah |
| Risiko tersisa | Rendah — pemindahan predikat murni; perbandingan runtime baru berisi angka 0 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | Uji ulang perbandingan begitu DB dev punya data kontrak; `BE-HRD-449` |
