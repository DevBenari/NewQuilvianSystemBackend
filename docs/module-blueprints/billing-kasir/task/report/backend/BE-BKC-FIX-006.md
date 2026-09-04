# Laporan Perubahan Backend — `BE-BKC-FIX-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-006` (ad-hoc, di luar roadmap, keputusan bisnis eksplisit pengguna lewat chat langsung) |
| Judul | `CoveragePercent` dan `CoPaymentPercent` kini SALING MELENGKAPI (jumlah selalu 100), bukan dua pengurang independen yang ditumpuk — di KEDUA engine coverage, dan diturunkan server-side di master data |
| Slice | Lanjutan investigasi live invoice Allianz, setelah `BE-BKC-FIX-005`/`FE-BKC-FIX-009` |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan skema/DTO; `CoPaymentPercent` tetap ada di request/response, cuma tidak lagi dipercaya dari client untuk kalkulasi/penyimpanan |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`/`ClinicalManagement`/`MasterData` — sudah terdaftar. Keberlakuan: `TOUCHED LEGACY` |
| Dependency | `BE-BKC-FIX-005` (task ini melanjutkan investigasi yang sama) |
| Klasifikasi | `HIGH` — mengubah rumus inti perhitungan coverage asuransi di KEDUA engine, berdampak finansial langsung pada semua rule yang mengisi `CoPaymentPercent` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `BillingCoverageAdapter.cs`, `InsuranceCoverageService.cs`, `InsuranceCoverageRuleController.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu rebuild |

---

## 1. Masalah

Pengguna mengubah rule "Coverage Allianz Kategori Radiologi" jadi `CoveragePercent=75`,
`CoPaymentPercent=25`, dan mempertanyakan kenapa asuransi ternyata cuma menanggung 166.500 dari
333.000 (persis 50%), bukan 75% seperti yang dimaksud. Ditelusuri: KEDUA engine coverage di sistem
ini menumpuk `CoveragePercent` dan `CoPaymentPercent` sebagai dua pengurang INDEPENDEN dan
BERURUTAN:

```
covered = eligible * CoveragePercent/100        // 333.000 * 75% = 249.750
covered -= eligible * CoPaymentPercent/100       // 249.750 - (333.000 * 25%) = 249.750 - 83.250 = 166.500
```

Dikonfirmasi ke pengguna: kedua field itu seharusnya SALING MELENGKAPI (`CoveragePercent +
CoPaymentPercent = 100` selalu), dengan `CoveragePercent` sebagai SATU-SATUNYA input yang diisi
user, dan `CoPaymentPercent` murni nilai TURUNAN (100 - CoveragePercent), bukan input independen
yang dipakai kalkulasi terpisah. `CoPaymentAmount` (nominal tetap) TIDAK termasuk keputusan ini —
tetap pengurang independen di kedua engine (mis. biaya visit tetap, bukan persentase yang tumpang
tindih dengan `CoveragePercent`).

---

## 2. Perubahan yang dikerjakan

### 2.1 `BillingCoverageAdapter.cs` — `CalculateCoveredAmount()`

Blok `if (rule.CoPaymentPercent.HasValue) covered -= eligible * Math.Clamp(rule.CoPaymentPercent.Value, 0, 100) / 100m;`
dihapus total. `covered = eligible * CoveragePercent/100` sekarang jadi satu-satunya penentu porsi
tertanggung sebelum pengurangan `CoPaymentAmount` (flat, tidak berubah).

### 2.2 `InsuranceCoverageService.cs` — `ResolveTariffInternalAsync`

Variabel `coPaymentPercent` dan `coPaymentFromPercent` dihapus total (dikonfirmasi lewat grep tidak
dipakai di tempat lain pada method yang sama — hanya mengalir ke `totalCoPayment`). `totalCoPayment`
sekarang murni dari `coPaymentAmount` (flat) saja.

### 2.3 `InsuranceCoverageRuleController.cs` — Create/Update + validasi

`CoPaymentPercent` pada entity CREATE (baris ~447) dan UPDATE (baris ~543) tidak lagi diambil
langsung dari `request.CoPaymentPercent` — SELALU diturunkan server-side:

