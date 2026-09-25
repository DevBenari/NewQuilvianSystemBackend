# Laporan Perubahan Backend — `BE-BUI-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BUI-001` |
| Judul | Perbaikan Logika `suggestedBillingStatus` |
| Slice | Gelombang `MVP-30` — Revisi UI Billing: Perbaikan Logika Backend (`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-30`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BUI-001` |
| Trace | `BUI-DEC-007`, `BUI-DEC-014` (keputusan bisnis, approved 24 September 2026); `BUI-DES-001` (keputusan arsitektur, **approved pemilik modul 25 September 2026** — gerbang wajib sebelum task ini boleh dikerjakan, terpisah dari approval bisnis) |
| Contract version | `BIL-API-1.5` (draft) — bentuk `InvoiceEditContextResponse` **tidak berubah**, hanya nilai `SuggestedBillingStatus` dan turunannya pada kasus coverage sebagian |
| Dependency | Tidak ada — task independen, tidak menyentuh berkas yang sama dengan `BE-BUI-002` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa ≤8 (skor 0: satu service, satu roadmap card); berkas diubah 1 (skor 0); logika bisnis sederhana — satu kondisi boolean dibalik dari OR ke AND (skor 0, bukan logika baru, murni pembalikan aturan yang sudah didesain); kontrak API tidak berubah bentuknya (skor 0); database tidak ada dampak (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow `NOT APPLICABLE` untuk backend murni (skor 0). Total skor 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — build lulus, verifikasi manual belum.** Source selesai persis sesuai scope roadmap. `dotnet build` dijalankan pengguna sendiri dan **lulus**, dikonfirmasi 25 September 2026. Empat kasus acceptance criteria (nol/sebagian/seluruh item tercover; payer non-`INSURANCE`) baru terverifikasi lewat analisis statis (pembacaan kode) — **belum** diuji manual nyata terhadap `GET /{id}/edit-context` dengan data invoice sungguhan. Build sukses memenuhi satu baris Definition of Done, bukan seluruhnya — lihat §5 |

---

## 1. Masalah yang diperbaiki

Pada layar edit payer tagihan (`GET /{id}/edit-context`), backend menyarankan `suggestedBillingStatus = "INSURANCE"` kepada kasir **begitu ada SATU SAJA item tagihan yang tercover asuransi** — walau item-item lain pada tagihan yang sama sama sekali tidak tercover. Ini ditemukan sebagai konflik pada audit `/trace-existing-capabilities` (`01-existing-capability-map.md` bagian 23.4): kasir bisa disarankan status "Asuransi" untuk tagihan yang sebagian besar isinya sebenarnya harus dibayar tunai oleh pasien, berisiko salah kategorikan cara bayar pada tagihan campuran (misalnya pasien BPJS yang mayoritas itemnya adalah obat non-formularium yang tidak ditanggung).

Keputusan bisnis `BUI-DEC-007`/`BUI-DEC-014` mengubah aturan ini menjadi: **seluruh item aktif pada tagihan harus tercover asuransi** baru backend menyarankan status "Asuransi"; bila ada satu saja yang tidak tercover, sarannya kembali ke "Tunai".

---

## 2. Proses bisnis

1. Kasir membuka layar edit payer sebuah tagihan (`InvoiceEditContextResponse`).
2. Untuk pasien dengan `currentPayer.PaymentType == "INSURANCE"`, backend memeriksa status coverage setiap item aktif pada tagihan lewat `calculation.Breakdown.Items` (hasil kalkulasi yang sudah ada, tidak dihitung ulang pada task ini).
3. **Aturan baru**: `suggestedBillingStatus` bernilai `"INSURANCE"` hanya jika **seluruh** item aktif tercover (`ItemPrimaryAmount > 0`). Bila ada satu item saja yang tidak tercover, `suggestedBillingStatus` menjadi `"CASH"`.
4. `effectivePaymentType` dan `PaymentMethodRow.IsSelected` (tombol Tunai/Asuransi/Penjamin Perusahaan pada layar) mewarisi nilai ini secara otomatis tanpa perubahan kode terpisah, karena keduanya membaca variabel `suggestedBillingStatus` yang sama.
5. Pasien dengan payer bukan `INSURANCE` (misalnya `CASH` atau `COMPANY_GUARANTOR`) **tidak terpengaruh** — `suggestedBillingStatus` untuk kasus itu tetap langsung mengikuti `currentPayer.PaymentType` apa adanya, jalur kode ini tidak diubah.
6. Item yang sudah memiliki assignment payer manual sebelumnya (`existingAssignments`) dilewati dari perhitungan agregat ini — perilaku ini **sudah ada sejak sebelum task ini** (item semacam itu juga tidak ikut menyumbang ke aturan lama) dan sengaja dipertahankan apa adanya sesuai scope roadmap ("nol perubahan pada `itemAssignments` per baris").

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` (seluruh method `GetEditContextAsync`, baris ~96-282)
- `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-30`, kartu `BE-BUI-001`
- `docs/module-blueprints/billing-kasir/01-existing-capability-map.md` bagian 23.4 (asal temuan konflik)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` | Tiga titik diubah, seluruhnya di dalam `GetEditContextAsync`: (1) baris 102, deklarasi `bool anyItemCoveredByInsurance = false;` → `bool allItemsCoveredByInsurance = true;`; (2) baris 125-128, `if (isCovered) { anyItemCoveredByInsurance = true; }` → `if (!isCovered) { allItemsCoveredByInsurance = false; }`; (3) baris 145-147, ternary `anyItemCoveredByInsurance ? "INSURANCE" : "CASH"` → `allItemsCoveredByInsurance ? "INSURANCE" : "CASH"`, komentar di atasnya disesuaikan. Tidak ada baris lain yang disentuh — `itemAssignments`, `effectivePaymentType`, `paymentMethodRow`, dan seluruh DTO tidak berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk `InvoiceEditContextResponse` **tidak berubah** — field, tipe, dan struktur JSON identik. Hanya **nilai** `SuggestedBillingStatus` (dan turunannya `PaymentMethodRow[].IsSelected`) yang berbeda pada kasus tagihan dengan coverage sebagian (sebagian item tercover, sebagian tidak) |
| Database | `NOT APPLICABLE` — nol migration, nol perubahan schema |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi/endpoint |

---

## 4. Dokumentasi endpoint

Endpoint `GET /{id}/edit-context` (grup `Billing Payer Edit` bila ada tag Swagger tersendiri — controller tidak diperiksa ulang pada task ini karena bentuk kontraknya tidak berubah) **tidak berubah bentuk maupun rute**; hanya nilai field `SuggestedBillingStatus` pada response yang berbeda untuk kasus coverage sebagian.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Lulus | `PASS` | Dijalankan pengguna sendiri, dikonfirmasi 25 September 2026 |
| Review diff — perubahan hanya 3 baris pada satu berkas, sesuai scope roadmap persis | Sesuai | `PASS` | §3.2; `git diff` atas berkas yang sama menunjukkan hanya 3 pasang baris berubah |
| Pembacaan manual: `effectivePaymentType`/`PaymentMethodRow.IsSelected` (baris 226-253) tidak direferensikan ke nama variabel lama | Tidak ada referensi tersisa ke `anyItemCoveredByInsurance` di seluruh berkas | `PASS` | `grep -rn "anyItemCoveredByInsurance"` hasil kosong |
| Kasus 5 item, 2 tercover 3 tidak → `suggestedBillingStatus == "CASH"` (kasus penentu) | Ditelusuri lewat pembacaan logika: `allItemsCoveredByInsurance` di-set `false` pada iterasi item pertama yang `isCovered == false` | `PASS` (analisis statis) | §3.2 — belum diuji terhadap data nyata |
| Kasus 5 item seluruhnya tercover → `"INSURANCE"` | Ditelusuri: `allItemsCoveredByInsurance` tidak pernah di-set `false`, tetap `true` bawaan | `PASS` (analisis statis) | Sda |
| Kasus 5 item nol tercover → `"CASH"` | Ditelusuri: `allItemsCoveredByInsurance` di-set `false` pada iterasi pertama | `PASS` (analisis statis) | Sda |
| Kasus payer bukan `INSURANCE` tidak terpengaruh | Ditelusuri: ternary baris 145 tetap mengembalikan `currentPayer.PaymentType` langsung untuk payer non-`INSURANCE`, jalur `allItemsCoveredByInsurance` tidak pernah dibaca pada cabang ini | `PASS` (analisis statis) | Sda |

Uji manual: `REQUIRED`, belum dijalankan — `NOT RUN`. Empat kasus di atas baru diverifikasi lewat pembacaan logika kode (analisis statis), **belum** dijalankan nyata lewat `GET /{id}/edit-context` terhadap tagihan sungguhan dengan kombinasi item tercover/tidak tercover.

**Tidak dijalankan:** `dotnet build`; uji unit `BillingPayerEditServiceTests` (disebut roadmap sebagai bukti verifikasi) — **tidak ada project test otomatis pada repository ini** (`rules/backend/TEST_POLICY.md`), sehingga bukti verifikasi diganti analisis statis + review diff sesuai kebijakan; regresi manual `GET /{id}/edit-context` terhadap data nyata.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`); tidak diminta secara eksplisit pada task ini. Nama `BillingPayerEditServiceTests` pada roadmap adalah rujukan historis/kontrak acceptance, bukan wewenang membuat project test baru.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kasus 5 item, 2 tercover 3 tidak → `"CASH"` (kasus penentu) | Terpenuhi lewat analisis statis, **belum diuji nyata** | §5 |
| Kasus 5 item seluruhnya tercover → `"INSURANCE"` | Terpenuhi lewat analisis statis, **belum diuji nyata** | §5 |
| Kasus 5 item nol tercover → `"CASH"` | Terpenuhi lewat analisis statis, **belum diuji nyata** | §5 |
| Payer bukan `INSURANCE` tidak tersentuh | Terpenuhi lewat analisis statis, **belum diuji nyata** | §5 |
| `PaymentMethodRow.IsSelected` ikut berubah benar (mewarisi otomatis) | Terpenuhi lewat pembacaan kode — tidak ada jalur kode terpisah yang perlu diubah | §2 butir 4, §3.2 |
| Nol perubahan bentuk DTO | Terpenuhi | §3.3 |
| QBE preflight PASS | Terpenuhi — `TOUCHED LEGACY`, `Area HealthServices/BillingManagement/Billing`, prefix `Bil` sudah `ACTIVE` pada registry, nol entity baru | Preflight ini |
| Build backend terverifikasi hijau | Terpenuhi | `dotnet build` dijalankan pengguna sendiri dan lulus, dikonfirmasi 25 September 2026 |
| Verifikasi manual/regresi nyata terhadap `GET /{id}/edit-context` | **Belum terpenuhi** | Menunggu pengguna |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` sudah lulus (dikonfirmasi pengguna 25 September 2026), tapi verifikasi manual runtime terhadap keempat kasus acceptance criteria belum dijalankan — bukti kebenaran logika pada laporan ini masih berasal dari pembacaan kode (analisis statis), bukan eksekusi nyata |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Sesuai catatan risiko roadmap: `effectivePaymentType` dan `PaymentMethodRow.IsSelected` bergantung penuh pada `suggestedBillingStatus` — bila ada jalur kode lain (di luar berkas ini) yang ikut membaca nama variabel lama `anyItemCoveredByInsurance` secara reflektif atau lewat mekanisme lain di luar pencarian teks biasa, risiko itu tidak akan terdeteksi oleh `grep`. Item dengan assignment payer manual (`existingAssignments`) tetap dikecualikan dari perhitungan agregat, warisan perilaku lama — bila ini ternyata bukan yang dimaksud desain arsitektur `BUI-DES-001`, perlu klarifikasi tersendiri sebelum dianggap final |
| Perubahan sampingan | `NONE` |
| Interupsi | Task ini awalnya diminta dengan Task ID keliru (`BUI-DES-001`, yang ternyata sudah dipakai sebagai gerbang arsitektur — bukan task implementasi) pada sesi yang sama; setelah klarifikasi pengguna, dikerjakan dengan Task ID yang benar (`BE-BUI-001`), gerbang arsitekturnya sendiri (`BUI-DES-001`) dicatat sebagai disetujui pada roadmap dan traceability |
| Status Git | `M Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` |
| Langkah berikutnya | Verifikasi manual `GET /{id}/edit-context` untuk keempat kasus di §5/§6 terhadap tagihan nyata (idealnya satu tagihan dengan campuran item tercover/tidak) — satu-satunya butir DoD yang masih terbuka |