```csharp
CoPaymentPercent = Math.Clamp(100m - Math.Clamp(request.CoveragePercent, 0m, 100m), 0m, 100m)
```

Ini defense-in-depth: form frontend akan dibuat read-only untuk field ini (task terpisah), tapi
backend jadi authoritative source supaya panggilan API langsung tidak bisa menyimpan pasangan yang
tidak konsisten. Validasi range `CoPaymentPercent` pada `ValidateRequestAsync` (baris ~974-975)
dihapus — sudah tidak relevan karena field itu tidak lagi dipercaya dari client dan selalu valid
by construction dari hasil derivasi. DTO request (`CreateInsuranceCoverageRuleRequest`/
`UpdateInsuranceCoverageRuleRequest`) TIDAK diubah — field `CoPaymentPercent` tetap ada di situ
(diterima tapi diabaikan), supaya tidak breaking existing request shape sebelum frontend selesai
diperbarui.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Analisis manual: Radiology gross 333.000, CoveragePercent=75, CoPaymentAmount=0 | `covered = 333.000 * 75% = 249.750` (tidak dipotong lagi) — sesuai maksud pengguna (75% tertanggung, bukan 50%) | `PASS` (analisis) |
| Grep `coPaymentPercent` di `InsuranceCoverageService.cs` setelah edit | Hanya tersisa di komentar penjelasan, tidak ada pemakaian variabel lagi | `PASS` |
| Analisis backward-compat: rule dengan `CoPaymentPercent=0` (mis. "Coverage Administration", "Coverage Obat Rajal") | Tidak berubah — sebelumnya `CoPaymentPercent=0` juga sudah tidak mengurangi apa pun (0% dari eligible = 0), jadi perilaku SAMA untuk rule yang sudah konsisten (persentase co-payment nol) | `PASS` (analisis) |
| Analisis dampak `CoPaymentAmount` (nominal tetap) | TIDAK disentuh di kedua engine — tetap pengurang independen, tidak terpengaruh perubahan ini | `PASS` (analisis) |

**`AUTOMATED TEST: BLOCKED`** — build/test tidak dijalankan (instruksi eksplisit pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Data lama tidak wajib migrasi | `CoPaymentPercent` yang sudah tersimpan di database TIDAK LAGI dipakai untuk kalkulasi coverage di kedua engine (dihapus dari rumus) — jadi tidak ada risiko finansial dari nilai lama yang salah/tidak konsisten. Nilai tampilan (`CoPaymentPercent` di daftar/detail rule) akan otomatis terkoreksi begitu pengguna membuka dan menyimpan ulang rule itu lewat form (create/update yang sudah diperbaiki menghitung ulang dari `CoveragePercent`) — TIDAK perlu migration/script database terpisah, dikonfirmasi ke pengguna |
| Dependency frontend | Form master data Insurance Coverage Rule (create/edit) MASIH mengirim `CoPaymentPercent` sebagai field independen yang bisa diisi manual — backend SUDAH mengabaikan nilai itu (aman), tapi UI form belum mencerminkan bahwa field itu murni tampilan/turunan. Task frontend terpisah dibutuhkan supaya field itu jadi read-only/otomatis terhitung live dari `CoveragePercent`, mencegah kebingungan pengguna (mengisi manual tapi diam-diam diabaikan) |
| Belum diverifikasi hidup | Berdasar penelusuran kode, bukan hasil kalkulasi nyata pasca-rebuild |
| Perubahan sampingan | `NONE` |
| Status Git | Modified: `BillingCoverageAdapter.cs`, `InsuranceCoverageService.cs`, `InsuranceCoverageRuleController.cs`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, verifikasi ulang item Radiology invoice IKBAL YULIYANTO → tertanggung harus jadi Rp249.750 (75% dari 333.000, CoPaymentAmount=0), bukan Rp166.500. Lanjutkan task frontend untuk membuat field "Persentase Co-Payment" read-only/otomatis di form master data Insurance Coverage Rule |

